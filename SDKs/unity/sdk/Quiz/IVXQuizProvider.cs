using System;
using System.Threading.Tasks;
using UnityEngine;
using Newtonsoft.Json;
using IntelliVerseX.Networking;
using IntelliVerseX.Core;

namespace IntelliVerseX.Quiz
{
    /// <summary>
    /// Interface for quiz data providers.
    /// Implement this to create custom quiz sources (S3, local files, API, etc.)
    /// </summary>
    public interface IIVXQuizProvider
    {
        Task<IVXQuizResult> FetchQuizAsync(string quizId);
        Task<IVXQuizResult> FetchQuizAsync(DateTime date);
    }

    /// <summary>
    /// S3-based quiz provider with automatic fallback.
    /// Fetches daily quizzes from AWS S3 with 7-day fallback window.
    /// Part of IntelliVerse.GameSDK.Quiz package.
    /// </summary>
    public class IVXS3QuizProvider : IIVXQuizProvider
    {
        private readonly string _s3BaseUrl;
        private readonly IVXRetryPolicy _retryPolicy;
        private const int MAX_FALLBACK_DAYS = 7;
        private const string QUIZ_FILE_PREFIX = "dailyquiz-";
        private const string QUIZ_FILE_EXTENSION = ".json";

        /// <summary>
        /// S3 base URL for quiz files
        /// </summary>
        public string S3BaseUrl
        {
            get => _s3BaseUrl;
            set { } // Read-only, set via constructor
        }

        /// <summary>
        /// Number of days to fallback when quiz not found (default 7)
        /// </summary>
        public int FallbackDays
        {
            get => MAX_FALLBACK_DAYS;
            set { } // Read-only constant
        }

        /// <summary>
        /// Enable local caching of downloaded quizzes (not yet implemented)
        /// </summary>
        public bool EnableCaching { get; set; } = false;

        public IVXS3QuizProvider(string s3BaseUrl, IVXRetryPolicy retryPolicy = null)
        {
            _s3BaseUrl = s3BaseUrl?.TrimEnd('/') ?? throw new ArgumentNullException(nameof(s3BaseUrl));
            _retryPolicy = retryPolicy ?? IVXRetryPolicy.Default;
        }

        /// <summary>
        /// Fetches quiz by ID (e.g., "dailyquiz-2025-11-17")
        /// </summary>
        public async Task<IVXQuizResult> FetchQuizAsync(string quizId)
        {
            string url = $"{_s3BaseUrl}/{quizId}{QUIZ_FILE_EXTENSION}";
            return await FetchFromUrlAsync(url, quizId);
        }

        /// <summary>
        /// Fetches quiz for a specific date with automatic fallback.
        /// If quiz not found, tries previous days up to MAX_FALLBACK_DAYS.
        /// </summary>
        public async Task<IVXQuizResult> FetchQuizAsync(DateTime date)
        {
            // Try current date first
            var result = await TryFetchQuizForDateAsync(date);
            
            if (result.Success)
            {
                IVXLogger.Log($"Successfully fetched quiz for {date:yyyy-MM-dd}");
                return result;
            }

            // Fallback: Try previous days
            IVXLogger.LogWarning($"Quiz not found for {date:yyyy-MM-dd}, trying fallback...");
            
            for (int daysBack = 1; daysBack <= MAX_FALLBACK_DAYS; daysBack++)
            {
                DateTime fallbackDate = date.AddDays(-daysBack);
                result = await TryFetchQuizForDateAsync(fallbackDate);
                
                if (result.Success)
                {
                    IVXLogger.Log($"Found quiz from {daysBack} day(s) ago ({fallbackDate:yyyy-MM-dd})");
                    return result;
                }
            }

            // No quiz found within fallback window
            string errorMsg = $"No quiz available for the past {MAX_FALLBACK_DAYS} days";
            IVXLogger.LogError(errorMsg);
            
            return IVXQuizResult.FailureResult(errorMsg);
        }

        private async Task<IVXQuizResult> TryFetchQuizForDateAsync(DateTime date)
        {
            string dateStr = date.ToString("yyyy-MM-dd");
            string quizId = $"{QUIZ_FILE_PREFIX}{dateStr}";
            string url = $"{_s3BaseUrl}/{quizId}{QUIZ_FILE_EXTENSION}";
            
            IVXLogger.Log($"Fetching quiz from: {url}");
            IVXLogger.Log($"Network status: {IVXNetworkRequest.GetNetworkType()}");

            return await FetchFromUrlAsync(url, quizId);
        }

