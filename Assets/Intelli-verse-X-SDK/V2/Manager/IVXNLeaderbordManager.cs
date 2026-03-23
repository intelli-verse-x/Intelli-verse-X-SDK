using IntelliVerseX.Core;
using Nakama;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

namespace IntelliVerseX.Backend.Nakama
{
    /// <summary>
    /// Static, production-ready leaderboard manager for IntelliVerse-X.
    /// Handles score submission and fetching all leaderboards via Nakama RPCs.
    /// </summary>
    public static class IVXNLeaderbordManager
    {
        private const string LOGTAG = "[IVXNLeaderboard]";

        private const string RPC_SUBMIT_SCORE_AND_SYNC = "submit_score_and_sync";
        private const string RPC_GET_ALL_LEADERBOARDS = "get_all_leaderboards";

        private static int _currentWinStreak = 0;
        public static int CurrentWinStreak => _currentWinStreak;

        public static bool EnableDebugLogs { get; set; } = true;

        public static event Action<IVXScoreSubmissionResponse> OnScoreSubmitted;
        public static event Action<IVXAllLeaderboardsResponse> OnLeaderboardsFetched;
        public static event Action<string> OnError;

        #region Public API

        public static async Task<IVXScoreSubmissionResponse> SubmitScoreAsync(
            int score,
            int subscore = 0,
            Dictionary<string, string> metadata = null)
        {
            var mgr = IVXNManager.Instance;
            if (!ValidateManager(mgr, nameof(SubmitScoreAsync)))
                return null;

            try
            {
                if (!await mgr.EnsureValidSessionAsync())
                {
                    LogError("SubmitScoreAsync aborted: failed to ensure valid Nakama session.");
                    return null;
                }

                // ------- Resolve identities ---------------------------------
                var apiUser = global::UserSessionManager.Current;
                if (apiUser == null || string.IsNullOrWhiteSpace(apiUser.userId))
                {
                    LogError("SubmitScoreAsync: API user session or userId is missing.");
                    return null;
                }

                string apiUserId = apiUser.userId;               // IntelliVerse API user id
                string nakamaUserId = ResolveNakamaUserId(mgr);  // Nakama user id
                string gameId = mgr.GameId;

                if (string.IsNullOrWhiteSpace(nakamaUserId))
                {
                    LogError("SubmitScoreAsync: NakamaUserId is empty. " +
                             "Make sure IVXNManager.InitializeForCurrentUserAsync has completed successfully.");
                    return null;
                }

                if (string.IsNullOrEmpty(gameId))
                {
                    LogError("SubmitScoreAsync: GameId on IVXNManager is empty. Check your IntelliVerseXConfig.");
                    return null;
                }

                // Username precedence: Nakama → API → runtime snapshot → generic
                string username =
                    mgr.NakamaUsername ??
                    IVXNUserRuntime.Instance?.NakamaUsername ??
                    apiUser.userName ??
                    IVXNUserRuntime.Instance?.ApiUserName ??
                    "Player";

                string deviceId = ResolveDeviceIdSafe();

                Log($"SubmitScoreAsync identity → apiUserId={apiUserId}, nakamaUserId={nakamaUserId}, " +
                    $"username={username}, gameId={gameId}, deviceId={deviceId}");

                // ------- Build payload --------------------------------------
                var payload = new
                {
                    // API user identity (what your HTTP backend likely expects)
                    userId = apiUserId,
                    user_id = apiUserId,
                    apiUserId = apiUserId,
                    api_user_id = apiUserId,

                    // Nakama identity (for leaderboard owner / diagnostics)
                    ownerId = nakamaUserId,
                    owner_id = nakamaUserId,
                    nakamaUserId = nakamaUserId,
                    nakama_user_id = nakamaUserId,

                    // Game + device
                    gameId = gameId,
                    game_id = gameId,
                    deviceId = deviceId,
                    device_id = deviceId,

                    // Score stuff
                    score = score,
                    subscore = subscore,
                    currentStreak = _currentWinStreak,
                    current_streak = _currentWinStreak,

                    // Display
                    username = username,

                    // Optional extra data
                    metadata = metadata
                };

                var jsonPayload = JsonConvert.SerializeObject(payload);
                Log($"SubmitScoreAsync JSON → {jsonPayload}");

                var rpcResponse = await mgr.Client.RpcAsync(mgr.Session, RPC_SUBMIT_SCORE_AND_SYNC, jsonPayload);
                Log($"SubmitScoreAsync RPC payload raw → {rpcResponse.Payload}");

                var result = JsonConvert.DeserializeObject<IVXScoreSubmissionResponse>(rpcResponse.Payload);

                if (result == null)
                {
                    LogError("SubmitScoreAsync: RPC response deserialized to null.");
                    _currentWinStreak = 0;
                    return null;
                }

                if (result.success)
                {
                    _currentWinStreak++;
                    LogScoreSubmissionSuccess(result);
                    SafeInvoke(OnScoreSubmitted, result);
                }
                else
                {
                    _currentWinStreak = 0;
                    LogError($"SubmitScoreAsync failed: {result.error ?? "Unknown error"}");
                }

                return result;
            }
            catch (Exception ex)
            {
                _currentWinStreak = 0;
                LogError($"SubmitScoreAsync exception: {ex}");
                return null;
            }
        }

