using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using IntelliVerseX.AI;

namespace IntelliVerseX.Demos
{
    /// <summary>
    /// Self-contained NPC dialog demo: register sample NPCs, chat UI, and IVXAINPCDialogManager events.
    /// Drop on any GameObject; creates a 1920×1080 canvas at runtime.
    /// </summary>
    public sealed class IVXAINPCDemo : MonoBehaviour
    {
        #region Constants — theme

        private static readonly Color32 ColBg = new Color32(0x1A, 0x1A, 0x2E, 0xFF);
        private static readonly Color32 ColPanel = new Color32(0x16, 0x21, 0x3E, 0xFF);
        private static readonly Color32 ColAccent = new Color32(0x0F, 0x34, 0x60, 0xFF);
        private static readonly Color32 ColHighlight = new Color32(0xE9, 0x45, 0x60, 0xFF);
        private static readonly Color ColText = new Color(0.93f, 0.94f, 0.96f, 1f);
        private static readonly Color ColDim = new Color(0.65f, 0.68f, 0.74f, 1f);

        #endregion

        #region Serialized

        [Header("AI")]
        [SerializeField] private IVXAIConfig _aiConfig;

        [Header("Player")]
        [SerializeField] private string _demoPlayerId = "demo_player_1";

        #endregion

        #region Private

        private IVXAINPCDialogManager _npc;
        private string _selectedNpcId = "npc_merchant";
        private string _sessionId;
        private readonly List<GameObject> _bubbleObjects = new List<GameObject>();
        private TextMeshProUGUI _actionLogText;
        private ScrollRect _actionLogScroll;

        private RectTransform _chatContent;
        private ScrollRect _chatScroll;
        private TMP_InputField _input;
        private RectTransform _npcButtonMerchant;
        private RectTransform _npcButtonGuard;
        private RectTransform _npcButtonSage;
        private TextMeshProUGUI _statusLine;

        #endregion

        #region Unity

        private void Start()
        {
            EnsureEventSystem();
            BuildCanvasAndUi();
            WireManagers();
            RegisterSampleNpcs();
            SubscribeEvents();
            SetNpcSelectionUi();
            SetStatus("Select an NPC, then Start Dialog.");
        }

        private void OnDestroy()
        {
            if (_npc == null)
                return;
            _npc.OnNPCResponse -= HandleNpcResponse;
            _npc.OnNPCAction -= HandleNpcAction;
            _npc.OnDialogStarted -= HandleDialogStarted;
            _npc.OnDialogEnded -= HandleDialogEnded;
            _npc.OnError -= HandleError;
        }

        #endregion

        #region Setup

        private static void EnsureEventSystem()
        {
            if (FindFirstObjectByType<EventSystem>() != null)
                return;
            var es = new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
            DontDestroyOnLoad(es);
        }

        private void WireManagers()
        {
            _npc = IVXAINPCDialogManager.Instance;
            if (_npc == null)
            {
                var go = new GameObject("IVXAINPCDialogManager");
                _npc = go.AddComponent<IVXAINPCDialogManager>();
            }

            if (_aiConfig != null)
                _npc.Initialize(_aiConfig);
        }

        private void RegisterSampleNpcs()
        {
            _npc.RegisterNPC(new IVXAINPCProfile
            {
                NpcId = "npc_merchant",
                DisplayName = "Merchant",
                PersonaPrompt = "You are a friendly shopkeeper who loves haggling and helping travelers.",
                Backstory = "Runs a bustling market stall by the city gates.",
                MaxTurns = 0,
                AvailableActions = new[] { "give_item", "open_shop", "start_quest" }
            });

            _npc.RegisterNPC(new IVXAINPCProfile
            {
                NpcId = "npc_guard",
                DisplayName = "Guard",
                PersonaPrompt = "You are a stern gate guard. You demand proper papers and speak in short commands.",
                Backstory = "Veteran of the city watch.",
                MaxTurns = 0,
                AvailableActions = new[] { "deny_entry", "alert_captain", "confiscate_item" }
            });

            _npc.RegisterNPC(new IVXAINPCProfile
            {
                NpcId = "npc_sage",
                DisplayName = "Sage",
                PersonaPrompt = "You are a wise old mystic who speaks in riddles and metaphors.",
                Backstory = "Lives in a tower overlooking the valley.",
                MaxTurns = 0,
                AvailableActions = new[] { "reveal_lore", "grant_blessing", "start_quest" }
            });
        }

