#!/usr/bin/env python3
"""
IntelliVerseX SDK - UPM Package Reorganization Script

This script reorganizes the SDK from the current Unity project structure
to a clean UPM package structure suitable for distribution.

Usage:
    python reorganize_for_upm.py [--dry-run] [--output-dir OUTPUT_DIR]

Options:
    --dry-run       Show what would be done without making changes
    --output-dir    Directory to create the clean package (default: ../intelliversex-sdk-package)
"""

import os
import sys
import shutil
import argparse
import json
from pathlib import Path
from datetime import datetime


class UPMReorganizer:
    """Reorganizes SDK to clean UPM package structure."""
    
    def __init__(self, source_dir: str, output_dir: str, dry_run: bool = False):
        self.source_dir = Path(source_dir)
        self.output_dir = Path(output_dir)
        self.dry_run = dry_run
        self.sdk_source = self.source_dir / "Assets" / "_IntelliVerseXSDK"
        
        # Define the folder mapping (source -> destination)
        self.runtime_folders = [
            "Core",
            "Identity",
            "Backend",
            "Monetization",
            "Analytics",
            "Localization",
            "Storage",
            "Networking",
            "Leaderboard",
            "Social",
            "Quiz",
            "QuizUI",
            "UI",
            "V2",
            "IAP",
            "IntroScene",
        ]
        
        self.editor_folders = [
            "Editor",
        ]
        
        self.root_files = [
            "package.json",
            "README.md",
            "CHANGELOG.md",
            "LICENSE",
            "INSTALLATION.md",
        ]
        
    def log(self, message: str, level: str = "INFO"):
        """Log a message with timestamp."""
        timestamp = datetime.now().strftime("%H:%M:%S")
        prefix = "[DRY-RUN] " if self.dry_run else ""
        print(f"[{timestamp}] {prefix}{level}: {message}")
        
    def validate_source(self) -> bool:
        """Validate the source directory structure."""
        if not self.sdk_source.exists():
            self.log(f"SDK source not found: {self.sdk_source}", "ERROR")
            return False
            
        package_json = self.sdk_source / "package.json"
        if not package_json.exists():
            self.log(f"package.json not found: {package_json}", "ERROR")
            return False
            
        self.log(f"Source validated: {self.sdk_source}")
        return True
        
    def create_directory(self, path: Path):
        """Create a directory if it doesn't exist."""
        if not path.exists():
            if not self.dry_run:
                path.mkdir(parents=True, exist_ok=True)
            self.log(f"Created directory: {path}")
            
    def copy_file(self, src: Path, dst: Path):
        """Copy a file from source to destination."""
        if not src.exists():
            self.log(f"Source file not found: {src}", "WARNING")
            return
            
        self.create_directory(dst.parent)
        
        if not self.dry_run:
            shutil.copy2(src, dst)
        self.log(f"Copied: {src.name} -> {dst}")
        
    def copy_directory(self, src: Path, dst: Path, exclude_patterns: list = None):
        """Copy a directory recursively."""
        if not src.exists():
            self.log(f"Source directory not found: {src}", "WARNING")
            return
            
        exclude_patterns = exclude_patterns or []
        
        if not self.dry_run:
            if dst.exists():
                shutil.rmtree(dst)
            shutil.copytree(
                src, dst,
                ignore=shutil.ignore_patterns(*exclude_patterns) if exclude_patterns else None
            )
        self.log(f"Copied directory: {src.name} -> {dst}")
        
    def update_package_json(self):
        """Update package.json with clean package paths."""
        src_package = self.sdk_source / "package.json"
        dst_package = self.output_dir / "package.json"
        
        if not src_package.exists():
            self.log("package.json not found", "ERROR")
            return
            
        with open(src_package, 'r', encoding='utf-8') as f:
            package_data = json.load(f)
            
        # Update sample paths for new structure
        if "samples" in package_data:
            for sample in package_data["samples"]:
                # Already using Samples~/ path format, which is correct
                pass
                
        # Update repository URL if needed
        package_data["repository"] = {
            "type": "git",
            "url": "https://github.com/intelliversex/unity-sdk.git"
        }
        
        # Update documentation URLs for root-level package
        package_data["documentationUrl"] = "https://github.com/intelliversex/unity-sdk#readme"
        package_data["changelogUrl"] = "https://github.com/intelliversex/unity-sdk/blob/main/CHANGELOG.md"
        package_data["licensesUrl"] = "https://github.com/intelliversex/unity-sdk/blob/main/LICENSE"
        
        if not self.dry_run:
            with open(dst_package, 'w', encoding='utf-8') as f:
                json.dump(package_data, f, indent=2)
        self.log("Updated package.json with clean paths")
        
    def create_main_asmdef(self):
        """Create main assembly definition for Runtime folder."""
        asmdef = {
            "name": "IntelliVerseX",
            "rootNamespace": "IntelliVerseX",
            "references": [],
            "includePlatforms": [],
            "excludePlatforms": [],
            "allowUnsafeCode": False,
            "overrideReferences": False,
            "precompiledReferences": [],
            "autoReferenced": True,
            "defineConstraints": [],
            "versionDefines": [],
            "noEngineReferences": False
        }
        
        dst_path = self.output_dir / "Runtime" / "IntelliVerseX.asmdef"
        
        if not self.dry_run:
            self.create_directory(dst_path.parent)
            with open(dst_path, 'w', encoding='utf-8') as f:
                json.dump(asmdef, f, indent=4)
        self.log("Created main Runtime assembly definition")
        
    def reorganize(self):
        """Execute the full reorganization."""
        self.log("=" * 60)
        self.log("IntelliVerseX SDK - UPM Package Reorganization")
        self.log("=" * 60)
        
        # Validate
        if not self.validate_source():
            return False
            
        # Create output directory
        self.create_directory(self.output_dir)
        
        # Copy root files
        self.log("\n--- Copying root files ---")
        for file in self.root_files:
            src = self.sdk_source / file
            dst = self.output_dir / file
            self.copy_file(src, dst)
            
        # Copy Runtime folders
        self.log("\n--- Copying Runtime modules ---")
        runtime_dir = self.output_dir / "Runtime"
        self.create_directory(runtime_dir)
        
        for folder in self.runtime_folders:
            src = self.sdk_source / folder
            dst = runtime_dir / folder
            if src.exists():
                self.copy_directory(src, dst, exclude_patterns=["*.meta"])
                
        # Copy Editor folder
        self.log("\n--- Copying Editor modules ---")
        for folder in self.editor_folders:
            src = self.sdk_source / folder
            dst = self.output_dir / "Editor"
            if src.exists():
                self.copy_directory(src, dst, exclude_patterns=["*.meta"])
                
        # Copy Samples~
        self.log("\n--- Copying Samples ---")
        samples_src = self.sdk_source / "Samples~"
        samples_dst = self.output_dir / "Samples~"
        if samples_src.exists():
            self.copy_directory(samples_src, samples_dst)
            
        # Copy Tests~
        self.log("\n--- Copying Tests ---")
        tests_src = self.sdk_source / "Tests~"
        tests_dst = self.output_dir / "Tests~"
        if tests_src.exists():
            self.copy_directory(tests_src, tests_dst)
            
        # Copy Documentation
        self.log("\n--- Copying Documentation ---")
        docs_src = self.sdk_source / "Documentation"
        docs_dst = self.output_dir / "Documentation~"
        if docs_src.exists():
            self.copy_directory(docs_src, docs_dst, exclude_patterns=["*.meta"])
            
        # Copy Icons
        self.log("\n--- Copying Icons ---")
        icons_src = self.sdk_source / "Icons"
        icons_dst = self.output_dir / "Icons"
        if icons_src.exists():
            self.copy_directory(icons_src, icons_dst, exclude_patterns=["*.meta"])
            
        # Update package.json
        self.log("\n--- Updating package.json ---")
        self.update_package_json()
        
        # Summary
        self.log("\n" + "=" * 60)
        self.log("Reorganization complete!")
        self.log(f"Output: {self.output_dir}")
        self.log("=" * 60)
        
        return True


