using System;
using UnityEngine;

namespace IntelliVerseX.Discord
{
    /// <summary>
    /// Represents an incoming game invite from Discord.
    /// </summary>
    [Serializable]
    public sealed class IVXGameInvite
    {
        /// <summary>Discord user ID of the inviter.</summary>
        public string InviterUserId;
        /// <summary>Display name of the inviter.</summary>
        public string InviterName;
        /// <summary>Avatar URL of the inviter.</summary>
        public string InviterAvatarUrl;
        /// <summary>The join secret for the session/lobby.</summary>
        public string JoinSecret;
        /// <summary>Activity details from the inviter's Rich Presence.</summary>
        public string ActivityDetails;
        /// <summary>Party size info from the inviter.</summary>
        public int PartyCurrentSize;
        /// <summary>Party max size from the inviter.</summary>
        public int PartyMaxSize;
    }

    /// <summary>
    /// Manages Discord game invites — both sending invites to friends
    /// and handling incoming join requests. Integrates with
    /// <see cref="IVXDiscordPresence"/> join secrets and
    /// <see cref="IVXDiscordLobby"/> for automatic lobby joining.
    /// </summary>
    public sealed class IVXDiscordInvites : MonoBehaviour
    {
        #region Constants

        private const string LOG_TAG = "[IVXDiscordInvites]";

        #endregion

        #region Private Fields

        private static IVXDiscordInvites _instance;

        #endregion

        #region Properties

        /// <summary>Singleton instance.</summary>
        public static IVXDiscordInvites Instance => _instance;

        #endregion

        #region Events

        /// <summary>
        /// Fired when an invite is received from a Discord friend.
        /// The game should show a UI prompt and call AcceptInvite or DeclineInvite.
        /// </summary>
        public event Action<IVXGameInvite> OnInviteReceived;

        /// <summary>
        /// Fired when an "Ask to Join" request is received.
        /// Another Discord user wants to join the local player's session.
        /// The game should call ApproveJoinRequest or DenyJoinRequest.
        /// </summary>
        public event Action<string, string> OnJoinRequested;

        /// <summary>
        /// Fired when an invite is accepted and the player should
        /// transition to the invited session.
        /// </summary>
        public event Action<string> OnInviteAccepted;

        /// <summary>
        /// Fired when an invite was sent successfully. Provides target user ID.
        /// </summary>
        public event Action<string> OnInviteSent;

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
        /// Send a game invite to a Discord friend.
        /// The friend will see a notification with an "Accept" button.
        /// Requires Rich Presence with a join secret set.
        /// </summary>
        /// <param name="discordUserId">Target Discord user ID.</param>
        /// <param name="message">Optional invite message.</param>
        public void SendInvite(string discordUserId, string message = null)
        {
            if (string.IsNullOrEmpty(discordUserId))
            {
                Debug.LogError($"{LOG_TAG} Cannot send invite: userId is null.");
                return;
            }

            Debug.Log($"{LOG_TAG} Sending invite to {discordUserId}...");

#if INTELLIVERSEX_HAS_DISCORD
            SendDiscordInvite(discordUserId, message);
#else
            Debug.Log($"{LOG_TAG} [Stub] Invite sent to {discordUserId}.");
            OnInviteSent?.Invoke(discordUserId);
#endif
        }

        /// <summary>
        /// Accept an incoming game invite. Joins the inviter's session.
        /// </summary>
        /// <param name="invite">The invite to accept.</param>
        public void AcceptInvite(IVXGameInvite invite)
        {
            if (invite == null)
            {
                Debug.LogError($"{LOG_TAG} Cannot accept null invite.");
                return;
            }

            Debug.Log($"{LOG_TAG} Accepting invite from {invite.InviterName}...");

#if INTELLIVERSEX_HAS_DISCORD
            JoinViaSecret(invite.JoinSecret);
#else
            Debug.Log($"{LOG_TAG} [Stub] Accepted invite. Join secret: {invite.JoinSecret}");
            OnInviteAccepted?.Invoke(invite.JoinSecret);
#endif
        }

        /// <summary>
        /// Decline an incoming game invite.
        /// </summary>
        /// <param name="invite">The invite to decline.</param>
        public void DeclineInvite(IVXGameInvite invite)
        {
            Debug.Log($"{LOG_TAG} Declined invite from {invite?.InviterName}.");
        }

        /// <summary>
        /// Approve an "Ask to Join" request from another Discord user.
        /// </summary>
        /// <param name="requesterId">Discord user ID of the requester.</param>
        public void ApproveJoinRequest(string requesterId)
        {
            Debug.Log($"{LOG_TAG} Approved join request from {requesterId}.");
#if INTELLIVERSEX_HAS_DISCORD
            ApproveDiscordJoinRequest(requesterId);
#endif
        }

        /// <summary>
        /// Deny an "Ask to Join" request from another Discord user.
        /// </summary>
        /// <param name="requesterId">Discord user ID of the requester.</param>
        public void DenyJoinRequest(string requesterId)
        {
            Debug.Log($"{LOG_TAG} Denied join request from {requesterId}.");
#if INTELLIVERSEX_HAS_DISCORD
            DenyDiscordJoinRequest(requesterId);
#endif
        }

        /// <summary>
        /// Register this component to listen for Discord invite/join callbacks.
        /// Called automatically during initialization, but can be re-called
        /// if callbacks need to be re-registered.
        /// </summary>
        public void RegisterCallbacks()
        {
#if INTELLIVERSEX_HAS_DISCORD
            RegisterDiscordCallbacks();
#else
            Debug.Log($"{LOG_TAG} [Stub] Invite callbacks registered.");
#endif
        }

        #endregion

        #region Private Methods

#if INTELLIVERSEX_HAS_DISCORD
        private void SendDiscordInvite(string userId, string message)
        {
            // Wire to: client->SendGameInvite(userId, callback)
            // Or use Rich Presence party/join secret for passive invites
        }

        private void JoinViaSecret(string joinSecret)
        {
            // Use the join secret to:
            // 1. Join the Discord lobby: client->CreateOrJoinLobby(joinSecret)
            // 2. Bridge to IVX lobby: IVXLobbyManager.Instance.JoinRoom(roomId)
            // 3. Optionally auto-join voice
        }

        private void ApproveDiscordJoinRequest(string requesterId)
        {
            // Wire to: client->AcceptGameInvite or respond to ActivityJoinRequest
        }

        private void DenyDiscordJoinRequest(string requesterId)
        {
            // Wire to: client->RejectGameInvite or respond to ActivityJoinRequest
        }

        private void RegisterDiscordCallbacks()
        {
            // Wire to:
            // client->SetActivityJoinCallback → OnInviteAccepted
            // client->SetActivityInviteCallback → OnInviteReceived
            // client->SetActivityJoinRequestCallback → OnJoinRequested
        }
#endif

        #endregion
    }
}
