using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace IntelliVerseX.Missions
{
    /// <summary>
    /// A single daily mission with progress tracking.
    /// </summary>
    [Serializable]
    public class DailyMission
    {
        /// <summary>Unique mission identifier.</summary>
        [JsonProperty("mission_id")] public string missionId;

        /// <summary>Localized mission title.</summary>
        [JsonProperty("title")] public string title;

        /// <summary>Localized mission description.</summary>
        [JsonProperty("description")] public string description;

        /// <summary>Progress value required to complete the mission.</summary>
        [JsonProperty("target_progress")] public int targetProgress;

        /// <summary>User's current progress towards the target.</summary>
        [JsonProperty("current_progress")] public int currentProgress;

        /// <summary>Reward granted upon completion.</summary>
        [JsonProperty("reward")] public MissionReward reward;

        /// <summary>Whether the mission target has been reached.</summary>
        [JsonProperty("completed")] public bool completed;

        /// <summary>Whether the reward has been claimed.</summary>
        [JsonProperty("claimed")] public bool claimed;

        /// <summary>Optional icon identifier for UI display.</summary>
        [JsonProperty("icon")] public string icon;
    }

    /// <summary>
    /// Reward descriptor attached to a daily mission.
    /// </summary>
    [Serializable]
    public class MissionReward
    {
        /// <summary>Type of reward (e.g. "coins", "gems", "xp", "item").</summary>
        [JsonProperty("reward_type")] public string rewardType;

        /// <summary>Quantity of the reward.</summary>
        [JsonProperty("amount")] public int amount;

        /// <summary>Currency or item identifier.</summary>
        [JsonProperty("currency_id")] public string currencyId;
    }

    /// <summary>
    /// Server response containing the list of today's daily missions.
    /// </summary>
    [Serializable]
    public class DailyMissionsResponse
    {
        /// <summary>Today's missions.</summary>
        [JsonProperty("missions")] public List<DailyMission> missions;

        /// <summary>ISO 8601 timestamp when missions reset.</summary>
        [JsonProperty("resets_at")] public string resetsAt;
    }

    /// <summary>
    /// Result of a mission progress update.
    /// </summary>
    [Serializable]
    public class MissionProgressResult
    {
        /// <summary>Updated mission identifier.</summary>
        [JsonProperty("mission_id")] public string missionId;

        /// <summary>Updated current progress.</summary>
        [JsonProperty("current_progress")] public int currentProgress;

        /// <summary>Whether the mission is now completed.</summary>
        [JsonProperty("completed")] public bool completed;
    }

    /// <summary>
    /// Result of claiming a mission reward.
    /// </summary>
    [Serializable]
    public class MissionClaimResult
    {
        /// <summary>Claimed mission identifier.</summary>
        [JsonProperty("mission_id")] public string missionId;

        /// <summary>Reward that was granted.</summary>
        [JsonProperty("reward")] public MissionReward reward;

        /// <summary>Whether the claim was successful.</summary>
        [JsonProperty("claimed")] public bool claimed;
    }
}
