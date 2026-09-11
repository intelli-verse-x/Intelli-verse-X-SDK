using IntelliVerseX.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

public static partial class APIManager
{
    #region Endpoints
    public static string OAuthTokenUrl = "https://api.intelli-verse-x.ai/api/admin/oauth/token";
    public static string AIPromptUrl = "https://ai.intelli-verse-x.ai/api/ai/ai-prompt/interrogate/custom/response";
    public static string NotesCreateUrl = "https://ai.intelli-verse-x.ai/api/ai/notes/create";
    public static string NotesJobStatusUrl(string jobId) => $"https://ai.intelli-verse-x.ai/api/ai/notes/jobs/{jobId}/status";
    
    // Chat with Notes API URLs
    public static string NotesChatCreateUrl(string noteId) => $"https://ai.intelli-verse-x.ai/api/ai/notes/{noteId}/chat";
    public static string NotesChatStreamUrl(string chatId, string message) => 
        $"https://ai.intelli-verse-x.ai/api/ai/notes/chat/{chatId}/stream?message={Uri.EscapeDataString(message)}";
    public static string NotesChatHistoryUrl(string chatId, int page = 1, int pageSize = 20) => 
        $"https://ai.intelli-verse-x.ai/api/ai/notes/chat/{chatId}/history?page={page}&pageSize={pageSize}";
    public static string NotesDetailsUrl(string noteId) => $"https://ai.intelli-verse-x.ai/api/ai/notes/{noteId}";
    
    // AI Debate API URLs
    public static string NotesDebateTopicsUrl(string noteId, string difficulty = "intermediate") => 
        $"https://ai.intelli-verse-x.ai/api/ai/notes/{noteId}/debate-topics?difficulty={difficulty}";
    public static string NotesDebateStartUrl(string noteId) => 
        $"https://ai.intelli-verse-x.ai/api/ai/notes/{noteId}/debate/start";
    public static string NotesDebateScoreUrl(string chatId) => 
        $"https://ai.intelli-verse-x.ai/api/ai/notes/debate/{chatId}/score";
    public static string NotesDebateModesUrl => "https://ai.intelli-verse-x.ai/api/ai/notes/debate/modes";
    public static string NotesDebateTimedUrl(string noteId) => 
        $"https://ai.intelli-verse-x.ai/api/ai/notes/{noteId}/debate/timed";
    public static string NotesDebateMultiRoundUrl(string noteId) => 
        $"https://ai.intelli-verse-x.ai/api/ai/notes/{noteId}/debate/multi-round";
    public static string NotesDebateRapidFireUrl(string noteId) => 
        $"https://ai.intelli-verse-x.ai/api/ai/notes/{noteId}/debate/rapid-fire";
    public static string NotesDebateNextRoundUrl(string chatId) => 
        $"https://ai.intelli-verse-x.ai/api/ai/notes/debate/{chatId}/next-round";
    public static string NotesDebateTimedStatusUrl(string chatId) => 
        $"https://ai.intelli-verse-x.ai/api/ai/notes/debate/{chatId}/timed-status";
    
    // Oxford Debate API URLs
    public static string NotesDebateOxfordStartUrl(string noteId) => 
        $"https://ai.intelli-verse-x.ai/api/ai/notes/{noteId}/debate/oxford/start";
    public static string NotesDebateOxfordAdvanceUrl(string chatId) => 
        $"https://ai.intelli-verse-x.ai/api/ai/notes/debate/{chatId}/oxford/advance-phase";
    public static string NotesDebateOxfordStatusUrl(string chatId) => 
        $"https://ai.intelli-verse-x.ai/api/ai/notes/debate/{chatId}/oxford/status";
    
    // Flashcards & Quiz API URL
    public static string NotesFlashcardsQuizzesUrl(string noteId) => 
        $"https://ai.intelli-verse-x.ai/api/ai/notes/{noteId}/generate-flashcards-quizzes";

    // Notes List & Management URLs
    public static string NotesListUrl(int page = 1, int pageSize = 20, string type = null, string search = null)
    {
        var url = $"https://ai.intelli-verse-x.ai/api/ai/notes?page={page}&pageSize={pageSize}";
        if (!string.IsNullOrEmpty(type)) url += $"&type={Uri.EscapeDataString(type)}";
        if (!string.IsNullOrEmpty(search)) url += $"&search={Uri.EscapeDataString(search)}";
        return url;
    }
    /// <summary>
    /// Get recent notes URL. Requires userId for the API to work correctly.
    /// </summary>
    public static string NotesRecentUrl(string userId, int limit = 10) => 
        $"https://ai.intelli-verse-x.ai/api/ai/notes/recent/{Uri.EscapeDataString(userId ?? string.Empty)}?limit={limit}";
    
    // Backwards compatibility - uses current user's ID
    public static string NotesRecentUrl(int limit = 10)
    {
        string userId = GetCurrentUserId();
        return NotesRecentUrl(userId, limit);
    }
    
    /// <summary>
    /// Get the current user's ID from the session
    /// </summary>
    private static string GetCurrentUserId()
    {
        // Try to get from live session first
        if (_liveUserSession != null && !string.IsNullOrEmpty(_liveUserSession.idpUsername))
        {
            return _liveUserSession.idpUsername;
        }
        
        // Fallback to stored session (respect remember-me / persist flag)
        var session = UserSessionManager.TryRestorePersistedSession();
        if (session != null && !string.IsNullOrEmpty(session.idpUsername))
        {
            return session.idpUsername;
        }
        
        // Return empty string if no user - API will return appropriate error
        Log("[APIManager] Warning: No user ID available for API call");
        return string.Empty;
    }
    public static string NotesStatsUrl => "https://ai.intelli-verse-x.ai/api/ai/notes/stats/overview";
    public static string NotesDeleteUrl(string noteId) => $"https://ai.intelli-verse-x.ai/api/ai/notes/{noteId}";
    public static string NotesUpdateUrl(string noteId) => $"https://ai.intelli-verse-x.ai/api/ai/notes/{noteId}";

    private static string RefreshTokenUrl(string idpUsername) =>
        $"https://api.intelli-verse-x.ai/api/user/auth/refresh-token?idp-username={UnityWebRequest.EscapeURL(idpUsername ?? string.Empty)}";

    #endregion

    #region Config
    public static string DefaultModel = "openai/gpt-4o";
    
    // Client credentials - loaded from secure config (config/keys.json, environment variables).
    // Production builds: no keys.json and no env vars yields empty strings (auth fails visibly).
    // UNITY_EDITOR: optional keys.json, then env, then dev fallback constants.
    private static string _clientId;
    private static string _clientSecret;

    [Serializable]
    private class OAuthKeysConfig
    {
        public string oauthClientId;
        public string oauthClientSecret;
    }

    /// <summary>
    /// Attempts to load OAuth client id and secret from project <c>config/keys.json</c> (<c>oauthClientId</c>, <c>oauthClientSecret</c>).
    /// </summary>
    public static void LoadCredentialsFromConfig()
    {
        var id = LoadFromKeysJson("oauthClientId");
        var secret = LoadFromKeysJson("oauthClientSecret");
        if (!string.IsNullOrEmpty(id))
            _clientId = id;
        if (!string.IsNullOrEmpty(secret))
            _clientSecret = secret;
    }

    private static string LoadFromKeysJson(string key)
    {
        try
        {
            string projectRoot = System.IO.Path.GetDirectoryName(Application.dataPath);
            if (string.IsNullOrEmpty(projectRoot))
                return null;
            string path = System.IO.Path.Combine(projectRoot, "config", "keys.json");
            if (!System.IO.File.Exists(path))
                return null;
            string json = System.IO.File.ReadAllText(path);
            var config = JsonUtility.FromJson<OAuthKeysConfig>(json);
            if (config == null)
                return null;
            if (key == "oauthClientId")
                return string.IsNullOrEmpty(config.oauthClientId) ? null : config.oauthClientId.Trim();
            if (key == "oauthClientSecret")
                return string.IsNullOrEmpty(config.oauthClientSecret) ? null : config.oauthClientSecret.Trim();
            return null;
        }
        catch (Exception ex)
        {
            Debug.LogWarning($"[APIManager] keys.json read failed: {ex.Message}");
            return null;
        }
    }

