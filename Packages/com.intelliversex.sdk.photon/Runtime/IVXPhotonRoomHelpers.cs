using System.Collections.Generic;
using UnityEngine;

namespace IntelliVerseX.Photon
{
    /// <summary>
    /// Optional Photon glue. Requires Photon PUN2 in the consumer project.
    /// Room isolation by IntelliVerseX Game ID.
    /// </summary>
    public static class IVXPhotonRoomHelpers
    {
        public const string RoomPropertyGameId = "gameId";

#if INTELLIVERSEX_HAS_PHOTON
        public static ExitGames.Client.Photon.Hashtable CreateBaseRoomProperties(string gameId)
        {
            return new ExitGames.Client.Photon.Hashtable
            {
                { RoomPropertyGameId, gameId ?? string.Empty }
            };
        }

        public static void AddGameId(ExitGames.Client.Photon.Hashtable properties, string gameId)
        {
            if (properties == null) return;
            properties[RoomPropertyGameId] = gameId ?? string.Empty;
        }

        public static bool IsRoomForGame(ExitGames.Client.Photon.Hashtable roomProperties, string gameId)
        {
            if (roomProperties == null || !roomProperties.ContainsKey(RoomPropertyGameId))
                return false;
            return roomProperties[RoomPropertyGameId]?.ToString() == gameId;
        }
#else
        public static Dictionary<string, string> CreateBaseRoomProperties(string gameId)
        {
            return new Dictionary<string, string> { { RoomPropertyGameId, gameId ?? string.Empty } };
        }
#endif
    }
}
