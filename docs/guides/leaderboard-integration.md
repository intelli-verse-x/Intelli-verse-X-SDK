# Leaderboard Integration Guide

This guide walks you from Nakama configuration through score submission, fetching rankings (global, contextual, and event-based), UI binding, and live updates. It aligns with the [Leaderboard module](../modules/leaderboards.md) reference and the APIs shipped in the SDK (Games leaderboard RPCs, Hiro systems, and `IVXLeaderboardUI`).

---

## Overview

IntelliVerseX leaderboard features sit on **Nakama**. Depending on your stack you will use one or more of:

| Layer | Typical use |
|-------|-------------|
| **Games leaderboard (RPC)** | `IVXGLeaderboardManager` — submit once, sync **daily / weekly / monthly / all-time / global** buckets defined for your `gameId`. |
| **Hiro — persistent boards** | `IVXHiroCoordinator.Instance.Leaderboards` — named leaderboards with **cursor pagination** and optional geo filters. |
| **Hiro — event boards** | `IVXHiroCoordinator.Instance.EventLeaderboards` — **time-limited** competitions, tiers, and claimable rewards. |
| **UI** | `IVXLeaderboardUI` (`IntelliVerseX.UI`) — tabs and fetch via `IVXNakamaManager.GetAllLeaderboards`. |

The module reference describes a focused facade (`IVXLeaderboardManager`, `IVXLeaderboardEntry`) in namespace `IntelliVerseX.Leaderboard`. Use [Leaderboard module](../modules/leaderboards.md) for that API shape; this guide uses the **concrete** types above so snippets match current assemblies.

---

## Prerequisites

- **Nakama** reachable from the client (correct scheme, host, port, server key).
- **Authentication** complete before any leaderboard call (valid `ISession`; for V2 flows, `IVXNManager` initialized).
- **Leaderboards** created and configured on the server (Console and/or Lua/runtime), with IDs and reset rules matching your game.
- **Hiro** (optional): `IVXHiroCoordinator` on a persistent `GameObject`, and `InitializeSystems(IClient, ISession)` called after auth (see [Hiro module](../modules/hiro.md)).

---

## Step 1 — Configure leaderboard in Nakama Console

Define each leaderboard’s **behavior**, not only its id.

| Setting | What to decide |
|---------|----------------|
| **Sort order** | `asc` or `desc` (e.g. high score = `desc`). |
| **Operator** | How writes merge: **best** (keep best only), **set** (always replace), **increment** / **decrement** (additive). |
| **Reset schedule** | Cron-style reset for rotating boards (daily, weekly, monthly) or none for all-time. |

Example intent (server-side Lua or admin tooling — adjust to your Nakama module layout):

```lua
-- Illustrative: sort_order, operator, reset_schedule must match your nakama/modules setup
{
  id = "weekly_challenge",
  sort_order = "desc",
  operator = "best",
  reset_schedule = "0 0 * * 1"  -- Monday 00:00 UTC
}
```

Naming conventions like `daily_*`, `weekly_*`, `monthly_*`, `alltime_*` help humans and docs stay aligned with reset expectations (see [Leaderboard module](../modules/leaderboards.md)).

!!! tip "Console vs code"
    You can create leaderboards in the **Nakama Console** or via **server runtime**. The client never creates definitions; it only **writes scores** and **reads records**.

---

## Step 2 — Submit scores (metadata, multiple leaderboards)

### Games SDK — single RPC, multiple periods

`IVXGLeaderboardManager` sends one payload to your backend; the server applies it to the leaderboards configured for the game.

```csharp
using System.Collections.Generic;
using IntelliVerseX.Games.Leaderboard;

var metadata = new Dictionary<string, string>
{
    { "level", "12" },
    { "game_version", Application.version }
};

var result = await IVXGLeaderboardManager.SubmitScoreAsync(
    score: playerScore,
    subscore: timeBonusMs,
    metadata: metadata);

if (result is { success: true })
{
    // Optional: inspect result.leaderboards_updated for rank deltas
}
```

Requirements: `IVXNManager` (or compatible manager) initialized; `GameId` and identities populated as described in `IVXGLeaderboardManager` (API user id, Nakama user id, device id).

### Hiro — explicit leaderboard id

Use this when you maintain **several named** Hiro leaderboards (not only the aggregate game buckets):

