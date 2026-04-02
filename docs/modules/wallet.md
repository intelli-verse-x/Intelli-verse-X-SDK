# Wallet & Economy

The Wallet & Economy surface spans two layers: **`IVXWalletManager`** (client-side cached balances, affordability checks, and local coin updates tied to `IntelliVerseXIdentity`) and **Hiro** systems **`IVXEconomySystem`** and **`IVXStoreSystem`** (Nakama RPC–driven, server-authoritative donations, rewarded video grants, and in-game store catalog/purchases). Use the wallet manager for fast UI reads and optimistic/local adjustments where appropriate; use Hiro for anything that must be validated on the server (store purchases, donation flows, rewarded completion).

---

## 1. Overview

| | |
|---|---|
| **Wallet namespace** | `IntelliVerseX.Backend` |
| **Wallet type** | `IVXWalletManager` (static class) |
| **Hiro namespaces** | `IntelliVerseX.Hiro`, `IntelliVerseX.Hiro.Systems` |
| **Economy / Store types** | `IVXEconomySystem`, `IVXStoreSystem` |
| **IAP (Hiro Base)** | `IVXBaseSystem` — `ValidateIAPAsync`, `GetPurchaseHistoryAsync` |
| **Assemblies** | `IntelliVerseX.Backend`, `IntelliVerseX.Hiro` |
| **Hiro access** | `IVXHiroCoordinator.Instance` after `InitializeSystems` (see [Hiro module](hiro.md)) |

### Responsibility split

| Layer | Responsibility |
|-------|----------------|
| `IVXWalletManager` | Cached game/global balances, wallet IDs, `CanAfford`, local `AddCoins`, balance refresh hook, balance/error events |
| `IVXEconomySystem` | Donation request / give / claim; rewarded video completion (server credit) |
| `IVXStoreSystem` | Store catalog listing; server-validated purchases |
| `IVXBaseSystem` | IAP receipt validation; purchase history RPC |

!!! note "Transaction history"
    **`IVXWalletManager` does not expose a transaction or ledger API.** For server-backed **IAP purchase history**, use `IVXBaseSystem.GetPurchaseHistoryAsync`. Store and economy operations return typed responses (for example `IVXReward`, `IVXStorePurchaseResponse`) suitable for UI updates.

---

## 2. Basic wallet operations (`IVXWalletManager`)

All methods below are **`public static`** on `IVXWalletManager`.

### 2.1 Read balances and wallet IDs

| Method | Returns | Description |
|--------|---------|-------------|
| `GetGameBalance()` | `int` | Cached game wallet balance (no network call). |
| `GetGlobalBalance()` | `int` | Cached global wallet balance (no network call). |
| `GetGameWalletId()` | `string` | Game wallet id from identity. |
| `GetGlobalWalletId()` | `string` | Global wallet id from identity. |
| `HasWalletIds()` | `bool` | Whether both wallet ids are assigned. |

### 2.2 Affordability

| Method | Returns | Description |
|--------|---------|-------------|
| `CanAfford(int cost, bool useGlobal = false)` | `bool` | `true` if cached balance is at least `cost` (game wallet by default; global when `useGlobal` is `true`). |

### 2.3 Grant / adjust currency (local)

| Method | Returns | Description |
|--------|---------|-------------|
| `AddCoins(int amount, string source = "unknown")` | `void` | Adds coins to the **game** wallet locally and raises `OnBalanceChanged`. Per API documentation in source, this is a **local update**; server sync is separate. |

### 2.4 Refresh from server

| Method | Returns | Description |
|--------|---------|-------------|
| `RefreshWalletsAsync()` | `Task<bool>` | Intended to refresh balances via server (`create_or_get_wallet` for both wallets). Returns `false` if wallet ids are missing; on failure invokes `OnWalletError` and returns `false`. **Current implementation** completes without remote fetch and logs cached balances—integrate when your Nakama client path is wired. |

