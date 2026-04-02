# Quiz API Reference

The Quiz package provides **session orchestration** (`IVXQuizSessionManager`), **content providers** (`IIVXQuizProvider` and implementations), optional **daily quiz** flows (`IVXDailyQuizManager` in `IntelliVerseX.Quiz.DailyQuiz`), and **QuizUI** components for TMPro-based screens.

## Namespaces

```csharp
using IntelliVerseX.Quiz;
using IntelliVerseX.QuizUI;          // IVXQuizQuestionPanel, IVXQuizResultPanel, IVXHintButton
using IntelliVerseX.Quiz.DailyQuiz;  // IVXDailyQuizManager, daily types
```

---

## `IVXQuizSessionManager`

Singleton (`IVXSafeSingleton`). Drives **custom-id** and **date-based** quiz loading through an injected `IIVXQuizProvider`.

### Configuration

| Method | Parameters | Description |
|--------|------------|-------------|
| `Initialize` | `IIVXQuizProvider provider`, `bool shuffleQuestions = true`, `bool shuffleOptions = true` | Required before starting sessions. |

### Session lifecycle

| Method | Returns | Description |
|--------|---------|-------------|
| `StartSessionAsync` | `Task<bool>` | Loads quiz by **`quizId`** via `FetchQuizAsync(quizId)`, shuffles if enabled, creates `IVXQuizSession`, raises `OnSessionStarted`, shows first question. |
| `StartDailyQuizAsync` | `Task<bool>` | Loads by **`DateTime.UtcNow`** via `FetchQuizAsync(date)` (daily content path). |
| `SubmitAnswer` | `void` | `SubmitAnswer(int answerIndex)` — validates index, updates score, raises `OnAnswerSubmitted`, advances or completes. |
| `SubmitAnswer` | `void` | Overload `(int questionIndex, int answerIndex)` for adapters; warns on index mismatch. |
| `CompleteSessionAsync` | `Task` | `saveToBackend` reserved; completes session locally. |
| `EndSession` | `void` | Clears session without full completion flow. |
| `ResetSession` | `void` | Drops current session reference. |

### Queries

| Member | Type | Description |
|--------|------|-------------|
| `CurrentSession` | `IVXQuizSession` | Active session or null. |
| `IsSessionActive` | `bool` | Session exists and not complete. |
| `GetCurrentQuestion` | `IVXQuizQuestion` | Question for UI. |
| `GetCurrentQuestionNumber` | `int` | 1-based index. |
| `GetTotalQuestions` | `int` | Count from quiz data. |
| `GetCurrentScore` | `int` | Running score. |

### Events

| Event | Signature | When |
|-------|-----------|------|
| `OnSessionStarted` | `Action<IVXQuizSession>` | After successful fetch and session creation. |
| `OnQuestionDisplayed` | `Action<IVXQuizQuestion, int>` | Question and 1-based number. |
| `OnAnswerSubmitted` | `Action<bool, int>` | Correctness and new score. |
| `OnSessionCompleted` | `Action<IVXQuizSession>` | All questions answered. |
| `OnError` | `Action<string>` | Provider missing, fetch failure, etc. |

**Example (custom weekly / pack id):**

```csharp
var sessionMgr = IVXQuizSessionManager.Instance;
sessionMgr.Initialize(new IVXLocalQuizProvider("weekly_pack_12"));
sessionMgr.OnQuestionDisplayed += (q, n) =>
    questionPanel.DisplayQuestion(q, n - 1, sessionMgr.GetTotalQuestions());
sessionMgr.OnAnswerSubmitted += (ok, score) => scoreLabel.text = score.ToString();
await sessionMgr.StartSessionAsync("weekly_pack_12");
```

**Example (daily by UTC date):**

```csharp
await IVXQuizSessionManager.Instance.StartDailyQuizAsync();
```

---

## Quiz modes: daily, weekly, custom

| Mode | How | Typical provider |
|------|-----|------------------|
| **Daily** | `StartDailyQuizAsync()` or `IVXDailyQuizManager` | `IVXS3QuizProvider`, API-backed service, or local JSON keyed by date |
| **Weekly** | `StartSessionAsync("weekly_2026_W14")` or backend id | Same providers with weekly asset ids |
| **Custom** | Any string id | `IVXLocalQuizProvider`, `IVXS3QuizProvider`, `IVXHybridQuizProvider` |

`IIVXQuizProvider` implementations in the SDK include **`IVXS3QuizProvider`**, **`IVXLocalQuizProvider`**, and **`IVXHybridQuizProvider`** (`IVXQuizProvider.cs`).

---

## Question loading

1. Provider **`FetchQuizAsync`** returns `IVXQuizResult` with `QuizData` and questions.
2. Session manager optionally **shuffles** questions/options.
3. **`IVXQuizSession`** tracks index, answers, score, and timing.

Handle `result.Success == false` and surface `ErrorMessage` to the player.

---

## Scoring

- Per-question correctness is determined by **`CorrectAnswerIndex`** vs submitted index.
- **`GetScore()`** / **`GetScorePercentage()`** / **`GetElapsedTime()`** on `IVXQuizSession` support results UI.
- `OnAnswerSubmitted` fires after each question; `OnSessionCompleted` fires when `IsComplete`.

---

## Quiz UI layer (`IVXQuizUIManager` naming)

There is **no** single class named `IVXQuizUIManager`. The **QuizUI** package composes:

| Type | Role |
|------|------|
| `IVXQuizQuestionPanel` | Question text, options, hints; `OnAnswerSelected`, `DisplayQuestion`. |
| `IVXQuizResultPanel` | End-of-quiz summary (wire to session completed). |
| `IVXHintButton` | Optional hint reveal. |
| `IVXQuizUIComponents` | Shared UI helpers |

**Typical flow:**

1. Subscribe `IVXQuizQuestionPanel.OnAnswerSelected` → `IVXQuizSessionManager.SubmitAnswer`.
2. On `OnQuestionDisplayed`, call `DisplayQuestion(question, questionNumber - 1, GetTotalQuestions())`.
3. On `OnSessionCompleted`, show `IVXQuizResultPanel` with score / percentage.

---

## Daily quiz parallel API (`IVXDailyQuizManager`)

For full **streaks**, **premium daily**, and **reset timers**, use `IVXDailyQuizManager` (`StartDailyQuiz`, `GetTimeUntilDailyReset`, PlayerPrefs keys for streaks, etc.). It overlaps conceptually with `StartDailyQuizAsync` on `IVXQuizSessionManager` but adds persistence and events for production daily modes.

---

## Error handling

| Condition | Outcome |
|-----------|---------|
| `Initialize` not called | `StartSessionAsync` returns false; `OnError` invoked. |
| Provider fetch fails | false return; `OnError` with message. |
| Invalid answer index | Logged; no event. |

---

## See also

- [Quiz module](../modules/quiz.md)
- [Quiz demo](../samples/quiz-demo.md)
- [Daily quiz demo](../samples/daily-quiz-demo.md)
- [Weekly quiz demo](../samples/weekly-quiz-demo.md)
