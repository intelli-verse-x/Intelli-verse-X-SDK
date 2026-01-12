# 🌳 Decision Tree - How to Approach Tasks

> **Purpose:** Guide AI through complex decision-making for consistent results.
> **Last Updated:** 2026-01-13

---

## Task Type Detection

```
START
  │
  ├─ Is this a bug fix?
  │   └─ YES → Go to BUG FIXING FLOW
  │
  ├─ Is this a new feature?
  │   └─ YES → Go to FEATURE FLOW
  │
  ├─ Is this a refactor?
  │   └─ YES → Go to REFACTOR FLOW
  │
  ├─ Is this documentation work?
  │   └─ YES → Go to DOCUMENTATION FLOW
  │
  ├─ Is this a configuration change?
  │   └─ YES → Go to CONFIG FLOW
  │
  └─ Is this a quick edit?
      └─ YES → Go to QUICK EDIT FLOW
```

---

## 🐛 BUG FIXING FLOW

```
1. Understand the bug
   ├─ What is the expected behavior?
   ├─ What is the actual behavior?
   └─ What are the reproduction steps?

2. Locate affected code
   └─ Search AGENT.md for relevant modules

3. Check constraints
   ├─ Is the code in a read-only zone? → STOP, report limitation
   └─ Does fix require architecture change? → Escalate

4. Implement fix
   ├─ Check ANTI_PATTERNS.md before coding
   ├─ Follow existing code style
   └─ Add error handling

5. Verify fix
   ├─ Does it compile?
   ├─ Does it fix the bug?
   └─ Does it introduce new issues?

6. Document
   └─ Update CHANGELOG if significant
```

---

## ✨ FEATURE FLOW

```
1. Understand requirements
   ├─ What is the feature?
   ├─ What module does it belong to?
   └─ What are the acceptance criteria?

2. Check scope
   ├─ Is this in scope? (Check NON_GOALS.md)
   └─ Does this require architecture decision? → Create ADR

3. Plan implementation
   ├─ Identify affected modules
   ├─ Check dependencies in architecture.md
   └─ Choose appropriate template from examples/

4. Implement
   ├─ Follow naming conventions (IVX prefix)
   ├─ Add XML documentation
   ├─ Include error handling
   └─ Add unit tests

5. Verify
   ├─ Does it compile?
   ├─ Do tests pass?
   └─ Does it meet requirements?

6. Document
   ├─ Update AGENT.md if new classes added
   └─ Update CHANGELOG
```

---

## 🔄 REFACTOR FLOW

```
1. Identify scope
   ├─ Single file → Proceed
   ├─ Multiple files → List all affected
   └─ System-wide → Create plan first

2. Check constraints
   ├─ Is behavior preservation required? (Usually YES)
   ├─ Are there tests to verify?
   └─ Is this in a read-only zone? → STOP

3. Assess risk
   ├─ LOW: Internal changes only
   ├─ MEDIUM: API changes (same module)
   └─ HIGH: Cross-module changes → Escalate

4. Implement
   ├─ Make incremental changes
   ├─ Verify after each step
   └─ NO "while I'm here" changes

5. Verify
   ├─ Behavior unchanged?
   ├─ Tests pass?
   └─ No new warnings?

6. Document
   └─ Update AGENT.md if APIs changed
```

---

## 📝 DOCUMENTATION FLOW

```
1. Identify what needs documentation
   ├─ New feature?
   ├─ API change?
   └─ Process update?

2. Choose documentation type
   ├─ Code docs → XML comments
   ├─ Architecture → ADR
   ├─ Process → AGENTS.md
   └─ Context → .cursor/*.md

3. Write documentation
   ├─ Be concise
   ├─ Include examples
   └─ Keep up to date

4. Verify
   └─ Is it accurate and complete?
```

---

## ⚙️ CONFIG FLOW

```
1. Identify configuration type
   ├─ SDK config → ScriptableObject
   ├─ Build config → ProjectSettings
   └─ Package config → package.json

2. Check constraints
   ├─ Is this a breaking change?
   └─ Does this affect consumers?

3. Implement change
   ├─ Update configuration
   ├─ Update documentation
   └─ Test with default values

4. Verify
   └─ Does SDK work with new config?
```

---

## ⚡ QUICK EDIT FLOW

```
1. Load HOT_CONTEXT.md
   └─ Check if file is in common files list

2. Make minimal change
   └─ Don't refactor surrounding code

3. Verify against ANTI_PATTERNS.md
   └─ Quick checklist

4. Done
   └─ No doc update needed for small edits
```

---

## Decision Points

### When to Escalate
- [ ] Change affects 3+ modules
- [ ] Requires new public API
- [ ] Changes architecture patterns
- [ ] Touches security-sensitive code
- [ ] Modifies serialization format

### When to Create ADR
- [ ] New module being added
- [ ] New architectural pattern
- [ ] Technology decision
- [ ] Breaking change

### When to Update AGENT.md
- [ ] New script created
- [ ] Script deleted
- [ ] Public API changed
- [ ] New module added

---

## Risk Assessment Matrix

| Change Type | Files | Risk | Action |
|-------------|-------|------|--------|
| Bug fix | 1 | 🟢 Low | Direct fix |
| Bug fix | 2-3 | 🟡 Medium | Review deps |
| Bug fix | 4+ | 🔴 High | Create plan |
| Feature | 1-2 | 🟢 Low | Use template |
| Feature | 3-5 | 🟡 Medium | Create plan |
| Feature | 6+ | 🔴 High | Full spec + ADR |
| Refactor | Any | 🟡+ | Always plan |
| Config | Any | 🟡 Medium | Test thoroughly |

---

## Pre-Implementation Checklist

Before starting ANY task:

- [ ] Loaded relevant context from `.cursor/`
- [ ] Checked `NON_GOALS.md` for scope
- [ ] Checked `AI_GUARDRAILS.md` for permissions
- [ ] Checked `ANTI_PATTERNS.md` for pitfalls
- [ ] Identified affected modules
- [ ] Assessed risk level
- [ ] Have clear acceptance criteria

---

*Follow this decision tree for consistent, high-quality results.*
