using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace IntelliVerseX.Hiro.Systems
{
    public sealed class IVXInventorySystem
    {
        private const string RPC_LIST = "hiro_inventory_list";
        private const string RPC_GRANT = "hiro_inventory_grant";
        private const string RPC_CONSUME = "hiro_inventory_consume";

        private readonly IVXHiroRpcClient _rpc;

        public IVXInventorySystem(IVXHiroRpcClient rpc) { _rpc = rpc ?? throw new ArgumentNullException(nameof(rpc)); }

        public async Task<IVXInventoryListResponse> ListAsync(string category = null, string gameId = null)
        {
            var r = await _rpc.CallAsync<IVXInventoryListResponse>(RPC_LIST, new { category, gameId });
            return r.success ? r.data : new IVXInventoryListResponse();
        }

        public async Task<IVXInventoryItem> GrantAsync(string itemId, int count = 1, string gameId = null,
            Dictionary<string, string> stringProperties = null, Dictionary<string, double> numericProperties = null)
        {
            var r = await _rpc.CallAsync<IVXInventoryItem>(RPC_GRANT, new { itemId, count, gameId, stringProperties, numericProperties });
            return r.success ? r.data : null;
        }

        public async Task<bool> ConsumeAsync(string itemId, int count = 1, string gameId = null)
        {
            return await _rpc.CallVoidAsync(RPC_CONSUME, new { itemId, count, gameId });
        }
    }
}
