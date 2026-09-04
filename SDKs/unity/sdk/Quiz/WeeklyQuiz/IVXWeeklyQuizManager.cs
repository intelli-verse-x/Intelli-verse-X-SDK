// IVXWeeklyQuizManager.cs
// Unified manager for all weekly quiz types in IntelliVerse-X SDK

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using IntelliVerseX.Core;

namespace IntelliVerseX.Quiz.WeeklyQuiz
{
    /// <summary>
    /// Unified manager for ALL weekly quiz types: Fortune 🔮, Emoji 🎉, Prediction ⚽, Health 💧.
    /// Singleton - lives on a DontDestroyOnLoad GameObject.
    /// </summary>
    public class IVXWeeklyQuizManager : MonoBehaviour
    {
        #region Singleton

        public static IVXWeeklyQuizManager Instance { get; private set; }

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

        #region Events - Fortune

        public event Action<IVXFortuneQuizData> OnFortuneQuizLoaded;
        public event Action<string> OnFortuneQuizLoadFailed;
        public event Action<IVXFortuneQuestion, int> OnFortuneQuestionDisplayed;
        public event Action<IVXFortuneQuestion, int, List<string>> OnFortuneAnswerSubmitted;
        public event Action<IVXFortuneResult, IVXWeeklyQuizSummary> OnFortuneQuizCompleted;
        public event Action OnFortuneQuizCancelled;

        #endregion

        #region Events - Emoji

        public event Action<IVXEmojiQuizData> OnEmojiQuizLoaded;
        public event Action<string> OnEmojiQuizLoadFailed;
        public event Action<IVXEmojiQuestion, int, int> OnEmojiQuestionDisplayed;
        public event Action<IVXEmojiQuestion, int, bool, int> OnEmojiAnswerSubmitted;
        public event Action<int> OnEmojiStreakBonus;
        public event Action<string> OnEmojiStreakBroken;
        public event Action<IVXEmojiResult, IVXWeeklyQuizSummary> OnEmojiQuizCompleted;
        public event Action OnEmojiQuizCancelled;

        #endregion

        #region Events - Prediction

        public event Action<IVXPredictionQuizData> OnPredictionQuizLoaded;
        public event Action<string> OnPredictionQuizLoadFailed;
        public event Action<IVXPredictionQuestion, int, int> OnPredictionQuestionDisplayed;
        public event Action<IVXPredictionQuestion, int, Dictionary<string, float>> OnPredictionSubmitted;
        public event Action<IVXWeeklyQuizSummary> OnPredictionQuizCompleted;
        public event Action OnPredictionQuizCancelled;

        #endregion

        #region Events - Health

        public event Action<IVXHealthQuizData> OnHealthQuizLoaded;
        public event Action<string> OnHealthQuizLoadFailed;
        public event Action<IVXHealthQuestion, int, int> OnHealthQuestionDisplayed;
        public event Action<IVXHealthQuestion, int, bool, int> OnHealthAnswerSubmitted;
        public event Action<int> OnHealthStreakBonus;
        public event Action<string> OnHealthStreakBroken;
        public event Action<IVXHealthResult, IVXWeeklyQuizSummary> OnHealthQuizCompleted;
        public event Action OnHealthQuizCancelled;

        #endregion

        #region State - Fortune

        private IVXFortuneQuizData _fortuneQuiz;
        private int _fortuneQuestionIndex;
        private List<IVXWeeklyQuizAnswerRecord> _fortuneAnswerHistory;
        private Dictionary<string, int> _fortuneTraitCounts;
        private bool _isFortuneLoading;
        private bool _isFortuneActive;

        private const string kFortuneCompletedKey = "IVX_FortuneQuiz_CompletedWeek_";
        private const string kFortuneProgressKey = "IVX_FortuneQuiz_Progress";

        public IVXFortuneQuizData CurrentFortuneQuiz => _fortuneQuiz;
        public int FortuneCurrentQuestionIndex => _fortuneQuestionIndex;
        public int FortuneTotalQuestions => _fortuneQuiz?.QuestionCount ?? 0;
        public bool IsFortuneQuizActive => _isFortuneActive;
        public bool IsFortuneLoading => _isFortuneLoading;

        #endregion

        #region State - Emoji

        private IVXEmojiQuizData _emojiQuiz;
        private int _emojiQuestionIndex;
        private List<IVXWeeklyQuizAnswerRecord> _emojiAnswerHistory;
        private int _emojiCorrectCount;
        private int _emojiCurrentStreak;
        private int _emojiBestStreak;
        private bool _isEmojiLoading;
        private bool _isEmojiActive;

