using System;
using UnityEngine;

namespace IntelliVerseX.AI
{
    /// <summary>
    /// Configuration for the IntelliVerseX AI system.
    /// Create via Assets > Create > IntelliVerseX > AI > Configuration.
    /// </summary>
    [CreateAssetMenu(fileName = "IVXAIConfig", menuName = "IntelliVerseX/AI/Configuration", order = 1)]
    public class IVXAIConfig : ScriptableObject
    {
        #region Serialized Fields

        [Header("API Configuration")]
        [Tooltip("Base URL for the IVX AI API")]
        [SerializeField] private string _apiBaseUrl = "https://api.intelli-verse-x.ai/api/ai";

        [Tooltip("API key for authentication (optional when using OAuth/Bearer token)")]
        [SerializeField] private string _apiKey = "";

        [Header("Session Settings")]
        [Tooltip("Interval in seconds for polling messages when WebSocket is unavailable")]
        [Range(0.1f, 2f)]
        [SerializeField] private float _pollingInterval = 0.5f;

        [Tooltip("HTTP request timeout in seconds")]
        [Range(5f, 60f)]
        [SerializeField] private float _requestTimeout = 30f;

        [Tooltip("Prefer WebSocket transport for realtime voice; falls back to HTTP polling")]
        [SerializeField] private bool _preferWebSocket = true;

        [Tooltip("Enable verbose debug logging")]
        [SerializeField] private bool _debugLogging;

        [Header("Audio Settings")]
        [Tooltip("Sample rate for audio playback (IVX AI service uses 16000 Hz)")]
        [SerializeField] private int _audioSampleRate = 16000;

        [Tooltip("Audio channels (1 = mono)")]
        [SerializeField] private int _audioChannels = 1;

        [Tooltip("Buffer size in bytes for audio streaming")]
        [SerializeField] private int _audioBufferSize = 4096;

        [Header("Language")]
        [Tooltip("Default language code (ISO 639-1)")]
        [SerializeField] private string _defaultLanguage = "en";

        [Tooltip("Supported language codes")]
        [SerializeField] private string[] _supportedLanguages = new[]
        {
            "en", "es", "fr", "de", "it", "pt", "ja", "ko",
            "zh", "ar", "hi", "ru", "nl", "pl", "tr", "vi", "th", "id"
        };

        [Header("Free Trial")]
        [Tooltip("Free sessions allowed per day before requiring a purchase")]
        [SerializeField] private int _freeSessionsPerDay = 1;

        [Tooltip("Show upsell prompt during free sessions")]
        [SerializeField] private bool _showUpsellDuringFreeSessions = true;

        [Tooltip("Seconds before session end to show the upsell")]
        [SerializeField] private int _upsellSecondsBeforeEnd = 15;

        [Header("UI Hints")]
        [Tooltip("Show social proof (active users, ratings) in UI")]
        [SerializeField] private bool _showSocialProof = true;

        [Tooltip("Show scarcity messages to drive urgency")]
        [SerializeField] private bool _showScarcityMessages = true;

        [Tooltip("Show session timer in voice UI")]
        [SerializeField] private bool _showSessionTimer = true;

        #endregion

        #region Properties

        public string ApiBaseUrl => _apiBaseUrl;
        public string ApiKey => _apiKey;
        public float PollingInterval => _pollingInterval;
        public float RequestTimeout => _requestTimeout;
        public bool PreferWebSocket => _preferWebSocket;
        public bool DebugLogging => _debugLogging;
        public int AudioSampleRate => _audioSampleRate;
        public int AudioChannels => _audioChannels;
        public int AudioBufferSize => _audioBufferSize;
        public string DefaultLanguage => _defaultLanguage;
        public string[] SupportedLanguages => _supportedLanguages;
        public int FreeSessionsPerDay => _freeSessionsPerDay;
        public bool ShowUpsellDuringFreeSessions => _showUpsellDuringFreeSessions;
        public int UpsellSecondsBeforeEnd => _upsellSecondsBeforeEnd;
        public bool ShowSocialProof => _showSocialProof;
        public bool ShowScarcityMessages => _showScarcityMessages;
        public bool ShowSessionTimer => _showSessionTimer;

