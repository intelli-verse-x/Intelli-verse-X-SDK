using System;
using System.Threading.Tasks;
using UnityEngine;
using IntelliVerseX.Identity;

namespace IntelliVerseX.Auth.UI
{
    /// <summary>
    /// Central authentication canvas controller.
    /// Manages all auth panels (Login, Register, OTP, ForgotPassword, Referral) and auth flow state.
    /// </summary>
    public class IVXCanvasAuth : MonoBehaviour
    {
        #region Serialized Fields

        [Header("Panels")]
        [SerializeField] private IVXPanelLogin _loginPanel;
        [SerializeField] private IVXPanelRegister _registerPanel;
        [SerializeField] private IVXPanelOTP _otpPanel;
        [SerializeField] private IVXPanelForgotPassword _forgotPasswordPanel;
        [SerializeField] private IVXPanelReferral _referralPanel;
        [SerializeField] private GameObject _loadingPanel;

        [Header("Config")]
        [SerializeField] private bool _allowGuestLogin = true;
        [SerializeField] private bool _tryRestoreSessionOnStart = true;
        [SerializeField] private string _gameSceneName = "";
        [SerializeField] private int _gameSceneBuildIndex = 1;

        #endregion

        #region Events

        /// <summary>
        /// Fired when authentication succeeds (login, register+OTP, guest, social)
        /// </summary>
        public event Action<AuthResult> OnAuthSuccess;

        /// <summary>
        /// Fired when authentication fails
        /// </summary>
        public event Action<string> OnAuthFailed;

        /// <summary>
        /// Fired when referral code is submitted from referral panel
        /// </summary>
        private event Action<string> _onReferralSubmitted;

        #endregion

        #region Public Properties

        public bool AllowGuestLogin => _allowGuestLogin;
        public IVXPanelLogin LoginPanel => _loginPanel;
        public IVXPanelRegister RegisterPanel => _registerPanel;
        public IVXPanelOTP OTPPanel => _otpPanel;
        public IVXPanelForgotPassword ForgotPasswordPanel => _forgotPasswordPanel;
        public IVXPanelReferral ReferralPanel => _referralPanel;
        public string GameSceneName => _gameSceneName;
        public int GameSceneBuildIndex => _gameSceneBuildIndex;

        #endregion

        #region Unity Lifecycle

        private void Awake()
        {
            BindPanels();
            
            if (_tryRestoreSessionOnStart)
            {
                APIManager.TryConfigureUserAuthFromSavedSession(enable: true);
            }
        }

        private void Start()
        {
            ShowLogin();
        }

        #endregion

        #region Panel Binding

        private void BindPanels()
        {
            if (_loginPanel != null) _loginPanel.Bind(this);
            if (_registerPanel != null) _registerPanel.Bind(this);
            if (_otpPanel != null) _otpPanel.Bind(this);
            if (_forgotPasswordPanel != null) _forgotPasswordPanel.Bind(this);
            if (_referralPanel != null) _referralPanel.Bind(this);
        }

        #endregion

        #region Panel Control

        /// <summary>
        /// Show the login panel
        /// </summary>
        public void ShowLogin()
        {
            HideAllPanels(_loginPanel);
            _loginPanel?.Open();
        }

        /// <summary>
        /// Show the register panel
        /// </summary>
        public void ShowRegister()
        {
            HideAllPanels(_registerPanel);
            _registerPanel?.Open();
        }

        /// <summary>
        /// Show the OTP verification panel
        /// </summary>
        public void ShowOTP()
        {
            HideAllPanels(_otpPanel);
            _otpPanel?.Open();
        }

        /// <summary>
        /// Hide the OTP panel
        /// </summary>
        public void HideOTP()
        {
            _otpPanel?.Close();
        }

        /// <summary>
        /// Show the forgot password panel
        /// </summary>
        public void ShowForgotPassword()
        {
            HideAllPanels(_forgotPasswordPanel);
            _forgotPasswordPanel?.Open();
        }

        /// <summary>
        /// Show the referral code popup
        /// </summary>
        /// <param name="prefillCode">Optional code to prefill</param>
        public void ShowReferralPopup(string prefillCode = "")
        {
            _referralPanel?.Open(prefillCode);
        }

        /// <summary>
        /// Hide the referral popup
        /// </summary>
        public void HideReferralPopup()
        {
            _referralPanel?.Close();
        }

        /// <summary>
        /// Show the loading overlay
        /// </summary>
        public void ShowLoading()
        {
            if (_loadingPanel != null) _loadingPanel.SetActive(true);
        }

        /// <summary>
        /// Hide the loading overlay
        /// </summary>
        public void HideLoading()
        {
            if (_loadingPanel != null) _loadingPanel.SetActive(false);
        }

        private void HideAllPanels(MonoBehaviour panelToKeepOpen = null)
        {
            if (_loginPanel != panelToKeepOpen) _loginPanel?.Close();
            if (_registerPanel != panelToKeepOpen) _registerPanel?.Close();
            if (_otpPanel != panelToKeepOpen) _otpPanel?.Close();
            if (_forgotPasswordPanel != panelToKeepOpen) _forgotPasswordPanel?.Close();
            HideLoading();
        }

        #endregion

        #region Referral System