        private const string kEmojiCompletedKey = "IVX_EmojiQuiz_CompletedWeek_";
        private const string kEmojiProgressKey = "IVX_EmojiQuiz_Progress";

        public IVXEmojiQuizData CurrentEmojiQuiz => _emojiQuiz;
        public int EmojiCurrentQuestionIndex => _emojiQuestionIndex;
        public int EmojiTotalQuestions => _emojiQuiz?.QuestionCount ?? 0;
        public int EmojiCorrectCount => _emojiCorrectCount;
        public int EmojiCurrentStreak => _emojiCurrentStreak;
        public int EmojiBestStreak => _emojiBestStreak;
        public bool IsEmojiQuizActive => _isEmojiActive;
        public bool IsEmojiLoading => _isEmojiLoading;

        #endregion

        #region State - Prediction

        private IVXPredictionQuizData _predictionQuiz;
        private int _predictionQuestionIndex;
        private List<IVXPredictionSubmission> _predictionSubmissions;
        private bool _isPredictionLoading;
        private bool _isPredictionActive;
        private bool _predictionResultsRevealed;

        private const string kPredictionSubmittedKey = "IVX_PredictionQuiz_Submitted_";
        private const string kPredictionAnswersKey = "IVX_PredictionQuiz_Answers_";

        public IVXPredictionQuizData CurrentPredictionQuiz => _predictionQuiz;
        public int PredictionCurrentQuestionIndex => _predictionQuestionIndex;
        public int PredictionTotalQuestions => _predictionQuiz?.QuestionCount ?? 0;
        public bool IsPredictionQuizActive => _isPredictionActive;
        public bool IsPredictionLoading => _isPredictionLoading;
        public bool ArePredictionResultsRevealed => _predictionResultsRevealed;

        #endregion

        #region State - Health

        private IVXHealthQuizData _healthQuiz;
        private int _healthQuestionIndex;
        private List<IVXWeeklyQuizAnswerRecord> _healthAnswerHistory;
        private int _healthCorrectCount;
        private int _healthCurrentStreak;
        private int _healthBestStreak;
        private bool _isHealthLoading;
        private bool _isHealthActive;

        private const string kHealthCompletedKey = "IVX_HealthQuiz_CompletedWeek_";
        private const string kHealthProgressKey = "IVX_HealthQuiz_Progress";

        public IVXHealthQuizData CurrentHealthQuiz => _healthQuiz;
        public int HealthCurrentQuestionIndex => _healthQuestionIndex;
        public int HealthTotalQuestions => _healthQuiz?.QuestionCount ?? 0;
        public int HealthCorrectCount => _healthCorrectCount;
        public int HealthCurrentStreak => _healthCurrentStreak;
        public int HealthBestStreak => _healthBestStreak;
        public bool IsHealthQuizActive => _isHealthActive;
        public bool IsHealthLoading => _isHealthLoading;

        #endregion

        #region Shared

        private IVXWeeklyQuizService _service;
        private static bool _prefsSavePending;

        public bool IsAnyQuizActive => _isFortuneActive || _isEmojiActive || _isPredictionActive || _isHealthActive;

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
            _service ??= new IVXWeeklyQuizService();
            _fortuneAnswerHistory ??= new List<IVXWeeklyQuizAnswerRecord>();
            _fortuneTraitCounts ??= new Dictionary<string, int>();
            _emojiAnswerHistory ??= new List<IVXWeeklyQuizAnswerRecord>();
            _predictionSubmissions ??= new List<IVXPredictionSubmission>();
            _healthAnswerHistory ??= new List<IVXWeeklyQuizAnswerRecord>();
        }

        #endregion

        #region Fortune API

        public async void StartFortuneQuiz()
        {
            EnsureInitialized();
            if (_isFortuneLoading)
            {
                Debug.LogWarning("[IVXWeeklyQuiz] Fortune already loading");
                return;
            }

            _isFortuneLoading = true;
            Debug.Log("[IVXWeeklyQuiz] Loading Fortune quiz...");

            try
            {
                var result = await _service.FetchFortuneQuizAsync();
                _isFortuneLoading = false;

                if (result.Success)
                {
                    _fortuneQuiz = result.GetQuizData<IVXFortuneQuizData>();
                    Debug.Log($"[IVXWeeklyQuiz] Fortune loaded: {_fortuneQuiz.title} ({_fortuneQuiz.QuestionCount}Q)");
                    OnFortuneQuizLoaded?.Invoke(_fortuneQuiz);
                }
                else
                {
                    Debug.LogError($"[IVXWeeklyQuiz] Fortune failed: {result.ErrorMessage}");
                    OnFortuneQuizLoadFailed?.Invoke(result.ErrorMessage);
                }
            }
            catch (Exception ex)
            {
                _isFortuneLoading = false;
                Debug.LogError($"[IVXWeeklyQuiz] Fortune exception: {ex.Message}");
                OnFortuneQuizLoadFailed?.Invoke(ex.Message);
            }
        }

