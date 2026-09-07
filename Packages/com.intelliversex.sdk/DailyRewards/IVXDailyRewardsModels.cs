using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace IntelliVerseX.DailyRewards
{
    /// <summary>
    /// Current daily-reward state for the authenticated user.
    /// </summary>
    [Serializable]
    public class DailyRewardState
    {
        /// <summary>The current reward day (1-based).</summary>
        [JsonProperty("current_day")] public int currentDay;

        /// <summary>ISO 8601 date of the last successful claim.</summary>
        [JsonProperty("last_claim_date")] public string lastClaimDate;

        /// <summary>Current consecutive-claim streak.</summary>
        [JsonProperty("streak")] public int streak;

        /// <summary>Whether the user can claim today's reward right now.</summary>
        [JsonProperty("can_claim")] public bool canClaim;

        /// <summary>ISO 8601 timestamp when the next claim becomes available.</summary>
        [JsonProperty("next_claim_at")] public string nextClaimAt;
    }

    /// <summary>
    /// A single day entry within the daily-reward calendar.
    /// </summary>
    [Serializable]
    public class DailyRewardCalendarDay
    {
        /// <summary>Calendar day number (1-based).</summary>
        [JsonProperty("day")] public int day;

        /// <summary>Reward granted on this day.</summary>
        [JsonProperty("reward")] public DailyReward reward;

        /// <summary>Whether the user has already claimed this day.</summary>
        [JsonProperty("claimed")] public bool claimed;

        /// <summary>Streak-based bonus multiplier for this day.</summary>
        [JsonProperty("bonus_multiplier")] public float bonusMultiplier;
    }

    /// <summary>
    /// Reward descriptor used within a calendar day.
    /// </summary>
    [Serializable]
    public class DailyReward
    {
        /// <summary>Type of reward (e.g. "coins", "gems", "item").</summary>
        [JsonProperty("reward_type")] public string rewardType;

        /// <summary>Quantity of the reward.</summary>
        [JsonProperty("amount")] public int amount;

        /// <summary>Currency or item identifier.</summary>
        [JsonProperty("currency_id")] public string currencyId;
    }

    /// <summary>
    /// Full daily-reward calendar returned by the server.
    /// </summary>
    [Serializable]
    public class DailyRewardCalendar
    {
        /// <summary>Ordered list of calendar day entries.</summary>
        [JsonProperty("days")] public List<DailyRewardCalendarDay> days;

        /// <summary>The user's current day in the calendar.</summary>
        [JsonProperty("current_day")] public int currentDay;

        /// <summary>Total days in the calendar cycle.</summary>
        [JsonProperty("total_days")] public int totalDays;
    }

    /// <summary>
    /// Result returned after successfully claiming a daily reward.
    /// </summary>
    [Serializable]
    public class DailyRewardClaimResult
    {
        /// <summary>The day that was claimed.</summary>
        [JsonProperty("day")] public int day;

        /// <summary>Reward that was granted.</summary>
        [JsonProperty("reward")] public DailyReward reward;

        /// <summary>Updated streak count after the claim.</summary>
        [JsonProperty("streak")] public int streak;

        /// <summary>Bonus multiplier applied.</summary>
        [JsonProperty("bonus_multiplier")] public float bonusMultiplier;
    }
}
