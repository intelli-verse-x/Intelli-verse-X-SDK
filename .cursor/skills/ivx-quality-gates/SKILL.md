---
name: ivx-quality-gates
description: >-
  Set up automated quality gates, context validation, and CI checks for IntelliVerseX
  SDK game projects. Use when the user says "quality gates", "CI validation",
  "context validation", "TODO tracking", "code quality", "runtime safety",
  "editor guard", "baseline check", "validate context", "pre-commit checks",
  "UPM packaging", "release packaging", "itch.io bundle", "package SDK",
  or needs help with any code quality, packaging, or validation workflow.
version: "1.0.0"
author: "IntelliVerse-X <team@intelli-verse-x.ai>"
allowed-tools:
  - Read
  - Write
  - Edit
  - Glob
  - Grep
  - Shell
---

# IntelliVerseX Quality Gates

## Overview

The Quality Gates module provides automated validation for code quality, context engineering consistency, runtime safety, asset integrity, and release packaging. These tools run locally during development and in CI pipelines to catch issues before they reach production.

```
tools/
├── context/
│   ├── validate_context.py   → Context engineering + code safety checks
│   └── baselines/            → Known TODO/FIXME baseline (gated)
├── scripts/
│   ├── reorganize_for_upm.py → Export as clean UPM package
│   ├── reorganize_for_upm.ps1
│   └── package-itch-bundles.ps1 → Release zip builder
└── validate.ps1              → Root CI launcher
```

---

## 1. Context Validation

### validate_context.py

Ensures context engineering rules are testable and enforced:

```bash
# Local run (warnings are informational)
python tools/context/validate_context.py

# CI run (warnings become errors)
python tools/context/validate_context.py --ci
```

Or via the PowerShell wrapper:

```powershell
# Local
.\tools\validate.ps1

# CI mode
.\tools\validate.ps1 -CI
```

### What It Checks

| Check | What It Validates | Severity |
|-------|------------------|----------|
| **Context loading order** | `.cursorrules`, `AGENTS.md`, and `.cursor/context.md` all list the same numbered context loading sequence | Error |
| **Memory JSON schema** | `.cursor/memory/decisions.json`, `patterns.json`, `corrections.json` are valid JSON with expected structure | Error |
| **Commands registry** | `.cursor/commands.json` matches expected schema (if present) | Error |
| **Runtime safety guard** | No raw `using UnityEditor;` in non-Editor SDK C# files (ignores `#if UNITY_EDITOR` blocks and Editor folders) | Error |
| **TODO/FIXME gating** | New TODO/FIXME in SDK `.cs` files vs tracked baseline | Error (new), Warning (removed) |

### Runtime Safety Guard

The validator scans all C# files under the SDK paths and flags any `using UnityEditor;` that appears outside of `#if UNITY_EDITOR` preprocessor blocks or Editor-only folders. This prevents runtime builds from including editor-only code.

```
PASS: No UnityEditor usage in runtime paths
FAIL: Assets/Intelli-verse-X-SDK/Core/SomeManager.cs:12 — using UnityEditor; outside #if UNITY_EDITOR
```

### TODO/FIXME Baseline Gating

The system tracks known TODOs in a baseline TSV file:

```
tools/context/baselines/todo_fixme_cs.tsv
```

Format (tab-separated):

```
<relative-path>\t<trimmed-line-containing-TODO-or-FIXME>
```

| Scenario | Result |
|----------|--------|
| New TODO added (not in baseline) | **Error** — must be resolved or intentionally added to baseline |
| Existing TODO removed | **Warning** (`BASELINE_STALE`) — update baseline to reflect cleanup |
| All TODOs match baseline | **Pass** |
| Baseline file missing | **Warning** (`BASELINE_MISSING`) — create baseline to enable gating |

### Updating the Baseline

After intentionally resolving TODOs:

```bash
# Regenerate baseline from current state
grep -rn "TODO\|FIXME" Assets/Intelli-verse-X-SDK/ --include="*.cs" | \
    awk -F: '{print $1 "\t" $3}' > tools/context/baselines/todo_fixme_cs.tsv
```

