# Changelog

All notable changes to the IntelliVerseX SDK.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/), and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

---

## [5.8.0] - 2026-04-01

### 🚀 Added
- **AI & LLM Stack** — 8 AI subsystems: NPC Dialog, In-Game Assistant, Chat Moderation, Content Generation, Player Profiling, Voice Services, Voice Personas, AI Host Commentary
- **Discord Social SDK** — Full parity wrapper: Rich Presence, Unified Friends, DM/Voice/Lobbies, Game Invites, Linked Channels, Account Linking, Moderation bridge
- **Satori Analytics** — `IVXSatoriClient` for live events, experiments, feature flags, custom properties
- **Hiro Economy** — 33 systems via `IVXHiroCoordinator`: Achievements, Leaderboards, Inventory, Economy, Energy, Stats, Teams, Tutorials, Event Leaderboards, Progression, Base Building, and more
- **Bootstrap System** — `IVXBootstrap` one-drop prefab: single MonoBehaviour initializes Nakama, Hiro, Satori, Discord in order with graceful offline fallback
- **Multiplayer** — `IVXGameModeManager`, `IVXLobbyManager`, `IVXMatchmakingManager`, `IVXLocalMultiplayerManager`
- **Retention** — Spin Wheel, Daily Rewards/Streak, Offerwall
- **AI Config Enhancements** — `IVXAIProvider` enum (IntelliVerseX / OpenAI / Azure / Anthropic / Custom), `MockMode`, `MaxRetries`, `RateLimitPerSecond`, runtime `SetApiKey()` / `SetApiBaseUrl()`, collection size limits
- **AI Auth Fix** — Bearer token auth (`SetAuthToken`) on all 7 AI managers (was missing on Moderator, ContentGenerator, Profiler, VoiceServices)
- **AI Memory Safety** — `AudioClip.Destroy()` after playback, capped conversation history, event queue, and audio queue
- **Cross-Platform SDKs** — Discord, Satori, and AI stubs added to all 9 non-Unity platforms (JS/TS, Web3, Java, Flutter, Unreal, Godot, Defold, C++, Cocos2d-x)
- **Documentation** — `docs/guides/ai-getting-started.md`, updated MASTER_INTEGRATION_PROMPT, quickstart, installation, and all platform READMEs

### 🐛 Fixed
- IP Geolocation: `PlayerPrefs` → `IVXSecureStorage` migration now falls back to legacy data
- Bearer token not sent on 4 of 7 AI managers (Moderator, ContentGenerator, Profiler, VoiceServices)
- `IVXAIAudioPlayer` leaked native `AudioClip` memory after playback
- `IVXAIAssistant._conversationLines` grew unboundedly; now capped by `MaxConversationHistory`
- `IVXAIProfiler._eventQueue` grew unboundedly; now capped by `MaxEventQueueSize`
- `IVXAIVoiceServices._wsHost` GameObject not destroyed on `OnDestroy()`
- `GameBootstrap.cs` example used stale `IntelliVerseXManager` API; rewritten for `IVXBootstrap`
- Documentation version drift: all docs, configs, and package manifests synced to v5.8.0
- Installation guide had wrong UPM Git path
- Changelog was missing entries for v5.2.0–v5.7.0

### 🔧 Changed
- **Breaking:** Minimum Unity version is now 2023.3+ (2021.3 may work but is unsupported)
- `IVXBootstrapConfig` now warns on default `127.0.0.1` / `defaultkey` values in `OnValidate()`
- All Editor `[MenuItem]` paths unified under `IntelliVerseX/`
- `[HelpURL]` added to all core MonoBehaviours and ScriptableObjects
- `[DisallowMultipleComponent]` on `IVXBootstrap`

---

## [5.1.0] - 2026-03-02

### 🚀 Added
- **IP Geolocation Service** - IVXIPGeolocationService with 6 free API providers, parallel fetch, caching, fallback
- **Flutter / Dart SDK** - Full IVXManager with auth, profile, wallet, leaderboards, storage, RPC
- **Web3 / TypeScript SDK** - IVXWeb3Manager with MetaMask/EIP-1193, wallet signature auth, NFT queries, token gating
- Non-blocking IP geolocation fetch during login

### 🔧 Changed
- Consolidated geolocation services into single IVXIPGeolocationService
- Total platform count: **10 SDKs** (added Flutter and Web3)

---

## [5.0.0] - 2026-02-27

### 🚀 Added
- **Friends Module** - IVXFriendsManager, IVXFriendSlot, IVXFriendsPanel
- Real-time friend status updates via Nakama
- DOTween animations for list transitions
- All test scenes synced to UPM Samples~/TestScenes

