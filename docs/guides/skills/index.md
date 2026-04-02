# AI Agent Skills

7 purpose-built skills that turn your AI coding assistant into an IntelliVerseX integration expert. Each skill teaches the agent how to set up, configure, and troubleshoot a specific part of the SDK.

---

## What Are Skills?

Skills are structured markdown guides (`SKILL.md`) that AI coding agents read to understand your SDK. When you ask your AI assistant something like "set up IntelliVerseX in my Unity project", the agent loads the matching skill and follows its instructions to make the right edits to your code.

**No code execution. No hidden calls. Just guided integration.**

---

## Available Skills

| Skill | File | Trigger Phrases |
|-------|------|----------------|
| [SDK Setup](sdk-setup.md) | `ivx-sdk-setup` | "set up IntelliVerseX", "integrate SDK", "bootstrap" |
| [Monetization](monetization.md) | `ivx-monetization` | "monetize my game", "add ads", "set up offerwall" |
| [Multiplayer](multiplayer.md) | `ivx-multiplayer` | "add multiplayer", "create lobby", "matchmaking" |
| [AI Integration](ai-integration.md) | `ivx-ai-integration` | "add AI host", "AI NPC", "AI voice chat" |
| [Live Operations](live-ops.md) | `ivx-live-ops` | "daily rewards", "season pass", "leagues" |
| [Quiz Content](quiz-content.md) | `ivx-quiz-content` | "add quiz", "daily quiz", "generate trivia" |
| [Cross-Platform](cross-platform.md) | `ivx-cross-platform` | "port to Unreal", "port to Godot", "feature parity" |

---

## Quick Install

=== "Cursor / Windsurf"

    Skills auto-activate when you open the repository. No install needed.

    ```
    .cursor/skills/
    ├── ivx-sdk-setup/SKILL.md
    ├── ivx-monetization/SKILL.md
    ├── ivx-multiplayer/SKILL.md
    ├── ivx-ai-integration/SKILL.md
    ├── ivx-live-ops/SKILL.md
    ├── ivx-quiz-content/SKILL.md
    └── ivx-cross-platform/SKILL.md
    ```

=== "Claude Code"

    ```bash
    /plugin marketplace add https://github.com/Intelli-verse-X/Intelli-verse-X-Unity-SDK
    /plugin install ivx-sdk-setup
    ```

=== "SkillsGate"

    ```bash
    skillsgate add @intelliversex/ivx-sdk-setup
    # or install all:
    skillsgate add @intelliversex/ivx-sdk-setup @intelliversex/ivx-monetization @intelliversex/ivx-multiplayer @intelliversex/ivx-ai-integration @intelliversex/ivx-live-ops @intelliversex/ivx-quiz-content @intelliversex/ivx-cross-platform
    ```
