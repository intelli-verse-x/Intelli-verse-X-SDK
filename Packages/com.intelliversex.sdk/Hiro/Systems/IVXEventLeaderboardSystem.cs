using System;
using System.Threading.Tasks;

namespace IntelliVerseX.Hiro.Systems
{
    public sealed class IVXEventLeaderboardSystem
    {
        private const string RPC_LIST = "hiro_event_lb_list";
        private const string RPC_SUBMIT = "hiro_event_lb_submit";
        private const string RPC_CLAIM = "hiro_event_lb_claim";

        private readonly IVXHiroRpcClient _rpc;

        public IVXEventLeaderboardSystem(IVXHiroRpcClient rpc) { _rpc = rpc ?? throw new ArgumentNullException(nameof(rpc)); }

        public async Task<IVXEventLeaderboardListResponse> ListAsync(string gameId = null)
        {
            var r = await _rpc.CallAsync<IVXEventLeaderboardListResponse>(RPC_LIST, new { gameId });
            return r.success ? r.data : new IVXEventLeaderboardListResponse();
        }

        public async Task<IVXEventLeaderboardSubmitResponse> SubmitScoreAsync(string eventId, long score, string gameId = null)
        {
            var r = await _rpc.CallAsync<IVXEventLeaderboardSubmitResponse>(RPC_SUBMIT, new { eventId, score, gameId });
            return r.success ? r.data : null;
        }

        public async Task<IVXReward> ClaimAsync(string eventId, string gameId = null)
        {
            var r = await _rpc.CallAsync<IVXReward>(RPC_CLAIM, new { eventId, gameId });
            return r.success ? r.data : null;
        }
    }
}
