// IVXWeeklyQuizTestController.cs
// Test controller for Weekly Quiz demo scene

using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace IntelliVerseX.Quiz.WeeklyQuiz
{
    /// <summary>
    /// Test controller for the Weekly Quiz demo scene.
    /// Provides UI for testing all four quiz types.
    /// </summary>
    public class IVXWeeklyQuizTestController : MonoBehaviour
    {
        #region Serialized Fields

        [Header("Auto-Discovery")]
        [SerializeField] private bool autoFindUI = true;

        [Header("UI References - Status")]
        [SerializeField] private TMP_Text statusText;
        [SerializeField] private TMP_Text weekInfoText;

        [Header("UI References - Quiz Buttons")]
        [SerializeField] private Button fortuneButton;
        [SerializeField] private Button emojiButton;
        [SerializeField] private Button predictionButton;
        [SerializeField] private Button healthButton;

        [Header("UI References - Utility")]
        [SerializeField] private Button resetButton;
        [SerializeField] private Button logStatusButton;

        #endregion

        #region Private Fields

        private const string LOG_TAG = "[IVXWeeklyQuizTest]";
        private IVXWeeklyQuizManager _manager;

        #endregion

        #region Unity Lifecycle

        private void Awake()
        {
            if (autoFindUI)
            {
                AutoFindUIReferences();
            }
        }

        private void Start()
        {
            EnsureManager();
            SetupButtonListeners();
            SubscribeToEvents();
            UpdateStatus("Ready - Select a quiz type to start");
            UpdateWeekInfo();
        }

        private void OnDestroy()
        {
            UnsubscribeFromEvents();
        }

        #endregion

        #region Setup

        private void AutoFindUIReferences()
        {
            if (statusText == null)
            {
                var go = GameObject.Find("StatusText");
                if (go != null) statusText = go.GetComponent<TMP_Text>();
            }

            if (weekInfoText == null)
            {
                var go = GameObject.Find("WeekInfoText");
                if (go != null) weekInfoText = go.GetComponent<TMP_Text>();
            }

            if (fortuneButton == null)
            {
                var go = GameObject.Find("FortuneButton");
                if (go != null) fortuneButton = go.GetComponent<Button>();
            }

            if (emojiButton == null)
            {
                var go = GameObject.Find("EmojiButton");
                if (go != null) emojiButton = go.GetComponent<Button>();
            }

            if (predictionButton == null)
            {
                var go = GameObject.Find("PredictionButton");
                if (go != null) predictionButton = go.GetComponent<Button>();
            }

            if (healthButton == null)
            {
                var go = GameObject.Find("HealthButton");
                if (go != null) healthButton = go.GetComponent<Button>();
            }

            if (resetButton == null)
            {
                var go = GameObject.Find("ResetButton");
                if (go != null) resetButton = go.GetComponent<Button>();
            }

            if (logStatusButton == null)
            {
                var go = GameObject.Find("LogStatusButton");
                if (go != null) logStatusButton = go.GetComponent<Button>();
            }

            Log($"Auto-found UI: Fortune={fortuneButton != null}, Emoji={emojiButton != null}, Prediction={predictionButton != null}, Health={healthButton != null}");
        }

        private void EnsureManager()
        {
            _manager = IVXWeeklyQuizManager.Instance;
            if (_manager == null)
            {
                var go = new GameObject("IVXWeeklyQuizManager");
                _manager = go.AddComponent<IVXWeeklyQuizManager>();
            }
        }

        private void SetupButtonListeners()
        {
            if (fortuneButton != null)
            {
                fortuneButton.onClick.RemoveAllListeners();
                fortuneButton.onClick.AddListener(OnFortuneClicked);
            }

            if (emojiButton != null)
            {
                emojiButton.onClick.RemoveAllListeners();
                emojiButton.onClick.AddListener(OnEmojiClicked);
            }

            if (predictionButton != null)
            {
                predictionButton.onClick.RemoveAllListeners();
                predictionButton.onClick.AddListener(OnPredictionClicked);
            }

            if (healthButton != null)
            {
                healthButton.onClick.RemoveAllListeners();
                healthButton.onClick.AddListener(OnHealthClicked);
            }

            if (resetButton != null)
            {
                resetButton.onClick.RemoveAllListeners();
                resetButton.onClick.AddListener(OnResetClicked);
            }

            if (logStatusButton != null)
            {
                logStatusButton.onClick.RemoveAllListeners();
                logStatusButton.onClick.AddListener(OnLogStatusClicked);
            }

            Log("Button listeners configured");
        }

        private void SubscribeToEvents()
        {
            if (_manager == null) return;

            _manager.OnFortuneQuizLoaded += HandleFortuneLoaded;
            _manager.OnFortuneQuizLoadFailed += HandleFortuneLoadFailed;
            _manager.OnFortuneQuizCompleted += HandleFortuneCompleted;

            _manager.OnEmojiQuizLoaded += HandleEmojiLoaded;
            _manager.OnEmojiQuizLoadFailed += HandleEmojiLoadFailed;
            _manager.OnEmojiQuizCompleted += HandleEmojiCompleted;

            _manager.OnHealthQuizLoaded += HandleHealthLoaded;
            _manager.OnHealthQuizLoadFailed += HandleHealthLoadFailed;
            _manager.OnHealthQuizCompleted += HandleHealthCompleted;
        }

        private void UnsubscribeFromEvents()
        {
            if (_manager == null) return;

            _manager.OnFortuneQuizLoaded -= HandleFortuneLoaded;
            _manager.OnFortuneQuizLoadFailed -= HandleFortuneLoadFailed;
            _manager.OnFortuneQuizCompleted -= HandleFortuneCompleted;

            _manager.OnEmojiQuizLoaded -= HandleEmojiLoaded;
            _manager.OnEmojiQuizLoadFailed -= HandleEmojiLoadFailed;
            _manager.OnEmojiQuizCompleted -= HandleEmojiCompleted;

            _manager.OnHealthQuizLoaded -= HandleHealthLoaded;
            _manager.OnHealthQuizLoadFailed -= HandleHealthLoadFailed;
            _manager.OnHealthQuizCompleted -= HandleHealthCompleted;
        }

        #endregion

        #region Button Handlers

        private void OnFortuneClicked()
        {
            Log("Fortune button clicked");
            UpdateStatus("Loading Fortune Quiz 🔮...");
            _manager.StartFortuneQuiz();
        }

        private void OnEmojiClicked()
        {
            Log("Emoji button clicked");
            UpdateStatus("Loading Emoji Quiz 🎉...");
            _manager.StartEmojiQuiz();
        }

        private void OnPredictionClicked()
        {
            Log("Prediction button clicked");
            UpdateStatus("Prediction Quiz coming soon! ⚽");
        }

        private void OnHealthClicked()
        {
            Log("Health button clicked");
            UpdateStatus("Loading Health Quiz 💧...");
            _manager.StartHealthQuiz();
        }

        private void OnResetClicked()
        {
            Log("Reset button clicked");
#if UNITY_EDITOR
            _manager.DebugResetAll();
#endif
            UpdateStatus("All weekly quizzes reset!");
            UpdateWeekInfo();
        }

        private void OnLogStatusClicked()
        {
            Log("Log status button clicked");
#if UNITY_EDITOR
            _manager.DebugPrintStatus();
#endif
            LogCurrentStatus();
        }

        #endregion

        #region Event Handlers - Fortune

        private void HandleFortuneLoaded(IVXFortuneQuizData quiz)
        {
            Log($"Fortune loaded: {quiz.title} with {quiz.QuestionCount} questions");
            UpdateStatus($"Fortune Quiz Loaded!\n{quiz.SafeEmoji} {quiz.SafeTitle}\n{quiz.QuestionCount} questions");
            
            if (quiz.QuestionCount > 0)
            {
                _manager.BeginFortuneQuiz();
                SimulateFortuneQuiz();
            }
        }

        private void HandleFortuneLoadFailed(string error)
        {
            Log($"Fortune load failed: {error}", true);
            UpdateStatus($"Fortune Load Failed!\n{error}");
        }

        private void HandleFortuneCompleted(IVXFortuneResult result, IVXWeeklyQuizSummary summary)
        {
            Log($"Fortune completed! Result: {result.SafeTitle}");
            UpdateStatus($"Fortune Complete! {result.SafeEmoji}\n{result.SafeTitle}\n{result.SafeDescription}");
        }

        private void SimulateFortuneQuiz()
        {
            int totalQuestions = _manager.FortuneTotalQuestions;
            for (int i = 0; i < totalQuestions; i++)
            {
                var question = _manager.GetCurrentFortuneQuestion();
                if (question != null)
                {
                    int randomOption = UnityEngine.Random.Range(0, question.OptionCount);
                    _manager.SubmitFortuneAnswer(randomOption);
                    _manager.NextFortuneQuestion();
                }
            }
        }

        #endregion

        #region Event Handlers - Emoji

        private void HandleEmojiLoaded(IVXEmojiQuizData quiz)
        {
            Log($"Emoji loaded: {quiz.title} with {quiz.QuestionCount} questions");
            UpdateStatus($"Emoji Quiz Loaded!\n{quiz.SafeEmoji} {quiz.SafeTitle}\n{quiz.QuestionCount} questions");
            
            if (quiz.QuestionCount > 0)
            {
                _manager.BeginEmojiQuiz();
                SimulateEmojiQuiz();
            }
        }

        private void HandleEmojiLoadFailed(string error)
        {
            Log($"Emoji load failed: {error}", true);
            UpdateStatus($"Emoji Load Failed!\n{error}");
        }

        private void HandleEmojiCompleted(IVXEmojiResult result, IVXWeeklyQuizSummary summary)
        {
            Log($"Emoji completed! Correct: {summary.correctAnswers}/{summary.totalQuestions}");
            UpdateStatus($"Emoji Complete! {result.SafeEmoji}\n{result.SafeTitle}\nScore: {summary.correctAnswers}/{summary.totalQuestions}");
        }

        private void SimulateEmojiQuiz()
        {
            int totalQuestions = _manager.EmojiTotalQuestions;
            for (int i = 0; i < totalQuestions; i++)
            {
                var question = _manager.GetCurrentEmojiQuestion();
                if (question != null)
                {
                    int answer = question.correctAnswer;
                    _manager.SubmitEmojiAnswer(answer);
                    _manager.NextEmojiQuestion();
                }
            }
        }

        #endregion

        #region Event Handlers - Health

        private void HandleHealthLoaded(IVXHealthQuizData quiz)
        {
            Log($"Health loaded: {quiz.title} with {quiz.QuestionCount} questions");
            UpdateStatus($"Health Quiz Loaded!\n{quiz.SafeEmoji} {quiz.SafeTitle}\n{quiz.QuestionCount} questions");
            
            if (quiz.QuestionCount > 0)
            {
                _manager.BeginHealthQuiz();
                SimulateHealthQuiz();
            }
        }

        private void HandleHealthLoadFailed(string error)
        {
            Log($"Health load failed: {error}", true);
            UpdateStatus($"Health Load Failed!\n{error}");
        }

        private void HandleHealthCompleted(IVXHealthResult result, IVXWeeklyQuizSummary summary)
        {
            Log($"Health completed! Correct: {summary.correctAnswers}/{summary.totalQuestions}");
            UpdateStatus($"Health Complete! {result.SafeEmoji}\n{result.SafeTitle}\nScore: {summary.correctAnswers}/{summary.totalQuestions}");
        }

        private void SimulateHealthQuiz()
        {
            int totalQuestions = _manager.HealthTotalQuestions;
            for (int i = 0; i < totalQuestions; i++)
            {
                var question = _manager.GetCurrentHealthQuestion();
                if (question != null)
                {
                    int answer = question.correctAnswer;
                    _manager.SubmitHealthAnswer(answer);
                    _manager.NextHealthQuestion();
                }
            }
        }

        #endregion

        #region UI Updates

        private void UpdateStatus(string message)
        {
            if (statusText != null)
            {
                statusText.text = message;
            }
            Log($"Status: {message}");
        }

        private void UpdateWeekInfo()
        {
            if (weekInfoText != null)
            {
                var weekKey = GetCurrentWeekKey();
                var timeUntilReset = IVXWeeklyQuizManager.GetTimeUntilWeeklyReset();
                weekInfoText.text = $"Week: {weekKey}\nReset in: {timeUntilReset:d\\d\\ hh\\h\\ mm\\m}";
            }
        }

        private static string GetCurrentWeekKey()
        {
            DateTime now = DateTime.UtcNow;
            int isoDayOfWeek = now.DayOfWeek == DayOfWeek.Sunday ? 7 : (int)now.DayOfWeek;
            DateTime thursday = now.AddDays(4 - isoDayOfWeek);
            int isoYear = thursday.Year;

            DateTime jan4 = new DateTime(isoYear, 1, 4);
            int jan4Dow = jan4.DayOfWeek == DayOfWeek.Sunday ? 7 : (int)jan4.DayOfWeek;
            DateTime startOfWeek1 = jan4.AddDays(1 - jan4Dow);

            int weekNumber = (thursday - startOfWeek1).Days / 7 + 1;
            return $"{isoYear}-W{weekNumber:D2}";
        }

        private void LogCurrentStatus()
        {
            string status = $"Weekly Quiz Status:\n" +
                           $"  Fortune: completed={_manager.HasCompletedFortuneThisWeek()}, loaded={_manager.CurrentFortuneQuiz != null}\n" +
                           $"  Emoji: completed={_manager.HasCompletedEmojiThisWeek()}, loaded={_manager.CurrentEmojiQuiz != null}\n" +
                           $"  Health: completed={_manager.HasCompletedHealthThisWeek()}, loaded={_manager.CurrentHealthQuiz != null}";
            
            UpdateStatus(status);
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
