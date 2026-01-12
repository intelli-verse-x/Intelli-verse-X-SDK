using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using Nakama;

namespace IntelliVerseX.Analytics
{
    /// <summary>
    /// Analytics service for IntelliVerse-X SDK
    /// Uses Nakama RPC for event tracking and session analytics
    /// 
    /// RPCs used:
    /// - quizverse_log_event: Log custom events with properties
    /// - quizverse_track_session_start: Track session start
    /// - quizverse_track_session_end: Track session end with duration
    /// 
    /// Storage:
    /// - Collection: "<gameId>_analytics"
    /// - Key: "event_<userId>_<timestamp>"
    /// </summary>
    public class IVXAnalyticsManager
    {
        private static IVXAnalyticsManager _instance;
        public static IVXAnalyticsManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new IVXAnalyticsManager();
                }
                return _instance;
            }
        }

        private const string GAME_ID = "33b245c8-a23f-4f9c-a06e-189885cc22a1";
        
        private IClient _nakamaClient;
        private ISession _nakamaSession;
        private bool _isInitialized;
        private string _sessionKey;
        private float _sessionStartTime;

        // Events
        public event Action<string> OnEventTracked;
        public event Action<string> OnSessionStarted;
        public event Action<string, float> OnSessionEnded;
        public event Action<string> OnError;

        // User properties
        private Dictionary<string, object> _userProperties = new Dictionary<string, object>();

        private IVXAnalyticsManager()
        {
            // Private constructor for singleton
        }

        /// <summary>
        /// Initialize analytics with Nakama client
        /// </summary>
        public void Initialize(IClient client, ISession session)
        {
            if (_isInitialized)
            {
                Debug.LogWarning("[IVXAnalyticsManager] Already initialized");
                return;
            }

            if (client == null || session == null)
            {
                Debug.LogError("[IVXAnalyticsManager] Invalid client or session");
                return;
            }

            _nakamaClient = client;
            _nakamaSession = session;
            _isInitialized = true;

            Debug.Log("[IVXAnalyticsManager] Initialized successfully");

            // Auto-start session
            _ = TrackSessionStart();
        }

        /// <summary>
        /// Track custom event with properties
        /// </summary>
        public async Task<bool> TrackEvent(string eventName, Dictionary<string, object> properties = null)
        {
            if (!_isInitialized)
            {
                Debug.LogWarning("[IVXAnalyticsManager] Not initialized");
                return false;
            }

            if (_nakamaSession == null || _nakamaSession.IsExpired)
            {
                Debug.LogError("[IVXAnalyticsManager] Session expired");
                return false;
            }

            try
            {
                // Prepare payload
                var payload = new Dictionary<string, object>
                {
                    { "gameID", GAME_ID },
                    { "eventName", eventName },
                    { "properties", properties ?? new Dictionary<string, object>() },
                    { "timestamp", DateTimeOffset.UtcNow.ToUnixTimeSeconds() }
                };

                string jsonPayload = JsonUtility.ToJson(new EventPayload
                {
                    gameID = GAME_ID,
                    eventName = eventName,
                    properties = properties ?? new Dictionary<string, object>()
                });

                Debug.Log($"[IVXAnalyticsManager] Tracking event: {eventName}");

                // Call Nakama RPC
                await _nakamaClient.RpcAsync(_nakamaSession, "quizverse_log_event", jsonPayload);

                OnEventTracked?.Invoke(eventName);
                return true;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[IVXAnalyticsManager] Failed to track event: {ex.Message}");
                OnError?.Invoke(ex.Message);
                return false;
            }
        }

        /// <summary>
        /// Track session start
        /// </summary>
        public async Task<bool> TrackSessionStart()
        {
            if (!_isInitialized)
            {
                Debug.LogWarning("[IVXAnalyticsManager] Not initialized");
                return false;
            }

            try
            {
                _sessionKey = $"session_{_nakamaSession.UserId}_{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}";
                _sessionStartTime = Time.realtimeSinceStartup;

                var deviceInfo = new Dictionary<string, object>
                {
                    { "platform", Application.platform.ToString() },
                    { "version", Application.version },
                    { "deviceModel", SystemInfo.deviceModel },
                    { "operatingSystem", SystemInfo.operatingSystem }
                };

                var payload = new SessionStartPayload
                {
                    gameID = GAME_ID,
                    sessionKey = _sessionKey,
                    deviceInfo = deviceInfo
                };

                string jsonPayload = JsonUtility.ToJson(payload);

                Debug.Log($"[IVXAnalyticsManager] Starting session: {_sessionKey}");

                await _nakamaClient.RpcAsync(_nakamaSession, "quizverse_track_session_start", jsonPayload);

                OnSessionStarted?.Invoke(_sessionKey);
                return true;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[IVXAnalyticsManager] Failed to start session: {ex.Message}");
                OnError?.Invoke(ex.Message);
                return false;
            }
        }

        /// <summary>
        /// Track session end
        /// </summary>
        public async Task<bool> TrackSessionEnd()
        {
            if (!_isInitialized || string.IsNullOrEmpty(_sessionKey))
            {
                Debug.LogWarning("[IVXAnalyticsManager] No active session");
                return false;
            }

            try
            {
                float duration = Time.realtimeSinceStartup - _sessionStartTime;

                var payload = new SessionEndPayload
                {
                    gameID = GAME_ID,
                    sessionKey = _sessionKey,
                    duration = (int)duration
                };

                string jsonPayload = JsonUtility.ToJson(payload);

                Debug.Log($"[IVXAnalyticsManager] Ending session: {_sessionKey} (duration: {duration}s)");

                await _nakamaClient.RpcAsync(_nakamaSession, "quizverse_track_session_end", jsonPayload);

                OnSessionEnded?.Invoke(_sessionKey, duration);
                
                _sessionKey = null;
                return true;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[IVXAnalyticsManager] Failed to end session: {ex.Message}");
                OnError?.Invoke(ex.Message);
                return false;
            }
        }

        /// <summary>
        /// Track screen view
        /// </summary>
        public async Task TrackScreen(string screenName, Dictionary<string, object> properties = null)
        {
            var eventProps = properties ?? new Dictionary<string, object>();
            eventProps["screen_name"] = screenName;
            await TrackEvent("screen_view", eventProps);
        }

        /// <summary>
        /// Track purchase
        /// </summary>
        public async Task TrackPurchase(string productId, decimal price, string currency, Dictionary<string, object> properties = null)
        {
            var eventProps = properties ?? new Dictionary<string, object>();
            eventProps["product_id"] = productId;
            eventProps["price"] = price.ToString();
            eventProps["currency"] = currency;
            
            await TrackEvent("purchase", eventProps);
        }

        /// <summary>
        /// Set user properties
        /// </summary>
        public void SetUserProperty(string key, object value)
        {
            _userProperties[key] = value;
            Debug.Log($"[IVXAnalyticsManager] Set user property: {key} = {value}");
        }

        /// <summary>
        /// Set multiple user properties
        /// </summary>
        public void SetUserProperties(Dictionary<string, object> properties)
        {
            foreach (var kvp in properties)
            {
                _userProperties[kvp.Key] = kvp.Value;
            }
            Debug.Log($"[IVXAnalyticsManager] Set {properties.Count} user properties");
        }

        /// <summary>
        /// Get user properties
        /// </summary>
        public Dictionary<string, object> GetUserProperties()
        {
            return new Dictionary<string, object>(_userProperties);
        }

        /// <summary>
        /// Check if initialized
        /// </summary>
        public bool IsInitialized()
        {
            return _isInitialized;
        }

        /// <summary>
        /// Reset analytics (for logout)
        /// </summary>
        public void Reset()
        {
            if (!string.IsNullOrEmpty(_sessionKey))
            {
                _ = TrackSessionEnd();
            }

            _userProperties.Clear();
            _sessionKey = null;
            _isInitialized = false;
            
            Debug.Log("[IVXAnalyticsManager] Reset complete");
        }
    }

    // Payload classes for JSON serialization
    [Serializable]
    internal class EventPayload
    {
        public string gameID;
        public string eventName;
        public Dictionary<string, object> properties;
    }

    [Serializable]
    internal class SessionStartPayload
    {
        public string gameID;
        public string sessionKey;
        public Dictionary<string, object> deviceInfo;
    }

    [Serializable]
    internal class SessionEndPayload
    {
        public string gameID;
        public string sessionKey;
        public int duration;
    }
}
