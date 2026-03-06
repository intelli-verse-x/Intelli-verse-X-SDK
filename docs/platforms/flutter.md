# Flutter / Dart

> IntelliVerseX Dart package for Flutter mobile apps, Dart CLI tools, and server-side Dart.

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

  ivx.initialize(const IVXConfig(
    nakamaHost: '127.0.0.1',
    nakamaPort: 7350,
    nakamaServerKey: 'defaultkey',
    enableDebugLogs: true,
  ));

  await ivx.authenticateDevice();

  final profile = await ivx.fetchProfile();
  final wallet = await ivx.fetchWallet();
  final records = await ivx.fetchLeaderboard('weekly', limit: 20);
}
```

## Features

| Feature | Status |
|---------|--------|
| Device Auth | :white_check_mark: |
| Email Auth | :white_check_mark: |
| Google Auth | :white_check_mark: |
| Apple Auth | :white_check_mark: |
| Custom Auth | :white_check_mark: |
| Profile | :white_check_mark: |
| Wallet / Economy | :white_check_mark: |
| Leaderboards | :white_check_mark: |
| Cloud Storage | :white_check_mark: |
| RPC Calls | :white_check_mark: |
| Hiro Systems | Via RPC |
| Dart Types | Full Support |

## Nakama Client

Built on [nakama](https://pub.dev/packages/nakama) (148 stars, 48 forks) — the official Heroic Labs Dart client.

## Source

[SDKs/flutter/](https://github.com/Intelli-verse-X/Intelli-verse-X-Unity-SDK/tree/main/SDKs/flutter)
