# Submitting IntelliVerseX Godot SDK to the Godot Asset Library

Use this as a step-by-step checklist when submitting the addon to the [Godot Asset Library](https://godotengine.org/asset-library/asset/submit).

---

## Before you start

- [ ] Addon builds and tests pass (see [PUBLISH_CHECKLIST.md](PUBLISH_CHECKLIST.md)).
- [ ] You have a **Godot Asset Library account** and are logged in.
- [ ] Repo has a **LICENSE** (or LICENSE.md) at the top level; **addon folder** has a copy of the license and a README (see `addons/intelliversex/LICENSE` and `addons/intelliversex/README.md`).
- [ ] **Icon**: square image (min 128×128), hosted at a **direct URL** (e.g. `https://raw.githubusercontent.com/ORG/REPO/BRANCH/path/icon.png`).

---

## Submission form fields

Fill the form at [Submit Assets](https://godotengine.org/asset-library/asset/submit) with the following.

| Field | What to enter |
|-------|----------------|
| **Asset Name** | `IntelliVerseX SDK` (or "IntelliVerseX Godot SDK") |
| **Category** | Addons → Scripts (or Networking if available) |
| **Godot version** | 4.2 (tested up to 4.6.x) |
| **Version** | Match `plugin.cfg` / `SDK_VERSION` (e.g. `5.1.0`) |
| **Repository host** | GitHub (or your Git host) |
| **Repository URL** | Your repo URL, e.g. `https://github.com/Intelli-verse-X/Intelli-verse-X-Unity-SDK` |
| **Issues URL** | Same repo issues, e.g. `https://github.com/Intelli-verse-X/Intelli-verse-X-Unity-SDK/issues` |
| **Download Commit** | Full commit hash of the release (e.g. `b1d3172f89b86e52465a74f63a74ac84c491d3e1`). Users get a ZIP of the repo at this commit. |
| **Icon URL** | Direct link to a square icon (PNG/JPG). Must be **raw** link (e.g. `raw.githubusercontent.com/.../icon.png`). |
| **License** | MIT (must match repo and addon LICENSE file) |
| **Description** | Short description in English: what the addon does (Auth, Backend/Nakama, Profile, Wallet, Leaderboards, Storage, RPC). Mention that the addon lives under `SDKs/godot/addons/intelliversex/` and that the [Nakama Godot addon](https://github.com/heroiclabs/nakama-godot) is required. |

**Preview (optional):** You can add up to 3 image or YouTube preview URLs with thumbnails.

---

## Important notes

- **Monorepo:** This addon lives in a subfolder (`SDKs/godot/addons/intelliversex/`). In the description, state clearly that users should copy the contents of `addons/intelliversex/` from the `SDKs/godot` folder into their project’s `addons/` folder, or open `SDKs/godot` as a project to use the example.
- **Download Commit:** Each time you update the asset, change the download commit to the new release hash and (optionally) bump the version.
- **Review:** Submission is reviewed manually; approval can take a few days. If rejected, you’ll get feedback to fix and resubmit.

---

## After submission

- [ ] Track the asset in the [pending queue](https://godotengine.org/asset-library/asset/edit?&asset=-1) if available.
- [ ] When approved, run through **Verify install via AssetLib browser** in [PUBLISH_CHECKLIST.md](PUBLISH_CHECKLIST.md) (§7.3).
