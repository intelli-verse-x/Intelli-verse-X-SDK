# Friends & Social Integration Guide

This guide walks through **Nakama-backed friends** (`IntelliVerseX.Social`) and how they connect to **Discord’s social graph** (`IntelliVerseX.Discord`) and **Hiro friend systems** (`IntelliVerseX.Hiro`). Use it as the end-to-end playbook for requests, lists, blocks, presence, and cross-platform UX.

---

## Overview

IntelliVerseX splits social into three layers that work together:

| Layer | Role |
|-------|------|
| **Nakama friends** | Authoritative friend graph: add/remove/block, incoming/outgoing requests, and list APIs via `IVXFriendsManager` (and the static `IVXFriendsService` facade for UI-oriented DTOs). |
| **Discord bridge** | `IVXDiscordFriends` merges Discord relationships with game (Nakama) relationships into `IVXUnifiedFriend` entries—dual `DiscordRelationshipType` and `GameRelationshipType`, plus online / in-game signals. |
| **Hiro** | Server RPCs for **Friend Quests** (co-op progress), **Friend Streaks** (daily bilateral engagement), and **Friend Battles** (async 1v1 challenges)—all expecting valid Nakama friend identities. |

!!! tip "When to use which API"
    Prefer **`IVXFriendsManager`** when you already have Nakama wired and want full control (socket, `IApiFriend`). Prefer **`IVXFriendsService`** for **`FriendInfo` / `FriendRequest`** lists and simple `bool` success paths without touching the manager singleton yourself.

---

## Prerequisites

1. **Nakama** — `IClient`, authenticated `ISession`, and (strongly recommended) a connected **`ISocket`** for friend-request notifications and presence (see [Backend module](../modules/backend.md)).
2. **`IVXFriendsManager`** — Persistent `GameObject` with the component, or allow the service to create one after `EnsureNakamaInitializedAsync()`.
3. **Discord (optional)** — `com.discord.social-sdk` installed, `INTELLIVERSEX_HAS_DISCORD` defined, `IVXDiscordConfig` assigned to `IVXDiscordManager` (see [Discord module](../modules/discord.md)).
4. **Hiro (optional)** — `IVXHiroCoordinator` initialized after login for friend quests, streaks, and battles (see [Hiro module](../modules/hiro.md)).

---

## Step 1 — Initialize Friends Manager

Call initialization **after** the player is authenticated. Passing the socket enables realtime friend notifications and presence-driven UI updates.

=== "Manager (recommended for full control)"

    ```csharp
    using IntelliVerseX.Social;
    using Nakama;

    void OnNakamaReady(IClient client, ISession session, ISocket socket)
    {
        var friends = IVXFriendsManager.Instance;
        friends.Initialize(client, session, socket);

        IVXFriendsEvents.OnFriendRequestReceived += OnFriendRequestReceived;
        IVXFriendsEvents.OnFriendPresenceChanged += userId => RefreshRow(userId);
        IVXFriendsEvents.OnFriendsError += msg => Debug.LogWarning(msg);
    }
    ```

=== "Reflection bootstrap"

    ```csharp
    if (!IVXFriendsManager.Instance.InitializeFromNakamaManager())
    {
        Debug.LogError("Friends: IVXNManager not ready (Client/Session).");
        return;
    }
    ```

=== "Static service bootstrap"

    ```csharp
    // Ensures IVXNManager + session, then creates/uses IVXFriendsManager
    bool ok = await IVXFriendsService.EnsureNakamaInitializedAsync();
    ```

Subscribe to **`IVXFriendsEvents`** early in your UI/lifecycle controller so incoming requests and presence changes update without polling.

---

## Step 2 — Send Friend Requests (by userId and by username)

Nakama’s **`AddFriendsAsync`** is exposed as **`AddFriendByIdAsync`** on the manager. It both **sends** an invite and **accepts** an incoming one (mutual add when appropriate).

**By user ID**

```csharp
await IVXFriendsManager.Instance.AddFriendByIdAsync(targetUserId, cancellationToken);
```

**By username**

Nakama lookup is **exact username** via `GetUsersAsync`. Resolve the user, then add by id:

```csharp
var mgr = IVXFriendsManager.Instance;
var user = await mgr.SearchUserByUsernameAsync(username.Trim(), ct);
if (user == null) { /* show “not found” */ return; }
await mgr.AddFriendByIdAsync(user.Id, ct);
```

**Via static service (userId)**

```csharp
bool sent = await IVXFriendsService.SendFriendRequestAsync(targetUserId, message: null, ct);
```

!!! note "Module reference vs. convenience"
    The [Social module](../modules/social.md) documents **`AddFriendByUsernameAsync`** as a single-call convenience. If your package version includes it, you may use that instead of **search + `AddFriendByIdAsync`**. The two-step pattern above matches the current Nakama-native manager surface.

---

## Step 3 — Accept / Reject Friend Requests (list pending, accept, reject)

