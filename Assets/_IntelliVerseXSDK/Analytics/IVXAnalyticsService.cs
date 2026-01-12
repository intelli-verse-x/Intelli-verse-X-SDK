// File: IVXAnalyticsService.cs
// Purpose: Analytics tracking service with Unity Analytics integration
// Package: IntelliVerseX.Analytics
// Dependencies: IntelliVerseX.Core

using System;
using System.Collections.Generic;
using UnityEngine;
using IntelliVerseX.Core;

#if UNITY_ANALYTICS
using Unity.Services.Analytics;
using Unity.Services.Core;
#endif

namespace IntelliVerseX.Analytics
{
    /// <summary>
    /// Analytics event data
    /// </summary>
    [Serializable]
    public class IVXAnalyticsEvent
    {
        public string EventName { get; set; }
        public Dictionary<string, object> Parameters { get; set; }

        public IVXAnalyticsEvent(string eventName)
        {
            EventName = eventName;
            Parameters = new Dictionary<string, object>();
        }

        public IVXAnalyticsEvent AddParameter(string key, object value)
        {
            Parameters[key] = value;
            return this;
        }
    }

    /// <summary>
    /// Core analytics service with Unity Analytics integration.
    /// Tracks custom events, user properties, and game metrics.
    /// 
    /// Usage:
    ///   await IVXAnalyticsService.Instance.InitializeAsync();
    ///   IVXAnalyticsService.Instance.TrackEvent("quiz_completed", new { score = 85, time = 120 });
    ///   IVXAnalyticsService.Instance.SetUserProperty("level", 10);
    /// </summary>
    public class IVXAnalyticsService : IVXSafeSingleton<IVXAnalyticsService>
    {
        #region Constants

        // Common event names
        public const string EVENT_GAME_START = "game_start";
        public const string EVENT_GAME_END = "game_end";
        public const string EVENT_LEVEL_START = "level_start";
        public const string EVENT_LEVEL_COMPLETE = "level_complete";
        public const string EVENT_QUIZ_START = "quiz_start";
        public const string EVENT_QUIZ_COMPLETE = "quiz_complete";
        public const string EVENT_PURCHASE = "purchase";
        public const string EVENT_AD_IMPRESSION = "ad_impression";

        #endregion

        #region Private Fields

        private bool _isInitialized;
        private Dictionary<string, object> _userProperties = new Dictionary<string, object>();

        #endregion

        #region Public Properties

        public bool IsInitialized => _isInitialized;

        #endregion

        #region Initialization

        /// <summary>
        /// Initialize analytics service
        /// </summary>
        public async System.Threading.Tasks.Task<bool> InitializeAsync()
        {
            if (_isInitialized)
            {
                Debug.LogWarning("[IVXAnalyticsService] Already initialized");
                return true;
            }

#if UNITY_ANALYTICS
            try
            {
                Debug.Log("[IVXAnalyticsService] Initializing Unity Analytics");

                // Initialize Unity Services
                await UnityServices.InitializeAsync();

                // Start data collection
                await AnalyticsService.Instance.CheckForRequiredConsents();

                _isInitialized = true;
                Debug.Log("[IVXAnalyticsService] Initialization complete");
                return true;
            }
            catch (Exception e)
            {
                Debug.LogError($"[IVXAnalyticsService] Initialization failed: {e.Message}");
                _isInitialized = false;
                return false;
            }
#else
            Debug.LogWarning("[IVXAnalyticsService] Unity Analytics not enabled");
            _isInitialized = false;
            await System.Threading.Tasks.Task.CompletedTask;
            return false;
#endif
        }

        #endregion

        #region Event Tracking

        /// <summary>
        /// Track a custom event with parameters
        /// </summary>
        public void TrackEvent(string eventName, Dictionary<string, object> parameters = null)
        {
            if (!_isInitialized)
            {
                Debug.LogWarning($"[IVXAnalyticsService] Not initialized, skipping event: {eventName}");
                return;
            }

#if UNITY_ANALYTICS
            try
            {
                if (parameters == null || parameters.Count == 0)
                {
                    AnalyticsService.Instance.CustomData(eventName);
                }
                else
                {
                    AnalyticsService.Instance.CustomData(eventName, parameters);
                }

                Debug.Log($"[IVXAnalyticsService] Event tracked: {eventName}");
            }
            catch (Exception e)
            {
                Debug.LogError($"[IVXAnalyticsService] Failed to track event {eventName}: {e.Message}");
            }
#else
            Debug.Log($"[IVXAnalyticsService] Event: {eventName} (Analytics disabled)");
#endif
        }

        /// <summary>
        /// Track event with anonymous object parameters
        /// </summary>
        public void TrackEvent(string eventName, object parameters)
        {
            if (parameters == null)
            {
                TrackEvent(eventName, null);
                return;
            }

            // Convert anonymous object to dictionary
            var dict = new Dictionary<string, object>();
            var properties = parameters.GetType().GetProperties();

            foreach (var prop in properties)
            {
                dict[prop.Name] = prop.GetValue(parameters);
            }

            TrackEvent(eventName, dict);
        }

        /// <summary>
        /// Track event using IVXAnalyticsEvent
        /// </summary>
        public void TrackEvent(IVXAnalyticsEvent analyticsEvent)
        {
            if (analyticsEvent == null)
            {
                Debug.LogError("[IVXAnalyticsService] Event is null");
                return;
            }

            TrackEvent(analyticsEvent.EventName, analyticsEvent.Parameters);
        }

        #endregion

