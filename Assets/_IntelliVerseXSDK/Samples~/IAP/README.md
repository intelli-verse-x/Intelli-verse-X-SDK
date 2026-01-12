# IAP Integration Sample

This sample demonstrates how to implement in-app purchases using the IntelliVerseX SDK.

## Features Demonstrated

- Product configuration
- Purchase flow
- Receipt validation
- Subscription handling
- Restore purchases

## Setup

1. Import this sample via Package Manager
2. Open the `IAPDemoScene` scene
3. Configure your store credentials:
   - Google Play: Add your license key
   - Apple App Store: Configure in App Store Connect
4. Define your products in `IVXIAPConfig`
5. Press Play

## Key Scripts

### Product Configuration
```csharp
using IntelliVerseX.Monetization;

// Products are configured in IVXIAPConfig ScriptableObject
// Create via: Assets > Create > IntelliVerseX > IAP Config
```

### Making Purchases
```csharp
using IntelliVerseX.Monetization;

// Initialize IAP
await IVXIAPManager.Instance.InitializeAsync();

// Purchase a product
var result = await IVXIAPManager.Instance.PurchaseAsync("com.game.coins_100");

if (result.Success)
{
    // Grant the purchased item
    Debug.Log($"Purchased: {result.ProductId}");
}
else
{
    Debug.LogError($"Purchase failed: {result.Error}");
}
```

### Subscriptions
```csharp
// Check subscription status
bool isSubscribed = IVXIAPManager.Instance.IsSubscribed("com.game.premium");

// Get subscription info
var info = IVXIAPManager.Instance.GetSubscriptionInfo("com.game.premium");
Debug.Log($"Expires: {info.ExpirationDate}");
```

### Restore Purchases
```csharp
// Restore previous purchases (required for iOS)
var restored = await IVXIAPManager.Instance.RestorePurchasesAsync();
Debug.Log($"Restored {restored.Count} purchases");
```

## Product Types

| Type | Description | Example |
|------|-------------|---------|
| Consumable | Can be purchased multiple times | Coins, gems |
| Non-Consumable | One-time purchase | Remove ads, unlock level |
| Subscription | Recurring payment | Premium membership |

## Dependencies

- IntelliVerseX.Core
- IntelliVerseX.Monetization
- Unity IAP (com.unity.purchasing)

## Platform Setup

### Google Play
1. Set up your app in Google Play Console
2. Create in-app products
3. Add license key to `IVXIAPConfig`

### Apple App Store
1. Set up your app in App Store Connect
2. Create in-app purchases
3. Configure Shared Secret for receipt validation

## Security

- Always validate receipts server-side for real money purchases
- The SDK provides client-side validation helpers
- For production, implement server-side validation via `IVXBackendService`
