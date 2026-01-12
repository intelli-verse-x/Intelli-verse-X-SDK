// IVXGeolocationService.cs
// Geolocation service for IntelliVerse-X SDK
// Captures device location and sends to Nakama for validation and storage

using System;
using System.Collections;
using System.Threading.Tasks;
using UnityEngine;
using Nakama;
using Newtonsoft.Json;

namespace IntelliVerseX.Backend
{
    /// <summary>
    /// Geolocation service for capturing and validating player location
    /// Integrates with Nakama's check_geo_and_update_profile RPC
    /// 
    /// Usage:
    ///   var service = IVXGeolocationService.Instance;
    ///   var result = await service.CheckAndUpdateLocationAsync();
    ///   if (result.allowed) {
    ///       // Player is in allowed region
    ///   } else {
    ///       // Show blocked message
    ///   }
    /// </summary>
    public class IVXGeolocationService : MonoBehaviour
    {
        private static IVXGeolocationService _instance;
        public static IVXGeolocationService Instance
        {
            get
            {
                if (_instance == null)
                {
                    var go = new GameObject("[IVXGeolocation]");
                    _instance = go.AddComponent<IVXGeolocationService>();
                    DontDestroyOnLoad(go);
                }
                return _instance;
            }
        }

        [Header("Configuration")]
        [Tooltip("Timeout for GPS location in seconds")]
        [SerializeField] private float locationTimeout = 30f;

        [Tooltip("Minimum accuracy in meters (lower is more accurate)")]
        [SerializeField] private float desiredAccuracyInMeters = 10f;

        [Tooltip("Cache location for this many seconds")]
        [SerializeField] private float cacheExpirationSeconds = 3600f; // 1 hour

        // RPC endpoint
        private const string RPC_CHECK_GEO = "check_geo_and_update_profile";

        // Cached location data
        private GeolocationResponse _cachedResponse;
        private float _lastLocationTime;

        // Events
        public event Action<GeolocationResponse> OnLocationChecked;
        public event Action<string> OnLocationError;

        /// <summary>
        /// Check device location and validate with Nakama server
        /// Automatically updates player metadata with location information
        /// </summary>
        /// <param name="forceRefresh">Force GPS check even if cached data exists</param>
        /// <returns>Geolocation response with allowed status and location details</returns>
        public async Task<GeolocationResponse> CheckAndUpdateLocationAsync(bool forceRefresh = false)
        {
            try
            {
                // Return cached if valid and not forcing refresh
                if (!forceRefresh && IsCacheValid())
                {
                    Debug.Log("[IVXGeo] Using cached location data");
                    return _cachedResponse;
                }

                // Get device location
                var location = await GetDeviceLocationAsync();
                if (!location.HasValue)
                {
                    var errorMsg = "Failed to get device location";
                    OnLocationError?.Invoke(errorMsg);
                    return new GeolocationResponse
                    {
                        allowed = false,
                        reason = "Location services unavailable"
                    };
                }

                // Send to Nakama for validation
                var response = await CheckGeolocationWithNakamaAsync(
                    location.Value.latitude,
                    location.Value.longitude
                );

                if (response != null)
                {
                    // Cache response
                    _cachedResponse = response;
                    _lastLocationTime = Time.time;

                    Debug.Log($"[IVXGeo] Location check complete - Allowed: {response.allowed}, " +
                             $"Country: {response.country}, Region: {response.region}, City: {response.city}");

                    OnLocationChecked?.Invoke(response);
                }

                return response;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[IVXGeo] Error checking location: {ex.Message}");
                OnLocationError?.Invoke(ex.Message);
                return new GeolocationResponse
                {
                    allowed = false,
                    reason = "Location check failed"
                };
            }
        }

