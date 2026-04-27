// IIVXMatchSession — per-match session handle.
//
// Lives for the lifetime of one Nakama match. Owns the inbound dispatch
// loop, server clock skew tracking, and the outbound opcode submitter.
// Game code subscribes via Subscribe<T>(opcode, handler) and sends via
// SendAsync(opcode, payload).

using System;
using System.Threading;
using System.Threading.Tasks;

namespace IntelliVerseX.MultiplayerKernel
{
    /// <summary>
    /// Handle to one joined match. Disposing leaves the match.
    /// </summary>
    public interface IIVXMatchSession : IDisposable
    {
        /// <summary>Server-issued match identifier.</summary>
        string MatchId { get; }

        /// <summary>Stable identifier of the kernel template (e.g. <c>sync-turn-v1</c>).</summary>
        string TemplateId { get; }

        /// <summary>The user id this client was assigned by Welcome.</summary>
        string LocalUserId { get; }

        /// <summary>Live server-clock match-time in ms (ticks while connected).</summary>
        ulong CurrentMatchTimeMs { get; }

        /// <summary>Active player count (best-effort, derived from joined/left events).</summary>
        int ActivePlayerCount { get; }

        /// <summary>Underlying transport state.</summary>
        IVXTransportState State { get; }

        // ----- subscribe -----

        /// <summary>
        /// Subscribe to a single opcode. <typeparamref name="TPayload"/> is the
        /// payload type (matches the per-template proto). The returned
        /// <see cref="IDisposable"/> unsubscribes on dispose.
        /// </summary>
        IDisposable Subscribe<TPayload>(int opcode, Action<IVXKernelEvent<TPayload>> handler);

        /// <summary>
        /// Subscribe to ALL opcodes in a [from..to] range. Useful for
        /// game-defined ranges (0xE000-0xE7FF) where the game wants a
        /// catch-all dispatcher.
        /// </summary>
        IDisposable SubscribeRange(int opcodeFrom, int opcodeTo, Action<IVXRawKernelEvent> handler);

        /// <summary>Raised when the kernel fans-out PlayerJoined.</summary>
        event Action<IVXKernelEvent<Wire.IVXPlayerJoinedPayload>> OnPlayerJoined;

        /// <summary>Raised when a player is removed (timeout, leave, kick).</summary>
        event Action<IVXKernelEvent<Wire.IVXPlayerLeftPayload>> OnPlayerLeft;

        /// <summary>Raised once on Welcome receipt.</summary>
        event Action<IVXKernelEvent<Wire.IVXWelcomePayload>> OnWelcome;

        /// <summary>Raised on MatchEnded; session disposes itself afterward.</summary>
        event Action<IVXKernelEvent<Wire.IVXMatchEndedPayload>> OnMatchEnded;

        /// <summary>Raised on every Error envelope addressed to us.</summary>
        event Action<IVXKernelEvent<Wire.IVXError>> OnError;

        /// <summary>Raised when the underlying transport bounces.</summary>
        event Action<IVXTransportState> OnStateChanged;

        // ----- send -----

        /// <summary>
        /// Send a payload to the server with the given opcode. The header
        /// (seq, match_time_ms, client_opcode_uuid) is auto-stamped.
        /// </summary>
        Task SendAsync<TPayload>(int opcode, TPayload payload, CancellationToken cancellationToken = default);

        /// <summary>
        /// Send a pre-built envelope (used internally + by SDK extensions).
        /// </summary>
        Task SendEnvelopeAsync<TPayload>(Wire.IVXEnvelope<TPayload> envelope, CancellationToken cancellationToken = default);

        /// <summary>
        /// Politely leave the match. Closes the session and sends a
        /// transport-level Nakama leave; the server fans out
        /// <c>PLAYER_LEFT</c> with <c>VOLUNTARY</c> on its own.
        /// </summary>
        Task LeaveAsync(CancellationToken cancellationToken = default);
    }
}
