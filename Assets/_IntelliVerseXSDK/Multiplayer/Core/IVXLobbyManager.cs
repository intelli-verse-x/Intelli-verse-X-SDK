using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IntelliVerseX.Backend;
using UnityEngine;

namespace IntelliVerseX.GameModes
{
    /// <summary>
    /// Manages online room/lobby lifecycle: create, join, list, leave.
    /// Backed by Nakama match listing when available; falls back to local mock for testing.
    /// </summary>
    public class IVXLobbyManager : MonoBehaviour
    {
        #region Singleton

        private static IVXLobbyManager _instance;

        /// <summary>Singleton accessor.</summary>
        public static IVXLobbyManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindFirstObjectByType<IVXLobbyManager>();
                    if (_instance == null)
                    {
                        var go = new GameObject("[IVXLobbyManager]");
                        _instance = go.AddComponent<IVXLobbyManager>();
                        DontDestroyOnLoad(go);
                    }
                }
                return _instance;
            }
        }

        #endregion

        #region Events

        /// <summary>Fired when room list is refreshed.</summary>
        public event Action<List<IVXRoomInfo>> OnRoomListUpdated;

        /// <summary>Fired when successfully joined a room.</summary>
        public event Action<IVXJoinRoomResponse> OnRoomJoined;

        /// <summary>Fired when a room is created.</summary>
        public event Action<IVXCreateRoomResponse> OnRoomCreated;

        /// <summary>Fired when leaving a room.</summary>
        public event Action OnRoomLeft;

        /// <summary>Fired on lobby events (player join/leave/ready, etc.).</summary>
        public event Action<IVXLobbyEvent> OnLobbyEvent;

        /// <summary>Fired on errors.</summary>
        public event Action<string> OnError;

        #endregion

        #region Properties

        /// <summary>Whether the player is currently in a room.</summary>
        public bool IsInRoom { get; private set; }

        /// <summary>Current room info (null if not in a room).</summary>
        public IVXRoomInfo CurrentRoom { get; private set; }

        /// <summary>Cached room list from last refresh.</summary>
        public IReadOnlyList<IVXRoomInfo> CachedRooms => _rooms;

        /// <summary>Whether a room list refresh is in progress.</summary>
        public bool IsRefreshing { get; private set; }

        #endregion

        #region Private Fields

        [Header("Nakama")]
        [SerializeField]
        [Tooltip("Assign IVXNManager or a concrete IVXNakamaManager subclass (IIVXNakamaRealtimeProvider).")]
        private MonoBehaviour _nakamaBackend;

        private readonly List<IVXRoomInfo> _rooms = new List<IVXRoomInfo>();
        private Coroutine _autoRefresh;

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
            if (_autoRefresh != null) StopCoroutine(_autoRefresh);
            if (_instance == this) _instance = null;
        }

        #endregion

        #region Public Methods

        /// <summary>Refresh the room list with optional filters.</summary>
        /// <param name="filter">Filter criteria (null = default).</param>
        public void RefreshRoomList(IVXRoomFilter filter = null)
        {
            if (IsRefreshing) return;
            StartCoroutine(RefreshRoomListRoutine(filter ?? new IVXRoomFilter()));
        }

        /// <summary>Start auto-refreshing the room list at an interval.</summary>
        /// <param name="intervalSeconds">Seconds between refreshes.</param>
        /// <param name="filter">Filter criteria.</param>
        public void StartAutoRefresh(float intervalSeconds = 5f, IVXRoomFilter filter = null)
        {
            StopAutoRefresh();
            _autoRefresh = StartCoroutine(AutoRefreshRoutine(intervalSeconds, filter ?? new IVXRoomFilter()));
        }

        /// <summary>Stop auto-refreshing the room list.</summary>
        public void StopAutoRefresh()
        {
            if (_autoRefresh != null)
            {
                StopCoroutine(_autoRefresh);
                _autoRefresh = null;
            }
        }

        /// <summary>Create a new room and join it.</summary>
        /// <param name="request">Room creation request.</param>
        public void CreateRoom(IVXCreateRoomRequest request)
        {
            if (request?.Config == null)
            {
                OnError?.Invoke("Invalid create room request.");
                return;
            }
            StartCoroutine(CreateRoomRoutine(request));
        }

        /// <summary>Join an existing room by ID.</summary>
        /// <param name="request">Join request with room ID and optional password.</param>
        public void JoinRoom(IVXJoinRoomRequest request)
        {
            if (string.IsNullOrEmpty(request?.RoomId))
            {
                OnError?.Invoke("Invalid room ID.");
                return;
            }
            StartCoroutine(JoinRoomRoutine(request));
        }

        /// <summary>Leave the current room.</summary>
        public void LeaveRoom()
        {
            if (!IsInRoom) return;

            Debug.Log($"[{nameof(IVXLobbyManager)}] Leaving room: {CurrentRoom?.RoomId}");

            IsInRoom = false;
            CurrentRoom = null;
            OnRoomLeft?.Invoke();

            IVXGameModeManager.Instance.ReturnToLobby();
        }

        /// <summary>Signal that the local player is ready.</summary>
        /// <param name="ready">Ready state.</param>
        public void SetReady(bool ready)
        {
            if (!IsInRoom) return;

            IVXGameModeManager.Instance.SetPlayerReady(0, ready);

            OnLobbyEvent?.Invoke(new IVXLobbyEvent
            {
                Type = ready ? IVXLobbyEventType.PlayerReady : IVXLobbyEventType.PlayerNotReady,
                Player = IVXGameModeManager.Instance.LocalPlayer
            });
        }

        /// <summary>Kick a player from the room (host only).</summary>
        /// <param name="slotIndex">Slot index of the player to kick.</param>
        public void KickPlayer(int slotIndex)
        {
            if (!IsInRoom || !IVXGameModeManager.Instance.IsHost) return;
            if (slotIndex <= 0) return;

            var player = IVXGameModeManager.Instance.Players[slotIndex];
            IVXGameModeManager.Instance.RemovePlayer(slotIndex);

            OnLobbyEvent?.Invoke(new IVXLobbyEvent
            {
                Type = IVXLobbyEventType.PlayerLeft,
                Player = player,
                Message = "Kicked by host"
            });
        }

        #endregion

        #region Private Methods — Network Integration

        /// <summary>Resolves injected Nakama backend (no static singleton).</summary>
        private IIVXNakamaRealtimeProvider ResolveNakamaRealtime()
        {
            return _nakamaBackend as IIVXNakamaRealtimeProvider;
        }

        private IEnumerator RefreshRoomListRoutine(IVXRoomFilter filter)
        {
            IsRefreshing = true;

#if INTELLIVERSEX_HAS_NAKAMA
            var task = RefreshRoomListNakamaAsync(filter);
            while (!task.IsCompleted) yield return null;
            if (task.IsFaulted)
            {
                Debug.LogError($"[{nameof(IVXLobbyManager)}] Nakama ListMatches error: {task.Exception?.InnerException?.Message}");
                IsRefreshing = false;
                yield break;
            }
#else
            yield return new WaitForSeconds(0.3f);
            _rooms.Clear();
            _rooms.AddRange(GenerateMockRooms(filter));
#endif

            IsRefreshing = false;
            OnRoomListUpdated?.Invoke(_rooms);

            Debug.Log($"[{nameof(IVXLobbyManager)}] Room list refreshed: {_rooms.Count} rooms");
        }

