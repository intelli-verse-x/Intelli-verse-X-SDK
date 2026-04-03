# Quiz Content Pipeline

**Skill ID:** `ivx-quiz-content`

---
name: ivx-quiz-content
description: >-
  Set up quiz content pipelines for IntelliVerseX SDK games including S3-hosted
  daily/weekly quizzes and content generation. Use when the user says "add quiz",
  "set up daily quiz", "weekly quiz", "quiz content", "S3 quiz", "generate trivia",
  "quiz pipeline", "custom quiz type",   or needs help with quiz content management.
version: "1.0.0"
author: "IntelliVerse-X <team@intelli-verse-x.ai>"
allowed-tools:
  - Read
  - Write
  - Edit
  - Glob
  - Grep
  - Shell
---



## Overview

The SDK provides a pluggable quiz content system that can source questions from S3-hosted JSON files, local bundles, or a hybrid of both. A CI/CD pipeline generates fresh content daily using LLM APIs and uploads it to S3.

---

## 1. S3 Bucket Structure

```
s3://{bucket}/quiz-verse/{game_id}/
├── daily/
│   ├── 2026-04-01.json
│   ├── 2026-04-02.json
│   └── ...
├── weekly/
│   ├── 2026-W14-prediction.json
│   ├── 2026-W14-fortune.json
│   ├── 2026-W14-emoji.json
│   ├── 2026-W14-health.json
│   └── 2026-W14-compatibility.json
└── categories/
    ├── science.json
    ├── history.json
    └── geography.json
```

### Naming Conventions

| Content | Path Pattern | Rotation |
|---------|-------------|----------|
| Daily quiz | `daily/{YYYY-MM-DD}.json` | New file each day |
| Weekly quiz | `weekly/{YYYY}-W{WW}-{mode}.json` | New file each week |
| Category bank | `categories/{category}.json` | Updated periodically |

---

## 2. JSON Schemas

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

Same structure with additional fields:

```json
{
  "week": "2026-W14",
  "mode": "prediction",
  "game_id": "your-game-id",
  "version": 1,
  "questions": [ ... ],
  "result_type": "card",
  "free_result": { "title": "Your Prediction Style", "description": "..." },
  "premium_result": { "title": "Deep Analysis", "description": "..." }
}
```

---

## 3. Quiz Providers

### IIVXQuizProvider Interface

```csharp
namespace IntelliVerseX.Quiz
{
    public interface IIVXQuizProvider
    {
        Task<QuizData> FetchDailyQuizAsync(DateTime date);
        Task<QuizData> FetchWeeklyQuizAsync(string weekId, string mode);
        Task<QuizData> FetchCategoryQuizAsync(string category, int count);
        bool IsAvailable { get; }
    }
}
```

### Built-In Providers

| Provider | Source | Offline | Freshness |
|----------|--------|---------|-----------|
| `IVXS3QuizProvider` | S3 bucket via HTTPS | No | Always current |
| `IVXLocalQuizProvider` | Bundled JSON in StreamingAssets | Yes | Stale after build |
| `IVXHybridQuizProvider` | S3 with local fallback | Yes | Best of both |

### Configuring the S3 Provider

```csharp
var s3Provider = new IVXS3QuizProvider(new S3QuizConfig
{
    BaseUrl = "https://your-bucket.s3.amazonaws.com/quiz-verse/your-game-id",
    CacheDurationMinutes = 60,
    TimeoutSeconds = 10,
});

IVXQuizManager.Instance.SetProvider(s3Provider);
```

### Using the Hybrid Provider

```csharp
var hybrid = new IVXHybridQuizProvider(
    primary: new IVXS3QuizProvider(s3Config),
    fallback: new IVXLocalQuizProvider()
);

IVXQuizManager.Instance.SetProvider(hybrid);
```

The hybrid provider tries S3 first. On network failure, it falls back to local content seamlessly.

---

## 4. Fetching Quizzes

```csharp
using IntelliVerseX.Quiz;

// Daily quiz for today
QuizData daily = await IVXQuizManager.Instance.GetDailyQuizAsync();

// Weekly prediction quiz
QuizData weekly = await IVXQuizManager.Instance.GetWeeklyQuizAsync("prediction");

// Random science questions
QuizData science = await IVXQuizManager.Instance.GetCategoryQuizAsync("science", count: 10);
```

---

## 5. Content Generation with LLM

### Python Script for Daily Generation

```python
import json
import datetime
import boto3
from openai import OpenAI

client = OpenAI()
s3 = boto3.client("s3")

GAME_ID = "your-game-id"
BUCKET = "your-quiz-bucket"
DATE = datetime.date.today().isoformat()

prompt = f"""Generate 10 trivia questions for a daily quiz dated {DATE}.
Return valid JSON matching this schema:
- question (string)
- options (array of 4 strings)
- correct_answer (index 0-3)
- category (string)
- difficulty (easy/medium/hard)
- explanation (string)
- time_limit_sec (integer, 10-30)
Mix categories. Medium difficulty average."""

response = client.chat.completions.create(
    model="gpt-4o",
    messages=[{"role": "user", "content": prompt}],
    response_format={"type": "json_object"},
)

questions = json.loads(response.choices[0].message.content)["questions"]

quiz = {
    "date": DATE,
    "game_id": GAME_ID,
    "version": 1,
    "questions": questions,
    "metadata": {
        "total_questions": len(questions),
        "estimated_duration_min": 5,
        "theme": f"Daily Challenge — {DATE}",
    },
}

s3.put_object(
    Bucket=BUCKET,
    Key=f"quiz-verse/{GAME_ID}/daily/{DATE}.json",
    Body=json.dumps(quiz, indent=2),
    ContentType="application/json",
)
```

