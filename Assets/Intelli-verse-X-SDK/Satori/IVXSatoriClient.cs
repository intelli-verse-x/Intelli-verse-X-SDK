using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Nakama;
using UnityEngine;

namespace IntelliVerseX.Satori
{
    /// <summary>
    /// Client for all Satori LiveOps operations: event capture, identity properties,
    /// feature flags, audiences, experiments, live events, and messages.
    /// All operations route through Nakama RPC endpoints.
    /// </summary>
    public sealed class IVXSatoriClient : MonoBehaviour
    {
        private static IVXSatoriClient _instance;
        private IVXSatoriRpcClient _rpc;
        private bool _initialized;

        #region RPC IDs (must match server registration)

        private const string RPC_EVENT = "satori_event";
        private const string RPC_EVENTS_BATCH = "satori_events_batch";
        private const string RPC_IDENTITY_GET = "satori_identity_get";
        private const string RPC_IDENTITY_UPDATE = "satori_identity_update_properties";
        private const string RPC_AUDIENCES_GET = "satori_audiences_get_memberships";
        private const string RPC_FLAGS_GET = "satori_flags_get";
        private const string RPC_FLAGS_GET_ALL = "satori_flags_get_all";
        private const string RPC_EXPERIMENTS_GET = "satori_experiments_get";
        private const string RPC_EXPERIMENTS_VARIANT = "satori_experiments_get_variant";
        private const string RPC_LIVE_EVENTS_LIST = "satori_live_events_list";
        private const string RPC_LIVE_EVENTS_JOIN = "satori_live_events_join";
        private const string RPC_LIVE_EVENTS_CLAIM = "satori_live_events_claim";
        private const string RPC_MESSAGES_LIST = "satori_messages_list";
        private const string RPC_MESSAGES_READ = "satori_messages_read";
        private const string RPC_MESSAGES_DELETE = "satori_messages_delete";
        private const string RPC_METRICS_QUERY = "satori_metrics_query";

        #endregion

        #region Properties

        public static IVXSatoriClient Instance => _instance;
        public bool IsInitialized => _initialized;

        /// <summary>Fired after initialization. Bool indicates success.</summary>
        public event Action<bool> OnInitialized;

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
            DontDestroyOnLoad(gameObject);
        }

        private void OnDestroy()
        {
            if (_instance == this) _instance = null;
        }

        #endregion

        #region Initialization

        /// <summary>
        /// Initialize with a valid Nakama client and session.
        /// </summary>
        public void Initialize(IClient client, ISession session)
        {
            if (client == null) throw new ArgumentNullException(nameof(client));
            if (session == null) throw new ArgumentNullException(nameof(session));

            _rpc = new IVXSatoriRpcClient(client, session);
            _initialized = true;

            Debug.Log("[IVXSatori] Initialized.");
            OnInitialized?.Invoke(true);
        }

        /// <summary>
        /// Update session after token refresh.
        /// </summary>
        public void RefreshSession(ISession session)
        {
            _rpc?.UpdateSession(session);
        }

        #endregion

        #region Event Capture

        /// <summary>
        /// Capture a single analytics event.
        /// </summary>
        public async Task CaptureEventAsync(string eventName, Dictionary<string, string> metadata = null)
        {
            EnsureReady();
            await _rpc.CallVoidAsync(RPC_EVENT, new { name = eventName, metadata });
        }

        /// <summary>
        /// Capture multiple events in a single batch call.
        /// Returns the number submitted and actually captured (post-validation).
        /// </summary>
        public async Task<IVXSatoriEventBatchResponse> CaptureEventsBatchAsync(List<IVXSatoriEvent> events)
        {
            EnsureReady();
            if (events == null || events.Count == 0)
                return new IVXSatoriEventBatchResponse { submitted = 0, captured = 0 };
            var data = await _rpc.CallAsync<IVXSatoriEventBatchResponse>(RPC_EVENTS_BATCH, new { events });
            return data ?? new IVXSatoriEventBatchResponse();
        }

        #endregion

        #region Identity Properties

        /// <summary>
        /// Get full identity properties (default, custom, computed).
        /// </summary>
        public async Task<IVXSatoriIdentity> GetIdentityAsync()
        {
            EnsureReady();
            var data = await _rpc.CallAsync<IVXSatoriIdentity>(RPC_IDENTITY_GET);
            return data ?? new IVXSatoriIdentity();
        }

        /// <summary>
        /// Update identity properties.
        /// </summary>
        public async Task UpdateIdentityAsync(
            Dictionary<string, string> defaultProperties = null,
            Dictionary<string, string> customProperties = null)
        {
            EnsureReady();
            await _rpc.CallVoidAsync(RPC_IDENTITY_UPDATE, new { defaultProperties, customProperties });
        }

        #endregion

        #region Audiences

        /// <summary>
        /// Get audience memberships for the current user.
        /// </summary>
        public async Task<List<string>> GetAudienceMembershipsAsync()
        {
            EnsureReady();
            var data = await _rpc.CallAsync<List<string>>(RPC_AUDIENCES_GET);
            return data ?? new List<string>();
        }

