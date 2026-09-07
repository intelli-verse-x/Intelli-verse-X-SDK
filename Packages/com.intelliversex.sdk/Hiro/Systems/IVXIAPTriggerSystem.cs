using System;
using System.Threading.Tasks;

namespace IntelliVerseX.Hiro.Systems
{
    /// <summary>
    /// Server-authoritative IAP trigger system.
    /// Evaluates player behaviour signals (win-streaks, low currency, session depth)
    /// and returns contextual purchase offers at optimal moments.
    /// </summary>
    public sealed class IVXIAPTriggerSystem
    {
        private const string RPC_EVALUATE = "hiro_iap_trigger_evaluate";
        private const string RPC_DISMISS = "hiro_iap_trigger_dismiss";
        private const string RPC_CONVERT = "hiro_iap_trigger_convert";

        private readonly IVXHiroRpcClient _rpc;

        public IVXIAPTriggerSystem(IVXHiroRpcClient rpc)
        {
            _rpc = rpc ?? throw new ArgumentNullException(nameof(rpc));
        }

        /// <summary>
        /// Evaluate the current session context and return any triggered offers.
        /// Call at natural break points (post-game, store visit, level-up).
        /// </summary>
        /// <param name="context">Contextual signal: "post_game", "store_visit", "low_currency", "win_streak", "level_up".</param>
        /// <param name="contextValue">Optional numeric context value (e.g. streak count, currency balance).</param>
        public async Task<IVXIAPTriggerEvalResponse> EvaluateAsync(string context, int contextValue = 0)
        {
            var r = await _rpc.CallAsync<IVXIAPTriggerEvalResponse>(
                RPC_EVALUATE,
                new { context, contextValue });
            return r.success ? r.data : new IVXIAPTriggerEvalResponse();
        }

        /// <summary>
        /// Record that the player dismissed a triggered offer.
        /// The server updates cooldown timers to avoid over-prompting.
        /// </summary>
        public async Task<IVXIAPTriggerDismissResponse> DismissAsync(string triggerId)
        {
            var r = await _rpc.CallAsync<IVXIAPTriggerDismissResponse>(
                RPC_DISMISS,
                new { triggerId });
            return r.success ? r.data : null;
        }

        /// <summary>
        /// Record a successful IAP conversion from a triggered offer.
        /// The server credits rewards and updates the trigger profile.
        /// </summary>
        /// <param name="triggerId">The trigger that led to the purchase.</param>
        /// <param name="receipt">Platform IAP receipt for server validation.</param>
        public async Task<bool> RecordConversionAsync(string triggerId, string receipt)
        {
            return await _rpc.CallVoidAsync(RPC_CONVERT, new { triggerId, receipt });
        }
    }
}
