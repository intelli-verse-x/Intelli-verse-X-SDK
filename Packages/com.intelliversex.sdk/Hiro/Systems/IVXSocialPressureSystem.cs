using System;
using System.Threading.Tasks;

namespace IntelliVerseX.Hiro.Systems
{
    /// <summary>
    /// Server-authoritative social pressure system.
    /// Provides social proof data (recent friend achievements, active player counts,
    /// friend activity) to create urgency and drive engagement through FOMO mechanics.
    /// </summary>
    public sealed class IVXSocialPressureSystem
    {
        private const string RPC_GET = "hiro_social_pressure_get";

        private readonly IVXHiroRpcClient _rpc;

        public IVXSocialPressureSystem(IVXHiroRpcClient rpc)
        {
            _rpc = rpc ?? throw new ArgumentNullException(nameof(rpc));
        }

        /// <summary>
        /// Get social pressure data: friend activity proofs, online counts,
        /// and global player statistics for display in UI.
        /// </summary>
        /// <param name="limit">Max number of social proof items to return.</param>
        public async Task<IVXSocialPressureState> GetAsync(int limit = 10)
        {
            var r = await _rpc.CallAsync<IVXSocialPressureState>(RPC_GET, new { limit });
            return r.success ? r.data : new IVXSocialPressureState();
        }
    }
}
