using System;
using System.Collections.Generic;

namespace IntelliVerseX.GameModes
{
    /// <summary>
    /// Supported game modes in IntelliVerseX.
    /// </summary>
    public enum IVXGameMode
    {
        Solo = 0,
        LocalMultiplayer = 1,
        OnlineMultiplayer = 2,
        OnlineCoop = 3,
        OnlineVersus = 4,
        TurnBased = 5,
        RankedMatch = 6
    }

    /// <summary>
    /// The current phase of an online match lifecycle.
    /// </summary>
    public enum IVXMatchPhase
    {
        None = 0,
        Lobby,
        Matchmaking,
        Loading,
        InProgress,
        Paused,
        Finished,
        Disconnected
    }

    /// <summary>
    /// Connection transport used for online play.
    /// </summary>
    public enum IVXNetworkTransport
    {
        None = 0,
        NakamaRealtime,
        NakamaAuthoritative,
        PhotonPun
    }

    /// <summary>
    /// Readiness state of a player in a lobby.
    /// </summary>
    public enum IVXPlayerReadyState
    {
        NotReady = 0,
        Ready,
        Loading,
        InGame
    }

    /// <summary>
    /// Team assignment for team-based modes.
    /// </summary>
    public enum IVXTeam
    {
        None = 0,
        TeamA,
        TeamB,
        TeamC,
        TeamD,
        Spectator
    }

    /// <summary>
    /// Describes a single player slot in a local or online match.
    /// </summary>
    [Serializable]
    public class IVXPlayerSlot
    {
        /// <summary>Unique slot index (0-based).</summary>
        public int SlotIndex;

        /// <summary>Display name for this player.</summary>
        public string DisplayName;

        /// <summary>User ID for online players (Nakama user ID or Photon actor).</summary>
        public string UserId;

        /// <summary>True if this slot is occupied by a human (local or remote).</summary>
        public bool IsHuman = true;

        /// <summary>True if this player is on the local device.</summary>
        public bool IsLocal = true;

        /// <summary>True if this is the host / room owner.</summary>
        public bool IsHost;

        /// <summary>Team assignment for team-based modes.</summary>
        public IVXTeam Team = IVXTeam.None;

        /// <summary>Ready state in lobby.</summary>
        public IVXPlayerReadyState ReadyState = IVXPlayerReadyState.NotReady;

        /// <summary>Avatar URL or sprite key.</summary>
        public string AvatarKey;

        /// <summary>Custom metadata the game can attach.</summary>
        public Dictionary<string, string> CustomData;

        /// <summary>Input device index for local multiplayer (-1 = keyboard/touch).</summary>
        public int InputDeviceIndex = -1;

        public IVXPlayerSlot() { CustomData = new Dictionary<string, string>(); }

        public IVXPlayerSlot(int slotIndex, string displayName, bool isLocal = true)
        {
            SlotIndex = slotIndex;
            DisplayName = displayName;
            IsLocal = isLocal;
            IsHuman = true;
            CustomData = new Dictionary<string, string>();
        }
    }
}
