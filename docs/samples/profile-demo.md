# Profile Demo

Sample flows for **Nakama-backed player profile**: load snapshot, edit fields, save, refresh, and inspect **portfolio** data (including per-game and global wallet balances in the sample UI).

---

## Scene overview

**Primary location:** `Assets/Scenes/Tests/IVX_Profile.unity`  

**Package sample copy:** `Assets/Intelli-verse-X-SDK/Samples~/TestScenes/IVX_Profile.unity`

This sample demonstrates:

- **Profile display** — avatar, display name, username-style fields, user id, email-style readouts, location (city/country), and **PlayerStats** rows (wins, losses, totals, win rate).
- **View vs. edit** — `ViewModePanel` vs. `EditModePanel` with Save / Cancel.
- **Async state** — `LoadingOverlay`, `StatusText`, and busy guarding while Nakama initializes.
- **Portfolio / wallet hints** — demo scripts can show `GlobalWalletBalance` and per-game `WalletBalance` from `IVXNProfileManager` portfolio snapshots.

---

## Scene hierarchy (high level)

```
IVX_Profile
├── Main Camera
├── Directional Light
├── IVXNManager                          ← Nakama session / backend wiring
├── ProfileCanvas
│   └── ProfilePanel
│       ├── Background
│       ├── HeaderSection
│       │   └── AvatarContainer / AvatarImage
│       ├── Scroll View → Content
│       │   ├── UserInfoContainer (UserId*, DisplayName*, Email*, Location*, names…)
│       │   ├── PlayerStatsPanel (WinsRow, LossesRow, TotalGamesRow, WinRateRow…)
│       │   └── StatusSection / StatusText
│       ├── ViewModePanel / EditModePanel (input fields, placeholders)
│       └── ButtonsSection (Refresh, Edit, Save, Cancel)
├── LoadingOverlay / LoadingText
└── EventSystem
```

Exact object names may vary slightly between branches; search the scene for `ProfileCanvas`, `IVXNManager`, and `PlayerStatsPanel` if you need to re-wire references.

---

## Source scripts

| Script | Path | Role |
|--------|------|------|
| `IVXProfileDemoController` | `Assets/Intelli-verse-X-SDK/Samples~/TestScenes/Scripts/IVXProfileDemoController.cs` | IMGUI-oriented sample: bootstrap Nakama, refresh profile, save, portfolio button |
| `IVXProfileDemoView` | Same folder | View + `OnGUI` form; builds `IVXNProfileUpdateRequest`; avatar preview via `UnityWebRequestTexture` |
| `IVXProfileDemoMocks` | Same folder | Mock profile/portfolio when backend is unavailable |
| `IVXProfileSceneDemoController` | `Assets/Intelli-verse-X-SDK/Examples/IVXProfileSceneDemoController.cs` | **Runtime-built Canvas** (TMP) with validation regexes for names, locale, country code |
| `IVXProfileView` / `IVXProfileController` | `Assets/Intelli-verse-X-SDK/V2/UI/Profile/` | Reusable V2 profile UI wiring (for integration beyond the test scene) |

---

## Key C# excerpts

### Controller workflow (test scene)

`IVXProfileDemoController` subscribes to `IVXNProfileManager` events and coordinates refresh/save:

```csharp
IVXNProfileManager.OnProfileLoaded += HandleProfileLoaded;
IVXNProfileManager.OnProfileUpdated += HandleProfileUpdated;
IVXNProfileManager.OnProfileError += HandleProfileError;

// Bootstrap: optional Nakama init, then refresh
await manager.InitializeForCurrentUserAsync();
await RefreshProfileAsync(cancellationToken);
```

### Building an update request from the view

```csharp
public IVXNProfileManager.IVXNProfileUpdateRequest BuildUpdateRequest()
{
    return new IVXNProfileManager.IVXNProfileUpdateRequest
    {
        FirstName = firstName,
        LastName = lastName,
        City = city,
        Region = region,
        Country = country,
        CountryCode = countryCode,
        Locale = locale,
        AvatarUrl = avatarUrl
    };
}
```

### Portfolio snapshot (includes wallet fields)

```csharp
sb.AppendLine($"GlobalWalletBalance: {portfolio.GlobalWalletBalance}");
// Per game: wallet={game.WalletBalance}
```

---

## How to run

1. Ensure **Nakama** / `IVXNManager` settings match your environment (same expectations as other backend samples).
2. Open `IVX_Profile.unity`.
3. Press **Play**.
4. Use **Refresh** to pull the latest profile; use **Edit** → change fields → **Save**; use **Portfolio** (or equivalent control in your wired UI) to fetch portfolio data where exposed.

If initialization fails, enable **mock fallback** on the demo controller components to inspect UI without a live session.

---

## Editing profile (behavior)

- **Validation** — `IVXProfileSceneDemoController` applies regex rules for display name, locale, and country code before save; align your production UI with the same constraints to avoid server rejection.
- **Avatar** — URL-based preview in demos; invalid or non-http(s) URLs show a clear status string instead of failing silently.
- **Events** — Prefer reacting to `OnProfileLoaded` / `OnProfileUpdated` / `OnProfileError` rather than polling, so multiple UI surfaces stay in sync.

---

## See also

- [Identity module](../modules/identity.md)
- [Backend / Nakama](../modules/backend.md)
- [Wallet module](../modules/wallet.md) — economy APIs beyond the profile portfolio readout
- [Authentication sample](./auth-demo.md) — sign-in before profile-backed flows
