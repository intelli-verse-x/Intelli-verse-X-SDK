using System;
using UnityEngine;
using UnityEngine.UI;

namespace IntelliVerseX.Auth.UI
{
    /// <summary>
    /// Main authentication canvas controller.
    /// Manages login, register, OTP, and guest authentication panels.
    /// </summary>
    public class IVXCanvasAuth : MonoBehaviour
    {
        #region Serialized Fields

        [Header("Panel References")]
        [SerializeField] private GameObject _loginPanel;
        [SerializeField] private GameObject _registerPanel;
        [SerializeField] private GameObject _otpPanel;
        [SerializeField] private GameObject _forgotPasswordPanel;
        [SerializeField] private GameObject _loadingPanel;

        [Header("Configuration")]
        [SerializeField] private bool _showOnStart = true;
        [SerializeField] private bool _allowGuestLogin = true;
        [SerializeField] private bool _enableSocialLogin = true;

        [Header("Social Login")]
        [SerializeField] private bool _enableGoogleSignIn = true;
        [SerializeField] private bool _enableAppleSignIn = true;
        [SerializeField] private bool _enableFacebookSignIn = false;

        #endregion

        #region Private Fields

        private AuthPanel _currentPanel = AuthPanel.None;
        private bool _isInitialized = false;

        #endregion

        #region Events

        public static event Action<AuthResult> OnAuthSuccess;
        public static event Action<string> OnAuthFailed;
        public static event Action OnLogout;

        #endregion

        #region Properties

        public bool IsInitialized => _isInitialized;
        public AuthPanel CurrentPanel => _currentPanel;
        public bool AllowGuestLogin => _allowGuestLogin;
        public bool EnableSocialLogin => _enableSocialLogin;
        public bool EnableGoogleSignIn => _enableGoogleSignIn;
        public bool EnableAppleSignIn => _enableAppleSignIn;
        public bool EnableFacebookSignIn => _enableFacebookSignIn;

        #endregion

        #region Unity Lifecycle

        private void Awake()
        {
            Initialize();
        }

        private void Start()
        {
            if (_showOnStart)
            {
                ShowLogin();
            }
            else
            {
                HideAllPanels();
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Initialize the auth canvas
        /// </summary>
        public void Initialize()
        {
            if (_isInitialized) return;

            ValidateReferences();
            _isInitialized = true;
            Debug.Log($"[{nameof(IVXCanvasAuth)}] Initialized");
        }

        /// <summary>
        /// Show login panel
        /// </summary>
        public void ShowLogin()
        {
            HideAllPanels();
            if (_loginPanel != null)
            {
                _loginPanel.SetActive(true);
                _currentPanel = AuthPanel.Login;
            }
        }

        /// <summary>
        /// Show register panel
        /// </summary>
        public void ShowRegister()
        {
            HideAllPanels();
            if (_registerPanel != null)
            {
                _registerPanel.SetActive(true);
                _currentPanel = AuthPanel.Register;
            }
        }

        /// <summary>
        /// Show OTP verification panel
        /// </summary>
        public void ShowOTP()
        {
            HideAllPanels();
            if (_otpPanel != null)
            {
                _otpPanel.SetActive(true);
                _currentPanel = AuthPanel.OTP;
            }
        }

        /// <summary>
        /// Show forgot password panel
        /// </summary>
        public void ShowForgotPassword()
        {
            HideAllPanels();
            if (_forgotPasswordPanel != null)
            {
                _forgotPasswordPanel.SetActive(true);
                _currentPanel = AuthPanel.ForgotPassword;
            }
        }

        /// <summary>
        /// Show loading state
        /// </summary>
        public void ShowLoading()
        {
            if (_loadingPanel != null)
            {
                _loadingPanel.SetActive(true);
            }
        }

        /// <summary>
        /// Hide loading state
        /// </summary>
        public void HideLoading()
        {
            if (_loadingPanel != null)
            {
                _loadingPanel.SetActive(false);
            }
        }

        /// <summary>
        /// Hide all panels and close canvas
        /// </summary>
        public void Close()
        {
            HideAllPanels();
            gameObject.SetActive(false);
        }

        /// <summary>
        /// Login as guest
        /// </summary>
        public void LoginAsGuest()
        {
            if (!_allowGuestLogin)
            {
                Debug.LogWarning($"[{nameof(IVXCanvasAuth)}] Guest login is disabled");
                return;
            }

            ShowLoading();
            // TODO: Implement guest login with backend
            Debug.Log($"[{nameof(IVXCanvasAuth)}] Guest login requested");
        }

        #endregion

        #region Internal Methods

        internal void NotifyAuthSuccess(AuthResult result)
        {
            HideLoading();
            OnAuthSuccess?.Invoke(result);
            Close();
        }

        internal void NotifyAuthFailed(string error)
        {
            HideLoading();
            OnAuthFailed?.Invoke(error);
        }

        internal void NotifyLogout()
        {
            OnLogout?.Invoke();
            ShowLogin();
        }

        #endregion

        #region Private Methods

        private void HideAllPanels()
        {
            if (_loginPanel != null) _loginPanel.SetActive(false);
            if (_registerPanel != null) _registerPanel.SetActive(false);
            if (_otpPanel != null) _otpPanel.SetActive(false);
            if (_forgotPasswordPanel != null) _forgotPasswordPanel.SetActive(false);
            if (_loadingPanel != null) _loadingPanel.SetActive(false);
            _currentPanel = AuthPanel.None;
        }

        private void ValidateReferences()
        {
            if (_loginPanel == null)
                Debug.LogWarning($"[{nameof(IVXCanvasAuth)}] Login panel not assigned");
            if (_registerPanel == null)
                Debug.LogWarning($"[{nameof(IVXCanvasAuth)}] Register panel not assigned");
        }

        #endregion
    }

    #region Enums

    public enum AuthPanel
    {
        None,
        Login,
        Register,
        OTP,
        ForgotPassword
    }

    #endregion

    #region Data Models

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
