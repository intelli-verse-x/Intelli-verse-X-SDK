using System;
using UnityEngine;
using IntelliVerseX.Storage;

namespace IntelliVerseX.Core
{
    /// <summary>
    /// Main identity manager for IntelliVerse-X SDK.
    /// Provides static API for accessing user identity across all systems.
    /// All sensitive data is stored using IVXSecureStorage (encrypted PlayerPrefs).
    /// 
    /// Usage:
    ///   string username = IntelliVerseXIdentity.Username;
    ///   string email = IntelliVerseXIdentity.Email;
    ///   bool isGuest = IntelliVerseXIdentity.IsGuest;
    ///   
    ///   IntelliVerseXIdentity.SetUsername("Player1");
    ///   IntelliVerseXIdentity.UpdateWalletBalances(gameBalance, globalBalance);
    /// </summary>
    public class IntelliVerseXIdentity : MonoBehaviour
    {
        // Secure Storage Keys
        private const string PREF_USERNAME = "IVX_Username";
        private const string PREF_DEVICE_ID = "IVX_DeviceId";
        private const string PREF_GAME_ID = "IVX_GameId";
        private const string PREF_GAME_WALLET_ID = "IVX_GameWalletId";
        private const string PREF_GLOBAL_WALLET_ID = "IVX_GlobalWalletId";
        private const string PREF_GAME_WALLET_BALANCE = "IVX_GameWalletBalance";
        private const string PREF_GLOBAL_WALLET_BALANCE = "IVX_GlobalWalletBalance";
        private const string PREF_GAME_WALLET_CURRENCY = "IVX_GameWalletCurrency";
        private const string PREF_GLOBAL_WALLET_CURRENCY = "IVX_GlobalWalletCurrency";
        private const string PREF_COGNITO_USER_ID = "IVX_CognitoUserId";
        private const string PREF_EMAIL = "IVX_Email";
        private const string PREF_IDP_USERNAME = "IVX_IdpUsername";
        private const string PREF_FIRST_NAME = "IVX_FirstName";
        private const string PREF_LAST_NAME = "IVX_LastName";
        private const string PREF_WALLET_ADDRESS = "IVX_WalletAddress";
        private const string PREF_ROLE = "IVX_Role";
        private const string PREF_IS_ADULT = "IVX_IsAdult";
        private const string PREF_LOGIN_TYPE = "IVX_LoginType";
        private const string PREF_ACCOUNT_STATUS = "IVX_AccountStatus";
        private const string PREF_KYC_STATUS = "IVX_KycStatus";
        private const string PREF_IS_GUEST = "IVX_IsGuestUser";
        private const string PREF_GUEST_CREATED_EPOCH = "IVX_GuestCreatedEpoch";
        private const string PREF_ACCESS_TOKEN = "IVX_AccessToken";
        private const string PREF_ID_TOKEN = "IVX_IdToken";
        private const string PREF_REFRESH_TOKEN = "IVX_RefreshToken";
        private const string PREF_ACCESS_TOKEN_EXPIRY = "IVX_AccessTokenExpiry";
        private const string PREF_IDENTITY_MIGRATED = "IVX_IdentityMigratedV1";

        private static IntelliVerseXIdentity _instance;
        private static IntelliVerseXConfig _config;
        private static IntelliVerseXUser _currentUser;

        // Events
        public static event Action OnIdentityUpdated;
        public static event Action<int, int> OnWalletBalanceChanged; // gameBalance, globalBalance

        /// <summary>
        /// Singleton instance for accessing identity system
        /// </summary>
        public static IntelliVerseXIdentity Instance
        {
            get
            {
                if (_instance == null)
                {
                    Debug.LogWarning("[IntelliVerseXIdentity] Not initialized. Call Initialize() first.");
                }
                return _instance;
            }
        }

        /// <summary>
        /// Initialize identity system with configuration
        /// </summary>
        public static void Initialize(IntelliVerseXConfig config)
        {
            if (_instance == null)
            {
                var go = new GameObject("IntelliVerseXIdentity");
                _instance = go.AddComponent<IntelliVerseXIdentity>();
                DontDestroyOnLoad(go);
            }

            _config = config;
            _instance.LoadOrCreateUser();
        }

        // ========== STATIC API ==========

