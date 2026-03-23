# IVX Leaderboard Entry Prefab Template

**File**: `IVXLeaderboardEntry.prefab`  
**Location**: `/SDK/UI/Prefabs/`  
**Purpose**: Individual leaderboard row (instantiated by IVXLeaderboardManager)

## Hierarchy Structure

```
IVXLeaderboardEntry (GameObject)
├── Background (Image)
├── RankSection
│   ├── RankBadge (Image) - Medal/trophy icon
│   └── RankText (TextMeshPro) - "#1", "#2", etc.
├── AvatarSection
│   ├── AvatarFrame (Image)
│   └── AvatarImage (Image)
├── PlayerInfoSection
│   ├── UsernameText (TextMeshPro) - "PlayerName"
│   └── CountryFlag (Image) - Optional
├── ScoreSection
│   └── ScoreText (TextMeshPro) - "1,234,567"
└── HighlightGlow (Image) - Shows for current player
```

## Required Components

### Root GameObject
- **Name**: `IVXLeaderboardEntry`
- **RectTransform**: 
  - Width: `100%` (stretch)
  - Height: `80`
  - Anchor: `Top Stretch`
- **Image**: Background with rounded corners
- **Layout Element**:
  - Preferred Height: `80`
  - Flexible Width: `1`

## Child Object Specifications

### Background
- **Image**:
  - Color: `#2A2A3E` (default), `#3DDC8440` (player highlight)
  - Material: Rounded corners (border radius 8px)
  - Raycast Target: `✗` (performance)

### RankSection
- **Position**: Left side (X: 20)
- **Size**: 60x60
- **RankBadge** (Image):
  - Sprite: Gold medal (rank 1-3), Trophy (rank 4-10), None (11+)
  - Size: 50x50
  - Active: Only for top 10
- **RankText** (TextMeshPro):
  - Font: `Roboto Bold`
  - Size: `32`
  - Color: `#FFD700` (1-3), `#FFFFFF` (4+)
  - Alignment: `Center`
  - Auto-size: `✗`

### AvatarSection
- **Position**: Left of username (X: 100)
- **Size**: 60x60
- **AvatarFrame** (Image):
  - Sprite: Circle frame
  - Color: `#FFD700` (top 3), `#808080` (others)
- **AvatarImage** (Image):
  - Sprite: Player avatar or default
  - Size: 54x54 (6px padding)
  - Preserve Aspect: `✓`
  - Type: `Simple`

### PlayerInfoSection
- **Position**: Center-left (X: 180)
- **Size**: Flexible width
- **UsernameText** (TextMeshPro):
  - Font: `Roboto Medium`
  - Size: `24`
  - Color: `#FFFFFF`
  - Alignment: `Middle Left`
  - Overflow: `Ellipsis`
  - Max Visible Characters: `20`
- **CountryFlag** (Image) - Optional:
  - Size: 32x24
  - Position: Right of username
  - Sprite: ISO country code → flag sprite

### ScoreSection
- **Position**: Right side (X: -20)
- **Size**: Auto-width
- **ScoreText** (TextMeshPro):
  - Font: `Roboto Bold`
  - Size: `28`
  - Color: `#FFD700`
  - Alignment: `Middle Right`
  - Format: `{0:N0}` (thousand separators)

### HighlightGlow
- **Image**:
  - Color: `#3DDC8480` (semi-transparent green)
  - Material: Glow shader
  - Active: Only for current player's entry
  - Raycast Target: `✗`

## Materials & Colors

### Rank Colors
```csharp
1st Place:  #FFD700 (Gold)
2nd Place:  #C0C0C0 (Silver)
3rd Place:  #CD7F32 (Bronze)
4-10:       #FFFFFF (White)
11+:        #B0B0C0 (Gray)
```

### Background States
```csharp
Normal:         #2A2A3E (dark purple)
Hover:          #3A3A4E (medium purple)
Player Entry:   #3DDC8440 (green tint)
Top 3:          #FFD70020 (gold tint)
```

## Animation

### Entry Spawn Animation
```csharp
// Stagger entries by 50ms each
DOTween.Sequence()
    .Join(transform.DOScale(0.8f, 0.2f).From())
    .Join(canvasGroup.DOFade(0f, 0.2f).From())
    .SetEase(Ease.OutBack)
    .SetDelay(index * 0.05f);
```

### Player Entry Pulse
```csharp
// Pulse highlight every 2 seconds
highlightGlow.DOFade(0.3f, 1f)
    .SetLoops(-1, LoopType.Yoyo)
    .SetEase(Ease.InOutSine);
```

### Rank Badge Shine
```csharp
// Rotate shine effect on medals (top 3 only)
rankBadge.transform.DORotate(new Vector3(0, 0, 360), 3f, RotateMode.FastBeyond360)
    .SetLoops(-1, LoopType.Restart)
    .SetEase(Ease.Linear);
```

## Code Integration

