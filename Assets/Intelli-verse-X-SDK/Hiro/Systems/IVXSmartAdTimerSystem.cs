using System;
using System.Threading.Tasks;

namespace IntelliVerseX.Hiro.Systems
{
    /// <summary>
    /// Server-authoritative smart ad timer system.
    /// Manages interstitial cooldowns, rewarded ad daily caps, and banner eligibility
    /// to balance revenue with player experience.
    /// </summary>
    public sealed class IVXSmartAdTimerSystem
    {
        private const string RPC_GET = "hiro_smart_ad_timer_get";
        private const string RPC_RECORD = "hiro_smart_ad_timer_record";
        private const string RPC_CAN_SHOW = "hiro_smart_ad_timer_can_show";

        private readonly IVXHiroRpcClient _rpc;

        public IVXSmartAdTimerSystem(IVXHiroRpcClient rpc)
        {
            _rpc = rpc ?? throw new ArgumentNullException(nameof(rpc));
        }

        /// <summary>
        /// Get the current ad timer state including cooldowns, daily caps, and banner status.
        /// </summary>
        public async Task<IVXSmartAdTimerState> GetAsync()
        {
            var r = await _rpc.CallAsync<IVXSmartAdTimerState>(RPC_GET);
            return r.success ? r.data : new IVXSmartAdTimerState();
        }

        /// <summary>
        /// Check if a specific ad type can be shown right now.
        /// Returns the timer state; check <c>nextInterstitialAt</c> or
        /// <c>rewardedAdsToday &lt; maxRewardedAdsPerDay</c>.
        /// </summary>
        /// <param name="adType">"interstitial", "rewarded", or "banner".</param>
        public async Task<IVXSmartAdTimerState> CanShowAsync(string adType)
        {
            var r = await _rpc.CallAsync<IVXSmartAdTimerState>(RPC_CAN_SHOW, new { adType });
            return r.success ? r.data : null;
        }

        /// <summary>
        /// Record an ad impression. The server updates counters, cooldowns,
        /// and returns any associated reward (for rewarded ads).
        /// </summary>
        /// <param name="adType">"interstitial", "rewarded", or "banner".</param>
        /// <param name="placementId">Ad placement identifier for analytics.</param>
        public async Task<IVXSmartAdTimerRecordResponse> RecordImpressionAsync(string adType, string placementId = null)
        {
            var r = await _rpc.CallAsync<IVXSmartAdTimerRecordResponse>(
                RPC_RECORD,
                new { adType, placementId });
            return r.success ? r.data : null;
        }
    }
}
