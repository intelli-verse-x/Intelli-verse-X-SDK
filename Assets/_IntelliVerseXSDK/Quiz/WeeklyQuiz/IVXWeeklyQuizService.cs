// IVXWeeklyQuizService.cs
// Service for fetching weekly quiz data from remote server

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
using IntelliVerseX.Core;

namespace IntelliVerseX.Quiz.WeeklyQuiz
{
    /// <summary>
    /// Service for fetching weekly quiz data from S3.
    /// Handles all four quiz types: Fortune, Emoji, Prediction, and Health.
    /// 
    /// URL Pattern:
    ///   https://intelli-verse-x-media.s3.us-east-1.amazonaws.com/quiz-verse/weekly/{YYYY}-{D}-{WW}-{type}_{lang}.json
    /// </summary>
    public class IVXWeeklyQuizService
    {
        #region Constants

        private const string S3_BASE_URL = "https://intelli-verse-x-media.s3.us-east-1.amazonaws.com/quiz-verse/weekly/";
        private const string QUIZ_FILE_EXTENSION = ".json";
        private const int MAX_FALLBACK_WEEKS = 2;
        private const int REQUEST_TIMEOUT_SECONDS = 10;
        private const int FALLBACK_TIMEOUT_SECONDS = 5;

        private static readonly string[] SUPPORTED_LOCALES = { "en", "es", "ar", "fr", "de", "pt", "hi", "zh", "ja", "ko", "id", "ru" };

        #endregion

        #region Fields

        private readonly Dictionary<string, IVXWeeklyQuizResult> _cache = new Dictionary<string, IVXWeeklyQuizResult>();
        private readonly bool[] _isFetchingByType = new bool[4];
        private readonly object _cacheLock = new object();

        #endregion

        #region Public API

        public Task<IVXWeeklyQuizResult> FetchFortuneQuizAsync() => FetchWeeklyQuizAsync(IVXWeeklyQuizType.Fortune);
        public Task<IVXWeeklyQuizResult> FetchEmojiQuizAsync() => FetchWeeklyQuizAsync(IVXWeeklyQuizType.Emoji);
        public Task<IVXWeeklyQuizResult> FetchPredictionQuizAsync() => FetchWeeklyQuizAsync(IVXWeeklyQuizType.Prediction);
        public Task<IVXWeeklyQuizResult> FetchHealthQuizAsync() => FetchWeeklyQuizAsync(IVXWeeklyQuizType.Health);

        public string GetCurrentQuizUrl(IVXWeeklyQuizType quizType)
        {
            DateTime utcNow = DateTime.UtcNow;
            string langCode = GetCurrentLanguageCode();
            string prefix = GetQuizPrefix(quizType);
            return BuildQuizUrl(prefix, langCode, utcNow);
        }

        public (int year, int dayOfWeek, int weekNumber) GetCurrentWeekInfo()
        {
            DateTime utcNow = DateTime.UtcNow;
            var (isoYear, isoWeek, isoDay) = GetISOWeekDate(utcNow);
            return (isoYear, isoDay, isoWeek);
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

                string cacheKey = BuildCacheKey(quizType, langCode);
                lock (_cacheLock)
                {
                    if (_cache.TryGetValue(cacheKey, out IVXWeeklyQuizResult cached) && cached.Success)
                    {
                        Debug.Log($"[IVXWeeklyQuizService] ✓ Returning cached {quizType}");
                        return cached;
                    }
                }

                Debug.Log($"[IVXWeeklyQuizService] Fetching {quizType} for {utcNow:yyyy-MM-dd}, lang={langCode}");
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

        #region Private Methods

        private static string BuildCacheKey(IVXWeeklyQuizType quizType, string langCode)
        {
            var (year, week, _) = GetISOWeekDate(DateTime.UtcNow);
            return $"{quizType}_{year}_{week}_{langCode}";
        }

        private async Task<IVXWeeklyQuizResult> FetchWithFallbackAsync(IVXWeeklyQuizType quizType, DateTime targetDate, string langCode)
        {
            langCode = NormalizeLanguageCode(langCode);
            string quizPrefix = GetQuizPrefix(quizType);

            var result = await TryFetchForWeekAsync(quizType, quizPrefix, targetDate, langCode);
            if (result.Success)
            {
                Debug.Log($"[IVXWeeklyQuizService] ✓ Fetched {quizType} for current week");
                return result;
            }

            Debug.LogWarning($"[IVXWeeklyQuizService] {quizType} not found for current week, trying previous weeks...");
            for (int weeksBack = 1; weeksBack <= MAX_FALLBACK_WEEKS; weeksBack++)
            {
                DateTime fallbackWeekDate = targetDate.AddDays(-7 * weeksBack);
                result = await TryFetchForWeekAsync(quizType, quizPrefix, fallbackWeekDate, langCode);
                if (result.Success)
                {
                    Debug.Log($"[IVXWeeklyQuizService] ✓ Found {quizType} from {weeksBack} week(s) ago");
                    return result;
                }
            }

            if (!langCode.Equals("en", StringComparison.OrdinalIgnoreCase))
            {
                Debug.LogWarning($"[IVXWeeklyQuizService] No {quizType} for '{langCode}', trying English fallback...");
                return await FetchWithFallbackAsync(quizType, targetDate, "en");
            }

            string errorMsg = $"No {quizType} quiz available for the past {MAX_FALLBACK_WEEKS} weeks";
            Debug.LogError($"[IVXWeeklyQuizService] {errorMsg}");
            return CreateErrorResult(quizType, errorMsg);
        }

        private async Task<IVXWeeklyQuizResult> TryFetchForWeekAsync(
            IVXWeeklyQuizType quizType,
            string prefix,
            DateTime targetDate,
            string langCode)
        {
            var (isoYear, isoWeek, currentDay) = GetISOWeekDate(targetDate);

            var result = await TryFetchForDateAsync(quizType, prefix, targetDate, langCode, false);
            if (result.Success)
                return result;

            DateTime weekMonday = targetDate.AddDays(-(currentDay - 1));

            var tasks = new List<Task<IVXWeeklyQuizResult>>(6);
            for (int day = 7; day >= 1; day--)
            {
                if (day == currentDay) continue;
                DateTime fallbackDate = weekMonday.AddDays(day - 1);
                tasks.Add(TryFetchForDateAsync(quizType, prefix, fallbackDate, langCode, true));
            }

            var results = await Task.WhenAll(tasks);

            foreach (var r in results)
            {
                if (r.Success)
                    return r;
            }

            return CreateErrorResult(quizType, $"No quiz found for week {isoWeek}");
        }

        private async Task<IVXWeeklyQuizResult> TryFetchForDateAsync(
            IVXWeeklyQuizType quizType,
            string prefix,
            DateTime date,
            string langCode,
            bool isFallback)
        {
            string url = BuildQuizUrl(prefix, langCode, date);

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
                        if (code != 404 && code != 403)
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

                    var (isoYear, isoWeek, isoDay) = GetISOWeekDate(date);
                    string dateKey = $"{isoYear}-{isoWeek}-{isoDay}";
                    return ParseAndValidate(quizType, jsonText, dateKey, langCode);
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"[IVXWeeklyQuizService] Exception fetching {url}: {ex.Message}");
                return CreateErrorResult(quizType, $"Exception: {ex.Message}");
            }
        }

