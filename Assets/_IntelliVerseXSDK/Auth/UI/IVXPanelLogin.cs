using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using IntelliVerseX.Identity;

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
            ValidateInputFields();
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

            // Try to auto-find input fields if not assigned (fallback)
            ValidateInputFields();

            // Validate input fields are assigned
            if (_emailInput == null)
            {
                ShowError("Email input field is not configured.");
                Debug.LogError("[IVXPanelLogin] Email input field is not assigned in the Inspector and could not be auto-found!");
                return;
            }

            if (_passwordInput == null)
            {
                ShowError("Password input field is not configured.");
                Debug.LogError("[IVXPanelLogin] Password input field is not assigned in the Inspector and could not be auto-found!");
                return;
            }

            string email = _emailInput.text?.Trim();
            string password = _passwordInput.text?.Trim();

            if (string.IsNullOrWhiteSpace(email))
            {
                ShowError("Please enter your email");
                return;
            }

            // Validate email format
            if (!IsValidEmail(email))
            {
                ShowError("Please enter a valid email address");
                return;
            }

            if (string.IsNullOrWhiteSpace(password))
            {
                ShowError("Please enter your password");
                return;
            }

            // Validate password is not just whitespace
            if (password.Length == 0)
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

            Debug.Log($"[{nameof(IVXPanelLogin)}] Login attempt: {email}");

            // Call real backend login API
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

        /// <summary>
        /// Process login with real backend API via AuthService.
        /// This allows players who registered and verified OTP to login with their email and password.
        /// </summary>
        private void ProcessLoginAsync(string email, string password)
        {
            _canvasAuth?.ShowLoading();
            ClearError();

            Debug.Log($"[{nameof(IVXPanelLogin)}] Calling login API for: {email}");

            // Try to find AuthService if not already found
            var authService = FindObjectOfType<AuthService>();
            if (authService == null)
            {
                GameObject authServiceObj = new GameObject("AuthService");
                authService = authServiceObj.AddComponent<AuthService>();
            }

            // Call real backend login API via AuthService
            authService.Login(
                email,
                password,
                OnLoginSuccess,
                OnLoginError);
        }

        private void OnLoginSuccess(APIManager.LoginResponse response)
        {
            _isProcessing = false;
            _canvasAuth?.HideLoading();

            if (response?.data?.user != null)
            {
                // Build auth result from real API response
                var result = BuildAuthResultFromLoginResponse(response);
                
                Debug.Log($"[{nameof(IVXPanelLogin)}] Login successful");
                
                // Configure user authentication (sets tokens, session, etc.)
                APIManager.ConfigureUserAuthFromLoginResponse(response, true);
                
                // Notify canvas of successful authentication
                _canvasAuth?.NotifyAuthSuccess(result);
            }
            else
            {
                string errorMsg = response?.message ?? "Login failed. Please check your email and password.";
                Debug.LogWarning($"[{nameof(IVXPanelLogin)}] Login failed: {errorMsg}");
                ShowError(errorMsg);
                _canvasAuth?.NotifyAuthFailed(errorMsg);
            }
        }

        /// <summary>
        /// Handles login error from backend.
        /// Provides user-friendly error messages, especially for unverified accounts.
        /// </summary>
        private void OnLoginError(string errorMessage)
        {
            _isProcessing = false;
            _canvasAuth?.HideLoading();

            string errorMsg = errorMessage ?? "Login failed. Please check your email and password.";
            
            // Check for specific error cases from backend
            if (!string.IsNullOrEmpty(errorMessage))
            {
                // Account not verified - OTP verification required
                if (errorMessage.Contains("verified") || 
                    errorMessage.Contains("OTP") || 
                    errorMessage.Contains("verify") ||
                    errorMessage.Contains("not activated") ||
                    errorMessage.Contains("activation"))
                {
                    errorMsg = "Please verify your email with OTP before logging in. Check your email for the verification code.";
                    Debug.LogWarning($"[{nameof(IVXPanelLogin)}] Login blocked: Account not verified. User must complete OTP verification first.");
                }
                // Invalid credentials
                else if (errorMessage.Contains("Invalid") || errorMessage.Contains("incorrect"))
                {
                    errorMsg = "Invalid email or password. Please try again.";
                }
                // Account not found
                else if (errorMessage.Contains("not found") || errorMessage.Contains("does not exist"))
                {
                    errorMsg = "Account not found. Please register first.";
                }
                // Account exists but not verified (backend should return this)
                else if (errorMessage.Contains("Account not verified") || errorMessage.Contains("not verified"))
                {
                    errorMsg = "Account not verified. Please verify your email with OTP first.";
                }
            }
            
            Debug.LogWarning($"[{nameof(IVXPanelLogin)}] Login failed: {errorMsg}");
            ShowError(errorMsg);
            _canvasAuth?.NotifyAuthFailed(errorMsg);
        }

        /// <summary>
        /// Build AuthResult from LoginResponse for compatibility with IVXCanvasAuth.
        /// </summary>
        private static AuthResult BuildAuthResultFromLoginResponse(APIManager.LoginResponse loginResp)
        {
            var d = loginResp.data;
            var u = d.user;
            
            string userId = !string.IsNullOrEmpty(u.idpUsername) ? u.idpUsername : u.id ?? string.Empty;
            string access = !string.IsNullOrEmpty(d.accessToken) ? d.accessToken : d.token ?? string.Empty;
            
            // Calculate expiration time
            long expEpoch = DateTimeOffset.UtcNow.ToUnixTimeSeconds() + Math.Max(0, d.expiresIn <= 0 ? 1800 : d.expiresIn);
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
                Email = u.email ?? string.Empty,
                DisplayName = u.userName ?? u.firstName ?? u.email?.Split('@')[0] ?? "User",
                AccessToken = access,
                RefreshToken = d.refreshToken ?? string.Empty,
                IsGuest = false,
                ExpiresAt = expAt
            };
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

        /// <summary>
        /// Validate email format using MailAddress class.
        /// </summary>
        private bool IsValidEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return false;

            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Validate that input fields are properly configured and auto-find them if not assigned.
        /// </summary>
        private void ValidateInputFields()
        {
            // Auto-find email input if not assigned
            if (_emailInput == null)
            {
                _emailInput = FindInputFieldByName("EmailInput", transform) 
                    ?? FindInputFieldByName("Email", transform) 
                    ?? FindInputFieldByPlaceholder("Email", transform);
                
                if (_emailInput != null)
                {
                    Debug.Log("[IVXPanelLogin] Auto-found email input field.");
                }
            }

            // Auto-find password input if not assigned
            if (_passwordInput == null)
            {
                _passwordInput = FindInputFieldByName("PasswordInput", transform) 
                    ?? FindInputFieldByName("Password", transform) 
                    ?? FindInputFieldByPlaceholder("Password", transform);
                
                if (_passwordInput != null)
                {
                    Debug.Log("[IVXPanelLogin] Auto-found password input field.");
                    // Ensure password field is configured for password input
                    if (_passwordInput.contentType != TMP_InputField.ContentType.Password)
                    {
                        Debug.LogWarning("[IVXPanelLogin] Password input field content type is not set to Password. Setting it now...");
                        _passwordInput.contentType = TMP_InputField.ContentType.Password;
                    }
                }
            }
            else
            {
                // Ensure password field is configured for password input even if assigned
                if (_passwordInput.contentType != TMP_InputField.ContentType.Password)
                {
                    Debug.LogWarning("[IVXPanelLogin] Password input field content type is not set to Password. Setting it now...");
                    _passwordInput.contentType = TMP_InputField.ContentType.Password;
                }
            }
        }

        /// <summary>
        /// Find TMP_InputField by GameObject name in children.
        /// </summary>
        private TMP_InputField FindInputFieldByName(string name, Transform parent)
        {
            if (parent == null) return null;

            string searchName = name.ToLower();
            
            // Search in direct children first
            foreach (Transform child in parent)
            {
                if (child.name.ToLower().IndexOf(searchName, StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    var inputField = child.GetComponent<TMP_InputField>();
                    if (inputField != null) return inputField;
                }
            }

            // Search in Content child if it exists
            Transform content = parent.Find("Content");
            if (content != null)
            {
                foreach (Transform child in content)
                {
                    if (child.name.ToLower().IndexOf(searchName, StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        var inputField = child.GetComponent<TMP_InputField>();
                        if (inputField != null) return inputField;
                    }
                }
            }

            // Deep search in all children (fallback)
            TMP_InputField[] allInputs = parent.GetComponentsInChildren<TMP_InputField>(true);
            foreach (var input in allInputs)
            {
                if (input.name.ToLower().IndexOf(searchName, StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    return input;
                }
            }

            return null;
        }

        /// <summary>
        /// Find TMP_InputField by placeholder text.
        /// </summary>
        private TMP_InputField FindInputFieldByPlaceholder(string placeholderText, Transform parent)
        {
            if (parent == null) return null;

            TMP_InputField[] allInputs = parent.GetComponentsInChildren<TMP_InputField>(true);
            string searchText = placeholderText.ToLower();
            
            foreach (var input in allInputs)
            {
                if (input.placeholder != null)
                {
                    string placeholder = input.placeholder.ToString().ToLower();
                    if (placeholder.IndexOf(searchText, StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        return input;
                    }
                }
            }
            
            return null;
        }

        #endregion
    }
}
