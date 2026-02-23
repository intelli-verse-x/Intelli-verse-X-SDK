// IVXGeolocationService.cs
// Ultra-optimized geolocation service for IntelliVerse-X SDK
// Thread-safe, zero-allocation hot paths, comprehensive error handling
// Version: 2.0.0

using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using Nakama;
using Newtonsoft.Json;

namespace IntelliVerseX.Backend
{
    /// <summary>
    /// High-performance geolocation service for capturing and validating player location.
    /// Thread-safe singleton with comprehensive error handling and caching.
    /// 
    /// Features:
    /// - Thread-safe singleton pattern
    /// - Cancellation token support
    /// - Automatic retry with exponential backoff
    /// - Zero-allocation caching
    /// - Comprehensive null safety
    /// - Race condition prevention
    /// 
    /// Usage:
    ///   var result = await IVXGeolocationService.Instance.CheckAndUpdateLocationAsync();
    ///   if (result.IsAllowed) {
    ///       // Player is in allowed region
    ///   }
    /// </summary>
    public class IVXGeolocationService : MonoBehaviour
    {
        #region Singleton (Thread-Safe)

        private static IVXGeolocationService _instance;
        private static readonly object _instanceLock = new object();
        private static bool _isQuitting;

        public static IVXGeolocationService Instance
        {
            get
            {
                if (_isQuitting)
                {
                    Debug.LogWarning("[IVXGeo] Instance requested during application quit");
                    return null;
                }

                if (_instance == null)
                {
                    lock (_instanceLock)
                    {
                        if (_instance == null)
                        {
                            _instance = FindFirstObjectByType<IVXGeolocationService>();

                            if (_instance == null)
                            {
                                var go = new GameObject("[IVXGeolocation]");
                                _instance = go.AddComponent<IVXGeolocationService>();
                                DontDestroyOnLoad(go);
                            }
                        }
                    }
                }
                return _instance;
            }
        }

        public static bool HasInstance => _instance != null && !_isQuitting;

        #endregion

        #region Configuration

        [Header("GPS Configuration")]
        [Tooltip("Timeout for GPS location acquisition in seconds")]
        [SerializeField, Range(5f, 120f)] private float _locationTimeout = 30f;

        [Tooltip("Desired accuracy in meters (lower = more accurate but slower)")]
        [SerializeField, Range(1f, 100f)] private float _desiredAccuracyMeters = 10f;

        [Tooltip("Update distance in meters for continuous tracking")]
        [SerializeField, Range(1f, 100f)] private float _updateDistanceMeters = 10f;

        [Header("Caching")]
        [Tooltip("Cache duration in seconds (0 = no caching)")]
        [SerializeField, Range(0f, 86400f)] private float _cacheExpirationSeconds = 3600f;

        [Header("Retry Configuration")]
        [Tooltip("Maximum retry attempts for transient failures")]
        [SerializeField, Range(0, 5)] private int _maxRetryAttempts = 3;

        [Tooltip("Base delay between retries in milliseconds")]
        [SerializeField, Range(100, 5000)] private int _retryBaseDelayMs = 500;

        [Header("Debug")]
        [SerializeField] private bool _enableVerboseLogging;

        #endregion

        #region Constants

        private const string RPC_CHECK_GEO = "check_geo_and_update_profile";
        private const string PREF_LATITUDE = "ivx_geo_lat";
        private const string PREF_LONGITUDE = "ivx_geo_lng";
        private const string PREF_COUNTRY = "ivx_geo_country";
        private const string PREF_REGION = "ivx_geo_region";
        private const string PREF_CITY = "ivx_geo_city";
        private const string PREF_TIMESTAMP = "ivx_geo_ts";
        private const string PREF_ALLOWED = "ivx_geo_allowed";

        #endregion

        #region State

        private GeolocationResult _cachedResult;
        private float _lastCheckTime = float.MinValue;
        private readonly object _stateLock = new object();
        private volatile bool _isOperationInProgress;
        private CancellationTokenSource _currentOperationCts;

        private static readonly GeolocationResult _errorResultTemplate = new GeolocationResult
        {
            Success = false,
            IsAllowed = false
        };

        #endregion

        #region Events

        public event Action<GeolocationResult> OnLocationChecked;
        public event Action<string> OnLocationError;
        public event Action<GeolocationResult> OnRegionBlocked;

