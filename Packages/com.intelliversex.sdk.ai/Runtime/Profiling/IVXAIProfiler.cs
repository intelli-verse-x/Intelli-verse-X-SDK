using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Networking;

namespace IntelliVerseX.AI
{
    #region Enums

    /// <summary>
    /// High-level player segmentation used for profiling and personalization.
    /// </summary>
    public enum IVXPlayerCohort
    {
        /// <summary>Casual engagement pattern.</summary>
        Casual,
        /// <summary>Socially driven players.</summary>
        Social,
        /// <summary>Competitive-focused players.</summary>
        Competitive,
        /// <summary>Exploration-oriented players.</summary>
        Explorer,
        /// <summary>Achievement collectors.</summary>
        Achiever,
        /// <summary>High-value spenders.</summary>
        Whale,
        /// <summary>Players showing disengagement signals.</summary>
        AtRisk,
        /// <summary>Recently onboarded players.</summary>
        NewPlayer,
        /// <summary>Long-term retained players.</summary>
        Veteran,
        /// <summary>Previously active, now inactive.</summary>
        Lapsed
    }

    #endregion

    #region Client data models

    /// <summary>
    /// Cached player profiling snapshot for gameplay and UI personalization.
    /// </summary>
    [Serializable]
    public class IVXPlayerProfile
    {
        /// <summary>Stable player identifier.</summary>
        public string PlayerId;
        /// <summary>Assigned cohort.</summary>
        public IVXPlayerCohort Cohort;
        /// <summary>Engagement score from 0–100.</summary>
        public float EngagementScore;
        /// <summary>Churn risk from 0–1.</summary>
        public float ChurnRiskScore;
        /// <summary>Likelihood to monetize from 0–1.</summary>
        public float MonetizationPropensity;
        /// <summary>Total sessions observed.</summary>
        public int TotalSessionCount;
        /// <summary>Average session length in minutes.</summary>
        public float AvgSessionDurationMinutes;
        /// <summary>Favourite game modes.</summary>
        public string[] PreferredGameModes;
        /// <summary>Favourite product features.</summary>
        public string[] PreferredFeatures;
        /// <summary>Unix milliseconds of last activity.</summary>
        public long LastActiveTimestamp;
        /// <summary>Custom scalar metrics keyed by name.</summary>
        public Dictionary<string, float> CustomMetrics;
    }

    /// <summary>
    /// A single personalization hint returned by the profiling service.
    /// </summary>
    [Serializable]
    public class IVXPersonalizationHint
    {
        /// <summary>Hint category, e.g. recommend_mode, offer_discount.</summary>
        [JsonProperty("hint_type")]
        public string HintType;
        /// <summary>Feature or surface to act on.</summary>
        [JsonProperty("target_feature")]
        public string TargetFeature;
        /// <summary>Human-readable message for UI.</summary>
        [JsonProperty("message")]
        public string Message;
        /// <summary>Relative priority from 0–1.</summary>
        [JsonProperty("priority")]
        public float Priority;
        /// <summary>Additional string parameters.</summary>
        [JsonProperty("parameters")]
        public Dictionary<string, string> Parameters;
    }

    #endregion

    #region API models

    /// <summary>
    /// Single analytics event payload sent to the profiling track API.
    /// </summary>
    [Serializable]
    public class IVXTrackEventRequest
    {
        /// <summary>Player identifier.</summary>
        [JsonProperty("player_id")]
        public string PlayerId { get; set; }

        /// <summary>Event name.</summary>
        [JsonProperty("event_name")]
        public string EventName { get; set; }

        /// <summary>Optional structured properties.</summary>
        [JsonProperty("data")]
        public Dictionary<string, object> Data { get; set; }

        /// <summary>Unix milliseconds (UTC).</summary>
        [JsonProperty("timestamp")]
        public long Timestamp { get; set; }
    }

    /// <summary>
    /// Profiling API response for a player profile.
    /// </summary>
    public class IVXProfileResponse
    {
        /// <summary>Player identifier.</summary>
        [JsonProperty("player_id")]
        public string PlayerId { get; set; }

