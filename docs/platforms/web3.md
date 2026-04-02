# Web3 (Thirdweb / Moralis)

> IntelliVerseX Web3 SDK — Wallet auth (MetaMask / WalletConnect), NFT rewards, token gating, and on-chain leaderboards backed by Nakama + Hiro.

## Requirements

- Node.js 18+ or modern browser with MetaMask / EIP-1193 wallet
- [@heroiclabs/nakama-js](https://github.com/heroiclabs/nakama-js) v2.7+
- [ethers](https://docs.ethers.org/) v6+

## Installation

```bash
npm install @intelliversex/sdk-web3 @heroiclabs/nakama-js ethers
```

## Quick Start

```typescript
import { IVXWeb3Manager } from '@intelliversex/sdk-web3';

const ivx = IVXWeb3Manager.getInstance();

ivx.on('walletConnected', (info) => console.log('Wallet:', info.address));
ivx.on('authSuccess', (userId) => console.log('Authenticated:', userId));

ivx.initialize({
  nakamaHost: '127.0.0.1',
  nakamaPort: 7350,
  chainId: 137,         // Polygon
  enableDebugLogs: true,
});

// Connect MetaMask
const wallet = await ivx.connectWallet();

// Authenticate via wallet signature
await ivx.authenticateWallet();

// Query NFTs owned by this wallet
const nfts = await ivx.fetchNfts('0xContractAddress');

// Token-gated content check
const hasAccess = await ivx.checkTokenGate('0xContractAddress', '1');

// Standard game features
const profile = await ivx.fetchProfile();
await ivx.submitScore('weekly_leaderboard', 2500);
```

## API Surface

`IVXWeb3Manager` is a TypeScript singleton that follows the same **Nakama-backed IVX manager pattern** as Unity’s IntelliVerseX managers: one coordinator for auth, economy hooks, leaderboards, storage, and RPC. It does not literally `extend` a shared `IVXManager` class in TypeScript (there is no shared base class in npm); behavior and naming are aligned with the Unity IVX surface.

| Area | Symbol | Description |
|------|--------|-------------|
| **Type** | `IVXWeb3Manager` | `getInstance()`, `resetInstance()` (tests) |
| **Lifecycle** | `initialize(config)` | Builds `nakama-js` `Client`, validates `IVXWeb3Config` |
| **State** | `client`, `session`, `isInitialized`, `hasValidSession`, `userId`, `username` | Mirrors standard IVX session model |
| **Wallet info** | `walletInfo`, `walletAddress`, `isWalletConnected` | Populated after `connectWallet()` |

### Wallet methods

| Method / member | Notes |
|-----------------|--------|
| `connectWallet()` | EIP-1193 (`window.ethereum`), `BrowserProvider`, requests accounts, emits `walletConnected` |
| `disconnectWallet()` | Clears provider/signer/local wallet state only (no chain revoke) |
| `getWalletAddress` | Use **`walletAddress`** getter (or `walletInfo?.address`) |
| `signMessage` | **Not a public API.** Signing runs **inside** `authenticateWallet()` (`signer.signMessage` with the IntelliVerseX auth payload). For custom messages, use **ethers** with the same EIP-1193 provider after `connectWallet()` |

### NFT methods

| Method | Notes |
|--------|--------|
| `fetchNfts(contractAddress?)` | RPC `ivx_web3_fetch_nfts` — server may use Moralis / Alchemy / Thirdweb |
| `checkTokenGate(contractAddress, minBalance?)` | RPC `ivx_web3_check_gate` — server-side balance / ownership check |

### Token methods

| Method | Notes |
|--------|--------|
| `fetchTokenBalances()` | RPC `ivx_web3_fetch_tokens` — ERC-20 style balances for connected wallet |
| `transferToken` | **Not built-in.** Use `callRpc()` with your game’s Nakama RPC (e.g. intent to transfer, relayed tx), or Hiro economy RPCs such as `grantCurrency` / `hiro_economy_*` — never move user funds from the SDK without explicit wallet transactions |

### Standard IVX-style methods (Nakama session)

| Method | Notes |
|--------|--------|
| `authenticateWallet()` | Custom auth path + `ivx_web3_verify_wallet` RPC |
| `authenticateDevice(deviceId?)` | Device fallback when Web3 is optional |
| `clearSession()` | Clears Nakama session locally |
| `fetchProfile`, `updateProfile` | Account API |
| `fetchWallet`, `grantCurrency` | Hiro economy via RPC |
| `submitScore`, `fetchLeaderboard` | Leaderboards |
| `writeStorage`, `readStorage` | User storage |
| `callRpc(rpcId, payload)` | Generic Nakama RPC |

### Events (`on` / `off`)

`initialized`, `walletConnected`, `walletDisconnected`, `authSuccess`, `authError`, `profileLoaded`, `walletUpdated`, `leaderboardFetched`, `nftsFetched`, `tokenBalanceFetched`, `storageRead`, `rpcResponse`, `error` — see `IVXWeb3EventMap` in the package typings.

### Configuration (`IVXWeb3Config`)

| Field | Purpose |
|-------|---------|
| `gameId` | IntelliVerseX title id from the developer dashboard (warns if empty) |
| `nakamaHost` / `nakamaPort` / `nakamaServerKey` / `useSSL` | Nakama REST endpoint |
| `chainId` | Default EVM chain for metadata and server hints (e.g. `137` Polygon) |
| `thirdwebClientId` | Optional Thirdweb dashboard client id for server/indexer integrations |
| `moralisApiKey` | Optional Moralis key — **use only on the server**; the field exists for BFF/RPC wiring |
| `enableAnalytics` / `enableDebugLogs` / `verboseLogging` | Telemetry and console verbosity |

Defaults favor the hosted IntelliVerseX Nakama endpoint; override for local development as in **Quick Start**.

## Feature Coverage

Legend: **Y** = included in this Web3 npm SDK, **-** = not in scope for the Web3 JS client, **S** = stub / use another IntelliVerseX SDK or host platform (e.g. Unity) for full support.

| Capability | Web3 |
|------------|------|
| SDK init | Y |
| Device auth | Y |
| Custom auth (wallet signature) | Y |
| Profile | Y |
| Wallet / economy (Hiro RPC) | Y |
| Leaderboards | Y |
| Storage | Y |
| Nakama RPC | Y |
| Email auth | - |
| Google / Apple auth | - |
| Session restore (stored refresh flows) | - |
| Real-time socket | - |
| AI init | Y |
| AI voice / LLM | S |
| Game modes | S |
| Lobby / matchmaking | S |
| Hiro (live ops economy) | Y |
| Discord | S |
| Satori | S |
| Web3 / NFT gating | Y |
| WebGL / browser target | Y |

## Web3-Specific Features

| Feature | Status |
|---------|--------|
| Wallet Connection (MetaMask / EIP-1193) | :white_check_mark: |
| Wallet Signature Auth | :white_check_mark: |
| NFT Ownership Queries | :white_check_mark: |
| ERC-20 Token Balances | :white_check_mark: |
| Token Gating | :white_check_mark: |
| Thirdweb Integration | Config Ready |
| Moralis Integration | Config Ready |

## Standard Features

All standard IntelliVerseX features are included: Device Auth, Profile, Wallet/Economy, Leaderboards, Storage, RPC.

## Architecture

```
Your Game / dApp
    |
    v
+----------------------------------------------+
|     IntelliVerseX Web3 SDK (IVXWeb3Manager)  |
|  Wallet | Auth | NFT | Tokens | Gate | RPC  |
+----------------------------------------------+
    |                    |
    v                    v
+------------------+  +------------------+
| Nakama Client    |  | ethers.js        |
| (nakama-js)      |  | (EIP-1193)       |
+------------------+  +------------------+
    |                    |
    v                    v
+------------------+  +------------------+
| Nakama Server    |  | EVM Blockchain   |
| + Hiro + Satori  |  | (via RPC nodes)  |
+------------------+  +------------------+
```

## Smart Contract Integration

### How token gating works

1. The browser wallet connects with `connectWallet()`; the SDK knows `walletAddress` and `chainId`.
2. `checkTokenGate(contract, minBalance)` sends a **Nakama RPC** (`ivx_web3_check_gate`) with that address, chain, contract, and threshold.
3. **Your server implementation** (Lua/Go in Nakama) should call an indexer or RPC node (Thirdweb, Moralis, Alchemy, etc.), verify balance or NFT ownership, and return `{ granted: true | false }`.
4. The client never trusts itself for gate results; it only displays what the server authorizes.

### NFT reward flow

1. `fetchNfts()` triggers `ivx_web3_fetch_nfts`, again **server-side**, so API keys for Moralis/Alchemy stay off the client.
2. To **grant** an NFT or off-chain reward, use a dedicated RPC that mints, queues airdrop, or credits Hiro currency after verifying eligibility (quest complete, tournament rank, etc.).
3. Keep mint/transfer authority in custodial or contract-admin flows; the SDK surfaces **reads** and **auth**, not private keys.

### On-chain leaderboard verification

- **Default path:** `submitScore` / `fetchLeaderboard` use Nakama leaderboards (authoritative game scores).
- **On-chain verification:** Implement an RPC that (a) accepts a score or proof payload, (b) verifies signatures or zk/state roots on the server, then (c) writes the Nakama leaderboard record only if valid. The Web3 SDK’s `callRpc` is the extension point for that custom pipeline.

## Security Model

### Wallet signature auth flow

1. After `connectWallet()`, `authenticateWallet()` builds a deterministic message (wallet address + nonce).
2. The user signs with their wallet (`eth_sign` / personal_sign via ethers).
3. The client calls `authenticateCustom` with the wallet id and then **`ivx_web3_verify_wallet`** with `{ address, message, signature, chainId }`.
4. The **server must** recover the signer from the signature, compare to `address`, enforce nonce/replay rules, and only then treat the session as bound to that wallet.

### Server-side verification

- All gating, NFT lists, and token balances intended for **security decisions** must be validated on Nakama (or your BFF), not by trusting client-side `ethers` reads alone (users can spoof devtools).
- Rate-limit verification RPCs and tie them to the authenticated Nakama user.

### Never store private keys

- The SDK never asks for seed phrases or private keys. Only public addresses and signatures in transit.
- Do not log full signatures in production analytics.

## Supported Chains

Any EVM-compatible chain: Ethereum (1), Polygon (137), Arbitrum (42161), Optimism (10), BSC (56), Avalanche (43114), Base (8453), and more.

## Advanced Examples

### NFT-gated tournament entry

```typescript
await ivx.connectWallet();
await ivx.authenticateWallet();

const PASS_CONTRACT = '0xYourTournamentPass';
const canEnter = await ivx.checkTokenGate(PASS_CONTRACT, '1');
if (!canEnter) {
  throw new Error('Tournament requires NFT pass');
}
const ok = await ivx.callRpc('tournament_register', JSON.stringify({ bracketId: 'spring-2026' }));
```

### Token reward on achievement

```typescript
// After server-side achievement validation (recommended pattern):
const result = await ivx.callRpc('grant_achievement_token', JSON.stringify({
  achievementId: 'first_win',
  walletAddress: ivx.walletAddress,
}));
// Server credits Hiro currency, queues ERC-20 transfer, or records claimable reward
await ivx.fetchWallet();
```

### Multi-chain setup

- Instantiate **one** `IVXWeb3Manager` per app shell; `chainId` in `initialize()` is the **default** expectation for your backend.
- If users may switch chains in MetaMask, re-read `walletInfo.chainId` after `connectWallet()` and pass `chainId` into every RPC payload so the server queries the correct network.
- For multiple environments (Polygon + Base), use separate Nakama RPC implementations keyed by `chainId`, or separate `gameId`/namespace per chain.

## Troubleshooting

| Symptom | What to check |
|---------|----------------|
| **MetaMask not detected** | `window.ethereum` undefined — extension disabled, or non-browser context. WalletConnect support is configuration-level; ensure you are not running inside a locked iframe without wallet injection. |
| **Wrong chain ID** | User switched network after connect. Compare `ivx.walletInfo.chainId` to your `initialize({ chainId })` default and prompt `wallet_switchEthereumChain` via your own code if needed. |
| **Transaction failed** | On-chain sends are **not** handled by `IVXWeb3Manager` directly. Inspect gas, nonce, and contract reverts in the wallet; for game economy prefer server-mediated RPCs. |
| **CORS with RPC nodes** | Browser cannot call some JSON-RPC URLs directly due to CORS. Proxy reads through **your Nakama RPC** or a same-origin backend. |
| **ethers version mismatch** | Package expects **ethers v6** (`BrowserProvider`, `JsonRpcSigner`). If another dependency pulls v5, dedupe or align versions in `package.json` / `overrides`. |

## Further Reading

- [SDKs/web3/README.md](https://github.com/Intelli-verse-X/Intelli-verse-X-Unity-SDK/tree/main/SDKs/web3) — npm package readme and API notes
- [SDK Web3 strategy doc](https://github.com/Intelli-verse-X/Intelli-verse-X-Unity-SDK/blob/main/SDKs/Web3-SDK-Strategy-and-Features.md) — product and integration context
- [Nakama JavaScript client](https://heroiclabs.com/docs/nakama/client-libraries/javascript/) — sessions, RPC, storage
- [Nakama custom authentication](https://heroiclabs.com/docs/nakama/concepts/user-accounts/#custom-authentication) — how `authenticateCustom` maps to your verifier
- [ethers v6 docs](https://docs.ethers.org/v6/) — `BrowserProvider`, signing
- [EIP-1193](https://eips.ethereum.org/EIPS/eip-1193) — Ethereum provider API
- [EIP-4361 Sign-In with Ethereum](https://eips.ethereum.org/EIPS/eip-4361) — optional upgrade path for structured auth messages
- [IntelliVerseX Hiro integration guide](../guides/hiro-integration.md) — economy RPCs used by `fetchWallet` / `grantCurrency`
- [Nakama integration guide](../guides/nakama-integration.md) — RPC authoring and deployment expectations
- [Feature coverage matrix](../FEATURE_COVERAGE_MATRIX.md) — cross-platform comparison
- [Unity WebGL platform doc](./webgl.md) — when pairing browser builds with the same backend

## Source

[SDKs/web3/](https://github.com/Intelli-verse-X/Intelli-verse-X-Unity-SDK/tree/main/SDKs/web3)
