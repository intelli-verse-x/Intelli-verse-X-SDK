using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using IntelliVerseX.Identity;
using IntelliVerseX.Core;
using IntelliVerseX.Backend;
#if DOTWEEN_ENABLED || DOTWEEN
using DG.Tweening;
#endif

namespace IntelliVerseX.Auth.UI
{
    /// <summary>
    /// Login panel controller with production-ready features:
    /// - Email/password validation
    /// - Remember me with session persistence
    /// - Password visibility toggle
    /// - Social login (Google, Apple, Facebook)
    /// - Guest login
    /// - Focus visuals with DOTween animations
    /// - Nakama initialization after login
    /// </summary>
    public class IVXPanelLogin : MonoBehaviour
    {
        #region Serialized Fields

        [Header("Root")]
        [SerializeField] private GameObject _panel;
        [SerializeField] private CanvasGroup _canvasGroup;

        [Header("Input Fields")]
        [SerializeField] private TMP_InputField _emailInput;
        [SerializeField] private TMP_InputField _passwordInput;

        [Header("Input Focus Visuals")]
        [SerializeField] private Graphic _emailFocusFrame;
        [SerializeField] private Graphic _passwordFocusFrame;
        [SerializeField] private Color _focusFrameColor = new Color(0.24f, 0.62f, 0.99f, 1f);
        [SerializeField] private Color _idleFrameColor = new Color(1f, 1f, 1f, 0.16f);
        [SerializeField] private Color _caretFocusColor = new Color(0.24f, 0.62f, 0.99f, 1f);
        [SerializeField] private Color _caretIdleColor = new Color(1f, 1f, 1f, 0.85f);
#pragma warning disable CS0414
        [SerializeField] private float _focusFrameScale = 1.01f;
        [SerializeField] private float _focusAnimDuration = 0.12f;
#pragma warning restore CS0414

        [Header("Buttons")]
        [SerializeField] private Button _loginButton;
        [SerializeField] private Button _registerButton;
        [SerializeField] private Button _forgotPasswordButton;
        [SerializeField] private Button _guestLoginButton;

        [Header("Social Login")]
        [SerializeField] private Button _googleSignInButton;
        [SerializeField] private Button _appleSignInButton;
        [SerializeField] private Button _facebookSignInButton;

        [Header("Password Toggle")]
        [SerializeField] private Button _passwordToggleButton;
        [SerializeField] private Image _passwordToggleIcon;
        [SerializeField] private Sprite _eyeClosedSprite;
        [SerializeField] private Sprite _eyeOpenSprite;

        [Header("Remember Me")]
        [SerializeField] private Toggle _rememberMeToggle;
        [SerializeField] private Image _rememberMeBg;
        [SerializeField] private GameObject _rememberMeKnob;
        [SerializeField] private Color _toggleOnColor = new Color(0.19f, 0.75f, 0.47f, 1f);
        [SerializeField] private Color _toggleOffColor = new Color(0.29f, 0.33f, 0.46f, 1f);
#pragma warning disable CS0414
        [SerializeField] private float _knobOffX = -18f;
        [SerializeField] private float _knobOnX = 18f;
        [SerializeField] private float _toggleAnimDuration = 0.18f;
#pragma warning restore CS0414

        [Header("UI Elements")]
        [SerializeField] private TextMeshProUGUI _errorText;
        [SerializeField] private TextMeshProUGUI _statusText;

        [Header("Scene Loading")]
        [SerializeField] private int _gameSceneBuildIndex = 1;
        [SerializeField] private bool _autoLoadSceneOnSuccess = true;

        #endregion

        #region Private Fields

        private IVXCanvasAuth _canvasAuth;
        private CancellationTokenSource _cts;
        private bool _isProcessing;
        private bool _passwordVisible;
        private int _panelTransitionVersion;

        private const string PP_REMEMBER = "IVX_auth.remember";
        private const string PP_LAST_EMAIL = "IVX_auth.last_email";
        private const string PP_PERSIST_FLAG = "IVX_auth.persisted";
        private const string PP_USER_ID = "IVX_auth.user_id";
        private const string PP_LOGIN_TYPE = "IVX_auth.login_type";

