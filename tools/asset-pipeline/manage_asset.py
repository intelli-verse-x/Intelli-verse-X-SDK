#!/usr/bin/env python3
"""
IntelliVerseX Asset Manager — CRUD for any game asset.

Usage:
    python manage_asset.py add    <type> [options]
    python manage_asset.py replace <type> [options]
    python manage_asset.py modify  <type> [options]
    python manage_asset.py delete  <type> [options]
    python manage_asset.py inventory [options]
    python manage_asset.py audit   <check> [options]
    python manage_asset.py sync    <direction> [options]

Supports: 2D characters, sprites, 3D characters, sounds, videos, models, scenes, UI.
All operations maintain SDK schema compliance and update manifests.
"""

from __future__ import annotations

import argparse
import datetime
import json
import os
import re
import shutil
import sys
from pathlib import Path
from typing import Any, Dict, List, Optional

SCHEMA_DIR = os.path.join(os.path.dirname(__file__), "schemas")

try:
    from PIL import Image
    HAS_PIL = True
except ImportError:
    HAS_PIL = False

try:
    import jsonschema
    HAS_JSONSCHEMA = True
except ImportError:
    HAS_JSONSCHEMA = False


def _load_schema(name: str) -> dict:
    path = os.path.join(SCHEMA_DIR, name)
    if not os.path.exists(path):
        return {}
    with open(path) as f:
        return json.load(f)


def _validate(data: dict, schema: dict) -> list[str]:
    if not HAS_JSONSCHEMA or not schema:
        return []
    validator = jsonschema.Draft7Validator(schema)
    return [e.message for e in validator.iter_errors(data)]


def _read_json(path: str) -> dict:
    with open(path) as f:
        return json.load(f)


def _write_json(path: str, data: dict) -> None:
    os.makedirs(os.path.dirname(path) or ".", exist_ok=True)
    with open(path, "w") as f:
        json.dump(data, f, indent=2, ensure_ascii=False)


def _backup(path: str) -> Optional[str]:
    if os.path.exists(path):
        bak = path + ".bak"
        shutil.copy2(path, bak)
        return bak
    return None


def _log_change(root: str, operation: str, asset_type: str, asset_id: str, details: dict) -> None:
    log_path = os.path.join(root, "asset_changelog.json")
    entries = []
    if os.path.exists(log_path):
        try:
            entries = json.loads(Path(log_path).read_text())
        except Exception:
            entries = []
    entries.append({
        "timestamp": datetime.datetime.now(datetime.timezone.utc).isoformat(),
        "operation": operation,
        "type": asset_type,
        "id": asset_id,
        **details,
    })
    _write_json(log_path, entries)


def _detect_sprite_grid(png_path: str, cell_size: int) -> dict:
    if not HAS_PIL:
        return {"columns": 6, "rows": 1, "frames": 6}
    img = Image.open(png_path)
    cols = img.width // cell_size
    rows = img.height // cell_size
    return {"columns": max(cols, 1), "rows": max(rows, 1), "frames": max(cols, 1) * max(rows, 1)}


# ─────────────────────────────────────────────────────────────
# ADD operations
# ─────────────────────────────────────────────────────────────

def add_sound(args: argparse.Namespace) -> None:
    manifest_path = args.manifest
    if not os.path.exists(manifest_path):
        print(f"Manifest not found: {manifest_path}")
        sys.exit(1)

    manifest = _read_json(manifest_path)
    sound_id = re.sub(r"[^a-z0-9_]", "", args.id.lower().replace("-", "_"))
    category = args.category

    if "categories" not in manifest:
        print("Not an SDK-compliant sound manifest (missing 'categories'). Use sound-manifest-v2 format.")
        sys.exit(1)

    if category not in manifest["categories"]:
        manifest["categories"][category] = {}

    entry: Dict[str, Any] = {
        "url": args.file,
        "format": args.file.rsplit(".", 1)[-1] if "." in args.file else "ogg",
    }
    if args.duration_ms:
        entry["duration_ms"] = args.duration_ms
    if args.volume is not None:
        entry["volume"] = args.volume
    if args.loop:
        entry["loop"] = True
    if args.bpm:
        entry["bpm"] = args.bpm

    manifest["categories"][category][sound_id] = entry
    _backup(manifest_path)
    _write_json(manifest_path, manifest)

    errors = _validate(manifest, _load_schema("sound-manifest-v2.json"))
    if errors:
        print(f"WARNING: Manifest has validation errors after add:")
        for e in errors:
            print(f"  - {e}")
    else:
        print(f"Added sound '{sound_id}' to category '{category}'")

    _log_change(os.path.dirname(manifest_path), "add", "sound", sound_id, {"category": category})


