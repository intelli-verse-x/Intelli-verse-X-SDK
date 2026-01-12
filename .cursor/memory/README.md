# 🧠 Memory System

This folder contains the AI learning and memory system for the IntelliVerseX Unity SDK.

## Files

| File | Purpose |
|------|---------|
| `decisions.json` | Logged decisions with rationale |
| `patterns.json` | Learned patterns to apply |
| `corrections.json` | User corrections to avoid |

## Schema

### decisions.json

```json
{
  "id": "DEC-YYYYMMDD-NNN",
  "date": "YYYY-MM-DD",
  "category": "architecture|security|api|workflow",
  "decision": "What was decided",
  "rationale": "Why this decision was made",
  "alternatives": ["What else was considered"],
  "outcome": "successful|failed|pending"
}
```

### patterns.json

```json
{
  "id": "PAT-NNN",
  "name": "Pattern Name",
  "description": "What this pattern does",
  "template": "Code template or format",
  "usage": "When to use this pattern",
  "confidence": 0.0-1.0
}
```

### corrections.json

```json
{
  "id": "COR-NNN",
  "date": "YYYY-MM-DD",
  "wrong": "What was done incorrectly",
  "correct": "What should have been done",
  "context": "When this applies"
}
```

## Usage

- **Decisions**: Logged automatically for significant choices
- **Patterns**: Applied automatically when similar context detected
- **Corrections**: Prevent repeating past mistakes

## Maintenance

- Review decisions monthly
- Prune outdated patterns (>90 days unused)
- Corrections never expire