        private const string DEFAULT_FROM_DEVICE = "machine";
        private const string DEFAULT_MAC = "00:1A:2B:3C:4D:5E";

        #endregion

        #region Unity Lifecycle

        private void Awake()
        {
            _canvasAuth = GetComponentInParent<IVXCanvasAuth>();
            
            EnsureCanvasGroup();
            SetupPasswordField();
            SetupButtons();
            SetupRememberMe();
            SetupPasswordToggle();
            SetupFocusVisuals();
            ValidateInputFields();
        }

        private void OnEnable()
        {
            ClearError();
            ClearPassword();
            DeactivateInputFieldsDelayed();
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
        /// Open the login panel
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
            
            // Start IP geolocation fetch in background (non-blocking)
            // This pre-fetches location so it's ready when login completes
            StartIPGeolocationBackground();
        }

        /// <summary>
        /// Close the login panel
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
        /// Attempt login with current credentials
        /// </summary>
        public async void Login()
        {
            if (_isProcessing) return;
            await OnClickLoginAsync();
        }

        /// <summary>
        /// Navigate to register panel
        /// </summary>
        public void GoToRegister()
        {
            AnimateButton(_registerButton);
            _canvasAuth?.ShowRegister();
        }

        /// <summary>
        /// Navigate to forgot password panel
        /// </summary>
        public void GoToForgotPassword()
        {
            AnimateButton(_forgotPasswordButton);
            _canvasAuth?.ShowForgotPassword();
        }

        /// <summary>
        /// Login as guest
        /// </summary>
        public void GuestLogin()
        {
            AnimateButton(_guestLoginButton);
            _canvasAuth?.LoginAsGuest();
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
        }

        private void SetupButtons()
        {
            _loginButton?.onClick.AddListener(() => _ = OnClickLoginAsync());
            _registerButton?.onClick.AddListener(GoToRegister);
            _forgotPasswordButton?.onClick.AddListener(GoToForgotPassword);
            _guestLoginButton?.onClick.AddListener(GuestLogin);

            _googleSignInButton?.onClick.AddListener(SignInWithGoogle);
            _appleSignInButton?.onClick.AddListener(SignInWithApple);
            _facebookSignInButton?.onClick.AddListener(SignInWithFacebook);

            if (_guestLoginButton != null && _canvasAuth != null)
            {
                _guestLoginButton.gameObject.SetActive(_canvasAuth.AllowGuestLogin);
            }
        }

        private void SetupRememberMe()
        {
            if (!PlayerPrefs.HasKey(PP_REMEMBER))
            {
                PlayerPrefs.SetInt(PP_REMEMBER, 1);
                PlayerPrefs.Save();
            }

            bool remembered = PlayerPrefs.GetInt(PP_REMEMBER, 1) == 1;
            if (_rememberMeToggle != null)
            {
                _rememberMeToggle.isOn = remembered;
                _rememberMeToggle.onValueChanged.AddListener(OnRememberMeChanged);
            }
            UpdateRememberVisual(remembered, instant: true);

            if (remembered && _emailInput != null)
            {
                var lastEmail = PlayerPrefs.GetString(PP_LAST_EMAIL, string.Empty);
                if (!string.IsNullOrWhiteSpace(lastEmail))
                {
                    _emailInput.text = lastEmail;
                }
            }
        }

        private void SetupPasswordToggle()
        {
            if (_passwordToggleButton != null && _passwordInput != null)
            {
                _passwordToggleButton.onClick.AddListener(TogglePasswordVisibility);
                UpdatePasswordToggleVisual();
            }
        }

        private void SetupFocusVisuals()
        {
            WireInputFocus(_emailInput, _emailFocusFrame);
            WireInputFocus(_passwordInput, _passwordFocusFrame);
        }

        #endregion

        #region Login Flow

