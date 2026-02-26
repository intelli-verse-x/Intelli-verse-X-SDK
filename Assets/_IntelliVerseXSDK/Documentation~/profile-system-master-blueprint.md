# Profile System Master Blueprint

This document defines the production profile flow between Unity SDK V2 and Nakama.

## Scope

- Backend RPC hardening for profile identity/metadata/portfolio/geo APIs.
- SDK runtime API via `IVXNProfileManager`.
- Test scene demo (`Samples~/TestScenes/IVX_ProfileTest.unity`) with ready-to-run controller/view scripts.
- Edge-case test matrix for auth, validation, throttling, conflict, and network failures.

## Stable RPC Contract

Profile endpoints are fixed:

- `create_or_sync_user`
- `rpc_update_player_metadata`
- `get_player_metadata`
- `get_player_portfolio`
- `check_geo_and_update_profile`
- `admin_delete_player_metadata` (admin-only)

Response envelope standard:

```json
{
  "success": true,
  "errorCode": null,
  "traceId": "trace-...",
  "requestId": "req-...",
  "data": {}
}
```

Backward-compatible aliases are preserved (`error_code`, `request_id`, legacy top-level fields).

## Backend Guarantees

- Strict profile allowlist and forbidden-field checks for mutable payloads.
- Canonicalization for `country_code`, `geo_location`, `platform`, `locale`.
- Self/admin authorization boundaries:
  - self-only by default
  - explicit admin gate for delete/admin operations
- Schema migration safety:
  - reads fallback from legacy profile keys
  - migrated writes normalized into `player_metadata:user_identity`
  - `schemaVersion` + `profileVersion` maintained and incremented
- Concurrency safety:
  - optional `expected_profile_version` on updates
  - `VERSION_CONFLICT` returned on stale writes
- Reliability and observability:
  - rate limits on read/write/admin profile RPCs
  - cache invalidation on metadata write/delete/geo update
  - trace propagation and structured middleware wrappers

## SDK Runtime API

`IVXNProfileManager` is the consumer-facing profile API:

- `FetchProfileAsync()`
- `UpdateProfileAsync(IVXNProfileUpdateRequest request)`
- `FetchPortfolioAsync()`
- `RefreshProfileAfterAuthAsync()`

Runtime model behavior:

- Keeps in-memory profile snapshot (`Snapshot`, `LastSyncedAtUtc`, `IsDirty`).
- Maps backend error codes to retryable/non-retryable semantics.
- Bounded retries for transient classes (`429`, `5xx`, network/upstream).
- Client-side validation for profile update fields before RPC calls.

Events:

- `OnProfileLoaded`
- `OnProfileUpdated`
- `OnProfileError`

`IVXNManager` integration:

- profile events are surfaced from manager:
  - `OnProfileLoaded`
  - `OnProfileUpdated`
  - `OnProfileError`
- profile auto-refresh runs after auth/session metadata sync.

## UPM Sample Scene

Location: `Samples~/TestScenes/IVX_ProfileTest.unity`

Includes:

- `IVXProfileDemoController`
- `IVXProfileDemoView`
- `IVXProfileDemoMocks` (fallback path)

Demo flow:

1. Initialize Nakama session (if needed).
2. Fetch profile automatically.
3. Edit and save fields with conflict-safe payload.
4. Re-fetch and verify profile updates.
5. Fetch portfolio and render game/global wallet snapshot.

## QA Edge-Case Matrix

- Auth/session:
  - missing session -> `AUTH_REQUIRED`
  - expired session -> re-init/re-auth flow
- Validation:
  - invalid JSON/payload types
  - invalid locale/country/platform formats
  - forbidden profile fields
- Concurrency:
  - stale `expected_profile_version` -> `VERSION_CONFLICT`
  - repeated writes under retry pressure
- Resilience:
  - `429` throttling and retry behavior
  - transient `5xx` / network errors
  - upstream geo failure (`UPSTREAM_ERROR`)
- Data compatibility:
  - legacy storage keys read + migration write
  - schema/version normalization

## Consumer Setup

1. Import `Test Scenes` sample from UPM.
2. Open `IVX_ProfileTest.unity`.
3. Ensure backend exposes the profile RPC set listed above.
4. Authenticate user (or keep mock fallback enabled for offline demo).
5. Run scene and verify profile/portfolio flows.
