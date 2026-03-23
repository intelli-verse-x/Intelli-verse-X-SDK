# Itch.io publishing — IntelliVerseX SDK (8 platforms)

This document mirrors the **plan → execute → manual follow-up** workflow used for GitHub and Nakama community publishing.

## Scope (8 SDKs)

| # | Itch.io draft title (suggested) | Source folder in repo |
|---|----------------------------------|------------------------|
| 1 | IntelliVerseX Unity SDK | `Assets/Intelli-verse-X-SDK/` |
| 2 | IntelliVerseX Unreal SDK | `SDKs/unreal/` |
| 3 | IntelliVerseX Godot SDK | `SDKs/godot/` |
| 4 | IntelliVerseX Defold SDK | `SDKs/defold/` |
| 5 | IntelliVerseX Cocos2d-x SDK | `SDKs/cocos2dx/` |
| 6 | IntelliVerseX JavaScript SDK | `SDKs/javascript/` |
| 7 | IntelliVerseX C++ SDK | `SDKs/cpp/` |
| 8 | IntelliVerseX Java SDK | `SDKs/java/` |

**Excluded (in development):** Web3, Flutter — same as other publications.

## Task checklist

| Step | Owner | Status |
|------|--------|--------|
| Create / polish developer profile on itch.io | You | Manual |
| Create 8 project pages (drafts → publish) | You | Manual |
| Build per-platform zip bundles | Script + you run locally | `tools/scripts/package-itch-bundles.ps1` |
| Upload zips + set price (e.g. free / pay what you want) | You | Manual |
| Paste page copy from `itch-io-sdk-page.md` | You | Manual |
| Add screenshots (see below) | You | Manual |
| Link docs + GitHub + release | In template | You paste |

## Screenshots (manual)

itch.io expects **visuals** even for tools. Add **at least one** per project:

| Platform | Suggested screenshot |
|----------|----------------------|
| Unity | Package Manager or Project window showing `Intelli-verse-X-SDK` |
| Unreal | Plugins folder + IntelliVerseX enabled in editor |
| Godot | FileSystem with `addons/intelliversex` |
| Defold | Assets tree with `intelliversex` |
| Cocos2d-x | IDE or folder structure in Explorer |
| JavaScript | VS Code + `package.json` / `src/` |
| C++ | VS Code / CMake + `include/intelliversex` |
| Java | Android Studio / Gradle tree |

Use **16:9** or itch’s recommended aspect ratio; PNG or JPG.

## Packaging

From repo root (Windows PowerShell):

```powershell
.\tools\scripts\package-itch-bundles.ps1 -Version 5.2.0
```

Output: `dist/itch/<version>/*.zip` — upload each zip to the matching itch.io project as the **primary file** (or attach as additional files).

## Links (use everywhere)

- **Repository:** https://github.com/intelli-verse-x/Intelli-verse-X-SDK  
- **Docs (MkDocs):** use your live docs URL if published (see root `README.md` badge)  
- **Release (zips mirror):** https://github.com/intelli-verse-x/Intelli-verse-X-SDK/releases/tag/v5.2.0  

## After publishing

- [ ] Smoke-test each download from itch (download → unzip → matches README expectations).
- [ ] Optional: add itch.io links back into root `README.md` in a “Also on itch.io” section (separate PR).
