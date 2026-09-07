using System;
using System.Threading.Tasks;

namespace IntelliVerseX.Hiro.Systems
{
    /// <summary>
    /// Server-authoritative friend quest system.
    /// Enables cooperative quests between friends with shared progress tracking
    /// and mutual rewards upon completion.
    /// </summary>
    public sealed class IVXFriendQuestSystem
    {
        private const string RPC_GET = "hiro_friend_quest_get";
        private const string RPC_ACCEPT = "hiro_friend_quest_accept";
        private const string RPC_PROGRESS = "hiro_friend_quest_progress";

        private readonly IVXHiroRpcClient _rpc;

        public IVXFriendQuestSystem(IVXHiroRpcClient rpc)
        {
            _rpc = rpc ?? throw new ArgumentNullException(nameof(rpc));
        }

        /// <summary>
        /// Get the friend quest state including active, available, and completed quests.
        /// </summary>
        public async Task<IVXFriendQuestState> GetAsync()
        {
            var r = await _rpc.CallAsync<IVXFriendQuestState>(RPC_GET);
            return r.success ? r.data : new IVXFriendQuestState();
        }

        /// <summary>
        /// Accept a friend quest with a specific partner.
        /// Both players must accept before progress tracking begins.
        /// </summary>
        /// <param name="questId">The quest to accept.</param>
        /// <param name="partnerId">The friend to partner with.</param>
        public async Task<IVXFriendQuestAcceptResponse> AcceptAsync(string questId, string partnerId)
        {
            var r = await _rpc.CallAsync<IVXFriendQuestAcceptResponse>(
                RPC_ACCEPT,
                new { questId, partnerId });
            return r.success ? r.data : null;
        }

        /// <summary>
        /// Report progress on an active friend quest. The server aggregates both
        /// players' contributions and awards rewards on completion.
        /// </summary>
        /// <param name="questId">The active quest.</param>
        /// <param name="amount">Progress increment.</param>
        public async Task<IVXFriendQuestProgressResponse> ReportProgressAsync(string questId, int amount)
        {
            var r = await _rpc.CallAsync<IVXFriendQuestProgressResponse>(
                RPC_PROGRESS,
                new { questId, amount });
            return r.success ? r.data : null;
        }
    }
}