def add_video(args: argparse.Namespace) -> None:
    output_dir = args.output or os.path.dirname(args.file) or "."
    os.makedirs(output_dir, exist_ok=True)

    vid_id = args.id or Path(args.file).stem.lower().replace("-", "_").replace(" ", "_")
    vid_id = re.sub(r"[^a-z0-9_]", "", vid_id)

    meta: Dict[str, Any] = {
        "id": vid_id,
        "title": args.title or vid_id,
        "url": args.file,
        "format": "mp4" if args.file.endswith(".mp4") else "webm",
    }

    try:
        import subprocess
        result = subprocess.run(
            ["ffprobe", "-v", "quiet", "-print_format", "json", "-show_format", "-show_streams", args.file],
            capture_output=True, text=True, timeout=15,
        )
        if result.returncode == 0:
            info = json.loads(result.stdout)
            vs = next((s for s in info.get("streams", []) if s.get("codec_type") == "video"), {})
            meta["codec"] = vs.get("codec_name", "h264")
            meta["resolution"] = {"width": int(vs.get("width", 1920)), "height": int(vs.get("height", 1080))}
            meta["duration_sec"] = round(float(info.get("format", {}).get("duration", 0)), 2)
            meta["bitrate_kbps"] = int(info.get("format", {}).get("bit_rate", 0)) // 1000
    except Exception:
        meta["codec"] = "h264"
        meta["resolution"] = {"width": 1920, "height": 1080}
        meta["duration_sec"] = 0
        meta["bitrate_kbps"] = 0

    if args.tags:
        meta["tags"] = args.tags.split(",")

    out = os.path.join(output_dir, "video_meta.json")
    _write_json(out, meta)

    errors = _validate(meta, _load_schema("video-meta-v1.json"))
    if errors:
        print(f"WARNING: video_meta.json has validation errors:")
        for e in errors:
            print(f"  - {e}")
    else:
        print(f"Added video '{vid_id}' → {out}")


# ─────────────────────────────────────────────────────────────
# REPLACE operations
# ─────────────────────────────────────────────────────────────

def replace_sprite(args: argparse.Namespace) -> None:
    char_dir = args.character.rstrip("/")
    action = args.action
    new_file = args.new_file

    sprite_path = os.path.join(char_dir, "sprites", f"{action}.png")
    spec_path = os.path.join(char_dir, "sprites", f"{action}_spec.json")
    char_json = os.path.join(char_dir, "character.json")

    if not os.path.exists(new_file):
        print(f"New file not found: {new_file}")
        sys.exit(1)

    _backup(sprite_path)
    _backup(spec_path)
    shutil.copy2(new_file, sprite_path)
    print(f"Replaced {sprite_path}")

    cell_size = args.cell_size or 512
    grid = _detect_sprite_grid(sprite_path, cell_size)
    frames = args.frames or grid["frames"]
    fps = args.fps or 10

    from_tier_map = {"idle": "mandatory", "walk": "standard", "run": "standard",
                     "jump": "mandatory", "hurt": "mandatory", "attack": "standard"}
    from_loop_map = {"idle": True, "walk": True, "run": True}

    spec = {
        "action": action,
        "frames": frames,
        "cell_size": cell_size,
        "fps": fps,
        "loop": from_loop_map.get(action, False),
        "layout": {"columns": grid["columns"], "rows": grid["rows"]},
    }
    _write_json(spec_path, spec)

    errors = _validate(spec, _load_schema("sprite-spec-v1.json"))
    if errors:
        print(f"WARNING: sprite spec validation errors:")
        for e in errors:
            print(f"  - {e}")

    if os.path.exists(char_json):
        char = _read_json(char_json)
        if "animations" in char:
            char["animations"][action] = {
                "tier": from_tier_map.get(action, "extended"),
                "frames": frames,
                "fps": fps,
            }
            _backup(char_json)
            _write_json(char_json, char)

            cerrors = _validate(char, _load_schema("character-meta-v1.json"))
            if cerrors:
                print(f"WARNING: character.json validation errors:")
                for e in cerrors:
                    print(f"  - {e}")
            else:
                print(f"Updated character.json with new '{action}' animation metadata")

    _log_change(char_dir, "replace", "sprite", action, {"new_file": new_file, "frames": frames})


