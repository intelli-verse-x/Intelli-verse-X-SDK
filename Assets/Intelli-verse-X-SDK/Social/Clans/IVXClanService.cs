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
    /// Low-level Nakama service for clan operations.
    /// </summary>
    public static class IVXClanService
    {
        private const string LOG_TAG = "[IVXClan]";
        private const string RPC_GET_USER_GROUPS = "get_user_groups";
        private const string RPC_CREATE_GAME_GROUP = "create_game_group";

        private static Type _cachedNakamaManagerType;
        private static bool _hasCheckedNakamaManagerType;

        /// <summary>
        /// Ensures the shared Nakama manager is initialized for the current user.
        /// Handles stale tokens by attempting Nakama-level session refresh.
        /// </summary>
        public static async Task<bool> EnsureNakamaInitializedAsync()
        {
            try
            {
                global::UserSessionManager.UserSession userSession = global::UserSessionManager.Current;
                if (userSession == null || string.IsNullOrWhiteSpace(userSession.accessToken))
                {
                    Debug.LogWarning($"{LOG_TAG} No persisted user session. User must log in first.");
                    return false;
                }

                bool tokenFresh = global::UserSessionManager.IsAccessTokenFresh();
                if (!tokenFresh)
                {
                    Debug.LogWarning($"{LOG_TAG} Access token is stale (epoch {userSession.accessTokenExpiryEpoch}). " +
                                     "Will attempt Nakama-level refresh if an existing session exists.");
                }

                if (!_hasCheckedNakamaManagerType)
                {
                    _cachedNakamaManagerType = Type.GetType("IntelliVerseX.Backend.Nakama.IVXNManager, IntelliVerseX.V2");
                    _hasCheckedNakamaManagerType = true;
                }
                Type managerType = _cachedNakamaManagerType;
                if (managerType == null)
                {
                    Debug.LogError($"{LOG_TAG} IVXNManager type not found. Check IntelliVerseX.V2 assembly.");
                    return false;
                }

                PropertyInfo instanceProperty = managerType.GetProperty(
                    "Instance",
                    BindingFlags.Public | BindingFlags.Static);

                object manager = instanceProperty?.GetValue(null);
                if (manager == null)
                {
                    if (!tokenFresh)
                    {
                        Debug.LogWarning($"{LOG_TAG} Cannot bootstrap IVXNManager with a stale token. Re-login required.");
                        return false;
                    }

                    Debug.Log($"{LOG_TAG} Creating IVXNManager singleton...");
                    var bootstrap = new GameObject("IVXNManager");
                    UnityEngine.Object.DontDestroyOnLoad(bootstrap);
                    bootstrap.AddComponent(managerType);
                    await Task.Yield();
                    manager = instanceProperty?.GetValue(null);
                }

                if (manager == null)
                {
                    Debug.LogError($"{LOG_TAG} Failed to create IVXNManager.");
                    return false;
                }

                PropertyInfo isInitializedProperty = managerType.GetProperty("IsInitialized");
                PropertyInfo sessionProperty = managerType.GetProperty("Session");
                bool isInitialized = (bool?)isInitializedProperty?.GetValue(manager) ?? false;
                ISession nakamaSession = sessionProperty?.GetValue(manager) as ISession;

                if (isInitialized && nakamaSession != null && !nakamaSession.IsExpired)
                {
                    Debug.Log($"{LOG_TAG} IVXNManager already initialized with valid session.");
                    return true;
                }

                if (isInitialized && nakamaSession != null && nakamaSession.IsExpired)
                {
                    Debug.Log($"{LOG_TAG} Nakama session expired, attempting EnsureValidSessionAsync...");
                    MethodInfo ensureSessionMethod = managerType.GetMethod("EnsureValidSessionAsync",
                        BindingFlags.Public | BindingFlags.Instance);
                    if (ensureSessionMethod != null)
                    {
                        if (ensureSessionMethod.Invoke(manager, null) is Task<bool> refreshTask)
                        {
                            bool refreshed = await refreshTask;
                            if (refreshed)
                            {
                                Debug.Log($"{LOG_TAG} Nakama session refreshed successfully.");
                                return true;
                            }
                        }
                    }
                }

                MethodInfo initializeMethod = managerType.GetMethod(
                    "InitializeForCurrentUserAsync",
                    BindingFlags.Public | BindingFlags.Instance,
                    null,
                    new[] { typeof(bool) },
                    null);

                if (initializeMethod != null)
                {
                    bool forceReauth = !tokenFresh;
                    Debug.Log($"{LOG_TAG} Calling InitializeForCurrentUserAsync(forceReauth={forceReauth})...");
                    if (initializeMethod.Invoke(manager, new object[] { forceReauth }) is Task<bool> initializeTask)
                    {
                        bool result = await initializeTask;
                        Debug.Log($"{LOG_TAG} InitializeForCurrentUserAsync returned {result}.");
                        return result;
                    }
                }

                initializeMethod = managerType.GetMethod(
                    "InitializeForCurrentUserAsync",
                    BindingFlags.Public | BindingFlags.Instance,
                    null,
                    Type.EmptyTypes,
                    null);

                if (initializeMethod != null && initializeMethod.Invoke(manager, null) is Task<bool> legacyTask)
                {
                    return await legacyTask;
                }

                Debug.LogError($"{LOG_TAG} InitializeForCurrentUserAsync method not found on IVXNManager.");
                return false;
            }
            catch (TargetInvocationException tie) when (tie.InnerException != null)
            {
                Debug.LogError($"{LOG_TAG} EnsureNakamaInitializedAsync inner exception: {tie.InnerException.Message}");
                return false;
            }
            catch (Exception ex)
            {
                Debug.LogError($"{LOG_TAG} EnsureNakamaInitializedAsync failed: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Loads the current user's clans.
        /// </summary>
        public static async Task<IVXClanOperationResult> LoadCurrentClanAsync(
            IClient client,
            ISession session,
            string gameId,
            CancellationToken ct = default)
        {
            if (!TryValidateContext(client, session, gameId, out string error))
            {
                return IVXClanOperationResult.Failure(error);
            }

            try
            {
                ct.ThrowIfCancellationRequested();

                var payload = new IVXGetUserGroupsPayload
                {
                    gameId = gameId
                };

                var result = await client.RpcAsync(session, RPC_GET_USER_GROUPS, JsonUtility.ToJson(payload));
                ct.ThrowIfCancellationRequested();

                var response = JsonUtility.FromJson<IVXUserGroupsResponse>(result.Payload);
                if (response == null)
                {
                    return IVXClanOperationResult.Failure("Clan response payload was empty.");
                }

                if (!response.success)
                {
                    return IVXClanOperationResult.Failure(string.IsNullOrWhiteSpace(response.error)
                        ? "Failed to load current clan."
                        : response.error);
                }

                if (response.groups == null || response.groups.Length == 0)
                {
                    return IVXClanOperationResult.Success();
                }

                return IVXClanOperationResult.Success(MapGroupInfo(response.groups[0]));
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                Debug.LogError($"{LOG_TAG} LoadCurrentClanAsync failed: {ex.Message}");
                return IVXClanOperationResult.Failure(ex.Message);
            }
        }

        /// <summary>
        /// Creates a new clan.
        /// </summary>
        public static async Task<IVXClanOperationResult> CreateClanAsync(
            IClient client,
            ISession session,
            string gameId,
            string name,
            string description,
            bool isOpen,
            int maxMembers,
            CancellationToken ct = default)
        {
            if (!TryValidateContext(client, session, gameId, out string error))
            {
                return IVXClanOperationResult.Failure(error);
            }

            if (string.IsNullOrWhiteSpace(name))
            {
                return IVXClanOperationResult.Failure("Clan name is required.");
            }

            try
            {
                ct.ThrowIfCancellationRequested();

                var payload = new IVXCreateClanPayload
                {
                    gameId = gameId,
                    name = name.Trim(),
                    description = description ?? string.Empty,
                    maxCount = Mathf.Max(2, maxMembers),
                    open = isOpen,
                    groupType = "guild"
                };

                var result = await client.RpcAsync(session, RPC_CREATE_GAME_GROUP, JsonUtility.ToJson(payload));
                ct.ThrowIfCancellationRequested();

                var response = JsonUtility.FromJson<IVXCreateClanResponse>(result.Payload);
                if (response == null)
                {
                    return IVXClanOperationResult.Failure("Clan create response payload was empty.");
                }

                if (!response.success)
                {
                    return IVXClanOperationResult.Failure(string.IsNullOrWhiteSpace(response.error)
                        ? "Failed to create clan."
                        : response.error);
                }

                var createdClan = new IVXClanData
                {
                    ClanId = response.groupId,
                    Name = response.name,
                    Description = response.description,
                    MemberCount = 1,
                    MaxMembers = response.maxCount,
                    Level = 1,
                    Experience = 0,
                    IsOpen = response.open,
                    UserRole = "owner",
                    JoinedAt = response.createdAt
                };

                return IVXClanOperationResult.Success(createdClan);
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                Debug.LogError($"{LOG_TAG} CreateClanAsync failed: {ex.Message}");
                return IVXClanOperationResult.Failure(ex.Message);
            }
        }

        /// <summary>
        /// Searches for clans using Nakama group listing.
        /// </summary>
        public static async Task<IVXClanBrowseResult> BrowseClansAsync(
            IClient client,
            ISession session,
            string query,
            int limit,
            CancellationToken ct = default)
        {
            var result = new IVXClanBrowseResult();

            if (!TryValidateContext(client, session, "unused", out string error, requireGameId: false))
            {
                result.ErrorMessage = error;
                return result;
            }

            try
            {
                ct.ThrowIfCancellationRequested();

                string trimmedQuery = string.IsNullOrWhiteSpace(query) ? null : query.Trim();
                var response = await client.ListGroupsAsync(session, trimmedQuery, Mathf.Clamp(limit, 1, 100));
                ct.ThrowIfCancellationRequested();

                if (response?.Groups == null)
                {
                    return result;
                }

                foreach (var group in response.Groups)
                {
                    if (group == null)
                    {
                        continue;
                    }

                    result.Clans.Add(new IVXClanData
                    {
                        ClanId = group.Id,
                        Name = group.Name,
                        Description = group.Description,
                        AvatarUrl = group.AvatarUrl,
                        MemberCount = group.EdgeCount,
                        MaxMembers = group.MaxCount,
                        IsOpen = group.Open,
                        CreatorId = group.CreatorId
                    });
                }

                return result;
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                Debug.LogError($"{LOG_TAG} BrowseClansAsync failed: {ex.Message}");
                result.ErrorMessage = ex.Message;
                return result;
            }
        }

        /// <summary>
        /// Joins a clan.
        /// </summary>
        public static async Task<IVXClanOperationResult> JoinClanAsync(
            IClient client,
            ISession session,
            string clanId,
            CancellationToken ct = default)
        {
            if (!TryValidateContext(client, session, "unused", out string error, requireGameId: false))
            {
                return IVXClanOperationResult.Failure(error);
            }

            if (string.IsNullOrWhiteSpace(clanId))
            {
                return IVXClanOperationResult.Failure("Clan ID is required.");
            }

            try
            {
                ct.ThrowIfCancellationRequested();
                await client.JoinGroupAsync(session, clanId);
                return IVXClanOperationResult.Success();
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                Debug.LogError($"{LOG_TAG} JoinClanAsync failed: {ex.Message}");
                return IVXClanOperationResult.Failure(ex.Message);
            }
        }

        /// <summary>
        /// Leaves a clan.
        /// </summary>
        public static async Task<IVXClanOperationResult> LeaveClanAsync(
            IClient client,
            ISession session,
            string clanId,
            CancellationToken ct = default)
        {
            if (!TryValidateContext(client, session, "unused", out string error, requireGameId: false))
            {
                return IVXClanOperationResult.Failure(error);
            }

            if (string.IsNullOrWhiteSpace(clanId))
            {
                return IVXClanOperationResult.Failure("Clan ID is required.");
            }

            try
            {
                ct.ThrowIfCancellationRequested();
                await client.LeaveGroupAsync(session, clanId);
                return IVXClanOperationResult.Success();
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                Debug.LogError($"{LOG_TAG} LeaveClanAsync failed: {ex.Message}");
                return IVXClanOperationResult.Failure(ex.Message);
            }
        }

        /// <summary>
        /// Loads members for the given clan.
        /// </summary>
        public static async Task<List<IVXClanMemberData>> LoadMembersAsync(
            IClient client,
            ISession session,
            string clanId,
            CancellationToken ct = default)
        {
            if (!TryValidateContext(client, session, "unused", out string error, requireGameId: false))
            {
                Debug.LogError($"{LOG_TAG} LoadMembersAsync failed: {error}");
                return new List<IVXClanMemberData>();
            }

            if (string.IsNullOrWhiteSpace(clanId))
            {
                return new List<IVXClanMemberData>();
            }

            try
            {
                ct.ThrowIfCancellationRequested();
                var response = await client.ListGroupUsersAsync(session, clanId);
                ct.ThrowIfCancellationRequested();

                var members = new List<IVXClanMemberData>();
                if (response?.GroupUsers == null)
                {
                    return members;
                }

                foreach (var groupUser in response.GroupUsers)
                {
                    if (groupUser?.User == null)
                    {
                        continue;
                    }

                    members.Add(new IVXClanMemberData
                    {
                        UserId = groupUser.User.Id,
                        Username = groupUser.User.Username,
                        DisplayName = string.IsNullOrWhiteSpace(groupUser.User.DisplayName)
                            ? groupUser.User.Username
                            : groupUser.User.DisplayName,
                        RoleState = groupUser.State,
                        IsOnline = groupUser.User.Online
                    });
                }

                return members;
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                Debug.LogError($"{LOG_TAG} LoadMembersAsync failed: {ex.Message}");
                return new List<IVXClanMemberData>();
            }
        }

        private static IVXClanData MapGroupInfo(IVXGroupInfo group)
        {
            if (group == null)
            {
                return null;
            }

            return new IVXClanData
            {
                ClanId = group.groupId,
                Name = group.name,
                Description = group.description,
                MemberCount = group.memberCount,
                MaxMembers = group.maxCount,
                Level = group.level,
                Experience = group.xp,
                IsOpen = group.open,
                UserRole = group.role,
                JoinedAt = group.joinedAt
            };
        }

        private static bool TryValidateContext(
            IClient client,
            ISession session,
            string gameId,
            out string error,
            bool requireGameId = true)
        {
            if (client == null)
            {
                error = "Nakama client is not initialized.";
                return false;
            }

            if (session == null || session.IsExpired)
            {
                error = "Nakama session is not ready or has expired.";
                return false;
            }

            if (requireGameId && string.IsNullOrWhiteSpace(gameId))
            {
                error = "Game ID is required for clan operations.";
                return false;
            }

            error = null;
            return true;
        }
    }
}
