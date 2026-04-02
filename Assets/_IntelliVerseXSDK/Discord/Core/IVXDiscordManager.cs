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
        private string _publisherId;
        private Action _authorizeRequestCallback;

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
        /// <summary>Publisher ID for cross-game shared authentication (Discord Social SDK).</summary>
        public string PublisherId => _publisherId;

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
        /// <summary>Fired when the Discord client requests account linking (user used an entry point in Discord).</summary>
        public event Action OnAuthorizeRequested;

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

            _authorizeRequestCallback = null;
#if INTELLIVERSEX_HAS_DISCORD
            RemoveDiscordAuthorizeCallback();
#endif

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

        #region Account Linking — Advanced

        /// <summary>
        /// Registers a callback for when the Discord client wants to start account linking (e.g. user tapped an entry point in Discord).
        /// Call when the game is ready to handle linking, such as from the main menu.
        /// </summary>
        /// <param name="onAuthorizeRequested">Invoked when the user initiates linking from Discord.</param>
        public void RegisterAuthorizeRequestCallback(Action onAuthorizeRequested)
        {
            if (!_initialized)
            {
                Debug.LogError($"{LOG_TAG} Not initialized. Call Initialize() first.");
                return;
            }

            Debug.Log($"{LOG_TAG} Registering authorize request callback.");

            _authorizeRequestCallback = onAuthorizeRequested;

#if INTELLIVERSEX_HAS_DISCORD
            // Wire to: client->RegisterAuthorizeRequestCallback
            RegisterDiscordAuthorizeCallback(HandleAuthorizeRequestedFromDiscord);
#else
            Debug.Log($"{LOG_TAG} Discord Social SDK not available; authorize request callback not registered.");
#endif
        }

        /// <summary>
        /// Removes the authorize request callback. Call when the game enters a state where linking cannot happen (e.g. match, cutscene).
        /// </summary>
        public void RemoveAuthorizeRequestCallback()
        {
            if (!_initialized)
            {
                Debug.LogWarning($"{LOG_TAG} Not initialized; authorize request callback not removed.");
                return;
            }

            Debug.Log($"{LOG_TAG} Removing authorize request callback.");

            _authorizeRequestCallback = null;

#if INTELLIVERSEX_HAS_DISCORD
            RemoveDiscordAuthorizeCallback();
#else
            Debug.Log($"{LOG_TAG} Discord Social SDK not available.");
#endif
        }

        /// <summary>
        /// Starts the mobile OAuth2 flow with PKCE and deep linking using the given URL scheme for the redirect URI.
        /// </summary>
        /// <param name="redirectScheme">Custom URL scheme for the app (mobile deep link).</param>
        /// <param name="onComplete">Called with true when linking succeeds.</param>
        public void StartMobileOAuth2Flow(string redirectScheme, Action<bool> onComplete = null)
        {
            if (!_initialized)
            {
                Debug.LogError($"{LOG_TAG} Not initialized. Call Initialize() first.");
                onComplete?.Invoke(false);
                return;
            }

            if (string.IsNullOrEmpty(redirectScheme))
            {
                Debug.LogError($"{LOG_TAG} redirectScheme is required for mobile OAuth2.");
                onComplete?.Invoke(false);
                return;
            }

            Debug.Log($"{LOG_TAG} Starting mobile OAuth2 (PKCE) flow with redirect scheme: {redirectScheme}");

#if INTELLIVERSEX_HAS_DISCORD
            // Wire to: mobile OAuth2 + PKCE
            StartMobilePKCEFlow(redirectScheme, onComplete);
#else
            Debug.Log($"{LOG_TAG} Discord Social SDK not available; mobile OAuth2 flow stubbed.");
            onComplete?.Invoke(false);
#endif
        }

        /// <summary>
        /// Starts the console device-code OAuth2 flow. Display the device code from <paramref name="onDeviceCode"/> to the user.
        /// </summary>
        /// <param name="onDeviceCode">Receives the user-visible device code string.</param>
        /// <param name="onComplete">Called with true when the flow completes successfully.</param>
        public void StartConsoleOAuth2Flow(Action<string> onDeviceCode, Action<bool> onComplete = null)
        {
            if (!_initialized)
            {
                Debug.LogError($"{LOG_TAG} Not initialized. Call Initialize() first.");
                onComplete?.Invoke(false);
                return;
            }

            if (onDeviceCode == null)
            {
                Debug.LogError($"{LOG_TAG} onDeviceCode is required for console OAuth2.");
                onComplete?.Invoke(false);
                return;
            }

            Debug.Log($"{LOG_TAG} Starting console OAuth2 (device code) flow.");

#if INTELLIVERSEX_HAS_DISCORD
            // Wire to: device code flow
            StartDeviceCodeFlow(onDeviceCode, onComplete);
#else
            Debug.Log($"{LOG_TAG} Discord Social SDK not available; console OAuth2 flow stubbed.");
            onComplete?.Invoke(false);
#endif
        }

        /// <summary>
        /// Sets the publisher ID used for publisher-level account linking across multiple games.
        /// </summary>
        /// <param name="publisherId">Publisher identifier from Discord.</param>
        public void SetPublisherId(string publisherId)
        {
            if (!_initialized)
            {
                Debug.LogError($"{LOG_TAG} Not initialized. Call Initialize() first.");
                return;
            }

            Debug.Log($"{LOG_TAG} Setting publisher ID.");

            _publisherId = publisherId;

#if INTELLIVERSEX_HAS_DISCORD
            // Wire to: client / publisher shared auth configuration
#else
            Debug.Log($"{LOG_TAG} Discord Social SDK not available; publisher ID stored locally only.");
#endif
        }

        /// <summary>
        /// Merges a provisional account with a full Discord account using an external auth token from your backend or OAuth completion.
        /// </summary>
        /// <param name="externalAuthToken">Token used to complete the merge with Discord.</param>
        /// <param name="onComplete">Called with true if the merge succeeds.</param>
        public void MergeProvisionalAccount(string externalAuthToken, Action<bool> onComplete = null)
        {
            if (!_initialized)
            {
                Debug.LogError($"{LOG_TAG} Not initialized. Call Initialize() first.");
                onComplete?.Invoke(false);
                return;
            }

            if (string.IsNullOrEmpty(externalAuthToken))
            {
                Debug.LogError($"{LOG_TAG} externalAuthToken is required to merge provisional account.");
                onComplete?.Invoke(false);
                return;
            }

            Debug.Log($"{LOG_TAG} Merging provisional account with Discord account.");

#if INTELLIVERSEX_HAS_DISCORD
            // Wire to: client provisional account merge
            MergeDiscordProvisionalAccount(externalAuthToken, onComplete);
#else
            Debug.Log($"{LOG_TAG} Discord Social SDK not available; merge provisional account stubbed.");
            onComplete?.Invoke(false);
#endif
        }

        /// <summary>
        /// Updates the stored OAuth2 access token without running a full sign-in (e.g. after browser-based linking on web).
        /// </summary>
        /// <param name="newToken">New bearer token from OAuth2.</param>
        public void UpdateToken(string newToken)
        {
            if (!_initialized)
            {
                Debug.LogError($"{LOG_TAG} Not initialized. Call Initialize() first.");
                return;
            }

            if (string.IsNullOrEmpty(newToken))
            {
                Debug.LogError($"{LOG_TAG} newToken is required.");
                return;
            }

            Debug.Log($"{LOG_TAG} Updating OAuth2 token.");

#if INTELLIVERSEX_HAS_DISCORD
            // Wire to: client->UpdateToken
            UpdateDiscordToken(newToken);
#else
            Debug.Log($"{LOG_TAG} Discord Social SDK not available; token not applied.");
#endif
        }

        #endregion

        #region Social Settings

        /// <summary>
        /// Opens Discord&apos;s Connected Games settings where users manage DM and related options for linked games.
        /// </summary>
        public void OpenConnectedGamesSettingsInDiscord()
        {
            if (!_initialized)
            {
                Debug.LogError($"{LOG_TAG} Not initialized. Call Initialize() first.");
                return;
            }

            Debug.Log($"{LOG_TAG} Opening Connected Games settings in Discord.");

#if INTELLIVERSEX_HAS_DISCORD
            // Wire to: client->OpenConnectedGamesSettingsInDiscord()
            OpenDiscordSettings();
#else
            Debug.Log($"{LOG_TAG} Discord Social SDK not available; cannot open Connected Games settings.");
#endif
        }

        /// <summary>
        /// Opens the Discord profile for the given user in the Discord client.
        /// </summary>
        /// <param name="userId">Discord snowflake user ID.</param>
        public void OpenUserProfileInDiscord(ulong userId)
        {
            if (!_initialized)
            {
                Debug.LogError($"{LOG_TAG} Not initialized. Call Initialize() first.");
                return;
            }

            Debug.Log($"{LOG_TAG} Opening Discord profile for user {userId}.");

#if INTELLIVERSEX_HAS_DISCORD
            // Wire to: open profile
            OpenDiscordProfile(userId);
#else
            Debug.Log($"{LOG_TAG} Discord Social SDK not available; cannot open profile.");
#endif
        }

        #endregion

        #region Private Methods — Discord SDK Wiring

        private void HandleAuthorizeRequestedFromDiscord()
        {
            Debug.Log($"{LOG_TAG} Authorize requested by Discord client.");
            OnAuthorizeRequested?.Invoke();
            _authorizeRequestCallback?.Invoke();
        }

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

        private void RegisterDiscordAuthorizeCallback(Action callback)
        {
            // Wire to: client->RegisterAuthorizeRequestCallback
        }

        private void RemoveDiscordAuthorizeCallback()
        {
            // Wire to: client->RemoveAuthorizeRequestCallback
        }

        private void StartMobilePKCEFlow(string redirectScheme, Action<bool> onComplete)
        {
            // Wire to: mobile OAuth2 + PKCE
        }

        private void StartDeviceCodeFlow(Action<string> onDeviceCode, Action<bool> onComplete)
        {
            // Wire to: device code flow
        }

        private void MergeDiscordProvisionalAccount(string externalAuthToken, Action<bool> onComplete)
        {
            // Wire to: client provisional account merge
        }

        private void UpdateDiscordToken(string token)
        {
            // Wire to: client->UpdateToken
        }

        private void OpenDiscordSettings()
        {
            // Wire to: client->OpenConnectedGamesSettingsInDiscord()
        }

        private void OpenDiscordProfile(ulong userId)
        {
            // Wire to: open profile
        }
#endif

        #endregion
    }
}