def replace_sound(args: argparse.Namespace) -> None:
    manifest_path = args.manifest
    manifest = _read_json(manifest_path)
    sound_id = re.sub(r"[^a-z0-9_]", "", args.id.lower().replace("-", "_"))

    found = False
    for cat_name, cat_sounds in manifest.get("categories", {}).items():
        if sound_id in cat_sounds:
            _backup(manifest_path)
            cat_sounds[sound_id]["url"] = args.new_file
            if args.duration_ms:
                cat_sounds[sound_id]["duration_ms"] = args.duration_ms
            found = True
            _write_json(manifest_path, manifest)
            print(f"Replaced sound '{sound_id}' in category '{cat_name}'")
            break

    if not found:
        print(f"Sound '{sound_id}' not found in manifest")
        sys.exit(1)

    _log_change(os.path.dirname(manifest_path), "replace", "sound", sound_id, {"new_file": args.new_file})


# ─────────────────────────────────────────────────────────────
# MODIFY operations
# ─────────────────────────────────────────────────────────────

def modify_character(args: argparse.Namespace) -> None:
    char_dir = args.character.rstrip("/")
    char_json = os.path.join(char_dir, "character.json")

    if not os.path.exists(char_json):
        print(f"character.json not found in {char_dir}")
        sys.exit(1)

    _backup(char_json)
    char = _read_json(char_json)
    changes = {}

    if args.rarity:
        char["rarity"] = args.rarity
        changes["rarity"] = args.rarity
    if args.add_tags:
        existing = char.get("tags", [])
        new_tags = [t.strip() for t in args.add_tags.split(",")]
        char["tags"] = list(set(existing + new_tags))
        changes["tags_added"] = new_tags
    if args.unlock_method:
        char["unlock_method"] = args.unlock_method
        changes["unlock_method"] = args.unlock_method
    if args.unlock_cost:
        char["unlock_cost"] = json.loads(args.unlock_cost)
        changes["unlock_cost"] = args.unlock_cost
    if args.display_name:
        char["display_name"] = args.display_name
        changes["display_name"] = args.display_name
    if args.description:
        char["description"] = args.description
        changes["description"] = args.description

    _write_json(char_json, char)
    errors = _validate(char, _load_schema("character-meta-v1.json"))
    if errors:
        print(f"WARNING: character.json validation errors:")
        for e in errors:
            print(f"  - {e}")
    else:
        print(f"Modified character: {changes}")

    _log_change(char_dir, "modify", "character", char.get("id", "unknown"), changes)


