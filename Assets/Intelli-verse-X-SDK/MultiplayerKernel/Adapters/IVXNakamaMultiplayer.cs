// Nakama-Unity adapter implementing IIVXMultiplayer.
//
// Wraps the Nakama-Unity IClient + ISocket. The kernel TS code on the
// server emits and accepts JSON envelopes; this adapter does the same on
// the client. Outbound goes via ISocket.SendMatchStateAsync(matchId, op,
// payloadJson). Inbound dispatches off socket.ReceivedMatchState.
//
// Per the reframe plan (Pillar 2 / 4), this adapter:
//   - Owns the realtime socket lifecycle (connect / reconnect / close).
//   - Stamps every outbound envelope with seq + client_opcode_uuid.
//   - Routes inbound envelopes to opcode handlers + range subscribers.
//   - Tracks the server clock authority via ClockSync.
//   - Emits transport state changes for the consumer's UI to react to.
//
// Voice/XR providers are NOT in scope for P2; those layer on later in
// P5 (LiveKit) and P6 (XR pose). The `Capabilities` array on
// IVXJoinOptions is plumbed through Hello so the server can pick a
// provider profile, but the adapter doesn't yet open a voice channel.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using IntelliVerseX.Backend;
using IntelliVerseX.MultiplayerKernel.Wire;
using Nakama;
using Newtonsoft.Json;
using UnityEngine;

namespace IntelliVerseX.MultiplayerKernel.Adapters
{
    /// <summary>
    /// Default <see cref="IIVXMultiplayer"/> implementation backed by the
    /// Nakama-Unity client.
    /// </summary>
    public class IVXNakamaMultiplayer : IIVXMultiplayer, IDisposable
    {
        private const string LOG_PREFIX = "[IVXNakamaMultiplayer]";
        private const string RPC_CREATE_MATCH = "mp_create_match";

        private readonly IIVXNakamaRealtimeProvider _provider;
        private readonly Dictionary<string, IVXMatchSession> _activeSessions = new Dictionary<string, IVXMatchSession>();
        private readonly object _sessionsLock = new object();

        private bool _initialized;
        private bool _disposed;
        private IVXTransportState _transportState = IVXTransportState.Disconnected;
        private ulong _lastServerUnixMs;

        public bool IsInitialized => _initialized;
        public bool IsConnected => _provider != null && _provider.Socket != null && _provider.Socket.IsConnected;
        public ulong LastServerUnixMs => _lastServerUnixMs;

        public event Action<IVXKernelEvent<IVXError>> OnKernelError;
        public event Action<IVXTransportState> OnTransportStateChanged;

        public IVXNakamaMultiplayer(IIVXNakamaRealtimeProvider provider)
        {
            _provider = provider ?? throw new ArgumentNullException(nameof(provider));
        }

        // ----- lifecycle -----

        public Task InitializeAsync(CancellationToken cancellationToken = default)
        {
            if (_initialized) return Task.CompletedTask;
            if (_provider.Socket == null)
            {
                throw new InvalidOperationException(
                    $"{LOG_PREFIX} Nakama socket is null; ensure IVXNakamaManager.InitializeAsync ran first.");
            }
            _provider.Socket.ReceivedMatchState    += OnReceivedMatchState;
            _provider.Socket.ReceivedMatchPresence += OnReceivedMatchPresence;
            _provider.Socket.Closed                += OnSocketClosed;
            _provider.Socket.Connected             += OnSocketConnected;
            _provider.Socket.ReceivedError         += OnSocketError;

            UpdateTransportState(_provider.Socket.IsConnected
                ? IVXTransportState.Connected
                : IVXTransportState.Connecting);
            _initialized = true;
            return Task.CompletedTask;
        }

        public async Task ShutdownAsync(CancellationToken cancellationToken = default)
        {
            if (!_initialized) return;
            List<IVXMatchSession> snapshot;
            lock (_sessionsLock) snapshot = new List<IVXMatchSession>(_activeSessions.Values);
            foreach (var s in snapshot)
            {
                try { await s.LeaveAsync(cancellationToken).ConfigureAwait(false); } catch { /* swallow */ }
                s.Dispose();
            }
            lock (_sessionsLock) _activeSessions.Clear();

            if (_provider.Socket != null)
            {
                _provider.Socket.ReceivedMatchState    -= OnReceivedMatchState;
                _provider.Socket.ReceivedMatchPresence -= OnReceivedMatchPresence;
                _provider.Socket.Closed                -= OnSocketClosed;
                _provider.Socket.Connected             -= OnSocketConnected;
                _provider.Socket.ReceivedError         -= OnSocketError;
            }
            _initialized = false;
            UpdateTransportState(IVXTransportState.Disconnected);
        }

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;
            try { _ = ShutdownAsync(); } catch { /* swallow */ }
        }

        // ----- match factory -----