### IVXLeaderboardManager Auto-Population
```csharp
// IVXLeaderboardManager automatically calls this
private void DisplayLeaderboardData(IVXLeaderboardData data)
{
    foreach (Transform child in entriesContainer)
    {
        Destroy(child.gameObject);
    }
    
    for (int i = 0; i < data.records.Count; i++)
    {
        var record = data.records[i];
        var entry = Instantiate(entryPrefab, entriesContainer);
        
        // Auto-finds child components by name
        var rankText = entry.transform.Find("RankText")?.GetComponent<TMP_Text>();
        var usernameText = entry.transform.Find("UsernameText")?.GetComponent<TMP_Text>();
        var scoreText = entry.transform.Find("ScoreText")?.GetComponent<TMP_Text>();
        
        if (rankText != null) rankText.text = $"#{record.rank}";
        if (usernameText != null) usernameText.text = record.username;
        if (scoreText != null) scoreText.text = record.score.ToString("N0");
        
        // Highlight player's entry
        if (record.owner_id == nakamaManager.Session?.UserId)
        {
            entry.GetComponent<Image>().color = new Color(0.24f, 0.86f, 0.52f, 0.25f);
        }
    }
}
```

### Custom Entry Component (Optional)
```csharp
public class IVXLeaderboardEntryUI : MonoBehaviour
{
    [SerializeField] private TMP_Text rankText;
    [SerializeField] private TMP_Text usernameText;
    [SerializeField] private TMP_Text scoreText;
    [SerializeField] private Image avatar;
    [SerializeField] private Image rankBadge;
    [SerializeField] private GameObject highlightGlow;
    
    public void Setup(IVXLeaderboardRecord record, bool isPlayer)
    {
        rankText.text = $"#{record.rank}";
        usernameText.text = record.username;
        scoreText.text = record.score.ToString("N0");
        
        // Show medals for top 3
        if (record.rank <= 3)
        {
            rankBadge.gameObject.SetActive(true);
            rankBadge.sprite = GetMedalSprite(record.rank);
        }
        else
        {
            rankBadge.gameObject.SetActive(false);
        }
        
        // Highlight player
        highlightGlow.SetActive(isPlayer);
        
        // Animate entry
        AnimateEntry(record.rank - 1);
    }
    
    private Sprite GetMedalSprite(int rank)
    {
        return rank switch
        {
            1 => Resources.Load<Sprite>("UI/Medals/Gold"),
            2 => Resources.Load<Sprite>("UI/Medals/Silver"),
            3 => Resources.Load<Sprite>("UI/Medals/Bronze"),
            _ => null
        };
    }
    
    private void AnimateEntry(int index)
    {
        transform.localScale = Vector3.one * 0.8f;
        GetComponent<CanvasGroup>().alpha = 0f;
        
        transform.DOScale(1f, 0.2f)
            .SetDelay(index * 0.05f)
            .SetEase(Ease.OutBack);
            
        GetComponent<CanvasGroup>().DOFade(1f, 0.2f)
            .SetDelay(index * 0.05f);
    }
}
```

## Asset Requirements

### Sprites
- `medal_gold.png` - 128x128, transparent
- `medal_silver.png` - 128x128, transparent
- `medal_bronze.png` - 128x128, transparent
- `trophy_icon.png` - 128x128, transparent
- `avatar_frame.png` - 128x128, circle mask
- `country_flags_atlas.png` - 32x24 per flag

### Fonts
- `Roboto-Bold.ttf`
- `Roboto-Medium.ttf`
- `Roboto-Regular.ttf`

## Performance Optimization

### Object Pooling
```csharp
// Reuse entries instead of Destroy/Instantiate
private Queue<GameObject> entryPool = new Queue<GameObject>();

private GameObject GetPooledEntry()
{
    if (entryPool.Count > 0)
    {
        var entry = entryPool.Dequeue();
        entry.SetActive(true);
        return entry;
    }
    return Instantiate(entryPrefab, entriesContainer);
}

private void ReturnToPool(GameObject entry)
{
    entry.SetActive(false);
    entryPool.Enqueue(entry);
}
```

### Texture Atlas
```
Combine all UI sprites into single atlas:
- Medals (gold, silver, bronze)
- Trophy icon
- Avatar frame
- Background patterns

Result: 1 draw call instead of 5+ per entry
```

## Accessibility

- **Semantic Labels**: `aria-label="Rank {rank}, {username}, {score} points"`
- **Color Blind**: Use icons (medal/trophy) not just colors
- **Touch Targets**: 80px height for easy tapping
- **Contrast**: WCAG AA compliant (4.5:1 minimum)

## Testing Checklist

- [ ] Displays rank correctly (1-100)
- [ ] Username truncates with ellipsis if too long
- [ ] Score formats with thousand separators (1,234,567)
- [ ] Top 3 show gold/silver/bronze medals
- [ ] Player's entry highlights in green
- [ ] Animations stagger smoothly (50ms delay)
- [ ] Avatar loads asynchronously without blocking
- [ ] Works with 10, 50, 100+ entries
- [ ] Pool optimization reduces GC spikes

---

**Created**: November 2025  
**Version**: 1.0  
**Compatibility**: Unity 2021.3+, TextMeshPro 3.0+, DOTween 1.2+
