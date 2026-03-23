# Web3 SDK Strategy & Features — For Review (Seniors & Colleagues)

**Document purpose:** Decisions, feature scope, and technical recommendations for the IntelliVerseX Web3 SDK in relation to the JavaScript SDK.  
**Audience:** Senior engineers, product, and team.  
**Status:** For discussion and decision.  
**Last updated:** 2026-03-12  

---

## 1. Decisions (Confirmed)

| # | Decision | Detail |
|---|----------|--------|
| 1 | **Web3 SDK extends JS SDK as base** | Web3 SDK will **import** `@intelliversex/sdk` as an npm dependency and extend/reuse its code. No duplication of shared logic (profile, storage, leaderboard, economy, events, etc.). |
| 2 | **Web3 = full feature set** | Web3 SDK must support **everything** the JS SDK supports (email, Google, Apple auth, session restore, etc.) **plus** wallet connect and wallet-based auth. It is a superset, not a subset. |
| 3 | **Packages stay separate** | JS SDK and Web3 SDK remain **two different npm packages** for different platforms/use cases. They are not merged into one. Web3 depends on JS SDK; they are not the same package. |
| 4 | **Real-time socket** | Whether Web3 SDK should expose `connectSocket()` (multiplayer, chat, notifications) is **TBD** — to be discussed and decided separately. |

---

## 2. Current State vs Target State

### 2.1 Today (before refactor)

- **JS SDK** (`@intelliversex/sdk`): Full auth (device, email, Google, Apple, custom), session restore, profile, economy, leaderboard, storage, RPC, socket. Publishment-ready.
- **Web3 SDK** (`@intelliversex/sdk-web3`): Duplicates most of the above, adds wallet connect + wallet auth + NFT/token/gate. Missing: email/Google/Apple auth, session restore, socket.

### 2.2 After refactor (target)

- **JS SDK**: Unchanged as the base package (same API, same behaviour).
- **Web3 SDK**:  
  - **Depends on** `@intelliversex/sdk` (e.g. `IVXManager` or shared types/helpers).  
  - **Adds** Web3-only: `connectWallet()`, `disconnectWallet()`, `authenticateWallet()`, `fetchNfts()`, `fetchTokenBalances()`, `checkTokenGate()`, plus wallet-related events and types.  
  - **Exposes** all JS SDK features (auth methods, session restore, profile, economy, leaderboard, storage, RPC).  
  - **Socket**: Included only if the team decides Web3 games need real-time (see §4).  

---

## 3. Feature Matrix (Web3 SDK After Refactor)

| Feature | JS SDK (base) | Web3 SDK (after refactor) |
|---------|----------------|---------------------------|
| Device auth | ✅ | ✅ (from base) |
| Email auth | ✅ | ✅ (from base) |
| Google auth | ✅ | ✅ (from base) |
| Apple auth | ✅ | ✅ (from base) |
| Custom ID auth | ✅ | ✅ (from base) |
| **Wallet connect** | — | ✅ (Web3-only) |
| **Wallet auth (signature)** | — | ✅ (Web3-only) |
| Session restore (localStorage) | ✅ | ✅ (from base) |
| Profile (fetch/update) | ✅ | ✅ (from base) |
| Economy (fetchWallet, grantCurrency) | ✅ | ✅ (from base) |
| Leaderboard (submit/fetch) | ✅ | ✅ (from base) |
| Storage (read/write) | ✅ | ✅ (from base) |
| Generic RPC | ✅ | ✅ (from base) |
| **NFT fetch** | — | ✅ (Web3-only) |
| **Token balances** | — | ✅ (Web3-only) |
| **Token gating** | — | ✅ (Web3-only) |
| Real-time socket | ✅ | TBD (see §4) |

---

## 4. Recommendation: Real-Time Socket (connectSocket)

- **Current:** Web3 SDK does not expose `connectSocket()`; JS SDK does.
- **Recommendation:** Decide based on product: if Web3 games will use real-time multiplayer, chat, or live notifications, Web3 SDK should expose the same socket API as the base (from JS SDK). If Web3 is REST-only for now, leave socket out until required.
- **Status:** Under discussion — no change until the team decides.

---

## 5. Backend: Web3-Specific RPCs — Recommendation

