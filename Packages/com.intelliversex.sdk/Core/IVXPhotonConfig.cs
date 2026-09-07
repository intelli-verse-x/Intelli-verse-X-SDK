using System;

namespace IntelliVerseX.Core
{
    /// <summary>
    /// Optional Photon settings stub. Photon Asset Store copies live in the sandbox only —
    /// not in the UPM install path. Prefer Nakama / MultiplayerKernel for core multiplayer.
    /// </summary>
    [Obsolete("Photon is optional and sandbox-only. Core path is Control Center + IVXBootstrap (no Photon). See docs/OPTIONAL_MODULES.md.")]
    public static class IVXPhotonConfig
    {
        /// <summary>No shared App ID ships in the core package. Set your own in the consumer project if you use Photon.</summary>
        public const string SHARED_APP_ID_REALTIME = "";

        public const string APP_ID_FUSION = "";
        public const string APP_ID_CHAT = "";
        public const string APP_ID_VOICE = "";
        public const string REGION = "";
        public const string APP_VERSION = "";
        public const bool USE_NAME_SERVER = true;
        public const bool ENABLE_PROTOCOL_FALLBACK = true;
        public const bool ENABLE_LOBBY_STATISTICS = false;
        public const int NETWORK_LOGGING_LEVEL = 1;
        public const string ROOM_PROPERTY_GAME_ID = "gameId";

        /// <summary>Always empty from core — configure Photon outside this package.</summary>
        public static string GetAppId()
        {
            return string.Empty;
        }

        public static string GetGameId()
        {
            return IntelliVerseXIdentity.GameId ?? string.Empty;
        }
    }
}
