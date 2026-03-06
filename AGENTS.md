# 🤖 AGENTS.md — AI Workflow Guide

**Project:** IntelliVerseX Unity SDK
**Last Updated:** 2026-01-13
**Version:** 1.0.0

---

## Quick Start

This document defines how AI agents (and humans) work in this repository.

### Key Documents

| Document | Purpose |
|----------|---------|
| `AGENT.md` | **Project Intelligence Index** (quick reference) |
| `AGENTS.md` | This file — workflow commands and working agreement |
| `.cursorrules` | **Primary entrypoint** and highest authority |
| `.cursor/NON_GOALS.md` | Scope boundaries (what NOT to do) |
| `.cursor/AI_GUARDRAILS.md` | AI permissions + escalation triggers |
| `.cursor/HOT_CONTEXT.md` | Quick reference (common tasks/files) |
| `.cursor/ANTI_PATTERNS.md` | What NOT to code (pitfalls) |
| `.cursor/context.md` | Master context (vision, principles) |
| `.cursor/architecture.md` | System structure rules |
| `.cursor/naming-and-style.md` | Naming conventions |
| `.cursor/assumptions.md` | Explicit assumptions |
| `.cursor/SYSTEM_MAP.md` | Master index (where everything lives) |

---

## 🏛️ Context Authority System

### Context Loading Order

**Before ANY task, load context in this order:**

1. `.cursor/NON_GOALS.md` — Scope boundaries (what NOT to do)
2. `.cursor/AI_GUARDRAILS.md` — AI permissions + escalation triggers
3. `.cursor/HOT_CONTEXT.md` — Quick reference
4. `.cursor/ANTI_PATTERNS.md` — What NOT to code
5. `.cursor/context.md` — Master context (vision, principles)
6. `.cursor/architecture.md` — System structure rules
7. `.cursor/naming-and-style.md` — Naming conventions
8. `.cursor/assumptions.md` — Current assumptions

### Context Lifecycle

```
┌─────────────────────────────────────────────────────────────┐
│                     BEFORE WORK                              │
├─────────────────────────────────────────────────────────────┤
│ 1. Load context from .cursor/*.md                           │
│ 2. Check for conflicts with existing decisions              │
│ 3. Verify assumptions are still valid                       │
│ 4. Identify scope boundaries                                │
└─────────────────────────────────────────────────────────────┘
                              │
                              ▼
┌─────────────────────────────────────────────────────────────┐
│                     DURING WORK                              │
├─────────────────────────────────────────────────────────────┤
│ 1. Follow naming conventions                                │
│ 2. Respect layer boundaries                                 │
│ 3. Document significant decisions                           │
│ 4. Update assumptions if changed                            │
└─────────────────────────────────────────────────────────────┘
                              │
                              ▼
┌─────────────────────────────────────────────────────────────┐
│                     AFTER WORK                               │
├─────────────────────────────────────────────────────────────┤
│ 1. Verify against context                                   │
│ 2. Update CHANGELOG if applicable                           │
│ 3. Update assumptions if new ones made                      │
│ 4. Log decisions if significant                             │
└─────────────────────────────────────────────────────────────┘
```

---

## 📋 Typical Workflows

### Feature Development

```
1. Load context (.cursor/*.md)
2. Verify feature aligns with architecture
3. Check NON_GOALS.md for scope boundaries
4. Implement with naming conventions
5. Add XML documentation
6. Update AGENT.md if new classes added
7. Update CHANGELOG.md
```

### Bug Fixing

```
1. Load context (.cursor/*.md)
2. Identify affected module
3. Check if fix touches read-only zones
4. Implement minimum viable fix
5. Verify no architecture violations
6. Update CHANGELOG.md
```

### Refactoring

```
1. Load context (.cursor/*.md)
2. Verify refactor is within scope
3. Check dependencies in architecture.md
4. Implement changes
5. Verify no breaking changes
6. Update documentation
```

### Adding New Module

```
1. Load context (.cursor/*.md)
2. Propose module in architecture.md
3. Get approval (architecture decision)
4. Create folder structure
5. Create assembly definition
6. Implement with conventions
7. Update AGENT.md
8. Update CHANGELOG.md
```

---

## 🚫 Forbidden Actions

> See `.cursor/NON_GOALS.md` for complete list with explanations.

**Key Prohibitions:**

- ❌ Modify third-party SDKs (`Assets/Nakama/`, `Assets/Photon/`, etc.)
- ❌ Touch Unity folders: `Library/`, `Temp/`, `Logs/`
- ❌ Leave `// TODO` or `// FIXME` in production code
- ❌ Use `Find()`/`GetComponent()` in `Update()`
- ❌ Commit with compiler errors or warnings
- ❌ Add dependencies without approval
- ❌ Change public API without version planning

---

## 🎯 Behavior Rules

### Rule 1: Minimum Viable Change

> Implement the **minimum** that satisfies the request. No more.

### Rule 2: Ask Before Expanding

> If scope seems larger than requested, **ASK first**.

### Rule 3: Stop at Boundaries

> When work touches read-only zones or architecture, **STOP and report**.

### Rule 4: Document Decisions

> Significant decisions must be documented.

### Rule 5: Respect Context

> You are governed by the context you create. Do not override it casually.

---

## Repository Structure

