using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
using static System.Net.WebRequestMethods;

public static class APIManager
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
        
        // Fallback to stored session
        var session = UserSessionManager.Load();
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
    
    // Client credentials - loaded from secure config (environment variables or local config file)
    // DO NOT hardcode secrets in production builds!
    // Fallback to hardcoded values in Editor for convenience, but use SecureAPIConfig in builds
    private static string _clientId;
    private static string _clientSecret;
    
    public static string ClientId
    {
        get
        {
            if (string.IsNullOrEmpty(_clientId))
            {
                #if UNITY_EDITOR
                // In editor, use hardcoded values for convenience
                _clientId = "54clc0uaqvr1944qvkas63o0rb";
                #else
                // In builds, try to get from environment variable
                _clientId = System.Environment.GetEnvironmentVariable("QUIZVERSE_CLIENT_ID") 
                    ?? "54clc0uaqvr1944qvkas63o0rb"; // Fallback for now
                #endif
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
                #if UNITY_EDITOR
                // In editor, use hardcoded values for convenience
                _clientSecret = "1eb7ooua6ft832nh8dpmi37mos4juqq27svaqvmkt5grc3b7e377";
                #else
                // In builds, try to get from environment variable
                _clientSecret = System.Environment.GetEnvironmentVariable("QUIZVERSE_CLIENT_SECRET")
                    ?? "1eb7ooua6ft832nh8dpmi37mos4juqq27svaqvmkt5grc3b7e377"; // Fallback for now
                #endif
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
    public static bool DebugLogs = true;
    public static bool EnableCurlLogging = true;
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

    public static UserSessionManager.UserSession _liveUserSession; // in-memory session for current run (even if not persisted)

 




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
        catch { return null; }
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
            var sess = UserSessionManager.Current;
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

        // Populate fields used by your UserAuthConfigured property
        _userIdpUsername = string.IsNullOrWhiteSpace(u.idpUsername)
            ? (u.email ?? string.Empty)
            : u.idpUsername;

        _userRefreshToken = d.refreshToken ?? string.Empty;

        // Optional: if you have these fields, hydrate them too:
        // _userAccessToken = !string.IsNullOrWhiteSpace(d.accessToken) ? d.accessToken : d.token;
        // _userAccessTokenExpiryEpoch = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
        //                              + Math.Max(0, d.expiresIn <= 0 ? 1800 : d.expiresIn);

        _useUserAuthToken = true;

        if (persistSession)
            UserSessionManager.SaveFromLoginResponse(resp);
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
        catch { return true; } // On any parse issue, default to usable rather than blocking.
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
        catch { return null; }
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
                var v = kv.Value ?? "";
                sb.Append(" \\\n  -H '").Append(EscapeForSingleQuotes(kv.Key)).Append(": ").Append(EscapeForSingleQuotes(v)).Append("'");
            }
        }
        if (!string.IsNullOrEmpty(body)) sb.Append(" \\\n  --data-raw '").Append(EscapeForSingleQuotes(body)).Append("'");
        return sb.ToString();
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
                var v = kv.Value ?? "";
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

    #region Auth V2: Signup / Initiate (Cognito)

    // === Endpoint ===
    public static string SignupInitiateUrl = "https://api.intelli-verse-x.ai/api/user/auth_v_2/signup/initiate";

    // === DTOs ===
    [Serializable]
    private class SignupInitiateRequest
    {
        public string email;
        public string password;
        public string role;
    }

    [Serializable]
    public class SignupInitiateResponse
    {
        public bool status;
        public string message;
    }

    // === Public API ===
    /// <summary>
    /// Creates a Cognito user (or resends OTP if already initiated).
    /// Validates email & password, masks sensitive values in logs,
    /// retries on 408/429/5xx with backoff, and returns a typed response.
    /// </summary>
    public static async Task<SignupInitiateResponse> SignupInitiateAsync(
        string email,
        string password,
        string role = "user",
        CancellationToken ct = default)
    {
        // ---- Input validation ----
        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("Email is required.", nameof(email));
        if (!IsValidEmail(email))
            throw new ArgumentException("Email format is invalid.", nameof(email));

        if (string.IsNullOrWhiteSpace(password))
            throw new ArgumentException("Password is required.", nameof(password));
        ValidatePassword(password); // throws with helpful message if weak

        role = string.IsNullOrWhiteSpace(role) ? "user" : role.Trim();

        var reqObj = new SignupInitiateRequest
        {
            email = email.Trim(),
            password = password,
            role = role
        };

        // Real body for the request
        string bodyJson = JsonUtility.ToJson(reqObj);

        // Masked body ONLY for logs / cURL preview
        string maskedJson = MaskSensitiveFields(bodyJson, new[] { "password", "email" });

        // cURL preview (masked)
        PrintCurl("POST", SignupInitiateUrl,
            new Dictionary<string, string> {
            { "accept", "*/*" },
            { "Content-Type", "application/json" }
            },
            maskedJson);

        Exception lastErr = null;

        for (int attempt = 0; attempt <= MaxRetries; attempt++)
        {
            if (attempt > 0)
            {
                // Exponential backoff with jitter
                var jitter = 1f + (float)((_rng.NextDouble() * 2 - 1) * JitterPct);
                float delaySec = RetryBackoffBaseSeconds * Mathf.Pow(2f, attempt) * jitter;
                Log($"[HTTP] Retry (signup) #{attempt} after backoff {delaySec:0.00}s …");
                await Task.Delay(TimeSpan.FromSeconds(delaySec), ct);
            }

            using (var req = new UnityWebRequest(SignupInitiateUrl, UnityWebRequest.kHttpVerbPOST))
            {
                try
                {
                    req.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(bodyJson));
                    req.downloadHandler = new DownloadHandlerBuffer();
                    req.SetRequestHeader("accept", "*/*");
                    req.SetRequestHeader("Content-Type", "application/json");
                    req.timeout = RequestTimeoutSeconds;

                    Log($"[HTTP] POST {SignupInitiateUrl} (signup/initiate)");

                    var op = req.SendWebRequest();
                    float start = Time.realtimeSinceStartup;

                    while (!op.isDone)
                    {
                        if (ct.IsCancellationRequested) { req.Abort(); ct.ThrowIfCancellationRequested(); }
                        if (Time.realtimeSinceStartup - start > RequestTimeoutSeconds)
                        {
                            req.Abort();
                            throw new TimeoutException($"Request timed out after {RequestTimeoutSeconds}s");
                        }
                        await Task.Yield();
                    }

#if UNITY_2020_1_OR_NEWER
                    bool isNetErr = req.result == UnityWebRequest.Result.ConnectionError;
                    bool isHttpErr = req.result == UnityWebRequest.Result.ProtocolError;
#else
                bool isNetErr  = req.isNetworkError;
                bool isHttpErr = req.isHttpError;
#endif
                    long code = req.responseCode;
                    string text = req.downloadHandler?.text ?? "";
                    Log($"[HTTP] ← {code} {(string.IsNullOrEmpty(text) ? "" : text)}");

                    // Success (treat any 2xx as OK)
                    if (!isNetErr && !isHttpErr && code >= 200 && code < 300)
                    {
                        // Parse typed response (fallback to raw text if shape changes)
                        SignupInitiateResponse resp = null;
                        try { resp = JsonUtility.FromJson<SignupInitiateResponse>(text); } catch { }
                        return resp ?? new SignupInitiateResponse
                        {
                            status = true,
                            message = string.IsNullOrWhiteSpace(text) ? "OK" : text
                        };
                    }

                    // Retryable?
                    bool retryable = code == 408 || code == 429 || (code >= 500 && code <= 599) || isNetErr;

                    // If 429, respect Retry-After when present
                    if (code == 429 && attempt < MaxRetries)
                    {
                        var retryAfter = req.GetResponseHeader("Retry-After");
                        if (int.TryParse(retryAfter, out int ra) && ra > 0)
                        {
                            Log($"[HTTP] 429 Rate limited – honoring Retry-After: {ra}s");
                            await Task.Delay(TimeSpan.FromSeconds(ra), ct);
                            continue;
                        }
                    }

                    if (retryable && attempt < MaxRetries)
                        continue;

                    // Non-retryable or exhausted retries → raise with server message if any
                    string msg = string.IsNullOrWhiteSpace(text) ? req.error : text;
                    throw new Exception($"Signup initiate failed: HTTP {code} - {msg}");
                }
                catch (OperationCanceledException) { LogError("[HTTP] Signup/initiate cancelled."); throw; }
                catch (TimeoutException tex) { lastErr = tex; LogError($"[HTTP] Timeout (signup): {tex.Message}"); if (attempt >= MaxRetries) throw; }
                catch (Exception ex) { lastErr = ex; LogError($"[HTTP] Attempt (signup) #{attempt} failed: {ex.GetType().Name} – {ex.Message}"); if (attempt >= MaxRetries) throw; }
            }
        }

        throw lastErr ?? new Exception("Unknown signup/initiate error");
    }

    // === Helpers (local to this region) ===
    private static bool IsValidEmail(string email)
    {
        // Lightweight RFC-ish check; avoids allocations & false-positives
        try
        {
            // Unity players may not have System.Net.Mail available everywhere; keep simple.
            // Must have one @, at least one dot after @, no spaces.
            email = email.Trim();
            int at = email.IndexOf('@');
            if (at <= 0 || at != email.LastIndexOf('@')) return false;
            int dot = email.IndexOf('.', at + 1);
            if (dot <= at + 1 || dot == email.Length - 1) return false;
            if (email.Contains(" ")) return false;
            return true;
        }
        catch { return false; }
    }

    private static void ValidatePassword(string password)
    {
        // Sensible defaults; adjust if backend has stricter policy.
        // - >= 8 chars
        // - at least 1 upper, 1 lower, 1 digit, 1 special
        if (password.Length < 8)
            throw new ArgumentException("Password must be at least 8 characters long.", nameof(password));

        bool hasUpper = false, hasLower = false, hasDigit = false, hasSpecial = false;
        foreach (char c in password)
        {
            if (char.IsUpper(c)) hasUpper = true;
            else if (char.IsLower(c)) hasLower = true;
            else if (char.IsDigit(c)) hasDigit = true;
            else hasSpecial = true;
        }

        if (!(hasUpper && hasLower && hasDigit && hasSpecial))
            throw new ArgumentException("Password must include upper, lower, digit, and special character.", nameof(password));
    }

    private static string MaskSensitiveFields(string json, string[] fieldNames)
    {
        if (string.IsNullOrEmpty(json) || fieldNames == null || fieldNames.Length == 0) return json;

        // naive mask: "field": "anything" -> "field": "****"
        foreach (var f in fieldNames)
        {
            // handles whitespace variants:  "password"  :   "value"
            string pattern = $"(\"{Regex.Escape(f)}\"\\s*:\\s*\")([^\"]*)(\")";
            json = Regex.Replace(json, pattern, $"$1****$3");
        }
        return json;
    }

    #endregion

    #region Auth V2: Confirm Signup (Cognito + Local)

    // === Endpoint ===
    public static string SignupConfirmUrl = "https://api.intelli-verse-x.ai/api/user/auth_v_2/signup";

    // === DTOs ===
    [Serializable]
    public class SignupConfirmRequest
    {
        public string email;
        public string firstName;
        public string lastName;
        public string otp;
        public string password;
        public string userName;
        public string role;          // e.g., "user"
        public string fcmToken;      // optional
        public string fromDevice;    // e.g., "web", "android", "ios", "unity" (optional)
        public string macAddress;    // optional
        public string referralCode;  // optional
    }

    [Serializable]
    public class SignupConfirmUser
    {
        public string id;
        public string firstName;
        public string lastName;
        public string userName;
        public string email;
        public string role;
        public string idpUsername;
        public bool isAdult;
        public string walletAddress;
    }

    [Serializable]
    public class SignupConfirmData
    {
        public SignupConfirmUser user;
        public string token;         // access token (JWT)
        public string idToken;       // id token (JWT)
        public string refreshToken;  // opaque/JWE
        public int expiresIn;     // seconds
    }

    [Serializable]
    public class SignupConfirmResponse
    {
        public bool status;
        public string message;
        public SignupConfirmData data;
    }

    /// <summary>
    /// Confirms email OTP in Cognito and creates the local user.
    /// On success, optionally configures runtime user-auth with returned tokens.
    /// </summary>
    /// <param name="request">Signup payload (email, otp, password, etc.)</param>
    /// <param name="configureUserAuthOnSuccess">
    /// If true, automatically enables runtime user-auth using (idpUsername, refreshToken, token, expiresIn).
    /// </param>
    public static async Task<SignupConfirmResponse> SignupConfirmAsync(
        SignupConfirmRequest request,
        bool configureUserAuthOnSuccess = true,
        CancellationToken ct = default)
    {
        // ---- Input validation ----
        if (request == null) throw new ArgumentNullException(nameof(request));

        if (string.IsNullOrWhiteSpace(request.email))
            throw new ArgumentException("Email is required.", nameof(request.email));
        if (!IsValidEmail(request.email))
            throw new ArgumentException("Email format is invalid.", nameof(request.email));

        if (string.IsNullOrWhiteSpace(request.otp))
            throw new ArgumentException("OTP is required.", nameof(request.otp));
        // Accept 4–8 digits (relax if backend supports alphanumerics)
        if (!System.Text.RegularExpressions.Regex.IsMatch(request.otp.Trim(), @"^\d{4,8}$"))
            throw new ArgumentException("OTP must be 4–8 digits.", nameof(request.otp));

        if (string.IsNullOrWhiteSpace(request.password))
            throw new ArgumentException("Password is required.", nameof(request.password));
        ValidatePassword(request.password); // throws if weak

        if (string.IsNullOrWhiteSpace(request.userName))
            throw new ArgumentException("userName is required.", nameof(request.userName));
        // Username: 3–32, letters/digits/_/.- (feel free to relax/tighten as needed)
        if (!System.Text.RegularExpressions.Regex.IsMatch(request.userName.Trim(), @"^[A-Za-z0-9_.-]{3,32}$"))
            throw new ArgumentException("userName must be 3–32 chars (letters, digits, . _ -).", nameof(request.userName));

        request.role = string.IsNullOrWhiteSpace(request.role) ? "user" : request.role.Trim();
        request.firstName = request.firstName?.Trim() ?? "";
        request.lastName = request.lastName?.Trim() ?? "";
        request.fromDevice = string.IsNullOrWhiteSpace(request.fromDevice) ? "web" : request.fromDevice.Trim();

        if (!string.IsNullOrWhiteSpace(request.macAddress))
        {
            // Soft-validate MAC if provided
            if (!System.Text.RegularExpressions.Regex.IsMatch(request.macAddress.Trim(), @"^([0-9A-Fa-f]{2}:){5}[0-9A-Fa-f]{2}$"))
                throw new ArgumentException("macAddress must be in format 00:11:22:33:44:55.", nameof(request.macAddress));
        }

        // Prepare JSON body (real)
        string bodyJson = JsonUtility.ToJson(request);

        // Mask sensitive fields for logs / cURL
        string maskedJson = MaskSensitiveFields(bodyJson, new[] { "password", "email", "otp", "fcmToken", "refreshToken" });

        // cURL preview (masked)
        PrintCurl("POST", SignupConfirmUrl,
            new Dictionary<string, string> {
            { "accept", "*/*" },
            { "Content-Type", "application/json" }
            },
            maskedJson);

        Exception lastErr = null;

        for (int attempt = 0; attempt <= MaxRetries; attempt++)
        {
            if (attempt > 0)
            {
                var jitter = 1f + (float)((_rng.NextDouble() * 2 - 1) * JitterPct);
                float delaySec = RetryBackoffBaseSeconds * Mathf.Pow(2f, attempt) * jitter;
                Log($"[HTTP] Retry (signup confirm) #{attempt} after backoff {delaySec:0.00}s …");
                await Task.Delay(TimeSpan.FromSeconds(delaySec), ct);
            }

            using (var req = new UnityWebRequest(SignupConfirmUrl, UnityWebRequest.kHttpVerbPOST))
            {
                try
                {
                    req.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(bodyJson));
                    req.downloadHandler = new DownloadHandlerBuffer();
                    req.SetRequestHeader("accept", "*/*");
                    req.SetRequestHeader("Content-Type", "application/json");
                    req.timeout = RequestTimeoutSeconds;

                    Log($"[HTTP] POST {SignupConfirmUrl} (confirm signup)");

                    var op = req.SendWebRequest();
                    float start = Time.realtimeSinceStartup;

                    while (!op.isDone)
                    {
                        if (ct.IsCancellationRequested) { req.Abort(); ct.ThrowIfCancellationRequested(); }
                        if (Time.realtimeSinceStartup - start > RequestTimeoutSeconds)
                        {
                            req.Abort();
                            throw new TimeoutException($"Request timed out after {RequestTimeoutSeconds}s");
                        }
                        await Task.Yield();
                    }

#if UNITY_2020_1_OR_NEWER
                    bool isNetErr = req.result == UnityWebRequest.Result.ConnectionError;
                    bool isHttpErr = req.result == UnityWebRequest.Result.ProtocolError;
#else
                bool isNetErr  = req.isNetworkError;
                bool isHttpErr = req.isHttpError;
#endif
                    long code = req.responseCode;
                    string text = req.downloadHandler?.text ?? "";
                    Log($"[HTTP] ← {code} {(string.IsNullOrEmpty(text) ? "" : (text.Length > 1500 ? text.Substring(0, 1500) + "...(truncated)" : text))}");

                    // Success (2xx)
                    if (!isNetErr && !isHttpErr && code >= 200 && code < 300)
                    {
                        SignupConfirmResponse resp = null;
                        try { resp = JsonUtility.FromJson<SignupConfirmResponse>(text); } catch { }

                        // Fallback if shape changes unexpectedly
                        if (resp == null) resp = new SignupConfirmResponse { status = true, message = "OK", data = null };

                        // Optionally configure runtime user-auth for subsequent requests
                        if (configureUserAuthOnSuccess && resp.data != null)
                        {
                            string idpUser = resp.data.user != null ? resp.data.user.idpUsername : null;
                            string refresh = resp.data.refreshToken;
                            string access = resp.data.token;
                            long expiresIn = Math.Max(0, resp.data.expiresIn);

                            long nowEpoch = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
                            long accessExpiryEpoch = nowEpoch + expiresIn;

                            // Enable runtime user-auth: auto-refresh on 401s for user endpoints
                            ConfigureUserAuth(
                                useUserAuthToken: true,
                                idpUsername: idpUser,
                                refreshToken: refresh,
                                initialAccessToken: access,
                                accessTokenExpiresInEpoch: accessExpiryEpoch
                            );
                        }

                        return resp;
                    }

                    // Retryable?
                    bool retryable = code == 408 || code == 429 || (code >= 500 && code <= 599) || isNetErr;

                    // Honor Retry-After for 429
                    if (code == 429 && attempt < MaxRetries)
                    {
                        var retryAfter = req.GetResponseHeader("Retry-After");
                        if (int.TryParse(retryAfter, out int ra) && ra > 0)
                        {
                            Log($"[HTTP] 429 Rate limited – honoring Retry-After: {ra}s");
                            await Task.Delay(TimeSpan.FromSeconds(ra), ct);
                            continue;
                        }
                    }

                    if (retryable && attempt < MaxRetries)
                        continue;

                    string msg = string.IsNullOrWhiteSpace(text) ? req.error : text;
                    throw new Exception($"Confirm signup failed: HTTP {code} - {msg}");
                }
                catch (OperationCanceledException) { LogError("[HTTP] Confirm signup cancelled."); throw; }
                catch (TimeoutException tex) { lastErr = tex; LogError($"[HTTP] Timeout (confirm signup): {tex.Message}"); if (attempt >= MaxRetries) throw; }
                catch (Exception ex) { lastErr = ex; LogError($"[HTTP] Attempt (confirm signup) #{attempt} failed: {ex.GetType().Name} – {ex.Message}"); if (attempt >= MaxRetries) throw; }
            }
        }

        throw lastErr ?? new Exception("Unknown confirm-signup error");
    }

    #endregion

    #region Auth V2: Login (Cognito)

    // === Base URL ===
    public const string API_BASE_URL = "https://api.intelli-verse-x.ai";

    // === Endpoint ===
    public static string LoginUrl = "https://api.intelli-verse-x.ai/api/user/auth_v_2/login";

    // === DTOs ===
    [Serializable]
    public class LoginRequest
    {
        public string email;
        public string password;
        public string fromDevice;   // e.g. "unity" | "machine" | "android" | "ios" | "web"
        public string macAddress;   // optional
    }

    [Serializable]
    public class LoginUser
    {
        public string id;
        public string firstName;
        public string lastName;
        public string userName;
        public string email;
        public string role;
        public string idpUsername;
        public string walletAddress;
        public bool isAdult;
        public string loginType;
        public string fcmToken;
        public string kycStatus;
        public string accountStatus;
        public string createdAt;
        public string updatedAt;
        // plus any extra fields from payload; Unity’s JsonUtility ignores unknowns safely
    }

    [Serializable]
    public class LoginData
    {
        public LoginUser user;
        public string token;         // sometimes present (alias of access token)
        public string accessToken;   // preferred
        public string idToken;
        public string refreshToken;
        public int expiresIn;     // seconds
    }

    [Serializable]
    public class LoginResponse
    {
        public bool status;
        public string message;
        public LoginData data;
    }

    /// <summary>
    /// Authenticates with Cognito and returns tokens.
    /// On success:
    ///  - Configures runtime user-auth (auto refresh via your Refresh endpoint)
    ///  - Persists the full response into UserSessionManager for later use
    /// </summary>
    public static async Task<LoginResponse> LoginAsync(
        LoginRequest req,
        bool configureUserAuthOnSuccess = true,
        bool persistSession = true,
        CancellationToken ct = default)
    {
        if (req == null) throw new ArgumentNullException(nameof(req));

        if (string.IsNullOrWhiteSpace(req.email))
            throw new ArgumentException("Email is required.", nameof(req.email));
        if (!IsValidEmail(req.email))
            throw new ArgumentException("Email format is invalid.", nameof(req.email));

        if (string.IsNullOrWhiteSpace(req.password))
            throw new ArgumentException("Password is required.", nameof(req.password));

        req.fromDevice = string.IsNullOrWhiteSpace(req.fromDevice) ? "unity" : req.fromDevice.Trim();

        if (!string.IsNullOrWhiteSpace(req.macAddress))
        {
            if (!System.Text.RegularExpressions.Regex.IsMatch(req.macAddress.Trim(), @"^([0-9A-Fa-f]{2}:){5}[0-9A-Fa-f]{2}$"))
                throw new ArgumentException("macAddress must be in format 00:11:22:33:44:55.", nameof(req.macAddress));
        }

        string bodyJson = JsonUtility.ToJson(req);
        string maskedJson = MaskSensitiveFields(bodyJson, new[] { "password", "email" });

        // Masked cURL preview
        PrintCurl("POST", LoginUrl,
            new Dictionary<string, string> {
            { "accept", "*/*" },
            { "Content-Type", "application/json" }
            },
            maskedJson);

        Exception lastErr = null;

        for (int attempt = 0; attempt <= MaxRetries; attempt++)
        {
            if (attempt > 0)
            {
                var jitter = 1f + (float)((_rng.NextDouble() * 2 - 1) * JitterPct);
                float delaySec = RetryBackoffBaseSeconds * Mathf.Pow(2f, attempt) * jitter;
                Log($"[HTTP] Retry (login) #{attempt} after backoff {delaySec:0.00}s …");
                await Task.Delay(TimeSpan.FromSeconds(delaySec), ct);
            }

            using (var uwr = new UnityWebRequest(LoginUrl, UnityWebRequest.kHttpVerbPOST))
            {
                try
                {
                    uwr.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(bodyJson));
                    uwr.downloadHandler = new DownloadHandlerBuffer();
                    uwr.SetRequestHeader("accept", "*/*");
                    uwr.SetRequestHeader("Content-Type", "application/json");
                    uwr.timeout = RequestTimeoutSeconds;

                    Log($"[HTTP] POST {LoginUrl} (login)");

                    var op = uwr.SendWebRequest();
                    float start = Time.realtimeSinceStartup;

                    while (!op.isDone)
                    {
                        if (ct.IsCancellationRequested) { uwr.Abort(); ct.ThrowIfCancellationRequested(); }
                        if (Time.realtimeSinceStartup - start > RequestTimeoutSeconds)
                        {
                            uwr.Abort();
                            throw new TimeoutException($"Request timed out after {RequestTimeoutSeconds}s");
                        }
                        await Task.Yield();
                    }

#if UNITY_2020_1_OR_NEWER
                    bool isNetErr = uwr.result == UnityWebRequest.Result.ConnectionError;
                    bool isHttpErr = uwr.result == UnityWebRequest.Result.ProtocolError;
#else
                bool isNetErr  = uwr.isNetworkError;
                bool isHttpErr = uwr.isHttpError;
#endif
                    long code = uwr.responseCode;
                    string text = uwr.downloadHandler?.text ?? "";
                    Log($"[HTTP] ← {code} {(string.IsNullOrEmpty(text) ? "" : (text.Length > 1500 ? text.Substring(0, 1500) + "...(truncated)" : text))}");

                    // Success (2xx)
                    if (!isNetErr && !isHttpErr && code >= 200 && code < 300)
                    {
                        LoginResponse resp = null;
                        try { resp = JsonUtility.FromJson<LoginResponse>(text); } catch { }
                        if (resp == null) resp = new LoginResponse { status = true, message = "OK", data = null };

                        // Configure runtime user-auth so other API calls can auto-use/refresh token
                        if (configureUserAuthOnSuccess && resp.data != null)
                        {
                            string idpUser = resp.data.user != null ? resp.data.user.idpUsername : null;
                            string refresh = resp.data.refreshToken;
                            // prefer accessToken if present, else fallback to token
                            string access = !string.IsNullOrWhiteSpace(resp.data.accessToken) ? resp.data.accessToken : resp.data.token;

                            var now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
                            long expEpoch = now + Math.Max(0, resp.data.expiresIn <= 0 ? 1800 : resp.data.expiresIn);

                            ConfigureUserAuth(
                                useUserAuthToken: true,
                                idpUsername: idpUser,
                                refreshToken: refresh,
                                initialAccessToken: access,
                                accessTokenExpiresInEpoch: expEpoch
                            );
                        }

                        // Persist complete session snapshot for later reference
                        if (persistSession && resp.data != null)
                        {
                            try { UserSessionManager.SaveFromLoginResponse(resp); }
                            catch (Exception ex) { LogError($"[Session] Save failed: {ex.Message}"); }
                        }

                        return resp;
                    }

                    // Retryable?
                    bool retryable = code == 408 || code == 429 || (code >= 500 && code <= 599) || isNetErr;

                    if (code == 429 && attempt < MaxRetries)
                    {
                        var retryAfter = uwr.GetResponseHeader("Retry-After");
                        if (int.TryParse(retryAfter, out int ra) && ra > 0)
                        {
                            Log($"[HTTP] 429 Rate limited – honoring Retry-After: {ra}s");
                            await Task.Delay(TimeSpan.FromSeconds(ra), ct);
                            continue;
                        }
                    }

                    if (retryable && attempt < MaxRetries)
                        continue;

                    throw new Exception($"Login failed: HTTP {code} - {(string.IsNullOrWhiteSpace(text) ? uwr.error : text)}");
                }
                catch (OperationCanceledException) { LogError("[HTTP] Login cancelled."); throw; }
                catch (TimeoutException tex) { lastErr = tex; LogError($"[HTTP] Timeout (login): {tex.Message}"); if (attempt >= MaxRetries) throw; }
                catch (Exception ex) { lastErr = ex; LogError($"[HTTP] Attempt (login) #{attempt} failed: {ex.GetType().Name} – {ex.Message}"); if (attempt >= MaxRetries) throw; }
            }
        }

        throw lastErr ?? new Exception("Unknown login error");
    }

    #endregion

    #region Auth V2: Guest Signup

    public static string GuestSignupUrl = "https://api.intelli-verse-x.ai/api/user/auth_v_2/guest-signup";

    [Serializable] public class GuestSignupRequest { public string role; }  // usually "user"

    [Serializable]
    public class GuestSignupResponse
    {
        public bool status;
        public string message;
        // Response shape is identical to login – reuse LoginData/LoginUser
        public LoginData data;
    }

    /// <summary>
    /// Creates a guest user (anonymous) and returns tokens.
    /// On success:
    ///  - Configures runtime user-auth (auto refresh via Refresh endpoint)
    ///  - Persists the full response into UserSessionManager
    /// </summary>
    public static async Task<GuestSignupResponse> GuestSignupAsync(
        string role = "user",
        bool configureUserAuthOnSuccess = true,
        bool persistSession = true,
        CancellationToken ct = default(CancellationToken))
    {
        if (string.IsNullOrWhiteSpace(role)) role = "user";
        var reqBody = new GuestSignupRequest { role = role.Trim() };

        string bodyJson = JsonUtility.ToJson(reqBody);

        // Masked cURL preview (no secrets here, but keep consistent)
        PrintCurl(
            "POST",
            GuestSignupUrl,
            new Dictionary<string, string> {
            { "accept", "*/*" },
            { "Content-Type", "application/json" }
            },
            bodyJson
        );

        Exception lastErr = null;

        for (int attempt = 0; attempt <= MaxRetries; attempt++)
        {
            if (attempt > 0)
            {
                var jitter = 1f + (float)((_rng.NextDouble() * 2 - 1) * JitterPct);
                float delaySec = RetryBackoffBaseSeconds * Mathf.Pow(2f, attempt) * jitter;
                Log($"[HTTP] Retry (guest-signup) #{attempt} after backoff {delaySec:0.00}s …");
                await Task.Delay(TimeSpan.FromSeconds(delaySec), ct);
            }

            using (var uwr = new UnityWebRequest(GuestSignupUrl, UnityWebRequest.kHttpVerbPOST))
            {
                try
                {
                    uwr.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(bodyJson));
                    uwr.downloadHandler = new DownloadHandlerBuffer();
                    uwr.SetRequestHeader("accept", "*/*");
                    uwr.SetRequestHeader("Content-Type", "application/json");
                    uwr.timeout = RequestTimeoutSeconds;

                    Log($"[HTTP] POST {GuestSignupUrl} (guest-signup)");

                    var op = uwr.SendWebRequest();
                    float t0 = Time.realtimeSinceStartup;

                    while (!op.isDone)
                    {
                        if (ct.IsCancellationRequested) { uwr.Abort(); ct.ThrowIfCancellationRequested(); }
                        if (Time.realtimeSinceStartup - t0 > RequestTimeoutSeconds)
                        {
                            uwr.Abort();
                            throw new TimeoutException($"Request timed out after {RequestTimeoutSeconds}s");
                        }
                        await Task.Yield();
                    }

#if UNITY_2020_1_OR_NEWER
                    bool isNetErr = uwr.result == UnityWebRequest.Result.ConnectionError;
                    bool isHttpErr = uwr.result == UnityWebRequest.Result.ProtocolError;
#else
                bool isNetErr  = uwr.isNetworkError;
                bool isHttpErr = uwr.isHttpError;
#endif
                    long code = uwr.responseCode;
                    string text = uwr.downloadHandler?.text ?? "";

                    // Avoid complicated escaping in interpolated strings – build the preview plainly
                    string preview = string.IsNullOrEmpty(text)
                        ? ""
                        : (text.Length > 1500 ? text.Substring(0, 1500) + "...(truncated)" : text);
                    Log("[HTTP] <- " + code + " " + preview);

                    if (!isNetErr && !isHttpErr && code >= 200 && code < 300)
                    {
                        GuestSignupResponse resp = null;
                        try { resp = JsonUtility.FromJson<GuestSignupResponse>(text); } catch { }
                        if (resp == null) resp = new GuestSignupResponse { status = true, message = "OK", data = null };

                        // Configure runtime user-auth (so other calls use/refresh token automatically)
                        if (configureUserAuthOnSuccess && resp.data != null)
                        {
                            string idpUser = (resp.data.user != null) ? resp.data.user.idpUsername : null;
                            string refresh = resp.data.refreshToken;
                            string access = !string.IsNullOrWhiteSpace(resp.data.accessToken) ? resp.data.accessToken : resp.data.token;

                            var now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
                            long expEpoch = now + Math.Max(0, (resp.data.expiresIn <= 0 ? 1800 : resp.data.expiresIn));

                            ConfigureUserAuth(
                                useUserAuthToken: true,
                                idpUsername: idpUser,
                                refreshToken: refresh,
                                initialAccessToken: access,
                                accessTokenExpiresInEpoch: expEpoch
                            );
                        }

                        // Persist for later use
                        if (persistSession && resp.data != null)
                        {
                            try { UserSessionManager.SaveFromGuestResponse(resp); }
                            catch (Exception ex) { LogError("[Session] Save (guest) failed: " + ex.Message); }
                        }

                        return resp;
                    }

                    // Honor Retry-After for 429
                    if (code == 429 && attempt < MaxRetries)
                    {
                        var retryAfter = uwr.GetResponseHeader("Retry-After");
                        if (int.TryParse(retryAfter, out int ra) && ra > 0)
                        {
                            Log("[HTTP] 429 Rate limited – honoring Retry-After: " + ra + "s");
                            await Task.Delay(TimeSpan.FromSeconds(ra), ct);
                            continue;
                        }
                    }

                    bool retryable = code == 408 || code == 429 || (code >= 500 && code <= 599) || isNetErr;
                    if (retryable && attempt < MaxRetries) continue;

                    throw new Exception("Guest-signup failed: HTTP " + code + " - " +
                                        (string.IsNullOrWhiteSpace(text) ? uwr.error : text));
                }
                catch (OperationCanceledException)
                {
                    LogError("[HTTP] Guest-signup cancelled.");
                    throw;
                }
                catch (TimeoutException tex)
                {
                    lastErr = tex;
                    LogError("[HTTP] Timeout (guest): " + tex.Message);
                    if (attempt >= MaxRetries) throw;
                }
                catch (Exception ex)
                {
                    lastErr = ex;
                    LogError("[HTTP] Attempt (guest) #" + attempt + " failed: " + ex.GetType().Name + " – " + ex.Message);
                    if (attempt >= MaxRetries) throw;
                }
            }
        }

        throw lastErr ?? new Exception("Unknown guest-signup error");
    }

    #endregion

    #region Auth V2: Social Login

    // === Endpoint ===
    // Note: this endpoint uses "auth-v2" (hyphen), not "auth_v_2" (underscore).
    public static string SocialLoginUrl =
    "https://api.intelli-verse-x.ai/api/user/auth-v2/social/game-login";


    // === DTOs (V2 exact payload) ===
    [Serializable]
    public class PayloadSocialLoginV2
    {
        public string loginType;       // "apple", "google", etc.
        public string email;
        public string appleKey;        // Apple auth code; null/empty for google
        public string firstName;
        public string lastName;
        public string userName;
        public string profilePicture;  // optional
        public string socialId;        // optional (Apple userId / Google userId)
        public string role;            // "user"
        public string fcmToken;
        public string fromDevice;      // "ios", "android", "unity", etc.
        public string macAddress;      // extra, for your own tracking
        public string password;        // extra / unused for social

        public PayloadSocialLoginV2(
            string loginType,
            string email,
            string firstName,
            string lastName,
            string userName,
            string role,
            string fcmToken,
            string fromDevice,
            string appleKey,
            string macAddress,
            string profilePicture = null,
            string socialId = null,
            string password = null)
        {
            this.loginType = loginType;
            this.email = email;
            this.firstName = firstName;
            this.lastName = lastName;
            this.userName = userName;
            this.role = role;
            this.fcmToken = fcmToken;
            this.fromDevice = fromDevice;
            this.appleKey = appleKey;
            this.macAddress = macAddress;
            this.profilePicture = profilePicture;
            this.socialId = socialId;
            this.password = password;
        }
    }

    [Serializable]
    public class SocialLoginResponse
    {
        public bool status;
        public string message;
        public SocialLoginData data;
        public object other; // ignore if present
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

    // === Public API (Task-based) ===
    /// <summary>
    /// Social login (Apple/Google/etc) that conforms to the new API payload.
    /// - POSTs PayloadSocialLoginV2
    /// - On success, persists session (if persistSession)
    /// - Optionally configures runtime user-auth for auto refresh
    /// Returns typed <see cref="SocialLoginResponse"/> or normalized fallback.
    /// </summary>
    public static async Task<SocialLoginResponse> SocialLoginAsync(
     string loginType,
     string email,
     string firstName,
     string lastName,
     string userName,
     string password,                     // optional for social; pass null/empty if not used
     string role,
     string fcmToken,
     string appleKey,                     // required for Apple; null/empty otherwise
     bool configureUserAuthOnSuccess = true,
     bool persistSession = true,
     string fromDeviceOverride = null,    // optional override (else auto)
     string macAddressOverride = null,    // optional override (else auto)
     CancellationToken ct = default)
    {
        // ---- Input validation ----
        if (string.IsNullOrWhiteSpace(loginType))
            throw new ArgumentException("loginType is required.", nameof(loginType));

        // Resolve device info
        DeviceInfoHelper.GetLoginDeviceFields(out string fromDeviceAuto, out string macAuto);

        // fromDevice: override > auto > "unity"
        string fromDevice = string.IsNullOrWhiteSpace(fromDeviceOverride)
            ? (string.IsNullOrWhiteSpace(fromDeviceAuto) ? "unity" : fromDeviceAuto)
            : fromDeviceOverride;

        // macAddress: override > auto
        string macAddress = string.IsNullOrWhiteSpace(macAddressOverride)
            ? macAuto
            : macAddressOverride;

        // fcm fallback to mac if empty
        if (string.IsNullOrWhiteSpace(fcmToken))
            fcmToken = macAddress;

        // Build V2 game-login payload
        var payload = new PayloadSocialLoginV2(
            loginType: loginType,
            email: email,
            firstName: firstName,
            lastName: lastName,
            userName: userName,
            role: string.IsNullOrWhiteSpace(role) ? "user" : role.Trim(),
            fcmToken: fcmToken,
            fromDevice: fromDevice,
            appleKey: appleKey,
            macAddress: macAddress,
            profilePicture: null,   // TODO: pass avatar URL if/when you have it
            socialId: null,         // TODO: pass provider userId if/when you have it
            password: password      // usually null/empty for social
        );

        string bodyJson = JsonUtility.ToJson(payload);

        // Mask sensitive fields in logs
        string masked = MaskSensitiveFields(
            bodyJson,
            new[] { "password", "appleKey", "fcmToken", "refreshToken" }
        );

        // Log masked cURL
        PrintCurl(
            "POST",
            SocialLoginUrl, // make sure this is set to /api/user/auth-v2/social/game-login
            new Dictionary<string, string>
            {
            { "Accept", "application/json" },
            { "Content-Type", "application/json" }
            },
            masked
        );

        Exception lastErr = null;

        for (int attempt = 0; attempt <= MaxRetries; attempt++)
        {
            if (attempt > 0)
            {
                var jitter = 1f + (float)((_rng.NextDouble() * 2 - 1) * JitterPct);
                float delaySec = RetryBackoffBaseSeconds * Mathf.Pow(2f, attempt) * jitter;
                Log($"[HTTP] Retry (social-login) #{attempt} after backoff {delaySec:0.00}s …");
                await Task.Delay(TimeSpan.FromSeconds(delaySec), ct);
            }

            using (var req = new UnityWebRequest(SocialLoginUrl, UnityWebRequest.kHttpVerbPOST))
            {
                try
                {
                    req.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(bodyJson));
                    req.downloadHandler = new DownloadHandlerBuffer();
                    req.SetRequestHeader("Accept", "application/json");
                    req.SetRequestHeader("Content-Type", "application/json");
                    req.timeout = RequestTimeoutSeconds;

                    Log($"[HTTP] POST {SocialLoginUrl} (social-login)");

                    var op = req.SendWebRequest();
                    float start = Time.realtimeSinceStartup;

                    while (!op.isDone)
                    {
                        if (ct.IsCancellationRequested)
                        {
                            req.Abort();
                            ct.ThrowIfCancellationRequested();
                        }

                        if (Time.realtimeSinceStartup - start > RequestTimeoutSeconds)
                        {
                            req.Abort();
                            throw new TimeoutException($"Request timed out after {RequestTimeoutSeconds}s");
                        }

                        await Task.Yield();
                    }

#if UNITY_2020_1_OR_NEWER
                    bool isNetErr = req.result == UnityWebRequest.Result.ConnectionError;
                    bool isHttpErr = req.result == UnityWebRequest.Result.ProtocolError;
#else
                bool isNetErr  = req.isNetworkError;
                bool isHttpErr = req.isHttpError;
#endif
                    long code = req.responseCode;
                    string text = req.downloadHandler?.text ?? "";
                    string preview = text.Length > 1500 ? text.Substring(0, 1500) + "...(truncated)" : text;
                    Log($"[HTTP] ← {code} {preview}");

                    // Success (2xx)
                    if (!isNetErr && !isHttpErr && code >= 200 && code < 300)
                    {
                        // Persist / configure via tolerant builder (handles social + normal login shapes)
                        if (!TryBuildAndSaveSessionFromLoginResponse(
                                text,
                                out var _,
                                out var buildErr,
                                configureUserAuthOnSuccess,
                                persistSession))
                        {
                            throw new Exception($"[SocialLogin] Parse/persist failed: {buildErr ?? "Unknown shape"}");
                        }

                        // Try direct SocialLoginResponse
                        SocialLoginResponse social = null;
                        try { social = JsonUtility.FromJson<SocialLoginResponse>(text); } catch { /* ignore */ }

                        if (social != null && social.data != null)
                            return social;

                        // If backend returned normal LoginResponse, normalize into SocialLoginResponse
                        LoginResponse lr = null;
                        try { lr = JsonUtility.FromJson<LoginResponse>(text); } catch { /* ignore */ }

                        if (lr != null && lr.data != null)
                        {
                            return new SocialLoginResponse
                            {
                                status = lr.status,
                                message = lr.message,
                                data = new SocialLoginData
                                {
                                    user = lr.data.user,
                                    accessToken = string.IsNullOrWhiteSpace(lr.data.accessToken)
                                        ? lr.data.token
                                        : lr.data.accessToken,
                                    idToken = lr.data.idToken,
                                    refreshToken = lr.data.refreshToken,
                                    expiresIn = Math.Max(0, lr.data.expiresIn),
                                    isNewUser = false,
                                    requiresPasswordSetup = false
                                }
                            };
                        }

                        // Fallback: success but unknown format
                        return new SocialLoginResponse { status = true, message = "OK", data = null };
                    }

                    // Honor Retry-After for 429
                    if (code == 429 && attempt < MaxRetries)
                    {
                        var retryAfter = req.GetResponseHeader("Retry-After");
                        if (int.TryParse(retryAfter, out int ra) && ra > 0)
                        {
                            Log($"[HTTP] 429 Rate limited – honoring Retry-After: {ra}s");
                            await Task.Delay(TimeSpan.FromSeconds(ra), ct);
                            continue;
                        }
                    }

                    bool retryable = code == 408 || code == 429 || (code >= 500 && code <= 599) || isNetErr;
                    if (retryable && attempt < MaxRetries)
                        continue;

                    throw new Exception(
                        $"Social login failed: HTTP {code} - {(string.IsNullOrWhiteSpace(text) ? req.error : text)}");
                }
                catch (OperationCanceledException)
                {
                    LogError("[HTTP] Social-login cancelled.");
                    throw;
                }
                catch (TimeoutException tex)
                {
                    lastErr = tex;
                    LogError($"[HTTP] Timeout (social-login): {tex.Message}");
                    if (attempt >= MaxRetries) throw;
                }
                catch (Exception ex)
                {
                    lastErr = ex;
                    LogError($"[HTTP] Attempt (social-login) #{attempt} failed: {ex.GetType().Name} – {ex.Message}");
                    if (attempt >= MaxRetries) throw;
                }
            }
        }

        throw lastErr ?? new Exception("Unknown social-login error");
    }


    // === Public API (callback-friendly shim) ===
    /// <summary>
    /// Callback wrapper around <see cref="SocialLoginAsync"/> for parity with older code.
    /// onSuccess receives raw response; onError receives a message.
    /// </summary>
    public static async void SocialLogin(
        string loginType,
        string email,
        string firstName,
        string lastName,
        string userName,
        string password,
        string role,
        string fcmToken,
        string appleKey,
        Action<string> onSuccess,
        Action<string> onError)
    {
        try
        {
            bool persist = PlayerPrefs.GetInt("auth.remember", 0) == 1;

            // Resolve device fields
            DeviceInfoHelper.GetLoginDeviceFields(out string fromDevice, out string macAddress);
            string resolvedFromDevice = string.IsNullOrWhiteSpace(fromDevice) ? "unity" : fromDevice;
            string resolvedFcm = string.IsNullOrWhiteSpace(fcmToken) ? macAddress : fcmToken;

            var payload = new PayloadSocialLoginV2(
                email: email,
                firstName: firstName,
                lastName: lastName,
                password: password,
                userName: userName,
                role: string.IsNullOrWhiteSpace(role) ? "user" : role.Trim(),
                fcmToken: resolvedFcm,
                fromDevice: resolvedFromDevice,
                appleKey: appleKey,
                macAddress: macAddress,
                loginType: loginType
            );

            string bodyJson = JsonUtility.ToJson(payload);
            string masked = MaskSensitiveFields(bodyJson, new[] { "password", "appleKey", "fcmToken", "refreshToken" });
            PrintCurl("POST", SocialLoginUrl,
                new Dictionary<string, string> {
                { "Accept", "application/json" },
                { "Content-Type", "application/json" }
                },
                masked
            );

            Exception lastErr = null;
            for (int attempt = 0; attempt <= MaxRetries; attempt++)
            {
                if (attempt > 0)
                {
                    var jitter = 1f + (float)((_rng.NextDouble() * 2 - 1) * JitterPct);
                    float delaySec = RetryBackoffBaseSeconds * Mathf.Pow(2f, attempt) * jitter;
                    Log($"[HTTP] Retry (social shim) #{attempt} after backoff {delaySec:0.00}s …");
                    await Task.Delay(TimeSpan.FromSeconds(delaySec));
                }

                using (var req = new UnityWebRequest(SocialLoginUrl, UnityWebRequest.kHttpVerbPOST))
                {
                    try
                    {
                        req.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(bodyJson));
                        req.downloadHandler = new DownloadHandlerBuffer();
                        req.SetRequestHeader("Accept", "application/json");
                        req.SetRequestHeader("Content-Type", "application/json");
                        req.timeout = RequestTimeoutSeconds;

                        var op = req.SendWebRequest();
                        float start = Time.realtimeSinceStartup;

                        while (!op.isDone)
                        {
                            if (Time.realtimeSinceStartup - start > RequestTimeoutSeconds)
                            {
                                req.Abort();
                                throw new TimeoutException($"Request timed out after {RequestTimeoutSeconds}s");
                            }
                            await Task.Yield();
                        }

#if UNITY_2020_1_OR_NEWER
                        bool isNetErr = req.result == UnityWebRequest.Result.ConnectionError;
                        bool isHttpErr = req.result == UnityWebRequest.Result.ProtocolError;
#else
                    bool isNetErr  = req.isNetworkError;
                    bool isHttpErr = req.isHttpError;
#endif
                        long code = req.responseCode;
                        string text = req.downloadHandler?.text ?? "";
                        string preview = text.Length > 1500 ? text.Substring(0, 1500) + "...(truncated)" : text;
                        Log($"[HTTP] ← {code} {preview}");

                        if (!isNetErr && !isHttpErr && code >= 200 && code < 300)
                        {
                            if (TryBuildAndSaveSessionFromLoginResponse(
                                    text,
                                    out var s,
                                    out var buildErr,
                                    configureUserAuthOnSuccess: true,
                                    persistSession: persist))
                            {
                                Log($"[SOCIAL LOGIN] Session persisted ({loginType}). token={PreviewToken(s?.accessToken)}");
                                onSuccess?.Invoke(text);
                                return;
                            }
                            else
                            {
                                LogError($"[SOCIAL LOGIN] Could not store session: {buildErr}");
                                onError?.Invoke(buildErr ?? "Invalid social login response.");
                                return;
                            }
                        }

                        if (code == 429 && attempt < MaxRetries)
                        {
                            var retryAfter = req.GetResponseHeader("Retry-After");
                            if (int.TryParse(retryAfter, out int ra) && ra > 0)
                            {
                                Log($"[HTTP] 429 Rate limited – honoring Retry-After: {ra}s");
                                await Task.Delay(TimeSpan.FromSeconds(ra));
                                continue;
                            }
                        }

                        bool retryable = code == 408 || code == 429 || (code >= 500 && code <= 599) || isNetErr;
                        if (retryable && attempt < MaxRetries) continue;

                        onError?.Invoke($"HTTP {code}: {(string.IsNullOrWhiteSpace(text) ? req.error : text)}");
                        return;
                    }
                    catch (TimeoutException tex) { lastErr = tex; LogError($"[HTTP] Timeout (social shim): {tex.Message}"); if (attempt >= MaxRetries) { onError?.Invoke(tex.Message); return; } }
                    catch (Exception ex) { lastErr = ex; LogError($"[HTTP] Attempt (social shim) #{attempt} failed: {ex.GetType().Name} – {ex.Message}"); if (attempt >= MaxRetries) { onError?.Invoke(ex.Message); return; } }
                }
            }

            onError?.Invoke(lastErr?.Message ?? "Unknown social login error.");
        }
        catch (Exception e)
        {
            onError?.Invoke(e.Message);
        }
    }

    // === Tolerant session builder ===
    /// <summary>
    /// Accepts either the normal LoginResponse JSON OR the SocialLoginResponse JSON.
    /// - Persists to UserSessionManager
    /// - Optionally configures runtime user-auth
    /// </summary>
    public static bool TryBuildAndSaveSessionFromLoginResponse(
        string responseJson,
        out UserSessionManager.UserSession session,
        out string error,
        bool configureUserAuthOnSuccess = true,
        bool persistSession = true)
    {
        session = null;
        error = null;

        if (string.IsNullOrWhiteSpace(responseJson))
        {
            error = "Empty response.";
            return false;
        }

        try
        {
            // Try normal login shape first
            LoginResponse login = null;
            try { login = JsonUtility.FromJson<LoginResponse>(responseJson); } catch { /* ignore */ }

            if (login != null && login.data != null && login.data.user != null &&
                (!string.IsNullOrEmpty(login.data.accessToken) || !string.IsNullOrEmpty(login.data.token)))
            {
                if (persistSession)
                {
                    try { UserSessionManager.SaveFromLoginResponse(login); }
                    catch (Exception e) { error = "Persist failed: " + e.Message; return false; }
                }

                if (configureUserAuthOnSuccess)
                {
                    string idpUser = login.data.user?.idpUsername;
                    string refresh = login.data.refreshToken;
                    string access = !string.IsNullOrWhiteSpace(login.data.accessToken) ? login.data.accessToken : login.data.token;
                    long now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
                    long expEpoch = now + Math.Max(0, (login.data.expiresIn <= 0 ? 1800 : login.data.expiresIn));
                    if (!string.IsNullOrWhiteSpace(idpUser) && !string.IsNullOrWhiteSpace(refresh) && !string.IsNullOrWhiteSpace(access))
                    {
                        ConfigureUserAuth(true, idpUser, refresh, access, expEpoch);
                    }
                }

                session = UserSessionManager.Current;
                return true;
            }

            // Try social envelope
            SocialLoginResponse social = null;
            try { social = JsonUtility.FromJson<SocialLoginResponse>(responseJson); } catch { /* ignore */ }

            if (social != null && social.data != null && social.data.user != null)
            {
                // Normalize to LoginResponse shape used by UserSessionManager
                var normalized = new LoginResponse
                {
                    status = social.status,
                    message = social.message,
                    data = new LoginData
                    {
                        user = social.data.user,
                        accessToken = social.data.accessToken,
                        idToken = social.data.idToken,
                        refreshToken = social.data.refreshToken,
                        // clamp to int safely
                        expiresIn = (int)Math.Max(0, Math.Min(int.MaxValue, social.data.expiresIn))
                    }
                };

                if (persistSession)
                {
                    try { UserSessionManager.SaveFromLoginResponse(normalized); }
                    catch (Exception e) { error = "Persist failed: " + e.Message; return false; }
                }

                if (configureUserAuthOnSuccess)
                {
                    string idpUser = normalized.data.user?.idpUsername;
                    string refresh = normalized.data.refreshToken;
                    string access = !string.IsNullOrWhiteSpace(normalized.data.accessToken) ? normalized.data.accessToken : normalized.data.token;
                    long now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
                    long expEpoch = now + Math.Max(0, (normalized.data.expiresIn <= 0 ? 1800 : normalized.data.expiresIn));
                    if (!string.IsNullOrWhiteSpace(idpUser) && !string.IsNullOrWhiteSpace(refresh) && !string.IsNullOrWhiteSpace(access))
                    {
                        ConfigureUserAuth(true, idpUser, refresh, access, expEpoch);
                    }
                }

                session = UserSessionManager.Current;
                return true;
            }

            error = "Unrecognized response format.";
            return false;
        }
        catch (Exception ex)
        {
            error = "Parse error: " + ex.Message;
            return false;
        }
    }

    // === Local helpers (region-scoped) ===
    private static string PreviewToken(string token)
    {
        if (string.IsNullOrEmpty(token)) return "<null>";
        return token.Length <= 12 ? token : token.Substring(0, 6) + "..." + token.Substring(token.Length - 4);
    }

    #endregion

    #region OAuth
    private static async Task<string> EnsureTokenAsync(CancellationToken ct)
    {
        if (!string.IsNullOrEmpty(_accessToken) && DateTime.UtcNow < _tokenExpiryUtc.AddSeconds(-30)) return _accessToken;
        await _tokenLock.WaitAsync(ct);
        try
        {
            if (!string.IsNullOrEmpty(_accessToken) && DateTime.UtcNow < _tokenExpiryUtc.AddSeconds(-30)) return _accessToken;
            return await FetchAccessTokenAsync(ct);
        }
        finally { _tokenLock.Release(); }
    }

    private static async Task<string> FetchAccessTokenAsync(CancellationToken ct)
    {
        var creds = new OAuthClientCredentials { client_id = ClientId, client_secret = ClientSecret };
        string bodyJson = JsonUtility.ToJson(creds);
        using (var req = new UnityWebRequest(OAuthTokenUrl, UnityWebRequest.kHttpVerbPOST))
        {
            req.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(bodyJson));
            req.downloadHandler = new DownloadHandlerBuffer();
            req.SetRequestHeader("Content-Type", "application/json");
            req.SetRequestHeader("accept", "application/json");
            string basic = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{ClientId}:{ClientSecret}"));
            req.SetRequestHeader("Authorization", "Basic " + basic);
            //  if (!string.IsNullOrEmpty(UserAgent)) req.SetRequestHeader("User-Agent", UserAgent);
            req.timeout = RequestTimeoutSeconds;

            PrintCurl("POST", OAuthTokenUrl, new Dictionary<string, string> {
                { "Content-Type", "application/json" }, { "accept", "application/json" }, { "Authorization", "Basic ****" }
            }, bodyJson);

            var op = req.SendWebRequest();
            while (!op.isDone)
            {
                if (ct.IsCancellationRequested) { req.Abort(); ct.ThrowIfCancellationRequested(); }
                await Task.Yield();
            }

#if UNITY_2020_1_OR_NEWER
            bool ok = (req.result == UnityWebRequest.Result.Success) && req.responseCode >= 200 && req.responseCode < 300;
#else
            bool ok = !req.isNetworkError && !req.isHttpError && req.responseCode >= 200 && req.responseCode < 300;
#endif
            if (!ok) throw new Exception($"HTTP {(int)req.responseCode} : {req.error} : {req.downloadHandler?.text}");
            var text = req.downloadHandler.text ?? "{}";
            var tokenResp = JsonUtility.FromJson<OAuthTokenResponse>(text);
            if (tokenResp == null || string.IsNullOrEmpty(tokenResp.access_token)) throw new Exception("Token parse failed: " + text);
            _accessToken = tokenResp.access_token;
            var seconds = (tokenResp.expires_in > 0 ? tokenResp.expires_in : 3300);
            _tokenExpiryUtc = DateTime.UtcNow.AddSeconds(seconds);
            Log("[OAUTH] token acquired; expires_in=" + seconds + "s");
            return _accessToken;
        }
    }

    public static async Task<string> EnsureTokenForExternalUseAsync(CancellationToken ct = default) => await EnsureTokenAsync(ct);






    #endregion

    #region Refresh Token 

    public static void ConfigureUserAuth(
    bool useUserAuthToken,
    string idpUsername,
    string refreshToken,
    string initialAccessToken = null,
    long? accessTokenExpiresInEpoch = null)
    {
        _useUserAuthToken = useUserAuthToken;
        _userIdpUsername = string.IsNullOrWhiteSpace(idpUsername) ? null : idpUsername.Trim();
        _userRefreshToken = string.IsNullOrWhiteSpace(refreshToken) ? null : refreshToken.Trim();

        _userAccessToken = string.IsNullOrWhiteSpace(initialAccessToken) ? null : initialAccessToken.Trim();
        if (accessTokenExpiresInEpoch.HasValue && accessTokenExpiresInEpoch.Value > 0)
        {
            try { _userAccessExpiryUtc = DateTimeOffset.FromUnixTimeSeconds(accessTokenExpiresInEpoch.Value).UtcDateTime; }
            catch { _userAccessExpiryUtc = DateTime.UtcNow.AddMinutes(30); }
        }
        else
        {
            _userAccessExpiryUtc = DateTime.UtcNow;
        }
    }

    public static void SetUserRefreshCredentials(string idpUsername, string refreshToken)
    {
        _userIdpUsername = string.IsNullOrWhiteSpace(idpUsername) ? null : idpUsername.Trim();
        _userRefreshToken = string.IsNullOrWhiteSpace(refreshToken) ? null : refreshToken.Trim();
        _userAccessToken = null;
        _userAccessExpiryUtc = DateTime.UtcNow;
    }

    public static void ClearUserAuth()
    {
        _useUserAuthToken = false;
        _userIdpUsername = _userRefreshToken = _userAccessToken = null;
        _userAccessExpiryUtc = DateTime.MinValue;
    }

    private static bool UserAuthConfigured =>
        _useUserAuthToken && !string.IsNullOrWhiteSpace(_userIdpUsername) && !string.IsNullOrWhiteSpace(_userRefreshToken);


    private static async Task<string> EnsureUserAccessTokenAsync(CancellationToken ct)
    {
        if (!UserAuthConfigured)
            throw new InvalidOperationException("[UserAuth] idp-username and refreshToken must be configured via ConfigureUserAuth/SetUserRefreshCredentials.");

        await _userAuthLock.WaitAsync(ct);
        try
        {
            if (!string.IsNullOrEmpty(_userAccessToken) && DateTime.UtcNow < _userAccessExpiryUtc.AddSeconds(-30))
                return _userAccessToken;

            await RefreshUserTokenAsync(ct);
            if (string.IsNullOrEmpty(_userAccessToken))
                throw new Exception("[UserAuth] Refresh returned empty access token.");
            return _userAccessToken;
        }
        finally { _userAuthLock.Release(); }
    }


    public static Task RefreshUserTokenAsync(CancellationToken ct = default)
    {
        // Backwards-compat: some call sites (or older branches) used RefreshTokenInternalAsync.
        // Keep a single implementation to avoid drift.
        return RefreshTokenInternalAsync(ct);
    }

    // NOTE: Unity error reports referenced this symbol in some versions of the file.
    // Keep it to prevent "missing method" compile failures across merges.
    private static async Task RefreshTokenInternalAsync(CancellationToken ct = default)
    {
        if (!UserAuthConfigured)
            throw new InvalidOperationException("[UserAuth] idp-username and refreshToken must be configured.");

        string url = RefreshTokenUrl(_userIdpUsername);
        var reqObj = new RefreshRequest { refreshToken = _userRefreshToken };
        string json = JsonUtility.ToJson(reqObj);

        PrintCurl("POST", url, new Dictionary<string, string> {
        { "accept", "application/json" }, { "Content-Type", "application/json" }
    }, json);

        using (var req = new UnityWebRequest(url, UnityWebRequest.kHttpVerbPOST))
        {
            req.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(json));
            req.downloadHandler = new DownloadHandlerBuffer();
            req.SetRequestHeader("accept", "application/json");
            req.SetRequestHeader("Content-Type", "application/json");
            req.timeout = RequestTimeoutSeconds;

            var op = req.SendWebRequest();
            float start = Time.realtimeSinceStartup;
            while (!op.isDone)
            {
                if (ct.IsCancellationRequested) { req.Abort(); ct.ThrowIfCancellationRequested(); }
                if (Time.realtimeSinceStartup - start > RequestTimeoutSeconds)
                {
                    req.Abort();
                    throw new TimeoutException($"Request timed out after {RequestTimeoutSeconds}s");
                }
                await Task.Yield();
            }

#if UNITY_2020_1_OR_NEWER
            bool ok = (req.result == UnityWebRequest.Result.Success) && req.responseCode >= 200 && req.responseCode < 300;
#else
        bool ok = !req.isNetworkError && !req.isHttpError && req.responseCode >= 200 && req.responseCode < 300;
#endif
            string text = req.downloadHandler?.text ?? "";
            Log($"[UserAuth] refresh ← {req.responseCode} {text}");

            if (!ok) throw new Exception($"Refresh failed: HTTP {req.responseCode} - {req.error} - {text}");

            RefreshResponse resp = null;
            try { resp = JsonUtility.FromJson<RefreshResponse>(text); } catch { }
            if (resp == null || resp.data == null || string.IsNullOrEmpty(resp.data.accessToken))
                throw new Exception("[UserAuth] Unexpected refresh response: " + text);

            _userAccessToken = resp.data.accessToken?.Trim();
            if (!string.IsNullOrWhiteSpace(resp.data.refreshToken))
                _userRefreshToken = resp.data.refreshToken.Trim();

            try { _userAccessExpiryUtc = DateTimeOffset.FromUnixTimeSeconds(resp.data.accessTokenExpiresIn).UtcDateTime; }
            catch { _userAccessExpiryUtc = DateTime.UtcNow.AddMinutes(30); }
        }
    }



    #endregion

    #region AI Quiz
    public static async Task<List<AIQuizItem>> GenerateQuestionsAsync(
     string category,
     string topic,
     string difficulty,
     int count,
     CancellationToken ct = default,
     string language = null)
    {
        if (string.IsNullOrWhiteSpace(ClientId) || string.IsNullOrWhiteSpace(ClientSecret))
            throw new InvalidOperationException("APIManager.ClientId/ClientSecret not set.");

        category = string.IsNullOrWhiteSpace(category) ? "General Knowledge" : category.Trim();
        topic = string.IsNullOrWhiteSpace(topic) ? category : topic.Trim();
        difficulty = string.IsNullOrWhiteSpace(difficulty) ? "easy" : difficulty.Trim().ToLower();
        count = Mathf.Clamp(count, 1, 50);

        var reqObj = new AIQuizRequest(
            BuildPrompt(category, topic, difficulty, count, language),
            BuildReturnFormat(count),
            string.IsNullOrWhiteSpace(DefaultModel) ? "deepseek/deepseek-chat" : DefaultModel
        );

        var aggregated = new List<AIQuizItem>(count);
        var sessionSeen = new HashSet<string>(StringComparer.Ordinal);

        // ---- Local helpers (self-contained; no external dependencies needed) ----
        const int MaxResolveTries = 3;

        static bool IsAuthError(Exception ex)
        {
            var m = ex?.Message ?? string.Empty;
            return m.IndexOf("401", StringComparison.OrdinalIgnoreCase) >= 0 ||
                   m.IndexOf("unauthorized", StringComparison.OrdinalIgnoreCase) >= 0;
        }

        static bool IsUsableToken(string token) => !string.IsNullOrWhiteSpace(token);

        async Task<string> GetBearerTokenResilientlyAsync(bool forceOAuth, CancellationToken tokenCt)
        {
            // If a previous attempt told us to force OAuth, skip user token path.
            if (!forceOAuth)
            {
                for (int i = 0; i < MaxResolveTries; i++)
                {
                    try
                    {
                        var userTok = await ResolveBearerTokenAsync(null, tokenCt); // your existing resolver
                        if (IsUsableToken(userTok))
                        {
                            Log($"[Auth] Using user/provided token (attempt {i + 1}/{MaxResolveTries}).");
                            return userTok;
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
                            await RefreshUserTokenAsync(tokenCt);
                        }
                        catch (Exception rex)
                        {
                            Log($"[Auth] Refresh failed: {rex.Message}");
                        }
                    }
                }
            }

            // OAuth client-credentials fallback (thread-safe Acquire)
            Log("[Auth] Falling back to OAuth client-credentials token.");
            var oauth = await EnsureTokenAsync(tokenCt);
            if (!IsUsableToken(oauth))
                throw new Exception("OAuth fallback token acquisition returned empty token.");
            return oauth;
        }

        async Task FetchOnceOrThrow()
        {
            Exception lastErr = null;
            bool forceOAuthNextAttempt = false;

            for (int attempt = 0; attempt <= MaxRetries; attempt++)
            {
                if (attempt > 0)
                {
                    var jitter = 1f + (float)((_rng.NextDouble() * 2 - 1) * JitterPct);
                    float delayS = RetryBackoffBaseSeconds * Mathf.Pow(2f, attempt) * jitter;
                    Log($"[AI QUIZ] Retry #{attempt} after backoff {delayS:0.00}s …");
                    await Task.Delay(TimeSpan.FromSeconds(delayS), ct);
                }

                try
                {
                    // One call handles user token -> refresh -> OAuth fallback.
                    var bearer = await GetBearerTokenResilientlyAsync(forceOAuthNextAttempt, ct);

                    var raw = await PostJsonAsync(
                        AIPromptUrl,
                        JsonUtility.ToJson(reqObj),
                        $"Bearer {bearer}",
                        /*logBody*/ false,
                        ct);

                    // Try batch first
                    var batch = TryParseBatch(raw, category, difficulty, ct);
                    if (batch != null && batch.Count > 0)
                    {
                        AppendValidated(aggregated, sessionSeen, batch, count);
                        return;
                    }

                    // Try single
                    var single = TryParseSingle(raw, category, difficulty);
                    if (single != null)
                    {
                        AppendValidated(aggregated, sessionSeen, new List<AIQuizItem> { single }, count);
                        return;
                    }

                    // Try extracting inner JSON once (providers that wrap JSON in text)
                    var inner = TryExtractInnerJson(raw);
                    if (!string.Equals(inner, raw, StringComparison.Ordinal))
                    {
                        var batch2 = TryParseBatch(inner, category, difficulty, ct);
                        if (batch2 != null && batch2.Count > 0)
                        {
                            AppendValidated(aggregated, sessionSeen, batch2, count);
                            return;
                        }

                        var single2 = TryParseSingle(inner, category, difficulty);
                        if (single2 != null)
                        {
                            AppendValidated(aggregated, sessionSeen, new List<AIQuizItem> { single2 }, count);
                            return;
                        }
                    }

                    throw new Exception("Unrecognized AI response shape.");
                }
                catch (OperationCanceledException) { throw; }
                catch (Exception ex)
                {
                    lastErr = ex;

                    // If it smells like auth, force next attempt to go straight to OAuth token.
                    if (IsAuthError(ex))
                    {
                        Log("[AI QUIZ] Auth error detected; forcing OAuth token on next attempt.");
                        forceOAuthNextAttempt = true;
                    }

                    if (attempt < MaxRetries) continue;
                    throw;
                }
            }

            if (lastErr != null) throw lastErr;
            throw new Exception("Fetch failed without specific error.");
        }

        // ---- First fetch ----
        await FetchOnceOrThrow();

        // Keep fetching until we have 'count' unique items
        while (aggregated.Count < count)
        {
            ct.ThrowIfCancellationRequested();
            await Task.Delay(TimeSpan.FromSeconds(BetweenCallsDelaySeconds), ct);
            await FetchOnceOrThrow();
        }

        if (aggregated.Count > count)
            aggregated.RemoveRange(count, aggregated.Count - count);

        Log($"[AI QUIZ] OK - returning {aggregated.Count} items.");
        return aggregated;
    }


    #endregion

    #region Notes API
    public static async Task<NotesCreateResponse> CreateNotesJobAsync(
     string filePath,
     string type,
     string difficulty,
     string bearerToken,
     CancellationToken ct = default,
     string language = null) // <- keep parameter for future use
    {
        if (string.IsNullOrEmpty(filePath) || !System.IO.File.Exists(filePath))
            throw new ArgumentException("File path is null or does not exist.", nameof(filePath));

        string ext = System.IO.Path.GetExtension(filePath)?.ToLowerInvariant();
        string autoType = (ext == ".docx") ? "docx" : (ext == ".pdf") ? "pdf" : null;
        type = string.IsNullOrWhiteSpace(type) ? (autoType ?? "pdf") : type.Trim().ToLowerInvariant();

        difficulty = string.IsNullOrWhiteSpace(difficulty) ? "beginner" : difficulty.Trim().ToLowerInvariant();
        if (difficulty != "beginner" && difficulty != "intermediate" && difficulty != "advanced")
            difficulty = "beginner";

        string mime = (ext == ".pdf") ? "application/pdf"
                   : (ext == ".docx") ? "application/vnd.openxmlformats-officedocument.wordprocessingml.document"
                   : "application/octet-stream";

        string fileName = System.IO.Path.GetFileName(filePath);
        byte[] fileBytes = System.IO.File.ReadAllBytes(filePath);

        // -------- Build form (NO language field in body) --------
        var form = new WWWForm();
        form.AddField("type", type);
        form.AddField("difficulty", difficulty);
        form.AddBinaryData("file", fileBytes, fileName, mime);

        // -------- cURL debug (mask auth) --------
        var curlHeaders = new Dictionary<string, string>
    {
        { "accept", "application/json" },
        { "Authorization", "Bearer ****" }
    };
        if (!string.IsNullOrWhiteSpace(language))
        {
            // Express language preference via header instead of body field
            curlHeaders["Accept-Language"] = language.Trim();
        }

        var parts = new List<(string name, string value, bool isFile, string filePathPart, string contentType)>
    {
        ("type", type, false, null, null),
        ("difficulty", difficulty, false, null, null),
        ("file", null, true, filePath, mime)
    };

        PrintCurlMultipart(NotesCreateUrl, curlHeaders, parts);

        // -------- Local auth helpers (self-contained) --------
        const int MaxResolveTries = 3;

        static bool IsAuthError(Exception ex)
        {
            var m = ex?.Message ?? string.Empty;
            return m.IndexOf("401", StringComparison.OrdinalIgnoreCase) >= 0 ||
                   m.IndexOf("unauthorized", StringComparison.OrdinalIgnoreCase) >= 0;
        }

        static bool IsUsableJwt(string token)
        {
            if (string.IsNullOrWhiteSpace(token)) return false;

            // If token doesn't look like a JWT, treat as usable (opaque tokens supported).
            int dots = 0;
            foreach (var c in token)
                if (c == '.') dots++;
            if (dots != 2) return true;

            try
            {
                string Pad(string s)
                {
                    s = s.Replace('-', '+').Replace('_', '/');
                    switch (s.Length % 4)
                    {
                        case 2: s += "=="; break;
                        case 3: s += "="; break;
                    }
                    return s;
                }

                var parts = token.Split('.');
                var payloadJson = Encoding.UTF8.GetString(Convert.FromBase64String(Pad(parts[1])));
                var expIdx = payloadJson.IndexOf("\"exp\":", StringComparison.OrdinalIgnoreCase);
                if (expIdx < 0) return true;

                var tail = payloadJson.Substring(expIdx + 6);
                var end = tail.IndexOfAny(new[] { ',', '}', ' ' });
                var numStr = end >= 0 ? tail.Substring(0, end) : tail;
                if (!long.TryParse(new string(numStr.Where(char.IsDigit).ToArray()), out var exp)) return true;

                var expUtc = DateTimeOffset.FromUnixTimeSeconds(exp).UtcDateTime;
                return DateTime.UtcNow < expUtc.AddSeconds(-30);
            }
            catch
            {
                return true;
            }
        }

        async Task<string> GetBearerTokenResilientlyAsync(string provided, bool forceOAuth, CancellationToken tokenCt)
        {
            if (!forceOAuth)
            {
                // 1) If caller provided a token and it's usable, use it first.
                if (!string.IsNullOrWhiteSpace(provided) && IsUsableJwt(provided))
                {
                    Log("[Auth] Using explicit provided bearer token.");
                    return provided;
                }

                // 2) Resolve user token (with refresh & retries)
                for (int i = 0; i < MaxResolveTries; i++)
                {
                    try
                    {
                        var userTok = await ResolveBearerTokenAsync(provided, tokenCt); // your resolver
                        if (IsUsableJwt(userTok))
                        {
                            Log($"[Auth] Using user/provided token (attempt {i + 1}/{MaxResolveTries}).");
                            return userTok;
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
                            await RefreshUserTokenAsync(tokenCt);
                        }
                        catch (Exception rex)
                        {
                            Log($"[Auth] Refresh failed: {rex.Message}");
                        }
                    }
                }
            }

            // 3) OAuth client-credentials fallback (thread-safe)
            Log("[Auth] Falling back to OAuth client-credentials token.");
            var oauth = await EnsureTokenAsync(tokenCt);
            if (string.IsNullOrWhiteSpace(oauth))
                throw new Exception("OAuth fallback token acquisition returned empty token.");
            return oauth;
        }

        // -------- Main request loop --------
        Exception lastErr = null;
        bool forceOAuthNextAttempt = false;

        for (int attempt = 0; attempt <= MaxRetries; attempt++)
        {
            if (attempt > 0)
            {
                var jitter = 1f + (float)((_rng.NextDouble() * 2 - 1) * JitterPct);
                float delaySec = RetryBackoffBaseSeconds * Mathf.Pow(2f, attempt) * jitter;
                Log($"[HTTP] Retry (multipart) #{attempt} after backoff {delaySec:0.00}s …");
                await Task.Delay(TimeSpan.FromSeconds(delaySec), ct);
            }

            UnityWebRequest req = null;
            try
            {
                var authToken = await GetBearerTokenResilientlyAsync(bearerToken, forceOAuthNextAttempt, ct);

#if UNITY_EDITOR
                var sub = TryGetJwtSub(authToken);
                if (!string.IsNullOrEmpty(sub))
                    Log($"[Auth] JWT sub: {sub}  (GUID? {LooksLikeGuid(sub)})");
#endif

                req = UnityWebRequest.Post(NotesCreateUrl, form);

#if !UNITY_2022_3_OR_NEWER
            // Some proxies/backends reject chunked multipart; send Content-Length instead.
            req.chunkedTransfer = false;
#endif

                req.SetRequestHeader("Accept", "application/json");
                req.SetRequestHeader("Authorization", "Bearer " + authToken);

                // Express language via header (server currently rejects body property "language")
                if (!string.IsNullOrWhiteSpace(language))
                    req.SetRequestHeader("Accept-Language", language.Trim());

                req.timeout = RequestTimeoutSeconds;

                Log($"[HTTP] POST {NotesCreateUrl} (multipart) file={fileName}, type={type}, difficulty={difficulty}");

                var op = req.SendWebRequest();
                float start = Time.realtimeSinceStartup;

                while (!op.isDone)
                {
                    if (ct.IsCancellationRequested)
                    {
                        req.Abort();
                        ct.ThrowIfCancellationRequested();
                    }
                    if (Time.realtimeSinceStartup - start > RequestTimeoutSeconds)
                    {
                        req.Abort();
                        throw new TimeoutException($"Request timed out after {RequestTimeoutSeconds}s");
                    }
                    await Task.Yield();
                }

                long code = req.responseCode;
                string text = req.downloadHandler?.text ?? "";
                Log($"[HTTP] ← {code} {text}");

#if UNITY_2020_1_OR_NEWER
                bool isSuccess = (req.result == UnityWebRequest.Result.Success) && code >= 200 && code < 300;
#else
            bool isSuccess = !req.isNetworkError && !req.isHttpError && code >= 200 && code < 300;
#endif

                if (isSuccess)
                {
                    Exception parseEx;
                    var resp = TryFromJson<NotesCreateResponse>(text, out parseEx)
                               ?? new NotesCreateResponse { jobId = "", status = "unknown", message = text };

                    // If server says failed or invalid input, bubble payload so UI can show message.
                    var lower = text.ToLowerInvariant();
                    if (lower.Contains("\"status\":\"failed\"") || lower.Contains("invalid input syntax for type uuid"))
                    {
                        return resp;
                    }

                    return resp;
                }

                // Auth: force OAuth next attempt on 401
                if (code == 401)
                {
                    Log("[HTTP] 401 on notes upload; forcing OAuth token on next attempt.");
                    forceOAuthNextAttempt = true;

                    // Re-acquire OAuth immediately once if we already were using OAuth
                    if (attempt < MaxRetries)
                    {
                        try { await EnsureTokenAsync(ct); } catch { /* ignore */ }
                        continue;
                    }
                }

                // Handle retryable statuses
                if (code == 429 && attempt < MaxRetries)
                {
                    var retryAfter = req.GetResponseHeader("Retry-After");
                    if (int.TryParse(retryAfter, out int secs))
                    {
                        Log($"[HTTP] 429 Too Many Requests. Retrying after {secs}s …");
                        await Task.Delay(TimeSpan.FromSeconds(secs), ct);
                        continue;
                    }
                }

                bool retryable = code == 408 || code == 423 || code == 425 || (code >= 500 && code <= 599);
                if (retryable && attempt < MaxRetries) continue;

                if ((code >= 400 && code < 500) && code != 408 && code != 429)
                    throw new Exception($"Upload failed (client error): HTTP {code} - {req.error} - {text}");

                throw new Exception($"Upload failed: HTTP {code} - {req.error} - {text}");
            }
            catch (OperationCanceledException)
            {
                req?.Abort();
                LogError("[HTTP] Multipart upload cancelled.");
                throw;
            }
            catch (TimeoutException tex)
            {
                lastErr = tex;
                LogError($"[HTTP] Timeout: {tex.Message}");
                if (attempt >= MaxRetries) throw;
            }
            catch (Exception ex)
            {
                lastErr = ex;
                LogError($"[HTTP] Attempt (multipart) #{attempt} failed: {ex.GetType().Name} – {ex.Message}");
                if (IsAuthError(ex))
                {
                    Log("[Auth] Auth-like error detected; will force OAuth on next attempt.");
                    forceOAuthNextAttempt = true;
                }
                if (attempt >= MaxRetries) throw;
            }
            finally
            {
                req?.Dispose();
            }
        }

        throw lastErr ?? new Exception("Unknown multipart upload error");
    }

    public static async Task<NotesJobStatusResponse> GetNotesJobStatusAsync(
    string jobId,
    string bearerToken,
    CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(jobId))
            throw new ArgumentException("jobId is required.", nameof(jobId));

        string url = NotesJobStatusUrl(jobId);

        PrintCurl("GET", url, new Dictionary<string, string> {
        { "Accept", "application/json" }, { "Authorization", "Bearer ****" }
    }, null);

        // -------- Local auth helpers (self-contained) --------
        const int MaxResolveTries = 3;

        static bool IsAuthError(Exception ex)
        {
            var m = ex?.Message ?? string.Empty;
            return m.IndexOf("401", StringComparison.OrdinalIgnoreCase) >= 0 ||
                   m.IndexOf("unauthorized", StringComparison.OrdinalIgnoreCase) >= 0;
        }

        static bool IsUsableJwt(string token)
        {
            if (string.IsNullOrWhiteSpace(token)) return false;
            // If it doesn't look like a JWT, treat as usable (opaque tokens supported).
            int dots = 0; foreach (var c in token) if (c == '.') dots++;
            if (dots != 2) return true;

            try
            {
                string Pad(string s) { s = s.Replace('-', '+').Replace('_', '/'); switch (s.Length % 4) { case 2: s += "=="; break; case 3: s += "="; break; } return s; }
                var parts = token.Split('.');
                var payloadJson = Encoding.UTF8.GetString(Convert.FromBase64String(Pad(parts[1])));
                var expIdx = payloadJson.IndexOf("\"exp\":", StringComparison.OrdinalIgnoreCase);
                if (expIdx < 0) return true;
                var tail = payloadJson.Substring(expIdx + 6);
                var end = tail.IndexOfAny(new[] { ',', '}', ' ' });
                var numStr = end >= 0 ? tail.Substring(0, end) : tail;
                if (!long.TryParse(new string(numStr.Where(char.IsDigit).ToArray()), out var exp)) return true;
                var expUtc = DateTimeOffset.FromUnixTimeSeconds(exp).UtcDateTime;
                return DateTime.UtcNow < expUtc.AddSeconds(-30);
            }
            catch { return true; }
        }

        async Task<string> GetBearerTokenResilientlyAsync(string provided, bool forceOAuth, CancellationToken tokenCt)
        {
            if (!forceOAuth)
            {
                // 1) If caller provided a token and it's usable, use it.
                if (!string.IsNullOrWhiteSpace(provided) && IsUsableJwt(provided))
                {
                    Log("[Auth] Using explicit provided bearer token.");
                    return provided;
                }

                // 2) Resolve user token (with refresh & retries).
                for (int i = 0; i < MaxResolveTries; i++)
                {
                    try
                    {
                        var userTok = await ResolveBearerTokenAsync(provided, tokenCt); // your resolver
                        if (IsUsableJwt(userTok))
                        {
                            Log($"[Auth] Using user/provided token (attempt {i + 1}/{MaxResolveTries}).");
                            return userTok;
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
                            await RefreshUserTokenAsync(tokenCt);
                        }
                        catch (Exception rex)
                        {
                            Log($"[Auth] Refresh failed: {rex.Message}");
                        }
                    }
                }
            }

            // 3) OAuth client-credentials fallback (thread-safe)
            Log("[Auth] Falling back to OAuth client-credentials token.");
            var oauth = await EnsureTokenAsync(tokenCt);
            if (string.IsNullOrWhiteSpace(oauth))
                throw new Exception("OAuth fallback token acquisition returned empty token.");
            return oauth;
        }

        // -------- Main request loop --------
        Exception lastErr = null;
        bool forceOAuthNextAttempt = false;

        for (int attempt = 0; attempt <= MaxRetries; attempt++)
        {
            if (attempt > 0)
            {
                var jitter = 1f + (float)((_rng.NextDouble() * 2 - 1) * JitterPct);
                float delayS = RetryBackoffBaseSeconds * Mathf.Pow(2f, attempt) * jitter;
                Log($"[HTTP] Retry (status) #{attempt} after backoff {delayS:0.00}s …");
                await Task.Delay(TimeSpan.FromSeconds(delayS), ct);
            }

            using (var req = UnityWebRequest.Get(url))
            {
                try
                {
                    var authToken = await GetBearerTokenResilientlyAsync(bearerToken, forceOAuthNextAttempt, ct);

#if UNITY_EDITOR
                    var sub = TryGetJwtSub(authToken);
                    if (!string.IsNullOrEmpty(sub))
                        Log($"[Auth] JWT sub: {sub}  (GUID? {LooksLikeGuid(sub)})");
#endif

                    req.SetRequestHeader("Accept", "application/json");
                    req.SetRequestHeader("Authorization", "Bearer " + authToken);
                    req.timeout = RequestTimeoutSeconds;

                    Log($"[HTTP] GET {url} (status)");

                    var op = req.SendWebRequest();
                    float start = Time.realtimeSinceStartup;

                    while (!op.isDone)
                    {
                        if (ct.IsCancellationRequested) { req.Abort(); ct.ThrowIfCancellationRequested(); }
                        if (Time.realtimeSinceStartup - start > RequestTimeoutSeconds)
                        {
                            req.Abort();
                            throw new TimeoutException($"Request timed out after {RequestTimeoutSeconds}s");
                        }
                        await Task.Yield();
                    }

                    long code = req.responseCode;
                    string text = req.downloadHandler?.text ?? "";
                    Log($"[HTTP] ← {code} {text}");

#if UNITY_2020_1_OR_NEWER
                    bool ok = (req.result == UnityWebRequest.Result.Success) && code >= 200 && code < 300;
#else
                bool ok = !req.isHttpError && !req.isNetworkError && code >= 200 && code < 300;
#endif

                    if (ok)
                    {
                        Exception parseEx;
                        var resp = TryFromJson<NotesJobStatusResponse>(text, out parseEx)
                                   ?? new NotesJobStatusResponse { jobId = jobId, status = "", message = text };

                        // Return parsed payload even if server indicates failure—UI can inspect status/message.
                        return resp;
                    }

                    // Auth: force OAuth next attempt on 401
                    if (code == 401)
                    {
                        Log("[HTTP] 401 on status; forcing OAuth token on next attempt.");
                        forceOAuthNextAttempt = true;

                        if (attempt < MaxRetries)
                        {
                            // If we were already on OAuth, reacquire once and retry.
                            try { await EnsureTokenAsync(ct); } catch { /* ignore */ }
                            continue;
                        }
                    }

                    // Respect Retry-After for 429
                    if (code == 429 && attempt < MaxRetries)
                    {
                        var retryAfter = req.GetResponseHeader("Retry-After");
                        if (int.TryParse(retryAfter, out int secs))
                        {
                            Log($"[HTTP] 429 Too Many Requests. Retrying after {secs}s …");
                            await Task.Delay(TimeSpan.FromSeconds(secs), ct);
                            continue;
                        }
                    }

                    bool retryable = code == 408 || code == 423 || code == 425 || (code >= 500 && code <= 599);
                    if (retryable && attempt < MaxRetries) continue;

                    if ((code >= 400 && code < 500) && code != 408 && code != 429)
                        throw new Exception($"Status failed (client error): HTTP {code} - {req.error} - {text}");

                    throw new Exception($"Status failed: HTTP {code} - {req.error} - {text}");
                }
                catch (OperationCanceledException) { LogError("[HTTP] Status request cancelled."); throw; }
                catch (TimeoutException tex) { lastErr = tex; LogError($"[HTTP] Timeout (status): {tex.Message}"); if (attempt >= MaxRetries) throw; }
                catch (Exception ex)
                {
                    lastErr = ex;
                    LogError($"[HTTP] Attempt (status) #{attempt} failed: {ex.GetType().Name} – {ex.Message}");
                    if (IsAuthError(ex))
                    {
                        Log("[Auth] Auth-like error detected; will force OAuth on next attempt.");
                        forceOAuthNextAttempt = true;
                    }
                    if (attempt >= MaxRetries) throw;
                }
            }
        }

        throw lastErr ?? new Exception("Unknown status request error");
    }

    /// <summary>
    /// Unified note creation supporting all note types (file, URL, text).
    /// Supports: pdf, docx, pptx, xls/xlsx, csv, audio, video, image, srt, text, chatgpt, youtube, website
    /// </summary>
    public static async Task<NotesCreateResponse> CreateNoteUnifiedAsync(
        CreateNoteRequest request,
        string bearerToken = null,
        CancellationToken ct = default,
        string language = null)
    {
        if (request == null)
            throw new ArgumentNullException(nameof(request));

        string typeStr = request.type.ToString().ToLowerInvariant();
        string difficulty = string.IsNullOrWhiteSpace(request.difficulty) ? "beginner" : request.difficulty.Trim().ToLowerInvariant();
        if (difficulty != "beginner" && difficulty != "intermediate" && difficulty != "advanced")
            difficulty = "beginner";

        // Validate input based on type
        ValidateNoteInput(request);

        // Build form
        var form = new WWWForm();
        form.AddField("type", typeStr);
        form.AddField("difficulty", difficulty);
        
        // Skip duplicate check to allow reprocessing same URLs
        form.AddField("skipDuplicateCheck", "true");

        if (!string.IsNullOrEmpty(request.title))
            form.AddField("title", request.title);
        if (!string.IsNullOrEmpty(request.folderId))
            form.AddField("folderId", request.folderId);
        
        // Add language support for multilingual content extraction and generation
        string finalLanguage = !string.IsNullOrEmpty(request.language) ? request.language : (language ?? "en");
        form.AddField("language", finalLanguage);
        
        // Auto-generate flashcards and quiz (default: true for seamless experience)
        form.AddField("autoGenerateStudyMaterials", request.autoGenerateStudyMaterials ? "true" : "false");

        // Handle different input types
        bool hasFile = !string.IsNullOrEmpty(request.filePath) && System.IO.File.Exists(request.filePath);

        if (hasFile)
        {
            string fileName = System.IO.Path.GetFileName(request.filePath);
            byte[] fileBytes = System.IO.File.ReadAllBytes(request.filePath);
            string mime = GetMimeType(request.filePath, request.type);
            form.AddBinaryData("file", fileBytes, fileName, mime);
        }
        else if (request.type == NoteType.youtube)
        {
            string ytUrl = !string.IsNullOrEmpty(request.youtubeUrl) ? request.youtubeUrl : request.url;
            if (string.IsNullOrEmpty(ytUrl))
                throw new ArgumentException("YouTube URL is required for youtube type.");
            form.AddField("youtubeUrl", ytUrl);
        }
        else if (request.type == NoteType.website)
        {
            if (string.IsNullOrEmpty(request.url))
                throw new ArgumentException("URL is required for website type.");
            form.AddField("url", request.url);
        }
        else if (request.type == NoteType.gdrive)
        {
            if (string.IsNullOrEmpty(request.url))
                throw new ArgumentException("Google Drive URL is required for gdrive type.");
            form.AddField("url", request.url);
        }
        else if (request.type == NoteType.text || request.type == NoteType.chatgpt)
        {
            if (string.IsNullOrEmpty(request.text))
                throw new ArgumentException("Text content is required for text/chatgpt type.");
            form.AddField("text", request.text);
        }
        else if (!string.IsNullOrEmpty(request.s3Url))
        {
            form.AddField("s3Url", request.s3Url);
        }
        else if (!string.IsNullOrEmpty(request.url))
        {
            form.AddField("url", request.url);
        }

        // Use the shared upload logic
        return await UploadNotesFormAsync(form, typeStr, difficulty, bearerToken, ct, language);
    }

    private static void ValidateNoteInput(CreateNoteRequest request)
    {
        bool hasFile = !string.IsNullOrEmpty(request.filePath) && System.IO.File.Exists(request.filePath);
        bool hasUrl = !string.IsNullOrEmpty(request.url);
        bool hasS3Url = !string.IsNullOrEmpty(request.s3Url);
        bool hasYoutubeUrl = !string.IsNullOrEmpty(request.youtubeUrl);
        bool hasText = !string.IsNullOrEmpty(request.text);

        switch (request.type)
        {
            case NoteType.pdf:
            case NoteType.docx:
            case NoteType.pptx:
            case NoteType.xls:
            case NoteType.xlsx:
            case NoteType.csv:
            case NoteType.audio:
            case NoteType.video:
            case NoteType.image:
            case NoteType.srt:
                if (!hasFile && !hasUrl && !hasS3Url)
                    throw new ArgumentException($"File, URL, or S3 URL is required for {request.type} type.");
                break;

            case NoteType.youtube:
                if (!hasYoutubeUrl && !hasUrl)
                    throw new ArgumentException("YouTube URL is required for youtube type.");
                break;

            case NoteType.website:
                if (!hasUrl)
                    throw new ArgumentException("URL is required for website type.");
                break;

            case NoteType.gdrive:
                if (!hasUrl)
                    throw new ArgumentException("Google Drive/Docs URL is required for gdrive type.");
                break;

            case NoteType.text:
            case NoteType.chatgpt:
                if (!hasText)
                    throw new ArgumentException("Text content is required for text/chatgpt type.");
                break;
        }
    }

    private static string GetMimeType(string filePath, NoteType type)
    {
        string ext = System.IO.Path.GetExtension(filePath)?.ToLowerInvariant() ?? "";
        
        return ext switch
        {
            ".pdf" => "application/pdf",
            ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
            ".doc" => "application/msword",
            ".pptx" => "application/vnd.openxmlformats-officedocument.presentationml.presentation",
            ".ppt" => "application/vnd.ms-powerpoint",
            ".xlsx" => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            ".xls" => "application/vnd.ms-excel",
            ".csv" => "text/csv",
            ".mp3" => "audio/mpeg",
            ".wav" => "audio/wav",
            ".m4a" => "audio/m4a",
            ".ogg" => "audio/ogg",
            ".flac" => "audio/flac",
            ".mp4" => "video/mp4",
            ".mov" => "video/quicktime",
            ".avi" => "video/x-msvideo",
            ".mkv" => "video/x-matroska",
            ".webm" => "video/webm",
            ".jpg" or ".jpeg" => "image/jpeg",
            ".png" => "image/png",
            ".gif" => "image/gif",
            ".webp" => "image/webp",
            ".heic" => "image/heic",
            ".srt" => "text/plain",
            ".txt" => "text/plain",
            _ => "application/octet-stream"
        };
    }

    private static async Task<NotesCreateResponse> UploadNotesFormAsync(
        WWWForm form,
        string type,
        string difficulty,
        string bearerToken,
        CancellationToken ct,
        string language)
    {
        // Local auth helpers
        const int MaxResolveTries = 3;

        static bool IsAuthError(Exception ex)
        {
            var m = ex?.Message ?? string.Empty;
            return m.IndexOf("401", StringComparison.OrdinalIgnoreCase) >= 0 ||
                   m.IndexOf("unauthorized", StringComparison.OrdinalIgnoreCase) >= 0;
        }

        static bool IsUsableJwt(string token)
        {
            if (string.IsNullOrWhiteSpace(token)) return false;
            int dots = 0;
            foreach (var c in token) if (c == '.') dots++;
            if (dots != 2) return true;

            try
            {
                string Pad(string s)
                {
                    s = s.Replace('-', '+').Replace('_', '/');
                    switch (s.Length % 4)
                    {
                        case 2: s += "=="; break;
                        case 3: s += "="; break;
                    }
                    return s;
                }

                var parts = token.Split('.');
                var payloadJson = Encoding.UTF8.GetString(Convert.FromBase64String(Pad(parts[1])));
                var expIdx = payloadJson.IndexOf("\"exp\":", StringComparison.OrdinalIgnoreCase);
                if (expIdx < 0) return true;

                var tail = payloadJson.Substring(expIdx + 6);
                var end = tail.IndexOfAny(new[] { ',', '}', ' ' });
                var numStr = end >= 0 ? tail.Substring(0, end) : tail;
                if (!long.TryParse(new string(numStr.Where(char.IsDigit).ToArray()), out var exp)) return true;

                var expUtc = DateTimeOffset.FromUnixTimeSeconds(exp).UtcDateTime;
                return DateTime.UtcNow < expUtc.AddSeconds(-30);
            }
            catch
            {
                return true;
            }
        }

        async Task<string> GetBearerTokenResilientlyAsync(string provided, bool forceOAuth, CancellationToken tokenCt)
        {
            if (!forceOAuth)
            {
                if (!string.IsNullOrWhiteSpace(provided) && IsUsableJwt(provided))
                {
                    Log("[Auth] Using explicit provided bearer token.");
                    return provided;
                }

                for (int i = 0; i < MaxResolveTries; i++)
                {
                    try
                    {
                        var userTok = await ResolveBearerTokenAsync(provided, tokenCt);
                        if (IsUsableJwt(userTok))
                        {
                            Log($"[Auth] Using user/provided token (attempt {i + 1}/{MaxResolveTries}).");
                            return userTok;
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
                            await RefreshUserTokenAsync(tokenCt);
                        }
                        catch (Exception rex)
                        {
                            Log($"[Auth] Refresh failed: {rex.Message}");
                        }
                    }
                }
            }

            Log("[Auth] Falling back to OAuth client-credentials token.");
            var oauth = await EnsureTokenAsync(tokenCt);
            if (string.IsNullOrWhiteSpace(oauth))
                throw new Exception("OAuth fallback token acquisition returned empty token.");
            return oauth;
        }

        // Main request loop
        Exception lastErr = null;
        bool forceOAuthNextAttempt = false;

        for (int attempt = 0; attempt <= MaxRetries; attempt++)
        {
            if (attempt > 0)
            {
                var jitter = 1f + (float)((_rng.NextDouble() * 2 - 1) * JitterPct);
                float delaySec = RetryBackoffBaseSeconds * Mathf.Pow(2f, attempt) * jitter;
                Log($"[HTTP] Retry (unified) #{attempt} after backoff {delaySec:0.00}s …");
                await Task.Delay(TimeSpan.FromSeconds(delaySec), ct);
            }

            UnityWebRequest req = null;
            try
            {
                var authToken = await GetBearerTokenResilientlyAsync(bearerToken, forceOAuthNextAttempt, ct);
                req = UnityWebRequest.Post(NotesCreateUrl, form);

#if !UNITY_2022_3_OR_NEWER
                req.chunkedTransfer = false;
#endif

                req.SetRequestHeader("Accept", "application/json");
                req.SetRequestHeader("Authorization", "Bearer " + authToken);

                if (!string.IsNullOrWhiteSpace(language))
                    req.SetRequestHeader("Accept-Language", language.Trim());

                req.timeout = RequestTimeoutSeconds;

                Log($"[HTTP] POST {NotesCreateUrl} (unified) type={type}, difficulty={difficulty}");

                var op = req.SendWebRequest();
                float start = Time.realtimeSinceStartup;

                while (!op.isDone)
                {
                    if (ct.IsCancellationRequested)
                    {
                        req.Abort();
                        ct.ThrowIfCancellationRequested();
                    }
                    if (Time.realtimeSinceStartup - start > RequestTimeoutSeconds)
                    {
                        req.Abort();
                        throw new TimeoutException($"Request timed out after {RequestTimeoutSeconds}s");
                    }
                    await Task.Yield();
                }

                long code = req.responseCode;
                string text = req.downloadHandler?.text ?? "";
                Log($"[HTTP] ← {code} {text}");

#if UNITY_2020_1_OR_NEWER
                bool isSuccess = (req.result == UnityWebRequest.Result.Success) && code >= 200 && code < 300;
#else
                bool isSuccess = !req.isNetworkError && !req.isHttpError && code >= 200 && code < 300;
#endif

                if (isSuccess)
                {
                    Exception parseEx;
                    var resp = TryFromJson<NotesCreateResponse>(text, out parseEx)
                               ?? new NotesCreateResponse { jobId = "", status = "unknown", message = text };
                    return resp;
                }

                if (code == 401)
                {
                    Log("[HTTP] 401 on notes upload; forcing OAuth token on next attempt.");
                    forceOAuthNextAttempt = true;
                    if (attempt < MaxRetries)
                    {
                        try { await EnsureTokenAsync(ct); } catch { }
                        continue;
                    }
                }

                if (code == 429 && attempt < MaxRetries)
                {
                    var retryAfter = req.GetResponseHeader("Retry-After");
                    if (int.TryParse(retryAfter, out int secs))
                    {
                        Log($"[HTTP] 429 Too Many Requests. Retrying after {secs}s …");
                        await Task.Delay(TimeSpan.FromSeconds(secs), ct);
                        continue;
                    }
                }

                bool retryable = code == 408 || code == 423 || code == 425 || (code >= 500 && code <= 599);
                if (retryable && attempt < MaxRetries) continue;

                if ((code >= 400 && code < 500) && code != 408 && code != 429)
                    throw new Exception($"Upload failed (client error): HTTP {code} - {req.error} - {text}");

                throw new Exception($"Upload failed: HTTP {code} - {req.error} - {text}");
            }
            catch (OperationCanceledException)
            {
                req?.Abort();
                LogError("[HTTP] Unified upload cancelled.");
                throw;
            }
            catch (TimeoutException tex)
            {
                lastErr = tex;
                LogError($"[HTTP] Timeout: {tex.Message}");
                if (attempt >= MaxRetries) throw;
            }
            catch (Exception ex)
            {
                lastErr = ex;
                LogError($"[HTTP] Attempt (unified) #{attempt} failed: {ex.GetType().Name} – {ex.Message}");
                if (IsAuthError(ex))
                {
                    Log("[Auth] Auth-like error detected; will force OAuth on next attempt.");
                    forceOAuthNextAttempt = true;
                }
                if (attempt >= MaxRetries) throw;
            }
            finally
            {
                req?.Dispose();
            }
        }

        throw lastErr ?? new Exception("Unknown unified upload error");
    }

    /// <summary>
    /// Quick helper to create a note from YouTube URL
    /// </summary>
    public static Task<NotesCreateResponse> CreateNoteFromYouTubeAsync(
        string youtubeUrl,
        string difficulty = "beginner",
        string title = null,
        string bearerToken = null,
        CancellationToken ct = default,
        string language = null)
    {
        return CreateNoteUnifiedAsync(new CreateNoteRequest
        {
            type = NoteType.youtube,
            youtubeUrl = youtubeUrl,
            difficulty = difficulty,
            title = title
        }, bearerToken, ct, language);
    }

    /// <summary>
    /// Quick helper to create a note from website URL
    /// </summary>
    public static Task<NotesCreateResponse> CreateNoteFromWebsiteAsync(
        string websiteUrl,
        string difficulty = "beginner",
        string title = null,
        string bearerToken = null,
        CancellationToken ct = default,
        string language = null)
    {
        return CreateNoteUnifiedAsync(new CreateNoteRequest
        {
            type = NoteType.website,
            url = websiteUrl,
            difficulty = difficulty,
            title = title
        }, bearerToken, ct, language);
    }

    /// <summary>
    /// Quick helper to create a note from Google Drive/Docs URL
    /// Supports: Google Docs, Sheets, Slides, and Drive file links
    /// </summary>
    public static Task<NotesCreateResponse> CreateNoteFromGoogleDriveAsync(
        string gdriveUrl,
        string difficulty = "beginner",
        string title = null,
        string bearerToken = null,
        CancellationToken ct = default,
        string language = null)
    {
        return CreateNoteUnifiedAsync(new CreateNoteRequest
        {
            type = NoteType.gdrive,
            url = gdriveUrl,
            difficulty = difficulty,
            title = title
        }, bearerToken, ct, language);
    }

    /// <summary>
    /// Quick helper to create a note from plain text
    /// </summary>
    public static Task<NotesCreateResponse> CreateNoteFromTextAsync(
        string text,
        string difficulty = "beginner",
        string title = null,
        string bearerToken = null,
        CancellationToken ct = default,
        string language = null)
    {
        return CreateNoteUnifiedAsync(new CreateNoteRequest
        {
            type = NoteType.text,
            text = text,
            difficulty = difficulty,
            title = title
        }, bearerToken, ct, language);
    }

    /// <summary>
    /// Quick helper to create a note from S3 URL (auto-detects type from URL extension)
    /// </summary>
    public static Task<NotesCreateResponse> CreateNoteFromS3UrlAsync(
        string s3Url,
        NoteType? type = null,
        string difficulty = "beginner",
        string title = null,
        string bearerToken = null,
        CancellationToken ct = default,
        string language = null)
    {
        // Auto-detect type from URL extension if not specified
        NoteType detectedType = type ?? DetectTypeFromUrl(s3Url);

        return CreateNoteUnifiedAsync(new CreateNoteRequest
        {
            type = detectedType,
            s3Url = s3Url,
            difficulty = difficulty,
            title = title
        }, bearerToken, ct, language);
    }

    /// <summary>
    /// Quick helper to create a note from any URL (auto-detects type)
    /// </summary>
    public static Task<NotesCreateResponse> CreateNoteFromUrlAsync(
        string url,
        NoteType? type = null,
        string difficulty = "beginner",
        string title = null,
        string bearerToken = null,
        CancellationToken ct = default,
        string language = null)
    {
        // Auto-detect type from URL
        NoteType detectedType = type ?? DetectTypeFromUrl(url);

        return CreateNoteUnifiedAsync(new CreateNoteRequest
        {
            type = detectedType,
            url = url,
            difficulty = difficulty,
            title = title
        }, bearerToken, ct, language);
    }

    private static NoteType DetectTypeFromUrl(string url)
    {
        if (string.IsNullOrEmpty(url)) return NoteType.pdf;

        string lowerUrl = url.ToLowerInvariant();

        // Check for YouTube - comprehensive detection including shorts, embeds, live, mobile
        if (lowerUrl.Contains("youtube.com") || lowerUrl.Contains("youtu.be"))
            return NoteType.youtube;

        // Check for Google Drive/Docs - use gdrive type for server-side handling
        if (lowerUrl.Contains("docs.google.com") || lowerUrl.Contains("drive.google.com"))
        {
            return NoteType.gdrive;
        }

        // Get extension from URL path (before query params)
        try
        {
            var uri = new Uri(url);
            string path = uri.AbsolutePath.ToLowerInvariant().Split('?')[0];
            
            if (path.EndsWith(".pdf")) return NoteType.pdf;
            if (path.EndsWith(".docx") || path.EndsWith(".doc")) return NoteType.docx;
            if (path.EndsWith(".pptx") || path.EndsWith(".ppt")) return NoteType.pptx;
            if (path.EndsWith(".xlsx") || path.EndsWith(".xls")) return NoteType.xlsx;
            if (path.EndsWith(".csv")) return NoteType.csv;
            if (path.EndsWith(".mp3") || path.EndsWith(".wav") || path.EndsWith(".m4a") || path.EndsWith(".ogg") || path.EndsWith(".flac")) return NoteType.audio;
            if (path.EndsWith(".mp4") || path.EndsWith(".mov") || path.EndsWith(".avi") || path.EndsWith(".mkv") || path.EndsWith(".webm")) return NoteType.video;
            if (path.EndsWith(".jpg") || path.EndsWith(".jpeg") || path.EndsWith(".png") || path.EndsWith(".gif") || path.EndsWith(".webp") || path.EndsWith(".heic")) return NoteType.image;
            if (path.EndsWith(".srt")) return NoteType.srt;
            if (path.EndsWith(".txt")) return NoteType.text;
        }
        catch { }

        // Default to website for unknown URLs
        return NoteType.website;
    }

    public static async Task<NotesContentResponse> GetNoteContentByIdAsync(
    string noteId,
    string bearerToken,
    CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(noteId))
            throw new ArgumentException("noteId is required.", nameof(noteId));

        if (!LooksLikeGuid(noteId))
            throw new ArgumentException($"noteId must be a UUID. Got '{noteId}'.", nameof(noteId));

        string url = $"https://ai.intelli-verse-x.ai/api/ai/notes/{noteId}";

        PrintCurl("GET", url, new Dictionary<string, string> {
        { "Accept", "application/json" }, { "Authorization", "Bearer ****" }
    }, null);

        // -------- Local auth helpers (self-contained) --------
        const int MaxResolveTries = 3;

        static bool IsAuthError(Exception ex)
        {
            var m = ex?.Message ?? string.Empty;
            return m.IndexOf("401", StringComparison.OrdinalIgnoreCase) >= 0 ||
                   m.IndexOf("unauthorized", StringComparison.OrdinalIgnoreCase) >= 0;
        }

        static bool IsUsableJwt(string token)
        {
            if (string.IsNullOrWhiteSpace(token)) return false;
            // If it doesn't look like a JWT, treat as usable (opaque tokens supported).
            int dots = 0; foreach (var c in token) if (c == '.') dots++;
            if (dots != 2) return true;

            try
            {
                string Pad(string s) { s = s.Replace('-', '+').Replace('_', '/'); switch (s.Length % 4) { case 2: s += "=="; break; case 3: s += "="; break; } return s; }
                var parts = token.Split('.');
                var payloadJson = Encoding.UTF8.GetString(Convert.FromBase64String(Pad(parts[1])));
                var expIdx = payloadJson.IndexOf("\"exp\":", StringComparison.OrdinalIgnoreCase);
                if (expIdx < 0) return true;
                var tail = payloadJson.Substring(expIdx + 6);
                var end = tail.IndexOfAny(new[] { ',', '}', ' ' });
                var numStr = end >= 0 ? tail.Substring(0, end) : tail;
                if (!long.TryParse(new string(numStr.Where(char.IsDigit).ToArray()), out var exp)) return true;
                var expUtc = DateTimeOffset.FromUnixTimeSeconds(exp).UtcDateTime;
                return DateTime.UtcNow < expUtc.AddSeconds(-30);
            }
            catch { return true; }
        }

        async Task<string> GetBearerTokenResilientlyAsync(string provided, bool forceOAuth, CancellationToken tokenCt)
        {
            if (!forceOAuth)
            {
                // 1) If caller provided a token and it's usable, use it immediately.
                if (!string.IsNullOrWhiteSpace(provided) && IsUsableJwt(provided))
                {
                    Log("[Auth] Using explicit provided bearer token.");
                    return provided;
                }

                // 2) Resolve user token (with refresh & retries).
                for (int i = 0; i < MaxResolveTries; i++)
                {
                    try
                    {
                        var userTok = await ResolveBearerTokenAsync(provided, tokenCt); // your resolver
                        if (IsUsableJwt(userTok))
                        {
                            Log($"[Auth] Using user/provided token (attempt {i + 1}/{MaxResolveTries}).");
                            return userTok;
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
                            await RefreshUserTokenAsync(tokenCt);
                        }
                        catch (Exception rex)
                        {
                            Log($"[Auth] Refresh failed: {rex.Message}");
                        }
                    }
                }
            }

            // 3) OAuth client-credentials fallback (thread-safe)
            Log("[Auth] Falling back to OAuth client-credentials token.");
            var oauth = await EnsureTokenAsync(tokenCt);
            if (string.IsNullOrWhiteSpace(oauth))
                throw new Exception("OAuth fallback token acquisition returned empty token.");
            return oauth;
        }

        // -------- Main request loop --------
        Exception lastErr = null;
        bool forceOAuthNextAttempt = false;

        for (int attempt = 0; attempt <= MaxRetries; attempt++)
        {
            if (attempt > 0)
            {
                var jitter = 1f + (float)((_rng.NextDouble() * 2 - 1) * JitterPct);
                float delayS = RetryBackoffBaseSeconds * Mathf.Pow(2f, attempt) * jitter;
                Log($"[HTTP] Retry (note) #{attempt} after backoff {delayS:0.00}s …");
                await Task.Delay(TimeSpan.FromSeconds(delayS), ct);
            }

            using (var req = UnityWebRequest.Get(url))
            {
                try
                {
                    var authToken = await GetBearerTokenResilientlyAsync(bearerToken, forceOAuthNextAttempt, ct);

#if UNITY_EDITOR
                    var sub = TryGetJwtSub(authToken);
                    if (!string.IsNullOrEmpty(sub))
                        Log($"[Auth] JWT sub: {sub}  (GUID? {LooksLikeGuid(sub)})");
#endif

                    req.SetRequestHeader("Accept", "application/json");
                    req.SetRequestHeader("Authorization", "Bearer " + authToken);
                    req.timeout = RequestTimeoutSeconds;

                    Log($"[HTTP] GET {url} (note content)");

                    var op = req.SendWebRequest();
                    float start = Time.realtimeSinceStartup;

                    while (!op.isDone)
                    {
                        if (ct.IsCancellationRequested) { req.Abort(); ct.ThrowIfCancellationRequested(); }
                        if (Time.realtimeSinceStartup - start > RequestTimeoutSeconds)
                        {
                            req.Abort();
                            throw new TimeoutException($"Request timed out after {RequestTimeoutSeconds}s");
                        }
                        await Task.Yield();
                    }

                    long code = req.responseCode;
                    string text = req.downloadHandler?.text ?? "";
                    Log($"[HTTP] ← {code} {text}");

#if UNITY_2020_1_OR_NEWER
                    bool ok = req.result == UnityWebRequest.Result.Success && code >= 200 && code < 300;
#else
                bool ok = !req.isHttpError && !req.isNetworkError && code >= 200 && code < 300;
#endif

                    if (ok)
                    {
                        Exception parseEx;
                        var resp = TryFromJson<NotesContentResponse>(text, out parseEx)
                                   ?? new NotesContentResponse { noteId = noteId, content = text };
                        return resp;
                    }

                    // Auth: force OAuth next attempt on 401
                    if (code == 401)
                    {
                        Log("[HTTP] 401 on note content; forcing OAuth token on next attempt.");
                        forceOAuthNextAttempt = true;

                        if (attempt < MaxRetries)
                        {
                            // If already using OAuth, reacquire once and retry.
                            try { await EnsureTokenAsync(ct); } catch { /* ignore */ }
                            continue;
                        }
                    }

                    // Respect Retry-After for 429
                    if (code == 429 && attempt < MaxRetries)
                    {
                        var retryAfter = req.GetResponseHeader("Retry-After");
                        if (int.TryParse(retryAfter, out int secs))
                        {
                            Log($"[HTTP] 429 Too Many Requests. Retrying after {secs}s …");
                            await Task.Delay(TimeSpan.FromSeconds(secs), ct);
                            continue;
                        }
                    }

                    bool retryable = code == 408 || code == 423 || code == 425 || (code >= 500 && code <= 599);
                    if (retryable && attempt < MaxRetries) continue;

                    if ((code >= 400 && code < 500) && code != 408 && code != 429)
                        throw new Exception($"Get note failed (client error): HTTP {code} - {req.error} - {text}");

                    throw new Exception($"Get note failed: HTTP {code} - {req.error} - {text}");
                }
                catch (OperationCanceledException) { LogError("[HTTP] Note content request cancelled."); throw; }
                catch (TimeoutException tex) { lastErr = tex; LogError($"[HTTP] Timeout (note): {tex.Message}"); if (attempt >= MaxRetries) throw; }
                catch (Exception ex)
                {
                    lastErr = ex;
                    LogError($"[HTTP] Attempt (note) #{attempt} failed: {ex.GetType().Name} – {ex.Message}");
                    if (IsAuthError(ex))
                    {
                        Log("[Auth] Auth-like error detected; will force OAuth on next attempt.");
                        forceOAuthNextAttempt = true;
                    }
                    if (attempt >= MaxRetries) throw;
                }
            }
        }

        throw lastErr ?? new Exception("Unknown note content request error");
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // NOTES LIST, DETAILS & MANAGEMENT API
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Get paginated list of user's notes with optional filtering
    /// </summary>
    public static async Task<NotesListResponse> GetNotesListAsync(
        int page = 1,
        int pageSize = 20,
        string type = null,
        string search = null,
        string bearerToken = null,
        CancellationToken ct = default)
    {
        string url = NotesListUrl(page, pageSize, type, search);
        string authToken = await ResolveBearerTokenAsync(bearerToken, ct);

        using (var request = UnityWebRequest.Get(url))
        {
            request.SetRequestHeader("Authorization", $"Bearer {authToken}");
            request.SetRequestHeader("Accept", "application/json");
            request.timeout = RequestTimeoutSeconds;

            PrintCurl("GET", url, new Dictionary<string, string> {
                { "Accept", "application/json" },
                { "Authorization", "Bearer ****" }
            }, null);

            var op = request.SendWebRequest();
            float start = Time.realtimeSinceStartup;
            while (!op.isDone)
            {
                if (ct.IsCancellationRequested) { request.Abort(); ct.ThrowIfCancellationRequested(); }
                if (Time.realtimeSinceStartup - start > RequestTimeoutSeconds) { request.Abort(); break; }
                await Task.Yield();
            }

            if (request.result != UnityWebRequest.Result.Success)
            {
                Log($"[GetNotesListAsync] Error: {request.error}");
                return new NotesListResponse { status = false, message = request.error };
            }

            string responseText = request.downloadHandler.text;
            Log($"[GetNotesListAsync] Response: {responseText}");

            try
            {
                return JsonUtility.FromJson<NotesListResponse>(responseText);
            }
            catch (Exception ex)
            {
                Log($"[GetNotesListAsync] Parse error: {ex.Message}");
                return new NotesListResponse { status = false, message = "Failed to parse response" };
            }
        }
    }

    /// <summary>
    /// Get recently updated notes for the user
    /// </summary>
    public static async Task<NotesListResponse> GetRecentNotesAsync(
        int limit = 10,
        string bearerToken = null,
        CancellationToken ct = default)
    {
        string url = NotesRecentUrl(limit);
        string authToken = await ResolveBearerTokenAsync(bearerToken, ct);

        using (var request = UnityWebRequest.Get(url))
        {
            request.SetRequestHeader("Authorization", $"Bearer {authToken}");
            request.SetRequestHeader("Accept", "application/json");
            request.timeout = RequestTimeoutSeconds;

            var op = request.SendWebRequest();
            float start = Time.realtimeSinceStartup;
            while (!op.isDone)
            {
                if (ct.IsCancellationRequested) { request.Abort(); ct.ThrowIfCancellationRequested(); }
                if (Time.realtimeSinceStartup - start > RequestTimeoutSeconds) { request.Abort(); break; }
                await Task.Yield();
            }

            if (request.result != UnityWebRequest.Result.Success)
            {
                Log($"[GetRecentNotesAsync] Error: {request.error}");
                return new NotesListResponse { status = false, message = request.error };
            }

            string responseText = request.downloadHandler.text;
            return JsonUtility.FromJson<NotesListResponse>(responseText);
        }
    }

    /// <summary>
    /// Get detailed note information including flashcards and quiz
    /// </summary>
    public static async Task<NoteDetailsResponse> GetNoteDetailsAsync(
        string noteId,
        string bearerToken = null,
        CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(noteId))
            throw new ArgumentException("noteId is required.", nameof(noteId));

        string url = NotesDetailsUrl(noteId);
        
        // Local auth helpers for resilient token handling
        const int MaxResolveTries = 3;

        static bool IsAuthError(Exception ex)
        {
            var m = ex?.Message ?? string.Empty;
            return m.IndexOf("401", StringComparison.OrdinalIgnoreCase) >= 0 ||
                   m.IndexOf("unauthorized", StringComparison.OrdinalIgnoreCase) >= 0;
        }

        static bool IsUsableJwt(string token)
        {
            if (string.IsNullOrWhiteSpace(token)) return false;
            int dots = 0;
            foreach (var c in token) if (c == '.') dots++;
            if (dots != 2) return true;

            try
            {
                string Pad(string s)
                {
                    s = s.Replace('-', '+').Replace('_', '/');
                    switch (s.Length % 4)
                    {
                        case 2: s += "=="; break;
                        case 3: s += "="; break;
                    }
                    return s;
                }

                var parts = token.Split('.');
                var payloadJson = Encoding.UTF8.GetString(Convert.FromBase64String(Pad(parts[1])));
                var expIdx = payloadJson.IndexOf("\"exp\":", StringComparison.OrdinalIgnoreCase);
                if (expIdx < 0) return true;

                var tail = payloadJson.Substring(expIdx + 6);
                var end = tail.IndexOfAny(new[] { ',', '}', ' ' });
                var numStr = end >= 0 ? tail.Substring(0, end) : tail;
                if (!long.TryParse(new string(numStr.Where(char.IsDigit).ToArray()), out var exp)) return true;

                var expUtc = DateTimeOffset.FromUnixTimeSeconds(exp).UtcDateTime;
                return DateTime.UtcNow < expUtc.AddSeconds(-30);
            }
            catch { return true; }
        }

        async Task<string> GetBearerTokenResilientlyAsync(string provided, bool forceOAuth, CancellationToken tokenCt)
        {
            if (!forceOAuth)
            {
                if (!string.IsNullOrWhiteSpace(provided) && IsUsableJwt(provided))
                {
                    Log("[Auth] Using explicit provided bearer token.");
                    return provided;
                }

                for (int i = 0; i < MaxResolveTries; i++)
                {
                    try
                    {
                        var userTok = await ResolveBearerTokenAsync(provided, tokenCt);
                        if (IsUsableJwt(userTok))
                        {
                            Log($"[Auth] Using user/provided token (attempt {i + 1}/{MaxResolveTries}).");
                            return userTok;
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
                            await RefreshUserTokenAsync(tokenCt);
                        }
                        catch { }
                    }
                }
            }

            // Fall back to OAuth
            Log("[Auth] Falling back to OAuth token …");
            return await EnsureTokenAsync(tokenCt);
        }

        // First attempt
        string authToken = await GetBearerTokenResilientlyAsync(bearerToken, false, ct);
        
        async Task<NoteDetailsResponse> ExecuteRequest(string token)
        {
            using (var request = UnityWebRequest.Get(url))
            {
                request.SetRequestHeader("Authorization", $"Bearer {token}");
                request.SetRequestHeader("Accept", "application/json");
                request.timeout = RequestTimeoutSeconds;

                Log($"[HTTP] GET {url} (note details)");

                var op = request.SendWebRequest();
                float start = Time.realtimeSinceStartup;
                while (!op.isDone)
                {
                    if (ct.IsCancellationRequested) { request.Abort(); ct.ThrowIfCancellationRequested(); }
                    if (Time.realtimeSinceStartup - start > RequestTimeoutSeconds) { request.Abort(); break; }
                    await Task.Yield();
                }

                long code = request.responseCode;
                string responseText = request.downloadHandler?.text ?? "";
                Log($"[HTTP] ← {code} {responseText.Substring(0, Math.Min(200, responseText.Length))}");

                if (request.result != UnityWebRequest.Result.Success)
                {
                    throw new Exception($"HTTP {code} - {request.error}");
                }

                return JsonUtility.FromJson<NoteDetailsResponse>(responseText);
            }
        }

        try
        {
            return await ExecuteRequest(authToken);
        }
        catch (Exception ex) when (IsAuthError(ex))
        {
            // Retry with forced OAuth
            Log($"[GetNoteDetailsAsync] Auth error, retrying with OAuth: {ex.Message}");
            authToken = await GetBearerTokenResilientlyAsync(bearerToken, true, ct);
            try
            {
                return await ExecuteRequest(authToken);
            }
            catch (Exception retryEx)
            {
                Log($"[GetNoteDetailsAsync] Retry failed: {retryEx.Message}");
                return new NoteDetailsResponse { status = false, message = retryEx.Message };
            }
        }
        catch (Exception ex)
        {
            Log($"[GetNoteDetailsAsync] Error: {ex.Message}");
            return new NoteDetailsResponse { status = false, message = ex.Message };
        }
    }

    /// <summary>
    /// Get statistics overview for user's notes
    /// </summary>
    public static async Task<NotesStatsResponse> GetNotesStatsAsync(
        string bearerToken = null,
        CancellationToken ct = default)
    {
        string url = NotesStatsUrl;
        string authToken = await ResolveBearerTokenAsync(bearerToken, ct);

        using (var request = UnityWebRequest.Get(url))
        {
            request.SetRequestHeader("Authorization", $"Bearer {authToken}");
            request.SetRequestHeader("Accept", "application/json");
            request.timeout = RequestTimeoutSeconds;

            var op = request.SendWebRequest();
            float start = Time.realtimeSinceStartup;
            while (!op.isDone)
            {
                if (ct.IsCancellationRequested) { request.Abort(); ct.ThrowIfCancellationRequested(); }
                if (Time.realtimeSinceStartup - start > RequestTimeoutSeconds) { request.Abort(); break; }
                await Task.Yield();
            }

            if (request.result != UnityWebRequest.Result.Success)
            {
                Log($"[GetNotesStatsAsync] Error: {request.error}");
                return new NotesStatsResponse { status = false, message = request.error };
            }

            string responseText = request.downloadHandler.text;
            return JsonUtility.FromJson<NotesStatsResponse>(responseText);
        }
    }

    /// <summary>
    /// Delete a note and all associated data
    /// </summary>
    public static async Task<DeleteNoteResponse> DeleteNoteAsync(
        string noteId,
        string bearerToken = null,
        CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(noteId))
            throw new ArgumentException("noteId is required.", nameof(noteId));

        string url = NotesDeleteUrl(noteId);
        string authToken = await ResolveBearerTokenAsync(bearerToken, ct);

        using (var request = UnityWebRequest.Delete(url))
        {
            request.SetRequestHeader("Authorization", $"Bearer {authToken}");
            request.SetRequestHeader("Accept", "application/json");
            request.timeout = RequestTimeoutSeconds;

            var op = request.SendWebRequest();
            float start = Time.realtimeSinceStartup;
            while (!op.isDone)
            {
                if (ct.IsCancellationRequested) { request.Abort(); ct.ThrowIfCancellationRequested(); }
                if (Time.realtimeSinceStartup - start > RequestTimeoutSeconds) { request.Abort(); break; }
                await Task.Yield();
            }

            if (request.result != UnityWebRequest.Result.Success)
            {
                Log($"[DeleteNoteAsync] Error: {request.error}");
                return new DeleteNoteResponse { status = false, message = request.error };
            }

            string responseText = request.downloadHandler.text;
            try
            {
                return JsonUtility.FromJson<DeleteNoteResponse>(responseText);
            }
            catch
            {
                return new DeleteNoteResponse { status = true, message = "Note deleted successfully" };
            }
        }
    }

    /// <summary>
    /// Generate flashcards and quiz for a note
    /// </summary>
    public static async Task<FlashcardsQuizResponse> GenerateFlashcardsQuizzesAsync(
        string noteId,
        string bearerToken = null,
        CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(noteId))
            throw new ArgumentException("noteId is required.", nameof(noteId));

        string url = NotesFlashcardsQuizzesUrl(noteId);
        string authToken = await ResolveBearerTokenAsync(bearerToken, ct);

        using (var request = new UnityWebRequest(url, "POST"))
        {
            request.SetRequestHeader("Authorization", $"Bearer {authToken}");
            request.SetRequestHeader("Accept", "application/json");
            request.SetRequestHeader("Content-Type", "application/json");
            request.downloadHandler = new DownloadHandlerBuffer();
            int aiTimeout = 120; // Longer timeout for AI generation

            PrintCurl("POST", url, new Dictionary<string, string> {
                { "Accept", "application/json" },
                { "Authorization", "Bearer ****" }
            }, null);

            var op = request.SendWebRequest();
            float start = Time.realtimeSinceStartup;
            while (!op.isDone)
            {
                if (ct.IsCancellationRequested) { request.Abort(); ct.ThrowIfCancellationRequested(); }
                if (Time.realtimeSinceStartup - start > aiTimeout) { request.Abort(); break; }
                await Task.Yield();
            }

            if (request.result != UnityWebRequest.Result.Success)
            {
                Log($"[GenerateFlashcardsQuizzesAsync] Error: {request.error}");
                return new FlashcardsQuizResponse { status = false, message = request.error };
            }

            string responseText = request.downloadHandler.text;
            Log($"[GenerateFlashcardsQuizzesAsync] Response: {responseText}");

            return JsonUtility.FromJson<FlashcardsQuizResponse>(responseText);
        }
    }

    /// <summary>
    /// Poll for job status until completed or failed
    /// </summary>
    public static async Task<NotesJobStatusResponse> GetNoteJobStatusAsync(
        string jobId,
        string bearerToken = null,
        CancellationToken ct = default)
    {
        return await GetNotesJobStatusAsync(jobId, bearerToken, ct);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // CHAT WITH NOTES API
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Create a new chat session for a note
    /// </summary>
    public static async Task<CreateChatResponse> CreateNoteChatAsync(
        string noteId,
        string title = "New Chat",
        string bearerToken = null,
        CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(noteId))
            throw new ArgumentException("noteId is required.", nameof(noteId));

        string url = NotesChatCreateUrl(noteId);
        string jsonBody = JsonUtility.ToJson(new { title = title ?? "New Chat" });

        var headers = new Dictionary<string, string>
        {
            { "Accept", "application/json" },
            { "Content-Type", "application/json" }
        };

        PrintCurl("POST", url, headers, jsonBody);

        string authToken = await ResolveBearerTokenAsync(bearerToken, ct);

        using var req = new UnityWebRequest(url, "POST");
        req.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(jsonBody));
        req.downloadHandler = new DownloadHandlerBuffer();
        req.SetRequestHeader("Accept", "application/json");
        req.SetRequestHeader("Content-Type", "application/json");
        req.SetRequestHeader("Authorization", "Bearer " + authToken);
        req.timeout = RequestTimeoutSeconds;

        Log($"[HTTP] POST {url} (create chat)");

        var op = req.SendWebRequest();
        float start = Time.realtimeSinceStartup;

        while (!op.isDone)
        {
            if (ct.IsCancellationRequested) { req.Abort(); ct.ThrowIfCancellationRequested(); }
            if (Time.realtimeSinceStartup - start > RequestTimeoutSeconds)
            {
                req.Abort();
                throw new TimeoutException($"Request timed out after {RequestTimeoutSeconds}s");
            }
            await Task.Yield();
        }

        long code = req.responseCode;
        string text = req.downloadHandler?.text ?? "";
        Log($"[HTTP] ← {code} {text}");