The Web3 SDK expects these **server-side** RPCs on Nakama:

| RPC ID | Purpose |
|--------|--------|
| `ivx_web3_verify_wallet` | Verify wallet signature after client sends address + message + signature + chainId. |
| `ivx_web3_fetch_nfts` | Return NFTs for a wallet (server can call Moralis/Alchemy/Thirdweb). |
| `ivx_web3_fetch_tokens` | Return ERC-20 balances for a wallet. |
| `ivx_web3_check_gate` | Check token-gated access (e.g. min balance / NFT ownership). |

**Recommendation: implement these on the Nakama server.**

| Reason | Explanation |
|--------|-------------|
| Security | Signature verification must happen server-side; client cannot be trusted. |
| API keys | Moralis/Alchemy/Thirdweb keys must stay on the server, not in the browser. |
| Consistency | Same backend serves both JS and Web3 clients; one Nakama deployment. |
| Flexibility | Server can switch providers (Moralis → Alchemy) without changing the SDK. |

If these RPCs are **not** implemented yet: plan server work (Go/Lua) and document request/response shapes so front-end and back-end stay in sync.  
If they **are** already implemented: ensure Web3 SDK payloads match what the server expects.

---

## 6. Auth Flow: Wallet Verification Order — Suggestion

**Current behaviour in Web3 SDK:**  
`authenticateWallet()` creates a Nakama session via `authenticateCustom(walletAddress)` **first**, then calls `ivx_web3_verify_wallet` to verify the signature. So the user gets a session before the server confirms the signature.

**Suggested improvement (for senior review):**  
Verify the signature **before** creating the session.

| Approach | Pros | Cons |
|----------|------|------|
| **Verify first** | Session is only created for a cryptographically verified wallet. No session if signature is invalid. | Requires a “pre-auth” RPC or custom flow that does not use `authenticateCustom` until after verification. |
| **Verify after (current)** | Simple: one `authenticateCustom` call, then one RPC. | A malicious client could get a session and only then fail verification; server should invalidate or restrict such sessions. |

**Recommendation:** Prefer **verify-first** for security. Implementation options: (1) New RPC that accepts wallet + message + signature, verifies on server, returns a short-lived token that the client then uses with `authenticateCustom`, or (2) Keep current flow but have the server invalidate the session if `ivx_web3_verify_wallet` fails (and document this). Final choice can be made when implementing or reviewing the backend.

---

## 7. Other Technical Notes

### 7.1 submitScore payload

- **JS SDK** passes `{ score: String(score) }` to Nakama.  
- **Web3 SDK** currently passes `{ score }` (number).  
- **Action:** Align both with whatever Nakama’s leaderboard API expects (usually string). After refactor, Web3 will use the base implementation, so this will be consistent.

### 7.2 Shared backend

- The **same** Nakama server can serve both JS SDK and Web3 SDK clients.
- Backend needs: standard Nakama features + `hiro_economy_list` / `hiro_economy_grant` + `ivx_sync_metadata` (already used by both) + the 4 Web3 RPCs above if Web3 features are used.

---

## 8. Suggested Next Steps

1. **Refactor Web3 SDK** to depend on `@intelliversex/sdk`: extend or compose `IVXManager`, reuse types and helpers; remove duplicated code.
2. **Add missing auth and session** in Web3: expose email, Google, Apple, custom auth and session restore from the base (no new backend RPCs required for these).
3. **Backend:** Confirm whether the 4 Web3 RPCs exist; if not, implement and document them (see §5).
4. **Auth flow:** Decide verify-first vs verify-after and update Web3 SDK (and optionally backend) accordingly (see §6).
5. **Socket:** Decide if Web3 SDK should expose `connectSocket()` and add it if yes (see §4).
6. **Versioning:** Bump Web3 SDK minor version after refactor; document breaking changes if any (e.g. if public API of Web3 changes when switching to base).

---

## 9. Document Control

| Version | Date | Changes |
|---------|------|---------|
| 1.0 | 2026-03-12 | Initial: decisions, feature matrix, RPC recommendation, auth flow suggestion, next steps. |

---

*This document can be shared as-is or converted to DOCX for distribution. Questions or edits can be tracked in the repo or via follow-up discussion.*
