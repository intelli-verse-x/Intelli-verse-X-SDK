# Agent skills (MCP) in this repository

This folder contains **versioned, publishable** agent skill packages for registries (e.g. Smithery Skills) that expect a directory with a **`SKILL.md`** file.

## Contents

| Path | Purpose |
|------|---------|
| [`intelliversex-game-sdk/SKILL.md`](intelliversex-game-sdk/SKILL.md) | MCP-focused skill: YAML frontmatter (`name`, `description`) + instructions for using the hosted IntelliVerseX MCP server (Nakama, Hiro, Satori, tools listed in [`smithery.yaml`](../smithery.yaml)). |
| [`platforms/`](platforms/README.md) | Per-engine **`SKILL.md`** stubs that point at `SDKs/<engine>/` (or Unity UPM) and [`docs/platforms/`](../docs/platforms/index.md). |

## Codex plugin skill IDs → documentation (GitHub)

[`.codex-plugin/plugin.json`](../.codex-plugin/plugin.json) lists seven skill IDs. Canonical **Markdown** guides (no separate `SKILL.md` per ID in this folder yet) live under **`docs/guides/skills/`**:

| Skill ID | Guide on GitHub |
|----------|-----------------|
| `ivx-sdk-setup` | [`docs/guides/skills/ivx-sdk-setup.md`](../docs/guides/skills/ivx-sdk-setup.md) |
| `ivx-monetization` | [`docs/guides/skills/ivx-monetization.md`](../docs/guides/skills/ivx-monetization.md) |
| `ivx-multiplayer` | [`docs/guides/skills/ivx-multiplayer.md`](../docs/guides/skills/ivx-multiplayer.md) |
| `ivx-ai-integration` | [`docs/guides/skills/ivx-ai-integration.md`](../docs/guides/skills/ivx-ai-integration.md) |
| `ivx-live-ops` | [`docs/guides/skills/ivx-live-ops.md`](../docs/guides/skills/ivx-live-ops.md) |
| `ivx-quiz-content` | [`docs/guides/skills/ivx-quiz-content.md`](../docs/guides/skills/ivx-quiz-content.md) |
| `ivx-cross-platform` | [`docs/guides/skills/ivx-cross-platform.md`](../docs/guides/skills/ivx-cross-platform.md) |

To expose **literal `SKILL.md`** files for marketplaces, add `skills/<skill-id>/SKILL.md` in a future change (or symlink locally); keep content in sync with the guides above to avoid drift.

## Related files (repo root)

| File | Purpose |
|------|---------|
| [`smithery.yaml`](../smithery.yaml) | Smithery MCP registry metadata: `url`, `transport`, `tools`, `repository`. |
| [`.well-known/mcp/server-card.json`](../.well-known/mcp/server-card.json) | Static server card JSON — must be deployed at the MCP origin for Smithery (see [`docs/platforms/mcp-smithery-publish.md`](../docs/platforms/mcp-smithery-publish.md)). |
| [`.codex-plugin/plugin.json`](../.codex-plugin/plugin.json) | OpenAI Codex plugin manifest: seven skill IDs + MCP server block. |

## `.agents/skills/` (generic agent discovery)

Some clients scan **`<project>/.agents/skills/`** or **`~/.agents/skills/`**. This repo includes a mirrored tree at [`.agents/skills/`](../.agents/skills/README.md) (same skill packs as here, paths adjusted). Install scripts: [`tools/scripts/install-agents-skills.ps1`](../tools/scripts/install-agents-skills.ps1), [`tools/scripts/install-agents-skills.sh`](../tools/scripts/install-agents-skills.sh).

## Cursor / `.cursor/skills/`

The [`.gitignore`](../.gitignore) ignores **`.cursor/*`** except **`.cursor/skills/`**, so teams can commit optional Cursor skill trees. See [`.cursor/skills/README.md`](../.cursor/skills/README.md). Narrative guides for many topics live under [`docs/guides/skills/`](../docs/guides/skills/index.md).

## Publishing checklist

See [`docs/PUBLISHING_CHECKLIST.md`](../docs/PUBLISHING_CHECKLIST.md) section **1** and subsection **Flash sprint — owner actions**.