#if UNITY_2020_1_OR_NEWER
        bool ok = req.result == UnityWebRequest.Result.Success && code >= 200 && code < 300;
#else
        bool ok = !req.isHttpError && !req.isNetworkError && code >= 200 && code < 300;
#endif

        if (ok)
        {
            Exception parseEx;
            return TryFromJson<CreateChatResponse>(text, out parseEx);
        }

        throw new Exception($"Create chat failed: HTTP {code} - {req.error} - {text}");
    }

    /// <summary>
    /// Get chat history for a chat session
    /// </summary>
    public static async Task<ChatHistoryResponse> GetChatHistoryAsync(
        string chatId,
        int page = 1,
        int pageSize = 20,
        string bearerToken = null,
        CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(chatId))
            throw new ArgumentException("chatId is required.", nameof(chatId));

        string url = NotesChatHistoryUrl(chatId, page, pageSize);

        string authToken = await ResolveBearerTokenAsync(bearerToken, ct);

        using var req = UnityWebRequest.Get(url);
        req.SetRequestHeader("Accept", "application/json");
        req.SetRequestHeader("Authorization", "Bearer " + authToken);
        req.timeout = RequestTimeoutSeconds;

        Log($"[HTTP] GET {url} (chat history)");

        var op = req.SendWebRequest();
        float start = Time.realtimeSinceStartup;

        while (!op.isDone)
        {
            if (ct.IsCancellationRequested) { req.Abort(); ct.ThrowIfCancellationRequested(); }
            if (Time.realtimeSinceStartup - start > RequestTimeoutSeconds)
            {
                req.Abort();
                throw new TimeoutException($"Request timed out after {RequestTimeoutSeconds}s");
            }
            await Task.Yield();
        }

        long code = req.responseCode;
        string text = req.downloadHandler?.text ?? "";
        Log($"[HTTP] ← {code} {text}");

