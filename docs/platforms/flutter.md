# Flutter / Dart

> IntelliVerseX **Dart package** for Flutter mobile apps, Dart CLIs, and server-side Dart workers. It layers typed helpers on top of the official **[nakama](https://pub.dev/packages/nakama)** client for auth, economy, storage, RPC, optional realtime, AI, Discord social surfaces, Satori, Hiro live-ops, multiplayer lobbies, and deep links.

## Requirements

- **Dart SDK** `>=3.0.0 <4.0.0` (see `SDKs/flutter/pubspec.yaml`)
- **Flutter** stable channel recommended for mobile targets (iOS/Android)
- **nakama** Dart package `^1.3.0` (declared as a direct dependency of this package)

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
  nakama: ^1.3.0
```

Run:

```bash
dart pub get
# or
flutter pub get
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
  final wallet = profile.wallet; // embedded in IVXProfile
  final records = await ivx.fetchLeaderboard('weekly', limit: 20);
}
```

Hook **multiplayer** after session exists:

```dart
IVXMultiplayer.instance.initialize(ivx.client!, ivx.session!);
final lobbies = await IVXMultiplayer.instance.listLobbies();
```

## API Surface

The library entry point is **`package:intelliversex_sdk/intelliversex_sdk.dart`**, which re-exports the files below.

| Type | Role |
|------|------|
| `IVXManager` | Singleton: `initialize`, typed `on`/`off` for **`IVXEvent`**, auth, profile, wallet, leaderboards, storage, RPC, access to underlying **`Client`** / **`Session`** |
| `IVXConfig` | Immutable config: host, port, server key, SSL, debug, optional `gameId` (`validate()` on init) |
| `IVXEvent` | Enum: `initialized`, `authSuccess`, `authError`, `profileLoaded`, `walletUpdated`, `leaderboardFetched`, `storageRead`, `rpcResponse`, `error` |
| `IVXMultiplayer` | Lobby create/join/list, matchmaking ticket start/cancel — RPC-backed (`initialize(client, session)` required) |
| `IVXGameModeManager` | Local/game-mode state: slots, rooms, match flow (companion to multiplayer) |
| `IVXHiroSystems` | Facade over Hiro subsystems: spin wheel, streaks, offerwall, retention, friend quests/battles, IAP trigger, smart ad timer |
| `IVXDiscordSocial` | Discord config, friends, invites, voice/chat models |
| `IVXDiscordSettings`, `IVXDiscordMessages`, `IVXDiscordModeration`, `IVXDiscordLinkedChannels`, `IVXDiscordDebug` | Discord feature slices |
| `IVXSatori`, `IVXSatoriConfig`, … | Satori-style analytics types and client |
| `IVXAIClient`, `IVXAIAssistant`, `IVXAIContentGenerator`, `IVXAIModerator`, `IVXAINPCDialogManager`, `IVXAIProfiler`, `IVXAIVoiceServices` | AI session, moderation, NPC dialog, voice |
| `IVXDeepLinks` | Campaign / deferred deep link helpers |
| `IVXProfile`, `IVXLeaderboardRecord`, `IVXError` | Shared DTOs (`types.dart`) |

**Naming note:** Hiro live-ops are exposed as **`IVXHiroSystems`**, not `IVXHiroClient`.

## Feature coverage

Legend: **Y** = supported in this package layer, **S** = stub, partial, or server-dependent, **—** = not provided here (build yourself).

| Area | Flutter |
|------|---------|
| Device / email / Google / Apple / custom auth | Y |
| Session restore from disk (built-in) | — (use `shared_preferences` + `Session` restore pattern yourself) |
| Profile / wallet / leaderboards / storage / RPC | Y |
| Real-time socket (full IVX wrapper) | S (use `nakama` socket APIs via `IVXManager.client`) |
| AI: init / voice / host-style client | Y |
| AI: hosted LLM orchestration | S |
| Lobby / matchmaking (`IVXMultiplayer`) | Y |
| Hiro systems (`IVXHiroSystems`) | Y |
| Discord stack | S |
| Satori (`IVXSatori`) | S |
| Deep links (`IVXDeepLinks`) | Y |

## State management

The SDK is **agnostic** to UI frameworks. Use whichever pattern you already standardize on.

### Provider (`provider` package)

```dart
class IvxHolder extends ChangeNotifier {
  final IVXManager ivx = IVXManager.instance;
  // call notifyListeners() from ivx.on handlers if you mirror state
}
```

Register a single `ChangeNotifierProvider` above `MaterialApp` and inject `context.read<IvxHolder>()` in screens.

### Riverpod

```dart
final ivxManagerProvider = Provider<IVXManager>((ref) => IVXManager.instance);
```

For async auth:

```dart
final sessionProvider = FutureProvider<void>((ref) async {
  final ivx = ref.read(ivxManagerProvider);
  ivx.initialize(const IVXConfig(/* ... */));
  await ivx.authenticateDevice();
});
```

Prefer **`ref.onDispose`** to `off()` event handlers if you register per-widget callbacks.

### Bloc / Cubit

- Emit states from **`IVXEvent`** handlers: `authSuccess` → `Authenticated`, `authError` → `AuthFailed`.
- Keep **`IVXManager.instance`** outside Bloc constructors if you unit-test with **`IVXManager.resetInstance()`** between tests.

### General rules

- **Never** call `initialize` from every widget `build`; do it once at app start (e.g. root `StatefulWidget` or `AppInitialization` future).
- Subscribe to events in **`initState`** / provider `build` scope and cancel in **`dispose`** / `off()` to avoid leaks.
- Long-running **`await ivx.authenticateDevice()`** should run before `runApp` or behind a splash screen FutureBuilder.

## Advanced examples

### Hiro “season pass” style UI (spin + streak + offerwall)

`IVXHiroSystems` is a **singleton**; call **`initialize(client, session)`** once after auth. Subsystems are nested fields (`spinWheel`, `streaks`, `offerwall`, …):

```dart
final ivx = IVXManager.instance;
final hiro = IVXHiroSystems.instance;
hiro.initialize(ivx.client!, ivx.session!);

