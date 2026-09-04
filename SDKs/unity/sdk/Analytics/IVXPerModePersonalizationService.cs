using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using Nakama;

namespace IntelliVerseX.Analytics
{
    /// <summary>
    /// IVXPerModePersonalizationService — Phase 4C (qv-insights-loop).
    ///
    /// Single-source-of-truth client cache for per-(gameId × userId)
    /// personalization data: full bundle + per-mode addenda. Backs the
    /// IVXSmartNudgeService, IVXTodayForYouFeed, and any per-mode AI
    /// surface (AI Host opener, Voice tutor intro, Fortune reveal,
    /// Tutor question-prep, Chat suggestion).
    ///
    /// Design goals:
    ///   1. Single network round-trip per session for the full bundle
    ///      (TTL: matches AI svc's Redis 6h cache; we re-fetch only on
    ///      explicit Invalidate, on stale=true response, or on the
    ///      hardcoded 30-min in-process expiry — whichever is first).
    ///   2. Per-mode access does NOT trigger a round-trip — addenda
    ///      live inside the bundle. Only the BACKGROUND refresh path
    ///      uses personalization_get_for_mode.
    ///   3. Fail-safe: every getter returns a safe stub (`null` or an
    ///      empty struct) when the bundle is missing / stale.
    ///   4. Thread-safe: all in-process state is read/written under a
    ///      single lock. Network calls happen outside the lock.
    /// </summary>
    public class IVXPerModePersonalizationService
    {
        private const string RPC_GET = "personalization_get";
        private const string RPC_GET_FOR_MODE = "personalization_get_for_mode";

        // Cache TTL upper bound (independent of Nakama's TTL). We only
        // trust the cache for 30 minutes locally, even if the server
        // would still serve it from Redis. This bounds staleness when
        // a user's cohort flips mid-session.
        private const float LOCAL_CACHE_TTL_SEC = 30f * 60f;

        // Hard cap to prevent a flapping AI svc from causing a refresh
        // storm — at most one bundle refetch every 30s.
        private const float REFETCH_COOLDOWN_SEC = 30f;

        public static IVXPerModePersonalizationService Instance { get; } =
            new IVXPerModePersonalizationService();

        private readonly object _lock = new object();
        private IClient _nakamaClient;
        private ISession _nakamaSession;
        private string _gameId;

        // In-process cache.
        private PersonalizationBundle _bundle;
        private float _bundleFetchedAt = -9999f;
        private float _lastFetchAttemptAt = -9999f;
        private bool _isFetching;

        public bool IsInitialized => _nakamaClient != null && _nakamaSession != null;

        public void Initialize(IClient client, ISession session, string gameId)
        {
            if (client == null || session == null || string.IsNullOrEmpty(gameId))
            {
                Debug.LogWarning("[IVXPerModePersonalizationService] Initialize ignored: null inputs.");
                return;
            }
            lock (_lock)
            {
                _nakamaClient = client;
                _nakamaSession = session;
                _gameId = gameId;
                _bundle = null;
                _bundleFetchedAt = -9999f;
            }
        }

        // ─── Public API ───────────────────────────────────────────

        /// <summary>
        /// Returns the full bundle, fetching once if not yet cached.
        /// Multiple concurrent callers coalesce into a single round-trip.
        /// </summary>
        public async Task<PersonalizationBundle> GetBundleAsync()
        {
            if (!IsInitialized) return null;
            PersonalizationBundle cached;
            bool needFetch;
            lock (_lock)
            {
                cached = _bundle;
                needFetch = ShouldRefetchUnsafe();
                if (!needFetch) return cached;
                _isFetching = true;
                _lastFetchAttemptAt = Time.realtimeSinceStartup;
            }
            try
            {
                var fresh = await FetchBundleOnceAsync();
                lock (_lock)
                {
                    if (fresh != null)
                    {
                        _bundle = fresh;
                        _bundleFetchedAt = Time.realtimeSinceStartup;
                    }
                }
                return fresh ?? cached;
            }
            finally
            {
                lock (_lock) _isFetching = false;
            }
        }

        /// <summary>
        /// Returns the per-mode addendum for one of the known modes.
        /// Falls back to a fast network re-fetch only for the
        /// requested mode if the bundle is missing the slot — never
        /// blocks the caller more than the local fetch timeout.
        /// </summary>
        public async Task<ModePromptAddendum> GetAddendumAsync(string mode)
        {
            if (!IsInitialized || string.IsNullOrEmpty(mode)) return null;
            var bundle = await GetBundleAsync();
            if (bundle?.Addenda != null && bundle.Addenda.TryGetValue(mode, out var addendum))
                return addendum;
            return await FetchAddendumOnlyAsync(mode);
        }

        public SmartNudge GetSmartNudge()
        {
            lock (_lock) return _bundle?.SmartNudge;
        }

