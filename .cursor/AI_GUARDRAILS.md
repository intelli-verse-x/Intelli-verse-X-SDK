# 🛡️ AI Guardrails - Permission & Responsibility Boundaries

> **Authority:** Defines what AI agents may and may not do in this project
> **Version:** 1.0.0
> **Last Updated:** 2026-01-13
> **Enforcement:** All AI interactions

---

## Permission Matrix

### Code Changes

| Action | Permission | Condition | Escalation |
|--------|------------|-----------|------------|
| Add new method | ✅ Allowed | Follows patterns | - |
| Add new class | ✅ Allowed | Single responsibility, IVX prefix | - |
| Add new file | ✅ Allowed | Correct location/namespace | - |
| Modify existing method | ✅ Allowed | Maintains contract | - |
| Delete method | ⚠️ Conditional | No external callers | Document why |
| Delete class | ⚠️ Conditional | Unused verified | Human approval |
| Delete file | ⚠️ Conditional | Unused verified | Human approval |
| Rename public API | ⚠️ Conditional | Update all usages | Human approval |

### Architecture Changes

| Action | Permission | Condition | Escalation |
|--------|------------|-----------|------------|
| Add new Manager | ⚠️ Conditional | Clear responsibility | ADR recommended |
| Change singleton pattern | ❌ Prohibited | - | Human decision |
| New base class | ⚠️ Conditional | 3+ concrete uses | ADR required |
| New namespace | ⚠️ Conditional | Justified grouping | Human approval |
| Layer violation | ❌ Prohibited | - | Never |
| Change data flow | ⚠️ Conditional | Documented | ADR required |
| New module | ⚠️ Conditional | Architecture decision | ADR required |

### SDK & Dependencies

| Action | Permission | Condition | Escalation |
|--------|------------|-----------|------------|
| Modify Nakama code | ❌ Prohibited | - | Never |
| Modify Photon code | ❌ Prohibited | - | Never |
| Modify Appodeal code | ❌ Prohibited | - | Never |
| Modify AppleAuth code | ❌ Prohibited | - | Never |
| Modify DOTween code | ❌ Prohibited | - | Never |
| Add new dependency | ⚠️ Conditional | Justified value | Human approval |
| Update dependency | ⚠️ Conditional | Breaking changes reviewed | Human approval |

### Security

| Action | Permission | Condition | Escalation |
|--------|------------|-----------|------------|
| Handle credentials | ⚠️ Flag | Must use secure storage | Mandatory review |
| API endpoint changes | ⚠️ Flag | Security implications | Mandatory review |
| Auth flow changes | ❌ Prohibited | - | Human only |
| Encryption changes | ❌ Prohibited | - | Human only |
| PII handling | ⚠️ Flag | Must mask in logs | Mandatory review |

### Documentation

| Action | Permission | Condition | Escalation |
|--------|------------|-----------|------------|
| Update code comments | ✅ Allowed | Accurate | - |
| Update context files | ✅ Allowed | Factual | - |
| Create ADR draft | ✅ Allowed | Significant decision | Human finalizes |
| Update CHANGELOG | ✅ Allowed | Accurate | - |
| Update AGENT.md | ✅ Allowed | New/changed scripts | - |
| Delete documentation | ⚠️ Conditional | Outdated verified | Human approval |

---

## Read-Only Zones

These locations are **NEVER** modified by AI:

```
Assets/Nakama/           # Third-party SDK
Assets/Photon/           # Third-party SDK
Assets/Appodeal/         # Third-party SDK
Assets/LevelPlay/        # Third-party SDK
Assets/AppleAuth/        # Third-party SDK
Assets/AppleAuthSample/  # Third-party sample
Assets/Plugins/Demigiant/ # Third-party (DOTween)
Assets/Plugins/NativeFilePicker/ # Third-party plugin
Assets/Sych/             # Third-party
Library/                 # Unity managed
Temp/                    # Unity managed
Logs/                    # Unity managed
UserSettings/            # User preferences
ProjectSettings/         # Unity settings (human only)
```

---

## Behavioral Rules

### Always Do (ENFORCED)

