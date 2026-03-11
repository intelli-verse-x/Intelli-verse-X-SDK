using System;
using System.Threading.Tasks;

namespace IntelliVerseX.Hiro.Systems
{
    public sealed class IVXEnergySystem
    {
        private const string RPC_GET = "hiro_energy_get";
        private const string RPC_SPEND = "hiro_energy_spend";
        private const string RPC_REFILL = "hiro_energy_refill";
        private const string RPC_ADD_MODIFIER = "hiro_energy_add_modifier";

        private readonly IVXHiroRpcClient _rpc;

        public IVXEnergySystem(IVXHiroRpcClient rpc) { _rpc = rpc ?? throw new ArgumentNullException(nameof(rpc)); }

        public async Task<IVXEnergyGetResponse> GetAsync(string gameId = null)
        {
            var r = await _rpc.CallAsync<IVXEnergyGetResponse>(RPC_GET, new { gameId });
            return r.success ? r.data : new IVXEnergyGetResponse();
        }

        public async Task<bool> SpendAsync(string energyId, int amount, string gameId = null)
        {
            return await _rpc.CallVoidAsync(RPC_SPEND, new { energyId, amount, gameId });
        }

        public async Task<IVXEnergyState> RefillAsync(string energyId, string gameId = null)
        {
            var r = await _rpc.CallAsync<IVXEnergyState>(RPC_REFILL, new { energyId, gameId });
            return r.success ? r.data : null;
        }

        public async Task<IVXEnergyModifierResponse> AddModifierAsync(
            string energyId, string type, double value, int durationSec, string gameId = null)
        {
            var r = await _rpc.CallAsync<IVXEnergyModifierResponse>(RPC_ADD_MODIFIER,
                new { energyId, type, value, durationSec, gameId });
            return r.success ? r.data : null;
        }
    }
}
