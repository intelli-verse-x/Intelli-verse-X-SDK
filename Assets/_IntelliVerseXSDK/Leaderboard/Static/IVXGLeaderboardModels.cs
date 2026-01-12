using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace IntelliVerseX.Games.Leaderboard
{
    #region Score Submission Response

    /// <summary>
    /// Response from score submission RPC.
    /// </summary>
    [Serializable]
    public class IVXGScoreSubmissionResponse
    {
        public bool success;
        public string error;
        public long score;
        public int subscore;
        public int reward_earned;
        public string reward_currency;
        public long wallet_balance;
        public IVXGRewardDetails reward_details;
        public List<IVXGLeaderboardUpdate> leaderboards_updated;
    }

    /// <summary>
    /// Detailed breakdown of reward calculation.
    /// </summary>
    [Serializable]
    public class IVXGRewardDetails
    {
        public int base_reward;
        public int score;
        public float multiplier;
        public float streak_multiplier;
        public int milestone_bonus;
        public int final_reward;
        public bool capped;
    }

    /// <summary>
    /// Information about a leaderboard update after score submission.
    /// </summary>
    [Serializable]
    public class IVXGLeaderboardUpdate
    {
        public string leaderboard_id;
        public string scope;
        public string period;
        public int prev_rank;
        public int new_rank;
        public bool new_record;
    }

    #endregion

    #region All Leaderboards Response

    /// <summary>
    /// Response containing all leaderboard data.
    /// </summary>
    [Serializable]
    public class IVXGAllLeaderboardsResponse
    {
        public bool success;
        public string error;
        public string game_id;

        /// <summary>
        /// Raw leaderboards dictionary from server (keyed by leaderboard_id).
        /// </summary>
        public Dictionary<string, IVXGLeaderboardData> leaderboards;

        /// <summary>
        /// Player rank information.
        /// </summary>
        public IVXGPlayerRanks player_ranks;

        // Convenience accessors (populated by ResolveGameLeaderboards)
        [JsonIgnore] public IVXGLeaderboardData daily;
        [JsonIgnore] public IVXGLeaderboardData weekly;
        [JsonIgnore] public IVXGLeaderboardData monthly;
        [JsonIgnore] public IVXGLeaderboardData alltime;
        [JsonIgnore] public IVXGLeaderboardData global_alltime;

        /// <summary>
        /// Resolves game-specific leaderboard references from the raw dictionary.
        /// </summary>
        /// <param name="gameId">The game ID to match leaderboards against</param>
        public void ResolveGameLeaderboards(string gameId)
        {
            if (leaderboards == null || string.IsNullOrWhiteSpace(gameId))
                return;

            string dailyKey = $"{gameId}_daily";
            string weeklyKey = $"{gameId}_weekly";
            string monthlyKey = $"{gameId}_monthly";
            string alltimeKey = $"{gameId}_alltime";
            string globalKey = "global_alltime";

            leaderboards.TryGetValue(dailyKey, out daily);
            leaderboards.TryGetValue(weeklyKey, out weekly);
            leaderboards.TryGetValue(monthlyKey, out monthly);
            leaderboards.TryGetValue(alltimeKey, out alltime);
            leaderboards.TryGetValue(globalKey, out global_alltime);

            // Fallback: try without game prefix for global
            if (global_alltime == null)
            {
                foreach (var kvp in leaderboards)
                {
                    if (kvp.Key.Contains("global") && kvp.Key.Contains("alltime"))
                    {
                        global_alltime = kvp.Value;
                        break;
                    }
                }
            }
        }
    }

    /// <summary>
    /// Player rank information across different leaderboards.
    /// </summary>
    [Serializable]
    public class IVXGPlayerRanks
    {
        public int? daily_rank;
        public int? weekly_rank;
        public int? monthly_rank;
        public int? alltime_rank;
        public int? global_rank;
    }

    #endregion

    #region Leaderboard Data

    /// <summary>
    /// Data for a single leaderboard.
    /// </summary>
    [Serializable]
    public class IVXGLeaderboardData
    {
        public string leaderboard_id;
        public string period;
        public string scope;
        public List<IVXGLeaderboardRecord> records;
        public string next_cursor;
        public string prev_cursor;
    }

    /// <summary>
    /// A single record in a leaderboard.
    /// </summary>
    [Serializable]
    public class IVXGLeaderboardRecord
    {
        public string leaderboard_id;
        public string owner_id;
        public string username;
        public long score;
        public int subscore;
        public int num_score;
        public int rank;
        public string metadata;
        public string create_time;
        public string update_time;
        public string expiry_time;
        public int max_num_score;

        /// <summary>
        /// Parse metadata JSON into a dictionary.
        /// </summary>
        public Dictionary<string, object> GetMetadata()
        {
            if (string.IsNullOrWhiteSpace(metadata))
                return new Dictionary<string, object>();

            try
            {
                return JsonConvert.DeserializeObject<Dictionary<string, object>>(metadata);
            }
            catch
            {
                return new Dictionary<string, object>();
            }
        }
    }

    #endregion

    #region Leaderboard Period Enum

    /// <summary>
    /// Leaderboard time periods.
    /// </summary>
    public enum IVXGLeaderboardPeriod
    {
        Daily,
        Weekly,
        Monthly,
        Alltime,
        Global
    }

    #endregion
}
