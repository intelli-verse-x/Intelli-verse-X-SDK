# Quiz Demo

Sample scene demonstrating Daily and Weekly quiz functionality.

---

## Scene Overview

**Locations:** 
- `Assets/_IntelliVerseXSDK/Samples/Scenes/IVX_DailyQuiz.unity`
- `Assets/_IntelliVerseXSDK/Samples/Scenes/IVX_WeeklyQuizTest.unity`

This sample demonstrates:

- Loading quiz questions
- Answering questions
- Timer functionality
- Score calculation
- Reward claiming

---

## Scene Hierarchy

```
Canvas
├── QuizInfoPanel
│   ├── QuizTitle
│   ├── QuestionCount
│   ├── TimeLimit
│   └── StartButton
├── QuestionPanel
│   ├── QuestionNumber
│   ├── QuestionText
│   ├── AnswerButtons (4x)
│   ├── TimerDisplay
│   └── ProgressBar
├── ResultPanel
│   ├── ScoreText
│   ├── CorrectAnswers
│   ├── RewardDisplay
│   └── ClaimRewardButton
└── LoadingOverlay
```

---

## Key Components

### QuizDemoController.cs

```csharp
using IntelliVerseX.Quiz;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class QuizDemoController : MonoBehaviour
{
    [Header("Panels")]
    [SerializeField] private GameObject _infoPanel;
    [SerializeField] private GameObject _questionPanel;
    [SerializeField] private GameObject _resultPanel;
    
    [Header("Info Panel")]
    [SerializeField] private TMP_Text _quizTitle;
    [SerializeField] private TMP_Text _questionCountText;
    [SerializeField] private Button _startButton;
    
    [Header("Question Panel")]
    [SerializeField] private TMP_Text _questionNumberText;
    [SerializeField] private TMP_Text _questionText;
    [SerializeField] private Button[] _answerButtons;
    [SerializeField] private TMP_Text _timerText;
    [SerializeField] private Slider _progressBar;
    
    [Header("Result Panel")]
    [SerializeField] private TMP_Text _scoreText;
    [SerializeField] private TMP_Text _correctAnswersText;
    [SerializeField] private TMP_Text _rewardText;
    [SerializeField] private Button _claimButton;
    
    private QuizSession _currentSession;
    private int _currentQuestionIndex;
    private float _timeRemaining;
    private bool _isTimerRunning;
    
    async void Start()
    {
        await LoadQuizInfo();
    }
    
    async System.Threading.Tasks.Task LoadQuizInfo()
    {
        var quizInfo = await IVXDailyQuizManager.Instance.GetQuizInfoAsync();
        
        _quizTitle.text = "Daily Quiz";
        _questionCountText.text = $"{quizInfo.QuestionCount} Questions";
        
        _startButton.interactable = !quizInfo.AlreadyCompleted;
        _startButton.GetComponentInChildren<TMP_Text>().text = 
            quizInfo.AlreadyCompleted ? "Completed Today" : "Start Quiz";
        
        ShowPanel(_infoPanel);
    }
    
    public async void StartQuiz()
    {
        _currentSession = await IVXDailyQuizManager.Instance.StartQuizAsync();
        _currentQuestionIndex = 0;
        
        ShowNextQuestion();
    }
    
    void ShowNextQuestion()
    {
        if (_currentQuestionIndex >= _currentSession.Questions.Count)
        {
            ShowResults();
            return;
        }
        
        var question = _currentSession.Questions[_currentQuestionIndex];
        
        _questionNumberText.text = $"Question {_currentQuestionIndex + 1}/{_currentSession.Questions.Count}";
        _questionText.text = question.Text;
        
        // Setup answer buttons
        for (int i = 0; i < _answerButtons.Length; i++)
        {
            if (i < question.Options.Count)
            {
                _answerButtons[i].gameObject.SetActive(true);
                _answerButtons[i].GetComponentInChildren<TMP_Text>().text = question.Options[i];
                
                int index = i;
                _answerButtons[i].onClick.RemoveAllListeners();
                _answerButtons[i].onClick.AddListener(() => SelectAnswer(index));
            }
            else
            {
                _answerButtons[i].gameObject.SetActive(false);
            }
        }
        
        // Start timer
        _timeRemaining = question.TimeLimit;
        _isTimerRunning = true;
        
        // Update progress
        _progressBar.value = (float)_currentQuestionIndex / _currentSession.Questions.Count;
        
        ShowPanel(_questionPanel);
    }
    
    void Update()
    {
        if (_isTimerRunning)
        {
            _timeRemaining -= Time.deltaTime;
            _timerText.text = Mathf.CeilToInt(_timeRemaining).ToString();
            
            if (_timeRemaining <= 0)
            {
                TimeUp();
            }
        }
    }
    
    async void SelectAnswer(int answerIndex)
    {
        _isTimerRunning = false;
        
        // Submit answer
        await IVXDailyQuizManager.Instance.SubmitAnswerAsync(
            _currentSession.SessionId,
            _currentQuestionIndex,
            answerIndex
        );
        
        // Visual feedback
        var question = _currentSession.Questions[_currentQuestionIndex];
        bool isCorrect = answerIndex == question.CorrectIndex;
        
        // Highlight correct/incorrect
        _answerButtons[answerIndex].image.color = isCorrect ? Color.green : Color.red;
        if (!isCorrect)
        {
            _answerButtons[question.CorrectIndex].image.color = Color.green;
        }
        
        // Wait and continue
        await System.Threading.Tasks.Task.Delay(1500);
        
        _currentQuestionIndex++;
        ResetButtonColors();
        ShowNextQuestion();
    }
    
    void TimeUp()
    {
        _isTimerRunning = false;
        
        // Auto-skip with no answer
        _currentQuestionIndex++;
        ShowNextQuestion();
    }
    
    async void ShowResults()
    {
        var results = await IVXDailyQuizManager.Instance.GetResultsAsync(_currentSession.SessionId);
        
        _scoreText.text = $"Score: {results.Score}";
        _correctAnswersText.text = $"Correct: {results.CorrectCount}/{results.TotalQuestions}";
        _rewardText.text = $"Reward: {results.RewardAmount} coins";
        
        _claimButton.interactable = !results.RewardClaimed;
        
        ShowPanel(_resultPanel);
    }
    
    public async void ClaimReward()
    {
        await IVXDailyQuizManager.Instance.ClaimRewardAsync(_currentSession.SessionId);
        _claimButton.interactable = false;
        _claimButton.GetComponentInChildren<TMP_Text>().text = "Claimed!";
    }
    
    void ShowPanel(GameObject panel)
    {
        _infoPanel.SetActive(panel == _infoPanel);
        _questionPanel.SetActive(panel == _questionPanel);
        _resultPanel.SetActive(panel == _resultPanel);
    }
    
    void ResetButtonColors()
    {
        foreach (var btn in _answerButtons)
        {
            btn.image.color = Color.white;
        }
    }
}
```