        private void SubscribeEvents()
        {
            _npc.OnNPCResponse += HandleNpcResponse;
            _npc.OnNPCAction += HandleNpcAction;
            _npc.OnDialogStarted += HandleDialogStarted;
            _npc.OnDialogEnded += HandleDialogEnded;
            _npc.OnError += HandleError;
        }

        #endregion

        #region Event handlers

        private void HandleDialogStarted(string sessionId)
        {
            _sessionId = sessionId;
            AppendActionLog($"[dialog] started session {sessionId}");
            SetStatus("Dialog active — type and Send, or End Dialog.");
        }

        private void HandleDialogEnded(string sessionId)
        {
            AppendActionLog($"[dialog] ended session {sessionId}");
            if (_sessionId == sessionId)
                _sessionId = null;
            SetStatus("Dialog ended.");
        }

        private void HandleNpcResponse(string sessionId, string text)
        {
            if (sessionId != _sessionId)
                return;
            AddBubble("NPC", text, ColAccent);
        }

        private void HandleNpcAction(string sessionId, IVXAINPCAction action)
        {
            if (action == null)
                return;
            string payload = string.IsNullOrEmpty(action.ActionPayload) ? "{}" : action.ActionPayload;
            AppendActionLog($"[tool] {action.ActionName}  payload={payload}");
        }

        private void HandleError(string sessionId, string message)
        {
            AppendActionLog($"[error] {(sessionId ?? "?")} — {message}");
            SetStatus("Error — see action log.");
        }

        #endregion

        #region UI actions

        private void OnSelectNpc(string npcId)
        {
            _selectedNpcId = npcId;
            SetNpcSelectionUi();
        }

        private void OnStartDialog()
        {
            if (_npc == null || !_npc.IsInitialized)
            {
                AppendActionLog("[error] IVXAINPCDialogManager not initialized — assign IVXAIConfig.");
                return;
            }

            _npc.StartDialog(_selectedNpcId, _demoPlayerId, "Demo scene: town square.", session =>
            {
                if (session != null && !string.IsNullOrEmpty(session.SessionId))
                    _sessionId = session.SessionId;
            });
        }

        private void OnSend()
        {
            if (string.IsNullOrEmpty(_sessionId) || _input == null)
                return;
            string msg = _input.text?.Trim();
            if (string.IsNullOrEmpty(msg))
                return;

            AddBubble("You", msg, ColHighlight);
            _npc.SendMessage(_sessionId, msg);
            _input.text = string.Empty;
        }

        private void OnEndDialog()
        {
            if (string.IsNullOrEmpty(_sessionId))
                return;
            string sid = _sessionId;
            _npc.EndDialog(sid);
        }

        #endregion

        #region UI construction

