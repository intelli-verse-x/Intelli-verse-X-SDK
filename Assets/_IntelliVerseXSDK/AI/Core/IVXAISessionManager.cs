using System;
using System.Collections;
using Newtonsoft.Json;
using UnityEngine;

namespace IntelliVerseX.AI
{
    /// <summary>
    /// Central manager for AI sessions (voice personas and host commentary).
    /// Singleton MonoBehaviour — create once and it persists across scenes.
    /// </summary>
    public sealed class IVXAISessionManager : MonoBehaviour
    {
        #region Singleton

        private static IVXAISessionManager _instance;

        /// <summary>Singleton instance (lazy-found if not yet assigned).</summary>
        public static IVXAISessionManager Instance
        {
            get
            {
                if (_instance == null)
                    _instance = FindFirstObjectByType<IVXAISessionManager>();
                return _instance;
            }
        }

        #endregion

        #region Serialized Fields

        [Header("Configuration")]
        [SerializeField] private IVXAIConfig _config;

        [Header("Audio")]
        [SerializeField] private AudioSource _audioSource;

        #endregion

        #region Events — Voice Session

        /// <summary>Fired when a voice session is successfully created.</summary>
        public event Action<IVXAICreateVoiceSessionResponse> OnSessionStarted;

        /// <summary>Fired when a voice session ends (with analytics).</summary>
        public event Action<IVXAISessionAnalytics> OnSessionEnded;

        /// <summary>Partial caption text streaming in.</summary>
        public event Action<string> OnCaptionReceived;

        /// <summary>Full caption text is complete.</summary>
        public event Action<string> OnCaptionComplete;

        /// <summary>Base64 audio chunk received from AI.</summary>
        public event Action<string> OnAudioReceived;

        /// <summary>AI finished its turn (done speaking).</summary>
        public event Action OnTurnComplete;

        /// <summary>Social proof data received.</summary>
        public event Action<IVXAISocialProofData> OnSocialProofReceived;

        /// <summary>Upsell prompt received.</summary>
        public event Action<string> OnUpsellPrompt;

        /// <summary>Scarcity/urgency message received.</summary>
        public event Action<string> OnScarcityMessage;

        /// <summary>Session time is running low.</summary>
        public event Action OnSessionTimeWarning;

        /// <summary>Error during session.</summary>
        public event Action<string> OnError;

        /// <summary>Entitlement check determined that payment is required.</summary>
        public event Action<IVXAIEntitlementResponse> OnEntitlementRequired;

        /// <summary>Fired when the backend reports user speech start (VAD).</summary>
        public event Action OnSpeechDetected;

        /// <summary>Fired when the backend reports user speech end.</summary>
        public event Action OnSpeechStopped;

        #endregion

        #region Events — Host Session

        /// <summary>Host session created.</summary>
        public event Action<IVXAICreateHostSessionResponse> OnHostSessionStarted;

        /// <summary>Host message received (text, action, audio).</summary>
        public event Action<IVXAIMessage> OnHostMessageReceived;

        /// <summary>Host session ended.</summary>
        public event Action OnHostSessionEnded;

        #endregion

        #region Properties

        /// <summary>The active AI configuration asset.</summary>
        public IVXAIConfig Config => _config;

        /// <summary>Whether <see cref="Initialize"/> has been called successfully.</summary>
        public bool IsInitialized { get; private set; }

        /// <summary>The authenticated user's identifier.</summary>
        public string UserId { get; private set; }

        /// <summary>The authenticated user's display name.</summary>
        public string UserName { get; private set; }

        /// <summary>ISO 639-1 language code used for AI sessions.</summary>
        public string CurrentLanguage { get; private set; }

        /// <summary>Backend session ID for the active voice session, or null.</summary>
        public string CurrentVoiceSessionId { get; private set; }

        /// <summary>Persona ID of the active voice session.</summary>
        public string CurrentVoicePersona { get; private set; }

        /// <summary>True when a voice session is currently in progress.</summary>
        public bool IsVoiceSessionActive => !string.IsNullOrEmpty(CurrentVoiceSessionId);

        /// <summary>Whether the active voice session is a premium (paid) session.</summary>
        public bool IsVoicePremium { get; private set; }

