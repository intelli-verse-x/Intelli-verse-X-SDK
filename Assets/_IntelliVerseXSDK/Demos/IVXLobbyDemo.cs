using System.Collections;
using IntelliVerseX.GameModes;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace IntelliVerseX.Demos
{
    /// <summary>
    /// Demo UI for the online lobby system.
    /// Builds a tabbed interface with room browsing, room creation, and matchmaking.
    /// Attach to a Canvas.
    /// </summary>
    public class IVXLobbyDemo : MonoBehaviour
    {
        #region Constants

        private static readonly Color COL_BG        = new Color(0.102f, 0.102f, 0.180f);
        private static readonly Color COL_CARD       = new Color(0.086f, 0.129f, 0.243f);
        private static readonly Color COL_ACCENT     = new Color(0.914f, 0.271f, 0.376f);
        private static readonly Color COL_SUCCESS    = new Color(0.180f, 0.835f, 0.451f);
        private static readonly Color COL_PANEL      = new Color(0.07f, 0.09f, 0.16f);
        private static readonly Color COL_TEXT       = new Color(0.92f, 0.93f, 0.95f);
        private static readonly Color COL_DIM        = new Color(0.50f, 0.53f, 0.60f);
        private static readonly Color COL_TAB_OFF    = new Color(0.12f, 0.14f, 0.22f);
        private static readonly Color COL_INPUT      = new Color(0.10f, 0.12f, 0.20f);
        private static readonly Color COL_VERSUS     = new Color(0.90f, 0.35f, 0.25f);
        private static readonly Color COL_COOP       = new Color(0.25f, 0.65f, 0.85f);
        private static readonly Color COL_TURNBASED  = new Color(0.70f, 0.55f, 0.20f);
        private static readonly Color COL_GRAYED     = new Color(0.30f, 0.32f, 0.38f);

        private static readonly IVXRoomInfo[] MOCK_ROOMS =
        {
            new() { RoomName = "Champions Arena",  HostName = "AlphaGamer",   PlayerCount = 2, MaxPlayers = 4, PingMs = 32,  Mode = IVXGameMode.OnlineVersus,  IsPasswordProtected = false, IsInProgress = false },
            new() { RoomName = "Casual Lounge",    HostName = "CoolDev42",    PlayerCount = 1, MaxPlayers = 4, PingMs = 58,  Mode = IVXGameMode.OnlineCoop,    IsPasswordProtected = false, IsInProgress = false },
            new() { RoomName = "Pro League",       HostName = "SpeedRunner",  PlayerCount = 2, MaxPlayers = 2, PingMs = 19,  Mode = IVXGameMode.OnlineVersus,  IsPasswordProtected = false, IsInProgress = true  },
            new() { RoomName = "Secret Vault",     HostName = "NoobMaster",   PlayerCount = 1, MaxPlayers = 3, PingMs = 71,  Mode = IVXGameMode.TurnBased,     IsPasswordProtected = true,  IsInProgress = false },
            new() { RoomName = "Quick Battle",     HostName = "ProPlayer99",  PlayerCount = 3, MaxPlayers = 4, PingMs = 45,  Mode = IVXGameMode.OnlineCoop,    IsPasswordProtected = false, IsInProgress = false },
            new() { RoomName = "Brain Bowl",       HostName = "StarFighter",  PlayerCount = 1, MaxPlayers = 2, PingMs = 88,  Mode = IVXGameMode.TurnBased,     IsPasswordProtected = false, IsInProgress = false },
            new() { RoomName = "Night Owls",       HostName = "MidnightOwl",  PlayerCount = 4, MaxPlayers = 4, PingMs = 105, Mode = IVXGameMode.OnlineCoop,    IsPasswordProtected = false, IsInProgress = true  },
            new() { RoomName = "Open Lobby",       HostName = "PixelHero",    PlayerCount = 1, MaxPlayers = 4, PingMs = 25,  Mode = IVXGameMode.OnlineVersus,  IsPasswordProtected = false, IsInProgress = false },
        };

        #endregion

        #region Private Fields

        private RectTransform _browsePanel;
        private RectTransform _createPanel;
        private RectTransform _matchPanel;

        private Image _tabBrowseImg;
        private Image _tabCreateImg;
        private Image _tabMatchImg;

        private TextMeshProUGUI _playerCountLabel;
        private TextMeshProUGUI _statusLabel;

        private IVXGameMode _selectedMode = IVXGameMode.OnlineVersus;
        private int _selectedMaxPlayers = 4;
        private bool _isPublic = true;
        private string _roomName = "My Room";

        private Image[] _modeBtnImages;
        private TextMeshProUGUI[] _modeBtnLabels;
        private Image[] _maxPBtnImages;
        private TextMeshProUGUI[] _maxPBtnLabels;
        private Image _publicBtnImg;
        private Image _privateBtnImg;
        private TextMeshProUGUI _publicBtnLbl;
        private TextMeshProUGUI _privateBtnLbl;

        private RectTransform _mmIdleGroup;
        private RectTransform _mmSearchGroup;
        private RectTransform _mmFoundGroup;
        private TextMeshProUGUI _searchText;
        private TextMeshProUGUI _searchTimer;
        private TextMeshProUGUI _oppName;
        private TextMeshProUGUI _oppRating;
        private TextMeshProUGUI _oppPing;
        private Coroutine _searchCoroutine;

        #endregion

        #region Unity Lifecycle

        private void Start()
        {
            BuildUI();
        }

        private void OnDestroy()
        {
            if (_searchCoroutine != null) StopCoroutine(_searchCoroutine);
        }

        #endregion

        #region UI Construction

        private void BuildUI()
        {
            var bg = MakePanel(transform, "BG", COL_BG);
            Stretch(bg);

            var root = MakeRect(transform, "Root");
            Stretch(root);
            var vlg = AddVLayout(root.gameObject, 8f, new RectOffset(16, 16, 20, 16));
            vlg.childForceExpandHeight = false;

            BuildHeader(root);
            BuildTabBar(root);
            BuildTabContent(root);
            BuildStatusBar(root);

            SelectTab(0);
        }

        private void BuildHeader(RectTransform parent)
        {
            var header = MakeRect(parent, "Header");
            header.gameObject.AddComponent<LayoutElement>().minHeight = 52f;
            var hlg = header.gameObject.AddComponent<HorizontalLayoutGroup>();
            hlg.spacing = 10f;
            hlg.padding = new RectOffset(4, 4, 4, 4);
            hlg.childAlignment = TextAnchor.MiddleLeft;
            hlg.childControlWidth = false;
            hlg.childControlHeight = true;
            hlg.childForceExpandWidth = false;

            MakeTMP(header, "Title", "ONLINE LOBBY", 26f, FontStyles.Bold, COL_ACCENT);

            var spacer = MakeRect(header, "Spacer");
            spacer.gameObject.AddComponent<LayoutElement>().flexibleWidth = 1f;

            _playerCountLabel = MakeTMP(header, "Count", "24 Online", 14f, FontStyles.Normal, COL_SUCCESS);
            _playerCountLabel.gameObject.AddComponent<LayoutElement>().SetLayout(100f, 32f);
            _playerCountLabel.alignment = TextAlignmentOptions.MidlineRight;

            var refreshBtn = MakeButton(header, "RefreshBtn", "Refresh", COL_CARD, COL_TEXT, 80f, 36f);
            refreshBtn.onClick.AddListener(() =>
            {
                int online = Random.Range(12, 48);
                _playerCountLabel.text = $"{online} Online";
                ShowStatus("Room list refreshed");
            });
        }

        private void BuildTabBar(RectTransform parent)
        {
            var bar = MakeRect(parent, "TabBar");
            bar.gameObject.AddComponent<LayoutElement>().minHeight = 40f;
            var hlg = bar.gameObject.AddComponent<HorizontalLayoutGroup>();
            hlg.spacing = 4f;
            hlg.childAlignment = TextAnchor.MiddleCenter;
            hlg.childControlWidth = true;
            hlg.childControlHeight = true;
            hlg.childForceExpandWidth = true;

            _tabBrowseImg = CreateTabButton(bar, "TabBrowse", "BROWSE ROOMS", () => SelectTab(0));
            _tabCreateImg = CreateTabButton(bar, "TabCreate", "CREATE ROOM", () => SelectTab(1));
            _tabMatchImg = CreateTabButton(bar, "TabMatch", "MATCHMAKING", () => SelectTab(2));
        }

        private Image CreateTabButton(RectTransform parent, string goName, string label,
            UnityEngine.Events.UnityAction onClick)
        {
            var go = MakeRect(parent, goName);
            var img = go.gameObject.AddComponent<Image>();
            img.color = COL_TAB_OFF;
            var btn = go.gameObject.AddComponent<Button>();
            btn.targetGraphic = img;
            btn.onClick.AddListener(onClick);
            var lbl = MakeTMP(go, "Lbl", label, 13f, FontStyles.Bold, COL_DIM, TextAlignmentOptions.Center);
            Stretch(lbl.rectTransform);
            return img;
        }

        private void BuildTabContent(RectTransform parent)
        {
            var content = MakeRect(parent, "TabContent");
            content.gameObject.AddComponent<LayoutElement>().flexibleHeight = 1f;

            _browsePanel = MakeRect(content, "BrowsePanel");
            Stretch(_browsePanel);
            _createPanel = MakeRect(content, "CreatePanel");
            Stretch(_createPanel);
            _matchPanel = MakeRect(content, "MatchPanel");
            Stretch(_matchPanel);

            BuildBrowsePanel(_browsePanel);
            BuildCreatePanel(_createPanel);
            BuildMatchmakingPanel(_matchPanel);
        }

        private void BuildStatusBar(RectTransform parent)
        {
            var bar = MakePanel(parent, "StatusBar", COL_PANEL);
            bar.gameObject.AddComponent<LayoutElement>().minHeight = 28f;
            var svlg = AddVLayout(bar.gameObject, 0f, new RectOffset(12, 12, 2, 2));
            svlg.childAlignment = TextAnchor.MiddleLeft;

            _statusLabel = MakeTMP(bar, "Status", "Ready", 12f, FontStyles.Italic, COL_DIM);
        }

        #endregion

        #region Browse Rooms Tab

        private void BuildBrowsePanel(RectTransform parent)
        {
            parent.gameObject.AddComponent<RectMask2D>();
            var scroll = parent.gameObject.AddComponent<ScrollRect>();
            scroll.horizontal = false;
            scroll.vertical = true;
            scroll.movementType = ScrollRect.MovementType.Elastic;

            var vp = MakeRect(parent, "VP");
            Stretch(vp);
            var content = MakeRect(vp, "Content");
            content.anchorMin = new Vector2(0, 1);
            content.anchorMax = new Vector2(1, 1);
            content.pivot = new Vector2(0.5f, 1f);
            var cvlg = AddVLayout(content.gameObject, 8f, new RectOffset(0, 0, 4, 12));
            cvlg.childForceExpandHeight = false;
            content.gameObject.AddComponent<ContentSizeFitter>().verticalFit =
                ContentSizeFitter.FitMode.PreferredSize;

            scroll.viewport = vp;
            scroll.content = content;

            foreach (var room in MOCK_ROOMS)
                CreateRoomCard(content, room);
        }

        private void CreateRoomCard(RectTransform parent, IVXRoomInfo room)
        {
            bool grayed = room.IsInProgress;
            Color cardCol = grayed ? COL_GRAYED : COL_CARD;

            var card = MakePanel(parent, "Room", cardCol);
            card.gameObject.AddComponent<LayoutElement>().minHeight = 82f;
            var hlg = card.gameObject.AddComponent<HorizontalLayoutGroup>();
            hlg.spacing = 12f;
            hlg.padding = new RectOffset(14, 14, 10, 10);
            hlg.childAlignment = TextAnchor.MiddleLeft;
            hlg.childControlWidth = false;
            hlg.childControlHeight = true;
            hlg.childForceExpandWidth = false;

            var info = MakeRect(card, "Info");
            info.gameObject.AddComponent<LayoutElement>().flexibleWidth = 1f;
            var ivlg = AddVLayout(info.gameObject, 3f, new RectOffset(0, 0, 0, 0));
            ivlg.childForceExpandHeight = false;

            var nameRow = MakeRect(info, "NameRow");
            var nhlg = nameRow.gameObject.AddComponent<HorizontalLayoutGroup>();
            nhlg.spacing = 6f;
            nhlg.childAlignment = TextAnchor.MiddleLeft;
            nhlg.childControlWidth = false;
            nhlg.childControlHeight = true;
            nhlg.childForceExpandWidth = false;
            nameRow.gameObject.AddComponent<ContentSizeFitter>().verticalFit =
                ContentSizeFitter.FitMode.PreferredSize;

            Color nameCol = grayed ? COL_DIM : COL_TEXT;
            MakeTMP(nameRow, "Name", room.RoomName, 16f, FontStyles.Bold, nameCol);

            if (room.IsPasswordProtected)
                MakeTMP(nameRow, "Lock", "[LOCK]", 11f, FontStyles.Bold, COL_ACCENT);

            if (grayed)
                MakeTMP(nameRow, "Prog", "[IN PROGRESS]", 10f, FontStyles.Bold, COL_DIM);

            MakeTMP(info, "Host", $"Host: {room.HostName}", 12f, FontStyles.Normal, COL_DIM);

            Color modeCol = GetModeColor(room.Mode);
            string modeStr = GetModeLabel(room.Mode);

            var badgeRow = MakeRect(info, "BadgeRow");
            badgeRow.gameObject.AddComponent<ContentSizeFitter>().verticalFit =
                ContentSizeFitter.FitMode.PreferredSize;
            var brhlg = badgeRow.gameObject.AddComponent<HorizontalLayoutGroup>();
            brhlg.spacing = 0f;
            brhlg.childAlignment = TextAnchor.MiddleLeft;
            brhlg.childControlWidth = false;
            brhlg.childControlHeight = true;
            brhlg.childForceExpandWidth = false;

            var badge = MakePanel(badgeRow, "Badge", grayed ? COL_GRAYED : modeCol);
            badge.gameObject.AddComponent<LayoutElement>().SetLayout(modeStr.Length * 7f + 16f, 20f);
            var bvlg = AddVLayout(badge.gameObject, 0f, new RectOffset(8, 8, 1, 1));
            bvlg.childAlignment = TextAnchor.MiddleCenter;
            MakeTMP(badge, "ML", modeStr, 10f, FontStyles.Bold, COL_TEXT, TextAlignmentOptions.Center);

            var countGo = MakeRect(card, "Count");
            countGo.gameObject.AddComponent<LayoutElement>().SetLayout(50f, 60f);
            var cvlg2 = AddVLayout(countGo.gameObject, 2f, new RectOffset(0, 0, 0, 0));
            cvlg2.childAlignment = TextAnchor.MiddleCenter;
            cvlg2.childForceExpandHeight = false;

            bool full = room.PlayerCount >= room.MaxPlayers;
            Color countCol = full ? COL_ACCENT : COL_TEXT;
            MakeTMP(countGo, "PC", $"{room.PlayerCount}/{room.MaxPlayers}", 18f, FontStyles.Bold,
                grayed ? COL_DIM : countCol, TextAlignmentOptions.Center);
            MakeTMP(countGo, "PL", "players", 10f, FontStyles.Normal, COL_DIM, TextAlignmentOptions.Center);

            var pingGo = MakeRect(card, "Ping");
            pingGo.gameObject.AddComponent<LayoutElement>().SetLayout(50f, 60f);
            var pvlg = AddVLayout(pingGo.gameObject, 2f, new RectOffset(0, 0, 0, 0));
            pvlg.childAlignment = TextAnchor.MiddleCenter;
            pvlg.childForceExpandHeight = false;

            Color pingCol = room.PingMs < 50 ? COL_SUCCESS : room.PingMs < 80 ? COL_TEXT : COL_ACCENT;
            MakeTMP(pingGo, "PV", $"{room.PingMs}", 16f, FontStyles.Bold,
                grayed ? COL_DIM : pingCol, TextAlignmentOptions.Center);
            MakeTMP(pingGo, "PM", "ms", 10f, FontStyles.Normal, COL_DIM, TextAlignmentOptions.Center);

            bool canJoin = !grayed && room.HasSpace;
            Color joinCol = canJoin ? COL_ACCENT : COL_GRAYED;
            string joinLabel = grayed ? "LIVE" : (full ? "FULL" : "JOIN");
            var joinBtn = MakeButton(card, "Join", joinLabel, joinCol, COL_TEXT, 64f, 36f);

            if (!canJoin)
            {
                joinBtn.interactable = false;
            }
            else
            {
                string rn = room.RoomName;
                joinBtn.onClick.AddListener(() => ShowStatus($"Joining \"{rn}\"..."));
            }
        }

        #endregion

        #region Create Room Tab

        private void BuildCreatePanel(RectTransform parent)
        {
            var vlg = AddVLayout(parent.gameObject, 12f, new RectOffset(8, 8, 12, 12));
            vlg.childForceExpandHeight = false;

            MakeTMP(parent, "RnLbl", "ROOM NAME", 12f, FontStyles.Bold, COL_DIM);
            BuildInputField(parent);

            MakeTMP(parent, "ModeLbl", "MODE", 12f, FontStyles.Bold, COL_DIM);
            BuildModeSelector(parent);

            MakeTMP(parent, "MpLbl", "MAX PLAYERS", 12f, FontStyles.Bold, COL_DIM);
            BuildMaxPlayersSelector(parent);

            MakeTMP(parent, "VisLbl", "VISIBILITY", 12f, FontStyles.Bold, COL_DIM);
            BuildVisibilityToggle(parent);

            var spacer = MakeRect(parent, "Spacer");
            spacer.gameObject.AddComponent<LayoutElement>().flexibleHeight = 1f;

            var createBtn = MakeButton(parent, "CreateBtn", "CREATE ROOM", COL_ACCENT, COL_TEXT, -1f, 52f);
            createBtn.gameObject.GetComponent<LayoutElement>().flexibleWidth = 1f;
            createBtn.onClick.AddListener(OnCreateRoomClicked);

            RefreshModeButtons();
            RefreshMaxPButtons();
            RefreshVisButtons();
        }

        private void BuildInputField(RectTransform parent)
        {
            var inputGo = MakePanel(parent, "RnInput", COL_INPUT);
            inputGo.gameObject.AddComponent<LayoutElement>().minHeight = 44f;

            var inputField = inputGo.gameObject.AddComponent<TMP_InputField>();

            var textArea = MakeRect(inputGo, "TextArea");
            Stretch(textArea, 10f);
            textArea.gameObject.AddComponent<RectMask2D>();

            var placeholder = MakeTMP(textArea, "Placeholder", "Enter room name...", 15f, FontStyles.Italic, COL_DIM);
            Stretch(placeholder.rectTransform);
            placeholder.alignment = TextAlignmentOptions.MidlineLeft;
            placeholder.raycastTarget = true;

            var inputText = MakeTMP(textArea, "Text", "", 15f, FontStyles.Normal, COL_TEXT);
            Stretch(inputText.rectTransform);
            inputText.alignment = TextAlignmentOptions.MidlineLeft;
            inputText.raycastTarget = true;

            inputField.textViewport = textArea;
            inputField.textComponent = inputText;
            inputField.placeholder = placeholder;
            inputField.text = _roomName;
            inputField.onValueChanged.AddListener(v => _roomName = v);
        }

        private void BuildModeSelector(RectTransform parent)
        {
            var modeRow = MakeRect(parent, "ModeRow");
            modeRow.gameObject.AddComponent<LayoutElement>().minHeight = 38f;
            var mhlg = modeRow.gameObject.AddComponent<HorizontalLayoutGroup>();
            mhlg.spacing = 6f;
            mhlg.childControlWidth = true;
            mhlg.childControlHeight = true;
            mhlg.childForceExpandWidth = true;

            var modes = new[] { IVXGameMode.OnlineVersus, IVXGameMode.OnlineCoop, IVXGameMode.TurnBased };
            var modeLabels = new[] { "Versus", "Co-op", "Turn-Based" };
            _modeBtnImages = new Image[3];
            _modeBtnLabels = new TextMeshProUGUI[3];

            for (int i = 0; i < 3; i++)
            {
                int idx = i;
                var mGo = MakeRect(modeRow, $"Mode{i}");
                _modeBtnImages[i] = mGo.gameObject.AddComponent<Image>();
                var mBtn = mGo.gameObject.AddComponent<Button>();
                mBtn.targetGraphic = _modeBtnImages[i];
                _modeBtnLabels[i] = MakeTMP(mGo, "Lbl", modeLabels[i], 13f, FontStyles.Bold, COL_TEXT,
                    TextAlignmentOptions.Center);
                Stretch(_modeBtnLabels[i].rectTransform);
                mBtn.onClick.AddListener(() =>
                {
                    _selectedMode = modes[idx];
                    RefreshModeButtons();
                });
            }
        }

        private void BuildMaxPlayersSelector(RectTransform parent)
        {
            var mpRow = MakeRect(parent, "MpRow");
            mpRow.gameObject.AddComponent<LayoutElement>().minHeight = 38f;
            var mphlg = mpRow.gameObject.AddComponent<HorizontalLayoutGroup>();
            mphlg.spacing = 6f;
            mphlg.childControlWidth = true;
            mphlg.childControlHeight = true;
            mphlg.childForceExpandWidth = true;

            var counts = new[] { 2, 3, 4 };
            _maxPBtnImages = new Image[3];
            _maxPBtnLabels = new TextMeshProUGUI[3];

            for (int i = 0; i < 3; i++)
            {
                int idx = i;
                var pGo = MakeRect(mpRow, $"MP{counts[i]}");
                _maxPBtnImages[i] = pGo.gameObject.AddComponent<Image>();
                var pBtn = pGo.gameObject.AddComponent<Button>();
                pBtn.targetGraphic = _maxPBtnImages[i];
                _maxPBtnLabels[i] = MakeTMP(pGo, "Lbl", counts[i].ToString(), 15f, FontStyles.Bold, COL_TEXT,
                    TextAlignmentOptions.Center);
                Stretch(_maxPBtnLabels[i].rectTransform);
                pBtn.onClick.AddListener(() =>
                {
                    _selectedMaxPlayers = counts[idx];
                    RefreshMaxPButtons();
                });
            }
        }

        private void BuildVisibilityToggle(RectTransform parent)
        {
            var visRow = MakeRect(parent, "VisRow");
            visRow.gameObject.AddComponent<LayoutElement>().minHeight = 38f;
            var vhlg = visRow.gameObject.AddComponent<HorizontalLayoutGroup>();
            vhlg.spacing = 6f;
            vhlg.childControlWidth = true;
            vhlg.childControlHeight = true;
            vhlg.childForceExpandWidth = true;

            var pubGo = MakeRect(visRow, "Public");
            _publicBtnImg = pubGo.gameObject.AddComponent<Image>();
            var pubBtn = pubGo.gameObject.AddComponent<Button>();
            pubBtn.targetGraphic = _publicBtnImg;
            _publicBtnLbl = MakeTMP(pubGo, "Lbl", "Public", 13f, FontStyles.Bold, COL_TEXT,
                TextAlignmentOptions.Center);
            Stretch(_publicBtnLbl.rectTransform);
            pubBtn.onClick.AddListener(() =>
            {
                _isPublic = true;
                RefreshVisButtons();
            });

            var privGo = MakeRect(visRow, "Private");
            _privateBtnImg = privGo.gameObject.AddComponent<Image>();
            var privBtn = privGo.gameObject.AddComponent<Button>();
            privBtn.targetGraphic = _privateBtnImg;
            _privateBtnLbl = MakeTMP(privGo, "Lbl", "Private", 13f, FontStyles.Bold, COL_TEXT,
                TextAlignmentOptions.Center);
            Stretch(_privateBtnLbl.rectTransform);
            privBtn.onClick.AddListener(() =>
            {
                _isPublic = false;
                RefreshVisButtons();
            });
        }

        private void RefreshModeButtons()
        {
            var modes = new[] { IVXGameMode.OnlineVersus, IVXGameMode.OnlineCoop, IVXGameMode.TurnBased };
            for (int i = 0; i < 3; i++)
            {
                bool sel = modes[i] == _selectedMode;
                _modeBtnImages[i].color = sel ? COL_ACCENT : COL_CARD;
                _modeBtnLabels[i].color = sel ? COL_TEXT : COL_DIM;
            }
        }

        private void RefreshMaxPButtons()
        {
            var counts = new[] { 2, 3, 4 };
            for (int i = 0; i < 3; i++)
            {
                bool sel = counts[i] == _selectedMaxPlayers;
                _maxPBtnImages[i].color = sel ? COL_ACCENT : COL_CARD;
                _maxPBtnLabels[i].color = sel ? COL_TEXT : COL_DIM;
            }
        }

        private void RefreshVisButtons()
        {
            _publicBtnImg.color = _isPublic ? COL_ACCENT : COL_CARD;
            _publicBtnLbl.color = _isPublic ? COL_TEXT : COL_DIM;
            _privateBtnImg.color = _isPublic ? COL_CARD : COL_ACCENT;
            _privateBtnLbl.color = _isPublic ? COL_DIM : COL_TEXT;
        }

        private void OnCreateRoomClicked()
        {
            string name = string.IsNullOrWhiteSpace(_roomName) ? "My Room" : _roomName;
            string vis = _isPublic ? "Public" : "Private";
            ShowStatus($"Created \"{name}\" - {GetModeLabel(_selectedMode)}, {_selectedMaxPlayers}P, {vis}");
        }

        #endregion

        #region Matchmaking Tab

        private void BuildMatchmakingPanel(RectTransform parent)
        {
            var vlg = AddVLayout(parent.gameObject, 0f, new RectOffset(0, 0, 0, 0));
            vlg.childAlignment = TextAnchor.MiddleCenter;
            vlg.childForceExpandHeight = true;

            BuildMatchIdleGroup(parent);
            BuildMatchSearchGroup(parent);
            BuildMatchFoundGroup(parent);

            SetMatchmakingState(0);
        }

        private void BuildMatchIdleGroup(RectTransform parent)
        {
            _mmIdleGroup = MakeRect(parent, "IdleGroup");
            _mmIdleGroup.gameObject.AddComponent<LayoutElement>().flexibleHeight = 1f;
            var idleVlg = AddVLayout(_mmIdleGroup.gameObject, 12f, new RectOffset(40, 40, 0, 0));
            idleVlg.childAlignment = TextAnchor.MiddleCenter;
            idleVlg.childForceExpandHeight = false;

            MakeTMP(_mmIdleGroup, "Title", "QUICK MATCH", 28f, FontStyles.Bold, COL_TEXT,
                TextAlignmentOptions.Center);
            MakeTMP(_mmIdleGroup, "Desc", "Find an opponent of similar skill", 14f, FontStyles.Normal, COL_DIM,
                TextAlignmentOptions.Center);

            MakeRect(_mmIdleGroup, "Sp").gameObject.AddComponent<LayoutElement>().minHeight = 20f;

            var findBtn = MakeButton(_mmIdleGroup, "FindBtn", "FIND MATCH", COL_ACCENT, COL_TEXT, -1f, 56f);
            findBtn.gameObject.GetComponent<LayoutElement>().flexibleWidth = 1f;
            findBtn.onClick.AddListener(OnFindMatchClicked);
        }

        private void BuildMatchSearchGroup(RectTransform parent)
        {
            _mmSearchGroup = MakeRect(parent, "SearchGroup");
            _mmSearchGroup.gameObject.AddComponent<LayoutElement>().flexibleHeight = 1f;
            var srchVlg = AddVLayout(_mmSearchGroup.gameObject, 10f, new RectOffset(40, 40, 0, 0));
            srchVlg.childAlignment = TextAnchor.MiddleCenter;
            srchVlg.childForceExpandHeight = false;

            _searchText = MakeTMP(_mmSearchGroup, "SrchTxt", "Searching...", 24f, FontStyles.Bold, COL_TEXT,
                TextAlignmentOptions.Center);
            _searchTimer = MakeTMP(_mmSearchGroup, "Timer", "0:00", 18f, FontStyles.Normal, COL_DIM,
                TextAlignmentOptions.Center);

            MakeRect(_mmSearchGroup, "Sp").gameObject.AddComponent<LayoutElement>().minHeight = 16f;

            var cancelBtn = MakeButton(_mmSearchGroup, "CancelBtn", "CANCEL", COL_CARD, COL_ACCENT, -1f, 48f);
            cancelBtn.gameObject.GetComponent<LayoutElement>().flexibleWidth = 1f;
            cancelBtn.onClick.AddListener(OnCancelSearchClicked);
        }

        private void BuildMatchFoundGroup(RectTransform parent)
        {
            _mmFoundGroup = MakeRect(parent, "FoundGroup");
            _mmFoundGroup.gameObject.AddComponent<LayoutElement>().flexibleHeight = 1f;
            var foundVlg = AddVLayout(_mmFoundGroup.gameObject, 10f, new RectOffset(24, 24, 0, 0));
            foundVlg.childAlignment = TextAnchor.MiddleCenter;
            foundVlg.childForceExpandHeight = false;

            MakeTMP(_mmFoundGroup, "FTitle", "MATCH FOUND", 26f, FontStyles.Bold, COL_SUCCESS,
                TextAlignmentOptions.Center);

            MakeRect(_mmFoundGroup, "Sp1").gameObject.AddComponent<LayoutElement>().minHeight = 8f;

            var oppCard = MakePanel(_mmFoundGroup, "OppCard", COL_CARD);
            oppCard.gameObject.AddComponent<LayoutElement>().minHeight = 110f;
            var oVlg = AddVLayout(oppCard.gameObject, 6f, new RectOffset(20, 20, 14, 14));
            oVlg.childAlignment = TextAnchor.MiddleCenter;
            oVlg.childForceExpandHeight = false;

            _oppName = MakeTMP(oppCard, "OName", "BrainMaster", 20f, FontStyles.Bold, COL_TEXT,
                TextAlignmentOptions.Center);

            var statRow = MakeRect(oppCard, "StatRow");
            var sthlg = statRow.gameObject.AddComponent<HorizontalLayoutGroup>();
            sthlg.spacing = 24f;
            sthlg.childAlignment = TextAnchor.MiddleCenter;
            sthlg.childControlWidth = false;
            sthlg.childControlHeight = true;
            sthlg.childForceExpandWidth = false;
            statRow.gameObject.AddComponent<ContentSizeFitter>().verticalFit =
                ContentSizeFitter.FitMode.PreferredSize;

            _oppRating = MakeTMP(statRow, "Rating", "Rating: 1450", 14f, FontStyles.Normal, COL_DIM);
            _oppPing = MakeTMP(statRow, "Ping", "Ping: 34ms", 14f, FontStyles.Normal, COL_DIM);

            MakeRect(_mmFoundGroup, "Sp2").gameObject.AddComponent<LayoutElement>().minHeight = 8f;

            var acceptBtn = MakeButton(_mmFoundGroup, "AcceptBtn", "ACCEPT", COL_SUCCESS, COL_TEXT, -1f, 52f);
            acceptBtn.gameObject.GetComponent<LayoutElement>().flexibleWidth = 1f;
            acceptBtn.onClick.AddListener(() =>
            {
                ShowStatus("Match accepted! Loading...");
                SetMatchmakingState(0);
            });
        }

        private void SetMatchmakingState(int state)
        {
            _mmIdleGroup.gameObject.SetActive(state == 0);
            _mmSearchGroup.gameObject.SetActive(state == 1);
            _mmFoundGroup.gameObject.SetActive(state == 2);
        }

        private void OnFindMatchClicked()
        {
            SetMatchmakingState(1);
            if (_searchCoroutine != null) StopCoroutine(_searchCoroutine);
            _searchCoroutine = StartCoroutine(SearchRoutine());
        }

        private void OnCancelSearchClicked()
        {
            if (_searchCoroutine != null)
            {
                StopCoroutine(_searchCoroutine);
                _searchCoroutine = null;
            }

            SetMatchmakingState(0);
            ShowStatus("Search cancelled");
        }

        private IEnumerator SearchRoutine()
        {
            float elapsed = 0f;
            float matchTime = Random.Range(3f, 7f);

            while (elapsed < matchTime)
            {
                elapsed += Time.deltaTime;
                int dots = (Mathf.FloorToInt(elapsed * 2f) % 3) + 1;
                _searchText.text = "Searching" + new string('.', dots);
                int minutes = Mathf.FloorToInt(elapsed / 60f);
                int seconds = Mathf.FloorToInt(elapsed % 60f);
                _searchTimer.text = $"{minutes}:{seconds:D2}";
                yield return null;
            }

            var mockNames = new[] { "Challenger99", "SpeedDemon", "BrainMaster", "QuizKing", "NightOwl" };
            _oppName.text = mockNames[Random.Range(0, mockNames.Length)];
            _oppRating.text = $"Rating: {Random.Range(800, 2200)}";
            _oppPing.text = $"Ping: {Random.Range(20, 90)}ms";

            _searchCoroutine = null;
            SetMatchmakingState(2);
            ShowStatus("Opponent found!");
        }

        #endregion

        #region Tab Management

        private void SelectTab(int index)
        {
            _browsePanel.gameObject.SetActive(index == 0);
            _createPanel.gameObject.SetActive(index == 1);
            _matchPanel.gameObject.SetActive(index == 2);

            _tabBrowseImg.color = index == 0 ? COL_ACCENT : COL_TAB_OFF;
            _tabCreateImg.color = index == 1 ? COL_ACCENT : COL_TAB_OFF;
            _tabMatchImg.color = index == 2 ? COL_ACCENT : COL_TAB_OFF;

            SetTabLabelColor(_tabBrowseImg, index == 0);
            SetTabLabelColor(_tabCreateImg, index == 1);
            SetTabLabelColor(_tabMatchImg, index == 2);

            if (_searchCoroutine != null && index != 2)
            {
                StopCoroutine(_searchCoroutine);
                _searchCoroutine = null;
                SetMatchmakingState(0);
            }
        }

        private static void SetTabLabelColor(Image tabImg, bool active)
        {
            var lbl = tabImg.GetComponentInChildren<TextMeshProUGUI>();
            if (lbl != null) lbl.color = active ? COL_TEXT : COL_DIM;
        }

        #endregion

        #region Helpers

        private static Color GetModeColor(IVXGameMode mode)
        {
            return mode switch
            {
                IVXGameMode.OnlineVersus => COL_VERSUS,
                IVXGameMode.OnlineCoop   => COL_COOP,
                IVXGameMode.TurnBased    => COL_TURNBASED,
                _                        => COL_DIM
            };
        }

        private static string GetModeLabel(IVXGameMode mode)
        {
            return mode switch
            {
                IVXGameMode.OnlineVersus => "VERSUS",
                IVXGameMode.OnlineCoop   => "CO-OP",
                IVXGameMode.TurnBased    => "TURN-BASED",
                _                        => mode.ToString().ToUpper()
            };
        }

        private void ShowStatus(string message)
        {
            if (_statusLabel != null) _statusLabel.text = message;
        }

        private Button MakeButton(RectTransform parent, string goName, string label, Color bgCol, Color txtCol,
            float w, float h)
        {
            var go = MakeRect(parent, goName);
            var le = go.gameObject.AddComponent<LayoutElement>();
            if (w > 0) { le.minWidth = w; le.preferredWidth = w; }
            if (h > 0) { le.minHeight = h; le.preferredHeight = h; }

            var img = go.gameObject.AddComponent<Image>();
            img.color = bgCol;
            var btn = go.gameObject.AddComponent<Button>();
            btn.targetGraphic = img;
            var lbl = MakeTMP(go, "Lbl", label, 14f, FontStyles.Bold, txtCol, TextAlignmentOptions.Center);
            Stretch(lbl.rectTransform);
            return btn;
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

        #endregion
    }

    internal static class LobbyLayoutExt
    {
        public static void SetLayout(this LayoutElement le, float minW, float minH, float flexW = -1f,
            float flexH = -1f)
        {
            if (minW >= 0) { le.minWidth = minW; le.preferredWidth = minW; }
            if (minH >= 0) { le.minHeight = minH; le.preferredHeight = minH; }
            if (flexW >= 0) le.flexibleWidth = flexW;
            if (flexH >= 0) le.flexibleHeight = flexH;
        }
    }
}
