using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace IntelliVerseX.Competition
{
    /// <summary>
    /// A prize awarded for tournament placement.
    /// </summary>
    [Serializable]
    public class IVXTournamentPrize
    {
        [JsonProperty("rank")] public int rank;
        [JsonProperty("reward_type")] public string rewardType;
        [JsonProperty("amount")] public int amount;
    }

    /// <summary>
    /// A single entry on the tournament leaderboard.
    /// </summary>
    [Serializable]
    public class IVXTournamentEntry
    {
        [JsonProperty("user_id")] public string userId;
        [JsonProperty("username")] public string username;
        [JsonProperty("score")] public long score;
        [JsonProperty("rank")] public int rank;
    }

    /// <summary>
    /// Competitive tournament (prod <c>tournament_list</c> / <c>tournament_get</c>).
    /// </summary>
    [Serializable]
    public class IVXTournament
    {
        /// <summary>Prod list uses <c>slug</c> as the stable id.</summary>
        [JsonProperty("slug")] public string slug;
        [JsonProperty("tournament_id")] public string tournamentId;
        [JsonProperty("name")] public string name;
        [JsonProperty("title")] public string title;
        [JsonProperty("description")] public string description;
        [JsonProperty("status")] public string status;
        [JsonProperty("format")] public string format;
        [JsonProperty("topic_tag")] public string topicTag;
        [JsonProperty("start_at")] public string startAt;
        [JsonProperty("end_at")] public string endAt;
        [JsonProperty("open_start_iso")] public string openStartIso;
        [JsonProperty("entry_fee")] public int entryFee;
        [JsonProperty("entry_fee_bc")] public int entryFeeBc;
        [JsonProperty("prize_pool")] public int prizePool;
        [JsonProperty("pot_bc")] public int potBc;
        [JsonProperty("entries_count")] public int entriesCount;
        [JsonProperty("joined")] public bool joined;
        [JsonProperty("current_rank")] public int currentRank;

        [JsonIgnore]
        public string Id => !string.IsNullOrEmpty(tournamentId) ? tournamentId : slug;

        [JsonIgnore]
        public string DisplayTitle => !string.IsNullOrEmpty(title) ? title : name;

        [JsonIgnore]
        public int EffectiveEntryFee => entryFeeBc > 0 ? entryFeeBc : entryFee;

        [JsonIgnore]
        public int EffectivePrizePool => potBc > 0 ? potBc : prizePool;
    }

    /// <summary>
    /// Response wrapper for listing active tournaments.
    /// </summary>
    [Serializable]
    public class IVXTournamentListResponse
    {
        [JsonProperty("tournaments")] public List<IVXTournament> tournaments;
    }

    /// <summary>
    /// Response wrapper for a tournament leaderboard.
    /// </summary>
    [Serializable]
    public class IVXTournamentLeaderboardResponse
    {
        [JsonProperty("entries")] public List<IVXTournamentEntry> entries;
    }

    /// <summary>
    /// Request payload for joining a tournament (prod requires <c>tournament_id</c>).
    /// </summary>
    [Serializable]
    public class IVXTournamentJoinRequest
    {
        [JsonProperty("tournament_id")] public string tournamentId;
        [JsonProperty("slug")] public string slug;
    }

    /// <summary>
    /// Request payload for submitting a tournament score.
    /// </summary>
    [Serializable]
    public class IVXTournamentScoreRequest
    {
        [JsonProperty("tournament_id")] public string tournamentId;
        [JsonProperty("slug")] public string slug;
        [JsonProperty("score")] public long score;
    }

    /// <summary>
    /// Request payload for retrieving a tournament leaderboard.
    /// </summary>
    [Serializable]
    public class IVXTournamentLeaderboardRequest
    {
        [JsonProperty("tournament_id")] public string tournamentId;
        [JsonProperty("slug")] public string slug;
    }
}
