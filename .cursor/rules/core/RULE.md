# 📜 Core Rules — IntelliVerseX Unity SDK

> **Authority:** Foundation rules that apply to ALL work in this repository
> **Version:** 1.0.0
> **Last Updated:** 2026-01-13

---

## 🎯 Purpose

These rules are **always active** and apply to every task, regardless of context.

---

## 📋 Pre-Work Checklist

Before starting ANY task:

- [ ] Load `.cursor/context.md` (master context)
- [ ] Check `.cursor/NON_GOALS.md` (scope boundaries)
- [ ] Verify `.cursor/assumptions.md` (current assumptions)
- [ ] Review `AGENT.md` (project index)

---

## 🚫 Absolute Prohibitions

### Never Modify

| Zone | Reason |
|------|--------|
| `Assets/Nakama/` | Third-party SDK |
| `Assets/Photon/` | Third-party SDK |
| `Assets/Appodeal/` | Third-party SDK |
| `Assets/LevelPlay/` | Third-party SDK |
| `Assets/AppleAuth/` | Third-party SDK |
| `Assets/Plugins/Demigiant/` | Third-party SDK |
| `Library/`, `Temp/`, `Logs/` | Unity-managed |

### Never Do

| Action | Reason |
|--------|--------|
| Leave `// TODO` in code | Incomplete work |
| Use `Find()` in `Update()` | Performance |
| Use `GetComponent()` in `Update()` | Performance |
| Allocate in hot paths | GC pressure |
| Hardcode secrets | Security |
| Skip null checks | Stability |
| Commit with errors | Quality |

---

## ✅ Always Do

### Code Quality

| Action | Reason |
|--------|--------|
| Use `IVX` prefix on public types | Namespace clarity |
| Add XML documentation | API discoverability |
| Use `[SerializeField]` | Encapsulation |
| Cache component references | Performance |
| Use `?.` operator | Null safety |
| Follow naming conventions | Consistency |

### Process

| Action | Reason |
|--------|--------|
| Check scope boundaries | Prevent creep |
| Document decisions | Future reference |
| Update CHANGELOG | Version tracking |
| Verify against context | Alignment |

---

## 📐 Naming Quick Reference

| Element | Convention | Example |
|---------|------------|---------|
| Classes | `IVX` + PascalCase | `IVXIdentityManager` |
| Interfaces | `IIVX` + PascalCase | `IIVXAuthProvider` |
| Private Fields | `_camelCase` | `_isInitialized` |
| Constants | `UPPER_SNAKE` | `MAX_RETRY_COUNT` |
| Events | `On` + PascalCase | `OnAuthStateChanged` |
| Async Methods | + `Async` suffix | `SignInAsync()` |

---

## 🏗️ Architecture Rules

### Layer Boundaries

```
Consumer Game → SDK Public API → SDK Internal → Third-Party
```

### Dependency Direction

- ✅ Higher layers may depend on lower layers
- ❌ Lower layers must NOT depend on higher layers
- ❌ No circular dependencies

### Module Independence

- Each module has its own assembly definition
- Modules communicate via public APIs only
- No direct access to internal implementations

---

## 🔄 Context Lifecycle

### Load Phase

```
1. Read .cursor/context.md
2. Read .cursor/NON_GOALS.md
3. Read .cursor/architecture.md
4. Verify no conflicts
```

### Work Phase

```
1. Follow naming conventions
2. Respect boundaries
3. Document decisions
4. Update assumptions if changed
```

### Verify Phase

```
1. Check against context
2. Update CHANGELOG
3. Log decisions
4. Update AGENT.md if needed
```

---

## ⚠️ Violation Handling

If you detect a violation:

1. **STOP** — Do not proceed
2. **REPORT** — Explain the violation
3. **PROPOSE** — Suggest alternatives
4. **WAIT** — Get approval before proceeding

---

## 📝 Decision Logging

Log a decision when:

- Change affects 3+ files
- Introduces new pattern
- Changes data flow
- Has security implications
- Future self needs context

---

## 🎯 Success Criteria

A task is successful when:

- [ ] No violations of core rules
- [ ] Context alignment verified
- [ ] Documentation updated
- [ ] No compiler errors/warnings
- [ ] Naming conventions followed

---

*These rules are non-negotiable. They form the foundation of quality and consistency.*
