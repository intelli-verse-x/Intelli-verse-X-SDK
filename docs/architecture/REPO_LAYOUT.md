# Repo layout: `SDKs/<platform>/`

**Status:** Hub project and UPM package live under `SDKs/unity/`. Open Hub at `SDKs/unity/editor`. Do not open the git root as a Unity project.  
**Related:** [ADR-002](adr/ADR-002-dual-tree-layout.md), [UNITY_SDK_REVAMP_PLAN.md](UNITY_SDK_REVAMP_PLAN.md)

Every engine lives under **one parent**: `SDKs/`. Unity is not special at git root. It is `SDKs/unity/`, next to `SDKs/javascript/` and `SDKs/unreal/`.

---

## Before this change

```text
Intelli-verse-X-SDK/          ← git root looks like a Unity game
  Assets/                     ← Unity project
  Packages/
  ProjectSettings/
  SDKs/
    javascript/               ← other engines hidden here
    unreal/
    godot/
    …
```

Unity sat **outside** `SDKs/`. GitHub UPM used `?path=Assets/Intelli-verse-X-SDK` and **missed** Bootstrap in `_IntelliVerseXSDK`.

---

## Target (same shape for every platform)

```text
Intelli-verse-X-SDK/                 git root — docs, CI, tools only
  README.md
  docs/
  tools/
  .github/
  SDKs/
    unity/
      sdk/                           UPM package (what GitHub installs)
        package.json                 name: com.intelliversex.sdk
        Runtime/
        Editor/
        Samples~/
      editor/                        Unity Hub project (you open this)
        Assets/                      Photon, scenes, vendors — not in sdk/
        Packages/manifest.json       file:../sdk
        ProjectSettings/
    javascript/                      already here — npm @intelliversex/sdk
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

**How to read it:** `SDKs/<engine>/` is that engine’s SDK. Unity has two subfolders because Unity needs a **package** (GitHub) and an **Editor project** (Hub). Other engines only need the package folder.

---

## GitHub install (one path per platform)

| Platform | Install | Update |
|----------|---------|--------|
| **Unity** | `"com.intelliversex.sdk": "https://github.com/Intelli-verse-X/Intelli-verse-X-SDK.git?path=SDKs/unity/sdk#v5.9.0"` | bump `#vX.Y.Z` |
| **JavaScript** | `npm install @intelliversex/sdk` (publish from `SDKs/javascript`) | `npm update` |
| **Flutter** | `pubspec.yaml` git url + `path: SDKs/flutter` | bump git `ref` |
| **Unreal** | add `SDKs/unreal` as a plugin | pull |
| **Godot** | copy `SDKs/godot` addon into `res://addons/` | recopy / submodule |

npm has no Unity `?path=`. Do not `npm install` the whole repo.

---

## How to move (when the Editor is closed)

Do this in **one dedicated PR**. Close Unity Hub / Editor first. Do not drag folders in Explorer while Pipeline is connected.

1. Create `SDKs/unity/sdk/` and copy **only** shippable SDK code (`Intelli-verse-X-SDK` + `_IntelliVerseXSDK` public API, `package.json`). This is the GitHub UPM folder. Do not GUID-merge the two trees in this step.
2. Create `SDKs/unity/editor/` and **git mv** the current Unity project into it: `Assets`, `Packages`, `ProjectSettings`. Leave `Library/`, `Temp/`, `Logs/` behind (gitignored; Unity recreates them).
3. Point `SDKs/unity/editor/Packages/manifest.json` at the package:

   ```json
   "com.intelliversex.sdk": "file:../sdk"
   ```

   Then remove duplicate SDK copies from `editor/Assets/` once the file: reference compiles (second PR).
4. Open Hub → **Add** → `…/Intelli-verse-X-SDK/SDKs/unity/editor`.
5. Change README install to `?path=SDKs/unity/sdk`.
6. Leave existing `SDKs/javascript`, `unreal`, … where they are. They are already in the right parent.

Until step 1 exists, the public Git URL is still incomplete.

---

## What not to do

- Do not put Photon, ads vendor copies, or `tools/` inside `SDKs/unity/sdk`.
- Do not `git mv` `Assets/` while this Editor is running.
- Do not make the **git root** the Unity package (that hides every other engine again).
