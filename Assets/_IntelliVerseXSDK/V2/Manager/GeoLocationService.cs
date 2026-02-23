// GeoLocationService.cs
// Lightweight geolocation facade for IntelliVerse-X SDK V2
// Delegates to IVXGeolocationService for core functionality
// Version: 2.0.0

using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;
using IntelliVerseX.Backend;

namespace IntelliVerseX.Services
{
    /// <summary>
    /// Lightweight geolocation facade providing Unity events and simplified API.
    /// Delegates heavy lifting to IVXGeolocationService to avoid code duplication.
    /// 
    /// Use this for:
    /// - UnityEvent-based workflows
    /// - Inspector-configurable callbacks
    /// - Simplified access patterns
    /// 
    /// For advanced control, use IVXGeolocationService directly.
    /// </summary>
    [DefaultExecutionOrder(-50)]
    public class GeoLocationService : MonoBehaviour
    {
        #region Singleton (Thread-Safe)

        private static GeoLocationService _instance;
        private static readonly object _instanceLock = new object();
        private static bool _isQuitting;

        public static GeoLocationService Instance
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
                            _instance = FindFirstObjectByType<GeoLocationService>();
                        }
                    }
                }
                return _instance;
            }
        }

        public static bool HasInstance => _instance != null && !_isQuitting;

        #endregion

        #region Events

        [Header("Events")]
        [Tooltip("Fired when geolocation is successfully captured")]
        public UnityEvent<GeoLocationResult> OnGeolocationCaptured;

        [Tooltip("Fired when geolocation capture fails")]
        public UnityEvent<string> OnGeolocationFailed;

        [Tooltip("Fired when user is in a blocked region")]
        public UnityEvent<string> OnRegionBlocked;

        [Tooltip("Fired when location permission is denied")]
        public UnityEvent OnLocationPermissionDenied;

        #endregion

        #region Configuration

        [Header("Configuration")]
        [Tooltip("Automatically check location on start")]
        [SerializeField] private bool _checkOnStart;

        [Tooltip("Delay before initial check (seconds)")]
        [SerializeField, Range(0f, 10f)] private float _startDelay = 1f;

        [Tooltip("Minimum time between updates (seconds)")]
        [SerializeField, Range(10f, 3600f)] private float _minUpdateInterval = 60f;

        [Tooltip("Show warning in console for blocked regions")]
        [SerializeField] private bool _logBlockedWarning = true;

        #endregion

        #region State

        private DateTime _lastUpdateTime = DateTime.MinValue;
        private GeoLocationResult _cachedResult;
        private CancellationTokenSource _startupCts;
        private bool _isInitialized;

        #endregion

        #region Properties

        public bool IsInitialized => _isInitialized;
        public bool HasCachedResult => _cachedResult != null && _cachedResult.Success;
        public GeoLocationResult CachedResult => _cachedResult;

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
                }
                else if (_instance != this)
                {
                    Debug.LogWarning("[GeoLocationService] Duplicate instance destroyed");
                    Destroy(gameObject);
                    return;
                }
            }

            InitializeEvents();
        }

        private async void Start()
        {
            await InitializeAsync();
        }

        private void OnDestroy()
        {
            CleanupSubscriptions();

            _startupCts?.Cancel();
            _startupCts?.Dispose();
            _startupCts = null;

            if (_instance == this)
            {
                lock (_instanceLock)
                {
                    _instance = null;
                }
            }
        }

        private void OnApplicationQuit()
        {
            _isQuitting = true;
            _startupCts?.Cancel();
        }

        #endregion

        #region Initialization

        private void InitializeEvents()
        {
            OnGeolocationCaptured ??= new UnityEvent<GeoLocationResult>();
            OnGeolocationFailed ??= new UnityEvent<string>();
            OnRegionBlocked ??= new UnityEvent<string>();
            OnLocationPermissionDenied ??= new UnityEvent();
        }

        private async Task InitializeAsync()
        {
            if (_isInitialized) return;

            _startupCts = new CancellationTokenSource();

            try
            {
                SubscribeToGeolocationService();
                _isInitialized = true;

                if (_checkOnStart)
                {
                    if (_startDelay > 0)
                    {
                        await Task.Delay(TimeSpan.FromSeconds(_startDelay), _startupCts.Token);
                    }

                    await RefreshGeolocationAsync(_startupCts.Token);
                }
            }
            catch (OperationCanceledException)
            {
            }
            catch (Exception ex)
            {
                Debug.LogError($"[GeoLocationService] Init failed: {ex.Message}");
            }
        }

        private void SubscribeToGeolocationService()
        {
            if (!IVXGeolocationService.HasInstance) return;

            var geoService = IVXGeolocationService.Instance;
            if (geoService == null) return;

            geoService.OnLocationChecked += HandleLocationChecked;
            geoService.OnLocationError += HandleLocationError;
            geoService.OnRegionBlocked += HandleRegionBlocked;
        }

        private void CleanupSubscriptions()
        {
            if (!IVXGeolocationService.HasInstance) return;

            var geoService = IVXGeolocationService.Instance;
            if (geoService == null) return;

            geoService.OnLocationChecked -= HandleLocationChecked;
            geoService.OnLocationError -= HandleLocationError;
            geoService.OnRegionBlocked -= HandleRegionBlocked;
        }

        #endregion

        #region Event Handlers

        private void HandleLocationChecked(GeolocationResult result)
        {
            if (result == null) return;

            _cachedResult = ConvertToLocalResult(result);
            _lastUpdateTime = DateTime.UtcNow;

            try
            {
                OnGeolocationCaptured?.Invoke(_cachedResult);
            }
            catch (Exception ex)
            {
                Debug.LogError($"[GeoLocationService] Event handler error: {ex.Message}");
            }
        }

        private void HandleLocationError(string error)
        {
            try
            {
                if (error?.Contains("permission") == true || error?.Contains("denied") == true)
                {
                    OnLocationPermissionDenied?.Invoke();
                }
                
                OnGeolocationFailed?.Invoke(error ?? "Unknown error");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[GeoLocationService] Event handler error: {ex.Message}");
            }
        }

        private void HandleRegionBlocked(GeolocationResult result)
        {
            if (result == null) return;

            var reason = result.BlockReason ?? $"Region blocked: {result.Country}";

            if (_logBlockedWarning)
            {
                Debug.LogWarning($"[GeoLocationService] BLOCKED: {result.Country} ({result.CountryCode}) - {reason}");
            }

            try
            {
                OnRegionBlocked?.Invoke(reason);
            }
            catch (Exception ex)
            {
                Debug.LogError($"[GeoLocationService] Event handler error: {ex.Message}");
            }
        }

        #endregion

        #region Public API

        /// <summary>
        /// Get cached geolocation result
        /// </summary>
        public GeoLocationResult GetCachedGeolocation()
        {
            if (_cachedResult != null)
                return _cachedResult;

            if (IVXGeolocationService.HasInstance)
            {
                var coreResult = IVXGeolocationService.Instance?.GetCachedLocation();
                if (coreResult != null)
                {
                    _cachedResult = ConvertToLocalResult(coreResult);
                    return _cachedResult;
                }
            }

            return null;
        }

        /// <summary>
        /// Check if location services are available
        /// </summary>
        public bool IsLocationServicesEnabled()
        {
#if UNITY_EDITOR
            return true;
#else
            return Input.location.isEnabledByUser;
#endif
        }

        /// <summary>
        /// Get country code (e.g., "US", "IN", "DE")
        /// </summary>
        public string GetCountryCode() => _cachedResult?.CountryCode;

        /// <summary>
        /// Get full country name
        /// </summary>
        public string GetCountry() => _cachedResult?.Country;

        /// <summary>
        /// Get region/state name
        /// </summary>
        public string GetRegion() => _cachedResult?.Region;

        /// <summary>
        /// Get city name
        /// </summary>
        public string GetCity() => _cachedResult?.City;

        /// <summary>
        /// Get device model
        /// </summary>
        public string GetDeviceModel() => SystemInfo.deviceModel;

        /// <summary>
        /// Get platform name
        /// </summary>
        public string GetPlatform() => Application.platform.ToString();

        /// <summary>
        /// Check if current location is allowed
        /// </summary>
        public bool IsLocationAllowed()
        {
            return _cachedResult?.IsAllowed ?? true;
        }

        /// <summary>
        /// Get block reason (null if allowed)
        /// </summary>
        public string GetBlockReason()
        {
            if (_cachedResult == null || _cachedResult.IsAllowed)
                return null;

            return _cachedResult.BlockReason;
        }

        /// <summary>
        /// Refresh geolocation
        /// </summary>
        public async Task<GeoLocationResult> RefreshGeolocationAsync(CancellationToken ct = default)
        {
            var timeSinceLastUpdate = (DateTime.UtcNow - _lastUpdateTime).TotalSeconds;
            if (timeSinceLastUpdate < _minUpdateInterval && _cachedResult != null && _cachedResult.Success)
            {
                return _cachedResult;
            }

            if (!IVXGeolocationService.HasInstance)
            {
                var error = "Geolocation service not available";
                OnGeolocationFailed?.Invoke(error);
                return CreateErrorResult(error);
            }

            try
            {
                bool forceRefresh = timeSinceLastUpdate >= _minUpdateInterval;
                var result = await IVXGeolocationService.Instance.CheckAndUpdateLocationAsync(forceRefresh, ct);
                
                if (result != null && result.Success)
                {
                    _cachedResult = ConvertToLocalResult(result);
                    _lastUpdateTime = DateTime.UtcNow;
                }

                return _cachedResult ?? CreateErrorResult("Failed to get location");
            }
            catch (OperationCanceledException)
            {
                return CreateErrorResult("Operation cancelled");
            }
            catch (Exception ex)
            {
                var error = ex.Message;
                OnGeolocationFailed?.Invoke(error);
                return CreateErrorResult(error);
            }
        }

        /// <summary>
        /// Get formatted location string
        /// </summary>
        public string GetFormattedLocation()
        {
            if (_cachedResult == null || !_cachedResult.Success)
                return "Unknown Location";

            if (!string.IsNullOrEmpty(_cachedResult.City))
                return $"{_cachedResult.City}, {_cachedResult.Region}, {_cachedResult.Country}";

            if (!string.IsNullOrEmpty(_cachedResult.Region))
                return $"{_cachedResult.Region}, {_cachedResult.Country}";

            if (!string.IsNullOrEmpty(_cachedResult.Country))
                return _cachedResult.Country;

            return $"{_cachedResult.Latitude:F4}, {_cachedResult.Longitude:F4}";
        }

        /// <summary>
        /// Get device info summary
        /// </summary>
        public string GetDeviceInfoSummary()
        {
            return $"{SystemInfo.deviceModel} | {Application.platform} | {SystemInfo.operatingSystem}";
        }

        #endregion

        #region Helpers

        private static GeoLocationResult ConvertToLocalResult(GeolocationResult source)
        {
            if (source == null) return null;

            return new GeoLocationResult
            {
                Success = source.Success,
                Latitude = source.Latitude,
                Longitude = source.Longitude,
                Country = source.Country,
                CountryCode = source.CountryCode,
                Region = source.Region,
                City = source.City,
                IsAllowed = source.IsAllowed,
                BlockReason = source.BlockReason,
                DeviceModel = SystemInfo.deviceModel,
                Platform = Application.platform.ToString(),
                Timestamp = source.Timestamp,
                Error = source.Error
            };
        }

        private static GeoLocationResult CreateErrorResult(string error)
        {
            return new GeoLocationResult
            {
                Success = false,
                IsAllowed = false,
                Error = error,
                Timestamp = DateTime.UtcNow
            };
        }

        #endregion

        #region Debug

        [ContextMenu("Log Current Location")]
        private void DebugLogLocation()
        {
            var result = GetCachedGeolocation();
            Debug.Log(result != null 
                ? $"[GeoLocationService] {result}" 
                : "[GeoLocationService] No cached location");
        }

        [ContextMenu("Log Device Info")]
        private void DebugLogDeviceInfo()
        {
            Debug.Log($"[GeoLocationService] Device: {GetDeviceInfoSummary()}");
        }

        [ContextMenu("Force Refresh")]
        private async void DebugForceRefresh()
        {
            _lastUpdateTime = DateTime.MinValue;
            var result = await RefreshGeolocationAsync();
            Debug.Log($"[GeoLocationService] Refresh: {result}");
        }

        #endregion
    }

    #region Data Classes

    /// <summary>
    /// Geolocation result for GeoLocationService
    /// </summary>
    [Serializable]
    public class GeoLocationResult
    {
        public bool Success;
        public double Latitude;
        public double Longitude;
        public string Country;
        public string CountryCode;
        public string Region;
        public string City;
        public string Timezone;
        public bool IsAllowed;
        public string BlockReason;
        public string DeviceModel;
        public string Platform;
        public DateTime Timestamp;
        public string Error;

        public override string ToString()
        {
            if (!Success)
                return $"GeoLocationResult(Failed: {Error})";

            return $"GeoLocationResult({City}, {Region}, {Country} [{CountryCode}] | {Latitude:F4}, {Longitude:F4})";
        }

        public static GeoLocationResult Empty => new GeoLocationResult
        {
            Success = false,
            IsAllowed = false,
            Error = "Not initialized",
            Timestamp = DateTime.UtcNow
        };
    }

    #endregion
}
