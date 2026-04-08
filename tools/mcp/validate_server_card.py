#!/usr/bin/env python3
"""
Validate `.well-known/mcp/server-card.json` for Smithery static server card publishing.

Checks:
  - Valid JSON (UTF-8; optional BOM stripped)
  - Required top-level keys per Smithery docs (serverInfo, tools list shape)
  - Tool names are unique; each tool has name, description, inputSchema
  - Tool name set matches `smithery.yaml` tools list (no drift)

Exit code 0 on success, 1 on failure (also prints GitHub Actions ::error lines).
"""

from __future__ import annotations

import json
import re
import sys
from pathlib import Path

def _repo_root() -> Path:
    """Resolve repo root by finding smithery.yaml (robust if cwd or script layout changes)."""
    here = Path(__file__).resolve()
    for d in [here.parent, *here.parents]:
        if (d / "smithery.yaml").is_file():
            return d
    return here.parents[2]


REPO_ROOT = _repo_root()
SERVER_CARD = REPO_ROOT / ".well-known" / "mcp" / "server-card.json"
SMITHERY = REPO_ROOT / "smithery.yaml"


def _tools_from_smithery_yaml(text: str) -> list[str]:
    """Parse top-level `tools:` list without PyYAML (flat dash-list only)."""
    lines = text.splitlines()
    in_tools = False
    tools: list[str] = []
    indent_unit = None

    for i, line in enumerate(lines):
        stripped = line.strip()
        if not in_tools:
            if stripped == "tools:":
                in_tools = True
                # detect base indent of first list item
            continue

        if not line.strip():
            continue

        # End section when a non-indented key appears (new top-level key)
        if stripped and not line.startswith(" ") and not line.startswith("\t"):
            if not stripped.startswith("-") and ":" in stripped:
                break

        m = re.match(r"^[\s\t]*-\s+(\S+)\s*$", line)
        if m:
            tools.append(m.group(1))
            if indent_unit is None:
                indent_unit = len(line) - len(line.lstrip())
            continue

        # Nested content under tools (should not happen in our smithery.yaml)
        if tools and indent_unit is not None:
            current_indent = len(line) - len(line.lstrip())
            if current_indent <= indent_unit and not line.lstrip().startswith("-"):
                break

    return tools


def main() -> int:
    errors: list[str] = []

    if not SERVER_CARD.is_file():
        errors.append(f"Missing server card: {SERVER_CARD}")
        _emit(errors)
        return 1

    if not SMITHERY.is_file():
        errors.append(f"Missing smithery.yaml: {SMITHERY}")
        _emit(errors)
        return 1

    raw = SERVER_CARD.read_bytes()
    if raw.startswith(b"\xef\xbb\xbf"):
        raw = raw[3:]

    try:
        data = json.loads(raw.decode("utf-8"))
    except (UnicodeDecodeError, json.JSONDecodeError) as e:
        errors.append(f"server-card.json is not valid UTF-8 JSON: {e}")
        _emit(errors)
        return 1

    if not isinstance(data, dict):
        errors.append("server-card root must be a JSON object")
        _emit(errors)
        return 1

    si = data.get("serverInfo")
    if not isinstance(si, dict):
        keys_preview = list(data.keys())[:20]
        print(
            f"validate_server_card: {SERVER_CARD} ({len(raw)} bytes), top-level keys={keys_preview}",
            file=sys.stderr,
        )
        hint = (
            " Expected an object like {\"name\": \"...\", \"version\": \"...\"}. "
            "Common mistakes: wrong file checked in, empty {}, null serverInfo, or "
            "PascalCase keys (JSON must use serverInfo and tools)."
        )
        errors.append(
            f"serverInfo must be an object (got {type(si).__name__}).{hint}"
        )
    else:
        if not si.get("name"):
            errors.append("serverInfo.name is required")
        if not si.get("version"):
            errors.append("serverInfo.version is required")

    tools = data.get("tools")
    if not isinstance(tools, list):
        keys_preview = list(data.keys())[:20]
        print(
            f"validate_server_card: {SERVER_CARD} ({len(raw)} bytes), top-level keys={keys_preview}",
            file=sys.stderr,
        )
        hint = (
            " Expected a JSON array of tool definitions. "
            "Common mistakes: tools missing, null, or an object instead of an array."
        )
        errors.append(
            f"tools must be an array (got {type(tools).__name__}).{hint}"
        )
        _emit(errors)
        return 1

    names: list[str] = []
    for i, t in enumerate(tools):
        if not isinstance(t, dict):
            errors.append(f"tools[{i}] must be an object")
            continue
        n = t.get("name")
        if not n or not isinstance(n, str):
            errors.append(f"tools[{i}].name must be a non-empty string")
        else:
            names.append(n)
        desc = t.get("description")
        if not desc or not isinstance(desc, str):
            errors.append(f"tools[{i}].description must be a non-empty string")
        schema = t.get("inputSchema")
        if not isinstance(schema, dict):
            errors.append(f"tools[{i}].inputSchema must be an object")
        elif schema.get("type") != "object":
            errors.append(f"tools[{i}].inputSchema.type must be 'object'")

    dupes = {n for n in names if names.count(n) > 1}
    if dupes:
        errors.append(f"Duplicate tool names: {sorted(dupes)}")

    for key in ("resources", "prompts"):
        if key in data and not isinstance(data[key], list):
            errors.append(f"{key} must be an array if present")

    smithery_text = SMITHERY.read_text(encoding="utf-8")
    sy_tools = _tools_from_smithery_yaml(smithery_text)
    if not sy_tools:
        errors.append("Could not parse tools list from smithery.yaml (is the format unchanged?)")
    else:
        set_card = set(names)
        set_smithery = set(sy_tools)
        if set_card != set_smithery:
            only_card = sorted(set_card - set_smithery)
            only_smithery = sorted(set_smithery - set_card)
            if only_card:
                errors.append(f"Tools only in server-card.json (not in smithery.yaml): {only_card}")
            if only_smithery:
                errors.append(f"Tools only in smithery.yaml (not in server-card.json): {only_smithery}")

    _emit(errors)
    return 1 if errors else 0


def _emit(errors: list[str]) -> None:
    for msg in errors:
        print(f"::error file=.well-known/mcp/server-card.json::{msg}")
        print(f"ERROR: {msg}", file=sys.stderr)
    if not errors:
        print("OK: server-card.json matches smithery.yaml and Smithery static card shape.")


if __name__ == "__main__":
    raise SystemExit(main())
