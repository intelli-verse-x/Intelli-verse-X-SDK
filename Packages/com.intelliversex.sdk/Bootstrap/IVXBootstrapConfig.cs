using UnityEngine;
using IntelliVerseX.Core;

namespace IntelliVerseX.Bootstrap
{
    /// <summary>
    /// Canonical Game ID and Nakama host config for the IntelliVerseX SDK.
    /// Create via Assets &gt; Create &gt; IntelliVerseX &gt; Bootstrap Config.
    /// Paste or create the Game ID here; <see cref="Validate"/> fails when it is empty.
    /// Nakama host/key are for maintainers / self-host only — not shown in Control Center.
    /// </summary>
    [CreateAssetMenu(fileName = "IVXBootstrapConfig", menuName = "IntelliVerseX/Bootstrap Config", order = 0)]
    [HelpURL("https://intelli-verse-x.github.io/Intelli-verse-X-SDK/getting-started/quickstart/")]
    public sealed class IVXBootstrapConfig : ScriptableObject
    {
        #region Serialized Fields

        [Header("Game Identity")]
        [Tooltip("Your Game ID (UUID) from IntelliVerseX Control Center (Auth V2 + unique-appid), " +
                 "the developers dashboard, or the CreateGame API.")]
        [SerializeField] private string _gameId = "";

        [Tooltip("Display name for your game (used in analytics and backend metadata)")]
        [SerializeField] private string _gameName = "";

        [Header("Backend (Nakama)")]
        [Tooltip("Nakama server hostname. Defaults to IntelliVerseX cloud. Change only if you self-host.")]
        [SerializeField] private string _serverHost = IVXNakamaConfig.HOST;

        [Tooltip("Nakama server port. Cloud default: 443 (HTTPS).")]
        [SerializeField] private int _serverPort = IVXNakamaConfig.PORT;

        [Tooltip("Nakama server key. Never show this in consumer Control Center UI. Change from cloud default only for self-hosted backends.")]
        [SerializeField] private string _serverKey = IVXNakamaConfig.SERVER_KEY;

        [Tooltip("Enable HTTPS/WSS for production servers")]
        [SerializeField] private bool _useSSL = true;

        [Tooltip("Automatically authenticate with device ID on startup")]
        [SerializeField] private bool _autoDeviceAuth = true;

        [Tooltip("Persist session token between launches")]
        [SerializeField] private bool _persistSession = true;

        [Header("Module Configs")]
        [Tooltip("Reference to IVXAIConfig ScriptableObject for AI features")]
        [SerializeField] private ScriptableObject _aiConfig;

        [Tooltip("Reference to IVXDiscordConfig ScriptableObject for Discord integration")]
        [SerializeField] private ScriptableObject _discordConfig;

        [Header("Feature Toggles")]
        [Tooltip("Enable Hiro live-ops systems (requires Nakama)")]
        [SerializeField] private bool _enableHiro = true;

        [Tooltip("Enable Satori analytics (requires Nakama)")]
        [SerializeField] private bool _enableSatori = true;

        [Tooltip("Enable AI conversational & LLM stack")]
        [SerializeField] private bool _enableAI = true;

        [Tooltip("Enable Discord Social SDK integration")]
        [SerializeField] private bool _enableDiscord = true;

        [Tooltip("Enable multiplayer game modes")]
        [SerializeField] private bool _enableMultiplayer = true;

        [Tooltip("Enable platform optimizations")]
        [SerializeField] private bool _enablePlatform = true;

        [Header("Debug")]
        [Tooltip("Enable verbose bootstrap logging")]
        [SerializeField] private bool _debugLogging = true;

        #endregion

        #region Properties

        /// <summary>Game ID (UUID) registered on the IntelliVerseX platform.</summary>
        public string GameId => _gameId;
        /// <summary>Display name for the game.</summary>
        public string GameName => _gameName;
        /// <summary>Nakama server host.</summary>
        public string ServerHost => _serverHost;
        /// <summary>Nakama server port.</summary>
        public int ServerPort => _serverPort;
        /// <summary>Nakama server key.</summary>
        public string ServerKey => _serverKey;
        /// <summary>Whether to use SSL.</summary>
        public bool UseSSL => _useSSL;
        /// <summary>Auto-authenticate with device ID on startup.</summary>
        public bool AutoDeviceAuth => _autoDeviceAuth;
        /// <summary>Persist session token between launches.</summary>
        public bool PersistSession => _persistSession;
        /// <summary>AI module configuration. Cast to IVXAIConfig at runtime.</summary>
        public ScriptableObject AIConfig => _aiConfig;
        /// <summary>Discord module configuration. Cast to IVXDiscordConfig at runtime.</summary>
        public ScriptableObject DiscordConfig => _discordConfig;
        /// <summary>Enable Hiro live-ops.</summary>
        public bool EnableHiro => _enableHiro;
        /// <summary>Enable Satori analytics.</summary>
        public bool EnableSatori => _enableSatori;
        /// <summary>Enable AI stack.</summary>
        public bool EnableAI => _enableAI;
        /// <summary>Enable Discord Social.</summary>
        public bool EnableDiscord => _enableDiscord;
        /// <summary>Enable multiplayer.</summary>
        public bool EnableMultiplayer => _enableMultiplayer;
        /// <summary>Enable platform optimizer.</summary>
        public bool EnablePlatform => _enablePlatform;
        /// <summary>Verbose logging.</summary>
        public bool DebugLogging => _debugLogging;

        #endregion

        /// <summary>
        /// Validates critical fields. Logs warnings at <paramref name="logWarnings"/> time
        /// (runtime bootstrap / Control Center). Inspector OnValidate stays quiet to avoid spam.
        /// </summary>
        public bool Validate(bool logWarnings = true)
        {
            bool valid = true;
            if (string.IsNullOrWhiteSpace(_gameId))
            {
                if (logWarnings)
                    Debug.LogWarning("[IVXBootstrapConfig] Game ID is empty. Get yours from https://intelli-verse-x.ai/developers or POST to msapi.intelli-verse-x.io/api/games/game/info");
                valid = false;
            }
            if (logWarnings && (_serverHost == "127.0.0.1" || _serverHost == "localhost"))
                Debug.LogWarning($"[IVXBootstrapConfig] Server host is '{_serverHost}'. Change this before shipping to production.");
            // Shared IntelliVerseX cloud uses the platform defaultkey; only warn when self-hosting with that default.
            if (logWarnings
                && _serverKey == "defaultkey"
                && !string.Equals(_serverHost, IVXNakamaConfig.HOST, System.StringComparison.OrdinalIgnoreCase))
                Debug.LogWarning("[IVXBootstrapConfig] Using default Nakama server key on a non-cloud host. Change this for self-hosted backends.");
            if (_serverPort <= 0 || _serverPort > 65535)
            {
                if (logWarnings)
                    Debug.LogError($"[IVXBootstrapConfig] Invalid server port: {_serverPort}. Must be 1-65535.");
                valid = false;
            }
            return valid;
        }

        #if UNITY_EDITOR
        private void OnValidate()
        {
            // Silent structural check only — do not spam Console while selecting the asset.
            Validate(logWarnings: false);
        }
        #endif
    }
}