#if UNITY_2020_1_OR_NEWER
        bool ok = req.result == UnityWebRequest.Result.Success && code >= 200 && code < 300;
#else
        bool ok = !req.isHttpError && !req.isNetworkError && code >= 200 && code < 300;
#endif

        if (ok)
        {
            Exception parseEx;
            return TryFromJson<ChatHistoryResponse>(text, out parseEx);
        }

        throw new Exception($"Get chat history failed: HTTP {code} - {req.error} - {text}");
    }

    /// <summary>
    /// Send a message and receive streaming response via SSE.
    /// Use the onChunk callback to receive streaming content.
    /// </summary>
    public static async Task SendChatMessageStreamingAsync(
        string chatId,
        string message,
        Action<string> onChunk,
        Action<ChatSource[]> onSourcesReceived = null,
        Action onComplete = null,
        Action<string> onError = null,
        string bearerToken = null,
        CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(chatId))
            throw new ArgumentException("chatId is required.", nameof(chatId));
        if (string.IsNullOrWhiteSpace(message))
            throw new ArgumentException("message is required.", nameof(message));

        string url = NotesChatStreamUrl(chatId, message);
        string authToken = await ResolveBearerTokenAsync(bearerToken, ct);

        Log($"[HTTP] SSE {url} (chat stream)");

        using var req = UnityWebRequest.Get(url);
        req.SetRequestHeader("Accept", "text/event-stream");
        req.SetRequestHeader("Authorization", "Bearer " + authToken);
        req.timeout = 120; // Longer timeout for streaming

        // Use custom download handler for streaming
        var downloadHandler = new DownloadHandlerBuffer();
        req.downloadHandler = downloadHandler;

        var op = req.SendWebRequest();
        float start = Time.realtimeSinceStartup;
        int lastProcessedLength = 0;
        bool sourcesReceived = false;

        while (!op.isDone)
        {
            if (ct.IsCancellationRequested)
            {
                req.Abort();
                ct.ThrowIfCancellationRequested();
            }

            if (Time.realtimeSinceStartup - start > 120f)
            {
                req.Abort();
                onError?.Invoke("Stream timed out");
                return;
            }

            // Process any new data that has arrived
            string currentText = downloadHandler.text ?? "";
            if (currentText.Length > lastProcessedLength)
            {
                string newData = currentText.Substring(lastProcessedLength);
                lastProcessedLength = currentText.Length;

                // Parse SSE events
                var lines = newData.Split('\n');
                foreach (var line in lines)
                {
                    if (string.IsNullOrWhiteSpace(line)) continue;

                    if (line.StartsWith("data:"))
                    {
                        string jsonData = line.Substring(5).Trim();
                        if (!string.IsNullOrEmpty(jsonData))
                        {
                            try
                            {
                                Exception parseEx;
                                var chunk = TryFromJson<StreamChunk>(jsonData, out parseEx);
                                if (chunk != null)
                                {
                                    if (!string.IsNullOrEmpty(chunk.content))
                                    {
                                        onChunk?.Invoke(chunk.content);
                                    }
                                    if (chunk.sources != null && chunk.sources.Length > 0 && !sourcesReceived)
                                    {
                                        sourcesReceived = true;
                                        onSourcesReceived?.Invoke(chunk.sources);
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                Log($"[SSE] Parse error: {ex.Message}");
                            }
                        }
                    }
                }
            }

            await Task.Yield();
        }

        // Process any remaining data
        string finalText = downloadHandler.text ?? "";
        if (finalText.Length > lastProcessedLength)
        {
            string remainingData = finalText.Substring(lastProcessedLength);
            var lines = remainingData.Split('\n');
            foreach (var line in lines)
            {
                if (line.StartsWith("data:"))
                {
                    string jsonData = line.Substring(5).Trim();
                    if (!string.IsNullOrEmpty(jsonData))
                    {
                        try
                        {
                            Exception parseEx;
                            var chunk = TryFromJson<StreamChunk>(jsonData, out parseEx);
                            if (chunk?.content != null)
                            {
                                onChunk?.Invoke(chunk.content);
                            }
                        }
                        catch { }
                    }
                }
            }
        }

        long code = req.responseCode;

#if UNITY_2020_1_OR_NEWER
        bool ok = req.result == UnityWebRequest.Result.Success && code >= 200 && code < 300;
#else
        bool ok = !req.isHttpError && !req.isNetworkError && code >= 200 && code < 300;
#endif

        if (ok)
        {
            onComplete?.Invoke();
        }
        else
        {
            onError?.Invoke($"Stream failed: HTTP {code} - {req.error}");
        }
    }

    /// <summary>
    /// Simple non-streaming chat message (polls for complete response)
    /// </summary>
    public static async Task<string> SendChatMessageAsync(
        string chatId,
        string message,
        string bearerToken = null,
        CancellationToken ct = default)
    {
        var fullResponse = new System.Text.StringBuilder();
        var tcs = new TaskCompletionSource<string>();

        await SendChatMessageStreamingAsync(
            chatId,
            message,
            chunk => fullResponse.Append(chunk),
            onComplete: () => tcs.TrySetResult(fullResponse.ToString()),
            onError: error => tcs.TrySetException(new Exception(error)),
            bearerToken: bearerToken,
            ct: ct
        );

        return await tcs.Task;
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // AI DEBATE API
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Get debatable topics from a note's content
    /// </summary>
    public static async Task<DebateTopicsResponse> GetDebateTopicsAsync(
        string noteId,
        string difficulty = "intermediate",
        string bearerToken = null,
        CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(noteId))
            throw new ArgumentException("noteId is required.", nameof(noteId));

        string url = NotesDebateTopicsUrl(noteId, difficulty);
        string authToken = await ResolveBearerTokenAsync(bearerToken, ct);

        using var req = UnityWebRequest.Get(url);
        req.SetRequestHeader("Accept", "application/json");
        req.SetRequestHeader("Authorization", "Bearer " + authToken);
        req.timeout = RequestTimeoutSeconds;

        Log($"[HTTP] GET {url} (debate topics)");

        var op = req.SendWebRequest();
        float start = Time.realtimeSinceStartup;

        while (!op.isDone)
        {
            if (ct.IsCancellationRequested) { req.Abort(); ct.ThrowIfCancellationRequested(); }
            if (Time.realtimeSinceStartup - start > RequestTimeoutSeconds)
            {
                req.Abort();
                throw new TimeoutException($"Request timed out after {RequestTimeoutSeconds}s");
            }
            await Task.Yield();
        }

        long code = req.responseCode;
        string text = req.downloadHandler?.text ?? "";
        Log($"[HTTP] ← {code} {text}");

#if UNITY_2020_1_OR_NEWER
        bool ok = req.result == UnityWebRequest.Result.Success && code >= 200 && code < 300;
#else
        bool ok = !req.isHttpError && !req.isNetworkError && code >= 200 && code < 300;
#endif

        if (ok)
        {
            Exception parseEx;
            return TryFromJson<DebateTopicsResponse>(text, out parseEx);
        }

        throw new Exception($"Get debate topics failed: HTTP {code} - {req.error} - {text}");
    }

    /// <summary>
    /// Start a debate session with AI taking the opposing position
    /// </summary>
    public static async Task<DebateStartResponse> StartDebateAsync(
        string noteId,
        string topic,
        string userPosition, // "for" or "against"
        string bearerToken = null,
        CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(noteId))
            throw new ArgumentException("noteId is required.", nameof(noteId));
        if (string.IsNullOrWhiteSpace(topic))
            throw new ArgumentException("topic is required.", nameof(topic));
        if (string.IsNullOrWhiteSpace(userPosition))
            throw new ArgumentException("userPosition is required.", nameof(userPosition));

        string url = NotesDebateStartUrl(noteId);
        string jsonBody = $"{{\"topic\":\"{EscapeJsonString(topic)}\",\"position\":\"{userPosition}\"}}";
        string authToken = await ResolveBearerTokenAsync(bearerToken, ct);

        using var req = new UnityWebRequest(url, "POST");
        req.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(jsonBody));
        req.downloadHandler = new DownloadHandlerBuffer();
        req.SetRequestHeader("Accept", "application/json");
        req.SetRequestHeader("Content-Type", "application/json");
        req.SetRequestHeader("Authorization", "Bearer " + authToken);
        req.timeout = RequestTimeoutSeconds;

        Log($"[HTTP] POST {url} (start debate)");

        var op = req.SendWebRequest();
        float start = Time.realtimeSinceStartup;

        while (!op.isDone)
        {
            if (ct.IsCancellationRequested) { req.Abort(); ct.ThrowIfCancellationRequested(); }
            if (Time.realtimeSinceStartup - start > RequestTimeoutSeconds)
            {
                req.Abort();
                throw new TimeoutException($"Request timed out after {RequestTimeoutSeconds}s");
            }
            await Task.Yield();
        }

        long code = req.responseCode;
        string text = req.downloadHandler?.text ?? "";
        Log($"[HTTP] ← {code} {text}");