    public static string ClientId
    {
        get
        {
            if (string.IsNullOrEmpty(_clientId))
            {
                var fromJson = LoadFromKeysJson("oauthClientId");
                if (!string.IsNullOrEmpty(fromJson))
                    _clientId = fromJson;
                else
                {
                    var envId = System.Environment.GetEnvironmentVariable("IVX_CLIENT_ID");
                    if (!string.IsNullOrEmpty(envId))
                        _clientId = envId.Trim();
                    else
                    {
                        _clientId = string.Empty;
                        Debug.LogWarning("[IVX] OAuth Client ID not configured. Set via config/keys.json or IVX_CLIENT_ID env var.");
                    }
                }
            }
            return _clientId;
        }
    }

    public static string ClientSecret
    {
        get
        {
            if (string.IsNullOrEmpty(_clientSecret))
            {
                var fromJson = LoadFromKeysJson("oauthClientSecret");
                if (!string.IsNullOrEmpty(fromJson))
                    _clientSecret = fromJson;
                else
                {
                    var envSecret = System.Environment.GetEnvironmentVariable("IVX_CLIENT_SECRET");
                    if (!string.IsNullOrEmpty(envSecret))
                        _clientSecret = envSecret.Trim();
                    else
                    {
                        _clientSecret = string.Empty;
                        Debug.LogWarning("[IVX] OAuth Client Secret not configured. Set via config/keys.json or IVX_CLIENT_SECRET env var.");
                    }
                }
            }
            return _clientSecret;
        }
    }
    
    public static int RequestTimeoutSeconds = 25;
    public static int MaxRetries = 2;
    public static float RetryBackoffBaseSeconds = 0.75f;
    public static float JitterPct = 0.15f;
    public static float BetweenCallsDelaySeconds = 0.12f;
    // public static string UserAgent = "QuizVerse-Unity/1.0";
    
    /// <summary>
    /// Enable general debug logs. Set to false in production for better performance.
    /// </summary>
    public static bool DebugLogs = false;
    
    /// <summary>
    /// Enable verbose API request/response logging. Shows full payloads and responses.
    /// WARNING: May expose sensitive data in logs. Use only for debugging.
    /// </summary>
    public static bool VerboseAPILogging = false;
    
    /// <summary>
    /// Enable CURL command logging for easy debugging and replication.
    /// </summary>
    public static bool EnableCurlLogging = false;
    
    /// <summary>
    /// Maximum length of payload/response to log. Longer values are truncated.
    /// </summary>
    public static int MaxLogPayloadLength = 2000;
    
    public static Action<string> OnLog;
    private static string _accessToken;
    private static DateTime _tokenExpiryUtc;
    private static readonly SemaphoreSlim _tokenLock = new SemaphoreSlim(1, 1);
    private const int RecentQueueMax = 200;
    private static readonly Queue<string> _recentQueue = new Queue<string>();
    private static readonly System.Random _rng = new System.Random();

    private static bool _useUserAuthToken = false;
    private static string _userIdpUsername = null;
    private static string _userRefreshToken = null;
    private static string _userAccessToken = null;
    private static DateTime _userAccessExpiryUtc = DateTime.MinValue;
    private static readonly SemaphoreSlim _userAuthLock = new SemaphoreSlim(1, 1);
    private const int MaxAuthFailuresBeforeFallback = 3;
    // ===== User Auth wiring =====
    private static readonly object _authLock = new object();

    private static UserSessionManager.UserSession _liveUserSession;
    public static UserSessionManager.UserSession LiveUserSession => _liveUserSession;

 




#if UNITY_EDITOR
    public static bool UseEditorTestingToken = false;
    public static string EditorTestingBearerToken = null;
#endif
    private static async Task<string> ResolveBearerTokenAsync(string provided, CancellationToken ct)
    {
#if UNITY_EDITOR
        // 1) Editor override (explicitly enabled)
        if (UseEditorTestingToken && !string.IsNullOrWhiteSpace(EditorTestingBearerToken))
            return EditorTestingBearerToken.Trim();
#endif

        // 2) Explicit token passed by caller
        if (!string.IsNullOrWhiteSpace(provided))
            return provided.Trim();

        // 3) If runtime user-auth is already enabled, use (and refresh) it
        if (_useUserAuthToken)
            return await EnsureUserAccessTokenAsync(ct);

        // 4) Try to bootstrap runtime user-auth from the saved session
        var s = UserSessionManager.Current;
        if (s != null)
        {
            bool haveRefresh = !string.IsNullOrWhiteSpace(s.idpUsername) && !string.IsNullOrWhiteSpace(s.refreshToken);
            bool haveAccess = !string.IsNullOrWhiteSpace(s.accessToken);

            if (haveRefresh)
            {
                // Turn on runtime user-auth so we get refresh on 401/expiry automatically
                ConfigureUserAuth(
                    useUserAuthToken: true,
                    idpUsername: s.idpUsername,
                    refreshToken: s.refreshToken,
                    initialAccessToken: s.accessToken,
                    accessTokenExpiresInEpoch: s.accessTokenExpiryEpoch > 0 ? s.accessTokenExpiryEpoch : (long?)null
                );
                return await EnsureUserAccessTokenAsync(ct);
            }

            if (haveAccess)
            {
                // No refresh creds available, but we can still use the saved access token as-is
                return s.accessToken.Trim();
            }
        }

        // 5) Nothing available
        throw new InvalidOperationException("No bearer token available. Login first or enable runtime user-auth.");
    }
    #endregion

    #region DTOs
    [Serializable] private class OAuthClientCredentials { public string client_id; public string client_secret; }
    [Serializable] private class OAuthTokenResponse { public string access_token; public string token_type; public long expires_in; }
    [Serializable] public class AIQuizItem { public string question; public string[] options; public int correct_answer; public string explanation; public string category; public string difficulty; public string question_type; public string folder_name; }
    [Serializable] public class NotesCreateResponse { public string jobId; public string status; public string message; }
    [Serializable] public class NotesJobStatusResponse { public string jobId; public string status; public string message; public string noteId; }
    
    /// <summary>
    /// Supported note types matching the AI Notes API
    /// </summary>
    public enum NoteType
    {
        // Documents
        pdf, docx, pptx, xls, xlsx, csv,
        // Media
        audio, video, image, srt, text,
        // Existing platforms
        chatgpt, youtube, website, gdrive,
        // High-impact social/collaboration platforms
        notion,      // 📓 Notion pages and workspaces
        twitter,     // 🐦 Twitter/X threads and posts
        reddit,      // 🔴 Reddit posts and AMAs
        // TIER 1: High stickiness link types
        quizlet,     // 🃏 Direct competitor conversion - import Quizlet flashcard sets
        wikipedia,   // 📚 Universal knowledge - Wikipedia articles
        medium,      // 📰 Quality long-form - Medium/Substack articles
        khan,        // 🎓 Education-native - Khan Academy videos/articles
        podcast,     // 🎙️ Audio learning - Spotify/Apple podcast episodes
        handwritten, // ✍️ Photo → quiz - OCR for handwritten notes
        // TIER 2: Education-Specific
        coursera,    // 🎯 Coursera/Udemy courses
        arxiv,       // 📑 arXiv research papers
        isbn,        // 📖 Textbook ISBN lookup
        slides       // 🎭 SlideShare/Google Slides/Prezi presentations
    }
    
    /// <summary>
    /// Request parameters for creating a note from various sources
    /// </summary>
    [Serializable]
    public class CreateNoteRequest
    {
        public NoteType type;
        public string filePath;      // For file uploads
        public string url;           // Generic URL
        public string s3Url;         // S3 URL
        public string youtubeUrl;    // YouTube URL
        public string text;          // For text/chatgpt types
        public string title;         // Optional title
        public string difficulty;    // beginner, intermediate, advanced
        public string folderId;      // Optional folder
        public string language;      // Language code (e.g., "en", "es", "hi") - defaults to English
        public bool autoGenerateStudyMaterials = true; // Auto-generate flashcards and quiz
    }
    
    // ═══════════════════════════════════════════════════════════════════════
    // CHAT WITH NOTES API - DTOs
    // ═══════════════════════════════════════════════════════════════════════
    
