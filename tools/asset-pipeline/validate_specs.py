#!/usr/bin/env python3
"""
IntelliVerseX Sprite Spec Validator
=====================================
Validates all _spec.json files in a directory tree against the sprite-spec-v1 schema
and checks that companion sprite sheet PNGs have correct dimensions.

Usage:
    python validate_specs.py --directory RemoteAssets/Characters/
    python validate_specs.py --directory RemoteAssets/Characters/Quizzy/sprites/

Requires: jsonschema, Pillow (pip install jsonschema Pillow)
"""

import argparse
import json
import os
import sys
from pathlib import Path

try:
    from jsonschema import validate, ValidationError
except ImportError:
    print("ERROR: jsonschema required. Install with: pip install jsonschema")
    sys.exit(1)

try:
    from PIL import Image
except ImportError:
    Image = None


SCHEMA_PATH = Path(__file__).parent / "schemas" / "sprite-spec-v1.json"
MAX_SHEET_DIM = 4096


def load_schema() -> dict:
    with open(SCHEMA_PATH) as f:
        return json.load(f)


def validate_spec_file(spec_path: str, schema: dict) -> list[str]:
    """Validate a single _spec.json file. Returns list of error strings."""
    errors = []
    spec_path = Path(spec_path)

    try:
        with open(spec_path) as f:
            spec = json.load(f)
    except json.JSONDecodeError as e:
        return [f"Invalid JSON: {e}"]

    try:
        validate(instance=spec, schema=schema)
    except ValidationError as e:
        errors.append(f"Schema violation: {e.message} (path: {'.'.join(str(p) for p in e.path)})")

    action = spec.get("action", "")
    frames = spec.get("frames", 0)
    cell_size = spec.get("cell_size", 0)
    fps = spec.get("fps", 10)
    cols = spec.get("layout", {}).get("columns", 0)
    rows = spec.get("layout", {}).get("rows", 0)

    if frames <= 0:
        errors.append(f"frames must be > 0 (got {frames})")
    if cell_size <= 0:
        errors.append(f"cell_size must be > 0 (got {cell_size})")

    if cols > 0 and rows > 0 and cols * rows < frames:
        errors.append(f"layout ({cols}×{rows}={cols * rows} cells) cannot hold {frames} frames")

    if cols > 0 and cell_size > 0 and cols * cell_size > MAX_SHEET_DIM:
        errors.append(f"Sheet width {cols * cell_size}px exceeds {MAX_SHEET_DIM}px limit")
    if rows > 0 and cell_size > 0 and rows * cell_size > MAX_SHEET_DIM:
        errors.append(f"Sheet height {rows * cell_size}px exceeds {MAX_SHEET_DIM}px limit")

    if fps < 0 or fps > 60:
        errors.append(f"fps must be 0–60 (got {fps})")

    expected_basename = spec_path.stem.replace("_spec", "")
    if action and action != expected_basename:
        errors.append(f"action '{action}' does not match filename '{expected_basename}'")

    for event in spec.get("events", []):
        frame_idx = event.get("frame", -1)
        if frame_idx < 0 or frame_idx >= frames:
            errors.append(f"Event frame index {frame_idx} out of range [0, {frames - 1}]")

    for frame_str in spec.get("sound_sync", {}):
        try:
            idx = int(frame_str)
            if idx < 0 or idx >= frames:
                errors.append(f"sound_sync frame '{frame_str}' out of range [0, {frames - 1}]")
        except ValueError:
            errors.append(f"sound_sync key '{frame_str}' is not a valid integer")

    sheet_path = spec_path.parent / f"{expected_basename}.png"
    if sheet_path.exists() and Image is not None:
        try:
            with Image.open(sheet_path) as img:
                w, h = img.size
                expected_cols = cols if cols > 0 else (w // cell_size if cell_size > 0 else 0)
                expected_rows = rows if rows > 0 else (h // cell_size if cell_size > 0 else 0)

                if cell_size > 0:
                    if w % cell_size != 0:
                        errors.append(f"Sheet width {w}px is not divisible by cell_size {cell_size}px")
                    if h % cell_size != 0:
                        errors.append(f"Sheet height {h}px is not divisible by cell_size {cell_size}px")
                    if expected_cols > 0 and w != expected_cols * cell_size:
                        errors.append(f"Sheet width {w}px != expected {expected_cols * cell_size}px "
                                      f"({expected_cols} cols × {cell_size}px)")
                    if expected_rows > 0 and h != expected_rows * cell_size:
                        errors.append(f"Sheet height {h}px != expected {expected_rows * cell_size}px "
                                      f"({expected_rows} rows × {cell_size}px)")
        except Exception as e:
            errors.append(f"Could not read sheet PNG: {e}")
    elif not sheet_path.exists():
        errors.append(f"Missing companion sheet: {sheet_path.name}")

    return errors


def main():
    parser = argparse.ArgumentParser(description="Validate sprite spec JSON files.")
    parser.add_argument("--directory", required=True,
                        help="Directory to scan for _spec.json files (recursive)")
    parser.add_argument("--strict", action="store_true",
                        help="Exit with error code on any warning")
    args = parser.parse_args()

    schema = load_schema()
    root = Path(args.directory)

    if not root.exists():
        print(f"ERROR: Directory not found: {root}")
        sys.exit(1)

    spec_files = sorted(root.rglob("*_spec.json"))
    if not spec_files:
        print(f"No _spec.json files found under {root}")
        sys.exit(0)

    total_errors = 0
    total_files = len(spec_files)

    for spec_file in spec_files:
        rel = spec_file.relative_to(root)
        errors = validate_spec_file(str(spec_file), schema)
        if errors:
            total_errors += len(errors)
            print(f"\n❌ {rel}")
            for e in errors:
                print(f"   • {e}")
        else:
            print(f"✅ {rel}")

    print(f"\n{'═' * 50}")
    print(f"Scanned: {total_files} spec files")
    print(f"Errors:  {total_errors}")
    print(f"Result:  {'PASS ✅' if total_errors == 0 else 'FAIL ❌'}")

    if total_errors > 0 and args.strict:
        sys.exit(1)


if __name__ == "__main__":
    main()
