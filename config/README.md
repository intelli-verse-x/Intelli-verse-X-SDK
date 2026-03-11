# Config and secrets

Sensitive data (API keys, auth URLs, backend URLs, etc.) is kept in a **single common file** so it is not scattered or hardcoded in the repo.

## Setup

1. Copy the example file to the live config (do not commit the live file):
   ```bash
   cp config/keys.example.json config/keys.json
   ```
2. Edit `config/keys.json` and fill in real values where needed.
3. **Do not commit `config/keys.json`.** It is listed in `.gitignore`.

## Fetching keys

- **Unity (C#):** Use `IVXSecretsConfig` or the path `config/keys.json` relative to the project root (e.g. `Application.dataPath + "/../config/keys.json"` in Editor). See `AuthService.cs` for auth base URL.
- **Web3 / JS:** Read `config/keys.json` from the repo root when running locally, or use environment variables (e.g. `IVX_MORALIS_API_KEY`). For Web3 config, set `moralisApiKey` from this file or env.
- **Docs / scripts:** Any doc or script that needs a key should reference this file (e.g. “see `config/keys.example.json`” and “load from `config/keys.json`”).

## Keys reference

| Key | Used by | Description |
|-----|---------|-------------|
| `authBaseUrl` | Unity AuthService | Base URL for auth API (e.g. `http://localhost:3000/auth`). |
| `devBackendUrl` | Unity / docs | Development Nakama/backend URL (e.g. `localhost:7350`). |
| `prodBackendUrl` | Unity / docs | Production backend URL. |
| `apiKey` | Various | Generic API key when required. |
| `moralisApiKey` | Web3 SDK | Optional Moralis API key for Web3 features. |
| `mcpServerUrl` | .vscode/mcp.json | MCP server URL for local dev (e.g. `http://localhost:8080/mcp`). |
