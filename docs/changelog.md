# Changelog

All notable changes to the IntelliVerseX SDK.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/), and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

---

## [5.0.0] - 2025-01-13

### 🚀 Added
- **Modular Architecture** - SDK now uses Unity Assembly Definitions for better separation
- **Daily Quiz System** - New quiz module with backend integration
- **Weekly Quiz Tournament** - Competitive weekly quizzes with leaderboards
- **WebGL Monetization** - Platform-specific ad support for WebGL
- **Secure Storage** - AES-256 encrypted local storage
- **Cloud Sync** - Automatic data synchronization with Nakama
- **More of Us** - Cross-game promotion module
- **Rate App Manager** - Native rating prompts for iOS/Android
- **Deep Linking** - App link handling for social features

### 🔧 Changed
- **Unified Backend** - Consolidated all backend calls through `IVXNakamaManager`
- **Simplified Auth** - Streamlined authentication flow with `IVXAuthService`
- **Improved Logging** - Category-based logging with `IVXLogger`
- **Better Error Handling** - Descriptive error codes and messages
- **Performance Optimizations** - Reduced GC allocations in hot paths

### 🗑️ Removed
- **Retention Module** - Removed retention/check-in features (moved to backend-only)
- **Legacy Friends Config** - Simplified friends configuration
- **Deprecated APIs** - Removed deprecated methods from v4.x

### 🐛 Fixed
- Fixed null reference in `UserSessionManager` on cold start
- Fixed WebGL build errors with `IVXAdsManager`
- Fixed iOS compilation warnings for Apple Auth
- Fixed Android 14 notification permissions

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
| 5.0.x | 2021.3+ | ✅ Current |
| 4.2.x | 2021.3+ | ⚠️ Security fixes only |
| 4.1.x | 2020.3+ | ❌ End of life |
| 4.0.x | 2020.3+ | ❌ End of life |
| 3.x | 2019.4+ | ❌ End of life |

---

## Links

- [Full Release Notes](https://github.com/intelli-verse-x/Intelli-verse-X-Unity-SDK/releases)
- [Migration Guide](guides/migration.md)
- [API Reference](api/index.md)
