# Changelog

All notable changes to IntelliVerseX SDK will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

---

## [5.4.0] - 2026-04-01

### Added

#### AI Module (`IntelliVerseX.AI`)
- **IVXAISessionManager** — Singleton orchestrator for AI voice and host sessions with dual transport (WebSocket + HTTP polling fallback)
- **IVXAIApiClient** — REST client covering both `ai-voice/*` and `ai-host/*` endpoints (session CRUD, text, audio, entitlement, polling)
- **IVXAIWebSocketClient** — Realtime WebSocket client with auto-reconnect (exponential backoff), heartbeat ping/pong, chunked PCM16 audio streaming, and WebGL JS-interop stubs
- **IVXAIAudioPlayer** — PCM16 queue-based audio playback with base64 decoding and sequential clip management
- **IVXAIAudioRecorder** — Microphone capture with chunked PCM16 encoding for voice input
- **IVXAIEntitlementManager** — AI persona entitlement checks, free session tracking, IAP purchase validation, and subscription status
- **IVXAIConfig** — ScriptableObject configuration for API endpoints, audio settings, language, polling interval, free trial, and UI hints
- **IVXAIPlayerContext** — Player context profiling with personality flags, performance stats, and `GetPersonalitySummary()` for AI host personalisation
- **IVXAIMatchContext** — Match context DTO with `GenerateContextString()` for dynamic host commentary
- **Models** — Full DTO coverage: personas, voice sessions, host sessions, messages, entitlements, products, social proof, analytics

#### Demo UIs (`IntelliVerseX.Demos`)
- **IVXAIVoiceChatDemo** — Persona selection grid, chat bubble interface, text input, push-to-talk mic button, live captions bar, session timer
- **IVXAIHostDemo** — AI Host commentary overlay with player context cards, live message feed, trigger/event controls
- **IVXSpinWheelDemo** — Segmented spin wheel with ease-out animation, reward reveal, spins-remaining counter, cooldown display
- **IVXStreakDemo** — 7-day reward calendar, streak counter, streak shield status, session booster indicator, claim button
- **IVXOfferwallDemo** — Scrollable offer cards with icons, tag badges (HOT/NEW/EASY), reward display, and claim flow

---

## [5.3.0] - 2026-04-01

### Added

#### Retention Module (Hiro)
- **IVXRetentionSystem** — Server-authoritative retention tracking (login streaks, comebacks, milestones)
- **IVXStreakShieldSystem** — Consumable streak shields with purchase and auto-consume
- **IVXSessionBoosterSystem** — Time-gated multiplier bonuses (XP, coins, etc.)
- **IVXAppointmentSystem** — Scheduled time-limited reward windows

#### Monetization Optimization Layer (Hiro)
- **IVXIAPTriggerSystem** — Dynamic IAP offer triggering based on player behavior
- **IVXSmartAdTimerSystem** — Server-managed ad pacing with cooldown and availability tracking
- **IVXAdRevenueOptimizerSystem** — Ad placement configuration with eCPM floors and frequency caps
- **IVXOfferwallSystem** — Third-party offerwall integration with offer listing and reward claiming

#### Engagement Module (Hiro)
- **IVXSpinWheelSystem** — Server-authoritative spin wheel with configurable segments and cooldowns
- **IVXSocialPressureSystem** — Social proof data feeds (recent purchases, live player counts, friend activity)

#### Social Extension (Hiro)
- **IVXFriendQuestSystem** — Cooperative friend quests with progress contribution and reward claiming
- **IVXFriendStreakSystem** — Bilateral friend streaks tracking daily interactions
- **IVXFriendBattleSystem** — Asynchronous friend battle challenges with score submission

#### Platform Utilities
- **IVXDeepLinkManager** — Cross-platform deep link handler with route registration and callback system
- **IVXEdgeToEdgeHelper** — Safe area inset utilities for edge-to-edge display
- **IVXFoldableHelper** — Foldable device state detection and screen configuration tracking
- **IVXPlatformOptimizer** — Cross-platform performance optimizer with quality presets and thermal management

### Changed
- **IVXHiroCoordinator** — Integrated 13 new Hiro systems (total: 33 systems initialized)
- Added `IntelliVerseX.Platform.asmdef` assembly definition for the new Platform module

---

## [5.2.0] - 2026-03-17

### Fixed
- **Java / Android SDK** — Committed the Gradle wrapper (`gradlew`, `gradlew.bat`, `gradle/wrapper/*`) so CI and contributors can run `./gradlew` on Linux/macOS/Windows.
- **CI** — Java SDK workflow: normalize `gradlew` line endings and `chmod +x` before build; run `clean build` + tests.

### Maintenance
- Removed 157 committed C++ build artifacts from `SDKs/cpp/test_package/build/`.
- Added MIT license headers to all 40 IntelliVerseX-owned source files across 7 SDKs.
- **Docs** — Removed `docs/community/nakama-forum-showcase.md` (forum draft superseded by itch.io workflow). Added itch.io publishing plan + page copy (`docs/community/itch-io-publish-plan.md`, `docs/community/itch-io-sdk-page.md`) and `tools/scripts/package-itch-bundles.ps1` for per-platform zip bundles.

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
| 5.2.x | 2023.3+ | Current |
| 5.1.x | 2023.3+ | Active |
| 5.0.x | 2023.3+ | Active |
| 4.2.x | 2021.3+ | Security fixes only |
| < 4.2 | -- | End of life |

---

For detailed per-version notes, see [Assets/_IntelliVerseXSDK/CHANGELOG.md](Assets/_IntelliVerseXSDK/CHANGELOG.md).
