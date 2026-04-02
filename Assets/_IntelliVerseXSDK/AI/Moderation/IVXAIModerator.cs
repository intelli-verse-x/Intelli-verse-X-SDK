using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Networking;

namespace IntelliVerseX.AI
{
    #region Enums

    /// <summary>High-level classification bucket for user-generated text.</summary>
    public enum IVXContentCategory
    {
        /// <summary>No policy issues detected.</summary>
        Clean,
        /// <summary>General toxic or abusive language.</summary>
        Toxic,
        /// <summary>Unwanted or repetitive promotional content.</summary>
        Spam,
        /// <summary>Personally identifiable information.</summary>
        PII,
        /// <summary>Harassing or bullying language.</summary>
        Harassment,
        /// <summary>Hate speech or slurs.</summary>
        HateSpeech,
        /// <summary>Self-harm or suicide-related content.</summary>
        SelfHarm,
        /// <summary>Sexual content.</summary>
        Sexual,
        /// <summary>Graphic violence or threats.</summary>
        Violence,
        /// <summary>Custom or vendor-specific category.</summary>
        Custom
    }

    /// <summary>Estimated severity of a policy violation.</summary>
    public enum IVXModerationSeverity
    {
        /// <summary>No violation.</summary>
        None,
        /// <summary>Mild concern.</summary>
        Low,
        /// <summary>Moderate concern.</summary>
        Medium,
        /// <summary>Strong concern.</summary>
        High,
        /// <summary>Immediate safety or legal risk.</summary>
        Critical
    }

    /// <summary>Suggested moderation outcome.</summary>
    public enum IVXModerationActionType
    {
        /// <summary>Allow the message as-is.</summary>
        Allow,
        /// <summary>Allow but show a warning to the user.</summary>
        Warn,
        /// <summary>Substitute sanitized text.</summary>
        Replace,
        /// <summary>Reject the message.</summary>
        Block,
        /// <summary>Send to human or async review.</summary>
        Flag
    }

    #endregion

    #region Client data models

    /// <summary>
    /// Result of classifying or scanning a single piece of text.
    /// </summary>
    [Serializable]
    public sealed class IVXModerationResult
    {
        /// <summary>Assigned content category.</summary>
        public IVXContentCategory Category;

        /// <summary>Severity estimate.</summary>
        public IVXModerationSeverity Severity;

        /// <summary>Model confidence in <c>[0,1]</c>.</summary>
        public float Confidence;

        /// <summary>Recommended action for the client or human reviewer.</summary>
        public IVXModerationActionType SuggestedAction;

        /// <summary>Sanitized replacement when <see cref="SuggestedAction"/> is <see cref="IVXModerationActionType.Replace"/>.</summary>
        public string Replacement;

        /// <summary>Original user text that was evaluated.</summary>
        public string OriginalText;
    }

    /// <summary>
    /// A client-defined rule evaluated locally before remote classification.
    /// </summary>
    [Serializable]
    public sealed class IVXModerationRule
    {
        /// <summary>Regular expression pattern, or literal keyword if not valid regex.</summary>
        public string Pattern;

        /// <summary>Category to assign when this rule matches.</summary>
        public IVXContentCategory Category;

        /// <summary>Action to suggest when this rule matches.</summary>
        public IVXModerationActionType Action;

        /// <summary>Replacement text for <see cref="IVXModerationActionType.Replace"/>.</summary>
        public string ReplacementText;
    }

    #endregion

    #region API wire models

    /// <summary>POST body for classify and filter endpoints.</summary>
    public sealed class IVXModerationRequest
    {
        /// <summary>Text to evaluate.</summary>
        [JsonProperty("text")]
        public string Text { get; set; }

        /// <summary>Optional rule overrides sent to the server.</summary>
        [JsonProperty("rules")]
        public List<IVXModerationRule> Rules { get; set; }

