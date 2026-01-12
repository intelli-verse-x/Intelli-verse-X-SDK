# 🏛️ IntelliVerseX Unity SDK — Context Authority

> **Authority:** Single source of truth for all AI and human decisions in this repository
> **Version:** 1.0.0
> **Last Updated:** 2026-01-13
> **Status:** Active

---

## 📋 Project Identity

```yaml
name: IntelliVerseX Unity SDK
type: Unity UPM Package
purpose: SDK to integrate Auth, Identity, Analytics, Backend, Social, Monetization into Unity games
unity_version: "6000.2.8f1"
render_pipeline: Universal Render Pipeline (URP) compatible
platforms: [Android, iOS, WebGL, Standalone]
package_name: com.intelliversex.sdk
```

---

## 🎯 Project Vision & Scope

### Vision
Provide a **production-ready, modular Unity SDK** that enables game developers to integrate IntelliVerseX ecosystem features with minimal friction, maximum reliability, and clear documentation.

### Scope (What We Build)

| Module | Purpose | Status |
|--------|---------|--------|
| **Core** | SDK initialization, configuration, lifecycle | Active |
| **Identity** | Authentication (Apple, Google, Email, Guest) | Active |
| **Backend** | Nakama integration, user data, sessions | Active |
| **Analytics** | Event tracking, telemetry | Active |
| **Monetization** | Ads (Appodeal, LevelPlay), IAP, Offerwall | Active |
| **Localization** | Multi-language support | Active |
| **Social** | Referrals, sharing, friends | Active |
| **Leaderboard** | Rankings, scores | Active |
| **Quiz** | Quiz game mode support | Active |
| **Storage** | Cloud save, local persistence | Active |
| **Networking** | Network utilities | Active |

### Non-Scope (What We Don't Build)

| Category | Non-Goal | Reason |
|----------|----------|--------|
| Game Logic | Game-specific features | SDK is infrastructure only |
| UI Themes | Visual customization | Consumer games handle this |
| Server Code | Backend implementation | Separate repository |
| Third-Party SDKs | Modifying Nakama/Photon/Appodeal | Read-only dependencies |

---

## 🔒 Architectural Principles

### SOLID Compliance

| Principle | Application |
|-----------|-------------|
| **S**ingle Responsibility | Each module handles one concern |
| **O**pen/Closed | Extensible via interfaces, closed for modification |
| **L**iskov Substitution | Interfaces define contracts |
| **I**nterface Segregation | Small, focused interfaces |
| **D**ependency Inversion | Depend on abstractions, not concretions |

### Layering Rules

```
┌─────────────────────────────────────────────────────────────┐
│                    CONSUMER GAME LAYER                       │
│                 (Games using this SDK)                       │
├─────────────────────────────────────────────────────────────┤
│                    SDK PUBLIC API LAYER                      │
│        IntelliVerseX.* namespaces (public interfaces)        │
├─────────────────────────────────────────────────────────────┤
│                    SDK INTERNAL LAYER                        │
│           Internal implementations, utilities                │
├─────────────────────────────────────────────────────────────┤
│                    THIRD-PARTY LAYER                         │
│         Nakama, Photon, Appodeal, Apple Auth, etc.          │
└─────────────────────────────────────────────────────────────┘
```

### Dependency Rules

| Layer | May Depend On | Must Not Depend On |
|-------|---------------|-------------------|
| Public API | Internal, Third-Party | Consumer Game |
| Internal | Third-Party | Consumer Game, Public API |
| Third-Party | External APIs only | SDK code |

---

## ⚡ Performance Rules

### Zero-Allocation Hot Paths

```yaml
forbidden_in_update:
  - LINQ queries
  - Lambda allocations
  - String concatenation
  - Boxing/unboxing
  - GetComponent<T>()
  - Find*() methods

required:
  - Pre-allocated collections
  - Cached component references
  - Object pooling for frequent allocations
  - StringBuilder for string building
```

### Memory Budget

| Category | Budget | Enforcement |
|----------|--------|-------------|
| SDK initialization | < 5MB | Profiler check |
| Per-frame overhead | < 0.1ms | Update loop audit |
| Event dispatch | Zero-alloc | Code review |

---

## 🔐 Security & Safety Constraints

### Hard Rules (Never Violate)

| Rule | Reason |
|------|--------|
| No hardcoded secrets | Security vulnerability |
| No plaintext credentials | Data breach risk |
| Server-authoritative for economy | Cheat prevention |
| Validate all external input | Injection prevention |
| Use HTTPS only | Man-in-the-middle prevention |

### Data Handling

```yaml
sensitive_data:
  - User credentials → Never log, encrypt at rest
  - Payment info → Never store locally
  - Session tokens → Secure storage only
  - Analytics → Anonymize PII

logging_rules:
  - Debug builds: Verbose allowed
  - Release builds: Errors only, no PII
```

