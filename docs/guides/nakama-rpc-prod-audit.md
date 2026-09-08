# Prod Nakama RPC audit (Unity SDK)

**Date:** 2026-09-07  
**Source of truth:** production Nakama via [nakama-mcp](https://nakama-mcp.intelli-verse-x.ai/) (`nakama_health`, `nakama_rpc`)  
**Base URL:** `https://nakama-rest.intelli-verse-x.ai`  
**Runtime health:** `nakama_js_health` → `ok: true`, `ts_owned_rpc_count: 867`

## Summary (Unity index × prod)

Probed all **162** RPCs in `Packages/com.intelliversex.sdk/Editor/RpcIndex/IVXRpcIndex.generated.json`:

| Result | Count | Meaning |
|--------|------:|---------|
| Registered on prod | 162 | **0 × `RPC function not found`** |
| Empty-payload soft success / data | ~52 | Handler ran (incl. some `success:false` auth messages) |
| Auth / permission gate | ~70 | Needs user session (expected for client RPCs) |
| Validation / missing fields | ~40 | Needs `gameId` / `device_id` / ids (expected) |

Empty-payload probes use the MCP **http_key** path (no `ctx.userId`). Client Play Mode with a real session is required for wallet / daily rewards / identity.

## Removed from Unity SDK (dead aliases)

These IDs are **no longer referenced** in the Unity package (index regenerated):

- `hiro_friend_battle_*`, `hiro_friend_quest_*`, `hiro_friend_streak_*`
- `friend_streaks_get` / `friend_streaks_interact`
- `friend_quests_get_active` / `friend_quests_contribute`

Use only the canonical replacements in the table above.

## Working core client RPCs (spot-checked)

| RPC | Notes |
|-----|--------|
| `get_all_leaderboards` | Needs `game_id` + `device_id` |
| `get_time_period_leaderboard` | Periods: `daily` / `weekly` / `monthly` / **`alltime`** (not `all_time`) |
| `wallet_get_balances` | Needs authenticated session + `gameId` (UUID) |
| `daily_rewards_*` | Needs authenticated session + UUID `gameId` |
| `nakama_js_health` | Always OK on prod |
| `send_friend_challenge`, `list_pending_friend_challenges`, … | Auth required; registered |
| `friend_streak_*` (5 RPCs) | Auth required; registered |
| `friend_quest_get_state`, `friend_quest_complete` | Auth required; registered |

QuizVerse game UUID (registry): `126bf539-dae2-4bcf-964d-316c0fa1f92b` (slug `quizverse`).

## Unity code updates (this pass)

- `IVXFriendBattleSystem` → canonical challenge RPCs  
- `IVXFriendQuestSystem` → `friend_quest_get_state` / `friend_quest_complete`  
- `IVXFriendStreakManager` → `friend_streak_*` + `friend_quest_*`  
- Models aligned to prod JSON field names  

**Follow-up (2026-09-08):** non-Social remaps — [nakama-rpc-prod-audit-nonsocial.md](nakama-rpc-prod-audit-nonsocial.md). Social clans/friends/chat/push/referral — [nakama-rpc-prod-audit-social.md](nakama-rpc-prod-audit-social.md). **Usage:** [social-nakama-usage.md](social-nakama-usage.md). 

## How to re-probe (CI / local)

```powershell
# Health
$body = '{"jsonrpc":"2.0","id":1,"method":"tools/call","params":{"name":"nakama_health","arguments":{}}}'
Invoke-RestMethod -Uri https://nakama-mcp.intelli-verse-x.ai/ -Method POST -ContentType application/json -Body $body

# Single RPC
$body = '{"jsonrpc":"2.0","id":2,"method":"tools/call","params":{"name":"nakama_rpc","arguments":{"rpc_id":"nakama_js_health","payload":{}}}}'
Invoke-RestMethod -Uri https://nakama-mcp.intelli-verse-x.ai/ -Method POST -ContentType application/json -Body $body

# Full Unity index probe
powershell -File tools/probe-prod-rpcs.ps1
```

See also: [nakama-integration.md](nakama-integration.md), [ivx-devops-cicd.md](skills/ivx-devops-cicd.md) (CI section).
