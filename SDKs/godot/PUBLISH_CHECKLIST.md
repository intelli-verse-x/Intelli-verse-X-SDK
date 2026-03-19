# IntelliVerseX Godot SDK — Publish-Ready Checklist

**Scene runs with no errors?** → Use **[READINESS.md](READINESS.md)** for the short ordered list to get ready for the Asset Library. This file is the full reference.

Use this list to make the Godot SDK production-ready: fix errors, verify data flow, and complete testing before release.

---

## 1. What to Do / What NOT to Do (from project rules)

| Do | Don't |
|----|--------|
| Change only SDK code under `addons/intelliversex/` | Modify third-party code (e.g. Nakama addon) |
| Follow naming: classes/configs match project style | Add new dependencies without approval |
| Emit signals and return values consistently | Break the Consumer → SDK → Nakama flow |
| Document public API and signals | Leave TODO/FIXME in committed code |
| Add/update tests for new or changed behavior | Commit with failing tests or linter errors |
| Update this checklist or CHANGELOG when shipping | Change public API without version/CHANGELOG note |

---

## 2. Errors & Data Flow

### 2.1 Completed (this pass)

- [x] **Example config** — Use `nakama_use_ssl` (not read-only `nakama_scheme`) in `examples/basic_example.gd`.
- [x] **Unit test** — `test_config_custom_values` sets `nakama_use_ssl`, asserts `nakama_scheme` getter.
- [x] **wallet_updated** — Emit `wallet_updated` from `fetch_wallet()` when wallet data is returned (README/example aligned).
- [x] **JSON safety** — `read_storage` and `call_rpc` handle `JSON.parse_string()` returning `null` (invalid/missing payload).

### 2.2 Parser / open-project errors (fixed)

- [x] **GutTest not found** — Added `addons/intelliversex/tests/.gdignore` so Godot does not parse the test script when the GUT addon is not installed.
- [x] **Nakama types not found** — Added runtime-loaded Nakama: `ivx_manager.gd` no longer references Nakama types at parse time; it uses `_load_nakama_refs()` to load either the real addon (from `addons/com.heroiclabs.nakama` or `addons/nakama`) or the built-in stub (`addons/intelliversex/nakama_stub.gd`). The project now opens in Godot without errors even when the Nakama addon is not installed.

### 2.3 To verify before publish

- [ ] **Init → Auth → Session** — No API is called before `initialize()`; auth flows set `nakama_session` before profile/wallet/leaderboard/storage.
- [ ] **Error signals** — All async failure paths emit `error` or `auth_error`; no silent failures.
- [ ] **Session expiry** — `restore_session()` and `has_valid_session()` behave correctly when token is expired (re-auth path).
- [ ] **Socket lifecycle** — `disconnect_socket()` / `clear_session()` leave state consistent; no use of closed socket.

---

## 3. Testing

### 3.1 Unit tests (GUT)