final spin = await hiro.spinWheel.spin();
// bind spin.rewardType, spin.amount to a widget

final streak = await hiro.streaks.getState();
// show streak.currentStreak in your season-pass sheet

final offers = await hiro.offerwall.list();
// ListView.builder for IVXOffer tiles
```

Wrap calls in **`try/catch`** and map **`IVXError`** to `SnackBar` / `AlertDialog`. RPC ids are fixed in `ivx_hiro_systems.dart` (`hiro_spin_wheel_spin`, etc.) and must exist on your Nakama server.

### AI assistant chat UI

`IVXAIClient` is also a **singleton** (`IVXAIClient.instance`). Voice-first flows use **`startVoiceSession`**; text uses **`sendText`** + **`pollMessages`** (or your own push channel):

```dart
final ai = IVXAIClient.instance;
await ai.initialize(
  apiBaseUrl: 'https://api.example.com',
  apiKey: secret,
);

final session = await ai.startVoiceSession('quiz_host', ivx.userId);
// For text-style UX on the same session:
await ai.sendText(session.sessionId, userInput);
final msgs = await ai.pollMessages(session.sessionId);
```

Drive a **`ListView`** from the latest **`IVXAIMessage`** list; throttle **`pollMessages`** (e.g. `Timer.periodic`) to avoid HTTP pile-ups.

## Troubleshooting

### `dart pub get` / `flutter pub get` fails

- **Git path dependency:** ensure the `url` + `path: SDKs/flutter` match your fork layout; shallow clones may omit submodules if you use any.
- **Version solving failed:** run `dart pub outdated` and align **`nakama`** with the range required by `intelliversex_sdk`.
- **Corporate proxy:** configure `HTTPS_PROXY` and pub’s hosted cache mirrors per Dart documentation.

### iOS build / CocoaPods

- Run **`cd ios && pod install`** after adding or upgrading Flutter plugins.
- If **`nakama`** or transitive pods require a higher iOS deployment target, set `platform :ios, '13.0'` (or newer) in `ios/Podfile` to match plugin minima.
- Clean build folder: `flutter clean && flutter pub get`.

### Android `minSdkVersion`

- Nakama and HTTP stacks may require **minSdk 21+**; set `minSdk` in `android/app/build.gradle` consistently with **`compileSdk`**.
- For desugaring (older APIs), enable core library desugaring only if another dependency demands it.

### Dart version conflicts

- Monorepos using **Melos** or path dependencies must use a single **SDK constraint** across packages; the IVX package requires Dart 3.
- CI images should pin **`dart --version`** to the same major.minor as local devs.

### Session not surviving restarts

- The Flutter **`IVXManager`** does not automatically persist refresh tokens in all builds; persist **`session.refreshToken`** / **`session.token`** securely (e.g. `flutter_secure_storage`) and restore via nakama’s session APIs on cold start.

## Features (summary)

| Feature | Status |
|---------|--------|
| Device Auth | Supported |
| Email Auth | Supported |
| Google Auth | Supported |
| Apple Auth | Supported |
| Custom Auth | Supported |
| Profile | Supported |
| Wallet / Economy | Supported |
| Leaderboards | Supported |
| Cloud Storage | Supported |
| RPC Calls | Supported |
| Hiro Systems | Via `IVXHiroSystems` + RPC |
| Multiplayer lobby | Via `IVXMultiplayer` + RPC |
| Dart Types | Full support in package |

## Nakama client

Built on **[nakama](https://pub.dev/packages/nakama)** — the official Heroic Labs Dart client. Use **`IVXManager.client`** and **`IVXManager.session`** for features not wrapped by IVX (matchmaker, realtime channel, groups).

## Source

- **[SDKs/flutter/](https://github.com/Intelli-verse-X/Intelli-verse-X-Unity-SDK/tree/main/SDKs/flutter)** — `lib/intelliversex_sdk.dart`, `lib/src/*.dart`, `pubspec.yaml`, tests under `test/` when present.

## Further reading

- [Nakama Dart client docs](https://heroiclabs.com/docs/nakama/client-libraries/dart/) — authentication, sockets, and API surface.
- [Flutter architectural overview](https://docs.flutter.dev/resources/architectural-overview) — how to place initialization relative to widgets.
- IntelliVerseX skills: **`.cursor/skills/ivx-sdk-setup/SKILL.md`**, **`.cursor/skills/ivx-quiz-content/SKILL.md`**, **`.cursor/skills/ivx-ai-integration/SKILL.md`** — content pipelines and AI configuration that complement mobile clients.
