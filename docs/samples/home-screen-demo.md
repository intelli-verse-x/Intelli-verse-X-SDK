# Home Screen Demo

Sample scene that acts as the **SDK test hub**: a minimal world plus a runtime-built navigation overlay for jumping between IntelliVerseX validation scenes.

---

## Scene overview

**Primary location:** `Assets/Scenes/Tests/IVX_HomeScreen.unity`  

**Package sample copy:** `Assets/Intelli-verse-X-SDK/Samples~/TestScenes/IVX_Homescreen.unity` (same role when consumed via UPM samples)

This sample demonstrates:

- **SDK “main menu” for QA** — one place to open Auth, Ads, Leaderboard, Wallet, Quiz, Profile, Friends, Clan, More Of Us, Share/Rate, and other scenes under `Assets/Scenes/Tests/`.
- **Quick-access buttons** — each available scene becomes a labeled button; the current scene is skipped so you never navigate to yourself.
- **Implicit module wiring hints** — only scenes that exist in **Build Settings** (or, in the Editor, under `Assets/Scenes/Tests/`) appear, which helps you see what is actually wired for a given project.
- **Return-to-home** — from any feature test scene, a **Home** button jumps back to `IVX_HomeScreen` (configurable).

!!! tip "Overlay toggle"
    Press **F8** (default) to show or hide the navigation canvas while Play Mode is running.

---

## Scene hierarchy (root objects)

The saved scene is intentionally small. At runtime, `IVXTestSceneNavigator` creates the menu UI under a new canvas.

**On disk (YAML):**

```
IVX_HomeScreen
├── IVX_TestSceneNavigator   ← IntelliVerseX.Core.IVXTestSceneNavigator
└── Main Camera
```

**After `Awake()` (runtime, conceptual):**

```
IVX_TestSceneNavigator
└── IVX_TestSceneMenuCanvas (Screen Space Overlay, sorting order 500)
    └── Panel
        ├── Labels ("IVX Test Scenes", "Current: …")
        └── Buttons… (Home when not on home, then one per available scene link)
```

The navigator also ensures an **EventSystem** exists, preferring **Input System**’s UI module when the package is present, otherwise **StandaloneInputModule**.

---

## Key script

### `IVXTestSceneNavigator.cs`

Path: `Assets/Intelli-verse-X-SDK/Core/IVXTestSceneNavigator.cs`

Default quick links (Auth through Share/Rate) are serialized in `_sceneLinks`. Additional links are merged from Build Settings and from `Assets/Scenes/Tests/*.unity` in the Editor:

```csharp
[SerializeField] private string _homeSceneName = "IVX_HomeScreen";
[SerializeField] private KeyCode _toggleOverlayKey = KeyCode.F8;

// Excerpt: default feature entries (see full list in source)
new SceneLink { Label = "Auth",        SceneName = "IVX_AuthTest",        ShowOnHome = true, ShowOnFeatureScenes = true },
new SceneLink { Label = "Profile",     SceneName = "IVX_Profile",         ShowOnHome = true, ShowOnFeatureScenes = true },
new SceneLink { Label = "More Of Us",  SceneName = "IVX_MoreOfUs",        ShowOnHome = true, ShowOnFeatureScenes = true },
```

Scene load behavior:

- **Player / device builds:** `SceneManager.LoadScene(name)` when `Application.CanStreamedLevelBeLoaded` is true.
- **Editor:** can load `Assets/Scenes/Tests/{name}.unity` via `EditorSceneManager.LoadSceneInPlayMode` even if the scene is not in Build Settings (see `TryEditorLoadScene`).

---

## How to run

1. Add `IVX_HomeScreen` and the test scenes you need to **File → Build Settings** (recommended for standalone testing), **or** rely on Editor-only loading from `Assets/Scenes/Tests/`.
2. Open `IVX_HomeScreen.unity`.
3. Press **Play**.
4. Use the overlay buttons to open a module sample; use **Home** or reload the home scene to return.

---

## Customization

| Goal | What to change |
|------|----------------|
| Home scene name | `_homeSceneName` on `IVXTestSceneNavigator` |
| Toggle key | `_toggleOverlayKey` |
| Which links appear on home vs. feature scenes | Each `SceneLink`’s `ShowOnHome` / `ShowOnFeatureScenes` |
| Panel position / colors | Adjust `BuildNavigationUI()` (panel anchor, `Image.color`, button styling) |
| Extra scenes | Add `SceneLink` entries or place `.unity` files under `Assets/Scenes/Tests/` |

!!! note "Not a production main menu"
    This overlay is for **SDK validation**. Ship builds should replace or hide it and use your game’s own shell UI.

---

## See also

- [Core module](../modules/core.md)
- [Demos index](../modules/demos.md)
- [Authentication sample](./auth-demo.md) — typical first stop from the hub
