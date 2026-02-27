# Global Cross-Game Friends System — Architecture Specification

**Version:** 1.0 | **Date:** 2026-02-27 | **Status:** Design Complete

---

## 1. Current System Analysis (Unity + Nakama)

### 1.1 Unity SDK — Friends Flow

| Component | Location | Behavior |
|-----------|----------|----------|
| **IVXFriendsService** | `Assets/_IntelliVerseXSDK/Social/Runtime/IVXFriendsService.cs` | Static API: `GetFriendsAsync`, `GetIncomingRequestsAsync`, `SearchUsersAsync`, `SendFriendRequestAsync`, `AcceptRequestAsync`, `RejectRequestAsync`, `RemoveFriendAsync`, `BlockUserAsync` |
| **APIManager** | `Assets/_IntelliVerseXSDK/Identity/APIManager.cs` | All friends operations use HTTP API: `GetFriendsAsync`, `GetIncomingRequestsAsync`, `SearchFriendsAsync`, `SendFriendInviteAsync`, `UpdateFriendStatusAsync` |
| **IVXFriendsPanel** | `Assets/_IntelliVerseXSDK/Social/UI/IVXFriendsPanel.cs` | UI calls `IVXFriendsService`; uses `FriendInfo`, `FriendRequest`, `FriendSearchResult` models |
| **IVXModels** | `Assets/_IntelliVerseXSDK/Identity/IVXModels.cs` | `FriendData` (relationId, status, user), `FriendsResponse`, `SearchUser`, `FriendInvitePayload`, `FriendStatusUpdatePayload` |
| **Auth/Session** | `UserSessionManager.Current` | Uses IntelliVerse HTTP auth (accessToken, userId, userName). **Not** Nakama session. |

**Current Data Flow:**
```
Unity → IVXFriendsService → APIManager → HTTP GET/POST/PATCH → api.intelli-verse-x.ai/api/games/friends/*
```

**URLs (IVXURLs.cs):**
- `GetFriendList` = `https://api.intelli-verse-x.ai/api/games/friends`
- `SearchFriend` = `https://api.intelli-verse-x.ai/api/games/friends/search`
- `SendFriendRequest` = `https://api.intelli-verse-x.ai/api/games/friends/invite`
- `UpdateFriendStatus` = `https://api.intelli-verse-x.ai/api/games/friends/status`

**Accept/Reject/Remove/Block:** All use `UpdateFriendStatusAsync(relationId, status)` → PATCH to `api/games/friends/status`.

---

### 1.2 Nakama Backend — Friends Flow

| Component | Location | Behavior |
|-----------|----------|----------|
| **send_friend_invite** | `data/modules/index.js` ~3772 | Writes to `friend_invites` storage, sends notification. Does **not** call `nk.friendsAdd`. |
| **accept_friend_invite** | `data/modules/index.js` ~3865 | Reads invite from `friend_invites`, calls `nk.friendsAdd(userId, [fromUserId], [fromUsername])`, updates invite status. |
| **decline_friend_invite** | `data/modules/index.js` ~3992 | Updates invite to "declined"; does **not** call Nakama friends API. |
| **friend_invites storage** | Custom collection | `collection: "friend_invites"`, key = inviteId, userId = targetUserId. |
| **get_friend_leaderboard** | `data/modules/index.js` ~3668 | Uses `nk.friendsList(userId, limit, null, null)` — expects Nakama native friends. |

**Nakama Native APIs Used:**
- `nk.friendsAdd` — only on accept
- `nk.friendsList` — only in get_friend_leaderboard RPC

**Nakama Native APIs NOT Used for Friends UI:** `nk.friendsRemove`, `nk.friendBlock`, `nk.friendUpdate`, `nk.friendsOfFriendsList`.

---

### 1.3 Disconnect Summary

| Aspect | Unity SDK | Nakama Backend |
|--------|-----------|----------------|
| **Friends source** | HTTP API (api.intelli-verse-x.ai) | RPCs + friend_invites storage |
| **SDK calls Nakama?** | ❌ No | N/A |
| **SDK calls HTTP API?** | ✅ Yes | N/A |
| **Nakama RPCs called by Unity?** | ❌ No (send/accept/decline never used) | RPCs registered but unused |
| **Leaderboard friend filter** | N/A | Uses `nk.friendsList` — would be empty if friends only in HTTP API |
| **Per-game?** | Unclear (HTTP API may use gameId) | Native Nakama friends are **user-based** |

---

## 2. Problems Identified

