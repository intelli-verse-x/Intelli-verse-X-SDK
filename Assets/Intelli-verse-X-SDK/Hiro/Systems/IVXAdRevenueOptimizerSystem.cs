using System;
using System.Threading.Tasks;

namespace IntelliVerseX.Hiro.Systems
{
    /// <summary>
    /// Server-authoritative ad revenue optimization system.
    /// Manages placement configurations, frequency caps, and reward multipliers
    /// with remote-configurable settings per player segment.
    /// </summary>
    public sealed class IVXAdRevenueOptimizerSystem
    {
        private const string RPC_GET_CONFIG = "hiro_ad_revenue_get_config";
        private const string RPC_RECORD_IMPRESSION = "hiro_ad_revenue_record_impression";

        private readonly IVXHiroRpcClient _rpc;

        public IVXAdRevenueOptimizerSystem(IVXHiroRpcClient rpc)
        {
            _rpc = rpc ?? throw new ArgumentNullException(nameof(rpc));
        }

        /// <summary>
        /// Fetch the personalized ad revenue configuration for the current player.
        /// Includes placement priorities, cooldowns, and reward multipliers that
        /// may differ by player segment (via Satori experiments/flags).
        /// </summary>
        public async Task<IVXAdRevenueConfig> GetConfigAsync()
        {
            var r = await _rpc.CallAsync<IVXAdRevenueConfig>(RPC_GET_CONFIG);
            return r.success ? r.data : new IVXAdRevenueConfig();
        }

        /// <summary>
        /// Record an ad impression for a specific placement.
        /// The server enforces frequency caps, updates analytics,
        /// and returns any associated reward.
        /// </summary>
        /// <param name="placementId">Placement ID from the config.</param>
        /// <param name="adNetwork">Ad network that served the impression (for revenue attribution).</param>
        /// <param name="revenue">Estimated revenue in USD (from mediation callback).</param>
        public async Task<IVXAdImpressionResponse> RecordImpressionAsync(
            string placementId,
            string adNetwork = null,
            double revenue = 0)
        {
            var r = await _rpc.CallAsync<IVXAdImpressionResponse>(
                RPC_RECORD_IMPRESSION,
                new { placementId, adNetwork, revenue });
            return r.success ? r.data : null;
        }
    }
}
