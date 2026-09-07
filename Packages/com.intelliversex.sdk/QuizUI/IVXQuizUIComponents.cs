// File: IVXHintButton.cs
// Purpose: Reusable hint reveal button component
// Package: IntelliVerseX.QuizUI

using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace IntelliVerseX.QuizUI
{
    /// <summary>
    /// Reusable hint button component for quiz questions.
    /// Shows/hides hint text when clicked.
    /// 
    /// Usage:
    ///   - Attach to Button GameObject
    ///   - Set hint panel and text references
    ///   - Call SetHint() to configure hint text
    /// </summary>
    [RequireComponent(typeof(Button))]
    public class IVXHintButton : MonoBehaviour
    {
        #region Inspector Fields

        [Header("UI References")]
        [SerializeField] private GameObject hintPanel;
        [SerializeField] private TextMeshProUGUI hintText;
        [SerializeField] private Button closeButton;

        [Header("Settings")]
        [SerializeField] private bool hideButtonAfterUse = false;
        [SerializeField] private bool playClickSound = true;
        [SerializeField] private AudioClip clickSound;

        #endregion

        #region Private Fields

        private Button _button;
        private string _currentHint;
        private bool _hintUsed;
        private AudioSource _audioSource;

        #endregion

        #region Unity Lifecycle

        private void Awake()
        {
            _button = GetComponent<Button>();

            // Initialize audio source if click sound is enabled
            if (playClickSound && clickSound != null)
            {
                _audioSource = gameObject.AddComponent<AudioSource>();
                _audioSource.playOnAwake = false;
                _audioSource.clip = clickSound;
            }

            if (_button != null)
            {
                _button.onClick.RemoveAllListeners();
                _button.onClick.AddListener(OnHintButtonClicked);
            }

            if (closeButton != null)
            {
                closeButton.onClick.RemoveAllListeners();
                closeButton.onClick.AddListener(HideHint);
            }

            // Hide hint panel initially
            if (hintPanel != null)
                hintPanel.SetActive(false);
        }

        #endregion

        #region Public API

        /// <summary>
        /// Set hint text for this question
        /// </summary>
        public void SetHint(string hint)
        {
            _currentHint = hint;
            _hintUsed = false;

            // Show/hide button based on hint availability
            if (_button != null)
            {
                _button.gameObject.SetActive(!string.IsNullOrEmpty(hint));
            }

            // Hide hint panel
            if (hintPanel != null)
                hintPanel.SetActive(false);
        }

        /// <summary>
        /// Show hint immediately (without button click)
        /// </summary>
        public void ShowHint()
        {
            if (string.IsNullOrEmpty(_currentHint))
            {
                Debug.LogWarning("[IVXHintButton] No hint text set");
                return;
            }

            if (hintText != null)
            {
                hintText.text = _currentHint;
            }

            if (hintPanel != null)
            {
                hintPanel.SetActive(true);
            }

            _hintUsed = true;

            // Hide button if configured
            if (hideButtonAfterUse && _button != null)
            {
                _button.gameObject.SetActive(false);
            }

            Debug.Log("[IVXHintButton] Hint shown");
        }

        /// <summary>
        /// Hide hint panel
        /// </summary>
        public void HideHint()
        {
            if (hintPanel != null)
            {
                hintPanel.SetActive(false);
            }
        }

        /// <summary>
        /// Check if hint was used
        /// </summary>
        public bool WasHintUsed() => _hintUsed;

        #endregion

        #region Private Methods

        private void OnHintButtonClicked()
        {
            // Play click sound if enabled
            if (_audioSource != null && _audioSource.clip != null)
            {
                _audioSource.Play();
            }

            ShowHint();
        }

        #endregion
    }

    /// <summary>
    /// Explanation modal for showing answer explanation after submission.
    /// </summary>
    public class IVXExplanationModal : MonoBehaviour
    {
        #region Inspector Fields

        [Header("UI References")]
        [SerializeField] private GameObject modalPanel;
        [SerializeField] private TextMeshProUGUI titleText;
        [SerializeField] private TextMeshProUGUI explanationText;
        [SerializeField] private Button closeButton;

        [Header("Feedback")]
        [SerializeField] private GameObject correctFeedback;
        [SerializeField] private GameObject incorrectFeedback;

        #endregion

        #region Unity Lifecycle

        private void Awake()
        {
            if (closeButton != null)
            {
                closeButton.onClick.RemoveAllListeners();
                closeButton.onClick.AddListener(Hide);
            }

            // Hide initially
            if (modalPanel != null)
                modalPanel.SetActive(false);
        }

        #endregion

        #region Public API

        /// <summary>
        /// Show explanation modal
        /// </summary>
        public void Show(string explanation, bool isCorrect)
        {
            if (modalPanel == null)
            {
                Debug.LogError("[IVXExplanationModal] Modal panel is null");
                return;
            }

            // Set explanation text
            if (explanationText != null)
            {
                explanationText.text = explanation;
            }

            // Set title
            if (titleText != null)
            {
                titleText.text = isCorrect ? "Correct! ✓" : "Incorrect ✗";
            }

            // Show correct/incorrect feedback
            if (correctFeedback != null)
                correctFeedback.SetActive(isCorrect);

            if (incorrectFeedback != null)
                incorrectFeedback.SetActive(!isCorrect);

            // Show modal
            modalPanel.SetActive(true);

            Debug.Log($"[IVXExplanationModal] Shown: isCorrect={isCorrect}");
        }

        /// <summary>
        /// Hide explanation modal
        /// </summary>
        public void Hide()
        {
            if (modalPanel != null)
            {
                modalPanel.SetActive(false);
            }
        }

        /// <summary>
        /// Check if modal is currently visible
        /// </summary>
        public bool IsVisible()
        {
            return modalPanel != null && modalPanel.activeSelf;
        }

        #endregion
    }
}
