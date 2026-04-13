# Analytics Pipeline

**Skill ID:** `ivx-analytics-pipeline`

---
name: ivx-analytics-pipeline
description: >-
  Set up analytics, event tracking, funnels, cohort analysis, and data export for
  IntelliVerseX SDK games. Use when the user says "add analytics", "track events",
  "set up funnels", "cohort analysis", "retention metrics", "LTV tracking",
  "export to BigQuery", "Snowflake export", "event taxonomy", "analytics dashboard",
  "D1 D7 D30 retention", "player segmentation", "data pipeline", or needs help
  with any analytics or telemetry integration.
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

The Analytics Pipeline provides a unified event tracking, funnel analysis, and data export system across all 10 supported engines. Built on top of the Satori analytics backend, it adds structured taxonomy enforcement, pre-built funnel definitions, cohort analysis helpers, and one-click export to warehouse targets (BigQuery, Snowflake, Redshift, S3).

```
Game Client → IVXAnalyticsManager → Satori Events API → Data Lake Export
                    │                       │
              Event Taxonomy          Audiences / Flags
              (validation)            (segmentation)
```

---

## 1. Configuration

### IVXAnalyticsConfig ScriptableObject

Create via **Create > IntelliVerseX > Analytics Configuration**.

| Field | Description |
|-------|-------------|
| `EnableAnalytics` | Master toggle for event tracking |
| `TaxonomyMode` | `Permissive` (log unknown events) or `Strict` (reject unknown events) |
| `BatchSize` | Number of events to batch before flushing (default 10) |
| `FlushIntervalSec` | Max seconds between flushes (default 30) |
| `EnableOfflineQueue` | Queue events when offline, flush on reconnect |
| `OfflineQueueMaxSize` | Max queued events before oldest are dropped (default 500) |
| `EnableDebugLogging` | Log all events to Unity console |
| `SamplingRate` | 0.0–1.0, percentage of sessions that track events (default 1.0) |

---

## 2. Event Taxonomy

### Taxonomy Categories

Every event belongs to one of six categories following the `category.action.detail` naming convention:

| Category | Examples | Purpose |
|----------|----------|---------|
| `engagement` | `engagement.session_start`, `engagement.session_end` | Session and attention metrics |
| `progression` | `progression.level_complete`, `progression.tutorial_step` | Player advancement tracking |
| `monetization` | `monetization.purchase`, `monetization.ad_viewed` | Revenue events |
| `social` | `social.friend_added`, `social.chat_message` | Social feature usage |
| `system` | `system.error`, `system.performance` | Technical health metrics |
| `custom` | `custom.*` | Game-specific events |

### Defining Event Schemas

```csharp
using IntelliVerseX.Analytics;

IVXEventTaxonomy.Instance.DefineEvent(new EventSchema
{
    Name = "progression.level_complete",
    Category = EventCategory.Progression,
    RequiredMetadata = new[] { "level_id", "score" },
    OptionalMetadata = new[] { "time_sec", "hints_used", "stars" },
    MetadataTypes = new Dictionary<string, MetadataType>
    {
        { "level_id", MetadataType.String },
        { "score", MetadataType.Number },
        { "time_sec", MetadataType.Number },
        { "hints_used", MetadataType.Number },
        { "stars", MetadataType.Number },
    },
});
```

### Tracking Events

```csharp
IVXAnalyticsManager.Instance.Track("progression.level_complete", new Dictionary<string, string>
{
    { "level_id", "world_1_boss" },
    { "score", "12500" },
    { "time_sec", "142" },
    { "stars", "3" },
});
```

In `Strict` taxonomy mode, events without a registered schema are rejected and logged as warnings.

### Validating Events (Development)

```csharp
var result = IVXEventTaxonomy.Instance.Validate("progression.level_complete", metadata);
if (!result.Valid)
{
    Debug.LogWarning($"Event validation failed: {string.Join(", ", result.Errors)}");
}
```

---

## 3. Pre-Built Funnels

### IVXFunnelBuilder

Define multi-step funnels and query conversion rates:

