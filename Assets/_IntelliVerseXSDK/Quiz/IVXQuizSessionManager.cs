using System;
using System.Threading.Tasks;
using UnityEngine;
using IntelliVerseX.Core;

namespace IntelliVerseX.Quiz
{
    /// <summary>
    /// Manages quiz session lifecycle: initialization, question navigation, answer submission, scoring.
    /// Part of IntelliVerse.GameSDK.Quiz package.
    /// </summary>
    public class IVXQuizSessionManager : IVXSafeSingleton<IVXQuizSessionManager>
    {
        private IIVXQuizProvider _quizProvider;
        private IVXQuizSession _currentSession;
        private bool _shuffleQuestions = true;
        private bool _shuffleOptions = true;

        public event Action<IVXQuizSession> OnSessionStarted;
        public event Action<IVXQuizQuestion, int> OnQuestionDisplayed;
        public event Action<bool, int> OnAnswerSubmitted; // isCorrect, score
        public event Action<IVXQuizSession> OnSessionCompleted;
        public event Action<string> OnError;

        public IVXQuizSession CurrentSession => _currentSession;
        public bool IsSessionActive => _currentSession != null && !_currentSession.IsComplete;

        /// <summary>
        /// Initializes the quiz manager with a provider
        /// </summary>
        public void Initialize(IIVXQuizProvider provider, bool shuffleQuestions = true, bool shuffleOptions = true)
        {
            _quizProvider = provider ?? throw new ArgumentNullException(nameof(provider));
            _shuffleQuestions = shuffleQuestions;
            _shuffleOptions = shuffleOptions;
            
            IVXLogger.Log("Quiz manager initialized");
        }

        /// <summary>
        /// Starts a new quiz session by fetching quiz data
        /// </summary>
        public async Task<bool> StartSessionAsync(string quizId)
        {
            if (_quizProvider == null)
            {
                IVXLogger.LogError("Quiz provider not initialized");
                OnError?.Invoke("Quiz provider not initialized");
                return false;
            }

            // Fetch quiz data
            var result = await _quizProvider.FetchQuizAsync(quizId);
            
            if (!result.Success)
            {
                IVXLogger.LogError($"Failed to fetch quiz: {result.ErrorMessage}");
                OnError?.Invoke(result.ErrorMessage);
                return false;
            }

            // Shuffle questions if enabled
            if (_shuffleQuestions && result.QuizData.questions != null)
            {
                var shuffledQuestions = result.QuizData.questions.ConvertAll(q => (IVXQuestion)q).ShuffleQuestions();
                result.QuizData.questions = shuffledQuestions.ConvertAll(q => (IVXQuizQuestion)q);
                IVXLogger.Log("Questions shuffled");
            }

            // Create new session
            _currentSession = new IVXQuizSession(result.QuizData);
            
            IVXLogger.Log($"Quiz session started: {_currentSession.SessionId}");
            OnSessionStarted?.Invoke(_currentSession);
            
            // Display first question
            DisplayCurrentQuestion();
            
            return true;
        }

        /// <summary>
        /// Starts a daily quiz session for today
        /// </summary>
        public async Task<bool> StartDailyQuizAsync()
        {
            if (_quizProvider == null)
            {
                IVXLogger.LogError("Quiz provider not initialized");
                OnError?.Invoke("Quiz provider not initialized");
                return false;
            }

            var result = await _quizProvider.FetchQuizAsync(DateTime.UtcNow);
            
            if (!result.Success)
            {
                IVXLogger.LogError($"Failed to fetch daily quiz: {result.ErrorMessage}");
                OnError?.Invoke(result.ErrorMessage);
                return false;
            }

            if (_shuffleQuestions && result.QuizData.questions != null)
            {
                var shuffledQuestions = result.QuizData.questions.ConvertAll(q => (IVXQuestion)q).ShuffleQuestions();
                result.QuizData.questions = shuffledQuestions.ConvertAll(q => (IVXQuizQuestion)q);
            }

            _currentSession = new IVXQuizSession(result.QuizData);
            
            IVXLogger.Log($"Daily quiz session started: {_currentSession.SessionId}");
            OnSessionStarted?.Invoke(_currentSession);
            
            DisplayCurrentQuestion();
            
            return true;
        }

        /// <summary>
        /// Displays the current question
        /// </summary>
        private void DisplayCurrentQuestion()
        {
            if (_currentSession == null)
                return;

            var question = _currentSession.GetCurrentQuestion();
            if (question == null)
            {
                IVXLogger.LogWarning("No current question available");
                return;
            }

            int questionNumber = _currentSession.CurrentQuestionIndex + 1;
            IVXLogger.Log($"Displaying question {questionNumber}/{_currentSession.QuizData.questions.Count}");
            
            OnQuestionDisplayed?.Invoke(question, questionNumber);
        }

