# Security & Anti-Cheat

**Skill ID:** `ivx-security-anticheat`

---
name: ivx-security-anticheat
description: >-
  Add security and anti-cheat features to IntelliVerseX SDK games including
  server-authoritative validation, tamper detection, secure storage, and ban systems.
  Use when the user says "anti-cheat", "prevent cheating", "server validation",
  "secure save data", "ban system", "exploit detection", "memory tampering",
  "speed hack", "replay validation", "rate limiting", "secure storage",
  or needs help with any game security feature.
version: "1.0.0"
author: "IntelliVerse-X <team@intelli-verse-x.ai>"
allowed-tools:
  - Read
  - Write
  - Edit
  - Glob
  - Grep
  - Shell
---



## Overview

The Security module provides server-authoritative validation patterns, client-side tamper detection, secure save data, rate limiting, and a ban/suspension system. Built on Nakama's server-side authority, it ensures that the server is always the source of truth for game state.

```
Client (untrusted)              Server (authoritative)
┌──────────────────┐           ┌──────────────────────┐
│ IVXAntiCheat     │           │ Nakama Server Logic   │
│  ├── Tamper Detect│  ──────► │  ├── State Validation │
│  ├── Speed Check  │  action  │  ├── Rate Limiter     │
│  └── Integrity    │  request │  ├── Replay Validator  │
│                   │  ◄────── │  ├── Anomaly Detector  │
│ IVXSecureSave     │  result  │  └── Ban Manager       │
│  └── Encrypted    │          │                        │
│      Local Store  │          │ IVX Secure Storage     │
└──────────────────┘           └──────────────────────┘
```

---

## 1. Server-Authoritative Patterns

### Core Principle

**Never trust the client.** All game state changes that affect scoring, currency, inventory, or competitive outcomes must be validated server-side.

### Validated Actions

```csharp
using IntelliVerseX.Security;

var result = await IVXSecureAction.Instance.ExecuteAsync(new SecureActionRequest
{
    Action = "complete_level",
    Payload = new Dictionary<string, object>
    {
        { "level_id", "world_1_5" },
        { "score", 12500 },
        { "time_sec", 142 },
        { "enemies_killed", 8 },
    },
    ClientTimestamp = Time.realtimeSinceStartup,
});

if (result.Validated)
{
    ApplyRewards(result.ServerRewards);
}
else
{
    Debug.LogWarning($"Action rejected: {result.RejectionReason}");
}
```

### Server Validation Rules

| Action | Server Checks |
|--------|--------------|
| **Level complete** | Time within possible range, score matches formula, enemy count matches spawn count |
| **Currency earn** | Source is valid (ad, level, daily reward), amount within expected range |
| **Currency spend** | Sufficient balance, item exists, price matches server config |
| **Inventory change** | Item source validated, stack limits respected |
| **Leaderboard submit** | Score within statistical bounds, session valid |
| **Achievement unlock** | Progress accumulation matches event history |

---

## 2. Client-Side Anti-Cheat

### IVXAntiCheat

Detects common cheat tools and tampering:

```csharp
IVXAntiCheat.Instance.Initialize(new AntiCheatConfig
{
    EnableMemoryProtection = true,
    EnableSpeedHackDetection = true,
    EnableIntegrityCheck = true,
    ReportToServer = true,
    TamperAction = TamperAction.ReportAndContinue,
});

IVXAntiCheat.Instance.OnTamperDetected += (report) =>
{
    Debug.LogWarning($"Tamper detected: {report.Type} - {report.Details}");
};
```

### Detection Capabilities

| Detection | Method | Platforms |
|-----------|--------|----------|
| **Speed hack** | Server-client time delta comparison | All |
| **Memory editing** | Checksum validation on critical values | Android, iOS, Standalone |
| **Debugger attachment** | `IsDebuggerPresent` / `ptrace` checks | Android, iOS, Standalone |
| **Root/Jailbreak** | File system and property checks | Android, iOS |
| **App tampering** | APK/IPA signature validation | Android, iOS |
| **Emulator detection** | Hardware fingerprint analysis | Android |
| **Injection detection** | Loaded module scanning | Standalone (Windows) |

