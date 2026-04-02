using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

#if !UNITY_WEBGL || UNITY_EDITOR
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
#endif

namespace IntelliVerseX.AI
{
    /// <summary>
    /// WebSocket client for realtime AI voice streaming.
    /// Handles connection lifecycle, reconnection with exponential back-off,
    /// heartbeat ping/pong, and chunked PCM16 audio transport.
    /// </summary>
    public sealed class IVXAIWebSocketClient : MonoBehaviour
    {
        #region Constants

        private const int RECONNECT_MAX_ATTEMPTS = 5;
        private const int RECONNECT_BASE_DELAY_MS = 1000;
        private const int HEARTBEAT_INTERVAL_MS = 30000;
        private const int CONNECTION_TIMEOUT_MS = 15000;
        private const int RECEIVE_BUFFER_SIZE = 16384;
        private const int AUDIO_CHUNK_SIZE = 4096;

        #endregion

        #region Events

        /// <summary>Fired when the WebSocket connection is established.</summary>
        public event Action OnConnected;

        /// <summary>Fired when the connection is closed (includes reason string).</summary>
        public event Action<string> OnDisconnected;

        /// <summary>Fired when the connection drops unexpectedly.</summary>
        public event Action OnConnectionLost;

        /// <summary>Fired on each reconnection attempt (current attempt, max attempts).</summary>
        public event Action<int, int> OnReconnecting;

        /// <summary>Fired when a reconnection attempt succeeds.</summary>
        public event Action OnReconnected;

        /// <summary>Fired when all reconnection attempts are exhausted.</summary>
        public event Action<string> OnReconnectionFailed;

        /// <summary>Fired when a JSON text message is received from the server.</summary>
        public event Action<string> OnMessageReceived;

        /// <summary>Fired when a binary message (e.g. PCM audio) is received.</summary>
        public event Action<byte[]> OnBinaryReceived;

        /// <summary>Fired when an error occurs during connection or communication.</summary>
        public event Action<string> OnError;

        #endregion

        #region Properties

        /// <summary>Current state of the WebSocket connection.</summary>
        public IVXAIConnectionState ConnectionState { get; private set; } = IVXAIConnectionState.Disconnected;

        /// <summary>Shorthand for <c>ConnectionState == Connected</c>.</summary>
        public bool IsConnected => ConnectionState == IVXAIConnectionState.Connected;

        /// <summary>Session ID associated with this connection.</summary>
        public string SessionId { get; private set; }

        /// <summary>Round-trip latency in milliseconds measured by heartbeat ping/pong.</summary>
        public int LatencyMs { get; private set; }

        #endregion

        #region Private Fields

#if !UNITY_WEBGL || UNITY_EDITOR
        private ClientWebSocket _webSocket;
        private CancellationTokenSource _cts;
#endif

        private string _serverUrl;
        private string _authToken;
        private Dictionary<string, string> _headers;

        private int _reconnectAttempts;
        private bool _shouldReconnect;
        private bool _isReconnecting;

        private Coroutine _heartbeatCoroutine;
        private Coroutine _receiveCoroutine;

        private DateTime _lastPingTime;
        private DateTime _lastPongTime;

        private const int MAX_QUEUE_SIZE = 256;

        private readonly Queue<string> _messageQueue = new Queue<string>();
        private readonly Queue<byte[]> _binaryQueue = new Queue<byte[]>();
        private readonly object _queueLock = new object();

        private bool _debugLogging;

        #endregion

        #region Unity Lifecycle

        private void Update()
        {
            ProcessQueues();
        }

        private void OnDestroy()
        {
            Disconnect();
        }

        private void OnApplicationPause(bool pauseStatus)
        {
            if (pauseStatus && IsConnected)
            {
                LogVerbose("App pausing — connection may be interrupted");
            }
            else if (!pauseStatus && ConnectionState == IVXAIConnectionState.Disconnected && _shouldReconnect)
            {
                LogVerbose("App resuming — checking connection");
                StartCoroutine(ResumeCheck());
            }
        }

        #endregion

        #region Public Methods

        /// <summary>Initialise the client with connection parameters.</summary>
        public void Initialize(string serverUrl, string authToken = null, string sessionId = null, Dictionary<string, string> headers = null)
        {
            _serverUrl = serverUrl;
            _authToken = authToken;
            SessionId = sessionId;
            _headers = headers ?? new Dictionary<string, string>();
        }

        /// <summary>Enable or disable verbose debug logging.</summary>
        /// <param name="enabled">True to enable debug logs.</param>
        public void SetDebugLogging(bool enabled) => _debugLogging = enabled;

