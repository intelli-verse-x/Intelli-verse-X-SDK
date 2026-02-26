using System;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using IntelliVerseX.Backend.Nakama;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Networking;
using UnityEngine.UI;

namespace IntelliVerseX.Examples
{
    /// <summary>
    /// Production-style profile demo UX for test scenes.
    /// Renders a Canvas-based UI at runtime with validation, loading states, and clear feedback.
    /// </summary>
    public sealed class IVXProfileSceneDemoController : MonoBehaviour
    {
        [Header("Behavior")]
        [SerializeField] private bool initializeNakamaIfNeeded = true;
        [SerializeField] private bool autoFetchOnStart = true;
        [SerializeField] private bool useMockFallback = true;
        [SerializeField] private bool enableLogs = true;

        [Header("Fallback Debug UI")]
        [SerializeField] private bool renderOnGuiFallback = false;
        [SerializeField] private Vector2 fallbackScroll = Vector2.zero;

        private const string LOG_PREFIX = "[IVX-ProfileDemo]";
        private static readonly Regex NamePattern = new Regex(@"^[\p{L}\p{N}\s\.\-_']{1,100}$", RegexOptions.Compiled);
        private static readonly Regex LocalePattern = new Regex(@"^[a-z]{2,3}(-[a-z0-9]{2,8}){0,2}$", RegexOptions.Compiled);
        private static readonly Regex CountryCodePattern = new Regex(@"^[A-Z]{2,5}$", RegexOptions.Compiled);

        private CancellationTokenSource _cts;
        private bool _busy;
        private bool _uiReady;
        private bool _ownsEventSystem;

        // Data state
        private string _status = "Idle";
        private string _debug = string.Empty;
        private string _profileSnapshot = string.Empty;
        private string _portfolioSnapshot = string.Empty;

        // Form state
        private string _firstName = string.Empty;
        private string _lastName = string.Empty;
        private string _city = string.Empty;
        private string _region = string.Empty;
        private string _country = string.Empty;
        private string _countryCode = string.Empty;
        private string _locale = "en-us";
        private string _avatarUrl = string.Empty;

        // Runtime UI refs
        private CanvasGroup _canvasGroup;
        private TMP_Text _statusText;
        private TMP_Text _debugText;
        private TMP_Text _profileText;
        private TMP_Text _portfolioText;
        private TMP_Text _busyText;
        private TMP_Text _avatarHintText;

        private TMP_InputField _firstNameInput;
        private TMP_InputField _lastNameInput;
        private TMP_InputField _cityInput;
        private TMP_InputField _regionInput;
        private TMP_InputField _countryInput;
        private TMP_InputField _countryCodeInput;
        private TMP_InputField _localeInput;
        private TMP_InputField _avatarUrlInput;

        private Button _refreshButton;
        private Button _saveButton;
        private Button _portfolioButton;
        private RawImage _avatarImage;
        private Texture2D _avatarTexture;
        private CancellationTokenSource _avatarPreviewCts;
        private GameObject _runtimeUiRoot;
        private EventSystem _eventSystem;

        private void OnEnable()
        {
            IVXNProfileManager.OnProfileLoaded += OnProfileLoaded;
            IVXNProfileManager.OnProfileUpdated += OnProfileUpdated;
            IVXNProfileManager.OnProfileError += OnProfileError;
        }

        private void OnDisable()
        {
            IVXNProfileManager.OnProfileLoaded -= OnProfileLoaded;
            IVXNProfileManager.OnProfileUpdated -= OnProfileUpdated;
            IVXNProfileManager.OnProfileError -= OnProfileError;

            if (_cts != null)
            {
                _cts.Cancel();
                _cts.Dispose();
                _cts = null;
            }

            if (_avatarPreviewCts != null)
            {
                _avatarPreviewCts.Cancel();
                _avatarPreviewCts.Dispose();
                _avatarPreviewCts = null;
            }

            if (_avatarTexture != null)
            {
                Destroy(_avatarTexture);
                _avatarTexture = null;
            }
        }

        private void OnDestroy()
        {
            if (_runtimeUiRoot != null)
            {
                Destroy(_runtimeUiRoot);
                _runtimeUiRoot = null;
            }

            if (_ownsEventSystem && _eventSystem != null)
            {
                Destroy(_eventSystem.gameObject);
                _eventSystem = null;
                _ownsEventSystem = false;
            }
        }

