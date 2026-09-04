// Public Multiplayer API for the IntelliVerseX Unity SDK.
//
// Game code lives ABOVE this interface; backend / transport / wire-codec
// concerns live BELOW. Implementations:
//   - IVXNakamaMultiplayer (this assembly): Nakama-Unity client + JSON wire.
//   - IVXMockMultiplayer (Tests~): in-memory loopback for unit tests.
//   - IVXOfflineMultiplayer (planned): solo-bot replay for designer mode.
//
// The shape mirrors the IIVXMultiplayer contract documented in the
// reframe plan ("Pillar 4 — Adapter Interface"). Every method is async
// and cancellation-aware so consumers can safely tear down on scene
// unload without leaking sockets.

using System;
using System.Threading;
using System.Threading.Tasks;

namespace IntelliVerseX.MultiplayerKernel
{
    /// <summary>
    /// Engine-agnostic multiplayer adapter contract. The Unity, JS, Unreal,
    /// Godot, etc. adapters all implement this same logical surface so a
    /// game written against it ports without rewrites.
    /// </summary>
    public interface IIVXMultiplayer
    {
        // ----- lifecycle -----

        /// <summary>True if a session was ever opened in this process.</summary>
        bool IsInitialized { get; }

        /// <summary>True while a real-time socket is open.</summary>
        bool IsConnected { get; }

        /// <summary>Last server time we observed (match-clock authority).</summary>
        ulong LastServerUnixMs { get; }

        /// <summary>
        /// Initialise the adapter. Must be called once after the Nakama
        /// session is established. Idempotent.
        /// </summary>
        Task InitializeAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Tear down sockets, joined matches, and any voice provider.
        /// Safe to call multiple times.
        /// </summary>
        Task ShutdownAsync(CancellationToken cancellationToken = default);

        // ----- match factory -----

        /// <summary>
        /// Create a new match via the kernel's `mp_create_match` RPC.
        /// </summary>
        /// <param name="request">Template id + game id + per-template init.</param>
        Task<IVXCreateMatchResponse> CreateMatchAsync(
            IVXCreateMatchRequest request,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Join an existing match by id. Returns a session handle that
        /// owns its event loop until disposed. The same handle is used
        /// for inbound events and outbound opcode submissions.
        /// </summary>
        Task<IIVXMatchSession> JoinMatchAsync(
            string matchId,
            IVXJoinOptions options = null,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Convenience: create + join in one round-trip. Returns a live
        /// session.
        /// </summary>
        Task<IIVXMatchSession> CreateAndJoinAsync(
            IVXCreateMatchRequest request,
            IVXJoinOptions options = null,
            CancellationToken cancellationToken = default);

        // ----- diagnostics -----

        /// <summary>Raised on every kernel error envelope received.</summary>
        event Action<IVXKernelEvent<Wire.IVXError>> OnKernelError;

        /// <summary>Raised on adapter-level transport state changes.</summary>
        event Action<IVXTransportState> OnTransportStateChanged;
    }

    public enum IVXTransportState
    {
        Disconnected = 0,
        Connecting   = 1,
        Connected    = 2,
        Reconnecting = 3,
        FailedFatal  = 4
    }
}
