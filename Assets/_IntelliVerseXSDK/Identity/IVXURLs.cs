// ============================================================================
// IVXURLs.cs - IntelliVerse-X Centralized API Endpoints
// ============================================================================
// PRODUCTION-READY | HIGH-PERFORMANCE | ENTERPRISE-GRADE
// 
// All API endpoints for the IntelliVerse-X platform.
// Single source of truth - no duplicates, no hardcoded URLs elsewhere.
// ============================================================================

using UnityEngine.Networking;

/// <summary>
/// Centralized API endpoints for IntelliVerse-X platform.
/// Static class - zero allocations, instant access.
/// 
/// Architecture:
/// - All URLs are readonly strings (immutable, thread-safe)
/// - Dynamic URLs use static methods with minimal string concat
/// - Organized by feature domain for easy navigation
/// </summary>
public static class IVXURLs
{
    // ========================================================================
    // BASE CONFIGURATION
    // ========================================================================
    
    /// <summary>Main API gateway</summary>
    public const string BaseUrl = "https://api.intelli-verse-x.ai/";
    
    /// <summary>AI services gateway</summary>
    public const string AIBaseUrl = "https://ai.intelli-verse-x.ai/";
    
    /// <summary>Payment services gateway</summary>
    public const string PaymentBaseUrl = "https://payment.intelli-verse-x.ai/";
    
    /// <summary>Default Game ID for QuizVerse</summary>
    public const string GameId = "a6bde9e8-ebc5-4c7b-9254-02e9c0e02d74";
    
    // ========================================================================
    // AUTHENTICATION - LOGIN
    // ========================================================================
    
    #region Login
    
    /// <summary>Standard email/password login</summary>
    public const string Login = BaseUrl + "api/user/auth/login";
    
    /// <summary>Social provider login (Google, Apple, Facebook)</summary>
    public const string SocialLogin = BaseUrl + "api/user/auth/social-login";
    
    /// <summary>Login V2 - Enhanced authentication</summary>
    public const string Login_V2 = BaseUrl + "api/user/auth_v_2/login";
    
    /// <summary>Social login V2 - Game-specific</summary>
    public const string SocialLogin_V2 = BaseUrl + "api/user/auth-v2/social/game-login";
    
    #endregion
    
    // ========================================================================
    // AUTHENTICATION - REGISTRATION
    // ========================================================================
    
    #region Registration
    
    /// <summary>Legacy registration (no OTP)</summary>
    public const string Register = BaseUrl + "auth/register";
    
    /// <summary>Standard signup</summary>
    public const string SignupUrl = BaseUrl + "api/user/auth/signup";
    
    /// <summary>Signup V2 - Game-specific</summary>
    public const string SignupUrl_V2 = BaseUrl + "api/user/auth_v_2/game-signup";
    
    /// <summary>Initiate registration with OTP</summary>
    public const string InitRegister = BaseUrl + "api/user/auth_v_2/signup/initiate";
    
    /// <summary>Complete signup with OTP verification</summary>
    public const string SignupOTP = BaseUrl + "api/user/auth_v_2/signup";
    
    #endregion
    
    // ========================================================================
    // AUTHENTICATION - PASSWORD MANAGEMENT
    // ========================================================================
    
    #region Password Management
    
    /// <summary>Initiate forgot password flow</summary>
    public const string ForgotPasswordUrl = BaseUrl + "api/user/auth/forgot-password";
    
    /// <summary>Reset password with code</summary>
    public const string ResetPasswordUrl = BaseUrl + "api/user/auth/reset-password";
    
    /// <summary>Forgot password V2</summary>
    public const string ForgotPasswordUrl_V2 = BaseUrl + "api/user/auth_v_2/forgot-password";
    
    /// <summary>Reset password V2</summary>
    public const string ResetPasswordUrl_V2 = BaseUrl + "api/user/auth_v_2/reset-password";
    
    /// <summary>Resend email verification</summary>
    public const string ResendEmailVerification_V2 = BaseUrl + "api/user/auth_v_2/resend-email-verification";
    
    #endregion
    
    // ========================================================================
    // AUTHENTICATION - TOKEN MANAGEMENT
    // ========================================================================
    
    #region Token Management
    
    /// <summary>Refresh access token</summary>
    public const string RefreshToken = BaseUrl + "api/user/auth-v2/token/refresh";
    
    /// <summary>OAuth token endpoint (admin operations)</summary>
    public const string OAuthToken = BaseUrl + "api/admin/oauth/token";
    
    /// <summary>Get refresh token URL with IDP username</summary>
    public static string GetRefreshTokenUrl(string idpUsername) =>
        $"{BaseUrl}api/user/auth/refresh-token?idp-username={UnityWebRequest.EscapeURL(idpUsername ?? string.Empty)}";
    
    #endregion
    
    // ========================================================================
    // GUEST AUTHENTICATION
    // ========================================================================
    
    #region Guest Flow
    
    /// <summary>Guest login (legacy)</summary>
    public const string GuestLogin = BaseUrl + "api/user/auth/guest-login";
    