        /// <summary>Open the WebSocket connection.</summary>
        public void Connect()
        {
            if (ConnectionState == IVXAIConnectionState.Connecting || IsConnected)
                return;

            if (string.IsNullOrEmpty(_serverUrl))
            {
                OnError?.Invoke("Server URL not set. Call Initialize first.");
                return;
            }

            _shouldReconnect = true;
            _reconnectAttempts = 0;

#if UNITY_WEBGL && !UNITY_EDITOR
            StartCoroutine(ConnectWebGL());
#else
            StartCoroutine(ConnectNative());
#endif
        }

        /// <summary>Close the WebSocket connection.</summary>
        public void Disconnect(bool allowReconnect = false)
        {
            _shouldReconnect = allowReconnect;
            StopHeartbeat();
            StopReceiving();

#if !UNITY_WEBGL || UNITY_EDITOR
            if (_webSocket != null)
            {
                try
                {
                    _cts?.Cancel();
                    if (_webSocket.State == WebSocketState.Open)
                        _ = _webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "disconnect", CancellationToken.None);
                    _webSocket.Dispose();
                    _webSocket = null;
                }
                catch (Exception ex)
                {
                    LogVerbose($"Disconnect error: {ex.Message}");
                }
            }
#endif
            ConnectionState = IVXAIConnectionState.Disconnected;
            OnDisconnected?.Invoke("Client disconnected");
        }

        /// <summary>Send a JSON text message.</summary>
        public void Send(string message)
        {
            if (!IsConnected) return;
#if UNITY_WEBGL && !UNITY_EDITOR
            SendWebGL(message);
#else
            _ = SendAsync(message);
#endif
        }

        /// <summary>Send raw binary data.</summary>
        public void SendBinary(byte[] data)
        {
            if (!IsConnected) return;
#if UNITY_WEBGL && !UNITY_EDITOR
            SendBinaryWebGL(data);
#else
            _ = SendBinaryAsync(data);
#endif
        }

        /// <summary>Send PCM16 audio data in chunked base64 JSON packets.</summary>
        public void SendAudioChunked(byte[] pcmData)
        {
            if (!IsConnected || pcmData == null || pcmData.Length == 0) return;

            int offset = 0;
            while (offset < pcmData.Length)
            {
                int chunkSize = Math.Min(AUDIO_CHUNK_SIZE, pcmData.Length - offset);
                byte[] chunk = new byte[chunkSize];
                Array.Copy(pcmData, offset, chunk, 0, chunkSize);

                string b64 = Convert.ToBase64String(chunk);
                Send($"{{\"type\":\"input_audio_buffer.append\",\"audio\":\"{b64}\"}}");
                offset += chunkSize;
            }
        }

        /// <summary>Force a fresh reconnection attempt.</summary>
        public void ForceReconnect()
        {
            if (_isReconnecting) return;
            _shouldReconnect = true;
            _reconnectAttempts = 0;
            Disconnect(true);
            Connect();
        }

        #endregion

        #region Native WebSocket

#if !UNITY_WEBGL || UNITY_EDITOR

        private IEnumerator ConnectNative()
        {
            ConnectionState = IVXAIConnectionState.Connecting;
            LogVerbose($"Connecting to {_serverUrl}");

            _cts?.Cancel();
            _cts = new CancellationTokenSource();
            _webSocket = new ClientWebSocket();

            if (!string.IsNullOrEmpty(_authToken))
                _webSocket.Options.SetRequestHeader("Authorization", $"Bearer {_authToken}");
            foreach (var h in _headers)
                _webSocket.Options.SetRequestHeader(h.Key, h.Value);

            var task = ConnectWithTimeout();
            while (!task.IsCompleted) yield return null;

            if (task.Exception != null || _webSocket.State != WebSocketState.Open)
            {
                string err = task.Exception?.InnerException?.Message ?? "Connection failed";
                Debug.LogWarning($"[{nameof(IVXAIWebSocketClient)}] {err}");
                ConnectionState = IVXAIConnectionState.Disconnected;

                if (_shouldReconnect)
                    HandleConnectionLost();
                else
                    OnError?.Invoke(err);
                yield break;
            }

            ConnectionState = IVXAIConnectionState.Connected;
            bool wasReconnect = _reconnectAttempts > 0;
            _reconnectAttempts = 0;
            _isReconnecting = false;

            StartReceiving();
            StartHeartbeat();

            if (wasReconnect)
                OnReconnected?.Invoke();
            else
                OnConnected?.Invoke();
        }

