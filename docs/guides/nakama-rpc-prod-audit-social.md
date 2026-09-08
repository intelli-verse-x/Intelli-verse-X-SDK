# Prod Nakama Social zone audit (Unity SDK)

**Date:** 2026-09-08  
**Probe:** [nakama-mcp](https://nakama-mcp.intelli-verse-x.ai/)  
**Artifact:** `docs/guides/rpc-prod-probe-social.json`

## Scope

1. Groups / clans  
2. Friends (+ challenges / streaks / quests)  
3. Chat / messaging  
4. Notifications (push + in-game)  
5. Referral / social pressure / mailbox  

## Probe result (84 candidates)

| Status | Count |
|--------|------:|
| AUTH (registered, needs session) | 25 |
| SOFT (registered, missing field) | 2 |
| NOT_FOUND | 57 |

## Canonical prod map

### 1) Groups / clans

| Unity path | Prod | Notes |
|------------|------|-------|
| `get_user_groups` | **LIVE** | Load current clan |
| `create_game_group` | **LIVE** | Create guild (`groupType=guild`) |
| Browse / join / leave / members | **Nakama native** | `ListGroupsAsync`, `JoinGroupAsync`, `LeaveGroupAsync`, `ListGroupUsersAsync` — custom `clan_*` / `list_groups` RPCs do **not** exist |

**Code:** `IVXClanService` hardened with Newtonsoft + `game_id`/`group_id` aliases.

### 2) Friends

| Path | Prod | Notes |
|------|------|-------|
| Native `ListFriendsAsync` / `AddFriendsAsync` / `DeleteFriendsAsync` / `BlockFriendsAsync` | Correct primary | No `add_friend` custom RPC |
| `friends_list` | **LIVE** (auth) | Optional enrichment |
| `friends_remove` | **LIVE** (auth) | Side-effect after native delete |
| `friends_add` | **NOT_FOUND** | Use native add |
| Challenges / streaks / quests | **LIVE** (auth) | Already remapped earlier |
| User search | Native `GetUsersAsync` (exact) | No `search_users` / `player_search` RPC |

**Code:** `IVXFriendsManager` keeps native primary; adds `friends_list` / `friends_remove` hooks.

### 3) Chat

| Custom `chat_*` / `dm_*` / `channel_*` | **All NOT_FOUND** |
| Async inbox | `hiro_mailbox_*` **LIVE** (auth) |
| Live ops inbox | `satori_messages_*` **LIVE** (auth) |

**Code:** new `IVXSocialChatService` — Nakama native DM / clan channels (`JoinChatAsync`, `WriteChatMessageAsync`, `ListChannelMessagesAsync`). Use Hiro mailbox for async gifts.

### 4) Notifications

| RPC | Status |
|-----|--------|
| `push_register_token` | **LIVE** (auth) |
| `push_get_endpoints` | **LIVE** (needs userId on server-key) |
| `push_send_event` | **LIVE** (needs userId) |
| `notification_list` etc. | **NOT_FOUND** → use native `ListNotificationsAsync` |

**Code:** `IVXPushNotificationManager` richer token payload + `SendEventAsync` + `ListInGameNotificationsAsync`.

### 5) Referral / other

| RPC | Status |
|-----|--------|
| `hiro_incentives_referral_code` | **LIVE** (auth) |
| `hiro_incentives_apply_referral` | **LIVE** (auth) |
| `hiro_social_pressure_get` | **LIVE** (auth) |
| HTTP-only `referral_get` | **NOT_FOUND** |

**Code:** new `IVXNakamaReferralService`; `IVXReferralUI` prefers Nakama then falls back to `APIManager`.

## Re-probe

```powershell
# See tools/probe-prod-rpcs.ps1 or re-run the social candidate list against nakama_rpc
```

See also: [nakama-rpc-prod-audit-nonsocial.md](nakama-rpc-prod-audit-nonsocial.md), [nakama-rpc-prod-audit.md](nakama-rpc-prod-audit.md).
