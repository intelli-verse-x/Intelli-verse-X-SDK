# Analytics Demo

This sample shows how to implement analytics tracking.

## Features

- Event tracking
- User properties
- Session analytics
- Custom events

## Setup

1. Configure analytics in `IVXGameConfig`
2. Initialize analytics on app start
3. Track events throughout your game

## Code Example

```csharp
using IntelliVerseX.Analytics;

// Track custom event
IVXAnalytics.LogEvent("level_complete", new Dictionary<string, object>
{
    { "level", 5 },
    { "score", 1200 },
    { "time_seconds", 120 }
});
```

## See Also

- [Analytics Module](../modules/analytics.md)
