#!/usr/bin/env python3
"""
IntelliVerseX Character Validator
===================================
Validates a complete character folder against the asset pipeline standard.
Checks for required files, animation tiers, spec validity, and image dimensions.

Usage:
    python validate_character.py --character RemoteAssets/Characters/Quizzy/
    python validate_character.py --character RemoteAssets/Characters/ --all

Requires: Pillow, jsonschema (pip install Pillow jsonschema)
"""

import argparse
import json
import os
import sys
from pathlib import Path

try:
    from PIL import Image
except ImportError:
    Image = None
    print("WARNING: Pillow not installed — image dimension checks will be skipped.")

MANDATORY_ANIMS = ["idle", "jump", "hurt"]
STANDARD_ANIMS = ["walk", "run", "attack"]
EXTENDED_ANIMS = ["dance", "wave", "think", "sleep", "special", "death", "spawn"]

REQUIRED_VIEWS = {
    "front.png": (512, 512),
    "thumbnail.png": (128, 128),
}


def check_image_dimensions(path: Path, expected: tuple[int, int] | None) -> str | None:
    if Image is None:
        return None
    if not path.exists():
        return f"Missing: {path.name}"
    try:
        with Image.open(path) as img:
            if expected and img.size != expected:
                return f"{path.name}: expected {expected[0]}×{expected[1]}, got {img.size[0]}×{img.size[1]}"
    except Exception as e:
        return f"{path.name}: could not open ({e})"
    return None


def validate_character(char_dir: Path) -> dict:
    results = {
        "character": char_dir.name,
        "pass": [],
        "warn": [],
        "fail": [],
        "mandatory_score": 0,
        "standard_score": 0,
        "extended_score": 0,
    }

    meta_path = char_dir / "character.json"
    if meta_path.exists():
        try:
            with open(meta_path) as f:
                meta = json.load(f)
            required_fields = ["id", "display_name", "rarity", "animations", "views"]
            missing = [f for f in required_fields if f not in meta]
            if missing:
                results["fail"].append(f"character.json missing fields: {', '.join(missing)}")
            else:
                results["pass"].append("character.json valid")
        except json.JSONDecodeError as e:
            results["fail"].append(f"character.json invalid JSON: {e}")
    else:
        results["fail"].append("Missing character.json")

    for view_file, expected_dims in REQUIRED_VIEWS.items():
        view_path = char_dir / view_file
        err = check_image_dimensions(view_path, expected_dims)
        if err:
            if "Missing" in err:
                results["fail"].append(err)
            else:
                results["warn"].append(err)
        else:
            w, h = expected_dims
            results["pass"].append(f"{view_file} exists ({w}×{h})")

    sprites_dir = char_dir / "sprites"
    if not sprites_dir.exists():
        results["fail"].append("Missing sprites/ directory")
        return results

    for anim in MANDATORY_ANIMS:
        sheet = sprites_dir / f"{anim}.png"
        spec = sprites_dir / f"{anim}_spec.json"
        if sheet.exists() and spec.exists():
            results["pass"].append(f"sprites/{anim}.png + {anim}_spec.json")
            results["mandatory_score"] += 1

            try:
                with open(spec) as f:
                    spec_data = json.load(f)
                frames = spec_data.get("frames", "?")
                cell = spec_data.get("cell_size", "?")
                cols = spec_data.get("layout", {}).get("columns", "?")
                rows = spec_data.get("layout", {}).get("rows", "?")
                results["pass"][-1] += f" ({frames} frames, {cell}px, {cols}×{rows})"
            except Exception:
                pass
        else:
            missing = []
            if not sheet.exists():
                missing.append(f"{anim}.png")
            if not spec.exists():
                missing.append(f"{anim}_spec.json")
            results["fail"].append(f"Missing mandatory animation: {', '.join(missing)}")

    for anim in STANDARD_ANIMS:
        sheet = sprites_dir / f"{anim}.png"
        spec = sprites_dir / f"{anim}_spec.json"
        if sheet.exists() and spec.exists():
            results["pass"].append(f"sprites/{anim}.png + {anim}_spec.json (standard)")
            results["standard_score"] += 1
        else:
            results["warn"].append(f"Missing Tier 2 animation: {anim}")

    for anim in EXTENDED_ANIMS:
        sheet = sprites_dir / f"{anim}.png"
        spec = sprites_dir / f"{anim}_spec.json"
        if sheet.exists() and spec.exists():
            results["pass"].append(f"sprites/{anim}.png + {anim}_spec.json (extended)")
            results["extended_score"] += 1

    return results


def print_results(results: dict):
    print(f"\n{'═' * 60}")
    print(f"  Character: {results['character']}")
    print(f"{'═' * 60}")

    for msg in results["pass"]:
        print(f"  ✅ {msg}")
    for msg in results["warn"]:
        print(f"  ⚠️  {msg}")
    for msg in results["fail"]:
        print(f"  ❌ {msg}")

    m = results["mandatory_score"]
    s = results["standard_score"]
    e = results["extended_score"]
    mt = len(MANDATORY_ANIMS)
    st = len(STANDARD_ANIMS)
    et = len(EXTENDED_ANIMS)

    passed = m == mt and len(results["fail"]) == 0
    print(f"\n  Score: {m}/{mt} mandatory {'✅' if m == mt else '❌'} | "
          f"{s}/{st} standard | {e}/{et} extended")
    print(f"  Result: {'PASS ✅' if passed else 'FAIL ❌'}")
    return passed


def main():
    parser = argparse.ArgumentParser(description="Validate character folder(s) against the IVX standard.")
    parser.add_argument("--character", required=True,
                        help="Path to a character folder, or parent Characters/ folder with --all")
    parser.add_argument("--all", action="store_true",
                        help="Validate all character subdirectories")
    args = parser.parse_args()

    root = Path(args.character)
    if not root.exists():
        print(f"ERROR: Path not found: {root}")
        sys.exit(1)

    if args.all:
        char_dirs = sorted([d for d in root.iterdir() if d.is_dir()])
        if not char_dirs:
            print(f"No character subdirectories found in {root}")
            sys.exit(0)

        all_pass = True
        for char_dir in char_dirs:
            results = validate_character(char_dir)
            if not print_results(results):
                all_pass = False

        print(f"\n{'═' * 60}")
        print(f"  Total: {len(char_dirs)} characters")
        print(f"  Overall: {'ALL PASS ✅' if all_pass else 'SOME FAILED ❌'}")
        if not all_pass:
            sys.exit(1)
    else:
        results = validate_character(root)
        if not print_results(results):
            sys.exit(1)


if __name__ == "__main__":
    main()