    [Serializable]
    public class CreateChatResponse
    {
        public bool status;
        public string message;
        public ChatData data;
    }
    
    [Serializable]
    public class ChatData
    {
        public ChatInfo chat;
    }
    
    [Serializable]
    public class ChatInfo
    {
        public string id;
        public string noteId;
        public string userId;
        public string title;
        public string createdAt;
        public string updatedAt;
    }
    
    [Serializable]
    public class ChatHistoryResponse
    {
        public bool status;
        public string message;
        public ChatHistoryData data;
    }
    
    [Serializable]
    public class ChatHistoryData
    {
        public ChatMessage[] messages;
        public int total;
        public int page;
        public int pageSize;
        public int totalPages;
    }
    
    [Serializable]
    public class ChatMessage
    {
        public string id;
        public string chatId;
        public string role;      // "user" or "assistant"
        public string content;
        public string createdAt;
        public ChatMessageMetadata metadata;
    }
    
    [Serializable]
    public class ChatMessageMetadata
    {
        public ChatSource[] sources;
    }
    
    [Serializable]
    public class ChatSource
    {
        public string content;
        public string metadata;
        public int order;
        public float similarity;
        public string type;
    }
    
    [Serializable]
    public class StreamChunk
    {
        public string content;
        public ChatSource[] sources;
    }
    
    // ═══════════════════════════════════════════════════════════════════════
    // AI DEBATE DTOs
    // ═══════════════════════════════════════════════════════════════════════
    
    [Serializable]
    public class DebateTopic
    {
        public string id;
        public string topic;
        public string description;
        public string stance;       // "for", "against", "neutral"
        public string difficulty;   // "beginner", "intermediate", "advanced"
        public string[] suggestedPoints;
        public string[] positions;  // e.g., ["Technology helps education", "Technology harms education"]
        public string userPosition; // User's selected position for debate
    }
    
    [Serializable]
    public class DebateTopicsResponse
    {
        public bool status;
        public string message;
        public DebateTopicsData data;
    }
    
    [Serializable]
    public class DebateTopicsData
    {
        public string noteId;
        public DebateTopic[] topics;
        public int totalTopics;
    }
    
    [Serializable]
    public class DebateStartResponse
    {
        public bool status;
        public string message;
        public DebateStartData data;
    }
    
    [Serializable]
    public class DebateStartData
    {
        public ChatInfo chat;
        public string topic;
        public string userPosition;
        public string aiPosition;
        public string openingMessage;
    }
    
    [Serializable]
    public class DebateScoreResponse
    {
        public bool status;
        public string message;
        public DebateScoreData data;
    }
    
    [Serializable]
    public class DebateScoreData
    {
        public string chatId;
        public string topic;
        public string userPosition;
        public DebateScore score;
        public int messageCount;
        public string evaluatedAt;
    }
    
    [Serializable]
    public class DebateScore
    {
        public int overallScore;    // 0-100
        public DebateScoreCategories categories;
        public string feedback;
        public string[] strengths;
        public string[] areasForImprovement;
        public string grade;        // A, B, C, D, F
    }
    
    [Serializable]
    public class DebateScoreCategories
    {
        public int argumentation;
        public int evidence;
        public int clarity;
        public int rebuttal;
        public int persuasiveness;
    }
    
    // ═══════════════════════════════════════════════════════════════════════
    // FLASHCARD DTOs (for link preview)
    // ═══════════════════════════════════════════════════════════════════════
    
    [Serializable]
    public class FlashcardItem
    {
        public string question;
        public string answer;
    }
    
    [Serializable]
    public class FlashcardsQuizResponse
    {
        public bool status;
        public string message;
        public FlashcardsQuizData data;
    }
    
    [Serializable]
    public class FlashcardsQuizData
    {
        public FlashcardItem[] flashcards;
        public QuizDataCompact quiz;
    }
    
    [Serializable]
    public class QuizDataCompact
    {
        public string title;
        public string description;
        public string difficulty;
        public int timeLimit;
        public QuizQuestionCompact[] questions;
    }
    
    [Serializable]
    public class QuizQuestionCompact
    {
        public string question;
        public string[] options;
        public string answer;
        public string explanation;
        public string type;
        public int points;
        public int order;
        public float source;
        public string sourceType;
    }
    
    // ═══════════════════════════════════════════════════════════════════════════
    // NOTES LIST & MANAGEMENT DTOs
    // ═══════════════════════════════════════════════════════════════════════════
    
    [Serializable]
    public class NotesListResponse
    {
        public bool status;
        public string message;
        public NotesListData data;
    }
    
    [Serializable]
    public class NotesListData
    {
        public NoteListItem[] notes;
        public int total;
        public int page;
        public int pageSize;
        public int totalPages;
    }
    
    [Serializable]
    public class NoteListItem
    {
        public string id;
        public string title;
        public string description;
        public string noteType;
        public string status;
        public string difficulty;
        public string createdAt;
        public string updatedAt;
        public string[] tags;
        public string sourceUrl;
        public bool hasQuiz;
        public bool hasFlashcards;
    }
    
    [Serializable]
    public class NoteDetailsResponse
    {
        public bool status;
        public string message;
        public NoteDetailsDataWrapper data;
    }
    
    /// <summary>
    /// Wrapper for note details API response.
    /// The API returns { data: { note: {...} } }
    /// </summary>
    [Serializable]
    public class NoteDetailsDataWrapper
    {
        public NoteDetailsData note;
    }
    
    [Serializable]
    public class NoteDetailsData
    {
        public string id;
        public string title;
        public string description;
        public string summary;
        public string studyNote;
        public string[] keyPoints;
        public string content;
        public string type;
        public string noteType;
        public string status;
        public string difficulty;
        public string language;
        public string createdAt;
        public string updatedAt;
        public string[] tags;
        public string sourceUrl;
        public FlashcardItem[] flashcards;
        public QuizData quiz;
        public QuizData[] quizzes;  // API returns array of quizzes
        public bool hasStudyNote;
        
        /// <summary>
        /// Gets the primary quiz (from quiz field or first item in quizzes array)
        /// </summary>
        public QuizData GetQuiz()
        {
            if (quiz != null && quiz.questions != null && quiz.questions.Length > 0)
                return quiz;
            
            if (quizzes != null && quizzes.Length > 0)
                return quizzes[0];
            
            return null;
        }
        
        /// <summary>
        /// Check if note has quiz data available
        /// </summary>
        public bool HasQuiz => GetQuiz() != null;
    }
    
    [Serializable]
    public class NotesStatsResponse
    {
        public bool status;
        public string message;
        public NotesStatsData data;
    }
    
    [Serializable]
    public class NotesStatsData
    {
        public int totalNotes;
        public int totalFlashcards;
        public int completedNotes;
        public int processingNotes;
        public NoteTypeCount[] byType;
    }
    
    [Serializable]
    public class NoteTypeCount
    {
        public string type;
        public int count;
    }
    
    [Serializable]
    public class DeleteNoteResponse
    {
        public bool status;
        public string message;
    }
    
    [Serializable]
    public class UpdateNoteResponse
    {
        public bool status;
        public string message;
        public NoteDetailsDataWrapper data;
    }
    
    [Serializable]
    public class UpdateNoteRequest
    {
        public string title;
        public string description;
        public string difficulty;
        public string[] tags;
    }
    
    // ═══════════════════════════════════════════════════════════════════════════
    // ADVANCED DEBATE MODES DTOs
    // ═══════════════════════════════════════════════════════════════════════════
    
    [Serializable]
    public class DebateModeInfo
    {
        public string mode;
        public string name;
        public string description;
        public string icon;
    }
    
    [Serializable]
    public class DebateModesResponse
    {
        public bool status;
        public string message;
        public DebateModesData data;
    }
    
    [Serializable]
    public class DebateModesData
    {
        public DebateModeInfo[] modes;
    }
    
    [Serializable]
    public class TimedDebateConfig
    {
        public int timeLimitSeconds;
        public int totalTimeLimitSeconds;
        public string endsAt;
    }
    
    [Serializable]
    public class TimedDebateStartResponse
    {
        public bool status;
        public string message;
        public TimedDebateStartData data;
    }
    
