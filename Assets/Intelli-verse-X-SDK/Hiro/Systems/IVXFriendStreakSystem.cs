using System;
using System.Threading.Tasks;

namespace IntelliVerseX.Hiro.Systems
{
    /// <summary>
    /// Server-authoritative bilateral friend streak system.
    /// Tracks daily interaction streaks between friend pairs where both players
    /// must contribute each day to maintain the streak.
    /// </summary>
    public sealed class IVXFriendStreakSystem
    {
        private const string RPC_GET = "hiro_friend_streak_get";
        private const string RPC_INTERACT = "hiro_friend_streak_interact";
        private const string RPC_CLAIM_MILESTONE = "hiro_friend_streak_claim_milestone";

        private readonly IVXHiroRpcClient _rpc;

        public IVXFriendStreakSystem(IVXHiroRpcClient rpc)
        {
            _rpc = rpc ?? throw new ArgumentNullException(nameof(rpc));
        }

        /// <summary>
        /// Get all friend streaks for the authenticated user.
        /// </summary>
        public async Task<IVXFriendStreakState> GetAsync()
        {
            var r = await _rpc.CallAsync<IVXFriendStreakState>(RPC_GET);
            return r.success ? r.data : new IVXFriendStreakState();
        }

        /// <summary>
        /// Record a daily interaction with a friend to maintain or advance the streak.
        /// Both players must interact each day; the server checks bilateral contribution.
        /// </summary>
        /// <param name="friendId">The friend to interact with.</param>
        public async Task<IVXFriendStreakInteractResponse> InteractAsync(string friendId)
        {
            var r = await _rpc.CallAsync<IVXFriendStreakInteractResponse>(
                RPC_INTERACT,
                new { friendId });
            return r.success ? r.data : null;
        }

        /// <summary>
        /// Claim a milestone reward for reaching a streak day threshold.
        /// </summary>
        /// <param name="streakId">The streak to claim for.</param>
        /// <param name="day">The milestone day (e.g. 3, 7, 14, 30).</param>
        public async Task<IVXFriendStreakInteractResponse> ClaimMilestoneAsync(string streakId, int day)
        {
            var r = await _rpc.CallAsync<IVXFriendStreakInteractResponse>(
                RPC_CLAIM_MILESTONE,
                new { streakId, day });
            return r.success ? r.data : null;
        }
    }
}
