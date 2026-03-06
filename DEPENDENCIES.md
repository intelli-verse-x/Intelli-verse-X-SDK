# IntelliVerseX SDK - Dependencies Guide

This guide covers all dependencies for the IntelliVerseX SDK across all supported platforms.

---

## Platform Dependencies (Nakama Client Libraries)

Each platform requires its respective Nakama client library:

| Platform | Nakama Client | Install Method |
|----------|---------------|----------------|
| Unity / .NET | [nakama-unity](https://github.com/heroiclabs/nakama-unity) | UPM / Asset Store |
| Unreal Engine | [nakama-unreal](https://github.com/heroiclabs/nakama-unreal) | Plugin |
| Godot Engine | [nakama-godot](https://github.com/heroiclabs/nakama-godot) | Addon |
| Defold | [nakama-defold](https://github.com/heroiclabs/nakama-defold) | Library URL |
| Cocos2d-x | [nakama-cpp](https://github.com/heroiclabs/nakama-cpp) | CMake |
| JavaScript | [@heroiclabs/nakama-js](https://github.com/heroiclabs/nakama-js) | npm |
| C / C++ | [nakama-cpp](https://github.com/heroiclabs/nakama-cpp) | CMake |
| Java / Android | [nakama-java](https://github.com/heroiclabs/nakama-java) | Gradle / Maven |

---

## Unity-Specific Dependencies

---

## 📦 Required Dependencies

These are automatically installed with the SDK if using UPM. For .unitypackage installs, add manually:

| Package | UPM ID | Purpose |
|---------|--------|---------|
| Newtonsoft.Json | `com.unity.nuget.newtonsoft-json` | JSON serialization |
| TextMeshPro | `com.unity.textmeshpro` | UI text rendering |

### Installation via Package Manager

```
Window > Package Manager > + > Add package by name
```

---

## 🎮 Core Features (Nakama)

**Required for:** Authentication, Leaderboards, Friends, Backend Services, User Profiles

### Option 1: Asset Store (Recommended)
[**Nakama - Unity Asset Store**](https://assetstore.unity.com/packages/tools/network/nakama-81338)

### Option 2: UPM (GitHub)
Add to `Packages/manifest.json`:
```json
{
  "dependencies": {
    "com.heroiclabs.nakama-unity": "https://github.com/heroiclabs/nakama-unity.git?path=Packages/Nakama#v3.10.0"
  }
}
```

### Option 3: Manual Download
[**GitHub Releases**](https://github.com/heroiclabs/nakama-unity/releases)

> **Auto-detected symbol:** `INTELLIVERSEX_HAS_NAKAMA`

---

## 🎯 Multiplayer (Photon PUN2)

**Required for:** Real-time multiplayer lobbies, matchmaking

### Asset Store
[**PUN 2 - FREE**](https://assetstore.unity.com/packages/tools/network/pun-2-free-119922)

After import, run the **Photon PUN Wizard** to configure your App ID.

> **Auto-detected symbol:** `INTELLIVERSEX_HAS_PHOTON`

---

## ✨ Animations (DOTween)

**Required for:** Intro scene animations, UI transitions

### Asset Store
[**DOTween (HOTween v2)**](https://assetstore.unity.com/packages/tools/animation/dotween-hotween-v2-27676)

After import, run **Tools > Demigiant > DOTween Utility Panel** and click **Setup DOTween**.

> **Auto-detected symbol:** `INTELLIVERSEX_HAS_DOTWEEN`

---

## 🍎 Apple Sign-In

**Required for:** iOS Apple ID authentication

### Asset Store
[**Sign in with Apple Plugin for Unity**](https://assetstore.unity.com/packages/tools/integration/sign-in-with-apple-plugin-for-unity-152088)

### Configuration
1. Enable **Sign in with Apple** capability in Xcode
2. Configure Apple Developer portal with Sign in with Apple

> **Auto-detected symbol:** `INTELLIVERSEX_HAS_APPLE_SIGNIN`

---

## 📤 Native Share

**Required for:** Social sharing on iOS/Android

### Asset Store
[**Share for iOS & Android**](https://assetstore.unity.com/packages/tools/integration/share-for-ios-and-android-309744)

> **Auto-detected symbol:** `INTELLIVERSEX_HAS_NATIVE_SHARE`

---

## 📺 Ad Mediation

### LevelPlay (IronSource)

**Required for:** LevelPlay/IronSource ad monetization

[**LevelPlay SDK Installation Guide**](https://docs.unity.com/monetization-dashboard/en-us/manual/LevelPlaySDKInstallation)

> **Auto-detected symbol:** `INTELLIVERSEX_HAS_LEVELPLAY`

### Appodeal

**Required for:** Appodeal ad monetization

[**Appodeal Unity SDK**](https://docs.appodeal.com/unity/get-started)

> **Auto-detected symbol:** `INTELLIVERSEX_HAS_APPODEAL`

---

## 🔄 Automatic Symbol Management

The SDK automatically detects installed dependencies and sets appropriate scripting define symbols:

| When You Install | SDK Auto-Adds |
|------------------|---------------|
| Nakama SDK | `INTELLIVERSEX_HAS_NAKAMA` |
| Photon PUN2 | `INTELLIVERSEX_HAS_PHOTON` |
| DOTween | `INTELLIVERSEX_HAS_DOTWEEN` |
| Apple Auth | `INTELLIVERSEX_HAS_APPLE_SIGNIN` |
| Native Share | `INTELLIVERSEX_HAS_NATIVE_SHARE` |
| LevelPlay | `INTELLIVERSEX_HAS_LEVELPLAY` |
| Appodeal | `INTELLIVERSEX_HAS_APPODEAL` |

### Check Your Status

Use **IntelliVerseX > SDK Tools > Show Define Symbol Status** to see which dependencies are detected.

### Force Refresh

If symbols aren't updating, use **IntelliVerseX > SDK Tools > Reapply Define Symbols**.

---

## ⚠️ Troubleshooting

### "Missing assembly reference" errors

Install the required dependency package. The SDK modules have `defineConstraints` that prevent compilation until dependencies are available.

### Symbols not being added

1. Check **IntelliVerseX > SDK Tools > Show Define Symbol Status**
2. Try **IntelliVerseX > SDK Tools > Reapply Define Symbols**
3. Ensure the package is fully imported (no import errors)

### Features not appearing

Some SDK features only activate when their dependency is installed. For example:
- Backend features require Nakama
- Multiplayer requires Photon
- Intro animations require DOTween

---

## 📋 Quick Reference

| Feature | Required Package | Asset Store Link |
|---------|------------------|------------------|
| Backend/Auth | Nakama | [Get](https://assetstore.unity.com/packages/tools/network/nakama-81338) |
| Multiplayer | Photon PUN2 | [Get](https://assetstore.unity.com/packages/tools/network/pun-2-free-119922) |
| Animations | DOTween | [Get](https://assetstore.unity.com/packages/tools/animation/dotween-hotween-v2-27676) |
| Apple Sign-In | Apple Auth | [Get](https://assetstore.unity.com/packages/tools/integration/sign-in-with-apple-plugin-for-unity-152088) |
| Social Share | Native Share | [Get](https://assetstore.unity.com/packages/tools/integration/share-for-ios-and-android-309744) |

---

*For full documentation, visit [intelliversex.github.io/intelliversex-unity-sdk](https://intelliversex.github.io/intelliversex-unity-sdk/)*
