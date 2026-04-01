using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Nakama;
using UnityEngine;

namespace IntelliVerseX.Social
{
    /// <summary>
    /// Owns clan state and coordinates clan operations for the SDK.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class IVXClanManager : MonoBehaviour
    {
        private const int DEFAULT_MAX_MEMBERS = 50;

        private static bool _isQuitting;

        private IClient _client;
        private ISession _session;
        private string _gameId;
        private bool _isInitialized;

        /// <summary>
        /// Gets the active clan manager instance.
        /// </summary>
        public static IVXClanManager Instance { get; private set; }

        /// <summary>
        /// Gets whether an active instance exists.
        /// </summary>
        public static bool HasInstance => !_isQuitting && Instance != null;

        /// <summary>
        /// Gets whether the manager has a valid Nakama context.
        /// </summary>
        public bool IsInitialized => _isInitialized && _client != null && _session != null && !_session.IsExpired;

        /// <summary>
        /// Gets the current clan, if any.
        /// </summary>
        public IVXClanData CurrentClan { get; private set; }

        /// <summary>
        /// Gets the last loaded clan members.
        /// </summary>
        public IReadOnlyList<IVXClanMemberData> Members => _members;

        /// <summary>
        /// Gets the most recent browse results.
        /// </summary>
        public IReadOnlyList<IVXClanData> LastBrowseResults => _lastBrowseResults;

        /// <summary>
        /// Gets whether the current user belongs to a clan.
        /// </summary>
        public bool IsInClan => CurrentClan != null;

        /// <summary>
        /// Fired whenever the current clan changes.
        /// </summary>
        public event Action<IVXClanData> OnClanChanged;

        /// <summary>
        /// Fired when the current clan is left or cleared.
        /// </summary>
        public event Action OnClanLeft;

        /// <summary>
        /// Fired when browse results are updated.
        /// </summary>
        public event Action<IReadOnlyList<IVXClanData>> OnBrowseResultsUpdated;

        /// <summary>
        /// Fired when member data is updated.
        /// </summary>
        public event Action<IReadOnlyList<IVXClanMemberData>> OnMembersUpdated;

        private readonly List<IVXClanMemberData> _members = new List<IVXClanMemberData>();
        private readonly List<IVXClanData> _lastBrowseResults = new List<IVXClanData>();

        private void Awake()
        {
            if (_isQuitting)
            {
                return;
            }

            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            if (transform.parent != null)
            {
                transform.SetParent(null, true);
            }

            DontDestroyOnLoad(gameObject);
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }

        private void OnApplicationQuit()
        {
            _isQuitting = true;
        }

        /// <summary>
        /// Initializes the manager from explicit Nakama values.
        /// </summary>
        public void Initialize(IClient client, ISession session, string gameId)
        {
            _client = client;
            _session = session;
            _gameId = gameId;
            _isInitialized = _client != null && _session != null && !_session.IsExpired && !string.IsNullOrWhiteSpace(_gameId);
        }

        /// <summary>
        /// Initializes the manager from the shared IVXNManager instance.
        /// </summary>
        public bool InitializeFromNakamaManager()
        {
            var manager = FindNakamaManager();
            if (manager == null)
            {
                return false;
            }

            var managerType = manager.GetType();
            var client = managerType.GetProperty("Client")?.GetValue(manager) as IClient;
            var session = managerType.GetProperty("Session")?.GetValue(manager) as ISession;
            var gameId = managerType.GetProperty("GameId")?.GetValue(manager) as string;

            if (client == null || session == null || string.IsNullOrWhiteSpace(gameId))
            {
                return false;
            }

            Initialize(client, session, gameId);
            return IsInitialized;
        }

        /// <summary>
        /// Ensures the manager is ready for clan operations.
        /// </summary>
        public async Task<bool> EnsureInitializedAsync()
        {
            if (IsInitialized)
            {
                return true;
            }

            bool ready = await IVXClanService.EnsureNakamaInitializedAsync();
            if (!ready)
            {
                return false;
            }

            return InitializeFromNakamaManager();
        }

        /// <summary>
        /// Loads the current clan and its members.
        /// </summary>
        public async Task<IVXClanOperationResult> LoadCurrentClanAsync(CancellationToken ct = default)
        {
            if (!await EnsureInitializedAsync())
            {
                return IVXClanOperationResult.Failure("Nakama is not initialized.");
            }

            var result = await IVXClanService.LoadCurrentClanAsync(_client, _session, _gameId, ct);
            if (!result.IsSuccess)
            {
                return result;
            }

            CurrentClan = result.Clan;
            _members.Clear();
            OnClanChanged?.Invoke(CurrentClan);

            if (CurrentClan == null)
            {
                OnClanLeft?.Invoke();
                return result;
            }

            await LoadMembersAsync(ct);
            return result;
        }

        /// <summary>
        /// Creates a clan, then refreshes current clan state.
        /// </summary>
        public async Task<IVXClanOperationResult> CreateClanAsync(
            string name,
            string description,
            bool isOpen = true,
            int maxMembers = DEFAULT_MAX_MEMBERS,
            CancellationToken ct = default)
        {
            if (IsInClan)
            {
                return IVXClanOperationResult.Failure("Leave your current clan before creating a new one.");
            }

            if (!await EnsureInitializedAsync())
            {
                return IVXClanOperationResult.Failure("Nakama is not initialized.");
            }

            var result = await IVXClanService.CreateClanAsync(
                _client,
                _session,
                _gameId,
                name,
                description,
                isOpen,
                maxMembers,
                ct);

            if (!result.IsSuccess)
            {
                return result;
            }

            return await LoadCurrentClanAsync(ct);
        }

        /// <summary>
        /// Searches for clans.
        /// </summary>
        public async Task<IVXClanBrowseResult> BrowseClansAsync(
            string query,
            int limit = 20,
            CancellationToken ct = default)
        {
            if (!await EnsureInitializedAsync())
            {
                return new IVXClanBrowseResult
                {
                    ErrorMessage = "Nakama is not initialized."
                };
            }

            var result = await IVXClanService.BrowseClansAsync(_client, _session, query, limit, ct);

            _lastBrowseResults.Clear();
            if (result.Clans != null)
            {
                _lastBrowseResults.AddRange(result.Clans);
            }

            OnBrowseResultsUpdated?.Invoke(_lastBrowseResults);
            return result;
        }

        /// <summary>
        /// Joins a clan and refreshes current clan state.
        /// </summary>
        public async Task<IVXClanOperationResult> JoinClanAsync(string clanId, CancellationToken ct = default)
        {
            if (IsInClan)
            {
                return IVXClanOperationResult.Failure("Leave your current clan before joining another one.");
            }

            if (!await EnsureInitializedAsync())
            {
                return IVXClanOperationResult.Failure("Nakama is not initialized.");
            }

            var result = await IVXClanService.JoinClanAsync(_client, _session, clanId, ct);
            if (!result.IsSuccess)
            {
                return result;
            }

            return await LoadCurrentClanAsync(ct);
        }

        /// <summary>
        /// Leaves the current clan and clears cached state.
        /// </summary>
        public async Task<IVXClanOperationResult> LeaveClanAsync(CancellationToken ct = default)
        {
            if (CurrentClan == null)
            {
                return IVXClanOperationResult.Success();
            }

            if (!await EnsureInitializedAsync())
            {
                return IVXClanOperationResult.Failure("Nakama is not initialized.");
            }

            var result = await IVXClanService.LeaveClanAsync(_client, _session, CurrentClan.ClanId, ct);
            if (!result.IsSuccess)
            {
                return result;
            }

            CurrentClan = null;
            _members.Clear();
            OnClanChanged?.Invoke(null);
            OnClanLeft?.Invoke();
            OnMembersUpdated?.Invoke(_members);
            return result;
        }

        /// <summary>
        /// Loads current clan members.
        /// </summary>
        public async Task<IReadOnlyList<IVXClanMemberData>> LoadMembersAsync(CancellationToken ct = default)
        {
            if (CurrentClan == null)
            {
                _members.Clear();
                OnMembersUpdated?.Invoke(_members);
                return _members;
            }

            if (!await EnsureInitializedAsync())
            {
                _members.Clear();
                OnMembersUpdated?.Invoke(_members);
                return _members;
            }

            var members = await IVXClanService.LoadMembersAsync(_client, _session, CurrentClan.ClanId, ct);
            _members.Clear();
            if (members != null)
            {
                _members.AddRange(members);
            }

            OnMembersUpdated?.Invoke(_members);
            return _members;
        }

        private static object FindNakamaManager()
        {
            Type managerType = Type.GetType("IntelliVerseX.Backend.Nakama.IVXNManager, IntelliVerseX.V2");
            if (managerType == null)
            {
                return null;
            }

            PropertyInfo instanceProperty = managerType.GetProperty(
                "Instance",
                BindingFlags.Public | BindingFlags.Static);

            return instanceProperty?.GetValue(null);
        }
    }
}