    [Serializable]
    public class TimedDebateStartData
    {
        public ChatInfo chat;
        public string topic;
        public string userPosition;
        public string aiPosition;
        public string mode;
        public TimedDebateConfig config;
        public string openingMessage;
    }
    
    [Serializable]
    public class MultiRoundConfig
    {
        public int totalRounds;
        public int currentRound;
        public string[] roundTopics;
    }
    
    [Serializable]
    public class MultiRoundDebateStartResponse
    {
        public bool status;
        public string message;
        public MultiRoundDebateStartData data;
    }
    
    [Serializable]
    public class MultiRoundDebateStartData
    {
        public ChatInfo chat;
        public string mainTopic;
        public string userPosition;
        public string aiPosition;
        public string mode;
        public MultiRoundConfig config;
        public string openingMessage;
    }
    
    [Serializable]
    public class RapidFireConfig
    {
        public int maxArgumentLength;
        public int timeLimitSeconds;
    }
    
    [Serializable]
    public class RapidFireDebateStartResponse
    {
        public bool status;
        public string message;
        public RapidFireDebateStartData data;
    }
    
    [Serializable]
    public class RapidFireDebateStartData
    {
        public ChatInfo chat;
        public string topic;
        public string userPosition;
        public string aiPosition;
        public string mode;
        public RapidFireConfig config;
        public string openingMessage;
    }
    
    [Serializable]
    public class NextRoundResponse
    {
        public bool status;
        public string message;
        public NextRoundData data;
    }
    
    [Serializable]
    public class NextRoundData
    {
        public string chatId;
        public int previousRound;
        public int currentRound;
        public int totalRounds;
        public string currentTopic;
        public DebateScore previousRoundScore;
        public string roundTransitionMessage;
    }
    
    [Serializable]
    public class TimedDebateStatusResponse
    {
        public bool status;
        public string message;
        public TimedDebateStatusData data;
    }
    
    [Serializable]
    public class TimedDebateStatusData
    {
        public bool isExpired;
        public int remainingSeconds;
        public string debateStatus;
        public int totalTimeLimitSeconds;
        public int timeLimitPerArgument;
    }
    
    // ═══════════════════════════════════════════════════════════════════════════
    // OXFORD DEBATE DTOs
    // ═══════════════════════════════════════════════════════════════════════════
    
    [Serializable]
    public class OxfordDebateStartResponse
    {
        public bool status;
        public string message;
        public OxfordDebateStartData data;
    }
    
    [Serializable]
    public class OxfordDebateStartData
    {
        public ChatInfo chat;
        public string topic;
        public string userPosition;
        public string aiPosition;
        public string mode;
        public OxfordDebateConfig config;
        public string openingMessage;
    }
    
    [Serializable]
    public class OxfordDebateConfig
    {
        public OxfordDebatePhase[] phases;
        public int currentPhase;
        public string currentPhaseName;
        public bool includesCrossExamination;
        public int totalPhases;
    }
    
    [Serializable]
    public class OxfordDebatePhase
    {
        public string name;
        public string title;
        public string description;
        public bool isCompleted;
        public bool isCurrent;
    }
    
    [Serializable]
    public class OxfordPhaseAdvanceResponse
    {
        public bool status;
        public string message;
        public OxfordPhaseAdvanceData data;
    }
    
    [Serializable]
    public class OxfordPhaseAdvanceData
    {
        public string chatId;
        public int previousPhase;
        public int currentPhase;
        public string currentPhaseName;
        public int totalPhases;
        public DebateScore previousPhaseScore;
        public string transitionMessage;
    }
    
    [Serializable]
    public class OxfordDebateStatusResponse
    {
        public bool status;
        public string message;
        public OxfordDebateStatusData data;
    }
    
    [Serializable]
    public class OxfordDebateStatusData
    {
        public bool isOxfordDebate;
        public string status;
        public string topic;
        public string userPosition;
        public string aiPosition;
        public int currentPhase;
        public string currentPhaseName;
        public string currentPhaseTitle;
        public string currentPhaseDescription;
        public int totalPhases;
        public OxfordDebatePhase[] phases;
        public OxfordPhaseScore[] phaseScores;
        public bool includesCrossExamination;
    }
    
    [Serializable]
    public class OxfordPhaseScore
    {
        public int phase;
        public string phaseName;
        public DebateScore score;
    }
    
    [Serializable] private class AIQuizBatchEnvelope { public List<AIQuizItem> items; }
    [Serializable] private class AIQuizBatchOuterEnvelopeObj { public AIQuizBatchEnvelope response; }
    [Serializable] public class NotesContentResponse { public string noteId; public string title; public string content; public string type; public string difficulty; public string createdAt; }
    [Serializable] private class AIQuizRequest { public string prompt; public string return_format; public string model; public AIQuizRequest(string prompt, string returnFormat, string model) { this.prompt = prompt; this.return_format = returnFormat; this.model = model; } }

    [Serializable] private class RefreshRequest { public string refreshToken; }
    [Serializable]
    private class RefreshData
    {
        public string accessToken;
        public string idToken;
        public long accessTokenExpiresIn;
        public string refreshToken;
    }
    [Serializable]
    private class RefreshResponse
    {
        public bool status;
        public string message;
        public RefreshData data;
    }

    #endregion

    #region Helpers
    private static string TryExtractInnerJson(string raw)
    {
        if (string.IsNullOrEmpty(raw)) return raw;
        var match = Regex.Match(raw, @"\{[^{}]*""items""\s*:\s*\[[\s\S]*?\}[\s\S]*\}", RegexOptions.Multiline);
        if (match.Success) return match.Value;
        match = Regex.Match(raw, @"\{[^{}]*""response""\s*:\s*\{[\s\S]*?""items""\s*:\s*\[[\s\S]*?\}[\s\S]*\}\s*\}", RegexOptions.Multiline);
        if (match.Success) return match.Value;
        return raw;
    }

    private static string BuildCompositeKey(string question, string[] options)
    {
        var sb = new StringBuilder();
        sb.Append(question?.Trim());
        for (int i = 0; i < 4 && i < (options?.Length ?? 0); i++) sb.Append("|").Append(options[i]?.Trim());
        return sb.ToString();
    }

    private static bool RecentlyServed(string question)
    {
        if (string.IsNullOrWhiteSpace(question)) return false;
        foreach (var q in _recentQueue) if (string.Equals(q, question, StringComparison.OrdinalIgnoreCase)) return true;
        return false;
    }

    private static void RememberQuestion(string question)
    {
        if (string.IsNullOrWhiteSpace(question)) return;
        _recentQueue.Enqueue(question.Trim());
        while (_recentQueue.Count > RecentQueueMax) _recentQueue.Dequeue();
    }

    private static string UnescapeJsonString(string s) => s?.Replace("\\\"", "\"").Replace("\\n", "\n").Replace("\\r", "\r").Replace("\\t", "\t");

    /// <summary>
    /// Gets the full language name from a locale code (e.g., "hi" -> "Hindi", "es" -> "Spanish")
    /// </summary>
    public static string GetLanguageNameFromLocale(string localeCode)
    {
        if (string.IsNullOrWhiteSpace(localeCode))
            return "English";

        var code = localeCode.Trim().ToLower();
        
        // Map locale codes to language names
        switch (code)
        {
            case "en": return "English";
            case "es": return "Spanish";
            case "es-419": return "Spanish";
            case "ar": return "Arabic";
            case "zh-cn": return "Chinese";
            case "zh": return "Chinese";
            case "fr": return "French";
            case "de": return "German";
            case "hi": return "Hindi";
            case "id": return "Indonesian";
            case "ja": return "Japanese";
            case "ko": return "Korean";
            case "pt": return "Portuguese";
            case "ru": return "Russian";
            case "zu": return "Zulu";
            default: return "English"; // fallback to English
        }
    }

    //public static void AICreateNote(
    //PayloadAICreateNoteNoFile payload,
    //Action<AICreateNoteResponse> onSuccess,
    //Action<string> onError)
    //{
    //    if (payload == null)
    //    {
    //        onError?.Invoke("Payload cannot be null.");
    //        return;
    //    }