### CI/CD: GitHub Action

```yaml
name: Daily Quiz Generation
on:
  schedule:
    - cron: "0 2 * * *"   # 2 AM UTC daily
  workflow_dispatch: {}

jobs:
  generate:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4
      - uses: actions/setup-python@v5
        with:
          python-version: "3.12"
      - run: pip install openai boto3
      - run: python tools/generate_daily_quiz.py
        env:
          OPENAI_API_KEY: ${{ secrets.OPENAI_API_KEY }}
          AWS_ACCESS_KEY_ID: ${{ secrets.AWS_ACCESS_KEY_ID }}
          AWS_SECRET_ACCESS_KEY: ${{ secrets.AWS_SECRET_ACCESS_KEY }}
          AWS_REGION: us-east-1
```

---

## 6. Custom Quiz Types

Extend `IIVXQuizProvider` for custom content sources:

```csharp
public class MyDatabaseQuizProvider : IIVXQuizProvider
{
    public bool IsAvailable => true;

    public async Task<QuizData> FetchDailyQuizAsync(DateTime date)
    {
        var questions = await MyDatabaseClient.GetQuestionsAsync(date, count: 10);
        return new QuizData
        {
            Date = date.ToString("yyyy-MM-dd"),
            Questions = questions.Select(q => q.ToQuizQuestion()).ToList(),
        };
    }

    public async Task<QuizData> FetchWeeklyQuizAsync(string weekId, string mode)
    {
        // Custom weekly quiz logic
    }

    public async Task<QuizData> FetchCategoryQuizAsync(string category, int count)
    {
        // Custom category quiz logic
    }
}

// Register
IVXQuizManager.Instance.SetProvider(new MyDatabaseQuizProvider());
```

---

## 7. Caching

The S3 provider caches fetched quizzes in memory and optionally on disk:

| Cache Level | Duration | Location |
|-------------|----------|----------|
| Memory | `CacheDurationMinutes` (default 60) | RAM |
| Disk | 24 hours | `Application.persistentDataPath/quiz_cache/` |

The disk cache ensures quizzes remain available if the app is backgrounded and the memory cache is cleared.

---

## Platform-Specific Quiz Considerations

### VR Platforms

| Consideration | Guidance |
|--------------|----------|
| **Quiz UI** | Display questions on a world-space panel (curved or flat). Use `IVXXRPlatformHelper.GetRecommendedSettings().UIScale` for text sizing. Minimum 24pt equivalent for VR readability. |
| **Input** | Use gaze + pinch or controller ray for answer selection via `IVXXRInputAdapter`. Avoid tiny touch targets — VR pointing is less precise. |
| **Media** | `media_url` fields in quiz JSON can reference 360° images or spatial videos for VR-enhanced questions. |
| **Timer** | Display countdown timers on a world-space element, not a screen overlay. Use spatial audio tick sounds for urgency. |

### Console Platforms

| Consideration | Guidance |
|--------------|----------|
| **Storage paths** | `Application.persistentDataPath` works on all consoles, but some platforms have storage quotas. Keep disk cache under 50MB. |
| **Content rating** | Quiz content must comply with the game's ESRB/PEGI rating. LLM-generated content should be filtered through `IVXAIModerator` before display. |
| **Offline mode** | Console certification often requires offline functionality. Bundle a local quiz fallback via `IVXLocalQuizProvider` for offline play. |
| **Parental controls** | If quizzes touch age-sensitive topics, respect platform parental control settings. |

### WebGL / Browser

| Consideration | Guidance |
|--------------|----------|
| **Caching** | Disk cache uses IndexedDB on WebGL instead of filesystem. The `IVXHybridQuizProvider` handles this transparently. |
| **Asset size** | Browser builds should minimize quiz JSON payload. Use compressed responses and paginate large category banks. |
| **Offline** | Service workers can cache quiz data for PWA offline support. Bundle a minimal local quiz set in the WebGL build. |

---

## Checklist

- [ ] S3 bucket created with correct folder structure
- [ ] Quiz JSON files validate against the schema
- [ ] `IVXS3QuizProvider` configured with correct base URL
- [ ] Daily quiz fetches and displays correctly
- [ ] Weekly quiz modes fetch correctly
- [ ] Fallback to local content works when offline
- [ ] CI/CD pipeline generates and uploads daily quizzes
- [ ] Content generation produces valid, diverse questions
- [ ] VR quiz UI uses world-space panels (if targeting VR)
- [ ] Console storage quota respected (if targeting consoles)
- [ ] WebGL IndexedDB caching verified (if targeting browser)
