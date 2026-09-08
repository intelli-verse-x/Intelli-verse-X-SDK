# Social + Nakama usage (prod-aligned)

How to use Unity Social / push / referral features against **production Nakama**.  
Source of truth audits: [social](nakama-rpc-prod-audit-social.md) · [non-social](nakama-rpc-prod-audit-nonsocial.md) · [friends/challenges](nakama-rpc-prod-audit.md)

**Prerequisite:** authenticated Nakama session (`IVXBootstrap` / `IVXNManager`).

---

## 1) Friends

Primary path is **Nakama native** APIs (no `friends_add` RPC on prod).

```csharp
using IntelliVerseX.Social;
using Nakama;

var friends = IVXFriendsManager.Instance;
friends.InitializeFromNakamaManager();

// List confirmed friends
var list = await friends.GetFriendsAsync();

// Send / accept (mutual add)
await friends.AddFriendByIdAsync(otherUserId);

// Remove (native delete + optional prod friends_remove side-effect)
await friends.RemoveFriendAsync(otherUserId);

// Search = exact username via GetUsersAsync
var users = await friends.SearchUsersAsync("ExactName");
```

Optional enrichment RPC (auth): `friends_list` via `GetFriendsPreferRpcAsync()`.

Friend challenges / streaks / quests (Hiro):

| Feature | Canonical RPCs |
|---------|----------------|
| Challenges | `send_friend_challenge`, `accept_friend_challenge`, `decline_friend_challenge`, `cancel_friend_challenge`, `list_pending_friend_challenges` |
| Streaks | `friend_streak_get_state`, `friend_streak_record_contribution`, `friend_streak_send_nudge`, `friend_streak_repair` |
| Quests | `friend_quest_get_state`, `friend_quest_complete` |

Use `IVXHiroCoordinator.Instance.FriendBattles` / `FriendStreaks` / `FriendQuests` (or `IVXFriendStreakManager`).

---

## 2) Clans / groups

| Action | API |
|--------|-----|
| Load my clan | RPC `get_user_groups` (`IVXClanService.LoadCurrentClanAsync`) |
| Create guild | RPC `create_game_group` (`CreateClanAsync`, `groupType=guild`) |
| Browse / search | Native `ListGroupsAsync` |
| Join / leave | Native `JoinGroupAsync` / `LeaveGroupAsync` |
| Members | Native `ListGroupUsersAsync` |

```csharp
using IntelliVerseX.Social;

var client = /* IVXNManager.Instance.Client */;
var session = /* IVXNManager.Instance.Session */;
string gameId = /* UUID from Bootstrap */;

var load = await IVXClanService.LoadCurrentClanAsync(client, session, gameId);
var create = await IVXClanService.CreateClanAsync(client, session, gameId, "MyClan", "desc", true, 50);
var browse = await IVXClanService.BrowseClansAsync(client, session, "quiz", 20);
await IVXClanService.JoinClanAsync(client, session, clanId);
```

Do **not** call dead `clan_*` / `list_groups` custom RPCs — they are not registered on prod.

---

## 3) Chat

Prod has **no** `chat_*` / `dm_*` custom RPCs. Use Nakama **native channels**:

```csharp
using IntelliVerseX.Social;

ISocket socket = /* connected socket */;

// 1:1 DM
var dm = await IVXSocialChatService.JoinDirectAsync(socket, otherUserId);
await IVXSocialChatService.SendTextAsync(socket, dm.Id, "gg");

// Clan / group chat
var clanChat = await IVXSocialChatService.JoinClanChatAsync(socket, groupId);
await IVXSocialChatService.SendTextAsync(socket, clanChat.Id, "Raid at 8");

// History
var history = await IVXSocialChatService.ListHistoryAsync(client, session, dm.Id, limit: 50);
```

Async gifts / mailbox (not realtime chat): Hiro `hiro_mailbox_list` / `claim` / `claim_all` / `delete` via `IVXHiroCoordinator.Instance.Mailbox`.

---

## 4) Notifications

### Push (device)

```csharp
using IntelliVerseX.Notifications;

var push = IVXPushNotificationManager.Instance;
push.Initialize(client, session);

await push.RegisterTokenAsync(deviceToken, PushPlatform.FCM);
var endpoints = await push.GetEndpointsAsync();
await push.SendEventAsync("daily_reward_available", "Reward ready", "Claim now", deepLink: "ivx://rewards");
```

Prod RPCs: `push_register_token`, `push_get_endpoints`, `push_send_event`.

### In-game (Nakama notifications)

No `notification_list` RPC — use native:

```csharp
var notes = await IVXPushNotificationManager.ListInGameNotificationsAsync(client, session, limit: 50);
```

Friend-request realtime still comes from socket `ReceivedNotification` (`subject == friend_request`).

---

## 5) Referral / invite

Prefer Nakama when a session exists:

```csharp
using IntelliVerseX.Social;

var code = await IVXNakamaReferralService.GetReferralCodeAsync(client, session);
Debug.Log(code?.ResolvedCode);

await IVXNakamaReferralService.ApplyReferralCodeAsync(client, session, "FRIEND123");
```

RPCs: `hiro_incentives_referral_code`, `hiro_incentives_apply_referral`.  
`IVXReferralUI` tries this path first, then falls back to HTTP `APIManager`.

---

## 6) Wallet / missions / retention (related non-Social)

| Feature | Prefer |
|---------|--------|
| Wallet read/write | `wallet_get_balances`, `wallet_update_game_wallet` |
| Daily missions | `daily_missions_get` / `update_progress` / `claim` |
| Retention heartbeat | `hiro_retention_get`, `hiro_retention_heartbeat` |
| Tournaments list | `tournament_list` |
| Achievements progress/claim | `hiro_achievements_progress`, `hiro_achievements_claim` |
| Badges catalog | `badges_get_all` |

See [nakama-rpc-prod-audit-nonsocial.md](nakama-rpc-prod-audit-nonsocial.md).

---

## Re-probe prod

```powershell
powershell -File tools/probe-prod-rpcs.ps1
```

MCP: `https://nakama-mcp.intelli-verse-x.ai/` (`nakama_health`, `nakama_rpc`).
