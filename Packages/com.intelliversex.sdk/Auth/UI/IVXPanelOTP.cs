using System;
using System.Threading;
using System.Threading.Tasks;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using IntelliVerseX.Identity;
#if DOTWEEN_ENABLED || DOTWEEN
using DG.Tweening;
#endif

namespace IntelliVerseX.Auth.UI
{
    /// <summary>
    /// OTP verification panel with production-ready features:
    /// - 6-digit OTP validation
    /// - Resend OTP with countdown timer
    /// - Focus visuals with DOTween animations
    /// - Proper error handling and user feedback
    /// - Scene loading after successful verification
    /// </summary>
    public class IVXPanelOTP : MonoBehaviour
    {
        private const int OTP_LENGTH = 6;

        #region Serialized Fields

        [Header("Root")]
        [SerializeField] private GameObject _panel;
        [SerializeField] private CanvasGroup _canvasGroup;

        [Header("Input")]
        [SerializeField] private TMP_InputField _otpInput;
        [SerializeField] private TMP_InputField[] _otpInputs;

        [Header("Input Focus Visuals")]
        [SerializeField] private Graphic[] _otpFocusFrames;
        [SerializeField] private Color _focusFrameColor = new Color(0.24f, 0.62f, 0.99f, 1f);
        [SerializeField] private Color _idleFrameColor = new Color(1f, 1f, 1f, 0.16f);
        [SerializeField] private Color _caretFocusColor = new Color(0.24f, 0.62f, 0.99f, 1f);
        [SerializeField] private Color _caretIdleColor = new Color(1f, 1f, 1f, 0.85f);
#pragma warning disable CS0414
        [SerializeField] private float _focusFrameScale = 1.01f;
        [SerializeField] private float _focusAnimDuration = 0.12f;
#pragma warning restore CS0414

        [Header("Buttons")]
        [SerializeField] private Button _verifyButton;
        [SerializeField] private Button _resendButton;
        [SerializeField] private Button _closeButton;

        [Header("UI Elements")]
        [SerializeField] private TextMeshProUGUI _errorText;
        [SerializeField] private TextMeshProUGUI _statusText;
        [SerializeField] private TextMeshProUGUI _timerText;
        [SerializeField] private TextMeshProUGUI _emailDisplayText;

        [Header("Resend Timer")]
        [SerializeField] private float _resendDelaySeconds = 30f;

        [Header("Scene Loading")]
        [SerializeField] private string _gameSceneName = string.Empty;
        [SerializeField] private int _gameSceneBuildIndex = 1;
        [SerializeField] private bool _autoLoadGameScene = false;

        #endregion

        #region Private Fields

        private IVXCanvasAuth _canvasAuth;
        private AuthService _authService;
        private CancellationTokenSource _cts;
        private Coroutine _timerCoroutine;
        private bool _isProcessing;
        private int _panelTransitionVersion;

        private string _email;
        private string _password;
        private string _username;
        private string _firstName;
        private string _lastName;
        private string _referralCode;

        private const string FIXED_ROLE = "user";
        private const string FIXED_FCM_TOKEN = "";

        #endregion

        #region Unity Lifecycle

        private void Awake()
        {
            _canvasAuth = GetComponentInParent<IVXCanvasAuth>();
            TryFindAuthService();
            
            EnsureCanvasGroup();
            SetupButtons();
            SetupFocusVisuals();
        }

        private void OnEnable()
        {
            ClearError();
            ClearOTPInput();
            StartResendTimer();
            UpdateEmailDisplay();

            if (_authService == null)
                TryFindAuthService();
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
        /// Open the OTP panel
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
            UpdateEmailDisplay();
            FirstEnabledInput()?.ActivateInputField();
        }

        /// <summary>
        /// Close the OTP panel
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

        /// <summary>
        /// Set the email for OTP verification
        /// </summary>
        public void SetEmail(string email)
        {
            _email = email ?? string.Empty;
            UpdateEmailDisplay();
        }

        /// <summary>
        /// Set all registration data for OTP confirmation
        /// </summary>
        public void SetRegistrationData(string email, string password, string username, 
            string firstName, string lastName, string referralCode)
        {
            _email = email ?? string.Empty;
            _password = password ?? string.Empty;
            _username = username ?? string.Empty;
            _firstName = firstName ?? string.Empty;
            _lastName = lastName ?? string.Empty;
            _referralCode = referralCode ?? string.Empty;
            UpdateEmailDisplay();
        }

        /// <summary>
        /// Set the game scene name to load after verification
        /// </summary>
        public void SetGameSceneName(string sceneName)
        {
            _gameSceneName = sceneName ?? string.Empty;
        }

        /// <summary>
        /// Enable/disable auto scene loading
        /// </summary>
        public void SetAutoLoadGameScene(bool autoLoad)
        {
            _autoLoadGameScene = autoLoad;
        }

