using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

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
            ClearForm();
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Attempt registration with current form data
        /// </summary>
        public void Register()
        {
            if (_isProcessing) return;

            string displayName = _displayNameInput?.text?.Trim();
            string email = _emailInput?.text?.Trim();
            string password = _passwordInput?.text;
            string confirmPassword = _confirmPasswordInput?.text;

            // Validation
            if (string.IsNullOrEmpty(displayName))
            {
                ShowError("Please enter a display name");
                return;
            }

            if (string.IsNullOrEmpty(email))
            {
                ShowError("Please enter your email");
                return;
            }

            if (!IsValidEmail(email))
            {
                ShowError("Please enter a valid email address");
                return;
            }

            if (string.IsNullOrEmpty(password))
            {
                ShowError("Please enter a password");
                return;
            }

            var passwordValidation = ValidatePassword(password);
            if (!passwordValidation.isValid)
            {
                ShowError(passwordValidation.message);
                return;
            }

            if (password != confirmPassword)
            {
                ShowError("Passwords do not match");
                return;
            }

            if (_termsToggle != null && !_termsToggle.isOn)
            {
                ShowError("Please accept the terms and conditions");
                return;
            }

            _isProcessing = true;
            _canvasAuth?.ShowLoading();
            ClearError();

            // TODO: Implement actual registration with backend
            Debug.Log($"[{nameof(IVXPanelRegister)}] Registration attempt: {email}");

            ProcessRegistrationAsync(displayName, email, password);
        }

        /// <summary>
        /// Navigate back to login panel
        /// </summary>
        public void GoToLogin()
        {
            _canvasAuth?.ShowLogin();
        }

        #endregion

        #region Private Methods

        private void SetupButtons()
        {
            _registerButton?.onClick.AddListener(Register);
            _backToLoginButton?.onClick.AddListener(GoToLogin);
        }

        private async void ProcessRegistrationAsync(string displayName, string email, string password)
        {
            try
            {
                // TODO: Replace with actual backend call
                await System.Threading.Tasks.Task.Delay(1500);

                // After successful registration, show OTP verification
                Debug.Log($"[{nameof(IVXPanelRegister)}] Registration successful, showing OTP");
                _canvasAuth?.ShowOTP();
            }
            catch (Exception ex)
            {
                ShowError(ex.Message);
                _canvasAuth?.NotifyAuthFailed(ex.Message);
            }
            finally
            {
                _isProcessing = false;
                _canvasAuth?.HideLoading();
            }
        }

        private bool IsValidEmail(string email)
        {
            if (string.IsNullOrEmpty(email)) return false;

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
            {
                return (false, $"Password must be at least {_minPasswordLength} characters");
            }

            if (_requireUppercase && !System.Text.RegularExpressions.Regex.IsMatch(password, "[A-Z]"))
            {
                return (false, "Password must contain at least one uppercase letter");
            }

            if (_requireNumber && !System.Text.RegularExpressions.Regex.IsMatch(password, "[0-9]"))
            {
                return (false, "Password must contain at least one number");
            }

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
            if (_displayNameInput != null) _displayNameInput.text = "";
            if (_emailInput != null) _emailInput.text = "";
            if (_passwordInput != null) _passwordInput.text = "";
            if (_confirmPasswordInput != null) _confirmPasswordInput.text = "";
            if (_termsToggle != null) _termsToggle.isOn = false;
        }

        #endregion
    }
}
