# Wallet & Economy Integration

This guide walks you through integrating the IntelliVerseX **wallet cache**, **Hiro economy** (donations, rewarded video), **in-game store**, and **IAP validation** into a Unity game. It complements the reference material in the [Wallet module](../modules/wallet.md).

---

## Overview

The SDK splits economy concerns into two layers that work together:

| Layer | What it does |
|-------|----------------|
| **`IVXWalletManager`** | Client-side **cached** game and global balances, wallet IDs, affordability checks, **local** coin grants, and hooks to refresh or push balances from your Nakama path. Reads and updates flow through `IntelliVerseXIdentity`. |
| **Hiro (`IVXEconomySystem`, `IVXStoreSystem`, `IVXBaseSystem`)** | **Server-authoritative** operations over Nakama RPCs: donations, rewarded video credits, store catalog and purchases, IAP receipt validation and purchase history. |

Use the wallet manager for **fast UI** and **optimistic hints** (for example enabling a button). Use Hiro for anything that **must** be validated on the server—store purchases, donation flows, and real-money IAP.

!!! tip "Mental model"
    Think: **cache + hints** on the client, **truth** on the server. After a successful Hiro call, refresh or apply rewards so the UI matches authoritative state.

---

## Prerequisites

Before following the steps below, ensure the following are in place:

1. **Nakama** — A running Nakama stack with Hiro RPCs configured for economy, store, and IAP (see [Backend module](../modules/backend.md) and [Nakama integration](nakama-integration.md)).
2. **Hiro coordinator** — `IVXHiroCoordinator` initialized with a valid `IClient` and `ISession` after authentication (see [Hiro module](../modules/hiro.md)).
3. **`IVXBootstrap`** — Your scene or startup flow uses the SDK bootstrap so identity, session, and backend wiring are consistent (`IVXBootstrap` in the `_IntelliVerseXSDK` package).
4. **Wallet IDs on the user** — Wallet identifiers are assigned when the user is created or synced (for example via `create_or_sync_user` on your backend). `IVXWalletManager.HasWalletIds()` should become `true` after a successful auth/sync path.

!!! warning "Assembly references"
    Wallet APIs live in `IntelliVerseX.Backend`. Hiro economy, store, and base IAP APIs live in `IntelliVerseX.Hiro` / `IntelliVerseX.Hiro.Systems`. Ensure your game assembly references the correct `.asmdef` dependencies.

---

## Step 1 — Access the wallet manager

`IVXWalletManager` is a **`public static` class** in the `IntelliVerseX.Backend` namespace. You do **not** obtain it through a MonoBehaviour singleton.

!!! info "No `Instance` property"
    Unlike `IVXLeaderboardManager.Instance`, **`IVXWalletManager` has no `Instance`**. Call static members directly on the type.

```csharp
using IntelliVerseX.Backend;

// Correct — static API
int coins = IVXWalletManager.GetGameBalance();

// Incorrect — will not compile
// var wallet = IVXWalletManager.Instance;
```

Add `using IntelliVerseX.Backend;` wherever you read balances, subscribe to events, or call refresh helpers.

---

## Step 2 — Check balances

All balance reads use the **cached** values stored on identity. They do **not** perform network I/O, so they are safe to call from UI every frame if needed (still prefer event-driven updates when possible).

| Method | Purpose |
|--------|---------|
| `GetGameBalance()` | Soft currency / game wallet (typically `"coins"` in examples). |
| `GetGlobalBalance()` | Global wallet (often mapped to premium currency such as gems). |
| `GetGameWalletId()` / `GetGlobalWalletId()` | String IDs for RPCs or debugging. |
| `HasWalletIds()` | `true` when both wallet IDs are assigned. |
| `CanAfford(int cost, bool useGlobal = false)` | `true` if cached balance ≥ `cost` (game wallet by default; pass `useGlobal: true` for the global wallet). |