        /// <summary>Optional structured context (channel, locale, etc.).</summary>
        [JsonProperty("context")]
        public string Context { get; set; }
    }

    /// <summary>Single moderation decision from the API.</summary>
    public sealed class IVXModerationResponse
    {
        /// <summary>Category label from the service (maps to <see cref="IVXContentCategory"/>).</summary>
        [JsonProperty("category")]
        public string Category { get; set; }

        /// <summary>Severity label from the service (maps to <see cref="IVXModerationSeverity"/>).</summary>
        [JsonProperty("severity")]
        public string Severity { get; set; }

        /// <summary>Confidence score in the range <c>[0,1]</c>.</summary>
        [JsonProperty("confidence")]
        public float Confidence { get; set; }

        /// <summary>Recommended action label (maps to <see cref="IVXModerationActionType"/>).</summary>
        [JsonProperty("action")]
        public string Action { get; set; }

        /// <summary>Sanitized replacement text when the action is replace.</summary>
        [JsonProperty("replacement")]
        public string Replacement { get; set; }
    }

    /// <summary>Batch moderation API response.</summary>
    public sealed class IVXBatchModerationResponse
    {
        /// <summary>One entry per input message, in the same order as the request.</summary>
        [JsonProperty("results")]
        public List<IVXModerationResponse> Results { get; set; }
    }

    /// <summary>Request body for the batch moderation endpoint.</summary>
    public sealed class IVXBatchModerationRequest
    {
        /// <summary>Messages to score in order.</summary>
        [JsonProperty("messages")]
        public List<string> Messages { get; set; }

        /// <summary>Optional rules forwarded to the server.</summary>
        [JsonProperty("rules")]
        public List<IVXModerationRule> Rules { get; set; }

        /// <summary>Optional context string.</summary>
        [JsonProperty("context")]
        public string Context { get; set; }
    }

    /// <summary>Filter endpoint response carrying sanitized text.</summary>
    public sealed class IVXModerationFilterResponse
    {
        /// <summary>Primary filtered output field.</summary>
        [JsonProperty("text")]
        public string Text { get; set; }

        /// <summary>Alternate field name used by some backends.</summary>
        [JsonProperty("filtered")]
        public string Filtered { get; set; }

        /// <summary>Another common alias for filtered output.</summary>
        [JsonProperty("filtered_text")]
        public string FilteredText { get; set; }
    }

    #endregion

    /// <summary>
    /// Client-side moderation helper: local rules plus remote classify / filter / batch APIs.
    /// </summary>
    public sealed class IVXAIModerator : MonoBehaviour
    {
        #region Singleton

        private static IVXAIModerator _instance;

        /// <summary>Singleton instance (lazy-resolved if not yet registered).</summary>
        public static IVXAIModerator Instance
        {
            get
            {
                if (_instance == null)
                    _instance = FindFirstObjectByType<IVXAIModerator>();
                return _instance;
            }
        }

        #endregion

        #region Serialized Fields

        [Header("Configuration")]
        [SerializeField] private IVXAIConfig _config;

        [Tooltip("When false, remote calls are skipped and only local rules apply where applicable.")]
        [SerializeField] private bool _remoteEnabled = true;

        #endregion

        #region Private Fields

        private readonly List<IVXModerationRule> _customRules = new List<IVXModerationRule>();
        private readonly List<CompiledModerationRule> _compiledRules = new List<CompiledModerationRule>();
        private bool _initialized;

        #endregion

        #region Events

        /// <summary>Raised when content should be reviewed or flagged.</summary>
        public event Action<IVXModerationResult> OnContentFlagged;

        /// <summary>Raised when content is blocked; arguments are original text and reason.</summary>
        public event Action<string, string> OnContentBlocked;

        /// <summary>Raised when content is replaced; arguments are original and replacement strings.</summary>
        public event Action<string, string> OnContentReplaced;

        #endregion

        #region Properties

