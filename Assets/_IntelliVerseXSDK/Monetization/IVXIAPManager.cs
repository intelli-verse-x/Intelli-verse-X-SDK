using System;
using System.Collections.Generic;
using UnityEngine;

namespace IntelliVerseX.Monetization
{
    // IVXProductType is now defined in IVXIAPConfig.cs to avoid duplication

    /// <summary>
    /// IAP product data
    /// </summary>
    [Serializable]
    public class IVXProduct
    {
        public string productId;
        public string title;
        public string description;
        public string price;
        public decimal priceDecimal;
        public string currencyCode;
        public IVXProductType type;
        public bool isAvailable;
    }

    /// <summary>
    /// IAP purchase result
    /// </summary>
    public class IVXPurchaseResult
    {
        public bool success;
        public string productId;
        public string transactionId;
        public string receipt;
        public string error;
    }

    /// <summary>
    /// In-App Purchase manager for IntelliVerse-X SDK.
    /// Unified IAP interface for iOS, Android, and other platforms.
    /// 
    /// Usage:
    ///   // Initialize
    ///   IVXIAPManager.Initialize(products);
    ///   
    ///   // Get products
    ///   var coinPack = IVXIAPManager.GetProduct("com.yourcompany.game.coins_100");
    ///   
    ///   // Purchase
    ///   IVXIAPManager.PurchaseProduct("com.yourcompany.game.coins_100", (result) => {
    ///       if (result.success) {
    ///           GiveCoins(100);
    ///       }
    ///   });
    ///   
    ///   // Restore purchases
    ///   IVXIAPManager.RestorePurchases((success) => {
    ///       if (success) ShowRestoreSuccess();
    ///   });
    /// </summary>
    public static class IVXIAPManager
    {
        private static bool _isInitialized = false;
        private static Dictionary<string, IVXProduct> _products = new Dictionary<string, IVXProduct>();

        // Events
        public static event Action<IVXPurchaseResult> OnPurchaseComplete;
        public static event Action<string> OnPurchaseFailed;
        public static event Action OnRestoreComplete;
#pragma warning disable CS0067 // Event is never used - public API for external consumption
        public static event Action<string> OnIAPError;
#pragma warning restore CS0067

        /// <summary>
        /// Initialize IAP system with product catalog
        /// </summary>
        /// <param name="productIds">Array of product IDs to initialize</param>
        public static void Initialize(string[] productIds)
        {
            if (_isInitialized)
            {
                Debug.LogWarning("[IVXIAPManager] Already initialized");
                return;
            }

            if (productIds == null || productIds.Length == 0)
            {
                Debug.LogWarning("[IVXIAPManager] No products provided");
                return;
            }

            Debug.Log($"[IVXIAPManager] Initializing with {productIds.Length} products");

            // Initialize Unity IAP or other IAP SDK here
            // Example with Unity IAP:
            // var builder = ConfigurationBuilder.Instance(StandardPurchasingModule.Instance());
            // foreach (var productId in productIds)
            // {
            //     builder.AddProduct(productId, ProductType.Consumable);
            // }
            // UnityPurchasing.Initialize(this, builder);

            _isInitialized = true;
        }

        /// <summary>
        /// Initialize IAP system with IVXIAPConfig
        /// </summary>
        /// <param name="config">IAP configuration</param>
        public static void Initialize(IVXIAPConfig config)
        {
            if (_isInitialized)
            {
                Debug.LogWarning("[IVXIAPManager] Already initialized");
                return;
            }

            if (config == null || config.products == null || config.products.Count == 0)
            {
                Debug.LogWarning("[IVXIAPManager] No products in config");
                return;
            }

            Debug.Log($"[IVXIAPManager] Initializing with {config.products.Count} products from config");

            // Store products from config
            foreach (var product in config.products)
            {
                _products[product.productId] = new IVXProduct
                {
                    productId = product.productId,
                    title = product.productName,
                    type = product.productType,
                    isAvailable = true
                };
            }

            _isInitialized = true;
            Debug.Log("[IVXIAPManager] IAP initialized successfully");
        }

