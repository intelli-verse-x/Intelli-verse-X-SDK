using System;
using System.Threading.Tasks;
using IntelliVerseX.Hiro;
using Nakama;
using UnityEngine;

namespace IntelliVerseX.DailyRewards
{
    /// <summary>
    /// Server-authoritative daily-rewards service backed by Nakama RPCs.
    /// Provides state queries, claiming, and calendar retrieval.
    /// </summary>
    public sealed class IVXDailyRewardsBackendService : MonoBehaviour
    {
        #region Constants

        private const string LOG_TAG = "[IVXDailyRewards]";
        private const string RPC_GET_STATE = "daily_rewards_get_state";
        private const string RPC_CLAIM = "daily_rewards_claim";
        private const string RPC_GET_CALENDAR = "daily_rewards_get_calendar";

        #endregion

        #region Private Fields

        private static IVXDailyRewardsBackendService _instance;
        private IVXHiroRpcClient _rpcClient;
        private bool _initialized;
        private DailyRewardState _cachedState;

        #endregion

        #region Properties

        /// <summary>Singleton accessor.</summary>
        public static IVXDailyRewardsBackendService Instance => _instance;

        /// <summary>Whether the service has been initialized.</summary>
        public bool IsInitialized => _initialized;

        /// <summary>Last fetched reward state (may be null before first query).</summary>
        public DailyRewardState CurrentState => _cachedState;

        #endregion

        #region Events

        /// <summary>Fired when a daily reward is successfully claimed.</summary>
        public event Action<DailyRewardClaimResult> OnRewardClaimed;

        /// <summary>Fired when the calendar data is refreshed from the server.</summary>
        public event Action<DailyRewardCalendar> OnCalendarUpdated;

        /// <summary>Fired when the server reports the user's streak was broken.</summary>
        public event Action<int> OnStreakBroken;

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
        /// <param name="client">Authenticated Nakama client.</param>
        /// <param name="session">Authenticated Nakama session.</param>
        public void Initialize(IClient client, ISession session)
        {
            if (client == null) throw new ArgumentNullException(nameof(client));
            if (session == null) throw new ArgumentNullException(nameof(session));

            _rpcClient = new IVXHiroRpcClient(client, session);
            _initialized = true;

            Debug.Log($"{LOG_TAG} Initialized.");
        }

        /// <summary>
        /// Update the session after a token refresh.
        /// </summary>
        public void RefreshSession(ISession session)
        {
            _rpcClient?.UpdateSession(session);
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Fetch the current daily-reward state from the server.
        /// </summary>
        /// <returns>Current state or null on failure.</returns>
        public async Task<DailyRewardState> GetStateAsync()
        {
            EnsureReady();

            var response = await _rpcClient.CallAsync<DailyRewardState>(RPC_GET_STATE);

            if (response.success && response.data != null)
            {
                var previousStreak = _cachedState?.streak ?? 0;
                _cachedState = response.data;

                if (previousStreak > 0 && response.data.streak == 0)
                    OnStreakBroken?.Invoke(previousStreak);

                return response.data;
            }

            Debug.LogWarning($"{LOG_TAG} GetState failed: {response.error}");
            return null;
        }

        /// <summary>
        /// Claim today's daily reward.
        /// </summary>
        /// <returns>Claim result or null on failure.</returns>
        public async Task<DailyRewardClaimResult> ClaimTodayAsync()
        {
            EnsureReady();

            var response = await _rpcClient.CallAsync<DailyRewardClaimResult>(RPC_CLAIM);

            if (response.success && response.data != null)
            {
                Debug.Log($"{LOG_TAG} Claimed day {response.data.day} — streak: {response.data.streak}");
                OnRewardClaimed?.Invoke(response.data);
                return response.data;
            }

            Debug.LogWarning($"{LOG_TAG} Claim failed: {response.error}");
            return null;
        }

        /// <summary>
        /// Fetch the full daily-reward calendar from the server.
        /// </summary>
        /// <returns>Calendar data or null on failure.</returns>
        public async Task<DailyRewardCalendar> GetCalendarAsync()
        {
            EnsureReady();

            var response = await _rpcClient.CallAsync<DailyRewardCalendar>(RPC_GET_CALENDAR);

            if (response.success && response.data != null)
            {
                OnCalendarUpdated?.Invoke(response.data);
                return response.data;
            }

            Debug.LogWarning($"{LOG_TAG} GetCalendar failed: {response.error}");
            return null;
        }

        #endregion

        #region Helpers

        private void EnsureReady()
        {
            if (!_initialized || _rpcClient == null)
                throw new InvalidOperationException($"{LOG_TAG} Not initialized. Call Initialize() first.");
        }

        #endregion
    }
}