```csharp
int game = IVXWalletManager.GetGameBalance();
int global = IVXWalletManager.GetGlobalBalance();

if (!IVXWalletManager.HasWalletIds())
{
    // User not fully provisioned — avoid relying on balances for purchases yet
    return;
}

bool canBuyPowerUp = IVXWalletManager.CanAfford(100);
bool canBuyCosmetic = IVXWalletManager.CanAfford(50, useGlobal: true);
```

!!! note "Hints, not guarantees"
    `CanAfford` reflects **cache only**. The server may still reject a purchase (race conditions, changed catalog, anti-cheat). Always handle `PurchaseAsync` failure and refresh after success.

---

## Step 3 — Grant currency locally (`AddCoins`)

`AddCoins(int amount, string source = "unknown")` increases the **game** wallet balance **on the client** and raises `OnBalanceChanged`.

Per the API contract in source, this is a **local update**; **server sync is separate**. Use it for:

- Offline or demo flows
- Temporary UI feedback before a server round-trip confirms the grant
- Test harnesses

```csharp
// Award 25 coins locally after a client-only milestone (sync via your own RPC if needed)
IVXWalletManager.AddCoins(25, source: "level_complete_bonus");
```

!!! warning "Authoritative grants"
    For production rewards that must not be spoofed (quests, rewarded ads, store purchases), prefer **Hiro** (`IVXEconomySystem`, `IVXStoreSystem`, `IVXBaseSystem`) and then **`UpdateBalances`** or **`RefreshWalletsAsync`** when your Nakama client supplies new balances.

---

## Step 4 — Refresh from server (`RefreshWalletsAsync`)

`RefreshWalletsAsync()` returns `Task<bool>`. It is intended to refresh balances via the server (for example `create_or_get_wallet` for both wallets). The SDK documents this intent in `IVXWalletManager`.

**Current behavior** in the repository: if `HasWalletIds()` is false, it logs a warning and returns `false`. Otherwise it completes without a remote fetch, logs cached balances, and returns `true`. Wire your Nakama client to populate authoritative values and call `UpdateBalances` when that path is ready.

```csharp
private async System.Threading.Tasks.Task SyncWalletsFromServer()
{
    bool ok = await IVXWalletManager.RefreshWalletsAsync();
    if (!ok)
    {
        // Missing wallet IDs or error — OnWalletError may have fired on exception
        return;
    }

    // After you extend RefreshWalletsAsync or use UpdateBalances from Nakama:
    UpdateHud(IVXWalletManager.GetGameBalance(), IVXWalletManager.GetGlobalBalance());
}
```

If another subsystem receives authoritative balances from Nakama, push them explicitly:

```csharp
IVXWalletManager.UpdateBalances(gameBalance: 1200, globalBalance: 40,
    gameCurrency: "coins", globalCurrency: "gems");
```

---

## Step 5 — Server-authoritative economy (`IVXEconomySystem`)

Access the economy system through the Hiro coordinator **after** initialization:

```csharp
using IntelliVerseX.Hiro;

var economy = IVXHiroCoordinator.Instance.Economy;
```

Typical RPC-backed methods:

| Method | RPC (conceptual) | Notes |
|--------|------------------|-------|
| `RequestDonationAsync(donationId, gameId)` | `hiro_economy_donation_request` | Returns `null` on failure. |
| `GiveDonationAsync(targetUserId, donationId, amount, gameId)` | `hiro_economy_donation_give` | Sender may receive `IVXReward` in response. |
| `ClaimDonationsAsync(donationIds, gameId)` | `hiro_economy_donation_claim` | Returns `IVXReward` with currencies/items. |
| `CompleteRewardedVideoAsync(gameId)` | `hiro_economy_rewarded_video` | Call after the user **successfully** finishes a rewarded placement. |

Example: rewarded video completion → apply server reward to UI and balances:

```csharp
var rv = await economy.CompleteRewardedVideoAsync(gameId: null);
if (rv != null && rv.rewarded && rv.reward != null)
{
    ApplyRewardToUi(rv.reward);
    // Optionally map rv.reward.currencies into IVXWalletManager.UpdateBalances
    // once you know the mapping from Hiro currency keys to game/global wallets.
}
```

