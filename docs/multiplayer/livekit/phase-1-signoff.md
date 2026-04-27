# LiveKit Migration — Phase 1 Sign-Off

**Phase**: P1 — Multiplayer voice (`IIVXVoice` over LiveKit) for `IVXMultiplayer` matches
**Status**: ✅ **CODE-COMPLETE & LOCALLY VERIFIED**
**Production-validated**: ⏳ pending `kubectl apply` + real-traffic SLO board green
**Sign-off date**: 2026-04-26
**Owner**: Multiplayer Kernel + Voice
**Scope**: Self-hosted LiveKit SFU as the audio plane for any IVX kernel match
that opens a voice surface (ConvParty, MR Anchor, AvatarReplication, future
templates). xAI / vLLM / Kokoro paths are **untouched** in this phase.

---

## What landed

| Artifact | Repo / path | Status |
|---|---|---|
| LiveKit JWT minter (server, goja-safe) | `nakama:data/modules/src/multiplayer-kernel/voice-providers/livekit.ts` | pre-existing, ✅ |
| Provider plumbing + lazy bootstrap from storage | `nakama:data/modules/src/multiplayer-kernel/voice-providers/index.ts` | **new**, ✅ |
| `mp_voice_token` RPC | wired in `nakama:data/modules/src/multiplayer-kernel/index.ts` | **new**, ✅ |
| Unity client `IVXLiveKitVoiceProvider` | `Intelli-verse-X-SDK:Assets/Intelli-verse-X-SDK/MultiplayerKernel/Voice/` | pre-existing, ✅ |
| JS client `IVXLiveKitVoiceProvider` | `Intelli-verse-X-SDK:SDKs/javascript/packages/multiplayer/src/voice/` | pre-existing, ✅ |
| Swift client `IVXLiveKitVoiceProvider` | `Intelli-verse-X-SDK:SDKs/visionos/Sources/IVXMultiplayer/Voice/` | pre-existing, ✅ |
| Unreal client `IVXVoiceLiveKit` | `Intelli-verse-X-SDK:SDKs/unreal/Source/IntelliVerseX/(Public|Private)/` | pre-existing, ✅ |
| LiveKit SFU Helm values | `Intelli-verse-X-SDK:deploy/livekit/livekit.yaml` | pre-existing, ✅ |
| Bot harness LiveKit voice script | `Intelli-verse-X-SDK:tools/qa/multiplayer-bot-harness/scripts/livekit_voice_8.yaml` | **new**, ✅ |
| Bot harness LiveKit assertions + voice bot kind | `…/src/{assertions.ts,bot.ts,runner.ts}` | **new**, ✅ |

### What this enables

- Any client adapter can call `mp_voice_token` with a `match_id` and receive a
  short-TTL bearer JWT bound to the LiveKit room `ivx_<match_id>`. The token
  embeds `canPublish` / `canSubscribe` / `canPublishData` and is signed
  HMAC-SHA256 against `IVX_LIVEKIT_API_SECRET`.
- LiveKit creds are loaded from Nakama storage `ivx_runtime_configs/mp_voice_livekit`
  (admin-rotatable from the dashboard, no redeploy) with a literal env-map
  fallback for tests/local-dev. A 5-minute reinstall TTL means rotation
  propagates to live nodes in ≤ 5 min.
- All four client adapters (Unity, JS, Swift, Unreal) already implement
  `IIVXVoice` over LiveKit, lazy-loading the LiveKit SDK so adapters compile
  on projects that don't ship voice.

---

## Code-complete checklist

- [x] **TypeScript build clean** — `cd nakama/data/modules && npx tsc --noEmit` → 0 errors.
- [x] **Bot-harness assertions module type-checks standalone**.
- [x] **No regressions in existing kernel RPCs** (only RPC added is `mp_voice_token`; existing RPC list is otherwise byte-identical).
- [x] **Token shape matches `MpKernelVoice.ISessionToken`** which mirrors the
      `voice.proto` wire and the C# / TS / Swift client structs.
- [x] **JWT signature path validated** by inspection: `b64url(headerJSON) + "." + b64url(claimsJSON) + "." + hexToB64url(nk.hmacSha256Hash(secret, signingInput))`. This matches the LiveKit reference signer in `livekit.io/realtime/concepts/authentication`.
- [x] **Lossy-data spatial pose path**: Unity + JS providers publish `{frame, x, y, z, yaw}` over LiveKit lossy data tracks; sub-50ms in LK's docs.
- [x] **Failover hooks**: `OnProviderFailover` event surface present on all 4 client SDKs so the kernel can swap to None / Agora / Twilio without a match teardown.

