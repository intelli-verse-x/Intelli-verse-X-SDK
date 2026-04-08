using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using IntelliVerseX.Backend;
using IntelliVerseX.Hiro;
using Nakama;
using UnityEngine;

namespace IntelliVerseX.Social
{
    /// <summary>
    /// Manages friend interaction streaks and cooperative friend quests.
    /// </summary>
    public class IVXFriendStreakManager : MonoBehaviour
    {
        #region Singleton

        private static IVXFriendStreakManager _instance;

        /// <summary>
        /// Singleton instance of the friend streak manager.
        /// </summary>
        public static IVXFriendStreakManager Instance
        {
            get
            {
                if (_instance == null)
                    _instance = FindAnyObjectByType<IVXFriendStreakManager>();
                return _instance;
            }
        }

        #endregion

        #region Events

        /// <summary>Raised when a friend streak is updated.</summary>
        public event Action<IVXFriendStreak> OnStreakUpdated;

        /// <summary>Raised when a friend streak is broken.</summary>
        public event Action<IVXFriendStreak> OnStreakBroken;

        /// <summary>Raised when progress is made on a friend quest.</summary>
        public event Action<IVXFriendQuest> OnQuestProgress;

        /// <summary>Raised when a friend quest is completed.</summary>
        public event Action<IVXFriendQuest> OnQuestCompleted;

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
        /// Initializes the friend streak manager with a Nakama client and session.
        /// </summary>
        /// <param name="client">The Nakama client.</param>
        /// <param name="session">The active Nakama session.</param>
        public void Initialize(IClient client, ISession session)
        {
            _rpcClient = new IVXHiroRpcClient(client, session);
            _isInitialized = true;
            Debug.Log($"[{nameof(IVXFriendStreakManager)}] Initialized");
        }

        /// <summary>
        /// Retrieves all friend streaks for the current player.
        /// </summary>
        /// <returns>A list of friend streaks.</returns>
        public async Task<List<IVXFriendStreak>> GetStreaksAsync()
        {
            var rpc = await _rpcClient.CallAsync<IVXFriendStreakListResponse>("friend_streaks_get");
            if (!HiroRpcResponseUtility.TryGetData(rpc, out var envelope, "friend_streaks_get"))
                return new List<IVXFriendStreak>();
            return envelope?.streaks ?? new List<IVXFriendStreak>();
        }

        /// <summary>
        /// Records an interaction with a friend to maintain or increase the streak.
        /// </summary>
        /// <param name="friendId">The friend's user identifier.</param>
        /// <returns>The updated friend streak.</returns>
        public async Task<IVXFriendStreak> RecordInteractionAsync(string friendId)
        {
            var payload = new IVXFriendInteractionRequest { friendId = friendId };
            var rpc = await _rpcClient.CallAsync<IVXFriendStreak>("friend_streaks_interact", payload);
            if (!HiroRpcResponseUtility.TryGetData(rpc, out var streak, "friend_streaks_interact"))
                return null;
            if (streak != null)
            {
                if (streak.currentStreak == 0)
                    OnStreakBroken?.Invoke(streak);
                else
                    OnStreakUpdated?.Invoke(streak);
            }
            return streak;
        }

        /// <summary>
        /// Retrieves all active friend quests for the current player.
        /// </summary>
        /// <returns>A list of active friend quests.</returns>
        public async Task<List<IVXFriendQuest>> GetActiveQuestsAsync()
        {
            var rpc = await _rpcClient.CallAsync<IVXFriendQuestListResponse>("friend_quests_get_active");
            if (!HiroRpcResponseUtility.TryGetData(rpc, out var envelope, "friend_quests_get_active"))
                return new List<IVXFriendQuest>();
            return envelope?.quests ?? new List<IVXFriendQuest>();
        }

        /// <summary>
        /// Contributes progress to a friend quest.
        /// </summary>
        /// <param name="questId">The quest identifier.</param>
        /// <param name="progress">The progress increment.</param>
        /// <returns>The updated friend quest.</returns>
        public async Task<IVXFriendQuest> ContributeToQuestAsync(string questId, int progress)
        {
            var payload = new IVXFriendQuestContributeRequest
            {
                questId = questId,
                progress = progress
            };
            var rpc = await _rpcClient.CallAsync<IVXFriendQuest>("friend_quests_contribute", payload);
            if (!HiroRpcResponseUtility.TryGetData(rpc, out var quest, "friend_quests_contribute"))
                return null;
            if (quest != null)
            {
                OnQuestProgress?.Invoke(quest);
                if (quest.completed)
                    OnQuestCompleted?.Invoke(quest);
            }
            return quest;
        }

        #endregion
    }
}
