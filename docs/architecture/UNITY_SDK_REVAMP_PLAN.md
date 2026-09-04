# Unity SDK Revamp Plan

**Scope:** `Intelli-verse-X-SDK` Unity project only (not Nakama Go, not other engine ports).  
**Method:** Unity CLI (`projects info`, `projects size`, `status`) + asset inventory. Editor was **not** running; Pipeline is **not** installed, so live `unity command` was unavailable.  
**Editor:** `6000.3.6f1` (matches `ProjectVersion.txt`).  
**Companion:** [SYSTEM_DESIGN.md](SYSTEM_DESIGN.md), [WORLD_CLASS_CONTROL_PLANE.md](WORLD_CLASS_CONTROL_PLANE.md), existing [UPM_PACKAGE_TRANSFORMATION_PLAN.md](../UPM_PACKAGE_TRANSFORMATION_PLAN.md).

This repo is a **full Unity sandbox** pretending to be a **shippable SDK**. That is why it feels like too many extra and bad scripts. The revamp is: **split sandbox from package**, **one facade per domain**, **one editor window**.

---

## 1. What the CLI actually measured

| Fact | Number |
|------|--------|
| Project size (no Library) | **~335 MB** (`unity projects size`) |
| `tools/` | **171 MB** — almost all `tools/boilerplate` |
| `Assets/` | **~80 MB** |
| Photon under Assets | **973 files, 217 C#, 35.5 MB** |
| TextMesh Pro (incl. examples) | **366 files, 9.5 MB** |
| Legacy SDK tree | **255 C#** in `Assets/Intelli-verse-X-SDK` |
| New SDK tree | **108 C#** in `Assets/_IntelliVerseXSDK` |
| `IVXSDKSetupWizard.cs` | **4,153 lines** |
| `*Manager*.cs` in IVX | **~50 managers** |
| Pipeline / live Editor | **0 instances** — cannot `unity test` or `eval` until `unity pipeline install` + Editor open |

`package.json` still says Unity **2023.3**; the project is **6000.3.6f1**. That mismatch is a consumer foot-gun.

---

## 2. Diagnosis: three products in one folder

```text
A. Shippable UPM SDK     (should be small: Runtime + Editor + Samples~)
B. Internal sandbox game (Photon, Appodeal copies, TMP examples, Scenes, VSCode)
C. Company monorepo      (SDKs/ for 10 engines, 171 MB boilerplate, graphify, .cursor)
```

A consumer who adds `com.intelliversex.sdk` should get **A**. Today they inherit **A+B** and the git repo is **A+B+C**.

### Assets/ that do not belong in a library package

| Folder | Verdict |
|--------|---------|
| `Photon/` | **Sandbox / optional extra.** Do not ship inside the core UPM. |
| `Appodeal/`, `LevelPlay/`, `ApplixirSDK/` | **Vendor copies + three ad stacks.** Pick **one** mediation for the core package; others as optional samples. |
| `AppleAuthSample/` | Sample → `Samples~` |
| `TextMesh Pro/Examples & Extras` | **Delete from SDK repo** (TMP itself via UPM) |
| `VSCode/` | Editor convenience, not SDK |
| `AuthBackend/` | Likely game-specific — prove or move out |
| `Sych/` | Third-party share helper; optional dep |
| `Scenes/`, `Resources/` at Assets root | Sandbox |
| `Editor/` at Assets root | Ambiguous — audit vs `Intelli-verse-X-SDK/Editor` |
| `IntelliVerseX/` | Generated consumer output — gitignore in the SDK repo |
| `MobileDependencyResolver/` | EDM; should come from UPM tarball, not a second copy if possible |
| `Nakama/` | Prefer **UPM git URL**, not a vendored tree (keep until UPM pin is proven) |

### Packages in `manifest.json` that are game/editor toys, not SDK

Remove from the **package** (keep only in a `sandbox/` Unity project if needed):

- `com.coplaydev.unity-mcp` (CLI-first policy)
- `com.unity.2d.animation`, `aseprite`, `psdimporter`, `spriteshape`, `tilemap.extras`
- `com.unity.visualscripting`
- `com.unity.multiplayer.center`
- `com.unity.xr.management` unless an XR **sample** is first-class
- `com.boxqkrtm.ide.cursor` — fine for this repo, not for consumers

