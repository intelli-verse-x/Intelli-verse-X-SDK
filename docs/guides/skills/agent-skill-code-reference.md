# Agent Skill Code Reference

This source map links each AI coding Agent Skill to the SDK code, tools, and
documentation it should inspect or modify. Use it when reviewing whether a skill
is backed by real code or only by guidance.

## Coverage Status

**Status:** GitHub documentation now has a central code-reference map for the
Agent Skills that are present in `.cursor/skills`.

This page does not mean every referenced feature is fully implemented on every
platform. It means each skill has an explicit repo path to check before an agent
edits a game integration.

## Core SDK And Platform References

| Area | Primary code paths |
|---|---|
| Unity SDK | `Assets/Intelli-verse-X-SDK/`, `Assets/_IntelliVerseXSDK/` |
| JavaScript SDK | `SDKs/javascript/src/`, `SDKs/javascript/packages/multiplayer/src/` |
| Unreal SDK | `SDKs/unreal/Source/IntelliVerseX/` |
| Godot SDK | `SDKs/godot/addons/intelliversex/` |
| Roblox SDK | `SDKs/roblox/src/`, `SDKs/roblox/examples/` |
| Defold SDK | `SDKs/defold/intelliversex/` |
| C++ SDK | `SDKs/cpp/include/intelliversex/`, `SDKs/cpp/src/intelliversex/` |
| Cocos2d-x SDK | `SDKs/cocos2dx/Classes/IntelliVerseX/` |
| Java SDK | `SDKs/java/src/main/java/com/intelliversex/sdk/` |
| Flutter SDK | `SDKs/flutter/lib/src/` |
| Web3 SDK | `SDKs/web3/src/` |
| visionOS SDK | `SDKs/visionos/Sources/IVXMultiplayer/` |
| Multiplayer schemas | `schemas/multiplayer/` |
| Avatar schemas | `schemas/avatar/` |
| Asset tools | `tools/asset-pipeline/` |
| Boilerplate tools | `tools/boilerplate/` |
| QA tools | `tools/qa/`, `tools/context/` |

## Skill To Code Map

