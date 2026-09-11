# TODO/FIXME Build Readiness — Status (v6.0.0 hardening)

**Updated:** 2026-09-11 (production hardening pass)

## Resolved in hardening pass

| Item | Resolution |
|------|------------|
| Apple/Google Sign In throws in login UI | **Finish-or-hide:** buttons default off; email uses Auth V2 `IVXAPIClient.LoginAsync` |
| Cognito email fallback to device | Wired to Auth V2 login |
| Wallet HTTP stub | Obsolete; canonical API is `IVXNWalletManager` |
| Paywall not implemented | `OnPaywallRequested` + optional `IVXPremiumGate` panel / purchase |
| Bootstrap always reports success | `IVXBootstrapStatus` Online/Offline/Partial/Failed + `OnBootstrapStatus` |
| Docs version drift | Synced install pins / matrix / SECURITY to **6.0.0** |
| CI skips Unity tests | Fail-closed without license; Unity 6 only |
| APIManager god-object | Split into partials (Core / Auth / Notes / Social) |
| Photon App ID in tooltips | Removed |
| Fixed secure-storage fallback keys | Install-scoped seed |

## Remaining (non-blocking / next)

| Item | Notes |
|------|-------|
| Quiz backend save | Still optional when `saveToBackend` |
| Prediction quiz / quiz cache | Still unfinished |
| Full AppleAuth credential callback in UI | Plugin + game wiring required |
| Dual-tree residual `_IntelliVerseXSDK` | SeasonPass / FortuneWheel — optional follow-up package extract |
| Non-Unity stub depth | Honest matrix published; deepen ports separately |

Validator path note: package lives under `Packages/com.intelliversex.sdk` (not `Assets/_IntelliVerseXSDK`).
