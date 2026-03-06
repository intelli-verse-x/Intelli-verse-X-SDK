# JavaScript / TypeScript

> IntelliVerseX npm package for browser and Node.js, with full TypeScript support.

## Requirements

- Node.js 18+ or modern browser
- [@heroiclabs/nakama-js](https://github.com/heroiclabs/nakama-js) v2.7+

## Installation

```bash
npm install @intelliversex/sdk @heroiclabs/nakama-js
```

## Quick Start

```typescript
import { IVXManager } from '@intelliversex/sdk';

const ivx = IVXManager.getInstance();

ivx.initialize({
  nakamaHost: '127.0.0.1',
  nakamaPort: 7350,
  nakamaServerKey: 'defaultkey',
  enableDebugLogs: true,
});

ivx.on('authSuccess', (userId) => {
  console.log('Logged in:', userId);
});

if (!ivx.restoreSession()) {
  await ivx.authenticateDevice();
}

const profile = await ivx.fetchProfile();
const wallet = await ivx.fetchWallet();
const records = await ivx.fetchLeaderboard('weekly', 20);
```

## Browser Usage

```html
<script src="https://unpkg.com/@heroiclabs/nakama-js/dist/nakama-js.umd.js"></script>
<script src="https://unpkg.com/@intelliversex/sdk/dist/index.js"></script>
<script>
  const ivx = IntelliVerseX.IVXManager.getInstance();
  ivx.initialize({ nakamaHost: '127.0.0.1' });
  ivx.authenticateDevice();
</script>
```

## Nakama Client

Built on [nakama-js](https://github.com/heroiclabs/nakama-js) (218 stars, 70 forks).

## Source

[SDKs/javascript/](https://github.com/Intelli-verse-X/Intelli-verse-X-Unity-SDK/tree/main/SDKs/javascript)
