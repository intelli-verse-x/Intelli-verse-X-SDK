using System;
using System.Threading.Tasks;

namespace IntelliVerseX.Hiro.Systems
{
    /// <summary>
    /// Friend quests on prod Nakama: <c>friend_quest_get_state</c>, <c>friend_quest_complete</c>.
    /// </summary>
    public sealed class IVXFriendQuestSystem
    {
        private const string RPC_GET = "friend_quest_get_state";
        private const string RPC_COMPLETE = "friend_quest_complete";

        private readonly IVXHiroRpcClient _rpc;

        public IVXFriendQuestSystem(IVXHiroRpcClient rpc)
        {
            _rpc = rpc ?? throw new ArgumentNullException(nameof(rpc));
        }

        /// <summary>Get friend quest state (prod field: <c>quests</c>).</summary>
        public async Task<IVXFriendQuestState> GetAsync()
        {
            var r = await _rpc.CallAsync<IVXFriendQuestState>(RPC_GET);
            if (!r.success || r.data == null)
                return new IVXFriendQuestState();

            if (r.data.quests == null)
                r.data.quests = new System.Collections.Generic.List<IVXFriendQuest>();

            return r.data;
        }

        /// <summary>Mark a friend quest complete.</summary>
        public async Task<IVXFriendQuestProgressResponse> CompleteAsync(string questId)
        {
            var r = await _rpc.CallAsync<IVXFriendQuestProgressResponse>(
                RPC_COMPLETE,
                new { questId, quest_id = questId });
            return r.success ? r.data : null;
        }
    }
}
