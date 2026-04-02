using System;
using Newtonsoft.Json;

namespace IntelliVerseX.AI
{
    #region Voice Session Models

    /// <summary>
    /// Request body for creating a voice session.
    /// </summary>
    [Serializable]
    public class IVXAICreateVoiceSessionRequest
    {
        [JsonProperty("persona")] public string Persona;
        [JsonProperty("userId")] public string UserId;
        [JsonProperty("userName")] public string UserName;
        [JsonProperty("topic")] public string Topic;
        [JsonProperty("language")] public string Language;
        /// <summary>Serialized <see cref="IVXAIPlayerContext"/> JSON for voice personalization when set.</summary>
        [JsonProperty("playerContext")] public string PlayerContext;
    }

    /// <summary>
    /// Response from voice session creation.
    /// </summary>
    [Serializable]
    public class IVXAICreateVoiceSessionResponse : IVXAIBaseResponse
    {
        [JsonProperty("sessionId")] public string SessionId;
        [JsonProperty("persona")] public string Persona;
        [JsonProperty("isPremium")] public bool IsPremium;
        [JsonProperty("durationSeconds")] public int DurationSeconds;
        [JsonProperty("config")] public IVXAISessionConfig Config;
        [JsonProperty("socialProof")] public IVXAISocialProofData SocialProof;
    }

    /// <summary>
    /// Session configuration returned by the backend.
    /// </summary>
    [Serializable]
    public class IVXAISessionConfig
    {
        [JsonProperty("language")] public string Language;
        [JsonProperty("durationSeconds")] public int DurationSeconds;
        [JsonProperty("isPremium")] public bool IsPremium;
        [JsonProperty("freeTrialAvailable")] public bool FreeTrialAvailable;
        [JsonProperty("remainingFreeSessions")] public int RemainingFreeSessions;
    }

    /// <summary>
    /// Session status response.
    /// </summary>
    [Serializable]
    public class IVXAISessionStatusResponse : IVXAIBaseResponse
    {
        [JsonProperty("sessionId")] public string SessionId;
        [JsonProperty("isConnected")] public bool IsConnected;
        [JsonProperty("persona")] public string Persona;
        [JsonProperty("userName")] public string UserName;
        [JsonProperty("isPremium")] public bool IsPremium;
        [JsonProperty("status")] public string Status;
        [JsonProperty("config")] public IVXAISessionConfig Config;
    }

    /// <summary>
    /// End-session response with analytics.
    /// </summary>
    [Serializable]
    public class IVXAIEndSessionResponse : IVXAIBaseResponse
    {
        [JsonProperty("message")] public string Message;
        [JsonProperty("analytics")] public IVXAISessionAnalytics Analytics;
    }

    /// <summary>
    /// Session analytics returned on session end.
    /// </summary>
    [Serializable]
    public class IVXAISessionAnalytics
    {
        [JsonProperty("sessionId")] public string SessionId;
        [JsonProperty("userId")] public string UserId;
        [JsonProperty("persona")] public string Persona;
        [JsonProperty("durationSeconds")] public int DurationSeconds;
        [JsonProperty("creditsUsed")] public int CreditsUsed;
        [JsonProperty("estimatedCost")] public float EstimatedCost;
        [JsonProperty("isPremium")] public bool IsPremium;
        [JsonProperty("wasFreeTrial")] public bool WasFreeTrial;
        [JsonProperty("completedSuccessfully")] public bool CompletedSuccessfully;
    }

    #endregion

    #region Host Session Models

    /// <summary>
    /// Request body for creating an AI Host session.
    /// </summary>
    [Serializable]
    public class IVXAICreateHostSessionRequest
    {
        [JsonProperty("gameMode")] public string GameMode;
        [JsonProperty("playerCount")] public int PlayerCount;
        [JsonProperty("playerNames")] public string[] PlayerNames;
        [JsonProperty("playerProfiles")] public IVXAIHostPlayerProfile[] PlayerProfiles;
        [JsonProperty("matchContext")] public string MatchContext;
        [JsonProperty("topic")] public string Topic;
        [JsonProperty("textOnlyMode")] public bool TextOnlyMode;
        [JsonProperty("language")] public string Language;
        [JsonProperty("difficulty")] public string Difficulty;
        [JsonProperty("totalQuestions")] public int TotalQuestions;
    }

