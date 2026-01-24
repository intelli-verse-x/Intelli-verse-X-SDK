// ============================================================================
// IVXAppCard.cs - Netflix-style App Card UI Component
// ============================================================================
// IntelliVerseX SDK - Cross-Promotion Feature
// Individual app card with hover animations and interactions
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
    /// Individual app card component with Netflix-style hover effects
    /// </summary>
    [RequireComponent(typeof(RectTransform))]
    public class IVXAppCard : MonoBehaviour, 
        IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
    {
        #region Serialized Fields

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
        [SerializeField] private float _hoverScale = 1.15f;
        [SerializeField] private float _animationDuration = 0.25f;
        [SerializeField] private AnimationCurve _scaleCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

        [Header("Hover Effects")]
        [SerializeField] private float _hoverLift = 24f;
        [SerializeField] private float _hoverTilt = -1.5f;
        [SerializeField] private float _hoverPunchScale = 0.04f;
        [SerializeField] private float _detailsSlideDistance = 20f;

        [Header("Visual Settings")]
        [SerializeField] private Color _normalBackgroundColor = new Color(0.15f, 0.15f, 0.18f, 1f);
        [SerializeField] private Color _hoverBackgroundColor = new Color(0.2f, 0.2f, 0.25f, 1f);
        [SerializeField] private float _cornerRadius = 12f;

        [Header("Loading State")]
        [SerializeField] private GameObject _loadingSpinner;
        [SerializeField] private Sprite _placeholderIcon;

        #endregion

        #region Private Fields

        private IVXUnifiedAppInfo _appInfo;
        private RectTransform _rectTransform;
        private Vector3 _originalScale;
        private Quaternion _originalRotation;
        private Vector2 _originalAnchoredPosition;
        private RectTransform _detailsRectTransform;
        private Vector2 _detailsOriginalPosition;
        private Coroutine _animationCoroutine;
        private bool _isHovered;
        private bool _isInitialized;
        private int _cardIndex;
        private int _originalSiblingIndex;

    #if DOTWEEN || DOTWEEN_ENABLED
        private Sequence _hoverSequence;
        private Sequence _entranceSequence;
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
            _originalScale = transform.localScale;
            _originalRotation = _rectTransform.localRotation;
            _originalAnchoredPosition = _rectTransform.anchoredPosition;
            _detailsRectTransform = _detailsPanel != null ? _detailsPanel.GetComponent<RectTransform>() : null;
            if (_detailsRectTransform != null)
            {
                _detailsOriginalPosition = _detailsRectTransform.anchoredPosition;
            }

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
            if (_installButton != null)
                _installButton.onClick.RemoveListener(OnInstallButtonClicked);

            if (_animationCoroutine != null)
                StopCoroutine(_animationCoroutine);

            KillTweens();
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

            if (_appNameText != null)
                _appNameText.text = "";
            if (_descriptionText != null)
                _descriptionText.text = "";
            if (_appIcon != null)
                _appIcon.texture = null;

            SetLoadingState(true);
            transform.localScale = _originalScale;
            _rectTransform.anchoredPosition = _originalAnchoredPosition;
            _rectTransform.localRotation = _originalRotation;
            if (_detailsRectTransform != null)
            {
                _detailsRectTransform.anchoredPosition = _detailsOriginalPosition;
            }
            RestoreSiblingIndex();
        }

        /// <summary>
        /// Play the card entrance animation
        /// </summary>
        public void PlayEntranceAnimation(float delay = 0f)
        {
#if DOTWEEN || DOTWEEN_ENABLED
            PlayEntranceAnimationDOTween(delay);
            return;
#endif
            StartCoroutine(EntranceAnimationCoroutine(delay));
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

            var stars = _starsContainer.GetComponentsInChildren<Image>();
            for (int i = 0; i < stars.Length && i < 5; i++)
            {
                float starThreshold = i + 1;
                if (rating >= starThreshold)
                    stars[i].sprite = _starFilled;
                else if (rating >= starThreshold - 0.5f && _starHalf != null)
                    stars[i].sprite = _starHalf;
                else
                    stars[i].sprite = _starEmpty;
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
            if (!_isInitialized)
                return;

            _isHovered = true;
            OnCardHoverEnter?.Invoke(this);

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
            if (!_isInitialized)
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
#if DOTWEEN || DOTWEEN_ENABLED
            AnimateToStateDOTween(hovered);
            return;
#endif
            if (_animationCoroutine != null)
                StopCoroutine(_animationCoroutine);

            _animationCoroutine = StartCoroutine(AnimateCoroutine(hovered));
        }

        private IEnumerator AnimateCoroutine(bool hovered)
        {
            Vector3 startScale = transform.localScale;
            Vector3 targetScale = hovered ? _originalScale * _hoverScale : _originalScale;

            Vector2 startPosition = _rectTransform.anchoredPosition;
            Vector2 targetPosition = hovered
                ? _originalAnchoredPosition + new Vector2(0f, _hoverLift)
                : _originalAnchoredPosition;

            Quaternion startRotation = _rectTransform.localRotation;
            Quaternion targetRotation = Quaternion.Euler(0f, 0f, hovered ? _hoverTilt : 0f);
            
            Color startColor = _cardBackground != null ? _cardBackground.color : _normalBackgroundColor;
            Color targetColor = hovered ? _hoverBackgroundColor : _normalBackgroundColor;
            
            float startAlpha = _detailsPanel != null ? _detailsPanel.alpha : 0;
            float targetAlpha = hovered ? 1f : 0f;

            Vector2 detailsStartPosition = _detailsRectTransform != null ? _detailsRectTransform.anchoredPosition : Vector2.zero;
            Vector2 detailsTargetPosition = _detailsRectTransform != null
                ? _detailsOriginalPosition + (hovered ? new Vector2(0f, _detailsSlideDistance) : Vector2.zero)
                : Vector2.zero;

            float elapsed = 0;
            while (elapsed < _animationDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = _scaleCurve.Evaluate(elapsed / _animationDuration);

                // Scale
                transform.localScale = Vector3.Lerp(startScale, targetScale, t);

                // Position + tilt
                _rectTransform.anchoredPosition = Vector2.Lerp(startPosition, targetPosition, t);
                _rectTransform.localRotation = Quaternion.Lerp(startRotation, targetRotation, t);

                // Background color
                if (_cardBackground != null)
                    _cardBackground.color = Color.Lerp(startColor, targetColor, t);

                // Details panel
                if (_detailsPanel != null)
                    _detailsPanel.alpha = Mathf.Lerp(startAlpha, targetAlpha, t);

                if (_detailsRectTransform != null)
                    _detailsRectTransform.anchoredPosition = Vector2.Lerp(detailsStartPosition, detailsTargetPosition, t);

                yield return null;
            }

            // Ensure final values
            transform.localScale = targetScale;
            _rectTransform.anchoredPosition = targetPosition;
            _rectTransform.localRotation = targetRotation;
            if (_cardBackground != null)
                _cardBackground.color = targetColor;
            if (_detailsPanel != null)
            {
                _detailsPanel.alpha = targetAlpha;
                _detailsPanel.interactable = hovered;
                _detailsPanel.blocksRaycasts = hovered;
            }

            if (_detailsRectTransform != null)
                _detailsRectTransform.anchoredPosition = detailsTargetPosition;

            if (!hovered)
                RestoreSiblingIndex();

            _animationCoroutine = null;
        }

        private IEnumerator EntranceAnimationCoroutine(float delay)
        {
            // Initial state
            transform.localScale = Vector3.zero;
            
            if (delay > 0)
                yield return new WaitForSeconds(delay);

            float elapsed = 0;
            float duration = _animationDuration * 1.5f;

            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = _scaleCurve.Evaluate(elapsed / duration);
                
                // Overshoot effect
                float overshoot = 1f + Mathf.Sin(t * Mathf.PI) * 0.1f;
                transform.localScale = _originalScale * t * overshoot;

                yield return null;
            }

            transform.localScale = _originalScale;
        }

