namespace IntelliVerseX.Core
{
    /// <summary>
    /// AWS Cognito configuration for IntelliVerse-X platform.
    /// 
    /// This is a SHARED Cognito user pool across ALL IntelliVerse-X games.
    /// All games use the same authentication pool for unified user accounts.
    /// 
    /// Pool Information:
    /// - User Pool Name: aicart-user-pool
    /// - Region: us-east-1
    /// - Created: March 21, 2025
    /// - Feature Plan: Essentials
    /// - Estimated Users: 999+
    /// 
    /// Usage:
    /// - All games authenticate against this pool
    /// - Guest accounts use this pool
    /// - Email/password login uses this pool
    /// - Social login (future) will use this pool
    /// 
    /// Security Notes:
    /// - Client ID is safe to expose in client code (public identifier)
    /// - Client Secret should NEVER be used in client code (use server-side only)
    /// - User Pool ID is public (needed for SDK initialization)
    /// </summary>
    public static class IVXCognitoConfig
    {
        /// <summary>
        /// AWS Cognito User Pool ID (shared across all IntelliVerse-X games)
        /// </summary>
        public const string USER_POOL_ID = "us-east-1_M5qxN8b74";

        /// <summary>
        /// AWS Region for Cognito User Pool
        /// </summary>
        public const string REGION = "us-east-1";

        /// <summary>
        /// Cognito App Client ID (safe to use in client code)
        /// </summary>
        public const string CLIENT_ID = "54clc0uaqvr1944qvkas63o0rb";

        /// <summary>
        /// Token signing key URL (for JWT validation)
        /// </summary>
        public const string TOKEN_SIGNING_KEY_URL = "https://cognito-idp.us-east-1.amazonaws.com/us-east-1_M5qxN8b74/.well-known/jwks.json";

        /// <summary>
        /// Cognito User Pool ARN (for reference)
        /// </summary>
        public const string USER_POOL_ARN = "arn:aws:cognito-idp:us-east-1:970547373533:userpool/us-east-1_M5qxN8b74";

        /// <summary>
        /// User Pool Name (for reference)
        /// </summary>
        public const string USER_POOL_NAME = "aicart-user-pool";
    }
}