        /// <summary>
        /// Initialize with product catalog (detailed)
        /// </summary>
        public static void Initialize(IVXProduct[] products)
        {
            if (_isInitialized)
            {
                Debug.LogWarning("[IVXIAPManager] Already initialized");
                return;
            }

            if (products == null || products.Length == 0)
            {
                Debug.LogWarning("[IVXIAPManager] No products provided");
                return;
            }

            Debug.Log($"[IVXIAPManager] Initializing with {products.Length} products");

            foreach (var product in products)
            {
                _products[product.productId] = product;
            }

            // Initialize with product IDs
            var productIds = new string[products.Length];
            for (int i = 0; i < products.Length; i++)
            {
                productIds[i] = products[i].productId;
            }

            Initialize(productIds);
        }

        /// <summary>
        /// Check if IAP is initialized
        /// </summary>
        public static bool IsInitialized()
        {
            return _isInitialized;
        }

        /// <summary>
        /// Get product by ID
        /// </summary>
        public static IVXProduct GetProduct(string productId)
        {
            if (!_isInitialized)
            {
                Debug.LogWarning("[IVXIAPManager] Not initialized");
                return null;
            }

            if (_products.ContainsKey(productId))
            {
                return _products[productId];
            }

            Debug.LogWarning($"[IVXIAPManager] Product not found: {productId}");
            return null;
        }

        /// <summary>
        /// Get all products
        /// </summary>
        public static List<IVXProduct> GetAllProducts()
        {
            return new List<IVXProduct>(_products.Values);
        }

        /// <summary>
        /// Purchase a product
        /// </summary>
        /// <param name="productId">Product ID to purchase</param>
        /// <param name="onComplete">Callback with purchase result</param>
        public static void PurchaseProduct(string productId, Action<IVXPurchaseResult> onComplete = null)
        {
            if (!_isInitialized)
            {
                Debug.LogError("[IVXIAPManager] Not initialized. Call Initialize() first.");
                onComplete?.Invoke(new IVXPurchaseResult
                {
                    success = false,
                    productId = productId,
                    error = "IAP not initialized"
                });
                OnPurchaseFailed?.Invoke("IAP not initialized");
                return;
            }

            if (!_products.ContainsKey(productId))
            {
                Debug.LogError($"[IVXIAPManager] Product not found: {productId}");
                onComplete?.Invoke(new IVXPurchaseResult
                {
                    success = false,
                    productId = productId,
                    error = "Product not found"
                });
                OnPurchaseFailed?.Invoke("Product not found");
                return;
            }

            Debug.Log($"[IVXIAPManager] Purchasing product: {productId}");

            // Initiate purchase with Unity IAP or other IAP SDK
            // Example:
            // m_StoreController.InitiatePurchase(productId);

            // For testing, simulate success
            SimulatePurchaseForTesting(productId, onComplete);
        }

        /// <summary>
        /// Restore previous purchases (iOS required)
        /// </summary>
        public static void RestorePurchases(Action<bool> onComplete = null)
        {
            if (!_isInitialized)
            {
                Debug.LogError("[IVXIAPManager] Not initialized");
                onComplete?.Invoke(false);
                return;
            }

            Debug.Log("[IVXIAPManager] Restoring purchases...");

            // Restore purchases with Unity IAP or other IAP SDK
            // Example:
            // if (Application.platform == RuntimePlatform.IPhonePlayer)
            // {
            //     var apple = m_StoreExtensionProvider.GetExtension<IAppleExtensions>();
            //     apple.RestoreTransactions((result) => {
            //         onComplete?.Invoke(result);
            //         if (result) OnRestoreComplete?.Invoke();
            //     });
            // }

            // For testing
            onComplete?.Invoke(true);
            OnRestoreComplete?.Invoke();
        }

        /// <summary>
        /// Check if a product is purchased (for non-consumables)
        /// </summary>
        public static bool IsProductPurchased(string productId)
        {
            if (!_isInitialized)
            {
                Debug.LogWarning("[IVXIAPManager] Not initialized");
                return false;
            }

            // Check with IAP SDK
            // Example:
            // var product = m_StoreController.products.WithID(productId);
            // return product != null && product.hasReceipt;

            // For testing, check PlayerPrefs
            return PlayerPrefs.GetInt($"IVX_IAP_Purchased_{productId}", 0) == 1;
        }

        /// <summary>
        /// Get localized price string
        /// </summary>
        public static string GetLocalizedPrice(string productId)
        {
            var product = GetProduct(productId);
            if (product != null)
            {
                return product.price;
            }

            return "$0.00";
        }

