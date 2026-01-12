using System;
using UnityEngine;

namespace IntelliVerseX.Identity
{
    /// <summary>
    /// Central runtime identity object for the current player.
    /// Syncs data from UserSessionManager while preserving gameId and deviceId.
    /// </summary>
    public static class IntelliVerseXUserIdentity
    {
        // Identity fields synced from UserSessionManager
        public static string UserId { get; private set; }
        public static string UserName { get; private set; }
        public static string DisplayName { get; private set; }
        public static string FirstName { get; private set; }
        public static string LastName { get; private set; }
        public static string Email { get; private set; }
        public static string IdpUsername { get; private set; }
        public static string Role { get; private set; }
        public static bool IsAdult { get; private set; }
        public static string LoginType { get; private set; }

        // Fields NOT touched by sync - preserved across sessions
        public static string GameId { get; set; } = "quizverse";
        public static string DeviceId { get; private set; }

        // Nakama-specific fields
        public static string NakamaUserId { get; private set; }
        public static string NakamaSessionToken { get; private set; }

        private static bool _isInitialized = false;

        /// <summary>
        /// Initialize device-specific fields that should persist.
        /// Call this once at app startup.
        /// </summary>
        public static void InitializeDevice()
        {
            if (string.IsNullOrEmpty(DeviceId))
            {
                DeviceId = SystemInfo.deviceUniqueIdentifier;
                if (string.IsNullOrEmpty(DeviceId))
                {
                    DeviceId = Guid.NewGuid().ToString();
                }
            }
            _isInitialized = true;
            Debug.Log($"[QUIZVERSE][IDENTITY] Device initialized: {DeviceId}");
        }

        /// <summary>
        /// Sync identity data from UserSessionManager.
        /// Does NOT overwrite GameId or DeviceId.
        /// Call this after successful login/session creation.
        /// </summary>
        public static void SyncFromUserSessionManager()
        {
            if (!_isInitialized)
            {
                InitializeDevice();
            }

            var session = UserSessionManager.Current;
            if (session == null)
            {
                Debug.LogWarning("[QUIZVERSE][IDENTITY] UserSessionManager.Current is null - cannot sync identity");
                return;
            }

            // Copy identity fields from session
            UserId = session.userId;
            UserName = session.userName;
            FirstName = session.firstName;
            LastName = session.lastName;
            Email = session.email;
            IdpUsername = session.idpUsername;
            Role = session.role;
            IsAdult = session.isAdult;
            LoginType = session.loginType;

            // Set display name (prefer userName, fallback to firstName + lastName, then idpUsername)
            if (!string.IsNullOrEmpty(UserName))
            {
                DisplayName = UserName;
            }
            else if (!string.IsNullOrEmpty(FirstName) || !string.IsNullOrEmpty(LastName))
            {
                DisplayName = $"{FirstName} {LastName}".Trim();
            }
            else if (!string.IsNullOrEmpty(IdpUsername))
            {
                DisplayName = IdpUsername;
            }
            else
            {
                DisplayName = "Guest";
            }

            // Validate critical fields
            if (string.IsNullOrEmpty(UserId))
            {
                Debug.LogWarning("[QUIZVERSE][IDENTITY] UserSessionManager.UserId is null or empty");
            }
            if (string.IsNullOrEmpty(UserName) && string.IsNullOrEmpty(DisplayName))
            {
                Debug.LogWarning("[QUIZVERSE][IDENTITY] No username or display name available");
            }

            Debug.Log($"[QUIZVERSE][IDENTITY] Synced from UserSessionManager - UserId: {UserId}, DisplayName: {DisplayName}");
        }

        /// <summary>
        /// Set Nakama-specific authentication data.
        /// Call this after successful Nakama authentication.
        /// </summary>
        public static void SetNakamaAuth(string nakamaUserId, string sessionToken)
        {
            NakamaUserId = nakamaUserId;
            NakamaSessionToken = sessionToken;
            Debug.Log($"[QUIZVERSE][IDENTITY] Nakama auth set - UserId: {nakamaUserId}");
        }

        /// <summary>
        /// Clear all identity data (e.g., on logout).
        /// GameId and DeviceId are preserved.
        /// </summary>
        public static void Clear()
        {
            UserId = null;
            UserName = null;
            DisplayName = null;
            FirstName = null;
            LastName = null;
            Email = null;
            IdpUsername = null;
            Role = null;
            IsAdult = false;
            LoginType = null;
            NakamaUserId = null;
            NakamaSessionToken = null;

            Debug.Log("[QUIZVERSE][IDENTITY] Identity cleared (GameId and DeviceId preserved)");
        }

        /// <summary>
        /// Check if we have valid identity data for leaderboard operations.
        /// </summary>
        public static bool IsValid()
        {
            return !string.IsNullOrEmpty(NakamaUserId) && !string.IsNullOrEmpty(DisplayName);
        }
    }
}
