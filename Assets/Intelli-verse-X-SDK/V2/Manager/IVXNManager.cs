using IntelliVerseX.Core;
using Nakama;
using Newtonsoft.Json;
#if SYCH_SHARE_ASSETS
using Sych.ShareAssets.Runtime;
#endif
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace IntelliVerseX.Backend.Nakama
{
    public sealed class IVXNManager : MonoBehaviour
    {
        #region Configuration
        public static IVXNManager Instance { get; private set; }
        private static bool _isQuitting;

        [Header("SDK Configuration")]
        [SerializeField] private IntelliVerseXConfig sdkConfig;

        [Header("Behaviour")]
        [SerializeField] private bool initializeOnAwake = false;

        [Header("Geolocation")]
        [Tooltip("Enable automatic geolocation capture on authentication")]
        [SerializeField] private bool captureGeolocationOnAuth = true;
        [SerializeField] private float geoLocationTimeoutSeconds = 20f;

        [Header("Retry Configuration")]
        [Tooltip("Number of retry attempts for RPC calls")]
        [SerializeField] private int maxRetryAttempts = 3;
        [Tooltip("Base delay between retries in seconds (exponential backoff)")]
        [SerializeField] private float retryBaseDelaySeconds = 1f;

        [Header("Debugging")]
        [SerializeField] private bool enableDebugLogs = true;
        [SerializeField] private bool enableVerboseLogging = false;

        private string _scheme;
        private string _host;
        private int _port;
        private string _serverKey;
        private string _gameId;

        private IClient _client;
        private ISession _session;
        private bool _isInitialized;
        private bool _isInitializing;

        private const string PREF_NAKAMA_AUTH_TOKEN = "ivxn.nakama.auth_token";
        private const string PREF_NAKAMA_REFRESH_TOKEN = "ivxn.nakama.refresh_token";
        private const string PP_REMEMBER = "auth.remember";

        public IClient Client => _client;
        public ISession Session => _session;
        public bool IsInitialized => _isInitialized;
        public bool IsInitializing => _isInitializing;
        public string GameId => _gameId;
        public string NakamaUserId => _session?.UserId;
        public string NakamaUsername => _session?.Username;
        public long NakamaExpireTimeUnix => _session?.ExpireTime ?? 0;

        public event Action<bool> OnInitialized;
        public event Action<string> OnMetadataSyncFailed;
        public event Action OnMetadataSyncSuccess;
        public event Action<IVXNProfileManager.IVXNProfileSnapshot> OnProfileLoaded;
        public event Action<IVXNProfileManager.IVXNProfileSnapshot> OnProfileUpdated;
        public event Action<string> OnProfileError;

        private const string LOGTAG = "[IVXNManager]";

        // Geolocation state
        private GeolocationData _cachedGeoData;
    #pragma warning disable CS0414 // Geolocation completion flag reserved for future conditional flows
        private bool _geoLocationCaptured;
    #pragma warning restore CS0414

        // Sync state tracking
        private bool _metadataSyncInProgress;
        private DateTime _lastMetadataSyncAttempt = DateTime.MinValue;
        private const int MIN_SYNC_INTERVAL_SECONDS = 30;


        #endregion

        #region Unity Lifecycle

        private void Awake()
        {
            if (_isQuitting)
            {
                return;
            }

            if (Instance != null && Instance != this)
            {
                Log("Duplicate instance detected.  Destroying this one.", isWarning: true);
                Destroy(gameObject);
                return;
            }

            Instance = this;
            if (transform.parent != null)
            {
                Log("Manager was parented in scene hierarchy. Detaching before DontDestroyOnLoad.", isWarning: true);
                transform.SetParent(null, true);
            }
            DontDestroyOnLoad(gameObject);

            try
            {
                LoadConfig();
                CreateClientIfNeeded();

                IVXNWalletManager.RefreshFromServerAsync = RefreshWalletFromServerAsync;
                IVXNWalletManager.ApplyOperationOnServerAsync = ApplyWalletOperationOnServerAsync;

                IVXNProfileManager.EnableDebugLogs = enableDebugLogs;
                IVXNProfileManager.OnProfileLoaded -= HandleProfileLoaded;
                IVXNProfileManager.OnProfileUpdated -= HandleProfileUpdated;
                IVXNProfileManager.OnProfileError -= HandleProfileError;
                IVXNProfileManager.OnProfileLoaded += HandleProfileLoaded;
                IVXNProfileManager.OnProfileUpdated += HandleProfileUpdated;
                IVXNProfileManager.OnProfileError += HandleProfileError;
            }
            catch (Exception ex)
            {
                Log($"Error during Awake configuration: {ex.Message}\n{ex.StackTrace}", isError: true);
            }

            if (initializeOnAwake)
            {
                StartCoroutine(InitializeAsync());
            }
        }

        private void OnApplicationQuit()
        {
            _isQuitting = true;
        }

        private void OnDestroy()
        {
            IVXNProfileManager.OnProfileLoaded -= HandleProfileLoaded;
            IVXNProfileManager.OnProfileUpdated -= HandleProfileUpdated;
            IVXNProfileManager.OnProfileError -= HandleProfileError;

            if (Instance == this)
            {
                Instance = null;
            }
        }

        private IEnumerator InitializeAsync()
        {
            var task = InitializeForCurrentUserAsync();
            while (!task.IsCompleted)
            {
                yield return null;
            }

            if (task.IsFaulted)
            {
                Log($"Initialization failed: {task.Exception?.GetBaseException()?.Message}", isError: true);
            }
        }

        #endregion

        #region Config & Client

        private void LoadConfig()
        {
            if (sdkConfig == null)
            {
                const string defaultPath = "IntelliVerseX/GameConfig";
                sdkConfig = UnityEngine.Resources.Load<IntelliVerseXConfig>(defaultPath);

                if (sdkConfig == null)
                {
                    Log($"IntelliVerseXConfig not found at Resources/{defaultPath}.", isError: true);
                    return;
                }
            }

            _scheme = sdkConfig.nakamaScheme;
            _host = sdkConfig.nakamaHost;
            _port = sdkConfig.nakamaPort;
            _serverKey = sdkConfig.nakamaServerKey;
            _gameId = sdkConfig.gameId;

            // Validate configuration
            if (string.IsNullOrWhiteSpace(_gameId) || !IsValidGuid(_gameId))
            {
                Log($"Invalid gameId in config: {_gameId}. Must be valid UUID.", isError: true);
            }

            Log($"Config loaded: gameId={_gameId}, endpoint={_scheme}://{_host}:{_port}");
        }


private void CreateClientIfNeeded()
        {
            if (_client != null) return;

            if (string.IsNullOrWhiteSpace(_host) || string.IsNullOrWhiteSpace(_scheme) ||
                _port <= 0 || string.IsNullOrWhiteSpace(_serverKey))
            {
                Log("Invalid Nakama config. Please check IntelliVerseXConfig.", isError: true);
                return;
            }

            // validate scheme
            var schemeLower = _scheme?.ToLowerInvariant();
            if (schemeLower != "http" && schemeLower != "https")
            {
                Log($"Unsupported Nakama scheme '{_scheme}'. Must be 'http' or 'https'.", isError: true);
                return;
            }

            try
            {
                // Mask serverKey in logs (only show first/last few chars)
                var maskedKey = _serverKey.Length > 8 ? $"{_serverKey.Substring(0, 4)}...{_serverKey.Substring(_serverKey.Length - 4)}" : _serverKey;
                Log($"Creating Nakama client for endpoint {_scheme}://{_host}:{_port} (serverKey={maskedKey})");

                _client = new Client(_scheme, _host, _port, _serverKey, UnityWebRequestAdapter.Instance);
                Log("Nakama Client created.");
            }
            catch (Exception ex)
            {
                Log($"CreateClientIfNeeded error: {ex}", isError: true);
            }
        }

        #endregion

        #region Public API

        /// <summary>
        /// Initialize Nakama for the CURRENT logged-in user with comprehensive error handling
        /// </summary>
        public async Task<bool> InitializeForCurrentUserAsync(bool forceReauth = false)
        {
            if (_isInitialized && !forceReauth && _session != null && !_session.IsExpired)
            {
                Log("Already initialized and session is valid.");
                SafeInvokeOnInitialized(true);
                return true;
            }

            if (_isInitializing)
            {
                Log("Initialization already in progress.  Please wait.. .", isWarning: true);

                // Wait for existing initialization to complete (with timeout)
                var timeout = DateTime.UtcNow.AddSeconds(30);
                while (_isInitializing && DateTime.UtcNow < timeout)
                {
                    await Task.Delay(100);
                }

                return _isInitialized;
            }

            _isInitializing = true;
            bool success = false;

            try
            {
                // Validate client
                if (_client == null) CreateClientIfNeeded();
                if (_client == null)
                {
                    Log("Client is null after creation attempt.  Check configuration.", isError: true);
                    return false;
                }

                bool allowPersistence = ShouldPersistNakama();

                // Step 1: Try restore session
                if (!forceReauth && allowPersistence &&
                    TryRestoreSession(out var restored) && restored != null && !restored.IsExpired)
                {
                    _session = restored;
                    _isInitialized = true;
                    success = true;

                    IVXNWalletManager.RefreshFromServerAsync = RefreshWalletFromServerAsync;
                    IVXNWalletManager.ApplyOperationOnServerAsync = ApplyWalletOperationOnServerAsync;

                    LogSession("Restored existing Nakama session.");

                    // Background sync (non-blocking)
                    SyncPlayerMetadataInBackground();

                    return true;
                }

                // Step 2: Get current user session
                var userSession = global::UserSessionManager.Current;
                if (userSession == null)
                {
                    Log("UserSessionManager.Current is null. User must login first.", isError: true);
                    return false;
                }

                // Step 3: Validate user session data
                if (!ValidateUserSession(userSession))
                {
                    Log("User session validation failed. Cannot proceed with Nakama auth.", isError: true);
                    return false;
                }

                // Step 4: Build auth credentials
                var customId = BuildCustomId(userSession);
                var username = BuildUsername(userSession);

                LogVerbose($"Auth credentials: customId='{customId}', username='{username}'");

                // Step 5: Authenticate with Nakama (with retry)
                var newSession = await AuthenticateWithRetryAsync(customId, username);
                if (newSession == null)
                {
                    Log("Authentication failed after all retry attempts.", isError: true);
                    return false;
                }

                _session = newSession;
                _isInitialized = true;
                success = true;

                IVXNWalletManager.RefreshFromServerAsync = RefreshWalletFromServerAsync;
                IVXNWalletManager.ApplyOperationOnServerAsync = ApplyWalletOperationOnServerAsync;

                SaveSession(_session);
                LogSession("Authenticated new Nakama session.");

                // Step 6: Sync player metadata + geolocation
                await SyncPlayerMetadataAndGeolocationAsync(userSession);

                return true;
            }
            catch (Exception ex)
            {
                Log($"InitializeForCurrentUserAsync failed: {ex.Message}\n{ex.StackTrace}", isError: true);
                return false;
            }
            finally
            {
                _isInitialized = success;
                _isInitializing = false;
                SafeInvokeOnInitialized(success);
            }
        }

        public async Task<bool> EnsureValidSessionAsync()
        {
            if (!_isInitialized || _client == null)
            {
                Log("Manager not initialized.  Initializing now...", isWarning: true);
                return await InitializeForCurrentUserAsync();
            }

            if (_session == null || _session.IsExpired)
            {
                Log("Session expired. Re-authenticating...", isWarning: true);
                return await InitializeForCurrentUserAsync(forceReauth: true);
            }

            return true;
        }

        public void ClearNakamaSession()
        {
            _session = null;
            _isInitialized = false;
            _geoLocationCaptured = false;
            _cachedGeoData = null;
            _metadataSyncInProgress = false;

            try
            {
                PlayerPrefs.DeleteKey(PREF_NAKAMA_AUTH_TOKEN);
                PlayerPrefs.DeleteKey(PREF_NAKAMA_REFRESH_TOKEN);
                PlayerPrefs.Save();
                Log("Nakama session cleared from PlayerPrefs.");
            }
            catch (Exception ex)
            {
                Log($"ClearNakamaSession error: {ex.Message}", isError: true);
            }
        }

        public Task<IVXNProfileManager.IVXNProfileFetchResult> FetchProfileAsync(CancellationToken cancellationToken = default)
        {
            return IVXNProfileManager.FetchProfileAsync(cancellationToken);
        }

        public Task<IVXNProfileManager.IVXNProfileUpdateResult> UpdateProfileAsync(
            IVXNProfileManager.IVXNProfileUpdateRequest request,
            CancellationToken cancellationToken = default)
        {
            return IVXNProfileManager.UpdateProfileAsync(request, cancellationToken);
        }

        public Task<IVXNProfileManager.IVXNProfilePortfolioResult> FetchPortfolioAsync(CancellationToken cancellationToken = default)
        {
            return IVXNProfileManager.FetchPortfolioAsync(cancellationToken);
        }

        #endregion

        #region Player Metadata & Geolocation Sync

        [Serializable]
        private class CreateOrSyncUserRequest
        {
            public string username;
            public string device_id;
            public string game_id;
            public string cognito_user_id;
            public string email;
            public string first_name;
            public string last_name;
            public string role;
            public string login_type;
            public string idp_username;
            public string account_status;
            public string wallet_address;
            public string is_adult;
        }

        [Serializable]
        private class CreateOrSyncUserResponse
        {
            public bool success;
            public string error;
            public string errorCode;
            public bool created;
            public string userId;
            public string username;
            public string device_id;
            public string game_id;
            public string wallet_id;
            public string global_wallet_id;
            public long gameWalletBalance;
            public long globalWalletBalance;
            public int executionTimeMs;
            public string requestId;
            public string timestamp;
            public List<string> validationErrors;
        }





        private class GeolocationData
        {
            public double Latitude;
            public double Longitude;
            public bool Success;
            public string Error;

            // Reverse geocoded data (from server)
            public string Country;
            public string CountryCode;
            public string Region;
            public string City;
            public string Timezone;
            public bool IsAllowed = true;
            public string BlockReason;

            // Metadata
            public DateTime CapturedAt;
            public string Source; // "gps", "network", "ip", "cached"
            public float AccuracyMeters;
        }

        private class DeviceInfoData
        {
            public string DeviceId;
            public string Platform;
            public string DeviceModel;
            public string DeviceName;
            public string OsVersion;
            public string AppVersion;
            public string UnityVersion;
            public string Locale;
            public string Timezone;
            public int ScreenWidth;
            public int ScreenHeight;
            public string ScreenDpi;
            public string GraphicsDevice;
            public int SystemMemoryMb;
            public string ProcessorType;
            public int ProcessorCount;
        }
        /// <summary>
        /// Main sync method with rate limiting and error handling
        /// </summary>
        /// <summary>
        /// UNIFIED sync method - single source of truth for player metadata
        /// Handles geolocation, device info, and metadata in one flow
        /// </summary>
        /// <summary>
        /// UNIFIED sync method - single source of truth for player metadata
        /// Handles geolocation (GPS or IP fallback), device info, and metadata
        /// </summary>
        private async Task SyncPlayerMetadataAndGeolocationAsync(global::UserSessionManager.UserSession userSession)
        {
            // Rate limiting
            var timeSinceLastSync = DateTime.UtcNow - _lastMetadataSyncAttempt;
            if (timeSinceLastSync.TotalSeconds < MIN_SYNC_INTERVAL_SECONDS && _lastMetadataSyncAttempt != DateTime.MinValue)
            {
                LogVerbose($"[Sync] Skipping (last sync was {timeSinceLastSync.TotalSeconds:F1}s ago)");
                return;
            }

            if (_metadataSyncInProgress)
            {
                Log("[Sync] Already in progress, skipping", isWarning: true);
                return;
            }

            _metadataSyncInProgress = true;
            _lastMetadataSyncAttempt = DateTime.UtcNow;

            var syncStartTime = DateTime.UtcNow;

            try
            {
                Log("[Sync] ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
                Log("[Sync] Starting UNIFIED player metadata sync.. .");

                // Step 1: Collect device information
                Log("[Sync] Step 1/5: Collecting device info...");
                var deviceInfo = CollectDeviceInfo();
                Log($"[Sync] ✓ Device: {deviceInfo.Platform} | {deviceInfo.DeviceModel}");
                LogVerbose($"[Sync]   OS: {deviceInfo.OsVersion}");
                LogVerbose($"[Sync]   App: {deviceInfo.AppVersion}");

                // Step 2: Capture geolocation (GPS or IP fallback)
                GeolocationData geoData = null;
                if (captureGeolocationOnAuth)
                {
                    Log("[Sync] Step 2/5: Capturing geolocation...");

                    bool hasGPS = IsLocationServiceAvailable();
                    Log($"[Sync] GPS Available: {hasGPS}");

                    if (hasGPS)
                    {
                        // Try GPS first (mobile devices)
                        try
                        {
                            geoData = await CaptureGeolocationAsync();

                            if (geoData != null && geoData.Success)
                            {
                                Log($"[Sync] ✓ GPS Location: {geoData.Latitude:F4}, {geoData.Longitude:F4}");
                            }
                            else
                            {
                                Log($"[Sync] ⚠ GPS failed: {geoData?.Error ?? "unknown"}, trying IP fallback.. .", isWarning: true);
                                geoData = await GetGeolocationFromIPAsync();
                            }
                        }
                        catch (Exception gpsEx)
                        {
                            Log($"[Sync] ⚠ GPS exception: {gpsEx.Message}, trying IP fallback.. .", isWarning: true);
                            geoData = await GetGeolocationFromIPAsync();
                        }
                    }
                    else
                    {
                        // Desktop/Editor - use IP geolocation directly
                        geoData = await GetGeolocationFromIPAsync();
                    }

                    // Update cache if successful
                    if (geoData != null && geoData.Success)
                    {
                        _cachedGeoData = geoData;
                        _geoLocationCaptured = true;
                        Log($"[Sync] ✓ Location ({geoData.Source}): {geoData.City}, {geoData.Region}, {geoData.Country} ({geoData.CountryCode})");
                    }
                    else
                    {
                        Log($"[Sync] ⚠ Geolocation unavailable: {geoData?.Error ?? "unknown"}", isWarning: true);
                    }
                }
                else
                {
                    Log("[Sync] Step 2/5: Geolocation disabled in settings, skipping");
                }

                // Step 3: Call create_or_sync_user for identity + wallet initialization
                Log("[Sync] Step 3/5: Syncing user identity and wallets...");
                var syncResult = await CallCreateOrSyncUserWithRetryAsync(userSession, deviceInfo.DeviceId);

                if (!syncResult.success)
                {
                    var errorMsg = $"Identity sync failed: {syncResult.error} (Code: {syncResult.errorCode})";
                    Log($"[Sync] ✗ {errorMsg}", isError: true);
                    OnMetadataSyncFailed?.Invoke(errorMsg);
                    return;
                }

                Log($"[Sync] ✓ Identity: Created={syncResult.created}, UserId={syncResult.userId}");
                LogVerbose($"[Sync]   Wallet: {syncResult.wallet_id}");
                LogVerbose($"[Sync]   GameBalance: {syncResult.gameWalletBalance}, GlobalBalance: {syncResult.globalWalletBalance}");

                // Step 4: Call rpc_update_player_metadata with FULL data (including geo)
                Log("[Sync] Step 4/5: Updating unified player metadata...");
                var metaResult = await CallUpdatePlayerMetadataWithRetryAsync(userSession, deviceInfo, geoData);

                if (!metaResult.success)
                {
                    Log($"[Sync] ⚠ Metadata update warning: {metaResult.error}", isWarning: true);
                }
                else
                {
                    var m = metaResult.metadata;
                    if (m != null)
                    {
                        Log($"[Sync] ✓ Metadata stored: Collection=player_metadata, Key=user_identity");
                        LogVerbose($"[Sync]   Games: {m.total_games}, Sessions: {m.total_sessions}, Devices: {m.total_devices}");

                        // Log geolocation from response
                        if (!string.IsNullOrEmpty(m.country_code))
                        {
                            Log($"[Sync]   📍 Location: {m.city}, {m.region}, {m.country} ({m.country_code})");
                        }
                        else if (m.latitude.HasValue && m.longitude.HasValue)
                        {
                            Log($"[Sync]   📍 Coords: {m.latitude:F4}, {m.longitude:F4}");
                        }

                        if (m.analytics != null)
                        {
                            LogVerbose($"[Sync]   Analytics: Streak={m.analytics.current_streak}, DaysActive={m.analytics.days_active}");
                        }
                    }
                }

                // Step 5: Additional geo resolution via server (if GPS was used and we need reverse geocoding)
                if (geoData != null && geoData.Success && geoData.Source == "gps" && string.IsNullOrEmpty(geoData.CountryCode))
                {
                    Log("[Sync] Step 5/5: Resolving GPS coordinates to location...");
                    try
                    {
                        var resolvedGeoData = await CallCheckGeoAndUpdateProfileRPCAsync(geoData);

                        if (resolvedGeoData != null && !string.IsNullOrEmpty(resolvedGeoData.CountryCode))
                        {
                            _cachedGeoData = resolvedGeoData;
                            Log($"[Sync] ✓ Location resolved: {resolvedGeoData.City}, {resolvedGeoData.Region}, {resolvedGeoData.Country} ({resolvedGeoData.CountryCode})");

                            if (!resolvedGeoData.IsAllowed)
                            {
                                Log($"[Sync] ⚠ Region blocked: {resolvedGeoData.BlockReason}", isWarning: true);
                            }
                        }
                    }
                    catch (Exception geoRpcEx)
                    {
                        Log($"[Sync] ⚠ Location resolution failed: {geoRpcEx.Message}", isWarning: true);
                    }
                }
                else
                {
                    Log("[Sync] Step 5/5: Location already resolved, skipping server geocoding");
                }

                Log("[Sync] Step 6/6: Refreshing profile snapshot cache...");
                var profileRefreshSuccess = await IVXNProfileManager.RefreshProfileAfterAuthAsync();
                if (!profileRefreshSuccess)
                {
                    Log("[Sync] ⚠ Profile snapshot refresh failed after auth sync", isWarning: true);
                }

                var syncDuration = (DateTime.UtcNow - syncStartTime).TotalMilliseconds;
                Log($"[Sync] ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
                Log($"[Sync] ✓ UNIFIED sync completed in {syncDuration:F0}ms");

                // Summary
                if (geoData != null && geoData.Success)
                {
                    Log($"[Sync] 📍 Final Location: {geoData.City}, {geoData.Country} ({geoData.CountryCode}) via {geoData.Source.ToUpper()}");
                }

                OnMetadataSyncSuccess?.Invoke();
            }
            catch (Exception ex)
            {
                Log($"[Sync] ✗ Fatal error: {ex.Message}\n{ex.StackTrace}", isError: true);
                OnMetadataSyncFailed?.Invoke(ex.Message);
            }
            finally
            {
                _metadataSyncInProgress = false;
            }
        }
        private bool IsLocationServiceAvailable()
        {
            // Unity location service is available on mobile platforms and some desktops
            // Check both platform and user permission
#if UNITY_EDITOR
            return false; // Editor does not support real location service
#elif UNITY_ANDROID || UNITY_IOS
        return Input.location.isEnabledByUser;
#else
        return false;
#endif
        }
        private void SyncPlayerMetadataInBackground()
        {
            StartCoroutine(SyncPlayerMetadataInBackgroundCoroutine());
        }

        private IEnumerator SyncPlayerMetadataInBackgroundCoroutine()
        {
            var userSession = global::UserSessionManager.Current;
            if (userSession == null)
            {
                Log("Cannot sync metadata: no user session", isWarning: true);
                yield break;
            }

            var task = SyncPlayerMetadataAndGeolocationAsync(userSession);

            while (!task.IsCompleted)
            {
                yield return null;
            }

            if (task.IsFaulted)
            {
                Log($"Background sync failed: {task.Exception?.GetBaseException()?.Message}", isWarning: true);
            }
            else
            {
                LogVerbose("Background sync completed");
            }
        }

        /// <summary>
        /// Call create_or_sync_user RPC with retry logic
        /// </summary>
        private async Task<CreateOrSyncUserResponse> CallCreateOrSyncUserWithRetryAsync(
    global::UserSessionManager.UserSession userSession,
    string deviceId)
        {
            CreateOrSyncUserResponse lastResult = null;

            for (int attempt = 1; attempt <= maxRetryAttempts; attempt++)
            {
                try
                {
                    LogVerbose($"[CreateOrSyncUser] Attempt {attempt}/{maxRetryAttempts} starting...");

                    var result = await CallCreateOrSyncUserRPCAsync(userSession, deviceId);
                    lastResult = result;

                    LogVerbose($"[CreateOrSyncUser] Attempt {attempt} result: " +
                               $"success={result.success}, errorCode={result.errorCode}, error='{result.error}'");

                    if (result.success)
                    {
                        Log($"[CreateOrSyncUser] Success on attempt {attempt}.");
                        return result;
                    }

                    if (IsNonRetryableError(result.errorCode))
                    {
                        Log($"[CreateOrSyncUser] Non-retryable error on attempt {attempt}: " +
                            $"{result.errorCode} - {result.error}", isWarning: true);
                        return result;
                    }

                    Log($"[CreateOrSyncUser] Attempt {attempt}/{maxRetryAttempts} failed with " +
                        $"errorCode={result.errorCode}, error='{result.error}'", isWarning: true);

                    // backoff
                    if (attempt < maxRetryAttempts)
                    {
                        var delay = retryBaseDelaySeconds * Math.Pow(2, attempt - 1);
                        LogVerbose($"[CreateOrSyncUser] Retrying in {delay:F1}s...");
                        await Task.Delay((int)(delay * 1000));
                    }
                }
                catch (Exception ex)
                {
                    Log($"[CreateOrSyncUser] Attempt {attempt}/{maxRetryAttempts} threw exception: {ex}", isWarning: true);

                    if (attempt >= maxRetryAttempts)
                    {
                        return new CreateOrSyncUserResponse
                        {
                            success = false,
                            error = ex.ToString(),
                            errorCode = "EXCEPTION"
                        };
                    }

                    var delay = retryBaseDelaySeconds * Math.Pow(2, attempt - 1);
                    await Task.Delay((int)(delay * 1000));
                }
            }

            // All attempts failed
            var finalError = lastResult != null
                ? $"LastErrorCode={lastResult.errorCode}, LastError='{lastResult.error}'"
                : "No response received";

            Log($"[CreateOrSyncUser] Max retry attempts exceeded. {finalError}", isError: true);

            return new CreateOrSyncUserResponse
            {
                success = false,
                error = "Max retry attempts exceeded. " + finalError,
                errorCode = "MAX_RETRIES_EXCEEDED"
            };
        }

        private async Task<CreateOrSyncUserResponse> CallCreateOrSyncUserRPCAsync(
    global::UserSessionManager.UserSession userSession,
    string deviceId)
        {
            if (_client == null || _session == null)
            {
                return new CreateOrSyncUserResponse
                {
                    success = false,
                    error = "Nakama client or session is null",
                    errorCode = "CLIENT_NOT_INITIALIZED"
                };
            }

            try
            {
                var request = new CreateOrSyncUserRequest
                {
                    username = BuildUsername(userSession),
                    device_id = deviceId,
                    game_id = _gameId,
                    cognito_user_id = userSession.userId,
                    email = userSession.email,
                    first_name = userSession.firstName,
                    last_name = userSession.lastName,
                    role = userSession.role,
                    login_type = userSession.loginType,
                    idp_username = userSession.idpUsername,
                    account_status = userSession.accountStatus,
                    wallet_address = userSession.walletAddress,
                    is_adult = userSession.isAdult.ToString()
                };

                string json = JsonConvert.SerializeObject(request);
                LogVerbose($"[CreateOrSyncUser] RPC payload: {json}");

                IApiRpc rpcResult;
                try
                {
                    rpcResult = await _client.RpcAsync(_session, "create_or_sync_user", json);
                }
                catch (ApiResponseException apiEx)
                {
                    // Nakama server responded with a specific HTTP/gRPC error
                    Log($"[CreateOrSyncUser] ApiResponseException: status={apiEx.StatusCode}, " +
                        $"grpc={apiEx.GrpcStatusCode}, message={apiEx.Message}", isWarning: true);

                    return new CreateOrSyncUserResponse
                    {
                        success = false,
                        error = apiEx.Message,
                        errorCode = $"HTTP_{apiEx.StatusCode}_GRPC_{apiEx.GrpcStatusCode}"
                    };
                }

                LogVerbose($"[CreateOrSyncUser] RPC response: {rpcResult.Payload}");

                var response = JsonConvert.DeserializeObject<CreateOrSyncUserResponse>(rpcResult.Payload);

                if (response == null)
                {
                    return new CreateOrSyncUserResponse
                    {
                        success = false,
                        error = "Failed to deserialize response",
                        errorCode = "DESERIALIZATION_FAILED"
                    };
                }

                return response;
            }
            catch (Exception ex)
            {
                Log($"[CreateOrSyncUser] RPC_EXCEPTION: {ex}", isWarning: true);
                return new CreateOrSyncUserResponse
                {
                    success = false,
                    error = ex.ToString(),
                    errorCode = "RPC_EXCEPTION"
                };
            }
        }



        private async Task<GeolocationData> CaptureGeolocationAsync()
        {
            var result = new GeolocationData
            {
                Success = false,
                CapturedAt = DateTime.UtcNow,
                Source = "none"
            };

            // Check if location services are enabled
            if (!Input.location.isEnabledByUser)
            {
                result.Error = "Location services not enabled by user";
                Log("[Geolocation] Location services not enabled", isWarning: true);
                return result;
            }

            try
            {
                // Check if already running
                if (Input.location.status == LocationServiceStatus.Running)
                {
                    // Use existing location
                    result.Latitude = Input.location.lastData.latitude;
                    result.Longitude = Input.location.lastData.longitude;
                    result.AccuracyMeters = Input.location.lastData.horizontalAccuracy;
                    result.Success = true;
                    result.Source = "gps_cached";

                    LogVerbose($"[Geolocation] Using cached GPS: {result.Latitude:F6}, {result.Longitude:F6}");
                    return result;
                }

                // Start location service
                Log("[Geolocation] Starting location service.. .");
                Input.location.Start(10f, 10f); // desiredAccuracyInMeters, updateDistanceInMeters

                float startTime = Time.realtimeSinceStartup;
                float timeoutSeconds = geoLocationTimeoutSeconds;

                // Wait for initialization
                while (Input.location.status == LocationServiceStatus.Initializing)
                {
                    if (Time.realtimeSinceStartup - startTime > timeoutSeconds)
                    {
                        result.Error = $"GPS initialization timeout after {timeoutSeconds}s";
                        Log($"[Geolocation] {result.Error}", isWarning: true);

                        try { Input.location.Stop(); } catch { }
                        return result;
                    }

                    await Task.Delay(100);
                }

                // Check status
                switch (Input.location.status)
                {
                    case LocationServiceStatus.Running:
                        result.Latitude = Input.location.lastData.latitude;
                        result.Longitude = Input.location.lastData.longitude;
                        result.AccuracyMeters = Input.location.lastData.horizontalAccuracy;
                        result.Success = true;
                        result.Source = "gps";

                        Log($"[Geolocation] ✓ GPS captured: {result.Latitude:F6}, {result.Longitude:F6} (accuracy: {result.AccuracyMeters:F0}m)");
                        break;

                    case LocationServiceStatus.Failed:
                        result.Error = "Location service failed to start";
                        Log($"[Geolocation] {result.Error}", isWarning: true);
                        break;

                    case LocationServiceStatus.Stopped:
                        result.Error = "Location service stopped unexpectedly";
                        Log($"[Geolocation] {result.Error}", isWarning: true);
                        break;

                    default:
                        result.Error = $"Unexpected location status: {Input.location.status}";
                        Log($"[Geolocation] {result.Error}", isWarning: true);
                        break;
                }

                // Stop location service to save battery
                try { Input.location.Stop(); } catch { }
            }
            catch (Exception ex)
            {
                result.Error = $"GPS exception: {ex.Message}";
                Log($"[Geolocation] {result.Error}", isWarning: true);

                try { Input.location.Stop(); } catch { }
            }

            return result;
        }

        private string GetStableDeviceId()
        {
            var id = SystemInfo.deviceUniqueIdentifier;
            if (string.IsNullOrWhiteSpace(id))
            {
                id = "editor-device-" + SystemInfo.deviceName;
            }
            return id;
        }

        #endregion

        #region Authentication Helpers

        private async Task<ISession> AuthenticateWithRetryAsync(string customId, string username)
        {
            // per-attempt timeout (ms) - keeps attempts from hanging indefinitely. Adjustable.
            const int attemptTimeoutMs = 20000;

            for (int attempt = 1; attempt <= maxRetryAttempts; attempt++)
            {
                try
                {
                    Log($"Authentication attempt {attempt}/{maxRetryAttempts}...");

                    var authTask = _client.AuthenticateCustomAsync(
                        id: customId,
                        username: username,
                        create: true);

                    var completed = await Task.WhenAny(authTask, Task.Delay(attemptTimeoutMs));
                    if (completed != authTask)
                    {
                        Log($"Authentication attempt {attempt} timed out after {attemptTimeoutMs}ms", isWarning: true);
                        // fall through to retry/backoff
                    }
                    else
                    {
                        var session = await authTask; // will rethrow if faulted
                        if (session != null)
                        {
                            Log($"✓ Authentication successful (attempt {attempt})");
                            return session;
                        }

                        Log($"Attempt {attempt} returned null session", isWarning: true);
                    }
                }
                catch (ApiResponseException apiEx)
                {
                    // Nakama-specific HTTP/gRPC error — log status codes for diagnosis
                    Log($"Attempt {attempt} ApiResponseException: Status={apiEx.StatusCode}, Grpc={apiEx.GrpcStatusCode}, Message={apiEx.Message}", isWarning: true);
                    LogVerbose($"ApiResponseException stack: {apiEx}");
                }
                catch (Exception ex)
                {
                    Log($"Attempt {attempt} failed: {ex.GetType().Name}: {ex.Message}", isWarning: true);
                    LogVerbose($"Exception stack: {ex}");
                }

                if (attempt < maxRetryAttempts)
                {
                    // exponential backoff with small jitter
                    var baseDelay = retryBaseDelaySeconds * Math.Pow(2, attempt - 1);
                    var jitter = UnityEngine.Random.Range(0f, 0.25f * (float)baseDelay);
                    var delay = (int)((baseDelay + jitter) * 1000);
                    LogVerbose($"Authentication retry backoff: {delay}ms");
                    await Task.Delay(delay);
                }
            }

            Log("Authentication failed after all retry attempts.", isError: true);
            return null;
        }

        private bool ValidateUserSession(global::UserSessionManager.UserSession userSession)
        {
            if (userSession == null)
            {
                Log("UserSession is null", isError: true);
                return false;
            }

            // At minimum, we need either userId, email, or userName
            if (string.IsNullOrWhiteSpace(userSession.userId) &&
                string.IsNullOrWhiteSpace(userSession.email) &&
                string.IsNullOrWhiteSpace(userSession.userName))
            {
                Log("UserSession has no valid identifier (userId, email, userName all empty)", isError: true);
                return false;
            }

            return true;
        }

        private bool IsNonRetryableError(string errorCode)
        {
            if (string.IsNullOrEmpty(errorCode)) return false;

            var nonRetryableCodes = new[]
            {
                "VALIDATION_ERROR",
                "INVALID_JSON",
                "CLIENT_NOT_INITIALIZED"
            };

            return Array.IndexOf(nonRetryableCodes, errorCode) >= 0;
        }

        #endregion

        #region Validation Helpers

        private bool IsValidGuid(string value)
        {
            if (string.IsNullOrWhiteSpace(value)) return false;
            return Guid.TryParse(value, out _);
        }

        #endregion

        #region Wallet Integration (existing code preserved)

        private const string RPC_WALLET_REFRESH = "wallet_get_balances";
        private const string RPC_WALLET_APPLY_DELTA = "wallet_update_game_wallet";

        [Serializable]
        private class WalletDto
        {
            [JsonProperty("balance")] public long Balance;
            [JsonProperty("currency")] public string Currency;
            [JsonProperty("updated_at")] public string UpdatedAt;
        }

        [Serializable]
        private class WalletRefreshResultDto
        {
            [JsonProperty("success")] public bool Success;
            [JsonProperty("error")] public string Error;
            [JsonProperty("game_balance")] public long GameBalance;
            [JsonProperty("global_balance")] public long GlobalBalance;
            [JsonProperty("currencies")] public Dictionary<string, long> Currencies;
            [JsonProperty("game_id")] public string GameId;
            [JsonProperty("gameId")] public string GameIdAlt;
            [JsonProperty("user_id")] public string UserId;
            [JsonProperty("userId")] public string UserIdAlt;
            [JsonProperty("timestamp")] public string Timestamp;
        }

        [Serializable]
        private class WalletOperationResultDto
        {
            [JsonProperty("success")] public bool Success;
            [JsonProperty("error")] public string Error;
            [JsonProperty("game_balance")] public long? GameBalance;
            [JsonProperty("global_balance")] public long? GlobalBalance;
            [JsonProperty("newBalance")] public long? NewBalance;
            [JsonProperty("currency")] public string Currency;
            [JsonProperty("currencies")] public Dictionary<string, long> Currencies;
            [JsonProperty("wallet")] public WalletDto Wallet;
            [JsonProperty("wallet_type")] public string WalletType;
            [JsonProperty("message")] public string Message;
            [JsonProperty("balance")] public long? Balance;
            [JsonProperty("userId")] public string UserId;
            [JsonProperty("gameId")] public string GameId;
            [JsonProperty("timestamp")] public string Timestamp;
        }

        private async Task<IVXNWalletManager.WalletSnapshot> RefreshWalletFromServerAsync(CancellationToken ct)
        {
            bool ok = await EnsureValidSessionAsync();
            if (!ok || _client == null || _session == null)
                throw new Exception("Nakama session not ready for wallet refresh.");

            if (string.IsNullOrWhiteSpace(_gameId))
                throw new Exception("GameId is not configured in IntelliVerseXConfig.");

            var request = new { gameId = _gameId };
            string json = JsonConvert.SerializeObject(request);
            Log($"[Wallet] Calling RPC '{RPC_WALLET_REFRESH}' with payload: {json}");

            var rpc = await _client.RpcAsync(_session, RPC_WALLET_REFRESH, json, retryConfiguration: null, canceller: ct);
            Log($"[Wallet] RPC '{RPC_WALLET_REFRESH}' responded with: {rpc.Payload}");

            var dto = JsonConvert.DeserializeObject<WalletRefreshResultDto>(rpc.Payload);
            if (dto == null)
                throw new Exception("WalletRefreshResultDto deserialization failed.");

            if (!dto.Success)
                throw new Exception(dto.Error ?? "wallet_get_balances failed on server.");

            long gameBalance = 0;
            long globalBalance = 0;

            if (dto.GameBalance > 0 || dto.GlobalBalance > 0)
            {
                gameBalance = dto.GameBalance;
                globalBalance = dto.GlobalBalance;
                Log($"[Wallet] Using direct balance fields.  Game={gameBalance}, Global={globalBalance}");
            }
            else if (dto.Currencies != null && dto.Currencies.Count > 0)
            {
                if (dto.Currencies.TryGetValue("game", out var gBalance))
                    gameBalance = gBalance;
                else if (dto.Currencies.TryGetValue("tokens", out var tBalance))
                    gameBalance = tBalance;

                if (dto.Currencies.TryGetValue("global", out var glBalance))
                    globalBalance = glBalance;
                else if (dto.Currencies.TryGetValue("xut", out var xBalance))
                    globalBalance = xBalance;

                Log($"[Wallet] Extracted from currencies dict. Game={gameBalance}, Global={globalBalance}");
            }
            else
            {
                gameBalance = dto.GameBalance;
                globalBalance = dto.GlobalBalance;
                Log($"[Wallet] Using fallback (possibly zero).  Game={gameBalance}, Global={globalBalance}");
            }

            Log($"[Wallet] Final refresh result: GameBalance={gameBalance}, GlobalBalance={globalBalance}");
            return new IVXNWalletManager.WalletSnapshot(gameBalance, globalBalance);
        }

        private async Task<IVXNWalletManager.WalletSnapshot> ApplyWalletOperationOnServerAsync(
            IVXNWalletManager.WalletOperation op, CancellationToken ct)
        {
            bool ok = await EnsureValidSessionAsync();
            if (!ok || _client == null || _session == null)
                throw new Exception("Nakama session not ready for wallet operation.");

            if (string.IsNullOrWhiteSpace(_gameId))
                throw new Exception("GameId is not configured in IntelliVerseXConfig.");

            var snapshotBefore = IVXNWalletManager.Snapshot;

            long currentBalance;
            string walletCurrency;
            switch (op.Wallet)
            {
                case IVXNWalletManager.WalletKind.Global:
                    walletCurrency = "global";
                    currentBalance = snapshotBefore.GlobalBalance;
                    break;
                case IVXNWalletManager.WalletKind.Game:
                default:
                    walletCurrency = "game";
                    currentBalance = snapshotBefore.GameBalance;
                    break;
            }

            long expectedNewBalance;
            try
            {
                checked { expectedNewBalance = currentBalance + op.Delta; }
            }
            catch (OverflowException ex)
            {
                throw new Exception(
                    $"Wallet balance overflow for {walletCurrency} wallet. " +
                    $"Current={currentBalance}, Delta={op.Delta}", ex);
            }

            if (expectedNewBalance < 0)
            {
                throw new Exception(
                    $"Wallet operation would result in negative {walletCurrency} balance. " +
                    $"Current={currentBalance}, Delta={op.Delta}, New={expectedNewBalance}");
            }

            var amount = Math.Abs(op.Delta);
            var operationKind = op.Delta >= 0 ? "add" : "subtract";

            var request = new
            {
                gameId = _gameId,
                currency = walletCurrency,
                amount = amount,
                operation = operationKind
            };

            string json = JsonConvert.SerializeObject(request);
            Log($"[Wallet] Calling RPC '{RPC_WALLET_APPLY_DELTA}' with payload: {json}");

            var rpc = await _client.RpcAsync(_session, RPC_WALLET_APPLY_DELTA, json, retryConfiguration: null, canceller: ct);
            Log($"[Wallet] RPC '{RPC_WALLET_APPLY_DELTA}' responded with: {rpc.Payload}");

            var dto = JsonConvert.DeserializeObject<WalletOperationResultDto>(rpc.Payload);
            if (dto == null)
                throw new Exception("WalletOperationResultDto deserialization failed.");

            if (!dto.Success)
                throw new Exception(dto.Error ?? "wallet_update_game_wallet failed on server.");

            long updatedGame = snapshotBefore.GameBalance;
            long updatedGlobal = snapshotBefore.GlobalBalance;

            if (dto.GameBalance.HasValue || dto.GlobalBalance.HasValue)
            {
                if (dto.GameBalance.HasValue) updatedGame = dto.GameBalance.Value;
                if (dto.GlobalBalance.HasValue) updatedGlobal = dto.GlobalBalance.Value;
                Log($"[Wallet] Using direct balance fields from response. Game={updatedGame}, Global={updatedGlobal}");
            }
            else if (dto.Currencies != null && dto.Currencies.Count > 0)
            {
                if (dto.Currencies.TryGetValue("game", out var gBalance))
                    updatedGame = gBalance;
                else if (dto.Currencies.TryGetValue("tokens", out var tBalance))
                    updatedGame = tBalance;

                if (dto.Currencies.TryGetValue("global", out var glBalance))
                    updatedGlobal = glBalance;
                else if (dto.Currencies.TryGetValue("xut", out var xBalance))
                    updatedGlobal = xBalance;

                Log($"[Wallet] Extracted from currencies dict. Game={updatedGame}, Global={updatedGlobal}");
            }
            else if (dto.NewBalance.HasValue)
            {
                var newBal = dto.NewBalance.Value;
                var respCurrency = dto.Currency ?? walletCurrency;

                if (string.Equals(respCurrency, "global", StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(respCurrency, "xut", StringComparison.OrdinalIgnoreCase))
                {
                    updatedGlobal = newBal;
                }
                else
                {
                    updatedGame = newBal;
                }

                Log($"[Wallet] Using newBalance field. Currency={respCurrency}, Value={newBal}");
            }
            else if (dto.Wallet != null)
            {
                var walletBalance = dto.Wallet.Balance;
                var wCurrency = dto.Wallet.Currency ?? dto.Currency ?? dto.WalletType ?? walletCurrency;

                if (string.Equals(wCurrency, "global", StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(wCurrency, "xut", StringComparison.OrdinalIgnoreCase))
                {
                    updatedGlobal = walletBalance;
                }
                else
                {
                    updatedGame = walletBalance;
                }

                Log($"[Wallet] Using legacy wallet object. Currency={wCurrency}, Balance={walletBalance}");
            }
            else if (dto.Balance.HasValue)
            {
                var walletBalance = dto.Balance.Value;
                var wCurrency = dto.Currency ?? dto.WalletType ?? walletCurrency;

                if (string.Equals(wCurrency, "global", StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(wCurrency, "xut", StringComparison.OrdinalIgnoreCase))
                {
                    updatedGlobal = walletBalance;
                }
                else
                {
                    updatedGame = walletBalance;
                }

                Log($"[Wallet] Using legacy balance field. Currency={wCurrency}, Balance={walletBalance}");
            }
            else
            {
                Log($"[Wallet] No balance in response, using calculated value: {expectedNewBalance}");

                if (op.Wallet == IVXNWalletManager.WalletKind.Global)
                    updatedGlobal = expectedNewBalance;
                else
                    updatedGame = expectedNewBalance;
            }

            Log($"[Wallet] Final operation result: GameBalance={updatedGame}, GlobalBalance={updatedGlobal}");
            return new IVXNWalletManager.WalletSnapshot(updatedGame, updatedGlobal);
        }

        #endregion

        #region Session Persistence

        private bool ShouldPersistNakama()
        {
            try
            {
                int remember = PlayerPrefs.GetInt(PP_REMEMBER, 1);
                return remember == 1;
            }
            catch
            {
                return true;
            }
        }

        private bool TryRestoreSession(out ISession session)
        {
            session = null;

            if (!ShouldPersistNakama())
            {
                Log("Remember=OFF, skipping Nakama session restore.");
                return false;
            }

            try
            {
                var token = PlayerPrefs.GetString(PREF_NAKAMA_AUTH_TOKEN, string.Empty);
                var refreshToken = PlayerPrefs.GetString(PREF_NAKAMA_REFRESH_TOKEN, string.Empty);

                // Migrate legacy key (common accidental space) — keeps backward compatibility
                if (string.IsNullOrWhiteSpace(token))
                {
                    const string legacyKey = "ivxn. nakama.auth_token";
                    var legacyToken = PlayerPrefs.GetString(legacyKey, string.Empty);
                    if (!string.IsNullOrWhiteSpace(legacyToken))
                    {
                        PlayerPrefs.SetString(PREF_NAKAMA_AUTH_TOKEN, legacyToken);
                        PlayerPrefs.DeleteKey(legacyKey);
                        PlayerPrefs.Save();
                        token = legacyToken;
                        Log("Migrated legacy Nakama auth token key to new key.", isWarning: true);
                    }
                }

                if (string.IsNullOrWhiteSpace(token))
                {
                    Log("No stored Nakama auth token.");
                    return false;
                }

                var restored = global::Nakama.Session.Restore(token, string.IsNullOrEmpty(refreshToken) ? null : refreshToken);
                if (restored == null)
                {
                    Log("Failed to restore session from token.", isWarning: true);
                    return false;
                }

                if (restored.IsExpired)
                {
                    Log("Stored Nakama session is expired.");
                    return false;
                }

                session = restored;
                return true;
            }
            catch (Exception ex)
            {
                Log($"TryRestoreSession error: {ex}", isWarning: true);
                return false;
            }
        }

        private void SaveSession(ISession session)
        {
            if (session == null)
            {
                Log("SaveSession called with null session.", isWarning: true);
                return;
            }

            if (!ShouldPersistNakama())
            {
                try
                {
                    PlayerPrefs.DeleteKey(PREF_NAKAMA_AUTH_TOKEN);
                    PlayerPrefs.DeleteKey(PREF_NAKAMA_REFRESH_TOKEN);
                    PlayerPrefs.Save();
                }
                catch (Exception ex)
                {
                    Log($"SaveSession (Remember=OFF cleanup) PlayerPrefs error: {ex}", isWarning: true);
                }

                Log("Remember=OFF, not persisting Nakama tokens. In-memory session remains active.");
                return;
            }

            try
            {
                PlayerPrefs.SetString(PREF_NAKAMA_AUTH_TOKEN, session.AuthToken);
                PlayerPrefs.SetString(PREF_NAKAMA_REFRESH_TOKEN, session.RefreshToken ?? string.Empty);
                PlayerPrefs.Save();

                var expireAtUtc = UnixToUtcSafe(session.ExpireTime);
                var masked = session.AuthToken != null ? $"len={session.AuthToken.Length}" : "null";
                Log($"Nakama session tokens saved. ExpireUnix={session.ExpireTime}, ExpireAtUTC={expireAtUtc:O}, AuthToken={masked}");
            }
            catch (Exception ex)
            {
                Log($"SaveSession error: {ex}", isError: true);
            }
        }

        private void LogSession(string prefix)
        {
            if (_session == null)
            {
                Log($"{prefix} but _session is null.", isWarning: true);
                return;
            }

            var createdAtUtc = UnixToUtcSafe(_session.CreateTime);
            var expireAtUtc = UnixToUtcSafe(_session.ExpireTime);

            Log($"{prefix}");
            Log($"  UserId:       {_session.UserId}");
            Log($"  Username:     {_session.Username}");
            Log($"  CreatedUnix:  {_session.CreateTime}, CreatedAtUTC: {createdAtUtc:O}");
            Log($"  ExpireUnix:   {_session.ExpireTime}, ExpireAtUTC: {expireAtUtc:O}");
            Log($"  HasExpired:   {_session.IsExpired}");
        }

        #endregion

        #region Helpers
        private static DateTime UnixToUtcSafe(long unixSeconds)
        {
            try
            {
                return DateTimeOffset.FromUnixTimeSeconds(unixSeconds).UtcDateTime;
            }
            catch
            {
                return new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            }
        }

        private static string BuildCustomId(global::UserSessionManager.UserSession user)
        {
            // Priority: userId (Cognito) > email > userName > generated
            if (!string.IsNullOrWhiteSpace(user.userId))
                return user.userId;

            if (!string.IsNullOrWhiteSpace(user.email))
                return user.email;

            if (!string.IsNullOrWhiteSpace(user.userName))
                return user.userName;

            // Fallback: generate deterministic ID from available data
            var fallbackId = $"user_{user.email ?? user.userName ?? Guid.NewGuid().ToString("N")}";
            return fallbackId;
        }

        private static string BuildUsername(global::UserSessionManager.UserSession user)
        {
            // Priority: userName > idpUsername > email prefix > firstName+lastName > fallback
            if (!string.IsNullOrWhiteSpace(user.userName))
                return SanitizeUsername(user.userName);

            if (!string.IsNullOrWhiteSpace(user.idpUsername))
                return SanitizeUsername(user.idpUsername);

            if (!string.IsNullOrWhiteSpace(user.email))
            {
                var at = user.email.IndexOf('@');
                var prefix = at > 0 ? user.email.Substring(0, at) : user.email;
                return SanitizeUsername(prefix);
            }

            if (!string.IsNullOrWhiteSpace(user.firstName))
            {
                var name = user.firstName;
                if (!string.IsNullOrWhiteSpace(user.lastName))
                    name += user.lastName;
                return SanitizeUsername(name);
            }

            if (!string.IsNullOrWhiteSpace(user.userId))
            {
                var shortId = user.userId.Length > 8 ? user.userId.Substring(0, 8) : user.userId;
                return "Player_" + shortId;
            }

            return "Player_" + Guid.NewGuid().ToString("N").Substring(0, 8);
        }

        private static string SanitizeUsername(string username)
        {
            if (string.IsNullOrWhiteSpace(username)) return "Player";

            // Remove invalid characters
            var sanitized = System.Text.RegularExpressions.Regex.Replace(username.Trim(), @"[^a-zA-Z0-9_\-]", "");

            // Limit length
            if (sanitized.Length > 20)
                sanitized = sanitized.Substring(0, 20);

            // Ensure not empty
            return string.IsNullOrEmpty(sanitized) ? "Player" : sanitized;
        }

        private void Log(string message, bool isWarning = false, bool isError = false)
        {
            if (!enableDebugLogs && !isError) return;

            var timestamp = DateTime.Now.ToString("HH:mm:ss. fff");
            var prefix = $"{LOGTAG} [{timestamp}]";

            if (isError)
                Debug.LogError($"{prefix} {message}");
            else if (isWarning)
                Debug.LogWarning($"{prefix} {message}");
            else
                Debug.Log($"{prefix} {message}");
        }

        private void LogVerbose(string message)
        {
            if (enableVerboseLogging)
            {
                Log($"[VERBOSE] {message}");
            }
        }

        private void SafeInvokeOnInitialized(bool success)
        {
            try
            {
                OnInitialized?.Invoke(success);
            }
            catch (Exception ex)
            {
                Log($"Error in OnInitialized: {ex.Message}", isWarning: true);
            }
        }

        private void HandleProfileLoaded(IVXNProfileManager.IVXNProfileSnapshot snapshot)
        {
            try
            {
                OnProfileLoaded?.Invoke(snapshot);
            }
            catch (Exception ex)
            {
                Log($"Error in OnProfileLoaded handler: {ex.Message}", isWarning: true);
            }
        }

        private void HandleProfileUpdated(IVXNProfileManager.IVXNProfileSnapshot snapshot)
        {
            try
            {
                OnProfileUpdated?.Invoke(snapshot);
            }
            catch (Exception ex)
            {
                Log($"Error in OnProfileUpdated handler: {ex.Message}", isWarning: true);
            }
        }

        private void HandleProfileError(string errorMessage)
        {
            try
            {
                OnProfileError?.Invoke(errorMessage);
            }
            catch (Exception ex)
            {
                Log($"Error in OnProfileError handler: {ex.Message}", isWarning: true);
            }
        }


        #endregion


        #region Player Metadata Storage (rpc_update_player_metadata - UNIFIED)

        // ============================================================================
        // DATA CLASSES
        // ============================================================================

        [Serializable]
        private class PlayerMetadataRequest
        {
            // Identity
            public string role;
            public string email;
            public string game_id;
            public string is_adult;
            public string is_guest;  // ADD THIS LINE
            public string last_name;
            public string first_name;
            public string login_type;
            public string idp_username;
            public string account_status;
            public string wallet_address;
            public string cognito_user_id;

            // Geolocation (from GPS or IP)
            public string geo_location;      // Country code (e.g., "IN")
            public double? latitude;
            public double? longitude;
            public string country;           // "India"
            public string country_code;      // "IN"
            public string region;            // "Karnataka"
            public string city;              // "Bangalore"
            public string location_timezone; // "Asia/Kolkata"
            public string location_source;   // ADD THIS LINE - "gps" or "ip"

            // Device Info
            public string device_id;
            public string platform;
            public string device_model;
            public string device_name;
            public string os_version;
            public string app_version;
            public string unity_version;
            public string locale;
            public string timezone;
            public int screen_width;
            public int screen_height;
            public string screen_dpi;
            public string graphics_device;
            public int system_memory_mb;
            public string processor_type;
            public int processor_count;
        }

        [Serializable]
        private class PlayerMetadataResponse
        {
            public bool success;
            public string error;
            public string error_code;
            public string request_id;
            public bool is_new_user;
            public int execution_time_ms;
            public PlayerMetadataMetadata metadata;
            public StorageInfo storage;

            [Serializable]
            public class StorageInfo
            {
                public string collection;
                public string key;
                public string permission_read;
                public string permission_write;
            }

            [Serializable]
            public class PlayerMetadataMetadata
            {
                // Identity
                public string user_id;
                public string role;
                public string email;
                public string game_id;
                public string current_game_id;
                public string is_adult;
                public string last_name;
                public string first_name;
                public string login_type;
                public string idp_username;
                public string account_status;
                public string wallet_address;
                public string cognito_user_id;
                public string nakama_username;

                // Geolocation
                public string geo_location;
                public double? latitude;
                public double? longitude;
                public string country;
                public string country_code;
                public string region;
                public string city;
                public string timezone;
                public string location_source;
                public string location_updated_at;

                // Device
                public string device_id;
                public string current_device_id;
                public string platform;
                public string device_model;
                public string os_version;
                public string app_version;
                public string locale;
                public int total_devices;

                // Games
                public List<GameEntry> games;
                public int total_games;
                public int total_sessions;
                public string last_game_played_at;

                // Analytics
                public AnalyticsData analytics;

                // Timestamps
                public string created_at;
                public string updated_at;
                public string first_seen_at;
            }

            [Serializable]
            public class GameEntry
            {
                public string game_id;
                public string first_played;
                public string last_played;
                public int play_count;
                public int session_count;
                public long total_playtime_seconds;
            }

            [Serializable]
            public class AnalyticsData
            {
                public string first_session;
                public string last_session;
                public int total_sessions;
                public int days_active;
                public int current_streak;
                public int longest_streak;
                public string last_active_date;
                public int days_since_first_session;
                public double average_sessions_per_day;
            }
        }

        [Serializable]
        private class GeolocationPayload
        {
            public double latitude;
            public double longitude;
        }

        [Serializable]
        private class GeolocationResponse
        {
            public bool success;
            public bool allowed;
            public string country;
            public string country_code;
            public string region;
            public string city;
            public string reason;
            public double? latitude;
            public double? longitude;
            public string error;
        }

        #endregion



        #region Device Info Collection

        /// <summary>
        /// Collect comprehensive device information
        /// </summary>
        private DeviceInfoData CollectDeviceInfo()
        {
            var info = new DeviceInfoData();

            try
            {
                // Device ID
                info.DeviceId = GetStableDeviceId();

                // Platform
                info.Platform = GetPlatformString();

                // Device Model
                info.DeviceModel = GetDeviceModel();

                // Device Name
                info.DeviceName = GetDeviceName();

                // OS Version
                info.OsVersion = GetOsVersion();

                // App Version
                info.AppVersion = GetAppVersion();

                // Unity Version
                info.UnityVersion = Application.unityVersion ?? "Unknown";

                // Locale
                info.Locale = GetLocale();

                // Timezone
                info.Timezone = GetTimezone();

                // Screen Info
                info.ScreenWidth = Screen.width;
                info.ScreenHeight = Screen.height;
                info.ScreenDpi = Screen.dpi > 0 ? Screen.dpi.ToString("F0") : "Unknown";

                // Graphics
                info.GraphicsDevice = GetGraphicsDevice();

                // Memory
                info.SystemMemoryMb = SystemInfo.systemMemorySize;

                // Processor
                info.ProcessorType = GetProcessorType();
                info.ProcessorCount = SystemInfo.processorCount;
            }
            catch (Exception ex)
            {
                Log($"[DeviceInfo] Error collecting device info: {ex.Message}", isWarning: true);
            }

            return info;
        }

        /// <summary>
        /// Get platform string (clean format)
        /// </summary>
        private string GetPlatformString()
        {
            try
            {
                switch (Application.platform)
                {
                    case RuntimePlatform.Android:
                        return "Android";
                    case RuntimePlatform.IPhonePlayer:
                        return "iOS";
                    case RuntimePlatform.WindowsPlayer:
                        return "Windows";
                    case RuntimePlatform.WindowsEditor:
                        return "Windows (Editor)";
                    case RuntimePlatform.OSXPlayer:
                        return "macOS";
                    case RuntimePlatform.OSXEditor:
                        return "macOS (Editor)";
                    case RuntimePlatform.LinuxPlayer:
                        return "Linux";
                    case RuntimePlatform.LinuxEditor:
                        return "Linux (Editor)";
                    case RuntimePlatform.WebGLPlayer:
                        return "WebGL";
                    case RuntimePlatform.tvOS:
                        return "tvOS";
                    case RuntimePlatform.PS4:
                        return "PlayStation 4";
                    case RuntimePlatform.PS5:
                        return "PlayStation 5";
                    case RuntimePlatform.XboxOne:
                        return "Xbox One";
                    case RuntimePlatform.GameCoreXboxOne:
                    case RuntimePlatform.GameCoreXboxSeries:
                        return "Xbox Series";
                    case RuntimePlatform.Switch:
                        return "Nintendo Switch";
                    default:
                        return Application.platform.ToString();
                }
            }
            catch
            {
                return "Unknown";
            }
        }

        /// <summary>
        /// Get device model with cleanup
        /// </summary>
        private string GetDeviceModel()
        {
            try
            {
                var model = SystemInfo.deviceModel;

                if (string.IsNullOrWhiteSpace(model))
                    return "Unknown";

                // Clean up common prefixes for better readability
                model = model.Trim();

                // Samsung devices often have "SAMSUNG-" prefix
                if (model.StartsWith("SAMSUNG-", StringComparison.OrdinalIgnoreCase))
                    model = "Samsung " + model.Substring(8);
                else if (model.StartsWith("samsung ", StringComparison.OrdinalIgnoreCase))
                    model = "Samsung " + model.Substring(8);

                // Xiaomi devices - keep Xiaomi prefix, add to Redmi
                if (model.StartsWith("Redmi ", StringComparison.OrdinalIgnoreCase))
                    model = "Xiaomi " + model;
                // Xiaomi devices already have the prefix, no change needed

                // Truncate if too long
                if (model.Length > 100)
                    model = model.Substring(0, 100);

                return model;
            }
            catch
            {
                return "Unknown";
            }
        }

        /// <summary>
        /// Get device name (user-assigned name)
        /// </summary>
        private string GetDeviceName()
        {
            try
            {
                var name = SystemInfo.deviceName;

                if (string.IsNullOrWhiteSpace(name))
                    return "Unknown";

                // Truncate if too long
                if (name.Length > 100)
                    name = name.Substring(0, 100);

                return name.Trim();
            }
            catch
            {
                return "Unknown";
            }
        }

        /// <summary>
        /// Get OS version with platform context
        /// </summary>
        private string GetOsVersion()
        {
            try
            {
                var os = SystemInfo.operatingSystem;

                if (string.IsNullOrWhiteSpace(os))
                    return "Unknown";

                // Truncate if too long
                if (os.Length > 100)
                    os = os.Substring(0, 100);

                return os.Trim();
            }
            catch
            {
                return "Unknown";
            }
        }

        /// <summary>
        /// Get application version
        /// </summary>
        private string GetAppVersion()
        {
            try
            {
                var version = Application.version;

                if (string.IsNullOrWhiteSpace(version))
                    return "1.0.0";

                return version.Trim();
            }
            catch
            {
                return "1.0.0";
            }
        }

        /// <summary>
        /// Get locale/culture string
        /// </summary>
        private string GetLocale()
        {
            try
            {
                // Try to get system language first
                var lang = Application.systemLanguage;

                // Try to get culture info
                try
                {
                    var culture = System.Globalization.CultureInfo.CurrentCulture;
                    if (culture != null && !string.IsNullOrEmpty(culture.Name))
                    {
                        return culture.Name; // e.g., "en-US", "ja-JP"
                    }
                }
                catch { }

                // Fallback to Unity's system language
                return lang.ToString();
            }
            catch
            {
                return "en-US";
            }
        }

        /// <summary>
        /// Get timezone identifier
        /// </summary>
        private string GetTimezone()
        {
            try
            {
                // Try to get timezone ID
                var tz = TimeZoneInfo.Local;
                if (tz != null)
                {
                    // Prefer standard name or ID
                    if (!string.IsNullOrEmpty(tz.Id))
                        return tz.Id;

                    if (!string.IsNullOrEmpty(tz.StandardName))
                        return tz.StandardName;
                }
            }
            catch { }

            try
            {
                // Fallback: Use UTC offset
                var offset = TimeZoneInfo.Local.GetUtcOffset(DateTime.Now);
                var sign = offset >= TimeSpan.Zero ? "+" : "-";
                return $"UTC{sign}{Math.Abs(offset.Hours):D2}:{Math.Abs(offset.Minutes):D2}";
            }
            catch
            {
                return "UTC";
            }
        }

        /// <summary>
        /// Get graphics device info
        /// </summary>
        private string GetGraphicsDevice()
        {
            try
            {
                var gpu = SystemInfo.graphicsDeviceName;

                if (string.IsNullOrWhiteSpace(gpu))
                    return "Unknown";

                // Truncate if too long
                if (gpu.Length > 150)
                    gpu = gpu.Substring(0, 150);

                return gpu.Trim();
            }
            catch
            {
                return "Unknown";
            }
        }

        /// <summary>
        /// Get processor type info
        /// </summary>
        private string GetProcessorType()
        {
            try
            {
                var cpu = SystemInfo.processorType;

                if (string.IsNullOrWhiteSpace(cpu))
                    return "Unknown";

                // Truncate if too long
                if (cpu.Length > 150)
                    cpu = cpu.Substring(0, 150);

                return cpu.Trim();
            }
            catch
            {
                return "Unknown";
            }
        }

        #endregion

        #region Updated Geo RPC Method

        /// <summary>
        /// Call geo RPC to get reverse geocoded location and update profile
        /// Returns enhanced GeolocationData with country, region, city
        /// </summary>
        private async Task<GeolocationData> CallCheckGeoAndUpdateProfileRPCAsync(GeolocationData geoData)
        {
            if (_client == null || _session == null)
            {
                Log("[Geolocation] Cannot call geo RPC: client or session is null", isWarning: true);
                return geoData;
            }

            if (geoData == null || !geoData.Success)
            {
                Log("[Geolocation] Cannot call geo RPC: no valid geo data", isWarning: true);
                return geoData;
            }

            try
            {
                var payload = new GeolocationPayload
                {
                    latitude = geoData.Latitude,
                    longitude = geoData.Longitude
                };

                string json = JsonConvert.SerializeObject(payload);
                LogVerbose($"[Geolocation] RPC 'check_geo_and_update_profile' payload: {json}");

                IApiRpc rpcResult;
                try
                {
                    rpcResult = await _client.RpcAsync(_session, "check_geo_and_update_profile", json);
                }
                catch (ApiResponseException apiEx)
                {
                    Log($"[Geolocation] API error: status={apiEx.StatusCode}, message={apiEx.Message}", isWarning: true);
                    return geoData;
                }

                LogVerbose($"[Geolocation] RPC response: {rpcResult.Payload}");

                var response = JsonConvert.DeserializeObject<GeolocationResponse>(rpcResult.Payload);

                if (response == null)
                {
                    Log("[Geolocation] Failed to deserialize geo response", isWarning: true);
                    return geoData;
                }

                // Update geoData with reverse geocoded info
                geoData.Country = response.country;
                geoData.CountryCode = response.country_code;
                geoData.Region = response.region;
                geoData.City = response.city;
                geoData.IsAllowed = response.allowed;
                geoData.BlockReason = response.reason;

                if (response.allowed)
                {
                    Log($"[Geolocation] ✓ Location resolved: {response.city}, {response.region}, {response.country} ({response.country_code})");
                }
                else
                {
                    Log($"[Geolocation] ⚠ Region blocked: {response.reason}", isWarning: true);
                }

                return geoData;
            }
            catch (Exception ex)
            {
                Log($"[Geolocation] RPC exception: {ex.Message}", isWarning: true);
                return geoData;
            }
        }

        #endregion

        /// <summary>
        /// Build comprehensive payload for rpc_update_player_metadata
        /// Includes identity, geolocation, and device information
        /// </summary>
        private PlayerMetadataRequest BuildPlayerMetadataRequest(
            global::UserSessionManager.UserSession userSession,
            string deviceId,
            GeolocationData geoData = null,
            DeviceInfoData deviceInfo = null)
        {
            if (userSession == null)
                throw new ArgumentNullException(nameof(userSession));

            if (deviceInfo == null)
            {
                deviceInfo = CollectDeviceInfo();
            }

            var req = new PlayerMetadataRequest
            {
                // Identity
                role = SanitizeField(userSession.role, "user", 50),
                email = SanitizeField(userSession.email, "", 254),
                game_id = _gameId,
                is_adult = userSession.isAdult ? "True" : "False",
                is_guest = userSession.isGuest ? "True" : "False",  // ADD THIS LINE
                last_name = SanitizeField(userSession.lastName, "", 100),
                first_name = SanitizeField(userSession.firstName, "", 100),
                login_type = SanitizeField(userSession.loginType, "device", 50),
                idp_username = SanitizeField(userSession.idpUsername, "", 256),
                account_status = SanitizeField(userSession.accountStatus, "active", 50),
                wallet_address = SanitizeField(userSession.walletAddress, "", 256),
                cognito_user_id = SanitizeField(userSession.userId, "", 128),

                // Device Info
                device_id = deviceInfo.DeviceId,
                platform = deviceInfo.Platform,
                device_model = deviceInfo.DeviceModel,
                device_name = deviceInfo.DeviceName,
                os_version = deviceInfo.OsVersion,
                app_version = deviceInfo.AppVersion,
                unity_version = deviceInfo.UnityVersion,
                locale = deviceInfo.Locale,
                timezone = deviceInfo.Timezone,
                screen_width = deviceInfo.ScreenWidth,
                screen_height = deviceInfo.ScreenHeight,
                screen_dpi = deviceInfo.ScreenDpi,
                graphics_device = deviceInfo.GraphicsDevice,
                system_memory_mb = deviceInfo.SystemMemoryMb,
                processor_type = deviceInfo.ProcessorType,
                processor_count = deviceInfo.ProcessorCount
            };

            // Geolocation - include ALL available data from IP or GPS
            if (geoData != null && geoData.Success)
            {
                req.latitude = geoData.Latitude;
                req.longitude = geoData.Longitude;
                req.geo_location = geoData.CountryCode ?? "";
                req.country = geoData.Country ?? "";
                req.country_code = geoData.CountryCode ?? "";
                req.region = geoData.Region ?? "";
                req.city = geoData.City ?? "";
                req.location_timezone = geoData.Timezone ?? "";
                req.location_source = geoData.Source ?? "unknown";  // ADD THIS LINE

                Log($"[BuildRequest] ✓ Geo included: {geoData.City}, {geoData.Region}, {geoData.Country} ({geoData.CountryCode}) via {geoData.Source}");
                Log($"[BuildRequest] ✓ Coordinates: lat={geoData.Latitude:F6}, lon={geoData.Longitude:F6}");
            }
            else
            {
                req.latitude = null;
                req.longitude = null;
                req.geo_location = "";
                req.country = "";
                req.country_code = "";
                req.region = "";
                req.city = "";
                req.location_timezone = "";
                req.location_source = "";

                Log("[BuildRequest] ⚠ No geo data available", isWarning: true);
            }

            return req;
        }

        /// <summary>
        /// Sanitize and truncate field value
        /// </summary>
        private string SanitizeField(string value, string defaultValue, int maxLength)
        {
            if (string.IsNullOrWhiteSpace(value))
                return defaultValue;

            var sanitized = value.Trim();

            // Remove control characters
            sanitized = System.Text.RegularExpressions.Regex.Replace(sanitized, @"[\x00-\x1F\x7F]", "");

            // Truncate if too long
            if (sanitized.Length > maxLength)
                sanitized = sanitized.Substring(0, maxLength);

            return sanitized;
        }

        /// <summary>
        /// Single call to rpc_update_player_metadata.
        /// Uses robust error handling and never throws to caller; returns a structured result.
        /// </summary>
        /// <summary>
        /// Call rpc_update_player_metadata with comprehensive data
        /// </summary>
        private async Task<PlayerMetadataResponse> CallUpdatePlayerMetadataRPCAsync(
            global::UserSessionManager.UserSession userSession,
            DeviceInfoData deviceInfo,
            GeolocationData geoData)
        {
            if (_client == null || _session == null)
            {
                return new PlayerMetadataResponse
                {
                    success = false,
                    error = "Nakama client or session is null",
                    error_code = "CLIENT_NOT_INITIALIZED"
                };
            }

            try
            {
                var request = BuildPlayerMetadataRequest(userSession, deviceInfo.DeviceId, geoData, deviceInfo);
                var json = JsonConvert.SerializeObject(request, new JsonSerializerSettings
                {
                    NullValueHandling = NullValueHandling.Ignore
                });

                LogVerbose($"[PlayerMetadata] RPC payload: {json}");

                IApiRpc rpcResult;
                try
                {
                    rpcResult = await _client.RpcAsync(_session, "rpc_update_player_metadata", json);
                }
                catch (ApiResponseException apiEx)
                {
                    Log($"[PlayerMetadata] API error: status={apiEx.StatusCode}, grpc={apiEx.GrpcStatusCode}, message={apiEx.Message}", isWarning: true);

                    return new PlayerMetadataResponse
                    {
                        success = false,
                        error = apiEx.Message,
                        error_code = $"HTTP_{apiEx.StatusCode}"
                    };
                }

                LogVerbose($"[PlayerMetadata] RPC response: {rpcResult.Payload}");

                PlayerMetadataResponse response;
                try
                {
                    response = JsonConvert.DeserializeObject<PlayerMetadataResponse>(rpcResult.Payload);
                }
                catch (Exception desEx)
                {
                    Log($"[PlayerMetadata] Deserialization failed: {desEx.Message}", isWarning: true);
                    return new PlayerMetadataResponse
                    {
                        success = false,
                        error = "Failed to deserialize response",
                        error_code = "DESERIALIZATION_ERROR"
                    };
                }

                return response ?? new PlayerMetadataResponse
                {
                    success = false,
                    error = "Empty response",
                    error_code = "EMPTY_RESPONSE"
                };
            }
            catch (Exception ex)
            {
                Log($"[PlayerMetadata] Exception: {ex.Message}", isWarning: true);
                return new PlayerMetadataResponse
                {
                    success = false,
                    error = ex.Message,
                    error_code = "EXCEPTION"
                };
            }
        }

        /// <summary>
        /// Retry wrapper for rpc_update_player_metadata
        /// </summary>
        private async Task<PlayerMetadataResponse> CallUpdatePlayerMetadataWithRetryAsync(
            global::UserSessionManager.UserSession userSession,
            DeviceInfoData deviceInfo,
            GeolocationData geoData)
        {
            PlayerMetadataResponse lastResult = null;

            for (int attempt = 1; attempt <= maxRetryAttempts; attempt++)
            {
                try
                {
                    LogVerbose($"[PlayerMetadata] Attempt {attempt}/{maxRetryAttempts}.. .");

                    var result = await CallUpdatePlayerMetadataRPCAsync(userSession, deviceInfo, geoData);
                    lastResult = result;

                    if (result.success)
                    {
                        if (attempt > 1)
                        {
                            Log($"[PlayerMetadata] ✓ Success on attempt {attempt}");
                        }
                        return result;
                    }

                    // Non-retryable errors
                    if (IsNonRetryableMetadataError(result.error_code))
                    {
                        Log($"[PlayerMetadata] Non-retryable error: {result.error_code}", isWarning: true);
                        return result;
                    }

                    Log($"[PlayerMetadata] Attempt {attempt} failed: {result.error}", isWarning: true);

                    if (attempt < maxRetryAttempts)
                    {
                        var delay = retryBaseDelaySeconds * Math.Pow(2, attempt - 1);
                        LogVerbose($"[PlayerMetadata] Retrying in {delay:F1}s...");
                        await Task.Delay((int)(delay * 1000));
                    }
                }
                catch (Exception ex)
                {
                    Log($"[PlayerMetadata] Attempt {attempt} exception: {ex.Message}", isWarning: true);

                    if (attempt >= maxRetryAttempts)
                    {
                        return new PlayerMetadataResponse
                        {
                            success = false,
                            error = ex.Message,
                            error_code = "EXCEPTION"
                        };
                    }

                    var delay = retryBaseDelaySeconds * Math.Pow(2, attempt - 1);
                    await Task.Delay((int)(delay * 1000));
                }
            }

            return new PlayerMetadataResponse
            {
                success = false,
                error = $"Max retries exceeded.  Last error: {lastResult?.error ?? "unknown"}",
                error_code = "MAX_RETRIES_EXCEEDED"
            };
        }

        /// <summary>
        /// Check if error code is non-retryable for metadata operations
        /// </summary>
        private bool IsNonRetryableMetadataError(string errorCode)
        {
            if (string.IsNullOrEmpty(errorCode)) return false;

            var nonRetryableCodes = new[]
            {
        "CLIENT_NOT_INITIALIZED",
        "INVALID_JSON",
        "VALIDATION_ERROR",
        "AUTH_REQUIRED"
    };

            return Array.IndexOf(nonRetryableCodes, errorCode) >= 0;
        }



        #region Public Geolocation & Device API

        /// <summary>
        /// Get cached geolocation data (may be null if not captured)
        /// </summary>
        public GeolocationInfo GetCachedGeolocation()
        {
            if (_cachedGeoData == null || !_cachedGeoData.Success)
                return null;

            return new GeolocationInfo
            {
                Latitude = _cachedGeoData.Latitude,
                Longitude = _cachedGeoData.Longitude,
                Country = _cachedGeoData.Country,
                CountryCode = _cachedGeoData.CountryCode,
                Region = _cachedGeoData.Region,
                City = _cachedGeoData.City,
                Timezone = _cachedGeoData.Timezone,
                IsAllowed = _cachedGeoData.IsAllowed,
                BlockReason = _cachedGeoData.BlockReason,
                CapturedAt = _cachedGeoData.CapturedAt
            };
        }

        /// <summary>
        /// Get current device information
        /// </summary>
        public DeviceInfo GetDeviceInfo()
        {
            var info = CollectDeviceInfo();

            return new DeviceInfo
            {
                DeviceId = info.DeviceId,
                Platform = info.Platform,
                DeviceModel = info.DeviceModel,
                OsVersion = info.OsVersion,
                AppVersion = info.AppVersion,
                Locale = info.Locale,
                Timezone = info.Timezone,
                ScreenResolution = $"{info.ScreenWidth}x{info.ScreenHeight}",
                SystemMemoryMb = info.SystemMemoryMb
            };
        }

        /// <summary>
        /// Force refresh geolocation and sync to server
        /// </summary>
        public async Task<GeolocationInfo> RefreshGeolocationAsync()
        {
            if (!_isInitialized || _session == null)
            {
                Log("[Geolocation] Cannot refresh: not initialized", isWarning: true);
                return null;
            }

            try
            {
                // Capture new geolocation
                var geoData = await CaptureGeolocationAsync();

                if (geoData == null || !geoData.Success)
                {
                    Log($"[Geolocation] Capture failed: {geoData?.Error ?? "unknown"}", isWarning: true);
                    return null;
                }

                // Resolve via server RPC
                geoData = await CallCheckGeoAndUpdateProfileRPCAsync(geoData);

                // Update cache
                _cachedGeoData = geoData;
                _geoLocationCaptured = true;

                return GetCachedGeolocation();
            }
            catch (Exception ex)
            {
                Log($"[Geolocation] Refresh failed: {ex.Message}", isError: true);
                return null;
            }
        }

        /// <summary>
        /// Check if current location is allowed (not in blocked region)
        /// </summary>
        public bool IsLocationAllowed()
        {
            if (_cachedGeoData == null)
                return true; // Assume allowed if no data

            return _cachedGeoData.IsAllowed;
        }

        /// <summary>
        /// Get location block reason (null if allowed)
        /// </summary>
        public string GetLocationBlockReason()
        {
            if (_cachedGeoData == null || _cachedGeoData.IsAllowed)
                return null;

            return _cachedGeoData.BlockReason;
        }

        /// <summary>
        /// Public data structure for geolocation info
        /// </summary>
        [Serializable]
        public class GeolocationInfo
        {
            public double Latitude;
            public double Longitude;
            public string Country;
            public string CountryCode;
            public string Region;
            public string City;
            public string Timezone;
            public bool IsAllowed;
            public string BlockReason;
            public DateTime CapturedAt;
        }

        /// <summary>
        /// Public data structure for device info
        /// </summary>
        [Serializable]
        public class DeviceInfo
        {
            public string DeviceId;
            public string Platform;
            public string DeviceModel;
            public string OsVersion;
            public string AppVersion;
            public string Locale;
            public string Timezone;
            public string ScreenResolution;
            public int SystemMemoryMb;
        }

        #endregion

        #region IP Geolocation - Production Ready with Multiple Fallbacks

        /// <summary>
        /// Get geolocation from IP address with multiple HTTPS fallbacks
        /// Priority: ipapi.co (HTTPS) → ipinfo.io (HTTPS) → ip-api.com (HTTP fallback)
        /// Production-ready with comprehensive error handling and logging
        /// </summary>
        private async Task<GeolocationData> GetGeolocationFromIPAsync()
        {
            var result = new GeolocationData
            {
                Success = false,
                CapturedAt = DateTime.UtcNow,
                Source = "ip"
            };

            // Define endpoints with priority (HTTPS first, HTTP as last resort)
            var endpoints = new GeoEndpoint[]
            {
        // Primary: ipapi.co - Free tier: 1000 requests/day, HTTPS supported
        new GeoEndpoint
        {
            Url = "https://ipapi.co/json/",
            Provider = "ipapi.co",
            TimeoutSeconds = 10,
            IsHttps = true
        },
        
        // Secondary: ipinfo.io - Free tier: 50k requests/month, HTTPS supported
        new GeoEndpoint
        {
            Url = "https://ipinfo.io/json",
            Provider = "ipinfo.io",
            TimeoutSeconds = 10,
            IsHttps = true
        },
        
        // Tertiary: ipwhois.app - Free tier: 10k requests/month, HTTPS supported
        new GeoEndpoint
        {
            Url = "https://ipwhois.app/json/",
            Provider = "ipwhois.app",
            TimeoutSeconds = 10,
            IsHttps = true
        },
        
        // Quaternary: ipdata.co - Free tier: 1500 requests/day, HTTPS supported (no API key needed for basic)
        new GeoEndpoint
        {
            Url = "https://ipapi.co/json/",
            Provider = "ipapi.co",
            TimeoutSeconds = 10,
            IsHttps = true
        },
        
        // Last resort: ip-api.com - Using HTTPS Pro endpoint (requires API key for production)
        // Note: Free tier only supports HTTP, use ipapi.co or ipinfo.io for free HTTPS
        new GeoEndpoint
        {
            Url = "https://ipapi.co/json/",  // Free HTTPS alternative
            Provider = "ipapi.co",
            TimeoutSeconds = 10,
            IsHttps = true
        }
            };

            int attemptCount = 0;
            int maxAttempts = endpoints.Length;

            foreach (var endpoint in endpoints)
            {
                attemptCount++;

                try
                {
                    Log($"[Geolocation] 🌐 Attempt {attemptCount}/{maxAttempts}: Trying {endpoint.Provider} ({(endpoint.IsHttps ? "HTTPS" : "HTTP")}).. .");

                    using (var request = UnityEngine.Networking.UnityWebRequest.Get(endpoint.Url))
                    {
                        request.timeout = endpoint.TimeoutSeconds;

                        // Add User-Agent header (some APIs require this)
                        request.SetRequestHeader("User-Agent", $"IntelliVerseX-Unity/{Application.version}");
                        request.SetRequestHeader("Accept", "application/json");

                        var operation = request.SendWebRequest();

                        // Wait for completion with timeout
                        float startTime = Time.realtimeSinceStartup;
                        float timeout = endpoint.TimeoutSeconds;

                        while (!operation.isDone)
                        {
                            if (Time.realtimeSinceStartup - startTime > timeout)
                            {
                                Log($"[Geolocation] ⚠ {endpoint.Provider} timed out after {timeout}s", isWarning: true);
                                break;
                            }
                            await Task.Delay(50);
                        }

                        // Check if request completed successfully
                        if (!operation.isDone)
                        {
                            Log($"[Geolocation] ⚠ {endpoint.Provider} request did not complete", isWarning: true);
                            continue; // Try next endpoint
                        }

                        if (request.result != UnityEngine.Networking.UnityWebRequest.Result.Success)
                        {
                            Log($"[Geolocation] ⚠ {endpoint.Provider} failed: {request.error} (HTTP {request.responseCode})", isWarning: true);
                            continue; // Try next endpoint
                        }

                        // Get response body
                        var json = request.downloadHandler.text;

                        if (string.IsNullOrWhiteSpace(json))
                        {
                            Log($"[Geolocation] ⚠ {endpoint.Provider} returned empty response", isWarning: true);
                            continue;
                        }

                        LogVerbose($"[Geolocation] {endpoint.Provider} response: {json}");

                        // Parse response based on provider
                        bool parseSuccess = ParseGeoResponse(json, endpoint.Provider, result);

                        if (parseSuccess && result.Success)
                        {
                            // Validate coordinates
                            if (result.Latitude < -90 || result.Latitude > 90 ||
                                result.Longitude < -180 || result.Longitude > 180)
                            {
                                Log($"[Geolocation] ⚠ {endpoint.Provider} returned invalid coordinates: {result.Latitude}, {result.Longitude}", isWarning: true);
                                result.Success = false;
                                continue;
                            }

                            Log($"[Geolocation] ✓ Success with {endpoint.Provider}:");
                            Log($"[Geolocation]   📍 Location: {result.City}, {result.Region}, {result.Country} ({result.CountryCode})");
                            Log($"[Geolocation]   🌐 Coordinates: {result.Latitude:F6}, {result.Longitude:F6}");
                            Log($"[Geolocation]   🕐 Timezone: {result.Timezone}");
                            Log($"[Geolocation]   📶 Provider: {endpoint.Provider} ({(endpoint.IsHttps ? "HTTPS" : "HTTP")})");

                            return result;
                        }
                        else
                        {
                            Log($"[Geolocation] ⚠ {endpoint.Provider} response parsing failed", isWarning: true);
                            continue;
                        }
                    }
                }
                catch (Exception ex)
                {
                    Log($"[Geolocation] ⚠ {endpoint.Provider} exception: {ex.Message}", isWarning: true);
                    LogVerbose($"[Geolocation] {endpoint.Provider} stack trace: {ex.StackTrace}");
                    continue; // Try next endpoint
                }
            }

            // All endpoints failed
            result.Success = false;
            result.Error = $"All {maxAttempts} IP geolocation services failed";
            Log($"[Geolocation] ✗ {result.Error}", isWarning: true);

            return result;
        }

        /// <summary>
        /// Parse geolocation response from different providers
        /// </summary>
        private bool ParseGeoResponse(string json, string provider, GeolocationData result)
        {
            if (string.IsNullOrWhiteSpace(json))
            {
                return false;
            }

            try
            {
                switch (provider.ToLowerInvariant())
                {
                    case "ipapi.co":
                        return ParseIpApiCoResponse(json, result);

                    case "ipinfo.io":
                        return ParseIpInfoResponse(json, result);

                    case "ipwhois.app":
                        return ParseIpWhoisResponse(json, result);

                    case "ipdata.co":
                        return ParseIpDataResponse(json, result);

                    case "ip-api.com":
                        return ParseIpApiComResponse(json, result);

                    default:
                        Log($"[Geolocation] Unknown provider: {provider}", isWarning: true);
                        return false;
                }
            }
            catch (Exception ex)
            {
                Log($"[Geolocation] Parse error for {provider}: {ex.Message}", isWarning: true);
                return false;
            }
        }

        /// <summary>
        /// Parse ipapi.co response
        /// </summary>
        private bool ParseIpApiCoResponse(string json, GeolocationData result)
        {
            var response = JsonConvert.DeserializeObject<IpApiCoResponse>(json);

            if (response == null)
                return false;

            // Check for errors
            if (!string.IsNullOrEmpty(response.error))
            {
                Log($"[Geolocation] ipapi.co error: {response.error} - {response.reason}", isWarning: true);
                return false;
            }

            // Check for reserved/private IPs
            if (response.reserved)
            {
                Log("[Geolocation] ipapi.co: IP is reserved/private", isWarning: true);
                return false;
            }

            result.Success = true;
            result.Latitude = response.latitude;
            result.Longitude = response.longitude;
            result.Country = response.country_name ?? response.country ?? "";
            result.CountryCode = response.country_code ?? response.country ?? "";
            result.Region = response.region ?? "";
            result.City = response.city ?? "";
            result.Timezone = response.timezone ?? "";
            result.Source = "ip";
            result.AccuracyMeters = 5000;
            result.IsAllowed = true;

            return true;
        }

        /// <summary>
        /// Parse ipinfo. io response
        /// </summary>
        private bool ParseIpInfoResponse(string json, GeolocationData result)
        {
            var response = JsonConvert.DeserializeObject<IpInfoResponse>(json);

            if (response == null)
                return false;

            // Check for bogon (private/reserved) IPs
            if (response.bogon)
            {
                Log("[Geolocation] ipinfo.io: IP is bogon/private", isWarning: true);
                return false;
            }

            // Parse location from "lat,lon" format
            double latitude = 0, longitude = 0;
            if (!string.IsNullOrEmpty(response.loc))
            {
                var parts = response.loc.Split(',');
                if (parts.Length == 2)
                {
                    double.TryParse(parts[0].Trim(), System.Globalization.NumberStyles.Float,
                        System.Globalization.CultureInfo.InvariantCulture, out latitude);
                    double.TryParse(parts[1].Trim(), System.Globalization.NumberStyles.Float,
                        System.Globalization.CultureInfo.InvariantCulture, out longitude);
                }
            }

            if (latitude == 0 && longitude == 0)
            {
                Log("[Geolocation] ipinfo.io: Could not parse location", isWarning: true);
                return false;
            }

            result.Success = true;
            result.Latitude = latitude;
            result.Longitude = longitude;
            result.Country = response.country ?? "";
            result.CountryCode = response.country ?? "";
            result.Region = response.region ?? "";
            result.City = response.city ?? "";
            result.Timezone = response.timezone ?? "";
            result.Source = "ip";
            result.AccuracyMeters = 5000;
            result.IsAllowed = true;

            return true;
        }

        /// <summary>
        /// Parse ipwhois. app response
        /// </summary>
        private bool ParseIpWhoisResponse(string json, GeolocationData result)
        {
            var response = JsonConvert.DeserializeObject<IpWhoisResponse>(json);

            if (response == null)
                return false;

            // Check for success
            if (!response.success)
            {
                Log($"[Geolocation] ipwhois.app error: {response.message}", isWarning: true);
                return false;
            }

            result.Success = true;
            result.Latitude = response.latitude;
            result.Longitude = response.longitude;
            result.Country = response.country ?? "";
            result.CountryCode = response.country_code ?? "";
            result.Region = response.region ?? "";
            result.City = response.city ?? "";
            result.Timezone = response.timezone ?? "";
            result.Source = "ip";
            result.AccuracyMeters = 5000;
            result.IsAllowed = true;

            return true;
        }

        /// <summary>
        /// Parse ipdata.co response
        /// </summary>
        private bool ParseIpDataResponse(string json, GeolocationData result)
        {
            var response = JsonConvert.DeserializeObject<IpDataResponse>(json);

            if (response == null)
                return false;

            // Check for errors
            if (!string.IsNullOrEmpty(response.message))
            {
                Log($"[Geolocation] ipdata.co message: {response.message}", isWarning: true);
                // Continue anyway as some messages are informational
            }

            result.Success = true;
            result.Latitude = response.latitude;
            result.Longitude = response.longitude;
            result.Country = response.country_name ?? "";
            result.CountryCode = response.country_code ?? "";
            result.Region = response.region ?? "";
            result.City = response.city ?? "";
            result.Timezone = response.time_zone?.name ?? "";
            result.Source = "ip";
            result.AccuracyMeters = 5000;
            result.IsAllowed = true;

            return true;
        }

        /// <summary>
        /// Parse ip-api. com response (HTTP fallback)
        /// </summary>
        private bool ParseIpApiComResponse(string json, GeolocationData result)
        {
            var response = JsonConvert.DeserializeObject<IpApiComResponse>(json);

            if (response == null)
                return false;

            // Check for success status
            if (response.status != "success")
            {
                Log($"[Geolocation] ip-api.com error: {response.message}", isWarning: true);
                return false;
            }

            result.Success = true;
            result.Latitude = response.lat;
            result.Longitude = response.lon;
            result.Country = response.country ?? "";
            result.CountryCode = response.countryCode ?? "";
            result.Region = response.regionName ?? response.region ?? "";
            result.City = response.city ?? "";
            result.Timezone = response.timezone ?? "";
            result.Source = "ip";
            result.AccuracyMeters = 5000;
            result.IsAllowed = true;

            return true;
        }

        #endregion

        #region Geolocation Data Classes

        /// <summary>
        /// Internal class to track geo endpoint configuration
        /// </summary>
        private class GeoEndpoint
        {
            public string Url { get; set; }
            public string Provider { get; set; }
            public int TimeoutSeconds { get; set; } = 10;
            public bool IsHttps { get; set; } = true;
        }

        /// <summary>
        /// Response structure for ipapi.co
        /// Free tier: 1000 requests/day, HTTPS supported
        /// Docs: https://ipapi.co/api/
        /// </summary>
        [Serializable]
        private class IpApiCoResponse
        {
            public string ip;
            public string network;
            public string version;
            public string city;
            public string region;
            public string region_code;
            public string country;              // Country code: "IN"
            public string country_code;         // Country code: "IN"
            public string country_code_iso3;    // ISO3 code: "IND"
            public string country_name;         // Full name: "India"
            public string country_capital;
            public string country_tld;
            public double country_area;
            public long country_population;
            public string continent_code;
            public bool in_eu;
            public string postal;
            public double latitude;
            public double longitude;
            public string timezone;             // "Asia/Kolkata"
            public string utc_offset;
            public string country_calling_code;
            public string currency;
            public string currency_name;
            public string languages;
            public string asn;
            public string org;

            // Error fields
            public string error;
            public string reason;
            public bool reserved;               // True if private/reserved IP
        }

        /// <summary>
        /// Response structure for ipinfo.io
        /// Free tier: 50k requests/month, HTTPS supported
        /// Docs: https://ipinfo.io/developers
        /// </summary>
        [Serializable]
        private class IpInfoResponse
        {
            public string ip;
            public string hostname;
            public string city;
            public string region;
            public string country;              // Country code: "IN"
            public string loc;                  // Location in "lat,lon" format: "12.9716,77.5946"
            public string org;
            public string postal;
            public string timezone;             // "Asia/Kolkata"
            public bool bogon;                  // True if private/reserved IP

            // Error fields
            public string error;
            public IpInfoError errorInfo;
        }

        [Serializable]
        private class IpInfoError
        {
            public string title;
            public string message;
        }

        /// <summary>
        /// Response structure for ipwhois.app
        /// Free tier: 10k requests/month, HTTPS supported
        /// Docs: https://ipwhois.io/documentation
        /// </summary>
        [Serializable]
        private class IpWhoisResponse
        {
            public string ip;
            public bool success;
            public string message;              // Error message if success is false
            public string type;                 // IPv4 or IPv6
            public string continent;
            public string continent_code;
            public string country;              // Full name: "India"
            public string country_code;         // Country code: "IN"
            public string country_flag;
            public string country_capital;
            public string country_phone;
            public string country_neighbours;
            public string region;               // "Karnataka"
            public string city;                 // "Bangalore"
            public double latitude;
            public double longitude;
            public string asn;
            public string org;
            public string isp;
            public string timezone;             // "Asia/Kolkata"
            public string timezone_name;
            public string timezone_dstOffset;
            public string timezone_gmtOffset;
            public string timezone_gmt;
            public string currency;
            public string currency_code;
            public string currency_symbol;
            public string currency_rates;
            public string currency_plural;
        }

        /// <summary>
        /// Response structure for ipdata.co
        /// Free tier: 1500 requests/day, HTTPS supported
        /// Docs: https://docs.ipdata.co/
        /// </summary>
        [Serializable]
        private class IpDataResponse
        {
            public string ip;
            public bool is_eu;
            public string city;
            public string region;
            public string region_code;
            public string country_name;         // "India"
            public string country_code;         // "IN"
            public string continent_name;
            public string continent_code;
            public double latitude;
            public double longitude;
            public string postal;
            public string calling_code;
            public string flag;
            public string emoji_flag;
            public string emoji_unicode;
            public IpDataAsn asn;
            public IpDataLanguage[] languages;
            public IpDataCurrency currency;
            public IpDataTimeZone time_zone;
            public IpDataThreat threat;
            public int count;

            // Error fields
            public string message;
        }

        [Serializable]
        private class IpDataAsn
        {
            public string asn;
            public string name;
            public string domain;
            public string route;
            public string type;
        }

        [Serializable]
        private class IpDataLanguage
        {
            public string name;
            public string native;
            public string code;
        }

        [Serializable]
        private class IpDataCurrency
        {
            public string name;
            public string code;
            public string symbol;
            public string native;
            public string plural;
        }

        [Serializable]
        private class IpDataTimeZone
        {
            public string name;                 // "Asia/Kolkata"
            public string abbr;
            public string offset;
            public bool is_dst;
            public string current_time;
        }

        [Serializable]
        private class IpDataThreat
        {
            public bool is_tor;
            public bool is_proxy;
            public bool is_anonymous;
            public bool is_known_attacker;
            public bool is_known_abuser;
            public bool is_threat;
            public bool is_bogon;
        }

        /// <summary>
        /// Response structure for ip-api.com (HTTP fallback)
        /// Free tier: 45 requests/minute, HTTP only (HTTPS requires paid)
        /// Docs: https://ip-api.com/docs/api:json
        /// </summary>
        [Serializable]
        private class IpApiComResponse
        {
            public string status;               // "success" or "fail"
            public string message;              // Error message if status is "fail"
            public string continent;
            public string continentCode;
            public string country;              // "India"
            public string countryCode;          // "IN"
            public string region;               // "KA"
            public string regionName;           // "Karnataka"
            public string city;                 // "Bangalore"
            public string district;
            public string zip;
            public double lat;
            public double lon;
            public string timezone;             // "Asia/Kolkata"
            public int offset;
            public string currency;
            public string isp;
            public string org;
            [JsonProperty("as")]
            public string asInfo;
            public string asname;
            public string reverse;
            public bool mobile;
            public bool proxy;
            public bool hosting;
            public string query;                // IP address
        }

        #endregion

    }
}