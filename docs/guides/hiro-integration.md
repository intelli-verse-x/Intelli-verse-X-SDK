# Hiro Live-Ops Integration

This guide walks through integrating **Hiro**—the IntelliVerseX server-authoritative metagame layer—after Nakama authentication. It follows the APIs and patterns documented in the [Hiro module](../modules/hiro.md).

---

## Overview

Hiro exposes **33 systems** through `IVXHiroCoordinator`. Together they cover live-ops–style gameplay and monetization without trusting the client for outcomes:

| Area | What Hiro provides |
|------|-------------------|
| **Economy** | Server-authoritative currency flows, donations, rewarded video credits (`IVXEconomySystem`). |
| **Energy** | Time-gated energy with regeneration timers (`IVXEnergySystem`). |
| **Inventory** | Items with categories, stacking, and expiry (`IVXInventorySystem`). |
| **Achievements** | Hierarchical achievements and sub-achievements (`IVXAchievementsSystem`). |
| **Event leaderboards** | Time-limited competitive events with tiers (`IVXEventLeaderboardSystem`). |
| **Spin wheel** | Weighted wheel with free, ad-gated, and currency spins (`IVXSpinWheelSystem`). |
| **Streaks** | Daily streaks and milestone rewards (`IVXStreaksSystem`). |
| **Offerwall** | Third-party offer completion and server-side validation (`IVXOfferwallSystem`). |
| **Retention** | Session depth, churn signals, onboarding, comeback bonuses (`IVXRetentionSystem`). |
| **Friend quests & battles** | Cooperative quests, friend streaks, and async 1v1 battles (`IVXFriendQuestSystem`, `IVXFriendStreakSystem`, `IVXFriendBattleSystem`). |
| **IAP triggers** | Context-aware offer evaluation and conversion tracking (`IVXIAPTriggerSystem`). |
| **Smart ad timers** | Interstitial cooldowns, rewarded caps, banner eligibility (`IVXSmartAdTimerSystem`). |

