using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace IntelliVerseX.Games.Leaderboard.UI
{
    /// <summary>
    /// UI component for displaying a single leaderboard entry.
    /// Attach to a prefab with rank, username, score text fields, and optional background.
    /// </summary>
    public class IVXGLeaderboardEntryView : MonoBehaviour
    {
        [Header("UI References")]
        [Tooltip("Text for displaying rank number (1, 2, 3, etc.)")]
        [SerializeField] private TextMeshProUGUI rankText;

        [Tooltip("Text for displaying player username")]
        [SerializeField] private TextMeshProUGUI usernameText;

        [Tooltip("Text for displaying player score")]
        [SerializeField] private TextMeshProUGUI scoreText;

        [Tooltip("Optional text for extra info (e.g., number of plays)")]
        [SerializeField] private TextMeshProUGUI extraInfoText;

        [Tooltip("Optional background image for highlighting")]
        [SerializeField] private Image backgroundImage;

        [Header("Visual Settings")]
        [Tooltip("Default background color for normal rows")]
        [SerializeField] private Color normalColor = Color.white;

        [Tooltip("Background color for the current player's row")]
        [SerializeField] private Color selfColor = new Color(1f, 0.95f, 0.4f);

        [Tooltip("Enable alternating row colors (zebra striping)")]
        [SerializeField] private bool useAlternateRowTint = true;

        [Tooltip("Background color for alternate rows")]
        [SerializeField] private Color alternateRowColor = new Color(0.95f, 0.95f, 0.95f);

        [Header("Rank Icons (Optional)")]
        [Tooltip("Icon for 1st place")]
        [SerializeField] private Sprite goldIcon;

        [Tooltip("Icon for 2nd place")]
        [SerializeField] private Sprite silverIcon;

        [Tooltip("Icon for 3rd place")]
        [SerializeField] private Sprite bronzeIcon;

        [Tooltip("Image component for rank icon")]
        [SerializeField] private Image rankIconImage;

        // Stored data
        private string _leaderboardId;
        private string _ownerId;
        private int _rank;
        private long _score;
        private int _numScore;
        private string _username;

        /// <summary>
        /// Data structure for leaderboard entry.
        /// </summary>
        [System.Serializable]
        public struct LeaderboardEntryData
        {
            public string LeaderboardId;
            public string OwnerId;
            public string Username;
            public long Score;
            public int Rank;
            public int NumScore;
            public long CreateUnixTime;
            public long UpdateUnixTime;
        }

        /// <summary>
        /// Configure the entry view with data.
        /// </summary>
        /// <param name="data">Entry data to display</param>
        /// <param name="localUserId">Current player's user ID (for highlighting)</param>
        /// <param name="isAlternateRow">Whether this is an alternate row (for zebra striping)</param>
        public void Setup(LeaderboardEntryData data, string localUserId, bool isAlternateRow = false)
        {
            _leaderboardId = data.LeaderboardId;
            _ownerId = data.OwnerId;
            _rank = data.Rank;
            _score = data.Score;
            _numScore = data.NumScore;
            _username = data.Username;

            // Set rank text
            if (rankText != null)
            {
                rankText.text = data.Rank > 0 ? data.Rank.ToString() : "-";
            }

            // Set rank icon for top 3
            UpdateRankIcon(data.Rank);

            // Set username
            if (usernameText != null)
            {
                string username = string.IsNullOrWhiteSpace(data.Username) ? "Anonymous" : data.Username;
                usernameText.text = username;
            }

            // Set score with formatting
            if (scoreText != null)
            {
                scoreText.text = data.Score.ToString("N0");
            }

            // Set extra info (optional)
            if (extraInfoText != null)
            {
                extraInfoText.text = data.NumScore > 0 ? $"Plays: {data.NumScore}" : string.Empty;
            }

            // Set background color
            UpdateBackgroundColor(localUserId, data.OwnerId, isAlternateRow);
        }

        /// <summary>
        /// Setup with raw IVXGLeaderboardRecord data.
        /// </summary>
        public void Setup(IVXGLeaderboardRecord record, string localUserId, bool isAlternateRow = false)
        {
            var data = new LeaderboardEntryData
            {
                LeaderboardId = record.leaderboard_id,
                OwnerId = record.owner_id,
                Username = record.username,
                Score = record.score,
                Rank = record.rank,
                NumScore = record.num_score,
                CreateUnixTime = ParseUnixTime(record.create_time),
                UpdateUnixTime = ParseUnixTime(record.update_time)
            };

            Setup(data, localUserId, isAlternateRow);
        }

        private void UpdateRankIcon(int rank)
        {
            if (rankIconImage == null) return;

            switch (rank)
            {
                case 1:
                    if (goldIcon != null)
                    {
                        rankIconImage.sprite = goldIcon;
                        rankIconImage.gameObject.SetActive(true);
                        if (rankText != null) rankText.gameObject.SetActive(false);
                    }
                    break;
                case 2:
                    if (silverIcon != null)
                    {
                        rankIconImage.sprite = silverIcon;
                        rankIconImage.gameObject.SetActive(true);
                        if (rankText != null) rankText.gameObject.SetActive(false);
                    }
                    break;
                case 3:
                    if (bronzeIcon != null)
                    {
                        rankIconImage.sprite = bronzeIcon;
                        rankIconImage.gameObject.SetActive(true);
                        if (rankText != null) rankText.gameObject.SetActive(false);
                    }
                    break;
                default:
                    rankIconImage.gameObject.SetActive(false);
                    if (rankText != null) rankText.gameObject.SetActive(true);
                    break;
            }
        }

        private void UpdateBackgroundColor(string localUserId, string ownerId, bool isAlternateRow)
        {
            if (backgroundImage == null) return;

            bool isSelf = !string.IsNullOrEmpty(localUserId) && localUserId == ownerId;

            if (isSelf)
            {
                backgroundImage.color = selfColor;
            }
            else if (useAlternateRowTint && isAlternateRow)
            {
                backgroundImage.color = alternateRowColor;
            }
            else
            {
                backgroundImage.color = normalColor;
            }
        }

        private static long ParseUnixTime(string raw)
        {
            if (string.IsNullOrEmpty(raw)) return 0;

            if (long.TryParse(raw, System.Globalization.NumberStyles.Integer,
                System.Globalization.CultureInfo.InvariantCulture, out var unix))
            {
                return unix;
            }

            if (System.DateTime.TryParse(raw, System.Globalization.CultureInfo.InvariantCulture,
                System.Globalization.DateTimeStyles.AdjustToUniversal | System.Globalization.DateTimeStyles.AssumeUniversal,
                out var dt))
            {
                return new System.DateTimeOffset(dt).ToUnixTimeSeconds();
            }

            return 0;
        }

        #region Public Properties

        public string OwnerId => _ownerId;
        public string LeaderboardId => _leaderboardId;
        public int Rank => _rank;
        public long Score => _score;
        public int NumScore => _numScore;
        public string Username => _username;

        #endregion
    }
}