```csharp
using System.Collections.Generic;
using IntelliVerseX.Hiro;

var hiro = IVXHiroCoordinator.Instance;
if (!hiro.IsInitialized) { /* wait for OnInitialized */ return; }

var meta = new Dictionary<string, object> { ["mode"] = "ranked" };

var submit = await hiro.Leaderboards.SubmitScoreAsync(
    leaderboardId: "season_3_highscore",
    score: playerScore,
    subscore: 0,
    metadata: meta,
    location: null,
    gameId: null);
```

### Module reference (per-id facade)

The [Leaderboard module](../modules/leaderboards.md) shows the same intent as:

```csharp
// Documented facade shape — see module for full signature
// await IVXLeaderboardManager.SubmitScoreAsync("weekly_highscore", 15000,
//     subscore: tieBreaker, metadata: metadata);
```

Use **subscore** for tie-breakers and **metadata** for analytics or anti-cheat hints (session id, build id). Final authority is always **server-side** validation.

---

## Step 3 — Fetch global leaderboard (top N, pagination)

### Games SDK — top N across periods

```csharp
using IntelliVerseX.Games.Leaderboard;

const int topN = 50;
var all = await IVXGLeaderboardManager.GetAllLeaderboardsAsync(limit: topN);

if (all is { success: true })
{
    var global = all.global_alltime;  // resolved after ResolveGameLeaderboards
    foreach (var row in global.records)
        Debug.Log($"#{row.rank} {row.username}: {row.score}");
}
```

`IVXGLeaderboardData` may expose `next_cursor` / `prev_cursor` when the RPC returns cursor fields (fallback time-period RPC path).

### Hiro — cursor pagination

```csharp
var hiro = IVXHiroCoordinator.Instance;
string cursor = null;

do
{
    var page = await hiro.Leaderboards.GetRecordsAsync(
        leaderboardId: "season_3_highscore",
        limit: 20,
        cursor: cursor,
        geoFilter: null,
        gameId: null);

    foreach (var r in page.records)
        Debug.Log($"#{r.rank} {r.username}: {r.score}");

    cursor = string.IsNullOrEmpty(page.nextCursor) ? null : page.nextCursor;
} while (cursor != null);
```

### Module reference

`GetTopScoresAsync(leaderboardId, limit)` with optional offset-style pagination is described in [Leaderboard module](../modules/leaderboards.md) and [API reference](../api/leaderboards.md). Prefer **cursor**-based Hiro APIs when listing large boards to avoid gaps under concurrent writes.

---

## Step 4 — Fetch “around me” rankings

**Goal:** Show the player surrounded by neighbors (a few ranks above and below).

### Module facade

The reference API is:

```csharp
// From Leaderboard module — around current user
// var around = await IVXLeaderboardManager.GetScoresAroundUserAsync(
//     "monthly_tournament", limit: 11);  // e.g. 5 + self + 5
```

### Practical patterns with shipped code

1. **Games SDK:** Ensure the player appears in `records` (some RPC paths merge **owner** rows). Filter/sort locally around `owner_id == session.UserId`, or use `player_ranks` plus a second fetch with a larger `limit` if the server does not return neighbors.
2. **Hiro:** Fetch pages with `GetRecordsAsync` until you bracket the user’s rank, or extend the server RPC to return an “around owner” window (recommended for very large boards).

Also expose **friends-only** views when your backend supports social scope (`GetFriendsScoresAsync` in the [Leaderboard module](../modules/leaderboards.md)).

---

## Step 5 — Hiro event leaderboards (time-limited competitions)

`IVXHiroCoordinator.EventLeaderboards` drives **scheduled events** (start/end, tier, top records, claim flow).

Initialize Hiro once after Nakama auth:

```csharp
var coordinator = IVXHiroCoordinator.Instance;
coordinator.InitializeSystems(nakamaClient, session);
```

List active events and render `IVXEventLeaderboard` (id, name, `startAt`/`endAt`, `status`, `rank`, `score`, `topRecords`):

```csharp
var list = await coordinator.EventLeaderboards.ListAsync(gameId: null);
foreach (var ev in list.events)
{
    Debug.Log($"{ev.name} ends at {ev.endAt} — your rank {ev.rank}");
}
```

Submit a score for a specific event:

```csharp
var submitted = await coordinator.EventLeaderboards.SubmitScoreAsync(
    eventId: ev.id,
    score: runScore,
    gameId: null);
```

Claim rewards when the server marks the event claimable:

```csharp
var reward = await coordinator.EventLeaderboards.ClaimAsync(eventId: ev.id, gameId: null);
```