        public void BeginFortuneQuiz()
        {
            if (_fortuneQuiz == null) { Debug.LogError("[IVXWeeklyQuiz] No fortune quiz loaded"); return; }

            _fortuneQuestionIndex = 0;
            _fortuneAnswerHistory.Clear();
            _fortuneTraitCounts.Clear();
            _isFortuneActive = true;
            ShowFortuneQuestion();
        }

        public void SubmitFortuneAnswer(int selectedOption)
        {
            if (!_isFortuneActive || _fortuneQuiz == null) return;

            var question = GetCurrentFortuneQuestion();
            if (question == null) return;

            List<string> earnedTraits = GetTraitsForOption(question, selectedOption);

            var record = new IVXWeeklyQuizAnswerRecord
            {
                questionId = question.id,
                selectedOption = selectedOption,
                earnedTraits = earnedTraits,
                answeredAt = DateTime.UtcNow
            };
            _fortuneAnswerHistory.Add(record);

            foreach (string trait in earnedTraits)
            {
                if (!_fortuneTraitCounts.ContainsKey(trait))
                    _fortuneTraitCounts[trait] = 0;
                _fortuneTraitCounts[trait]++;
            }

            Debug.Log($"[IVXWeeklyQuiz] Fortune answer: Option {selectedOption}, Traits: {string.Join(", ", earnedTraits)}");
            OnFortuneAnswerSubmitted?.Invoke(question, selectedOption, earnedTraits);
            SaveFortuneProgress();
        }

        public void NextFortuneQuestion()
        {
            _fortuneQuestionIndex++;
            if (_fortuneQuestionIndex >= _fortuneQuiz.QuestionCount)
                EndFortuneQuiz();
            else
                ShowFortuneQuestion();
        }

        public IVXFortuneQuestion GetCurrentFortuneQuestion()
        {
            if (_fortuneQuiz?.questions == null || _fortuneQuestionIndex < 0 || _fortuneQuestionIndex >= _fortuneQuiz.questions.Count)
                return null;
            return _fortuneQuiz.questions[_fortuneQuestionIndex];
        }

        public void CancelFortuneQuiz()
        {
            _isFortuneActive = false;
            ClearFortuneProgress();
            OnFortuneQuizCancelled?.Invoke();
        }

        public bool HasCompletedFortuneThisWeek()
        {
            string key = kFortuneCompletedKey + GetCurrentWeekKey();
            return PlayerPrefs.GetInt(key, 0) == 1;
        }

        private void ShowFortuneQuestion()
        {
            var question = GetCurrentFortuneQuestion();
            if (question != null)
            {
                Debug.Log($"[IVXWeeklyQuiz] Fortune Q{_fortuneQuestionIndex + 1}/{_fortuneQuiz.QuestionCount}");
                OnFortuneQuestionDisplayed?.Invoke(question, _fortuneQuestionIndex);
            }
        }

        private static List<string> GetTraitsForOption(IVXFortuneQuestion question, int optionIndex)
        {
            if (question.traitMapping == null || question.options == null) return new List<string>();
            if (optionIndex < 0 || optionIndex >= question.OptionCount) return new List<string>();

            string optionId = question.GetOptionId(optionIndex);
            string optionText = question.GetOptionText(optionIndex);

            if (!string.IsNullOrEmpty(optionId) && question.traitMapping.TryGetValue(optionId, out var traitsById))
                return new List<string>(traitsById);

            if (question.traitMapping.TryGetValue(optionText, out var traitsByText))
                return new List<string>(traitsByText);

            return new List<string>();
        }

        private void EndFortuneQuiz()
        {
            _isFortuneActive = false;
            IVXFortuneResult result = DetermineFortuneResult();

            var summary = new IVXWeeklyQuizSummary
            {
                quizType = IVXWeeklyQuizType.Fortune,
                quizId = _fortuneQuiz.quizId,
                quizTitle = _fortuneQuiz.title,
                totalQuestions = _fortuneQuiz.QuestionCount,
                completedAt = DateTime.UtcNow,
                answerHistory = new List<IVXWeeklyQuizAnswerRecord>(_fortuneAnswerHistory),
                fortuneResult = result,
                traitCounts = new Dictionary<string, int>(_fortuneTraitCounts)
            };

            MarkFortuneCompleted();
            ClearFortuneProgress();

            Debug.Log($"[IVXWeeklyQuiz] Fortune completed! Result: {result?.title}");
            OnFortuneQuizCompleted?.Invoke(result, summary);
        }