        public List<TodayFeedCard> GetTodayFeed()
        {
            lock (_lock) return _bundle?.TodayFeed;
        }

        public PushSchedule GetPushSchedule()
        {
            lock (_lock) return _bundle?.PushSchedule;
        }

        public void Invalidate()
        {
            lock (_lock)
            {
                _bundle = null;
                _bundleFetchedAt = -9999f;
            }
        }

        // ─── Network ──────────────────────────────────────────────

        private bool ShouldRefetchUnsafe()
        {
            if (_isFetching) return false;
            if (_bundle == null) return true;
            float now = Time.realtimeSinceStartup;
            if ((now - _bundleFetchedAt) > LOCAL_CACHE_TTL_SEC) return true;
            if ((now - _lastFetchAttemptAt) < REFETCH_COOLDOWN_SEC) return false;
            return false;
        }

        private async Task<PersonalizationBundle> FetchBundleOnceAsync()
        {
            try
            {
                var payload = new Dictionary<string, object>
                {
                    { "game_id", _gameId },
                };
                var json = MiniJson.Serialize(payload);
                var resp = await _nakamaClient.RpcAsync(_nakamaSession, RPC_GET, json);
                if (resp == null || string.IsNullOrEmpty(resp.Payload)) return null;
                return PersonalizationBundle.TryParse(resp.Payload);
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[IVXPerModePersonalizationService] FetchBundle failed: {e.Message}");
                return null;
            }
        }

        private async Task<ModePromptAddendum> FetchAddendumOnlyAsync(string mode)
        {
            try
            {
                var payload = new Dictionary<string, object>
                {
                    { "game_id", _gameId },
                    { "mode",    mode },
                };
                var json = MiniJson.Serialize(payload);
                var resp = await _nakamaClient.RpcAsync(_nakamaSession, RPC_GET_FOR_MODE, json);
                if (resp == null || string.IsNullOrEmpty(resp.Payload)) return null;
                return ModePromptAddendum.TryParse(resp.Payload, mode);
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[IVXPerModePersonalizationService] FetchAddendumOnly failed: {e.Message}");
                return null;
            }
        }

        // ─── DTOs ─────────────────────────────────────────────────

        public class PersonalizationBundle
        {
            public string CohortLabel;
            public float CohortConfidence;
            public bool Stale;
            public SmartNudge SmartNudge;
            public List<TodayFeedCard> TodayFeed;
            public PushSchedule PushSchedule;
            public Dictionary<string, ModePromptAddendum> Addenda;

            public static PersonalizationBundle TryParse(string json)
            {
                try
                {
                    var top = MiniJson.Deserialize(json) as Dictionary<string, object>;
                    if (top == null) return null;
                    var b = new PersonalizationBundle
                    {
                        CohortLabel = ReadString(top, "cohortLabel"),
                        CohortConfidence = ReadFloat(top, "cohortConfidence"),
                        Stale = ReadBool(top, "stale"),
                        SmartNudge = SmartNudge.TryParse(top.TryGetValue("smartNudge", out var sn) ? sn : null),
                        TodayFeed = TodayFeedCard.TryParseList(top.TryGetValue("todayFeed", out var tf) ? tf : null),
                        PushSchedule = PushSchedule.TryParse(top.TryGetValue("pushSchedule", out var ps) ? ps : null),
                        Addenda = ParseAddenda(top.TryGetValue("addenda", out var ad) ? ad : null),
                    };
                    return b;
                }
                catch (Exception e)
                {
                    Debug.LogWarning($"[IVXPerModePersonalizationService] PersonalizationBundle.TryParse failed: {e.Message}");
                    return null;
                }
            }

            private static Dictionary<string, ModePromptAddendum> ParseAddenda(object raw)
            {
                var dict = new Dictionary<string, ModePromptAddendum>();
                if (!(raw is Dictionary<string, object> map)) return dict;
                foreach (var kvp in map)
                {
                    var addendum = ModePromptAddendum.TryParse(kvp.Value, kvp.Key);
                    if (addendum != null) dict[kvp.Key] = addendum;
                }
                return dict;
            }
        }

        public class SmartNudge
        {
            public string Title;
            public string Body;
            public string CtaLabel;
            public string CtaActionId;
            public Dictionary<string, object> Payload;

            public static SmartNudge TryParse(object raw)
            {
                if (!(raw is Dictionary<string, object> d)) return null;
                if (d.Count == 0) return null;
                return new SmartNudge
                {
                    Title = ReadString(d, "title"),
                    Body = ReadString(d, "body"),
                    CtaLabel = ReadString(d, "ctaLabel"),
                    CtaActionId = ReadString(d, "ctaActionId"),
                    Payload = d.TryGetValue("payload", out var p) ? p as Dictionary<string, object> : null,
                };
            }
        }

