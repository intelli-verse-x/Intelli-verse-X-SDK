using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace IntelliVerseX.Hiro
{
    // ========================================================================
    // COMMON
    // ========================================================================

    [Serializable]
    public class IVXReward
    {
        [JsonProperty("currencies")] public Dictionary<string, float> currencies;
        [JsonProperty("items")] public Dictionary<string, int> items;

        public IVXReward()
        {
            currencies = new Dictionary<string, float>();
            items = new Dictionary<string, int>();
        }
    }

    // ========================================================================
    // ECONOMY
    // ========================================================================

    [Serializable]
    public class IVXDonation
    {
        [JsonProperty("donationId")] public string donationId;
        [JsonProperty("name")] public string name;
        [JsonProperty("description")] public string description;
        [JsonProperty("status")] public string status;
        [JsonProperty("currentCount")] public int currentCount;
        [JsonProperty("maxCount")] public int maxCount;
        [JsonProperty("expiresAt")] public long expiresAt;
        [JsonProperty("contributors")] public List<IVXDonationContributor> contributors;

        public IVXDonation() { contributors = new List<IVXDonationContributor>(); }
    }

    [Serializable]
    public class IVXDonationContributor
    {
        [JsonProperty("userId")] public string userId;
        [JsonProperty("username")] public string username;
        [JsonProperty("amount")] public int amount;
    }

    [Serializable]
    public class IVXDonationGiveResponse
    {
        [JsonProperty("donationId")] public string donationId;
        [JsonProperty("contributed")] public int contributed;
        [JsonProperty("senderReward")] public IVXReward senderReward;
    }

    [Serializable]
    public class IVXRewardedVideoResponse
    {
        [JsonProperty("rewarded")] public bool rewarded;
        [JsonProperty("reward")] public IVXReward reward;
    }

    // ========================================================================
    // INVENTORY
    // ========================================================================

    [Serializable]
    public class IVXInventoryItem
    {
        [JsonProperty("itemId")] public string itemId;
        [JsonProperty("name")] public string name;
        [JsonProperty("description")] public string description;
        [JsonProperty("category")] public string category;
        [JsonProperty("count")] public int count;
        [JsonProperty("maxCount")] public int maxCount;
        [JsonProperty("stackable")] public bool stackable;
        [JsonProperty("consumable")] public bool consumable;
        [JsonProperty("expiresAt")] public long expiresAt;
        [JsonProperty("stringProperties")] public Dictionary<string, string> stringProperties;
        [JsonProperty("numericProperties")] public Dictionary<string, double> numericProperties;

        public IVXInventoryItem()
        {
            stringProperties = new Dictionary<string, string>();
            numericProperties = new Dictionary<string, double>();
        }
    }

    [Serializable]
    public class IVXInventoryListResponse
    {
        [JsonProperty("items")] public List<IVXInventoryItem> items;
        public IVXInventoryListResponse() { items = new List<IVXInventoryItem>(); }
    }

    // ========================================================================
    // ACHIEVEMENTS
    // ========================================================================

    [Serializable]
    public class IVXAchievement
    {
        [JsonProperty("id")] public string id;
        [JsonProperty("name")] public string name;
        [JsonProperty("description")] public string description;
        [JsonProperty("category")] public string category;
        [JsonProperty("currentCount")] public int currentCount;
        [JsonProperty("maxCount")] public int maxCount;
        [JsonProperty("completed")] public bool completed;
        [JsonProperty("claimed")] public bool claimed;
        [JsonProperty("claimTimeSec")] public long claimTimeSec;
        [JsonProperty("subAchievements")] public List<IVXSubAchievement> subAchievements;

        public IVXAchievement() { subAchievements = new List<IVXSubAchievement>(); }
    }

    [Serializable]
    public class IVXSubAchievement
    {
        [JsonProperty("id")] public string id;
        [JsonProperty("name")] public string name;
        [JsonProperty("currentCount")] public int currentCount;
        [JsonProperty("maxCount")] public int maxCount;
        [JsonProperty("completed")] public bool completed;
    }

    [Serializable]
    public class IVXAchievementsListResponse
    {
        [JsonProperty("achievements")] public List<IVXAchievement> achievements;
        public IVXAchievementsListResponse() { achievements = new List<IVXAchievement>(); }
    }

    [Serializable]
    public class IVXAchievementProgressResponse
    {
        [JsonProperty("achievement")] public IVXAchievement achievement;
    }

    // ========================================================================
    // PROGRESSION
    // ========================================================================

    [Serializable]
    public class IVXProgression
    {
        [JsonProperty("level")] public int level;
        [JsonProperty("xp")] public long xp;
        [JsonProperty("xpRequired")] public long xpRequired;
        [JsonProperty("xpRemaining")] public long xpRemaining;
        [JsonProperty("maxLevel")] public int maxLevel;
        [JsonProperty("prestige")] public int prestige;
    }

    [Serializable]
    public class IVXProgressionXpResponse
    {
        [JsonProperty("progression")] public IVXProgression progression;
        [JsonProperty("levelsGained")] public int levelsGained;
        [JsonProperty("reward")] public IVXReward reward;
    }

    // ========================================================================
    // ENERGY
    // ========================================================================

    [Serializable]
    public class IVXEnergyState
    {
        [JsonProperty("energyId")] public string energyId;
        [JsonProperty("name")] public string name;
        [JsonProperty("current")] public int current;
        [JsonProperty("max")] public int max;
        [JsonProperty("regenTimeSec")] public int regenTimeSec;
        [JsonProperty("nextRegenAt")] public long nextRegenAt;
    }

    [Serializable]
    public class IVXEnergyGetResponse
    {
        [JsonProperty("energies")] public List<IVXEnergyState> energies;
        public IVXEnergyGetResponse() { energies = new List<IVXEnergyState>(); }
    }

    // ========================================================================
    // STATS
    // ========================================================================

    [Serializable]
    public class IVXStat
    {
        [JsonProperty("statId")] public string statId;
        [JsonProperty("name")] public string name;
        [JsonProperty("value")] public double value;
        [JsonProperty("isPublic")] public bool isPublic;
    }

    [Serializable]
    public class IVXStatsGetResponse
    {
        [JsonProperty("stats")] public List<IVXStat> stats;
        public IVXStatsGetResponse() { stats = new List<IVXStat>(); }
    }

    [Serializable]
    public class IVXStatUpdateResponse
    {
        [JsonProperty("statId")] public string statId;
        [JsonProperty("value")] public double value;
    }

    // ========================================================================
    // STREAKS
    // ========================================================================

    [Serializable]
    public class IVXStreak
    {
        [JsonProperty("streakId")] public string streakId;
        [JsonProperty("name")] public string name;
        [JsonProperty("currentCount")] public int currentCount;
        [JsonProperty("bestCount")] public int bestCount;
        [JsonProperty("lastUpdateSec")] public long lastUpdateSec;
        [JsonProperty("resetAt")] public long resetAt;
        [JsonProperty("claimedMilestones")] public List<int> claimedMilestones;

        public IVXStreak() { claimedMilestones = new List<int>(); }
    }

    [Serializable]
    public class IVXStreaksGetResponse
    {
        [JsonProperty("streaks")] public List<IVXStreak> streaks;
        public IVXStreaksGetResponse() { streaks = new List<IVXStreak>(); }
    }

    [Serializable]
    public class IVXStreakClaimResponse
    {
        [JsonProperty("streak")] public IVXStreak streak;
        [JsonProperty("reward")] public IVXReward reward;
    }

    // ========================================================================
    // EVENT LEADERBOARDS
    // ========================================================================

    [Serializable]
    public class IVXEventLeaderboard
    {
        [JsonProperty("id")] public string id;
        [JsonProperty("name")] public string name;
        [JsonProperty("description")] public string description;
        [JsonProperty("startAt")] public long startAt;
        [JsonProperty("endAt")] public long endAt;
        [JsonProperty("status")] public string status;
        [JsonProperty("currentTier")] public string currentTier;
        [JsonProperty("score")] public long score;
        [JsonProperty("rank")] public int rank;
        [JsonProperty("claimed")] public bool claimed;
        [JsonProperty("topRecords")] public List<IVXEventLeaderboardRecord> topRecords;

        public IVXEventLeaderboard() { topRecords = new List<IVXEventLeaderboardRecord>(); }
    }

    [Serializable]
    public class IVXEventLeaderboardRecord
    {
        [JsonProperty("userId")] public string userId;
        [JsonProperty("username")] public string username;
        [JsonProperty("score")] public long score;
        [JsonProperty("rank")] public int rank;
    }

    [Serializable]
    public class IVXEventLeaderboardListResponse
    {
        [JsonProperty("events")] public List<IVXEventLeaderboard> events;
        public IVXEventLeaderboardListResponse() { events = new List<IVXEventLeaderboard>(); }
    }

    [Serializable]
    public class IVXEventLeaderboardSubmitResponse
    {
        [JsonProperty("eventId")] public string eventId;
        [JsonProperty("score")] public long score;
        [JsonProperty("rank")] public int rank;
    }

    // ========================================================================
    // STORE
    // ========================================================================

    [Serializable]
    public class IVXStoreSection
    {
        [JsonProperty("sectionId")] public string sectionId;
        [JsonProperty("name")] public string name;
        [JsonProperty("items")] public List<IVXStoreItem> items;

        public IVXStoreSection() { items = new List<IVXStoreItem>(); }
    }

    [Serializable]
    public class IVXStoreItem
    {
        [JsonProperty("itemId")] public string itemId;
        [JsonProperty("name")] public string name;
        [JsonProperty("description")] public string description;
        [JsonProperty("category")] public string category;
        [JsonProperty("cost")] public Dictionary<string, long> cost;
        [JsonProperty("sku")] public string sku;
        [JsonProperty("reward")] public IVXReward reward;
        [JsonProperty("availableAt")] public long availableAt;
        [JsonProperty("expiresAt")] public long expiresAt;
        [JsonProperty("maxPurchases")] public int maxPurchases;
        [JsonProperty("purchaseCount")] public int purchaseCount;
        [JsonProperty("disabled")] public bool disabled;
        [JsonProperty("additionalProperties")] public Dictionary<string, string> additionalProperties;

        public IVXStoreItem()
        {
            cost = new Dictionary<string, long>();
            additionalProperties = new Dictionary<string, string>();
        }
    }

    [Serializable]
    public class IVXStoreListResponse
    {
        [JsonProperty("sections")] public List<IVXStoreSection> sections;
        public IVXStoreListResponse() { sections = new List<IVXStoreSection>(); }
    }

    [Serializable]
    public class IVXStorePurchaseResponse
    {
        [JsonProperty("reward")] public IVXReward reward;
        [JsonProperty("item")] public IVXStoreItem item;
    }

    // ========================================================================
    // CHALLENGES
    // ========================================================================

    [Serializable]
    public class IVXChallenge
    {
        [JsonProperty("instanceId")] public string instanceId;
        [JsonProperty("challengeId")] public string challengeId;
        [JsonProperty("name")] public string name;
        [JsonProperty("description")] public string description;
        [JsonProperty("status")] public string status;
        [JsonProperty("creatorId")] public string creatorId;
        [JsonProperty("maxParticipants")] public int maxParticipants;
        [JsonProperty("participants")] public List<IVXChallengeParticipant> participants;
        [JsonProperty("endsAt")] public long endsAt;

        public IVXChallenge() { participants = new List<IVXChallengeParticipant>(); }
    }

    [Serializable]
    public class IVXChallengeParticipant
    {
        [JsonProperty("userId")] public string userId;
        [JsonProperty("username")] public string username;
        [JsonProperty("score")] public long score;
        [JsonProperty("rank")] public int rank;
    }

    [Serializable]
    public class IVXChallengeSubmitResponse
    {
        [JsonProperty("instanceId")] public string instanceId;
        [JsonProperty("score")] public long score;
        [JsonProperty("rank")] public int rank;
    }

    [Serializable]
    public class IVXChallengeClaimResponse
    {
        [JsonProperty("reward")] public IVXReward reward;
    }

    // ========================================================================
    // TEAMS
    // ========================================================================

    [Serializable]
    public class IVXTeamData
    {
        [JsonProperty("groupId")] public string groupId;
        [JsonProperty("name")] public string name;
        [JsonProperty("wallet")] public Dictionary<string, long> wallet;
        [JsonProperty("stats")] public Dictionary<string, double> stats;
        [JsonProperty("achievements")] public List<IVXAchievement> achievements;

        public IVXTeamData()
        {
            wallet = new Dictionary<string, long>();
            stats = new Dictionary<string, double>();
            achievements = new List<IVXAchievement>();
        }
    }

    [Serializable]
    public class IVXTeamWallet
    {
        [JsonProperty("groupId")] public string groupId;
        [JsonProperty("wallet")] public Dictionary<string, long> wallet;

        public IVXTeamWallet() { wallet = new Dictionary<string, long>(); }
    }

    // ========================================================================
    // TUTORIALS
    // ========================================================================

    [Serializable]
    public class IVXTutorialProgress
    {
        [JsonProperty("tutorialId")] public string tutorialId;
        [JsonProperty("name")] public string name;
        [JsonProperty("currentStep")] public int currentStep;
        [JsonProperty("totalSteps")] public int totalSteps;
        [JsonProperty("completed")] public bool completed;
    }

    [Serializable]
    public class IVXTutorialsGetResponse
    {
        [JsonProperty("tutorials")] public List<IVXTutorialProgress> tutorials;
        public IVXTutorialsGetResponse() { tutorials = new List<IVXTutorialProgress>(); }
    }

    [Serializable]
    public class IVXTutorialAdvanceResponse
    {
        [JsonProperty("tutorial")] public IVXTutorialProgress tutorial;
        [JsonProperty("reward")] public IVXReward reward;
    }

    // ========================================================================
    // UNLOCKABLES
    // ========================================================================

    [Serializable]
    public class IVXUnlockableSlot
    {
        [JsonProperty("slotIndex")] public int slotIndex;
        [JsonProperty("unlockableId")] public string unlockableId;
        [JsonProperty("name")] public string name;
        [JsonProperty("startedAt")] public long startedAt;
        [JsonProperty("completesAt")] public long completesAt;
        [JsonProperty("completed")] public bool completed;
        [JsonProperty("claimed")] public bool claimed;
    }

    [Serializable]
    public class IVXUnlockablesGetResponse
    {
        [JsonProperty("slots")] public List<IVXUnlockableSlot> slots;
        [JsonProperty("maxSlots")] public int maxSlots;
        [JsonProperty("available")] public List<IVXUnlockableDefinition> available;

        public IVXUnlockablesGetResponse()
        {
            slots = new List<IVXUnlockableSlot>();
            available = new List<IVXUnlockableDefinition>();
        }
    }

    [Serializable]
    public class IVXUnlockableDefinition
    {
        [JsonProperty("id")] public string id;
        [JsonProperty("name")] public string name;
        [JsonProperty("description")] public string description;
        [JsonProperty("waitTimeSec")] public int waitTimeSec;
    }

    [Serializable]
    public class IVXUnlockableClaimResponse
    {
        [JsonProperty("slot")] public IVXUnlockableSlot slot;
        [JsonProperty("reward")] public IVXReward reward;
    }

    // ========================================================================
    // AUCTIONS
    // ========================================================================

    [Serializable]
    public class IVXAuctionListing
    {
        [JsonProperty("listingId")] public string listingId;
        [JsonProperty("sellerId")] public string sellerId;
        [JsonProperty("sellerUsername")] public string sellerUsername;
        [JsonProperty("category")] public string category;
        [JsonProperty("itemId")] public string itemId;
        [JsonProperty("itemCount")] public int itemCount;
        [JsonProperty("startingPrice")] public long startingPrice;
        [JsonProperty("currentBid")] public long currentBid;
        [JsonProperty("bidderId")] public string bidderId;
        [JsonProperty("bidderUsername")] public string bidderUsername;
        [JsonProperty("endsAt")] public long endsAt;
        [JsonProperty("status")] public string status;
    }

    [Serializable]
    public class IVXAuctionListResponse
    {
        [JsonProperty("listings")] public List<IVXAuctionListing> listings;
        public IVXAuctionListResponse() { listings = new List<IVXAuctionListing>(); }
    }

    [Serializable]
    public class IVXAuctionBidResponse
    {
        [JsonProperty("listing")] public IVXAuctionListing listing;
    }

    // ========================================================================
    // INCENTIVES
    // ========================================================================

    [Serializable]
    public class IVXReferralCodeResponse
    {
        [JsonProperty("code")] public string code;
        [JsonProperty("referralCount")] public int referralCount;
    }

    [Serializable]
    public class IVXApplyReferralResponse
    {
        [JsonProperty("applied")] public bool applied;
        [JsonProperty("reward")] public IVXReward reward;
    }

    [Serializable]
    public class IVXReturnBonusResponse
    {
        [JsonProperty("eligible")] public bool eligible;
        [JsonProperty("daysSinceLastLogin")] public int daysSinceLastLogin;
        [JsonProperty("reward")] public IVXReward reward;
        [JsonProperty("claimed")] public bool claimed;
    }

    // ========================================================================
    // MAILBOX
    // ========================================================================

    [Serializable]
    public class IVXMailboxMessage
    {
        [JsonProperty("id")] public string id;
        [JsonProperty("title")] public string title;
        [JsonProperty("body")] public string body;
        [JsonProperty("imageUrl")] public string imageUrl;
        [JsonProperty("metadata")] public Dictionary<string, string> metadata;
        [JsonProperty("hasReward")] public bool hasReward;
        [JsonProperty("claimed")] public bool claimed;
        [JsonProperty("createdAt")] public long createdAt;
        [JsonProperty("expiresAt")] public long expiresAt;

        public IVXMailboxMessage() { metadata = new Dictionary<string, string>(); }
    }

    [Serializable]
    public class IVXMailboxListResponse
    {
        [JsonProperty("messages")] public List<IVXMailboxMessage> messages;
        public IVXMailboxListResponse() { messages = new List<IVXMailboxMessage>(); }
    }

    [Serializable]
    public class IVXMailboxClaimResponse
    {
        [JsonProperty("message")] public IVXMailboxMessage message;
        [JsonProperty("reward")] public IVXReward reward;
    }

    [Serializable]
    public class IVXMailboxClaimAllResponse
    {
        [JsonProperty("claimed")] public int claimed;
        [JsonProperty("rewards")] public IVXReward rewards;
    }

    // ========================================================================
    // REWARD BUCKET
    // ========================================================================

    [Serializable]
    public class IVXRewardBucketTier
    {
        [JsonProperty("tierIndex")] public int tierIndex;
        [JsonProperty("name")] public string name;
        [JsonProperty("pointsRequired")] public int pointsRequired;
        [JsonProperty("unlocked")] public bool unlocked;
        [JsonProperty("reward")] public IVXReward reward;
    }

    [Serializable]
    public class IVXRewardBucket
    {
        [JsonProperty("bucketId")] public string bucketId;
        [JsonProperty("name")] public string name;
        [JsonProperty("description")] public string description;
        [JsonProperty("currentPoints")] public int currentPoints;
        [JsonProperty("tiers")] public List<IVXRewardBucketTier> tiers;

        public IVXRewardBucket() { tiers = new List<IVXRewardBucketTier>(); }
    }

    [Serializable]
    public class IVXRewardBucketGetResponse
    {
        [JsonProperty("buckets")] public List<IVXRewardBucket> buckets;
        public IVXRewardBucketGetResponse() { buckets = new List<IVXRewardBucket>(); }
    }

    [Serializable]
    public class IVXRewardBucketProgressResponse
    {
        [JsonProperty("bucket")] public IVXRewardBucket bucket;
        [JsonProperty("pointsAdded")] public int pointsAdded;
    }

    [Serializable]
    public class IVXRewardBucketUnlockResponse
    {
        [JsonProperty("bucket")] public IVXRewardBucket bucket;
        [JsonProperty("reward")] public IVXReward reward;
    }

    // ========================================================================
    // LEADERBOARDS
    // ========================================================================

    [Serializable]
    public class IVXLeaderboard
    {
        [JsonProperty("id")] public string id;
        [JsonProperty("name")] public string name;
        [JsonProperty("sortOrder")] public string sortOrder;
        [JsonProperty("operator")] public string operatorType;
        [JsonProperty("resetSchedule")] public string resetSchedule;
    }

    [Serializable]
    public class IVXLeaderboardsListResponse
    {
        [JsonProperty("leaderboards")] public List<IVXLeaderboard> leaderboards;
        public IVXLeaderboardsListResponse() { leaderboards = new List<IVXLeaderboard>(); }
    }

    [Serializable]
    public class IVXLeaderboardRecord
    {
        [JsonProperty("userId")] public string userId;
        [JsonProperty("username")] public string username;
        [JsonProperty("score")] public long score;
        [JsonProperty("subscore")] public long subscore;
        [JsonProperty("rank")] public int rank;
        [JsonProperty("metadata")] public Dictionary<string, object> metadata;

        public IVXLeaderboardRecord() { metadata = new Dictionary<string, object>(); }
    }

    [Serializable]
    public class IVXLeaderboardSubmitResponse
    {
        [JsonProperty("leaderboardId")] public string leaderboardId;
        [JsonProperty("score")] public long score;
        [JsonProperty("subscore")] public long subscore;
        [JsonProperty("rank")] public int rank;
    }

    [Serializable]
    public class IVXLeaderboardRecordsResponse
    {
        [JsonProperty("records")] public List<IVXLeaderboardRecord> records;
        [JsonProperty("nextCursor")] public string nextCursor;

        public IVXLeaderboardRecordsResponse() { records = new List<IVXLeaderboardRecord>(); }
    }

    // ========================================================================
    // PERSONALIZER
    // ========================================================================

    [Serializable]
    public class IVXPersonalizerOverride
    {
        [JsonProperty("system")] public string system;
        [JsonProperty("path")] public string path;
        [JsonProperty("value")] public object value;
    }

    [Serializable]
    public class IVXPersonalizerOverridesResponse
    {
        [JsonProperty("overrides")] public List<IVXPersonalizerOverride> overrides;
        public IVXPersonalizerOverridesResponse() { overrides = new List<IVXPersonalizerOverride>(); }
    }

    [Serializable]
    public class IVXPersonalizerPreviewResponse
    {
        [JsonProperty("system")] public string system;
        [JsonProperty("config")] public object config;
    }

    // ========================================================================
    // BASE / IAP
    // ========================================================================

    [Serializable]
    public class IVXIAPValidateResponse
    {
        [JsonProperty("valid")] public bool valid;
        [JsonProperty("productId")] public string productId;
        [JsonProperty("storeType")] public string storeType;
        [JsonProperty("reward")] public IVXReward reward;
        [JsonProperty("transactionId")] public string transactionId;
    }

    [Serializable]
    public class IVXIAPPurchase
    {
        [JsonProperty("productId")] public string productId;
        [JsonProperty("storeType")] public string storeType;
        [JsonProperty("price")] public float price;
        [JsonProperty("currency")] public string currency;
        [JsonProperty("purchasedAt")] public long purchasedAt;
        [JsonProperty("transactionId")] public string transactionId;
    }

    [Serializable]
    public class IVXIAPHistoryResponse
    {
        [JsonProperty("purchases")] public List<IVXIAPPurchase> purchases;
        public IVXIAPHistoryResponse() { purchases = new List<IVXIAPPurchase>(); }
    }

    // ========================================================================
    // ENERGY MODIFIER
    // ========================================================================

    [Serializable]
    public class IVXEnergyModifier
    {
        [JsonProperty("id")] public string id;
        [JsonProperty("energyId")] public string energyId;
        [JsonProperty("type")] public string type;
        [JsonProperty("value")] public double value;
        [JsonProperty("durationSec")] public int durationSec;
        [JsonProperty("expiresAt")] public long expiresAt;
    }

    [Serializable]
    public class IVXEnergyModifierResponse
    {
        [JsonProperty("modifier")] public IVXEnergyModifier modifier;
        [JsonProperty("energy")] public IVXEnergyState energy;
    }
}
