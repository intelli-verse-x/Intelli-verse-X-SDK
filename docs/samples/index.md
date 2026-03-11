# Sample Scenes

The SDK includes comprehensive sample scenes demonstrating every feature.

---

## Sample Location

```
Assets/_IntelliVerseXSDK/Samples/Scenes/
```

---

## Available Samples

### Authentication

| Scene | Description |
|-------|-------------|
| `IVX_AuthTest` | Complete authentication flow with all providers |

**Features Demonstrated:**
- Email/password registration and login
- Device ID (guest) authentication
- Social login (Google, Apple, Facebook)
- Account linking
- Password recovery
- Session persistence

[View Auth Demo Details](auth-demo.md)

---

### Ads Integration

| Scene | Description |
|-------|-------------|
| `IVX_AdsTest` | Ad network integration showcase |

**Features Demonstrated:**
- Rewarded video ads
- Interstitial ads
- Banner ads
- Ad network waterfall
- Test mode

[View Ads Demo Details](ads-demo.md)

---

### Friends & Social

| Scene | Description |
|-------|-------------|
| `IVX_Friends` | Complete friends system implementation |

**Features Demonstrated:**
- Friend list display
- Add/remove friends
- Friend requests
- Real-time presence
- Search users

[View Friends Demo Details](friends-demo.md)

---

### Leaderboards

| Scene | Description |
|-------|-------------|
| `IVX_LeaderboardTest` | Leaderboard integration example |

**Features Demonstrated:**
- Global leaderboard
- Friends leaderboard
- Score submission
- Pagination
- Real-time updates

[View Leaderboard Demo Details](leaderboard-demo.md)

---

### Daily Quiz

| Scene | Description |
|-------|-------------|
| `IVX_DailyQuiz` | Daily quiz implementation |

**Features Demonstrated:**
- Question display
- Answer selection
- Timer mechanics
- Progress tracking
- Results display

[View Daily Quiz Demo Details](daily-quiz-demo.md)

---

### Weekly Quiz

| Scene | Description |
|-------|-------------|
| `IVX_WeeklyQuizTest` | Weekly tournament quiz |

**Features Demonstrated:**
- Extended quiz format
- Tournament leaderboard
- Reward distribution
- Weekly reset

[View Weekly Quiz Demo Details](weekly-quiz-demo.md)

---

### Wallet & Economy

| Scene | Description |
|-------|-------------|
| `IVX_WalletTest` | Virtual currency system |

**Features Demonstrated:**
- Multiple currencies
- Balance display
- Transaction history
- Purchase flow

[View Wallet Demo Details](wallet-demo.md)

---

### Player Profile

| Scene | Description |
|-------|-------------|
| `IVX_Profile` | User profile management |

**Features Demonstrated:**
- Profile display
- Avatar selection
- Username editing
- Stats display

[View Profile Demo Details](profile-demo.md)

---

### Home Screen

| Scene | Description |
|-------|-------------|
| `IVX_HomeScreen` | Complete home screen example |

**Features Demonstrated:**
- Navigation setup
- Module integration
- UI composition

[View Home Screen Demo Details](home-screen-demo.md)

---

### Share & Rate

| Scene | Description |
|-------|-------------|
| `IVX_Share&RateUs` | Sharing and rating features |

**Features Demonstrated:**
- Native share dialog
- App rating prompts
- Deep linking

[View Share Demo Details](share-demo.md)

---

### More of Us

| Scene | Description |
|-------|-------------|
| `IVX_MoreOfUs` | Cross-game promotion |

**Features Demonstrated:**
- Game listing
- Store links
- Promotional banners

[View More of Us Demo Details](more-of-us-demo.md)

---

## Running Samples

### Prerequisites

1. SDK properly installed
2. Configuration file set up
3. Backend connection configured

### Steps

1. Open sample scene from `Samples/Scenes/`
2. Ensure `IntelliVerseXConfig` is in `Resources/`
3. Enter Play Mode
4. Follow on-screen instructions

### Testing Without Backend

Some samples work offline with mock data:

```csharp
// Enable mock mode in scene
mockModeEnabled = true; // Toggle in inspector
```

---

## Sample Code Patterns

### Basic Initialization Pattern

```csharp
public class SampleScene : MonoBehaviour
{
    async void Start()
    {
        // Initialize SDK
        await IntelliVerseXSDK.InitializeAsync();
        
        // Connect to backend
        await IVXNakamaManager.ConnectAsync();
        
        // Authenticate
        if (!IVXAuthService.IsAuthenticated)
        {
            await IVXAuthService.LoginWithDeviceIdAsync();
        }
        
        // Load feature-specific data
        await LoadSampleData();
    }
}
```

### UI Update Pattern

```csharp
void OnEnable()
{
    // Subscribe to events
    IVXSomeManager.OnDataChanged += UpdateUI;
}

void OnDisable()
{
    // Unsubscribe
    IVXSomeManager.OnDataChanged -= UpdateUI;
}
```

---

## Customizing Samples

Samples are designed to be modified:

1. **Duplicate the scene** before editing
2. **Modify UI** - Change colors, layouts, text
3. **Extend logic** - Add your game-specific features
4. **Use as templates** - Copy patterns to your scenes

---

## Sample Dependencies

Each sample scene includes:
- Required prefabs
- UI components
- Test configuration

Import all sample assets via Package Manager to ensure dependencies.

---

## Need Help?

- Check [Troubleshooting](../troubleshooting/index.md)
- Review [Module Documentation](../modules/index.md)
- Ask in [GitHub Discussions](https://github.com/Intelli-verse-X/Intelli-verse-X-Unity-SDK/discussions)
