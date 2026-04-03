#!/usr/bin/env python3
"""
gdd_to_entity.py — Convert structured GDD Markdown sections into
Content-Factory entity JSON files (brand_entity.json + game_context.json).

Reads the design/ directory produced by the ivx-game-design-studio skill
and outputs JSON ready for S3 upload or local Content-Factory consumption.

Usage:
    python tools/asset-pipeline/gdd_to_entity.py \
        --gdd-dir design/ \
        --output-dir output/entities/ \
        --brand-id my-studio \
        --game-id my-game

    python tools/asset-pipeline/gdd_to_entity.py \
        --gdd-dir design/ \
        --output-dir output/entities/ \
        --brand-id my-studio \
        --game-id my-game \
        --upload-s3 s3://intelli-verse-x-media/agent-assets/
"""

from __future__ import annotations

import argparse
import json
import os
import re
import sys
from pathlib import Path
from typing import Any


def _parse_field(line: str) -> tuple[str, str] | None:
    """Extract key-value from '- **Key:** Value' markdown lines."""
    m = re.match(r"^-\s+\*\*(.+?)\*\*:\s*(.+)$", line.strip())
    if m:
        return m.group(1).strip(), m.group(2).strip()
    return None


def _parse_list_value(raw: str) -> list[str] | str:
    """If value looks like [a, b, c] or a, b, c return a list."""
    raw = raw.strip()
    if raw.startswith("[") and raw.endswith("]"):
        raw = raw[1:-1]
    if "," in raw:
        return [item.strip().strip("'\"") for item in raw.split(",") if item.strip()]
    return raw


def _parse_section(text: str, heading: str) -> dict[str, Any]:
    """Extract fields from a specific ## heading section."""
    pattern = rf"^##\s+{re.escape(heading)}\s*$"
    lines = text.splitlines()
    in_section = False
    fields: dict[str, Any] = {}
    for line in lines:
        if re.match(pattern, line.strip()):
            in_section = True
            continue
        if in_section and re.match(r"^##\s+", line.strip()):
            break
        if in_section:
            parsed = _parse_field(line)
            if parsed:
                key, val = parsed
                fields[key] = val
    return fields


def _parse_table(text: str, heading: str) -> list[dict[str, str]]:
    """Parse a markdown table under a ## heading into a list of dicts."""
    pattern = rf"^##\s+{re.escape(heading)}\s*$"
    lines = text.splitlines()
    in_section = False
    table_lines: list[str] = []
    for line in lines:
        if re.match(pattern, line.strip()):
            in_section = True
            continue
        if in_section and re.match(r"^##\s+", line.strip()):
            break
        if in_section and "|" in line:
            table_lines.append(line.strip())

    if len(table_lines) < 3:
        return []

    headers = [h.strip() for h in table_lines[0].split("|") if h.strip()]
    rows: list[dict[str, str]] = []
    for row_line in table_lines[2:]:
        cells = [c.strip() for c in row_line.split("|") if c.strip()]
        if len(cells) >= len(headers):
            rows.append(dict(zip(headers, cells)))
    return rows


def _safe_read(path: Path) -> str:
    if path.exists():
        return path.read_text(encoding="utf-8")
    return ""


def _slug(name: str) -> str:
    return re.sub(r"[^a-z0-9_]", "", name.lower().replace(" ", "_").replace("-", "_"))


def parse_brand_identity(gdd_dir: Path) -> dict[str, Any]:
    text = _safe_read(gdd_dir / "brand-identity.md")
    if not text:
        return {}

    studio = _parse_section(text, "Studio")
    audience = _parse_section(text, "Audience")
    visual = _parse_section(text, "Visual Identity")

    result: dict[str, Any] = {}
    if "Brand ID" in studio:
        result["brand_id"] = studio["Brand ID"]
    if "Studio Name" in studio:
        result["name"] = studio["Studio Name"]
    for field in ("Tagline", "Mission", "Personality", "Primary Emotion"):
        key = _slug(field)
        if key == "primary_emotion":
            key = "primary_emotion"
        if field in studio:
            result[key] = studio[field]

    if "Target Audience" in audience:
        result["target_audience"] = audience["Target Audience"]
    if "Age Range" in audience:
        result["age_range"] = audience["Age Range"]
    if "Geographic Markets" in audience:
        result["geographic_markets"] = _parse_list_value(audience["Geographic Markets"])
    if "Languages" in audience:
        result["languages"] = _parse_list_value(audience["Languages"])

    if "Imagery Style" in visual:
        result["imagery_style"] = visual["Imagery Style"]

    return result