    //    try
    //    {
    //        // Simulate an API call (replace with actual HTTP request logic)
    //        var response = new AICreateNoteResponse
    //        {
    //            jobId = "example-job-id",
    //            status = "success",
    //            message = "Note created successfully."
    //        };

    //        onSuccess?.Invoke(response);
    //    }
    //    catch (Exception ex)
    //    {
    //        onError?.Invoke($"Error creating note: {ex.Message}");
    //    }
    //}

    private static string BuildPrompt(string category, string topic, string difficulty, int count, string language = null)
    {
        var banned = new List<string>();
        if (_recentQueue.Count > 0)
        {
            int take = Math.Min(20, _recentQueue.Count);
            var arr = _recentQueue.ToArray();
            for (int i = Math.Max(0, arr.Length - take); i < arr.Length; i++) banned.Add(arr[i]);
        }
        string bannedClause = banned.Count > 0 ? "\nDo NOT repeat any of these question stems:\n- " + string.Join("\n- ", banned) : "";
        
        // Add language instruction if specified
        string languageClause = string.IsNullOrWhiteSpace(language) || language.Equals("en", StringComparison.OrdinalIgnoreCase) || language.Equals("English", StringComparison.OrdinalIgnoreCase)
            ? ""
            : $"\n- **IMPORTANT**: Generate ALL questions, options, and explanations in {language} language.";
        
        return
$@"Create exactly {count} distinct, high-quality multiple-choice questions **ONLY** about this topic:
- Topic: {topic}

Requirements for each item:
- Category: {category}
- Difficulty: {difficulty} (set the ""difficulty"" field to ""{difficulty}"")
- The question MUST be clearly about the Topic above (no unrelated general knowledge).
- Provide exactly 4 plausible options.
- Do NOT use ""All of the above"" or ""None of the above"".
- Only one option is correct.
- Options must be short and similar in length/tone.
- Include a concise 1–2 sentence explanation of the correct answer (if available).
- The ""category"" field must match exactly: {category}.
- For categories like ""Geo Explorer"" or ""Who's That"", you MAY set ""question_type"" to ""Image"" and provide a relevant ""folder_name"" if it enhances the question.
- Default ""question_type"" is ""Text"" and ""folder_name"" is empty.
- ALL {count} questions must have DIFFERENT stems (no rephrasing of the same fact).{bannedClause}{languageClause}

Output must follow the separate return_format instructions exactly.";
    }

    private static string BuildReturnFormat(int count)
    {
        return
@"Return ONLY JSON with this shape (no markdown, no prose):
{
  ""items"": [
    {
      ""question"": string,
      ""options"": array of exactly 4 strings,
      ""correct_answer"": integer 0..3 (MUST be integer index, NOT string),
      ""explanation"": string (1–2 sentences),
      ""category"": string,
      ""difficulty"": ""easy""|""medium""|""hard"",
      ""question_type"": ""Text""|""Image""|""Video""|""Audio"",
      ""folder_name"": string (folder name in Resources/QuestionMedia/ if applicable)
    },
    ... exactly " + count + @" objects total ...
  ]
}

CRITICAL: The correct_answer field MUST be an integer (0, 1, 2, or 3) representing the index of the correct option in the options array, NOT the answer text itself.
Do not include any other top-level keys. Do not include code fences.";
    }

    private static List<AIQuizItem> TryParseBatch(string json, string category, string difficulty, CancellationToken ct)
    {
        if (string.IsNullOrEmpty(json)) return null;
        try
        {
            var top = JsonUtility.FromJson<AIQuizBatchEnvelope>(json);
            var items = top?.items;
            if (items == null || items.Count == 0)
            {
                var outer = JsonUtility.FromJson<AIQuizBatchOuterEnvelopeObj>(json);
                items = outer?.response?.items;
            }
            if (items == null || items.Count == 0) return null;
            
            // CRITICAL FIX: Handle cases where API returns correct_answer as STRING instead of INTEGER
            // This happens when the AI returns the actual answer text instead of the index
            foreach (var q in items)
            {
                ct.ThrowIfCancellationRequested();
                if (q == null) continue;
                q.difficulty = string.IsNullOrWhiteSpace(q.difficulty) ? difficulty : q.difficulty.Trim().ToLower();
                q.category = string.IsNullOrWhiteSpace(q.category) ? category : q.category.Trim();
                if (q.options != null) for (int i = 0; i < q.options.Length; i++) q.options[i] = (q.options[i] ?? "").Trim();
                
                // FIX: If correct_answer is 0 (default/failed deserialization), try to find it by matching answer text
                // This handles the case where API returns correct_answer as a string (the actual answer)
                if (q.correct_answer == 0 && q.options != null && q.options.Length == 4)
                {
                    // Try to extract the correct answer string from the original JSON
                    var answerMatch = Regex.Match(json, $@"""question""\s*:\s*""{Regex.Escape(q.question)}""[\s\S]*?""correct_answer""\s*:\s*""([^""]+)""");
                    if (answerMatch.Success)
                    {
                        string correctAnswerText = UnescapeJsonString(answerMatch.Groups[1].Value).Trim();
                        // Find which option matches this text
                        for (int i = 0; i < q.options.Length; i++)
                        {
                            if (string.Equals(q.options[i], correctAnswerText, StringComparison.OrdinalIgnoreCase))
                            {
                                q.correct_answer = i;
                                Debug.Log($"[APIManager] Fixed correct_answer for '{q.question}': matched option {i} = '{correctAnswerText}'");
                                break;
                            }
                        }
                    }
                }
            }
            return items;
        }
        catch (Exception ex)
        {
            Debug.LogWarning($"[APIManager] Operation failed: {ex.Message}");
            return null;
        }
    }

    private static AIQuizItem TryParseSingle(string json, string category, string difficulty)
    {
        if (string.IsNullOrEmpty(json)) return null;
        var qMatch = Regex.Match(json, "\"question\"\\s*:\\s*\"([\\s\\S]*?)\"", RegexOptions.Multiline);
        if (!qMatch.Success) return null;
        string question = UnescapeJsonString(qMatch.Groups[1].Value)?.Trim();
        if (string.IsNullOrWhiteSpace(question)) return null;
        var oMatch = Regex.Match(json, "\"options\"\\s*:\\s*\\[(.*?)\\]", RegexOptions.Singleline);
        if (!oMatch.Success) return null;
        var optionsRaw = oMatch.Groups[1].Value;
        var optTokens = new List<string>();
        var sb = new StringBuilder();
        bool inString = false;
        for (int i = 0; i < optionsRaw.Length; i++)
        {
            char c = optionsRaw[i];
            if (c == '"' && (i == 0 || optionsRaw[i - 1] != '\\')) inString = !inString;
            if (c == ',' && !inString) { optTokens.Add(sb.ToString().Trim()); sb.Length = 0; }
            else sb.Append(c);
        }
        if (sb.Length > 0) optTokens.Add(sb.ToString().Trim());
        var options = new List<string>(4);
        foreach (var t in optTokens)
        {
            var s = t.Trim();
            if (s.StartsWith("\"") && s.EndsWith("\"")) s = UnescapeJsonString(s.Substring(1, s.Length - 2));
            options.Add(s);
        }
        if (options.Count != 4) return null;
        int correctIndex = -1;
        var caMatch = Regex.Match(json, "\"correct_answer\"\\s*:\\s*([^,}\\n\\r\\t]+)");
        if (caMatch.Success)
        {
            var val = caMatch.Groups[1].Value.Trim();
            string caStr = val;
            if (caStr.StartsWith("\"") && caStr.EndsWith("\"")) caStr = UnescapeJsonString(caStr.Substring(1, caStr.Length - 2));
            for (int i = 0; i < options.Count; i++) if (string.Equals(options[i], caStr, StringComparison.OrdinalIgnoreCase)) { correctIndex = i; break; }
            if (correctIndex < 0 && int.TryParse(caStr, out int caNum))
            {
                for (int i = 0; i < options.Count; i++) if (int.TryParse(options[i], out int optNum) && optNum == caNum) { correctIndex = i; break; }
            }
            if (correctIndex < 0 && int.TryParse(caStr, out int caMaybeIdx) && caMaybeIdx >= 0 && caMaybeIdx < 4) correctIndex = caMaybeIdx;
        }
        if (correctIndex < 0) return null;
        for (int i = 0; i < options.Count; i++) options[i] = (options[i] ?? "").Trim();
        return new AIQuizItem { question = question, options = options.ToArray(), correct_answer = correctIndex, explanation = "", category = category, difficulty = difficulty };
    }

