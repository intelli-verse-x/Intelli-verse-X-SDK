using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using IntelliVerseX.Backend;
using IntelliVerseX.Hiro;
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
            if (!_isInitialized) { Debug.LogError($"[{nameof(IVXAchievementManager)}] Not initialized. Call Initialize() first."); return new List<IVXAchievement>(); }
            var rpc = await _rpcClient.CallAsync<IVXAchievementListResponse>("achievements_get_all");
            if (!HiroRpcResponseUtility.TryGetData(rpc, out var envelope, "achievements_get_all"))
                return new List<IVXAchievement>();
            return envelope?.achievements ?? new List<IVXAchievement>();
        }

        /// <summary>
        /// Tracks progress toward an achievement.
        /// Prod RPC: <c>hiro_achievements_progress</c>.
        /// </summary>
        public async Task<IVXAchievement> TrackProgressAsync(string achievementId, int progress)
        {
            if (!_isInitialized) { Debug.LogError($"[{nameof(IVXAchievementManager)}] Not initialized. Call Initialize() first."); return null; }
            var payload = new
            {
                achievementId,
                achievement_id = achievementId,
                amount = progress,
                progress
            };
            var rpc = await _rpcClient.CallAsync<IVXAchievementProgressEnvelope>("hiro_achievements_progress", payload);
            if (!HiroRpcResponseUtility.TryGetData(rpc, out var envelope, "hiro_achievements_progress"))
                return null;

            var achievement = envelope?.achievement ?? envelope?.ToAchievement(achievementId, progress);
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
        /// Prod RPC: <c>hiro_achievements_claim</c>.
        /// </summary>
        public async Task<IVXAchievement> ClaimRewardAsync(string achievementId)
        {
            if (!_isInitialized) { Debug.LogError($"[{nameof(IVXAchievementManager)}] Not initialized. Call Initialize() first."); return null; }
            var payload = new
            {
                achievementId,
                achievement_id = achievementId
            };
            var rpc = await _rpcClient.CallAsync<IVXAchievement>("hiro_achievements_claim", payload);
            if (!HiroRpcResponseUtility.TryGetData(rpc, out var achievement, "hiro_achievements_claim"))
            {
                // Some prod shapes nest under achievement / return only reward — refresh list item.
                var all = await GetAllAsync();
                achievement = all?.Find(a => a != null && a.id == achievementId);
                if (achievement == null)
                    return null;
                achievement.rewardClaimed = true;
                achievement.unlocked = true;
            }

            OnRewardClaimed?.Invoke(achievement);
            return achievement;
        }

        #endregion
    }
}
