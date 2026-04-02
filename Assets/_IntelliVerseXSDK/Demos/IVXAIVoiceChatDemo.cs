using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using IntelliVerseX.AI;

namespace IntelliVerseX.Demos
{
    /// <summary>
    /// Self-contained demo UI for the AI Voice Chat system.
    /// Attach to any Canvas to get a full persona-based voice/text chat interface.
    /// Works with mock data when no backend is connected.
    /// </summary>
    public class IVXAIVoiceChatDemo : MonoBehaviour
    {
        #region Constants

        private static readonly Color COL_BG = new Color(0.06f, 0.07f, 0.10f);
        private static readonly Color COL_PANEL = new Color(0.11f, 0.13f, 0.18f);
        private static readonly Color COL_INPUT = new Color(0.15f, 0.17f, 0.23f);
        private static readonly Color COL_PRIMARY = new Color(0.30f, 0.55f, 0.95f);
        private static readonly Color COL_ACCENT = new Color(0.65f, 0.35f, 0.90f);
        private static readonly Color COL_USER_BUBBLE = new Color(0.22f, 0.45f, 0.80f);
        private static readonly Color COL_AI_BUBBLE = new Color(0.16f, 0.18f, 0.25f);
        private static readonly Color COL_DANGER = new Color(0.85f, 0.25f, 0.25f);
        private static readonly Color COL_TEXT = new Color(0.92f, 0.93f, 0.95f);
        private static readonly Color COL_DIM = new Color(0.55f, 0.58f, 0.64f);
        private static readonly Color COL_RECORD = new Color(0.90f, 0.25f, 0.30f);

        private static readonly string[] MOCK_PERSONAS = { "Fortune Teller", "AI Teacher", "Career Coach", "Story Teller", "Party Host", "Health Advisor" };
        private static readonly string[] MOCK_ICONS = { "\u2728", "\ud83d\udcda", "\ud83d\udcbc", "\ud83d\udcd6", "\ud83c\udf89", "\ud83d\udc9a" };

        #endregion

        #region Private Fields

        private RectTransform _root;
        private RectTransform _personaGrid;
        private RectTransform _chatPanel;
        private RectTransform _chatContent;
        private ScrollRect _chatScroll;
        private TMP_InputField _inputField;
        private TextMeshProUGUI _timerText;
        private TextMeshProUGUI _captionText;
        private TextMeshProUGUI _headerTitle;
        private Button _sendBtn;
        private Button _micBtn;
        private Button _backBtn;
        private Button _endBtn;
        private Image _micBtnImg;

        private bool _inSession;
        private bool _isRecording;
        private float _sessionStart;
        private int _sessionDuration = 180;
        private string _currentPersona;
        private readonly List<GameObject> _chatBubbles = new List<GameObject>();

        #endregion

        #region Unity Lifecycle

        private void Start()
        {
            BuildUI();
            ShowPersonaSelection();
        }

        private void Update()
        {
            if (!_inSession) return;

            float remaining = Mathf.Max(0, _sessionDuration - (Time.realtimeSinceStartup - _sessionStart));
            int min = (int)(remaining / 60f);
            int sec = (int)(remaining % 60f);
            _timerText.text = $"{min:D2}:{sec:D2}";

            if (remaining <= 0f) EndSession();
        }

        #endregion

        #region UI Construction

        private void BuildUI()
        {
            var bg = CreatePanel(transform, "BG", COL_BG);
            Stretch(bg);

            _root = CreateRect(transform, "Root");
            Stretch(_root);

            BuildPersonaGrid();
            BuildChatPanel();
        }