        // User Identity
        public static string Username => _currentUser?.Username ?? string.Empty;
        public static string Email => _currentUser?.Email ?? string.Empty;
        public static string DeviceId => _currentUser?.DeviceId ?? string.Empty;
        public static string GameId => _currentUser?.GameId ?? string.Empty;
        public static string CognitoUserId => _currentUser?.CognitoUserId ?? string.Empty;
        public static string GameName => _config?.gameName ?? string.Empty;
        public static string NakamaUserId
        {
            get
            {
                // 1) Prefer REAL backend user id (from login / Cognito)
                if (!string.IsNullOrEmpty(_currentUser?.CognitoUserId))
                    return _currentUser.CognitoUserId;

                // 2) Fallbacks (should rarely be needed)
                if (!string.IsNullOrEmpty(_currentUser?.GameId))
                    return _currentUser.GameId;

                if (!string.IsNullOrEmpty(_currentUser?.DeviceId))
                    return _currentUser.DeviceId;

                return string.Empty;
            }
        }
        // Wallet System
        public static string GameWalletId => _currentUser?.GameWalletId ?? string.Empty;
        public static string GlobalWalletId => _currentUser?.GlobalWalletId ?? string.Empty;
        public static int GameWalletBalance => _currentUser?.GameWalletBalance ?? 0;
        public static int GlobalWalletBalance => _currentUser?.GlobalWalletBalance ?? 0;
        
        // User State
        public static bool IsGuest => _currentUser?.IsGuestUser ?? false;
        public static bool IsAuthenticated => _currentUser?.IsAuthenticated() ?? false;
        public static bool HasCognitoAccount => _currentUser?.HasCognitoAccount() ?? false;
        public static bool HasWalletIds => _currentUser?.HasWalletIds() ?? false;
        public static bool IsGuestExpired => _currentUser?.IsGuestExpired() ?? false;
        public static int GuestDaysRemaining => _currentUser?.GetGuestDaysRemaining() ?? 0;
        
        /// <summary>
        /// Get Cognito configuration (hardcoded at SDK level for all games).
        /// Returns (PoolId, ClientId, Region) tuple.
        /// </summary>
        public static (string PoolId, string ClientId, string Region) GetCognitoConfig()
        {
            return (IVXCognitoConfig.USER_POOL_ID, IVXCognitoConfig.CLIENT_ID, IVXCognitoConfig.REGION);
        }
        
        /// <summary>
        /// Get Nakama configuration (hardcoded at SDK level for all games).
        /// Returns (Protocol, Host, Port, ServerKey) tuple.
        /// </summary>
        public static (string Protocol, string Host, int Port, string ServerKey) GetNakamaConfig()
        {
            return (IVXNakamaConfig.PROTOCOL, IVXNakamaConfig.HOST, IVXNakamaConfig.PORT, IVXNakamaConfig.SERVER_KEY);
        }
        
        /// <summary>
        /// Get Photon configuration (hardcoded at SDK level for all games).
        /// Returns Photon App ID for Realtime (PUN2).
        /// </summary>
        public static string GetPhotonAppId()
        {
            return IVXPhotonConfig.GetAppId();
        }
        
        // Tokens
        public static string AccessToken => _currentUser?.AccessToken ?? string.Empty;
        public static string IdToken => _currentUser?.IdToken ?? string.Empty;
        public static string RefreshToken => _currentUser?.RefreshToken ?? string.Empty;
        public static long AccessTokenExpiry => _currentUser?.AccessTokenExpiryEpoch ?? 0;

        /// <summary>
        /// Get the full user object (for advanced use)
        /// </summary>
        public static IntelliVerseXUser GetUser()
        {
            return _currentUser;
        }

        /// <summary>
        /// Get the current user object (alias for GetUser)
        /// </summary>
        public static IntelliVerseXUser CurrentUser => _currentUser;

        /// <summary>
        /// Set the current user object (for login/authentication)
        /// </summary>
        public static void SetCurrentUser(IntelliVerseXUser user)
        {
            _currentUser = user;
            if (_instance != null)
            {
                _instance.SaveUser();
            }
            OnIdentityUpdated?.Invoke();
        }