```csharp
using IntelliVerseX.Analytics;

var onboardingFunnel = IVXFunnelBuilder.Create("onboarding")
    .Step("engagement.session_start")
    .Step("progression.tutorial_step", metadata => metadata["step"] == "1")
    .Step("progression.tutorial_step", metadata => metadata["step"] == "3")
    .Step("progression.tutorial_complete")
    .Step("monetization.first_purchase")
    .Build();

IVXAnalyticsManager.Instance.RegisterFunnel(onboardingFunnel);
```

### Pre-Defined Funnel Templates

| Funnel | Steps | Purpose |
|--------|-------|---------|
| `ftue_funnel` | Session start -> Tutorial start -> Tutorial complete -> First action | First-time user experience |
| `monetization_funnel` | Session start -> Store viewed -> Item selected -> Purchase confirmed | Purchase conversion |
| `retention_funnel` | D0 session -> D1 session -> D7 session -> D30 session | Retention curve |
| `social_funnel` | Session start -> Friends list viewed -> Friend added -> Match played | Social adoption |

---

## 4. Cohort Analysis

### IVXCohortTracker

Track retention cohorts by install date, acquisition source, or custom dimensions:

```csharp
var tracker = IVXCohortTracker.Instance;

tracker.DefineCohort(new CohortDefinition
{
    Name = "jan_2026_installs",
    Dimension = CohortDimension.InstallDate,
    StartDate = new DateTime(2026, 1, 1),
    EndDate = new DateTime(2026, 1, 31),
});

tracker.DefineCohort(new CohortDefinition
{
    Name = "organic_users",
    Dimension = CohortDimension.Custom,
    Filter = (playerProfile) => playerProfile.AcquisitionSource == "organic",
});
```

### Retention Metrics

```csharp
var retention = await IVXCohortTracker.Instance.GetRetentionAsync("jan_2026_installs");

Debug.Log($"D1: {retention.D1:P1}");   // e.g. "D1: 45.2%"
Debug.Log($"D7: {retention.D7:P1}");   // e.g. "D7: 22.8%"
Debug.Log($"D30: {retention.D30:P1}"); // e.g. "D30: 8.1%"
```

### LTV Projections

```csharp
var ltv = await IVXCohortTracker.Instance.GetLTVAsync("jan_2026_installs", days: 180);

Debug.Log($"Predicted 180-day LTV: ${ltv.Predicted:F2}");
Debug.Log($"Current ARPU: ${ltv.CurrentARPU:F2}");
Debug.Log($"Paying user %: {ltv.PayerRate:P1}");
```

---

## 5. Data Lake Export

### Export Targets

Configure export to external data warehouses:

```csharp
using IntelliVerseX.Analytics;

IVXDataLakeManager.Instance.AddTarget(new DataLakeTarget
{
    Id = "main_bigquery",
    Type = DataLakeType.BigQuery,
    Config = new BigQueryConfig
    {
        ProjectId = "my-gcp-project",
        DatasetId = "game_analytics",
        CredentialsJson = SecureConfigService.GetKey("BQ_CREDENTIALS"),
    },
    EventFilters = new[] { "monetization.*", "progression.*" },
    Enabled = true,
});
```

### Supported Targets

| Target | Config Fields | Use Case |
|--------|--------------|----------|
| **BigQuery** | ProjectId, DatasetId, CredentialsJson | Google Cloud analytics |
| **Snowflake** | Account, Warehouse, Database, Schema, User, Password | Enterprise analytics |
| **Redshift** | ClusterEndpoint, Database, User, Password | AWS analytics |
| **S3** | Bucket, Region, AccessKey, SecretKey, Prefix | Raw event archival |

### Manual Export

```csharp
var result = await IVXDataLakeManager.Instance.ExportAsync(limit: 1000);
Debug.Log($"Exported {result.EventCount} events to {result.TargetCount} targets");
```

---

## 6. Dashboard Templates

Pre-built Satori metric definitions for common KPIs:

| Dashboard | Metrics Included |
|-----------|-----------------|
| **Executive** | DAU, MAU, D1/D7/D30 retention, revenue, ARPDAU |
| **Engagement** | Session length, sessions/day, events/session, feature adoption |
| **Monetization** | ARPU, ARPPU, conversion rate, revenue by source (ads/IAP/offerwall) |
| **Progression** | Level completion rates, average progress, churn points |
| **Technical** | Crash-free rate, load times, API latency, error rates |

### Setting Up Dashboards

