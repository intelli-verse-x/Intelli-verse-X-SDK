# 🗺️ System Map - Master Index

> **Purpose:** Single source of truth for how all context engineering files connect.
> **Last Updated:** 2026-01-13
> **Version:** 2.2.0

---

## Quick Navigation

| Need | Go To |
|------|-------|
| **🚀 Quick Start** | `.cursor/HOT_CONTEXT.md` |
| **Master Context** | `.cursor/context.md` |
| **Architecture Rules** | `.cursor/architecture.md` |
| **Scope Boundaries** | `.cursor/NON_GOALS.md` |
| **AI Permissions** | `.cursor/AI_GUARDRAILS.md` |
| **Anti-patterns** | `.cursor/ANTI_PATTERNS.md` |
| **Intent Taxonomy** | `.cursor/INTENTS.md` |
| **Decision Tree** | `.cursor/DECISION_TREE.md` |
| **Golden Paths** | `docs/GOLDEN_PATHS.md` |
| **Enforcement** | `.cursor/ENFORCEMENT.md` |
| **Naming Conventions** | `.cursor/naming-and-style.md` |
| **Assumptions** | `.cursor/assumptions.md` |
| **Context Freshness** | `.cursor/FRESHNESS.md` |
| **Project Index** | `AGENT.md` |
| **Workflows** | `AGENTS.md` |
| **UPM Transformation** | `docs/UPM_PACKAGE_TRANSFORMATION_PLAN.md` |
| **SDK Installation** | `Assets/_IntelliVerseXSDK/INSTALLATION.md` |
| **Context Validator** | `tools/context/validate_context.py` |

---

## File Hierarchy

