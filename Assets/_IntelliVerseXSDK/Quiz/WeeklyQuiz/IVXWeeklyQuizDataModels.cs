// IVXWeeklyQuizDataModels.cs
// Data models for Weekly Quiz functionality in IntelliVerse-X SDK

using System;
using System.Collections.Generic;
using UnityEngine;

namespace IntelliVerseX.Quiz.WeeklyQuiz
{
    #region Enums

    /// <summary>
    /// All weekly quiz types supported by the platform.
    /// </summary>
    public enum IVXWeeklyQuizType
    {
        Fortune,    // 🔮 Personality/fortune quiz with trait mapping
        Emoji,      // 🎉 Emoji combination guessing game
        Prediction, // ⚽ Sports/events prediction quiz
        Health      // 💧 Health & wellness knowledge quiz
    }

    #endregion

    #region Base Result

    /// <summary>
    /// Result of weekly quiz fetch operation
    /// </summary>
    [Serializable]
    public class IVXWeeklyQuizResult
    {
        public bool Success { get; set; }
        public IVXWeeklyQuizType QuizType { get; set; }
        public object QuizData { get; set; }
        public string FetchedDate { get; set; }
        public string LanguageCode { get; set; }
        public string ErrorMessage { get; set; }

        public T GetQuizData<T>() where T : class => QuizData as T;
    }

    #endregion

    #region Fortune Quiz

    /// <summary>
    /// Fortune/personality quiz data structure.
    /// </summary>
    [Serializable]
    public class IVXFortuneQuizData
    {
        public string weekId;
        public string locale;
        public string themeId;
        public List<string> personalityFramework;
        public string legacyQuizId;
        public string title;
        public string emoji;
        public string category;
        public List<IVXFortuneQuestion> questions;
        public List<IVXFortuneResult> results;

        public string quizId => !string.IsNullOrEmpty(weekId) ? weekId : legacyQuizId;
        public int QuestionCount => questions?.Count ?? 0;
        public string SafeTitle => !string.IsNullOrEmpty(title) ? title : "Fortune Quiz";
        public string SafeEmoji => !string.IsNullOrEmpty(emoji) ? emoji : "🔮";

        public bool IsValid(out string error)
        {
            if (string.IsNullOrEmpty(quizId)) { error = "Quiz ID missing"; return false; }
            if (questions == null || questions.Count == 0) { error = "No questions"; return false; }
            if (results == null || results.Count == 0) { error = "No results defined"; return false; }
            error = null;
            return true;
        }
    }

    [Serializable]
    public class IVXFortuneQuestion
    {
        public string questionId;
        public string legacyId;
        public string questionText;
        public List<IVXQuizOption> options;
        public Dictionary<string, List<string>> traitMapping;
        public string hint;
        public string explanation;

        public string id => !string.IsNullOrEmpty(questionId) ? questionId : legacyId;
        public int OptionCount => options?.Count ?? 0;

        public string GetOptionText(int index)
        {
            if (options == null || index < 0 || index >= options.Count) return string.Empty;
            return options[index]?.text ?? string.Empty;
        }

        public string GetOptionId(int index)
        {
            if (options == null || index < 0 || index >= options.Count) return string.Empty;
            return options[index]?.optionId ?? string.Empty;
        }
    }

    [Serializable]
    public class IVXFortuneResult
    {
        public string id;
        public string title;
        public string description;
        public string emoji;
        public List<string> traits;
        public string personalityType;
        public string hiddenInsight;
        public string fortunePrediction;
        public string shareText;
        public int luckyNumber;
        public string luckyColor;

        public string SafeTitle => !string.IsNullOrEmpty(title) ? title : "Your Result";
        public string SafeEmoji => !string.IsNullOrEmpty(emoji) ? emoji : "🔮";
        public string SafeDescription => !string.IsNullOrEmpty(fortunePrediction) ? fortunePrediction 
            : (!string.IsNullOrEmpty(description) ? description : "Your future awaits!");
    }

    #endregion

    #region Emoji Quiz

