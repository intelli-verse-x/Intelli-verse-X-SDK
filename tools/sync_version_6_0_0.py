#!/usr/bin/env python3
"""Sync public install pins and doc version strings to v6.0.0."""
from pathlib import Path

ROOT = Path(r"c:\Office\Unity\Intelli-verse-X-SDK")
SKIP = {".git", "Library", "node_modules", "Temp", "graphify-out", "obj", "bin"}
EXTS = {".md", ".yml", ".yaml", ".json", ".cs", ".txt", ".toml"}

REPLACEMENTS = [
    ("?path=Packages/com.intelliversex.sdk#v5.10.0", "?path=Packages/com.intelliversex.sdk#v6.0.0"),
    ("?path=Packages/com.intelliversex.sdk#v5.11.0", "?path=Packages/com.intelliversex.sdk#v6.0.0"),
    ("?path=Assets/_IntelliVerseXSDK#v5.8.0", "?path=Packages/com.intelliversex.sdk#v6.0.0"),
    ("?path=Assets/Intelli-verse-X-SDK#v5.2.0", "?path=Packages/com.intelliversex.sdk#v6.0.0"),
    ("IntelliVerseX SDK v5.8.0", "IntelliVerseX SDK v6.0.0"),
    ("IntelliVerseX SDK v5.10.0", "IntelliVerseX SDK v6.0.0"),
    ("**5.11.0**", "**6.0.0**"),
    ("GIT_TAG v5.8.0", "GIT_TAG v6.0.0"),
    ("v5.8.0+", "v6.0.0+"),
    ("tag/v5.8.0", "tag/v6.0.0"),
    ("tags/v5.8.0", "tags/v6.0.0"),
    ("(e.g. `v5.8.0`)", "(e.g. `v6.0.0`)"),
    ("Use semver (e.g. `v5.8.0`)", "Use semver (e.g. `v6.0.0`)"),
    ("git tag v5.8.0 && git push origin v5.8.0", "git tag v6.0.0 && git push origin v6.0.0"),
    ("synced to v5.8.0", "synced to v6.0.0"),
    ("GitHub release v5.8.0", "GitHub release v6.0.0"),
    ("What's New in v5.8.0", "What's New in v6.0.0"),
    ("## What's New in v5.8.0", "## What's New in v6.0.0"),
    ("At **v5.8.0**", "At **v6.0.0**"),
    ("(v5.8.0)", "(v6.0.0)"),
    ("**v5.8.0 adds", "**v6.0.0 adds"),
]

changed = []
for path in ROOT.rglob("*"):
    if not path.is_file():
        continue
    if any(s in path.parts for s in SKIP):
        continue
    if path.suffix.lower() not in EXTS:
        continue
    try:
        text = path.read_text(encoding="utf-8")
    except Exception:
        continue
    orig = text
    for old, new in REPLACEMENTS:
        text = text.replace(old, new)
    if text != orig:
        path.write_text(text, encoding="utf-8", newline="\n")
        changed.append(str(path.relative_to(ROOT)))

print(f"Updated {len(changed)} files")
for c in changed:
    print(c)