        /// <summary>Total allowed duration of the voice session in seconds.</summary>
        public int VoiceSessionDuration { get; private set; }

        /// <summary><see cref="Time.realtimeSinceStartup"/> when the voice session started.</summary>
        public float VoiceSessionStartTime { get; private set; }

        /// <summary>Seconds remaining in the active voice session (0 when inactive).</summary>
        public float RemainingVoiceTime => IsVoiceSessionActive
            ? Mathf.Max(0, VoiceSessionDuration - (Time.realtimeSinceStartup - VoiceSessionStartTime))
            : 0f;

        /// <summary>Backend session ID for the active host session, or null.</summary>
        public string CurrentHostSessionId { get; private set; }

        /// <summary>True when a host commentary session is currently in progress.</summary>
        public bool IsHostSessionActive => !string.IsNullOrEmpty(CurrentHostSessionId);

        #endregion

        #region Private Fields

        private IVXAIApiClient _api;
        private IVXAIWebSocketClient _ws;
        private IVXAIAudioPlayer _audioPlayer;
        private IVXAIAudioRecorder _audioRecorder;
        private IVXAIEntitlementManager _entitlement;

        private string _authToken;
        private IVXAIConnectionMode _connectionMode;
        private Coroutine _pollingCoroutine;
        private Coroutine _hostPollingCoroutine;
        private IVXAIPlayerContext _playerContext;

        #endregion

        #region Unity Lifecycle

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }
            _instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private void OnDestroy()
        {
            if (_instance == this) _instance = null;
        }

        #endregion

        #region Initialization

        /// <summary>
        /// Initialise the AI system. Must be called before starting any session.
        /// </summary>
        /// <param name="userId">Unique player/user identifier.</param>
        /// <param name="userName">Display name shown to the AI persona.</param>
        /// <param name="authToken">Optional bearer token for authenticated API calls.</param>
        /// <param name="language">Optional ISO 639-1 language code; defaults to <see cref="IVXAIConfig.DefaultLanguage"/>.</param>
        public void Initialize(string userId, string userName, string authToken = null, string language = null)
        {
            if (_config == null)
            {
                Debug.LogError($"[{nameof(IVXAISessionManager)}] Config asset not assigned.");
                return;
            }

            if (!_config.Validate(out string err))
            {
                Debug.LogError($"[{nameof(IVXAISessionManager)}] Config invalid: {err}");
                return;
            }

            UserId = userId;
            UserName = userName;
            CurrentLanguage = string.IsNullOrEmpty(language) ? _config.DefaultLanguage : language;

            _authToken = authToken;
            _api = new IVXAIApiClient(_config, this);
            if (!string.IsNullOrEmpty(authToken))
                _api.SetAuthToken(authToken);

            _entitlement = new IVXAIEntitlementManager(_api, _config, userId);

            EnsureAudioComponents();
            IsInitialized = true;

            LogDebug("Initialized");
        }

        /// <summary>Update the bearer token (e.g. after Nakama auth refresh).</summary>
        public void SetAuthToken(string token)
        {
            _authToken = token;
            _api?.SetAuthToken(token);
        }

        /// <summary>Set player context for AI personalization in voice sessions.</summary>
        public void SetPlayerContext(IVXAIPlayerContext context)
        {
            _playerContext = context;
        }

        #endregion

        #region Voice Session API

        /// <summary>
        /// Start a voice session with an AI persona.
        /// Performs an entitlement check first; if the user lacks access,
        /// <see cref="OnEntitlementRequired"/> fires instead.
        /// </summary>
        /// <param name="personaId">ID of the AI persona to converse with.</param>
        /// <param name="topic">Optional conversation topic or context hint.</param>
        /// <param name="onSuccess">Callback invoked on successful session creation.</param>
        /// <param name="onError">Callback invoked with an error message on failure.</param>
        public void StartVoiceSession(string personaId, string topic = null, Action<IVXAICreateVoiceSessionResponse> onSuccess = null, Action<string> onError = null)
        {
            EnsureInitialized("StartVoiceSession");

            _entitlement.CheckAccess(personaId, ent =>
            {
                if (ent == null || !ent.CanAccessPersona)
                {
                    OnEntitlementRequired?.Invoke(ent);
                    return;
                }

                CreateVoiceSessionInternal(personaId, topic, onSuccess, onError);
            },
            err => onError?.Invoke(err));
        }

