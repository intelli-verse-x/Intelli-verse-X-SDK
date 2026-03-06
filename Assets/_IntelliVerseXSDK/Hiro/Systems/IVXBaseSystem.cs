using System;
using System.Threading.Tasks;

namespace IntelliVerseX.Hiro.Systems
{
    public sealed class IVXBaseSystem
    {
        private const string RPC_IAP_VALIDATE = "hiro_iap_validate";
        private const string RPC_IAP_HISTORY = "hiro_iap_history";

        private readonly IVXHiroRpcClient _rpc;

        public IVXBaseSystem(IVXHiroRpcClient rpc) { _rpc = rpc ?? throw new ArgumentNullException(nameof(rpc)); }

        public async Task<IVXIAPValidateResponse> ValidateIAPAsync(
            string receipt, string storeType, string productId,
            float? price = null, string currency = null, string gameId = null)
        {
            var r = await _rpc.CallAsync<IVXIAPValidateResponse>(RPC_IAP_VALIDATE,
                new { receipt, storeType, productId, price, currency, gameId });
            return r.success ? r.data : null;
        }

        public async Task<IVXIAPHistoryResponse> GetPurchaseHistoryAsync(string gameId = null)
        {
            var r = await _rpc.CallAsync<IVXIAPHistoryResponse>(RPC_IAP_HISTORY, new { gameId });
            return r.success ? r.data : new IVXIAPHistoryResponse();
        }
    }
}
