using System;
using System.Threading.Tasks;

namespace IntelliVerseX.Hiro.Systems
{
    /// <summary>
    /// Server-authoritative streak shield system.
    /// Allows players to protect daily streaks from breaking with consumable shields.
    /// Shields can be replenished via rewarded ads, IAP, or in-game currency.
    /// </summary>
    public sealed class IVXStreakShieldSystem
    {
        private const string RPC_GET = "hiro_streak_shield_get";
        private const string RPC_ACTIVATE = "hiro_streak_shield_activate";
        private const string RPC_REPLENISH = "hiro_streak_shield_replenish";

        private readonly IVXHiroRpcClient _rpc;

        public IVXStreakShieldSystem(IVXHiroRpcClient rpc)
        {
            _rpc = rpc ?? throw new ArgumentNullException(nameof(rpc));
        }

        /// <summary>
        /// Get the current streak shield state (remaining shields, active status, expiry).
        /// </summary>
        public async Task<IVXStreakShieldState> GetAsync()
        {
            var r = await _rpc.CallAsync<IVXStreakShieldState>(RPC_GET);
            return r.success ? r.data : new IVXStreakShieldState();
        }

        /// <summary>
        /// Activate a streak shield. Consumes one shield from the player's inventory
        /// and protects the current streak for the configured duration.
        /// </summary>
        public async Task<IVXStreakShieldActivateResponse> ActivateAsync()
        {
            var r = await _rpc.CallAsync<IVXStreakShieldActivateResponse>(RPC_ACTIVATE);
            return r.success ? r.data : null;
        }

        /// <summary>
        /// Replenish streak shields via a specified source.
        /// </summary>
        /// <param name="source">Replenishment source: "ad", "iap", or "currency".</param>
        /// <param name="receiptOrId">IAP receipt or currency transaction ID when applicable.</param>
        public async Task<IVXStreakShieldReplenishResponse> ReplenishAsync(string source, string receiptOrId = null)
        {
            var r = await _rpc.CallAsync<IVXStreakShieldReplenishResponse>(
                RPC_REPLENISH,
                new { source, receiptOrId });
            return r.success ? r.data : null;
        }
    }
}
