// File: IVXQuizQuestionPanel.cs
// Purpose: Reusable quiz question display panel component
// Package: IntelliVerseX.QuizUI
// Dependencies: IntelliVerseX.Core, IntelliVerseX.Quiz, TextMeshPro

using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using IntelliVerseX.Core;
using IntelliVerseX.Quiz;

namespace IntelliVerseX.QuizUI
{
    /// <summary>
    /// Reusable quiz question panel UI component.
    /// Displays question text, options, hints, and handles answer selection.
    /// 
    /// Usage:
    ///   - Attach to panel GameObject
    ///   - Configure UI references in Inspector
    ///   - Subscribe to OnAnswerSelected event
    ///   - Call DisplayQuestion() to show question
    /// </summary>
    public class IVXQuizQuestionPanel : MonoBehaviour
    {
        #region Events

        /// <summary>
        /// Fired when user selects an answer (answerIndex)
        /// </summary>
        public event Action<int> OnAnswerSelected;

        /// <summary>
        /// Fired when hint button is clicked
        /// </summary>
        public event Action OnHintRequested;

        #endregion

        #region Inspector Fields

        [Header("Question Display")]
        [SerializeField] private TextMeshProUGUI questionNumberText;
        [SerializeField] private TextMeshProUGUI questionText;
        [SerializeField] private TextMeshProUGUI topicText;
        [SerializeField] private TextMeshProUGUI difficultyText;

        [Header("Option Buttons (4 options)")]
        [SerializeField] private List<Button> optionButtons;
        [SerializeField] private List<TextMeshProUGUI> optionTexts;
        [SerializeField] private List<Image> optionBackgrounds;

        [Header("Optional Features")]
        [SerializeField] private Button hintButton;
        [SerializeField] private GameObject hintPanel;
        [SerializeField] private TextMeshProUGUI hintText;

        [Header("Feedback Colors")]
        [SerializeField] private Color correctColor = Color.green;
        [SerializeField] private Color incorrectColor = Color.red;
        [SerializeField] private Color defaultColor = Color.white;

        #endregion

        #region Private Fields

        private IVXQuizQuestion _currentQuestion;
        private int _currentQuestionIndex;
        private bool _hasAnswered;
        private int[] _shuffledIndices; // Maps displayed option index to original index

        #endregion

        #region Unity Lifecycle

        private void Start()
        {
            // Set up hint button
            if (hintButton != null)
            {
                hintButton.onClick.RemoveAllListeners();
                hintButton.onClick.AddListener(OnHintButtonClicked);
            }

            // Set up option buttons
            for (int i = 0; i < optionButtons.Count; i++)
            {
                int optionIndex = i; // Capture for closure
                if (optionButtons[i] != null)
                {
                    optionButtons[i].onClick.RemoveAllListeners();
                    optionButtons[i].onClick.AddListener(() => OnOptionClicked(optionIndex));
                }
            }

            // Hide hint panel initially
            if (hintPanel != null)
                hintPanel.SetActive(false);
        }

        #endregion

        #region Public API

        /// <summary>
        /// Display a quiz question
        /// </summary>
        public void DisplayQuestion(IVXQuizQuestion question, int questionIndex, int totalQuestions)
        {
            _currentQuestion = question;
            _currentQuestionIndex = questionIndex;
            _hasAnswered = false;

            // Reset UI
            ResetUI();

            // Update question number
            if (questionNumberText != null)
            {
                questionNumberText.text = $"Question {questionIndex + 1} of {totalQuestions}";
            }

            // Update question text
            if (questionText != null)
            {
                questionText.text = question.QuestionText;
            }

            // Update topic (if available)
            if (topicText != null)
            {
                topicText.text = question.Category ?? "";
                topicText.gameObject.SetActive(!string.IsNullOrEmpty(question.Category));
            }

            // Update difficulty (if available)
            if (difficultyText != null)
            {
                string difficultyLabel = question.Difficulty > 0 ? $"Difficulty: {question.Difficulty}" : "";
                difficultyText.text = difficultyLabel;
                difficultyText.gameObject.SetActive(question.Difficulty > 0);
            }

            // Display options (shuffled)
            DisplayOptions(question);

            // Update hint button visibility
            if (hintButton != null)
            {
                hintButton.gameObject.SetActive(!string.IsNullOrEmpty(question.Hint));
            }
        }

