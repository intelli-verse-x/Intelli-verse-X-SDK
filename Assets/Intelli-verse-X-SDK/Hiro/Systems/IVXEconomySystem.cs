using System;
using System.Threading.Tasks;

namespace IntelliVerseX.Hiro.Systems
{
    public sealed class IVXEconomySystem
    {
        private const string RPC_DONATION_REQUEST = "hiro_economy_donation_request";
        private const string RPC_DONATION_GIVE = "hiro_economy_donation_give";
        private const string RPC_DONATION_CLAIM = "hiro_economy_donation_claim";
        private const string RPC_REWARDED_VIDEO = "hiro_economy_rewarded_video";

        private readonly IVXHiroRpcClient _rpc;

        public IVXEconomySystem(IVXHiroRpcClient rpc) { _rpc = rpc ?? throw new ArgumentNullException(nameof(rpc)); }

        public async Task<IVXDonation> RequestDonationAsync(string donationId, string gameId = null)
        {
            var r = await _rpc.CallAsync<IVXDonation>(RPC_DONATION_REQUEST, new { donationId, gameId });
            return r.success ? r.data : null;
        }

        public async Task<IVXDonationGiveResponse> GiveDonationAsync(string targetUserId, string donationId, int amount = 1, string gameId = null)
        {
            var r = await _rpc.CallAsync<IVXDonationGiveResponse>(RPC_DONATION_GIVE, new { targetUserId, donationId, amount, gameId });
            return r.success ? r.data : null;
        }

        public async Task<IVXReward> ClaimDonationsAsync(string[] donationIds, string gameId = null)
        {
            var r = await _rpc.CallAsync<IVXReward>(RPC_DONATION_CLAIM, new { donationIds, gameId });
            return r.success ? r.data : null;
        }

        public async Task<IVXRewardedVideoResponse> CompleteRewardedVideoAsync(string gameId = null)
        {
            var r = await _rpc.CallAsync<IVXRewardedVideoResponse>(RPC_REWARDED_VIDEO, new { gameId });
            return r.success ? r.data : null;
        }
    }
}
