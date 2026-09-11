#!/usr/bin/env python3
"""Split APIManager.cs into partial class files by major region groups."""
from pathlib import Path

SRC = Path(r"c:\Office\Unity\Intelli-verse-X-SDK\Packages\com.intelliversex.sdk\Identity\APIManager.cs")
OUT_DIR = SRC.parent

text = SRC.read_text(encoding="utf-8")
if "public static partial class APIManager" in text:
    print("Already partial — skipping")
    raise SystemExit(0)

# Mark as partial
text = text.replace("public static class APIManager", "public static partial class APIManager", 1)

# Find region starts of interest (line-based)
lines = text.splitlines(keepends=True)

# Region markers we care about (first occurrence)
targets = {
    "Auth": "#region Auth V2: Signup / Initiate (Cognito)",
    "Notes": "#region Notes API",
    "Friends": "#region Friends API",
}

indices = {}
for i, line in enumerate(lines):
    for key, marker in targets.items():
        if key not in indices and marker in line:
            indices[key] = i

# Also find AI Quiz as boundary before Notes, Referral after Friends start areas
# Structure we want:
# Core: start .. Auth
# Auth: Auth .. Notes (includes OAuth, Refresh, AI Quiz)
# Notes: Notes .. Friends
# Friends: Friends .. EOF (includes Guest Conversion, Referral, Avatar)

if "Auth" not in indices or "Notes" not in indices or "Friends" not in indices:
    print("Could not find required regions:", indices)
    raise SystemExit(1)

# Find class opening brace line
class_start = None
for i, line in enumerate(lines):
    if "public static partial class APIManager" in line:
        class_start = i
        break

# Find end of using / before class — keep in all files
header_end = class_start  # include blank line before class

# Core file: header + class open + lines until Auth
# Auth file: header + partial class + Auth..Notes
# Notes file: header + partial class + Notes..Friends  
# Friends file: header + partial class + Friends..closing brace

auth_i = indices["Auth"]
notes_i = indices["Notes"]
friends_i = indices["Friends"]

# Find final closing brace of class (last line that is just })
end_i = len(lines) - 1
while end_i > 0 and lines[end_i].strip() == "":
    end_i -= 1
# expect }
assert lines[end_i].strip() == "}", f"Unexpected end: {lines[end_i]!r}"

header = "".join(lines[:header_end])
class_open = "public static partial class APIManager\n{\n"

core_body = "".join(lines[class_start + 1:auth_i])  # after "public static ... {" line — need care

# Re-parse: class_start line is `public static partial class APIManager`
# next non-empty should be `{`
brace_i = class_start + 1
while brace_i < len(lines) and lines[brace_i].strip() == "":
    brace_i += 1
assert lines[brace_i].strip() == "{"

core_content = "".join(lines[brace_i + 1:auth_i])
auth_content = "".join(lines[auth_i:notes_i])
notes_content = "".join(lines[notes_i:friends_i])
friends_content = "".join(lines[friends_i:end_i])  # exclude final }

def wrap(body: str) -> str:
    return header + class_open + body + "}\n"

core_path = OUT_DIR / "APIManager.cs"
auth_path = OUT_DIR / "APIManager.Auth.cs"
notes_path = OUT_DIR / "APIManager.Notes.cs"
friends_path = OUT_DIR / "APIManager.Social.cs"

core_path.write_text(wrap(core_content), encoding="utf-8", newline="\n")
auth_path.write_text(wrap(auth_content), encoding="utf-8", newline="\n")
notes_path.write_text(wrap(notes_content), encoding="utf-8", newline="\n")
friends_path.write_text(wrap(friends_content), encoding="utf-8", newline="\n")

print(f"Wrote {core_path.name} ({core_path.stat().st_size} bytes)")
print(f"Wrote {auth_path.name} ({auth_path.stat().st_size} bytes)")
print(f"Wrote {notes_path.name} ({notes_path.stat().st_size} bytes)")
print(f"Wrote {friends_path.name} ({friends_path.stat().st_size} bytes)")
print(f"Split points: Auth@{auth_i}, Notes@{notes_i}, Friends@{friends_i}, End@{end_i}")
