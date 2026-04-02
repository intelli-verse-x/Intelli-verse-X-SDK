# IntelliVerseX Web3 SDK

> Web3 game development SDK — Wallet auth (MetaMask/WalletConnect), NFT rewards, token gating, on-chain leaderboards, AI, Multiplayer, Hiro Live-Ops, backed by Nakama + Hiro.

## What's New in v5.8.0

### AI Voice & Host (`IVXAIClient`)

- Voice persona sessions with text & audio
- AI host game commentary
- Entitlement & persona management

```typescript
import { IVXAIClient } from '@intelliversex/sdk-web3';

const ai = new IVXAIClient({
  apiBaseUrl: 'https://ai.intelli-verse-x.ai',
  apiKey: 'your-key',
});

const session = await ai.startVoiceSession('persona-1', userId);
await ai.sendText(session.sessionId, 'Hello!');
const personas = await ai.getPersonas();
```

### Multiplayer & Game Modes (`IVXGameModes`)

- Solo, Local Multiplayer, Online Versus/Co-op, Ranked, Turn-Based
- Room/lobby management
- Quick-match & ranked matchmaking

```typescript
import { IVXGameModes, IVXGameMode } from '@intelliversex/sdk-web3';

const gm = new IVXGameModes();
gm.selectMode(IVXGameMode.ONLINE_VERSUS, 4);
gm.addPlayer('Alice', true);

const room = await gm.createRoom({ maxPlayers: 4 });
await gm.quickMatch(IVXGameMode.RANKED);
```

### Hiro Live-Ops Systems (`IVXHiroSystems`)

- Spin Wheel, Daily Streaks, Offerwall
- Friend Quests & Battles
- IAP Triggers, Smart Ad Timers

```typescript
import { IVXHiroSystems } from '@intelliversex/sdk-web3';

const hiro = new IVXHiroSystems(nakamaClient, session);

const spin = await hiro.spinWheel('daily_wheel');
const streak = await hiro.getStreakState();
await hiro.claimStreak();
const offers = await hiro.getOfferwallState();
```

## What's New in v5.8.0

- Discord Social SDK integration (Rich Presence, friends, lobbies, voice, invites, DMs, moderation)
- Satori Analytics (events, feature flags, A/B experiments, live events)
- Hiro parity: retention, IAP triggers, smart ad timer (Unreal/C++/Cocos/Godot/Defold)

### Discord Social SDK (`IVXDiscordSocial`)

- Rich Presence, friends list, lobbies, voice chat
- Game invites, DMs, moderation tools

```typescript
import { IVXDiscordSocial } from '@intelliversex/sdk-web3';

const discord = IVXDiscordSocial.getInstance();
discord.initialize({ applicationId: 'YOUR_APP_ID', clientId: 'YOUR_CLIENT_ID' });

await discord.updatePresence({ state: 'In Match', details: 'Round 3 of 5' });
const friends = await discord.getFriends();
```

### Satori Analytics (`IVXSatori`)

- Event capture, feature flags, A/B experiments, live events

```typescript
import { IVXSatori } from '@intelliversex/sdk-web3';

const satori = IVXSatori.getInstance();
satori.initialize({ satoriUrl: 'https://satori.example.com', apiKey: 'your-satori-key' });

await satori.captureEvents([{ name: 'level_complete', value: '5' }]);
const flags = await satori.getFeatureFlags();
```

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

## Setting Up Nakama Server

The SDK requires a [Nakama](https://heroiclabs.com/nakama/) game server for backend features.

**Quick start with Docker:**

```bash
docker run -d --name nakama -p 7349:7349 -p 7350:7350 -p 7351:7351 heroiclabs/nakama
```

**Heroic Labs Cloud:** For production, use [Heroic Labs Cloud](https://heroiclabs.com/) for managed hosting.

See [Nakama documentation](https://heroiclabs.com/docs/nakama/) for full setup instructions.

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
| Wallet Connection (MetaMask / EIP-1193) | ✅ Supported |
| Wallet Signature Auth | ✅ Supported |
| Device Auth (fallback) | ✅ Supported |
| NFT Ownership Queries | ✅ Supported |
| ERC-20 Token Balances | ✅ Supported |
| Token Gating | ✅ Supported |
| Profile Management | ✅ Supported |
| Wallet / Economy (Hiro) | ✅ Supported |
| Leaderboards | ✅ Supported |
| Cloud Storage | ✅ Supported |
| RPC Calls | ✅ Supported |
| AI Voice & Host | ✅ New in v5.8.0 |
| Multiplayer & Game Modes | ✅ New in v5.8.0 |
| Hiro Live-Ops Systems | ✅ New in v5.8.0 |
| Analytics | ✅ Supported |
| Discord Social SDK | ✅ New in v5.8.0 |
| Satori Analytics | ✅ New in v5.8.0 |
| TypeScript Types | ✅ Full Support |
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

## Project Structure

```
src/
├── index.ts              # Core IVXWeb3Manager
├── types.ts              # Shared types
├── IVXAIClient.ts        # AI voice & host client
├── IVXGameModes.ts       # Multiplayer & game mode management
└── IVXHiroSystems.ts     # Hiro live-ops typed wrappers
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

## Troubleshooting

| Issue | Solution |
|-------|----------|
| Connection timeout | Verify Nakama server is running and accessible at the configured host:port |
| Auth failed | Check server key matches your Nakama configuration |
| AI features not working | Verify AI API endpoint and key are set in config |
| Wallet connection failed | Ensure MetaMask or compatible Web3 wallet is installed and unlocked |
| Discord not connecting | Ensure `applicationId` and `clientId` are valid and Discord app is approved |
| Satori events not captured | Check `satoriUrl` and `apiKey` are correctly configured |

## License

MIT License — see [LICENSE](../../LICENSE)
