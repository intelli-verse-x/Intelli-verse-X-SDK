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

    /// <summary>
    /// Bridge from prod <c>daily_missions_get</c> into weekly goal models
    /// (prod has no <c>weekly_goals_get</c>).
    /// </summary>
    [Serializable]
    public class IVXDailyMissionsGoalsBridge
    {
        [JsonProperty("missions")] public List<IVXDailyMissionGoalRow> missions;

        public List<IVXWeeklyGoal> ToWeeklyGoals()
        {
            var list = new List<IVXWeeklyGoal>();
            if (missions == null) return list;
            foreach (var m in missions)
            {
                if (m == null) continue;
                list.Add(new IVXWeeklyGoal
                {
                    goalId = m.missionId,
                    title = m.title,
                    description = m.description,
                    targetProgress = m.targetProgress,
                    currentProgress = m.currentProgress,
                    completed = m.completed,
                    reward = m.reward == null
                        ? null
                        : new IVXGoalReward
                        {
                            type = m.reward.rewardType,
                            amount = m.reward.amount,
                            itemId = m.reward.currencyId
                        }
                });
            }
            return list;
        }
    }

    [Serializable]
    public class IVXDailyMissionGoalRow
    {
        [JsonProperty("mission_id")] public string missionId;
        [JsonProperty("title")] public string title;
        [JsonProperty("description")] public string description;
        [JsonProperty("target_progress")] public int targetProgress;
        [JsonProperty("current_progress")] public int currentProgress;
        [JsonProperty("completed")] public bool completed;
        [JsonProperty("reward")] public IVXDailyMissionGoalReward reward;
    }

    [Serializable]
    public class IVXDailyMissionGoalReward
    {
        [JsonProperty("reward_type")] public string rewardType;
        [JsonProperty("amount")] public int amount;
        [JsonProperty("currency_id")] public string currencyId;
    }
}
