# Wallet Demo

This sample demonstrates the virtual wallet system.

## Scene

`IVX_WalletTest.unity`

## Features

- Multiple currency support
- Transaction history
- Secure balance management
- IAP integration

## Code Example

```csharp
using IntelliVerseX.Monetization;

// Get balance
var coins = IVXWalletManager.Instance.GetBalance("coins");

// Add currency
await IVXWalletManager.Instance.AddCurrencyAsync("coins", 100);
```

## See Also

- [Monetization Module](../modules/monetization.md)
