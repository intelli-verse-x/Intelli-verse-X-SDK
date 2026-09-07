using System;
using System.Threading.Tasks;

namespace IntelliVerseX.Hiro.Systems
{
    public sealed class IVXIncentivesSystem
    {
        private const string RPC_REFERRAL_CODE = "hiro_incentives_referral_code";
        private const string RPC_APPLY_REFERRAL = "hiro_incentives_apply_referral";
        private const string RPC_RETURN_BONUS = "hiro_incentives_return_bonus";

        private readonly IVXHiroRpcClient _rpc;

        public IVXIncentivesSystem(IVXHiroRpcClient rpc) { _rpc = rpc ?? throw new ArgumentNullException(nameof(rpc)); }

        public async Task<IVXReferralCodeResponse> GetReferralCodeAsync(string gameId = null)
        {
            var r = await _rpc.CallAsync<IVXReferralCodeResponse>(RPC_REFERRAL_CODE, new { gameId });
            return r.success ? r.data : null;
        }

        public async Task<IVXApplyReferralResponse> ApplyReferralCodeAsync(string code, string gameId = null)
        {
            var r = await _rpc.CallAsync<IVXApplyReferralResponse>(RPC_APPLY_REFERRAL, new { code, gameId });
            return r.success ? r.data : null;
        }

        public async Task<IVXReturnBonusResponse> CheckReturnBonusAsync(string gameId = null)
        {
            var r = await _rpc.CallAsync<IVXReturnBonusResponse>(RPC_RETURN_BONUS, new { gameId });
            return r.success ? r.data : null;
        }
    }
}
