using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace IntelliVerseX.Hiro
{
    // ========================================================================
    // SPIN WHEEL
    // ========================================================================

    [Serializable]
    public class IVXSpinWheelSegment
    {
        [JsonProperty("segmentId")] public string segmentId;
        [JsonProperty("label")] public string label;
        [JsonProperty("reward")] public IVXReward reward;
        [JsonProperty("weight")] public float weight;
        [JsonProperty("color")] public string color;
        [JsonProperty("isJackpot")] public bool isJackpot;
    }

    [Serializable]
    public class IVXSpinWheelConfig
    {
        [JsonProperty("wheelId")] public string wheelId;
        [JsonProperty("name")] public string name;
        [JsonProperty("segments")] public List<IVXSpinWheelSegment> segments;
        [JsonProperty("freeSpinsRemaining")] public int freeSpinsRemaining;
        [JsonProperty("maxFreeSpinsPerDay")] public int maxFreeSpinsPerDay;
        [JsonProperty("nextFreeSpinAt")] public long nextFreeSpinAt;
        [JsonProperty("adSpinsRemaining")] public int adSpinsRemaining;
        [JsonProperty("maxAdSpinsPerDay")] public int maxAdSpinsPerDay;
        [JsonProperty("spinCost")] public IVXReward spinCost;

        public IVXSpinWheelConfig()
        {
            segments = new List<IVXSpinWheelSegment>();
        }
    }

    [Serializable]
    public class IVXSpinWheelResult
    {
        [JsonProperty("wheelId")] public string wheelId;
        [JsonProperty("winningSegment")] public IVXSpinWheelSegment winningSegment;
        [JsonProperty("reward")] public IVXReward reward;
        [JsonProperty("spinType")] public string spinType;
        [JsonProperty("freeSpinsRemaining")] public int freeSpinsRemaining;
        [JsonProperty("adSpinsRemaining")] public int adSpinsRemaining;
    }

    // ========================================================================
    // SOCIAL PRESSURE
    // ========================================================================

    [Serializable]
    public class IVXSocialProof
    {
        [JsonProperty("proofId")] public string proofId;
        [JsonProperty("proofType")] public string proofType;
        [JsonProperty("message")] public string message;
        [JsonProperty("username")] public string username;
        [JsonProperty("avatarUrl")] public string avatarUrl;
        [JsonProperty("value")] public string value;
        [JsonProperty("timestamp")] public long timestamp;
    }

    [Serializable]
    public class IVXSocialPressureState
    {
        [JsonProperty("proofs")] public List<IVXSocialProof> proofs;
        [JsonProperty("friendsOnline")] public int friendsOnline;
        [JsonProperty("friendsPlayedToday")] public int friendsPlayedToday;
        [JsonProperty("globalPlayersActive")] public int globalPlayersActive;

        public IVXSocialPressureState()
        {
            proofs = new List<IVXSocialProof>();
        }
    }
}
