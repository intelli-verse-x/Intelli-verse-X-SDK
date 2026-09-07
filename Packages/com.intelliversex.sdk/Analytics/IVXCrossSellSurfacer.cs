using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using Nakama;

namespace IntelliVerseX.Analytics
{
    /// <summary>
    /// IVXCrossSellSurfacer — Phase 4 Cross-Sell Engine (qv-insights-loop).
    ///
    /// Thin SDK helper that fronts the Nakama xsell_pick / xsell_record
    /// RPCs and exposes a clean async API for game UI code:
    ///
    ///   var pick = await IVXCrossSellSurfacer.Instance.PickAsync(
    ///       surface: "post_match_sheet",
    ///       cohortLabel: ivxCohortService.CurrentLabel,
    ///       quizMode: "ai_host");
    ///   if (pick != null)
    ///   {
    ///       myUiPanel.Render(pick.Title, pick.Body, pick.CtaLabel);
    ///       await IVXCrossSellSurfacer.Instance.RecordAsync(
    ///           pick.OfferId, surface: pick.Surface, kind: "impression");
    ///       myUiPanel.OnCtaClicked += async () =>
    ///           await IVXCrossSellSurfacer.Instance.RecordAsync(
    ///               pick.OfferId, surface: pick.Surface, kind: "engagement");
    ///   }
    ///
    /// Design goals:
    ///   1. Never block the UI thread — every call is async and returns
    ///      null on any error (network, RPC error, schema mismatch).
    ///   2. Server-trust model — we DON'T trust the SDK to compute the
    ///      pick locally. The Thompson Sampling + delivery-cap logic
    ///      lives in the AI service. The SDK only renders + reports.
    ///   3. Conversion is server-confirmed — the conversion path goes
    ///      through the IAP / billing webhook, NOT through this SDK
    ///      method (which is why there's no `Convert` helper here).
    ///   4. Defensive parsing — a malformed RPC response logs a warning
    ///      and returns null. Never throws into game code.
    ///   5. Local impression cooldown — a 90-second client cooldown per
    ///      (surface) prevents accidental double-pick spam (e.g. if the
    ///      user mashes the back button). This is on top of the
    ///      server-side per-day cap which is the real source of truth.
    /// </summary>
    public class IVXCrossSellSurfacer
    {
        private const string RPC_PICK = "xsell_pick";
        private const string RPC_RECORD = "xsell_record";
        private const float LOCAL_PICK_COOLDOWN_SEC = 90f;

        private static IVXCrossSellSurfacer _instance;
        public static IVXCrossSellSurfacer Instance =>
            _instance ?? (_instance = new IVXCrossSellSurfacer());

        private IClient _nakamaClient;
        private ISession _nakamaSession;
        private string _gameId;
        private readonly Dictionary<string, float> _surfaceLastPickTime =
            new Dictionary<string, float>();

        public bool IsInitialized => _nakamaClient != null && _nakamaSession != null;

        public void Initialize(IClient client, ISession session, string gameId)
        {
            if (client == null || session == null || string.IsNullOrEmpty(gameId))
            {
                Debug.LogWarning("[IVXCrossSellSurfacer] Initialize ignored: null inputs.");
                return;
            }
            _nakamaClient = client;
            _nakamaSession = session;
            _gameId = gameId;
        }

        // ─── Pick ────────────────────────────────────────────────

        /// <summary>
        /// Async pick of the best eligible cross-sell offer for this
        /// surface. Returns null if no eligible offer / cap reached /
        /// network error. Caller should treat null as "render nothing".
        /// </summary>
        public async Task<CrossSellPick> PickAsync(
            string surface,
            string cohortLabel = null,
            string quizMode = null,
            UserFeatures features = null)
        {
            if (!IsInitialized || string.IsNullOrEmpty(surface)) return null;
            if (IsOnLocalCooldown(surface)) return null;

            try
            {
                var payload = new Dictionary<string, object>
                {
                    { "game_id",      _gameId },
                    { "surface",      surface },
                    { "cohort_label", cohortLabel ?? string.Empty },
                    { "quiz_mode",    quizMode ?? string.Empty },
                    { "features",     features?.ToDict() ?? new Dictionary<string, object>() },
                };
                string json = MiniJson.Serialize(payload);
                var resp = await _nakamaClient.RpcAsync(_nakamaSession, RPC_PICK, json);
                if (resp == null || string.IsNullOrEmpty(resp.Payload)) return null;
                _surfaceLastPickTime[surface] = Time.realtimeSinceStartup;
                return CrossSellPick.TryParse(resp.Payload);
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[IVXCrossSellSurfacer] PickAsync failed: {e.Message}");
                return null;
            }
        }

        // ─── Record ──────────────────────────────────────────────

        /// <summary>
        /// Records an impression or engagement event for an offer that
        /// was picked + rendered. `kind` must be "impression" or
        /// "engagement". Conversions are server-confirmed via the
        /// billing webhook and should never be recorded here.
        /// </summary>
        public async Task RecordAsync(string offerId, string surface, string kind)
        {
            if (!IsInitialized || string.IsNullOrEmpty(offerId)) return;
            if (kind != "impression" && kind != "engagement")
            {
                Debug.LogWarning($"[IVXCrossSellSurfacer] RecordAsync invalid kind: {kind}");
                return;
            }
            try
            {
                var payload = new Dictionary<string, object>
                {
                    { "game_id",  _gameId },
                    { "offer_id", offerId },
                    { "surface",  surface },
                    { "kind",     kind },
                };
                string json = MiniJson.Serialize(payload);
                await _nakamaClient.RpcAsync(_nakamaSession, RPC_RECORD, json);
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[IVXCrossSellSurfacer] RecordAsync failed: {e.Message}");
            }
        }

