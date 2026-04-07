# MCP tooling

| Script | Purpose |
|--------|---------|
| [`validate_server_card.py`](validate_server_card.py) | Validates [`.well-known/mcp/server-card.json`](../../.well-known/mcp/server-card.json) (JSON shape, unique tool names, parity with [`smithery.yaml`](../../smithery.yaml)). Run: `python3 tools/mcp/validate_server_card.py` |

CI runs this on changes to the server card or `smithery.yaml` (see [`.github/workflows/mcp-server-card.yml`](../../.github/workflows/mcp-server-card.yml)).
