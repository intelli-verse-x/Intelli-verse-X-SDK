using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace IntelliVerseX.Analytics
{
    /// <summary>
    /// IVXServerDrivenPushScheduler — Phase 4C (qv-insights-loop).
    ///
    /// Surfaces the server-computed push schedule (per cohort, per
    /// timezone) to the game-side notification system. The SDK does
    /// NOT take a hard Firebase dependency — instead, game code
    /// subscribes to OnScheduleChanged and registers the resulting
    /// hours with Firebase Local Notifications, OneSignal, or
    /// whichever push provider it uses.
    ///
    /// Usage:
    ///   IVXServerDrivenPushScheduler.Instance.OnScheduleChanged += sched => {
    ///       NotificationManager.ClearScheduled();
    ///       foreach (var h in sched.SendHoursUtc)
    ///           NotificationManager.Schedule(h, sched.Topic);
    ///   };
    ///   await IVXServerDrivenPushScheduler.Instance.PrimeAsync();
    ///
    /// Why server-driven? The AI svc PersonalizationService computes
    /// optimal send times per cohort × timezone based on historical
    /// session_start density — a 7am send for "morning_commuter" cohort
    /// vs an 11pm send for "night_owl_streak". Lifting that decision
    /// out of the client lets us A/B test send timing without an app
    /// release.
    /// </summary>
    public class IVXServerDrivenPushScheduler
    {
        public static IVXServerDrivenPushScheduler Instance { get; } =
            new IVXServerDrivenPushScheduler();

        public event Action<IVXPerModePersonalizationService.PushSchedule> OnScheduleChanged;

        private IVXPerModePersonalizationService.PushSchedule _current;

        public bool IsInitialized => IVXPerModePersonalizationService.Instance.IsInitialized;

        public IVXPerModePersonalizationService.PushSchedule GetCurrent() => _current;

        public async Task PrimeAsync()
        {
            if (!IsInitialized) return;
            await IVXPerModePersonalizationService.Instance.GetBundleAsync();
            var fresh = IVXPerModePersonalizationService.Instance.GetPushSchedule();
            if (!IsSameSchedule(fresh, _current))
            {
                _current = fresh;
                try { OnScheduleChanged?.Invoke(_current); }
                catch (Exception e)
                {
                    Debug.LogWarning($"[IVXServerDrivenPushScheduler] OnScheduleChanged threw: {e.Message}");
                }
            }
        }

        public async Task RefreshAsync()
        {
            if (!IsInitialized) return;
            IVXPerModePersonalizationService.Instance.Invalidate();
            await PrimeAsync();
        }

        private static bool IsSameSchedule(
            IVXPerModePersonalizationService.PushSchedule a,
            IVXPerModePersonalizationService.PushSchedule b)
        {
            if (a == null && b == null) return true;
            if (a == null || b == null) return false;
            if (a.Topic != b.Topic) return false;
            if (a.Timezone != b.Timezone) return false;
            var ah = a.SendHoursUtc ?? new List<int>();
            var bh = b.SendHoursUtc ?? new List<int>();
            if (ah.Count != bh.Count) return false;
            for (int i = 0; i < ah.Count; i++) if (ah[i] != bh[i]) return false;
            return true;
        }
    }
}