- [ ] **GUT addon** — Install [GUT](https://github.com/bitwes/Gut) in the Godot project that includes this addon.
- [ ] **Run all tests** — Open GUT panel, run `addons/intelliversex/tests/test_ivx.gd`; all tests pass.
- [ ] **No Nakama required** — Current tests do not require a running Nakama server (init/session/signals only).

### 3.2 Integration / manual

- [ ] **Example scene** — Create a minimal scene with `basic_example.gd` (or equivalent), IntelliVerseX autoload enabled, Nakama addon enabled.
- [ ] **With local Nakama** — Run Nakama locally; run example: init → device auth → fetch profile → fetch wallet → leaderboard → storage. No errors in console.
- [ ] **Session restore** — Close and reopen game; confirm session restore and that no duplicate auth occurs.
- [ ] **Error paths** — Wrong host/port or invalid credentials; confirm `auth_error` / `error` emitted and no crash.

### 3.3 CI

- [ ] **Platform SDK workflow** — Push to a branch that touches `SDKs/godot/`; `.github/workflows/platform-sdks-validation.yml` passes (structure + version check).

---

## 4. Documentation & Release

- [ ] **README.md** — Requirements (Godot 4.2+, Nakama addon), install steps, Quick Start, and feature table are correct.
- [ ] **API/signals** — Public methods and signals (e.g. `initialized`, `auth_success`, `auth_error`, `error`, `profile_loaded`, `wallet_updated`) documented in README or linked docs.
- [ ] **Version** — Same version in `addons/intelliversex/plugin.cfg` and `ivx_manager.gd` (`SDK_VERSION`); matches repo/release version (e.g. 5.2.0).
- [ ] **CHANGELOG** — Godot-specific changes noted in repo CHANGELOG (or a short `SDKs/godot/CHANGELOG.md` if the project uses per-SDK logs).

---

## 5. Production Readiness

- [ ] **No debug-only code** — No `print()` or debug paths that run in production unless gated by `config.enable_debug_logs`.
- [ ] **Secrets** — No hardcoded server keys or credentials; config (host, port, key) comes from project/config resource.
- [ ] **Dependencies** — Only documented deps: Godot 4.2+, Nakama Godot addon v3.5+; no extra addons required except for running tests (GUT).

---

## 6. Quick Reference — Key Files

| File | Purpose |
|------|--------|
| `addons/intelliversex/plugin.cfg` | Plugin metadata, version |
| `addons/intelliversex/ivx_plugin.gd` | Autoload registration |
| `addons/intelliversex/core/ivx_manager.gd` | Core API, session, auth, profile, wallet, leaderboard, storage, RPC |
| `addons/intelliversex/core/ivx_config.gd` | Config resource (host, port, SSL, debug) |
| `addons/intelliversex/tests/test_ivx.gd` | Unit tests (GUT) |
| `examples/basic_example.gd` | Full usage example |
| `README.md` | Install, quick start, features |

---

## 7. Godot 4.2+ Build-Test & Asset Library

### 7.1 Build-test addon against Godot 4.2+

- [ ] **Local:** Open `SDKs/godot` as a project in Godot 4.2 (or 4.3+); enable the IntelliVerseX plugin; confirm no errors in the editor or in the Output panel.
- [ ] **CI:** The workflow `platform-sdks-validation` runs a Godot 4.2 headless build-test (see job `godot-build-test`). Push to a branch that touches `SDKs/godot/` and confirm the job passes.
- [ ] **Optional local headless:** From repo root, run  
  `docker run --rm -v "%cd%":/project -w /project/SDKs/godot godotengine/godot:4.2-headless --headless --path . --quit-after 5`  
  (Windows) or the same with `$(pwd)` (Linux/macOS). Exit code 0 means the project and addon load without crash.

### 7.2 Submit to Godot Asset Library

- [ ] **Prerequisites:** LICENSE and README in `addons/intelliversex/` (copy of repo license + short readme); .gitignore at repo root; no essential submodules; icon URL is a direct link (e.g. raw.githubusercontent.com).
- [ ] **Account:** Log in at [Godot Asset Library](https://godotengine.org/asset-library/asset/submit).
- [ ] **Submit:** Use [ASSET_LIBRARY_SUBMISSION.md](ASSET_LIBRARY_SUBMISSION.md) in this folder for the exact form fields (name, category, Godot version 4.2, version, repo URL, download commit, icon, license, description). Submit and wait for review.

### 7.3 Verify install via AssetLib browser

- [ ] **In Godot:** Project Manager → AssetLib → search for "IntelliVerseX" (or your asset name).
- [ ] **Install:** Click Install and complete installation into a test project.
- [ ] **Verify:** Enable the plugin in Project Settings → Plugins; run a minimal script that calls `IntelliVerseX.initialize(IVXConfig.new())` and confirm no errors (Nakama addon still required for full flow).

---

## 8. Suggested Order of Work

1. Run unit tests (GUT) and fix any failures.
2. **Build-test addon against Godot 4.2+** (local and/or CI).
3. Run platform-sdks-validation workflow (push or local equivalent).
4. Manually run the example with local Nakama; verify init → auth → profile → wallet → leaderboard → storage and session restore.
5. Verify error paths and that all public signals are documented.
6. Confirm version and CHANGELOG; then tag/release.
7. **Submit to Godot Asset Library** (see §7.2 and ASSET_LIBRARY_SUBMISSION.md).
8. **Verify install via AssetLib browser** (see §7.3).

---

*Last updated: 2026-03-16*
