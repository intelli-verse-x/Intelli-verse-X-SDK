# 📦 UPM Package Rules — IntelliVerseX Unity SDK

> **Authority:** Rules specific to Unity Package Manager compliance
> **Version:** 1.0.0
> **Last Updated:** 2026-01-13

---

## 🎯 Purpose

These rules ensure the SDK remains a valid, installable UPM package.

---

## 📁 Required Package Structure

```
Assets/_IntelliVerseXSDK/
├── package.json              # UPM manifest (REQUIRED)
├── README.md                 # Package documentation (REQUIRED)
├── CHANGELOG.md              # Version history (REQUIRED)
├── LICENSE.md                # License file (REQUIRED)
├── Documentation~/           # External docs (not imported)
├── Editor/                   # Editor-only code
├── Runtime/                  # Runtime code
├── Tests/                    # Unit tests
│   ├── Editor/
│   └── Runtime/
└── Samples~/                 # Optional samples (not imported)
```

---

## 📋 package.json Requirements

### Required Fields

```json
{
  "name": "com.intelliversex.sdk",
  "version": "1.0.0",
  "displayName": "IntelliVerseX SDK",
  "description": "SDK for IntelliVerseX ecosystem integration",
  "unity": "2021.3",
  "author": {
    "name": "IntelliVerseX",
    "url": "https://intelliversex.com"
  },
  "keywords": [
    "intelliversex",
    "sdk",
    "authentication",
    "analytics",
    "monetization"
  ]
}
```

### Version Format

```yaml
format: MAJOR.MINOR.PATCH
rules:
  - MAJOR: Breaking API changes
  - MINOR: New features, backward compatible
  - PATCH: Bug fixes only

pre_release:
  - alpha: Early development
  - beta: Feature complete, testing
  - rc: Release candidate

examples:
  - 1.0.0
  - 1.1.0-alpha.1
  - 2.0.0-beta.2
  - 2.0.0-rc.1
```

---

## 📝 Assembly Definition Rules

### Naming Convention

| Type | Format | Example |
|------|--------|---------|
| Runtime | `IntelliVerseX.{Module}` | `IntelliVerseX.Identity` |
| Editor | `IntelliVerseX.{Module}.Editor` | `IntelliVerseX.Identity.Editor` |
| Tests | `IntelliVerseX.{Module}.Tests` | `IntelliVerseX.Identity.Tests` |

### Required Settings

```json
{
  "name": "IntelliVerseX.Identity",
  "rootNamespace": "IntelliVerseX.Identity",
  "references": [
    "IntelliVerseX.Core"
  ],
  "includePlatforms": [],
  "excludePlatforms": [],
  "allowUnsafeCode": false,
  "overrideReferences": false,
  "precompiledReferences": [],
  "autoReferenced": true,
  "defineConstraints": [],
  "versionDefines": [],
  "noEngineReferences": false
}
```

### Editor Assembly Settings

```json
{
  "name": "IntelliVerseX.Identity.Editor",
  "rootNamespace": "IntelliVerseX.Identity.Editor",
  "references": [
    "IntelliVerseX.Identity",
    "IntelliVerseX.Core"
  ],
  "includePlatforms": ["Editor"],
  "excludePlatforms": []
}
```

---

## 📖 Documentation Requirements

### README.md

Must include:
- [ ] Package description
- [ ] Installation instructions
- [ ] Quick start guide
- [ ] API overview
- [ ] Requirements/dependencies
- [ ] License information

### CHANGELOG.md

Format:
```markdown
# Changelog

All notable changes to this package will be documented in this file.

## [1.1.0] - 2026-01-13

### Added
- New feature description

### Changed
- Changed behavior description

### Fixed
- Bug fix description

### Deprecated
- Deprecated feature description

### Removed
- Removed feature description

## [1.0.0] - 2026-01-01

### Added
- Initial release
```

---

## 🔗 Dependency Management

### Internal Dependencies

```json
{
  "dependencies": {
    "com.unity.textmeshpro": "3.0.0"
  }
}
```

### Optional Dependencies

Use scripting define symbols:
```csharp
#if INTELLIVERSEX_PHOTON
using Photon.Pun;
#endif
```

### Version Constraints

```json
{
  "dependencies": {
    "com.unity.textmeshpro": "3.0.0"  // Exact version
  }
}
```

---

## 🚫 UPM Prohibitions

### Never Include

| Item | Reason |
|------|--------|
| `.git/` folder | Version control |
| `Library/` folder | Unity cache |
| `.vs/`, `.idea/` | IDE files |
| `*.csproj`, `*.sln` | Generated files |
| Build outputs | Platform-specific |

### Never Do

| Action | Reason |
|--------|--------|
| Hardcode absolute paths | Portability |
| Reference external assets | Package isolation |
| Modify Unity packages | Upstream maintained |
| Use Resources folder | Performance |

---

## ✅ Release Checklist

Before releasing a new version:

- [ ] Version bumped in package.json
- [ ] CHANGELOG.md updated
- [ ] README.md current
- [ ] All tests pass
- [ ] No compiler errors/warnings
- [ ] Documentation complete
- [ ] Breaking changes documented
- [ ] Migration guide (if breaking)
- [ ] Sample code updated

---

## 📦 Distribution

### Git URL Installation

```
https://github.com/intelliversex/unity-sdk.git?path=Assets/_IntelliVerseXSDK
```

### OpenUPM (Future)

```bash
openupm add com.intelliversex.sdk
```

### Local Development

```json
{
  "dependencies": {
    "com.intelliversex.sdk": "file:../Intelli-verse-X-Unity-SDK/Assets/_IntelliVerseXSDK"
  }
}
```

---

*UPM compliance ensures the SDK is easily installable and maintainable across projects.*