### 2.5 Balance updates (integration / advanced)

| Method | Returns | Description |
|--------|---------|-------------|
| `UpdateBalances(int gameBalance, int globalBalance, string gameCurrency = "coins", string globalCurrency = "gems")` | `void` | Pushes balances into `IntelliVerseXIdentity` and invokes `OnBalanceChanged`. Suitable when another subsystem (for example Nakama client code) receives authoritative balances. |

### 2.6 Example: bind UI to cached wallet

```csharp
using IntelliVerseX.Backend;
using UnityEngine;

public class WalletHud : MonoBehaviour
{
    private void OnEnable()
    {
        IVXWalletManager.OnBalanceChanged += OnBalanceChanged;
        RefreshLabels();
    }

    private void OnDisable()
    {
        IVXWalletManager.OnBalanceChanged -= OnBalanceChanged;
    }

    private void OnBalanceChanged(int game, int global)
    {
        SetLabels(game, global);
    }

    private async void RefreshFromServer()
    {
        bool ok = await IVXWalletManager.RefreshWalletsAsync();
        if (ok)
            RefreshLabels();
    }

    private void RefreshLabels()
    {
        SetLabels(IVXWalletManager.GetGameBalance(), IVXWalletManager.GetGlobalBalance());
    }

    private void SetLabels(int game, int global) { /* ... */ }
}
```

---

## 3. Hiro economy system (`IVXEconomySystem`)

Access: `IVXHiroCoordinator.Instance.Economy`.

Constructor (internal to Hiro): `public IVXEconomySystem(IVXHiroRpcClient rpc)`.

| Method | Returns | Description |
|--------|---------|-------------|
| `RequestDonationAsync(string donationId, string gameId = null)` | `Task<IVXDonation>` | RPC `hiro_economy_donation_request`. Returns `null` on RPC failure. |
| `GiveDonationAsync(string targetUserId, string donationId, int amount = 1, string gameId = null)` | `Task<IVXDonationGiveResponse>` | RPC `hiro_economy_donation_give`. |
| `ClaimDonationsAsync(string[] donationIds, string gameId = null)` | `Task<IVXReward>` | RPC `hiro_economy_donation_claim`. |
| `CompleteRewardedVideoAsync(string gameId = null)` | `Task<IVXRewardedVideoResponse>` | RPC `hiro_economy_rewarded_video`. |

### Response models (from `IVXHiroModels`)

```csharp
public class IVXDonation
{
    public string donationId;
    public string name;
    public string description;
    public string status;
    public int currentCount;
    public int maxCount;
    public long expiresAt;
    public List<IVXDonationContributor> contributors;
}

public class IVXDonationContributor
{
    public string userId;
    public string username;
    public int amount;
}

public class IVXDonationGiveResponse
{
    public string donationId;
    public int contributed;
    public IVXReward senderReward;
}

public class IVXRewardedVideoResponse
{
    public bool rewarded;
    public IVXReward reward;
}
```

`IVXReward` (shared Hiro type):

```csharp
public class IVXReward
{
    public Dictionary<string, float> currencies;
    public Dictionary<string, int> items;
}
```

### Example: rewarded video → server credit

```csharp
var economy = IVXHiroCoordinator.Instance.Economy;
var rv = await economy.CompleteRewardedVideoAsync(gameId: null);
if (rv != null && rv.rewarded && rv.reward != null)
{
    ApplyRewardToUi(rv.reward);
}
```

---

## 4. Hiro store system (`IVXStoreSystem`)

Access: `IVXHiroCoordinator.Instance.Store`.

Constructor: `public IVXStoreSystem(IVXHiroRpcClient rpc)`.

| Method | Returns | Description |
|--------|---------|-------------|
| `ListAsync(string gameId = null)` | `Task<IVXStoreListResponse>` | RPC `hiro_store_list`. On failure returns **`new IVXStoreListResponse()`** (empty sections), not `null`. |
| `PurchaseAsync(string sectionId, string itemId, string gameId = null)` | `Task<IVXStorePurchaseResponse>` | RPC `hiro_store_purchase`. Returns **`null`** on failure. |

