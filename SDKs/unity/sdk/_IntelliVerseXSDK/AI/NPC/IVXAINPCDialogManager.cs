using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Networking;

namespace IntelliVerseX.AI
{
    /// <summary>
    /// Manages registration of NPC profiles, active dialog sessions, and HTTP dialog traffic to the IVX AI NPC API.
    /// </summary>
    public sealed class IVXAINPCDialogManager : MonoBehaviour
    {
        #region Singleton

        private static IVXAINPCDialogManager _instance;

        /// <summary>Singleton instance (lazy-resolved via <see cref="FindFirstObjectByType{T}"/> if unset).</summary>
        public static IVXAINPCDialogManager Instance
        {
            get
            {
                if (_instance == null)
                    _instance = FindFirstObjectByType<IVXAINPCDialogManager>();
                return _instance;
            }
        }

        #endregion

        #region Private Fields

        private readonly Dictionary<string, IVXAINPCProfile> _registeredNPCs = new Dictionary<string, IVXAINPCProfile>(StringComparer.Ordinal);
        private readonly Dictionary<string, IVXAINPCDialogSession> _activeSessions = new Dictionary<string, IVXAINPCDialogSession>(StringComparer.Ordinal);
        private IVXAIConfig _config;
        private string _authToken;
        private ReadOnlyDictionary<string, IVXAINPCProfile> _readOnlyRegistered;
        private ReadOnlyDictionary<string, IVXAINPCDialogSession> _readOnlySessions;

        #endregion

        #region Events

        /// <summary>Fired when the NPC produces text for a session: (sessionId, responseText).</summary>
        public event Action<string, string> OnNPCResponse;

        /// <summary>Fired when the server returns an action to run: (sessionId, action).</summary>
        public event Action<string, IVXAINPCAction> OnNPCAction;

        /// <summary>Fired when a dialog session becomes active: (sessionId).</summary>
        public event Action<string> OnDialogStarted;

        /// <summary>Fired when a dialog session ends: (sessionId).</summary>
        public event Action<string> OnDialogEnded;

        /// <summary>Fired on transport or logic errors: (sessionId, errorMessage).</summary>
        public event Action<string, string> OnError;

        #endregion

        #region Properties

        /// <summary>Registered NPC profiles keyed by <see cref="IVXAINPCProfile.NpcId"/>.</summary>
        public IReadOnlyDictionary<string, IVXAINPCProfile> RegisteredNPCs
        {
            get
            {
                if (_readOnlyRegistered == null)
                    _readOnlyRegistered = new ReadOnlyDictionary<string, IVXAINPCProfile>(_registeredNPCs);
                return _readOnlyRegistered;
            }
        }

        /// <summary>Active dialog sessions keyed by <see cref="IVXAINPCDialogSession.SessionId"/>.</summary>
        public IReadOnlyDictionary<string, IVXAINPCDialogSession> ActiveSessions
        {
            get
            {
                if (_readOnlySessions == null)
                    _readOnlySessions = new ReadOnlyDictionary<string, IVXAINPCDialogSession>(_activeSessions);
                return _readOnlySessions;
            }
        }

        /// <summary>True after <see cref="Initialize"/> with a valid config.</summary>
        public bool IsInitialized => _config != null;

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
            if (_instance == this)
                _instance = null;
        }

        #endregion

        #region Public Methods — Initialization

        /// <summary>
        /// Binds configuration for NPC HTTP calls (headers and base URL from <see cref="IVXAIConfig"/>).
        /// </summary>
        /// <param name="config">AI configuration asset (base URL, API key, timeout).</param>
        public void Initialize(IVXAIConfig config)
        {
            if (config == null)
            {
                Debug.LogError($"[{nameof(IVXAINPCDialogManager)}] Config is null.");
                return;
            }

            if (!config.Validate(out string err))
            {
                Debug.LogError($"[{nameof(IVXAINPCDialogManager)}] Invalid config: {err}");
                return;
            }

            _config = config;
        }

        /// <summary>
        /// Sets the bearer token applied to subsequent NPC HTTP requests (<c>Authorization</c> header).
        /// </summary>
        /// <param name="token">OAuth / JWT bearer token, or null to clear.</param>
        public void SetAuthToken(string token)
        {
            _authToken = token;
        }

        #endregion

        #region Public Methods — NPC Registry

        /// <summary>Registers or replaces an NPC profile.</summary>
        public void RegisterNPC(IVXAINPCProfile profile)
        {
            if (profile == null || string.IsNullOrEmpty(profile.NpcId))
            {
                RaiseError(null, "RegisterNPC: profile or NpcId is invalid.");
                return;
            }

            _registeredNPCs[profile.NpcId] = profile;
        }

        /// <summary>Removes a previously registered NPC by id.</summary>
        public void UnregisterNPC(string npcId)
        {
            if (string.IsNullOrEmpty(npcId))
                return;
            _registeredNPCs.Remove(npcId);
        }

