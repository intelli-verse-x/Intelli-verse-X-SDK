using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace IntelliVerseX.Retention
{
    /// <summary>
    /// Reward granted upon completing a goal or milestone.
    /// </summary>
    [Serializable]
    public class IVXGoalReward
    {
        [JsonProperty("type")] public string type;
        [JsonProperty("amount")] public int amount;
        [JsonProperty("item_id")] public string itemId;
    }

    /// <summary>
    /// A weekly goal with progress tracking and expiration.
    /// </summary>
    [Serializable]
    public class IVXWeeklyGoal
    {
        [JsonProperty("goal_id")] public string goalId;
        [JsonProperty("title")] public string title;
        [JsonProperty("description")] public string description;
        [JsonProperty("target_progress")] public int targetProgress;
        [JsonProperty("current_progress")] public int currentProgress;
        [JsonProperty("reward")] public IVXGoalReward reward;
        [JsonProperty("completed")] public bool completed;
        [JsonProperty("expires_at")] public string expiresAt;
    }

    /// <summary>
    /// A monthly milestone with progress tracking.
    /// </summary>
    [Serializable]
    public class IVXMonthlyMilestone
    {
        [JsonProperty("milestone_id")] public string milestoneId;
        [JsonProperty("title")] public string title;
        [JsonProperty("description")] public string description;
        [JsonProperty("target_progress")] public int targetProgress;
        [JsonProperty("current_progress")] public int currentProgress;
        [JsonProperty("reward")] public IVXGoalReward reward;
        [JsonProperty("completed")] public bool completed;
        [JsonProperty("month")] public string month;
    }

    /// <summary>
    /// Response wrapper for weekly goals.
    /// </summary>
    [Serializable]
    public class IVXWeeklyGoalsResponse
    {
        [JsonProperty("goals")] public List<IVXWeeklyGoal> goals;
    }

    /// <summary>
    /// Response wrapper for monthly milestones.
    /// </summary>
    [Serializable]
    public class IVXMonthlyMilestonesResponse
    {
        [JsonProperty("milestones")] public List<IVXMonthlyMilestone> milestones;
    }

    /// <summary>
    /// Request payload for updating weekly goal progress.
    /// </summary>
    [Serializable]
    public class IVXWeeklyGoalProgressRequest
    {
        [JsonProperty("goal_id")] public string goalId;
        [JsonProperty("progress")] public int progress;
    }

    /// <summary>
    /// Request payload for updating monthly milestone progress.
    /// </summary>
    [Serializable]
    public class IVXMonthlyMilestoneProgressRequest
    {
        [JsonProperty("milestone_id")] public string milestoneId;
        [JsonProperty("progress")] public int progress;
    }
}
