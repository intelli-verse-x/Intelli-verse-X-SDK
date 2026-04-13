using System;
using System.Threading.Tasks;

namespace IntelliVerseX.Hiro.Systems
{
    public sealed class IVXAuctionsSystem
    {
        private const string RPC_LIST = "hiro_auctions_list";
        private const string RPC_CREATE = "hiro_auctions_create";
        private const string RPC_BID = "hiro_auctions_bid";
        private const string RPC_RESOLVE = "hiro_auctions_resolve";

        private readonly IVXHiroRpcClient _rpc;

        public IVXAuctionsSystem(IVXHiroRpcClient rpc) { _rpc = rpc ?? throw new ArgumentNullException(nameof(rpc)); }

        public async Task<IVXAuctionListResponse> ListAsync(string category = null, string gameId = null)
        {
            var r = await _rpc.CallAsync<IVXAuctionListResponse>(RPC_LIST, new { category, gameId });
            return r.success ? r.data : new IVXAuctionListResponse();
        }

        public async Task<IVXAuctionListing> CreateAsync(string itemId, int itemCount, long startingPrice,
            string category = null, string gameId = null)
        {
            var r = await _rpc.CallAsync<IVXAuctionListing>(RPC_CREATE,
                new { itemId, itemCount, startingPrice, category, gameId });
            return r.success ? r.data : null;
        }

        public async Task<IVXAuctionBidResponse> BidAsync(string listingId, long amount, string gameId = null)
        {
            var r = await _rpc.CallAsync<IVXAuctionBidResponse>(RPC_BID, new { listingId, amount, gameId });
            return r.success ? r.data : null;
        }

        public async Task<bool> ResolveAsync(string listingId, string gameId = null)
        {
            return await _rpc.CallVoidAsync(RPC_RESOLVE, new { listingId, gameId });
        }
    }
}