        #endregion

        #region Endpoint Helpers

        /// <summary>Full URL for an ai-voice sub-path.</summary>
        public string VoiceEndpoint(string path) => $"{_apiBaseUrl}/ai-voice/{path}";

        /// <summary>Full URL for an ai-host sub-path.</summary>
        public string HostEndpoint(string path) => $"{_apiBaseUrl}/ai-host/{path}";

        public string PersonasEndpoint => VoiceEndpoint("personas");
        public string ProductsEndpoint => VoiceEndpoint("products");
        public string PurchaseEndpoint => VoiceEndpoint("purchase");
        public string VoiceSessionsEndpoint => VoiceEndpoint("sessions");
        public string HostSessionsEndpoint => HostEndpoint("sessions");
        public string HealthEndpoint => VoiceEndpoint("health");

        public string GetEntitlementEndpoint(string userId) => VoiceEndpoint($"entitlements/{userId}");
        public string GetVoiceSessionEndpoint(string sessionId) => VoiceEndpoint($"sessions/{sessionId}");
        public string GetMessagesEndpoint(string sessionId) => VoiceEndpoint($"sessions/{sessionId}/messages");
        public string GetTextEndpoint(string sessionId) => VoiceEndpoint($"sessions/{sessionId}/text");
        public string GetAudioEndpoint(string sessionId) => VoiceEndpoint($"sessions/{sessionId}/audio");
        public string GetAudioCommitEndpoint(string sessionId) => VoiceEndpoint($"sessions/{sessionId}/audio/commit");
        public string GetTriggerEndpoint(string sessionId) => VoiceEndpoint($"sessions/{sessionId}/trigger");

        public string GetHostSessionEndpoint(string sessionId) => HostEndpoint($"sessions/{sessionId}");
        public string GetHostMessagesEndpoint(string sessionId) => HostEndpoint($"sessions/{sessionId}/messages");
        public string GetHostTextEndpoint(string sessionId) => HostEndpoint($"sessions/{sessionId}/text");
        public string GetHostEventsEndpoint(string sessionId) => HostEndpoint($"sessions/{sessionId}/events");
        public string GetHostAnswersEndpoint(string sessionId) => HostEndpoint($"sessions/{sessionId}/answers");
        public string GetHostTriggerEndpoint(string sessionId) => HostEndpoint($"sessions/{sessionId}/trigger");

        /// <summary>WebSocket URL derived from the API base URL (https→wss, http→ws).</summary>
        public string WebSocketUrl
        {
            get
            {
                var wsBase = _apiBaseUrl
                    .Replace("https://", "wss://")
                    .Replace("http://", "ws://");
                return $"{wsBase}/ai-voice/ws";
            }
        }

        #endregion

        #region Validation

        /// <summary>Validates this configuration. Returns false and sets <paramref name="error"/> on failure.</summary>
        public bool Validate(out string error)
        {
            if (string.IsNullOrEmpty(_apiBaseUrl))
            {
                error = "API Base URL is required";
                return false;
            }

            if (_pollingInterval < 0.1f)
            {
                error = "Polling interval must be at least 0.1 seconds";
                return false;
            }

            error = null;
            return true;
        }

        /// <summary>Whether the given ISO 639-1 language code is in the supported list.</summary>
        public bool IsLanguageSupported(string languageCode)
        {
            if (string.IsNullOrEmpty(languageCode)) return false;
            foreach (var lang in _supportedLanguages)
            {
                if (lang.Equals(languageCode, StringComparison.OrdinalIgnoreCase))
                    return true;
            }
            return false;
        }

        #endregion
    }
}
