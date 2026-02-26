#!/usr/bin/env python3
"""
IVX Emoji Sprite Sheet Generator
--------------------------------
Generates atlas PNGs + JSON metadata from a folder of emoji PNG files.

Designed for production use with Unity TextMeshPro sprite assets.
"""

import argparse
import json
import math
import os
from dataclasses import dataclass
from pathlib import Path
from typing import Dict, List, Optional, Tuple

from PIL import Image

VARIATION_SELECTORS = {"fe0e", "fe0f"}
ZERO_WIDTH_JOINER = "200d"


@dataclass
class SpriteEntry:
    filename: str
    canonical_name: str
    name_with_zwj: str
    name_with_variation: str
    codepoints: List[str]
    x: int
    y: int
    width: int
    height: int
    atlas_index: int
    glyph_index: int


def normalize_parts(parts: List[str], keep_zwj: bool, keep_variation: bool) -> List[str]:
    out: List[str] = []
    for part in parts:
        low = part.lower()
        if not keep_variation and low in VARIATION_SELECTORS:
            continue
        if not keep_zwj and low == ZERO_WIDTH_JOINER:
            continue
        out.append(low)
    return out


def parse_filename(name: str) -> Optional[Tuple[List[str], str, str, str]]:
    stem = Path(name).stem.lower()
    if not stem:
        return None

    raw_parts = stem.split("-")
    for part in raw_parts:
        if not part:
            return None
        try:
            int(part, 16)
        except ValueError:
            return None

    canonical = "-".join(normalize_parts(raw_parts, keep_zwj=False, keep_variation=False))
    with_zwj = "-".join(normalize_parts(raw_parts, keep_zwj=True, keep_variation=False))
    with_variation = "-".join(normalize_parts(raw_parts, keep_zwj=True, keep_variation=True))
    return raw_parts, canonical, with_zwj, with_variation


def load_png_paths(source_dir: Path) -> List[Path]:
    files = sorted(source_dir.glob("*.png"))
    return files


def build_entries(
    png_paths: List[Path],
    sprite_size: int,
    atlas_size: int,
    atlas_prefix: str,
    output_dir: Path,
) -> List[SpriteEntry]:
    if not png_paths:
        return []

    output_dir.mkdir(parents=True, exist_ok=True)

    per_row = atlas_size // sprite_size
    per_atlas = per_row * per_row
    atlas_count = int(math.ceil(len(png_paths) / float(per_atlas)))
    all_entries: List[SpriteEntry] = []

    print(f"Found {len(png_paths)} png files")
    print(f"Atlas size: {atlas_size} x {atlas_size}")
    print(f"Sprite size: {sprite_size} x {sprite_size}")
    print(f"Sprites per atlas: {per_atlas}")
    print(f"Atlas count: {atlas_count}")

    glyph_index = 0
    for atlas_index in range(atlas_count):
        atlas = Image.new("RGBA", (atlas_size, atlas_size), (0, 0, 0, 0))
        local_entries: List[SpriteEntry] = []

        start = atlas_index * per_atlas
        end = min(start + per_atlas, len(png_paths))
        batch = png_paths[start:end]

        for local_i, png_path in enumerate(batch):
            parsed = parse_filename(png_path.name)
            if parsed is None:
                print(f"Skipping invalid filename: {png_path.name}")
                continue

            codepoints, canonical, with_zwj, with_variation = parsed
            if not canonical:
                print(f"Skipping empty canonical name: {png_path.name}")
                continue

            col = local_i % per_row
            row = local_i // per_row
            x = col * sprite_size
            y = atlas_size - (row + 1) * sprite_size

            with Image.open(png_path).convert("RGBA") as src:
                if src.size != (sprite_size, sprite_size):
                    src = src.resize((sprite_size, sprite_size), Image.Resampling.LANCZOS)
                atlas.paste(src, (x, y))

            entry = SpriteEntry(
                filename=png_path.stem,
                canonical_name=canonical,
                name_with_zwj=with_zwj,
                name_with_variation=with_variation,
                codepoints=codepoints,
                x=x,
                y=y,
                width=sprite_size,
                height=sprite_size,
                atlas_index=atlas_index,
                glyph_index=glyph_index,
            )
            local_entries.append(entry)
            all_entries.append(entry)
            glyph_index += 1

        atlas_png = output_dir / f"{atlas_prefix}_{atlas_index}.png"
        atlas_json = output_dir / f"{atlas_prefix}_{atlas_index}.json"
        atlas.save(atlas_png, "PNG", optimize=True)

        with atlas_json.open("w", encoding="utf-8") as handle:
            json.dump(
                {
                    "atlasIndex": atlas_index,
                    "atlasSize": atlas_size,
                    "spriteSize": sprite_size,
                    "spriteCount": len(local_entries),
                    "sprites": [entry_to_dict(item) for item in local_entries],
                },
                handle,
                indent=2,
            )

        print(f"Saved atlas: {atlas_png.name} ({len(local_entries)} sprites)")

    return all_entries


