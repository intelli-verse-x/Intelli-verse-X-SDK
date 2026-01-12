// File: IVXRTLLayoutComponent.cs
// Purpose: Right-to-left (RTL) layout support for Arabic and Hebrew
// Package: IntelliVerseX.Localization
// Dependencies: TextMeshPro

using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace IntelliVerseX.Localization
{
    /// <summary>
    /// Automatic RTL (right-to-left) layout support for Arabic and Hebrew.
    /// Attach to UI elements that need RTL support.
    /// 
    /// Features:
    /// - Automatic text direction switching based on current locale
    /// - Layout reversing for RTL languages
    /// - Alignment adjustment for text components
    /// 
    /// Usage:
    ///   - Attach to TextMeshPro GameObject
    ///   - Configure options in Inspector
    ///   - Component auto-applies RTL when language changes
    /// </summary>
    [AddComponentMenu("IntelliVerse/Localization/RTL Layout Component")]
    public class IVXRTLLayoutComponent : MonoBehaviour
    {
        #region Inspector Fields

        [Header("RTL Configuration")]
        [Tooltip("Automatically apply RTL when the locale changes")]
        [SerializeField] private bool autoApplyRTL = true;

        [Tooltip("Reverse the layout (horizontally flip) for RTL languages")]
        [SerializeField] private bool reverseLayout = true;

        [Tooltip("Change text alignment for RTL languages")]
        [SerializeField] private bool adjustTextAlignment = true;

        [Header("Component References (Auto-detected if null)")]
        [Tooltip("TextMeshPro component to apply RTL to")]
        [SerializeField] private TMP_Text tmpText;

        [Tooltip("Legacy Text component (if not using TextMeshPro)")]
        [SerializeField] private Text legacyText;

        [Tooltip("RectTransform to reverse for RTL layout")]
        [SerializeField] private RectTransform layoutTransform;

        #endregion

        #region Private Fields

        private Vector3 _originalScale;
        private TextAlignmentOptions _originalTMPAlignment;
        private TextAnchor _originalLegacyAlignment;
        private bool _isRTL = false;

        #endregion

        #region Unity Lifecycle

        private void Awake()
        {
            // Auto-find components if not assigned
            if (tmpText == null)
                tmpText = GetComponent<TMP_Text>();

            if (legacyText == null && tmpText == null)
                legacyText = GetComponent<Text>();

            if (layoutTransform == null)
                layoutTransform = GetComponent<RectTransform>();

            // Store original values
            if (layoutTransform != null)
                _originalScale = layoutTransform.localScale;

            if (tmpText != null)
                _originalTMPAlignment = tmpText.alignment;

            if (legacyText != null)
                _originalLegacyAlignment = legacyText.alignment;
        }

        private void Start()
        {
            // Apply RTL on start if needed
            if (autoApplyRTL)
            {
                ApplyRTLIfNeeded();
            }
        }

        private void OnEnable()
        {
            // Subscribe to locale change events
            IVXLocalizationService.OnLanguageChanged += OnLanguageChanged;

            // Apply RTL when enabled
            if (autoApplyRTL)
            {
                ApplyRTLIfNeeded();
            }
        }

        private void OnDisable()
        {
            // Unsubscribe from locale change events
            IVXLocalizationService.OnLanguageChanged -= OnLanguageChanged;
        }

        #endregion

        #region Event Handlers

        private void OnLanguageChanged(string newLanguage)
        {
            if (autoApplyRTL)
            {
                ApplyRTLIfNeeded();
            }
        }

        #endregion

        #region RTL Application

        /// <summary>
        /// Apply RTL layout if current language is RTL
        /// </summary>
        public void ApplyRTLIfNeeded()
        {
            // Check if current language is RTL
            bool shouldBeRTL = IVXLocalizationService.Instance != null && IVXLocalizationService.Instance.IsRTL();

            // Only apply if state changed
            if (_isRTL == shouldBeRTL)
                return;

            _isRTL = shouldBeRTL;

            if (_isRTL)
            {
                ApplyRTL();
            }
            else
            {
                RevertRTL();
            }
        }

        /// <summary>
        /// Force apply RTL layout
        /// </summary>
        public void ApplyRTL()
        {
            _isRTL = true;

            // Reverse layout (flip horizontally)
            if (reverseLayout && layoutTransform != null)
            {
                Vector3 scale = _originalScale;
                scale.x = -Mathf.Abs(scale.x); // Ensure negative
                layoutTransform.localScale = scale;
            }

            // Adjust text alignment
            if (adjustTextAlignment)
            {
                if (tmpText != null)
                {
                    tmpText.alignment = ConvertToRTLAlignment(_originalTMPAlignment);
                }

                if (legacyText != null)
                {
                    legacyText.alignment = ConvertToRTLAlignment(_originalLegacyAlignment);
                }
            }

            Debug.Log($"[IVXRTLLayoutComponent] RTL applied to: {gameObject.name}");
        }

        /// <summary>
        /// Revert to LTR layout
        /// </summary>
        public void RevertRTL()
        {
            _isRTL = false;

            // Restore original scale
            if (reverseLayout && layoutTransform != null)
            {
                layoutTransform.localScale = _originalScale;
            }

            // Restore original alignment
            if (adjustTextAlignment)
            {
                if (tmpText != null)
                {
                    tmpText.alignment = _originalTMPAlignment;
                }

                if (legacyText != null)
                {
                    legacyText.alignment = _originalLegacyAlignment;
                }
            }

            Debug.Log($"[IVXRTLLayoutComponent] RTL reverted on: {gameObject.name}");
        }

        #endregion

        #region Alignment Conversion

        /// <summary>
        /// Convert TMP alignment to RTL equivalent
        /// </summary>
        private TextAlignmentOptions ConvertToRTLAlignment(TextAlignmentOptions alignment)
        {
            switch (alignment)
            {
                case TextAlignmentOptions.Left:
                case TextAlignmentOptions.TopLeft:
                case TextAlignmentOptions.BottomLeft:
                case TextAlignmentOptions.MidlineLeft:
                case TextAlignmentOptions.CaplineLeft:
                case TextAlignmentOptions.BaselineLeft:
                    // Left -> Right
                    return alignment + 2; // Offset to right alignment

                case TextAlignmentOptions.Right:
                case TextAlignmentOptions.TopRight:
                case TextAlignmentOptions.BottomRight:
                case TextAlignmentOptions.MidlineRight:
                case TextAlignmentOptions.CaplineRight:
                case TextAlignmentOptions.BaselineRight:
                    // Right -> Left
                    return alignment - 2; // Offset to left alignment

                default:
                    // Center, Justified, etc. remain unchanged
                    return alignment;
            }
        }

        /// <summary>
        /// Convert legacy Text alignment to RTL equivalent
        /// </summary>
        private TextAnchor ConvertToRTLAlignment(TextAnchor alignment)
        {
            switch (alignment)
            {
                case TextAnchor.UpperLeft: return TextAnchor.UpperRight;
                case TextAnchor.MiddleLeft: return TextAnchor.MiddleRight;
                case TextAnchor.LowerLeft: return TextAnchor.LowerRight;
                case TextAnchor.UpperRight: return TextAnchor.UpperLeft;
                case TextAnchor.MiddleRight: return TextAnchor.MiddleLeft;
                case TextAnchor.LowerRight: return TextAnchor.LowerLeft;
                default: return alignment; // Center alignments remain unchanged
            }
        }

        #endregion

        #region Public API

        /// <summary>
        /// Check if currently in RTL mode
        /// </summary>
        public bool IsRTL => _isRTL;

        /// <summary>
        /// Manually toggle RTL mode
        /// </summary>
        public void ToggleRTL()
        {
            if (_isRTL)
                RevertRTL();
            else
                ApplyRTL();
        }

        #endregion
    }
}
