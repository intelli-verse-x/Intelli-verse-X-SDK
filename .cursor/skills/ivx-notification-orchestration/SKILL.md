---
name: ivx-notification-orchestration
description: >-
  Set up push notification scheduling and orchestration for IntelliVerseX SDK games.
  Use when the user says "push notifications", "notification scheduling",
  "Firebase push", "APNs", "web push", "notification templates", "deep linking",
  "smart send time", "notification A/B test", "frequency cap", "quiet hours",
  "re-engagement notifications", or needs help with any push notification feature.
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

# IntelliVerseX Notification Orchestration

## Overview

The Notification Orchestration module provides push notification scheduling, smart send-time optimization, templated messages with deep linking, A/B testing of notification content, and suppression rules. Built on Firebase Cloud Messaging (FCM), APNs, and Web Push, with Satori audiences for targeting and Nakama for scheduling.

```
Notification Campaign
      │
      ├── Audience (Satori) ──► Who receives it
      ├── Template ──► What they see
      ├── Schedule ──► When it sends
      ├── Deep Link ──► Where it goes
      └── Suppression ──► Rules to skip
      │
      ▼
IVXNotificationScheduler → FCM / APNs / Web Push → Player Device
```

---

## 1. Configuration

### IVXNotificationConfig ScriptableObject

Create via **Create > IntelliVerseX > Notification Configuration**.

| Field | Description |
|-------|-------------|
| `EnableNotifications` | Master toggle |
| `FCMServerKey` | Firebase Cloud Messaging server key |
| `APNsKeyId` | Apple Push Notification key ID |
| `APNsTeamId` | Apple Developer team ID |
| `APNsKeyPath` | Path to `.p8` key file |
| `WebPushVapidPublicKey` | VAPID public key for web push |
| `WebPushVapidPrivateKey` | VAPID private key for web push |
| `DefaultIcon` | Notification icon resource name |
| `DefaultSound` | Notification sound file |

---

## 2. Registering for Push

### Client Registration

```csharp
using IntelliVerseX.Notifications;

await IVXNotificationManager.Instance.RegisterAsync();

IVXNotificationManager.Instance.OnTokenRefreshed += async (newToken) =>
{
    await IVXNotificationManager.Instance.UpdateTokenAsync(newToken);
};
```

The SDK automatically stores the device token in Nakama for server-side push.

### Permission Request

```csharp
var permission = await IVXNotificationManager.Instance.RequestPermissionAsync();

if (permission == NotificationPermission.Granted)
{
    Debug.Log("Push notifications enabled");
}
else
{
    Debug.Log("User declined push notifications");
}
```

---

## 3. Notification Templates

### Defining Templates

```csharp
IVXNotificationTemplates.Instance.Register(new NotificationTemplate
{
    Id = "daily_reminder",
    Title = "Your daily reward is waiting!",
    Body = "Come back and claim {{reward_amount}} coins!",
    Icon = "icon_coins",
    Sound = "notification_chime",
    DeepLink = "ivxgame://daily-rewards",
    Category = NotificationCategory.Engagement,
});

IVXNotificationTemplates.Instance.Register(new NotificationTemplate
{
    Id = "friend_challenge",
    Title = "{{friend_name}} challenged you!",
    Body = "Can you beat their score of {{score}}?",
    DeepLink = "ivxgame://challenge/{{challenge_id}}",
    Category = NotificationCategory.Social,
});
```

### Template Variables

Templates support `{{variable}}` placeholders filled at send time:

| Variable | Source | Example |
|----------|--------|---------|
| `{{player_name}}` | Player profile | "Alex" |
| `{{friend_name}}` | Social graph | "Jordan" |
| `{{reward_amount}}` | Campaign config | "500" |
| `{{score}}` | Leaderboard data | "12,500" |
| `{{challenge_id}}` | Match/challenge ID | "abc123" |
| `{{time_left}}` | Event timer | "2 hours" |

---

## 4. Scheduling

### IVXNotificationScheduler

```csharp
await IVXNotificationScheduler.Instance.ScheduleAsync(new ScheduledNotification
{
    TemplateId = "daily_reminder",
    Audience = "lapsed_24h",
    ScheduleAt = DateTime.UtcNow.AddHours(24),
    Variables = new Dictionary<string, string>
    {
        { "reward_amount", "500" },
    },
});
```

### Schedule Types

| Type | Description | Use Case |
|------|------------|----------|
| `Immediate` | Send now | Transactional (friend request, match found) |
| `Scheduled` | Send at specific UTC time | Event announcements |
| `SmartSend` | Send at player's optimal time | Re-engagement |
| `Recurring` | Send on a schedule (daily/weekly) | Daily reminders |
| `Triggered` | Send when player matches condition | Lapse prevention |

### Smart Send-Time Optimization

```csharp
await IVXNotificationScheduler.Instance.ScheduleAsync(new ScheduledNotification
{
    TemplateId = "daily_reminder",
    Audience = "all_active",
    ScheduleType = ScheduleType.SmartSend,
    SmartSendWindow = new TimeWindow
    {
        EarliestHour = 9,
        LatestHour = 21,
    },
});
```

Smart send analyzes each player's historical session start times and sends at the hour with highest open probability, within the allowed window.

---

## 5. Deep Linking

### IVXDeepLinkRouter

Route players to specific screens when they tap a notification:

