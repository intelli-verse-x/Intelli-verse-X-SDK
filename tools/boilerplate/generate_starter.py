#!/usr/bin/env python3
"""
IntelliVerseX Starter Project Generator

Reads GDD outputs (brand_entity.json, game_context.json) or raw GDD markdown
and produces a fully-wired, runnable game project for any of the 11 supported
engines. Every generated project includes authentication, economy, achievements,
daily rewards, streaks, energy, leaderboards, progression, store, analytics,
settings, and FTUE — all connected to the IntelliVerseX SDK.

Usage:
    python tools/boilerplate/generate_starter.py \
      --engine unity \
      --gdd-dir design/ \
      --output-dir output/starter-project/ \
      --brand-id my-studio --game-id my-game

    python tools/boilerplate/generate_starter.py \
      --engine javascript \
      --brand-entity output/entities/brand_entity.json \
      --game-context output/entities/game_context.json \
      --output-dir output/starter-project/
"""
from __future__ import annotations

import argparse
import json
import logging
import os
import re
import shutil
import sys
from pathlib import Path
from typing import Any, Dict, List, Optional

logger = logging.getLogger(__name__)

SUPPORTED_ENGINES = [
    "unity", "javascript", "godot", "roblox", "java", "flutter",
    "unreal", "defold", "cpp", "cocos2dx", "web3",
]

DEFAULT_FEATURES = [
    "auth", "economy", "achievements", "streaks", "energy",
    "leaderboard", "progression", "store", "analytics", "settings", "ftue",
]

OPTIONAL_FEATURES = ["multiplayer", "ai"]

TEMPLATES_DIR = Path(__file__).parent / "templates"


def _load_json(path: str) -> Dict[str, Any]:
    with open(path, encoding="utf-8") as f:
        return json.load(f)


def _try_load_json(path: str) -> Dict[str, Any]:
    if path and os.path.exists(path):
        return _load_json(path)
    return {}


def _load_gdd_entities(gdd_dir: str, brand_id: str, game_id: str) -> Dict[str, Any]:
    """Run gdd_to_entity.py and load the resulting JSON, or find pre-existing."""
    output_dir = os.path.join(gdd_dir, "..", "output", "entities")
    brand_path = os.path.join(output_dir, "brand_entity.json")
    game_path = os.path.join(output_dir, "game_context.json")

    if not os.path.exists(brand_path):
        gdd_script = Path(__file__).parent.parent / "asset-pipeline" / "gdd_to_entity.py"
        if gdd_script.exists():
            import subprocess
            logger.info("Running gdd_to_entity.py to export entities...")
            subprocess.run([
                sys.executable, str(gdd_script),
                "--gdd-dir", gdd_dir,
                "--output-dir", output_dir,
                "--brand-id", brand_id,
                "--game-id", game_id,
            ], check=False)

    return {
        "brand_entity": _try_load_json(brand_path),
        "game_context": _try_load_json(game_path),
    }


