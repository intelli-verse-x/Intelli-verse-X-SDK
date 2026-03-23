using System;
using System.Threading;
using System.Threading.Tasks;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using IntelliVerseX.Identity;
#if DOTWEEN_ENABLED || DOTWEEN
using DG.Tweening;
#endif

namespace IntelliVerseX.Auth.UI
{
    /// <summary>
    /// Forgot password panel with two-step flow:
    /// Step 1: Request OTP by email
    /// Step 2: Verify OTP and set new password
    /// Includes resend timer and proper error handling.
    /// </summary>
    public class IVXPanelForgotPassword : MonoBehaviour
    {
        private const int OTP_LENGTH = 6;

        #region Serialized Fields

        [Header("Root")]
        [SerializeField] private GameObject _panel;
        [SerializeField] private CanvasGroup _canvasGroup;

        [Header("Step 1: Request OTP")]
        [SerializeField] private GameObject _step1Container;
        [SerializeField] private TMP_InputField _emailInput;
        [SerializeField] private Button _requestOTPButton;

        [Header("Step 2: Reset Password")]
        [SerializeField] private GameObject _step2Container;
        [SerializeField] private TMP_InputField _otpInput;
        [SerializeField] private TMP_InputField _newPasswordInput;
        [SerializeField] private TMP_InputField _confirmPasswordInput;
        [SerializeField] private Button _resetPasswordButton;
        [SerializeField] private Button _resendButton;
        [SerializeField] private TextMeshProUGUI _timerText;

        [Header("Input Focus Visuals")]
        [SerializeField] private Graphic _emailFocusFrame;
        [SerializeField] private Graphic _otpFocusFrame;
        [SerializeField] private Graphic _newPasswordFocusFrame;
        [SerializeField] private Graphic _confirmPasswordFocusFrame;
        [SerializeField] private Color _focusFrameColor = new Color(0.24f, 0.62f, 0.99f, 1f);
        [SerializeField] private Color _idleFrameColor = new Color(1f, 1f, 1f, 0.16f);
        [SerializeField] private Color _caretFocusColor = new Color(0.24f, 0.62f, 0.99f, 1f);
        [SerializeField] private Color _caretIdleColor = new Color(1f, 1f, 1f, 0.85f);
#pragma warning disable CS0414
        [SerializeField] private float _focusFrameScale = 1.01f;
        [SerializeField] private float _focusAnimDuration = 0.12f;
#pragma warning restore CS0414

        [Header("Password Toggle")]
        [SerializeField] private Button _passwordToggleButton;
        [SerializeField] private Image _passwordToggleIcon;
        [SerializeField] private Sprite _eyeClosedSprite;
        [SerializeField] private Sprite _eyeOpenSprite;

        [Header("Buttons")]
        [SerializeField] private Button _backToLoginButton;

        [Header("UI Elements")]
        [SerializeField] private TextMeshProUGUI _errorText;
        [SerializeField] private TextMeshProUGUI _statusText;
        [SerializeField] private TextMeshProUGUI _emailDisplayText;

        [Header("Settings")]
        [SerializeField] private float _resendDelaySeconds = 30f;
        [SerializeField] private int _minPasswordLength = 8;

        #endregion

        #region Private Fields

        private IVXCanvasAuth _canvasAuth;
        private CancellationTokenSource _cts;
        private Coroutine _timerCoroutine;
        private bool _isProcessing;
        private bool _passwordVisible;
        private string _storedEmail;
        private int _panelTransitionVersion;

        #endregion

        #region Unity Lifecycle

        private void Awake()
        {
            _canvasAuth = GetComponentInParent<IVXCanvasAuth>();
            
            EnsureCanvasGroup();
            SetupPasswordFields();
            SetupButtons();
            SetupFocusVisuals();
        }

        private void OnEnable()
        {
            ClearError();
            ShowStep1();
        }

        private void OnDisable()
        {
            StopResendTimer();
        }

