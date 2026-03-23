// IVXWeeklyQuizRawModels.cs
// Raw JSON data models for Weekly Quiz - matches S3 JSON structure
// These models are used for deserialization, then converted to SDK-friendly models

using System;
using System.Collections.Generic;
using UnityEngine;

namespace IntelliVerseX.Quiz.WeeklyQuiz
{
    #region Raw S3 JSON Models
    
    /// <summary>
    /// Raw weekly quiz data as stored in S3.
    /// URL Pattern: {year}-{dayOfWeek}-{weekNumber}-{type}_{lang}.json
    /// Example: 2026-6-5-health_en.json
    /// </summary>
    [Serializable]
    public class IVXWeeklyQuizRaw
    {
        public string quizId;
        public LocalizedString topic;
        public LocalizedString date;
        public LocalizedString number_of_questions;
        public LocalizedString difficulty;
        public IVXRawTodayQuiz today_quiz;
        public IVXRawTomorrowQuiz tomorrow_quiz;
        public List<string> topic_history;
        public bool webSearch;
        public int daysToSkip;
        public List<IVXRawMotivationalMessage> motivationalMessages;
        public string s3_url;
    }

    [Serializable]
    public class IVXRawTodayQuiz
    {
        public List<IVXRawQuestion> questions;
        public string generated_at;
        public LocalizedNarrationUrls narration_audio_urls;
    }

    [Serializable]
    public class IVXRawTomorrowQuiz
    {
        public LocalizedString topic;
        public List<IVXRawQuestion> questions;
        public string generated_at;
        public string difficulty;
        public string quizId;
        public LocalizedNarrationUrls narration_audio_urls;
    }

    [Serializable]
    public class IVXRawQuestion
    {
        public string id;
        public string question;
        public List<string> options;
        public int correct_answer;
        public string explanation;
        public IVXRawHints hints;
        public Dictionary<string, IVXRawQuestionLanguage> languages;
    }

    [Serializable]
    public class IVXRawQuestionLanguage
    {
        public string question;
        public List<string> options;
        public string explanation;
        public IVXRawHints hints;
        public string question_audio_url;
        public List<string> options_audio_urls;
    }

    [Serializable]
    public class IVXRawHints
    {
        public string text;
        public string audio_url;
    }

    [Serializable]
    public class IVXRawMotivationalMessage
    {
        public string message;
        public string audioUrl;
        public string topic;
        public string region;
        public string country;
        public string language;
        public string langCode;
    }

    #endregion

    #region Localized String Helper
    
    /// <summary>
    /// Represents a localized string with multiple language variants.
    /// Supports: en, hi, ar, fr, de, es, pt-BR, ru, zh-Hans, ja, ko, id
    /// </summary>
    [Serializable]
    public class LocalizedString
    {
        public string en;
        public string hi;
        public string ar;
        public string fr;
        public string de;
        public string es;
        [SerializeField] private string _ptBR;
        public string ru;
        [SerializeField] private string _zhHans;
        public string ja;
        public string ko;
        public string id;

        // Handle special field names with hyphens
        public string ptBR { get => _ptBR; set => _ptBR = value; }
        public string zhHans { get => _zhHans; set => _zhHans = value; }

        /// <summary>
        /// Gets the localized value for the specified language code.
        /// Falls back to English if the language is not available.
        /// </summary>
        public string Get(string langCode)
        {
            if (string.IsNullOrEmpty(langCode)) langCode = "en";
            
            string result = langCode.ToLowerInvariant() switch
            {
                "en" => en,
                "hi" => hi,
                "ar" => ar,
                "fr" => fr,
                "de" => de,
                "es" or "es-419" => es,
                "pt" or "pt-br" => _ptBR,
                "ru" => ru,
                "zh" or "zh-hans" or "zh-cn" => _zhHans,
                "ja" => ja,
                "ko" => ko,
                "id" => id,
                _ => null
            };

            // Fallback to English if not found
            return !string.IsNullOrEmpty(result) ? result : en;
        }

        public override string ToString() => en ?? string.Empty;
    }

    /// <summary>
    /// Localized narration audio URLs by language code.
    /// </summary>
    [Serializable]
    public class LocalizedNarrationUrls
    {
        public string en;
        public string hi;
        public string ar;
        public string fr;
        public string de;
        public string es;
        [SerializeField] private string _ptBR;
        public string ru;
        [SerializeField] private string _zhHans;
        public string ja;
        public string ko;
        public string id;

        public string ptBR { get => _ptBR; set => _ptBR = value; }
        public string zhHans { get => _zhHans; set => _zhHans = value; }

        public string Get(string langCode)
        {
            if (string.IsNullOrEmpty(langCode)) langCode = "en";
            
            string result = langCode.ToLowerInvariant() switch
            {
                "en" => en,
                "hi" => hi,
                "ar" => ar,
                "fr" => fr,
                "de" => de,
                "es" or "es-419" => es,
                "pt" or "pt-br" => _ptBR,
                "ru" => ru,
                "zh" or "zh-hans" or "zh-cn" => _zhHans,
                "ja" => ja,
                "ko" => ko,
                "id" => id,
                _ => null
            };

            return !string.IsNullOrEmpty(result) ? result : en;
        }
    }

