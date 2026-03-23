using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace IntelliVerseX.Networking
{
    /// <summary>
    /// Retry policy configuration for network requests.
    /// Implements exponential backoff with configurable parameters.
    /// Part of IntelliVerse.GameSDK.Networking package.
    /// </summary>
    [Serializable]
    public class IVXRetryPolicy
    {
        [Tooltip("Maximum number of retry attempts (0 = no retries)")]
        public int MaxRetries = 3;
        
        [Tooltip("Initial delay before first retry (seconds)")]
        public float InitialDelaySeconds = 1f;
        
        [Tooltip("Maximum delay between retries (seconds)")]
        public float MaxDelaySeconds = 30f;
        
        [Tooltip("Multiplier for exponential backoff")]
        public float BackoffMultiplier = 2f;

        /// <summary>
        /// Default retry policy with sensible defaults (3 retries, 1-30s delay)
        /// </summary>
        public static IVXRetryPolicy Default => new IVXRetryPolicy
        {
            MaxRetries = 3,
            InitialDelaySeconds = 1f,
            MaxDelaySeconds = 30f,
            BackoffMultiplier = 2f
        };

        /// <summary>
        /// Aggressive retry policy for critical operations (5 retries, 0.5-60s delay)
        /// </summary>
        public static IVXRetryPolicy Aggressive => new IVXRetryPolicy
        {
            MaxRetries = 5,
            InitialDelaySeconds = 0.5f,
            MaxDelaySeconds = 60f,
            BackoffMultiplier = 2.5f
        };

        /// <summary>
        /// No retry policy - fail fast
        /// </summary>
        public static IVXRetryPolicy NoRetry => new IVXRetryPolicy
        {
            MaxRetries = 0,
            InitialDelaySeconds = 0f,
            MaxDelaySeconds = 0f,
            BackoffMultiplier = 1f
        };
    }

    /// <summary>
    /// Result wrapper for network operations with comprehensive error handling.
    /// Provides detailed information about success/failure and retry recommendations.
    /// </summary>
    /// <typeparam name="T">The type of data returned on success</typeparam>
    public class IVXNetworkResult<T>
    {
        public bool IsSuccess { get; private set; }
        public T Data { get; private set; }
        public string ErrorMessage { get; private set; }
        public long ResponseCode { get; private set; }
        public bool ShouldRetry { get; private set; }
        public int AttemptCount { get; private set; }

        public static IVXNetworkResult<T> Success(T data, int attemptCount = 1)
        {
            return new IVXNetworkResult<T>
            {
                IsSuccess = true,
                Data = data,
                ResponseCode = 200,
                AttemptCount = attemptCount
            };
        }

        public static IVXNetworkResult<T> Failure(string error, long responseCode = 0, bool shouldRetry = true, int attemptCount = 1)
        {
            return new IVXNetworkResult<T>
            {
                IsSuccess = false,
                ErrorMessage = error,
                ResponseCode = responseCode,
                ShouldRetry = shouldRetry,
                AttemptCount = attemptCount
            };
        }
    }

    /// <summary>
    /// Enhanced network request handler with automatic retry logic, exponential backoff, and offline detection.
    /// Replaces direct UnityWebRequest usage throughout the SDK.
    /// Part of IntelliVerse.GameSDK.Networking package.
    /// </summary>
    public static class IVXNetworkRequest
    {
        /// <summary>
        /// Performs a GET request with retry logic and exponential backoff.
        /// Automatically handles offline detection, rate limiting, and server errors.
        /// </summary>
        /// <param name="url">The URL to fetch</param>
        /// <param name="policy">Retry policy (uses Default if null)</param>
        /// <param name="timeoutSeconds">Request timeout in seconds (default: 30)</param>
        /// <param name="cancellationToken">Cancellation token for async operations</param>
        /// <returns>NetworkResult with data or error information</returns>
        public static async Task<IVXNetworkResult<string>> GetAsync(
            string url,
            IVXRetryPolicy policy = null,
            int timeoutSeconds = 30,
            CancellationToken cancellationToken = default)
        {
            policy = policy ?? IVXRetryPolicy.Default;
            int attempt = 0;
            float delay = policy.InitialDelaySeconds;

            while (attempt < policy.MaxRetries + 1)
            {
                attempt++;

                // Check for cancellation
                if (cancellationToken.IsCancellationRequested)
                {
                    return IVXNetworkResult<string>.Failure(
                        "Request cancelled",
                        responseCode: 0,
                        shouldRetry: false,
                        attemptCount: attempt
                    );
                }

                // Check network availability FIRST
                if (Application.internetReachability == NetworkReachability.NotReachable)
                {
                    Debug.LogWarning($"[IVXNetworkRequest] No internet connection (attempt {attempt}/{policy.MaxRetries + 1})");
                    
                    if (attempt >= policy.MaxRetries + 1)
                    {
                        return IVXNetworkResult<string>.Failure(
                            "No internet connection. Please check your network settings.",
                            responseCode: 0,
                            shouldRetry: false,
                            attemptCount: attempt
                        );
                    }

                    // Wait and retry
                    await Task.Delay((int)(delay * 1000), cancellationToken);
                    delay = Mathf.Min(delay * policy.BackoffMultiplier, policy.MaxDelaySeconds);
                    continue;
                }

                try
                {
                    using (UnityWebRequest request = UnityWebRequest.Get(url))
                    {
                        request.timeout = timeoutSeconds;

                        // Send request asynchronously
                        var operation = request.SendWebRequest();

                        // Wait for completion with cancellation support
                        while (!operation.isDone && !cancellationToken.IsCancellationRequested)
                        {
                            await Task.Yield();
                        }

                        if (cancellationToken.IsCancellationRequested)
                        {
                            request.Abort();
                            return IVXNetworkResult<string>.Failure(
                                "Request cancelled",
                                responseCode: 0,
                                shouldRetry: false,
                                attemptCount: attempt
                            );
                        }

                        // SUCCESS
                        if (request.result == UnityWebRequest.Result.Success)
                        {
                            Debug.Log($"[IVXNetworkRequest] ✅ Success on attempt {attempt}/{policy.MaxRetries + 1}");
                            return IVXNetworkResult<string>.Success(
                                request.downloadHandler.text,
                                attemptCount: attempt
                            );
                        }

                        // Handle specific HTTP status codes
                        long responseCode = request.responseCode;

                        // 404 Not Found - Don't retry
                        if (responseCode == 404)
                        {
                            Debug.LogWarning($"[IVXNetworkRequest] Resource not found (404): {url}");
                            return IVXNetworkResult<string>.Failure(
                                $"Resource not found: {url}",
                                responseCode: 404,
                                shouldRetry: false,
                                attemptCount: attempt
                            );
                        }

                        // 429 Rate Limited - Use Retry-After header if available
                        if (responseCode == 429)
                        {
                            string retryAfter = request.GetResponseHeader("Retry-After");
                            if (int.TryParse(retryAfter, out int retrySeconds))
                            {
                                delay = Mathf.Min(retrySeconds, policy.MaxDelaySeconds);
                                Debug.LogWarning($"[IVXNetworkRequest] Rate limited (429), retrying after {delay}s");
                            }
                            else
                            {
                                Debug.LogWarning($"[IVXNetworkRequest] Rate limited (429), using exponential backoff");
                            }
                        }

                        // 5xx Server Errors - Retry
                        if (responseCode >= 500)
                        {
                            Debug.LogWarning($"[IVXNetworkRequest] Server error {responseCode}, retrying in {delay}s (attempt {attempt}/{policy.MaxRetries + 1})");
                        }

                        // Connection errors - Retry
                        if (request.result == UnityWebRequest.Result.ConnectionError)
                        {
                            Debug.LogWarning($"[IVXNetworkRequest] Connection error: {request.error}, retrying in {delay}s (attempt {attempt}/{policy.MaxRetries + 1})");
                        }

                        // Timeout - Retry
                        if (request.result == UnityWebRequest.Result.ProtocolError && responseCode == 0)
                        {
                            Debug.LogWarning($"[IVXNetworkRequest] Request timeout, retrying in {delay}s (attempt {attempt}/{policy.MaxRetries + 1})");
                        }

                        // Last attempt?
                        if (attempt >= policy.MaxRetries + 1)
                        {
                            string errorMsg = $"Failed after {attempt} attempts: {request.error}";
                            Debug.LogError($"[IVXNetworkRequest] ❌ {errorMsg}");
                            
                            return IVXNetworkResult<string>.Failure(
                                errorMsg,
                                responseCode: responseCode,
                                shouldRetry: false,
                                attemptCount: attempt
                            );
                        }

                        // Wait before retry (exponential backoff)
                        Debug.Log($"[IVXNetworkRequest] Waiting {delay}s before retry {attempt + 1}...");
                        await Task.Delay((int)(delay * 1000), cancellationToken);
                        delay = Mathf.Min(delay * policy.BackoffMultiplier, policy.MaxDelaySeconds);
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogError($"[IVXNetworkRequest] Exception on attempt {attempt}/{policy.MaxRetries + 1}: {ex.GetType().Name}");
                    Debug.LogError($"[IVXNetworkRequest] Message: {ex.Message}");
                    Debug.LogError($"[IVXNetworkRequest] Stack trace: {ex.StackTrace}");

                    // Last attempt?
                    if (attempt >= policy.MaxRetries + 1)
                    {
                        return IVXNetworkResult<string>.Failure(
                            $"Exception after {attempt} attempts: {ex.Message}",
                            responseCode: 0,
                            shouldRetry: false,
                            attemptCount: attempt
                        );
                    }

                    // Wait before retry
                    await Task.Delay((int)(delay * 1000), cancellationToken);
                    delay = Mathf.Min(delay * policy.BackoffMultiplier, policy.MaxDelaySeconds);
                }
            }

            // Should never reach here
            return IVXNetworkResult<string>.Failure(
                "Unexpected error in retry loop",
                responseCode: 0,
                shouldRetry: false,
                attemptCount: attempt
            );
        }

        /// <summary>
        /// Checks if network is currently available (any connection type)
        /// </summary>
        public static bool IsNetworkAvailable()
        {
            return Application.internetReachability != NetworkReachability.NotReachable;
        }

        /// <summary>
        /// Gets the current network type (No Connection / Mobile Data / WiFi)
        /// </summary>
        public static string GetNetworkType()
        {
            switch (Application.internetReachability)
            {
                case NetworkReachability.NotReachable:
                    return "No Connection";
                case NetworkReachability.ReachableViaCarrierDataNetwork:
                    return "Mobile Data";
                case NetworkReachability.ReachableViaLocalAreaNetwork:
                    return "WiFi/LAN";
                default:
                    return "Unknown";
            }
        }

        /// <summary>
        /// Waits for network connection to become available (with timeout)
        /// </summary>
        /// <param name="timeoutSeconds">Maximum wait time in seconds</param>
        /// <param name="checkIntervalMs">Interval between checks in milliseconds</param>
        /// <returns>True if connection available, false if timeout</returns>
        public static async Task<bool> WaitForNetworkAsync(int timeoutSeconds = 30, int checkIntervalMs = 500)
        {
            int elapsed = 0;
            while (elapsed < timeoutSeconds * 1000)
            {
                if (IsNetworkAvailable())
                {
                    Debug.Log($"[IVXNetworkRequest] Network connection restored after {elapsed}ms");
                    return true;
                }

                await Task.Delay(checkIntervalMs);
                elapsed += checkIntervalMs;
            }

            Debug.LogWarning($"[IVXNetworkRequest] Network wait timeout after {timeoutSeconds}s");
            return false;
        }
    }
}
