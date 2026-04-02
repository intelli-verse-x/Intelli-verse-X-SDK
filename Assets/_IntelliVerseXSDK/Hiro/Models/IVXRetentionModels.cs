using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace IntelliVerseX.Hiro
{
    // ========================================================================
    // RETENTION
    // ========================================================================

    [Serializable]
    public class IVXRetentionState
    {
        [JsonProperty("userId")] public string userId;
        [JsonProperty("firstSessionAt")] public long firstSessionAt;
        [JsonProperty("lastSessionAt")] public long lastSessionAt;
        [JsonProperty("totalSessions")] public int totalSessions;
        [JsonProperty("currentSessionDepth")] public int currentSessionDepth;
        [JsonProperty("daysSinceLastSession")] public int daysSinceLastSession;
        [JsonProperty("churnRisk")] public string churnRisk;
        [JsonProperty("onboardingComplete")] public bool onboardingComplete;
        [JsonProperty("onboardingStep")] public int onboardingStep;
        [JsonProperty("comebackBonusAvailable")] public bool comebackBonusAvailable;
        [JsonProperty("comebackBonusReward")] public IVXReward comebackBonusReward;
    }

    [Serializable]
    public class IVXRetentionHeartbeatResponse
    {
        [JsonProperty("state")] public IVXRetentionState state;
        [JsonProperty("reward")] public IVXReward reward;
        [JsonProperty("comebackClaimed")] public bool comebackClaimed;
    }

    // ========================================================================
    // STREAK SHIELD
    // ========================================================================

    [Serializable]
    public class IVXStreakShieldState
    {
        [JsonProperty("shieldsRemaining")] public int shieldsRemaining;
        [JsonProperty("maxShields")] public int maxShields;
        [JsonProperty("shieldActive")] public bool shieldActive;
        [JsonProperty("shieldExpiresAt")] public long shieldExpiresAt;
        [JsonProperty("lastReplenishedAt")] public long lastReplenishedAt;
        [JsonProperty("streakProtected")] public bool streakProtected;
    }

    [Serializable]
    public class IVXStreakShieldActivateResponse
    {
        [JsonProperty("state")] public IVXStreakShieldState state;
        [JsonProperty("activated")] public bool activated;
    }

    [Serializable]
    public class IVXStreakShieldReplenishResponse
    {
        [JsonProperty("state")] public IVXStreakShieldState state;
        [JsonProperty("replenished")] public bool replenished;
        [JsonProperty("source")] public string source;
    }

    // ========================================================================
    // SESSION BOOSTER
    // ========================================================================

    [Serializable]
    public class IVXSessionBooster
    {
        [JsonProperty("boosterId")] public string boosterId;
        [JsonProperty("name")] public string name;
        [JsonProperty("multiplier")] public float multiplier;
        [JsonProperty("durationSec")] public int durationSec;
        [JsonProperty("expiresAt")] public long expiresAt;
        [JsonProperty("active")] public bool active;
    }

    [Serializable]
    public class IVXSessionBoosterState
    {
        [JsonProperty("activeBoosters")] public List<IVXSessionBooster> activeBoosters;
        [JsonProperty("availableBoosters")] public List<IVXSessionBooster> availableBoosters;
        [JsonProperty("nextFreeBoosterAt")] public long nextFreeBoosterAt;

        public IVXSessionBoosterState()
        {
            activeBoosters = new List<IVXSessionBooster>();
            availableBoosters = new List<IVXSessionBooster>();
        }
    }

    [Serializable]
    public class IVXSessionBoosterActivateResponse
    {
        [JsonProperty("state")] public IVXSessionBoosterState state;
        [JsonProperty("activated")] public IVXSessionBooster activated;
    }

    // ========================================================================
    // APPOINTMENT MECHANIC
    // ========================================================================

    [Serializable]
    public class IVXAppointment
    {
        [JsonProperty("appointmentId")] public string appointmentId;
        [JsonProperty("name")] public string name;
        [JsonProperty("description")] public string description;
        [JsonProperty("scheduledAt")] public long scheduledAt;
        [JsonProperty("windowDurationSec")] public int windowDurationSec;
        [JsonProperty("reward")] public IVXReward reward;
        [JsonProperty("claimed")] public bool claimed;
        [JsonProperty("expired")] public bool expired;
        [JsonProperty("recurring")] public bool recurring;
        [JsonProperty("recurrenceRule")] public string recurrenceRule;
    }

    [Serializable]
    public class IVXAppointmentState
    {
        [JsonProperty("appointments")] public List<IVXAppointment> appointments;
        [JsonProperty("nextAppointmentAt")] public long nextAppointmentAt;

        public IVXAppointmentState()
        {
            appointments = new List<IVXAppointment>();
        }
    }

    [Serializable]
    public class IVXAppointmentClaimResponse
    {
        [JsonProperty("state")] public IVXAppointmentState state;
        [JsonProperty("reward")] public IVXReward reward;
        [JsonProperty("claimed")] public bool claimed;
    }

    // ========================================================================
    // LIMITED DAILY CONTENT
    // ========================================================================

    [Serializable]
    public class IVXDailyContentSlot
    {
        [JsonProperty("slotId")] public string slotId;
        [JsonProperty("contentType")] public string contentType;
        [JsonProperty("title")] public string title;
        [JsonProperty("description")] public string description;
        [JsonProperty("imageUrl")] public string imageUrl;
        [JsonProperty("reward")] public IVXReward reward;
        [JsonProperty("availableAt")] public long availableAt;
        [JsonProperty("expiresAt")] public long expiresAt;
        [JsonProperty("claimed")] public bool claimed;
        [JsonProperty("claimedAt")] public long claimedAt;
        [JsonProperty("requiresAction")] public bool requiresAction;
        [JsonProperty("actionPayload")] public string actionPayload;
    }

    [Serializable]
    public class IVXDailyContentState
    {
        [JsonProperty("slots")] public List<IVXDailyContentSlot> slots;
        [JsonProperty("nextResetAt")] public long nextResetAt;
        [JsonProperty("totalClaimedToday")] public int totalClaimedToday;
        [JsonProperty("totalAvailableToday")] public int totalAvailableToday;

        public IVXDailyContentState()
        {
            slots = new List<IVXDailyContentSlot>();
        }
    }

    [Serializable]
    public class IVXDailyContentClaimResponse
    {
        [JsonProperty("state")] public IVXDailyContentState state;
        [JsonProperty("reward")] public IVXReward reward;
        [JsonProperty("claimed")] public bool claimed;
    }
}
