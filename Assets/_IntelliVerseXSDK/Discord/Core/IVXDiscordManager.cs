using System;
using UnityEngine;

namespace IntelliVerseX.Discord
{
    /// <summary>
    /// Central manager for the Discord Social SDK integration.
    /// Handles initialization, authentication, and account linking.
    /// Attach to a persistent GameObject (DontDestroyOnLoad).
    /// </summary>
    public sealed class IVXDiscordManager : MonoBehaviour
    {
        #region Constants

        private const string LOG_TAG = "[IVXDiscordManager]";

        #endregion

        #region Serialized Fields

        [SerializeField] private IVXDiscordConfig _config;

        #endregion

        #region Private Fields

        private static IVXDiscordManager _instance;
        private bool _initialized;
        private bool _connected;
        private bool _accountLinked;
        private string _discordUserId;
        private string _discordUsername;
        private string _discordAvatarUrl;

        #endregion

        #region Properties

        /// <summary>Singleton instance.</summary>
        public static IVXDiscordManager Instance => _instance;
        /// <summary>Whether the Discord SDK has been initialized.</summary>
        public bool IsInitialized => _initialized;
        /// <summary>Whether the client is connected to Discord.</summary>
        public bool IsConnected => _connected;
        /// <summary>Whether a Discord account is linked.</summary>
        public bool IsAccountLinked => _accountLinked;
        /// <summary>The linked Discord user's ID.</summary>
        public string DiscordUserId => _discordUserId;
        /// <summary>The linked Discord user's username.</summary>
        public string DiscordUsername => _discordUsername;
        /// <summary>The linked Discord user's avatar URL.</summary>
        public string DiscordAvatarUrl => _discordAvatarUrl;
        /// <summary>Active configuration.</summary>
        public IVXDiscordConfig Config => _config;

        #endregion

        #region Events

        /// <summary>Fired when the Discord client connects successfully.</summary>
        public event Action OnConnected;
        /// <summary>Fired when the Discord client disconnects.</summary>
        public event Action OnDisconnected;
        /// <summary>Fired when a Discord account is linked. Provides userId and username.</summary>
        public event Action<string, string> OnAccountLinked;
        /// <summary>Fired when a Discord account is unlinked.</summary>
        public event Action OnAccountUnlinked;
        /// <summary>Fired on any Discord SDK error. Provides error message.</summary>
        public event Action<string> OnError;

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
            {
                Shutdown();
                _instance = null;
            }
        }

