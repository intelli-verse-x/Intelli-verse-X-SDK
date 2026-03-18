// IVXDailyQuizManager.cs
// Unified manager for daily quiz types in IntelliVerse-X SDK

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace IntelliVerseX.Quiz.DailyQuiz
{
    /// <summary>
    /// Unified manager for Daily Quiz types: Daily 📅 and Premium Daily 💎.
    /// Singleton - lives on a DontDestroyOnLoad GameObject.
    /// </summary>
    public class IVXDailyQuizManager : MonoBehaviour
    {
        #region Singleton

        public static IVXDailyQuizManager Instance { get; private set; }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        #endregion

        #region Events - Daily Quiz

        public event Action<IVXDailyQuizData> OnDailyQuizLoaded;
        public event Action<string> OnDailyQuizLoadFailed;
        public event Action<IVXDailyQuestion, int, int> OnDailyQuestionDisplayed;
        public event Action<IVXDailyQuestion, int, bool, int> OnDailyAnswerSubmitted;
        public event Action<int> OnDailyStreakBonus;
        public event Action<string> OnDailyStreakBroken;
        public event Action<IVXDailyResult, IVXDailyQuizSummary> OnDailyQuizCompleted;
        public event Action OnDailyQuizCancelled;

        #endregion

        #region Events - Premium Daily Quiz

        public event Action<IVXDailyQuizData> OnPremiumQuizLoaded;
        public event Action<string> OnPremiumQuizLoadFailed;
        public event Action<IVXDailyQuestion, int, int> OnPremiumQuestionDisplayed;
        public event Action<IVXDailyQuestion, int, bool, int> OnPremiumAnswerSubmitted;
        public event Action<int> OnPremiumStreakBonus;
        public event Action<string> OnPremiumStreakBroken;
        public event Action<IVXDailyResult, IVXDailyQuizSummary> OnPremiumQuizCompleted;
        public event Action OnPremiumQuizCancelled;

        #endregion

        #region State - Daily Quiz

        private IVXDailyQuizData _dailyQuiz;
        private int _dailyQuestionIndex;
        private List<IVXDailyQuizAnswerRecord> _dailyAnswerHistory;
        private int _dailyCorrectCount;
        private int _dailyCurrentStreak;
        private int _dailyBestStreak;
        private bool _isDailyLoading;
        private bool _isDailyActive;

        private const string kDailyCompletedKey = "IVX_DailyQuiz_CompletedDate_";
        private const string kDailyProgressKey = "IVX_DailyQuiz_Progress";
        private const string kDailyStreakKey = "IVX_DailyQuiz_ConsecutiveStreak";
        private const string kDailyLastCompletedKey = "IVX_DailyQuiz_LastCompletedDate";

        public IVXDailyQuizData CurrentDailyQuiz => _dailyQuiz;
        public int DailyCurrentQuestionIndex => _dailyQuestionIndex;
        public int DailyTotalQuestions => _dailyQuiz?.QuestionCount ?? 0;
        public int DailyCorrectCount => _dailyCorrectCount;
        public int DailyCurrentStreak => _dailyCurrentStreak;
        public int DailyBestStreak => _dailyBestStreak;
        public bool IsDailyQuizActive => _isDailyActive;
        public bool IsDailyLoading => _isDailyLoading;

        #endregion

        #region State - Premium Daily Quiz

        private IVXDailyQuizData _premiumQuiz;
        private int _premiumQuestionIndex;
        private List<IVXDailyQuizAnswerRecord> _premiumAnswerHistory;
        private int _premiumCorrectCount;
        private int _premiumCurrentStreak;
        private int _premiumBestStreak;
        private bool _isPremiumLoading;
        private bool _isPremiumActive;

        private const string kPremiumCompletedKey = "IVX_PremiumDailyQuiz_CompletedDate_";
        private const string kPremiumProgressKey = "IVX_PremiumDailyQuiz_Progress";
        private const string kPremiumStreakKey = "IVX_PremiumDailyQuiz_ConsecutiveStreak";
        private const string kPremiumLastCompletedKey = "IVX_PremiumDailyQuiz_LastCompletedDate";

        public IVXDailyQuizData CurrentPremiumQuiz => _premiumQuiz;
        public int PremiumCurrentQuestionIndex => _premiumQuestionIndex;
        public int PremiumTotalQuestions => _premiumQuiz?.QuestionCount ?? 0;
        public int PremiumCorrectCount => _premiumCorrectCount;
        public int PremiumCurrentStreak => _premiumCurrentStreak;
        public int PremiumBestStreak => _premiumBestStreak;
        public bool IsPremiumQuizActive => _isPremiumActive;
        public bool IsPremiumLoading => _isPremiumLoading;

        #endregion

        #region Shared

        private IVXDailyQuizService _service;
        private static bool _prefsSavePending;

        public bool IsAnyQuizActive => _isDailyActive || _isPremiumActive;

        /// <summary>
        /// Gets the consecutive days streak for daily quizzes.
        /// </summary>
        public int ConsecutiveDaysStreak => PlayerPrefs.GetInt(kDailyStreakKey, 0);

        /// <summary>
        /// Gets the consecutive days streak for premium quizzes.
        /// </summary>
        public int PremiumConsecutiveDaysStreak => PlayerPrefs.GetInt(kPremiumStreakKey, 0);

        #endregion

        #region Unity Lifecycle

        private void Start()
        {
            EnsureInitialized();
        }

        private void OnApplicationPause(bool pauseStatus)
        {
            if (pauseStatus) FlushPrefsSaveImmediate();
        }

        private void OnApplicationQuit()
        {
            FlushPrefsSaveImmediate();
        }

        #endregion

        #region Initialization

        private void EnsureInitialized()
        {
            _service ??= new IVXDailyQuizService();
            _dailyAnswerHistory ??= new List<IVXDailyQuizAnswerRecord>();
            _premiumAnswerHistory ??= new List<IVXDailyQuizAnswerRecord>();
        }

        #endregion

        #region Daily Quiz API

        public async void StartDailyQuiz()
        {
            EnsureInitialized();
            if (_isDailyLoading)
            {
                Debug.LogWarning("[IVXDailyQuiz] Daily already loading");
                return;
            }

            _isDailyLoading = true;
            Debug.Log("[IVXDailyQuiz] Loading Daily quiz...");

            try
            {
                var result = await _service.FetchDailyQuizAsync();
                _isDailyLoading = false;

                if (result.Success)
                {
                    _dailyQuiz = result.GetQuizData<IVXDailyQuizData>();
                    Debug.Log($"[IVXDailyQuiz] Daily loaded: {_dailyQuiz.SafeTitle} ({_dailyQuiz.QuestionCount}Q)");
                    OnDailyQuizLoaded?.Invoke(_dailyQuiz);
                }
                else
                {
                    Debug.LogError($"[IVXDailyQuiz] Daily failed: {result.ErrorMessage}");
                    OnDailyQuizLoadFailed?.Invoke(result.ErrorMessage);
                }
            }
            catch (Exception ex)
            {
                _isDailyLoading = false;
                Debug.LogError($"[IVXDailyQuiz] Daily exception: {ex.Message}");
                OnDailyQuizLoadFailed?.Invoke(ex.Message);
            }
        }

        public void BeginDailyQuiz()
        {
            if (_dailyQuiz == null) { Debug.LogError("[IVXDailyQuiz] No daily quiz loaded"); return; }

            EnsureInitialized();
            _dailyQuestionIndex = 0;
            _dailyAnswerHistory.Clear();
            _dailyCorrectCount = 0;
            _dailyCurrentStreak = 0;
            _dailyBestStreak = 0;
            _isDailyActive = true;
            ShowDailyQuestion();
        }

        public void SubmitDailyAnswer(int selectedOption)
        {
            if (!_isDailyActive || _dailyQuiz == null) return;

            var question = GetCurrentDailyQuestion();
            if (question == null) return;

            bool isCorrect = selectedOption == question.correctAnswer;

            if (isCorrect)
            {
                _dailyCorrectCount++;
                _dailyCurrentStreak++;
                if (_dailyCurrentStreak > _dailyBestStreak) _dailyBestStreak = _dailyCurrentStreak;

                if (_dailyCurrentStreak > 1)
                    OnDailyStreakBonus?.Invoke(_dailyCurrentStreak);
            }
            else
            {
                if (_dailyCurrentStreak >= 3)
                    OnDailyStreakBroken?.Invoke($"Streak of {_dailyCurrentStreak} broken!");
                _dailyCurrentStreak = 0;
            }

            var record = new IVXDailyQuizAnswerRecord
            {
                questionId = question.questionId,
                selectedOption = selectedOption,
                isCorrect = isCorrect,
                answeredAt = DateTime.UtcNow
            };
            _dailyAnswerHistory.Add(record);

            Debug.Log($"[IVXDailyQuiz] Daily answer: {(isCorrect ? "CORRECT" : "WRONG")} | Streak: {_dailyCurrentStreak}");
            OnDailyAnswerSubmitted?.Invoke(question, selectedOption, isCorrect, _dailyCurrentStreak);
            SaveDailyProgress();
        }

        public void NextDailyQuestion()
        {
            _dailyQuestionIndex++;
            if (_dailyQuestionIndex >= _dailyQuiz.QuestionCount)
                EndDailyQuiz();
            else
                ShowDailyQuestion();
        }

        public IVXDailyQuestion GetCurrentDailyQuestion()
        {
            if (_dailyQuiz?.questions == null || _dailyQuestionIndex < 0 || _dailyQuestionIndex >= _dailyQuiz.questions.Count)
                return null;
            return _dailyQuiz.questions[_dailyQuestionIndex];
        }

        public void CancelDailyQuiz()
        {
            _isDailyActive = false;
            ClearDailyProgress();
            OnDailyQuizCancelled?.Invoke();
        }

        public bool HasCompletedDailyToday()
        {
            string key = kDailyCompletedKey + GetCurrentDateKey();
            return PlayerPrefs.GetInt(key, 0) == 1;
        }

        private void ShowDailyQuestion()
        {
            var question = GetCurrentDailyQuestion();
            if (question != null)
            {
                Debug.Log($"[IVXDailyQuiz] Daily Q{_dailyQuestionIndex + 1}/{_dailyQuiz.QuestionCount}");
                OnDailyQuestionDisplayed?.Invoke(question, _dailyQuestionIndex, _dailyQuiz.QuestionCount);
            }
        }

        private void EndDailyQuiz()
        {
            _isDailyActive = false;
            IVXDailyResult result = DetermineDailyResult();

            var summary = new IVXDailyQuizSummary
            {
                quizType = IVXDailyQuizType.Daily,
                quizId = _dailyQuiz.quizId,
                quizTitle = _dailyQuiz.SafeTitle,
                quizDate = _dailyQuiz.quizDate,
                totalQuestions = _dailyQuiz.QuestionCount,
                correctAnswers = _dailyCorrectCount,
                completedAt = DateTime.UtcNow,
                answerHistory = new List<IVXDailyQuizAnswerRecord>(_dailyAnswerHistory),
                isPremium = false,
                currentStreak = _dailyCurrentStreak,
                bestStreak = _dailyBestStreak
            };

            MarkDailyCompleted();
            UpdateConsecutiveDaysStreak(false);
            ClearDailyProgress();

            Debug.Log($"[IVXDailyQuiz] Daily completed! Correct: {_dailyCorrectCount}/{_dailyQuiz.QuestionCount}");
            OnDailyQuizCompleted?.Invoke(result, summary);
        }

        private IVXDailyResult DetermineDailyResult()
        {
            float accuracy = DailyTotalQuestions > 0 ? (float)_dailyCorrectCount / DailyTotalQuestions * 100f : 0;

            if (_dailyQuiz?.results != null && _dailyQuiz.results.Count > 0)
            {
                foreach (var r in _dailyQuiz.results)
                {
                    if (accuracy >= r.minScore && accuracy <= r.maxScore)
                        return r;
                }
                return _dailyQuiz.results[_dailyQuiz.results.Count - 1];
            }

            return GenerateDailyResultFromPerformance(accuracy);
        }

        private static IVXDailyResult GenerateDailyResultFromPerformance(float accuracy)
        {
            if (accuracy >= 100f) return new IVXDailyResult { id = "perfect", title = "Perfect Score!", description = "Amazing! You got them all!", emoji = "🏆" };
            if (accuracy >= 80f) return new IVXDailyResult { id = "excellent", title = "Excellent!", description = "Outstanding!", emoji = "🌟" };
            if (accuracy >= 60f) return new IVXDailyResult { id = "good", title = "Good Job!", description = "Well done!", emoji = "👍" };
            if (accuracy >= 40f) return new IVXDailyResult { id = "learning", title = "Keep Learning!", description = "You'll do better!", emoji = "📚" };
            return new IVXDailyResult { id = "beginner", title = "Keep Trying!", description = "Practice makes perfect!", emoji = "💪" };
        }

        private void SaveDailyProgress()
        {
            var progress = new IVXDailyQuizProgress
            {
                quizId = _dailyQuiz?.quizId,
                quizDate = _dailyQuiz?.quizDate,
                currentIndex = _dailyQuestionIndex,
                correctCount = _dailyCorrectCount,
                currentStreak = _dailyCurrentStreak,
                bestStreak = _dailyBestStreak,
                isPremium = false,
                savedAt = DateTime.UtcNow.ToString("O")
            };
            PlayerPrefs.SetString(kDailyProgressKey, JsonUtility.ToJson(progress));
            SchedulePrefsSave();
        }

        private static void ClearDailyProgress()
        {
            PlayerPrefs.DeleteKey(kDailyProgressKey);
            SchedulePrefsSave();
        }

        private static void MarkDailyCompleted()
        {
            string key = kDailyCompletedKey + GetCurrentDateKey();
            PlayerPrefs.SetInt(key, 1);
            SchedulePrefsSave();
        }

        #endregion

        #region Premium Quiz API

        public async void StartPremiumQuiz()
        {
            EnsureInitialized();
            if (_isPremiumLoading)
            {
                Debug.LogWarning("[IVXDailyQuiz] Premium already loading");
                return;
            }

            _isPremiumLoading = true;
            Debug.Log("[IVXDailyQuiz] Loading Premium quiz...");

            try
            {
                var result = await _service.FetchPremiumDailyQuizAsync();
                _isPremiumLoading = false;

                if (result.Success)
                {
                    _premiumQuiz = result.GetQuizData<IVXDailyQuizData>();
                    Debug.Log($"[IVXDailyQuiz] Premium loaded: {_premiumQuiz.SafeTitle} ({_premiumQuiz.QuestionCount}Q)");
                    OnPremiumQuizLoaded?.Invoke(_premiumQuiz);
                }
                else
                {
                    Debug.LogError($"[IVXDailyQuiz] Premium failed: {result.ErrorMessage}");
                    OnPremiumQuizLoadFailed?.Invoke(result.ErrorMessage);
                }
            }
            catch (Exception ex)
            {
                _isPremiumLoading = false;
                Debug.LogError($"[IVXDailyQuiz] Premium exception: {ex.Message}");
                OnPremiumQuizLoadFailed?.Invoke(ex.Message);
            }
        }

        public void BeginPremiumQuiz()
        {
            if (_premiumQuiz == null) { Debug.LogError("[IVXDailyQuiz] No premium quiz loaded"); return; }

            EnsureInitialized();
            _premiumQuestionIndex = 0;
            _premiumAnswerHistory.Clear();
            _premiumCorrectCount = 0;
            _premiumCurrentStreak = 0;
            _premiumBestStreak = 0;
            _isPremiumActive = true;
            ShowPremiumQuestion();
        }

        public void SubmitPremiumAnswer(int selectedOption)
        {
            if (!_isPremiumActive || _premiumQuiz == null) return;

            var question = GetCurrentPremiumQuestion();
            if (question == null) return;

            bool isCorrect = selectedOption == question.correctAnswer;

            if (isCorrect)
            {
                _premiumCorrectCount++;
                _premiumCurrentStreak++;
                if (_premiumCurrentStreak > _premiumBestStreak) _premiumBestStreak = _premiumCurrentStreak;

                if (_premiumCurrentStreak > 1)
                    OnPremiumStreakBonus?.Invoke(_premiumCurrentStreak);
            }
            else
            {
                if (_premiumCurrentStreak >= 3)
                    OnPremiumStreakBroken?.Invoke($"Streak of {_premiumCurrentStreak} broken!");
                _premiumCurrentStreak = 0;
            }

            var record = new IVXDailyQuizAnswerRecord
            {
                questionId = question.questionId,
                selectedOption = selectedOption,
                isCorrect = isCorrect,
                answeredAt = DateTime.UtcNow
            };
            _premiumAnswerHistory.Add(record);

            Debug.Log($"[IVXDailyQuiz] Premium answer: {(isCorrect ? "CORRECT" : "WRONG")} | Streak: {_premiumCurrentStreak}");
            OnPremiumAnswerSubmitted?.Invoke(question, selectedOption, isCorrect, _premiumCurrentStreak);
            SavePremiumProgress();
        }

        public void NextPremiumQuestion()
        {
            _premiumQuestionIndex++;
            if (_premiumQuestionIndex >= _premiumQuiz.QuestionCount)
                EndPremiumQuiz();
            else
                ShowPremiumQuestion();
        }

        public IVXDailyQuestion GetCurrentPremiumQuestion()
        {
            if (_premiumQuiz?.questions == null || _premiumQuestionIndex < 0 || _premiumQuestionIndex >= _premiumQuiz.questions.Count)
                return null;
            return _premiumQuiz.questions[_premiumQuestionIndex];
        }

        public void CancelPremiumQuiz()
        {
            _isPremiumActive = false;
            ClearPremiumProgress();
            OnPremiumQuizCancelled?.Invoke();
        }

        public bool HasCompletedPremiumToday()
        {
            string key = kPremiumCompletedKey + GetCurrentDateKey();
            return PlayerPrefs.GetInt(key, 0) == 1;
        }

        private void ShowPremiumQuestion()
        {
            var question = GetCurrentPremiumQuestion();
            if (question != null)
            {
                Debug.Log($"[IVXDailyQuiz] Premium Q{_premiumQuestionIndex + 1}/{_premiumQuiz.QuestionCount}");
                OnPremiumQuestionDisplayed?.Invoke(question, _premiumQuestionIndex, _premiumQuiz.QuestionCount);
            }
        }

        private void EndPremiumQuiz()
        {
            _isPremiumActive = false;
            IVXDailyResult result = DeterminePremiumResult();

            var summary = new IVXDailyQuizSummary
            {
                quizType = IVXDailyQuizType.DailyPremium,
                quizId = _premiumQuiz.quizId,
                quizTitle = _premiumQuiz.SafeTitle,
                quizDate = _premiumQuiz.quizDate,
                totalQuestions = _premiumQuiz.QuestionCount,
                correctAnswers = _premiumCorrectCount,
                completedAt = DateTime.UtcNow,
                answerHistory = new List<IVXDailyQuizAnswerRecord>(_premiumAnswerHistory),
                isPremium = true,
                currentStreak = _premiumCurrentStreak,
                bestStreak = _premiumBestStreak
            };

            MarkPremiumCompleted();
            UpdateConsecutiveDaysStreak(true);
            ClearPremiumProgress();

            Debug.Log($"[IVXDailyQuiz] Premium completed! Correct: {_premiumCorrectCount}/{_premiumQuiz.QuestionCount}");
            OnPremiumQuizCompleted?.Invoke(result, summary);
        }

        private IVXDailyResult DeterminePremiumResult()
        {
            float accuracy = PremiumTotalQuestions > 0 ? (float)_premiumCorrectCount / PremiumTotalQuestions * 100f : 0;

            if (_premiumQuiz?.results != null && _premiumQuiz.results.Count > 0)
            {
                foreach (var r in _premiumQuiz.results)
                {
                    if (accuracy >= r.minScore && accuracy <= r.maxScore)
                        return r;
                }
                return _premiumQuiz.results[_premiumQuiz.results.Count - 1];
            }

            return GeneratePremiumResultFromPerformance(accuracy);
        }

        private static IVXDailyResult GeneratePremiumResultFromPerformance(float accuracy)
        {
            if (accuracy >= 100f) return new IVXDailyResult { id = "master", title = "Quiz Master!", description = "Flawless!", emoji = "💎" };
            if (accuracy >= 80f) return new IVXDailyResult { id = "expert", title = "Expert Level!", description = "Impressive!", emoji = "🏅" };
            if (accuracy >= 60f) return new IVXDailyResult { id = "skilled", title = "Skilled Player!", description = "Great effort!", emoji = "⭐" };
            if (accuracy >= 40f) return new IVXDailyResult { id = "challenger", title = "Challenger!", description = "Premium is tough!", emoji = "🎯" };
            return new IVXDailyResult { id = "learner", title = "Premium Learner!", description = "Keep at it!", emoji = "📖" };
        }

        private void SavePremiumProgress()
        {
            var progress = new IVXDailyQuizProgress
            {
                quizId = _premiumQuiz?.quizId,
                quizDate = _premiumQuiz?.quizDate,
                currentIndex = _premiumQuestionIndex,
                correctCount = _premiumCorrectCount,
                currentStreak = _premiumCurrentStreak,
                bestStreak = _premiumBestStreak,
                isPremium = true,
                savedAt = DateTime.UtcNow.ToString("O")
            };
            PlayerPrefs.SetString(kPremiumProgressKey, JsonUtility.ToJson(progress));
            SchedulePrefsSave();
        }

        private static void ClearPremiumProgress()
        {
            PlayerPrefs.DeleteKey(kPremiumProgressKey);
            SchedulePrefsSave();
        }

        private static void MarkPremiumCompleted()
        {
            string key = kPremiumCompletedKey + GetCurrentDateKey();
            PlayerPrefs.SetInt(key, 1);
            SchedulePrefsSave();
        }

        #endregion

        #region Date Utilities

        private static string GetCurrentDateKey()
        {
            return DateTime.UtcNow.ToString("yyyy-MM-dd");
        }

        private void UpdateConsecutiveDaysStreak(bool isPremium)
        {
            string lastCompletedKey = isPremium ? kPremiumLastCompletedKey : kDailyLastCompletedKey;
            string streakKey = isPremium ? kPremiumStreakKey : kDailyStreakKey;

            string lastCompleted = PlayerPrefs.GetString(lastCompletedKey, "");
            string today = GetCurrentDateKey();

            if (lastCompleted == today)
            {
                return;
            }

            int currentStreak = PlayerPrefs.GetInt(streakKey, 0);

            if (!string.IsNullOrEmpty(lastCompleted))
            {
                if (DateTime.TryParse(lastCompleted, out DateTime lastDate))
                {
                    TimeSpan diff = DateTime.UtcNow.Date - lastDate.Date;

                    if (diff.TotalDays == 1)
                    {
                        currentStreak++;
                    }
                    else if (diff.TotalDays > 1)
                    {
                        currentStreak = 1;
                    }
                }
            }
            else
            {
                currentStreak = 1;
            }

            PlayerPrefs.SetInt(streakKey, currentStreak);
            PlayerPrefs.SetString(lastCompletedKey, today);
            SchedulePrefsSave();

            Debug.Log($"[IVXDailyQuiz] {(isPremium ? "Premium" : "Daily")} consecutive streak: {currentStreak} days");
        }

        public static TimeSpan GetTimeUntilDailyReset()
        {
            DateTime now = DateTime.UtcNow;
            DateTime nextMidnight = now.Date.AddDays(1);
            TimeSpan remaining = nextMidnight - now;
            return remaining.TotalSeconds > 0 ? remaining : TimeSpan.Zero;
        }

        #endregion

        #region PlayerPrefs Batching

        private static void SchedulePrefsSave()
        {
            if (_prefsSavePending) return;
            _prefsSavePending = true;
            if (Instance != null)
                Instance.StartCoroutine(FlushPrefsSaveCoroutine());
            else
                FlushPrefsSaveImmediate();
        }

        private static IEnumerator FlushPrefsSaveCoroutine()
        {
            yield return null;
            FlushPrefsSaveImmediate();
        }

        private static void FlushPrefsSaveImmediate()
        {
            if (!_prefsSavePending) return;
            _prefsSavePending = false;
            PlayerPrefs.Save();
        }

        #endregion

        #region Generic Helpers

        public bool HasCompletedToday(IVXDailyQuizType type)
        {
            return type switch
            {
                IVXDailyQuizType.Daily => HasCompletedDailyToday(),
                IVXDailyQuizType.DailyPremium => HasCompletedPremiumToday(),
                _ => false
            };
        }

        public static IVXDailyQuizTheme GetTheme(IVXDailyQuizType type) => IVXDailyQuizThemes.GetTheme(type);

        #endregion

        #region Debug

#if UNITY_EDITOR
        [ContextMenu("Debug: Reset All Daily Quizzes")]
        public void DebugResetAll()
        {
            string dateKey = GetCurrentDateKey();
            PlayerPrefs.DeleteKey(kDailyCompletedKey + dateKey);
            PlayerPrefs.DeleteKey(kDailyProgressKey);
            PlayerPrefs.DeleteKey(kDailyStreakKey);
            PlayerPrefs.DeleteKey(kDailyLastCompletedKey);
            PlayerPrefs.DeleteKey(kPremiumCompletedKey + dateKey);
            PlayerPrefs.DeleteKey(kPremiumProgressKey);
            PlayerPrefs.DeleteKey(kPremiumStreakKey);
            PlayerPrefs.DeleteKey(kPremiumLastCompletedKey);
            _service?.ClearCache();
            Debug.Log("[IVXDailyQuiz] All daily quizzes reset");
        }

        [ContextMenu("Debug: Print Status")]
        public void DebugPrintStatus()
        {
            Debug.Log($"[IVXDailyQuiz] Status (Date: {GetCurrentDateKey()}):\n" +
                      $"  Daily: loaded={_dailyQuiz != null}, active={_isDailyActive}, completed={HasCompletedDailyToday()}, streak={ConsecutiveDaysStreak}\n" +
                      $"  Premium: loaded={_premiumQuiz != null}, active={_isPremiumActive}, completed={HasCompletedPremiumToday()}, streak={PremiumConsecutiveDaysStreak}\n" +
                      $"  Time until reset: {GetTimeUntilDailyReset():hh\\hmm\\mss\\s}");
        }

        [ContextMenu("Debug: Log URL Patterns")]
        public void DebugLogUrlPatterns()
        {
            _service?.LogUrlPatternForDays(IVXDailyQuizType.Daily, 7);
            _service?.LogUrlPatternForDays(IVXDailyQuizType.DailyPremium, 7);
        }
#endif

        #endregion
    }
}
