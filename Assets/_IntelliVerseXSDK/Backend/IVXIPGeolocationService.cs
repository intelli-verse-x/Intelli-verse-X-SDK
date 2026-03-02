// IVXIPGeolocationService.cs
// Ultra-optimized IP-based geolocation service for IntelliVerseX SDK
// Uses multiple free APIs with intelligent fallback and parallel fetching
// Production-ready with proper error handling and caching
// Version: 5.1.0

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace IntelliVerseX.Backend
{
    /// <summary>
    /// High-performance IP-based geolocation service using multiple free APIs.
    /// Optimized for speed with parallel fetching and intelligent fallback.
    /// 
    /// Features:
    /// - Multiple free API providers with automatic fallback
    /// - Parallel fetching for fastest response
    /// - Intelligent caching with configurable TTL
    /// - Non-blocking async operations
    /// - Zero-allocation hot paths where possible
    /// - Comprehensive error handling
    /// - Thread-safe singleton pattern
    /// 
    /// API Priority (by reliability and speed):
    /// 1. ip-api.com (45 req/min, HTTP only, fast)
    /// 2. ipapi.co (1000 req/day, HTTPS)
    /// 3. geojs.io (unlimited, HTTPS) 
    /// 4. geoplugin.net (unlimited, HTTP)
    /// 5. ipinfo.io (50k/month, HTTPS)
    /// 
    /// Usage:
    ///   var result = await IVXIPGeolocationService.Instance.GetLocationAsync();
    ///   Debug.Log($"Country: {result.Country}, City: {result.City}");
    /// </summary>
    public class IVXIPGeolocationService : MonoBehaviour
    {
        #region Singleton (Thread-Safe)

        private static IVXIPGeolocationService _instance;
        private static readonly object _instanceLock = new object();
        private static bool _isQuitting;

        public static IVXIPGeolocationService Instance
        {
            get
            {
                if (_isQuitting) return null;

                if (_instance == null)
                {
                    lock (_instanceLock)
                    {
                        if (_instance == null)
                        {
                            _instance = FindFirstObjectByType<IVXIPGeolocationService>();

                            if (_instance == null)
                            {
                                var go = new GameObject("[IVXIPGeolocation]");
                                _instance = go.AddComponent<IVXIPGeolocationService>();
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

        [Header("Fetching Strategy")]
        [Tooltip("Use parallel fetching for fastest response (recommended)")]
        [SerializeField] private bool _useParallelFetching = true;

        [Tooltip("Timeout per API request in seconds")]
        [SerializeField, Range(2f, 30f)] private float _requestTimeout = 5f;

        [Tooltip("Overall operation timeout in seconds")]
        [SerializeField, Range(5f, 60f)] private float _overallTimeout = 15f;

        [Header("Caching")]
        [Tooltip("Cache duration in seconds (0 = no caching)")]
        [SerializeField, Range(0f, 86400f)] private float _cacheDurationSeconds = 3600f;

        [Header("Retry")]
        [Tooltip("Retry failed requests")]
        [SerializeField] private bool _enableRetry = true;

        [Tooltip("Maximum retry attempts per API")]
        [SerializeField, Range(0, 3)] private int _maxRetryPerApi = 1;

        [Header("Debug")]
        [SerializeField] private bool _enableVerboseLogging;

        #endregion

        #region Constants

        // PlayerPrefs keys for caching
        private const string PREF_IP = "ivx_ipgeo_ip";
        private const string PREF_COUNTRY = "ivx_ipgeo_country";
        private const string PREF_COUNTRY_CODE = "ivx_ipgeo_cc";
        private const string PREF_REGION = "ivx_ipgeo_region";
        private const string PREF_CITY = "ivx_ipgeo_city";
        private const string PREF_LAT = "ivx_ipgeo_lat";
        private const string PREF_LON = "ivx_ipgeo_lon";
        private const string PREF_TIMEZONE = "ivx_ipgeo_tz";
        private const string PREF_ISP = "ivx_ipgeo_isp";
        private const string PREF_TIMESTAMP = "ivx_ipgeo_ts";
        private const string PREF_PROVIDER = "ivx_ipgeo_provider";

        #endregion

        #region API Providers

        /// <summary>
        /// API provider configuration - ordered by priority
        /// </summary>
        private static readonly List<GeoApiProvider> _providers = new List<GeoApiProvider>
        {
            // Tier 1: Fast and reliable free APIs
            new GeoApiProvider
            {
                Name = "ip-api",
                Url = "http://ip-api.com/json/?fields=status,message,country,countryCode,region,regionName,city,lat,lon,timezone,isp,query",
                Priority = 1,
                Parser = ParseIpApi
            },
            new GeoApiProvider
            {
                Name = "ipapi.co",
                Url = "https://ipapi.co/json/",
                Priority = 2,
                Parser = ParseIpapiCo
            },
            new GeoApiProvider
            {
                Name = "GeoJS",
                Url = "https://get.geojs.io/v1/ip/geo.json",
                Priority = 3,
                Parser = ParseGeoJs
            },
            
            // Tier 2: Backup APIs
            new GeoApiProvider
            {
                Name = "geoPlugin",
                Url = "http://www.geoplugin.net/json.gp",
                Priority = 4,
                Parser = ParseGeoPlugin
            },
            new GeoApiProvider
            {
                Name = "ipinfo.io",
                Url = "https://ipinfo.io/json",
                Priority = 5,
                Parser = ParseIpInfo
            },
            
            // Tier 3: Fallback for country only
            new GeoApiProvider
            {
                Name = "Country.is",
                Url = "https://api.country.is/",
                Priority = 6,
                Parser = ParseCountryIs
            }
        };

        #endregion

        #region State

        private IPGeolocationResult _cachedResult;
        private float _lastFetchTime = float.MinValue;
        private readonly object _stateLock = new object();
        private volatile bool _isOperationInProgress;
        private CancellationTokenSource _currentCts;

        #endregion

        #region Events

        /// <summary>
        /// Fired when location is successfully fetched
        /// </summary>
        public event Action<IPGeolocationResult> OnLocationFetched;

        /// <summary>
        /// Fired when all APIs fail
        /// </summary>
        public event Action<string> OnLocationError;

        /// <summary>
        /// Fired when fetching starts
        /// </summary>
        public event Action OnFetchStarted;

        /// <summary>
        /// Fired when fetching completes (success or failure)
        /// </summary>
        public event Action OnFetchCompleted;

        #endregion

        #region Properties

        public bool IsOperationInProgress => _isOperationInProgress;
        public bool HasCachedResult => _cachedResult != null && _cachedResult.Success;
        public IPGeolocationResult CachedResult => _cachedResult;

        public bool IsCacheValid
        {
            get
            {
                if (_cachedResult == null || !_cachedResult.Success)
                    return false;

                float elapsed = Time.realtimeSinceStartup - _lastFetchTime;
                return elapsed < _cacheDurationSeconds;
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
                    if (transform.parent != null)
                    {
                        transform.SetParent(null, true);
                    }
                    DontDestroyOnLoad(gameObject);
                    LoadCachedResult();
                }
                else if (_instance != this)
                {
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
        /// Get IP-based location asynchronously.
        /// Uses cached result if valid, otherwise fetches from APIs.
        /// </summary>
        /// <param name="forceRefresh">Force new fetch even if cache is valid</param>
        /// <param name="ct">Cancellation token</param>
        /// <returns>Geolocation result</returns>
        public async Task<IPGeolocationResult> GetLocationAsync(
            bool forceRefresh = false,
            CancellationToken ct = default)
        {
            if (_isQuitting)
            {
                return CreateErrorResult("Application is quitting");
            }

            // Return cache if valid
            if (!forceRefresh && IsCacheValid)
            {
                LogVerbose($"Returning cached result from {_cachedResult.Provider}");
                return _cachedResult;
            }

            // Wait if operation in progress
            if (_isOperationInProgress)
            {
                LogVerbose("Operation in progress, waiting...");
                return await WaitForCurrentOperationAsync(ct);
            }

            return await FetchLocationInternalAsync(ct);
        }

        /// <summary>
        /// Get cached location without triggering fetch.
        /// Returns null if no cached result.
        /// </summary>
        public IPGeolocationResult GetCachedLocation()
        {
            lock (_stateLock)
            {
                return _cachedResult;
            }
        }

        /// <summary>
        /// Start fetching location in background (fire and forget).
        /// Safe to call multiple times - will not duplicate requests.
        /// </summary>
        public void FetchLocationInBackground()
        {
            if (_isOperationInProgress || IsCacheValid) return;
            _ = GetLocationAsync(false);
        }

        /// <summary>
        /// Clear all cached data
        /// </summary>
        public void ClearCache()
        {
            lock (_stateLock)
            {
                _cachedResult = null;
                _lastFetchTime = float.MinValue;
            }
            ClearPrefsCache();
            LogVerbose("Cache cleared");
        }

        /// <summary>
        /// Cancel ongoing operation
        /// </summary>
        public void CancelCurrentOperation()
        {
            try
            {
                _currentCts?.Cancel();
                _currentCts?.Dispose();
                _currentCts = null;
            }
            catch (ObjectDisposedException) { }
            finally
            {
                _isOperationInProgress = false;
            }
        }

        #endregion

        #region Core Implementation

        private async Task<IPGeolocationResult> FetchLocationInternalAsync(CancellationToken ct)
        {
            _isOperationInProgress = true;
            _currentCts = CancellationTokenSource.CreateLinkedTokenSource(ct);
            
            OnFetchStarted?.Invoke();
            LogVerbose("Starting IP geolocation fetch...");

            try
            {
                ct.ThrowIfCancellationRequested();

                IPGeolocationResult result;

                if (_useParallelFetching)
                {
                    result = await FetchParallelAsync(_currentCts.Token);
                }
                else
                {
                    result = await FetchSequentialAsync(_currentCts.Token);
                }

                if (result.Success)
                {
                    lock (_stateLock)
                    {
                        _cachedResult = result;
                        _lastFetchTime = Time.realtimeSinceStartup;
                    }
                    SaveResultToPrefs(result);
                    LogVerbose($"Location fetched: {result.Country}, {result.City} (via {result.Provider})");
                    OnLocationFetched?.Invoke(result);
                }
                else
                {
                    LogVerbose($"All APIs failed: {result.Error}");
                    OnLocationError?.Invoke(result.Error);
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
                Debug.LogError($"[IVXIPGeo] Error: {ex.Message}");
                OnLocationError?.Invoke(ex.Message);
                return CreateErrorResult(ex.Message);
            }
            finally
            {
                _isOperationInProgress = false;
                _currentCts?.Dispose();
                _currentCts = null;
                OnFetchCompleted?.Invoke();
            }
        }

        /// <summary>
        /// Fetch from multiple APIs in parallel - returns first successful result
        /// </summary>
        private async Task<IPGeolocationResult> FetchParallelAsync(CancellationToken ct)
        {
            LogVerbose("Using parallel fetch strategy");

            // Split into tiers for smarter parallel fetching
            var tier1 = _providers.FindAll(p => p.Priority <= 3);
            var tier2 = _providers.FindAll(p => p.Priority > 3);

            // First try tier 1 in parallel
            var tier1Tasks = new List<Task<IPGeolocationResult>>();
            foreach (var provider in tier1)
            {
                tier1Tasks.Add(FetchFromProviderAsync(provider, ct));
            }

            // Wait for first successful result from tier 1
            var result = await WaitForFirstSuccessAsync(tier1Tasks, ct);
            if (result != null && result.Success)
            {
                return result;
            }

            // If tier 1 failed, try tier 2
            LogVerbose("Tier 1 APIs failed, trying tier 2...");
            var tier2Tasks = new List<Task<IPGeolocationResult>>();
            foreach (var provider in tier2)
            {
                tier2Tasks.Add(FetchFromProviderAsync(provider, ct));
            }

            result = await WaitForFirstSuccessAsync(tier2Tasks, ct);
            if (result != null && result.Success)
            {
                return result;
            }

            return CreateErrorResult("All geolocation APIs failed");
        }

        /// <summary>
        /// Wait for first successful result from a list of tasks
        /// </summary>
        private async Task<IPGeolocationResult> WaitForFirstSuccessAsync(
            List<Task<IPGeolocationResult>> tasks, 
            CancellationToken ct)
        {
            var pendingTasks = new List<Task<IPGeolocationResult>>(tasks);
            var errors = new List<string>();

            while (pendingTasks.Count > 0)
            {
                ct.ThrowIfCancellationRequested();

                var completedTask = await Task.WhenAny(pendingTasks);
                pendingTasks.Remove(completedTask);

                try
                {
                    var result = await completedTask;
                    if (result != null && result.Success)
                    {
                        // Cancel remaining tasks (we don't need them)
                        return result;
                    }
                    if (result != null && !string.IsNullOrEmpty(result.Error))
                    {
                        errors.Add(result.Error);
                    }
                }
                catch (Exception ex)
                {
                    errors.Add(ex.Message);
                }
            }

            return CreateErrorResult(string.Join("; ", errors));
        }

        /// <summary>
        /// Fetch from APIs sequentially by priority (fallback strategy)
        /// </summary>
        private async Task<IPGeolocationResult> FetchSequentialAsync(CancellationToken ct)
        {
            LogVerbose("Using sequential fetch strategy");

            var errors = new List<string>();

            foreach (var provider in _providers)
            {
                ct.ThrowIfCancellationRequested();

                var result = await FetchFromProviderAsync(provider, ct);
                if (result.Success)
                {
                    return result;
                }

                errors.Add($"{provider.Name}: {result.Error}");
                LogVerbose($"{provider.Name} failed: {result.Error}");
            }

            return CreateErrorResult("All APIs failed: " + string.Join("; ", errors));
        }

        /// <summary>
        /// Fetch from a single API provider with optional retry
        /// </summary>
        private async Task<IPGeolocationResult> FetchFromProviderAsync(
            GeoApiProvider provider, 
            CancellationToken ct)
        {
            int attempts = 0;
            int maxAttempts = _enableRetry ? _maxRetryPerApi + 1 : 1;

            while (attempts < maxAttempts)
            {
                attempts++;
                ct.ThrowIfCancellationRequested();

                LogVerbose($"Fetching from {provider.Name} (attempt {attempts})...");

                try
                {
                    var result = await FetchFromUrlAsync(provider, ct);
                    if (result.Success)
                    {
                        return result;
                    }

                    // Don't retry on parse errors
                    if (result.Error?.Contains("parse") == true)
                    {
                        return result;
                    }
                }
                catch (OperationCanceledException)
                {
                    throw;
                }
                catch (Exception ex)
                {
                    if (attempts >= maxAttempts)
                    {
                        return CreateErrorResult($"{provider.Name}: {ex.Message}");
                    }
                }

                // Wait before retry
                if (attempts < maxAttempts)
                {
                    await Task.Delay(200 * attempts, ct);
                }
            }

            return CreateErrorResult($"{provider.Name}: Max retries exceeded");
        }

        /// <summary>
        /// Actual HTTP fetch from URL
        /// </summary>
        private async Task<IPGeolocationResult> FetchFromUrlAsync(
            GeoApiProvider provider, 
            CancellationToken ct)
        {
            using (var request = UnityWebRequest.Get(provider.Url))
            {
                request.timeout = Mathf.RoundToInt(_requestTimeout);
                request.SetRequestHeader("User-Agent", "IntelliVerseX-SDK/1.0");
                request.SetRequestHeader("Accept", "application/json");

                var operation = request.SendWebRequest();

                // Wait with cancellation support
                while (!operation.isDone)
                {
                    ct.ThrowIfCancellationRequested();
                    await Task.Yield();
                }

                if (request.result != UnityWebRequest.Result.Success)
                {
                    return CreateErrorResult($"{provider.Name}: {request.error}");
                }

                var json = request.downloadHandler.text;
                
                if (string.IsNullOrWhiteSpace(json))
                {
                    return CreateErrorResult($"{provider.Name}: Empty response");
                }

                LogVerbose($"{provider.Name} response: {json.Substring(0, Math.Min(200, json.Length))}...");

                try
                {
                    var result = provider.Parser(json);
                    if (result != null)
                    {
                        result.Provider = provider.Name;
                        result.FetchTime = DateTime.UtcNow;
                    }
                    return result ?? CreateErrorResult($"{provider.Name}: Failed to parse");
                }
                catch (Exception ex)
                {
                    return CreateErrorResult($"{provider.Name}: Parse error - {ex.Message}");
                }
            }
        }

        #endregion

        #region API Parsers

        /// <summary>
        /// Parser for ip-api.com
        /// </summary>
        private static IPGeolocationResult ParseIpApi(string json)
        {
            var data = JObject.Parse(json);
            
            if (data["status"]?.ToString() != "success")
            {
                return CreateErrorResult($"ip-api: {data["message"]}");
            }

            return new IPGeolocationResult
            {
                Success = true,
                IP = data["query"]?.ToString() ?? "",
                Country = data["country"]?.ToString() ?? "",
                CountryCode = data["countryCode"]?.ToString() ?? "",
                Region = data["regionName"]?.ToString() ?? "",
                RegionCode = data["region"]?.ToString() ?? "",
                City = data["city"]?.ToString() ?? "",
                Latitude = ParseDouble(data["lat"]),
                Longitude = ParseDouble(data["lon"]),
                Timezone = data["timezone"]?.ToString() ?? "",
                ISP = data["isp"]?.ToString() ?? ""
            };
        }

        /// <summary>
        /// Parser for ipapi.co
        /// </summary>
        private static IPGeolocationResult ParseIpapiCo(string json)
        {
            var data = JObject.Parse(json);

            // Check for error
            if (data["error"] != null && (bool)data["error"])
            {
                return CreateErrorResult($"ipapi.co: {data["reason"]}");
            }

            return new IPGeolocationResult
            {
                Success = true,
                IP = data["ip"]?.ToString() ?? "",
                Country = data["country_name"]?.ToString() ?? "",
                CountryCode = data["country_code"]?.ToString() ?? "",
                Region = data["region"]?.ToString() ?? "",
                RegionCode = data["region_code"]?.ToString() ?? "",
                City = data["city"]?.ToString() ?? "",
                Latitude = ParseDouble(data["latitude"]),
                Longitude = ParseDouble(data["longitude"]),
                Timezone = data["timezone"]?.ToString() ?? "",
                ISP = data["org"]?.ToString() ?? "",
                Currency = data["currency"]?.ToString() ?? "",
                Languages = data["languages"]?.ToString() ?? ""
            };
        }

        /// <summary>
        /// Parser for geojs.io
        /// </summary>
        private static IPGeolocationResult ParseGeoJs(string json)
        {
            var data = JObject.Parse(json);

            return new IPGeolocationResult
            {
                Success = true,
                IP = data["ip"]?.ToString() ?? "",
                Country = data["country"]?.ToString() ?? "",
                CountryCode = data["country_code"]?.ToString() ?? "",
                Region = data["region"]?.ToString() ?? "",
                City = data["city"]?.ToString() ?? "",
                Latitude = ParseDouble(data["latitude"]),
                Longitude = ParseDouble(data["longitude"]),
                Timezone = data["timezone"]?.ToString() ?? "",
                ISP = data["organization"]?.ToString() ?? ""
            };
        }

        /// <summary>
        /// Parser for geoplugin.net
        /// </summary>
        private static IPGeolocationResult ParseGeoPlugin(string json)
        {
            var data = JObject.Parse(json);

            // geoPlugin returns 404 status on error
            if (data["geoplugin_status"]?.ToString() == "404")
            {
                return CreateErrorResult("geoPlugin: Location not found");
            }

            return new IPGeolocationResult
            {
                Success = true,
                IP = data["geoplugin_request"]?.ToString() ?? "",
                Country = data["geoplugin_countryName"]?.ToString() ?? "",
                CountryCode = data["geoplugin_countryCode"]?.ToString() ?? "",
                Region = data["geoplugin_regionName"]?.ToString() ?? "",
                RegionCode = data["geoplugin_regionCode"]?.ToString() ?? "",
                City = data["geoplugin_city"]?.ToString() ?? "",
                Latitude = ParseDouble(data["geoplugin_latitude"]),
                Longitude = ParseDouble(data["geoplugin_longitude"]),
                Timezone = data["geoplugin_timezone"]?.ToString() ?? "",
                Currency = data["geoplugin_currencyCode"]?.ToString() ?? ""
            };
        }

        /// <summary>
        /// Parser for ipinfo.io
        /// </summary>
        private static IPGeolocationResult ParseIpInfo(string json)
        {
            var data = JObject.Parse(json);

            // Check for error
            if (data["error"] != null)
            {
                return CreateErrorResult($"ipinfo.io: {data["error"]["message"]}");
            }

            // Parse location (format: "lat,lon")
            double lat = 0, lon = 0;
            var loc = data["loc"]?.ToString();
            if (!string.IsNullOrEmpty(loc))
            {
                var parts = loc.Split(',');
                if (parts.Length == 2)
                {
                    double.TryParse(parts[0], out lat);
                    double.TryParse(parts[1], out lon);
                }
            }

            return new IPGeolocationResult
            {
                Success = true,
                IP = data["ip"]?.ToString() ?? "",
                Country = data["country"]?.ToString() ?? "",
                CountryCode = data["country"]?.ToString() ?? "",
                Region = data["region"]?.ToString() ?? "",
                City = data["city"]?.ToString() ?? "",
                Latitude = lat,
                Longitude = lon,
                Timezone = data["timezone"]?.ToString() ?? "",
                ISP = data["org"]?.ToString() ?? ""
            };
        }

        /// <summary>
        /// Parser for country.is (country only)
        /// </summary>
        private static IPGeolocationResult ParseCountryIs(string json)
        {
            var data = JObject.Parse(json);

            var countryCode = data["country"]?.ToString();
            if (string.IsNullOrEmpty(countryCode))
            {
                return CreateErrorResult("country.is: No country returned");
            }

            return new IPGeolocationResult
            {
                Success = true,
                IP = data["ip"]?.ToString() ?? "",
                CountryCode = countryCode,
                Country = countryCode // Country.is only returns code
            };
        }

        private static double ParseDouble(JToken token)
        {
            if (token == null) return 0;
            
            var str = token.ToString();
            if (string.IsNullOrEmpty(str)) return 0;
            
            double.TryParse(str, out double result);
            return result;
        }

        #endregion

        #region Persistence

        private void LoadCachedResult()
        {
            try
            {
                if (!PlayerPrefs.HasKey(PREF_COUNTRY))
                    return;

                var timestamp = PlayerPrefs.GetString(PREF_TIMESTAMP, "");
                if (DateTime.TryParse(timestamp, out DateTime cachedTime))
                {
                    // Check if cache is still valid
                    var elapsed = (DateTime.UtcNow - cachedTime).TotalSeconds;
                    if (elapsed > _cacheDurationSeconds)
                    {
                        LogVerbose("Cached result expired");
                        return;
                    }
                }

                var result = new IPGeolocationResult
                {
                    Success = true,
                    IP = PlayerPrefs.GetString(PREF_IP, ""),
                    Country = PlayerPrefs.GetString(PREF_COUNTRY, ""),
                    CountryCode = PlayerPrefs.GetString(PREF_COUNTRY_CODE, ""),
                    Region = PlayerPrefs.GetString(PREF_REGION, ""),
                    City = PlayerPrefs.GetString(PREF_CITY, ""),
                    Latitude = PlayerPrefs.GetFloat(PREF_LAT, 0),
                    Longitude = PlayerPrefs.GetFloat(PREF_LON, 0),
                    Timezone = PlayerPrefs.GetString(PREF_TIMEZONE, ""),
                    ISP = PlayerPrefs.GetString(PREF_ISP, ""),
                    Provider = PlayerPrefs.GetString(PREF_PROVIDER, "cache"),
                    FetchTime = cachedTime
                };

                lock (_stateLock)
                {
                    _cachedResult = result;
                    _lastFetchTime = Time.realtimeSinceStartup;
                }

                LogVerbose($"Loaded cached result: {result.Country}, {result.City}");
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[IVXIPGeo] Failed to load cache: {ex.Message}");
            }
        }

        private void SaveResultToPrefs(IPGeolocationResult result)
        {
            if (result == null || !result.Success) return;

            try
            {
                PlayerPrefs.SetString(PREF_IP, result.IP ?? "");
                PlayerPrefs.SetString(PREF_COUNTRY, result.Country ?? "");
                PlayerPrefs.SetString(PREF_COUNTRY_CODE, result.CountryCode ?? "");
                PlayerPrefs.SetString(PREF_REGION, result.Region ?? "");
                PlayerPrefs.SetString(PREF_CITY, result.City ?? "");
                PlayerPrefs.SetFloat(PREF_LAT, (float)result.Latitude);
                PlayerPrefs.SetFloat(PREF_LON, (float)result.Longitude);
                PlayerPrefs.SetString(PREF_TIMEZONE, result.Timezone ?? "");
                PlayerPrefs.SetString(PREF_ISP, result.ISP ?? "");
                PlayerPrefs.SetString(PREF_PROVIDER, result.Provider ?? "");
                PlayerPrefs.SetString(PREF_TIMESTAMP, DateTime.UtcNow.ToString("O"));
                PlayerPrefs.Save();
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[IVXIPGeo] Failed to save cache: {ex.Message}");
            }
        }

        private void ClearPrefsCache()
        {
            try
            {
                PlayerPrefs.DeleteKey(PREF_IP);
                PlayerPrefs.DeleteKey(PREF_COUNTRY);
                PlayerPrefs.DeleteKey(PREF_COUNTRY_CODE);
                PlayerPrefs.DeleteKey(PREF_REGION);
                PlayerPrefs.DeleteKey(PREF_CITY);
                PlayerPrefs.DeleteKey(PREF_LAT);
                PlayerPrefs.DeleteKey(PREF_LON);
                PlayerPrefs.DeleteKey(PREF_TIMEZONE);
                PlayerPrefs.DeleteKey(PREF_ISP);
                PlayerPrefs.DeleteKey(PREF_PROVIDER);
                PlayerPrefs.DeleteKey(PREF_TIMESTAMP);
                PlayerPrefs.Save();
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[IVXIPGeo] Failed to clear cache: {ex.Message}");
            }
        }

        #endregion

        #region Helpers

        private async Task<IPGeolocationResult> WaitForCurrentOperationAsync(CancellationToken ct)
        {
            int waitAttempts = 0;
            const int maxWaitAttempts = 150; // 15 seconds max

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

        private static IPGeolocationResult CreateErrorResult(string error)
        {
            return new IPGeolocationResult
            {
                Success = false,
                Error = error ?? "Unknown error",
                FetchTime = DateTime.UtcNow
            };
        }

        private void LogVerbose(string message)
        {
            if (_enableVerboseLogging)
            {
                Debug.Log($"[IVXIPGeo] {message}");
            }
        }

        #endregion

        #region Debug

        [ContextMenu("Debug: Log Cache Status")]
        private void DebugLogCacheStatus()
        {
            Debug.Log($"[IVXIPGeo] Cache Valid: {IsCacheValid}, Has Result: {HasCachedResult}, In Progress: {_isOperationInProgress}");
            if (_cachedResult != null)
            {
                Debug.Log($"[IVXIPGeo] Cached: {_cachedResult}");
            }
        }

        [ContextMenu("Debug: Clear Cache")]
        private void DebugClearCache()
        {
            ClearCache();
            Debug.Log("[IVXIPGeo] Cache cleared");
        }

        [ContextMenu("Debug: Force Fetch")]
        private async void DebugForceFetch()
        {
            var result = await GetLocationAsync(true);
            Debug.Log($"[IVXIPGeo] Force fetch result: {result}");
        }

        [ContextMenu("Debug: Test All APIs")]
        private async void DebugTestAllApis()
        {
            Debug.Log("[IVXIPGeo] Testing all APIs...");
            
            foreach (var provider in _providers)
            {
                var result = await FetchFromProviderAsync(provider, CancellationToken.None);
                Debug.Log($"[IVXIPGeo] {provider.Name}: {(result.Success ? $"✓ {result.Country}, {result.City}" : $"✗ {result.Error}")}");
            }
        }

        #endregion
    }

    #region Data Models

    /// <summary>
    /// Result from IP-based geolocation
    /// </summary>
    [Serializable]
    public class IPGeolocationResult
    {
        // Status
        public bool Success;
        public string Error;

        // Location data
        public string IP;
        public string Country;
        public string CountryCode;
        public string Region;
        public string RegionCode;
        public string City;
        public double Latitude;
        public double Longitude;

        // Additional data
        public string Timezone;
        public string ISP;
        public string Currency;
        public string Languages;

        // Metadata
        public string Provider;
        public DateTime FetchTime;

        /// <summary>
        /// Get formatted location string
        /// </summary>
        public string GetLocationString()
        {
            if (!Success) return "Unknown";

            var parts = new List<string>();
            if (!string.IsNullOrEmpty(City)) parts.Add(City);
            if (!string.IsNullOrEmpty(Region)) parts.Add(Region);
            if (!string.IsNullOrEmpty(Country)) parts.Add(Country);

            return parts.Count > 0 ? string.Join(", ", parts) : "Unknown";
        }

        /// <summary>
        /// Get short location (City, Country)
        /// </summary>
        public string GetShortLocation()
        {
            if (!Success) return "Unknown";

            if (!string.IsNullOrEmpty(City) && !string.IsNullOrEmpty(Country))
                return $"{City}, {Country}";
            if (!string.IsNullOrEmpty(Country))
                return Country;
            if (!string.IsNullOrEmpty(CountryCode))
                return CountryCode;

            return "Unknown";
        }

        public override string ToString()
        {
            if (!Success)
                return $"IPGeolocationResult(Failed: {Error})";

            return $"IPGeolocationResult({GetShortLocation()} | {IP} | via {Provider})";
        }
    }

    /// <summary>
    /// API provider configuration
    /// </summary>
    internal class GeoApiProvider
    {
        public string Name;
        public string Url;
        public int Priority;
        public Func<string, IPGeolocationResult> Parser;
    }

    #endregion
}
