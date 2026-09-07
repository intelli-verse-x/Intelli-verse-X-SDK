# Optional Unity modules

Core: `com.intelliversex.sdk` at `Packages/com.intelliversex.sdk` (**5.11.0**).

Kid path: **Control Center → Game ID → Play** (core only + Nakama client).

## Install optional packages

```json
"com.intelliversex.sdk.ai": "https://github.com/Intelli-verse-X/Intelli-verse-X-SDK.git?path=Packages/com.intelliversex.sdk.ai",
"com.intelliversex.sdk.discord": "https://github.com/Intelli-verse-X/Intelli-verse-X-SDK.git?path=Packages/com.intelliversex.sdk.discord",
"com.intelliversex.sdk.photon": "https://github.com/Intelli-verse-X/Intelli-verse-X-SDK.git?path=Packages/com.intelliversex.sdk.photon"
```

| Package | Contains | Notes |
|---------|----------|--------|
| `.ai` | LLM / voice / moderation Runtime | Bootstrap soft-loads when present |
| `.discord` | Discord Social Runtime | Bootstrap soft-loads when present |
| `.photon` | Room helpers only | Install PUN2 from Asset Store separately |

Sandbox already references all three via `file:` paths.

See [ADR-003](architecture/adr/ADR-003-package-source-of-truth.md).
