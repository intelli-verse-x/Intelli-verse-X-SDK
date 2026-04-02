# IntelliVerseX Flutter / Dart SDK

> Complete modular game development SDK for Flutter/Dart — Auth, Backend (Nakama), Analytics, Economy, Leaderboards, Storage, RPC, AI, Multiplayer, Hiro Live-Ops, and more.

## What's New in v5.8.0

### AI Voice & Host (`IVXAIClient`)

- Voice persona sessions with text & audio
- AI host game commentary
- Entitlement & persona management

```dart
import 'package:intelliversex_sdk/intelliversex_sdk.dart';

final ai = IVXAIClient(apiBaseUrl: 'https://ai.intelli-verse-x.ai', apiKey: 'your-key');
final session = await ai.startVoiceSession(personaId: 'persona-1', userId: userId);
await ai.sendText(sessionId: session.sessionId, text: 'Hello!');
final personas = await ai.getPersonas();
```

### Multiplayer & Game Modes (`IVXGameModeManager`)

- Solo, Local Multiplayer, Online Versus/Co-op, Ranked, Turn-Based
- Room/lobby management
- Quick-match & ranked matchmaking

```dart
final gm = IVXGameModeManager();
gm.selectMode(IVXGameMode.onlineMultiplayer, maxPlayers: 4);
gm.addPlayer('Alice', isLocal: true);
gm.setPlayerReady(0, true);
if (gm.canStartMatch) gm.startMatch();
```

### Hiro Live-Ops Systems (`IVXHiroSystems`)

- Spin Wheel, Daily Streaks, Offerwall
- Friend Quests & Battles
- IAP Triggers, Smart Ad Timers

```dart
final hiro = IVXHiroSystems(nakamaClient: client, session: session);
final spin = await hiro.spinWheel('daily_wheel');
final streak = await hiro.getStreakState();
await hiro.claimStreak();
final offers = await hiro.getOfferwallState();
```

## What's New in v5.8.0

- Discord Social SDK integration (Rich Presence, friends, lobbies, voice, invites, DMs, moderation)
- Satori Analytics (events, feature flags, A/B experiments, live events)
- Hiro parity: retention, IAP triggers, smart ad timer (Unreal/C++/Cocos/Godot/Defold)

### Discord Social SDK (`IVXDiscordSocial`)

- Rich Presence, friends list, lobbies, voice chat
- Game invites, DMs, moderation tools

```dart
final discord = IVXDiscordSocial.instance;
discord.initialize(const IVXDiscordConfig(
  applicationId: 'YOUR_APP_ID',
  clientId: 'YOUR_CLIENT_ID',
));

await discord.updatePresence(state: 'In Match', details: 'Round 3 of 5');
final friends = await discord.getFriends();
```

### Satori Analytics (`IVXSatori`)

- Event capture, feature flags, A/B experiments, live events

```dart
final satori = IVXSatori.instance;
satori.initialize(const IVXSatoriConfig(
  satoriUrl: 'https://satori.example.com',
  apiKey: 'your-satori-key',
));

await satori.captureEvents([IVXEvent(name: 'level_complete', value: '5')]);
final flags = await satori.getFeatureFlags();
```

## Requirements

