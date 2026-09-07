using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using IntelliVerseX.AI;

namespace IntelliVerseX.Demos
{
    /// <summary>
    /// Demo UI for the AI Host commentary system.
    /// Shows an in-game overlay with host messages, player context, and controls.
    /// Attach to a Canvas.
    /// </summary>
    public class IVXAIHostDemo : MonoBehaviour
    {
        #region Constants

        private static readonly Color COL_BG = new Color(0.05f, 0.06f, 0.09f, 0.92f);
        private static readonly Color COL_PANEL = new Color(0.10f, 0.12f, 0.17f);
        private static readonly Color COL_HOST = new Color(0.95f, 0.75f, 0.20f);
        private static readonly Color COL_PRIMARY = new Color(0.25f, 0.50f, 0.90f);
        private static readonly Color COL_SUCCESS = new Color(0.20f, 0.75f, 0.45f);
        private static readonly Color COL_TEXT = new Color(0.92f, 0.93f, 0.95f);
        private static readonly Color COL_DIM = new Color(0.55f, 0.58f, 0.64f);

        private static readonly string[] MOCK_HOST_LINES =
        {
            "Welcome everyone to today's trivia showdown!",
            "Ooh, that's a tough one! Let's see who gets it first...",
            "Player 1 is on fire with a 5-question streak!",
            "It's neck and neck! Only 2 points separate our top players!",
            "Time's running out \u2014 last 3 questions!",
            "And the final scores are in! What an incredible match!"
        };

        #endregion

        #region Private Fields

        private RectTransform _messageArea;
        private ScrollRect _scroll;
        private RectTransform _content;
        private TextMeshProUGUI _statusText;
        private RectTransform _playerPanel;
        private readonly List<GameObject> _msgs = new List<GameObject>();
        private int _mockIdx;
        private Coroutine _mockRoutine;
        private System.Action<IVXAIMessage> _hostMsgHandler;

        #endregion

        #region Unity Lifecycle

        private void Start()
        {
            BuildUI();
            StartMockCommentary();
        }

        private void OnDestroy()
        {
            if (_mockRoutine != null) StopCoroutine(_mockRoutine);
            if (_hostMsgHandler != null && IVXAISessionManager.Instance != null)
                IVXAISessionManager.Instance.OnHostMessageReceived -= _hostMsgHandler;
        }

        #endregion

        #region UI Construction

        private void BuildUI()
        {
            var bg = MakePanel(transform, "BG", COL_BG);
            Stretch(bg);

            var root = MakeRect(transform, "Root");
            Stretch(root);
            var vlg = AddVLayout(root.gameObject, 10f, new RectOffset(16, 16, 16, 16));
            vlg.childForceExpandHeight = false;

            // Header
            var header = MakeRect(root, "Header");
            header.gameObject.AddComponent<LayoutElement>().minHeight = 50f;
            var hhlg = header.gameObject.AddComponent<HorizontalLayoutGroup>();
            hhlg.spacing = 12f; hhlg.childAlignment = TextAnchor.MiddleLeft;
            hhlg.childControlWidth = false; hhlg.childControlHeight = true;
            hhlg.padding = new RectOffset(4, 4, 0, 0);

            var hostIcon = MakeTMP(header, "Icon", "\ud83c\udfa4", 30f, FontStyles.Normal, COL_HOST);
            hostIcon.gameObject.AddComponent<LayoutElement>().minWidth = 40f;
            var title = MakeTMP(header, "Title", "AI Host Commentary", 24f, FontStyles.Bold, COL_TEXT);
            title.gameObject.AddComponent<LayoutElement>().flexibleWidth = 1f;
            _statusText = MakeTMP(header, "Status", "\u25CF LIVE", 16f, FontStyles.Bold, COL_SUCCESS);
            _statusText.gameObject.AddComponent<LayoutElement>().minWidth = 70f;

            // Player context cards
            _playerPanel = MakePanel(root, "Players", COL_PANEL);
            _playerPanel.gameObject.AddComponent<LayoutElement>().minHeight = 80f;
            var phlg = _playerPanel.gameObject.AddComponent<HorizontalLayoutGroup>();
            phlg.spacing = 10f; phlg.padding = new RectOffset(12, 12, 10, 10);
            phlg.childAlignment = TextAnchor.MiddleCenter;
            phlg.childControlWidth = true; phlg.childControlHeight = true;
            phlg.childForceExpandWidth = true;

            CreatePlayerCard(_playerPanel, "Player 1", "veteran, competitive", "89%", "#1");
            CreatePlayerCard(_playerPanel, "Player 2", "newcomer, casual", "62%", "#2");
            CreatePlayerCard(_playerPanel, "Player 3", "speed-demon", "78%", "#3");

            // Host messages scroll
            _messageArea = MakeRect(root, "MsgArea");
            _messageArea.gameObject.AddComponent<LayoutElement>().flexibleHeight = 1f;
            _messageArea.gameObject.AddComponent<Image>().color = new Color(0.07f, 0.08f, 0.11f);
            _messageArea.gameObject.AddComponent<RectMask2D>();
            _scroll = _messageArea.gameObject.AddComponent<ScrollRect>();
            _scroll.horizontal = false; _scroll.vertical = true;

            var vp = MakeRect(_messageArea, "VP"); Stretch(vp);
            _content = MakeRect(vp, "Content");
            _content.anchorMin = new Vector2(0, 1); _content.anchorMax = new Vector2(1, 1);
            _content.pivot = new Vector2(0.5f, 1f);
            var cvlg = AddVLayout(_content.gameObject, 8f, new RectOffset(12, 12, 12, 12));
            cvlg.childForceExpandHeight = false;
            _content.gameObject.AddComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            _scroll.viewport = vp; _scroll.content = _content;

            // Controls
            var controls = MakeRect(root, "Controls");
            controls.gameObject.AddComponent<LayoutElement>().minHeight = 52f;
            var chlg = controls.gameObject.AddComponent<HorizontalLayoutGroup>();
            chlg.spacing = 12f; chlg.childAlignment = TextAnchor.MiddleCenter;
            chlg.childControlWidth = true; chlg.childControlHeight = true;
            chlg.childForceExpandWidth = true;
            chlg.padding = new RectOffset(4, 4, 4, 4);

            MakeButton(controls, "Trigger", "Trigger Speech", COL_PRIMARY, () =>
            {
                AddHostMessage("Let me give you all a fun fact about this topic!");
            });
            MakeButton(controls, "Event", "Send Event", COL_PANEL, () =>
            {
                AddHostMessage("A game event has been received \u2014 updating commentary...");
            });
        }

