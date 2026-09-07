using System;
using System.Threading.Tasks;

namespace IntelliVerseX.Hiro.Systems
{
    /// <summary>
    /// Manages daily / weekly streak tracking, updates, and milestone claims via Hiro RPCs.
    /// </summary>
    public sealed class IVXStreaksSystem
    {
        private const string RPC_GET = "hiro_streaks_get";
        private const string RPC_UPDATE = "hiro_streaks_update";
        private const string RPC_CLAIM = "hiro_streaks_claim";

        private readonly IVXHiroRpcClient _rpc;

        /// <summary>
        /// Creates a new streaks system backed by the given RPC client.
        /// </summary>
        /// <param name="rpc">The Hiro RPC client for server communication.</param>
        public IVXStreaksSystem(IVXHiroRpcClient rpc) { _rpc = rpc ?? throw new ArgumentNullException(nameof(rpc)); }

        /// <summary>
        /// Retrieves all active streaks for the current player.
        /// </summary>
        /// <param name="gameId">Optional game identifier to scope the request.</param>
        public async Task<IVXStreaksGetResponse> GetAsync(string gameId = null)
        {
            var r = await _rpc.CallAsync<IVXStreaksGetResponse>(RPC_GET, new { gameId });
            return r.success ? r.data : new IVXStreaksGetResponse();
        }

        /// <summary>
        /// Records a streak increment for the specified streak.
        /// </summary>
        /// <param name="streakId">The identifier of the streak to update.</param>
        /// <param name="gameId">Optional game identifier to scope the request.</param>
        public async Task<IVXStreak> UpdateAsync(string streakId, string gameId = null)
        {
            var r = await _rpc.CallAsync<IVXStreak>(RPC_UPDATE, new { streakId, gameId });
            return r.success ? r.data : null;
        }

        /// <summary>
        /// Claims the reward for reaching a streak milestone.
        /// </summary>
        /// <param name="streakId">The identifier of the streak.</param>
        /// <param name="milestone">The milestone threshold to claim.</param>
        /// <param name="gameId">Optional game identifier to scope the request.</param>
        public async Task<IVXStreakClaimResponse> ClaimMilestoneAsync(string streakId, int milestone, string gameId = null)
        {
            var r = await _rpc.CallAsync<IVXStreakClaimResponse>(RPC_CLAIM, new { streakId, milestone, gameId });
            return r.success ? r.data : null;
        }
    }
}
