# Repo layout: `SDKs/<platform>/` + Unity UPM at `Packages/`

**Status (P4):** UPM product is `Packages/com.intelliversex.sdk`. Open the Hub sandbox at `SDKs/unity/editor`. Do **not** open the git root as a Unity project.  
**Related:** [ADR-002](adr/ADR-002-dual-tree-layout.md), [UNITY_SDK_REVAMP_PLAN.md](UNITY_SDK_REVAMP_PLAN.md), [OPTIONAL_MODULES.md](../OPTIONAL_MODULES.md)

Every engine lives under **one parent**: `SDKs/`. Unity is special only because the **shippable package** sits at the UPM-standard repo path `Packages/com.intelliversex.sdk`, while the **sandbox Editor project** stays under `SDKs/unity/editor`.

---

## Current shape

```text
Intelli-verse-X-SDK/                 git root — docs, CI, tools, UPM package
  README.md
  docs/
  tools/
  .github/
  Packages/
    com.intelliversex.sdk/           UPM package (what GitHub installs)
      package.json                   name: com.intelliversex.sdk
      Runtime modules / Editor / Samples~ / Tests~
  SDKs/
    unity/
      editor/                        Unity Hub project (you open this)
        Assets/                      Photon, scenes, vendors — not in package
        Packages/manifest.json       file:../../../../Packages/com.intelliversex.sdk
        ProjectSettings/
    javascript/                      npm @intelliversex/sdk
    web3/
    unreal/
    godot/
    flutter/
    java/
    cpp/
    defold/
    roblox/
    cocos2dx/
    visionos/
```

**How to read it:** consumers install only the UPM folder. Contributors open `SDKs/unity/editor` to dogfood the package via a local `file:` dependency.

---

## GitHub install (one path per platform)

| Platform | Install | Update |
|----------|---------|--------|
| **Unity** | `"com.intelliversex.sdk": "https://github.com/Intelli-verse-X/Intelli-verse-X-SDK.git?path=Packages/com.intelliversex.sdk#v5.10.0"` | bump `#vX.Y.Z` |
| **JavaScript** | `npm install @intelliversex/sdk` (publish from `SDKs/javascript`) | `npm update` |
| **Flutter** | `pubspec.yaml` git url + `path: SDKs/flutter` | bump git `ref` |
| **Unreal** | add `SDKs/unreal` as a plugin | pull |
| **Godot** | copy `SDKs/godot` addon into `res://addons/` | recopy / submodule |

npm has no Unity `?path=`. Do not `npm install` the whole repo.

Blank Unity **6000.3** project path: add the git URL above → **IntelliVerseX → Control Center** → paste Game ID → Play.

---

## What not to do

- Do not put Photon Asset Store copies, ads vendor trees, or `tools/` inside `Packages/com.intelliversex.sdk`.
- Do not GUID-merge dual trees (`Intelli-verse-X-SDK` vs `_IntelliVerseXSDK`) in the same change as a path move.
- Do not make the **git root** itself the only Unity package root in a way that hides other engines (keep non-Unity SDKs under `SDKs/`).
- Do not open `Packages/com.intelliversex.sdk` alone as a Hub project — open `SDKs/unity/editor`.
