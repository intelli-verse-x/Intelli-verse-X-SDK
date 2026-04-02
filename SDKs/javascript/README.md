# IntelliVerseX JavaScript SDK

> Complete modular game development SDK for JavaScript/TypeScript — Auth, Backend (Nakama), Analytics, Social, Monetization, AI, Multiplayer, Hiro Live-Ops, and more.

## What's New in v5.5.0

### AI Voice & Host (`IVXAIClient`)

- Voice persona sessions with text & audio
- AI host game commentary
- Entitlement & persona management

```typescript
import { IVXAIClient } from '@intelliversex/sdk';

const ai = new IVXAIClient({
  apiBaseUrl: 'https://ai.intelli-verse-x.ai',
  apiKey: 'your-key',
});

const session = await ai.startVoiceSession('persona-1', userId);
await ai.sendText(session.sessionId, 'Hello!');
const personas = await ai.getPersonas();
const entitlement = await ai.checkEntitlement(userId);
```

### Multiplayer & Game Modes (`IVXGameModes`)

- Solo, Local Multiplayer, Online Versus/Co-op, Ranked, Turn-Based
- Room/lobby management
- Quick-match & ranked matchmaking

```typescript
import { IVXGameModes, IVXGameMode } from '@intelliversex/sdk';

const gm = new IVXGameModes();
gm.selectMode(IVXGameMode.ONLINE_VERSUS, 4);
gm.addPlayer('Alice', true);
gm.setPlayerReady(0, true);

const room = await gm.createRoom({ maxPlayers: 4 });
const rooms = await gm.listRooms();
await gm.quickMatch(IVXGameMode.RANKED);
```

### Hiro Live-Ops Systems (`IVXHiroSystems`)

- Spin Wheel, Daily Streaks, Offerwall
- Friend Quests & Battles
- IAP Triggers, Smart Ad Timers

```typescript
import { IVXHiroSystems } from '@intelliversex/sdk';

const hiro = new IVXHiroSystems(nakamaClient, session);

const spin = await hiro.spinWheel('daily_wheel');
const streak = await hiro.getStreakState();
await hiro.claimStreak();
const offers = await hiro.getOfferwallState();
await hiro.startFriendBattle(friendId, 'quiz_duel');
```

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
  nakamaHost: 'nakama-rest.intelli-verse-x.ai',
  nakamaPort: 443,
  nakamaServerKey: 'defaultkey',
  useSSL: true,
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
ivx.initialize({ nakamaHost: 'nakama-rest.intelli-verse-x.ai' });
```

### Browser (Script Tag)

```html
<script src="https://unpkg.com/@heroiclabs/nakama-js/dist/nakama-js.umd.js"></script>
<script src="https://unpkg.com/@intelliversex/sdk/dist/index.js"></script>
<script>
  const ivx = IntelliVerseX.IVXManager.getInstance();
  ivx.initialize({ nakamaHost: 'nakama-rest.intelli-verse-x.ai' });
  ivx.authenticateDevice().then(() => {
    console.log('Ready!', ivx.username);
  });
