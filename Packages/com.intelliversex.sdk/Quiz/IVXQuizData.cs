using System;
using System.Collections.Generic;

namespace IntelliVerseX.Quiz
{
    /// <summary>
    /// Interface for quiz questions.
    /// Implement this interface to create custom question types.
    /// </summary>
    public interface IVXQuestion
    {
        string QuestionText { get; }
        List<string> Options { get; }
        int CorrectAnswerIndex { get; }
        string Category { get; }
        int Difficulty { get; }
    }

    /// <summary>
    /// Standard quiz question implementation.
    /// Part of IntelliVerse.GameSDK.Quiz package.
    /// </summary>
    [Serializable]
    public class IVXQuizQuestion : IVXQuestion
    {
        public string questionText;
        public List<string> options;
        public int correctAnswerIndex;
        public string category;
        public int difficulty;
        public string explanation;
        public string hint;
        public Dictionary<string, object> metadata;

        public string QuestionText => questionText;
        public List<string> Options => options;
        public int CorrectAnswerIndex => correctAnswerIndex;
        public string Category => category;
        public int Difficulty => difficulty;
        public string Explanation => explanation;
        public string Hint => hint;
        public Dictionary<string, object> Metadata
        {
            get => metadata ?? (metadata = new Dictionary<string, object>());
            set => metadata = value;
        }

        public IVXQuizQuestion()
        {
            options = new List<string>();
            metadata = new Dictionary<string, object>();
        }
    }

    /// <summary>
    /// Quiz data container for a complete quiz session.
    /// </summary>
    [Serializable]
    public class IVXQuizData
    {
        public string quizId;
        public string title;
        public string description;
        public List<IVXQuizQuestion> questions;
        public DateTime createdDate;
        public string language;
        public Dictionary<string, object> metadata;

        public string QuizId => quizId;
        public string Title => title;
        public string Description => description;
        public List<IVXQuizQuestion> Questions => questions;
        public Dictionary<string, object> Metadata
        {
            get => metadata ?? (metadata = new Dictionary<string, object>());
            set => metadata = value;
        }

        public IVXQuizData()
        {
            questions = new List<IVXQuizQuestion>();
            metadata = new Dictionary<string, object>();
        }
    }

    /// <summary>
    /// Result of a quiz fetch operation.
    /// </summary>
    public class IVXQuizResult
    {
        public bool Success { get; set; }
        public IVXQuizData QuizData { get; set; }
        public string ErrorMessage { get; set; }
        public DateTime FetchDate { get; set; }
        public string SourceUrl { get; set; }

        public static IVXQuizResult SuccessResult(IVXQuizData data, string sourceUrl = null)
        {
            return new IVXQuizResult
            {
                Success = true,
                QuizData = data,
                FetchDate = DateTime.UtcNow,
                SourceUrl = sourceUrl
            };
        }

        public static IVXQuizResult FailureResult(string error)
        {
            return new IVXQuizResult
            {
                Success = false,
                ErrorMessage = error,
                FetchDate = DateTime.UtcNow
            };
        }
    }

    /// <summary>
    /// Quiz session state tracking.
    /// </summary>
    public class IVXQuizSession
    {
        public string SessionId { get; private set; }
        public IVXQuizData QuizData { get; private set; }
        public int CurrentQuestionIndex { get; private set; }
        public List<int> PlayerAnswers { get; private set; }
        public List<bool> AnswerCorrectness { get; private set; }
        public DateTime StartTime { get; private set; }
        public DateTime? EndTime { get; private set; }
        public bool IsComplete { get; private set; }

        public IVXQuizSession(IVXQuizData quizData)
        {
            SessionId = Guid.NewGuid().ToString();
            QuizData = quizData;
            CurrentQuestionIndex = 0;
            PlayerAnswers = new List<int>();
            AnswerCorrectness = new List<bool>();
            StartTime = DateTime.UtcNow;
            IsComplete = false;
        }

        public IVXQuizQuestion GetCurrentQuestion()
        {
            if (QuizData == null || QuizData.questions == null || QuizData.questions.Count == 0)
                return null;

            if (CurrentQuestionIndex >= QuizData.questions.Count)
                return null;

            return QuizData.questions[CurrentQuestionIndex];
        }

        public void SubmitAnswer(int answerIndex)
        {
            var currentQuestion = GetCurrentQuestion();
            if (currentQuestion == null)
                return;

            PlayerAnswers.Add(answerIndex);
            bool isCorrect = answerIndex == currentQuestion.CorrectAnswerIndex;
            AnswerCorrectness.Add(isCorrect);

            CurrentQuestionIndex++;

            if (CurrentQuestionIndex >= QuizData.questions.Count)
            {
                IsComplete = true;
                EndTime = DateTime.UtcNow;
            }
        }

        public int GetScore()
        {
            return AnswerCorrectness.FindAll(x => x).Count;
        }

        public float GetScorePercentage()
        {
            if (QuizData == null || QuizData.questions.Count == 0)
                return 0f;

            return (float)GetScore() / QuizData.questions.Count * 100f;
        }

        public TimeSpan GetElapsedTime()
        {
            if (EndTime.HasValue)
                return EndTime.Value - StartTime;
            
            return DateTime.UtcNow - StartTime;
        }
    }
}