#if DOTWEEN || DOTWEEN_ENABLED
        private void PlayEntranceAnimationDOTween(float delay)
        {
            KillTweens();

            transform.localScale = Vector3.zero;

            _entranceSequence = DOTween.Sequence().SetUpdate(true);
            if (delay > 0f)
            {
                _entranceSequence.AppendInterval(delay);
            }

            _entranceSequence.Append(DOTween.To(
                    () => _rectTransform.localScale,
                    v => _rectTransform.localScale = v,
                    _originalScale,
                    _animationDuration * 1.5f)
                .SetEase(Ease.OutBack)
                .SetTarget(_rectTransform));

            if (_hoverPunchScale > 0f)
            {
                Vector3 punchScale = _originalScale * (1f + (_hoverPunchScale * 0.75f));
                _entranceSequence.Join(DOTween.To(
                        () => _rectTransform.localScale,
                        v => _rectTransform.localScale = v,
                        punchScale,
                        _animationDuration * 0.35f)
                    .SetEase(Ease.OutQuad)
                    .SetTarget(_rectTransform));
            }
        }
#endif

#if DOTWEEN || DOTWEEN_ENABLED
        private void AnimateToStateDOTween(bool hovered)
        {
            KillTweens();

            Vector3 targetScale = hovered ? _originalScale * _hoverScale : _originalScale;
            Vector2 targetPosition = hovered
                ? _originalAnchoredPosition + new Vector2(0f, _hoverLift)
                : _originalAnchoredPosition;
            float targetRotation = hovered ? _hoverTilt : 0f;
            float targetAlpha = hovered ? 1f : 0f;

            _hoverSequence = DOTween.Sequence().SetUpdate(true);

            if (_rectTransform != null)
            {
                _hoverSequence.Join(DOTween.To(
                        () => _rectTransform.localScale,
                        v => _rectTransform.localScale = v,
                        targetScale,
                        _animationDuration)
                    .SetEase(hovered ? Ease.OutBack : Ease.InOutQuad)
                    .SetTarget(_rectTransform));
                _hoverSequence.Join(DOTween.To(
                        () => _rectTransform.anchoredPosition,
                        v => _rectTransform.anchoredPosition = v,
                        targetPosition,
                        _animationDuration)
                    .SetEase(hovered ? Ease.OutQuad : Ease.InOutQuad)
                    .SetTarget(_rectTransform));
                _hoverSequence.Join(DOTween.To(
                        () => _rectTransform.localEulerAngles,
                        v => _rectTransform.localEulerAngles = v,
                        new Vector3(0f, 0f, targetRotation),
                        _animationDuration)
                    .SetEase(hovered ? Ease.OutQuad : Ease.InOutQuad)
                    .SetTarget(_rectTransform));
            }

            if (_cardBackground != null)
            {
                _hoverSequence.Join(DOTween.To(
                        () => _cardBackground.color,
                        v => _cardBackground.color = v,
                        hovered ? _hoverBackgroundColor : _normalBackgroundColor,
                        _animationDuration)
                    .SetEase(Ease.OutQuad)
                    .SetTarget(_cardBackground));
            }

            if (_detailsRectTransform != null)
            {
                Vector2 detailsTarget = hovered
                    ? _detailsOriginalPosition + new Vector2(0f, _detailsSlideDistance)
                    : _detailsOriginalPosition;
                _hoverSequence.Join(DOTween.To(
                        () => _detailsRectTransform.anchoredPosition,
                        v => _detailsRectTransform.anchoredPosition = v,
                        detailsTarget,
                        _animationDuration)
                    .SetEase(hovered ? Ease.OutQuad : Ease.InOutQuad)
                    .SetTarget(_detailsRectTransform));
            }

            if (_detailsPanel != null)
            {
                _hoverSequence.Join(DOTween.To(
                        () => _detailsPanel.alpha,
                        v => _detailsPanel.alpha = v,
                        targetAlpha,
                        _animationDuration * 0.9f)
                    .SetEase(Ease.OutQuad)
                    .SetTarget(_detailsPanel));
            }

            if (hovered && _hoverPunchScale > 0f && _rectTransform != null)
            {
                Vector3 punchScale = targetScale * (1f + _hoverPunchScale);
                _hoverSequence.Append(DOTween.To(
                        () => _rectTransform.localScale,
                        v => _rectTransform.localScale = v,
                        punchScale,
                        _animationDuration * 0.35f)
                    .SetEase(Ease.OutQuad)
                    .SetTarget(_rectTransform));
                _hoverSequence.Append(DOTween.To(
                        () => _rectTransform.localScale,
                        v => _rectTransform.localScale = v,
                        targetScale,
                        _animationDuration * 0.35f)
                    .SetEase(Ease.OutQuad)
                    .SetTarget(_rectTransform));
            }

            _hoverSequence.OnComplete(() =>
            {
                if (_detailsPanel != null)
                {
                    _detailsPanel.interactable = hovered;
                    _detailsPanel.blocksRaycasts = hovered;
                }

                if (!hovered)
                    RestoreSiblingIndex();
            });
        }