        private IVXFortuneResult DetermineFortuneResult()
        {
            if (_fortuneQuiz?.results == null || _fortuneQuiz.results.Count == 0)
                return GetDefaultFortuneResult();

            IVXFortuneResult bestMatch = null;
            int bestScore = 0;

            foreach (var r in _fortuneQuiz.results)
            {
                if (r.traits == null) continue;

                int score = 0;
                foreach (var trait in r.traits)
                {
                    if (_fortuneTraitCounts.TryGetValue(trait, out int count))
                        score += count;
                }

                if (score > bestScore) { bestScore = score; bestMatch = r; }
            }

            return bestMatch ?? _fortuneQuiz.results[0];
        }

        private static IVXFortuneResult GetDefaultFortuneResult()
        {
            return new IVXFortuneResult
            {
                id = "default",
                title = "Path Seeker",
                description = "Your journey is unique and full of potential!",
                emoji = "✨",
                luckyNumber = UnityEngine.Random.Range(1, 100),
                luckyColor = "Purple"
            };
        }

        private void SaveFortuneProgress()
        {
            var progress = new IVXFortuneQuizProgress
            {
                quizId = _fortuneQuiz?.quizId,
                currentIndex = _fortuneQuestionIndex,
                answers = _fortuneAnswerHistory.Select(a => a.selectedOption).ToList(),
                traitCounts = new Dictionary<string, int>(_fortuneTraitCounts),
                savedAt = DateTime.UtcNow.ToString("O")
            };
            PlayerPrefs.SetString(kFortuneProgressKey, JsonUtility.ToJson(progress));
            SchedulePrefsSave();
        }

        private static void ClearFortuneProgress()
        {
            PlayerPrefs.DeleteKey(kFortuneProgressKey);
            SchedulePrefsSave();
        }

        private static void MarkFortuneCompleted()
        {
            string key = kFortuneCompletedKey + GetCurrentWeekKey();
            PlayerPrefs.SetInt(key, 1);
            SchedulePrefsSave();
        }

        #endregion

        #region Emoji API

        public async void StartEmojiQuiz()
        {
            EnsureInitialized();
            if (_isEmojiLoading)
            {
                Debug.LogWarning("[IVXWeeklyQuiz] Emoji already loading");
                return;
            }

            _isEmojiLoading = true;
            Debug.Log("[IVXWeeklyQuiz] Loading Emoji quiz...");

            try
            {
                var result = await _service.FetchEmojiQuizAsync();
                _isEmojiLoading = false;

                if (result.Success)
                {
                    _emojiQuiz = result.GetQuizData<IVXEmojiQuizData>();
                    Debug.Log($"[IVXWeeklyQuiz] Emoji loaded: {_emojiQuiz.title} ({_emojiQuiz.QuestionCount}Q)");
                    OnEmojiQuizLoaded?.Invoke(_emojiQuiz);
                }
                else
                {
                    Debug.LogError($"[IVXWeeklyQuiz] Emoji failed: {result.ErrorMessage}");
                    OnEmojiQuizLoadFailed?.Invoke(result.ErrorMessage);
                }
            }
            catch (Exception ex)
            {
                _isEmojiLoading = false;
                Debug.LogError($"[IVXWeeklyQuiz] Emoji exception: {ex.Message}");
                OnEmojiQuizLoadFailed?.Invoke(ex.Message);
            }
        }

        public void BeginEmojiQuiz()
        {
            if (_emojiQuiz == null) { Debug.LogError("[IVXWeeklyQuiz] No emoji quiz loaded"); return; }

            EnsureInitialized();
            _emojiQuestionIndex = 0;
            _emojiAnswerHistory.Clear();
            _emojiCorrectCount = 0;
            _emojiCurrentStreak = 0;
            _emojiBestStreak = 0;
            _isEmojiActive = true;
            ShowEmojiQuestion();
        }

