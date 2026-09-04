using System;
using System.Threading.Tasks;

namespace IntelliVerseX.Hiro.Systems
{
    public sealed class IVXTutorialsSystem
    {
        private const string RPC_GET = "hiro_tutorials_get";
        private const string RPC_ADVANCE = "hiro_tutorials_advance";

        private readonly IVXHiroRpcClient _rpc;

        public IVXTutorialsSystem(IVXHiroRpcClient rpc) { _rpc = rpc ?? throw new ArgumentNullException(nameof(rpc)); }

        public async Task<IVXTutorialsGetResponse> GetAsync(string gameId = null)
        {
            var r = await _rpc.CallAsync<IVXTutorialsGetResponse>(RPC_GET, new { gameId });
            return r.success ? r.data : new IVXTutorialsGetResponse();
        }

        public async Task<IVXTutorialAdvanceResponse> AdvanceAsync(string tutorialId, string gameId = null)
        {
            var r = await _rpc.CallAsync<IVXTutorialAdvanceResponse>(RPC_ADVANCE, new { tutorialId, gameId });
            return r.success ? r.data : null;
        }
    }
}