- Dart SDK 3.0+
- [nakama](https://pub.dev/packages/nakama) v1.3+

### Platform Support

- **Flutter**: iOS, Android, Web, macOS, Windows, Linux (via Flutter)

## Installation

Add to your `pubspec.yaml`:

```yaml
dependencies:
  intelliversex_sdk:
    git:
      url: https://github.com/Intelli-verse-X/Intelli-verse-X-Unity-SDK.git
      path: SDKs/flutter
  nakama: ^1.3.0
```

Or, once published to pub.dev:

```yaml
dependencies:
  intelliversex_sdk: ^5.8.0
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

```dart
import 'package:intelliversex_sdk/intelliversex_sdk.dart';

Future<void> main() async {
  final ivx = IVXManager.instance;

  ivx.on(IVXEvent.authSuccess, (userId) => print('Logged in: $userId'));
  ivx.on(IVXEvent.error, (err) => print('Error: $err'));

  ivx.initialize(const IVXConfig(
    nakamaHost: '127.0.0.1',
    nakamaPort: 7350,
    nakamaServerKey: 'defaultkey',
    enableDebugLogs: true,
  ));

  await ivx.authenticateDevice();

  final profile = await ivx.fetchProfile();
  print('Profile: $profile');

  final wallet = await ivx.fetchWallet();
  print('Wallet: $wallet');

  await ivx.submitScore('weekly_leaderboard', 1500);

  final records = await ivx.fetchLeaderboard('weekly_leaderboard');
  print('Leaderboard: $records');
}
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
| AI Voice & Host | 🔶 Stub |
| Multiplayer & Game Modes | 🔶 Stub |
| Hiro Live-Ops Systems | 🔶 Stub |
| Analytics | ✅ Supported |
| Discord Social SDK | 🔶 Stub |
| Satori Analytics | 🔶 Stub |
| Dart Types | ✅ Full Support |
| Flutter (iOS/Android) | ✅ Supported |
| Dart CLI / Server | ✅ Supported |

> 🔶 **Stub** = Full API surface exists. Methods log warnings and return empty/mock data. Zero code changes needed when backend support ships.

## API Overview

### IVXManager

| Method | Description |
|--------|-------------|
| `initialize(IVXConfig)` | Set up the Nakama client |
| `authenticateDevice([deviceId])` | Auth with device ID |
| `authenticateEmail(email, password)` | Auth with email/password |
| `authenticateGoogle(token)` | Auth with Google token |
| `authenticateApple(token)` | Auth with Apple token |
| `authenticateCustom(customId)` | Auth with custom ID |
| `clearSession()` | Clear current session |
| `fetchProfile()` | Get user profile |
| `updateProfile(...)` | Update display name, avatar, etc. |
| `fetchWallet()` | Get economy data via Hiro |
| `grantCurrency(id, amount)` | Grant currency |
| `submitScore(id, score)` | Submit leaderboard score |
| `fetchLeaderboard(id)` | Get leaderboard records |
| `writeStorage(collection, key, value)` | Write cloud save |
| `readStorage(collection, key)` | Read cloud save |
| `callRpc(rpcId, [payload])` | Call any server RPC |

### Events

```dart
ivx.on(IVXEvent.initialized, (_) { ... });
ivx.on(IVXEvent.authSuccess, (userId) { ... });
ivx.on(IVXEvent.authError, (error) { ... });
ivx.on(IVXEvent.profileLoaded, (profile) { ... });
ivx.on(IVXEvent.walletUpdated, (wallet) { ... });
ivx.on(IVXEvent.leaderboardFetched, (records) { ... });
ivx.on(IVXEvent.storageRead, (data) { ... });
ivx.on(IVXEvent.rpcResponse, (result) { ... });
ivx.on(IVXEvent.error, (error) { ... });
```

## Project Structure

```
lib/
├── intelliversex_sdk.dart       # Package barrel export
└── src/
    ├── types.dart               # Shared types
    ├── ivx_ai_client.dart       # AI voice & host client
    ├── ivx_game_modes.dart      # Multiplayer & game mode management
    └── ivx_hiro_systems.dart    # Hiro live-ops typed wrappers
```

## Running Tests

```bash
dart test
```

## Nakama Client Library

Built on [nakama](https://pub.dev/packages/nakama) (148 stars, 48 forks) — the official Heroic Labs Dart client for Nakama.

## Troubleshooting

| Issue | Solution |
|-------|----------|
| Connection timeout | Verify Nakama server is running and accessible at the configured host:port |
| Auth failed | Check server key matches your Nakama configuration |
| AI features not working | Verify AI API endpoint and key are set in config |
| Discord not connecting | Ensure `applicationId` and `clientId` are valid and Discord app is approved |
| Satori events not captured | Check `satoriUrl` and `apiKey` are correctly configured |

## License

MIT License — see [LICENSE](../../LICENSE)
