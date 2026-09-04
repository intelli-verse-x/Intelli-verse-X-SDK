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
        [JsonProperty("questId")] public string questId;
        [JsonProperty("title")] public string title;
        [JsonProperty("description")] public string description;
        [JsonProperty("targetValue")] public int targetValue;
        [JsonProperty("currentValue")] public int currentValue;
        [JsonProperty("partnerId")] public string partnerId;
        [JsonProperty("partnerName")] public string partnerName;
        [JsonProperty("partnerProgress")] public int partnerProgress;
        [JsonProperty("reward")] public IVXReward reward;
        [JsonProperty("status")] public string status;
        [JsonProperty("expiresAt")] public long expiresAt;
        [JsonProperty("completedAt")] public long completedAt;
    }

    [Serializable]
    public class IVXFriendQuestState
    {
        [JsonProperty("activeQuests")] public List<IVXFriendQuest> activeQuests;
        [JsonProperty("availableQuests")] public List<IVXFriendQuest> availableQuests;
        [JsonProperty("completedToday")] public int completedToday;

        public IVXFriendQuestState()
        {
            activeQuests = new List<IVXFriendQuest>();
            availableQuests = new List<IVXFriendQuest>();
        }
    }

    [Serializable]
    public class IVXFriendQuestAcceptResponse
    {
        [JsonProperty("quest")] public IVXFriendQuest quest;
        [JsonProperty("accepted")] public bool accepted;
    }

    [Serializable]
    public class IVXFriendQuestProgressResponse
    {
        [JsonProperty("quest")] public IVXFriendQuest quest;
        [JsonProperty("updated")] public bool updated;
        [JsonProperty("completed")] public bool completed;
        [JsonProperty("reward")] public IVXReward reward;
    }

    // ========================================================================
    // FRIEND STREAK
    // ========================================================================

    [Serializable]
    public class IVXFriendStreak
    {
        [JsonProperty("streakId")] public string streakId;
        [JsonProperty("friendId")] public string friendId;
        [JsonProperty("friendName")] public string friendName;
        [JsonProperty("currentStreak")] public int currentStreak;
        [JsonProperty("longestStreak")] public int longestStreak;
        [JsonProperty("lastInteractionAt")] public long lastInteractionAt;
        [JsonProperty("myContributionToday")] public bool myContributionToday;
        [JsonProperty("friendContributionToday")] public bool friendContributionToday;
        [JsonProperty("expiresAt")] public long expiresAt;
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
        [JsonProperty("maxActiveStreaks")] public int maxActiveStreaks;

        public IVXFriendStreakState()
        {
            streaks = new List<IVXFriendStreak>();
        }
    }

    [Serializable]
    public class IVXFriendStreakInteractResponse
    {
        [JsonProperty("streak")] public IVXFriendStreak streak;
        [JsonProperty("recorded")] public bool recorded;
        [JsonProperty("milestoneReward")] public IVXReward milestoneReward;
    }

    // ========================================================================
    // FRIEND BATTLE
    // ========================================================================

    [Serializable]
    public class IVXFriendBattleChallenge
    {
        [JsonProperty("challengeId")] public string challengeId;
        [JsonProperty("challengerId")] public string challengerId;
        [JsonProperty("challengerName")] public string challengerName;
        [JsonProperty("challengerScore")] public int challengerScore;
        [JsonProperty("opponentId")] public string opponentId;
        [JsonProperty("opponentName")] public string opponentName;
        [JsonProperty("opponentScore")] public int opponentScore;
        [JsonProperty("gameMode")] public string gameMode;
        [JsonProperty("status")] public string status;
        [JsonProperty("wager")] public IVXReward wager;
        [JsonProperty("winnerReward")] public IVXReward winnerReward;
        [JsonProperty("expiresAt")] public long expiresAt;
        [JsonProperty("winnerId")] public string winnerId;
    }

    [Serializable]
    public class IVXFriendBattleState
    {
        [JsonProperty("pendingChallenges")] public List<IVXFriendBattleChallenge> pendingChallenges;
        [JsonProperty("activeBattles")] public List<IVXFriendBattleChallenge> activeBattles;
        [JsonProperty("recentResults")] public List<IVXFriendBattleChallenge> recentResults;

        public IVXFriendBattleState()
        {
            pendingChallenges = new List<IVXFriendBattleChallenge>();
            activeBattles = new List<IVXFriendBattleChallenge>();
            recentResults = new List<IVXFriendBattleChallenge>();
        }
    }

    [Serializable]
    public class IVXFriendBattleSendResponse
    {
        [JsonProperty("challenge")] public IVXFriendBattleChallenge challenge;
        [JsonProperty("sent")] public bool sent;
    }

    [Serializable]
    public class IVXFriendBattleSubmitResponse
    {
        [JsonProperty("challenge")] public IVXFriendBattleChallenge challenge;
        [JsonProperty("submitted")] public bool submitted;
        [JsonProperty("reward")] public IVXReward reward;
    }
}
