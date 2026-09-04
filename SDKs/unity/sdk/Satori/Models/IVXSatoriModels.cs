using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace IntelliVerseX.Satori
{
    // ========================================================================
    // RPC RESPONSE WRAPPER
    // ========================================================================

    [Serializable]
    public class SatoriRpcResponse<T>
    {
        [JsonProperty("success")] public bool success;
        [JsonProperty("data")] public T data;
        [JsonProperty("error")] public string error;
    }

    // ========================================================================
    // EVENTS
    // ========================================================================

    [Serializable]
    public class IVXSatoriEvent
    {
        [JsonProperty("name")] public string name;
        [JsonProperty("timestamp")] public long timestamp;
        [JsonProperty("metadata")] public Dictionary<string, string> metadata;

        public IVXSatoriEvent() { metadata = new Dictionary<string, string>(); }

        public IVXSatoriEvent(string name, Dictionary<string, string> metadata = null)
        {
            this.name = name;
            this.timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            this.metadata = metadata ?? new Dictionary<string, string>();
        }
    }

    // ========================================================================
    // IDENTITY
    // ========================================================================

    [Serializable]
    public class IVXSatoriIdentity
    {
        [JsonProperty("defaultProperties")] public Dictionary<string, string> defaultProperties;
        [JsonProperty("customProperties")] public Dictionary<string, string> customProperties;
        [JsonProperty("computedProperties")] public Dictionary<string, string> computedProperties;

        public IVXSatoriIdentity()
        {
            defaultProperties = new Dictionary<string, string>();
            customProperties = new Dictionary<string, string>();
            computedProperties = new Dictionary<string, string>();
        }
    }

    // ========================================================================
    // FEATURE FLAGS
    // ========================================================================

    [Serializable]
    public class IVXSatoriFlag
    {
        [JsonProperty("name")] public string name;
        [JsonProperty("value")] public string value;
        [JsonProperty("enabled")] public bool enabled;

        public IVXSatoriFlag() { }

        public IVXSatoriFlag(string name, string value)
        {
            this.name = name;
            this.value = value;
            this.enabled = true;
        }
    }

    // ========================================================================
    // EXPERIMENTS
    // ========================================================================

    [Serializable]
    public class IVXSatoriExperimentVariant
    {
        [JsonProperty("id")] public string id;
        [JsonProperty("name")] public string name;
        [JsonProperty("config")] public Dictionary<string, string> config;

        public IVXSatoriExperimentVariant() { config = new Dictionary<string, string>(); }
    }

    [Serializable]
    public class IVXSatoriExperiment
    {
        [JsonProperty("id")] public string id;
        [JsonProperty("name")] public string name;
        [JsonProperty("description")] public string description;
        [JsonProperty("status")] public string status;
        [JsonProperty("variant")] public IVXSatoriExperimentVariant variant;
    }

    [Serializable]
    public class IVXSatoriExperimentsResponse
    {
        [JsonProperty("experiments")] public List<IVXSatoriExperiment> experiments;
        public IVXSatoriExperimentsResponse() { experiments = new List<IVXSatoriExperiment>(); }
    }

    // ========================================================================
    // LIVE EVENTS
    // ========================================================================

    [Serializable]
    public class IVXSatoriLiveEvent
    {
        [JsonProperty("id")] public string id;
        [JsonProperty("name")] public string name;
        [JsonProperty("description")] public string description;
        [JsonProperty("startAt")] public long startAt;
        [JsonProperty("endAt")] public long endAt;
        [JsonProperty("status")] public string status;
        [JsonProperty("joined")] public bool joined;
        [JsonProperty("claimed")] public bool claimed;
        [JsonProperty("hasReward")] public bool hasReward;
        [JsonProperty("config")] public Dictionary<string, string> config;

        public IVXSatoriLiveEvent() { config = new Dictionary<string, string>(); }

        public bool IsActive => status == "active";
        public bool IsUpcoming => status == "upcoming";
        public bool IsEnded => status == "ended";
    }

    [Serializable]
    public class IVXSatoriLiveEventsResponse
    {
        [JsonProperty("events")] public List<IVXSatoriLiveEvent> events;
        public IVXSatoriLiveEventsResponse() { events = new List<IVXSatoriLiveEvent>(); }
    }

    [Serializable]
    public class IVXSatoriLiveEventReward
    {
        [JsonProperty("currencies")] public Dictionary<string, float> currencies;
        [JsonProperty("items")] public Dictionary<string, int> items;

        public IVXSatoriLiveEventReward()
        {
            currencies = new Dictionary<string, float>();
            items = new Dictionary<string, int>();
        }
    }

    // ========================================================================
    // MESSAGES
    // ========================================================================

    [Serializable]
    public class IVXSatoriMessage
    {
        [JsonProperty("id")] public string id;
        [JsonProperty("title")] public string title;
        [JsonProperty("body")] public string body;
        [JsonProperty("imageUrl")] public string imageUrl;
        [JsonProperty("metadata")] public Dictionary<string, string> metadata;
        [JsonProperty("hasReward")] public bool hasReward;
        [JsonProperty("createdAt")] public long createdAt;
        [JsonProperty("expiresAt")] public long expiresAt;
        [JsonProperty("readAt")] public long readAt;
        [JsonProperty("consumedAt")] public long consumedAt;

        public IVXSatoriMessage() { metadata = new Dictionary<string, string>(); }

        public bool IsRead => readAt > 0;
        public bool IsConsumed => consumedAt > 0;
    }

    [Serializable]
    public class IVXSatoriMessagesResponse
    {
        [JsonProperty("messages")] public List<IVXSatoriMessage> messages;
        public IVXSatoriMessagesResponse() { messages = new List<IVXSatoriMessage>(); }
    }

    [Serializable]
    public class IVXSatoriMessageReadResponse
    {
        [JsonProperty("message")] public IVXSatoriMessage message;
        [JsonProperty("reward")] public IVXSatoriLiveEventReward reward;
    }

    // ========================================================================
    // EVENT BATCH RESPONSE
    // ========================================================================

    [Serializable]
    public class IVXSatoriEventBatchResponse
    {
        [JsonProperty("submitted")] public int submitted;
        [JsonProperty("captured")] public int captured;
    }

    // ========================================================================
    // METRICS
    // ========================================================================

    [Serializable]
    public class IVXSatoriMetricDataPoint
    {
        [JsonProperty("bucket")] public string bucket;
        [JsonProperty("count")] public long count;
        [JsonProperty("sum")] public double sum;
        [JsonProperty("min")] public double min;
        [JsonProperty("max")] public double max;
        [JsonProperty("avg")] public double avg;
    }

    [Serializable]
    public class IVXSatoriMetricQueryResponse
    {
        [JsonProperty("metricId")] public string metricId;
        [JsonProperty("dataPoints")] public List<IVXSatoriMetricDataPoint> dataPoints;

        public IVXSatoriMetricQueryResponse() { dataPoints = new List<IVXSatoriMetricDataPoint>(); }
    }

    // ========================================================================
    // AUDIENCES (membership detail)
    // ========================================================================

    [Serializable]
    public class IVXSatoriAudienceMembership
    {
        [JsonProperty("audienceId")] public string audienceId;
        [JsonProperty("name")] public string name;
    }

    [Serializable]
    public class IVXSatoriAudiencesResponse
    {
        [JsonProperty("audiences")] public List<string> audiences;
        public IVXSatoriAudiencesResponse() { audiences = new List<string>(); }
    }
}