        private async Task OnClickLoginAsync()
        {
            if (_isProcessing) return;
            _isProcessing = true;
            AnimateButton(_loginButton);

            ValidateInputFields();

            string email = _emailInput?.text?.Trim() ?? "";
            string password = _passwordInput?.text ?? "";
            bool remember = _rememberMeToggle != null && _rememberMeToggle.isOn;

            if (string.IsNullOrWhiteSpace(email))
            {
                ShowError("Email is required.");
                _emailInput?.ActivateInputField();
                _isProcessing = false;
                return;
            }

            if (!IsValidEmail(email))
            {
                ShowError("Please enter a valid email address.");
                _emailInput?.ActivateInputField();
                _isProcessing = false;
                return;
            }

            if (string.IsNullOrWhiteSpace(password))
            {
                ShowError("Password is required.");
                _passwordInput?.ActivateInputField();
                _isProcessing = false;
                return;
            }

            if (remember)
            {
                PlayerPrefs.SetString(PP_LAST_EMAIL, email);
                PlayerPrefs.SetInt(PP_REMEMBER, 1);
                PlayerPrefs.Save();
            }
            else
            {
                PlayerPrefs.DeleteKey(PP_LAST_EMAIL);
                PlayerPrefs.SetInt(PP_REMEMBER, 0);
                PlayerPrefs.Save();
            }

            SetInteractable(false);
            SetStatus("Signing in...");
            ClearError();

            CleanupCancellationToken();
            _cts = new CancellationTokenSource();
            var localCts = _cts;

            try
            {
                var req = new APIManager.LoginRequest
                {
                    email = email,
                    password = password,
                    fromDevice = DEFAULT_FROM_DEVICE,
                    macAddress = DEFAULT_MAC
                };

                using (var timeoutCts = new CancellationTokenSource(TimeSpan.FromSeconds(15)))
                using (var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(localCts.Token, timeoutCts.Token))
                {
                    var resp = await APIManager.LoginAsync(
                        req,
                        configureUserAuthOnSuccess: true,
                        persistSession: remember,
                        ct: linkedCts.Token);

                    ClearPassword();

                    if (resp != null && resp.status && resp.data != null)
                    {
                        SyncCoreIdentityFromLoginResponse(resp);
                        TryRefreshRuntimeSnapshot();

                        if (remember)
                        {
                            PlayerPrefs.SetInt(PP_PERSIST_FLAG, 1);
                            if (resp.data.user != null)
                            {
                                PlayerPrefs.SetString(PP_USER_ID, resp.data.user.id ?? "");
                                PlayerPrefs.SetString(PP_LOGIN_TYPE, resp.data.user.loginType ?? "cognito");
                            }
                            PlayerPrefs.Save();
                        }

                        SetStatus("Signed in! Syncing player data...");

                        bool nakamaSuccess = await InitializeNakamaAsync();

                        if (nakamaSuccess)
                        {
                            SetStatus("Player data synced. Loading game...");
                        }
                        else
                        {
                            SetStatus("Continuing to game...");
                        }

                        // Sync IP geolocation to player profile (non-blocking)
                        SyncIPGeolocationToProfile();

                        var result = BuildAuthResultFromLoginResponse(resp);
                        _canvasAuth?.NotifyAuthSuccess(result);

                        if (_autoLoadSceneOnSuccess)
                        {
                            await Task.Delay(1000, localCts.Token);
                            SceneManager.LoadScene(_gameSceneBuildIndex);
                        }
                    }
                    else
                    {
                        var friendly = FriendlyLoginFail(resp?.message);
                        ShowError(friendly);
                        SetInteractable(true);
                        _isProcessing = false;
                        _canvasAuth?.NotifyAuthFailed(friendly);
                    }
                }
            }
            catch (OperationCanceledException)
            {
                ShowError("Request timed out. Please try again.");
                SetInteractable(true);
                _isProcessing = false;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[{nameof(IVXPanelLogin)}] Login error: {ex.Message}");
                var friendly = FriendlyLoginFail(ex.Message);
                ShowError(friendly);
                SetInteractable(true);
                _isProcessing = false;
                _canvasAuth?.NotifyAuthFailed(friendly);
            }
        }