        /// <summary>
        /// Start a voice session without an entitlement check.
        /// Use when the game manages its own access control.
        /// </summary>
        public void StartVoiceSessionDirect(string personaId, string topic = null, Action<IVXAICreateVoiceSessionResponse> onSuccess = null, Action<string> onError = null)
        {
            EnsureInitialized("StartVoiceSessionDirect");
            CreateVoiceSessionInternal(personaId, topic, onSuccess, onError);
        }

        /// <summary>End the current voice session.</summary>
        public void EndVoiceSession(Action<IVXAISessionAnalytics> callback = null)
        {
            if (!IsVoiceSessionActive) return;

            string sid = CurrentVoiceSessionId;
            StopVoicePolling();
            DisconnectWebSocket();
            _audioPlayer?.StopAll();

            _api.EndVoiceSession(sid, resp =>
            {
                LogDebug("Voice session ended");
                ClearVoiceState();
                callback?.Invoke(resp?.Analytics);
                OnSessionEnded?.Invoke(resp?.Analytics);
            },
            err =>
            {
                Debug.LogWarning($"[{nameof(IVXAISessionManager)}] EndVoiceSession error: {err}");
                ClearVoiceState();
                OnSessionEnded?.Invoke(null);
            });
        }

        /// <summary>Send a text message in the current voice session.</summary>
        /// <param name="text">User text to send to the AI persona.</param>
        public void SendText(string text)
        {
            if (!IsVoiceSessionActive) return;

            if (_connectionMode == IVXAIConnectionMode.WebSocket && _ws != null && _ws.IsConnected)
            {
                _ws.Send(JsonConvert.SerializeObject(new { type = "user_text", text }));
            }
            else
            {
                _api.SendVoiceText(CurrentVoiceSessionId, text, null, err => OnError?.Invoke(err));
            }
        }

        /// <summary>Send PCM16 audio data in the current voice session.</summary>
        public void SendAudio(byte[] pcmData)
        {
            if (!IsVoiceSessionActive) return;

            if (_connectionMode == IVXAIConnectionMode.WebSocket && _ws != null && _ws.IsConnected)
            {
                _ws.SendAudioChunked(pcmData);
            }
            else
            {
                _api.SendVoiceAudio(CurrentVoiceSessionId, pcmData, null, err => OnError?.Invoke(err));
            }
        }

        /// <summary>Commit buffered audio (signals end of user speech).</summary>
        public void CommitAudio()
        {
            if (!IsVoiceSessionActive) return;

            if (_connectionMode == IVXAIConnectionMode.WebSocket && _ws != null && _ws.IsConnected)
            {
                _ws.Send("{\"type\":\"input_audio_buffer.commit\"}");
            }
            else
            {
                _api.CommitVoiceAudio(CurrentVoiceSessionId, null, err => OnError?.Invoke(err));
            }
        }

        /// <summary>Trigger the AI to speak a specific prompt.</summary>
        public void TriggerSpeech(string prompt)
        {
            if (!IsVoiceSessionActive) return;

            if (_connectionMode == IVXAIConnectionMode.WebSocket && _ws != null && _ws.IsConnected)
            {
                _ws.Send(JsonConvert.SerializeObject(new { type = "trigger_speech", prompt }));
            }
            else
            {
                _api.TriggerVoiceSpeech(CurrentVoiceSessionId, prompt, null, err => OnError?.Invoke(err));
            }
        }

        /// <summary>Stop audio playback immediately.</summary>
        public void StopAudio() => _audioPlayer?.StopAll();

        /// <summary>Start capturing microphone audio.</summary>
        public void StartRecording() => _audioRecorder?.StartRecording();

        /// <summary>Stop capturing microphone audio.</summary>
        public void StopRecording() => _audioRecorder?.StopRecording();

        #endregion

        #region Host Session API