        public void SubmitEmojiAnswer(int selectedOption)
        {
            if (!_isEmojiActive || _emojiQuiz == null) return;

            var question = GetCurrentEmojiQuestion();
            if (question == null) return;

            bool isCorrect = selectedOption == question.correctAnswer;

            if (isCorrect)
            {
                _emojiCorrectCount++;
                _emojiCurrentStreak++;
                if (_emojiCurrentStreak > _emojiBestStreak) _emojiBestStreak = _emojiCurrentStreak;

                if (_emojiCurrentStreak > 1)
                    OnEmojiStreakBonus?.Invoke(_emojiCurrentStreak);
            }
            else
            {
                if (_emojiCurrentStreak >= 3)
                    OnEmojiStreakBroken?.Invoke($"Streak of {_emojiCurrentStreak} broken!");
                _emojiCurrentStreak = 0;
            }

            var record = new IVXWeeklyQuizAnswerRecord
            {
                questionId = question.id,
                selectedOption = selectedOption,
                isCorrect = isCorrect,
                answeredAt = DateTime.UtcNow
            };
            _emojiAnswerHistory.Add(record);

            Debug.Log($"[IVXWeeklyQuiz] Emoji answer: {(isCorrect ? "CORRECT" : "WRONG")} | Streak: {_emojiCurrentStreak}");
            OnEmojiAnswerSubmitted?.Invoke(question, selectedOption, isCorrect, _emojiCurrentStreak);
            SaveEmojiProgress();
        }

        public void NextEmojiQuestion()
        {
            _emojiQuestionIndex++;
            if (_emojiQuestionIndex >= _emojiQuiz.QuestionCount)
                EndEmojiQuiz();
            else
                ShowEmojiQuestion();
        }

        public IVXEmojiQuestion GetCurrentEmojiQuestion()
        {
            if (_emojiQuiz?.questions == null || _emojiQuestionIndex < 0 || _emojiQuestionIndex >= _emojiQuiz.questions.Count)
                return null;
            return _emojiQuiz.questions[_emojiQuestionIndex];
        }

        public void CancelEmojiQuiz()
        {
            _isEmojiActive = false;
            ClearEmojiProgress();
            OnEmojiQuizCancelled?.Invoke();
        }

        public bool HasCompletedEmojiThisWeek()
        {
            string key = kEmojiCompletedKey + GetCurrentWeekKey();
            return PlayerPrefs.GetInt(key, 0) == 1;
        }

        private void ShowEmojiQuestion()
        {
            var question = GetCurrentEmojiQuestion();
            if (question != null)
            {
                Debug.Log($"[IVXWeeklyQuiz] Emoji Q{_emojiQuestionIndex + 1}/{_emojiQuiz.QuestionCount}");
                OnEmojiQuestionDisplayed?.Invoke(question, _emojiQuestionIndex, _emojiQuiz.QuestionCount);
            }
        }

        private void EndEmojiQuiz()
        {
            _isEmojiActive = false;
            IVXEmojiResult result = DetermineEmojiResult();

            var summary = new IVXWeeklyQuizSummary
            {
                quizType = IVXWeeklyQuizType.Emoji,
                quizId = _emojiQuiz.quizId,
                quizTitle = _emojiQuiz.title,
                totalQuestions = _emojiQuiz.QuestionCount,
                correctAnswers = _emojiCorrectCount,
                completedAt = DateTime.UtcNow,
                answerHistory = new List<IVXWeeklyQuizAnswerRecord>(_emojiAnswerHistory)
            };

            MarkEmojiCompleted();
            ClearEmojiProgress();

            Debug.Log($"[IVXWeeklyQuiz] Emoji completed! Correct: {_emojiCorrectCount}/{_emojiQuiz.QuestionCount}");
            OnEmojiQuizCompleted?.Invoke(result, summary);
        }

        private IVXEmojiResult DetermineEmojiResult()
        {
            float accuracy = EmojiTotalQuestions > 0 ? (float)_emojiCorrectCount / EmojiTotalQuestions : 0;

            if (_emojiQuiz?.results != null && _emojiQuiz.results.Count > 0)
            {
                int resultIndex = Mathf.Clamp(Mathf.FloorToInt(accuracy * _emojiQuiz.results.Count), 0, _emojiQuiz.results.Count - 1);
                return _emojiQuiz.results[resultIndex];
            }

            return GenerateEmojiResultFromPerformance(accuracy);
        }

