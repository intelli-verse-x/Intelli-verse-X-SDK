// IVXWeeklyQuizService.cs
// Service for fetching weekly quiz data from S3
// Supports multilingual JSON format with language-specific content extraction

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace IntelliVerseX.Quiz.WeeklyQuiz
{
    /// <summary>
    /// Service for fetching weekly quiz data from S3.
    /// Handles all four quiz types: Fortune, Emoji, Prediction, and Health.
    /// 
    /// URL Pattern:
    ///   https://intelli-verse-x-media.s3.us-east-1.amazonaws.com/quiz-verse/weekly/{YYYY}-{D}-{WW}-{type}_{lang}.json
    /// 
    /// Where:
    ///   YYYY = Year (e.g., 2026)
    ///   D    = Day of week when uploaded (1-7, doesn't affect content)
    ///   WW   = Week number of the year (1-53, ISO 8601)
    ///   type = Quiz type (fortune, emoji, prediction, health)
    ///   lang = Language code (en, es, ar, etc.)
    /// 
    /// Example: 2026-6-5-emoji_en.json = Week 5 of 2026, uploaded on Saturday(6), Emoji quiz, English
    /// 
    /// Note: Quizzes are uploaded once per week, not daily. The day-of-week in the URL
    /// indicates when the file was uploaded, not the content date.
    /// </summary>
    public class IVXWeeklyQuizService
    {
        #region Constants

        private const string S3_BASE_URL = "https://intelli-verse-x-media.s3.us-east-1.amazonaws.com/quiz-verse/weekly/";
        private const string QUIZ_FILE_EXTENSION = ".json";
        private const int MAX_FALLBACK_WEEKS = 8;
        private const int REQUEST_TIMEOUT_SECONDS = 15;
        private const int FALLBACK_TIMEOUT_SECONDS = 10;

        private static readonly string[] SUPPORTED_LOCALES = { "en", "es", "ar", "fr", "de", "pt", "hi", "zh", "ja", "ko", "id", "ru" };

        #endregion

        #region Fields

        private readonly Dictionary<string, IVXWeeklyQuizResult> _cache = new Dictionary<string, IVXWeeklyQuizResult>();
        private readonly bool[] _isFetchingByType = new bool[4];
        private readonly object _cacheLock = new object();
        
        /// <summary>
        /// Enable verbose debug logging for URL generation and fetch attempts.
        /// </summary>
#if UNITY_EDITOR
        public bool EnableVerboseLogging { get; set; } = true;
#else
        public bool EnableVerboseLogging { get; set; } = false;
#endif

        #endregion

        #region Public API

        public Task<IVXWeeklyQuizResult> FetchFortuneQuizAsync() => FetchWeeklyQuizAsync(IVXWeeklyQuizType.Fortune);
        public Task<IVXWeeklyQuizResult> FetchEmojiQuizAsync() => FetchWeeklyQuizAsync(IVXWeeklyQuizType.Emoji);
        public Task<IVXWeeklyQuizResult> FetchPredictionQuizAsync() => FetchWeeklyQuizAsync(IVXWeeklyQuizType.Prediction);
        public Task<IVXWeeklyQuizResult> FetchHealthQuizAsync() => FetchWeeklyQuizAsync(IVXWeeklyQuizType.Health);

        /// <summary>
        /// Gets the URL that would be used to fetch a quiz for a specific week.
        /// </summary>
        public string GetQuizUrl(IVXWeeklyQuizType quizType, int year, int week, int dayOfWeek, string langCode = "en")
        {
            string prefix = GetQuizPrefix(quizType);
            return BuildQuizUrlDirect(prefix, langCode, year, week, dayOfWeek);
        }

        /// <summary>
        /// Gets the URL for the current week's quiz (using current day of week).
        /// </summary>
        public string GetCurrentQuizUrl(IVXWeeklyQuizType quizType)
        {
            DateTime utcNow = DateTime.UtcNow;
            string langCode = GetCurrentLanguageCode();
            string prefix = GetQuizPrefix(quizType);
            var (year, week, day) = GetISOWeekDate(utcNow);
            return BuildQuizUrlDirect(prefix, langCode, year, week, day);
        }

        /// <summary>
        /// Gets the current week information.
        /// Returns (year, dayOfWeek, weekNumber) using ISO 8601 week date.
        /// </summary>
        public (int year, int dayOfWeek, int weekNumber) GetCurrentWeekInfo()
        {
            DateTime utcNow = DateTime.UtcNow;
            var (year, week, day) = GetISOWeekDate(utcNow);
            return (year, day, week);
        }

        /// <summary>
        /// Debug helper: Logs the URL pattern for multiple weeks to help verify S3 file naming.
        /// </summary>
        public void LogUrlPatternForWeeks(IVXWeeklyQuizType quizType, int weeksToShow = 10)
        {
            DateTime today = DateTime.UtcNow;
            string prefix = GetQuizPrefix(quizType);
            string langCode = GetCurrentLanguageCode();
            
            Debug.Log($"[IVXWeeklyQuizService] === URL Pattern Debug for {quizType} ===");
            Debug.Log($"[IVXWeeklyQuizService] Today: {today:yyyy-MM-dd} ({today.DayOfWeek})");
            Debug.Log($"[IVXWeeklyQuizService] Language: {langCode}");
            
            for (int i = 0; i < weeksToShow; i++)
            {
                DateTime targetDate = today.AddDays(-7 * i);
                var (year, week, day) = GetISOWeekDate(targetDate);
                
                // Show all possible URLs for this week (day 1-7)
                Debug.Log($"[IVXWeeklyQuizService] --- Week {week} of {year} (offset -{i}) ---");
                for (int d = 1; d <= 7; d++)
                {
                    string url = BuildQuizUrlDirect(prefix, langCode, year, week, d);
                    Debug.Log($"[IVXWeeklyQuizService]   Day {d}: {url}");
                }
            }
        }

        public void ClearCache()
        {
            lock (_cacheLock)
            {
                _cache.Clear();
            }
            Debug.Log("[IVXWeeklyQuizService] Cache cleared");
        }

        public void ClearCache(IVXWeeklyQuizType quizType)
        {
            string cacheKeyPrefix = GetQuizPrefix(quizType);
            lock (_cacheLock)
            {
                var keysToRemove = new List<string>();
                foreach (var key in _cache.Keys)
                {
                    if (key.Contains(cacheKeyPrefix))
                        keysToRemove.Add(key);
                }
                foreach (var key in keysToRemove)
                    _cache.Remove(key);
            }
        }

        public async Task<IVXWeeklyQuizResult> FetchWeeklyQuizAsync(IVXWeeklyQuizType quizType)
        {
            int typeIndex = (int)quizType;
            if (_isFetchingByType[typeIndex])
            {
                Debug.LogWarning($"[IVXWeeklyQuizService] Already fetching {quizType} — skipping duplicate request");
                return CreateErrorResult(quizType, "Fetch already in progress");
            }

            _isFetchingByType[typeIndex] = true;
            try
            {
                DateTime utcNow = DateTime.UtcNow;
                string langCode = GetCurrentLanguageCode();
                var (year, week, day) = GetISOWeekDate(utcNow);

                string cacheKey = BuildCacheKey(quizType, langCode, year, week);
                lock (_cacheLock)
                {
                    if (_cache.TryGetValue(cacheKey, out IVXWeeklyQuizResult cached) && cached.Success)
                    {
                        Debug.Log($"[IVXWeeklyQuizService] ✓ Returning cached {quizType}");
                        return cached;
                    }
                }

                Debug.Log($"[IVXWeeklyQuizService] Fetching {quizType} for week {week} of {year}, lang={langCode}");
                
                var result = await FetchWithFallbackAsync(quizType, utcNow, langCode);

                if (result.Success)
                {
                    lock (_cacheLock)
                    {
                        _cache[cacheKey] = result;
                    }
                }

                return result;
            }
            finally
            {
                _isFetchingByType[typeIndex] = false;
            }
        }

        #endregion

        #region Private Methods - Fetch Logic

        private static string BuildCacheKey(IVXWeeklyQuizType quizType, string langCode, int year, int week)
        {
            return $"{quizType}_{year}_{week}_{langCode}";
        }

        private async Task<IVXWeeklyQuizResult> FetchWithFallbackAsync(IVXWeeklyQuizType quizType, DateTime targetDate, string langCode)
        {
            langCode = NormalizeLanguageCode(langCode);
            string quizPrefix = GetQuizPrefix(quizType);

            // Try current week first
            var result = await TryFetchForWeekAsync(quizType, quizPrefix, targetDate, langCode, isFallback: false);
            if (result.Success)
            {
                return result;
            }

            // Fallback to previous weeks
            Debug.Log($"[IVXWeeklyQuizService] {quizType} not found for current week, trying previous weeks...");
            
            for (int weeksBack = 1; weeksBack <= MAX_FALLBACK_WEEKS; weeksBack++)
            {
                DateTime fallbackWeekDate = targetDate.AddDays(-7 * weeksBack);
                result = await TryFetchForWeekAsync(quizType, quizPrefix, fallbackWeekDate, langCode, isFallback: true);
                if (result.Success)
                {
                    var (y, w, d) = GetISOWeekDate(fallbackWeekDate);
                    Debug.Log($"[IVXWeeklyQuizService] ✓ Found {quizType} from week {w} ({weeksBack} week(s) ago)");
                    return result;
                }
            }

            // Language fallback to English
            if (!langCode.Equals("en", StringComparison.OrdinalIgnoreCase))
            {
                Debug.LogWarning($"[IVXWeeklyQuizService] No {quizType} for '{langCode}', trying English fallback...");
                return await FetchWithFallbackAsync(quizType, targetDate, "en");
            }

            string errorMsg = $"No {quizType} quiz available for the past {MAX_FALLBACK_WEEKS} weeks";
            Debug.LogError($"[IVXWeeklyQuizService] {errorMsg}");
            return CreateErrorResult(quizType, errorMsg);
        }

        /// <summary>
        /// Try to fetch quiz for a specific week by trying all 7 possible upload days.
        /// S3 files are named with the day they were uploaded (1-7).
        /// </summary>
        private async Task<IVXWeeklyQuizResult> TryFetchForWeekAsync(
            IVXWeeklyQuizType quizType,
            string prefix,
            DateTime anyDateInWeek,
            string langCode,
            bool isFallback)
        {
            var (year, week, currentDay) = GetISOWeekDate(anyDateInWeek);

            if (EnableVerboseLogging)
            {
                Debug.Log($"[IVXWeeklyQuizService] Trying week {week} of {year}...");
            }

            // Try each possible upload day (7=Sunday to 1=Monday, most recent first)
            for (int uploadDay = 7; uploadDay >= 1; uploadDay--)
            {
                var result = await TryFetchFromUrlAsync(quizType, prefix, year, week, uploadDay, langCode, isFallback);
                if (result.Success)
                {
                    return result;
                }
            }

            return CreateErrorResult(quizType, $"No quiz found for week {week} of {year}");
        }

        /// <summary>
        /// Try to fetch quiz from a specific URL.
        /// </summary>
        private async Task<IVXWeeklyQuizResult> TryFetchFromUrlAsync(
            IVXWeeklyQuizType quizType,
            string prefix,
            int year,
            int week,
            int dayOfWeek,
            string langCode,
            bool isFallback)
        {
            string url = BuildQuizUrlDirect(prefix, langCode, year, week, dayOfWeek);

            if (EnableVerboseLogging)
            {
                Debug.Log($"[IVXWeeklyQuizService] Trying: {url}");
            }

            try
            {
                using (UnityWebRequest request = UnityWebRequest.Get(url))
                {
                    request.timeout = isFallback ? FALLBACK_TIMEOUT_SECONDS : REQUEST_TIMEOUT_SECONDS;
                    var operation = request.SendWebRequest();

                    while (!operation.isDone)
                        await Task.Yield();

                    if (request.result != UnityWebRequest.Result.Success)
                    {
                        long code = request.responseCode;
                        if (code != 404 && code != 403 && EnableVerboseLogging)
                        {
                            Debug.LogWarning($"[IVXWeeklyQuizService] HTTP {code} for {url}: {request.error}");
                        }
                        return CreateErrorResult(quizType, $"HTTP {code}: {request.error}");
                    }

                    string jsonText = request.downloadHandler.text;
                    if (string.IsNullOrEmpty(jsonText))
                    {
                        return CreateErrorResult(quizType, "Empty response from server");
                    }

                    Debug.Log($"[IVXWeeklyQuizService] ✓ Successfully fetched from: {url}");
                    
                    string dateKey = $"{year}-W{week}";
                    return ParseAndConvert(quizType, jsonText, dateKey, langCode, url);
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"[IVXWeeklyQuizService] Exception fetching {url}: {ex.Message}");
                return CreateErrorResult(quizType, $"Exception: {ex.Message}");
            }
        }

        #endregion

        #region URL Building

        /// <summary>
        /// Builds quiz URL from explicit year, week, and day values.
        /// URL Format: {baseUrl}{year}-{dayOfWeek}-{weekNumber}-{type}_{lang}.json
        /// Example: 2026-6-5-emoji_en.json
        /// </summary>
        private string BuildQuizUrlDirect(string prefix, string langCode, int year, int week, int dayOfWeek)
        {
            return $"{S3_BASE_URL}{year}-{dayOfWeek}-{week}-{prefix}_{langCode}{QUIZ_FILE_EXTENSION}";
        }

        private static string GetQuizPrefix(IVXWeeklyQuizType quizType)
        {
            return quizType switch
            {
                IVXWeeklyQuizType.Fortune => "fortune",
                IVXWeeklyQuizType.Emoji => "emoji",
                IVXWeeklyQuizType.Prediction => "prediction",
                IVXWeeklyQuizType.Health => "health",
                _ => "fortune"
            };
        }

        #endregion

        #region ISO Week Date Calculation

        /// <summary>
        /// Calculates ISO 8601 week date components from a DateTime.
        /// Returns (year, weekNumber, dayOfWeek) where:
        /// - year: ISO week-numbering year
        /// - weekNumber: 1-53
        /// - dayOfWeek: 1=Monday, 2=Tuesday, ..., 7=Sunday
        /// </summary>
        private static (int year, int week, int day) GetISOWeekDate(DateTime date)
        {
            int isoDayOfWeek = date.DayOfWeek == DayOfWeek.Sunday ? 7 : (int)date.DayOfWeek;
            DateTime thursday = date.AddDays(4 - isoDayOfWeek);
            int isoYear = thursday.Year;

            DateTime jan4 = new DateTime(isoYear, 1, 4);
            int jan4DayOfWeek = jan4.DayOfWeek == DayOfWeek.Sunday ? 7 : (int)jan4.DayOfWeek;
            DateTime startOfWeek1 = jan4.AddDays(1 - jan4DayOfWeek);

            int weekNumber = ((thursday - startOfWeek1).Days / 7) + 1;

            return (isoYear, weekNumber, isoDayOfWeek);
        }

        #endregion

        #region JSON Parsing & Conversion

        /// <summary>
        /// Parses the raw multilingual JSON and converts to SDK-friendly models.
        /// </summary>
        private IVXWeeklyQuizResult ParseAndConvert(IVXWeeklyQuizType quizType, string jsonText, string dateStr, string langCode, string sourceUrl)
        {
            try
            {
                // First try to parse as the new multilingual format
                IVXWeeklyQuizRaw rawData = ParseRawJson(jsonText);
                
                if (rawData != null && rawData.today_quiz?.questions != null && rawData.today_quiz.questions.Count > 0)
                {
                    // Convert from raw multilingual format to SDK-friendly format
                    object quizData = ConvertRawToQuizData(quizType, rawData, langCode);
                    
                    if (quizData != null)
                    {
                        string validationError = ValidateQuizData(quizType, quizData);
                        if (string.IsNullOrEmpty(validationError))
                        {
                            Debug.Log($"[IVXWeeklyQuizService] ✓ Parsed multilingual format with {rawData.today_quiz.questions.Count} questions");
                            return new IVXWeeklyQuizResult
                            {
                                Success = true,
                                QuizType = quizType,
                                QuizData = quizData,
                                FetchedDate = dateStr,
                                LanguageCode = langCode,
                                SourceUrl = sourceUrl
                            };
                        }
                        Debug.LogWarning($"[IVXWeeklyQuizService] Converted data failed validation: {validationError}");
                    }
                }

                // Fallback: try parsing as legacy format
                return ParseLegacyFormat(quizType, jsonText, dateStr, langCode, sourceUrl);
            }
            catch (Exception ex)
            {
                Debug.LogError($"[IVXWeeklyQuizService] Parse error: {ex.Message}");
                return ParseLegacyFormat(quizType, jsonText, dateStr, langCode, sourceUrl);
            }
        }

        /// <summary>
        /// Parses the raw multilingual JSON from S3.
        /// Uses a custom parser since Unity's JsonUtility doesn't handle Dictionary well.
        /// </summary>
        private IVXWeeklyQuizRaw ParseRawJson(string jsonText)
        {
            try
            {
                // Unity's JsonUtility doesn't support Dictionary<string, T>
                // We need to manually extract the relevant data
                
                var raw = new IVXWeeklyQuizRaw();
                
                // Extract quizId
                raw.quizId = ExtractJsonString(jsonText, "quizId");
                
                // Extract topic (localized)
                raw.topic = ExtractLocalizedString(jsonText, "topic");
                
                // Extract today_quiz questions
                raw.today_quiz = ExtractTodayQuiz(jsonText);
                
                return raw;
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[IVXWeeklyQuizService] Raw JSON parse failed: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Extracts a simple string value from JSON.
        /// </summary>
        private string ExtractJsonString(string json, string key)
        {
            string searchPattern = $"\"{key}\":";
            int keyStart = json.IndexOf(searchPattern);
            if (keyStart < 0) return null;
            
            int valueStart = keyStart + searchPattern.Length;
            while (valueStart < json.Length && char.IsWhiteSpace(json[valueStart]))
                valueStart++;
            
            if (valueStart >= json.Length || json[valueStart] != '"')
                return null;
            
            valueStart++; // Skip opening quote
            int valueEnd = json.IndexOf('"', valueStart);
            if (valueEnd < 0) return null;
            
            return json.Substring(valueStart, valueEnd - valueStart);
        }

        /// <summary>
        /// Extracts a localized string object from JSON.
        /// </summary>
        private LocalizedString ExtractLocalizedString(string json, string key)
        {
            var result = new LocalizedString();
            
            // Find the object
            string searchPattern = $"\"{key}\":";
            int keyStart = json.IndexOf(searchPattern);
            if (keyStart < 0) return result;
            
            int objStart = json.IndexOf('{', keyStart);
            if (objStart < 0) return result;
            
            int objEnd = FindMatchingBrace(json, objStart);
            if (objEnd < 0) return result;
            
            string objJson = json.Substring(objStart, objEnd - objStart + 1);
            
            // Extract each language
            result.en = ExtractJsonString(objJson, "en");
            result.hi = ExtractJsonString(objJson, "hi");
            result.ar = ExtractJsonString(objJson, "ar");
            result.fr = ExtractJsonString(objJson, "fr");
            result.de = ExtractJsonString(objJson, "de");
            result.es = ExtractJsonString(objJson, "es");
            result.ru = ExtractJsonString(objJson, "ru");
            result.ja = ExtractJsonString(objJson, "ja");
            result.ko = ExtractJsonString(objJson, "ko");
            result.id = ExtractJsonString(objJson, "id");
            
            return result;
        }

        /// <summary>
        /// Extracts today_quiz section from JSON.
        /// </summary>
        private IVXRawTodayQuiz ExtractTodayQuiz(string json)
        {
            var result = new IVXRawTodayQuiz { questions = new List<IVXRawQuestion>() };
            
            string searchPattern = "\"today_quiz\":";
            int keyStart = json.IndexOf(searchPattern);
            if (keyStart < 0) return result;
            
            int objStart = json.IndexOf('{', keyStart);
            if (objStart < 0) return result;
            
            int objEnd = FindMatchingBrace(json, objStart);
            if (objEnd < 0) return result;
            
            string todayQuizJson = json.Substring(objStart, objEnd - objStart + 1);
            
            // Find questions array
            string questionsPattern = "\"questions\":";
            int questionsStart = todayQuizJson.IndexOf(questionsPattern);
            if (questionsStart < 0) return result;
            
            int arrStart = todayQuizJson.IndexOf('[', questionsStart);
            if (arrStart < 0) return result;
            
            int arrEnd = FindMatchingBracket(todayQuizJson, arrStart);
            if (arrEnd < 0) return result;
            
            string questionsJson = todayQuizJson.Substring(arrStart, arrEnd - arrStart + 1);
            
            // Extract individual questions
            result.questions = ExtractQuestions(questionsJson);
            
            return result;
        }

        /// <summary>
        /// Extracts questions from the questions array JSON.
        /// </summary>
        private List<IVXRawQuestion> ExtractQuestions(string questionsArrayJson)
        {
            var questions = new List<IVXRawQuestion>();
            
            int pos = 1; // Skip opening [
            while (pos < questionsArrayJson.Length)
            {
                // Find next question object
                int objStart = questionsArrayJson.IndexOf('{', pos);
                if (objStart < 0) break;
                
                int objEnd = FindMatchingBrace(questionsArrayJson, objStart);
                if (objEnd < 0) break;
                
                string questionJson = questionsArrayJson.Substring(objStart, objEnd - objStart + 1);
                var question = ExtractSingleQuestion(questionJson);
                if (question != null)
                {
                    questions.Add(question);
                }
                
                pos = objEnd + 1;
            }
            
            return questions;
        }

        /// <summary>
        /// Extracts a single question from JSON.
        /// </summary>
        private IVXRawQuestion ExtractSingleQuestion(string questionJson)
        {
            var question = new IVXRawQuestion
            {
                id = ExtractJsonString(questionJson, "id"),
                question = ExtractJsonString(questionJson, "question"),
                explanation = ExtractJsonString(questionJson, "explanation"),
                correct_answer = ExtractJsonInt(questionJson, "correct_answer"),
                options = ExtractStringArray(questionJson, "options"),
                hints = ExtractHints(questionJson),
                languages = ExtractLanguages(questionJson)
            };
            
            return question;
        }

        private int ExtractJsonInt(string json, string key)
        {
            string searchPattern = $"\"{key}\":";
            int keyStart = json.IndexOf(searchPattern);
            if (keyStart < 0) return 0;
            
            int valueStart = keyStart + searchPattern.Length;
            while (valueStart < json.Length && char.IsWhiteSpace(json[valueStart]))
                valueStart++;
            
            int valueEnd = valueStart;
            while (valueEnd < json.Length && (char.IsDigit(json[valueEnd]) || json[valueEnd] == '-'))
                valueEnd++;
            
            if (int.TryParse(json.Substring(valueStart, valueEnd - valueStart), out int result))
                return result;
            
            return 0;
        }

        private List<string> ExtractStringArray(string json, string key)
        {
            var result = new List<string>();
            
            string searchPattern = $"\"{key}\":";
            int keyStart = json.IndexOf(searchPattern);
            if (keyStart < 0) return result;
            
            int arrStart = json.IndexOf('[', keyStart);
            if (arrStart < 0) return result;
            
            int arrEnd = FindMatchingBracket(json, arrStart);
            if (arrEnd < 0) return result;
            
            string arrayJson = json.Substring(arrStart + 1, arrEnd - arrStart - 1);
            
            // Simple string extraction from array
            int pos = 0;
            while (pos < arrayJson.Length)
            {
                int strStart = arrayJson.IndexOf('"', pos);
                if (strStart < 0) break;
                
                int strEnd = FindClosingQuote(arrayJson, strStart + 1);
                if (strEnd < 0) break;
                
                string value = arrayJson.Substring(strStart + 1, strEnd - strStart - 1);
                value = UnescapeJsonString(value);
                result.Add(value);
                
                pos = strEnd + 1;
            }
            
            return result;
        }

        private IVXRawHints ExtractHints(string json)
        {
            string searchPattern = "\"hints\":";
            int keyStart = json.IndexOf(searchPattern);
            if (keyStart < 0) return null;
            
            int objStart = json.IndexOf('{', keyStart);
            if (objStart < 0) return null;
            
            int objEnd = FindMatchingBrace(json, objStart);
            if (objEnd < 0) return null;
            
            string hintsJson = json.Substring(objStart, objEnd - objStart + 1);
            
            return new IVXRawHints
            {
                text = ExtractJsonString(hintsJson, "text"),
                audio_url = ExtractJsonString(hintsJson, "audio_url")
            };
        }

        private Dictionary<string, IVXRawQuestionLanguage> ExtractLanguages(string json)
        {
            var result = new Dictionary<string, IVXRawQuestionLanguage>();
            
            string searchPattern = "\"languages\":";
            int keyStart = json.IndexOf(searchPattern);
            if (keyStart < 0) return result;
            
            int objStart = json.IndexOf('{', keyStart);
            if (objStart < 0) return result;
            
            int objEnd = FindMatchingBrace(json, objStart);
            if (objEnd < 0) return result;
            
            string languagesJson = json.Substring(objStart, objEnd - objStart + 1);
            
            // Extract common language codes
            string[] langCodes = { "en", "hi", "ar", "fr", "de", "es-419", "pt-BR", "ru", "zh-Hans", "ja", "ko", "id" };
            
            foreach (var langCode in langCodes)
            {
                var langData = ExtractLanguageData(languagesJson, langCode);
                if (langData != null)
                {
                    result[langCode] = langData;
                }
            }
            
            return result;
        }

        private IVXRawQuestionLanguage ExtractLanguageData(string json, string langCode)
        {
            string searchPattern = $"\"{langCode}\":";
            int keyStart = json.IndexOf(searchPattern);
            if (keyStart < 0) return null;
            
            int objStart = json.IndexOf('{', keyStart);
            if (objStart < 0) return null;
            
            int objEnd = FindMatchingBrace(json, objStart);
            if (objEnd < 0) return null;
            
            string langJson = json.Substring(objStart, objEnd - objStart + 1);
            
            return new IVXRawQuestionLanguage
            {
                question = ExtractJsonString(langJson, "question"),
                explanation = ExtractJsonString(langJson, "explanation"),
                options = ExtractStringArray(langJson, "options"),
                hints = ExtractHints(langJson),
                question_audio_url = ExtractJsonString(langJson, "question_audio_url"),
                options_audio_urls = ExtractStringArray(langJson, "options_audio_urls")
            };
        }

        private int FindMatchingBrace(string json, int start)
        {
            if (start >= json.Length || json[start] != '{') return -1;
            
            int depth = 1;
            int pos = start + 1;
            bool inString = false;
            
            while (pos < json.Length && depth > 0)
            {
                char c = json[pos];
                
                if (inString)
                {
                    if (c == '\\' && pos + 1 < json.Length)
                    {
                        pos += 2;
                        continue;
                    }
                    if (c == '"')
                        inString = false;
                }
                else
                {
                    if (c == '"')
                        inString = true;
                    else if (c == '{')
                        depth++;
                    else if (c == '}')
                        depth--;
                }
                
                pos++;
            }
            
            return depth == 0 ? pos - 1 : -1;
        }

        private int FindMatchingBracket(string json, int start)
        {
            if (start >= json.Length || json[start] != '[') return -1;
            
            int depth = 1;
            int pos = start + 1;
            bool inString = false;
            
            while (pos < json.Length && depth > 0)
            {
                char c = json[pos];
                
                if (inString)
                {
                    if (c == '\\' && pos + 1 < json.Length)
                    {
                        pos += 2;
                        continue;
                    }
                    if (c == '"')
                        inString = false;
                }
                else
                {
                    if (c == '"')
                        inString = true;
                    else if (c == '[')
                        depth++;
                    else if (c == ']')
                        depth--;
                }
                
                pos++;
            }
            
            return depth == 0 ? pos - 1 : -1;
        }

        private int FindClosingQuote(string json, int start)
        {
            int pos = start;
            while (pos < json.Length)
            {
                if (json[pos] == '\\' && pos + 1 < json.Length)
                {
                    pos += 2;
                    continue;
                }
                if (json[pos] == '"')
                    return pos;
                pos++;
            }
            return -1;
        }

        private string UnescapeJsonString(string str)
        {
            if (string.IsNullOrEmpty(str)) return str;
            
            return str
                .Replace("\\\"", "\"")
                .Replace("\\\\", "\\")
                .Replace("\\n", "\n")
                .Replace("\\r", "\r")
                .Replace("\\t", "\t");
        }

        /// <summary>
        /// Converts raw multilingual data to SDK-friendly quiz data.
        /// </summary>
        private object ConvertRawToQuizData(IVXWeeklyQuizType quizType, IVXWeeklyQuizRaw raw, string langCode)
        {
            return quizType switch
            {
                IVXWeeklyQuizType.Fortune => IVXWeeklyQuizConverter.ToFortuneQuizData(raw, langCode),
                IVXWeeklyQuizType.Emoji => IVXWeeklyQuizConverter.ToEmojiQuizData(raw, langCode),
                IVXWeeklyQuizType.Prediction => IVXWeeklyQuizConverter.ToPredictionQuizData(raw, langCode),
                IVXWeeklyQuizType.Health => IVXWeeklyQuizConverter.ToHealthQuizData(raw, langCode),
                _ => null
            };
        }

        /// <summary>
        /// Fallback: tries to parse as the legacy format.
        /// </summary>
        private IVXWeeklyQuizResult ParseLegacyFormat(IVXWeeklyQuizType quizType, string jsonText, string dateStr, string langCode, string sourceUrl)
        {
            try
            {
                object quizData = quizType switch
                {
                    IVXWeeklyQuizType.Fortune => JsonUtility.FromJson<IVXFortuneQuizData>(jsonText),
                    IVXWeeklyQuizType.Emoji => JsonUtility.FromJson<IVXEmojiQuizData>(jsonText),
                    IVXWeeklyQuizType.Prediction => JsonUtility.FromJson<IVXPredictionQuizData>(jsonText),
                    IVXWeeklyQuizType.Health => JsonUtility.FromJson<IVXHealthQuizData>(jsonText),
                    _ => null
                };

                if (quizData == null)
                {
                    return CreateErrorResult(quizType, $"Failed to parse {quizType} quiz JSON");
                }

                string validationError = ValidateQuizData(quizType, quizData);
                if (!string.IsNullOrEmpty(validationError))
                {
                    return CreateErrorResult(quizType, validationError);
                }

                return new IVXWeeklyQuizResult
                {
                    Success = true,
                    QuizType = quizType,
                    QuizData = quizData,
                    FetchedDate = dateStr,
                    LanguageCode = langCode,
                    SourceUrl = sourceUrl
                };
            }
            catch (Exception ex)
            {
                Debug.LogError($"[IVXWeeklyQuizService] Legacy parse error: {ex.Message}");
                return CreateErrorResult(quizType, $"Parse error: {ex.Message}");
            }
        }

        private static string ValidateQuizData(IVXWeeklyQuizType quizType, object quizData)
        {
            string error = null;
            switch (quizType)
            {
                case IVXWeeklyQuizType.Fortune:
                    var fortune = quizData as IVXFortuneQuizData;
                    if (fortune == null) return "Fortune quiz is null";
                    return fortune.IsValid(out error) ? null : error;
                case IVXWeeklyQuizType.Emoji:
                    var emoji = quizData as IVXEmojiQuizData;
                    if (emoji == null) return "Emoji quiz is null";
                    return emoji.IsValid(out error) ? null : error;
                case IVXWeeklyQuizType.Prediction:
                    var prediction = quizData as IVXPredictionQuizData;
                    if (prediction == null) return "Prediction quiz is null";
                    return prediction.IsValid(out error) ? null : error;
                case IVXWeeklyQuizType.Health:
                    var health = quizData as IVXHealthQuizData;
                    if (health == null) return "Health quiz is null";
                    return health.IsValid(out error) ? null : error;
                default:
                    return "Unknown quiz type";
            }
        }

        #endregion

        #region Language Helpers

        private static string GetCurrentLanguageCode()
        {
            return NormalizeLanguageCode(PlayerPrefs.GetString("SelectedLanguage", "en"));
        }

        private static string NormalizeLanguageCode(string langCode)
        {
            if (string.IsNullOrEmpty(langCode)) return "en";

            string lower = langCode.ToLowerInvariant();

            foreach (var locale in SUPPORTED_LOCALES)
            {
                if (lower.Equals(locale, StringComparison.Ordinal))
                    return locale;
            }

            if (lower.StartsWith("zh")) return "zh";
            if (lower.StartsWith("es")) return "es";
            if (lower.StartsWith("pt")) return "pt";
            if (lower.StartsWith("ar")) return "ar";
            if (lower.StartsWith("fr")) return "fr";
            if (lower.StartsWith("de")) return "de";

            int dashIndex = lower.IndexOf('-');
            if (dashIndex > 0)
            {
                string baseLang = lower.Substring(0, dashIndex);
                foreach (var locale in SUPPORTED_LOCALES)
                {
                    if (baseLang.Equals(locale, StringComparison.Ordinal))
                        return locale;
                }
            }

            return "en";
        }

        #endregion

        #region Helpers

        private static IVXWeeklyQuizResult CreateErrorResult(IVXWeeklyQuizType quizType, string errorMessage)
        {
            return new IVXWeeklyQuizResult
            {
                Success = false,
                QuizType = quizType,
                ErrorMessage = errorMessage
            };
        }

        #endregion
    }
}
