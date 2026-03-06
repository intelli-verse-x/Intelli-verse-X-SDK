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

## Supported Chains

Any EVM-compatible chain: Ethereum (1), Polygon (137), Arbitrum (42161), Optimism (10), BSC (56), Avalanche (43114), Base (8453), and more.

## Source

[SDKs/web3/](https://github.com/Intelli-verse-X/Intelli-verse-X-Unity-SDK/tree/main/SDKs/web3)
