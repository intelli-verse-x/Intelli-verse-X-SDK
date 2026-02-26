using Nakama;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

namespace IntelliVerseX.Games.Leaderboard
{
    /// <summary>
    /// Static, production-ready leaderboard manager for IntelliVerse-X Games SDK.
    /// Handles score submission and fetching all leaderboards via Nakama RPCs.
    /// This is the core static API - use IVXGLeaderboard (MonoBehaviour) for scene-based integration.
    /// </summary>
    public static class IVXGLeaderboardManager
    {
        private const string LOGTAG = "[IVXGLeaderboard]";

        private const string RPC_SUBMIT_SCORE_AND_SYNC = "submit_score_and_sync";
        private const string RPC_GET_ALL_LEADERBOARDS = "get_all_leaderboards";
        private const string RPC_GET_TIME_PERIOD_LEADERBOARD = "get_time_period_leaderboard";

        private static int _currentWinStreak = 0;
        public static int CurrentWinStreak => _currentWinStreak;

        public static bool EnableDebugLogs { get; set; } = true;

        public static event Action<IVXGScoreSubmissionResponse> OnScoreSubmitted;
        public static event Action<IVXGAllLeaderboardsResponse> OnLeaderboardsFetched;
        public static event Action<string> OnError;

        #region Public API

