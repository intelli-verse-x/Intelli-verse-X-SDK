using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace IntelliVerseX.Progression
{
    /// <summary>
    /// Tier rarity for badges.
    /// </summary>
    public enum BadgeTier
    {
        Common,
        Uncommon,
        Rare,
        Epic,
        Legendary,
        Seasonal
    }

    /// <summary>
    /// Category grouping for badges.
    /// </summary>
    public enum BadgeCategory
    {
        Tiered,
        Achievement,
        Legendary,
        Seasonal
    }

    /// <summary>
    /// Represents a collectible badge.
    /// </summary>
    [Serializable]
    public class IVXBadge
    {
        [JsonProperty("badge_id")] public string badgeId;
        [JsonProperty("name")] public string name;
        [JsonProperty("description")] public string description;
        [JsonProperty("icon_url")] public string iconUrl;
        [JsonProperty("tier")] public string tier;
        [JsonProperty("category")] public string category;
        [JsonProperty("unlocked")] public bool unlocked;
        [JsonProperty("equipped_at")] public string equippedAt;
    }

    /// <summary>
    /// Response wrapper for listing all badges.
    /// </summary>
    [Serializable]
    public class IVXBadgeListResponse
    {
        [JsonProperty("badges")] public List<IVXBadge> badges;
    }

    /// <summary>
    /// Request payload for badge operations.
    /// </summary>
    [Serializable]
    public class IVXBadgeRequest
    {
        [JsonProperty("badge_id")] public string badgeId;
    }
}
