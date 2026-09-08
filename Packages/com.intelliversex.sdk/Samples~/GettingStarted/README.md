# Getting Started Sample

Honest status for SDK **6.0.0**: this sample currently ships **scripts only**.

## What you get

- `Scripts/IVXGettingStartedDemo.cs` — demo glue (legacy identity helpers may appear; prefer Bootstrap)
- `Scripts/IVXGettingStartedUI.cs` — simple status UI hooks

There is **no** `Scenes/GettingStartedScene.unity` or `Prefabs/IVXDemoManager.prefab` in this folder yet.

## Recommended first-run (everyone)

1. Install `com.intelliversex.sdk`
2. Open **IntelliVerseX → Control Center**
3. Paste **Game ID** → **Add bootstrap to this scene** → Play
4. For richer demos, import the **Test Scenes** sample from Package Manager

## Optional: use these scripts

Attach `IVXGettingStartedDemo` / `IVXGettingStartedUI` to a scene that already has `IVXBootstrap` + `IVXBootstrapConfig`. Do not treat device-only init as the canonical path.

## Requirements

- Unity **6000.3** (see package `unity` field)
- Newtonsoft.Json (UPM dependency)
- Nakama Unity package for backend features
