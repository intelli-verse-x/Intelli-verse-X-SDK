# More of Us Module

The More of Us module promotes cross-game discovery by showcasing other games from your portfolio within the SDK.

---

## Overview

| | |
|---|---|
| **Namespace** | `IntelliVerseX.MoreOfUs` |
| **Assembly** | `IntelliVerseX.MoreOfUs` |
| **Purpose** | Cross-promotion of portfolio games |

---

## Features

- **Game Showcase** - Display other games in your portfolio
- **Deep Linking** - Direct links to app stores
- **Analytics** - Track cross-game traffic
- **Remote Config** - Update game list without app update

---

## Key Classes

| Class | Purpose |
|-------|---------|
| `IVXMoreOfUsManager` | Main manager for game list |
| `IVXGameInfo` | Game information model |
| `IVXMoreOfUsPanel` | Pre-built UI panel |

---

## IVXMoreOfUsManager

```csharp
public static class IVXMoreOfUsManager
{
    // Data
    public static List<IVXGameInfo> Games { get; }
    public static bool IsLoaded { get; }
    
    // Events
    public static event Action OnGamesLoaded;
    public static event Action<string> OnGameClicked;
    
    // Load games
    public static async Task LoadGamesAsync();
    
    // Open store
    public static void OpenGame(IVXGameInfo game);
    public static void OpenGame(string gameId);
    
    // Filter
    public static List<IVXGameInfo> GetGamesByPlatform(RuntimePlatform platform);
    public static List<IVXGameInfo> GetGamesByCategory(string category);
}
```

---

## IVXGameInfo

```csharp
public class IVXGameInfo
{
    public string Id { get; }
    public string Name { get; }
    public string Description { get; }
    
    public string IconUrl { get; }
    public string BannerUrl { get; }
    public string[] ScreenshotUrls { get; }
    
    public string Category { get; }
    public string[] Tags { get; }
    
    // Store links
    public string AndroidPackageId { get; }
    public string IOSAppId { get; }
    public string WebUrl { get; }
    
    // Metadata
    public float Rating { get; }
    public int Downloads { get; }
    public bool IsNew { get; }
    public bool IsFeatured { get; }
    
    // Platform availability
    public bool IsAvailableOnAndroid { get; }
    public bool IsAvailableOnIOS { get; }
    public bool IsAvailableOnWebGL { get; }
}
```

---

## Usage

### Load and Display Games

```csharp
using IntelliVerseX.MoreOfUs;

public class MoreOfUsUI : MonoBehaviour
{
    [SerializeField] private Transform gameListContainer;
    [SerializeField] private GameCard gameCardPrefab;
    
    async void Start()
    {
        // Load games from backend
        await IVXMoreOfUsManager.LoadGamesAsync();
        
        // Display games
        foreach (var game in IVXMoreOfUsManager.Games)
        {
            // Skip current game
            if (game.AndroidPackageId == Application.identifier)
                continue;
            
            // Only show games for current platform
            #if UNITY_ANDROID
            if (!game.IsAvailableOnAndroid) continue;
            #elif UNITY_IOS
            if (!game.IsAvailableOnIOS) continue;
            #endif
            
            var card = Instantiate(gameCardPrefab, gameListContainer);
            card.SetGame(game);
            card.OnClick += () => IVXMoreOfUsManager.OpenGame(game);
        }
    }
}
```

### Game Card Component

```csharp
public class GameCard : MonoBehaviour
{
    [SerializeField] private RawImage iconImage;
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private TMP_Text descriptionText;
    [SerializeField] private Image ratingStars;
    [SerializeField] private GameObject newBadge;
    [SerializeField] private GameObject featuredBadge;
    [SerializeField] private Button button;
    
    public event Action OnClick;
    
    public async void SetGame(IVXGameInfo game)
    {
        nameText.text = game.Name;
        descriptionText.text = game.Description;
        
        // Load icon
        iconImage.texture = await LoadTextureAsync(game.IconUrl);
        
        // Set rating
        ratingStars.fillAmount = game.Rating / 5f;
        
        // Badges
        newBadge.SetActive(game.IsNew);
        featuredBadge.SetActive(game.IsFeatured);
        
        // Click handler
        button.onClick.AddListener(() => OnClick?.Invoke());
    }
}
```

