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
    /// Represents a competitive tournament.
    /// </summary>
    [Serializable]
    public class IVXTournament
    {
        [JsonProperty("tournament_id")] public string tournamentId;
        [JsonProperty("title")] public string title;
        [JsonProperty("description")] public string description;
        [JsonProperty("start_at")] public string startAt;
        [JsonProperty("end_at")] public string endAt;
        [JsonProperty("entry_fee")] public int entryFee;
        [JsonProperty("prize_pool")] public int prizePool;
        [JsonProperty("joined")] public bool joined;
        [JsonProperty("current_rank")] public int currentRank;
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
    /// Request payload for joining a tournament.
    /// </summary>
    [Serializable]
    public class IVXTournamentJoinRequest
    {
        [JsonProperty("tournament_id")] public string tournamentId;
    }

    /// <summary>
    /// Request payload for submitting a tournament score.
    /// </summary>
    [Serializable]
    public class IVXTournamentScoreRequest
    {
        [JsonProperty("tournament_id")] public string tournamentId;
        [JsonProperty("score")] public long score;
    }

    /// <summary>
    /// Request payload for retrieving a tournament leaderboard.
    /// </summary>
    [Serializable]
    public class IVXTournamentLeaderboardRequest
    {
        [JsonProperty("tournament_id")] public string tournamentId;
    }
}
