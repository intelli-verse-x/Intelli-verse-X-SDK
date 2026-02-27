# Authentication Demo

This sample demonstrates the complete authentication flow in IntelliVerseX SDK.

## Features

- **Login Flow**: Email/password authentication with Nakama backend
- **Registration**: New user signup with validation
- **OTP Verification**: One-time password verification
- **Social Auth**: Google, Apple, and other social providers
- **Session Management**: Automatic session persistence

## Setup

1. Import this sample via Window > Package Manager > IntelliVerseX SDK > Samples
2. Configure Nakama backend in Window > IntelliVerseX > Project Setup Wizard
3. Open `IVX_AuthTest.unity` scene
4. Press Play to test authentication flow

## Prefabs

- `IVXAuthCanvas.prefab` - Complete authentication UI
- `NakamaManager.prefab` - Backend connection manager

## Scripts

Uses the `IntelliVerseX.Auth` namespace for authentication logic.

## Requirements

- Nakama backend server configured
- TextMeshPro installed (auto-installed with SDK)
