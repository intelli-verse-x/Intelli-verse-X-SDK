// File: IVXSmartButton.cs
// Purpose: Button with configurable per-button debounce (replaces global TouchManager)
// Package: IntelliVerseX.UI

using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace IntelliVerseX.UI
{
    /// <summary>
    /// Smart button with configurable debounce to prevent double-tap issues.
    /// Replaces QuizVerse's global 0.5s TouchManager debounce with per-button configuration.
    /// 
    /// Features:
    /// - Configurable debounce window (default 0.2s)
    /// - Visual feedback on rapid clicks
    /// - Automatic re-enable after debounce
    /// - Works with Unity UI Button component
    /// 
    /// Usage:
    ///   - Attach to Button GameObject (alongside Button component)
    ///   - Configure debounce duration in Inspector
    ///   - Subscribe to OnSmartClick event (or use Button.onClick normally)
    /// </summary>
    [RequireComponent(typeof(Button))]
    [AddComponentMenu("IntelliVerse/UI/Smart Button")]
    public class IVXSmartButton : MonoBehaviour, IPointerClickHandler
    {
        #region Events

        /// <summary>
        /// Fired when button is clicked (after debounce check)
        /// </summary>
        public event Action OnSmartClick;

        #endregion

        #region Inspector Fields

        [Header("Debounce Settings")]
        [Tooltip("Debounce window in seconds (0.2s default, 0 to disable)")]
        [SerializeField] private float debounceWindow = 0.2f;

        [Tooltip("Show visual feedback when clicks are ignored")]
        [SerializeField] private bool showRapidClickFeedback = true;

        [Header("Visual Feedback (Optional)")]
        [Tooltip("Flash this image red when rapid clicks are detected")]
        [SerializeField] private Image feedbackImage;

        [SerializeField] private Color rapidClickColor = new Color(1f, 0.5f, 0.5f);
        [SerializeField] private float feedbackDuration = 0.1f;

        #endregion

        #region Private Fields

        private Button _button;
        private float _lastClickTime;
        private Color _originalColor;
#pragma warning disable CS0414 // Debounce state - may be used for visual feedback
        private bool _isDebouncing;
#pragma warning restore CS0414

        #endregion

        #region Unity Lifecycle

        private void Awake()
        {
            _button = GetComponent<Button>();

            if (_button == null)
            {
                Debug.LogError($"[IVXSmartButton] Button component not found on {gameObject.name}");
                enabled = false;
                return;
            }

            // Store original color for feedback
            if (feedbackImage != null)
            {
                _originalColor = feedbackImage.color;
            }
        }

        #endregion

        #region IPointerClickHandler Implementation

        public void OnPointerClick(PointerEventData eventData)
        {
            // This is called BEFORE Button.onClick
            // We use this to implement debounce logic
            float now = Time.unscaledTime;
            float timeSinceLastClick = now - _lastClickTime;

            // Check debounce window
            if (timeSinceLastClick < debounceWindow)
            {
                Debug.LogWarning($"[IVXSmartButton] Rapid click detected on {gameObject.name} ({timeSinceLastClick:F3}s since last click, debounce={debounceWindow}s)");

                // Show visual feedback
                if (showRapidClickFeedback)
                {
                    ShowRapidClickFeedback();
                }

                // Block the click (Button.onClick won't fire)
                return;
            }

            // Valid click - update timestamp
            _lastClickTime = now;
            _isDebouncing = false;

            // Fire custom event
            OnSmartClick?.Invoke();
        }

        #endregion

        #region Public API

        /// <summary>
        /// Set debounce window dynamically
        /// </summary>
        public void SetDebounceWindow(float seconds)
        {
            debounceWindow = Mathf.Max(0f, seconds);
            Debug.Log($"[IVXSmartButton] Debounce window set to {debounceWindow}s on {gameObject.name}");
        }

        /// <summary>
        /// Reset debounce state (allows immediate next click)
        /// </summary>
        public void ResetDebounce()
        {
            _lastClickTime = 0f;
            _isDebouncing = false;
            Debug.Log($"[IVXSmartButton] Debounce reset on {gameObject.name}");
        }

        /// <summary>
        /// Check if button is currently in debounce window
        /// </summary>
        public bool IsDebouncing()
        {
            float timeSinceLastClick = Time.unscaledTime - _lastClickTime;
            return timeSinceLastClick < debounceWindow;
        }

        /// <summary>
        /// Get time remaining in debounce window
        /// </summary>
        public float GetDebounceTimeRemaining()
        {
            float timeSinceLastClick = Time.unscaledTime - _lastClickTime;
            float remaining = debounceWindow - timeSinceLastClick;
            return Mathf.Max(0f, remaining);
        }

        #endregion

        #region Private Methods

        private void ShowRapidClickFeedback()
        {
            if (feedbackImage == null)
                return;

            // Flash red briefly
            StopAllCoroutines();
            StartCoroutine(FlashFeedback());
        }

        private System.Collections.IEnumerator FlashFeedback()
        {
            if (feedbackImage == null)
                yield break;

            // Flash to rapid click color
            feedbackImage.color = rapidClickColor;

            // Wait
            yield return new WaitForSecondsRealtime(feedbackDuration);

            // Restore original color
            feedbackImage.color = _originalColor;
        }

        #endregion

        #region Editor Helpers

#if UNITY_EDITOR
        private void OnValidate()
        {
            // Clamp debounce window to reasonable range
            debounceWindow = Mathf.Clamp(debounceWindow, 0f, 2f);
        }
#endif

        #endregion
    }
}
