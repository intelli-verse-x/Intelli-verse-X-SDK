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
    /// Represents a collectible badge (aligned to prod <c>badges_get_all</c>).
    /// </summary>
    [Serializable]
    public class IVXBadge
    {
        [JsonProperty("badge_id")] public string badgeId;

        /// <summary>Prod uses <c>title</c>; legacy used <c>name</c>.</summary>
        [JsonProperty("title")] public string title;
        [JsonProperty("name")] public string name;

        [JsonProperty("description")] public string description;
        [JsonProperty("icon_url")] public string iconUrl;
        [JsonProperty("tier")] public string tier;
        [JsonProperty("rarity")] public string rarity;
        [JsonProperty("category")] public string category;
        [JsonProperty("unlocked")] public bool unlocked;
        [JsonProperty("displayed")] public bool displayed;
        [JsonProperty("equipped_at")] public string equippedAt;
        [JsonProperty("progress")] public int progress;
        [JsonProperty("target")] public int target;
        [JsonProperty("points")] public int points;

        [JsonIgnore]
        public string DisplayName => !string.IsNullOrEmpty(title) ? title : name;

        [JsonIgnore]
        public string EffectiveTier => !string.IsNullOrEmpty(rarity) ? rarity : tier;

        [JsonIgnore]
        public bool IsEquipped => displayed || !string.IsNullOrEmpty(equippedAt);
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
        [JsonProperty("badgeId")] public string badgeIdCamel;
    }
}