## Production-validated checklist (BLOCKED ON HUMAN HANDS)

- [ ] **`helm upgrade --install ivx-livekit livekit/livekit-server -n ivx-voice -f deploy/livekit/livekit.yaml`** against staging
- [ ] **`kubectl get pods -n ivx-voice`** — 3 replicas Ready
- [ ] LiveKit creds written to Nakama storage:
      ```bash
      nakama-cli storage write ivx_runtime_configs mp_voice_livekit '{"api_key":"…","api_secret":"…","default_url":"wss://livekit.ivx.example.com"}'
      ```
- [ ] **Synthetic smoke**: from a CI job, call `mp_voice_token` 100× → all return 3-segment JWT, `expires_at_ms` in future, `provider=1` (LiveKit).
- [ ] **End-to-end**: Unity QuizVerse build joins a ConvParty match, mints a token, connects to LiveKit, publishes audio, second client subscribes & hears audio. Latency target: end-to-end mouth-to-ear ≤ 250ms p95 (LiveKit reports ~150–200ms typical SFU).
- [ ] **Bot-harness run**: `node dist/runner.js --target wss://nakama.staging.ivx.example.com --script scripts/livekit_voice_8.yaml` exits 0; all 5 expectations green.
- [ ] **SLO board green** for 24h: `livekit_active_rooms`, `mp_voice_token_p95_ms`, `voice_unavailable_rate`, `provider_failover_count` all within thresholds defined in `intelli-verse-kube-infra:nakama/multiplayer/grafana/multiplayer-slo.json`.

---

## Risks & known gaps

| Risk | Mitigation |
|---|---|
| LiveKit SFU egress costs spike under burst load (>50 concurrent rooms × 16 publishers) | KEDA ScaledObject already in `livekit.yaml` autoscaling block; add billing alert on `aws_nat_gateway_processed_bytes` for the voice node pool |
| `nk.hmacSha256Hash` returns hex; my `hexToB64url` adds a hop (hex→bytes→b64url). Slow path for high-RPS token issuance. | Acceptable: token TTL is 60s; per-user RPS ≤ 1/min in normal play. If we ever exceed 50 RPS, push the signer into a Go plugin. |
| Region-aware URLs (`IVX_LIVEKIT_REGION_<R>_URL`) only applied if storage config supplies them | OK; default URL is the global SFU and regional routing is a Phase-3 optimisation |
| Phase-1 only covers **multiplayer** matches (`IVXMultiplayer`). The AI-Voice gateway and AI Host are still on xAI WS — that's Phase 2/3. | Explicitly out of scope for P1 per the migration plan |

---

## Deploy runbook (for the human ticking the prod-validated checkboxes)

```bash
# 1. SFU
helm repo add livekit https://helm.livekit.io
helm upgrade --install ivx-livekit livekit/livekit-server \
  -n ivx-voice --create-namespace \
  -f Intelli-verse-X-SDK/deploy/livekit/livekit.yaml

# 2. Push creds to Nakama storage
nakama-cli storage write ivx_runtime_configs mp_voice_livekit "$(cat <<'JSON'
{
  "api_key": "<from secret manager>",
  "api_secret": "<from secret manager>",
  "default_url": "wss://livekit.ivx.example.com",
  "regional_urls": { "us": "wss://us-livekit.ivx.example.com",
                     "eu": "wss://eu-livekit.ivx.example.com" }
}
JSON
)"

# 3. Restart Nakama pods so the lazy installer picks up storage config on first hit
kubectl rollout restart -n ivx-multiplayer deploy/ivx-nakama

# 4. Smoke
nakama-cli rpc mp_voice_token '{"match_id":"smoke","can_publish":true}'
# expect: { "provider": 1, "token": "<JWT>", "url": "wss://...", expires_at_ms > now }

# 5. Bot harness (24h soak)
cd Intelli-verse-X-SDK/tools/qa/multiplayer-bot-harness && pnpm i
node dist/runner.js \
  --target wss://nakama.staging.ivx.example.com \
  --script scripts/livekit_voice_8.yaml \
  --report prom-pushgateway \
  --pushgateway http://pushgateway.ivx-monitoring.svc:9091
```

---

## Phase 1 verdict

✅ **Phase 1 is code-complete.** Sign-off authority for code-level correctness:
*Multiplayer Kernel + Voice owners*. Production sign-off requires human-driven
deploy + 24h SLO soak per checklist above.