        #endregion

        #region Public Methods — Dialog

        /// <summary>
        /// Creates a server session and starts tracking it locally.
        /// </summary>
        /// <param name="npcId">Registered NPC id.</param>
        /// <param name="playerId">Current player id.</param>
        /// <param name="playerContext">Optional serialized context.</param>
        /// <param name="onStarted">Invoked with the session when the server returns.</param>
        public void StartDialog(string npcId, string playerId, string playerContext = null, Action<IVXAINPCDialogSession> onStarted = null)
        {
            if (!EnsureReady())
                return;

            if (!_registeredNPCs.ContainsKey(npcId))
            {
                RaiseError(null, $"StartDialog: NPC '{npcId}' is not registered.");
                return;
            }

            var request = new IVXAINPCDialogRequest
            {
                NpcId = npcId,
                PlayerId = playerId,
                PlayerContext = playerContext,
                Message = null,
                SessionId = null
            };

            string url = _config.ApiBaseUrl.TrimEnd('/') + "/npc/sessions";
            SendNpcPost(url, request, (IVXAINPCDialogResponse response) =>
            {
                if (response == null || string.IsNullOrEmpty(response.SessionId))
                {
                    RaiseError(null, "StartDialog: invalid server response (missing session_id).");
                    return;
                }

                long now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                var session = new IVXAINPCDialogSession
                {
                    SessionId = response.SessionId,
                    NpcId = npcId,
                    PlayerId = playerId,
                    StartTimestamp = now,
                    TurnCount = response.TurnCount,
                    State = IVXAINPCDialogState.Active
                };

                if (!string.IsNullOrEmpty(response.NpcResponse))
                {
                    session.History.Add(new IVXAINPCDialogMessage
                    {
                        Role = "npc",
                        Content = response.NpcResponse,
                        Timestamp = now,
                        Action = response.Action
                    });
                }

                session.State = response.IsComplete ? IVXAINPCDialogState.Ended : IVXAINPCDialogState.WaitingForPlayer;

                _activeSessions[session.SessionId] = session;
                OnDialogStarted?.Invoke(session.SessionId);
                if (!string.IsNullOrEmpty(response.NpcResponse))
                    OnNPCResponse?.Invoke(session.SessionId, response.NpcResponse);
                if (response.Action != null)
                    OnNPCAction?.Invoke(session.SessionId, response.Action);
                onStarted?.Invoke(session);
            }, err => RaiseError(null, err));
        }

        /// <summary>
        /// Sends a player message for an active session and updates history from the server reply.
        /// </summary>
        public void SendMessage(string sessionId, string message, Action<string> onResponse = null)
        {
            if (!EnsureReady())
                return;

            if (string.IsNullOrEmpty(sessionId) || !_activeSessions.TryGetValue(sessionId, out IVXAINPCDialogSession session))
            {
                RaiseError(sessionId, "SendMessage: session not found or ended.");
                return;
            }

            if (session.State == IVXAINPCDialogState.Ended)
            {
                RaiseError(sessionId, "SendMessage: session already ended.");
                return;
            }

            if (!_registeredNPCs.TryGetValue(session.NpcId, out IVXAINPCProfile profile))
            {
                RaiseError(sessionId, "SendMessage: NPC profile missing.");
                return;
            }

            if (profile.MaxTurns > 0 && session.TurnCount >= profile.MaxTurns)
            {
                RaiseError(sessionId, "SendMessage: maximum turns reached.");
                return;
            }

            session.State = IVXAINPCDialogState.WaitingForNPC;
            long tsPlayer = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            session.History.Add(new IVXAINPCDialogMessage
            {
                Role = "player",
                Content = message,
                Timestamp = tsPlayer,
                Action = null
            });

            var request = new IVXAINPCDialogRequest
            {
                NpcId = session.NpcId,
                PlayerId = session.PlayerId,
                PlayerContext = null,
                Message = message,
                SessionId = sessionId
            };

            string url = _config.ApiBaseUrl.TrimEnd('/') + "/npc/message";
            SendNpcPost(url, request, (IVXAINPCDialogResponse response) =>
            {
                if (response == null)
                {
                    RaiseError(sessionId, "SendMessage: empty response.");
                    session.State = IVXAINPCDialogState.WaitingForPlayer;
                    return;
                }

                session.TurnCount = response.TurnCount;
                long tsNpc = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                if (!string.IsNullOrEmpty(response.NpcResponse))
                {
                    session.History.Add(new IVXAINPCDialogMessage
                    {
                        Role = "npc",
                        Content = response.NpcResponse,
                        Timestamp = tsNpc,
                        Action = response.Action
                    });
                }

                session.State = response.IsComplete ? IVXAINPCDialogState.Ended : IVXAINPCDialogState.WaitingForPlayer;
                if (!string.IsNullOrEmpty(response.NpcResponse))
                    OnNPCResponse?.Invoke(sessionId, response.NpcResponse);
                if (response.Action != null)
                    OnNPCAction?.Invoke(sessionId, response.Action);
                if (response.IsComplete)
                {
                    OnDialogEnded?.Invoke(sessionId);
                    _activeSessions.Remove(sessionId);
                }

                onResponse?.Invoke(response.NpcResponse);
            }, err =>
            {
                session.State = IVXAINPCDialogState.WaitingForPlayer;
                RaiseError(sessionId, err);
            });
        }