### Models

```csharp
public class IVXStoreListResponse
{
    public List<IVXStoreSection> sections;
}

public class IVXStoreSection
{
    public string sectionId;
    public string name;
    public List<IVXStoreItem> items;
}

public class IVXStoreItem
{
    public string itemId;
    public string name;
    public string description;
    public string category;
    public Dictionary<string, long> cost;
    public string sku;
    public IVXReward reward;
    public long availableAt;
    public long expiresAt;
    public int maxPurchases;
    public int purchaseCount;
    public bool disabled;
    public Dictionary<string, string> additionalProperties;
}

public class IVXStorePurchaseResponse
{
    public IVXReward reward;
    public IVXStoreItem item;
}
```

### Example: load catalog

```csharp
var store = IVXHiroCoordinator.Instance.Store;
var list = await store.ListAsync();
foreach (var section in list.sections)
{
    foreach (var item in section.items)
    {
        if (item.disabled) continue;
        ShowStoreRow(section.sectionId, item);
    }
}
```

---

## 5. Combining wallet + store (end-to-end purchase flow)

Server **always** decides whether a purchase succeeds (currency deduction, limits, time gates). The client can use **`IVXWalletManager.CanAfford`** only as a **hint** for UI (for example graying out a button when cached soft currency is low). Authoritative costs and balances after purchase come from Hiro responses and/or a subsequent wallet refresh.

```csharp
using IntelliVerseX.Backend;
using IntelliVerseX.Hiro;
using UnityEngine;

public async void BuyStoreItem(string sectionId, IVXStoreItem item)
{
    long coinCost = item.cost.TryGetValue("coins", out var c) ? c : 0;
    if (coinCost > 0 && !IVXWalletManager.CanAfford((int)coinCost))
    {
        ShowNotEnoughCoins();
        return;
    }

    var store = IVXHiroCoordinator.Instance.Store;
    var result = await store.PurchaseAsync(sectionId, item.itemId);
    if (result == null)
    {
        ShowPurchaseFailed();
        return;
    }

    if (result.reward != null)
        ShowRewardPopup(result.reward);

    await IVXWalletManager.RefreshWalletsAsync();
}
```

---

## 6. IAP validation (Hiro `IVXBaseSystem`)

IAP flows through **`IVXBaseSystem`**, not `IVXWalletManager`. Access: `IVXHiroCoordinator.Instance.Base`.

| Method | Returns | Description |
|--------|---------|-------------|
| `ValidateIAPAsync(string receipt, string storeType, string productId, float? price = null, string currency = null, string gameId = null)` | `Task<IVXIAPValidateResponse>` | RPC `hiro_iap_validate`. Returns `null` on failure. |
| `GetPurchaseHistoryAsync(string gameId = null)` | `Task<IVXIAPHistoryResponse>` | RPC `hiro_iap_history`. On failure returns **`new IVXIAPHistoryResponse()`**. |

### Models

```csharp
public class IVXIAPValidateResponse
{
    public bool valid;
    public string productId;
    public string storeType;
    public IVXReward reward;
    public string transactionId;
}

public class IVXIAPPurchase
{
    public string productId;
    public string storeType;
    public float price;
    public string currency;
    public long purchasedAt;
    public string transactionId;
}

public class IVXIAPHistoryResponse
{
    public List<IVXIAPPurchase> purchases;
}
```

### Example: validate after platform purchase

```csharp
var baseSystem = IVXHiroCoordinator.Instance.Base;
var validation = await baseSystem.ValidateIAPAsync(
    receipt: platformReceipt,
    storeType: "googleplay", // or your server-expected discriminator
    productId: sku,
    price: localizedPrice,
    currency: isoCurrencyCode);

if (validation != null && validation.valid && validation.reward != null)
    ApplyRewardToUi(validation.reward);
```

