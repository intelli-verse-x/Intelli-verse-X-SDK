// Per-match session — implements IIVXMatchSession over a single Nakama
// IMatch + ISocket. Owns the inbound dispatcher map, the outbound seq
// counter, and the simple per-second token-bucket rate limit.

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using IntelliVerseX.Backend;
using IntelliVerseX.MultiplayerKernel.Wire;
using Nakama;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace IntelliVerseX.MultiplayerKernel.Adapters
{
    internal class IVXMatchSession : IIVXMatchSession
    {
        private const string LOG_PREFIX = "[IVXMatchSession]";

        private readonly IIVXNakamaRealtimeProvider _provider;
        private readonly IMatch _match;
        private readonly IVXJoinOptions _options;
        private readonly Action<IVXKernelEvent<IVXError>> _onError;
        private readonly Action<ulong> _onClockSampled;
        private readonly Action<string> _onSelfDispose;

        // Per-opcode handler list (multi-subscribe).
        private readonly Dictionary<int, List<Action<IVXRawKernelEvent>>> _opHandlers
            = new Dictionary<int, List<Action<IVXRawKernelEvent>>>();
        private readonly List<RangeSubscription> _rangeHandlers = new List<RangeSubscription>();
        private readonly object _handlersLock = new object();

        // Outbound seq + clock state.
        private long _outboundSeq = 1;
        private ulong _serverMatchTimeAtLastSyncMs;
        private DateTime _lastSyncWallClockUtc = DateTime.UtcNow;

        // Active player set (best-effort tracking).
        private readonly HashSet<string> _activeUsers = new HashSet<string>();
        private readonly object _activeLock = new object();

        private bool _disposed;
        private IVXTransportState _state = IVXTransportState.Connected;

        // Token-bucket rate limit (default 30/s).
        private int _opsRemainingThisSecond;
        private DateTime _bucketStartUtc;

        public string MatchId    => _match?.Id ?? string.Empty;
        public string TemplateId { get; private set; }
        public string LocalUserId { get; private set; }
        public ulong CurrentMatchTimeMs
        {
            get
            {
                var elapsed = (DateTime.UtcNow - _lastSyncWallClockUtc).TotalMilliseconds;
                return _serverMatchTimeAtLastSyncMs + (ulong)Math.Max(0, elapsed);
            }
        }
        public int ActivePlayerCount
        {
            get { lock (_activeLock) return _activeUsers.Count; }
        }
        public IVXTransportState State => _state;

        public event Action<IVXKernelEvent<IVXPlayerJoinedPayload>> OnPlayerJoined;
        public event Action<IVXKernelEvent<IVXPlayerLeftPayload>>   OnPlayerLeft;
        public event Action<IVXKernelEvent<IVXWelcomePayload>>      OnWelcome;
        public event Action<IVXKernelEvent<IVXMatchEndedPayload>>   OnMatchEnded;
        public event Action<IVXKernelEvent<IVXError>>               OnError;
        public event Action<IVXTransportState>                      OnStateChanged;

        public IVXMatchSession(
            IIVXNakamaRealtimeProvider provider,
            IMatch match,
            string templateIdHint,
            IVXJoinOptions options,
            Action<IVXKernelEvent<IVXError>> onError,
            Action<ulong> onClockSampled,
            Action<string> onSelfDispose)
        {
            _provider       = provider;
            _match          = match;
            _options        = options ?? new IVXJoinOptions();
            _onError        = onError;
            _onClockSampled = onClockSampled;
            _onSelfDispose  = onSelfDispose;
            TemplateId      = ExtractTemplateId(templateIdHint);
            LocalUserId     = match.Self?.UserId ?? string.Empty;

            ResetBucket();

            // Seed our active set from the join response.
            lock (_activeLock)
            {
                foreach (var p in match.Presences)
                {
                    if (p?.UserId != null) _activeUsers.Add(p.UserId);
                }
                if (match.Self?.UserId != null) _activeUsers.Add(match.Self.UserId);
            }
        }

        // ----- subscribe -----

        public IDisposable Subscribe<TPayload>(int opcode, Action<IVXKernelEvent<TPayload>> handler)
        {
            if (handler == null) throw new ArgumentNullException(nameof(handler));
            Action<IVXRawKernelEvent> raw = ev =>
            {
                TPayload p;
                try { p = ev.PayloadJson != null ? JsonConvert.DeserializeObject<TPayload>(ev.PayloadJson) : default; }
                catch (Exception e)
                {
                    Debug.LogWarning($"{LOG_PREFIX} payload decode failed op=0x{opcode:X4}: {e.Message}");
                    return;
                }
                try { handler(new IVXKernelEvent<TPayload>(ev.Header, p, ev.RecvUnixMs)); }
                catch (Exception e) { Debug.LogWarning($"{LOG_PREFIX} handler threw op=0x{opcode:X4}: {e.Message}"); }
            };

            lock (_handlersLock)
            {
                if (!_opHandlers.TryGetValue(opcode, out var list))
                {
                    list = new List<Action<IVXRawKernelEvent>>();
                    _opHandlers[opcode] = list;
                }
                list.Add(raw);
            }
            return new SubscriptionToken(() =>
            {
                lock (_handlersLock)
                {
                    if (_opHandlers.TryGetValue(opcode, out var list2)) list2.Remove(raw);
                }
            });
        }

        public IDisposable SubscribeRange(int opcodeFrom, int opcodeTo, Action<IVXRawKernelEvent> handler)
        {
            if (handler == null) throw new ArgumentNullException(nameof(handler));
            var sub = new RangeSubscription { From = opcodeFrom, To = opcodeTo, Handler = handler };
            lock (_handlersLock) _rangeHandlers.Add(sub);
            return new SubscriptionToken(() =>
            {
                lock (_handlersLock) _rangeHandlers.Remove(sub);
            });
        }

        // ----- send -----

        public Task SendAsync<TPayload>(int opcode, TPayload payload, CancellationToken cancellationToken = default)
        {
            EnsureLive();
            if (!ConsumeBucket())
            {
                Debug.LogWarning($"{LOG_PREFIX} outbound rate limit hit (cap={_options.OutboundOpsPerSecondLimit > 0 ? _options.OutboundOpsPerSecondLimit : 30})");
                return Task.CompletedTask;
            }
            var env = new IVXEnvelope<TPayload>(opcode, payload, _match.Id, LocalUserId);
            env.Header.Seq = (ulong)Interlocked.Increment(ref _outboundSeq);
            env.Header.MatchTimeMs = CurrentMatchTimeMs;
            return SendEnvelopeAsync(env, cancellationToken);
        }

        public async Task SendEnvelopeAsync<TPayload>(IVXEnvelope<TPayload> envelope, CancellationToken cancellationToken = default)
        {
            EnsureLive();
            if (envelope == null) throw new ArgumentNullException(nameof(envelope));
            if (envelope.Header == null) envelope.Header = new IVXHeader();
            envelope.Header.MatchId = _match.Id;
            if (string.IsNullOrEmpty(envelope.Header.SenderUserId)) envelope.Header.SenderUserId = LocalUserId;
            if (string.IsNullOrEmpty(envelope.Header.ClientOpcodeUuid)) envelope.Header.ClientOpcodeUuid = Guid.NewGuid().ToString("N");
            var json = JsonConvert.SerializeObject(envelope);
            try
            {
                await _provider.Socket.SendMatchStateAsync(_match.Id, (long)envelope.Header.Op, json).ConfigureAwait(false);
            }
            catch (Exception e)
            {
                Debug.LogError($"{LOG_PREFIX} SendMatchStateAsync failed op=0x{envelope.Header.Op:X4} match={_match.Id}: {e.Message}");
                throw;
            }
        }

        public async Task LeaveAsync(CancellationToken cancellationToken = default)
        {
            if (_disposed) return;
            // Voluntary client leave is a transport-level concern — Nakama
            // emits the matchLeave callback to the server, which fans out
            // PLAYER_LEFT(reason=VOLUNTARY) on its own. We do NOT send any
            // PLAYER_LEFT (0x0005) ourselves; that opcode is server→client
            // only in the v1 contract.
            try
            {
                await _provider.Socket.LeaveMatchAsync(_match.Id).ConfigureAwait(false);
            }
            catch (Exception e)
            {
                Debug.LogWarning($"{LOG_PREFIX} LeaveMatchAsync threw match={_match.Id}: {e.Message}");
            }
            Dispose();
        }

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;
            try { _onSelfDispose?.Invoke(_match.Id); } catch { }
        }

        // ----- inbound (called by adapter) -----

        internal void HandleInbound(IMatchState state)
        {
            if (_disposed) return;
            string raw;
            try
            {
                raw = state.State != null ? Encoding.UTF8.GetString(state.State) : string.Empty;
            }
            catch (Exception e)
            {
                Debug.LogWarning($"{LOG_PREFIX} state utf8 decode failed: {e.Message}");
                return;
            }
            if (string.IsNullOrEmpty(raw)) return;

            JObject env;
            IVXHeader header;
            string payloadJson;
            try
            {
                env = JObject.Parse(raw);
                header = env["h"] != null ? env["h"].ToObject<IVXHeader>() : null;
                payloadJson = env["p"] != null ? env["p"].ToString(Formatting.None) : null;
            }
            catch (Exception e)
            {
                Debug.LogWarning($"{LOG_PREFIX} envelope parse failed op=0x{state.OpCode:X4}: {e.Message}");
                return;
            }
            if (header == null)
            {
                header = new IVXHeader { Op = (int)state.OpCode, MatchId = _match.Id };
            }

            var recvMs = (ulong)DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            DispatchKernelOpcodes(header, payloadJson, recvMs);

            // Game opcodes — fan out via op + range subscribers.
            var rawEvent = new IVXRawKernelEvent(header, payloadJson, recvMs);
            List<Action<IVXRawKernelEvent>> opCopy = null;
            List<RangeSubscription> rangeCopy = null;
            lock (_handlersLock)
            {
                if (_opHandlers.TryGetValue(header.Op, out var list))
                    opCopy = new List<Action<IVXRawKernelEvent>>(list);
                if (_rangeHandlers.Count > 0)
                    rangeCopy = new List<RangeSubscription>(_rangeHandlers);
            }
            if (opCopy != null)
            {
                for (int i = 0; i < opCopy.Count; i++)
                {
                    try { opCopy[i](rawEvent); }
                    catch (Exception e) { Debug.LogWarning($"{LOG_PREFIX} handler threw op=0x{header.Op:X4}: {e.Message}"); }
                }
            }
            if (rangeCopy != null)
            {
                for (int i = 0; i < rangeCopy.Count; i++)
                {
                    var rs = rangeCopy[i];
                    if (header.Op >= rs.From && header.Op <= rs.To)
                    {
                        try { rs.Handler(rawEvent); }
                        catch (Exception e) { Debug.LogWarning($"{LOG_PREFIX} range handler threw op=0x{header.Op:X4}: {e.Message}"); }
                    }
                }
            }
        }

        internal void HandlePresence(IMatchPresenceEvent ev)
        {
            if (_disposed) return;
            lock (_activeLock)
            {
                if (ev.Joins != null)
                {
                    foreach (var j in ev.Joins) if (j?.UserId != null) _activeUsers.Add(j.UserId);
                }
                if (ev.Leaves != null)
                {
                    foreach (var l in ev.Leaves) if (l?.UserId != null) _activeUsers.Remove(l.UserId);
                }
            }
        }

        internal void OnTransportStateUpdated(IVXTransportState s)
        {
            if (_state == s) return;
            _state = s;
            try { OnStateChanged?.Invoke(s); } catch { }
        }

        // ----- helpers -----

        private void DispatchKernelOpcodes(IVXHeader header, string payloadJson, ulong recvMs)
        {
            switch (header.Op)
            {
                case IVXKernelOp.WELCOME:
                {
                    IVXWelcomePayload p = null;
                    try { p = payloadJson != null ? JsonConvert.DeserializeObject<IVXWelcomePayload>(payloadJson) : null; }
                    catch (Exception e) { Debug.LogWarning($"{LOG_PREFIX} welcome decode: {e.Message}"); }
                    if (p != null)
                    {
                        SampleClock(p.ServerMatchTimeMs, p.ServerUnixMs);
                        if (!string.IsNullOrEmpty(p.AssignedUserId)) LocalUserId = p.AssignedUserId;
                    }
                    try { OnWelcome?.Invoke(new IVXKernelEvent<IVXWelcomePayload>(header, p, recvMs)); }
                    catch (Exception e) { Debug.LogWarning($"{LOG_PREFIX} OnWelcome threw: {e.Message}"); }
                    break;
                }
                case IVXKernelOp.PLAYER_JOINED:
                {
                    var p = SafeDeserialize<IVXPlayerJoinedPayload>(payloadJson);
                    if (p != null)
                    {
                        lock (_activeLock) _activeUsers.Add(p.UserId);
                        try { OnPlayerJoined?.Invoke(new IVXKernelEvent<IVXPlayerJoinedPayload>(header, p, recvMs)); }
                        catch (Exception e) { Debug.LogWarning($"{LOG_PREFIX} OnPlayerJoined threw: {e.Message}"); }
                    }
                    break;
                }
                case IVXKernelOp.PLAYER_LEFT:
                {
                    var p = SafeDeserialize<IVXPlayerLeftPayload>(payloadJson);
                    if (p != null)
                    {
                        lock (_activeLock) _activeUsers.Remove(p.UserId);
                        try { OnPlayerLeft?.Invoke(new IVXKernelEvent<IVXPlayerLeftPayload>(header, p, recvMs)); }
                        catch (Exception e) { Debug.LogWarning($"{LOG_PREFIX} OnPlayerLeft threw: {e.Message}"); }
                    }
                    break;
                }
                case IVXKernelOp.CLOCK_SYNC:
                {
                    var p = SafeDeserialize<IVXClockSyncPayload>(payloadJson);
                    if (p != null) SampleClock(p.ServerMatchTimeMs, p.ServerUnixMs);
                    break;
                }
                case IVXKernelOp.MATCH_ENDED:
                {
                    var p = SafeDeserialize<IVXMatchEndedPayload>(payloadJson);
                    try { OnMatchEnded?.Invoke(new IVXKernelEvent<IVXMatchEndedPayload>(header, p, recvMs)); }
                    catch (Exception e) { Debug.LogWarning($"{LOG_PREFIX} OnMatchEnded threw: {e.Message}"); }
                    Dispose();
                    break;
                }
                case IVXKernelOp.ERROR:
                {
                    var p = SafeDeserialize<IVXError>(payloadJson);
                    if (p != null)
                    {
                        var ev = new IVXKernelEvent<IVXError>(header, p, recvMs);
                        try { OnError?.Invoke(ev); } catch (Exception e) { Debug.LogWarning($"{LOG_PREFIX} OnError threw: {e.Message}"); }
                        try { _onError?.Invoke(ev); } catch { }
                    }
                    break;
                }
            }
        }

        private void SampleClock(ulong serverMatchTimeMs, ulong serverUnixMs)
        {
            _serverMatchTimeAtLastSyncMs = serverMatchTimeMs;
            _lastSyncWallClockUtc = DateTime.UtcNow;
            try { _onClockSampled?.Invoke(serverUnixMs); } catch { }
        }

        private static T SafeDeserialize<T>(string json)
        {
            if (string.IsNullOrEmpty(json)) return default;
            try { return JsonConvert.DeserializeObject<T>(json); }
            catch (Exception e)
            {
                Debug.LogWarning($"[IVXMatchSession] decode failed type={typeof(T).Name}: {e.Message}");
                return default;
            }
        }

        private bool ConsumeBucket()
        {
            var cap = _options.OutboundOpsPerSecondLimit > 0 ? _options.OutboundOpsPerSecondLimit : 30;
            var now = DateTime.UtcNow;
            if ((now - _bucketStartUtc).TotalMilliseconds >= 1000)
            {
                _bucketStartUtc = now;
                _opsRemainingThisSecond = cap;
            }
            if (_opsRemainingThisSecond <= 0) return false;
            _opsRemainingThisSecond--;
            return true;
        }

        private void ResetBucket()
        {
            _bucketStartUtc = DateTime.UtcNow;
            _opsRemainingThisSecond = _options.OutboundOpsPerSecondLimit > 0 ? _options.OutboundOpsPerSecondLimit : 30;
        }

        private void EnsureLive()
        {
            if (_disposed) throw new ObjectDisposedException(nameof(IVXMatchSession));
            if (_provider == null || _provider.Socket == null)
                throw new InvalidOperationException($"{LOG_PREFIX} socket missing");
        }

        private static string ExtractTemplateId(string label)
        {
            if (string.IsNullOrEmpty(label)) return string.Empty;
            try
            {
                var jo = JObject.Parse(label);
                var v = jo["template_id"];
                return v != null ? v.ToString() : string.Empty;
            }
            catch { return string.Empty; }
        }

        private struct RangeSubscription
        {
            public int From;
            public int To;
            public Action<IVXRawKernelEvent> Handler;
        }

        private class SubscriptionToken : IDisposable
        {
            private readonly Action _onDispose;
            private bool _disposed;
            public SubscriptionToken(Action onDispose) { _onDispose = onDispose; }
            public void Dispose() { if (_disposed) return; _disposed = true; try { _onDispose?.Invoke(); } catch { } }
        }
    }
}