#if INTELLIVERSEX_HAS_NAKAMA
        private async Task RefreshRoomListNakamaAsync(IVXRoomFilter filter)
        {
            var backend = ResolveNakamaRealtime();
            if (backend == null || backend.Client == null || backend.Session == null)
            {
                Debug.LogWarning($"[{nameof(IVXLobbyManager)}] Nakama not connected; falling back to mock rooms.");
                _rooms.Clear();
                _rooms.AddRange(GenerateMockRooms(filter));
                return;
            }

            var matchList = await backend.Client.ListMatchesAsync(
                backend.Session,
                min: 0,
                max: filter.Limit,
                limit: filter.Limit,
                authoritative: true,
                label: null,
                query: "*");

            _rooms.Clear();
            foreach (var m in matchList.Matches)
            {
                _rooms.Add(new IVXRoomInfo
                {
                    RoomId = m.MatchId,
                    RoomName = string.IsNullOrEmpty(m.Label) ? m.MatchId[..8] : m.Label,
                    HostName = "Host",
                    PlayerCount = m.Size,
                    MaxPlayers = 4,
                    Mode = IVXGameMode.OnlineVersus,
                    IsPasswordProtected = false,
                    IsInProgress = m.Size >= 2,
                    CreatedAt = DateTime.UtcNow,
                    PingMs = -1
                });
            }
        }