    private static void AppendValidated(List<AIQuizItem> aggregated, HashSet<string> sessionSeen, List<AIQuizItem> incoming, int target)
    {
        foreach (var q in incoming)
        {
            if (q == null) continue;
            if (q.options == null || q.options.Length != 4) continue;
            bool ok = true;
            var set = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            for (int i = 0; i < 4; i++)
            {
                q.options[i] = (q.options[i] ?? "").Trim();
                if (string.IsNullOrEmpty(q.options[i])) { ok = false; break; }
                set.Add(q.options[i]);
            }
            if (!ok || set.Count < 4) continue;
            
            // CRITICAL VALIDATION: Ensure correct_answer is a valid index (0-3)
            if (q.correct_answer < 0 || q.correct_answer > 3)
            {
                Debug.LogError($"[APIManager] ❌ CRITICAL: Question has invalid correct_answer={q.correct_answer}! Question: '{q.question}'");
                Debug.LogError($"[APIManager] This usually means the AI returned the answer as text instead of integer index.");
                Debug.LogError($"[APIManager] Options: [{string.Join(", ", q.options)}]");
                continue; // Skip this question
            }
            
            string key = BuildCompositeKey(q.question, q.options);
            if (RecentlyServed(q.question) || sessionSeen.Contains(key)) continue;
            sessionSeen.Add(key);
            RememberQuestion(q.question);
            aggregated.Add(q);
            if (aggregated.Count >= target) break;
        }
    }

    public static bool TryConfigureUserAuthFromSavedSession(bool enable = true)
    {
        try
        {
            var sess = UserSessionManager.TryRestorePersistedSession() ?? UserSessionManager.Current;
            if (sess == null) return false;

            // Prefer email -> userName -> idpUsername (server expects a real identifier)
            string identifier =
                !string.IsNullOrWhiteSpace(sess.email) ? sess.email.Trim() :
                !string.IsNullOrWhiteSpace(sess.userName) ? sess.userName.Trim() :
                !string.IsNullOrWhiteSpace(sess.idpUsername) ? sess.idpUsername.Trim() :
                null;

            bool haveRefresh = !string.IsNullOrWhiteSpace(sess.refreshToken) && !string.IsNullOrWhiteSpace(identifier);
            bool haveAccess = !string.IsNullOrWhiteSpace(sess.accessToken);

            if (!haveRefresh && !haveAccess)
                return false;

            if (!enable)
                return true; // caller only wanted to know if a session exists

            // Configure runtime user-auth (access may be null/expired; refresh handles it)
            ConfigureUserAuth(
                useUserAuthToken: true,
                idpUsername: identifier ?? string.Empty,
                refreshToken: haveRefresh ? sess.refreshToken : null,
                initialAccessToken: haveAccess ? sess.accessToken : null,
                accessTokenExpiresInEpoch: sess.accessTokenExpiryEpoch > 0 ? sess.accessTokenExpiryEpoch : (long?)null
            );

#if UNITY_EDITOR
            Log($"[UserAuth] Configured with identifier='{identifier}', refresh={(haveRefresh ? "present" : "missing")}, access={(haveAccess ? "present" : "missing")}");
#endif
            return true;
        }
        catch (Exception ex)
        {
            LogError("[UserAuth] TryConfigureUserAuthFromSavedSession failed: " + ex.Message);
            return false;
        }
    }

    public static void ConfigureUserAuthFromLoginResponse(LoginResponse resp, bool persistSession)
    {
        if (resp?.data?.user == null) return;

        var d = resp.data;
        var u = d.user;

        _userIdpUsername = string.IsNullOrWhiteSpace(u.idpUsername)
            ? (u.email ?? string.Empty)
            : u.idpUsername;

        _userRefreshToken = d.refreshToken ?? string.Empty;

        string access = !string.IsNullOrWhiteSpace(d.accessToken) ? d.accessToken : d.token;
        _userAccessToken = string.IsNullOrWhiteSpace(access) ? null : access.Trim();
        long expEpoch = DateTimeOffset.UtcNow.ToUnixTimeSeconds() + Math.Max(0, d.expiresIn <= 0 ? 1800 : d.expiresIn);
        try { _userAccessExpiryUtc = DateTimeOffset.FromUnixTimeSeconds(expEpoch).UtcDateTime; }
        catch (Exception ex)
        {
            Debug.LogWarning($"[APIManager] Operation failed: {ex.Message}");
            _userAccessExpiryUtc = DateTime.UtcNow.AddMinutes(30);
        }

        _useUserAuthToken = true;

        if (persistSession)
            UserSessionManager.ApplyLoginResponse(resp, persist: true);
        else
            UserSessionManager.ApplyLoginResponse(resp, persist: false);
    }

    private static bool LooksLikeGuid(string s)
    {
        if (string.IsNullOrWhiteSpace(s)) return false;
        Guid g;
        return Guid.TryParse(s, out g);
    }

    private static T TryFromJson<T>(string json, out Exception parseEx)
    {
        parseEx = null;
        try
        {
            if (string.IsNullOrWhiteSpace(json)) return default;
            return JsonUtility.FromJson<T>(json);
        }
        catch (Exception ex)
        {
            parseEx = ex;
            return default;
        }
    }

    // --- AUTH RESOLUTION CORE ---
    // Put alongside your other auth utils in APIManager (static class).
    private const int MaxResolveTries = 3;

    private static bool IsUsableJwt(string token)
    {
        if (string.IsNullOrWhiteSpace(token)) return false;
        // Accept non-JWT opaque tokens too (some IdPs do this). If it looks JWT-like, do a soft exp check.
        var dotCount = 0;
        foreach (var c in token) if (c == '.') dotCount++;
        if (dotCount != 2) return true; // Opaque or non-JWT token, treat as usable.

        try
        {
            var parts = token.Split('.');
            if (parts.Length != 3) return false;
            // Base64Url decode payload
            string P(string s) { s = s.Replace('-', '+').Replace('_', '/'); switch (s.Length % 4) { case 2: s += "=="; break; case 3: s += "="; break; } return s; }
            var payloadJson = Encoding.UTF8.GetString(Convert.FromBase64String(P(parts[1])));
            // Very light exp parse to avoid JSON deps
            var expIdx = payloadJson.IndexOf("\"exp\":", StringComparison.OrdinalIgnoreCase);
            if (expIdx < 0) return true; // no exp -> assume usable
            var span = payloadJson.Substring(expIdx + 6);
            var end = span.IndexOfAny(new[] { ',', '}', ' ' });
            var numStr = end >= 0 ? span.Substring(0, end) : span;
            if (!long.TryParse(new string(numStr.Where(char.IsDigit).ToArray()), out var exp)) return true;
            var expUtc = DateTimeOffset.FromUnixTimeSeconds(exp).UtcDateTime;
            return DateTime.UtcNow < expUtc.AddSeconds(-30);
        }
        catch (Exception ex)
        {
            Debug.LogWarning($"[APIManager] Operation failed: {ex.Message}");
            return true;
        } // On any parse issue, default to usable rather than blocking.
    }

    private static async Task<string> TryResolveUserTokenOnceAsync(string provided, CancellationToken ct)
    {
        var t = await ResolveBearerTokenAsync(provided, ct); // your function
        return IsUsableJwt(t) ? t : null;
    }

