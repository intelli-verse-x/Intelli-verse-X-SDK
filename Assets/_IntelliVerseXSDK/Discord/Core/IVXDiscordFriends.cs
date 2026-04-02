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
        private bool _loaded;

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
                CanInvite = true
            });
            _friends.Add(new IVXUnifiedFriend
            {
                Source = IVXFriendSource.Discord,
                DisplayName = "DiscordOnlyFriend",
                DiscordUserId = "stub_222",
                IsOnline = true,
                IsInGame = false,
                ActivityText = "Online on Discord",
                CanInvite = false
            });
            _friends.Add(new IVXUnifiedFriend
            {
                Source = IVXFriendSource.Game,
                DisplayName = "GameOnlyFriend",
                GameUserId = "nakama_333",
                IsOnline = false,
                IsInGame = false,
                CanInvite = false
            });
            _loaded = true;
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

        #endregion

        #region Private Methods

#if INTELLIVERSEX_HAS_DISCORD
        private void FetchAndMerge()
        {
            // 1. Fetch Discord relationships via client->GetRelationships()
            // 2. Fetch Nakama friends via IVXNakamaManager
            // 3. Merge by matching Discord userId ↔ Nakama custom metadata
            // 4. Set Source = Both for matched, Discord for Discord-only, Game for Nakama-only
            // 5. Populate IsOnline, IsInGame, ActivityText from Discord presence
            // 6. Fire OnFriendsUpdated
        }
#endif

        #endregion
    }
}
