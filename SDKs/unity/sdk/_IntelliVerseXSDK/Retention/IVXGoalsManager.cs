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
        /// Retrieves the current weekly goals for the player.
        /// </summary>
        /// <returns>A list of weekly goals.</returns>
        public async Task<List<IVXWeeklyGoal>> GetWeeklyGoalsAsync()
        {
            var rpc = await _rpcClient.CallAsync<IVXWeeklyGoalsResponse>("weekly_goals_get");
            if (!HiroRpcResponseUtility.TryGetData(rpc, out var envelope, "weekly_goals_get"))
                return new List<IVXWeeklyGoal>();
            OnGoalsRefreshed?.Invoke();
            return envelope?.goals ?? new List<IVXWeeklyGoal>();
        }

        /// <summary>
        /// Updates progress toward a weekly goal.
        /// </summary>
        /// <param name="goalId">The goal identifier.</param>
        /// <param name="progress">The progress increment.</param>
        /// <returns>The updated weekly goal.</returns>
        public async Task<IVXWeeklyGoal> UpdateWeeklyProgressAsync(string goalId, int progress)
        {
            var payload = new IVXWeeklyGoalProgressRequest
            {
                goalId = goalId,
                progress = progress
            };
            var rpc = await _rpcClient.CallAsync<IVXWeeklyGoal>("weekly_goals_update_progress", payload);
            if (!HiroRpcResponseUtility.TryGetData(rpc, out var goal, "weekly_goals_update_progress"))
                return null;
            if (goal != null && goal.completed)
                OnGoalCompleted?.Invoke(goal);
            return goal;
        }

        /// <summary>
        /// Retrieves the current monthly milestones for the player.
        /// </summary>
        /// <returns>A list of monthly milestones.</returns>
        public async Task<List<IVXMonthlyMilestone>> GetMonthlyMilestonesAsync()
        {
            var rpc = await _rpcClient.CallAsync<IVXMonthlyMilestonesResponse>("monthly_milestones_get");
            if (!HiroRpcResponseUtility.TryGetData(rpc, out var envelope, "monthly_milestones_get"))
                return new List<IVXMonthlyMilestone>();
            return envelope?.milestones ?? new List<IVXMonthlyMilestone>();
        }

        /// <summary>
        /// Updates progress toward a monthly milestone.
        /// </summary>
        /// <param name="milestoneId">The milestone identifier.</param>
        /// <param name="progress">The progress increment.</param>
        /// <returns>The updated monthly milestone.</returns>
        public async Task<IVXMonthlyMilestone> UpdateMonthlyProgressAsync(string milestoneId, int progress)
        {
            var payload = new IVXMonthlyMilestoneProgressRequest
            {
                milestoneId = milestoneId,
                progress = progress
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