---

## How to Use

### Running the Sample

1. Open `IVX_DailyQuiz.unity` or `IVX_WeeklyQuizTest.unity`
2. Ensure authenticated
3. Press **Play**

### Daily Quiz Flow

1. View quiz info (question count, status)
2. Click **"Start Quiz"**
3. Answer questions within time limit
4. View results
5. Claim rewards

### Weekly Quiz

Same flow, but resets weekly and often includes:
- More questions
- Higher rewards
- Leaderboard ranking

---

## Quiz Types

### Daily Quiz

```csharp
// Check daily quiz availability
var info = await IVXDailyQuizManager.Instance.GetQuizInfoAsync();

if (!info.AlreadyCompleted)
{
    var session = await IVXDailyQuizManager.Instance.StartQuizAsync();
}
```

### Weekly Quiz

```csharp
// Check weekly quiz availability
var info = await IVXWeeklyQuizManager.Instance.GetQuizInfoAsync();

if (info.IsActive && !info.AlreadyCompleted)
{
    var session = await IVXWeeklyQuizManager.Instance.StartQuizAsync();
}
```

---

## Customization

### Custom Timer

```csharp
[SerializeField] private float _warningThreshold = 5f;
[SerializeField] private Color _warningColor = Color.red;

void UpdateTimer()
{
    _timerText.text = Mathf.CeilToInt(_timeRemaining).ToString();
    
    if (_timeRemaining <= _warningThreshold)
    {
        _timerText.color = _warningColor;
        // Add pulse animation
    }
}
```

### Answer Feedback

```csharp
IEnumerator ShowAnswerFeedback(int selectedIndex, int correctIndex)
{
    bool isCorrect = selectedIndex == correctIndex;
    
    // Highlight selection
    _answerButtons[selectedIndex].image.color = isCorrect ? Color.green : Color.red;
    
    // Show correct if wrong
    if (!isCorrect)
    {
        yield return new WaitForSeconds(0.5f);
        _answerButtons[correctIndex].image.color = Color.green;
    }
    
    // Play sound
    AudioManager.Play(isCorrect ? "correct" : "wrong");
    
    yield return new WaitForSeconds(1f);
}
```

---

## Server Integration

Quiz questions come from your Nakama backend:

```typescript
// Server-side RPC
function rpcGetDailyQuiz(ctx: nkruntime.Context): string {
    const userId = ctx.userId;
    
    // Check if already completed today
    const lastPlayed = getLastQuizDate(userId);
    if (isToday(lastPlayed)) {
        return JSON.stringify({ error: "Already completed" });
    }
    
    // Get questions
    const questions = selectRandomQuestions(5);
    
    return JSON.stringify({ questions });
}
```

---

## See Also

- [Quiz Module](../modules/quiz.md)
- [Nakama Integration Guide](../guides/nakama-integration.md)
