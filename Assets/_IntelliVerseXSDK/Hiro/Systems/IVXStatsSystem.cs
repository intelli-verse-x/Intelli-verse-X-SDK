using System;
using System.Threading.Tasks;

namespace IntelliVerseX.Hiro.Systems
{
    public sealed class IVXStatsSystem
    {
        private const string RPC_GET = "hiro_stats_get";
        private const string RPC_UPDATE = "hiro_stats_update";

        private readonly IVXHiroRpcClient _rpc;

        public IVXStatsSystem(IVXHiroRpcClient rpc) { _rpc = rpc ?? throw new ArgumentNullException(nameof(rpc)); }

        public async Task<IVXStatsGetResponse> GetAsync(bool publicOnly = false, string gameId = null)
        {
            var r = await _rpc.CallAsync<IVXStatsGetResponse>(RPC_GET, new { publicOnly, gameId });
            return r.success ? r.data : new IVXStatsGetResponse();
        }

        public async Task<IVXStatUpdateResponse> UpdateAsync(string statId, double value, string gameId = null)
        {
            var r = await _rpc.CallAsync<IVXStatUpdateResponse>(RPC_UPDATE, new { statId, value, gameId });
            return r.success ? r.data : null;
        }
    }
}