        /// <summary>
        /// Show feedback for submitted answer
        /// </summary>
        public void ShowAnswerFeedback(bool isCorrect, int selectedOptionIndex)
        {
            _hasAnswered = true;

            // Disable all option buttons
            foreach (var button in optionButtons)
            {
                if (button != null)
                    button.interactable = false;
            }

            // Highlight selected option
            if (selectedOptionIndex >= 0 && selectedOptionIndex < optionBackgrounds.Count)
            {
                if (optionBackgrounds[selectedOptionIndex] != null)
                {
                    optionBackgrounds[selectedOptionIndex].color = isCorrect ? correctColor : incorrectColor;
                }
            }

            // Highlight correct answer if user was wrong
            if (!isCorrect)
            {
                int correctOptionIndex = GetCorrectOptionDisplayIndex();
                if (correctOptionIndex >= 0 && correctOptionIndex < optionBackgrounds.Count)
                {
                    if (optionBackgrounds[correctOptionIndex] != null)
                    {
                        optionBackgrounds[correctOptionIndex].color = correctColor;
                    }
                }
            }
        }

        /// <summary>
        /// Reset panel to default state
        /// </summary>
        public void ResetUI()
        {
            _hasAnswered = false;

            // Reset option buttons
            foreach (var button in optionButtons)
            {
                if (button != null)
                    button.interactable = true;
            }

            // Reset option backgrounds
            foreach (var bg in optionBackgrounds)
            {
                if (bg != null)
                    bg.color = defaultColor;
            }

            // Hide hint panel
            if (hintPanel != null)
                hintPanel.SetActive(false);
        }

        #endregion

        #region Private Methods

        private void DisplayOptions(IVXQuizQuestion question)
        {
            if (question.Options == null || question.Options.Count == 0)
            {
                Debug.LogError("[IVXQuizQuestionPanel] Question has no options");
                return;
            }

            // Create shuffled index map
            _shuffledIndices = IVXQuestionShuffler.CreateShuffledIndexMap(question.Options.Count);

            // Display shuffled options
            for (int i = 0; i < optionButtons.Count; i++)
            {
                if (i < question.Options.Count)
                {
                    int originalIndex = _shuffledIndices[i];
                    string optionText = question.Options[originalIndex];

                    if (optionTexts[i] != null)
                    {
                        optionTexts[i].text = optionText;
                    }

                    if (optionButtons[i] != null)
                    {
                        optionButtons[i].gameObject.SetActive(true);
                    }
                }
                else
                {
                    // Hide unused option buttons
                    if (optionButtons[i] != null)
                    {
                        optionButtons[i].gameObject.SetActive(false);
                    }
                }
            }
        }

        private void OnOptionClicked(int displayedIndex)
        {
            if (_hasAnswered)
            {
                Debug.LogWarning("[IVXQuizQuestionPanel] Answer already submitted");
                return;
            }

            // Map displayed index to original index
            int originalIndex = _shuffledIndices != null && displayedIndex < _shuffledIndices.Length 
                ? _shuffledIndices[displayedIndex] 
                : displayedIndex;

            Debug.Log($"[IVXQuizQuestionPanel] Option selected: displayed={displayedIndex}, original={originalIndex}");

            // Fire event
            OnAnswerSelected?.Invoke(originalIndex);
        }

        private void OnHintButtonClicked()
        {
            if (hintPanel != null && hintText != null && !string.IsNullOrEmpty(_currentQuestion?.Hint))
            {
                hintText.text = _currentQuestion.Hint;
                hintPanel.SetActive(true);
            }

            // Fire event
            OnHintRequested?.Invoke();
        }

        private int GetCorrectOptionDisplayIndex()
        {
            if (_currentQuestion == null || _shuffledIndices == null)
                return -1;

            int correctOriginalIndex = _currentQuestion.CorrectAnswerIndex;

            // Find which displayed index maps to the correct original index
            for (int i = 0; i < _shuffledIndices.Length; i++)
            {
                if (_shuffledIndices[i] == correctOriginalIndex)
                    return i;
            }

            return -1;
        }

        #endregion
    }
}
