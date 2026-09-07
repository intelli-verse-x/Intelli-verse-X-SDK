// IVXDailyQuizDataModels.cs
// Data models for Daily Quiz functionality in IntelliVerse-X SDK
// Supports both standard Daily Quiz and Premium Daily Quiz

using System;
using System.Collections.Generic;
using UnityEngine;

namespace IntelliVerseX.Quiz.DailyQuiz
{
    #region Enums

    /// <summary>
    /// Daily quiz types supported by the platform.
    /// </summary>
    public enum IVXDailyQuizType
    {
        Daily,        // Standard daily quiz (free)
        DailyPremium  // Premium daily quiz (paid/subscription)
    }

    #endregion

    #region Base Result

    /// <summary>
    /// Result of daily quiz fetch operation.
    /// </summary>
    [Serializable]
    public class IVXDailyQuizResult
    {
        public bool Success { get; set; }
        public IVXDailyQuizType QuizType { get; set; }
        public object QuizData { get; set; }
        public string FetchedDate { get; set; }
        public string LanguageCode { get; set; }
        public string ErrorMessage { get; set; }
        public string SourceUrl { get; set; }

        public T GetQuizData<T>() where T : class => QuizData as T;

        public override string ToString()
        {
            if (Success)
                return $"[{QuizType}] Success from {SourceUrl} (Date: {FetchedDate}, Lang: {LanguageCode})";
            return $"[{QuizType}] Failed: {ErrorMessage}";
        }
    }

    #endregion

    #region Raw S3 JSON Models

    /// <summary>
    /// Raw daily quiz data as stored in S3.
    /// URL Pattern: dailyquiz-{YYYY}-{MM}-{DD}.json
    /// Example: dailyquiz-2026-02-10.json
    /// </summary>
    [Serializable]
    public class IVXDailyQuizRaw
    {
        public string quizId;
        public IVXRawDailyTodayQuiz today_quiz;
        public string s3_url;
        public string generated_at;
    }

    /// <summary>
    /// Raw premium daily quiz data as stored in S3.
    /// URL Pattern: dailyquiz-prem-{lang}-{YYYY}-{MM}-{DD}.json
    /// Example: dailyquiz-prem-hi-2026-01-19.json
    /// </summary>
    [Serializable]
    public class IVXDailyPremiumQuizRaw
    {
        public string quizId;
        public List<IVXRawDailyQuestion> premium_questions;
        public string s3_url;
        public string generated_at;
        public string difficulty;
        public string topic;
    }

    [Serializable]
    public class IVXRawDailyTodayQuiz
    {
        public List<IVXRawDailyQuestion> questions;
        public string generated_at;
        public string topic;
        public string difficulty;
    }

    [Serializable]
    public class IVXRawDailyQuestion
    {
        public string id;
        public string question;
        public List<string> options;
        public int correct_answer;
        public string explanation;
        public IVXRawDailyHints hints;
        public Dictionary<string, IVXRawDailyQuestionLanguage> languages;
    }

    [Serializable]
    public class IVXRawDailyQuestionLanguage
    {
        public string question;
        public List<string> options;
        public string explanation;
        public IVXRawDailyHints hints;
        public string question_audio_url;
        public List<string> options_audio_urls;
    }

    [Serializable]
    public class IVXRawDailyHints
    {
        public string text;
        public string audio_url;
    }

    #endregion

    #region SDK-Friendly Models

    /// <summary>
    /// Processed daily quiz data for SDK consumption.
    /// </summary>
    [Serializable]
    public class IVXDailyQuizData
    {
        public string quizId;
        public string locale;
        public string title;
        public string emoji;
        public string category;
        public string difficulty;
        public string topic;
        public List<IVXDailyQuestion> questions;
        public List<IVXDailyResult> results;
        public bool isPremium;
        public string quizDate;

        public int QuestionCount => questions?.Count ?? 0;
        public string SafeTitle => !string.IsNullOrEmpty(title) ? title : (isPremium ? "Premium Daily Quiz" : "Daily Quiz");
        public string SafeEmoji => !string.IsNullOrEmpty(emoji) ? emoji : (isPremium ? "💎" : "📅");

        public bool IsValid(out string error)
        {
            if (string.IsNullOrEmpty(quizId)) { error = "Quiz ID missing"; return false; }
            if (questions == null || questions.Count == 0) { error = "No questions"; return false; }
            error = null;
            return true;
        }
    }

    [Serializable]
    public class IVXDailyQuestion
    {
        public string questionId;
        public string questionText;
        public List<IVXDailyQuizOption> options;
        public int correctAnswer;
        public string hint;
        public string explanation;

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
    public class IVXDailyQuizOption
    {
        public string optionId;
        public string text;

