using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace IntelliVerseX.Demos
{
    /// <summary>
    /// Demo UI for the Daily Rewards / Streak system.
    /// Shows a 7-day calendar, current streak, shield status, session booster, and claim button.
    /// Attach to a Canvas.
    /// </summary>
    public class IVXStreakDemo : MonoBehaviour
    {
        #region Constants

        private static readonly Color COL_BG = new Color(0.05f, 0.06f, 0.09f);
        private static readonly Color COL_PANEL = new Color(0.10f, 0.12f, 0.17f);
        private static readonly Color COL_PRIMARY = new Color(0.25f, 0.50f, 0.90f);
        private static readonly Color COL_GOLD = new Color(0.95f, 0.78f, 0.20f);
        private static readonly Color COL_SUCCESS = new Color(0.20f, 0.75f, 0.45f);
        private static readonly Color COL_CLAIMED = new Color(0.18f, 0.55f, 0.35f);
        private static readonly Color COL_TODAY = new Color(0.90f, 0.60f, 0.15f);
        private static readonly Color COL_LOCKED = new Color(0.18f, 0.20f, 0.27f);
        private static readonly Color COL_TEXT = new Color(0.92f, 0.93f, 0.95f);
        private static readonly Color COL_DIM = new Color(0.55f, 0.58f, 0.64f);

        private static readonly string[] DAY_REWARDS = { "50 Coins", "1 Gem", "100 Coins", "Shield", "2 Gems", "200 Coins", "Rare Chest" };
        private static readonly string[] DAY_ICONS = { "\ud83e\ude99", "\ud83d\udc8e", "\ud83e\ude99", "\ud83d\udee1\ufe0f", "\ud83d\udc8e", "\ud83e\ude99", "\ud83c\udf81" };

        #endregion

        #region Private Fields

        private int _currentStreak = 4;
        private bool _todayClaimed;
        private int _shieldsOwned = 2;
        private bool _boosterActive;
        private Button _claimBtn;
        private TextMeshProUGUI _claimLabel;
        private TextMeshProUGUI _streakCounter;
        private TextMeshProUGUI _shieldText;
        private TextMeshProUGUI _boosterText;
        private Image[] _dayImages;
        private TextMeshProUGUI[] _dayCheckmarks;

        #endregion

        #region Unity Lifecycle

        private void Start()
        {
            BuildUI();
        }

        #endregion

        #region UI Construction

        private void BuildUI()
        {
            var bg = MakePanel(transform, "BG", COL_BG);
            Stretch(bg);

            var root = MakeRect(transform, "Root");
            Stretch(root);
            var vlg = AddVLayout(root.gameObject, 14f, new RectOffset(20, 20, 40, 20));
            vlg.childForceExpandHeight = false;

            // Header
            MakeTMP(root, "Title", "Daily Rewards", 28f, FontStyles.Bold, COL_GOLD, TextAlignmentOptions.Center)
                .gameObject.AddComponent<LayoutElement>().minHeight = 40f;

            // Streak counter
            var streakRow = MakeRect(root, "StreakRow");
            streakRow.gameObject.AddComponent<LayoutElement>().minHeight = 60f;
            var shlg = streakRow.gameObject.AddComponent<HorizontalLayoutGroup>();
            shlg.spacing = 12f; shlg.childAlignment = TextAnchor.MiddleCenter;
            shlg.childControlWidth = false; shlg.childControlHeight = true;

            MakeTMP(streakRow, "Fire", "\ud83d\udd25", 36f, FontStyles.Normal, COL_GOLD);
            _streakCounter = MakeTMP(streakRow, "Count", $"{_currentStreak}-Day Streak!", 24f, FontStyles.Bold, COL_TEXT);

            // 7-day grid
            var gridPanel = MakePanel(root, "Grid", COL_PANEL);
            gridPanel.gameObject.AddComponent<LayoutElement>().minHeight = 130f;
            var gridH = gridPanel.gameObject.AddComponent<HorizontalLayoutGroup>();
            gridH.spacing = 6f; gridH.padding = new RectOffset(10, 10, 12, 12);
            gridH.childAlignment = TextAnchor.MiddleCenter;
            gridH.childControlWidth = true; gridH.childControlHeight = true;
            gridH.childForceExpandWidth = true;

            _dayImages = new Image[7];
            _dayCheckmarks = new TextMeshProUGUI[7];

            for (int i = 0; i < 7; i++)
            {
                var day = MakeRect(gridPanel, $"Day{i + 1}");
                Color bg2;
                if (i < _currentStreak) bg2 = COL_CLAIMED;
                else if (i == _currentStreak) bg2 = COL_TODAY;
                else bg2 = COL_LOCKED;

                _dayImages[i] = day.gameObject.AddComponent<Image>();
                _dayImages[i].color = bg2;

                var dvlg = AddVLayout(day.gameObject, 2f, new RectOffset(4, 4, 6, 4));
                dvlg.childAlignment = TextAnchor.MiddleCenter;

                MakeTMP(day, "DayN", $"Day {i + 1}", 10f, FontStyles.Bold, COL_TEXT, TextAlignmentOptions.Center);
                MakeTMP(day, "Icon", DAY_ICONS[i], 24f, FontStyles.Normal, COL_TEXT, TextAlignmentOptions.Center);
                MakeTMP(day, "Reward", DAY_REWARDS[i], 9f, FontStyles.Normal, COL_DIM, TextAlignmentOptions.Center);

                _dayCheckmarks[i] = MakeTMP(day, "Check", i < _currentStreak ? "\u2713" : "", 16f, FontStyles.Bold, COL_SUCCESS, TextAlignmentOptions.Center);
            }

            // Shield status
            var shieldRow = MakePanel(root, "ShieldRow", COL_PANEL);
            shieldRow.gameObject.AddComponent<LayoutElement>().minHeight = 52f;
            var shhlg = shieldRow.gameObject.AddComponent<HorizontalLayoutGroup>();
            shhlg.spacing = 10f; shhlg.padding = new RectOffset(16, 16, 8, 8);
            shhlg.childAlignment = TextAnchor.MiddleLeft;
            shhlg.childControlWidth = false; shhlg.childControlHeight = true;

            MakeTMP(shieldRow, "Icon", "\ud83d\udee1\ufe0f", 24f, FontStyles.Normal, COL_PRIMARY);
            MakeTMP(shieldRow, "Label", "Streak Shield", 16f, FontStyles.Bold, COL_TEXT);
            _shieldText = MakeTMP(shieldRow, "Count", $"{_shieldsOwned} owned", 14f, FontStyles.Normal, COL_DIM);
            _shieldText.gameObject.AddComponent<LayoutElement>().flexibleWidth = 1f;
            _shieldText.alignment = TextAlignmentOptions.MidlineRight;

            // Booster status
            var boostRow = MakePanel(root, "BoostRow", COL_PANEL);
            boostRow.gameObject.AddComponent<LayoutElement>().minHeight = 52f;
            var bhlg = boostRow.gameObject.AddComponent<HorizontalLayoutGroup>();
            bhlg.spacing = 10f; bhlg.padding = new RectOffset(16, 16, 8, 8);
            bhlg.childAlignment = TextAnchor.MiddleLeft;
            bhlg.childControlWidth = false; bhlg.childControlHeight = true;

            MakeTMP(boostRow, "Icon", "\u26a1", 24f, FontStyles.Normal, COL_GOLD);
            MakeTMP(boostRow, "Label", "Session Booster", 16f, FontStyles.Bold, COL_TEXT);
            _boosterText = MakeTMP(boostRow, "Status", "2x XP \u2014 Inactive", 14f, FontStyles.Normal, COL_DIM);
            _boosterText.gameObject.AddComponent<LayoutElement>().flexibleWidth = 1f;
            _boosterText.alignment = TextAlignmentOptions.MidlineRight;

            // Claim button
            var btnGo = MakeRect(root, "ClaimBtn");
            btnGo.gameObject.AddComponent<LayoutElement>().minHeight = 58f;
            var btnImg = btnGo.gameObject.AddComponent<Image>(); btnImg.color = COL_TODAY;
            _claimBtn = btnGo.gameObject.AddComponent<Button>(); _claimBtn.targetGraphic = btnImg;
            _claimLabel = MakeTMP(btnGo, "Lbl", $"Claim Day {_currentStreak + 1} Reward", 20f, FontStyles.Bold, COL_TEXT, TextAlignmentOptions.Center);
            Stretch(_claimLabel.rectTransform);
            _claimBtn.onClick.AddListener(OnClaim);

            // Subtitle
            MakeTMP(root, "Sub", "Come back every day to keep your streak alive!", 13f, FontStyles.Italic, COL_DIM, TextAlignmentOptions.Center)
                .gameObject.AddComponent<LayoutElement>().minHeight = 20f;
        }

        #endregion

        #region Claim Logic

        private void OnClaim()
        {
            if (_todayClaimed) return;
            _todayClaimed = true;

            if (_currentStreak < 7)
            {
                _dayImages[_currentStreak].color = COL_CLAIMED;
                _dayCheckmarks[_currentStreak].text = "\u2713";
                _currentStreak++;
            }

            _streakCounter.text = $"{_currentStreak}-Day Streak!";
            _claimLabel.text = "\u2713  Claimed!";
            _claimBtn.interactable = false;
            (_claimBtn.targetGraphic as Image).color = COL_CLAIMED;

            _boosterActive = true;
            _boosterText.text = "2x XP \u2014 Active \u2713";
            _boosterText.color = COL_SUCCESS;
        }

        #endregion

        #region Primitives

        private static RectTransform MakeRect(Transform p, string n) { var go = new GameObject(n, typeof(RectTransform)); go.transform.SetParent(p, false); return go.GetComponent<RectTransform>(); }
        private static RectTransform MakePanel(Transform p, string n, Color c) { var r = MakeRect(p, n); r.gameObject.AddComponent<Image>().color = c; return r; }
        private static TextMeshProUGUI MakeTMP(RectTransform p, string n, string t, float s, FontStyles st, Color c, TextAlignmentOptions a = TextAlignmentOptions.MidlineLeft)
        { var r = MakeRect(p, n); var tmp = r.gameObject.AddComponent<TextMeshProUGUI>(); tmp.text = t; tmp.fontSize = s; tmp.fontStyle = st; tmp.color = c; tmp.alignment = a; tmp.raycastTarget = false; return tmp; }
        private static VerticalLayoutGroup AddVLayout(GameObject go, float sp, RectOffset pad)
        { var v = go.AddComponent<VerticalLayoutGroup>(); v.spacing = sp; v.padding = pad; v.childAlignment = TextAnchor.UpperLeft; v.childControlWidth = true; v.childControlHeight = false; v.childForceExpandWidth = true; v.childForceExpandHeight = false; return v; }
        private static void Stretch(RectTransform r, float pad = 0f) { r.anchorMin = Vector2.zero; r.anchorMax = Vector2.one; r.offsetMin = new Vector2(pad, pad); r.offsetMax = new Vector2(-pad, -pad); }

        #endregion
    }
}
