using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace IntelliVerseX.Hiro.Systems
{
    public sealed class IVXLeaderboardsSystem
    {
        private const string RPC_LIST = "hiro_leaderboards_list";
        private const string RPC_SUBMIT = "hiro_leaderboards_submit";
        private const string RPC_RECORDS = "hiro_leaderboards_records";

        private readonly IVXHiroRpcClient _rpc;

        public IVXLeaderboardsSystem(IVXHiroRpcClient rpc) { _rpc = rpc ?? throw new ArgumentNullException(nameof(rpc)); }

        public async Task<IVXLeaderboardsListResponse> ListAsync(string gameId = null)
        {
            var r = await _rpc.CallAsync<IVXLeaderboardsListResponse>(RPC_LIST, new { gameId });
            return r.success ? r.data : new IVXLeaderboardsListResponse();
        }

        public async Task<IVXLeaderboardSubmitResponse> SubmitScoreAsync(
            string leaderboardId, long score, long subscore = 0,
            Dictionary<string, object> metadata = null, Dictionary<string, string> location = null,
            string gameId = null)
        {
            var r = await _rpc.CallAsync<IVXLeaderboardSubmitResponse>(RPC_SUBMIT,
                new { leaderboardId, score, subscore, metadata, location, gameId });
            return r.success ? r.data : null;
        }

        public async Task<IVXLeaderboardRecordsResponse> GetRecordsAsync(
            string leaderboardId, int limit = 20, string cursor = null,
            Dictionary<string, string> geoFilter = null, string gameId = null)
        {
            var r = await _rpc.CallAsync<IVXLeaderboardRecordsResponse>(RPC_RECORDS,
                new { leaderboardId, limit, cursor, geoFilter, gameId });
            return r.success ? r.data : new IVXLeaderboardRecordsResponse();
        }
    }
}
