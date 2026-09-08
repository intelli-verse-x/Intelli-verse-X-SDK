# IntelliVerseX UI Toolkit Demos

**Source of truth** for UITK sample UXML/USS/scripts/scenes.

## Open the UITK home scene

In the sandbox editor project:

1. Open `Assets/IntelliVerseX UITK Demo Scenes/IVX_HomeScreen_UITK.unity`
2. Enter Play Mode
3. Use the animated nav cards to open Auth and feature demos

If UI is blank, select `UITK_Demo` and confirm `UIDocument` has **Panel Settings** (`IVXUITKPanelSettings`) and a Visual Tree Asset. Rebind via menu:

**IntelliVerse-X → Samples → Create UITK Demo Scenes**

(That rebuilds scenes under `Assets/IntelliVerseX UITK Demo Scenes` and mirrors into `Scenes/` here.)

## Package Manager import

Package Manager → IntelliVerseX SDK → Samples → **UI Toolkit Demos** → Import.

Sandbox development uses a junction:

`Assets/Samples/IntelliVerseX SDK/UIToolkit` → `Packages/com.intelliversex.sdk/Samples~/UIToolkit`

## Scenes

| Scene | Controller |
|-------|------------|
| `IVX_HomeScreen_UITK` | `IVXUITKHomeDemo` |
| `IVX_AuthTest_UITK` | `IVXUITKAuthDemo` |
| `IVX_Friends_UITK` | `IVXUITKFriendsDemo` |
| `IVX_Clan_UITK` | `IVXUITKClanDemo` |
| `IVX_Profile_UITK` | `IVXUITKProfileDemo` |
| `IVX_LeaderboardTest_UITK` | `IVXUITKLeaderboardDemo` |
| `IVX_WalletTest_UITK` | `IVXUITKWalletDemo` |
| `IVX_WeeklyQuizTest_UITK` | `IVXUITKWeeklyQuizDemo` |
| `IVX_DailyQuiz_UITK` | `IVXUITKDailyQuizDemo` |
| `IVX_AdsTest_UITK` | `IVXUITKAdsDemo` |
| `IVX_MoreOfUs_UITK` | `IVXUITKMoreOfUsDemo` |
| `IVX_Share&RateUs_UITK` | `IVXUITKShareRateDemo` |

## Architecture

- `UI/IVXUITKTheme.uss` — shared tokens, panel fade/slide, button press scale
- `IVXUITKDemoShell` — UIDocument bind, animated show/hide, status, home nav
- Auth calls `APIManager` (login, guest, signup OTP initiate/confirm, forgot/reset)
- Feature shells call the same managers as the uGUI demos
- `IVXTestSceneNavigator` prefers `*_UITK` scene names when available

## Sync note

Legacy uGUI demos remain in `Samples~/TestScenes` and `Assets/IntelliVerseX Demo Scenes`. Prefer this UIToolkit folder as source of truth; sync Demo Scenes later if needed.
