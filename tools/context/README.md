# 🧪 Context Validator (Testable Context Engineering)

This folder makes the IntelliVerseX **context-engineering system testable** locally and in CI.

## What it validates

- **Context loading order consistency** across:
  - `.cursorrules`
  - `AGENTS.md`
  - `.cursor/context.md`
- **Memory JSON schema**:
  - `.cursor/memory/decisions.json`
  - `.cursor/memory/patterns.json`
  - `.cursor/memory/corrections.json`
- **Runtime safety guard**: prevents `using UnityEditor;` in non-Editor SDK C# files under `Assets/_IntelliVerseXSDK/`
- **No new TODO/FIXME** in SDK C# (baseline gated):
  - Existing TODOs are tracked in `baselines/todo_fixme_cs.tsv`
  - CI fails only if **new** TODO/FIXME lines appear

## Run locally

### Windows (PowerShell)

```powershell
python tools/context/validate_context.py
```

### macOS/Linux

```bash
python3 tools/context/validate_context.py
```

## Baselines

When you remove TODO/FIXME lines (good!), update the baseline intentionally:

- `tools/context/baselines/todo_fixme_cs.tsv`

Baseline format (TSV):

```
<relative-path>\t<trimmed-line-containing-TODO-or-FIXME>
```