### 2.1 Architecture Fragmentation
- **Two independent systems:** Unity friends via HTTP API; Nakama friends via RPCs.
- **get_friend_leaderboard** relies on Nakama native friends; if users only use HTTP friends, that leaderboard is empty.
- **No single source of truth** for friends.

### 2.2 Non-Global / Potential Per-Game
- HTTP API (`api/games/friends`) suggests game-scoped endpoints; `gameId` may be used.
- Custom `friend_invites` storage is not per-game, but the HTTP API backend is unknown.

### 2.3 Nakama Underutilized
- Custom `friend_invites` collection duplicates Nakama’s invite model.
- Nakama’s native `AddFriendsAsync` sends invites; `ListFriendsAsync` returns INVITE_SENT, INVITE_RECEIVED, FRIEND, BLOCKED.
- RPCs add complexity and storage without leveraging native APIs.

### 2.4 Accept/Reject Semantics Mismatch
- Unity uses `relationId` (from HTTP API) for accept/reject.
- Nakama RPCs use `inviteId` (storage key).
- Nakama native flow: accept = `AddFriendsAsync(initiatorId)`; reject = `DeleteFriendsAsync(initiatorId)`.

### 2.5 Auth Identity Split
- Friends use `UserSessionManager` (IntelliVerse HTTP auth).
- Profile/Wallet/Leaderboard use Nakama (`IVXNManager`, `IVXNProfileManager`, `IVXNWalletManager`).
- Must ensure same user identity across HTTP and Nakama (Nakama userId = IntelliVerse userId via account linking).

### 2.6 Missing Capabilities
- **Block:** HTTP API supports it; Nakama RPCs do not expose `nk.friendBlock`.
- **GetBlockedUsers:** Not implemented.
- **Rate limiting:** Not enforced on server.
- **Self-add validation:** HTTP API may check; Nakama RPCs do not explicitly validate.

---

## 3. Required Backend Changes (Nakama)

### 3.1 Principle
Use **only** Nakama native friend APIs. Remove custom `friend_invites` and related RPCs.

### 3.2 Native API Mapping

| Operation | Nakama Server API | Notes |
|-----------|-------------------|-------|
| Add friend / Send request | `nk.friendsAdd(userId, [targetId], [username])` | Creates invite if not yet friends |
| Accept request | `nk.friendsAdd(userId, [initiatorId], [username])` | Mutually adds |
| Reject request | `nk.friendsRemove(userId, [initiatorId])` | Declines |
| Remove friend | `nk.friendsRemove(userId, [friendId])` | Removes relationship |
| Block user | `nk.friendBlock(userId, targetId)` | Blocks |
| List friends | `nk.friendsList(userId, limit, state, cursor)` | state: 0=FRIEND, 1=INVITE_SENT, 2=INVITE_RECEIVED, 3=BLOCKED |
| Friends of friends | `nk.friendsOfFriendsList(userId, limit)` | Optional discovery |
| Update state | `nk.friendUpdate(userId, targetId, state)` | If needed for custom flows |

### 3.3 Required RPCs (Thin Wrappers for Validation)

| RPC | Purpose | Server Logic |
|-----|---------|--------------|
| `friends_add` | Send request or accept | Validate payload; `nk.friendsAdd`; rate limit |
| `friends_remove` | Reject or remove | Validate; `nk.friendsRemove` |
| `friends_block` | Block user | Validate; `nk.friendBlock` |
| `friends_list` | List by state | Validate; `nk.friendsList`; optionally merge with profile metadata |
| `friends_search` | Search users by username | `nk.usersGetUsername` or custom logic; filter blocked/self |

**Deprecate/Remove:**
- `send_friend_invite`
- `accept_friend_invite`
- `decline_friend_invite`
- `friend_invites` storage collection (after migration)

### 3.4 Server-Side Validation (All RPCs)

- **AUTH_REQUIRED:** `ctx.userId` must exist
- **SELF_ADD:** `targetUserId !== ctx.userId`
- **BLOCKED_CHECK:** Before add, ensure target has not blocked caller (optional; Nakama may enforce)
- **RATE_LIMIT:** e.g., max 20 add attempts per user per hour (Redis or in-memory)
- **USER_EXISTS:** `nk.usersGetId([targetId])` before add

### 3.5 Migrate `friend_invites` → Native

1. Read all `friend_invites` where `status === "pending"`.
2. For each: if targetUserId has not declined, call `nk.friendsAdd(targetUserId, [fromUserId], [fromUsername])` (simulate “accept” for historical pending).
3. Optionally notify users of migrated pending requests.
4. After validation period, delete `friend_invites` collection entries.

---

