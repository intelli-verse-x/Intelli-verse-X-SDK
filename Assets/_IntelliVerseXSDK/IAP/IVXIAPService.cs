// File: IVXIAPService.cs
// Purpose: In-App Purchase service with Unity IAP v5 integration
// Package: IntelliVerseX.IAP
// Dependencies: IntelliVerseX.Core, Unity IAP 5.x

using System;
using System.Collections.Generic;
using UnityEngine;
using IntelliVerseX.Core;

#if UNITY_PURCHASING
using System.Threading.Tasks;
using UnityEngine.Purchasing;
#endif

namespace IntelliVerseX.IAP
{
    /// <summary>
    /// Product types for IAP
    /// </summary>
    public enum IVXProductType
    {
        Consumable,
        NonConsumable,
        Subscription
    }

    /// <summary>
    /// IAP product configuration
    /// </summary>
    [Serializable]
    public class IVXProductConfig
    {
        [Tooltip("Store-specific product identifier (e.g. com.yourgame.coins_1000)")]
        public string productId;

        public IVXProductType productType;

        [Tooltip("Readable name for UI / debug")]
        public string displayName;

        [Tooltip("Reward amount (coins, gems etc.) that this purchase grants")]
        public int rewardAmount;

        [Tooltip("Optional: default price string used before store loads real price")]
        public string defaultPrice;

        [Tooltip("Optional: ISO 4217 currency code (e.g. USD, EUR)")]
        public string currencyCode;
    }

    /// <summary>
    /// IAP purchase result
    /// </summary>
    public class IVXPurchaseResult
    {
        public bool Success { get; set; }
        public string ProductId { get; set; }
        public string TransactionId { get; set; }
        public string ErrorMessage { get; set; }

        public static IVXPurchaseResult Ok(string productId, string transactionId) =>
            new IVXPurchaseResult
            {
                Success = true,
                ProductId = productId,
                TransactionId = transactionId
            };

        public static IVXPurchaseResult Error(string productId, string error) =>
            new IVXPurchaseResult
            {
                Success = false,
                ProductId = productId,
                ErrorMessage = error
            };
    }

    /// <summary>
    /// Core IAP service for Unity IAP v5 integration.
    /// Handles purchases, entitlements, and restores.
    ///
    /// Usage:
    ///   IVXIAPService.Instance.Initialize(products);
    ///   IVXIAPService.Instance.PurchaseProduct("product_id");
    ///   IVXIAPService.OnPurchaseComplete += HandlePurchase;
    /// </summary>
    public class IVXIAPService : IVXSafeSingleton<IVXIAPService>
    {
        #region Events

        /// <summary>
        /// Fired when purchase completes successfully (new or restored).
        /// </summary>
#pragma warning disable CS0067 // Event never used - public SDK API for external subscribers
        public static event Action<IVXPurchaseResult> OnPurchaseComplete;
#pragma warning restore CS0067

        /// <summary>
        /// Alias for OnPurchaseComplete (adapter compatibility).
        /// </summary>
        public static event Action<IVXPurchaseResult> OnPurchaseSuccess
        {
            add => OnPurchaseComplete += value;
            remove => OnPurchaseComplete -= value;
        }

        /// <summary>
        /// Fired when purchase fails.
        /// String contains a human-readable error message.
        /// </summary>
        public static event Action<string> OnPurchaseFailedEvent;

        /// <summary>
        /// Backwards-compatible alias for OnPurchaseFailedEvent.
        /// </summary>
        public static event Action<string> OnPurchaseFailed
        {
            add => OnPurchaseFailedEvent += value;
            remove => OnPurchaseFailedEvent -= value;
        }

        /// <summary>
        /// Fired when a restore flow completes (true = success, false = failed / not supported).
        /// </summary>
        public static event Action<bool> OnRestoreComplete;

        #endregion

        #region Private Fields

