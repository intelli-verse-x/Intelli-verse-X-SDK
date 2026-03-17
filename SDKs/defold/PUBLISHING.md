# Publishing the IntelliVerseX Defold SDK

This guide covers publishing the SDK to **GitHub**, **itch.io**, and your **developer portal** so it works for all platforms (Windows, macOS, Linux, HTML5, Android, iOS) that Defold supports.

---

## What you are publishing

- The **SDK is a Lua library**. You publish the **source package** (this folder as a zip or as a Git dependency).
- **Platform builds** (Windows exe, Android apk, etc.) are created by **developers who use the SDK** in their Defold game — they use **Project → Bundle** in Defold (or Bob) for the platform they need. You do **not** need to build the SDK per platform.

---

## 1. GitHub (required)

### 1.1 Create a release

1. In the repo: **Releases → Create a new release**.
2. **Tag:** e.g. `defold-v5.1.0` or use the main SDK tag `v5.1.0` (if the repo has one release per version for all platforms).
3. **Title:** e.g. `IntelliVerseX Defold SDK 5.1.0`.
4. **Description:** Copy from CHANGELOG or list: Auth, profile, wallet, leaderboards, storage, RPC, backend alignment with Unity SDK, tests (15/15).

### 1.2 Add the SDK zip as an asset

Create a zip so the **zip root is the Defold project root** (Defold expects `game.project` at the root of a dependency):

- **Option A (recommended):** Zip the **contents** of `SDKs/defold/` (so the zip root has `game.project`, `intelliversex/`, `nakama/`, `README.md`, etc.). Name it e.g. `intelliversex-defold-5.1.0.zip`.
- **Option B:** Zip the whole repo; consumers then add the repo archive URL and must set **include_dirs** to the path where Defold can find the module (e.g. the merged library may expose a single folder; document the exact path).

Upload the zip to the GitHub release as an asset.

### 1.3 Dependency URL for consumers

- **If you used Option A:** Consumers add the **release asset URL** in `game.project`:

```ini
[dependencies]
intelliversex = https://github.com/YOUR_ORG/Intelli-verse-X-Unity-SDK/releases/download/v5.1.0/intelliversex-defold-5.1.0.zip
```

The library’s `include_dirs` (in the SDK’s `game.project`) will expose `intelliversex`, `tests`, `nakama`; they require the module with `require "intelliversex.ivx"`.

- **If you used Option B:** Use the repo archive URL and document that the Defold SDK is in `SDKs/defold/`; they may need to depend on a path inside the archive (see Defold docs on library paths).

---

## 2. itch.io

itch.io is mainly for **playable builds** or **downloadable tools/assets**.

### Option A: SDK as a downloadable asset

1. Create a new project on [itch.io](https://itch.io/game/new).
2. **Project type:** Choose **Asset** or **Tool** (not “Game”).
3. **Upload:** Attach the same zip you built for GitHub (e.g. `intelliversex-defold-5.1.0.zip`).
4. **Title / description:** e.g. “IntelliVerseX Defold SDK — Auth, Backend (Nakama), Wallet, Leaderboards”.
5. Set visibility (e.g. public) and publish.

### Option B: Demo project (optional)

If you want a “playable” page: bundle the **current Defold project** (with the test runner) for one platform (e.g. HTML5 or Windows) and upload that build to itch.io as a **Game**. In the description, link to the SDK zip or GitHub for the actual library.

---

## 3. Developer portal

1. **Documentation:** Add a “Defold” page that links to:
   - [SDKs/defold/README.md](https://github.com/Intelli-verse-X/Intelli-verse-X-Unity-SDK/blob/main/SDKs/defold/README.md) or your docs site’s Defold section.
   - Full API/docs if you have them (e.g. [platforms/defold](https://intelli-verse-x.github.io/Intelli-verse-X-Unity-SDK/platforms/defold/)).
2. **Download:** One clear “Download Defold SDK” button/link that points to:
   - The **GitHub release asset** (zip), or
   - The **repo archive** for the chosen tag (e.g. `https://github.com/.../archive/refs/tags/v5.1.0.zip`) with a note that the Defold SDK is in `SDKs/defold/`.
3. **Requirements:** Defold 1.6+, Nakama Defold client v3.5+ (as in README).

---

## 4. Checklist before publishing

- [ ] All 15 tests pass in Defold (main/run_tests.script).
- [ ] README has correct version and links (API reference, license).
- [ ] `game.project` version matches (e.g. `5.1.0`).
- [ ] No secrets or local paths in the SDK folder.
- [ ] Zip contains: `intelliversex/`, `nakama/` (vendored), `main/`, `input/`, `tests/`, `examples/`, `game.project`, `README.md`, and optionally `PUBLISHING.md`.

---

## 5. “All platforms” summary

| Channel        | What to publish | Platforms |
|----------------|-----------------|-----------|
| **GitHub**     | Release + zip of SDK (or repo tag) | N/A (source) |
| **itch.io**    | Same zip as Asset/Tool, or a demo build | Optional: HTML5/Windows for demo |
| **Dev portal** | Doc page + download link to GitHub (or zip) | N/A (source) |

**Defold supports:** Windows, macOS, Linux, HTML5, Android, iOS. The SDK is Lua and runs in any Defold project; **developers** choose **Project → Bundle → [Platform]** when building their game. No extra packaging per platform is required from you for the SDK itself.