        private async Task<IVXQuizResult> FetchFromUrlAsync(string url, string quizId)
        {
            try
            {
                var networkResult = await IVXNetworkRequest.GetAsync(url, _retryPolicy, 30);

                if (!networkResult.IsSuccess)
                {
                    if (networkResult.ResponseCode == 404)
                    {
                        return IVXQuizResult.FailureResult($"Quiz not found: {quizId}");
                    }

                    return IVXQuizResult.FailureResult(networkResult.ErrorMessage);
                }

                string jsonText = networkResult.Data;
                
                if (string.IsNullOrEmpty(jsonText))
                {
                    return IVXQuizResult.FailureResult("Empty response from server");
                }

                // Parse JSON
                IVXQuizData quizData = ParseQuizJson(jsonText);
                
                if (quizData == null)
                {
                    return IVXQuizResult.FailureResult("Failed to parse quiz JSON");
                }

                IVXLogger.Log($"Successfully parsed quiz with {quizData.questions.Count} questions");
                
                return IVXQuizResult.SuccessResult(quizData, url);
            }
            catch (Exception ex)
            {
                IVXLogger.LogError($"Exception during quiz fetch: {ex.Message}");
                return IVXQuizResult.FailureResult($"Exception: {ex.Message}");
            }
        }

        private IVXQuizData ParseQuizJson(string jsonText)
        {
            try
            {
                // Try Newtonsoft.Json first (QuizVerse format)
                var quizData = JsonConvert.DeserializeObject<IVXQuizData>(jsonText);
                return quizData;
            }
            catch (Exception ex)
            {
                IVXLogger.Log($"Failed to parse quiz JSON with Newtonsoft.Json: {ex.Message}");
                
                // Fallback to Unity JsonUtility
                try
                {
                    return JsonUtility.FromJson<IVXQuizData>(jsonText);
                }
                catch (Exception ex2)
                {
                    IVXLogger.LogError($"Failed to parse quiz JSON with JsonUtility: {ex2.Message}");
                    return null;
                }
            }
        }
    }

    /// <summary>
    /// Local quiz provider that loads from Resources or StreamingAssets.
    /// Used for offline mode or bundled quizzes.
    /// </summary>
    public class IVXLocalQuizProvider : IIVXQuizProvider
    {
        private readonly string _resourcePath;

        public IVXLocalQuizProvider(string resourcePath)
        {
            _resourcePath = resourcePath ?? throw new ArgumentNullException(nameof(resourcePath));
        }

        public async Task<IVXQuizResult> FetchQuizAsync(string quizId)
        {
            return await Task.Run(() =>
            {
                try
                {
                    TextAsset textAsset = Resources.Load<TextAsset>(_resourcePath);
                    
                    if (textAsset == null)
                    {
                        return IVXQuizResult.FailureResult($"Local quiz not found: {_resourcePath}");
                    }

                    var quizData = JsonUtility.FromJson<IVXQuizData>(textAsset.text);
                    
                    if (quizData == null)
                    {
                        return IVXQuizResult.FailureResult("Failed to parse local quiz JSON");
                    }

                    IVXLogger.Log($"Loaded local quiz with {quizData.questions.Count} questions");
                    
                    return IVXQuizResult.SuccessResult(quizData, _resourcePath);
                }
                catch (Exception ex)
                {
                    IVXLogger.LogError(ex.Message);
                    return IVXQuizResult.FailureResult(ex.Message);
                }
            });
        }

        public Task<IVXQuizResult> FetchQuizAsync(DateTime date)
        {
            // Local provider doesn't support date-based fetching
            return FetchQuizAsync(_resourcePath);
        }
    }

    /// <summary>
    /// Hybrid quiz provider with primary and fallback sources.
    /// Tries primary (usually S3), falls back to local on failure.
    /// Enables offline-first quiz experience.
    /// </summary>
    public class IVXHybridQuizProvider : IIVXQuizProvider
    {
        private readonly IIVXQuizProvider _primaryProvider;
        private readonly IIVXQuizProvider _fallbackProvider;

        public IVXHybridQuizProvider(IIVXQuizProvider primaryProvider, IIVXQuizProvider fallbackProvider)
        {
            _primaryProvider = primaryProvider ?? throw new ArgumentNullException(nameof(primaryProvider));
            _fallbackProvider = fallbackProvider ?? throw new ArgumentNullException(nameof(fallbackProvider));
        }

        public async Task<IVXQuizResult> FetchQuizAsync(string quizId)
        {
            // Try primary first
            var result = await _primaryProvider.FetchQuizAsync(quizId);
            
            if (result.Success)
            {
                IVXLogger.Log("Primary provider succeeded");
                return result;
            }

            // Fallback to secondary
            IVXLogger.LogWarning("Primary provider failed, trying fallback");
            result = await _fallbackProvider.FetchQuizAsync(quizId);
            
            if (result.Success)
            {
                IVXLogger.Log("Fallback provider succeeded");
            }
            
            return result;
        }

        public async Task<IVXQuizResult> FetchQuizAsync(DateTime date)
        {
            var result = await _primaryProvider.FetchQuizAsync(date);
            
            if (result.Success)
            {
                return result;
            }

            IVXLogger.LogWarning("Primary provider failed, trying fallback");
            return await _fallbackProvider.FetchQuizAsync(date);
        }
    }
}
