using System;
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
    /// Current retention state for a player (aligned to prod <c>hiro_retention_get</c>).
    /// </summary>
    [Serializable]
    public class IVXRetentionState
    {
        /// <summary>Prod bucket: active / at_risk / lapsed / churned.</summary>
        [JsonProperty("bucket")] public string bucket;

        /// <summary>ISO timestamp of last seen / heartbeat.</summary>
        [JsonProperty("lastSeen")] public string lastSeen;

        [JsonProperty("days_played")] public int daysPlayed;
        [JsonProperty("consecutive_days")] public int consecutiveDays;
        [JsonProperty("last_login_at")] public string lastLoginAt;
        [JsonProperty("risk_level")] public string riskLevel;
        [JsonProperty("segment")] public string segment;
        [JsonProperty("comebackBonusAvailable")] public bool comebackBonusAvailable;

        /// <summary>Prefer prod <see cref="bucket"/>, fall back to legacy risk_level.</summary>
        [JsonIgnore]
        public string EffectiveRisk =>
            !string.IsNullOrEmpty(bucket) ? bucket :
            !string.IsNullOrEmpty(riskLevel) ? riskLevel :
            segment;

        /// <summary>Prefer prod <see cref="lastSeen"/>.</summary>
        [JsonIgnore]
        public string EffectiveLastLogin =>
            !string.IsNullOrEmpty(lastSeen) ? lastSeen : lastLoginAt;
    }

    /// <summary>
    /// Response wrapper for retention state (legacy nested shape).
    /// </summary>
    [Serializable]
    public class IVXRetentionStateResponse
    {
        [JsonProperty("state")] public IVXRetentionState state;
    }

    /// <summary>
    /// Reward associated with a winback / return bonus offer.
    /// </summary>
    [Serializable]
    public class IVXWinbackReward
    {
        [JsonProperty("type")] public string type;
        [JsonProperty("amount")] public int amount;
        [JsonProperty("item_id")] public string itemId;
        [JsonProperty("coins")] public int coins;
        [JsonProperty("xp")] public int xp;
    }

    /// <summary>
    /// A winback / return-bonus offer (prod: <c>hiro_incentives_return_bonus</c> /
    /// <c>hiro_retention_claim_comeback</c>).
    /// </summary>
    [Serializable]
    public class IVXWinbackOffer
    {
        [JsonProperty("offer_id")] public string offerId;
        [JsonProperty("title")] public string title;
        [JsonProperty("description")] public string description;
        [JsonProperty("reward")] public IVXWinbackReward reward;
        [JsonProperty("expires_at")] public string expiresAt;
        [JsonProperty("available")] public bool available;
        [JsonProperty("claimed")] public bool claimed;
        [JsonProperty("success")] public bool success;
    }

    /// <summary>
    /// Request payload for claiming a winback / comeback offer.
    /// </summary>
    [Serializable]
    public class IVXWinbackClaimRequest
    {
        [JsonProperty("offer_id")] public string offerId;
        [JsonProperty("offerId")] public string offerIdCamel;
    }
}
