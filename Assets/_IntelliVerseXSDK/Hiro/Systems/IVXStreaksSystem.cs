using System;
using System.Threading.Tasks;

namespace IntelliVerseX.Hiro.Systems
{
    public sealed class IVXStreaksSystem
    {
        private const string RPC_GET = "hiro_streaks_get";
        private const string RPC_UPDATE = "hiro_streaks_update";
        private const string RPC_CLAIM = "hiro_streaks_claim";

        private readonly IVXHiroRpcClient _rpc;

        public IVXStreaksSystem(IVXHiroRpcClient rpc) { _rpc = rpc ?? throw new ArgumentNullException(nameof(rpc)); }

        public async Task<IVXStreaksGetResponse> GetAsync(string gameId = null)
        {
            var r = await _rpc.CallAsync<IVXStreaksGetResponse>(RPC_GET, new { gameId });
            return r.success ? r.data : new IVXStreaksGetResponse();
        }

        public async Task<IVXStreak> UpdateAsync(string streakId, string gameId = null)
        {
            var r = await _rpc.CallAsync<IVXStreak>(RPC_UPDATE, new { streakId, gameId });
            return r.success ? r.data : null;
        }

        public async Task<IVXStreakClaimResponse> ClaimMilestoneAsync(string streakId, int milestone, string gameId = null)
        {
            var r = await _rpc.CallAsync<IVXStreakClaimResponse>(RPC_CLAIM, new { streakId, milestone, gameId });
            return r.success ? r.data : null;
        }
    }
}
