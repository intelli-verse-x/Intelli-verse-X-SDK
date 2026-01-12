#!/usr/bin/env python3
"""
IntelliVerseX Unity SDK - Context Engineering Validator

Goal: make the context-engineering system testable (CI/local) without adding dependencies.
Runs lightweight, deterministic checks and exits non-zero on violations.
"""

from __future__ import annotations

import argparse
import json
import os
import re
import sys
from dataclasses import dataclass
from pathlib import Path
from typing import Iterable, List, Optional, Sequence, Set, Tuple


@dataclass(frozen=True)
class Finding:
    code: str
    message: str
    details: Optional[str] = None


def _repo_root() -> Path:
    # tools/context/validate_context.py -> repo root is 2 levels up
    return Path(__file__).resolve().parents[2]


def _read_text(path: Path) -> str:
    return path.read_text(encoding="utf-8", errors="replace")


def _load_json(path: Path) -> object:
    return json.loads(_read_text(path))


def _is_numbered_list_line(line: str) -> bool:
    return bool(re.match(r"^\s*\d+\.\s+", line))


def _extract_loading_order_from_markdown(markdown: str) -> List[str]:
    """
    Extracts paths from a numbered list containing backticked file paths.
    Example line:
      1. `.cursor/NON_GOALS.md` — ...
    Returns extracted paths in order.
    """
    order: List[str] = []
    for line in markdown.splitlines():
        if not _is_numbered_list_line(line):
            continue
        match = re.search(r"`([^`]+)`", line)
        if not match:
            continue
        order.append(match.group(1).strip())
    return order


def _normalize_slashes(p: str) -> str:
    return p.replace("\\", "/")


def _load_todo_baseline(path: Path) -> Set[Tuple[str, str]]:
    """
    Baseline format: TSV with 2 columns:
      <relative-path>\t<trimmed-line-containing-TODO-or-FIXME>
    """
    baseline: Set[Tuple[str, str]] = set()
    raw = _read_text(path)
    for idx, line in enumerate(raw.splitlines(), start=1):
        if not line.strip():
            continue
        parts = line.split("\t")
        if len(parts) != 2:
            raise ValueError(f"Invalid baseline TSV at {path}:{idx} (expected 2 columns)")
        rel_path = _normalize_slashes(parts[0].strip())
        text = parts[1].strip()
        baseline.add((rel_path, text))
    return baseline


def _scan_todo_fixme_cs(repo: Path, root_relative: str) -> Set[Tuple[str, str]]:
    """
    Returns a set of (relative_path, trimmed_line) for any TODO/FIXME occurrences in .cs files.
    """
    target_root = repo / root_relative
    results: Set[Tuple[str, str]] = set()
    if not target_root.exists():
        return results

    pattern = re.compile(r"\b(TODO|FIXME)\b")

    for file_path in target_root.rglob("*.cs"):
        # Skip anything inside Unity hidden package folders if needed later.
        try:
            text = _read_text(file_path)
        except OSError:
            continue

        rel_path = _normalize_slashes(str(file_path.relative_to(repo)))
        for line in text.splitlines():
            if pattern.search(line):
                results.add((rel_path, line.strip()))
    return results


