using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;
using IntelliVerseX.Backend.Nakama;

namespace IntelliVerseX.Services
{
    /// <summary>
    /// Geolocation service for manual location operations and events. 
    /// Main geolocation is handled automatically by IVXNManager during initialization.
    /// This service provides additional utilities and events. 
    /// </summary>
    public class GeoLocationService : MonoBehaviour
    {
        #region Singleton

        public static GeoLocationService Instance { get; private set; }

        #endregion

        #region Events

        [Header("Events")]
        [Tooltip("Fired when geolocation is successfully captured")]
        public UnityEvent<GeolocationResult> OnGeolocationCaptured;

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
        [SerializeField] private bool checkOnStart = false;

        [Tooltip("Show warning dialog if location is blocked")]
        [SerializeField] private bool showBlockedWarning = true;

        [Tooltip("Minimum time between location updates (seconds)")]
        [SerializeField] private float minUpdateInterval = 60f;

        #endregion

        #region State

        private DateTime _lastUpdateTime = DateTime.MinValue;
        private bool _isUpdating = false;
        private GeolocationResult _cachedResult;

        #endregion

        #region Data Classes

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
                    return $"GeolocationResult(Failed: {Error})";

                return $"GeolocationResult({City}, {Region}, {Country} [{CountryCode}] | {Latitude:F4}, {Longitude:F4})";
            }
        }

        #endregion

        #region Unity Lifecycle

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Debug.LogWarning("[GeoLocationService] Duplicate instance, destroying this one.");
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private async void Start()
        {
            if (IVXNManager.Instance == null)
            {
                Debug.LogError("[GeoLocationService] IVXNManager instance not found!");
                return;
            }

            Debug.Log("[GeoLocationService] Initialized.  Geolocation is managed by IVXNManager.");

            // Subscribe to IVXNManager events
            IVXNManager.Instance.OnMetadataSyncSuccess += HandleMetadataSyncSuccess;
            IVXNManager.Instance.OnMetadataSyncFailed += HandleMetadataSyncFailed;

            if (checkOnStart)
            {
                await Task.Delay(1000); // Wait for IVXNManager to initialize
                await RefreshGeolocationAsync();
            }
        }

        private void OnDestroy()
        {
            if (IVXNManager.Instance != null)
            {
                IVXNManager.Instance.OnMetadataSyncSuccess -= HandleMetadataSyncSuccess;
                IVXNManager.Instance.OnMetadataSyncFailed -= HandleMetadataSyncFailed;
            }
        }

        #endregion

        #region Event Handlers

        private void HandleMetadataSyncSuccess()
        {
            // Update cached result from IVXNManager
            UpdateCachedResultFromManager();
        }

        private void HandleMetadataSyncFailed(string error)
        {
            Debug.LogWarning($"[GeoLocationService] Metadata sync failed: {error}");
        }

        private void UpdateCachedResultFromManager()
        {
            if (IVXNManager.Instance == null) return;

            var geoInfo = IVXNManager.Instance.GetCachedGeolocation();
            var deviceInfo = IVXNManager.Instance.GetDeviceInfo();

            if (geoInfo != null)
            {
                _cachedResult = new GeolocationResult
                {
                    Success = true,
                    Latitude = geoInfo.Latitude,
                    Longitude = geoInfo.Longitude,
                    Country = geoInfo.Country,
                    CountryCode = geoInfo.CountryCode,
                    Region = geoInfo.Region,
                    City = geoInfo.City,
                    Timezone = geoInfo.Timezone,
                    IsAllowed = geoInfo.IsAllowed,
                    BlockReason = geoInfo.BlockReason,
                    DeviceModel = deviceInfo?.DeviceModel ?? "Unknown",
                    Platform = deviceInfo?.Platform ?? "Unknown",
                    Timestamp = geoInfo.CapturedAt
                };

                Debug.Log($"[GeoLocationService] Cached: {_cachedResult}");
            }
        }

        #endregion

        #region Public API

        /// <summary>
        /// Get the current cached geolocation result
        /// </summary>
        public GeolocationResult GetCachedGeolocation()
        {
            if (_cachedResult == null)
            {
                UpdateCachedResultFromManager();
            }
            return _cachedResult;
        }

        /// <summary>
        /// Check if location services are available
        /// </summary>
        public bool IsLocationServicesEnabled()
        {
            return Input.location.isEnabledByUser;
        }

        /// <summary>
        /// Get country code (e.g., "US", "IN", "DE")
        /// </summary>
        public string GetCountryCode()
        {
            return _cachedResult?.CountryCode;
        }

        /// <summary>
        /// Get full country name
        /// </summary>
        public string GetCountry()
        {
            return _cachedResult?.Country;
        }

        /// <summary>
        /// Get region/state name
        /// </summary>
        public string GetRegion()
        {
            return _cachedResult?.Region;
        }

        /// <summary>
        /// Get city name
        /// </summary>
        public string GetCity()
        {
            return _cachedResult?.City;
        }

        /// <summary>
        /// Get device model name
        /// </summary>
        public string GetDeviceModel()
        {
            return _cachedResult?.DeviceModel ?? IVXNManager.Instance?.GetDeviceInfo()?.DeviceModel ?? SystemInfo.deviceModel;
        }

        /// <summary>
        /// Get platform name
        /// </summary>
        public string GetPlatform()
        {
            return _cachedResult?.Platform ?? IVXNManager.Instance?.GetDeviceInfo()?.Platform ?? Application.platform.ToString();
        }

        /// <summary>
        /// Check if current location is allowed
        /// </summary>
        public bool IsLocationAllowed()
        {
            if (_cachedResult == null)
                return true; // Assume allowed if no data

            return _cachedResult.IsAllowed;
        }

        /// <summary>
        /// Get reason for location block (null if allowed)
        /// </summary>
        public string GetBlockReason()
        {
            if (_cachedResult == null || _cachedResult.IsAllowed)
                return null;

            return _cachedResult.BlockReason;
        }

        /// <summary>
        /// Refresh geolocation (capture new GPS and resolve via server)
        /// </summary>
        public async Task<GeolocationResult> RefreshGeolocationAsync()
        {
            // Rate limiting
            var timeSinceLastUpdate = (DateTime.UtcNow - _lastUpdateTime).TotalSeconds;
            if (timeSinceLastUpdate < minUpdateInterval && _cachedResult != null && _cachedResult.Success)
            {
                Debug.Log($"[GeoLocationService] Using cached result (last update was {timeSinceLastUpdate:F0}s ago)");
                return _cachedResult;
            }

            // Prevent concurrent updates
            if (_isUpdating)
            {
                Debug.LogWarning("[GeoLocationService] Update already in progress");
                return _cachedResult;
            }

            _isUpdating = true;

            try
            {
                // Check prerequisites
                if (IVXNManager.Instance == null || !IVXNManager.Instance.IsInitialized)
                {
                    var error = "IVXNManager not initialized";
                    Debug.LogError($"[GeoLocationService] {error}");
                    OnGeolocationFailed?.Invoke(error);

                    return new GeolocationResult
                    {
                        Success = false,
                        Error = error,
                        Timestamp = DateTime.UtcNow
                    };
                }

                // Check location permission
                if (!Input.location.isEnabledByUser)
                {
                    Debug.LogWarning("[GeoLocationService] Location services not enabled");
                    OnLocationPermissionDenied?.Invoke();

                    return new GeolocationResult
                    {
                        Success = false,
                        Error = "Location services not enabled",
                        Timestamp = DateTime.UtcNow
                    };
                }

                // Capture and resolve location
                Debug.Log("[GeoLocationService] Refreshing geolocation.. .");
                var geoInfo = await IVXNManager.Instance.RefreshGeolocationAsync();

                if (geoInfo == null)
                {
                    var error = "Failed to capture geolocation";
                    Debug.LogWarning($"[GeoLocationService] {error}");
                    OnGeolocationFailed?.Invoke(error);

                    return new GeolocationResult
                    {
                        Success = false,
                        Error = error,
                        Timestamp = DateTime.UtcNow
                    };
                }

                // Get device info
                var deviceInfo = IVXNManager.Instance.GetDeviceInfo();

                // Build result
                var result = new GeolocationResult
                {
                    Success = true,
                    Latitude = geoInfo.Latitude,
                    Longitude = geoInfo.Longitude,
                    Country = geoInfo.Country,
                    CountryCode = geoInfo.CountryCode,
                    Region = geoInfo.Region,
                    City = geoInfo.City,
                    Timezone = geoInfo.Timezone,
                    IsAllowed = geoInfo.IsAllowed,
                    BlockReason = geoInfo.BlockReason,
                    DeviceModel = deviceInfo?.DeviceModel ?? "Unknown",
                    Platform = deviceInfo?.Platform ?? "Unknown",
                    Timestamp = DateTime.UtcNow
                };

                // Update cache
                _cachedResult = result;
                _lastUpdateTime = DateTime.UtcNow;

                Debug.Log($"[GeoLocationService] ✓ Location refreshed: {result}");

                // Fire events
                OnGeolocationCaptured?.Invoke(result);

                if (!result.IsAllowed)
                {
                    OnRegionBlocked?.Invoke(result.BlockReason);

                    if (showBlockedWarning)
                    {
                        ShowBlockedRegionWarning(result);
                    }
                }

                return result;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[GeoLocationService] Exception: {ex.Message}");
                OnGeolocationFailed?.Invoke(ex.Message);

                return new GeolocationResult
                {
                    Success = false,
                    Error = ex.Message,
                    Timestamp = DateTime.UtcNow
                };
            }
            finally
            {
                _isUpdating = false;
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
            {
                return $"{_cachedResult.City}, {_cachedResult.Region}, {_cachedResult.Country}";
            }
            else if (!string.IsNullOrEmpty(_cachedResult.Region))
            {
                return $"{_cachedResult.Region}, {_cachedResult.Country}";
            }
            else if (!string.IsNullOrEmpty(_cachedResult.Country))
            {
                return _cachedResult.Country;
            }

            return $"{_cachedResult.Latitude:F4}, {_cachedResult.Longitude:F4}";
        }

        /// <summary>
        /// Get device info summary string
        /// </summary>
        public string GetDeviceInfoSummary()
        {
            var info = IVXNManager.Instance?.GetDeviceInfo();
            if (info == null)
                return $"{SystemInfo.deviceModel} | {Application.platform}";

            return $"{info.DeviceModel} | {info.Platform} | {info.OsVersion}";
        }

        #endregion

        #region Private Methods

        private void ShowBlockedRegionWarning(GeolocationResult result)
        {
            // You can replace this with your own UI dialog
            Debug.LogWarning($"[GeoLocationService] ⚠ BLOCKED REGION: {result.Country} ({result.CountryCode})\nReason: {result.BlockReason}");

            // Example: Show a popup dialog
            // UIManager.Instance?. ShowDialog(
            //     "Region Not Supported",
            //     $"Sorry, this game is not available in {result.Country}.\n\nReason: {result. BlockReason}",
            //     "OK"
            // );
        }

        #endregion

        #region Debug / Inspector

        [ContextMenu("Log Current Location")]
        private void DebugLogLocation()
        {
            var result = GetCachedGeolocation();
            if (result != null)
            {
                Debug.Log($"[GeoLocationService] Current: {result}");
            }
            else
            {
                Debug.Log("[GeoLocationService] No cached location");
            }
        }

        [ContextMenu("Log Device Info")]
        private void DebugLogDeviceInfo()
        {
            Debug.Log($"[GeoLocationService] Device: {GetDeviceInfoSummary()}");
        }

        [ContextMenu("Force Refresh Location")]
        private async void DebugForceRefresh()
        {
            var result = await RefreshGeolocationAsync();
            Debug.Log($"[GeoLocationService] Refresh result: {result}");
        }

        #endregion
    }
}