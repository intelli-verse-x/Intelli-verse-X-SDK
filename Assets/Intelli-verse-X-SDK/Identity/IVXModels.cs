// ============================================================================
// IVXModels.cs - IntelliVerse-X API Data Models
// ============================================================================
// PRODUCTION-READY | HIGH-PERFORMANCE | ENTERPRISE-GRADE
// 
// All API request/response models for the IntelliVerse-X platform.
// Uses fields (not properties) for Unity JsonUtility compatibility.
// Thread-safe, immutable-first design where applicable.
// ============================================================================

using System;
using System.Collections.Generic;

namespace IVXModels
{
    // ========================================================================
    // AUTHENTICATION - LOGIN
    // ========================================================================
    
    #region Login Models
    
    /// <summary>Login request payload</summary>
    [Serializable]
    public class PayloadLogin
    {
        public string email;
        public string password;
        public string fromDevice;   // "machine" | "webgl" | "android" | "ios"
        public string macAddress;
        
        public PayloadLogin() { }
        
        public PayloadLogin(string email, string password, string fromDevice, string macAddress)
        {
            this.email = email;
            this.password = password;
            this.fromDevice = fromDevice;
            this.macAddress = macAddress;
        }
    }
    
    /// <summary>Login response envelope</summary>
    [Serializable]
    public class LoginResponse
    {
        public bool status;
        public string message;
        public LoginData data;
    }
    
    /// <summary>Login response data</summary>
    [Serializable]
    public class LoginData
    {
        public LoginUser user;
        public string token;
        public string accessToken;
        public string idToken;
        public string refreshToken;
        public long expiresIn;
    }
    
    /// <summary>User object from login/auth responses</summary>
    [Serializable]
    public class LoginUser
    {
        public string id;
        public string firstName;
        public string lastName;
        public string userName;
        public string email;
        public string phoneNumber;
        public string profilePicture;
        public bool shopifyConnected;
        public string shopifyAccessToken;
        public string shopifyShopName;
        public string role;
        public int age;
        public string kycStatus;
        public string accountStatus;
        public bool isAdult;
        public string idpUsername;
        public string loginType;
        public string createdAt;
        public string updatedAt;
        public string walletAddress;
        public string encryptedPrivateKey;
        public string encryptedMnemonic;
        public bool isGuest;
        public bool isEnablePushNotification;
        public bool isEnableEmailNotification;
        public string fcmToken;
        public string refreshToken;
    }
    
    /// <summary>Basic user info</summary>
    [Serializable]
    public class User
    {
        public string id;
        public string userName;
        public string email;
        public string phoneNumber;
        public string walletAddress;
        public string idpUsername;
        public bool isGuest;
    }
    
    #endregion
    
    // ========================================================================
    // AUTHENTICATION - SIGNUP
    // ========================================================================
    
    #region Signup Models
    
    /// <summary>Signup request payload</summary>
    [Serializable]
    public class PayloadSignup
    {
        public string email;
        public string password;
        public string userName;
        public string firstName;
        public string lastName;
        public string role;
        public string fcmToken;
        public string fromDevice;
        
        public PayloadSignup() { }
        
        public PayloadSignup(string email, string password, string userName, 
            string firstName, string lastName, string role, string fcmToken, string fromDevice)
        {
            this.email = email;
            this.password = password;
            this.userName = userName;
            this.firstName = firstName;
            this.lastName = lastName;
            this.role = role;
            this.fcmToken = fcmToken;
            this.fromDevice = fromDevice;
        }
    }
    
    /// <summary>Initiate signup with OTP</summary>
    [Serializable]
    public class InitSignupPayload
    {
        public string email;
        public string password;
        
        public InitSignupPayload() { }
        public InitSignupPayload(string email, string password)
        {
            this.email = email;
            this.password = password;
        }
    }
    
    /// <summary>Init signup response envelope</summary>
    [Serializable]
    public class InitSignupEnvelope
    {
        public bool status;
        public string message;
        public InitSignupData data;
    }
    
    /// <summary>Init signup response data</summary>
    [Serializable]
    public class InitSignupData
    {
        public string userId;
        public string email;
        public InitSignupCodeDeliveryDetails CodeDeliveryDetails;
    }
    
    /// <summary>Code delivery details</summary>
    [Serializable]
    public class InitSignupCodeDeliveryDetails
    {
        public string Destination;
        public string DeliveryMedium;
        public string AttributeName;
    }
    
    /// <summary>Complete signup with OTP</summary>
    [Serializable]
    public class SignupOtpPayload
    {
        public string email;
        public string firstName;
        public string lastName;
        public string otp;
        public string password;
        public string userName;
        public string role;
        public string fcmToken;
        public string fromDevice;
        public string macAddress;
        public string referralCode;
        
        public SignupOtpPayload() { }
        
        public SignupOtpPayload(string email, string firstName, string lastName, 
            string otp, string password, string userName, string role = "user",
            string fcmToken = null, string fromDevice = null, string macAddress = null,
            string referralCode = null)
        {
            this.email = email;
            this.firstName = firstName;
            this.lastName = lastName;
            this.otp = otp;
            this.password = password;
            this.userName = userName;
            this.role = string.IsNullOrWhiteSpace(role) ? "user" : role;
            this.fcmToken = fcmToken;
            this.fromDevice = fromDevice;
            this.macAddress = macAddress;
            this.referralCode = referralCode;
        }
    }
    
