// File: IVXSubscriptionManager.cs
// Purpose: Subscription management with entitlement tracking
// Package: IntelliVerseX.IAP
// Dependencies: IntelliVerseX.Core, IntelliVerseX.IAP

using System;
using UnityEngine;
using IntelliVerseX.Core;

namespace IntelliVerseX.IAP
{
    /// <summary>
    /// Subscription status
    /// </summary>
    public enum IVXSubscriptionStatus
    {
        NotSubscribed,
        Active,
        Expired,
        Cancelled,
        GracePeriod
    }

    /// <summary>
    /// Manages subscription entitlements and premium features.
    /// Supports free trial and monthly subscription gating.
    /// 
    /// Usage:
    ///   IVXSubscriptionManager.Instance.Initialize("subscription_product_id");
    ///   bool hasAccess = IVXSubscriptionManager.Instance.HasPremiumAccess();
    ///   IVXSubscriptionManager.Instance.ConsumeTrial();
    /// </summary>
    public class IVXSubscriptionManager : IVXSafeSingleton<IVXSubscriptionManager>
    {
        #region Constants

        private const string TRIAL_USED_KEY = "ivx_trial_used";
        private const string SUBSCRIPTION_EXPIRY_KEY = "ivx_subscription_expiry";
        private const string GRACE_PERIOD_DAYS = "ivx_grace_period_days";
        private const int DEFAULT_GRACE_PERIOD = 3; // days

        #endregion

        #region Events

        /// <summary>
        /// Fired when subscription status changes
        /// </summary>
        public static event Action<IVXSubscriptionStatus> OnSubscriptionStatusChanged;

        /// <summary>
        /// Fired when premium access is denied and a paywall should be shown.
        /// Args: (productId). Games/samples should open UI and call <see cref="PurchaseSubscriptionAsync"/>.
        /// </summary>
        public static event Action<string> OnPaywallRequested;

        #endregion

        #region Private Fields

        private string _subscriptionProductId;
        private IVXSubscriptionStatus _currentStatus = IVXSubscriptionStatus.NotSubscribed;
        private DateTime _expiryDate = DateTime.MinValue;

        #endregion

        #region Public Properties

        public IVXSubscriptionStatus CurrentStatus => _currentStatus;
        public DateTime ExpiryDate => _expiryDate;

        #endregion

        #region Initialization

        /// <summary>
        /// Initialize subscription manager
        /// </summary>
        public void Initialize(string subscriptionProductId)
        {
            _subscriptionProductId = subscriptionProductId;

            // Load subscription state
            LoadSubscriptionState();

            // Subscribe to IAP events
            IVXIAPService.OnPurchaseComplete += OnPurchaseComplete;

            Debug.Log($"[IVXSubscriptionManager] Initialized with product: {subscriptionProductId}, Status: {_currentStatus}");
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            // Unsubscribe from events
            IVXIAPService.OnPurchaseComplete -= OnPurchaseComplete;
        }

        #endregion

        #region Premium Access

        /// <summary>
        /// Check if user has premium access (trial or subscription)
        /// </summary>
        public bool HasPremiumAccess()
        {
            // Check subscription first
            if (_currentStatus == IVXSubscriptionStatus.Active)
                return true;

            // Check grace period
            if (_currentStatus == IVXSubscriptionStatus.GracePeriod)
                return true;

            // Check free trial
            if (HasTrialAvailable())
                return false; // Trial available but not consumed yet

            return false;
        }

        /// <summary>
        /// Check if free trial is available (not consumed)
        /// </summary>
        public bool HasTrialAvailable()
        {
            return PlayerPrefs.GetInt(TRIAL_USED_KEY, 0) == 0;
        }

        /// <summary>
        /// Consume free trial (one-time use)
        /// </summary>
        public void ConsumeTrial()
        {
            if (!HasTrialAvailable())
            {
                Debug.LogWarning("[IVXSubscriptionManager] Trial already consumed");
                return;
            }

            PlayerPrefs.SetInt(TRIAL_USED_KEY, 1);
            PlayerPrefs.Save();

            Debug.Log("[IVXSubscriptionManager] Free trial consumed");
        }

        /// <summary>
        /// Require premium access (shows paywall if not subscribed)
        /// Returns true if access granted, false if paywall should be shown
        /// </summary>
        public bool RequirePremiumAccess()
        {
            // Check subscription
            if (HasPremiumAccess())
                return true;

            // Check trial
            if (HasTrialAvailable())
            {
                ConsumeTrial();
                return true;
            }

            RequestPaywall();
            return false;
        }