#endif

        private void KillTweens()
        {
#if DOTWEEN || DOTWEEN_ENABLED
            if (_hoverSequence != null && _hoverSequence.IsActive())
                _hoverSequence.Kill();
            if (_entranceSequence != null && _entranceSequence.IsActive())
                _entranceSequence.Kill();

            DOTween.Kill(_rectTransform);
            DOTween.Kill(_cardBackground);
            DOTween.Kill(_detailsPanel);
            DOTween.Kill(_detailsRectTransform);
#endif
        }

        private void BringToFront()
        {
            _originalSiblingIndex = transform.GetSiblingIndex();
            transform.SetAsLastSibling();
        }

        private void RestoreSiblingIndex()
        {
            if (_originalSiblingIndex < 0)
                return;

            transform.SetSiblingIndex(_originalSiblingIndex);
        }

        #endregion

        #region Editor Helpers

#if UNITY_EDITOR
        [ContextMenu("Preview Hover State")]
        private void PreviewHoverState()
        {
            if (_cardBackground != null)
                _cardBackground.color = _hoverBackgroundColor;
            transform.localScale = _originalScale * _hoverScale;
            if (_detailsPanel != null)
                _detailsPanel.alpha = 1f;
        }

        [ContextMenu("Preview Normal State")]
        private void PreviewNormalState()
        {
            if (_cardBackground != null)
                _cardBackground.color = _normalBackgroundColor;
            transform.localScale = _originalScale;
            if (_detailsPanel != null)
                _detailsPanel.alpha = 0f;
        }

        private void OnValidate()
        {
            if (_rectTransform == null)
                _rectTransform = GetComponent<RectTransform>();
            _originalScale = transform.localScale;
            _originalRotation = _rectTransform.localRotation;
            _originalAnchoredPosition = _rectTransform.anchoredPosition;
            _detailsRectTransform = _detailsPanel != null ? _detailsPanel.GetComponent<RectTransform>() : null;
            if (_detailsRectTransform != null)
            {
                _detailsOriginalPosition = _detailsRectTransform.anchoredPosition;
            }
        }
#endif

        #endregion
    }
}
