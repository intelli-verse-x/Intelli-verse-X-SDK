# Requirements

Complete system requirements for the IntelliVerseX SDK.

---

## Unity Version

| Unity Version | Support Status | Notes |
|---------------|----------------|-------|
| **Unity 6000.x** | :material-check-circle:{ .success } Fully Supported | Recommended |
| **Unity 2023.3 LTS** | :material-check-circle:{ .success } Fully Supported | Minimum version |
| Unity 2022.x | :material-alert:{ .warning } Partial | May work, not tested |
| Unity 2021.x | :material-close-circle:{ .error } Not Supported | API incompatibilities |
| Unity 2020.x | :material-close-circle:{ .error } Not Supported | Missing features |

!!! info "LTS Recommendation"
    For production games, use Unity 2023.3 LTS or Unity 6 LTS (when available).

---

## Platform Support

### Fully Supported

| Platform | Minimum Version | Notes |
|----------|-----------------|-------|
| Android | API 21 (Lollipop) | Full feature support |
| iOS | iOS 12+ | Full feature support |
| WebGL | - | Ads have limitations |
| Windows | Windows 10+ | Standalone |
| macOS | macOS 10.15+ | Standalone |

### Platform-Specific Notes

=== "Android"

    - **Minimum SDK:** API 21 (Android 5.0)
    - **Target SDK:** API 34 recommended
    - **Architecture:** ARM64-v8a, ARMv7
    - **Gradle:** 7.x+ required
    - **Ad networks:** All supported

=== "iOS"

    - **Minimum Version:** iOS 12
    - **Architecture:** ARM64
    - **Xcode:** 14+ recommended
    - **CocoaPods:** Required for some dependencies
    - **Ad networks:** All supported

=== "WebGL"

    - **Browser:** Modern browsers (Chrome, Firefox, Safari, Edge)
    - **Ads:** Limited (WebGL-specific implementation)
    - **IAP:** Not supported
    - **Backend:** Fully supported

=== "Standalone"

    - **Windows:** 64-bit only
    - **macOS:** Intel and Apple Silicon
    - **Ads:** Not applicable
    - **IAP:** Not applicable
    - **Backend:** Fully supported

---

## Scripting Backend

| Backend | Support | Recommendation |
|---------|---------|----------------|
| **IL2CPP** | :material-check-circle: Recommended | Use for release builds |
| **Mono** | :material-check-circle: Supported | Good for development |

---

## .NET API Compatibility

| Level | Support |
|-------|---------|
| **.NET Standard 2.1** | :material-check-circle: Required |
| .NET 4.x | :material-check-circle: Supported |
| .NET Standard 2.0 | :material-alert: May have issues |

!!! warning "API Compatibility"
    Set to .NET Standard 2.1 in **Edit > Project Settings > Player > Other Settings > Api Compatibility Level**.

---

## Dependencies

### Required (Auto-Installed)

| Dependency | Version | Purpose |
|------------|---------|---------|
| Newtonsoft.Json | 13.0.0+ | JSON serialization |
| TextMeshPro | 3.0.0+ | UI text rendering |

### Required for Features

