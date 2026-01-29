// ============================================================================
// IVXMoreOfUsCanvas.cs - Netflix-style App Carousel UI
// ============================================================================
// IntelliVerseX SDK - Cross-Promotion Feature
// Main canvas controller with horizontal scrolling carousel
// ============================================================================

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace IntelliVerseX.MoreOfUs.UI
{
    /// <summary>
    /// Netflix-style horizontal scrolling carousel for displaying other apps
    /// </summary>
    public class IVXMoreOfUsCanvas : MonoBehaviour
    {
        #region Serialized Fields

        [Header("Required References")]
        [SerializeField] private Canvas _canvas;
        [SerializeField] private CanvasGroup _canvasGroup;
        [SerializeField] private RectTransform _contentContainer;
        [SerializeField] private ScrollRect _scrollRect;
        [SerializeField] private HorizontalLayoutGroup _layoutGroup;
        [SerializeField] private IVXAppCard _cardPrefab;

        [Header("Header")]
        [SerializeField] private TextMeshProUGUI _titleText;
        [SerializeField] private TextMeshProUGUI _subtitleText;
        [SerializeField] private Button _closeButton;
        [SerializeField] private Button _refreshButton;

        [Header("Navigation Arrows")]
        [SerializeField] private Button _leftArrowButton;
        [SerializeField] private Button _rightArrowButton;
        [SerializeField] private CanvasGroup _leftArrowGroup;
        [SerializeField] private CanvasGroup _rightArrowGroup;

        [Header("Loading State")]
        [SerializeField] private GameObject _loadingPanel;
        [SerializeField] private TextMeshProUGUI _loadingText;
        [SerializeField] private Image _loadingSpinner;

        [Header("Empty State")]
        [SerializeField] private GameObject _emptyStatePanel;
        [SerializeField] private TextMeshProUGUI _emptyStateText;
        [SerializeField] private Button _retryButton;

        [Header("Scroll Settings")]
        [SerializeField] private float _scrollSpeed = 500f;
        [SerializeField] private float _snapDuration = 0.3f;
        [SerializeField] private bool _enableAutoScroll = true;
        [SerializeField] private float _autoScrollInterval = 5f;
        [SerializeField] private float _autoScrollPauseDuration = 10f;

        [Header("Card Settings")]
        [SerializeField] private float _cardWidth = 300f;
        [SerializeField] private float _cardHeight = 400f;
        [SerializeField] private float _cardSpacing = 20f;
        [SerializeField] private float _entranceDelay = 0.1f;

        [Header("Animation")]
        [SerializeField] private float _panelAnimationDuration = 0.4f;
        [SerializeField] private AnimationCurve _panelAnimationCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

        [Header("Customization")]
        [SerializeField] private string _defaultTitle = "More From Us";
        [SerializeField] private string _defaultSubtitle = "Check out our other games!";
        [SerializeField] private Color _backgroundColor = new Color(0.08f, 0.08f, 0.1f, 0.95f);

        [Header("Debug / Testing")]
        [Tooltip("If enabled, the panel will automatically show when Play mode starts (Editor only). Useful for testing.")]
        [SerializeField] private bool _showOnStartInEditor = true;

        #endregion

        #region Private Fields

        private readonly List<IVXAppCard> _activeCards = new List<IVXAppCard>();
        private readonly Queue<IVXAppCard> _cardPool = new Queue<IVXAppCard>();
        private Coroutine _autoScrollCoroutine;
        private Coroutine _animationCoroutine;
        private float _autoScrollPauseTime;
        private int _currentCardIndex;
        private bool _isDragging;
        private bool _isVisible;
        private bool _isInitialized;
        
        // Cached arrow states to avoid unnecessary updates
        private bool _lastCanScrollLeft;
        private bool _lastCanScrollRight;
        private float _arrowUpdateTimer;
        private const float ARROW_UPDATE_INTERVAL = 0.05f; // 20 FPS for arrows is plenty

        #endregion

        #region Events

        /// <summary>
        /// Fired when the panel is shown
        /// </summary>
        public event Action OnPanelShown;

        /// <summary>
        /// Fired when the panel is hidden
        /// </summary>
        public event Action OnPanelHidden;

        /// <summary>
        /// Fired when an app is selected
        /// </summary>
        public event Action<IVXUnifiedAppInfo> OnAppSelected;

        #endregion

        #region Properties

        /// <summary>
        /// Is the panel currently visible?
        /// </summary>
        public bool IsVisible => _isVisible;

        /// <summary>
        /// Number of apps currently displayed
        /// </summary>
        public int DisplayedAppCount => _activeCards.Count;

        #endregion

        #region Unity Lifecycle

        private void Awake()
        {
            Initialize();
        }

        private void Start()
        {
            // Check platform support (in builds only - Editor always allows for testing)
            if (!IsSupportedPlatform)
            {
                Debug.Log("[IVXMoreOfUs] Canvas disabled - unsupported platform (only Android/iOS supported in builds)");
                gameObject.SetActive(false);
                return;
            }

#if UNITY_EDITOR
            // Log build target info for debugging
            var buildTarget = UnityEditor.EditorUserBuildSettings.activeBuildTarget;
            if (buildTarget != UnityEditor.BuildTarget.Android && buildTarget != UnityEditor.BuildTarget.iOS)
            {
                Debug.Log($"[IVXMoreOfUs] Running in Editor with build target: {buildTarget}. Panel enabled for testing (would be disabled in non-mobile builds).");
            }
#endif

            // Subscribe to manager events safely
            if (IVXMoreOfUsManager.Instance != null)
            {
                IVXMoreOfUsManager.Instance.OnCatalogLoaded += OnCatalogLoaded;
                IVXMoreOfUsManager.Instance.OnLoadFailed += OnLoadFailed;
                IVXMoreOfUsManager.Instance.OnAppSelected += OnAppSelectedInternal;

                // Initial fetch if not already loaded
                if (!IVXMoreOfUsManager.Instance.HasCachedData)
                {
                    ShowLoadingState();
                    IVXMoreOfUsManager.Instance.FetchCatalog();
                }
                else
                {
                    // Data already cached - populate cards immediately (useful for Editor testing)
                    var apps = IVXMoreOfUsManager.Instance.GetAppsForCurrentPlatform();
                    if (apps.Count > 0)
                    {
                        PopulateCards(apps);
                    }
                    else
                    {
                        ShowEmptyState();
                    }
                }
            }
        }

        private void OnDestroy()
        {
            // Stop any running coroutines first
            StopAllCoroutines();
            
            // Use HasInstance to safely check without creating new instance during cleanup
            if (IVXMoreOfUsManager.HasInstance)
            {
                var instance = IVXMoreOfUsManager.Instance;
                if (instance != null)
                {
                    instance.OnCatalogLoaded -= OnCatalogLoaded;
                    instance.OnLoadFailed -= OnLoadFailed;
                    instance.OnAppSelected -= OnAppSelectedInternal;
                }
            }

            // Clean up cards safely
            if (_activeCards != null)
            {
                ClearCards();
            }
        }

        private void Update()
        {
            // Only update arrows when visible and we have cards
            if (!_isVisible || _activeCards.Count == 0)
                return;
                
            UpdateNavigationArrows();
        }

        #endregion

        #region Initialization

        private void Initialize()
        {
            if (_isInitialized)
                return;

            // Setup canvas
            if (_canvas == null)
                _canvas = GetComponent<Canvas>();
            if (_canvasGroup == null)
                _canvasGroup = GetComponent<CanvasGroup>();

            // Setup buttons
            if (_closeButton != null)
                _closeButton.onClick.AddListener(Hide);
            if (_refreshButton != null)
                _refreshButton.onClick.AddListener(RefreshCatalog);
            if (_retryButton != null)
                _retryButton.onClick.AddListener(RefreshCatalog);
            if (_leftArrowButton != null)
                _leftArrowButton.onClick.AddListener(ScrollLeft);
            if (_rightArrowButton != null)
                _rightArrowButton.onClick.AddListener(ScrollRight);

            // Setup scroll rect with optimized settings to prevent jitter
            if (_scrollRect != null)
            {
                _scrollRect.onValueChanged.AddListener(OnScrollValueChanged);
                
                // Optimize scroll rect for smooth scrolling without jitter
                _scrollRect.movementType = ScrollRect.MovementType.Elastic;
                _scrollRect.elasticity = 0.1f;
                _scrollRect.inertia = true;
                _scrollRect.decelerationRate = 0.135f;
                _scrollRect.scrollSensitivity = 1f;
                
                // Ensure horizontal only
                _scrollRect.horizontal = true;
                _scrollRect.vertical = false;
            }

            // Setup layout - CRITICAL: disable child force expand to prevent layout fighting
            if (_layoutGroup != null)
            {
                _layoutGroup.spacing = _cardSpacing;
                _layoutGroup.childForceExpandWidth = false;
                _layoutGroup.childForceExpandHeight = false;
                _layoutGroup.childControlWidth = false;
                _layoutGroup.childControlHeight = false;
                _layoutGroup.childAlignment = TextAnchor.MiddleLeft;
            }

            // Set default text
            if (_titleText != null)
                _titleText.text = _defaultTitle;
            if (_subtitleText != null)
                _subtitleText.text = _defaultSubtitle;

            // Initially hidden (unless in Editor with _showOnStartInEditor enabled)
#if UNITY_EDITOR
            if (!_showOnStartInEditor)
            {
                SetPanelVisible(false, immediate: true);
            }
            else
            {
                // Keep visible for Editor testing
                SetPanelVisible(true, immediate: true);
            }
#else
            SetPanelVisible(false, immediate: true);
#endif

            // Ensure layout groups are configured correctly to avoid overlap/misalignment
            // in older generated prefabs or hand-edited scenes.
            EnsureLayoutConfiguration();

            _isInitialized = true;
        }

        private void EnsureLayoutConfiguration()
        {
            try
            {
                // MainPanel should be driven by VerticalLayoutGroup.
                var mainPanel = transform.Find("MainPanel");
                if (mainPanel != null)
                {
                    var vlg = mainPanel.GetComponent<VerticalLayoutGroup>();
                    if (vlg != null)
                    {
                        // These must be enabled so children use LayoutElement sizing.
                        vlg.childControlHeight = true;
                        vlg.childControlWidth = true;
                        vlg.childForceExpandHeight = false;
                        vlg.childForceExpandWidth = true;
                    }
                }

                // Header should be driven by HorizontalLayoutGroup.
                var header = transform.Find("MainPanel/Header");
                if (header != null)
                {
                    var hlg = header.GetComponent<HorizontalLayoutGroup>();
                    if (hlg != null)
                    {
                        hlg.childControlHeight = true;
                        hlg.childControlWidth = true;
                        hlg.childForceExpandHeight = false;
                        hlg.childForceExpandWidth = false;
                    }
                }

                // Force a layout rebuild once.
                Canvas.ForceUpdateCanvases();
                var rootRect = transform as RectTransform;
                if (rootRect != null)
                {
                    LayoutRebuilder.ForceRebuildLayoutImmediate(rootRect);
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[IVXMoreOfUs] Layout configuration failed: {ex.Message}");
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Check if current platform is supported (Android or iOS).
        /// In Editor, always returns true to allow testing regardless of build target.
        /// </summary>
        public static bool IsSupportedPlatform
        {
            get
            {
#if UNITY_EDITOR
                // Always allow in Editor for testing/preview purposes
                return true;
#elif UNITY_ANDROID
                return true;
#elif UNITY_IOS
                return true;
#else
                return false;
#endif
            }
        }

        /// <summary>
        /// Show the "More Of Us" panel.
        /// Only shows on supported platforms (Android/iOS).
        /// </summary>
        public void Show()
        {
            if (_isVisible)
                return;

            // Check platform support
            if (!IsSupportedPlatform)
            {
                Debug.Log("[IVXMoreOfUs] Panel not shown - unsupported platform. Only Android and iOS are supported.");
                return;
            }

            gameObject.SetActive(true);
            SetPanelVisible(true);

            // Populate cards if we have data
            if (IVXMoreOfUsManager.HasInstance && IVXMoreOfUsManager.Instance.HasCachedData)
            {
                var apps = IVXMoreOfUsManager.Instance.GetAppsForCurrentPlatform();
                if (apps.Count > 0)
                {
                    PopulateCards(apps);
                }
                else
                {
                    ShowEmptyState();
                }
            }
            else
            {
                ShowLoadingState();
                IVXMoreOfUsManager.Instance?.FetchCatalog();
            }

            // Start auto-scroll
            if (_enableAutoScroll)
                StartAutoScroll();

            OnPanelShown?.Invoke();
        }

        /// <summary>
        /// Hide the panel
        /// </summary>
        public void Hide()
        {
            if (!_isVisible)
                return;

            StopAutoScroll();
            SetPanelVisible(false);

            OnPanelHidden?.Invoke();
        }

        /// <summary>
        /// Toggle panel visibility
        /// </summary>
        public void Toggle()
        {
            if (_isVisible)
                Hide();
            else
                Show();
        }

        /// <summary>
        /// Refresh the app catalog
        /// </summary>
        public void RefreshCatalog()
        {
            ShowLoadingState();
            IVXMoreOfUsManager.Instance.FetchCatalog(forceRefresh: true);
        }

        /// <summary>
        /// Scroll to a specific card index
        /// </summary>
        public void ScrollToCard(int index)
        {
            if (_activeCards.Count == 0 || _scrollRect == null)
                return;

            index = Mathf.Clamp(index, 0, _activeCards.Count - 1);
            _currentCardIndex = index;

            float targetPosition = CalculateScrollPosition(index);
            StartCoroutine(SmoothScrollTo(targetPosition));

            // Pause auto-scroll
            PauseAutoScroll();
        }

        /// <summary>
        /// Scroll left by one card
        /// </summary>
        public void ScrollLeft()
        {
            ScrollToCard(_currentCardIndex - 1);
        }

        /// <summary>
        /// Scroll right by one card
        /// </summary>
        public void ScrollRight()
        {
            ScrollToCard(_currentCardIndex + 1);
        }

        #endregion

        #region Card Management

        private void PopulateCards(List<IVXUnifiedAppInfo> apps)
        {
            ClearCards();

            if (apps == null || apps.Count == 0)
            {
                ShowEmptyState();
                return;
            }

            HideLoadingState();
            HideEmptyState();

            // Create cards
            for (int i = 0; i < apps.Count; i++)
            {
                var card = GetOrCreateCard();
                card.transform.SetParent(_contentContainer, false);
                card.gameObject.SetActive(true);

                // Set size
                var rectTransform = card.GetComponent<RectTransform>();
                rectTransform.sizeDelta = new Vector2(_cardWidth, _cardHeight);

                // Initialize
                card.Initialize(apps[i], i);
                card.OnCardClicked += OnCardClicked;
                card.OnCardHoverEnter += OnCardHovered;

                _activeCards.Add(card);

                // Animate entrance
                card.PlayEntranceAnimation(_entranceDelay * i);
            }

            // Reset scroll position
            if (_scrollRect != null)
                _scrollRect.horizontalNormalizedPosition = 0;
            _currentCardIndex = 0;

            Debug.Log($"[IVXMoreOfUs] Populated {apps.Count} app cards");
        }

        private void ClearCards()
        {
            foreach (var card in _activeCards)
            {
                if (card != null)
                {
                    card.OnCardClicked -= OnCardClicked;
                    card.OnCardHoverEnter -= OnCardHovered;
                    card.Reset();
                    card.gameObject.SetActive(false);
                    _cardPool.Enqueue(card);
                }
            }
            _activeCards.Clear();
        }

        private IVXAppCard GetOrCreateCard()
        {
            if (_cardPool.Count > 0)
                return _cardPool.Dequeue();

            if (_cardPrefab != null)
            {
                var card = Instantiate(_cardPrefab);
                return card;
            }

            // Create basic card if no prefab
            var go = new GameObject("AppCard");
            return go.AddComponent<IVXAppCard>();
        }

        #endregion

        #region Scrolling

        private void OnScrollValueChanged(Vector2 value)
        {
            // Update current card index based on scroll position
            if (_activeCards.Count > 0)
            {
                float totalWidth = _activeCards.Count * (_cardWidth + _cardSpacing);
                float scrollPosition = (1 - value.x) * totalWidth;
                _currentCardIndex = Mathf.FloorToInt(scrollPosition / (_cardWidth + _cardSpacing));
                _currentCardIndex = Mathf.Clamp(_currentCardIndex, 0, _activeCards.Count - 1);
            }
        }

        private float CalculateScrollPosition(int cardIndex)
        {
            if (_activeCards.Count <= 1)
                return 0;

            float totalWidth = _activeCards.Count * (_cardWidth + _cardSpacing) - _cardSpacing;
            float viewportWidth = _scrollRect.viewport.rect.width;
            float scrollableWidth = Mathf.Max(0, totalWidth - viewportWidth);

            if (scrollableWidth <= 0)
                return 0;

            float cardPosition = cardIndex * (_cardWidth + _cardSpacing);
            float normalizedPosition = cardPosition / scrollableWidth;

            return 1 - Mathf.Clamp01(normalizedPosition);
        }

        private IEnumerator SmoothScrollTo(float targetPosition)
        {
            float startPosition = _scrollRect.horizontalNormalizedPosition;
            float elapsed = 0;

            while (elapsed < _snapDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = _panelAnimationCurve.Evaluate(elapsed / _snapDuration);
                _scrollRect.horizontalNormalizedPosition = Mathf.Lerp(startPosition, targetPosition, t);
                yield return null;
            }

            _scrollRect.horizontalNormalizedPosition = targetPosition;
        }

        private void UpdateNavigationArrows()
        {
            if (_leftArrowGroup == null || _rightArrowGroup == null)
                return;

            // Throttle arrow updates to reduce per-frame work
            _arrowUpdateTimer += Time.unscaledDeltaTime;
            
            bool canScrollLeft = _currentCardIndex > 0;
            bool canScrollRight = _currentCardIndex < _activeCards.Count - 1;
            
            // Only update interactable state when scroll state changes
            if (canScrollLeft != _lastCanScrollLeft || canScrollRight != _lastCanScrollRight)
            {
                _lastCanScrollLeft = canScrollLeft;
                _lastCanScrollRight = canScrollRight;
                
                if (_leftArrowButton != null)
                    _leftArrowButton.interactable = canScrollLeft;
                if (_rightArrowButton != null)
                    _rightArrowButton.interactable = canScrollRight;
            }

            // Throttle alpha lerp updates
            if (_arrowUpdateTimer < ARROW_UPDATE_INTERVAL)
                return;
            _arrowUpdateTimer = 0f;

            // Fade arrows
            float leftAlpha = canScrollLeft ? 1f : 0.3f;
            float rightAlpha = canScrollRight ? 1f : 0.3f;

            float lerpSpeed = ARROW_UPDATE_INTERVAL * 10f;
            _leftArrowGroup.alpha = Mathf.Lerp(_leftArrowGroup.alpha, leftAlpha, lerpSpeed);
            _rightArrowGroup.alpha = Mathf.Lerp(_rightArrowGroup.alpha, rightAlpha, lerpSpeed);
        }

        #endregion

        #region Auto-Scroll

        private void StartAutoScroll()
        {
            if (!_enableAutoScroll || _autoScrollCoroutine != null)
                return;

            _autoScrollCoroutine = StartCoroutine(AutoScrollCoroutine());
        }

        private void StopAutoScroll()
        {
            if (_autoScrollCoroutine != null)
            {
                StopCoroutine(_autoScrollCoroutine);
                _autoScrollCoroutine = null;
            }
        }

        private void PauseAutoScroll()
        {
            _autoScrollPauseTime = Time.unscaledTime + _autoScrollPauseDuration;
        }

        private IEnumerator AutoScrollCoroutine()
        {
            while (_isVisible)
            {
                yield return new WaitForSecondsRealtime(_autoScrollInterval);

                // Skip if paused or dragging
                if (Time.unscaledTime < _autoScrollPauseTime || _isDragging)
                    continue;

                // Scroll to next card, or loop back
                int nextIndex = _currentCardIndex + 1;
                if (nextIndex >= _activeCards.Count)
                    nextIndex = 0;

                ScrollToCard(nextIndex);
            }
        }

        #endregion

        #region Panel Visibility

        private void SetPanelVisible(bool visible, bool immediate = false)
        {
            _isVisible = visible;

            if (_animationCoroutine != null)
                StopCoroutine(_animationCoroutine);

            if (immediate)
            {
                if (_canvasGroup != null)
                {
                    _canvasGroup.alpha = visible ? 1f : 0f;
                    _canvasGroup.interactable = visible;
                    _canvasGroup.blocksRaycasts = visible;
                }
                if (!visible)
                    gameObject.SetActive(false);
            }
            else
            {
                _animationCoroutine = StartCoroutine(AnimatePanelVisibility(visible));
            }
        }

        private IEnumerator AnimatePanelVisibility(bool visible)
        {
            float startAlpha = _canvasGroup.alpha;
            float targetAlpha = visible ? 1f : 0f;
            float elapsed = 0;

            while (elapsed < _panelAnimationDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = _panelAnimationCurve.Evaluate(elapsed / _panelAnimationDuration);
                
                _canvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, t);
                yield return null;
            }

            _canvasGroup.alpha = targetAlpha;
            _canvasGroup.interactable = visible;
            _canvasGroup.blocksRaycasts = visible;

            if (!visible)
                gameObject.SetActive(false);

            _animationCoroutine = null;
        }

        #endregion

        #region State Display

        private void ShowLoadingState()
        {
            if (_loadingPanel != null)
                _loadingPanel.SetActive(true);
            if (_emptyStatePanel != null)
                _emptyStatePanel.SetActive(false);
        }

        private void HideLoadingState()
        {
            if (_loadingPanel != null)
                _loadingPanel.SetActive(false);
        }

        private void ShowEmptyState()
        {
            if (_loadingPanel != null)
                _loadingPanel.SetActive(false);
            if (_emptyStatePanel != null)
            {
                _emptyStatePanel.SetActive(true);
                if (_emptyStateText != null)
                    _emptyStateText.text = "No other apps available at this time.\nCheck back later!";
            }
        }

        private void HideEmptyState()
        {
            if (_emptyStatePanel != null)
                _emptyStatePanel.SetActive(false);
        }

        #endregion

        #region Event Handlers

        private void OnCatalogLoaded(IVXMergedAppCatalog catalog)
        {
            if (!_isVisible)
                return;

            var apps = catalog.GetAppsForCurrentPlatform();
            PopulateCards(apps);
        }

        private void OnLoadFailed(string error)
        {
            if (!_isVisible)
                return;

            HideLoadingState();
            
            if (_emptyStatePanel != null)
            {
                _emptyStatePanel.SetActive(true);
                if (_emptyStateText != null)
                    _emptyStateText.text = $"Failed to load apps.\n{error}\n\nTap to retry.";
            }
        }

        private void OnCardClicked(IVXAppCard card, IVXUnifiedAppInfo appInfo)
        {
            OnAppSelected?.Invoke(appInfo);
        }

        private void OnCardHovered(IVXAppCard card)
        {
            // Pause auto-scroll when user interacts
            PauseAutoScroll();
        }

        private void OnAppSelectedInternal(IVXUnifiedAppInfo appInfo)
        {
            // Could log analytics here
            Debug.Log($"[IVXMoreOfUs] App selected: {appInfo.appName}");
        }

        #endregion

        #region Static Factory

        /// <summary>
        /// Create a new "More Of Us" canvas instance
        /// </summary>
        public static IVXMoreOfUsCanvas Create(Transform parent = null)
        {
            var prefab = Resources.Load<IVXMoreOfUsCanvas>("IntelliVerseX/MoreOfUs/IVX_MoreOfUsCanvas");
            if (prefab != null)
            {
                var instance = Instantiate(prefab, parent);
                return instance;
            }

            // Create basic canvas if no prefab found
            var go = new GameObject("IVX_MoreOfUsCanvas");
            if (parent != null)
                go.transform.SetParent(parent, false);

            var canvas = go.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 100;

            go.AddComponent<CanvasScaler>();
            go.AddComponent<GraphicRaycaster>();

            var moreOfUs = go.AddComponent<IVXMoreOfUsCanvas>();
            return moreOfUs;
        }

        #endregion
    }
}
