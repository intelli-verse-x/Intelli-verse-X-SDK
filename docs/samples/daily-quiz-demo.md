# Daily Quiz Demo

This sample walks through **daily reset**, **question loading**, **streak tracking**, and **UI flow** using `IVXDailyQuizManager` and the slimmer `IVXQuizSessionManager.StartDailyQuizAsync` path.

## Scene

`IVX_DailyQuiz.unity` — include **`IVXDailyQuizManager`** on a `DontDestroyOnLoad` object (see `IVXDailyQuizTestController` for guarded creation).

## Daily reset logic

`IVXDailyQuizManager` persists completion and streak state in **PlayerPrefs** keys such as:

- `IVX_DailyQuiz_CompletedDate_*` — per-quiz completion markers.
- `IVX_DailyQuiz_ConsecutiveStreak` — consecutive **days** streak.
- `IVX_DailyQuiz_LastCompletedDate` — last completion date.

**Time until reset:**

```csharp
using IntelliVerseX.Quiz.DailyQuiz;

TimeSpan remaining = IVXDailyQuizManager.GetTimeUntilDailyReset();
// Bind to a countdown label
```

Use this for “next daily in …” HUD elements. Align your **UTC vs local** policy with backend daily quiz generation.

## Question rotation

Questions arrive from **`IVXDailyQuizService.FetchDailyQuizAsync()`** inside `StartDailyQuiz()`. On success, **`OnDailyQuizLoaded`** fires with **`IVXDailyQuizData`**.

```csharp
var mgr = IVXDailyQuizManager.Instance;
mgr.OnDailyQuizLoaded += data =>
{
    Debug.Log($"{data.SafeTitle} — {data.QuestionCount} questions");
};
mgr.StartDailyQuiz();
```

For **API-backed** rotation, configure the service URL / credentials in your project’s daily quiz settings (see test controller and service implementation).

## Streak tracking

Subscribe to streak-related events:

| Event | Meaning |
|-------|--------|
| `OnDailyStreakBonus` | Bonus granted for streak milestone (int payload). |
| `OnDailyStreakBroken` | Streak reset reason (string). |

Read **`ConsecutiveDaysStreak`** for display. Premium daily mirrors the same pattern with `Premium*` members and separate PlayerPrefs keys.

## UI flow (daily)

1. **Entry** — Button calls `StartDailyQuiz()`; show loading while **`IsDailyLoading`** is true.
2. **Loaded** — `OnDailyQuizLoaded` → populate topic/title; show first question via your UI binding `OnDailyQuestionDisplayed`.
3. **Answer** — User picks option; call the manager’s submit API for daily flow (see `IVXDailyQuizManager` methods for advancing questions).
4. **Complete** — `OnDailyQuizCompleted` provides **`IVXDailyResult`** and **`IVXDailyQuizSummary`** for results screen and rewards.
5. **Cancel** — `OnDailyQuizCancelled` if user backs out.

**Alternate path — session manager only:**

```csharp
using IntelliVerseX.Quiz;

await IVXQuizSessionManager.Instance.StartDailyQuizAsync();
```

Use this when you do **not** need PlayerPrefs streaks or premium split; pair with `IVXQuizQuestionPanel` as in the weekly demo.

## Configuration

1. Import the sample from Package Manager if applicable.
2. Ensure **`IIVXQuizProvider`** or daily **service** endpoints match your environment.
3. Add **`IVXGameConfig`** / bootstrap values your daily service expects.

## Code reference

`IVXDailyQuizTestController` demonstrates:

- Lazy creation of **`IVXDailyQuizManager`**
- Wiring question UI to daily events
- Timer and streak HUD

Prefer copying patterns from that script rather than inventing parallel state.

## See also

- [Quiz API](../api/quiz.md)
- [Quiz module](../modules/quiz.md)
- [Weekly quiz demo](weekly-quiz-demo.md)
