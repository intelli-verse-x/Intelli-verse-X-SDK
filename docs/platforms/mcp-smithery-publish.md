# MCP host: Smithery publish and OAuth metadata

This document describes how the IntelliVerseX hosted MCP endpoint (`https://mcp.intelli-verse-x.ai/api/mcp`) must behave for [Smithery](https://smithery.ai/docs/build/publish) URL publishing and OAuth discovery.

## Verification

Probed from the public internet (re-run after infra changes):

| URL | Last known | Expected when production-ready |
|-----|------------|-------------------------------|
| `GET https://mcp.intelli-verse-x.ai/.well-known/mcp/server-card.json` | **404** | **200**, `Content-Type: application/json; charset=utf-8`, body = repo [`.well-known/mcp/server-card.json`](../../.well-known/mcp/server-card.json) |
| `HEAD` same URL | (same) | **200**; no auth on this path |
| `GET https://mcp.intelli-verse-x.ai/api/mcp` (no auth) | **401** JSON | **401** (not 403) for MCP route; optional `WWW-Authenticate` (see Fix 4) |
| `GET https://mcp.intelli-verse-x.ai/.well-known/oauth-protected-resource` | **404** | **404** until RFC 9728 is fully implemented (Fix 3) |
| `GET https://intelli-verse-x.ai/.well-known/oauth-authorization-server` | **404** | **404** until a real OAuth AS publishes metadata at that issuer |

**Repo automation:** `python3 tools/mcp/validate_server_card.py` ensures JSON shape and **tool list parity** with [`smithery.yaml`](../../smithery.yaml). CI: [`.github/workflows/mcp-server-card.yml`](../../.github/workflows/mcp-server-card.yml) (includes an **informational** curl to production; non-200 does not fail the job until you deploy).

Smithery’s publish flow performs OAuth / Protected Resource metadata discovery. Non-conforming responses (404 on metadata URLs, or unexpected status on the metadata document) produce errors such as `auth_required` and `Resource Server Metadata response (unexpected HTTP status code)`.

## Smithery UI: common release errors

### Warning: “No config schema provided”

Smithery warns when users will not be prompted for API keys or other session settings. **Fix:** add a `configSchema` to [`smithery.yaml`](../../smithery.yaml) (JSON Schema with [`x-from` / `x-to`](https://smithery.ai/docs/build/session-config) for headers). Merge to your default branch and **start a new release** so Smithery picks up the file.

### “Initialization failed with status **503**” / “advertise `/.well-known/mcp/server-card.json`”

This usually means **automatic scanning** of `https://mcp.intelli-verse-x.ai/api/mcp` failed (auth wall, overload, WAF, or upstream **503**). Smithery then asks you to serve a **static server card** on the **same origin** so metadata does not depend on a live MCP handshake during publish.

**Do both:**

1. Deploy [`.well-known/mcp/server-card.json`](../../.well-known/mcp/server-card.json) at `https://mcp.intelli-verse-x.ai/.well-known/mcp/server-card.json` (**200**, public, no API key) — see [Fix 1](#fix-1-static-server-card-recommended-for-publish) and [`infra/mcp-well-known/README.md`](../../infra/mcp-well-known/README.md).
2. Ensure Smithery’s scanner is not blocked: allow User-Agent `SmitheryBot/1.0`, prefer **401** not **403** on the MCP route when unauthenticated, and confirm your origin is not returning **503** to Cloudflare Workers during the release job.

If 503 persists **after** the server card returns 200, inspect MCP service health and load balancer/backend capacity for requests from Smithery’s network.

## Fix 1: Static server card (recommended for publish)

Serve the repository file [`.well-known/mcp/server-card.json`](../../.well-known/mcp/server-card.json) at the **same origin** as the MCP URL:

- **Path:** `/.well-known/mcp/server-card.json`
- **Full URL:** `https://mcp.intelli-verse-x.ai/.well-known/mcp/server-card.json`
- **Response:** `200`, `Content-Type: application/json`

This matches [Smithery’s static server card](https://smithery.ai/docs/build/publish#static-server-card-manual-metadata) format (`serverInfo`, `authentication`, `tools`, …) and can bypass automatic scanning when discovery fails.

**Deployment runbook (production headers, nginx/ALB/S3 edge cases):** [`infra/mcp-well-known/README.md`](../../infra/mcp-well-known/README.md)

This path must be **public** (no API key). The MCP API at `/api/mcp` remains protected.

## Fix 2: WAF and bot protection

Allow automated checks from Smithery:

- User-Agent: `SmitheryBot/1.0 (+https://smithery.ai)`
- Prefer **401 Unauthorized** (not **403 Forbidden**) for unauthenticated access to OAuth-protected MCP routes, per Smithery troubleshooting.

## Fix 3: RFC 9728 (optional, when authorization server metadata exists)

If you expose a full OAuth 2.0 authorization server with metadata at:

`https://<issuer>/.well-known/oauth-authorization-server`

then you can also serve **RFC 9728** Protected Resource Metadata at `/.well-known/oauth-protected-resource` (see [`.well-known/oauth-protected-resource.sample.json`](../../.well-known/oauth-protected-resource.sample.json)).

**Important:** `authorization_servers` in that document must point to an issuer that returns a **valid** authorization-server metadata document. Until that exists, rely on the static server card (Fix 1).

## Fix 4: MCP server behavior (service team)

For unauthenticated requests to the MCP endpoint, consider adding a **`WWW-Authenticate`** header (per MCP authorization and OAuth discovery) so clients can discover resource metadata without relying only on well-known path heuristics. Current behavior: 401 JSON body only, no `WWW-Authenticate`.