        private bool _isInitialized;
        private bool _isInitializing;
        private List<IVXProductConfig> _products = new List<IVXProductConfig>();

#if UNITY_PURCHASING
        private StoreController _storeController;

        // Cache of fetched products for price / metadata access
        private readonly Dictionary<string, Product> _fetchedProducts = new();
#endif

        #endregion

        #region Public Properties

        /// <summary>
        /// True when IAP is connected to the store and products have been fetched.
        /// </summary>
        public bool IsInitialized => _isInitialized;

        #endregion

        #region Initialization

        /// <summary>
        /// Initialize IAP system with product list.
        /// Can be safely called multiple times; subsequent calls are ignored once initialized.
        /// </summary>
        public void Initialize(List<IVXProductConfig> products)
        {
#if UNITY_PURCHASING
            if (_isInitialized || _isInitializing)
            {
                Debug.LogWarning("[IVXIAPService] Initialize called but IAP is already initializing / initialized.");
                return;
            }

            _products = products ?? new List<IVXProductConfig>();
            _ = InitializeInternalAsync(); // fire-and-forget async bootstrap
#else
            Debug.LogError("[IVXIAPService] Unity IAP is not enabled. Install/enable the In-App Purchasing package.");
            _isInitialized = false;
#endif
        }

#if UNITY_PURCHASING
        /// <summary>
        /// Full IAP v5 initialization:
        /// 1. Get StoreController
        /// 2. Attach event handlers
        /// 3. Connect()
        /// 4. FetchProducts()
        /// 5. FetchPurchases() (restores)
        /// </summary>
        private async Task InitializeInternalAsync()
        {
            _isInitializing = true;
            Debug.Log($"[IVXIAPService] Initializing IAP v5 with {_products.Count} products.");

            try
            {
                _storeController = UnityIAPServices.StoreController();
                if (_storeController == null)
                {
                    Debug.LogError("[IVXIAPService] UnityIAPServices.StoreController() returned null.");
                    _isInitializing = false;
                    _isInitialized = false;
                    return;
                }

                // Attach event handlers BEFORE calling Connect / Fetch.
                _storeController.OnProductsFetched += HandleProductsFetched;
                _storeController.OnProductsFetchFailed += HandleProductsFetchFailed;
                _storeController.OnPurchasesFetched += HandlePurchasesFetched;
                _storeController.OnPurchasesFetchFailed += HandlePurchasesFetchFailed;

                _storeController.OnPurchasePending += HandlePurchasePending;
                _storeController.OnPurchaseConfirmed += HandlePurchaseConfirmed;
                _storeController.OnPurchaseFailed += HandlePurchaseFailed;

                _storeController.OnStoreDisconnected += HandleStoreDisconnected;

                Debug.Log("[IVXIAPService] Connecting to store...");
                await _storeController.Connect();  // Connect to current app store

                Debug.Log("[IVXIAPService] Store connected. Fetching products...");

                var productDefs = BuildProductDefinitions();
                if (productDefs.Count == 0)
                {
                    Debug.LogWarning("[IVXIAPService] No valid products configured; IAP will be effectively disabled.");
                    _isInitialized = true;
                    _isInitializing = false;
                    return;
                }

                _storeController.FetchProducts(productDefs);
            }
            catch (Exception ex)
            {
                Debug.LogError($"[IVXIAPService] IAP Connect/Fetch failed: {ex}");
                _isInitialized = false;
                _isInitializing = false;
            }
        }

        private List<ProductDefinition> BuildProductDefinitions()
        {
            var defs = new List<ProductDefinition>();

            foreach (var cfg in _products)
            {
                if (cfg == null)
                    continue;

                if (string.IsNullOrWhiteSpace(cfg.productId))
                {
                    Debug.LogWarning("[IVXIAPService] Skipping product with empty productId.");
                    continue;
                }

                var type = ConvertProductType(cfg.productType);
                defs.Add(new ProductDefinition(cfg.productId, type));
                Debug.Log($"[IVXIAPService] Configured product: {cfg.productId} ({type})");
            }

            return defs;
        }
#endif

