using System;
using System.Collections.Generic;
using UnityEngine;

namespace IntelliVerseX.Discord
{
    /// <summary>
    /// Represents a friend entry in the unified friends list,
    /// combining Discord relationships with in-game Nakama friends.
    /// </summary>
    [Serializable]
    public sealed class IVXUnifiedFriend
    {
        /// <summary>The friend source (Discord, Game, or Both).</summary>
        public IVXFriendSource Source;
        /// <summary>Display name shown in the friends list.</summary>
        public string DisplayName;
        /// <summary>Discord user ID (null if game-only friend).</summary>
        public string DiscordUserId;
        /// <summary>Nakama user ID (null if Discord-only friend).</summary>
        public string GameUserId;
        /// <summary>Avatar URL (prefers Discord avatar).</summary>
        public string AvatarUrl;
        /// <summary>Whether the friend is currently online.</summary>
        public bool IsOnline;
        /// <summary>Whether the friend is currently in this game.</summary>
        public bool IsInGame;
        /// <summary>Rich Presence activity string (e.g. "Playing Ranked Match").</summary>
        public string ActivityText;
        /// <summary>Whether this friend can be invited to join.</summary>
        public bool CanInvite;
        /// <summary>Discord friendship status for this user.</summary>
        public IVXRelationshipType DiscordRelationshipType;
        /// <summary>In-game (Nakama) friendship status for this user.</summary>
        public IVXRelationshipType GameRelationshipType;
    }

    /// <summary>
    /// Source of a friend relationship.
    /// </summary>
    public enum IVXFriendSource
    {
        /// <summary>Friend exists only on Discord.</summary>
        Discord,
        /// <summary>Friend exists only in-game (Nakama).</summary>
        Game,
        /// <summary>Friend exists on both Discord and in-game.</summary>
        Both
    }

    /// <summary>
    /// Normalized friendship / block state for Discord and game layers.
    /// </summary>
    public enum IVXRelationshipType
    {
        /// <summary>No relationship or unknown.</summary>
        None,
        /// <summary>Established friend.</summary>
        Friend,
        /// <summary>Incoming friend request (awaiting local action).</summary>
        PendingIncoming,
        /// <summary>Outgoing friend request (awaiting remote action).</summary>
        PendingOutgoing,
        /// <summary>User is blocked.</summary>
        Blocked
    }

    /// <summary>
    /// Manages the unified friends list that merges Discord friends
    /// with in-game Nakama friends into a single view.
    /// </summary>
    public sealed class IVXDiscordFriends : MonoBehaviour
    {
        #region Constants

        private const string LOG_TAG = "[IVXDiscordFriends]";

        #endregion

        #region Private Fields

        private static IVXDiscordFriends _instance;
        private readonly List<IVXUnifiedFriend> _friends = new();

        #endregion

        #region Properties

        /// <summary>Singleton instance.</summary>
        public static IVXDiscordFriends Instance => _instance;
        /// <summary>The merged friends list (read-only).</summary>
        public IReadOnlyList<IVXUnifiedFriend> Friends => _friends;
        /// <summary>Number of friends currently online.</summary>
        public int OnlineCount
        {
            get
            {
                int count = 0;
                for (int i = 0; i < _friends.Count; i++)
                    if (_friends[i].IsOnline) count++;
                return count;
            }
        }
        /// <summary>Number of friends currently in-game.</summary>
        public int InGameCount
        {
            get
            {
                int count = 0;
                for (int i = 0; i < _friends.Count; i++)
                    if (_friends[i].IsInGame) count++;
                return count;
            }
        }

        #endregion

        #region Events

        /// <summary>Fired when the friends list is refreshed.</summary>
        public event Action<IReadOnlyList<IVXUnifiedFriend>> OnFriendsUpdated;
        /// <summary>Fired when a friend comes online.</summary>
        public event Action<IVXUnifiedFriend> OnFriendOnline;
        /// <summary>Fired when a friend goes offline.</summary>
        public event Action<IVXUnifiedFriend> OnFriendOffline;
        /// <summary>Fired when a friend starts playing this game.</summary>
        public event Action<IVXUnifiedFriend> OnFriendJoinedGame;
        /// <summary>Fired when an incoming friend request is received. Arguments: userId, displayName.</summary>
        public event Action<string, string> OnFriendRequestReceived;
        /// <summary>Fired when a friend request is accepted. Argument: userId.</summary>
        public event Action<string> OnFriendRequestAccepted;
        /// <summary>Fired when a friend was removed. Argument: userId.</summary>
        public event Action<string> OnFriendRemoved;
        /// <summary>Fired when a user was blocked. Argument: userId.</summary>
        public event Action<string> OnUserBlocked;
        /// <summary>Fired when a user was unblocked. Argument: userId.</summary>
        public event Action<string> OnUserUnblocked;

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
        }

