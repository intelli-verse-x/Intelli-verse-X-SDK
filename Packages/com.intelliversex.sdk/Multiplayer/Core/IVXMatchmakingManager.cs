using System;
using System.Collections;
using System.Threading.Tasks;
using IntelliVerseX.Backend;
using UnityEngine;

namespace IntelliVerseX.GameModes
{
    /// <summary>
    /// Quick-match / matchmaking manager. Searches for compatible opponents
    /// via Nakama matchmaker (when available) or mock matching for testing.
    /// </summary>
    public class IVXMatchmakingManager : MonoBehaviour
    {
        #region Singleton

        private static IVXMatchmakingManager _instance;

        /// <summary>Singleton accessor.</summary>
        public static IVXMatchmakingManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindFirstObjectByType<IVXMatchmakingManager>();
                    if (_instance == null)
                    {
                        var go = new GameObject("[IVXMatchmakingManager]");
                        _instance = go.AddComponent<IVXMatchmakingManager>();
                        DontDestroyOnLoad(go);
                    }
                }
                return _instance;
            }
        }

        #endregion

        #region Events

        /// <summary>Fired when matchmaking begins searching.</summary>
        public event Action OnSearchStarted;

        /// <summary>Fired with elapsed seconds while searching.</summary>
        public event Action<float> OnSearchProgress;

        /// <summary>Fired when a match is found with opponent info.</summary>
        public event Action<IVXMatchFoundResult> OnMatchFound;

        /// <summary>Fired when matchmaking is cancelled or times out.</summary>
        public event Action<string> OnSearchCancelled;

        /// <summary>Fired on matchmaking errors.</summary>
        public event Action<string> OnError;

        #endregion

        #region Properties

        /// <summary>Whether matchmaking is currently active.</summary>
        public bool IsSearching { get; private set; }

        /// <summary>Elapsed search time in seconds.</summary>
        public float SearchElapsed { get; private set; }

        /// <summary>Maximum search time before timeout (seconds).</summary>
        public float MaxSearchTime { get; set; } = 60f;

        /// <summary>The matchmaking ticket ID (Nakama) if active.</summary>
        public string TicketId { get; private set; }

        #endregion

        #region Private Fields

        [Header("Nakama")]
        [SerializeField]
        [Tooltip("Assign IVXNManager or a concrete IVXNakamaManager subclass (IIVXNakamaRealtimeProvider).")]
        private MonoBehaviour _nakamaBackend;

        private Coroutine _searchCoroutine;
        private IVXMatchConfig _searchConfig;

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

        #region Public Methods

        /// <summary>
        /// Start searching for a match.
        /// </summary>
        /// <param name="config">Match configuration (mode, player count, etc.).</param>
        public void StartSearch(IVXMatchConfig config = null)
        {
            if (IsSearching)
            {
                Debug.LogWarning($"[{nameof(IVXMatchmakingManager)}] Already searching.");
                return;
            }

            _searchConfig = config ?? IVXMatchConfig.OnlineVersus();
            IVXGameModeManager.Instance.SetConfig(_searchConfig);
            IVXGameModeManager.Instance.SetPhase(IVXMatchPhase.Matchmaking);

            _searchCoroutine = StartCoroutine(SearchRoutine());
        }

        /// <summary>Cancel the current matchmaking search.</summary>
        public void CancelSearch()
        {
            if (!IsSearching) return;

            if (_searchCoroutine != null)
            {
                StopCoroutine(_searchCoroutine);
                _searchCoroutine = null;
            }

#if INTELLIVERSEX_HAS_NAKAMA
            if (!string.IsNullOrEmpty(TicketId))
            {
                var backend = ResolveNakamaRealtime();
                if (backend?.Socket != null)
                {
                    _ = backend.Socket.RemoveMatchmakerAsync(TicketId);
                }
            }
#endif

            IsSearching = false;
            TicketId = null;

            IVXGameModeManager.Instance.SetPhase(IVXMatchPhase.Lobby);
            OnSearchCancelled?.Invoke("Cancelled by user");

            Debug.Log($"[{nameof(IVXMatchmakingManager)}] Search cancelled.");
        }

        /// <summary>
        /// Quick match shortcut — starts an online versus search with defaults.
        /// </summary>
        public void QuickMatch()
        {
            StartSearch(IVXMatchConfig.OnlineVersus());
        }

        /// <summary>
        /// Quick ranked match shortcut.
        /// </summary>
        public void RankedMatch()
        {
            StartSearch(IVXMatchConfig.Ranked());
        }

        #endregion

        #region Private Methods

        /// <summary>Resolves injected Nakama backend (no static singleton).</summary>
        private IIVXNakamaRealtimeProvider ResolveNakamaRealtime()
        {
            return _nakamaBackend as IIVXNakamaRealtimeProvider;
        }

        private IEnumerator SearchRoutine()
        {
            IsSearching = true;
            SearchElapsed = 0f;
            TicketId = Guid.NewGuid().ToString("N").Substring(0, 12);

            OnSearchStarted?.Invoke();
            Debug.Log($"[{nameof(IVXMatchmakingManager)}] Searching... Ticket: {TicketId}");

#if INTELLIVERSEX_HAS_NAKAMA
            bool nakamaStarted = false;
            var backend = ResolveNakamaRealtime();
            if (backend?.Socket != null)
            {
                var addTask = StartNakamaMatchmaking(backend);
                while (!addTask.IsCompleted)
                {
                    yield return null;
                    SearchElapsed += Time.deltaTime;
                    OnSearchProgress?.Invoke(SearchElapsed);
                }
                nakamaStarted = !addTask.IsFaulted;
            }

            if (nakamaStarted)
            {
                while (IsSearching && SearchElapsed < MaxSearchTime)
                {
                    yield return null;
                    SearchElapsed += Time.deltaTime;
                    OnSearchProgress?.Invoke(SearchElapsed);
                }

                if (IsSearching)
                {
                    IsSearching = false;
                    _searchCoroutine = null;
                    TicketId = null;
                    IVXGameModeManager.Instance.SetPhase(IVXMatchPhase.Lobby);
                    OnSearchCancelled?.Invoke("Search timed out");
                }
                yield break;
            }
#endif

            float mockMatchTime = UnityEngine.Random.Range(2f, 8f);

            while (SearchElapsed < MaxSearchTime)
            {
                yield return null;
                SearchElapsed += Time.deltaTime;
                OnSearchProgress?.Invoke(SearchElapsed);

                if (SearchElapsed >= mockMatchTime)
                {
                    var result = GenerateMockMatch();
                    IsSearching = false;
                    _searchCoroutine = null;

                    IVXGameModeManager.Instance.AddRemotePlayer(result.OpponentUserId, result.OpponentDisplayName);
                    IVXGameModeManager.Instance.SetPhase(IVXMatchPhase.Loading);

                    OnMatchFound?.Invoke(result);
                    Debug.Log($"[{nameof(IVXMatchmakingManager)}] Match found: {result.OpponentDisplayName}");
                    yield break;
                }
            }

            IsSearching = false;
            _searchCoroutine = null;
            TicketId = null;

            IVXGameModeManager.Instance.SetPhase(IVXMatchPhase.Lobby);
            OnSearchCancelled?.Invoke("Search timed out");
            Debug.Log($"[{nameof(IVXMatchmakingManager)}] Search timed out.");
        }