    /// <summary>Signup OTP response envelope</summary>
    [Serializable]
    public class SignupOtpEnvelope
    {
        public bool status;
        public string message;
        public SignupOtpData data;
    }
    
    /// <summary>Signup OTP response data</summary>
    [Serializable]
    public class SignupOtpData
    {
        public SignupOtpUser user;
        public string token;
        public string idToken;
        public string refreshToken;
        public int expiresIn;
    }
    
    /// <summary>User from signup OTP response</summary>
    [Serializable]
    public class SignupOtpUser
    {
        public string firstName;
        public string lastName;
        public string userName;
        public string email;
        public string password;
        public string phoneNumber;
        public string profilePicture;
        public string role;
        public string age;
        public bool isAdult;
        public string idpUsername;
        public string loginType;
        public string walletAddress;
        public string encryptedPrivateKey;
        public string encryptedMnemonic;
        public string fcmToken;
        public string refreshToken;
        public string machineIds;
        public string shopifyAccessToken;
        public string shopifyShopName;
        public string id;
        public string dailyReward;
        public bool shopifyConnected;
        public string kycStatus;
        public string accountStatus;
        public string createdAt;
        public string updatedAt;
        public bool isGuest;
        public bool isEnablePushNotification;
        public bool isEnableEmailNotification;
        public int guestAttempts;
        public bool isAdManagementEnabled;
    }
    
    #endregion
    
    // ========================================================================
    // AUTHENTICATION - PASSWORD
    // ========================================================================
    
    #region Password Models
    
    /// <summary>Forgot password request</summary>
    [Serializable]
    public class PayloadForgotPassword
    {
        public string email;
        public PayloadForgotPassword() { }
        public PayloadForgotPassword(string email) => this.email = email;
    }
    
    /// <summary>Forgot password response</summary>
    [Serializable]
    public class ForgotPasswordEnvelope
    {
        public bool status;
        public string message;
        public ForgotPasswordData data;
    }
    
    [Serializable]
    public class ForgotPasswordData
    {
        public ForgotPasswordResponse response;
    }
    
    [Serializable]
    public class ForgotPasswordResponse
    {
        public CodeDeliveryDetails CodeDeliveryDetails;
    }
    
    [Serializable]
    public class CodeDeliveryDetails
    {
        public string AttributeName;
        public string DeliveryMedium;
        public string Destination;
    }
    
    /// <summary>Reset password request</summary>
    [Serializable]
    public class PayloadResetPassword
    {
        public string email;
        public string code;
        public string newPassword;
        
        public PayloadResetPassword() { }
        public PayloadResetPassword(string email, string code, string newPassword)
        {
            this.email = email;
            this.code = code;
            this.newPassword = newPassword;
        }
    }
    
    /// <summary>Change password request</summary>
    [Serializable]
    public class PayloadChangePassword
    {
        public string currentPassword;
        public string newPassword;
        public string confirmPassword;
        
        public PayloadChangePassword() { }
        public PayloadChangePassword(string currentPassword, string newPassword, string confirmPassword)
        {
            this.currentPassword = currentPassword;
            this.newPassword = newPassword;
            this.confirmPassword = confirmPassword;
        }
    }
    
    [Serializable]
    public class ChangePasswordResponse
    {
        public bool status;
        public string message;
        public ChangePasswordData data;
        public object other;
    }
    
    [Serializable]
    public class ChangePasswordData
    {
        public string id;
        public string email;
        public string userName;
    }
    
    #endregion
    
    // ========================================================================
    // AUTHENTICATION - TOKEN REFRESH
    // ========================================================================
    
    #region Token Models
    
    /// <summary>Refresh token request</summary>
    [Serializable]
    public class PayloadRefreshToken
    {
        public string refreshToken;
        public PayloadRefreshToken() { }
        public PayloadRefreshToken(string token) => refreshToken = token;
    }
    
    /// <summary>Refresh token V2 request</summary>
    [Serializable]
    public class PayloadRefreshTokenV2
    {
        public string usernameOrEmail;
        public string idpUsername;
        public string refreshToken;
        public string loginType;
        public string grantType = "refresh_token";
    }
    
    /// <summary>Refresh token response</summary>
    [Serializable]
    public class RefreshTokenEnvelope
    {
        public bool status;
        public string message;
        public RefreshTokenData data;
    }
    
    [Serializable]
    public class RefreshTokenData
    {
        public string accessToken;
        public string idToken;
        public string refreshToken;
        public long accessTokenExpiresIn;
    }
    
    #endregion
    
    // ========================================================================
    // GUEST AUTHENTICATION
    // ========================================================================
    
    #region Guest Models
    
    /// <summary>Guest login request</summary>
    [Serializable]
    public class GuestLoginRequest
    {
        public string role = "user";
    }
    
    /// <summary>Guest login response</summary>
    [Serializable]
    public class GuestLoginResponse
    {
        public bool status;
        public string message;
        public GuestLoginData data;
    }
    