#if UNITY_2020_1_OR_NEWER
        bool ok = req.result == UnityWebRequest.Result.Success && code >= 200 && code < 300;
#else
        bool ok = !req.isHttpError && !req.isNetworkError && code >= 200 && code < 300;
#endif

        if (ok)
        {
            Exception parseEx;
            return TryFromJson<DebateStartResponse>(text, out parseEx);
        }

        throw new Exception($"Start debate failed: HTTP {code} - {req.error} - {text}");
    }

    /// <summary>
    /// Score a completed debate session
    /// </summary>
    public static async Task<DebateScoreResponse> ScoreDebateAsync(
        string chatId,
        string topic,
        string userPosition = null,
        string bearerToken = null,
        CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(chatId))
            throw new ArgumentException("chatId is required.", nameof(chatId));
        if (string.IsNullOrWhiteSpace(topic))
            throw new ArgumentException("topic is required.", nameof(topic));

        string url = NotesDebateScoreUrl(chatId);
        string jsonBody = userPosition != null
            ? $"{{\"topic\":\"{EscapeJsonString(topic)}\",\"userPosition\":\"{EscapeJsonString(userPosition)}\"}}"
            : $"{{\"topic\":\"{EscapeJsonString(topic)}\"}}";
        string authToken = await ResolveBearerTokenAsync(bearerToken, ct);

        using var req = new UnityWebRequest(url, "POST");
        req.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(jsonBody));
        req.downloadHandler = new DownloadHandlerBuffer();
        req.SetRequestHeader("Accept", "application/json");
        req.SetRequestHeader("Content-Type", "application/json");
        req.SetRequestHeader("Authorization", "Bearer " + authToken);
        req.timeout = 60; // Longer timeout for AI scoring

        Log($"[HTTP] POST {url} (score debate)");

        var op = req.SendWebRequest();
        float start = Time.realtimeSinceStartup;

        while (!op.isDone)
        {
            if (ct.IsCancellationRequested) { req.Abort(); ct.ThrowIfCancellationRequested(); }
            if (Time.realtimeSinceStartup - start > 60f)
            {
                req.Abort();
                throw new TimeoutException("Debate scoring timed out after 60s");
            }
            await Task.Yield();
        }

        long code = req.responseCode;
        string text = req.downloadHandler?.text ?? "";
        Log($"[HTTP] ← {code} {text}");

