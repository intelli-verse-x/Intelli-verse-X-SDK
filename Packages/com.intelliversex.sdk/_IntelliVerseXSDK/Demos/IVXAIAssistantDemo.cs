using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using IntelliVerseX.AI;

namespace IntelliVerseX.Demos
{
    /// <summary>
    /// Self-contained AI Assistant demo: Ask, hints, tutorials, and knowledge search.
    /// Drop on any GameObject; builds a 1920×1080 scaled canvas at runtime.
    /// </summary>
    public sealed class IVXAIAssistantDemo : MonoBehaviour
    {
        #region Theme

        private static readonly Color32 ColBg = new Color32(0x1A, 0x1A, 0x2E, 0xFF);
        private static readonly Color32 ColPanel = new Color32(0x16, 0x21, 0x3E, 0xFF);
        private static readonly Color32 ColAccent = new Color32(0x0F, 0x34, 0x60, 0xFF);
        private static readonly Color32 ColHighlight = new Color32(0xE9, 0x45, 0x60, 0xFF);
        private static readonly Color ColText = new Color(0.93f, 0.94f, 0.96f, 1f);
        private static readonly Color ColDim = new Color(0.62f, 0.65f, 0.72f, 1f);

        #endregion

        [SerializeField] private IVXAIConfig _aiConfig;

        private IVXAIAssistant _assistant;
        private RectTransform _responseContent;
        private ScrollRect _responseScroll;
        private TMP_InputField _askInput;
        private TMP_InputField _hintLevel;
        private TMP_InputField _hintObjective;
        private TMP_InputField _tutorialFeatureId;
        private TMP_InputField _searchQuery;
        private GameObject _loadingBlock;
        private TextMeshProUGUI _loadingLabel;
        private readonly System.Collections.Generic.List<GameObject> _responseChunks = new System.Collections.Generic.List<GameObject>();

        private void Start()
        {
            EnsureEventSystem();
            BuildUi();
            WireAssistant();
        }

        private void Update()
        {
            if (_assistant == null || _loadingBlock == null)
                return;
            bool busy = _assistant.IsProcessing;
            _loadingBlock.SetActive(busy);
            if (_loadingLabel != null)
                _loadingLabel.text = busy ? "Processing\u2026" : string.Empty;
        }

        private void OnDestroy()
        {
            if (_assistant == null)
                return;
            _assistant.OnError -= HandleError;
        }

        private static void EnsureEventSystem()
        {
            if (FindFirstObjectByType<EventSystem>() != null)
                return;
            new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
        }

        private void WireAssistant()
        {
            _assistant = IVXAIAssistant.Instance;
            if (_assistant == null)
                _assistant = new GameObject("IVXAIAssistant").AddComponent<IVXAIAssistant>();
            if (_aiConfig != null)
                _assistant.Initialize(_aiConfig);

            _assistant.OnError += HandleError;
        }

        private void HandleError(string err)
        {
            AppendResponse("Error", err, ColHighlight);
        }

        private void OnAsk()
        {
            if (_assistant == null || !_assistant.IsInitialized || _askInput == null)
            {
                AppendResponse("System", "Assign IVXAIConfig and ensure IVXAIAssistant.Initialize.", ColHighlight);
                return;
            }

            string q = _askInput.text?.Trim();
            if (string.IsNullOrEmpty(q))
                return;

            AppendResponse("You", q, ColHighlight);
            _assistant.Ask(q, null, res =>
            {
                if (res == null || string.IsNullOrEmpty(res.Response))
                    return;
                var sb = new StringBuilder();
                sb.AppendLine(res.Response);
                if (res.Sources != null && res.Sources.Length > 0)
                    sb.AppendLine("Sources: " + string.Join(", ", res.Sources));
                sb.AppendLine($"Confidence: {res.Confidence:0.###}");
                AppendResponse("Assistant", sb.ToString(), ColAccent);
            });
        }

        private void OnGetHint()
        {
            if (_assistant == null || !_assistant.IsInitialized)
                return;
            string level = _hintLevel != null ? _hintLevel.text?.Trim() : "level_1";
            string obj = _hintObjective != null ? _hintObjective.text?.Trim() : "main_objective";
            _assistant.GetHint(level, obj, null, hint =>
            {
                if (hint == null)
                    return;
                var sb = new StringBuilder();
                sb.AppendLine(hint.Hint);
                sb.AppendLine($"Difficulty: {hint.DifficultyLevel}");
                sb.AppendLine($"Next available: {hint.NextHintAvailable}");
                AppendResponse("Hint", sb.ToString(), ColAccent);
            });
        }

