using System;
using System.Threading.Tasks;

namespace IntelliVerseX.Hiro.Systems
{
    /// <summary>
    /// Bilateral friend streaks on prod Nakama:
    /// <c>friend_streak_get_state</c>, <c>friend_streak_record_contribution</c>,
    /// <c>friend_streak_send_nudge</c>, <c>friend_streak_repair</c>.
    /// </summary>
    public sealed class IVXFriendStreakSystem
    {
        private const string RPC_GET = "friend_streak_get_state";
        private const string RPC_CONTRIBUTE = "friend_streak_record_contribution";
        private const string RPC_NUDGE = "friend_streak_send_nudge";
        private const string RPC_REPAIR = "friend_streak_repair";

        private readonly IVXHiroRpcClient _rpc;

        public IVXFriendStreakSystem(IVXHiroRpcClient rpc)
        {
            _rpc = rpc ?? throw new ArgumentNullException(nameof(rpc));
        }

        /// <summary>Get all friend streaks for the authenticated user.</summary>
        public async Task<IVXFriendStreakState> GetAsync()
        {
            var r = await _rpc.CallAsync<IVXFriendStreakState>(RPC_GET);
            return r.success ? (r.data ?? new IVXFriendStreakState()) : new IVXFriendStreakState();
        }

        /// <summary>Record a daily bilateral contribution with a friend.</summary>
        public async Task<IVXFriendStreakInteractResponse> InteractAsync(string friendId)
        {
            var r = await _rpc.CallAsync<IVXFriendStreakInteractResponse>(
                RPC_CONTRIBUTE,
                new { friendId });
            return r.success ? r.data : null;
        }

        /// <summary>Send a nudge to a friend about the streak.</summary>
        public async Task<bool> SendNudgeAsync(string friendId)
        {
            var r = await _rpc.CallAsync<object>(RPC_NUDGE, new { friendId });
            return r.success;
        }

        /// <summary>Repair a broken streak (prod gem cost applies server-side).</summary>
        public async Task<bool> RepairAsync(string friendId, string idempotencyKey = null)
        {
            var r = await _rpc.CallAsync<object>(
                RPC_REPAIR,
                new { friendId, idempotencyKey });
            return r.success;
        }
    }
}
