# JavaScript / TypeScript

> IntelliVerseX npm package (`@intelliversex/sdk`) for browsers, Electron, and Node.js 18+, with full TypeScript declarations. Core flows use **async/await** on top of [@heroiclabs/nakama-js](https://github.com/heroiclabs/nakama-js). SDK version **5.8.0** aligns with the Unity package and [Feature Coverage Matrix](../FEATURE_COVERAGE_MATRIX.md).

## Requirements

- **Node.js** 18+ or a modern evergreen browser
- **Peer dependency:** `@heroiclabs/nakama-js` **≥ 2.7.0** (repo tests use 2.8.x)
- A running **Nakama** instance (local Docker or IntelliVerseX-hosted) with matching server key and RPCs enabled for Hiro / multiplayer if you use those modules
- Optional **gameId** (UUID) from the IntelliVerseX developer console for metadata sync and dashboards

## Installation

```bash
npm install @intelliversex/sdk @heroiclabs/nakama-js
```

Lock `nakama-js` to a single major/minor in `package.json` to avoid subtle API drift between `Client`, `Session`, and `Socket` types.

## Quick Start (TypeScript)

```typescript
import { IVXManager } from '@intelliversex/sdk';

const ivx = IVXManager.getInstance();

ivx.initialize({
  nakamaHost: '127.0.0.1',
  nakamaPort: 7350,
  nakamaServerKey: 'defaultkey',
  useSSL: false,
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

## Browser Usage (UMD)

Load Nakama first, then the SDK bundle. The global namespace exposes `IntelliVerseX` (see your build’s actual global name in `dist` if customized).

```html
<script src="https://unpkg.com/@heroiclabs/nakama-js/dist/nakama-js.umd.js"></script>
<script src="https://unpkg.com/@intelliversex/sdk/dist/index.js"></script>
<script>
  const ivx = IntelliVerseX.IVXManager.getInstance();
  ivx.initialize({ nakamaHost: '127.0.0.1', nakamaPort: 7350, nakamaServerKey: 'defaultkey' });
  ivx.authenticateDevice();
</script>
```

## API Surface

High-level entry points exported from `@intelliversex/sdk` (see `SDKs/javascript/src/index.ts`).

| Symbol | Role |
|--------|------|
| **IVXManager** | Singleton: `initialize`, auth (device / email / Google / Apple / custom), session restore/clear, profile, wallet (Hiro economy RPCs), leaderboards, storage, generic `callRpc`, `connectSocket`, typed events (`on` / `off`). |
| **IVXConfig** | Host, port, `nakamaServerKey`, `useSSL`, `enableDebugLogs`, optional `gameId`. Validated via `validateConfig`; `DEFAULT_CONFIG` re-exported for merges. |
| **IVXMultiplayer** | Lobby + matchmaking over Nakama RPCs: create/join/leave/list lobbies, start/cancel matchmaking. Requires wiring to the same Nakama `Client` + `Session` as core auth. |
| **IVXHiroSystems** | Live-ops RPC façade: `spinWheel`, `streaks`, `offerwall`, `retention`, `friendQuests`, `friendBattles`, `iapTrigger`, `smartAdTimer`. Call `initialize(client, session)` before use. |
| **IVXDiscordSocial** | Discord-shaped social API (Unity parity). On JS/TS the implementation is **stub** (`S` in the coverage matrix): compile-time types and events, mock/local behavior until native/bridge wiring ships. |
| **IVXSatori** | Satori-style analytics API surface (**stub** on JS): flags, experiments, events—use for forward-compatible integration. |
| **IVXAIClient** | AI **voice / host** session singleton (personas, messages, entitlements). |
| **IVXAISessionManager** | *Not a separate export in this package.* Use **`IVXAIClient.getInstance()`** for the same responsibility (voice/host session lifecycle). Unity or docs may use the “session manager” name; npm ships `IVXAIClient` only. |
| **AI sub-managers** | **IVXAINPCDialogManager**, **IVXAIAssistant**, **IVXAIVoiceServices**, **IVXAIModerator**, **IVXAIContentGenerator**, **IVXAIProfiler** — typed APIs; many are **stubs** (`S`) returning mock/local data until backend keys and routes are configured. |
| **IVXWebXRHelper** | Browser WebXR capability/session helpers for WebGL experiences (see `IVXWebXRHelper.ts`, `WebXRSessionType`, `WebXRCapabilities`). |
| **IVXGameModes** | High-level game-mode / player-slot model (**stub** `S` for match flow on JS). |
| **IVXDeepLinks** | Deep link parsing and dispatch for web and hybrid shells. |

Also exported: Discord DM/moderation/settings/linked-channels/debug modules, **SDK_VERSION**, and shared types (`IVXProfile`, `IVXLeaderboardRecord`, `IVXError`, `IVXEventMap`, etc.).

## Feature Coverage (JS/TS column)

Abbreviated from [Feature Coverage Matrix](../FEATURE_COVERAGE_MATRIX.md). **Y** = implemented, **S** = stub/mock API.

| Area | Coverage |
|------|:--------:|
| Core init / config | Y |
| Auth (device, email, Google, Apple, custom) | Y |
| Profile fetch/update | Y |
| Wallet fetch/grant (Hiro RPC) | Y |
| Leaderboards submit/fetch | Y |
| Storage read/write | Y |
| Generic RPC | Y |
| Real-time socket | Y |
| Events (`IVXEventMap`) | Y |
| AI initialize (`IVXAIClient`) | Y |
| AI voice / LLM stack (NPC, assistant, moderator, content gen, profiler, TTS/STT classes) | S |
| Game modes (`IVXGameModes`) | S |
| Lobby / matchmaking (`IVXMultiplayer`) | Y |
| Hiro live-ops (spin wheel, streaks, offerwall, retention, friends, IAP trigger, smart ad) | Y |
| Discord social / Satori | S |
| Deep links | Y |
| WebGL / Web | Y |
| Bootstrap-style init (`initialize` + optional module `initialize`) | Y |

## TypeScript Types — Key Exports

| Category | Types / enums |
|----------|----------------|
| Config | `IVXConfig` |
| Core data | `IVXProfile`, `IVXLeaderboardRecord`, `IVXError`, `IVXEventMap` |
| AI client | `IVXAIConfig`, `IVXAIPersona`, `IVXAIMessage`, `IVXAISessionResponse`, `IVXAIEntitlement`, `IVXAIHostProfile`, `IVXAIEventMap` |
| Hiro | `IVXSpinWheelResult`, `IVXSpinWheelState`, `IVXStreakState`, `IVXStreakMilestone`, `IVXOffer`, `IVXOfferWallState`, `IVXRetentionState`, `IVXFriendQuest`, `IVXFriendBattle`, `IVXIapTriggerResult`, `IVXSmartAdResult` |
| Multiplayer | `IVXLobby`, `IVXLobbyPlayer`, `IVXMatchmakingTicket`, `IVXMultiplayerLobbyAPI`, `IVXMultiplayerMatchmakingAPI` |
| Game modes | `IVXPlayerSlot`, `IVXMatchConfig`, `IVXRoomConfig`, `IVXRoomInfo`, `IVXRoomFilter`, `IVXMatchResult`, `IVXGameModesEventMap` |
| Discord | `IVXDiscordConfig`, `IVXUnifiedFriend`, `IVXGameInvite`, `IVXDiscordLobbyInfo`, `IVXVoiceParticipant`, `IVXDiscordSocialEventMap`, `IVXDirectMessage`, `IVXDMSummary`, `IVXModerationDecision`, `IVXDiscordSettingsState`, `IVXLinkedChannel`, `IVXDiscordLogEntry`, `IVXDiscordLogCallback` |
| Satori | `IVXSatoriConfig`, `IVXSatoriEvent`, `IVXSatoriFlag`, `IVXSatoriExperiment`, `IVXSatoriLiveEvent` |
| Deep links | `DeepLinkConfig`, `DeepLinkResult`, `DeepLinkHandler` |
| WebXR | `WebXRCapabilities`, `WebXRSessionType` |
| Content gen | `IVXQuestTemplate`, `IVXGeneratedQuest`, `IVXGeneratedStory`, `IVXGeneratedItem`, `IVXGeneratedDialogue`, … |

Import what you need from the package root; tree-aware bundlers will shake unused modules.

## Advanced Examples

### Hiro streaks (async)

After auth, bind Hiro to the same Nakama client/session as `IVXManager`:

```typescript
import { IVXManager, IVXHiroSystems } from '@intelliversex/sdk';

const ivx = IVXManager.getInstance();
const hiro = IVXHiroSystems.getInstance();

await ivx.authenticateDevice();
const client = ivx.client!;
const session = ivx.session!;
hiro.initialize(client, session);

const streaks = await hiro.streaks.get();
console.log(streaks);

await hiro.streaks.update('daily_login');
await hiro.streaks.claimMilestone('daily_login', 7);
```

RPC IDs used internally include `hiro_streaks_get`, `hiro_streaks_update`, and `hiro_streaks_claim` — your Nakama project must expose matching RPCs (or Hiro module equivalents).

### AI content generation (stub API)

`IVXAIContentGenerator` follows the same shape as Unity’s content pipeline; configure `IVXAIConfig` (base URL, key, provider) when moving from stub to production.

```typescript
import { IVXAIContentGenerator, type IVXQuestTemplate } from '@intelliversex/sdk';

const gen = IVXAIContentGenerator.getInstance();
gen.initialize({
  apiBaseUrl: 'https://your-ai-gateway.example',
  apiKey: process.env.IVX_AI_KEY!,
  provider: 'intelliversex',
  mockMode: false,
});

const template: IVXQuestTemplate = {
  genre: 'sci-fi',
  difficulty: 'medium',
  requiredElements: ['puzzle', 'dialogue'],
  estimatedDurationMinutes: 15,
};

const quest = await gen.generateQuest(template);
console.log(quest.title, quest.objectives);
```

While `mockMode: true` (or before backend wiring), responses are local placeholders — safe for UI prototyping.

### Real-time socket (optional)

```typescript
const socket = await ivx.connectSocket();
socket.onchannelmessage = (msg) => { /* ... */ };
```

Use Nakama’s channel/match APIs through the returned `Socket` instance from `nakama-js`.

## Node.js, SSR, and tooling

- **Node.js**: `IVXManager` works in Node for automation and headless tests. `localStorage` is absent; device IDs and sessions are not persisted unless you polyfill storage or subclass patterns. Use `authenticateDevice(explicitId)` in CI.
- **Vite / Webpack / esbuild**: consume the **ESM** entry (`import` from `@intelliversex/sdk`) for tree-shaking. CJS (`require`) is supported via `dist/index.js`.
- **TypeScript**: `strict` mode compatible; enable `skipLibCheck` only if a peer’s `.d.ts` conflicts — prefer fixing version alignment first.
- **Testing**: `IVXManager.resetInstance()` clears the singleton between Vitest/Jest cases. Mock `@heroiclabs/nakama-js` `Client` if you need deterministic RPC without a server.
- **Game ID**: Set `gameId` in `IVXConfig` when using IntelliVerseX-hosted Nakama so `ivx_sync_metadata` and platform dashboards correlate sessions. Empty `gameId` logs a warning in some engines; JS mirrors that behavior where implemented.

## Multiplayer and Hiro prerequisites

`IVXMultiplayer` and `IVXHiroSystems` assume your Nakama project registers the same **RPC ids** as the Unity reference (e.g. `create_lobby`, `hiro_streaks_get`). If an RPC returns 404 or empty payloads, verify Go/Lua server handlers and Hiro module deployment — the SDK does not embed server logic.

## Troubleshooting

### CORS errors in the browser

Nakama’s **HTTP API** and **WebSocket** endpoints must allow your web origin. Configure Nakama (`socket.server_key`, `console` settings, and reverse proxy) to emit `Access-Control-Allow-*` for your dev server (e.g. `http://localhost:5173`) and production domain. Preflight failures often show as blocked `POST` to `/v2/account/authenticate/*` or blocked socket handshake.

### WebSocket issues

- Confirm `useSSL` matches your deployment (`wss` vs `ws`).
- Corporate proxies and some mobile WebViews strip or delay WebSockets — test with a minimal `connectSocket()` call and Nakama server logs.
- Session must be valid (not expired); refresh tokens are handled by `nakama-js` when configured.

### Bundle size

The SDK re-exports many modules. Prefer **named imports** and ensure your bundler supports ESM tree-shaking (`import` from `@intelliversex/sdk` dist ESM). For landing pages, consider lazy `import()` of Hiro or AI modules after first screen.

### `nakama-js` version mismatch

Peer dependency is `>=2.7.0`. If TypeScript complains about `Client`/`Session`/`Socket` types, align `@heroiclabs/nakama-js` with the version used to build `@intelliversex/sdk` (see devDependencies in `SDKs/javascript/package.json`) and run a clean install.

### Session / `localStorage`

`restoreSession` and device ID persistence use `localStorage` when available. In **SSR** or **private mode** without storage, provide your own persistence or call `authenticateDevice` each time.

### Hiro `initialize` order

Calling `hiro.streaks.get()` before `IVXHiroSystems.initialize(client, session)` throws. Always authenticate, then pass the same `Client` and `Session` instances from `IVXManager` into `initialize`.

### Vitest / JSDOM

If `crypto.randomUUID` is missing in the test environment, polyfill it or pass a fixed `deviceId` to `authenticateDevice` to avoid flaky ID generation.

## Nakama Client

Built on [nakama-js](https://github.com/heroiclabs/nakama-js). Wallet and several Hiro paths call **RPCs** (`hiro_economy_list`, `hiro_economy_grant`, etc.) rather than raw REST, matching other IntelliVerseX platforms.

## Source

- [SDKs/javascript/](https://github.com/Intelli-verse-X/Intelli-verse-X-SDK/tree/main/SDKs/javascript)
- [Feature Coverage Matrix](../FEATURE_COVERAGE_MATRIX.md)
- [ivx-cross-platform skill](../../.cursor/skills/ivx-cross-platform/SKILL.md) (repo) — parity and stub semantics

## Further Reading

- Package scripts: `npm run build` (tsup + `tsc` declarations), `npm test` (Vitest) — see `SDKs/javascript/package.json`.
- [Heroic Labs — Nakama JavaScript client](https://heroiclabs.com/docs/nakama/client-libraries/javascript/)
- [IntelliVerseX SDK setup (Unity-oriented concepts apply to backend IDs)](../../.cursor/skills/ivx-sdk-setup/SKILL.md)
- [Live ops / Hiro skill](../../.cursor/skills/ivx-live-ops/SKILL.md)
- [AI integration skill](../../.cursor/skills/ivx-ai-integration/SKILL.md)
- [Multiplayer skill](../../.cursor/skills/ivx-multiplayer/SKILL.md) — lobby/RPC contracts shared across engines
- Example script: `SDKs/javascript/examples/node-example.ts` in the repo
