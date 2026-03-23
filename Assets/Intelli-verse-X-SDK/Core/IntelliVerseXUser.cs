using System;
using UnityEngine;

namespace IntelliVerseX.Core
{
    /// <summary>
    /// Unified user data model for IntelliVerse-X platform.
    /// Represents identity across Cognito, Nakama, Photon, and PlayerPrefs.
    /// </summary>
    [Serializable]
    public class IntelliVerseXUser
    {
        // Display Identity
        public string Username;             // User-entered display name (synced to Photon/Nakama)
        
        // Permanent Identifiers (Never Deleted)
        public string DeviceId;             // Permanent device identifier
        public string GameId;               // Permanent player GUID
        
        // Cognito/AWS Identity
        public string CognitoUserId;        // AWS Cognito user ID (unique across platform)
        public string Email;                // User email from Cognito
        public string IdpUsername;          // Identity Provider username (Cognito)
        public string FirstName;            // User's first name
        public string LastName;             // User's last name
        
        // Authentication Tokens (JWT from Cognito)
        public string AccessToken;          // JWT access token for API calls
        public string IdToken;              // JWT ID token
        public string RefreshToken;         // Refresh token for renewing access
        public long AccessTokenExpiryEpoch; // When access token expires (epoch seconds)
        
        // Wallet System (Dual-Wallet Architecture)
        public string GameWalletId;         // Per-game wallet ID (wallet:<device_id>:<game_id>)
        public string GlobalWalletId;       // Global platform wallet ID (wallet:<device_id>:global)
        public int GameWalletBalance;       // Cached game wallet balance
        public int GlobalWalletBalance;     // Cached global wallet balance
        public string GameWalletCurrency;   // Game wallet currency (e.g., "coins")
        public string GlobalWalletCurrency; // Global wallet currency (e.g., "gems")
        public string WalletAddress;        // Blockchain wallet address (future use)
        
        // User Metadata
        public string Role;                 // User role (e.g., "player", "admin")
        public string IsAdult;              // Age verification status ("True" or "False" as string for server compatibility)
        public string LoginType;            // Login method (e.g., "email", "google", "apple", "guest")
        public string AccountStatus;        // Account status (e.g., "active", "suspended")
        public string KycStatus;            // KYC verification status
        
        // Guest User Management
        public bool IsGuestUser;            // True if this is a temporary guest account
        public long GuestCreatedEpoch;      // When guest account was created (for cleanup after 4 days)
        
        /// <summary>
        /// Check if user data is valid
        /// </summary>
        public bool IsValid()
        {
            return !string.IsNullOrEmpty(DeviceId) && !string.IsNullOrEmpty(GameId);
        }
        
        /// <summary>
        /// Check if guest account has expired (4 days old)
        /// </summary>
        public bool IsGuestExpired()
        {
            if (!IsGuestUser) return false;
            
            var currentEpoch = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            var fourDaysInSeconds = 4 * 24 * 60 * 60; // 4 days
            return (currentEpoch - GuestCreatedEpoch) > fourDaysInSeconds;
        }
        
        /// <summary>
        /// Get remaining days before guest account expires
        /// </summary>
        public int GetGuestDaysRemaining()
        {
            if (!IsGuestUser) return 0;
            
            var currentEpoch = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            var fourDaysInSeconds = 4 * 24 * 60 * 60;
            var elapsed = currentEpoch - GuestCreatedEpoch;
            var remaining = fourDaysInSeconds - elapsed;
            return Mathf.Max(0, (int)(remaining / (24 * 60 * 60)));
        }
        
        /// <summary>
        /// Check if user has Cognito account
        /// </summary>
        public bool HasCognitoAccount()
        {
            return !string.IsNullOrEmpty(CognitoUserId);
        }
        
        /// <summary>
        /// Check if user is authenticated (has valid access token)
        /// </summary>
        public bool IsAuthenticated()
        {
            if (string.IsNullOrEmpty(AccessToken)) return false;
            
            var currentEpoch = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            return currentEpoch < AccessTokenExpiryEpoch;
        }
        
        /// <summary>
        /// Check if user has wallet IDs assigned
        /// </summary>
        public bool HasWalletIds()
        {
            return !string.IsNullOrEmpty(GameWalletId) && !string.IsNullOrEmpty(GlobalWalletId);
        }
    }
}
