// ============================================================================
// IVXFriendsCache.cs - Local cache for friends list
// ============================================================================

using System;
using System.Collections.Generic;
using Nakama;

namespace IntelliVerseX.Social
{
    /// <summary>
    /// Local cache for friends. Updated by IVXFriendsManager on RefreshFriendsAsync.
    /// </summary>
    public class IVXFriendsCache
    {
        private readonly Dictionary<string, IApiFriend> _friends = new Dictionary<string, IApiFriend>();

        public IReadOnlyDictionary<string, IApiFriend> Friends => _friends;

        public void Update(IEnumerable<IApiFriend> list)
        {
            _friends.Clear();
            if (list == null) return;
            foreach (var f in list)
            {
                if (f?.User?.Id != null)
                    _friends[f.User.Id] = f;
            }
        }

        public IApiFriend Get(string userId)
        {
            return userId != null && _friends.TryGetValue(userId, out var f) ? f : null;
        }

        public bool Contains(string userId) => userId != null && _friends.ContainsKey(userId);

        public int Count => _friends.Count;
    }
}