        /// <summary>
        /// Subscribe to referral code submission events
        /// </summary>
        public void SubscribeReferralSubmit(Action<string> handler)
        {
            if (handler != null)
            {
                _onReferralSubmitted += handler;
            }
        }

        /// <summary>
        /// Unsubscribe from referral code submission events
        /// </summary>
        public void UnsubscribeReferralSubmit(Action<string> handler)
        {
            if (handler != null)
            {
                _onReferralSubmitted -= handler;
            }
        }

        /// <summary>
        /// Called by referral panel when code is submitted
        /// </summary>
        internal void NotifyReferralSubmitted(string code)
        {
            _onReferralSubmitted?.Invoke(code);
        }

        #endregion

        #region Auth Callbacks

        /// <summary>
        /// Called by panels when authentication succeeds
        /// </summary>
        public void NotifyAuthSuccess(AuthResult result)
        {
            HideLoading();
            Debug.Log($"[{nameof(IVXCanvasAuth)}] Auth success: {result?.Email ?? "Unknown"}");
            OnAuthSuccess?.Invoke(result);
        }

        /// <summary>
        /// Called by panels when authentication fails
        /// </summary>
        public void NotifyAuthFailed(string error)
        {
            HideLoading();
            Debug.LogWarning($"[{nameof(IVXCanvasAuth)}] Auth failed: {error}");
            OnAuthFailed?.Invoke(error);
        }

        #endregion

        #region Guest Login

        /// <summary>
        /// Perform guest login
        /// </summary>
        public async void LoginAsGuest()
        {
            if (!_allowGuestLogin)
            {
                Debug.LogWarning($"[{nameof(IVXCanvasAuth)}] Guest login is disabled");
                return;
            }

            ShowLoading();

            try
            {
                var response = await APIManager.GuestSignupAsync(
                    role: "user",
                    configureUserAuthOnSuccess: true,
                    persistSession: true);

                if (response?.data != null)
                {
                    var result = BuildAuthResultFromGuestResponse(response);
                    NotifyAuthSuccess(result);
                }
                else
                {
                    NotifyAuthFailed(response?.message ?? "Guest signup failed");
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"[{nameof(IVXCanvasAuth)}] Guest login error: {ex.Message}");
                NotifyAuthFailed(ex.Message);
            }
        }

        private static AuthResult BuildAuthResultFromGuestResponse(APIManager.GuestSignupResponse response)
        {
            var d = response.data;
            var u = d?.user;
            
            string userId = u != null && !string.IsNullOrEmpty(u.idpUsername) 
                ? u.idpUsername 
                : u?.id ?? string.Empty;
            
            string access = d != null 
                ? (!string.IsNullOrEmpty(d.accessToken) ? d.accessToken : d.token ?? string.Empty) 
                : string.Empty;
            
            long expEpoch = d != null
                ? (DateTimeOffset.UtcNow.ToUnixTimeSeconds() + Math.Max(0, d.expiresIn <= 0 ? 1800 : d.expiresIn))
                : DateTimeOffset.UtcNow.ToUnixTimeSeconds() + 1800;
            
            DateTime expAt;
            try
            {
                expAt = DateTimeOffset.FromUnixTimeSeconds(expEpoch).UtcDateTime;
            }
            catch
            {
                expAt = DateTime.UtcNow.AddMinutes(30);
            }

            return new AuthResult
            {
                UserId = userId,
                Email = u?.email ?? "guest@local",
                DisplayName = u?.userName ?? u?.firstName ?? "Guest",
                AccessToken = access,
                RefreshToken = d?.refreshToken ?? string.Empty,
                IsGuest = true,
                ExpiresAt = expAt
            };
        }

        #endregion

        #region Auto-Find Panels (Editor Helper)

        /// <summary>
        /// Auto-find and assign panel references from children
        /// </summary>
        [ContextMenu("Auto-Find Panel References")]
        public void AutoFindPanels()
        {
            if (_loginPanel == null)
                _loginPanel = GetComponentInChildren<IVXPanelLogin>(true);
            
            if (_registerPanel == null)
                _registerPanel = GetComponentInChildren<IVXPanelRegister>(true);
            
            if (_otpPanel == null)
                _otpPanel = GetComponentInChildren<IVXPanelOTP>(true);
            
            if (_forgotPasswordPanel == null)
                _forgotPasswordPanel = GetComponentInChildren<IVXPanelForgotPassword>(true);
            
            if (_referralPanel == null)
                _referralPanel = GetComponentInChildren<IVXPanelReferral>(true);

            Debug.Log($"[{nameof(IVXCanvasAuth)}] Auto-find complete. " +
                $"Login={_loginPanel != null}, Register={_registerPanel != null}, " +
                $"OTP={_otpPanel != null}, ForgotPwd={_forgotPasswordPanel != null}, " +
                $"Referral={_referralPanel != null}");
        }

        #endregion
    }

    #region Data Models

    /// <summary>
    /// Result of successful authentication
    /// </summary>
    [Serializable]
    public class AuthResult
    {
        public string UserId;
        public string Email;
        public string DisplayName;
        public string AccessToken;
        public string RefreshToken;
        public bool IsGuest;
        public DateTime ExpiresAt;
    }

    #endregion
}
