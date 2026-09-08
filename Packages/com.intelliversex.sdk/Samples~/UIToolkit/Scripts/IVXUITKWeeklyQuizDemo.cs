using System;
using IntelliVerseX.Quiz.WeeklyQuiz;
using UnityEngine;
using UnityEngine.UIElements;

namespace IntelliVerseX.Samples.UIToolkit
{
    [AddComponentMenu("IntelliVerse-X/Samples/UITK Weekly Quiz Demo")]
    public sealed class IVXUITKWeeklyQuizDemo : IVXUITKFeatureDemoBase
    {
        private IVXWeeklyQuizManager _manager;

        protected override void BuildFeatureUI()
        {
            SetTitle("Weekly Quiz", "Fortune / Emoji / Health via IVXWeeklyQuizManager");
            AddAction("Start Fortune", () => StartQuiz(m => m.StartFortuneQuiz()));
            AddAction("Start Emoji", () => StartQuiz(m => m.StartEmojiQuiz()));
            AddAction("Start Health", () => StartQuiz(m => m.StartHealthQuiz()));
            EnsureManager();
            SetStatus("Weekly quiz demo ready.");
        }

        private void EnsureManager()
        {
            _manager = IVXWeeklyQuizManager.Instance ?? FindFirstObjectByType<IVXWeeklyQuizManager>();
            if (_manager == null)
            {
                var go = new GameObject("[IVXWeeklyQuizManager]");
                _manager = go.AddComponent<IVXWeeklyQuizManager>();
            }
        }

        private void StartQuiz(Action<IVXWeeklyQuizManager> starter)
        {
            EnsureManager();
            try
            {
                starter(_manager);
                SetStatus("Quiz start requested.");
            }
            catch (Exception ex)
            {
                SetStatus(ex.Message, true);
            }
        }
    }
}
