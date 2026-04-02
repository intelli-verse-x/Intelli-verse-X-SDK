# Privacy & Compliance Guide

This guide summarizes how an IntelliVerseX integration should treat **user data**, **regulations (GDPR, COPPA, regional ads)**, and **SDK hooks** for consent, export, deletion, and ad privacy.

---

## Overview

IntelliVerseX spans **identity**, **backend (Nakama)**, **analytics (e.g. Satori)**, **monetization (ads, IAP)**, **social**, and **local storage**. Your obligations depend on:

- **Where users live** (EEA/UK, US state laws, other regions).
- **Audience** (general vs child-directed).
- **What you collect** beyond the SDK (your own analytics, crash reporters, support tools).

The SDK provides **building blocks** (storage export/delete helpers, ads consent configuration, UMP-related APIs); **legal text, DPIAs, and policy hosting** remain your responsibility.

!!! danger "Not legal advice"
    This document is **technical guidance** only. Engage qualified counsel for GDPR, COPPA, COPPA Rule, and regional ad law compliance.

---

## GDPR compliance

### Consent collection

For EEA/UK users, obtain **appropriate consent** before non-essential processing (personalized ads, optional analytics, some social features). Pattern:

1. Show a **first-run consent UI** (your UI).
2. Persist consent with `PlayerPrefs`, encrypted storage, or your account system.
3. Initialize or enable SDK modules **only after** consent where required.

```csharp
public static class ConsentStore
{
    private const string AnalyticsKey = "consent_analytics_v1";
    private const string AdsPersonalizationKey = "consent_ads_personalization_v1";

    public static bool AnalyticsAllowed => PlayerPrefs.GetInt(AnalyticsKey, 0) == 1;
    public static bool AdsPersonalizationAllowed => PlayerPrefs.GetInt(AdsPersonalizationKey, 0) == 1;

    public static void SetAnalyticsAllowed(bool allowed)
    {
        PlayerPrefs.SetInt(AnalyticsKey, allowed ? 1 : 0);
        PlayerPrefs.Save();
    }
}
```

### Data access requests (export)

`IVXPrivacyManager.ExportAllData()` returns a **JSON snapshot** including an export timestamp, device identifier, and a structured payload. **Unity does not enumerate all PlayerPrefs keys** — maintain your own key list if you need a complete export.

```csharp
using IntelliVerseX.Storage;
using UnityEngine;

public void OnUserRequestedDataExport()
{
    string json = IVXPrivacyManager.ExportAllData();
    // Deliver securely (email link, authenticated download, support ticket).
    Debug.Log($"[Privacy] Export length: {json?.Length ?? 0}");
}
```

### Data deletion

`IVXPrivacyManager.DeleteAllData()` clears secure storage and attempts to remove files under **`Application.persistentDataPath`**. You must still:

- **Delete server-side** Nakama / Hiro / custom service records (RPCs or admin tools).
- **Revoke refresh tokens** where your identity provider supports it.
- **Re-request consent** on next launch if the user re-onboards.

```csharp
using IntelliVerseX.Storage;

public void OnUserConfirmedErasure()
{
    IVXPrivacyManager.DeleteAllData();
    // Invoke your backend user deletion and sign-out flow here.
}
```

!!! note "Backend deletion"
    Local deletion **does not** remove leaderboard entries or wallet balances on the server unless **you** invoke the appropriate APIs or ops processes.

---

## COPPA compliance

For **child-directed** games (or mixed audience with under-13 users in the US):

- **Disable personalized ads** and behavior that targets children; use **`IVXAdsConfig.enableCOPPA`** and platform child flags as required by your mediation stack.
- **Minimize data collection** — avoid optional social graph, precise location, and profiling for kids’ accounts.
- **Parental gate** purchasing and external links.

```csharp
// Illustrative: gate an optional social feature
public bool CanOpenSocialHub(int ageYears) => ageYears >= 13 && ConsentStore.AnalyticsAllowed;
```

Align in-app behavior with your **store declarations** (Google Play “Designed for Families”, Apple Kids Category, etc.).

---

## Data collection inventory (by concern)

| Concern | Typical data | Notes |
|--------|----------------|-------|
| **Analytics / live-ops** | Events, session metadata, A/B assignments | Gate behind consent; configure Satori/Hiro per environment. |
| **Identity** | Account IDs, device IDs, auth tokens | Needed for online play; document retention in your policy. |
| **Wallet / economy** | Balances, transaction history | Often PII-linked; treat as sensitive. |
| **Social / friends** | Friend lists, display names | High sensitivity; offer unfriend/block and export coverage. |
| **Ads** | Ad IDs, consent strings, mediation metadata | Use UMP / platform ATT as applicable; see below. |

Refresh this table when you add new SDK modules or third-party SDKs.

---

## Consent flow implementation

1. **Block initialization** of analytics and personalized ads until consent is known.
2. **`IVXAdsConfig`** exposes **`enableGDPRConsent`**, **`enableCCPA`**, and **`enableCOPPA`** — align with your legal review.
3. After consent changes, **re-initialize** or **update** the ads consent state per your mediation provider’s docs.

```csharp
// Pseudocode: order of operations
// 1. Show consent UI
// 2. Persist choices
// 3. Initialize ads only if policy allows
// 4. Initialize analytics only if ConsentStore.AnalyticsAllowed
```

Use `IVXAdsManager.ShowPrivacyOptionsForm` when the user opens **“Privacy options”** in settings (UMP-dependent; see ads integration).

---

## Data deletion (full stack)

| Layer | Action |
|-------|--------|
| **Device** | `IVXPrivacyManager.DeleteAllData()`, clear your own `PlayerPrefs` keys, remove cached files you created. |
| **Nakama / backend** | Delete user object, storage, leaderboard opt-out per your design. |
| **Tokens** | Clear session on device; revoke server-side sessions if supported. |
| **Third parties** | Crash, analytics, ad partners — use their dashboards or APIs where available. |

---

## Ad privacy

### IDFA / GAID and limited tracking

- **iOS**: Respect **App Tracking Transparency (ATT)** — request authorization before accessing IDFA for tracking; honor “deny”.
- **Android**: Respect **Advertising ID** limitations and **UMP** outcomes for EEA users.

### SDK helpers

`IVXAdsManager` includes **privacy options** helpers such as **`ShowPrivacyOptionsForm`** and **`IsPrivacyOptionsRequired`** (Google Mobile Ads UMP when the define assemblies are present).

Configure test device IDs in **`IVXAdsConfig.umpTestDeviceIds`** during consent debugging.

!!! tip "Settings screen"
    Surface **“Ad choices”** / **“Privacy options”** next to your privacy policy link so users can revisit consent.

---

## Best practices

- **Collect the minimum** necessary for the feature; default off optional profiling.
- **Retention** — define how long logs, backups, and analytics exports are kept; document in your policy.
- **Privacy policy** — link from store listing and first run; name controllers, purposes, and legal bases (GDPR).
- **Data processing agreements** — sign DPAs with processors (hosting, analytics, ads).
- **Children** — if there is any doubt, assume **strict** COPPA posture until counsel confirms.

---

## See also

- [Analytics module](../modules/analytics.md)
- [Ads configuration](../configuration/ads-config.md)
