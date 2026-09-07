// IVXLeaderboardUI.cs
// Reusable leaderboard UI component for all games
// Auto-fetches leaderboard data and handles UI updates
// Drag-drop component for instant leaderboard integration

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using IntelliVerseX.Backend;

namespace IntelliVerseX.UI
{
    /// <summary>
    /// Manages leaderboard UI and data fetching.
    /// Works with any IVXNakamaManager instance.
    /// 
    /// Usage:
    /// 1. Attach to GameObject with leaderboard UI
    /// 2. Assign nakamaManager reference
    /// 3. Assign UI references in Inspector
    /// 4. Call RefreshLeaderboards() to fetch data
    /// 5. Subscribe to OnLeaderboardDataUpdated for custom handling
    /// </summary>
    public class IVXLeaderboardUI : MonoBehaviour
    {
        [Header("Nakama Integration")]
        [Tooltip("Reference to your game's Nakama manager (extends IVXNakamaManager)")]
        [SerializeField] private MonoBehaviour nakamaManagerComponent;
        private IVXNakamaManager nakamaManager;
        
        [Header("UI References")]
        [SerializeField] private GameObject leaderboardPanel;
        [SerializeField] private Transform entriesContainer;
        [SerializeField] private GameObject entryPrefab;
        [SerializeField] private TMP_Text playerRankText;
        [SerializeField] private TMP_Text playerScoreText;
        [SerializeField] private Button refreshButton;
        
        [Header("Tab Buttons (Optional)")]
        [SerializeField] private Button dailyButton;
        [SerializeField] private Button weeklyButton;
        [SerializeField] private Button monthlyButton;
        [SerializeField] private Button alltimeButton;
        [SerializeField] private Button globalButton;
        
        [Header("Settings")]
        [SerializeField] private int entriesPerPage = 50;
        [SerializeField] private bool autoRefreshOnShow = true;
        [SerializeField] private float cacheTimeSeconds = 30f;
        
        // Current state
        private LeaderboardPeriod currentPeriod = LeaderboardPeriod.Daily;
        private IVXAllLeaderboardsResponse cachedData;
        private float lastFetchTime;
        private bool isFetching;
        
        // Events
        public event Action<IVXAllLeaderboardsResponse> OnLeaderboardDataUpdated;
        public event Action<LeaderboardPeriod> OnPeriodChanged;
        
        public enum LeaderboardPeriod
        {
            Daily,
            Weekly,
            Monthly,
            Alltime,
            Global
        }
        
        private void Awake()
        {
            // Cast component to IVXNakamaManager
            if (nakamaManagerComponent != null)
            {
                nakamaManager = nakamaManagerComponent as IVXNakamaManager;
                if (nakamaManager == null)
                {
                    Debug.LogError("[IVXLeaderboard] NakamaManager must extend IVXNakamaManager!");
                }
            }
        }
        
        private void Start()
        {
            SetupTabButtons();
            
            if (refreshButton != null)
                refreshButton.onClick.AddListener(() => RefreshLeaderboards(force: true));
        }
        
        private void OnEnable()
        {
            if (autoRefreshOnShow && !isFetching)
            {
                RefreshLeaderboards();
            }
        }
        
        private void SetupTabButtons()
        {
            if (dailyButton != null)
                dailyButton.onClick.AddListener(() => SwitchPeriod(LeaderboardPeriod.Daily));
            if (weeklyButton != null)
                weeklyButton.onClick.AddListener(() => SwitchPeriod(LeaderboardPeriod.Weekly));
            if (monthlyButton != null)
                monthlyButton.onClick.AddListener(() => SwitchPeriod(LeaderboardPeriod.Monthly));
            if (alltimeButton != null)
                alltimeButton.onClick.AddListener(() => SwitchPeriod(LeaderboardPeriod.Alltime));
            if (globalButton != null)
                globalButton.onClick.AddListener(() => SwitchPeriod(LeaderboardPeriod.Global));
        }
        
        public void SwitchPeriod(LeaderboardPeriod period)
        {
            currentPeriod = period;
            UpdateTabButtonStates();
            DisplayCurrentPeriod();
            OnPeriodChanged?.Invoke(period);
        }
        
        private void UpdateTabButtonStates()
        {
            // Visual feedback for selected tab (override in child class or use events)
            // Example: Set colors, scales, or enable/disable states
        }
        
