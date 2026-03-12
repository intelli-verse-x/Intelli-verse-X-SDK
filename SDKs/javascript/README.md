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