        private void BuildPersonaGrid()
        {
            _personaGrid = CreatePanel(_root, "PersonaGrid", COL_BG);
            Stretch(_personaGrid);
            var vlg = AddVLayout(_personaGrid.gameObject, 16f, new RectOffset(20, 20, 60, 20));
            vlg.childForceExpandHeight = false;

            var title = CreateTMP(_personaGrid, "Title", "Choose Your AI Persona", 28f, FontStyles.Bold, COL_TEXT, TextAlignmentOptions.Center);
            title.gameObject.AddComponent<LayoutElement>().minHeight = 44f;

            var subtitle = CreateTMP(_personaGrid, "Sub", "Select a persona to start a conversation", 16f, FontStyles.Normal, COL_DIM, TextAlignmentOptions.Center);
            subtitle.gameObject.AddComponent<LayoutElement>().minHeight = 28f;

            var gridHolder = CreateRect(_personaGrid, "GridHolder");
            gridHolder.gameObject.AddComponent<LayoutElement>().flexibleHeight = 1f;
            var grid = gridHolder.gameObject.AddComponent<GridLayoutGroup>();
            grid.cellSize = new Vector2(160f, 180f);
            grid.spacing = new Vector2(16f, 16f);
            grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            grid.constraintCount = 2;
            grid.childAlignment = TextAnchor.UpperCenter;
            grid.padding = new RectOffset(10, 10, 10, 10);

            for (int i = 0; i < MOCK_PERSONAS.Length; i++)
            {
                int idx = i;
                var card = CreatePanel(gridHolder, $"P_{i}", COL_PANEL);
                var cardVlg = AddVLayout(card.gameObject, 6f, new RectOffset(10, 10, 16, 12));
                cardVlg.childAlignment = TextAnchor.MiddleCenter;

                var icon = CreateTMP(card, "Icon", MOCK_ICONS[i], 40f, FontStyles.Normal, COL_PRIMARY, TextAlignmentOptions.Center);
                icon.gameObject.AddComponent<LayoutElement>().minHeight = 52f;
                var label = CreateTMP(card, "Name", MOCK_PERSONAS[i], 17f, FontStyles.Bold, COL_TEXT, TextAlignmentOptions.Center);
                label.gameObject.AddComponent<LayoutElement>().minHeight = 24f;
                var desc = CreateTMP(card, "Desc", "Tap to chat", 13f, FontStyles.Italic, COL_DIM, TextAlignmentOptions.Center);
                desc.gameObject.AddComponent<LayoutElement>().minHeight = 20f;

                var btn = card.gameObject.AddComponent<Button>();
                btn.targetGraphic = card.GetComponent<Image>();
                btn.onClick.AddListener(() => SelectPersona(MOCK_PERSONAS[idx]));
            }
        }