        /// <summary>
        /// Set username
        /// </summary>
        public static void SetUsername(string username)
        {
            if (_currentUser == null) return;
            
            _currentUser.Username = username;
            IVXSecureStorage.SetString(PREF_USERNAME, username);
            
            OnIdentityUpdated?.Invoke();
        }

        /// <summary>
        /// Set wallet IDs (called by backend module after create_or_sync_user RPC)
        /// </summary>
        public static void SetWalletIds(string gameWalletId, string globalWalletId)
        {
            if (_currentUser == null) return;
            
            _currentUser.GameWalletId = gameWalletId;
            _currentUser.GlobalWalletId = globalWalletId;
            
            IVXSecureStorage.SetString(PREF_GAME_WALLET_ID, gameWalletId);
            IVXSecureStorage.SetString(PREF_GLOBAL_WALLET_ID, globalWalletId);
            
            OnIdentityUpdated?.Invoke();
        }

        /// <summary>
        /// Update wallet balances (called by wallet manager after fetching from server)
        /// </summary>
        public static void UpdateWalletBalances(int gameBalance, int globalBalance, string gameCurrency = "coins", string globalCurrency = "gems")
        {
            if (_currentUser == null) return;
            
            _currentUser.GameWalletBalance = gameBalance;
            _currentUser.GlobalWalletBalance = globalBalance;
            _currentUser.GameWalletCurrency = gameCurrency;
            _currentUser.GlobalWalletCurrency = globalCurrency;
            
            IVXSecureStorage.SetInt(PREF_GAME_WALLET_BALANCE, gameBalance);
            IVXSecureStorage.SetInt(PREF_GLOBAL_WALLET_BALANCE, globalBalance);
            IVXSecureStorage.SetString(PREF_GAME_WALLET_CURRENCY, gameCurrency);
            IVXSecureStorage.SetString(PREF_GLOBAL_WALLET_CURRENCY, globalCurrency);
            
            OnWalletBalanceChanged?.Invoke(gameBalance, globalBalance);
        }

        /// <summary>
        /// Update Cognito identity (called by authentication module)
        /// </summary>
        public static void UpdateCognitoIdentity(
            string cognitoUserId,
            string email,
            string accessToken,
            string idToken,
            string refreshToken,
            long expiryEpoch,
            string firstName = "",
            string lastName = "",
            string role = "",
            bool isAdult = false,
            string loginType = "",
            string accountStatus = "",
            string kycStatus = "")
        {
            if (_currentUser == null) return;
            
            _currentUser.CognitoUserId = cognitoUserId;
            _currentUser.Email = email;
            _currentUser.AccessToken = accessToken;
            _currentUser.IdToken = idToken;
            _currentUser.RefreshToken = refreshToken;
            _currentUser.AccessTokenExpiryEpoch = expiryEpoch;
            _currentUser.FirstName = firstName;
            _currentUser.LastName = lastName;
            _currentUser.Role = role;
            _currentUser.IsAdult = isAdult ? "True" : "False";
            _currentUser.LoginType = loginType;
            _currentUser.AccountStatus = accountStatus;
            _currentUser.KycStatus = kycStatus;
            
            // Save to secure storage
            SaveCognitoIdentityToPrefs();
            
            OnIdentityUpdated?.Invoke();
        }

        /// <summary>
        /// Mark user as guest (called by authentication module)
        /// </summary>
        public static void SetGuestUser(bool isGuest)
        {
            if (_currentUser == null) return;
            
            _currentUser.IsGuestUser = isGuest;
            
            if (isGuest && _currentUser.GuestCreatedEpoch == 0)
            {
                _currentUser.GuestCreatedEpoch = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
                IVXSecureStorage.SetString(PREF_GUEST_CREATED_EPOCH, _currentUser.GuestCreatedEpoch.ToString());
            }
            
            IVXSecureStorage.SetInt(PREF_IS_GUEST, isGuest ? 1 : 0);
            
            OnIdentityUpdated?.Invoke();
        }