def main():
    parser = argparse.ArgumentParser(
        description="Reorganize IntelliVerseX SDK to clean UPM package structure"
    )
    parser.add_argument(
        "--dry-run",
        action="store_true",
        help="Show what would be done without making changes"
    )
    parser.add_argument(
        "--output-dir",
        type=str,
        default="../intelliversex-sdk-package",
        help="Directory to create the clean package"
    )
    parser.add_argument(
        "--source-dir",
        type=str,
        default=".",
        help="Source Unity project directory"
    )
    
    args = parser.parse_args()
    
    # Resolve paths
    source_dir = Path(args.source_dir).resolve()
    output_dir = Path(args.output_dir)
    
    if not output_dir.is_absolute():
        output_dir = (source_dir / output_dir).resolve()
        
    # Run reorganization
    reorganizer = UPMReorganizer(
        source_dir=str(source_dir),
        output_dir=str(output_dir),
        dry_run=args.dry_run
    )
    
    success = reorganizer.reorganize()
    
    if success:
        print("\n✅ Success! Clean UPM package created at:")
        print(f"   {output_dir}")
        print("\nNext steps:")
        print("1. Review the generated package structure")
        print("2. Test installation in a fresh Unity project")
        print("3. Push to a new Git repository for distribution")
    else:
        print("\n❌ Reorganization failed. Check errors above.")
        sys.exit(1)


if __name__ == "__main__":
    main()