    [Serializable]
    public class IVXEmojiQuizData
    {
        public string weekId;
        public string locale;
        public string themeId;
        public string legacyQuizId;
        public string title;
        public string emoji;
        public string category;
        public List<IVXEmojiQuestion> questions;
        public List<IVXEmojiResult> results;

        public string quizId => !string.IsNullOrEmpty(weekId) ? weekId : legacyQuizId;
        public int QuestionCount => questions?.Count ?? 0;
        public string SafeTitle => !string.IsNullOrEmpty(title) ? title : "Emoji Quiz";
        public string SafeEmoji => !string.IsNullOrEmpty(emoji) ? emoji : "🎉";

        public bool IsValid(out string error)
        {
            if (string.IsNullOrEmpty(quizId)) { error = "Quiz ID missing"; return false; }
            if (questions == null || questions.Count == 0) { error = "No questions"; return false; }
            error = null;
            return true;
        }
    }

    [Serializable]
    public class IVXEmojiQuestion
    {
        public string questionId;
        public string legacyId;
        public string questionText;
        public List<IVXQuizOption> options;
        public int correctAnswer;
        public string hint;
        public string explanation;

        public string id => !string.IsNullOrEmpty(questionId) ? questionId : legacyId;
        public int OptionCount => options?.Count ?? 0;
        public bool HasValidAnswer => options != null && correctAnswer >= 0 && correctAnswer < options.Count;

        public string GetOptionText(int index)
        {
            if (options == null || index < 0 || index >= options.Count) return string.Empty;
            return options[index]?.text ?? string.Empty;
        }

        public string GetCorrectAnswerText() => HasValidAnswer ? options[correctAnswer]?.text ?? string.Empty : string.Empty;

        public string GetEmojisOnly()
        {
            if (string.IsNullOrEmpty(questionText)) return "❓";
            int idx = questionText.IndexOf('?');
            if (idx >= 0 && idx < questionText.Length - 1)
                return questionText.Substring(idx + 1).Trim();
            return questionText;
        }
    }

    [Serializable]
    public class IVXEmojiResult
    {
        public string id;
        public string title;
        public string description;
        public string emoji;

        public string SafeTitle => string.IsNullOrEmpty(title) ? "Quiz Complete!" : title;
        public string SafeEmoji => string.IsNullOrEmpty(emoji) ? "🎉" : emoji;
        public string SafeDescription => string.IsNullOrEmpty(description) ? "Great effort!" : description;
    }

    #endregion

    #region Prediction Quiz

    [Serializable]
    public class IVXPredictionQuizData
    {
        public string weekId;
        public string locale;
        public string themeId;
        public string legacyQuizId;
        public string title;
        public string emoji;
        public string category;
        public List<IVXPredictionQuestion> questions;
        public string eventDate;
        public string resultsRevealDate;

        public string quizId => !string.IsNullOrEmpty(weekId) ? weekId : legacyQuizId;
        public int QuestionCount => questions?.Count ?? 0;
        public string SafeTitle => !string.IsNullOrEmpty(title) ? title : "Prediction Quiz";
        public string SafeEmoji => !string.IsNullOrEmpty(emoji) ? emoji : "⚽";

        public bool IsResultsRevealed()
        {
            if (string.IsNullOrEmpty(resultsRevealDate)) return false;
            if (DateTime.TryParse(resultsRevealDate, out DateTime revealDate))
                return DateTime.UtcNow >= revealDate;
            return false;
        }

        public bool IsValid(out string error)
        {
            if (string.IsNullOrEmpty(quizId)) { error = "Quiz ID missing"; return false; }
            if (questions == null || questions.Count == 0) { error = "No questions"; return false; }
            error = null;
            return true;
        }
    }

    [Serializable]
    public class IVXPredictionQuestion
    {
        public string questionId;
        public string legacyId;
        public string questionText;
        public List<IVXQuizOption> options;
        public int? correctAnswer;
        public Dictionary<string, float> votePercentages;
        public string hint;
        public string explanation;

        public string id => !string.IsNullOrEmpty(questionId) ? questionId : legacyId;
        public int OptionCount => options?.Count ?? 0;

