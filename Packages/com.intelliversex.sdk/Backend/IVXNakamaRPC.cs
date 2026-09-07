// IVXNakamaRPC.cs
// Generic Nakama RPC helper for IntelliVerseX SDK
// Provides reusable RPC calling logic and common RPCs across all games

using System;
using System.Threading.Tasks;
using UnityEngine;
using Newtonsoft.Json;
using Nakama;

namespace IntelliVerseX.Backend
{
    /// <summary>
    /// Generic RPC helper for Nakama backend calls.
    /// All games can use these methods for common operations.
    /// Game-specific RPCs should extend this in their own namespace.
    /// </summary>
    public static class IVXNakamaRPC
    {
        /// <summary>
        /// Generic RPC caller with automatic JSON serialization/deserialization.
        /// </summary>
        /// <typeparam name="TRequest">Request payload type</typeparam>
        /// <typeparam name="TResponse">Expected response type</typeparam>
        /// <param name="client">Nakama client</param>
        /// <param name="session">Current session</param>
        /// <param name="rpcId">RPC endpoint ID</param>
        /// <param name="payload">Request payload object</param>
        /// <returns>Deserialized response</returns>
        public static async Task<TResponse> CallRPC<TRequest, TResponse>(
            IClient client,
            ISession session,
            string rpcId,
            TRequest payload)
        {
            if (client == null)
                throw new ArgumentNullException(nameof(client), "Nakama client is null");
            
            if (session == null || session.IsExpired)
                throw new InvalidOperationException("Nakama session is null or expired");

            try
            {
                string jsonPayload = JsonConvert.SerializeObject(payload);
                Debug.Log($"[IVXNakamaRPC] Calling {rpcId}...");
                
                var result = await client.RpcAsync(session, rpcId, jsonPayload);
                
                if (string.IsNullOrEmpty(result.Payload))
                {
                    Debug.LogWarning($"[IVXNakamaRPC] {rpcId} returned empty payload");
                    return default(TResponse);
                }
                
                var response = JsonConvert.DeserializeObject<TResponse>(result.Payload);
                Debug.Log($"[IVXNakamaRPC] {rpcId} completed successfully");
                
                return response;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[IVXNakamaRPC] {rpcId} failed: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Generic RPC caller without request payload.
        /// </summary>
        public static async Task<TResponse> CallRPC<TResponse>(
            IClient client,
            ISession session,
            string rpcId)
        {
            return await CallRPC<object, TResponse>(client, session, rpcId, new { });
        }

        // ============================================================================
        // COMMON RPCs - Daily Rewards
        // ============================================================================

        private const string RPC_DAILY_REWARDS_GET_STATUS = "daily_rewards_get_status";
        private const string RPC_DAILY_REWARDS_CLAIM = "daily_rewards_claim";

        /// <summary>
        /// Get current daily reward status for the game.
        /// Shows current streak, next reward, and whether user can claim today.
        /// </summary>
        public static async Task<DailyRewardStatus> GetDailyRewardStatus(
            IClient client,
            ISession session,
            string gameId)
        {
            var payload = new { gameId };
            return await CallRPC<object, DailyRewardStatus>(
                client, session, RPC_DAILY_REWARDS_GET_STATUS, payload);
        }

        /// <summary>
        /// Claim today's daily reward.
        /// Can only be claimed once per day (UTC timezone).
        /// </summary>
        public static async Task<DailyRewardClaim> ClaimDailyReward(
            IClient client,
            ISession session,
            string gameId)
        {
            var payload = new { gameId };
            return await CallRPC<object, DailyRewardClaim>(
                client, session, RPC_DAILY_REWARDS_CLAIM, payload);
        }

        // ============================================================================
        // COMMON RPCs - Daily Missions
        // ============================================================================

        private const string RPC_GET_DAILY_MISSIONS = "get_daily_missions";
        private const string RPC_SUBMIT_MISSION_PROGRESS = "submit_mission_progress";
        private const string RPC_CLAIM_MISSION_REWARD = "claim_mission_reward";

        /// <summary>
        /// Get today's daily missions for the current game.
        /// Returns all missions with current progress and completion status.
        /// </summary>
        public static async Task<DailyMissionsResponse> GetDailyMissions(
            IClient client,
            ISession session,
            string gameId)
        {
            var payload = new { gameId };
            return await CallRPC<object, DailyMissionsResponse>(
                client, session, RPC_GET_DAILY_MISSIONS, payload);
        }

        /// <summary>
        /// Submit progress for a specific mission.
        /// </summary>
        public static async Task<MissionProgressResponse> SubmitMissionProgress(
            IClient client,
            ISession session,
            string gameId,
            string missionId,
            int progressValue)
        {
            var payload = new { gameId, missionId, progress = progressValue };
            return await CallRPC<object, MissionProgressResponse>(
                client, session, RPC_SUBMIT_MISSION_PROGRESS, payload);
        }

        /// <summary>
        /// Claim reward for a completed mission.
        /// </summary>
        public static async Task<MissionRewardResponse> ClaimMissionReward(
            IClient client,
            ISession session,
            string gameId,
            string missionId)
        {
            var payload = new { gameId, missionId };
            return await CallRPC<object, MissionRewardResponse>(
                client, session, RPC_CLAIM_MISSION_REWARD, payload);
        }
    }

    // ============================================================================
    // COMMON DATA MODELS - Daily Rewards
    // ============================================================================

    [Serializable]
    public class DailyRewardStatus
    {
        public bool success;
        public string error;
        public int currentStreak;
        public int totalClaims;
        public bool canClaimToday;
        public string claimReason;
        public DailyReward nextReward;
        public string lastClaimDate;
    }

    [Serializable]
    public class DailyReward
    {
        public int day;
        public int xp;
        public int tokens;
        public string multiplier;
        public string nft;
    }

    [Serializable]
    public class DailyRewardClaim
    {
        public bool success;
        public string error;
        public DailyReward reward;
        public int newStreak;
        public int totalClaims;
        public string nextClaimDate;
    }

    // ============================================================================
    // COMMON DATA MODELS - Daily Missions
    // ============================================================================

    [Serializable]
    public class DailyMissionsResponse
    {
        public bool success;
        public string error;
        public DailyMission[] missions;
        public string resetTime;
    }

    [Serializable]
    public class DailyMission
    {
        public string id;
        public string title;
        public string description;
        public string type;
        public int targetValue;
        public int currentProgress;
        public bool completed;
        public bool claimed;
        public MissionReward reward;
    }

    [Serializable]
    public class MissionReward
    {
        public int xp;
        public int tokens;
    }

    [Serializable]
    public class MissionProgressResponse
    {
        public bool success;
        public string error;
        public int newProgress;
        public bool completed;
    }

    [Serializable]
    public class MissionRewardResponse
    {
        public bool success;
        public string error;
        public MissionReward reward;
        public int totalXP;
        public int totalTokens;
    }
}
