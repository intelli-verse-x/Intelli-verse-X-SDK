# IntelliVerseX SDK — Dependency Document

**Document title:** Dependency Doc  
**Purpose:** Release-ready dependency inventory for IntelliVerseX SDKs (starting with Web3).  
**Audience:** Senior engineers, release managers, and team members.  
**Last updated:** 2026-03-12  
**SDK version:** 5.1.0  

---

## 1. Executive Summary

This document lists all dependencies required to build and release the **IntelliVerseX Web3 SDK** in a production-ready way. It covers:

- **Runtime & peer dependencies** — what consumers must have to use the SDK.
- **Nakama backend** — full server and RPC requirements for production.
- **Development & tooling** — build, test, and type-check dependencies.
- **Optional integrations** — Thirdweb, Moralis, and configuration.

Use this doc for release checklists, onboarding, and dependency audits.

---

## 2. Web3 SDK — Dependency Overview

| Category | Purpose |
|----------|---------|
| **Peer / runtime** | `@heroiclabs/nakama-js`, `ethers` — required by the SDK at runtime. |
| **Backend** | Nakama server + custom RPCs + optional Hiro/Satori for production. |
| **Dev** | TypeScript, tsup, vitest — build and test only. |
| **Config / secrets** | `config/keys.json` (or env) for Moralis, Nakama host, etc. |

---

## 3. Runtime Dependencies (Peer Dependencies)

These are **not bundled** with the SDK. Consumers must install them.

| Package | Min version | Purpose |
|---------|-------------|---------|
| **@heroiclabs/nakama-js** | ≥ 2.7.0 (tested 2.8.0) | Nakama client: auth, RPC, leaderboards, storage, account. |
| **ethers** | ≥ 6.0.0 (tested 6.16.0) | EVM wallet (EIP-1193), signing, balance, chain ID. |

**Consumer install example:**

```bash
npm install @intelliversex/sdk-web3 @heroiclabs/nakama-js ethers
```

---

## 4. Nakama Backend — Production-Ready Requirements

To run the Web3 SDK in production, the **entire Nakama backend** must be deployed and configured as below.

### 4.1 Nakama Server

