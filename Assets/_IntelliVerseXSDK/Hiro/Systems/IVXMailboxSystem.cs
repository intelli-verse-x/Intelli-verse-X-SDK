using System;
using System.Threading.Tasks;

namespace IntelliVerseX.Hiro.Systems
{
    public sealed class IVXMailboxSystem
    {
        private const string RPC_LIST = "hiro_mailbox_list";
        private const string RPC_CLAIM = "hiro_mailbox_claim";
        private const string RPC_CLAIM_ALL = "hiro_mailbox_claim_all";
        private const string RPC_DELETE = "hiro_mailbox_delete";

        private readonly IVXHiroRpcClient _rpc;

        public IVXMailboxSystem(IVXHiroRpcClient rpc) { _rpc = rpc ?? throw new ArgumentNullException(nameof(rpc)); }

        public async Task<IVXMailboxListResponse> ListAsync(string gameId = null)
        {
            var r = await _rpc.CallAsync<IVXMailboxListResponse>(RPC_LIST, new { gameId });
            return r.success ? r.data : new IVXMailboxListResponse();
        }

        public async Task<IVXMailboxClaimResponse> ClaimAsync(string messageId, string gameId = null)
        {
            var r = await _rpc.CallAsync<IVXMailboxClaimResponse>(RPC_CLAIM, new { messageId, gameId });
            return r.success ? r.data : null;
        }

        public async Task<IVXMailboxClaimAllResponse> ClaimAllAsync(string gameId = null)
        {
            var r = await _rpc.CallAsync<IVXMailboxClaimAllResponse>(RPC_CLAIM_ALL, new { gameId });
            return r.success ? r.data : null;
        }

        public async Task<bool> DeleteAsync(string messageId, string gameId = null)
        {
            return await _rpc.CallVoidAsync(RPC_DELETE, new { messageId, gameId });
        }
    }
}
