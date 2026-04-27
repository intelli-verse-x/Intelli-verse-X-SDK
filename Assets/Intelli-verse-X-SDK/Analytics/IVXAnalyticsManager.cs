using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using Nakama;

namespace IntelliVerseX.Analytics
{
    /// <summary>
    /// Analytics service for IntelliVerse-X SDK
    /// Uses Nakama RPC for event tracking and session analytics.
    /// Defaults preserve the original QuizVerse RPCs; new games can call
    /// ConfigureRpcIds or SetGameRpcPrefix before Initialize.
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

        private static string _gameId = string.Empty;
        private static string _logEventRpcId = "quizverse_log_event";
        private static string _sessionStartRpcId = "quizverse_track_session_start";
        private static string _sessionEndRpcId = "quizverse_track_session_end";

        /// <summary>
        /// Sets the Game ID for analytics. Must be called before Initialize().
        /// Each game should set its own unique ID.
        /// </summary>
        public static void SetGameId(string gameId)
        {
            if (string.IsNullOrWhiteSpace(gameId))
            {
                Debug.LogError("[IVXAnalyticsManager] Game ID cannot be null or empty");
                return;
            }
            _gameId = gameId;
        }

        /// <summary>
        /// Configures game-specific analytics RPC IDs. Call before Initialize.
        /// </summary>
        public static void ConfigureRpcIds(string logEventRpcId, string sessionStartRpcId, string sessionEndRpcId)
        {
            if (string.IsNullOrWhiteSpace(logEventRpcId) ||
                string.IsNullOrWhiteSpace(sessionStartRpcId) ||
                string.IsNullOrWhiteSpace(sessionEndRpcId))
            {
                Debug.LogError("[IVXAnalyticsManager] Analytics RPC IDs cannot be null or empty");
                return;
            }

            _logEventRpcId = logEventRpcId;
            _sessionStartRpcId = sessionStartRpcId;
            _sessionEndRpcId = sessionEndRpcId;
        }

        /// <summary>
        /// Configures RPC IDs using the standard game prefix pattern:
        /// {prefix}_log_event, {prefix}_track_session_start, {prefix}_track_session_end.
        /// </summary>
        public static void SetGameRpcPrefix(string rpcPrefix)
        {
            if (string.IsNullOrWhiteSpace(rpcPrefix))
            {
                Debug.LogError("[IVXAnalyticsManager] RPC prefix cannot be null or empty");
                return;
            }

            ConfigureRpcIds(
                $"{rpcPrefix}_log_event",
                $"{rpcPrefix}_track_session_start",
                $"{rpcPrefix}_track_session_end");
        }

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

            if (string.IsNullOrEmpty(_gameId))
            {
                Debug.LogWarning("[IVXAnalyticsManager] Game ID not set. Call SetGameId() before Initialize() for proper analytics tracking.");
            }

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

            if (string.IsNullOrWhiteSpace(eventName) ||
                string.Equals(eventName, "unknown", StringComparison.OrdinalIgnoreCase))
            {
                Debug.LogWarning("[IVXAnalyticsManager] Rejected analytics event with missing or unknown event name");
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
                string jsonPayload = SerializeEventPayload(_gameId, eventName, properties);

                Debug.Log($"[IVXAnalyticsManager] Tracking event: {eventName}");

                // Call Nakama RPC
                await _nakamaClient.RpcAsync(_nakamaSession, _logEventRpcId, jsonPayload);

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

                string jsonPayload = SerializeSessionStartPayload(_gameId, _sessionKey);

                Debug.Log($"[IVXAnalyticsManager] Starting session: {_sessionKey}");

                await _nakamaClient.RpcAsync(_nakamaSession, _sessionStartRpcId, jsonPayload);

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

                string jsonPayload = $"{{\"gameID\":\"{_gameId}\",\"sessionKey\":\"{_sessionKey}\",\"duration\":{(int)duration}}}";

                Debug.Log($"[IVXAnalyticsManager] Ending session: {_sessionKey} (duration: {duration}s)");

                await _nakamaClient.RpcAsync(_nakamaSession, _sessionEndRpcId, jsonPayload);

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

        #region JSON Serialization Helpers

        private static string SerializeEventPayload(string gameId, string eventName, Dictionary<string, object> properties)
        {
            var sb = new System.Text.StringBuilder(256);
            sb.Append("{\"gameID\":\"").Append(EscapeJson(gameId))
              .Append("\",\"eventName\":\"").Append(EscapeJson(eventName))
              .Append("\",\"timestamp\":").Append(DateTimeOffset.UtcNow.ToUnixTimeSeconds())
              .Append(",\"properties\":");
            SerializeDictionary(sb, properties);
            sb.Append('}');
            return sb.ToString();
        }

        private static string SerializeSessionStartPayload(string gameId, string sessionKey)
        {
            var sb = new System.Text.StringBuilder(256);
            sb.Append("{\"gameID\":\"").Append(EscapeJson(gameId))
              .Append("\",\"sessionKey\":\"").Append(EscapeJson(sessionKey))
              .Append("\",\"deviceInfo\":{")
              .Append("\"platform\":\"").Append(EscapeJson(Application.platform.ToString()))
              .Append("\",\"version\":\"").Append(EscapeJson(Application.version))
              .Append("\",\"deviceModel\":\"").Append(EscapeJson(SystemInfo.deviceModel))
              .Append("\",\"operatingSystem\":\"").Append(EscapeJson(SystemInfo.operatingSystem))
              .Append("\"}}");
            return sb.ToString();
        }

        private static void SerializeDictionary(System.Text.StringBuilder sb, Dictionary<string, object> dict)
        {
            if (dict == null || dict.Count == 0)
            {
                sb.Append("{}");
                return;
            }

            sb.Append('{');
            bool first = true;
            foreach (var kvp in dict)
            {
                if (!first) sb.Append(',');
                first = false;
                sb.Append('"').Append(EscapeJson(kvp.Key)).Append("\":");
                if (kvp.Value == null)
                    sb.Append("null");
                else if (kvp.Value is string s)
                    sb.Append('"').Append(EscapeJson(s)).Append('"');
                else if (kvp.Value is bool b)
                    sb.Append(b ? "true" : "false");
                else if (kvp.Value is int || kvp.Value is long || kvp.Value is float || kvp.Value is double || kvp.Value is decimal)
                    sb.Append(Convert.ToString(kvp.Value, System.Globalization.CultureInfo.InvariantCulture));
                else
                    sb.Append('"').Append(EscapeJson(kvp.Value.ToString())).Append('"');
            }
            sb.Append('}');
        }

        private static string EscapeJson(string s)
        {
            if (string.IsNullOrEmpty(s)) return string.Empty;
            return s.Replace("\\", "\\\\").Replace("\"", "\\\"").Replace("\n", "\\n").Replace("\r", "\\r").Replace("\t", "\\t");
        }

        #endregion
    }
}