        public string SafeText => !string.IsNullOrEmpty(text) ? text : "Option";
        public override string ToString() => text ?? string.Empty;
    }

    [Serializable]
    public class IVXDailyResult
    {
        public string id;
        public string title;
        public string description;
        public string emoji;
        public int minScore;
        public int maxScore;

        public string SafeTitle => string.IsNullOrEmpty(title) ? "Quiz Complete!" : title;
        public string SafeEmoji => string.IsNullOrEmpty(emoji) ? "🏆" : emoji;
        public string SafeDescription => string.IsNullOrEmpty(description) ? "Great effort!" : description;
    }

    #endregion

    #region Answer Tracking

    [Serializable]
    public class IVXDailyQuizAnswerRecord
    {
        public string questionId;
        public int selectedOption;
        public bool isCorrect;
        public DateTime answeredAt;
    }

    [Serializable]
    public class IVXDailyQuizSummary
    {
        public IVXDailyQuizType quizType;
        public string quizId;
        public string quizTitle;
        public string quizDate;
        public int totalQuestions;
        public int correctAnswers;
        public int score;
        public DateTime completedAt;
        public List<IVXDailyQuizAnswerRecord> answerHistory;
        public bool isPremium;
        public int currentStreak;
        public int bestStreak;

        public float GetAccuracy()
        {
            if (totalQuestions <= 0) return 0f;
            return Mathf.Clamp((float)correctAnswers / totalQuestions * 100f, 0f, 100f);
        }

        public bool IsPassing => GetAccuracy() >= 60f;
    }

    #endregion

    #region Progress Persistence

    [Serializable]
    public class IVXDailyQuizProgress
    {
        public string quizId;
        public string quizDate;
        public int currentIndex;
        public int score;
        public int correctCount;
        public int currentStreak;
        public int bestStreak;
        public bool isPremium;
        public string savedAt;
    }

    #endregion

    #region Theme Configuration

    [Serializable]
    public class IVXDailyQuizTheme
    {
        public IVXDailyQuizType quizType;
        public string primaryEmoji;
        public string themeName;
        public Color primaryColor;
        public Color secondaryColor;
        public Color accentColor;
        public string buttonText;
        public string completedMessage;
    }

    public static class IVXDailyQuizThemes
    {
        public static readonly IVXDailyQuizTheme DailyTheme = new IVXDailyQuizTheme
        {
            quizType = IVXDailyQuizType.Daily,
            primaryEmoji = "📅",
            themeName = "Daily Quiz",
            primaryColor = new Color(0.2f, 0.6f, 0.9f),
            secondaryColor = new Color(0.4f, 0.7f, 0.95f),
            accentColor = new Color(1f, 0.8f, 0.2f),
            buttonText = "Start Daily Quiz",
            completedMessage = "See you tomorrow! 🌟"
        };

        public static readonly IVXDailyQuizTheme PremiumTheme = new IVXDailyQuizTheme
        {
            quizType = IVXDailyQuizType.DailyPremium,
            primaryEmoji = "💎",
            themeName = "Premium Daily Quiz",
            primaryColor = new Color(0.6f, 0.3f, 0.8f),
            secondaryColor = new Color(0.8f, 0.5f, 0.95f),
            accentColor = new Color(1f, 0.85f, 0.3f),
            buttonText = "Start Premium Quiz",
            completedMessage = "Premium Complete! 💎✨"
        };

        public static IVXDailyQuizTheme GetTheme(IVXDailyQuizType type)
        {
            return type switch
            {
                IVXDailyQuizType.Daily => DailyTheme,
                IVXDailyQuizType.DailyPremium => PremiumTheme,
                _ => DailyTheme
            };
        }
    }

    #endregion

    #region Converter - Raw to SDK Models

    /// <summary>
    /// Converts raw S3 JSON models to SDK-friendly models for a specific language.
    /// </summary>
    public static class IVXDailyQuizConverter
    {
        /// <summary>
        /// Converts raw Daily quiz data to SDK-friendly IVXDailyQuizData.
        /// </summary>
        public static IVXDailyQuizData ToDailyQuizData(IVXDailyQuizRaw raw, string langCode, string quizDate)
        {
            if (raw == null || raw.today_quiz?.questions == null)
                return null;

            var questions = new List<IVXDailyQuestion>();
            foreach (var rawQ in raw.today_quiz.questions)
            {
                var q = ConvertToDailyQuestion(rawQ, langCode);
                if (q != null) questions.Add(q);
            }

            return new IVXDailyQuizData
            {
                quizId = raw.quizId ?? $"daily-{quizDate}",
                locale = langCode,
                title = raw.today_quiz.topic ?? "Daily Quiz",
                emoji = "📅",
                category = "daily",
                difficulty = raw.today_quiz.difficulty ?? "medium",
                topic = raw.today_quiz.topic,
                questions = questions,
                results = CreateDefaultDailyResults(),
                isPremium = false,
                quizDate = quizDate
            };
        }

