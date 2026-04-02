using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace IntelliVerseX.Competition
{
    /// <summary>
    /// Ranked league tier progression.
    /// </summary>
    public enum LeagueTier
    {
        /// <summary>Starting tier.</summary>
        Bronze = 0,

        /// <summary>Second tier.</summary>
        Silver = 1,

        /// <summary>Third tier.</summary>
        Gold = 2,

        /// <summary>Fourth tier.</summary>
        Platinum = 3,

        /// <summary>Fifth tier.</summary>
        Diamond = 4,

        /// <summary>Highest tier.</summary>
        Legend = 5
    }

    /// <summary>
    /// Current league state for the authenticated user.
    /// </summary>
    [Serializable]
    public class LeagueState
    {
        /// <summary>User's current league tier.</summary>
        [JsonProperty("tier")] public LeagueTier tier;

        /// <summary>Accumulated points in the current season.</summary>
        [JsonProperty("points")] public int points;

        /// <summary>Current rank within the tier.</summary>
        [JsonProperty("rank")] public int rank;

        /// <summary>Points needed to promote to the next tier.</summary>
        [JsonProperty("promotion_threshold")] public int promotionThreshold;

        /// <summary>Points below which the user is relegated.</summary>
        [JsonProperty("relegation_threshold")] public int relegationThreshold;

        /// <summary>ISO 8601 timestamp when the current season ends.</summary>
        [JsonProperty("season_ends_at")] public string seasonEndsAt;

        /// <summary>Current season identifier.</summary>
        [JsonProperty("season_id")] public string seasonId;

        /// <summary>Total number of players in the user's tier.</summary>
        [JsonProperty("tier_player_count")] public int tierPlayerCount;
    }

    /// <summary>
    /// A single entry on the league leaderboard.
    /// </summary>
    [Serializable]
    public class LeagueEntry
    {
        /// <summary>Player's user identifier.</summary>
        [JsonProperty("user_id")] public string userId;

        /// <summary>Player's display name.</summary>
        [JsonProperty("username")] public string username;

        /// <summary>Points accumulated this season.</summary>
        [JsonProperty("points")] public int points;

        /// <summary>Rank within the leaderboard.</summary>
        [JsonProperty("rank")] public int rank;

        /// <summary>Player's current tier.</summary>
        [JsonProperty("tier")] public LeagueTier tier;

        /// <summary>Optional avatar URL.</summary>
        [JsonProperty("avatar_url")] public string avatarUrl;
    }

    /// <summary>
    /// Response containing league leaderboard data.
    /// </summary>
    [Serializable]
    public class LeagueLeaderboardResponse
    {
        /// <summary>Ordered list of leaderboard entries.</summary>
        [JsonProperty("entries")] public List<LeagueEntry> entries;

        /// <summary>The requesting user's own entry.</summary>
        [JsonProperty("self")] public LeagueEntry self;

        /// <summary>Total entries in this leaderboard page.</summary>
        [JsonProperty("total")] public int total;
    }

    /// <summary>
    /// Result of submitting points to the league.
    /// </summary>
    [Serializable]
    public class LeaguePointsResult
    {
        /// <summary>Updated total points.</summary>
        [JsonProperty("points")] public int points;

        /// <summary>Updated rank.</summary>
        [JsonProperty("rank")] public int rank;

        /// <summary>Whether the user was promoted after this submission.</summary>
        [JsonProperty("promoted")] public bool promoted;

        /// <summary>New tier if promoted; otherwise the current tier.</summary>
        [JsonProperty("tier")] public LeagueTier tier;
    }

    /// <summary>
    /// Season information returned at end-of-season processing.
    /// </summary>
    [Serializable]
    public class SeasonInfo
    {
        /// <summary>Current season identifier.</summary>
        [JsonProperty("season_id")] public string seasonId;

        /// <summary>ISO 8601 start timestamp.</summary>
        [JsonProperty("starts_at")] public string startsAt;

        /// <summary>ISO 8601 end timestamp.</summary>
        [JsonProperty("ends_at")] public string endsAt;

        /// <summary>Whether the season is currently active.</summary>
        [JsonProperty("is_active")] public bool isActive;
    }
}
