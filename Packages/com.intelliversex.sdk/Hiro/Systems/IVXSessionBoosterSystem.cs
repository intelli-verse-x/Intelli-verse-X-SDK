using System;
using System.Threading.Tasks;

namespace IntelliVerseX.Hiro.Systems
{
    /// <summary>
    /// Server-authoritative session booster system.
    /// Provides time-gated multiplier bonuses (XP, coins, etc.) to incentivize
    /// longer play sessions and returning at specific times.
    /// </summary>
    public sealed class IVXSessionBoosterSystem
    {
        private const string RPC_GET = "hiro_session_booster_get";
        private const string RPC_ACTIVATE = "hiro_session_booster_activate";
        private const string RPC_CLAIM_FREE = "hiro_session_booster_claim_free";

        private readonly IVXHiroRpcClient _rpc;

        public IVXSessionBoosterSystem(IVXHiroRpcClient rpc)
        {
            _rpc = rpc ?? throw new ArgumentNullException(nameof(rpc));
        }

        /// <summary>
        /// Get available and active session boosters for the player.
        /// </summary>
        public async Task<IVXSessionBoosterState> GetAsync()
        {
            var r = await _rpc.CallAsync<IVXSessionBoosterState>(RPC_GET);
            return r.success ? r.data : new IVXSessionBoosterState();
        }

        /// <summary>
        /// Activate a booster by ID. The server validates ownership and
        /// starts the duration timer.
        /// </summary>
        /// <param name="boosterId">The booster to activate.</param>
        /// <param name="source">Activation source: "inventory", "ad", or "iap".</param>
        public async Task<IVXSessionBoosterActivateResponse> ActivateAsync(string boosterId, string source = "inventory")
        {
            var r = await _rpc.CallAsync<IVXSessionBoosterActivateResponse>(
                RPC_ACTIVATE,
                new { boosterId, source });
            return r.success ? r.data : null;
        }

        /// <summary>
        /// Claim the next free time-gated booster.
        /// Returns null if no free booster is available yet.
        /// </summary>
        public async Task<IVXSessionBoosterActivateResponse> ClaimFreeAsync()
        {
            var r = await _rpc.CallAsync<IVXSessionBoosterActivateResponse>(RPC_CLAIM_FREE);
            return r.success ? r.data : null;
        }
    }
}