---

## 2. UPM Package Export

### reorganize_for_upm.py

Exports the SDK as a clean Unity Package Manager (UPM) layout from the full project tree:

```bash
# Preview changes without writing files
python tools/scripts/reorganize_for_upm.py --dry-run

# Export to a clean directory
python tools/scripts/reorganize_for_upm.py --output-dir ../IntelliVerseX-UPM/

# Custom source path
python tools/scripts/reorganize_for_upm.py --source-dir Assets/_IntelliVerseXSDK --output-dir dist/upm/
```

### What It Does

| Step | Action |
|------|--------|
| **1. Copy runtime modules** | Core, Identity, Backend, Analytics, Monetization, etc. |
| **2. Copy Editor code** | Editor-only assemblies to `Editor/` |
| **3. Move Samples** | `Samples` → `Samples~` (UPM convention: hidden by default) |
| **4. Move Tests** | `Tests` → `Tests~` (UPM convention) |
| **5. Move Docs** | `Documentation` → `Documentation~` (UPM convention) |
| **6. Copy Icons** | Package icons for UPM manager display |
| **7. Update package.json** | Sets repository and documentation URLs |
| **8. Exclude .meta** | Unity `.meta` files are excluded from the output |

### PowerShell Version

```powershell
.\tools\scripts\reorganize_for_upm.ps1 -OutputDir ..\IntelliVerseX-UPM\
.\tools\scripts\reorganize_for_upm.ps1 -OutputDir dist\upm\ -DryRun
```

---

## 3. Release Bundle Packaging

### package-itch-bundles.ps1

Creates distribution ZIPs for each platform SDK, suitable for itch.io, GitHub Releases, or direct distribution:

```powershell
.\tools\scripts\package-itch-bundles.ps1 -Version "5.8.0"
.\tools\scripts\package-itch-bundles.ps1 -Version "5.8.0" -OutputRoot dist\release\
```

### Output Structure

```
dist/itch/5.8.0/
├── IntelliVerseX-Unity-5.8.0.zip      ← Unity SDK (Assets/Intelli-verse-X-SDK/)
├── IntelliVerseX-Unreal-5.8.0.zip     ← Unreal plugin (SDKs/unreal/)
├── IntelliVerseX-Godot-5.8.0.zip      ← Godot addon (SDKs/godot/)
├── IntelliVerseX-Defold-5.8.0.zip     ← Defold library (SDKs/defold/)
├── IntelliVerseX-Cocos2dx-5.8.0.zip   ← Cocos2d-x SDK (SDKs/cocos2dx/)
├── IntelliVerseX-JavaScript-5.8.0.zip ← npm package (SDKs/javascript/)
├── IntelliVerseX-Cpp-5.8.0.zip        ← C++ SDK (SDKs/cpp/)
└── IntelliVerseX-Java-5.8.0.zip       ← Java/Android SDK (SDKs/java/)
```

### Per-Platform Excludes

| Platform | Excluded |
|----------|---------|
| **Unity** | `Tests~`, `Samples~`, `*.meta` (only in clean export) |
| **JavaScript** | `node_modules/`, `dist/`, `.env` |
| **Java** | `build/`, `.gradle/`, `local.properties` |
| **C++** | `build/`, `cmake-build-*/`, `.o`, `.a` |
| **All** | `.git/`, `.DS_Store`, `__pycache__/` |

---

## 4. CI Pipeline Integration

### GitHub Actions Example

```yaml
name: Quality Gates
on: [push, pull_request]

jobs:
  validate:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4

      - name: Set up Python
        uses: actions/setup-python@v5
        with:
          python-version: '3.11'

      - name: Install dependencies
        run: pip install Pillow jsonschema

      - name: Context validation
        run: python tools/context/validate_context.py --ci

      - name: Asset validation
        run: |
          python tools/asset-pipeline/validate_specs.py --directory RemoteAssets/
          python tools/asset-pipeline/validate_character.py --character RemoteAssets/Characters/ --all
          python tools/asset-pipeline/validate_sound_manifest.py --manifest RemoteAssets/sound_manifest.json

  package:
    runs-on: ubuntu-latest
    needs: validate
    if: startsWith(github.ref, 'refs/tags/v')
    steps:
      - uses: actions/checkout@v4

      - name: Set up Python
        uses: actions/setup-python@v5
        with:
          python-version: '3.11'

      - name: Export UPM package
        run: python tools/scripts/reorganize_for_upm.py --output-dir dist/upm/

      - name: Upload UPM artifact
        uses: actions/upload-artifact@v4
        with:
          name: intelliversex-upm
          path: dist/upm/
```