        private void OnDestroy()
        {
            CleanupCancellationToken();
            
#if DOTWEEN_ENABLED || DOTWEEN
            if (_canvasGroup != null) _canvasGroup.DOKill();
            if (_panel != null) _panel.transform.DOKill();
#endif
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Bind to parent canvas auth
        /// </summary>
        public void Bind(IVXCanvasAuth owner) => _canvasAuth = owner;

        /// <summary>
        /// Open the forgot password panel
        /// </summary>
        public void Open()
        {
            if (_panel == null) return;
            _panelTransitionVersion++;
#if DOTWEEN_ENABLED || DOTWEEN
            _canvasGroup?.DOKill();
            _panel.transform.DOKill();
#endif
            _panel.SetActive(true);
            FadeIn();
            SetStatus("");
            ShowStep1();
        }

        /// <summary>
        /// Close the forgot password panel
        /// </summary>
        public void Close()
        {
            if (_panel == null) return;
            _panelTransitionVersion++;
            int closeVersion = _panelTransitionVersion;
#if DOTWEEN_ENABLED || DOTWEEN
            _canvasGroup?.DOKill();
            _panel.transform.DOKill();
#endif
            FadeOut(() =>
            {
                if (closeVersion != _panelTransitionVersion) return;
                _panel.SetActive(false);
            });
        }

        #endregion

        #region Setup Methods

        private void EnsureCanvasGroup()
        {
            if (_canvasGroup == null && _panel != null)
            {
                _canvasGroup = _panel.GetComponent<CanvasGroup>();
                if (_canvasGroup == null)
                    _canvasGroup = _panel.AddComponent<CanvasGroup>();
            }
        }

        private void SetupPasswordFields()
        {
            if (_newPasswordInput != null)
            {
                _newPasswordInput.contentType = TMP_InputField.ContentType.Password;
                _newPasswordInput.ForceLabelUpdate();
            }
            if (_confirmPasswordInput != null)
            {
                _confirmPasswordInput.contentType = TMP_InputField.ContentType.Password;
                _confirmPasswordInput.ForceLabelUpdate();
            }
        }

        private void SetupButtons()
        {
            _requestOTPButton?.onClick.AddListener(() => _ = RequestOTPAsync());
            _resetPasswordButton?.onClick.AddListener(() => _ = ResetPasswordAsync());
            _resendButton?.onClick.AddListener(() => _ = ResendOTPAsync());
            _backToLoginButton?.onClick.AddListener(GoToLogin);
            _passwordToggleButton?.onClick.AddListener(TogglePasswordVisibility);
        }

        private void SetupFocusVisuals()
        {
            WireInputFocus(_emailInput, _emailFocusFrame);
            WireInputFocus(_otpInput, _otpFocusFrame);
            WireInputFocus(_newPasswordInput, _newPasswordFocusFrame);
            WireInputFocus(_confirmPasswordInput, _confirmPasswordFocusFrame);
        }

        #endregion

        #region Step Navigation

        private void ShowStep1()
        {
            if (_step1Container != null) _step1Container.SetActive(true);
            if (_step2Container != null) _step2Container.SetActive(false);
            ClearForm();
            _emailInput?.ActivateInputField();
        }

        private void ShowStep2()
        {
            if (_step1Container != null) _step1Container.SetActive(false);
            if (_step2Container != null) _step2Container.SetActive(true);
            UpdateEmailDisplay();
            StartResendTimer();
            _otpInput?.ActivateInputField();
        }

        #endregion

        #region Request OTP (Step 1)

        private async Task RequestOTPAsync()
        {
            if (_isProcessing) return;
            _isProcessing = true;
            AnimateButton(_requestOTPButton);

            string email = _emailInput?.text?.Trim() ?? "";

            if (!IsValidEmail(email))
            {
                ShowError("Please enter a valid email address.");
                _emailInput?.ActivateInputField();
                _isProcessing = false;
                return;
            }

            SetInteractable(false);
            SetStatus("Sending reset code...");
            ClearError();

            CleanupCancellationToken();
            _cts = new CancellationTokenSource();

            try
            {
                var result = await APIManager.ForgotPasswordAsync(email, _cts.Token);

                if (result.status)
                {
                    _storedEmail = email;
                    SetStatus("Reset code sent to your email.");
                    ShowStep2();
                }
                else
                {
                    ShowError(result.message ?? "Failed to send reset code.");
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"[{nameof(IVXPanelForgotPassword)}] RequestOTP error: {ex.Message}");
                ShowError("Failed to send reset code. Please try again.");
            }
            finally
            {
                SetInteractable(true);
                _isProcessing = false;
            }
        }

        #endregion

        #region Reset Password (Step 2)

        private async Task ResetPasswordAsync()
        {
            if (_isProcessing) return;
            _isProcessing = true;
            AnimateButton(_resetPasswordButton);

            string otp = _otpInput?.text?.Trim() ?? "";
            string newPassword = _newPasswordInput?.text ?? "";
            string confirmPassword = _confirmPasswordInput?.text ?? "";

            if (!IsValidOTP(otp))
            {
                ShowError("Please enter a valid 6-digit code.");
                ClearOTPInput();
                _otpInput?.ActivateInputField();
                _isProcessing = false;
                return;
            }

            var passwordValidation = ValidatePassword(newPassword);
            if (!passwordValidation.isValid)
            {
                ShowError(passwordValidation.errorMessage);
                _newPasswordInput?.ActivateInputField();
                _isProcessing = false;
                return;
            }

            if (newPassword != confirmPassword)
            {
                ShowError("Passwords do not match.");
                _confirmPasswordInput?.ActivateInputField();
                _isProcessing = false;
                return;
            }

            SetInteractable(false);
            SetStatus("Resetting password...");
            ClearError();

            CleanupCancellationToken();
            _cts = new CancellationTokenSource();

            try
            {
                var result = await APIManager.ResetPasswordAsync(_storedEmail, otp, newPassword, _cts.Token);

                if (result.status)
                {
                    SetStatus("Password reset successfully!");
                    await Task.Delay(1500, _cts.Token);
                    _canvasAuth?.ShowLogin();
                }
                else
                {
                    ClearOTPInput();
                    ShowError(MapResetErrorToFriendly(result.message));
                }
            }
            catch (OperationCanceledException)
            {
                // Silent
            }
            catch (Exception ex)
            {
                Debug.LogError($"[{nameof(IVXPanelForgotPassword)}] ResetPassword error: {ex.Message}");
                ClearOTPInput();
                ShowError("Failed to reset password. Please try again.");
            }
            finally
            {
                SetInteractable(true);
                _isProcessing = false;
            }
        }

        #endregion

        #region Resend OTP

        private async Task ResendOTPAsync()
        {
            if (string.IsNullOrEmpty(_storedEmail))
            {
                ShowError("Email missing. Please go back and try again.");
                return;
            }

            SetStatus("Resending reset code...");
            _resendButton.interactable = false;

            try
            {
                var result = await APIManager.ForgotPasswordAsync(_storedEmail, CancellationToken.None);

                if (result.status)
                {
                    SetStatus("Reset code sent!");
                    ClearOTPInput();
                }
                else
                {
                    ShowError(result.message ?? "Failed to resend code.");
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"[{nameof(IVXPanelForgotPassword)}] Resend error: {ex.Message}");
                ShowError("Failed to resend code.");
            }

            StartResendTimer();
        }

        private void StartResendTimer()
        {
            StopResendTimer();
            _timerCoroutine = StartCoroutine(TimerCoroutine());
        }

        private void StopResendTimer()
        {
            if (_timerCoroutine != null)
            {
                StopCoroutine(_timerCoroutine);
                _timerCoroutine = null;
            }
        }

        private IEnumerator TimerCoroutine()
        {
            if (_resendButton != null)
                _resendButton.interactable = false;

            float currentTime = _resendDelaySeconds;

            while (currentTime > 0)
            {
                if (_timerText != null)
                    _timerText.text = $"Resend in {Mathf.CeilToInt(currentTime)}s";

                currentTime -= Time.deltaTime;
                yield return null;
            }

            if (_timerText != null)
                _timerText.text = "";

            if (_resendButton != null)
                _resendButton.interactable = true;
        }

        #endregion

        #region Password Toggle

        private void TogglePasswordVisibility()
        {
            _passwordVisible = !_passwordVisible;
            
            if (_newPasswordInput != null)
            {
                _newPasswordInput.contentType = _passwordVisible
                    ? TMP_InputField.ContentType.Standard
                    : TMP_InputField.ContentType.Password;
                _newPasswordInput.ForceLabelUpdate();
            }
            
            if (_confirmPasswordInput != null)
            {
                _confirmPasswordInput.contentType = _passwordVisible
                    ? TMP_InputField.ContentType.Standard
                    : TMP_InputField.ContentType.Password;
                _confirmPasswordInput.ForceLabelUpdate();
            }
            
            UpdatePasswordToggleVisual();
        }

        private void UpdatePasswordToggleVisual()
        {
            if (_passwordToggleIcon == null) return;
            try
            {
                _passwordToggleIcon.sprite = _passwordVisible ? _eyeOpenSprite : _eyeClosedSprite;
            }
            catch { }
        }

        #endregion

        #region Validation

        private bool IsValidEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email)) return false;

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