def parse_store_metadata(gdd_dir: Path) -> dict[str, Any]:
    text = _safe_read(gdd_dir / "store-metadata.md")
    if not text:
        return {}

    ids = _parse_section(text, "Identifiers")
    descs = _parse_section(text, "Descriptions")
    kw = _parse_section(text, "Keywords / Tags")
    ratings = _parse_section(text, "Ratings")
    pricing = _parse_section(text, "Pricing")

    store_config: dict[str, Any] = {}
    if "iOS Bundle ID" in ids:
        store_config["ios_bundle_id"] = ids["iOS Bundle ID"]
    if "Android Package" in ids:
        store_config["android_package"] = ids["Android Package"]
    if "Steam App ID" in ids:
        val = ids["Steam App ID"]
        store_config["steam_app_id"] = None if "pending" in val.lower() else val
    if "Short Description (80 chars)" in descs:
        store_config["short_description"] = descs["Short Description (80 chars)"]
    elif "Short Description" in descs:
        store_config["short_description"] = descs["Short Description"]
    if "iOS Keywords (100 chars)" in kw:
        store_config["keywords"] = [k.strip() for k in kw["iOS Keywords (100 chars)"].split(",")]
    elif "Google Play Tags" in kw:
        store_config["keywords"] = [k.strip() for k in kw["Google Play Tags"].split(",")]
    if "ESRB" in ratings:
        rating_val = ratings["ESRB"]
        if "E " in rating_val or rating_val.strip() == "E":
            store_config["content_rating"] = "everyone"
        elif "T " in rating_val or rating_val.strip() == "T":
            store_config["content_rating"] = "teen"
        elif "M " in rating_val or rating_val.strip() == "M":
            store_config["content_rating"] = "mature"
        else:
            store_config["content_rating"] = rating_val.strip().lower()

    game_name = descs.get("App Name", "")
    short_desc = store_config.get("short_description", "")
    full_desc = descs.get("Full Description", "")

    return {
        "store_config": store_config,
        "game_name": game_name,
        "short_description": short_desc,
        "full_description": full_desc,
    }


def parse_localization_plan(gdd_dir: Path) -> dict[str, Any]:
    text = _safe_read(gdd_dir / "localization-plan.md")
    if not text:
        return {}

    rows = _parse_table(text, "Target Languages")
    languages = []
    for row in rows:
        locale = row.get("Locale", "").strip()
        if locale:
            languages.append(locale.split("-")[0] if "-" in locale else locale)

    rtl_section = _parse_section(text, "RTL Support")
    rtl_required = "yes" in rtl_section.get("Required", "").lower()

    return {
        "languages": languages,
        "rtl_required": rtl_required,
        "localization_tiers": rows,
    }


