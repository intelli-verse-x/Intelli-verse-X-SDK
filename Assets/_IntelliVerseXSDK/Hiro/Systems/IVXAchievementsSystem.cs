using System;
using System.Threading.Tasks;

namespace IntelliVerseX.Hiro.Systems
{
    public sealed class IVXAchievementsSystem
    {
        private const string RPC_LIST = "hiro_achievements_list";
        private const string RPC_PROGRESS = "hiro_achievements_progress";
        private const string RPC_CLAIM = "hiro_achievements_claim";

        private readonly IVXHiroRpcClient _rpc;

        public IVXAchievementsSystem(IVXHiroRpcClient rpc) { _rpc = rpc ?? throw new ArgumentNullException(nameof(rpc)); }

        public async Task<IVXAchievementsListResponse> ListAsync(string gameId = null)
        {
            var r = await _rpc.CallAsync<IVXAchievementsListResponse>(RPC_LIST, new { gameId });
            return r.success ? r.data : new IVXAchievementsListResponse();
        }

        public async Task<IVXAchievementProgressResponse> AddProgressAsync(string achievementId, int amount = 1, string gameId = null)
        {
            var r = await _rpc.CallAsync<IVXAchievementProgressResponse>(RPC_PROGRESS, new { achievementId, amount, gameId });
            return r.success ? r.data : null;
        }

        public async Task<IVXReward> ClaimAsync(string achievementId, string gameId = null)
        {
            var r = await _rpc.CallAsync<IVXReward>(RPC_CLAIM, new { achievementId, gameId });
            return r.success ? r.data : null;
        }
    }
}