---

## 📦 UPM Package Standards

### Package Structure

```
Assets/_IntelliVerseXSDK/
├── package.json              # UPM manifest (required)
├── README.md                 # Package documentation
├── CHANGELOG.md              # Version history
├── LICENSE.md                # License file
├── Documentation~/           # External docs (not imported)
├── Editor/                   # Editor-only code
│   └── IntelliVerseX.Editor.asmdef
├── Runtime/                  # Runtime code (main SDK)
│   └── IntelliVerseX.Runtime.asmdef
├── Tests/                    # Unit tests
│   ├── Editor/
│   └── Runtime/
└── Samples~/                 # Optional samples (not imported)
```

### Assembly Definition Rules

| Assembly | Contains | References |
|----------|----------|------------|
| `IntelliVerseX.Core` | Core utilities, config | Unity only |
| `IntelliVerseX.Identity` | Auth providers | Core |
| `IntelliVerseX.Backend` | Nakama integration | Core, Nakama |
| `IntelliVerseX.Monetization` | Ads, IAP | Core |
| `IntelliVerseX.Editor` | Editor tools | All runtime asmdefs |

### Versioning

```yaml
format: MAJOR.MINOR.PATCH
rules:
  - MAJOR: Breaking API changes
  - MINOR: New features, backward compatible
  - PATCH: Bug fixes, no API changes
  
pre_release: -alpha, -beta, -rc
example: 1.2.3-beta.1
```

---

## 📝 Documentation Standards

### Required Documentation

| Type | Location | When Required |
|------|----------|---------------|
| XML docs | Inline | All public APIs |
| README | Module root | Each module |
| CHANGELOG | Package root | Every release |
| API reference | Documentation~/ | Major features |
| Integration guide | Documentation~/ | Complex features |

### XML Documentation Format

```csharp
/// <summary>
/// Brief description of what this does.
/// </summary>
/// <param name="paramName">Description of parameter.</param>
/// <returns>Description of return value.</returns>
/// <exception cref="ExceptionType">When this is thrown.</exception>
/// <example>
/// <code>
/// var result = MyMethod(value);
/// </code>
/// </example>
public ReturnType MyMethod(ParamType paramName) { }
```

---

## 🔄 CI/CD Rules

### Pre-Commit Checks

- [ ] No compiler errors
- [ ] No compiler warnings (treat as errors)
- [ ] All tests pass
- [ ] No TODO/FIXME in production code
- [ ] XML documentation complete
- [ ] CHANGELOG updated

### Release Checklist

- [ ] Version bumped in package.json
- [ ] CHANGELOG entry added
- [ ] All tests pass
- [ ] Documentation updated
- [ ] Breaking changes documented
- [ ] Migration guide (if breaking)

---

## 🎯 Context Lifecycle

### Before Any Work

Load context in this order (authoritative sequence; see also `.cursorrules`):

1. **Check** `.cursor/NON_GOALS.md` for scope boundaries
2. **Check** `.cursor/AI_GUARDRAILS.md` for permissions + escalation triggers
3. **Load** `.cursor/HOT_CONTEXT.md` for quick reference (optional but recommended)
4. **Review** `.cursor/ANTI_PATTERNS.md` before any code generation
5. **Load** this file (`.cursor/context.md`) for vision/principles
6. **Check** `.cursor/architecture.md` for structural rules
7. **Follow** `.cursor/naming-and-style.md` for naming conventions
8. **Verify** assumptions in `.cursor/assumptions.md`

### During Work

1. **Follow** naming conventions from `.cursor/naming-and-style.md`
2. **Respect** layer boundaries
3. **Document** significant decisions
4. **Update** assumptions if changed

### After Work

1. **Verify** against this context
2. **Update** CHANGELOG if applicable
3. **Update** assumptions if new ones made
4. **Log** decisions if significant

---

## 📍 Context File Index

| File | Purpose | Authority Level |
|------|---------|-----------------|
| `.cursor/context.md` | This file - master context | **Highest** |
| `.cursor/architecture.md` | System structure rules | High |
| `.cursor/naming-and-style.md` | Naming conventions | High |
| `.cursor/assumptions.md` | Explicit assumptions | Medium |
| `.cursor/NON_GOALS.md` | Scope boundaries | High |
| `AGENT.md` | Project intelligence index | Reference |
| `AGENTS.md` | Workflow guide | Reference |

---

## ✅ Verification Checklist

Before committing any change, verify:

- [ ] Change aligns with project vision
- [ ] Change respects layer boundaries
- [ ] Change follows naming conventions
- [ ] Change doesn't violate security rules
- [ ] Change doesn't violate performance rules
- [ ] Change is documented appropriately
- [ ] Assumptions are still valid

---

*This document is the authoritative source of truth. All other context documents derive from and must align with this file.*