        /// <summary>
        /// Raise <see cref="OnPaywallRequested"/> for game UI. Safe to call from gates/buttons.
        /// </summary>
        public void RequestPaywall()
        {
            string productId = _subscriptionProductId ?? string.Empty;
            Debug.Log($"[IVXSubscriptionManager] Paywall requested for product '{productId}'");
            try
            {
                OnPaywallRequested?.Invoke(productId);
            }
            catch (Exception ex)
            {
                Debug.LogError($"[IVXSubscriptionManager] OnPaywallRequested subscriber error: {ex.Message}");
            }
        }

        /// <summary>
        /// Start the store purchase flow for the configured subscription product.
        /// </summary>
        public void PurchaseSubscriptionAsync()
        {
            if (string.IsNullOrEmpty(_subscriptionProductId))
            {
                Debug.LogError("[IVXSubscriptionManager] No subscription product id. Call Initialize first.");
                return;
            }

            var iap = IVXIAPService.Instance;
            if (iap == null)
            {
                Debug.LogError("[IVXSubscriptionManager] IVXIAPService not available.");
                return;
            }

            iap.PurchaseProduct(_subscriptionProductId);
        }

        #endregion

        #region Subscription Management

        /// <summary>
        /// Activate subscription (called after purchase)
        /// </summary>
        public void ActivateSubscription(int durationDays = 30)
        {
            DateTime expiry = DateTime.UtcNow.AddDays(durationDays);
            UpdateSubscriptionStatus(expiry, true);
            Debug.Log($"[IVXSubscriptionManager] Subscription activated for {durationDays} days");
        }

        /// <summary>
        /// Check if subscription is currently active
        /// </summary>
        public bool IsSubscriptionActive()
        {
            return _currentStatus == IVXSubscriptionStatus.Active || 
                   _currentStatus == IVXSubscriptionStatus.GracePeriod;
        }

        /// <summary>
        /// Alias for IsSubscriptionActive (adapter compatibility)
        /// </summary>
        public bool HasActiveSubscription()
        {
            return IsSubscriptionActive();
        }

        /// <summary>
        /// Update subscription status from server/receipt
        /// </summary>
        public void UpdateSubscriptionStatus(DateTime expiryDate, bool autoRenews)
        {
            _expiryDate = expiryDate;

            DateTime now = DateTime.UtcNow;
            IVXSubscriptionStatus newStatus;

            if (expiryDate > now)
            {
                // Active subscription
                newStatus = IVXSubscriptionStatus.Active;
            }
            else if (expiryDate.AddDays(DEFAULT_GRACE_PERIOD) > now)
            {
                // Grace period
                newStatus = IVXSubscriptionStatus.GracePeriod;
            }
            else
            {
                // Expired
                newStatus = autoRenews ? IVXSubscriptionStatus.Cancelled : IVXSubscriptionStatus.Expired;
            }

            SetSubscriptionStatus(newStatus);

            // Save expiry date
            PlayerPrefs.SetString(SUBSCRIPTION_EXPIRY_KEY, expiryDate.ToString("o"));
            PlayerPrefs.Save();

            Debug.Log($"[IVXSubscriptionManager] Status updated: {newStatus}, Expiry: {expiryDate:yyyy-MM-dd}");
        }

        #endregion

        #region Private Methods

        private void LoadSubscriptionState()
        {
            // Load expiry date
            string expiryString = PlayerPrefs.GetString(SUBSCRIPTION_EXPIRY_KEY, "");
            if (!string.IsNullOrEmpty(expiryString))
            {
                if (DateTime.TryParse(expiryString, out DateTime expiry))
                {
                    _expiryDate = expiry;
                    UpdateSubscriptionStatus(_expiryDate, false);
                }
            }
        }

        private void SetSubscriptionStatus(IVXSubscriptionStatus newStatus)
        {
            if (_currentStatus == newStatus)
                return;

            _currentStatus = newStatus;
            OnSubscriptionStatusChanged?.Invoke(_currentStatus);

            Debug.Log($"[IVXSubscriptionManager] Status changed to: {_currentStatus}");
        }

        private void OnPurchaseComplete(IVXPurchaseResult result)
        {
            if (result.ProductId != _subscriptionProductId)
                return;

            Debug.Log($"[IVXSubscriptionManager] Subscription purchased: {result.ProductId}");

            // Activate subscription (30 days from now)
            DateTime expiry = DateTime.UtcNow.AddDays(30);
            UpdateSubscriptionStatus(expiry, true);
        }

        #endregion
    }

