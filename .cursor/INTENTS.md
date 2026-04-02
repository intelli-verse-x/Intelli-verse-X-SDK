# IntelliVerseX SDK — Intents Registry

Defines the explicit intents that AI agents can handle for this project.

## Development Intents

| Intent | Description | Typical Trigger |
|--------|-------------|----------------|
| `setup-sdk` | Install and bootstrap the SDK | "Set up IntelliVerseX" |
| `add-feature` | Add a new SDK feature module | "Add multiplayer" |
| `fix-bug` | Diagnose and fix a bug | "Fix the auth flow" |
| `refactor` | Improve code structure | "Refactor the manager" |
| `add-docs` | Create or update documentation | "Document the API" |
| `port-feature` | Port a feature to another platform | "Port to Godot" |
| `configure` | Set up configuration | "Configure ads" |
| `optimize` | Performance improvement | "Reduce GC allocations" |
| `test` | Add or fix tests | "Add tests for auth" |

## Scope Rules

- All intents must respect `.cursor/NON_GOALS.md` boundaries
- Read-only zones are never modified (see `.cursorrules`)
- New features require architecture alignment (see `.cursor/architecture.md`)