        private void OnGetTutorial()
        {
            if (_assistant == null || !_assistant.IsInitialized)
                return;
            string fid = _tutorialFeatureId != null ? _tutorialFeatureId.text?.Trim() : "inventory_ui";
            if (string.IsNullOrEmpty(fid))
                return;

            _assistant.GetTutorial(fid, tut =>
            {
                if (tut == null)
                    return;
                var sb = new StringBuilder();
                sb.AppendLine($"Feature: {tut.FeatureId}  (~{tut.EstimatedTimeSeconds}s)");
                if (tut.Steps != null)
                {
                    foreach (var s in tut.Steps)
                        sb.AppendLine($"  {s.StepNumber}. {s.Title}: {s.Description}");
                }

                AppendResponse("Tutorial", sb.ToString(), ColAccent);
            });
        }

        private void OnSearch()
        {
            if (_assistant == null || !_assistant.IsInitialized || _searchQuery == null)
                return;
            string q = _searchQuery.text?.Trim();
            if (string.IsNullOrEmpty(q))
                return;

            _assistant.SearchKnowledgeBase(q, results =>
            {
                AppendResponse("Search", string.Join("\n---\n", results), ColAccent);
            });
        }

        private void OnClearHistory()
        {
            _assistant?.ClearHistory();
            foreach (var go in _responseChunks)
            {
                if (go != null)
                    Destroy(go);
            }

            _responseChunks.Clear();
        }