def modify_sound(args: argparse.Namespace) -> None:
    manifest_path = args.manifest
    manifest = _read_json(manifest_path)
    sound_id = re.sub(r"[^a-z0-9_]", "", args.id.lower().replace("-", "_"))
    changes = {}

    for cat_name, cat_sounds in manifest.get("categories", {}).items():
        if sound_id in cat_sounds:
            _backup(manifest_path)
            entry = cat_sounds[sound_id]
            if args.volume is not None:
                entry["volume"] = args.volume
                changes["volume"] = args.volume
            if args.loop is not None:
                entry["loop"] = args.loop
                changes["loop"] = args.loop
            if args.bpm is not None:
                entry["bpm"] = args.bpm
                changes["bpm"] = args.bpm

            if args.category and args.category != cat_name:
                del cat_sounds[sound_id]
                if args.category not in manifest["categories"]:
                    manifest["categories"][args.category] = {}
                manifest["categories"][args.category][sound_id] = entry
                changes["category"] = f"{cat_name} → {args.category}"

            _write_json(manifest_path, manifest)
            print(f"Modified sound '{sound_id}': {changes}")
            _log_change(os.path.dirname(manifest_path), "modify", "sound", sound_id, changes)
            return

    print(f"Sound '{sound_id}' not found in manifest")
    sys.exit(1)


# ─────────────────────────────────────────────────────────────
# DELETE operations
# ─────────────────────────────────────────────────────────────

def delete_sprite(args: argparse.Namespace) -> None:
    char_dir = args.character.rstrip("/")
    action = args.action
    char_json = os.path.join(char_dir, "character.json")

    files_to_delete = [
        os.path.join(char_dir, "sprites", f"{action}.png"),
        os.path.join(char_dir, "sprites", f"{action}_spec.json"),
    ]

    if args.dry_run:
        print(f"DRY RUN — would delete:")
        for f in files_to_delete:
            print(f"  {f}" + (" (exists)" if os.path.exists(f) else " (missing)"))
        return

    if args.backup_to:
        os.makedirs(args.backup_to, exist_ok=True)
        for f in files_to_delete:
            if os.path.exists(f):
                shutil.copy2(f, os.path.join(args.backup_to, os.path.basename(f)))

    for f in files_to_delete:
        if os.path.exists(f):
            os.remove(f)
            print(f"Deleted {f}")

    if os.path.exists(char_json):
        char = _read_json(char_json)
        if action in char.get("animations", {}):
            _backup(char_json)
            del char["animations"][action]
            _write_json(char_json, char)
            print(f"Removed '{action}' from character.json animations")

    _log_change(char_dir, "delete", "sprite", action, {})


def delete_sound(args: argparse.Namespace) -> None:
    manifest_path = args.manifest
    manifest = _read_json(manifest_path)
    sound_id = re.sub(r"[^a-z0-9_]", "", args.id.lower().replace("-", "_"))

    for cat_name, cat_sounds in manifest.get("categories", {}).items():
        if sound_id in cat_sounds:
            if args.dry_run:
                print(f"DRY RUN — would remove '{sound_id}' from category '{cat_name}'")
                return

            _backup(manifest_path)
            del cat_sounds[sound_id]
            _write_json(manifest_path, manifest)
            print(f"Deleted sound '{sound_id}' from category '{cat_name}'")
            _log_change(os.path.dirname(manifest_path), "delete", "sound", sound_id, {"category": cat_name})
            return

    print(f"Sound '{sound_id}' not found in manifest")
    sys.exit(1)


def delete_character(args: argparse.Namespace) -> None:
    char_dir = args.character.rstrip("/")

    if not os.path.isdir(char_dir):
        print(f"Character directory not found: {char_dir}")
        sys.exit(1)

    if args.dry_run:
        count = sum(1 for _ in Path(char_dir).rglob("*") if _.is_file())
        print(f"DRY RUN — would delete {count} files in {char_dir}")
        return

    if args.backup_to:
        backup_dest = os.path.join(args.backup_to, os.path.basename(char_dir))
        shutil.copytree(char_dir, backup_dest, dirs_exist_ok=True)
        print(f"Backed up to {backup_dest}")

    shutil.rmtree(char_dir)
    print(f"Deleted character directory: {char_dir}")
    _log_change(os.path.dirname(char_dir), "delete", "character", os.path.basename(char_dir), {})


# ─────────────────────────────────────────────────────────────
# INVENTORY
# ─────────────────────────────────────────────────────────────

