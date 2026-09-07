using System;
using System.Threading.Tasks;

namespace IntelliVerseX.Hiro.Systems
{
    /// <summary>
    /// Friend challenges on prod Nakama:
    /// <c>send_friend_challenge</c>, <c>accept_friend_challenge</c>,
    /// <c>decline_friend_challenge</c>, <c>cancel_friend_challenge</c>,
    /// <c>list_pending_friend_challenges</c>.
    /// </summary>
    public sealed class IVXFriendBattleSystem
    {
        private const string RPC_LIST = "list_pending_friend_challenges";
        private const string RPC_SEND = "send_friend_challenge";
        private const string RPC_ACCEPT = "accept_friend_challenge";
        private const string RPC_DECLINE = "decline_friend_challenge";
        private const string RPC_CANCEL = "cancel_friend_challenge";

        private readonly IVXHiroRpcClient _rpc;

        public IVXFriendBattleSystem(IVXHiroRpcClient rpc)
        {
            _rpc = rpc ?? throw new ArgumentNullException(nameof(rpc));
        }

        /// <summary>List pending challenges for the authenticated user.</summary>
        public async Task<IVXFriendBattleState> GetAsync()
        {
            var r = await _rpc.CallAsync<IVXFriendBattleState>(RPC_LIST);
            return r.success ? (r.data ?? new IVXFriendBattleState()) : new IVXFriendBattleState();
        }

        /// <summary>Send a challenge to a friend.</summary>
        public async Task<IVXFriendBattleSendResponse> SendChallengeAsync(
            string friendId,
            string gameId,
            string gameMode = null,
            int score = 0)
        {
            if (string.IsNullOrWhiteSpace(friendId))
                throw new ArgumentException("friendId is required", nameof(friendId));
            if (string.IsNullOrWhiteSpace(gameId))
                throw new ArgumentException("gameId is required", nameof(gameId));

            var r = await _rpc.CallAsync<IVXFriendBattleSendResponse>(
                RPC_SEND,
                new
                {
                    friendUserId = friendId,
                    gameId,
                    challengeData = new { modeName = gameMode, score, isAsync = true }
                });
            return r.success ? r.data : null;
        }

        /// <summary>Accept a pending challenge.</summary>
        public async Task<IVXFriendBattleSendResponse> AcceptChallengeAsync(string challengeId)
        {
            var r = await _rpc.CallAsync<IVXFriendBattleSendResponse>(
                RPC_ACCEPT,
                new { challengeId });
            return r.success ? r.data : null;
        }

        /// <summary>Decline a pending challenge.</summary>
        public async Task<bool> DeclineChallengeAsync(string challengeId)
        {
            var r = await _rpc.CallAsync<object>(RPC_DECLINE, new { challengeId });
            return r.success;
        }

        /// <summary>Cancel a challenge you sent.</summary>
        public async Task<bool> CancelChallengeAsync(string challengeId)
        {
            var r = await _rpc.CallAsync<object>(RPC_CANCEL, new { challengeId });
            return r.success;
        }
    }
}