        /// <summary>
        /// Clear all user data (logout)
        /// </summary>
        public static void ClearUserData()
        {
            // Keep DeviceId and GameId (permanent identifiers)
            string deviceId = _currentUser?.DeviceId;
            string gameId = _currentUser?.GameId;
            
            IVXSecureStorage.DeleteKey(PREF_USERNAME);
            IVXSecureStorage.DeleteKey(PREF_GAME_WALLET_ID);
            IVXSecureStorage.DeleteKey(PREF_GLOBAL_WALLET_ID);
            IVXSecureStorage.DeleteKey(PREF_GAME_WALLET_BALANCE);
            IVXSecureStorage.DeleteKey(PREF_GLOBAL_WALLET_BALANCE);
            IVXSecureStorage.DeleteKey(PREF_GAME_WALLET_CURRENCY);
            IVXSecureStorage.DeleteKey(PREF_GLOBAL_WALLET_CURRENCY);
            IVXSecureStorage.DeleteKey(PREF_COGNITO_USER_ID);
            IVXSecureStorage.DeleteKey(PREF_EMAIL);
            IVXSecureStorage.DeleteKey(PREF_IDP_USERNAME);
            IVXSecureStorage.DeleteKey(PREF_FIRST_NAME);
            IVXSecureStorage.DeleteKey(PREF_LAST_NAME);
            IVXSecureStorage.DeleteKey(PREF_WALLET_ADDRESS);
            IVXSecureStorage.DeleteKey(PREF_ROLE);
            IVXSecureStorage.DeleteKey(PREF_IS_ADULT);
            IVXSecureStorage.DeleteKey(PREF_LOGIN_TYPE);
            IVXSecureStorage.DeleteKey(PREF_ACCOUNT_STATUS);
            IVXSecureStorage.DeleteKey(PREF_KYC_STATUS);
            IVXSecureStorage.DeleteKey(PREF_IS_GUEST);
            IVXSecureStorage.DeleteKey(PREF_GUEST_CREATED_EPOCH);
            IVXSecureStorage.DeleteKey(PREF_ACCESS_TOKEN);
            IVXSecureStorage.DeleteKey(PREF_ID_TOKEN);
            IVXSecureStorage.DeleteKey(PREF_REFRESH_TOKEN);
            IVXSecureStorage.DeleteKey(PREF_ACCESS_TOKEN_EXPIRY);
            
            // Recreate user with permanent IDs
            _currentUser = new IntelliVerseXUser
            {
                DeviceId = deviceId,
                GameId = gameId
            };
            
            OnIdentityUpdated?.Invoke();
        }

        // ========== INTERNAL METHODS ==========

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

        private void LoadOrCreateUser()
        {
            MigrateToSecureStorage();

            _currentUser = new IntelliVerseXUser();

            // 1. Load or generate DeviceId (persistent, never changes)
            _currentUser.DeviceId = GetOrCreateDeviceId();

            // 2. Load or generate GameId (permanent unique player ID)
            _currentUser.GameId = GetOrCreateGameId();

            // 3. Load username
            _currentUser.Username = IVXSecureStorage.GetString(PREF_USERNAME, string.Empty);

            // 4. Load wallet IDs and balances
            _currentUser.GameWalletId = IVXSecureStorage.GetString(PREF_GAME_WALLET_ID, string.Empty);
            _currentUser.GlobalWalletId = IVXSecureStorage.GetString(PREF_GLOBAL_WALLET_ID, string.Empty);
            _currentUser.GameWalletBalance = IVXSecureStorage.GetInt(PREF_GAME_WALLET_BALANCE, 0);
            _currentUser.GlobalWalletBalance = IVXSecureStorage.GetInt(PREF_GLOBAL_WALLET_BALANCE, 0);
            _currentUser.GameWalletCurrency = IVXSecureStorage.GetString(PREF_GAME_WALLET_CURRENCY, "coins");
            _currentUser.GlobalWalletCurrency = IVXSecureStorage.GetString(PREF_GLOBAL_WALLET_CURRENCY, "gems");

            // 5. Load Cognito identity
            LoadCognitoIdentityFromPrefs();

            Debug.Log($"[IntelliVerseX] Identity loaded - Username: {_currentUser.Username}, DeviceId: {_currentUser.DeviceId}, GameId: {_currentUser.GameId}");
        }

