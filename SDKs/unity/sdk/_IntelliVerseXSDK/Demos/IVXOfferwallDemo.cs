using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace IntelliVerseX.Demos
{
    /// <summary>
    /// Demo UI for the Offerwall system.
    /// Shows scrollable offer cards with rewards, descriptions, and claim/complete buttons.
    /// Attach to a Canvas.
    /// </summary>
    public class IVXOfferwallDemo : MonoBehaviour
    {
        #region Constants

        private static readonly Color COL_BG = new Color(0.05f, 0.06f, 0.09f);
        private static readonly Color COL_PANEL = new Color(0.10f, 0.12f, 0.17f);
        private static readonly Color COL_CARD = new Color(0.13f, 0.15f, 0.21f);
        private static readonly Color COL_PRIMARY = new Color(0.25f, 0.50f, 0.90f);
        private static readonly Color COL_GOLD = new Color(0.95f, 0.78f, 0.20f);
        private static readonly Color COL_SUCCESS = new Color(0.20f, 0.75f, 0.45f);
        private static readonly Color COL_TEXT = new Color(0.92f, 0.93f, 0.95f);
        private static readonly Color COL_DIM = new Color(0.55f, 0.58f, 0.64f);
        private static readonly Color COL_TAG_HOT = new Color(0.90f, 0.30f, 0.25f);
        private static readonly Color COL_TAG_NEW = new Color(0.30f, 0.70f, 0.40f);
        private static readonly Color COL_TAG_EASY = new Color(0.25f, 0.50f, 0.90f);

        private struct OfferData
        {
            public string Title, Description, Reward, Icon, Tag;
            public Color TagColor;
            public bool Completed;
        }

        private static readonly OfferData[] MOCK_OFFERS =
        {
            new() { Title = "Install Puzzle Quest", Description = "Install and reach level 5", Reward = "500 Gems", Icon = "\ud83c\udfae", Tag = "HOT", TagColor = COL_TAG_HOT },
            new() { Title = "Sign Up for StreamPlus", Description = "Create a free account", Reward = "200 Coins", Icon = "\ud83d\udcfa", Tag = "EASY", TagColor = COL_TAG_EASY },
            new() { Title = "Complete Survey #12", Description = "Answer 10 questions (~3 min)", Reward = "150 Gems", Icon = "\ud83d\udcdd", Tag = "NEW", TagColor = COL_TAG_NEW },
            new() { Title = "Battle Royale Download", Description = "Install and play first match", Reward = "750 Gems", Icon = "\ud83d\udee1\ufe0f", Tag = "HOT", TagColor = COL_TAG_HOT },
            new() { Title = "Watch 3 Video Ads", Description = "Watch 3 rewarded videos", Reward = "100 Coins", Icon = "\ud83c\udfa5", Tag = "EASY", TagColor = COL_TAG_EASY, Completed = true },
            new() { Title = "Fantasy RPG: Reach Lv10", Description = "Install and reach level 10", Reward = "1200 Gems", Icon = "\u2694\ufe0f", Tag = "HOT", TagColor = COL_TAG_HOT },
            new() { Title = "Newsletter Signup", Description = "Subscribe to weekly digest", Reward = "50 Coins", Icon = "\ud83d\udce7", Tag = "EASY", TagColor = COL_TAG_EASY },
        };

        #endregion

        #region Private Fields

        private TextMeshProUGUI _totalEarned;
        private int _totalClaimed;

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
            var vlg = AddVLayout(root.gameObject, 12f, new RectOffset(16, 16, 40, 16));
            vlg.childForceExpandHeight = false;

            // Header
            var header = MakeRect(root, "Header");
            header.gameObject.AddComponent<LayoutElement>().minHeight = 70f;
            var hvlg = AddVLayout(header.gameObject, 4f, new RectOffset(4, 4, 0, 0));
            hvlg.childAlignment = TextAnchor.MiddleCenter;

            MakeTMP(header, "Title", "Offerwall", 28f, FontStyles.Bold, COL_GOLD, TextAlignmentOptions.Center);
            MakeTMP(header, "Sub", "Complete offers to earn rewards!", 14f, FontStyles.Normal, COL_DIM, TextAlignmentOptions.Center);

            // Earnings bar
            var earnBar = MakePanel(root, "EarnBar", COL_PANEL);
            earnBar.gameObject.AddComponent<LayoutElement>().minHeight = 48f;
            var ehlg = earnBar.gameObject.AddComponent<HorizontalLayoutGroup>();
            ehlg.spacing = 8f; ehlg.padding = new RectOffset(16, 16, 8, 8);
            ehlg.childAlignment = TextAnchor.MiddleCenter;
            ehlg.childControlWidth = false; ehlg.childControlHeight = true;

            MakeTMP(earnBar, "Lbl", "Total Earned:", 15f, FontStyles.Normal, COL_DIM);
            _totalEarned = MakeTMP(earnBar, "Val", "0 rewards", 15f, FontStyles.Bold, COL_GOLD);
            _totalEarned.gameObject.AddComponent<LayoutElement>().flexibleWidth = 1f;
            _totalEarned.alignment = TextAlignmentOptions.MidlineRight;

            // Offers scroll
            var scrollArea = MakeRect(root, "ScrollArea");
            scrollArea.gameObject.AddComponent<LayoutElement>().flexibleHeight = 1f;
            scrollArea.gameObject.AddComponent<RectMask2D>();
            var scroll = scrollArea.gameObject.AddComponent<ScrollRect>();
            scroll.horizontal = false; scroll.vertical = true;
            scroll.movementType = ScrollRect.MovementType.Elastic;

            var vp = MakeRect(scrollArea, "VP"); Stretch(vp);
            var content = MakeRect(vp, "Content");
            content.anchorMin = new Vector2(0, 1); content.anchorMax = new Vector2(1, 1);
            content.pivot = new Vector2(0.5f, 1f);
            var cvlg = AddVLayout(content.gameObject, 10f, new RectOffset(0, 0, 4, 12));
            cvlg.childForceExpandHeight = false;
            content.gameObject.AddComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            scroll.viewport = vp; scroll.content = content;

            foreach (var offer in MOCK_OFFERS)
                CreateOfferCard(content, offer);
        }

        private void CreateOfferCard(RectTransform parent, OfferData offer)
        {
            var card = MakePanel(parent, "Card", COL_CARD);
            card.gameObject.AddComponent<LayoutElement>().minHeight = 100f;
            var hlg = card.gameObject.AddComponent<HorizontalLayoutGroup>();
            hlg.spacing = 14f; hlg.padding = new RectOffset(14, 14, 12, 12);
            hlg.childAlignment = TextAnchor.MiddleLeft;
            hlg.childControlWidth = false; hlg.childControlHeight = true;
            hlg.childForceExpandWidth = false;

            // Icon
            var iconBg = MakePanel(card, "IconBg", new Color(0.18f, 0.20f, 0.28f));
            iconBg.gameObject.AddComponent<LayoutElement>().SetLayout(56f, 56f);
            var iconVlg = AddVLayout(iconBg.gameObject, 0f, new RectOffset(0, 0, 0, 0));
            iconVlg.childAlignment = TextAnchor.MiddleCenter;
            MakeTMP(iconBg, "Ic", offer.Icon, 30f, FontStyles.Normal, COL_TEXT, TextAlignmentOptions.Center);

            // Info column
            var info = MakeRect(card, "Info");
            info.gameObject.AddComponent<LayoutElement>().flexibleWidth = 1f;
            var ivlg = AddVLayout(info.gameObject, 3f, new RectOffset(0, 0, 0, 0));
            ivlg.childForceExpandHeight = false;

            // Title row with tag
            var titleRow = MakeRect(info, "TitleRow");
            var thlg = titleRow.gameObject.AddComponent<HorizontalLayoutGroup>();
            thlg.spacing = 8f; thlg.childAlignment = TextAnchor.MiddleLeft;
            thlg.childControlWidth = false; thlg.childControlHeight = true;
            thlg.childForceExpandWidth = false;
            titleRow.gameObject.AddComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            MakeTMP(titleRow, "T", offer.Title, 16f, FontStyles.Bold, COL_TEXT);

            if (!string.IsNullOrEmpty(offer.Tag))
            {
                var tagBg = MakePanel(titleRow, "Tag", offer.TagColor);
                tagBg.gameObject.AddComponent<LayoutElement>().SetLayout(42f, 20f);
                var tVlg = AddVLayout(tagBg.gameObject, 0f, new RectOffset(6, 6, 1, 1));
                tVlg.childAlignment = TextAnchor.MiddleCenter;
                MakeTMP(tagBg, "TL", offer.Tag, 9f, FontStyles.Bold, COL_TEXT, TextAlignmentOptions.Center);
            }

            MakeTMP(info, "Desc", offer.Description, 12f, FontStyles.Normal, COL_DIM);

            // Reward row
            var rewardRow = MakeRect(info, "RewardRow");
            var rhlg = rewardRow.gameObject.AddComponent<HorizontalLayoutGroup>();
            rhlg.spacing = 4f; rhlg.childAlignment = TextAnchor.MiddleLeft;
            rhlg.childControlWidth = false; rhlg.childControlHeight = true;
            rewardRow.gameObject.AddComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            MakeTMP(rewardRow, "RI", "\ud83c\udfc6", 14f, FontStyles.Normal, COL_GOLD);
            MakeTMP(rewardRow, "RV", offer.Reward, 14f, FontStyles.Bold, COL_GOLD);

            // Action button
            var btnGo = MakeRect(card, "Btn");
            btnGo.gameObject.AddComponent<LayoutElement>().SetLayout(80f, 40f);
            Color btnColor = offer.Completed ? COL_SUCCESS : COL_PRIMARY;
            var btnImg = btnGo.gameObject.AddComponent<Image>(); btnImg.color = btnColor;
            var btn = btnGo.gameObject.AddComponent<Button>(); btn.targetGraphic = btnImg;

            string btnText = offer.Completed ? "\u2713 Done" : "Start";
            var btnLabel = MakeTMP(btnGo, "Lbl", btnText, 14f, FontStyles.Bold, COL_TEXT, TextAlignmentOptions.Center);
            Stretch(btnLabel.rectTransform);

            if (offer.Completed)
            {
                btn.interactable = false;
            }
            else
            {
                btn.onClick.AddListener(() =>
                {
                    btnLabel.text = "\u2713 Done";
                    btnImg.color = COL_SUCCESS;
                    btn.interactable = false;
                    _totalClaimed++;
                    _totalEarned.text = $"{_totalClaimed} reward{(_totalClaimed > 1 ? "s" : "")} claimed";
                });
            }
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
