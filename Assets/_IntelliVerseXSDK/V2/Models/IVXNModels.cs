using System.Text;
using UnityEngine;

namespace IntelliVerseX.Backend.Nakama
{
    /// <summary>
    /// Helper / utility class for working with IntelliVerse-X Nakama models.
    /// Provides:
    /// - Null-safe success checks
    /// - Debug string builders for leaderboards
    /// - Logging helpers for full leaderboard snapshots
    ///
    /// Usage:
    ///   if (response.IsSuccess()) { ... }
    ///   Debug.Log(record.ToDebugString());
    ///   IVXNModels.LogLeaderboards(allLeaderboardsResponse);
    /// </summary>
    public static class IVXNModels
    {
        private const string LOGTAG = "[IVXNModels]";

        // ----------------------------------------------------------------------
        // Success helpers
        // ----------------------------------------------------------------------

        public static bool IsSuccess(this IVXScoreSubmissionResponse response) =>
            response != null && response.success;

        public static bool IsSuccess(this IVXAllLeaderboardsResponse response) =>
            response != null && response.success;

        public static bool IsSuccess(this IVXCalculateScoreRewardResponse response) =>
            response != null && response.success;

        // ----------------------------------------------------------------------
        // Debug string helpers
        // ----------------------------------------------------------------------

        public static string ToDebugString(this IVXLeaderboardRecord record)
        {
            if (record == null) return "<null record>";

            return $"#{record.rank} {record.username} ({record.owner_id}) " +
                   $"score={record.score}, sub={record.subscore}, count={record.num_score}";
        }

        public static string ToDebugString(this IVXLeaderboardData data, string labelOverride = null)
        {
            if (data == null) return "<null leaderboard>";

            var sb = new StringBuilder();
            var label = labelOverride ?? data.leaderboard_id ?? "<leaderboard>";

            sb.AppendLine(label);

            if (data.records == null || data.records.Count == 0)
            {
                sb.AppendLine("  (no records)");
            }
            else
            {
                for (int i = 0; i < data.records.Count; i++)
                {
                    sb.AppendLine($"  {i + 1}. {data.records[i].ToDebugString()}");
                }
            }

            if (!string.IsNullOrEmpty(data.next_cursor))
                sb.AppendLine($"  next_cursor: {data.next_cursor}");
            if (!string.IsNullOrEmpty(data.prev_cursor))
                sb.AppendLine($"  prev_cursor: {data.prev_cursor}");

            return sb.ToString();
        }

        // ----------------------------------------------------------------------
        // Logging helpers
        // ----------------------------------------------------------------------

        /// <summary>
        /// Log a full leaderboard snapshot (daily/weekly/monthly/alltime/global).
        /// </summary>
        public static void LogLeaderboards(IVXAllLeaderboardsResponse response)
        {
            if (response == null)
            {
                Debug.LogWarning($"{LOGTAG} Response is null.");
                return;
            }

            if (!response.success)
            {
                Debug.LogWarning($"{LOGTAG} Response not successful: {response.error}");
                return;
            }

            if (response.player_ranks != null)
            {
                var r = response.player_ranks;
                Debug.Log($"{LOGTAG} Player ranks -> daily={r.daily_rank}, weekly={r.weekly_rank}, " +
                          $"monthly={r.monthly_rank}, alltime={r.alltime_rank}, global={r.global_rank}");
            }

            if (response.daily != null)
                Debug.Log($"{LOGTAG} DAILY\n{response.daily.ToDebugString("Daily")}");
            if (response.weekly != null)
                Debug.Log($"{LOGTAG} WEEKLY\n{response.weekly.ToDebugString("Weekly")}");
            if (response.monthly != null)
                Debug.Log($"{LOGTAG} MONTHLY\n{response.monthly.ToDebugString("Monthly")}");
            if (response.alltime != null)
                Debug.Log($"{LOGTAG} ALLTIME\n{response.alltime.ToDebugString("Alltime")}");
            if (response.global_alltime != null)
                Debug.Log($"{LOGTAG} GLOBAL ALLTIME\n{response.global_alltime.ToDebugString("Global Alltime")}");
        }
    }
}