def parse_character_bible(char_file: Path) -> dict[str, Any]:
    text = _safe_read(char_file)
    if not text:
        return {}

    identity = _parse_section(text, "Identity")
    appearance = _parse_section(text, "Appearance (drives sprite/3D generation)")
    if not appearance:
        appearance = _parse_section(text, "Appearance")
    personality = _parse_section(text, "Personality")
    voice = _parse_section(text, "Voice (drives TTS / voice line generation)")
    if not voice:
        voice = _parse_section(text, "Voice")
    relationships_section = _parse_section(text, "Relationships")
    game_section = _parse_section(text, "Game Presence")

    char: dict[str, Any] = {}
    if "Character ID" in identity:
        char["character_id"] = identity["Character ID"]
    if "Display Name" in identity:
        char["name"] = identity["Display Name"]
    elif "Character ID" in identity:
        char["name"] = identity["Character ID"].replace("_", " ").title()
    if "Role" in identity:
        char["role"] = identity["Role"].split("|")[0].strip()
    if "Species" in identity:
        char["species"] = identity["Species"].split("|")[0].strip()
    if "Age Group" in identity:
        char["age_group"] = identity["Age Group"].split("|")[0].strip()
    if "Gender" in identity:
        char["gender"] = identity["Gender"].split("|")[0].strip()

    if "Full Description" in appearance:
        char["appearance"] = appearance["Full Description"]
    if "Static Features" in appearance:
        char["static_features"] = appearance["Static Features"]
    if "Dynamic Features" in appearance:
        char["dynamic_features"] = appearance["Dynamic Features"]

    if "Personality" in personality:
        char["personality"] = personality["Personality"]
    if "Catchphrase" in personality:
        char["catchphrase"] = personality["Catchphrase"].strip('"')
    if "Traits" in personality:
        char["traits"] = _parse_list_value(personality["Traits"])
    if "Goals" in personality:
        char["goals"] = personality["Goals"]
    if "Fears" in personality:
        char["fears"] = personality["Fears"]
    if "Backstory" in personality:
        char["backstory"] = personality["Backstory"]

    if "Voice Description" in voice:
        char["voice_description"] = voice["Voice Description"]

    if relationships_section:
        rels: dict[str, str] = {}
        for k, v in relationships_section.items():
            rels[k] = v
        if rels:
            char["relationships"] = rels

    if "Appears In Games" in game_section:
        char["appears_in_games"] = _parse_list_value(game_section["Appears In Games"])
    if "Aliases" in game_section:
        char["aliases"] = _parse_list_value(game_section["Aliases"])
    if "Is Visible" in game_section:
        char["is_visible"] = game_section["Is Visible"].lower() == "true"
    if "Identifier In Scene" in game_section:
        char["identifier_in_scene"] = game_section["Identifier In Scene"]

    char["description"] = char.get("appearance", char.get("personality", ""))
    char["portrait_urls"] = {}

    return char


def parse_media_requirements(gdd_dir: Path) -> dict[str, Any]:
    text = _safe_read(gdd_dir / "media-requirements.md")
    if not text:
        return {}

    sound = _parse_section(text, "Sound Config (drives ivx-game-audio-factory)")
    if not sound:
        sound = _parse_section(text, "Sound Config")

    result: dict[str, Any] = {}
    sound_config: dict[str, Any] = {}
    if "Genre Preset" in sound:
        sound_config["genre_preset"] = sound["Genre Preset"]
    if "BGM BPM Range" in sound:
        bpm = sound["BGM BPM Range"]
        parts = re.findall(r"\d+", bpm)
        if len(parts) == 2:
            sound_config["bgm_bpm_range"] = [int(parts[0]), int(parts[1])]
    if "SFX Style" in sound:
        sound_config["sfx_style"] = sound["SFX Style"]
    if sound_config:
        result["sound_config"] = sound_config

    return result


def parse_game_concept(gdd_dir: Path) -> dict[str, Any]:
    text = _safe_read(gdd_dir / "gdd" / "game-concept.md")
    if not text:
        return {}

    fields: dict[str, Any] = {}
    for section_name in ("Overview", "Core Loop", "Genre", "Platforms", "Key Features"):
        section = _parse_section(text, section_name)
        fields.update(section)

    result: dict[str, Any] = {}
    if "Genre" in fields:
        result["genre"] = fields["Genre"].lower()
    if "Platforms" in fields:
        result["platforms"] = _parse_list_value(fields["Platforms"])

    return result


