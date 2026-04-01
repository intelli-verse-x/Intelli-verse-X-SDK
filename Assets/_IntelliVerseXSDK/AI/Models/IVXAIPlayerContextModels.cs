using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace IntelliVerseX.AI
{
    /// <summary>
    /// Player context data sent to the AI backend for personalised responses.
    /// Studios populate this from their own data systems; the SDK serialises and
    /// sends it as part of session creation.
    /// </summary>
    [Serializable]
    public class IVXAIPlayerContext
    {
        #region Identity

        [JsonProperty("playerId")] public string PlayerId;
        [JsonProperty("displayName")] public string DisplayName;
        [JsonProperty("firstName")] public string FirstName;
        [JsonProperty("isGuest")] public bool IsGuest;

        #endregion

        #region Performance

        [JsonProperty("totalGamesPlayed")] public int TotalGamesPlayed;
        [JsonProperty("overallAccuracy")] public float OverallAccuracy;
        [JsonProperty("longestStreak")] public int LongestStreak;
        [JsonProperty("currentStreak")] public int CurrentStreak;
        [JsonProperty("averageAnswerTime")] public float AverageAnswerTime;
        [JsonProperty("bestScore")] public int BestScore;

        #endregion

        #region Topics

        [JsonProperty("strongTopics")] public string[] StrongTopics;
        [JsonProperty("weakTopics")] public string[] WeakTopics;
        [JsonProperty("favoriteTopics")] public string[] FavoriteTopics;

        #endregion

        #region Personality Flags

        [JsonProperty("isCompetitive")] public bool IsCompetitive;
        [JsonProperty("isCasual")] public bool IsCasual;
        [JsonProperty("isNewPlayer")] public bool IsNewPlayer;
        [JsonProperty("isVeteran")] public bool IsVeteran;
        [JsonProperty("isSpeedDemon")] public bool IsSpeedDemon;
        [JsonProperty("isAccuracyFocused")] public bool IsAccuracyFocused;

        #endregion

        #region Leaderboard

        [JsonProperty("dailyRank")] public int DailyRank;
        [JsonProperty("weeklyRank")] public int WeeklyRank;
        [JsonProperty("monthlyRank")] public int MonthlyRank;
        [JsonProperty("alltimeRank")] public int AlltimeRank;
        [JsonProperty("globalRank")] public int GlobalRank;

        #endregion

        #region Custom Data

        /// <summary>
        /// Arbitrary key-value pairs studios can attach for custom server-side prompting.
        /// </summary>
        [JsonProperty("customData")]
        public Dictionary<string, string> CustomData;

        #endregion

        #region Helpers

        /// <summary>
        /// Produces a comma-separated personality summary suitable for injection
        /// into AI host context strings (e.g. "veteran, competitive, accuracy-focused").
        /// </summary>
        public string GetPersonalitySummary()
        {
            var sb = new StringBuilder(64);
            if (IsNewPlayer) sb.Append("newcomer, ");
            if (IsVeteran) sb.Append("veteran, ");
            if (IsCompetitive) sb.Append("competitive, ");
            if (IsCasual) sb.Append("casual, ");
            if (IsSpeedDemon) sb.Append("speed-demon, ");
            if (IsAccuracyFocused) sb.Append("accuracy-focused, ");

            if (StrongTopics != null && StrongTopics.Length > 0)
                sb.Append($"expert in {string.Join("/", StrongTopics)}, ");

            if (sb.Length >= 2)
                sb.Length -= 2; // trim trailing ", "

            return sb.ToString();
        }

        /// <summary>
        /// Auto-detect personality flags from raw stats.
        /// Call after populating performance fields.
        /// </summary>
        public void CalculatePersonalityHints()
        {
            IsNewPlayer = TotalGamesPlayed < 5;
            IsVeteran = TotalGamesPlayed >= 100;
            IsCompetitive = OverallAccuracy >= 0.75f && TotalGamesPlayed >= 20;
            IsCasual = !IsCompetitive && TotalGamesPlayed >= 5;
            IsSpeedDemon = AverageAnswerTime > 0 && AverageAnswerTime < 5f;
            IsAccuracyFocused = OverallAccuracy >= 0.85f;
        }

        #endregion
    }

    /// <summary>
    /// Match context sent alongside host sessions for dynamic AI commentary.
    /// </summary>
    [Serializable]
    public class IVXAIMatchContext
    {
        [JsonProperty("matchId")] public string MatchId;
        [JsonProperty("gameMode")] public string GameMode;
        [JsonProperty("topic")] public string Topic;
        [JsonProperty("totalQuestions")] public int TotalQuestions;
        [JsonProperty("currentQuestionIndex")] public int CurrentQuestionIndex;
        [JsonProperty("difficulty")] public string Difficulty;
        [JsonProperty("players")] public IVXAIPlayerContext[] Players;
        [JsonProperty("currentLeader")] public string CurrentLeader;
        [JsonProperty("isCloseMatch")] public bool IsCloseMatch;
        [JsonProperty("hasUnderdog")] public bool HasUnderdog;
        [JsonProperty("questionsRemaining")] public int QuestionsRemaining;

        /// <summary>
        /// Generates a multi-line context string suitable for the AI Host API.
        /// </summary>
        public string GenerateContextString()
        {
            var sb = new StringBuilder(512);
            sb.AppendLine($"Match: {GameMode} | Topic: {Topic} | Difficulty: {Difficulty}");
            sb.AppendLine($"Questions: {CurrentQuestionIndex + 1}/{TotalQuestions} ({QuestionsRemaining} remaining)");

            if (!string.IsNullOrEmpty(CurrentLeader))
                sb.AppendLine($"Leader: {CurrentLeader}");
            if (IsCloseMatch)
                sb.AppendLine("Match is close!");
            if (HasUnderdog)
                sb.AppendLine("Underdog comeback possible.");

            if (Players != null)
            {
                sb.AppendLine("Players:");
                foreach (var p in Players)
                {
                    sb.AppendLine($"  - {p.DisplayName ?? p.PlayerId}: {p.GetPersonalitySummary()}");
                }
            }

            return sb.ToString();
        }
    }
}