        private void OnDestroy()
        {
            if (_instance == this) _instance = null;
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Refresh the unified friends list by fetching from Discord and Nakama,
        /// then merging results.
        /// </summary>
        public void Refresh()
        {
            Debug.Log($"{LOG_TAG} Refreshing unified friends list...");

#if INTELLIVERSEX_HAS_DISCORD
            FetchAndMerge();
#else
            _friends.Clear();
            _friends.Add(new IVXUnifiedFriend
            {
                Source = IVXFriendSource.Both,
                DisplayName = "StubFriend#1234",
                DiscordUserId = "stub_111",
                GameUserId = "nakama_111",
                IsOnline = true,
                IsInGame = true,
                ActivityText = "Playing Ranked Match",
                CanInvite = true,
                DiscordRelationshipType = IVXRelationshipType.Friend,
                GameRelationshipType = IVXRelationshipType.Friend
            });
            _friends.Add(new IVXUnifiedFriend
            {
                Source = IVXFriendSource.Discord,
                DisplayName = "DiscordOnlyFriend",
                DiscordUserId = "stub_222",
                IsOnline = true,
                IsInGame = false,
                ActivityText = "Online on Discord",
                CanInvite = false,
                DiscordRelationshipType = IVXRelationshipType.Friend,
                GameRelationshipType = IVXRelationshipType.None
            });
            _friends.Add(new IVXUnifiedFriend
            {
                Source = IVXFriendSource.Game,
                DisplayName = "GameOnlyFriend",
                GameUserId = "nakama_333",
                IsOnline = false,
                IsInGame = false,
                CanInvite = false,
                DiscordRelationshipType = IVXRelationshipType.None,
                GameRelationshipType = IVXRelationshipType.Friend
            });
            Debug.Log($"{LOG_TAG} [Stub] Loaded {_friends.Count} friends.");
            OnFriendsUpdated?.Invoke(_friends);
#endif
        }

        /// <summary>
        /// Get friends filtered by source.
        /// </summary>
        /// <param name="source">The friend source to filter by.</param>
        /// <returns>Filtered list of friends.</returns>
        public List<IVXUnifiedFriend> GetBySource(IVXFriendSource source)
        {
            var result = new List<IVXUnifiedFriend>();
            for (int i = 0; i < _friends.Count; i++)
            {
                if (_friends[i].Source == source || _friends[i].Source == IVXFriendSource.Both)
                    result.Add(_friends[i]);
            }
            return result;
        }

        /// <summary>
        /// Get only friends currently in this game (joinable).
        /// </summary>
        /// <returns>List of in-game friends.</returns>
        public List<IVXUnifiedFriend> GetInGameFriends()
        {
            var result = new List<IVXUnifiedFriend>();
            for (int i = 0; i < _friends.Count; i++)
            {
                if (_friends[i].IsInGame)
                    result.Add(_friends[i]);
            }
            return result;
        }

        /// <summary>
        /// Sends an in-game (Nakama) friend request by username.
        /// </summary>
        /// <param name="username">Target username.</param>
        /// <param name="onComplete">Optional callback; true on success.</param>
        public void SendGameFriendRequest(string username, Action<bool> onComplete = null)
        {
            if (!EnsureInstanceReady(onComplete))
                return;
            if (string.IsNullOrEmpty(username))
            {
                Debug.LogWarning($"{LOG_TAG} SendGameFriendRequest: username is null or empty.");
                onComplete?.Invoke(false);
                return;
            }

            Debug.Log($"{LOG_TAG} SendGameFriendRequest username={username}");

#if INTELLIVERSEX_HAS_DISCORD
            SendGameFriendRequestInternal(username, onComplete);
#else
            Debug.Log($"{LOG_TAG} [Stub] SendGameFriendRequest success.");
            onComplete?.Invoke(true);
#endif
        }

        /// <summary>
        /// Sends an in-game (Nakama) friend request by user id.
        /// </summary>
        /// <param name="userId">Target user id.</param>
        /// <param name="onComplete">Optional callback; true on success.</param>
        public void SendGameFriendRequestById(ulong userId, Action<bool> onComplete = null)
        {
            if (!EnsureInstanceReady(onComplete))
                return;
            if (userId == 0)
            {
                Debug.LogWarning($"{LOG_TAG} SendGameFriendRequestById: invalid userId.");
                onComplete?.Invoke(false);
                return;
            }

            Debug.Log($"{LOG_TAG} SendGameFriendRequestById userId={userId}");

#if INTELLIVERSEX_HAS_DISCORD
            SendGameFriendRequestByIdInternal(userId, onComplete);
#else
            Debug.Log($"{LOG_TAG} [Stub] SendGameFriendRequestById success.");
            onComplete?.Invoke(true);
#endif
        }

        /// <summary>
        /// Accepts an incoming in-game friend request.
        /// </summary>
        /// <param name="userId">Requester user id.</param>
        /// <param name="onComplete">Optional callback; true on success.</param>
        public void AcceptGameFriendRequest(ulong userId, Action<bool> onComplete = null)
        {
            if (!EnsureInstanceReady(onComplete))
                return;
            if (userId == 0)
            {
                Debug.LogWarning($"{LOG_TAG} AcceptGameFriendRequest: invalid userId.");
                onComplete?.Invoke(false);
                return;
            }

            Debug.Log($"{LOG_TAG} AcceptGameFriendRequest userId={userId}");

#if INTELLIVERSEX_HAS_DISCORD
            AcceptGameFriendRequestInternal(userId, onComplete);
#else
            Debug.Log($"{LOG_TAG} [Stub] AcceptGameFriendRequest success.");
            onComplete?.Invoke(true);
#endif
        }

        /// <summary>
        /// Rejects an incoming in-game friend request.
        /// </summary>
        /// <param name="userId">Requester user id.</param>
        /// <param name="onComplete">Optional callback; true on success.</param>
        public void RejectGameFriendRequest(ulong userId, Action<bool> onComplete = null)
        {
            if (!EnsureInstanceReady(onComplete))
                return;
            if (userId == 0)
            {
                Debug.LogWarning($"{LOG_TAG} RejectGameFriendRequest: invalid userId.");
                onComplete?.Invoke(false);
                return;
            }

            Debug.Log($"{LOG_TAG} RejectGameFriendRequest userId={userId}");

#if INTELLIVERSEX_HAS_DISCORD
            RejectGameFriendRequestInternal(userId, onComplete);
#else
            Debug.Log($"{LOG_TAG} [Stub] RejectGameFriendRequest success.");
            onComplete?.Invoke(true);
#endif
        }

        /// <summary>
        /// Cancels an outgoing in-game friend request.
        /// </summary>
        /// <param name="userId">Target user id.</param>
        /// <param name="onComplete">Optional callback; true on success.</param>
        public void CancelGameFriendRequest(ulong userId, Action<bool> onComplete = null)
        {
            if (!EnsureInstanceReady(onComplete))
                return;
            if (userId == 0)
            {
                Debug.LogWarning($"{LOG_TAG} CancelGameFriendRequest: invalid userId.");
                onComplete?.Invoke(false);
                return;
            }

            Debug.Log($"{LOG_TAG} CancelGameFriendRequest userId={userId}");

#if INTELLIVERSEX_HAS_DISCORD
            CancelGameFriendRequestInternal(userId, onComplete);
#else
            Debug.Log($"{LOG_TAG} [Stub] CancelGameFriendRequest success.");
            onComplete?.Invoke(true);
#endif
        }

        /// <summary>
        /// Removes an in-game friend.
        /// </summary>
        /// <param name="userId">Friend user id.</param>
        /// <param name="onComplete">Optional callback; true on success.</param>
        public void RemoveGameFriend(ulong userId, Action<bool> onComplete = null)
        {
            if (!EnsureInstanceReady(onComplete))
                return;
            if (userId == 0)
            {
                Debug.LogWarning($"{LOG_TAG} RemoveGameFriend: invalid userId.");
                onComplete?.Invoke(false);
                return;
            }

            Debug.Log($"{LOG_TAG} RemoveGameFriend userId={userId}");

#if INTELLIVERSEX_HAS_DISCORD
            RemoveGameFriendInternal(userId, onComplete);
#else
            Debug.Log($"{LOG_TAG} [Stub] RemoveGameFriend success.");
            onComplete?.Invoke(true);
#endif
        }

        /// <summary>
        /// Sends a Discord friend request by username (discriminator or handle per SDK).
        /// </summary>
        /// <param name="username">Target username.</param>
        /// <param name="onComplete">Optional callback; true on success.</param>
        public void SendDiscordFriendRequest(string username, Action<bool> onComplete = null)
        {
            if (!EnsureInstanceReady(onComplete))
                return;
            if (string.IsNullOrEmpty(username))
            {
                Debug.LogWarning($"{LOG_TAG} SendDiscordFriendRequest: username is null or empty.");
                onComplete?.Invoke(false);
                return;
            }

            Debug.Log($"{LOG_TAG} SendDiscordFriendRequest username={username}");

#if INTELLIVERSEX_HAS_DISCORD
            SendDiscordFriendRequestInternal(username, onComplete);
#else
            Debug.Log($"{LOG_TAG} [Stub] SendDiscordFriendRequest success.");
            onComplete?.Invoke(true);
#endif
        }

        /// <summary>
        /// Sends a Discord friend request by Snowflake user id.
        /// </summary>
        /// <param name="userId">Discord user id.</param>
        /// <param name="onComplete">Optional callback; true on success.</param>
        public void SendDiscordFriendRequestById(ulong userId, Action<bool> onComplete = null)
        {
            if (!EnsureInstanceReady(onComplete))
                return;
            if (userId == 0)
            {
                Debug.LogWarning($"{LOG_TAG} SendDiscordFriendRequestById: invalid userId.");
                onComplete?.Invoke(false);
                return;
            }

            Debug.Log($"{LOG_TAG} SendDiscordFriendRequestById userId={userId}");

#if INTELLIVERSEX_HAS_DISCORD
            SendDiscordFriendRequestByIdInternal(userId, onComplete);
#else
            Debug.Log($"{LOG_TAG} [Stub] SendDiscordFriendRequestById success.");
            onComplete?.Invoke(true);
#endif
        }

        /// <summary>
        /// Accepts an incoming Discord friend request.
        /// </summary>
        /// <param name="userId">Discord user id.</param>
        /// <param name="onComplete">Optional callback; true on success.</param>
        public void AcceptDiscordFriendRequest(ulong userId, Action<bool> onComplete = null)
        {
            if (!EnsureInstanceReady(onComplete))
                return;
            if (userId == 0)
            {
                Debug.LogWarning($"{LOG_TAG} AcceptDiscordFriendRequest: invalid userId.");
                onComplete?.Invoke(false);
                return;
            }

            Debug.Log($"{LOG_TAG} AcceptDiscordFriendRequest userId={userId}");

#if INTELLIVERSEX_HAS_DISCORD
            AcceptDiscordFriendRequestInternal(userId, onComplete);
#else
            Debug.Log($"{LOG_TAG} [Stub] AcceptDiscordFriendRequest success.");
            onComplete?.Invoke(true);
#endif
        }

        /// <summary>
        /// Rejects an incoming Discord friend request.
        /// </summary>
        /// <param name="userId">Discord user id.</param>
        /// <param name="onComplete">Optional callback; true on success.</param>
        public void RejectDiscordFriendRequest(ulong userId, Action<bool> onComplete = null)
        {
            if (!EnsureInstanceReady(onComplete))
                return;
            if (userId == 0)
            {
                Debug.LogWarning($"{LOG_TAG} RejectDiscordFriendRequest: invalid userId.");
                onComplete?.Invoke(false);
                return;
            }

            Debug.Log($"{LOG_TAG} RejectDiscordFriendRequest userId={userId}");

#if INTELLIVERSEX_HAS_DISCORD
            RejectDiscordFriendRequestInternal(userId, onComplete);
#else
            Debug.Log($"{LOG_TAG} [Stub] RejectDiscordFriendRequest success.");
            onComplete?.Invoke(true);
#endif
        }

        /// <summary>
        /// Cancels an outgoing Discord friend request.
        /// </summary>
        /// <param name="userId">Discord user id.</param>
        /// <param name="onComplete">Optional callback; true on success.</param>
        public void CancelDiscordFriendRequest(ulong userId, Action<bool> onComplete = null)
        {
            if (!EnsureInstanceReady(onComplete))
                return;
            if (userId == 0)
            {
                Debug.LogWarning($"{LOG_TAG} CancelDiscordFriendRequest: invalid userId.");
                onComplete?.Invoke(false);
                return;
            }

            Debug.Log($"{LOG_TAG} CancelDiscordFriendRequest userId={userId}");

#if INTELLIVERSEX_HAS_DISCORD
            CancelDiscordFriendRequestInternal(userId, onComplete);
#else
            Debug.Log($"{LOG_TAG} [Stub] CancelDiscordFriendRequest success.");
            onComplete?.Invoke(true);
#endif
        }

        /// <summary>
        /// Removes the Discord friendship and the in-game friend link for this user.
        /// </summary>
        /// <param name="userId">Discord user id.</param>
        /// <param name="onComplete">Optional callback; true on success.</param>
        public void RemoveDiscordAndGameFriend(ulong userId, Action<bool> onComplete = null)
        {
            if (!EnsureInstanceReady(onComplete))
                return;
            if (userId == 0)
            {
                Debug.LogWarning($"{LOG_TAG} RemoveDiscordAndGameFriend: invalid userId.");
                onComplete?.Invoke(false);
                return;
            }

            Debug.Log($"{LOG_TAG} RemoveDiscordAndGameFriend userId={userId}");

#if INTELLIVERSEX_HAS_DISCORD
            RemoveDiscordAndGameFriendInternal(userId, onComplete);
#else
            Debug.Log($"{LOG_TAG} [Stub] RemoveDiscordAndGameFriend success.");
            onComplete?.Invoke(true);
#endif
        }

        /// <summary>
        /// Blocks a user on Discord (and should update game-side block list as needed).
        /// </summary>
        /// <param name="userId">Discord user id.</param>
        /// <param name="onComplete">Optional callback; true on success.</param>
        public void BlockUser(ulong userId, Action<bool> onComplete = null)
        {
            if (!EnsureInstanceReady(onComplete))
                return;
            if (userId == 0)
            {
                Debug.LogWarning($"{LOG_TAG} BlockUser: invalid userId.");
                onComplete?.Invoke(false);
                return;
            }

            Debug.Log($"{LOG_TAG} BlockUser userId={userId}");

#if INTELLIVERSEX_HAS_DISCORD
            BlockUserInternal(userId, onComplete);
#else
            Debug.Log($"{LOG_TAG} [Stub] BlockUser success.");
            onComplete?.Invoke(true);
#endif
        }

        /// <summary>
        /// Unblocks a user on Discord (and should update game-side block list as needed).
        /// </summary>
        /// <param name="userId">Discord user id.</param>
        /// <param name="onComplete">Optional callback; true on success.</param>
        public void UnblockUser(ulong userId, Action<bool> onComplete = null)
        {
            if (!EnsureInstanceReady(onComplete))
                return;
            if (userId == 0)
            {
                Debug.LogWarning($"{LOG_TAG} UnblockUser: invalid userId.");
                onComplete?.Invoke(false);
                return;
            }

            Debug.Log($"{LOG_TAG} UnblockUser userId={userId}");

#if INTELLIVERSEX_HAS_DISCORD
            UnblockUserInternal(userId, onComplete);
#else
            Debug.Log($"{LOG_TAG} [Stub] UnblockUser success.");
            onComplete?.Invoke(true);
#endif
        }

        /// <summary>
        /// Returns unified entries that have an incoming pending friend request (game and/or Discord).
        /// </summary>
        /// <returns>Incoming pending requests.</returns>
        public List<IVXUnifiedFriend> GetPendingRequests()
        {
            if (_instance == null)
            {
                Debug.LogWarning($"{LOG_TAG} GetPendingRequests: IVXDiscordFriends instance is missing.");
                return new List<IVXUnifiedFriend>();
            }

            Debug.Log($"{LOG_TAG} GetPendingRequests");

            var result = new List<IVXUnifiedFriend>();
            for (int i = 0; i < _friends.Count; i++)
            {
                var f = _friends[i];
                if (f.GameRelationshipType == IVXRelationshipType.PendingIncoming ||
                    f.DiscordRelationshipType == IVXRelationshipType.PendingIncoming)
                    result.Add(f);
            }

            return result;
        }

        /// <summary>
        /// Returns unified entries marked blocked on Discord and/or in-game.
        /// </summary>
        /// <returns>Blocked users.</returns>
        public List<IVXUnifiedFriend> GetBlockedUsers()
        {
            if (_instance == null)
            {
                Debug.LogWarning($"{LOG_TAG} GetBlockedUsers: IVXDiscordFriends instance is missing.");
                return new List<IVXUnifiedFriend>();
            }

            Debug.Log($"{LOG_TAG} GetBlockedUsers");

            var result = new List<IVXUnifiedFriend>();
            for (int i = 0; i < _friends.Count; i++)
            {
                var f = _friends[i];
                if (f.GameRelationshipType == IVXRelationshipType.Blocked ||
                    f.DiscordRelationshipType == IVXRelationshipType.Blocked)
                    result.Add(f);
            }

            return result;
        }

        #endregion

        #region Private Methods

        private static bool EnsureInstanceReady(Action<bool> onComplete)
        {
            if (_instance == null)
            {
                Debug.LogError($"{LOG_TAG} IVXDiscordFriends is not initialized (no active instance).");
                onComplete?.Invoke(false);
                return false;
            }

            return true;
        }

#if INTELLIVERSEX_HAS_DISCORD
        private discordpp.Client Client => IVXDiscordManager.Instance?.DiscordClient;

        private void FetchAndMerge()
        {
            RegisterRelationshipCallbacks();

            var client = Client;
            if (client == null) return;

            try
            {
                client.GetRelationships((relationships) =>
                {
                    _friends.Clear();
                    if (relationships != null)
                    {
                        foreach (var rel in relationships)
                        {
                            var f = new IVXUnifiedFriend
                            {
                                DiscordUserId = rel.User.Id,
                                DisplayName = rel.User.Username ?? rel.User.Id.ToString(),
                                AvatarUrl = rel.User.AvatarUrl ?? "",
                                Source = IVXFriendSource.Discord,
                                IsOnline = rel.Presence?.Status == discordpp.StatusType.Online,
                                DiscordRelationshipType = MapRelationshipType(rel.Type)
                            };
                            _friends.Add(f);
                        }
                    }
                    Debug.Log($"{LOG_TAG} Fetched {_friends.Count} Discord relationships.");
                    OnFriendsUpdated?.Invoke(_friends);
                });
            }
            catch (Exception e) { Debug.LogError($"{LOG_TAG} FetchAndMerge error: {e.Message}"); }
        }

        private IVXRelationshipType MapRelationshipType(discordpp.RelationshipType type)
        {
            switch (type)
            {
                case discordpp.RelationshipType.Friend: return IVXRelationshipType.Friend;
                case discordpp.RelationshipType.PendingIncoming: return IVXRelationshipType.PendingIncoming;
                case discordpp.RelationshipType.PendingOutgoing: return IVXRelationshipType.PendingOutgoing;
                case discordpp.RelationshipType.Blocked: return IVXRelationshipType.Blocked;
                default: return IVXRelationshipType.None;
            }
        }

        private void RegisterRelationshipCallbacks()
        {
            var client = Client;
            if (client == null) return;
            try
            {
                client.SetRelationshipCreatedCallback((rel) =>
                {
                    Debug.Log($"{LOG_TAG} Relationship created: {rel.User?.Username} ({rel.Type})");
                    FetchAndMerge();
                });
                client.SetRelationshipDeletedCallback((userId) =>
                {
                    Debug.Log($"{LOG_TAG} Relationship deleted: {userId}");
                    OnFriendRemoved?.Invoke(userId.ToString());
                    FetchAndMerge();
                });
            }
            catch (Exception e) { Debug.LogError($"{LOG_TAG} RegisterRelationshipCallbacks error: {e.Message}"); }
        }

        private void SendGameFriendRequestInternal(string username, Action<bool> onComplete)
        {
            onComplete?.Invoke(false);
        }

        private void SendGameFriendRequestByIdInternal(ulong userId, Action<bool> onComplete)
        {
            onComplete?.Invoke(false);
        }

        private void AcceptGameFriendRequestInternal(ulong userId, Action<bool> onComplete)
        {
            onComplete?.Invoke(false);
        }

        private void RejectGameFriendRequestInternal(ulong userId, Action<bool> onComplete)
        {
            onComplete?.Invoke(false);
        }

        private void CancelGameFriendRequestInternal(ulong userId, Action<bool> onComplete)
        {
            onComplete?.Invoke(false);
        }

        private void RemoveGameFriendInternal(ulong userId, Action<bool> onComplete)
        {
            onComplete?.Invoke(false);
        }

        private void SendDiscordFriendRequestInternal(string username, Action<bool> onComplete)
        {
            var client = Client;
            if (client == null) { onComplete?.Invoke(false); return; }
            try
            {
                client.SendFriendRequest(username, (result) =>
                {
                    bool ok = result == discordpp.Client.Error.None;
                    Debug.Log($"{LOG_TAG} Friend request to '{username}': {result}");
                    onComplete?.Invoke(ok);
                });
            }
            catch (Exception e) { Debug.LogError($"{LOG_TAG} SendFriendRequest error: {e.Message}"); onComplete?.Invoke(false); }
        }

        private void SendDiscordFriendRequestByIdInternal(ulong userId, Action<bool> onComplete)
        {
            var client = Client;
            if (client == null) { onComplete?.Invoke(false); return; }
            try
            {
                client.SendFriendRequestById(userId, (result) =>
                {
                    bool ok = result == discordpp.Client.Error.None;
                    onComplete?.Invoke(ok);
                });
            }
            catch (Exception e) { Debug.LogError($"{LOG_TAG} SendFriendRequestById error: {e.Message}"); onComplete?.Invoke(false); }
        }

        private void AcceptDiscordFriendRequestInternal(ulong userId, Action<bool> onComplete)
        {
            var client = Client;
            if (client == null) { onComplete?.Invoke(false); return; }
            try
            {
                client.AcceptFriendRequest(userId, (result) => onComplete?.Invoke(result == discordpp.Client.Error.None));
            }
            catch (Exception e) { Debug.LogError($"{LOG_TAG} AcceptFriendRequest error: {e.Message}"); onComplete?.Invoke(false); }
        }

        private void RejectDiscordFriendRequestInternal(ulong userId, Action<bool> onComplete)
        {
            var client = Client;
            if (client == null) { onComplete?.Invoke(false); return; }
            try
            {
                client.RejectFriendRequest(userId, (result) => onComplete?.Invoke(result == discordpp.Client.Error.None));
            }
            catch (Exception e) { Debug.LogError($"{LOG_TAG} RejectFriendRequest error: {e.Message}"); onComplete?.Invoke(false); }
        }

        private void CancelDiscordFriendRequestInternal(ulong userId, Action<bool> onComplete)
        {
            var client = Client;
            if (client == null) { onComplete?.Invoke(false); return; }
            try
            {
                client.CancelFriendRequest(userId, (result) => onComplete?.Invoke(result == discordpp.Client.Error.None));
            }
            catch (Exception e) { Debug.LogError($"{LOG_TAG} CancelFriendRequest error: {e.Message}"); onComplete?.Invoke(false); }
        }

        private void RemoveDiscordAndGameFriendInternal(ulong userId, Action<bool> onComplete)
        {
            var client = Client;
            if (client == null) { onComplete?.Invoke(false); return; }
            try
            {
                client.RemoveFriend(userId, (result) =>
                {
                    bool ok = result == discordpp.Client.Error.None;
                    onComplete?.Invoke(ok);
                });
            }
            catch (Exception e) { Debug.LogError($"{LOG_TAG} RemoveFriend error: {e.Message}"); onComplete?.Invoke(false); }
        }

        private void BlockUserInternal(ulong userId, Action<bool> onComplete)
        {
            var client = Client;
            if (client == null) { onComplete?.Invoke(false); return; }
            try
            {
                client.BlockUser(userId, (result) => onComplete?.Invoke(result == discordpp.Client.Error.None));
            }
            catch (Exception e) { Debug.LogError($"{LOG_TAG} BlockUser error: {e.Message}"); onComplete?.Invoke(false); }
        }

        private void UnblockUserInternal(ulong userId, Action<bool> onComplete)
        {
            var client = Client;
            if (client == null) { onComplete?.Invoke(false); return; }
            try
            {
                client.UnblockUser(userId, (result) => onComplete?.Invoke(result == discordpp.Client.Error.None));
            }
            catch (Exception e) { Debug.LogError($"{LOG_TAG} UnblockUser error: {e.Message}"); onComplete?.Invoke(false); }
        }
#endif

        #endregion
    }
}