def build_brand_entity(
    brand_data: dict,
    characters: list[dict],
    localization: dict,
    brand_id: str,
    game_id: str,
    game_concept: dict,
    store_data: dict,
) -> dict[str, Any]:
    entity: dict[str, Any] = {
        "brand_id": brand_data.get("brand_id", brand_id),
        "name": brand_data.get("name", brand_id.replace("-", " ").title()),
        "tagline": brand_data.get("tagline", ""),
        "description": brand_data.get("description", ""),
        "mission": brand_data.get("mission", ""),
        "personality": brand_data.get("personality", "playful"),
        "primary_emotion": brand_data.get("primary_emotion", "joy"),
        "business_type": "gaming",
        "target_audience": brand_data.get("target_audience", ""),
        "age_range": brand_data.get("age_range", ""),
        "languages": localization.get("languages", brand_data.get("languages", ["en"])),
        "geographic_markets": brand_data.get("geographic_markets", ["US"]),
        "imagery_style": brand_data.get("imagery_style", ""),
        "games": [
            {
                "game_id": game_id,
                "name": store_data.get("game_name", game_id.replace("-", " ").title()),
                "description": store_data.get("short_description", ""),
                "genre": game_concept.get("genre", ""),
                "platforms": game_concept.get("platforms", []),
                "visual_style": brand_data.get("imagery_style", ""),
                "characters": [c.get("character_id", "") for c in characters],
                "target_audience": brand_data.get("target_audience", ""),
                "age_rating": "E",
                "status": "active",
            }
        ],
        "characters": characters,
        "status": "active",
        "version": 1,
        "tags": ["gaming"],
    }
    return entity


def build_game_context(
    brand_id: str,
    game_id: str,
    store_data: dict,
    game_concept: dict,
    media_data: dict,
    characters: list[dict],
    brand_data: dict,
) -> dict[str, Any]:
    context: dict[str, Any] = {
        "game_id": game_id,
        "name": store_data.get("game_name", game_id.replace("-", " ").title()),
        "description": store_data.get("short_description", ""),
        "brand": brand_data.get("brand_id", brand_id),
        "genre": game_concept.get("genre", ""),
        "platforms": game_concept.get("platforms", []),
        "visual_style": brand_data.get("imagery_style", ""),
        "gameplay_summary": "",
        "target_audience": brand_data.get("target_audience", ""),
        "age_rating": "E",
        "key_features": [],
        "unique_selling_points": [],
        "content_themes": [],
        "characters": [
            {"id": c.get("character_id", ""), "name": c.get("name", ""), "role": c.get("role", "")}
            for c in characters
        ],
        "media": {"icon": {"primary": ""}},
        "sound_config": media_data.get("sound_config", {}),
        "store_config": store_data.get("store_config", {}),
    }
    return context


def generate_report(
    brand_entity: dict, game_context: dict, warnings: list[str]
) -> str:
    lines = [
        "# GDD-to-Entity Export Report",
        "",
        f"**Brand:** {brand_entity.get('name', 'Unknown')} (`{brand_entity.get('brand_id', '')}`)",
        f"**Game:** {game_context.get('name', 'Unknown')} (`{game_context.get('game_id', '')}`)",
        f"**Characters:** {len(brand_entity.get('characters', []))}",
        f"**Languages:** {', '.join(brand_entity.get('languages', []))}",
        f"**Platforms:** {', '.join(game_context.get('platforms', []))}",
        "",
    ]
    if warnings:
        lines.append("## Warnings")
        lines.append("")
        for w in warnings:
            lines.append(f"- {w}")
        lines.append("")

    lines.append("## Extracted Fields")
    lines.append("")
    lines.append(f"- Brand entity fields: {len(brand_entity)}")
    lines.append(f"- Game context fields: {len(game_context)}")
    lines.append(f"- Store config fields: {len(game_context.get('store_config', {}))}")
    lines.append(f"- Sound config fields: {len(game_context.get('sound_config', {}))}")
    lines.append("")
    lines.append("## Next Steps")
    lines.append("")
    lines.append("1. Review the generated JSON files for accuracy")
    lines.append("2. Upload to S3: `aws s3 cp output/entities/ s3://intelli-verse-x-media/agent-assets/{brand_id}/ --recursive`")
    lines.append("3. Run Content-Factory: `python -m pipelines.runner run --config configs/pipelines/ivx_full_game.yaml --brand_id {brand_id} --game_id {game_id}`")

    return "\n".join(lines)


