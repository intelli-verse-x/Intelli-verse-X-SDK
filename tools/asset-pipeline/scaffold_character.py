#!/usr/bin/env python3
"""
IntelliVerseX Character Scaffolder
=====================================
Creates a complete character folder structure with template specs,
placeholder PNGs, and character.json — ready for artists to fill in.

Usage:
    python scaffold_character.py --name Quizzy --output RemoteAssets/Characters/
    python scaffold_character.py --name DragonLord --rarity epic --tier 3 --with-extended

Requires: Pillow (pip install Pillow)
"""

import argparse
import json
import os
import shutil
import sys
from pathlib import Path

try:
    from PIL import Image, ImageDraw, ImageFont
except ImportError:
    print("ERROR: Pillow is required. Install with: pip install Pillow")
    sys.exit(1)

TEMPLATES_DIR = Path(__file__).parent / "templates"

MANDATORY_ANIMS = {
    "idle":   {"frames": 6, "fps": 10, "cols": 3, "rows": 2},
    "jump":   {"frames": 4, "fps": 12, "cols": 2, "rows": 2},
    "hurt":   {"frames": 4, "fps": 10, "cols": 2, "rows": 2},
}
STANDARD_ANIMS = {
    "walk":   {"frames": 8, "fps": 10, "cols": 4, "rows": 2},
    "run":    {"frames": 6, "fps": 14, "cols": 3, "rows": 2},
    "attack": {"frames": 5, "fps": 14, "cols": 3, "rows": 2},
}
EXTENDED_ANIMS = {
    "dance":   {"frames": 8, "fps": 10, "cols": 4, "rows": 2},
    "wave":    {"frames": 4, "fps": 10, "cols": 2, "rows": 2},
    "think":   {"frames": 4, "fps": 8,  "cols": 2, "rows": 2},
    "sleep":   {"frames": 4, "fps": 6,  "cols": 2, "rows": 2},
    "special": {"frames": 6, "fps": 12, "cols": 3, "rows": 2},
    "death":   {"frames": 4, "fps": 10, "cols": 2, "rows": 2},
    "spawn":   {"frames": 4, "fps": 12, "cols": 2, "rows": 2},
}


