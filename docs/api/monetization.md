# Monetization API Reference

Complete API reference for the Monetization module.

---

## IVXAdsManager

Advertising manager for banners, interstitials, and rewarded videos.

### Properties

| Property | Type | Description |
|----------|------|-------------|
| `Instance` | `IVXAdsManager` | Singleton instance |
| `IsInitialized` | `bool` | Initialization status |
| `ActiveProvider` | `AdProvider` | Current ad provider |

---

### Banner Methods

#### ShowBanner

```csharp
public void ShowBanner(BannerPosition position = BannerPosition.Bottom)
```

Shows a banner ad.

---

#### HideBanner

```csharp
public void HideBanner()
```

Hides the current banner.

---

### Interstitial Methods

#### LoadInterstitial

```csharp
public void LoadInterstitial()
```

Preloads an interstitial ad.

---

#### IsInterstitialReady

```csharp
public bool IsInterstitialReady()
```

Checks if interstitial is loaded.

---

#### ShowInterstitial

```csharp
public void ShowInterstitial()
```

Shows the loaded interstitial.

---

### Rewarded Video Methods

#### LoadRewardedVideo

```csharp
public void LoadRewardedVideo()
```

Preloads a rewarded video.

---

#### IsRewardedVideoReady

```csharp
public bool IsRewardedVideoReady()
```

Checks if rewarded video is loaded.

---

#### ShowRewardedVideo

```csharp
public void ShowRewardedVideo()
```

Shows the rewarded video.

---

### Privacy Methods

#### SetGDPRConsent

```csharp
public void SetGDPRConsent(bool hasConsent)
```

Sets GDPR consent status.

---

#### SetCCPAConsent

```csharp
public void SetCCPAConsent(bool hasConsent)
```

Sets CCPA consent status.

---

### Events

| Event | Signature | Description |
|-------|-----------|-------------|
| `OnBannerLoaded` | `Action` | Banner loaded |
| `OnBannerFailed` | `Action<string>` | Banner failed |
| `OnBannerClicked` | `Action` | Banner clicked |
| `OnInterstitialLoaded` | `Action` | Interstitial ready |
| `OnInterstitialFailed` | `Action<string>` | Interstitial failed |
| `OnInterstitialShown` | `Action` | Interstitial displayed |
| `OnInterstitialClosed` | `Action` | Interstitial closed |
| `OnRewardedVideoLoaded` | `Action` | Rewarded ready |
| `OnRewardedVideoFailed` | `Action<string>` | Rewarded failed |
| `OnRewardedVideoCompleted` | `Action<RewardInfo>` | Reward earned |
| `OnRewardedVideoClosed` | `Action<bool>` | Rewarded closed |
| `OnAdRevenue` | `Action<AdRevenueData>` | Revenue event |

---

## IVXIAPManager

In-app purchase manager.

### Properties

| Property | Type | Description |
|----------|------|-------------|
| `Instance` | `IVXIAPManager` | Singleton instance |
| `Products` | `List<IAPProduct>` | Available products |
| `IsInitialized` | `bool` | Store initialized |

---

### Methods

#### GetProductsAsync

```csharp
public async Task<List<IAPProduct>> GetProductsAsync()
```

Gets available products from the store.

---

#### PurchaseAsync

```csharp
public async Task<PurchaseResult> PurchaseAsync(string productId)
```

Initiates a purchase.

**Returns:** Purchase result with receipt

---

#### RestorePurchasesAsync

```csharp
public async Task<List<PurchaseResult>> RestorePurchasesAsync()
```

Restores previous purchases (iOS).

---

### Events

| Event | Signature | Description |
|-------|-----------|-------------|
| `OnProductsLoaded` | `Action<List<IAPProduct>>` | Products loaded |
| `OnPurchaseComplete` | `Action<PurchaseResult>` | Purchase succeeded |
| `OnPurchaseFailed` | `Action<string, string>` | Purchase failed |
| `OnPurchaseRestored` | `Action<PurchaseResult>` | Purchase restored |

---

## Data Classes

### BannerPosition

```csharp
public enum BannerPosition
{
    Top,
    Bottom,
    TopLeft,
    TopRight,
    BottomLeft,
    BottomRight,
    Center
}
```

### RewardInfo

| Property | Type | Description |
|----------|------|-------------|
| `Amount` | `int` | Reward amount |
| `Currency` | `string` | Reward type |

### IAPProduct

| Property | Type | Description |
|----------|------|-------------|
| `ProductId` | `string` | Store product ID |
| `Title` | `string` | Product title |
| `Description` | `string` | Product description |
| `Price` | `string` | Formatted price |
| `PriceValue` | `decimal` | Numeric price |
| `CurrencyCode` | `string` | Currency code |
| `Type` | `ProductType` | Consumable/Non-consumable/Subscription |

---

## See Also

- [Monetization Module Guide](../modules/monetization.md)
- [Ad Integration Guide](../guides/ad-integration.md)
- [Ads Configuration](../configuration/ads-config.md)
