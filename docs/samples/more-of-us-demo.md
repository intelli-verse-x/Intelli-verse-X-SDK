# More Of Us Demo

Sample scene and prefabs for **cross-promotion**: a carousel of other apps from the same publisher, loaded from **hosted JSON catalogs**, with **store URLs** used as the install/open path on tap.

---

## Scene overview

**Typical location:** `IVX_MoreOfUs.unity` (imported into `Assets/Scenes/Tests/` via the SDK’s scene importer / consumer installer — see `IVXSceneImporter` and `IVXConsumerAssetInstaller`).

**Core UI type:** `IntelliVerseX.MoreOfUs.UI.IVXMoreOfUsCanvas`  
**Prefab names in tooling:** `IVXGMoreOfUsCanvas` (Setup Wizard), Resources path `IntelliVerseX/MoreOfUs/IVX_MoreOfUsCanvas` used by `IVXMoreOfUsCanvas.Create`.

This sample demonstrates:

- **Netflix-style horizontal carousel** of app cards (`IVXAppCard`).
- **Catalog fetch + cache** — Android and iOS JSON merged in `IVXMoreOfUsManager`.
- **Platform filtering** — only the current store’s apps are listed on device builds; Editor follows **active build target** (Android vs iOS).
- **Excluding the running app** — `IVXUnifiedAppInfo.IsCurrentApp()` compares `Application.identifier` to catalog `bundleId`.
- **Store deep links** — `OpenStorePage` raises `OnAppSelected` then calls `Application.OpenURL(storeUrl)` (Play Store / App Store URLs from JSON).

---

## How games are listed

1. **Configuration** — `IVXMoreOfUsConfig` on `IVXMoreOfUsManager` (or default instance) sets:
   - `androidCatalogUrl` / `iosCatalogUrl` — HTTP(S) endpoints returning catalog JSON.
   - `cacheDurationHours`, `enableOfflineCache`, `maxAppsToDisplay`, animation timings, optional editor-only `showBothPlatformsInEditor`.
2. **Fetch** — `IVXMoreOfUsManager.FetchCatalog` downloads both platform catalogs, maps entries to `IVXUnifiedAppInfo`, merges into `IVXMergedAppCatalog`, persists cache to disk (`ivx_app_catalog_cache.json`), and fires `OnCatalogLoaded`.
3. **UI population** — `IVXMoreOfUsCanvas` listens for `OnCatalogLoaded` / `OnLoadFailed`, calls `GetAppsForCurrentPlatform()`, and instantiates cards up to `maxAppsToDisplay`.
4. **Icons** — `LoadAppIcon` deduplicates in-flight loads and fills `RawImage` textures on each card.

!!! warning "Platform support in builds"
    On **Standalone / WebGL** builds the feature is **disabled** for end users (empty list / panel logic). **Android** and **iOS** are first-class. The **Editor** enables the panel for testing with a log explaining simulated behavior.

---

## “Deep link” configuration

In this module, “deep linking” means **store listing URLs**, not custom URI schemes:

| Field (unified model) | Meaning |
|----------------------|---------|
| `storeUrl` | Play Store or App Store URL opened on card tap |
| `appId` / `bundleId` | Package or bundle identifier; `bundleId` also drives `IsCurrentApp()` |
| `appIconUrl` | Remote icon for the card |

Ensure your catalog JSON supplies correct **`playStoreUrl`** (Android) and App Store URLs (iOS model) so `Application.OpenURL` lands on the right listing.

For custom attribution or reward callbacks, subscribe to **`IVXMoreOfUsManager.OnAppSelected`** (or `IVXMoreOfUsHelper.OnAppSelected`) and record analytics before or after the store opens.

---

## Key C# excerpts

### Helper entry points

`IVXMoreOfUsHelper` (`Assets/Intelli-verse-X-SDK/MoreOfUs/IVXMoreOfUsHelper.cs`) wraps show/hide/toggle and optional preload:

```csharp
IVXMoreOfUsHelper.Show();
IVXMoreOfUsHelper.PreloadData();
```

### Creating the canvas from code

```csharp
var canvas = IVXMoreOfUsCanvas.Create(parentTransform);
// or find an instance already in the scene
```

### Opening the store

```csharp
public void OpenStorePage(IVXUnifiedAppInfo appInfo)
{
    if (appInfo == null || string.IsNullOrEmpty(appInfo.storeUrl)) return;
    OnAppSelected?.Invoke(appInfo);
    Application.OpenURL(appInfo.storeUrl);
}
```

---

## Scene / prefab setup

1. Add **`IVXMoreOfUsManager`** to a bootstrap object (or rely on lazy `Instance` creation — the manager can auto-create a `DontDestroyOnLoad` object).
2. Assign or override **`IVXMoreOfUsConfig`** URLs for your publisher’s catalogs.
3. Place **`IVXMoreOfUsCanvas`** (or `IVXGMoreOfUsCanvas` prefab) in the scene **or** spawn it via `Create()` / `IVXMoreOfUsHelper`.
4. Use the **SDK Setup Wizard** More Of Us tab for guided prefab placement when working inside the Unity Editor.

---

## How to run

1. Import the **More Of Us** scene and prefabs if they are not already in the project.
2. Set build target to **Android** or **iOS** (or stay in Editor with that target) so `GetAppsForCurrentPlatform()` returns data.
3. Enter Play Mode; confirm catalog fetch logs and populated cards.
4. Tap a card — the device should open the browser or store client for the configured URL.

---

## Customization

- **Card layout** — Adjust spacing and animation in `IVXMoreOfUsCanvas` / `IVXAppCard` and config fields (`cardAnimationDuration`, `autoScrollInterval`).
- **Catalog source** — Point `androidCatalogUrl` / `iosCatalogUrl` to your own JSON buckets (same schema as the default IntelliVerseX catalogs).
- **Styling** — Treat the canvas as a starting layout; swap sprites, fonts, and colors to match your game (see [Theming guide](../guides/theming.md)).

---

## See also

- [More Of Us module](../modules/more-of-us.md)
- [Platform module](../modules/platform.md)
- [Home screen / test hub](./home-screen-demo.md)
- [Theming & UI customization](../guides/theming.md)