    #endregion

    #region Converter - Raw to SDK Models

    /// <summary>
    /// Converts raw S3 JSON models to SDK-friendly models for a specific language.
    /// </summary>
    public static class IVXWeeklyQuizConverter
    {
        /// <summary>
        /// Converts raw Health quiz data to SDK-friendly IVXHealthQuizData.
        /// </summary>
        public static IVXHealthQuizData ToHealthQuizData(IVXWeeklyQuizRaw raw, string langCode)
        {
            if (raw == null || raw.today_quiz?.questions == null)
                return null;

            var questions = new List<IVXHealthQuestion>();
            foreach (var rawQ in raw.today_quiz.questions)
            {
                var q = ConvertToHealthQuestion(rawQ, langCode);
                if (q != null) questions.Add(q);
            }

            return new IVXHealthQuizData
            {
                weekId = raw.quizId,
                locale = langCode,
                title = raw.topic?.Get(langCode) ?? "Health Quiz",
                emoji = "💧",
                category = "health",
                questions = questions,
                results = CreateDefaultHealthResults()
            };
        }

        /// <summary>
        /// Converts raw Emoji quiz data to SDK-friendly IVXEmojiQuizData.
        /// </summary>
        public static IVXEmojiQuizData ToEmojiQuizData(IVXWeeklyQuizRaw raw, string langCode)
        {
            if (raw == null || raw.today_quiz?.questions == null)
                return null;

            var questions = new List<IVXEmojiQuestion>();
            foreach (var rawQ in raw.today_quiz.questions)
            {
                var q = ConvertToEmojiQuestion(rawQ, langCode);
                if (q != null) questions.Add(q);
            }

            return new IVXEmojiQuizData
            {
                weekId = raw.quizId,
                locale = langCode,
                title = raw.topic?.Get(langCode) ?? "Emoji Quiz",
                emoji = "🎉",
                category = "emoji",
                questions = questions,
                results = CreateDefaultEmojiResults()
            };
        }

        /// <summary>
        /// Converts raw Fortune quiz data to SDK-friendly IVXFortuneQuizData.
        /// </summary>
        public static IVXFortuneQuizData ToFortuneQuizData(IVXWeeklyQuizRaw raw, string langCode)
        {
            if (raw == null || raw.today_quiz?.questions == null)
                return null;

            var questions = new List<IVXFortuneQuestion>();
            foreach (var rawQ in raw.today_quiz.questions)
            {
                var q = ConvertToFortuneQuestion(rawQ, langCode);
                if (q != null) questions.Add(q);
            }

            return new IVXFortuneQuizData
            {
                weekId = raw.quizId,
                locale = langCode,
                title = raw.topic?.Get(langCode) ?? "Fortune Quiz",
                emoji = "🔮",
                category = "fortune",
                questions = questions,
                results = CreateDefaultFortuneResults()
            };
        }

        /// <summary>
        /// Converts raw Prediction quiz data to SDK-friendly IVXPredictionQuizData.
        /// </summary>
        public static IVXPredictionQuizData ToPredictionQuizData(IVXWeeklyQuizRaw raw, string langCode)
        {
            if (raw == null || raw.today_quiz?.questions == null)
                return null;

            var questions = new List<IVXPredictionQuestion>();
            foreach (var rawQ in raw.today_quiz.questions)
            {
                var q = ConvertToPredictionQuestion(rawQ, langCode);
                if (q != null) questions.Add(q);
            }

            return new IVXPredictionQuizData
            {
                weekId = raw.quizId,
                locale = langCode,
                title = raw.topic?.Get(langCode) ?? "Prediction Quiz",
                emoji = "⚽",
                category = "prediction",
                questions = questions
            };
        }

        #region Question Converters

        private static IVXHealthQuestion ConvertToHealthQuestion(IVXRawQuestion rawQ, string langCode)
        {
            if (rawQ == null) return null;

            // Get language-specific content or fallback to default
            var langData = GetLanguageData(rawQ, langCode);
            
            var options = new List<IVXQuizOption>();
            var optionTexts = langData?.options ?? rawQ.options ?? new List<string>();
            for (int i = 0; i < optionTexts.Count; i++)
            {
                options.Add(new IVXQuizOption
                {
                    optionId = $"opt_{i}",
                    text = optionTexts[i]
                });
            }

            return new IVXHealthQuestion
            {
                questionId = rawQ.id,
                questionText = langData?.question ?? rawQ.question,
                options = options,
                correctAnswer = rawQ.correct_answer,
                hint = langData?.hints?.text ?? rawQ.hints?.text,
                explanation = langData?.explanation ?? rawQ.explanation
            };
        }