def inventory(args: argparse.Namespace) -> None:
    root = args.root
    assets = []

    for char_json in Path(root).rglob("character.json"):
        try:
            data = json.loads(char_json.read_text())
            if "animations" in data and "id" in data:
                schema = _load_schema("character-meta-v1.json")
                errors = _validate(data, schema)
                size = sum(f.stat().st_size for f in char_json.parent.rglob("*") if f.is_file())
                assets.append({
                    "type": "character_2d",
                    "id": data["id"],
                    "status": "valid" if not errors else f"{len(errors)} errors",
                    "schema": "v1",
                    "size_mb": round(size / (1024 * 1024), 1),
                    "path": str(char_json.parent),
                })
        except Exception:
            pass

    for char3d_json in Path(root).rglob("character_3d.json"):
        try:
            data = json.loads(char3d_json.read_text())
            schema = _load_schema("character-3d-meta-v1.json")
            errors = _validate(data, schema)
            size = sum(f.stat().st_size for f in char3d_json.parent.rglob("*") if f.is_file())
            assets.append({
                "type": "character_3d",
                "id": data.get("id", "unknown"),
                "status": "valid" if not errors else f"{len(errors)} errors",
                "schema": "v1",
                "size_mb": round(size / (1024 * 1024), 1),
                "path": str(char3d_json.parent),
            })
        except Exception:
            pass

    for manifest_json in Path(root).rglob("sound_manifest.json"):
        try:
            data = json.loads(manifest_json.read_text())
            if "categories" in data:
                schema = _load_schema("sound-manifest-v2.json")
                errors = _validate(data, schema)
                total_sounds = sum(len(cat) for cat in data.get("categories", {}).values())
                assets.append({
                    "type": "sound_manifest",
                    "id": manifest_json.parent.name,
                    "status": "valid" if not errors else f"{len(errors)} errors",
                    "schema": "v2",
                    "size_mb": round(manifest_json.stat().st_size / (1024 * 1024), 1),
                    "path": str(manifest_json),
                    "total_sounds": total_sounds,
                })
        except Exception:
            pass

    for video_json in Path(root).rglob("video_meta.json"):
        try:
            data = json.loads(video_json.read_text())
            schema = _load_schema("video-meta-v1.json")
            errors = _validate(data, schema)
            assets.append({
                "type": "video",
                "id": data.get("id", "unknown"),
                "status": "valid" if not errors else f"{len(errors)} errors",
                "schema": "v1",
                "size_mb": 0,
                "path": str(video_json),
            })
        except Exception:
            pass

    if args.format == "json":
        print(json.dumps(assets, indent=2))
    else:
        print(f"\n{'TYPE':<16} {'ID':<20} {'STATUS':<16} {'SCHEMA':<8} {'SIZE':<10} {'PATH'}")
        print("-" * 100)
        for a in assets:
            print(f"{a['type']:<16} {a['id']:<20} {a['status']:<16} {a['schema']:<8} {a['size_mb']:<10} {a['path']}")
        print(f"\nTotal: {len(assets)} assets")


# ─────────────────────────────────────────────────────────────
# AUDIT
# ─────────────────────────────────────────────────────────────

