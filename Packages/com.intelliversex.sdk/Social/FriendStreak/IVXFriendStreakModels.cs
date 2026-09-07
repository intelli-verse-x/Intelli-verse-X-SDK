using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace IntelliVerseX.Social
{
    /// <summary>
    /// Reward granted upon completing a friend quest.
    /// </summary>
    [Serializable]
    public class IVXFriendQuestReward
    {
        [JsonProperty("type")] public string type;
        [JsonProperty("amount")] public int amount;
        [JsonProperty("item_id")] public string itemId;
    }

    /// <summary>
    /// Represents an interaction streak with a friend (prod: <c>friend_streak_get_state</c>).
    /// </summary>
    [Serializable]
    public class IVXFriendStreak
    {
        [JsonProperty("friendId")] public string friendId;
        [JsonProperty("friendDisplayName")] public string friendName;
        [JsonProperty("streakDays")] public int currentStreak;
        [JsonProperty("longest_streak")] public int longestStreak;
        [JsonProperty("lastInteractionAt")] public string lastInteractionAt;
        [JsonProperty("hoursUntilBreak")] public float hoursRemaining;
        [JsonProperty("myContributionToday")] public bool myContributionToday;
        [JsonProperty("friendContributionToday")] public bool friendContributionToday;
        [JsonProperty("isAtRisk")] public bool isAtRisk;
    }

    /// <summary>
    /// A cooperative quest completed with friends (prod: <c>friend_quest_get_state</c>).
    /// </summary>
    [Serializable]
    public class IVXFriendQuest
    {
        [JsonProperty("id")] public string questId;
        [JsonProperty("title")] public string title;
        [JsonProperty("description")] public string description;
        [JsonProperty("type")] public string type;
        [JsonProperty("target")] public int targetProgress;
        [JsonProperty("progress")] public int currentProgress;
        [JsonProperty("completed")] public bool completed;
        [JsonProperty("completedAt")] public string completedAt;
        [JsonProperty("expiresAt")] public string expiresAt;
        [JsonProperty("reward")] public IVXFriendQuestReward reward;
    }

    /// <summary>
    /// Response wrapper for friend streaks (prod: <c>friend_streak_get_state</c>).
    /// </summary>
    [Serializable]
    public class IVXFriendStreakListResponse
    {
        [JsonProperty("streaks")] public List<IVXFriendStreak> streaks;
    }

    /// <summary>
    /// Response wrapper for active friend quests (prod: <c>friend_quest_get_state</c>).
    /// </summary>
    [Serializable]
    public class IVXFriendQuestListResponse
    {
        [JsonProperty("quests")] public List<IVXFriendQuest> quests;
    }

    /// <summary>
    /// Request payload for recording a friend interaction.
    /// </summary>
    [Serializable]
    public class IVXFriendInteractionRequest
    {
        [JsonProperty("friendId")] public string friendId;
    }

    /// <summary>
    /// Request payload for completing a friend quest.
    /// </summary>
    [Serializable]
    public class IVXFriendQuestContributeRequest
    {
        [JsonProperty("questId")] public string questId;
        [JsonProperty("quest_id")] public string quest_id;
    }
}
