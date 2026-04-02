# Demos

Self-contained demo scenes that showcase individual SDK features. Each demo builds its own UI at runtime -- no prefabs or scene setup required.

---

## Overview

| | |
|---|---|
| **Namespace** | `IntelliVerseX.Demos` |
| **Assembly** | `IntelliVerseX.Demos` |
| **Dependencies** | TextMeshPro, UnityEngine.UI, module-specific assemblies |
| **Location** | `Assets/_IntelliVerseXSDK/Demos/` |

The SDK ships with ready-to-run demos that exercise key modules in isolation. They are designed for:

- **Evaluation** -- see a feature in action before reading the API docs.
- **Integration testing** -- verify backend connectivity with real UI.
- **Prototyping** -- fork a demo and restyle it into a production screen.

| Demo | Class | Backend Required | Highlights |
|---|---|---|---|
| AI Voice Chat | `IVXAIVoiceChatDemo` | Optional (mock data available) | Persona selection, text/voice input, chat bubbles |
| AI Host | `IVXAIHostDemo` | Optional (mock data available) | Live host commentary overlay, player context |
| Spin Wheel | `IVXSpinWheelDemo` | No | Segmented wheel, spin animation, reward reveal, cooldown |
| Daily Streak | `IVXStreakDemo` | No | 7-day calendar, streak counter, shields, session booster |
| Offerwall | `IVXOfferwallDemo` | No | Scrollable offer cards, tags, reward claims |

---

## How to Use

1. Create an empty **GameObject** in any scene.
2. Add a **Canvas** component (or use an existing Canvas).
3. Attach the desired demo script (e.g. `IVXSpinWheelDemo`) to the Canvas GameObject.
4. Press **Play**.

The demo constructs all UI elements programmatically on `Start`. No additional assets, prefabs, or configuration files are needed.

!!! tip "Multiple demos"
    Each demo expects to own the full Canvas. To try several demos in one session, place each on a separate Canvas and toggle their GameObjects.

---

## Demo Descriptions

### AI Voice Chat (`IVXAIVoiceChatDemo`)

A full persona-based voice and text chat interface powered by the `IntelliVerseX.AI` module.

**What it showcases:**

- Persona gallery with selectable AI characters (Fortune Teller, AI Teacher, Career Coach, Story Teller, Party Host, Health Advisor).
- Two-column chat layout with distinct user and AI message bubbles.
- Voice recording button with visual feedback.
- Conversation history and session management.

**Backend behaviour:** When the AI backend is connected, messages are routed to the live inference service. Without a backend, the demo responds with mock replies so the full UI flow can be exercised offline.

---

### AI Host (`IVXAIHostDemo`)

An in-game overlay that simulates a live AI host providing commentary during gameplay.

**What it showcases:**

- Host message feed with animated entry.
- Player context panel (score, round, status).
- Start / stop controls for the commentary stream.
- Timed auto-advance of host lines.

**Backend behaviour:** Functions identically to the Voice Chat demo -- live when connected, mock commentary lines when offline.

---

### Spin Wheel (`IVXSpinWheelDemo`)

A segmented prize wheel with physics-style spin animation.

**What it showcases:**

- Eight configurable segments with labels and colours (50 Coins, 100 Gems, 2x XP, Shield, 200 Coins, Rare Item, 25 Gems, Free Spin).
- Smooth deceleration spin animation with pointer indicator.
- Reward reveal popup after the wheel stops.
- Cooldown timer preventing immediate re-spin.

**Backend behaviour:** Fully client-side. No backend connection needed.

---

### Daily Streak (`IVXStreakDemo`)

A 7-day daily-rewards calendar with streak tracking.

**What it showcases:**

- Seven day tiles with reward icons and escalating values.
- Visual states: claimed, today (claimable), and locked (future).
- Current streak counter and streak-shield indicator.
- Session XP booster tied to streak length.
- Claim button with animated state transition.

**Backend behaviour:** Fully client-side with simulated state. In production, streak data would be persisted via the Backend module.

---

### Offerwall (`IVXOfferwallDemo`)

A scrollable list of promotional offer cards.

**What it showcases:**

- Vertically scrolling card layout with offer details, reward amounts, and action buttons.
- Tag badges per offer (Hot, New, Easy) with distinct colours.
- Claim and completion flow per card.
- Coin/gem reward display.

