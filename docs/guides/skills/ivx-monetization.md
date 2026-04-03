# Monetization

**Skill ID:** `ivx-monetization`

---
name: ivx-monetization
description: >-
  Configure ads, in-app purchases, offerwalls, and monetization strategy for
  IntelliVerseX SDK games. Use when the user says "monetize my game", "add ads",
  "set up offerwall", "add IAP", "wire rewarded ads", "set up LevelPlay",
  "configure AdMob", "Appodeal setup", "add season pass", "configure interstitials",
  "banner ads", "rewarded video", "Pubscale", "Xsolla offerwall", "ad unit IDs",
  or needs help with any revenue integration.
version: "1.0.0"
author: "IntelliVerse-X <team@intelli-verse-x.ai>"
allowed-tools:
  - Read
  - Write
  - Edit
  - Glob
  - Grep
  - Shell
---



## Overview

The SDK supports three monetization pillars: **Ads**, **In-App Purchases (IAP)**, and **Offerwalls**. Each has a dedicated config ScriptableObject, manager, and server-side validation path.

---

## When to Use

Ask your AI agent any of these:

- "Monetize my casual puzzle game"
- "Set up LevelPlay ads with rewarded video"
- "Add Pubscale offerwall and wire it to the wallet"
- "Configure IAP for a coins pack"
- "Add server-side reward validation for rewarded ads"
- "Set up AdMob banner and interstitial ads"
- "Configure ad waterfall priority"

---

## What the Agent Does

```mermaid
flowchart TD
    A[You: "Monetize my game"] --> B[Agent loads ivx-monetization skill]
    B --> C{What type of game?}
    C -->|Hypercasual| D[Interstitials + Rewarded]
    C -->|Casual| E[Rewarded + IAP + Banner]
    C -->|Midcore| F[IAP + Season Pass]
    C -->|Hardcore| G[Subscription + IAP]
    D --> H[Configure ad provider]
    E --> H
    F --> I[Configure IAP products]
    G --> I
    H --> J[Set up IVXAdsConfig]
    I --> K[Set up IVXIAPConfig]
    J --> L[Wire reward callbacks]
    K --> L
    L --> M[Enable server validation]
```

---



## 1. Ad Providers

### Supported Networks

| Provider | Banner | Interstitial | Rewarded Video | Offerwall |
|----------|--------|-------------|---------------|-----------|
| **LevelPlay (ironSource)** | Yes | Yes | Yes | Yes |
| **Appodeal** | Yes | Yes | Yes | Yes |
| **AdMob** | Yes | Yes | Yes | No |

### Per-Provider Setup

#### LevelPlay (ironSource)