        #region User Properties

        /// <summary>
        /// Set a user property
        /// </summary>
        public void SetUserProperty(string key, object value)
        {
            if (string.IsNullOrEmpty(key))
            {
                Debug.LogError("[IVXAnalyticsService] User property key is null or empty");
                return;
            }

            _userProperties[key] = value;

#if UNITY_ANALYTICS
            // Unity Analytics doesn't have direct user property setting
            // Instead, we include them in events
            Debug.Log($"[IVXAnalyticsService] User property set: {key} = {value}");
#else
            Debug.Log($"[IVXAnalyticsService] User property: {key} = {value} (Analytics disabled)");
#endif
        }

        /// <summary>
        /// Get a user property
        /// </summary>
        public object GetUserProperty(string key)
        {
            return _userProperties.TryGetValue(key, out object value) ? value : null;
        }

        /// <summary>
        /// Get all user properties
        /// </summary>
        public Dictionary<string, object> GetAllUserProperties()
        {
            return new Dictionary<string, object>(_userProperties);
        }

        #endregion

        #region Convenience Methods

        /// <summary>
        /// Track quiz start
        /// </summary>
        public void TrackQuizStart(string quizId, string difficulty = null)
        {
            TrackEvent(EVENT_QUIZ_START, new
            {
                quiz_id = quizId,
                difficulty = difficulty ?? "normal"
            });
        }

        /// <summary>
        /// Track quiz complete
        /// </summary>
        public void TrackQuizComplete(string quizId, int score, int totalQuestions, float timeSeconds)
        {
            TrackEvent(EVENT_QUIZ_COMPLETE, new
            {
                quiz_id = quizId,
                score = score,
                total_questions = totalQuestions,
                percentage = (score * 100) / totalQuestions,
                time_seconds = timeSeconds
            });
        }

        /// <summary>
        /// Track purchase
        /// </summary>
        public void TrackPurchase(string productId, decimal price, string currency)
        {
            TrackEvent(EVENT_PURCHASE, new
            {
                product_id = productId,
                price = price,
                currency = currency
            });
        }

        /// <summary>
        /// Track ad impression
        /// </summary>
        public void TrackAdImpression(string placementId, string adType)
        {
            TrackEvent(EVENT_AD_IMPRESSION, new
            {
                placement_id = placementId,
                ad_type = adType
            });
        }

        #endregion
    }

    /// <summary>
    /// Event tracker component for tracking UI interactions.
    /// Attach to buttons or other UI elements.
    /// </summary>
    public class IVXEventTracker : MonoBehaviour
    {
        #region Inspector Fields

        [Header("Event Configuration")]
        [Tooltip("Event name to track")]
        [SerializeField] private string eventName = "button_click";

        [Tooltip("Track event on Start()")]
        [SerializeField] private bool trackOnStart = false;

        [Tooltip("Track event on Enable()")]
        [SerializeField] private bool trackOnEnable = false;

        [Tooltip("Track event on Click (requires Button component)")]
        [SerializeField] private bool trackOnClick = true;

        [Header("Event Parameters (Optional)")]
        [SerializeField] private List<EventParameter> parameters = new List<EventParameter>();

        #endregion

        #region Private Fields

        private UnityEngine.UI.Button _button;

        #endregion

        #region Unity Lifecycle

        private void Start()
        {
            if (trackOnClick)
            {
                _button = GetComponent<UnityEngine.UI.Button>();
                if (_button != null)
                {
                    _button.onClick.AddListener(TrackEvent);
                }
            }

            if (trackOnStart)
            {
                TrackEvent();
            }
        }

        private void OnEnable()
        {
            if (trackOnEnable)
            {
                TrackEvent();
            }
        }

        private void OnDestroy()
        {
            if (_button != null)
            {
                _button.onClick.RemoveListener(TrackEvent);
            }
        }

        #endregion

        #region Public API

        /// <summary>
        /// Track the configured event
        /// </summary>
        public void TrackEvent()
        {
            if (IVXAnalyticsService.Instance == null)
            {
                Debug.LogWarning($"[IVXEventTracker] Analytics service not available, skipping event: {eventName}");
                return;
            }

            // Build parameters dictionary
            var paramDict = new Dictionary<string, object>();
            foreach (var param in parameters)
            {
                paramDict[param.key] = ConvertParameterValue(param);
            }

            IVXAnalyticsService.Instance.TrackEvent(eventName, paramDict);
        }

        /// <summary>
        /// Track event with custom parameters
        /// </summary>
        public void TrackEvent(Dictionary<string, object> customParameters)
        {
            if (IVXAnalyticsService.Instance == null)
            {
                Debug.LogWarning($"[IVXEventTracker] Analytics service not available");
                return;
            }

            IVXAnalyticsService.Instance.TrackEvent(eventName, customParameters);
        }

        #endregion

        #region Private Methods

        private object ConvertParameterValue(EventParameter param)
        {
            switch (param.type)
            {
                case ParameterType.String: return param.stringValue;
                case ParameterType.Int: return param.intValue;
                case ParameterType.Float: return param.floatValue;
                case ParameterType.Bool: return param.boolValue;
                default: return param.stringValue;
            }
        }

        #endregion

        #region Helper Classes

        public enum ParameterType
        {
            String,
            Int,
            Float,
            Bool
        }

        [Serializable]
        public class EventParameter
        {
            public string key;
            public ParameterType type;
            public string stringValue;
            public int intValue;
            public float floatValue;
            public bool boolValue;
        }

        #endregion
    }
}