        private bool IsValidOTP(string otp)
        {
            if (string.IsNullOrWhiteSpace(otp)) return false;
            if (otp.Length != OTP_LENGTH) return false;

            foreach (char c in otp)
            {
                if (!char.IsDigit(c)) return false;
            }

            return true;
        }

        private (bool isValid, string errorMessage) ValidatePassword(string password)
        {
            if (string.IsNullOrEmpty(password))
                return (false, "Password is required.");

            if (password.Length < _minPasswordLength)
                return (false, $"Password must be at least {_minPasswordLength} characters.");

            return (true, null);
        }

        private string MapResetErrorToFriendly(string message)
        {
            if (string.IsNullOrWhiteSpace(message))
                return "Password reset failed. Please try again.";

            var m = message.ToLowerInvariant();

            if (m.Contains("expired"))
                return "Reset code has expired. Please request a new one.";

            if (m.Contains("invalid") || m.Contains("incorrect") || m.Contains("wrong"))
                return "Invalid reset code. Please check and try again.";

            if (m.Contains("not found"))
                return "Account not found. Please check your email.";

            return message.Length <= 100 ? message : "Password reset failed. Please try again.";
        }

        #endregion

        #region UI Helpers

        private void GoToLogin()
        {
            AnimateButton(_backToLoginButton);
            Close();
            _canvasAuth?.ShowLogin();
        }