---

## 7. Events and callbacks

### `IVXWalletManager` (static events)

| Event | Signature | Description |
|-------|-----------|-------------|
| `OnBalanceChanged` | `Action<int, int>` | Invoked with `(gameBalance, globalBalance)` when balances update (for example `AddCoins`, `UpdateBalances`). |
| `OnWalletError` | `Action<string>` | Invoked with error message when `RefreshWalletsAsync` catches an exception. |

`IVXEconomySystem` and `IVXStoreSystem` **do not declare public events** in the SDK sources; completion is signaled by the returned `Task` and response objects. Coordinator-level lifecycle remains `IVXHiroCoordinator.OnInitialized` (see [Hiro module](hiro.md)).

---

## 8. Error handling

### Wallet

- **`RefreshWalletsAsync`**: returns `false` if `HasWalletIds()` is false (logs a warning) or on exception (logs error, raises `OnWalletError`).
- **`AddCoins`**: does not surface errors via events; it updates cache and fires `OnBalanceChanged`.
- Prefer null/empty checks on any future Nakama-driven paths you add alongside `UpdateBalances`.

### Hiro economy and store

- **`RequestDonationAsync`**, **`GiveDonationAsync`**, **`ClaimDonationsAsync`**, **`CompleteRewardedVideoAsync`**, **`PurchaseAsync`**: return **`null`** when `HiroRpcResponse<T>.success` is false.
- **`ListAsync`**: returns **`new IVXStoreListResponse()`** when the RPC does not succeed (safe empty catalog).
- **`GetPurchaseHistoryAsync`**: returns **`new IVXIAPHistoryResponse()`** when the RPC does not succeed.

Pattern (matches [Hiro module](hiro.md)):

```csharp
var purchased = await IVXHiroCoordinator.Instance.Store.PurchaseAsync(sectionId, itemId);
if (purchased == null)
{
    Debug.LogWarning("Store purchase failed.");
    return;
}
```

For raw RPC envelopes, use `IVXHiroCoordinator.Instance.RpcClient.CallAsync<T>` and inspect `success` / `error`.

---

## 9. Cross-platform status

| Area | Notes |
|------|--------|
| **`IVXWalletManager`** | Unity-only static API; balances live in `IntelliVerseXIdentity`. No WebGL-specific restriction in the manager itself. **`RefreshWalletsAsync`** is the extension point for server sync once your Nakama client is connected on each platform. |
| **Hiro Economy / Store / Base** | Uses Nakama RPCs over the authenticated session. Available on any build target where your game initializes `IVXHiroCoordinator` with a valid `IClient` and `ISession` (see [Backend module](backend.md)). |
| **IAP** | Platform receipt strings and `storeType` values must match what your Hiro server validates (Google Play, App Store, etc.). |

---

## 10. RPC reference (wallet-related Hiro)

| RPC ID | System | Purpose |
|--------|--------|---------|
| `hiro_economy_donation_request` | Economy | Request donation state |
| `hiro_economy_donation_give` | Economy | Give to a donation |
| `hiro_economy_donation_claim` | Economy | Claim donation rewards |
| `hiro_economy_rewarded_video` | Economy | Rewarded video completion |
| `hiro_store_list` | Store | List store sections/items |
| `hiro_store_purchase` | Store | Purchase item |
| `hiro_iap_validate` | Base | Validate IAP receipt |
| `hiro_iap_history` | Base | Purchase history |

---

## 11. See also

- [Backend module](backend.md) — Nakama session and identity (wallet ids, auth)
- [Hiro module](hiro.md) — coordinator setup, `IVXHiroRpcClient`, shared patterns
- [Monetization module](monetization.md) — ad/IAP SDK integration at the game layer
- [Analytics module](analytics.md) — optional telemetry for economy events
