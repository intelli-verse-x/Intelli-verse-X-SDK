using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IntelliVerseX.Hiro;
using IntelliVerseX.Satori;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace {{game_name}}.UI
{
    /// <summary>
    /// Seven-day calendar UI backed by Hiro streaks: updates streak, claims milestones, and surfaces streak count.
    /// Wraps <see cref="IntelliVerseX.Hiro.Systems.IVXStreaksSystem"/> (equivalent to a daily-reward claim flow).
    /// </summary>
    public sealed class DailyRewardsPanel : MonoBehaviour
    {
        #region Serialized Fields

        [SerializeField] private string _dailyStreakId = "daily_login";
        [SerializeField] private Image[] _dayHighlights = new Image[7];
        [SerializeField] private TextMeshProUGUI _streakCounter;
        [SerializeField] private Button _claimButton;
        [SerializeField] private GameObject _root;

        #endregion

        #region Private Fields

        private int _currentDayIndex;
        private IVXStreak _streak;

        #endregion

        #region Public Methods

        /// <summary>Shows the panel (e.g. auto-popup from retention).</summary>
        public void ShowPanel()
        {
            if (_root != null)
                _root.SetActive(true);
            _ = RefreshAsync();
        }

        #endregion

        #region Unity Lifecycle

        private async void OnEnable()
        {
            _claimButton?.onClick.AddListener(OnClaimClicked);
            await RefreshAsync();
        }

        private void OnDisable()
        {
            _claimButton?.onClick.RemoveListener(OnClaimClicked);
        }

        #endregion

        #region Private Methods

        private async Task RefreshAsync()
        {
            var hi = IVXHiroCoordinator.Instance;
            if (hi == null || !hi.IsInitialized)
                return;

            var data = await hi.Streaks.GetAsync("{{game_id}}");
            _streak = data.streaks?.FirstOrDefault(s => s.streakId == _dailyStreakId);
            var count = _streak?.currentCount ?? 0;
            _currentDayIndex = Mathf.Clamp(count % 7, 0, 6);

            if (_streakCounter != null)
                _streakCounter.text = $"Streak: {count}";

            for (var i = 0; i < _dayHighlights.Length; i++)
            {
                if (_dayHighlights[i] != null)
                    _dayHighlights[i].enabled = i == _currentDayIndex;
            }
        }

        private void OnClaimClicked()
        {
            _ = ClaimDailyRewardAsync();
        }

        private async Task ClaimDailyRewardAsync()
        {
            var hi = IVXHiroCoordinator.Instance;
            if (hi == null || !hi.IsInitialized)
                return;

            await hi.Streaks.UpdateAsync(_dailyStreakId, "{{game_id}}");
            var milestone = _currentDayIndex + 1;
            var claim = await hi.Streaks.ClaimMilestoneAsync(_dailyStreakId, milestone, "{{game_id}}");

            var satori = IVXSatoriClient.Instance;
            if (satori != null && satori.IsInitialized)
                _ = satori.CaptureEventAsync("daily_reward_claimed", new Dictionary<string, string>
                {
                    { "game_id", "{{game_id}}" },
                    { "milestone", milestone.ToString() },
                    { "ok", (claim != null).ToString() },
                });

            await RefreshAsync();
        }

        #endregion
    }
}
