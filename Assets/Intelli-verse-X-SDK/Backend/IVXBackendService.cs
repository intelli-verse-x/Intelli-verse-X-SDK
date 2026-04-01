using System;
using System.Threading.Tasks;
using UnityEngine;
using Nakama;
using IntelliVerseX.Identity;

namespace IntelliVerseX.Backend
{
    /// <summary>
    /// Manages Nakama client and session lifecycle.
    /// Provides a centralized point for backend authentication.
    /// </summary>
    public class IVXBackendService : MonoBehaviour
    {
        private static IVXBackendService _instance;
        private static readonly object _instanceLock = new object();
        private static bool _isQuitting;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics()
        {
            _instance = null;
            _isQuitting = false;
        }

        public static IVXBackendService Instance
        {
            get
            {
                if (_isQuitting) return null;

                lock (_instanceLock)
                {
                    if (_instance == null)
                    {
                        var go = new GameObject("IVXBackendService");
                        _instance = go.AddComponent<IVXBackendService>();
                        DontDestroyOnLoad(go);
                    }
                    return _instance;
                }
            }
        }

        public static bool HasInstance => _instance != null && !_isQuitting;

        [Header("Nakama Configuration")]
        [SerializeField] private string scheme = "https";
        [SerializeField] private string host = "nakama-rest.intelli-verse-x.ai";
        [SerializeField] private int port = 443;
        [SerializeField] private string serverKey = "";

        private IClient _client;
        private ISession _session;
        private bool _isInitialized = false;

        public IClient Client => _client;
        public ISession Session => _session;
        public bool IsInitialized => _isInitialized;
        public bool IsSessionValid => _session != null && !_session.IsExpired;

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

        private void OnApplicationQuit()
        {
            _isQuitting = true;
        }

        private void OnDestroy()
        {
            if (_instance == this)
            {
                _instance = null;
            }
        }

        /// <summary>
        /// Initialize Nakama client.
        /// </summary>
        private void InitializeClient()
        {
            if (_client == null)
            {
                _client = new Client(scheme, host, port, serverKey, UnityWebRequestAdapter.Instance);
                Debug.Log($"[IVXBackendService] Nakama client initialized - {scheme}://{host}:{port}");
            }
        }

        /// <summary>
        /// Authenticate with Nakama using device ID and username.
        /// Updates IntelliVerseXUserIdentity with Nakama credentials.
        /// </summary>
        public async Task<bool> AuthenticateAsync()
        {
            try
            {
                InitializeClient();

                // Ensure identity is synced
                if (string.IsNullOrEmpty(IntelliVerseXUserIdentity.DeviceId))
                {
                    IntelliVerseXUserIdentity.InitializeDevice();
                }

                if (string.IsNullOrEmpty(IntelliVerseXUserIdentity.DisplayName))
                {
                    IntelliVerseXUserIdentity.SyncFromUserSessionManager();
                }

                string deviceId = IntelliVerseXUserIdentity.DeviceId;
                string username = IntelliVerseXUserIdentity.DisplayName;

                if (string.IsNullOrEmpty(username))
                {
                    username = $"Guest_{UnityEngine.Random.Range(1000, 9999)}";
                    Debug.LogWarning($"[IVXBackendService] No username available, using fallback: {username}");
                }

                Debug.Log($"[IVXBackendService] Authenticating with Nakama - Device: {deviceId}, Username: {username}");

                _session = await _client.AuthenticateDeviceAsync(deviceId, create: true, username: username);

                if (_session != null)
                {
                    // Update identity with Nakama credentials
                    IntelliVerseXUserIdentity.SetNakamaAuth(_session.UserId, _session.AuthToken);

                    _isInitialized = true;
                    Debug.Log($"[IVXBackendService] Authentication successful - NakamaUserId: {_session.UserId}, Expires: {_session.ExpireTime}");
                    return true;
                }
                else
                {
                    Debug.LogError("[IVXBackendService] Authentication failed - session is null");
                    return false;
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"[IVXBackendService] Authentication failed: {ex.Message}\nStack: {ex.StackTrace}");
                return false;
            }
        }

        /// <summary>
        /// Ensure we have a valid session, re-authenticating if necessary.
        /// </summary>
        public async Task<bool> EnsureAuthenticatedAsync()
        {
            if (IsSessionValid)
            {
                return true;
            }

            Debug.Log("[IVXBackendService] Session invalid or expired, re-authenticating...");
            return await AuthenticateAsync();
        }

        /// <summary>
        /// Clear session data (e.g., on logout).
        /// </summary>
        public void ClearSession()
        {
            _session = null;
            _isInitialized = false;
            Debug.Log("[IVXBackendService] Session cleared");
        }
    }
}
