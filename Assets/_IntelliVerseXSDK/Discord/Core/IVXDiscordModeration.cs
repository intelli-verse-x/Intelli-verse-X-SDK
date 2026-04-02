using System;
using System.Collections.Generic;
using UnityEngine;

namespace IntelliVerseX.Discord
{
    /// <summary>
    /// Describes how moderated message content should be presented.
    /// </summary>
    public enum IVXModerationAction
    {
        /// <summary>Show the message as normal.</summary>
        Show,
        /// <summary>Hide the message entirely.</summary>
        Hide,
        /// <summary>Blur or obscure the message.</summary>
        Blur,
        /// <summary>Replace content with a safe placeholder.</summary>
        Replace
    }

    /// <summary>
    /// Parsed moderation outcome for a single Discord message.
    /// </summary>
    [Serializable]
    public sealed class IVXModerationDecision
    {
        /// <summary>Discord message snowflake ID.</summary>
        public ulong MessageId;
        /// <summary>Recommended presentation action.</summary>
        public IVXModerationAction Action;
        /// <summary>Human-readable moderation reason, if any.</summary>
        public string Reason;
        /// <summary>Replacement text when <see cref="Action"/> is <see cref="IVXModerationAction.Replace"/>.</summary>
        public string Replacement;
        /// <summary>Severity label from the provider (e.g. low, medium, high).</summary>
        public string Severity;
    }

    /// <summary>
    /// Handles Discord Social SDK moderation metadata, optional voice capture for moderation pipelines,
    /// and user reporting. Real behavior requires the Discord Social SDK; otherwise runs in stub mode.
    /// </summary>
    public sealed class IVXDiscordModeration : MonoBehaviour
    {
        #region Constants

        private const string LOG_TAG = "[IVXDiscordModeration]";

        private const string META_ACTION = "action";
        private const string META_REASON = "reason";
        private const string META_REPLACEMENT = "replacement";
        private const string META_SEVERITY = "severity";
        private const string META_MESSAGE_ID = "message_id";
        private const string META_FLAGGED = "flagged";
        private const string META_CONTENT_FLAGGED = "content_flagged";

        #endregion

        #region Private Fields

        private static IVXDiscordModeration _instance;
        private bool _autoModerateEnabled = true;
        private bool _voiceCaptureActive;
        private ulong _voiceCaptureLobbyId;

        #endregion

        #region Properties

        /// <summary>Singleton instance.</summary>
        public static IVXDiscordModeration Instance => _instance;

        /// <summary>When true, incoming moderation metadata is processed and events are raised.</summary>
        public bool AutoModerateEnabled
        {
            get => _autoModerateEnabled;
            set => _autoModerateEnabled = value;
        }

        #endregion

        #region Events

        /// <summary>Moderation metadata was received and parsed for a message.</summary>
        public event Action<IVXModerationDecision> OnModerationDecisionReceived;

        /// <summary>A message was flagged; provides message id and reason.</summary>
        public event Action<ulong, string> OnContentFlagged;

        /// <summary>PCM audio chunk for voice moderation: lobby id, samples, sample rate, channel count.</summary>
        public event Action<ulong, byte[], int, int> OnVoiceDataCaptured;

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
        }

        private void OnDestroy()
        {
            if (_instance == this)
            {
#if INTELLIVERSEX_HAS_DISCORD
                if (_voiceCaptureActive)
                {
                    StopDiscordVoiceCapture();
                }
#endif
                _voiceCaptureActive = false;
                _instance = null;
            }
        }

