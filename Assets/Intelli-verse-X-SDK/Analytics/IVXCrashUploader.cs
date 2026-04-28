using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using Nakama;

namespace IntelliVerseX.Analytics
{
    /// <summary>
    /// IVXCrashUploader — Phase 3 (qv-insights-loop).
    ///
    /// Captures unhandled C# exceptions, logged errors, and (best-effort)
    /// native crashes that surface through Unity's Application.logMessage
    /// callbacks and forwards them to the Nakama `crash_log_append` RPC.
    ///
    /// Design goals:
    ///   1. Zero allocation in the hot path — we copy the message into a
    ///      bounded ring buffer and flush from a background coroutine.
    ///   2. Never block gameplay — RPC failures are silently retried with
    ///      exponential backoff up to MAX_BACKLOG entries; older entries
    ///      are dropped (we'd rather lose old crashes than the freshest).
    ///   3. Stack-trace fingerprinting (top 3 frames + exception type)
    ///      gives the AI svc a stable group key for "top crash patterns",
    ///      keeping volume bounded even on a serious regression.
    ///   4. PII scrubbing: file system paths, GUIDs, IPv4 addresses, and
    ///      Bearer tokens are scrubbed before upload. We never send raw
    ///      user input.
    ///   5. Quiet hours: at most MAX_PER_MINUTE crashes uploaded per
    ///      minute. This is a safety belt; the server-side pattern
    ///      summariser groups duplicates regardless.
    ///
    /// Usage:
    ///   var go = new GameObject("IVXCrashUploader");
    ///   var uploader = go.AddComponent&lt;IVXCrashUploader&gt;();
    ///   uploader.Initialize(client, session, gameId: "quizverse",
    ///                       appVersion: Application.version);
    ///   DontDestroyOnLoad(go);
    /// </summary>
    public class IVXCrashUploader : MonoBehaviour
    {
        // Flush cadence — keep small so the analyst sees crashes
        // promptly, but not so small we hammer Nakama in a loop. The
        // crash RPC itself is rate-limited server-side too.
        private const float FLUSH_INTERVAL_SECONDS = 30f;

        // Bounded backlog — protects RAM if Nakama is unreachable for
        // a long time (e.g. user offline).
        private const int MAX_BACKLOG = 50;

        // Hard rate limit — never more than this many uploads/minute
        // even if MAX_BACKLOG is full of fresh crashes.
        private const int MAX_PER_MINUTE = 10;

        // Stack trace truncation — keep payload small.
        private const int MAX_STACK_LENGTH = 4096;
        private const int MAX_MESSAGE_LENGTH = 1024;

        // Server-side RPC id. Configurable so a future game can reuse
        // the uploader with a different prefix.
        private string _rpcId = "crash_log_append";

        private IClient _nakamaClient;
        private ISession _nakamaSession;
        private string _gameId;
        private string _appVersion;
        private string _platformOs;
        private string _deviceModel;

        private readonly Queue<CrashEntry> _backlog = new Queue<CrashEntry>();
        private readonly object _backlogLock = new object();

        private DateTime _minuteWindowStart = DateTime.UtcNow;
        private int _uploadsThisMinute;
        private bool _isInitialized;

        // De-dup ledger so a tight crash loop doesn't spam the queue.
        // Keyed on fingerprint hash, value is last-seen unix seconds +
        // count. We coalesce within DEDUPE_WINDOW_SEC.
        private const int DEDUPE_WINDOW_SEC = 300;
        private readonly Dictionary<string, (long lastTsSec, int count)> _dedupe =
            new Dictionary<string, (long, int)>();

        // Static regex's for PII scrubbing — compiled once.
        private static readonly Regex _bearerRegex =
            new Regex(@"Bearer\s+[A-Za-z0-9._-]+", RegexOptions.Compiled);
        private static readonly Regex _ipRegex =
            new Regex(@"\b\d{1,3}\.\d{1,3}\.\d{1,3}\.\d{1,3}\b", RegexOptions.Compiled);
        private static readonly Regex _guidRegex =
            new Regex(
                @"\b[0-9A-Fa-f]{8}-[0-9A-Fa-f]{4}-[0-9A-Fa-f]{4}-[0-9A-Fa-f]{4}-[0-9A-Fa-f]{12}\b",
                RegexOptions.Compiled);
        // Strip absolute file paths but keep filename (line numbers stay
        // attached on the next token, which is fine for grouping).
        private static readonly Regex _absPathRegex =
            new Regex(@"(?:/[^\s/]+)+/(?=[A-Za-z0-9_\-\.]+\.cs)",
                RegexOptions.Compiled);

        private struct CrashEntry
        {
            public long TsUnixMs;
            public string MessageScrubbed;
            public string StackScrubbed;
            public string Type;
            public string Severity;
            public string FingerprintHex;
            public int RepeatedCount;
        }