        private static IVXEmojiResult GenerateEmojiResultFromPerformance(float accuracy)
        {
            if (accuracy >= 1.0f) return new IVXEmojiResult { id = "perfect", title = "Emoji Genius!", description = "PERFECT SCORE!", emoji = "🏆" };
            if (accuracy >= 0.8f) return new IVXEmojiResult { id = "excellent", title = "Emoji Expert", description = "Amazing!", emoji = "🌟" };
            if (accuracy >= 0.6f) return new IVXEmojiResult { id = "good", title = "Emoji Fan", description = "Great job!", emoji = "😊" };
            if (accuracy >= 0.4f) return new IVXEmojiResult { id = "learning", title = "Emoji Learner", description = "Keep practicing!", emoji = "🤔" };
            return new IVXEmojiResult { id = "beginner", title = "Emoji Newbie", description = "Keep trying!", emoji = "📱" };
        }

        private void SaveEmojiProgress()
        {
            var progress = new IVXEmojiQuizProgress
            {
                quizId = _emojiQuiz?.quizId,
                currentIndex = _emojiQuestionIndex,
                correctCount = _emojiCorrectCount,
                currentStreak = _emojiCurrentStreak,
                bestStreak = _emojiBestStreak,
                savedAt = DateTime.UtcNow.ToString("O")
            };
            PlayerPrefs.SetString(kEmojiProgressKey, JsonUtility.ToJson(progress));
            SchedulePrefsSave();
        }

        private static void ClearEmojiProgress()
        {
            PlayerPrefs.DeleteKey(kEmojiProgressKey);
            SchedulePrefsSave();
        }

        private static void MarkEmojiCompleted()
        {
            string key = kEmojiCompletedKey + GetCurrentWeekKey();
            PlayerPrefs.SetInt(key, 1);
            SchedulePrefsSave();
        }

        #endregion

        #region Health API

        public async void StartHealthQuiz()
        {
            EnsureInitialized();
            if (_isHealthLoading)
            {
                Debug.LogWarning("[IVXWeeklyQuiz] Health already loading");
                return;
            }

            _isHealthLoading = true;
            Debug.Log("[IVXWeeklyQuiz] Loading Health quiz...");

            try
            {
                var result = await _service.FetchHealthQuizAsync();
                _isHealthLoading = false;

                if (result.Success)
                {
                    _healthQuiz = result.GetQuizData<IVXHealthQuizData>();
                    Debug.Log($"[IVXWeeklyQuiz] Health loaded: {_healthQuiz.title} ({_healthQuiz.QuestionCount}Q)");
                    OnHealthQuizLoaded?.Invoke(_healthQuiz);
                }
                else
                {
                    Debug.LogError($"[IVXWeeklyQuiz] Health failed: {result.ErrorMessage}");
                    OnHealthQuizLoadFailed?.Invoke(result.ErrorMessage);
                }
            }
            catch (Exception ex)
            {
                _isHealthLoading = false;
                Debug.LogError($"[IVXWeeklyQuiz] Health exception: {ex.Message}");
                OnHealthQuizLoadFailed?.Invoke(ex.Message);
            }
        }

        public void BeginHealthQuiz()
        {
            if (_healthQuiz == null) { Debug.LogError("[IVXWeeklyQuiz] No health quiz loaded"); return; }

            EnsureInitialized();
            _healthQuestionIndex = 0;
            _healthAnswerHistory.Clear();
            _healthCorrectCount = 0;
            _healthCurrentStreak = 0;
            _healthBestStreak = 0;
            _isHealthActive = true;
            ShowHealthQuestion();
        }

        public void SubmitHealthAnswer(int selectedOption)
        {
            if (!_isHealthActive || _healthQuiz == null) return;

            var question = GetCurrentHealthQuestion();
            if (question == null) return;

            bool isCorrect = selectedOption == question.correctAnswer;

            if (isCorrect)
            {
                _healthCorrectCount++;
                _healthCurrentStreak++;
                if (_healthCurrentStreak > _healthBestStreak) _healthBestStreak = _healthCurrentStreak;

                if (_healthCurrentStreak > 1)
                    OnHealthStreakBonus?.Invoke(_healthCurrentStreak);
            }
            else
            {
                if (_healthCurrentStreak >= 3)
                    OnHealthStreakBroken?.Invoke($"Streak of {_healthCurrentStreak} broken!");
                _healthCurrentStreak = 0;
            }

            var record = new IVXWeeklyQuizAnswerRecord
            {
                questionId = question.id,
                selectedOption = selectedOption,
                isCorrect = isCorrect,
                answeredAt = DateTime.UtcNow
            };
            _healthAnswerHistory.Add(record);

            Debug.Log($"[IVXWeeklyQuiz] Health answer: {(isCorrect ? "CORRECT" : "WRONG")} | Streak: {_healthCurrentStreak}");
            OnHealthAnswerSubmitted?.Invoke(question, selectedOption, isCorrect, _healthCurrentStreak);
            SaveHealthProgress();
        }

