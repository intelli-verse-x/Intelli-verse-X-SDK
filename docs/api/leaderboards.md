# Leaderboards API Reference

IntelliVerseX leaderboard features combine **Nakama leaderboards** (scores, metadata, expiry, sort order) with **Unity Games SDK helpers** (`IVXGLeaderboardManager`) that call server RPCs (`submit_score_and_sync`, `get_all_leaderboards`, `get_time_period_leaderboard`). Hiro **event leaderboards** are a separate surface for time-limited competitions.

## Unity Games SDK namespace

```csharp
using IntelliVerseX.Games.Leaderboard;
```

## Nakama leaderboard concepts

| Concept | Description |
|--------|-------------|
| **Leaderboard id** | Server-defined string (e.g. `leaderboard_{gameId}_weekly`). Unity RPC responses map game + period into concrete ids. |
| **Score / subscore** | Primary score plus optional tie-breaker (`subscore`). |
| **Metadata** | Opaque string or JSON attached to a record (e.g. loadout, avatar). |
| **Sort order** | Configured on the server (ascending vs descending). Client submits scores; ordering is enforced by Nakama. |
| **Reset schedule** | Optional rolling reset (daily / weekly / monthly). Matches `get_time_period_leaderboard` periods. |
| **Owner id** | Nakama user id for the record. |

---

## `SubmitScoreAsync` (Unity)

| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| `score` | `int` | — | Primary score. |
| `subscore` | `int` | `0` | Tie-breaker. |
| `metadata` | `Dictionary<string, string>` | `null` | Optional key/value metadata serialized for the record. |

| Returns | Description |
|---------|-------------|
| `Task<IVXGScoreSubmissionResponse>` | `null` on failure; check `success` and `error` when non-null. |

**Behavior:** Resolves API user id, Nakama user id, `gameId`, and `deviceId`, then calls RPC `submit_score_and_sync`. Maintains an internal **win streak** counter on successful submissions.

**Example:**

```csharp
var response = await IVXGLeaderboardManager.SubmitScoreAsync(
    score: 15000,
    subscore: 0,
    metadata: new Dictionary<string, string> {
        { "mode", "weekly_tournament" },
        { "build", "1.2.0" }
    });

if (response != null && response.success) {
    Debug.Log("Score accepted");
} else {
    Debug.LogWarning(response?.error ?? "Submit failed");
}
```

**Error handling:** Returns `null` on session failure, missing ids, RPC errors, or exceptions. Subscribe to `IVXGLeaderboardManager.OnError` for diagnostics.

---

## `GetLeaderboardAsync` (aggregate fetch)

The Unity SDK exposes **`GetAllLeaderboardsAsync`** rather than a single-id `GetLeaderboardAsync`:

| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| `limit` | `int` | `50` | Max records per leaderboard segment returned by the RPC. |

| Returns | Description |
|---------|-------------|
| `Task<IVXGAllLeaderboardsResponse>` | Contains daily / weekly / monthly / all-time slices, global variants, and `player_ranks`. |

**Fallback:** If the aggregate RPC fails, the manager falls back to per-period RPC `get_time_period_leaderboard` for `daily`, `weekly`, `monthly`, `alltime` × `game` / `global`.

**Example:**

```csharp
var all = await IVXGLeaderboardManager.GetAllLeaderboardsAsync(limit: 25);
if (all != null && all.success) {
    var topAllTime = all.alltime?.records;
    // Bind to UI
}
```

For a **Nakama-native** read by explicit id (other engines), use the client’s list-records API or your server RPC.

---

## `GetAroundMeAsync` (player-relative records)

Nakama supports **owner** / **cursor** APIs to fetch records around the current player. The Unity `IVXGLeaderboardManager` does not expose a method named `GetAroundMeAsync`; instead:

- Use **`GetPlayerRankAsync(bool global)`** for rank summary.
- Use **`GetPlayerBestScoreAsync(bool global)`** for best score from fetched data.
- For true “around me” windows, call Nakama `ListLeaderboardRecordsAroundOwnerAsync` with your session and target leaderboard id (from `IVXGAllLeaderboardsResponse` / server config).

**Example (conceptual Nakama):**

```csharp
// After resolving leaderboardId from server config or RPC response
var around = await client.ListLeaderboardRecordsAroundOwnerAsync(
    session, leaderboardId, session.UserId, limit: 10);
```

---

## `DeleteScoreAsync`

Score deletion is **server-dependent** (permissions and hooks). Many games never delete competitive scores; for test accounts, use Nakama console or a privileged RPC. If your project adds `delete_score` RPC, wrap it the same way as `submit_score_and_sync`. Document the RPC id in your game backend.

---

## Additional Unity static API

| Method | Returns | Description |
|--------|---------|-------------|
| `GetPlayerRankAsync(bool global)` | `Task<int>` | Rank from last aggregate fetch path. |
| `GetPlayerBestScoreAsync(bool global)` | `Task<long>` | Best score from cached leaderboard payload. |
| `ResetWinStreak()` | `void` | Clears local streak used on next submit. |

**Events:**

| Event | Payload | Description |
|-------|---------|-------------|
| `OnScoreSubmitted` | `IVXGScoreSubmissionResponse` | Successful submission path. |
| `OnLeaderboardsFetched` | `IVXGAllLeaderboardsResponse` | Aggregate fetch success. |
| `OnError` | `string` | Error message. |

---

## Metadata usage

- Keep metadata **small** (string keys/values or compact JSON).
- Use it for **display** (username hints) or **segmentation** (mode), not large blobs (use storage).
- Unity RPC path passes `metadata` through to the server payload as serialized fields.

---

## Hiro event leaderboards

For limited-time events, use the Hiro integration:

- Type: `IVXEventLeaderboardSystem` (`ListAsync`, etc.) under `Assets/Intelli-verse-X-SDK/Hiro/` (UPM package root).
- See [Hiro API](hiro.md) and [Hiro integration guide](../guides/hiro-integration.md).

Event leaderboards complement Nakama period leaderboards; configure both in backend and keep ids documented per game.

---

## Cross-platform notes

| Platform | Typical API |
|----------|-------------|
| Java `IVXManager` | `submitScore(leaderboardId, score)`, `fetchLeaderboard(id, limit)` |
| JavaScript | Leaderboard helpers on `IVXManager` / Web3 variant |
| Flutter | `ivx_manager` leaderboard methods |

Always authenticate before submit/fetch.

---

## See also

- [Leaderboard module](../modules/leaderboards.md)
- [Leaderboard demo](../samples/leaderboard-demo.md)
- [Hiro API](hiro.md)