#endif

        private IEnumerator AutoRefreshRoutine(float interval, IVXRoomFilter filter)
        {
            while (true)
            {
                RefreshRoomList(filter);
                yield return new WaitForSeconds(interval);
            }
        }

        private IEnumerator CreateRoomRoutine(IVXCreateRoomRequest request)
        {
#if INTELLIVERSEX_HAS_NAKAMA
            var task = CreateRoomNakamaAsync(request);
            while (!task.IsCompleted) yield return null;
            if (task.IsFaulted)
            {
                OnError?.Invoke(task.Exception?.InnerException?.Message ?? "Create room failed.");
                yield break;
            }
#else
            yield return new WaitForSeconds(0.2f);

            var roomId = Guid.NewGuid().ToString("N").Substring(0, 8);

            var response = new IVXCreateRoomResponse
            {
                RoomId = roomId,
                Success = true
            };

            CurrentRoom = new IVXRoomInfo
            {
                RoomId = roomId,
                RoomName = request.RoomName ?? $"Room-{roomId}",
                HostName = IVXGameModeManager.Instance.LocalPlayer?.DisplayName ?? "Host",
                PlayerCount = 1,
                MaxPlayers = request.Config.MaxPlayers,
                Mode = request.Config.Mode,
                IsPasswordProtected = !string.IsNullOrEmpty(request.Password),
                CreatedAt = DateTime.UtcNow
            };

            IsInRoom = true;
            IVXGameModeManager.Instance.SetConfig(request.Config);

            OnRoomCreated?.Invoke(response);

            Debug.Log($"[{nameof(IVXLobbyManager)}] Room created: {roomId}");
#endif
        }

#if INTELLIVERSEX_HAS_NAKAMA
        private async Task CreateRoomNakamaAsync(IVXCreateRoomRequest request)
        {
            var backend = ResolveNakamaRealtime();
            if (backend?.Socket == null)
            {
                OnError?.Invoke("Nakama socket not connected.");
                return;
            }

            var match = await backend.Socket.CreateMatchAsync(request.RoomName ?? Guid.NewGuid().ToString("N")[..8]);

            var roomId = match.Id;
            CurrentRoom = new IVXRoomInfo
            {
                RoomId = roomId,
                RoomName = request.RoomName ?? $"Room-{roomId[..8]}",
                HostName = IVXGameModeManager.Instance.LocalPlayer?.DisplayName ?? "Host",
                PlayerCount = 1,
                MaxPlayers = request.Config.MaxPlayers,
                Mode = request.Config.Mode,
                IsPasswordProtected = !string.IsNullOrEmpty(request.Password),
                CreatedAt = DateTime.UtcNow
            };

            IsInRoom = true;
            IVXGameModeManager.Instance.SetConfig(request.Config);

            OnRoomCreated?.Invoke(new IVXCreateRoomResponse { RoomId = roomId, Success = true });
            Debug.Log($"[{nameof(IVXLobbyManager)}] Nakama match created: {roomId}");
        }
#endif

        private IEnumerator JoinRoomRoutine(IVXJoinRoomRequest request)
        {
#if INTELLIVERSEX_HAS_NAKAMA
            var task = JoinRoomNakamaAsync(request);
            while (!task.IsCompleted) yield return null;
            if (task.IsFaulted)
            {
                OnError?.Invoke(task.Exception?.InnerException?.Message ?? "Join room failed.");
                yield break;
            }
#else
            yield return new WaitForSeconds(0.3f);

            var room = _rooms.Find(r => r.RoomId == request.RoomId);
            if (room == null)
            {
                OnError?.Invoke($"Room {request.RoomId} not found.");
                yield break;
            }

            if (room.IsPasswordProtected && string.IsNullOrEmpty(request.Password))
            {
                OnError?.Invoke("Password required.");
                yield break;
            }

            if (!room.HasSpace)
            {
                OnError?.Invoke("Room is full.");
                yield break;
            }

            CurrentRoom = room;
            CurrentRoom.PlayerCount++;
            IsInRoom = true;

            var config = new IVXMatchConfig
            {
                Mode = room.Mode,
                MaxPlayers = room.MaxPlayers
            };
            IVXGameModeManager.Instance.SetConfig(config);

            var response = new IVXJoinRoomResponse
            {
                Success = true,
                Players = new List<IVXPlayerSlot>(IVXGameModeManager.Instance.Players)
            };

            OnRoomJoined?.Invoke(response);
            OnLobbyEvent?.Invoke(new IVXLobbyEvent
            {
                Type = IVXLobbyEventType.PlayerJoined,
                Player = IVXGameModeManager.Instance.LocalPlayer,
                Room = room
            });

            Debug.Log($"[{nameof(IVXLobbyManager)}] Joined room: {request.RoomId}");
#endif
        }