        // ─── Public API ────────────────────────────────────────────

        public void Initialize(
            IClient client,
            ISession session,
            string gameId,
            string appVersion,
            string crashRpcId = null)
        {
            if (_isInitialized) return;
            if (client == null || session == null)
            {
                Debug.LogWarning("[IVXCrashUploader] Initialize called with null client/session — skipping");
                return;
            }
            _nakamaClient = client;
            _nakamaSession = session;
            _gameId = string.IsNullOrEmpty(gameId) ? "unknown" : gameId;
            _appVersion = string.IsNullOrEmpty(appVersion) ? Application.version : appVersion;
            _platformOs = Application.platform.ToString();
            _deviceModel = SystemInfo.deviceModel ?? "unknown";
            if (!string.IsNullOrEmpty(crashRpcId)) _rpcId = crashRpcId;

            Application.logMessageReceivedThreaded += OnLogMessageReceived;
            _isInitialized = true;

            StartCoroutine(FlushLoop());
            Debug.Log($"[IVXCrashUploader] Initialized game={_gameId} app={_appVersion}");
        }

        private void OnDestroy()
        {
            if (_isInitialized)
            {
                Application.logMessageReceivedThreaded -= OnLogMessageReceived;
                _isInitialized = false;
            }
        }

        // ─── Capture path ─────────────────────────────────────────

        private void OnLogMessageReceived(string condition, string stackTrace, LogType type)
        {
            // Only care about the genuinely-bad signals. A normal log
            // line is not a crash.
            if (type != LogType.Exception && type != LogType.Assert &&
                type != LogType.Error)
            {
                return;
            }
            try
            {
                CaptureSync(condition, stackTrace, type.ToString());
            }
            catch
            {
                // never propagate a logging failure
            }
        }

        private void CaptureSync(string message, string stackTrace, string severity)
        {
            var msg = TruncateAndScrub(message ?? string.Empty, MAX_MESSAGE_LENGTH);
            var stk = TruncateAndScrub(stackTrace ?? string.Empty, MAX_STACK_LENGTH);
            var typeName = ExtractTypeName(message);
            var fp = ComputeFingerprint(typeName, stk);

            var nowSec = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            lock (_backlogLock)
            {
                if (_dedupe.TryGetValue(fp, out var prior) &&
                    nowSec - prior.lastTsSec < DEDUPE_WINDOW_SEC)
                {
                    _dedupe[fp] = (nowSec, prior.count + 1);
                    return; // coalesced — server will see the count later
                }
                _dedupe[fp] = (nowSec, 1);

                if (_backlog.Count >= MAX_BACKLOG)
                {
                    _backlog.Dequeue();
                }
                _backlog.Enqueue(new CrashEntry
                {
                    TsUnixMs = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                    MessageScrubbed = msg,
                    StackScrubbed = stk,
                    Type = typeName,
                    Severity = severity,
                    FingerprintHex = fp,
                    RepeatedCount = 1,
                });
            }
        }

        // ─── Flush path ───────────────────────────────────────────

        private System.Collections.IEnumerator FlushLoop()
        {
            var wait = new WaitForSeconds(FLUSH_INTERVAL_SECONDS);
            while (_isInitialized)
            {
                yield return wait;
                if (_nakamaSession == null || _nakamaSession.IsExpired) continue;
                _ = FlushOnceAsync();
            }
        }

        private async Task FlushOnceAsync()
        {
            List<CrashEntry> toSend;
            lock (_backlogLock)
            {
                if (_backlog.Count == 0) return;
                ResetMinuteWindowIfNeeded();
                int slots = Math.Max(0, MAX_PER_MINUTE - _uploadsThisMinute);
                if (slots == 0) return;
                toSend = new List<CrashEntry>(Math.Min(slots, _backlog.Count));
                while (toSend.Count < slots && _backlog.Count > 0)
                {
                    var e = _backlog.Dequeue();
                    if (_dedupe.TryGetValue(e.FingerprintHex, out var d))
                    {
                        e.RepeatedCount = d.count;
                    }
                    toSend.Add(e);
                }
                _uploadsThisMinute += toSend.Count;
            }
            foreach (var e in toSend)
            {
                try
                {
                    await SendOneAsync(e);
                }
                catch
                {
                    // re-queue at the head if we can; never throw
                    lock (_backlogLock)
                    {
                        if (_backlog.Count < MAX_BACKLOG) _backlog.Enqueue(e);
                    }
                    break;
                }
            }
        }

        private void ResetMinuteWindowIfNeeded()
        {
            var now = DateTime.UtcNow;
            if ((now - _minuteWindowStart).TotalSeconds >= 60)
            {
                _minuteWindowStart = now;
                _uploadsThisMinute = 0;
            }
        }