Keep for the SDK: Newtonsoft, Input System, uGUI, Purchasing, Test Framework, **one** ads package, Nakama.

---

## 3. Duplicate / bad scripts (Unity tree)

These are the “too much” the user is feeling. **Do not delete in one PR** — deprecate with `[Obsolete]` then remove after a minor version.

### 3.1 Two SDKs

| Keep (canonical) | Deprecate |
|------------------|-----------|
| `_IntelliVerseXSDK/Bootstrap/IVXBootstrap` + `IVXBootstrapConfig` | `Core/IntelliVerseXManager` as the init path |
| `V2/Manager/IVXNManager` as **the** Nakama client | Parallel use of `IntelliVerseXConfig` **and** BootstrapConfig |
| `_IntelliVerseXSDK` modules for new live-ops | Copying the same feature into the legacy tree |

ADR-002 said “keep dual tree.” For **revamp**, dual tree stays on disk until UPM extract, but **only one public init API**.

### 3.2 Two of everything

| Domain | Collision | Revamp |
|--------|-----------|--------|
| Wallet | `IVXWalletManager` vs `IVXNWalletManager` | **IVXNWalletManager** + RequestBus; wrap or obsolete the old one |
| Leaderboard | `IVXGLeaderboardManager` vs `IVXNLeaderbordManager` (typo in name) | One type, rename typo in a **major** or keep alias |
| Multiplayer | `Core/IVXMultiplayerManager` vs `_IntelliVerseXSDK/Multiplayer/Core/*` | New tree wins; old becomes a thin wrapper or obsolete |
| Friends models | `Social/Runtime/IVXFriendsModels.cs` **and** `Social/Friends/IVXFriendsModels.cs` | Merge to one namespace |
| UI scaler | `V2/UI/UI_ResolutionHandler.cs` vs `UI/IVXUIResolutionHandler.cs` | One |
| Satori | Only in legacy tree today — good; don’t add a second |
| Ads | LevelPlay UPM + Appodeal UPM + Applixir Assets + waterfall manager | **One adapter interface**, two optional implementations |
| Editor setup | Control Center + SetupWizard + AutoSetup + FeatureSetup + 3 dependency scripts + ProjectSetup | **Control Center + one Advanced window** |

### 3.3 Editor pile (keep / fold / drop)

| File | Lines | Action |
|------|------:|--------|
| `IVXControlCenter.cs` | ~300 | **Keep — grow this** (Game ID, Welcome, Traffic) |
| `IVXSDKSetupWizard.cs` | 4153 | **Fold into Advanced**; stop adding tabs |
| `IVXSetupWizard.cs` | 468 | Already redirects; **delete after one release** |
| `IVXAutoSetup.cs`, `IVXFeatureSetup.cs` | ~400–600 | Merge into Advanced or Control Center |
| `IVXDependencyChecker/Installer/Validator` | 3 files | **One** `IVXDependencies.cs` |
| `IVXEmojiSupportSetupWindow.cs` | 678 | Sample/tooling, not core |
| `IVXSDKExporter.cs` | 756 | Keep for maintainers only (`IntelliVerseX/Maintainers/`) |

### 3.4 QuizVerse vs generic SDK

Quiz, WeeklyQuiz, DailyQuiz, MoreOfUs, TutorXCoinGate are **product features**. They can stay as **optional assemblies** (`IntelliVerseX.Quiz`) default **off**, not as required boot.

---

## 4. Target Unity shape (after revamp)

```text
com.intelliversex.sdk
  Runtime/     Bootstrap, RequestBus, Identity, Wallet, Social (generic)
  Editor/      Control Center + Advanced + Dependencies
  Samples~/    GettingStarted, Auth, IAP, Ads (one network), Quiz (optional)

com.intelliversex.sdk.photon      (optional)
com.intelliversex.sdk.discord     (optional)
com.intelliversex.sdk.ai          (optional)

sandbox/   (this git repo’s Unity project)
  Vendors, Photon, TMP examples, demo scenes, boilerplate
```

Consumer install:

```json
"com.intelliversex.sdk": "https://github.com/…/Intelli-verse-X-SDK.git?path=Packages/com.intelliversex.sdk"
```

**Kid path (unchanged north star):** Package in → Control Center → paste Game ID → Welcome → Play.

---

## 5. How we use Unity CLI / skills during the revamp