        /// <summary>
        /// One-time migration of plaintext PlayerPrefs data to encrypted IVXSecureStorage.
        /// Runs once per device; subsequent loads read encrypted values directly.
        /// </summary>
        private static void MigrateToSecureStorage()
        {
            if (IVXSecureStorage.GetInt(PREF_IDENTITY_MIGRATED, 0) == 1)
                return;

            Debug.Log("[IntelliVerseXIdentity] Migrating identity data to secure storage...");

            string[] sensitiveKeys =
            {
                PREF_USERNAME, PREF_DEVICE_ID, PREF_GAME_ID,
                PREF_GAME_WALLET_ID, PREF_GLOBAL_WALLET_ID,
                PREF_GAME_WALLET_CURRENCY, PREF_GLOBAL_WALLET_CURRENCY,
                PREF_COGNITO_USER_ID, PREF_EMAIL, PREF_IDP_USERNAME,
                PREF_FIRST_NAME, PREF_LAST_NAME, PREF_WALLET_ADDRESS,
                PREF_ROLE, PREF_LOGIN_TYPE, PREF_ACCOUNT_STATUS,
                PREF_KYC_STATUS, PREF_GUEST_CREATED_EPOCH,
                PREF_ACCESS_TOKEN, PREF_ID_TOKEN, PREF_REFRESH_TOKEN,
                PREF_ACCESS_TOKEN_EXPIRY
            };

            foreach (string key in sensitiveKeys)
            {
                IVXSecureStorage.MigrateFromPlayerPrefs(key);
            }

            IVXSecureStorage.SetInt(PREF_IDENTITY_MIGRATED, 1);
            Debug.Log("[IntelliVerseXIdentity] Migration complete.");
        }

        private string GetOrCreateDeviceId()
        {
            string deviceId = IVXSecureStorage.GetString(PREF_DEVICE_ID, string.Empty);

            if (string.IsNullOrEmpty(deviceId))
            {
                deviceId = SystemInfo.deviceUniqueIdentifier;
                
                if (string.IsNullOrEmpty(deviceId) || deviceId == SystemInfo.unsupportedIdentifier)
                {
                    deviceId = Guid.NewGuid().ToString();
                }
                
                IVXSecureStorage.SetString(PREF_DEVICE_ID, deviceId);
                Debug.Log($"[IntelliVerseX] Generated new DeviceId: {deviceId}");
            }

            return deviceId;
        }

        private string GetOrCreateGameId()
        {
            string gameId = IVXSecureStorage.GetString(PREF_GAME_ID, string.Empty);

            if (string.IsNullOrEmpty(gameId))
            {
                gameId = Guid.NewGuid().ToString();
                IVXSecureStorage.SetString(PREF_GAME_ID, gameId);
                Debug.Log($"[IntelliVerseX] Generated new GameId: {gameId}");
            }

            return gameId;
        }

        private void LoadCognitoIdentityFromPrefs()
        {
            _currentUser.CognitoUserId = IVXSecureStorage.GetString(PREF_COGNITO_USER_ID, string.Empty);
            _currentUser.Email = IVXSecureStorage.GetString(PREF_EMAIL, string.Empty);
            _currentUser.IdpUsername = IVXSecureStorage.GetString(PREF_IDP_USERNAME, string.Empty);
            _currentUser.FirstName = IVXSecureStorage.GetString(PREF_FIRST_NAME, string.Empty);
            _currentUser.LastName = IVXSecureStorage.GetString(PREF_LAST_NAME, string.Empty);
            _currentUser.WalletAddress = IVXSecureStorage.GetString(PREF_WALLET_ADDRESS, string.Empty);
            _currentUser.Role = IVXSecureStorage.GetString(PREF_ROLE, string.Empty);
            _currentUser.IsAdult = IVXSecureStorage.GetInt(PREF_IS_ADULT, 0) == 1 ? "True" : "False";
            _currentUser.LoginType = IVXSecureStorage.GetString(PREF_LOGIN_TYPE, string.Empty);
            _currentUser.AccountStatus = IVXSecureStorage.GetString(PREF_ACCOUNT_STATUS, string.Empty);
            _currentUser.KycStatus = IVXSecureStorage.GetString(PREF_KYC_STATUS, string.Empty);
            
            // Guest user fields
            _currentUser.IsGuestUser = IVXSecureStorage.GetInt(PREF_IS_GUEST, 0) == 1;
            _currentUser.GuestCreatedEpoch = IVXSecureStorage.HasKey(PREF_GUEST_CREATED_EPOCH) 
                ? long.Parse(IVXSecureStorage.GetString(PREF_GUEST_CREATED_EPOCH, "0")) 
                : 0;
            
            // Tokens
            _currentUser.AccessToken = IVXSecureStorage.GetString(PREF_ACCESS_TOKEN, string.Empty);
            _currentUser.IdToken = IVXSecureStorage.GetString(PREF_ID_TOKEN, string.Empty);
            _currentUser.RefreshToken = IVXSecureStorage.GetString(PREF_REFRESH_TOKEN, string.Empty);
            _currentUser.AccessTokenExpiryEpoch = IVXSecureStorage.HasKey(PREF_ACCESS_TOKEN_EXPIRY)
                ? long.Parse(IVXSecureStorage.GetString(PREF_ACCESS_TOKEN_EXPIRY, "0"))
                : 0;
        }