#if UNITY_2020_1_OR_NEWER
        bool ok = req.result == UnityWebRequest.Result.Success && code >= 200 && code < 300;
#else
        bool ok = !req.isHttpError && !req.isNetworkError && code >= 200 && code < 300;
#endif

        if (ok)
        {
            Exception parseEx;
            return TryFromJson<DebateScoreResponse>(text, out parseEx);
        }

        throw new Exception($"Score debate failed: HTTP {code} - {req.error} - {text}");
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // ADVANCED DEBATE MODES
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Get available debate modes with descriptions
    /// </summary>
    public static async Task<DebateModesResponse> GetDebateModesAsync(
        string bearerToken = null,
        CancellationToken ct = default)
    {
        string url = NotesDebateModesUrl;
        string authToken = await ResolveBearerTokenAsync(bearerToken, ct);

        using var req = UnityWebRequest.Get(url);
        req.SetRequestHeader("Accept", "application/json");
        req.SetRequestHeader("Authorization", "Bearer " + authToken);
        req.timeout = RequestTimeoutSeconds;

        Log($"[HTTP] GET {url} (get debate modes)");

        var op = req.SendWebRequest();
        while (!op.isDone)
        {
            if (ct.IsCancellationRequested) { req.Abort(); ct.ThrowIfCancellationRequested(); }
            await Task.Yield();
        }

        long code = req.responseCode;
        string text = req.downloadHandler?.text ?? "";
        Log($"[HTTP] ← {code} {text}");

#if UNITY_2020_1_OR_NEWER
        bool ok = req.result == UnityWebRequest.Result.Success && code >= 200 && code < 300;
#else
        bool ok = !req.isHttpError && !req.isNetworkError && code >= 200 && code < 300;
#endif

        if (ok) return TryFromJson<DebateModesResponse>(text, out _);
        throw new Exception($"Get debate modes failed: HTTP {code} - {req.error} - {text}");
    }

    /// <summary>
    /// Start a timed debate with time constraints per argument
    /// </summary>
    public static async Task<TimedDebateStartResponse> StartTimedDebateAsync(
        string noteId,
        string topic,
        string userPosition,
        int timeLimitSeconds = 120,
        int totalTimeLimitSeconds = 600,
        string bearerToken = null,
        CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(noteId)) throw new ArgumentException("noteId is required.", nameof(noteId));
        if (string.IsNullOrWhiteSpace(topic)) throw new ArgumentException("topic is required.", nameof(topic));
        if (string.IsNullOrWhiteSpace(userPosition)) throw new ArgumentException("userPosition is required.", nameof(userPosition));

        string url = NotesDebateTimedUrl(noteId);
        string jsonBody = $"{{\"topic\":\"{EscapeJsonString(topic)}\",\"position\":\"{EscapeJsonString(userPosition)}\",\"timeLimitSeconds\":{timeLimitSeconds},\"totalTimeLimitSeconds\":{totalTimeLimitSeconds}}}";
        string authToken = await ResolveBearerTokenAsync(bearerToken, ct);

        using var req = new UnityWebRequest(url, "POST");
        req.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(jsonBody));
        req.downloadHandler = new DownloadHandlerBuffer();
        req.SetRequestHeader("Accept", "application/json");
        req.SetRequestHeader("Content-Type", "application/json");
        req.SetRequestHeader("Authorization", "Bearer " + authToken);
        req.timeout = 30;

        Log($"[HTTP] POST {url} (start timed debate)");

        var op = req.SendWebRequest();
        while (!op.isDone)
        {
            if (ct.IsCancellationRequested) { req.Abort(); ct.ThrowIfCancellationRequested(); }
            await Task.Yield();
        }

        long code = req.responseCode;
        string text = req.downloadHandler?.text ?? "";
        Log($"[HTTP] ← {code} {text}");

