# Hiro Live-Ops Demo

Three sample scripts demonstrating Hiro-powered live operations features — a spin wheel with animated rewards, a daily streak/rewards calendar, and a scrollable offerwall.

---

## Scene Overview

**Location:**

- `Assets/_IntelliVerseXSDK/Demos/IVXSpinWheelDemo.cs`
- `Assets/_IntelliVerseXSDK/Demos/IVXStreakDemo.cs`
- `Assets/_IntelliVerseXSDK/Demos/IVXOfferwallDemo.cs`

This sample demonstrates:

- Animated spin wheel with 8 reward segments
- Ease-out cubic spin animation with random landing
- Spin count limits and cooldown messaging
- 7-day daily reward calendar with streak tracking
- Streak shields and session boosters
- Claim flow with visual state transitions
- Scrollable offerwall with tagged offer cards
- Reward tracking and completion states

---

## Spin Wheel — IVXSpinWheelDemo

### Scene Hierarchy

```
Canvas
├── Background
├── Root
│   ├── Title ("Daily Spin Wheel")
│   ├── Spins remaining counter
│   ├── WheelArea
│   │   ├── Wheel (rotatable, 8 segments)
│   │   ├── Pointer (gold triangle)
│   │   └── Center dot (star)
│   ├── Result text
│   ├── Spin button
│   └── Cooldown text
```

### Spin Animation Logic

```csharp
private IEnumerator SpinAnimation()
{
    int winIdx = Random.Range(0, SEGMENT_LABELS.Length);
    float anglePerSeg = 360f / SEGMENT_LABELS.Length;
    float targetAngle = 360f * 5 + (anglePerSeg * winIdx)
                        + Random.Range(5f, anglePerSeg - 5f);

    float duration = 3.5f;
    float elapsed = 0f;
    float startAngle = _wheel.localEulerAngles.z;

    while (elapsed < duration)
    {
        elapsed += Time.deltaTime;
        float t = elapsed / duration;
        float ease = 1f - Mathf.Pow(1f - t, 3f); // ease-out cubic
        float angle = Mathf.Lerp(startAngle, startAngle + targetAngle, ease);
        _wheel.localRotation = Quaternion.Euler(0, 0, angle);
        yield return null;
    }

    _resultText.text = $"You won: {SEGMENT_LABELS[winIdx]}!";
}
```

### Reward Segments

| Segment | Color |
|---------|-------|
| 50 Coins | Red |
| 100 Gems | Green |
| 2x XP | Blue |
| Shield | Orange |
| 200 Coins | Purple |
| Rare Item | Gold |
| 25 Gems | Teal |
| Free Spin | Gray |

---

## Daily Streak — IVXStreakDemo

### Scene Hierarchy

```
Canvas
├── Background
├── Root
│   ├── Title ("Daily Rewards")
│   ├── StreakRow (fire icon + "4-Day Streak!")
│   ├── Grid (7 day cards with icons and rewards)
│   ├── ShieldRow (shield count)
│   ├── BoostRow (session booster status)
│   ├── Claim button
│   └── Subtitle
```

### Claim Logic

```csharp
private void OnClaim()
{
    if (_todayClaimed) return;
    _todayClaimed = true;

    if (_currentStreak < 7)
    {
        _dayImages[_currentStreak].color = COL_CLAIMED;
        _dayCheckmarks[_currentStreak].text = "\u2713";
        _currentStreak++;
    }

    _streakCounter.text = $"{_currentStreak}-Day Streak!";
    _claimLabel.text = "\u2713  Claimed!";
    _claimBtn.interactable = false;
}
```

### Daily Rewards Schedule

| Day | Reward | Icon |
|-----|--------|------|
| 1 | 50 Coins | Coin |
| 2 | 1 Gem | Gem |
| 3 | 100 Coins | Coin |
| 4 | Shield | Shield |
| 5 | 2 Gems | Gem |
| 6 | 200 Coins | Coin |
| 7 | Rare Chest | Gift |

---

## Offerwall — IVXOfferwallDemo

### Scene Hierarchy

```
Canvas
├── Background
├── Root
│   ├── Header ("Offerwall")
│   ├── Earnings bar (total rewards claimed)
│   └── ScrollArea
│       └── Offer cards (7 mock offers)
│           ├── Icon + Title + Tag (HOT/NEW/EASY)
│           ├── Description + Reward
│           └── Start/Done button
```

### Offer Card Structure

```csharp
// Each card displays: icon, title, tag badge, description, reward, and action button
var card = MakePanel(parent, "Card", COL_CARD);
// ... icon area, info column with title row + tag ...
btn.onClick.AddListener(() =>
{
    btnLabel.text = "\u2713 Done";
    btn.interactable = false;
    _totalClaimed++;
    _totalEarned.text = $"{_totalClaimed} reward{(_totalClaimed > 1 ? "s" : "")} claimed";
});
```

### Sample Offers

| Offer | Reward | Tag |
|-------|--------|-----|
| Install Puzzle Quest | 500 Gems | HOT |
| Sign Up for StreamPlus | 200 Coins | EASY |
| Complete Survey #12 | 150 Gems | NEW |
| Battle Royale Download | 750 Gems | HOT |
| Watch 3 Video Ads | 100 Coins | EASY |
| Fantasy RPG: Reach Lv10 | 1200 Gems | HOT |
| Newsletter Signup | 50 Coins | EASY |

---

## How to Use

### Running the Spin Wheel

1. Add `IVXSpinWheelDemo` to a Canvas
2. Press **Play**
3. Click **SPIN!** — watch the wheel animate and land on a reward
4. After 3 spins, the cooldown message appears

### Running the Streak Demo

1. Add `IVXStreakDemo` to a Canvas
2. Press **Play**
3. Click **"Claim Day 5 Reward"** to advance the streak
4. Observe the shield count and booster activation

### Running the Offerwall

1. Add `IVXOfferwallDemo` to a Canvas
2. Press **Play**
3. Scroll through offers, click **Start** to complete them
4. Watch the earnings bar update

---

## Customization

### Changing Spin Rewards

Edit `SEGMENT_LABELS` and `SEGMENT_COLORS` in `IVXSpinWheelDemo.cs`.

### Extending the Streak Calendar

Modify `DAY_REWARDS` and `DAY_ICONS` arrays, and adjust the grid loop count.

### Adding Real Offers

Replace `MOCK_OFFERS` with data fetched from your Hiro backend via the economy system.

---

## See Also

- [Hiro Module](../modules/hiro.md)
- [Hiro Integration Guide](../guides/hiro-integration.md)
