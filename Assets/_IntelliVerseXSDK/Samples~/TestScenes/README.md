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

## Usage

1. Import this sample from Package Manager
2. Open the desired test scene
3. Configure any required settings (API keys, backend URLs)
4. Enter Play Mode to test

## Requirements

- IntelliVerseX SDK Core module
- Relevant feature modules for each test (Auth, Monetization, etc.)
- Backend services configured (for Auth, Leaderboard tests)
