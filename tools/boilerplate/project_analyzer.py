"""
Project Analyzer Tool

Scans an existing game project to detect the engine, architecture patterns,
and existing SDK/feature integrations. Outputs a project_profile.json
that the Wire Integrator uses to generate missing SDK pieces matching
the project's existing conventions.
"""

from __future__ import annotations

import argparse
import json
import logging
import os
import re
from pathlib import Path
from typing import Any, Dict

logging.basicConfig(level=logging.INFO, format="%(levelname)s: %(message)s")
logger = logging.getLogger(__name__)

class ProjectAnalyzer:
    def __init__(self, project_path: str):
        self.project_path = Path(project_path).resolve()
        self.profile: Dict[str, Any] = {
            "engine": "unknown",
            "engine_version": "unknown",
            "architecture": {
                "pattern": "singleton",
                "di_framework": None,
                "ui_framework": "unknown",
                "state_management": "unknown"
            },
            "namespaces": {
                "root": "MyGame",
                "convention": "PascalCase",
                "prefix": ""
            },
            "folder_structure": {},
            "existing_integrations": {
                "nakama": {"present": False},
                "hiro": {"present": False},
                "satori": {"present": False},
                "auth": {"provider": "custom", "present": False},
                "economy": {"present": False},
                "analytics": {"provider": "none", "present": False}
            },
            "features_detected": {
                "auth": False,
                "wallet": False,
                "store": False,
                "achievements": False,
                "daily_rewards": False,
                "energy": False,
                "leaderboard": False,
                "progression": False,
                "analytics": False,
                "ftue": False,
                "settings": False,
                "retention": False
            },
            "missing_features": []
        }

    def analyze(self) -> Dict[str, Any]:
        """Run the full analysis."""
        if not self.project_path.exists():
            logger.error("Project path does not exist: %s", self.project_path)
            return self.profile

        self._detect_engine()
        self._detect_architecture()
        self._detect_integrations()
        self._calculate_missing()
        
        return self.profile

    def _detect_engine(self):
        """Auto-detect the game engine from project files."""
        # Unity
        if (self.project_path / "ProjectSettings" / "ProjectVersion.txt").exists():
            self.profile["engine"] = "unity"
            with open(self.project_path / "ProjectSettings" / "ProjectVersion.txt", "r") as f:
                content = f.read()
                m = re.search(r"m_EditorVersion:\s*(.*)", content)
                if m:
                    self.profile["engine_version"] = m.group(1).strip()
            self.profile["folder_structure"]["scripts"] = "Assets/Scripts/"
            return

        # Godot
        if (self.project_path / "project.godot").exists():
            self.profile["engine"] = "godot"
            self.profile["folder_structure"]["scripts"] = "scripts/"
            return

        # JS/TS (React/Web3)
        if (self.project_path / "package.json").exists():
            with open(self.project_path / "package.json", "r") as f:
                pkg = json.load(f)
                deps = {**pkg.get("dependencies", {}), **pkg.get("devDependencies", {})}
                
                if "ethers" in deps or "web3" in deps:
                    self.profile["engine"] = "web3"
                else:
                    self.profile["engine"] = "javascript"
                
                if "react" in deps:
                    self.profile["architecture"]["ui_framework"] = "react"
            self.profile["folder_structure"]["scripts"] = "src/"
            return

        # Unreal
        if list(self.project_path.glob("*.uproject")):
            self.profile["engine"] = "unreal"
            self.profile["folder_structure"]["scripts"] = "Source/"
            return

        # Check other markers
        if (self.project_path / "default.project.json").exists():
            self.profile["engine"] = "roblox"
            self.profile["folder_structure"]["scripts"] = "src/"
        elif (self.project_path / "pubspec.yaml").exists():
            self.profile["engine"] = "flutter"
            self.profile["folder_structure"]["scripts"] = "lib/"
        elif (self.project_path / "game.project").exists():
            self.profile["engine"] = "defold"
            self.profile["folder_structure"]["scripts"] = "main/"

    def _detect_architecture(self):
        """Detect MVC, DI frameworks, State Management patterns."""
        if self.profile["engine"] == "unity":
            # Check for Zenject/VContainer
            if self._find_in_files("Assets/Scripts", ["Zenject", "MonoInstaller", "[Inject]"]):
                self.profile["architecture"]["di_framework"] = "zenject"
                self.profile["architecture"]["pattern"] = "dependency_injection"
            elif self._find_in_files("Assets/Scripts", ["ScriptableObject", "GameEvent"]):
                self.profile["architecture"]["state_management"] = "scriptableobject_events"
                
        elif self.profile["engine"] in ["javascript", "web3"]:
            # Check for Zustand/Redux
            if self._find_in_files("src", ["create(", "zustand"]):
                self.profile["architecture"]["state_management"] = "zustand"
            elif self._find_in_files("src", ["useSelector", "configureStore"]):
                self.profile["architecture"]["state_management"] = "redux"

        elif self.profile["engine"] == "godot":
            if self._find_in_files("scripts", ["SignalBus", "EventBus"]):
                self.profile["architecture"]["pattern"] = "signal_bus"

    def _detect_integrations(self):
        """Detect existing SDKs (Nakama, Firebase, PlayFab, etc)."""
        search_dirs = [self.profile["folder_structure"].get("scripts", "")]
        
        # Check Nakama
        if self._find_in_files(search_dirs[0], ["Nakama", "heroiclabs", "authenticateDevice"]):
            self.profile["existing_integrations"]["nakama"]["present"] = True
            
        # Check Auth
        if self._find_in_files(search_dirs[0], ["FirebaseAuth", "PlayFabClientAPI"]):
            self.profile["existing_integrations"]["auth"]["present"] = True
            self.profile["existing_integrations"]["auth"]["provider"] = "firebase_or_playfab"
            self.profile["features_detected"]["auth"] = True
            
        # Check Analytics
        if self._find_in_files(search_dirs[0], ["FirebaseAnalytics", "LogEvent"]):
            self.profile["existing_integrations"]["analytics"]["present"] = True
            self.profile["existing_integrations"]["analytics"]["provider"] = "firebase"
            self.profile["features_detected"]["analytics"] = True

    def _calculate_missing(self):
        features = self.profile["features_detected"]
        self.profile["missing_features"] = [k for k, v in features.items() if not v]
        
        self.profile["integration_plan"] = {
            "add_hiro": True,
            "add_satori": True,
            "replace_auth": False,
            "bridge_existing_auth": features["auth"],
            "bridge_existing_economy": features["economy"],
            "add_ui_panels": [f for f in self.profile["missing_features"]],
            "analytics_adapter": "bridge_existing_to_satori" if features["analytics"] else "direct_satori"
        }

    def _find_in_files(self, sub_dir: str, patterns: list) -> bool:
        """Helper to scan files for specific keywords."""
        search_path = self.project_path / sub_dir
        if not search_path.exists():
            return False
            
        for ext in ["*.cs", "*.ts", "*.tsx", "*.js", "*.gd", "*.lua", "*.dart", "*.cpp", "*.h"]:
            for file_path in search_path.rglob(ext):
                try:
                    with open(file_path, "r", encoding="utf-8") as f:
                        content = f.read()
                        if any(p in content for p in patterns):
                            return True
                except Exception:
                    continue
        return False

def main():
    parser = argparse.ArgumentParser(description="Analyze game project architecture.")
    parser.add_argument("--project", required=True, help="Path to existing game project")
    parser.add_argument("--output", default="project_profile.json", help="Output JSON path")
    args = parser.parse_args()

    analyzer = ProjectAnalyzer(args.project)
    profile = analyzer.analyze()
    
    with open(args.output, "w", encoding="utf-8") as f:
        json.dump(profile, f, indent=2)
        
    logger.info("Project analyzed. Engine: %s. Output: %s", profile["engine"], args.output)

if __name__ == "__main__":
    main()
