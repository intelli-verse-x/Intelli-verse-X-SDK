using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using IntelliVerseX.GameModes;

namespace IntelliVerseX.Demos
{
    /// <summary>
    /// Self-contained demo UI for the Game Mode Selector.
    /// Programmatically builds a card-based mode picker with a scrollable list
    /// and a per-mode player configuration panel at the bottom.
    /// Attach to a Canvas.
    /// </summary>
    public class IVXGameModeSelectorDemo : MonoBehaviour
    {
        #region Constants

        private static readonly Color COL_BG        = new Color(0.102f, 0.102f, 0.180f);
        private static readonly Color COL_CARD      = new Color(0.086f, 0.129f, 0.243f);
        private static readonly Color COL_HEADER    = new Color(0.06f, 0.07f, 0.14f);
        private static readonly Color COL_TEXT      = new Color(0.92f, 0.93f, 0.95f);
        private static readonly Color COL_DIM       = new Color(0.55f, 0.58f, 0.64f);
        private static readonly Color COL_CONFIG    = new Color(0.08f, 0.10f, 0.18f);
        private static readonly Color COL_SELECTED  = new Color(0.14f, 0.18f, 0.32f);
        private static readonly Color COL_INPUT_BG  = new Color(0.12f, 0.14f, 0.22f);
        private static readonly Color COL_BTN_ALT   = new Color(0.18f, 0.22f, 0.35f);

        private static readonly Color COL_GREEN     = new Color(0.20f, 0.78f, 0.45f);
        private static readonly Color COL_BLUE      = new Color(0.25f, 0.52f, 0.95f);
        private static readonly Color COL_RED       = new Color(0.92f, 0.40f, 0.20f);
        private static readonly Color COL_PURPLE    = new Color(0.62f, 0.32f, 0.90f);
        private static readonly Color COL_GOLD      = new Color(0.95f, 0.78f, 0.20f);
        private static readonly Color COL_TEAL      = new Color(0.18f, 0.80f, 0.75f);

        private static readonly Color COL_TAG_ONLINE = new Color(0.90f, 0.35f, 0.25f, 0.90f);
        private static readonly Color COL_TAG_LOCAL  = new Color(0.25f, 0.52f, 0.95f, 0.90f);

        private const int LOCAL_MIN_PLAYERS = 2;
        private const int LOCAL_MAX_PLAYERS = 4;

        #endregion

        #region Mode Data

        private struct ModeEntry
        {
            public IVXGameMode Mode;
            public string Title;
            public string Description;
            public string PlayersBadge;
            public Color Accent;
            public string Tag;
            public string IconLabel;
        }

        private static readonly ModeEntry[] MODES =
        {
            new ModeEntry { Mode = IVXGameMode.Solo,             Title = "SOLO PLAY",         Description = "Play alone against AI",       PlayersBadge = "1 Player",    Accent = COL_GREEN,  Tag = "",       IconLabel = "SOLO" },
            new ModeEntry { Mode = IVXGameMode.LocalMultiplayer, Title = "LOCAL MULTIPLAYER", Description = "Same device, pass and play",  PlayersBadge = "2-4 Players", Accent = COL_BLUE,   Tag = "LOCAL",  IconLabel = "LOCAL" },
            new ModeEntry { Mode = IVXGameMode.OnlineVersus,     Title = "ONLINE VERSUS",     Description = "Challenge players worldwide", PlayersBadge = "2 Players",   Accent = COL_RED,    Tag = "ONLINE", IconLabel = "VS" },
            new ModeEntry { Mode = IVXGameMode.OnlineCoop,       Title = "ONLINE CO-OP",      Description = "Team up with friends",        PlayersBadge = "2-4 Players", Accent = COL_PURPLE, Tag = "ONLINE", IconLabel = "COOP" },
            new ModeEntry { Mode = IVXGameMode.RankedMatch,      Title = "RANKED MATCH",      Description = "Competitive ranked play",     PlayersBadge = "2 Players",   Accent = COL_GOLD,   Tag = "ONLINE", IconLabel = "RANK" },
            new ModeEntry { Mode = IVXGameMode.TurnBased,        Title = "TURN BASED",        Description = "Take turns at your pace",     PlayersBadge = "2+ Players",  Accent = COL_TEAL,   Tag = "",       IconLabel = "TURN" },
        };

