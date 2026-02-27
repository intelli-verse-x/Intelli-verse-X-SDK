# Daily Quiz Demo

This sample demonstrates implementing a daily quiz feature.

## Scene

`IVX_DailyQuiz.unity`

## Features

- Daily quiz with timer
- Multiple choice questions
- Score tracking and rewards
- Streak bonuses

## Setup

1. Import the sample from Package Manager
2. Add `IVXQuizManager` to your scene
3. Configure quiz settings in `IVXGameConfig`

## Code Example

```csharp
using IntelliVerseX.Quiz.DailyQuiz;
using UnityEngine;

public class DailyQuizDemo : MonoBehaviour
{
    async void Start()
    {
        var quiz = await IVXDailyQuizManager.Instance.GetTodayQuizAsync();
        Debug.Log($"Today's quiz has {quiz.Questions.Count} questions");
    }
}
```

## See Also

- [Quiz Module](../modules/quiz.md)