    [Serializable]
    public class GuestLoginData
    {
        public GuestUser user;
        public string token;
        public string accessToken;
        public string idToken;
        public string refreshToken;
        public int expiresIn;
    }
    
    [Serializable]
    public class GuestUser
    {
        public string firstName;
        public string lastName;
        public string userName;
        public string email;
        public string password;
        public string phoneNumber;
        public string profilePicture;
        public string role;
        public bool isAdult;
        public string idpUsername;
        public string loginType;
        public string walletAddress;
        public string encryptedPrivateKey;
        public string encryptedMnemonic;
        public string refreshToken;
        public string[] machineIds;
        public string shopifyAccessToken;
        public string shopifyShopName;
        public string age;
        public string fcmToken;
        public string id;
        public string dailyReward;
        public bool shopifyConnected;
        public string kycStatus;
        public string accountStatus;
        public string createdAt;
        public string updatedAt;
        public bool isGuest;
        public bool isEnablePushNotification;
        public bool isEnableEmailNotification;
        public int guestAttempts;
        public bool isAdManagementEnabled;
    }
    
    /// <summary>Convert guest without OTP</summary>
    [Serializable]
    public class ConvertGuestPayload
    {
        public string guestUserId;
        public string email;
        public string password;
        public string userName;
        
        public ConvertGuestPayload() { }
        public ConvertGuestPayload(string guestUserId, string email, string password, string userName)
        {
            this.guestUserId = guestUserId;
            this.email = email;
            this.password = password;
            this.userName = userName;
        }
    }
    
    /// <summary>Convert guest response</summary>
    [Serializable]
    public class ConvertGuestResponse
    {
        public bool status;
        public string message;
        public ConvertGuestResponseData data;
    }
    
    [Serializable]
    public class ConvertGuestResponseData
    {
        public User user;
        public string token;
        public string accessToken;
        public string idToken;
        public string refreshToken;
        public long expiresIn;
    }
    
    /// <summary>Guest convert initiate (with OTP)</summary>
    [Serializable]
    public class GuestConvertInitiateRequest
    {
        public string guestUserId;
        public string email;
        public string password;
        public string userName;
    }
    
    [Serializable]
    public class GuestConvertInitiateResponse
    {
        public bool status;
        public string message;
        public GuestConvertInitiateData data;
    }
    
    [Serializable]
    public class GuestConvertInitiateData
    {
        public bool status;
    }
    
    /// <summary>Guest convert confirm (with OTP)</summary>
    [Serializable]
    public sealed class GuestConvertConfirmRequest
    {
        public string guestUserId;
        public string email;
        public string password;
        public string userName;
        public string otp;
    }
    
    [Serializable]
    public sealed class GuestConvertConfirmResponse
    {
        public bool status;
        public string message;
        public GuestConvertConfirmData data;
    }
    
    [Serializable]
    public sealed class GuestConvertConfirmData
    {
        public GuestConvertConfirmUser user;
        public string token;
        public string accessToken;
        public string idToken;
        public string refreshToken;
        public long expiresIn;
    }
    
    [Serializable]
    public sealed class GuestConvertConfirmUser
    {
        public string id;
        public string firstName;
        public string lastName;
        public string userName;
        public string email;
        public string password;
        public string phoneNumber;
        public string profilePicture;
        public string dailyReward;
        public bool shopifyConnected;
        public string shopifyAccessToken;
        public string shopifyShopName;
        public string role;
        public string age;
        public string kycStatus;
        public string accountStatus;
        public bool isAdult;
        public string idpUsername;
        public string loginType;
        public string createdAt;
        public string updatedAt;
        public string walletAddress;
        public string encryptedPrivateKey;
        public string encryptedMnemonic;
        public bool isGuest;
        public bool isEnablePushNotification;
        public bool isEnableEmailNotification;
        public string fcmToken;
        public int guestAttempts;
        public string[] machineIds;
        public bool isAdManagementEnabled;
    }
    
    /// <summary>Guest reward tracking</summary>
    [Serializable]
    public class GuestRewardEntry
    {
        public int levelIndex;
        public int rank;
        public int amount;
        public int timeTaken;
        public int playerKills;
        public int botKills;
        public long unix;
    }
    
    [Serializable]
    public class GuestRewardLedger
    {
        public int total;
        public List<GuestRewardEntry> entries = new List<GuestRewardEntry>();
    }
    
    #endregion
    
    // ========================================================================
    // SOCIAL LOGIN
    // ========================================================================
    
    #region Social Login Models
    
    /// <summary>Social login request</summary>
    [Serializable]
    public class PayloadSocialLogin
    {
        public string token;
        public string loginType;
        public string email;
        public string firstName;
        public string lastName;
        public string userName;
        public string profilePicture;
        public string socialId;
        public string role;
        public string fcmToken;
        public string fromDevice;
        
        public PayloadSocialLogin() { }
        
        public PayloadSocialLogin(string token, string loginType, string email,
            string firstName, string lastName, string userName, string profilePicture,
            string socialId, string role, string fcmToken, string fromDevice)
        {
            this.token = token;
            this.loginType = loginType;
            this.email = email;
            this.firstName = firstName;
            this.lastName = lastName;
            this.userName = userName;
            this.profilePicture = profilePicture;
            this.socialId = socialId;
            this.role = role;
            this.fcmToken = fcmToken;
            this.fromDevice = fromDevice;
        }
    }
    