        private void CreatePlayerCard(RectTransform parent, string name, string personality, string accuracy, string rank)
        {
            var card = MakePanel(parent, name, new Color(0.14f, 0.16f, 0.22f));
            var vlg = AddVLayout(card.gameObject, 2f, new RectOffset(10, 10, 6, 6));
            vlg.childAlignment = TextAnchor.MiddleCenter;

            MakeTMP(card, "Name", name, 15f, FontStyles.Bold, COL_TEXT, TextAlignmentOptions.Center);
            MakeTMP(card, "Pers", personality, 11f, FontStyles.Italic, COL_DIM, TextAlignmentOptions.Center);
            MakeTMP(card, "Stats", $"Acc: {accuracy}  Rank: {rank}", 12f, FontStyles.Normal, COL_HOST, TextAlignmentOptions.Center);
        }

        #endregion

        #region Mock Commentary

        private void StartMockCommentary()
        {
            if (IVXAISessionManager.Instance != null && IVXAISessionManager.Instance.IsInitialized)
            {
                _hostMsgHandler = msg =>
                {
                    if (!string.IsNullOrEmpty(msg?.Text))
                        AddHostMessage(msg.Text);
                };
                IVXAISessionManager.Instance.OnHostMessageReceived += _hostMsgHandler;
            }

            _mockRoutine = StartCoroutine(MockLoop());
        }

        private IEnumerator MockLoop()
        {
            yield return new WaitForSeconds(1f);
            while (true)
            {
                if (_mockIdx < MOCK_HOST_LINES.Length)
                {
                    AddHostMessage(MOCK_HOST_LINES[_mockIdx]);
                    _mockIdx++;
                }
                yield return new WaitForSeconds(Random.Range(3f, 6f));
            }
        }

        private void AddHostMessage(string text)
        {
            var msg = MakePanel(_content, "Msg", new Color(0.12f, 0.14f, 0.20f));
            var hlg = msg.gameObject.AddComponent<HorizontalLayoutGroup>();
            hlg.spacing = 10f; hlg.padding = new RectOffset(12, 12, 8, 8);
            hlg.childAlignment = TextAnchor.UpperLeft;
            hlg.childControlWidth = false; hlg.childControlHeight = true;
            hlg.childForceExpandWidth = false;
            msg.gameObject.AddComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            var icon = MakeTMP(msg, "Icon", "\ud83c\udfa4", 20f, FontStyles.Normal, COL_HOST);
            icon.gameObject.AddComponent<LayoutElement>().minWidth = 28f;
            var txt = MakeTMP(msg, "Text", text, 15f, FontStyles.Normal, COL_TEXT);
            txt.textWrappingMode = TextWrappingModes.Normal;
            txt.gameObject.AddComponent<LayoutElement>().flexibleWidth = 1f;

            _msgs.Add(msg.gameObject);
            if (_msgs.Count > 20) { Destroy(_msgs[0]); _msgs.RemoveAt(0); }

            Canvas.ForceUpdateCanvases();
            _scroll.normalizedPosition = Vector2.zero;
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

        private static void MakeButton(RectTransform parent, string name, string label, Color bg, UnityEngine.Events.UnityAction onClick)
        {
            var r = MakeRect(parent, name);
            var img = r.gameObject.AddComponent<Image>(); img.color = bg;
            var btn = r.gameObject.AddComponent<Button>(); btn.targetGraphic = img;
            btn.onClick.AddListener(onClick);
            r.gameObject.AddComponent<LayoutElement>().minHeight = 44f;
            var lbl = MakeTMP(r, "Lbl", label, 16f, FontStyles.Bold, new Color(0.92f, 0.93f, 0.95f), TextAlignmentOptions.Center);
            Stretch(lbl.rectTransform);
        }

        #endregion
    }
}
