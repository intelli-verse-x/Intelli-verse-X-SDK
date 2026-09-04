// Copyright (c) 2026 Intelli-verse-X
// MIT License — see LICENSE in the project root.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using IntelliVerseX.Hiro;
using Nakama;
using UnityEngine;

namespace IntelliVerseX.Quest
{
    /// <summary>
    /// Server-authoritative quest/challenge manager — fetching, progress
    /// tracking, reward claiming, and event-driven updates via Nakama RPCs.
    /// </summary>
    public sealed class IVXQuestManager : MonoBehaviour
    {
        #region Constants

        private const string LOG_TAG = "[IVXQuest]";
        private const string RPC_GET_QUESTS = "ivx_quest_get";
        private const string RPC_PROGRESS = "ivx_quest_progress";
        private const string RPC_CLAIM = "ivx_quest_claim";
        private const string RPC_GET_CONFIG = "ivx_quest_config";

        #endregion

        #region Private Fields

        private static IVXQuestManager _instance;
        private IVXHiroRpcClient _rpcClient;
        private bool _initialized;
        private List<IVXQuest> _cachedQuests;
        private QuestConfig _cachedConfig;
        private DateTime _lastRefresh;

        #endregion

        #region Properties

        public static IVXQuestManager Instance => _instance;
        public bool IsInitialized => _initialized;
        public IReadOnlyList<IVXQuest> CachedQuests => _cachedQuests;
        public QuestConfig Config => _cachedConfig;

        #endregion

        #region Events

        public event Action<List<IVXQuest>> OnQuestsRefreshed;
        public event Action<QuestProgressResult> OnQuestProgress;
        public event Action<string> OnQuestCompleted;
        public event Action<IVXQuestClaimResult> OnQuestRewardClaimed;
        public event Action<string> OnQuestError;

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

        public void Initialize(IClient client, ISession session)
        {
            if (client == null) throw new ArgumentNullException(nameof(client));
            if (session == null) throw new ArgumentNullException(nameof(session));

            _rpcClient = new IVXHiroRpcClient(client, session);
            _cachedQuests = new List<IVXQuest>();
            _initialized = true;

            Debug.Log($"{LOG_TAG} Initialized.");
        }