| Skill | Skill file | Main code references |
|---|---|---|
| 3D Character Pipeline | `.cursor/skills/ivx-3d-character-pipeline/SKILL.md` | `tools/asset-pipeline/scaffold_character.py`, `tools/asset-pipeline/validate_character.py`, `tools/asset-pipeline/validate_specs.py`, `Assets/_IntelliVerseXSDK/Characters/IVXCharacterManager.cs` |
| Accessibility | `.cursor/skills/ivx-accessibility/SKILL.md` | `docs/guides/skills/ivx-accessibility.md`, `Assets/Intelli-verse-X-SDK/UI/`, `Assets/Intelli-verse-X-SDK/Localization/`, `SDKs/javascript/src/IVXManager.ts` |
| AI Integration | `.cursor/skills/ivx-ai-integration/SKILL.md` | `Assets/_IntelliVerseXSDK/AI/Core/IVXAISessionManager.cs`, `Assets/_IntelliVerseXSDK/AI/NPC/IVXAINPCDialogManager.cs`, `SDKs/javascript/src/IVXAIClient.ts`, `SDKs/javascript/src/ai-npc.ts`, `SDKs/javascript/src/ai-assistant.ts`, `SDKs/javascript/src/ai-content-generator.ts`, `SDKs/javascript/src/ai-moderator.ts`, `SDKs/javascript/src/ai-profiler.ts`, `SDKs/javascript/src/ai-voice-services.ts` |
| Analytics Pipeline | `.cursor/skills/ivx-analytics-pipeline/SKILL.md` | `Assets/Intelli-verse-X-SDK/Analytics/IVXAnalyticsManager.cs`, `SDKs/javascript/src/ivx-satori.ts`, `docs/api/analytics.md`, `docs/modules/satori.md` |
| Asset Manager | `.cursor/skills/ivx-asset-manager/SKILL.md` | `tools/asset-pipeline/manage_asset.py`, `tools/asset-pipeline/validate_character.py`, `tools/asset-pipeline/validate_sound_manifest.py` |
| Asset Pipeline | `.cursor/skills/ivx-asset-pipeline/SKILL.md` | `tools/asset-pipeline/`, `tools/asset-pipeline/generate_spritesheet.py`, `tools/asset-pipeline/scaffold_character.py`, `tools/asset-pipeline/validate_specs.py` |
| Avatar Studio | `.cursor/skills/ivx-avatar-studio/SKILL.md` | `Assets/Intelli-verse-X-SDK/MultiplayerKernel/Avatar/`, `Assets/Intelli-verse-X-SDK/MultiplayerKernel/Voice/`, `schemas/avatar/`, `schemas/multiplayer/templates/avatar_replication.proto`, `SDKs/javascript/packages/multiplayer/src/avatar/`, `SDKs/visionos/Sources/IVXMultiplayer/Avatar/` |
| Character Factory | `.cursor/skills/ivx-character-factory/SKILL.md` | `tools/asset-pipeline/scaffold_character.py`, `tools/asset-pipeline/validate_character.py`, `Assets/_IntelliVerseXSDK/Characters/IVXCharacterManager.cs` |
| Competitor Intelligence | `.cursor/skills/ivx-competitor-intel/SKILL.md` | `docs/guides/skills/ivx-competitor-intel.md`, `docs/analysis/` |
| Crashlytics | `.cursor/skills/ivx-crashlytics/SKILL.md` | `docs/guides/skills/ivx-crashlytics.md`, `Assets/Intelli-verse-X-SDK/Core/IVXLogger.cs`, `SDKs/javascript/src/IVXManager.ts` |
| Cross-Platform SDK | `.cursor/skills/ivx-cross-platform/SKILL.md` | `SDKs/`, `docs/platforms/`, `docs/FEATURE_COVERAGE_MATRIX.md`, `schemas/multiplayer/` |
| DevOps & CI/CD | `.cursor/skills/ivx-devops-cicd/SKILL.md` | `.github/workflows/`, `tools/scripts/reorganize_for_upm.py`, `tools/context/validate_context.py`, `docs/PUBLISHING_CHECKLIST.md` |
| Economy Simulator | `.cursor/skills/ivx-economy-simulator/SKILL.md` | `Assets/Intelli-verse-X-SDK/Hiro/Systems/IVXEconomySystem.cs`, `Assets/Intelli-verse-X-SDK/Backend/IVXWalletManager.cs`, `SDKs/javascript/src/IVXHiroSystems.ts`, `docs/modules/wallet.md` |
| Environment Generator | `.cursor/skills/ivx-environment-generator/SKILL.md` | `docs/guides/skills/ivx-environment-generator.md`, `tools/asset-pipeline/`, `Assets/Intelli-verse-X-SDK/Editor/IVXSceneImporter.cs` |
| Game Audio Factory | `.cursor/skills/ivx-game-audio-factory/SKILL.md` | `tools/asset-pipeline/validate_sound_manifest.py`, `Assets/Intelli-verse-X-SDK/Core/IVXAudioManager.cs`, `docs/guides/skills/ivx-game-audio-factory.md` |
| Game Design Studio | `.cursor/skills/ivx-game-design-studio/SKILL.md` | `tools/asset-pipeline/gdd_to_entity.py`, `tools/boilerplate/generate_starter.py`, `tools/boilerplate/project_analyzer.py`, `tools/boilerplate/wire_integrator.py` |
| Landing Page | `.cursor/skills/ivx-landing-page/SKILL.md` | `docs/guides/skills/ivx-landing-page.md`, `tools/boilerplate/templates/`, `docs/strategy/` |
| Legal Compliance | `.cursor/skills/ivx-legal-compliance/SKILL.md` | `docs/guides/skills/ivx-legal-compliance.md`, `docs/guides/privacy.md`, `docs/strategy/` |
| Live Ops | `.cursor/skills/ivx-live-ops/SKILL.md` | `Assets/Intelli-verse-X-SDK/Hiro/Systems/`, `Assets/_IntelliVerseXSDK/Retention/`, `Assets/_IntelliVerseXSDK/Missions/`, `SDKs/javascript/src/IVXHiroSystems.ts`, `docs/api/hiro.md` |
| Localization | `.cursor/skills/ivx-localization/SKILL.md` | `Assets/Intelli-verse-X-SDK/Localization/IVXLanguageManager.cs`, `Assets/Intelli-verse-X-SDK/Localization/IVXLocalizedText.cs`, `docs/api/localization.md`, `SDKs/javascript/src/IVXManager.ts` |
| Marketing Kit | `.cursor/skills/ivx-marketing-kit/SKILL.md` | `docs/guides/skills/ivx-marketing-kit.md`, `docs/strategy/`, `tools/asset-pipeline/` |
| Monetization | `.cursor/skills/ivx-monetization/SKILL.md` | `Assets/Intelli-verse-X-SDK/Monetization/`, `Assets/Intelli-verse-X-SDK/Hiro/Systems/IVXOfferwallSystem.cs`, `Assets/Intelli-verse-X-SDK/Hiro/Systems/IVXSmartAdTimerSystem.cs`, `docs/api/monetization.md` |
| Multiplayer | `.cursor/skills/ivx-multiplayer/SKILL.md` | `Assets/Intelli-verse-X-SDK/MultiplayerKernel/`, `Assets/_IntelliVerseXSDK/Multiplayer/`, `SDKs/javascript/packages/multiplayer/src/`, `tools/qa/multiplayer-bot-harness/`, `schemas/multiplayer/`, `docs/multiplayer/` |
| Narrative Engine | `.cursor/skills/ivx-narrative-engine/SKILL.md` | `docs/guides/skills/ivx-narrative-engine.md`, `SDKs/javascript/src/ai-npc.ts`, `Assets/_IntelliVerseXSDK/AI/NPC/IVXAINPCDialogManager.cs` |
| Notification Orchestration | `.cursor/skills/ivx-notification-orchestration/SKILL.md` | `Assets/_IntelliVerseXSDK/Notifications/IVXPushNotificationManager.cs`, `docs/guides/skills/ivx-notification-orchestration.md` |
| Procedural AI | `.cursor/skills/ivx-procedural-ai/SKILL.md` | `docs/guides/skills/ivx-procedural-ai.md`, `SDKs/javascript/src/ai-content-generator.ts`, `tools/asset-pipeline/` |
| Quality Gates | `.cursor/skills/ivx-quality-gates/SKILL.md` | `tools/context/validate_context.py`, `tools/asset-pipeline/validate_specs.py`, `tools/asset-pipeline/validate_character.py`, `tools/asset-pipeline/validate_sound_manifest.py`, `tools/qa/` |
| Quest System | `.cursor/skills/ivx-quest/SKILL.md` | `Assets/_IntelliVerseXSDK/Quest/IVXQuestManager.cs`, `Assets/_IntelliVerseXSDK/Missions/IVXDailyMissionsManager.cs`, `docs/modules/quest.md` |
| Quiz Content | `.cursor/skills/ivx-quiz-content/SKILL.md` | `Assets/Intelli-verse-X-SDK/Quiz/`, `Assets/Intelli-verse-X-SDK/Samples~/QuizDemo/`, `docs/guides/skills/ivx-quiz-content.md`, `SDKs/javascript/src/IVXManager.ts` |
| Remote Config | `.cursor/skills/ivx-remote-config/SKILL.md` | `docs/guides/skills/ivx-remote-config.md`, `docs/configuration/`, `Assets/Intelli-verse-X-SDK/Core/`, `SDKs/javascript/src/IVXConfig.ts` |
| Roblox SDK | `.cursor/skills/ivx-roblox/SKILL.md` | `SDKs/roblox/src/`, `SDKs/roblox/examples/`, `SDKs/roblox/plugin/`, `docs/platforms/roblox.md` |
| SDK Setup | `.cursor/skills/ivx-sdk-setup/SKILL.md` | `Assets/Intelli-verse-X-SDK/Bootstrap/`, `Assets/Intelli-verse-X-SDK/Editor/IVXSetupWizard.cs`, `SDKs/*/README.md`, `docs/getting-started/`, `docs/configuration/` |
| Security & Anti-Cheat | `.cursor/skills/ivx-security-anticheat/SKILL.md` | `docs/guides/skills/ivx-security-anticheat.md`, `Assets/Intelli-verse-X-SDK/Backend/IVXNakamaManager.cs`, `SDKs/javascript/src/IVXManager.ts` |
| Store Launcher | `.cursor/skills/ivx-store-launcher/SKILL.md` | `docs/guides/skills/ivx-store-launcher.md`, `docs/PUBLISHING_CHECKLIST.md`, `tools/scripts/reorganize_for_upm.py` |
| UA & Marketing Strategy | `.cursor/skills/ivx-ua-marketing-strategy/SKILL.md` | `docs/guides/skills/ivx-ua-marketing-strategy.md`, `docs/strategy/`, `Assets/Intelli-verse-X-SDK/Analytics/IVXAnalyticsManager.cs` |
| UGC Pipeline | `.cursor/skills/ivx-ugc-pipeline/SKILL.md` | `docs/guides/skills/ivx-ugc-pipeline.md`, `Assets/Intelli-verse-X-SDK/Storage/`, `Assets/Intelli-verse-X-SDK/Backend/IVXNakamaManager.cs` |