        private async Task<bool> InitializeNakamaAsync()
        {
            try
            {
                var manager = IntelliVerseX.Backend.Nakama.IVXNManager.Instance;
                if (manager == null)
                {
                    Debug.LogWarning($"[{nameof(IVXPanelLogin)}] No IVXNManager in scene; skipping Nakama init.");
                    return false;
                }

                Debug.Log($"[{nameof(IVXPanelLogin)}] Starting Nakama initialization...");
                bool ok = await manager.InitializeForCurrentUserAsync();

                if (ok)
                {
                    Debug.Log($"[{nameof(IVXPanelLogin)}] Nakama initialization complete.");
                }
                else
                {
                    Debug.LogWarning($"[{nameof(IVXPanelLogin)}] Nakama initialization returned false.");
                }

                TryRefreshRuntimeSnapshot();

                return ok;
            }
            catch (Exception e)
            {
                Debug.LogError($"[{nameof(IVXPanelLogin)}] Nakama init error: {e.Message}");
                return false;
            }
        }

        private static AuthResult BuildAuthResultFromLoginResponse(APIManager.LoginResponse loginResp)
        {
            var d = loginResp.data;
            var u = d.user;

            string userId = !string.IsNullOrEmpty(u.idpUsername) ? u.idpUsername : u.id ?? string.Empty;
            string access = !string.IsNullOrEmpty(d.accessToken) ? d.accessToken : d.token ?? string.Empty;

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

        private static void SyncCoreIdentityFromLoginResponse(APIManager.LoginResponse loginResp)
        {
            try
            {
                if (loginResp?.data?.user == null)
                    return;

                var d = loginResp.data;
                var u = d.user;
                var existing = IntelliVerseXIdentity.GetUser();
                long expEpoch = DateTimeOffset.UtcNow.ToUnixTimeSeconds() + Math.Max(0, d.expiresIn <= 0 ? 1800 : d.expiresIn);

                var mapped = new IntelliVerseXUser
                {
                    Username = u.userName ?? existing?.Username ?? u.firstName ?? string.Empty,
                    DeviceId = existing?.DeviceId ?? string.Empty,
                    GameId = existing?.GameId ?? string.Empty,

                    CognitoUserId = !string.IsNullOrWhiteSpace(u.idpUsername) ? u.idpUsername : (u.id ?? string.Empty),
                    Email = u.email ?? string.Empty,
                    IdpUsername = u.idpUsername ?? string.Empty,
                    FirstName = u.firstName ?? string.Empty,
                    LastName = u.lastName ?? string.Empty,

                    AccessToken = !string.IsNullOrWhiteSpace(d.accessToken) ? d.accessToken : (d.token ?? string.Empty),
                    IdToken = d.idToken ?? string.Empty,
                    RefreshToken = d.refreshToken ?? string.Empty,
                    AccessTokenExpiryEpoch = expEpoch,

                    GameWalletId = existing?.GameWalletId ?? string.Empty,
                    GlobalWalletId = existing?.GlobalWalletId ?? string.Empty,
                    GameWalletBalance = existing?.GameWalletBalance ?? 0,
                    GlobalWalletBalance = existing?.GlobalWalletBalance ?? 0,
                    GameWalletCurrency = string.IsNullOrWhiteSpace(existing?.GameWalletCurrency) ? "coins" : existing.GameWalletCurrency,
                    GlobalWalletCurrency = string.IsNullOrWhiteSpace(existing?.GlobalWalletCurrency) ? "gems" : existing.GlobalWalletCurrency,
                    WalletAddress = u.walletAddress ?? string.Empty,

                    Role = u.role ?? "user",
                    IsAdult = u.isAdult ? "True" : "False",
                    LoginType = u.loginType ?? "email",
                    AccountStatus = u.accountStatus ?? string.Empty,
                    KycStatus = u.kycStatus ?? string.Empty,

                    IsGuestUser = false,
                    GuestCreatedEpoch = 0
                };

                IntelliVerseXIdentity.SetCurrentUser(mapped);
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[{nameof(IVXPanelLogin)}] Core identity sync failed: {ex.Message}");
            }
        }

        private static void TryRefreshRuntimeSnapshot()
        {
            try
            {
                global::IntelliVerseX.Backend.Nakama.IVXNUserRuntime.Instance?.RefreshFromGlobals();
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[{nameof(IVXPanelLogin)}] Runtime refresh failed: {e.Message}");
            }
        }

        #endregion

        #region Social Login

        private void SignInWithGoogle()
        {
            AnimateButton(_googleSignInButton);
            Debug.Log($"[{nameof(IVXPanelLogin)}] Google Sign-In requested");
        }

        private void SignInWithApple()
        {
            AnimateButton(_appleSignInButton);
            Debug.Log($"[{nameof(IVXPanelLogin)}] Apple Sign-In requested");
        }

        private void SignInWithFacebook()
        {
            AnimateButton(_facebookSignInButton);
            Debug.Log($"[{nameof(IVXPanelLogin)}] Facebook Sign-In requested");
        }

        #endregion

        #region Password Toggle

        private void TogglePasswordVisibility()
        {
            _passwordVisible = !_passwordVisible;
            SetPasswordFieldVisibility(_passwordVisible);
            UpdatePasswordToggleVisual();
        }

        private void SetPasswordFieldVisibility(bool visible)
        {
            if (_passwordInput == null) return;

            int caret = _passwordInput.caretPosition;
            int anchor = _passwordInput.selectionAnchorPosition;
            int focus = _passwordInput.selectionFocusPosition;
            string current = _passwordInput.text;

            _passwordInput.contentType = visible
                ? TMP_InputField.ContentType.Standard
                : TMP_InputField.ContentType.Password;

            _passwordInput.ForceLabelUpdate();
            _passwordInput.SetTextWithoutNotify("");
            _passwordInput.SetTextWithoutNotify(current);

            _passwordInput.caretPosition = Mathf.Clamp(caret, 0, current.Length);
            _passwordInput.selectionAnchorPosition = Mathf.Clamp(anchor, 0, current.Length);
            _passwordInput.selectionFocusPosition = Mathf.Clamp(focus, 0, current.Length);

            _passwordInput.ForceLabelUpdate();
            _passwordInput.ActivateInputField();
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

        #region Remember Me

        private void OnRememberMeChanged(bool on)
        {
            PlayerPrefs.SetInt(PP_REMEMBER, on ? 1 : 0);
            PlayerPrefs.Save();
            UpdateRememberVisual(on, instant: false);

            if (!on)
            {
                PlayerPrefs.DeleteKey(PP_LAST_EMAIL);
                PlayerPrefs.DeleteKey(PP_PERSIST_FLAG);
                PlayerPrefs.DeleteKey(PP_USER_ID);
                PlayerPrefs.DeleteKey(PP_LOGIN_TYPE);
                PlayerPrefs.Save();
            }
        }

        private void UpdateRememberVisual(bool on, bool instant)
        {
            if (_rememberMeBg != null)
            {
#if DOTWEEN_ENABLED || DOTWEEN
                if (!instant)
                    _rememberMeBg.DOColor(on ? _toggleOnColor : _toggleOffColor, _toggleAnimDuration);
                else
                    _rememberMeBg.color = on ? _toggleOnColor : _toggleOffColor;
#else
                _rememberMeBg.color = on ? _toggleOnColor : _toggleOffColor;
#endif
            }

            if (_rememberMeKnob != null)
            {
                _rememberMeKnob.SetActive(on);
            }
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

        #region IP Geolocation (Non-Blocking)

        /// <summary>
        /// Start IP geolocation fetch in background.
        /// This is non-blocking and will not delay login.
        /// </summary>
        private void StartIPGeolocationBackground()
        {
            try
            {
                // Check if service exists and cache is not already valid
                if (IVXIPGeolocationService.HasInstance && IVXIPGeolocationService.Instance.IsCacheValid)
                {
                    LogGeoVerbose("IP location already cached");
                    return;
                }

                // Fire and forget - don't await, don't block
                _ = FetchIPGeolocationAsync();
            }
            catch (Exception ex)
            {
                // Never let geolocation errors affect login
                Debug.LogWarning($"[IVXLogin] IP geolocation background start failed: {ex.Message}");
            }
        }

        /// <summary>
        /// Fetch IP geolocation asynchronously.
        /// Errors are caught and logged but never thrown.
        /// </summary>
        private async Task FetchIPGeolocationAsync()
        {
            try
            {
                LogGeoVerbose("Starting background IP geolocation fetch...");

                var geoService = IVXIPGeolocationService.Instance;
                if (geoService == null)
                {
                    LogGeoVerbose("IP geolocation service not available");
                    return;
                }

                var result = await geoService.GetLocationAsync(false, _cts?.Token ?? default);

                if (result != null && result.Success)
                {
                    LogGeoVerbose($"IP location fetched: {result.GetShortLocation()} via {result.Provider}");
                }
                else
                {
                    LogGeoVerbose($"IP location fetch returned: {result?.Error ?? "null result"}");
                }
            }
            catch (OperationCanceledException)
            {
                LogGeoVerbose("IP geolocation fetch cancelled");
            }
            catch (Exception ex)
            {
                // Swallow all errors - geolocation should never block or affect login
                Debug.LogWarning($"[IVXLogin] IP geolocation fetch error (non-blocking): {ex.Message}");
            }
        }

        /// <summary>
        /// Get the current IP geolocation result (if available).
        /// Returns null if not fetched yet or failed.
        /// </summary>
        public static IPGeolocationResult GetCurrentIPLocation()
        {
            try
            {
                if (IVXIPGeolocationService.HasInstance)
                {
                    return IVXIPGeolocationService.Instance.GetCachedLocation();
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[IVXLogin] Failed to get IP location: {ex.Message}");
            }
            return null;
        }

        /// <summary>
        /// Sync IP geolocation to player profile after successful login.
        /// This is called internally after successful authentication.
        /// </summary>
        private void SyncIPGeolocationToProfile()
        {
            try
            {
                var ipLocation = GetCurrentIPLocation();
                if (ipLocation == null || !ipLocation.Success)
                {
                    LogGeoVerbose("No IP location available to sync");
                    return;
                }

                // Save to PlayerPrefs for persistence
                if (!string.IsNullOrEmpty(ipLocation.Country))
                {
                    PlayerPrefs.SetString("ivx_player_country", ipLocation.Country);
                    PlayerPrefs.SetString("ivx_player_country_code", ipLocation.CountryCode ?? "");
                    PlayerPrefs.SetString("ivx_player_city", ipLocation.City ?? "");
                    PlayerPrefs.SetString("ivx_player_region", ipLocation.Region ?? "");
                    PlayerPrefs.SetString("ivx_player_timezone", ipLocation.Timezone ?? "");
                    PlayerPrefs.SetFloat("ivx_player_latitude", (float)ipLocation.Latitude);
                    PlayerPrefs.SetFloat("ivx_player_longitude", (float)ipLocation.Longitude);
                    PlayerPrefs.SetString("ivx_player_isp", ipLocation.ISP ?? "");
                    PlayerPrefs.SetString("ivx_player_ip", ipLocation.IP ?? "");
                    PlayerPrefs.Save();
                    
                    LogGeoVerbose($"Synced IP location to PlayerPrefs: {ipLocation.GetShortLocation()}");
                }
            }
            catch (Exception ex)
            {
                // Never let sync errors affect anything
                Debug.LogWarning($"[IVXLogin] IP location sync error (non-blocking): {ex.Message}");
            }
        }

        private void LogGeoVerbose(string message)
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            Debug.Log($"[IVXLogin.Geo] {message}");
#endif
        }

        #endregion

        #region UI Helpers

        private void SetInteractable(bool enabled)
        {
            if (_loginButton) _loginButton.interactable = enabled;
            if (_guestLoginButton) _guestLoginButton.interactable = enabled;
            if (_registerButton) _registerButton.interactable = enabled;
            if (_forgotPasswordButton) _forgotPasswordButton.interactable = enabled;
            if (_googleSignInButton) _googleSignInButton.interactable = enabled;
            if (_appleSignInButton) _appleSignInButton.interactable = enabled;
            if (_facebookSignInButton) _facebookSignInButton.interactable = enabled;

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

        private void ClearPassword()
        {
            if (_passwordInput != null)
                _passwordInput.text = string.Empty;
        }

        private void DeactivateInputFieldsDelayed()
        {
            StartCoroutine(DeactivateInputFieldsCoroutine());
        }

        private System.Collections.IEnumerator DeactivateInputFieldsCoroutine()
        {
            yield return new WaitForEndOfFrame();

            if (_emailInput != null && _emailInput.isFocused)
                _emailInput.DeactivateInputField();
            if (_passwordInput != null && _passwordInput.isFocused)
                _passwordInput.DeactivateInputField();
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

        #region Validation

        private void ValidateInputFields()
        {
            if (_emailInput == null)
            {
                _emailInput = FindInputFieldByName("EmailInput", transform)
                    ?? FindInputFieldByName("Email", transform)
                    ?? FindInputFieldByPlaceholder("Email", transform);
            }

            if (_passwordInput == null)
            {
                _passwordInput = FindInputFieldByName("PasswordInput", transform)
                    ?? FindInputFieldByName("Password", transform)
                    ?? FindInputFieldByPlaceholder("Password", transform);

                if (_passwordInput != null && _passwordInput.contentType != TMP_InputField.ContentType.Password)
                {
                    _passwordInput.contentType = TMP_InputField.ContentType.Password;
                }
            }
            else if (_passwordInput.contentType != TMP_InputField.ContentType.Password)
            {
                _passwordInput.contentType = TMP_InputField.ContentType.Password;
            }
        }

        private TMP_InputField FindInputFieldByName(string name, Transform parent)
        {
            if (parent == null) return null;

            string searchName = name.ToLower();

            foreach (Transform child in parent)
            {
                if (child.name.ToLower().Contains(searchName))
                {
                    var inputField = child.GetComponent<TMP_InputField>();
                    if (inputField != null) return inputField;
                }
            }

            Transform content = parent.Find("Content");
            if (content != null)
            {
                foreach (Transform child in content)
                {
                    if (child.name.ToLower().Contains(searchName))
                    {
                        var inputField = child.GetComponent<TMP_InputField>();
                        if (inputField != null) return inputField;
                    }
                }
            }

            TMP_InputField[] allInputs = parent.GetComponentsInChildren<TMP_InputField>(true);
            foreach (var input in allInputs)
            {
                if (input.name.ToLower().Contains(searchName))
                {
                    return input;
                }
            }

            return null;
        }

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
                    if (placeholder.Contains(searchText))
                    {
                        return input;
                    }
                }
            }

            return null;
        }

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

        private static string FriendlyLoginFail(string hint = null)
        {
            var h = (hint ?? string.Empty).ToLowerInvariant();
            
            if (h.Contains("incorrect") || h.Contains("invalid") || h.Contains("credential") || 
                h.Contains("wrong password") || h.Contains("mismatch") || h.Contains("401"))
                return "Incorrect email or password.";
            
            if (h.Contains("otp") || h.Contains("verify") || h.Contains("unverified") || h.Contains("not verified"))
                return "Please verify your account with the OTP sent to your email.";
            
            if (h.Contains("not found") || h.Contains("no user") || h.Contains("unknown user") || h.Contains("404"))
                return "We couldn't find an account with that email.";
            
            if (h.Contains("locked") || h.Contains("suspend") || h.Contains("disabled"))
                return "Your account is temporarily locked. Please contact support.";
            
            if (h.Contains("too many") || h.Contains("rate") || h.Contains("429"))
                return "Too many attempts. Please wait a minute and try again.";
            
            if (h.Contains("timeout") || h.Contains("timed out"))
                return "Network timeout. Please check your connection and try again.";
            
            if (h.Contains("network") || h.Contains("ssl") || h.Contains("connect") || h.Contains("dns"))
                return "Network error. Check your internet connection and try again.";
            
            if (h.Contains("500") || h.Contains("server"))
                return "Server error. Please try again in a moment.";
            
            return "Couldn't sign in. Please try again.";
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