def _validate_memory_schema(repo: Path) -> List[Finding]:
    findings: List[Finding] = []
    memory_dir = repo / ".cursor" / "memory"

    def require_keys(obj: object, keys: Sequence[str], where: str) -> Optional[dict]:
        if not isinstance(obj, dict):
            findings.append(Finding("MEMORY_SCHEMA", f"{where} must be a JSON object"))
            return None
        missing = [k for k in keys if k not in obj]
        if missing:
            findings.append(Finding("MEMORY_SCHEMA", f"{where} missing keys: {', '.join(missing)}"))
            return None
        return obj

    # decisions.json
    decisions_path = memory_dir / "decisions.json"
    if decisions_path.exists():
        data = _load_json(decisions_path)
        obj = require_keys(data, ["version", "lastUpdated", "decisions"], "decisions.json")
        if obj is not None:
            if not isinstance(obj["decisions"], list):
                findings.append(Finding("MEMORY_SCHEMA", "decisions.json 'decisions' must be an array"))
            else:
                for i, item in enumerate(obj["decisions"]):
                    where = f"decisions.json decisions[{i}]"
                    item_obj = require_keys(
                        item,
                        ["id", "date", "category", "decision", "rationale", "alternatives", "outcome"],
                        where,
                    )
                    if item_obj is None:
                        continue
                    if not isinstance(item_obj["alternatives"], list):
                        findings.append(Finding("MEMORY_SCHEMA", f"{where}.alternatives must be an array"))

    # patterns.json
    patterns_path = memory_dir / "patterns.json"
    if patterns_path.exists():
        data = _load_json(patterns_path)
        obj = require_keys(data, ["version", "lastUpdated", "patterns"], "patterns.json")
        if obj is not None:
            if not isinstance(obj["patterns"], list):
                findings.append(Finding("MEMORY_SCHEMA", "patterns.json 'patterns' must be an array"))
            else:
                for i, item in enumerate(obj["patterns"]):
                    where = f"patterns.json patterns[{i}]"
                    item_obj = require_keys(item, ["id", "name", "description", "template", "usage", "confidence"], where)
                    if item_obj is None:
                        continue
                    conf = item_obj.get("confidence")
                    if not isinstance(conf, (int, float)) or conf < 0.0 or conf > 1.0:
                        findings.append(Finding("MEMORY_SCHEMA", f"{where}.confidence must be 0.0-1.0"))

    # corrections.json
    corrections_path = memory_dir / "corrections.json"
    if corrections_path.exists():
        data = _load_json(corrections_path)
        obj = require_keys(data, ["version", "lastUpdated", "corrections"], "corrections.json")
        if obj is not None:
            if not isinstance(obj["corrections"], list):
                findings.append(Finding("MEMORY_SCHEMA", "corrections.json 'corrections' must be an array"))
            else:
                for i, item in enumerate(obj["corrections"]):
                    where = f"corrections.json corrections[{i}]"
                    require_keys(item, ["id", "date", "wrong", "correct", "context"], where)

    return findings


def _validate_command_registry(repo: Path) -> List[Finding]:
    findings: List[Finding] = []
    path = repo / ".cursor" / "commands.json"
    if not path.exists():
        return [Finding("COMMANDS_MISSING", "Missing command registry: .cursor/commands.json")]

    data = _load_json(path)
    if not isinstance(data, dict):
        return [Finding("COMMANDS_SCHEMA", ".cursor/commands.json must be a JSON object")]

    for key in ["version", "lastUpdated", "commands"]:
        if key not in data:
            findings.append(Finding("COMMANDS_SCHEMA", f".cursor/commands.json missing key: {key}"))
            return findings

    commands = data.get("commands")
    if not isinstance(commands, list) or len(commands) == 0:
        findings.append(Finding("COMMANDS_SCHEMA", ".cursor/commands.json 'commands' must be a non-empty array"))
        return findings

    ids: Set[str] = set()
    for i, cmd in enumerate(commands):
        where = f"commands[{i}]"
        if not isinstance(cmd, dict):
            findings.append(Finding("COMMANDS_SCHEMA", f"{where} must be an object"))
            continue
        for key in ["id", "name", "intent", "when_to_use", "run", "outputs"]:
            if key not in cmd:
                findings.append(Finding("COMMANDS_SCHEMA", f"{where} missing key: {key}"))
        cmd_id = cmd.get("id")
        if isinstance(cmd_id, str):
            if cmd_id in ids:
                findings.append(Finding("COMMANDS_SCHEMA", f"Duplicate command id: {cmd_id}"))
            ids.add(cmd_id)

    return findings


def _validate_runtime_no_unityeditor(repo: Path) -> List[Finding]:
    """
    Prevents UnityEditor usage in runtime code paths inside our SDK package.
    This does NOT scan third-party folders.
    """
    findings: List[Finding] = []
    sdk_root = repo / "Assets" / "_IntelliVerseXSDK"
    if not sdk_root.exists():
        return findings

    editor_using = re.compile(r"^\s*using\s+UnityEditor\s*;\s*$", re.MULTILINE)

    for file_path in sdk_root.rglob("*.cs"):
        rel = _normalize_slashes(str(file_path.relative_to(repo)))
        # Allowed in Editor folders and Editor tests.
        if "/Editor/" in rel or "/Tests~/Editor/" in rel:
            continue

        text = _read_text(file_path)
        if editor_using.search(text):
            findings.append(
                Finding(
                    "RUNTIME_UNITYEDITOR",
                    f"UnityEditor used in runtime file: {rel}",
                    "Move this code under an Editor/ folder or use an Editor asmdef.",
                )
            )

    return findings