        private void BuildCanvasAndUi()
        {
            var canvasGo = new GameObject("IVXAINPCDemoCanvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            canvasGo.transform.SetParent(transform, false);
            var canvas = canvasGo.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = canvasGo.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;

            var bg = Panel(canvasGo.transform, "BG", ColBg);
            Stretch(bg);

            var root = Rect(canvasGo.transform, "Root");
            Stretch(root);
            VLayout(root.gameObject, 12f, new RectOffset(24, 24, 24, 24));

            var title = Tmp(root, "Title", "NPC Dialog System", 26f, FontStyles.Bold, ColText);
            title.gameObject.AddComponent<LayoutElement>().minHeight = 36f;

            var sub = Tmp(root, "Sub", "IVXAINPCDialogManager — sample personas & tool calls", 15f, FontStyles.Normal, ColDim);
            sub.gameObject.AddComponent<LayoutElement>().minHeight = 22f;

            var npcRow = Rect(root, "NpcRow");
            npcRow.gameObject.AddComponent<LayoutElement>().minHeight = 52f;
            var npcH = npcRow.gameObject.AddComponent<HorizontalLayoutGroup>();
            npcH.spacing = 10f;
            npcH.childAlignment = TextAnchor.MiddleLeft;
            npcH.childControlWidth = false;
            npcH.childControlHeight = true;

            Tmp(npcRow, "Lbl", "NPC:", 16f, FontStyles.Bold, ColText).gameObject.AddComponent<LayoutElement>().minWidth = 48f;
            _npcButtonMerchant = MakeNpcChip(npcRow, "Merchant", "npc_merchant");
            _npcButtonGuard = MakeNpcChip(npcRow, "Guard", "npc_guard");
            _npcButtonSage = MakeNpcChip(npcRow, "Sage", "npc_sage");

            _statusLine = Tmp(root, "Status", "", 14f, FontStyles.Italic, ColDim);
            _statusLine.gameObject.AddComponent<LayoutElement>().minHeight = 22f;

            var mid = Rect(root, "Mid");
            mid.gameObject.AddComponent<LayoutElement>().flexibleHeight = 1f;
            var midH = mid.gameObject.AddComponent<HorizontalLayoutGroup>();
            midH.spacing = 16f;
            midH.childForceExpandWidth = true;
            midH.childForceExpandHeight = true;

            BuildChatColumn(mid);
            BuildActionLogColumn(mid);

            var inputRow = Rect(root, "InputRow");
            inputRow.gameObject.AddComponent<LayoutElement>().minHeight = 48f;
            var inH = inputRow.gameObject.AddComponent<HorizontalLayoutGroup>();
            inH.spacing = 10f;
            inH.childAlignment = TextAnchor.MiddleCenter;
            inH.childForceExpandWidth = true;
            inH.childControlHeight = true;

            _input = MakeTmpInput(inputRow, "Message");
            _input.gameObject.AddComponent<LayoutElement>().flexibleWidth = 1f;
            MakeBtn(inputRow, "Send", ColHighlight, OnSend).gameObject.AddComponent<LayoutElement>().minWidth = 120f;

            var ctrlRow = Rect(root, "CtrlRow");
            ctrlRow.gameObject.AddComponent<LayoutElement>().minHeight = 48f;
            var cH = ctrlRow.gameObject.AddComponent<HorizontalLayoutGroup>();
            cH.spacing = 12f;
            cH.childForceExpandWidth = true;

            MakeBtn(ctrlRow, "Start Dialog", ColAccent, OnStartDialog);
            MakeBtn(ctrlRow, "End Dialog", ColPanel, OnEndDialog);
        }

        private RectTransform MakeNpcChip(RectTransform parent, string label, string npcId)
        {
            var r = Rect(parent, "Npc_" + npcId);
            var img = r.gameObject.AddComponent<Image>();
            img.color = ColPanel;
            var le = r.gameObject.AddComponent<LayoutElement>();
            le.minWidth = 140f;
            le.minHeight = 44f;
            var btn = r.gameObject.AddComponent<Button>();
            btn.targetGraphic = img;
            btn.onClick.AddListener(() => OnSelectNpc(npcId));
            var t = Tmp(r, "T", label, 16f, FontStyles.Bold, ColText, TextAlignmentOptions.Center);
            Stretch(t.rectTransform);
            return r;
        }

        private void BuildChatColumn(RectTransform mid)
        {
            var col = Panel(mid, "ChatCol", ColPanel);
            col.gameObject.AddComponent<LayoutElement>().flexibleWidth = 1.2f;
            VLayout(col.gameObject, 8f, new RectOffset(12, 12, 12, 12));

            Tmp(col, "ChatTitle", "Conversation", 18f, FontStyles.Bold, ColText).gameObject.AddComponent<LayoutElement>().minHeight = 28f;

            var scrollRt = Rect(col, "Scroll");
            scrollRt.gameObject.AddComponent<LayoutElement>().flexibleHeight = 1f;
            scrollRt.gameObject.AddComponent<Image>().color = new Color(0.08f, 0.09f, 0.14f, 1f);
            scrollRt.gameObject.AddComponent<RectMask2D>();
            var scroll = scrollRt.gameObject.AddComponent<ScrollRect>();
            scroll.horizontal = false;
            scroll.vertical = true;

            var vp = Rect(scrollRt, "Viewport");
            Stretch(vp);
            vp.gameObject.AddComponent<RectMask2D>();

            _chatContent = Rect(vp, "Content");
            _chatContent.anchorMin = new Vector2(0f, 1f);
            _chatContent.anchorMax = new Vector2(1f, 1f);
            _chatContent.pivot = new Vector2(0.5f, 1f);
            var cv = VLayout(_chatContent.gameObject, 8f, new RectOffset(10, 10, 10, 10));
            cv.childForceExpandHeight = false;
            _chatContent.gameObject.AddComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            scroll.viewport = vp;
            scroll.content = _chatContent;
            _chatScroll = scroll;
        }

        private void BuildActionLogColumn(RectTransform mid)
        {
            var col = Panel(mid, "LogCol", ColPanel);
            col.gameObject.AddComponent<LayoutElement>().flexibleWidth = 0.85f;
            VLayout(col.gameObject, 6f, new RectOffset(12, 12, 12, 12));

            Tmp(col, "LogTitle", "Action log (tools)", 18f, FontStyles.Bold, ColText).gameObject.AddComponent<LayoutElement>().minHeight = 28f;

            var scrollRt = Rect(col, "LogScroll");
            scrollRt.gameObject.AddComponent<LayoutElement>().flexibleHeight = 1f;
            scrollRt.gameObject.AddComponent<Image>().color = new Color(0.06f, 0.07f, 0.11f, 1f);
            scrollRt.gameObject.AddComponent<RectMask2D>();
            var scroll = scrollRt.gameObject.AddComponent<ScrollRect>();
            scroll.horizontal = false;
            scroll.vertical = true;

            var vp = Rect(scrollRt, "Viewport");
            Stretch(vp);

            var content = Rect(vp, "LogContent");
            content.anchorMin = new Vector2(0f, 1f);
            content.anchorMax = new Vector2(1f, 1f);
            content.pivot = new Vector2(0.5f, 1f);
            var lv = VLayout(content.gameObject, 4f, new RectOffset(8, 8, 8, 8));
            lv.childForceExpandHeight = false;
            content.gameObject.AddComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            scroll.viewport = vp;
            scroll.content = content;
            _actionLogScroll = scroll;

            _actionLogText = Tmp(content, "LogBody", string.Empty, 12f, FontStyles.Normal, ColDim, TextAlignmentOptions.TopLeft);
            _actionLogText.textWrappingMode = TextWrappingModes.Normal;
            var le = _actionLogText.gameObject.AddComponent<LayoutElement>();
            le.minWidth = 300f;
        }

        private void SetNpcSelectionUi()
        {
            SetChip(_npcButtonMerchant, _selectedNpcId == "npc_merchant");
            SetChip(_npcButtonGuard, _selectedNpcId == "npc_guard");
            SetChip(_npcButtonSage, _selectedNpcId == "npc_sage");
        }

        private static void SetChip(RectTransform chip, bool on)
        {
            if (chip == null)
                return;
            var img = chip.GetComponent<Image>();
            if (img != null)
                img.color = on ? ColAccent : ColPanel;
        }

        private void SetStatus(string s)
        {
            if (_statusLine != null)
                _statusLine.text = s;
        }

        private void AddBubble(string who, string text, Color32 bubbleColor)
        {
            var row = Rect(_chatContent, "Bubble", 0f);
            var h = row.gameObject.AddComponent<HorizontalLayoutGroup>();
            h.spacing = 8f;
            h.childAlignment = TextAnchor.UpperLeft;
            h.childControlWidth = false;
            h.childForceExpandWidth = false;
            h.padding = new RectOffset(4, 4, 4, 4);
            row.gameObject.AddComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            if (who == "You")
            {
                var spacer = Rect(row, "Sp");
                spacer.gameObject.AddComponent<LayoutElement>().flexibleWidth = 1f;
            }

            var bubble = Panel(row, "Inner", bubbleColor);
            var le = bubble.gameObject.AddComponent<LayoutElement>();
            le.flexibleWidth = 0f;
            le.preferredWidth = 520f;
            var innerV = VLayout(bubble.gameObject, 4f, new RectOffset(12, 12, 10, 10));
            Tmp(bubble, "Who", who, 13f, FontStyles.Bold, ColText);
            var body = Tmp(bubble, "Body", text, 15f, FontStyles.Normal, ColText, TextAlignmentOptions.TopLeft);
            body.textWrappingMode = TextWrappingModes.Normal;

            if (who != "You")
            {
                var spacer = Rect(row, "Sp2");
                spacer.gameObject.AddComponent<LayoutElement>().flexibleWidth = 1f;
            }

            _bubbleObjects.Add(row.gameObject);
            Canvas.ForceUpdateCanvases();
            if (_chatScroll != null)
                _chatScroll.verticalNormalizedPosition = 0f;
        }

        private void AppendActionLog(string line)
        {
            if (_actionLogText == null)
                return;
            if (!string.IsNullOrEmpty(_actionLogText.text))
                _actionLogText.text += "\n";
            _actionLogText.text += line;
            Canvas.ForceUpdateCanvases();
            if (_actionLogScroll != null)
                _actionLogScroll.verticalNormalizedPosition = 0f;
        }

        private TMP_InputField MakeTmpInput(RectTransform parent, string placeholder)
        {
            var rt = Rect(parent, "Input");
            var img = rt.gameObject.AddComponent<Image>();
            img.color = new Color(0.1f, 0.12f, 0.18f, 1f);
            var field = rt.gameObject.AddComponent<TMP_InputField>();
            var textRt = Rect(rt, "Text");
            textRt.anchorMin = Vector2.zero;
            textRt.anchorMax = Vector2.one;
            textRt.offsetMin = new Vector2(12f, 8f);
            textRt.offsetMax = new Vector2(-12f, -8f);
            var tmp = textRt.gameObject.AddComponent<TextMeshProUGUI>();
            tmp.text = string.Empty;
            tmp.fontSize = 16f;
            tmp.color = ColText;
            tmp.textWrappingMode = TextWrappingModes.NoWrap;

            var phRt = Rect(rt, "Placeholder");
            phRt.anchorMin = Vector2.zero;
            phRt.anchorMax = Vector2.one;
            phRt.offsetMin = new Vector2(12f, 8f);
            phRt.offsetMax = new Vector2(-12f, -8f);
            var ph = phRt.gameObject.AddComponent<TextMeshProUGUI>();
            ph.text = placeholder;
            ph.fontSize = 16f;
            ph.fontStyle = FontStyles.Italic;
            ph.color = new Color(1f, 1f, 1f, 0.35f);

            field.textComponent = tmp;
            field.placeholder = ph;
            field.textViewport = rt;
            return field;
        }

        private static RectTransform MakeBtn(RectTransform parent, string name, Color32 bg, Action onClick)
        {
            var r = Rect(parent, name);
            var img = r.gameObject.AddComponent<Image>();
            img.color = bg;
            var btn = r.gameObject.AddComponent<Button>();
            btn.targetGraphic = img;
            btn.onClick.AddListener(() => onClick?.Invoke());
            r.gameObject.AddComponent<LayoutElement>().minHeight = 44f;
            var lbl = Tmp(r, "Lbl", name, 16f, FontStyles.Bold, ColText, TextAlignmentOptions.Center);
            Stretch(lbl.rectTransform);
            return r;
        }

        private static RectTransform Rect(Transform p, string n, float _ = 0f)
        {
            var go = new GameObject(n, typeof(RectTransform));
            go.transform.SetParent(p, false);
            return go.GetComponent<RectTransform>();
        }

        private static RectTransform Panel(Transform p, string n, Color32 c)
        {
            var r = Rect(p, n);
            r.gameObject.AddComponent<Image>().color = c;
            return r;
        }

        private static void Stretch(RectTransform r, float pad = 0f)
        {
            r.anchorMin = Vector2.zero;
            r.anchorMax = Vector2.one;
            r.offsetMin = new Vector2(pad, pad);
            r.offsetMax = new Vector2(-pad, -pad);
        }

        private static TextMeshProUGUI Tmp(RectTransform p, string n, string t, float size, FontStyles st, Color c,
            TextAlignmentOptions align = TextAlignmentOptions.MidlineLeft)
        {
            var r = Rect(p, n);
            var tmp = r.gameObject.AddComponent<TextMeshProUGUI>();
            tmp.text = t;
            tmp.fontSize = size;
            tmp.fontStyle = st;
            tmp.color = c;
            tmp.alignment = align;
            tmp.raycastTarget = false;
            return tmp;
        }

        private static VerticalLayoutGroup VLayout(GameObject go, float sp, RectOffset pad)
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

        #endregion
    }
}