1. Create an account at [is.com](https://platform.ironsrc.com/).
2. Register your app and note the **App Key**.
3. Import the LevelPlay Unity SDK via their integration manager.
4. In `IVXAdsConfig`, set `Provider = LevelPlay` and enter the App Key.

#### Appodeal

1. Register at [appodeal.com](https://app.appodeal.com/).
2. Create your app and note the **App Key**.
3. Import the Appodeal SDK via their Unity plugin.
4. In `IVXAdsConfig`, set `Provider = Appodeal` and enter the App Key.

#### AdMob

1. Register at [admob.google.com](https://admob.google.com/).
2. Create ad units for each format and note the **Ad Unit IDs**.
3. Import GoogleMobileAds via UPM or `.unitypackage`.
4. In `IVXAdsConfig`, set `Provider = AdMob` and enter the per-format IDs.

---

## 2. Ad Unit Configuration

### IVXAdsConfig ScriptableObject

Create via **Create > IntelliVerseX > Ads Configuration**.

| Field | Description |
|-------|-------------|
| `Provider` | `LevelPlay`, `Appodeal`, or `AdMob` |
| `AppKey` | Provider app key (LevelPlay/Appodeal) |
| `BannerAdUnitId` | Ad unit ID for banners |
| `InterstitialAdUnitId` | Ad unit ID for interstitials |
| `RewardedVideoAdUnitId` | Ad unit ID for rewarded videos |
| `UseTestAds` | Enable test mode (always true during development) |

### Test IDs

Use these during development to avoid invalid traffic flags:

| Provider | Format | Test ID |
|----------|--------|---------|
| **LevelPlay** | All | Use `demoapp` as App Key |
| **Appodeal** | All | Use test App Key from dashboard |
| **AdMob** | Banner | `ca-app-pub-3940256099942544/6300978111` |
| **AdMob** | Interstitial | `ca-app-pub-3940256099942544/1033173712` |
| **AdMob** | Rewarded | `ca-app-pub-3940256099942544/5224354917` |

### Waterfall & Failover

`IVXAdsWaterfallManager` manages failover between providers:

```csharp
// Priority order — falls through on load failure
waterfallManager.SetPriority(new[] {
    AdProvider.LevelPlay,
    AdProvider.Appodeal,
    AdProvider.AdMob,
});
```

If the primary provider fails to fill, the next provider in the waterfall is tried automatically.

---

## 3. Showing Ads

```csharp
using IntelliVerseX.Monetization;

// Banner
IVXAdsManager.Instance.ShowBanner(BannerPosition.Bottom);
IVXAdsManager.Instance.HideBanner();

// Interstitial
if (IVXAdsManager.Instance.IsInterstitialReady())
{
    IVXAdsManager.Instance.ShowInterstitial(
        onClosed: () => Debug.Log("Interstitial closed"),
        onFailed: (err) => Debug.LogWarning($"Interstitial failed: {err}")
    );
}

// Rewarded Video
if (IVXAdsManager.Instance.IsRewardedReady())
{
    IVXAdsManager.Instance.ShowRewarded(
        onRewarded: (reward) => GrantReward(reward),
        onClosed: () => Debug.Log("Rewarded closed"),
        onFailed: (err) => Debug.LogWarning($"Rewarded failed: {err}")
    );
}
```

### Rewarded Ads Server Validation

For high-value rewards, validate server-side via the `rewarded_ads` Nakama RPC:

```csharp
var result = await IVXAdsManager.Instance.ShowRewardedWithServerValidation(
    placement: "double_coins",
    userId: currentUserId
);
if (result.Validated) GrantReward(result.Reward);
```

The server verifies the callback from the ad network before granting currency.

---

## 4. Offerwall Setup

### IVXOfferwallConfig ScriptableObject

Create via **Create > IntelliVerseX > Offerwall Configuration**.

| Field | Description |
|-------|-------------|
| `EnablePubscale` | Toggle Pubscale offerwall |
| `PubscaleAppId` | Pubscale App ID from dashboard |
| `PubscaleSecretKey` | Pubscale Secret Key for server callbacks |
| `EnableXsolla` | Toggle Xsolla offerwall |
| `XsollaProjectId` | Xsolla project ID |
| `XsollaMerchantId` | Xsolla merchant ID |

### Showing the Offerwall

```csharp
using IntelliVerseX.Monetization;

IVXOfferwallManager.Instance.ShowOfferwall(OfferwallProvider.Pubscale);

IVXOfferwallManager.Instance.OnOfferCompleted += async (offer) =>
{
    await IVXWalletManager.Instance.GrantCurrencyAsync(
        "coins", offer.RewardAmount
    );
};
```

---

## 5. In-App Purchases (IAP)

### IVXIAPConfig ScriptableObject

Create via **Create > IntelliVerseX > IAP Configuration**.

Define products:

| Field | Description |
|-------|-------------|
| `ProductId` | Internal identifier (e.g. `gems_100`) |
| `ProductType` | `Consumable`, `NonConsumable`, `Subscription` |
| `AppleProductId` | App Store product ID |
| `GoogleProductId` | Google Play product ID |
| `PriceUSD` | Display price for UI (actual price from store) |

### Purchase Flow

```csharp
using IntelliVerseX.Monetization;

var result = await IVXIAPManager.Instance.PurchaseAsync("gems_100");

switch (result.Status)
{
    case PurchaseStatus.Success:
        // Server validated receipt — gems already granted
        RefreshUI();
        break;
    case PurchaseStatus.Cancelled:
        Debug.Log("Purchase cancelled by user");
        break;
    case PurchaseStatus.Failed:
        Debug.LogWarning($"Purchase failed: {result.Error}");
        break;
}
```

**Flow:** Client purchase -> receipt sent to Nakama RPC `iap_validate` -> server validates with Apple/Google -> server grants items -> client receives confirmation.

---

## 6. Monetization Strategy by Genre

| Genre | Primary Revenue | Secondary | Ad Frequency |
|-------|----------------|-----------|-------------|
| **Hypercasual** | Interstitials + Rewarded | — | Every 2-3 levels |
| **Casual** | Rewarded + IAP | Banner | Every 3-5 minutes |
| **Midcore** | IAP + Season Pass | Rewarded (opt-in) | Rare, opt-in only |
| **Hardcore** | Subscription + IAP | — | Never |

### Recommended Defaults

- Always use **rewarded video** for premium currency shortcuts.
- Place **interstitials** at natural break points (level end, menu transition) — never mid-gameplay.
- **Banners** only in menus or non-gameplay screens.
- Validate all high-value rewards server-side.

---

## Platform-Specific Monetization

### VR Platforms (Meta Quest / SteamVR / Vision Pro)

| Platform | Ad Support | IAP Store | Offerwall |
|----------|-----------|-----------|-----------|
| **Meta Quest** | No traditional ads — Meta prohibits interstitials/banners in VR | Meta Quest Store IAP | Not supported |
| **SteamVR** | No ads | Steam IAP / Steamworks | Not supported |
| **Apple Vision Pro** | No ads | App Store IAP (standard Apple flow) | Not supported |

**VR monetization strategy:** Focus on IAP and premium pricing. Rewarded mechanics can use in-game reward systems (daily login, challenges) instead of ad-based rewards. Season passes and cosmetic DLC work well in VR.

### Console Platforms (PS5 / Xbox / Switch)

| Platform | Ad Support | IAP Store | Certification |
|----------|-----------|-----------|---------------|
| **PlayStation 5** | No ads | PlayStation Store (`SceNpCommerce2`) | TRC compliance required |
| **Xbox Series** | No ads | Microsoft Store (`Windows.Services.Store`) | XR compliance required |
| **Nintendo Switch** | No ads | Nintendo eShop | Lotcheck certification |

**Console IAP:** Each console has its own proprietary store API (NDA-protected). The `IVXIAPManager` delegates to the platform's native purchasing API through platform-specific build targets. Product IDs must be registered in each console's developer portal separately.

**Console certification:** All monetization must pass platform certification (Sony TRC, Microsoft XR, Nintendo Lotcheck). Key requirements: clear pricing display, purchase confirmation dialogs, restore purchases functionality, and parental controls.

### WebGL / Browser

| Feature | Support | Notes |
|---------|---------|-------|
| Banner ads | Yes | Via AdSense `.jslib` plugin |
| Interstitial | Yes | Via Applixir or custom `.jslib` |
| Rewarded video | Limited | Via Applixir video ads |
| IAP | No native | Use web payment APIs or Stripe |
| Offerwall | No | Not applicable to browser |

**WebGL monetization:** Traditional mobile ad SDKs (LevelPlay, Appodeal, AdMob) do not work in WebGL. Use browser-native ad solutions. For IAP, integrate web payment processors directly.

---

## Checklist

- [ ] Ad provider SDK imported and configured
- [ ] `IVXAdsConfig` created with correct IDs (test IDs for dev builds)
- [ ] `UseTestAds` enabled during development
- [ ] Rewarded ads grant rewards via server validation
- [ ] Offerwall config set up if using Pubscale/Xsolla
- [ ] IAP products defined with matching store product IDs
- [ ] Purchase flow tested end-to-end with sandbox accounts
- [ ] Ad waterfall priority configured
- [ ] VR store policies reviewed if targeting Quest/Steam/Vision Pro
- [ ] Console store products registered if targeting PS5/Xbox/Switch
- [ ] WebGL ad solution configured if targeting browser