        /// <summary>
        /// Get device GPS location
        /// </summary>
        private async Task<DeviceLocation?> GetDeviceLocationAsync()
        {
            // Check if location services are enabled
            if (!Input.location.isEnabledByUser)
            {
                Debug.LogWarning("[IVXGeo] Location services not enabled by user");
                OnLocationError?.Invoke("Location permission denied");
                return null;
            }

            Debug.Log("[IVXGeo] Starting location service...");

            // Start location service
            Input.location.Start(desiredAccuracyInMeters, desiredAccuracyInMeters);

            // Wait for initialization
            float timeout = locationTimeout;
            while (Input.location.status == LocationServiceStatus.Initializing && timeout > 0)
            {
                await Task.Delay(100);
                timeout -= 0.1f;
            }

            // Check if timed out
            if (timeout <= 0)
            {
                Debug.LogError("[IVXGeo] Location service initialization timed out");
                Input.location.Stop();
                return null;
            }

            // Check if failed
            if (Input.location.status == LocationServiceStatus.Failed)
            {
                Debug.LogError("[IVXGeo] Location service failed to initialize");
                Input.location.Stop();
                return null;
            }

            // Get location data
            var lastData = Input.location.lastData;
            Debug.Log($"[IVXGeo] Got device location - Lat: {lastData.latitude}, Lng: {lastData.longitude}, " +
                     $"Accuracy: {lastData.horizontalAccuracy}m");

            // Stop location service to save battery
            Input.location.Stop();

            return new DeviceLocation
            {
                latitude = lastData.latitude,
                longitude = lastData.longitude,
                accuracy = lastData.horizontalAccuracy,
                timestamp = lastData.timestamp
            };
        }

        /// <summary>
        /// Send location to Nakama for validation and metadata update
        /// Works for both guest and authenticated users
        /// </summary>
        private async Task<GeolocationResponse> CheckGeolocationWithNakamaAsync(float latitude, float longitude)
        {
            try
            {
                // Get Nakama client and session from IVXBackendService
                if (!IVXBackendService.Instance.IsSessionValid)
                {
                    Debug.LogWarning("[IVXGeo] Not authenticated, attempting to authenticate...");
                    bool authenticated = await IVXBackendService.Instance.EnsureAuthenticatedAsync();
                    if (!authenticated)
                    {
                        Debug.LogError("[IVXGeo] Authentication failed");
                        return new GeolocationResponse
                        {
                            allowed = false,
                            reason = "Authentication required"
                        };
                    }
                }

                var client = IVXBackendService.Instance.Client;
                var session = IVXBackendService.Instance.Session;

                if (client == null || session == null)
                {
                    Debug.LogError("[IVXGeo] Client or session is null");
                    return new GeolocationResponse
                    {
                        allowed = false,
                        reason = "Backend not initialized"
                    };
                }

                // Build payload
                var payload = new GeolocationPayload
                {
                    latitude = latitude,
                    longitude = longitude
                };

                string jsonPayload = JsonConvert.SerializeObject(payload);
                Debug.Log($"[IVXGeo] Sending location to Nakama: {jsonPayload}");

                // Call Nakama RPC
                var rpcResponse = await client.RpcAsync(session, RPC_CHECK_GEO, jsonPayload);

                if (string.IsNullOrEmpty(rpcResponse.Payload))
                {
                    Debug.LogError("[IVXGeo] Empty response from server");
                    return new GeolocationResponse
                    {
                        allowed = false,
                        reason = "Invalid server response"
                    };
                }

                Debug.Log($"[IVXGeo] Server response: {rpcResponse.Payload}");

                // Parse response
                var response = JsonConvert.DeserializeObject<GeolocationResponse>(rpcResponse.Payload);

                // Update local player metadata cache
                if (response != null && response.allowed)
                {
                    UpdateLocalMetadataCache(latitude, longitude, response);
                }

                return response;
            }
            catch (Nakama.ApiResponseException apiEx)
            {
                Debug.LogError($"[IVXGeo] Nakama API Error: {apiEx.Message} | StatusCode: {apiEx.StatusCode}");
                return new GeolocationResponse
                {
                    allowed = false,
                    reason = "Server error"
                };
            }
            catch (Exception ex)
            {
                Debug.LogError($"[IVXGeo] Error calling Nakama: {ex.Message}");
                return new GeolocationResponse
                {
                    allowed = false,
                    reason = "Connection error"
                };
            }
        }