        private void BuildChatPanel()
        {
            _chatPanel = CreatePanel(_root, "ChatPanel", COL_BG);
            Stretch(_chatPanel);
            _chatPanel.gameObject.SetActive(false);
            var vlg = AddVLayout(_chatPanel.gameObject, 0f, new RectOffset(0, 0, 0, 0));
            vlg.childForceExpandHeight = false;

            // Header
            var header = CreatePanel(_chatPanel, "Header", COL_PANEL);
            header.gameObject.AddComponent<LayoutElement>().minHeight = 60f;
            var hh = header.gameObject.AddComponent<HorizontalLayoutGroup>();
            hh.spacing = 12f; hh.padding = new RectOffset(12, 12, 8, 8);
            hh.childAlignment = TextAnchor.MiddleLeft;
            hh.childControlWidth = false; hh.childControlHeight = true;
            hh.childForceExpandWidth = false;

            var backGo = CreateRect(header, "Back");
            backGo.sizeDelta = new Vector2(44f, 44f);
            var backImg = backGo.gameObject.AddComponent<Image>(); backImg.color = COL_INPUT;
            _backBtn = backGo.gameObject.AddComponent<Button>(); _backBtn.targetGraphic = backImg;
            var backLbl = CreateTMP(backGo, "Lbl", "\u25C0", 22f, FontStyles.Bold, COL_TEXT, TextAlignmentOptions.Center);
            Stretch(backLbl.rectTransform);
            _backBtn.onClick.AddListener(EndSession);

            _headerTitle = CreateTMP(header, "Title", "AI Chat", 22f, FontStyles.Bold, COL_TEXT);
            _headerTitle.gameObject.AddComponent<LayoutElement>().flexibleWidth = 1f;
            _timerText = CreateTMP(header, "Timer", "03:00", 18f, FontStyles.Normal, COL_ACCENT);
            _timerText.gameObject.AddComponent<LayoutElement>().minWidth = 60f;

            var endGo = CreateRect(header, "End");
            endGo.sizeDelta = new Vector2(44f, 44f);
            var endImg = endGo.gameObject.AddComponent<Image>(); endImg.color = COL_DANGER;
            _endBtn = endGo.gameObject.AddComponent<Button>(); _endBtn.targetGraphic = endImg;
            var endLbl = CreateTMP(endGo, "X", "\u2716", 20f, FontStyles.Bold, COL_TEXT, TextAlignmentOptions.Center);
            Stretch(endLbl.rectTransform);
            _endBtn.onClick.AddListener(EndSession);

            // Chat scroll area
            var scrollArea = CreateRect(_chatPanel, "ScrollArea");
            scrollArea.gameObject.AddComponent<LayoutElement>().flexibleHeight = 1f;
            scrollArea.gameObject.AddComponent<Image>().color = COL_BG;
            scrollArea.gameObject.AddComponent<RectMask2D>();
            _chatScroll = scrollArea.gameObject.AddComponent<ScrollRect>();
            _chatScroll.horizontal = false; _chatScroll.vertical = true;
            _chatScroll.movementType = ScrollRect.MovementType.Elastic;

            var viewport = CreateRect(scrollArea, "Viewport"); Stretch(viewport);
            _chatContent = CreateRect(viewport, "Content");
            _chatContent.anchorMin = new Vector2(0, 1); _chatContent.anchorMax = new Vector2(1, 1);
            _chatContent.pivot = new Vector2(0.5f, 1f);
            var cvlg = AddVLayout(_chatContent.gameObject, 8f, new RectOffset(12, 12, 12, 12));
            cvlg.childForceExpandHeight = false;
            var csf = _chatContent.gameObject.AddComponent<ContentSizeFitter>();
            csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            _chatScroll.viewport = viewport;
            _chatScroll.content = _chatContent;

            // Caption bar
            var captionBar = CreatePanel(_chatPanel, "CaptionBar", new Color(0.08f, 0.09f, 0.13f));
            captionBar.gameObject.AddComponent<LayoutElement>().minHeight = 36f;
            var cpad = AddVLayout(captionBar.gameObject, 0f, new RectOffset(16, 16, 6, 6));
            cpad.childAlignment = TextAnchor.MiddleLeft;
            _captionText = CreateTMP(captionBar, "Caption", "", 14f, FontStyles.Italic, COL_DIM);

            // Input bar
            var inputBar = CreatePanel(_chatPanel, "InputBar", COL_PANEL);
            inputBar.gameObject.AddComponent<LayoutElement>().minHeight = 64f;
            var ihlg = inputBar.gameObject.AddComponent<HorizontalLayoutGroup>();
            ihlg.spacing = 10f; ihlg.padding = new RectOffset(12, 12, 8, 8);
            ihlg.childAlignment = TextAnchor.MiddleCenter;
            ihlg.childControlWidth = true; ihlg.childControlHeight = true;
            ihlg.childForceExpandWidth = false;

            // Mic button
            var micGo = CreateRect(inputBar, "Mic");
            micGo.gameObject.AddComponent<LayoutElement>().SetLayout(48f, 48f);
            _micBtnImg = micGo.gameObject.AddComponent<Image>(); _micBtnImg.color = COL_INPUT;
            _micBtn = micGo.gameObject.AddComponent<Button>(); _micBtn.targetGraphic = _micBtnImg;
            var micLbl = CreateTMP(micGo, "Lbl", "\ud83c\udfa4", 22f, FontStyles.Normal, COL_TEXT, TextAlignmentOptions.Center);
            Stretch(micLbl.rectTransform);
            _micBtn.onClick.AddListener(ToggleMic);

            // Text input
            var inputGo = CreateRect(inputBar, "Input");
            inputGo.gameObject.AddComponent<LayoutElement>().flexibleWidth = 1f;
            var inputBg = inputGo.gameObject.AddComponent<Image>(); inputBg.color = COL_INPUT;
            var textArea = CreateRect(inputGo, "TextArea"); Stretch(textArea, 10f);
            textArea.gameObject.AddComponent<RectMask2D>();
            var txt = CreateRect(textArea, "Text"); Stretch(txt);
            var txtTmp = txt.gameObject.AddComponent<TextMeshProUGUI>();
            txtTmp.fontSize = 16f; txtTmp.color = COL_TEXT;
            var ph = CreateRect(textArea, "Placeholder"); Stretch(ph);
            var phTmp = ph.gameObject.AddComponent<TextMeshProUGUI>();
            phTmp.text = "Type a message..."; phTmp.fontSize = 16f; phTmp.fontStyle = FontStyles.Italic; phTmp.color = COL_DIM;
            _inputField = inputGo.gameObject.AddComponent<TMP_InputField>();
            _inputField.textViewport = textArea; _inputField.textComponent = txtTmp; _inputField.placeholder = phTmp;
            _inputField.targetGraphic = inputBg;

            // Send button
            var sendGo = CreateRect(inputBar, "Send");
            sendGo.gameObject.AddComponent<LayoutElement>().SetLayout(48f, 48f);
            var sendImg = sendGo.gameObject.AddComponent<Image>(); sendImg.color = COL_PRIMARY;
            _sendBtn = sendGo.gameObject.AddComponent<Button>(); _sendBtn.targetGraphic = sendImg;
            var sendLbl = CreateTMP(sendGo, "Lbl", "\u27A4", 22f, FontStyles.Bold, COL_TEXT, TextAlignmentOptions.Center);
            Stretch(sendLbl.rectTransform);
            _sendBtn.onClick.AddListener(OnSend);
        }

