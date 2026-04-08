# `.agents/skills/` — project and user discovery

Many agent clients look for skills under:

| Scope | Path | In this repo |
|-------|------|----------------|
| **Project** | `<repository>/.agents/skills/<skill-id>/SKILL.md` | **Committed** — use as-is when the repo is the workspace root |
| **User** | `~/.agents/skills/<skill-id>/SKILL.md` | **Optional** — copy or sync from this folder (see install scripts below) |

Each skill is a directory containing a **`SKILL.md`** file with optional YAML frontmatter (`name`, `description`).

---

## What is here (mirrors `skills/`)

| Path | Purpose |
|------|---------|
| [`intelliversex-game-sdk/SKILL.md`](intelliversex-game-sdk/SKILL.md) | MCP / Nakama / Hiro / Satori — same content as [`skills/intelliversex-game-sdk/SKILL.md`](../../skills/intelliversex-game-sdk/SKILL.md), with links adjusted for `.agents/skills/` depth |
| [`platforms/`](platforms/README.md) | Per-engine SDK pointers — same as [`skills/platforms/`](../../skills/platforms/README.md) |

Canonical copies also live under **`skills/`** (Smithery, docs). Keep behavior in sync when editing; prefer editing one side and copying, or treat `skills/` as canonical and regenerate `.agents` in a follow-up.

---

## “Every marketplace” — how paths map

| Platform / client | Typical skill location | This repo |
|-------------------|------------------------|-----------|
| **Agents / generic** | `~/.agents/skills/` or `<project>/.agents/skills/` | **This folder** |
| **Cursor** | `.cursor/skills/` (optional) | [`.cursor/skills/README.md`](../../.cursor/skills/README.md) |
| **Smithery Skills** | Git path e.g. `skills/intelliversex-game-sdk/` | [`skills/intelliversex-game-sdk/`](../../skills/intelliversex-game-sdk/) |
| **OpenAI Codex** | Plugin + Markdown guides | [`.codex-plugin/plugin.json`](../../.codex-plugin/plugin.json), [`docs/guides/skills/`](../../docs/guides/skills/) |
| **Windsurf / Codeium** | Marketplace URL + sample `SKILL.md` | Point reviewers at `skills/intelliversex-game-sdk/SKILL.md` or this tree |
| **Claude Code** | Git-backed `SKILL.md` | Same as Smithery-style path under `skills/` or here |
| **Gemini Code Assist** | MCP URL in settings (not a file tree) | `https://mcp.intelli-verse-x.ai/api/mcp` per [`smithery.yaml`](../../smithery.yaml) |

---

## Install into user-level `~/.agents/skills/` (optional)

From the repository root:

**Windows (PowerShell):**

```powershell
./tools/scripts/install-agents-skills.ps1
```

**macOS / Linux:**

```bash
chmod +x tools/scripts/install-agents-skills.sh
./tools/scripts/install-agents-skills.sh
```

These scripts copy **this repo’s** `.agents/skills/*` into **`%USERPROFILE%\.agents\skills`** or **`$HOME/.agents/skills`**, merging with existing files. Re-run after pulls if skills change.

---

## Publishing checklist

See [`docs/PUBLISHING_CHECKLIST.md`](../../docs/PUBLISHING_CHECKLIST.md) section **1** (AI agent / MCP registries).
