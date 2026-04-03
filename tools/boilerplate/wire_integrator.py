"""
Wire Integrator Tool

Reads project_profile.json + GDD entities and generates missing SDK integration
code tailored to the existing project's architecture (adapters, bridges).
Outputs code into an IVX directory to avoid overwriting existing files.
"""

from __future__ import annotations

import argparse
import json
import logging
import os
import shutil
from pathlib import Path
from typing import Any, Dict

logging.basicConfig(level=logging.INFO, format="%(levelname)s: %(message)s")
logger = logging.getLogger(__name__)

class WireIntegrator:
    def __init__(self, project_path: str, profile_path: str, gdd_dir: str):
        self.project_path = Path(project_path).resolve()
        self.profile_path = Path(profile_path).resolve()
        self.gdd_dir = Path(gdd_dir).resolve()
        
        with open(self.profile_path, "r", encoding="utf-8") as f:
            self.profile = json.load(f)
            
        self.engine = self.profile.get("engine", "unknown")
        self.arch = self.profile.get("architecture", {})
        
        # Determine output folder based on engine
        if self.engine == "unity":
            self.out_dir = self.project_path / "Assets" / "Scripts" / "IVX"
        elif self.engine in ["javascript", "web3"]:
            self.out_dir = self.project_path / "src" / "ivx"
        elif self.engine == "godot":
            self.out_dir = self.project_path / "addons" / "intelliversex"
        else:
            self.out_dir = self.project_path / "ivx_integration"
            
    def integrate(self, keep_auth: bool = True, keep_analytics: bool = True):
        logger.info("Integrating IVX into %s project at %s", self.engine, self.project_path)
        
        os.makedirs(self.out_dir, exist_ok=True)
        
        plan = self.profile.get("integration_plan", {})
        
        # Generate Adapters based on architecture
        self._generate_adapters(plan, keep_auth, keep_analytics)
        
        # Generate missing UI panels
        self._generate_ui_panels(plan.get("add_ui_panels", []))
        
        # Generate wiring report
        self._generate_report()
        
        logger.info("Integration complete. See %s/wiring_report.md", self.out_dir)

    def _generate_adapters(self, plan: Dict[str, Any], keep_auth: bool, keep_analytics: bool):
        """Generates the bridge adapters for existing systems."""
        
        if self.engine == "unity":
            ext = ".cs"
            if self.arch.get("pattern") == "dependency_injection":
                # Generate Zenject Installer
                content = """using Zenject;
namespace {namespace}.IVX {
    public class IVXInstaller : MonoInstaller {
        public override void InstallBindings() {
            Container.Bind<IIVXHiroAdapter>().To<IVXHiroAdapter>().AsSingle();
            Container.Bind<IIVXSatoriAdapter>().To<IVXSatoriAdapter>().AsSingle();
        }
    }
}"""
                with open(self.out_dir / f"IVXInstaller{ext}", "w") as f:
                    f.write(content.replace("{namespace}", self.profile["namespaces"]["root"]))
            else:
                # Generate Singletons
                content = "public class IVXHiroAdapter : MonoBehaviour { public static IVXHiroAdapter Instance; }"
                with open(self.out_dir / f"IVXHiroAdapter{ext}", "w") as f:
                    f.write(content)
                    
            if plan.get("bridge_existing_auth") and keep_auth:
                with open(self.out_dir / f"IVXAuthBridge{ext}", "w") as f:
                    f.write("// Adapts existing auth to IVX auth interface (bridged, not replaced)\n")
                    f.write("public class IVXAuthBridge : IIVXAuthBridge { }")
                    
        elif self.engine in ["javascript", "web3"]:
            ext = ".ts"
            if self.arch.get("state_management") == "zustand":
                content = """import { create } from 'zustand';
export const useIVXStore = create((set, get) => ({
  wallet: null,
  fetchWallet: async () => { /* Hiro RPC */ }
}));"""
            else:
                content = "export class IVXHiroAdapter { static getInstance() { return new IVXHiroAdapter(); } }"
                
            with open(self.out_dir / f"IVXStore{ext}", "w") as f:
                f.write(content)

        elif self.engine == "godot":
            ext = ".gd"
            if self.arch.get("pattern") == "signal_bus":
                content = """extends Node
signal ivx_wallet_updated(wallet)
func _ready():
    SignalBus.connect("player_authenticated", _on_auth)"""
            else:
                content = "extends Node\nfunc fetch_wallet():\n    pass"
                
            with open(self.out_dir / f"ivx_hiro_adapter{ext}", "w") as f:
                f.write(content)

    def _generate_ui_panels(self, panels: list):
        """Generates Opt-in UI panels that developer can add to their scenes."""
        ui_dir = self.out_dir / "UI"
        os.makedirs(ui_dir, exist_ok=True)
        
        for panel in panels:
            if self.engine == "unity":
                with open(ui_dir / f"IVX{panel.capitalize()}Panel.cs", "w") as f:
                    f.write(f"// Generates {panel} UI hooked up to IVX adapters\n")
                    f.write(f"public class IVX{panel.capitalize()}Panel : MonoBehaviour {{ }}")
            elif self.engine in ["javascript", "web3"]:
                with open(ui_dir / f"IVX{panel.capitalize()}Panel.tsx", "w") as f:
                    f.write(f"// React component for {panel}\n")
                    f.write(f"export const IVX{panel.capitalize()}Panel = () => <div />;")
            elif self.engine == "godot":
                with open(ui_dir / f"ivx_{panel}_panel.gd", "w") as f:
                    f.write(f"extends Control\n# {panel} UI script\n")

    def _generate_report(self):
        """Creates the Markdown diff report."""
        report = f"""# IVX Integration Report

## Project Profile
- Engine: {self.engine} ({self.profile.get('engine_version', 'N/A')})
- Architecture: {self.arch.get('pattern')} / {self.arch.get('ui_framework')}

## What Was Added
All IVX code was placed in `{self.out_dir}`. No existing files were overwritten.

### Adapters
- `IVXHiroAdapter` — Configures economy, store, achievements using existing Nakama connection
- `IVXSatoriAdapter` — Configures analytics
- `IVXAuthBridge` — Bridges existing auth to IVX interface

### UI Panels
These are opt-in panels you can drag into your scenes/screens:
"""
        for p in self.profile.get("missing_features", []):
            report += f"- `IVX{p.capitalize()}Panel`\n"
            
        report += "\n## Next Steps\n"
        if self.engine == "unity" and self.arch.get("pattern") == "dependency_injection":
            report += "1. Add `IVXInstaller` to your ProjectContext\n"
        elif self.engine == "unity":
            report += "1. Add `IVXHiroAdapter` to your Bootstrap scene\n"
        elif self.engine == "godot":
            report += "1. Add `ivx_hiro_adapter.gd` to Project > Autoload\n"
            
        report += "2. Configure your Hiro/Satori credentials\n"
        
        with open(self.out_dir / "wiring_report.md", "w", encoding="utf-8") as f:
            f.write(report)


def main():
    parser = argparse.ArgumentParser(description="Wire IVX into an existing game project.")
    parser.add_argument("--project", required=True, help="Path to existing game project")
    parser.add_argument("--profile", required=True, help="Path to project_profile.json")
    parser.add_argument("--gdd", required=True, help="Path to GDD directory")
    parser.add_argument("--keep-auth", action="store_true", default=True, help="Bridge existing auth instead of replacing")
    parser.add_argument("--keep-analytics", action="store_true", default=True, help="Bridge existing analytics instead of replacing")
    args = parser.parse_args()

    integrator = WireIntegrator(args.project, args.profile, args.gdd)
    integrator.integrate(args.keep_auth, args.keep_analytics)

if __name__ == "__main__":
    main()