        /// <summary>
        /// Ends a session on the server and removes it locally.
        /// </summary>
        public void EndDialog(string sessionId, Action onComplete = null)
        {
            if (!EnsureReady())
                return;

            if (string.IsNullOrEmpty(sessionId))
            {
                RaiseError(null, "EndDialog: sessionId is empty.");
                return;
            }

            string url = _config.ApiBaseUrl.TrimEnd('/') + "/npc/sessions/" + UnityWebRequest.EscapeURL(sessionId);
            StartCoroutine(DeleteSessionCoroutine(url, sessionId, onComplete));
        }

        /// <summary>Returns the active session for an id, or null.</summary>
        public IVXAINPCDialogSession GetSession(string sessionId)
        {
            if (string.IsNullOrEmpty(sessionId))
                return null;
            _activeSessions.TryGetValue(sessionId, out IVXAINPCDialogSession s);
            return s;
        }

        /// <summary>Lists active sessions whose <see cref="IVXAINPCDialogSession.NpcId"/> matches.</summary>
        public List<IVXAINPCDialogSession> GetSessionsForNPC(string npcId)
        {
            var list = new List<IVXAINPCDialogSession>();
            if (string.IsNullOrEmpty(npcId))
                return list;
            foreach (var kv in _activeSessions)
            {
                if (string.Equals(kv.Value.NpcId, npcId, StringComparison.Ordinal))
                    list.Add(kv.Value);
            }

            return list;
        }

        #endregion

        #region Private Methods — HTTP

        private void SendNpcPost(string url, IVXAINPCDialogRequest body, Action<IVXAINPCDialogResponse> onSuccess, Action<string> onError)
        {
            string json = JsonConvert.SerializeObject(body, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
            var request = new UnityWebRequest(url, UnityWebRequest.kHttpVerbPOST)
            {
                uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(json)),
                downloadHandler = new DownloadHandlerBuffer()
            };
            request.SetRequestHeader("Content-Type", "application/json");
            StartCoroutine(RequestCoroutine(request, onSuccess, onError));
        }

        private IEnumerator RequestCoroutine(UnityWebRequest request, Action<IVXAINPCDialogResponse> onSuccess, Action<string> onError)
        {
            ApplyHeaders(request);
            request.timeout = (int)_config.RequestTimeout;

            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                onError?.Invoke($"{request.method} {request.url} failed: {request.error}");
                request.Dispose();
                yield break;
            }

            try
            {
                string text = request.downloadHandler.text;
                if (_config.DebugLogging)
                    Debug.Log($"[{nameof(IVXAINPCDialogManager)}] {request.method} {request.url} → {text}");
                var result = JsonConvert.DeserializeObject<IVXAINPCDialogResponse>(text);
                onSuccess?.Invoke(result);
            }
            catch (Exception ex)
            {
                onError?.Invoke($"Deserialization error: {ex.Message}");
            }
            finally
            {
                request.Dispose();
            }
        }

        private IEnumerator DeleteSessionCoroutine(string url, string sessionId, Action onComplete)
        {
            var request = UnityWebRequest.Delete(url);
            request.downloadHandler = new DownloadHandlerBuffer();
            ApplyHeaders(request);
            request.timeout = (int)_config.RequestTimeout;

            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                RaiseError(sessionId, $"{request.method} {request.url} failed: {request.error}");
                request.Dispose();
                yield break;
            }

            _activeSessions.Remove(sessionId);
            OnDialogEnded?.Invoke(sessionId);
            onComplete?.Invoke();
            request.Dispose();
        }

        private void ApplyHeaders(UnityWebRequest request)
        {
            if (!string.IsNullOrEmpty(_authToken))
                request.SetRequestHeader("Authorization", $"Bearer {_authToken}");

            if (_config != null && !string.IsNullOrEmpty(_config.ApiKey))
                request.SetRequestHeader("X-API-Key", _config.ApiKey);
        }

        private bool EnsureReady()
        {
            if (_config != null)
                return true;
            RaiseError(null, $"{nameof(IVXAINPCDialogManager)} is not initialized. Call Initialize first.");
            return false;
        }

        private void RaiseError(string sessionId, string message)
        {
            Debug.LogWarning($"[{nameof(IVXAINPCDialogManager)}] {message}");
            OnError?.Invoke(sessionId, message);
        }

        #endregion
    }
}