class TemplateContext:
    """Resolves template variables from brand_entity + game_context."""

    def __init__(
        self,
        brand_entity: Dict[str, Any],
        game_context: Dict[str, Any],
        brand_id: str = "default",
        game_id: str = "my-game",
        engine: str = "unity",
        features: Optional[List[str]] = None,
        ui_style: str = "modern-dark",
    ):
        self.brand = brand_entity
        self.game = game_context
        self.brand_id = brand_id
        self.game_id = game_id
        self.engine = engine
        self.features = features or DEFAULT_FEATURES
        self.ui_style = ui_style

    @property
    def vars(self) -> Dict[str, str]:
        import re
        b = self.brand
        g = self.game
        store = g.get("store_config", {})
        economy = g.get("economy", {})
        energy = g.get("energy", {})
        colors = b.get("color_palette", [])
        typo = b.get("typography", {})

        game_name = g.get("name") or b.get("name") or self.game_id.replace("-", " ").title()
        game_name_slug = re.sub(r'[^a-zA-Z0-9_]', '', game_name)

        return {
            "game_name": game_name,
            "game_name_slug": game_name_slug,
            "game_id": self.game_id,
            "brand_id": self.brand_id,
            "server_host": g.get("server_host", "nakama.intelli-verse-x.ai"),
            "server_port": str(g.get("server_port", 7350)),
            "server_key": g.get("server_key", "defaultkey"),
            "primary_color": colors[0] if len(colors) > 0 else "#FF6B35",
            "secondary_color": colors[1] if len(colors) > 1 else "#1A1A2E",
            "background_color": colors[2] if len(colors) > 2 else "#0F0F23",
            "display_font": typo.get("display", "Fredoka One"),
            "body_font": typo.get("body", "Nunito"),
            "tagline": b.get("tagline", "Powered by IntelliVerseX"),
            "bundle_id": store.get("ios_bundle_id", f"com.{self.brand_id}.{self.game_id}".replace("-", "")),
            "package_name": store.get("android_package", f"com.{self.brand_id}.{self.game_id}".replace("-", "")),
            "company_name": b.get("studio_name") or b.get("name") or self.brand_id.replace("-", " ").title(),
            "initial_coins": str(economy.get("initial_coins", 100)),
            "initial_gems": str(economy.get("initial_gems", 10)),
            "max_energy": str(energy.get("max", 5)),
            "energy_refill_minutes": str(energy.get("refill_minutes", 30)),
            "chain_id": str(g.get("web3_chain_id", 1)),
            "contract_address": g.get(
                "web3_contract_address", "0x0000000000000000000000000000000000000000"
            ),
            "engine": self.engine,
            "ui_style": self.ui_style,
            "features_csv": ",".join(self.features),
            "year": "2026",
        }

    def render(self, template_content: str) -> str:
        result = template_content
        for key, value in self.vars.items():
            result = result.replace("{{" + key + "}}", value)
        return result

    def has_feature(self, name: str) -> bool:
        return name in self.features