        private async Task ConnectWithTimeout()
        {
            using var timeout = new CancellationTokenSource(CONNECTION_TIMEOUT_MS);
            using var linked = CancellationTokenSource.CreateLinkedTokenSource(_cts.Token, timeout.Token);
            try
            {
                await _webSocket.ConnectAsync(new Uri(_serverUrl), linked.Token);
            }
            catch (OperationCanceledException) when (timeout.IsCancellationRequested)
            {
                throw new TimeoutException("Connection timeout");
            }
        }

        private async Task SendAsync(string message)
        {
            if (_webSocket?.State != WebSocketState.Open) return;
            try
            {
                byte[] buf = Encoding.UTF8.GetBytes(message);
                await _webSocket.SendAsync(new ArraySegment<byte>(buf), WebSocketMessageType.Text, true, _cts.Token);
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[{nameof(IVXAIWebSocketClient)}] Send failed: {ex.Message}");
                HandleConnectionLost();
            }
        }

        private async Task SendBinaryAsync(byte[] data)
        {
            if (_webSocket?.State != WebSocketState.Open) return;
            try
            {
                await _webSocket.SendAsync(new ArraySegment<byte>(data), WebSocketMessageType.Binary, true, _cts.Token);
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[{nameof(IVXAIWebSocketClient)}] SendBinary failed: {ex.Message}");
                HandleConnectionLost();
            }
        }

        private void StartReceiving()
        {
            if (_receiveCoroutine != null) StopCoroutine(_receiveCoroutine);
            _receiveCoroutine = StartCoroutine(ReceiveLoop());
        }

        private void StopReceiving()
        {
            if (_receiveCoroutine != null) { StopCoroutine(_receiveCoroutine); _receiveCoroutine = null; }
        }

        private IEnumerator ReceiveLoop()
        {
            var buffer = new byte[RECEIVE_BUFFER_SIZE];
            var msgBuf = new List<byte>();

            while (_webSocket?.State == WebSocketState.Open)
            {
                var task = ReceiveMessage(buffer, msgBuf);
                while (!task.IsCompleted) yield return null;

                if (task.Exception != null)
                {
                    Debug.LogWarning($"[{nameof(IVXAIWebSocketClient)}] Receive error: {task.Exception.InnerException?.Message}");
                    HandleConnectionLost();
                    yield break;
                }
                yield return null;
            }
        }

        private async Task ReceiveMessage(byte[] buffer, List<byte> msgBuf)
        {
            try
            {
                var result = await _webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), _cts.Token);
                if (result.MessageType == WebSocketMessageType.Close)
                {
                    HandleConnectionLost();
                    return;
                }

                msgBuf.AddRange(new ArraySegment<byte>(buffer, 0, result.Count));
                if (result.EndOfMessage)
                {
                    if (result.MessageType == WebSocketMessageType.Text)
                    {
                        lock (_queueLock)
                        {
                            while (_messageQueue.Count >= MAX_QUEUE_SIZE) _messageQueue.Dequeue();
                            _messageQueue.Enqueue(Encoding.UTF8.GetString(msgBuf.ToArray()));
                        }
                    }
                    else if (result.MessageType == WebSocketMessageType.Binary)
                    {
                        lock (_queueLock)
                        {
                            while (_binaryQueue.Count >= MAX_QUEUE_SIZE) _binaryQueue.Dequeue();
                            _binaryQueue.Enqueue(msgBuf.ToArray());
                        }
                    }
                    msgBuf.Clear();
                }
            }
            catch (OperationCanceledException) { }
        }

#endif

        #endregion

        #region WebGL WebSocket

#if UNITY_WEBGL && !UNITY_EDITOR
        [System.Runtime.InteropServices.DllImport("__Internal")]
        private static extern void IVXAI_WebSocket_Connect(string url, string objectName);
        [System.Runtime.InteropServices.DllImport("__Internal")]
        private static extern void IVXAI_WebSocket_Send(string message);
        [System.Runtime.InteropServices.DllImport("__Internal")]
        private static extern void IVXAI_WebSocket_SendBinary(byte[] data, int length);
        [System.Runtime.InteropServices.DllImport("__Internal")]
        private static extern void IVXAI_WebSocket_Close();

        private IEnumerator ConnectWebGL()
        {
            ConnectionState = IVXAIConnectionState.Connecting;
            IVXAI_WebSocket_Connect(_serverUrl, gameObject.name);

            float elapsed = 0f;
            float timeout = CONNECTION_TIMEOUT_MS / 1000f;
            while (ConnectionState == IVXAIConnectionState.Connecting && elapsed < timeout)
            {
                elapsed += Time.deltaTime;
                yield return null;
            }
            if (!IsConnected) HandleConnectionLost();
        }

        private void SendWebGL(string msg) => IVXAI_WebSocket_Send(msg);
        private void SendBinaryWebGL(byte[] data) => IVXAI_WebSocket_SendBinary(data, data.Length);

        public void OnWebGLConnected()
        {
            ConnectionState = IVXAIConnectionState.Connected;
            _reconnectAttempts = 0;
            StartHeartbeat();
            OnConnected?.Invoke();
        }

        public void OnWebGLMessage(string message)
        {
            lock (_queueLock) _messageQueue.Enqueue(message);
        }

        public void OnWebGLError(string error) { OnError?.Invoke(error); HandleConnectionLost(); }

        public void OnWebGLClosed()
        {
            ConnectionState = IVXAIConnectionState.Disconnected;
            if (_shouldReconnect) HandleConnectionLost();
            else OnDisconnected?.Invoke("Connection closed");
        }
