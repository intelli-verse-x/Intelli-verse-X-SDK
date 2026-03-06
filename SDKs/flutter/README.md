# IntelliVerseX Flutter / Dart SDK

> Complete modular game development SDK for Flutter/Dart — Auth, Backend (Nakama), Analytics, Economy, Leaderboards, Storage, RPC, and more.

## Requirements

- Dart SDK 3.0+
- [nakama](https://pub.dev/packages/nakama) v1.3+

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
  intelliversex_sdk: ^5.1.0
```

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
| Hiro Systems | Via RPC |
| Dart Types | Full Support |
| Flutter (iOS/Android) | Supported |
| Dart CLI / Server | Supported |

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

## Running Tests

```bash
dart test
```

## Nakama Client Library

Built on [nakama](https://pub.dev/packages/nakama) (148 stars, 48 forks) — the official Heroic Labs Dart client for Nakama.

## License

MIT License — see [LICENSE](../../LICENSE)
