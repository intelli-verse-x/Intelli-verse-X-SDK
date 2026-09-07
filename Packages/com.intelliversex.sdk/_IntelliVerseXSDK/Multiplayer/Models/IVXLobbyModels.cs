using System;
using System.Collections.Generic;

namespace IntelliVerseX.GameModes
{
    /// <summary>
    /// Represents a joinable room in an online lobby listing.
    /// </summary>
    [Serializable]
    public class IVXRoomInfo
    {
        /// <summary>Server-assigned room/match ID.</summary>
        public string RoomId;

        /// <summary>Human-readable room name or label.</summary>
        public string RoomName;

        /// <summary>Host player display name.</summary>
        public string HostName;

        /// <summary>Current player count.</summary>
        public int PlayerCount;

        /// <summary>Maximum player capacity.</summary>
        public int MaxPlayers;

        /// <summary>Game mode for this room.</summary>
        public IVXGameMode Mode;

        /// <summary>Whether the room requires a password.</summary>
        public bool IsPasswordProtected;

        /// <summary>Whether the match is already in progress.</summary>
        public bool IsInProgress;

        /// <summary>Room creation timestamp (UTC).</summary>
        public DateTime CreatedAt;

        /// <summary>Average ping to this room in ms (-1 if unknown).</summary>
        public int PingMs = -1;

        /// <summary>Custom properties set by the host.</summary>
        public Dictionary<string, string> CustomProperties;

        /// <summary>Whether there is space for another player.</summary>
        public bool HasSpace => PlayerCount < MaxPlayers;

        public IVXRoomInfo()
        {
            CustomProperties = new Dictionary<string, string>();
        }
    }

    /// <summary>
    /// Request to create a new room.
    /// </summary>
    [Serializable]
    public class IVXCreateRoomRequest
    {
        /// <summary>Room display name.</summary>
        public string RoomName;

        /// <summary>Match config for the room.</summary>
        public IVXMatchConfig Config;

        /// <summary>Optional password.</summary>
        public string Password;
    }

    /// <summary>
    /// Response after successfully creating a room.
    /// </summary>
    [Serializable]
    public class IVXCreateRoomResponse
    {
        /// <summary>Server-assigned room ID.</summary>
        public string RoomId;

        /// <summary>Whether creation was successful.</summary>
        public bool Success;

        /// <summary>Error message if creation failed.</summary>
        public string Error;
    }

    /// <summary>
    /// Request to join an existing room.
    /// </summary>
    [Serializable]
    public class IVXJoinRoomRequest
    {
        /// <summary>Room ID to join.</summary>
        public string RoomId;

        /// <summary>Password if room is protected.</summary>
        public string Password;
    }

    /// <summary>
    /// Response after attempting to join a room.
    /// </summary>
    [Serializable]
    public class IVXJoinRoomResponse
    {
        /// <summary>Whether join was successful.</summary>
        public bool Success;

        /// <summary>Error message if join failed.</summary>
        public string Error;

        /// <summary>Current players in the room after joining.</summary>
        public List<IVXPlayerSlot> Players;
    }

    /// <summary>
    /// Filter criteria for room listing queries.
    /// </summary>
    [Serializable]
    public class IVXRoomFilter
    {
        /// <summary>Filter by game mode (null = any).</summary>
        public IVXGameMode? Mode;

        /// <summary>Only show rooms with available space.</summary>
        public bool OnlyAvailable = true;

        /// <summary>Only show rooms not in progress.</summary>
        public bool OnlyWaiting = true;

        /// <summary>Maximum number of results.</summary>
        public int Limit = 20;

        /// <summary>Custom property filters (key=value must match).</summary>
        public Dictionary<string, string> PropertyFilters;

        public IVXRoomFilter()
        {
            PropertyFilters = new Dictionary<string, string>();
        }
    }

    /// <summary>
    /// Lobby event data passed with lobby callbacks.
    /// </summary>
    [Serializable]
    public class IVXLobbyEvent
    {
        /// <summary>Type of lobby event.</summary>
        public IVXLobbyEventType Type;

        /// <summary>Affected player (if applicable).</summary>
        public IVXPlayerSlot Player;

        /// <summary>Room info context.</summary>
        public IVXRoomInfo Room;

        /// <summary>Additional message or data.</summary>
        public string Message;
    }

    /// <summary>
    /// Types of events that occur in a lobby.
    /// </summary>
    public enum IVXLobbyEventType
    {
        PlayerJoined,
        PlayerLeft,
        PlayerReady,
        PlayerNotReady,
        HostChanged,
        CountdownStarted,
        CountdownCancelled,
        MatchStarting,
        RoomClosed,
        ChatMessage,
        SettingsChanged
    }
}
