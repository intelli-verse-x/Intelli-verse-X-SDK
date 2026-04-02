# ADR-002: Dual-Tree Unity Asset Layout

## Status

Accepted

## Context

The Unity project contains two top-level SDK folders under `Assets/`:

| Folder | Contains |
|--------|----------|
| `Assets/Intelli-verse-X-SDK/` | Legacy SDK — Identity, Monetization/Ads, UI, Leaderboard, Quiz, Backend, IAP, Editor tooling. Holds the `package.json` UPM manifest. |
| `Assets/_IntelliVerseXSDK/` | New SDK modules — Bootstrap, AI/LLM stack, Hiro live-ops, Satori analytics, Discord Social, Multiplayer, Platform utilities, 16 demo scenes. |

This split occurred organically as new modules (AI, Hiro, Discord, etc.) were built with stricter assembly-definition boundaries and different dependency graphs from the original SDK. Merging them into a single tree would require renaming hundreds of asset GUIDs and updating every consumer project's references.

## Decision

Keep the dual-tree layout. Each tree owns separate `.asmdef` files and can reference the other via assembly references when cross-module calls are needed (e.g., `IVXBootstrap` references `IVXURLs` from the legacy tree to set `GameId`).

The **UPM package manifest** (`package.json`) lives in `Assets/Intelli-verse-X-SDK/` because that was the original importable package. Both trees ship together when the SDK is distributed.

## Consequences

**Easier:**

- Adding new modules without touching legacy code
- Independent assembly compilation (faster incremental builds)
- Isolating demo scenes from production assemblies
- Avoiding GUID conflicts during merges

**Harder:**

- New contributors must understand that the SDK spans two folders
- Cross-tree references require explicit `.asmdef` dependencies
- `SYSTEM_MAP.md` must document both trees clearly

## Alternatives Considered

1. **Merge everything into `Assets/Intelli-verse-X-SDK/`** — rejected due to GUID breakage for existing consumers and the scale of the rename.
2. **Merge everything into `Assets/_IntelliVerseXSDK/`** — rejected because the legacy tree holds the UPM manifest and external projects already depend on its paths.
3. **UPM-only distribution with no Assets/ folder** — long-term goal (see `docs/UPM_PACKAGE_TRANSFORMATION_PLAN.md`) but not feasible today given third-party dependencies that require Assets/.