    /// <summary>Convert guest to full account (legacy)</summary>
    public const string ConvertGuest = BaseUrl + "api/user/auth/convert-guest";
    
    /// <summary>Guest signup V2</summary>
    public const string GuestLogin_V2 = BaseUrl + "api/user/auth_v_2/guest-signup";
    
    /// <summary>Convert guest V2 (without OTP)</summary>
    public const string ConvertGuest_V2 = BaseUrl + "api/user/auth_v_2/convert-guest";
    
    /// <summary>Initiate guest conversion with OTP</summary>
    public const string ConvertGuest_V2_Init = BaseUrl + "api/user/auth_v_2/guest/convert/initiate";
    
    /// <summary>Confirm guest conversion with OTP</summary>
    public const string ConvertGuest_V2_Confirm = BaseUrl + "api/user/auth_v_2/guest/convert/confirm";
    
    #endregion
    
    // ========================================================================
    // USER PROFILE
    // ========================================================================
    
    #region User Profile
    
    /// <summary>Get current user profile</summary>
    public const string GetUserProfile = BaseUrl + "api/user/auth/me";
    
    /// <summary>Change user password</summary>
    public const string ChangePassword = BaseUrl + "api/user/user/change-password";
    
    /// <summary>Change profile picture</summary>
    public const string ChangeProfilePic = BaseUrl + "api/user/user/change-profile-pic";
    
    /// <summary>Update user profile</summary>
    public const string UpdateUserProfile = BaseUrl + "api/user/user/profile";
    
    #endregion
    
    // ========================================================================
    // FRIENDS SYSTEM
    // ========================================================================
    
    #region Friends
    
    /// <summary>Get friends list (accepts userId, status, query params)</summary>
    public const string GetFriendList = BaseUrl + "api/games/friends";
    
    /// <summary>Search for users to add as friends</summary>
    public const string SearchFriend = BaseUrl + "api/games/friends/search";
    
    /// <summary>Send friend request/invite</summary>
    public const string SendFriendRequest = BaseUrl + "api/games/friends/invite";
    
    /// <summary>Send gift to a friend</summary>
    public const string SendGiftToFriend = BaseUrl + "api/games/friends/send-gift";
    
    /// <summary>Update friend relationship status (accept/reject/block)</summary>
    public const string UpdateFriendStatus = BaseUrl + "api/games/friends/status";
    
    /// <summary>
    /// Build friends list URL with query parameters.
    /// </summary>
    public static string GetFriendsUrl(string userId, string status = null, string query = null)
    {
        var url = $"{GetFriendList}?userId={UnityWebRequest.EscapeURL(userId)}";
        if (!string.IsNullOrWhiteSpace(status))
            url += $"&status={UnityWebRequest.EscapeURL(status)}";
        if (!string.IsNullOrWhiteSpace(query))
            url += $"&query={UnityWebRequest.EscapeURL(query)}";
        return url;
    }
    
    /// <summary>
    /// Build search friends URL.
    /// </summary>
    public static string GetSearchFriendsUrl(string searchQuery, string userId) =>
        $"{SearchFriend}?search={UnityWebRequest.EscapeURL(searchQuery.Trim())}&userId={UnityWebRequest.EscapeURL(userId)}";
    
    #endregion
    
    // ========================================================================
    // LEADERBOARD
    // ========================================================================
    
    #region Leaderboard
    
    /// <summary>Get leaderboard entries</summary>
    public const string GetLeaderboard = BaseUrl + "api/games/leaderboard";
    
    /// <summary>Get top N leaderboard entries</summary>
    public const string GetTopLeaderboard = BaseUrl + "api/games/leaderboard/top";
    
    /// <summary>Update leaderboard entry</summary>
    public const string UpdateLeaderboard = BaseUrl + "api/games/leaderboard/update";
    
    /// <summary>Create new leaderboard entry</summary>
    public const string CreateLeaderboardEntry = BaseUrl + "api/games/leaderboard/create";
    
    /// <summary>Get specific user's leaderboard data</summary>
    public static string GetUserLeaderboard(string userId) =>
        $"{BaseUrl}api/games/leaderboard/user/{UnityWebRequest.EscapeURL(userId)}";
    
    #endregion
    
    // ========================================================================
    // WALLET & PAYMENTS
    // ========================================================================
    
    #region Wallet
    
    /// <summary>Get user wallet balance</summary>
    public static string GetWalletBalance(string userId) =>
        $"{PaymentBaseUrl}api/payment/getUserWalletBalance/{UnityWebRequest.EscapeURL(userId)}";
    
    /// <summary>Transfer tokens between wallets</summary>
    public const string TransferToken = BaseUrl + "transferToken";
    
    /// <summary>Get transaction history</summary>
    public static string GetTransactionHistory(string userId) =>
        $"{BaseUrl}getUserTransactionHistory/{UnityWebRequest.EscapeURL(userId)}";
    