## 4. Required Unity SDK Changes

### 4.1 New FriendService Architecture

Introduce **IVXNFriendService** (or refactor **IVXFriendsService**) to use **Nakama client** instead of HTTP API.

**Prerequisite:** Session must be Nakama `ISession` (from `IVXNManager`), not only `UserSessionManager`.

**API surface (preserve existing where possible):**

| Method | Implementation | Notes |
|--------|----------------|------|
| `GetFriendsAsync()` | `Client.ListFriendsAsync(session, 0, limit, null)` | state 0 = FRIEND |
| `GetPendingRequestsAsync()` | `Client.ListFriendsAsync(session, 2, limit, null)` | state 2 = INVITE_RECEIVED |
| `GetBlockedUsersAsync()` | `Client.ListFriendsAsync(session, 3, limit, null)` | state 3 = BLOCKED |
| `SendFriendRequestAsync(userId)` | `Client.AddFriendsAsync(session, null, [userId])` | Or RPC for rate limit |
| `AcceptFriendRequestAsync(userId)` | `Client.AddFriendsAsync(session, null, [userId])` | userId = initiator |
| `RejectFriendRequestAsync(userId)` | `Client.DeleteFriendsAsync(session, null, [userId])` | userId = initiator |
| `RemoveFriendAsync(userId)` | `Client.DeleteFriendsAsync(session, null, [userId])` | |
| `BlockUserAsync(userId)` | `Client.BlockFriendsAsync(session, null, [userId])` | |
| `SearchUsersAsync(query)` | RPC `friends_search` | Username search; exclude self/blocked |

### 4.2 Model Mapping

| Unity Model | Nakama `IApiFriend` / List Response |
|-------------|------------------------------------|
| `FriendInfo.userId` | `friend.User.Id` |
| `FriendInfo.displayName` | `friend.User.DisplayName` or username |
| `FriendInfo.avatarUrl` | From profile/metadata RPC |
| `FriendInfo.isOnline` | Presence (if socket connected) |
| `FriendRequest.requestId` | Use `friend.User.Id` (Nakama uses userId for accept) |
| `FriendRequest.fromUserId` | `friend.User.Id` |

**Breaking change:** `requestId` becomes `userId`. Update `IVXFriendsPanel` to pass `fromUserId` to `AcceptRequestAsync` / `RejectRequestAsync`.

### 4.3 Session Source

- **Option A:** Use `IVXNManager.Session` when Nakama is primary. Require Nakama init before friends.
- **Option B:** Support dual auth — use Nakama session when available; fallback to HTTP only if Nakama disabled.
- **Recommended:** Nakama as single source; ensure account linking so IntelliVerse userId = Nakama userId.

### 4.4 Remove / Deprecate

- APIManager friends HTTP calls (`GetFriendsAsync`, `SendFriendInviteAsync`, etc.) — deprecate after migration.
- IVXURLs friends endpoints — keep for legacy fallback config if needed.

---

## 5. Migration Plan

### Phase 1: Backend Preparation
1. Add RPCs: `friends_add`, `friends_remove`, `friends_block`, `friends_list`, `friends_search` with validation.
2. Add rate limiting and security checks.
3. Run migration script: `friend_invites` pending → `nk.friendsAdd` where applicable.
4. Deprecate `send_friend_invite`, `accept_friend_invite`, `decline_friend_invite` (keep temporarily for old clients).

### Phase 2: Unity SDK — Nakama Path
1. Add `IVXNFriendService` (or extend `IVXFriendsService`) using Nakama client.
2. Resolve `ISession` from `IVXNManager`; require Nakama init before friends.
3. Map Nakama `IApiFriend` → `FriendInfo` / `FriendRequest`.
4. Update `IVXFriendsPanel` to use userId for accept/reject (remove relationId).
5. Add `GetBlockedUsersAsync`.

### Phase 3: Dual-Write (Optional)
- If HTTP API backend can write to Nakama: dual-write add/accept/remove/block during transition.
- Ensures users on old clients still populate Nakama friends.

### Phase 4: Cutover
1. Flip Unity SDK default to Nakama; remove HTTP friends calls from main path.
2. Remove `friend_invites` storage usage; delete collection after verification.
3. Remove deprecated RPCs.

### Phase 5: Verification
- Test: Add in Game A → verify friend appears in Game B.
- Test: Guest → Email upgrade → friends preserved.
- Test: Block, remove, reject flows.

---

## 6. Edge Case Handling Strategy

