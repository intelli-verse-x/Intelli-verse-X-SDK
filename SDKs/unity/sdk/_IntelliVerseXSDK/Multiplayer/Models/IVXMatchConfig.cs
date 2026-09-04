using System;
using System.Collections.Generic;

namespace IntelliVerseX.GameModes
{
    /// <summary>
    /// Configuration for a match instance — drives lobby creation, matchmaking tickets, and local sessions.
    /// </summary>
    [Serializable]
    public class IVXMatchConfig
    {
        /// <summary>Game mode for this match.</summary>
        public IVXGameMode Mode = IVXGameMode.Solo;

        /// <summary>Minimum players required to start.</summary>
        public int MinPlayers = 1;

        /// <summary>Maximum players allowed.</summary>
        public int MaxPlayers = 4;

        /// <summary>Number of teams (0 = free-for-all).</summary>
        public int TeamCount;

        /// <summary>Maximum players per team (only used when TeamCount > 0).</summary>
        public int MaxPlayersPerTeam = 2;

        /// <summary>Allow AI bots to fill empty slots.</summary>
        public bool AllowBots;

        /// <summary>Allow spectators to join.</summary>
        public bool AllowSpectators;

        /// <summary>Match duration in seconds (0 = unlimited).</summary>
        public int MatchDurationSeconds;

        /// <summary>Network transport to use for online modes.</summary>
        public IVXNetworkTransport Transport = IVXNetworkTransport.NakamaRealtime;

        /// <summary>Nakama match label or Photon room name prefix.</summary>
        public string RoomLabel;

        /// <summary>Whether the room is publicly listed.</summary>
        public bool IsPublic = true;

        /// <summary>Password for private rooms (empty = no password).</summary>
        public string Password;

        /// <summary>Custom match properties the game can use for filtering.</summary>
        public Dictionary<string, string> CustomProperties;

        /// <summary>Turn time limit in seconds for turn-based modes (0 = unlimited).</summary>
        public int TurnTimeLimitSeconds;

        /// <summary>Whether to auto-start when MinPlayers is reached.</summary>
        public bool AutoStartWhenReady = true;

        /// <summary>Countdown seconds before match starts after all ready.</summary>
        public int CountdownSeconds = 3;

        public IVXMatchConfig()
        {
            CustomProperties = new Dictionary<string, string>();
        }

        /// <summary>Create a default solo config.</summary>
        public static IVXMatchConfig Solo()
        {
            return new IVXMatchConfig
            {
                Mode = IVXGameMode.Solo,
                MinPlayers = 1,
                MaxPlayers = 1
            };
        }

        /// <summary>Create a default local multiplayer config.</summary>
        /// <param name="maxPlayers">Maximum local players on this device.</param>
        public static IVXMatchConfig Local(int maxPlayers = 4)
        {
            return new IVXMatchConfig
            {
                Mode = IVXGameMode.LocalMultiplayer,
                MinPlayers = 2,
                MaxPlayers = maxPlayers,
                Transport = IVXNetworkTransport.None
            };
        }

        /// <summary>Create a default online versus config.</summary>
        /// <param name="maxPlayers">Maximum online players.</param>
        /// <param name="transport">Network transport to use.</param>
        public static IVXMatchConfig OnlineVersus(int maxPlayers = 2, IVXNetworkTransport transport = IVXNetworkTransport.NakamaRealtime)
        {
            return new IVXMatchConfig
            {
                Mode = IVXGameMode.OnlineVersus,
                MinPlayers = 2,
                MaxPlayers = maxPlayers,
                Transport = transport
            };
        }

        /// <summary>Create a default online coop config.</summary>
        /// <param name="maxPlayers">Maximum coop players.</param>
        public static IVXMatchConfig OnlineCoop(int maxPlayers = 4)
        {
            return new IVXMatchConfig
            {
                Mode = IVXGameMode.OnlineCoop,
                MinPlayers = 2,
                MaxPlayers = maxPlayers,
                Transport = IVXNetworkTransport.NakamaRealtime
            };
        }

        /// <summary>Create a default ranked match config.</summary>
        public static IVXMatchConfig Ranked()
        {
            return new IVXMatchConfig
            {
                Mode = IVXGameMode.RankedMatch,
                MinPlayers = 2,
                MaxPlayers = 2,
                Transport = IVXNetworkTransport.NakamaAuthoritative,
                IsPublic = false
            };
        }

        /// <summary>Whether this config requires network connectivity.</summary>
        public bool RequiresNetwork => Mode != IVXGameMode.Solo && Mode != IVXGameMode.LocalMultiplayer;

        /// <summary>Whether this config uses teams.</summary>
        public bool IsTeamBased => TeamCount > 0;
    }
}