        /// <summary>Whether remote moderation calls are allowed.</summary>
        public bool IsEnabled => _initialized && _config != null && _remoteEnabled;

        /// <summary>Active custom rules (read-only view).</summary>
        public IReadOnlyList<IVXModerationRule> CustomRules => _customRules;

        #endregion

        #region Unity Lifecycle

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private void OnDestroy()
        {
            if (_instance == this)
                _instance = null;
        }

        #endregion

        #region Initialization

        /// <summary>
        /// Binds configuration and marks the moderator ready for use.
        /// </summary>
        /// <param name="config">API configuration; must not be null.</param>
        public void Initialize(IVXAIConfig config)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
            if (!_config.Validate(out string err))
            {
                Debug.LogError($"[{nameof(IVXAIModerator)}] Invalid config: {err}");
                _initialized = false;
                return;
            }

            _initialized = true;
        }

        #endregion

        #region Public API — Classification & filtering

        /// <summary>
        /// Classifies a single string. Local rules run first; the API is used when no local rule applies.
        /// </summary>
        public void ClassifyText(string text, Action<IVXModerationResult> callback)
        {
            if (callback == null)
                throw new ArgumentNullException(nameof(callback));

            string original = text ?? string.Empty;
            IVXModerationResult local = CheckLocalRules(original);
            if (local.SuggestedAction != IVXModerationActionType.Allow)
            {
                RaiseLocalEvents(local);
                callback(local);
                return;
            }

            if (!IsEnabled)
            {
                callback(local);
                return;
            }

            var body = new IVXModerationRequest
            {
                Text = original,
                Rules = _customRules.Count > 0 ? new List<IVXModerationRule>(_customRules) : null,
                Context = null
            };

            PostJson<IVXModerationResponse>(ModerationClassifyUrl(), body,
                response =>
                {
                    IVXModerationResult r = MapResponse(response, original);
                    RaiseLocalEvents(r);
                    callback(r);
                },
                err => { Debug.LogWarning($"[{nameof(IVXAIModerator)}] {err}"); callback(local); });
        }

        /// <summary>
        /// Applies local replacement rules, then calls the remote filter endpoint for final sanitization.
        /// </summary>
        public void FilterMessage(string text, Action<string> onFiltered)
        {
            if (onFiltered == null)
                throw new ArgumentNullException(nameof(onFiltered));

            string working = text ?? string.Empty;
            working = ApplyLocalReplacements(working);

            IVXModerationResult blockCheck = CheckLocalRules(working);
            if (blockCheck.SuggestedAction == IVXModerationActionType.Block)
            {
                OnContentBlocked?.Invoke(working, "Blocked by local rule");
                onFiltered(string.Empty);
                return;
            }

            if (!IsEnabled)
            {
                onFiltered(working);
                return;
            }

            var body = new IVXModerationRequest
            {
                Text = working,
                Rules = _customRules.Count > 0 ? new List<IVXModerationRule>(_customRules) : null,
                Context = null
            };

            PostJson<IVXModerationFilterResponse>(ModerationFilterUrl(), body,
                response =>
                {
                    string sanitized = PickFilteredFromFilterResponse(response);
                    if (string.IsNullOrEmpty(sanitized))
                        sanitized = working;
                    if (!string.Equals(working, sanitized, StringComparison.Ordinal))
                        OnContentReplaced?.Invoke(working, sanitized);
                    onFiltered(sanitized);
                },
                err =>
                {
                    Debug.LogWarning($"[{nameof(IVXAIModerator)}] Filter failed: {err}");
                    onFiltered(working);
                });
        }

