# 🔒 Context Engineering Enforcement

> **Authority:** Defines how Context Engineering rules are enforced
> **Version:** 1.0.0
> **Last Updated:** 2026-01-13
> **Status:** Production-Ready

---

## Purpose

This document defines the **enforcement mechanisms** that ensure Context Engineering rules are followed automatically, not just documented.

---

## Enforcement Layers

### Layer 1: Automated Validation (CI/CD)

**GitHub Actions Workflows:**
- `.github/workflows/context-validation.yml` - Validates context files on PR
- `.github/workflows/package-validation.yml` - Validates UPM package structure
- `.github/workflows/release-tag.yml` - Creates releases when tags are pushed

**When:** On every PR, on push to main, on version tags

**What it catches:**
- ✅ Missing required context files
- ✅ Read-only zone modifications
- ✅ Invalid JSON in memory files
- ✅ Memory schema + guardrails validator failures (`tools/context/validate_context.py`)
- ✅ Invalid package.json structure
- ✅ Missing UPM required fields
- ✅ Version mismatch between tag and package.json

---

### Layer 2: PR Template Enforcement

**File:** `.github/pull_request_template.md`

**Enforces:**
- ✅ Guardrails checklist (P0, P1, P2)
- ✅ Context engineering checklist
- ✅ Testing checklist
- ✅ Documentation checklist

**Automatic reminders:**
- Check NON_GOALS.md before implementing
- Follow naming conventions
- Update AGENT.md if scripts added

---

### Layer 3: Code Review (Human)

**What reviewers check:**
- ✅ Guardrails compliance
- ✅ Architecture boundaries respected
- ✅ Naming conventions followed
- ✅ No anti-patterns
- ✅ Documentation updated

**Automatic rejection criteria:**
- ❌ Third-party SDK code modified
- ❌ Hardcoded secrets
- ❌ Layer violations
- ❌ Compilation errors
- ❌ Missing IVX prefix on public types

---

### Layer 4: AI Self-Enforcement

**Files that AI must check:**
1. `NON_GOALS.md` - Before any implementation
2. `AI_GUARDRAILS.md` - For permission checks
3. `ANTI_PATTERNS.md` - Before code generation
4. `architecture.md` - For structural decisions
5. `naming-and-style.md` - For code generation

**AI behavioral rules:**
- Stop at read-only zone boundaries
- Ask before expanding scope
- Document significant decisions
- Follow minimum viable change rule

---

## Validation Checks

### Required Context Files Check

```yaml
required_files:
  - .cursor/context.md
  - .cursor/architecture.md
  - .cursor/naming-and-style.md
  - .cursor/assumptions.md
  - .cursor/NON_GOALS.md
  - .cursor/AI_GUARDRAILS.md
  - .cursor/ANTI_PATTERNS.md
  - .cursor/HOT_CONTEXT.md
  - .cursor/FRESHNESS.md
  - .cursor/SYSTEM_MAP.md
  - .cursor/rules/core/RULE.md
  - .cursor/rules/upm/RULE.md
  - .cursorrules
  - AGENT.md
  - AGENTS.md
```

### Read-Only Zone Check

```yaml
read_only_zones:
  - Assets/Nakama/
  - Assets/Photon/
  - Assets/Appodeal/
  - Assets/LevelPlay/
  - Assets/AppleAuth/
  - Assets/Plugins/Demigiant/
  - Assets/Plugins/NativeFilePicker/
```

### JSON Validation Check

```yaml
json_files:
  - .cursor/memory/decisions.json
  - .cursor/memory/patterns.json
  - .cursor/memory/corrections.json
```

---

## Enforcement Priorities

### P0 - Must Pass (Blocking)

| Rule | Enforced By | Severity |
|------|-------------|----------|
| No SDK modifications | CI + Review | 🔴 Critical |
| No hardcoded secrets | CI + Review | 🔴 Critical |
| Layer architecture | Review | 🔴 Critical |
| Compilation success | CI | 🔴 Critical |

### P1 - Should Pass

| Rule | Enforced By | Severity |
|------|-------------|----------|
| IVX prefix on public types | Review | 🟡 High |
| Correct namespace | Review | 🟡 High |
| No GC in hot paths | Review | 🟡 High |
| Singleton null checks | Review | 🟡 High |

### P2 - Nice to Have

| Rule | Enforced By | Severity |
|------|-------------|----------|
| XML documentation | Review | 🟢 Medium |
| Unit tests | Review | 🟢 Medium |
| CHANGELOG updated | Review | 🟢 Medium |

---

## Maintenance Schedule

### Weekly
- [ ] Check context freshness (FRESHNESS.md)
- [ ] Review memory files for cleanup

### Monthly
- [ ] Review and update FRESHNESS.md
- [ ] Verify all context files are current
- [ ] Review enforcement effectiveness

### Pre-Release
- [ ] Run all CI checks
- [ ] Verify no P0 violations
- [ ] Update all context files

---

## Enforcement Gaps & Mitigations

### Gap 1: No Automated Code Analysis
**Status:** Manual (code review)
**Mitigation:** PR template checklist, review guidelines
**Future:** Add Roslyn analyzers or custom linting

### Gap 1.5: Local Validation / Guardrail Scripts
**Status:** ✅ Implemented
**Mitigation:** `tools/context/validate_context.py` (runs locally and in CI)

### Gap 2: No Pre-Commit Hooks
**Status:** Not implemented
**Mitigation:** CI catches issues on PR
**Future:** Add husky or similar pre-commit hooks

### Gap 3: No Runtime Validation
**Status:** Partial (null checks, assertions)
**Mitigation:** Code review catches violations
**Future:** Debug-only runtime validators

---

## Quick Reference

| Enforcement Type | When | What |
|------------------|------|------|
| CI Validation | Every PR | Files exist, no read-only violations |
| PR Template | PR creation | Checklist completion |
| Code Review | Before merge | Quality, patterns, conventions |
| AI Self-Check | During work | Context loading, guardrails |

---

*This enforcement system ensures Context Engineering rules are followed automatically.*
