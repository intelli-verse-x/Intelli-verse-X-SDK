# 📅 Context Freshness Tracker

> **Purpose:** Track when context files were last verified and flag stale information.
> **Last Updated:** 2026-01-13

---

## Freshness Status

### Core Context Files
| File | Last Verified | Verified By | Status | Next Review |
|------|---------------|-------------|--------|-------------|
| `context.md` | 2026-01-13 | Initial Creation | 🟢 Fresh | 2026-02-13 |
| `architecture.md` | 2026-01-13 | Initial Creation | 🟢 Fresh | 2026-02-13 |
| `naming-and-style.md` | 2026-01-13 | Initial Creation | 🟢 Fresh | 2026-02-13 |
| `assumptions.md` | 2026-01-13 | Initial Creation | 🟢 Fresh | 2026-02-13 |
| `NON_GOALS.md` | 2026-01-13 | Initial Creation | 🟢 Fresh | 2026-02-13 |
| `.cursorrules` | 2026-01-13 | Initial Creation | 🟢 Fresh | 2026-02-13 |

### Rules Files
| File | Last Verified | Verified By | Status | Next Review |
|------|---------------|-------------|--------|-------------|
| `rules/core/RULE.md` | 2026-01-13 | Initial Creation | 🟢 Fresh | 2026-02-13 |
| `rules/upm/RULE.md` | 2026-01-13 | Initial Creation | 🟢 Fresh | 2026-02-13 |

### Memory Files
| File | Last Verified | Verified By | Status | Next Review |
|------|---------------|-------------|--------|-------------|
| `memory/decisions.json` | 2026-01-13 | Initial Creation | 🟢 Fresh | Auto-decay |
| `memory/patterns.json` | 2026-01-13 | Initial Creation | 🟢 Fresh | Auto-decay |
| `memory/corrections.json` | 2026-01-13 | Initial Creation | 🟢 Fresh | Never |

### Project Files
| File | Last Verified | Verified By | Status | Next Review |
|------|---------------|-------------|--------|-------------|
| `AGENT.md` | 2026-01-13 | Initial Creation | 🟢 Fresh | 2026-02-01 |
| `AGENTS.md` | 2026-01-13 | Initial Creation | 🟢 Fresh | 2026-02-01 |

---

## Status Definitions

| Status | Icon | Meaning | Action |
|--------|------|---------|--------|
| Fresh | 🟢 | Verified within policy period | Use normally |
| Warning | 🟡 | Approaching staleness | Review soon |
| Stale | 🔴 | Past due for verification | Verify before using |
| Auto | 🔵 | Auto-generated/maintained | No manual review needed |

---

## Staleness Policies

| Content Type | Fresh Period | Warning At | Stale At |
|--------------|--------------|------------|----------|
| **Core context** (context.md, architecture.md) | 30 days | 25 days | 30 days |
| **Rules** (rules/*.md) | 30 days | 25 days | 30 days |
| **Hot context** (HOT_CONTEXT.md) | 14 days | 10 days | 14 days |
| **Project index** (AGENT.md) | 14 days | 10 days | 14 days |
| **Memory/patterns** | Auto-decay | 60 days unused | 90 days unused |
| **Corrections** | Never | Never | Never |

---

## Verification Checklist

When verifying a context file:

- [ ] Information matches current codebase
- [ ] Links/references are valid
- [ ] No outdated dates/versions
- [ ] No references to deleted files
- [ ] Examples still compile
- [ ] Patterns still used in codebase

---

## Recent Changes Log

| Date | File | Change | By |
|------|------|--------|-----|
| 2026-01-13 | All files | Initial context engineering system creation | Cursor AI |

---

## How to Update This File

When you verify or update a context file:

1. Update the "Last Verified" date
2. Update "Verified By" (Human/AI/Auto-gen)
3. Recalculate "Next Review" based on policy
4. Add entry to "Recent Changes Log"

---

*AI should check this file when loading context to identify potentially stale information.*
