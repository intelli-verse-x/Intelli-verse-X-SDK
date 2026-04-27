# IVX Multiplayer — Documentation Index

This is the master index for everything multiplayer-related across the
IntelliVerseX repos. The kernel + adapter quick-reference lives at
[`SDKs/MULTIPLAYER_KERNEL_ADAPTERS.md`](../../SDKs/MULTIPLAYER_KERNEL_ADAPTERS.md);
this file links the *cluster* docs (XR / voice / agents / k8s / QA)
that grew up around it.

## 1. Kernel & schemas

| Doc | What it answers |
|-----|-----------------|
| [`SDKs/MULTIPLAYER_KERNEL_ADAPTERS.md`](../../SDKs/MULTIPLAYER_KERNEL_ADAPTERS.md) | Cross-engine matrix: which engine speaks which template |
| [`docs/multiplayer/error-taxonomy.md`](./error-taxonomy.md) | `ErrorCode` / `WarningCode` enum + retry guidance |
| [`docs/multiplayer/avatar-interop.md`](./avatar-interop.md) | GLB / RPM blendshapes / bone naming |
| [`schemas/multiplayer/*.proto`](../../schemas/multiplayer/) | Wire format (V1) |

## 2. XR cluster

| Doc | What it answers |
|-----|-----------------|
| [`Assets/Intelli-verse-X-SDK/MultiplayerKernel/XR/PICO_XR_NOTES.md`](../../Assets/Intelli-verse-X-SDK/MultiplayerKernel/XR/PICO_XR_NOTES.md) | Pico XR Unity bootstrap |
| [`SDKs/visionos/README.md`](../../SDKs/visionos/README.md) | visionOS Swift adapter (nakama-cpp + LiveKit-Swift + RealityKit) |
| [`SDKs/javascript/packages/multiplayer/examples/webxr/README.md`](../../SDKs/javascript/packages/multiplayer/examples/webxr/) | WebXR samples (Three.js / Babylon / A-Frame) |
| `nakama/data/modules/avatar_replication/README.md` | `AvatarReplicationMatch` Go module |
| `nakama/data/modules/mr_anchor/README.md` | `MixedRealityAnchorMatch` + cloud-anchor adapters |

## 3. Voice cluster

| Doc | What it answers |
|-----|-----------------|
| `infra/livekit/livekit.yaml` | Self-hosted SFU deploy |
| `Assets/Intelli-verse-X-SDK/Voice/LiveKitProvider/README.md` | Unity LiveKit voice provider |
| `SDKs/javascript/packages/voice/README.md` | JS LiveKit voice provider |
| `SDKs/visionos/Sources/IVXVoice/README.md` | Swift LiveKit voice provider |
| [`docs/multiplayer/error-taxonomy.md`](./error-taxonomy.md) | Voice error / fall-back rules |

## 4. Conversational-party + agent cluster

| Doc | What it answers |
|-----|-----------------|
| `nakama/data/modules/conversational_party/README.md` | `ConversationalPartyMatch` |
| `nakama/data/modules/iivxagent/README.md` | `IIVXAgent` kernel service |
| `Assets/Intelli-verse-X-SDK/MultiplayerKernel/Avatar/IIVXAvatar.cs` | Cross-platform avatar abstraction |
| `nakama/data/modules/moderation/README.md` | Real-time moderation pipeline |

## 5. Production / k8s cluster

| Doc | What it answers |
|-----|-----------------|
| `intelli-verse-kube-infra/nakama/multiplayer/README.md` | Why a separate multiplayer Nakama tier |
| `intelli-verse-kube-infra/nakama/multiplayer/deployment.yaml` | Pod spec, drain, OTel sidecar |
| `intelli-verse-kube-infra/nakama/multiplayer/keda-scaledobject.yaml` | KEDA scaling on Prom triggers |
| `intelli-verse-kube-infra/nakama/multiplayer/topology-routing-configmap.yaml` | Multi-region match routing |
| `intelli-verse-kube-infra/nakama/multiplayer/dr-replicator-cron.yaml` | DR snapshot cron |
| `intelli-verse-kube-infra/nakama/multiplayer/servicemonitor.yaml` | Prom scrape + alert rules |
| `intelli-verse-kube-infra/nakama/multiplayer/grafana/multiplayer-slo.json` | SLO dashboard |

## 6. QA + sign-off cluster

| Doc | What it answers |
|-----|-----------------|
| [`docs/multiplayer/qa-gates.md`](./qa-gates.md) | Gates A–F + PR sign-off template |
| [`tools/qa/multiplayer-bot-harness/README.md`](../../tools/qa/multiplayer-bot-harness/README.md) | Synthetic-bot harness |
| [`docs/multiplayer/staged-rollout.md`](./staged-rollout.md) | Cohort graduation playbook |
| [`docs/multiplayer/production-signoff-checklist.md`](./production-signoff-checklist.md) | Per-release sign-off |

## 7. Cross-cluster checklist when adding a new feature

When you add a new template / adapter / opcode, check that you've
updated **at least one doc in each of the clusters above** that the
change touches. The PR template in `qa-gates.md` makes this auditable.