### 🐛 Fixed
- DOTween animation stacking in Friends panel
- Tab switching race conditions

---

## [4.0.0] - 2026-02-23

### 🚀 Added
- **IVXPanelForgotPassword** - Complete forgot password UI panel
- **Weekly Quiz System** - IVXWeeklyQuizManager, IVXWeeklyQuizService
- **Ads System** - IVXAdsTestController, IVXAdsBootstrap prefab
- **Test Scenes** - AdsTest, AuthTest, LeaderboardTest, WalletTest, WeeklyQuizTest

### 🐛 Fixed
- Auth canvas panel transitions and state management
- OTP panel auto-focus and resend cooldown edge cases

---

## [4.2.0] - 2024-11-15

### Added
- LevelPlay (IronSource) ad integration
- Appodeal mediation support
- Friend presence notifications
- Offline mode for leaderboards

### Changed
- Updated Nakama client to 3.x
- Improved session management
- Better reconnection handling

### Fixed
- Memory leak in ad preloading
- Race condition in auth flow

---

## [4.1.0] - 2024-09-20

### Added
- Apple Sign-In support
- Google Play Games authentication
- Share functionality with native dialogs

### Changed
- Refactored IAP module for better testability
- Improved localization loading performance

### Fixed
- Android back button handling
- iOS keyboard issues in input fields

---

## [4.0.0] - 2024-07-01

### Added
- Complete UI module with pre-built panels
- Analytics dashboard integration
- Wallet system with virtual currencies
- Achievement system foundation

### Changed
- **Breaking:** Renamed namespaces from `IVX` to `IntelliVerseX`
- **Breaking:** Changed config structure (migration guide below)
- Updated minimum Unity version to 2021.3

### Migration Guide

From v3.x to v4.0:

```csharp
// Old namespace
using IVX.Core;

// New namespace
using IntelliVerseX.Core;

// Old config access
IVXConfig.Instance.ServerUrl;

// New config access
IntelliVerseXSDK.Config.backendUrl;
```

---

## [3.5.0] - 2024-04-15

### Added
- Leaderboard pagination
- Friend search functionality
- Custom avatar support

### Fixed
- WebGL WebSocket reconnection
- Android 13 permission handling

---

## [3.4.0] - 2024-02-01

### Added
- Basic offline support
- Data caching layer
- Connection status indicators

### Changed
- Improved error messages
- Better timeout handling

---

## [3.3.0] - 2023-12-10

### Added
- Social login (Google, Facebook)
- Push notification hooks
- Deep link handlers

### Fixed
- iOS build issues with Xcode 15
- Android target SDK 34 compatibility

---

## [3.2.0] - 2023-10-05

### Added
- Localization system
- Multi-language support (10 languages)
- RTL text support

### Changed
- UI system refactored
- Better TextMeshPro integration

---

## [3.1.0] - 2023-08-20

### Added
- Friends system
- Real-time presence
- Friend requests/invitations

### Fixed
- Memory usage optimizations
- Network retry logic

---

## [3.0.0] - 2023-06-01

### Added
- Nakama backend integration
- Complete authentication system
- Basic leaderboards

### Changed
- **Breaking:** New architecture
- **Breaking:** Complete API redesign

---

## Upgrade Notes

### Upgrading to 5.0.0

1. **Backup your project** before upgrading
2. **Remove old SDK** folder if doing clean install
3. **Update imports** - namespace changes may require updates
4. **Reconfigure settings** - some config options have moved
5. **Test thoroughly** - run all integration tests

### Deprecated Features

The following features will be removed in v6.0:

- `IVXLegacyAuth` - Use `IVXAuthService` instead
- `IVXOldStorage` - Use `IVXSecureStorage` instead
- Callback-based APIs - Prefer async/await patterns

---

## Version Support

| Version | Unity Support | Status |
|---------|---------------|--------|
| 5.8.x | 2023.3+ | ✅ Current |
| 5.1.x | 2023.3+ | ✅ Active |
| 5.0.x | 2023.3+ | ⚠️ Maintenance |
| 4.2.x | 2021.3+ | ⚠️ Security fixes only |
| 4.1.x | 2020.3+ | ❌ End of life |
| 4.0.x | 2020.3+ | ❌ End of life |
| 3.x | 2019.4+ | ❌ End of life |

---

## Links

- [Full Release Notes](https://github.com/intelli-verse-x/Intelli-verse-X-SDK/releases)
- [Migration Guide](guides/migration.md)
- [AI Getting Started](guides/ai-getting-started.md)