        /// <summary>
        /// Submit a score to the leaderboard system.
        /// </summary>
        /// <param name="score">The score to submit</param>
        /// <param name="subscore">Optional subscore for tiebreaker</param>
        /// <param name="metadata">Optional metadata dictionary</param>
        /// <returns>Score submission response or null on failure</returns>
        public static async Task<IVXGScoreSubmissionResponse> SubmitScoreAsync(
            int score,
            int subscore = 0,
            Dictionary<string, string> metadata = null)
        {
            var mgr = GetNakamaManager();
            if (!ValidateManager(mgr, nameof(SubmitScoreAsync)))
                return null;

            try
            {
                if (!await mgr.EnsureValidSessionAsync())
                {
                    LogError("SubmitScoreAsync aborted: failed to ensure valid Nakama session.");
                    return null;
                }

                // Resolve identities using reflection for decoupling
                string apiUserId = ResolveApiUserId();
                string nakamaUserId = ResolveNakamaUserId(mgr);
                string gameId = GetGameId(mgr);
                string username = ResolveUsername(mgr);
                string deviceId = ResolveDeviceIdSafe();
                if (string.IsNullOrWhiteSpace(deviceId))
                {
                    // Backend contract requires device_id for identity lookup.
                    deviceId = apiUserId;
                    Log("SubmitScoreAsync: DeviceId unavailable, falling back to API user id.");
                }

                if (string.IsNullOrWhiteSpace(apiUserId))
                {
                    LogError("SubmitScoreAsync: API user session or userId is missing.");
                    return null;
                }

                if (string.IsNullOrWhiteSpace(nakamaUserId))
                {
                    LogError("SubmitScoreAsync: NakamaUserId is empty. " +
                             "Make sure Nakama initialization has completed successfully.");
                    return null;
                }

                if (string.IsNullOrEmpty(gameId))
                {
                    LogError("SubmitScoreAsync: GameId is empty. Check your IntelliVerseXConfig.");
                    return null;
                }

                Log($"SubmitScoreAsync identity → apiUserId={apiUserId}, nakamaUserId={nakamaUserId}, " +
                    $"username={username}, gameId={gameId}, deviceId={deviceId}");

                // Build payload
                var payload = new
                {
                    // API user identity
                    userId = apiUserId,
                    user_id = apiUserId,
                    apiUserId = apiUserId,
                    api_user_id = apiUserId,

                    // Nakama identity
                    ownerId = nakamaUserId,
                    owner_id = nakamaUserId,
                    nakamaUserId = nakamaUserId,
                    nakama_user_id = nakamaUserId,

                    // Game + device
                    gameId = gameId,
                    game_id = gameId,
                    deviceId = deviceId,
                    device_id = deviceId,

                    // Score
                    score = score,
                    subscore = subscore,
                    currentStreak = _currentWinStreak,
                    current_streak = _currentWinStreak,

                    // Display
                    username = username,

                    // Metadata
                    metadata = metadata
                };

                var jsonPayload = JsonConvert.SerializeObject(payload);
                Log($"SubmitScoreAsync JSON → {jsonPayload}");

                var client = GetClient(mgr);
                var session = GetSession(mgr);
                
                var rpcResponse = await client.RpcAsync(session, RPC_SUBMIT_SCORE_AND_SYNC, jsonPayload);
                Log($"SubmitScoreAsync RPC payload raw → {rpcResponse.Payload}");

                var result = JsonConvert.DeserializeObject<IVXGScoreSubmissionResponse>(rpcResponse.Payload);

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

        /// <summary>
        /// Get all leaderboards (daily, weekly, monthly, all-time, global).
        /// </summary>
        /// <param name="limit">Maximum entries to fetch per leaderboard</param>
        /// <returns>All leaderboards response or null on failure</returns>
        public static async Task<IVXGAllLeaderboardsResponse> GetAllLeaderboardsAsync(int limit = 50)
        {
            var mgr = GetNakamaManager();
            if (!ValidateManager(mgr, nameof(GetAllLeaderboardsAsync)))
                return null;

            try
            {
                if (!await mgr.EnsureValidSessionAsync())
                {
                    LogError("GetAllLeaderboardsAsync aborted: failed to ensure valid Nakama session.");
                    return null;
                }

                string apiUserId = ResolveApiUserId();
                string nakamaUserId = ResolveNakamaUserId(mgr);
                string gameId = GetGameId(mgr);
                string deviceId = ResolveDeviceIdSafe();
                if (string.IsNullOrWhiteSpace(deviceId))
                {
                    // Backend contract requires device_id for identity lookup.
                    deviceId = apiUserId;
                    Log("GetAllLeaderboardsAsync: DeviceId unavailable, falling back to API user id.");
                }

                if (string.IsNullOrWhiteSpace(apiUserId))
                {
                    LogError("GetAllLeaderboardsAsync: API user session or userId is missing.");
                    return null;
                }

                if (string.IsNullOrWhiteSpace(nakamaUserId))
                {
                    LogError("GetAllLeaderboardsAsync: NakamaUserId is empty. " +
                             "Make sure Nakama initialization has completed successfully.");
                    return null;
                }

                if (string.IsNullOrEmpty(gameId))
                {
                    LogError("GetAllLeaderboardsAsync: GameId is empty. Check your IntelliVerseXConfig.");
                    return null;
                }

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

                    limit = limit,
                    offset = 0
                };

                var jsonPayload = JsonConvert.SerializeObject(payload);
                Log($"GetAllLeaderboardsAsync JSON → {jsonPayload}");

                var client = GetClient(mgr);
                var session = GetSession(mgr);
                
                var rpcResponse = await client.RpcAsync(session, RPC_GET_ALL_LEADERBOARDS, jsonPayload);
                Log($"GetAllLeaderboardsAsync RPC payload raw → {rpcResponse.Payload}");

                var result = JsonConvert.DeserializeObject<IVXGAllLeaderboardsResponse>(rpcResponse.Payload);

                if (result == null)
                {
                    Log("GetAllLeaderboardsAsync: aggregate RPC returned null response. Trying fallback RPCs.");
                    var fallbackNull = await GetAllLeaderboardsViaPeriodRpcAsync(client, session, gameId, nakamaUserId, limit);
                    if (fallbackNull != null)
                    {
                        SafeInvoke(OnLeaderboardsFetched, fallbackNull);
                        return fallbackNull;
                    }

                    LogError("GetAllLeaderboardsAsync: RPC response deserialized to null.");
                    return null;
                }

                // Resolve game-specific leaderboards
                try
                {
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
                    Log($"GetAllLeaderboardsAsync failed: {result.error ?? "Unknown error"}. Trying fallback RPCs.");
                    var fallbackFailed = await GetAllLeaderboardsViaPeriodRpcAsync(client, session, gameId, nakamaUserId, limit);
                    if (fallbackFailed != null)
                    {
                        SafeInvoke(OnLeaderboardsFetched, fallbackFailed);
                        return fallbackFailed;
                    }

                    LogError($"GetAllLeaderboardsAsync failed: {result.error ?? "Unknown error"}");
                    return result;
                }

                Log("✓ Leaderboards fetched successfully.");
                SafeInvoke(OnLeaderboardsFetched, result);

                return result;
            }
            catch (Exception ex)
            {
                Log($"GetAllLeaderboardsAsync aggregate RPC exception: {ex.Message}. Trying fallback RPCs.");

                try
                {
                    var fallback = await GetAllLeaderboardsViaPeriodRpcAsync(
                        GetClient(mgr),
                        GetSession(mgr),
                        GetGameId(mgr),
                        ResolveNakamaUserId(mgr),
                        limit);

                    if (fallback != null)
                    {
                        SafeInvoke(OnLeaderboardsFetched, fallback);
                        return fallback;
                    }
                }
                catch (Exception fallbackEx)
                {
                    Log($"GetAllLeaderboardsAsync fallback exception: {fallbackEx.Message}");
                }

                LogError($"GetAllLeaderboardsAsync exception: {ex}");
                return null;
            }
        }

        /// <summary>
        /// Get the current player's rank.
        /// </summary>
        /// <param name="global">If true, returns global rank; otherwise game-specific rank</param>
        /// <returns>Player rank or 0 if not found</returns>
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

        /// <summary>
        /// Get the current player's best score.
        /// </summary>
        /// <param name="global">If true, returns global best; otherwise game-specific best</param>
        /// <returns>Best score or 0 if not found</returns>
        public static async Task<long> GetPlayerBestScoreAsync(bool global = false)
        {
            var mgr = GetNakamaManager();
            if (!ValidateManager(mgr, nameof(GetPlayerBestScoreAsync)))
                return 0;

            var response = await GetAllLeaderboardsAsync(limit: 100);
            if (response == null || !response.success)
                return 0;

            string ownerId = ResolveNakamaUserId(mgr);
            if (string.IsNullOrWhiteSpace(ownerId))
                return 0;

            IVXGLeaderboardData data = global ? response.global_alltime : response.alltime;
            if (data?.records == null || data.records.Count == 0)
                return 0;

            var record = data.records.FirstOrDefault(r => r.owner_id == ownerId);
            return record?.score ?? 0;
        }

        /// <summary>
        /// Reset the current win streak counter.
        /// </summary>
        public static void ResetWinStreak()
        {
            _currentWinStreak = 0;
            Log("Win streak reset.");
        }

        private static async Task<IVXGAllLeaderboardsResponse> GetAllLeaderboardsViaPeriodRpcAsync(
            IClient client,
            ISession session,
            string gameId,
            string nakamaUserId,
            int limit)
        {
            if (client == null || session == null || string.IsNullOrWhiteSpace(gameId))
                return null;

            int safeLimit = Mathf.Clamp(limit, 1, 100);
            bool anySuccess = false;
            var periods = new[] { "daily", "weekly", "monthly", "alltime" };

            var response = new IVXGAllLeaderboardsResponse
            {
                success = true,
                game_id = gameId,
                leaderboards = new Dictionary<string, IVXGLeaderboardData>(StringComparer.OrdinalIgnoreCase),
                player_ranks = new IVXGPlayerRanks()
            };

            foreach (var period in periods)
            {
                var gameData = await FetchTimePeriodLeaderboardAsync(client, session, gameId, period, "game", safeLimit);
                if (gameData != null)
                {
                    response.leaderboards[$"{gameId}_{period}"] = gameData;
                    anySuccess = true;
                    SetRankFromRecords(response.player_ranks, gameData.records, nakamaUserId, period, isGlobal: false);
                }

                var globalData = await FetchTimePeriodLeaderboardAsync(client, session, gameId, period, "global", safeLimit);
                if (globalData != null)
                {
                    response.leaderboards[$"global_{period}"] = globalData;
                    anySuccess = true;
                    SetRankFromRecords(response.player_ranks, globalData.records, nakamaUserId, period, isGlobal: true);
                }
            }

            if (!anySuccess)
            {
                return null;
            }

            response.ResolveGameLeaderboards(gameId);
            if (response.global_alltime == null &&
                response.leaderboards.TryGetValue("global_alltime", out var globalAlltime))
            {
                response.global_alltime = globalAlltime;
            }

            Log("GetAllLeaderboardsAsync fallback succeeded via get_time_period_leaderboard.");
            return response;
        }

        private static async Task<IVXGLeaderboardData> FetchTimePeriodLeaderboardAsync(
            IClient client,
            ISession session,
            string gameId,
            string period,
            string scope,
            int limit)
        {
            try
            {
                var payload = new
                {
                    game_id = gameId,
                    gameId = gameId,
                    period = period,
                    scope = scope,
                    limit = limit
                };

                var rpcResponse = await client.RpcAsync(
                    session,
                    RPC_GET_TIME_PERIOD_LEADERBOARD,
                    JsonConvert.SerializeObject(payload));

                var parsed = JsonConvert.DeserializeObject<IVXGTimePeriodLeaderboardRpcResponse>(rpcResponse.Payload);
                if (parsed == null || !parsed.success)
                {
                    Log($"Fallback RPC {RPC_GET_TIME_PERIOD_LEADERBOARD} {scope}/{period} failed: {parsed?.error ?? "null response"}");
                    return null;
                }

                var records = ConvertRecords(parsed.records);
                var ownerRecords = ConvertRecords(parsed.ownerRecords);
                if (ownerRecords.Count > 0)
                {
                    foreach (var ownerRecord in ownerRecords)
                    {
                        if (!records.Any(r => string.Equals(r.owner_id, ownerRecord.owner_id, StringComparison.Ordinal)))
                        {
                            records.Add(ownerRecord);
                        }
                    }
                }

                return new IVXGLeaderboardData
                {
                    leaderboard_id = FirstNonEmpty(
                        parsed.leaderboard_id,
                        parsed.leaderboardId,
                        BuildLeaderboardId(gameId, period, scope)),
                    period = period,
                    scope = scope,
                    records = records,
                    next_cursor = FirstNonEmpty(parsed.next_cursor, parsed.nextCursor),
                    prev_cursor = FirstNonEmpty(parsed.prev_cursor, parsed.prevCursor)
                };
            }
            catch (Exception ex)
            {
                Log($"Fallback RPC {RPC_GET_TIME_PERIOD_LEADERBOARD} {scope}/{period} exception: {ex.Message}");
                return null;
            }
        }

        private static string BuildLeaderboardId(string gameId, string period, string scope)
        {
            return string.Equals(scope, "global", StringComparison.OrdinalIgnoreCase)
                ? $"leaderboard_global_{period}"
                : $"leaderboard_{gameId}_{period}";
        }

        private static void SetRankFromRecords(
            IVXGPlayerRanks ranks,
            List<IVXGLeaderboardRecord> records,
            string nakamaUserId,
            string period,
            bool isGlobal)
        {
            if (ranks == null || records == null || string.IsNullOrWhiteSpace(nakamaUserId))
                return;

            var record = records.FirstOrDefault(r =>
                !string.IsNullOrWhiteSpace(r?.owner_id) &&
                string.Equals(r.owner_id, nakamaUserId, StringComparison.Ordinal));

            if (record == null || record.rank <= 0)
                return;

            if (isGlobal)
            {
                if (string.Equals(period, "alltime", StringComparison.OrdinalIgnoreCase))
                    ranks.global_rank = record.rank;
                return;
            }

            if (string.Equals(period, "daily", StringComparison.OrdinalIgnoreCase)) ranks.daily_rank = record.rank;
            else if (string.Equals(period, "weekly", StringComparison.OrdinalIgnoreCase)) ranks.weekly_rank = record.rank;
            else if (string.Equals(period, "monthly", StringComparison.OrdinalIgnoreCase)) ranks.monthly_rank = record.rank;
            else if (string.Equals(period, "alltime", StringComparison.OrdinalIgnoreCase)) ranks.alltime_rank = record.rank;
        }

        private static List<IVXGLeaderboardRecord> ConvertRecords(List<IVXGTimePeriodLeaderboardRecord> source)
        {
            var records = new List<IVXGLeaderboardRecord>();
            if (source == null || source.Count == 0)
                return records;

            foreach (var item in source)
            {
                if (item == null) continue;

                string ownerId = FirstNonEmpty(item.owner_id, item.ownerId);
                if (string.IsNullOrWhiteSpace(ownerId)) continue;

                string metadata = null;
                if (item.metadata != null)
                {
                    metadata = item.metadata as string ?? JsonConvert.SerializeObject(item.metadata);
                }

                records.Add(new IVXGLeaderboardRecord
                {
                    leaderboard_id = FirstNonEmpty(item.leaderboard_id, item.leaderboardId),
                    owner_id = ownerId,
                    username = item.username ?? string.Empty,
                    score = item.score,
                    subscore = item.subscore,
                    num_score = item.num_score != 0 ? item.num_score : item.numScore,
                    rank = item.rank,
                    metadata = metadata ?? "{}",
                    create_time = FirstNonEmpty(item.create_time, item.createTime),
                    update_time = FirstNonEmpty(item.update_time, item.updateTime),
                    expiry_time = FirstNonEmpty(item.expiry_time, item.expiryTime)
                });
            }

            return records;
        }

        private static string FirstNonEmpty(params string[] values)
        {
            if (values == null) return null;
            for (int i = 0; i < values.Length; i++)
            {
                if (!string.IsNullOrWhiteSpace(values[i]))
                    return values[i];
            }

            return null;
        }

        #endregion

        #region Internal Helpers - Reflection-based for Decoupling

        private static object _cachedNakamaManager;
        private static Type _nakamaManagerType;

        private static object GetNakamaManager()
        {
            if (_cachedNakamaManager != null) return _cachedNakamaManager;

            // Try to find IVXNManager via reflection
            _nakamaManagerType = FindType("IntelliVerseX.Backend.Nakama.IVXNManager");
            if (_nakamaManagerType != null)
            {
                var instanceProp = _nakamaManagerType.GetProperty("Instance", 
                    System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
                if (instanceProp != null)
                {
                    _cachedNakamaManager = instanceProp.GetValue(null);
                }
            }

            return _cachedNakamaManager;
        }

        private static bool ValidateManager(object mgr, string method)
        {
            if (mgr == null)
            {
                LogError($"{method}: Nakama Manager not found. " +
                         "Make sure IVXNManager is present in the scene and initialized before using leaderboards.");
                return false;
            }

            var isInitializedProp = mgr.GetType().GetProperty("IsInitialized");
            var isInitializingProp = mgr.GetType().GetProperty("IsInitializing");
            
            bool isInitialized = isInitializedProp?.GetValue(mgr) as bool? ?? false;
            bool isInitializing = isInitializingProp?.GetValue(mgr) as bool? ?? false;

            if (!isInitialized && !isInitializing)
            {
                Debug.LogWarning($"{LOGTAG} {method}: Nakama Manager is not initialized yet. " +
                                 "Call InitializeForCurrentUserAsync() first or let your login flow handle it.");
            }

            return true;
        }

        private static IClient GetClient(object mgr)
        {
            var clientProp = mgr.GetType().GetProperty("Client");
            return clientProp?.GetValue(mgr) as IClient;
        }

        private static ISession GetSession(object mgr)
        {
            var sessionProp = mgr.GetType().GetProperty("Session");
            return sessionProp?.GetValue(mgr) as ISession;
        }

        private static string GetGameId(object mgr)
        {
            var gameIdProp = mgr.GetType().GetProperty("GameId");
            return gameIdProp?.GetValue(mgr) as string;
        }

        private static string ResolveNakamaUserId(object mgr)
        {
            // Try from manager
            var nakamaUserIdProp = mgr?.GetType().GetProperty("NakamaUserId");
            string id = nakamaUserIdProp?.GetValue(mgr) as string;

            // Try from IVXNUserRuntime
            if (string.IsNullOrWhiteSpace(id))
            {
                var runtimeType = FindType("IntelliVerseX.Backend.Nakama.IVXNUserRuntime");
                if (runtimeType != null)
                {
                    var instanceProp = runtimeType.GetProperty("Instance", 
                        System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
                    var runtime = instanceProp?.GetValue(null);
                    if (runtime != null)
                    {
                        var runtimeIdProp = runtimeType.GetProperty("NakamaUserId");
                        id = runtimeIdProp?.GetValue(runtime) as string;
                        if (string.Equals(id, "<null>", StringComparison.OrdinalIgnoreCase))
                            id = null;
                    }
                }
            }

            return id;
        }

        private static string ResolveApiUserId()
        {
            // Try UserSessionManager.Current.userId
            var userSessionType = FindType("UserSessionManager");
            if (userSessionType != null)
            {
                var currentProp = userSessionType.GetProperty("Current", 
                    System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
                var current = currentProp?.GetValue(null);
                if (current != null)
                {
                    var userIdProp = current.GetType().GetProperty("userId");
                    if (userIdProp == null)
                        userIdProp = current.GetType().GetField("userId")?.FieldType != null ? null : null;
                    
                    var userIdField = current.GetType().GetField("userId");
                    
                    if (userIdProp != null)
                        return userIdProp.GetValue(current) as string;
                    if (userIdField != null)
                        return userIdField.GetValue(current) as string;
                }
            }
            return null;
        }

        private static string ResolveUsername(object mgr)
        {
            // Try multiple sources
            string username = null;

            // 1. From Nakama manager
            var nakamaUsernameProp = mgr?.GetType().GetProperty("NakamaUsername");
            username = nakamaUsernameProp?.GetValue(mgr) as string;
            if (!string.IsNullOrWhiteSpace(username)) return username;

            // 2. From IVXNUserRuntime
            var runtimeType = FindType("IntelliVerseX.Backend.Nakama.IVXNUserRuntime");
            if (runtimeType != null)
            {
                var instanceProp = runtimeType.GetProperty("Instance", 
                    System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
                var runtime = instanceProp?.GetValue(null);
                if (runtime != null)
                {
                    var runtimeUsernameProp = runtimeType.GetProperty("NakamaUsername");
                    username = runtimeUsernameProp?.GetValue(runtime) as string;
                    if (!string.IsNullOrWhiteSpace(username)) return username;

                    var apiUsernameProp = runtimeType.GetProperty("ApiUserName");
                    username = apiUsernameProp?.GetValue(runtime) as string;
                    if (!string.IsNullOrWhiteSpace(username)) return username;
                }
            }

            // 3. From UserSessionManager
            var userSessionType = FindType("UserSessionManager");
            if (userSessionType != null)
            {
                var currentProp = userSessionType.GetProperty("Current", 
                    System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
                var current = currentProp?.GetValue(null);
                if (current != null)
                {
                    var userNameProp = current.GetType().GetProperty("userName");
                    var userNameField = current.GetType().GetField("userName");
                    
                    if (userNameProp != null)
                        username = userNameProp.GetValue(current) as string;
                    else if (userNameField != null)
                        username = userNameField.GetValue(current) as string;
                    
                    if (!string.IsNullOrWhiteSpace(username)) return username;
                }
            }

            return "Player";
        }

        private static string ResolveDeviceIdSafe()
        {
            string id = null;

            try
            {
                // 1) Use IVXNManager stable device id (matches create_or_sync_user contract).
                var mgr = GetNakamaManager();
                if (mgr != null)
                {
                    var stableDeviceIdMethod = mgr.GetType().GetMethod(
                        "GetStableDeviceId",
                        System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
                    if (stableDeviceIdMethod != null)
                    {
                        id = stableDeviceIdMethod.Invoke(mgr, null) as string;
                        if (!string.IsNullOrWhiteSpace(id))
                        {
                            return id;
                        }
                    }
                }

                // 2) Fallback to identity model if available.
                var identityType = FindType("IntelliVerseXUserIdentity");
                if (identityType != null)
                {
                    var currentUserProp = identityType.GetProperty("CurrentUser", 
                        System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
                    var currentUser = currentUserProp?.GetValue(null);
                    if (currentUser != null)
                    {
                        var deviceIdProp = currentUser.GetType().GetProperty("DeviceId");
                        id = deviceIdProp?.GetValue(currentUser) as string;
                        if (!string.IsNullOrWhiteSpace(id))
                        {
                            return id;
                        }
                    }
                }
            }
            catch { }

            // 3) Final fallback for robustness in Editor/mobile/web builds.
            try
            {
                id = SystemInfo.deviceUniqueIdentifier;
                if (!string.IsNullOrWhiteSpace(id) &&
                    !string.Equals(id, "unknown", StringComparison.OrdinalIgnoreCase))
                {
                    return id;
                }
            }
            catch { }

            return null;
        }

        private static Type FindType(string typeName)
        {
            var type = Type.GetType(typeName);
            if (type != null) return type;

            type = Type.GetType($"{typeName}, Assembly-CSharp");
            if (type != null) return type;

            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                type = assembly.GetType(typeName);
                if (type != null) return type;
            }

            return null;
        }

        private static void LogScoreSubmissionSuccess(IVXGScoreSubmissionResponse response)
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

        [Serializable]
        private class IVXGTimePeriodLeaderboardRpcResponse
        {
            public bool success;
            public string error;
            public string leaderboardId;
            public string leaderboard_id;
            public string period;
            public string scope;
            public List<IVXGTimePeriodLeaderboardRecord> records;
            public List<IVXGTimePeriodLeaderboardRecord> ownerRecords;
            public string prevCursor;
            public string nextCursor;
            public string prev_cursor;
            public string next_cursor;
        }

        [Serializable]
        private class IVXGTimePeriodLeaderboardRecord
        {
            public string leaderboardId;
            public string leaderboard_id;
            public string ownerId;
            public string owner_id;
            public string username;
            public long score;
            public int subscore;
            public int numScore;
            public int num_score;
            public int rank;
            public object metadata;
            public string createTime;
            public string updateTime;
            public string expiryTime;
            public string create_time;
            public string update_time;
            public string expiry_time;
        }

        #endregion
    }
}

// Extension method for EnsureValidSessionAsync via reflection
namespace IntelliVerseX.Games.Leaderboard
{
    internal static class NakamaManagerExtensions
    {
        public static async System.Threading.Tasks.Task<bool> EnsureValidSessionAsync(this object mgr)
        {
            if (mgr == null) return false;

            var method = mgr.GetType().GetMethod("EnsureValidSessionAsync");
            if (method != null)
            {
                var task = method.Invoke(mgr, null) as System.Threading.Tasks.Task<bool>;
                if (task != null)
                {
                    return await task;
                }
            }

            // Fallback: check if session exists and is not expired
            var sessionProp = mgr.GetType().GetProperty("Session");
            var session = sessionProp?.GetValue(mgr) as ISession;
            return session != null && !session.IsExpired;
        }
    }
}
