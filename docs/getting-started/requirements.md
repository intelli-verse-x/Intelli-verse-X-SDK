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

| Dependency | Required For | Source |
|------------|--------------|--------|
| Nakama Unity SDK | Backend, Leaderboards, Wallets, Friends | [GitHub](https://github.com/heroiclabs/nakama-unity) |
| Unity Purchasing | IAP | Unity Package Manager |
| LevelPlay SDK | Ads (IronSource) | [Unity Ads](https://unity.com/products/unity-ads) |
| Appodeal SDK | Ads (Appodeal) | [Appodeal](https://appodeal.com) |
| AdMob SDK | Ads (Google) | [AdMob Unity](https://developers.google.com/admob/unity) |

### Optional

| Dependency | Purpose | Source |
|------------|---------|--------|
| DOTween | Animations | Asset Store |
| Photon PUN2 | Multiplayer | Asset Store |
| Apple Auth | Sign in with Apple | [GitHub](https://github.com/lupidan/apple-signin-unity) |
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

The SDK uses these define symbols:

| Symbol | Purpose | Auto-Added |
|--------|---------|------------|
| `INTELLIVERSEX_SDK` | SDK detection | Yes |
| `IVX_NAKAMA` | Nakama integration enabled | No |
| `IVX_PHOTON` | Photon integration enabled | No |
| `IVX_ADS_LEVELPLAY` | LevelPlay ads enabled | No |
| `IVX_ADS_APPODEAL` | Appodeal ads enabled | No |
| `IVX_ADS_ADMOB` | AdMob ads enabled | No |

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
