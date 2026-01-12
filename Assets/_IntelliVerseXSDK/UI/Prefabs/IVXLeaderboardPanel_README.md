# IVX Leaderboard Prefab Template

**File**: `IVXLeaderboardPanel.prefab`  
**Location**: `/SDK/UI/Prefabs/`  
**Purpose**: Ready-to-use leaderboard UI with tab navigation

## Hierarchy Structure

```
IVXLeaderboardPanel (Canvas)
├── Background (Image)
├── Header
│   ├── Title (TextMeshPro) - "Leaderboards"
│   └── CloseButton (Button)
├── TabButtons (Horizontal Layout Group)
│   ├── DailyTab (Button + TextMeshPro) - "Daily"
│   ├── WeeklyTab (Button + TextMeshPro) - "Weekly"
│   ├── MonthlyTab (Button + TextMeshPro) - "Monthly"
│   ├── AlltimeTab (Button + TextMeshPro) - "All-Time"
│   └── GlobalTab (Button + TextMeshPro) - "Global"
├── LeaderboardContent (ScrollRect)
│   ├── Viewport (Mask)
│   │   └── Content (Vertical Layout Group)
│   │       └── [Entries spawn here]
├── PlayerRankSection
│   ├── Background (Image)
│   ├── PlayerRankText (TextMeshPro) - "Your Rank: #42"
│   └── PlayerScoreText (TextMeshPro) - "Score: 1,234"
└── RefreshButton (Button + TextMeshPro) - "Refresh"
```

## Component Configuration

### IVXLeaderboardPanel (Root GameObject)
- **IVXLeaderboardManager** (Component)
  - Nakama Manager: `Drag QuizVerseNakamaManager here`
  - Leaderboard Panel: `This GameObject`
  - Entries Container: `Content`
  - Entry Prefab: `IVXLeaderboardEntry`
  - Player Rank Text: `PlayerRankText`
  - Daily Button: `DailyTab`
  - Weekly Button: `WeeklyTab`
  - Monthly Button: `MonthlyTab`
  - Alltime Button: `AlltimeTab`
  - Global Button: `GlobalTab`
  - Refresh Button: `RefreshButton`
  - Entries Per Page: `50`
  - Auto Refresh On Show: `✓`
  - Cache Time Seconds: `30`

### TabButtons Configuration
- **Horizontal Layout Group**
  - Spacing: `10`
  - Child Alignment: `Middle Center`
  - Child Force Expand: `Width ✓, Height ✓`
  
- **Each Tab Button**
  - Image: Selected color = `#FFD700` (gold)
  - TextMeshPro: Font size = `24`, Alignment = `Center`

### LeaderboardContent (ScrollRect)
- **ScrollRect Component**
  - Content: `Content`
  - Horizontal: `✗`
  - Vertical: `✓`
  - Movement Type: `Elastic`
  - Scrollbar Visibility: `Auto Hide`

- **Content (Vertical Layout Group)**
  - Spacing: `5`
  - Child Alignment: `Upper Center`
  - Child Force Expand: `Width ✓, Height ✗`
  - Child Control Size: `Width ✓, Height ✓`

### PlayerRankSection
- **Layout**
  - Anchor: `Bottom`
  - Height: `80`
  - Background: Semi-transparent `#00000080`

## Materials & Colors

### Color Palette
- **Background**: `#1E1E2E` (dark purple)
- **Header**: `#2A2A3E` (medium purple)
- **Tab Active**: `#FFD700` (gold)
- **Tab Inactive**: `#4A4A5E` (gray)
- **Player Highlight**: `#3DDC84` (green accent)
- **Text Primary**: `#FFFFFF` (white)
- **Text Secondary**: `#B0B0C0` (light gray)

### Fonts
- **Title**: `Exo 2 Bold`, Size `36`
- **Tab Text**: `Roboto Medium`, Size `24`
- **Rank Text**: `Roboto Bold`, Size `28`
- **Score Text**: `Roboto Regular`, Size `20`

## Animation

### Tab Selection
```
Tween: Scale 1.0 → 1.1 → 1.0 (200ms)
Tween: Color Gray → Gold (150ms)
Ease: EaseOutQuad
```

### Panel Open/Close
```
Open: Scale 0.8 → 1.0 + Fade 0 → 1 (300ms)
Close: Scale 1.0 → 0.8 + Fade 1 → 0 (200ms)
Ease: EaseOutBack
```

### Refresh Animation
```
Button: Rotate 0° → 360° (500ms)
Ease: EaseInOutQuad
```

## Setup Instructions

1. **Import Prefab**:
   ```
   Drag IVXLeaderboardPanel.prefab into scene
   ```

2. **Assign Nakama Manager**:
   ```
   IVXLeaderboardManager → Nakama Manager Component
   Drag your QuizVerseNakamaManager from scene
   ```

3. **Test in Play Mode**:
   ```
   - Click Daily/Weekly/Monthly tabs
   - Verify entries populate
   - Check player rank highlight
   - Test refresh button
   ```

4. **Customize (Optional)**:
   ```
   - Change colors in Background/Header
   - Replace fonts with game theme
   - Adjust spacing/padding
   - Add sound effects to buttons
   ```

## Integration Code

### Show Leaderboard
```csharp
public class GameUI : MonoBehaviour
{
    [SerializeField] private GameObject leaderboardPanel;
    
    public void ShowLeaderboards()
    {
        leaderboardPanel.SetActive(true);
        // IVXLeaderboardManager auto-fetches data on enable
    }
}
```

### Custom Callbacks
```csharp
void Start()
{
    var lbManager = GetComponent<IVXLeaderboardManager>();
    
    lbManager.OnLeaderboardDataUpdated += (data) => {
        Debug.Log($"Leaderboards updated: {data.daily.records.Count} entries");
        PlaySound("leaderboard_updated");
    };
    
    lbManager.OnPeriodChanged += (period) => {
        Debug.Log($"Switched to {period} leaderboard");
        PlaySound("tab_switch");
    };
}
```

## Performance Tips

- **Object Pooling**: For 100+ entries, implement object pooling for LeaderboardEntry prefabs
- **Lazy Loading**: Only load first 50 entries, load more on scroll
- **Caching**: Use `cacheTimeSeconds` to avoid excessive API calls
- **Texture Atlasing**: Combine UI sprites into atlas for better batching

## Accessibility

- **Keyboard Navigation**: Tab order: Tabs → Scroll → Refresh → Close
- **Screen Reader**: Semantic labels on all interactive elements
- **Color Blind Mode**: Use icons in addition to colors for rank tiers
- **Font Scaling**: Support TextMeshPro auto-sizing

---

**Created**: November 2025  
**Version**: 1.0  
**Compatibility**: Unity 2021.3+, TextMeshPro 3.0+
