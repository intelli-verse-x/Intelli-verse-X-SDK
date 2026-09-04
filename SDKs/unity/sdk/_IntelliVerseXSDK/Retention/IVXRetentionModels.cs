using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace IntelliVerseX.Retention
{
    /// <summary>
    /// Player risk level for churn prediction.
    /// </summary>
    public enum RiskLevel
    {
        Active,
        AtRisk,
        Lapsed,
        Churned
    }

    /// <summary>
    /// Current retention state for a player.
    /// </summary>
    [Serializable]
    public class IVXRetentionState
    {
        [JsonProperty("days_played")] public int daysPlayed;
        [JsonProperty("consecutive_days")] public int consecutiveDays;
        [JsonProperty("last_login_at")] public string lastLoginAt;
        [JsonProperty("risk_level")] public string riskLevel;
        [JsonProperty("segment")] public string segment;
    }

    /// <summary>
    /// Response wrapper for retention state.
    /// </summary>
    [Serializable]
    public class IVXRetentionStateResponse
    {
        [JsonProperty("state")] public IVXRetentionState state;
    }

    /// <summary>
    /// Reward associated with a winback offer.
    /// </summary>
    [Serializable]
    public class IVXWinbackReward
    {
        [JsonProperty("type")] public string type;
        [JsonProperty("amount")] public int amount;
        [JsonProperty("item_id")] public string itemId;
    }

    /// <summary>
    /// A winback offer presented to lapsed or at-risk players.
    /// </summary>
    [Serializable]
    public class IVXWinbackOffer
    {
        [JsonProperty("offer_id")] public string offerId;
        [JsonProperty("title")] public string title;
        [JsonProperty("description")] public string description;
        [JsonProperty("reward")] public IVXWinbackReward reward;
        [JsonProperty("expires_at")] public string expiresAt;
    }

    /// <summary>
    /// Request payload for claiming a winback offer.
    /// </summary>
    [Serializable]
    public class IVXWinbackClaimRequest
    {
        [JsonProperty("offer_id")] public string offerId;
    }
}