        /// <summary>
        /// Scores multiple messages in one HTTP request. Results align by index with <paramref name="messages"/>.
        /// </summary>
        public void ScanBatch(List<string> messages, Action<List<IVXModerationResult>> onComplete)
        {
            if (onComplete == null)
                throw new ArgumentNullException(nameof(onComplete));
            if (messages == null)
            {
                onComplete(new List<IVXModerationResult>());
                return;
            }

            if (!IsEnabled)
            {
                var localOnly = new List<IVXModerationResult>(messages.Count);
                foreach (string m in messages)
                {
                    IVXModerationResult r = CheckLocalRules(m ?? string.Empty);
                    RaiseLocalEvents(r);
                    localOnly.Add(r);
                }

                onComplete(localOnly);
                return;
            }

            var body = new IVXBatchModerationRequest
            {
                Messages = new List<string>(messages),
                Rules = _customRules.Count > 0 ? new List<IVXModerationRule>(_customRules) : null,
                Context = null
            };

            PostJson<IVXBatchModerationResponse>(ModerationBatchUrl(), body,
                batch =>
                {
                    var results = new List<IVXModerationResult>(messages.Count);
                    List<IVXModerationResponse> list = batch?.Results;
                    for (int i = 0; i < messages.Count; i++)
                    {
                        string orig = messages[i] ?? string.Empty;
                        IVXModerationResponse row = list != null && i < list.Count ? list[i] : null;
                        IVXModerationResult r = row != null ? MapResponse(row, orig) : CheckLocalRules(orig);
                        RaiseLocalEvents(r);
                        results.Add(r);
                    }

                    onComplete(results);
                },
                err =>
                {
                    Debug.LogWarning($"[{nameof(IVXAIModerator)}] Batch failed: {err}");
                    var fallback = new List<IVXModerationResult>(messages.Count);
                    foreach (string m in messages)
                        fallback.Add(CheckLocalRules(m ?? string.Empty));
                    onComplete(fallback);
                });
        }

        #endregion

        #region Public API — Rules

        /// <summary>Adds a custom rule and refreshes local matchers.</summary>
        public void AddCustomRule(IVXModerationRule rule)
        {
            if (rule == null || string.IsNullOrEmpty(rule.Pattern))
                return;
            _customRules.Add(rule);
            RebuildCompiledRules();
        }

        /// <summary>Removes the first rule whose pattern matches exactly.</summary>
        public void RemoveCustomRule(string pattern)
        {
            if (string.IsNullOrEmpty(pattern))
                return;
            for (int i = _customRules.Count - 1; i >= 0; i--)
            {
                if (string.Equals(_customRules[i].Pattern, pattern, StringComparison.Ordinal))
                    _customRules.RemoveAt(i);
            }

            RebuildCompiledRules();
        }

        /// <summary>Replaces the full custom rule list.</summary>
        public void SetCustomRules(List<IVXModerationRule> rules)
        {
            _customRules.Clear();
            if (rules != null)
            {
                foreach (IVXModerationRule r in rules)
                {
                    if (r != null && !string.IsNullOrEmpty(r.Pattern))
                        _customRules.Add(r);
                }
            }

            RebuildCompiledRules();
        }

        /// <summary>Clears all custom rules.</summary>
        public void ClearCustomRules()
        {
            _customRules.Clear();
            _compiledRules.Clear();
        }

        /// <summary>
        /// Fast synchronous scan using configured custom rules only (no HTTP).
        /// </summary>
        public IVXModerationResult CheckLocalRules(string text)
        {
            string original = text ?? string.Empty;
            foreach (CompiledModerationRule compiled in _compiledRules)
            {
                if (compiled.TryMatch(original, out IVXModerationResult hit))
                    return hit;
            }

            return new IVXModerationResult
            {
                Category = IVXContentCategory.Clean,
                Severity = IVXModerationSeverity.None,
                Confidence = 1f,
                SuggestedAction = IVXModerationActionType.Allow,
                Replacement = original,
                OriginalText = original
            };
        }

        #endregion

        #region Public API — Integrations