#if UNITY_2020_1_OR_NEWER
        bool ok = req.result == UnityWebRequest.Result.Success && code >= 200 && code < 300;
#else
        bool ok = !req.isHttpError && !req.isNetworkError && code >= 200 && code < 300;
#endif

        if (ok) return TryFromJson<TimedDebateStartResponse>(text, out _);
        throw new Exception($"Start timed debate failed: HTTP {code} - {req.error} - {text}");
    }

    /// <summary>
    /// Start a multi-round debate tournament
    /// </summary>
    public static async Task<MultiRoundDebateStartResponse> StartMultiRoundDebateAsync(
        string noteId,
        string topic,
        string userPosition,
        int totalRounds = 3,
        string bearerToken = null,
        CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(noteId)) throw new ArgumentException("noteId is required.", nameof(noteId));
        if (string.IsNullOrWhiteSpace(topic)) throw new ArgumentException("topic is required.", nameof(topic));
        if (string.IsNullOrWhiteSpace(userPosition)) throw new ArgumentException("userPosition is required.", nameof(userPosition));

        string url = NotesDebateMultiRoundUrl(noteId);
        string jsonBody = $"{{\"topic\":\"{EscapeJsonString(topic)}\",\"position\":\"{EscapeJsonString(userPosition)}\",\"totalRounds\":{totalRounds}}}";
        string authToken = await ResolveBearerTokenAsync(bearerToken, ct);

        using var req = new UnityWebRequest(url, "POST");
        req.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(jsonBody));
        req.downloadHandler = new DownloadHandlerBuffer();
        req.SetRequestHeader("Accept", "application/json");
        req.SetRequestHeader("Content-Type", "application/json");
        req.SetRequestHeader("Authorization", "Bearer " + authToken);
        req.timeout = 30;

        Log($"[HTTP] POST {url} (start multi-round debate)");

        var op = req.SendWebRequest();
        while (!op.isDone)
        {
            if (ct.IsCancellationRequested) { req.Abort(); ct.ThrowIfCancellationRequested(); }
            await Task.Yield();
        }

        long code = req.responseCode;
        string text = req.downloadHandler?.text ?? "";
        Log($"[HTTP] ← {code} {text}");

#if UNITY_2020_1_OR_NEWER
        bool ok = req.result == UnityWebRequest.Result.Success && code >= 200 && code < 300;
#else
        bool ok = !req.isHttpError && !req.isNetworkError && code >= 200 && code < 300;
#endif

        if (ok) return TryFromJson<MultiRoundDebateStartResponse>(text, out _);
        throw new Exception($"Start multi-round debate failed: HTTP {code} - {req.error} - {text}");
    }

    /// <summary>
    /// Start a rapid-fire debate with short argument limits
    /// </summary>
    public static async Task<RapidFireDebateStartResponse> StartRapidFireDebateAsync(
        string noteId,
        string topic,
        string userPosition,
        int maxArgumentLength = 280,
        int timeLimitSeconds = 30,
        string bearerToken = null,
        CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(noteId)) throw new ArgumentException("noteId is required.", nameof(noteId));
        if (string.IsNullOrWhiteSpace(topic)) throw new ArgumentException("topic is required.", nameof(topic));
        if (string.IsNullOrWhiteSpace(userPosition)) throw new ArgumentException("userPosition is required.", nameof(userPosition));

        string url = NotesDebateRapidFireUrl(noteId);
        string jsonBody = $"{{\"topic\":\"{EscapeJsonString(topic)}\",\"position\":\"{EscapeJsonString(userPosition)}\",\"maxArgumentLength\":{maxArgumentLength},\"timeLimitSeconds\":{timeLimitSeconds}}}";
        string authToken = await ResolveBearerTokenAsync(bearerToken, ct);

        using var req = new UnityWebRequest(url, "POST");
        req.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(jsonBody));
        req.downloadHandler = new DownloadHandlerBuffer();
        req.SetRequestHeader("Accept", "application/json");
        req.SetRequestHeader("Content-Type", "application/json");
        req.SetRequestHeader("Authorization", "Bearer " + authToken);
        req.timeout = 30;

        Log($"[HTTP] POST {url} (start rapid-fire debate)");

        var op = req.SendWebRequest();
        while (!op.isDone)
        {
            if (ct.IsCancellationRequested) { req.Abort(); ct.ThrowIfCancellationRequested(); }
            await Task.Yield();
        }

        long code = req.responseCode;
        string text = req.downloadHandler?.text ?? "";
        Log($"[HTTP] ← {code} {text}");

#if UNITY_2020_1_OR_NEWER
        bool ok = req.result == UnityWebRequest.Result.Success && code >= 200 && code < 300;
#else
        bool ok = !req.isHttpError && !req.isNetworkError && code >= 200 && code < 300;
#endif

        if (ok) return TryFromJson<RapidFireDebateStartResponse>(text, out _);
        throw new Exception($"Start rapid-fire debate failed: HTTP {code} - {req.error} - {text}");
    }

    /// <summary>
    /// Advance to next round in multi-round debate
    /// </summary>
    public static async Task<NextRoundResponse> AdvanceToNextRoundAsync(
        string chatId,
        string bearerToken = null,
        CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(chatId)) throw new ArgumentException("chatId is required.", nameof(chatId));

        string url = NotesDebateNextRoundUrl(chatId);
        string authToken = await ResolveBearerTokenAsync(bearerToken, ct);

        using var req = new UnityWebRequest(url, "POST");
        req.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes("{}"));
        req.downloadHandler = new DownloadHandlerBuffer();
        req.SetRequestHeader("Accept", "application/json");
        req.SetRequestHeader("Content-Type", "application/json");
        req.SetRequestHeader("Authorization", "Bearer " + authToken);
        req.timeout = 30;

        Log($"[HTTP] POST {url} (advance to next round)");

        var op = req.SendWebRequest();
        while (!op.isDone)
        {
            if (ct.IsCancellationRequested) { req.Abort(); ct.ThrowIfCancellationRequested(); }
            await Task.Yield();
        }

        long code = req.responseCode;
        string text = req.downloadHandler?.text ?? "";
        Log($"[HTTP] ← {code} {text}");

#if UNITY_2020_1_OR_NEWER
        bool ok = req.result == UnityWebRequest.Result.Success && code >= 200 && code < 300;
#else
        bool ok = !req.isHttpError && !req.isNetworkError && code >= 200 && code < 300;
#endif

        if (ok) return TryFromJson<NextRoundResponse>(text, out _);
        throw new Exception($"Advance to next round failed: HTTP {code} - {req.error} - {text}");
    }

    /// <summary>
    /// Check status of a timed debate
    /// </summary>
    public static async Task<TimedDebateStatusResponse> CheckTimedDebateStatusAsync(
        string chatId,
        string bearerToken = null,
        CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(chatId)) throw new ArgumentException("chatId is required.", nameof(chatId));

        string url = NotesDebateTimedStatusUrl(chatId);
        string authToken = await ResolveBearerTokenAsync(bearerToken, ct);

        using var req = UnityWebRequest.Get(url);
        req.SetRequestHeader("Accept", "application/json");
        req.SetRequestHeader("Authorization", "Bearer " + authToken);
        req.timeout = RequestTimeoutSeconds;

        Log($"[HTTP] GET {url} (check timed debate status)");

        var op = req.SendWebRequest();
        while (!op.isDone)
        {
            if (ct.IsCancellationRequested) { req.Abort(); ct.ThrowIfCancellationRequested(); }
            await Task.Yield();
        }

        long code = req.responseCode;
        string text = req.downloadHandler?.text ?? "";
        Log($"[HTTP] ← {code} {text}");

#if UNITY_2020_1_OR_NEWER
        bool ok = req.result == UnityWebRequest.Result.Success && code >= 200 && code < 300;
#else
        bool ok = !req.isHttpError && !req.isNetworkError && code >= 200 && code < 300;
#endif

        if (ok) return TryFromJson<TimedDebateStatusResponse>(text, out _);
        throw new Exception($"Check timed debate status failed: HTTP {code} - {req.error} - {text}");
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // OXFORD DEBATE API METHODS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Start an Oxford-style formal debate with structured phases:
    /// Opening Statements → Rebuttals → Cross-Examination (optional) → Closing Statements
    /// </summary>
    public static async Task<OxfordDebateStartResponse> StartOxfordDebateAsync(
        string noteId,
        string topic,
        string userPosition,
        bool includesCrossExamination = true,
        string bearerToken = null,
        CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(noteId)) throw new ArgumentException("noteId is required.", nameof(noteId));
        if (string.IsNullOrWhiteSpace(topic)) throw new ArgumentException("topic is required.", nameof(topic));
        if (string.IsNullOrWhiteSpace(userPosition)) throw new ArgumentException("userPosition is required.", nameof(userPosition));

        string url = NotesDebateOxfordStartUrl(noteId);
        string jsonBody = $"{{\"topic\":\"{EscapeJsonString(topic)}\",\"position\":\"{EscapeJsonString(userPosition)}\",\"includesCrossExamination\":{(includesCrossExamination ? "true" : "false")}}}";
        string authToken = await ResolveBearerTokenAsync(bearerToken, ct);

        using var req = new UnityWebRequest(url, "POST");
        req.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(jsonBody));
        req.downloadHandler = new DownloadHandlerBuffer();
        req.SetRequestHeader("Accept", "application/json");
        req.SetRequestHeader("Content-Type", "application/json");
        req.SetRequestHeader("Authorization", "Bearer " + authToken);
        req.timeout = 30;

        Log($"[HTTP] POST {url} (start Oxford debate)");

        var op = req.SendWebRequest();
        while (!op.isDone)
        {
            if (ct.IsCancellationRequested) { req.Abort(); ct.ThrowIfCancellationRequested(); }
            await Task.Yield();
        }

        long code = req.responseCode;
        string text = req.downloadHandler?.text ?? "";
        Log($"[HTTP] ← {code} {text}");

#if UNITY_2020_1_OR_NEWER
        bool ok = req.result == UnityWebRequest.Result.Success && code >= 200 && code < 300;
#else
        bool ok = !req.isHttpError && !req.isNetworkError && code >= 200 && code < 300;
#endif

        if (ok) return TryFromJson<OxfordDebateStartResponse>(text, out _);
        throw new Exception($"Start Oxford debate failed: HTTP {code} - {req.error} - {text}");
    }

    /// <summary>
    /// Advance to the next phase in an Oxford-style debate.
    /// Scores the current phase and transitions to the next.
    /// </summary>
    public static async Task<OxfordPhaseAdvanceResponse> AdvanceOxfordPhaseAsync(
        string chatId,
        string bearerToken = null,
        CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(chatId)) throw new ArgumentException("chatId is required.", nameof(chatId));

        string url = NotesDebateOxfordAdvanceUrl(chatId);
        string authToken = await ResolveBearerTokenAsync(bearerToken, ct);

        using var req = new UnityWebRequest(url, "POST");
        req.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes("{}"));
        req.downloadHandler = new DownloadHandlerBuffer();
        req.SetRequestHeader("Accept", "application/json");
        req.SetRequestHeader("Content-Type", "application/json");
        req.SetRequestHeader("Authorization", "Bearer " + authToken);
        req.timeout = 30;

        Log($"[HTTP] POST {url} (advance Oxford phase)");

        var op = req.SendWebRequest();
        while (!op.isDone)
        {
            if (ct.IsCancellationRequested) { req.Abort(); ct.ThrowIfCancellationRequested(); }
            await Task.Yield();
        }

        long code = req.responseCode;
        string text = req.downloadHandler?.text ?? "";
        Log($"[HTTP] ← {code} {text}");

#if UNITY_2020_1_OR_NEWER
        bool ok = req.result == UnityWebRequest.Result.Success && code >= 200 && code < 300;
#else
        bool ok = !req.isHttpError && !req.isNetworkError && code >= 200 && code < 300;
#endif

        if (ok) return TryFromJson<OxfordPhaseAdvanceResponse>(text, out _);
        throw new Exception($"Advance Oxford phase failed: HTTP {code} - {req.error} - {text}");
    }

    /// <summary>
    /// Get the status of an Oxford-style debate including current phase, scores, etc.
    /// </summary>
    public static async Task<OxfordDebateStatusResponse> GetOxfordDebateStatusAsync(
        string chatId,
        string bearerToken = null,
        CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(chatId)) throw new ArgumentException("chatId is required.", nameof(chatId));

        string url = NotesDebateOxfordStatusUrl(chatId);
        string authToken = await ResolveBearerTokenAsync(bearerToken, ct);

        using var req = UnityWebRequest.Get(url);
        req.SetRequestHeader("Accept", "application/json");
        req.SetRequestHeader("Authorization", "Bearer " + authToken);
        req.timeout = RequestTimeoutSeconds;

        Log($"[HTTP] GET {url} (get Oxford debate status)");

        var op = req.SendWebRequest();
        while (!op.isDone)
        {
            if (ct.IsCancellationRequested) { req.Abort(); ct.ThrowIfCancellationRequested(); }
            await Task.Yield();
        }

        long code = req.responseCode;
        string text = req.downloadHandler?.text ?? "";
        Log($"[HTTP] ← {code} {text}");

#if UNITY_2020_1_OR_NEWER
        bool ok = req.result == UnityWebRequest.Result.Success && code >= 200 && code < 300;
#else
        bool ok = !req.isHttpError && !req.isNetworkError && code >= 200 && code < 300;
#endif

        if (ok) return TryFromJson<OxfordDebateStatusResponse>(text, out _);
        throw new Exception($"Get Oxford debate status failed: HTTP {code} - {req.error} - {text}");
    }

    /// <summary>
    /// Get flashcards for a note (for link preview display)
    /// </summary>
    public static async Task<FlashcardsQuizResponse> GetFlashcardsAsync(
        string noteId,
        string bearerToken = null,
        CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(noteId))
            throw new ArgumentException("noteId is required.", nameof(noteId));

        string url = NotesFlashcardsQuizzesUrl(noteId);
        string authToken = await ResolveBearerTokenAsync(bearerToken, ct);

        using var req = new UnityWebRequest(url, "POST");
        req.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes("{}"));
        req.downloadHandler = new DownloadHandlerBuffer();
        req.SetRequestHeader("Accept", "application/json");
        req.SetRequestHeader("Content-Type", "application/json");
        req.SetRequestHeader("Authorization", "Bearer " + authToken);
        req.timeout = 60; // Longer timeout for AI generation

        Log($"[HTTP] POST {url} (generate flashcards)");

        var op = req.SendWebRequest();
        float start = Time.realtimeSinceStartup;

        while (!op.isDone)
        {
            if (ct.IsCancellationRequested) { req.Abort(); ct.ThrowIfCancellationRequested(); }
            if (Time.realtimeSinceStartup - start > 60f)
            {
                req.Abort();
                throw new TimeoutException("Flashcard generation timed out after 60s");
            }
            await Task.Yield();
        }

        long code = req.responseCode;
        string text = req.downloadHandler?.text ?? "";
        Log($"[HTTP] ← {code} {text}");

#if UNITY_2020_1_OR_NEWER
        bool ok = req.result == UnityWebRequest.Result.Success && code >= 200 && code < 300;
#else
        bool ok = !req.isHttpError && !req.isNetworkError && code >= 200 && code < 300;
