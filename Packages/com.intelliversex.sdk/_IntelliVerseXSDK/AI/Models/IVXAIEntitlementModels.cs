using System;
using Newtonsoft.Json;

namespace IntelliVerseX.AI
{
    /// <summary>
    /// Response from the entitlement check endpoint.
    /// </summary>
    [Serializable]
    public class IVXAIEntitlementResponse : IVXAIBaseResponse
    {
        [JsonProperty("userId")] public string UserId;
        [JsonProperty("hasSubscription")] public bool HasSubscription;
        [JsonProperty("subscriptionExpiryDate")] public string SubscriptionExpiryDate;
        [JsonProperty("freeTrialUsed")] public bool FreeTrialUsed;
        [JsonProperty("freeSessionsRemaining")] public int FreeSessionsRemaining;
        [JsonProperty("totalSessionsCompleted")] public int TotalSessionsCompleted;
        [JsonProperty("canAccessPersona")] public bool CanAccessPersona;
        [JsonProperty("reason")] public string Reason;
    }

    /// <summary>
    /// IAP product information returned by the AI products endpoint.
    /// </summary>
    [Serializable]
    public class IVXAIProductInfo
    {
        [JsonProperty("productId")] public string ProductId;
        [JsonProperty("displayName")] public string DisplayName;
        [JsonProperty("description")] public string Description;
        [JsonProperty("price")] public float Price;
        [JsonProperty("currency")] public string Currency;
        [JsonProperty("type")] public string Type;
        [JsonProperty("persona")] public string Persona;
        [JsonProperty("sessionsIncluded")] public int SessionsIncluded;
        [JsonProperty("durationDays")] public int DurationDays;
        [JsonProperty("badge")] public string Badge;
        [JsonProperty("isPopular")] public bool IsPopular;
        [JsonProperty("discountPercent")] public int DiscountPercent;

        public IVXAIProductType GetProductType()
        {
            return Type switch
            {
                "consumable"     => IVXAIProductType.Consumable,
                "non_consumable" => IVXAIProductType.NonConsumable,
                "subscription"   => IVXAIProductType.Subscription,
                _                => IVXAIProductType.Consumable
            };
        }

        public string GetFormattedPrice() => $"${Price:F2}";
    }

    /// <summary>
    /// Response from the products list endpoint.
    /// </summary>
    [Serializable]
    public class IVXAIProductsResponse : IVXAIBaseResponse
    {
        [JsonProperty("products")] public IVXAIProductInfo[] Products;
    }

    /// <summary>
    /// Purchase request body.
    /// </summary>
    [Serializable]
    public class IVXAIPurchaseRequest
    {
        [JsonProperty("userId")] public string UserId;
        [JsonProperty("productId")] public string ProductId;
        [JsonProperty("receiptData")] public string ReceiptData;
        [JsonProperty("platform")] public string Platform;
    }

    /// <summary>
    /// Purchase response.
    /// </summary>
    [Serializable]
    public class IVXAIPurchaseResponse : IVXAIBaseResponse
    {
        [JsonProperty("message")] public string Message;
        [JsonProperty("entitlement")] public IVXAIEntitlementResponse Entitlement;
    }
}