        /// <summary>
        /// Internal: Process successful purchase
        /// </summary>
        internal static void ProcessPurchaseSuccess(string productId, string transactionId, string receipt)
        {
            Debug.Log($"[IVXIAPManager] Purchase successful: {productId}");

            var product = GetProduct(productId);
            
            // For non-consumables, mark as purchased
            if (product != null && product.type == IVXProductType.NonConsumable)
            {
                PlayerPrefs.SetInt($"IVX_IAP_Purchased_{productId}", 1);
                PlayerPrefs.Save();
            }

            var result = new IVXPurchaseResult
            {
                success = true,
                productId = productId,
                transactionId = transactionId,
                receipt = receipt
            };

            OnPurchaseComplete?.Invoke(result);
        }

        /// <summary>
        /// Internal: Process failed purchase
        /// </summary>
        internal static void ProcessPurchaseFailure(string productId, string error)
        {
            Debug.LogError($"[IVXIAPManager] Purchase failed: {productId} - {error}");

            var result = new IVXPurchaseResult
            {
                success = false,
                productId = productId,
                error = error
            };

            OnPurchaseFailed?.Invoke(error);
        }

        /// <summary>
        /// Simulate purchase for testing (remove in production)
        /// </summary>
        private static void SimulatePurchaseForTesting(string productId, Action<IVXPurchaseResult> onComplete)
        {
            Debug.Log($"[IVXIAPManager] TESTING MODE - Simulating purchase");

            // Simulate 2-second purchase flow
            var result = new IVXPurchaseResult
            {
                success = true,
                productId = productId,
                transactionId = Guid.NewGuid().ToString(),
                receipt = "test_receipt_" + DateTime.Now.Ticks
            };

            ProcessPurchaseSuccess(productId, result.transactionId, result.receipt);
            onComplete?.Invoke(result);
        }

        /// <summary>
        /// Clear all purchase data (testing only)
        /// </summary>
        public static void ClearPurchaseData()
        {
            foreach (var product in _products.Values)
            {
                PlayerPrefs.DeleteKey($"IVX_IAP_Purchased_{product.productId}");
            }
            PlayerPrefs.Save();
            Debug.Log("[IVXIAPManager] Purchase data cleared");
        }
    }

    /// <summary>
    /// Helper class for common IAP product definitions
    /// </summary>
    public static class IVXIAPProducts
    {
        // Common product ID prefixes (customize for your company)
        public const string PREFIX = "com.intelliversex.";

        /// <summary>
        /// Create a consumable product (e.g., coins)
        /// </summary>
        public static IVXProduct CreateConsumable(string gameId, string productName, string title, string description)
        {
            return new IVXProduct
            {
                productId = $"{PREFIX}{gameId}.{productName}",
                title = title,
                description = description,
                type = IVXProductType.Consumable,
                isAvailable = true
            };
        }

        /// <summary>
        /// Create a non-consumable product (e.g., remove ads)
        /// </summary>
        public static IVXProduct CreateNonConsumable(string gameId, string productName, string title, string description)
        {
            return new IVXProduct
            {
                productId = $"{PREFIX}{gameId}.{productName}",
                title = title,
                description = description,
                type = IVXProductType.NonConsumable,
                isAvailable = true
            };
        }

        /// <summary>
        /// Create a subscription product (e.g., VIP pass)
        /// </summary>
        public static IVXProduct CreateSubscription(string gameId, string productName, string title, string description)
        {
            return new IVXProduct
            {
                productId = $"{PREFIX}{gameId}.{productName}",
                title = title,
                description = description,
                type = IVXProductType.Subscription,
                isAvailable = true
            };
        }

        // Common product templates
        public static IVXProduct[] GetCommonProducts(string gameId)
        {
            return new IVXProduct[]
            {
                // Consumables (Coins)
                CreateConsumable(gameId, "coins_100", "100 Coins", "Small coin pack"),
                CreateConsumable(gameId, "coins_500", "500 Coins", "Medium coin pack"),
                CreateConsumable(gameId, "coins_1000", "1000 Coins", "Large coin pack"),
                CreateConsumable(gameId, "coins_5000", "5000 Coins", "Mega coin pack"),

                // Non-Consumables
                CreateNonConsumable(gameId, "remove_ads", "Remove Ads", "Remove all advertisements"),
                CreateNonConsumable(gameId, "unlock_all", "Unlock All", "Unlock all content"),

                // Subscription
                CreateSubscription(gameId, "vip_monthly", "VIP Monthly", "Monthly VIP membership"),
                CreateSubscription(gameId, "vip_yearly", "VIP Yearly", "Yearly VIP membership (save 20%)")
            };
        }
    }
}