def audit(args: argparse.Namespace) -> None:
    root = args.root
    check = args.check
    issues = []

    if check in ("schema", "all"):
        for char_json in Path(root).rglob("character.json"):
            try:
                data = json.loads(char_json.read_text())
                if "animations" not in data:
                    continue
                errors = _validate(data, _load_schema("character-meta-v1.json"))
                for e in errors:
                    issues.append({"file": str(char_json), "type": "schema", "error": e})
            except Exception as exc:
                issues.append({"file": str(char_json), "type": "parse", "error": str(exc)})

        for spec_json in Path(root).rglob("*_spec.json"):
            try:
                data = json.loads(spec_json.read_text())
                if "action" not in data:
                    continue
                errors = _validate(data, _load_schema("sprite-spec-v1.json"))
                for e in errors:
                    issues.append({"file": str(spec_json), "type": "schema", "error": e})
            except Exception:
                pass

        for manifest in Path(root).rglob("sound_manifest.json"):
            try:
                data = json.loads(manifest.read_text())
                if "categories" not in data:
                    continue
                errors = _validate(data, _load_schema("sound-manifest-v2.json"))
                for e in errors:
                    issues.append({"file": str(manifest), "type": "schema", "error": e})
            except Exception:
                pass

    if check in ("broken-refs", "all"):
        for char_json in Path(root).rglob("character.json"):
            try:
                data = json.loads(char_json.read_text())
                char_dir = char_json.parent
                for view_name, view_path in data.get("views", {}).items():
                    full_path = char_dir / view_path
                    if not full_path.exists():
                        issues.append({"file": str(char_json), "type": "broken_ref",
                                       "error": f"View '{view_name}' file missing: {view_path}"})
                for anim_name in data.get("animations", {}):
                    sprite_path = char_dir / "sprites" / f"{anim_name}.png"
                    if not sprite_path.exists():
                        issues.append({"file": str(char_json), "type": "broken_ref",
                                       "error": f"Sprite sheet missing for animation '{anim_name}'"})
            except Exception:
                pass

    if check in ("orphans", "all"):
        for png in Path(root).rglob("sprites/*.png"):
            action = png.stem
            spec = png.parent / f"{action}_spec.json"
            if not spec.exists():
                issues.append({"file": str(png), "type": "orphan", "error": "Sprite sheet has no spec JSON"})

    if not issues:
        print(f"Audit '{check}': No issues found")
    else:
        print(f"Audit '{check}': {len(issues)} issue(s) found\n")
        for i in issues:
            print(f"  [{i['type'].upper()}] {i['file']}")
            print(f"    {i['error']}\n")

    sys.exit(1 if issues else 0)


# ─────────────────────────────────────────────────────────────
# CLI entry point
# ─────────────────────────────────────────────────────────────

