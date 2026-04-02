using System;
using System.Threading.Tasks;

namespace IntelliVerseX.Hiro.Systems
{
    /// <summary>
    /// Server-authoritative limited daily content system.
    /// Provides a rotating set of time-gated content slots (daily challenges, deals,
    /// quizzes, reward chests) that reset on a server-controlled cadence.
    /// Creates habitual return behaviour through scarcity and FOMO.
    /// </summary>
    public sealed class IVXLimitedDailyContentSystem
    {
        private const string RPC_GET = "hiro_daily_content_get";
        private const string RPC_CLAIM = "hiro_daily_content_claim";

        private readonly IVXHiroRpcClient _rpc;

        public IVXLimitedDailyContentSystem(IVXHiroRpcClient rpc)
        {
            _rpc = rpc ?? throw new ArgumentNullException(nameof(rpc));
        }

        /// <summary>
        /// Fetch today's available daily content slots for the authenticated user.
        /// Returns all slots regardless of claim status so the client can render
        /// the full grid with locked/claimed states.
        /// </summary>
        public async Task<IVXDailyContentState> GetAsync()
        {
            var r = await _rpc.CallAsync<IVXDailyContentState>(RPC_GET);
            return r.success ? r.data : new IVXDailyContentState();
        }

        /// <summary>
        /// Claim or complete a daily content slot within its availability window.
        /// The server validates the window is open, the slot has not already been
        /// claimed, and any required action (e.g. watching a video, completing a
        /// quiz) has been fulfilled via <paramref name="actionPayload"/>.
        /// </summary>
        /// <param name="slotId">The slot to claim.</param>
        /// <param name="actionPayload">
        /// Optional proof-of-action data (quiz answers, ad callback token, etc.).
        /// Pass <c>null</c> when the slot does not require an action.
        /// </param>
        public async Task<IVXDailyContentClaimResponse> ClaimAsync(
            string slotId,
            string actionPayload = null)
        {
            var r = await _rpc.CallAsync<IVXDailyContentClaimResponse>(
                RPC_CLAIM,
                new { slotId, actionPayload });
            return r.success ? r.data : null;
        }
    }
}
