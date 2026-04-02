using System;
using System.Collections.Generic;
using UnityEngine;

namespace IntelliVerseX.GameModes
{
    /// <summary>
    /// Central singleton for game mode selection, player slot management, and match lifecycle.
    /// Attach to a persistent GameObject or let it auto-create via Instance.
    /// </summary>
    [HelpURL("https://intelli-verse-x.github.io/Intelli-verse-X-Unity-SDK/modules/multiplayer/")]
    public class IVXGameModeManager : MonoBehaviour
    {
        #region Singleton

        private static IVXGameModeManager _instance;

        /// <summary>Singleton accessor. Auto-creates if not present in scene.</summary>
        public static IVXGameModeManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindFirstObjectByType<IVXGameModeManager>();
                    if (_instance == null)
                    {
                        var go = new GameObject("[IVXGameModeManager]");
                        _instance = go.AddComponent<IVXGameModeManager>();
                        DontDestroyOnLoad(go);
                    }
                }
                return _instance;
            }
        }

        #endregion

        #region Events

        /// <summary>Fired when the game mode changes.</summary>
        public event Action<IVXGameMode> OnModeChanged;

        /// <summary>Fired when a player is added to a slot.</summary>
        public event Action<IVXPlayerSlot> OnPlayerAdded;

        /// <summary>Fired when a player is removed from a slot.</summary>
        public event Action<IVXPlayerSlot> OnPlayerRemoved;

        /// <summary>Fired when a player's ready state changes.</summary>
        public event Action<IVXPlayerSlot> OnPlayerReadyChanged;

        /// <summary>Fired when all players are ready and match can start.</summary>
        public event Action<IVXMatchConfig> OnAllPlayersReady;

        /// <summary>Fired when match phase transitions.</summary>
        public event Action<IVXMatchPhase, IVXMatchPhase> OnMatchPhaseChanged;

        /// <summary>Fired when the match config is updated.</summary>
        public event Action<IVXMatchConfig> OnConfigChanged;

        #endregion

        #region Properties

        /// <summary>Current match configuration.</summary>
        public IVXMatchConfig CurrentConfig { get; private set; }

        /// <summary>Currently selected game mode.</summary>
        public IVXGameMode CurrentMode => CurrentConfig?.Mode ?? IVXGameMode.Solo;

        /// <summary>Current match phase.</summary>
        public IVXMatchPhase Phase { get; private set; } = IVXMatchPhase.None;

        /// <summary>All registered player slots.</summary>
        public IReadOnlyList<IVXPlayerSlot> Players => _players;

        /// <summary>Number of occupied slots.</summary>
        public int PlayerCount => _players.Count;

        /// <summary>Whether all minimum players have joined and are ready.</summary>
        public bool CanStartMatch => CurrentConfig != null
            && _players.Count >= CurrentConfig.MinPlayers
            && _players.TrueForAll(p => p.ReadyState == IVXPlayerReadyState.Ready || !p.IsHuman);

        /// <summary>Whether the local player is the host.</summary>
        public bool IsHost { get; private set; } = true;

        /// <summary>Local player slot (always slot 0 for the primary local player).</summary>
        public IVXPlayerSlot LocalPlayer => _players.Count > 0 ? _players[0] : null;

        #endregion

        #region Private Fields

        private readonly List<IVXPlayerSlot> _players = new List<IVXPlayerSlot>();

        #endregion

        #region Unity Lifecycle

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }
            _instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private void OnDestroy()
        {
            if (_instance == this) _instance = null;
        }

        #endregion

        #region Public Methods — Mode Selection

        /// <summary>Set the game mode and configure default match settings.</summary>
        /// <param name="mode">Desired game mode.</param>
        /// <param name="maxPlayers">Override max players (0 = mode default).</param>
        public void SelectMode(IVXGameMode mode, int maxPlayers = 0)
        {
            IVXMatchConfig config;
            switch (mode)
            {
                case IVXGameMode.Solo:
                    config = IVXMatchConfig.Solo();
                    break;
                case IVXGameMode.LocalMultiplayer:
                    config = IVXMatchConfig.Local(maxPlayers > 0 ? maxPlayers : 4);
                    break;
                case IVXGameMode.OnlineVersus:
                case IVXGameMode.RankedMatch:
                    config = mode == IVXGameMode.RankedMatch
                        ? IVXMatchConfig.Ranked()
                        : IVXMatchConfig.OnlineVersus(maxPlayers > 0 ? maxPlayers : 2);
                    break;
                case IVXGameMode.OnlineCoop:
                    config = IVXMatchConfig.OnlineCoop(maxPlayers > 0 ? maxPlayers : 4);
                    break;
                default:
                    config = new IVXMatchConfig { Mode = mode, MaxPlayers = maxPlayers > 0 ? maxPlayers : 2 };
                    break;
            }

            SetConfig(config);
        }

        /// <summary>Apply a fully custom match configuration.</summary>
        /// <param name="config">The match configuration to use.</param>
        public void SetConfig(IVXMatchConfig config)
        {
            var oldMode = CurrentConfig?.Mode ?? IVXGameMode.Solo;
            CurrentConfig = config ?? throw new ArgumentNullException(nameof(config));

            _players.Clear();

            AddLocalPlayer("Player 1");
            IsHost = true;

            if (config.Mode != oldMode)
                OnModeChanged?.Invoke(config.Mode);

            OnConfigChanged?.Invoke(config);
            SetPhase(IVXMatchPhase.Lobby);

            Debug.Log($"[{nameof(IVXGameModeManager)}] Mode set: {config.Mode}, Max: {config.MaxPlayers}");
        }

        #endregion

        #region Public Methods — Player Management

        /// <summary>Add a local player to the next available slot.</summary>
        /// <param name="displayName">Display name for the player.</param>
        /// <param name="inputDeviceIndex">Input device index (-1 = default).</param>
        /// <returns>The created player slot, or null if full.</returns>
        public IVXPlayerSlot AddLocalPlayer(string displayName, int inputDeviceIndex = -1)
        {
            if (CurrentConfig == null) return null;
            if (_players.Count >= CurrentConfig.MaxPlayers) return null;

            var slot = new IVXPlayerSlot(_players.Count, displayName, true)
            {
                InputDeviceIndex = inputDeviceIndex,
                IsHost = _players.Count == 0
            };

            _players.Add(slot);
            OnPlayerAdded?.Invoke(slot);
            return slot;
        }

        /// <summary>Add a remote (online) player to the next available slot.</summary>
        /// <param name="userId">Remote player user ID.</param>
        /// <param name="displayName">Remote player display name.</param>
        /// <returns>The created player slot, or null if full.</returns>
        public IVXPlayerSlot AddRemotePlayer(string userId, string displayName)
        {
            if (CurrentConfig == null) return null;
            if (_players.Count >= CurrentConfig.MaxPlayers) return null;

            var slot = new IVXPlayerSlot(_players.Count, displayName, false)
            {
                UserId = userId,
                IsHost = false
            };

            _players.Add(slot);
            OnPlayerAdded?.Invoke(slot);
            return slot;
        }

        /// <summary>Add an AI bot to the next available slot.</summary>
        /// <param name="botName">Display name for the bot.</param>
        /// <returns>The created player slot, or null if full or bots not allowed.</returns>
        public IVXPlayerSlot AddBot(string botName)
        {
            if (CurrentConfig == null || !CurrentConfig.AllowBots) return null;
            if (_players.Count >= CurrentConfig.MaxPlayers) return null;

            var slot = new IVXPlayerSlot(_players.Count, botName, true)
            {
                IsHuman = false,
                ReadyState = IVXPlayerReadyState.Ready
            };

            _players.Add(slot);
            OnPlayerAdded?.Invoke(slot);
            return slot;
        }

        /// <summary>Remove a player from a slot.</summary>
        /// <param name="slotIndex">Index of the slot to remove.</param>
        public void RemovePlayer(int slotIndex)
        {
            if (slotIndex < 0 || slotIndex >= _players.Count) return;
            if (slotIndex == 0) return; // Cannot remove primary local player

            var removed = _players[slotIndex];
            _players.RemoveAt(slotIndex);

            for (int i = 0; i < _players.Count; i++)
                _players[i].SlotIndex = i;

            OnPlayerRemoved?.Invoke(removed);
        }

        /// <summary>Set a player's ready state.</summary>
        /// <param name="slotIndex">Player slot index.</param>
        /// <param name="ready">Whether the player is ready.</param>
        public void SetPlayerReady(int slotIndex, bool ready)
        {
            if (slotIndex < 0 || slotIndex >= _players.Count) return;

            _players[slotIndex].ReadyState = ready ? IVXPlayerReadyState.Ready : IVXPlayerReadyState.NotReady;
            OnPlayerReadyChanged?.Invoke(_players[slotIndex]);

            if (CanStartMatch)
                OnAllPlayersReady?.Invoke(CurrentConfig);
        }

        /// <summary>Assign a player to a team.</summary>
        /// <param name="slotIndex">Player slot index.</param>
        /// <param name="team">Team to assign.</param>
        public void SetPlayerTeam(int slotIndex, IVXTeam team)
        {
            if (slotIndex < 0 || slotIndex >= _players.Count) return;
            _players[slotIndex].Team = team;
        }

        #endregion

        #region Public Methods — Match Lifecycle

        /// <summary>Transition the match to a new phase.</summary>
        /// <param name="phase">Target phase.</param>
        public void SetPhase(IVXMatchPhase phase)
        {
            if (Phase == phase) return;
            var old = Phase;
            Phase = phase;
            OnMatchPhaseChanged?.Invoke(old, phase);
            Debug.Log($"[{nameof(IVXGameModeManager)}] Phase: {old} -> {phase}");
        }

        /// <summary>Start the match (transitions to Loading then InProgress).</summary>
        public void StartMatch()
        {
            if (!CanStartMatch)
            {
                Debug.LogWarning($"[{nameof(IVXGameModeManager)}] Cannot start: not enough ready players.");
                return;
            }

            foreach (var p in _players)
                p.ReadyState = IVXPlayerReadyState.Loading;

            SetPhase(IVXMatchPhase.Loading);
        }

        /// <summary>Signal that loading is complete and gameplay begins.</summary>
        public void BeginGameplay()
        {
            foreach (var p in _players)
                p.ReadyState = IVXPlayerReadyState.InGame;

            SetPhase(IVXMatchPhase.InProgress);
        }

        /// <summary>End the current match.</summary>
        public void EndMatch()
        {
            SetPhase(IVXMatchPhase.Finished);
        }

        /// <summary>Reset to lobby state, keeping current config.</summary>
        public void ReturnToLobby()
        {
            foreach (var p in _players)
                p.ReadyState = IVXPlayerReadyState.NotReady;

            SetPhase(IVXMatchPhase.Lobby);
        }

        /// <summary>Full reset — clears config, players, and phase.</summary>
        public void Reset()
        {
            _players.Clear();
            CurrentConfig = null;
            Phase = IVXMatchPhase.None;
            IsHost = true;
        }

        #endregion

        #region Public Methods — Queries

        /// <summary>Get all players on a specific team.</summary>
        /// <param name="team">Team to query.</param>
        /// <returns>List of players on the team.</returns>
        public List<IVXPlayerSlot> GetTeamPlayers(IVXTeam team)
        {
            return _players.FindAll(p => p.Team == team);
        }

        /// <summary>Get all local players (same-device).</summary>
        public List<IVXPlayerSlot> GetLocalPlayers()
        {
            return _players.FindAll(p => p.IsLocal);
        }

        /// <summary>Get all remote (online) players.</summary>
        public List<IVXPlayerSlot> GetRemotePlayers()
        {
            return _players.FindAll(p => !p.IsLocal);
        }

        /// <summary>Check if a specific game mode is available given current SDK configuration.</summary>
        /// <param name="mode">Mode to check.</param>
        /// <returns>True if the mode can be selected.</returns>
        public bool IsModeAvailable(IVXGameMode mode)
        {
            switch (mode)
            {
                case IVXGameMode.Solo:
                case IVXGameMode.LocalMultiplayer:
                case IVXGameMode.TurnBased:
                    return true;
                case IVXGameMode.OnlineMultiplayer:
                case IVXGameMode.OnlineVersus:
                case IVXGameMode.OnlineCoop:
                case IVXGameMode.RankedMatch:
                    return Application.internetReachability != NetworkReachability.NotReachable;
                default:
                    return false;
            }
        }

        #endregion
    }
}