    /// <summary>
    /// Component for gating premium features.
    /// Attach to GameObjects that should only be accessible with premium access.
    /// </summary>
    public class IVXPremiumGate : MonoBehaviour
    {
        #region Inspector Fields

        [Header("Gate Settings")]
        [Tooltip("Auto-disable if premium access not available")]
        [SerializeField] private bool autoDisable = true;

        [Tooltip("Show paywall on click if not premium")]
        [SerializeField] private bool showPaywallOnClick = true;

        [Tooltip("Optional: purchase immediately via IAP when paywall is requested (no custom UI).")]
        [SerializeField] private bool purchaseOnPaywall = false;

        [Header("Optional Paywall UI")]
        [Tooltip("Optional panel shown when paywall is requested. If null, only the event fires.")]
        [SerializeField] private GameObject paywallPanel;

        [Tooltip("Optional buy button on the paywall panel")]
        [SerializeField] private UnityEngine.UI.Button paywallBuyButton;

        [Tooltip("Optional close button on the paywall panel")]
        [SerializeField] private UnityEngine.UI.Button paywallCloseButton;

        #endregion

        #region Unity Lifecycle

        private void Start()
        {
            if (autoDisable)
            {
                CheckPremiumAccess();
            }

            IVXSubscriptionManager.OnSubscriptionStatusChanged += OnStatusChanged;
            IVXSubscriptionManager.OnPaywallRequested += OnPaywallRequested;

            if (paywallBuyButton != null)
                paywallBuyButton.onClick.AddListener(OnPaywallBuyClicked);
            if (paywallCloseButton != null)
                paywallCloseButton.onClick.AddListener(HidePaywall);

            if (paywallPanel != null)
                paywallPanel.SetActive(false);
        }

        private void OnDestroy()
        {
            IVXSubscriptionManager.OnSubscriptionStatusChanged -= OnStatusChanged;
            IVXSubscriptionManager.OnPaywallRequested -= OnPaywallRequested;
        }

        #endregion

        #region Public API

        /// <summary>
        /// Check if premium access is available
        /// </summary>
        public bool CheckPremiumAccess()
        {
            if (IVXSubscriptionManager.Instance == null)
            {
                Debug.LogError("[IVXPremiumGate] Subscription manager not initialized");
                return false;
            }

            bool hasAccess = IVXSubscriptionManager.Instance.HasPremiumAccess();

            if (autoDisable)
            {
                gameObject.SetActive(hasAccess);
            }

            return hasAccess;
        }

        /// <summary>
        /// Require premium access (call before premium action)
        /// </summary>
        public bool RequirePremiumAccess()
        {
            if (IVXSubscriptionManager.Instance == null)
            {
                Debug.LogError("[IVXPremiumGate] Subscription manager not initialized");
                return false;
            }

            // Manager raises OnPaywallRequested when access is denied.
            return IVXSubscriptionManager.Instance.RequirePremiumAccess();
        }

        /// <summary>Show the optional paywall panel and/or start purchase.</summary>
        public void ShowPaywall()
        {
            if (paywallPanel != null)
                paywallPanel.SetActive(true);
            else
                IVXSubscriptionManager.Instance?.RequestPaywall();

            if (purchaseOnPaywall)
                IVXSubscriptionManager.Instance?.PurchaseSubscriptionAsync();
        }

        public void HidePaywall()
        {
            if (paywallPanel != null)
                paywallPanel.SetActive(false);
        }

        #endregion

        #region Private Methods

        private void OnStatusChanged(IVXSubscriptionStatus newStatus)
        {
            Debug.Log($"[IVXPremiumGate] Status changed: {newStatus}");
            CheckPremiumAccess();
            if (newStatus == IVXSubscriptionStatus.Active || newStatus == IVXSubscriptionStatus.GracePeriod)
                HidePaywall();
        }

        private void OnPaywallRequested(string productId)
        {
            if (!showPaywallOnClick)
                return;

            if (paywallPanel != null)
                paywallPanel.SetActive(true);

            if (purchaseOnPaywall && IVXSubscriptionManager.Instance != null)
                IVXSubscriptionManager.Instance.PurchaseSubscriptionAsync();
            else if (paywallPanel == null && !purchaseOnPaywall)
                Debug.Log($"[IVXPremiumGate] Paywall requested for '{productId}'. Assign paywallPanel or subscribe to IVXSubscriptionManager.OnPaywallRequested.");
        }

        private void OnPaywallBuyClicked()
        {
            IVXSubscriptionManager.Instance?.PurchaseSubscriptionAsync();
        }

        #endregion
    }
}