---

## Pre-built UI

### IVXMoreOfUsPanel

Ready-to-use panel prefab.

```csharp
// Simply add the prefab to your scene
// Configure in inspector:
// - Number of games to show
// - Layout (grid/list)
// - Filter by category
// - Show featured first
```

**Inspector Properties:**

| Property | Description |
|----------|-------------|
| `Max Games` | Maximum games to display |
| `Layout Type` | Grid or List layout |
| `Filter Category` | Filter by game category |
| `Sort Order` | Rating, Downloads, Name, Featured |
| `Show Current Game` | Include current game in list |

---

## Configuration

Games are configured on the Nakama backend:

```json
{
  "games": [
    {
      "id": "puzzle_master",
      "name": "Puzzle Master",
      "description": "Brain-teasing puzzles for all ages",
      "icon_url": "https://cdn.example.com/puzzle_master_icon.png",
      "banner_url": "https://cdn.example.com/puzzle_master_banner.png",
      "category": "puzzle",
      "tags": ["puzzle", "brain", "casual"],
      "android_package": "com.studio.puzzlemaster",
      "ios_app_id": "1234567890",
      "rating": 4.5,
      "is_featured": true
    },
    {
      "id": "space_shooter",
      "name": "Space Shooter X",
      "description": "Epic space battles await!",
      "icon_url": "https://cdn.example.com/space_shooter_icon.png",
      "category": "action",
      "android_package": "com.studio.spaceshooter",
      "ios_app_id": "0987654321",
      "rating": 4.2
    }
  ]
}
```

---

## Store Opening

The SDK handles platform-specific store opening:

```csharp
// Android - Opens Play Store
// iOS - Opens App Store
// WebGL - Opens web URL

IVXMoreOfUsManager.OpenGame(game);

// Or by ID
IVXMoreOfUsManager.OpenGame("puzzle_master");
```

### Platform Behavior

| Platform | Behavior |
|----------|----------|
| Android | `market://details?id=com.example.app` → Play Store |
| iOS | `itms-apps://itunes.apple.com/app/id123` → App Store |
| WebGL | Opens web URL in new tab |
| Editor | Logs URL to console |

---

## Analytics

Cross-promotion analytics are tracked automatically:

```csharp
// Tracked events:
// - more_of_us_opened (panel viewed)
// - more_of_us_game_clicked (game tapped)
// - more_of_us_game_installed (if user returns after install)

// Custom tracking
IVXMoreOfUsManager.OnGameClicked += (gameId) =>
{
    IVXAnalyticsManager.LogEvent("cross_promo_click", new Dictionary<string, object>
    {
        { "game_id", gameId },
        { "source_game", Application.productName }
    });
};
```

---

## Best Practices

### 1. Placement

```csharp
// Good placements for "More Games":
// - Main menu
// - Settings screen
// - Post-level completion
// - Game over screen

// Avoid:
// - Mid-gameplay
// - During critical actions
```

### 2. Filtering

```csharp
// Show relevant games based on context
var puzzleGames = IVXMoreOfUsManager.GetGamesByCategory("puzzle");
var actionGames = IVXMoreOfUsManager.GetGamesByCategory("action");

// Or filter by tags
var casualGames = IVXMoreOfUsManager.Games
    .Where(g => g.Tags.Contains("casual"))
    .ToList();
```

### 3. Featured Games

```csharp
// Highlight featured games
var featured = IVXMoreOfUsManager.Games
    .Where(g => g.IsFeatured)
    .OrderByDescending(g => g.Rating)
    .ToList();
```

### 4. Caching

```csharp
// Games list is cached locally
// Refreshes periodically from backend
// Works offline with cached data

// Force refresh
await IVXMoreOfUsManager.LoadGamesAsync(forceRefresh: true);
```

---

## Localization

Game names and descriptions can be localized:

```csharp
// Backend returns localized content based on device language
// Automatic language detection

// Or specify language
await IVXMoreOfUsManager.LoadGamesAsync(language: "es");
```

---

## Related Documentation

- [More of Us Demo](../samples/more-of-us-demo.md) - Sample implementation
