using System;
using UnityEngine;
using Nakama;
using IntelliVerseX.Core;

namespace IntelliVerseX.Backend.Nakama
{
    public sealed class IVXNUserRuntime : MonoBehaviour
    {
        public static IVXNUserRuntime Instance { get; private set; }

        [Header("Behaviour")]
        [SerializeField] private bool autoRefreshOnStart = true;
        [SerializeField] private bool enableDebugLogs = true;

        // API snapshot
        [Header("API User (UserSessionManager.Current) - READONLY RUNTIME SNAPSHOT")]
        [SerializeField] private string apiUserId;
        [SerializeField] private string apiUserName;
        [SerializeField] private string apiEmail;
        [SerializeField] private string apiLoginType;
        [SerializeField] private string apiRole;
        [SerializeField] private string apiCreatedAt;
        [SerializeField] private string apiUpdatedAt;

        // IVX identity snapshot
        [Header("IntelliVerseX Identity (IntelliVerseXIdentity.CurrentUser) - READONLY SNAPSHOT")]
        [SerializeField] private string ivxUsername;
        [SerializeField] private string ivxEmail;
        [SerializeField] private string ivxDeviceId;
        [SerializeField] private string ivxGameId;

        [SerializeField] private string ivxGameWalletId;
        [SerializeField] private string ivxGlobalWalletId;
        [SerializeField] private int ivxGameWalletBalance;
        [SerializeField] private int ivxGlobalWalletBalance;

        [SerializeField] private bool ivxIsGuest;
        [SerializeField] private bool ivxIsAuthenticated;
        [SerializeField] private bool ivxHasWalletIds;

        [SerializeField] private string ivxAccessTokenExpiryUtc;
        [SerializeField] private string ivxNakamaUserId;

        // Nakama snapshot
        [Header("Nakama Session (IVXNManager.Session) - READONLY RUNTIME SNAPSHOT")]
        [SerializeField] private string nakamaUserId;
        [SerializeField] private string nakamaUsername;
        [SerializeField] private string nakamaExpireAtUtc;
        [SerializeField] private bool nakamaIsExpired;

        // Public properties
        public string ApiUserId => apiUserId;
        public string ApiUserName => apiUserName;
        public string ApiEmail => apiEmail;
        public string ApiLoginType => apiLoginType;
        public string ApiRole => apiRole;
        public string ApiCreatedAt => apiCreatedAt;
        public string ApiUpdatedAt => apiUpdatedAt;

        public string IvxUsername => ivxUsername;
        public string IvxEmail => ivxEmail;
        public string IvxDeviceId => ivxDeviceId;
        public string IvxGameId => ivxGameId;
        public string IvxGameWalletId => ivxGameWalletId;
        public string IvxGlobalWalletId => ivxGlobalWalletId;
        public int IvxGameWalletBalance => ivxGameWalletBalance;
        public int IvxGlobalWalletBalance => ivxGlobalWalletBalance;
        public bool IvxIsGuest => ivxIsGuest;
        public bool IvxIsAuthenticated => ivxIsAuthenticated;
        public bool IvxHasWalletIds => ivxHasWalletIds;
        public string IvxAccessTokenExpiryUtc => ivxAccessTokenExpiryUtc;
        public string IvxNakamaUserId => ivxNakamaUserId;

        public string NakamaUserId => nakamaUserId;
        public string NakamaUsername => nakamaUsername;
        public string NakamaExpireAtUtc => nakamaExpireAtUtc;
        public bool NakamaIsExpired => nakamaIsExpired;

        private const string LOGTAG = "[IVXNUserRuntime]";

        private bool _subscribedToManager;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Log("Duplicate instance detected. Destroying this one.", isWarning: true);
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);

