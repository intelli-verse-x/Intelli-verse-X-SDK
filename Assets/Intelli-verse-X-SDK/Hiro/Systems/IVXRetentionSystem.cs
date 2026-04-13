using System;
using System.Threading.Tasks;

namespace IntelliVerseX.Hiro.Systems
{
    /// <summary>
    /// Server-authoritative retention system.
    /// Tracks session depth, churn risk, onboarding progress, and comeback bonuses.
    /// </summary>
    public sealed class IVXRetentionSystem
    {
        private const string RPC_GET = "hiro_retention_get";
        private const string RPC_HEARTBEAT = "hiro_retention_heartbeat";
        private const string RPC_COMPLETE_ONBOARDING = "hiro_retention_complete_onboarding";
        private const string RPC_CLAIM_COMEBACK = "hiro_retention_claim_comeback";

        private readonly IVXHiroRpcClient _rpc;

        public IVXRetentionSystem(IVXHiroRpcClient rpc)
        {
            _rpc = rpc ?? throw new ArgumentNullException(nameof(rpc));
        }

        /// <summary>
        /// Get the current retention state for the authenticated user.
        /// </summary>
        public async Task<IVXRetentionState> GetAsync()
        {
            var r = await _rpc.CallAsync<IVXRetentionState>(RPC_GET);
            return r.success ? r.data : new IVXRetentionState();
        }

        /// <summary>
        /// Send a session heartbeat. The server updates session depth,
        /// recalculates churn risk, and may return time-gated rewards.
        /// Call once per session start and periodically during long sessions.
        /// </summary>
        public async Task<IVXRetentionHeartbeatResponse> HeartbeatAsync()
        {
            var r = await _rpc.CallAsync<IVXRetentionHeartbeatResponse>(RPC_HEARTBEAT);
            return r.success ? r.data : null;
        }

        /// <summary>
        /// Mark an onboarding step as complete. The server advances the step counter
        /// and awards step rewards when applicable.
        /// </summary>
        public async Task<IVXRetentionState> CompleteOnboardingStepAsync(int step)
        {
            var r = await _rpc.CallAsync<IVXRetentionState>(RPC_COMPLETE_ONBOARDING, new { step });
            return r.success ? r.data : null;
        }

        /// <summary>
        /// Claim a comeback bonus after an absence period.
        /// Only succeeds if the server determines a bonus is available.
        /// </summary>
        public async Task<IVXRetentionHeartbeatResponse> ClaimComebackBonusAsync()
        {
            var r = await _rpc.CallAsync<IVXRetentionHeartbeatResponse>(RPC_CLAIM_COMEBACK);
            return r.success ? r.data : null;
        }
    }
}
