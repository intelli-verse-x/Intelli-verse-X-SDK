# IVX Wallet Display Prefab Template

**File**: `IVXWalletDisplay.prefab`  
**Location**: `/SDK/UI/Prefabs/`  
**Purpose**: Auto-updating currency display for game/global wallets

## Prefab Variants

### Variant 1: Inline Wallet (Simple)
```
IVXWalletDisplay_Inline
├── CurrencyIcon (Image) - 💎
├── BalanceText (TextMeshPro) - "1,234"
└── PlusSign (Image) - Shows during increment
```

### Variant 2: Card Wallet (Detailed)
```
IVXWalletDisplay_Card
├── Background (Image)
├── Header
│   ├── CurrencyIcon (Image)
│   └── CurrencyLabel (TextMeshPro) - "Gems"
├── BalanceText (TextMeshPro) - "1,234"
└── ChangeIndicator
    ├── Arrow (Image) - ↑ or ↓
    └── ChangeText (TextMeshPro) - "+100"
```

### Variant 3: Compact Wallet (Header)
```
IVXWalletDisplay_Compact
├── IconBackground (Image)
├── CurrencyIcon (Image)
└── BalanceText (TextMeshPro) - "1.2K"
```

## Variant 1: Inline Wallet

### Hierarchy
```
IVXWalletDisplay_Inline (GameObject)
├── CurrencyIcon (Image)
├── BalanceText (TextMeshPro)
└── PlusSign (Image)
```

### Component Setup
- **IVXWalletDisplay**:
  - Wallet Type: `Game` or `Global`
  - Currency ID: `"gems"` (for game wallet)
  - Balance Text: `BalanceText`
  - Currency Icon: `CurrencyIcon`
  - Prefix: `""` (icon shows currency)
  - Suffix: `""`
  - Format: `"N0"` (1,234)
  - Abbreviate Large Numbers: `✓`
  - Animate Changes: `✓`
  - Animation Duration: `0.5`

### Layout
- **Horizontal Layout Group**:
  - Spacing: `8`
  - Child Alignment: `Middle Left`
  - Child Force Expand: `Height ✓`
  - Child Control Size: `Height ✓`

### Visual Specs
- **CurrencyIcon**:
  - Size: `32x32`
  - Preserve Aspect: `✓`
  - Raycast Target: `✗`
  
- **BalanceText**:
  - Font: `Roboto Bold`
  - Size: `24`
  - Color: `#FFD700` (gold)
  - Alignment: `Middle Left`
  - Auto-size: `✗`
  - Best Fit: `✓` (min 18, max 28)

- **PlusSign**:
  - Sprite: Green `+` icon
  - Size: `24x24`
  - Color: `#3DDC84`
  - Active: Only during increment animation
  - Alpha: Fades 1 → 0 over 1 second

## Variant 2: Card Wallet

### Hierarchy
```
IVXWalletDisplay_Card (GameObject)
├── Background (Image)
├── Header
│   ├── CurrencyIcon (Image)
│   └── CurrencyLabel (TextMeshPro)
├── BalanceText (TextMeshPro)
└── ChangeIndicator
    ├── Arrow (Image)
    └── ChangeText (TextMeshPro)
```

### Component Setup
- **IVXWalletDisplay**:
  - Wallet Type: `Game`
  - Currency ID: `"gems"`
  - Balance Text: `BalanceText`
  - Currency Icon: `CurrencyIcon`
  - Prefix: `""`
  - Suffix: `""`
  - Format: `"N0"`
  - Abbreviate Large Numbers: `✓`
  - Animate Changes: `✓`
  - Animation Duration: `0.5`
  - Animation Curve: `EaseInOut`

### Visual Specs
- **Background**:
  - Size: `200x120`
  - Sprite: Rounded rectangle
  - Color: `#2A2A3E`
  - Material: Gradient top-to-bottom
  - Border: 2px `#FFD700`

- **CurrencyIcon**:
  - Size: `48x48`
  - Position: Top-left (X: 16, Y: -16)
  
- **CurrencyLabel**:
  - Font: `Roboto Medium`
  - Size: `18`
  - Color: `#B0B0C0`
  - Text: `"Gems"`, `"Coins"`, etc.
  - Position: Right of icon (X: 72, Y: -16)

- **BalanceText**:
  - Font: `Roboto Bold`
  - Size: `36`
  - Color: `#FFD700`
  - Alignment: `Center`
  - Position: Center (Y: 0)

- **ChangeIndicator**:
  - Position: Bottom-right (X: -16, Y: 16)
  - Active: Shows for 2 seconds after balance change
  - **Arrow**: 
    - Size: `20x20`
    - Color: `#3DDC84` (increase), `#FF5252` (decrease)
    - Rotation: `0°` (up), `180°` (down)
  - **ChangeText**:
    - Font: `Roboto Medium`
    - Size: `16`
    - Color: `#3DDC84` (increase), `#FF5252` (decrease)
    - Format: `"+{0}"` or `"-{0}"`

## Variant 3: Compact Wallet

### Hierarchy
```
IVXWalletDisplay_Compact (GameObject)
├── IconBackground (Image)
├── CurrencyIcon (Image)
└── BalanceText (TextMeshPro)
```

### Component Setup
- **IVXWalletDisplay**:
  - Wallet Type: `Game`
  - Currency ID: `"gems"`
  - Balance Text: `BalanceText`
  - Currency Icon: `CurrencyIcon`
  - Prefix: `""`
  - Suffix: `""`
  - Format: `"N0"`
  - Abbreviate Large Numbers: `✓` (1.2K, 3.4M)
  - Animate Changes: `✓`
  - Animation Duration: `0.3`