        #endregion

        #region Private Fields

        private int _selectedIndex = -1;
        private int _localPlayerCount = LOCAL_MIN_PLAYERS;
        private RectTransform _configPanel;
        private LayoutElement _configLE;
        private Image[] _cardBackgrounds;
        private readonly List<TMP_InputField> _nameInputs = new List<TMP_InputField>();
        private TextMeshProUGUI _playerCountLabel;

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
            var vlg = AddVLayout(root.gameObject, 0f, new RectOffset(0, 0, 0, 0));
            vlg.childControlHeight = true;
            vlg.childForceExpandHeight = false;

            BuildHeader(root);
            BuildCardScrollArea(root);
            BuildConfigPanel(root);
        }

        private void BuildHeader(RectTransform parent)
        {
            var header = MakePanel(parent, "Header", COL_HEADER);
            var le = header.gameObject.AddComponent<LayoutElement>();
            le.minHeight = 60f;
            le.preferredHeight = 60f;
            var pad = AddVLayout(header.gameObject, 0f, new RectOffset(24, 24, 0, 0));
            pad.childAlignment = TextAnchor.MiddleCenter;
            pad.childControlHeight = true;

            MakeTMP(header, "Title", "SELECT GAME MODE", 26f, FontStyles.Bold, COL_TEXT, TextAlignmentOptions.Center);
        }

        private void BuildCardScrollArea(RectTransform parent)
        {
            var scrollArea = MakeRect(parent, "ScrollArea");
            var saLE = scrollArea.gameObject.AddComponent<LayoutElement>();
            saLE.flexibleHeight = 1f;
            saLE.minHeight = 120f;
            scrollArea.gameObject.AddComponent<Image>().color = Color.clear;

            var viewport = MakeRect(scrollArea, "Viewport");
            Stretch(viewport);
            viewport.gameObject.AddComponent<Image>().color = Color.clear;
            viewport.gameObject.AddComponent<Mask>().showMaskGraphic = false;

            var content = MakeRect(viewport, "Content");
            content.anchorMin = new Vector2(0f, 1f);
            content.anchorMax = Vector2.one;
            content.pivot = new Vector2(0.5f, 1f);
            content.sizeDelta = Vector2.zero;
            var cvlg = AddVLayout(content.gameObject, 12f, new RectOffset(20, 20, 16, 16));
            cvlg.childForceExpandHeight = false;
            content.gameObject.AddComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            var scroll = scrollArea.gameObject.AddComponent<ScrollRect>();
            scroll.viewport = viewport;
            scroll.content = content;
            scroll.horizontal = false;
            scroll.vertical = true;
            scroll.movementType = ScrollRect.MovementType.Elastic;
            scroll.scrollSensitivity = 30f;

            _cardBackgrounds = new Image[MODES.Length];
            for (int i = 0; i < MODES.Length; i++)
                BuildModeCard(content, i);
        }