        /// <summary>
        /// Maps a moderation result into Discord-oriented metadata key/value pairs.
        /// </summary>
        public Dictionary<string, string> GetDiscordModerationMetadata(IVXModerationResult result)
        {
            if (result == null)
                return new Dictionary<string, string>();

            return new Dictionary<string, string>
            {
                ["ivx.category"] = result.Category.ToString(),
                ["ivx.severity"] = result.Severity.ToString(),
                ["ivx.confidence"] = result.Confidence.ToString("0.###", CultureInfo.InvariantCulture),
                ["ivx.action"] = result.SuggestedAction.ToString(),
                ["ivx.has_replacement"] = string.IsNullOrEmpty(result.Replacement) ? "false" : "true"
            };
        }

        #endregion

        #region Private — HTTP

        private string ModerationClassifyUrl() => $"{TrimBase(_config.ApiBaseUrl)}/moderation/classify";

        private string ModerationFilterUrl() => $"{TrimBase(_config.ApiBaseUrl)}/moderation/filter";

        private string ModerationBatchUrl() => $"{TrimBase(_config.ApiBaseUrl)}/moderation/batch";

        private static string TrimBase(string baseUrl)
        {
            if (string.IsNullOrEmpty(baseUrl))
                return string.Empty;
            return baseUrl.TrimEnd('/');
        }

        private void PostJson<T>(string url, object body, Action<T> onSuccess, Action<string> onError) where T : class
        {
            StartCoroutine(PostCoroutine(url, body,
                json =>
                {
                    try
                    {
                        T parsed = JsonConvert.DeserializeObject<T>(json);
                        if (parsed == null)
                        {
                            onError?.Invoke("Empty or invalid JSON response");
                            return;
                        }

                        onSuccess?.Invoke(parsed);
                    }
                    catch (Exception ex)
                    {
                        onError?.Invoke(ex.Message);
                    }
                }, onError));
        }

        private IEnumerator PostCoroutine(string url, object body, Action<string> onSuccess, Action<string> onError)
        {
            string json = JsonConvert.SerializeObject(body, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
            var request = new UnityWebRequest(url, UnityWebRequest.kHttpVerbPOST)
            {
                uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(json)),
                downloadHandler = new DownloadHandlerBuffer()
            };
            request.SetRequestHeader("Content-Type", "application/json");
            ApplyHeaders(request);
            request.timeout = (int)_config.RequestTimeout;

            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                onError?.Invoke($"{request.method} {request.url} failed: {request.error}");
                request.Dispose();
                yield break;
            }