        public void NextHealthQuestion()
        {
            _healthQuestionIndex++;
            if (_healthQuestionIndex >= _healthQuiz.QuestionCount)
                EndHealthQuiz();
            else
                ShowHealthQuestion();
        }

        public IVXHealthQuestion GetCurrentHealthQuestion()
        {
            if (_healthQuiz?.questions == null || _healthQuestionIndex < 0 || _healthQuestionIndex >= _healthQuiz.questions.Count)
                return null;
            return _healthQuiz.questions[_healthQuestionIndex];
        }

        public void CancelHealthQuiz()
        {
            _isHealthActive = false;
            ClearHealthProgress();
            OnHealthQuizCancelled?.Invoke();
        }

        public bool HasCompletedHealthThisWeek()
        {
            string key = kHealthCompletedKey + GetCurrentWeekKey();
            return PlayerPrefs.GetInt(key, 0) == 1;
        }

        private void ShowHealthQuestion()
        {
            var question = GetCurrentHealthQuestion();
            if (question != null)
            {
                Debug.Log($"[IVXWeeklyQuiz] Health Q{_healthQuestionIndex + 1}/{_healthQuiz.QuestionCount}");
                OnHealthQuestionDisplayed?.Invoke(question, _healthQuestionIndex, _healthQuiz.QuestionCount);
            }
        }

        private void EndHealthQuiz()
        {
            _isHealthActive = false;
            IVXHealthResult result = DetermineHealthResult();

            var summary = new IVXWeeklyQuizSummary
            {
                quizType = IVXWeeklyQuizType.Health,
                quizId = _healthQuiz.quizId,
                quizTitle = _healthQuiz.title,
                totalQuestions = _healthQuiz.QuestionCount,
                correctAnswers = _healthCorrectCount,
                completedAt = DateTime.UtcNow,
                answerHistory = new List<IVXWeeklyQuizAnswerRecord>(_healthAnswerHistory)
            };

            MarkHealthCompleted();
            ClearHealthProgress();

            Debug.Log($"[IVXWeeklyQuiz] Health completed! Correct: {_healthCorrectCount}/{_healthQuiz.QuestionCount}");
            OnHealthQuizCompleted?.Invoke(result, summary);
        }

        private IVXHealthResult DetermineHealthResult()
        {
            float accuracy = HealthTotalQuestions > 0 ? (float)_healthCorrectCount / HealthTotalQuestions : 0;

            if (_healthQuiz?.results != null && _healthQuiz.results.Count > 0)
            {
                int resultIndex = Mathf.Clamp(Mathf.FloorToInt(accuracy * _healthQuiz.results.Count), 0, _healthQuiz.results.Count - 1);
                return _healthQuiz.results[resultIndex];
            }

            return GenerateHealthResultFromPerformance(accuracy);
        }

        private static IVXHealthResult GenerateHealthResultFromPerformance(float accuracy)
        {
            if (accuracy >= 1.0f) return new IVXHealthResult { id = "perfect", title = "Health Guru!", description = "Perfect score!", emoji = "🏆" };
            if (accuracy >= 0.8f) return new IVXHealthResult { id = "excellent", title = "Health Expert", description = "Impressive!", emoji = "🌟" };
            if (accuracy >= 0.6f) return new IVXHealthResult { id = "good", title = "Health Aware", description = "Good job!", emoji = "💪" };
            if (accuracy >= 0.4f) return new IVXHealthResult { id = "learning", title = "Health Learner", description = "Keep learning!", emoji = "📚" };
            return new IVXHealthResult { id = "beginner", title = "Health Starter", description = "Start your journey!", emoji = "🌱" };
        }

        private void SaveHealthProgress()
        {
            var progress = new IVXHealthQuizProgress
            {
                quizId = _healthQuiz?.quizId,
                currentIndex = _healthQuestionIndex,
                correctCount = _healthCorrectCount,
                currentStreak = _healthCurrentStreak,
                bestStreak = _healthBestStreak,
                savedAt = DateTime.UtcNow.ToString("O")
            };
            PlayerPrefs.SetString(kHealthProgressKey, JsonUtility.ToJson(progress));
            SchedulePrefsSave();
        }

        private static void ClearHealthProgress()
        {
            PlayerPrefs.DeleteKey(kHealthProgressKey);
            SchedulePrefsSave();
        }

        private static void MarkHealthCompleted()
        {
            string key = kHealthCompletedKey + GetCurrentWeekKey();
            PlayerPrefs.SetInt(key, 1);
            SchedulePrefsSave();
        }