        public async void RefreshLeaderboards(bool force = false)
        {
            // Check cache validity
            if (!force && cachedData != null && Time.time - lastFetchTime < cacheTimeSeconds)
            {
                Debug.Log("[IVXLeaderboard] Using cached data");
                DisplayCurrentPeriod();
                return;
            }
            
            if (isFetching)
            {
                Debug.Log("[IVXLeaderboard] Already fetching...");
                return;
            }
            
            if (nakamaManager == null || !nakamaManager.IsInitialized)
            {
                Debug.LogError("[IVXLeaderboard] Nakama manager not initialized!");
                return;
            }
            
            isFetching = true;
            
            try
            {
                cachedData = await nakamaManager.GetAllLeaderboards(entriesPerPage);
                lastFetchTime = Time.time;
                
                if (cachedData != null && cachedData.success)
                {
                    Debug.Log("[IVXLeaderboard] Leaderboard data fetched successfully");
                    DisplayCurrentPeriod();
                    UpdatePlayerRank();
                    OnLeaderboardDataUpdated?.Invoke(cachedData);
                }
                else
                {
                    Debug.LogError("[IVXLeaderboard] Failed to fetch leaderboards");
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"[IVXLeaderboard] Error fetching leaderboards: {ex.Message}");
            }
            finally
            {
                isFetching = false;
            }
        }
        
        private void DisplayCurrentPeriod()
        {
            if (cachedData == null) return;
            
            IVXLeaderboardData data = currentPeriod switch
            {
                LeaderboardPeriod.Daily => cachedData.daily,
                LeaderboardPeriod.Weekly => cachedData.weekly,
                LeaderboardPeriod.Monthly => cachedData.monthly,
                LeaderboardPeriod.Alltime => cachedData.alltime,
                LeaderboardPeriod.Global => cachedData.global_alltime,
                _ => cachedData.daily
            };
            
            DisplayLeaderboardData(data);
        }
        
        private void DisplayLeaderboardData(IVXLeaderboardData data)
        {
            // Clear existing entries
            foreach (Transform child in entriesContainer)
            {
                Destroy(child.gameObject);
            }
            
            if (data == null || data.records == null || data.records.Count == 0)
            {
                Debug.Log("[IVXLeaderboard] No records to display");
                return;
            }
            
            // Create entry for each record
            for (int i = 0; i < data.records.Count; i++)
            {
                var record = data.records[i];
                var entry = Instantiate(entryPrefab, entriesContainer);
                
                // Find components (assumes specific hierarchy - override for custom)
                var rankText = entry.transform.Find("RankText")?.GetComponent<TMP_Text>();
                var usernameText = entry.transform.Find("UsernameText")?.GetComponent<TMP_Text>();
                var scoreText = entry.transform.Find("ScoreText")?.GetComponent<TMP_Text>();
                
                if (rankText != null) rankText.text = $"#{record.rank}";
                if (usernameText != null) usernameText.text = record.username;
                if (scoreText != null) scoreText.text = record.score.ToString("N0");
                
                // Highlight player's entry
                if (record.owner_id == nakamaManager.Session?.UserId)
                {
                    entry.GetComponent<Image>()?.material.SetColor("_Color", Color.yellow);
                }
            }
        }
        
        private void UpdatePlayerRank()
        {
            if (cachedData == null || cachedData.player_ranks == null) return;
            
            int? rank = currentPeriod switch
            {
                LeaderboardPeriod.Daily => cachedData.player_ranks.daily_rank,
                LeaderboardPeriod.Weekly => cachedData.player_ranks.weekly_rank,
                LeaderboardPeriod.Monthly => cachedData.player_ranks.monthly_rank,
                LeaderboardPeriod.Alltime => cachedData.player_ranks.alltime_rank,
                LeaderboardPeriod.Global => cachedData.player_ranks.global_rank,
                _ => null
            };
            
            if (playerRankText != null)
            {
                playerRankText.text = rank.HasValue ? $"Your Rank: #{rank.Value}" : "Unranked";
            }
        }
        
        /// <summary>
        /// Get current leaderboard data for custom processing
        /// </summary>
        public IVXAllLeaderboardsResponse GetCachedData() => cachedData;
        
        /// <summary>
        /// Show leaderboard panel
        /// </summary>
        public void Show()
        {
            if (leaderboardPanel != null)
                leaderboardPanel.SetActive(true);
        }
        
        /// <summary>
        /// Hide leaderboard panel
        /// </summary>
        public void Hide()
        {
            if (leaderboardPanel != null)
                leaderboardPanel.SetActive(false);
        }
    }
}
