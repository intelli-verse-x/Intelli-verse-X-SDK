using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using IntelliVerseX.Hiro;
using Nakama;
using UnityEngine;

namespace IntelliVerseX.Missions
{
    /// <summary>
    /// Manages server-authoritative daily missions — fetching, progress
    /// tracking, and reward claiming via Nakama RPCs.
    /// </summary>
    public sealed class IVXDailyMissionsManager : MonoBehaviour
    {
        #region Constants

        private const string LOG_TAG = "[IVXDailyMissions]";
        private const string RPC_GET_MISSIONS = "daily_missions_get";
        private const string RPC_UPDATE_PROGRESS = "daily_missions_update_progress";
        private const string RPC_CLAIM = "daily_missions_claim";

        #endregion

        #region Private Fields

        private static IVXDailyMissionsManager _instance;
        private IVXHiroRpcClient _rpcClient;
        private bool _initialized;
        private List<DailyMission> _cachedMissions;

        #endregion

        #region Properties

        /// <summary>Singleton accessor.</summary>
        public static IVXDailyMissionsManager Instance => _instance;

        /// <summary>Whether the manager has been initialized.</summary>
        public bool IsInitialized => _initialized;

        /// <summary>Last fetched list of today's missions (may be null).</summary>
        public IReadOnlyList<DailyMission> CachedMissions => _cachedMissions;

        #endregion

        #region Events

        /// <summary>Fired when the missions list is refreshed from the server.</summary>
        public event Action<List<DailyMission>> OnMissionsRefreshed;

        /// <summary>Fired when progress is updated on a mission.</summary>
        public event Action<MissionProgressResult> OnMissionProgress;

        /// <summary>Fired when a mission's progress reaches the target.</summary>
        public event Action<string> OnMissionCompleted;

        /// <summary>Fired when a mission reward is successfully claimed.</summary>
        public event Action<MissionClaimResult> OnMissionRewardClaimed;

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
        /// Fetch today's daily missions from the server.
        /// </summary>
        /// <returns>List of today's missions, or an empty list on failure.</returns>
        public async Task<List<DailyMission>> GetTodaysMissionsAsync()
        {
            EnsureReady();

            var response = await _rpcClient.CallAsync<DailyMissionsResponse>(RPC_GET_MISSIONS);

            if (response.success && response.data != null)
            {
                _cachedMissions = response.data.missions ?? new List<DailyMission>();
                OnMissionsRefreshed?.Invoke(_cachedMissions);
                return _cachedMissions;
            }

            Debug.LogWarning($"{LOG_TAG} GetTodaysMissions failed: {response.error}");
            return new List<DailyMission>();
        }

        /// <summary>
        /// Report progress on a specific mission.
        /// </summary>
        /// <param name="missionId">Identifier of the mission to update.</param>
        /// <param name="progress">Progress delta to add.</param>
        /// <returns>Progress result, or null on failure.</returns>
        public async Task<MissionProgressResult> UpdateProgressAsync(string missionId, int progress)
        {
            EnsureReady();

            if (string.IsNullOrEmpty(missionId))
                throw new ArgumentNullException(nameof(missionId));

            var response = await _rpcClient.CallAsync<MissionProgressResult>(
                RPC_UPDATE_PROGRESS,
                new { mission_id = missionId, progress });

            if (response.success && response.data != null)
            {
                OnMissionProgress?.Invoke(response.data);

                if (response.data.completed)
                    OnMissionCompleted?.Invoke(response.data.missionId);

                return response.data;
            }

            Debug.LogWarning($"{LOG_TAG} UpdateProgress failed for {missionId}: {response.error}");
            return null;
        }

        /// <summary>
        /// Claim the reward for a completed mission.
        /// </summary>
        /// <param name="missionId">Identifier of the mission to claim.</param>
        /// <returns>Claim result, or null on failure.</returns>
        public async Task<MissionClaimResult> ClaimRewardAsync(string missionId)
        {
            EnsureReady();

            if (string.IsNullOrEmpty(missionId))
                throw new ArgumentNullException(nameof(missionId));

            var response = await _rpcClient.CallAsync<MissionClaimResult>(
                RPC_CLAIM,
                new { mission_id = missionId });

            if (response.success && response.data != null)
            {
                Debug.Log($"{LOG_TAG} Claimed reward for mission: {missionId}");
                OnMissionRewardClaimed?.Invoke(response.data);
                return response.data;
            }

            Debug.LogWarning($"{LOG_TAG} ClaimReward failed for {missionId}: {response.error}");
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