        private void BuildModeCard(RectTransform parent, int index)
        {
            var entry = MODES[index];

            var card = MakeRect(parent, $"Card_{entry.Mode}");
            card.gameObject.AddComponent<LayoutElement>().minHeight = 90f;
            _cardBackgrounds[index] = card.gameObject.AddComponent<Image>();
            _cardBackgrounds[index].color = COL_CARD;

            var btn = card.gameObject.AddComponent<Button>();
            btn.targetGraphic = _cardBackgrounds[index];
            var nav = btn.navigation;
            nav.mode = Navigation.Mode.None;
            btn.navigation = nav;
            int captured = index;
            btn.onClick.AddListener(() => OnCardSelected(captured));

            var hlg = card.gameObject.AddComponent<HorizontalLayoutGroup>();
            hlg.spacing = 0f;
            hlg.padding = new RectOffset(0, 12, 0, 0);
            hlg.childAlignment = TextAnchor.MiddleLeft;
            hlg.childControlWidth = true;
            hlg.childControlHeight = true;
            hlg.childForceExpandWidth = false;

            var accent = MakePanel(card, "Accent", entry.Accent);
            var accentLE = accent.gameObject.AddComponent<LayoutElement>();
            accentLE.minWidth = 5f;
            accentLE.preferredWidth = 5f;

            var iconArea = MakePanel(card, "IconArea", WithAlpha(entry.Accent, 0.12f));
            var iconLE = iconArea.gameObject.AddComponent<LayoutElement>();
            iconLE.minWidth = 66f;
            iconLE.preferredWidth = 66f;
            var iconVlg = AddVLayout(iconArea.gameObject, 0f, new RectOffset(4, 4, 8, 8));
            iconVlg.childAlignment = TextAnchor.MiddleCenter;
            iconVlg.childControlHeight = true;
            MakeTMP(iconArea, "IconLbl", entry.IconLabel, 14f, FontStyles.Bold, entry.Accent, TextAlignmentOptions.Center);

            var textCol = MakeRect(card, "TextCol");
            textCol.gameObject.AddComponent<LayoutElement>().flexibleWidth = 1f;
            var tcVlg = AddVLayout(textCol.gameObject, 3f, new RectOffset(14, 8, 10, 10));
            tcVlg.childAlignment = TextAnchor.MiddleLeft;

            var titleRow = MakeRect(textCol, "TitleRow");
            titleRow.gameObject.AddComponent<LayoutElement>().minHeight = 26f;
            var trHlg = titleRow.gameObject.AddComponent<HorizontalLayoutGroup>();
            trHlg.spacing = 10f;
            trHlg.childAlignment = TextAnchor.MiddleLeft;
            trHlg.childControlWidth = true;
            trHlg.childControlHeight = true;
            trHlg.childForceExpandWidth = false;

            MakeTMP(titleRow, "Title", entry.Title, 17f, FontStyles.Bold, COL_TEXT);

            if (!string.IsNullOrEmpty(entry.Tag))
            {
                Color tagCol = entry.Tag == "ONLINE" ? COL_TAG_ONLINE : COL_TAG_LOCAL;
                var tagBg = MakePanel(titleRow, "Tag", tagCol);
                var tagLE = tagBg.gameObject.AddComponent<LayoutElement>();
                tagLE.minWidth = 60f;
                tagLE.preferredWidth = 60f;
                tagLE.minHeight = 20f;
                var tagPad = AddVLayout(tagBg.gameObject, 0f, new RectOffset(6, 6, 1, 1));
                tagPad.childAlignment = TextAnchor.MiddleCenter;
                tagPad.childControlHeight = true;
                MakeTMP(tagBg, "TagText", entry.Tag, 10f, FontStyles.Bold, COL_TEXT, TextAlignmentOptions.Center);
            }

            MakeTMP(textCol, "Desc", entry.Description, 13f, FontStyles.Normal, COL_DIM);

            var badgeRow = MakeRect(textCol, "BadgeRow");
            badgeRow.gameObject.AddComponent<LayoutElement>().minHeight = 22f;
            var brHlg = badgeRow.gameObject.AddComponent<HorizontalLayoutGroup>();
            brHlg.childAlignment = TextAnchor.MiddleLeft;
            brHlg.childControlWidth = true;
            brHlg.childControlHeight = true;
            brHlg.childForceExpandWidth = false;

            var badgeBg = MakePanel(badgeRow, "Badge", WithAlpha(entry.Accent, 0.20f));
            var badgeLE = badgeBg.gameObject.AddComponent<LayoutElement>();
            badgeLE.minWidth = 84f;
            badgeLE.preferredWidth = 84f;
            badgeLE.minHeight = 20f;
            var bPad = AddVLayout(badgeBg.gameObject, 0f, new RectOffset(8, 8, 2, 2));
            bPad.childAlignment = TextAnchor.MiddleCenter;
            bPad.childControlHeight = true;
            MakeTMP(badgeBg, "BadgeText", entry.PlayersBadge, 11f, FontStyles.Bold, entry.Accent, TextAlignmentOptions.Center);
        }

