# Wallet Demo

This sample explains the **virtual wallet** flow in the Unity SDK: cached balances, refresh, affordance checks, UI binding, grants (server-side), store purchases, and how IAP ties into balances.

## Scene

`IVX_WalletTest.unity` (package sample). Add **`IVXWalletDisplay`** (or your UI) to show numeric balances from `IVXWalletManager`.

## Balance display

Balances are read from **`IntelliVerseXIdentity`** caches via static accessors:

| API | Purpose |
|-----|---------|
| `IVXWalletManager.GetGameBalance()` | Cached game-wallet coins (no network). |
| `IVXWalletManager.GetGlobalBalance()` | Cached global-wallet currency. |
| `IVXWalletManager.GetGameWalletId()` / `GetGlobalWalletId()` | Ids after `create_or_sync_user`. |
| `IVXWalletManager.HasWalletIds()` | Whether both ids are present. |

**UI pattern:**

```csharp
using IntelliVerseX.Backend;

void RefreshLabels()
{
    gameCoinsText.text = IVXWalletManager.GetGameBalance().ToString();
    globalGemsText.text = IVXWalletManager.GetGlobalBalance().ToString();
}

void OnEnable()
{
    IVXWalletManager.OnBalanceChanged += (game, global) => RefreshLabels();
    RefreshLabels();
}
```

## Currency grant

Server-authoritative grants typically go through **Hiro** or your Nakama RPC (e.g. `hiro_economy_grant` on other SDKs). On Unity, **`IVXWalletManager.AddCoins`** updates the **local** game balance for immediate UI feedback; you should reconcile with **`RefreshWalletsAsync`** after the server confirms.

```csharp
// Local-only convenience (e.g. dev cheat or optimistic UI)
IVXWalletManager.AddCoins(100, source: "level_reward");
```

For production, call your backend RPC, then **`IVXWalletManager.UpdateBalances`** (internal) is invoked from the Nakama client path when wallet payloads arrive—keep one source of truth on the server.

## Store purchase

1. Check **`IVXWalletManager.CanAfford(cost)`** or **`CanAfford(cost, useGlobal: true)`**.
2. Deduct via server RPC (recommended); avoid trusting client-only subtraction.
3. On success, refresh UI from `OnBalanceChanged`.

```csharp
if (!IVXWalletManager.CanAfford(itemPrice))
{
    ShowInsufficientFunds();
    return;
}
await PurchaseItemOnServerAsync(itemId);
await IVXWalletManager.RefreshWalletsAsync();
```

## IAP flow

In-app purchases (App Store / Play Billing) should:

1. Complete the platform IAP transaction.
2. Validate receipt server-side.
3. Credit wallet through your economy RPC.
4. Fire **`IVXWalletManager.OnBalanceChanged`** indirectly when identity caches update.

Hook **`IVXSubscriptionManager`** / your monetization module for subscription entitlements separately from soft currency.

## Events

| Event | Args | Use |
|-------|------|-----|
| `OnBalanceChanged` | `(int gameBalance, int globalBalance)` | Refresh HUD, shop labels. |
| `OnWalletError` | `string message` | Toasts / logging when refresh fails. |

## Troubleshooting

| Symptom | Check |
|---------|--------|
| Balances stuck at zero | `HasWalletIds()` — ensure auth + `create_or_sync_user` completed. |
| Refresh no-ops | `RefreshWalletsAsync` currently documents future Nakama wiring; verify your project’s client updates `UpdateBalances`. |

## See also

- [Monetization module](../modules/monetization.md)
- [Wallet integration guide](../guides/wallet-integration.md)
- [Hiro integration](../guides/hiro-integration.md)