def create_placeholder_png(path: Path, width: int, height: int, label: str):
    """Generate a placeholder PNG with a label and grid pattern."""
    bg_color = (40, 40, 60, 255)
    grid_color = (80, 80, 120, 255)
    text_color = (200, 200, 255, 255)

    img = Image.new("RGBA", (width, height), bg_color)
    draw = ImageDraw.Draw(img)

    grid_step = 64
    for x in range(0, width, grid_step):
        draw.line([(x, 0), (x, height)], fill=grid_color, width=1)
    for y in range(0, height, grid_step):
        draw.line([(0, y), (width, y)], fill=grid_color, width=1)

    try:
        font = ImageFont.truetype("/System/Library/Fonts/Helvetica.ttc", 24)
    except (IOError, OSError):
        try:
            font = ImageFont.truetype("/usr/share/fonts/truetype/dejavu/DejaVuSans.ttf", 24)
        except (IOError, OSError):
            font = ImageFont.load_default()

    bbox = draw.textbbox((0, 0), label, font=font)
    tw = bbox[2] - bbox[0]
    th = bbox[3] - bbox[1]
    tx = (width - tw) // 2
    ty = (height - th) // 2
    draw.text((tx, ty), label, fill=text_color, font=font)

    size_text = f"{width}×{height}"
    bbox2 = draw.textbbox((0, 0), size_text, font=font)
    stw = bbox2[2] - bbox2[0]
    draw.text(((width - stw) // 2, ty + th + 10), size_text, fill=(150, 150, 180, 255), font=font)

    img.save(str(path), "PNG")


def create_placeholder_sheet(path: Path, cell_size: int, cols: int, rows: int, anim_name: str):
    """Generate a placeholder sprite sheet with numbered frame cells."""
    w = cols * cell_size
    h = rows * cell_size
    bg_color = (30, 30, 50, 255)
    cell_border = (100, 100, 160, 255)
    text_color = (200, 200, 255, 255)

    img = Image.new("RGBA", (w, h), bg_color)
    draw = ImageDraw.Draw(img)

    try:
        font = ImageFont.truetype("/System/Library/Fonts/Helvetica.ttc", max(12, cell_size // 8))
    except (IOError, OSError):
        try:
            font = ImageFont.truetype("/usr/share/fonts/truetype/dejavu/DejaVuSans.ttf", max(12, cell_size // 8))
        except (IOError, OSError):
            font = ImageFont.load_default()

    for r in range(rows):
        for c in range(cols):
            x0 = c * cell_size
            y0 = r * cell_size
            x1 = x0 + cell_size - 1
            y1 = y0 + cell_size - 1
            draw.rectangle([x0, y0, x1, y1], outline=cell_border, width=2)

            frame_num = r * cols + c
            label = f"{anim_name}\nF{frame_num}"
            bbox = draw.textbbox((0, 0), label, font=font)
            tw = bbox[2] - bbox[0]
            th = bbox[3] - bbox[1]
            tx = x0 + (cell_size - tw) // 2
            ty = y0 + (cell_size - th) // 2
            draw.text((tx, ty), label, fill=text_color, font=font, align="center")

    img.save(str(path), "PNG")


def scaffold(name: str, output_dir: str, rarity: str, tier: int,
             with_standard: bool, with_extended: bool):
    char_dir = Path(output_dir) / name
    sprites_dir = char_dir / "sprites"
    sprites_dir.mkdir(parents=True, exist_ok=True)

    print(f"Scaffolding character: {name}")
    print(f"Output: {char_dir}")

    create_placeholder_png(char_dir / "front.png", 512, 512, f"{name}\nFRONT")
    create_placeholder_png(char_dir / "thumbnail.png", 128, 128, name[:6])
    create_placeholder_png(char_dir / "back.png", 512, 512, f"{name}\nBACK")
    print(f"  Created: front.png, back.png, thumbnail.png")

    anims_to_create = dict(MANDATORY_ANIMS)
    if with_standard:
        anims_to_create.update(STANDARD_ANIMS)
    if with_extended:
        anims_to_create.update(EXTENDED_ANIMS)

    character_meta = {
        "id": name,
        "display_name": name,
        "description": f"Replace with {name}'s description.",
        "rarity": rarity,
        "unlock_method": "default",
        "unlock_cost": {},
        "tier": tier,
        "tags": [],
        "views": {
            "front": "front.png",
            "back": "back.png",
            "thumbnail": "thumbnail.png"
        },
        "animations": {},
        "sounds": {
            "on_select": "sfx_character_select",
            "on_hurt": "sfx_character_hurt",
            "on_victory": "stinger_victory"
        }
    }

    for anim_name, info in anims_to_create.items():
        frames = info["frames"]
        fps = info["fps"]
        cols = info["cols"]
        rows = info["rows"]

        template_spec = TEMPLATES_DIR / f"{anim_name}_spec.json"
        if template_spec.exists():
            shutil.copy2(template_spec, sprites_dir / f"{anim_name}_spec.json")
        else:
            spec = {
                "action": anim_name,
                "frames": frames,
                "cell_size": 512,
                "fps": fps,
                "loop": anim_name in ("idle", "walk", "run", "dance", "sleep"),
                "ping_pong": False,
                "layout": {"columns": cols, "rows": rows},
                "events": [],
                "sound_sync": {},
                "tags": []
            }
            with open(sprites_dir / f"{anim_name}_spec.json", "w") as f:
                json.dump(spec, f, indent=2)

        create_placeholder_sheet(
            sprites_dir / f"{anim_name}.png",
            512, cols, rows, anim_name
        )

        anim_tier = ("mandatory" if anim_name in MANDATORY_ANIMS
                      else "standard" if anim_name in STANDARD_ANIMS
                      else "extended")
        character_meta["animations"][anim_name] = {
            "tier": anim_tier,
            "frames": frames,
            "fps": fps
        }

        print(f"  Created: sprites/{anim_name}.png + {anim_name}_spec.json ({anim_tier})")

    with open(char_dir / "character.json", "w") as f:
        json.dump(character_meta, f, indent=2)
    print(f"  Created: character.json")

    total = len(anims_to_create)
    m = sum(1 for a in anims_to_create if a in MANDATORY_ANIMS)
    s = sum(1 for a in anims_to_create if a in STANDARD_ANIMS)
    e = sum(1 for a in anims_to_create if a in EXTENDED_ANIMS)
    print(f"\nDone! {total} animations scaffolded ({m} mandatory, {s} standard, {e} extended)")
    print(f"Next step: Replace placeholder PNGs with real artwork, then run:")
    print(f"  python validate_character.py --character {char_dir}")


def main():
    parser = argparse.ArgumentParser(description="Scaffold a new character folder with templates.")
    parser.add_argument("--name", required=True,
                        help="Character name (PascalCase, e.g. Quizzy)")
    parser.add_argument("--output", default=".",
                        help="Parent directory for character folder (default: current dir)")
    parser.add_argument("--rarity", default="common",
                        choices=["common", "uncommon", "rare", "epic", "legendary"],
                        help="Character rarity tier")
    parser.add_argument("--tier", type=int, default=1,
                        help="Numeric tier for sorting")
    parser.add_argument("--with-standard", action="store_true", default=True,
                        help="Include Tier 2 (standard) animations (default: true)")
    parser.add_argument("--no-standard", action="store_true",
                        help="Skip Tier 2 animations (mandatory only)")
    parser.add_argument("--with-extended", action="store_true",
                        help="Include Tier 3 (extended) animations")

    args = parser.parse_args()
    standard = not args.no_standard

    scaffold(
        name=args.name,
        output_dir=args.output,
        rarity=args.rarity,
        tier=args.tier,
        with_standard=standard,
        with_extended=args.with_extended,
    )


if __name__ == "__main__":
    main()
