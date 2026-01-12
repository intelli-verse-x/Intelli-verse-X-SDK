# 📐 Architecture Decision Records (ADRs)

This folder contains Architecture Decision Records for the IntelliVerseX Unity SDK.

## What is an ADR?

An ADR documents a significant architectural decision along with its context and consequences.

## When to Create an ADR

Create an ADR when:

- [ ] Change affects 3+ systems
- [ ] Introduces new dependency
- [ ] Changes data flow
- [ ] Has security implications
- [ ] Team disagreed on approach
- [ ] Future self needs context

## ADR Template

```markdown
# ADR-NNN: Title

## Status

Proposed | Accepted | Deprecated | Superseded

## Context

What is the issue that we're seeing that is motivating this decision?

## Decision

What is the change that we're proposing and/or doing?

## Consequences

What becomes easier or more difficult to do because of this change?

## Alternatives Considered

What other options were considered and why were they rejected?
```

You can copy: `ADR-000-template.md`

## Naming Convention

```
ADR-{NNN}-{kebab-case-title}.md
```

Examples:
- `ADR-001-singleton-pattern-for-managers.md`
- `ADR-002-nakama-backend-integration.md`

## ADR Lifecycle

```
Proposed → Accepted → [Deprecated → Superseded]
              ↓
         Implemented
```

## Current ADRs

| ADR | Title | Status |
|-----|-------|--------|
| ADR-000 | Template | - |

*No ADRs yet. Create one when making significant architectural decisions.*