        public string GetOptionText(int index)
        {
            if (options == null || index < 0 || index >= options.Count) return string.Empty;
            return options[index]?.text ?? string.Empty;
        }
    }

    [Serializable]
    public class IVXPredictionSubmission
    {
        public string quizId;
        public string questionId;
        public int selectedOption;
        public DateTime submittedAt;
        public string userId;
    }

    #endregion

    #region Health Quiz

    [Serializable]
    public class IVXHealthQuizData
    {
        public string weekId;
        public string locale;
        public string themeId;
        public string legacyQuizId;
        public string title;
        public string emoji;
        public string category;
        public List<IVXHealthQuestion> questions;
        public List<IVXHealthResult> results;

        public string quizId => !string.IsNullOrEmpty(weekId) ? weekId : legacyQuizId;
        public int QuestionCount => questions?.Count ?? 0;
        public string SafeTitle => string.IsNullOrEmpty(title) ? "Health Quiz" : title;
        public string SafeEmoji => string.IsNullOrEmpty(emoji) ? "💧" : emoji;

        public bool IsValid(out string error)
        {
            if (string.IsNullOrEmpty(quizId)) { error = "Quiz ID missing"; return false; }
            if (questions == null || questions.Count == 0) { error = "No questions"; return false; }
            error = null;
            return true;
        }
    }

    [Serializable]
    public class IVXHealthQuestion
    {
        public string questionId;
        public string legacyId;
        public string questionText;
        public List<IVXQuizOption> options;
        public int correctAnswer;
        public string hint;
        public string explanation;

        public string id => !string.IsNullOrEmpty(questionId) ? questionId : legacyId;
        public int OptionCount => options?.Count ?? 0;
        public bool HasValidAnswer => options != null && correctAnswer >= 0 && correctAnswer < options.Count;

        public string GetOptionText(int index)
        {
            if (options == null || index < 0 || index >= options.Count) return string.Empty;
            return options[index]?.text ?? string.Empty;
        }

        public string GetCorrectAnswerText() => HasValidAnswer ? options[correctAnswer]?.text ?? string.Empty : string.Empty;
    }

    [Serializable]
    public class IVXHealthResult
    {
        public string id;
        public string title;
        public string description;
        public string emoji;
        public List<string> traits;

        public string SafeTitle => string.IsNullOrEmpty(title) ? "Quiz Complete!" : title;
        public string SafeEmoji => string.IsNullOrEmpty(emoji) ? "💧" : emoji;
        public string SafeDescription => string.IsNullOrEmpty(description) ? "Great effort!" : description;
    }

    #endregion

    #region Shared Types

    [Serializable]
    public class IVXQuizOption
    {
        public string optionId;
        public string text;

        public string SafeText => !string.IsNullOrEmpty(text) ? text : "Option";
        public override string ToString() => text ?? string.Empty;
    }

    [Serializable]
    public class IVXWeeklyQuizAnswerRecord
    {
        public string questionId;
        public int selectedOption;
        public List<string> earnedTraits;
        public bool isCorrect;
        public DateTime answeredAt;
    }

    [Serializable]
    public class IVXWeeklyQuizSummary
    {
        public IVXWeeklyQuizType quizType;
        public string quizId;
        public string quizTitle;
        public int totalQuestions;
        public int correctAnswers;
        public int score;
        public DateTime completedAt;
        public List<IVXWeeklyQuizAnswerRecord> answerHistory;
        public IVXFortuneResult fortuneResult;
        public Dictionary<string, int> traitCounts;
        public int predictionsSubmitted;
        public int correctPredictions;

        public float GetAccuracy()
        {
            if (totalQuestions <= 0) return 0f;
            return Mathf.Clamp((float)correctAnswers / totalQuestions * 100f, 0f, 100f);
        }

        public bool IsPassing => GetAccuracy() >= 60f;
    }

    #endregion

    #region Theme Configuration

