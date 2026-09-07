using System;
using System.Threading.Tasks;

namespace IntelliVerseX.Hiro.Systems
{
    public sealed class IVXChallengesSystem
    {
        private const string RPC_CREATE = "hiro_challenges_create";
        private const string RPC_JOIN = "hiro_challenges_join";
        private const string RPC_SUBMIT = "hiro_challenges_submit";
        private const string RPC_CLAIM = "hiro_challenges_claim";

        private readonly IVXHiroRpcClient _rpc;

        public IVXChallengesSystem(IVXHiroRpcClient rpc) { _rpc = rpc ?? throw new ArgumentNullException(nameof(rpc)); }

        public async Task<IVXChallenge> CreateAsync(string challengeId, string gameId = null)
        {
            var r = await _rpc.CallAsync<IVXChallenge>(RPC_CREATE, new { challengeId, gameId });
            return r.success ? r.data : null;
        }

        public async Task<IVXChallenge> JoinAsync(string instanceId, string gameId = null)
        {
            var r = await _rpc.CallAsync<IVXChallenge>(RPC_JOIN, new { instanceId, gameId });
            return r.success ? r.data : null;
        }

        public async Task<IVXChallengeSubmitResponse> SubmitScoreAsync(string instanceId, long score, string gameId = null)
        {
            var r = await _rpc.CallAsync<IVXChallengeSubmitResponse>(RPC_SUBMIT, new { instanceId, score, gameId });
            return r.success ? r.data : null;
        }

        public async Task<IVXChallengeClaimResponse> ClaimAsync(string instanceId, string gameId = null)
        {
            var r = await _rpc.CallAsync<IVXChallengeClaimResponse>(RPC_CLAIM, new { instanceId, gameId });
            return r.success ? r.data : null;
        }
    }
}
