# Changelog

All notable changes to IntelliVerseX SDK will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

---

## [5.1.0] - 2026-03-02

### Added

#### IP Geolocation Service
- **IVXIPGeolocationService** - Ultra-optimized IP-based geolocation with 6 free API providers
  - Parallel fetching for fastest response (typically <500ms)
  - Intelligent tiered fallback: ip-api.com, ipapi.co, GeoJS, geoPlugin, ipinfo.io, Country.is
  - Configurable caching (default: 1 hour TTL)
  - Thread-safe singleton pattern
  - Events: `OnLocationFetched`, `OnLocationError`, `OnFetchStarted`, `OnFetchCompleted`

#### Login Integration
- Non-blocking IP geolocation fetch during login panel open
- Location synced to PlayerPrefs on successful authentication

#### Multi-Platform SDK Expansion
- **Flutter / Dart SDK** (`SDKs/flutter/`) - Full IVXManager with auth, profile, wallet, leaderboards, storage, RPC. Dart 3.0+, pub.dev ready.
- **Web3 / TypeScript SDK** (`SDKs/web3/`) - IVXWeb3Manager with MetaMask/EIP-1193 wallet connection, wallet signature auth, NFT queries, token gating, ERC-20 balances. Built on ethers.js v6.
- Total platform count: **10 SDKs** (Unity, Unreal, Godot, Defold, Cocos2d-x, JavaScript, C++, Java, Flutter, Web3)

### Changed
- Consolidated geolocation services - removed redundant `IVXGeolocationService` (GPS-based) and `GeoLocationService` (facade)
- Updated all tracking files, README, docs, and CI/CD for 10-platform coverage

---

## [5.0.0] - 2026-02-27

### Added
- **Friends Module** - IVXFriendsManager, IVXFriendSlot, IVXFriendsPanel with tabs (Online, All, Requests, Blocked)
- Real-time friend status updates via Nakama
- Smooth DOTween animations for list transitions
- All test scenes synced to UPM Samples~/TestScenes

### Fixed
- DOTween animation stacking issues in Friends panel
- Tab switching race conditions with `_isTabSwitching` flag

---

## [4.0.0] - 2026-02-23

### Added
- **IVXPanelForgotPassword** - Complete forgot password UI panel
- **IVXPanelReferral** - Referral code entry and validation panel
- **Weekly Quiz System** - IVXWeeklyQuizManager, IVXWeeklyQuizService, IVXWeeklyQuizDataModels
- **Ads System** - IVXAdsTestController, IVXAdsBootstrap prefab
- **Test Scenes** - AdsTest, AuthTest, LeaderboardTest, WalletTest, WeeklyQuizTest

### Changed
- Updated all Auth UI panels with improved validation
- Enhanced IVXAdsWaterfallManager with better ad network fallback

### Fixed
- Auth canvas panel transitions and state management
- OTP panel auto-focus and resend cooldown edge cases

---

## [3.0.1] - 2026-01-24

### Fixed
- SDK Setup Wizard detects global types correctly
- Feature Setup status checks work for UPM package installs
- Friends prefab adder locates prefabs from Packages paths

---

## [3.0.0] - 2026-01-20

### Added
- **SDK Version Panel** with auto-update check from GitHub releases
- Platform-specific app filtering for "More Of Us" feature

### Fixed
- NullReferenceException in IVXMoreOfUsManager singleton
- "Objects not cleaned up" error when closing scenes

### Changed
- Package version: 2.5.0 to 3.0.0
- Changed `FindObjectOfType` to `FindFirstObjectByType`

---

## [2.5.0] - 2026-01-13

### Added
- **IVXCanvasAuth** - Complete authentication canvas with panel management
- **IVXPanelLogin** - Login with email/password, social auth, guest login
- **IVXPanelRegister** - Registration with validation, terms acceptance
- **IVXPanelOTP** - OTP verification with auto-focus and resend cooldown

### Fixed
- Appodeal/LevelPlay conditional compilation (no compile errors without SDKs)
- GUI layout Begin/End mismatch errors in Setup Wizard

---

## [2.0.0] - 2026-01-13

### Added
- Complete UPM package structure with Samples~/, Tests~/, Documentation~/
- **IVXProjectSetup** - Comprehensive project validation and setup
- **IVXSetupWizard** - Guided dependency installation
- GitHub Actions CI/CD for Unity 2023 LTS and Unity 6

### Changed
- Minimum Unity version bumped to 2023.3 LTS

---

## [1.0.0] - 2025-11-17

### Added
- Initial release with 12 modular packages
- Core, Networking, Storage, Localization, Quiz, IAP, Analytics, Backend, Monetization
- Network success rate: 70% to 95% (retry logic)
- Memory savings: ~50MB (ResourcePool)

---

## Version Support

| Version | Unity Support | Status |
|---------|---------------|--------|
| 5.1.x | 2023.3+ | Active |
| 5.0.x | 2023.3+ | Active |
| 4.2.x | 2021.3+ | Security fixes only |
| < 4.2 | -- | End of life |

---

For detailed per-version notes, see [Assets/_IntelliVerseXSDK/CHANGELOG.md](Assets/_IntelliVerseXSDK/CHANGELOG.md).