</script>
```

## Features

| Feature | Status |
|---------|--------|
| Device Auth | ✅ Supported |
| Email Auth | ✅ Supported |
| Google Auth | ✅ Supported |
| Apple Auth | ✅ Supported |
| Custom Auth | ✅ Supported |
| Profile Management | ✅ Supported |
| Wallet / Economy | ✅ Supported |
| Leaderboards | ✅ Supported |
| Cloud Storage | ✅ Supported |
| RPC Calls | ✅ Supported |
| Real-time Socket | ✅ Supported |
| AI Voice & Host | ✅ New in v5.5.0 |
| Multiplayer & Game Modes | ✅ New in v5.5.0 |
| Hiro Live-Ops Systems | ✅ New in v5.5.0 |
| Analytics | ✅ Supported |
| TypeScript Types | ✅ Full Support |
| Node.js | ✅ Supported |
| Browser | ✅ Supported |

## Project Structure

```
src/
├── index.ts              # Core IVXManager
├── types.ts              # Shared types
├── IVXAIClient.ts        # AI voice & host client
├── IVXGameModes.ts       # Multiplayer & game mode management
└── IVXHiroSystems.ts     # Hiro live-ops typed wrappers
```

## API Reference

See the [full documentation](https://intelli-verse-x.github.io/Intelli-verse-X-Unity-SDK/platforms/javascript/).

## Nakama Client Library

This SDK wraps the official [Nakama JS Client](https://github.com/heroiclabs/nakama-js) (218 stars, 70 forks).

## Optimize bundle size

**When:** Do this **before** you log in and publish. It’s part of preparing the package, not part of the npm/CodeArtifact login flow.

**What it does:**

- Shrinks the JavaScript files (and the published tarball) by removing whitespace, shortening names, and stripping dead code where possible.
- Result: faster installs, less bandwidth, and sometimes slightly faster load in the browser.

**How it’s done in this project:**

| Command | Purpose |
|--------|--------|
| `npm run build` | Normal build (readable output, good for debugging). |
| `npm run build:prod` | Production build with **minification** (smaller files for publishing). |

For publishing, use the optimized build so the package you upload is as small as possible:

```bash
npm run build:prod
npm publish
```

Or use the optimized build in your publish script. The `prepublishOnly` script currently runs `npm run build`; you can switch it to `npm run build:prod` if you always want minified output when publishing.

**Already in place:**

- **`@heroiclabs/nakama-js`** is a **peerDependency**, so it is not bundled into your SDK. Users install it separately. That keeps your bundle small and avoids shipping Nakama twice.

---

## Publishing to npm

### Option A — Publish to AWS CodeArtifact (private registry)

If your team uses **AWS CodeArtifact** as the npm registry:

**Step 1 — Log in using AWS**

You need AWS CLI installed and credentials with access to CodeArtifact. Run:

```bash
aws codeartifact login \
  --tool npm \
  --repository intelli-verse-npm-store \
  --domain intelli-verse-x \
  --region us-east-1
```

- **What it does:** Authenticates npm with your CodeArtifact repository. It updates your **`.npmrc`** (in your user folder or project) so that `npm install` and `npm publish` use the CodeArtifact URL and token instead of the public npm registry.
- **`--repository`** = the CodeArtifact repo name (`intelli-verse-npm-store`).
- **`--domain`** = the CodeArtifact domain (`intelli-verse-x`).
- **`--region`** = AWS region where the domain lives (`us-east-1`).

**Step 2 — Verify registry**

```bash
npm config get registry
```

- **What it does:** Prints which registry npm will use. It should show your CodeArtifact URL (e.g. `https://intelli-verse-x-123456789012.d.codeartifact.us-east-1.amazonaws.com/npm/intelli-verse-npm-store/`). If it still shows `https://registry.npmjs.org/`, login didn’t apply; run Step 1 again.

**Step 3 — Publish the SDK**

From this directory (`SDKs/javascript`):

```bash
npm run build
npm publish
```

- **What it does:** Builds the package, then publishes it to the CodeArtifact repository. Anyone with access to that repo can install it with `npm install @intelliversex/sdk` (or whatever the package name is in `package.json`).

**Package name:** The project’s `package.json` currently has `"name": "@intelliversex/sdk"`. If your internal standard is **`@intelliverse/javascript-sdk`**, change the `name` in `package.json` to that so it publishes under the correct name in your private registry.

---

### Option B — Publish to public npm (npmjs.com)

### 1. Create an npm account (if you don’t have one)

- Go to [https://www.npmjs.com/signup](https://www.npmjs.com/signup) and create an account.

### 2. Log in from the terminal

From this directory (`SDKs/javascript`):

```bash
npm login
```

Enter your npm username, password, and email when prompted. If you use 2FA, enter the one-time code when asked.

### 3. Use the right scope for the package name

The package name is **`@intelliversex/sdk`**. The scope is `intelliversex`.

- If your **npm username** is `intelliversex`, you can keep the name and publish.
- If your username is different (e.g. `mycompany`), either:
  - Create an npm **organization** named `intelliversex` at [https://www.npmjs.com/org/create](https://www.npmjs.com/org/create) and publish under that org, or  
  - Change the name in `package.json` to your scope, e.g. `@mycompany/sdk`, then publish.

### 4. Validate, then publish

```bash
npm install
npm run validate-publish
```

- `validate-publish` builds and runs `npm publish --dry-run` so you can see what would be published.

When everything looks good, publish as a **public** package (so anyone can install it for free):

```bash
npm run publish:public
```

Or run the steps yourself:

```bash
npm run build
npm publish --access public
```

- `--access public` is required for scoped packages (`@scope/name`) so the package is public, not private.

### 5. After publishing

- Your package will be at: `https://www.npmjs.com/package/@intelliversex/sdk`
- Users install with: `npm install @intelliversex/sdk @heroiclabs/nakama-js`

## License

MIT License — see [LICENSE](../../LICENSE)
