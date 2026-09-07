// Copyright (c) 2026 Intelli-verse-X
// MIT License — see LICENSE in the project root.

using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace IntelliVerseX.Quest
{
    public enum QuestType
    {
        Daily,
        Weekly,
        Challenge,
        Event,
        Achievement
    }

    public enum QuestStatus
    {
        Locked,
        Active,
        InProgress,
        Completed,
        Claimed,
        Expired
    }

    [Serializable]
    public class QuestReward
    {
        [JsonProperty("reward_type")] public string rewardType;
        [JsonProperty("amount")] public int amount;
        [JsonProperty("currency_id")] public string currencyId;
        [JsonProperty("item_id")] public string itemId;
    }

    [Serializable]
    public class IVXQuest
    {
        [JsonProperty("quest_id")] public string questId;
        [JsonProperty("title")] public string title;
        [JsonProperty("description")] public string description;
        [JsonProperty("quest_type")] public string questType;
        [JsonProperty("status")] public string status;
        [JsonProperty("target_progress")] public int targetProgress;
        [JsonProperty("current_progress")] public int currentProgress;
        [JsonProperty("rewards")] public List<QuestReward> rewards;
        [JsonProperty("icon")] public string icon;
        [JsonProperty("expires_at")] public string expiresAt;
        [JsonProperty("unlock_conditions")] public Dictionary<string, object> unlockConditions;

        public QuestType QuestTypeEnum =>
            Enum.TryParse(questType, true, out QuestType t) ? t : QuestType.Daily;

        public QuestStatus StatusEnum =>
            Enum.TryParse(status, true, out QuestStatus s) ? s : QuestStatus.Locked;

        public bool IsComplete => currentProgress >= targetProgress;
        public float ProgressNormalized => targetProgress > 0 ? (float)currentProgress / targetProgress : 0f;
    }

    [Serializable]
    public class GameEvent
    {
        [JsonProperty("event_name")] public string eventName;
        [JsonProperty("value")] public int value;
        [JsonProperty("metadata")] public Dictionary<string, string> metadata;
    }

    [Serializable]
    public class QuestConfig
    {
        [JsonProperty("quests")] public List<IVXQuest> quests;
        [JsonProperty("refresh_interval_sec")] public int refreshIntervalSec;
        [JsonProperty("max_active_quests")] public int maxActiveQuests;
        [JsonProperty("event_mappings")] public Dictionary<string, List<string>> eventMappings;
    }

    [Serializable]
    public class QuestsResponse
    {
        [JsonProperty("quests")] public List<IVXQuest> quests;
        [JsonProperty("resets_at")] public string resetsAt;
        [JsonProperty("config_version")] public int configVersion;
    }

    [Serializable]
    public class QuestProgressResult
    {
        [JsonProperty("quest_id")] public string questId;
        [JsonProperty("current_progress")] public int currentProgress;
        [JsonProperty("completed")] public bool completed;
        [JsonProperty("newly_completed")] public bool newlyCompleted;
    }

    [Serializable]
    public class IVXQuestClaimResult
    {
        [JsonProperty("quest_id")] public string questId;
        [JsonProperty("rewards")] public List<QuestReward> rewards;
        [JsonProperty("claimed")] public bool claimed;
        [JsonProperty("wallet_update")] public Dictionary<string, long> walletUpdate;
    }
}