        /// <summary>
        /// Create an AI Host session for game commentary.
        /// </summary>
        /// <param name="request">Host session configuration (match ID, game mode, players, etc.).</param>
        /// <param name="onSuccess">Callback invoked on successful session creation.</param>
        /// <param name="onError">Callback invoked with an error message on failure.</param>
        public void StartHostSession(IVXAICreateHostSessionRequest request, Action<IVXAICreateHostSessionResponse> onSuccess = null, Action<string> onError = null)
        {
            EnsureInitialized("StartHostSession");

            if (request.Language == null)
                request.Language = CurrentLanguage;

            _api.CreateHostSession(request, resp =>
            {
                if (resp == null || !resp.Success)
                {
                    string err = resp?.Error ?? "Host session creation failed";
                    onError?.Invoke(err);
                    return;
                }

                CurrentHostSessionId = resp.SessionId;
                StartHostPolling();

                LogDebug($"Host session started: {resp.SessionId}");
                onSuccess?.Invoke(resp);
                OnHostSessionStarted?.Invoke(resp);
            },
            err => onError?.Invoke(err));
        }

        /// <summary>End the current host session.</summary>
        public void EndHostSession(Action callback = null)
        {
            if (!IsHostSessionActive) return;

            string sid = CurrentHostSessionId;
            StopHostPolling();

            _api.EndHostSession(sid, _ =>
            {
                LogDebug("Host session ended");
                CurrentHostSessionId = null;
                callback?.Invoke();
                OnHostSessionEnded?.Invoke();
            },
            err =>
            {
                Debug.LogWarning($"[{nameof(IVXAISessionManager)}] EndHostSession error: {err}");
                CurrentHostSessionId = null;
                OnHostSessionEnded?.Invoke();
            });
        }

        /// <summary>Send text in the host session.</summary>
        /// <param name="playerId">Identifier of the player sending the message.</param>
        /// <param name="text">Text message to relay to the AI host.</param>
        public void SendHostText(string playerId, string text)
        {
            if (IsHostSessionActive)
                _api.SendHostText(CurrentHostSessionId, playerId, text, null, err => OnError?.Invoke(err));
        }

        /// <summary>Send a game event to the host.</summary>
        /// <param name="eventType">Type of game event (e.g. "round_start", "question_reveal").</param>
        /// <param name="state">Serialised game state snapshot.</param>
        /// <param name="data">Optional additional event payload.</param>
        public void SendHostGameEvent(string eventType, string state, string data = null)
        {
            if (IsHostSessionActive)
                _api.SendHostGameEvent(CurrentHostSessionId, eventType, state, data, null, err => OnError?.Invoke(err));
        }

        /// <summary>Submit a player answer to the host.</summary>
        /// <param name="playerId">Identifier of the answering player.</param>
        /// <param name="answerIndex">Zero-based index of the selected answer option.</param>
        public void SubmitHostAnswer(string playerId, int answerIndex)
        {
            if (IsHostSessionActive)
                _api.SubmitHostAnswer(CurrentHostSessionId, playerId, answerIndex, null, err => OnError?.Invoke(err));
        }

        /// <summary>Trigger host speech.</summary>
        public void TriggerHostSpeech(string prompt)
        {
            if (IsHostSessionActive)
                _api.TriggerHostSpeech(CurrentHostSessionId, prompt, null, err => OnError?.Invoke(err));
        }

        #endregion

        #region Entitlement / Products

        /// <summary>Directly query the entitlement manager.</summary>
        public IVXAIEntitlementManager Entitlement => _entitlement;

        /// <summary>Fetch available personas from the backend.</summary>
        public void GetPersonas(Action<IVXAIPersona[]> onSuccess, Action<string> onError = null)
        {
            EnsureInitialized("GetPersonas");
            _api.GetPersonas(resp =>
            {
                if (resp?.Success == true) onSuccess?.Invoke(resp.Personas);
                else onError?.Invoke(resp?.Error ?? "Failed to fetch personas");
            }, err => onError?.Invoke(err));
        }

        #endregion

        #region Internal — Voice Session

