// IVXQuizPlayController.cs
// Unified UI controller for playing all weekly quiz types

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace IntelliVerseX.Quiz.WeeklyQuiz
{
    /// <summary>
    /// Unified controller for playing all weekly quiz types.
    /// Manages QuestionPanel and ResultPanel UI flow.
    /// </summary>
    public class IVXQuizPlayController : MonoBehaviour
    {
        #region Serialized Fields

        [Header("Panel References")]
        [SerializeField] private GameObject mainMenuPanel;
        [SerializeField] private GameObject questionPanel;
        [SerializeField] private GameObject resultPanel;

        [Header("Question Panel - Header")]
        [SerializeField] private TMP_Text quizTypeText;
        [SerializeField] private TMP_Text progressText;
        [SerializeField] private Image progressFill;
        [SerializeField] private Button closeButton;

        [Header("Question Panel - Question")]
        [SerializeField] private TMP_Text questionText;
        [SerializeField] private TMP_Text questionNumberText;

        [Header("Question Panel - Options")]
        [SerializeField] private Button[] optionButtons;
        [SerializeField] private TMP_Text[] optionTexts;
        [SerializeField] private Image[] optionBackgrounds;

        [Header("Question Panel - Feedback")]
        [SerializeField] private GameObject feedbackPanel;
        [SerializeField] private TMP_Text feedbackText;
        [SerializeField] private TMP_Text explanationText;
        [SerializeField] private Button nextButton;

        [Header("Question Panel - Hint")]
        [SerializeField] private Button hintButton;
        [SerializeField] private GameObject hintPanel;
        [SerializeField] private TMP_Text hintText;

        [Header("Result Panel")]
        [SerializeField] private TMP_Text resultTitleText;
        [SerializeField] private TMP_Text resultEmojiText;
        [SerializeField] private TMP_Text resultDescriptionText;
        [SerializeField] private TMP_Text scoreText;
        [SerializeField] private TMP_Text scoreDetailsText;
        [SerializeField] private Button retryButton;
        [SerializeField] private Button backButton;

        [Header("Colors")]
        [SerializeField] private Color normalOptionColor = new Color(0.2f, 0.2f, 0.3f);
        [SerializeField] private Color correctColor = new Color(0.2f, 0.7f, 0.3f);
        [SerializeField] private Color incorrectColor = new Color(0.8f, 0.2f, 0.2f);
        [SerializeField] private Color selectedColor = new Color(0.3f, 0.5f, 0.8f);

        #endregion

        #region Private Fields

        private IVXWeeklyQuizManager _manager;
        private IVXWeeklyQuizType _currentQuizType;
        private int _currentQuestionIndex;
        private int _totalQuestions;
        private int _correctAnswers;
        private bool _answerSubmitted;
        private bool _hintUsed;

        private const string LOG_TAG = "[IVXQuizPlay]";

        #endregion

        #region Unity Lifecycle

        private void Start()
        {
            Initialize();
        }

        #endregion

        #region Initialization

        private void Initialize()
        {
            _manager = IVXWeeklyQuizManager.Instance;
            
            SetupButtonListeners();
            SubscribeToEvents();
            
            ShowMainMenu();
        }

        private void SetupButtonListeners()
        {
            if (closeButton != null)
            {
                closeButton.onClick.RemoveAllListeners();
                closeButton.onClick.AddListener(OnCloseClicked);
            }

            if (nextButton != null)
            {
                nextButton.onClick.RemoveAllListeners();
                nextButton.onClick.AddListener(OnNextClicked);
            }

            if (hintButton != null)
            {
                hintButton.onClick.RemoveAllListeners();
                hintButton.onClick.AddListener(OnHintClicked);
            }

            if (retryButton != null)
            {
                retryButton.onClick.RemoveAllListeners();
                retryButton.onClick.AddListener(OnRetryClicked);
            }

            if (backButton != null)
            {
                backButton.onClick.RemoveAllListeners();
                backButton.onClick.AddListener(OnBackClicked);
            }

            for (int i = 0; i < optionButtons.Length; i++)
            {
                int index = i;
                if (optionButtons[i] != null)
                {
                    optionButtons[i].onClick.RemoveAllListeners();
                    optionButtons[i].onClick.AddListener(() => OnOptionClicked(index));
                }
            }
        }

        private void SubscribeToEvents()
        {
            if (_manager == null) return;

            _manager.OnFortuneQuizLoaded += OnFortuneLoaded;
            _manager.OnFortuneQuizLoadFailed += OnQuizLoadFailed;
            _manager.OnFortuneQuizCompleted += OnFortuneCompleted;

            _manager.OnEmojiQuizLoaded += OnEmojiLoaded;
            _manager.OnEmojiQuizLoadFailed += OnQuizLoadFailed;
            _manager.OnEmojiQuizCompleted += OnEmojiCompleted;

            _manager.OnHealthQuizLoaded += OnHealthLoaded;
            _manager.OnHealthQuizLoadFailed += OnQuizLoadFailed;
            _manager.OnHealthQuizCompleted += OnHealthCompleted;
        }

        private void OnDestroy()
        {
            if (_manager == null) return;

            _manager.OnFortuneQuizLoaded -= OnFortuneLoaded;
            _manager.OnFortuneQuizLoadFailed -= OnQuizLoadFailed;
            _manager.OnFortuneQuizCompleted -= OnFortuneCompleted;

            _manager.OnEmojiQuizLoaded -= OnEmojiLoaded;
            _manager.OnEmojiQuizLoadFailed -= OnQuizLoadFailed;
            _manager.OnEmojiQuizCompleted -= OnEmojiCompleted;

            _manager.OnHealthQuizLoaded -= OnHealthLoaded;
            _manager.OnHealthQuizLoadFailed -= OnQuizLoadFailed;
            _manager.OnHealthQuizCompleted -= OnHealthCompleted;
        }

        #endregion

        #region Public Methods

        public void StartQuiz(IVXWeeklyQuizType quizType)
        {
            _currentQuizType = quizType;
            _currentQuestionIndex = 0;
            _correctAnswers = 0;
            
            Log($"Starting {quizType} quiz...");
            
            switch (quizType)
            {
                case IVXWeeklyQuizType.Fortune:
                    _manager.StartFortuneQuiz();
                    break;
                case IVXWeeklyQuizType.Emoji:
                    _manager.StartEmojiQuiz();
                    break;
                case IVXWeeklyQuizType.Prediction:
                    Log("Prediction quiz not yet implemented", true);
                    ShowMainMenu();
                    return;
                case IVXWeeklyQuizType.Health:
                    _manager.StartHealthQuiz();
                    break;
            }
        }

        #endregion

        #region Quiz Load Handlers

        private void OnFortuneLoaded(IVXFortuneQuizData quiz)
        {
            Log($"Fortune loaded: {quiz.QuestionCount} questions");
            _totalQuestions = quiz.QuestionCount;
            _manager.BeginFortuneQuiz();
            ShowQuestionPanel();
            DisplayCurrentQuestion();
        }

        private void OnEmojiLoaded(IVXEmojiQuizData quiz)
        {
            Log($"Emoji loaded: {quiz.QuestionCount} questions");
            _totalQuestions = quiz.QuestionCount;
            _manager.BeginEmojiQuiz();
            ShowQuestionPanel();
            DisplayCurrentQuestion();
        }

        private void OnHealthLoaded(IVXHealthQuizData quiz)
        {
            Log($"Health loaded: {quiz.QuestionCount} questions");
            _totalQuestions = quiz.QuestionCount;
            _manager.BeginHealthQuiz();
            ShowQuestionPanel();
            DisplayCurrentQuestion();
        }

        private void OnQuizLoadFailed(string error)
        {
            Log($"Quiz load failed: {error}", true);
            ShowMainMenu();
        }

        #endregion

        #region Quiz Complete Handlers

        private void OnFortuneCompleted(IVXFortuneResult result, IVXWeeklyQuizSummary summary)
        {
            ShowResult(result.SafeTitle, result.SafeEmoji, result.SafeDescription, summary);
        }

        private void OnEmojiCompleted(IVXEmojiResult result, IVXWeeklyQuizSummary summary)
        {
            ShowResult(result.SafeTitle, result.SafeEmoji, result.SafeDescription, summary);
        }

        private void OnHealthCompleted(IVXHealthResult result, IVXWeeklyQuizSummary summary)
        {
            ShowResult(result.SafeTitle, result.SafeEmoji, result.SafeDescription, summary);
        }

        #endregion

        #region Question Display

        private void DisplayCurrentQuestion()
        {
            _answerSubmitted = false;
            _hintUsed = false;
            
            ResetOptionColors();
            HideFeedback();
            HideHint();
            
            string questionStr = "";
            string[] options = new string[4];
            string hint = "";
            int correctAnswer = -1;

            switch (_currentQuizType)
            {
                case IVXWeeklyQuizType.Fortune:
                    var fortuneQ = _manager.GetCurrentFortuneQuestion();
                    if (fortuneQ != null)
                    {
                        questionStr = fortuneQ.questionText;
                        hint = fortuneQ.hint;
                        for (int i = 0; i < fortuneQ.OptionCount && i < 4; i++)
                            options[i] = fortuneQ.GetOptionText(i);
                    }
                    break;

                case IVXWeeklyQuizType.Emoji:
                    var emojiQ = _manager.GetCurrentEmojiQuestion();
                    if (emojiQ != null)
                    {
                        questionStr = emojiQ.questionText;
                        hint = emojiQ.hint;
                        correctAnswer = emojiQ.correctAnswer;
                        for (int i = 0; i < emojiQ.OptionCount && i < 4; i++)
                            options[i] = emojiQ.GetOptionText(i);
                    }
                    break;

                case IVXWeeklyQuizType.Health:
                    var healthQ = _manager.GetCurrentHealthQuestion();
                    if (healthQ != null)
                    {
                        questionStr = healthQ.questionText;
                        hint = healthQ.hint;
                        correctAnswer = healthQ.correctAnswer;
                        for (int i = 0; i < healthQ.OptionCount && i < 4; i++)
                            options[i] = healthQ.GetOptionText(i);
                    }
                    break;
            }

            UpdateQuestionUI(questionStr, options, hint);
            UpdateProgress();
        }

        private void UpdateQuestionUI(string question, string[] options, string hint)
        {
            if (quizTypeText != null)
            {
                string emoji = GetQuizEmoji(_currentQuizType);
                quizTypeText.text = $"{emoji} {_currentQuizType} Quiz";
            }

            if (questionText != null)
                questionText.text = question;

            if (questionNumberText != null)
                questionNumberText.text = $"Question {_currentQuestionIndex + 1}";

            for (int i = 0; i < optionTexts.Length && i < options.Length; i++)
            {
                if (optionTexts[i] != null)
                    optionTexts[i].text = options[i] ?? "";

                if (optionButtons[i] != null)
                    optionButtons[i].gameObject.SetActive(!string.IsNullOrEmpty(options[i]));
            }

            if (hintButton != null)
                hintButton.interactable = !string.IsNullOrEmpty(hint);

            if (hintText != null)
                hintText.text = hint ?? "";
        }

        private void UpdateProgress()
        {
            if (progressText != null)
                progressText.text = $"{_currentQuestionIndex + 1} / {_totalQuestions}";

            if (progressFill != null)
                progressFill.fillAmount = (float)(_currentQuestionIndex + 1) / _totalQuestions;
        }

        private string GetQuizEmoji(IVXWeeklyQuizType type)
        {
            return type switch
            {
                IVXWeeklyQuizType.Fortune => "🔮",
                IVXWeeklyQuizType.Emoji => "🎉",
                IVXWeeklyQuizType.Prediction => "⚽",
                IVXWeeklyQuizType.Health => "💧",
                _ => "❓"
            };
        }

        #endregion

        #region Answer Handling

        private void OnOptionClicked(int optionIndex)
        {
            if (_answerSubmitted) return;
            
            _answerSubmitted = true;
            
            int correctAnswer = -1;
            string explanation = "";
            bool isCorrect = false;

            switch (_currentQuizType)
            {
                case IVXWeeklyQuizType.Fortune:
                    _manager.SubmitFortuneAnswer(optionIndex);
                    isCorrect = true;
                    break;

                case IVXWeeklyQuizType.Emoji:
                    var emojiQ = _manager.GetCurrentEmojiQuestion();
                    if (emojiQ != null)
                    {
                        correctAnswer = emojiQ.correctAnswer;
                        explanation = emojiQ.explanation;
                        isCorrect = optionIndex == correctAnswer;
                    }
                    _manager.SubmitEmojiAnswer(optionIndex);
                    break;

                case IVXWeeklyQuizType.Health:
                    var healthQ = _manager.GetCurrentHealthQuestion();
                    if (healthQ != null)
                    {
                        correctAnswer = healthQ.correctAnswer;
                        explanation = healthQ.explanation;
                        isCorrect = optionIndex == correctAnswer;
                    }
                    _manager.SubmitHealthAnswer(optionIndex);
                    break;
            }

            if (isCorrect)
                _correctAnswers++;

            ShowAnswerFeedback(optionIndex, correctAnswer, isCorrect, explanation);
        }

        private void ShowAnswerFeedback(int selectedIndex, int correctIndex, bool isCorrect, string explanation)
        {
            if (optionBackgrounds[selectedIndex] != null)
            {
                optionBackgrounds[selectedIndex].color = isCorrect ? correctColor : incorrectColor;
            }

            if (!isCorrect && correctIndex >= 0 && correctIndex < optionBackgrounds.Length && optionBackgrounds[correctIndex] != null)
            {
                optionBackgrounds[correctIndex].color = correctColor;
            }

            for (int i = 0; i < optionButtons.Length; i++)
            {
                if (optionButtons[i] != null)
                    optionButtons[i].interactable = false;
            }

            if (feedbackPanel != null)
                feedbackPanel.SetActive(true);

            if (feedbackText != null)
                feedbackText.text = isCorrect ? "Correct! ✓" : "Incorrect ✗";

            if (explanationText != null)
                explanationText.text = explanation ?? "";

            if (nextButton != null)
            {
                nextButton.gameObject.SetActive(true);
                var btnText = nextButton.GetComponentInChildren<TMP_Text>();
                if (btnText != null)
                {
                    btnText.text = (_currentQuestionIndex + 1 >= _totalQuestions) ? "See Results" : "Next Question";
                }
            }
        }

        private void ResetOptionColors()
        {
            foreach (var bg in optionBackgrounds)
            {
                if (bg != null)
                    bg.color = normalOptionColor;
            }

            foreach (var btn in optionButtons)
            {
                if (btn != null)
                    btn.interactable = true;
            }
        }

        private void HideFeedback()
        {
            if (feedbackPanel != null)
                feedbackPanel.SetActive(false);

            if (nextButton != null)
                nextButton.gameObject.SetActive(false);
        }

        #endregion

        #region Hint

        private void OnHintClicked()
        {
            if (_hintUsed) return;
            _hintUsed = true;

            if (hintPanel != null)
                hintPanel.SetActive(true);

            if (hintButton != null)
                hintButton.interactable = false;
        }

        private void HideHint()
        {
            if (hintPanel != null)
                hintPanel.SetActive(false);
        }

        #endregion

        #region Navigation

        private void OnNextClicked()
        {
            _currentQuestionIndex++;

            switch (_currentQuizType)
            {
                case IVXWeeklyQuizType.Fortune:
                    _manager.NextFortuneQuestion();
                    break;
                case IVXWeeklyQuizType.Emoji:
                    _manager.NextEmojiQuestion();
                    break;
                case IVXWeeklyQuizType.Health:
                    _manager.NextHealthQuestion();
                    break;
            }

            if (_currentQuestionIndex < _totalQuestions)
            {
                DisplayCurrentQuestion();
            }
        }

        private void OnCloseClicked()
        {
            ShowMainMenu();
        }

        private void OnRetryClicked()
        {
            StartQuiz(_currentQuizType);
        }

        private void OnBackClicked()
        {
            ShowMainMenu();
        }

        #endregion

        #region Result Display

        private void ShowResult(string title, string emoji, string description, IVXWeeklyQuizSummary summary)
        {
            ShowResultPanel();

            if (resultTitleText != null)
                resultTitleText.text = title;

            if (resultEmojiText != null)
                resultEmojiText.text = emoji;

            if (resultDescriptionText != null)
                resultDescriptionText.text = description;

            if (scoreText != null)
            {
                int percent = _totalQuestions > 0 ? (_correctAnswers * 100 / _totalQuestions) : 0;
                scoreText.text = $"{percent}%";
            }

            if (scoreDetailsText != null)
                scoreDetailsText.text = $"{_correctAnswers} out of {_totalQuestions} correct";
        }

        #endregion

        #region Panel Management

        private void ShowMainMenu()
        {
            if (mainMenuPanel != null) mainMenuPanel.SetActive(true);
            if (questionPanel != null) questionPanel.SetActive(false);
            if (resultPanel != null) resultPanel.SetActive(false);
        }

        private void ShowQuestionPanel()
        {
            if (mainMenuPanel != null) mainMenuPanel.SetActive(false);
            if (questionPanel != null) questionPanel.SetActive(true);
            if (resultPanel != null) resultPanel.SetActive(false);
        }

        private void ShowResultPanel()
        {
            if (mainMenuPanel != null) mainMenuPanel.SetActive(false);
            if (questionPanel != null) questionPanel.SetActive(false);
            if (resultPanel != null) resultPanel.SetActive(true);
        }

        #endregion

        #region Logging

        private void Log(string message, bool isError = false)
        {
            if (isError)
                Debug.LogError($"{LOG_TAG} {message}");
            else
                Debug.Log($"{LOG_TAG} {message}");
        }

        #endregion
    }
}
