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
            // Subscribe to manager events
            IVXMoreOfUsManager.Instance.OnCatalogLoaded += OnCatalogLoaded;
            IVXMoreOfUsManager.Instance.OnLoadFailed += OnLoadFailed;
            IVXMoreOfUsManager.Instance.OnAppSelected += OnAppSelectedInternal;

            // Initial fetch if not already loaded
            if (!IVXMoreOfUsManager.Instance.HasCachedData)
            {
                ShowLoadingState();
                IVXMoreOfUsManager.Instance.FetchCatalog();
            }
        }

        private void OnDestroy()
        {
            // Unsubscribe from manager events
            if (IVXMoreOfUsManager.Instance != null)
            {
                IVXMoreOfUsManager.Instance.OnCatalogLoaded -= OnCatalogLoaded;
                IVXMoreOfUsManager.Instance.OnLoadFailed -= OnLoadFailed;
                IVXMoreOfUsManager.Instance.OnAppSelected -= OnAppSelectedInternal;
            }

            // Clean up cards
            ClearCards();
        }

        private void Update()
        {
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

            // Setup scroll rect
            if (_scrollRect != null)
            {
                _scrollRect.onValueChanged.AddListener(OnScrollValueChanged);
            }

            // Setup layout
            if (_layoutGroup != null)
            {
                _layoutGroup.spacing = _cardSpacing;
                _layoutGroup.childForceExpandWidth = false;
                _layoutGroup.childForceExpandHeight = false;
                _layoutGroup.childControlWidth = false;
                _layoutGroup.childControlHeight = false;
            }

            // Set default text
            if (_titleText != null)
                _titleText.text = _defaultTitle;
            if (_subtitleText != null)
                _subtitleText.text = _defaultSubtitle;

            // Initially hidden
            SetPanelVisible(false, immediate: true);

            _isInitialized = true;
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Show the "More Of Us" panel
        /// </summary>
        public void Show()
        {
            if (_isVisible)
                return;

            gameObject.SetActive(true);
            SetPanelVisible(true);

            // Populate cards if we have data
            if (IVXMoreOfUsManager.Instance.HasCachedData)
            {
                PopulateCards(IVXMoreOfUsManager.Instance.GetAppsForCurrentPlatform());
            }
            else
            {
                ShowLoadingState();
                IVXMoreOfUsManager.Instance.FetchCatalog();
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

            bool canScrollLeft = _currentCardIndex > 0;
            bool canScrollRight = _currentCardIndex < _activeCards.Count - 1;

            // Fade arrows
            float leftAlpha = canScrollLeft ? 1f : 0.3f;
            float rightAlpha = canScrollRight ? 1f : 0.3f;

            _leftArrowGroup.alpha = Mathf.Lerp(_leftArrowGroup.alpha, leftAlpha, Time.unscaledDeltaTime * 10f);
            _rightArrowGroup.alpha = Mathf.Lerp(_rightArrowGroup.alpha, rightAlpha, Time.unscaledDeltaTime * 10f);

            if (_leftArrowButton != null)
                _leftArrowButton.interactable = canScrollLeft;
            if (_rightArrowButton != null)
                _rightArrowButton.interactable = canScrollRight;
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