        public static async Task<IVXAllLeaderboardsResponse> GetAllLeaderboardsAsync(int limit = 50)
        {
            var mgr = IVXNManager.Instance;
            if (!ValidateManager(mgr, nameof(GetAllLeaderboardsAsync)))
                return null;

            try
            {
                if (!await mgr.EnsureValidSessionAsync())
                {
                    LogError("GetAllLeaderboardsAsync aborted: failed to ensure valid Nakama session.");
                    return null;
                }

                var apiUser = global::UserSessionManager.Current;
                if (apiUser == null || string.IsNullOrWhiteSpace(apiUser.userId))
                {
                    LogError("GetAllLeaderboardsAsync: API user session or userId is missing.");
                    return null;
                }

                string apiUserId = apiUser.userId;
                string nakamaUserId = ResolveNakamaUserId(mgr);
                string gameId = mgr.GameId;

                if (string.IsNullOrWhiteSpace(nakamaUserId))
                {
                    LogError("GetAllLeaderboardsAsync: NakamaUserId is empty. " +
                             "Make sure IVXNManager.InitializeForCurrentUserAsync has completed successfully.");
                    return null;
                }

                if (string.IsNullOrEmpty(gameId))
                {
                    LogError("GetAllLeaderboardsAsync: GameId on IVXNManager is empty. Check your IntelliVerseXConfig.");
                    return null;
                }

                string deviceId = ResolveDeviceIdSafe();

                Log($"GetAllLeaderboardsAsync identity → apiUserId={apiUserId}, nakamaUserId={nakamaUserId}, " +
                    $"gameId={gameId}, deviceId={deviceId}, limit={limit}");

                var payload = new
                {
                    userId = apiUserId,
                    user_id = apiUserId,
                    apiUserId = apiUserId,
                    api_user_id = apiUserId,

                    ownerId = nakamaUserId,
                    owner_id = nakamaUserId,
                    nakamaUserId = nakamaUserId,
                    nakama_user_id = nakamaUserId,

                    gameId = gameId,
                    game_id = gameId,

                    deviceId = deviceId,
                    device_id = deviceId,

                    limit = limit
                };

                var jsonPayload = JsonConvert.SerializeObject(payload);
                Log($"GetAllLeaderboardsAsync JSON → {jsonPayload}");

                var rpcResponse = await mgr.Client.RpcAsync(mgr.Session, RPC_GET_ALL_LEADERBOARDS, jsonPayload);
                Log($"GetAllLeaderboardsAsync RPC payload raw → {rpcResponse.Payload}");

                var result = JsonConvert.DeserializeObject<IVXAllLeaderboardsResponse>(rpcResponse.Payload);

                if (result == null)
                {
                    LogError("GetAllLeaderboardsAsync: RPC response deserialized to null.");
                    return null;
                }

                // Map Nakama "leaderboards" dictionary => daily/weekly/monthly/alltime/global_alltime
                try
                {
                    // Prefer the server-reported game_id if present, otherwise fall back to mgr.GameId
                    string resolvedGameId = !string.IsNullOrWhiteSpace(result.game_id) ? result.game_id : gameId;
                    result.ResolveGameLeaderboards(resolvedGameId);

                    if (result.alltime != null)
                    {
                        Log($"Mapped ALLTIME leaderboard: id={result.alltime.leaderboard_id}, " +
                            $"records={result.alltime.records?.Count ?? 0}");
                    }
                    else
                    {
                        Log("ResolveGameLeaderboards: alltime leaderboard is null (check game_id & Nakama setup).");
                    }
                }
                catch (Exception ex)
                {
                    LogError($"GetAllLeaderboardsAsync: ResolveGameLeaderboards exception: {ex}");
                }

                if (!result.success)
                {
                    LogError($"GetAllLeaderboardsAsync failed: {result.error ?? "Unknown error"}");
                    return result;
                }

                Log("✓ Leaderboards fetched successfully.");
                SafeInvoke(OnLeaderboardsFetched, result);

                return result;
            }
            catch (Exception ex)
            {
                LogError($"GetAllLeaderboardsAsync exception: {ex}");
                return null;
            }
        }