        private static IVXEmojiQuestion ConvertToEmojiQuestion(IVXRawQuestion rawQ, string langCode)
        {
            if (rawQ == null) return null;

            var langData = GetLanguageData(rawQ, langCode);
            
            var options = new List<IVXQuizOption>();
            var optionTexts = langData?.options ?? rawQ.options ?? new List<string>();
            for (int i = 0; i < optionTexts.Count; i++)
            {
                options.Add(new IVXQuizOption
                {
                    optionId = $"opt_{i}",
                    text = optionTexts[i]
                });
            }

            return new IVXEmojiQuestion
            {
                questionId = rawQ.id,
                questionText = langData?.question ?? rawQ.question,
                options = options,
                correctAnswer = rawQ.correct_answer,
                hint = langData?.hints?.text ?? rawQ.hints?.text,
                explanation = langData?.explanation ?? rawQ.explanation
            };
        }

        private static IVXFortuneQuestion ConvertToFortuneQuestion(IVXRawQuestion rawQ, string langCode)
        {
            if (rawQ == null) return null;

            var langData = GetLanguageData(rawQ, langCode);
            
            var options = new List<IVXQuizOption>();
            var optionTexts = langData?.options ?? rawQ.options ?? new List<string>();
            for (int i = 0; i < optionTexts.Count; i++)
            {
                options.Add(new IVXQuizOption
                {
                    optionId = $"opt_{i}",
                    text = optionTexts[i]
                });
            }

            return new IVXFortuneQuestion
            {
                questionId = rawQ.id,
                questionText = langData?.question ?? rawQ.question,
                options = options,
                hint = langData?.hints?.text ?? rawQ.hints?.text,
                explanation = langData?.explanation ?? rawQ.explanation
            };
        }

        private static IVXPredictionQuestion ConvertToPredictionQuestion(IVXRawQuestion rawQ, string langCode)
        {
            if (rawQ == null) return null;

            var langData = GetLanguageData(rawQ, langCode);
            
            var options = new List<IVXQuizOption>();
            var optionTexts = langData?.options ?? rawQ.options ?? new List<string>();
            for (int i = 0; i < optionTexts.Count; i++)
            {
                options.Add(new IVXQuizOption
                {
                    optionId = $"opt_{i}",
                    text = optionTexts[i]
                });
            }

            return new IVXPredictionQuestion
            {
                questionId = rawQ.id,
                questionText = langData?.question ?? rawQ.question,
                options = options,
                correctAnswer = rawQ.correct_answer,
                hint = langData?.hints?.text ?? rawQ.hints?.text,
                explanation = langData?.explanation ?? rawQ.explanation
            };
        }

        private static IVXRawQuestionLanguage GetLanguageData(IVXRawQuestion rawQ, string langCode)
        {
            if (rawQ?.languages == null || string.IsNullOrEmpty(langCode))
                return null;

            langCode = langCode.ToLowerInvariant();
            
            // Try exact match first
            if (rawQ.languages.TryGetValue(langCode, out var exact))
                return exact;

            // Try common variations
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

            // Fallback to English
            if (rawQ.languages.TryGetValue("en", out var en))
                return en;

            return null;
        }

        #endregion

        #region Default Results

        private static List<IVXHealthResult> CreateDefaultHealthResults()
        {
            return new List<IVXHealthResult>
            {
                new IVXHealthResult { id = "excellent", title = "Health Expert!", description = "You know your health facts!", emoji = "🏆" },
                new IVXHealthResult { id = "good", title = "Health Aware", description = "Good knowledge of health topics!", emoji = "💪" },
                new IVXHealthResult { id = "average", title = "Learning!", description = "Keep learning about health!", emoji = "📚" }
            };
        }

        private static List<IVXEmojiResult> CreateDefaultEmojiResults()
        {
            return new List<IVXEmojiResult>
            {
                new IVXEmojiResult { id = "master", title = "Emoji Master!", description = "You cracked them all!", emoji = "🏆" },
                new IVXEmojiResult { id = "good", title = "Emoji Pro", description = "Great emoji skills!", emoji = "🎉" },
                new IVXEmojiResult { id = "average", title = "Emoji Learner", description = "Keep practicing!", emoji = "😊" }
            };
        }

        private static List<IVXFortuneResult> CreateDefaultFortuneResults()
        {
            return new List<IVXFortuneResult>
            {
                new IVXFortuneResult { id = "mystic", title = "The Mystic", description = "Your intuition is strong!", emoji = "🔮", fortunePrediction = "Great things await you!" },
                new IVXFortuneResult { id = "seeker", title = "The Seeker", description = "Always curious!", emoji = "✨", fortunePrediction = "Adventure is calling!" },
                new IVXFortuneResult { id = "dreamer", title = "The Dreamer", description = "Your creativity shines!", emoji = "🌙", fortunePrediction = "Follow your dreams!" }
            };
        }

        #endregion
    }

    #endregion
}