```csharp
await IVXAnalyticsDashboard.Instance.ProvisionAsync(DashboardTemplate.Executive);
await IVXAnalyticsDashboard.Instance.ProvisionAsync(DashboardTemplate.Monetization);
```

This creates the corresponding metric definitions and alert thresholds in Satori.

---

## 7. Alerts

### Metric Alerts

Set up threshold-based alerts for critical metrics:

```csharp
IVXAnalyticsAlerts.Instance.SetAlert(new MetricAlert
{
    Name = "crash_rate_spike",
    MetricId = "system.crash_free_rate",
    Operator = AlertOperator.LessThan,
    Threshold = 0.95,
    WebhookUrl = "https://hooks.slack.com/services/...",
});

IVXAnalyticsAlerts.Instance.SetAlert(new MetricAlert
{
    Name = "revenue_drop",
    MetricId = "monetization.daily_revenue",
    Operator = AlertOperator.LessThan,
    Threshold = 500.0,
    WebhookUrl = "https://hooks.slack.com/services/...",
});
```

---

## 8. Cross-Platform Event SDK

The same taxonomy and tracking API is available across all engines:

| Engine | Class / Module | Event API |
|--------|---------------|-----------|
| **Unity** | `IVXAnalyticsManager` | `Track(name, metadata)` |
| **Unreal** | `UIVXAnalyticsSubsystem` | `Track(FString, TMap)` |
| **Godot** | `IVXAnalytics` autoload | `track(name, metadata)` |
| **JavaScript** | `IVXAnalytics` | `track(name, metadata)` |
| **Roblox** | `IVX.Analytics` | `IVX.Analytics.track(name, metadata)` |
| **Java** | `IVXAnalytics` | `track(String, Map)` |
| **Flutter** | `IvxAnalytics` | `track(String, Map)` |
| **C++** | `IVXAnalytics` | `Track(std::string, std::map)` |

All platform SDKs route events to the same Satori backend, so cross-platform funnels and cohorts work automatically.

---

## Platform-Specific Analytics

### VR Platforms

| Metric | VR Adaptation |
|--------|--------------|
| **Session length** | VR sessions are typically shorter (15-45 min). Adjust session-length buckets in dashboards. |
| **Gaze tracking** | Track `engagement.gaze_target` events for heatmap analysis of where players look. |
| **Comfort metrics** | Track `system.comfort_break` and `system.motion_sickness_quit` for VR comfort analysis. |
| **Room-scale** | Track play area size from `IVXXRPlatformHelper` for content accessibility analysis. |

### Console Platforms

| Platform | Analytics Requirements |
|----------|----------------------|
| **PlayStation** | Sony requires specific telemetry disclosure. Include analytics in your privacy policy. |
| **Xbox** | Xbox Game Analytics is separate. Mirror key events to Xbox analytics for Game Pass metrics. |
| **Switch** | Nintendo NEX analytics integration. Track online/offline session ratios. |

### WebGL / Browser

| Feature | WebGL Notes |
|---------|------------|
| **Event persistence** | Use `IndexedDB` for offline event queue. Browser storage may be cleared. |
| **Session tracking** | Use `visibilitychange` and `beforeunload` events for reliable session end detection. |
| **Ad blockers** | Some ad blockers block analytics endpoints. Implement fallback via first-party proxy. |
| **GDPR** | Browser games require cookie consent before tracking. `IVXAnalyticsManager` respects `SetConsent(false)`. |

---

## Checklist

- [ ] `EnableAnalytics` toggled on in `IVXBootstrapConfig`
- [ ] `IVXAnalyticsConfig` created with appropriate taxonomy mode
- [ ] Core event schemas defined for your game's key actions
- [ ] FTUE funnel defined and tracking verified
- [ ] Monetization funnel defined and tracking verified
- [ ] Cohort definitions created for current install window
- [ ] Data lake export target configured (if using external warehouse)
- [ ] Executive dashboard provisioned with metric definitions
- [ ] Critical metric alerts configured (crash rate, revenue)
- [ ] Cross-platform event names verified to match across engines
- [ ] GDPR/privacy consent flow integrated before tracking begins
- [ ] VR comfort metrics added (if targeting VR)
- [ ] Console telemetry disclosure included in privacy policy (if targeting consoles)
- [ ] WebGL offline queue and ad-blocker fallback configured (if targeting browser)