        public async Task<IVXCreateMatchResponse> CreateMatchAsync(
            IVXCreateMatchRequest request,
            CancellationToken cancellationToken = default)
        {
            EnsureReady();
            if (request == null) throw new ArgumentNullException(nameof(request));
            if (string.IsNullOrEmpty(request.TemplateId))
            {
                throw new ArgumentException("template_id required", nameof(request));
            }

            var rpcPayload = new Dictionary<string, object>
            {
                { "template_id", request.TemplateId },
                { "game_id",     request.GameId ?? string.Empty },
                { "region",      request.Region ?? string.Empty },
                { "template_init", request.TemplateInit ?? new Dictionary<string, object>() },
            };
            var payloadJson = JsonConvert.SerializeObject(rpcPayload);

            IApiRpc rpc;
            try
            {
                rpc = await _provider.Client
                    .RpcAsync(_provider.Session, RPC_CREATE_MATCH, payloadJson)
                    .ConfigureAwait(false);
            }
            catch (Exception e)
            {
                Debug.LogError($"{LOG_PREFIX} mp_create_match RPC failed: {e.Message}");
                throw;
            }

            IVXCreateMatchResponse resp;
            try
            {
                resp = JsonConvert.DeserializeObject<IVXCreateMatchResponse>(rpc.Payload);
            }
            catch (Exception e)
            {
                Debug.LogError($"{LOG_PREFIX} mp_create_match response decode failed: {e.Message} body={rpc.Payload}");
                throw;
            }
            if (resp == null || string.IsNullOrEmpty(resp.MatchId))
            {
                throw new InvalidOperationException($"{LOG_PREFIX} mp_create_match returned empty match id (body={rpc.Payload})");
            }
            return resp;
        }

        public async Task<IIVXMatchSession> JoinMatchAsync(
            string matchId,
            IVXJoinOptions options = null,
            CancellationToken cancellationToken = default)
        {
            EnsureReady();
            if (string.IsNullOrEmpty(matchId)) throw new ArgumentException("matchId required", nameof(matchId));

            // Build metadata for join. Nakama matches accept a metadata
            // dictionary that the server template can read on join attempt.
            var metadata = new Dictionary<string, string>();
            if (options != null && !string.IsNullOrEmpty(options.PreferredLocale))
                metadata["locale"] = options.PreferredLocale;
            if (options != null && !string.IsNullOrEmpty(options.ClientBuildId))
                metadata["client_build_id"] = options.ClientBuildId;
            if (options != null && options.Capabilities != null && options.Capabilities.Length > 0)
                metadata["capabilities"] = string.Join(",", options.Capabilities);

            IMatch match;
            try
            {
                match = await _provider.Socket.JoinMatchAsync(matchId, metadata).ConfigureAwait(false);
            }
            catch (Exception e)
            {
                Debug.LogError($"{LOG_PREFIX} JoinMatchAsync failed match={matchId}: {e.Message}");
                throw;
            }

            var session = new IVXMatchSession(
                provider: _provider,
                match: match,
                templateIdHint: match.Label, // Nakama match-label carries template_id JSON
                options: options ?? new IVXJoinOptions(),
                onError: e => OnKernelError?.Invoke(e),
                onClockSampled: t => _lastServerUnixMs = t,
                onSelfDispose: id =>
                {
                    lock (_sessionsLock) _activeSessions.Remove(id);
                });

            lock (_sessionsLock) _activeSessions[match.Id] = session;

            // Send Hello so the server stamps schema/feature flags.
            var hello = new IVXHelloPayload
            {
                ClientProtocolVersion = IVXWireVersion.V1,
                ClientCapabilities    = options?.Capabilities,
                PreferredLocale       = options?.PreferredLocale,
                ClientBuildId         = options?.ClientBuildId ?? Application.version,
                ClientUnixMs          = (ulong)DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
            };
            try
            {
                await session.SendAsync(IVXKernelOp.HELLO, hello, cancellationToken).ConfigureAwait(false);
            }
            catch (Exception e)
            {
                Debug.LogWarning($"{LOG_PREFIX} Hello send failed (continuing) match={matchId}: {e.Message}");
            }
            return session;
        }

        public async Task<IIVXMatchSession> CreateAndJoinAsync(
            IVXCreateMatchRequest request,
            IVXJoinOptions options = null,
            CancellationToken cancellationToken = default)
        {
            var created = await CreateMatchAsync(request, cancellationToken).ConfigureAwait(false);
            return await JoinMatchAsync(created.MatchId, options, cancellationToken).ConfigureAwait(false);
        }

        // ----- inbound dispatch -----

        private void OnReceivedMatchState(IMatchState state)
        {
            IVXMatchSession session;
            lock (_sessionsLock) _activeSessions.TryGetValue(state.MatchId, out session);
            if (session == null) return;
            session.HandleInbound(state);
        }

        private void OnReceivedMatchPresence(IMatchPresenceEvent presenceEvent)
        {
            IVXMatchSession session;
            lock (_sessionsLock) _activeSessions.TryGetValue(presenceEvent.MatchId, out session);
            if (session == null) return;
            session.HandlePresence(presenceEvent);
        }

        private void OnSocketConnected() => UpdateTransportState(IVXTransportState.Connected);
        private void OnSocketClosed() => UpdateTransportState(IVXTransportState.Disconnected);
        private void OnSocketError(Exception e)
        {
            Debug.LogError($"{LOG_PREFIX} socket error: {e.Message}");
            UpdateTransportState(IVXTransportState.Reconnecting);
        }

        private void UpdateTransportState(IVXTransportState s)
        {
            if (_transportState == s) return;
            _transportState = s;
            try { OnTransportStateChanged?.Invoke(s); }
            catch (Exception e) { Debug.LogWarning($"{LOG_PREFIX} OnTransportStateChanged threw: {e.Message}"); }
            lock (_sessionsLock)
            {
                foreach (var sess in _activeSessions.Values)
                {
                    sess.OnTransportStateUpdated(s);
                }
            }
        }

        private void EnsureReady()
        {
            if (_disposed) throw new ObjectDisposedException(nameof(IVXNakamaMultiplayer));
            if (!_initialized) throw new InvalidOperationException($"{LOG_PREFIX} call InitializeAsync first");
            if (_provider.Session == null) throw new InvalidOperationException($"{LOG_PREFIX} Nakama session not established");
            if (_provider.Socket == null) throw new InvalidOperationException($"{LOG_PREFIX} Nakama socket null");
        }
    }
}
