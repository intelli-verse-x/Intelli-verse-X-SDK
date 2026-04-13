# Deploy `/.well-known/mcp/server-card.json` on `mcp.intelli-verse-x.ai`

The MCP streamable HTTP endpoint is `https://mcp.intelli-verse-x.ai/api/mcp`. Smithery and other clients expect **the same origin** to serve a **static** MCP server card at:

`https://mcp.intelli-verse-x.ai/.well-known/mcp/server-card.json`

**Source of truth in git:** [`.well-known/mcp/server-card.json`](../../.well-known/mcp/server-card.json)  
**CI:** [`.github/workflows/mcp-server-card.yml`](../../.github/workflows/mcp-server-card.yml) validates JSON + parity with [`smithery.yaml`](../../smithery.yaml).

---

## Production response contract (edge cases)

| Concern | Requirement |
|--------|-------------|
| **URL** | Exact path `/.well-known/mcp/server-card.json` (case-sensitive on some stacks). No trailing slash. |
| **Method** | **GET** returns `200` and body. **HEAD** should return `200` (same headers; nginx serves HEAD automatically). |
| **HTTPS** | TLS 1.2+ only; HTTP should redirect to HTTPS (301/308). |
| **Status** | **200** for successful discovery — not **3xx** (Smithery may not follow) and not **401/403** on this path (public metadata). |
| **Content-Type** | `application/json; charset=utf-8` |
| **Body** | Valid JSON, UTF-8, **no BOM** |
| **Caching** | `Cache-Control: public, max-age=300` (or similar short TTL) so updates propagate; optional `ETag` |
| **CORS** | `Access-Control-Allow-Origin: *` (or include Smithery origins) for browser-based checks |
| **Compression** | `gzip` optional; ensure scanners accept identity |
| **WAF** | Allow User-Agent `SmitheryBot/1.0 (+https://smithery.ai)` — see [docs/platforms/mcp-smithery-publish.md](../../docs/platforms/mcp-smithery-publish.md) |
| **Auth** | This path must **not** require API keys (unlike `/api/mcp`) |

---

## Option A — Reverse proxy (nginx / Envoy / Traefik)

Copy [nginx-location.conf](nginx-location.conf) into your MCP gateway’s server block for `mcp.intelli-verse-x.ai`, adjusting `root` to the directory where you deploy the file from this repo.

---

## Option B — AWS Application Load Balancer fixed response

Use a **listener rule** with priority higher than the default:

1. **Condition:** Path is `/.well-known/mcp/server-card.json` (exact path rule if your ALB supports it; otherwise use a prefix rule that matches only this path).
2. **Action:** Fixed response, status `200`, content type `application/json; charset=utf-8`.
3. **Body:** paste the **minified or formatted** contents of `.well-known/mcp/server-card.json` (keep under ALB body size limits; current file is small).

**Edge case:** ALB fixed response has a size limit (~1024 bytes on some setups for old APIs — verify current quota). If the body is too large, use Option A or S3.

---

## Option C — Amazon S3 + CloudFront

1. Upload `server-card.json` to a bucket key `well-known/mcp/server-card.json` (no leading dot in S3 key; map URI in CloudFront).
2. CloudFront behavior: path pattern `.well-known/mcp/server-card.json` → S3 origin with correct object key / function URL rewrite.
3. Set `Content-Type` metadata on the object.

---

## Option D — Application route

If the MCP service is a single app (Node, Go, etc.), add an explicit route **before** auth middleware:

`GET /.well-known/mcp/server-card.json` → static file or embedded JSON, **no authentication**.

---

## Verify after deploy

```bash
curl -sS -o /dev/null -w "%{http_code}\n" \
  https://mcp.intelli-verse-x.ai/.well-known/mcp/server-card.json
# expect: 200

curl -sS -I https://mcp.intelli-verse-x.ai/.well-known/mcp/server-card.json | tr -d '\r'
# expect: Content-Type: application/json

python3 tools/mcp/validate_server_card.py
# run from repo clone — already green before deploy; after deploy, optional: jq empty on downloaded JSON
```

---

## Drift prevention

After changing [`smithery.yaml`](../../smithery.yaml) or the server card, run `python3 tools/mcp/validate_server_card.py` locally; CI runs the same check on push/PR.