        private void BuildConfigPanel(RectTransform parent)
        {
            _configPanel = MakePanel(parent, "ConfigPanel", COL_CONFIG);
            _configLE = _configPanel.gameObject.AddComponent<LayoutElement>();
            _configLE.minHeight = 100f;
            _configLE.preferredHeight = 100f;
            var vlg = AddVLayout(_configPanel.gameObject, 10f, new RectOffset(24, 24, 14, 14));
            vlg.childForceExpandHeight = false;
            vlg.childAlignment = TextAnchor.MiddleCenter;

            MakeTMP(_configPanel, "Prompt", "Select a game mode above", 15f, FontStyles.Italic, COL_DIM, TextAlignmentOptions.Center)
                .gameObject.AddComponent<LayoutElement>().minHeight = 28f;
        }

        #endregion

        #region Selection Logic

        private void OnCardSelected(int index)
        {
            for (int i = 0; i < _cardBackgrounds.Length; i++)
                _cardBackgrounds[i].color = i == index ? COL_SELECTED : COL_CARD;

            _selectedIndex = index;
            _localPlayerCount = LOCAL_MIN_PLAYERS;

            IVXGameModeManager.Instance.SelectMode(MODES[index].Mode);
            RebuildConfigPanel(MODES[index]);
        }

        private void RebuildConfigPanel(ModeEntry entry)
        {
            for (int i = _configPanel.childCount - 1; i >= 0; i--)
                Destroy(_configPanel.GetChild(i).gameObject);
            _nameInputs.Clear();

            MakeTMP(_configPanel, "CfgTitle", entry.Title + " -- Configuration", 15f, FontStyles.Bold, entry.Accent, TextAlignmentOptions.Center)
                .gameObject.AddComponent<LayoutElement>().minHeight = 26f;

            switch (entry.Mode)
            {
                case IVXGameMode.Solo:
                    SetConfigHeight(108f);
                    BuildSoloConfig(entry);
                    break;
                case IVXGameMode.LocalMultiplayer:
                    SetConfigHeight(130f + _localPlayerCount * 40f);
                    BuildLocalConfig(entry);
                    break;
                default:
                    SetConfigHeight(112f);
                    BuildOnlineConfig(entry);
                    break;
            }
        }

        private void SetConfigHeight(float h)
        {
            _configLE.minHeight = h;
            _configLE.preferredHeight = h;
        }

        private void BuildSoloConfig(ModeEntry entry)
        {
            MakeButton(_configPanel, "StartBtn", "START GAME", entry.Accent, COL_TEXT, 46f, OnStartGame);
        }

        private void BuildLocalConfig(ModeEntry entry)
        {
            var countRow = MakeRect(_configPanel, "CountRow");
            countRow.gameObject.AddComponent<LayoutElement>().minHeight = 36f;
            var crHlg = countRow.gameObject.AddComponent<HorizontalLayoutGroup>();
            crHlg.spacing = 10f;
            crHlg.childAlignment = TextAnchor.MiddleCenter;
            crHlg.childControlWidth = true;
            crHlg.childControlHeight = true;
            crHlg.childForceExpandWidth = false;

            MakeTMP(countRow, "Lbl", "Players:", 14f, FontStyles.Bold, COL_TEXT);
            MakeButton(countRow, "Minus", "-", COL_BTN_ALT, COL_TEXT, 30f, OnDecrementPlayers, 30f);

            _playerCountLabel = MakeTMP(countRow, "Num", _localPlayerCount.ToString(), 18f, FontStyles.Bold, COL_TEXT, TextAlignmentOptions.Center);
            _playerCountLabel.gameObject.AddComponent<LayoutElement>().minWidth = 28f;

            MakeButton(countRow, "Plus", "+", COL_BTN_ALT, COL_TEXT, 30f, OnIncrementPlayers, 30f);

            for (int i = 0; i < _localPlayerCount; i++)
            {
                var row = MakeRect(_configPanel, $"NameRow{i}");
                row.gameObject.AddComponent<LayoutElement>().minHeight = 32f;
                var rhlg = row.gameObject.AddComponent<HorizontalLayoutGroup>();
                rhlg.spacing = 8f;
                rhlg.childAlignment = TextAnchor.MiddleLeft;
                rhlg.childControlWidth = true;
                rhlg.childControlHeight = true;
                rhlg.childForceExpandWidth = false;

                var lbl = MakeTMP(row, "PLbl", $"P{i + 1}:", 13f, FontStyles.Bold, COL_DIM);
                lbl.gameObject.AddComponent<LayoutElement>().minWidth = 34f;
                _nameInputs.Add(MakeInputField(row, $"Name{i}", $"Player {i + 1}"));
            }

            MakeButton(_configPanel, "StartBtn", "START", entry.Accent, COL_TEXT, 44f, OnStartGame);
        }