!!! tip "Donations and social flows"
    Use `RequestDonationAsync` to show progress, `GiveDonationAsync` for contributing, and `ClaimDonationsAsync` when the player collects pooled rewards. Always null-check task results; Hiro returns `null` when `success` is false on the RPC envelope.

---

## Step 6 — In-game store (`IVXStoreSystem`)

Access:

```csharp
var store = IVXHiroCoordinator.Instance.Store;
```

| Method | Behavior |
|--------|----------|
| `ListAsync(string gameId = null)` | Loads catalog via `hiro_store_list`. On RPC failure returns **`new IVXStoreListResponse()`** (empty sections), **not** `null`. |
| `PurchaseAsync(sectionId, itemId, gameId)` | Server-validated purchase via `hiro_store_purchase`. Returns **`null`** on failure. |

```csharp
var list = await store.ListAsync();
if (list.sections == null) { /* treat as empty */ }

foreach (var section in list.sections)
{
    foreach (var item in section.items)
    {
        if (item.disabled) continue;
        // item.cost is Dictionary<string, long> — keys are currency ids from Hiro
        ShowRow(section.sectionId, item);
    }
}
```

---

## Step 7 — End-to-end purchase flow

Recommended pattern:

1. **Optional UI hint:** `CanAfford` against cached balance for the relevant currency key (for example `"coins"`).
2. **Server purchase:** `PurchaseAsync`.
3. **Handle result:** if non-null, show `reward` / updated `item`; if null, show failure.
4. **Reconcile wallet:** `RefreshWalletsAsync` and/or `UpdateBalances` from server payload.

```csharp
using IntelliVerseX.Backend;
using IntelliVerseX.Hiro;
using UnityEngine;

public async void BuyStoreItem(string sectionId, IntelliVerseX.Hiro.IVXStoreItem item)
{
    long coinCost = item.cost != null && item.cost.TryGetValue("coins", out var c) ? c : 0;
    if (coinCost > 0 && !IVXWalletManager.CanAfford((int)coinCost))
    {
        ShowNotEnoughCoins();
        return;
    }

    var purchased = await IVXHiroCoordinator.Instance.Store.PurchaseAsync(sectionId, item.itemId);
    if (purchased == null)
    {
        Debug.LogWarning("Store purchase failed (server rejected or RPC error).");
        ShowPurchaseFailed();
        return;
    }

    if (purchased.reward != null)
        ShowRewardPopup(purchased.reward);

    await IVXWalletManager.RefreshWalletsAsync();
}
```

!!! warning "Catalog vs cache mismatch"
    Costs and stock limits live on the server. The client catalog from `ListAsync` can be stale; the server is always right on `PurchaseAsync`.

---

## Step 8 — IAP validation (`ValidateIAPAsync`, receipts)

Real-money purchases are **not** handled by `IVXWalletManager`. Use **`IVXHiroCoordinator.Instance.Base`** (`IVXBaseSystem`):

| Method | Purpose |
|--------|---------|
| `ValidateIAPAsync(receipt, storeType, productId, price?, currency?, gameId?)` | RPC `hiro_iap_validate`. Returns `null` on failure. |
| `GetPurchaseHistoryAsync(gameId)` | RPC `hiro_iap_history`. On failure returns **`new IVXIAPHistoryResponse()`**. |

After the platform store reports a successful purchase, obtain the **platform receipt string** (Google Play, App Store, etc.) and validate server-side:

```csharp
var baseSystem = IVXHiroCoordinator.Instance.Base;

var validation = await baseSystem.ValidateIAPAsync(
    receipt: platformReceipt,
    storeType: "googleplay",   // Must match what your Hiro server expects
    productId: sku,
    price: localizedPrice,
    currency: isoCurrencyCode,
    gameId: null);

if (validation != null && validation.valid && validation.reward != null)
{
    ApplyRewardToUi(validation.reward);
    await IVXWalletManager.RefreshWalletsAsync();
}
```