        /// <summary>Cohort key from the backend.</summary>
        [JsonProperty("cohort")]
        public string Cohort { get; set; }

        /// <summary>Engagement score 0–100.</summary>
        [JsonProperty("engagement_score")]
        public float EngagementScore { get; set; }

        /// <summary>Churn risk 0–1.</summary>
        [JsonProperty("churn_risk")]
        public float ChurnRisk { get; set; }

        /// <summary>Monetization propensity 0–1.</summary>
        [JsonProperty("monetization_propensity")]
        public float MonetizationPropensity { get; set; }

        /// <summary>Total sessions.</summary>
        [JsonProperty("total_sessions")]
        public int TotalSessions { get; set; }

        /// <summary>Average session duration (minutes).</summary>
        [JsonProperty("avg_session_duration")]
        public float AvgSessionDuration { get; set; }

        /// <summary>Preferred game modes.</summary>
        [JsonProperty("preferred_modes")]
        public string[] PreferredModes { get; set; }

        /// <summary>Preferred features.</summary>
        [JsonProperty("preferred_features")]
        public string[] PreferredFeatures { get; set; }

        /// <summary>Last active timestamp (Unix ms).</summary>
        [JsonProperty("last_active")]
        public long LastActive { get; set; }

        /// <summary>Custom metrics map.</summary>
        [JsonProperty("custom_metrics")]
        public Dictionary<string, float> CustomMetrics { get; set; }
    }

    /// <summary>
    /// API wrapper for personalization hints.
    /// </summary>
    public class IVXPersonalizationResponse
    {
        /// <summary>List of hints.</summary>
        [JsonProperty("hints")]
        public List<IVXPersonalizationHint> Hints { get; set; }
    }

    /// <summary>
    /// Churn prediction API response.
    /// </summary>
    public class IVXChurnPredictionResponse
    {
        /// <summary>Risk score 0–1.</summary>
        [JsonProperty("risk_score")]
        public float RiskScore { get; set; }

        /// <summary>Human-readable contributing factors.</summary>
        [JsonProperty("risk_factors")]
        public string[] RiskFactors { get; set; }

        /// <summary>Recommended remediation actions.</summary>
        [JsonProperty("recommended_actions")]
        public string[] RecommendedActions { get; set; }
    }

    /// <summary>
    /// Batch body for the track endpoint.
    /// </summary>
    internal class IVXTrackEventBatchBody
    {
        /// <summary>Queued events.</summary>
        [JsonProperty("events")]
        public List<IVXTrackEventRequest> Events { get; set; }
    }

    #endregion

    /// <summary>
    /// Queues and sends player profiling events, fetches profiles, personalization, and churn signals.
    /// </summary>
    public sealed class IVXAIProfiler : MonoBehaviour
    {
        #region Singleton

        private static IVXAIProfiler _instance;

        /// <summary>Singleton instance (lazy-resolved if not yet created).</summary>
        public static IVXAIProfiler Instance
        {
            get
            {
                if (_instance == null)
                    _instance = FindFirstObjectByType<IVXAIProfiler>();
                return _instance;
            }
        }

        #endregion

        #region Events

        /// <summary>Fired when the cached profile is refreshed.</summary>
        public event Action<IVXPlayerProfile> OnProfileUpdated;

        /// <summary>Fired when personalization hints are received.</summary>
        public event Action<List<IVXPersonalizationHint>> OnPersonalizationReady;

        /// <summary>Fired when churn assessment completes (score and factors).</summary>
        public event Action<float, string[]> OnChurnRiskAssessed;

        #endregion

        #region Properties

        /// <summary>Latest profile returned by the backend, or null.</summary>
        public IVXPlayerProfile CachedProfile => _cachedProfile;

        /// <summary>True while automatic session/event tracking is enabled.</summary>
        public bool IsTracking { get; private set; }

        /// <summary>True after <see cref="Initialize(IVXAIConfig, string)"/> completes successfully.</summary>
        public bool IsInitialized => _isInitialized;

        #endregion

        #region Private Fields

