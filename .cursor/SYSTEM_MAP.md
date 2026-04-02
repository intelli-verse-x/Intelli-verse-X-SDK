# IntelliVerseX SDK — System Map

Master index of all files, modules, and their relationships.

## Context System

| File | Purpose | Authority Level |
|------|---------|----------------|
| `.cursorrules` | Primary AI rules | Highest |
| `.cursor/NON_GOALS.md` | Scope boundaries | High |
| `.cursor/AI_GUARDRAILS.md` | AI permissions | High |
| `.cursor/HOT_CONTEXT.md` | Quick reference | Medium |
| `.cursor/ANTI_PATTERNS.md` | What not to code | Medium |
| `.cursor/context.md` | Vision and principles | High |
| `.cursor/architecture.md` | System structure | High |
| `.cursor/naming-and-style.md` | Naming conventions | Medium |
| `.cursor/assumptions.md` | Explicit assumptions | Medium |
| `.cursor/INTENTS.md` | AI intent registry | Medium |
| `.cursor/commands.json` | Command registry | Medium |
| `.cursor/FRESHNESS.md` | Freshness tracker | Low |
| `.cursor/SYSTEM_MAP.md` | This file | Low |

## SDK Modules

| Module | Path | Assembly |
|--------|------|----------|
| Core / Bootstrap | `Assets/Intelli-verse-X-SDK/` | `IntelliVerseX.Core` |
| AI / LLM Stack | `Assets/_IntelliVerseXSDK/AI/` | `IntelliVerseX.AI` |
| Backend / Hiro | `Assets/_IntelliVerseXSDK/Backend/` | `IntelliVerseX.Backend` |
| Multiplayer | `Assets/_IntelliVerseXSDK/Multiplayer/` | `IntelliVerseX.Multiplayer` |
| Platform / XR | `Assets/_IntelliVerseXSDK/Platform/` | `IntelliVerseX.Platform` |
| Console | `Assets/_IntelliVerseXSDK/Console/` | `IntelliVerseX.Console` |
| Analytics / Satori | `Assets/_IntelliVerseXSDK/Analytics/` | `IntelliVerseX.Analytics` |
| Discord Social | `Assets/_IntelliVerseXSDK/Discord/` | `IntelliVerseX.Discord` |
| Monetization | `Assets/Intelli-verse-X-SDK/Monetization/` | `IntelliVerseX.Monetization` |

## Cross-Platform SDKs

| Platform | Path | Language |
|----------|------|----------|
| JavaScript/TypeScript | `SDKs/javascript/` | TypeScript |
| Web3 | `SDKs/web3/` | TypeScript |
| Java/Android | `SDKs/java/` | Java |
| Flutter/Dart | `SDKs/flutter/` | Dart |
| Unreal Engine 5 | `SDKs/unreal/` | C++ |
| Godot 4 | `SDKs/godot/` | GDScript |
| Defold | `SDKs/defold/` | Lua |
| C++ | `SDKs/cpp/` | C++ |
| Cocos2d-x | `SDKs/cocos2dx/` | C++ |

## Documentation

| Section | Path |
|---------|------|
| Docs site | `docs/` |
| Guides | `docs/guides/` |
| API Reference | `docs/api/` |
| Platform docs | `docs/platforms/` |
| Samples | `docs/samples/` |

## CI/CD

| Workflow | Path |
|----------|------|
| Unity Tests | `.github/workflows/unity-tests.yml` |
| C++ SDK | `.github/workflows/cpp-sdk.yml` |
| Flutter SDK | `.github/workflows/flutter-sdk.yml` |
| Context Validation | `.github/workflows/context-validation.yml` |
| Documentation | `.github/workflows/docs.yml` |
