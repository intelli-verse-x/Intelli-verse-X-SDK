# Optional Unity modules (not in core install)

Core package: **`com.intelliversex.sdk`** at `Packages/com.intelliversex.sdk`.

Kid path needs **only** the core package + Nakama client in the consumer project:

**Control Center → paste Game ID → Play.**

Photon Asset Store copies, AppleAuth samples, Appodeal, and LevelPlay vendor trees belong in **`SDKs/unity/editor`** (sandbox), not in the UPM package path. `IVXPhotonConfig` is obsolete and ships **no** shared App ID.

These areas may still exist as optional code folders inside the package, but are **not** required for the kid path. Future optional UPM ids (not extracted yet — dual-tree / GUID work is a later ADR):

| Planned package id | What it covers | Today |
|--------------------|----------------|--------|
| `com.intelliversex.sdk.photon` | PUN2 / Photon multiplayer helpers | Photon Asset Store copy stays in **sandbox** (`SDKs/unity/editor`). Core package must not require Photon. |
| `com.intelliversex.sdk.discord` | Discord social / rich presence modules | Code may exist under package `Discord` / related folders; treat as opt-in. |
| `com.intelliversex.sdk.ai` | LLM / persona / voice host modules | Code may exist under AI folders; treat as opt-in. |

## Rules

1. Do **not** add Photon / Discord / AI as hard UPM `dependencies` of `com.intelliversex.sdk`.
2. Sandbox may keep vendors for demos; exporters and docs must say they are optional.
3. When extracting a real optional package, use a **new** folder under `Packages/` and a new `package.json` — do not GUID-merge dual trees in the same PR.
4. Control Center marks Photon as **Optional** on the Home checklist; Traffic / APIs tabs cover `IVXRequestBus` without Photon.

See [UNITY_SDK_REVAMP_PLAN.md](architecture/UNITY_SDK_REVAMP_PLAN.md) §4 and [UPM_PACKAGE_TRANSFORMATION_PLAN.md](UPM_PACKAGE_TRANSFORMATION_PLAN.md).