        private readonly List<IVXTrackEventRequest> _eventQueue = new List<IVXTrackEventRequest>();
        private float _flushInterval = 5f;
        private float _lastFlushTime;
        private IVXPlayerProfile _cachedProfile;
        private IVXAIConfig _config;
        private MonoBehaviour _coroutineHost;
        private string _playerId;
        private string _authToken;
        private int _maxQueueSize = 2000;
        private bool _flushInProgress;
        private bool _isInitialized;

        #endregion

        #region Unity Lifecycle

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this;
            _coroutineHost = this;
            DontDestroyOnLoad(gameObject);
        }

        private void Update()
        {
            if (_eventQueue.Count == 0 || !_isInitialized)
                return;

            if (Time.realtimeSinceStartup - _lastFlushTime >= _flushInterval)
                FlushEvents();
        }

        private void OnApplicationPause(bool pauseStatus)
        {
            if (pauseStatus)
                FlushEvents();
        }

        private void OnDestroy()
        {
            if (_instance == this)
                _instance = null;

            FlushEvents();
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Binds configuration and player identity. Required before tracking or API calls.
        /// </summary>
        /// <param name="config">AI configuration asset.</param>
        /// <param name="playerId">Current player id.</param>
        public void Initialize(IVXAIConfig config, string playerId)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
            if (!_config.Validate(out string err))
            {
                Debug.LogError($"[{nameof(IVXAIProfiler)}] Config validation failed: {err}");
                return;
            }

            _playerId = string.IsNullOrEmpty(playerId) ? throw new ArgumentException("Player id required.", nameof(playerId)) : playerId;
            _coroutineHost = this;
            _lastFlushTime = Time.realtimeSinceStartup;
            _maxQueueSize = _config.MaxEventQueueSize;
            _isInitialized = true;

            if (_config.DebugLogging)
                Debug.Log($"[{nameof(IVXAIProfiler)}] Initialized for player {_playerId}");
        }

        /// <summary>Sets the bearer token applied to profiling HTTP requests.</summary>
        public void SetAuthToken(string token)
        {
            _authToken = token;
        }

        /// <summary>
        /// Enqueues a profiling event for batched delivery.
        /// </summary>
        /// <param name="eventName">Logical event name.</param>
        /// <param name="data">Optional properties.</param>
        public void TrackEvent(string eventName, Dictionary<string, object> data = null)
        {
            if (!_isInitialized)
            {
                Debug.LogWarning($"[{nameof(IVXAIProfiler)}] TrackEvent called before Initialize.");
                return;
            }

            if (_eventQueue.Count >= _maxQueueSize)
            {
                if (_config.DebugLogging)
                    Debug.LogWarning($"[{nameof(IVXAIProfiler)}] Event queue full ({_maxQueueSize}). Dropping oldest.");
                _eventQueue.RemoveAt(0);
            }

            _eventQueue.Add(new IVXTrackEventRequest
            {
                PlayerId = _playerId,
                EventName = eventName,
                Data = data ?? new Dictionary<string, object>(),
                Timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
            });
        }

        /// <summary>
        /// Sends all queued events immediately.
        /// </summary>
        public void FlushEvents()
        {
            if (!_isInitialized || _flushInProgress || _eventQueue.Count == 0)
                return;

            var batch = new List<IVXTrackEventRequest>(_eventQueue);
            _eventQueue.Clear();
            _lastFlushTime = Time.realtimeSinceStartup;
            _coroutineHost.StartCoroutine(PostTrackBatchCoroutine(batch));
        }