```
┌─────────────────────────────────────────────────────────────────┐
│                         ENTRY POINTS                             │
├─────────────────────────────────────────────────────────────────┤
│  .cursorrules          │ First file AI reads, links to all      │
│  AGENT.md              │ Project intelligence index              │
│  AGENTS.md             │ Workflow guide                          │
└─────────────────────────────────────────────────────────────────┘
                                   │
                                   ▼
┌─────────────────────────────────────────────────────────────────┐
│                      .cursor/ FOLDER                             │
├─────────────────────────────────────────────────────────────────┤
│  SYSTEM_MAP.md         │ This file - master index               │
│  context.md            │ Master context (vision, principles)    │
│  architecture.md       │ System structure, dependencies         │
│  naming-and-style.md   │ Naming conventions, templates          │
│  assumptions.md        │ Explicit assumptions                   │
│  NON_GOALS.md          │ Scope boundaries                       │
│  AI_GUARDRAILS.md      │ AI permission matrix                   │
│  ANTI_PATTERNS.md      │ Code anti-patterns (16 patterns)       │
│  INTENTS.md            │ Intent taxonomy (closed set) 🆕         │
│  commands.json         │ Command registry (testable) 🆕          │
│  HOT_CONTEXT.md        │ Quick reference                        │
│  FRESHNESS.md          │ Context staleness tracking             │
│  DECISION_TREE.md      │ Task approach flowcharts 🆕            │
│  ENFORCEMENT.md        │ Enforcement mechanisms 🆕              │
├─────────────────────────────────────────────────────────────────┤
│  examples/             │ Code templates                         │
│  ├── MANAGER_TEMPLATE.cs    │ Singleton manager pattern         │
│  ├── SERVICE_TEMPLATE.cs    │ Stateless service pattern         │
│  ├── INTERFACE_TEMPLATE.cs  │ Interface pattern                 │
│  └── CONFIG_TEMPLATE.cs     │ ScriptableObject config pattern   │
├─────────────────────────────────────────────────────────────────┤
│  memory/               │ AI Learning System                     │
│  ├── decisions.json    │ Architecture decisions                 │
│  ├── patterns.json     │ Code patterns                          │
│  ├── corrections.json  │ User corrections                       │
│  └── README.md         │ Memory system docs                     │
├─────────────────────────────────────────────────────────────────┤
│  rules/                │ Rule files                             │
│  ├── core/             │ Always active - foundational           │
│  │   └── RULE.md       │ Core rules                             │
│  └── upm/              │ UPM package rules                      │
│      └── RULE.md       │ UPM-specific rules                     │
└─────────────────────────────────────────────────────────────────┘
                                    │
                                    ▼
┌─────────────────────────────────────────────────────────────────┐
│                        docs/ FOLDER                              │
├─────────────────────────────────────────────────────────────────┤
│  GOLDEN_PATHS.md       │ Happy-path workflows 🆕                │
│  CONTEXT_ENGINEERING_REVIEW.md │ System review & rating 🆕      │
├─────────────────────────────────────────────────────────────────┤
│  context/              │ Context documentation                  │
│  └── README.md         │ Context docs index                     │
├─────────────────────────────────────────────────────────────────┤
│  architecture/         │ Architecture documentation             │
│  └── adr/              │ Architecture Decision Records          │
│      └── README.md     │ ADR index and template                 │
├─────────────────────────────────────────────────────────────────┤
│  tracking/             │ Tracking documentation                 │
│  └── README.md         │ Tracking docs index                    │
└─────────────────────────────────────────────────────────────────┘
                                    │
                                    ▼
┌─────────────────────────────────────────────────────────────────┐
│                      .github/ FOLDER 🆕                          │
├─────────────────────────────────────────────────────────────────┤
│  workflows/            │ GitHub Actions                         │
│  ├── context-validation.yml │ Context validation on PR          │
│  ├── package-validation.yml │ UPM package structure validation  │
│  └── release-tag.yml   │ Release tag workflow (for UPM)         │
├─────────────────────────────────────────────────────────────────┤
│  pull_request_template.md │ PR checklist template               │
│  CODEOWNERS            │ Code ownership rules                   │
└─────────────────────────────────────────────────────────────────┘
                                   │
                                   ▼
┌─────────────────────────────────────────────────────────────────┐
│                      tools/ FOLDER 🆕                            │
├─────────────────────────────────────────────────────────────────┤
│  scripts/              │ Utility scripts                        │
│  ├── reorganize_for_upm.py  │ Python UPM reorganization script  │
│  └── reorganize_for_upm.ps1 │ PowerShell UPM reorganization     │
│  context/              │ Context validator (CI + local) 🆕       │
│  ├── validate_context.py │ Guardrails + schema checks           │
│  ├── README.md         │ How to run + baselines                  │
│  └── baselines/        │ Baselines for existing violations       │
└─────────────────────────────────────────────────────────────────┘
                                   │
                                   ▼
┌─────────────────────────────────────────────────────────────────┐
│                 Assets/_IntelliVerseXSDK/ 🆕                     │
├─────────────────────────────────────────────────────────────────┤
│  package.json          │ UPM package manifest                   │
│  README.md             │ SDK documentation                      │
│  CHANGELOG.md          │ Version history                        │
│  LICENSE               │ MIT License                            │
│  INSTALLATION.md       │ Installation guide                     │
│  Samples~/             │ Importable samples (hidden)            │
│  Tests~/               │ Unit tests (hidden)                    │
│  Documentation/        │ SDK documentation                      │
└─────────────────────────────────────────────────────────────────┘
```

---

## Context Loading Order

```
1. .cursorrules            → Always loaded first
2. NON_GOALS.md            → Check scope boundaries
3. AI_GUARDRAILS.md        → Check permissions
4. DECISION_TREE.md        → Determine task type 🆕
5. rules/core/RULE.md      → Always active
6. HOT_CONTEXT.md          → For quick tasks
7. ANTI_PATTERNS.md        → Before code generation
8. architecture.md         → For structural decisions
9. naming-and-style.md     → For code generation
10. examples/              → When creating new scripts
11. GOLDEN_PATHS.md        → For workflow guidance 🆕
12. AGENT.md               → When deep context needed
```

---

## Cross-Reference Matrix

### Which files reference which:

| File | References |
|------|------------|
| `.cursorrules` | context.md, NON_GOALS.md, AI_GUARDRAILS.md, HOT_CONTEXT.md, ANTI_PATTERNS.md, AGENT.md |
| `context.md` | architecture.md, assumptions.md |
| `architecture.md` | naming-and-style.md |
| `AI_GUARDRAILS.md` | NON_GOALS.md, ANTI_PATTERNS.md |
| `DECISION_TREE.md` | NON_GOALS.md, AI_GUARDRAILS.md, ANTI_PATTERNS.md, AGENT.md |
| `GOLDEN_PATHS.md` | NON_GOALS.md, AI_GUARDRAILS.md, architecture.md, ANTI_PATTERNS.md |
| `ENFORCEMENT.md` | AI_GUARDRAILS.md, NON_GOALS.md, GitHub workflows |
| `HOT_CONTEXT.md` | AGENT.md, examples/ |
| `AGENT.md` | All .cursor/ files |
| `AGENTS.md` | AGENT.md, .cursor/ files |

