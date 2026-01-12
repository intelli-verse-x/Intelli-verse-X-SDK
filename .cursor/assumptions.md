# 📋 Assumptions — IntelliVerseX Unity SDK

> **Authority:** Explicit assumptions that govern development decisions
> **Version:** 1.0.0
> **Last Updated:** 2026-01-13
> **Rule:** Update this file instead of silently changing assumptions

---

## 🎯 Purpose

This document captures **explicit assumptions** made during SDK development. When assumptions change, this file must be updated with:
- What changed
- Why it changed
- Impact on existing code

---

## 🎮 Unity Environment Assumptions

### Unity Version

| Assumption | Value | Confidence | Last Verified |
|------------|-------|------------|---------------|
| Minimum Unity version | 2021.3 LTS | High | 2026-01-13 |
| Target Unity version | 6000.2.x (Unity 6) | High | 2026-01-13 |
| Maximum tested version | 6000.2.8f1 | High | 2026-01-13 |

**Rationale:** Unity 6 is the current major version. We support 2021.3 LTS as minimum for broad compatibility.

### Render Pipeline

| Assumption | Value | Confidence |
|------------|-------|------------|
| Primary pipeline | Universal Render Pipeline (URP) | High |
| Built-in support | Yes, compatible | Medium |
| HDRP support | Not tested, likely works | Low |

**Rationale:** URP is the most common pipeline for mobile games. SDK has no rendering dependencies.

### Scripting Backend

| Assumption | Value | Confidence |
|------------|-------|------------|
| IL2CPP support | Required | High |
| Mono support | Supported | High |
| .NET Standard | 2.1 | High |

**Rationale:** IL2CPP is required for iOS and recommended for Android. SDK must work with both backends.

---

## 📱 Platform Assumptions

### Supported Platforms

| Platform | Support Level | Notes |
|----------|---------------|-------|
| Android | Full | API 24+ (Android 7.0+) |
| iOS | Full | iOS 13.0+ |
| WebGL | Full | Modern browsers |
| Windows Standalone | Partial | Development/testing |
| macOS Standalone | Partial | Development/testing |
| Linux | Not tested | May work |
| Consoles | Not supported | Out of scope |

### Platform-Specific Assumptions

#### Android
- Google Play Services available
- Google Play Billing for IAP
- Appodeal/LevelPlay for ads
- Min SDK: 24, Target SDK: 34

#### iOS
- App Store distribution
- Apple Sign-In available
- StoreKit for IAP
- Appodeal/LevelPlay for ads
- Deployment target: iOS 13.0

#### WebGL
- Modern browser (Chrome 90+, Firefox 90+, Safari 14+)
- WebGL 2.0 support
- No native plugins
- Alternative ad providers (web-based)

---

## 🔌 Third-Party Dependencies

### Required Dependencies

| Dependency | Version | Purpose | Assumption |
|------------|---------|---------|------------|
| Nakama | 3.x | Backend services | Always available |
| Photon PUN2 | 2.x | Multiplayer | Optional, may not be present |
| DOTween | 1.x | Animations | Optional, may not be present |

### Ad Network Dependencies

| Dependency | Version | Purpose | Assumption |
|------------|---------|---------|------------|
| Appodeal | Latest | Ad mediation | Primary ad provider |
| LevelPlay (ironSource) | Latest | Ad mediation | Alternative provider |
| Google Mobile Ads | Latest | AdMob | Included via mediation |

### Auth Dependencies

| Dependency | Version | Purpose | Assumption |
|------------|---------|---------|------------|
| Apple Auth | Latest | Sign in with Apple | iOS only |
| Google Sign-In | Latest | Google authentication | Android/iOS |

---

## 👤 User Assumptions

### Developer Skill Level

| Assumption | Value | Rationale |
|------------|-------|-----------|
| Unity experience | Intermediate+ | SDK is not for beginners |
| C# proficiency | Intermediate+ | Async/await, events, interfaces |
| Mobile dev experience | Some | Platform-specific concepts |

### Integration Expectations

| Assumption | Value |
|------------|-------|
| Integration time | 1-4 hours for basic setup |
| Documentation reading | Expected before integration |
| Sample code usage | Provided and expected to be used |

