---
name: ivx-crashlytics
description: >-
  Set up crash reporting, error tracking, and diagnostics for IntelliVerseX SDK games.
  Use when the user says "crash reporting", "error tracking", "add crashlytics",
  "Sentry integration", "ANR detection", "crash-free rate", "breadcrumbs",
  "exception logging", "crash alerts", "diagnostics", "stack traces",
  "crash dashboard", or needs help with any crash reporting or error monitoring.
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

# IntelliVerseX Crashlytics

## Overview

The Crashlytics module provides automatic crash and exception capture, breadcrumb trails, crash grouping, and alert webhooks. It supports self-hosted reporting via Nakama storage, or integration with external services (Sentry, Firebase Crashlytics, Backtrace). All platforms share the same API surface.

```
Exception/Crash
      │
      ▼
IVXCrashReporter
      │
      ├── Stack Trace Capture
      ├── Breadcrumb Trail (last N actions)
      ├── Device + Game State Metadata
      └── Crash Grouping (fingerprint)
      │
      ├──► Nakama Storage (self-hosted)
      ├──► Sentry (external)
      ├──► Firebase Crashlytics (external)
      └──► Webhook Alerts (Slack, Discord)
```

---

## 1. Configuration

### IVXCrashlyticsConfig ScriptableObject

Create via **Create > IntelliVerseX > Crashlytics Configuration**.

| Field | Description |
|-------|-------------|
| `EnableCrashReporting` | Master toggle |
| `Backend` | `Nakama`, `Sentry`, `FirebaseCrashlytics`, `Backtrace` |
| `SentryDSN` | Sentry project DSN (if using Sentry) |
| `BreadcrumbCapacity` | Max breadcrumbs to retain (default 50) |
| `CaptureLogErrors` | Auto-capture `Debug.LogError` as non-fatal events |
| `CaptureLogExceptions` | Auto-capture `Debug.LogException` (default true) |
| `CaptureNativeCrashes` | Capture native C/C++ crashes on Android/iOS |
| `ANRTimeoutSec` | Seconds of UI thread stall to flag as ANR on Android (default 5) |
| `AttachScreenshot` | Capture screenshot on crash (increases report size) |
| `MaxReportsPerSession` | Rate limit: max crash reports per session (default 10) |

---

## 2. Automatic Crash Capture

### Unity Setup

The crash reporter hooks into Unity's log callback automatically:

```csharp
using IntelliVerseX.Diagnostics;

IVXCrashReporter.Instance.Initialize();
```

After initialization, all unhandled exceptions and `LogError`/`LogException` calls are captured automatically.

### Manual Exception Reporting

```csharp
try
{
    RiskyOperation();
}
catch (Exception ex)
{
    IVXCrashReporter.Instance.ReportException(ex, new Dictionary<string, string>
    {
        { "operation", "risky_op" },
        { "player_level", "12" },
    });
}
```

### Non-Fatal Error Reporting

```csharp
IVXCrashReporter.Instance.ReportError(
    "network_timeout",
    "Failed to fetch leaderboard after 3 retries",
    severity: ErrorSeverity.Warning,
    metadata: new Dictionary<string, string>
    {
        { "endpoint", "/v2/leaderboard" },
        { "retry_count", "3" },
    }
);
```

---

## 3. Breadcrumbs

Breadcrumbs record the player's recent actions leading up to a crash:

### Automatic Breadcrumbs

The SDK automatically logs these breadcrumb types:

| Type | Example | Auto-Captured |
|------|---------|--------------|
| **Navigation** | Scene loaded, menu opened | Yes |
| **UI** | Button clicked, dialog shown | Yes (if using IVX UI) |
| **Network** | RPC call, socket connect/disconnect | Yes |
| **State** | Login, purchase, level start | Yes |
| **System** | Memory warning, focus lost | Yes |

### Custom Breadcrumbs

```csharp
IVXCrashReporter.Instance.AddBreadcrumb(
    category: "gameplay",
    message: "Boss fight started",
    metadata: new Dictionary<string, string>
    {
        { "boss_id", "dragon_king" },
        { "player_hp", "850" },
    }
);
```

### Breadcrumb in Crash Report

When a crash occurs, the most recent breadcrumbs are attached:

```
[12:05:01] navigation: Loaded scene "BossFight"
[12:05:03] gameplay: Boss fight started (boss_id=dragon_king)
[12:05:15] gameplay: Player used ability (ability=fireball)
[12:05:16] network: RPC call hiro_energy_spend
[12:05:17] system: Memory warning (available=128MB)
[12:05:18] CRASH: NullReferenceException at BossAI.UpdatePhase()
```

---

## 4. Crash Grouping

### Fingerprinting

Crashes are grouped by a fingerprint derived from:

1. Exception type
2. Top 3 stack frames (method + file + line)
3. Platform and architecture

```csharp
IVXCrashGrouper.Instance.GroupingStrategy = CrashGroupingStrategy.StackTrace;
```

### Custom Fingerprints

```csharp
IVXCrashReporter.Instance.ReportException(ex, fingerprint: "boss_ai_null_phase");
```

---

## 5. Device and Game State Metadata

Every crash report automatically includes:

| Category | Fields |
|----------|--------|
| **Device** | Model, OS, OS version, GPU, RAM, disk free |
| **App** | Version, build number, SDK version, engine version |
| **Session** | Session ID, session duration, scene name |
| **Player** | User ID, display name, account age |
| **Game State** | Current level, score, inventory count |
| **Performance** | FPS at crash time, memory usage, draw calls |

### Custom Metadata

```csharp
IVXCrashReporter.Instance.SetCustomMetadata("game_mode", "ranked");
IVXCrashReporter.Instance.SetCustomMetadata("match_id", matchId);
```