            try
            {
                onSuccess?.Invoke(request.downloadHandler.text);
            }
            finally
            {
                request.Dispose();
            }
        }

        private void ApplyHeaders(UnityWebRequest request)
        {
            if (!string.IsNullOrEmpty(_config.ApiKey))
                request.SetRequestHeader("X-API-Key", _config.ApiKey);
        }

        #endregion

        #region Private — Mapping & rules

        private void RebuildCompiledRules()
        {
            _compiledRules.Clear();
            foreach (IVXModerationRule rule in _customRules)
                _compiledRules.Add(new CompiledModerationRule(rule));
        }

        private static IVXModerationResult MapResponse(IVXModerationResponse response, string originalText)
        {
            var r = new IVXModerationResult
            {
                OriginalText = originalText ?? string.Empty,
                Confidence = Mathf.Clamp01(response.Confidence),
                Replacement = response.Replacement ?? originalText
            };

            r.Category = ParseEnum(response.Category, IVXContentCategory.Custom);
            r.Severity = ParseEnum(response.Severity, IVXModerationSeverity.None);
            r.SuggestedAction = ParseEnum(response.Action, IVXModerationActionType.Allow);

            return r;
        }

        private static TEnum ParseEnum<TEnum>(string value, TEnum fallback) where TEnum : struct, Enum
        {
            if (string.IsNullOrEmpty(value))
                return fallback;
            if (Enum.TryParse(value, true, out TEnum direct))
                return direct;
            string norm = NormalizeEnumToken(value);
            foreach (string name in Enum.GetNames(typeof(TEnum)))
            {
                if (norm.Equals(NormalizeEnumToken(name), StringComparison.OrdinalIgnoreCase))
                {
                    Enum.TryParse(name, true, out TEnum parsed);
                    return parsed;
                }
            }

            return fallback;
        }

        private static string NormalizeEnumToken(string s)
        {
            if (string.IsNullOrEmpty(s))
                return s;
            return s.Replace("-", string.Empty).Replace("_", string.Empty);
        }

        private void RaiseLocalEvents(IVXModerationResult r)
        {
            if (r == null)
                return;
            if (r.SuggestedAction == IVXModerationActionType.Flag || r.Category != IVXContentCategory.Clean)
                OnContentFlagged?.Invoke(r);
            if (r.SuggestedAction == IVXModerationActionType.Block)
                OnContentBlocked?.Invoke(r.OriginalText, r.Category.ToString());
            if (r.SuggestedAction == IVXModerationActionType.Replace &&
                !string.Equals(r.OriginalText, r.Replacement, StringComparison.Ordinal))
                OnContentReplaced?.Invoke(r.OriginalText, r.Replacement);
        }

        private string ApplyLocalReplacements(string text)
        {
            string working = text;
            foreach (CompiledModerationRule compiled in _compiledRules)
            {
                if (compiled.Rule.Action == IVXModerationActionType.Replace)
                    working = compiled.ApplyReplace(working);
            }

            return working;
        }

        private static string PickFilteredFromFilterResponse(IVXModerationFilterResponse r)
        {
            if (r == null)
                return string.Empty;
            if (!string.IsNullOrEmpty(r.Text))
                return r.Text;
            if (!string.IsNullOrEmpty(r.Filtered))
                return r.Filtered;
            if (!string.IsNullOrEmpty(r.FilteredText))
                return r.FilteredText;
            return string.Empty;
        }

        private sealed class CompiledModerationRule
        {
            public readonly IVXModerationRule Rule;
            private readonly Regex _regex;
            private readonly bool _useRegex;

            public CompiledModerationRule(IVXModerationRule rule)
            {
                Rule = rule;
                if (string.IsNullOrEmpty(rule.Pattern))
                {
                    _useRegex = false;
                    return;
                }

                try
                {
                    _regex = new Regex(rule.Pattern, RegexOptions.CultureInvariant | RegexOptions.IgnoreCase,
                        TimeSpan.FromMilliseconds(250));
                    _useRegex = true;
                }
                catch (ArgumentException)
                {
                    _useRegex = false;
                }
            }

            public bool TryMatch(string text, out IVXModerationResult hit)
            {
                hit = null;
                if (string.IsNullOrEmpty(Rule.Pattern))
                    return false;

                bool matched = _useRegex ? _regex.IsMatch(text) : text.IndexOf(Rule.Pattern, StringComparison.OrdinalIgnoreCase) >= 0;
                if (!matched)
                    return false;

                hit = new IVXModerationResult
                {
                    OriginalText = text,
                    Category = Rule.Category,
                    Severity = Rule.Action == IVXModerationActionType.Block ? IVXModerationSeverity.High : IVXModerationSeverity.Medium,
                    Confidence = 1f,
                    SuggestedAction = Rule.Action,
                    Replacement = Rule.Action == IVXModerationActionType.Replace
                        ? ApplyReplace(text)
                        : text
                };
                return true;
            }

            public string ApplyReplace(string text)
            {
                if (Rule.Action != IVXModerationActionType.Replace)
                    return text;
                string replacement = Rule.ReplacementText ?? string.Empty;
                if (_useRegex)
                    return _regex.Replace(text, replacement);
                return text.Replace(Rule.Pattern, replacement, StringComparison.OrdinalIgnoreCase);
            }
        }

        #endregion
    }
}