| Step | Command / skill | Why |
|------|-----------------|-----|
| Baseline compile | `unity pipeline install` then `unity open` this project | Need Editor for tests |
| Tests | `unity test … --mode EditMode` then PlayMode | Proof we didn’t break facades |
| Package hygiene | `unity-package-management` skill | Add Nakama via Client API; **do not** hand-edit manifest for experiments |
| Size | `unity projects size` | Watch Photon/boilerplate not re-enter the package |
| Ads/IAP | `levelplay-unity-integration` / `implement-in-app-purchases` | Only on the **kept** ads/IAP path |
| UI samples | `ui-ugui` | Welcome overlay + Control Center IMGUI; no new theme system |
| Do not | `unity mcp configure` | CLI-first |

Until Pipeline is installed, **do not** claim PlayMode verification.

---

## 6. Phased revamp (safe order)

Breaking public APIs is a **major version**. Wrappers keep 5.x compiling.

### P0 — Stop the bleeding (days)

1. Document **canonical types** (this file + `[Obsolete]` on the losers).
2. Control Center is the only first-run (already started).
3. Pin `package.json` `unity` to **6000.3**.
4. `.gitignore` `Assets/IntelliVerseX/Generated` if generated.
5. Do not add menus or managers.

### P1 — Shrink the sandbox (1–2 weeks)

1. Move Photon, AppleAuthSample, TMP Examples, VSCode to `sandbox/` or delete Examples.
2. Remove MCP + 2D + Visual Scripting from **package** dependencies (sandbox project can keep them).
3. `tools/boilerplate` → Git LFS or a separate repo (171 MB).
4. One ads adapter interface; hide Applixir from default bootstrap.

### P2 — One Nakama path (1–2 weeks)

1. `IVXNManager` reads **only** `IVXBootstrapConfig`.
2. `IVXRequestBus` under all RPCs.
3. Obsolete `IVXWalletManager` / old leaderboard in favor of V2 types (aliases).
4. EditMode tests: Game ID empty fails fast; identity gate blocks wallet.

### P3 — Editor collapse (1 week)

1. Single Advanced window; delete SetupWizard class.
2. One dependency helper.
3. Maintainer exporter under a hidden menu.

### P4 — UPM extract (follows existing UPM plan)

1. `Packages/com.intelliversex.sdk` as the product.
2. This Unity project becomes the **sandbox** that *consumes* the package.
3. Samples~ only.

---

## 7. Definition of done (Unity)

- Consumer project: Control Center + Game ID + Play, **without** Photon or TMP examples in the package.
- One wallet type, one leaderboard type, one multiplayer entry.
- `unity test` EditMode green on 6000.3.6f1.
- Wizard not in the IntelliVerseX menu.
- `unity projects size` of the **package folder** far below 80 MB Assets today (vendors out).

---

## 8. What we will not do in this revamp

- Merge dual trees in one GUID rewrite (ADR-002 still applies until P4).
- Delete Photon from the **sandbox** before a sample that needs it is moved.
- Add UGS as a second backend.
- “Clean up” by rewriting all 50 managers at once.

**Next code slice:** P0 obsolete list + `package.json` Unity version + Control Center Game ID path (Phase A of the control-plane plan), then `unity pipeline install` so tests can run.

---

## 9. Step-by-step runbook (do this, in this order)

Work in a **non-elevated** PowerShell. One git branch (and one PR) per phase. Do not start the next phase until that phase’s **Done when** box is true.

Project path used below:

```powershell
$IVX = "C:\Office\Unity\Intelli-verse-X-SDK"
cd $IVX
$env:PATH = "$env:LOCALAPPDATA\Unity\bin;$env:PATH"
```

### Day 0 — Unlock the Editor (no product code yet)

Without this, you cannot prove later steps compiled.

1. Confirm CLI and license (not an Admin/`system32` shell):

```powershell
unity --version
unity auth status --format json
unity license status --format json
unity projects info --project-path $IVX --format json
unity status --format json
```

2. Install Pipeline into **this** project (needed for `unity command` / live tests):

```powershell
unity pipeline install --project-path $IVX
```

If the CLI has no `pipeline install` flag, open the project once and add `com.unity.pipeline` via Package Manager, then retry.

3. Open the Editor and wait until it is not compiling / not Safe Mode:

```powershell
unity open --project-path $IVX
# other terminal:
unity status --format json
```

If `unity status` cannot connect: compile errors → Safe Mode → Pipeline does not load. Fix C# errors first; do not hand-edit `.unity` / `.prefab` while an Editor is connected.

4. Baseline tests (record the count; later phases must not go backwards):

```powershell
unity test --project-path $IVX --platform EditMode
unity projects size --project-path $IVX
```

**Done when:** `unity status` shows a connected Editor; EditMode test run finishes (even if some tests already fail — write the number down).

---

### P0 — Stop the bleeding (days)

Goal: one public init story, no new sprawl, version pin. Dual trees stay on disk.

| # | Do | Files |
|---|----|--------|
| 1 | Branch `revamp/p0-stop-bleeding` | git |
| 2 | Pin UPM metadata to the real editor | `Assets/Intelli-verse-X-SDK/package.json` → `"unity": "6000.3"` (drop fake `unityRelease` `0f1` or set it to a real 6000.3 patch) |
| 3 | Gitignore generated consumer output | `.gitignore` → `Assets/IntelliVerseX/Generated/` |
| 4 | Mark losers `[Obsolete("Use …")]` — **do not delete** | `Core/IntelliVerseXManager` as init; `IVXWalletManager`; `IVXGLeaderboardManager`; `IVXSetupWizard`; `Core/IVXMultiplayerManager` if the `_IntelliVerseXSDK` path exists |
| 5 | Canonical comment on keepers | `IVXBootstrap`, `IVXBootstrapConfig`, `IVXNManager`, `IVXNWalletManager`, `IVXControlCenter` |
| 6 | Control Center Game ID field (Phase A, thin) | `Editor/IVXControlCenter.cs` + `IVXBootstrapConfig`: paste UUID → write gameId; empty ID fails with one sentence. Do **not** build Traffic/APIs tabs yet |
| 7 | First-run still opens Control Center only | `IVXProjectSetup.cs` (already routes); confirm no new `MenuItem`s |
| 8 | EditMode test: Control Center menu exists; empty Game ID is invalid | `Tests~/Editor/IVXEditorTests.cs` |
| 9 | Compile + tests | `unity test --platform EditMode` |

**Do not in P0:** delete Photon, TMP examples, wizard class, dual-tree folders, or `manifest.json` 2D/MCP packages.

**Done when:** Control Center is the only first-run window; `package.json` says 6000.3; obsolete attributes compile; EditMode not worse than Day 0.

**P0 EditMode baseline (2026-09-04):** Editor was already open, so `unity test --mode EditMode` could not launch a second instance. Pipeline `run_tests --mode editor` on this project: **2 total, 2 passed, 0 failed, 0 skipped**. Both are `IVXP0EditorTests` (`Menu_ControlCenterExists`, `BootstrapConfig_EmptyGameId_IsInvalid`). `Tests~/` is hidden by Unity’s `~` convention, so those older tests are not in this count.

---

### P1 — Shrink the sandbox (1–2 weeks)

Goal: consumers stop inheriting the demo kitchen sink. Sandbox may still contain vendors.

| # | Do | How |
|---|----|-----|
| 1 | Branch `revamp/p1-shrink-sandbox` | git |
| 2 | List what ships in the **package** vs the **Unity project** | Package = `Assets/Intelli-verse-X-SDK` (+ later `_IntelliVerseXSDK`). Project = everything else |
| 3 | Stop shipping TMP examples | Delete or move `Assets/TextMesh Pro/Examples & Extras` out of anything UPM will pack |
| 4 | Photon out of core package | Leave `Assets/Photon` in this repo for now, but **exclude** from `package.json` samples / exporter. Optional later: `com.intelliversex.sdk.photon` |
| 5 | Move samples that are not SDK | `AppleAuthSample/` → `Samples~` or sandbox-only; `VSCode/` editor toy stays in repo, not in UPM |
| 6 | Hide extra ads from default bootstrap | Applixir off unless a define/sample; keep **one** of LevelPlay or Appodeal as the default adapter |
| 7 | Strip **package** deps, not necessarily this sandbox `manifest.json` yet | `Assets/Intelli-verse-X-SDK/package.json` should not imply Photon/Appodeal/2D as required. Sandbox may keep MCP/2D until P4 |
| 8 | Boilerplate 171 MB | `tools/boilerplate` → Git LFS **or** a separate repo. Do not copy into UPM |
| 9 | Size check | `unity projects size --project-path $IVX` |

