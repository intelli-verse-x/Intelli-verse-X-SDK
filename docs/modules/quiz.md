# Quiz Module

The Quiz module provides daily and weekly knowledge challenges with questions fetched from the Nakama backend.

---

## Overview

| | |
|---|---|
| **Namespace** | `IntelliVerseX.Quiz` |
| **Assembly** | `IntelliVerseX.Quiz` |
| **UI Assembly** | `IntelliVerseX.QuizUI` |

---

## Features

- **Daily Quiz** - 10 questions per day, resets at midnight UTC
- **Weekly Quiz** - Larger quiz with weekly leaderboard
- **Question Categories** - Multiple choice, true/false, image-based
- **Progress Tracking** - Resume incomplete quizzes
- **Rewards** - Coins and XP for completion

---

## Key Classes

| Class | Purpose |
|-------|---------|
| `IVXDailyQuizManager` | Daily quiz logic |
| `IVXWeeklyQuizManager` | Weekly quiz logic |
| `IVXQuizQuestion` | Question data model |
| `IVXQuizResult` | Quiz completion result |

---

## Daily Quiz

### IVXDailyQuizManager

```csharp
public static class IVXDailyQuizManager
{
    // State
    public static bool IsAvailable { get; }
    public static bool IsCompleted { get; }
    public static int QuestionsAnswered { get; }
    public static int TotalQuestions { get; }
    public static DateTime ResetTime { get; }
    
    // Events
    public static event Action<IVXQuizQuestion> OnQuestionLoaded;
    public static event Action<bool, int> OnAnswerSubmitted; // correct, coinsEarned
    public static event Action<IVXQuizResult> OnQuizComplete;
    
    // Start/Resume quiz
    public static async Task<IVXQuizQuestion> StartQuizAsync();
    
    // Submit answer
    public static async Task<AnswerResult> SubmitAnswerAsync(int answerIndex);
    public static async Task<AnswerResult> SubmitAnswerAsync(string answerId);
    
    // Get next question
    public static async Task<IVXQuizQuestion> GetNextQuestionAsync();
    
    // Get results
    public static async Task<IVXQuizResult> GetResultsAsync();
    
    // Skip (with cost)
    public static async Task SkipQuestionAsync();
}
```

### Usage

```csharp
using IntelliVerseX.Quiz;

public class DailyQuizController : MonoBehaviour
{
    async void Start()
    {
        // Subscribe to events
        IVXDailyQuizManager.OnQuestionLoaded += DisplayQuestion;
        IVXDailyQuizManager.OnAnswerSubmitted += HandleAnswer;
        IVXDailyQuizManager.OnQuizComplete += ShowResults;
        
        // Check availability
        if (IVXDailyQuizManager.IsCompleted)
        {
            ShowCompletedState();
            return;
        }
        
        // Start or resume quiz
        var firstQuestion = await IVXDailyQuizManager.StartQuizAsync();
        DisplayQuestion(firstQuestion);
    }
    
    void DisplayQuestion(IVXQuizQuestion question)
    {
        questionText.text = question.Text;
        
        for (int i = 0; i < question.Answers.Length; i++)
        {
            answerButtons[i].SetAnswer(i, question.Answers[i]);
        }
        
        if (question.HasImage)
        {
            questionImage.texture = await LoadImage(question.ImageUrl);
        }
        
        progressText.text = $"{IVXDailyQuizManager.QuestionsAnswered + 1}/{IVXDailyQuizManager.TotalQuestions}";
    }
    
    async void OnAnswerSelected(int index)
    {
        var result = await IVXDailyQuizManager.SubmitAnswerAsync(index);
        
        if (result.IsCorrect)
        {
            ShowCorrectFeedback();
            AddCoins(result.CoinsEarned);
        }
        else
        {
            ShowIncorrectFeedback(result.CorrectAnswerIndex);
        }
        
        // Load next or finish
        await Task.Delay(1500); // Show feedback
        
        var nextQuestion = await IVXDailyQuizManager.GetNextQuestionAsync();
        if (nextQuestion != null)
        {
            DisplayQuestion(nextQuestion);
        }
        // OnQuizComplete event will fire when done
    }
    
    void ShowResults(IVXQuizResult result)
    {
        resultsPanel.Show();
        correctCountText.text = $"{result.CorrectAnswers}/{result.TotalQuestions}";
        coinsEarnedText.text = $"+{result.TotalCoinsEarned}";
        accuracyText.text = $"{result.Accuracy:P0}";
    }
}
```

---

## Weekly Quiz

### IVXWeeklyQuizManager

Similar API to daily quiz with longer format:

```csharp
public static class IVXWeeklyQuizManager
{
    public static bool IsAvailable { get; }
    public static bool IsCompleted { get; }
    public static int QuestionsAnswered { get; }
    public static int TotalQuestions { get; } // Usually 50
    public static DateTime WeekStartTime { get; }
    public static DateTime WeekEndTime { get; }
    
    // Same methods as DailyQuizManager
    public static async Task<IVXQuizQuestion> StartQuizAsync();
    public static async Task<AnswerResult> SubmitAnswerAsync(int answerIndex);
    public static async Task<IVXQuizQuestion> GetNextQuestionAsync();
    public static async Task<IVXQuizResult> GetResultsAsync();
    
    // Leaderboard
    public static async Task<LeaderboardEntry[]> GetLeaderboardAsync(int limit = 50);
}
```

---

## Data Models