class StarterProjectGenerator:
    """Generates a starter project from engine-specific templates."""

    def __init__(self, ctx: TemplateContext, output_dir: str, dry_run: bool = False):
        self.ctx = ctx
        self.output_dir = Path(output_dir)
        self.dry_run = dry_run
        self.files_written: List[str] = []

    def generate(self) -> Dict[str, Any]:
        engine = self.ctx.engine
        template_dir = TEMPLATES_DIR / engine

        if not template_dir.exists():
            raise FileNotFoundError(
                f"No templates found for engine '{engine}' at {template_dir}. "
                f"Supported engines: {', '.join(SUPPORTED_ENGINES)}"
            )

        logger.info("Generating %s starter project → %s", engine, self.output_dir)
        logger.info("Features: %s", ", ".join(self.ctx.features))
        logger.info("Game: %s (%s)", self.ctx.vars["game_name"], self.ctx.game_id)

        if not self.dry_run:
            self.output_dir.mkdir(parents=True, exist_ok=True)

        self._copy_and_render_tree(template_dir, self.output_dir)

        self._remove_disabled_features()

        readme_path = self.output_dir / "README.md"
        if not self.dry_run:
            self._generate_readme(readme_path)
            self.files_written.append(str(readme_path))

        result = {
            "engine": engine,
            "output_dir": str(self.output_dir),
            "files_written": len(self.files_written),
            "features": self.ctx.features,
            "game_name": self.ctx.vars["game_name"],
            "game_id": self.ctx.game_id,
            "brand_id": self.ctx.brand_id,
        }

        manifest_path = self.output_dir / "starter_manifest.json"
        if not self.dry_run:
            with open(manifest_path, "w", encoding="utf-8") as f:
                json.dump(result, f, indent=2)

        logger.info("Generated %d files for %s starter project", len(self.files_written), engine)
        return result

    def _copy_and_render_tree(self, src_dir: Path, dst_dir: Path):
        for item in sorted(src_dir.rglob("*")):
            if item.is_dir():
                continue
            if item.name.startswith("."):
                continue

            rel = item.relative_to(src_dir)
            rendered_rel = Path(self.ctx.render(str(rel)))
            dst_path = dst_dir / rendered_rel

            if not self.dry_run:
                dst_path.parent.mkdir(parents=True, exist_ok=True)

            if self._is_text_file(item):
                content = item.read_text(encoding="utf-8")
                rendered = self.ctx.render(content)
                if not self.dry_run:
                    dst_path.write_text(rendered, encoding="utf-8")
            else:
                if not self.dry_run:
                    shutil.copy2(item, dst_path)

            self.files_written.append(str(rendered_rel))
            if self.dry_run:
                print(f"  [DRY RUN] {rendered_rel}")

    def _is_text_file(self, path: Path) -> bool:
        text_exts = {
            ".cs", ".ts", ".tsx", ".js", ".jsx", ".json", ".yaml", ".yml",
            ".md", ".txt", ".gd", ".lua", ".luau", ".tscn", ".tres", ".cfg",
            ".cpp", ".h", ".hpp", ".dart", ".xml", ".gradle", ".html", ".css",
            ".ini", ".toml", ".asmdef", ".uplugin", ".uproject",
        }
        return path.suffix.lower() in text_exts

    def _remove_disabled_features(self):
        feature_dir_map = {
            "multiplayer": ["Multiplayer", "multiplayer"],
            "ai": ["AI", "ai", "ai_chat"],
        }
        for feature, dirs in feature_dir_map.items():
            if self.ctx.has_feature(feature):
                continue
            for d in dirs:
                target = self.output_dir / "**" / d
                for match in self.output_dir.rglob(d):
                    if match.is_dir() and not self.dry_run:
                        shutil.rmtree(match, ignore_errors=True)
                        logger.info("Removed disabled feature dir: %s", match)

    def _generate_readme(self, path: Path):
        v = self.ctx.vars
        engine_setup = {
            "unity": "1. Open the project in Unity 6000+\n2. Open the Bootstrap scene\n3. Press Play",
            "javascript": "1. Run `npm install`\n2. Run `npm run dev`\n3. Open http://localhost:3000",
            "godot": "1. Open project.godot in Godot 4.2+\n2. Press F5 to run",
            "roblox": "1. Open in Roblox Studio via Rojo\n2. Press Play to test",
            "java": "1. Open in Android Studio\n2. Sync Gradle\n3. Run on emulator/device",
            "flutter": "1. Run `flutter pub get`\n2. Run `flutter run`",
            "unreal": "1. Open the .uproject in Unreal Engine 5.3+\n2. Press Play in Editor",
            "defold": "1. Open game.project in Defold Editor\n2. Build and Run",
            "cpp": "1. Run `cmake -B build && cmake --build build`\n2. Run `./build/game`",
            "cocos2dx": "1. Run `cmake -B build && cmake --build build`\n2. Run the executable",
            "web3": "1. Run `npm install`\n2. Run `npm run dev`\n3. Connect wallet at http://localhost:3000",
        }

        content = f"""# {v['game_name']} — IntelliVerseX Starter Project

> {v['tagline']}

## Quick Start

{engine_setup.get(self.ctx.engine, "See engine-specific documentation.")}

## What's Included

This starter project was generated by the IntelliVerseX Game Design Studio with
all SDK features pre-wired and ready to run.

### Features

| Feature | Status | Description |
|---------|--------|-------------|
| Authentication | {"Enabled" if self.ctx.has_feature("auth") else "Disabled"} | Guest, email, and social sign-in |
| Economy | {"Enabled" if self.ctx.has_feature("economy") else "Disabled"} | Wallet (coins + gems), rewards |
| Store | {"Enabled" if self.ctx.has_feature("store") else "Disabled"} | Purchasable items, IAP hooks |
| Achievements | {"Enabled" if self.ctx.has_feature("achievements") else "Disabled"} | Progress tracking, badges |
| Daily Rewards | {"Enabled" if self.ctx.has_feature("streaks") else "Disabled"} | Calendar grid, streak counter |
| Energy System | {"Enabled" if self.ctx.has_feature("energy") else "Disabled"} | Energy bar, refill timer |
| Progression | {"Enabled" if self.ctx.has_feature("progression") else "Disabled"} | XP bar, level-up |
| Leaderboards | {"Enabled" if self.ctx.has_feature("leaderboard") else "Disabled"} | Global + friends rankings |
| Analytics | {"Enabled" if self.ctx.has_feature("analytics") else "Disabled"} | Satori event tracking |
| Feature Flags | {"Enabled" if self.ctx.has_feature("analytics") else "Disabled"} | Satori flag-gated UI |
| Settings | {"Enabled" if self.ctx.has_feature("settings") else "Disabled"} | Audio, notifications, account |
| FTUE | {"Enabled" if self.ctx.has_feature("ftue") else "Disabled"} | First-time onboarding |
| Multiplayer | {"Enabled" if self.ctx.has_feature("multiplayer") else "Disabled"} | Lobby, matchmaking |
| AI Chat | {"Enabled" if self.ctx.has_feature("ai") else "Disabled"} | AI NPC dialog |

### Configuration

| Setting | Value |
|---------|-------|
| Game ID | `{v['game_id']}` |
| Brand ID | `{v['brand_id']}` |
| Server | `{v['server_host']}:{v['server_port']}` |
| Engine | {self.ctx.engine.title()} |

### SDK Documentation

- [IntelliVerseX SDK](https://github.com/intelli-verse-x/Intelli-verse-X-Unity-SDK)
- [Hiro Live-Ops](https://heroiclabs.com/docs/hiro/)
- [Satori Analytics](https://heroiclabs.com/docs/satori/)
- [Nakama Server](https://heroiclabs.com/docs/nakama/)

---

*Generated by IntelliVerseX Game Design Studio v3.0.0*
"""
        path.write_text(content, encoding="utf-8")