        #endregion

        #region Session Logic

        private void ShowPersonaSelection()
        {
            _personaGrid.gameObject.SetActive(true);
            _chatPanel.gameObject.SetActive(false);
            _inSession = false;
        }

        private void SelectPersona(string persona)
        {
            _currentPersona = persona;
            _inSession = true;
            _sessionStart = Time.realtimeSinceStartup;
            _isRecording = false;

            foreach (var b in _chatBubbles) Destroy(b);
            _chatBubbles.Clear();

            _headerTitle.text = persona;
            _captionText.text = "";
            _personaGrid.gameObject.SetActive(false);
            _chatPanel.gameObject.SetActive(true);

            if (IVXAISessionManager.Instance != null && IVXAISessionManager.Instance.IsInitialized)
            {
                IVXAISessionManager.Instance.OnCaptionReceived += OnCaption;
                IVXAISessionManager.Instance.OnCaptionComplete += OnCaptionDone;
                IVXAISessionManager.Instance.StartVoiceSessionDirect(persona, onSuccess: resp =>
                {
                    _sessionDuration = resp.DurationSeconds;
                    AddBubble($"Hello! I'm your {persona}. How can I help you today?", false);
                }, onError: err => AddBubble($"[Connection error: {err}]", false));
            }
            else
            {
                AddBubble($"Hello! I'm your {persona}. How can I help you today?", false);
                AddBubble("(Demo mode \u2014 no backend connected)", false);
            }
        }

        private void EndSession()
        {
            if (IVXAISessionManager.Instance != null)
            {
                IVXAISessionManager.Instance.OnCaptionReceived -= OnCaption;
                IVXAISessionManager.Instance.OnCaptionComplete -= OnCaptionDone;
                IVXAISessionManager.Instance.EndVoiceSession();
            }
            ShowPersonaSelection();
        }

        private void OnSend()
        {
            string text = _inputField.text?.Trim();
            if (string.IsNullOrEmpty(text)) return;
            _inputField.text = "";

            AddBubble(text, true);

            if (IVXAISessionManager.Instance != null && IVXAISessionManager.Instance.IsVoiceSessionActive)
            {
                IVXAISessionManager.Instance.SendText(text);
            }
            else
            {
                AddBubble("Thanks for your message! (demo mode)", false);
            }
        }