Package add/remove (when you touch UPM): use the **unity-package-management** skill (Editor `PackageManager.Client`), not hand-edits of `Packages/manifest.json` for experiments.

**Done when:** a hypothetical `path=Assets/Intelli-verse-X-SDK` install does not need Photon or TMP Examples; sandbox still opens.

---

### P2 — One Nakama path (1–2 weeks)

Goal: one Game ID, one session, one RPC pipe. Matches [WORLD_CLASS_CONTROL_PLANE.md](WORLD_CLASS_CONTROL_PLANE.md) Phases A–B.

| # | Do | Detail |
|---|----|--------|
| 1 | Branch `revamp/p2-one-nakama-path` | git |
| 2 | `IVXNManager` reads **only** `IVXBootstrapConfig` | Kill parallel `IntelliVerseXConfig` for host/gameId/session |
| 3 | Connect path | Game ID → resolve directory (existing bootstrap URL) → device auth → RPC `create_or_sync_user` → Welcome |
| 4 | `IVXRequestBus` | Timeout, jitter retry, honor `retry_after_ms`; all new RPCs go through it |
| 5 | Identity gate | Wallet/leaderboard refuse until `IsIdentitySynced` |
| 6 | Aliases | Old `IVXWalletManager` methods forward to `IVXNWalletManager` |
| 7 | Tests | Empty Game ID fails fast; wallet without identity fails with a clear error |

```powershell
unity test --project-path $IVX --platform EditMode
# after Editor is Play-ready:
unity test --project-path $IVX --platform PlayMode
```

**Done when:** one config asset; `create_or_sync_user` is the first RPC after auth; no silent dual host/gameId.

---

### P3 — Thin the Editor (1 week)

Goal: two windows max.

| # | Do |
|---|----|
| 1 | Branch `revamp/p3-thin-editor` |
| 2 | Fold `IVXSDKSetupWizard` tabs into **Advanced Setup** only (no new menus) |
| 3 | Merge `IVXAutoSetup` + `IVXFeatureSetup` + three dependency scripts → `IVXDependencies.cs` |
| 4 | Delete `IVXSetupWizard.cs` after the obsolete release (or keep a 5-line stub) |
| 5 | `IVXSDKExporter` → menu `IntelliVerseX/Maintainers/Export` (hidden / high priority) |
| 6 | Emoji window → sample or Maintainers |

**Done when:** `IntelliVerseX` menu is Control Center, Advanced Setup, Maintainers. Wizard type gone or stub. EditMode green.

---

### P4 — Extract the UPM package (follows `docs/UPM_PACKAGE_TRANSFORMATION_PLAN.md`)

Do **not** GUID-merge dual trees in the same PR as the extract.

| # | Do |
|---|----|
| 1 | Branch `revamp/p4-upm-extract` |
| 2 | Create `Packages/com.intelliversex.sdk` (Runtime, Editor, Samples~) from the canonical types |
| 3 | This Unity project **consumes** that package (sandbox keeps Photon, scenes, boilerplate) |
| 4 | Optional extra packages: Photon, Discord, AI |
| 5 | Consumer install string: git URL `?path=Packages/com.intelliversex.sdk` |
| 6 | Dual-tree merge only after the package is the source of truth (new ADR superseding ADR-002) |

**Done when:** a blank Unity 6000.3 project + that git URL → Control Center → Game ID → Play, without Photon in the package.

---

### Rules that apply on every step

- CLI first: `unity status` → `unity test` / `unity command`. Do not run `unity mcp configure` unless someone asks.
- Do not hand-edit `data/modules/index.js` (Nakama). This runbook is Unity-only.
- Do not push to main; one PR per phase.
- If stuck >15 minutes on Pipeline/Safe Mode: fix compile errors, restart Editor, `unity status` again.
- Stop if EditMode regressions vs Day 0 baseline.

### What you do **this week** (if you only have time for one slice)

1. Day 0 (Pipeline + open + baseline tests).  
2. P0 steps 2–8 (version pin, gitignore, Obsolete, Game ID on Control Center).  
3. Stop. Do not start Photon moves until that PR is merged.

