using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace IntelliVerseX.Monetization
{
    /// <summary>
    /// A single reward on the season pass track.
    /// </summary>
    [Serializable]
    public class IVXSeasonPassReward
    {
        [JsonProperty("level")] public int level;
        [JsonProperty("reward_type")] public string rewardType;
        [JsonProperty("amount")] public int amount;
        [JsonProperty("item_id")] public string itemId;
        [JsonProperty("claimed")] public bool claimed;
        [JsonProperty("is_premium_only")] public bool isPremiumOnly;
    }

    /// <summary>
    /// Current state of the season pass for a player.
    /// </summary>
    [Serializable]
    public class IVXSeasonPassState
    {
        [JsonProperty("season_id")] public string seasonId;
        [JsonProperty("current_level")] public int currentLevel;
        [JsonProperty("current_xp")] public int currentXp;
        [JsonProperty("xp_to_next_level")] public int xpToNextLevel;
        [JsonProperty("is_premium")] public bool isPremium;
        [JsonProperty("free_rewards")] public List<IVXSeasonPassReward> freeRewards;
        [JsonProperty("premium_rewards")] public List<IVXSeasonPassReward> premiumRewards;
        [JsonProperty("ends_at")] public string endsAt;
    }

    /// <summary>
    /// Response wrapper for season pass state.
    /// </summary>
    [Serializable]
    public class IVXSeasonPassStateResponse
    {
        [JsonProperty("state")] public IVXSeasonPassState state;
    }

    /// <summary>
    /// Request payload for claiming a season pass reward.
    /// </summary>
    [Serializable]
    public class IVXSeasonPassClaimRequest
    {
        [JsonProperty("level")] public int level;
        [JsonProperty("is_premium_track")] public bool isPremiumTrack;
    }

    /// <summary>
    /// Request payload for adding XP to the season pass.
    /// </summary>
    [Serializable]
    public class IVXSeasonPassXpRequest
    {
        [JsonProperty("amount")] public int amount;
    }
}
