// IVXSyncTurnClient — convenience wrapper over a sync-turn match session.
//
// Game code typically wants typed events (TurnStart, TurnResolved, etc.)
// instead of raw opcodes. This helper subscribes to the sync-turn opcode
// range on the underlying IIVXMatchSession and exposes them as C# events.

using System;
using System.Threading;
using System.Threading.Tasks;
using IntelliVerseX.MultiplayerKernel.Wire;
using Newtonsoft.Json.Linq;

namespace IntelliVerseX.MultiplayerKernel.Templates.SyncTurn
{
    /// <summary>
    /// Strongly-typed binding around <see cref="IIVXMatchSession"/> for the
    /// `sync-turn-v1` template.
    /// </summary>
    public class IVXSyncTurnClient : IDisposable
    {
        private readonly IIVXMatchSession _session;
        private readonly IDisposable _subStart;
        private readonly IDisposable _subInputOpen;
        private readonly IDisposable _subInputClose;
        private readonly IDisposable _subResolved;
        private readonly IDisposable _subScore;

        public IIVXMatchSession Session => _session;
        public string MatchId => _session.MatchId;

        public event Action<IVXKernelEvent<TurnStartPayload>>       OnTurnStart;
        public event Action<IVXKernelEvent<TurnInputOpenedPayload>> OnTurnInputOpened;
        public event Action<IVXKernelEvent<TurnInputClosedPayload>> OnTurnInputClosed;
        public event Action<IVXKernelEvent<TurnResolvedPayload>>    OnTurnResolved;
        public event Action<IVXKernelEvent<ScoreUpdatePayload>>     OnScoreUpdate;

        public IVXSyncTurnClient(IIVXMatchSession session)
        {
            _session = session ?? throw new ArgumentNullException(nameof(session));
            _subStart       = session.Subscribe<TurnStartPayload>      (IVXSyncTurnOp.TURN_START,        FwdStart);
            _subInputOpen   = session.Subscribe<TurnInputOpenedPayload>(IVXSyncTurnOp.TURN_INPUT_OPENED, FwdOpen);
            _subInputClose  = session.Subscribe<TurnInputClosedPayload>(IVXSyncTurnOp.TURN_INPUT_CLOSED, FwdClose);
            _subResolved    = session.Subscribe<TurnResolvedPayload>   (IVXSyncTurnOp.TURN_RESOLVED,     FwdResolved);
            _subScore       = session.Subscribe<ScoreUpdatePayload>    (IVXSyncTurnOp.SCORE_UPDATE,      FwdScore);
        }

        private void FwdStart(IVXKernelEvent<TurnStartPayload> e)        => OnTurnStart?.Invoke(e);
        private void FwdOpen(IVXKernelEvent<TurnInputOpenedPayload> e)   => OnTurnInputOpened?.Invoke(e);
        private void FwdClose(IVXKernelEvent<TurnInputClosedPayload> e)  => OnTurnInputClosed?.Invoke(e);
        private void FwdResolved(IVXKernelEvent<TurnResolvedPayload> e)  => OnTurnResolved?.Invoke(e);
        private void FwdScore(IVXKernelEvent<ScoreUpdatePayload> e)      => OnScoreUpdate?.Invoke(e);

        /// <summary>Submit the local player's response for the current turn.</summary>
        public Task SubmitInputAsync(int turnIndex, JToken submission, int clientResponseMs, CancellationToken cancellationToken = default)
        {
            return _session.SendAsync(IVXSyncTurnOp.TURN_INPUT_SUBMIT, new TurnInputSubmitPayload
            {
                TurnIndex        = turnIndex,
                ClientResponseMs = clientResponseMs,
                Submission       = submission
            }, cancellationToken);
        }

        public Task ReadyAsync(CancellationToken cancellationToken = default)
            => _session.SendAsync(IVXSyncTurnOp.PLAYER_READY, new PlayerReadyPayload { Ready = true }, cancellationToken);

        public Task ForfeitAsync(CancellationToken cancellationToken = default)
            => _session.SendAsync(IVXSyncTurnOp.PLAYER_FORFEIT, new PlayerForfeitPayload(), cancellationToken);

        public void Dispose()
        {
            _subStart?.Dispose();
            _subInputOpen?.Dispose();
            _subInputClose?.Dispose();
            _subResolved?.Dispose();
            _subScore?.Dispose();
        }
    }
}
