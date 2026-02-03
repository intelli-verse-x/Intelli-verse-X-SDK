using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using IntelliVerseX.Identity;

namespace IntelliVerseX.Auth.UI
{
    /// <summary>
    /// Register panel controller for authentication.
    /// </summary>
    public class IVXPanelRegister : MonoBehaviour
    {
        #region Serialized Fields

        [Header("Input Fields")]
        [SerializeField] private TMP_InputField _displayNameInput;
        [SerializeField] private TMP_InputField _emailInput;
        [SerializeField] private TMP_InputField _passwordInput;
        [SerializeField] private TMP_InputField _confirmPasswordInput;

        [Header("Buttons")]
        [SerializeField] private Button _registerButton;
        [SerializeField] private Button _backToLoginButton;

        [Header("UI Elements")]
        [SerializeField] private TextMeshProUGUI _errorText;
        [SerializeField] private Toggle _termsToggle;

        [Header("Validation")]
        [SerializeField] private int _minPasswordLength = 8;
        [SerializeField] private bool _requireUppercase = true;
        [SerializeField] private bool _requireNumber = true;

        #endregion

        #region Private Fields

        private IVXCanvasAuth _canvasAuth;
        private AuthService _authService;
        private bool _isProcessing;
        private string _pendingEmail;

        #endregion

        #region Unity Lifecycle

        private void Awake()
        {
            _canvasAuth = GetComponentInParent<IVXCanvasAuth>();
            TryFindAuthService();

            if (_canvasAuth == null)
                Debug.LogError("[IVXPanelRegister] IVXCanvasAuth not found in parent!");
            // AuthService will be auto-created if not found, so no error needed

            SetupButtons();
        }

        private void OnEnable()
        {
            ClearForm();
            // Try to find AuthService again in case it was added to the scene
            if (_authService == null)
            {
                TryFindAuthService();
            }
        }

        #endregion

        #region Public Methods

        public void Register()
        {
            if (_isProcessing) return;

            string displayName = _displayNameInput?.text?.Trim();
            string email = _emailInput?.text?.Trim();
            string password = _passwordInput?.text;
            string confirmPassword = _confirmPasswordInput?.text;

            if (string.IsNullOrEmpty(displayName))
            {
                ShowError("Please enter a display name");
                return;
            }

            if (string.IsNullOrEmpty(email) || !IsValidEmail(email))
            {
                ShowError("Please enter a valid email");
                return;
            }

            if (string.IsNullOrEmpty(password))
            {
                ShowError("Please enter a password");
                return;
            }

            var validation = ValidatePassword(password);
            if (!validation.isValid)
            {
                ShowError(validation.message);
                return;
            }

            if (password != confirmPassword)
            {
                ShowError("Passwords do not match");
                return;
            }

            if (_termsToggle != null && !_termsToggle.isOn)
            {
                ShowError("Please accept terms & conditions");
                return;
            }

            _pendingEmail = email;
            _isProcessing = true;
            ClearError();
            _canvasAuth?.ShowLoading();

            Debug.Log($"[IVXPanelRegister] Register → {email}");

            // Try to find AuthService if not already found
            if (_authService == null)
            {
                TryFindAuthService();
            }

            if (_authService == null)
            {
                ShowError("Auth service not available. Please ensure AuthService component exists in the scene.");
                _isProcessing = false;
                _canvasAuth?.HideLoading();
                return;
            }

            // TODO: Registration requires OTP verification before login is allowed.
            // Backend sends OTP to email. User must verify OTP to activate account.
            _authService.Register(
                email,
                password,
                displayName,
                OnRegisterSuccess,
                OnRegisterError);
        }

        public void GoToLogin()
        {
            _canvasAuth?.ShowLogin();
        }

        #endregion

        #region Private Methods

        private void TryFindAuthService()
        {
            if (_authService == null)
            {
                _authService = FindObjectOfType<AuthService>();
                if (_authService == null)
                {
                    // Automatically create AuthService GameObject if it doesn't exist
                    GameObject authServiceObj = new GameObject("AuthService");
                    _authService = authServiceObj.AddComponent<AuthService>();
                    Debug.Log("[IVXPanelRegister] AuthService automatically created in scene.");
                }
                else
                {
                    Debug.Log("[IVXPanelRegister] AuthService found successfully.");
                }
            }
        }

        private void SetupButtons()
        {
            _registerButton?.onClick.AddListener(Register);
            _backToLoginButton?.onClick.AddListener(GoToLogin);
        }

        private void OnRegisterSuccess()
        {
            _isProcessing = false;
            _canvasAuth?.HideLoading();
            Debug.Log("[IVXPanelRegister] Backend OK → OTP sent → Opening OTP panel");
            
            // Registration success: Immediately open OTP panel (mandatory step)
            // DO NOT allow navigation to Login until OTP is verified
            _canvasAuth?.ShowOTP();
            var otpPanel = _canvasAuth?.GetComponentInChildren<IVXPanelOTP>(true);
            if (otpPanel != null)
            {
                otpPanel.SetEmail(_pendingEmail);
                Debug.Log($"[IVXPanelRegister] Email passed to OTP panel: {_pendingEmail}");
            }
            else
            {
                Debug.LogError("[IVXPanelRegister] OTP panel not found! User cannot verify OTP.");
            }
        }

        private void OnRegisterError(string message)
        {
            _isProcessing = false;
            _canvasAuth?.HideLoading();
            ShowError(message ?? "Registration failed");
            _canvasAuth?.NotifyAuthFailed(message ?? "Registration failed");
        }

        private bool IsValidEmail(string email)
        {
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

        private (bool isValid, string message) ValidatePassword(string password)
        {
            if (password.Length < _minPasswordLength)
                return (false, $"Password must be at least {_minPasswordLength} characters");

            if (_requireUppercase &&
                !System.Text.RegularExpressions.Regex.IsMatch(password, "[A-Z]"))
                return (false, "Password must contain an uppercase letter");

            if (_requireNumber &&
                !System.Text.RegularExpressions.Regex.IsMatch(password, "[0-9]"))
                return (false, "Password must contain a number");

            return (true, "");
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

        private void ClearForm()
        {
            ClearError();
            if (_displayNameInput) _displayNameInput.text = "";
            if (_emailInput) _emailInput.text = "";
            if (_passwordInput) _passwordInput.text = "";
            if (_confirmPasswordInput) _confirmPasswordInput.text = "";
            if (_termsToggle) _termsToggle.isOn = false;
        }

        #endregion
    }
}