    /// <summary>Social login V2 request</summary>
    [Serializable]
    public class PayloadSocialLoginV2
    {
        public string email;
        public string firstName;
        public string lastName;
        public string password;
        public string userName;
        public string role;
        public string fcmToken;
        public string fromDevice;
        public string appleKey;
        public string macAddress;
        public string loginType;
        
        public PayloadSocialLoginV2() { }
        
        public PayloadSocialLoginV2(string email, string firstName, string lastName,
            string password, string userName, string role, string fcmToken,
            string fromDevice, string appleKey, string macAddress, string loginType)
        {
            this.email = email;
            this.firstName = firstName;
            this.lastName = lastName;
            this.password = password;
            this.userName = userName;
            this.role = role;
            this.fcmToken = fcmToken;
            this.fromDevice = fromDevice;
            this.appleKey = appleKey;
            this.macAddress = macAddress;
            this.loginType = loginType;
        }
    }
    
    [Serializable]
    public class SocialLoginResponse
    {
        public bool status;
        public string message;
        public SocialLoginData data;
        public object other;
    }
    
    [Serializable]
    public class SocialLoginData
    {
        public LoginUser user;
        public string accessToken;
        public string idToken;
        public string refreshToken;
        public long expiresIn;
        public bool requiresPasswordSetup;
        public bool isNewUser;
    }
    
    #endregion
    
    // ========================================================================
    // USER SESSION
    // ========================================================================
    
    #region Session Models
    
    /// <summary>Unified session persisted on device</summary>
    [Serializable]
    public class UserSession
    {
        public string userId;
        public string username;
        public string email;
        public string role;
        public string idpUsername;
        public string loginType;
        public string walletAddress;
        public string profilePicture;
        public string fcmToken;
        public bool isGuest;
        public string kycStatus;
        public string accountStatus;
        public string accessToken;
        public string idToken;
        public string refreshToken;
        public long expiresIn;
        public long expiresAtUnix;
    }
    
    /// <summary>Local UI state for login screen</summary>
    [Serializable]
    public class LocalLoginState
    {
        public AuthType authType;
        public bool rememberMe;
        public string lastLoginDate;
    }
    
    public enum AuthType { Email, Google, Apple, Facebook, Guest }
    
    #endregion
    
    // ========================================================================
    // USER PROFILE
    // ========================================================================
    
    #region Profile Models
    
    /// <summary>User profile response</summary>
    [Serializable]
    public class UserProfileResponse
    {
        public bool status;
        public string message;
        public UserProfileData data;
        public OtherField other;
    }
    
    [Serializable]
    public class OtherField { }
    
    [Serializable]
    public class UserProfileData
    {
        public string id;
        public string firstName;
        public string lastName;
        public string userName;
        public string email;
        public string password;
        public string phoneNumber;
        public string profilePicture;
        public string dailyReward;
        public bool shopifyConnected;
        public string shopifyAccessToken;
        public string shopifyShopName;
        public string role;
        public string roles;
        public string portalAccess;
        public string age;
        public string kycStatus;
        public string accountStatus;
        public bool isAdult;
        public string idpUsername;
        public string loginType;
        public string createdAt;
        public string updatedAt;
        public string walletAddress;
        public string encryptedPrivateKey;
        public string encryptedMnemonic;
        public bool isGuest;
        public bool isEnablePushNotification;
        public bool isEnableEmailNotification;
        public string fcmToken;
        public int guestAttempts;
        public string refreshToken;
        public string roleId;
        public string machineIds;
        public bool isAdManagementEnabled;
        public string userType;
        public string appleKey;
    }
    
    /// <summary>Update profile request</summary>
    [Serializable]
    public class PayloadUpdateUserProfile
    {
        public string firstName;
        public string lastName;
        public string userName;
        public int age;
        public string phoneNumber;
        
        public PayloadUpdateUserProfile() { }
        public PayloadUpdateUserProfile(string firstName, string lastName, string userName, int age, string phoneNumber)
        {
            this.firstName = firstName;
            this.lastName = lastName;
            this.userName = userName;
            this.age = age;
            this.phoneNumber = phoneNumber;
        }
    }
    
    [Serializable]
    public class UpdateUserProfileResponse
    {
        public bool status;
        public string message;
        public UserProfileData data;
    }
    
    #endregion
    
    // ========================================================================
    // FRIENDS
    // ========================================================================
    
    #region Friends Models
    
    /// <summary>Friend data</summary>
    [Serializable]
    public class FriendData
    {
        public string relationId;
        public string status;
        public string requestStatus;
        public FriendUser user;
        
        // Convenience accessors
        public string id => user?.id;
        public string userName => user?.userName;
        public string profilePicture => user?.profilePicture;
        public string email => user?.email;
        public string firstName => user?.firstName;
        public string lastName => user?.lastName;
    }
    
    [Serializable]
    public class FriendUser
    {
        public string id;
        public string userName;
        public string profilePicture;
        public string email;
        public string firstName;
        public string lastName;
    }
    