    /// <summary>
    /// Use a provided/user token if possible (with refresh + retries). 
    /// Fallback to OAuth client-credentials if still not usable.
    /// </summary>
    private static async Task<string> GetEffectiveBearerTokenAsync(string provided, CancellationToken ct)
    {
        // Try user/provided path with refresh between attempts
        for (int i = 0; i < MaxResolveTries; i++)
        {
            try
            {
                var token = await TryResolveUserTokenOnceAsync(provided, ct);
                if (!string.IsNullOrWhiteSpace(token))
                {
                    Log($"[Auth] Using user/provided token (attempt {i + 1}/{MaxResolveTries}).");
                    return token;
                }
            }
            catch (Exception ex)
            {
                Log($"[Auth] Resolve attempt {i + 1}/{MaxResolveTries} failed: {ex.Message}");
            }

            if (_useUserAuthToken && UserAuthConfigured)
            {
                try
                {
                    Log("[Auth] Refreshing user token …");
                    await RefreshUserTokenAsync(ct);
                }
                catch (Exception rex)
                {
                    Log($"[Auth] Refresh failed: {rex.Message}");
                }
            }
        }

        // Fallback to OAuth client-credentials (thread-safe)
        Log("[Auth] Falling back to OAuth client-credentials token.");
        var oauth = await EnsureTokenAsync(ct); // your existing implementation
        if (string.IsNullOrWhiteSpace(oauth))
            throw new Exception("OAuth fallback token acquisition returned empty token.");
        return oauth;
    }

#if UNITY_EDITOR
    private static string TryGetJwtSub(string jwt)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(jwt)) return null;
            var parts = jwt.Split('.');
            if (parts.Length < 2) return null;
            string payload = PadBase64(parts[1]);
            var json = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(payload));
            var match = System.Text.RegularExpressions.Regex.Match(json, "\"sub\"\\s*:\\s*\"([^\"]+)\"");
            return match.Success ? match.Groups[1].Value : null;
        }
        catch (Exception ex)
        {
            Debug.LogWarning($"[APIManager] Operation failed: {ex.Message}");
            return null;
        }
    }
    private static string PadBase64(string s) => s.PadRight(s.Length + (4 - s.Length % 4) % 4, '=');