All RPC traffic goes through the shared `IVXHiroRpcClient` (see [RPC Reference](../modules/hiro.md#rpc-reference) in the module doc).

!!! tip "Authoritative server"
    Never treat client-side calculations as final for rewards, spin results, streak counts, or leaderboard placement. The Nakama Hiro RPCs are the source of truth.

---

## Prerequisites

Before calling any Hiro system:

1. **Nakama backend** — Hiro RPCs are implemented on your Nakama project; the Unity client must point at a server where those RPCs are deployed.
2. **`IVXBootstrap` configured** — Follow the [quickstart](../getting-started/quickstart.md) so core SDK bootstrap runs (platform, backend, optional Hiro wiring depending on your scene setup). You need a valid game configuration and network reachability to Nakama.
3. **Authenticated session** — Obtain an `IClient` and `ISession` after sign-in (see [Backend module](../modules/backend.md)). Hiro methods expect a live session; refresh it on token rotation.

!!! note "Coordinator in the scene"
    The `IVXHiroCoordinator` component must live on a **persistent** `GameObject` (typically `DontDestroyOnLoad`). The module describes it as the singleton entry point for all Hiro systems.

---

## Step 1 — Initialize Hiro coordinator

After Nakama authentication succeeds, inject the client and session into the coordinator. The documented API is **`InitializeSystems`**, which builds all system instances and attaches the shared RPC client.

```csharp
using IntelliVerseX.Backend;
using IntelliVerseX.Hiro;
using Nakama;
using UnityEngine;

public class GameBootstrap : MonoBehaviour
{
    [SerializeField] private MyGameNakamaManager _nakamaManager;

    private async void Start()
    {
        bool connected = await _nakamaManager.InitializeAsync();
        if (!connected)
            return;

        var coordinator = IVXHiroCoordinator.Instance;
        coordinator.OnInitialized += OnHiroReady;

        coordinator.InitializeSystems(
            _nakamaManager.Client,
            _nakamaManager.Session);
    }

    private void OnHiroReady(bool success)
    {
        if (success)
            Debug.Log("All Hiro systems ready.");
        else
            Debug.LogWarning("Hiro initialization failed.");
    }
}
```

When the Nakama token is refreshed, propagate the new session:

```csharp
IVXHiroCoordinator.Instance.RefreshSession(newSession);
```

!!! note "Naming"
    The Hiro module documents **`InitializeSystems`**, not `InitializeAsync`, on `IVXHiroCoordinator`. Your game’s own bootstrap type may expose `InitializeAsync`; the coordinator entry point remains `InitializeSystems`.

---

## Step 2 — Economy & wallet

The Hiro module lists **`Economy`** as `IVXEconomySystem`: **currency operations, donations, and rewarded video credits**, all validated on the server.

```csharp
var economy = IVXHiroCoordinator.Instance.Economy;
```

Typical responsibilities (per the module overview):

- **Server-authoritative currency** — Grants and deductions should go through economy RPCs so balances cannot be spoofed.
- **Donations** — Player-to-player or similar flows that must be validated centrally.
- **Rewarded video credits** — Completion hooks that credit rewards only after server acknowledgment.

For how **client-cached balances** interact with Hiro (`IVXWalletManager` vs `IVXEconomySystem` / `IVXStoreSystem`), see [Wallet & Economy](../modules/wallet.md).

!!! tip "After init"
    Only use `economy` after `OnInitialized` has fired with `success == true`, or you risk null or uninitialized RPC clients.

---

## Step 3 — Energy system

**`Energy`** maps to `IVXEnergySystem`: **time-gated energy with regeneration timers**.

```csharp
var energy = IVXHiroCoordinator.Instance.Energy;
```

Use this system when gameplay needs:

- **Current energy** and caps as defined server-side.
- **Time-to-refill** driven by regeneration rules (not a local timer pretending to be authoritative).
- **Spend / grant** flows that must be validated so players cannot bypass costs.

Exact async method names and payloads for `IVXEnergySystem` are not expanded in the Hiro module page; treat the coordinator property as the access point and align your implementation with the same RPC response patterns described under [Error handling](../modules/hiro.md#error-handling) (null-safe returns, optional `IVXHiroRpcClient` for diagnostics).

---

## Step 4 — Inventory

**`Inventory`** is `IVXInventorySystem`: **item management with categories, stacking, and expiry**.

```csharp
var inventory = IVXHiroCoordinator.Instance.Inventory;
```

Integration goals:

- **Adding items** — Server grants after quests, IAP, or spin rewards (often expressed as `IVXReward` with an `items` map; see [Common data types](../modules/hiro.md#common-data-types)).
- **Consuming items** — Server validates consumption so duplicates or negative counts cannot be forced from the client.
- **Listing** — Drive inventory UI from server-backed state after initialization.

---

## Step 5 — Achievements

**`Achievements`** is `IVXAchievementsSystem`: **hierarchical achievements with sub-achievements**.

```csharp
var achievements = IVXHiroCoordinator.Instance.Achievements;
```

You will typically:

- **List** achievement trees for menus and progress widgets.
- **Update** progress when gameplay events occur (server merges sub-progress).
- **Claim** rewards for completed nodes when the server marks them claimable.

---

## Step 6 — Event leaderboards

**`EventLeaderboards`** is `IVXEventLeaderboardSystem`: **time-limited competitive events with tiers**.

```csharp
var eventLeaderboards = IVXHiroCoordinator.Instance.EventLeaderboards;
```

Use it for:

- **Submitting scores** during an active event window.
- **Fetching rankings** and tier placement for UI and end-of-event rewards.

For persistent, non-event leaderboards, the module cross-references the standalone [Leaderboard module](../modules/leaderboards.md) (`IVXLeaderboardsSystem` is also listed under Hiro core systems).

---

## Step 7 — Spin wheel

Documented API on `IVXSpinWheelSystem`:

| Method | Returns | Description |
|--------|---------|-------------|
| `GetAsync(wheelId?)` | `IVXSpinWheelConfig` | Segments, free/ad spin counts, costs. |
| `SpinAsync(spinType, wheelId?)` | `IVXSpinWheelResult` | Server-selected winning segment and reward. |

`spinType`: `"free"`, `"ad"`, `"currency"`.

```csharp
var wheel = IVXHiroCoordinator.Instance.SpinWheel;

var config = await wheel.GetAsync();
Debug.Log($"Free spins left: {config.freeSpinsRemaining}");

foreach (var seg in config.segments)
{
    AddWheelSegment(seg.label, seg.color, seg.isJackpot);
}

if (config.freeSpinsRemaining > 0)
{
    var result = await wheel.SpinAsync("free");
    if (result != null)
    {
        AnimateWheelTo(result.winningSegment.segmentId);
        ShowRewardPopup(result.reward);
    }
}
```

---

## Step 8 — Streaks

`IVXStreaksSystem` tracks **daily streaks** with milestone rewards.

| Method | Returns | Description |
|--------|---------|-------------|
| `GetAsync(gameId?)` | `IVXStreaksGetResponse` | Current/best counts and metadata. |
| `UpdateAsync(streakId, gameId?)` | `IVXStreak` | Increment or maintain a streak. |
| `ClaimMilestoneAsync(streakId, milestone, gameId?)` | `IVXStreakClaimResponse` | Claim a milestone reward. |

`IVXStreak` includes `streakId`, `name`, `currentCount`, `bestCount`, `lastUpdateSec`, `resetAt`, and `claimedMilestones`.

```csharp
var streaks = IVXHiroCoordinator.Instance.Streaks;

var snapshot = await streaks.GetAsync();
var updated = await streaks.UpdateAsync("daily_login");
if (updated != null && updated.currentCount == 7)
{
    var claimed = await streaks.ClaimMilestoneAsync("daily_login", 7);
    ShowMilestoneReward(claimed?.reward);
}
```

Additional streak-related systems on the same coordinator include **`StreakShield`** (`GetAsync`, `ActivateAsync`, `ReplenishAsync`) and **`FriendStreaks`** (bilateral daily interactions—see Step 11).

---

## Step 9 — Offerwall

`IVXOfferwallSystem`:

| Method | Returns | Description |
|--------|---------|-------------|
| `GetAsync()` | `IVXOfferwallState` | Available, pending, and completed offers. |
| `CompleteOfferAsync(offerId, provider, transactionId)` | `IVXOfferwallCompleteResponse` | Record completion from provider callback. |
| `ClaimPendingAsync()` | `IVXOfferwallCompleteResponse` | Credit pending rewards to the wallet. |

```csharp
var offerwall = IVXHiroCoordinator.Instance.Offerwall;

var state = await offerwall.GetAsync();
foreach (var offer in state.offers)
{
    if (!offer.completed)
        AddOfferCard(offer.title, offer.rewardAmount, offer.rewardCurrency);
}

await offerwall.CompleteOfferAsync(
    offerId: "offer_123",
    provider: "tapjoy",
    transactionId: "txn_abc");

if (state.pendingRewards > 0)
{
    var claimed = await offerwall.ClaimPendingAsync();
    ShowRewardPopup(claimed?.reward);
}
```

---

## Step 10 — Retention & engagement

### Retention

| Method | Returns | Description |
|--------|---------|-------------|
| `GetAsync()` | `IVXRetentionState` | Session depth, churn risk, onboarding, comeback flags. |
| `HeartbeatAsync()` | `IVXRetentionHeartbeatResponse` | Session heartbeat; may return rewards. |
| `CompleteOnboardingStepAsync(step)` | `IVXRetentionState` | Advance onboarding. |
| `ClaimComebackBonusAsync()` | `IVXRetentionHeartbeatResponse` | Claim absence bonus if eligible. |

### Related engagement surfaces (same coordinator)

- **`Appointments`** — `GetAsync`, `ClaimAsync(appointmentId)` for scheduled reward windows.
- **`DailyContent`** — `GetAsync`, `ClaimAsync(slotId, actionPayload?)` for rotating daily slots.
- **`SessionBoosters`** — `GetAsync`, `ActivateAsync(boosterId, source?)`, `ClaimFreeAsync`.

Example combining heartbeat, comeback, and streak update (from the module):

```csharp
var hiro = IVXHiroCoordinator.Instance;

var heartbeat = await hiro.Retention.HeartbeatAsync();
if (heartbeat?.reward != null)
    ShowRewardPopup(heartbeat.reward);

var state = await hiro.Retention.GetAsync();
if (state.comebackBonusAvailable)
{
    var bonus = await hiro.Retention.ClaimComebackBonusAsync();
    ShowWelcomeBackDialog(state.daysSinceLastSession, bonus?.reward);
}

var streak = await hiro.Streaks.UpdateAsync("daily_login");
```

“Login rewards” in the sense of **first actions each day** are usually composed of `Retention.HeartbeatAsync`, `Streaks.UpdateAsync`, `DailyContent.GetAsync` / `ClaimAsync`, and optional `Appointments`—all server-driven.

---

## Step 11 — Friend quests & battles

### Friend quests

| Method | Returns |
|--------|---------|
| `GetAsync()` | `IVXFriendQuestState` |
| `AcceptAsync(questId, partnerId)` | `IVXFriendQuestAcceptResponse` |
| `ReportProgressAsync(questId, amount)` | `IVXFriendQuestProgressResponse` |

### Friend streaks (social)

| Method | Returns |
|--------|---------|
| `GetAsync()` | `IVXFriendStreakState` |
| `InteractAsync(friendId)` | `IVXFriendStreakInteractResponse` |
| `ClaimMilestoneAsync(streakId, day)` | `IVXFriendStreakInteractResponse` |

### Friend battles

| Method | Returns |
|--------|---------|
| `GetAsync()` | `IVXFriendBattleState` |
| `SendChallengeAsync(friendId, gameMode, score?)` | `IVXFriendBattleSendResponse` |
| `AcceptChallengeAsync(challengeId)` | `IVXFriendBattleSendResponse` |
| `SubmitScoreAsync(challengeId, score)` | `IVXFriendBattleSubmitResponse` |

```csharp
var hiro = IVXHiroCoordinator.Instance;

var questState = await hiro.FriendQuests.GetAsync();
// Accept and report progress as in the module’s social flow example.

var interact = await hiro.FriendStreaks.InteractAsync(friendUserId);

var challenge = await hiro.FriendBattles.SendChallengeAsync(
    friendUserId, gameMode: "trivia_blitz", score: 850);
```

Friend lists and social graph details are covered in the [Social module](../modules/social.md).

---

## Step 12 — IAP triggers & smart ads

### IAP triggers (`IVXIAPTriggerSystem`)

| Method | Returns |
|--------|---------|
| `EvaluateAsync(context, contextValue?)` | `IVXIAPTriggerEvalResponse` |
| `DismissAsync(triggerId)` | `IVXIAPTriggerDismissResponse` |
| `RecordConversionAsync(triggerId, receipt)` | `bool` |

Context examples from the module: `"post_game"`, `"store_visit"`, `"low_currency"`, `"win_streak"`, `"level_up"`.

### Smart ad timer (`IVXSmartAdTimerSystem`)

| Method | Returns |
|--------|---------|
| `GetAsync()` | `IVXSmartAdTimerState` |
| `CanShowAsync(adType)` | `IVXSmartAdTimerState` |
| `RecordImpressionAsync(adType, placementId?)` | `IVXSmartAdTimerRecordResponse` |

`adType`: `"interstitial"`, `"rewarded"`, `"banner"`.

```csharp
var hiro = IVXHiroCoordinator.Instance;

var evalResponse = await hiro.IAPTriggers.EvaluateAsync("post_game", contextValue: 5);
if (evalResponse.triggers.Count > 0)
{
    var offer = evalResponse.triggers[0];
    ShowOfferPopup(offer.displayTitle, offer.displayMessage, offer.offerSku);
}

var timerState = await hiro.SmartAdTimer.GetAsync();
if (timerState.rewardedAdsToday < timerState.maxRewardedAdsPerDay)
    ShowRewardedAdButton();

var adResult = await hiro.SmartAdTimer.RecordImpressionAsync("rewarded", "double_coins");
if (adResult?.reward != null)
    CreditReward(adResult.reward);
```

Optional complement: **`AdRevenueOptimizer`** (`GetConfigAsync`, `RecordImpressionAsync`) for placement configs and revenue attribution, as shown in the module’s monetization example.

---

## Troubleshooting

| Symptom | Likely cause | What to do |
|---------|----------------|------------|
| All Hiro calls return `null` | Network failure, expired session, or RPC error swallowed per [Error handling](../modules/hiro.md#error-handling) | Verify connectivity; refresh session with `RefreshSession`; temporarily inspect `IVXHiroRpcClient.CallAsync` for `success` / `error`. |
| “Hiro not ready” race | UI calls systems before `OnInitialized` | Gate UI on `OnInitialized` or a game-level “metagame ready” flag. |
| Stale user after token refresh | Session not propagated | Call `IVXHiroCoordinator.Instance.RefreshSession(newSession)` whenever Nakama refreshes the JWT. |
| Spin / offerwall seems “wrong” | Client-side assumptions | Re-fetch `GetAsync` state after every mutation; trust server fields (`freeSpinsRemaining`, `completed`, etc.). |
| Verbose RPC logs in release | Debug logging enabled | Set `IVXHiroRpcClient.EnableDebugLogs = false` for production builds. |
| Duplicate initialization | `InitializeSystems` called multiple times | The module recommends **initialize once** after auth; guard with an app-level flag if multiple entry points exist. |

---

## See Also

- [Hiro module](../modules/hiro.md) — Full system list, RPC ID tables, architecture diagram, and extended examples.
- [Hiro API reference](../api/hiro.md) — Full API detail for all 33 Hiro systems.
- [Hiro demo sample](../samples/hiro-demo.md) — End-to-end sample walkthrough for spin wheel, streaks, and offerwall.
- [Wallet & Economy](../modules/wallet.md) — `IVXWalletManager` vs `IVXEconomySystem` / `IVXStoreSystem` split.
- [Backend module](../modules/backend.md) — Nakama client and authentication.
- [Monetization module](../modules/monetization.md) — Ad SDK wiring alongside `SmartAdTimer` / `AdRevenueOptimizer`.

---

## Quick checklist

- [ ] `IVXHiroCoordinator` present on a persistent object.
- [ ] `InitializeSystems(client, session)` after successful auth.
- [ ] `RefreshSession` wired to token refresh.
- [ ] `OnInitialized` used to unlock UI that calls Hiro.
- [ ] Null checks on all async Hiro results before dereferencing.
- [ ] Production builds disable `IVXHiroRpcClient.EnableDebugLogs`.