    /// <summary>Friends list response</summary>
    [Serializable]
    public class FriendsResponse
    {
        public bool status;
        public string message;
        public List<FriendData> data;
    }
    
    /// <summary>Search user result</summary>
    [Serializable]
    public class SearchUser
    {
        public string id;
        public string userName;
        public string profilePicture;
        public string relationId;
        public string requestStatus;
    }
    
    /// <summary>Friend search response</summary>
    [Serializable]
    public class FriendSearchResponse
    {
        public bool status;
        public string message;
        public List<SearchUser> data;
    }
    
    /// <summary>Send friend request payload</summary>
    [Serializable]
    public class FriendInvitePayload
    {
        public string requesterId;
        public string receiverId;
        
        public FriendInvitePayload() { }
        public FriendInvitePayload(string requesterId, string receiverId)
        {
            this.requesterId = requesterId;
            this.receiverId = receiverId;
        }
    }
    
    /// <summary>Update friend status payload</summary>
    [Serializable]
    public class FriendStatusUpdatePayload
    {
        public string userId;
        public string relationId;
        public string status;
        
        public FriendStatusUpdatePayload() { }
        public FriendStatusUpdatePayload(string userId, string relationId, string status)
        {
            this.userId = userId;
            this.relationId = relationId;
            this.status = status;
        }
    }
    
    #endregion
    
    // ========================================================================
    // LEADERBOARD
    // ========================================================================
    
    #region Leaderboard Models
    
    /// <summary>Leaderboard entry</summary>
    [Serializable]
    public class LeaderboardEntryData
    {
        public string id;
        public string gameId;
        public string gameMode;
        public string gameType;
        public string userId;
        public int score;
        public int level;
        public int rank;
        public string lastPlayed;
        public string lastRankUpdate;
        public string createdAt;
        public string updatedAt;
        public LeaderboardUser user;
        public string userName;
        
        public string GetProfileImageUrl()
        {
            if (user == null) return null;
            return string.IsNullOrWhiteSpace(user.profilePicture) ? null : user.profilePicture;
        }
    }
    
    [Serializable]
    public class LeaderboardUser
    {
        public string id;
        public string firstName;
        public string lastName;
        public string userName;
        public string email;
        public string password;
        public string phoneNumber;
        public string profilePicture;
        public string dailyReward;
        public bool shopifyConnected;
        public string shopifyAccessToken;
        public string shopifyShopName;
        public string role;
        public string age;
        public string kycStatus;
        public string accountStatus;
        public bool isAdult;
        public string idpUsername;
        public string loginType;
        public string createdAt;
        public string updatedAt;
        public string walletAddress;
        public string encryptedPrivateKey;
        public string encryptedMnemonic;
        public bool isGuest;
        public bool isEnablePushNotification;
        public bool isEnableEmailNotification;
        public string fcmToken;
        public int guestAttempts;
        public string refreshToken;
        public string machineIds;
        public bool isAdManagementEnabled;
    }
    
    /// <summary>Leaderboard list response</summary>
    [Serializable]
    public class LeaderboardResponseList
    {
        public List<LeaderboardEntryData> entries;
    }
    
    /// <summary>Single leaderboard entry response</summary>
    [Serializable]
    public class LeaderboardEntryResponse
    {
        public bool status;
        public string message;
        public LeaderboardEntryData data;
    }
    
    /// <summary>Create leaderboard entry payload</summary>
    [Serializable]
    public class PayloadLeaderboardCreate
    {
        public string gameId;
        public string gameMode;
        public string gameType;
        public int score;
        public int level;
        public int rank;
        public string lastPlayed;
        
        public PayloadLeaderboardCreate() { }
        
        public PayloadLeaderboardCreate(int score, int level, DateTime? lastPlayedUtc = null,
            string gameMode = null, string gameType = null, string gameId = null)
        {
            this.gameId = string.IsNullOrWhiteSpace(gameId) ? IVXURLs.GameId : gameId;
            this.gameMode = string.IsNullOrWhiteSpace(gameMode) ? LeaderboardDefaults.GameMode : gameMode;
            this.gameType = string.IsNullOrWhiteSpace(gameType) ? LeaderboardDefaults.GameType : gameType;
            this.score = score;
            this.level = level;
            this.lastPlayed = (lastPlayedUtc ?? DateTime.UtcNow).ToString("o");
        }
    }
    
    /// <summary>Update leaderboard entry payload</summary>
    [Serializable]
    public class PayloadLeaderboardUpdate
    {
        public string gameId;
        public string gameMode;
        public string gameType;
        public int score;
        public int level;
        public string lastPlayed;
        
        public PayloadLeaderboardUpdate() { }
        
        public PayloadLeaderboardUpdate(int score, int level, string lastPlayedIso = null,
            string gameId = null, string gameMode = null, string gameType = null)
        {
            this.gameId = string.IsNullOrWhiteSpace(gameId) ? IVXURLs.GameId : gameId;
            this.gameMode = string.IsNullOrWhiteSpace(gameMode) ? LeaderboardDefaults.GameMode : gameMode;
            this.gameType = string.IsNullOrWhiteSpace(gameType) ? LeaderboardDefaults.GameType : gameType;
            this.score = score;
            this.level = level;
            this.lastPlayed = string.IsNullOrEmpty(lastPlayedIso) ? DateTime.UtcNow.ToString("o") : lastPlayedIso;
        }
    }
    