#if INTELLIVERSEX_HAS_NAKAMA
        private async Task JoinRoomNakamaAsync(IVXJoinRoomRequest request)
        {
            var backend = ResolveNakamaRealtime();
            if (backend?.Socket == null)
            {
                OnError?.Invoke("Nakama socket not connected.");
                return;
            }

            var match = await backend.Socket.JoinMatchAsync(request.RoomId);

            CurrentRoom = new IVXRoomInfo
            {
                RoomId = match.Id,
                RoomName = match.Id[..8],
                HostName = "Host",
                PlayerCount = match.Presences?.Count() ?? 1,
                MaxPlayers = 4,
                Mode = IVXGameMode.OnlineVersus,
                CreatedAt = DateTime.UtcNow
            };

            IsInRoom = true;

            var config = new IVXMatchConfig
            {
                Mode = CurrentRoom.Mode,
                MaxPlayers = CurrentRoom.MaxPlayers
            };
            IVXGameModeManager.Instance.SetConfig(config);

            OnRoomJoined?.Invoke(new IVXJoinRoomResponse
            {
                Success = true,
                Players = new List<IVXPlayerSlot>(IVXGameModeManager.Instance.Players)
            });

            Debug.Log($"[{nameof(IVXLobbyManager)}] Joined Nakama match: {match.Id}");
        }
#endif

        private List<IVXRoomInfo> GenerateMockRooms(IVXRoomFilter filter)
        {
            var mock = new List<IVXRoomInfo>();
            var modes = new[] { IVXGameMode.OnlineVersus, IVXGameMode.OnlineCoop, IVXGameMode.TurnBased };
            var names = new[] { "Champions Arena", "Casual Room", "Pro League", "Training Ground", "Quick Battle",
                               "Team Clash", "Brain Bowl", "Night Owls", "Weekend Warriors", "Open Lobby" };
            var hosts = new[] { "AlphaGamer", "CoolDev42", "SpeedRunner", "NoobMaster", "ProPlayer99",
                               "StarFighter", "LuckyCharm", "MidnightOwl", "PixelHero", "TurboKing" };

            int count = Mathf.Min(filter.Limit, 10);
            for (int i = 0; i < count; i++)
            {
                var mode = modes[i % modes.Length];
                if (filter.Mode.HasValue && filter.Mode.Value != mode) continue;

                int maxP = mode == IVXGameMode.OnlineVersus ? 2 : 4;
                int curP = UnityEngine.Random.Range(1, maxP + 1);
                bool inProgress = curP >= maxP && UnityEngine.Random.value > 0.5f;

                if (filter.OnlyAvailable && curP >= maxP) continue;
                if (filter.OnlyWaiting && inProgress) continue;

                mock.Add(new IVXRoomInfo
                {
                    RoomId = Guid.NewGuid().ToString("N").Substring(0, 8),
                    RoomName = names[i % names.Length],
                    HostName = hosts[i % hosts.Length],
                    PlayerCount = curP,
                    MaxPlayers = maxP,
                    Mode = mode,
                    IsPasswordProtected = i == 3,
                    IsInProgress = inProgress,
                    CreatedAt = DateTime.UtcNow.AddMinutes(-UnityEngine.Random.Range(1, 30)),
                    PingMs = UnityEngine.Random.Range(15, 120)
                });
            }

            return mock;
        }

        #endregion
    }
}