#if INTELLIVERSEX_HAS_NAKAMA
        private async Task StartNakamaMatchmaking(IIVXNakamaRealtimeProvider backend)
        {
            var minCount = _searchConfig?.MinPlayers ?? 2;
            var maxCount = _searchConfig?.MaxPlayers ?? 2;
            var query = "*";

            var ticket = await backend.Socket.AddMatchmakerAsync(query, minCount, maxCount);
            TicketId = ticket.Ticket;
            Debug.Log($"[{nameof(IVXMatchmakingManager)}] Nakama matchmaker ticket: {TicketId}");

            backend.Socket.ReceivedMatchmakerMatched += (matched) =>
            {
                if (!IsSearching) return;

                IsSearching = false;
                _searchCoroutine = null;

                var opponents = matched.Users;
                string opName = "Opponent";
                string opId = "";
                foreach (var u in opponents)
                {
                    if (u.Presence.UserId != backend.Session.UserId)
                    {
                        opName = u.Presence.Username;
                        opId = u.Presence.UserId;
                        break;
                    }
                }

                var result = new IVXMatchFoundResult
                {
                    MatchId = matched.MatchId ?? matched.Token,
                    OpponentUserId = opId,
                    OpponentDisplayName = opName,
                    Transport = IVXNetworkTransport.NakamaRealtime
                };

                IVXGameModeManager.Instance.AddRemotePlayer(result.OpponentUserId, result.OpponentDisplayName);
                IVXGameModeManager.Instance.SetPhase(IVXMatchPhase.Loading);

                OnMatchFound?.Invoke(result);
                Debug.Log($"[{nameof(IVXMatchmakingManager)}] Nakama match found: {result.OpponentDisplayName}");
            };
        }
#endif

        private IVXMatchFoundResult GenerateMockMatch()
        {
            var mockNames = new[] { "Challenger99", "SpeedDemon", "BrainMaster", "QuizKing", "NightOwl",
                                    "FlashGamer", "ProdigyX", "StarHunter", "VortexPlay", "TitanForce" };
            return new IVXMatchFoundResult
            {
                MatchId = Guid.NewGuid().ToString("N").Substring(0, 12),
                OpponentUserId = Guid.NewGuid().ToString(),
                OpponentDisplayName = mockNames[UnityEngine.Random.Range(0, mockNames.Length)],
                OpponentRating = UnityEngine.Random.Range(800, 2200),
                EstimatedPingMs = UnityEngine.Random.Range(20, 100),
                Transport = _searchConfig?.Transport ?? IVXNetworkTransport.NakamaRealtime
            };
        }

        #endregion
    }

    /// <summary>
    /// Data returned when a match is found through matchmaking.
    /// </summary>
    [Serializable]
    public class IVXMatchFoundResult
    {
        /// <summary>Server-assigned match ID.</summary>
        public string MatchId;

        /// <summary>Opponent's user ID.</summary>
        public string OpponentUserId;

        /// <summary>Opponent's display name.</summary>
        public string OpponentDisplayName;

        /// <summary>Opponent's skill rating (-1 if unranked).</summary>
        public int OpponentRating = -1;

        /// <summary>Estimated ping to match server in ms.</summary>
        public int EstimatedPingMs = -1;

        /// <summary>Transport used for this match.</summary>
        public IVXNetworkTransport Transport;
    }
}