```csharp
IVXDeepLinkRouter.Instance.RegisterRoute("daily-rewards", () =>
{
    SceneManager.LoadScene("DailyRewards");
});

IVXDeepLinkRouter.Instance.RegisterRoute("challenge/{id}", (parameters) =>
{
    string challengeId = parameters["id"];
    ChallengeManager.Instance.OpenChallenge(challengeId);
});

IVXDeepLinkRouter.Instance.RegisterRoute("store/{category}", (parameters) =>
{
    StoreManager.Instance.OpenCategory(parameters["category"]);
});
```

### URL Scheme

```
ivxgame://daily-rewards
ivxgame://challenge/abc123
ivxgame://store/gems
ivxgame://live-event/spring_festival
```

---

## 6. A/B Testing Notifications

### Experiment Integration

```csharp
await IVXNotificationScheduler.Instance.ScheduleABTestAsync(new NotificationABTest
{
    ExperimentId = "daily_reminder_test",
    Audience = "lapsed_24h",
    Variants = new[]
    {
        new NotificationVariant
        {
            Name = "control",
            Weight = 50,
            TemplateId = "daily_reminder",
            Variables = new Dictionary<string, string> { { "reward_amount", "500" } },
        },
        new NotificationVariant
        {
            Name = "higher_reward",
            Weight = 50,
            TemplateId = "daily_reminder",
            Variables = new Dictionary<string, string> { { "reward_amount", "1000" } },
        },
    },
    SuccessMetric = "engagement.session_start_within_24h",
});
```

### Metrics Tracked

| Metric | Description |
|--------|------------|
| **Delivery rate** | % of notifications successfully delivered |
| **Open rate** | % of delivered notifications that were opened |
| **Conversion rate** | % of opens that led to the target action |
| **Opt-out rate** | % of users who disabled notifications after receiving |

---

## 7. Suppression Rules

### Frequency Caps

```csharp
IVXNotificationSuppressionRules.Instance.SetFrequencyCap(new FrequencyCap
{
    MaxPerDay = 3,
    MaxPerWeek = 10,
    MinIntervalMinutes = 120,
});
```

### Quiet Hours

```csharp
IVXNotificationSuppressionRules.Instance.SetQuietHours(new QuietHours
{
    Start = new TimeSpan(22, 0, 0),
    End = new TimeSpan(8, 0, 0),
    UsePlayerTimezone = true,
});
```

### Suppression Conditions

| Rule | Behavior |
|------|---------|
| **Frequency cap** | Skip if player received max notifications in window |
| **Quiet hours** | Delay until quiet hours end |
| **Active session** | Don't send if player is currently in-game |
| **Recently opened** | Skip if player opened app in last N hours |
| **Unsubscribed category** | Skip if player opted out of this notification category |
| **Platform limit** | Respect OS-level notification limits |

---

## 8. Local Notifications

For time-based reminders that don't require server involvement:

```csharp
IVXNotificationManager.Instance.ScheduleLocal(new LocalNotification
{
    Title = "Your energy is full!",
    Body = "Come back and play!",
    FireAt = DateTime.Now.AddMinutes(energyRefillMinutes),
    DeepLink = "ivxgame://main-menu",
    RepeatInterval = TimeSpan.Zero,
});
```

---

## 9. Cross-Platform API

| Engine | Class / Module | Push API |
|--------|---------------|----------|
| **Unity** | `IVXNotificationManager` | Full feature set |
| **Unreal** | `UIVXNotificationSubsystem` | FCM/APNs, templates, deep links |
| **Godot** | `IVXNotifications` autoload | FCM/APNs, local notifications |
| **JavaScript** | `IVXNotifications` | Web Push, local notifications |
| **Roblox** | N/A | Roblox handles notifications natively |
| **Java** | `IVXNotificationManager` | FCM, templates, deep links |
| **Flutter** | `IvxNotifications` | FCM/APNs, local notifications |
| **C++** | `IVXNotifications` | FCM/APNs (via platform bridge) |

---

## Platform-Specific Notifications

### VR Platforms

| Platform | Notification Support |
|----------|---------------------|
| **Meta Quest** | Limited. Notifications appear in Quest system UI. Use in-game notification panel for active sessions. |
| **SteamVR** | No push notifications. Use Steam Rich Presence for visibility. |
| **Vision Pro** | Standard iOS notifications via APNs. Appear in Notification Center. |

### Console Platforms

| Platform | Notification Support |
|----------|---------------------|
| **PlayStation** | System notifications via Sony SDK. Limited customization. |
| **Xbox** | Toast notifications via Xbox Live SDK. Deep linking to game sections. |
| **Switch** | News channel for announcements. No real-time push to device. |

### WebGL / Browser

| Feature | WebGL Notes |
|---------|------------|
| **Web Push** | Requires service worker registration and VAPID keys. User must grant permission. |
| **Fallback** | If push not supported, use email notifications via Satori messages. |
| **PWA** | Full push support when installed as Progressive Web App. |

---

## Checklist

- [ ] `EnableNotifications` toggled on in `IVXBootstrapConfig`
- [ ] FCM server key configured for Android
- [ ] APNs key configured for iOS
- [ ] Web Push VAPID keys configured (if targeting browser)
- [ ] Push permission request implemented with graceful decline handling
- [ ] Notification templates defined for key engagement scenarios
- [ ] Deep link routes registered for all notification destinations
- [ ] Smart send-time optimization configured for re-engagement campaigns
- [ ] Frequency caps set (max 3/day, min 2h apart recommended)
- [ ] Quiet hours enabled with player timezone awareness
- [ ] A/B test configured for at least one notification campaign
- [ ] Local notifications set for time-based events (energy refill, etc.)
- [ ] VR in-game notification panel implemented (if targeting VR)
- [ ] Console notification APIs integrated (if targeting consoles)
