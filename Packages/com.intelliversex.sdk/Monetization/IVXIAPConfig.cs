using System;
using System.Collections.Generic;
using UnityEngine;

namespace IntelliVerseX.Monetization
{
    /// <summary>
    /// In-App Purchase product definition
    /// Supports: Consumable, Non-Consumable, Subscription
    /// </summary>
    [Serializable]
    public class IVXIAPProduct
    {
        [Tooltip("Unique identifier for this product (used in code)")]
        public string productId;
        
        [Tooltip("Human-readable product name")]
        public string productName;
        
        [Tooltip("Product type: Consumable (coins), Non-Consumable (no-ads), Subscription (premium)")]
        public IVXProductType productType;
        
        [Tooltip("iOS App Store product ID")]
        public string appleProductId;
        
        [Tooltip("Google Play Store product ID")]
        public string googleProductId;
        
        [Tooltip("Default price (USD) for display purposes")]
        public float defaultPrice = 0.99f;
        
        [Tooltip("Currency code (USD, EUR, etc.)")]
        public string currencyCode = "USD";
        
        [Tooltip("Product description")]
        [TextArea(2, 4)]
        public string description = "";
        
        [Tooltip("Reward amount (for consumables like coins)")]
        public int rewardAmount = 0;
        
        [Tooltip("Is this product currently enabled?")]
        public bool isEnabled = true;
        
        /// <summary>
        /// Get platform-specific product ID
        /// </summary>
        public string GetPlatformProductId()
        {
            #if UNITY_IOS
            return appleProductId;
            #elif UNITY_ANDROID
            return googleProductId;
            #else
            return productId;
            #endif
        }
    }
    
    /// <summary>
    /// Product type enumeration
    /// </summary>
    public enum IVXProductType
    {
        Consumable,        // Coins, hints, etc. (can be purchased multiple times)
        NonConsumable,     // No-ads, unlock features (purchased once)
        Subscription       // Monthly/yearly premium access
    }
    
    /// <summary>
    /// IAP configuration - config-driven approach like ads
    /// Create in: Assets → Create → IntelliVerse-X → IAP Configuration
    /// </summary>
    [CreateAssetMenu(fileName = "IVXIAPConfig", menuName = "IntelliVerse-X/IAP Configuration", order = 3)]
    public class IVXIAPConfig : ScriptableObject
    {
        [Header("IAP Settings")]
        [Tooltip("Enable IAP system")]
        public bool enableIAP = true;
        
        [Tooltip("Enable test purchases (sandbox mode)")]
        public bool testMode = false;
        
        [Tooltip("Auto-restore purchases on startup (iOS)")]
        public bool autoRestorePurchases = true;
        
        [Header("Platform Configuration")]
        [Tooltip("Apple App Store enabled")]
        public bool enableAppleStore = true;
        
        [Tooltip("Google Play Store enabled")]
        public bool enableGooglePlay = true;
        
        [Header("Product Catalog")]
        [Tooltip("All available products")]
        public List<IVXIAPProduct> products = new List<IVXIAPProduct>();
        
        [Header("Receipt Validation")]
        [Tooltip("Validate receipts with server")]
        public bool enableReceiptValidation = false;
        
        [Tooltip("Receipt validation endpoint")]
        public string validationEndpoint = "https://api.intelli-verse-x.ai/iap/validate";
        
        [Header("Security")]
        [Tooltip("Obfuscate account secrets (Google Play)")]
        public bool obfuscateSecrets = true;
        
        [Tooltip("Google Play License Key (base64)")]
        [TextArea(2, 4)]
        public string googlePlayLicenseKey = "";
        
        /// <summary>
        /// Get product by ID
        /// </summary>
        public IVXIAPProduct GetProduct(string productId)
        {
            return products.Find(p => p.productId == productId);
        }
        
        /// <summary>
        /// Get all products of specific type
        /// </summary>
        public List<IVXIAPProduct> GetProductsByType(IVXProductType type)
        {
            return products.FindAll(p => p.productType == type && p.isEnabled);
        }
        
        /// <summary>
        /// Get all enabled products
        /// </summary>
        public List<IVXIAPProduct> GetEnabledProducts()
        {
            return products.FindAll(p => p.isEnabled);
        }
        
        /// <summary>
        /// Validate configuration
        /// </summary>
        public bool IsValid()
        {
            if (!enableIAP)
            {
                Debug.LogWarning("[IVXIAPConfig] IAP is disabled");
                return false;
            }
            
            if (products == null || products.Count == 0)
            {
                Debug.LogError("[IVXIAPConfig] No products configured!");
                return false;
            }
            
            // Check for duplicate product IDs
            HashSet<string> productIds = new HashSet<string>();
            foreach (var product in products)
            {
                if (string.IsNullOrEmpty(product.productId))
                {
                    Debug.LogError("[IVXIAPConfig] Product has empty ID!");
                    return false;
                }
                
                if (productIds.Contains(product.productId))
                {
                    Debug.LogError($"[IVXIAPConfig] Duplicate product ID: {product.productId}");
                    return false;
                }
                
                productIds.Add(product.productId);
                
                #if UNITY_IOS
                if (string.IsNullOrEmpty(product.appleProductId))
                {
                    Debug.LogWarning($"[IVXIAPConfig] Product {product.productId} missing Apple ID");
                }
                #elif UNITY_ANDROID
                if (string.IsNullOrEmpty(product.googleProductId))
                {
                    Debug.LogWarning($"[IVXIAPConfig] Product {product.productId} missing Google ID");
                }
                #endif
            }
            
            return true;
        }
    }
}