        /// <summary>
        /// Verify the OTP
        /// </summary>
        public async void Verify()
        {
            if (_isProcessing) return;
            await OnClickVerifyAsync();
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

        private void SetupButtons()
        {
            _verifyButton?.onClick.AddListener(() => _ = OnClickVerifyAsync());
            _resendButton?.onClick.AddListener(OnClickResend);
            _closeButton?.onClick.AddListener(OnClickClose);
        }

        private void SetupFocusVisuals()
        {
            if (_otpInputs != null && _otpInputs.Length > 0)
            {
                for (int i = 0; i < _otpInputs.Length; i++)
                {
                    var field = _otpInputs[i];
                    var frame = (_otpFocusFrames != null && i < _otpFocusFrames.Length) 
                        ? _otpFocusFrames[i] 
                        : null;
                    WireInputFocus(field, frame);
                }
            }
            else if (_otpInput != null)
            {
                var frame = (_otpFocusFrames != null && _otpFocusFrames.Length > 0) 
                    ? _otpFocusFrames[0] 
                    : null;
                WireInputFocus(_otpInput, frame);
            }
        }

        private void TryFindAuthService()
        {
            if (_authService == null)
            {
                _authService = FindObjectOfType<AuthService>();
                if (_authService == null)
                {
                    GameObject authServiceObj = new GameObject("AuthService");
                    _authService = authServiceObj.AddComponent<AuthService>();
                    Debug.Log($"[{nameof(IVXPanelOTP)}] AuthService automatically created.");
                }
            }
        }

        #endregion

        #region Verification Flow

        private async Task OnClickVerifyAsync()
        {
            if (_isProcessing) return;
            _isProcessing = true;
            AnimateButton(_verifyButton);

            string otp = GetOTPValue();

            if (string.IsNullOrEmpty(_email))
            {
                ShowError("Email missing. Please start from registration.");
                _isProcessing = false;
                return;
            }

            if (!IsValidOTP(otp))
            {
                ShowError("Please enter a valid 6-digit code.");
                ClearOTPInput();
                FirstEnabledInput()?.ActivateInputField();
                _isProcessing = false;
                return;
            }

            SetInteractable(false);
            SetStatus("Verifying...");
            ClearError();

            CleanupCancellationToken();
            _cts = new CancellationTokenSource();
            var localCts = _cts;

            try
            {
                DeviceInfoHelper.GetLoginDeviceFields(out string fromDevice, out string macAddress);
                // Registration confirm endpoint expects web-style device identity.
                fromDevice = "web";

                var req = new APIManager.SignupConfirmRequest
                {
                    email = _email,
                    password = _password,
                    userName = _username,
                    firstName = _firstName,
                    lastName = _lastName,
                    otp = otp,
                    role = FIXED_ROLE,
                    fcmToken = string.IsNullOrWhiteSpace(FIXED_FCM_TOKEN) ? macAddress : FIXED_FCM_TOKEN,
                    fromDevice = fromDevice,
                    macAddress = macAddress,
                    referralCode = _referralCode
                };

                var resp = await APIManager.SignupConfirmAsync(req, configureUserAuthOnSuccess: true, ct: localCts.Token);

                if (resp != null && resp.status && resp.data != null)
                {
                    SetStatus("Account created successfully!");
                    
                    var result = BuildAuthResult(resp);
                    _canvasAuth?.NotifyAuthSuccess(result);

                    if (_autoLoadGameScene)
                    {
                        LoadGameScene();
                    }
                    else
                    {
                        await Task.Delay(1500, localCts.Token);
                        _canvasAuth?.ShowLogin();
                    }
                }
                else
                {
                    ClearOTPInput();
                    var errorMsg = MapOTPErrorToFriendly(resp?.message);
                    ShowError(errorMsg);
                    _isProcessing = false;
                    SetInteractable(true);
                }
            }
            catch (OperationCanceledException)
            {
                _isProcessing = false;
                SetInteractable(true);
            }
            catch (Exception ex)
            {
                Debug.LogError($"[{nameof(IVXPanelOTP)}] Verify error: {ex.Message}");
                ClearOTPInput();
                ShowError(MapOTPErrorToFriendly(ex.Message));
                _isProcessing = false;
                SetInteractable(true);
            }
        }

        private static AuthResult BuildAuthResult(APIManager.SignupConfirmResponse resp)
        {
            var d = resp.data;
            var u = d?.user;
            
            string userId = u != null && !string.IsNullOrEmpty(u.idpUsername) 
                ? u.idpUsername 
                : u?.id ?? string.Empty;
            
            string access = d != null 
                ? (d.token ?? string.Empty) 
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
                Email = u?.email ?? string.Empty,
                DisplayName = u?.userName ?? u?.firstName ?? string.Empty,
                AccessToken = access,
                RefreshToken = d?.refreshToken ?? string.Empty,
                IsGuest = false,
                ExpiresAt = expAt
            };
        }