        #endregion

        #region Week Utilities

        private static string GetCurrentWeekKey()
        {
            DateTime now = DateTime.UtcNow;
            int weekNum = GetISOWeekNumber(now);
            int year = GetISOWeekYear(now);
            return $"{year}-W{weekNum:D2}";
        }

        private static int GetISOWeekNumber(DateTime date)
        {
            int isoDayOfWeek = date.DayOfWeek == DayOfWeek.Sunday ? 7 : (int)date.DayOfWeek;
            DateTime thursday = date.AddDays(4 - isoDayOfWeek);
            int isoYear = thursday.Year;

            DateTime jan4 = new DateTime(isoYear, 1, 4);
            int jan4Dow = jan4.DayOfWeek == DayOfWeek.Sunday ? 7 : (int)jan4.DayOfWeek;
            DateTime startOfWeek1 = jan4.AddDays(1 - jan4Dow);

            return (thursday - startOfWeek1).Days / 7 + 1;
        }

        private static int GetISOWeekYear(DateTime date)
        {
            int isoDayOfWeek = date.DayOfWeek == DayOfWeek.Sunday ? 7 : (int)date.DayOfWeek;
            DateTime thursday = date.AddDays(4 - isoDayOfWeek);
            return thursday.Year;
        }

        public static DateTime GetCurrentWeekStart()
        {
            DateTime now = DateTime.UtcNow.Date;
            int isoDayOfWeek = now.DayOfWeek == DayOfWeek.Sunday ? 7 : (int)now.DayOfWeek;
            return now.AddDays(1 - isoDayOfWeek);
        }

        public static DateTime GetCurrentWeekEnd()
        {
            return GetCurrentWeekStart().AddDays(7).AddSeconds(-1);
        }

        public static TimeSpan GetTimeUntilWeeklyReset()
        {
            DateTime weekEnd = GetCurrentWeekEnd();
            TimeSpan remaining = weekEnd - DateTime.UtcNow;
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

        public bool HasCompletedThisWeek(IVXWeeklyQuizType type)
        {
            return type switch
            {
                IVXWeeklyQuizType.Fortune => HasCompletedFortuneThisWeek(),
                IVXWeeklyQuizType.Emoji => HasCompletedEmojiThisWeek(),
                IVXWeeklyQuizType.Prediction => HasSubmittedPredictions(),
                IVXWeeklyQuizType.Health => HasCompletedHealthThisWeek(),
                _ => false
            };
        }

        public bool HasSubmittedPredictions()
        {
            if (_predictionQuiz == null) return false;
            return PlayerPrefs.GetInt(kPredictionSubmittedKey + _predictionQuiz.quizId, 0) == 1;
        }

        public static IVXWeeklyQuizTheme GetTheme(IVXWeeklyQuizType type) => IVXWeeklyQuizThemes.GetTheme(type);

        #endregion

        #region Debug

#if UNITY_EDITOR
        [ContextMenu("Debug: Reset All Weekly Quizzes")]
        public void DebugResetAll()
        {
            PlayerPrefs.DeleteKey(kFortuneCompletedKey + GetCurrentWeekKey());
            PlayerPrefs.DeleteKey(kFortuneProgressKey);
            PlayerPrefs.DeleteKey(kEmojiCompletedKey + GetCurrentWeekKey());
            PlayerPrefs.DeleteKey(kEmojiProgressKey);
            PlayerPrefs.DeleteKey(kHealthCompletedKey + GetCurrentWeekKey());
            PlayerPrefs.DeleteKey(kHealthProgressKey);
            _service?.ClearCache();
            Debug.Log("[IVXWeeklyQuiz] All quizzes reset");
        }

        [ContextMenu("Debug: Print Status")]
        public void DebugPrintStatus()
        {
            Debug.Log($"[IVXWeeklyQuiz] Status (Week: {GetCurrentWeekKey()}):\n" +
                      $"  Fortune: loaded={_fortuneQuiz != null}, active={_isFortuneActive}, completed={HasCompletedFortuneThisWeek()}\n" +
                      $"  Emoji: loaded={_emojiQuiz != null}, active={_isEmojiActive}, completed={HasCompletedEmojiThisWeek()}\n" +
                      $"  Health: loaded={_healthQuiz != null}, active={_isHealthActive}, completed={HasCompletedHealthThisWeek()}\n" +
                      $"  Time until reset: {GetTimeUntilWeeklyReset():d\\dhh\\hmm\\m}");
        }
#endif

        #endregion
    }
}