        private void SetInteractable(bool enabled)
        {
            if (_requestOTPButton) _requestOTPButton.interactable = enabled;
            if (_resetPasswordButton) _resetPasswordButton.interactable = enabled;
            if (_backToLoginButton) _backToLoginButton.interactable = enabled;

            if (_canvasGroup != null)
            {
                _canvasGroup.interactable = enabled;
                _canvasGroup.blocksRaycasts = enabled;
            }
        }

        private void ShowError(string message)
        {
            if (_errorText != null)
            {
                _errorText.text = message;
                _errorText.gameObject.SetActive(true);
            }
            ShakePanel();
        }

        private void ClearError()
        {
            if (_errorText != null)
            {
                _errorText.text = "";
                _errorText.gameObject.SetActive(false);
            }
        }

        private void SetStatus(string msg)
        {
            if (_statusText != null)
                _statusText.text = msg ?? "";
        }

        private void ClearForm()
        {
            if (_emailInput) _emailInput.text = "";
            ClearOTPInput();
            if (_newPasswordInput) _newPasswordInput.text = "";
            if (_confirmPasswordInput) _confirmPasswordInput.text = "";
            _storedEmail = "";
        }

        private void ClearOTPInput()
        {
            if (_otpInput) _otpInput.text = "";
        }