        private void BuildOnlineConfig(ModeEntry entry)
        {
            var btnRow = MakeRect(_configPanel, "BtnRow");
            btnRow.gameObject.AddComponent<LayoutElement>().minHeight = 46f;
            var brHlg = btnRow.gameObject.AddComponent<HorizontalLayoutGroup>();
            brHlg.spacing = 14f;
            brHlg.childAlignment = TextAnchor.MiddleCenter;
            brHlg.childControlWidth = true;
            brHlg.childControlHeight = true;
            brHlg.childForceExpandWidth = true;

            MakeButton(btnRow, "QuickMatch", "QUICK MATCH", entry.Accent, COL_TEXT, 46f, OnQuickMatch);
            MakeButton(btnRow, "BrowseRooms", "BROWSE ROOMS", COL_BTN_ALT, COL_TEXT, 46f, OnBrowseRooms);
        }

        #endregion

        #region Button Callbacks

        private void OnStartGame()
        {
            if (_selectedIndex < 0) return;
            var entry = MODES[_selectedIndex];

            if (entry.Mode == IVXGameMode.LocalMultiplayer)
            {
                for (int i = 0; i < _nameInputs.Count; i++)
                {
                    string playerName = string.IsNullOrWhiteSpace(_nameInputs[i].text)
                        ? $"Player {i + 1}"
                        : _nameInputs[i].text;

                    if (i == 0)
                    {
                        var local = IVXGameModeManager.Instance.LocalPlayer;
                        if (local != null) local.DisplayName = playerName;
                    }
                    else
                    {
                        IVXGameModeManager.Instance.AddLocalPlayer(playerName, i);
                    }
                }
            }

            IVXGameModeManager.Instance.StartMatch();
            Debug.Log($"[{nameof(IVXGameModeSelectorDemo)}] Starting {entry.Title}");
        }

        private void OnQuickMatch()
        {
            if (_selectedIndex < 0) return;
            Debug.Log($"[{nameof(IVXGameModeSelectorDemo)}] Quick Match: {MODES[_selectedIndex].Title}");
        }

        private void OnBrowseRooms()
        {
            if (_selectedIndex < 0) return;
            Debug.Log($"[{nameof(IVXGameModeSelectorDemo)}] Browse Rooms: {MODES[_selectedIndex].Title}");
        }

        private void OnIncrementPlayers()
        {
            if (_localPlayerCount >= LOCAL_MAX_PLAYERS || _selectedIndex < 0) return;
            _localPlayerCount++;
            RebuildConfigPanel(MODES[_selectedIndex]);
        }

        private void OnDecrementPlayers()
        {
            if (_localPlayerCount <= LOCAL_MIN_PLAYERS || _selectedIndex < 0) return;
            _localPlayerCount--;
            RebuildConfigPanel(MODES[_selectedIndex]);
        }

        #endregion

        #region Primitives

        private static RectTransform MakeRect(Transform p, string n)
        {
            var go = new GameObject(n, typeof(RectTransform));
            go.transform.SetParent(p, false);
            return go.GetComponent<RectTransform>();
        }

        private static RectTransform MakePanel(Transform p, string n, Color c)
        {
            var r = MakeRect(p, n);
            r.gameObject.AddComponent<Image>().color = c;
            return r;
        }

