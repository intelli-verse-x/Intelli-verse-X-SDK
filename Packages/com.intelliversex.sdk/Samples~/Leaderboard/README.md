# Leaderboard Sample

This sample demonstrates how to integrate the global leaderboard system using Nakama backend.

## Features Demonstrated

- Score submission to global leaderboards
- Fetching top scores
- Getting scores around the current player
- Leaderboard UI display

## Setup

1. Import this sample via Package Manager
2. Open the `LeaderboardDemoScene` scene
3. Configure your Nakama backend:
   - Host: Your Nakama server URL
   - Port: Your Nakama server port (usually 443 for HTTPS)
   - Server Key: Your Nakama server key
4. Press Play

## Key Scripts

### LeaderboardDemoController.cs
Demonstrates the complete leaderboard flow:

```csharp
using IntelliVerseX.Leaderboard;
using IntelliVerseX.Identity;
using IntelliVerseX.Backend;

public class LeaderboardDemoController : MonoBehaviour
{
    async void Start()
    {
        // Initialize identity
        IntelliVerseXUserIdentity.InitializeDevice();
        
        // Submit a score
        bool success = await IVXLeaderboardManager.SubmitScoreAsync(1000);
        
        // Get top 20 scores
        var topScores = await IVXLeaderboardManager.GetTopScoresAsync(20);
        
        // Get scores around current player
        var nearbyScores = await IVXLeaderboardManager.GetAroundPlayerAsync(10);
    }
}
```

## Dependencies

- IntelliVerseX.Core
- IntelliVerseX.Identity
- IntelliVerseX.Backend
- IntelliVerseX.Leaderboard
- Nakama Unity SDK (external)

## Backend Setup

This sample requires a Nakama server with a leaderboard configured:

1. Create a leaderboard named `global_leaderboard` in Nakama
2. Configure it for "best" score ordering (higher is better)
3. Set the reset schedule if desired (daily, weekly, never)

## UI Components

The sample includes:
- `LeaderboardPanel` - Main leaderboard display
- `LeaderboardEntry` - Individual score entry row
- `SubmitScorePanel` - Score submission UI
