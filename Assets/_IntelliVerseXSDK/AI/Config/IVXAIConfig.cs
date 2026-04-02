using System;
using UnityEngine;

namespace IntelliVerseX.AI
{
    /// <summary>
    /// AI provider preset.  Use <see cref="IVXAIProvider.Custom"/> when self-hosting an
    /// OpenAI-compatible endpoint (Ollama, Azure OpenAI, vLLM, LiteLLM, etc.).
    /// </summary>
    public enum IVXAIProvider
    {
        /// <summary>IntelliVerseX managed AI API (default).</summary>
        IntelliVerseX,
        /// <summary>Direct OpenAI endpoint.</summary>
        OpenAI,
        /// <summary>Azure OpenAI Service.</summary>
        AzureOpenAI,
        /// <summary>Anthropic Claude API.</summary>
        Anthropic,
        /// <summary>Any OpenAI-compatible endpoint (Ollama, vLLM, LiteLLM, etc.).</summary>
        Custom
    }

    /// <summary>
    /// Configuration for the IntelliVerseX AI system.
    /// Create via Assets > Create > IntelliVerseX > AI > Configuration.
    /// </summary>
    [CreateAssetMenu(fileName = "IVXAIConfig", menuName = "IntelliVerseX/AI/Configuration", order = 1)]
    [HelpURL("https://intelli-verse-x.github.io/Intelli-verse-X-Unity-SDK/modules/ai/")]
    public class IVXAIConfig : ScriptableObject
    {
        #region Serialized Fields

        [Header("API Configuration")]
        [Tooltip("Base URL for the IVX AI API (or any OpenAI-compatible endpoint when using Custom provider)")]
        [SerializeField] private string _apiBaseUrl = "https://api.intelli-verse-x.ai/api/ai";

        [Tooltip("API key for authentication (optional when using OAuth/Bearer token). " +
                 "WARNING: Keys stored here ship inside builds. For production, inject at runtime via SetApiKey().")]
        [SerializeField] private string _apiKey = "";

        [Tooltip("AI provider to use. Set to Custom for self-hosted LLMs (Ollama, vLLM, etc.)")]
        [SerializeField] private IVXAIProvider _provider = IVXAIProvider.IntelliVerseX;

        [Tooltip("Model name passed to the backend (e.g. gpt-4o, claude-3-opus, llama3). Leave empty for server default.")]
        [SerializeField] private string _modelName = "";

        [Header("Developer Mode")]
        [Tooltip("When enabled, all AI managers return canned mock responses without making HTTP calls. " +
                 "Use during development to avoid burning API credits.")]
        [SerializeField] private bool _mockMode;

        [Header("Resilience")]
        [Tooltip("Maximum number of retry attempts for failed HTTP requests (5xx errors only)")]
        [Range(0, 5)]
        [SerializeField] private int _maxRetries = 2;

        [Tooltip("Base delay in seconds between retries (doubles each attempt)")]
        [Range(0.1f, 5f)]
        [SerializeField] private float _retryBaseDelay = 0.5f;

        [Tooltip("Maximum requests per second across all AI managers (0 = unlimited)")]
        [Range(0, 100)]
        [SerializeField] private int _rateLimitPerSecond;

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

        [Header("Collection Limits")]
        [Tooltip("Max conversation history lines kept by IVXAIAssistant (older entries trimmed)")]
        [Range(10, 1000)]
        [SerializeField] private int _maxConversationHistory = 200;

        [Tooltip("Max queued profiling events before oldest are dropped")]
        [Range(50, 10000)]
        [SerializeField] private int _maxEventQueueSize = 2000;

        [Tooltip("Max queued audio clips in IVXAIAudioPlayer")]
        [Range(5, 100)]
        [SerializeField] private int _maxAudioQueueSize = 30;

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

        /// <summary>Base URL for the IVX AI REST API.</summary>
        public string ApiBaseUrl => _apiBaseUrl;

        /// <summary>Optional API key used when bearer-token auth is not available.</summary>
        public string ApiKey => _apiKey;

        /// <summary>Selected AI provider.</summary>
        public IVXAIProvider Provider => _provider;

        /// <summary>Model name sent to the backend (empty = server default).</summary>
        public string ModelName => _modelName;

        /// <summary>When true, managers return mock data without HTTP calls.</summary>
        public bool MockMode => _mockMode;

        /// <summary>Max HTTP retry attempts for 5xx errors.</summary>
        public int MaxRetries => _maxRetries;

        /// <summary>Base delay between retries in seconds.</summary>
        public float RetryBaseDelay => _retryBaseDelay;

        /// <summary>Max requests per second (0 = unlimited).</summary>
        public int RateLimitPerSecond => _rateLimitPerSecond;

        /// <summary>Interval in seconds between HTTP polling requests.</summary>
        public float PollingInterval => _pollingInterval;

        /// <summary>HTTP request timeout in seconds.</summary>
        public float RequestTimeout => _requestTimeout;

        /// <summary>Whether to prefer WebSocket transport over HTTP polling.</summary>
        public bool PreferWebSocket => _preferWebSocket;

        /// <summary>Whether verbose debug logging is enabled.</summary>
        public bool DebugLogging => _debugLogging;

        /// <summary>Audio sample rate in Hz (default 16 000).</summary>
        public int AudioSampleRate => _audioSampleRate;

        /// <summary>Number of audio channels (1 = mono).</summary>
        public int AudioChannels => _audioChannels;

        /// <summary>Buffer size in bytes for streaming audio.</summary>
        public int AudioBufferSize => _audioBufferSize;

        /// <summary>Default ISO 639-1 language code.</summary>
        public string DefaultLanguage => _defaultLanguage;

        /// <summary>Array of ISO 639-1 language codes the backend supports.</summary>
        public string[] SupportedLanguages => _supportedLanguages;

        /// <summary>Max conversation lines kept by IVXAIAssistant.</summary>
        public int MaxConversationHistory => _maxConversationHistory;

        /// <summary>Max queued profiling events before oldest are dropped.</summary>
        public int MaxEventQueueSize => _maxEventQueueSize;

        /// <summary>Max queued audio clips in the audio player.</summary>
        public int MaxAudioQueueSize => _maxAudioQueueSize;

        /// <summary>Number of free voice sessions allowed per user per day.</summary>
        public int FreeSessionsPerDay => _freeSessionsPerDay;

        /// <summary>Whether to show an upsell prompt during free sessions.</summary>
        public bool ShowUpsellDuringFreeSessions => _showUpsellDuringFreeSessions;

        /// <summary>Seconds before session end to display the upsell prompt.</summary>
        public int UpsellSecondsBeforeEnd => _upsellSecondsBeforeEnd;

        /// <summary>Whether to display social-proof data (active users, ratings) in the UI.</summary>
        public bool ShowSocialProof => _showSocialProof;

        /// <summary>Whether to show scarcity/urgency messages to drive conversions.</summary>
        public bool ShowScarcityMessages => _showScarcityMessages;

        /// <summary>Whether to show the remaining-time countdown in the voice UI.</summary>
        public bool ShowSessionTimer => _showSessionTimer;

        #endregion

        #region Runtime API-Key Injection

        /// <summary>
        /// Sets the API key at runtime so it is never baked into the build.
        /// Call this from your server-side auth flow before initializing AI managers.
        /// </summary>
        public void SetApiKey(string key)
        {
            _apiKey = key;
        }

        /// <summary>
        /// Overrides the API base URL at runtime (e.g. staging vs production).
        /// </summary>
        public void SetApiBaseUrl(string url)
        {
            _apiBaseUrl = url;
        }

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