        /// <summary>
        /// Converts raw Premium Daily quiz data to SDK-friendly IVXDailyQuizData.
        /// </summary>
        public static IVXDailyQuizData ToPremiumDailyQuizData(IVXDailyPremiumQuizRaw raw, string langCode, string quizDate)
        {
            if (raw == null || raw.premium_questions == null)
                return null;

            var questions = new List<IVXDailyQuestion>();
            foreach (var rawQ in raw.premium_questions)
            {
                var q = ConvertToDailyQuestion(rawQ, langCode);
                if (q != null) questions.Add(q);
            }

            return new IVXDailyQuizData
            {
                quizId = raw.quizId ?? $"daily-prem-{quizDate}",
                locale = langCode,
                title = raw.topic ?? "Premium Daily Quiz",
                emoji = "💎",
                category = "daily-premium",
                difficulty = raw.difficulty ?? "hard",
                topic = raw.topic,
                questions = questions,
                results = CreateDefaultPremiumResults(),
                isPremium = true,
                quizDate = quizDate
            };
        }

        private static IVXDailyQuestion ConvertToDailyQuestion(IVXRawDailyQuestion rawQ, string langCode)
        {
            if (rawQ == null) return null;

            var langData = GetLanguageData(rawQ, langCode);

            var options = new List<IVXDailyQuizOption>();
            var optionTexts = langData?.options ?? rawQ.options ?? new List<string>();
            for (int i = 0; i < optionTexts.Count; i++)
            {
                options.Add(new IVXDailyQuizOption
                {
                    optionId = $"opt_{i}",
                    text = optionTexts[i]
                });
            }

            return new IVXDailyQuestion
            {
                questionId = rawQ.id,
                questionText = langData?.question ?? rawQ.question,
                options = options,
                correctAnswer = rawQ.correct_answer,
                hint = langData?.hints?.text ?? rawQ.hints?.text,
                explanation = langData?.explanation ?? rawQ.explanation
            };
        }

        private static IVXRawDailyQuestionLanguage GetLanguageData(IVXRawDailyQuestion rawQ, string langCode)
        {
            if (rawQ?.languages == null || string.IsNullOrEmpty(langCode))
                return null;

            langCode = langCode.ToLowerInvariant();

            if (rawQ.languages.TryGetValue(langCode, out var exact))
                return exact;

            string altKey = langCode switch
            {
                "es" => "es-419",
                "es-419" => "es",
                "pt" or "pt-br" => "pt-BR",
                "zh" or "zh-cn" => "zh-Hans",
                "zh-hans" => "zh-Hans",
                _ => null
            };

            if (altKey != null && rawQ.languages.TryGetValue(altKey, out var alt))
                return alt;

            if (rawQ.languages.TryGetValue("en", out var en))
                return en;

            return null;
        }

        private static List<IVXDailyResult> CreateDefaultDailyResults()
        {
            return new List<IVXDailyResult>
            {
                new IVXDailyResult { id = "perfect", title = "Perfect Score!", description = "Amazing! You got them all right!", emoji = "🏆", minScore = 100, maxScore = 100 },
                new IVXDailyResult { id = "excellent", title = "Excellent!", description = "Outstanding performance!", emoji = "🌟", minScore = 80, maxScore = 99 },
                new IVXDailyResult { id = "good", title = "Good Job!", description = "Well done!", emoji = "👍", minScore = 60, maxScore = 79 },
                new IVXDailyResult { id = "keep_trying", title = "Keep Trying!", description = "You'll do better tomorrow!", emoji = "💪", minScore = 0, maxScore = 59 }
            };
        }

        private static List<IVXDailyResult> CreateDefaultPremiumResults()
        {
            return new List<IVXDailyResult>
            {
                new IVXDailyResult { id = "master", title = "Quiz Master!", description = "Flawless victory!", emoji = "💎", minScore = 100, maxScore = 100 },
                new IVXDailyResult { id = "expert", title = "Expert Level!", description = "Impressive premium performance!", emoji = "🏅", minScore = 80, maxScore = 99 },
                new IVXDailyResult { id = "skilled", title = "Skilled Player!", description = "Great premium effort!", emoji = "⭐", minScore = 60, maxScore = 79 },
                new IVXDailyResult { id = "challenger", title = "Challenger!", description = "Premium quizzes are tough!", emoji = "🎯", minScore = 0, maxScore = 59 }
            };
        }
    }

    #endregion
}
