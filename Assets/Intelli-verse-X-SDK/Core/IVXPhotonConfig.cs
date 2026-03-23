namespace IntelliVerseX.Core
{
    /// <summary>
    /// Photon multiplayer configuration for IntelliVerse-X platform.
    /// 
    /// Photon PUN (Photon Unity Networking) is used for real-time multiplayer
    /// features across IntelliVerse-X games.
    /// 
    /// App Information:
    /// - App ID (Realtime): fa2f730e-1c81-4d01-b11f-708680dcaf37
    /// - Region: Best (auto-select closest)
    /// - Protocol: UDP with fallback to WebSocket
    /// - Environment: Production
    /// 
    /// Features:
    /// - Real-time matchmaking
    /// - Room-based multiplayer
    /// - Voice chat support (optional)
    /// - Player state synchronization
    /// - Custom properties and events
    /// 
    /// Game Isolation:
    /// - Each game uses the same Photon App ID
    /// - Game ID is added to room properties for isolation
    /// - Players can only join rooms from the same game
    /// - Use GetBaseRoomProperties() when creating rooms
    /// 
    /// Usage:
    /// - QuizVerse: Multiplayer quiz battles
    /// - Terminal Rush: Real-time racing
    /// - Future games: Any real-time multiplayer
    /// 
    /// Security Notes:
    /// - App ID is safe for client use (public identifier)
    /// - Photon Cloud handles authentication
    /// - Room-level access control via game ID filtering
    /// </summary>
    public static class IVXPhotonConfig
    {
    /// <summary>
    /// Shared Photon App ID for Realtime (PUN2)
    /// Used as fallback if game doesn't specify its own App ID
    /// </summary>
    public const string SHARED_APP_ID_REALTIME = "fa2f730e-1c81-4d01-b11f-708680dcaf37";
    
    /// <summary>
    /// Get the Photon App ID for the current game.
    /// Returns game-specific App ID if configured, otherwise returns shared App ID.
    /// </summary>
    public static string GetAppId()
    {
        var config = IntelliVerseXManager.Config;
        if (config != null && !string.IsNullOrEmpty(config.photonAppId))
        {
            return config.photonAppId;
        }
        return SHARED_APP_ID_REALTIME;
    }        /// <summary>
        /// Photon App ID for Fusion (if using Fusion instead of PUN)
        /// Leave empty if not using Fusion
        /// </summary>
        public const string APP_ID_FUSION = "";

        /// <summary>
        /// Photon App ID for Chat (if using Photon Chat)
        /// Leave empty if not using Chat
        /// </summary>
        public const string APP_ID_CHAT = "";

        /// <summary>
        /// Photon App ID for Voice (if using Photon Voice)
        /// Leave empty if not using Voice
        /// </summary>
        public const string APP_ID_VOICE = "";

        /// <summary>
        /// Photon region (empty = auto-select best region)
        /// Options: "us", "eu", "asia", "jp", "au", "usw", "sa", "cae", "kr", "in", "ru"
        /// </summary>
        public const string REGION = "";

        /// <summary>
        /// App version (for matching players on same version)
        /// Can be set per-game or left empty to match any version
        /// </summary>
        public const string APP_VERSION = "";

        /// <summary>
        /// Whether to use Photon Name Server (true) or connect directly to server (false)
        /// Recommended: true for automatic region selection
        /// </summary>
        public const bool USE_NAME_SERVER = true;

        /// <summary>
        /// Enable protocol fallback (UDP -> WebSocket if UDP fails)
        /// Recommended: true for better connectivity
        /// </summary>
        public const bool ENABLE_PROTOCOL_FALLBACK = true;

        /// <summary>
        /// Enable lobby statistics (player counts, room lists)
        /// Disable to reduce network traffic if not needed
        /// </summary>
        public const bool ENABLE_LOBBY_STATISTICS = false;

        /// <summary>
        /// Network logging level for debugging
        /// 0 = None, 1 = Error, 2 = Warning, 3 = Info, 4 = All
        /// </summary>
        public const int NETWORK_LOGGING_LEVEL = 1; // Error only in production

        /// <summary>
        /// Custom room property key for game ID.
        /// Use this to ensure players only join rooms from the same game.
        /// 
        /// IMPORTANT DISTINCTION:
        /// - Room Name/ID: Unique identifier for each Photon room (e.g., "QuizRoom_1234")
        /// - Game ID: UUID identifying which game created the room (e.g., QuizVerse vs Terminal Rush)
        /// 
        /// Example:
        /// Room Name: "QuizRoom_5678" (unique per room)
        /// Game ID: "126bf539-dae2-4bcf-964d-316c0fa1f92b" (same for all QuizVerse rooms)
        /// 
        /// This prevents QuizVerse players from joining Terminal Rush rooms even though
        /// they share the same Photon App ID.
        /// </summary>
        public const string ROOM_PROPERTY_GAME_ID = "gameId";

        /// <summary>
        /// Get the current game ID from IntelliVerseXIdentity.
        /// Use this when creating or filtering Photon rooms.
        /// </summary>
        /// <returns>The game ID string, or empty if not initialized</returns>
        public static string GetGameId()
        {
            return IntelliVerseXIdentity.GameId ?? string.Empty;
        }

        /// <summary>
        /// Create base room properties with game ID included.
        /// Ensures rooms are game-specific and prevents cross-game joining.
        /// 
        /// NOTE: This adds the GAME ID (which game), not the ROOM CODE/NAME (which room).
        /// - Room Code/Name: Set when calling PhotonNetwork.CreateRoom(roomName, ...)
        /// - Game ID: Added automatically to room properties via this method
        /// </summary>
        /// <returns>Hashtable with game ID property</returns>
        public static ExitGames.Client.Photon.Hashtable GetBaseRoomProperties()
        {
            return new ExitGames.Client.Photon.Hashtable
            {
                { ROOM_PROPERTY_GAME_ID, GetGameId() }
            };
        }

        /// <summary>
        /// Add game ID to existing room properties.
        /// Call this before creating a Photon room to ensure game isolation.
        /// </summary>
        /// <param name="properties">Existing custom properties</param>
        public static void AddGameIdToRoomProperties(ExitGames.Client.Photon.Hashtable properties)
        {
            if (properties == null) return;
            properties[ROOM_PROPERTY_GAME_ID] = GetGameId();
        }

        /// <summary>
        /// Check if a room belongs to the current game.
        /// Use this when filtering room lists.
        /// </summary>
        /// <param name="roomProperties">Room's custom properties</param>
        /// <returns>True if room belongs to current game</returns>
        public static bool IsRoomForCurrentGame(ExitGames.Client.Photon.Hashtable roomProperties)
        {
            if (roomProperties == null) return false;
            if (!roomProperties.ContainsKey(ROOM_PROPERTY_GAME_ID)) return false;
            return roomProperties[ROOM_PROPERTY_GAME_ID]?.ToString() == GetGameId();
        }
    }
}