        private async void Start()
        {
            _cts = new CancellationTokenSource();
            EnsureRuntimeUi();
            PushFormToUi();
            RefreshStatusViews();
            RequestAvatarPreviewRefresh(_avatarUrl);

            if (!autoFetchOnStart)
            {
                return;
            }

            await RunBusyGuardedAsync(async token =>
            {
                if (!await EnsureManagerReadyAsync(token))
                {
                    return;
                }

                await FetchProfileAsync(token);
            });
        }

        private void EnsureRuntimeUi()
        {
            if (_uiReady)
            {
                return;
            }

            EnsureEventSystem();

            var font = TMP_Settings.defaultFontAsset;

            var canvasGo = new GameObject("IVX Profile Demo Canvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster), typeof(CanvasGroup));
            _runtimeUiRoot = canvasGo;
            var canvas = canvasGo.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 2000;
            _canvasGroup = canvasGo.GetComponent<CanvasGroup>();

            var scaler = canvasGo.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;

            var rootPanel = CreatePanel("Root", canvasGo.transform, new Color(0.09f, 0.11f, 0.14f, 0.94f), new Vector2(16f, 16f), new Vector2(-16f, -16f));
            var rootLayout = rootPanel.gameObject.AddComponent<VerticalLayoutGroup>();
            rootLayout.padding = new RectOffset(20, 20, 18, 18);
            rootLayout.spacing = 12f;
            rootLayout.childControlHeight = true;
            rootLayout.childControlWidth = true;
            rootLayout.childForceExpandHeight = false;
            rootLayout.childForceExpandWidth = true;

            CreateText("Title", rootPanel, "IntelliVerseX Profile Demo", font, 34, new Color(0.94f, 0.96f, 1f, 1f), FontStyles.Bold);
            _statusText = CreateText("Status", rootPanel, "Status: Idle", font, 24, new Color(0.78f, 0.92f, 1f, 1f), FontStyles.Normal);
            _debugText = CreateText("Debug", rootPanel, string.Empty, font, 18, new Color(0.62f, 0.72f, 0.84f, 1f), FontStyles.Normal);

            CreateText("FormHeader", rootPanel, "Profile Fields", font, 24, Color.white, FontStyles.Bold);
            _firstNameInput = CreateInputRow(rootPanel, "First Name", _firstName, font);
            _lastNameInput = CreateInputRow(rootPanel, "Last Name", _lastName, font);
            _cityInput = CreateInputRow(rootPanel, "City", _city, font);
            _regionInput = CreateInputRow(rootPanel, "Region", _region, font);
            _countryInput = CreateInputRow(rootPanel, "Country", _country, font);
            _countryCodeInput = CreateInputRow(rootPanel, "Country Code", _countryCode, font);
            _localeInput = CreateInputRow(rootPanel, "Locale", _locale, font);
            _avatarUrlInput = CreateInputRow(rootPanel, "Avatar URL", _avatarUrl, font);
            CreateText("AvatarHeader", rootPanel, "Player Avatar", font, 22, Color.white, FontStyles.Bold);
            _avatarImage = CreateAvatarImagePanel(rootPanel);
            _avatarHintText = CreateText("AvatarHint", rootPanel, "No profile image loaded.", font, 18, new Color(0.75f, 0.82f, 0.92f, 1f), FontStyles.Normal);

            var buttonsRow = new GameObject("ButtonsRow", typeof(RectTransform), typeof(HorizontalLayoutGroup), typeof(LayoutElement));
            buttonsRow.transform.SetParent(rootPanel, false);
            var buttonsLayout = buttonsRow.GetComponent<HorizontalLayoutGroup>();
            buttonsLayout.spacing = 10f;
            buttonsLayout.childForceExpandWidth = true;
            buttonsLayout.childForceExpandHeight = true;
            buttonsRow.GetComponent<LayoutElement>().preferredHeight = 64f;

            _refreshButton = CreateButton("Refresh Profile", buttonsRow.transform, font, new Color(0.18f, 0.45f, 0.78f, 1f), OnClickRefresh);
            _saveButton = CreateButton("Save Profile", buttonsRow.transform, font, new Color(0.2f, 0.56f, 0.35f, 1f), OnClickSave);
            _portfolioButton = CreateButton("Fetch Portfolio", buttonsRow.transform, font, new Color(0.48f, 0.34f, 0.75f, 1f), OnClickPortfolio);

            _busyText = CreateText("Busy", rootPanel, string.Empty, font, 20, new Color(1f, 0.83f, 0.3f, 1f), FontStyles.Bold);
            _busyText.gameObject.SetActive(false);

            CreateText("ProfileHeader", rootPanel, "Profile Snapshot", font, 22, Color.white, FontStyles.Bold);
            _profileText = CreateOutputPanel(rootPanel, font);
            CreateText("PortfolioHeader", rootPanel, "Portfolio Snapshot", font, 22, Color.white, FontStyles.Bold);
            _portfolioText = CreateOutputPanel(rootPanel, font);

            WireInputEvents();
            _uiReady = true;
        }

        private void EnsureEventSystem()
        {
            _eventSystem = FindObjectOfType<EventSystem>();
            if (_eventSystem == null)
            {
                _eventSystem = new GameObject("EventSystem", typeof(EventSystem)).GetComponent<EventSystem>();
                _ownsEventSystem = true;
            }
            else
            {
                _ownsEventSystem = false;
            }

            var inputSystemModuleType = Type.GetType("UnityEngine.InputSystem.UI.InputSystemUIInputModule, Unity.InputSystem");
            if (inputSystemModuleType != null)
            {
                if (_eventSystem.GetComponent(inputSystemModuleType) == null)
                {
                    _eventSystem.gameObject.AddComponent(inputSystemModuleType);
                }

                var inputSystemModule = _eventSystem.GetComponent(inputSystemModuleType) as BaseInputModule;
                if (inputSystemModule != null)
                {
                    inputSystemModule.enabled = true;
                    EnsureOnlyInputModuleEnabled(_eventSystem, inputSystemModule);
                }

                return;
            }

            var standaloneModule = _eventSystem.GetComponent<StandaloneInputModule>();
            if (standaloneModule == null)
            {
                standaloneModule = _eventSystem.gameObject.AddComponent<StandaloneInputModule>();
            }
            standaloneModule.enabled = true;
            EnsureOnlyInputModuleEnabled(_eventSystem, standaloneModule);
        }

        private async Task<bool> EnsureManagerReadyAsync(CancellationToken token)
        {
            var manager = IVXNManager.Instance;
            if (manager == null)
            {
                ApplyMockIfNeeded("IVXNManager is missing in this scene.");
                return false;
            }

            if (!initializeNakamaIfNeeded)
            {
                return manager.IsInitialized;
            }

            if (manager.IsInitialized)
            {
                return true;
            }

            SetStatus("Initializing Nakama...");
            var ok = await manager.InitializeForCurrentUserAsync();
            if (!ok || token.IsCancellationRequested)
            {
                ApplyMockIfNeeded("Nakama initialization failed.");
                return false;
            }

            return true;
        }

        private async Task FetchProfileAsync(CancellationToken token)
        {
            SetStatus("Fetching profile...");
            var result = await IVXNProfileManager.FetchProfileAsync(token);
            if (!result.Success)
            {
                SetStatus(MapError(result.ErrorCode, result.ErrorMessage), true);
                return;
            }

            ApplyProfile(result.Profile);
            SetStatus("Profile loaded successfully.");
            SetDebug("traceId=" + result.TraceId + ", requestId=" + result.RequestId);
        }

        private async Task SaveProfileAsync(CancellationToken token)
        {
            PullFormFromUi();

            if (!ValidateForm(out var validationError))
            {
                SetStatus(validationError, true);
                return;
            }

            SetStatus("Saving profile...");
            var updateRequest = new IVXNProfileManager.IVXNProfileUpdateRequest
            {
                FirstName = _firstName,
                LastName = _lastName,
                City = _city,
                Region = _region,
                Country = _country,
                CountryCode = _countryCode,
                Locale = _locale,
                AvatarUrl = _avatarUrl
            };

            var result = await IVXNProfileManager.UpdateProfileAsync(updateRequest, token);
            if (!result.Success)
            {
                SetStatus(MapError(result.ErrorCode, result.ErrorMessage), true);
                return;
            }

            ApplyProfile(result.Profile);
            SetStatus("Profile saved successfully.");
            SetDebug("traceId=" + result.TraceId + ", requestId=" + result.RequestId);
        }

        private async Task FetchPortfolioAsync(CancellationToken token)
        {
            SetStatus("Fetching portfolio...");
            var result = await IVXNProfileManager.FetchPortfolioAsync(token);
            if (!result.Success)
            {
                SetStatus(MapError(result.ErrorCode, result.ErrorMessage), true);
                return;
            }

            if (result.Portfolio == null)
            {
                _portfolioSnapshot = "Portfolio is empty.";
            }
            else
            {
                var sb = new StringBuilder(256);
                sb.AppendLine("UserId: " + result.Portfolio.UserId);
                sb.AppendLine("Total Games: " + result.Portfolio.TotalGames);
                sb.AppendLine("Global Wallet Balance: " + result.Portfolio.GlobalWalletBalance);
                sb.AppendLine("Games:");
                for (var i = 0; i < result.Portfolio.Games.Count; i++)
                {
                    var game = result.Portfolio.Games[i];
                    sb.AppendLine("- " + game.GameId + " | plays=" + game.PlayCount + ", sessions=" + game.SessionCount + ", wallet=" + game.WalletBalance);
                }
                _portfolioSnapshot = sb.ToString();
            }

            RefreshStatusViews();
            SetStatus("Portfolio loaded successfully.");
            SetDebug("traceId=" + result.TraceId + ", requestId=" + result.RequestId);
        }

        private async Task RunBusyGuardedAsync(Func<CancellationToken, Task> action)
        {
            if (_busy)
            {
                SetStatus("Another operation is already in progress.");
                return;
            }

            _busy = true;
            SetBusyUi(true);
            try
            {
                if (_cts == null || _cts.IsCancellationRequested)
                {
                    if (_cts != null)
                    {
                        _cts.Dispose();
                    }
                    _cts = new CancellationTokenSource();
                }

                await action(_cts.Token);
            }
            catch (OperationCanceledException)
            {
                SetStatus("Operation canceled.");
            }
            catch (Exception ex)
            {
                SetStatus("Unexpected error: " + ex.Message, true);
            }
            finally
            {
                _busy = false;
                SetBusyUi(false);
            }
        }

        private void OnProfileLoaded(IVXNProfileManager.IVXNProfileSnapshot snapshot)
        {
            ApplyProfile(snapshot);
        }

        private void OnProfileUpdated(IVXNProfileManager.IVXNProfileSnapshot snapshot)
        {
            ApplyProfile(snapshot);
        }

        private void OnProfileError(string message)
        {
            SetStatus("Profile error: " + message, true);
        }

        private void ApplyProfile(IVXNProfileManager.IVXNProfileSnapshot profile)
        {
            if (profile == null)
            {
                return;
            }

            _firstName = profile.FirstName ?? string.Empty;
            _lastName = profile.LastName ?? string.Empty;
            _city = profile.City ?? string.Empty;
            _region = profile.Region ?? string.Empty;
            _country = profile.Country ?? string.Empty;
            _countryCode = profile.CountryCode ?? string.Empty;
            _locale = string.IsNullOrWhiteSpace(profile.Locale) ? _locale : profile.Locale;
            _avatarUrl = profile.AvatarUrl ?? string.Empty;

            var sb = new StringBuilder(512);
            sb.AppendLine("UserId: " + profile.UserId);
            sb.AppendLine("Name: " + (_firstName + " " + _lastName).Trim());
            sb.AppendLine("Location: " + _city + ", " + _region + ", " + _country + " (" + _countryCode + ")");
            sb.AppendLine("Locale: " + _locale);
            sb.AppendLine("Platform: " + profile.Platform);
            sb.AppendLine("Schema/Profile Version: " + profile.SchemaVersion + "/" + profile.ProfileVersion);
            _profileSnapshot = sb.ToString();

            PushFormToUi();
            RefreshStatusViews();
            RequestAvatarPreviewRefresh(_avatarUrl);
        }

        private void ApplyMockIfNeeded(string reason)
        {
            if (!useMockFallback)
            {
                SetStatus(reason, true);
                return;
            }

            _firstName = "Demo";
            _lastName = "Player";
            _city = "Bengaluru";
            _region = "Karnataka";
            _country = "India";
            _countryCode = "IN";
            _locale = "en-in";
            _avatarUrl = string.Empty;

            _profileSnapshot = "UserId: mock-user-001\nName: Demo Player\nLocation: Bengaluru, Karnataka, India (IN)";
            _portfolioSnapshot = "UserId: mock-user-001\nTotalGames: 2\nGlobalWalletBalance: 1500";
            PushFormToUi();
            RefreshStatusViews();
            RequestAvatarPreviewRefresh(_avatarUrl);
            SetStatus("Using mock fallback. " + reason, true);
        }

        private bool ValidateForm(out string error)
        {
            error = null;

            if (!string.IsNullOrWhiteSpace(_firstName) && !NamePattern.IsMatch(_firstName))
            {
                error = "First Name contains unsupported characters (max 100).";
                return false;
            }

            if (!string.IsNullOrWhiteSpace(_lastName) && !NamePattern.IsMatch(_lastName))
            {
                error = "Last Name contains unsupported characters (max 100).";
                return false;
            }

            if (!string.IsNullOrWhiteSpace(_countryCode))
            {
                _countryCode = _countryCode.Trim().ToUpperInvariant();
                if (!CountryCodePattern.IsMatch(_countryCode))
                {
                    error = "Country Code must be 2-5 uppercase letters.";
                    return false;
                }
            }

            if (!string.IsNullOrWhiteSpace(_locale))
            {
                _locale = _locale.Trim().ToLowerInvariant().Replace("_", "-");
                if (!LocalePattern.IsMatch(_locale))
                {
                    error = "Locale format should look like en-us.";
                    return false;
                }
            }

            if (!string.IsNullOrWhiteSpace(_avatarUrl))
            {
                if (!Uri.TryCreate(_avatarUrl, UriKind.Absolute, out var parsed) ||
                    (parsed.Scheme != Uri.UriSchemeHttp && parsed.Scheme != Uri.UriSchemeHttps))
                {
                    error = "Avatar URL must be a valid HTTP/HTTPS URL.";
                    return false;
                }
            }

            return true;
        }

        private static string MapError(string errorCode, string message)
        {
            var code = string.IsNullOrEmpty(errorCode) ? "UNKNOWN_ERROR" : errorCode.ToUpperInvariant();
            if (code == "AUTH_REQUIRED")
            {
                return "Session missing or expired. Please login again.";
            }
            if (code == "RATE_LIMITED" || code == "HTTP_429")
            {
                return "You are being rate limited. Please retry in a moment.";
            }
            if (code == "VERSION_CONFLICT")
            {
                return "This profile was updated elsewhere. Refresh and retry.";
            }
            if (code == "FORBIDDEN")
            {
                return "You do not have permission for this operation.";
            }

            return string.IsNullOrWhiteSpace(message) ? ("Request failed (" + code + ").") : (message + " (" + code + ")");
        }

        private void SetStatus(string message, bool isError = false)
        {
            _status = isError ? "ERROR: " + message : message;
            RefreshStatusViews();

            if (enableLogs)
            {
                if (isError) Debug.LogWarning(LOG_PREFIX + " " + _status);
                else Debug.Log(LOG_PREFIX + " " + _status);
            }
        }

        private void SetDebug(string message)
        {
            _debug = message ?? string.Empty;
            RefreshStatusViews();
        }

        private void SetBusyUi(bool busy)
        {
            if (_canvasGroup != null)
            {
                _canvasGroup.blocksRaycasts = true;
                _canvasGroup.alpha = busy ? 0.98f : 1f;
            }

            if (_refreshButton != null) _refreshButton.interactable = !busy;
            if (_saveButton != null) _saveButton.interactable = !busy;
            if (_portfolioButton != null) _portfolioButton.interactable = !busy;

            if (_busyText != null)
            {
                _busyText.gameObject.SetActive(busy);
                _busyText.text = busy ? "Processing request..." : string.Empty;
            }
        }

        private void RefreshStatusViews()
        {
            if (_statusText != null)
            {
                _statusText.text = "Status: " + _status;
                _statusText.color = _status.StartsWith("ERROR:", StringComparison.OrdinalIgnoreCase)
                    ? new Color(0.96f, 0.43f, 0.43f, 1f)
                    : new Color(0.78f, 0.92f, 1f, 1f);
            }

            if (_debugText != null)
            {
                _debugText.text = string.IsNullOrEmpty(_debug) ? string.Empty : ("Debug: " + _debug);
            }

            if (_profileText != null)
            {
                _profileText.text = _profileSnapshot;
            }

            if (_portfolioText != null)
            {
                _portfolioText.text = _portfolioSnapshot;
            }
        }

        private void PullFormFromUi()
        {
            if (!_uiReady)
            {
                return;
            }

            _firstName = _firstNameInput != null ? _firstNameInput.text : _firstName;
            _lastName = _lastNameInput != null ? _lastNameInput.text : _lastName;
            _city = _cityInput != null ? _cityInput.text : _city;
            _region = _regionInput != null ? _regionInput.text : _region;
            _country = _countryInput != null ? _countryInput.text : _country;
            _countryCode = _countryCodeInput != null ? _countryCodeInput.text : _countryCode;
            _locale = _localeInput != null ? _localeInput.text : _locale;
            _avatarUrl = _avatarUrlInput != null ? _avatarUrlInput.text : _avatarUrl;
        }

        private void PushFormToUi()
        {
            if (!_uiReady)
            {
                return;
            }

            if (_firstNameInput != null) _firstNameInput.text = _firstName ?? string.Empty;
            if (_lastNameInput != null) _lastNameInput.text = _lastName ?? string.Empty;
            if (_cityInput != null) _cityInput.text = _city ?? string.Empty;
            if (_regionInput != null) _regionInput.text = _region ?? string.Empty;
            if (_countryInput != null) _countryInput.text = _country ?? string.Empty;
            if (_countryCodeInput != null) _countryCodeInput.text = _countryCode ?? string.Empty;
            if (_localeInput != null) _localeInput.text = _locale ?? string.Empty;
            if (_avatarUrlInput != null) _avatarUrlInput.text = _avatarUrl ?? string.Empty;
        }

        private void WireInputEvents()
        {
            _firstNameInput.onValueChanged.AddListener(value => _firstName = value);
            _lastNameInput.onValueChanged.AddListener(value => _lastName = value);
            _cityInput.onValueChanged.AddListener(value => _city = value);
            _regionInput.onValueChanged.AddListener(value => _region = value);
            _countryInput.onValueChanged.AddListener(value => _country = value);
            _countryCodeInput.onValueChanged.AddListener(value => _countryCode = value);
            _localeInput.onValueChanged.AddListener(value => _locale = value);
            _avatarUrlInput.onValueChanged.AddListener(value => _avatarUrl = value);
            _avatarUrlInput.onEndEdit.AddListener(value => RequestAvatarPreviewRefresh(value));
        }

        private async void OnClickRefresh()
        {
            await RunBusyGuardedAsync(async token => await FetchProfileAsync(token));
        }

        private async void OnClickSave()
        {
            await RunBusyGuardedAsync(async token => await SaveProfileAsync(token));
        }

        private async void OnClickPortfolio()
        {
            await RunBusyGuardedAsync(async token => await FetchPortfolioAsync(token));
        }

        private RectTransform CreatePanel(string name, Transform parent, Color background, Vector2 minOffset, Vector2 maxOffset)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image));
            go.transform.SetParent(parent, false);
            var rect = go.GetComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = minOffset;
            rect.offsetMax = maxOffset;
            go.GetComponent<Image>().color = background;
            return rect;
        }

        private TMP_Text CreateText(string name, Transform parent, string value, TMP_FontAsset font, float fontSize, Color color, FontStyles style)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI), typeof(LayoutElement));
            go.transform.SetParent(parent, false);
            var text = go.GetComponent<TextMeshProUGUI>();
            text.font = font;
            text.fontSize = fontSize;
            text.color = color;
            text.fontStyle = style;
            text.text = value ?? string.Empty;
            text.enableWordWrapping = true;
            text.alignment = TextAlignmentOptions.Left;
            go.GetComponent<LayoutElement>().preferredHeight = Mathf.Max(30f, fontSize + 12f);
            return text;
        }

        private TMP_InputField CreateInputRow(Transform parent, string label, string value, TMP_FontAsset font)
        {
            var row = new GameObject(label + " Row", typeof(RectTransform), typeof(HorizontalLayoutGroup), typeof(LayoutElement));
            row.transform.SetParent(parent, false);
            var rowLayout = row.GetComponent<HorizontalLayoutGroup>();
            rowLayout.spacing = 8f;
            rowLayout.childAlignment = TextAnchor.MiddleLeft;
            rowLayout.childControlHeight = true;
            rowLayout.childControlWidth = true;
            rowLayout.childForceExpandWidth = true;
            row.GetComponent<LayoutElement>().preferredHeight = 54f;

            var labelText = CreateText(label + " Label", row.transform, label, font, 20f, new Color(0.85f, 0.89f, 0.96f, 1f), FontStyles.Normal);
            var labelLayout = labelText.GetComponent<LayoutElement>();
            labelLayout.preferredWidth = 210f;
            labelLayout.flexibleWidth = 0f;

            var inputRoot = new GameObject(label + " Input", typeof(RectTransform), typeof(Image), typeof(TMP_InputField), typeof(LayoutElement));
            inputRoot.transform.SetParent(row.transform, false);
            inputRoot.GetComponent<Image>().color = new Color(0.16f, 0.19f, 0.24f, 0.98f);
            var inputLayout = inputRoot.GetComponent<LayoutElement>();
            inputLayout.flexibleWidth = 1f;
            inputLayout.minHeight = 44f;
            inputLayout.preferredHeight = 44f;

            var inputField = inputRoot.GetComponent<TMP_InputField>();

            var textArea = new GameObject("Text Area", typeof(RectTransform), typeof(RectMask2D));
            textArea.transform.SetParent(inputRoot.transform, false);
            var textAreaRect = textArea.GetComponent<RectTransform>();
            textAreaRect.anchorMin = Vector2.zero;
            textAreaRect.anchorMax = Vector2.one;
            textAreaRect.offsetMin = new Vector2(12f, 6f);
            textAreaRect.offsetMax = new Vector2(-12f, -6f);

            var text = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI)).GetComponent<TextMeshProUGUI>();
            text.transform.SetParent(textArea.transform, false);
            text.font = font;
            text.fontSize = 20f;
            text.color = Color.white;
            text.alignment = TextAlignmentOptions.Left;
            text.text = value ?? string.Empty;
            text.enableWordWrapping = false;
            var textRect = text.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;

            var placeholder = new GameObject("Placeholder", typeof(RectTransform), typeof(TextMeshProUGUI)).GetComponent<TextMeshProUGUI>();
            placeholder.transform.SetParent(textArea.transform, false);
            placeholder.font = font;
            placeholder.fontSize = 18f;
            placeholder.color = new Color(0.64f, 0.68f, 0.74f, 0.8f);
            placeholder.alignment = TextAlignmentOptions.Left;
            placeholder.text = label;
            placeholder.enableWordWrapping = false;
            var placeholderRect = placeholder.GetComponent<RectTransform>();
            placeholderRect.anchorMin = Vector2.zero;
            placeholderRect.anchorMax = Vector2.one;
            placeholderRect.offsetMin = Vector2.zero;
            placeholderRect.offsetMax = Vector2.zero;

            inputField.textViewport = textAreaRect;
            inputField.textComponent = text;
            inputField.placeholder = placeholder;
            inputField.lineType = TMP_InputField.LineType.SingleLine;
            inputField.caretColor = Color.white;
            inputField.selectionColor = new Color(0.32f, 0.52f, 0.92f, 0.35f);
            inputField.targetGraphic = inputRoot.GetComponent<Image>();
            inputField.readOnly = false;
            inputField.interactable = true;
            inputField.enabled = true;
            inputField.text = value ?? string.Empty;

            return inputField;
        }

        private RawImage CreateAvatarImagePanel(Transform parent)
        {
            var panel = new GameObject("AvatarPanel", typeof(RectTransform), typeof(Image), typeof(LayoutElement));
            panel.transform.SetParent(parent, false);
            panel.GetComponent<Image>().color = new Color(0.12f, 0.15f, 0.2f, 0.95f);
            var panelLayout = panel.GetComponent<LayoutElement>();
            panelLayout.preferredHeight = 200f;
            panelLayout.minHeight = 160f;

            var avatarGo = new GameObject("AvatarImage", typeof(RectTransform), typeof(RawImage));
            avatarGo.transform.SetParent(panel.transform, false);
            var avatarRect = avatarGo.GetComponent<RectTransform>();
            avatarRect.anchorMin = new Vector2(0.5f, 0.5f);
            avatarRect.anchorMax = new Vector2(0.5f, 0.5f);
            avatarRect.pivot = new Vector2(0.5f, 0.5f);
            avatarRect.sizeDelta = new Vector2(164f, 164f);
            avatarGo.GetComponent<RawImage>().color = new Color(1f, 1f, 1f, 0f);
            return avatarGo.GetComponent<RawImage>();
        }

        private void RequestAvatarPreviewRefresh(string rawUrl)
        {
            if (_avatarImage == null)
            {
                return;
            }

            if (_avatarPreviewCts != null)
            {
                _avatarPreviewCts.Cancel();
                _avatarPreviewCts.Dispose();
                _avatarPreviewCts = null;
            }

            _avatarPreviewCts = new CancellationTokenSource();
            _ = RefreshAvatarPreviewAsync(rawUrl, _avatarPreviewCts.Token);
        }

        private async Task RefreshAvatarPreviewAsync(string rawUrl, CancellationToken token)
        {
            var url = rawUrl == null ? string.Empty : rawUrl.Trim();
            if (string.IsNullOrWhiteSpace(url))
            {
                ApplyAvatarTexture(null, "No profile image URL set.");
                return;
            }

            if (!Uri.TryCreate(url, UriKind.Absolute, out var parsed) ||
                (parsed.Scheme != Uri.UriSchemeHttp && parsed.Scheme != Uri.UriSchemeHttps))
            {
                ApplyAvatarTexture(null, "Avatar URL is invalid. Use http/https.");
                return;
            }

            SetAvatarHint("Loading profile image...");
            try
            {
                using (var request = UnityWebRequestTexture.GetTexture(url, true))
                {
                    var operation = request.SendWebRequest();
                    while (!operation.isDone)
                    {
                        token.ThrowIfCancellationRequested();
                        await Task.Yield();
                    }

                    token.ThrowIfCancellationRequested();
                    if (request.result != UnityWebRequest.Result.Success)
                    {
                        ApplyAvatarTexture(null, "Could not load profile image.");
                        return;
                    }

                    var texture = DownloadHandlerTexture.GetContent(request);
                    ApplyAvatarTexture(texture, "Profile image loaded.");
                }
            }
            catch (OperationCanceledException)
            {
                // A newer avatar load request replaced this one.
            }
            catch (Exception)
            {
                ApplyAvatarTexture(null, "Failed to load profile image.");
            }
        }

        private void ApplyAvatarTexture(Texture2D texture, string hint)
        {
            if (_avatarTexture != null && _avatarTexture != texture)
            {
                Destroy(_avatarTexture);
            }

            _avatarTexture = texture;
            if (_avatarImage != null)
            {
                _avatarImage.texture = texture;
                _avatarImage.color = texture == null ? new Color(1f, 1f, 1f, 0f) : Color.white;
            }

            SetAvatarHint(hint);
        }

        private void SetAvatarHint(string hint)
        {
            if (_avatarHintText != null)
            {
                _avatarHintText.text = hint ?? string.Empty;
            }
        }

        private static void EnsureOnlyInputModuleEnabled(EventSystem eventSystem, BaseInputModule activeModule)
        {
            var modules = eventSystem.GetComponents<BaseInputModule>();
            for (var i = 0; i < modules.Length; i++)
            {
                modules[i].enabled = modules[i] == activeModule;
            }
        }

        private Button CreateButton(string text, Transform parent, TMP_FontAsset font, Color color, UnityEngine.Events.UnityAction onClick)
        {
            var buttonGo = new GameObject(text + " Button", typeof(RectTransform), typeof(Image), typeof(Button));
            buttonGo.transform.SetParent(parent, false);
            buttonGo.GetComponent<Image>().color = color;
            var button = buttonGo.GetComponent<Button>();
            button.onClick.AddListener(onClick);

            var label = new GameObject("Label", typeof(RectTransform), typeof(TextMeshProUGUI)).GetComponent<TextMeshProUGUI>();
            label.transform.SetParent(buttonGo.transform, false);
            label.font = font;
            label.fontSize = 20f;
            label.color = Color.white;
            label.alignment = TextAlignmentOptions.Center;
            label.fontStyle = FontStyles.Bold;
            label.text = text;
            var rect = label.GetComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;

            return button;
        }

        private TMP_Text CreateOutputPanel(Transform parent, TMP_FontAsset font)
        {
            var container = new GameObject("OutputPanel", typeof(RectTransform), typeof(Image), typeof(LayoutElement));
            container.transform.SetParent(parent, false);
            container.GetComponent<Image>().color = new Color(0.12f, 0.15f, 0.2f, 0.95f);
            var layout = container.GetComponent<LayoutElement>();
            layout.preferredHeight = 220f;
            layout.minHeight = 180f;

            var text = new GameObject("OutputText", typeof(RectTransform), typeof(TextMeshProUGUI)).GetComponent<TextMeshProUGUI>();
            text.transform.SetParent(container.transform, false);
            text.font = font;
            text.fontSize = 18f;
            text.color = new Color(0.87f, 0.91f, 0.99f, 1f);
            text.alignment = TextAlignmentOptions.TopLeft;
            text.enableWordWrapping = true;
            text.text = string.Empty;
            var rect = text.GetComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = new Vector2(12f, 10f);
            rect.offsetMax = new Vector2(-12f, -10f);

            return text;
        }

        private void OnGUI()
        {
            if (!renderOnGuiFallback || _uiReady)
            {
                return;
            }

            GUILayout.BeginArea(new Rect(12f, 12f, Mathf.Min(Screen.width - 24f, 740f), Screen.height - 24f), GUI.skin.box);
            fallbackScroll = GUILayout.BeginScrollView(fallbackScroll);
            GUILayout.Label("Building runtime Canvas UI...");
            GUILayout.Label("Status: " + _status);
            GUILayout.EndScrollView();
            GUILayout.EndArea();
        }
    }
}
