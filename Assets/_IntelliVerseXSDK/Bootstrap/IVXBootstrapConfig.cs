using UnityEngine;

namespace IntelliVerseX.Bootstrap
{
    /// <summary>
    /// Master configuration for the IntelliVerseX SDK.
    /// Create via Assets > Create > IntelliVerseX > Bootstrap Config.
    /// Holds references to all module configs plus backend settings.
    /// </summary>
    [CreateAssetMenu(fileName = "IVXBootstrapConfig", menuName = "IntelliVerseX/Bootstrap Config", order = 0)]
    public sealed class IVXBootstrapConfig : ScriptableObject
    {
        #region Serialized Fields

        [Header("Backend (Nakama)")]
        [Tooltip("Nakama server host (e.g. 127.0.0.1 or your-server.com)")]
        [SerializeField] private string _serverHost = "127.0.0.1";

        [Tooltip("Nakama server port (default 7350)")]
        [SerializeField] private int _serverPort = 7350;

        [Tooltip("Nakama server key")]
        [SerializeField] private string _serverKey = "defaultkey";

        [Tooltip("Use SSL for Nakama connection")]
        [SerializeField] private bool _useSSL;

        [Tooltip("Automatically authenticate with device ID on startup")]
        [SerializeField] private bool _autoDeviceAuth = true;

        [Tooltip("Persist session token between launches")]
        [SerializeField] private bool _persistSession = true;

        [Header("Module Configs")]
        [Tooltip("AI module configuration (create via IntelliVerseX > AI > Configuration)")]
        [SerializeField] private ScriptableObject _aiConfig;

        [Tooltip("Discord module configuration (create via IntelliVerseX > Discord Config)")]
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
    }
}
