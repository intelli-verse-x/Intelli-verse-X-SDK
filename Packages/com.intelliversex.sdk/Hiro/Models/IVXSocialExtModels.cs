using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace IntelliVerseX.Hiro
{
    // ========================================================================
    // FRIEND QUEST
    // ========================================================================

    [Serializable]
    public class IVXFriendQuest
    {
        [JsonProperty("id")] public string questId;
        [JsonProperty("title")] public string title;
        [JsonProperty("description")] public string description;
        [JsonProperty("type")] public string type;
        [JsonProperty("target")] public int targetValue;
        [JsonProperty("progress")] public int currentValue;
        [JsonProperty("partnerId")] public string partnerId;
        [JsonProperty("partnerName")] public string partnerName;
        [JsonProperty("partnerProgress")] public int partnerProgress;
        [JsonProperty("reward")] public IVXReward reward;
        [JsonProperty("status")] public string status;
        [JsonProperty("expiresAt")] public string expiresAt;
        [JsonProperty("completedAt")] public string completedAt;
    }

    [Serializable]
    public class IVXFriendQuestState
    {
        [JsonProperty("quests")] public List<IVXFriendQuest> quests;
        [JsonProperty("generatedAt")] public string generatedAt;

        public IVXFriendQuestState()
        {
            quests = new List<IVXFriendQuest>();
        }
    }

    [Serializable]
    public class IVXFriendQuestProgressResponse
    {
        [JsonProperty("quest")] public IVXFriendQuest quest;
        [JsonProperty("updated")] public bool updated;
        [JsonProperty("completed")] public bool completed;
        [JsonProperty("reward")] public IVXReward reward;
        [JsonProperty("success")] public bool success;
    }

    // ========================================================================
    // FRIEND STREAK
    // ========================================================================

    [Serializable]
    public class IVXFriendStreak
    {
        [JsonProperty("friendId")] public string friendId;
        [JsonProperty("friendDisplayName")] public string friendName;
        [JsonProperty("streakDays")] public int currentStreak;
        [JsonProperty("longestStreak")] public int longestStreak;
        [JsonProperty("lastInteractionAt")] public string lastInteractionAt;
        [JsonProperty("myContributionToday")] public bool myContributionToday;
        [JsonProperty("friendContributionToday")] public bool friendContributionToday;
        [JsonProperty("hoursUntilBreak")] public float hoursUntilBreak;
        [JsonProperty("isAtRisk")] public bool isAtRisk;
        [JsonProperty("milestoneRewards")] public List<IVXFriendStreakMilestone> milestoneRewards;

        public IVXFriendStreak()
        {
            milestoneRewards = new List<IVXFriendStreakMilestone>();
        }
    }

    [Serializable]
    public class IVXFriendStreakMilestone
    {
        [JsonProperty("day")] public int day;
        [JsonProperty("reward")] public IVXReward reward;
        [JsonProperty("claimed")] public bool claimed;
    }

    [Serializable]
    public class IVXFriendStreakState
    {
        [JsonProperty("streaks")] public List<IVXFriendStreak> streaks;
        [JsonProperty("totalActive")] public int totalActive;
        [JsonProperty("maxStreaks")] public int maxActiveStreaks;
        [JsonProperty("nudgesRemaining")] public int nudgesRemaining;

        public IVXFriendStreakState()
        {
            streaks = new List<IVXFriendStreak>();
        }
    }

    [Serializable]
    public class IVXFriendStreakInteractResponse
    {
        [JsonProperty("friendId")] public string friendId;
        [JsonProperty("streakDays")] public int streakDays;
        [JsonProperty("advanced")] public bool advanced;
        [JsonProperty("myContributionToday")] public bool myContributionToday;
        [JsonProperty("friendContributionToday")] public bool friendContributionToday;
        [JsonProperty("success")] public bool success;
    }

    // ========================================================================
    // FRIEND BATTLE / CHALLENGE
    // ========================================================================

    [Serializable]
    public class IVXFriendBattleChallenge
    {
        [JsonProperty("challengeId")] public string challengeId;
        [JsonProperty("fromUserId")] public string challengerId;
        [JsonProperty("toUserId")] public string opponentId;
        [JsonProperty("challengerName")] public string challengerName;
        [JsonProperty("opponentName")] public string opponentName;
        [JsonProperty("gameId")] public string gameId;
        [JsonProperty("gameMode")] public string gameMode;
        [JsonProperty("status")] public string status;
        [JsonProperty("roomCode")] public string roomCode;
        [JsonProperty("shareCode")] public string shareCode;
        [JsonProperty("isAsync")] public bool isAsync;
        [JsonProperty("expiresAt")] public string expiresAt;
        [JsonProperty("winnerId")] public string winnerId;
    }

    [Serializable]
    public class IVXFriendBattleState
    {
        /// <summary>Prod list RPC may return challenges under several keys; normalize in callers if needed.</summary>
        [JsonProperty("challenges")] public List<IVXFriendBattleChallenge> challenges;
        [JsonProperty("pendingChallenges")] public List<IVXFriendBattleChallenge> pendingChallenges;
        [JsonProperty("activeBattles")] public List<IVXFriendBattleChallenge> activeBattles;
        [JsonProperty("recentResults")] public List<IVXFriendBattleChallenge> recentResults;

        public IVXFriendBattleState()
        {
            challenges = new List<IVXFriendBattleChallenge>();
            pendingChallenges = new List<IVXFriendBattleChallenge>();
            activeBattles = new List<IVXFriendBattleChallenge>();
            recentResults = new List<IVXFriendBattleChallenge>();
        }
    }

    [Serializable]
    public class IVXFriendBattleSendResponse
    {
        [JsonProperty("challengeId")] public string challengeId;
        [JsonProperty("fromUserId")] public string fromUserId;
        [JsonProperty("toUserId")] public string toUserId;
        [JsonProperty("gameId")] public string gameId;
        [JsonProperty("status")] public string status;
        [JsonProperty("success")] public bool success;
        [JsonProperty("challenge")] public IVXFriendBattleChallenge challenge;
        [JsonProperty("sent")] public bool sent;
    }
}