        private void ToggleMic()
        {
            _isRecording = !_isRecording;
            _micBtnImg.color = _isRecording ? COL_RECORD : COL_INPUT;

            if (IVXAISessionManager.Instance != null)
            {
                if (_isRecording) IVXAISessionManager.Instance.StartRecording();
                else
                {
                    IVXAISessionManager.Instance.StopRecording();
                    IVXAISessionManager.Instance.CommitAudio();
                }
            }
        }

        private void OnCaption(string text) => _captionText.text = text;
        private void OnCaptionDone(string text)
        {
            _captionText.text = "";
            AddBubble(text, false);
        }

        #endregion

        #region Chat Bubbles

        private void AddBubble(string text, bool isUser)
        {
            var bubble = CreateRect(_chatContent, "Msg");
            bubble.gameObject.AddComponent<LayoutElement>().minHeight = 36f;

            var hlg = bubble.gameObject.AddComponent<HorizontalLayoutGroup>();
            hlg.spacing = 0; hlg.childControlWidth = false; hlg.childControlHeight = true;
            hlg.childForceExpandWidth = false;
            hlg.childAlignment = isUser ? TextAnchor.MiddleRight : TextAnchor.MiddleLeft;
            hlg.padding = isUser ? new RectOffset(60, 0, 0, 0) : new RectOffset(0, 60, 0, 0);

            var msgBg = CreateRect(bubble, "Bg");
            var img = msgBg.gameObject.AddComponent<Image>();
            img.color = isUser ? COL_USER_BUBBLE : COL_AI_BUBBLE;
            var vlg = AddVLayout(msgBg.gameObject, 2f, new RectOffset(14, 14, 8, 8));
            var bubbleFitter = msgBg.gameObject.AddComponent<ContentSizeFitter>();
            bubbleFitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
            bubbleFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            var tmp = CreateTMP(msgBg, "Text", text, 15f, FontStyles.Normal, COL_TEXT);
            tmp.textWrappingMode = TextWrappingModes.Normal;
            var le = tmp.gameObject.AddComponent<LayoutElement>();
            le.preferredWidth = 400f;

            _chatBubbles.Add(bubble.gameObject);
            Canvas.ForceUpdateCanvases();
            _chatScroll.normalizedPosition = Vector2.zero;
        }

        #endregion

        #region UI Primitives

        private static RectTransform CreateRect(Transform parent, string name)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            return go.GetComponent<RectTransform>();
        }

        private static RectTransform CreatePanel(Transform parent, string name, Color c)
        {
            var r = CreateRect(parent, name);
            r.gameObject.AddComponent<Image>().color = c;
            return r;
        }

        private static TextMeshProUGUI CreateTMP(RectTransform parent, string name, string text, float size, FontStyles style, Color col, TextAlignmentOptions align = TextAlignmentOptions.MidlineLeft)
        {
            var r = CreateRect(parent, name);
            var t = r.gameObject.AddComponent<TextMeshProUGUI>();
            t.text = text; t.fontSize = size; t.fontStyle = style; t.color = col;
            t.alignment = align; t.raycastTarget = false;
            return t;
        }

        private static VerticalLayoutGroup AddVLayout(GameObject go, float sp, RectOffset pad)
        {
            var v = go.AddComponent<VerticalLayoutGroup>();
            v.spacing = sp; v.padding = pad;
            v.childAlignment = TextAnchor.UpperLeft;
            v.childControlWidth = true; v.childControlHeight = false;
            v.childForceExpandWidth = true; v.childForceExpandHeight = false;
            return v;
        }

        private static void Stretch(RectTransform r, float pad = 0f)
        {
            r.anchorMin = Vector2.zero; r.anchorMax = Vector2.one;
            r.offsetMin = new Vector2(pad, pad); r.offsetMax = new Vector2(-pad, -pad);
        }

        #endregion
    }
}