### Visual Specs
- **IconBackground**:
  - Size: `48x48`
  - Sprite: Circle
  - Color: `#FFD70040` (gold tint)
  
- **CurrencyIcon**:
  - Size: `32x32`
  - Position: Center of background
  
- **BalanceText**:
  - Font: `Roboto Bold`
  - Size: `20`
  - Color: `#FFFFFF`
  - Alignment: `Middle Left`
  - Position: Right of icon (X: 56)
  - Shadow: 2px offset, 50% opacity

## Animation System

### Balance Increment
```csharp
// Count-up animation
DOTween.Sequence()
    .Append(DOTween.To(() => displayBalance, x => displayBalance = x, targetBalance, 0.5f))
    .Join(balanceText.transform.DOPunchScale(Vector3.one * 0.2f, 0.3f))
    .Join(plusSign.DOFade(1f, 0.1f).From(0f))
    .Append(plusSign.DOFade(0f, 0.5f).SetDelay(0.5f))
    .SetEase(Ease.OutQuad);
```

### Balance Decrement
```csharp
// Count-down animation (red flash)
DOTween.Sequence()
    .Append(DOTween.To(() => displayBalance, x => displayBalance = x, targetBalance, 0.5f))
    .Join(balanceText.DOColor(Color.red, 0.2f))
    .Append(balanceText.DOColor(goldColor, 0.3f))
    .SetEase(Ease.OutQuad);
```

### Large Change (1000+)
```csharp
// Particle burst + scale punch
DOTween.Sequence()
    .Append(balanceText.transform.DOScale(1.3f, 0.2f))
    .Append(balanceText.transform.DOScale(1f, 0.3f))
    .Join(SpawnCoinParticles(changeAmount))
    .SetEase(Ease.OutBack);
```

## Currency Icon Mapping

```csharp
private Dictionary<string, Sprite> currencyIcons = new()
{
    { "gems", Resources.Load<Sprite>("UI/Icons/gem") },
    { "coins", Resources.Load<Sprite>("UI/Icons/coin") },
    { "stars", Resources.Load<Sprite>("UI/Icons/star") },
    { "ivx_tokens", Resources.Load<Sprite>("UI/Icons/ivx_logo") },
};
```

## Usage Examples

### Example 1: Game Currency (Inline)
```csharp
// Automatically updates when IntelliVerseXUserIdentity.OnWalletUpdated fires
// No manual code needed - just attach IVXWalletDisplay component!

// In Inspector:
Wallet Type: Game
Currency ID: "gems"
Prefix: ""
Suffix: ""
Abbreviate Large Numbers: ✓
```

### Example 2: Global Tokens (Card)
```csharp
// In Inspector:
Wallet Type: Global
Currency ID: "" (ignored for global)
Prefix: "IVX "
Suffix: ""
Abbreviate Large Numbers: ✓
```

### Example 3: Multiple Currencies (Compact)
```csharp
// Create 3 compact wallets in horizontal layout:
// 1. Gems wallet
// 2. Coins wallet  
// 3. Stars wallet

// Each auto-updates independently
```

### Example 4: Custom Update Event
```csharp
public class GameUI : MonoBehaviour
{
    [SerializeField] private IVXWalletDisplay gemsWallet;
    
    void Start()
    {
        // Wallet already auto-subscribes to IntelliVerseXUserIdentity
        // But you can add custom behavior:
        
        IntelliVerseXUserIdentity.Instance.OnWalletUpdated += () =>
        {
            // Play sound
            AudioManager.Instance.PlaySound("coin_collect");
            
            // Spawn particles
            SpawnCelebrationParticles();
        };
    }
}
```

## Asset Requirements

### Sprites
- `gem_icon.png` - 128x128, transparent
- `coin_icon.png` - 128x128, transparent
- `star_icon.png` - 128x128, transparent
- `ivx_logo.png` - 128x128, transparent
- `plus_sign.png` - 64x64, green
- `arrow_up.png` - 64x64, green
- `arrow_down.png` - 64x64, red
- `wallet_background.png` - 256x256, rounded corners

### Fonts
- `Roboto-Bold.ttf`
- `Roboto-Medium.ttf`

### Particles (Optional)
- `coin_burst.prefab` - Particle system for large gains
- `sparkle_trail.prefab` - Particle trail during count-up

## Performance

### Memory
- Inline: ~2KB per instance
- Card: ~5KB per instance
- Compact: ~3KB per instance

### Updates
- Event-driven (no Update() loop)
- Only animates when balance changes
- Auto-unsubscribes OnDestroy

### Batching
- Use sprite atlas for all currency icons
- Share materials across wallet instances
- Pool particle effects

## Accessibility

- **Screen Reader**: "Balance: {amount} {currency}"
- **Color Blind**: Use icons, not just colors for currency
- **Font Scaling**: Support TextMeshPro auto-sizing
- **Contrast**: WCAG AA compliant (4.5:1)

## Testing Checklist

- [ ] Displays initial balance correctly
- [ ] Updates when balance changes
- [ ] Animates count-up smoothly (0.5s)
- [ ] Shows "+X" indicator on increment
- [ ] Abbreviates large numbers (1.2K, 3.4M)
- [ ] Formats with thousand separators (1,234,567)
- [ ] Handles zero balance
- [ ] Handles negative balance (if applicable)
- [ ] Multiple wallets update independently
- [ ] Particle effects don't stack/leak
- [ ] Currency icon loads asynchronously
- [ ] Works with both game and global wallets

---

**Created**: November 2025  
**Version**: 1.0  
**Compatibility**: Unity 2021.3+, TextMeshPro 3.0+, DOTween 1.2+
