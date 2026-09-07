# Admin Dashboard, Analytics, Hiro, and Satori Readiness

This proofcheck confirms the SDK is not limited to QuizVerse. Any game built with the IntelliVerseX SDK can be wired into the same Nakama admin dashboard, analytics pipeline, Hiro systems, and Satori LiveOps surfaces when it follows the game ID and RPC/config conventions below.

## What Is Game-Agnostic

| Capability | SDK surface | Game-specific input | Admin dashboard surface |
| --- | --- | --- | --- |
| Analytics events | `IVXSatoriClient.CaptureEventAsync` | `game_id` metadata | Analytics, Game Intelligence, Satori metrics |
| Batch analytics | `IVXSatoriClient.CaptureEventsBatchAsync` | Event names and metadata taxonomy | Analytics, data lake, cohorts |
| Feature flags | `IVXSatoriClient.GetFlagAsync`, `GetAllFlagsAsync` | Flag names such as `mygame_enable_new_ui` | Feature Flags |
| Experiments | `IVXSatoriClient.GetExperimentsAsync`, `GetExperimentVariantAsync` | Experiment IDs and variants | Experiments |
| Live events | `IVXSatoriClient.GetLiveEventsAsync`, `JoinLiveEventAsync`, `ClaimLiveEventAsync` | Event IDs and reward config | Live Events |
| Messages | `IVXSatoriClient.GetMessagesAsync`, `ReadMessageAsync` | Message/audience IDs | Messages |
| Hiro systems | `IVXHiroCoordinator` and `IVXHiroRpcClient` | Config records and optional `gameId` payloads | Hiro Config, Economy, Retention |
| Challenges | `IVXHiroCoordinator.Instance.Challenges` | Challenge definitions in `hiro_configs/challenges` | Hiro Config, challenge analytics |
| Incentives | `IVXHiroCoordinator.Instance.Incentives` | Incentive definitions in `hiro_configs/incentives` | Hiro Config, retention analytics |

## Required New-Game Pattern

Every SDK-built game should define one canonical game ID, such as `quizverse`, `lasttolive`, or `my_new_game`.

Use that ID consistently in:

- Client analytics metadata: `game_id = "<game_id>"`.
- Legacy analytics manager game ID: `IVXAnalyticsManager.SetGameId("<game_id>")`.
- Legacy analytics manager RPC prefix when using game-prefixed RPCs: `IVXAnalyticsManager.SetGameRpcPrefix("<game_id>")`.
- Hiro reward calls that accept `gameId`.
- Admin dashboard QA fixture IDs: `ivx_qa_<game_id>_*`.
- Satori flag, event, experiment, message, and audience IDs.

## Recommended Bootstrap

```csharp
using System.Collections.Generic;
using IntelliVerseX.Analytics;
using IntelliVerseX.Hiro;
using IntelliVerseX.Satori;
using Nakama;
using UnityEngine;

public sealed class MyGameLiveOpsBootstrap : MonoBehaviour
{
    private const string GAME_ID = "my_new_game";

    public void Initialize(IClient nakamaClient, ISession session)
    {
        IVXAnalyticsManager.SetGameId(GAME_ID);
        IVXAnalyticsManager.SetGameRpcPrefix(GAME_ID);
        IVXAnalyticsManager.Instance.Initialize(nakamaClient, session);

        IVXSatoriClient.Instance.Initialize(nakamaClient, session);
        IVXHiroCoordinator.Instance.InitializeSystems(nakamaClient, session);
    }

    public async void TrackGameplayStart()
    {
        if (IVXSatoriClient.Instance == null || !IVXSatoriClient.Instance.IsInitialized)
            return;

        await IVXSatoriClient.Instance.CaptureEventAsync("gameplay_start", new Dictionary<string, string>
        {
            { "game_id", GAME_ID },
            { "mode", "classic" },
        });
    }
}
```

## Admin Dashboard Proofcheck

Production dashboard hardening and QuizVerse fixture verification proved the server path works for one shipped game:

- Admin login and proxy guard: passed.
- Admin-safe Satori flags, live events, experiments, and messages list RPCs: passed.
- Hiro `challenges` and `incentives` config read/write through admin proxy: passed.
- Player-facing reads for `ivx_qa_quizverse_flag`, `ivx_qa_quizverse_event`, `ivx_qa_quizverse_experiment`, `ivx_qa_quizverse_challenge`, and incentives: passed.

For a new SDK-built game, repeat the same QA with `<game_id>` substituted:

| QA object | Example |
| --- | --- |
| Feature flag | `ivx_qa_my_new_game_flag` |
| Live event | `ivx_qa_my_new_game_event` |
| Experiment | `ivx_qa_my_new_game_experiment` |
| Challenge | `ivx_qa_my_new_game_challenge` |
| Incentive config | `ivx_qa_my_new_game_incentive` in rewards/metadata |

## Production Checklist For New Games

- [ ] Game has a stable lowercase `game_id`.
- [ ] Client sends `game_id` on every Satori analytics event.
- [ ] `IVXSatoriClient` initializes after Nakama authentication.
- [ ] `IVXHiroCoordinator` initializes after Nakama authentication.
- [ ] Legacy `IVXAnalyticsManager` is configured with `SetGameId` and, if needed, `SetGameRpcPrefix`.
- [ ] Hiro `challenges` and `incentives` configs exist in the admin dashboard.
- [ ] Satori flag, event, experiment, and message IDs are prefixed by the game or environment.
- [ ] Admin-created QA objects are visible through player-facing RPCs before launch.
- [ ] Data freshness and event counts are checked in the admin analytics view after the first live session.

## Known Limitations

- The older `IVXAnalyticsManager` defaults to QuizVerse RPC IDs for backward compatibility. New games should use `IVXSatoriClient` for analytics or call `SetGameRpcPrefix`/`ConfigureRpcIds` before initialization.
- Satori player inbox delivery for audience-targeted messages needs a separate server-side fix before broad production message broadcasts are considered fully signed off.
- Admin dashboard changes are server-side and do not require shipping Nakama HTTP keys in any SDK-built game.
