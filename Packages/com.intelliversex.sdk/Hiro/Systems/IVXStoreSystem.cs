using System;
using System.Threading.Tasks;

namespace IntelliVerseX.Hiro.Systems
{
    public sealed class IVXStoreSystem
    {
        private const string RPC_LIST = "hiro_store_list";
        private const string RPC_PURCHASE = "hiro_store_purchase";

        private readonly IVXHiroRpcClient _rpc;

        public IVXStoreSystem(IVXHiroRpcClient rpc) { _rpc = rpc ?? throw new ArgumentNullException(nameof(rpc)); }

        public async Task<IVXStoreListResponse> ListAsync(string gameId = null)
        {
            var r = await _rpc.CallAsync<IVXStoreListResponse>(RPC_LIST, new { gameId });
            return r.success ? r.data : new IVXStoreListResponse();
        }

        public async Task<IVXStorePurchaseResponse> PurchaseAsync(string sectionId, string itemId, string gameId = null)
        {
            var r = await _rpc.CallAsync<IVXStorePurchaseResponse>(RPC_PURCHASE, new { sectionId, itemId, gameId });
            return r.success ? r.data : null;
        }
    }
}
