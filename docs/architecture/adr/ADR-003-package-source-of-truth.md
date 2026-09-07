# ADR-003: Dual-Tree Flattened — Package Is Source of Truth

## Status

Accepted (supersedes [ADR-002](ADR-002-dual-tree-layout.md) for layout policy)

## Context

ADR-002 kept `Intelli-verse-X-SDK/` and `_IntelliVerseXSDK/` as sibling trees under Assets to avoid a GUID rewrite. After P4, both trees already live inside `Packages/com.intelliversex.sdk`. Contributors still had to know two roots.

## Decision

1. **Canonical root:** `Packages/com.intelliversex.sdk/` (flat module folders).
2. **Moved without GUID rewrite:** non-colliding `_IntelliVerseXSDK/*` modules were `git mv`'d to the package root (Characters, Competition, Console, DailyRewards, Demos, Missions, Multiplayer, Notifications, Platform, Progression, Quest, Retention, Bootstrap runtime). `.meta` GUIDs travel with the files — no consumer GUID remapping.
3. **Optional packages extracted:**
   - `Packages/com.intelliversex.sdk.ai`
   - `Packages/com.intelliversex.sdk.discord`
   - `Packages/com.intelliversex.sdk.photon` (glue only; PUN2 stays Asset Store / sandbox)
4. **Remaining dual leftovers:** `_IntelliVerseXSDK/{Icons,IntroScene,Monetization}` stay until a dedicated pass (name collisions with top-level folders). They are not a second product tree.
5. **Bootstrap** soft-loads AI/Discord via reflection so core compiles without optional packages.

## Consequences

**Easier:** one mental model; optional installs via git `?path=`; Control Center APIs tab reads generated RPC index.

**Harder:** sandbox must list optional packages in `manifest.json` to dogfood AI/Discord demos; colliding folders still need a later merge.

## Alternatives Rejected

- Full GUID rewrite of Monetization/Icons/IntroScene in the same change — risk without payoff.
- Leaving AI/Discord inside core — contradicts optional UPM plan.