        public static async Task<int> GetPlayerRankAsync(bool global = false)
        {
            var response = await GetAllLeaderboardsAsync(limit: 1);
            if (response == null || !response.success || response.player_ranks == null)
                return 0;

            int? rank = global
                ? response.player_ranks.global_rank
                : response.player_ranks.alltime_rank;

            return rank ?? 0;
        }

        public static async Task<long> GetPlayerBestScoreAsync(bool global = false)
        {
            var mgr = IVXNManager.Instance;
            if (!ValidateManager(mgr, nameof(GetPlayerBestScoreAsync)))
                return 0;

            var response = await GetAllLeaderboardsAsync(limit: 100);
            if (response == null || !response.success)
                return 0;

            string ownerId = ResolveNakamaUserId(mgr);
            if (string.IsNullOrWhiteSpace(ownerId))
                return 0;

            // Uses the mapped fields from ResolveGameLeaderboards
            IVXLeaderboardData data = global ? response.global_alltime : response.alltime;
            if (data?.records == null || data.records.Count == 0)
                return 0;

            var record = data.records.FirstOrDefault(r => r.owner_id == ownerId);
            return record?.score ?? 0;
        }

        public static void ResetWinStreak()
        {
            _currentWinStreak = 0;
            Log("Win streak reset.");
        }

        #endregion

        #region Internal helpers

        private static bool ValidateManager(IVXNManager mgr, string method)
        {
            if (mgr == null)
            {
                LogError($"{method}: IVXNManager.Instance is null. " +
                         "Make sure an IVXNManager is present in the scene and initialized before using leaderboards.");
                return false;
            }

            if (!mgr.IsInitialized && !mgr.IsInitializing)
            {
                Debug.LogWarning($"{LOGTAG} {method}: IVXNManager is not initialized yet. " +
                                 "Call await IVXNManager.Instance.InitializeForCurrentUserAsync() first " +
                                 "or let your login flow handle it.");
            }

            return true;
        }

        private static string ResolveNakamaUserId(IVXNManager mgr)
        {
            string id = mgr?.NakamaUserId;

            if (string.IsNullOrWhiteSpace(id) && IVXNUserRuntime.Instance != null)
            {
                id = IVXNUserRuntime.Instance.NakamaUserId;
                if (string.Equals(id, "<null>", StringComparison.OrdinalIgnoreCase))
                    id = null;
            }

            return id;
        }

        private static string ResolveDeviceIdSafe()
        {
            try
            {
                return IntelliVerseXIdentity.CurrentUser?.DeviceId;
            }
            catch
            {
                return null;
            }
        }

        private static void LogScoreSubmissionSuccess(IVXScoreSubmissionResponse response)
        {
            if (response == null)
            {
                Log("Score submission success handler called with null response.");
                return;
            }

            Log("✓ Score submitted successfully.");
            Log($"  Score:     {response.score}");
            Log($"  Subscore:  {response.subscore}");
            Log($"  Reward:    {response.reward_earned} {response.reward_currency}");
            Log($"  Balance:   {response.wallet_balance}");
            Log($"  Streak:    {CurrentWinStreak}");

            if (response.reward_details != null)
            {
                var d = response.reward_details;
                Log($"  Details: base={d.base_reward} (score={d.score} × mult={d.multiplier}), " +
                    $"streakMult={d.streak_multiplier}, bonus={d.milestone_bonus}, final={d.final_reward}, capped={d.capped}");
            }

            if (response.leaderboards_updated != null)
            {
                foreach (var lb in response.leaderboards_updated)
                {
                    Log($"  Leaderboard updated -> scope={lb.scope}, period={lb.period}, " +
                        $"rank {lb.prev_rank} → {lb.new_rank}, new_record={lb.new_record}");
                }
            }
        }

        private static void Log(string message)
        {
            if (!EnableDebugLogs) return;
            Debug.Log($"{LOGTAG} {message}");
        }

        private static void LogError(string message)
        {
            Debug.LogError($"{LOGTAG} {message}");
            SafeInvoke(OnError, message);
        }

        private static void SafeInvoke<T>(Action<T> evt, T arg)
        {
            try
            {
                evt?.Invoke(arg);
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"{LOGTAG} Listener exception: {ex}");
            }
        }

        #endregion
    }
}