def main():
    parser = argparse.ArgumentParser(
        description="IntelliVerseX Starter Project Generator",
        formatter_class=argparse.RawDescriptionHelpFormatter,
    )
    parser.add_argument("--gdd-dir", help="Path to design/ folder with GDD markdown files")
    parser.add_argument("--brand-entity", help="Direct path to brand_entity.json")
    parser.add_argument("--game-context", help="Direct path to game_context.json")
    parser.add_argument("--engine", required=True, choices=SUPPORTED_ENGINES,
                        help="Target game engine")
    parser.add_argument("--engine-version", help="Engine version string")
    parser.add_argument("--output-dir", required=True, help="Output directory for generated project")
    parser.add_argument("--brand-id", default="default", help="Brand identifier")
    parser.add_argument("--game-id", default="my-game", help="Game identifier")
    parser.add_argument("--features", help="Comma-separated features (default: all core features)")
    parser.add_argument("--include-multiplayer", action="store_true", help="Include multiplayer demo")
    parser.add_argument("--include-ai", action="store_true", help="Include AI chat demo")
    parser.add_argument("--ui-style", default="modern-dark",
                        choices=["modern-dark", "modern-light", "retro", "minimal"],
                        help="UI theme style")
    parser.add_argument("--dry-run", action="store_true", help="Print file list without writing")
    parser.add_argument("-v", "--verbose", action="store_true")

    args = parser.parse_args()

    logging.basicConfig(
        level=logging.DEBUG if args.verbose else logging.INFO,
        format="%(levelname)s: %(message)s",
    )

    brand_entity: Dict[str, Any] = {}
    game_context: Dict[str, Any] = {}

    if args.brand_entity:
        brand_entity = _load_json(args.brand_entity)
    if args.game_context:
        game_context = _load_json(args.game_context)

    if args.gdd_dir and not brand_entity:
        entities = _load_gdd_entities(args.gdd_dir, args.brand_id, args.game_id)
        brand_entity = entities.get("brand_entity", {})
        game_context = entities.get("game_context", {})

    features = list(DEFAULT_FEATURES)
    if args.features:
        features = [f.strip() for f in args.features.split(",")]
    if args.include_multiplayer:
        features.append("multiplayer")
    if args.include_ai:
        features.append("ai")

    ctx = TemplateContext(
        brand_entity=brand_entity,
        game_context=game_context,
        brand_id=args.brand_id,
        game_id=args.game_id,
        engine=args.engine,
        features=features,
        ui_style=args.ui_style,
    )

    generator = StarterProjectGenerator(ctx=ctx, output_dir=args.output_dir, dry_run=args.dry_run)
    result = generator.generate()

    print(json.dumps(result, indent=2))
    return 0


if __name__ == "__main__":
    sys.exit(main())