        #endregion

        #region Product Access

        /// <summary>
        /// Get product configuration by ID.
        /// Returns null if not found.
        /// </summary>
        public IVXProductConfig GetProduct(string productId)
        {
            if (string.IsNullOrEmpty(productId))
            {
                Debug.LogWarning("[IVXIAPService] GetProduct called with null/empty productId.");
                return null;
            }

            if (_products == null || _products.Count == 0)
            {
                Debug.LogWarning("[IVXIAPService] GetProduct called but no products are configured.");
                return null;
            }

            return _products.Find(p => p.productId == productId);
        }

        /// <summary>
        /// Get all configured products.
        /// Returns an empty list if nothing is configured.
        /// </summary>
        public List<IVXProductConfig> GetAllProducts()
        {
            return _products ?? new List<IVXProductConfig>();
        }

#if UNITY_PURCHASING
        /// <summary>
        /// Get localized price string from store metadata (e.g. "$0.99").
        /// </summary>
        public string GetLocalizedPrice(string productId)
        {
            return _fetchedProducts.TryGetValue(productId, out var p)
                ? p.metadata.localizedPriceString
                : string.Empty;
        }

        public string GetCurrencyCode(string productId)
        {
            return _fetchedProducts.TryGetValue(productId, out var p)
                ? p.metadata.isoCurrencyCode
                : string.Empty;
        }

        public string GetLocalizedTitle(string productId)
        {
            return _fetchedProducts.TryGetValue(productId, out var p)
                ? p.metadata.localizedTitle
                : string.Empty;
        }

        public string GetLocalizedDescription(string productId)
        {
            return _fetchedProducts.TryGetValue(productId, out var p)
                ? p.metadata.localizedDescription
                : string.Empty;
        }
#endif

        #endregion

        #region Purchase Methods

        /// <summary>
        /// Start purchasing a product by ID.
        /// </summary>
        public void PurchaseProduct(string productId)
        {
#if UNITY_PURCHASING
            if (string.IsNullOrEmpty(productId))
            {
                Debug.LogError("[IVXIAPService] PurchaseProduct called with null/empty productId.");
                OnPurchaseFailedEvent?.Invoke("Invalid product id");
                return;
            }

            if (!_isInitialized)
            {
                Debug.LogError("[IVXIAPService] PurchaseProduct failed: IAP not initialized.");
                OnPurchaseFailedEvent?.Invoke("IAP not initialized");
                return;
            }

            if (_storeController == null)
            {
                Debug.LogError("[IVXIAPService] PurchaseProduct failed: StoreController is null.");
                OnPurchaseFailedEvent?.Invoke("Store not available");
                return;
            }

            // Optional: sanity check against fetched products
            if (!_fetchedProducts.ContainsKey(productId))
            {
                Debug.LogWarning($"[IVXIAPService] Purchasing product '{productId}' that has not been fetched yet.");
            }

            Debug.Log($"[IVXIAPService] Initiating purchase for product: {productId}");
            _storeController.PurchaseProduct(productId); // v5 API
#else
            Debug.LogError("[IVXIAPService] Unity IAP not enabled.");
            OnPurchaseFailedEvent?.Invoke("Unity IAP not enabled");
#endif
        }

        /// <summary>
        /// Restore previous purchases.
        /// On iOS this is required; on Google Play it generally maps to FetchPurchases.
        /// </summary>
        public void RestorePurchases()
        {
#if UNITY_PURCHASING
            if (!_isInitialized || _storeController == null)
            {
                Debug.LogError("[IVXIAPService] Cannot restore purchases - IAP not initialized or StoreController is null.");
                OnRestoreComplete?.Invoke(false);
                return;
            }

            Debug.Log("[IVXIAPService] Restoring purchases via StoreController.RestoreTransactions()");

            // v5: RestoreTransactions triggers OnPurchasesFetched with restored orders.
            _storeController.RestoreTransactions((ok, message) =>
            {
                Debug.Log($"[IVXIAPService] Restore result: {ok} ({message})");
                OnRestoreComplete?.Invoke(ok);
            });
#else
            Debug.LogWarning("[IVXIAPService] RestorePurchases called but Unity IAP is not enabled.");
            OnRestoreComplete?.Invoke(false);
#endif
        }

