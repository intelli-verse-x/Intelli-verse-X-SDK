using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace IntelliVerseX.Auth.UI
{
    /// <summary>
    /// OTP verification panel controller.
    /// </summary>
    public class IVXPanelOTP : MonoBehaviour
    {
        #region Serialized Fields

        [Header("OTP Input")]
        [SerializeField] private TMP_InputField[] _otpInputs;
        [SerializeField] private TMP_InputField _singleOtpInput;

        [Header("Buttons")]
        [SerializeField] private Button _verifyButton;
        [SerializeField] private Button _resendButton;
        [SerializeField] private Button _backButton;

        [Header("UI Elements")]
        [SerializeField] private TextMeshProUGUI _errorText;
        [SerializeField] private TextMeshProUGUI _timerText;
        [SerializeField] private TextMeshProUGUI _emailText;

        [Header("Settings")]
        [SerializeField] private int _otpLength = 6;
        [SerializeField] private float _resendCooldown = 60f;

        #endregion

        #region Private Fields

        private IVXCanvasAuth _canvasAuth;
        private bool _isProcessing = false;
        private float _resendTimer = 0f;
        private string _pendingEmail = "";

        #endregion

        #region Unity Lifecycle

        private void Awake()
        {
            _canvasAuth = GetComponentInParent<IVXCanvasAuth>();
            SetupButtons();
            SetupOtpInputs();
        }

        private void OnEnable()
        {
            ClearError();
            ClearOtpInputs();
            StartResendCooldown();
        }

        private void Update()
        {
            UpdateResendTimer();
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Set the email being verified
        /// </summary>
        public void SetEmail(string email)
        {
            _pendingEmail = email;
            if (_emailText != null)
            {
                _emailText.text = $"Enter the code sent to {MaskEmail(email)}";
            }
        }

        /// <summary>
        /// Verify the entered OTP
        /// </summary>
        public void VerifyOTP()
        {
            if (_isProcessing) return;

            string otp = GetOtpValue();

            if (string.IsNullOrEmpty(otp) || otp.Length != _otpLength)
            {
                ShowError($"Please enter a {_otpLength}-digit code");
                return;
            }

            _isProcessing = true;
            _canvasAuth?.ShowLoading();
            ClearError();

            Debug.Log($"[{nameof(IVXPanelOTP)}] Verifying OTP: {otp}");
            ProcessVerificationAsync(otp);
        }

        /// <summary>
        /// Resend OTP code
        /// </summary>
        public void ResendOTP()
        {
            if (_resendTimer > 0) return;

            Debug.Log($"[{nameof(IVXPanelOTP)}] Resending OTP to {_pendingEmail}");
            StartResendCooldown();

            // TODO: Implement resend OTP with backend
        }

        /// <summary>
        /// Go back to previous panel
        /// </summary>
        public void GoBack()
        {
            _canvasAuth?.ShowRegister();
        }

        #endregion

        #region Private Methods

        private void SetupButtons()
        {
            _verifyButton?.onClick.AddListener(VerifyOTP);
            _resendButton?.onClick.AddListener(ResendOTP);
            _backButton?.onClick.AddListener(GoBack);
        }

        private void SetupOtpInputs()
        {
            if (_otpInputs != null && _otpInputs.Length > 0)
            {
                for (int i = 0; i < _otpInputs.Length; i++)
                {
                    int index = i;
                    var input = _otpInputs[i];
                    if (input == null) continue;

                    input.characterLimit = 1;
                    input.contentType = TMP_InputField.ContentType.IntegerNumber;
                    input.onValueChanged.AddListener((value) => OnOtpInputChanged(index, value));
                }
            }
            else if (_singleOtpInput != null)
            {
                _singleOtpInput.characterLimit = _otpLength;
                _singleOtpInput.contentType = TMP_InputField.ContentType.IntegerNumber;
            }
        }

        private void OnOtpInputChanged(int index, string value)
        {
            if (_otpInputs == null) return;

            // Auto-focus next input when digit entered
            if (!string.IsNullOrEmpty(value) && index < _otpInputs.Length - 1)
            {
                _otpInputs[index + 1]?.Select();
            }
        }

        private string GetOtpValue()
        {
            if (_singleOtpInput != null)
            {
                return _singleOtpInput.text?.Trim();
            }

            if (_otpInputs != null && _otpInputs.Length > 0)
            {
                var sb = new System.Text.StringBuilder();
                foreach (var input in _otpInputs)
                {
                    if (input != null)
                    {
                        sb.Append(input.text);
                    }
                }
                return sb.ToString();
            }

            return "";
        }

        private void ClearOtpInputs()
        {
            if (_singleOtpInput != null)
            {
                _singleOtpInput.text = "";
            }

            if (_otpInputs != null)
            {
                foreach (var input in _otpInputs)
                {
                    if (input != null)
                    {
                        input.text = "";
                    }
                }

                // Focus first input
                if (_otpInputs.Length > 0 && _otpInputs[0] != null)
                {
                    _otpInputs[0].Select();
                }
            }
        }

        private async void ProcessVerificationAsync(string otp)
        {
            try
            {
                // TODO: Replace with actual backend call
                await System.Threading.Tasks.Task.Delay(1000);

                var result = new AuthResult
                {
                    UserId = Guid.NewGuid().ToString(),
                    Email = _pendingEmail,
                    DisplayName = _pendingEmail.Split('@')[0],
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
                _canvasAuth?.HideLoading();
            }
        }

        private void StartResendCooldown()
        {
            _resendTimer = _resendCooldown;
            UpdateResendButtonState();
        }

        private void UpdateResendTimer()
        {
            if (_resendTimer > 0)
            {
                _resendTimer -= Time.deltaTime;
                UpdateResendButtonState();

                if (_timerText != null)
                {
                    _timerText.text = $"Resend in {Mathf.CeilToInt(_resendTimer)}s";
                }
            }
            else if (_timerText != null)
            {
                _timerText.text = "";
            }
        }

        private void UpdateResendButtonState()
        {
            if (_resendButton != null)
            {
                _resendButton.interactable = _resendTimer <= 0;
            }
        }

        private string MaskEmail(string email)
        {
            if (string.IsNullOrEmpty(email) || !email.Contains("@"))
                return email;

            var parts = email.Split('@');
            if (parts[0].Length <= 2)
                return email;

            var masked = parts[0][0] + "***" + parts[0][parts[0].Length - 1] + "@" + parts[1];
            return masked;
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

        #endregion
    }
}