| Component | Requirement |
|-----------|-------------|
| **Nakama** | [Heroic Labs Nakama](https://github.com/heroiclabs/nakama) server (open-source). |
| **Deployment** | Host + port (e.g. `nakamaHost`, `nakamaPort`); SSL optional via `useSSL`. |
| **Server key** | `nakamaServerKey` must match server configuration (e.g. `defaultkey` in dev). |

### 4.2 Required Custom RPCs (Server-Side)

The Web3 SDK calls these RPC IDs. They **must** be implemented on the Nakama server for full functionality.

| RPC ID | Purpose |
|--------|---------|
| **ivx_web3_verify_wallet** | Verify wallet signature on auth (address, message, signature, chainId). |
| **ivx_web3_fetch_nfts** | Query NFTs for a wallet (walletAddress, chainId, contractAddress). |
| **ivx_web3_fetch_tokens** | Query ERC-20 balances for a wallet (walletAddress, chainId). |
| **ivx_web3_check_gate** | Verify token-gated access (walletAddress, chainId, contractAddress, minBalance). |
| **ivx_sync_metadata** | Sync SDK metadata (sdk_version, platform, wallet_address, chain_id). |

### 4.3 Optional Backend Features (Hiro / Economy)

| RPC ID | Purpose |
|--------|---------|
| **hiro_economy_list** | List in-game wallet/currency (used by `fetchWallet()`). |
| **hiro_economy_grant** | Grant currency (used by `grantCurrency()`). |

These require Hiro/Satori or equivalent economy backend integration on the server.

### 4.4 Summary: What “Whole Nakama Backend” Means

- **Nakama server** running and reachable.
- **Custom RPCs** listed in § 4.2 implemented (and optionally § 4.3 for economy).
- **Auth** — custom auth (e.g. wallet-based) and/or device ID supported.
- **Storage, leaderboards, account** — standard Nakama APIs used by the SDK.

---

## 5. Development Dependencies (Build & Test)

Used only for building, type-checking, and testing. Not shipped with the package.

| Package | Version (dev) | Purpose |
|---------|----------------|---------|
| **@heroiclabs/nakama-js** | ^2.8.0 | Peer dep resolution + tests. |
| **ethers** | ^6.13.0 | Peer dep resolution + tests. |
| **tsup** | ^8.0.0 | Bundle (CJS + ESM) and `.d.ts` generation. |
| **typescript** | ^5.4.0 | Type checking, `tsconfig.json`. |
| **vitest** | ^2.0.0 | Unit tests. |

**Transitive dev tooling (representative):** esbuild, rollup, vite, @vitest/*, postcss, chokidar, etc. — see `package-lock.json` for the full tree.

---

## 6. Optional Integrations & Configuration

| Item | Type | Purpose |
|------|------|---------|
| **thirdwebClientId** | Config | Optional Thirdweb SDK integration. |
| **moralisApiKey** | Config / secrets | Optional Moralis API for NFT/token data (server or client). |
| **config/keys.json** | Repo config | Common secrets file; copy from `config/keys.example.json`. Do not commit. |
| **Environment** | Env vars | e.g. `IVX_MORALIS_API_KEY` for Moralis. |

Sensitive values must not be hardcoded; use `config/keys.json` or environment variables.

---

## 7. Source-File Dependency Map (Web3 SDK)

From scanning `SDKs/web3`:

| File | Imports / dependencies |
|------|-------------------------|
| **src/index.ts** | Re-exports from `IVXWeb3Manager`, `types`. |
| **src/IVXWeb3Manager.ts** | `@heroiclabs/nakama-js` (Client, Session), `ethers` (BrowserProvider, JsonRpcSigner, formatEther), local `types`. |
| **src/types.ts** | None (pure types and config). |
| **src/__tests__/IVXWeb3Manager.test.ts** | `vitest` (describe, it, expect, beforeEach, vi), `IVXWeb3Manager`, `types`. |
| **examples/web3-example.ts** | `../src` (IVXWeb3Manager). |

**Build:** `tsup src/index.ts --format cjs,esm --dts`  
**Test:** `vitest run`

---

## 8. Environment & Platform Requirements

| Requirement | Detail |
|-------------|--------|
| **Node.js** | 18+ for development and tooling. |
| **Browser** | Modern browser with Web3 wallet (MetaMask, Coinbase Wallet, or other EIP-1193). |
| **Chains** | Any EVM-compatible (Ethereum 1, Polygon 137, Arbitrum 42161, etc.). |

---

## 9. Dependency Checklist for Release

Use this when preparing a Web3 SDK release:

- [ ] **Peer dependencies** — `@heroiclabs/nakama-js` ≥ 2.7.0, `ethers` ≥ 6.0.0 documented and tested.
- [ ] **Nakama backend** — Required RPCs (§ 4.2) documented and (where applicable) implemented in server repo or docs.
- [ ] **Dev dependencies** — All build/test deps in `package.json`; `npm ci` and `npm run build` and `npm test` pass.
- [ ] **Secrets** — No keys in code; `config/keys.example.json` and README describe config.
- [ ] **Changelog** — Dependency or backend changes noted in CHANGELOG.

---

## 10. Other SDKs (Reference)

This document currently details the **Web3 SDK** only. Other IntelliVerseX SDKs (Unity, Unreal, JavaScript, C++, etc.) have their own dependency sets; those can be added to this doc in separate sections or linked from here for a single “Dependency Doc” view for seniors and the team.

---

## Document Control

| Version | Date | Author | Changes |
|---------|------|--------|---------|
| 1.0 | 2026-03-12 | — | Initial Dependency Doc: Web3 SDK + Nakama backend + full dependency list. |

---

*To open in Microsoft Word: open this file in Word and use **Save As → Word Document (.docx)** to create “Dependency Doc.docx” for distribution.*