        #endregion

        #region Unity IAP v5 Event Handlers

#if UNITY_PURCHASING
        // === Product fetching ===

        private void HandleProductsFetched(List<Product> products)
        {
            Debug.Log($"[IVXIAPService] Products fetched: {products?.Count ?? 0}");

            _fetchedProducts.Clear();

            if (products != null)
            {
                foreach (var p in products)
                {
                    if (p?.definition == null) continue;

                    _fetchedProducts[p.definition.id] = p;
                    Debug.Log($"[IVXIAPService] Product metadata: {p.definition.id} | " +
                              $"{p.metadata.localizedTitle} | {p.metadata.localizedPriceString}");
                }
            }

            // At this point we consider the system "initialized enough" to purchase.
            _isInitialized = true;
            _isInitializing = false;

            // Fetch existing purchases to restore entitlements (non-consumables, subs).
            Debug.Log("[IVXIAPService] Fetching existing purchases...");
            _storeController.FetchPurchases();
        }

        private void HandleProductsFetchFailed(ProductFetchFailed failure)
        {
            if (failure == null)
            {
                Debug.LogError("[IVXIAPService] OnProductsFetchFailed: failure object is null");
                _isInitialized = false;
                _isInitializing = false;
                return;
            }

            string reason = failure.FailureReason;
            string failedIds = string.Empty;

            var failedList = failure.FailedFetchProducts;
            if (failedList != null && failedList.Count > 0)
            {
                var ids = new List<string>(failedList.Count);
                for (int i = 0; i < failedList.Count; i++)
                {
                    if (failedList[i] != null && !string.IsNullOrEmpty(failedList[i].id))
                        ids.Add(failedList[i].id);
                }

                if (ids.Count > 0)
                    failedIds = string.Join(", ", ids);
            }

            if (!string.IsNullOrEmpty(failedIds))
                Debug.LogError($"[IVXIAPService] OnProductsFetchFailed: {reason} | Products: {failedIds}");
            else
                Debug.LogError($"[IVXIAPService] OnProductsFetchFailed: {reason}");

            _isInitialized = false;
            _isInitializing = false;
        }

        // === Purchases fetching (restores / entitlements) ===

        private void HandlePurchasesFetched(Orders orders)
        {
            Debug.Log("[IVXIAPService] OnPurchasesFetched received.");

            if (orders == null)
            {
                Debug.LogWarning("[IVXIAPService] OnPurchasesFetched called with null Orders.");
                return;
            }

            Debug.Log($"[IVXIAPService] Confirmed: {orders.ConfirmedOrders.Count}, " +
                      $"Pending: {orders.PendingOrders.Count}, Deferred: {orders.DeferredOrders.Count}");

            // NOTE: Actual entitlement restoration should be idempotent and implemented
            //       in your game code using data from Orders (ConfirmedOrders, etc.).
        }

        private void HandlePurchasesFetchFailed(PurchasesFetchFailureDescription failure)
        {
            if (failure == null)
            {
                Debug.LogError("[IVXIAPService] OnPurchasesFetchFailed: failure object is null");
                return;
            }

            Debug.LogError($"[IVXIAPService] OnPurchasesFetchFailed: {failure.FailureReason} - {failure.Message}");
        }

        // === Live purchase flow ===

