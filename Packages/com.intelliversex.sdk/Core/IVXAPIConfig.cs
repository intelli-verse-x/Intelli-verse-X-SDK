namespace IntelliVerseX.Core
{
    /// <summary>
    /// IntelliVerse-X API configuration.
    /// 
    /// Centralized API endpoints and configuration for all IntelliVerse-X games.
    /// All games use the same API infrastructure with game-specific isolation via headers/params.
    /// 
    /// API Infrastructure:
    /// - Main API: api.intelli-verse-x.ai (user auth, referrals, admin)
    /// - AI API: ai.intelli-verse-x.ai (AI prompts, notes, flashcards)
    /// - Media Storage: S3 bucket (intelli-verse-x-media)
    /// 
    /// Authentication:
    /// - OAuth2 with Cognito integration
    /// - Client credentials grant (admin operations)
    /// - User token grant (user operations)
    /// 
    /// Usage:
    /// - Use IVXAPIClient for standardized API calls
    /// - Game-specific data isolated by game ID in requests
    /// - All games share same endpoints
    /// </summary>
    public static class IVXAPIConfig
    {
        // ====================================
        // Base URLs
        // ====================================
        
        /// <summary>
        /// Main API base URL (user auth, admin, referrals)
        /// </summary>
        public const string API_BASE_URL = "https://api.intelli-verse-x.ai";
        
        /// <summary>
        /// AI API base URL (prompts, notes, flashcards)
        /// </summary>
        public const string AI_API_BASE_URL = "https://ai.intelli-verse-x.ai";
        
        /// <summary>
        /// S3 bucket base URL for media storage
        /// </summary>
        public const string S3_BASE_URL = "https://intelli-verse-x-media.s3.us-east-1.amazonaws.com";
        
        // ====================================
        // Authentication Endpoints
        // ====================================
        
        /// <summary>
        /// OAuth token endpoint (admin operations)
        /// </summary>
        public const string OAUTH_TOKEN_URL = API_BASE_URL + "/api/admin/oauth/token";
        
        /// <summary>
        /// User signup initiate endpoint
        /// </summary>
        public const string SIGNUP_INITIATE_URL = API_BASE_URL + "/api/user/auth_v_2/signup/initiate";
        
        /// <summary>
        /// User signup confirm endpoint
        /// </summary>
        public const string SIGNUP_CONFIRM_URL = API_BASE_URL + "/api/user/auth_v_2/signup";
        
        /// <summary>
        /// User login endpoint
        /// </summary>
        public const string LOGIN_URL = API_BASE_URL + "/api/user/auth_v_2/login";
        
        /// <summary>
        /// Guest signup endpoint
        /// </summary>
        public const string GUEST_SIGNUP_URL = API_BASE_URL + "/api/user/auth_v_2/guest-signup";
        
        /// <summary>
        /// Social login endpoint (Google, Apple, etc.)
        /// </summary>
        public const string SOCIAL_LOGIN_URL = API_BASE_URL + "/api/user/auth-v2/social/game/login";
        
        /// <summary>
        /// Get refresh token URL for a user
        /// </summary>
        /// <param name="idpUsername">IDP username</param>
        /// <returns>Full URL with query parameter</returns>
        public static string GetRefreshTokenUrl(string idpUsername)
        {
            return $"{API_BASE_URL}/api/user/auth/refresh-token?idp-username={UnityEngine.Networking.UnityWebRequest.EscapeURL(idpUsername ?? string.Empty)}";
        }
        
        // ====================================
        // AI API Endpoints
        // ====================================
        
        /// <summary>
        /// AI prompt interrogate endpoint (custom responses)
        /// </summary>
        public const string AI_PROMPT_URL = AI_API_BASE_URL + "/api/ai/ai-prompt/interrogate/custom/response";
        
        /// <summary>
        /// Create notes endpoint
        /// </summary>
        public const string NOTES_CREATE_URL = AI_API_BASE_URL + "/api/ai/notes/create";
        
        /// <summary>
        /// Get notes job status URL
        /// </summary>
        /// <param name="jobId">Job ID</param>
        /// <returns>Full URL</returns>
        public static string GetNotesJobStatusUrl(string jobId)
        {
            return $"{AI_API_BASE_URL}/api/ai/notes/jobs/{jobId}/status";
        }
        
        /// <summary>
        /// Get note by ID URL
        /// </summary>
        /// <param name="noteId">Note ID</param>
        /// <returns>Full URL</returns>
        public static string GetNoteUrl(string noteId)
        {
            return $"{AI_API_BASE_URL}/api/ai/notes/{noteId}";
        }
        
        /// <summary>
        /// Generate flashcards/quizzes from note URL
        /// </summary>
        /// <param name="noteId">Note ID</param>
        /// <returns>Full URL</returns>
        public static string GetGenerateFlashcardsUrl(string noteId)
        {
            return $"{AI_API_BASE_URL}/api/ai/notes/{noteId}/generate-flashcards-quizzes";
        }
        
        // ====================================
        // User API Endpoints
        // ====================================
        
        /// <summary>
        /// Get/create referral URL endpoint
        /// </summary>
        public const string REFERRAL_URL_ENDPOINT = API_BASE_URL + "/api/user/referral/url";
        
        /// <summary>
        /// Claim signup reward endpoint
        /// </summary>
        public const string CLAIM_SIGNUP_REWARD_ENDPOINT = API_BASE_URL + "/api/user/user/claim-signup-reward";
        
        // ====================================
        // S3 Storage Paths
        // ====================================
        
        /// <summary>
        /// Get S3 URL for game-specific content
        /// </summary>
        /// <param name="gameFolder">Game folder name (e.g., "quiz-verse", "terminal-rush")</param>
        /// <param name="path">Relative path within game folder</param>
        /// <returns>Full S3 URL</returns>
        public static string GetGameS3Url(string gameFolder, string path)
        {
            return $"{S3_BASE_URL}/{gameFolder}/{path}";
        }
        
        /// <summary>
        /// Get daily quiz S3 URL (shared across games)
        /// </summary>
        /// <param name="fileName">File name (e.g., "2025-11-15.json")</param>
        /// <returns>Full S3 URL</returns>
        public static string GetDailyQuizS3Url(string fileName)
        {
            return $"{S3_BASE_URL}/daily-quiz/{fileName}";
        }
        
        /// <summary>
        /// Get ASMR audio S3 URL for QuizVerse
        /// </summary>
        /// <param name="audioFile">Audio file name</param>
        /// <returns>Full S3 URL</returns>
        public static string GetASMRAudioUrl(string audioFile)
        {
            return GetGameS3Url("quiz-verse/asmr-audio", audioFile);
        }
        
        // ====================================
        // OAuth Configuration
        // ====================================
        
        /// <summary>
        /// OAuth Client ID (from Cognito)
        /// Same as IVXCognitoConfig.CLIENT_ID
        /// </summary>
        public const string OAUTH_CLIENT_ID = "54clc0uaqvr1944qvkas63o0rb";
        
        /// <summary>
        /// OAuth Client Secret (for admin operations)
        /// WARNING: Only use server-side or in secure contexts
        /// </summary>
        public const string OAUTH_CLIENT_SECRET = "1eb7ooua6ft832nh8dpmi37mos4juqq27svaqvmkt5grc3b7e377";
        
        // ====================================
        // Request Configuration
        // ====================================
        
        /// <summary>
        /// Default request timeout in seconds
        /// </summary>
        public const int REQUEST_TIMEOUT_SECONDS = 25;
        
        /// <summary>
        /// Maximum retry attempts for failed requests
        /// </summary>
        public const int MAX_RETRIES = 2;
        
        /// <summary>
        /// Base backoff time in seconds for retries
        /// </summary>
        public const float RETRY_BACKOFF_BASE_SECONDS = 0.75f;
        
        /// <summary>
        /// Jitter percentage for retry backoff (0-1)
        /// </summary>
        public const float RETRY_JITTER_PERCENT = 0.15f;
        
        /// <summary>
        /// Delay between consecutive API calls (rate limiting)
        /// </summary>
        public const float BETWEEN_CALLS_DELAY_SECONDS = 0.12f;
        
        /// <summary>
        /// Default AI model for prompts
        /// </summary>
        public const string DEFAULT_AI_MODEL = "openai/gpt-4o";
        
        // ====================================
        // Debug Configuration
        // ====================================
        
        /// <summary>
        /// Enable debug logging for API calls
        /// </summary>
        public const bool DEBUG_LOGS = true;
        
        /// <summary>
        /// Enable CURL command logging for debugging
        /// </summary>
        public const bool ENABLE_CURL_LOGGING = true;
    }
}
