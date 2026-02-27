// IVXDailyQuizTestController.cs
// Test controller for Daily Quiz scene - handles UI flow and interactions

using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace IntelliVerseX.Quiz.DailyQuiz
{
    /// <summary>
    /// Unified controller for playing Daily Quiz types (Daily and Premium).
    /// Manages MainMenuPanel, QuestionPanel, and ResultPanel UI flow.
    /// </summary>
    public class IVXDailyQuizTestController : MonoBehaviour
    {
        #region Serialized Fields

        [Header("Panel References")]
        [SerializeField] private GameObject mainMenuPanel;
        [SerializeField] private GameObject questionPanel;
        [SerializeField] private GameObject resultPanel;

        [Header("Main Menu - Quiz Selection")]
        [SerializeField] private Button dailyQuizButton;
        [SerializeField] private Button premiumQuizButton;
        [SerializeField] private TMP_Text dailyQuizStatusText;
        [SerializeField] private TMP_Text premiumQuizStatusText;
        [SerializeField] private TMP_Text streakText;
        [SerializeField] private TMP_Text timeUntilResetText;

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
        [SerializeField] private TMP_Text streakResultText;
        [SerializeField] private Button retryButton;
        [SerializeField] private Button backButton;

        [Header("Colors")]
        [SerializeField] private Color normalOptionColor = new Color(0.2f, 0.2f, 0.3f);
        [SerializeField] private Color correctColor = new Color(0.2f, 0.7f, 0.3f);
        [SerializeField] private Color incorrectColor = new Color(0.8f, 0.2f, 0.2f);
        [SerializeField] private Color selectedColor = new Color(0.3f, 0.5f, 0.8f);

        #endregion

        #region Private Fields

        private IVXDailyQuizManager _manager;
        private IVXDailyQuizType _currentQuizType;
        private int _currentQuestionIndex;
        private int _totalQuestions;
        private int _correctAnswers;
        private bool _answerSubmitted;
        private bool _hintUsed;

        private const string LOG_TAG = "[IVXDailyQuizTest]";

        #endregion

        #region Unity Lifecycle

        private void Start()
        {
            Initialize();
        }

        private void Update()
        {
            if (mainMenuPanel != null && mainMenuPanel.activeSelf)
            {
                UpdateTimeUntilReset();
            }
        }

        #endregion

        #region Initialization

        private void Initialize()
        {
            EnsureManagerExists();
            _manager = IVXDailyQuizManager.Instance;

            SetupButtonListeners();
            SubscribeToEvents();
            
            ShowMainMenu();
        }

        private void EnsureManagerExists()
        {
            if (IVXDailyQuizManager.Instance == null)
            {
                var managerGO = new GameObject("IVXDailyQuizManager");
                managerGO.AddComponent<IVXDailyQuizManager>();
                Log("Created IVXDailyQuizManager");
            }
        }

        private void SetupButtonListeners()
        {
            if (dailyQuizButton != null)
            {
                dailyQuizButton.onClick.RemoveAllListeners();
                dailyQuizButton.onClick.AddListener(() => StartQuiz(IVXDailyQuizType.Daily));
            }

            if (premiumQuizButton != null)
            {
                premiumQuizButton.onClick.RemoveAllListeners();
                premiumQuizButton.onClick.AddListener(() => StartQuiz(IVXDailyQuizType.DailyPremium));
            }

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

            _manager.OnDailyQuizLoaded += OnDailyLoaded;
            _manager.OnDailyQuizLoadFailed += OnQuizLoadFailed;
            _manager.OnDailyQuizCompleted += OnDailyCompleted;

            _manager.OnPremiumQuizLoaded += OnPremiumLoaded;
            _manager.OnPremiumQuizLoadFailed += OnQuizLoadFailed;
            _manager.OnPremiumQuizCompleted += OnPremiumCompleted;
        }

        private void OnDestroy()
        {
            if (_manager == null) return;

            _manager.OnDailyQuizLoaded -= OnDailyLoaded;
            _manager.OnDailyQuizLoadFailed -= OnQuizLoadFailed;
            _manager.OnDailyQuizCompleted -= OnDailyCompleted;

            _manager.OnPremiumQuizLoaded -= OnPremiumLoaded;
            _manager.OnPremiumQuizLoadFailed -= OnQuizLoadFailed;
            _manager.OnPremiumQuizCompleted -= OnPremiumCompleted;
        }

        #endregion

        #region Public Methods

        public void StartQuiz(IVXDailyQuizType quizType)
        {
            _currentQuizType = quizType;
            _currentQuestionIndex = 0;
            _correctAnswers = 0;

            Log($"Starting {quizType} quiz...");

            switch (quizType)
            {
                case IVXDailyQuizType.Daily:
                    _manager.StartDailyQuiz();
                    break;
                case IVXDailyQuizType.DailyPremium:
                    _manager.StartPremiumQuiz();
                    break;
            }
        }

        #endregion

        #region Quiz Load Handlers

        private void OnDailyLoaded(IVXDailyQuizData quiz)
        {
            Log($"Daily loaded: {quiz.QuestionCount} questions");
            _totalQuestions = quiz.QuestionCount;
            _manager.BeginDailyQuiz();
            ShowQuestionPanel();
            DisplayCurrentQuestion();
        }

        private void OnPremiumLoaded(IVXDailyQuizData quiz)
        {
            Log($"Premium loaded: {quiz.QuestionCount} questions");
            _totalQuestions = quiz.QuestionCount;
            _manager.BeginPremiumQuiz();
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

        private void OnDailyCompleted(IVXDailyResult result, IVXDailyQuizSummary summary)
        {
            ShowResult(result.SafeTitle, result.SafeEmoji, result.SafeDescription, summary);
        }

        private void OnPremiumCompleted(IVXDailyResult result, IVXDailyQuizSummary summary)
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

            switch (_currentQuizType)
            {
                case IVXDailyQuizType.Daily:
                    var dailyQ = _manager.GetCurrentDailyQuestion();
                    if (dailyQ != null)
                    {
                        questionStr = dailyQ.questionText;
                        hint = dailyQ.hint;
                        for (int i = 0; i < dailyQ.OptionCount && i < 4; i++)
                            options[i] = dailyQ.GetOptionText(i);
                    }
                    break;

                case IVXDailyQuizType.DailyPremium:
                    var premiumQ = _manager.GetCurrentPremiumQuestion();
                    if (premiumQ != null)
                    {
                        questionStr = premiumQ.questionText;
                        hint = premiumQ.hint;
                        for (int i = 0; i < premiumQ.OptionCount && i < 4; i++)
                            options[i] = premiumQ.GetOptionText(i);
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
                string typeName = _currentQuizType == IVXDailyQuizType.DailyPremium ? "Premium Daily Quiz" : "Daily Quiz";
                quizTypeText.text = $"{emoji} {typeName}";
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

        private string GetQuizEmoji(IVXDailyQuizType type)
        {
            return type switch
            {
                IVXDailyQuizType.Daily => "📅",
                IVXDailyQuizType.DailyPremium => "💎",
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
                case IVXDailyQuizType.Daily:
                    var dailyQ = _manager.GetCurrentDailyQuestion();
                    if (dailyQ != null)
                    {
                        correctAnswer = dailyQ.correctAnswer;
                        explanation = dailyQ.explanation;
                        isCorrect = optionIndex == correctAnswer;
                    }
                    _manager.SubmitDailyAnswer(optionIndex);
                    break;

                case IVXDailyQuizType.DailyPremium:
                    var premiumQ = _manager.GetCurrentPremiumQuestion();
                    if (premiumQ != null)
                    {
                        correctAnswer = premiumQ.correctAnswer;
                        explanation = premiumQ.explanation;
                        isCorrect = optionIndex == correctAnswer;
                    }
                    _manager.SubmitPremiumAnswer(optionIndex);
                    break;
            }

            if (isCorrect)
                _correctAnswers++;

            ShowAnswerFeedback(optionIndex, correctAnswer, isCorrect, explanation);
        }

        private void ShowAnswerFeedback(int selectedIndex, int correctIndex, bool isCorrect, string explanation)
        {
            if (selectedIndex >= 0 && selectedIndex < optionBackgrounds.Length && optionBackgrounds[selectedIndex] != null)
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
                case IVXDailyQuizType.Daily:
                    _manager.NextDailyQuestion();
                    break;
                case IVXDailyQuizType.DailyPremium:
                    _manager.NextPremiumQuestion();
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

        private void ShowResult(string title, string emoji, string description, IVXDailyQuizSummary summary)
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

            if (streakResultText != null)
            {
                int consecutiveStreak = summary.isPremium 
                    ? _manager.PremiumConsecutiveDaysStreak 
                    : _manager.ConsecutiveDaysStreak;
                streakResultText.text = consecutiveStreak > 1 
                    ? $"🔥 {consecutiveStreak} day streak!" 
                    : "";
            }
        }

        #endregion

        #region Panel Management

        private void ShowMainMenu()
        {
            if (mainMenuPanel != null) mainMenuPanel.SetActive(true);
            if (questionPanel != null) questionPanel.SetActive(false);
            if (resultPanel != null) resultPanel.SetActive(false);

            UpdateMainMenuStatus();
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

        private void UpdateMainMenuStatus()
        {
            if (_manager == null) return;

            bool dailyCompleted = _manager.HasCompletedDailyToday();
            bool premiumCompleted = _manager.HasCompletedPremiumToday();

            if (dailyQuizStatusText != null)
            {
                dailyQuizStatusText.text = dailyCompleted ? "✓ Completed Today" : "📅 Available";
                dailyQuizStatusText.color = dailyCompleted ? Color.green : Color.white;
            }

            if (premiumQuizStatusText != null)
            {
                premiumQuizStatusText.text = premiumCompleted ? "✓ Completed Today" : "💎 Available";
                premiumQuizStatusText.color = premiumCompleted ? Color.green : Color.white;
            }

            if (streakText != null)
            {
                int streak = _manager.ConsecutiveDaysStreak;
                streakText.text = streak > 0 ? $"🔥 {streak} day streak" : "Start your streak!";
            }

            if (dailyQuizButton != null)
                dailyQuizButton.interactable = !dailyCompleted;

            if (premiumQuizButton != null)
                premiumQuizButton.interactable = !premiumCompleted;
        }

        private void UpdateTimeUntilReset()
        {
            if (timeUntilResetText == null) return;

            TimeSpan remaining = IVXDailyQuizManager.GetTimeUntilDailyReset();
            timeUntilResetText.text = $"Resets in: {remaining:hh\\:mm\\:ss}";
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
