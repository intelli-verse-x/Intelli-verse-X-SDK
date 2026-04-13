# MCP tooling

| Script | Purpose |
|--------|---------|
| [`validate_server_card.py`](validate_server_card.py) | Validates [`.well-known/mcp/server-card.json`](../../.well-known/mcp/server-card.json) (JSON shape, unique tool names, parity with [`smithery.yaml`](../../smithery.yaml)). Run: `python3 tools/mcp/validate_server_card.py` |

CI runs this on changes to the server card or `smithery.yaml` (see [`.github/workflows/mcp-server-card.yml`](../../.github/workflows/mcp-server-card.yml)).

### CI: “serverInfo must be an object” / “tools must be an array”

The script reads **`.well-known/mcp/server-card.json`** at the repo root (next to `smithery.yaml`). Those messages mean the parsed JSON does not have:

- **`serverInfo`**: a JSON **object** (`{ "name": "...", "version": "..." }`), not `null`, not an array, not omitted.
- **`tools`**: a JSON **array** (`[ ... ]`), not an object and not `null`.

**Typical causes:** an empty `{}`, a placeholder file, a merge that dropped content, or the wrong branch where the full server card was never committed. **Fix:** restore the file from `main` or copy the canonical version from this repo’s [`.well-known/mcp/server-card.json`](../../.well-known/mcp/server-card.json), then re-run the validator locally: `python3 tools/mcp/validate_server_card.py`.
