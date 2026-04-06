---
name: intelliversex-game-sdk
description: >
  This skill should be used when the user wants to manage a Nakama game backend via MCP
  (health checks, auth, RPC, Hiro live-ops config, Satori analytics, players, wallets,
  storage, webhooks, metrics, or data lake settings), or when integrating IntelliVerseX
  server tooling from an AI agent. Use it for operational and admin tasks against the
  hosted IntelliVerseX MCP endpoint—not for Unity C# client code edits unless the user
  explicitly asks for both.
---

# IntelliVerseX Game SDK (MCP)

This skill describes how AI agents should use the **IntelliVerseX MCP server** to operate game backends: **Nakama**, **Hiro** live-ops, and **Satori**-style analytics configuration, plus player and storage inspection.

## MCP endpoint

- **Transport:** Streamable HTTP (`streamable-http` in [`smithery.yaml`](../../smithery.yaml))
- **URL:** `https://mcp.intelli-verse-x.ai/api/mcp`

Configure this URL in the user’s MCP client (Cursor, Claude Desktop, Gemini Code Assist, etc.). The server exposes **50+ tools**; names are listed in `smithery.yaml` under `tools:`.

## When to use which tools (summary)

| Area | Example tool names (see `smithery.yaml` for full list) |
|------|----------------------------------------------------------|
| Nakama core | `nakama_health`, `nakama_auth`, `nakama_rpc`, `nakama_account`, `nakama_build`, `nakama_restart`, `nakama_rpc_list` |
| Hiro live-ops | `hiro_config_get`, `hiro_config_set`, `reward_bucket_progress`, `personalizer_preview`, `personalizer_set_override` |
| Satori / analytics | `satori_config_get`, `satori_config_set`, `events_timeline`, `experiment_setup`, `flag_toggle`, `live_event_schedule` |
| Players | `player_inspect`, `player_search` |
| Economy | `wallet_view`, `wallet_grant`, `wallet_reset`, `inventory_grant` |
| Messaging | `mailbox_send`, `message_broadcast` |
| Storage | `storage_list`, `storage_read`, `storage_write` |
| Config & cache | `config_export`, `config_import`, `cache_invalidate` |
| Webhooks | `webhooks_list`, `webhooks_upsert`, `webhooks_delete`, `webhooks_test` |
| Metrics | `metrics_prometheus`, `metrics_set_alert` |
| Taxonomy / data lake | `taxonomy_*`, `datalake_*` |

## Safety and scope

- Prefer **read-only** tools (`*_get`, `*_list`, `*_inspect`, `health`) until the user asks for mutations.
- Destructive or high-impact actions (`wallet_reset`, `cache_invalidate`, `nakama_restart`, `webhooks_delete`, `datalake_delete_target`, etc.) require explicit user confirmation when possible.
- Do **not** embed secrets in chat; use the user’s existing MCP/auth configuration.

## Relationship to the Unity / multi-platform SDK

- This MCP skill is for **server operations and admin workflows**.
- For **client integration** (Unity, Unreal, Godot, etc.), use the repository’s documentation and agent guides under `docs/guides/skills/` and the platform packs in `SDKs/`—not this MCP tool list alone.

## Registry metadata

Smithery server metadata lives in the repo root [`smithery.yaml`](../../smithery.yaml) (`name: intelliversex-game-sdk`).