**Backend behaviour:** Fully client-side with hardcoded offer data. In production, offers would be fetched from the Backend module.

---

## Configuration

### Backend connectivity matrix

| Demo | Works offline | Live backend adds |
|---|---|---|
| AI Voice Chat | Yes (mock personas and replies) | Real AI inference, voice-to-text |
| AI Host | Yes (mock host lines) | Context-aware live commentary |
| Spin Wheel | Yes | Server-validated rewards, anti-cheat |
| Daily Streak | Yes (simulated calendar) | Persistent streak across sessions |
| Offerwall | Yes (hardcoded offers) | Dynamic offer catalog, completion tracking |

### Dependencies per demo

| Demo | Required imports |
|---|---|
| AI Voice Chat | `IntelliVerseX.AI`, `TMPro`, `UnityEngine.UI` |
| AI Host | `IntelliVerseX.AI`, `TMPro`, `UnityEngine.UI` |
| Spin Wheel | `TMPro`, `UnityEngine.UI` |
| Daily Streak | `TMPro`, `UnityEngine.UI` |
| Offerwall | `TMPro`, `UnityEngine.UI` |

---

## Customization

Every demo defines its colour palette as `static readonly Color` constants at the top of the class. Changing the look requires editing only these values.

### Colour constants (common pattern)

```csharp
private static readonly Color COL_BG      = new Color(0.05f, 0.06f, 0.09f);
private static readonly Color COL_PANEL   = new Color(0.10f, 0.12f, 0.17f);
private static readonly Color COL_PRIMARY = new Color(0.25f, 0.50f, 0.90f);
private static readonly Color COL_GOLD    = new Color(0.95f, 0.78f, 0.20f);
private static readonly Color COL_TEXT    = new Color(0.92f, 0.93f, 0.95f);
private static readonly Color COL_DIM     = new Color(0.55f, 0.58f, 0.64f);
```

To restyle a demo:

1. Open the demo script.
2. Modify the `COL_*` constants to match your game's palette.
3. Press Play to see the result immediately.

### Content customization

| Demo | Editable data |
|---|---|
| AI Voice Chat | `MOCK_PERSONAS` array (persona names and count) |
| AI Host | `MOCK_HOST_LINES` array (commentary strings) |
| Spin Wheel | `SEGMENT_LABELS` and `SEGMENT_COLORS` arrays |
| Daily Streak | `DAY_REWARDS` and `DAY_ICONS` arrays |
| Offerwall | `OfferData` struct entries (title, description, reward, icon, tag) |

### Layout and sizing

All demos build UI with code using `RectTransform` anchors and `LayoutGroup` components. To adjust sizing, search for dimension constants or `sizeDelta` assignments within the relevant `Build*()` methods of each demo class.

!!! note "Production use"
    Demos are intended as reference implementations. For production, extract the UI logic you need into your own MonoBehaviour backed by the corresponding SDK module API rather than shipping the demo scripts directly.

---

## Demo Hub

`IVXDemoHub` is a master navigation scene containing cards for all 16+ demos. Drop the `IVX_DemoHub.prefab` (or the `IVXDemoHub` MonoBehaviour) onto a Canvas to get a clickable grid of every module demo. Each card opens its respective demo and provides a "Back to Hub" button.

### Discord Social Demo

`IVXDiscordSocialDemo` provides a 7-tab programmatic UI covering:

| Tab | Features |
|-----|----------|
| Account | Link / unlink, OAuth2, provisional accounts, PKCE mobile flow |
| Presence | Rich presence, party info, buttons, URLs, RPC-only mode |
| Friends | Unified friend list (Discord + Game), block/unblock |
| DMs | Send, edit, history, summaries, chat visibility toggle |
| Lobby & Voice | Create/join, lobby chat, voice calls, mute/deafen, VAD slider |
| Invites | Send/accept/decline game invites |
| Moderation | Metadata processing, voice capture, user reporting |

### Prefabs for All Demos

Every demo has a corresponding prefab under `Assets/_IntelliVerseXSDK/Prefabs/`. Generate or refresh them via **IntelliVerseX > Generate All Prefabs**.

---

## Related Documentation

- [Core Module](core.md) -- Foundation utilities used by all demos
- [Discord Social SDK](discord.md) -- Discord integration reference
- [Monetization Module](monetization.md) -- Production spin-wheel and offerwall APIs
- [Backend Module](backend.md) -- Server connectivity for live demo modes
