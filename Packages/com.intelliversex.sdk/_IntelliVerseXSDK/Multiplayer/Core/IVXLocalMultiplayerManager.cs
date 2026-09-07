using System;
using System.Collections.Generic;
using UnityEngine;

namespace IntelliVerseX.GameModes
{
    /// <summary>
    /// Manages same-device (local) multiplayer: player registration, turn management,
    /// input device assignment, and split-screen viewport helpers.
    /// </summary>
    public class IVXLocalMultiplayerManager : MonoBehaviour
    {
        #region Singleton

        private static IVXLocalMultiplayerManager _instance;

        /// <summary>Singleton accessor.</summary>
        public static IVXLocalMultiplayerManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindFirstObjectByType<IVXLocalMultiplayerManager>();
                    if (_instance == null)
                    {
                        var go = new GameObject("[IVXLocalMultiplayerManager]");
                        _instance = go.AddComponent<IVXLocalMultiplayerManager>();
                        DontDestroyOnLoad(go);
                    }
                }
                return _instance;
            }
        }

        #endregion

        #region Events

        /// <summary>Fired when it becomes a player's turn (hot-seat mode).</summary>
        public event Action<IVXPlayerSlot> OnTurnStarted;

        /// <summary>Fired when a player ends their turn.</summary>
        public event Action<IVXPlayerSlot> OnTurnEnded;

        /// <summary>Fired when all players have completed a round.</summary>
        public event Action<int> OnRoundCompleted;

        /// <summary>Fired when the local session starts.</summary>
        public event Action<List<IVXPlayerSlot>> OnLocalSessionStarted;

        /// <summary>Fired when the local session ends.</summary>
        public event Action OnLocalSessionEnded;

        #endregion

        #region Properties

        /// <summary>Whether a local multiplayer session is active.</summary>
        public bool IsSessionActive { get; private set; }

        /// <summary>Index of the player whose turn it is (hot-seat mode).</summary>
        public int CurrentTurnIndex { get; private set; }

        /// <summary>Current round number (1-based).</summary>
        public int CurrentRound { get; private set; } = 1;

        /// <summary>The player whose turn it currently is.</summary>
        public IVXPlayerSlot CurrentTurnPlayer
        {
            get
            {
                var players = IVXGameModeManager.Instance.GetLocalPlayers();
                if (CurrentTurnIndex >= 0 && CurrentTurnIndex < players.Count)
                    return players[CurrentTurnIndex];
                return null;
            }
        }

        /// <summary>Whether the session is in hot-seat (turn-based) mode.</summary>
        public bool IsHotSeat { get; private set; }

        /// <summary>Turn timer remaining in seconds (-1 if no timer).</summary>
        public float TurnTimeRemaining { get; private set; } = -1f;

        #endregion

        #region Private Fields

        private float _turnTimeLimit;
        private bool _turnTimerActive;

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

        private void Update()
        {
            if (!_turnTimerActive || TurnTimeRemaining <= 0f) return;

            TurnTimeRemaining -= Time.deltaTime;
            if (TurnTimeRemaining <= 0f)
            {
                TurnTimeRemaining = 0f;
                EndTurn();
            }
        }

        private void OnDestroy()
        {
            if (_instance == this) _instance = null;
        }

        #endregion

        #region Public Methods — Session

        /// <summary>
        /// Start a local multiplayer session. Players must already be registered
        /// via IVXGameModeManager.AddLocalPlayer().
        /// </summary>
        /// <param name="hotSeat">True for hot-seat (turn-based), false for simultaneous play.</param>
        /// <param name="turnTimeLimitSeconds">Turn time limit in seconds (0 = unlimited).</param>
        public void StartSession(bool hotSeat = true, float turnTimeLimitSeconds = 0f)
        {
            var gm = IVXGameModeManager.Instance;
            var locals = gm.GetLocalPlayers();

            if (locals.Count < 2)
            {
                Debug.LogWarning($"[{nameof(IVXLocalMultiplayerManager)}] Need at least 2 local players.");
                return;
            }

            IsSessionActive = true;
            IsHotSeat = hotSeat;
            CurrentTurnIndex = 0;
            CurrentRound = 1;
            _turnTimeLimit = turnTimeLimitSeconds;

            gm.SetPhase(IVXMatchPhase.InProgress);
            OnLocalSessionStarted?.Invoke(locals);

            if (hotSeat)
                BeginTurn();

            Debug.Log($"[{nameof(IVXLocalMultiplayerManager)}] Session started: {locals.Count} players, " +
                      $"HotSeat={hotSeat}, TurnLimit={turnTimeLimitSeconds}s");
        }

        /// <summary>End the current local multiplayer session.</summary>
        public void EndSession()
        {
            if (!IsSessionActive) return;

            IsSessionActive = false;
            _turnTimerActive = false;
            TurnTimeRemaining = -1f;

            IVXGameModeManager.Instance.SetPhase(IVXMatchPhase.Finished);
            OnLocalSessionEnded?.Invoke();

            Debug.Log($"[{nameof(IVXLocalMultiplayerManager)}] Session ended.");
        }

        #endregion

        #region Public Methods — Turn Management

        /// <summary>End the current player's turn and advance to the next player.</summary>
        public void EndTurn()
        {
            if (!IsSessionActive || !IsHotSeat) return;

            var current = CurrentTurnPlayer;
            _turnTimerActive = false;
            OnTurnEnded?.Invoke(current);

            var locals = IVXGameModeManager.Instance.GetLocalPlayers();
            CurrentTurnIndex++;

            if (CurrentTurnIndex >= locals.Count)
            {
                CurrentTurnIndex = 0;
                CurrentRound++;
                OnRoundCompleted?.Invoke(CurrentRound - 1);
            }

            BeginTurn();
        }

        /// <summary>Skip directly to a specific player's turn.</summary>
        /// <param name="slotIndex">Local slot index to jump to.</param>
        public void JumpToPlayer(int slotIndex)
        {
            if (!IsSessionActive || !IsHotSeat) return;

            var locals = IVXGameModeManager.Instance.GetLocalPlayers();
            if (slotIndex < 0 || slotIndex >= locals.Count) return;

            _turnTimerActive = false;
            CurrentTurnIndex = slotIndex;
            BeginTurn();
        }

        #endregion

        #region Public Methods — Viewport Helpers

        /// <summary>
        /// Calculate split-screen viewport rects for the given number of players.
        /// Supports 2-player (horizontal split) and 3-4 player (quad grid).
        /// </summary>
        /// <param name="playerCount">Number of local players.</param>
        /// <returns>Array of normalized viewport rects, one per player.</returns>
        public static Rect[] CalculateSplitScreenRects(int playerCount)
        {
            switch (playerCount)
            {
                case 1:
                    return new[] { new Rect(0, 0, 1, 1) };
                case 2:
                    return new[]
                    {
                        new Rect(0, 0.5f, 1, 0.5f),
                        new Rect(0, 0, 1, 0.5f)
                    };
                case 3:
                    return new[]
                    {
                        new Rect(0, 0.5f, 1, 0.5f),
                        new Rect(0, 0, 0.5f, 0.5f),
                        new Rect(0.5f, 0, 0.5f, 0.5f)
                    };
                default:
                    return new[]
                    {
                        new Rect(0, 0.5f, 0.5f, 0.5f),
                        new Rect(0.5f, 0.5f, 0.5f, 0.5f),
                        new Rect(0, 0, 0.5f, 0.5f),
                        new Rect(0.5f, 0, 0.5f, 0.5f)
                    };
            }
        }

        /// <summary>
        /// Apply split-screen viewports to an array of cameras (one per player).
        /// </summary>
        /// <param name="cameras">Player cameras in slot order.</param>
        public static void ApplySplitScreen(Camera[] cameras)
        {
            if (cameras == null || cameras.Length == 0) return;
            var rects = CalculateSplitScreenRects(cameras.Length);
            for (int i = 0; i < cameras.Length && i < rects.Length; i++)
            {
                if (cameras[i] != null)
                    cameras[i].rect = rects[i];
            }
        }

        #endregion

        #region Private Methods

        private void BeginTurn()
        {
            var player = CurrentTurnPlayer;
            if (player == null) return;

            if (_turnTimeLimit > 0f)
            {
                TurnTimeRemaining = _turnTimeLimit;
                _turnTimerActive = true;
            }

            OnTurnStarted?.Invoke(player);
            Debug.Log($"[{nameof(IVXLocalMultiplayerManager)}] Turn: {player.DisplayName} (Round {CurrentRound})");
        }

        #endregion
    }
}