def main() -> None:
    parser = argparse.ArgumentParser(
        description="Convert structured GDD Markdown into Content-Factory entity JSON"
    )
    parser.add_argument("--gdd-dir", required=True, help="Path to design/ directory containing GDD files")
    parser.add_argument("--output-dir", required=True, help="Output directory for JSON files")
    parser.add_argument("--brand-id", required=True, help="Brand identifier (lowercase, hyphenated)")
    parser.add_argument("--game-id", required=True, help="Game identifier (lowercase, hyphenated)")
    parser.add_argument("--upload-s3", default=None, help="S3 base URI to upload (e.g. s3://bucket/prefix/)")
    parser.add_argument("--dry-run", action="store_true", help="Print output without writing files")
    args = parser.parse_args()

    gdd_dir = Path(args.gdd_dir)
    output_dir = Path(args.output_dir)
    warnings: list[str] = []

    if not gdd_dir.exists():
        print(f"ERROR: GDD directory not found: {gdd_dir}", file=sys.stderr)
        sys.exit(1)

    brand_data = parse_brand_identity(gdd_dir)
    if not brand_data:
        warnings.append("brand-identity.md not found or empty — using defaults")

    store_data = parse_store_metadata(gdd_dir)
    if not store_data:
        warnings.append("store-metadata.md not found or empty — store_config will be minimal")

    localization = parse_localization_plan(gdd_dir)
    if not localization:
        warnings.append("localization-plan.md not found — defaulting to English only")

    media_data = parse_media_requirements(gdd_dir)
    if not media_data:
        warnings.append("media-requirements.md not found — sound_config will be empty")

    game_concept = parse_game_concept(gdd_dir)
    if not game_concept:
        warnings.append("gdd/game-concept.md not found — genre and platforms will be empty")

    characters: list[dict] = []
    char_dir = gdd_dir / "gdd" / "characters"
    if char_dir.exists():
        for char_file in sorted(char_dir.glob("*.md")):
            char = parse_character_bible(char_file)
            if char and char.get("character_id"):
                characters.append(char)
                print(f"  Parsed character: {char.get('name', char.get('character_id'))}")
    if not characters:
        warnings.append("No character bibles found in gdd/characters/ — characters list will be empty")

    brand_entity = build_brand_entity(
        brand_data, characters, localization, args.brand_id, args.game_id, game_concept, store_data
    )
    game_context = build_game_context(
        args.brand_id, args.game_id, store_data, game_concept, media_data, characters, brand_data
    )

    if args.dry_run:
        print("\n=== brand_entity.json ===")
        print(json.dumps(brand_entity, indent=2))
        print("\n=== game_context.json ===")
        print(json.dumps(game_context, indent=2))
        if warnings:
            print("\n=== Warnings ===")
            for w in warnings:
                print(f"  - {w}")
        return

    output_dir.mkdir(parents=True, exist_ok=True)

    brand_path = output_dir / "brand_entity.json"
    brand_path.write_text(json.dumps(brand_entity, indent=2, ensure_ascii=False), encoding="utf-8")
    print(f"Wrote {brand_path}")

    game_path = output_dir / "game_context.json"
    game_path.write_text(json.dumps(game_context, indent=2, ensure_ascii=False), encoding="utf-8")
    print(f"Wrote {game_path}")

    report = generate_report(brand_entity, game_context, warnings)
    report_path = output_dir / "export_report.md"
    report_path.write_text(report, encoding="utf-8")
    print(f"Wrote {report_path}")

    if args.upload_s3:
        base = args.upload_s3.rstrip("/")
        brand_s3 = f"{base}/{args.brand_id}/brand_entity.json"
        game_s3 = f"{base}/{args.brand_id}/{args.game_id}/game.json"
        print(f"\nUploading to S3...")
        os.system(f'aws s3 cp "{brand_path}" "{brand_s3}"')
        os.system(f'aws s3 cp "{game_path}" "{game_s3}"')
        print("S3 upload complete.")

    if warnings:
        print(f"\n{len(warnings)} warning(s) — see export_report.md for details")

    print("\nExport complete.")


if __name__ == "__main__":
    main()