**List incoming requests** (Nakama state **2 = invite received**):

```csharp
var incoming = await IVXFriendsManager.Instance.GetPendingRequestsAsync(ct);
foreach (var row in incoming)
{
    var id = row.User.Id;
    var name = row.User.DisplayName ?? row.User.Username;
    // bind UI
}
```

Or with DTOs:

```csharp
var requests = await IVXFriendsService.GetIncomingRequestsAsync(ct);
// FriendRequest.requestId / fromUserId == other user’s Nakama id
```

**Accept** — add the requester’s user id (same as accepting in Nakama):

```csharp
await IVXFriendsManager.Instance.AddFriendByIdAsync(fromUserId, ct);
// or
await IVXFriendsService.AcceptRequestAsync(fromUserId, ct);
```

**Reject / decline** — delete the friendship edge for that user (Nakama **`DeleteFriendsAsync`**):

```csharp
await IVXFriendsManager.Instance.RemoveFriendAsync(fromUserId, ct);
// or
await IVXFriendsService.RejectRequestAsync(fromUserId, ct);
```

!!! info "Notifications"
    With a connected socket, the manager listens for **`friend_request`** notifications and raises **`IVXFriendsEvents.OnFriendRequestReceived`** (and the instance event on **`IVXFriendsManager`**). Use that to prompt the player without polling.

---

## Step 4 — View Friends List (online status, filtering)

**Confirmed friends** (state **0**):

```csharp
var list = await IVXFriendsManager.Instance.GetFriendsAsync(ct);
foreach (var f in list)
{
    var u = f.User;
    // Presence: enrich from socket/status or unified Discord layer
}
```

**DTO list + UI event**

```csharp
IVXFriendsService.OnFriendsListUpdated += friends => Rebind(friends);
var friends = await IVXFriendsService.GetFriendsAsync(ct);
```

**Filtering ideas**

| Goal | Approach |
|------|----------|
| Friends only | `GetFriendsAsync` / `GetFriendsAsync` via service (state 0). |
| Pending inbound | `GetPendingRequestsAsync`. |
| Blocked | `GetBlockedUsersAsync` on the manager (state **3**). |
| Online first | Sort using realtime presence or **`IVXDiscordFriends`** `IsOnline` / `IsInGame` when Discord is enabled. |

**Refresh cache + broadcast**

```csharp
await IVXFriendsManager.Instance.RefreshFriendsAsync(ct);
```

`RefreshFriendsAsync` updates the internal cache and raises list-changed signals for bound UI.

---

## Step 5 — Block / Unblock Users

**Block** — uses Nakama **`BlockFriendsAsync`**:

```csharp
await IVXFriendsManager.Instance.BlockFriendAsync(userId, ct);
// or IVXFriendsService.BlockUserAsync(userId, reason: null, ct);
```

**List blocked users**

```csharp
var blocked = await IVXFriendsManager.Instance.GetBlockedUsersAsync(ct);
```

**Unblock**

Nakama clears a block by removing that relationship via the client’s friend-delete path. In the current manager, use **`RemoveFriendAsync`** against the blocked user id after confirming your server/Nakama version’s semantics, or implement an explicit unblock RPC if your project adds one. Validate in staging before shipping.

!!! warning "UX"
    Blocking should be obvious in UI and should cancel pending invites with that user to avoid confusing “ghost” requests.

---

## Step 6 — Discord Friends Integration (`IVXDiscordManager` + unified list)

`IVXDiscordFriends` sits alongside **`IVXDiscordManager`** (init, linking, settings). It provides a **unified** list: Discord-only, game-only, or **both**, with per-layer relationship enums.

```csharp
using IntelliVerseX.Discord;

var f = IVXDiscordFriends.Instance;
f.OnFriendsUpdated += list => BindUnifiedList(list);
f.Refresh();

var inGame = f.GetInGameFriends();
var discordOnly = f.GetBySource(IVXFriendSource.Discord);
```

**Game-side requests** (Nakama) from the Discord layer:

- `SendGameFriendRequest`, `SendGameFriendRequestById`
- `AcceptGameFriendRequest`, `RejectGameFriendRequest`, `CancelGameFriendRequest`, `RemoveGameFriend`

**Discord-side requests**

- `SendDiscordFriendRequest`, `SendDiscordFriendRequestById`
- `AcceptDiscordFriendRequest`, `RejectDiscordFriendRequest`, `CancelDiscordFriendRequest`, `RemoveDiscordAndGameFriend`

**Cross-platform mental model**

- Use **`GameUserId`** / **`DiscordUserId`** on **`IVXUnifiedFriend`** to route invites, DMs, or profile deep links.
- Players may be friends in Discord but not in Nakama—drive **“add in game”** flows with the game APIs above.

!!! info "Stub mode"
    Without the Discord UPM package, IVX Discord types operate in **stub mode** (safe no-ops / mock data). WebGL targets typically do not run the native Discord Social SDK—plan a Nakama-only path.