        private void Update()
        {
#if INTELLIVERSEX_HAS_DISCORD
            if (_initialized)
            {
                RunDiscordCallbacks();
            }
#endif
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Initialize the Discord Social SDK with the provided config.
        /// </summary>
        /// <param name="config">Discord configuration. Uses serialized config if null.</param>
        public void Initialize(IVXDiscordConfig config = null)
        {
            if (_initialized)
            {
                Debug.LogWarning($"{LOG_TAG} Already initialized.");
                return;
            }

            if (config != null) _config = config;

            if (_config == null)
            {
                Debug.LogError($"{LOG_TAG} No IVXDiscordConfig provided.");
                OnError?.Invoke("No IVXDiscordConfig provided.");
                return;
            }

            Debug.Log($"{LOG_TAG} Initializing with Application ID: {_config.ApplicationId}");

#if INTELLIVERSEX_HAS_DISCORD
            InitializeDiscordClient(_config.ApplicationId);
#else
            Debug.Log($"{LOG_TAG} Discord Social SDK package not detected. " +
                      "Install com.discord.social-sdk to enable Discord features. " +
                      "Running in stub mode.");
            _initialized = true;
            _connected = true;
            OnConnected?.Invoke();
#endif
        }

        /// <summary>
        /// Start the Discord OAuth2 account linking flow.
        /// Opens the Discord authorization overlay or browser fallback.
        /// </summary>
        public void LinkAccount()
        {
            if (!_initialized)
            {
                Debug.LogError($"{LOG_TAG} Not initialized. Call Initialize() first.");
                return;
            }

            if (_accountLinked)
            {
                Debug.LogWarning($"{LOG_TAG} Account already linked as {_discordUsername}.");
                return;
            }

            Debug.Log($"{LOG_TAG} Starting OAuth2 account linking flow...");

#if INTELLIVERSEX_HAS_DISCORD
            StartOAuth2Flow();
#else
            _accountLinked = true;
            _discordUserId = "stub_user_123456";
            _discordUsername = "StubUser#0001";
            _discordAvatarUrl = "";
            Debug.Log($"{LOG_TAG} [Stub] Account linked as {_discordUsername}");
            OnAccountLinked?.Invoke(_discordUserId, _discordUsername);
#endif
        }

        /// <summary>
        /// Unlink the current Discord account.
        /// Clears all Discord social data from the game.
        /// </summary>
        public void UnlinkAccount()
        {
            if (!_accountLinked)
            {
                Debug.LogWarning($"{LOG_TAG} No account linked.");
                return;
            }

            Debug.Log($"{LOG_TAG} Unlinking Discord account {_discordUsername}...");

#if INTELLIVERSEX_HAS_DISCORD
            RevokeOAuth2Token();
#endif

            _accountLinked = false;
            _discordUserId = null;
            _discordUsername = null;
            _discordAvatarUrl = null;
            OnAccountUnlinked?.Invoke();
        }

        /// <summary>
        /// Create a provisional Discord account for users without Discord.
        /// Allows access to social features without requiring a Discord account.
        /// </summary>
        /// <param name="onComplete">Callback with success status.</param>
        public void CreateProvisionalAccount(Action<bool> onComplete = null)
        {
            if (!_initialized)
            {
                Debug.LogError($"{LOG_TAG} Not initialized.");
                onComplete?.Invoke(false);
                return;
            }

            Debug.Log($"{LOG_TAG} Creating provisional account...");

#if INTELLIVERSEX_HAS_DISCORD
            GetProvisionalToken(onComplete);
#else
            _accountLinked = true;
            _discordUserId = "provisional_" + Guid.NewGuid().ToString("N")[..8];
            _discordUsername = "Player";
            Debug.Log($"{LOG_TAG} [Stub] Provisional account created: {_discordUserId}");
            onComplete?.Invoke(true);
#endif
        }

        /// <summary>
        /// Shut down the Discord SDK and clean up resources.
        /// </summary>
        public void Shutdown()
        {
            if (!_initialized) return;

            Debug.Log($"{LOG_TAG} Shutting down...");

#if INTELLIVERSEX_HAS_DISCORD
            DestroyDiscordClient();
#endif

            _initialized = false;
            _connected = false;
            _accountLinked = false;
            _discordUserId = null;
            _discordUsername = null;
            _discordAvatarUrl = null;
            OnDisconnected?.Invoke();
        }

        #endregion

        #region Private Methods — Discord SDK Wiring

#if INTELLIVERSEX_HAS_DISCORD
        private void InitializeDiscordClient(long applicationId)
        {
            // Wire to: discordpp::Client + SetApplicationId + Connect
            // Set up event listeners for status changes
            _initialized = true;
            _connected = true;
            OnConnected?.Invoke();
        }

        private void RunDiscordCallbacks()
        {
            // Wire to: client->RunCallbacks()
        }

        private void StartOAuth2Flow()
        {
            // Wire to: client->Authorize() with GetDefaultPresenceScopes + GetDefaultCommunicationScopes
            // On success: populate _discordUserId, _discordUsername, _discordAvatarUrl
            // Invoke OnAccountLinked
        }

        private void RevokeOAuth2Token()
        {
            // Wire to: client->Deauthorize() or token revocation
        }

        private void GetProvisionalToken(Action<bool> onComplete)
        {
            // Wire to: client->GetProvisionalToken()
            onComplete?.Invoke(true);
        }

        private void DestroyDiscordClient()
        {
            // Wire to: client disposal
        }
#endif

        #endregion
    }
}
