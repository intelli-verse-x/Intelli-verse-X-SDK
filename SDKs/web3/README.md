# IntelliVerseX Web3 SDK

> Web3 game development SDK — Wallet auth (MetaMask/WalletConnect), NFT rewards, token gating, on-chain leaderboards, backed by Nakama + Hiro.

## Configuration and secrets

Sensitive or environment-specific values (e.g. `moralisApiKey`, Nakama host/port) should not be hardcoded. Use the repo **common config file**: copy `config/keys.example.json` to `config/keys.json` in the repo root, fill in values, and do not commit `config/keys.json`. See [config/README.md](../../config/README.md). When initializing the SDK, set `moralisApiKey` from that file or from the environment (e.g. `process.env.IVX_MORALIS_API_KEY`).

## Requirements

- Node.js 18+ or modern browser with a Web3 wallet (MetaMask, Coinbase Wallet, etc.)
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

ivx.on('walletConnected', (info) => {
  console.log('Wallet:', info.address, 'Balance:', info.balance, 'ETH');
});
ivx.on('authSuccess', (userId) => console.log('Authenticated:', userId));
ivx.on('error', (err) => console.error('Error:', err.message));

ivx.initialize({
  nakamaHost: '127.0.0.1',
  nakamaPort: 7350,
  nakamaServerKey: 'defaultkey',
  chainId: 137,        // Polygon
  enableDebugLogs: true,
});

// Connect MetaMask / browser wallet
const wallet = await ivx.connectWallet();

// Authenticate with Nakama using wallet signature
await ivx.authenticateWallet();

// Fetch player profile
const profile = await ivx.fetchProfile();

// Query NFTs owned by this wallet
const nfts = await ivx.fetchNfts('0xYourContractAddress');

// Check token-gated access
const hasAccess = await ivx.checkTokenGate('0xYourContractAddress', '1');

// Standard game features work too
await ivx.submitScore('weekly_leaderboard', 2500);
const records = await ivx.fetchLeaderboard('weekly_leaderboard');
```

## Features

| Feature | Status |
|---------|--------|
| Wallet Connection (MetaMask / EIP-1193) | Supported |
| Wallet Signature Auth | Supported |
| Device Auth (fallback) | Supported |
| NFT Ownership Queries | Supported |
| ERC-20 Token Balances | Supported |
| Token Gating | Supported |
| Profile Management | Supported |
| Wallet / Economy (Hiro) | Supported |
| Leaderboards | Supported |
| Cloud Storage | Supported |
| RPC Calls | Supported |
| TypeScript Types | Full Support |
| Thirdweb Integration | Config Ready |
| Moralis Integration | Config Ready |

## Web3-Specific API

### IVXWeb3Manager

| Method | Description |
|--------|-------------|
| `connectWallet()` | Connect browser wallet (MetaMask etc.) |
| `disconnectWallet()` | Disconnect wallet |
| `authenticateWallet()` | Auth with Nakama via wallet signature |
| `fetchNfts([contract])` | Query NFTs via server RPC |
| `fetchTokenBalances()` | Query ERC-20 balances via server RPC |
| `checkTokenGate(contract, min)` | Check token-gated access |

Plus all standard features: `authenticateDevice()`, `fetchProfile()`, `updateProfile()`, `fetchWallet()`, `submitScore()`, `fetchLeaderboard()`, `writeStorage()`, `readStorage()`, `callRpc()`.

### Events

```typescript
ivx.on('walletConnected', (info: IVXWalletInfo) => { ... });
ivx.on('walletDisconnected', () => { ... });
ivx.on('authSuccess', (userId) => { ... });
ivx.on('authError', (error) => { ... });
ivx.on('nftsFetched', (nfts: IVXNft[]) => { ... });
ivx.on('tokenBalanceFetched', (tokens: IVXTokenBalance[]) => { ... });
ivx.on('profileLoaded', (profile) => { ... });
ivx.on('walletUpdated', (wallet) => { ... });
ivx.on('leaderboardFetched', (records) => { ... });
ivx.on('error', (error) => { ... });
```

### Configuration

```typescript
ivx.initialize({
  nakamaHost: '127.0.0.1',
  nakamaPort: 7350,
  nakamaServerKey: 'defaultkey',
  useSSL: false,
  enableDebugLogs: true,
  chainId: 137,                    // Polygon mainnet
  thirdwebClientId: 'your-id',    // Optional Thirdweb integration
  moralisApiKey: 'your-key',      // Optional Moralis integration
});
```

## Server-Side RPCs

The Web3 SDK expects these server RPC endpoints:

| RPC ID | Purpose |
|--------|---------|
| `ivx_web3_verify_wallet` | Verify wallet signature on auth |
| `ivx_web3_fetch_nfts` | Query NFTs for a wallet address |
| `ivx_web3_fetch_tokens` | Query ERC-20 balances for a wallet |
| `ivx_web3_check_gate` | Verify token-gated access |
| `ivx_sync_metadata` | Sync SDK metadata (shared) |

## Supported Chains

Any EVM-compatible chain works. Common chain IDs:

| Chain | ID |
|-------|----|
| Ethereum Mainnet | 1 |
| Polygon | 137 |
| Arbitrum | 42161 |
| Optimism | 10 |
| BSC | 56 |
| Avalanche | 43114 |
| Base | 8453 |

## Running Tests

```bash
npm test
```

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

## License

MIT License — see [LICENSE](../../LICENSE)