#endif




    #endregion

    #region Curl + Logging
    private static string EscapeForSingleQuotes(string s) => string.IsNullOrEmpty(s) ? s : s.Replace("'", "'\"'\"'");
    private static string BuildCurlCommand(string method, string url, IEnumerable<KeyValuePair<string, string>> headers, string body)
    {
        var sb = new StringBuilder();
        sb.Append("curl -X ").Append(method ?? "POST").Append(" '").Append(url).Append("'");
        if (headers != null)
        {
            foreach (var kv in headers)
            {
                if (string.IsNullOrEmpty(kv.Key)) continue;
                var v = RedactHeaderValueForLogs(kv.Key, kv.Value);
                sb.Append(" \\\n  -H '").Append(EscapeForSingleQuotes(kv.Key)).Append(": ").Append(EscapeForSingleQuotes(v)).Append("'");
            }
        }
        if (!string.IsNullOrEmpty(body))
        {
            string safeBody = MaskSensitiveFields(body, new[]
            {
                "password", "email", "otp", "refreshToken", "accessToken", "idToken", "token", "client_secret", "newPassword"
            });
            sb.Append(" \\\n  --data-raw '").Append(EscapeForSingleQuotes(safeBody)).Append("'");
        }
        return sb.ToString();
    }

    private static string RedactHeaderValueForLogs(string headerName, string value)
    {
        if (string.IsNullOrEmpty(headerName) || string.IsNullOrEmpty(value))
            return value ?? string.Empty;

        if (headerName.Equals("Authorization", StringComparison.OrdinalIgnoreCase))
        {
            if (value.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
                return "Bearer ***";
            if (value.StartsWith("Basic ", StringComparison.OrdinalIgnoreCase))
                return "Basic ***";
            return "***";
        }

        return value;
    }

    private static void PrintCurl(string method, string url, IEnumerable<KeyValuePair<string, string>> headers, string body)
    {
        if (!EnableCurlLogging) return;
        var cmd = BuildCurlCommand(method, url, headers, body);
        Debug.Log("[CURL]\n" + cmd);
        OnLog?.Invoke("[CURL]\n" + cmd);
    }
    // Add alongside EscapeForSingleQuotes
    private static string EscapeForDoubleQuotes(string s) =>
        string.IsNullOrEmpty(s) ? s : s.Replace("\"", "\\\"");

    // New: build a multipart/form-data curl like the one you shared
    private static string BuildCurlMultipartCommand(
        string url,
        IEnumerable<KeyValuePair<string, string>> headers,
        IEnumerable<(string name, string value, bool isFile, string filePath, string contentType)> parts,
        bool followRedirects = true)
    {
        var sb = new StringBuilder();
        sb.Append("curl ");
        if (followRedirects) sb.Append("--location ");
        sb.Append("'").Append(url).Append("'");

        if (headers != null)
        {
            foreach (var kv in headers)
            {
                if (string.IsNullOrEmpty(kv.Key)) continue;
                var v = RedactHeaderValueForLogs(kv.Key, kv.Value ?? "");
                sb.Append(" \\\n  --header '")
                  .Append(EscapeForSingleQuotes(kv.Key))
                  .Append(": ")
                  .Append(EscapeForSingleQuotes(v))
                  .Append("'");
            }
        }

        if (parts != null)
        {
            foreach (var p in parts)
            {
                if (p.isFile)
                {
                    // Example: --form 'file=@"/path/to/file.docx";type=application/vnd.openxmlformats-officedocument.wordprocessingml.document'
                    sb.Append(" \\\n  --form '")
                      .Append(EscapeForSingleQuotes(p.name))
                      .Append("=@\"").Append(EscapeForDoubleQuotes(p.filePath)).Append("\"");
                    if (!string.IsNullOrEmpty(p.contentType))
                        sb.Append(";type=").Append(p.contentType);
                    sb.Append("'");
                }
                else
                {
                    // Example: --form 'type="docx"'
                    sb.Append(" \\\n  --form '")
                      .Append(EscapeForSingleQuotes(p.name))
                      .Append("=\"")
                      .Append(EscapeForDoubleQuotes(p.value))
                      .Append("\"'");
                }
            }
        }

        return sb.ToString();
    }

    private static void PrintCurlMultipart(
        string url,
        IEnumerable<KeyValuePair<string, string>> headers,
        IEnumerable<(string name, string value, bool isFile, string filePath, string contentType)> parts)
    {
        if (!EnableCurlLogging) return;
        var cmd = BuildCurlMultipartCommand(url, headers, parts, followRedirects: true);
        Debug.Log("[CURL]\n" + cmd);
        OnLog?.Invoke("[CURL]\n" + cmd);
    }

    private static void Log(string msg) { if (!DebugLogs) return; Debug.Log(msg); OnLog?.Invoke(msg); }
    private static void LogError(string msg) { if (!DebugLogs) return; Debug.LogError(msg); OnLog?.Invoke(msg); }
    
    /// <summary>
    /// Log API request details with full payload (when VerboseAPILogging is enabled).
    /// </summary>
    public static void LogAPIRequest(string method, string url, string payload, Dictionary<string, string> headers = null)
    {
        if (!VerboseAPILogging) return;
        
        var sb = new StringBuilder();
        sb.AppendLine($"\n========== [IVX-API REQUEST] ==========");
        sb.AppendLine($"Method: {method}");
        sb.AppendLine($"URL: {url}");
        sb.AppendLine($"Time: {DateTime.Now:HH:mm:ss.fff}");
        
        if (headers != null && headers.Count > 0)
        {
            sb.AppendLine("Headers:");
            foreach (var h in headers)
            {
                string value = RedactHeaderValueForLogs(h.Key, h.Value);
                sb.AppendLine($"  {h.Key}: {value}");
            }
        }
        
        if (!string.IsNullOrEmpty(payload))
        {
            string displayPayload = payload;
            if (displayPayload.Length > MaxLogPayloadLength)
            {
                displayPayload = displayPayload.Substring(0, MaxLogPayloadLength) + $"... (truncated, total {payload.Length} chars)";
            }
            // Redact common sensitive fields
            displayPayload = Regex.Replace(displayPayload, "(\"password\"\\s*:\\s*\")([^\"]+)(\")", "$1****$3");
            displayPayload = Regex.Replace(displayPayload, "(\"client_secret\"\\s*:\\s*\")([^\"]+)(\")", "$1****$3");
            displayPayload = Regex.Replace(displayPayload, "(\"refreshToken\"\\s*:\\s*\")([^\"]+)(\")", "$1****$3");
            sb.AppendLine($"Payload:\n{displayPayload}");
        }
        
        sb.AppendLine("========================================");
        Debug.Log(sb.ToString());
        OnLog?.Invoke(sb.ToString());
    }
    
    /// <summary>
    /// Log API response details with full body (when VerboseAPILogging is enabled).
    /// </summary>
    public static void LogAPIResponse(string url, long statusCode, string responseBody, float durationMs = 0)
    {
        if (!VerboseAPILogging) return;
        
        var sb = new StringBuilder();
        sb.AppendLine($"\n========== [IVX-API RESPONSE] ==========");
        sb.AppendLine($"URL: {url}");
        sb.AppendLine($"Status: {statusCode}");
        sb.AppendLine($"Time: {DateTime.Now:HH:mm:ss.fff}");
        if (durationMs > 0)
        {
            sb.AppendLine($"Duration: {durationMs:F0}ms");
        }
        
        if (!string.IsNullOrEmpty(responseBody))
        {
            string displayBody = responseBody;
            if (displayBody.Length > MaxLogPayloadLength)
            {
                displayBody = displayBody.Substring(0, MaxLogPayloadLength) + $"... (truncated, total {responseBody.Length} chars)";
            }
            // Redact common sensitive fields in response (never print JWTs)
            displayBody = MaskSensitiveFields(displayBody, new[]
            {
                "accessToken", "refreshToken", "idToken", "token", "password", "client_secret"
            });
            sb.AppendLine($"Body:\n{displayBody}");
        }
        else
        {
            sb.AppendLine("Body: (empty)");
        }
        
        sb.AppendLine("========================================");
        
        // Use appropriate log level based on status code
        if (statusCode >= 200 && statusCode < 300)
        {
            Debug.Log(sb.ToString());
        }
        else if (statusCode >= 400)
        {
            Debug.LogWarning(sb.ToString());
        }
        else
        {
            Debug.Log(sb.ToString());
        }
        OnLog?.Invoke(sb.ToString());
    }
    
    /// <summary>
    /// Log API error with context information.
    /// </summary>
    public static void LogAPIError(string url, string method, string errorMessage, Exception ex = null)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"\n========== [IVX-API ERROR] ==========");
        sb.AppendLine($"Method: {method}");
        sb.AppendLine($"URL: {url}");
        sb.AppendLine($"Error: {errorMessage}");
        if (ex != null)
        {
            sb.AppendLine($"Exception: {ex.GetType().Name}: {ex.Message}");
            if (DebugLogs)
            {
                sb.AppendLine($"Stack: {ex.StackTrace}");
            }
        }
        sb.AppendLine("======================================");
        
        Debug.LogError(sb.ToString());
        OnLog?.Invoke(sb.ToString());
    }
    #endregion

    #region HTTP Core
    private static async Task<string> PostJsonAsync(string url, string json, string authorizationHeader, bool redactSecretsInBodyLog, CancellationToken ct)
    {
        if (ct.IsCancellationRequested) { LogError("[HTTP] Request was called with an already-cancelled token."); ct.ThrowIfCancellationRequested(); }
        Exception lastError = null;

        for (int attempt = 0; attempt <= MaxRetries; attempt++)
        {
            ct.ThrowIfCancellationRequested();
            if (attempt > 0)
            {
                var jitter = 1f + (float)((_rng.NextDouble() * 2 - 1) * JitterPct);
                float delaySec = RetryBackoffBaseSeconds * Mathf.Pow(2f, attempt) * jitter;
                Log($"[HTTP] Retry #{attempt} after backoff {delaySec:0.00}s …");
                await Task.Delay(TimeSpan.FromSeconds(delaySec), ct);
            }

            try
            {
                using (var uwr = new UnityWebRequest(url, UnityWebRequest.kHttpVerbPOST))
                {
                    byte[] bodyRaw = Encoding.UTF8.GetBytes(json ?? "{}");
                    uwr.uploadHandler = new UploadHandlerRaw(bodyRaw);
                    uwr.downloadHandler = new DownloadHandlerBuffer();
                    uwr.SetRequestHeader("Content-Type", "application/json");
                    // if (!string.IsNullOrEmpty(UserAgent)) uwr.SetRequestHeader("User-Agent", UserAgent);
                    if (!string.IsNullOrEmpty(authorizationHeader)) uwr.SetRequestHeader("Authorization", authorizationHeader);

                    string preview = json ?? "{}";
                    if (redactSecretsInBodyLog)
                    {
                        preview = Regex.Replace(preview, "(\"client_secret\"\\s*:\\s*\")([^\"]+)(\")", "$1****$3");
                        preview = Regex.Replace(preview, "(\"client_id\"\\s*:\\s*\")([^\"]+)(\")", "$1****$3");
                    }
                    if (preview.Length > 600) preview = preview.Substring(0, 600) + "...(truncated)";
                    string hdr = string.IsNullOrEmpty(authorizationHeader) ? "[no auth]" : (authorizationHeader.StartsWith("Bearer", StringComparison.OrdinalIgnoreCase) ? "[Bearer ***]" : "[auth ***]");
                    Log($"\n[HTTP] POST {url}\nHeaders: {hdr} + Content-Type: application/json\nBody[{(json ?? "{}").Length}]: {preview}");

                    var curlHeaders = new Dictionary<string, string> { { "Content-Type", "application/json" } };
                    // if (!string.IsNullOrEmpty(UserAgent)) curlHeaders["User-Agent"] = UserAgent;
                    if (!string.IsNullOrEmpty(authorizationHeader)) curlHeaders["Authorization"] = authorizationHeader;
                    PrintCurl("POST", url, curlHeaders, json ?? "{}");

                    var op = uwr.SendWebRequest();
                    float start = Time.realtimeSinceStartup;

                    while (!op.isDone)
                    {
                        if (ct.IsCancellationRequested) { uwr.Abort(); break; }
                        if (Time.realtimeSinceStartup - start > RequestTimeoutSeconds) { uwr.Abort(); throw new TimeoutException($"Request timed out after {RequestTimeoutSeconds}s"); }
                        await Task.Yield();
                    }

                    ct.ThrowIfCancellationRequested();

#if UNITY_2020_1_OR_NEWER
                    bool isNetworkError = uwr.result == UnityWebRequest.Result.ConnectionError;
                    bool isHttpError = uwr.result == UnityWebRequest.Result.ProtocolError;
#else
                    bool isNetworkError = uwr.isNetworkError;
                    bool isHttpError    = uwr.isHttpError;
#endif
                    string text = uwr.downloadHandler?.text;
                    long code = uwr.responseCode;

                    Log($"[HTTP] ← {code} in {(Time.realtimeSinceStartup - start):0.000}s");
                    if (!string.IsNullOrEmpty(text))
                    {
                        string respPreview = text.Length > 1200 ? text.Substring(0, 1200) + "...(truncated)" : text;
                        Log($"[HTTP] Response Body[{text.Length}]: {respPreview}");
                    }

                    if (!isNetworkError && !isHttpError && code >= 200 && code < 300) return string.IsNullOrEmpty(text) ? "{}" : text;

                    if (code == 401 && authorizationHeader != null && authorizationHeader.StartsWith("Bearer", StringComparison.OrdinalIgnoreCase) && attempt < MaxRetries)
                    {
                        Log("[HTTP] 401 Unauthorized – refreshing token and retrying …");
                        await _tokenLock.WaitAsync(ct);
                        try { _accessToken = null; _tokenExpiryUtc = DateTime.MinValue; }
                        finally { _tokenLock.Release(); }
                        continue;
                    }

                    throw new Exception($"HTTP {(int)code} : {text}");
                }
            }
            catch (Exception ex) when (!(ex is OperationCanceledException))
            {
                lastError = ex;
                LogError($"[HTTP] Attempt #{attempt} failed: {ex.GetType().Name} – {ex.Message}\n{ex.StackTrace}");
            }
            catch (OperationCanceledException oce)
            {
                LogError($"[HTTP] Attempt #{attempt} cancelled.");
                throw oce;
            }
        }

        throw lastError ?? new Exception("Unknown networking error");
    }
    #endregion

}