        // ─── Helpers ─────────────────────────────────────────────

        private bool IsOnLocalCooldown(string surface)
        {
            if (!_surfaceLastPickTime.TryGetValue(surface, out var last)) return false;
            return (Time.realtimeSinceStartup - last) < LOCAL_PICK_COOLDOWN_SEC;
        }

        // ─── DTOs ────────────────────────────────────────────────

        public class UserFeatures
        {
            public float RecencyDays;
            public int SessionsP30D;
            public int IapCount;
            public float ModeAffinity;

            public Dictionary<string, object> ToDict()
            {
                return new Dictionary<string, object>
                {
                    { "recencyDays",   RecencyDays },
                    { "sessionsP30D",  SessionsP30D },
                    { "iapCount",      IapCount },
                    { "modeAffinity",  ModeAffinity },
                };
            }
        }

        public class CrossSellPick
        {
            public string OfferId;
            public string Surface;
            public string ProductId;
            public string Title;
            public string Body;
            public string CtaLabel;
            public float? DisplayPriceUsd;
            public string ScoringPath;
            public Dictionary<string, object> Payload;

            public static CrossSellPick TryParse(string json)
            {
                try
                {
                    var top = MiniJson.Deserialize(json) as Dictionary<string, object>;
                    if (top == null) return null;
                    if (!top.TryGetValue("pick", out var pickObj) || pickObj == null) return null;
                    var p = pickObj as Dictionary<string, object>;
                    if (p == null) return null;
                    var pick = new CrossSellPick
                    {
                        OfferId = ReadString(p, "offerId"),
                        Surface = ReadString(p, "surface"),
                        ProductId = ReadString(p, "productId"),
                        Title = ReadString(p, "title"),
                        Body = ReadString(p, "body"),
                        CtaLabel = ReadString(p, "ctaLabel"),
                        ScoringPath = ReadString(p, "scoringPath"),
                        DisplayPriceUsd = ReadNullableFloat(p, "displayPriceUsd"),
                        Payload = p.TryGetValue("payload", out var pp)
                            ? pp as Dictionary<string, object>
                            : null,
                    };
                    if (string.IsNullOrEmpty(pick.OfferId)) return null;
                    return pick;
                }
                catch (Exception e)
                {
                    Debug.LogWarning($"[IVXCrossSellSurfacer] CrossSellPick.TryParse failed: {e.Message}");
                    return null;
                }
            }

            private static string ReadString(Dictionary<string, object> d, string k)
            {
                if (!d.TryGetValue(k, out var v) || v == null) return null;
                return v.ToString();
            }

            private static float? ReadNullableFloat(Dictionary<string, object> d, string k)
            {
                if (!d.TryGetValue(k, out var v) || v == null) return null;
                if (v is double dd) return (float)dd;
                if (v is float ff) return ff;
                if (v is long ll) return ll;
                if (v is int ii) return ii;
                if (float.TryParse(v.ToString(), out var f)) return f;
                return null;
            }
        }

        // ─── MiniJson — dependency-free JSON serializer/deserializer ──
        // Identical wire format to Nakama's expected RPC payload (JSON).
        // We piggy-back on the same minimal MiniJson that IVXCrashUploader
        // uses if available — but keep a self-contained copy so this file
        // compiles even if the crash uploader is excluded from the build.
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

            // ─── writer ────────────────────────────────────────
            private static void Write(System.Text.StringBuilder sb, object v)
            {
                if (v == null) { sb.Append("null"); return; }
                switch (v)
                {
                    case string s:
                        sb.Append('"').Append(EscapeString(s)).Append('"');
                        break;
                    case bool b:
                        sb.Append(b ? "true" : "false");
                        break;
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
                    default:
                        sb.Append(v.ToString());
                        break;
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

            // ─── reader (minimal) ──────────────────────────────
            private static object ReadValue(string s, ref int i)
            {
                SkipWs(s, ref i);
                if (i >= s.Length) return null;
                char c = s[i];
                if (c == '{') return ReadObject(s, ref i);
                if (c == '[') return ReadArray(s, ref i);
                if (c == '"') return ReadString(s, ref i);
                if (c == 't' || c == 'f') return ReadBool(s, ref i);
                if (c == 'n') { i += 4; return null; }
                return ReadNumber(s, ref i);
            }

            private static Dictionary<string, object> ReadObject(string s, ref int i)
            {
                var d = new Dictionary<string, object>();
                i++; // skip {
                SkipWs(s, ref i);
                if (i < s.Length && s[i] == '}') { i++; return d; }
                while (i < s.Length)
                {
                    SkipWs(s, ref i);
                    var key = ReadString(s, ref i);
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
                i++; // skip [
                SkipWs(s, ref i);
                if (i < s.Length && s[i] == ']') { i++; return list; }
                while (i < s.Length)
                {
                    var v = ReadValue(s, ref i);
                    list.Add(v);
                    SkipWs(s, ref i);
                    if (i < s.Length && s[i] == ',') { i++; continue; }
                    if (i < s.Length && s[i] == ']') { i++; break; }
                }
                return list;
            }

            private static string ReadString(string s, ref int i)
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
                                    {
                                        sb.Append((char)code);
                                    }
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
                if (slice.IndexOf('.') >= 0 ||
                    slice.IndexOf('e') >= 0 || slice.IndexOf('E') >= 0)
                {
                    if (double.TryParse(slice, System.Globalization.NumberStyles.Float,
                        System.Globalization.CultureInfo.InvariantCulture, out var d))
                        return d;
                }
                if (long.TryParse(slice, out var l)) return l;
                return slice;
            }

            private static bool ReadBool(string s, ref int i)
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