| Dependency | Required For | Get It |
|------------|--------------|--------|
| Nakama Unity SDK | Backend, Auth, Leaderboards, Friends | [:material-github: GitHub](https://github.com/heroiclabs/nakama-unity) or [:material-shopping: Asset Store](https://assetstore.unity.com/packages/tools/network/nakama-81338) |
| Unity Purchasing | IAP | Unity Package Manager (`com.unity.purchasing`) |
| LevelPlay SDK | Ads (IronSource) | [:material-web: LevelPlay](https://docs.unity.com/monetization-dashboard/en-us/manual/LevelPlaySDKInstallation) |
| Appodeal SDK | Ads (Appodeal) | [:material-web: Appodeal](https://docs.appodeal.com/unity/get-started) |
| AdMob SDK | Ads (Google) | [:material-web: AdMob Unity](https://developers.google.com/admob/unity/quick-start) |

### Optional Enhancements

| Dependency | Purpose | Get It |
|------------|---------|--------|
| Photon PUN2 | Real-time multiplayer | [:material-shopping: Asset Store](https://assetstore.unity.com/packages/tools/network/pun-2-free-119922) |
| DOTween | Smooth UI animations | [:material-shopping: Asset Store](https://assetstore.unity.com/packages/tools/animation/dotween-hotween-v2-27676) |
| Sign in with Apple | Apple ID authentication | [:material-shopping: Asset Store](https://assetstore.unity.com/packages/tools/integration/sign-in-with-apple-plugin-for-unity-152088) |
| Native Share | Social sharing | [:material-shopping: Asset Store](https://assetstore.unity.com/packages/tools/integration/share-for-ios-and-android-309744) |
| Google Sign-In | Google authentication | Unity Package Manager |

---

## Hardware Requirements

### Development Machine

| Component | Minimum | Recommended |
|-----------|---------|-------------|
| RAM | 8 GB | 16+ GB |
| CPU | Quad-core | 8+ cores |
| Storage | 10 GB free | SSD recommended |
| GPU | Any | DirectX 11+ / Metal |

### Target Devices

| Platform | Minimum RAM | Notes |
|----------|-------------|-------|
| Android | 2 GB | 3 GB+ recommended |
| iOS | 2 GB | iPhone 6s or newer |
| WebGL | 4 GB browser | Modern browser required |

---

## Network Requirements

### Backend Services

| Service | Protocol | Port |
|---------|----------|------|
| Nakama API | HTTPS | 443 |
| Nakama Realtime | WSS | 443 |
| Analytics | HTTPS | 443 |

### Firewall Rules

Ensure outbound access to:

- `nakama-rest.intelli-verse-x.ai:443`
- `*.googleapis.com` (for Google services)
- `*.facebook.com` (for Facebook login, if used)
- Ad network endpoints (varies by network)

---

## Scripting Define Symbols

The SDK automatically manages define symbols based on detected dependencies:

| Symbol | Purpose | Auto-Managed |
|--------|---------|--------------|
| `INTELLIVERSEX_SDK` | SDK installed marker | :material-check-circle: Yes |
| `INTELLIVERSEX_HAS_NAKAMA` | Nakama SDK detected | :material-check-circle: Yes |
| `INTELLIVERSEX_HAS_PHOTON` | Photon PUN2 detected | :material-check-circle: Yes |
| `INTELLIVERSEX_HAS_DOTWEEN` | DOTween detected | :material-check-circle: Yes |
| `INTELLIVERSEX_HAS_APPODEAL` | Appodeal SDK detected | :material-check-circle: Yes |
| `INTELLIVERSEX_HAS_LEVELPLAY` | LevelPlay SDK detected | :material-check-circle: Yes |
| `INTELLIVERSEX_HAS_NATIVE_SHARE` | Native Share detected | :material-check-circle: Yes |
| `INTELLIVERSEX_HAS_APPLE_SIGNIN` | Apple Sign-In detected | :material-check-circle: Yes |

!!! tip "Automatic Symbol Management"
    When you import a dependency package, the SDK automatically detects it and adds the appropriate define symbol. No manual configuration required!
    
    Use **IntelliVerseX > SDK Tools > Show Define Symbol Status** to see current symbols.

---

## Summary Checklist

Before starting, verify:

- [x] Unity 2023.3+ or Unity 6 installed
- [x] Git installed and available in PATH
- [x] Target platform configured (Android/iOS SDK if needed)
- [x] .NET Standard 2.1 API compatibility level
- [x] Internet access for backend features
- [x] Required dependencies installed

---

## Next Steps

Ready to install? Continue to:

[:octicons-arrow-right-24: Installation Guide](installation.md)
