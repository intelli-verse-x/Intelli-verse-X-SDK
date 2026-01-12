// File: IVXUIComponents.cs
// Purpose: Common UI components (Loading Spinner, Confirm Dialog)
// Package: IntelliVerseX.UI

using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace IntelliVerseX.UI
{
    /// <summary>
    /// Animated loading spinner component.
    /// Shows a rotating icon and optional loading text.
    /// 
    /// Usage:
    ///   - Attach to spinner GameObject
    ///   - Call Show() to display, Hide() to hide
    ///   - Customize spinner icon and rotation speed
    /// </summary>
    public class IVXLoadingSpinner : MonoBehaviour
    {
        #region Inspector Fields

        [Header("UI References")]
        [SerializeField] private GameObject spinnerPanel;
        [SerializeField] private RectTransform spinnerIcon;
        [SerializeField] private TextMeshProUGUI loadingText;

        [Header("Animation Settings")]
        [SerializeField] private float rotationSpeed = 360f; // degrees per second
        [SerializeField] private bool rotateClockwise = false;

        [Header("Text Settings")]
        [SerializeField] private string defaultLoadingText = "Loading...";
        [SerializeField] private bool animateText = true;
        [SerializeField] private float textAnimationInterval = 0.5f;

        #endregion

        #region Private Fields

        private bool _isSpinning;
        private float _textAnimationTimer;
        private int _dotCount;

        #endregion

        #region Unity Lifecycle

        private void Awake()
        {
            // Hide initially
            if (spinnerPanel != null)
                spinnerPanel.SetActive(false);
        }

        private void Update()
        {
            if (!_isSpinning)
                return;

            // Rotate spinner icon
            if (spinnerIcon != null)
            {
                float rotation = rotationSpeed * Time.deltaTime * (rotateClockwise ? -1f : 1f);
                spinnerIcon.Rotate(0f, 0f, rotation);
            }

            // Animate loading text
            if (animateText && loadingText != null)
            {
                _textAnimationTimer += Time.deltaTime;
                if (_textAnimationTimer >= textAnimationInterval)
                {
                    _textAnimationTimer = 0f;
                    _dotCount = (_dotCount + 1) % 4;
                    loadingText.text = defaultLoadingText + new string('.', _dotCount);
                }
            }
        }

        #endregion

        #region Public API

        /// <summary>
        /// Show loading spinner
        /// </summary>
        public void Show(string text = null)
        {
            if (spinnerPanel != null)
                spinnerPanel.SetActive(true);

            if (loadingText != null && !string.IsNullOrEmpty(text))
            {
                defaultLoadingText = text;
                loadingText.text = text;
            }

            _isSpinning = true;
            _textAnimationTimer = 0f;
            _dotCount = 0;

            Debug.Log("[IVXLoadingSpinner] Shown");
        }

        /// <summary>
        /// Hide loading spinner
        /// </summary>
        public void Hide()
        {
            if (spinnerPanel != null)
                spinnerPanel.SetActive(false);

            _isSpinning = false;

            Debug.Log("[IVXLoadingSpinner] Hidden");
        }

        /// <summary>
        /// Check if spinner is currently visible
        /// </summary>
        public bool IsVisible()
        {
            return spinnerPanel != null && spinnerPanel.activeSelf;
        }

        #endregion
    }

    /// <summary>
    /// Reusable confirmation dialog component.
    /// Shows a modal with title, message, and confirm/cancel buttons.
    /// 
    /// Usage:
    ///   - Attach to dialog GameObject
    ///   - Call Show() with callback
    ///   - User clicks confirm or cancel
    /// </summary>
    public class IVXConfirmDialog : MonoBehaviour
    {
        #region Inspector Fields

        [Header("UI References")]
        [SerializeField] private GameObject dialogPanel;
        [SerializeField] private TextMeshProUGUI titleText;
        [SerializeField] private TextMeshProUGUI messageText;
        [SerializeField] private Button confirmButton;
        [SerializeField] private Button cancelButton;
        [SerializeField] private TextMeshProUGUI confirmButtonText;
        [SerializeField] private TextMeshProUGUI cancelButtonText;

        [Header("Default Settings")]
        [SerializeField] private string defaultTitle = "Confirm";
        [SerializeField] private string defaultConfirmText = "Yes";
        [SerializeField] private string defaultCancelText = "No";

        #endregion

        #region Private Fields

        private Action<bool> _callback;

        #endregion

        #region Unity Lifecycle

        private void Awake()
        {
            // Set up button listeners
            if (confirmButton != null)
            {
                confirmButton.onClick.RemoveAllListeners();
                confirmButton.onClick.AddListener(() => OnConfirm(true));
            }

            if (cancelButton != null)
            {
                cancelButton.onClick.RemoveAllListeners();
                cancelButton.onClick.AddListener(() => OnConfirm(false));
            }

            // Hide initially
            if (dialogPanel != null)
                dialogPanel.SetActive(false);
        }

        #endregion

        #region Public API

        /// <summary>
        /// Show confirmation dialog
        /// </summary>
        public void Show(string message, Action<bool> callback, string title = null, string confirmText = null, string cancelText = null)
        {
            _callback = callback;

            // Set title
            if (titleText != null)
            {
                titleText.text = title ?? defaultTitle;
            }

            // Set message
            if (messageText != null)
            {
                messageText.text = message;
            }

            // Set button texts
            if (confirmButtonText != null)
            {
                confirmButtonText.text = confirmText ?? defaultConfirmText;
            }

            if (cancelButtonText != null)
            {
                cancelButtonText.text = cancelText ?? defaultCancelText;
            }

            // Show dialog
            if (dialogPanel != null)
                dialogPanel.SetActive(true);

            Debug.Log($"[IVXConfirmDialog] Shown: {message}");
        }

        /// <summary>
        /// Hide dialog without callback
        /// </summary>
        public void Hide()
        {
            if (dialogPanel != null)
                dialogPanel.SetActive(false);

            _callback = null;
        }

        /// <summary>
        /// Check if dialog is currently visible
        /// </summary>
        public bool IsVisible()
        {
            return dialogPanel != null && dialogPanel.activeSelf;
        }

        #endregion

        #region Private Methods

        private void OnConfirm(bool confirmed)
        {
            // Hide dialog
            if (dialogPanel != null)
                dialogPanel.SetActive(false);

            // Invoke callback
            _callback?.Invoke(confirmed);

            Debug.Log($"[IVXConfirmDialog] User response: {confirmed}");

            // Clear callback
            _callback = null;
        }

        #endregion
    }
}