        /// <summary>
        /// Submits an answer for the current question
        /// </summary>
        public void SubmitAnswer(int answerIndex)
        {
            if (_currentSession == null || !IsSessionActive)
            {
                IVXLogger.LogWarning("No active session");
                return;
            }

            var currentQuestion = _currentSession.GetCurrentQuestion();
            if (currentQuestion == null)
            {
                IVXLogger.LogError("No current question");
                return;
            }

            // Validate answer index
            if (answerIndex < 0 || answerIndex >= currentQuestion.Options.Count)
            {
                IVXLogger.LogError($"Invalid answer index: {answerIndex}");
                return;
            }

            // Submit answer to session
            _currentSession.SubmitAnswer(answerIndex);
            
            bool isCorrect = answerIndex == currentQuestion.CorrectAnswerIndex;
            int currentScore = _currentSession.GetScore();
            
            IVXLogger.Log($"Answer submitted: {(isCorrect ? "Correct" : "Incorrect")} | Score: {currentScore}");
            OnAnswerSubmitted?.Invoke(isCorrect, currentScore);

            // Check if quiz is complete
            if (_currentSession.IsComplete)
            {
                CompleteSession();
            }
            else
            {
                // Display next question
                DisplayCurrentQuestion();
            }
        }

        /// <summary>
        /// Submits an answer with additional context (adapter compatibility)
        /// </summary>
        public void SubmitAnswer(int questionIndex, int answerIndex)
        {
            // Validate question index matches current question
            if (_currentSession != null && _currentSession.CurrentQuestionIndex == questionIndex)
            {
                SubmitAnswer(answerIndex);
            }
            else
            {
                IVXLogger.LogWarning($"Question index mismatch: expected {_currentSession?.CurrentQuestionIndex}, got {questionIndex}");
                SubmitAnswer(answerIndex); // Submit anyway
            }
        }

        /// <summary>
        /// Completes the current quiz session
        /// </summary>
        private void CompleteSession()
        {
            if (_currentSession == null)
                return;

            int finalScore = _currentSession.GetScore();
            float percentage = _currentSession.GetScorePercentage();
            var elapsedTime = _currentSession.GetElapsedTime();
            
            IVXLogger.Log($"Quiz completed! Score: {finalScore}/{_currentSession.QuizData.questions.Count} ({percentage:F1}%) | Time: {elapsedTime:mm\\:ss}");
            
            OnSessionCompleted?.Invoke(_currentSession);
        }

        /// <summary>
        /// Gets the current question (for UI display)
        /// </summary>
        public IVXQuizQuestion GetCurrentQuestion()
        {
            return _currentSession?.GetCurrentQuestion();
        }

        /// <summary>
        /// Gets the current question number (1-based)
        /// </summary>
        public int GetCurrentQuestionNumber()
        {
            if (_currentSession == null)
                return 0;

            return _currentSession.CurrentQuestionIndex + 1;
        }

        /// <summary>
        /// Gets total number of questions
        /// </summary>
        public int GetTotalQuestions()
        {
            if (_currentSession == null || _currentSession.QuizData == null)
                return 0;

            return _currentSession.QuizData.questions.Count;
        }

        /// <summary>
        /// Gets current score
        /// </summary>
        public int GetCurrentScore()
        {
            return _currentSession?.GetScore() ?? 0;
        }

        /// <summary>
        /// Ends the current session prematurely
        /// </summary>
        public void EndSession()
        {
            if (_currentSession != null)
            {
                IVXLogger.Log("Session ended manually");
                _currentSession = null;
            }
        }

        /// <summary>
        /// Reset current session to start over
        /// </summary>
        public void ResetSession()
        {
            if (_currentSession != null)
            {
                IVXLogger.Log("Resetting session");
                _currentSession = null;
            }
        }

        /// <summary>
        /// Complete session and optionally save to backend
        /// </summary>
        public async Task CompleteSessionAsync(bool saveToBackend = false)
        {
            if (_currentSession == null)
            {
                IVXLogger.LogWarning("No active session to complete");
                return;
            }

            CompleteSession();

            if (saveToBackend)
            {
                // TODO: Implement backend saving if needed
                IVXLogger.Log("Session saved to backend (not yet implemented)");
                await Task.CompletedTask;
            }
        }

        protected override void OnCleanup()
        {
            EndSession();
            base.OnCleanup();
        }
    }
}
