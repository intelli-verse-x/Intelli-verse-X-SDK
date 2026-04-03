using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using IntelliVerseX.Hiro;
using IntelliVerseX.Satori;
using UnityEngine;
using {{game_name}}.UI;

namespace {{game_name}}.Engagement
{
    /// <summary>
    /// Session-start analytics, optional daily-reward auto-popup, streak refresh, and a deferred local reminder hook.
    /// </summary>
    public sealed class RetentionManager : MonoBehaviour
    {
        #region Serialized Fields

        [SerializeField] private DailyRewardsPanel _dailyRewardsPanel;
        [SerializeField] private bool _autoPopupDaily = true;

        #endregion

        #region Constants

        private const string SessionCountKey = "ivx_session_count";
        private const string ReminderUtcKey = "ivx_next_daily_reminder_utc";

        #endregion

        #region Unity Lifecycle

        private async void Start()
        {
            var count = PlayerPrefs.GetInt(SessionCountKey, 0) + 1;
            PlayerPrefs.SetInt(SessionCountKey, count);
            PlayerPrefs.Save();

            await TrackSessionStartAsync(count);
            await EvaluateDailyAsync();
            ScheduleLocalReminder();
        }

        #endregion

        #region Private Methods

        private async Task TrackSessionStartAsync(int sessionCount)
        {
            var satori = IVXSatoriClient.Instance;
            if (satori == null || !satori.IsInitialized)
                return;

            await satori.CaptureEventAsync("session_start", new Dictionary<string, string>
            {
                { "game_id", "{{game_id}}" },
                { "session_count", sessionCount.ToString() },
            });
        }

        private async Task EvaluateDailyAsync()
        {
            var hi = IVXHiroCoordinator.Instance;
            if (hi != null && hi.IsInitialized)
                await hi.Streaks.GetAsync("{{game_id}}");

            if (_autoPopupDaily && _dailyRewardsPanel != null)
                _dailyRewardsPanel.ShowPanel();
        }

        private void ScheduleLocalReminder()
        {
            var next = DateTime.UtcNow.AddHours(24);
            PlayerPrefs.SetString(ReminderUtcKey, next.ToString("o"));
            PlayerPrefs.Save();
            Debug.Log(
                $"[RetentionManager] Come back for your daily reward! — scheduled ~{next:o} UTC (install Unity Mobile Notifications to deliver).");
        }

        #endregion
    }
}