    /// <summary>
    /// Flat player profile DTO sent to the AI Host backend.
    /// Studios populate this from their own player data systems.
    /// </summary>
    [Serializable]
    public class IVXAIHostPlayerProfile
    {
        [JsonProperty("name")] public string Name;
        [JsonProperty("displayName")] public string DisplayName;
        [JsonProperty("firstName")] public string FirstName;
        [JsonProperty("isGuest")] public bool IsGuest;
        [JsonProperty("isNewPlayer")] public bool IsNewPlayer;
        [JsonProperty("isVeteran")] public bool IsVeteran;
        [JsonProperty("totalGamesPlayed")] public int TotalGamesPlayed;
        [JsonProperty("overallAccuracy")] public float OverallAccuracy;
        [JsonProperty("longestStreak")] public int LongestStreak;
        [JsonProperty("currentStreak")] public int CurrentStreak;
        [JsonProperty("averageAnswerTime")] public float AverageAnswerTime;
        [JsonProperty("strongTopics")] public string[] StrongTopics;
        [JsonProperty("weakTopics")] public string[] WeakTopics;
        [JsonProperty("bestScore")] public int BestScore;
    }

    /// <summary>
    /// Host session configuration returned by the backend.
    /// </summary>
    [Serializable]
    public class IVXAIHostSessionConfig
    {
        [JsonProperty("language")] public string Language;
        [JsonProperty("difficulty")] public string Difficulty;
        [JsonProperty("questionTime")] public int QuestionTime;
        [JsonProperty("hintAvailableAfter")] public int HintAvailableAfter;
        [JsonProperty("warningAt")] public int WarningAt;
        [JsonProperty("speedBonusCutoff")] public int? SpeedBonusCutoff;
    }

    /// <summary>
    /// Response from host session creation.
    /// </summary>
    [Serializable]
    public class IVXAICreateHostSessionResponse : IVXAIBaseResponse
    {
        [JsonProperty("sessionId")] public string SessionId;
        [JsonProperty("gameMode")] public string GameMode;
        [JsonProperty("textOnlyMode")] public bool TextOnlyMode;
        [JsonProperty("playersRegistered")] public int PlayersRegistered;
        [JsonProperty("config")] public IVXAIHostSessionConfig Config;
    }

    /// <summary>
    /// Game event request sent to the host mid-match.
    /// </summary>
    [Serializable]
    public class IVXAIHostGameEventRequest
    {
        [JsonProperty("eventType")] public string EventType;
        [JsonProperty("state")] public string State;
        [JsonProperty("data")] public string Data;
    }

    /// <summary>
    /// Player answer request for the host.
    /// </summary>
    [Serializable]
    public class IVXAIHostPlayerAnswerRequest
    {
        [JsonProperty("playerId")] public string PlayerId;
        [JsonProperty("answerIndex")] public int AnswerIndex;
    }

    #endregion

    #region Shared

    /// <summary>
    /// Base response wrapper for all AI API responses.
    /// </summary>
    [Serializable]
    public class IVXAIBaseResponse
    {
        [JsonProperty("success")] public bool Success;
        [JsonProperty("error")] public string Error;
    }

    /// <summary>
    /// Simple text response.
    /// </summary>
    [Serializable]
    public class IVXAISimpleResponse : IVXAIBaseResponse
    {
        [JsonProperty("message")] public string Message;
    }

    /// <summary>
    /// Social proof data for conversion optimisation.
    /// </summary>
    [Serializable]
    public class IVXAISocialProofData
    {
        [JsonProperty("readingsToday")] public int ReadingsToday;
        [JsonProperty("activeUsers")] public int ActiveUsers;
        [JsonProperty("averageRating")] public float AverageRating;
        [JsonProperty("totalSessionsAllTime")] public int TotalSessionsAllTime;
        [JsonProperty("happyUsers")] public int HappyUsers;
    }

    /// <summary>
    /// Text-only send request (voice or host).
    /// </summary>
    [Serializable]
    public class IVXAISendTextRequest
    {
        [JsonProperty("text")] public string Text;
        [JsonProperty("playerId")] public string PlayerId;
    }

    /// <summary>
    /// Trigger speech request.
    /// </summary>
    [Serializable]
    public class IVXAITriggerSpeechRequest
    {
        [JsonProperty("prompt")] public string Prompt;
    }

    /// <summary>
    /// Audio send request (base64 PCM16).
    /// </summary>
    [Serializable]
    public class IVXAISendAudioRequest
    {
        [JsonProperty("audio")] public string Audio;
    }

    /// <summary>
    /// Response from the personas list endpoint.
    /// </summary>
    [Serializable]
    public class IVXAIPersonasResponse : IVXAIBaseResponse
    {
        [JsonProperty("personas")] public IVXAIPersona[] Personas;
    }

    #endregion
}