        /// <summary>
        /// Fetches the latest player profile from the backend and updates <see cref="CachedProfile"/>.
        /// </summary>
        /// <param name="onComplete">Optional callback with the mapped profile.</param>
        public void GetPlayerProfile(Action<IVXPlayerProfile> onComplete = null)
        {
            if (!_isInitialized)
            {
                Debug.LogWarning($"[{nameof(IVXAIProfiler)}] GetPlayerProfile called before Initialize.");
                onComplete?.Invoke(null);
                return;
            }

            string url = $"{TrimApiBase(_config.ApiBaseUrl)}/profiling/profile/{UnityWebRequest.EscapeURL(_playerId)}";
            _coroutineHost.StartCoroutine(GetJsonCoroutine<IVXProfileResponse>(url, response =>
            {
                _cachedProfile = MapProfile(response);
                OnProfileUpdated?.Invoke(_cachedProfile);
                onComplete?.Invoke(_cachedProfile);
            }, err =>
            {
                Debug.LogWarning($"[{nameof(IVXAIProfiler)}] GetPlayerProfile failed: {err}");
                onComplete?.Invoke(_cachedProfile);
            }));
        }

        /// <summary>
        /// Requests personalization hints for the current player.
        /// </summary>
        /// <param name="onComplete">Callback with hints (may be empty).</param>
        public void GetPersonalizationHints(Action<List<IVXPersonalizationHint>> onComplete = null)
        {
            if (!_isInitialized)
            {
                Debug.LogWarning($"[{nameof(IVXAIProfiler)}] GetPersonalizationHints called before Initialize.");
                onComplete?.Invoke(new List<IVXPersonalizationHint>());
                return;
            }

            string url = $"{TrimApiBase(_config.ApiBaseUrl)}/profiling/personalize/{UnityWebRequest.EscapeURL(_playerId)}";
            _coroutineHost.StartCoroutine(GetJsonCoroutine<IVXPersonalizationResponse>(url, response =>
            {
                var list = response?.Hints ?? new List<IVXPersonalizationHint>();
                OnPersonalizationReady?.Invoke(list);
                onComplete?.Invoke(list);
            }, err =>
            {
                Debug.LogWarning($"[{nameof(IVXAIProfiler)}] GetPersonalizationHints failed: {err}");
                onComplete?.Invoke(new List<IVXPersonalizationHint>());
            }));
        }

        /// <summary>
        /// Resolves the player's cohort (typically via profile).
        /// </summary>
        /// <param name="onComplete">Callback with cohort.</param>
        public void ClassifyPlayer(Action<IVXPlayerCohort> onComplete = null)
        {
            GetPlayerProfile(profile =>
            {
                var cohort = profile?.Cohort ?? IVXPlayerCohort.Casual;
                onComplete?.Invoke(cohort);
            });
        }

        /// <summary>
        /// Requests churn risk and explanatory factors.
        /// </summary>
        /// <param name="onComplete">Callback with score and factors.</param>
        public void PredictChurn(Action<float, string[]> onComplete = null)
        {
            if (!_isInitialized)
            {
                Debug.LogWarning($"[{nameof(IVXAIProfiler)}] PredictChurn called before Initialize.");
                onComplete?.Invoke(0f, Array.Empty<string>());
                return;
            }

            string url = $"{TrimApiBase(_config.ApiBaseUrl)}/profiling/churn/{UnityWebRequest.EscapeURL(_playerId)}";
            _coroutineHost.StartCoroutine(GetJsonCoroutine<IVXChurnPredictionResponse>(url, response =>
            {
                var factors = response?.RiskFactors ?? Array.Empty<string>();
                float score = response?.RiskScore ?? 0f;
                OnChurnRiskAssessed?.Invoke(score, factors);
                onComplete?.Invoke(score, factors);
            }, err =>
            {
                Debug.LogWarning($"[{nameof(IVXAIProfiler)}] PredictChurn failed: {err}");
                onComplete?.Invoke(0f, Array.Empty<string>());
            }));
        }

        /// <summary>
        /// Starts lightweight automatic tracking (session markers and periodic flush).
        /// </summary>
        public void StartAutoTracking()
        {
            if (!_isInitialized)
            {
                Debug.LogWarning($"[{nameof(IVXAIProfiler)}] StartAutoTracking called before Initialize.");
                return;
            }

            IsTracking = true;
            TrackEvent("auto_tracking_start", null);
            _lastFlushTime = Time.realtimeSinceStartup;
        }

        /// <summary>
        /// Stops automatic tracking and flushes queued events.
        /// </summary>
        public void StopAutoTracking()
        {
            IsTracking = false;
            TrackEvent("auto_tracking_stop", null);
            FlushEvents();
        }