def _validate_todo_fixme_baseline(repo: Path) -> List[Finding]:
    findings: List[Finding] = []
    baseline_path = repo / "tools" / "context" / "baselines" / "todo_fixme_cs.tsv"
    if not baseline_path.exists():
        return [
            Finding(
                "BASELINE_MISSING",
                "Missing TODO/FIXME baseline file",
                "Expected: tools/context/baselines/todo_fixme_cs.tsv",
            )
        ]

    baseline = _load_todo_baseline(baseline_path)
    current = _scan_todo_fixme_cs(repo, "Assets/_IntelliVerseXSDK")

    new_items = sorted(current - baseline)
    removed_items = sorted(baseline - current)

    if new_items:
        details = "\n".join([f"- {p}\t{s}" for (p, s) in new_items[:50]])
        if len(new_items) > 50:
            details += f"\n... and {len(new_items) - 50} more"
        findings.append(
            Finding(
                "NEW_TODO_FIXME",
                f"New TODO/FIXME found in SDK C# code: {len(new_items)} new occurrence(s)",
                details,
            )
        )

    # Removals are allowed (you're reducing TODOs), but we warn so baseline can be refreshed deliberately.
    if removed_items:
        details = "\n".join([f"- {p}\t{s}" for (p, s) in removed_items[:50]])
        if len(removed_items) > 50:
            details += f"\n... and {len(removed_items) - 50} more"
        findings.append(
            Finding(
                "BASELINE_STALE",
                f"TODO/FIXME baseline has {len(removed_items)} removed occurrence(s)",
                "If intentional, update tools/context/baselines/todo_fixme_cs.tsv.\n" + details,
            )
        )

    return findings


def _validate_loading_order_consistency(repo: Path) -> List[Finding]:
    """
    Ensures the loading order is consistent across the main entry-point docs.
    """
    findings: List[Finding] = []
    canonical = [
        ".cursor/NON_GOALS.md",
        ".cursor/AI_GUARDRAILS.md",
        ".cursor/HOT_CONTEXT.md",
        ".cursor/ANTI_PATTERNS.md",
        ".cursor/context.md",
        ".cursor/architecture.md",
        ".cursor/naming-and-style.md",
        ".cursor/assumptions.md",
    ]
    canonical = [_normalize_slashes(x) for x in canonical]

    def check_file(path: Path, required: bool = True) -> None:
        if not path.exists():
            if required:
                findings.append(Finding("LOADING_ORDER", f"Missing file: {path.relative_to(repo)}"))
            return
        order = _extract_loading_order_from_markdown(_read_text(path))
        order = [_normalize_slashes(x) for x in order]
        if not order:
            findings.append(
                Finding(
                    "LOADING_ORDER",
                    f"Could not extract numbered loading-order list from {path.relative_to(repo)}",
                    "Expected numbered list with backticked file paths.",
                )
            )
            return
        # Compare only the canonical prefix — some docs include extra steps (DECISION_TREE, RULE.md, etc.).
        prefix = order[: len(canonical)]
        if prefix != canonical:
            details = (
                "Expected prefix:\n"
                + "\n".join([f"- {x}" for x in canonical])
                + "\n\nFound prefix:\n"
                + "\n".join([f"- {x}" for x in prefix])
            )
            findings.append(
                Finding(
                    "LOADING_ORDER",
                    f"Context loading order mismatch in {path.relative_to(repo)}",
                    details,
                )
            )

    check_file(repo / ".cursorrules")
    check_file(repo / "AGENTS.md")
    check_file(repo / ".cursor" / "context.md")

    return findings


def main(argv: Optional[Sequence[str]] = None) -> int:
    parser = argparse.ArgumentParser(description="Validate IntelliVerseX context-engineering rules.")
    parser.add_argument("--ci", action="store_true", help="CI mode (treat warnings as errors).")
    args = parser.parse_args(argv)

    repo = _repo_root()

    findings: List[Finding] = []
    findings.extend(_validate_loading_order_consistency(repo))
    findings.extend(_validate_memory_schema(repo))
    findings.extend(_validate_command_registry(repo))
    findings.extend(_validate_runtime_no_unityeditor(repo))
    findings.extend(_validate_todo_fixme_baseline(repo))

    errors: List[Finding] = []
    warnings: List[Finding] = []

    # Only BASELINE_STALE is a warning by default (baseline needs refresh after TODO removal).
    warning_codes = {"BASELINE_STALE"}

    for f in findings:
        if f.code in warning_codes:
            warnings.append(f)
        else:
            errors.append(f)

    if warnings:
        print("\nWarnings:")
        for w in warnings:
            print(f"- [{w.code}] {w.message}")
            if w.details:
                print(w.details)

    if errors:
        print("\nErrors:")
        for e in errors:
            print(f"- [{e.code}] {e.message}")
            if e.details:
                print(e.details)
        return 1

    return 0 if (not args.ci or not warnings) else 1


if __name__ == "__main__":
    raise SystemExit(main())