---

## 🏗️ Architecture Assumptions

### Singleton Pattern

| Assumption | Value | Rationale |
|------------|-------|-----------|
| Manager pattern | Singleton with DontDestroyOnLoad | Standard Unity pattern |
| Initialization | Explicit via InitializeAsync | Predictable lifecycle |
| Cleanup | Automatic on scene unload | Memory management |

### Async Pattern

| Assumption | Value | Rationale |
|------------|-------|-----------|
| Async library | System.Threading.Tasks | Standard .NET |
| UniTask support | Optional, not required | Performance optimization |
| Coroutine support | For MonoBehaviour contexts | Unity compatibility |

### Event Pattern

| Assumption | Value | Rationale |
|------------|-------|-----------|
| Event type | C# events (Action<T>) | Simple, standard |
| UnityEvent support | For inspector binding | Designer-friendly |
| Event threading | Main thread only | Unity thread safety |

---

## 🔐 Security Assumptions

### Authentication

| Assumption | Value | Rationale |
|------------|-------|-----------|
| Token storage | Secure platform storage | PlayerPrefs not secure |
| Token refresh | Automatic by SDK | Seamless UX |
| Session timeout | Server-controlled | Security best practice |

### Data Handling

| Assumption | Value | Rationale |
|------------|-------|-----------|
| PII handling | Minimal, anonymized | Privacy compliance |
| Data encryption | In transit (HTTPS) | Standard security |
| Local storage | Non-sensitive only | Security best practice |

---

## 📊 Performance Assumptions

### Memory Budget

| Assumption | Value | Rationale |
|------------|-------|-----------|
| SDK memory footprint | < 10MB | Mobile constraints |
| Per-frame allocation | Zero in hot paths | GC pressure |
| Texture memory | Minimal (UI only) | Mobile constraints |

### CPU Budget

| Assumption | Value | Rationale |
|------------|-------|-----------|
| Update loop overhead | < 0.1ms | 60fps target |
| Initialization time | < 3 seconds | UX requirement |
| Network timeout | 30 seconds default | Balance UX/reliability |

---

## 🌐 Network Assumptions

### Connectivity

| Assumption | Value | Rationale |
|------------|-------|-----------|
| Internet required | Yes, for most features | Backend-dependent |
| Offline mode | Limited (cached data) | Mobile reality |
| Retry logic | Automatic with backoff | Reliability |

### Backend

| Assumption | Value | Rationale |
|------------|-------|-----------|
| Nakama availability | 99.9% uptime | Production requirement |
| API versioning | Backward compatible | SDK stability |
| Response time | < 500ms average | UX requirement |

---

## 📦 UPM Assumptions

### Package Distribution

| Assumption | Value | Rationale |
|------------|-------|-----------|
| Distribution method | Git URL + OpenUPM | Standard UPM |
| Version format | SemVer (MAJOR.MINOR.PATCH) | UPM requirement |
| Unity Package Manager | Available | Unity 2019.3+ |

### Dependencies

| Assumption | Value | Rationale |
|------------|-------|-----------|
| Dependency resolution | UPM handles | Standard behavior |
| Conflicting versions | Consumer resolves | UPM limitation |
| Optional dependencies | Conditional compilation | Flexibility |

---

## 🔄 Change Log

### Template for Recording Changes

```markdown
### [DATE] - [ASSUMPTION CHANGED]

**Previous:** [Old assumption]
**New:** [New assumption]
**Reason:** [Why the change was made]
**Impact:** [What code/docs need updating]
**Updated by:** [Who made the change]
```

### Change History

#### 2026-01-13 - Initial Assumptions Document

**Previous:** No documented assumptions
**New:** All assumptions documented
**Reason:** Establishing context engineering system
**Impact:** None (initial creation)
**Updated by:** Cursor AI

---

## ✅ Assumption Review Checklist

When making decisions, verify:

- [ ] Does this align with documented assumptions?
- [ ] If not, should the assumption change?
- [ ] Have I documented the new assumption?
- [ ] Have I noted the impact of the change?
- [ ] Have I updated affected code/docs?

---

*When in doubt about an assumption, document it here. Explicit assumptions prevent implicit drift.*
