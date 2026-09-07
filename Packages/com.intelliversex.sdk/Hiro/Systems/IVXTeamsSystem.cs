using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace IntelliVerseX.Hiro.Systems
{
    public sealed class IVXTeamsSystem
    {
        private const string RPC_GET = "hiro_teams_get";
        private const string RPC_STATS = "hiro_teams_stats";
        private const string RPC_WALLET_GET = "hiro_teams_wallet_get";
        private const string RPC_WALLET_UPDATE = "hiro_teams_wallet_update";
        private const string RPC_ACHIEVEMENTS = "hiro_teams_achievements";

        private readonly IVXHiroRpcClient _rpc;

        public IVXTeamsSystem(IVXHiroRpcClient rpc) { _rpc = rpc ?? throw new ArgumentNullException(nameof(rpc)); }

        public async Task<IVXTeamData> GetAsync(string groupId, string gameId = null)
        {
            var r = await _rpc.CallAsync<IVXTeamData>(RPC_GET, new { groupId, gameId });
            return r.success ? r.data : null;
        }

        public async Task<bool> UpdateStatAsync(string groupId, string statId, double value, string gameId = null)
        {
            return await _rpc.CallVoidAsync(RPC_STATS, new { groupId, statId, value, gameId });
        }

        public async Task<IVXTeamWallet> GetWalletAsync(string groupId, string gameId = null)
        {
            var r = await _rpc.CallAsync<IVXTeamWallet>(RPC_WALLET_GET, new { groupId, gameId });
            return r.success ? r.data : null;
        }

        public async Task<IVXTeamWallet> UpdateWalletAsync(string groupId, Dictionary<string, long> changeset, string gameId = null)
        {
            var r = await _rpc.CallAsync<IVXTeamWallet>(RPC_WALLET_UPDATE, new { groupId, changeset, gameId });
            return r.success ? r.data : null;
        }

        public async Task<List<IVXAchievement>> GetAchievementsAsync(string groupId, string gameId = null)
        {
            var r = await _rpc.CallAsync<List<IVXAchievement>>(RPC_ACHIEVEMENTS, new { groupId, gameId });
            return r.success ? r.data : new List<IVXAchievement>();
        }
    }
}
