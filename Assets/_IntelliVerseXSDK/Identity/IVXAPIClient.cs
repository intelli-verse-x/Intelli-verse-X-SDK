using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace IntelliVerseX.Identity
{
    /// <summary>
    /// Main API client for IntelliVerseX SDK.
    /// Provides authentication, user management, and backend API access.
    /// 
    /// This is the recommended entry point for all API operations.
    /// </summary>
    /// <example>
    /// <code>
    /// // Login
    /// var result = await IVXAPIClient.LoginAsync("email@example.com", "password");
    /// if (result.status)
    /// {
    ///     Debug.Log($"Welcome {result.data.user.firstName}!");
    /// }
    /// 
    /// // Guest login
    /// var guestResult = await IVXAPIClient.GuestSignupAsync();
    /// 
    /// // Signup Step 1: Initiate with email/password
    /// var otpResult = await IVXAPIClient.SignupInitiateAsync("email@example.com", "password");
    /// 
    /// // Signup Step 2: Confirm with OTP and user details
    /// var signupResult = await IVXAPIClient.SignupConfirmAsync(
    ///     "email@example.com", "123456", "password", "myusername", "John", "Doe");
    /// </code>
    /// </example>
    public static class IVXAPIClient
    {
        #region Configuration

        /// <summary>
        /// Enable or disable debug logging for API calls.
        /// </summary>
        public static bool DebugLogs
        {
            get => APIManager.DebugLogs;
            set => APIManager.DebugLogs = value;
        }

        /// <summary>
        /// Enable verbose API logging (shows full request/response payloads).
        /// WARNING: May expose sensitive data. Use only for debugging.
        /// </summary>
        public static bool VerboseLogging
        {
            get => APIManager.VerboseAPILogging;
            set => APIManager.VerboseAPILogging = value;
        }

        /// <summary>
        /// Request timeout in seconds.
        /// </summary>
        public static int RequestTimeout
        {
            get => APIManager.RequestTimeoutSeconds;
            set => APIManager.RequestTimeoutSeconds = value;
        }

        /// <summary>
        /// Maximum retry attempts for failed requests.
        /// </summary>
        public static int MaxRetries
        {
            get => APIManager.MaxRetries;
            set => APIManager.MaxRetries = value;
        }

        #endregion

        #region Authentication

        /// <summary>
        /// Login with email and password.
        /// </summary>
        /// <param name="email">User's email address</param>
        /// <param name="password">User's password</param>
        /// <param name="persistSession">Whether to save the session to disk</param>
        /// <param name="ct">Cancellation token</param>
        /// <returns>Login response with user data and tokens</returns>
        public static async Task<APIManager.LoginResponse> LoginAsync(
            string email,
            string password,
            bool persistSession = true,
            CancellationToken ct = default)
        {
            var request = new APIManager.LoginRequest
            {
                email = email,
                password = password,
                fromDevice = "unity"
            };

            return await APIManager.LoginAsync(request, configureUserAuthOnSuccess: true, persistSession: persistSession, ct: ct);
        }

        /// <summary>
        /// Login with a full LoginRequest object for advanced options.
        /// </summary>
        public static async Task<APIManager.LoginResponse> LoginAsync(
            APIManager.LoginRequest request,
            bool persistSession = true,
            CancellationToken ct = default)
        {
            return await APIManager.LoginAsync(request, configureUserAuthOnSuccess: true, persistSession: persistSession, ct: ct);
        }

        /// <summary>
        /// Create a guest account (no email/password required).
        /// </summary>
        /// <param name="role">User role (default: "user")</param>
        /// <param name="persistSession">Whether to save the session to disk</param>
        /// <param name="ct">Cancellation token</param>
        /// <returns>Guest signup response with user data and tokens</returns>
        public static async Task<APIManager.GuestSignupResponse> GuestSignupAsync(
            string role = "user",
            bool persistSession = true,
            CancellationToken ct = default)
        {
            return await APIManager.GuestSignupAsync(
                role: role,
                configureUserAuthOnSuccess: true,
                persistSession: persistSession,
                ct: ct);
        }

        /// <summary>
        /// Initiate signup process (sends OTP to email).
        /// </summary>
        /// <param name="email">User's email address</param>
        /// <param name="password">User's password</param>
        /// <param name="role">User role (default: "user")</param>
        /// <param name="ct">Cancellation token</param>
        /// <returns>Response indicating if OTP was sent</returns>
        public static async Task<APIManager.SignupInitiateResponse> SignupInitiateAsync(
            string email,
            string password,
            string role = "user",
            CancellationToken ct = default)
        {
            return await APIManager.SignupInitiateAsync(email, password, role, ct);
        }

        /// <summary>
        /// Confirm signup with OTP code.
        /// </summary>
        /// <param name="email">User's email address</param>
        /// <param name="otp">OTP code received via email</param>
        /// <param name="password">User's password (same as used in SignupInitiateAsync)</param>
        /// <param name="userName">Desired username (3-32 chars, letters/digits/._-)</param>
        /// <param name="firstName">User's first name (optional)</param>
        /// <param name="lastName">User's last name (optional)</param>
        /// <param name="referralCode">Referral code if any (optional)</param>
        /// <param name="configureUserAuthOnSuccess">Auto-configure user auth on success</param>
        /// <param name="ct">Cancellation token</param>
        /// <returns>Signup confirmation response with user data</returns>
        public static async Task<APIManager.SignupConfirmResponse> SignupConfirmAsync(
            string email,
            string otp,
            string password,
            string userName,
            string firstName = null,
            string lastName = null,
            string referralCode = null,
            bool configureUserAuthOnSuccess = true,
            CancellationToken ct = default)
        {
            var request = new APIManager.SignupConfirmRequest
            {
                email = email,
                otp = otp,
                password = password,
                userName = userName,
                firstName = firstName,
                lastName = lastName,
                referralCode = referralCode,
                role = "user",
                fromDevice = "unity"
            };

            return await APIManager.SignupConfirmAsync(request, configureUserAuthOnSuccess, ct);
        }

        /// <summary>
        /// Login with social provider (Google, Apple, Facebook).
        /// </summary>
        /// <param name="loginType">Provider name: "google", "apple", or "facebook"</param>
        /// <param name="email">User's email from the social provider</param>
        /// <param name="firstName">User's first name (optional)</param>
        /// <param name="lastName">User's last name (optional)</param>
        /// <param name="userName">Username (optional)</param>
        /// <param name="appleKey">Apple key/identifier (required for Apple sign-in, null otherwise)</param>
        /// <param name="persistSession">Whether to save the session to disk</param>
        /// <param name="ct">Cancellation token</param>
        /// <returns>Social login response with user data and tokens</returns>
        public static async Task<APIManager.SocialLoginResponse> SocialLoginAsync(
            string loginType,
            string email,
            string firstName = null,
            string lastName = null,
            string userName = null,
            string appleKey = null,
            bool persistSession = true,
            CancellationToken ct = default)
        {
            return await APIManager.SocialLoginAsync(
                loginType: loginType,
                email: email,
                firstName: firstName,
                lastName: lastName,
                userName: userName,
                password: null,
                role: "user",
                fcmToken: null,
                appleKey: appleKey,
                configureUserAuthOnSuccess: true,
                persistSession: persistSession,
                ct: ct);
        }

        /// <summary>
        /// Logout and clear the current session.
        /// </summary>
        public static void Logout()
        {
            UserSessionManager.Clear();
            Debug.Log("[IVXAPIClient] User logged out");
        }

        /// <summary>
        /// Check if user is currently logged in with a valid session.
        /// </summary>
        public static bool IsLoggedIn => UserSessionManager.HasSession && UserSessionManager.IsAccessTokenFresh();

        /// <summary>
        /// Get the current user's session data.
        /// </summary>
        public static UserSessionManager.UserSession CurrentSession => UserSessionManager.Current;

        #endregion

        #region Session Management

        /// <summary>
        /// Try to restore authentication from a saved session.
        /// Call this at app startup to restore the previous login.
        /// </summary>
        /// <returns>True if a valid session was restored</returns>
        public static bool TryRestoreSession()
        {
            return APIManager.TryConfigureUserAuthFromSavedSession(enable: true);
        }

        /// <summary>
        /// Refresh the access token if needed.
        /// This is called automatically by API methods, but can be called manually.
        /// </summary>
        public static async Task<bool> RefreshTokenAsync(CancellationToken ct = default)
        {
            try
            {
                await APIManager.RefreshUserTokenAsync(ct);
                return true;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[IVXAPIClient] Token refresh failed: {ex.Message}");
                return false;
            }
        }

        #endregion

        #region User Management

        /// <summary>
        /// Get the current user's referral URL for inviting friends.
        /// </summary>
        /// <returns>ReferralUrlResponse containing the referral code and URL</returns>
        public static async Task<APIManager.ReferralUrlResponse> GetReferralUrlAsync(CancellationToken ct = default)
        {
            return await APIManager.GetReferralUrlAsync(null, ct);
        }

        /// <summary>
        /// Get referral statistics (how many users invited, rewards earned).
        /// </summary>
        public static async Task<APIManager.ReferralStatsResponse> GetReferralStatsAsync(CancellationToken ct = default)
        {
            return await APIManager.GetReferralStatsAsync(null, ct);
        }

        #endregion

        #region Friend Management

        /// <summary>
        /// Get the user's friend list.
        /// </summary>
        /// <param name="status">Filter by status: "accepted", "pending", etc.</param>
        /// <param name="query">Search query</param>
        /// <param name="ct">Cancellation token</param>
        /// <returns>List of friend data</returns>
        public static async Task<System.Collections.Generic.List<IVXModels.FriendData>> GetFriendsAsync(
            string status = "accepted",
            string query = null,
            CancellationToken ct = default)
        {
            string userId = UserSessionManager.Current?.userId;
            return await APIManager.GetFriendsAsync(userId, status, query, ct);
        }

        /// <summary>
        /// Get pending incoming friend requests.
        /// </summary>
        /// <param name="ct">Cancellation token</param>
        /// <returns>List of pending friend requests</returns>
        public static Task<System.Collections.Generic.List<IVXModels.FriendData>> GetPendingFriendRequestsAsync(
            CancellationToken ct = default)
        {
            return APIManager.GetIncomingRequestsAsync(ct);
        }

        /// <summary>
        /// Get accepted friends list.
        /// </summary>
        /// <param name="ct">Cancellation token</param>
        /// <returns>List of accepted friends</returns>
        public static Task<System.Collections.Generic.List<IVXModels.FriendData>> GetAcceptedFriendsAsync(
            CancellationToken ct = default)
        {
            return APIManager.GetAcceptedFriendsAsync(ct);
        }

        /// <summary>
        /// Search for users to add as friends.
        /// </summary>
        /// <param name="searchQuery">Search query (username, email, etc.)</param>
        /// <param name="ct">Cancellation token</param>
        /// <returns>List of matching users</returns>
        public static async Task<System.Collections.Generic.List<IVXModels.SearchUser>> SearchFriendsAsync(
            string searchQuery,
            CancellationToken ct = default)
        {
            return await APIManager.SearchFriendsAsync(searchQuery, ct);
        }

        /// <summary>
        /// Send a friend request (invite).
        /// </summary>
        /// <param name="receiverUserId">User ID of the person to add</param>
        /// <param name="ct">Cancellation token</param>
        /// <returns>True if successful</returns>
        public static async Task<bool> SendFriendRequestAsync(
            string receiverUserId,
            CancellationToken ct = default)
        {
            return await APIManager.SendFriendInviteAsync(receiverUserId, ct);
        }

        /// <summary>
        /// Accept a friend request.
        /// </summary>
        /// <param name="relationId">Relation ID of the friend request</param>
        /// <param name="ct">Cancellation token</param>
        /// <returns>True if successful</returns>
        public static async Task<bool> AcceptFriendRequestAsync(
            string relationId,
            CancellationToken ct = default)
        {
            return await APIManager.AcceptFriendRequestAsync(relationId, ct);
        }

        /// <summary>
        /// Reject a friend request.
        /// </summary>
        /// <param name="relationId">Relation ID of the friend request</param>
        /// <param name="ct">Cancellation token</param>
        /// <returns>True if successful</returns>
        public static async Task<bool> RejectFriendRequestAsync(
            string relationId,
            CancellationToken ct = default)
        {
            return await APIManager.RejectFriendRequestAsync(relationId, ct);
        }

        /// <summary>
        /// Remove a friend (cancel friendship).
        /// </summary>
        /// <param name="relationId">Relation ID of the friend to remove</param>
        /// <param name="ct">Cancellation token</param>
        /// <returns>True if successful</returns>
        public static async Task<bool> RemoveFriendAsync(
            string relationId,
            CancellationToken ct = default)
        {
            return await APIManager.RemoveFriendAsync(relationId, ct);
        }

        #endregion

        #region Wallet

        /// <summary>
        /// Get the user's wallet balance.
        /// Note: Wallet APIs may require additional backend configuration.
        /// </summary>
        /// <param name="ct">Cancellation token</param>
        /// <returns>Wallet balance response</returns>
        public static async Task<IVXModels.WalletBalanceResponse> GetWalletBalanceAsync(
            CancellationToken ct = default)
        {
            string walletAddress = UserSessionManager.Current?.walletAddress;
            if (string.IsNullOrEmpty(walletAddress))
            {
                throw new InvalidOperationException("User must be logged in with a wallet address to get wallet balance");
            }
            
            // TODO: Implement when wallet API is available in APIManager
            // For now, return a placeholder response
            Debug.LogWarning("[IVXAPIClient] GetWalletBalanceAsync: Wallet API not yet implemented in APIManager. Returning empty response.");
            return await Task.FromResult(new IVXModels.WalletBalanceResponse
            {
                success = false,
                message = "Wallet API not yet implemented"
            });
        }

        #endregion
    }
}