        private void Start()
        {
#if INTELLIVERSEX_HAS_DISCORD
            RegisterModerationCallbacks();
#endif
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Enables or disables automatic handling of moderation metadata from Discord.
        /// </summary>
        /// <param name="enable">True to process metadata; false to ignore.</param>
        public void EnableAutoModeration(bool enable)
        {
            _autoModerateEnabled = enable;
            Debug.Log($"{LOG_TAG} Auto-moderation {(enable ? "enabled" : "disabled")}.");
        }

        /// <summary>
        /// Parses provider metadata for a message and raises moderation events when <see cref="AutoModerateEnabled"/> is true.
        /// </summary>
        /// <param name="messageId">Discord message id.</param>
        /// <param name="metadata">Key-value moderation fields from the SDK.</param>
        public void ProcessModerationMetadata(ulong messageId, Dictionary<string, string> metadata)
        {
            if (!_autoModerateEnabled || metadata == null)
            {
                return;
            }

            var decision = GetModerationAction(metadata);
            decision.MessageId = messageId;

            OnModerationDecisionReceived?.Invoke(decision);

            if (ShouldRaiseContentFlagged(metadata, decision))
            {
                var reason = string.IsNullOrEmpty(decision.Reason) ? "flagged" : decision.Reason;
                OnContentFlagged?.Invoke(messageId, reason);
            }
        }

        /// <summary>
        /// Builds an <see cref="IVXModerationDecision"/> from a moderation metadata dictionary.
        /// </summary>
        /// <param name="metadata">Key-value pairs (e.g. action, reason, severity).</param>
        /// <returns>A populated decision; <see cref="IVXModerationDecision.MessageId"/> may be set from metadata or default to 0.</returns>
        public static IVXModerationDecision GetModerationAction(Dictionary<string, string> metadata)
        {
            var decision = new IVXModerationDecision
            {
                MessageId = 0,
                Action = IVXModerationAction.Show,
                Reason = string.Empty,
                Replacement = string.Empty,
                Severity = string.Empty
            };

            if (metadata == null || metadata.Count == 0)
            {
                return decision;
            }

            if (TryGetKey(metadata, META_MESSAGE_ID, out var midStr) && ulong.TryParse(midStr, out var mid))
            {
                decision.MessageId = mid;
            }

            if (TryGetKey(metadata, META_ACTION, out var actionStr))
            {
                decision.Action = ParseModerationAction(actionStr);
            }

            if (TryGetKey(metadata, META_REASON, out var reason))
            {
                decision.Reason = reason ?? string.Empty;
            }

            if (TryGetKey(metadata, META_REPLACEMENT, out var replacement))
            {
                decision.Replacement = replacement ?? string.Empty;
            }

            if (TryGetKey(metadata, META_SEVERITY, out var severity))
            {
                decision.Severity = severity ?? string.Empty;
            }

            return decision;
        }

        /// <summary>
        /// Starts capturing voice PCM for a moderation pipeline for the given lobby.
        /// </summary>
        /// <param name="lobbyId">Discord lobby id.</param>
        public void StartVoiceModerationCapture(ulong lobbyId)
        {
            if (_voiceCaptureActive)
            {
                Debug.LogWarning($"{LOG_TAG} Voice moderation capture already active for lobby {_voiceCaptureLobbyId}.");
                return;
            }

            _voiceCaptureLobbyId = lobbyId;
            _voiceCaptureActive = true;

#if INTELLIVERSEX_HAS_DISCORD
            StartDiscordVoiceCapture(lobbyId);
#else
            Debug.Log($"{LOG_TAG} [Stub] Voice moderation capture started for lobby {lobbyId}.");
#endif
        }

        /// <summary>
        /// Stops voice PCM capture for moderation.
        /// </summary>
        public void StopVoiceModerationCapture()
        {
            if (!_voiceCaptureActive)
            {
                return;
            }

#if INTELLIVERSEX_HAS_DISCORD
            StopDiscordVoiceCapture();
#endif

            _voiceCaptureActive = false;
            _voiceCaptureLobbyId = 0;
            Debug.Log($"{LOG_TAG} Voice moderation capture stopped.");
        }

        /// <summary>
        /// Reports a Discord user through the platform report flow.
        /// </summary>
        /// <param name="userId">Discord user snowflake id.</param>
        /// <param name="reason">Report reason text.</param>
        /// <param name="onComplete">Invoked with true if the report UI or API completed successfully.</param>
        public void ReportUser(ulong userId, string reason, Action<bool> onComplete = null)
        {
            if (userId == 0)
            {
                Debug.LogError($"{LOG_TAG} Invalid user id.");
                onComplete?.Invoke(false);
                return;
            }

            Debug.Log($"{LOG_TAG} Reporting user {userId}...");

#if INTELLIVERSEX_HAS_DISCORD
            OpenDiscordReportDialog(userId);
            onComplete?.Invoke(true);
#else
            Debug.Log($"{LOG_TAG} [Stub] ReportUser({userId}): {reason}");
            onComplete?.Invoke(true);
#endif
        }

        #endregion

        #region Private Methods

        private static bool ShouldRaiseContentFlagged(Dictionary<string, string> metadata, IVXModerationDecision decision)
        {
            if (TryGetKey(metadata, META_FLAGGED, out var f) && IsTruthyFlag(f))
            {
                return true;
            }

            if (TryGetKey(metadata, META_CONTENT_FLAGGED, out var cf) && IsTruthyFlag(cf))
            {
                return true;
            }

            switch (decision.Action)
            {
                case IVXModerationAction.Hide:
                case IVXModerationAction.Blur:
                case IVXModerationAction.Replace:
                    return true;
                default:
                    return false;
            }
        }

        private static bool IsTruthyFlag(string value)
        {
            if (string.IsNullOrEmpty(value)) return false;
            var v = value.Trim();
            return v.Equals("1", StringComparison.OrdinalIgnoreCase)
                   || v.Equals("true", StringComparison.OrdinalIgnoreCase)
                   || v.Equals("yes", StringComparison.OrdinalIgnoreCase);
        }

        private static bool TryGetKey(Dictionary<string, string> metadata, string key, out string value)
        {
            foreach (var kv in metadata)
            {
                if (string.Equals(kv.Key, key, StringComparison.OrdinalIgnoreCase))
                {
                    value = kv.Value;
                    return true;
                }
            }

            value = null;
            return false;
        }

        private static IVXModerationAction ParseModerationAction(string raw)
        {
            if (string.IsNullOrEmpty(raw))
            {
                return IVXModerationAction.Show;
            }

            switch (raw.Trim().ToLowerInvariant())
            {
                case "hide":
                    return IVXModerationAction.Hide;
                case "blur":
                    return IVXModerationAction.Blur;
                case "replace":
                    return IVXModerationAction.Replace;
                case "show":
                default:
                    return IVXModerationAction.Show;
            }
        }

#if INTELLIVERSEX_HAS_DISCORD
        private discordpp.Client Client => IVXDiscordManager.Instance?.DiscordClient;

        private void RegisterModerationCallbacks()
        {
            var client = Client;
            if (client == null) return;
            try
            {
                client.SetUserMessageUpdatedCallback((msg) =>
                {
                    if (msg.ModerationMetadata != null && msg.ModerationMetadata.Count > 0)
                    {
                        var metadata = new Dictionary<string, string>();
                        foreach (var kv in msg.ModerationMetadata)
                            metadata[kv.Key] = kv.Value;
                        ProcessModerationMetadata(msg.Id, metadata);
                    }
                });
            }
            catch (Exception e) { Debug.LogError($"{LOG_TAG} RegisterModerationCallbacks error: {e.Message}"); }
        }

        private void StartDiscordVoiceCapture(ulong lobbyId)
        {
            var client = Client;
            if (client == null) return;
            try
            {
                client.SetAudioCapturedCallback((data, samplesPerChannel, sampleRate, channels) =>
                {
                    byte[] pcm = new byte[data.Length * 2];
                    Buffer.BlockCopy(data, 0, pcm, 0, pcm.Length);
                    OnVoiceDataCaptured?.Invoke(lobbyId, pcm, sampleRate, channels);
                });
            }
            catch (Exception e) { Debug.LogError($"{LOG_TAG} StartVoiceCapture error: {e.Message}"); }
        }

        private void StopDiscordVoiceCapture()
        {
            try { Client?.SetAudioCapturedCallback(null); }
            catch (Exception e) { Debug.LogError($"{LOG_TAG} StopVoiceCapture error: {e.Message}"); }
        }

        private void OpenDiscordReportDialog(ulong userId)
        {
            try { Client?.OpenUserProfileInDiscord(userId); }
            catch (Exception e) { Debug.LogError($"{LOG_TAG} OpenReportDialog error: {e.Message}"); }
        }
#endif

        #endregion
    }
}
