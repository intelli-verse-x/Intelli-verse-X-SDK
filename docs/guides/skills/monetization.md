# Skill: Monetization

**Skill ID:** `ivx-monetization`

Configures all revenue streams for your game -- ads (LevelPlay, Appodeal, AdMob), in-app purchases (Apple/Google), offerwalls (Pubscale, Xsolla), and server-side reward validation.

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

## Revenue Streams

### 1. Ads

**Supported networks:**

| Provider | Banner | Interstitial | Rewarded | Offerwall |
|----------|--------|-------------|----------|-----------|
| LevelPlay (ironSource) | Yes | Yes | Yes | Yes |
| Appodeal | Yes | Yes | Yes | Yes |
| AdMob | Yes | Yes | Yes | No |

**Configuration:** The agent creates an `IVXAdsConfig` ScriptableObject with your provider, app key, and ad unit IDs.

**Showing ads:**

```csharp
// Banner
IVXAdsManager.Instance.ShowBanner(BannerPosition.Bottom);

// Interstitial (at natural break points)
if (IVXAdsManager.Instance.IsInterstitialReady())
    IVXAdsManager.Instance.ShowInterstitial(onClosed: () => ContinueGame());

// Rewarded video (for premium currency)
if (IVXAdsManager.Instance.IsRewardedReady())
    IVXAdsManager.Instance.ShowRewarded(onRewarded: (r) => GrantReward(r));
```

**Waterfall failover:** The agent configures `IVXAdsWaterfallManager` to fall through providers on load failure:

```csharp
waterfallManager.SetPriority(new[] {
    AdProvider.LevelPlay, AdProvider.Appodeal, AdProvider.AdMob
});
```

### 2. In-App Purchases

**Configuration:** `IVXIAPConfig` ScriptableObject with product definitions:

| Field | Example |
|-------|---------|
| ProductId | `gems_100` |
| ProductType | `Consumable` |
| AppleProductId | `com.mygame.gems100` |
| GoogleProductId | `gems_100` |

**Purchase flow:**

```csharp
var result = await IVXIAPManager.Instance.PurchaseAsync("gems_100");
if (result.Status == PurchaseStatus.Success) RefreshUI();
```

Client purchase -> Nakama `iap_validate` RPC -> Server validates with Apple/Google -> Server grants items -> Client confirmation.

### 3. Offerwalls

**Supported providers:** Pubscale, Xsolla

**Configuration:** `IVXOfferwallConfig` ScriptableObject with app ID and secret key.

```csharp
IVXOfferwallManager.Instance.ShowOfferwall(OfferwallProvider.Pubscale);
IVXOfferwallManager.Instance.OnOfferCompleted += async (offer) =>
{
    await IVXWalletManager.Instance.GrantCurrencyAsync("coins", offer.RewardAmount);
};
```

### 4. Server-Side Reward Validation

For high-value rewards, the agent wires Nakama's `rewarded_ads` RPC to prevent client-side cheating:

```csharp
var result = await IVXAdsManager.Instance.ShowRewardedWithServerValidation(
    placement: "double_coins", userId: currentUserId
);
if (result.Validated) GrantReward(result.Reward);
```

---

## Genre Strategy Reference

| Genre | Primary Revenue | Secondary | Ad Frequency |
|-------|----------------|-----------|-------------|
| Hypercasual | Interstitials + Rewarded | -- | Every 2-3 levels |
| Casual | Rewarded + IAP | Banner | Every 3-5 minutes |
| Midcore | IAP + Season Pass | Rewarded (opt-in) | Rare, opt-in only |
| Hardcore | Subscription + IAP | -- | Never |

---

## Test Ad IDs

| Provider | Format | Test ID |
|----------|--------|---------|
| LevelPlay | All | Use `demoapp` as App Key |
| Appodeal | All | Use test App Key from dashboard |
| AdMob | Banner | `ca-app-pub-3940256099942544/6300978111` |
| AdMob | Interstitial | `ca-app-pub-3940256099942544/1033173712` |
| AdMob | Rewarded | `ca-app-pub-3940256099942544/5224354917` |

---

## Completion Checklist

- [ ] Ad provider SDK imported and configured
- [ ] `IVXAdsConfig` created with correct IDs (test IDs for dev)
- [ ] `UseTestAds` enabled during development
- [ ] Rewarded ads grant rewards via server validation
- [ ] Offerwall config set up if using Pubscale/Xsolla
- [ ] IAP products defined with matching store product IDs
- [ ] Purchase flow tested end-to-end with sandbox accounts
- [ ] Ad waterfall priority configured
