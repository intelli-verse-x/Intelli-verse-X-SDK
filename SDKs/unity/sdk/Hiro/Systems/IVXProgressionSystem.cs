using System;
using System.Threading.Tasks;

namespace IntelliVerseX.Hiro.Systems
{
    public sealed class IVXProgressionSystem
    {
        private const string RPC_GET = "hiro_progression_get";
        private const string RPC_ADD_XP = "hiro_progression_add_xp";

        private readonly IVXHiroRpcClient _rpc;

        public IVXProgressionSystem(IVXHiroRpcClient rpc) { _rpc = rpc ?? throw new ArgumentNullException(nameof(rpc)); }

        public async Task<IVXProgression> GetAsync(string gameId = null)
        {
            var r = await _rpc.CallAsync<IVXProgression>(RPC_GET, new { gameId });
            return r.success ? r.data : null;
        }

        public async Task<IVXProgressionXpResponse> AddXpAsync(long amount, string gameId = null)
        {
            var r = await _rpc.CallAsync<IVXProgressionXpResponse>(RPC_ADD_XP, new { amount, gameId });
            return r.success ? r.data : null;
        }
    }
}