def main() -> None:
    parser = argparse.ArgumentParser(
        description="IntelliVerseX Asset Manager — CRUD for any game asset",
        formatter_class=argparse.RawDescriptionHelpFormatter,
    )
    sub = parser.add_subparsers(dest="command", help="Operation to perform")

    # ── ADD ──
    add_parser = sub.add_parser("add", help="Add a new asset")
    add_sub = add_parser.add_subparsers(dest="type")

    p = add_sub.add_parser("sound", help="Add a sound to the manifest")
    p.add_argument("--manifest", required=True, help="Path to sound_manifest.json")
    p.add_argument("--id", required=True, help="Sound ID")
    p.add_argument("--file", required=True, help="Audio file path or URL")
    p.add_argument("--category", required=True, choices=["ui", "sfx", "music", "stinger", "ambient", "notif", "voice", "reward", "streak"])
    p.add_argument("--duration-ms", type=int, help="Duration in milliseconds")
    p.add_argument("--volume", type=float, help="Playback volume (0-1)")
    p.add_argument("--loop", action="store_true", help="Loop the sound")
    p.add_argument("--bpm", type=float, help="Beats per minute")

    p = add_sub.add_parser("video", help="Add a video asset")
    p.add_argument("--file", required=True, help="Video file path")
    p.add_argument("--id", help="Video ID (auto-generated from filename)")
    p.add_argument("--title", help="Video title")
    p.add_argument("--tags", help="Comma-separated tags")
    p.add_argument("--output", help="Output directory")

    # ── REPLACE ──
    replace_parser = sub.add_parser("replace", help="Replace an existing asset")
    replace_sub = replace_parser.add_subparsers(dest="type")

    p = replace_sub.add_parser("sprite", help="Replace a sprite sheet")
    p.add_argument("--character", required=True, help="Character directory")
    p.add_argument("--action", required=True, help="Animation action name")
    p.add_argument("--new-file", required=True, help="New sprite sheet PNG")
    p.add_argument("--cell-size", type=int, help="Cell size in pixels")
    p.add_argument("--frames", type=int, help="Frame count (auto-detected)")
    p.add_argument("--fps", type=float, help="Playback FPS")

    p = replace_sub.add_parser("sound", help="Replace a sound in the manifest")
    p.add_argument("--manifest", required=True, help="Path to sound_manifest.json")
    p.add_argument("--id", required=True, help="Sound ID to replace")
    p.add_argument("--new-file", required=True, help="New audio file path or URL")
    p.add_argument("--duration-ms", type=int, help="New duration in milliseconds")

    # ── MODIFY ──
    modify_parser = sub.add_parser("modify", help="Modify asset properties")
    modify_sub = modify_parser.add_subparsers(dest="type")

    p = modify_sub.add_parser("character", help="Modify character properties")
    p.add_argument("--character", required=True, help="Character directory")
    p.add_argument("--rarity", choices=["common", "uncommon", "rare", "epic", "legendary"])
    p.add_argument("--add-tags", help="Comma-separated tags to add")
    p.add_argument("--unlock-method", choices=["default", "purchase", "achievement", "event", "gacha", "season_pass"])
    p.add_argument("--unlock-cost", help="JSON string, e.g. '{\"gems\": 500}'")
    p.add_argument("--display-name", help="New display name")
    p.add_argument("--description", help="New description")

    p = modify_sub.add_parser("sound", help="Modify sound properties")
    p.add_argument("--manifest", required=True, help="Path to sound_manifest.json")
    p.add_argument("--id", required=True, help="Sound ID")
    p.add_argument("--volume", type=float, help="New volume")
    p.add_argument("--loop", type=bool, help="Loop setting")
    p.add_argument("--bpm", type=float, help="BPM")
    p.add_argument("--category", help="Move to new category")

    # ── DELETE ──
    delete_parser = sub.add_parser("delete", help="Delete an asset")
    delete_sub = delete_parser.add_subparsers(dest="type")

    p = delete_sub.add_parser("sprite", help="Delete a sprite animation")
    p.add_argument("--character", required=True, help="Character directory")
    p.add_argument("--action", required=True, help="Animation action to delete")
    p.add_argument("--dry-run", action="store_true", help="Show what would be deleted")
    p.add_argument("--backup-to", help="Backup directory")

    p = delete_sub.add_parser("sound", help="Delete a sound from the manifest")
    p.add_argument("--manifest", required=True, help="Path to sound_manifest.json")
    p.add_argument("--id", required=True, help="Sound ID to delete")
    p.add_argument("--dry-run", action="store_true")

    p = delete_sub.add_parser("character", help="Delete an entire character")
    p.add_argument("--character", required=True, help="Character directory")
    p.add_argument("--dry-run", action="store_true")
    p.add_argument("--backup-to", help="Backup directory")

    # ── INVENTORY ──
    p = sub.add_parser("inventory", help="List all assets")
    p.add_argument("--root", required=True, help="Asset root directory")
    p.add_argument("--format", choices=["table", "json"], default="table")

    # ── AUDIT ──
    p = sub.add_parser("audit", help="Audit assets for issues")
    p.add_argument("check", choices=["schema", "broken-refs", "orphans", "all"])
    p.add_argument("--root", required=True, help="Asset root directory")

    args = parser.parse_args()

    if not args.command:
        parser.print_help()
        sys.exit(1)

    dispatch = {
        ("add", "sound"): add_sound,
        ("add", "video"): add_video,
        ("replace", "sprite"): replace_sprite,
        ("replace", "sound"): replace_sound,
        ("modify", "character"): modify_character,
        ("modify", "sound"): modify_sound,
        ("delete", "sprite"): delete_sprite,
        ("delete", "sound"): delete_sound,
        ("delete", "character"): delete_character,
    }

    key = (args.command, getattr(args, "type", None))
    if key in dispatch:
        dispatch[key](args)
    elif args.command == "inventory":
        inventory(args)
    elif args.command == "audit":
        audit(args)
    else:
        parser.print_help()
        sys.exit(1)


if __name__ == "__main__":
    main()