#endif

        if (ok)
        {
            Exception parseEx;
            return TryFromJson<FlashcardsQuizResponse>(text, out parseEx);
        }

        throw new Exception($"Get flashcards failed: HTTP {code} - {req.error} - {text}");
    }

    private static string EscapeJsonString(string str)
    {
        if (string.IsNullOrEmpty(str)) return str;
        return str
            .Replace("\\", "\\\\")
            .Replace("\"", "\\\"")
            .Replace("\n", "\\n")
            .Replace("\r", "\\r")
            .Replace("\t", "\\t");
    }

    #endregion

    #region Notes: Generate Flashcards & Quizzes

    [Serializable]
    public class FlashcardData
    {
        public string question;
        public string answer;
    }

    [Serializable]
    public class QuizQuestion
    {
        public string question;
        public string[] options;
        public string answer;
        public string explanation;
        public string type;
        public int points;
        public int order;
        public float source;  // Can be timestamp (float) or index
        public string sourceType;
    }

    [Serializable]
    public class QuizData
    {
        public string title;
        public string description;
        public string difficulty;
        public int timeLimit;
        public QuizQuestion[] questions;
    }

    [Serializable]
    public class FlashcardsAndQuizResponse
    {
        public bool status;
        public string message;
        public FlashcardsAndQuizInnerData data;

        [Serializable]
        public class FlashcardsAndQuizInnerData
        {
            public FlashcardData[] flashcards;
            public QuizData quiz;
        }
    }

    public static async Task<FlashcardsAndQuizResponse> GenerateFlashcardsAndQuizAsync(
        string noteId,
        string bearerToken,
        CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(noteId))
            throw new ArgumentException("NoteId cannot be empty.", nameof(noteId));

        // Backend requires UUID format in the path
        if (!LooksLikeGuid(noteId))
            throw new ArgumentException($"noteId must be a UUID. Got '{noteId}'.", nameof(noteId));

        string url = $"https://ai.intelli-verse-x.ai/api/ai/notes/{noteId}/generate-flashcards-quizzes";

        // Masked cURL (useful in logs)
        PrintCurl("POST", url, new Dictionary<string, string> {
        { "Accept", "*/*" },
        { "Content-Type", "application/json" },
        { "Authorization", "Bearer ****" }
    }, "{}");

        // -------- Local auth helpers (self-contained) --------
        const int MaxResolveTries = 3;

        static bool IsAuthError(Exception ex)
        {
            var m = ex?.Message ?? string.Empty;
            return m.IndexOf("401", StringComparison.OrdinalIgnoreCase) >= 0 ||
                   m.IndexOf("unauthorized", StringComparison.OrdinalIgnoreCase) >= 0;
        }

        static bool IsUsableJwt(string token)
        {
            if (string.IsNullOrWhiteSpace(token)) return false;
            // Allow opaque tokens; if not JWT-like, treat as usable.
            int dots = 0; foreach (var c in token) if (c == '.') dots++;
            if (dots != 2) return true;

            try
            {
                string Pad(string s) { s = s.Replace('-', '+').Replace('_', '/'); switch (s.Length % 4) { case 2: s += "=="; break; case 3: s += "="; break; } return s; }
                var parts = token.Split('.');
                var payloadJson = Encoding.UTF8.GetString(Convert.FromBase64String(Pad(parts[1])));
                var expIdx = payloadJson.IndexOf("\"exp\":", StringComparison.OrdinalIgnoreCase);
                if (expIdx < 0) return true;
                var tail = payloadJson.Substring(expIdx + 6);
                var end = tail.IndexOfAny(new[] { ',', '}', ' ' });
                var numStr = end >= 0 ? tail.Substring(0, end) : tail;
                if (!long.TryParse(new string(numStr.Where(char.IsDigit).ToArray()), out var exp)) return true;
                var expUtc = DateTimeOffset.FromUnixTimeSeconds(exp).UtcDateTime;
                return DateTime.UtcNow < expUtc.AddSeconds(-30);
            }
            catch { return true; }
        }

        async Task<string> GetBearerTokenResilientlyAsync(string provided, bool forceOAuth, CancellationToken tokenCt)
        {
            if (!forceOAuth)
            {
                // 1) Respect explicit provided token if usable
                if (!string.IsNullOrWhiteSpace(provided) && IsUsableJwt(provided))
                {
                    Log("[Auth] Using explicit provided bearer token.");
                    return provided;
                }

                // 2) Resolve user token with refresh+retries
                for (int i = 0; i < MaxResolveTries; i++)
                {
                    try
                    {
                        var userTok = await ResolveBearerTokenAsync(provided, tokenCt);
                        if (IsUsableJwt(userTok))
                        {
                            Log($"[Auth] Using user/provided token (attempt {i + 1}/{MaxResolveTries}).");
                            return userTok;
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
                            await RefreshUserTokenAsync(tokenCt);
                        }
                        catch (Exception rex)
                        {
                            Log($"[Auth] Refresh failed: {rex.Message}");
                        }
                    }
                }
            }

            // 3) OAuth client-credentials fallback
            Log("[Auth] Falling back to OAuth client-credentials token.");
            var oauth = await EnsureTokenAsync(tokenCt);
            if (string.IsNullOrWhiteSpace(oauth))
                throw new Exception("OAuth fallback token acquisition returned empty token.");
            return oauth;
        }

        // -------- Main request loop --------
        Exception lastErr = null;
        bool forceOAuthNextAttempt = false;

        for (int attempt = 0; attempt <= MaxRetries; attempt++)
        {
            if (attempt > 0)
            {
                var jitter = 1f + (float)((_rng.NextDouble() * 2 - 1) * JitterPct);
                float delayS = RetryBackoffBaseSeconds * Mathf.Pow(2f, attempt) * jitter;
                Log($"[HTTP] Retry (flashcards/quizzes) #{attempt} after backoff {delayS:0.00}s …");
                await Task.Delay(TimeSpan.FromSeconds(delayS), ct);
            }

            using (var req = new UnityWebRequest(url, UnityWebRequest.kHttpVerbPOST))
            {
                try
                {
                    var authToken = await GetBearerTokenResilientlyAsync(bearerToken, forceOAuthNextAttempt, ct);

#if UNITY_EDITOR
                    var sub = TryGetJwtSub(authToken);
                    if (!string.IsNullOrEmpty(sub))
                        Log($"[Auth] JWT sub: {sub}  (GUID? {LooksLikeGuid(sub)})");
#endif

                    // Some servers reject zero-length bodies; send "{}"
                    var bodyBytes = System.Text.Encoding.UTF8.GetBytes("{}");
                    req.uploadHandler = new UploadHandlerRaw(bodyBytes);
                    req.downloadHandler = new DownloadHandlerBuffer();

                    // Match cURL as closely as possible
                    req.SetRequestHeader("Accept", "*/*");
                    req.SetRequestHeader("Content-Type", "application/json; charset=utf-8");
                    req.SetRequestHeader("Authorization", "Bearer " + authToken);
                    req.timeout = RequestTimeoutSeconds;

                    Log($"[HTTP] POST {url} (generate flashcards & quizzes)");

                    var op = req.SendWebRequest();
                    float start = Time.realtimeSinceStartup;

                    while (!op.isDone)
                    {
                        if (ct.IsCancellationRequested) { req.Abort(); ct.ThrowIfCancellationRequested(); }
                        if (Time.realtimeSinceStartup - start > RequestTimeoutSeconds)
                        {
                            req.Abort();
                            throw new TimeoutException($"Request timed out after {RequestTimeoutSeconds}s");
                        }
                        await Task.Yield();
                    }

                    long code = req.responseCode;
                    string text = req.downloadHandler?.text ?? "";
                    Log($"[HTTP] ← {code} {text}");

#if UNITY_2020_1_OR_NEWER
                    bool isNetErr = req.result == UnityWebRequest.Result.ConnectionError;
                    bool isHttpErr = req.result == UnityWebRequest.Result.ProtocolError;
#else
                bool isNetErr  = req.isNetworkError;
                bool isHttpErr = req.isHttpError;
#endif

                    // 2xx — return parsed payload (even if status:false) so UI can show message
                    if (!isNetErr && !isHttpErr && code >= 200 && code < 300)
                    {
                        Exception parseEx;
                        var resp = TryFromJson<FlashcardsAndQuizResponse>(text, out parseEx)
                                   ?? new FlashcardsAndQuizResponse { status = false, message = text, data = null };

                        return resp;
                    }

                    // 202 Accepted — treat as retryable (server is still working)
                    if (code == 202)
                    {
                        var retryAfter = req.GetResponseHeader("Retry-After");
                        if (int.TryParse(retryAfter, out int secs) && attempt < MaxRetries)
                        {
                            Log($"[HTTP] 202 Accepted. Retrying after {secs}s …");
                            await Task.Delay(TimeSpan.FromSeconds(secs), ct);
                            continue;
                        }
                        // else fall through to normal backoff loop
                    }

                    // 401 — force OAuth next attempt
                    if (code == 401 && attempt < MaxRetries)
                    {
                        Log("[HTTP] 401 on generate; forcing OAuth token on next attempt.");
                        forceOAuthNextAttempt = true;

                        // If already on OAuth, reacquire once and retry
                        try { await EnsureTokenAsync(ct); } catch { /* ignore */ }
                        continue;
                    }

                    // 429 — respect Retry-After if present
                    if (code == 429 && attempt < MaxRetries)
                    {
                        var retryAfter = req.GetResponseHeader("Retry-After");
                        if (int.TryParse(retryAfter, out int secs429))
                        {
                            Log($"[HTTP] 429 Too Many Requests. Retrying after {secs429}s …");
                            await Task.Delay(TimeSpan.FromSeconds(secs429), ct);
                            continue;
                        }
                        continue; // let outer backoff handle if header missing
                    }

                    // Other transient errors: 408/423/425/5xx or network errors
                    bool retryable = code == 408 || code == 423 || code == 425
                                     || (code >= 500 && code <= 599) || isNetErr;
                    if (retryable && attempt < MaxRetries) continue;

                    // Non-retryable 4xx — try to return server JSON so UI can show the message
                    if (code >= 400 && code < 500)
                    {
                        Exception parseClientEx;
                        var clientResp = TryFromJson<FlashcardsAndQuizResponse>(text, out parseClientEx)
                                         ?? new FlashcardsAndQuizResponse { status = false, message = text, data = null };
                        return clientResp;
                    }

                    throw new Exception($"Generate flashcards/quizzes failed: HTTP {code} - {req.error} - {text}");
                }
                catch (OperationCanceledException)
                {
                    LogError("[HTTP] Flashcards/quizzes request cancelled.");
                    throw;
                }
                catch (TimeoutException tex)
                {
                    lastErr = tex;
                    LogError($"[HTTP] Timeout (flashcards/quizzes): {tex.Message}");
                    if (attempt >= MaxRetries) throw;
                }
                catch (Exception ex)
                {
                    lastErr = ex;
                    LogError($"[HTTP] Attempt (flashcards/quizzes) #{attempt} failed: {ex.GetType().Name} – {ex.Message}");
                    if (IsAuthError(ex))
                    {
                        Log("[Auth] Auth-like error detected; will force OAuth on next attempt.");
                        forceOAuthNextAttempt = true;
                    }
                    if (attempt >= MaxRetries) throw;
                }
            }
        }

        throw lastErr ?? new Exception("Unknown flashcards/quizzes request error");
    }


    #endregion

    #region User Referral API

    // === Endpoint ===
    public static string ReferralUrlEndpoint = "https://api.intelli-verse-x.ai/api/user/referral/url";

    // === DTOs ===
    [Serializable]
    public class ReferralUrlData
    {
        public string referralCode;
        public string referralUrl;
    }

    [Serializable]
    public class ReferralUrlResponse
    {
        public bool status;
        public string message;
        public ReferralUrlData data;
        public object other; // ignored if present
    }

    /// <summary>
    /// Returns the current user's referral URL/code. Uses:
    ///  - Provided bearer token (if not null),
    ///  - Otherwise your runtime user-auth (auto-refresh),
    ///  - Otherwise the persisted token from UserSessionManager.Current.
    /// Robust against 401/408/429/5xx with exponential backoff and Retry-After.
    /// </summary>
    public static async Task<ReferralUrlResponse> GetReferralUrlAsync(
        string bearerToken = null,
        CancellationToken ct = default)
    {
        // Allow calling without a token: use persisted token if runtime user-auth is off.
        var fallbackFromSession = UserSessionManager.Current?.accessToken;
        var tokenToTry = bearerToken ?? fallbackFromSession;

        // Masked cURL preview
        PrintCurl("GET", ReferralUrlEndpoint,
            new Dictionary<string, string> {
            { "accept", "application/json" },
            { "Authorization", "Bearer ****" }
            },
            null);

        Exception lastErr = null;
        bool triedAutoEnableFromSession = false;

        for (int attempt = 0; attempt <= MaxRetries; attempt++)
        {
            if (attempt > 0)
            {
                var jitter = 1f + (float)((_rng.NextDouble() * 2 - 1) * JitterPct);
                float delaySec = RetryBackoffBaseSeconds * Mathf.Pow(2f, attempt) * jitter;
                Log($"[HTTP] Retry (referral) #{attempt} after backoff {delaySec:0.00}s …");
                await Task.Delay(TimeSpan.FromSeconds(delaySec), ct);
            }

            using (var req = UnityWebRequest.Get(ReferralUrlEndpoint))
            {
                try
                {
                    // Resolve bearer (supports runtime user-auth auto-refresh path)
                    var resolved = await ResolveBearerTokenAsync(tokenToTry, ct);
                    if (string.IsNullOrWhiteSpace(resolved))
                        throw new InvalidOperationException("No bearer token available. Please login or enable runtime user-auth.");

                    req.SetRequestHeader("accept", "application/json");
                    req.SetRequestHeader("Authorization", "Bearer " + resolved);
                    req.timeout = RequestTimeoutSeconds;

                    Log($"[HTTP] GET {ReferralUrlEndpoint} (referral/url)");

                    var op = req.SendWebRequest();
                    float start = Time.realtimeSinceStartup;

                    while (!op.isDone)
                    {
                        if (ct.IsCancellationRequested) { req.Abort(); ct.ThrowIfCancellationRequested(); }
                        if (Time.realtimeSinceStartup - start > RequestTimeoutSeconds)
                        {
                            req.Abort();
                            throw new TimeoutException($"Request timed out after {RequestTimeoutSeconds}s");
                        }
                        await Task.Yield();
                    }

#if UNITY_2020_1_OR_NEWER
                    bool isNetErr = req.result == UnityWebRequest.Result.ConnectionError;
                    bool isHttpErr = req.result == UnityWebRequest.Result.ProtocolError;
#else
                bool isNetErr  = req.isNetworkError;
                bool isHttpErr = req.isHttpError;
#endif
                    long code = req.responseCode;
                    string text = req.downloadHandler?.text ?? "";
                    string preview = text.Length > 1000 ? text.Substring(0, 1000) + "...(truncated)" : text;
                    Log($"[HTTP] ← {code} {preview}");

                    // Success (2xx)
                    if (!isNetErr && !isHttpErr && code >= 200 && code < 300)
                    {
                        ReferralUrlResponse resp = null;
                        try { resp = JsonUtility.FromJson<ReferralUrlResponse>(text); } catch { /* tolerate shape drift */ }

                        // Best-effort fallback if server shape changes
                        if (resp == null)
                            resp = new ReferralUrlResponse { status = true, message = string.IsNullOrWhiteSpace(text) ? "OK" : text, data = null };

                        // If API delivered only the code (rare), synthesize URL as a convenience
                        if (resp.data != null &&
                            string.IsNullOrWhiteSpace(resp.data.referralUrl) &&
                            !string.IsNullOrWhiteSpace(resp.data.referralCode))
                        {
                            // Fallback base derived from current public flow (safe default)
                            resp.data.referralUrl = "https://intelli-verse-x.ai/auth?mode=signup&ref=" + UnityWebRequest.EscapeURL(resp.data.referralCode);
                        }

                        return resp;
                    }

                    // Handle 401:
                    //  - If runtime user-auth is active/configured, refresh and retry.
                    //  - Else if we have a saved session with refresh creds, enable runtime user-auth once and retry.
                    if (code == 401)
                    {
                        if (_useUserAuthToken && !string.IsNullOrWhiteSpace(_userIdpUsername) && !string.IsNullOrWhiteSpace(_userRefreshToken) && attempt < MaxRetries)
                        {
                            Log("[HTTP] 401 (referral) – refreshing runtime user token and retrying …");
                            await RefreshUserTokenAsync(ct);
                            tokenToTry = null; // force ResolveBearerTokenAsync to use runtime auth on next loop
                            continue;
                        }

                        if (!triedAutoEnableFromSession && attempt < MaxRetries)
                        {
                            var s = UserSessionManager.Current;
                            if (s != null && !string.IsNullOrWhiteSpace(s.idpUsername) && !string.IsNullOrWhiteSpace(s.refreshToken))
                            {
                                triedAutoEnableFromSession = true;
                                Log("[HTTP] 401 (referral) – enabling runtime user-auth from saved session and retrying …");
                                try
                                {
                                    ConfigureUserAuth(
                                        useUserAuthToken: true,
                                        idpUsername: s.idpUsername,
                                        refreshToken: s.refreshToken,
                                        initialAccessToken: s.accessToken,
                                        accessTokenExpiresInEpoch: s.accessTokenExpiryEpoch > 0 ? s.accessTokenExpiryEpoch : (long?)null
                                    );
                                    tokenToTry = null; // next iteration will use runtime auth
                                    continue;
                                }
                                catch (Exception e)
                                {
                                    LogError("[HTTP] Failed to enable runtime auth from session: " + e.Message);
                                }
                            }
                        }
                    }

                    // Honor Retry-After for 429
                    if (code == 429 && attempt < MaxRetries)
                    {
                        var retryAfter = req.GetResponseHeader("Retry-After");
                        if (int.TryParse(retryAfter, out int ra) && ra > 0)
                        {
                            Log($"[HTTP] 429 Rate limited – honoring Retry-After: {ra}s");
                            await Task.Delay(TimeSpan.FromSeconds(ra), ct);
                            continue;
                        }
                    }

                    bool retryable = code == 408 || code == 429 || (code >= 500 && code <= 599) || isNetErr;
                    if (retryable && attempt < MaxRetries) continue;

                    throw new Exception($"Referral URL failed: HTTP {code} - {(string.IsNullOrWhiteSpace(text) ? req.error : text)}");
                }
                catch (OperationCanceledException) { LogError("[HTTP] Referral request cancelled."); throw; }
                catch (TimeoutException tex) { lastErr = tex; LogError($"[HTTP] Timeout (referral): {tex.Message}"); if (attempt >= MaxRetries) throw; }
                catch (Exception ex) { lastErr = ex; LogError($"[HTTP] Attempt (referral) #{attempt} failed: {ex.GetType().Name} – {ex.Message}"); if (attempt >= MaxRetries) throw; }
            }
        }

        throw lastErr ?? new Exception("Unknown referral request error");
    }

    /// <summary>
    /// Convenience helper that returns just the referral URL string (or throws with context).
    /// Uses the same token rules as <see cref="GetReferralUrlAsync"/>.
    /// </summary>
    public static async Task<string> GetReferralUrlOnlyAsync(
        string bearerToken = null,
        CancellationToken ct = default)
    {
        var res = await GetReferralUrlAsync(bearerToken, ct);
        if (res?.data?.referralUrl != null) return res.data.referralUrl;
        if (!string.IsNullOrWhiteSpace(res?.data?.referralCode))
            return "https://intelli-verse-x.ai/auth?mode=signup&ref=" + UnityWebRequest.EscapeURL(res.data.referralCode);
        throw new Exception(string.IsNullOrWhiteSpace(res?.message) ? "Referral URL unavailable." : res.message);
    }

    #endregion

    #region User: Claim Signup Reward

    // === Endpoint ===
    public static string ClaimSignupRewardEndpoint = "https://api.intelli-verse-x.ai/api/user/user/claim-signup-reward";

    // === DTOs ===
    [Serializable]
    public class SignupRewardData
    {
        public bool success;        // true if reward granted in this call
        public string message;      // e.g., "User has already received signup reward"
        public string rewardAmount; // "50.00" (string for shape tolerance)
    }

    [Serializable]
    public class SignupRewardResponse
    {
        public bool status;              // envelope status
        public string message;           // e.g., "Signup reward processed successfully"
        public SignupRewardData data;
        public object other;             // ignored
    }

    /// <summary>
    /// Calls the signup reward endpoint. Handles:
    /// - Provided bearer or runtime user-auth (with refresh on 401)
    /// - 408/429/5xx retry with exponential backoff + Retry-After
    /// - Robust JSON shape tolerance
    /// Returns the typed envelope (even when reward already claimed).
    /// </summary>
    public static async Task<SignupRewardResponse> ClaimSignupRewardAsync(
        string bearerToken = null,
        CancellationToken ct = default)
    {
        // Allow calling without explicit token; prefer runtime user-auth if enabled,
        // else fallback to persisted session's token for convenience.
        var fallbackFromSession = UserSessionManager.Current?.accessToken;
        var tokenToTry = bearerToken ?? fallbackFromSession;

        // Masked cURL preview
        PrintCurl("POST", ClaimSignupRewardEndpoint,
            new Dictionary<string, string> {
            { "accept", "application/json" },
            { "Authorization", "Bearer ****" }
            },
            "" // body is empty
        );

        Exception lastErr = null;
        bool triedAutoEnableFromSession = false;

        for (int attempt = 0; attempt <= MaxRetries; attempt++)
        {
            if (attempt > 0)
            {
                var jitter = 1f + (float)((_rng.NextDouble() * 2 - 1) * JitterPct);
                float delaySec = RetryBackoffBaseSeconds * Mathf.Pow(2f, attempt) * jitter;
                Log($"[HTTP] Retry (signup-reward) #{attempt} after backoff {delaySec:0.00}s …");
                await Task.Delay(TimeSpan.FromSeconds(delaySec), ct);
            }

            using (var req = new UnityWebRequest(ClaimSignupRewardEndpoint, UnityWebRequest.kHttpVerbPOST))
            {
                try
                {
                    // Some gateways prefer explicit zero-length body
                    req.uploadHandler = new UploadHandlerRaw(Array.Empty<byte>());
                    req.downloadHandler = new DownloadHandlerBuffer();

                    // Resolve token (runtime user-auth auto-refresh path supported below)
                    var resolved = await ResolveBearerTokenAsync(tokenToTry, ct);
                    if (string.IsNullOrWhiteSpace(resolved))
                        throw new InvalidOperationException("No bearer token available. Please login or enable runtime user-auth.");

                    req.SetRequestHeader("accept", "application/json");
                    req.SetRequestHeader("Authorization", "Bearer " + resolved);
                    req.timeout = RequestTimeoutSeconds;

                    Log($"[HTTP] POST {ClaimSignupRewardEndpoint} (claim-signup-reward)");

                    var op = req.SendWebRequest();
                    float start = Time.realtimeSinceStartup;

                    while (!op.isDone)
                    {
                        if (ct.IsCancellationRequested) { req.Abort(); ct.ThrowIfCancellationRequested(); }
                        if (Time.realtimeSinceStartup - start > RequestTimeoutSeconds)
                        {
                            req.Abort();
                            throw new TimeoutException($"Request timed out after {RequestTimeoutSeconds}s");
                        }
                        await Task.Yield();
                    }

#if UNITY_2020_1_OR_NEWER
                    bool isNetErr = req.result == UnityWebRequest.Result.ConnectionError;
                    bool isHttpErr = req.result == UnityWebRequest.Result.ProtocolError;
#else
                bool isNetErr  = req.isNetworkError;
                bool isHttpErr = req.isHttpError;
#endif
                    long code = req.responseCode;
                    string text = req.downloadHandler?.text ?? "";
                    string preview = text.Length > 1000 ? text.Substring(0, 1000) + "...(truncated)" : text;
                    Log($"[HTTP] ← {code} {preview}");

                    // Success (2xx)
                    if (!isNetErr && !isHttpErr && code >= 200 && code < 300)
                    {
                        // Tolerant parse
                        SignupRewardResponse resp = null;
                        try { resp = JsonUtility.FromJson<SignupRewardResponse>(text); } catch { /* tolerate */ }

                        if (resp == null)
                        {
                            // Fallback: synthesize minimal response from raw
                            resp = new SignupRewardResponse
                            {
                                status = true,
                                message = string.IsNullOrWhiteSpace(text) ? "OK" : text,
                                data = new SignupRewardData
                                {
                                    success = text.IndexOf("\"success\":true", StringComparison.OrdinalIgnoreCase) >= 0,
                                    message = ExtractFieldLoose(text, "message"),
                                    rewardAmount = ExtractFieldLoose(text, "rewardAmount")
                                }
                            };
                        }
                        else
                        {
                            // If amount missing but present in raw, try to extract
                            if (resp.data != null && string.IsNullOrWhiteSpace(resp.data.rewardAmount))
                                resp.data.rewardAmount = ExtractFieldLoose(text, "rewardAmount");
                        }

                        return resp;
                    }

                    // 401 → attempt refresh (or auto-enable runtime user-auth from saved session once)
                    if (code == 401)
                    {
                        if (_useUserAuthToken && !string.IsNullOrWhiteSpace(_userIdpUsername) && !string.IsNullOrWhiteSpace(_userRefreshToken) && attempt < MaxRetries)
                        {
                            Log("[HTTP] 401 (signup-reward) – refreshing runtime user token and retrying …");
                            await RefreshUserTokenAsync(ct);
                            tokenToTry = null; // next loop will use runtime auth
                            continue;
                        }

                        if (!triedAutoEnableFromSession && attempt < MaxRetries)
                        {
                            var s = UserSessionManager.Current;
                            if (s != null && !string.IsNullOrWhiteSpace(s.idpUsername) && !string.IsNullOrWhiteSpace(s.refreshToken))
                            {
                                triedAutoEnableFromSession = true;
                                Log("[HTTP] 401 (signup-reward) – enabling runtime user-auth from saved session and retrying …");
                                try
                                {
                                    ConfigureUserAuth(
                                        useUserAuthToken: true,
                                        idpUsername: s.idpUsername,
                                        refreshToken: s.refreshToken,
                                        initialAccessToken: s.accessToken,
                                        accessTokenExpiresInEpoch: s.accessTokenExpiryEpoch > 0 ? s.accessTokenExpiryEpoch : (long?)null
                                    );
                                    tokenToTry = null;
                                    continue;
                                }
                                catch (Exception e)
                                {
                                    LogError("[HTTP] Failed to enable runtime auth from session: " + e.Message);
                                }
                            }
                        }
                    }

                    // Honor Retry-After for 429
                    if (code == 429 && attempt < MaxRetries)
                    {
                        var retryAfter = req.GetResponseHeader("Retry-After");
                        if (int.TryParse(retryAfter, out int ra) && ra > 0)
                        {
                            Log($"[HTTP] 429 Rate limited – honoring Retry-After: {ra}s");
                            await Task.Delay(TimeSpan.FromSeconds(ra), ct);
                            continue;
                        }
                    }

                    bool retryable = code == 408 || code == 429 || (code >= 500 && code <= 599) || isNetErr;
                    if (retryable && attempt < MaxRetries) continue;

                    // If non-2xx but server returned a body, try to surface amount/message
                    if (!string.IsNullOrWhiteSpace(text))
                    {
                        var fallback = new SignupRewardResponse
                        {
                            status = false,
                            message = $"HTTP {code}",
                            data = new SignupRewardData
                            {
                                success = text.IndexOf("\"success\":true", StringComparison.OrdinalIgnoreCase) >= 0,
                                message = ExtractFieldLoose(text, "message"),
                                rewardAmount = ExtractFieldLoose(text, "rewardAmount")
                            }
                        };
                        throw new Exception($"Signup reward failed: HTTP {code} - {(string.IsNullOrWhiteSpace(fallback.data?.message) ? text : fallback.data.message)}");
                    }

                    throw new Exception($"Signup reward failed: HTTP {code} - {req.error}");
                }
                catch (OperationCanceledException) { LogError("[HTTP] Signup-reward request cancelled."); throw; }
                catch (TimeoutException tex) { lastErr = tex; LogError($"[HTTP] Timeout (signup-reward): {tex.Message}"); if (attempt >= MaxRetries) throw; }
                catch (Exception ex) { lastErr = ex; LogError($"[HTTP] Attempt (signup-reward) #{attempt} failed: {ex.GetType().Name} – {ex.Message}"); if (attempt >= MaxRetries) throw; }
            }
        }

        throw lastErr ?? new Exception("Unknown signup-reward request error");
    }

    /// <summary>
    /// Convenience helper: returns (granted, amount, message) in one go.
    /// - granted: true if reward was granted in THIS call (false if it was already claimed)
    /// - amount: parsed decimal amount (0 if unavailable)
    /// - message: server message (e.g., "User has already received signup reward")
    /// </summary>
    public static async Task<(bool granted, decimal amount, string message)> ClaimSignupRewardSummaryAsync(
        string bearerToken = null,
        CancellationToken ct = default)
    {
        var resp = await ClaimSignupRewardAsync(bearerToken, ct);

        bool granted = resp?.data?.success == true;

        // Parse amount robustly (string or number, invariant culture)
        decimal amount = 0m;
        string raw = resp?.data?.rewardAmount;
        if (!string.IsNullOrWhiteSpace(raw))
        {
            if (!decimal.TryParse(raw, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out amount))
            {
                // strip non-numeric except dot
                var cleaned = System.Text.RegularExpressions.Regex.Replace(raw, @"[^\d.]+", "");
                decimal.TryParse(cleaned, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out amount);
            }
        }
        else
        {
            // Final fallback: scan last raw body captured in logs (not stored) is not available here,
            // so keep 0 if missing — server normally returns rewardAmount even when already claimed.
            amount = 0m;
        }

        string msg = resp?.data?.message ?? resp?.message ?? "";
        return (granted, amount, msg);
    }

    /// <summary>
    /// One-liner if you only need the numeric amount (throws with context if not available).
    /// Note: backend returns the amount even when already claimed.
    /// </summary>
    public static async Task<decimal> ClaimSignupRewardAmountOnlyAsync(
        string bearerToken = null,
        CancellationToken ct = default)
    {
        var (granted, amount, msg) = await ClaimSignupRewardSummaryAsync(bearerToken, ct);
        // We still return the amount regardless of granted/already-claimed.
        // If amount is zero and that's unexpected for your product, you can choose to throw instead:
        // if (amount <= 0m) throw new Exception(string.IsNullOrWhiteSpace(msg) ? "Reward amount unavailable." : msg);
        return amount;
    }

    // === Local helpers (region-scoped) ===
    private static string ExtractFieldLoose(string json, string field)
    {
        if (string.IsNullOrEmpty(json) || string.IsNullOrEmpty(field)) return null;

        // Matches:  "field": "value"  OR  "field": 50.00
        var m = System.Text.RegularExpressions.Regex.Match(
            json,
            $"\"{System.Text.RegularExpressions.Regex.Escape(field)}\"\\s*:\\s*(\"(?<s>[^\"]*)\"|(?<n>[-]?[0-9]+(?:\\.[0-9]+)?))",
            System.Text.RegularExpressions.RegexOptions.IgnoreCase | System.Text.RegularExpressions.RegexOptions.Multiline
        );
        if (!m.Success) return null;

        if (m.Groups["s"].Success) return m.Groups["s"].Value;
        if (m.Groups["n"].Success) return m.Groups["n"].Value;
        return null;
    }

    #endregion

    // ========================================================================
    // FRIENDS API
    // ========================================================================
    // Production-ready Friends system API calls
    // Uses centralized IVXURLs and IVXModels
    // ========================================================================

    #region Friends API

    /// <summary>
    /// Gets the current user session. Returns null if not logged in.
    /// </summary>
    public static UserSessionManager.UserSession GetCurrentSession() => UserSessionManager.Current;

    /// <summary>
    /// Log warning (if debug enabled).
    /// </summary>
    private static void LogWarning(string msg) { if (DebugLogs) Debug.LogWarning(msg); OnLog?.Invoke("[WARN] " + msg); }

    /// <summary>
    /// Generic authenticated GET request.
    /// </summary>
    private static async Task GetAuthenticatedRequestAsync(
        string url,
        Action<string> onSuccess,
        Action<string> onError,
        CancellationToken ct = default)
    {
        try
        {
            string token = await ResolveBearerTokenAsync(null, ct);
            string authHeader = string.IsNullOrWhiteSpace(token) ? null : $"Bearer {token}";

            using (var req = UnityWebRequest.Get(url))
            {
                if (!string.IsNullOrEmpty(authHeader))
                    req.SetRequestHeader("Authorization", authHeader);
                req.SetRequestHeader("Accept", "application/json");
                req.timeout = RequestTimeoutSeconds;

                PrintCurl("GET", url, new Dictionary<string, string> {
                    { "Accept", "application/json" },
                    { "Authorization", string.IsNullOrWhiteSpace(token) ? "" : "Bearer ***" }
                }, null);

                var op = req.SendWebRequest();
                float start = Time.realtimeSinceStartup;

                while (!op.isDone)
                {
                    if (ct.IsCancellationRequested) { req.Abort(); ct.ThrowIfCancellationRequested(); }
                    if (Time.realtimeSinceStartup - start > RequestTimeoutSeconds)
                    {
                        req.Abort();
                        throw new TimeoutException($"Request timed out after {RequestTimeoutSeconds}s");
                    }
                    await Task.Yield();
                }

#if UNITY_2020_1_OR_NEWER
                bool isNetErr = req.result == UnityWebRequest.Result.ConnectionError;
                bool isHttpErr = req.result == UnityWebRequest.Result.ProtocolError;
#else
                bool isNetErr = req.isNetworkError;
                bool isHttpErr = req.isHttpError;
#endif
                string text = req.downloadHandler?.text ?? "";
                long code = req.responseCode;

                Log($"[HTTP] GET ← {code} {(text.Length > 500 ? text.Substring(0, 500) + "..." : text)}");

                if (!isNetErr && !isHttpErr && code >= 200 && code < 300)
                {
                    onSuccess?.Invoke(text);
                }
                else
                {
                    onError?.Invoke($"HTTP {code}: {req.error} - {text}");
                }
            }
        }
        catch (Exception ex)
        {
            onError?.Invoke(ex.Message);
        }
    }

    /// <summary>
    /// Generic authenticated POST request.
    /// </summary>
    private static async Task PostAuthenticatedRequestAsync<T>(
        string url,
        T payload,
        Action<string> onSuccess,
        Action<string> onError,
        CancellationToken ct = default)
    {
        try
        {
            string token = await ResolveBearerTokenAsync(null, ct);
            string authHeader = string.IsNullOrWhiteSpace(token) ? null : $"Bearer {token}";
            string json = JsonUtility.ToJson(payload);

            using (var req = new UnityWebRequest(url, UnityWebRequest.kHttpVerbPOST))
            {
                req.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(json));
                req.downloadHandler = new DownloadHandlerBuffer();
                req.SetRequestHeader("Content-Type", "application/json");
                if (!string.IsNullOrEmpty(authHeader))
                    req.SetRequestHeader("Authorization", authHeader);
                req.timeout = RequestTimeoutSeconds;

                PrintCurl("POST", url, new Dictionary<string, string> {
                    { "Content-Type", "application/json" },
                    { "Authorization", string.IsNullOrWhiteSpace(token) ? "" : "Bearer ***" }
                }, json);

                var op = req.SendWebRequest();
                float start = Time.realtimeSinceStartup;

                while (!op.isDone)
                {
                    if (ct.IsCancellationRequested) { req.Abort(); ct.ThrowIfCancellationRequested(); }
                    if (Time.realtimeSinceStartup - start > RequestTimeoutSeconds)
                    {
                        req.Abort();
                        throw new TimeoutException($"Request timed out after {RequestTimeoutSeconds}s");
                    }
                    await Task.Yield();
                }

#if UNITY_2020_1_OR_NEWER
                bool isNetErr = req.result == UnityWebRequest.Result.ConnectionError;
                bool isHttpErr = req.result == UnityWebRequest.Result.ProtocolError;
#else
                bool isNetErr = req.isNetworkError;
                bool isHttpErr = req.isHttpError;
#endif
                string text = req.downloadHandler?.text ?? "";
                long code = req.responseCode;

                Log($"[HTTP] POST ← {code} {(text.Length > 500 ? text.Substring(0, 500) + "..." : text)}");

                if (!isNetErr && !isHttpErr && code >= 200 && code < 300)
                {
                    onSuccess?.Invoke(text);
                }
                else
                {
                    onError?.Invoke($"HTTP {code}: {req.error} - {text}");
                }
            }
        }
        catch (Exception ex)
        {
            onError?.Invoke(ex.Message);
        }
    }

    /// <summary>
    /// Generic authenticated PATCH request.
    /// </summary>
    private static async Task PatchAuthenticatedRequestAsync<T>(
        string url,
        T payload,
        Action<string> onSuccess,
        Action<string> onError,
        CancellationToken ct = default)
    {
        try
        {
            string token = await ResolveBearerTokenAsync(null, ct);
            string authHeader = string.IsNullOrWhiteSpace(token) ? null : $"Bearer {token}";
            string json = JsonUtility.ToJson(payload);

            using (var req = new UnityWebRequest(url, "PATCH"))
            {
                req.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(json));
                req.downloadHandler = new DownloadHandlerBuffer();
                req.SetRequestHeader("Content-Type", "application/json");
                if (!string.IsNullOrEmpty(authHeader))
                    req.SetRequestHeader("Authorization", authHeader);
                req.timeout = RequestTimeoutSeconds;

                PrintCurl("PATCH", url, new Dictionary<string, string> {
                    { "Content-Type", "application/json" },
                    { "Authorization", string.IsNullOrWhiteSpace(token) ? "" : "Bearer ***" }
                }, json);

                var op = req.SendWebRequest();
                float start = Time.realtimeSinceStartup;

                while (!op.isDone)
                {
                    if (ct.IsCancellationRequested) { req.Abort(); ct.ThrowIfCancellationRequested(); }
                    if (Time.realtimeSinceStartup - start > RequestTimeoutSeconds)
                    {
                        req.Abort();
                        throw new TimeoutException($"Request timed out after {RequestTimeoutSeconds}s");
                    }
                    await Task.Yield();
                }

#if UNITY_2020_1_OR_NEWER
                bool isNetErr = req.result == UnityWebRequest.Result.ConnectionError;
                bool isHttpErr = req.result == UnityWebRequest.Result.ProtocolError;
#else
                bool isNetErr = req.isNetworkError;
                bool isHttpErr = req.isHttpError;
#endif
                string text = req.downloadHandler?.text ?? "";
                long code = req.responseCode;

                Log($"[HTTP] PATCH ← {code} {(text.Length > 500 ? text.Substring(0, 500) + "..." : text)}");

                if (!isNetErr && !isHttpErr && code >= 200 && code < 300)
                {
                    onSuccess?.Invoke(text);
                }
                else
                {
                    onError?.Invoke($"HTTP {code}: {req.error} - {text}");
                }
            }
        }
        catch (Exception ex)
        {
            onError?.Invoke(ex.Message);
        }
    }

    /// <summary>
    /// Fetches the friends list for a user.
    /// </summary>
    /// <param name="userId">User ID to fetch friends for</param>
    /// <param name="status">Filter by status: accepted | pending | rejected</param>
    /// <param name="query">Optional search query</param>
    /// <param name="onSuccess">Callback with list of friends</param>
    /// <param name="onError">Error callback</param>
    public static async void FetchFriendsList(
        string userId,
        string status,
        string query,
        Action<List<IVXModels.FriendData>> onSuccess,
        Action<string> onError)
    {
        string reqId = "FRI-LIST-" + Guid.NewGuid().ToString("N").Substring(0, 8);

        try
        {
            var session = GetCurrentSession();
            if (session == null)
            {
                LogError($"[{reqId}] [Friends] No session found. User must sign in first.");
                onError?.Invoke("Not signed in. Please login first.");
                return;
            }

            // Validate session has required fields
            if (string.IsNullOrWhiteSpace(session.accessToken))
            {
                LogError($"[{reqId}] [Friends] Session exists but accessToken is empty. User must re-login.");
                onError?.Invoke("Session invalid. Please login again.");
                return;
            }

            if (string.IsNullOrWhiteSpace(userId))
            {
                userId = session.userId;
                if (string.IsNullOrWhiteSpace(userId))
                {
                    LogError($"[{reqId}] [Friends] No userId available in session. Session is incomplete.");
                    onError?.Invoke("User ID not found in session. Please login again.");
                    return;
                }
            }

            string url = IVXURLs.GetFriendsUrl(userId, status, query);
            Log($"[{reqId}] [Friends] GET {url} (userId={userId}, status={status})");

            await GetAuthenticatedRequestAsync(
                url,
                response =>
                {
                    Log($"[{reqId}] [Friends] 200 OK; payload length={response?.Length ?? 0}");

                    try
                    {
                        var result = JsonUtility.FromJson<IVXModels.FriendsResponse>(response);
                        if (result?.status == true && result.data != null)
                        {
                            Log($"[{reqId}] [Friends] Parsed OK; count={result.data.Count}");
                            onSuccess?.Invoke(result.data);
                        }
                        else
                        {
                            LogWarning($"[{reqId}] [Friends] API returned no data or status=false. message='{result?.message}'");
                            onSuccess?.Invoke(new List<IVXModels.FriendData>());
                        }
                    }
                    catch (Exception ex)
                    {
                        LogError($"[{reqId}] [Friends] Parse error: {ex.Message}");
                        onError?.Invoke("Invalid friends response format.");
                    }
                },
                error =>
                {
                    LogError($"[{reqId}] [Friends] GET failed: {error}");
                    onError?.Invoke(error);
                }
            );
        }
        catch (Exception ex)
        {
            LogError($"[{reqId}] [Friends] Unexpected exception: {ex}");
            onError?.Invoke("Unexpected error while fetching friends.");
        }
    }

    /// <summary>
    /// Async version of FetchFriendsList for modern async/await usage.
    /// </summary>
    public static Task<List<IVXModels.FriendData>> GetFriendsAsync(
        string userId = null,
        string status = "accepted",
        string query = null,
        CancellationToken ct = default)
    {
        var tcs = new TaskCompletionSource<List<IVXModels.FriendData>>();
        
        FetchFriendsList(
            userId,
            status,
            query,
            friends => tcs.TrySetResult(friends ?? new List<IVXModels.FriendData>()),
            error => tcs.TrySetException(new Exception(error ?? "Unknown error fetching friends"))
        );
        
        ct.Register(() => tcs.TrySetCanceled());
        return tcs.Task;
    }

    /// <summary>
    /// Searches for users to add as friends.
    /// </summary>
    /// <param name="query">Search query (min 2 characters)</param>
    /// <param name="onSuccess">Callback with list of matching users</param>
    /// <param name="onError">Error callback</param>
    public static void SearchFriends(
        string query,
        Action<List<IVXModels.SearchUser>> onSuccess,
        Action<string> onError)
    {
        string reqId = "FRI-SEARCH-" + Guid.NewGuid().ToString("N").Substring(0, 8);

        if (string.IsNullOrWhiteSpace(query) || query.Trim().Length < 2)
        {
            Log($"[{reqId}] [Friends] Query too short → returning empty list.");
            onSuccess?.Invoke(new List<IVXModels.SearchUser>());
            return;
        }

        var session = GetCurrentSession();
        if (session == null)
        {
            LogError($"[{reqId}] [Friends] No session found. User must sign in first.");
            onError?.Invoke("Not signed in. Please login first.");
            return;
        }

        // Validate session has required fields
        if (string.IsNullOrWhiteSpace(session.accessToken))
        {
            LogError($"[{reqId}] [Friends] Session exists but accessToken is empty. User must re-login.");
            onError?.Invoke("Session invalid. Please login again.");
            return;
        }

        if (string.IsNullOrWhiteSpace(session.userId))
        {
            LogError($"[{reqId}] [Friends] Session exists but userId is empty. Session is incomplete.");
            onError?.Invoke("Session incomplete. Please login again.");
            return;
        }

        string url = IVXURLs.GetSearchFriendsUrl(query, session.userId);
        Log($"[{reqId}] [Friends] Search GET {url} (userId={session.userId})");

        _ = GetAuthenticatedRequestAsync(
            url,
            response =>
            {
                Log($"[{reqId}] [Friends] Search 200 OK; payload length={response?.Length ?? 0}");
                try
                {
                    var result = JsonUtility.FromJson<IVXModels.FriendSearchResponse>(response);
                    if (result?.status == true && result.data != null)
                    {
                        Log($"[{reqId}] [Friends] Search parsed OK; count={result.data.Count}");
                        onSuccess?.Invoke(result.data);
                    }
                    else
                    {
                        LogWarning($"[{reqId}] [Friends] Search empty/invalid. message='{result?.message}'");
                        onSuccess?.Invoke(new List<IVXModels.SearchUser>());
                    }
                }
                catch (Exception ex)
                {
                    LogError($"[{reqId}] [Friends] Search parse failed: {ex.Message}");
                    onError?.Invoke("Failed to parse search response.");
                }
            },
            error =>
            {
                LogError($"[{reqId}] [Friends] Search failed: {error}");
                onError?.Invoke(error);
            }
        );
    }

    /// <summary>
    /// Async version of SearchFriends.
    /// </summary>
    public static Task<List<IVXModels.SearchUser>> SearchFriendsAsync(
        string query,
        CancellationToken ct = default)
    {
        var tcs = new TaskCompletionSource<List<IVXModels.SearchUser>>();
        
        SearchFriends(
            query,
            users => tcs.TrySetResult(users ?? new List<IVXModels.SearchUser>()),
            error => tcs.TrySetException(new Exception(error ?? "Unknown error searching users"))
        );
        
        ct.Register(() => tcs.TrySetCanceled());
        return tcs.Task;
    }

    /// <summary>
    /// Sends a friend request/invite.
    /// </summary>
    /// <param name="receiverId">User ID to send invite to</param>
    /// <param name="onSuccess">Success callback with response</param>
    /// <param name="onError">Error callback</param>
    public static void SendFriendInvite(
        string receiverId,
        Action<string> onSuccess,
        Action<string> onError)
    {
        string reqId = "FRI-INVITE-" + Guid.NewGuid().ToString("N").Substring(0, 8);

        var session = GetCurrentSession();
        if (session == null)
        {
            LogError($"[{reqId}] [Friends] No session; user must sign in.");
            onError?.Invoke("Not signed in.");
            return;
        }

        if (string.IsNullOrWhiteSpace(receiverId))
        {
            LogError($"[{reqId}] [Friends] receiverId is required.");
            onError?.Invoke("Receiver ID is required.");
            return;
        }

        if (receiverId == session.userId)
        {
            LogError($"[{reqId}] [Friends] Cannot send invite to yourself.");
            onError?.Invoke("Cannot send friend request to yourself.");
            return;
        }

        var payload = new IVXModels.FriendInvitePayload(session.userId, receiverId);
        Log($"[{reqId}] [Friends] Invite POST → receiverId={receiverId}");

        _ = PostAuthenticatedRequestAsync(
            IVXURLs.SendFriendRequest,
            payload,
            response =>
            {
                Log($"[{reqId}] [Friends] Invite 200 OK; payload length={response?.Length ?? 0}");
                onSuccess?.Invoke(response);
            },
            error =>
            {
                LogError($"[{reqId}] [Friends] Invite failed: {error}");
                onError?.Invoke(error);
            }
        );
    }

    /// <summary>
    /// Async version of SendFriendInvite.
    /// </summary>
    public static Task<bool> SendFriendInviteAsync(
        string receiverId,
        CancellationToken ct = default)
    {
        var tcs = new TaskCompletionSource<bool>();
        
        SendFriendInvite(
            receiverId,
            _ => tcs.TrySetResult(true),
            error => tcs.TrySetException(new Exception(error))
        );
        
        ct.Register(() => tcs.TrySetCanceled());
        return tcs.Task;
    }

    /// <summary>
    /// Updates friend relationship status.
    /// </summary>
    /// <param name="relationId">The relation/request ID</param>
    /// <param name="newStatus">New status: accepted | rejected | blocked | cancelled</param>
    /// <param name="onSuccess">Success callback</param>
    /// <param name="onError">Error callback</param>
    public static void UpdateFriendStatus(
        string relationId,
        string newStatus,
        Action<string> onSuccess,
        Action<string> onError)
    {
        string reqId = "FRI-STATUS-" + Guid.NewGuid().ToString("N").Substring(0, 8);

        var session = GetCurrentSession();
        if (session == null)
        {
            LogError($"[{reqId}] [Friends] No session; user must sign in.");
            onError?.Invoke("Not signed in.");
            return;
        }

        if (string.IsNullOrWhiteSpace(relationId))
        {
            LogError($"[{reqId}] [Friends] relationId is required.");
            onError?.Invoke("Relation ID is required.");
            return;
        }

        if (string.IsNullOrWhiteSpace(newStatus))
        {
            LogError($"[{reqId}] [Friends] newStatus is required.");
            onError?.Invoke("Status is required.");
            return;
        }

        var payload = new IVXModels.FriendStatusUpdatePayload(session.userId, relationId, newStatus);
        Log($"[{reqId}] [Friends] Update PATCH → relationId={relationId}, status={newStatus}");

        _ = PatchAuthenticatedRequestAsync(
            IVXURLs.UpdateFriendStatus,
            payload,
            response =>
            {
                Log($"[{reqId}] [Friends] Update 200 OK; payload length={response?.Length ?? 0}");
                onSuccess?.Invoke(response);
            },
            error =>
            {
                LogError($"[{reqId}] [Friends] Update failed: {error}");
                onError?.Invoke(error);
            }
        );
    }

    /// <summary>
    /// Async version of UpdateFriendStatus.
    /// </summary>
    public static Task<bool> UpdateFriendStatusAsync(
        string relationId,
        string newStatus,
        CancellationToken ct = default)
    {
        var tcs = new TaskCompletionSource<bool>();
        
        UpdateFriendStatus(
            relationId,
            newStatus,
            _ => tcs.TrySetResult(true),
            error => tcs.TrySetException(new Exception(error))
        );
        
        ct.Register(() => tcs.TrySetCanceled());
        return tcs.Task;
    }

    /// <summary>
    /// Accept a friend request.
    /// </summary>
    public static Task<bool> AcceptFriendRequestAsync(string relationId, CancellationToken ct = default)
        => UpdateFriendStatusAsync(relationId, "accepted", ct);

    /// <summary>
    /// Reject a friend request.
    /// </summary>
    public static Task<bool> RejectFriendRequestAsync(string relationId, CancellationToken ct = default)
        => UpdateFriendStatusAsync(relationId, "rejected", ct);

    /// <summary>
    /// Remove a friend.
    /// </summary>
    public static Task<bool> RemoveFriendAsync(string relationId, CancellationToken ct = default)
        => UpdateFriendStatusAsync(relationId, "cancelled", ct);

    /// <summary>
    /// Block a user.
    /// </summary>
    public static Task<bool> BlockUserAsync(string relationId, CancellationToken ct = default)
        => UpdateFriendStatusAsync(relationId, "blocked", ct);

    /// <summary>
    /// Get pending incoming friend requests.
    /// </summary>
    public static Task<List<IVXModels.FriendData>> GetIncomingRequestsAsync(CancellationToken ct = default)
        => GetFriendsAsync(null, "pending", null, ct);

    /// <summary>
    /// Get accepted friends list.
    /// </summary>
    public static Task<List<IVXModels.FriendData>> GetAcceptedFriendsAsync(CancellationToken ct = default)
        => GetFriendsAsync(null, "accepted", null, ct);

    #endregion

    // ========================================================================
    // GUEST CONVERSION API (WITH OTP)
    // ========================================================================

    #region Guest Conversion

    /// <summary>
    /// Initiate guest conversion with OTP verification.
    /// </summary>
    public static async Task<IVXModels.GuestConvertInitiateResponse> InitiateGuestConversionAsync(
        string guestUserId,
        string email,
        string password,
        string userName,
        CancellationToken ct = default)
    {
        string reqId = "GUEST-INIT-" + Guid.NewGuid().ToString("N").Substring(0, 8);

        if (string.IsNullOrWhiteSpace(guestUserId))
            throw new ArgumentException("Guest user ID is required.", nameof(guestUserId));
        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("Email is required.", nameof(email));
        if (string.IsNullOrWhiteSpace(password))
            throw new ArgumentException("Password is required.", nameof(password));
        if (string.IsNullOrWhiteSpace(userName))
            throw new ArgumentException("Username is required.", nameof(userName));

        var request = new IVXModels.GuestConvertInitiateRequest
        {
            guestUserId = guestUserId,
            email = email.Trim(),
            password = password,
            userName = userName.Trim()
        };

        Log($"[{reqId}] [Guest] Convert initiate POST → {IVXURLs.ConvertGuest_V2_Init}");

        var tcs = new TaskCompletionSource<IVXModels.GuestConvertInitiateResponse>();

        await PostAuthenticatedRequestAsync(
            IVXURLs.ConvertGuest_V2_Init,
            request,
            response =>
            {
                try
                {
                    var result = JsonUtility.FromJson<IVXModels.GuestConvertInitiateResponse>(response);
                    Log($"[{reqId}] [Guest] Convert initiate OK: status={result?.status}");
                    tcs.TrySetResult(result);
                }
                catch (Exception ex)
                {
                    LogError($"[{reqId}] [Guest] Convert initiate parse error: {ex.Message}");
                    tcs.TrySetException(ex);
                }
            },
            error =>
            {
                LogError($"[{reqId}] [Guest] Convert initiate failed: {error}");
                tcs.TrySetException(new Exception(error));
            }
        );

        ct.Register(() => tcs.TrySetCanceled());
        return await tcs.Task;
    }

    /// <summary>
    /// Confirm guest conversion with OTP code.
    /// </summary>
    public static async Task<IVXModels.GuestConvertConfirmResponse> ConfirmGuestConversionAsync(
        string guestUserId,
        string email,
        string password,
        string userName,
        string otp,
        CancellationToken ct = default)
    {
        string reqId = "GUEST-CONFIRM-" + Guid.NewGuid().ToString("N").Substring(0, 8);

        if (string.IsNullOrWhiteSpace(otp))
            throw new ArgumentException("OTP code is required.", nameof(otp));

        var request = new IVXModels.GuestConvertConfirmRequest
        {
            guestUserId = guestUserId,
            email = email.Trim(),
            password = password,
            userName = userName.Trim(),
            otp = otp.Trim()
        };

        Log($"[{reqId}] [Guest] Convert confirm POST → {IVXURLs.ConvertGuest_V2_Confirm}");

        var tcs = new TaskCompletionSource<IVXModels.GuestConvertConfirmResponse>();

        await PostAuthenticatedRequestAsync(
            IVXURLs.ConvertGuest_V2_Confirm,
            request,
            response =>
            {
                try
                {
                    var result = JsonUtility.FromJson<IVXModels.GuestConvertConfirmResponse>(response);
                    Log($"[{reqId}] [Guest] Convert confirm OK: status={result?.status}");
                    tcs.TrySetResult(result);
                }
                catch (Exception ex)
                {
                    LogError($"[{reqId}] [Guest] Convert confirm parse error: {ex.Message}");
                    tcs.TrySetException(ex);
                }
            },
            error =>
            {
                LogError($"[{reqId}] [Guest] Convert confirm failed: {error}");
                tcs.TrySetException(new Exception(error));
            }
        );

        ct.Register(() => tcs.TrySetCanceled());
        return await tcs.Task;
    }

    #endregion

    // ========================================================================
    // REFERRAL STATS & CLAIM
    // ========================================================================

    #region Referral Stats & Claim

    // === Endpoints ===
    public static string ReferralStatsEndpoint = "https://api.intelli-verse-x.ai/api/user/referral/stats";
    public static string ClaimReferralRewardsEndpoint = "https://api.intelli-verse-x.ai/api/user/referral/claim";

    // === DTOs ===
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

    [Serializable]
    public class ReferralStatsResponse
    {
        public bool status;
        public string message;
        public ReferralStatsData data;
        public object other;
    }

    [Serializable]
    public class ClaimReferralRewardsRequest
    {
        public string[] referralIds;
    }

    [Serializable]
    public class ClaimReferralRewardsData
    {
        public int totalClaimed;
        public int totalRewardAmount;
        public string rewardCurrency;
        public string[] claimedReferralIds;
    }

    [Serializable]
    public class ClaimReferralRewardsResponse
    {
        public bool status;
        public string message;
        public ClaimReferralRewardsData data;
        public object other;
    }

    /// <summary>
    /// Get referral statistics for the current user.
    /// Returns total, completed, pending, and expired referral counts.
    /// </summary>
    public static async Task<ReferralStatsResponse> GetReferralStatsAsync(
        string bearerToken = null,
        CancellationToken ct = default)
    {
        var fallbackFromSession = UserSessionManager.Current?.accessToken;
        var tokenToTry = bearerToken ?? fallbackFromSession;

        PrintCurl("GET", ReferralStatsEndpoint,
            new Dictionary<string, string> {
                { "accept", "application/json" },
                { "Authorization", "Bearer ****" }
            },
            null);

        Exception lastErr = null;

        for (int attempt = 0; attempt <= MaxRetries; attempt++)
        {
            if (attempt > 0)
            {
                var jitter = 1f + (float)((_rng.NextDouble() * 2 - 1) * JitterPct);
                float delaySec = RetryBackoffBaseSeconds * Mathf.Pow(2f, attempt) * jitter;
                Log($"[HTTP] Retry (referral-stats) #{attempt} after backoff {delaySec:0.00}s …");
                await Task.Delay(TimeSpan.FromSeconds(delaySec), ct);
            }

            using (var req = UnityWebRequest.Get(ReferralStatsEndpoint))
            {
                try
                {
                    var resolved = await ResolveBearerTokenAsync(tokenToTry, ct);
                    if (string.IsNullOrWhiteSpace(resolved))
                        throw new InvalidOperationException("No bearer token available. Please login.");

                    req.SetRequestHeader("accept", "application/json");
                    req.SetRequestHeader("Authorization", "Bearer " + resolved);
                    req.timeout = RequestTimeoutSeconds;

                    Log($"[HTTP] GET {ReferralStatsEndpoint} (referral/stats)");

                    var op = req.SendWebRequest();
                    float start = Time.realtimeSinceStartup;

                    while (!op.isDone)
                    {
                        if (ct.IsCancellationRequested) { req.Abort(); ct.ThrowIfCancellationRequested(); }
                        if (Time.realtimeSinceStartup - start > RequestTimeoutSeconds)
                        {
                            req.Abort();
                            throw new TimeoutException($"Request timed out after {RequestTimeoutSeconds}s");
                        }
                        await Task.Yield();
                    }

#if UNITY_2020_1_OR_NEWER
                    bool isNetErr = req.result == UnityWebRequest.Result.ConnectionError;
                    bool isHttpErr = req.result == UnityWebRequest.Result.ProtocolError;
#else
                    bool isNetErr = req.isNetworkError;
                    bool isHttpErr = req.isHttpError;
#endif
                    long code = req.responseCode;
                    string text = req.downloadHandler?.text ?? "";

                    Log($"[HTTP] ← {code} {(text.Length > 500 ? text.Substring(0, 500) + "..." : text)}");

                    if (!isNetErr && !isHttpErr && code >= 200 && code < 300)
                    {
                        ReferralStatsResponse resp = null;
                        try { resp = JsonUtility.FromJson<ReferralStatsResponse>(text); } catch { }

                        if (resp == null)
                            resp = new ReferralStatsResponse { status = true, message = "OK", data = new ReferralStatsData() };

                        return resp;
                    }

                    // Handle 401 with token refresh
                    if (code == 401)
                    {
                        Log("[HTTP] 401 on referral-stats – attempting token refresh...");
                        try
                        {
                            await RefreshUserTokenAsync(ct);
                            tokenToTry = null; // Use refreshed token
                            continue;
                        }
                        catch { }
                    }

                    bool retryable = code == 408 || code == 429 || (code >= 500 && code <= 599) || isNetErr;
                    if (retryable && attempt < MaxRetries) continue;

                    throw new Exception($"Referral stats failed: HTTP {code} - {text}");
                }
                catch (OperationCanceledException) { throw; }
                catch (TimeoutException tex) { lastErr = tex; if (attempt >= MaxRetries) throw; }
                catch (Exception ex) { lastErr = ex; if (attempt >= MaxRetries) throw; }
            }
        }

        throw lastErr ?? new Exception("Unknown referral stats error");
    }

    /// <summary>
    /// Claim referral rewards.
    /// Pass null or empty array to claim all completed referrals.
    /// </summary>
    public static async Task<ClaimReferralRewardsResponse> ClaimReferralRewardsAsync(
        string[] referralIds = null,
        string bearerToken = null,
        CancellationToken ct = default)
    {
        var fallbackFromSession = UserSessionManager.Current?.accessToken;
        var tokenToTry = bearerToken ?? fallbackFromSession;

        var requestBody = new ClaimReferralRewardsRequest { referralIds = referralIds ?? new string[0] };
        string jsonBody = JsonUtility.ToJson(requestBody);

        PrintCurl("POST", ClaimReferralRewardsEndpoint,
            new Dictionary<string, string> {
                { "Content-Type", "application/json" },
                { "Authorization", "Bearer ****" }
            },
            jsonBody);

        Exception lastErr = null;

        for (int attempt = 0; attempt <= MaxRetries; attempt++)
        {
            if (attempt > 0)
            {
                var jitter = 1f + (float)((_rng.NextDouble() * 2 - 1) * JitterPct);
                float delaySec = RetryBackoffBaseSeconds * Mathf.Pow(2f, attempt) * jitter;
                Log($"[HTTP] Retry (referral-claim) #{attempt} after backoff {delaySec:0.00}s …");
                await Task.Delay(TimeSpan.FromSeconds(delaySec), ct);
            }

            using (var req = new UnityWebRequest(ClaimReferralRewardsEndpoint, "POST"))
            {
                try
                {
                    var resolved = await ResolveBearerTokenAsync(tokenToTry, ct);
                    if (string.IsNullOrWhiteSpace(resolved))
                        throw new InvalidOperationException("No bearer token available. Please login.");

                    byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonBody);
                    req.uploadHandler = new UploadHandlerRaw(bodyRaw);
                    req.downloadHandler = new DownloadHandlerBuffer();
                    req.SetRequestHeader("Content-Type", "application/json");
                    req.SetRequestHeader("Authorization", "Bearer " + resolved);
                    req.timeout = RequestTimeoutSeconds;

                    Log($"[HTTP] POST {ClaimReferralRewardsEndpoint} (referral/claim)");

                    var op = req.SendWebRequest();
                    float start = Time.realtimeSinceStartup;

                    while (!op.isDone)
                    {
                        if (ct.IsCancellationRequested) { req.Abort(); ct.ThrowIfCancellationRequested(); }
                        if (Time.realtimeSinceStartup - start > RequestTimeoutSeconds)
                        {
                            req.Abort();
                            throw new TimeoutException($"Request timed out after {RequestTimeoutSeconds}s");
                        }
                        await Task.Yield();
                    }

#if UNITY_2020_1_OR_NEWER
                    bool isNetErr = req.result == UnityWebRequest.Result.ConnectionError;
                    bool isHttpErr = req.result == UnityWebRequest.Result.ProtocolError;
#else
                    bool isNetErr = req.isNetworkError;
                    bool isHttpErr = req.isHttpError;
#endif
                    long code = req.responseCode;
                    string text = req.downloadHandler?.text ?? "";

                    Log($"[HTTP] ← {code} {(text.Length > 500 ? text.Substring(0, 500) + "..." : text)}");

                    if (!isNetErr && !isHttpErr && code >= 200 && code < 300)
                    {
                        ClaimReferralRewardsResponse resp = null;
                        try { resp = JsonUtility.FromJson<ClaimReferralRewardsResponse>(text); } catch { }

                        if (resp == null)
                            resp = new ClaimReferralRewardsResponse { status = true, message = "OK", data = new ClaimReferralRewardsData() };

                        return resp;
                    }

                    // Handle 401 with token refresh
                    if (code == 401)
                    {
                        Log("[HTTP] 401 on referral-claim – attempting token refresh...");
                        try
                        {
                            await RefreshUserTokenAsync(ct);
                            tokenToTry = null;
                            continue;
                        }
                        catch { }
                    }

                    bool retryable = code == 408 || code == 429 || (code >= 500 && code <= 599) || isNetErr;
                    if (retryable && attempt < MaxRetries) continue;

                    throw new Exception($"Claim rewards failed: HTTP {code} - {text}");
                }
                catch (OperationCanceledException) { throw; }
                catch (TimeoutException tex) { lastErr = tex; if (attempt >= MaxRetries) throw; }
                catch (Exception ex) { lastErr = ex; if (attempt >= MaxRetries) throw; }
            }
        }

        throw lastErr ?? new Exception("Unknown claim rewards error");
    }

    #endregion

    // ========================================================================
    // AI AVATAR GENERATION
    // ========================================================================

    #region AI Avatar Generation

    // === Endpoint ===
    public static string EnhanceImageEndpoint = "https://ai.intelli-verse-x.ai/api/ai/ai-enhancement/images/enhance";

    // === DTOs (using existing IVXModels.EnhanceImageRequest/Response) ===

    /// <summary>
    /// Generate AI avatar from text prompt.
    /// Uses AI service to create avatar images.
    /// </summary>
    public static async Task<IVXModels.EnhanceImageResponse> GenerateAvatarFromPromptAsync(
        string prompt,
        string[] tags = null,
        string model = "gpt-image-1",
        string bearerToken = null,
        CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(prompt))
            throw new ArgumentException("Prompt is required", nameof(prompt));

        var request = new IVXModels.EnhanceImageRequest
        {
            imageUrls = new string[0],
            type = "user",
            model = model,
            prompt = prompt,
            isAiPrompt = true,
            tags = tags ?? new string[0],
            id = "866"
        };

        return await EnhanceImageInternalAsync(request, bearerToken, ct);
    }

    /// <summary>
    /// Enhance existing images with AI.
    /// </summary>
    public static async Task<IVXModels.EnhanceImageResponse> EnhanceImagesAsync(
        string[] imageUrls,
        string prompt = null,
        string[] tags = null,
        string model = "gpt-image-1",
        string bearerToken = null,
        CancellationToken ct = default)
    {
        if (imageUrls == null || imageUrls.Length == 0)
            throw new ArgumentException("At least one image URL is required", nameof(imageUrls));

        var request = new IVXModels.EnhanceImageRequest
        {
            imageUrls = imageUrls,
            type = "user",
            model = model,
            prompt = prompt ?? string.Empty,
            isAiPrompt = !string.IsNullOrEmpty(prompt),
            tags = tags ?? new string[0],
            id = "866"
        };

        return await EnhanceImageInternalAsync(request, bearerToken, ct);
    }

    /// <summary>
    /// Internal implementation for image enhancement/avatar generation.
    /// </summary>
    private static async Task<IVXModels.EnhanceImageResponse> EnhanceImageInternalAsync(
        IVXModels.EnhanceImageRequest request,
        string bearerToken,
        CancellationToken ct)
    {
        var fallbackFromSession = UserSessionManager.Current?.accessToken;
        var tokenToTry = bearerToken ?? fallbackFromSession;

        string jsonBody = JsonUtility.ToJson(request);

        PrintCurl("POST", EnhanceImageEndpoint,
            new Dictionary<string, string> {
                { "Content-Type", "application/json" },
                { "Authorization", "Bearer ****" }
            },
            jsonBody);

        Exception lastErr = null;

        for (int attempt = 0; attempt <= MaxRetries; attempt++)
        {
            if (attempt > 0)
            {
                var jitter = 1f + (float)((_rng.NextDouble() * 2 - 1) * JitterPct);
                float delaySec = RetryBackoffBaseSeconds * Mathf.Pow(2f, attempt) * jitter;
                Log($"[HTTP] Retry (enhance-image) #{attempt} after backoff {delaySec:0.00}s …");
                await Task.Delay(TimeSpan.FromSeconds(delaySec), ct);
            }

            using (var req = new UnityWebRequest(EnhanceImageEndpoint, "POST"))
            {
                try
                {
                    var resolved = await ResolveBearerTokenAsync(tokenToTry, ct);
                    if (string.IsNullOrWhiteSpace(resolved))
                        throw new InvalidOperationException("No bearer token available. Please login.");

                    byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonBody);
                    req.uploadHandler = new UploadHandlerRaw(bodyRaw);
                    req.downloadHandler = new DownloadHandlerBuffer();
                    req.SetRequestHeader("Content-Type", "application/json");
                    req.SetRequestHeader("Authorization", "Bearer " + resolved);
                    req.timeout = 60; // Longer timeout for AI generation

                    Log($"[HTTP] POST {EnhanceImageEndpoint} (enhance-image)");

                    var op = req.SendWebRequest();
                    float start = Time.realtimeSinceStartup;

                    while (!op.isDone)
                    {
                        if (ct.IsCancellationRequested) { req.Abort(); ct.ThrowIfCancellationRequested(); }
                        if (Time.realtimeSinceStartup - start > 60)
                        {
                            req.Abort();
                            throw new TimeoutException("AI generation timed out after 60s");
                        }
                        await Task.Yield();
                    }

#if UNITY_2020_1_OR_NEWER
                    bool isNetErr = req.result == UnityWebRequest.Result.ConnectionError;
                    bool isHttpErr = req.result == UnityWebRequest.Result.ProtocolError;
#else
                    bool isNetErr = req.isNetworkError;
                    bool isHttpErr = req.isHttpError;
#endif
                    long code = req.responseCode;
                    string text = req.downloadHandler?.text ?? "";

                    Log($"[HTTP] ← {code} {(text.Length > 500 ? text.Substring(0, 500) + "..." : text)}");

                    if (!isNetErr && !isHttpErr && code >= 200 && code < 300)
                    {
                        IVXModels.EnhanceImageResponse resp = null;
                        try { resp = JsonUtility.FromJson<IVXModels.EnhanceImageResponse>(text); } catch { }

                        if (resp == null)
                            resp = new IVXModels.EnhanceImageResponse { status = true, message = "OK" };

                        return resp;
                    }

                    // Handle 401 with token refresh
                    if (code == 401)
                    {
                        Log("[HTTP] 401 on enhance-image – attempting token refresh...");
                        try
                        {
                            await RefreshUserTokenAsync(ct);
                            tokenToTry = null;
                            continue;
                        }
                        catch { }
                    }

                    bool retryable = code == 408 || code == 429 || (code >= 500 && code <= 599) || isNetErr;
                    if (retryable && attempt < MaxRetries) continue;

                    throw new Exception($"Image enhancement failed: HTTP {code} - {text}");
                }
                catch (OperationCanceledException) { throw; }
                catch (TimeoutException tex) { lastErr = tex; if (attempt >= MaxRetries) throw; }
                catch (Exception ex) { lastErr = ex; if (attempt >= MaxRetries) throw; }
            }
        }

        throw lastErr ?? new Exception("Unknown image enhancement error");
    }

    #endregion

}
