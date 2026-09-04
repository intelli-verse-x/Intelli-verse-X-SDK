using System;
using System.Threading.Tasks;

namespace IntelliVerseX.Hiro.Systems
{
    /// <summary>
    /// Server-authoritative offerwall system.
    /// Manages third-party offerwall integrations (Tapjoy, IronSource, etc.)
    /// with server-side reward validation and credit tracking.
    /// </summary>
    public sealed class IVXOfferwallSystem
    {
        private const string RPC_GET = "hiro_offerwall_get";
        private const string RPC_COMPLETE = "hiro_offerwall_complete";
        private const string RPC_CLAIM = "hiro_offerwall_claim";

        private readonly IVXHiroRpcClient _rpc;

        public IVXOfferwallSystem(IVXHiroRpcClient rpc)
        {
            _rpc = rpc ?? throw new ArgumentNullException(nameof(rpc));
        }

        /// <summary>
        /// Get the offerwall state including available, pending, and completed offers.
        /// </summary>
        public async Task<IVXOfferwallState> GetAsync()
        {
            var r = await _rpc.CallAsync<IVXOfferwallState>(RPC_GET);
            return r.success ? r.data : new IVXOfferwallState();
        }

        /// <summary>
        /// Record an offer completion callback from the offerwall provider.
        /// Typically called from a server-to-server callback handler.
        /// </summary>
        /// <param name="offerId">The completed offer ID.</param>
        /// <param name="provider">Offerwall provider name (e.g. "tapjoy", "ironsource").</param>
        /// <param name="transactionId">Provider transaction ID for deduplication.</param>
        public async Task<IVXOfferwallCompleteResponse> CompleteOfferAsync(
            string offerId,
            string provider,
            string transactionId)
        {
            var r = await _rpc.CallAsync<IVXOfferwallCompleteResponse>(
                RPC_COMPLETE,
                new { offerId, provider, transactionId });
            return r.success ? r.data : null;
        }

        /// <summary>
        /// Claim pending offerwall rewards to the player's wallet.
        /// </summary>
        public async Task<IVXOfferwallCompleteResponse> ClaimPendingAsync()
        {
            var r = await _rpc.CallAsync<IVXOfferwallCompleteResponse>(RPC_CLAIM);
            return r.success ? r.data : null;
        }
    }
}
