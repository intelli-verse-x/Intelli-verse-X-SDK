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
        private discordpp.Client Client => IVXDiscordManager.Instance?.DiscordClient;

        private void SendDiscordInvite(string userId, string message)
        {
            var client = Client;
            if (client == null) return;
            try
            {
                if (ulong.TryParse(userId, out var uid))
                    client.SendGameInvite(uid, (result) => Debug.Log($"{LOG_TAG} Invite sent to {userId}: {result}"));
            }
            catch (Exception e) { Debug.LogError($"{LOG_TAG} SendDiscordInvite error: {e.Message}"); }
        }

        private void JoinViaSecret(string joinSecret)
        {
            var client = Client;
            if (client == null) return;
            try
            {
                client.CreateOrJoinLobby(joinSecret, (lobbyId) =>
                {
                    Debug.Log($"{LOG_TAG} Joined lobby {lobbyId} via invite secret.");
                    var discordLobby = IVXDiscordLobby.Instance;
                    if (discordLobby != null)
                        discordLobby.CreateOrJoinLobby(joinSecret);

                    var ivxLobby = IntelliVerseX.GameModes.IVXLobbyManager.Instance;
                    if (ivxLobby != null)
                        ivxLobby.JoinRoom(new IntelliVerseX.GameModes.IVXJoinRoomRequest { RoomId = joinSecret });

                    OnInviteAccepted?.Invoke(joinSecret);
                });
            }
            catch (Exception e) { Debug.LogError($"{LOG_TAG} JoinViaSecret error: {e.Message}"); }
        }

        private void ApproveDiscordJoinRequest(string requesterId)
        {
            var client = Client;
            if (client == null) return;
            try
            {
                if (ulong.TryParse(requesterId, out var uid))
                    client.AcceptGameInvite(uid, (result) => Debug.Log($"{LOG_TAG} Approved join: {result}"));
            }
            catch (Exception e) { Debug.LogError($"{LOG_TAG} ApproveJoinRequest error: {e.Message}"); }
        }

        private void DenyDiscordJoinRequest(string requesterId)
        {
            var client = Client;
            if (client == null) return;
            try
            {
                if (ulong.TryParse(requesterId, out var uid))
                    client.RejectGameInvite(uid, (result) => Debug.Log($"{LOG_TAG} Denied join: {result}"));
            }
            catch (Exception e) { Debug.LogError($"{LOG_TAG} DenyJoinRequest error: {e.Message}"); }
        }

        private void RegisterDiscordCallbacks()
        {
            var client = Client;
            if (client == null) return;
            try
            {
                client.SetActivityJoinCallback((secret) =>
                {
                    Debug.Log($"{LOG_TAG} Activity join callback: {secret}");
                    OnInviteAccepted?.Invoke(secret);
                    JoinViaSecret(secret);
                });

                client.SetActivityInviteCallback((userId, activityDetails) =>
                {
                    Debug.Log($"{LOG_TAG} Activity invite from {userId}");
                    var invite = new IVXGameInvite
                    {
                        InviterUserId = userId.ToString(),
                        InviterName = userId.ToString(),
                        JoinSecret = activityDetails,
                        ActivityDetails = activityDetails
                    };
                    OnInviteReceived?.Invoke(invite);
                });

                client.SetActivityJoinRequestCallback((userId) =>
                {
                    Debug.Log($"{LOG_TAG} Join request from {userId}");
                    var invite = new IVXGameInvite
                    {
                        InviterUserId = userId.ToString(),
                        InviterName = userId.ToString()
                    };
                    OnJoinRequested?.Invoke(invite);
                });
            }
            catch (Exception e) { Debug.LogError($"{LOG_TAG} RegisterDiscordCallbacks error: {e.Message}"); }
        }
#endif

        #endregion
    }
}