        /// <summary>
        /// Safely extracts the first productId from a cart, or "Unknown".
        /// </summary>
        private string GetProductIdFromCart(ICart cart)
        {
            if (cart == null)
                return "Unknown";

            var items = cart.Items();
            if (items == null || items.Count == 0 || items[0] == null || items[0].Product == null ||
                items[0].Product.definition == null)
                return "Unknown";

            return items[0].Product.definition.id;
        }

        /// <summary>
        /// Called when a new purchase is created and is waiting to be confirmed.
        /// This is where we grant the reward & then confirm with the store.
        /// </summary>
        private void HandlePurchasePending(PendingOrder pending)
        {
            if (pending == null)
            {
                Debug.LogError("[IVXIAPService] HandlePurchasePending received null PendingOrder.");
                OnPurchaseFailedEvent?.Invoke("Invalid pending order");
                return;
            }

            string productId = GetProductIdFromCart(pending.CartOrdered);
            string transactionId = pending.Info != null ? pending.Info.TransactionID : string.Empty;

            Debug.Log($"[IVXIAPService] Purchase pending: {productId}, Tx: {transactionId}");

            // Fire success result so game code can grant rewards immediately.
            var result = IVXPurchaseResult.Ok(productId, transactionId);
            OnPurchaseComplete?.Invoke(result);

            // IMPORTANT:
            // If you do server-side receipt validation, move ConfirmPurchase AFTER validation succeeds.
            _storeController.ConfirmPurchase(pending);
            Debug.Log("[IVXIAPService] Pending purchase confirmed with store.");
        }

        /// <summary>
        /// Called when the store reports that an order is fully confirmed.
        /// We mostly log here because we already granted the reward in HandlePurchasePending.
        /// </summary>
        private void HandlePurchaseConfirmed(Order order)
        {
            if (order == null)
            {
                Debug.LogWarning("[IVXIAPService] HandlePurchaseConfirmed: order is null.");
                return;
            }

            string productId = GetProductIdFromCart(order.CartOrdered);
            string transactionId = order.Info != null ? order.Info.TransactionID : string.Empty;

            Debug.Log($"[IVXIAPService] Purchase confirmed by store. Product: {productId}, Tx: {transactionId}");
        }

        /// <summary>
        /// Called when a purchase fails.
        /// </summary>
        private void HandlePurchaseFailed(FailedOrder failed)
        {
            if (failed == null)
            {
                Debug.LogError("[IVXIAPService] HandlePurchaseFailed received null FailedOrder.");
                OnPurchaseFailedEvent?.Invoke("Unknown purchase failure");
                return;
            }

            string productId = GetProductIdFromCart(failed.CartOrdered);
            string reason = failed.FailureReason.ToString();
            string details = failed.Details ?? string.Empty;

            string message = string.IsNullOrEmpty(details)
                ? $"Purchase failed for {productId}: {reason}"
                : $"Purchase failed for {productId}: {reason} ({details})";

            Debug.LogWarning($"[IVXIAPService] {message}");
            OnPurchaseFailedEvent?.Invoke(message);
        }

        /// <summary>
        /// Called when the store connection is lost.
        /// </summary>
        private void HandleStoreDisconnected(StoreConnectionFailureDescription failure)
        {
            string message = failure != null ? failure.Message : "Unknown store disconnection";
            Debug.LogError($"[IVXIAPService] Store disconnected: {message}");
            _isInitialized = false;
        }
#endif

        #endregion

        #region Helper Methods

#if UNITY_PURCHASING
        private ProductType ConvertProductType(IVXProductType type)
        {
            switch (type)
            {
                case IVXProductType.Consumable:
                    return ProductType.Consumable;
                case IVXProductType.NonConsumable:
                    return ProductType.NonConsumable;
                case IVXProductType.Subscription:
                    return ProductType.Subscription;
                default:
                    Debug.LogWarning($"[IVXIAPService] Unknown IVXProductType {type}, defaulting to Consumable.");
                    return ProductType.Consumable;
            }
        }
#endif

        #endregion
    }
}