---

## 6. Backend Options

### Nakama (Self-Hosted)

Crash reports are stored in Nakama storage under `ivx_crash_reports`:

```csharp
IVXCrashlyticsConfig.Instance.Backend = CrashBackend.Nakama;
```

Query reports via MCP:

```
storage_list collection=ivx_crash_reports
storage_read collection=ivx_crash_reports key=<crash_id>
```

### Sentry

```csharp
IVXCrashlyticsConfig.Instance.Backend = CrashBackend.Sentry;
IVXCrashlyticsConfig.Instance.SentryDSN = "https://key@sentry.io/project";
```

### Firebase Crashlytics

```csharp
IVXCrashlyticsConfig.Instance.Backend = CrashBackend.FirebaseCrashlytics;
```

Requires Firebase SDK imported separately.

---

## 7. ANR Detection (Android)

Application Not Responding detection monitors the main thread:

```csharp
IVXCrashlyticsConfig.Instance.ANRTimeoutSec = 5;
```

When the main thread is blocked for longer than the timeout, an ANR report is filed with the current stack trace of the main thread.

---

## 8. Alert Webhooks

### Configuring Alerts

```csharp
IVXCrashAlerts.Instance.AddWebhook(new CrashWebhook
{
    Url = "https://hooks.slack.com/services/T.../B.../xxx",
    Events = new[] { CrashAlertEvent.NewCrashGroup, CrashAlertEvent.RegressionDetected },
    MinSeverity = ErrorSeverity.Error,
});

IVXCrashAlerts.Instance.AddWebhook(new CrashWebhook
{
    Url = "https://discord.com/api/webhooks/.../...",
    Events = new[] { CrashAlertEvent.CrashRateSpike },
    ThresholdPercent = 2.0f,
});
```

### Alert Events

| Event | Trigger |
|-------|---------|
| `NewCrashGroup` | First occurrence of a new crash fingerprint |
| `RegressionDetected` | A previously resolved crash reappears |
| `CrashRateSpike` | Crash rate exceeds threshold (default 2%) |
| `ANRDetected` | ANR event on Android |
| `CriticalError` | Error with `Severity.Critical` |

---

## 9. Crash-Free Rate Tracking

```csharp
var metrics = await IVXCrashReporter.Instance.GetMetricsAsync();

Debug.Log($"Crash-free sessions: {metrics.CrashFreeRate:P2}");
Debug.Log($"Total crashes today: {metrics.CrashesToday}");
Debug.Log($"Top crash: {metrics.TopCrashGroup.Fingerprint} ({metrics.TopCrashGroup.Count} occurrences)");
```

---

## 10. Cross-Platform API

| Engine | Class / Module | Report API |
|--------|---------------|------------|
| **Unity** | `IVXCrashReporter` | `ReportException()`, `ReportError()` |
| **Unreal** | `UIVXCrashSubsystem` | `ReportException()`, `ReportError()` |
| **Godot** | `IVXCrashReporter` autoload | `report_exception()`, `report_error()` |
| **JavaScript** | `IVXCrashReporter` | `reportException()`, `reportError()` |
| **Roblox** | `IVX.Diagnostics` | `IVX.Diagnostics.report_error(msg, metadata)` |
| **Java** | `IVXCrashReporter` | `reportException()`, `reportError()` |
| **Flutter** | `IvxCrashReporter` | `reportException()`, `reportError()` |
| **C++** | `IVXCrashReporter` | `ReportException()`, `ReportError()` |

---

## Platform-Specific Crashlytics

### VR Platforms

| Platform | Crash Reporting Notes |
|----------|---------------------|
| **Meta Quest** | Native crash capture via `libunwind`. Include Quest-specific metadata: guardian boundary status, controller battery, tracking state. |
| **SteamVR** | Hook into SteamVR's crash handler. Include HMD model and firmware version in metadata. |
| **Vision Pro** | Apple's crash reporter is primary. IVX captures non-fatal errors and breadcrumbs for supplemental diagnostics. |

### Console Platforms

| Platform | Crash Reporting Requirements |
|----------|----------------------------|
| **PlayStation** | Sony requires crash dumps via their crash reporter. IVX captures application-level exceptions as supplemental data. |
| **Xbox** | Watson crash reporting is mandatory for certification. IVX supplements with breadcrumbs and game state. |
| **Switch** | Limited crash reporting support. IVX captures to Nakama storage on next successful session. |

### WebGL / Browser

| Feature | WebGL Notes |
|---------|------------|
| **`window.onerror`** | IVX hooks into `window.onerror` and `unhandledrejection` for browser-level error capture. |
| **Source maps** | Upload source maps to Sentry for readable stack traces from minified WebGL builds. |
| **Offline reports** | Failed uploads are queued in `IndexedDB` and retried on next page load. |

---

## Checklist

- [ ] `EnableCrashReporting` toggled on in `IVXBootstrapConfig`
- [ ] Backend selected and configured (Nakama, Sentry, or Firebase)
- [ ] Sentry DSN set (if using Sentry)
- [ ] Breadcrumb capacity appropriate for game complexity
- [ ] Custom breadcrumbs added at key gameplay moments
- [ ] ANR timeout configured for Android builds
- [ ] Alert webhooks set up for Slack/Discord
- [ ] Crash-free rate baseline established
- [ ] Custom metadata attached (game mode, match ID, player level)
- [ ] Native crash capture enabled for Android/iOS
- [ ] VR headset metadata included (if targeting VR)
- [ ] Console platform crash requirements reviewed (if targeting consoles)
- [ ] WebGL source maps uploaded (if targeting browser)