### Protected Values

```csharp
var protectedScore = new IVXProtectedInt(0);
protectedScore.Value = 1000;

int rawValue = protectedScore.Value;
```

`IVXProtectedInt`, `IVXProtectedFloat`, and `IVXProtectedString` store values with encryption and detect memory editing attempts.

---

## 3. Secure Save Data

### IVXSecureSaveData

Encrypted local storage with server-side verification:

```csharp
using IntelliVerseX.Security;

await IVXSecureSaveData.Instance.SaveAsync("player_progress", new PlayerProgress
{
    Level = 15,
    Score = 45000,
    Inventory = playerInventory,
});

var progress = await IVXSecureSaveData.Instance.LoadAsync<PlayerProgress>("player_progress");
```

### Security Layers

| Layer | Protection |
|-------|-----------|
| **Encryption** | AES-256-GCM encryption of save data |
| **Integrity** | HMAC-SHA256 signature prevents modification |
| **Device binding** | Save data tied to device ID (prevents copy) |
| **Server sync** | Hash of save data synced to Nakama for verification |
| **Rollback detection** | Save version counter prevents restoring old saves |

### Cloud Save Integration

```csharp
IVXSecureSaveData.Instance.EnableCloudSync = true;
IVXSecureSaveData.Instance.ConflictResolution = SaveConflictResolution.ServerWins;
```

---

## 4. Rate Limiting

### Server-Side Rate Limits

```csharp
var rateLimiter = IVXRateLimiter.Instance;

rateLimiter.Configure(new RateLimitConfig
{
    Endpoint = "hiro_economy_grant",
    MaxRequests = 10,
    WindowSec = 60,
    BurstAllowance = 3,
    ExceedAction = RateLimitAction.Reject,
});

rateLimiter.Configure(new RateLimitConfig
{
    Endpoint = "leaderboard_submit",
    MaxRequests = 5,
    WindowSec = 300,
    ExceedAction = RateLimitAction.Throttle,
});
```

### Client-Side Throttling

```csharp
if (IVXRateLimiter.Instance.CanExecute("leaderboard_submit"))
{
    await SubmitScore();
}
else
{
    ShowCooldownMessage(IVXRateLimiter.Instance.GetCooldownSec("leaderboard_submit"));
}
```

---

## 5. Anomaly Detection

### Statistical Anomaly Checks

```csharp
IVXAnomalyDetector.Instance.Configure(new AnomalyConfig
{
    MetricName = "level_complete_score",
    DetectionMethod = AnomalyMethod.ZScore,
    Threshold = 3.0f,
    MinSamplesRequired = 100,
    OnAnomaly = AnomalyAction.FlagForReview,
});
```

### Anomaly Types

| Anomaly | Detection | Response |
|---------|----------|----------|
| **Impossible score** | Score exceeds theoretical maximum | Auto-reject |
| **Impossible time** | Level completed faster than possible | Auto-reject |
| **Rapid progression** | Level-ups faster than design allows | Flag for review |
| **Currency anomaly** | Balance exceeds earn rate projections | Flag for review |
| **Login pattern** | Bot-like login intervals | Flag for review |
| **Behavioral outlier** | Z-score > 3 on any tracked metric | Flag for review |

---

## 6. Replay Validation

### Competitive Match Replay

For ranked and competitive modes, record and validate match replays:

```csharp
var recorder = IVXReplayRecorder.Instance;

recorder.StartRecording(matchId);

recorder.RecordInput(new ReplayFrame
{
    Tick = currentTick,
    Inputs = capturedInputs,
    GameState = currentStateHash,
});

var replay = recorder.StopRecording();
await replay.SubmitForValidationAsync();
```

### Server-Side Replay Check

The server deterministically replays the inputs and compares the resulting game state hash with the client's claimed state. Mismatches indicate tampering.

---

## 7. Ban and Suspension System

### IVXBanManager

