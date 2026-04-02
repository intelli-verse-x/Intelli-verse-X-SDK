# UI Components Demo

The IntelliVerseX SDK does **not** ship a single `IVX_UI.unity` “gallery” scene. Instead, UI behavior is demonstrated **across samples and reusable types**: shared primitives in `IVXUIComponents`, game-level shell navigation in `IVXUIManager`, resolution helpers, and richer patterns inside feature modules (friends, clans, leaderboard).

This page is the **conceptual UI demo**: what exists, where to look, and how to compose it in your game.

---

## What the SDK provides

| Area | Types / locations | Typical use |
|------|-------------------|-------------|
| **Loading feedback** | `IVXLoadingSpinner` in `IVXUIComponents.cs` | Full-screen or panel spinner with optional animated “…” text |
| **Modal confirmation** | `IVXConfirmDialog` in `IVXUIComponents.cs` | Title, message, confirm/cancel with `Action<bool>` callback |
| **Shell UI / popups** | `IVXUIManager<T>` in `IVXUIManager.cs` | Register panels; `ShowPanel` / `ShowPopup` / history stack |
| **Resolution** | `IVXUIResolutionHandler.cs`, V2 `UI_ResolutionHandler` | Scale / safe-area style handling for different devices |
| **Toasts / inline errors** | e.g. `IVXFriendsPanel.ShowToast` | Lightweight transient messages on social prefabs |
| **Modal roots** | Clan / friends scene builders use `ModalRoot`, `ToastRoot` | Layout convention for stacking dialogs toasts above gameplay UI |
| **Optional tabs** | `IVXLeaderboardUI` tab buttons | Switch leaderboard views without a separate prefab system |
| **Quiz-specific** | `IVXQuizUIComponents`, `IVXExplanationModal` | In-quiz overlays and explanation dialogs |

!!! note "Toasts and modals in `IVXUIComponents`"
    The file `IVXUIComponents.cs` currently contains **`IVXLoadingSpinner`** and **`IVXConfirmDialog`**. Other modules implement **toasts** and **extra modals** on their own panels; reuse those patterns or wrap them behind your `IVXUIManager` subclass.

---

## `IVXUIComponents` — spinner

Path: `Assets/Intelli-verse-X-SDK/UI/IVXUIComponents.cs`

- Attach to a GameObject that references a **panel** root, optional **rotating RectTransform** icon, and optional **TMP** label.
- Call `Show(string text = null)` / `Hide()` from your async flows.

```csharp
// Conceptual usage after wiring references in the Inspector
loadingSpinner.Show("Syncing profile…");
// … await work …
loadingSpinner.Hide();
```

Spinner rotation runs in `Update`; keep heavy work off the main thread where possible, then toggle visibility when results arrive.

---

## `IVXUIComponents` — confirm dialog

Same file — `IVXConfirmDialog`:

```csharp
confirmDialog.Show(
    "Discard unsaved changes?",
    confirmed =>
    {
        if (confirmed) { /* discard */ }
        else { /* stay */ }
    },
    title: "Discard?",
    confirmText: "Discard",
    cancelText: "Keep editing");
```

The dialog hides itself before invoking the callback; avoid showing a second modal synchronously inside the callback if your canvas sort order is ambiguous.

---

## `IVXUIManager<T>` — panels and popups

Path: `Assets/Intelli-verse-X-SDK/UI/IVXUIManager.cs`

Subclass per game for a **type-safe singleton** (`YourGameUIManager.Instance`). Intended responsibilities:

- **Panel stack / history** — optional back navigation (`trackNavigationHistory`, `maxHistorySize`).
- **Popups** — one active popup at a time conceptually; hide before opening another from gameplay code.
- **Events** — subscribe to panel/popup change events for analytics or input routing.

This is the right place to unify **tabs** at the app level: each “tab” can be a registered panel name, even if individual SDK widgets (like leaderboard) expose their own tab buttons.

---

## Theming

SDK screens are **uGUI + TextMeshPro**. There is no single global skin; you override prefabs and drive colors/fonts from a **ScriptableObject** or your existing style system.

See the step-by-step patterns (palette asset, applying colors at runtime) in:

- [Theming & UI customization guide](../guides/theming.md)

---

## How to “run” the UI demo

1. Open any test scene that already embeds SDK UI (e.g. **Friends**, **Clan**, **Leaderboard**, **Profile**) from the [home screen hub](./home-screen-demo.md).
2. For **spinner / confirm** in isolation, drop the prefabs or a minimal Canvas into an empty scene, wire `IVXLoadingSpinner` / `IVXConfirmDialog` fields, and invoke `Show` / `Hide` from a temporary test button.
3. For **shell navigation**, add your `IVXUIManager` subclass to the scene and register panel roots matching your game.

---

## Customization tips

- **Sorting layers** — Give SDK overlays a dedicated canvas **sorting order** so toasts and modals always draw above gameplay HUD.
- **Input** — Ensure one **EventSystem**; mix of Input System and legacy input is handled in test navigators — mirror that in your root scene.
- **Accessibility** — TMP supports font scaling; centralize font sizes through your theme asset (see theming guide).

---

## See also

- [UI module](../modules/ui.md)
- [Theming guide](../guides/theming.md)
- [Home screen / test hub](./home-screen-demo.md)
- [Leaderboards module](../modules/leaderboards.md) — optional tab UI pattern
