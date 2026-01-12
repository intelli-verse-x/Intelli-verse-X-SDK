# 🚫 Non-Goals — What AI Should NOT Do

> **Authority:** Explicit scope boundaries to prevent AI overreach and scope creep
> **Version:** 1.0.0
> **Last Updated:** 2026-01-13

---

## 🎯 Purpose

This document defines what is **intentionally OUT OF SCOPE** for AI work in this repository. These boundaries exist to:
- Prevent scope creep
- Protect stability
- Maintain focus
- Respect ownership boundaries

---

## 🚧 Scope Boundaries

### Features Out of Scope

| Category | Non-Goal | When to Ask Human |
|----------|----------|-------------------|
| **Game Logic** | Implementing game-specific features | Never - SDK is infrastructure only |
| **UI Themes** | Creating visual themes/skins | Consumer games handle this |
| **New Modules** | Adding entirely new SDK modules | Explicit architecture decision required |
| **Platform Support** | Adding new platform support | Requires testing infrastructure |
| **Backend Changes** | Modifying server-side logic | Separate repository |

### Architecture Out of Scope

| Category | Non-Goal | When to Ask Human |
|----------|----------|-------------------|
| **Pattern Changes** | Changing singleton to DI | Explicit architecture decision |
| **New Base Classes** | Creating new inheritance hierarchies | Justified by 3+ use cases |
| **Dependency Changes** | Adding new third-party dependencies | Security/license review required |
| **Breaking Changes** | Modifying public API signatures | Major version planning |

### Third-Party Code (READ-ONLY)

| Category | Non-Goal | Reason |
|----------|----------|--------|
| **Nakama SDK** | Modifying Nakama code | Third-party, upstream maintained |
| **Photon SDK** | Modifying Photon code | Third-party, upstream maintained |
| **Appodeal SDK** | Modifying Appodeal code | Third-party, upstream maintained |
| **Apple Auth** | Modifying Apple Auth code | Third-party, upstream maintained |
| **DOTween** | Modifying DOTween code | Third-party, upstream maintained |

---

## 🛑 Default Behavior Rules

### Rule 1: Minimum Viable Change

> Implement the **minimum** that satisfies the request. No more.

```
❌ User: "Fix null check in IVXIdentityManager"
   AI: Fixes null check + refactors entire class + adds logging + updates docs

✅ User: "Fix null check in IVXIdentityManager"  
   AI: Fixes only the null check
```

### Rule 2: Ask Before Expanding

> If a task seems to require work beyond the explicit request, **ASK first**.

```
❌ User: "Add retry logic to SignInAsync"
   AI: Adds retry + adds circuit breaker + adds telemetry + refactors error handling

✅ User: "Add retry logic to SignInAsync"
   AI: "I'll add retry logic with exponential backoff. Should I also add:
        - Circuit breaker pattern?
        - Retry telemetry?
        Let me know, or I'll just add the basic retry."
```

### Rule 3: Stop at Boundaries

> When work touches a boundary (third-party, backend, architecture), **STOP and report**.

```
❌ User: "Fix the authentication bug"
   AI: Modifies Nakama server code

✅ User: "Fix the authentication bug"
   AI: "The bug appears to originate in the Nakama backend response.
        Client-side, I can add error handling, but the root fix
        requires backend changes. Should I:
        1. Add client-side error handling (workaround)
        2. Document the backend issue for the server team"
```

### Rule 4: Respect Read-Only Zones

> Never modify files in read-only zones, even if it would "fix" something.

```
❌ User: "The Nakama client has a bug"
   AI: Modifies Assets/Nakama/Runtime/ClientExtensions.cs

✅ User: "The Nakama client has a bug"
   AI: "The Nakama SDK is read-only (third-party). Options:
        1. Create a wrapper/extension in our SDK code
        2. Report upstream to Nakama team
        3. Fork the SDK (requires architecture decision)"
```

---

## 📁 Read-Only Zones

These folders are **NEVER** to be modified by AI:

| Folder | Reason |
|--------|--------|
| `Assets/Nakama/` | Third-party SDK |
| `Assets/Photon/` | Third-party SDK |
| `Assets/Appodeal/` | Third-party SDK |
| `Assets/LevelPlay/` | Third-party SDK |
| `Assets/AppleAuth/` | Third-party SDK |
| `Assets/Plugins/Demigiant/` | Third-party (DOTween) |
| `Assets/Plugins/NativeFilePicker/` | Third-party plugin |
| `Library/` | Unity-managed |
| `Temp/` | Unity-managed |
| `Logs/` | Unity-managed |
| `obj/` | Build artifacts |

---

## ⚠️ Scope Creep Examples to Avoid

| User Request | ❌ Over-Engineered Response | ✅ Correct Response |
|--------------|----------------------------|---------------------|
| "Fix typo in error message" | Refactor entire error handling system | Fix the typo only |
| "Add timeout parameter" | Create configurable timeout system with UI | Add the parameter |
| "Null check on manager" | Add defensive programming throughout SDK | Add the specific null check |
| "This method is slow" | Rewrite entire module | Profile, identify bottleneck, fix that |
| "Add logging here" | Implement comprehensive logging framework | Add logging where requested |

---

## 🔄 When to Break These Rules

Only break these rules when:

1. **User explicitly requests it**
   - "Yes, refactor the whole thing"
   - "Add comprehensive error handling"

2. **Safety requires it**
   - Security vulnerability discovered
   - Data loss risk identified
   - Critical bug affects multiple systems

3. **Compilation requires it**
   - Code won't compile without related change
   - Breaking change requires cascade updates

4. **User approves expansion**
   - "Should I also..." → "Yes"

---

## 📋 Quick Reference Card

```
Before implementing, ask yourself:

[ ] Is this exactly what the user asked for?
[ ] Am I adding anything they didn't request?
[ ] Does this touch a read-only zone?
[ ] Does this change architecture?
[ ] Does this add a new dependency?
[ ] Should I ask before proceeding?

If any answer is concerning → ASK THE USER
```

---

## 🎯 Decision Tree

```
User Request
    │
    ▼
Is it in a read-only zone?
    │
    ├─ YES → STOP. Report limitation.
    │
    ▼
Does it change architecture?
    │
    ├─ YES → STOP. Propose, don't implement.
    │
    ▼
Does it add dependencies?
    │
    ├─ YES → STOP. Requires approval.
    │
    ▼
Is scope clear and bounded?
    │
    ├─ NO → ASK for clarification.
    │
    ▼
Implement minimum viable change.
```

---

## 📝 Logging Non-Goal Encounters

When you encounter a non-goal boundary:

1. **Acknowledge** the boundary
2. **Explain** why it's out of scope
3. **Offer** alternatives within scope
4. **Document** if it's a recurring request

Example response:
```
"This request touches the Nakama SDK (read-only zone).

Why it's out of scope:
- Third-party code we don't maintain
- Changes would be lost on SDK updates

Alternatives within scope:
1. Create a wrapper in IntelliVerseX.Backend
2. Add error handling in our integration layer
3. Document as known limitation

Which approach would you prefer?"
```

---

## ✅ Non-Goals Checklist

Before starting any task:

- [ ] Verified request doesn't touch read-only zones
- [ ] Verified request doesn't require architecture changes
- [ ] Verified request doesn't add new dependencies
- [ ] Verified scope is clear and bounded
- [ ] Prepared to ask if scope expands

---

*This file is loaded for all implementation tasks. Respect these boundaries to maintain SDK stability and focus.*
