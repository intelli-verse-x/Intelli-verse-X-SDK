using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using IntelliVerseX.Hiro;
using Nakama;
using UnityEngine;

namespace IntelliVerseX.Competition
{
    /// <summary>
    /// Manages ranked league progression — tier tracking, point submission,
    /// leaderboards, and season lifecycle via Nakama RPCs.
    /// </summary>
    public sealed class IVXLeagueManager : MonoBehaviour
    {
        #region Constants

        private const string LOG_TAG = "[IVXLeague]";
        private const string RPC_GET_STATE = "league_get_state";
        private const string RPC_SUBMIT_POINTS = "league_submit_points";
        private const string RPC_GET_LEADERBOARD = "league_get_leaderboard";
        private const string RPC_PROCESS_SEASON = "league_process_season";

        #endregion

        #region Private Fields

        private static IVXLeagueManager _instance;
        private IVXHiroRpcClient _rpcClient;
        private bool _initialized;
        private LeagueState _cachedState;

        #endregion

        #region Properties

        /// <summary>Singleton accessor.</summary>
        public static IVXLeagueManager Instance => _instance;

        /// <summary>Whether the manager has been initialized.</summary>
        public bool IsInitialized => _initialized;

        /// <summary>Last fetched league state (may be null before first query).</summary>
        public LeagueState CurrentState => _cachedState;

        #endregion

        #region Events

        /// <summary>Fired when the league state is refreshed.</summary>
        public event Action<LeagueState> OnLeagueStateUpdated;

        /// <summary>Fired after points are successfully submitted.</summary>
        public event Action<LeaguePointsResult> OnPointsSubmitted;

        /// <summary>Fired when the user is promoted to a higher tier.</summary>
        public event Action<LeagueTier> OnPromotion;

        /// <summary>Fired when the user is relegated to a lower tier.</summary>
        public event Action<LeagueTier> OnRelegation;

        /// <summary>Fired when a season ends and results are processed.</summary>
        public event Action<SeasonInfo> OnSeasonEnded;

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
        /// Fetch the current league state from the server.
        /// </summary>
        /// <returns>League state or null on failure.</returns>
        public async Task<LeagueState> GetStateAsync()
        {
            EnsureReady();

            var response = await _rpcClient.CallAsync<LeagueState>(RPC_GET_STATE);

            if (response.success && response.data != null)
            {
                var previousTier = _cachedState?.tier;
                _cachedState = response.data;

                OnLeagueStateUpdated?.Invoke(response.data);

                if (previousTier.HasValue && previousTier.Value != response.data.tier)
                {
                    if (response.data.tier > previousTier.Value)
                        OnPromotion?.Invoke(response.data.tier);
                    else
                        OnRelegation?.Invoke(response.data.tier);
                }

                return response.data;
            }

            Debug.LogWarning($"{LOG_TAG} GetState failed: {response.error}");
            return null;
        }

        /// <summary>
        /// Submit earned points to the league.
        /// </summary>
        /// <param name="points">Number of points to submit (must be positive).</param>
        /// <returns>Points result or null on failure.</returns>
        public async Task<LeaguePointsResult> SubmitPointsAsync(int points)
        {
            EnsureReady();

            if (points <= 0)
                throw new ArgumentOutOfRangeException(nameof(points), "Points must be positive.");

            var response = await _rpcClient.CallAsync<LeaguePointsResult>(
                RPC_SUBMIT_POINTS,
                new { points });

            if (response.success && response.data != null)
            {
                Debug.Log($"{LOG_TAG} Submitted {points} pts — total: {response.data.points}, rank: {response.data.rank}");
                OnPointsSubmitted?.Invoke(response.data);

                if (response.data.promoted)
                    OnPromotion?.Invoke(response.data.tier);

                return response.data;
            }

            Debug.LogWarning($"{LOG_TAG} SubmitPoints failed: {response.error}");
            return null;
        }

        /// <summary>
        /// Fetch the league leaderboard for the user's current tier.
        /// </summary>
        /// <returns>Leaderboard entries or an empty list on failure.</returns>
        public async Task<LeagueLeaderboardResponse> GetLeaderboardAsync()
        {
            EnsureReady();

            var response = await _rpcClient.CallAsync<LeagueLeaderboardResponse>(RPC_GET_LEADERBOARD);

            if (response.success && response.data != null)
                return response.data;

            Debug.LogWarning($"{LOG_TAG} GetLeaderboard failed: {response.error}");
            return null;
        }

        /// <summary>
        /// Fetch current season information.
        /// </summary>
        /// <returns>Season info or null on failure.</returns>
        public async Task<SeasonInfo> GetSeasonInfoAsync()
        {
            EnsureReady();

            var response = await _rpcClient.CallAsync<SeasonInfo>(RPC_PROCESS_SEASON);

            if (response.success && response.data != null)
            {
                if (!response.data.isActive)
                    OnSeasonEnded?.Invoke(response.data);

                return response.data;
            }

            Debug.LogWarning($"{LOG_TAG} GetSeasonInfo failed: {response.error}");
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