        private void CreateVoiceSessionInternal(string personaId, string topic, Action<IVXAICreateVoiceSessionResponse> onSuccess, Action<string> onError)
        {
            var request = new IVXAICreateVoiceSessionRequest
            {
                Persona = personaId,
                UserId = UserId,
                UserName = UserName,
                Topic = topic,
                Language = CurrentLanguage,
                PlayerContext = _playerContext != null ? JsonConvert.SerializeObject(_playerContext) : null
            };

            _api.CreateVoiceSession(request, resp =>
            {
                if (resp == null || !resp.Success)
                {
                    string err = resp?.Error ?? "Voice session creation failed";
                    onError?.Invoke(err);
                    return;
                }

                CurrentVoiceSessionId = resp.SessionId;
                CurrentVoicePersona = resp.Persona;
                IsVoicePremium = resp.IsPremium;
                VoiceSessionDuration = resp.DurationSeconds;
                VoiceSessionStartTime = Time.realtimeSinceStartup;

                if (_config.PreferWebSocket)
                    ConnectWebSocket(resp.SessionId);
                else
                    StartVoicePolling();

                LogDebug($"Voice session started: {resp.SessionId} (persona={resp.Persona})");
                onSuccess?.Invoke(resp);
                OnSessionStarted?.Invoke(resp);
            },
            err => onError?.Invoke(err));
        }

        private void ConnectWebSocket(string sessionId)
        {
            _connectionMode = IVXAIConnectionMode.WebSocket;

            if (_ws == null)
            {
                var go = new GameObject("IVXAIWebSocket");
                go.transform.SetParent(transform);
                _ws = go.AddComponent<IVXAIWebSocketClient>();
            }

            _ws.SetDebugLogging(_config.DebugLogging);
            string url = $"{_config.WebSocketUrl}?sessionId={sessionId}&userId={UserId}";
            _ws.Initialize(url, _authToken, sessionId);

            _ws.OnConnected -= OnWsConnected;
            _ws.OnConnected += OnWsConnected;
            _ws.OnMessageReceived -= OnWsMessage;
            _ws.OnMessageReceived += OnWsMessage;
            _ws.OnBinaryReceived -= OnWsBinary;
            _ws.OnBinaryReceived += OnWsBinary;
            _ws.OnConnectionLost -= OnWsLost;
            _ws.OnConnectionLost += OnWsLost;
            _ws.OnReconnectionFailed -= OnWsFailed;
            _ws.OnReconnectionFailed += OnWsFailed;

            _ws.Connect();
        }

        private void DisconnectWebSocket()
        {
            if (_ws != null)
            {
                _ws.OnConnected -= OnWsConnected;
                _ws.OnMessageReceived -= OnWsMessage;
                _ws.OnBinaryReceived -= OnWsBinary;
                _ws.OnConnectionLost -= OnWsLost;
                _ws.OnReconnectionFailed -= OnWsFailed;
                _ws.Disconnect();
            }
        }

        private void OnWsConnected()
        {
            _ws.Send(JsonConvert.SerializeObject(new { type = "join_session", sessionId = CurrentVoiceSessionId, userId = UserId }));
            LogDebug("WebSocket connected, joined session");
        }

        private void OnWsMessage(string json)
        {
            try
            {
                var msg = JsonConvert.DeserializeObject<IVXAIMessage>(json);
                ProcessVoiceMessage(msg);
            }
            catch (Exception ex)
            {
                LogDebug($"WS message parse error: {ex.Message}");
            }
        }

        private void OnWsBinary(byte[] data)
        {
            _audioPlayer?.EnqueuePcm(data);
        }

        private void OnWsLost()
        {
            LogDebug("WebSocket lost — falling back to HTTP polling");
            _connectionMode = IVXAIConnectionMode.HttpPolling;
            StartVoicePolling();
        }

        private void OnWsFailed(string reason)
        {
            OnError?.Invoke($"Realtime connection failed: {reason}");
        }

        private void StartVoicePolling()
        {
            StopVoicePolling();
            _connectionMode = IVXAIConnectionMode.HttpPolling;
            _pollingCoroutine = StartCoroutine(VoicePollLoop());
        }

        private void StopVoicePolling()
        {
            if (_pollingCoroutine != null) { StopCoroutine(_pollingCoroutine); _pollingCoroutine = null; }
        }

        private IEnumerator VoicePollLoop()
        {
            var wait = new WaitForSeconds(_config.PollingInterval);
            while (IsVoiceSessionActive)
            {
                _api.PollVoiceMessages(CurrentVoiceSessionId, resp =>
                {
                    if (resp?.Messages == null) return;
                    foreach (var msg in resp.Messages)
                        ProcessVoiceMessage(msg);
                }, null);
                yield return wait;
            }
        }

