# Quiz Demo Sample

This sample demonstrates how to build a complete quiz game using the IntelliVerseX SDK.

## Features Demonstrated

- Quiz session management with `IVXQuizSessionManager`
- Question shuffling with `IVXQuestionShuffler`
- Score tracking and submission
- Leaderboard integration
- UI components with `IVXQuizQuestionPanel` and `IVXQuizResultPanel`

## Setup

1. Import this sample via Package Manager
2. Open the `QuizDemoScene` scene
3. Configure your Nakama backend settings in the `IVXBackendService` component
4. Press Play

## Key Scripts

### QuizDemoController.cs
Main controller that demonstrates:
- Starting a quiz session
- Loading questions from a provider
- Handling user answers
- Calculating and submitting scores

## Dependencies

- IntelliVerseX.Core
- IntelliVerseX.Quiz
- IntelliVerseX.QuizUI
- IntelliVerseX.Leaderboard
- IntelliVerseX.Backend

## Configuration

Edit the `QuizDemoConfig` ScriptableObject to customize:
- Number of questions per session
- Time per question
- Scoring rules
- Categories to include
