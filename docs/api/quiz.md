# Quiz API Reference

The Quiz module provides trivia and quiz game functionality.

## Namespace

```csharp
using IntelliVerseX.Quiz;
using IntelliVerseX.Quiz.DailyQuiz;
```

## IVXQuizManager

Main class for quiz operations.

### Methods

| Method | Description |
|--------|-------------|
| `LoadQuestionsAsync()` | Load quiz questions from backend |
| `SubmitAnswerAsync(string questionId, string answer)` | Submit an answer |
| `GetDailyQuizAsync()` | Get today's daily quiz |

### Properties

| Property | Type | Description |
|----------|------|-------------|
| `CurrentQuiz` | `Quiz` | Currently active quiz |
| `IsQuizActive` | `bool` | Whether a quiz is in progress |

### Example

```csharp
// Start daily quiz
var quiz = await IVXQuizManager.Instance.GetDailyQuizAsync();

// Submit answer
var result = await IVXQuizManager.Instance.SubmitAnswerAsync(
    quiz.Questions[0].Id, 
    "Paris"
);
```

## See Also

- [Quiz Module](../modules/quiz.md)
- [Quiz Demo](../samples/quiz-demo.md)
