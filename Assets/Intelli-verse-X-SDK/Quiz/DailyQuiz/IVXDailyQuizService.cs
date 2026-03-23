// IVXDailyQuizService.cs
// Service for fetching daily quiz data from S3
// Supports both standard and premium daily quizzes with dynamic date-based URLs

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace IntelliVerseX.Quiz.DailyQuiz
{
    /// <summary>
    /// Service for fetching daily quiz data from S3.
    /// Handles both Daily Quiz and Premium Daily Quiz types.
    /// 
    /// URL Patterns:
    ///   Daily:   https://intelli-verse-x-media.s3.us-east-1.amazonaws.com/quiz-verse/daily/dailyquiz-{YYYY}-{MM}-{DD}.json
    ///   Premium: https://intelli-verse-x-media.s3.us-east-1.amazonaws.com/quiz-verse/daily/dailyquiz-prem-{lang}-{YYYY}-{MM}-{DD}.json
    /// 
    /// Examples:
    ///   dailyquiz-2026-02-10.json
    ///   dailyquiz-prem-hi-2026-01-19.json
    /// </summary>
    public class IVXDailyQuizService
    {
        #region Constants

        private const string S3_BASE_URL = "https://intelli-verse-x-media.s3.us-east-1.amazonaws.com/quiz-verse/daily/";
        private const string QUIZ_FILE_EXTENSION = ".json";
        private const int MAX_FALLBACK_DAYS = 7;
        private const int REQUEST_TIMEOUT_SECONDS = 15;
        private const int FALLBACK_TIMEOUT_SECONDS = 10;

        private static readonly string[] SUPPORTED_LOCALES = { "en", "es", "ar", "fr", "de", "pt", "hi", "zh", "ja", "ko", "id", "ru" };

        #endregion

        #region Fields

        private readonly Dictionary<string, IVXDailyQuizResult> _cache = new Dictionary<string, IVXDailyQuizResult>();
        private bool _isFetchingDaily;
        private bool _isFetchingPremium;
        private readonly object _cacheLock = new object();

#if UNITY_EDITOR
        public bool EnableVerboseLogging { get; set; } = true;
#else
        public bool EnableVerboseLogging { get; set; } = false;
#endif

        #endregion

        #region Public API

        /// <summary>
        /// Fetches today's daily quiz.
        /// </summary>
        public Task<IVXDailyQuizResult> FetchDailyQuizAsync() => FetchDailyQuizAsync(DateTime.UtcNow);

        /// <summary>
        /// Fetches the daily quiz for a specific date.
        /// </summary>
        public Task<IVXDailyQuizResult> FetchDailyQuizAsync(DateTime date) => FetchQuizInternalAsync(IVXDailyQuizType.Daily, date);

        /// <summary>
        /// Fetches today's premium daily quiz.
        /// </summary>
        public Task<IVXDailyQuizResult> FetchPremiumDailyQuizAsync() => FetchPremiumDailyQuizAsync(DateTime.UtcNow);

        /// <summary>
        /// Fetches the premium daily quiz for a specific date.
        /// </summary>
        public Task<IVXDailyQuizResult> FetchPremiumDailyQuizAsync(DateTime date) => FetchQuizInternalAsync(IVXDailyQuizType.DailyPremium, date);

        /// <summary>
        /// Gets the URL that would be used to fetch a daily quiz for a specific date.
        /// </summary>
        public string GetDailyQuizUrl(DateTime date)
        {
            return BuildDailyQuizUrl(date);
        }

        /// <summary>
        /// Gets the URL that would be used to fetch a premium daily quiz for a specific date.
        /// </summary>
        public string GetPremiumQuizUrl(DateTime date, string langCode = "en")
        {
            return BuildPremiumQuizUrl(date, langCode);
        }

        /// <summary>
        /// Gets the date key for a specific DateTime (YYYY-MM-DD format).
        /// </summary>
        public static string GetDateKey(DateTime date)
        {
            return date.ToString("yyyy-MM-dd");
        }

        /// <summary>
        /// Debug helper: Logs URL patterns for multiple days.
        /// </summary>
        public void LogUrlPatternForDays(IVXDailyQuizType quizType, int daysToShow = 7)
        {
            DateTime today = DateTime.UtcNow;
            string langCode = GetCurrentLanguageCode();

            Debug.Log($"[IVXDailyQuizService] === URL Pattern Debug for {quizType} ===");
            Debug.Log($"[IVXDailyQuizService] Today: {today:yyyy-MM-dd} ({today.DayOfWeek})");
            Debug.Log($"[IVXDailyQuizService] Language: {langCode}");

            for (int i = 0; i < daysToShow; i++)
            {
                DateTime targetDate = today.AddDays(-i);
                string url = quizType == IVXDailyQuizType.DailyPremium
                    ? BuildPremiumQuizUrl(targetDate, langCode)
                    : BuildDailyQuizUrl(targetDate);
                Debug.Log($"[IVXDailyQuizService] Day -{i}: {url}");
            }
        }

        public void ClearCache()
        {
            lock (_cacheLock)
            {
                _cache.Clear();
            }
            Debug.Log("[IVXDailyQuizService] Cache cleared");
        }

        public void ClearCache(IVXDailyQuizType quizType)
        {
            string prefix = quizType == IVXDailyQuizType.DailyPremium ? "prem_" : "daily_";
            lock (_cacheLock)
            {
                var keysToRemove = new List<string>();
                foreach (var key in _cache.Keys)
                {
                    if (key.StartsWith(prefix))
                        keysToRemove.Add(key);
                }
                foreach (var key in keysToRemove)
                    _cache.Remove(key);
            }
        }

        #endregion

        #region Private Methods - Fetch Logic

        private async Task<IVXDailyQuizResult> FetchQuizInternalAsync(IVXDailyQuizType quizType, DateTime targetDate)
        {
            bool isPremium = quizType == IVXDailyQuizType.DailyPremium;

            if (isPremium && _isFetchingPremium)
            {
                Debug.LogWarning("[IVXDailyQuizService] Already fetching Premium quiz — skipping duplicate request");
                return CreateErrorResult(quizType, "Fetch already in progress");
            }
            if (!isPremium && _isFetchingDaily)
            {
                Debug.LogWarning("[IVXDailyQuizService] Already fetching Daily quiz — skipping duplicate request");
                return CreateErrorResult(quizType, "Fetch already in progress");
            }

            if (isPremium)
                _isFetchingPremium = true;
            else
                _isFetchingDaily = true;

            try
            {
                string langCode = GetCurrentLanguageCode();
                string dateKey = GetDateKey(targetDate);
                string cacheKey = BuildCacheKey(quizType, langCode, dateKey);

                lock (_cacheLock)
                {
                    if (_cache.TryGetValue(cacheKey, out IVXDailyQuizResult cached) && cached.Success)
                    {
                        Debug.Log($"[IVXDailyQuizService] ✓ Returning cached {quizType} for {dateKey}");
                        return cached;
                    }
                }

                Debug.Log($"[IVXDailyQuizService] Fetching {quizType} for {dateKey}, lang={langCode}");

                var result = await FetchWithFallbackAsync(quizType, targetDate, langCode);

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
                if (isPremium)
                    _isFetchingPremium = false;
                else
                    _isFetchingDaily = false;
            }
        }

        private static string BuildCacheKey(IVXDailyQuizType quizType, string langCode, string dateKey)
        {
            string prefix = quizType == IVXDailyQuizType.DailyPremium ? "prem" : "daily";
            return $"{prefix}_{dateKey}_{langCode}";
        }

        private async Task<IVXDailyQuizResult> FetchWithFallbackAsync(IVXDailyQuizType quizType, DateTime targetDate, string langCode)
        {
            langCode = NormalizeLanguageCode(langCode);
            bool isPremium = quizType == IVXDailyQuizType.DailyPremium;

            var result = await TryFetchForDateAsync(quizType, targetDate, langCode, isFallback: false);
            if (result.Success)
            {
                return result;
            }

            Debug.Log($"[IVXDailyQuizService] {quizType} not found for today, trying previous days...");

            for (int daysBack = 1; daysBack <= MAX_FALLBACK_DAYS; daysBack++)
            {
                DateTime fallbackDate = targetDate.AddDays(-daysBack);
                result = await TryFetchForDateAsync(quizType, fallbackDate, langCode, isFallback: true);
                if (result.Success)
                {
                    Debug.Log($"[IVXDailyQuizService] ✓ Found {quizType} from {daysBack} day(s) ago ({GetDateKey(fallbackDate)})");
                    return result;
                }
            }

            if (!langCode.Equals("en", StringComparison.OrdinalIgnoreCase))
            {
                Debug.LogWarning($"[IVXDailyQuizService] No {quizType} for '{langCode}', trying English fallback...");
                return await FetchWithFallbackAsync(quizType, targetDate, "en");
            }

            string errorMsg = $"No {quizType} quiz available for the past {MAX_FALLBACK_DAYS} days";
            Debug.LogError($"[IVXDailyQuizService] {errorMsg}");
            return CreateErrorResult(quizType, errorMsg);
        }

        private async Task<IVXDailyQuizResult> TryFetchForDateAsync(
            IVXDailyQuizType quizType,
            DateTime date,
            string langCode,
            bool isFallback)
        {
            bool isPremium = quizType == IVXDailyQuizType.DailyPremium;
            string url = isPremium ? BuildPremiumQuizUrl(date, langCode) : BuildDailyQuizUrl(date);
            string dateKey = GetDateKey(date);

            if (EnableVerboseLogging)
            {
                Debug.Log($"[IVXDailyQuizService] Trying: {url}");
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
                            Debug.LogWarning($"[IVXDailyQuizService] HTTP {code} for {url}: {request.error}");
                        }
                        return CreateErrorResult(quizType, $"HTTP {code}: {request.error}");
                    }

                    string jsonText = request.downloadHandler.text;
                    if (string.IsNullOrEmpty(jsonText))
                    {
                        return CreateErrorResult(quizType, "Empty response from server");
                    }

                    Debug.Log($"[IVXDailyQuizService] ✓ Successfully fetched from: {url}");

                    return ParseAndConvert(quizType, jsonText, dateKey, langCode, url);
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"[IVXDailyQuizService] Exception fetching {url}: {ex.Message}");
                return CreateErrorResult(quizType, $"Exception: {ex.Message}");
            }
        }

        #endregion

        #region URL Building

        /// <summary>
        /// Builds daily quiz URL from date.
        /// Format: dailyquiz-{YYYY}-{MM}-{DD}.json
        /// Example: dailyquiz-2026-02-10.json
        /// </summary>
        private string BuildDailyQuizUrl(DateTime date)
        {
            return $"{S3_BASE_URL}dailyquiz-{date:yyyy-MM-dd}{QUIZ_FILE_EXTENSION}";
        }

        /// <summary>
        /// Builds premium daily quiz URL from date and language.
        /// Format: dailyquiz-prem-{lang}-{YYYY}-{MM}-{DD}.json
        /// Example: dailyquiz-prem-hi-2026-01-19.json
        /// </summary>
        private string BuildPremiumQuizUrl(DateTime date, string langCode)
        {
            return $"{S3_BASE_URL}dailyquiz-prem-{langCode}-{date:yyyy-MM-dd}{QUIZ_FILE_EXTENSION}";
        }

        #endregion

        #region JSON Parsing & Conversion

        private IVXDailyQuizResult ParseAndConvert(IVXDailyQuizType quizType, string jsonText, string dateStr, string langCode, string sourceUrl)
        {
            try
            {
                bool isPremium = quizType == IVXDailyQuizType.DailyPremium;

                if (isPremium)
                {
                    return ParsePremiumQuiz(jsonText, dateStr, langCode, sourceUrl);
                }
                else
                {
                    return ParseDailyQuiz(jsonText, dateStr, langCode, sourceUrl);
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"[IVXDailyQuizService] Parse error: {ex.Message}");
                return CreateErrorResult(quizType, $"Parse error: {ex.Message}");
            }
        }

        private IVXDailyQuizResult ParseDailyQuiz(string jsonText, string dateStr, string langCode, string sourceUrl)
        {
            try
            {
                IVXDailyQuizRaw rawData = ParseRawDailyJson(jsonText);

                if (rawData != null && rawData.today_quiz?.questions != null && rawData.today_quiz.questions.Count > 0)
                {
                    IVXDailyQuizData quizData = IVXDailyQuizConverter.ToDailyQuizData(rawData, langCode, dateStr);

                    if (quizData != null)
                    {
                        string validationError;
                        if (quizData.IsValid(out validationError))
                        {
                            Debug.Log($"[IVXDailyQuizService] ✓ Parsed daily quiz with {quizData.QuestionCount} questions");
                            return new IVXDailyQuizResult
                            {
                                Success = true,
                                QuizType = IVXDailyQuizType.Daily,
                                QuizData = quizData,
                                FetchedDate = dateStr,
                                LanguageCode = langCode,
                                SourceUrl = sourceUrl
                            };
                        }
                        Debug.LogWarning($"[IVXDailyQuizService] Validation failed: {validationError}");
                    }
                }

                return CreateErrorResult(IVXDailyQuizType.Daily, "Failed to parse daily quiz JSON");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[IVXDailyQuizService] Daily parse error: {ex.Message}");
                return CreateErrorResult(IVXDailyQuizType.Daily, $"Parse error: {ex.Message}");
            }
        }

        private IVXDailyQuizResult ParsePremiumQuiz(string jsonText, string dateStr, string langCode, string sourceUrl)
        {
            try
            {
                IVXDailyPremiumQuizRaw rawData = ParseRawPremiumJson(jsonText);

                if (rawData != null && rawData.premium_questions != null && rawData.premium_questions.Count > 0)
                {
                    IVXDailyQuizData quizData = IVXDailyQuizConverter.ToPremiumDailyQuizData(rawData, langCode, dateStr);

                    if (quizData != null)
                    {
                        string validationError;
                        if (quizData.IsValid(out validationError))
                        {
                            Debug.Log($"[IVXDailyQuizService] ✓ Parsed premium quiz with {quizData.QuestionCount} questions");
                            return new IVXDailyQuizResult
                            {
                                Success = true,
                                QuizType = IVXDailyQuizType.DailyPremium,
                                QuizData = quizData,
                                FetchedDate = dateStr,
                                LanguageCode = langCode,
                                SourceUrl = sourceUrl
                            };
                        }
                        Debug.LogWarning($"[IVXDailyQuizService] Validation failed: {validationError}");
                    }
                }

                return CreateErrorResult(IVXDailyQuizType.DailyPremium, "Failed to parse premium quiz JSON");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[IVXDailyQuizService] Premium parse error: {ex.Message}");
                return CreateErrorResult(IVXDailyQuizType.DailyPremium, $"Parse error: {ex.Message}");
            }
        }

        #endregion

        #region Raw JSON Parsing

        private IVXDailyQuizRaw ParseRawDailyJson(string jsonText)
        {
            try
            {
                var raw = new IVXDailyQuizRaw();

                raw.quizId = ExtractJsonString(jsonText, "quizId");
                raw.today_quiz = ExtractTodayQuiz(jsonText);
                raw.generated_at = ExtractJsonString(jsonText, "generated_at");

                return raw;
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[IVXDailyQuizService] Raw daily JSON parse failed: {ex.Message}");
                return null;
            }
        }

        private IVXDailyPremiumQuizRaw ParseRawPremiumJson(string jsonText)
        {
            try
            {
                var raw = new IVXDailyPremiumQuizRaw();

                raw.quizId = ExtractJsonString(jsonText, "quizId");
                raw.topic = ExtractJsonString(jsonText, "topic");
                raw.difficulty = ExtractJsonString(jsonText, "difficulty");
                raw.generated_at = ExtractJsonString(jsonText, "generated_at");
                raw.premium_questions = ExtractPremiumQuestions(jsonText);

                return raw;
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[IVXDailyQuizService] Raw premium JSON parse failed: {ex.Message}");
                return null;
            }
        }

        private IVXRawDailyTodayQuiz ExtractTodayQuiz(string json)
        {
            var result = new IVXRawDailyTodayQuiz { questions = new List<IVXRawDailyQuestion>() };

            string searchPattern = "\"today_quiz\":";
            int keyStart = json.IndexOf(searchPattern);
            if (keyStart < 0) return result;

            int objStart = json.IndexOf('{', keyStart);
            if (objStart < 0) return result;

            int objEnd = FindMatchingBrace(json, objStart);
            if (objEnd < 0) return result;

            string todayQuizJson = json.Substring(objStart, objEnd - objStart + 1);

            result.topic = ExtractJsonString(todayQuizJson, "topic");
            result.difficulty = ExtractJsonString(todayQuizJson, "difficulty");
            result.generated_at = ExtractJsonString(todayQuizJson, "generated_at");

            string questionsPattern = "\"questions\":";
            int questionsStart = todayQuizJson.IndexOf(questionsPattern);
            if (questionsStart < 0) return result;

            int arrStart = todayQuizJson.IndexOf('[', questionsStart);
            if (arrStart < 0) return result;

            int arrEnd = FindMatchingBracket(todayQuizJson, arrStart);
            if (arrEnd < 0) return result;

            string questionsJson = todayQuizJson.Substring(arrStart, arrEnd - arrStart + 1);
            result.questions = ExtractQuestions(questionsJson);

            return result;
        }

        private List<IVXRawDailyQuestion> ExtractPremiumQuestions(string json)
        {
            string searchPattern = "\"premium_questions\":";
            int keyStart = json.IndexOf(searchPattern);
            if (keyStart < 0) return new List<IVXRawDailyQuestion>();

            int arrStart = json.IndexOf('[', keyStart);
            if (arrStart < 0) return new List<IVXRawDailyQuestion>();

            int arrEnd = FindMatchingBracket(json, arrStart);
            if (arrEnd < 0) return new List<IVXRawDailyQuestion>();

            string questionsJson = json.Substring(arrStart, arrEnd - arrStart + 1);
            return ExtractQuestions(questionsJson);
        }

        private List<IVXRawDailyQuestion> ExtractQuestions(string questionsArrayJson)
        {
            var questions = new List<IVXRawDailyQuestion>();

            int pos = 1;
            while (pos < questionsArrayJson.Length)
            {
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

        private IVXRawDailyQuestion ExtractSingleQuestion(string questionJson)
        {
            var question = new IVXRawDailyQuestion
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

        #endregion

        #region JSON Utility Methods

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

            valueStart++;
            int valueEnd = FindClosingQuote(json, valueStart);
            if (valueEnd < 0) return null;

            return UnescapeJsonString(json.Substring(valueStart, valueEnd - valueStart));
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

        private IVXRawDailyHints ExtractHints(string json)
        {
            string searchPattern = "\"hints\":";
            int keyStart = json.IndexOf(searchPattern);
            if (keyStart < 0) return null;

            int objStart = json.IndexOf('{', keyStart);
            if (objStart < 0) return null;

            int objEnd = FindMatchingBrace(json, objStart);
            if (objEnd < 0) return null;

            string hintsJson = json.Substring(objStart, objEnd - objStart + 1);

            return new IVXRawDailyHints
            {
                text = ExtractJsonString(hintsJson, "text"),
                audio_url = ExtractJsonString(hintsJson, "audio_url")
            };
        }

        private Dictionary<string, IVXRawDailyQuestionLanguage> ExtractLanguages(string json)
        {
            var result = new Dictionary<string, IVXRawDailyQuestionLanguage>();

            string searchPattern = "\"languages\":";
            int keyStart = json.IndexOf(searchPattern);
            if (keyStart < 0) return result;

            int objStart = json.IndexOf('{', keyStart);
            if (objStart < 0) return result;

            int objEnd = FindMatchingBrace(json, objStart);
            if (objEnd < 0) return result;

            string languagesJson = json.Substring(objStart, objEnd - objStart + 1);

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

        private IVXRawDailyQuestionLanguage ExtractLanguageData(string json, string langCode)
        {
            string searchPattern = $"\"{langCode}\":";
            int keyStart = json.IndexOf(searchPattern);
            if (keyStart < 0) return null;

            int objStart = json.IndexOf('{', keyStart);
            if (objStart < 0) return null;

            int objEnd = FindMatchingBrace(json, objStart);
            if (objEnd < 0) return null;

            string langJson = json.Substring(objStart, objEnd - objStart + 1);

            return new IVXRawDailyQuestionLanguage
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

        private static IVXDailyQuizResult CreateErrorResult(IVXDailyQuizType quizType, string errorMessage)
        {
            return new IVXDailyQuizResult
            {
                Success = false,
                QuizType = quizType,
                ErrorMessage = errorMessage
            };
        }

        #endregion
    }
}
