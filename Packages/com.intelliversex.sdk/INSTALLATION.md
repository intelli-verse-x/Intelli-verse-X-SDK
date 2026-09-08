# IntelliVerseX SDK Installation

Package: `com.intelliversex.sdk` **6.0.0** · Unity **6000.3**

## Install (Git URL)

Add to `Packages/manifest.json`:

```json
{
  "dependencies": {
    "com.intelliversex.sdk": "https://github.com/Intelli-verse-X/Intelli-verse-X-SDK.git?path=Packages/com.intelliversex.sdk"
  }
}
```

Pin a tag when shipping:

```json
"com.intelliversex.sdk": "https://github.com/Intelli-verse-X/Intelli-verse-X-SDK.git?path=Packages/com.intelliversex.sdk#v6.0.0"
```

Or Package Manager → **+** → **Add package from git URL** with the same URL.

Local sandbox (this repo): the `editor` project uses `"com.intelliversex.sdk": "file:../../../../Packages/com.intelliversex.sdk"` (relative to `Packages/manifest.json` → `Packages/com.intelliversex.sdk`).

## Package vs sandbox (P1 boundary)

| Ships in UPM (`path=Packages/com.intelliversex.sdk`) | Stays in sandbox (`SDKs/unity/editor`) |
|--------------------------------------|----------------------------------------|
| Bootstrap, V2 Nakama facades, Auth/UI modules, Samples~ | Photon (~35 MB), vendored Nakama/AppleAuth, LevelPlay/Appodeal copies |
| UPM deps: TMP + Newtonsoft only | Cursor IDE package, pipeline, purchasing (project toys OK) |
| | `tools/boilerplate` (~171 MB) — **not** in package; gitignored |

TMP Examples & Extras are not present. Photon is never required to install the SDK.

## First-run (canonical)

1. **IntelliVerseX → Control Center**
2. Paste **Game ID** (creates / edits `IVXBootstrapConfig`)
3. **Add bootstrap to this scene**
4. Press Play

Advanced modules / optional deps: **IntelliVerseX → Advanced Setup**  
Maintainer export tools: **IntelliVerseX → Maintainers**

## Required external packages

Listed under `_ivx_externalDependencies` in `package.json`. Minimum for backend:

- **Nakama Unity** (required for wallet / leaderboard / sync)
- **DOTween** (required by several UI modules)

Ads / IAP / Photon / Discord are optional — enable only when you need them.

## Do not use (obsolete)

| Avoid | Use instead |
|-------|-------------|
| `IntelliVerseXManager` | `IVXBootstrap` |
| `IntelliVerseXConfig` | `IVXBootstrapConfig` |
| `IVXWalletManager` | `IVXNWalletManager` |
| `IVXGLeaderboardManager` / `IVXGLeaderboard` | `IVXNLeaderbordManager` |
| `IVXPhotonConfig` / Photon App ID helpers | Optional sandbox Photon only — not required for Control Center → Play |
| Device-only `InitializeDevice` as “SDK ready” | Control Center + Bootstrap |

## Canonical public types

| Domain | Type |
|--------|------|
| Wallet | `IVXNWalletManager` |
| Leaderboard | `IVXNLeaderbordManager` |
| Session / RPC | `IVXNManager` + `IVXRequestBus` |

## Verify

With the sandbox Editor open and Pipeline connected:

```powershell
unity status --format json
unity test "c:\Office\Unity\Intelli-verse-X-SDK\SDKs\unity\editor" --mode EditMode
```

## More

- [README.md](README.md) — feature overview
- [CHANGELOG.md](CHANGELOG.md) — releases
- Architecture: `docs/architecture/UNITY_SDK_REVAMP_PLAN.md` (repo root)