        private void UpdateEmailDisplay()
        {
            if (_emailDisplayText != null && !string.IsNullOrEmpty(_storedEmail))
            {
                _emailDisplayText.text = $"Code sent to: {_storedEmail}";
            }
        }

        private void ShakePanel()
        {
#if DOTWEEN_ENABLED || DOTWEEN
            if (_panel != null)
            {
                var t = _panel.transform;
                t.DOKill();
                t.DOShakePosition(0.25f, 8f, 18, 90f, false, true);
            }
#endif
        }

        private void FadeIn()
        {
#if DOTWEEN_ENABLED || DOTWEEN
            if (_canvasGroup != null)
            {
                _canvasGroup.alpha = 0f;
                _canvasGroup.DOFade(1f, 0.18f);
            }
            if (_panel != null)
            {
                _panel.transform.localScale = Vector3.one * 0.97f;
                _panel.transform.DOScale(1f, 0.18f).SetEase(Ease.OutQuad);
            }
#endif
        }

        private void FadeOut(Action onComplete)
        {
#if DOTWEEN_ENABLED || DOTWEEN
            Tween t1 = null;
            if (_canvasGroup != null) t1 = _canvasGroup.DOFade(0f, 0.15f);
            var t2 = _panel != null ? _panel.transform.DOScale(0.97f, 0.15f).SetEase(Ease.InQuad) : null;

            if (t1 != null) t1.OnComplete(() => onComplete?.Invoke());
            else if (t2 != null) t2.OnComplete(() => onComplete?.Invoke());
            else onComplete?.Invoke();
#else
            onComplete?.Invoke();
#endif
        }

        private void AnimateButton(Button b)
        {
#if DOTWEEN_ENABLED || DOTWEEN
            if (b == null) return;
            var tr = b.transform;
            tr.DOKill();
            tr.DOPunchScale(Vector3.one * -0.05f, 0.12f, 8, 1f);
#endif
        }

        #endregion

        #region Focus Visuals

        private void WireInputFocus(TMP_InputField field, Graphic frame)
        {
            if (field == null) return;

            SetFieldFocusVisual(field, frame, focused: false, instant: true);

            field.onSelect.AddListener(_ => SetFieldFocusVisual(field, frame, focused: true, instant: false));
            field.onDeselect.AddListener(_ => SetFieldFocusVisual(field, frame, focused: false, instant: false));
            field.onEndEdit.AddListener(_ => SetFieldFocusVisual(field, frame, focused: false, instant: false));
        }

        private void SetFieldFocusVisual(TMP_InputField field, Graphic frame, bool focused, bool instant)
        {
#if DOTWEEN_ENABLED || DOTWEEN
            if (frame != null)
            {
                frame.DOKill();
                if (!instant)
                    frame.DOColor(focused ? _focusFrameColor : _idleFrameColor, _focusAnimDuration);
                else
                    frame.color = focused ? _focusFrameColor : _idleFrameColor;

                var rt = frame.rectTransform;
                if (rt != null)
                {
                    rt.DOKill();
                    if (!instant)
                        rt.DOScale(focused ? _focusFrameScale : 1f, _focusAnimDuration).SetEase(Ease.OutQuad);
                    else
                        rt.localScale = Vector3.one * (focused ? _focusFrameScale : 1f);
                }
            }
#else
            if (frame != null)
                frame.color = focused ? _focusFrameColor : _idleFrameColor;
#endif

            if (field != null)
            {
                field.caretColor = focused ? _caretFocusColor : _caretIdleColor;
                var sel = focused ? _caretFocusColor : _caretIdleColor;
                sel.a = 0.28f;
                field.selectionColor = sel;
                field.caretWidth = focused ? 2 : 1;
            }
        }

        #endregion

        #region Cleanup

        private void CleanupCancellationToken()
        {
            try { _cts?.Cancel(); } catch { }
            try { _cts?.Dispose(); } catch { }
            _cts = null;
        }

        #endregion
    }
}