    [Serializable]
    public class LeaderboardUpdateResponse
    {
        public string id;
        public string gameId;
        public string gameMode;
        public string gameType;
        public string userId;
        public int score;
        public int level;
        public int rank;
        public string lastPlayed;
        public string lastRankUpdate;
        public string createdAt;
        public string updatedAt;
    }
    
    /// <summary>Default leaderboard values</summary>
    public static class LeaderboardDefaults
    {
        public const string GameMode = "SinglePlayer";
        public const string GameType = "LevelBased";
    }
    
    #endregion
    
    // ========================================================================
    // WALLET & PAYMENTS
    // ========================================================================
    
    #region Wallet Models
    
    /// <summary>Wallet balance response</summary>
    [Serializable]
    public class WalletBalanceResponse
    {
        public bool success;
        public WalletBalanceData data;
        public string message;
    }
    
    [Serializable]
    public class WalletBalanceData
    {
        public string wallet;
        public string rawBalance;
        public string balance;
        public string metic;
        public string decimals;
    }
    
    /// <summary>Game payment request</summary>
    [Serializable]
    public class PayloadGamePayment
    {
        public string userId;
        public string gameId;
        public long amount;
        public string remark;
        
        public PayloadGamePayment() { }
        public PayloadGamePayment(string userId, string gameId, long amount, string remark)
        {
            this.userId = userId;
            this.gameId = gameId;
            this.amount = amount;
            this.remark = remark;
        }
    }
    
    [Serializable]
    public class GamePaymentResponse
    {
        public bool success;
        public string message;
        public string paymentId;
        public GamePaymentResponseData data;
    }
    
    [Serializable]
    public class GamePaymentResponseData
    {
        public string message;
        public bool error;
        public string txHash;
    }
    
    /// <summary>Transfer game reward</summary>
    [Serializable]
    public class PayloadTransferGameReward
    {
        public string userId;
        public long amount;
        
        public PayloadTransferGameReward() { }
        public PayloadTransferGameReward(string userId, long amount)
        {
            this.userId = userId;
            this.amount = amount;
        }
    }
    
    [Serializable]
    public class TransferGameRewardEnvelope
    {
        public bool success;
        public string message;
        public TransferGameRewardData data;
    }
    
    [Serializable]
    public class TransferGameRewardData
    {
        public string paymentId;
        public string transactionHash;
        public long amount;
        public string recipient;
    }
    
    /// <summary>Transaction history</summary>
    [Serializable]
    public class TransactionData
    {
        public string id;
        public float amount;
        public string currency;
        public string status;
    }
    
    [Serializable]
    public class TransactionHistoryResponse
    {
        public bool success;
        public string message;
        public TransactionData data;
    }
    
    #endregion
    
    // ========================================================================
    // NFTs
    // ========================================================================
    
    #region NFT Models
    
    [Serializable]
    public class NFTData
    {
        public int id;
        public string nftName;
        public string image;
        public string createdAt;
    }
    
    [Serializable]
    public class NFTResponse
    {
        public bool error;
        public string message;
        public List<NFTData> data;
    }
    
    [Serializable]
    public class PayloadPurchaseNft
    {
        public int userId;
        public string amount;
        public int to;
        public int nftId;
        public int nftAmount;
        
        public PayloadPurchaseNft() { }
        public PayloadPurchaseNft(int userId, string amount, int to, int nftId, int nftAmount)
        {
            this.userId = userId;
            this.amount = amount;
            this.to = to;
            this.nftId = nftId;
            this.nftAmount = nftAmount;
        }
    }
    
    [Serializable]
    public class PayloadRewardDistribute
    {
        public string userId;
        public string reward;
        public string to;
        public string type;
        public int nftId;
        public int nftAmount;
        
        public PayloadRewardDistribute() { }
        public PayloadRewardDistribute(string userId, string reward, string to, string type, int nftId = 0, int nftAmount = 0)
        {
            this.userId = userId;
            this.reward = reward;
            this.to = to;
            this.type = type;
            this.nftId = nftId;
            this.nftAmount = nftAmount;
        }
    }
    
    #endregion
    
    // ========================================================================
    // REWARDS & DAILY LOGIN
    // ========================================================================
    
    #region Rewards Models
    
    [Serializable]
    public class Rewards
    {
        public int amount;
        public string lastWatchTime;
    }
    
    [Serializable]
    public class AdRewardsResponse
    {
        public string message;
        public int cooldown;
        public Rewards reward;
        
        public bool IsSuccess() => reward != null && reward.amount > 0;
        public bool IsOnCooldown() => cooldown > 0 && !IsSuccess();
    }
    
    [Serializable]
    public class DailyLoginResponse
    {
        public bool status;
        public string message;
        public DailyLoginResponseData data;
        public object other;
    }
    
    [Serializable]
    public class DailyLoginResponseData
    {
        public bool success;
        public DailyLoginData data;
        public string message;
    }
    
    [Serializable]
    public class DailyLoginData
    {
        public int loginStreak;
        public RewardInfo rewardAwarded;
        public RewardInfo nextReward;
    }
    