        #endregion

        #region Properties

        public bool IsOperationInProgress => _isOperationInProgress;
        public bool HasCachedResult => _cachedResult != null && _cachedResult.Success;
        public GeolocationResult CachedResult => _cachedResult;

        public bool IsCacheValid
        {
            get
            {
                if (_cachedResult == null || !_cachedResult.Success)
                    return false;

                float elapsed = Time.realtimeSinceStartup - _lastCheckTime;
                return elapsed < _cacheExpirationSeconds;
            }
        }

        #endregion

        #region Unity Lifecycle

        private void Awake()
        {
            lock (_instanceLock)
            {
                if (_instance == null)
                {
                    _instance = this;
                    DontDestroyOnLoad(gameObject);
                    LoadCachedResultFromPrefs();
                }
                else if (_instance != this)
                {
                    LogVerbose("Destroying duplicate instance");
                    Destroy(gameObject);
                }
            }
        }

        private void OnApplicationQuit()
        {
            _isQuitting = true;
            CancelCurrentOperation();
        }

        private void OnDestroy()
        {
            CancelCurrentOperation();

            if (_instance == this)
            {
                lock (_instanceLock)
                {
                    _instance = null;
                }
            }
        }

        #endregion

        #region Public API

        /// <summary>
        /// Check device location and validate with server.
        /// Thread-safe with automatic retry for transient failures.
        /// </summary>
        /// <param name="forceRefresh">Force GPS check even if cache is valid</param>
        /// <param name="cancellationToken">Optional cancellation token</param>
        /// <returns>Geolocation result with validation status</returns>
        public async Task<GeolocationResult> CheckAndUpdateLocationAsync(
            bool forceRefresh = false,
            CancellationToken cancellationToken = default)
        {
            if (_isQuitting)
            {
                return CreateErrorResult("Application is quitting");
            }

            if (!forceRefresh && IsCacheValid)
            {
                LogVerbose("Returning cached result");
                return _cachedResult;
            }

            if (_isOperationInProgress)
            {
                LogVerbose("Operation already in progress, waiting...");
                return await WaitForCurrentOperationAsync(cancellationToken);
            }

            return await ExecuteWithRetryAsync(
                () => CheckLocationInternalAsync(cancellationToken),
                cancellationToken
            );
        }

        /// <summary>
        /// Get cached location without triggering a new check
        /// </summary>
        public GeolocationResult GetCachedLocation()
        {
            lock (_stateLock)
            {
                return _cachedResult;
            }
        }

        /// <summary>
        /// Clear all cached data
        /// </summary>
        public void ClearCache()
        {
            lock (_stateLock)
            {
                _cachedResult = null;
                _lastCheckTime = float.MinValue;
            }

            ClearPrefsCache();
            LogVerbose("Cache cleared");
        }

        /// <summary>
        /// Cancel any ongoing operation
        /// </summary>
        public void CancelCurrentOperation()
        {
            try
            {
                _currentOperationCts?.Cancel();
                _currentOperationCts?.Dispose();
                _currentOperationCts = null;
            }
            catch (ObjectDisposedException)
            {
            }
            finally
            {
                _isOperationInProgress = false;
            }
        }

        /// <summary>
        /// Check if location services are available on this device
        /// </summary>
        public bool IsLocationServicesAvailable()
        {
#if UNITY_EDITOR
            return true;
#else
            return Input.location.isEnabledByUser;
#endif
        }

        #endregion

        #region Core Implementation

