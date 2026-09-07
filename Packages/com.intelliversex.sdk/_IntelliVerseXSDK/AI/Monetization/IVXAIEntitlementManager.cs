using System;
using UnityEngine;

namespace IntelliVerseX.AI
{
    /// <summary>
    /// Manages AI persona entitlements — free session tracking, subscription status,
    /// and purchase validation against the IVX backend.
    /// </summary>
    public sealed class IVXAIEntitlementManager
    {
        #region Events

        /// <summary>Fired when the entitlement state changes (e.g. after a purchase).</summary>
        public event Action<IVXAIEntitlementResponse> OnEntitlementChanged;

        /// <summary>Fired when payment is required to access a persona.</summary>
        public event Action<string> OnPaymentRequired;

        #endregion

        #region Properties

        /// <summary>Last known entitlement response from the backend.</summary>
        public IVXAIEntitlementResponse LastEntitlement { get; private set; }

        /// <summary>Whether the user currently has an active subscription.</summary>
        public bool HasSubscription => LastEntitlement?.HasSubscription == true;

        /// <summary>Remaining free sessions for today (from last check).</summary>
        public int FreeSessionsRemaining => LastEntitlement?.FreeSessionsRemaining ?? 0;

        #endregion

        #region Private Fields

        private readonly IVXAIApiClient _api;
        private readonly IVXAIConfig _config;
        private readonly string _userId;

        #endregion

        #region Constructor

        public IVXAIEntitlementManager(IVXAIApiClient api, IVXAIConfig config, string userId)
        {
            _api = api ?? throw new ArgumentNullException(nameof(api));
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _userId = userId;
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Check whether the user can access the given persona.
        /// The callback receives the full entitlement response.
        /// </summary>
        public void CheckAccess(string personaId, Action<IVXAIEntitlementResponse> onResult, Action<string> onError = null)
        {
            _api.CheckEntitlement(_userId, personaId, resp =>
            {
                LastEntitlement = resp;
                OnEntitlementChanged?.Invoke(resp);

                if (resp != null && !resp.CanAccessPersona)
                    OnPaymentRequired?.Invoke(resp.Reason ?? "Payment required");

                onResult?.Invoke(resp);
            },
            err =>
            {
                Debug.LogWarning($"[{nameof(IVXAIEntitlementManager)}] Entitlement check failed: {err}");
                onError?.Invoke(err);
            });
        }

        /// <summary>
        /// Refresh entitlement state without a specific persona check.
        /// </summary>
        public void Refresh(Action<IVXAIEntitlementResponse> onResult = null, Action<string> onError = null)
        {
            CheckAccess(null, onResult, onError);
        }

        /// <summary>
        /// Fetch available IAP products for AI.
        /// </summary>
        public void GetProducts(Action<IVXAIProductInfo[]> onResult, Action<string> onError = null)
        {
            _api.GetProducts(resp =>
            {
                if (resp?.Success == true)
                    onResult?.Invoke(resp.Products);
                else
                    onError?.Invoke(resp?.Error ?? "Failed to fetch products");
            },
            err => onError?.Invoke(err));
        }

        /// <summary>
        /// Submit a purchase receipt to the IVX backend for validation.
        /// On success, the entitlement state is automatically refreshed.
        /// </summary>
        public void SubmitPurchase(string productId, string receiptData, Action<IVXAIPurchaseResponse> onResult, Action<string> onError = null)
        {
            string platform;
#if UNITY_IOS
            platform = "ios";
#elif UNITY_ANDROID
            platform = "android";
#else
            platform = "other";
#endif

            var request = new IVXAIPurchaseRequest
            {
                UserId = _userId,
                ProductId = productId,
                ReceiptData = receiptData,
                Platform = platform
            };

            _api.Purchase(request, resp =>
            {
                if (resp?.Success == true && resp.Entitlement != null)
                {
                    LastEntitlement = resp.Entitlement;
                    OnEntitlementChanged?.Invoke(resp.Entitlement);
                }

                onResult?.Invoke(resp);
            },
            err => onError?.Invoke(err));
        }

        /// <summary>
        /// Convenience: returns true if the user can start at least one more free session.
        /// Based on the last cached entitlement response.
        /// </summary>
        public bool CanStartFreeSession() => FreeSessionsRemaining > 0;

        #endregion
    }
}
