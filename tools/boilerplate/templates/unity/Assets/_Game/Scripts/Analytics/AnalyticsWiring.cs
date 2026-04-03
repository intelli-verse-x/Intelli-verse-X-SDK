using System.Collections.Generic;
using IntelliVerseX.Backend;
using IntelliVerseX.Monetization;
using IntelliVerseX.Progression;
using IntelliVerseX.Satori;
using IntelliVerseX.Social;
using UnityEngine;

namespace {{game_name}}.Analytics
{
    /// <summary>
    /// Forwards wallet, achievement, streak, season-pass level, and energy snapshots to Satori.
    /// Place on the bootstrap object alongside SDK clients.
    /// </summary>
    public sealed class AnalyticsWiring : MonoBehaviour
    {
        #region Singleton

        public static AnalyticsWiring Instance { get; private set; }

        #endregion

        #region Private Fields

        private int _lastEnergy = int.MinValue;
        private int _lastEnergyMax = int.MinValue;

        #endregion

        #region Unity Lifecycle

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
        }

        private void OnEnable()
        {
            IVXWalletManager.OnBalanceChanged += OnWalletBalanceChanged;

            var ach = IVXAchievementManager.Instance;
            if (ach != null)
                ach.OnAchievementUnlocked += OnAchievementUnlocked;

            var fs = IVXFriendStreakManager.Instance;
            if (fs != null)
                fs.OnStreakUpdated += OnFriendStreakUpdated;

            var sp = IVXSeasonPassManager.Instance;
            if (sp != null)
                sp.OnLevelUp += OnSeasonLevelUp;
        }

        private void OnDisable()
        {
            IVXWalletManager.OnBalanceChanged -= OnWalletBalanceChanged;

            var ach = IVXAchievementManager.Instance;
            if (ach != null)
                ach.OnAchievementUnlocked -= OnAchievementUnlocked;

            var fs = IVXFriendStreakManager.Instance;
            if (fs != null)
                fs.OnStreakUpdated -= OnFriendStreakUpdated;

            var sp = IVXSeasonPassManager.Instance;
            if (sp != null)
                sp.OnLevelUp -= OnSeasonLevelUp;
        }

        private void OnDestroy()
        {
            if (Instance == this)
                Instance = null;
        }

        #endregion

        #region Public Methods

        /// <summary>Called when Hiro player level increases (from the progression HUD).</summary>
        public void ForwardPlayerLevelUp(int level, long xp)
        {
            var satori = IVXSatoriClient.Instance;
            if (satori == null || !satori.IsInitialized)
                return;

            _ = satori.CaptureEventAsync("level_up", new Dictionary<string, string>
            {
                { "game_id", "{{game_id}}" },
                { "level", level.ToString() },
                { "xp", xp.ToString() },
                { "source", "hiro_progression" },
            });
        }

        /// <summary>Called by the HUD energy bar when Hiro energy is refreshed.</summary>
        public void ForwardEnergyChanged(int current, int max)
        {
            if (current == _lastEnergy && max == _lastEnergyMax)
                return;

            _lastEnergy = current;
            _lastEnergyMax = max;

            var satori = IVXSatoriClient.Instance;
            if (satori == null || !satori.IsInitialized)
                return;

            _ = satori.CaptureEventAsync("energy_changed", new Dictionary<string, string>
            {
                { "game_id", "{{game_id}}" },
                { "current", current.ToString() },
                { "max", max.ToString() },
            });
        }

        #endregion

        #region Private Methods

        private void OnWalletBalanceChanged(int gameBalance, int globalBalance)
        {
            var satori = IVXSatoriClient.Instance;
            if (satori == null || !satori.IsInitialized)
                return;

            _ = satori.CaptureEventAsync("balance_changed", new Dictionary<string, string>
            {
                { "game_id", "{{game_id}}" },
                { "coins", gameBalance.ToString() },
                { "gems", globalBalance.ToString() },
            });
        }

        private void OnAchievementUnlocked(IVXAchievement achievement)
        {
            var satori = IVXSatoriClient.Instance;
            if (satori == null || !satori.IsInitialized || achievement == null)
                return;

            _ = satori.CaptureEventAsync("achievement_unlocked", new Dictionary<string, string>
            {
                { "game_id", "{{game_id}}" },
                { "achievement_id", achievement.id ?? "" },
                { "title", achievement.title ?? "" },
            });
        }

        private void OnFriendStreakUpdated(IVXFriendStreak streak)
        {
            var satori = IVXSatoriClient.Instance;
            if (satori == null || !satori.IsInitialized || streak == null)
                return;

            _ = satori.CaptureEventAsync("streak_updated", new Dictionary<string, string>
            {
                { "game_id", "{{game_id}}" },
                { "friend_id", streak.friendId ?? "" },
                { "current_streak", streak.currentStreak.ToString() },
            });
        }

        private void OnSeasonLevelUp(IVXSeasonPassState state)
        {
            var satori = IVXSatoriClient.Instance;
            if (satori == null || !satori.IsInitialized || state == null)
                return;

            _ = satori.CaptureEventAsync("season_pass_level_up", new Dictionary<string, string>
            {
                { "game_id", "{{game_id}}" },
                { "season_level", state.currentLevel.ToString() },
                { "source", "season_pass" },
            });
        }

        #endregion
    }
}
