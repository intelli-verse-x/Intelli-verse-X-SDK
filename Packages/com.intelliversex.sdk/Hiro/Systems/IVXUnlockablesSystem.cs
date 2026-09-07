using System;
using System.Threading.Tasks;

namespace IntelliVerseX.Hiro.Systems
{
    public sealed class IVXUnlockablesSystem
    {
        private const string RPC_GET = "hiro_unlockables_get";
        private const string RPC_START = "hiro_unlockables_start";
        private const string RPC_CLAIM = "hiro_unlockables_claim";
        private const string RPC_BUY_SLOT = "hiro_unlockables_buy_slot";

        private readonly IVXHiroRpcClient _rpc;

        public IVXUnlockablesSystem(IVXHiroRpcClient rpc) { _rpc = rpc ?? throw new ArgumentNullException(nameof(rpc)); }

        public async Task<IVXUnlockablesGetResponse> GetAsync(string gameId = null)
        {
            var r = await _rpc.CallAsync<IVXUnlockablesGetResponse>(RPC_GET, new { gameId });
            return r.success ? r.data : new IVXUnlockablesGetResponse();
        }

        public async Task<IVXUnlockableSlot> StartUnlockAsync(int slotIndex, string unlockableId, string gameId = null)
        {
            var r = await _rpc.CallAsync<IVXUnlockableSlot>(RPC_START, new { slotIndex, unlockableId, gameId });
            return r.success ? r.data : null;
        }

        public async Task<IVXUnlockableClaimResponse> ClaimAsync(int slotIndex, string gameId = null)
        {
            var r = await _rpc.CallAsync<IVXUnlockableClaimResponse>(RPC_CLAIM, new { slotIndex, gameId });
            return r.success ? r.data : null;
        }

        public async Task<bool> BuySlotAsync(string gameId = null)
        {
            return await _rpc.CallVoidAsync(RPC_BUY_SLOT, new { gameId });
        }
    }
}
