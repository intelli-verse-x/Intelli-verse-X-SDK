# Legacy dual-tree leftovers

Most `_IntelliVerseXSDK` modules were flattened to the package root (see ADR-003).

Still here (name collisions with top-level folders — merge later):

- `Icons/`
- `IntroScene/`
- `Monetization/`

Do not add new modules under `_IntelliVerseXSDK`. Prefer package-root folders or optional packages (`com.intelliversex.sdk.ai` / `.discord` / `.photon`).
