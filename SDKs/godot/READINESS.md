# IntelliVerseX Godot SDK — Ready for Asset Library

Use this list now that the scene runs with no errors. Work in order.

---

## One iteration to publish

1. **Run the scene (F5).** If you see a connection error: start Nakama (e.g. `docker run -d -p 7350:7350 heroiclabs/nakama`) or set **Server Host** / **Port** on **IntelliVerseXExample**. Use **Use Ssl** = false for local HTTP Nakama.
2. **Icon:** Add a square icon (min 128×128) to the repo and note its raw URL for the submission form.
3. **Version:** Ensure `plugin.cfg` and `ivx_manager.gd` (`SDK_VERSION`) match (e.g. `5.2.0`).
4. **CHANGELOG:** Add a Godot SDK entry for this release.
5. **Submit:** Use [ASSET_LIBRARY_SUBMISSION.md](ASSET_LIBRARY_SUBMISSION.md) to fill the form (name, category, Godot 4.2, repo URL, download commit, icon URL, license, description).

The connection error you see is **not a code bug** — it means the Nakama server is not reachable at the configured URL. Fix by running Nakama and/or correcting host/port/SSL.

---

## Done already

- [x] Project opens in Godot with no parse/editor errors
- [x] IntelliVerseX and Nakama plugins visible and enabled
- [x] Main scene (`main.tscn`) runs (F5) with no errors
- [x] Example script type-inference fixes applied
- [x] Nakama paths and `restore_session` fixed for official addon
- [x] README: install, troubleshooting, re-add Nakama note, Godot 4.6
- [x] LICENSE + README in `addons/intelliversex/`

---

## 1. Test with your Nakama server (required)

Do this in the Editor so the addon is ready for the Asset Library:

1. **Open the main scene:** In the FileSystem dock, double‑click `main.tscn`.
2. **Select the example node:** In the Scene tree, select the **IntelliVerseXExample** node (child of Main).
3. **Set server in the Inspector:** In the Inspector, set:
   - **Server Host** — e.g. `127.0.0.1` for local or your Nakama host.
   - **Server Port** — e.g. `7350`.
   - **Server Key** — e.g. `defaultkey` (must match your server).
   - **Use Ssl** — turn on if your server uses HTTPS.
4. **Run the project:** Press **F5** (or Project → Run Project).
5. **Check the Output panel:** Confirm you see (in order, no errors):
   - `SDK ready — attempting session restore…`
   - Either `Session restored!` or `No saved session — authenticating with device ID…`
   - `Authenticated! User ID: … Username: …`
   - `Profile loaded: …`
   - `Wallet updated: …`
   - Leaderboard and storage messages (submit/read).
6. **Optional:** Stop, run again (F5); you should see session restore instead of device auth.

**If you see "Could not connect to the server at http(s)://…"** — Start your Nakama server first (e.g. Docker: `docker run -d -p 7350:7350 heroiclabs/nakama`), or set **Server Host** / **Port** in the Inspector to where Nakama is actually running. **Local Nakama is usually HTTP:** set **Use Ssl** = false unless your server uses HTTPS.

- [ ] Editor test done: server set in Inspector, F5 run, full flow in Output with no errors.
- [ ] Optional: close and run again; confirm session restore (no duplicate auth).

---

## 2. Asset Library prerequisites

- [ ] **Icon:** Create or choose a square icon (min 128×128 px), upload to your repo, and note the **raw** URL (e.g. `https://raw.githubusercontent.com/ORG/REPO/main/SDKs/godot/icon.png`). You will need this for the submission form.
- [ ] **Version:** Confirm `addons/intelliversex/plugin.cfg` and `ivx_manager.gd` (`SDK_VERSION`) both say the same version (e.g. `5.2.0`).
- [ ] **CHANGELOG:** Add a Godot SDK entry to the repo CHANGELOG (or `SDKs/godot/CHANGELOG.md`) for this release.

---

## 3. Final checks before submit

- [ ] No `print()` in production code except when gated by `config.enable_debug_logs` (example can keep prints for demo).
- [ ] No hardcoded secrets; server config comes from Inspector or code (IVXConfig).
- [ ] Push your branch and confirm **platform-sdks-validation** (and **godot-build-test** if present) pass in GitHub Actions.

---

## 4. Submit to Godot Asset Library

- [ ] Log in at [Godot Asset Library → Submit](https://godotengine.org/asset-library/asset/submit).
- [ ] Fill the form using [ASSET_LIBRARY_SUBMISSION.md](ASSET_LIBRARY_SUBMISSION.md) (name, category, Godot 4.2, version, repo URL, **download commit**, icon URL, license, description).
- [ ] Submit and wait for review.

---

## 5. After approval

- [ ] In Godot: Project Manager → AssetLib → search "IntelliVerseX" → Install into a **new** test project.
- [ ] Enable the plugin, run a minimal scene that calls `IntelliVerseX.initialize(IVXConfig.new())`, confirm no errors.

---

**Full detail:** [PUBLISH_CHECKLIST.md](PUBLISH_CHECKLIST.md)  
**Submission form guide:** [ASSET_LIBRARY_SUBMISSION.md](ASSET_LIBRARY_SUBMISSION.md)