#endif

        #endregion

        #region Reconnection

        private void HandleConnectionLost()
        {
            if (_isReconnecting || !_shouldReconnect) return;
            _isReconnecting = true;
            ConnectionState = IVXAIConnectionState.Reconnecting;
            OnConnectionLost?.Invoke();
            StartCoroutine(ReconnectWithBackoff());
        }

        private IEnumerator ReconnectWithBackoff()
        {
            while (_reconnectAttempts < RECONNECT_MAX_ATTEMPTS && _shouldReconnect)
            {
                _reconnectAttempts++;
                int delay = RECONNECT_BASE_DELAY_MS * (int)Math.Pow(2, _reconnectAttempts - 1);
                OnReconnecting?.Invoke(_reconnectAttempts, RECONNECT_MAX_ATTEMPTS);

                yield return new WaitForSeconds(delay / 1000f);
                if (!_shouldReconnect) yield break;

#if UNITY_WEBGL && !UNITY_EDITOR
                yield return StartCoroutine(ConnectWebGL());
#else
                yield return StartCoroutine(ConnectNative());
#endif
                if (IsConnected)
                {
                    _isReconnecting = false;
                    yield break;
                }
            }

            _isReconnecting = false;
            ConnectionState = IVXAIConnectionState.Failed;
            OnReconnectionFailed?.Invoke($"Reconnection failed after {RECONNECT_MAX_ATTEMPTS} attempts");
        }

        private IEnumerator ResumeCheck()
        {
            yield return new WaitForSeconds(1f);
            if (ConnectionState == IVXAIConnectionState.Disconnected && _shouldReconnect) Connect();
        }

        #endregion

        #region Heartbeat

        private void StartHeartbeat()
        {
            StopHeartbeat();
            _lastPongTime = DateTime.UtcNow;
            _heartbeatCoroutine = StartCoroutine(HeartbeatLoop());
        }

        private void StopHeartbeat()
        {
            if (_heartbeatCoroutine != null) { StopCoroutine(_heartbeatCoroutine); _heartbeatCoroutine = null; }
        }

        private IEnumerator HeartbeatLoop()
        {
            while (IsConnected)
            {
                yield return new WaitForSeconds(HEARTBEAT_INTERVAL_MS / 1000f);
                if (!IsConnected) yield break;

                if ((DateTime.UtcNow - _lastPongTime).TotalMilliseconds > HEARTBEAT_INTERVAL_MS * 2)
                {
                    Debug.LogWarning($"[{nameof(IVXAIWebSocketClient)}] Heartbeat timeout");
                    HandleConnectionLost();
                    yield break;
                }

                _lastPingTime = DateTime.UtcNow;
                Send("{\"type\":\"ping\"}");
            }
        }

        #endregion

        #region Queue Processing

        private void ProcessQueues()
        {
            while (true)
            {
                string msg = null;
                lock (_queueLock) { if (_messageQueue.Count > 0) msg = _messageQueue.Dequeue(); }
                if (msg == null) break;
                if (msg.Contains("\"type\":\"pong\""))
                {
                    _lastPongTime = DateTime.UtcNow;
                    LatencyMs = (int)(DateTime.UtcNow - _lastPingTime).TotalMilliseconds;
                    continue;
                }
                OnMessageReceived?.Invoke(msg);
            }

            while (true)
            {
                byte[] data = null;
                lock (_queueLock) { if (_binaryQueue.Count > 0) data = _binaryQueue.Dequeue(); }
                if (data == null) break;
                OnBinaryReceived?.Invoke(data);
            }
        }

        #endregion

        #region Logging

        private void LogVerbose(string msg)
        {
            if (_debugLogging) Debug.Log($"[{nameof(IVXAIWebSocketClient)}] {msg}");
        }

        #endregion
    }
}
