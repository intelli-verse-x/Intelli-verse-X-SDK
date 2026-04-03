#!/usr/bin/env python3
"""
IntelliVerseX Sprite Sheet Generator
=====================================
Combines individual frame PNGs into a single sprite sheet with a companion _spec.json.

Usage:
    python generate_spritesheet.py --frames "frames/idle_*.png" --output idle --cell-size 512
    python generate_spritesheet.py --frames "frames/walk_*.png" --output walk --cell-size 256 --fps 12

Requires: Pillow (pip install Pillow)
"""

import argparse
import glob
import json
import math
import os
import re
import sys
from pathlib import Path

try:
    from PIL import Image
except ImportError:
    print("ERROR: Pillow is required. Install with: pip install Pillow")
    sys.exit(1)

MAX_SHEET_DIM = 4096


def natural_sort_key(path: str):
    """Sort filenames with embedded numbers naturally (frame_01, frame_02, ... frame_10)."""
    return [int(c) if c.isdigit() else c.lower() for c in re.split(r'(\d+)', os.path.basename(path))]


def calculate_layout(frame_count: int, cell_size: int) -> tuple[int, int]:
    """Pick columns × rows that fit within MAX_SHEET_DIM."""
    max_cols = MAX_SHEET_DIM // cell_size
    cols = min(frame_count, max_cols)
    rows = math.ceil(frame_count / cols)

    if rows * cell_size > MAX_SHEET_DIM:
        print(f"WARNING: Sheet height ({rows * cell_size}px) exceeds {MAX_SHEET_DIM}px limit. "
              f"Consider reducing cell_size or splitting the animation.")

    return cols, rows


def generate(frame_paths: list[str], output_name: str, cell_size: int,
             fps: float, action: str, output_dir: str, loop: bool, ping_pong: bool):
    frame_count = len(frame_paths)
    if frame_count == 0:
        print("ERROR: No frame files matched the pattern.")
        sys.exit(1)

    cols, rows = calculate_layout(frame_count, cell_size)
    sheet_w = cols * cell_size
    sheet_h = rows * cell_size

    print(f"Generating sprite sheet: {frame_count} frames, {cols}×{rows} grid, "
          f"{sheet_w}×{sheet_h}px sheet")

    sheet = Image.new("RGBA", (sheet_w, sheet_h), (0, 0, 0, 0))

    for i, fpath in enumerate(frame_paths):
        frame = Image.open(fpath).convert("RGBA")

        if frame.size != (cell_size, cell_size):
            frame = frame.resize((cell_size, cell_size), Image.Resampling.LANCZOS)

        col = i % cols
        row = i // cols
        x = col * cell_size
        y = row * cell_size
        sheet.paste(frame, (x, y))
        print(f"  Frame {i:3d} → ({col},{row}) = ({x},{y})")

    os.makedirs(output_dir, exist_ok=True)
    sheet_path = os.path.join(output_dir, f"{output_name}.png")
    sheet.save(sheet_path, "PNG", optimize=True)
    print(f"Sheet saved: {sheet_path} ({os.path.getsize(sheet_path) / 1024:.1f} KB)")

    spec = {
        "action": action or output_name,
        "frames": frame_count,
        "cell_size": cell_size,
        "fps": fps,
        "loop": loop,
        "ping_pong": ping_pong,
        "layout": {
            "columns": cols,
            "rows": rows
        }
    }

    spec_path = os.path.join(output_dir, f"{output_name}_spec.json")
    with open(spec_path, "w") as f:
        json.dump(spec, f, indent=2)
    print(f"Spec saved: {spec_path}")

    return sheet_path, spec_path


def main():
    parser = argparse.ArgumentParser(
        description="Generate a sprite sheet + spec from individual frame PNGs.",
        formatter_class=argparse.RawDescriptionHelpFormatter,
        epilog="""
Examples:
  %(prog)s --frames "frames/idle_*.png" --output idle --cell-size 512
  %(prog)s --frames "art/walk_00.png art/walk_01.png" --output walk --cell-size 256 --fps 12
  %(prog)s --frames "frames/dance_*.png" --output dance --cell-size 512 --ping-pong
        """
    )
    parser.add_argument("--frames", required=True,
                        help="Glob pattern or space-separated paths to frame PNGs")
    parser.add_argument("--output", required=True,
                        help="Output name (without extension): produces {name}.png + {name}_spec.json")
    parser.add_argument("--cell-size", type=int, default=512,
                        help="Frame cell size in pixels (default: 512)")
    parser.add_argument("--fps", type=float, default=10.0,
                        help="Playback FPS (default: 10)")
    parser.add_argument("--action", default=None,
                        help="Action name in spec (defaults to output name)")
    parser.add_argument("--output-dir", default=".",
                        help="Output directory (default: current dir)")
    parser.add_argument("--loop", action="store_true", default=True,
                        help="Mark animation as looping (default: true)")
    parser.add_argument("--no-loop", action="store_true",
                        help="Mark animation as non-looping")
    parser.add_argument("--ping-pong", action="store_true",
                        help="Mark animation as ping-pong looping")

    args = parser.parse_args()

    frame_paths = sorted(glob.glob(args.frames), key=natural_sort_key)
    if not frame_paths:
        parts = args.frames.split()
        frame_paths = sorted([p for p in parts if os.path.exists(p)], key=natural_sort_key)

    if not frame_paths:
        print(f"ERROR: No files matched pattern: {args.frames}")
        sys.exit(1)

    loop = not args.no_loop

    generate(
        frame_paths=frame_paths,
        output_name=args.output,
        cell_size=args.cell_size,
        fps=args.fps,
        action=args.action,
        output_dir=args.output_dir,
        loop=loop,
        ping_pong=args.ping_pong,
    )


if __name__ == "__main__":
    main()