    [Serializable]
    public class RewardInfo
    {
        public string type;
        public int amount;
    }
    
    #endregion
    
    // ========================================================================
    // DAILY MISSIONS
    // ========================================================================
    
    #region Daily Missions Models
    
    [Serializable]
    public class DailyMissionsEnvelope
    {
        public List<DailyMissionProgress> items;
    }
    
    [Serializable]
    public class DailyMissionProgress
    {
        public string id;
        public string userId;
        public string missionId;
        public string gameId;
        public int currentValue;
        public int targetValue;
        public string progressPercentage;
        public string status;
        public string date;
        public string startedAt;
        public string completedAt;
        public string expiredAt;
        public int consecutiveDays;
        public int totalCompletions;
        public string currentMultiplier;
        public int attempts;
        public int failures;
        public string bestTime;
        public int bestScore;
        public bool baseRewardClaimed;
        public bool bonusRewardsClaimed;
        public bool streakRewardClaimed;
        public string claimedRewards;
        public string teamData;
        public bool isChainCompleted;
        public string nextChainMissionId;
        public bool chainRewardClaimed;
        public string metadata;
        public string createdAt;
        public string updatedAt;
        public DailyMissionDef mission;
    }
    
    [Serializable]
    public class DailyMissionDef
    {
        public string id;
        public string gameId;
        public string title;
        public string description;
        public string missionType;
        public string difficulty;
        public string status;
        public int targetValue;
        public string targetUnit;
        public string conditions;
        public string startTime;
        public string endTime;
        public string startDate;
        public string endDate;
        public int maxCompletionsPerDay;
        public int maxCompletionsTotal;
        public int experienceReward;
        public int coinReward;
        public int gemReward;
        public bool isChainMission;
        public string previousMissionId;
        public string nextMissionId;
        public int chainOrder;
        public bool isTeamMission;
        public int minTeamSize;
        public int maxTeamSize;
        public string tags;
        public string iconUrl;
        public string backgroundUrl;
        public int priority;
        public bool isVisible;
        public string metadata;
        public string createdAt;
        public string updatedAt;
    }
    
    [Serializable]
    public class PayloadDailyMissionProgress
    {
        public string missionId;
        public string gameId;
        public int currentValue;
        
        public PayloadDailyMissionProgress() { }
        public PayloadDailyMissionProgress(string missionId, string gameId, int currentValue)
        {
            this.missionId = missionId;
            this.gameId = gameId;
            this.currentValue = currentValue;
        }
    }
    
    #endregion
    
    // ========================================================================
    // SHOP & INVENTORY
    // ========================================================================
    
    #region Shop Models
    
    [Serializable]
    public class ShopsEnvelope
    {
        public bool status;
        public string message;
        public List<ShopDTO> data;
        public object other;
    }
    
    [Serializable]
    public class ShopDTO
    {
        public string id;
        public string userId;
        public string gameId;
        public string shopName;
        public string description;
        public string imageUrl;
        public string iconUrl;
        public string createdAt;
        public string updatedAt;
        public List<ShopItemDTO> items;
    }
    
    [Serializable]
    public class ShopItemDTO
    {
        public string id;
        public string shopId;
        public string productId;
        public string productType;
        public int productUseCount;
        public string status;
        public string createdAt;
        public string updatedAt;
        public ProductDTO product;
    }
    
    [Serializable]
    public class ProductDTO
    {
        public string id;
        public string gameId;
        public string userId;
        public string productName;
        public string price;
        public string description;
        public string imageUrl;
        public string iconUrl;
        public string status;
        public string createdAt;
        public string updatedAt;
        public GameMini game;
        public UserMini user;
    }
    
    [Serializable]
    public class GameMini
    {
        public string id;
        public string gameTitle;
    }
    
    [Serializable]
    public class UserMini
    {
        public string id;
        public string userName;
        public string email;
    }
    
    [Serializable]
    public class PayloadPurchaseProduct
    {
        public string gameId;
        public string productId;
        public string shopId;
        
        public PayloadPurchaseProduct() { }
        public PayloadPurchaseProduct(string gameId, string productId, string shopId)
        {
            this.gameId = gameId;
            this.productId = productId;
            this.shopId = shopId;
        }
    }
    
    [Serializable]
    public class PurchaseEnvelope
    {
        public bool status;
        public string message;
        public PurchaseData data;
        public object other;
    }
    
    [Serializable]
    public class PurchaseData
    {
        public string userId;
        public string gameId;
        public string productId;
        public int remainingUseCount;
        public int totalUseCount;
        public string status;
        public string id;
        public string createdAt;
        public string updatedAt;
    }
    
    [Serializable]
    public class InventoryEnvelope
    {
        public bool status;
        public string message;
        public List<InventoryItemDTO> data;
        public object other;
    }
    
    [Serializable]
    public class InventoryItemDTO
    {
        public string id;
        public string userId;
        public string gameId;
        public string productId;
        public int remainingUseCount;
        public int totalUseCount;
        public string status;
        public string createdAt;
        public string updatedAt;
    }
    
    [Serializable]
    public class PayloadUseInventory
    {
        public string gameId;
        public string productId;
        public int count;
        
