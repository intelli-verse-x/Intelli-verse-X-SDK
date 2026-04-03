#!/usr/bin/env python3
"""
IntelliVerseX Sound Manifest Validator
=======================================
Validates sound_manifest.json against the sound-manifest-v2 schema,
checks for required minimum sound set, and optionally verifies file URLs.

Usage:
    python validate_sound_manifest.py --manifest RemoteAssets/sound_manifest.json
    python validate_sound_manifest.py --manifest sound_manifest.json --check-urls

Requires: jsonschema (pip install jsonschema)
"""

import argparse
import json
import sys
from pathlib import Path

try:
    from jsonschema import validate, ValidationError
except ImportError:
    print("ERROR: jsonschema required. Install with: pip install jsonschema")
    sys.exit(1)

SCHEMA_PATH = Path(__file__).parent / "schemas" / "sound-manifest-v2.json"

REQUIRED_SOUNDS = {
    "ui": [
        "ui_button_click", "ui_button_hover", "ui_card_flip",
        "ui_menu_open", "ui_menu_close", "ui_page_turn"
    ],
    "sfx": [
        "sfx_correct_answer", "sfx_wrong_answer",
        "sfx_countdown_start", "sfx_level_complete"
    ],
    "stinger": [
        "stinger_victory", "stinger_defeat", "stinger_combo"
    ],
    "music": [
        "music_menu", "music_gameplay"
    ],
    "ambient": [
        "ambient_default"
    ]
}


def validate_manifest(manifest_path: str, check_urls: bool = False) -> list[str]:
    errors = []
    warnings = []
    path = Path(manifest_path)

    try:
        with open(path) as f:
            manifest = json.load(f)
    except json.JSONDecodeError as e:
        return [f"Invalid JSON: {e}"], []

    schema_path = SCHEMA_PATH
    if schema_path.exists():
        try:
            with open(schema_path) as f:
                schema = json.load(f)
            validate(instance=manifest, schema=schema)
        except ValidationError as e:
            errors.append(f"Schema violation: {e.message}")
    else:
        warnings.append(f"Schema file not found at {schema_path}, skipping schema validation")

    categories = manifest.get("categories", {})

    all_ids = set()
    duplicate_ids = []
    for cat_name, cat_entries in categories.items():
        for sound_id, entry in cat_entries.items():
            if sound_id in all_ids:
                duplicate_ids.append(sound_id)
            all_ids.add(sound_id)

            if not sound_id.startswith(f"{cat_name}_"):
                warnings.append(f"Sound '{sound_id}' in category '{cat_name}' does not "
                                f"use expected prefix '{cat_name}_'")

            vol = entry.get("volume")
            if vol is not None and (vol < 0 or vol > 1):
                errors.append(f"Sound '{sound_id}' has invalid volume: {vol} (must be 0.0–1.0)")

            if entry.get("variations", 1) > 1 and not entry.get("variation_urls"):
                warnings.append(f"Sound '{sound_id}' has {entry['variations']} variations "
                                f"but no variation_urls")

    if duplicate_ids:
        errors.append(f"Duplicate sound IDs: {', '.join(duplicate_ids)}")

    for cat, required_ids in REQUIRED_SOUNDS.items():
        cat_entries = categories.get(cat, {})
        for sound_id in required_ids:
            if sound_id not in cat_entries:
                warnings.append(f"Missing minimum required sound: {sound_id} (category: {cat})")

    if check_urls:
        try:
            import urllib.request
            base_url = manifest.get("base_url", "")
            for cat_entries in categories.values():
                for sound_id, entry in cat_entries.items():
                    url = entry.get("url", "")
                    if not url:
                        continue

                    full_url = url if url.startswith("http") else f"{base_url}{url}"
                    try:
                        req = urllib.request.Request(full_url, method="HEAD")
                        resp = urllib.request.urlopen(req, timeout=5)
                        if resp.status != 200:
                            errors.append(f"Sound '{sound_id}' URL returned {resp.status}: {full_url}")
                    except Exception as e:
                        errors.append(f"Sound '{sound_id}' URL unreachable: {full_url} ({e})")
        except ImportError:
            warnings.append("URL checking requires urllib (standard library)")

    return errors, warnings


def main():
    parser = argparse.ArgumentParser(description="Validate a sound manifest JSON file.")
    parser.add_argument("--manifest", required=True, help="Path to sound_manifest.json")
    parser.add_argument("--check-urls", action="store_true",
                        help="HEAD-request each URL to verify accessibility")
    parser.add_argument("--strict", action="store_true",
                        help="Treat warnings as errors")
    args = parser.parse_args()

    if not Path(args.manifest).exists():
        print(f"ERROR: File not found: {args.manifest}")
        sys.exit(1)

    errors, warnings = validate_manifest(args.manifest, args.check_urls)

    if warnings:
        print("\n⚠️  Warnings:")
        for w in warnings:
            print(f"   • {w}")

    if errors:
        print("\n❌ Errors:")
        for e in errors:
            print(f"   • {e}")

    total_issues = len(errors) + (len(warnings) if args.strict else 0)

    print(f"\n{'═' * 50}")
    print(f"Errors:   {len(errors)}")
    print(f"Warnings: {len(warnings)}")
    print(f"Result:   {'PASS ✅' if total_issues == 0 else 'FAIL ❌'}")

    if total_issues > 0:
        sys.exit(1)


if __name__ == "__main__":
    main()
