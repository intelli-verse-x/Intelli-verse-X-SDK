# AI Agent Skills

35+ purpose-built skills that turn your AI coding assistant into an IntelliVerseX integration expert. Each skill teaches the agent how to set up, configure, and troubleshoot a specific part of the SDK.

---

## What Are Skills?

Skills are structured markdown guides (`SKILL.md`) that AI coding agents read to understand your SDK. When you ask your AI assistant something like "set up IntelliVerseX in my Unity project", the agent loads the matching skill and follows its instructions to make the right edits to your code.

**No code execution. No hidden calls. Just guided integration.**

For live in-game AI-agent RPC support by language and platform, see the
[Agent Skills Game-ID Platform Matrix](agent-skills-game-id-platform-matrix.md).
For a skill-by-skill map from Agent Skills to concrete SDK code paths, see the
[Agent Skill Code Reference](agent-skill-code-reference.md).

---

## Available Skills

| Skill | Folder | Example Trigger Phrases |
|-------|------|----------------|
| [3D Character Pipeline](ivx-3d-character-pipeline.md) | `ivx-3d-character-pipeline` | "3D character", "generate 3D model", |
| [AI Integration](ivx-ai-integration.md) | `ivx-ai-integration` | "add AI host", |
| [Accessibility](ivx-accessibility.md) | `ivx-accessibility` | "accessibility", "color blind mode", "screen reader", |
| [Analytics Pipeline](ivx-analytics-pipeline.md) | `ivx-analytics-pipeline` | "add analytics", "track events", |
| [Asset Manager](ivx-asset-manager.md) | `ivx-asset-manager` | "add asset", |
| [Asset Pipeline](ivx-asset-pipeline.md) | `ivx-asset-pipeline` | Various phrases |
| [Avatar Studio](ivx-avatar-studio.md) | `ivx-avatar-studio` | "create |
| [Character Factory](ivx-character-factory.md) | `ivx-character-factory` | "generate character", "create sprites", |
| [Competitor Intelligence Skill](ivx-competitor-intel.md) | `ivx-competitor-intel` | Various phrases |
| [Crashlytics](ivx-crashlytics.md) | `ivx-crashlytics` | "crash reporting", "error tracking", "add crashlytics", |
| [Cross-Platform SDK](ivx-cross-platform.md) | `ivx-cross-platform` | Various phrases |
| [DevOps & CI/CD](ivx-devops-cicd.md) | `ivx-devops-cicd` | "CI/CD", |
| [Economy Simulator](ivx-economy-simulator.md) | `ivx-economy-simulator` | "economy design", "currency flow", "balance economy", |
| [Environment Generator](ivx-environment-generator.md) | `ivx-environment-generator` |  |
| [Game Audio Factory](ivx-game-audio-factory.md) | `ivx-game-audio-factory` | "generate game audio", "create sound |
| [Game Design Studio](ivx-game-design-studio.md) | `ivx-game-design-studio` | "create GDD", |
| [Landing Page Generator](ivx-landing-page.md) | `ivx-landing-page` | "landing page", "game website", "pricing page", "coming soon page", |
| [Legal Compliance Skill](ivx-legal-compliance.md) | `ivx-legal-compliance` | Various phrases |
| [Live Operations](ivx-live-ops.md) | `ivx-live-ops` | "add live ops", "set up daily rewards", "add season pass", |
| [Localization](ivx-localization.md) | `ivx-localization` | "localize my game", "translate store listing", |
| [Marketing Kit Skill](ivx-marketing-kit.md) | `ivx-marketing-kit` | Various phrases |
| [Monetization](ivx-monetization.md) | `ivx-monetization` | "monetize my game", "add ads", |
| [Multiplayer](ivx-multiplayer.md) | `ivx-multiplayer` | "add multiplayer", |
| [Narrative Engine](ivx-narrative-engine.md) | `ivx-narrative-engine` | "dialog system", "branching dialog", |
| [Notification Orchestration](ivx-notification-orchestration.md) | `ivx-notification-orchestration` | "push notifications", "notification scheduling", |
| [Procedural AI](ivx-procedural-ai.md) | `ivx-procedural-ai` | "procedural generation", |
| [Quality Gates](ivx-quality-gates.md) | `ivx-quality-gates` | "quality gates", "CI validation", |
| [Quest System](ivx-quest.md) | `ivx-quest` | "add quests", "daily missions", "Scratch & Win", "Spin & Win", "IntelliDraws", "PvP challenge" |
| [Quiz Content Pipeline](ivx-quiz-content.md) | `ivx-quiz-content` | "add quiz", |
| [Remote Config](ivx-remote-config.md) | `ivx-remote-config` | "remote config", "server config", "feature flags", |
| [Roblox SDK](ivx-roblox.md) | `ivx-roblox` | Various phrases |
| [SDK Setup & Configuration](ivx-sdk-setup.md) | `ivx-sdk-setup` | "set up IntelliVerseX", "integrate SDK", "bootstrap SDK", |
| [Security & Anti-Cheat](ivx-security-anticheat.md) | `ivx-security-anticheat` | "anti-cheat", "prevent cheating", "server validation", |
| [Store Launcher](ivx-store-launcher.md) | `ivx-store-launcher` | "store assets", "app icon", "screenshots", |
| [UA & Marketing Strategy](ivx-ua-marketing-strategy.md) | `ivx-ua-marketing-strategy` | Various phrases |
| [UGC Pipeline](ivx-ugc-pipeline.md) | `ivx-ugc-pipeline` |  |

---

## Quick Install

=== "Cursor / Windsurf"

    Skills auto-activate when you open the repository. No install needed.

=== "Claude Code"

    ```bash
    /plugin marketplace add https://github.com/intelli-verse-x/Intelli-verse-X-SDK
    # Install specific skills as needed:
    /plugin install ivx-sdk-setup
    ```

=== "SkillsGate"

    ```bash
    skillsgate add @intelliversex/ivx-sdk-setup
    ```