| Edge Case | Handling |
|-----------|----------|
| **Add self** | Server: `targetUserId === ctx.userId` → reject with `SELF_ADD`. Client: validate before call. |
| **Add already friend** | Nakama `friendsAdd` is idempotent; returns existing state. |
| **Accept already accepted** | Idempotent. |
| **Remove while request pending** | `DeleteFriendsAsync` removes invite. |
| **Blocked user** | Nakama prevents add from blocked user. Before add, optional check via `friendsList` state 3. |
| **Simultaneous mutual add** | Nakama handles atomically; one may see INVITE_RECEIVED then FRIEND. |
| **Cross-game version mismatch** | Same Nakama server + userId → consistent. Old client on HTTP may not see Nakama friends until migrated. |
| **Deleted account** | `usersGetId` returns empty; reject add. ListFriends may return stale entries until Nakama cleanup. |
| **Username change** | Nakama friends keyed by userId; no impact. |
| **Guest upgraded to email** | Preserve friends if account linking keeps same Nakama userId. |
| **Network interrupt mid-request** | Client retry with idempotent operations; server dedup if needed. |
| **Friend request spam** | Rate limit: e.g. 20 adds/hour per user. |
| **Request after block** | Nakama blocks automatically; add fails. |

---

## 7. Concurrency Handling Strategy

- **Nakama guarantees:** Friend operations are atomic; no duplicate relationships.
- **Idempotency:** `friendsAdd`, `friendsRemove`, `friendBlock` safe to retry.
- **List pagination:** Use cursor from `friendsList` for large lists.
- **Optimistic UI:** Update local state on success; revert on error.
- **Real-time:** Subscribe to Nakama socket `StatusPresence` for online status; optional `Notification` for new friend requests.

---

## 8. Security Hardening Plan

| Check | Where | Action |
|-------|-------|--------|
| No self-add | Server + Client | Reject `targetUserId === ctx.userId` |
| No bypass block | Server | Nakama enforces; no custom bypass |
| Rate limiting | Server | 20 add/block per user per hour |
| Validate target exists | Server | `nk.usersGetId([targetId])` before add |
| Auth required | Server | All RPCs require `ctx.userId` |
| Input validation | Server | userId format (UUID), query length for search |
| Do not trust client | Server | All state changes server-side |

---

## 9. Multi-Game Compatibility Validation

| Scenario | Validation |
|----------|------------|
| Same SDK, same Nakama, same user | ✅ Friends in Nakama are user-scoped; Game A and Game B share list. |
| Same auth (e.g. email) | ✅ Same Nakama userId → same friends. |
| Device auth | ⚠️ Device IDs are per-device; link to email/custom ID for global identity. |
| Guest upgrade | ✅ Account linking must preserve Nakama userId; friends then global. |
| 10+ games, 5M users | ✅ Nakama scales; friends stored once per user. |

---

## 10. Why This Architecture Is Production Safe

1. **Single source of truth:** Nakama native friends; no custom storage or HTTP API for core state.
2. **User-scoped:** Friends keyed by `userId`; no gameId in schema.
3. **Atomic operations:** Nakama handles races and duplicates.
4. **Battle-tested:** Native Nakama friends API used in production by many titles.
5. **Consistent with Profile/Wallet:** Same Nakama backend, same identity model.
6. **Scalable:** No N+1, no per-game buckets, pagination supported.
7. **Secure:** Server-side validation, rate limiting, no client trust.
8. **Migration path:** Clear phases; backward compatibility during transition.

---

## Appendix A: Nakama Unity Client Friends API Reference

```csharp
// Add (send request or accept)
await client.AddFriendsAsync(session, null, new[] { userId });

// List (state: 0=FRIEND, 1=INVITE_SENT, 2=INVITE_RECEIVED, 3=BLOCKED)
var list = await client.ListFriendsAsync(session, state: 0, limit: 100, cursor: null);

// Delete (reject or remove)
await client.DeleteFriendsAsync(session, null, new[] { userId });

// Block
await client.BlockFriendsAsync(session, null, new[] { userId });
```

---

## Appendix B: File Change Summary

| File | Change |
|------|--------|
| `nakama/data/modules/index.js` | Add friends_add, friends_remove, friends_block, friends_list, friends_search; deprecate send/accept/decline; migrate friend_invites |
| `Unity/IVXFriendsService.cs` | Use Nakama client; resolve session from IVXNManager |
| `Unity/APIManager.cs` | Deprecate friends HTTP methods |
| `Unity/IVXFriendsPanel.cs` | Use userId for accept/reject; update model binding |
| `Unity/IVXFriendsModels.cs` | Add GetBlockedUsers; ensure FriendRequest.requestId = userId |
