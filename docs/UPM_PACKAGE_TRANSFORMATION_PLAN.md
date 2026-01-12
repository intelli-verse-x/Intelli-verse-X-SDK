# 📦 UPM Package Transformation Plan

> **Goal:** Transform this repository into a properly structured Unity Package Manager (UPM) package
> **Current State:** Development Unity project with SDK embedded
> **Target State:** Clean UPM package repository that developers can install via Git URL

---

## 📊 Current State Analysis

### ✅ What You Already Have (Good!)

| Item | Status | Notes |
|------|--------|-------|
| `package.json` | ✅ Exists | Well-structured with dependencies |
| Assembly Definitions | ✅ 20 asmdefs | Good modular structure |
| SDK Folder | ✅ `Assets/_IntelliVerseXSDK/` | Main package content |
| README.md | ✅ Exists | In SDK root |
| CHANGELOG.md | ✅ Exists | In SDK root |
| Documentation | ✅ 17 .md files | In Documentation folder |
| Modular Structure | ✅ Excellent | Core, Identity, Backend, Monetization, etc. |
| Editor Scripts | ✅ Exists | Setup wizards, validators |

### ❌ Issues to Fix

| Issue | Problem | Impact |
|-------|---------|--------|
| **Third-party SDKs in Assets/** | Nakama, Photon, AppleAuth, Appodeal in Assets/ | Bloats package, licensing issues |
| **Repository structure** | Full Unity project, not package root | Confusing Git URL path |
| **LICENSE file** | Not in package root | Required for UPM |
| **Samples location** | Examples in package, not `Samples~` | Won't appear in Package Manager |
| **Test files** | No Tests~ folder | No testable structure |
| **IntroScene assets** | Binary assets (mp3, images) in package | Bloats package size |

---

## 🎯 Recommended Repository Structure

### Option A: Package at Repository Root (RECOMMENDED)

```
Intelli-verse-X-Unity-SDK/           # Repository root = Package root
├── package.json                      # UPM package manifest
├── README.md                         # Package documentation
├── CHANGELOG.md                      # Version history
├── LICENSE                           # License file
├── Runtime/                          # Runtime code
│   ├── IntelliVerseX.Runtime.asmdef
│   ├── Core/
│   ├── Identity/
│   ├── Backend/
│   ├── Monetization/
│   ├── Analytics/
│   ├── Localization/
│   ├── Storage/
│   ├── Networking/
│   ├── Leaderboard/
│   ├── Social/
│   ├── Quiz/
│   ├── QuizUI/
│   └── UI/
├── Editor/                           # Editor-only code
│   ├── IntelliVerseX.Editor.asmdef
│   └── (all editor scripts)
├── Samples~/                         # Importable samples (hidden by ~)
│   ├── QuizDemo/
│   ├── Localization/
│   └── IAP/
├── Tests~/                           # Tests (hidden by ~)
│   ├── Editor/
│   └── Runtime/
├── Documentation~/                   # Docs (hidden by ~)
│   └── (all .md files)
├── .cursor/                          # Context engineering
└── .github/                          # CI/CD
```

**Installation:**
```json
"com.intelliverse.gamesdk": "https://github.com/intelliverse-x/unity-sdk.git"
```

### Option B: Package in Subdirectory (Current Approach)

```
Intelli-verse-X-Unity-SDK/           # Repository root
├── Assets/
│   └── _IntelliVerseXSDK/           # Package root
│       ├── package.json
│       └── ...
├── Packages/
├── ProjectSettings/
└── ...
```

**Installation:**
```json
"com.intelliverse.gamesdk": "https://github.com/intelliverse-x/unity-sdk.git?path=Assets/_IntelliVerseXSDK"
```

### ⚠️ Recommendation: Use Option A

**Why Option A is better:**
1. Cleaner Git URL (no `?path=` parameter)
2. Standard UPM package structure
3. Easier for consumers to understand
4. Better for versioning and releases
5. No Unity project bloat in consumers' projects

---

## 🔧 Transformation Steps

### Phase 1: Clean Up Third-Party Dependencies

**Problem:** Your Assets/ folder contains third-party SDKs that shouldn't be in your package.

**Current third-party SDKs:**
- `Assets/Nakama/` - Nakama Unity SDK
- `Assets/Photon/` - Photon PUN2
- `Assets/AppleAuth/` - Apple Sign-In
- `Assets/Appodeal/` - Appodeal Ads
- `Assets/LevelPlay/` - LevelPlay/IronSource
- `Assets/Plugins/Demigiant/` - DOTween

**Solution:** These should be **documented dependencies**, not included in your package.

**Action:**
1. Document all dependencies in `package.json` (already done ✅)
2. Create a dependency checker/installer in Editor scripts (you have `IVXDependencyChecker.cs` ✅)
3. Remove third-party folders from Git tracking for the clean package repo

### Phase 2: Restructure for Option A

**Create new branch or repository with this structure:**

```bash
# New repository structure
/
├── package.json              # Move from Assets/_IntelliVerseXSDK/
├── README.md                 # Move from Assets/_IntelliVerseXSDK/
├── CHANGELOG.md              # Move from Assets/_IntelliVerseXSDK/
├── LICENSE                   # Create or move
├── Runtime/
│   ├── IntelliVerseX.asmdef  # New: Main runtime assembly
│   ├── Core/                 # From Assets/_IntelliVerseXSDK/Core/
│   ├── Identity/             # From Assets/_IntelliVerseXSDK/Identity/
│   ├── Backend/              # From Assets/_IntelliVerseXSDK/Backend/
│   ├── Monetization/         # From Assets/_IntelliVerseXSDK/Monetization/
│   ├── Analytics/            # From Assets/_IntelliVerseXSDK/Analytics/
│   ├── Localization/         # From Assets/_IntelliVerseXSDK/Localization/
│   ├── Storage/              # From Assets/_IntelliVerseXSDK/Storage/
│   ├── Networking/           # From Assets/_IntelliVerseXSDK/Networking/
│   ├── Leaderboard/          # From Assets/_IntelliVerseXSDK/Leaderboard/
│   ├── Social/               # From Assets/_IntelliVerseXSDK/Social/
│   ├── Quiz/                 # From Assets/_IntelliVerseXSDK/Quiz/
│   ├── QuizUI/               # From Assets/_IntelliVerseXSDK/QuizUI/
│   └── UI/                   # From Assets/_IntelliVerseXSDK/UI/
├── Editor/
│   ├── IntelliVerseX.Editor.asmdef
│   └── (all editor scripts)
├── Samples~/
│   └── (examples - won't be imported by default)
├── Tests~/
│   ├── Editor/
│   │   └── IntelliVerseX.Editor.Tests.asmdef
│   └── Runtime/
│       └── IntelliVerseX.Tests.asmdef
└── Documentation~/
    └── (all .md docs)
```

### Phase 3: Update Assembly Definitions

**Current:** 20 separate asmdefs (one per module)
**Consideration:** Keep modular OR consolidate

**Option A: Keep Modular (Recommended for SDK)**
- Allows consumers to reference only what they need
- Better compile time isolation
- More complex dependency management

**Option B: Consolidate**
- Simpler for consumers
- Single `IntelliVerseX.Runtime.asmdef` + `IntelliVerseX.Editor.asmdef`
- Faster initial setup

**Recommended: Keep modular but fix naming consistency**

Current naming issues:
- `IntelliVerseXIdentity.asmdef` → Should be `IntelliVerseX.Identity.asmdef`

### Phase 4: Move Binary Assets

**Problem:** `IntroScene/` contains binary assets (mp3, images) that bloat package size.

**Solution:**
1. Move to `Samples~/IntroScene/` (optional import)
2. Or create separate "IntelliVerseX Assets" package
3. Or host on CDN with Addressables

### Phase 5: Update package.json

**Fixes needed:**

```json
{
  "name": "com.intelliversex.sdk",  // Fix: lowercase, consistent
  "version": "1.0.0",
  "displayName": "IntelliVerseX SDK",
  "description": "...",
  "unity": "2021.3",
  "unityRelease": "0f1",  // Add: Specific Unity release
  "documentationUrl": "https://github.com/intelliverse-x/unity-sdk/wiki",  // Add
  "changelogUrl": "https://github.com/intelliverse-x/unity-sdk/blob/main/CHANGELOG.md",  // Add
  "licensesUrl": "https://github.com/intelliverse-x/unity-sdk/blob/main/LICENSE",  // Add
  "dependencies": {
    "com.unity.textmeshpro": "3.0.6"  // Use minimum supported version
  },
  "samples": [
    {
      "displayName": "Quiz Demo",
      "description": "Complete quiz game example",
      "path": "Samples~/QuizDemo"  // Update path
    }
  ]
}
```

---

## 📋 Action Checklist

### Immediate (Do Now)

- [ ] Create `LICENSE` file in `Assets/_IntelliVerseXSDK/`
- [ ] Fix asmdef naming: `IntelliVerseXIdentity.asmdef` → `IntelliVerseX.Identity.asmdef`
- [ ] Add `.meta` files for any missing files
- [ ] Update `package.json` with documentation URLs

### Short-Term (This Week)

- [ ] Decide: Keep current structure OR migrate to Option A
- [ ] If Option A: Create new clean repository
- [ ] Move `Examples/` to `Samples~/`
- [ ] Move `Documentation/` to `Documentation~/`
- [ ] Create `Tests~/` folder structure

### Medium-Term (Before Release)

- [ ] Create migration script for existing users
- [ ] Test installation via Git URL
- [ ] Test in fresh Unity project
- [ ] Verify all dependencies are documented
- [ ] Create integration tests

---

## 🚀 Quick Start: Minimal Changes for Current Structure

If you want to **keep the current structure** but make it a valid UPM package:

### 1. Add LICENSE file

```bash
# In Assets/_IntelliVerseXSDK/
LICENSE
```

### 2. Verify all required files exist

```
Assets/_IntelliVerseXSDK/
├── package.json     ✅ Exists
├── README.md        ✅ Exists  
├── CHANGELOG.md     ✅ Exists
├── LICENSE          ❌ CREATE THIS
└── *.asmdef files   ✅ Exist
```

### 3. Fix package.json name

```json
{
  "name": "com.intelliversex.sdk"  // Currently: com.intelliverse.gamesdk
}
```

### 4. Installation Instructions for Users

```json
// Packages/manifest.json
{
  "dependencies": {
    "com.intelliversex.sdk": "https://github.com/YOUR_ORG/Intelli-verse-X-Unity-SDK.git?path=Assets/_IntelliVerseXSDK"
  }
}
```

---

## ⚠️ Important Considerations

### Third-Party Dependencies

Your SDK depends on:
- Nakama (required)
- Photon PUN2 (required)
- DOTween (required)
- Apple Sign-In (required for iOS)
- Appodeal/LevelPlay (optional)

**These CANNOT be included in your package due to:**
1. Licensing restrictions
2. Package size
3. Version conflicts

**Solution:** Your `IVXDependencyChecker.cs` should guide users to install these separately.

### Version Compatibility

Your package targets Unity 2021.3 LTS but you're developing on 6000.2.8f1. Ensure:
- Code compiles on 2021.3
- No 6000+ only APIs used (or use version defines)
- Test on minimum supported version

---

## 📊 Summary

| Aspect | Current | Recommended |
|--------|---------|-------------|
| Structure | Unity project with embedded SDK | Package-only repository |
| Installation | `?path=Assets/_IntelliVerseXSDK` | Direct Git URL |
| Third-party SDKs | Included in Assets/ | External dependencies |
| Samples | In package | In `Samples~/` |
| Tests | None | In `Tests~/` |
| LICENSE | Missing | Required |

---

*This plan outlines how to transform your SDK into a production-ready UPM package.*