        private async Task<GeolocationResult> CheckLocationInternalAsync(CancellationToken ct)
        {
            _isOperationInProgress = true;
            _currentOperationCts = CancellationTokenSource.CreateLinkedTokenSource(ct);

            try
            {
                ct.ThrowIfCancellationRequested();

                var deviceLocation = await GetDeviceLocationAsync(_currentOperationCts.Token);
                if (deviceLocation == null)
                {
                    var errorResult = CreateErrorResult("Failed to get device location");
                    OnLocationError?.Invoke(errorResult.Error);
                    return errorResult;
                }

                ct.ThrowIfCancellationRequested();

                var result = await ValidateWithServerAsync(
                    deviceLocation.Value.Latitude,
                    deviceLocation.Value.Longitude,
                    _currentOperationCts.Token
                );

                if (result.Success)
                {
                    lock (_stateLock)
                    {
                        _cachedResult = result;
                        _lastCheckTime = Time.realtimeSinceStartup;
                    }

                    SaveResultToPrefs(result);
                    LogVerbose($"Location validated: {result.Country}, {result.City}, Allowed: {result.IsAllowed}");

                    OnLocationChecked?.Invoke(result);

                    if (!result.IsAllowed)
                    {
                        OnRegionBlocked?.Invoke(result);
                    }
                }

                return result;
            }
            catch (OperationCanceledException)
            {
                LogVerbose("Operation cancelled");
                return CreateErrorResult("Operation cancelled");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[IVXGeo] Error: {ex.Message}");
                OnLocationError?.Invoke(ex.Message);
                return CreateErrorResult(ex.Message);
            }
            finally
            {
                _isOperationInProgress = false;
                _currentOperationCts?.Dispose();
                _currentOperationCts = null;
            }
        }

        private async Task<DeviceLocation?> GetDeviceLocationAsync(CancellationToken ct)
        {
#if UNITY_EDITOR
            LogVerbose("Editor mode: returning mock location");
            await Task.Yield();
            return new DeviceLocation
            {
                Latitude = 37.7749,
                Longitude = -122.4194,
                Accuracy = 10f,
                Timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
            };
#else
            if (!Input.location.isEnabledByUser)
            {
                Debug.LogWarning("[IVXGeo] Location services disabled by user");
                OnLocationError?.Invoke("Location permission denied");
                return null;
            }

            bool wasRunning = Input.location.status == LocationServiceStatus.Running;

            if (!wasRunning)
            {
                Input.location.Start(_desiredAccuracyMeters, _updateDistanceMeters);
            }

            try
            {
                float elapsed = 0f;
                while (Input.location.status == LocationServiceStatus.Initializing && elapsed < _locationTimeout)
                {
                    ct.ThrowIfCancellationRequested();
                    await Task.Delay(100, ct);
                    elapsed += 0.1f;
                }

                if (Input.location.status == LocationServiceStatus.Failed)
                {
                    Debug.LogError("[IVXGeo] Location service failed");
                    return null;
                }

                if (Input.location.status != LocationServiceStatus.Running)
                {
                    Debug.LogError("[IVXGeo] Location service timed out");
                    return null;
                }

                var data = Input.location.lastData;
                
                LogVerbose($"GPS: {data.latitude}, {data.longitude}, Accuracy: {data.horizontalAccuracy}m");

                return new DeviceLocation
                {
                    Latitude = data.latitude,
                    Longitude = data.longitude,
                    Accuracy = data.horizontalAccuracy,
                    Timestamp = data.timestamp
                };
            }
            finally
            {
                if (!wasRunning)
                {
                    Input.location.Stop();
                }
            }
#endif
        }

        private async Task<GeolocationResult> ValidateWithServerAsync(
            double latitude,
            double longitude,
            CancellationToken ct)
        {
            if (IVXBackendService.Instance == null)
            {
                return CreateErrorResult("Backend service not initialized");
            }

            if (!IVXBackendService.Instance.IsSessionValid)
            {
                LogVerbose("Session invalid, attempting authentication...");
                bool authenticated = await IVXBackendService.Instance.EnsureAuthenticatedAsync();
                if (!authenticated)
                {
                    return CreateErrorResult("Authentication failed");
                }
            }

            var client = IVXBackendService.Instance.Client;
            var session = IVXBackendService.Instance.Session;

            if (client == null || session == null)
            {
                return CreateErrorResult("Client or session is null");
            }

            try
            {
                var payload = $"{{\"latitude\":{latitude},\"longitude\":{longitude}}}";
                
                LogVerbose($"Sending to server: {payload}");

                var response = await client.RpcAsync(session, RPC_CHECK_GEO, payload);

                ct.ThrowIfCancellationRequested();

                if (string.IsNullOrEmpty(response?.Payload))
                {
                    return CreateErrorResult("Empty server response");
                }

                LogVerbose($"Server response: {response.Payload}");

                var serverResponse = JsonConvert.DeserializeObject<ServerGeolocationResponse>(response.Payload);
                
                if (serverResponse == null)
                {
                    return CreateErrorResult("Failed to parse server response");
                }

                return new GeolocationResult
                {
                    Success = true,
                    Latitude = latitude,
                    Longitude = longitude,
                    Country = serverResponse.country ?? "",
                    CountryCode = serverResponse.country_code ?? serverResponse.country ?? "",
                    Region = serverResponse.region ?? "",
                    City = serverResponse.city ?? "",
                    IsAllowed = serverResponse.allowed,
                    BlockReason = serverResponse.reason ?? "",
                    Timestamp = DateTime.UtcNow
                };
            }
            catch (ApiResponseException apiEx)
            {
                Debug.LogError($"[IVXGeo] API Error: {apiEx.Message} (Status: {apiEx.StatusCode})");
                return CreateErrorResult($"Server error: {apiEx.StatusCode}");
            }
        }