        private static void SaveCognitoIdentityToPrefs()
        {
            IVXSecureStorage.SetString(PREF_COGNITO_USER_ID, _currentUser.CognitoUserId ?? string.Empty);
            IVXSecureStorage.SetString(PREF_EMAIL, _currentUser.Email ?? string.Empty);
            IVXSecureStorage.SetString(PREF_IDP_USERNAME, _currentUser.IdpUsername ?? string.Empty);
            IVXSecureStorage.SetString(PREF_FIRST_NAME, _currentUser.FirstName ?? string.Empty);
            IVXSecureStorage.SetString(PREF_LAST_NAME, _currentUser.LastName ?? string.Empty);
            IVXSecureStorage.SetString(PREF_WALLET_ADDRESS, _currentUser.WalletAddress ?? string.Empty);
            IVXSecureStorage.SetString(PREF_ROLE, _currentUser.Role ?? string.Empty);
            IVXSecureStorage.SetInt(PREF_IS_ADULT, _currentUser.IsAdult == "True" ? 1 : 0);
            IVXSecureStorage.SetString(PREF_LOGIN_TYPE, _currentUser.LoginType ?? string.Empty);
            IVXSecureStorage.SetString(PREF_ACCOUNT_STATUS, _currentUser.AccountStatus ?? string.Empty);
            IVXSecureStorage.SetString(PREF_KYC_STATUS, _currentUser.KycStatus ?? string.Empty);
            IVXSecureStorage.SetString(PREF_ACCESS_TOKEN, _currentUser.AccessToken ?? string.Empty);
            IVXSecureStorage.SetString(PREF_ID_TOKEN, _currentUser.IdToken ?? string.Empty);
            IVXSecureStorage.SetString(PREF_REFRESH_TOKEN, _currentUser.RefreshToken ?? string.Empty);
            IVXSecureStorage.SetString(PREF_ACCESS_TOKEN_EXPIRY, _currentUser.AccessTokenExpiryEpoch.ToString());
        }

        /// <summary>
        /// Save current user data to secure storage
        /// </summary>
        private void SaveUser()
        {
            if (_currentUser == null) return;
            
            IVXSecureStorage.SetString(PREF_USERNAME, _currentUser.Username ?? string.Empty);
            IVXSecureStorage.SetString(PREF_DEVICE_ID, _currentUser.DeviceId ?? string.Empty);
            IVXSecureStorage.SetString(PREF_GAME_ID, _currentUser.GameId ?? string.Empty);
            IVXSecureStorage.SetString(PREF_GAME_WALLET_ID, _currentUser.GameWalletId ?? string.Empty);
            IVXSecureStorage.SetString(PREF_GLOBAL_WALLET_ID, _currentUser.GlobalWalletId ?? string.Empty);
            IVXSecureStorage.SetInt(PREF_GAME_WALLET_BALANCE, _currentUser.GameWalletBalance);
            IVXSecureStorage.SetInt(PREF_GLOBAL_WALLET_BALANCE, _currentUser.GlobalWalletBalance);
            IVXSecureStorage.SetString(PREF_GAME_WALLET_CURRENCY, _currentUser.GameWalletCurrency ?? string.Empty);
            IVXSecureStorage.SetString(PREF_GLOBAL_WALLET_CURRENCY, _currentUser.GlobalWalletCurrency ?? string.Empty);
            SaveCognitoIdentityToPrefs();
        }
    }
}
