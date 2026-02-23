using System;
using System.Threading;
using System.Threading.Tasks;
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
    /// Register panel controller with production-ready features:
    /// - Comprehensive input validation (email, password, username)
    /// - OTP initiation flow
    /// - Referral code support
    /// - Focus visuals with DOTween animations
    /// - Proper error handling and user feedback
    /// </summary>
    public class IVXPanelRegister : MonoBehaviour
    {
        #region Serialized Fields

        [Header("Root")]
        [SerializeField] private GameObject _panel;
        [SerializeField] private CanvasGroup _canvasGroup;

        [Header("Input Fields")]
        [SerializeField] private TMP_InputField _emailInput;
        [SerializeField] private TMP_InputField _passwordInput;
        [SerializeField] private TMP_InputField _confirmPasswordInput;
        [SerializeField] private TMP_InputField _usernameInput;
        [SerializeField] private TMP_InputField _firstNameInput;
        [SerializeField] private TMP_InputField _lastNameInput;

        [Header("Input Focus Visuals")]
        [SerializeField] private Graphic _emailFocusFrame;
        [SerializeField] private Graphic _passwordFocusFrame;
        [SerializeField] private Graphic _confirmPasswordFocusFrame;
        [SerializeField] private Graphic _usernameFocusFrame;
        [SerializeField] private Graphic _firstNameFocusFrame;
        [SerializeField] private Graphic _lastNameFocusFrame;
        [SerializeField] private Color _focusFrameColor = new Color(0.24f, 0.62f, 0.99f, 1f);
        [SerializeField] private Color _idleFrameColor = new Color(1f, 1f, 1f, 0.16f);
        [SerializeField] private Color _caretFocusColor = new Color(0.24f, 0.62f, 0.99f, 1f);
        [SerializeField] private Color _caretIdleColor = new Color(1f, 1f, 1f, 0.85f);
#pragma warning disable CS0414
        [SerializeField] private float _focusFrameScale = 1.01f;
        [SerializeField] private float _focusAnimDuration = 0.12f;
#pragma warning restore CS0414

        [Header("Buttons")]
        [SerializeField] private Button _registerButton;
        [SerializeField] private Button _backToLoginButton;
        [SerializeField] private Button _referralButton;

        [Header("UI Elements")]
        [SerializeField] private TextMeshProUGUI _errorText;
        [SerializeField] private TextMeshProUGUI _statusText;
        [SerializeField] private TextMeshProUGUI _referralBadgeText;
        [SerializeField] private Toggle _termsToggle;

        [Header("Validation Settings")]
        [SerializeField] private int _minPasswordLength = 8;
        [SerializeField] private bool _requireUppercase = true;
        [SerializeField] private bool _requireLowercase = true;
        [SerializeField] private bool _requireNumber = true;
        [SerializeField] private bool _requireSpecialChar = true;
        [SerializeField] private int _minUsernameLength = 3;
        [SerializeField] private int _maxUsernameLength = 30;

        [Header("Error Display")]
        [SerializeField] private bool _hideInlineErrorText = true;

        #endregion

        #region Private Fields

        private IVXCanvasAuth _canvasAuth;
        private CancellationTokenSource _cts;
        private bool _isProcessing;
        private string _referralCodeOverride = string.Empty;
        private string _pendingEmail;

        private const string FIXED_ROLE = "user";
        private const string FIXED_FCM_TOKEN = "";
        private const string FIXED_FROM_DEVICE = "machine";
        private const string FIXED_MAC = "00:1A:2B:3C:4D:5E";

        #endregion

        #region Unity Lifecycle

        private void Awake()
        {
            _canvasAuth = GetComponentInParent<IVXCanvasAuth>();
            
            EnsureCanvasGroup();
            SetupPasswordField();
            SetupButtons();
            SetupFocusVisuals();
        }

        private void OnEnable()
        {
            ClearForm();
            DeactivateInputFieldsDelayed();
            _canvasAuth?.SubscribeReferralSubmit(OnReferralSubmitted);
        }

        private void OnDisable()
        {
            _canvasAuth?.UnsubscribeReferralSubmit(OnReferralSubmitted);
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
        /// Open the register panel
        /// </summary>
        public void Open()
        {
            if (_panel == null) return;
            _panel.SetActive(true);
            FadeIn();
            SetStatus("");
            ClearForm();
        }

        /// <summary>
        /// Close the register panel
        /// </summary>
        public void Close()
        {
            if (_panel == null) return;
            FadeOut(() => _panel.SetActive(false));
        }

        /// <summary>
        /// Attempt registration
        /// </summary>
        public async void Register()
        {
            if (_isProcessing) return;
            await OnClickRegisterAsync();
        }

        /// <summary>
        /// Navigate to login panel
        /// </summary>
        public void GoToLogin()
        {
            AnimateButton(_backToLoginButton);
            _canvasAuth?.ShowLogin();
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

        private void SetupPasswordField()
        {
            if (_passwordInput != null)
            {
                _passwordInput.contentType = TMP_InputField.ContentType.Password;
                _passwordInput.ForceLabelUpdate();
            }
            if (_confirmPasswordInput != null)
            {
                _confirmPasswordInput.contentType = TMP_InputField.ContentType.Password;
                _confirmPasswordInput.ForceLabelUpdate();
            }
        }

        private void SetupButtons()
        {
            _registerButton?.onClick.AddListener(() => _ = OnClickRegisterAsync());
            _backToLoginButton?.onClick.AddListener(GoToLogin);
            _referralButton?.onClick.AddListener(OnClickReferral);
        }

        private void SetupFocusVisuals()
        {
            WireInputFocus(_emailInput, _emailFocusFrame);
            WireInputFocus(_passwordInput, _passwordFocusFrame);
            WireInputFocus(_confirmPasswordInput, _confirmPasswordFocusFrame);
            WireInputFocus(_usernameInput, _usernameFocusFrame);
            WireInputFocus(_firstNameInput, _firstNameFocusFrame);
            WireInputFocus(_lastNameInput, _lastNameFocusFrame);
        }

        #endregion

        #region Registration Flow

        private async Task OnClickRegisterAsync()
        {
            if (_isProcessing) return;
            _isProcessing = true;
            AnimateButton(_registerButton);

            string email = _emailInput?.text?.Trim() ?? "";
            string password = _passwordInput?.text ?? "";
            string confirmPassword = _confirmPasswordInput?.text ?? "";
            string username = _usernameInput?.text?.Trim() ?? "";
            string firstName = _firstNameInput?.text?.Trim() ?? "";
            string lastName = _lastNameInput?.text?.Trim() ?? "";

            // Email validation
            if (!IsValidEmail(email))
            {
                ShowWarning("Please enter a valid email address.");
                _emailInput?.ActivateInputField();
                _isProcessing = false;
                return;
            }

            // Password validation
            var passwordValidation = ValidatePassword(password);
            if (!passwordValidation.isValid)
            {
                ShowWarning(passwordValidation.errorMessage);
                _passwordInput?.ActivateInputField();
                _isProcessing = false;
                return;
            }

            // Confirm password
            if (_confirmPasswordInput != null && password != confirmPassword)
            {
                ShowWarning("Passwords do not match.");
                _confirmPasswordInput?.ActivateInputField();
                _isProcessing = false;
                return;
            }

            // Username validation (if field exists)
            if (_usernameInput != null)
            {
                var usernameValidation = ValidateUsername(username);
                if (!usernameValidation.isValid)
                {
                    ShowWarning(usernameValidation.errorMessage);
                    _usernameInput?.ActivateInputField();
                    _isProcessing = false;
                    return;
                }
            }

            // Terms check
            if (_termsToggle != null && !_termsToggle.isOn)
            {
                ShowWarning("Please accept the terms and conditions.");
                _isProcessing = false;
                return;
            }

            _pendingEmail = email;

            SetInteractable(false);
            SetStatus("Sending verification code...");
            ClearError();

            CleanupCancellationToken();
            _cts = new CancellationTokenSource();
            var localCts = _cts;

            try
            {
                var resp = await APIManager.SignupInitiateAsync(email, password, FIXED_ROLE, localCts.Token);

                if (resp != null && resp.status)
                {
                    ShowSuccess("Verification code sent! Check your email.");
                    
                    var otpPanel = _canvasAuth?.OTPPanel;
                    if (otpPanel != null)
                    {
                        otpPanel.SetRegistrationData(email, password, username, firstName, lastName, _referralCodeOverride);
                    }
                    
                    _canvasAuth?.ShowOTP();
                }
                else
                {
                    var serverMsg = resp?.message?.Trim();
                    var message = !string.IsNullOrWhiteSpace(serverMsg) 
                        ? MapToUserFriendlyError(serverMsg) 
                        : "Couldn't send verification code. Please try again.";
                    ShowError(message);
                }
            }
            catch (OperationCanceledException)
            {
                // Silent cancellation
            }
            catch (Exception ex)
            {
                Debug.LogError($"[{nameof(IVXPanelRegister)}] Registration error: {ex.Message}");
                var msg = ExtractMeaningfulApiMessage(ex.Message, "Couldn't send verification code. Please try again.");
                ShowError(msg);
            }
            finally
            {
                SetInteractable(true);
                _isProcessing = false;
            }
        }

        #endregion

        #region Referral

        private void OnClickReferral()
        {
            AnimateButton(_referralButton);
            _canvasAuth?.ShowReferralPopup(_referralCodeOverride);
        }

        private void OnReferralSubmitted(string code)
        {
            _referralCodeOverride = code?.Trim() ?? string.Empty;

            if (_referralBadgeText != null)
            {
                _referralBadgeText.text = !string.IsNullOrEmpty(_referralCodeOverride)
                    ? $"Referral: {_referralCodeOverride}"
                    : "";
            }

            if (!string.IsNullOrEmpty(_referralCodeOverride))
            {
                SetStatus("Referral code applied.");
            }
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

        private (bool isValid, string errorMessage) ValidatePassword(string password)
        {
            if (string.IsNullOrEmpty(password))
                return (false, "Password is required.");

            if (string.IsNullOrWhiteSpace(password))
                return (false, "Password cannot contain only spaces.");

            if (password.Length < _minPasswordLength)
                return (false, $"Password must be at least {_minPasswordLength} characters long.");

            if (password.Length > 128)
                return (false, "Password must be 128 characters or less.");

            bool hasUppercase = false;
            bool hasLowercase = false;
            bool hasDigit = false;
            bool hasSpecialChar = false;

            foreach (char c in password)
            {
                if (char.IsUpper(c)) hasUppercase = true;
                else if (char.IsLower(c)) hasLowercase = true;
                else if (char.IsDigit(c)) hasDigit = true;
                else if (!char.IsWhiteSpace(c)) hasSpecialChar = true;
            }

            if (_requireUppercase && !hasUppercase)
                return (false, "Password must contain at least one uppercase letter (A-Z).");

            if (_requireLowercase && !hasLowercase)
                return (false, "Password must contain at least one lowercase letter (a-z).");

            if (_requireNumber && !hasDigit)
                return (false, "Password must contain at least one number (0-9).");

            if (_requireSpecialChar && !hasSpecialChar)
                return (false, "Password must contain at least one special character (@, #, $, etc.).");

            return (true, null);
        }

        private (bool isValid, string errorMessage) ValidateUsername(string username)
        {
            if (string.IsNullOrEmpty(username))
                return (false, "Username is required.");

            if (string.IsNullOrWhiteSpace(username))
                return (false, "Username cannot contain only spaces.");

            if (username != username.Trim())
                return (false, "Username cannot start or end with spaces.");

            if (username.Length < _minUsernameLength)
                return (false, $"Username must be at least {_minUsernameLength} characters.");

            if (username.Length > _maxUsernameLength)
                return (false, $"Username must be {_maxUsernameLength} characters or less.");

            foreach (char c in username)
            {
                if (!char.IsLetterOrDigit(c) && c != '_' && c != '-' && c != '.')
                    return (false, "Username can only contain letters, numbers, underscores, dashes, and periods.");
            }

            if (!char.IsLetterOrDigit(username[0]))
                return (false, "Username must start with a letter or number.");

            for (int i = 0; i < username.Length - 1; i++)
            {
                char current = username[i];
                char next = username[i + 1];
                if ((current == '_' || current == '-' || current == '.') &&
                    (next == '_' || next == '-' || next == '.'))
                    return (false, "Username cannot have consecutive special characters.");
            }

            return (true, null);
        }

        #endregion

        #region Error Mapping

        private string MapToUserFriendlyError(string backendMsg)
        {
            if (string.IsNullOrWhiteSpace(backendMsg))
                return "An error occurred. Please try again.";

            var m = backendMsg.ToLowerInvariant();

            if (m.Contains("email"))
            {
                if (m.Contains("exist") || m.Contains("already") || m.Contains("taken"))
                    return "This email is already registered. Please log in or use a different email.";
                if (m.Contains("invalid") || m.Contains("format"))
                    return "Please enter a valid email address.";
                return "Email error. Please check your email address.";
            }

            if (m.Contains("username") || m.Contains("user name"))
            {
                if (m.Contains("exist") || m.Contains("already") || m.Contains("taken"))
                    return "This username is already taken. Please choose a different one.";
                if (m.Contains("invalid") || m.Contains("format"))
                    return "Username format is invalid. Use letters, numbers, and underscores only.";
                return "Username error. Please try a different username.";
            }

            if (m.Contains("password"))
            {
                if (m.Contains("weak") || m.Contains("strength"))
                    return "Password is too weak. Please use a stronger password.";
                return "Password error. Please check your password.";
            }

            if ((m.Contains("user") || m.Contains("account")) &&
                (m.Contains("exist") || m.Contains("already")))
                return "An account with this information already exists. Please log in.";

            if (m.Contains("rate") || m.Contains("limit") || m.Contains("too many"))
                return "Too many requests. Please wait a moment and try again.";

            if (m.Contains("network") || m.Contains("connection") || m.Contains("timeout"))
                return "Connection error. Please check your internet and try again.";

            if (m.Contains("server") || m.Contains("internal"))
                return "Server error. Please try again later.";

            return backendMsg.Length <= 100 ? backendMsg : "An error occurred. Please try again.";
        }

        private static string ExtractMeaningfulApiMessage(string exMessage, string fallback)
        {
            if (string.IsNullOrWhiteSpace(exMessage)) return fallback;

            var low = exMessage.ToLowerInvariant();
            if ((low.Contains("exist") || low.Contains("already")) &&
                (low.Contains("user") || low.Contains("email") || low.Contains("account")))
            {
                return "User already exists.";
            }
            if (low.Contains("password")) return "Password doesn't meet requirements.";

            if (exMessage.Length <= 160 && !low.Contains("stack") && !low.Contains("exception"))
                return exMessage.Trim();

            return fallback;
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

        #region UI Helpers

        private void SetInteractable(bool enabled)
        {
            if (_registerButton) _registerButton.interactable = enabled;
            if (_backToLoginButton) _backToLoginButton.interactable = enabled;
            if (_referralButton) _referralButton.interactable = enabled;

            if (_canvasGroup != null)
            {
                _canvasGroup.interactable = enabled;
                _canvasGroup.blocksRaycasts = enabled;
            }
        }

        private void ShowError(string message)
        {
            if (!_hideInlineErrorText && _errorText != null)
            {
                _errorText.text = message;
                _errorText.gameObject.SetActive(true);
            }
            SetStatus(message);
            ShakePanel();
        }

        private void ShowWarning(string message)
        {
            if (!_hideInlineErrorText && _errorText != null)
            {
                _errorText.text = message;
                _errorText.gameObject.SetActive(true);
            }
            SetStatus(message);
            ShakePanel();
        }

        private void ShowSuccess(string message)
        {
            SetStatus(message);
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
            ClearError();
            if (_emailInput) _emailInput.text = "";
            if (_passwordInput) _passwordInput.text = "";
            if (_confirmPasswordInput) _confirmPasswordInput.text = "";
            if (_usernameInput) _usernameInput.text = "";
            if (_firstNameInput) _firstNameInput.text = "";
            if (_lastNameInput) _lastNameInput.text = "";
            if (_termsToggle) _termsToggle.isOn = false;
            _referralCodeOverride = string.Empty;
            if (_referralBadgeText) _referralBadgeText.text = "";
        }

        private void DeactivateInputFieldsDelayed()
        {
            StartCoroutine(DeactivateInputFieldsCoroutine());
        }

        private System.Collections.IEnumerator DeactivateInputFieldsCoroutine()
        {
            yield return new WaitForEndOfFrame();

            if (_emailInput != null && _emailInput.isFocused) _emailInput.DeactivateInputField();
            if (_passwordInput != null && _passwordInput.isFocused) _passwordInput.DeactivateInputField();
            if (_confirmPasswordInput != null && _confirmPasswordInput.isFocused) _confirmPasswordInput.DeactivateInputField();
            if (_usernameInput != null && _usernameInput.isFocused) _usernameInput.DeactivateInputField();
            if (_firstNameInput != null && _firstNameInput.isFocused) _firstNameInput.DeactivateInputField();
            if (_lastNameInput != null && _lastNameInput.isFocused) _lastNameInput.DeactivateInputField();
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