!!! danger "Never trust the client alone"
    Always send the receipt to the server through `ValidateIAPAsync`. Do not grant premium currency solely from client-side IAP callbacks without validation.

---

## Events and callbacks

### `IVXWalletManager` (static events)

| Event | Signature | When it fires |
|-------|-----------|----------------|
| `OnBalanceChanged` | `Action<int, int>` | `(gameBalance, globalBalance)` after `AddCoins`, `UpdateBalances`, or any code path that calls `UpdateBalances` internally. |
| `OnWalletError` | `Action<string>` | Error message when `RefreshWalletsAsync` catches an exception. |

Subscribe in `OnEnable` / unsubscribe in `OnDisable` on UI components:

```csharp
private void OnEnable()
{
    IVXWalletManager.OnBalanceChanged += HandleBalanceChanged;
    IVXWalletManager.OnWalletError += HandleWalletError;
    RefreshLabels();
}

private void OnDisable()
{
    IVXWalletManager.OnBalanceChanged -= HandleBalanceChanged;
    IVXWalletManager.OnWalletError -= HandleWalletError;
}

private void HandleBalanceChanged(int game, int global) => SetLabels(game, global);
private void HandleWalletError(string message) => Debug.LogWarning($"Wallet: {message}");
```

!!! note "Hiro systems and events"
    `IVXEconomySystem` and `IVXStoreSystem` do not expose public C# events in the SDK sources; use `Task` completion and response objects. Coordinator lifecycle remains available via `IVXHiroCoordinator.OnInitialized` (see [Hiro module](../modules/hiro.md)).

---

## Troubleshooting

| Symptom | Likely cause | What to try |
|---------|----------------|-------------|
| `RefreshWalletsAsync` always returns `false` | `HasWalletIds()` is false | Ensure post-login `create_or_sync_user` (or equivalent) runs and `IVXWalletManager.SetWalletIds` is invoked from your Nakama integration path (internal to SDK client code). |
| Balances never match server | Cache not updated | After Hiro purchases or rewards, call `UpdateBalances` from Nakama responses or extend `RefreshWalletsAsync` to fetch real balances. |
| `PurchaseAsync` returns `null` | RPC failure or server rejection | Check Nakama logs, session validity, and catalog IDs. Confirm `sectionId` / `itemId` match `ListAsync`. |
| `ListAsync` shows empty store | Failure path returns empty object | Distinguish “empty catalog” from “RPC failed” using raw `IVXHiroRpcClient` logging if needed. |
| `ValidateIAPAsync` returns `null` | Invalid receipt, wrong `storeType`, or server error | Verify platform-specific receipt format and Hiro IAP configuration. |
| UI does not update after `AddCoins` | Missing event subscription | Subscribe to `OnBalanceChanged` or poll `GetGameBalance` after known updates. |
| Duplicate subscriptions | Forgetting `-=` in `OnDisable` | Always unsubscribe symmetrically to avoid duplicate UI updates or leaks. |

!!! tip "Debugging RPCs"
    For low-level inspection, use `IVXHiroCoordinator.Instance.RpcClient.CallAsync<T>` and check `success` / `error` on the Hiro response envelope (see [Hiro module](../modules/hiro.md)).

---

## See also

- [Wallet module](../modules/wallet.md) — Full API tables, models, and RPC reference
- [Backend module](../modules/backend.md) — Nakama session, identity, and wallet IDs
- [Hiro module](../modules/hiro.md) — `IVXHiroCoordinator`, `IVXHiroRpcClient`, initialization
- [Monetization module](../modules/monetization.md) — Ad and IAP SDK wiring at the game layer
- [Nakama integration](nakama-integration.md) — Backend connection patterns
- [Authentication flow](auth-flow.md) — Ensuring users are signed in before economy calls

---

*Last updated to match SDK sources: `IVXWalletManager`, Hiro economy/store/base as documented in the Wallet module.*