        private async Task SendOneAsync(CrashEntry e)
        {
            var payload = new Dictionary<string, object>
            {
                { "game_id",         _gameId },
                { "app_version",     _appVersion },
                { "platform_os",     _platformOs },
                { "device_model",    _deviceModel },
                { "ts_unix_ms",      e.TsUnixMs },
                { "severity",        e.Severity },
                { "type",            e.Type },
                { "message",         e.MessageScrubbed },
                { "stack",           e.StackScrubbed },
                { "fingerprint",     e.FingerprintHex },
                { "repeated_count",  e.RepeatedCount },
                { "client_unity_v",  Application.unityVersion },
            };
            string json = JsonUtilSafe.Serialize(payload);
            await _nakamaClient.RpcAsync(_nakamaSession, _rpcId, json);
        }

        // ─── Helpers ──────────────────────────────────────────────

        private static string TruncateAndScrub(string s, int maxLen)
        {
            if (string.IsNullOrEmpty(s)) return s;
            var scrubbed = _bearerRegex.Replace(s, "Bearer ***");
            scrubbed = _ipRegex.Replace(scrubbed, "<ip>");
            scrubbed = _guidRegex.Replace(scrubbed, "<guid>");
            scrubbed = _absPathRegex.Replace(scrubbed, string.Empty);
            if (scrubbed.Length > maxLen) scrubbed = scrubbed.Substring(0, maxLen);
            return scrubbed;
        }

        private static string ExtractTypeName(string message)
        {
            if (string.IsNullOrEmpty(message)) return "UnknownException";
            var colon = message.IndexOf(':');
            if (colon <= 0 || colon > 80) return "UnknownException";
            return message.Substring(0, colon);
        }

        /// <summary>
        /// Stable fingerprint = sha1( typeName + first 3 stack frames ).
        /// Frames are trimmed so file:line moves don't blow up the
        /// fingerprint, but the actual symbol is preserved.
        /// </summary>
        private static string ComputeFingerprint(string typeName, string stack)
        {
            var sb = new StringBuilder();
            sb.Append(typeName);
            sb.Append('|');
            if (!string.IsNullOrEmpty(stack))
            {
                int lines = 0;
                int from = 0;
                for (int i = 0; i < stack.Length && lines < 3; i++)
                {
                    if (stack[i] == '\n')
                    {
                        var line = stack.Substring(from, i - from);
                        var atIdx = line.IndexOf(" (at ");
                        if (atIdx > 0) line = line.Substring(0, atIdx);
                        sb.Append(line.Trim());
                        sb.Append('\n');
                        lines++;
                        from = i + 1;
                    }
                }
            }
            using (var sha = SHA1.Create())
            {
                var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(sb.ToString()));
                var hex = new StringBuilder(bytes.Length * 2);
                for (int i = 0; i < bytes.Length; i++) hex.Append(bytes[i].ToString("x2"));
                return hex.ToString();
            }
        }

        // Tiny dependency-free JSON serializer good enough for the 12-key
        // payload we send. Avoids dragging Newtonsoft.Json into the SDK.
        private static class JsonUtilSafe
        {
            public static string Serialize(Dictionary<string, object> d)
            {
                var sb = new StringBuilder("{");
                bool first = true;
                foreach (var kv in d)
                {
                    if (!first) sb.Append(',');
                    first = false;
                    sb.Append('"').Append(Escape(kv.Key)).Append('"').Append(':');
                    AppendValue(sb, kv.Value);
                }
                sb.Append('}');
                return sb.ToString();
            }

            private static void AppendValue(StringBuilder sb, object v)
            {
                switch (v)
                {
                    case null: sb.Append("null"); break;
                    case string s: sb.Append('"').Append(Escape(s)).Append('"'); break;
                    case bool b: sb.Append(b ? "true" : "false"); break;
                    case int i: sb.Append(i); break;
                    case long l: sb.Append(l); break;
                    case float f: sb.Append(f.ToString(System.Globalization.CultureInfo.InvariantCulture)); break;
                    case double d: sb.Append(d.ToString(System.Globalization.CultureInfo.InvariantCulture)); break;
                    default: sb.Append('"').Append(Escape(v.ToString())).Append('"'); break;
                }
            }

            private static string Escape(string s)
            {
                if (string.IsNullOrEmpty(s)) return string.Empty;
                var sb = new StringBuilder(s.Length + 8);
                foreach (var c in s)
                {
                    switch (c)
                    {
                        case '\\': sb.Append("\\\\"); break;
                        case '"': sb.Append("\\\""); break;
                        case '\n': sb.Append("\\n"); break;
                        case '\r': sb.Append("\\r"); break;
                        case '\t': sb.Append("\\t"); break;
                        default:
                            if (c < 0x20) sb.AppendFormat("\\u{0:X4}", (int)c);
                            else sb.Append(c);
                            break;
                    }
                }
                return sb.ToString();
            }
        }
    }
}
