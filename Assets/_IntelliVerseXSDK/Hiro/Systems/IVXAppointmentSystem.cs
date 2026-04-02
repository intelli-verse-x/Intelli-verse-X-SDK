using System;
using System.Threading.Tasks;

namespace IntelliVerseX.Hiro.Systems
{
    /// <summary>
    /// Server-authoritative appointment mechanic system.
    /// Schedules time-limited reward windows (e.g., "Daily Bonus at 8 PM")
    /// to create habitual return behaviour. Supports one-shot and recurring appointments.
    /// </summary>
    public sealed class IVXAppointmentSystem
    {
        private const string RPC_GET = "hiro_appointment_get";
        private const string RPC_CLAIM = "hiro_appointment_claim";

        private readonly IVXHiroRpcClient _rpc;

        public IVXAppointmentSystem(IVXHiroRpcClient rpc)
        {
            _rpc = rpc ?? throw new ArgumentNullException(nameof(rpc));
        }

        /// <summary>
        /// Get all appointments for the authenticated user, including active,
        /// upcoming, and recently expired windows.
        /// </summary>
        public async Task<IVXAppointmentState> GetAsync()
        {
            var r = await _rpc.CallAsync<IVXAppointmentState>(RPC_GET);
            return r.success ? r.data : new IVXAppointmentState();
        }

        /// <summary>
        /// Claim the reward for an appointment within its active window.
        /// The server validates that the window is open and the reward has not been claimed.
        /// </summary>
        /// <param name="appointmentId">The appointment to claim.</param>
        public async Task<IVXAppointmentClaimResponse> ClaimAsync(string appointmentId)
        {
            var r = await _rpc.CallAsync<IVXAppointmentClaimResponse>(
                RPC_CLAIM,
                new { appointmentId });
            return r.success ? r.data : null;
        }
    }
}