---

## Validation Checklist

### Files That Must Exist

#### Entry Points
- [x] `.cursorrules`
- [x] `AGENT.md`
- [x] `AGENTS.md`

#### Core Context
- [x] `.cursor/context.md`
- [x] `.cursor/architecture.md`
- [x] `.cursor/naming-and-style.md`
- [x] `.cursor/assumptions.md`
- [x] `.cursor/NON_GOALS.md`
- [x] `.cursor/AI_GUARDRAILS.md`
- [x] `.cursor/ANTI_PATTERNS.md`
- [x] `.cursor/HOT_CONTEXT.md`
- [x] `.cursor/FRESHNESS.md`
- [x] `.cursor/SYSTEM_MAP.md`
- [x] `.cursor/DECISION_TREE.md` 🆕
- [x] `.cursor/ENFORCEMENT.md` 🆕

#### Rules
- [x] `.cursor/rules/core/RULE.md`
- [x] `.cursor/rules/upm/RULE.md`

#### Memory
- [x] `.cursor/memory/decisions.json`
- [x] `.cursor/memory/patterns.json`
- [x] `.cursor/memory/corrections.json`
- [x] `.cursor/memory/README.md`

#### Examples
- [x] `.cursor/examples/MANAGER_TEMPLATE.cs`
- [x] `.cursor/examples/SERVICE_TEMPLATE.cs`
- [x] `.cursor/examples/INTERFACE_TEMPLATE.cs`
- [x] `.cursor/examples/CONFIG_TEMPLATE.cs`

#### Documentation
- [x] `docs/context/README.md`
- [x] `docs/architecture/adr/README.md`
- [x] `docs/tracking/README.md`
- [x] `docs/GOLDEN_PATHS.md` 🆕
- [x] `docs/CONTEXT_ENGINEERING_REVIEW.md` 🆕
- [x] `docs/UPM_PACKAGE_TRANSFORMATION_PLAN.md` 🆕

#### GitHub (CI/CD) 🆕
- [x] `.github/workflows/context-validation.yml`
- [x] `.github/workflows/package-validation.yml`
- [x] `.github/workflows/release-tag.yml`
- [x] `.github/pull_request_template.md`
- [x] `.github/CODEOWNERS`

### Folders That Must Exist
- [x] `.cursor/`
- [x] `.cursor/rules/`
- [x] `.cursor/rules/core/`
- [x] `.cursor/rules/upm/`
- [x] `.cursor/memory/`
- [x] `.cursor/examples/`
- [x] `docs/`
- [x] `docs/context/`
- [x] `docs/architecture/`
- [x] `docs/architecture/adr/`
- [x] `docs/tracking/`
- [x] `.github/` 🆕
- [x] `.github/workflows/` 🆕
- [x] `tools/scripts/` 🆕

#### SDK Package Files 🆕
- [x] `Assets/_IntelliVerseXSDK/package.json`
- [x] `Assets/_IntelliVerseXSDK/README.md`
- [x] `Assets/_IntelliVerseXSDK/CHANGELOG.md`
- [x] `Assets/_IntelliVerseXSDK/LICENSE`
- [x] `Assets/_IntelliVerseXSDK/INSTALLATION.md`
- [x] `Assets/_IntelliVerseXSDK/Samples~/`
- [x] `Assets/_IntelliVerseXSDK/Tests~/`

---

## Troubleshooting

| Problem | Check | Fix |
|---------|-------|-----|
| Wrong context loaded | SYSTEM_MAP.md | Update loading order |
| Outdated info | FRESHNESS.md | Verify and update |
| Missing file | Validation checklist | Create from template |
| Broken cross-ref | Cross-reference matrix | Update references |
| CI failing | .github/workflows/ | Check workflow logs |
| PR blocked | pull_request_template.md | Complete checklist |

---

## Version History

| Version | Date | Changes |
|---------|------|---------|
| 2.1.0 | 2026-01-13 | Added UPM package setup, Samples~, Tests~, Installation guide, reorganization scripts |
| 2.0.0 | 2026-01-13 | Added GitHub Actions, DECISION_TREE, ENFORCEMENT, GOLDEN_PATHS |
| 1.0.0 | 2026-01-13 | Initial creation with full context engineering system |

---

*This file is the master index. Update it when adding new context files.*