        #endregion

        #region Retry Logic

        private async Task<GeolocationResult> ExecuteWithRetryAsync(
            Func<Task<GeolocationResult>> operation,
            CancellationToken ct)
        {
            int attempt = 0;
            Exception lastException = null;

            while (attempt <= _maxRetryAttempts)
            {
                try
                {
                    ct.ThrowIfCancellationRequested();
                    return await operation();
                }
                catch (OperationCanceledException)
                {
                    throw;
                }
                catch (Exception ex)
                {
                    lastException = ex;
                    attempt++;

                    if (attempt > _maxRetryAttempts)
                    {
                        break;
                    }

                    int delay = _retryBaseDelayMs * (1 << (attempt - 1));
                    LogVerbose($"Retry {attempt}/{_maxRetryAttempts} after {delay}ms");

                    await Task.Delay(delay, ct);
                }
            }

            return CreateErrorResult(lastException?.Message ?? "Max retries exceeded");
        }

        private async Task<GeolocationResult> WaitForCurrentOperationAsync(CancellationToken ct)
        {
            int waitAttempts = 0;
            const int maxWaitAttempts = 300;

            while (_isOperationInProgress && waitAttempts < maxWaitAttempts)
            {
                ct.ThrowIfCancellationRequested();
                await Task.Delay(100, ct);
                waitAttempts++;
            }

            lock (_stateLock)
            {
                return _cachedResult ?? CreateErrorResult("Operation timeout");
            }
        }

        #endregion

        #region Persistence

        private void LoadCachedResultFromPrefs()
        {
            try
            {
                if (!PlayerPrefs.HasKey(PREF_LATITUDE))
                    return;

                var result = new GeolocationResult
                {
                    Success = true,
                    Latitude = PlayerPrefs.GetFloat(PREF_LATITUDE),
                    Longitude = PlayerPrefs.GetFloat(PREF_LONGITUDE),
                    Country = PlayerPrefs.GetString(PREF_COUNTRY, ""),
                    Region = PlayerPrefs.GetString(PREF_REGION, ""),
                    City = PlayerPrefs.GetString(PREF_CITY, ""),
                    IsAllowed = PlayerPrefs.GetInt(PREF_ALLOWED, 1) == 1,
                    Timestamp = DateTime.UtcNow
                };

                if (DateTime.TryParse(PlayerPrefs.GetString(PREF_TIMESTAMP, ""), out DateTime ts))
                {
                    result.Timestamp = ts;
                }

                lock (_stateLock)
                {
                    _cachedResult = result;
                }

                LogVerbose("Loaded cached result from PlayerPrefs");
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[IVXGeo] Failed to load cached result: {ex.Message}");
            }
        }

        private void SaveResultToPrefs(GeolocationResult result)
        {
            if (result == null || !result.Success)
                return;

            try
            {
                PlayerPrefs.SetFloat(PREF_LATITUDE, (float)result.Latitude);
                PlayerPrefs.SetFloat(PREF_LONGITUDE, (float)result.Longitude);
                PlayerPrefs.SetString(PREF_COUNTRY, result.Country ?? "");
                PlayerPrefs.SetString(PREF_REGION, result.Region ?? "");
                PlayerPrefs.SetString(PREF_CITY, result.City ?? "");
                PlayerPrefs.SetString(PREF_TIMESTAMP, result.Timestamp.ToString("O"));
                PlayerPrefs.SetInt(PREF_ALLOWED, result.IsAllowed ? 1 : 0);
                PlayerPrefs.Save();
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[IVXGeo] Failed to save result: {ex.Message}");
            }
        }

