namespace IntelliVerseX.Core
{
    /// <summary>
    /// Nakama backend configuration for IntelliVerse-X platform.
    /// 
    /// This is a SHARED Nakama server across ALL IntelliVerse-X games.
    /// All games connect to the same backend for:
    /// - User storage (game progress, settings)
    /// - Leaderboards (per-game and global)
    /// - Wallets (game-specific and global)
    /// - Matchmaking (multiplayer games)
    /// - Real-time multiplayer
    /// 
    /// Server Information:
    /// - Host: nakama-rest.intelli-verse-x.ai
    /// - Port: 443 (HTTPS)
    /// - Protocol: HTTPS
    /// - Server Key: defaultkey
    /// - Environment: Production
    /// 
    /// Usage:
    /// - All games use this server
    /// - Data is separated by game ID
    /// - Per-game collections and leaderboards
    /// - Shared global wallet across games
    /// 
    /// Security Notes:
    /// - Server key is safe for client use (it's the default Nakama key)
    /// - HTTPS ensures encrypted communication
    /// - Authentication via device ID or Cognito
    /// </summary>
    public static class IVXNakamaConfig
    {
        /// <summary>
        /// Nakama server protocol (http or https)
        /// </summary>
        public const string PROTOCOL = "https";

        /// <summary>
        /// Nakama server host
        /// </summary>
        public const string HOST = "nakama-rest.intelli-verse-x.ai";

        /// <summary>
        /// Nakama server port (443 for HTTPS, 7350 for HTTP)
        /// </summary>
        public const int PORT = 443;

        /// <summary>
        /// Nakama server key (default key for authentication)
        /// </summary>
        public const string SERVER_KEY = "defaultkey";

        /// <summary>
        /// Full server URL (for REST API calls)
        /// </summary>
        public static readonly string SERVER_URL = $"{PROTOCOL}://{HOST}:{PORT}";

        /// <summary>
        /// WebSocket URL (for real-time features)
        /// </summary>
        public static readonly string WEBSOCKET_URL = $"wss://{HOST}:{PORT}";
    }
}