### IVXQuizQuestion

```csharp
public class IVXQuizQuestion
{
    public string Id { get; }
    public string Text { get; }
    public QuestionType Type { get; }
    
    public IVXQuizAnswer[] Answers { get; }
    
    public bool HasImage { get; }
    public string ImageUrl { get; }
    
    public string Category { get; }
    public string Difficulty { get; } // easy, medium, hard
    
    public int TimeLimit { get; } // seconds, 0 = no limit
    public int PointValue { get; }
}

public class IVXQuizAnswer
{
    public string Id { get; }
    public string Text { get; }
    public string ImageUrl { get; }
}

public enum QuestionType
{
    MultipleChoice,
    TrueFalse,
    ImageChoice
}
```

### IVXQuizResult

```csharp
public class IVXQuizResult
{
    public int CorrectAnswers { get; }
    public int TotalQuestions { get; }
    public float Accuracy { get; }
    
    public int TotalCoinsEarned { get; }
    public int XPEarned { get; }
    
    public TimeSpan TotalTime { get; }
    public float AverageTimePerQuestion { get; }
    
    public int Rank { get; } // Weekly quiz only
    public bool IsPersonalBest { get; }
    
    public string CompletionId { get; }
}
```

### AnswerResult

```csharp
public class AnswerResult
{
    public bool IsCorrect { get; }
    public int CorrectAnswerIndex { get; }
    public string CorrectAnswerId { get; }
    
    public int CoinsEarned { get; }
    public int CurrentStreak { get; }
    
    public string Explanation { get; } // Optional explanation
}
```

---

## Question Categories

| Category | Description |
|----------|-------------|
| `General` | General knowledge |
| `Science` | Science and nature |
| `History` | Historical events |
| `Geography` | World geography |
| `Sports` | Sports trivia |
| `Entertainment` | Movies, music, TV |
| `Technology` | Tech and computing |
| `Art` | Art and literature |

---

## Rewards System

### Daily Quiz Rewards

| Correct Answers | Coins | XP |
|-----------------|-------|-----|
| 1-3 | 10 | 5 |
| 4-6 | 25 | 15 |
| 7-9 | 50 | 30 |
| 10 (Perfect) | 100 | 50 |

### Weekly Quiz Rewards

| Rank | Coins | XP |
|------|-------|-----|
| 1st | 1000 | 500 |
| 2nd-3rd | 500 | 250 |
| 4th-10th | 250 | 125 |
| 11th-50th | 100 | 50 |
| Participated | 50 | 25 |

---

## Timer Implementation

For timed questions:

```csharp
public class QuizTimerUI : MonoBehaviour
{
    [SerializeField] private Slider timerSlider;
    [SerializeField] private TMP_Text timerText;
    
    private float _remainingTime;
    private bool _isRunning;
    
    public void StartTimer(int seconds)
    {
        _remainingTime = seconds;
        timerSlider.maxValue = seconds;
        _isRunning = true;
    }
    
    void Update()
    {
        if (!_isRunning) return;
        
        _remainingTime -= Time.deltaTime;
        timerSlider.value = _remainingTime;
        timerText.text = Mathf.CeilToInt(_remainingTime).ToString();
        
        if (_remainingTime <= 0)
        {
            _isRunning = false;
            OnTimeExpired?.Invoke();
        }
    }
    
    public event Action OnTimeExpired;
}
```

---

## Quiz UI Prefabs

The SDK includes ready-to-use UI prefabs:

| Prefab | Description |
|--------|-------------|
| `IVXDailyQuizPanel` | Complete daily quiz UI |
| `IVXWeeklyQuizPanel` | Complete weekly quiz UI |
| `IVXQuizQuestionCard` | Question display component |
| `IVXQuizAnswerButton` | Answer button component |
| `IVXQuizResultsPanel` | Results summary display |
| `IVXQuizLeaderboardPanel` | Weekly leaderboard UI |

---

## Best Practices

### 1. Handle Disconnection

```csharp
// Quiz state is saved server-side
// Player can resume after reconnection

async void OnReconnected()
{
    if (IVXDailyQuizManager.QuestionsAnswered > 0 && 
        !IVXDailyQuizManager.IsCompleted)
    {
        ShowResumeDialog();
    }
}
```

### 2. Prevent Answer Spoofing

```csharp
// Answers are validated server-side
// Client receives correct answer only after submission
// Cannot cheat by inspecting question data
```

### 3. Smooth Transitions

```csharp
async void ShowNextQuestion()
{
    // Animate out
    await questionCard.AnimateOut();
    
    // Load next
    var next = await IVXDailyQuizManager.GetNextQuestionAsync();
    
    // Animate in
    await questionCard.AnimateIn(next);
}
```

---

## Testing

### Test Questions

In development, use test questions:

```csharp
#if UNITY_EDITOR
// Backend returns test questions in development
// Real questions only in production builds
#endif
```

### Force Reset (Debug)

```csharp
#if UNITY_EDITOR
[ContextMenu("Reset Daily Quiz")]
void DebugResetQuiz()
{
    // Call debug RPC to reset quiz state
    IVXNakamaManager.RpcAsync("quiz/debug_reset", new { type = "daily" });
}
#endif
```

---

## Related Documentation

- [Daily Quiz Demo](../samples/daily-quiz-demo.md) - Sample implementation
- [Weekly Quiz Demo](../samples/weekly-quiz-demo.md) - Tournament quiz