        public void RefreshSession(ISession session)
        {
            _rpcClient?.UpdateSession(session);
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Fetch all active quests for the authenticated user.
        /// Optional <paramref name="questType"/> filter ("daily", "weekly", etc.).
        /// </summary>
        public async Task<List<IVXQuest>> GetQuestsAsync(string questType = null)
        {
            EnsureReady();

            object payload = string.IsNullOrEmpty(questType)
                ? (object)new { }
                : new { quest_type = questType };

            var response = await _rpcClient.CallAsync<QuestsResponse>(RPC_GET_QUESTS, payload);

            if (response.success && response.data != null)
            {
                _cachedQuests = response.data.quests ?? new List<IVXQuest>();
                _lastRefresh = DateTime.UtcNow;
                OnQuestsRefreshed?.Invoke(_cachedQuests);
                return _cachedQuests;
            }

            var error = $"GetQuests failed: {response.error}";
            Debug.LogWarning($"{LOG_TAG} {error}");
            OnQuestError?.Invoke(error);
            return new List<IVXQuest>();
        }

        /// <summary>
        /// Report a game event that may advance one or more quests.
        /// </summary>
        public async Task<QuestProgressResult> ReportEventAsync(GameEvent gameEvent)
        {
            EnsureReady();

            if (gameEvent == null) throw new ArgumentNullException(nameof(gameEvent));
            if (string.IsNullOrEmpty(gameEvent.eventName))
                throw new ArgumentException("eventName is required", nameof(gameEvent));

            var response = await _rpcClient.CallAsync<QuestProgressResult>(
                RPC_PROGRESS,
                new
                {
                    event_name = gameEvent.eventName,
                    value = gameEvent.value,
                    metadata = gameEvent.metadata
                });

            if (response.success && response.data != null)
            {
                OnQuestProgress?.Invoke(response.data);

                if (response.data.newlyCompleted)
                    OnQuestCompleted?.Invoke(response.data.questId);

                UpdateCachedQuest(response.data);
                return response.data;
            }

            var error = $"ReportEvent failed for {gameEvent.eventName}: {response.error}";
            Debug.LogWarning($"{LOG_TAG} {error}");
            OnQuestError?.Invoke(error);
            return null;
        }

        /// <summary>
        /// Report a simple event by name and value.
        /// </summary>
        public Task<QuestProgressResult> ReportEventAsync(string eventName, int value = 1)
        {
            return ReportEventAsync(new GameEvent { eventName = eventName, value = value });
        }

        /// <summary>
        /// Claim the reward for a completed quest.
        /// </summary>
        public async Task<IVXQuestClaimResult> ClaimRewardAsync(string questId)
        {
            EnsureReady();

            if (string.IsNullOrEmpty(questId))
                throw new ArgumentNullException(nameof(questId));

            var response = await _rpcClient.CallAsync<IVXQuestClaimResult>(
                RPC_CLAIM,
                new { quest_id = questId });

            if (response.success && response.data != null)
            {
                Debug.Log($"{LOG_TAG} Claimed reward for quest: {questId}");
                OnQuestRewardClaimed?.Invoke(response.data);
                MarkQuestClaimed(questId);
                return response.data;
            }

            var error = $"ClaimReward failed for {questId}: {response.error}";
            Debug.LogWarning($"{LOG_TAG} {error}");
            OnQuestError?.Invoke(error);
            return null;
        }

        /// <summary>
        /// Fetch the quest configuration (mappings, limits, refresh interval).
        /// </summary>
        public async Task<QuestConfig> GetConfigAsync()
        {
            EnsureReady();

            var response = await _rpcClient.CallAsync<QuestConfig>(RPC_GET_CONFIG);

            if (response.success && response.data != null)
            {
                _cachedConfig = response.data;
                return _cachedConfig;
            }

            Debug.LogWarning($"{LOG_TAG} GetConfig failed: {response.error}");
            return null;
        }

        /// <summary>
        /// Get quests filtered by status from the local cache.
        /// Call <see cref="GetQuestsAsync"/> first to populate.
        /// </summary>
        public List<IVXQuest> GetQuestsByStatus(QuestStatus status)
        {
            if (_cachedQuests == null) return new List<IVXQuest>();
            return _cachedQuests.FindAll(q => q.StatusEnum == status);
        }

        /// <summary>
        /// Get quests filtered by type from the local cache.
        /// </summary>
        public List<IVXQuest> GetQuestsByType(QuestType type)
        {
            if (_cachedQuests == null) return new List<IVXQuest>();
            var typeName = type.ToString().ToLowerInvariant();
            return _cachedQuests.FindAll(q =>
                string.Equals(q.questType, typeName, StringComparison.OrdinalIgnoreCase));
        }

        #endregion

        #region Helpers

        private void EnsureReady()
        {
            if (!_initialized || _rpcClient == null)
                throw new InvalidOperationException($"{LOG_TAG} Not initialized. Call Initialize() first.");
        }

        private void UpdateCachedQuest(QuestProgressResult result)
        {
            if (_cachedQuests == null || result == null) return;

            var quest = _cachedQuests.Find(q => q.questId == result.questId);
            if (quest != null)
            {
                quest.currentProgress = result.currentProgress;
                if (result.completed)
                    quest.status = QuestStatus.Completed.ToString().ToLowerInvariant();
            }
        }

        private void MarkQuestClaimed(string questId)
        {
            if (_cachedQuests == null) return;

            var quest = _cachedQuests.Find(q => q.questId == questId);
            if (quest != null)
                quest.status = QuestStatus.Claimed.ToString().ToLowerInvariant();
        }

        #endregion
    }
}