        private static TextMeshProUGUI MakeTMP(RectTransform p, string n, string t, float s, FontStyles st, Color c,
            TextAlignmentOptions a = TextAlignmentOptions.MidlineLeft)
        {
            var r = MakeRect(p, n);
            var tmp = r.gameObject.AddComponent<TextMeshProUGUI>();
            tmp.text = t;
            tmp.fontSize = s;
            tmp.fontStyle = st;
            tmp.color = c;
            tmp.alignment = a;
            tmp.raycastTarget = false;
            return tmp;
        }

        private static VerticalLayoutGroup AddVLayout(GameObject go, float sp, RectOffset pad)
        {
            var v = go.AddComponent<VerticalLayoutGroup>();
            v.spacing = sp;
            v.padding = pad;
            v.childAlignment = TextAnchor.UpperLeft;
            v.childControlWidth = true;
            v.childControlHeight = false;
            v.childForceExpandWidth = true;
            v.childForceExpandHeight = false;
            return v;
        }

        private static void Stretch(RectTransform r, float pad = 0f)
        {
            r.anchorMin = Vector2.zero;
            r.anchorMax = Vector2.one;
            r.offsetMin = new Vector2(pad, pad);
            r.offsetMax = new Vector2(-pad, -pad);
        }

        private static Color WithAlpha(Color c, float a)
        {
            return new Color(c.r, c.g, c.b, a);
        }

        private static Button MakeButton(RectTransform parent, string name, string label, Color bgColor,
            Color textColor, float height, UnityEngine.Events.UnityAction onClick, float width = -1f)
        {
            var rt = MakeRect(parent, name);
            var le = rt.gameObject.AddComponent<LayoutElement>();
            le.minHeight = height;
            if (width > 0f)
            {
                le.minWidth = width;
                le.preferredWidth = width;
            }

            var img = rt.gameObject.AddComponent<Image>();
            img.color = bgColor;

            var b = rt.gameObject.AddComponent<Button>();
            b.targetGraphic = img;
            var nav = b.navigation;
            nav.mode = Navigation.Mode.None;
            b.navigation = nav;
            b.onClick.AddListener(onClick);

            var lbl = MakeTMP(rt, "Lbl", label, 15f, FontStyles.Bold, textColor, TextAlignmentOptions.Center);
            Stretch(lbl.rectTransform);

            return b;
        }

        private static TMP_InputField MakeInputField(RectTransform parent, string name, string placeholder)
        {
            var root = MakeRect(parent, name);
            var le = root.gameObject.AddComponent<LayoutElement>();
            le.minWidth = 160f;
            le.preferredWidth = 160f;
            le.minHeight = 28f;

            root.gameObject.AddComponent<Image>().color = COL_INPUT_BG;

            var textArea = MakeRect(root, "TextArea");
            Stretch(textArea, 6f);
            textArea.gameObject.AddComponent<RectMask2D>();

            var phRect = MakeRect(textArea, "Placeholder");
            Stretch(phRect);
            var phTmp = phRect.gameObject.AddComponent<TextMeshProUGUI>();
            phTmp.text = placeholder;
            phTmp.fontSize = 13f;
            phTmp.fontStyle = FontStyles.Italic;
            phTmp.color = COL_DIM;
            phTmp.alignment = TextAlignmentOptions.MidlineLeft;
            phTmp.raycastTarget = false;

            var txtRect = MakeRect(textArea, "Text");
            Stretch(txtRect);
            var txtTmp = txtRect.gameObject.AddComponent<TextMeshProUGUI>();
            txtTmp.text = "";
            txtTmp.fontSize = 13f;
            txtTmp.color = COL_TEXT;
            txtTmp.alignment = TextAlignmentOptions.MidlineLeft;
            txtTmp.raycastTarget = false;

            var input = root.gameObject.AddComponent<TMP_InputField>();
            input.textViewport = textArea;
            input.placeholder = phTmp;
            input.textComponent = txtTmp;
            input.fontAsset = txtTmp.font;
            input.pointSize = 13f;

            return input;
        }

        #endregion
    }
}
