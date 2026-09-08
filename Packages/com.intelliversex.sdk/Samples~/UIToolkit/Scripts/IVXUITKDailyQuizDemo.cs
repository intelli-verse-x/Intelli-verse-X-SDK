using System;
using IntelliVerseX.Quiz.DailyQuiz;
using UnityEngine;
using UnityEngine.UIElements;

namespace IntelliVerseX.Samples.UIToolkit
{
    [AddComponentMenu("IntelliVerse-X/Samples/UITK Daily Quiz Demo")]
    public sealed class IVXUITKDailyQuizDemo : IVXUITKFeatureDemoBase
    {
        private IVXDailyQuizManager _manager;

        protected override void BuildFeatureUI()
        {
            SetTitle("Daily Quiz", "Daily / Premium via IVXDailyQuizManager");
            AddAction("Start Daily", StartDaily);
            AddAction("Start Premium", StartPremium);
            EnsureManager();
            SetStatus("Daily quiz demo ready.");
        }

        private void EnsureManager()
        {
            _manager = FindFirstObjectByType<IVXDailyQuizManager>();
            if (_manager == null)
            {
                var go = new GameObject("[IVXDailyQuizManager]");
                _manager = go.AddComponent<IVXDailyQuizManager>();
            }
        }

        private void StartDaily()
        {
            EnsureManager();
            try
            {
                _manager.StartDailyQuiz();
                SetStatus("Daily quiz start requested.");
            }
            catch (Exception ex)
            {
                SetStatus(ex.Message, true);
            }
        }

        private void StartPremium()
        {
            EnsureManager();
            try
            {
                _manager.StartPremiumQuiz();
                SetStatus("Premium quiz start requested.");
            }
            catch (Exception ex)
            {
                SetStatus(ex.Message, true);
            }
        }
    }
}