---

## 5. Pre-Commit Hook Integration

### Setting Up Hooks

```bash
# Create .git/hooks/pre-commit
cat > .git/hooks/pre-commit << 'EOF'
#!/bin/bash
echo "Running IntelliVerseX quality gates..."

# Context validation (fast, no deps)
python3 tools/context/validate_context.py
if [ $? -ne 0 ]; then
    echo "Context validation failed. Fix issues before committing."
    exit 1
fi

echo "Quality gates passed."
EOF

chmod +x .git/hooks/pre-commit
```

---

## 6. Quality Gate Summary

### Full Gate Matrix

| Gate | Local Dev | Pre-Commit | CI (Push) | CI (Release) |
|------|----------|------------|-----------|-------------|
| Context loading order | Optional | Yes | Yes | Yes |
| Memory JSON schema | Optional | Yes | Yes | Yes |
| Runtime safety guard | Optional | Yes | Yes | Yes |
| TODO/FIXME baseline | Optional | Yes | Yes (--ci) | Yes (--ci) |
| Sprite spec validation | After art | No | Yes | Yes |
| Character validation | After art | No | Yes | Yes |
| Sound manifest validation | After audio | No | Yes | Yes |
| UPM package export | Manual | No | No | Yes |
| Release bundle packaging | Manual | No | No | Yes |

---

## 7. Integration with Other Skills

| Skill | Quality Gate Integration |
|-------|------------------------|
| **ivx-asset-pipeline** | Asset validation tools are the primary consumers of schemas |
| **ivx-devops-cicd** | Quality gates are CI pipeline steps in generated workflows |
| **ivx-crashlytics** | Runtime safety guard prevents editor crashes in production |
| **ivx-cross-platform** | Release bundler creates per-platform distribution packages |
| **ivx-remote-config** | Config JSON validated against schemas in CI |

---

## Platform-Specific Quality Considerations

### VR Platforms

| Check | VR Guidance |
|-------|------------|
| **Texture sizes** | Add validation for VR minimum texture requirements (2048x2048). |
| **Poly counts** | Add LOD validation in character_3d.json (min 2 LOD levels for VR). |
| **Frame rate** | Add performance budget checks to CI (90 FPS target for Quest). |

### Console Platforms

| Check | Console Guidance |
|-------|-----------------|
| **Certification** | Add TRC/XR/Lotcheck checklist validation to release gate. |
| **Content rating** | Verify ESRB/PEGI metadata exists before release packaging. |
| **Submission** | Platform-specific packaging requirements (NDA tools needed). |

### WebGL / Browser

| Check | WebGL Guidance |
|-------|---------------|
| **Bundle size** | Add total asset size check (< 50MB recommended for web). |
| **Format** | Verify audio formats are web-compatible (OGG, not WAV). |
| **Compression** | Verify Brotli/gzip compression in build output. |

---

## Checklist

- [ ] `validate_context.py` runs without errors locally
- [ ] TODO/FIXME baseline created and committed
- [ ] Runtime safety guard passing (no UnityEditor in runtime paths)
- [ ] Asset validation scripts integrated into workflow
- [ ] Pre-commit hook installed (optional but recommended)
- [ ] CI workflow includes context and asset validation steps
- [ ] UPM export tested and output verified
- [ ] Release bundle packaging tested for target platforms
- [ ] VR-specific validation rules added (if targeting VR)
- [ ] Console certification checklist gate added (if targeting consoles)
- [ ] WebGL bundle size gate added (if targeting browser)
