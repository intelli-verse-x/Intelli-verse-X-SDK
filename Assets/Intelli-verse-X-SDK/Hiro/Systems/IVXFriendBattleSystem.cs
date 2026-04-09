using System;
using System.Threading.Tasks;

namespace IntelliVerseX.Hiro.Systems
{
    /// <summary>
    /// Server-authoritative friend battle / challenge system.
    /// Enables asynchronous 1v1 challenges between friends with optional wagers
    /// and server-validated score submission.
    /// </summary>
    public sealed class IVXFriendBattleSystem
    {
        private const string RPC_GET = "hiro_friend_battle_get";
        private const string RPC_SEND = "hiro_friend_battle_send";
        private const string RPC_ACCEPT = "hiro_friend_battle_accept";
        private const string RPC_SUBMIT = "hiro_friend_battle_submit";

        private readonly IVXHiroRpcClient _rpc;

        public IVXFriendBattleSystem(IVXHiroRpcClient rpc)
        {
            _rpc = rpc ?? throw new ArgumentNullException(nameof(rpc));
        }

        /// <summary>
        /// Get the friend battle state including pending challenges,
        /// active battles, and recent results.
        /// </summary>
        public async Task<IVXFriendBattleState> GetAsync()
        {
            var r = await _rpc.CallAsync<IVXFriendBattleState>(RPC_GET);
            return r.success ? r.data : new IVXFriendBattleState();
        }

        /// <summary>
        /// Send a challenge to a friend. Optionally include a wager.
        /// </summary>
        /// <param name="friendId">Target friend user ID.</param>
        /// <param name="gameMode">Game mode identifier for the challenge.</param>
        /// <param name="score">Challenger's score (set after playing).</param>
        public async Task<IVXFriendBattleSendResponse> SendChallengeAsync(
            string friendId,
            string gameMode,
            int score = 0)
        {
            var r = await _rpc.CallAsync<IVXFriendBattleSendResponse>(
                RPC_SEND,
                new { friendId, gameMode, score });
            return r.success ? r.data : null;
        }

        /// <summary>
        /// Accept a pending challenge from a friend.
        /// </summary>
        /// <param name="challengeId">The challenge to accept.</param>
        public async Task<IVXFriendBattleSendResponse> AcceptChallengeAsync(string challengeId)
        {
            var r = await _rpc.CallAsync<IVXFriendBattleSendResponse>(
                RPC_ACCEPT,
                new { challengeId });
            return r.success ? r.data : null;
        }

        /// <summary>
        /// Submit a score for an active battle. The server determines the winner
        /// when both scores are submitted and distributes rewards accordingly.
        /// </summary>
        /// <param name="challengeId">The active battle.</param>
        /// <param name="score">The player's score.</param>
        public async Task<IVXFriendBattleSubmitResponse> SubmitScoreAsync(string challengeId, int score)
        {
            var r = await _rpc.CallAsync<IVXFriendBattleSubmitResponse>(
                RPC_SUBMIT,
                new { challengeId, score });
            return r.success ? r.data : null;
        }
    }
}
