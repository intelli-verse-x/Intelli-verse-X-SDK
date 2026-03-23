// File: IVXQuizResultPanel.cs
// Purpose: Reusable quiz result/summary display panel
// Package: IntelliVerseX.QuizUI
// Dependencies: IntelliVerseX.Core, IntelliVerseX.Quiz, TextMeshPro

using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using IntelliVerseX.Quiz;

namespace IntelliVerseX.QuizUI
{
    /// <summary>
    /// Reusable quiz result panel UI component.
    /// Displays final score, percentage, and feedback message.
    /// 
    /// Usage:
    ///   - Attach to result panel GameObject
    ///   - Configure UI references in Inspector
    ///   - Subscribe to button events
    ///   - Call DisplayResults() to show summary
    /// </summary>
    public class IVXQuizResultPanel : MonoBehaviour
    {
        #region Events

        /// <summary>
        /// Fired when Review button is clicked
        /// </summary>
        public event Action OnReviewRequested;

        /// <summary>
        /// Fired when Try Again button is clicked
        /// </summary>
        public event Action OnTryAgainRequested;

        /// <summary>
        /// Fired when Share button is clicked
        /// </summary>
        public event Action OnShareRequested;

        /// <summary>
        /// Fired when Back button is clicked
        /// </summary>
        public event Action OnBackRequested;

        #endregion

        #region Inspector Fields

        [Header("Result Display")]
        [SerializeField] private TextMeshProUGUI titleText;
        [SerializeField] private TextMeshProUGUI scoreText;
        [SerializeField] private TextMeshProUGUI percentageText;
        [SerializeField] private TextMeshProUGUI messageText;
        [SerializeField] private Image scoreIcon;

        [Header("Score Icons")]
        [SerializeField] private Sprite perfectScoreIcon;
        [SerializeField] private Sprite goodScoreIcon;
        [SerializeField] private Sprite averageScoreIcon;
        [SerializeField] private Sprite poorScoreIcon;

        [Header("Buttons")]
        [SerializeField] private Button reviewButton;
        [SerializeField] private Button tryAgainButton;
        [SerializeField] private Button shareButton;
        [SerializeField] private Button backButton;

        [Header("Localization Keys (Optional)")]
        [SerializeField] private string perfectMessageKey = "Result_Perfect";
        [SerializeField] private string goodMessageKey = "Result_Good";
        [SerializeField] private string averageMessageKey = "Result_Average";
        [SerializeField] private string poorMessageKey = "Result_Poor";

        #endregion

        #region Private Fields

        private IVXQuizSession _currentSession;

        #endregion

        #region Unity Lifecycle

        private void Start()
        {
            // Set up button listeners
            if (reviewButton != null)
            {
                reviewButton.onClick.RemoveAllListeners();
                reviewButton.onClick.AddListener(() => OnReviewRequested?.Invoke());
            }

            if (tryAgainButton != null)
            {
                tryAgainButton.onClick.RemoveAllListeners();
                tryAgainButton.onClick.AddListener(() => OnTryAgainRequested?.Invoke());
            }

            if (shareButton != null)
            {
                shareButton.onClick.RemoveAllListeners();
                shareButton.onClick.AddListener(() => OnShareRequested?.Invoke());
            }

            if (backButton != null)
            {
                backButton.onClick.RemoveAllListeners();
                backButton.onClick.AddListener(() => OnBackRequested?.Invoke());
            }
        }

        #endregion

        #region Public API

        /// <summary>
        /// Display quiz results from session
        /// </summary>
        public void DisplayResults(IVXQuizSession session)
        {
            if (session == null)
            {
                Debug.LogError("[IVXQuizResultPanel] Session is null");
                return;
            }

            _currentSession = session;

            // Update score text
            if (scoreText != null)
            {
                scoreText.text = $"{session.GetScore()} / {session.QuizData.questions.Count}";
            }

            // Update percentage
            int percentage = (int)session.GetScorePercentage();
            if (percentageText != null)
            {
                percentageText.text = $"{percentage}%";
            }

            // Update message based on score
            if (messageText != null)
            {
                messageText.text = GetScoreMessage(percentage);
            }

            // Update icon based on score
            if (scoreIcon != null)
            {
                scoreIcon.sprite = GetScoreIcon(percentage);
            }

            // Update title
            if (titleText != null)
            {
                titleText.text = percentage >= 60 ? "Congratulations!" : "Quiz Complete";
            }

            Debug.Log($"[IVXQuizResultPanel] Displayed results: {session.GetScore()}/{session.QuizData.questions.Count} ({percentage}%)");
        }

        /// <summary>
        /// Display results with custom score values
        /// </summary>
        public void DisplayResults(int correctAnswers, int totalQuestions)
        {
            if (scoreText != null)
            {
                scoreText.text = $"{correctAnswers} / {totalQuestions}";
            }

            int percentage = totalQuestions > 0 ? (correctAnswers * 100) / totalQuestions : 0;

            if (percentageText != null)
            {
                percentageText.text = $"{percentage}%";
            }

            if (messageText != null)
            {
                messageText.text = GetScoreMessage(percentage);
            }

            if (scoreIcon != null)
            {
                scoreIcon.sprite = GetScoreIcon(percentage);
            }

            if (titleText != null)
            {
                titleText.text = percentage >= 60 ? "Congratulations!" : "Quiz Complete";
            }
        }

        #endregion

        #region Private Methods

        private string GetScoreMessage(int percentage)
        {
            // Try to get localized message first (if localization service is available)
            string localizedMessage = TryGetLocalizedMessage(percentage);
            if (!string.IsNullOrEmpty(localizedMessage))
                return localizedMessage;

            // Fallback to default messages
            if (percentage == 100)
            {
                return "Perfect Score! Amazing job! 🎉";
            }
            else if (percentage >= 80)
            {
                return "Great work! You're a quiz master! 🌟";
            }
            else if (percentage >= 60)
            {
                return "Good job! Keep it up! 👍";
            }
            else if (percentage >= 40)
            {
                return "Not bad! Try again for a better score! 💪";
            }
            else
            {
                return "Keep learning! Practice makes perfect! 📚";
            }
        }

        private string TryGetLocalizedMessage(int percentage)
        {
            // Check if localization service is available
            if (IntelliVerseX.Localization.IVXLocalizationService.Instance == null)
                return null;

            string key = "";
            if (percentage == 100)
                key = perfectMessageKey;
            else if (percentage >= 80)
                key = goodMessageKey;
            else if (percentage >= 60)
                key = averageMessageKey;
            else
                key = poorMessageKey;

            if (string.IsNullOrEmpty(key))
                return null;

            return IntelliVerseX.Localization.IVXLocalizationService.Instance.GetString(key, null);
        }

        private Sprite GetScoreIcon(int percentage)
        {
            if (percentage == 100 && perfectScoreIcon != null)
            {
                return perfectScoreIcon;
            }
            else if (percentage >= 80 && goodScoreIcon != null)
            {
                return goodScoreIcon;
            }
            else if (percentage >= 60 && averageScoreIcon != null)
            {
                return averageScoreIcon;
            }
            else if (poorScoreIcon != null)
            {
                return poorScoreIcon;
            }

            return null;
        }

        #endregion
    }
}
