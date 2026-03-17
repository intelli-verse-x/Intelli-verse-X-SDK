
# Step 2 — Backend & Scripting Audit (Defold vs Unity SDK)

## Summary

- **One flow mismatch** (Defold uses a different path than Unity for score submit).
- **Two additive gaps** (Unity has RPCs that Defold did not expose).
- **No removal** of existing Defold APIs; only new functions added so current projects keep working.

---

## 1. Flow break (needs alignment)

### Score submission

| | Unity SDK | Defold SDK (before Step 2) |
|--|-----------|----------------------------|
| **API** | RPC **submit_score_and_sync** | Nakama native **write_leaderboard_record(leaderboard_id, score)** |
| **Payload** | user_id, device_id, game_id, score, subscore, current_streak, username, metadata | leaderboard_id, score only |
| **Backend** | Server returns rewards, wallet update, streak; can sync to multiple leaderboards | Only writes to one Nakama leaderboard; no rewards/wallet sync |

**Impact:** If your backend expects all score submissions to go through **submit_score_and_sync** (rewards, wallet, multi-leaderboard), then Defold’s current `submit_score(leaderboard_id, score)` **bypasses** that flow.

**Change in Step 2:** Add a new function **submit_score_and_sync(score, callback)** that calls the same RPC with the same contract as Unity (device_id, game_id, user_id from session/config). Keep existing **submit_score(leaderboard_id, score, callback)** for raw Nakama so nothing breaks.

---

## 2. Additive gaps (no break, adding parity)

### get_all_leaderboards

- **Unity:** RPC **get_all_leaderboards** with device_id, game_id, limit → returns all leaderboards in one call.
- **Defold:** Only **fetch_leaderboard(leaderboard_id, limit)** (single leaderboard via Nakama API).

**Change:** Add **fetch_all_leaderboards(limit, callback)** that calls the **get_all_leaderboards** RPC when your backend provides it.

### Wallet (V2 backend)

- **Unity:** Uses both **wallet_get_balances** (payload: gameId) and **hiro_economy_list**.
- **Defold:** Only **fetch_wallet** → **hiro_economy_list**.

**Change:** Add **fetch_wallet_balances(callback)** that calls **wallet_get_balances** so Defold can use the same backend when it uses that RPC. Keep **fetch_wallet** (Hiro) unchanged.

---

## 3. What is already correct (no change)

- **create_or_sync_user** — Done in Step 1 (Auth).
- **Profile** — fetch_profile / update_profile use Nakama get_account / update_account; no backend RPC required.
- **Storage** — read_storage / write_storage use Nakama storage; no change.
- **Hiro economy** — fetch_wallet (hiro_economy_list), grant_currency (hiro_economy_grant); unchanged.
- **call_rpc** — Generic RPC; unchanged. New functions use it or direct nakama.rpc where needed.

---

## 4. RPC names (Unity = Defold after Step 2)

| RPC | Unity | Defold (after Step 2) |
|-----|--------|------------------------|
| create_or_sync_user | ✓ | ✓ (Step 1) |
| submit_score_and_sync | ✓ | ✓ (new) |
| get_all_leaderboards | ✓ | ✓ (new) |
| wallet_get_balances | ✓ | ✓ (new) |
| hiro_economy_list | ✓ | ✓ (existing) |
| hiro_economy_grant | ✓ | ✓ (existing) |

No RPCs removed; no breaking changes to existing Defold APIs.

---

## Step 3 — Paywall / Premium

- **Unity:** IVXSubscriptionManager + ShowPaywall() (TODO: paywall UI). No flow break in the published Asset Store SDK; the TODO is intentional.
- **Defold:** No paywall UI in the SDK. Use **call_rpc** for any backend entitlement/subscription RPC; paywall UI is game-specific. See README “Paywall / Premium” section.
