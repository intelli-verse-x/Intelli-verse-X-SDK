# Skill: Quiz Content Pipeline

**Skill ID:** `ivx-quiz-content`

Builds an automated quiz content system using S3-hosted JSON files, LLM-based content generation, and CI/CD pipelines. Supports daily, weekly, and category-based quizzes with offline fallback.

---

## When to Use

Ask your AI agent any of these:

- "Set up a daily quiz system with S3"
- "Generate trivia questions using GPT-4o and upload to S3"
- "Create a GitHub Action that generates fresh quiz content daily"
- "Add offline fallback for quiz content"
- "Set up weekly themed quizzes"
- "Add a custom quiz provider backed by my own database"

---

## What the Agent Does

```mermaid
flowchart LR
    A[You: "Set up daily quiz"] --> B[Agent loads ivx-quiz-content skill]
    B --> C[Designs S3 bucket structure]
    C --> D[Defines JSON schemas]
    D --> E[Configures IVXS3QuizProvider]
    E --> F[Sets up hybrid fallback]
    F --> G[Creates LLM generation script]
    G --> H[Writes GitHub Action for CI/CD]
```

---

## S3 Bucket Structure

```
s3://{bucket}/quiz-verse/{game_id}/
├── daily/
│   ├── 2026-04-01.json
│   ├── 2026-04-02.json
│   └── ...
├── weekly/
│   ├── 2026-W14-prediction.json
│   ├── 2026-W14-fortune.json
│   └── ...
└── categories/
    ├── science.json
    ├── history.json
    └── geography.json
```

| Content | Path Pattern | Rotation |
|---------|-------------|----------|
| Daily quiz | `daily/{YYYY-MM-DD}.json` | New file each day |
| Weekly quiz | `weekly/{YYYY}-W{WW}-{mode}.json` | New file each week |
| Category bank | `categories/{category}.json` | Updated periodically |

---

## JSON Schema

### Daily Quiz

```json
{
  "date": "2026-04-02",
  "game_id": "your-game-id",
  "version": 1,
  "questions": [
    {
      "id": "q_001",
      "question": "What is the chemical symbol for gold?",
      "options": ["Au", "Ag", "Fe", "Cu"],
      "correct_answer": 0,
      "category": "science",
      "difficulty": "medium",
      "explanation": "Au comes from the Latin 'aurum'.",
      "media_url": null,
      "time_limit_sec": 15
    }
  ],
  "metadata": {
    "total_questions": 10,
    "estimated_duration_min": 5,
    "theme": "Science Spectacular"
  }
}
```

### Weekly Quiz

Same structure, plus `week`, `mode`, `result_type`, `free_result`, `premium_result` fields.

---

## Quiz Providers

| Provider | Source | Offline | Freshness |
|----------|--------|---------|-----------|
| `IVXS3QuizProvider` | S3 via HTTPS | No | Always current |
| `IVXLocalQuizProvider` | Bundled JSON in StreamingAssets | Yes | Stale after build |
| `IVXHybridQuizProvider` | S3 with local fallback | Yes | Best of both |

### Configure S3 Provider

```csharp
var s3Provider = new IVXS3QuizProvider(new S3QuizConfig {
    BaseUrl = "https://your-bucket.s3.amazonaws.com/quiz-verse/your-game-id",
    CacheDurationMinutes = 60,
    TimeoutSeconds = 10,
});
IVXQuizManager.Instance.SetProvider(s3Provider);
```

### Hybrid Provider (Recommended)

```csharp
var hybrid = new IVXHybridQuizProvider(
    primary: new IVXS3QuizProvider(s3Config),
    fallback: new IVXLocalQuizProvider()
);
IVXQuizManager.Instance.SetProvider(hybrid);
```

---

## Fetching Quizzes

```csharp
// Daily quiz for today
QuizData daily = await IVXQuizManager.Instance.GetDailyQuizAsync();

// Weekly prediction quiz
QuizData weekly = await IVXQuizManager.Instance.GetWeeklyQuizAsync("prediction");

// Random science questions
QuizData science = await IVXQuizManager.Instance.GetCategoryQuizAsync("science", count: 10);
```

---

## Content Generation

### Python Script

```python
import json, datetime, boto3
from openai import OpenAI

client = OpenAI()
s3 = boto3.client("s3")
DATE = datetime.date.today().isoformat()

prompt = f"""Generate 10 trivia questions for {DATE}.
Return JSON with: question, options (4), correct_answer (0-3),
category, difficulty, explanation, time_limit_sec (10-30)."""

response = client.chat.completions.create(
    model="gpt-4o",
    messages=[{"role": "user", "content": prompt}],
    response_format={"type": "json_object"},
)
questions = json.loads(response.choices[0].message.content)["questions"]

quiz = {
    "date": DATE,
    "game_id": "your-game-id",
    "version": 1,
    "questions": questions,
    "metadata": {"total_questions": len(questions), "theme": f"Daily — {DATE}"},
}
s3.put_object(
    Bucket="your-bucket",
    Key=f"quiz-verse/your-game-id/daily/{DATE}.json",
    Body=json.dumps(quiz, indent=2),
    ContentType="application/json",
)
```

### GitHub Action (Daily at 2 AM UTC)

```yaml
name: Daily Quiz Generation
on:
  schedule:
    - cron: "0 2 * * *"
  workflow_dispatch: {}
jobs:
  generate:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4
      - uses: actions/setup-python@v5
        with: { python-version: "3.12" }
      - run: pip install openai boto3
      - run: python tools/generate_daily_quiz.py
        env:
          OPENAI_API_KEY: ${{ secrets.OPENAI_API_KEY }}
          AWS_ACCESS_KEY_ID: ${{ secrets.AWS_ACCESS_KEY_ID }}
          AWS_SECRET_ACCESS_KEY: ${{ secrets.AWS_SECRET_ACCESS_KEY }}
```

---

## Custom Quiz Provider

Extend `IIVXQuizProvider` for any custom source:

```csharp
public class MyDatabaseQuizProvider : IIVXQuizProvider
{
    public bool IsAvailable => true;

    public async Task<QuizData> FetchDailyQuizAsync(DateTime date)
    {
        var questions = await MyDB.GetQuestionsAsync(date, count: 10);
        return new QuizData { Date = date.ToString("yyyy-MM-dd"), Questions = questions };
    }

    public async Task<QuizData> FetchWeeklyQuizAsync(string weekId, string mode) { ... }
    public async Task<QuizData> FetchCategoryQuizAsync(string category, int count) { ... }
}

IVXQuizManager.Instance.SetProvider(new MyDatabaseQuizProvider());
```

---

## Caching

| Level | Duration | Location |
|-------|----------|----------|
| Memory | `CacheDurationMinutes` (default 60) | RAM |
| Disk | 24 hours | `Application.persistentDataPath/quiz_cache/` |

Disk cache ensures quizzes remain available when the app is backgrounded.

---

## Completion Checklist

- [ ] S3 bucket created with correct folder structure
- [ ] Quiz JSON files validate against the schema
- [ ] `IVXS3QuizProvider` configured with correct base URL
- [ ] Daily quiz fetches and displays correctly
- [ ] Weekly quiz modes fetch correctly
- [ ] Fallback to local content works when offline
- [ ] CI/CD pipeline generates and uploads daily quizzes
- [ ] Content generation produces valid, diverse questions