        #endregion

        #region Resend OTP

        private void OnClickResend()
        {
            AnimateButton(_resendButton);
            _ = ResendOTPAsync();
        }

        private async Task ResendOTPAsync()
        {
            if (string.IsNullOrEmpty(_email))
            {
                ShowError("Email missing. Please start from registration.");
                return;
            }

            SetStatus("Resending verification code...");
            _resendButton.interactable = false;

            try
            {
                var resp = await APIManager.SignupInitiateAsync(_email, _password, FIXED_ROLE, CancellationToken.None);

                if (resp != null && resp.status)
                {
                    SetStatus("Verification code sent!");
                    ClearOTPInput();
                }
                else
                {
                    ShowError(resp?.message ?? "Failed to resend code.");
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"[{nameof(IVXPanelOTP)}] Resend error: {ex.Message}");
                ShowError("Failed to resend code. Please try again.");
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
                    _timerText.text = $"Resend OTP in {Mathf.CeilToInt(currentTime)}s";

                currentTime -= Time.deltaTime;
                yield return null;
            }

            if (_timerText != null)
                _timerText.text = "";

            if (_resendButton != null)
                _resendButton.interactable = true;
        }

        #endregion

        #region Scene Loading

        private void LoadGameScene()
        {
            string sceneName = !string.IsNullOrWhiteSpace(_gameSceneName) 
                ? _gameSceneName 
                : null;

            if (sceneName != null)
            {
                try
                {
                    SceneManager.LoadScene(sceneName);
                }
                catch (Exception ex)
                {
                    Debug.LogError($"[{nameof(IVXPanelOTP)}] Scene load error: {ex.Message}");
                    SceneManager.LoadScene(_gameSceneBuildIndex);
                }
            }
            else
            {
                SceneManager.LoadScene(_gameSceneBuildIndex);
            }
        }

        #endregion

        #region OTP Helpers

        private string GetOTPValue()
        {
            if (_otpInputs != null && _otpInputs.Length > 0)
            {
                var sb = new System.Text.StringBuilder();
                foreach (var input in _otpInputs)
                {
                    if (input != null)
                        sb.Append(input.text?.Trim() ?? "");
                }
                return sb.ToString();
            }

            return _otpInput?.text?.Trim() ?? "";
        }

        private void ClearOTPInput()
        {
            if (_otpInputs != null && _otpInputs.Length > 0)
            {
                foreach (var input in _otpInputs)
                {
                    if (input != null)
                        input.text = "";
                }
            }
            
            if (_otpInput != null)
                _otpInput.text = "";
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

        private TMP_InputField FirstEnabledInput()
        {
            if (_otpInputs != null && _otpInputs.Length > 0)
            {
                foreach (var f in _otpInputs)
                {
                    if (f != null && f.enabled && f.gameObject.activeInHierarchy)
                        return f;
                }
            }
            return _otpInput;
        }

        private void UpdateEmailDisplay()
        {
            if (_emailDisplayText != null && !string.IsNullOrEmpty(_email))
            {
                _emailDisplayText.text = $"Code sent to: {_email}";
            }
        }

        private string MapOTPErrorToFriendly(string message)
        {
            if (string.IsNullOrWhiteSpace(message))
                return "OTP verification failed. Please try again.";

            var m = message.ToLowerInvariant();

            if (m.Contains("expired") || m.Contains("timeout"))
                return "Verification code has expired. Please request a new one.";

            if (m.Contains("invalid") || m.Contains("incorrect") || m.Contains("wrong"))
                return "Incorrect verification code. Please check and try again.";

            if (m.Contains("attempts") || m.Contains("limit") || m.Contains("exceeded"))
                return "Too many attempts. Please wait a few minutes and try again.";

            if (m.Contains("not found") || m.Contains("does not exist"))
                return "OTP verification failed. Please start from registration.";

            return message.Length <= 100 ? message : "OTP verification failed. Please try again.";
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
            if (_verifyButton) _verifyButton.interactable = enabled;
            if (_closeButton) _closeButton.interactable = enabled;

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
            SetStatus(message);
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

        private void OnClickClose()
        {
            AnimateButton(_closeButton);
            Close();
            _canvasAuth?.ShowRegister();
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
                _canvasGroup.DOFade(1f, 0.15f);
            }
            if (_panel != null)
            {
                _panel.transform.localScale = Vector3.one * 0.97f;
                _panel.transform.DOScale(1f, 0.15f).SetEase(Ease.OutQuad);
            }
#endif
        }

        private void FadeOut(Action onComplete)
        {
#if DOTWEEN_ENABLED || DOTWEEN
            Tween t1 = null;
            if (_canvasGroup != null) t1 = _canvasGroup.DOFade(0f, 0.12f);
            var t2 = _panel != null ? _panel.transform.DOScale(0.97f, 0.12f).SetEase(Ease.InQuad) : null;

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
