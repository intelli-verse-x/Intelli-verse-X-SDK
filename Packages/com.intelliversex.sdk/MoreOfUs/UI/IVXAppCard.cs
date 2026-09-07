// ============================================================================
// IVXAppCard.cs - Netflix-style App Card UI Component
// ============================================================================
// IntelliVerseX SDK - Cross-Promotion Feature
// Individual app card with hover animations and interactions
// 
// ARCHITECTURE: Layout Container + Visual Child pattern
// - The card itself is a layout placeholder (never scales/moves)
// - A child "VisualContainer" handles all visual scaling
// - This prevents layout fighting with animations
// ============================================================================

using System;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

#if DOTWEEN || DOTWEEN_ENABLED
using DG.Tweening;
#endif

namespace IntelliVerseX.MoreOfUs.UI
{
    /// <summary>
    /// Individual app card component with Netflix-style hover effects.
    /// Uses a Layout Container + Visual Child architecture to prevent
    /// layout conflicts during animation.
    /// </summary>
    [RequireComponent(typeof(RectTransform))]
    [RequireComponent(typeof(LayoutElement))]
    public class IVXAppCard : MonoBehaviour, 
        IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
    {
        #region Serialized Fields

        [Header("Visual Container (REQUIRED)")]
        [Tooltip("The child RectTransform that contains all visual elements. This is what gets scaled during hover.")]
        [SerializeField] private RectTransform _visualContainer;
        
        [Header("Required References")]
        [SerializeField] private RawImage _appIcon;
        [SerializeField] private TextMeshProUGUI _appNameText;
        [SerializeField] private TextMeshProUGUI _descriptionText;
        [SerializeField] private TextMeshProUGUI _ratingText;
        [SerializeField] private TextMeshProUGUI _priceText;
        [SerializeField] private GameObject _freeLabel;
        [SerializeField] private Button _installButton;
        [SerializeField] private CanvasGroup _detailsPanel;
        [SerializeField] private Image _cardBackground;

        [Header("Rating Stars")]
        [SerializeField] private Transform _starsContainer;
        [SerializeField] private Sprite _starFilled;
        [SerializeField] private Sprite _starEmpty;
        [SerializeField] private Sprite _starHalf;

        [Header("Animation Settings")]
        [SerializeField] private float _hoverScale = 1.08f;
        [SerializeField] private float _animationDuration = 0.15f;
        [SerializeField] private AnimationCurve _scaleCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

        [Header("Visual Settings")]
        [SerializeField] private Color _normalBackgroundColor = new Color(0.15f, 0.15f, 0.18f, 1f);
        [SerializeField] private Color _hoverBackgroundColor = new Color(0.2f, 0.2f, 0.25f, 1f);

        [Header("Loading State")]
        [SerializeField] private GameObject _loadingSpinner;
        [SerializeField] private Sprite _placeholderIcon;

        #endregion

        #region Private Fields

        private IVXUnifiedAppInfo _appInfo;
        private RectTransform _rectTransform;
        private LayoutElement _layoutElement;
        private Canvas _overrideCanvas;
        private GraphicRaycaster _overrideRaycaster;
        private Coroutine _animationCoroutine;
        private bool _isHovered;
        private bool _isInitialized;
        private int _cardIndex;
        private int _originalSortingOrder;
        
        // Cached star images to avoid GC allocations
        private Image[] _cachedStarImages;
        private bool _starsCached;
        
        // Animation state
        private bool _isAnimating;

    #if DOTWEEN || DOTWEEN_ENABLED
        private Tween _scaleTween;
        private Tween _colorTween;
        private Tween _alphaTween;
    #endif

        #endregion

        #region Events

        /// <summary>
        /// Fired when this card is clicked
        /// </summary>
        public event Action<IVXAppCard, IVXUnifiedAppInfo> OnCardClicked;

        /// <summary>
        /// Fired when pointer enters the card
        /// </summary>
        public event Action<IVXAppCard> OnCardHoverEnter;

        /// <summary>
        /// Fired when pointer exits the card
        /// </summary>
        public event Action<IVXAppCard> OnCardHoverExit;

        #endregion

        #region Properties

        /// <summary>
        /// The app info displayed by this card
        /// </summary>
        public IVXUnifiedAppInfo AppInfo => _appInfo;

        /// <summary>
        /// Index of this card in the carousel
        /// </summary>
        public int CardIndex
        {
            get => _cardIndex;
            set => _cardIndex = value;
        }

        /// <summary>
        /// Is this card currently being hovered?
        /// </summary>
        public bool IsHovered => _isHovered;

        #endregion

        #region Unity Lifecycle

        private void Awake()
        {
            _rectTransform = GetComponent<RectTransform>();
            _layoutElement = GetComponent<LayoutElement>();
            
            // Ensure layout element exists and has proper settings
            if (_layoutElement == null)
            {
                _layoutElement = gameObject.AddComponent<LayoutElement>();
            }
            
            // Ensure pivot is centered (0.5, 0.5) to prevent offset during scale
            _rectTransform.pivot = new Vector2(0.5f, 0.5f);
            
            // Setup visual container - create if not assigned
            SetupVisualContainer();
            
            // Setup override canvas for z-ordering (disabled by default)
            SetupOverrideCanvas();

            if (_installButton != null)
                _installButton.onClick.AddListener(OnInstallButtonClicked);

            // Hide details panel initially
            if (_detailsPanel != null)
            {
                _detailsPanel.alpha = 0;
                _detailsPanel.interactable = false;
                _detailsPanel.blocksRaycasts = false;
            }

            // Show loading spinner initially
            SetLoadingState(true);
        }

        private void OnDestroy()
        {
            // Stop coroutines first
            StopAllCoroutines();
            _animationCoroutine = null;
            
            // Remove button listener
            if (_installButton != null)
                _installButton.onClick.RemoveListener(OnInstallButtonClicked);

            // Clear event subscribers to prevent memory leaks
            OnCardClicked = null;
            OnCardHoverEnter = null;
            OnCardHoverExit = null;
            
            // Clear cached references
            _cachedStarImages = null;

            KillTweens();
        }

        #endregion

        #region Setup Methods

        /// <summary>
        /// Sets up the visual container for animations.
        /// If not assigned, uses the first child or creates one.
        /// </summary>
        private void SetupVisualContainer()
        {
            if (_visualContainer != null)
            {
                // Ensure visual container has centered pivot
                _visualContainer.pivot = new Vector2(0.5f, 0.5f);
                _visualContainer.anchorMin = Vector2.zero;
                _visualContainer.anchorMax = Vector2.one;
                _visualContainer.offsetMin = Vector2.zero;
                _visualContainer.offsetMax = Vector2.zero;
                _visualContainer.localScale = Vector3.one;
                return;
            }

            // Try to find existing visual container by name
            var existingContainer = transform.Find("VisualContainer");
            if (existingContainer != null)
            {
                _visualContainer = existingContainer as RectTransform;
                if (_visualContainer != null)
                {
                    _visualContainer.pivot = new Vector2(0.5f, 0.5f);
                    return;
                }
            }

            // If first child exists and has children, use it as visual container
            if (transform.childCount > 0)
            {
                var firstChild = transform.GetChild(0);
                if (firstChild.childCount > 0)
                {
                    _visualContainer = firstChild as RectTransform;
                    if (_visualContainer != null)
                    {
                        _visualContainer.pivot = new Vector2(0.5f, 0.5f);
                        return;
                    }
                }
            }

            // Create visual container if none exists and reparent all children
            var containerGO = new GameObject("VisualContainer", typeof(RectTransform));
            _visualContainer = containerGO.GetComponent<RectTransform>();
            _visualContainer.SetParent(transform, false);
            _visualContainer.pivot = new Vector2(0.5f, 0.5f);
            _visualContainer.anchorMin = Vector2.zero;
            _visualContainer.anchorMax = Vector2.one;
            _visualContainer.offsetMin = Vector2.zero;
            _visualContainer.offsetMax = Vector2.zero;
            _visualContainer.localScale = Vector3.one;

            // Move all existing children into visual container
            for (int i = transform.childCount - 1; i >= 0; i--)
            {
                var child = transform.GetChild(i);
                if (child != _visualContainer.transform)
                {
                    child.SetParent(_visualContainer, true);
                }
            }
        }

        /// <summary>
        /// Sets up an override canvas for z-ordering during hover.
        /// This allows the card to render on top of siblings.
        /// </summary>
        private void SetupOverrideCanvas()
        {
            // Add Canvas component for sorting override (disabled by default)
            _overrideCanvas = GetComponent<Canvas>();
            if (_overrideCanvas == null)
            {
                _overrideCanvas = gameObject.AddComponent<Canvas>();
            }
            _overrideCanvas.overrideSorting = false;
            
            // Add GraphicRaycaster so the card remains interactive
            _overrideRaycaster = GetComponent<GraphicRaycaster>();
            if (_overrideRaycaster == null)
            {
                _overrideRaycaster = gameObject.AddComponent<GraphicRaycaster>();
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Initialize the card with app data
        /// </summary>
        public void Initialize(IVXUnifiedAppInfo appInfo, int index = 0)
        {
            _appInfo = appInfo;
            _cardIndex = index;
            _isInitialized = true;

            UpdateUI();
            LoadIcon();
        }

        /// <summary>
        /// Refresh the card display
        /// </summary>
        public void Refresh()
        {
            if (_appInfo != null)
                UpdateUI();
        }

        /// <summary>
        /// Reset the card to empty state
        /// </summary>
        public void Reset()
        {
            _appInfo = null;
            _isInitialized = false;
            _isHovered = false;
            _isAnimating = false;
            
            // Clear cached stars when card is recycled
            _starsCached = false;
            _cachedStarImages = null;

            if (_appNameText != null)
                _appNameText.text = "";
            if (_descriptionText != null)
                _descriptionText.text = "";
            if (_appIcon != null)
                _appIcon.texture = null;

            SetLoadingState(true);
            
            // Reset visual container scale
            if (_visualContainer != null)
                _visualContainer.localScale = Vector3.one;
            
            // Disable override canvas
            if (_overrideCanvas != null)
                _overrideCanvas.overrideSorting = false;
            
            // Reset details panel
            if (_detailsPanel != null)
            {
                _detailsPanel.alpha = 0;
                _detailsPanel.interactable = false;
                _detailsPanel.blocksRaycasts = false;
            }
            
            // Reset background color
            if (_cardBackground != null)
                _cardBackground.color = _normalBackgroundColor;
        }

        /// <summary>
        /// Play the card entrance animation
        /// </summary>
        public void PlayEntranceAnimation(float delay = 0f)
        {
            if (_visualContainer == null)
                return;

#if DOTWEEN || DOTWEEN_ENABLED
            PlayEntranceAnimationDOTween(delay);
#else
            StartCoroutine(EntranceAnimationCoroutine(delay));
#endif
        }

        #endregion

        #region UI Updates

        private void UpdateUI()
        {
            if (_appInfo == null)
                return;

            // App name
            if (_appNameText != null)
                _appNameText.text = _appInfo.appName;

            // Description
            if (_descriptionText != null)
                _descriptionText.text = _appInfo.GetShortDescription(100);

            // Rating
            if (_ratingText != null)
            {
                if (_appInfo.rating > 0)
                    _ratingText.text = $"{_appInfo.rating:F1}";
                else
                    _ratingText.text = "New";
            }

            // Update star rating display
            UpdateStarRating(_appInfo.rating);

            // Price / Free label
            if (_priceText != null)
                _priceText.text = _appInfo.GetPriceDisplay();

            if (_freeLabel != null)
                _freeLabel.SetActive(_appInfo.isFree);

            // Background color
            if (_cardBackground != null)
                _cardBackground.color = _normalBackgroundColor;

            // Hide loading state once we have data
            SetLoadingState(false);
        }

        private void UpdateStarRating(float rating)
        {
            if (_starsContainer == null || _starFilled == null || _starEmpty == null)
                return;

            // Cache star images once to avoid GC allocations
            if (!_starsCached || _cachedStarImages == null)
            {
                _cachedStarImages = _starsContainer.GetComponentsInChildren<Image>();
                _starsCached = true;
            }

            int starCount = Mathf.Min(_cachedStarImages.Length, 5);
            for (int i = 0; i < starCount; i++)
            {
                float starThreshold = i + 1;
                if (rating >= starThreshold)
                    _cachedStarImages[i].sprite = _starFilled;
                else if (rating >= starThreshold - 0.5f && _starHalf != null)
                    _cachedStarImages[i].sprite = _starHalf;
                else
                    _cachedStarImages[i].sprite = _starEmpty;
            }
        }

        private void LoadIcon()
        {
            if (_appInfo == null || _appIcon == null)
                return;

            // Check if already cached
            if (_appInfo.cachedIcon != null)
            {
                _appIcon.texture = _appInfo.cachedIcon;
                return;
            }

            // Show placeholder
            if (_placeholderIcon != null)
                _appIcon.texture = _placeholderIcon.texture;

            // Load from manager safely
            if (!IVXMoreOfUsManager.HasInstance)
                return;
                
            IVXMoreOfUsManager.Instance.LoadAppIcon(_appInfo, texture =>
            {
                if (texture != null && this != null && _appIcon != null)
                {
                    _appIcon.texture = texture;
                    _appInfo.cachedIcon = texture;
                }
            });
        }

        private void SetLoadingState(bool isLoading)
        {
            if (_loadingSpinner != null)
                _loadingSpinner.SetActive(isLoading);
        }

        #endregion

        #region Pointer Events

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (!_isInitialized || _visualContainer == null)
                return;

            _isHovered = true;
            OnCardHoverEnter?.Invoke(this);

            // Bring to front using override canvas
            BringToFront();
            
            if (_detailsPanel != null)
            {
                _detailsPanel.interactable = true;
                _detailsPanel.blocksRaycasts = true;
            }
            
            // Animate to hover state
            AnimateToState(true);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (!_isInitialized || _visualContainer == null)
                return;

            _isHovered = false;
            OnCardHoverExit?.Invoke(this);
            
            // Animate back to normal
            AnimateToState(false);
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (!_isInitialized || _appInfo == null)
                return;

            OnCardClicked?.Invoke(this, _appInfo);
            
            // Open store page safely
            if (IVXMoreOfUsManager.HasInstance)
            {
                IVXMoreOfUsManager.Instance.OpenStorePage(_appInfo);
            }
        }

        private void OnInstallButtonClicked()
        {
            if (_appInfo == null)
                return;

            // Open store page safely
            if (IVXMoreOfUsManager.HasInstance)
            {
                IVXMoreOfUsManager.Instance.OpenStorePage(_appInfo);
            }
        }

        #endregion

        #region Animations

        private void AnimateToState(bool hovered)
        {
            if (_visualContainer == null)
                return;

            _isAnimating = true;

#if DOTWEEN || DOTWEEN_ENABLED
            AnimateToStateDOTween(hovered);
#else
            if (_animationCoroutine != null)
                StopCoroutine(_animationCoroutine);

            _animationCoroutine = StartCoroutine(AnimateCoroutine(hovered));
#endif
        }

        private IEnumerator AnimateCoroutine(bool hovered)
        {
            Vector3 startScale = _visualContainer.localScale;
            Vector3 targetScale = hovered ? Vector3.one * _hoverScale : Vector3.one;
            
            Color startColor = _cardBackground != null ? _cardBackground.color : _normalBackgroundColor;
            Color targetColor = hovered ? _hoverBackgroundColor : _normalBackgroundColor;
            
            float startAlpha = _detailsPanel != null ? _detailsPanel.alpha : 0;
            float targetAlpha = hovered ? 1f : 0f;

            float elapsed = 0;
            while (elapsed < _animationDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = _scaleCurve.Evaluate(elapsed / _animationDuration);

                // Scale the VISUAL CONTAINER only (not the layout parent)
                _visualContainer.localScale = Vector3.Lerp(startScale, targetScale, t);

                // Background color
                if (_cardBackground != null)
                    _cardBackground.color = Color.Lerp(startColor, targetColor, t);

                // Details panel alpha
                if (_detailsPanel != null)
                    _detailsPanel.alpha = Mathf.Lerp(startAlpha, targetAlpha, t);

                yield return null;
            }

            // Ensure final values
            _visualContainer.localScale = targetScale;
            if (_cardBackground != null)
                _cardBackground.color = targetColor;
            if (_detailsPanel != null)
            {
                _detailsPanel.alpha = targetAlpha;
                _detailsPanel.interactable = hovered;
                _detailsPanel.blocksRaycasts = hovered;
            }

            _isAnimating = false;
            
            if (!hovered)
                SendToBack();

            _animationCoroutine = null;
        }

        private IEnumerator EntranceAnimationCoroutine(float delay)
        {
            // Initial state
            _visualContainer.localScale = Vector3.zero;
            
            if (delay > 0)
                yield return new WaitForSeconds(delay);

            float elapsed = 0;
            float duration = _animationDuration * 1.5f;

            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = _scaleCurve.Evaluate(elapsed / duration);
                
                // Simple scale up with slight overshoot
                float overshoot = 1f + Mathf.Sin(t * Mathf.PI) * 0.05f;
                _visualContainer.localScale = Vector3.one * t * overshoot;

                yield return null;
            }

            _visualContainer.localScale = Vector3.one;
        }

#if DOTWEEN || DOTWEEN_ENABLED
        private void PlayEntranceAnimationDOTween(float delay)
        {
            KillTweens();

            _visualContainer.localScale = Vector3.zero;

            _scaleTween = _visualContainer.DOScale(Vector3.one, _animationDuration * 1.5f)
                .SetDelay(delay)
                .SetEase(Ease.OutBack)
                .SetUpdate(true);
        }

        private void AnimateToStateDOTween(bool hovered)
        {
            KillTweens();

            Vector3 targetScale = hovered ? Vector3.one * _hoverScale : Vector3.one;
            float targetAlpha = hovered ? 1f : 0f;

            // Scale the VISUAL CONTAINER (not the card itself)
            _scaleTween = _visualContainer.DOScale(targetScale, _animationDuration)
                .SetEase(Ease.OutQuad)
                .SetUpdate(true)
                .OnComplete(() =>
                {
                    _isAnimating = false;
                    if (!hovered)
                        SendToBack();
                });

            // Background color change using generic tweener (DOTween.Modules may not be available)
            if (_cardBackground != null)
            {
                Color targetColor = hovered ? _hoverBackgroundColor : _normalBackgroundColor;
                _colorTween = DOTween.To(
                        () => _cardBackground.color,
                        v => _cardBackground.color = v,
                        targetColor,
                        _animationDuration)
                    .SetEase(Ease.OutQuad)
                    .SetUpdate(true);
            }

            // Details panel alpha
            if (_detailsPanel != null)
            {
                _alphaTween = DOTween.To(
                        () => _detailsPanel.alpha,
                        v => _detailsPanel.alpha = v,
                        targetAlpha,
                        _animationDuration * 0.8f)
                    .SetEase(Ease.OutQuad)
                    .SetUpdate(true)
                    .OnComplete(() =>
                    {
                        if (_detailsPanel != null)
                        {
                            _detailsPanel.interactable = hovered;
                            _detailsPanel.blocksRaycasts = hovered;
                        }
                    });
            }
        }
#endif

        private void KillTweens()
        {
#if DOTWEEN || DOTWEEN_ENABLED
            _scaleTween?.Kill();
            _colorTween?.Kill();
            _alphaTween?.Kill();
            _scaleTween = null;
            _colorTween = null;
            _alphaTween = null;
#endif
        }

        /// <summary>
        /// Brings this card to front by enabling override sorting
        /// </summary>
        private void BringToFront()
        {
            if (_overrideCanvas == null)
                return;

            // Get current canvas sorting order
            var parentCanvas = GetComponentInParent<Canvas>();
            _originalSortingOrder = parentCanvas != null ? parentCanvas.sortingOrder : 0;
            
            // Enable override with higher sorting order
            _overrideCanvas.overrideSorting = true;
            _overrideCanvas.sortingOrder = _originalSortingOrder + 100;
        }

        /// <summary>
        /// Sends this card back to normal sorting
        /// </summary>
        private void SendToBack()
        {
            if (_overrideCanvas == null)
                return;

            _overrideCanvas.overrideSorting = false;
        }

        #endregion

        #region Editor Helpers

#if UNITY_EDITOR
        [ContextMenu("Preview Hover State")]
        private void PreviewHoverState()
        {
            if (_visualContainer != null)
                _visualContainer.localScale = Vector3.one * _hoverScale;
            if (_cardBackground != null)
                _cardBackground.color = _hoverBackgroundColor;
            if (_detailsPanel != null)
                _detailsPanel.alpha = 1f;
        }

        [ContextMenu("Preview Normal State")]
        private void PreviewNormalState()
        {
            if (_visualContainer != null)
                _visualContainer.localScale = Vector3.one;
            if (_cardBackground != null)
                _cardBackground.color = _normalBackgroundColor;
            if (_detailsPanel != null)
                _detailsPanel.alpha = 0f;
        }

        [ContextMenu("Setup Visual Container")]
        private void EditorSetupVisualContainer()
        {
            SetupVisualContainer();
            UnityEditor.EditorUtility.SetDirty(this);
        }

        private void OnValidate()
        {
            if (_rectTransform == null)
                _rectTransform = GetComponent<RectTransform>();
            
            // Ensure pivot is centered
            if (_rectTransform != null && _rectTransform.pivot != new Vector2(0.5f, 0.5f))
            {
                _rectTransform.pivot = new Vector2(0.5f, 0.5f);
            }
        }
#endif

        #endregion
    }
}
