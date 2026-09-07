using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace IntelliVerseX.Hiro
{
    // ========================================================================
    // IAP TRIGGER
    // ========================================================================

    [Serializable]
    public class IVXIAPTrigger
    {
        [JsonProperty("triggerId")] public string triggerId;
        [JsonProperty("offerSku")] public string offerSku;
        [JsonProperty("triggerType")] public string triggerType;
        [JsonProperty("displayTitle")] public string displayTitle;
        [JsonProperty("displayMessage")] public string displayMessage;
        [JsonProperty("expiresAt")] public long expiresAt;
        [JsonProperty("priority")] public int priority;
        [JsonProperty("metadata")] public Dictionary<string, string> metadata;

        public IVXIAPTrigger()
        {
            metadata = new Dictionary<string, string>();
        }
    }

    [Serializable]
    public class IVXIAPTriggerEvalResponse
    {
        [JsonProperty("triggers")] public List<IVXIAPTrigger> triggers;
        [JsonProperty("suppressedUntil")] public long suppressedUntil;

        public IVXIAPTriggerEvalResponse()
        {
            triggers = new List<IVXIAPTrigger>();
        }
    }

    [Serializable]
    public class IVXIAPTriggerDismissResponse
    {
        [JsonProperty("dismissed")] public bool dismissed;
        [JsonProperty("cooldownUntil")] public long cooldownUntil;
    }

    // ========================================================================
    // SMART AD TIMER
    // ========================================================================

    [Serializable]
    public class IVXSmartAdTimerState
    {
        [JsonProperty("interstitialCooldownSec")] public int interstitialCooldownSec;
        [JsonProperty("nextInterstitialAt")] public long nextInterstitialAt;
        [JsonProperty("rewardedAdsToday")] public int rewardedAdsToday;
        [JsonProperty("maxRewardedAdsPerDay")] public int maxRewardedAdsPerDay;
        [JsonProperty("bannerEnabled")] public bool bannerEnabled;
        [JsonProperty("sessionAdCount")] public int sessionAdCount;
    }

    [Serializable]
    public class IVXSmartAdTimerRecordResponse
    {
        [JsonProperty("state")] public IVXSmartAdTimerState state;
        [JsonProperty("recorded")] public bool recorded;
        [JsonProperty("reward")] public IVXReward reward;
    }

    // ========================================================================
    // AD REVENUE OPTIMIZER
    // ========================================================================

    [Serializable]
    public class IVXAdRevenueConfig
    {
        [JsonProperty("placements")] public List<IVXAdPlacement> placements;
        [JsonProperty("globalFrequencyCapSec")] public int globalFrequencyCapSec;
        [JsonProperty("sessionCap")] public int sessionCap;

        public IVXAdRevenueConfig()
        {
            placements = new List<IVXAdPlacement>();
        }
    }

    [Serializable]
    public class IVXAdPlacement
    {
        [JsonProperty("placementId")] public string placementId;
        [JsonProperty("adType")] public string adType;
        [JsonProperty("priority")] public int priority;
        [JsonProperty("cooldownSec")] public int cooldownSec;
        [JsonProperty("rewardMultiplier")] public float rewardMultiplier;
        [JsonProperty("enabled")] public bool enabled;
    }

    [Serializable]
    public class IVXAdImpressionResponse
    {
        [JsonProperty("recorded")] public bool recorded;
        [JsonProperty("reward")] public IVXReward reward;
        [JsonProperty("nextAllowedAt")] public long nextAllowedAt;
    }

    // ========================================================================
    // OFFERWALL
    // ========================================================================

    [Serializable]
    public class IVXOfferwallOffer
    {
        [JsonProperty("offerId")] public string offerId;
        [JsonProperty("provider")] public string provider;
        [JsonProperty("title")] public string title;
        [JsonProperty("description")] public string description;
        [JsonProperty("rewardAmount")] public float rewardAmount;
        [JsonProperty("rewardCurrency")] public string rewardCurrency;
        [JsonProperty("completed")] public bool completed;
        [JsonProperty("expiresAt")] public long expiresAt;
    }

    [Serializable]
    public class IVXOfferwallState
    {
        [JsonProperty("offers")] public List<IVXOfferwallOffer> offers;
        [JsonProperty("totalEarned")] public float totalEarned;
        [JsonProperty("pendingRewards")] public float pendingRewards;

        public IVXOfferwallState()
        {
            offers = new List<IVXOfferwallOffer>();
        }
    }

    [Serializable]
    public class IVXOfferwallCompleteResponse
    {
        [JsonProperty("state")] public IVXOfferwallState state;
        [JsonProperty("reward")] public IVXReward reward;
        [JsonProperty("credited")] public bool credited;
    }
}