def entry_to_dict(entry: SpriteEntry) -> Dict:
    return {
        "filename": entry.filename,
        "name": entry.canonical_name,
        "aliases": {
            "withZwJ": entry.name_with_zwj,
            "withVariation": entry.name_with_variation,
        },
        "codepoints": entry.codepoints,
        "x": entry.x,
        "y": entry.y,
        "width": entry.width,
        "height": entry.height,
        "atlasIndex": entry.atlas_index,
        "glyphIndex": entry.glyph_index,
    }


def write_metadata(
    output_dir: Path,
    metadata_name: str,
    entries: List[SpriteEntry],
    sprite_size: int,
    atlas_size: int,
    atlas_prefix: str,
) -> None:
    metadata_path = output_dir / metadata_name
    with metadata_path.open("w", encoding="utf-8") as handle:
        json.dump(
            {
                "generator": "IVX Emoji Sprite Sheet Generator",
                "atlasPrefix": atlas_prefix,
                "atlasSize": atlas_size,
                "spriteSize": sprite_size,
                "totalSprites": len(entries),
                "sprites": [entry_to_dict(item) for item in entries],
            },
            handle,
            indent=2,
        )
    print(f"Saved metadata: {metadata_path.name}")


def parse_args() -> argparse.Namespace:
    parser = argparse.ArgumentParser(description="Generate IVX emoji atlases and metadata from PNG files.")
    parser.add_argument("--source", required=True, help="Input folder containing emoji PNG files.")
    parser.add_argument("--output", required=True, help="Output folder for atlases and metadata.")
    parser.add_argument("--sprite-size", type=int, default=72, choices=[72, 128], help="Output sprite size.")
    parser.add_argument("--atlas-size", type=int, default=4096, choices=[2048, 4096, 8192], help="Atlas texture size.")
    parser.add_argument("--atlas-prefix", default="IVXEmojiAtlas", help="Prefix for atlas files.")
    parser.add_argument("--metadata-name", default="IVXEmojiMetadata.json", help="Combined metadata output file name.")
    return parser.parse_args()


def main() -> int:
    args = parse_args()
    source_dir = Path(args.source).expanduser().resolve()
    output_dir = Path(args.output).expanduser().resolve()

    if not source_dir.exists() or not source_dir.is_dir():
        print(f"Source folder not found: {source_dir}")
        return 1

    png_paths = load_png_paths(source_dir)
    if not png_paths:
        print("No PNG files found in source folder.")
        return 1

    entries = build_entries(
        png_paths=png_paths,
        sprite_size=args.sprite_size,
        atlas_size=args.atlas_size,
        atlas_prefix=args.atlas_prefix,
        output_dir=output_dir,
    )

    if not entries:
        print("No valid sprites generated.")
        return 1

    write_metadata(
        output_dir=output_dir,
        metadata_name=args.metadata_name,
        entries=entries,
        sprite_size=args.sprite_size,
        atlas_size=args.atlas_size,
        atlas_prefix=args.atlas_prefix,
    )

    print("Done.")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