        private string BuildQuizUrl(string prefix, string langCode, DateTime date)
        {
            var (isoYear, isoWeek, isoDay) = GetISOWeekDate(date);
            return $"{S3_BASE_URL}{isoYear}-{isoDay}-{isoWeek}-{prefix}_{langCode}{QUIZ_FILE_EXTENSION}";
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

        #region ISO Week Date

        private static (int year, int week, int day) GetISOWeekDate(DateTime date)
        {
            int isoDayOfWeek = date.DayOfWeek == DayOfWeek.Sunday ? 7 : (int)date.DayOfWeek;
            DateTime thursday = date.AddDays(4 - isoDayOfWeek);
            int isoYear = thursday.Year;

            DateTime jan4 = new DateTime(isoYear, 1, 4);
            int jan4Dow = jan4.DayOfWeek == DayOfWeek.Sunday ? 7 : (int)jan4.DayOfWeek;
            DateTime startOfWeek1 = jan4.AddDays(1 - jan4Dow);

            int weekNumber = (thursday - startOfWeek1).Days / 7 + 1;

            return (isoYear, weekNumber, isoDayOfWeek);
        }

        #endregion

        #region JSON Parsing

        private IVXWeeklyQuizResult ParseAndValidate(IVXWeeklyQuizType quizType, string jsonText, string dateStr, string langCode)
        {
            object quizData = ParseQuizJson(quizType, jsonText);
            if (quizData == null)
            {
                return CreateErrorResult(quizType, $"Failed to parse {quizType} quiz JSON");
            }

            string validationError = ValidateQuizData(quizType, quizData);
            if (!string.IsNullOrEmpty(validationError))
            {
                Debug.LogWarning($"[IVXWeeklyQuizService] Validation failed: {validationError}");
                return CreateErrorResult(quizType, validationError);
            }

            return new IVXWeeklyQuizResult
            {
                Success = true,
                QuizType = quizType,
                QuizData = quizData,
                FetchedDate = dateStr,
                LanguageCode = langCode
            };
        }

        private static object ParseQuizJson(IVXWeeklyQuizType quizType, string jsonText)
        {
            try
            {
                return quizType switch
                {
                    IVXWeeklyQuizType.Fortune => JsonUtility.FromJson<IVXFortuneQuizData>(jsonText),
                    IVXWeeklyQuizType.Emoji => JsonUtility.FromJson<IVXEmojiQuizData>(jsonText),
                    IVXWeeklyQuizType.Prediction => JsonUtility.FromJson<IVXPredictionQuizData>(jsonText),
                    IVXWeeklyQuizType.Health => JsonUtility.FromJson<IVXHealthQuizData>(jsonText),
                    _ => null
                };
            }
            catch (Exception ex)
            {
                Debug.LogError($"[IVXWeeklyQuizService] JSON parse error for {quizType}: {ex.Message}");
                return null;
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