        public class TodayFeedCard
        {
            public string Id;
            public string Kind;
            public string Title;
            public string Body;
            public string CtaLabel;
            public string CtaActionId;
            public Dictionary<string, object> Payload;

            public static List<TodayFeedCard> TryParseList(object raw)
            {
                var list = new List<TodayFeedCard>();
                if (!(raw is List<object> arr)) return list;
                foreach (var v in arr)
                {
                    if (!(v is Dictionary<string, object> d)) continue;
                    list.Add(new TodayFeedCard
                    {
                        Id = ReadString(d, "id"),
                        Kind = ReadString(d, "kind"),
                        Title = ReadString(d, "title"),
                        Body = ReadString(d, "body"),
                        CtaLabel = ReadString(d, "ctaLabel"),
                        CtaActionId = ReadString(d, "ctaActionId"),
                        Payload = d.TryGetValue("payload", out var p) ? p as Dictionary<string, object> : null,
                    });
                }
                return list;
            }
        }

        public class PushSchedule
        {
            public string Timezone;
            public List<int> SendHoursUtc;
            public string Topic;

            public static PushSchedule TryParse(object raw)
            {
                if (!(raw is Dictionary<string, object> d)) return null;
                var hours = new List<int>();
                if (d.TryGetValue("sendHoursUtc", out var ho) && ho is List<object> arr)
                {
                    foreach (var v in arr)
                    {
                        if (v is long l) hours.Add((int)l);
                        else if (v is int i) hours.Add(i);
                        else if (int.TryParse(v?.ToString() ?? "", out var pi)) hours.Add(pi);
                    }
                }
                return new PushSchedule
                {
                    Timezone = ReadString(d, "timezone"),
                    SendHoursUtc = hours,
                    Topic = ReadString(d, "topic"),
                };
            }
        }

        public class ModePromptAddendum
        {
            public string Mode;
            public string SystemAddendum;
            public List<string> TopicHints;
            public string ToneHint;
            public string PromptVersion;
            public string ModelUsed;

            public static ModePromptAddendum TryParse(object raw, string fallbackMode)
            {
                if (!(raw is Dictionary<string, object> d)) return null;
                var hints = new List<string>();
                if (d.TryGetValue("topicHints", out var th) && th is List<object> arr)
                {
                    foreach (var v in arr)
                        if (v != null) hints.Add(v.ToString());
                }
                return new ModePromptAddendum
                {
                    Mode = ReadString(d, "mode") ?? fallbackMode,
                    SystemAddendum = ReadString(d, "systemAddendum"),
                    TopicHints = hints,
                    ToneHint = ReadString(d, "toneHint"),
                    PromptVersion = ReadString(d, "promptVersion"),
                    ModelUsed = ReadString(d, "modelUsed"),
                };
            }
        }

        // ─── Helpers ──────────────────────────────────────────────

        private static string ReadString(Dictionary<string, object> d, string k)
        {
            if (d == null || !d.TryGetValue(k, out var v) || v == null) return null;
            return v.ToString();
        }
        private static float ReadFloat(Dictionary<string, object> d, string k)
        {
            if (d == null || !d.TryGetValue(k, out var v) || v == null) return 0f;
            if (v is double dd) return (float)dd;
            if (v is long ll) return ll;
            if (v is int ii) return ii;
            if (float.TryParse(v.ToString(), out var f)) return f;
            return 0f;
        }
        private static bool ReadBool(Dictionary<string, object> d, string k)
        {
            if (d == null || !d.TryGetValue(k, out var v) || v == null) return false;
            if (v is bool b) return b;
            return v.ToString() == "true";
        }