---

## Step 7 — Online Presence & Status (who’s online / in-game)

**Nakama / IVXFriendsManager**

- Attach **`ISocket`** during **`Initialize`** so **`ReceivedStatusPresence`** can propagate **`IVXFriendsEvents.OnFriendPresenceChanged(userId)`**.
- On callback, re-query that friend’s row or patch a single list item.

**Discord unified list**

- **`IVXUnifiedFriend.IsOnline`**, **`IsInGame`**, **`ActivityText`**, **`CanInvite`** support rich UI (“Playing Ranked”, invite eligible).
- **`IVXDiscordPresence`** (see Discord module) drives what Discord shows; align in-game labels with the same session state where possible.

**Social proof / live-ops**

- Hiro’s **`IVXSocialPressureSystem`** can complement raw presence with feeds and counters—useful for hub screens (details in [Hiro module](../modules/hiro.md)).

---

## Step 8 — Friend Quests & Battles (Hiro tie-in)

Hiro expects **real friend relationships** on Nakama before cooperative or competitive social features feel fair and cheat-resistant.

| System | Class | Use case |
|--------|--------|----------|
| Co-op quests | `IVXFriendQuestSystem` | Shared progress; **`AcceptAsync(questId, partnerId)`**, **`ReportProgressAsync`**. |
| Daily engagement | `IVXFriendStreakSystem` | **`InteractAsync(friendId)`**, milestone claims. |
| Async PvP | `IVXFriendBattleSystem` | **`SendChallengeAsync`**, **`AcceptChallengeAsync`**, **`SubmitScoreAsync`**. |

```csharp
var hiro = IVXHiroCoordinator.Instance;

var questState = await hiro.FriendQuests.GetAsync();
// await hiro.FriendQuests.AcceptAsync(questId, friendUserId);
// await hiro.FriendQuests.ReportProgressAsync(questId, amount: 1);

// var battle = await hiro.FriendBattles.SendChallengeAsync(friendUserId, "your_mode", score: optional);
```

Server RPC names include `hiro_friend_quest_*`, `hiro_friend_streak_*`, and `hiro_friend_battle_*`—keep client calls behind the Hiro systems so validation stays server-side.

---

## Best Practices

1. **Initialize after auth** — Never call friend APIs with a null or expired session; listen for session refresh and re-`Initialize` if needed.
2. **Pass the socket** — Friend requests and presence are dramatically better with **`ISocket`** connected.
3. **Debounce UI refresh** — `OnFriendPresenceChanged` can fire often; throttle list rebuilds and prefer per-row updates.
4. **Exact username search** — `SearchUserByUsernameAsync` is **exact match**; partial search needs a custom Nakama RPC (documented pattern in manager XML).
5. **Single source of truth** — Use **Nakama** for game logic (Hiro, matchmaking); use **Discord** for discovery, voice, and rich presence—merge in UI with **`IVXDiscordFriends`**.
6. **Cancellation** — Pass **`CancellationToken`** through async UI flows so scene teardown does not complete stale tasks.
7. **Error surface** — Subscribe to **`IVXFriendsEvents.OnFriendsError`** and **`IVXFriendsService.OnError`** for user-visible toasts vs. logs.

---

## Troubleshooting

| Symptom | Things to check |
|---------|------------------|
| `InitializeFromNakamaManager` fails | `IVXNManager` present, **`Client`** / **`Session`** non-null, assembly **`IntelliVerseX.V2`** loaded. |
| No incoming request UI | Socket connected? Notification subject **`friend_request`** enabled on server; handler registered for **`OnFriendRequestReceived`**. |
| “User not found” on username | Exact username only; typos / case sensitivity; user must exist on same Nakama project. |
| Accept does nothing | Accept uses **requester’s user id** (`AddFriendByIdAsync`), not an arbitrary opaque id. |
| Discord list empty | Account linking state, **`INTELLIVERSEX_HAS_DISCORD`**, stub mode, or platform (e.g. WebGL) limitations. |
| Hiro friend feature errors | Hiro coordinator initialized? Partner actually a **Nakama friend**? RPCs deployed for your Nakama module version. |

---

## See Also

- [Social module](../modules/social.md) — `IVXFriendsManager`, **`IVXFriendsEvents`**, share/rate helpers, and patterns.
- [Discord module](../modules/discord.md) — **`IVXDiscordFriends`**, lobbies, voice, invites, DMs.
- [Hiro module](../modules/hiro.md) — Friend Quests, Streaks, Battles, RPC tables.
- [Social API reference](../api/social.md) — Alternate **`FriendInfo`** / request DTO reference (cross-check with shipped **`IVXFriendsService`**).
- [Friends demo sample](../samples/friends-demo.md) — Scene **`IVX_Friends.unity`** and UI wiring.

---

*Last expanded for IntelliVerseX docs — aligns Nakama-native friends, Discord unified relationships, and Hiro social systems.*