```yaml
before_any_change:
  - Check NON_GOALS.md for scope boundaries
  - Load relevant context from .cursor/
  - Check ANTI_PATTERNS.md before code generation
  - Verify change aligns with architecture.md

during_implementation:
  - Follow namespace conventions (IntelliVerseX.*)
  - Use IVX prefix on all public types
  - Use [SerializeField] private, not public
  - Null-check all singleton access
  - Include error handling with context
  - Add XML docs to public methods

after_completion:
  - List all files changed
  - Provide verification steps
  - Suggest next action
  - Update AGENT.md if scripts added
```

### Never Do

```yaml
prohibited_actions:
  - Modify read-only zones
  - Leave // TODO placeholders in production code
  - Use Find()/GetComponent() in Update()
  - Hardcode secrets or credentials
  - Skip null checks on singletons
  - Create layer violations
  - Expose third-party types in public API
  - Create circular dependencies
  - Assume scope beyond explicit request
  - Continue after 3 failed attempts without asking
```

### When Uncertain

```yaml
uncertainty_protocol:
  level_1_minor:
    - State assumption
    - Proceed with minimal change
    - Document assumption in response
    
  level_2_moderate:
    - List options
    - Recommend one
    - Ask for confirmation before proceeding
    
  level_3_significant:
    - Stop
    - Explain uncertainty
    - Request clarification
    - Do NOT proceed without answer
```

---

## Escalation Triggers

### Immediate Human Review Required

1. **Security implications** - Any change touching auth, tokens, credentials
2. **Breaking changes** - Public API modifications
3. **Architecture changes** - New patterns, new layers, new systems
4. **Data loss risk** - Delete operations, data migrations
5. **External integrations** - API endpoints, SDK usage
6. **Performance critical** - Hot path changes, memory patterns

### ADR Required

1. Changes affecting 3+ systems
2. New architectural pattern introduction
3. Technology decisions (new libraries, tools)
4. Deprecation of existing systems
5. Security model changes
6. New SDK module

### Log Required (decisions.json)

1. All escalation-level decisions
2. Approach choices when alternatives exist
3. Workarounds for limitations
4. Deferred decisions with rationale

---

## Scope Enforcement

### Minimum Viable Change Rule

```
Given request: "Fix null check in IVXIdentityManager"

❌ WRONG:
- Fix null check
- Refactor entire class
- Update related managers
- Add logging system

✅ RIGHT:
- Fix null check only
- Offer to do more if related issues found
```

### Scope Expansion Protocol

When noticing related issues:

```yaml
1. Complete requested task first
2. Note related issue in response
3. Ask: "I also noticed X. Should I address that?"
4. Wait for confirmation before expanding
```

### Distraction Handling

```yaml
noticed_bug:
  action: Note in response
  then: Continue current task

noticed_tech_debt:
  action: Note in response
  then: Continue current task

noticed_improvement:
  action: Note in response
  then: Continue current task
  
new_request_during_task:
  action: Acknowledge
  then: "I'll finish current task first, then address that"
```

---

## Verification Requirements

### Before Declaring Done

- [ ] Code compiles (no errors)
- [ ] No new console errors
- [ ] Follows project patterns
- [ ] No layer violations
- [ ] No forbidden actions
- [ ] Scope matches request (no over-engineering)
- [ ] IVX prefix on public types
- [ ] XML documentation on public APIs

### Output Format

Every response must end with:

```
## Summary
- **Files Changed:** [list]
- **Verification:** [how to verify]
- **Next Action:** [suggested next step]
```

---

## Permission Symbols Reference

| Symbol | Meaning |
|--------|---------|
| ✅ | Allowed - proceed without asking |
| ⚠️ | Conditional - check conditions, may need approval |
| ❌ | Prohibited - never do, even if asked |

---

## High Risk Areas (Automatic HIGH Risk Classification)

Changes to these areas require explicit approval + ADR:

1. **Public API** - Manager public methods, events (breaking changes)
2. **Serialization** - `[Serializable]` attributes, ScriptableObject fields
3. **Configuration** - SDK configuration, feature flags
4. **Authentication** - Auth providers, token handling
5. **Networking** - Nakama calls, API endpoints
6. **Monetization** - Ad integration, IAP
7. **Data Storage** - Save/load, PlayerPrefs, cloud storage
8. **Package Structure** - Assembly definitions, package.json

---

*This document is referenced by `.cursor/rules/core/RULE.md` and applies to all AI interactions.*
