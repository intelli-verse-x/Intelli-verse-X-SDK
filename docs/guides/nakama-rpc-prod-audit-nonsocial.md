# Prod Nakama RPC audit — non-Social (Unity SDK)

**Date:** 2026-09-08  
**Scope:** All Unity-indexed RPCs **except** Social / friend-* (left untouched per product request)  
**Probe:** [nakama-mcp](https://nakama-mcp.intelli-verse-x.ai/) `nakama_rpc` with QuizVerse `game_id`  
**Artifact:** `docs/guides/rpc-prod-probe-nonsocial.json`

## Probe summary (176 non-social)

| Status | Count | Meaning |
|--------|------:|---------|
| auth_or_perm | 97 | Needs real user session (expected) |
| soft_fail | 36 | Registered; missing field / not configured |
| success | 17 | Empty-payload OK |
| ok | 10 | Handler ran |
| **not_found** | **12** | Dead IDs Unity was still calling |
| error / exception | 4 | Transient MCP / validation (e.g. `mp_create_match` needs `template_id`) |

## Dead → canonical (code updated)

| Removed / dead | Canonical prod | Module updated |
|----------------|----------------|----------------|
| `achievements_track_progress` | `hiro_achievements_progress` | `Progression/IVXAchievementManager` |
| `achievements_claim_reward` | `hiro_achievements_claim` | same |
| `badges_check_unlock` / `badges_equip` | `badges_get_all` (+ client resolve) | `Progression/IVXBadgeManager` |
| `retention_get_state` | `hiro_retention_get` | `Retention/IVXRetentionManager` |
| `retention_check_in` | `hiro_retention_heartbeat` | same |
| `winback_get_offer` | `hiro_incentives_return_bonus` | same |
| `winback_claim_offer` | `hiro_retention_claim_comeback` | same |
| `tournament_get_active` | `tournament_list` | `Competition/IVXTournamentManager` |
| `weekly_goals_get` | `daily_missions_get` (bridge) | `Retention/IVXGoalsManager` |
| `monthly_milestones_get` | *not on prod* (empty + warn) | same |
| `season_pass_get_state` | *not on prod* (null + warn) | `SeasonPass/IVXSeasonPassManager` |
| `get_daily_missions` / `submit_mission_progress` / `claim_mission_reward` | `daily_missions_*` | `Backend/IVXNakamaRPC` |

## World-class hardening this pass

1. **`IVXHiroRpcClient`** — parses both `{ success, data }` **and** flat envelopes (`badges`, `achievements`, …).
2. **Wallet** — `IVXNakamaManager` prefers `wallet_get_balances` / `wallet_update_game_wallet`, falls back to legacy.
3. **Models** — badges (`title`/`rarity`/`displayed`), tournaments (`slug`/`pot_bc`/`entry_fee_bc`), retention (`bucket`/`lastSeen`).
4. **RPC index** — dead IDs removed; canonical IDs added (`Editor/RpcIndex/IVXRpcIndex.generated.json`).

## Still server-side / auth-only

- Season pass **get** missing on prod (claim / purchase / add_xp exist).
- Monthly milestones **list** missing (update RPC exists).
- Badge **equip** mutate RPC missing (catalog is read-only).
- `mp_voice_token` returned 500 on empty probe (server).
- Hiro systems requiring `ctx.userId` need Play Mode session.

## Re-probe

```powershell
powershell -File tools/probe-prod-rpcs.ps1
# Or non-social filtered run documented in prior Unity Tests / this guide's JSON artifact
```

See also: [nakama-rpc-prod-audit.md](nakama-rpc-prod-audit.md) (social pass), [nakama-integration.md](nakama-integration.md).
