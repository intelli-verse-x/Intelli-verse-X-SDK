using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace IntelliVerseX.Monetization
{
    /// <summary>
    /// Current fortune-wheel state for the authenticated user.
    /// </summary>
    [Serializable]
    public class FortuneWheelState
    {
        /// <summary>Number of paid spins available.</summary>
        [JsonProperty("available_spins")] public int availableSpins;

        /// <summary>Remaining free spins for the current period.</summary>
        [JsonProperty("free_spins_remaining")] public int freeSpinsRemaining;

        /// <summary>ISO 8601 timestamp when the next free spin becomes available.</summary>
        [JsonProperty("next_free_spin_at")] public string nextFreeSpinAt;

        /// <summary>Result of the most recent spin (null if no spin yet).</summary>
        [JsonProperty("last_spin_result")] public SpinResult lastSpinResult;
    }

    /// <summary>
    /// A single segment on the fortune wheel.
    /// </summary>
    [Serializable]
    public class FortuneWheelSegment
    {
        /// <summary>Unique segment identifier.</summary>
        [JsonProperty("segment_id")] public string segmentId;

        /// <summary>Type of reward (e.g. "coins", "gems", "item", "jackpot").</summary>
        [JsonProperty("reward_type")] public string rewardType;

        /// <summary>Quantity of the reward.</summary>
        [JsonProperty("reward_amount")] public int rewardAmount;

        /// <summary>Server-side probability weight (0.0 – 1.0).</summary>
        [JsonProperty("probability")] public float probability;

        /// <summary>Hex color code for the UI segment.</summary>
        [JsonProperty("color")] public string color;

        /// <summary>Display label for the segment.</summary>
        [JsonProperty("label")] public string label;
    }

    /// <summary>
    /// Configuration for a fortune wheel instance.
    /// </summary>
    [Serializable]
    public class FortuneWheelConfig
    {
        /// <summary>Unique wheel identifier.</summary>
        [JsonProperty("wheel_id")] public string wheelId;

        /// <summary>Ordered list of wheel segments.</summary>
        [JsonProperty("segments")] public List<FortuneWheelSegment> segments;

        /// <summary>Cost of a single spin in the wheel's currency.</summary>
        [JsonProperty("spin_cost")] public int spinCost;

        /// <summary>Interval in seconds between free spins.</summary>
        [JsonProperty("free_spin_interval")] public int freeSpinInterval;

        /// <summary>Currency used for spin cost (e.g. "coins", "gems").</summary>
        [JsonProperty("spin_currency")] public string spinCurrency;
    }

    /// <summary>
    /// Result of a single fortune-wheel spin.
    /// </summary>
    [Serializable]
    public class SpinResult
    {
        /// <summary>Segment the wheel landed on.</summary>
        [JsonProperty("segment_id")] public string segmentId;

        /// <summary>Type of reward received.</summary>
        [JsonProperty("reward_type")] public string rewardType;

        /// <summary>Amount of the reward received.</summary>
        [JsonProperty("reward_amount")] public int rewardAmount;

        /// <summary>Arbitrary metadata (bonus flags, animation hints, etc.).</summary>
        [JsonProperty("metadata")] public Dictionary<string, string> metadata;
    }
}