        private void BuildUi()
        {
            var canvasGo = new GameObject("IVXAIAssistantDemoCanvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            canvasGo.transform.SetParent(transform, false);
            var canvas = canvasGo.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = canvasGo.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;

            var bg = Panel(canvasGo.transform, "BG", ColBg);
            Stretch(bg);

            var root = Rect(canvasGo.transform, "Root");
            Stretch(root);
            VLayout(root.gameObject, 10f, new RectOffset(20, 20, 20, 20));

            Tmp(root, "Title", "AI Assistant", 28f, FontStyles.Bold, ColText).gameObject.AddComponent<LayoutElement>().minHeight = 36f;
            Tmp(root, "Sub", "IVXAIAssistant — Ask, hints, tutorials, knowledge search", 15f, FontStyles.Normal, ColDim).gameObject.AddComponent<LayoutElement>().minHeight = 22f;

            var loadingRow = Rect(root, "LoadingRow");
            loadingRow.gameObject.AddComponent<LayoutElement>().minHeight = 28f;
            _loadingBlock = new GameObject("Loading");
            _loadingBlock.transform.SetParent(loadingRow, false);
            var lrt = _loadingBlock.AddComponent<RectTransform>();
            Stretch(lrt);
            _loadingBlock.SetActive(false);
            _loadingLabel = Tmp(lrt, "Lbl", string.Empty, 16f, FontStyles.Bold, ColHighlight, TextAlignmentOptions.Center);
            Stretch(_loadingLabel.rectTransform);

            var askRow = Rect(root, "AskRow");
            askRow.gameObject.AddComponent<LayoutElement>().minHeight = 52f;
            var askH = askRow.gameObject.AddComponent<HorizontalLayoutGroup>();
            askH.spacing = 10f;
            askH.childForceExpandWidth = true;
            _askInput = TmpInput(askRow, "AskInput", "Ask the assistant anything\u2026");
            _askInput.gameObject.AddComponent<LayoutElement>().flexibleWidth = 1f;
            Btn(askRow, "Ask", ColHighlight, OnAsk).gameObject.AddComponent<LayoutElement>().minWidth = 120f;

            var hintRow = Rect(root, "HintRow");
            hintRow.gameObject.AddComponent<LayoutElement>().minHeight = 48f;
            var hh = hintRow.gameObject.AddComponent<HorizontalLayoutGroup>();
            hh.spacing = 8f;
            hh.childForceExpandWidth = true;
            _hintLevel = TmpInput(hintRow, "Lvl", "Level id");
            _hintLevel.gameObject.AddComponent<LayoutElement>().flexibleWidth = 1f;
            _hintObjective = TmpInput(hintRow, "Obj", "Objective id");
            _hintObjective.gameObject.AddComponent<LayoutElement>().flexibleWidth = 1f;
            Btn(hintRow, "Get Hint", ColAccent, OnGetHint).gameObject.AddComponent<LayoutElement>().minWidth = 140f;

            var tutRow = Rect(root, "TutRow");
            tutRow.gameObject.AddComponent<LayoutElement>().minHeight = 48f;
            var th = tutRow.gameObject.AddComponent<HorizontalLayoutGroup>();
            th.spacing = 8f;
            th.childForceExpandWidth = true;
            _tutorialFeatureId = TmpInput(tutRow, "Feat", "Feature ID (e.g. shop_ui)");
            _tutorialFeatureId.gameObject.AddComponent<LayoutElement>().flexibleWidth = 1f;
            Btn(tutRow, "Get Tutorial", ColAccent, OnGetTutorial).gameObject.AddComponent<LayoutElement>().minWidth = 160f;

            var searchRow = Rect(root, "SearchRow");
            searchRow.gameObject.AddComponent<LayoutElement>().minHeight = 48f;
            var sh = searchRow.gameObject.AddComponent<HorizontalLayoutGroup>();
            sh.spacing = 8f;
            sh.childForceExpandWidth = true;
            _searchQuery = TmpInput(searchRow, "Q", "Knowledge search query");
            _searchQuery.gameObject.AddComponent<LayoutElement>().flexibleWidth = 1f;
            Btn(searchRow, "Search Knowledge", ColAccent, OnSearch).gameObject.AddComponent<LayoutElement>().minWidth = 180f;

            var respArea = Rect(root, "Responses");
            respArea.gameObject.AddComponent<LayoutElement>().flexibleHeight = 1f;
            respArea.gameObject.AddComponent<Image>().color = new Color(0.07f, 0.08f, 0.12f, 1f);
            respArea.gameObject.AddComponent<RectMask2D>();
            var scroll = respArea.gameObject.AddComponent<ScrollRect>();
            scroll.horizontal = false;
            scroll.vertical = true;
            var vp = Rect(respArea, "VP");
            Stretch(vp);
            _responseContent = Rect(vp, "Content");
            _responseContent.anchorMin = new Vector2(0f, 1f);
            _responseContent.anchorMax = new Vector2(1f, 1f);
            _responseContent.pivot = new Vector2(0.5f, 1f);
            VLayout(_responseContent.gameObject, 10f, new RectOffset(14, 14, 14, 14));
            _responseContent.gameObject.AddComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            scroll.viewport = vp;
            scroll.content = _responseContent;
            _responseScroll = scroll;

            var clearRow = Rect(root, "ClearRow");
            clearRow.gameObject.AddComponent<LayoutElement>().minHeight = 44f;
            Btn(clearRow, "Clear History", ColPanel, OnClearHistory);
        }

        private void AppendResponse(string header, string body, Color32 hdrColor)
        {
            var block = Panel(_responseContent, "Block", ColPanel);
            var v = VLayout(block.gameObject, 6f, new RectOffset(12, 12, 10, 12));
            v.childForceExpandHeight = false;
            block.gameObject.AddComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            Tmp(block, "H", header, 15f, FontStyles.Bold, hdrColor);
            var t = Tmp(block, "B", body, 14f, FontStyles.Normal, ColText, TextAlignmentOptions.TopLeft);
            t.textWrappingMode = TextWrappingModes.Normal;
            var le = t.gameObject.AddComponent<LayoutElement>();
            le.minWidth = 400f;

            _responseChunks.Add(block.gameObject);
            Canvas.ForceUpdateCanvases();
            if (_responseScroll != null)
                _responseScroll.verticalNormalizedPosition = 0f;
        }

        private static RectTransform Btn(RectTransform p, string label, Color32 bg, UnityEngine.Events.UnityAction onClick)
        {
            var r = Rect(p, label);
            var img = r.gameObject.AddComponent<Image>();
            img.color = bg;
            var b = r.gameObject.AddComponent<Button>();
            b.targetGraphic = img;
            b.onClick.AddListener(onClick);
            r.gameObject.AddComponent<LayoutElement>().minHeight = 44f;
            var t = Tmp(r, "T", label, 15f, FontStyles.Bold, ColText, TextAlignmentOptions.Center);
            Stretch(t.rectTransform);
            return r;
        }

        private static TMP_InputField TmpInput(RectTransform parent, string name, string placeholder)
        {
            var rt = Rect(parent, name);
            rt.gameObject.AddComponent<Image>().color = new Color(0.09f, 0.11f, 0.16f, 1f);
            var field = rt.gameObject.AddComponent<TMP_InputField>();
            var tr = Rect(rt, "Text");
            Stretch(tr, 12f, 8f);
            var tmp = tr.gameObject.AddComponent<TextMeshProUGUI>();
            tmp.fontSize = 15f;
            tmp.color = ColText;
            var phr = Rect(rt, "Ph");
            Stretch(phr, 12f, 8f);
            var ph = phr.gameObject.AddComponent<TextMeshProUGUI>();
            ph.text = placeholder;
            ph.fontSize = 15f;
            ph.fontStyle = FontStyles.Italic;
            ph.color = new Color(1f, 1f, 1f, 0.35f);
            field.textComponent = tmp;
            field.placeholder = ph;
            field.textViewport = rt;
            return field;
        }

        private static void Stretch(RectTransform r, float xPad, float yPad)
        {
            r.anchorMin = Vector2.zero;
            r.anchorMax = Vector2.one;
            r.offsetMin = new Vector2(xPad, yPad);
            r.offsetMax = new Vector2(-xPad, -yPad);
        }

        private static RectTransform Rect(Transform p, string n)
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
    }
}