```csharp
await IVXBanManager.Instance.BanAsync(new BanRequest
{
    UserId = cheaterUserId,
    Reason = "Speed hack detected",
    Duration = BanDuration.Days(7),
    Evidence = new[] { "tamper_report_123", "anomaly_456" },
});

await IVXBanManager.Instance.SuspendAsync(new SuspendRequest
{
    UserId = suspectUserId,
    Reason = "Unusual score pattern under review",
    Duration = BanDuration.Hours(24),
});
```

### Ban Tiers

| Tier | Offense | Duration | Actions |
|------|---------|----------|---------|
| **Warning** | First minor offense | 0 (warning only) | In-game warning message |
| **Temporary** | Confirmed exploit use | 24h–7 days | Matchmaking ban, leaderboard removal |
| **Extended** | Repeat offense | 30–90 days | Full account suspension |
| **Permanent** | Egregious cheating | Indefinite | Account disabled |

### Checking Ban Status

```csharp
var status = await IVXBanManager.Instance.CheckStatusAsync(userId);

if (status.IsBanned)
{
    ShowBanScreen(status.Reason, status.ExpiresAt);
    return;
}
```

---

## 8. Exploit Reporting

### Player Reports

```csharp
IVXExploitReporter.Instance.SubmitReport(new ExploitReport
{
    ReporterUserId = currentUserId,
    ReportedUserId = suspectUserId,
    Type = ExploitType.Cheating,
    Description = "Player moving impossibly fast in match",
    MatchId = currentMatchId,
    EvidenceTimestamp = DateTime.UtcNow,
});
```

### Admin Dashboard Integration

Reports are stored in Nakama storage under `ivx_exploit_reports` and can be queried via MCP:

```
storage_list collection=ivx_exploit_reports
```

---

## 9. Cross-Platform API

| Engine | Class / Module | Core API |
|--------|---------------|----------|
| **Unity** | `IVXSecurityManager` | Full feature set |
| **Unreal** | `UIVXSecuritySubsystem` | Server validation, secure save, ban system |
| **Godot** | `IVXSecurity` autoload | Server validation, secure save, ban system |
| **JavaScript** | `IVXSecurity` | Server validation, rate limiting, ban system |
| **Roblox** | `IVX.Security` | Server validation, ban system (Roblox handles anti-cheat) |
| **Java** | `IVXSecurity` | Server validation, secure save, ban system |
| **Flutter** | `IvxSecurity` | Server validation, secure save |
| **C++** | `IVXSecurity` | Full feature set |

---

## Platform-Specific Security

### VR Platforms

| Feature | VR Guidance |
|---------|------------|
| **Tamper detection** | VR-specific: detect teleport exploits (players moving outside guardian boundary). |
| **Input validation** | Validate controller input ranges match physical capabilities. |
| **Replay** | VR replay includes head and hand tracking data for validation. |

### Console Platforms

| Platform | Security Notes |
|----------|---------------|
| **PlayStation / Xbox** | Console platforms provide hardware-level anti-tamper. Focus on server-side validation. |
| **Switch** | Homebrew scene is active. Validate all save data server-side. |

### WebGL / Browser

| Feature | WebGL Notes |
|---------|------------|
| **Client-side** | Browser anti-cheat is weak. All security must be server-side. Client-side checks are deterrents only. |
| **DevTools** | Players can open DevTools. Never trust client-reported values in web builds. |
| **Obfuscation** | Use WebGL code obfuscation, but do not rely on it for security. |

---

## Checklist

- [ ] Server-authoritative validation implemented for all scoring/currency/inventory actions
- [ ] Client-side anti-cheat initialized with appropriate detection level
- [ ] Protected value types used for critical in-memory game state
- [ ] Secure save data encryption enabled with device binding
- [ ] Cloud save sync configured with conflict resolution strategy
- [ ] Rate limits configured for all sensitive RPC endpoints
- [ ] Anomaly detection rules defined for key gameplay metrics
- [ ] Replay recording enabled for competitive game modes
- [ ] Ban system configured with appropriate tier escalation
- [ ] Exploit reporting UI available to players
- [ ] VR teleport exploit detection added (if targeting VR)
- [ ] Console server-side validation prioritized (if targeting consoles)
- [ ] WebGL security fully server-side (if targeting browser)