        #endregion

        #region Private Methods — HTTP

        private IEnumerator PostTrackBatchCoroutine(List<IVXTrackEventRequest> batch)
        {
            _flushInProgress = true;
            string url = $"{TrimApiBase(_config.ApiBaseUrl)}/profiling/track";
            var body = new IVXTrackEventBatchBody { Events = batch };
            string json = JsonConvert.SerializeObject(body, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });

            using (var request = new UnityWebRequest(url, "POST"))
            {
                request.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(json));
                request.downloadHandler = new DownloadHandlerBuffer();
                request.SetRequestHeader("Content-Type", "application/json");
                ApplyAuthHeaders(request);
                request.timeout = (int)_config.RequestTimeout;

                yield return request.SendWebRequest();

                if (request.result != UnityWebRequest.Result.Success)
                    Debug.LogWarning($"[{nameof(IVXAIProfiler)}] Track batch failed: {request.error}");
                else if (_config.DebugLogging)
                    Debug.Log($"[{nameof(IVXAIProfiler)}] Track batch OK ({batch.Count} events)");
            }

            _flushInProgress = false;
        }

        private IEnumerator GetJsonCoroutine<T>(string url, Action<T> onSuccess, Action<string> onError) where T : class
        {
            using (var request = UnityWebRequest.Get(url))
            {
                ApplyAuthHeaders(request);
                request.timeout = (int)_config.RequestTimeout;

                yield return request.SendWebRequest();

                if (request.result != UnityWebRequest.Result.Success)
                {
                    onError?.Invoke($"{request.method} {url}: {request.error}");
                    yield break;
                }

                try
                {
                    var result = JsonConvert.DeserializeObject<T>(request.downloadHandler.text);
                    onSuccess?.Invoke(result);
                }
                catch (Exception ex)
                {
                    onError?.Invoke($"Deserialize: {ex.Message}");
                }
            }
        }

        private void ApplyAuthHeaders(UnityWebRequest request)
        {
            if (!string.IsNullOrEmpty(_authToken))
                request.SetRequestHeader("Authorization", $"Bearer {_authToken}");
            if (!string.IsNullOrEmpty(_config.ApiKey))
                request.SetRequestHeader("X-API-Key", _config.ApiKey);
        }

        private static string TrimApiBase(string apiBaseUrl)
        {
            if (string.IsNullOrEmpty(apiBaseUrl))
                return string.Empty;
            return apiBaseUrl.TrimEnd('/');
        }

        private static IVXPlayerProfile MapProfile(IVXProfileResponse r)
        {
            if (r == null)
                return null;

            return new IVXPlayerProfile
            {
                PlayerId = r.PlayerId,
                Cohort = ParseCohort(r.Cohort),
                EngagementScore = r.EngagementScore,
                ChurnRiskScore = r.ChurnRisk,
                MonetizationPropensity = r.MonetizationPropensity,
                TotalSessionCount = r.TotalSessions,
                AvgSessionDurationMinutes = r.AvgSessionDuration,
                PreferredGameModes = r.PreferredModes ?? Array.Empty<string>(),
                PreferredFeatures = r.PreferredFeatures ?? Array.Empty<string>(),
                LastActiveTimestamp = r.LastActive,
                CustomMetrics = r.CustomMetrics ?? new Dictionary<string, float>()
            };
        }

        private static IVXPlayerCohort ParseCohort(string raw)
        {
            if (string.IsNullOrEmpty(raw))
                return IVXPlayerCohort.Casual;

            if (Enum.TryParse<IVXPlayerCohort>(raw, true, out var cohort))
                return cohort;

            var normalized = raw.Replace(" ", "").Replace("_", "").Replace("-", "");
            foreach (IVXPlayerCohort c in Enum.GetValues(typeof(IVXPlayerCohort)))
            {
                if (string.Equals(c.ToString(), normalized, StringComparison.OrdinalIgnoreCase))
                    return c;
            }

            return IVXPlayerCohort.Casual;
        }

        #endregion
    }
}