        // Same minimal MiniJson shared by IVXCrossSellSurfacer; kept
        // self-contained so this file compiles even if the cross-sell
        // surfacer is excluded from the build.
        private static class MiniJson
        {
            public static string Serialize(object obj)
            {
                var sb = new System.Text.StringBuilder();
                Write(sb, obj);
                return sb.ToString();
            }
            public static object Deserialize(string json)
            {
                if (string.IsNullOrEmpty(json)) return null;
                int i = 0;
                return ReadValue(json, ref i);
            }
            private static void Write(System.Text.StringBuilder sb, object v)
            {
                if (v == null) { sb.Append("null"); return; }
                switch (v)
                {
                    case string s:
                        sb.Append('"').Append(EscapeString(s)).Append('"');
                        break;
                    case bool b: sb.Append(b ? "true" : "false"); break;
                    case IDictionary<string, object> d:
                        sb.Append('{');
                        bool first = true;
                        foreach (var kvp in d)
                        {
                            if (!first) sb.Append(',');
                            sb.Append('"').Append(EscapeString(kvp.Key)).Append('"').Append(':');
                            Write(sb, kvp.Value);
                            first = false;
                        }
                        sb.Append('}');
                        break;
                    case System.Collections.IEnumerable e:
                        sb.Append('[');
                        bool first2 = true;
                        foreach (var item in e)
                        {
                            if (!first2) sb.Append(',');
                            Write(sb, item);
                            first2 = false;
                        }
                        sb.Append(']');
                        break;
                    case float f:
                        sb.Append(f.ToString(System.Globalization.CultureInfo.InvariantCulture));
                        break;
                    case double dbl:
                        sb.Append(dbl.ToString(System.Globalization.CultureInfo.InvariantCulture));
                        break;
                    default: sb.Append(v.ToString()); break;
                }
            }
            private static string EscapeString(string s)
            {
                var sb = new System.Text.StringBuilder(s.Length + 4);
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
            private static object ReadValue(string s, ref int i)
            {
                SkipWs(s, ref i);
                if (i >= s.Length) return null;
                char c = s[i];
                if (c == '{') return ReadObject(s, ref i);
                if (c == '[') return ReadArray(s, ref i);
                if (c == '"') return ReadStringTok(s, ref i);
                if (c == 't' || c == 'f') return ReadBoolTok(s, ref i);
                if (c == 'n') { i += 4; return null; }
                return ReadNumber(s, ref i);
            }
            private static Dictionary<string, object> ReadObject(string s, ref int i)
            {
                var d = new Dictionary<string, object>();
                i++;
                SkipWs(s, ref i);
                if (i < s.Length && s[i] == '}') { i++; return d; }
                while (i < s.Length)
                {
                    SkipWs(s, ref i);
                    var key = ReadStringTok(s, ref i);
                    SkipWs(s, ref i);
                    if (i < s.Length && s[i] == ':') i++;
                    var val = ReadValue(s, ref i);
                    d[key] = val;
                    SkipWs(s, ref i);
                    if (i < s.Length && s[i] == ',') { i++; continue; }
                    if (i < s.Length && s[i] == '}') { i++; break; }
                }
                return d;
            }
            private static List<object> ReadArray(string s, ref int i)
            {
                var list = new List<object>();
                i++;
                SkipWs(s, ref i);
                if (i < s.Length && s[i] == ']') { i++; return list; }
                while (i < s.Length)
                {
                    list.Add(ReadValue(s, ref i));
                    SkipWs(s, ref i);
                    if (i < s.Length && s[i] == ',') { i++; continue; }
                    if (i < s.Length && s[i] == ']') { i++; break; }
                }
                return list;
            }
            private static string ReadStringTok(string s, ref int i)
            {
                if (s[i] == '"') i++;
                var sb = new System.Text.StringBuilder();
                while (i < s.Length && s[i] != '"')
                {
                    char c = s[i++];
                    if (c == '\\' && i < s.Length)
                    {
                        char esc = s[i++];
                        switch (esc)
                        {
                            case '"': sb.Append('"'); break;
                            case '\\': sb.Append('\\'); break;
                            case '/': sb.Append('/'); break;
                            case 'n': sb.Append('\n'); break;
                            case 'r': sb.Append('\r'); break;
                            case 't': sb.Append('\t'); break;
                            case 'b': sb.Append('\b'); break;
                            case 'f': sb.Append('\f'); break;
                            case 'u':
                                if (i + 4 <= s.Length)
                                {
                                    var hex = s.Substring(i, 4);
                                    i += 4;
                                    if (int.TryParse(hex, System.Globalization.NumberStyles.HexNumber,
                                        System.Globalization.CultureInfo.InvariantCulture, out var code))
                                        sb.Append((char)code);
                                }
                                break;
                            default: sb.Append(esc); break;
                        }
                    }
                    else sb.Append(c);
                }
                if (i < s.Length && s[i] == '"') i++;
                return sb.ToString();
            }
            private static object ReadNumber(string s, ref int i)
            {
                int start = i;
                while (i < s.Length && "0123456789.-eE+".IndexOf(s[i]) >= 0) i++;
                var slice = s.Substring(start, i - start);
                if (slice.IndexOf('.') >= 0 || slice.IndexOf('e') >= 0 || slice.IndexOf('E') >= 0)
                {
                    if (double.TryParse(slice, System.Globalization.NumberStyles.Float,
                        System.Globalization.CultureInfo.InvariantCulture, out var d)) return d;
                }
                if (long.TryParse(slice, out var l)) return l;
                return slice;
            }
            private static bool ReadBoolTok(string s, ref int i)
            {
                if (s[i] == 't') { i += 4; return true; }
                i += 5; return false;
            }
            private static void SkipWs(string s, ref int i)
            {
                while (i < s.Length && (s[i] == ' ' || s[i] == '\t' || s[i] == '\n' || s[i] == '\r')) i++;
            }
        }
    }
}