        public PayloadUseInventory() { }
        public PayloadUseInventory(string gameId, string productId, int count)
        {
            this.gameId = gameId;
            this.productId = productId;
            this.count = count;
        }
    }
    
    [Serializable]
    public class InventoryUpdateEnvelope
    {
        public bool status;
        public string message;
        public InventoryItemDTO data;
        public object other;
    }
    
    #endregion
    
    // ========================================================================
    // AI SERVICES
    // ========================================================================
    
    #region AI Models
    
    /// <summary>Request for AI avatar/image enhancement</summary>
    [Serializable]
    public class EnhanceImageRequest
    {
        public string[] imageUrls;
        public string type;
        public string model;
        public string prompt;
        public bool isAiPrompt;
        public string[] tags;
        public string id;
        public string userId;
        
        public EnhanceImageRequest() { }
        
        public EnhanceImageRequest(
            string[] imageUrls,
            string prompt,
            string[] tags,
            string model = "gpt-image-1",
            string type = "user",
            bool isAiPrompt = true,
            string id = "866")
        {
            this.imageUrls = imageUrls ?? new string[0];
            this.type = type;
            this.model = model;
            this.prompt = prompt ?? string.Empty;
            this.isAiPrompt = isAiPrompt;
            this.tags = tags ?? new string[0];
            this.id = id;
        }
    }
    
    /// <summary>Response from POST /api/ai/ai-enhancement/images/enhance</summary>
    [Serializable]
    public class EnhanceImageResponse
    {
        public bool status;
        public string message;
        public EnhanceImageData data;
    }
    
    /// <summary>Enhanced image data</summary>
    [Serializable]
    public class EnhanceImageData
    {
        public string enhancedImageUrl;
        public string[] imageUrls;
        public string type;
        public string model;
        public string[] tags;
        public string userId;
    }
    
    [Serializable]
    public class AIQuizRequest
    {
        public string prompt;
        public string return_format;
        public string model;
        
        public AIQuizRequest() { }
        public AIQuizRequest(string prompt, string returnFormat, string model)
        {
            this.prompt = prompt;
            this.return_format = returnFormat;
            this.model = model;
        }
    }
    
    [Serializable]
    public class AIQuizItem
    {
        public string question;
        public string[] options;
        public int correct_answer;
        public string explanation;
        public string category;
        public string difficulty;
    }
    
    [Serializable]
    public class AIQuizBatchEnvelope
    {
        public List<AIQuizItem> items;
    }
    
    [Serializable]
    public class AIQuizBatchOuterEnvelopeObj
    {
        public AIQuizBatchEnvelope response;
        public string model;
        public string version;
    }
    
    [Serializable]
    public class AIQuizBatchOuterEnvelopeString
    {
        public string response;
        public string model;
        public string version;
    }
    
    #endregion
    
    // ========================================================================
    // GENERIC API RESPONSE
    // ========================================================================
    
    #region Generic Response
    
    /// <summary>Generic API response wrapper</summary>
    [Serializable]
    public class ApiResponse<T>
    {
        public bool status;
        public string message;
        public T data;
        public object other;
    }
    
    /// <summary>Generic API response with success flag</summary>
    [Serializable]
    public class ApiSuccessResponse<T>
    {
        public bool success;
        public string message;
        public T data;
    }
    
    #endregion
    
    // ========================================================================
    // REFERRAL SYSTEM (INVITE & EARN)
    // ========================================================================
    
    #region Referral Models
    
    /// <summary>Response from GET /api/user/referral/url</summary>
    [Serializable]
    public class ReferralUrlResponse
    {
        public bool status;
        public string message;
        public ReferralUrlData data;
    }
    
    [Serializable]
    public class ReferralUrlData
    {
        public string referralCode;
        public string referralUrl;
    }
    
    /// <summary>Response from GET /api/user/referral/stats</summary>
    [Serializable]
    public class ReferralStatsResponse
    {
        public bool status;
        public string message;
        public ReferralStatsData data;
    }
    
    [Serializable]
    public class ReferralStatsData
    {
        public int totalReferrals;
        public int completedReferrals;
        public int pendingReferrals;
        public int expiredReferrals;
        public ReferralItem[] referrals;
    }
    
    [Serializable]
    public class ReferralItem
    {
        public string id;
        public string referredUserId;
        public string referredUserName;
        public string referredUserEmail;
        public string status;
        public string createdAt;
        public string completedAt;
        public int rewardAmount;
        public string rewardCurrency;
    }
    
    /// <summary>Request for claiming referral rewards</summary>
    [Serializable]
    public class ClaimReferralRewardsRequest
    {
        public string[] referralIds;
        
        public ClaimReferralRewardsRequest() { }
        public ClaimReferralRewardsRequest(string[] referralIds)
        {
            this.referralIds = referralIds ?? new string[0];
        }
    }
    
    /// <summary>Response from POST /api/user/referral/claim</summary>
    [Serializable]
    public class ClaimReferralRewardsResponse
    {
        public bool status;
        public string message;
        public ClaimReferralRewardsData data;
    }
    
    [Serializable]
    public class ClaimReferralRewardsData
    {
        public int totalClaimed;
        public int totalRewardAmount;
        public string rewardCurrency;
        public string[] claimedReferralIds;
    }
    
    #endregion
}