```
Intelli-verse-X-Unity-SDK/
|-- .cursor/                    # Context authority system
|-- Assets/                     # Unity assets
|   +-- _IntelliVerseXSDK/      # Unity SDK (UPM Package)
|-- SDKs/                       # Cross-platform SDKs
|   |-- unreal/                 # Unreal Engine 5 Plugin
|   |-- godot/                  # Godot 4 Addon
|   |-- defold/                 # Defold Library Module
|   |-- cocos2dx/               # Cocos2d-x / CMake
|   |-- javascript/             # npm / TypeScript
|   |-- cpp/                    # Native C++ / CMake
|   +-- java/                   # Java / Gradle / Android
|-- docs/                       # MkDocs documentation
|   +-- platforms/              # Per-platform docs
|-- .github/workflows/          # CI/CD
|-- tools/                      # Dev utilities
|-- AGENT.md                    # Project intelligence
|-- AGENTS.md                   # This file
+-- README.md                   # Repository readme
```

---

## ✅ Definition of Done

A task is complete when:

- [ ] Code follows project standards (see `.cursor/naming-and-style.md`)
- [ ] No compiler errors or warnings
- [ ] XML documentation on public APIs
- [ ] No forbidden patterns used
- [ ] Architecture boundaries respected
- [ ] CHANGELOG.md updated (if applicable)
- [ ] AGENT.md updated (if new classes added)

---

## Project Quick Facts

| Property | Value |
|----------|-------|
| Unity Version | 6000.2.8f1 |
| Package Name | com.intelliversex.sdk |
| Root Namespace | `IntelliVerseX` |
| Target Platforms | Unity, Unreal, Godot, Defold, Cocos2d-x, JS, C++, Java |
| Min Unity Version | 2021.3 LTS |
| SDK Version | 5.1.0 |

---

## 📐 Coding Standards (Quick Reference)

### Naming

| Element | Convention | Example |
|---------|------------|---------|
| Classes | `IVX` + PascalCase | `IVXIdentityManager` |
| Interfaces | `IIVX` + PascalCase | `IIVXAuthProvider` |
| Private Fields | `_camelCase` | `_isInitialized` |
| Constants | `UPPER_SNAKE` | `MAX_RETRY_COUNT` |
| Events | `On` + PascalCase | `OnAuthStateChanged` |
| Async Methods | + `Async` suffix | `SignInAsync()` |

### Script Template

```csharp
using System;
using UnityEngine;

namespace IntelliVerseX.[Module]
{
    /// <summary>
    /// Brief description.
    /// </summary>
    public class IVXClassName : MonoBehaviour
    {
        #region Constants
        private const int MAX_VALUE = 100;
        #endregion

        #region Serialized Fields
        [SerializeField] private float _value;
        #endregion

        #region Private Fields
        private bool _isInitialized;
        #endregion

        #region Properties
        public bool IsInitialized => _isInitialized;
        #endregion

        #region Unity Lifecycle
        private void Awake() { }
        #endregion

        #region Public Methods
        public void Initialize() { }
        #endregion

        #region Private Methods
        private void ValidateState() { }
        #endregion
    }
}
```

---

## 🔄 Self-Enforced Guardrails

### Continuous Checks

During every task, verify:

- [ ] No feature without architecture alignment
- [ ] No public API without documentation
- [ ] No runtime code in Editor assemblies
- [ ] No hidden dependencies
- [ ] No breaking changes without version notes

### Self-Evaluation

After major changes, ask:

- [ ] Did I violate my own context?
- [ ] Did I add hidden complexity?
- [ ] Did I make future changes harder?
- [ ] Did I document my decisions?

If yes to any concern → refactor immediately, update context docs.

---

## 📊 Decision Logging

### When to Log Decisions

Log a decision when:
- Change affects 3+ files
- Introduces new pattern
- Changes data flow
- Has security implications
- Future self needs context

### Decision Format

```json
{
  "id": "DEC-YYYYMMDD-NNN",
  "date": "2026-01-13",
  "category": "architecture|security|api|workflow",
  "decision": "What was decided",
  "rationale": "Why this decision was made",
  "alternatives": ["What else was considered"],
  "outcome": "successful|failed|pending"
}
```

### Decision Storage

- Quick decisions: `.cursor/memory/decisions.json`
- Architecture decisions: `docs/architecture/adr/ADR-NNN-title.md`

---

## 💡 Tips

### Development

1. **Always load context first** — Read `.cursor/*.md` before starting
2. **Check AGENT.md** — Find module locations and dependencies
3. **Verify scope** — Check `NON_GOALS.md` before expanding
4. **Use templates** — Follow naming and style conventions

### Quality

5. **Document public APIs** — XML docs on all public members
6. **No hot path allocations** — Zero GC in Update loops
7. **Null safety** — Use `?.` operator, check singletons

### Release

8. **Update CHANGELOG** — Every change needs a log entry
9. **Bump version** — Follow SemVer in package.json
10. **Update AGENT.md** — Keep project index current

---

## 🚀 Success Definition

You have succeeded when:

- ✅ The repository enforces consistency without reminders
- ✅ Future changes naturally follow existing patterns
- ✅ Context documents explain why the system is the way it is
- ✅ Another senior Unity engineer could onboard without confusion

---

## 📜 Absolute Rule

> **You are governed by the context you create.**
> **You do not override it casually.**
> **You evolve it deliberately.**

---

*For project intelligence, see `AGENT.md`*