        /// <summary>
        /// Update local metadata cache with location information
        /// </summary>
        private void UpdateLocalMetadataCache(float latitude, float longitude, GeolocationResponse response)
        {
            try
            {
                // Store in player prefs for quick access
                PlayerPrefs.SetFloat("player_latitude", latitude);
                PlayerPrefs.SetFloat("player_longitude", longitude);
                PlayerPrefs.SetString("player_country", response.country ?? "");
                PlayerPrefs.SetString("player_region", response.region ?? "");
                PlayerPrefs.SetString("player_city", response.city ?? "");
                PlayerPrefs.SetString("player_location_updated", DateTime.UtcNow.ToString("o"));
                PlayerPrefs.Save();

                Debug.Log("[IVXGeo] Updated local metadata cache");
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[IVXGeo] Failed to update local cache: {ex.Message}");
            }
        }

        /// <summary>
        /// Check if cached location data is still valid
        /// </summary>
        private bool IsCacheValid()
        {
            if (_cachedResponse == null)
                return false;

            float timeSinceLastCheck = Time.time - _lastLocationTime;
            return timeSinceLastCheck < cacheExpirationSeconds;
        }

        /// <summary>
        /// Get cached location data from PlayerPrefs
        /// </summary>
        public static GeolocationData GetCachedLocation()
        {
            if (!PlayerPrefs.HasKey("player_latitude"))
                return null;

            return new GeolocationData
            {
                latitude = PlayerPrefs.GetFloat("player_latitude"),
                longitude = PlayerPrefs.GetFloat("player_longitude"),
                country = PlayerPrefs.GetString("player_country"),
                region = PlayerPrefs.GetString("player_region"),
                city = PlayerPrefs.GetString("player_city"),
                updatedAt = PlayerPrefs.GetString("player_location_updated")
            };
        }

        /// <summary>
        /// Clear cached location data
        /// </summary>
        public void ClearCache()
        {
            _cachedResponse = null;
            _lastLocationTime = 0;
            PlayerPrefs.DeleteKey("player_latitude");
            PlayerPrefs.DeleteKey("player_longitude");
            PlayerPrefs.DeleteKey("player_country");
            PlayerPrefs.DeleteKey("player_region");
            PlayerPrefs.DeleteKey("player_city");
            PlayerPrefs.DeleteKey("player_location_updated");
            PlayerPrefs.Save();
            Debug.Log("[IVXGeo] Cleared location cache");
        }

        private void Awake()
        {
            if (_instance == null)
            {
                _instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else if (_instance != this)
            {
                Destroy(gameObject);
            }
        }
    }

    // ============================================================================
    // DATA MODELS
    // ============================================================================

    /// <summary>
    /// Payload sent to Nakama check_geo_and_update_profile RPC
    /// </summary>
    [Serializable]
    public class GeolocationPayload
    {
        public float latitude;
        public float longitude;
    }

    /// <summary>
    /// Response from Nakama check_geo_and_update_profile RPC
    /// </summary>
    [Serializable]
    public class GeolocationResponse
    {
        /// <summary>
        /// Whether the player is allowed to play from this location
        /// </summary>
        public bool allowed;

        /// <summary>
        /// Country code (e.g., "US", "FR", "DE")
        /// </summary>
        public string country;

        /// <summary>
        /// Region/State name (e.g., "Texas", "California")
        /// </summary>
        public string region;

        /// <summary>
        /// City name (e.g., "Houston", "Paris")
        /// </summary>
        public string city;

        /// <summary>
        /// Reason for blocking if not allowed
        /// </summary>
        public string reason;
    }

    /// <summary>
    /// Device location data from Unity Input.location
    /// </summary>
    [Serializable]
    public struct DeviceLocation
    {
        public float latitude;
        public float longitude;
        public float accuracy;
        public double timestamp;
    }

    /// <summary>
    /// Cached geolocation data
    /// </summary>
    [Serializable]
    public class GeolocationData
    {
        public float latitude;
        public float longitude;
        public string country;
        public string region;
        public string city;
        public string updatedAt;
    }
}
