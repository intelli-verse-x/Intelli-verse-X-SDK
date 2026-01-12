// IVXNModels.cs
// Response and payload models for Nakama backend integration
// Includes: Authentication, Leaderboards, Scores, Wallets, Adaptive Rewards
// Used by IVXNakamaManager and game-specific managers

using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace IntelliVerseX.Backend
{
    // ============================================================================
    // AUTHENTICATION & IDENTITY
    // ============================================================================

    [Serializable]
    public class IVXCreateOrSyncUserResponse
    {
        public bool success;
        public bool created;                // True if new user, false if synced existing
        public string username;
        public string wallet_id;            // Game-specific wallet ID
        public string global_wallet_id;     // Cross-game wallet ID
        public string error;
    }

    // ============================================================================
    // SCORE SUBMISSION
    // ============================================================================

    [Serializable]
    public class IVXScoreSubmissionResponse
    {
        public bool success;
        public int score;
        public int subscore;

        // Adaptive reward fields
        public int reward_earned;
        public string reward_currency;
        public int wallet_balance;
        public IVXRewardCalculationDetails reward_details;
        public IVXMilestoneBonus[] bonuses;

        // Leaderboard results
        public IVXLeaderboardUpdateResult[] leaderboards_updated;

        // Backward compatibility
        public IVXLeaderboardUpdateResult[] results => leaderboards_updated;

        public string error;
    }

    [Serializable]
    public class IVXLeaderboardUpdateResult
    {
        public string scope;        // "game" or "global"
        public string period;       // "daily", "weekly", "monthly", "alltime"
        public int new_rank;
        public int prev_rank;
        public bool new_record;
    }

    // ============================================================================
    // LEADERBOARDS
    // ============================================================================

    /// <summary>
    /// Wrapper for the Nakama get_all_leaderboards RPC response.
    /// Matches JSON like:
    /// {
    ///   "success": true,
    ///   "device_id": "...",
    ///   "game_id": "126bf539-...",
    ///   "leaderboards": {
    ///      "leaderboard_126bf539-..._alltime": { ... },
    ///      "leaderboard_126bf539-..._daily":   { ... },
    ///      "leaderboard_global_alltime":       { ... },
    ///      ...
    ///   }
    /// }
    /// Convenience fields (daily/weekly/monthly/alltime/global_alltime) are
    /// populated by calling ResolveGameLeaderboards(gameId).
    /// </summary>
    [Serializable]
    public class IVXAllLeaderboardsResponse
    {
        // Raw JSON
        [JsonProperty("success")]
        public bool success;

        [JsonProperty("device_id")]
        public string device_id;

        [JsonProperty("game_id")]
        public string game_id;

        // "leaderboards": { "<leaderboard_id>": { ... }, ... }
        [JsonProperty("leaderboards")]
        public Dictionary<string, IVXLeaderboardData> leaderboards;

        [JsonProperty("player_ranks")]
        public IVXPlayerRanks player_ranks;

        [JsonProperty("error")]
        public string error;

        // Convenience views (NOT in JSON directly)
        // These are what the rest of your SDK/UI uses.
        [JsonIgnore] public IVXLeaderboardData daily;
        [JsonIgnore] public IVXLeaderboardData weekly;
        [JsonIgnore] public IVXLeaderboardData monthly;
        [JsonIgnore] public IVXLeaderboardData alltime;        // leaderboard_<gameId>_alltime
        [JsonIgnore] public IVXLeaderboardData global_alltime; // leaderboard_global_alltime

        /// <summary>
        /// Map the raw leaderboards dictionary into the convenience fields above.
        /// Call this once right after deserialization, passing the current gameId.
        /// </summary>
        public void ResolveGameLeaderboards(string gameId)
        {
            if (leaderboards == null || string.IsNullOrWhiteSpace(gameId))
                return;

            string prefix = $"leaderboard_{gameId}";

            daily = TryGet($"{prefix}_daily");
            weekly = TryGet($"{prefix}_weekly");
            monthly = TryGet($"{prefix}_monthly");
            alltime = TryGet($"{prefix}_alltime");             // ← leaderboard_126bf539-..._alltime
            global_alltime = TryGet("leaderboard_global_alltime");
        }

        private IVXLeaderboardData TryGet(string id)
        {
            if (leaderboards == null)
                return null;

            IVXLeaderboardData data;
            return leaderboards.TryGetValue(id, out data) ? data : null;
        }
    }

    /// <summary>
    /// Single leaderboard container: records + user_record + cursors.
    /// </summary>
    [Serializable]
    public class IVXLeaderboardData
    {
        [JsonProperty("leaderboard_id")]
        public string leaderboard_id;

        [JsonProperty("records")]
        public List<IVXLeaderboardRecord> records;

        // Optional – Nakama user_record for the current player
        [JsonProperty("user_record")]
        public IVXLeaderboardRecord user_record;

        [JsonProperty("next_cursor")]
        public string next_cursor;

        [JsonProperty("prev_cursor")]
        public string prev_cursor;
    }

    /// <summary>
    /// Single leaderboard record (row).
    /// Matches Nakama JSON fields exactly via JsonProperty attributes.
    /// </summary>
    [Serializable]
    public class IVXLeaderboardRecord
    {
        [JsonProperty("ownerId")]
        public string owner_id;

        [JsonProperty("username")]
        public string username;

        [JsonProperty("score")]
        public long score;

        [JsonProperty("subscore")]
        public long subscore;

        [JsonProperty("rank")]
        public int rank;

        [JsonProperty("numScore")]
        public int num_score;

        [JsonProperty("maxNumScore")]
        public int max_num_score;

        [JsonProperty("metadata")]
        public Dictionary<string, string> metadata;

        [JsonProperty("createTime")]
        public string create_time;
        
        [JsonProperty("isCurrentPlayer")]
        public bool is_current_player;

        [JsonProperty("updateTime")]
        public string update_time;

        [JsonProperty("expiryTime")]
        public string expiry_time;

        [JsonProperty("leaderboardId")]
        public string leaderboard_id;
    }

    [Serializable]
    public class IVXPlayerRanks
    {
        [JsonProperty("daily_rank")]
        public int? daily_rank;

        [JsonProperty("weekly_rank")]
        public int? weekly_rank;

        [JsonProperty("monthly_rank")]
        public int? monthly_rank;

        [JsonProperty("alltime_rank")]
        public int? alltime_rank;

        [JsonProperty("global_rank")]
        public int? global_rank;
    }

    // ============================================================================
    // ADAPTIVE REWARDS
    // ============================================================================

    /// <summary>
    /// Response from calculate_score_reward RPC
    /// Shows player how much they will earn before submitting score
    /// </summary>
    [Serializable]
    public class IVXCalculateScoreRewardResponse
    {
        public bool success;
        public int reward_amount;
        public string currency;
        public IVXMilestoneBonus[] bonuses;
        public IVXRewardCalculationDetails details;
        public string error;
        
        // Convenience properties
        public int baseReward;
        public int streakBonus;
        public int totalReward;
    }

    /// <summary>
    /// Individual bonus earned from milestones or streaks
    /// </summary>
    [Serializable]
    public class IVXMilestoneBonus
    {
        public string type;          // "milestone_1k", "milestone_5k", "streak_3", etc.
        public int amount;           // Bonus amount
        public int threshold;        // Score/streak threshold that triggered this bonus
    }

    /// <summary>
    /// Detailed breakdown of reward calculation
    /// Useful for debugging and showing players how rewards are calculated
    /// </summary>
    [Serializable]
    public class IVXRewardCalculationDetails
    {
        public string game_name;            // Game name (e.g., "QuizVerse")
        public int score;                   // Original score
        public int base_reward;             // score × multiplier
        public float multiplier;            // Game's score_to_coins_multiplier
        public int streak;                  // Current win streak
        public float streak_multiplier;     // Multiplier applied for streak (e.g., 1.5x)
        public int milestone_bonus;         // Total bonus from milestones
        public int final_reward;            // Final reward after all bonuses
        public bool capped;                 // True if reward hit max_reward_per_match
    }

    /// <summary>
    /// Response from update_game_reward_config RPC (Admin only)
    /// </summary>
    [Serializable]
    public class IVXUpdateGameRewardConfigResponse
    {
        public bool success;
        public string game_id;
        public IVXGameRewardConfig config;
        public string message;
        public string error;
    }

    /// <summary>
    /// Game-specific reward configuration
    /// Defines how scores are converted to currency for a specific game
    /// </summary>
    [Serializable]
    public class IVXGameRewardConfig
    {
        public string game_name;                        // Human-readable game name
        public float score_to_coins_multiplier;         // Base conversion rate (e.g., 0.1 = 1000 score → 100 coins)
        public int min_score_for_reward;                // Minimum score to earn any reward
        public int max_reward_per_match;                // Cap on rewards per match
        public string currency_id;                      // Currency ID (e.g., "coins", "gems")
        public IVXMilestoneThreshold[] bonus_thresholds;  // Score milestones with bonus rewards
        public Dictionary<int, float> streak_multipliers; // Streak → multiplier mapping

        // Backward compatibility
        public string currency => currency_id;
    }

    /// <summary>
    /// Score milestone that grants bonus rewards
    /// Example: { score: 1000, bonus: 50, type: "milestone_1k" }
    /// </summary>
    [Serializable]
    public class IVXMilestoneThreshold
    {
        public int score;           // Score threshold to reach
        public int bonus;           // Bonus amount granted
        public string type;         // Bonus identifier (e.g., "milestone_1k")
    }

    // ============================================================================
    // REWARD CONFIG BUILDER (SDK Helper)
    // ============================================================================

    /// <summary>
    /// Helper class to build reward config for admin updates
    /// Provides default configurations for different game types
    /// </summary>
    public static class IVXRewardConfigBuilder
    {
        /// <summary>
        /// Create default quiz game reward config
        /// High frequency gameplay, moderate rewards
        /// </summary>
        public static IVXGameRewardConfig CreateQuizConfig(string gameName)
        {
            return new IVXGameRewardConfig
            {
                game_name = gameName,
                score_to_coins_multiplier = 0.1f,
                min_score_for_reward = 10,
                max_reward_per_match = 10000,
                currency_id = "coins",
                bonus_thresholds = new IVXMilestoneThreshold[]
                {
                    new IVXMilestoneThreshold { score = 1000, bonus = 100, type = "milestone_1k" },
                    new IVXMilestoneThreshold { score = 5000, bonus = 500, type = "milestone_5k" },
                    new IVXMilestoneThreshold { score = 10000, bonus = 1000, type = "milestone_10k" }
                },
                streak_multipliers = new Dictionary<int, float>
                {
                    { 3, 1.1f },   // 10% bonus at 3 wins
                    { 5, 1.25f },  // 25% bonus at 5 wins
                    { 10, 1.5f }   // 50% bonus at 10 wins
                }
            };
        }

        /// <summary>
        /// Create default action game reward config
        /// Lower frequency gameplay, higher rewards per match
        /// </summary>
        public static IVXGameRewardConfig CreateActionConfig(string gameName)
        {
            return new IVXGameRewardConfig
            {
                game_name = gameName,
                score_to_coins_multiplier = 0.2f,
                min_score_for_reward = 50,
                max_reward_per_match = 15000,
                currency_id = "coins",
                bonus_thresholds = new IVXMilestoneThreshold[]
                {
                    new IVXMilestoneThreshold { score = 2000, bonus = 200, type = "survivor" },
                    new IVXMilestoneThreshold { score = 5000, bonus = 1000, type = "elite" },
                    new IVXMilestoneThreshold { score = 10000, bonus = 3000, type = "master" }
                },
                streak_multipliers = new Dictionary<int, float>
                {
                    { 3, 1.15f },
                    { 5, 1.3f },
                    { 10, 1.75f },
                    { 20, 2.0f }
                }
            };
        }

        /// <summary>
        /// Create default casual game reward config
        /// Very high frequency gameplay, small rewards
        /// </summary>
        public static IVXGameRewardConfig CreateCasualConfig(string gameName)
        {
            return new IVXGameRewardConfig
            {
                game_name = gameName,
                score_to_coins_multiplier = 0.05f,
                min_score_for_reward = 5,
                max_reward_per_match = 5000,
                currency_id = "coins",
                bonus_thresholds = new IVXMilestoneThreshold[]
                {
                    new IVXMilestoneThreshold { score = 500, bonus = 25, type = "bronze" },
                    new IVXMilestoneThreshold { score = 2000, bonus = 100, type = "silver" },
                    new IVXMilestoneThreshold { score = 5000, bonus = 500, type = "gold" }
                },
                streak_multipliers = new Dictionary<int, float>
                {
                    { 5, 1.1f },
                    { 10, 1.2f },
                    { 25, 1.5f }
                }
            };
        }

        /// <summary>
        /// Create custom config with builder pattern
        /// </summary>
        public static IVXGameRewardConfig CreateCustomConfig(string gameName)
        {
            return new IVXGameRewardConfig
            {
                game_name = gameName,
                score_to_coins_multiplier = 0.1f,
                min_score_for_reward = 10,
                max_reward_per_match = 10000,
                currency_id = "coins",
                bonus_thresholds = new IVXMilestoneThreshold[0],
                streak_multipliers = new Dictionary<int, float>()
            };
        }
    }

    // ============================================================================
    // EXTENSION METHODS FOR BUILDER PATTERN
    // ============================================================================

    public static class IVXRewardConfigExtensions
    {
        public static IVXGameRewardConfig WithMultiplier(this IVXGameRewardConfig config, float multiplier)
        {
            config.score_to_coins_multiplier = multiplier;
            return config;
        }

        public static IVXGameRewardConfig WithMaxReward(this IVXGameRewardConfig config, int maxReward)
        {
            config.max_reward_per_match = maxReward;
            return config;
        }

        public static IVXGameRewardConfig WithMinScore(this IVXGameRewardConfig config, int minScore)
        {
            config.min_score_for_reward = minScore;
            return config;
        }

        public static IVXGameRewardConfig WithCurrency(this IVXGameRewardConfig config, string currencyId)
        {
            config.currency_id = currencyId;
            return config;
        }

        public static IVXGameRewardConfig AddMilestone(this IVXGameRewardConfig config, int score, int bonus, string type)
        {
            var list = new List<IVXMilestoneThreshold>(config.bonus_thresholds ?? new IVXMilestoneThreshold[0])
            {
                new IVXMilestoneThreshold { score = score, bonus = bonus, type = type }
            };
            config.bonus_thresholds = list.ToArray();
            return config;
        }

        public static IVXGameRewardConfig AddStreakMultiplier(this IVXGameRewardConfig config, int streakCount, float multiplier)
        {
            if (config.streak_multipliers == null)
                config.streak_multipliers = new Dictionary<int, float>();

            config.streak_multipliers[streakCount] = multiplier;
            return config;
        }

        public static IVXGameRewardConfig Build(this IVXGameRewardConfig config)
        {
            return config;
        }
    }
}
