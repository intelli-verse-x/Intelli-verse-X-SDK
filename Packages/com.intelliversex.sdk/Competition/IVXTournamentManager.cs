using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using IntelliVerseX.Backend;
using IntelliVerseX.Hiro;
using Nakama;
using UnityEngine;

namespace IntelliVerseX.Competition
{
    /// <summary>
    /// Manages competitive tournaments including joining, score submission, and leaderboard retrieval.
    /// </summary>
    public class IVXTournamentManager : MonoBehaviour
    {
        #region Singleton

        private static IVXTournamentManager _instance;

        /// <summary>
        /// Singleton instance of the tournament manager.
        /// </summary>
        public static IVXTournamentManager Instance
        {
            get
            {
                if (_instance == null)
                    _instance = FindAnyObjectByType<IVXTournamentManager>();
                return _instance;
            }
        }

        #endregion

        #region Events

        /// <summary>Raised when the player joins a tournament.</summary>
        public event Action<IVXTournament> OnTournamentJoined;

        /// <summary>Raised when a score is submitted to a tournament.</summary>
        public event Action<IVXTournament> OnScoreSubmitted;

        /// <summary>Raised when a tournament ends.</summary>
        public event Action<IVXTournament> OnTournamentEnded;

        /// <summary>Raised when a tournament prize is awarded.</summary>
        public event Action<IVXTournamentPrize> OnPrizeAwarded;

        #endregion

        #region Private Fields

        private IVXHiroRpcClient _rpcClient;
        private bool _isInitialized;

        #endregion

        #region Properties

        /// <summary>Whether the manager has been initialized.</summary>
        public bool IsInitialized => _isInitialized;

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
            if (_instance == this)
                _instance = null;
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Initializes the tournament manager with a Nakama client and session.
        /// </summary>
        /// <param name="client">The Nakama client.</param>
        /// <param name="session">The active Nakama session.</param>
        public void Initialize(IClient client, ISession session)
        {
            _rpcClient = new IVXHiroRpcClient(client, session);
            _isInitialized = true;
            Debug.Log($"[{nameof(IVXTournamentManager)}] Initialized");
        }

        /// <summary>
        /// Retrieves all currently active tournaments.
        /// </summary>
        /// <returns>A list of active tournaments.</returns>
        public async Task<List<IVXTournament>> GetActiveTournamentsAsync()
        {
            if (!_isInitialized) { Debug.LogError($"[{nameof(IVXTournamentManager)}] Not initialized. Call Initialize() first."); return new List<IVXTournament>(); }
            var rpc = await _rpcClient.CallAsync<IVXTournamentListResponse>("tournament_get_active");
            if (!HiroRpcResponseUtility.TryGetData(rpc, out var envelope, "tournament_get_active"))
                return new List<IVXTournament>();
            return envelope?.tournaments ?? new List<IVXTournament>();
        }

        /// <summary>
        /// Joins a tournament by its identifier.
        /// </summary>
        /// <param name="tournamentId">The tournament identifier.</param>
        /// <returns>The joined tournament.</returns>
        public async Task<IVXTournament> JoinAsync(string tournamentId)
        {
            if (!_isInitialized) { Debug.LogError($"[{nameof(IVXTournamentManager)}] Not initialized. Call Initialize() first."); return null; }
            var payload = new IVXTournamentJoinRequest { tournamentId = tournamentId };
            var rpc = await _rpcClient.CallAsync<IVXTournament>("tournament_join", payload);
            if (!HiroRpcResponseUtility.TryGetData(rpc, out var tournament, "tournament_join"))
                return null;
            if (tournament != null)
                OnTournamentJoined?.Invoke(tournament);
            return tournament;
        }

        /// <summary>
        /// Submits a score to a tournament.
        /// </summary>
        /// <param name="tournamentId">The tournament identifier.</param>
        /// <param name="score">The score to submit.</param>
        /// <returns>The updated tournament with new rank.</returns>
        public async Task<IVXTournament> SubmitScoreAsync(string tournamentId, long score)
        {
            if (!_isInitialized) { Debug.LogError($"[{nameof(IVXTournamentManager)}] Not initialized. Call Initialize() first."); return null; }
            var payload = new IVXTournamentScoreRequest
            {
                tournamentId = tournamentId,
                score = score
            };
            var rpc = await _rpcClient.CallAsync<IVXTournament>("tournament_submit_score", payload);
            if (!HiroRpcResponseUtility.TryGetData(rpc, out var tournament, "tournament_submit_score"))
                return null;
            if (tournament != null)
                OnScoreSubmitted?.Invoke(tournament);
            return tournament;
        }

        /// <summary>
        /// Retrieves the leaderboard for a tournament.
        /// </summary>
        /// <param name="tournamentId">The tournament identifier.</param>
        /// <returns>A list of tournament entries.</returns>
        public async Task<List<IVXTournamentEntry>> GetLeaderboardAsync(string tournamentId)
        {
            if (!_isInitialized) { Debug.LogError($"[{nameof(IVXTournamentManager)}] Not initialized. Call Initialize() first."); return new List<IVXTournamentEntry>(); }
            var payload = new IVXTournamentLeaderboardRequest { tournamentId = tournamentId };
            var rpc = await _rpcClient.CallAsync<IVXTournamentLeaderboardResponse>("tournament_get_leaderboard", payload);
            if (!HiroRpcResponseUtility.TryGetData(rpc, out var envelope, "tournament_get_leaderboard"))
                return new List<IVXTournamentEntry>();
            return envelope?.entries ?? new List<IVXTournamentEntry>();
        }

        #endregion
    }
}