## Live Agent Skills Runtime References

For live in-game AI agents, use these runtime-specific references:

| Runtime surface | Code references |
|---|---|
| Server schemas | `schemas/multiplayer/services/agent.proto`, `schemas/multiplayer/templates/conversational_party.proto` |
| JS typed client | `SDKs/javascript/packages/multiplayer/src/api.ts`, `SDKs/javascript/packages/multiplayer/src/client.ts` |
| JS voice/avatar | `SDKs/javascript/packages/multiplayer/src/voice/`, `SDKs/javascript/packages/multiplayer/src/avatar/`, `SDKs/javascript/packages/multiplayer/src/webxr/` |
| Unity avatar/voice | `Assets/Intelli-verse-X-SDK/MultiplayerKernel/Avatar/`, `Assets/Intelli-verse-X-SDK/MultiplayerKernel/Voice/` |
| QA harness | `tools/qa/multiplayer-bot-harness/` |
| Platform status | `docs/guides/skills/agent-skills-game-id-platform-matrix.md`, `docs/multiplayer/NAKAMA_SERVER_SYNC_STATUS_2026-05-01.md` |

## Review Rule

Before marking a skill as production-ready, check:

1. Its `.cursor/skills/<skill>/SKILL.md` exists.
2. Its GitHub docs page under `docs/guides/skills/` exists.
3. This page lists concrete code paths for the feature.
4. The referenced code path exists in the repo.
5. At least one local verification command exists in the skill doc, platform
   doc, or QA gate.
