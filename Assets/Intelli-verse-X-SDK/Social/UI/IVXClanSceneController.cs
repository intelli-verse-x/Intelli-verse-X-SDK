using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace IntelliVerseX.Social.UI
{
    /// <summary>
    /// Scene controller for the IVX_Clan demo scene.
    /// Coordinates auth verification, Nakama initialization, clan manager, and the panel UI.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class IVXClanSceneController : MonoBehaviour
    {
        private const string LOG_TAG = "[IVXClanScene]";
        private const int RETRY_DELAY_MS = 2000;
        private const int MAX_INIT_RETRIES = 3;

        [Header("Behavior")]
        [SerializeField] private bool _loadCurrentClanOnStart = true;
        [SerializeField] private bool _searchClansOnStart = true;

        [Header("Optional References")]
        [SerializeField] private IVXClanPanel _panel;

        private CancellationTokenSource _cts;

        /// <summary>
        /// Gets the shared clan manager used by this scene.
        /// </summary>
        public IVXClanManager ClanManager { get; private set; }

        /// <summary>
        /// Gets the latest UI status message.
        /// </summary>
        public string StatusMessage { get; private set; } = "Initializing clan demo...";

        /// <summary>
        /// Gets whether an async operation is in progress.
        /// </summary>
        public bool IsBusy { get; private set; }

        /// <summary>
        /// Gets the current browse results.
        /// </summary>
        public IReadOnlyList<IVXClanData> BrowseResults => ClanManager != null ? ClanManager.LastBrowseResults : Array.Empty<IVXClanData>();

        #region Unity Lifecycle

        private void Awake()
        {
            _cts = new CancellationTokenSource();
            EnsureManager();
            EnsurePanel();
        }

        private async void Start()
        {
            bool ready = await InitializeManagerAsync(_cts.Token);
            if (!ready)
            {
                return;
            }

            if (_loadCurrentClanOnStart)
            {
                await RefreshCurrentClanAsync(_cts.Token);
            }

            if (_searchClansOnStart)
            {
                await SearchClansAsync(string.Empty, _cts.Token);
            }
        }

        private void OnEnable()
        {
            EnsureManager();
            if (ClanManager == null) return;

            ClanManager.OnClanChanged += HandleClanChanged;
            ClanManager.OnBrowseResultsUpdated += HandleBrowseResultsUpdated;
            ClanManager.OnMembersUpdated += HandleMembersUpdated;
        }

        private void OnDisable()
        {
            if (ClanManager == null) return;

            ClanManager.OnClanChanged -= HandleClanChanged;
            ClanManager.OnBrowseResultsUpdated -= HandleBrowseResultsUpdated;
            ClanManager.OnMembersUpdated -= HandleMembersUpdated;
        }

        private void OnDestroy()
        {
            _cts?.Cancel();
            _cts?.Dispose();
            _cts = null;
        }

        #endregion

        #region Public API

        /// <summary>
        /// Refreshes the current clan and member list.
        /// </summary>
        public async Task RefreshCurrentClanAsync(CancellationToken ct = default)
        {
            if (ClanManager == null)
            {
                SetStatus("Clan manager is missing.");
                return;
            }

            await RunBusyOperationAsync("Loading current clan...", async () =>
            {
                var result = await ClanManager.LoadCurrentClanAsync(ct);
                SetStatus(result.IsSuccess
                    ? (result.Clan != null ? $"Loaded clan: {result.Clan.Name}" : "No clan joined.")
                    : $"Load failed: {result.ErrorMessage}");
            });
        }

        /// <summary>
        /// Searches for clans.
        /// </summary>
        public async Task SearchClansAsync(string query, CancellationToken ct = default)
        {
            if (ClanManager == null)
            {
                SetStatus("Clan manager is missing.");
                return;
            }

            await RunBusyOperationAsync("Searching clans...", async () =>
            {
                var result = await ClanManager.BrowseClansAsync(query, 20, ct);
                SetStatus(result.IsSuccess
                    ? $"Loaded {result.Clans.Count} clan result(s)."
                    : $"Search failed: {result.ErrorMessage}");
            });
        }

        /// <summary>
        /// Creates a clan and refreshes current state.
        /// </summary>
        public async Task CreateClanAsync(string name, string description, bool isOpen, CancellationToken ct = default)
        {
            if (ClanManager == null)
            {
                SetStatus("Clan manager is missing.");
                return;
            }

            await RunBusyOperationAsync("Creating clan...", async () =>
            {
                var result = await ClanManager.CreateClanAsync(name, description, isOpen, 50, ct);
                SetStatus(result.IsSuccess
                    ? $"Clan created: {ClanManager.CurrentClan?.Name ?? name}"
                    : $"Create failed: {result.ErrorMessage}");
            });
        }

        /// <summary>
        /// Joins a clan and refreshes current state.
        /// </summary>
        public async Task JoinClanAsync(string clanId, CancellationToken ct = default)
        {
            if (ClanManager == null)
            {
                SetStatus("Clan manager is missing.");
                return;
            }

            await RunBusyOperationAsync("Joining clan...", async () =>
            {
                var result = await ClanManager.JoinClanAsync(clanId, ct);
                SetStatus(result.IsSuccess
                    ? $"Joined clan: {ClanManager.CurrentClan?.Name ?? clanId}"
                    : $"Join failed: {result.ErrorMessage}");
            });
        }

        /// <summary>
        /// Leaves the current clan.
        /// </summary>
        public async Task LeaveClanAsync(CancellationToken ct = default)
        {
            if (ClanManager == null)
            {
                SetStatus("Clan manager is missing.");
                return;
            }

            await RunBusyOperationAsync("Leaving clan...", async () =>
            {
                var result = await ClanManager.LeaveClanAsync(ct);
                SetStatus(result.IsSuccess ? "Left clan." : $"Leave failed: {result.ErrorMessage}");
            });
        }

        #endregion

        #region Initialization

        private async Task<bool> InitializeManagerAsync(CancellationToken ct)
        {
            if (ClanManager == null)
            {
                SetStatus("Clan manager is missing.");
                return false;
            }

            if (!VerifyUserSession())
            {
                return false;
            }

            for (int attempt = 1; attempt <= MAX_INIT_RETRIES; attempt++)
            {
                ct.ThrowIfCancellationRequested();

                SetStatus(attempt == 1
                    ? "Initializing Nakama..."
                    : $"Retrying Nakama initialization ({attempt}/{MAX_INIT_RETRIES})...");

                bool ready = ClanManager.InitializeFromNakamaManager();
                if (ready)
                {
                    Debug.Log($"{LOG_TAG} Initialized from existing IVXNManager on attempt {attempt}.");
                    SetStatus("Clan demo ready.");
                    return true;
                }

                Debug.Log($"{LOG_TAG} Direct init failed (attempt {attempt}), trying EnsureNakamaInitializedAsync...");
                ready = await IVXClanService.EnsureNakamaInitializedAsync();
                if (ready)
                {
                    ready = ClanManager.InitializeFromNakamaManager();
                }

                if (ready)
                {
                    Debug.Log($"{LOG_TAG} Initialized via EnsureNakama on attempt {attempt}.");
                    SetStatus("Clan demo ready.");
                    return true;
                }

                if (attempt < MAX_INIT_RETRIES)
                {
                    Debug.Log($"{LOG_TAG} Attempt {attempt} failed, waiting {RETRY_DELAY_MS}ms before retry...");
                    await Task.Delay(RETRY_DELAY_MS, ct);
                }
            }

            string reason = DiagnoseInitFailure();
            SetStatus($"Nakama is not ready. {reason}");
            Debug.LogWarning($"{LOG_TAG} All {MAX_INIT_RETRIES} init attempts failed. {reason}");
            return false;
        }

        private bool VerifyUserSession()
        {
            global::UserSessionManager.UserSession session = global::UserSessionManager.Current;
            if (session == null)
            {
                SetStatus("No auth session found. Log in from IVX_AuthTest first.");
                Debug.LogWarning($"{LOG_TAG} UserSessionManager.Current is null. User has not logged in.");
                return false;
            }

            if (string.IsNullOrWhiteSpace(session.accessToken))
            {
                SetStatus("Auth session has no access token. Please log in again.");
                Debug.LogWarning($"{LOG_TAG} UserSessionManager has a session but accessToken is empty.");
                return false;
            }

            bool tokenFresh = global::UserSessionManager.IsAccessTokenFresh();
            if (!tokenFresh)
            {
                Debug.LogWarning($"{LOG_TAG} Access token has expired (epoch: {session.accessTokenExpiryEpoch}). " +
                                 $"Will attempt Nakama refresh or re-login.");
            }

            Debug.Log($"{LOG_TAG} UserSession verified: userId={session.userId}, " +
                       $"tokenFresh={tokenFresh}, email={session.email ?? "n/a"}");
            return true;
        }

        private string DiagnoseInitFailure()
        {
            global::UserSessionManager.UserSession session = global::UserSessionManager.Current;
            if (session == null)
            {
                return "No auth session. Log in first.";
            }

            if (!global::UserSessionManager.IsAccessTokenFresh())
            {
                return "Access token has expired. Re-login from IVX_AuthTest.";
            }

            System.Type managerType = System.Type.GetType("IntelliVerseX.Backend.Nakama.IVXNManager, IntelliVerseX.V2");
            if (managerType == null)
            {
                return "IVXNManager type not found. Check assembly definitions.";
            }

            var instanceProp = managerType.GetProperty("Instance", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
            object manager = instanceProp?.GetValue(null);
            if (manager == null)
            {
                return "IVXNManager singleton not created. Check IVXPanelLogin flow.";
            }

            bool isInit = (bool?)managerType.GetProperty("IsInitialized")?.GetValue(manager) ?? false;
            if (!isInit)
            {
                return "IVXNManager exists but is not initialized. Initialization may have failed.";
            }

            string gameId = managerType.GetProperty("GameId")?.GetValue(manager) as string;
            if (string.IsNullOrWhiteSpace(gameId))
            {
                return "IVXNManager is initialized but GameId is empty. Check SDK config.";
            }

            return "Log in from IVX_AuthTest, then reopen this scene.";
        }

        #endregion

        #region Internal Helpers

        private async Task RunBusyOperationAsync(string busyMessage, Func<Task> action)
        {
            if (IsBusy) return;

            IsBusy = true;
            SetStatus(busyMessage);
            _panel?.RefreshView();

            try
            {
                await action();
            }
            catch (OperationCanceledException)
            {
                SetStatus("Operation cancelled.");
            }
            catch (Exception ex)
            {
                SetStatus($"Operation failed: {ex.Message}");
                Debug.LogError($"{LOG_TAG} {ex}");
            }
            finally
            {
                IsBusy = false;
                _panel?.RefreshView();
            }
        }

        private void EnsureManager()
        {
            if (ClanManager != null) return;

            ClanManager = IVXClanManager.Instance;
            if (ClanManager != null) return;

            var managerObject = new GameObject("IVXClanManager");
            ClanManager = managerObject.AddComponent<IVXClanManager>();
        }

        private void EnsurePanel()
        {
            if (_panel != null)
            {
                _panel.Initialize(this);
                return;
            }

            _panel = FindFirstObjectByType<IVXClanPanel>();
            if (_panel == null)
            {
                Debug.LogWarning($"{LOG_TAG} IVXClanPanel not found in scene. Creating fallback.");
                var panelObject = new GameObject("IVXClanPanel");
                _panel = panelObject.AddComponent<IVXClanPanel>();
            }

            _panel.Initialize(this);
        }

        private void HandleClanChanged(IVXClanData clan)
        {
            SetStatus(clan == null ? "No clan joined." : $"Current clan: {clan.Name}");
        }

        private void HandleBrowseResultsUpdated(IReadOnlyList<IVXClanData> clans)
        {
            if (clans == null) return;
            SetStatus($"Browse results updated: {clans.Count} clan(s).");
        }

        private void HandleMembersUpdated(IReadOnlyList<IVXClanMemberData> members)
        {
            if (CurrentClan == null) return;
            SetStatus($"Loaded {members?.Count ?? 0} member(s) for {CurrentClan.Name}.");
        }

        private void SetStatus(string message)
        {
            StatusMessage = message;
            _panel?.RefreshView();
        }

        private IVXClanData CurrentClan => ClanManager != null ? ClanManager.CurrentClan : null;

        #endregion
    }
}
