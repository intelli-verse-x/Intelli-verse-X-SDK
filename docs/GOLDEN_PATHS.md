# 🌟 Golden Paths - How to Work

> **Purpose:** Minimal, happy-path workflows for common tasks
> **Version:** 1.0.0
> **Last Updated:** 2026-01-13

---

## Purpose

This document provides **minimal, validated workflows** for the most common tasks in the IntelliVerseX Unity SDK.

---

## Golden Path 1: Add a New Feature to the SDK

### Example
"Add a new analytics event for tracking user sign-ins"

### Workflow

#### Step 1: Check Scope
```
Check NON_GOALS.md:
- Is this in scope? ✅ Yes (SDK feature)
- Any conflicts? ❌ No

Check architecture.md:
- Which module? Analytics
- Dependencies? Core
```

#### Step 2: Plan
```
Affected files:
- IVXAnalyticsManager.cs (add method)
- IVXAnalyticsEvents.cs (add event type)

Template to use:
- Follow existing patterns in Analytics module
```

#### Step 3: Implement
```
Constraints applied:
- IVX prefix on public types ✅
- XML documentation ✅
- Null-check singletons ✅
- Error handling ✅

Code:
/// <summary>
/// Tracks a user sign-in event.
/// </summary>
/// <param name="provider">The authentication provider used.</param>
public void TrackSignIn(string provider)
{
    if (!_isInitialized)
    {
        LogWarning("Analytics not initialized");
        return;
    }
    
    try
    {
        TrackEvent(IVXAnalyticsEvents.UserSignIn, new Dictionary<string, object>
        {
            { "provider", provider }
        });
    }
    catch (Exception ex)
    {
        LogError($"Failed to track sign-in: {ex.Message}");
    }
}
```

#### Step 4: Verify
```
- Compiles ✅
- No console errors ✅
- Manual test ✅
```

#### Step 5: Document
```
- Update CHANGELOG.md ✅
- Update AGENT.md if new class ✅
```

**Total Time:** ~15 minutes
**Files Changed:** 1-2

---

## Golden Path 2: Fix a Bug

### Example
"Fix: NullReferenceException in IVXIdentityManager.SignInAsync when config is null"

### Workflow

#### Step 1: Understand the Bug
```
Expected: Graceful error handling
Actual: NullReferenceException crash
Location: IVXIdentityManager.SignInAsync
```

#### Step 2: Check Constraints
```
Check AI_GUARDRAILS.md:
- Is this in a read-only zone? ❌ No
- Is this HIGH risk? ❌ No (internal fix)
```

#### Step 3: Implement Fix
```
Before:
public async Task<IVXAuthResult> SignInAsync(IIVXAuthProvider provider)
{
    var result = await provider.AuthenticateAsync();
    // ...
}

After:
public async Task<IVXAuthResult> SignInAsync(IIVXAuthProvider provider)
{
    if (provider == null)
    {
        LogError("Provider cannot be null");
        return IVXAuthResult.Failure("Provider is null", IVXAuthErrorCode.InvalidArgument);
    }
    
    if (!_isInitialized)
    {
        LogError("Manager not initialized");
        return IVXAuthResult.Failure("Not initialized", IVXAuthErrorCode.NotInitialized);
    }
    
    try
    {
        var result = await provider.AuthenticateAsync();
        // ...
    }
    catch (Exception ex)
    {
        LogError($"Sign-in failed: {ex.Message}");
        return IVXAuthResult.Failure(ex.Message, IVXAuthErrorCode.Unknown);
    }
}
```

#### Step 4: Verify
```
- Compiles ✅
- No crash with null config ✅
- Error logged properly ✅
```

#### Step 5: Document
```
- Update CHANGELOG.md ✅
```

**Total Time:** ~10 minutes
**Files Changed:** 1

---

## Golden Path 3: Add a New Module

### Example
"Add a new Achievements module to the SDK"

### Workflow

#### Step 1: Check Scope & Get Approval
```
Check NON_GOALS.md:
- Is new module in scope? → Requires approval

Check architecture.md:
- Where does it fit? After Leaderboard
- Dependencies? Core, Backend
```

#### Step 2: Create ADR (Required for new modules)
```
Create: docs/architecture/adr/ADR-XXX-achievements-module.md

Contents:
- Why we need this module
- How it fits in architecture
- Dependencies
- Public API design
```

#### Step 3: Create Module Structure
```
Assets/_IntelliVerseXSDK/Achievements/
├── IntelliVerseX.Achievements.asmdef
├── IVXAchievementsManager.cs
├── IVXAchievement.cs
├── IVXAchievementConfig.cs
└── README.md
```

#### Step 4: Implement Using Templates
```
Use: .cursor/examples/MANAGER_TEMPLATE.cs
Use: .cursor/examples/CONFIG_TEMPLATE.cs

Follow:
- IVX prefix ✅
- Namespace: IntelliVerseX.Achievements ✅
- XML documentation ✅
- Singleton pattern ✅
```

#### Step 5: Update Documentation
```
- Update architecture.md (add module) ✅
- Update AGENT.md (add to module list) ✅
- Update CHANGELOG.md ✅
```

**Total Time:** ~2-4 hours
**Files Changed:** 5+

---

## Golden Path 4: Safe Refactor

### Example
"Extract authentication logic from IVXIdentityManager into IVXAuthService"

### Workflow

#### Step 1: Check Scope
```
Check REFACTOR_TIMING.md (if exists):
- Should I refactor now? Yes (explicit request)
- Scope: Extract to service ✅
- Behavior preservation: Required ✅
```

#### Step 2: Plan Refactor
```
Scope:
- Extract auth methods to IVXAuthService
- Update IVXIdentityManager to use service
- NO other changes ("while I'm here" forbidden)
```

#### Step 3: Implement
```
1. Create IVXAuthService.cs (using SERVICE_TEMPLATE.cs)
2. Move authentication logic
3. Update IVXIdentityManager to delegate
4. Verify behavior unchanged
```

#### Step 4: Verify No Scope Creep
```
Check:
- Only expected files changed? ✅
- No "while I'm here" changes? ✅
- Behavior unchanged? ✅
```

#### Step 5: Document
```
- Update AGENT.md (new class) ✅
- Update CHANGELOG.md ✅
```

**Total Time:** ~30 minutes
**Files Changed:** 2-3

---

## Quick Reference Card

```
┌─────────────────────────────────────────────────────────┐
│ GOLDEN PATH QUICK REFERENCE                             │
├─────────────────────────────────────────────────────────┤
│ 1. Check NON_GOALS.md for scope                         │
│ 2. Check AI_GUARDRAILS.md for permissions               │
│ 3. Check architecture.md for module boundaries          │
│ 4. Check ANTI_PATTERNS.md before coding                 │
│ 5. Use templates from .cursor/examples/                 │
│ 6. Follow naming conventions (IVX prefix)               │
│ 7. Add XML documentation                                │
│ 8. Update CHANGELOG.md                                  │
│ 9. Update AGENT.md if new classes added                 │
└─────────────────────────────────────────────────────────┘
```

---

## Context Refresh Triggers

Update context artifacts when:

| Trigger | Update |
|---------|--------|
| New module added | architecture.md, AGENT.md |
| New class added | AGENT.md |
| API changed | AGENT.md, CHANGELOG.md |
| Pattern changed | patterns.json |
| Assumption changed | assumptions.md |
| New anti-pattern found | ANTI_PATTERNS.md |

---

*Follow these golden paths for consistent, validated workflows.*