        #endregion

        #region Feature Flags

        /// <summary>
        /// Get a single feature flag by name.
        /// </summary>
        public async Task<IVXSatoriFlag> GetFlagAsync(string name, string defaultValue = "")
        {
            EnsureReady();
            var data = await _rpc.CallAsync<IVXSatoriFlag>(RPC_FLAGS_GET, new { name, defaultValue });
            return data ?? new IVXSatoriFlag(name, defaultValue);
        }

        /// <summary>
        /// Get all feature flags, optionally filtered by a list of names.
        /// </summary>
        public async Task<List<IVXSatoriFlag>> GetAllFlagsAsync(List<string> names = null)
        {
            EnsureReady();
            var payload = names != null && names.Count > 0 ? (object)new { names } : new { };
            var data = await _rpc.CallAsync<List<IVXSatoriFlag>>(RPC_FLAGS_GET_ALL, payload);
            return data ?? new List<IVXSatoriFlag>();
        }

        #endregion

        #region Experiments

        /// <summary>
        /// Get all running experiments and the current user's variant assignments.
        /// </summary>
        public async Task<IVXSatoriExperimentsResponse> GetExperimentsAsync()
        {
            EnsureReady();
            var data = await _rpc.CallAsync<IVXSatoriExperimentsResponse>(RPC_EXPERIMENTS_GET);
            return data ?? new IVXSatoriExperimentsResponse();
        }

        /// <summary>
        /// Get the assigned variant for a specific experiment.
        /// </summary>
        public async Task<IVXSatoriExperimentVariant> GetExperimentVariantAsync(string experimentId)
        {
            EnsureReady();
            var data = await _rpc.CallAsync<IVXSatoriExperimentVariant>(RPC_EXPERIMENTS_VARIANT, new { experimentId });
            return data;
        }

        #endregion

        #region Live Events

        /// <summary>
        /// List live events, optionally filtered by names.
        /// </summary>
        public async Task<IVXSatoriLiveEventsResponse> GetLiveEventsAsync(List<string> names = null)
        {
            EnsureReady();
            var payload = names != null && names.Count > 0 ? (object)new { names } : new { };
            var data = await _rpc.CallAsync<IVXSatoriLiveEventsResponse>(RPC_LIVE_EVENTS_LIST, payload);
            return data ?? new IVXSatoriLiveEventsResponse();
        }

        /// <summary>
        /// Join an active live event.
        /// </summary>
        public async Task<bool> JoinLiveEventAsync(string eventId)
        {
            EnsureReady();
            return await _rpc.CallVoidAsync(RPC_LIVE_EVENTS_JOIN, new { eventId });
        }

        /// <summary>
        /// Claim rewards for a live event.
        /// </summary>
        public async Task<IVXSatoriLiveEventReward> ClaimLiveEventAsync(string eventId, string gameId = null)
        {
            EnsureReady();
            var data = await _rpc.CallAsync<IVXSatoriLiveEventReward>(RPC_LIVE_EVENTS_CLAIM, new { eventId, gameId });
            return data ?? new IVXSatoriLiveEventReward();
        }

        #endregion

        #region Messages

        /// <summary>
        /// List all messages in the inbox.
        /// </summary>
        public async Task<IVXSatoriMessagesResponse> GetMessagesAsync()
        {
            EnsureReady();
            var data = await _rpc.CallAsync<IVXSatoriMessagesResponse>(RPC_MESSAGES_LIST);
            return data ?? new IVXSatoriMessagesResponse();
        }

        /// <summary>
        /// Mark a message as read and claim its reward (if any).
        /// </summary>
        public async Task<IVXSatoriMessageReadResponse> ReadMessageAsync(string messageId, string gameId = null)
        {
            EnsureReady();
            var data = await _rpc.CallAsync<IVXSatoriMessageReadResponse>(RPC_MESSAGES_READ, new { messageId, gameId });
            return data;
        }

        /// <summary>
        /// Delete a message.
        /// </summary>
        public async Task<bool> DeleteMessageAsync(string messageId)
        {
            EnsureReady();
            return await _rpc.CallVoidAsync(RPC_MESSAGES_DELETE, new { messageId });
        }

        #endregion

        #region Metrics

        /// <summary>
        /// Query a metric by ID with optional time range.
        /// </summary>
        public async Task<IVXSatoriMetricQueryResponse> QueryMetricAsync(
            string metricId, string startDate = null, string endDate = null, string granularity = null)
        {
            EnsureReady();
            var data = await _rpc.CallAsync<IVXSatoriMetricQueryResponse>(RPC_METRICS_QUERY,
                new { metricId, startDate, endDate, granularity });
            return data ?? new IVXSatoriMetricQueryResponse();
        }

        #endregion

        #region Helpers

        private void EnsureReady()
        {
            if (!_initialized || _rpc == null)
                throw new InvalidOperationException("IVXSatoriClient is not initialized. Call Initialize() first.");
        }

        #endregion
    }
}