    /// <summary>Process game payment (entry fee)</summary>
    public const string GamePayment = PaymentBaseUrl + "api/payment/gamePayment";
    
    /// <summary>Distribute rewards</summary>
    public const string RewardDistribute = BaseUrl + "api/payment/rewardDistribute";
    
    /// <summary>Transfer game rewards to user</summary>
    public const string GameRewards = PaymentBaseUrl + "api/payment/transferGameReward";
    
    #endregion
    
    // ========================================================================
    // NFTs
    // ========================================================================
    
    #region NFTs
    
    /// <summary>Get all available NFTs</summary>
    public const string GetAllNFTs = BaseUrl + "api/payment/getAllNft";
    
    /// <summary>Purchase an NFT</summary>
    public const string PurchaseNft = BaseUrl + "api/payment/purchaseNft";
    
    #endregion
    
    // ========================================================================
    // REWARDS & STREAKS
    // ========================================================================
    
    #region Rewards
    
    /// <summary>Claim ad watch rewards</summary>
    public const string ClaimAdsRewards = BaseUrl + "api/games/rewards/watch-ad";
    
    /// <summary>Check daily login status</summary>
    public const string DailyLoginCheck = BaseUrl + "api/games/streak/login-check";
    
    /// <summary>Claim daily login reward</summary>
    public const string DailyLoginClaim = BaseUrl + "api/games/streak/claim-reward";
    
    #endregion
    
    // ========================================================================
    // DAILY MISSIONS
    // ========================================================================
    
    #region Daily Missions
    
    /// <summary>Get today's daily missions</summary>
    public const string GetDailyMissions = BaseUrl + "api/games/daily-missions/get-daily-missions";
    
    /// <summary>Update mission progress</summary>
    public const string UpdateDailyMissionProgress = BaseUrl + "api/games/daily-missions/progress";
    
    /// <summary>Claim mission rewards</summary>
    public const string ClaimDailyMissionRewards = BaseUrl + "api/games/daily-missions/progress";
    
    #endregion
    
    // ========================================================================
    // SHOP & INVENTORY
    // ========================================================================
    
    #region Shop
    
    /// <summary>Get shop by ID</summary>
    public static string GetShop(string shopId) =>
        $"{BaseUrl}api/games/shop/{UnityWebRequest.EscapeURL(shopId)}";
    
    /// <summary>Purchase a product</summary>
    public const string PurchaseProduct = BaseUrl + "api/games/shop/purchase";
    
    /// <summary>Get inventory by game</summary>
    public static string GetInventoryByGame(string gameId) =>
        $"{BaseUrl}api/games/shop/inventory/{UnityWebRequest.EscapeURL(gameId)}";
    
    /// <summary>Use inventory item</summary>
    public const string UseInventory = BaseUrl + "api/games/shop/inventory/use";
    
    #endregion
    
    // ========================================================================
    // AI SERVICES
    // ========================================================================
    
    #region AI
    
    /// <summary>Master prompts for image generation</summary>
    public const string MasterPromptImageGeneration = BaseUrl + "api/ai/ai-enhancement/image/master-prompts";
    
    /// <summary>Enhance/generate AI image</summary>
    public const string EnhanceImage = AIBaseUrl + "api/ai/ai-enhancement/images/enhance";
    
    /// <summary>AI-generated quiz questions</summary>
    public const string AIPromptInterrogateCustomResponse = BaseUrl + "api/ai/ai-prompt/v2/interrogate/custom/response";
    
    /// <summary>AI prompt URL (legacy)</summary>
    public const string AIPromptUrl = AIBaseUrl + "api/ai/ai-prompt/interrogate/custom/response";
    
    /// <summary>Create AI notes</summary>
    public const string NotesCreate = AIBaseUrl + "api/ai/notes/create";
    
    /// <summary>Get notes job status</summary>
    public static string GetNotesJobStatus(string jobId) =>
        $"{AIBaseUrl}api/ai/notes/jobs/{jobId}/status";
    
    /// <summary>Get note details</summary>
    public static string GetNoteDetails(string noteId) =>
        $"{AIBaseUrl}api/ai/notes/{noteId}";
    
    /// <summary>Generate flashcards from note</summary>
    public static string GenerateFlashcards(string noteId) =>
        $"{AIBaseUrl}api/ai/notes/{noteId}/generate-flashcards-quizzes";
    
    #endregion
    
    // ========================================================================
    // REFERRALS
    // ========================================================================
    
    #region Referrals
    
    /// <summary>Get/create referral URL</summary>
    public const string ReferralUrl = BaseUrl + "api/user/referral/url";
    
    /// <summary>Get referral statistics</summary>
    public const string ReferralStats = BaseUrl + "api/user/referral/stats";
    
    /// <summary>Claim referral rewards</summary>
    public const string ClaimReferralRewards = BaseUrl + "api/user/referral/claim";
    
    /// <summary>Claim signup reward</summary>
    public const string ClaimSignupReward = BaseUrl + "api/user/user/claim-signup-reward";
    
    #endregion
}
