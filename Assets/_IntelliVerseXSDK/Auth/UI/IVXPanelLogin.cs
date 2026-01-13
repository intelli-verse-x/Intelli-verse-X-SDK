using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace IntelliVerseX.Auth.UI
{
    /// <summary>
    /// Login panel controller for authentication.
    /// </summary>
    public class IVXPanelLogin : MonoBehaviour
    {
        #region Serialized Fields

        [Header("Input Fields")]
        [SerializeField] private TMP_InputField _emailInput;
        [SerializeField] private TMP_InputField _passwordInput;

        [Header("Buttons")]
        [SerializeField] private Button _loginButton;
        [SerializeField] private Button _registerButton;
        [SerializeField] private Button _forgotPasswordButton;
        [SerializeField] private Button _guestLoginButton;

        [Header("Social Login")]
        [SerializeField] private Button _googleSignInButton;
        [SerializeField] private Button _appleSignInButton;
        [SerializeField] private Button _facebookSignInButton;

        [Header("UI Elements")]
        [SerializeField] private TextMeshProUGUI _errorText;
        [SerializeField] private Toggle _rememberMeToggle;

        #endregion

        #region Private Fields

        private IVXCanvasAuth _canvasAuth;
        private bool _isProcessing = false;

        #endregion

        #region Unity Lifecycle

        private void Awake()
        {
            _canvasAuth = GetComponentInParent<IVXCanvasAuth>();
            SetupButtons();
        }

        private void OnEnable()
        {
            ClearError();
            LoadSavedCredentials();
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Attempt login with current credentials
        /// </summary>
        public void Login()
        {
            if (_isProcessing) return;

            string email = _emailInput?.text?.Trim();
            string password = _passwordInput?.text;

            if (string.IsNullOrEmpty(email))
            {
                ShowError("Please enter your email");
                return;
            }

            if (string.IsNullOrEmpty(password))
            {
                ShowError("Please enter your password");
                return;
            }

            _isProcessing = true;
            _canvasAuth?.ShowLoading();
            ClearError();

            // Save credentials if remember me is checked
            if (_rememberMeToggle != null && _rememberMeToggle.isOn)
            {
                SaveCredentials(email);
            }

            // TODO: Implement actual login with backend
            Debug.Log($"[{nameof(IVXPanelLogin)}] Login attempt: {email}");

            // Simulate login (replace with actual implementation)
            ProcessLoginAsync(email, password);
        }

        /// <summary>
        /// Navigate to register panel
        /// </summary>
        public void GoToRegister()
        {
            _canvasAuth?.ShowRegister();
        }

        /// <summary>
        /// Navigate to forgot password panel
        /// </summary>
        public void GoToForgotPassword()
        {
            _canvasAuth?.ShowForgotPassword();
        }

        /// <summary>
        /// Login as guest
        /// </summary>
        public void GuestLogin()
        {
            _canvasAuth?.LoginAsGuest();
        }

        #endregion

        #region Private Methods

        private void SetupButtons()
        {
            _loginButton?.onClick.AddListener(Login);
            _registerButton?.onClick.AddListener(GoToRegister);
            _forgotPasswordButton?.onClick.AddListener(GoToForgotPassword);
            _guestLoginButton?.onClick.AddListener(GuestLogin);

            // Social login buttons
            _googleSignInButton?.onClick.AddListener(SignInWithGoogle);
            _appleSignInButton?.onClick.AddListener(SignInWithApple);
            _facebookSignInButton?.onClick.AddListener(SignInWithFacebook);

            // Update guest button visibility
            if (_guestLoginButton != null && _canvasAuth != null)
            {
                _guestLoginButton.gameObject.SetActive(_canvasAuth.AllowGuestLogin);
            }
        }

        private async void ProcessLoginAsync(string email, string password)
        {
            try
            {
                // TODO: Replace with actual backend call
                await System.Threading.Tasks.Task.Delay(1000);

                var result = new AuthResult
                {
                    UserId = Guid.NewGuid().ToString(),
                    Email = email,
                    DisplayName = email.Split('@')[0],
                    IsGuest = false,
                    AccessToken = "mock_token",
                    ExpiresAt = DateTime.UtcNow.AddDays(7)
                };

                _canvasAuth?.NotifyAuthSuccess(result);
            }
            catch (Exception ex)
            {
                ShowError(ex.Message);
                _canvasAuth?.NotifyAuthFailed(ex.Message);
            }
            finally
            {
                _isProcessing = false;
            }
        }

        private void SignInWithGoogle()
        {
            Debug.Log($"[{nameof(IVXPanelLogin)}] Google Sign-In requested");
            // TODO: Implement Google Sign-In
        }

        private void SignInWithApple()
        {
            Debug.Log($"[{nameof(IVXPanelLogin)}] Apple Sign-In requested");
            // TODO: Implement Apple Sign-In
        }

        private void SignInWithFacebook()
        {
            Debug.Log($"[{nameof(IVXPanelLogin)}] Facebook Sign-In requested");
            // TODO: Implement Facebook Sign-In
        }

        private void ShowError(string message)
        {
            if (_errorText != null)
            {
                _errorText.text = message;
                _errorText.gameObject.SetActive(true);
            }
        }

        private void ClearError()
        {
            if (_errorText != null)
            {
                _errorText.text = "";
                _errorText.gameObject.SetActive(false);
            }
        }

        private void SaveCredentials(string email)
        {
            PlayerPrefs.SetString("IVX_SavedEmail", email);
            PlayerPrefs.Save();
        }

        private void LoadSavedCredentials()
        {
            if (_rememberMeToggle != null && _emailInput != null)
            {
                string savedEmail = PlayerPrefs.GetString("IVX_SavedEmail", "");
                if (!string.IsNullOrEmpty(savedEmail))
                {
                    _emailInput.text = savedEmail;
                    _rememberMeToggle.isOn = true;
                }
            }
        }

        #endregion
    }
}