            TrySubscribeToManager();
        }

        private void Start()
        {
            if (autoRefreshOnStart)
            {
                try
                {
                    RefreshFromGlobals();
                }
                catch (Exception ex)
                {
                    Log($"Start() RefreshFromGlobals exception: {ex}", isError: true);
                }
            }

            // In case manager was created after our Awake.
            TrySubscribeToManager();
        }

        private void OnDestroy()
        {
            try
            {
                if (_subscribedToManager && IVXNManager.Instance != null)
                {
                    IVXNManager.Instance.OnInitialized -= HandleNakamaInitialized;
                }
            }
            catch (Exception ex)
            {
                Log($"OnDestroy unsubscribe error: {ex}", isWarning: true);
            }

            if (Instance == this)
            {
                Instance = null;
            }
        }

        private void TrySubscribeToManager()
        {
            if (_subscribedToManager) return;

            try
            {
                if (IVXNManager.Instance != null)
                {
                    IVXNManager.Instance.OnInitialized -= HandleNakamaInitialized;
                    IVXNManager.Instance.OnInitialized += HandleNakamaInitialized;
                    _subscribedToManager = true;
                }
            }
            catch (Exception ex)
            {
                Log($"Failed to subscribe to IVXNManager.OnInitialized: {ex}", isError: true);
            }
        }

        private void HandleNakamaInitialized(bool success)
        {
            if (!success)
            {
                Log("Received OnInitialized(false) from IVXNManager. Snapshot will not be refreshed.");
                return;
            }

            try
            {
                RefreshFromGlobals();
            }
            catch (Exception ex)
            {
                Log($"HandleNakamaInitialized.RefreshFromGlobals exception: {ex}", isError: true);
            }
        }

        public void RefreshFromGlobals()
        {
            global::UserSessionManager.UserSession apiUser = null;
            ISession nakamaSession = null;
            IntelliVerseXUser ivxUser = null;

            try
            {
                apiUser = global::UserSessionManager.Current;
            }
            catch (Exception ex)
            {
                Log($"Error reading UserSessionManager.Current: {ex}", isError: true);
            }

            try
            {
                var mgr = IVXNManager.Instance;
                if (mgr != null)
                {
                    nakamaSession = mgr.Session;
                }
                else
                {
                    Log("IVXNManager.Instance is null while refreshing snapshot.", isWarning: true);
                }
            }
            catch (Exception ex)
            {
                Log($"Error reading IVXNManager.Instance.Session: {ex}", isError: true);
            }

            try
            {
                ivxUser = IntelliVerseXIdentity.GetUser();
            }
            catch (Exception ex)
            {
                Log($"Error reading IntelliVerseXIdentity.GetUser(): {ex}", isError: true);
            }

            CaptureFromSession(apiUser, nakamaSession, ivxUser);
        }

        public void CaptureFromSession(
            global::UserSessionManager.UserSession apiUser,
            ISession nakamaSession,
            IntelliVerseXUser ivxUser = null)
        {
            // API side
            if (apiUser != null)
            {
                apiUserId = apiUser.userId;
                apiUserName = apiUser.userName;
                apiEmail = apiUser.email;
                apiLoginType = apiUser.loginType;
                apiRole = apiUser.role;
                apiCreatedAt = apiUser.createdAt;
                apiUpdatedAt = apiUser.updatedAt;
            }
            else
            {
                apiUserId = apiUserName = apiEmail =
                    apiLoginType = apiRole = apiCreatedAt = apiUpdatedAt = "<null>";
            }

            // Nakama side
            if (nakamaSession != null)
            {
                nakamaUserId = nakamaSession.UserId;
                nakamaUsername = nakamaSession.Username;

                try
                {
                    var expireAtUtc = DateTimeOffset
                        .FromUnixTimeSeconds(nakamaSession.ExpireTime)
                        .UtcDateTime;
                    nakamaExpireAtUtc = expireAtUtc.ToString("O");
                }
                catch
                {
                    nakamaExpireAtUtc = nakamaSession.ExpireTime.ToString();
                }

                nakamaIsExpired = nakamaSession.IsExpired;
            }
            else
            {
                nakamaUserId = nakamaUsername = nakamaExpireAtUtc = "<null>";
                nakamaIsExpired = true;
            }

            // IVX identity side
            if (ivxUser != null)
            {
                ivxUsername = ivxUser.Username;
                ivxEmail = ivxUser.Email;
                ivxDeviceId = ivxUser.DeviceId;
                ivxGameId = ivxUser.GameId;
                ivxGameWalletId = ivxUser.GameWalletId;
                ivxGlobalWalletId = ivxUser.GlobalWalletId;
                ivxGameWalletBalance = ivxUser.GameWalletBalance;
                ivxGlobalWalletBalance = ivxUser.GlobalWalletBalance;
                ivxIsGuest = ivxUser.IsGuestUser;
                ivxIsAuthenticated = ivxUser.IsAuthenticated();
                ivxHasWalletIds = ivxUser.HasWalletIds();

                try
                {
                    if (ivxUser.AccessTokenExpiryEpoch > 0)
                    {
                        var atExpiryUtc = DateTimeOffset
                            .FromUnixTimeSeconds(ivxUser.AccessTokenExpiryEpoch)
                            .UtcDateTime;
                        ivxAccessTokenExpiryUtc = atExpiryUtc.ToString("O");
                    }
                    else
                    {
                        ivxAccessTokenExpiryUtc = "0";
                    }
                }
                catch
                {
                    ivxAccessTokenExpiryUtc = ivxUser.AccessTokenExpiryEpoch.ToString();
                }

                ivxNakamaUserId = IntelliVerseXIdentity.NakamaUserId;
            }
            else
            {
                ivxUsername = ivxEmail = ivxDeviceId = ivxGameId =
                    ivxGameWalletId = ivxGlobalWalletId =
                    ivxAccessTokenExpiryUtc = ivxNakamaUserId = "<null>";

                ivxGameWalletBalance = 0;
                ivxGlobalWalletBalance = 0;
                ivxIsGuest = false;
                ivxIsAuthenticated = false;
                ivxHasWalletIds = false;
            }

            LogSnapshot();
        }

        public void LogSnapshot()
        {
            if (!enableDebugLogs) return;

            Debug.Log($"{LOGTAG} API USER:");
            Debug.Log($"{LOGTAG}   userId={apiUserId}, userName={apiUserName}, email={apiEmail}, loginType={apiLoginType}, role={apiRole}");
            Debug.Log($"{LOGTAG}   createdAt={apiCreatedAt}, updatedAt={apiUpdatedAt}");

            Debug.Log($"{LOGTAG} IVX IDENTITY:");
            Debug.Log($"{LOGTAG}   username={ivxUsername}, email={ivxEmail}, deviceId={ivxDeviceId}, gameId={ivxGameId}");
            Debug.Log($"{LOGTAG}   gameWalletId={ivxGameWalletId}, globalWalletId={ivxGlobalWalletId}, " +
                      $"gameBalance={ivxGameWalletBalance}, globalBalance={ivxGlobalWalletBalance}");
            Debug.Log($"{LOGTAG}   isGuest={ivxIsGuest}, isAuthenticated={ivxIsAuthenticated}, hasWalletIds={ivxHasWalletIds}");
            Debug.Log($"{LOGTAG}   accessTokenExpiryUtc={ivxAccessTokenExpiryUtc}, ivxNakamaUserId={ivxNakamaUserId}");

            Debug.Log($"{LOGTAG} NAKAMA:");
            Debug.Log($"{LOGTAG}   userId={nakamaUserId}, username={nakamaUsername}, expireAt={nakamaExpireAtUtc}, isExpired={nakamaIsExpired}");
        }

        private void Log(string message, bool isWarning = false, bool isError = false)
        {
            if (!enableDebugLogs && !isError) return;

            if (isError) Debug.LogError($"{LOGTAG} {message}");
            else if (isWarning) Debug.LogWarning($"{LOGTAG} {message}");
            else Debug.Log($"{LOGTAG} {message}");
        }
    }
}