    [Serializable]
    public class IVXWeeklyQuizTheme
    {
        public IVXWeeklyQuizType quizType;
        public string primaryEmoji;
        public string themeName;
        public Color primaryColor;
        public Color secondaryColor;
        public Color accentColor;
        public string buttonText;
        public string completedMessage;
    }

    public static class IVXWeeklyQuizThemes
    {
        public static readonly IVXWeeklyQuizTheme FortuneTheme = new IVXWeeklyQuizTheme
        {
            quizType = IVXWeeklyQuizType.Fortune,
            primaryEmoji = "🔮",
            themeName = "Fortune Quiz",
            primaryColor = new Color(0.5f, 0.2f, 0.8f),
            secondaryColor = new Color(0.8f, 0.6f, 0.9f),
            accentColor = new Color(1f, 0.85f, 0.4f),
            buttonText = "Reveal My Fortune",
            completedMessage = "The stars have spoken! ✨"
        };

        public static readonly IVXWeeklyQuizTheme EmojiTheme = new IVXWeeklyQuizTheme
        {
            quizType = IVXWeeklyQuizType.Emoji,
            primaryEmoji = "🎉",
            themeName = "Emoji Quiz",
            primaryColor = new Color(1f, 0.8f, 0.2f),
            secondaryColor = new Color(1f, 0.5f, 0.3f),
            accentColor = new Color(0.2f, 0.8f, 0.4f),
            buttonText = "Guess the Emoji!",
            completedMessage = "You're an Emoji Master! 🏆"
        };

        public static readonly IVXWeeklyQuizTheme PredictionTheme = new IVXWeeklyQuizTheme
        {
            quizType = IVXWeeklyQuizType.Prediction,
            primaryEmoji = "⚽",
            themeName = "Prediction Quiz",
            primaryColor = new Color(0.2f, 0.6f, 0.9f),
            secondaryColor = new Color(0.1f, 0.8f, 0.4f),
            accentColor = new Color(1f, 0.4f, 0.2f),
            buttonText = "Submit Predictions",
            completedMessage = "Predictions locked in! 🔒"
        };

        public static readonly IVXWeeklyQuizTheme HealthTheme = new IVXWeeklyQuizTheme
        {
            quizType = IVXWeeklyQuizType.Health,
            primaryEmoji = "💧",
            themeName = "Health Quiz",
            primaryColor = new Color(0.2f, 0.8f, 0.6f),
            secondaryColor = new Color(0.4f, 0.9f, 0.7f),
            accentColor = new Color(1f, 0.4f, 0.4f),
            buttonText = "Test Your Health IQ",
            completedMessage = "Stay healthy! 💪"
        };

        public static IVXWeeklyQuizTheme GetTheme(IVXWeeklyQuizType type)
        {
            return type switch
            {
                IVXWeeklyQuizType.Fortune => FortuneTheme,
                IVXWeeklyQuizType.Emoji => EmojiTheme,
                IVXWeeklyQuizType.Prediction => PredictionTheme,
                IVXWeeklyQuizType.Health => HealthTheme,
                _ => FortuneTheme
            };
        }
    }

    #endregion

    #region Progress Persistence

    [Serializable]
    public class IVXFortuneQuizProgress
    {
        public string quizId;
        public int currentIndex;
        public List<int> answers;
        public Dictionary<string, int> traitCounts;
        public string savedAt;
    }

    [Serializable]
    public class IVXEmojiQuizProgress
    {
        public string quizId;
        public int currentIndex;
        public int score;
        public int correctCount;
        public int currentStreak;
        public int bestStreak;
        public string savedAt;
    }

    [Serializable]
    public class IVXHealthQuizProgress
    {
        public string quizId;
        public int currentIndex;
        public int score;
        public int correctCount;
        public int currentStreak;
        public int bestStreak;
        public string savedAt;
    }

    [Serializable]
    public class IVXPredictionSaveData
    {
        public string quizId;
        public List<IVXPredictionSaveItem> submissions;
    }

    [Serializable]
    public class IVXPredictionSaveItem
    {
        public string questionId;
        public int selectedOption;
        public string submittedAt;
    }

    #endregion
}
