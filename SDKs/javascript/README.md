# IntelliVerseX JavaScript SDK

> Complete modular game development SDK for JavaScript/TypeScript — Auth, Backend (Nakama), Analytics, Social, Monetization, and more.

## Requirements

- Node.js 18+ or modern browser
- [@heroiclabs/nakama-js](https://github.com/heroiclabs/nakama-js) v2.7+

## Installation

```bash
npm install @intelliversex/sdk @heroiclabs/nakama-js
```

## Quick Start

### TypeScript / ES Modules

```typescript
import { IVXManager } from '@intelliversex/sdk';

const ivx = IVXManager.getInstance();

ivx.on('authSuccess', (userId) => {
  console.log('Logged in:', userId);
});

ivx.on('error', (error) => {
  console.error('Error:', error.message);
});

ivx.initialize({
  nakamaHost: '127.0.0.1',
  nakamaPort: 7350,
  nakamaServerKey: 'defaultkey',
  enableDebugLogs: true,
});

// Try restoring a previous session, or authenticate fresh
if (!ivx.restoreSession()) {
  await ivx.authenticateDevice();
}

// Fetch profile and wallet
const profile = await ivx.fetchProfile();
console.log('Profile:', profile);

const wallet = await ivx.fetchWallet();
console.log('Wallet:', wallet);

// Submit a leaderboard score
await ivx.submitScore('weekly_leaderboard', 1500);

// Read leaderboard
const records = await ivx.fetchLeaderboard('weekly_leaderboard');
console.log('Leaderboard:', records);
```

### CommonJS

```javascript
const { IVXManager } = require('@intelliversex/sdk');

const ivx = IVXManager.getInstance();
ivx.initialize({ nakamaHost: '127.0.0.1' });
```

### Browser (Script Tag)

```html
<script src="https://unpkg.com/@heroiclabs/nakama-js/dist/nakama-js.umd.js"></script>
<script src="https://unpkg.com/@intelliversex/sdk/dist/index.js"></script>
<script>
  const ivx = IntelliVerseX.IVXManager.getInstance();
  ivx.initialize({ nakamaHost: '127.0.0.1' });
  ivx.authenticateDevice().then(() => {
    console.log('Ready!', ivx.username);
  });
</script>
```

## Features

| Feature | Status |
|---------|--------|
| Device Auth | Supported |
| Email Auth | Supported |
| Google Auth | Supported |
| Apple Auth | Supported |
| Custom Auth | Supported |
| Profile Management | Supported |
| Wallet / Economy | Supported |
| Leaderboards | Supported |
| Cloud Storage | Supported |
| RPC Calls | Supported |
| Real-time Socket | Supported |
| Hiro Systems | Via RPC |
| TypeScript Types | Full Support |
| Node.js | Supported |
| Browser | Supported |

## API Reference

See the [full documentation](https://intelli-verse-x.github.io/Intelli-verse-X-Unity-SDK/platforms/javascript/).

## Nakama Client Library

This SDK wraps the official [Nakama JS Client](https://github.com/heroiclabs/nakama-js) (218 stars, 70 forks).

## License

MIT License — see [LICENSE](../../LICENSE)
