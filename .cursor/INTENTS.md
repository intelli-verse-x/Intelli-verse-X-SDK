# 🎛️ Intent Taxonomy (Closed Set)

> **Purpose:** Convert natural language requests into a small, consistent set of intents.
> **Version:** 1.0.0
> **Last Updated:** 2026-01-13
> **Status:** Active

---

## Why this exists

Without a closed set of intents, work drifts into:
- vague scope
- inconsistent verification
- accidental architecture changes

This taxonomy is the **routing layer**: request → intent → required context → required outputs.

---

## Intent Set

| Intent | Meaning | Typical Outputs | Required Verification |
|--------|---------|------------------|-----------------------|
| `docs_update` | Update docs/context | Updated `.md` | CI context validation |
| `bugfix` | Fix incorrect behavior | Minimal code change | Compile + targeted test |
| `feature_add` | Add new capability (non-breaking) | New/updated APIs | Tests + CHANGELOG |
| `refactor` | Restructure without behavior change | Internal code movement | Compile + regression check |
| `api_change` | Public API change (potentially breaking) | Updated public surface | Version plan + ADR |
| `performance` | Reduce CPU/GC/memory | Optimized code | Profiling evidence (where possible) |
| `security` | Auth/token/PII/storage changes | Hardened handling | Mandatory human review + ADR |
| `build_release` | Package/release operations | Version/tag/docs | CI + release checklist |
| `tooling` | Scripts/CI/dev tooling | New tools/workflows | Script run proof + CI passing |

---

## Intent → Context Loading Minimum

All intents must load (at minimum):

1. `.cursor/NON_GOALS.md`
2. `.cursor/AI_GUARDRAILS.md`
3. `.cursor/ANTI_PATTERNS.md` (if code is touched)
4. `.cursor/architecture.md` (if structure is touched)
5. `.cursor/naming-and-style.md` (if code is created/edited)

---

## Escalation Rules (Intent-based)

- `security` → human approval required (see `.cursor/AI_GUARDRAILS.md`)
- `api_change` → ADR required + SemVer planning
- `feature_add` touching 3+ modules → ADR recommended
- Any request touching read-only zones → stop and report (`.cursor/NON_GOALS.md`)

---

## Command Registry

See: `.cursor/commands.json`

