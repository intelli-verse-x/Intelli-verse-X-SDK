using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using IntelliVerseX.Backend;
using IntelliVerseX.Hiro;
using Nakama;
using UnityEngine;

namespace IntelliVerseX.Retention
{
    /// <summary>
    /// Manages weekly goals and monthly milestones including progress tracking and completion.
    /// </summary>
    public class IVXGoalsManager : MonoBehaviour
    {
        #region Singleton

        private static IVXGoalsManager _instance;

        /// <summary>
        /// Singleton instance of the goals manager.
        /// </summary>
        public static IVXGoalsManager Instance
        {
            get
            {
                if (_instance == null)
                    _instance = FindAnyObjectByType<IVXGoalsManager>();
                return _instance;
            }
        }

        #endregion

        #region Events

        /// <summary>Raised when a weekly goal is completed.</summary>
        public event Action<IVXWeeklyGoal> OnGoalCompleted;

        /// <summary>Raised when a monthly milestone is reached.</summary>
        public event Action<IVXMonthlyMilestone> OnMilestoneReached;

        /// <summary>Raised when the goals list is refreshed.</summary>
        public event Action OnGoalsRefreshed;

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
        /// Initializes the goals manager with a Nakama client and session.
        /// </summary>
        /// <param name="client">The Nakama client.</param>
        /// <param name="session">The active Nakama session.</param>
        public void Initialize(IClient client, ISession session)
        {
            _rpcClient = new IVXHiroRpcClient(client, session);
            _isInitialized = true;
            Debug.Log($"[{nameof(IVXGoalsManager)}] Initialized");
        }

        /// <summary>
        /// Retrieves current goals. Prod has no <c>weekly_goals_get</c> — uses liveops
        /// <c>daily_missions_get</c> and maps missions into weekly goal models.
        /// </summary>
        public async Task<List<IVXWeeklyGoal>> GetWeeklyGoalsAsync(string gameId = null)
        {
            object payload = string.IsNullOrEmpty(gameId)
                ? new { }
                : new { gameId, game_id = gameId };
            var rpc = await _rpcClient.CallAsync<IVXDailyMissionsGoalsBridge>("daily_missions_get", payload);
            if (!HiroRpcResponseUtility.TryGetData(rpc, out var envelope, "daily_missions_get"))
                return new List<IVXWeeklyGoal>();

            OnGoalsRefreshed?.Invoke();
            return envelope?.ToWeeklyGoals() ?? new List<IVXWeeklyGoal>();
        }

        /// <summary>
        /// Updates progress toward a weekly goal.
        /// Prefers prod <c>weekly_goals_update_progress</c>; also sends <c>value</c> for mission aliases.
        /// </summary>
        public async Task<IVXWeeklyGoal> UpdateWeeklyProgressAsync(string goalId, int progress)
        {
            var payload = new
            {
                goal_id = goalId,
                goalId,
                missionId = goalId,
                mission_id = goalId,
                progress,
                value = progress
            };
            var rpc = await _rpcClient.CallAsync<IVXWeeklyGoal>("weekly_goals_update_progress", payload);
            if (!HiroRpcResponseUtility.TryGetData(rpc, out var goal, "weekly_goals_update_progress"))
            {
                // Fallback: canonical daily missions progress (prod note: may be auto-tracked).
                var missionRpc = await _rpcClient.CallAsync<IVXWeeklyGoal>("daily_missions_update_progress", payload);
                if (!HiroRpcResponseUtility.TryGetData(missionRpc, out goal, "daily_missions_update_progress"))
                    return null;
            }

            if (goal != null && goal.completed)
                OnGoalCompleted?.Invoke(goal);
            return goal;
        }

        /// <summary>
        /// Monthly milestones: prod has no list RPC. Returns empty and logs once.
        /// Progress updates still go through <c>monthly_milestones_update_progress</c> when available.
        /// </summary>
        public async Task<List<IVXMonthlyMilestone>> GetMonthlyMilestonesAsync()
        {
            Debug.LogWarning($"[{nameof(IVXGoalsManager)}] monthly_milestones_get is not registered on prod Nakama; returning empty list.");
            await Task.CompletedTask;
            return new List<IVXMonthlyMilestone>();
        }

        /// <summary>
        /// Updates progress toward a monthly milestone.
        /// Prod RPC: <c>monthly_milestones_update_progress</c>.
        /// </summary>
        public async Task<IVXMonthlyMilestone> UpdateMonthlyProgressAsync(string milestoneId, int progress)
        {
            var payload = new
            {
                milestone_id = milestoneId,
                milestoneId,
                progress,
                value = progress
            };
            var rpc = await _rpcClient.CallAsync<IVXMonthlyMilestone>("monthly_milestones_update_progress", payload);
            if (!HiroRpcResponseUtility.TryGetData(rpc, out var milestone, "monthly_milestones_update_progress"))
                return null;
            if (milestone != null && milestone.completed)
                OnMilestoneReached?.Invoke(milestone);
            return milestone;
        }

        #endregion
    }
}
