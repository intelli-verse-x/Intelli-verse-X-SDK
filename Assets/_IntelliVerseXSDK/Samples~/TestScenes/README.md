# IntelliVerseX SDK - Test Scenes

This sample contains pre-built test scenes for validating SDK integrations.

## Available Scenes

### IVX_AdsTest
Complete ads integration test scene featuring:
- Banner, interstitial, and rewarded ad testing
- Multiple ad network support (Appodeal, LevelPlay, AdMob)
- Waterfall fallback demonstration
- Ad event logging

### IVX_AuthTest
Authentication flow test scene with:
- Login with email/password
- Social authentication (Google, Apple)
- Guest login
- OTP verification
- Registration flow
- Forgot password

### IVX_LeaderboardTest
Leaderboard integration test scene showing:
- Score submission
- Global rankings
- Around-player rankings
- Nakama backend integration

### IVX_WalletTest
Wallet/IAP test scene demonstrating:
- Product fetching
- Purchase flow
- Balance display
- Transaction history

### IVX_WeeklyQuizTest
Weekly quiz system test scene with:
- Quiz session management
- Category selection
- Timer-based questions
- Score tracking
- Prize display

### IVX_ProfileTest
Production-ready profile flow scene covering:
- Post-auth profile bootstrap via `IVXNProfileManager`
- Profile fetch/update with validation and version-conflict handling
- Portfolio fetch with game/global wallet snapshots
- Error mapping for `AUTH_REQUIRED`, `RATE_LIMITED`, `VERSION_CONFLICT`, and upstream failures
- IMGUI fallback UI so the sample works without extra Canvas setup

## Usage

1. Import this sample from Package Manager
2. Open the desired test scene
3. Configure any required settings (API keys, backend URLs)
4. Enter Play Mode to test

## Requirements

- IntelliVerseX SDK Core module
- Relevant feature modules for each test (Auth, Monetization, etc.)
- Backend services configured (for Auth, Leaderboard tests)
- Nakama profile RPCs available:
  - `create_or_sync_user`
  - `rpc_update_player_metadata`
  - `get_player_metadata`
  - `get_player_portfolio`
  - `check_geo_and_update_profile`
