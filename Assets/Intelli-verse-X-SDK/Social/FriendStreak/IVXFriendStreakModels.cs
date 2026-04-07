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
    /// Represents an interaction streak with a friend.
    /// </summary>
    [Serializable]
    public class IVXFriendStreak
    {
        [JsonProperty("friend_id")] public string friendId;
        [JsonProperty("friend_name")] public string friendName;
        [JsonProperty("current_streak")] public int currentStreak;
        [JsonProperty("longest_streak")] public int longestStreak;
        [JsonProperty("last_interaction_at")] public string lastInteractionAt;
        [JsonProperty("streak_expires_at")] public string streakExpiresAt;
    }

    /// <summary>
    /// A cooperative quest completed with friends.
    /// </summary>
    [Serializable]
    public class IVXFriendQuest
    {
        [JsonProperty("quest_id")] public string questId;
        [JsonProperty("title")] public string title;
        [JsonProperty("description")] public string description;
        [JsonProperty("target_progress")] public int targetProgress;
        [JsonProperty("current_progress")] public int currentProgress;
        [JsonProperty("completed")] public bool completed;
        [JsonProperty("expires_at")] public string expiresAt;
        [JsonProperty("reward")] public IVXFriendQuestReward reward;
    }

    /// <summary>
    /// Response wrapper for friend streaks.
    /// </summary>
    [Serializable]
    public class IVXFriendStreakListResponse
    {
        [JsonProperty("streaks")] public List<IVXFriendStreak> streaks;
    }

    /// <summary>
    /// Response wrapper for active friend quests.
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
        [JsonProperty("friend_id")] public string friendId;
    }

    /// <summary>
    /// Request payload for contributing to a friend quest.
    /// </summary>
    [Serializable]
    public class IVXFriendQuestContributeRequest
    {
        [JsonProperty("quest_id")] public string questId;
        [JsonProperty("progress")] public int progress;
    }
}
