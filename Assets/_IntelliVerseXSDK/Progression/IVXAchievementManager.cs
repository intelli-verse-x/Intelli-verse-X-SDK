using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using IntelliVerseX.Backend;
using Nakama;
using UnityEngine;

namespace IntelliVerseX.Progression
{
    /// <summary>
    /// Manages player achievements including progress tracking, unlocking, and reward claiming.
    /// </summary>
    public class IVXAchievementManager : MonoBehaviour
    {
        #region Singleton

        private static IVXAchievementManager _instance;

        /// <summary>
        /// Singleton instance of the achievement manager.
        /// </summary>
        public static IVXAchievementManager Instance
        {
            get
            {
                if (_instance == null)
                    _instance = FindAnyObjectByType<IVXAchievementManager>();
                return _instance;
            }
        }

        #endregion

        #region Events

        /// <summary>Raised when an achievement is unlocked.</summary>
        public event Action<IVXAchievement> OnAchievementUnlocked;

        /// <summary>Raised when achievement progress is updated.</summary>
        public event Action<IVXAchievement> OnProgressUpdated;

        /// <summary>Raised when an achievement reward is claimed.</summary>
        public event Action<IVXAchievement> OnRewardClaimed;

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
        /// Initializes the achievement manager with a Nakama client and session.
        /// </summary>
        /// <param name="client">The Nakama client.</param>
        /// <param name="session">The active Nakama session.</param>
        public void Initialize(IClient client, ISession session)
        {
            _rpcClient = new IVXHiroRpcClient(client, session);
            _isInitialized = true;
            Debug.Log($"[{nameof(IVXAchievementManager)}] Initialized");
        }

        /// <summary>
        /// Retrieves all achievements for the current player.
        /// </summary>
        /// <returns>A list of achievements.</returns>
        public async Task<List<IVXAchievement>> GetAllAsync()
        {
            var response = await _rpcClient.CallAsync<IVXAchievementListResponse>("achievements_get_all");
            return response?.achievements ?? new List<IVXAchievement>();
        }

        /// <summary>
        /// Tracks progress toward an achievement.
        /// </summary>
        /// <param name="achievementId">The achievement identifier.</param>
        /// <param name="progress">The progress increment.</param>
        /// <returns>The updated achievement.</returns>
        public async Task<IVXAchievement> TrackProgressAsync(string achievementId, int progress)
        {
            var payload = new IVXAchievementProgressRequest
            {
                achievementId = achievementId,
                progress = progress
            };
            var achievement = await _rpcClient.CallAsync<IVXAchievement>("achievements_track_progress", payload);
            if (achievement != null)
            {
                OnProgressUpdated?.Invoke(achievement);
                if (achievement.unlocked)
                    OnAchievementUnlocked?.Invoke(achievement);
            }
            return achievement;
        }

        /// <summary>
        /// Claims the reward for a completed achievement.
        /// </summary>
        /// <param name="achievementId">The achievement identifier.</param>
        /// <returns>The updated achievement.</returns>
        public async Task<IVXAchievement> ClaimRewardAsync(string achievementId)
        {
            var payload = new IVXAchievementClaimRequest { achievementId = achievementId };
            var achievement = await _rpcClient.CallAsync<IVXAchievement>("achievements_claim_reward", payload);
            if (achievement != null)
                OnRewardClaimed?.Invoke(achievement);
            return achievement;
        }

        #endregion
    }
}
