using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace IntelliVerseX.Progression
{
    /// <summary>
    /// Category grouping for achievements.
    /// </summary>
    public enum AchievementCategory
    {
        General,
        Social,
        Competitive,
        Collection,
        Mastery,
        Exploration,
        Seasonal
    }

    /// <summary>
    /// Reward granted when an achievement is completed.
    /// </summary>
    [Serializable]
    public class IVXAchievementReward
    {
        [JsonProperty("type")] public string type;
        [JsonProperty("amount")] public int amount;
        [JsonProperty("item_id")] public string itemId;
    }

    /// <summary>
    /// Represents a single trackable achievement.
    /// </summary>
    [Serializable]
    public class IVXAchievement
    {
        [JsonProperty("id")] public string id;
        [JsonProperty("title")] public string title;
        [JsonProperty("description")] public string description;
        [JsonProperty("category")] public string category;
        [JsonProperty("target_progress")] public int targetProgress;
        [JsonProperty("current_progress")] public int currentProgress;
        [JsonProperty("unlocked")] public bool unlocked;
        [JsonProperty("reward_claimed")] public bool rewardClaimed;
        [JsonProperty("reward")] public IVXAchievementReward reward;
    }

    /// <summary>
    /// Response wrapper for listing all achievements.
    /// </summary>
    [Serializable]
    public class IVXAchievementListResponse
    {
        [JsonProperty("achievements")] public List<IVXAchievement> achievements;
    }

    /// <summary>
    /// Request payload for tracking achievement progress.
    /// </summary>
    [Serializable]
    public class IVXAchievementProgressRequest
    {
        [JsonProperty("achievement_id")] public string achievementId;
        [JsonProperty("progress")] public int progress;
    }

    /// <summary>
    /// Request payload for claiming an achievement reward.
    /// </summary>
    [Serializable]
    public class IVXAchievementClaimRequest
    {
        [JsonProperty("achievement_id")] public string achievementId;
    }
}