!!! note "Session lifecycle"
    On token refresh, call `IVXHiroCoordinator.Instance.RefreshSession(newSession)` so RPCs keep working ([Hiro module](../modules/hiro.md)).

---

## Step 6 — Display in UI (`IVXLeaderboardUI` or custom)

### Drop-in: `IVXLeaderboardUI`

Namespace: `IntelliVerseX.UI`.

1. Add the component to your leaderboard panel.
2. Assign a `MonoBehaviour` that implements **`IVXNakamaManager`**.
3. Wire **entries container**, **entry prefab** (child names `RankText`, `UsernameText`, `ScoreText`), optional tab buttons (daily / weekly / monthly / all-time / global).
4. Call `RefreshLeaderboards()` or rely on `autoRefreshOnShow`.

It uses `entriesPerPage`, `cacheTimeSeconds`, and raises `OnLeaderboardDataUpdated` with `IVXAllLeaderboardsResponse` for custom styling.

### Custom UI

Bind `IVXGLeaderboardRecord` or Hiro `IVXLeaderboardRecord` / `IVXEventLeaderboardRecord` to rows; highlight the current user by comparing `owner_id` / `userId` to `nakamaManager.Session.UserId`. The [Leaderboard module](../modules/leaderboards.md) includes a full **MonoBehaviour** example using `IVXLeaderboardEntry` and highlight rules — mirror that pattern with your row view type.

---

## Step 7 — Real-time updates

### Games SDK events

```csharp
IVXGLeaderboardManager.OnScoreSubmitted += response =>
{
    if (response?.leaderboards_updated != null)
        foreach (var u in response.leaderboards_updated)
            Debug.Log($"{u.period}: rank {u.prev_rank} → {u.new_rank}");
};

IVXGLeaderboardManager.OnLeaderboardsFetched += all => { /* refresh UI */ };
IVXGLeaderboardManager.OnError += msg => Debug.LogWarning(msg);
```

### Module reference events

[Leaderboard module](../modules/leaderboards.md) documents `OnLeaderboardLoaded` and `OnRankChanged` on `IVXLeaderboardManager` for reacting to loads and rank changes without polling.

### Polling vs push

Nakama can expose **real-time** leaderboard updates depending on server setup; if you only have RPCs, use **short TTL caching** plus refresh on focus or after `SubmitScoreAsync`.

---

## Best practices

- **Rate-limit submissions:** Debounce score posts (e.g. end-of-run only), not per frame or per second; respect server throttles.
- **Cache reads:** `IVXLeaderboardUI` already caches; for custom code, keep a 15–60s TTL per `leaderboardId` unless the UI must be live.
- **Tournament resets:** After a scheduled reset, **invalidate** client cache and refetch; show “reset in …” using server-provided times when available.
- **Operators:** Match client expectations to server **operator** (e.g. don’t expect monotonic growth if the board uses **best**).
- **Offline:** Queue scores locally and flush when `IVXNManager` reports a valid session; never trust client-only high scores for rewards.

---

## Troubleshooting

| Symptom | Things to check |
|---------|------------------|
| Submit returns null / `success: false` | Session expired, missing `GameId`, empty Nakama user id, or RPC error payload from server. |
| Empty leaderboards | Leaderboard ids / `gameId` mismatch; reset wiped records; wrong **scope** (game vs global). |
| Rank always 0 | `GetPlayerRankAsync` uses aggregate response — ensure `player_ranks` populated; user may have no record yet. |
| Hiro calls no-op | `InitializeSystems` not called, or session not refreshed after login. |
| `IVXLeaderboardUI` errors | Manager not `IVXNakamaManager`, or `IsInitialized` false when opening the panel. |

Enable `IVXGLeaderboardManager.EnableDebugLogs` in development builds to trace RPC payloads (Editor logs more detail).

---

## See also

- [Leaderboard module](../modules/leaderboards.md) — conceptual API, operators, reset schedules, `IVXLeaderboardEntry`, friends and anti-cheat notes  
- [Leaderboards API](../api/leaderboards.md) — quick method index  
- [Leaderboard demo](../samples/leaderboard-demo.md) — sample scene / mock flow  
- [Hiro module](../modules/hiro.md) — `IVXHiroCoordinator`, metagame systems, RPC envelope  

External: [Nakama leaderboards](https://heroiclabs.com/docs/nakama/concepts/leaderboards/) for server concepts and Console fields.