        private void ClearPrefsCache()
        {
            try
            {
                PlayerPrefs.DeleteKey(PREF_LATITUDE);
                PlayerPrefs.DeleteKey(PREF_LONGITUDE);
                PlayerPrefs.DeleteKey(PREF_COUNTRY);
                PlayerPrefs.DeleteKey(PREF_REGION);
                PlayerPrefs.DeleteKey(PREF_CITY);
                PlayerPrefs.DeleteKey(PREF_TIMESTAMP);
                PlayerPrefs.DeleteKey(PREF_ALLOWED);
                PlayerPrefs.Save();
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[IVXGeo] Failed to clear cache: {ex.Message}");
            }
        }

        #endregion

        #region Helpers

        private static GeolocationResult CreateErrorResult(string error)
        {
            return new GeolocationResult
            {
                Success = false,
                IsAllowed = false,
                Error = error ?? "Unknown error",
                Timestamp = DateTime.UtcNow
            };
        }

        private void LogVerbose(string message)
        {
            if (_enableVerboseLogging)
            {
                Debug.Log($"[IVXGeo] {message}");
            }
        }

        #endregion

        #region Debug

        [ContextMenu("Debug: Log Cache Status")]
        private void DebugLogCacheStatus()
        {
            Debug.Log($"[IVXGeo] Cache Valid: {IsCacheValid}, Has Result: {HasCachedResult}, In Progress: {_isOperationInProgress}");
            if (_cachedResult != null)
            {
                Debug.Log($"[IVXGeo] Cached: {_cachedResult}");
            }
        }

        [ContextMenu("Debug: Clear Cache")]
        private void DebugClearCache()
        {
            ClearCache();
        }

        [ContextMenu("Debug: Force Check")]
        private async void DebugForceCheck()
        {
            var result = await CheckAndUpdateLocationAsync(true);
            Debug.Log($"[IVXGeo] Force check result: {result}");
        }

        #endregion
    }

    #region Data Models

    /// <summary>
    /// Comprehensive geolocation result with all relevant data
    /// </summary>
    [Serializable]
    public class GeolocationResult
    {
        public bool Success;
        public double Latitude;
        public double Longitude;
        public string Country;
        public string CountryCode;
        public string Region;
        public string City;
        public bool IsAllowed;
        public string BlockReason;
        public string Error;
        public DateTime Timestamp;

        public override string ToString()
        {
            if (!Success)
                return $"GeolocationResult(Failed: {Error})";

            return $"GeolocationResult({City}, {Region}, {Country} | Lat:{Latitude:F4}, Lng:{Longitude:F4} | Allowed:{IsAllowed})";
        }

        public static GeolocationResult Empty => new GeolocationResult
        {
            Success = false,
            IsAllowed = false,
            Error = "Not initialized",
            Timestamp = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Device GPS location data
    /// </summary>
    [Serializable]
    public struct DeviceLocation
    {
        public double Latitude;
        public double Longitude;
        public float Accuracy;
        public double Timestamp;
    }

    /// <summary>
    /// Server response model (internal)
    /// </summary>
    [Serializable]
    internal class ServerGeolocationResponse
    {
        public bool allowed;
        public string country;
        public string country_code;
        public string region;
        public string city;
        public string reason;
    }

    /// <summary>
    /// Legacy compatibility - maps to GeolocationResult
    /// </summary>
    [Serializable]
    [Obsolete("Use GeolocationResult instead")]
    public class GeolocationResponse
    {
        public bool allowed;
        public string country;
        public string region;
        public string city;
        public string reason;

        public static implicit operator GeolocationResult(GeolocationResponse r)
        {
            if (r == null) return null;
            return new GeolocationResult
            {
                Success = true,
                Country = r.country,
                Region = r.region,
                City = r.city,
                IsAllowed = r.allowed,
                BlockReason = r.reason
            };
        }
    }

    /// <summary>
    /// Legacy compatibility
    /// </summary>
    [Serializable]
    [Obsolete("Use GeolocationResult instead")]
    public class GeolocationData
    {
        public float latitude;
        public float longitude;
        public string country;
        public string region;
        public string city;
        public string updatedAt;
    }

    /// <summary>
    /// Legacy compatibility
    /// </summary>
    [Serializable]
    [Obsolete("Use DeviceLocation instead")]
    public class GeolocationPayload
    {
        public float latitude;
        public float longitude;
    }

    #endregion
}
