using System;
using System.Threading.Tasks;

namespace IntelliVerseX.Hiro.Systems
{
    /// <summary>
    /// Server-authoritative spin wheel (Lucky Wheel / Fortune Wheel) system.
    /// The server determines the winning segment via weighted random selection
    /// and credits rewards. Supports free spins, ad-gated spins, and currency-cost spins.
    /// </summary>
    public sealed class IVXSpinWheelSystem
    {
        private const string RPC_GET = "hiro_spin_wheel_get";
        private const string RPC_SPIN = "hiro_spin_wheel_spin";

        private readonly IVXHiroRpcClient _rpc;

        public IVXSpinWheelSystem(IVXHiroRpcClient rpc)
        {
            _rpc = rpc ?? throw new ArgumentNullException(nameof(rpc));
        }

        /// <summary>
        /// Get the spin wheel configuration including segments, free spin availability,
        /// and ad-gated spin counts.
        /// </summary>
        /// <param name="wheelId">Optional wheel ID; defaults to the primary wheel.</param>
        public async Task<IVXSpinWheelConfig> GetAsync(string wheelId = null)
        {
            var r = await _rpc.CallAsync<IVXSpinWheelConfig>(RPC_GET, new { wheelId });
            return r.success ? r.data : new IVXSpinWheelConfig();
        }

        /// <summary>
        /// Execute a spin. The server picks the winning segment, credits rewards,
        /// and decrements the appropriate spin counter.
        /// </summary>
        /// <param name="wheelId">Optional wheel ID; defaults to the primary wheel.</param>
        /// <param name="spinType">"free", "ad", or "currency".</param>
        public async Task<IVXSpinWheelResult> SpinAsync(string spinType = "free", string wheelId = null)
        {
            var r = await _rpc.CallAsync<IVXSpinWheelResult>(
                RPC_SPIN,
                new { wheelId, spinType });
            return r.success ? r.data : null;
        }
    }
}