        private void ProcessVoiceMessage(IVXAIMessage msg)
        {
            if (msg == null) return;
            var type = msg.GetMessageType();

            switch (type)
            {
                case IVXAIMessageType.VoiceAudio:
                    OnAudioReceived?.Invoke(msg.Audio);
                    _audioPlayer?.EnqueueBase64(msg.Audio);
                    break;
                case IVXAIMessageType.VoiceCaption:
                    OnCaptionReceived?.Invoke(msg.Text);
                    break;
                case IVXAIMessageType.VoiceCaptionComplete:
                    OnCaptionComplete?.Invoke(msg.Text);
                    break;
                case IVXAIMessageType.VoiceTurnComplete:
                    OnTurnComplete?.Invoke();
                    break;
                case IVXAIMessageType.SpeechDetected:
                    OnSpeechDetected?.Invoke();
                    break;
                case IVXAIMessageType.SpeechStopped:
                    OnSpeechStopped?.Invoke();
                    break;
                case IVXAIMessageType.SocialProof:
                    if (msg.SocialProof != null) OnSocialProofReceived?.Invoke(msg.SocialProof);
                    break;
                case IVXAIMessageType.UpsellMessage:
                    OnUpsellPrompt?.Invoke(msg.UpsellMessage ?? msg.Message);
                    break;
                case IVXAIMessageType.ScarcityMessage:
                    OnScarcityMessage?.Invoke(msg.ScarcityMessage ?? msg.Message);
                    break;
                case IVXAIMessageType.SessionEnding:
                    OnSessionTimeWarning?.Invoke();
                    break;
                case IVXAIMessageType.SessionComplete:
                    EndVoiceSession();
                    break;
                case IVXAIMessageType.Error:
                    OnError?.Invoke(msg.Error ?? msg.Text);
                    break;
                case IVXAIMessageType.ServerShutdown:
                    EndVoiceSession();
                    break;
            }
        }

        private void ClearVoiceState()
        {
            CurrentVoiceSessionId = null;
            CurrentVoicePersona = null;
            IsVoicePremium = false;
            VoiceSessionDuration = 0;
        }

        #endregion

        #region Internal — Host Session

        private void StartHostPolling()
        {
            StopHostPolling();
            _hostPollingCoroutine = StartCoroutine(HostPollLoop());
        }

        private void StopHostPolling()
        {
            if (_hostPollingCoroutine != null) { StopCoroutine(_hostPollingCoroutine); _hostPollingCoroutine = null; }
        }

        private IEnumerator HostPollLoop()
        {
            var wait = new WaitForSeconds(_config.PollingInterval);
            while (IsHostSessionActive)
            {
                _api.PollHostMessages(CurrentHostSessionId, resp =>
                {
                    if (resp?.Messages == null) return;
                    foreach (var msg in resp.Messages)
                        OnHostMessageReceived?.Invoke(msg);
                }, null);
                yield return wait;
            }
        }

        #endregion

        #region Internal — Audio Setup

        private void EnsureAudioComponents()
        {
            if (_audioPlayer == null)
            {
                _audioPlayer = GetComponentInChildren<IVXAIAudioPlayer>();
                if (_audioPlayer == null)
                {
                    var go = new GameObject("IVXAIAudioPlayer");
                    go.transform.SetParent(transform);
                    _audioPlayer = go.AddComponent<IVXAIAudioPlayer>();
                }
            }
            _audioPlayer.Initialize(_config, _audioSource);

            if (_audioRecorder == null)
            {
                _audioRecorder = GetComponentInChildren<IVXAIAudioRecorder>();
                if (_audioRecorder == null)
                {
                    var go = new GameObject("IVXAIAudioRecorder");
                    go.transform.SetParent(transform);
                    _audioRecorder = go.AddComponent<IVXAIAudioRecorder>();
                }
            }
            _audioRecorder.Initialize(_config, pcm => SendAudio(pcm));
        }

        #endregion

        #region Helpers

        private void EnsureInitialized(string caller)
        {
            if (!IsInitialized)
                Debug.LogError($"[{nameof(IVXAISessionManager)}] {caller} called before Initialize().");
        }

        private void LogDebug(string msg)
        {
            if (_config != null && _config.DebugLogging)
                Debug.Log($"[{nameof(IVXAISessionManager)}] {msg}");
        }

        #endregion
    }
}
