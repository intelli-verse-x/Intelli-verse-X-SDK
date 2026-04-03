using UnityEngine;
using UnityEngine.UI;
using TMPro;
using IntelliVerseX.Analytics;

namespace {{game_name}}.UI
{
    /// <summary>
    /// Hub controller for the main menu. Manages tab navigation between all
    /// feature panels: Store, Achievements, Daily Rewards, Leaderboard, Settings.
    /// </summary>
    public class MainMenuController : MonoBehaviour
    {
        #region Serialized Fields

        [Header("Header")]
        [SerializeField] private TextMeshProUGUI _gameTitle;
        [SerializeField] private TextMeshProUGUI _playerName;

        [Header("Panels")]
        [SerializeField] private GameObject _homePanel;
        [SerializeField] private GameObject _storePanel;
        [SerializeField] private GameObject _achievementsPanel;
        [SerializeField] private GameObject _dailyRewardsPanel;
        [SerializeField] private GameObject _leaderboardPanel;
        [SerializeField] private GameObject _settingsPanel;

        [Header("Tab Buttons")]
        [SerializeField] private Button _homeTab;
        [SerializeField] private Button _storeTab;
        [SerializeField] private Button _achievementsTab;
        [SerializeField] private Button _dailyRewardsTab;
        [SerializeField] private Button _leaderboardTab;
        [SerializeField] private Button _settingsTab;

        [Header("Play Button")]
        [SerializeField] private Button _playButton;
        [SerializeField] private string _gameplayScene = "GamePlay";

        #endregion

        #region Private Fields

        private GameObject _activePanel;
        private GameObject[] _allPanels;

        #endregion

        #region Unity Lifecycle

        private void Awake()
        {
            _allPanels = new[]
            {
                _homePanel, _storePanel, _achievementsPanel,
                _dailyRewardsPanel, _leaderboardPanel, _settingsPanel,
            };

            _homeTab?.onClick.AddListener(() => ShowPanel(_homePanel, "home"));
            _storeTab?.onClick.AddListener(() => ShowPanel(_storePanel, "store"));
            _achievementsTab?.onClick.AddListener(() => ShowPanel(_achievementsPanel, "achievements"));
            _dailyRewardsTab?.onClick.AddListener(() => ShowPanel(_dailyRewardsPanel, "daily_rewards"));
            _leaderboardTab?.onClick.AddListener(() => ShowPanel(_leaderboardPanel, "leaderboard"));
            _settingsTab?.onClick.AddListener(() => ShowPanel(_settingsPanel, "settings"));

            _playButton?.onClick.AddListener(OnPlay);
        }

        private void Start()
        {
            if (_gameTitle != null)
                _gameTitle.text = "{{game_name}}";

            var account = IntelliVerseX.Identity.IVXAuthManager.Instance;
            if (_playerName != null && account != null)
                _playerName.text = account.DisplayName ?? "Player";

            ShowPanel(_homePanel, "home");
            TrackScreen("main_menu");
        }

        #endregion

        #region Panel Navigation

        private void ShowPanel(GameObject panel, string screenName)
        {
            foreach (var p in _allPanels)
            {
                if (p != null)
                    p.SetActive(p == panel);
            }
            _activePanel = panel;
            TrackScreen(screenName);
        }

        private void OnPlay()
        {
            TrackScreen("gameplay");
            IVXSatoriClient.Instance?.TrackEvent("game_start", new()
            {
                { "game_id", "{{game_id}}" },
                { "source", "main_menu" },
            });
            UnityEngine.SceneManagement.SceneManager.LoadScene(_gameplayScene);
        }

        #endregion

        #region Analytics

        private void TrackScreen(string name)
        {
            IVXSatoriClient.Instance?.TrackEvent("screen_view", new()
            {
                { "screen", name },
                { "game_id", "{{game_id}}" },
            });
        }

        #endregion
    }
}
