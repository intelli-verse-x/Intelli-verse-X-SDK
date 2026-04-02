# Weekly Quiz Demo

This sample describes how to run a **weekly** quiz using the same pipeline as custom quiz ids: `IVXQuizSessionManager` + an `IIVXQuizProvider`, QuizUI panels, optional leaderboard submission, and a visible timer.

## Scene

Use the package sample scene if present (e.g. `IVX_WeeklyQuizTest.unity`), or create:

1. A **Canvas** with `IVXQuizQuestionPanel` (question text, four option buttons, optional hint).
2. A **results** area with `IVXQuizResultPanel` or your own summary UI.
3. An empty GameObject **`IVXQuizSessionManager`** is not required—the manager is a singleton; ensure bootstrap runs early.

## Quiz configuration

| Setting | Where | Notes |
|---------|-------|------|
| Weekly quiz id | Backend or JSON manifest | e.g. `weekly_2026_W14` — must match what `FetchQuizAsync(string)` loads. |
| Provider | `IVXLocalQuizProvider`, `IVXS3QuizProvider`, or `IVXHybridQuizProvider` | Hybrid: remote first, local fallback. |
| Shuffle | `Initialize(provider, shuffleQuestions, shuffleOptions)` | Tournaments often shuffle; fixed order for fairness audits. |

**Bootstrap snippet:**

```csharp
using IntelliVerseX.Quiz;

void Awake()
{
    var provider = new IVXLocalQuizProvider("Resources/Quizzes/weekly_current");
    IVXQuizSessionManager.Instance.Initialize(provider, shuffleQuestions: true, shuffleOptions: true);
}
```

## Scoring system

Scoring is handled inside `IVXQuizSession`:

- Each `SubmitAnswer(int answerIndex)` compares to `CorrectAnswerIndex`.
- Read **`GetCurrentScore()`** from `IVXQuizSessionManager` or subscribe to **`OnAnswerSubmitted(bool isCorrect, int score)`**.
- On **`OnSessionCompleted`**, use `IVXQuizSession.GetScore()`, `GetScorePercentage()`, and `GetElapsedTime()` for the results screen.

## Timer

`IVXQuizSession` tracks elapsed time for the session. For a **per-question** cap:

- Start a `Coroutine` or `async` delay when `OnQuestionDisplayed` fires.
- On timeout, call `SubmitAnswer(-1)` only if you extend validation to treat timeout as wrong, or submit a designated “skip” index per game rules.

For **weekly window** (only playable Mon–Sun), enforce dates in your provider or server when resolving the weekly id.

## UI flow

1. **Load** — Show spinner; `await StartSessionAsync("weekly_id")`.
2. **Play** — `OnQuestionDisplayed` → `IVXQuizQuestionPanel.DisplayQuestion(question, questionNumber - 1, totalQuestions)` (panel uses 0-based index internally).
3. **Answer** — `OnAnswerSelected` → `IVXQuizSessionManager.Instance.SubmitAnswer(index)`.
4. **Feedback** — Optional: use panel’s correct/incorrect coloring (see `IVXQuizQuestionPanel` feedback colors).
5. **Complete** — `OnSessionCompleted` → show score and CTA (retry, home, share).

**Wire-up example:**

```csharp
using IntelliVerseX.Quiz;
using IntelliVerseX.QuizUI;

[SerializeField] IVXQuizQuestionPanel questionPanel;

void OnEnable()
{
    var m = IVXQuizSessionManager.Instance;
    m.OnQuestionDisplayed += OnQuestion;
    m.OnSessionCompleted += OnComplete;
    if (questionPanel != null)
        questionPanel.OnAnswerSelected += idx => m.SubmitAnswer(idx);
}

void OnQuestion(IVXQuizQuestion q, int number)
{
    int total = IVXQuizSessionManager.Instance.GetTotalQuestions();
    questionPanel.DisplayQuestion(q, number - 1, total);
}

void OnComplete(IVXQuizSession session)
{
    Debug.Log($"Weekly done: {session.GetScore()}/{session.QuizData.questions.Count}");
}
```

## Leaderboards

After completion, submit to your weekly board via **`IVXGLeaderboardManager.SubmitScoreAsync`** with metadata `{ "week": "2026-W14" }` so server-side filters stay consistent.

## Features checklist

- Weekly quiz competitions tied to a stable id per week.
- Global / game leaderboards via existing RPC pipeline.
- Optional prize distribution (your economy / mail RPCs).
- Tournament brackets are game-specific UI; SDK supplies score and completion events.

## See also

- [Quiz API](../api/quiz.md)
- [Quiz module](../modules/quiz.md)
- [Daily quiz demo](daily-quiz-demo.md)
- [Leaderboard demo](leaderboard-demo.md)
