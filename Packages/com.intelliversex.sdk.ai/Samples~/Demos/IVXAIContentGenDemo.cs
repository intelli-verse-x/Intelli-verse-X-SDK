using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using IntelliVerseX.AI;

namespace IntelliVerseX.Demos
{
    /// <summary>
    /// Self-contained AI content generation demo: quest, story, item, dialogue tabs.
    /// </summary>
    public sealed class IVXAIContentGenDemo : MonoBehaviour
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

        private IVXAIContentGenerator _gen;
        private int _tab;

        private RectTransform[] _tabPanels = new RectTransform[4];
        private Button[] _tabButtons = new Button[4];
        private GameObject _progressBlock;
        private TextMeshProUGUI _progressLabel;

        private string _questGenre = "fantasy";
        private string _questDifficulty = "medium";
        private TextMeshProUGUI _questOutput;

        private TMP_InputField _storyPrompt;
        private string _storyGenre = "fantasy";
        private Slider _storyWordSlider;
        private TextMeshProUGUI _storyWordValueLabel;
        private TextMeshProUGUI _storyOutput;
        private ScrollRect _storyScroll;

        private TMP_InputField _itemName;
        private TMP_InputField _itemType;
        private TMP_InputField _itemRarity;
        private TextMeshProUGUI _itemOutput;

        private TMP_InputField _dlgScenario;
        private TMP_InputField _dlgChars;
        private TextMeshProUGUI _dlgOutput;
        private ScrollRect _dlgScroll;

        private void Start()
        {
            EnsureEventSystem();
            BuildUi();
            WireGenerator();
            SelectTab(0);
        }

        private void Update()
        {
            if (_gen == null || _progressBlock == null)
                return;
            bool g = _gen.IsGenerating;
            _progressBlock.SetActive(g);
            if (_progressLabel != null)
                _progressLabel.text = g ? "Generating\u2026" : string.Empty;
        }

        private void OnDestroy()
        {
            if (_gen == null)
                return;
            _gen.OnError -= HandleError;
        }

        private static void EnsureEventSystem()
        {
            if (FindFirstObjectByType<EventSystem>() != null)
                return;
            new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
        }

        private void WireGenerator()
        {
            _gen = IVXAIContentGenerator.Instance;
            if (_gen == null)
                _gen = new GameObject("IVXAIContentGenerator").AddComponent<IVXAIContentGenerator>();
            if (_aiConfig != null)
                _gen.Initialize(_aiConfig);
            _gen.OnError += HandleError;
        }

        private void HandleError(string err)
        {
            Debug.LogWarning($"[{nameof(IVXAIContentGenDemo)}] {err}");
        }

        private void SelectTab(int index)
        {
            _tab = Mathf.Clamp(index, 0, 3);
            for (int i = 0; i < _tabPanels.Length; i++)
            {
                if (_tabPanels[i] != null)
                    _tabPanels[i].gameObject.SetActive(i == _tab);
            }

            for (int i = 0; i < _tabButtons.Length; i++)
            {
                if (_tabButtons[i] == null)
                    continue;
                var img = _tabButtons[i].GetComponent<Image>();
                if (img != null)
                    img.color = i == _tab ? ColHighlight : ColAccent;
            }
        }

        private void OnGenerateQuest()
        {
            if (_gen == null || _aiConfig == null)
                return;

            var tmpl = new IVXQuestTemplate
            {
                Genre = _questGenre,
                Difficulty = _questDifficulty,
                RequiredElements = new[] { "exploration", "combat" },
                EstimatedDurationMinutes = 15,
                CustomPrompt = "Make it suitable for a casual mobile RPG."
            };

            _gen.GenerateQuest(tmpl, "demo_player", q =>
            {
                if (q == null || _questOutput == null)
                    return;
                var sb = new StringBuilder();
                sb.AppendLine($"<b>{q.Title}</b>");
                sb.AppendLine(q.Description);
                sb.AppendLine();
                sb.AppendLine("<b>Objectives</b>");
                if (q.Objectives != null)
                {
                    foreach (string o in q.Objectives)
                        sb.AppendLine("• " + o);
                }

                sb.AppendLine();
                sb.AppendLine("<b>Rewards</b>");
                if (q.Rewards != null)
                {
                    foreach (string r in q.Rewards)
                        sb.AppendLine("• " + r);
                }

                sb.AppendLine();
                sb.AppendLine($"Difficulty: {q.Difficulty}  |  ~{q.EstimatedDurationMinutes} min");
                if (!string.IsNullOrEmpty(q.NarrativeHook))
                    sb.AppendLine("Hook: " + q.NarrativeHook);

                _questOutput.text = sb.ToString();
            });
        }

        private void OnGenerateStory()
        {
            if (_gen == null || _aiConfig == null)
                return;
            string prompt = _storyPrompt != null ? _storyPrompt.text?.Trim() : "A hero opens a forbidden door.";
            int maxWords = _storyWordSlider != null
                ? Mathf.RoundToInt(_storyWordSlider.value)
                : 500;
            _gen.GenerateStory(prompt, _storyGenre, maxWords, s =>
            {
                if (s == null || _storyOutput == null)
                    return;
                var sb = new StringBuilder();
                if (!string.IsNullOrEmpty(s.Title))
                    sb.AppendLine($"<b>{s.Title}</b>\n");
                sb.AppendLine(s.Content);
                sb.AppendLine();
                sb.AppendLine($"<size=12><color=#8899aa>Genre: {s.Genre} | Words: {s.WordCount}</color></size>");
                _storyOutput.text = sb.ToString();
                Canvas.ForceUpdateCanvases();
                if (_storyScroll != null)
                    _storyScroll.verticalNormalizedPosition = 1f;
            });
        }

        private void OnGenerateItem()
        {
            if (_gen == null || _aiConfig == null)
                return;
            string n = _itemName != null ? _itemName.text?.Trim() : "Mystic Blade";
            string t = _itemType != null ? _itemType.text?.Trim() : "weapon";
            string r = _itemRarity != null ? _itemRarity.text?.Trim() : "rare";
            _gen.GenerateItemDescription(n, t, r, item =>
            {
                if (item == null || _itemOutput == null)
                    return;
                var sb = new StringBuilder();
                sb.AppendLine($"<b>{item.ItemName}</b> ({item.ItemType}, {item.Rarity})");
                sb.AppendLine();
                sb.AppendLine(item.FlavorText);
                sb.AppendLine(item.Description);
                sb.AppendLine();
                if (item.Stats != null && item.Stats.Count > 0)
                {
                    sb.AppendLine("<b>Stats</b>");
                    foreach (var kv in item.Stats)
                        sb.AppendLine($"{kv.Key}: {kv.Value}");
                }

                _itemOutput.text = sb.ToString();
            });
        }

        private void OnGenerateDialogue()
        {
            if (_gen == null || _aiConfig == null)
                return;
            string scenario = _dlgScenario != null ? _dlgScenario.text?.Trim() : "A tense negotiation at the city gate.";
            string charsRaw = _dlgChars != null ? _dlgChars.text : "Guard,Mercenary,Merchant";
            string[] chars = charsRaw.Split(new[] { ',', ';' }, System.StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < chars.Length; i++)
                chars[i] = chars[i].Trim();

            _gen.GenerateDialogue(scenario, chars, d =>
            {
                if (d == null || _dlgOutput == null)
                    return;
                var sb = new StringBuilder();
                if (d.Lines != null)
                {
                    foreach (var line in d.Lines)
                    {
                        sb.AppendLine($"<b>{line.Character}</b> <i>({line.Emotion})</i>");
                        if (!string.IsNullOrEmpty(line.Action))
                            sb.AppendLine($"[{line.Action}]");
                        sb.AppendLine(line.Text);
                        sb.AppendLine();
                    }
                }

                _dlgOutput.text = sb.ToString();
                Canvas.ForceUpdateCanvases();
                if (_dlgScroll != null)
                    _dlgScroll.verticalNormalizedPosition = 1f;
            });
        }

        private void OnCancelGen()
        {
            _gen?.CancelGeneration();
        }

        #region UI

        private void BuildUi()
        {
            var canvasGo = new GameObject("IVXAIContentGenDemoCanvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
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
            VLayout(root.gameObject, 8f, new RectOffset(16, 16, 16, 16));

            Tmp(root, "Title", "AI Content Generation", 26f, FontStyles.Bold, ColText).gameObject.AddComponent<LayoutElement>().minHeight = 32f;
            Tmp(root, "Sub", "IVXAIContentGenerator — quest, story, item, dialogue", 14f, FontStyles.Normal, ColDim).gameObject.AddComponent<LayoutElement>().minHeight = 20f;

            var progRow = Rect(root, "Prog");
            progRow.gameObject.AddComponent<LayoutElement>().minHeight = 26f;
            _progressBlock = new GameObject("Progress");
            _progressBlock.transform.SetParent(progRow, false);
            var prt = _progressBlock.AddComponent<RectTransform>();
            Stretch(prt);
            _progressBlock.SetActive(false);
            _progressLabel = Tmp(prt, "P", string.Empty, 15f, FontStyles.Bold, ColHighlight, TextAlignmentOptions.Center);
            Stretch(_progressLabel.rectTransform);

            var tabBar = Rect(root, "TabBar");
            tabBar.gameObject.AddComponent<LayoutElement>().minHeight = 48f;
            var th = tabBar.gameObject.AddComponent<HorizontalLayoutGroup>();
            th.spacing = 8f;
            th.childForceExpandWidth = true;
            string[] labels = { "Quest", "Story", "Item", "Dialogue" };
            for (int i = 0; i < 4; i++)
            {
                int idx = i;
                var b = TabBtn(tabBar, labels[i], () => SelectTab(idx));
                _tabButtons[i] = b;
            }

            var body = Rect(root, "Body");
            body.gameObject.AddComponent<LayoutElement>().flexibleHeight = 1f;

            _tabPanels[0] = BuildQuestTab(body);
            _tabPanels[1] = BuildStoryTab(body);
            _tabPanels[2] = BuildItemTab(body);
            _tabPanels[3] = BuildDialogueTab(body);

            var cancelRow = Rect(root, "CancelRow");
            cancelRow.gameObject.AddComponent<LayoutElement>().minHeight = 48f;
            Btn(cancelRow, "Cancel generation", ColHighlight, OnCancelGen);

            for (int i = 0; i < _tabPanels.Length; i++)
            {
                if (_tabPanels[i] != null)
                    _tabPanels[i].gameObject.SetActive(false);
            }
        }

        private RectTransform BuildQuestTab(RectTransform body)
        {
            var p = Panel(body, "QuestTab", ColPanel);
            Stretch(p);
            var v = VLayout(p.gameObject, 10f, new RectOffset(14, 14, 14, 14));

            Tmp(p, "H", "Quest", 18f, FontStyles.Bold, ColText).gameObject.AddComponent<LayoutElement>().minHeight = 26f;
            var gRow = Rect(p, "GenreRow");
            gRow.gameObject.AddComponent<LayoutElement>().minHeight = 40f;
            var gh = gRow.gameObject.AddComponent<HorizontalLayoutGroup>();
            gh.spacing = 8f;
            Tmp(gRow, "gL", "Genre:", 14f, FontStyles.Bold, ColDim).gameObject.AddComponent<LayoutElement>().minWidth = 56f;
            Chip(gRow, "Fantasy", () => { _questGenre = "fantasy"; });
            Chip(gRow, "Sci-Fi", () => { _questGenre = "sci-fi"; });
            Chip(gRow, "Horror", () => { _questGenre = "horror"; });

            var dRow = Rect(p, "DiffRow");
            dRow.gameObject.AddComponent<LayoutElement>().minHeight = 40f;
            dRow.gameObject.AddComponent<HorizontalLayoutGroup>().spacing = 8f;
            Tmp(dRow, "dL", "Difficulty:", 14f, FontStyles.Bold, ColDim).gameObject.AddComponent<LayoutElement>().minWidth = 80f;
            Chip(dRow, "Easy", () => { _questDifficulty = "easy"; });
            Chip(dRow, "Medium", () => { _questDifficulty = "medium"; });
            Chip(dRow, "Hard", () => { _questDifficulty = "hard"; });

            var genRow = Rect(p, "GenQuest");
            genRow.gameObject.AddComponent<LayoutElement>().minHeight = 44f;
            Btn(genRow, "Generate Quest", ColAccent, OnGenerateQuest);

            var outScroll = ScrollBox(p, "QuestOut");
            outScroll.rt.gameObject.AddComponent<LayoutElement>().flexibleHeight = 1f;
            _questOutput = Tmp(outScroll.content, "Txt", string.Empty, 14f, FontStyles.Normal, ColText, TextAlignmentOptions.TopLeft);
            _questOutput.textWrappingMode = TextWrappingModes.Normal;
            outScroll.content.gameObject.AddComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            return p;
        }

        private RectTransform BuildStoryTab(RectTransform body)
        {
            var p = Panel(body, "StoryTab", ColPanel);
            Stretch(p);
            VLayout(p.gameObject, 10f, new RectOffset(14, 14, 14, 14));

            Tmp(p, "H", "Story", 18f, FontStyles.Bold, ColText).gameObject.AddComponent<LayoutElement>().minHeight = 26f;
            _storyPrompt = TmpInput(p, "StoryPrompt", "Story prompt\u2026");
            _storyPrompt.gameObject.AddComponent<LayoutElement>().minHeight = 72f;

            var gRow = Rect(p, "SGenre");
            gRow.gameObject.AddComponent<LayoutElement>().minHeight = 40f;
            gRow.gameObject.AddComponent<HorizontalLayoutGroup>().spacing = 8f;
            Tmp(gRow, "sg", "Genre:", 14f, FontStyles.Bold, ColDim).gameObject.AddComponent<LayoutElement>().minWidth = 56f;
            Chip(gRow, "Fantasy", () => { _storyGenre = "fantasy"; });
            Chip(gRow, "Mystery", () => { _storyGenre = "mystery"; });
            Chip(gRow, "Comedy", () => { _storyGenre = "comedy"; });

            var slRow = Rect(p, "WordsRow");
            slRow.gameObject.AddComponent<LayoutElement>().minHeight = 44f;
            var sh = slRow.gameObject.AddComponent<HorizontalLayoutGroup>();
            sh.spacing = 12f;
            sh.childForceExpandWidth = true;
            sh.childAlignment = TextAnchor.MiddleLeft;
            Tmp(slRow, "wl", "Max words", 14f, FontStyles.Bold, ColDim).gameObject.AddComponent<LayoutElement>().minWidth = 88f;
            BuildWordSlider(slRow);

            var genRow = Rect(p, "GenStory");
            genRow.gameObject.AddComponent<LayoutElement>().minHeight = 44f;
            Btn(genRow, "Generate Story", ColAccent, OnGenerateStory);

            var outScroll = ScrollBox(p, "StoryOut");
            outScroll.rt.gameObject.AddComponent<LayoutElement>().flexibleHeight = 1f;
            _storyOutput = Tmp(outScroll.content, "Txt", string.Empty, 14f, FontStyles.Normal, ColText, TextAlignmentOptions.TopLeft);
            _storyOutput.textWrappingMode = TextWrappingModes.Normal;
            outScroll.content.gameObject.AddComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            _storyScroll = outScroll.scroll;
            return p;
        }

        private RectTransform BuildItemTab(RectTransform body)
        {
            var p = Panel(body, "ItemTab", ColPanel);
            Stretch(p);
            VLayout(p.gameObject, 10f, new RectOffset(14, 14, 14, 14));

            Tmp(p, "H", "Item", 18f, FontStyles.Bold, ColText).gameObject.AddComponent<LayoutElement>().minHeight = 26f;
            var r1 = Rect(p, "R1");
            r1.gameObject.AddComponent<LayoutElement>().minHeight = 44f;
            var h1 = r1.gameObject.AddComponent<HorizontalLayoutGroup>();
            h1.spacing = 8f;
            h1.childForceExpandWidth = true;
            _itemName = TmpInput(r1, "IName", "Name");
            _itemName.gameObject.AddComponent<LayoutElement>().flexibleWidth = 1f;
            _itemType = TmpInput(r1, "IType", "Type");
            _itemType.gameObject.AddComponent<LayoutElement>().flexibleWidth = 1f;
            _itemRarity = TmpInput(r1, "IRar", "Rarity");
            _itemRarity.gameObject.AddComponent<LayoutElement>().flexibleWidth = 1f;

            var genRow = Rect(p, "GenItem");
            genRow.gameObject.AddComponent<LayoutElement>().minHeight = 44f;
            Btn(genRow, "Generate Description", ColAccent, OnGenerateItem);

            var outScroll = ScrollBox(p, "ItemOut");
            outScroll.rt.gameObject.AddComponent<LayoutElement>().flexibleHeight = 1f;
            _itemOutput = Tmp(outScroll.content, "Txt", string.Empty, 14f, FontStyles.Normal, ColText, TextAlignmentOptions.TopLeft);
            _itemOutput.textWrappingMode = TextWrappingModes.Normal;
            outScroll.content.gameObject.AddComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            return p;
        }

        private RectTransform BuildDialogueTab(RectTransform body)
        {
            var p = Panel(body, "DlgTab", ColPanel);
            Stretch(p);
            VLayout(p.gameObject, 10f, new RectOffset(14, 14, 14, 14));

            Tmp(p, "H", "Dialogue", 18f, FontStyles.Bold, ColText).gameObject.AddComponent<LayoutElement>().minHeight = 26f;
            _dlgScenario = TmpInput(p, "Scen", "Scenario");
            _dlgScenario.gameObject.AddComponent<LayoutElement>().minHeight = 64f;
            _dlgChars = TmpInput(p, "Chars", "Characters (comma-separated)");
            _dlgChars.gameObject.AddComponent<LayoutElement>().minHeight = 44f;

            var genRow = Rect(p, "GenDlg");
            genRow.gameObject.AddComponent<LayoutElement>().minHeight = 44f;
            Btn(genRow, "Generate Dialogue", ColAccent, OnGenerateDialogue);

            var outScroll = ScrollBox(p, "DlgOut");
            outScroll.rt.gameObject.AddComponent<LayoutElement>().flexibleHeight = 1f;
            _dlgOutput = Tmp(outScroll.content, "Txt", string.Empty, 14f, FontStyles.Normal, ColText, TextAlignmentOptions.TopLeft);
            _dlgOutput.textWrappingMode = TextWrappingModes.Normal;
            outScroll.content.gameObject.AddComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            _dlgScroll = outScroll.scroll;
            return p;
        }

        private struct ScrollBoxResult
        {
            public RectTransform rt;
            public RectTransform content;
            public ScrollRect scroll;
        }

        private static ScrollBoxResult ScrollBox(RectTransform parent, string name)
        {
            var scrollRt = Rect(parent, name);
            scrollRt.gameObject.AddComponent<Image>().color = new Color(0.07f, 0.08f, 0.11f, 1f);
            scrollRt.gameObject.AddComponent<RectMask2D>();
            var scroll = scrollRt.gameObject.AddComponent<ScrollRect>();
            scroll.horizontal = false;
            scroll.vertical = true;
            var vp = Rect(scrollRt, "VP");
            Stretch(vp);
            var content = Rect(vp, "Content");
            content.anchorMin = new Vector2(0f, 1f);
            content.anchorMax = new Vector2(1f, 1f);
            content.pivot = new Vector2(0.5f, 1f);
            VLayout(content.gameObject, 6f, new RectOffset(10, 10, 10, 10));
            scroll.viewport = vp;
            scroll.content = content;
            return new ScrollBoxResult { rt = scrollRt, content = content, scroll = scroll };
        }

        private void BuildWordSlider(RectTransform parent)
        {
            var track = Rect(parent, "WordSlider");
            track.gameObject.AddComponent<LayoutElement>().flexibleWidth = 1f;
            track.gameObject.AddComponent<LayoutElement>().minHeight = 28f;
            var bg = Rect(track, "Bg");
            Stretch(bg);
            bg.gameObject.AddComponent<Image>().color = new Color(0.12f, 0.14f, 0.2f, 1f);
            var fillArea = Rect(track, "FillArea");
            Stretch(fillArea);
            var fill = Rect(fillArea, "Fill");
            var fr = fill.GetComponent<RectTransform>();
            fr.anchorMin = new Vector2(0f, 0f);
            fr.anchorMax = new Vector2(0f, 1f);
            fr.pivot = new Vector2(0f, 0.5f);
            fr.sizeDelta = new Vector2(0f, 0f);
            fr.anchoredPosition = Vector2.zero;
            fill.gameObject.AddComponent<Image>().color = ColAccent;
            var handleArea = Rect(track, "HandleSlide");
            Stretch(handleArea);
            var handle = Rect(handleArea, "Handle");
            var hrt = handle.GetComponent<RectTransform>();
            hrt.sizeDelta = new Vector2(22f, 22f);
            hrt.anchorMin = new Vector2(0f, 0.5f);
            hrt.anchorMax = new Vector2(0f, 0.5f);
            hrt.pivot = new Vector2(0.5f, 0.5f);
            handle.gameObject.AddComponent<Image>().color = ColHighlight;
            _storyWordSlider = track.gameObject.AddComponent<Slider>();
            _storyWordSlider.fillRect = fr;
            _storyWordSlider.handleRect = hrt;
            _storyWordSlider.targetGraphic = handle.GetComponent<Image>();
            _storyWordSlider.minValue = 100f;
            _storyWordSlider.maxValue = 2000f;
            _storyWordSlider.wholeNumbers = true;
            _storyWordSlider.value = 500f;
            _storyWordValueLabel = Tmp(parent, "WordVal", "500", 14f, FontStyles.Bold, ColText, TextAlignmentOptions.MidlineRight);
            _storyWordValueLabel.gameObject.AddComponent<LayoutElement>().minWidth = 48f;
            _storyWordSlider.onValueChanged.AddListener(v =>
            {
                if (_storyWordValueLabel != null)
                    _storyWordValueLabel.text = Mathf.RoundToInt(v).ToString();
            });
        }

        private void Chip(RectTransform row, string label, UnityEngine.Events.UnityAction onClick)
        {
            var r = Rect(row, "C_" + label);
            var img = r.gameObject.AddComponent<Image>();
            img.color = ColAccent;
            var b = r.gameObject.AddComponent<Button>();
            b.targetGraphic = img;
            b.onClick.AddListener(onClick);
            r.gameObject.AddComponent<LayoutElement>().minWidth = 88f;
            r.gameObject.AddComponent<LayoutElement>().minHeight = 36f;
            var t = Tmp(r, "T", label, 13f, FontStyles.Bold, ColText, TextAlignmentOptions.Center);
            Stretch(t.rectTransform);
        }

        private static Button TabBtn(RectTransform parent, string label, UnityEngine.Events.UnityAction onClick)
        {
            var r = Rect(parent, "Tab_" + label);
            var img = r.gameObject.AddComponent<Image>();
            img.color = ColAccent;
            var b = r.gameObject.AddComponent<Button>();
            b.targetGraphic = img;
            b.onClick.AddListener(onClick);
            r.gameObject.AddComponent<LayoutElement>().minHeight = 44f;
            var t = Tmp(r, "T", label, 15f, FontStyles.Bold, ColText, TextAlignmentOptions.Center);
            Stretch(t.rectTransform);
            return b;
        }

        private static void Btn(RectTransform parent, string label, Color32 bg, UnityEngine.Events.UnityAction onClick)
        {
            var r = Rect(parent, "B");
            var img = r.gameObject.AddComponent<Image>();
            img.color = bg;
            var b = r.gameObject.AddComponent<Button>();
            b.targetGraphic = img;
            b.onClick.AddListener(onClick);
            r.gameObject.AddComponent<LayoutElement>().minHeight = 44f;
            var t = Tmp(r, "T", label, 15f, FontStyles.Bold, ColText, TextAlignmentOptions.Center);
            Stretch(t.rectTransform);
        }

        private static TMP_InputField TmpInput(RectTransform parent, string name, string ph)
        {
            var rt = Rect(parent, name);
            rt.gameObject.AddComponent<Image>().color = new Color(0.09f, 0.11f, 0.16f, 1f);
            var field = rt.gameObject.AddComponent<TMP_InputField>();
            field.lineType = TMP_InputField.LineType.MultiLineNewline;
            var tr = Rect(rt, "Text");
            StretchPad(tr, 10f, 8f);
            var tmp = tr.gameObject.AddComponent<TextMeshProUGUI>();
            tmp.fontSize = 14f;
            tmp.color = ColText;
            tmp.enableWordWrapping = true;
            var phr = Rect(rt, "Ph");
            StretchPad(phr, 10f, 8f);
            var pht = phr.gameObject.AddComponent<TextMeshProUGUI>();
            pht.text = ph;
            pht.fontSize = 14f;
            pht.fontStyle = FontStyles.Italic;
            pht.color = new Color(1f, 1f, 1f, 0.35f);
            field.textComponent = tmp;
            field.placeholder = pht;
            field.textViewport = rt;
            return field;
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

        private static void Stretch(RectTransform r)
        {
            r.anchorMin = Vector2.zero;
            r.anchorMax = Vector2.one;
            r.offsetMin = Vector2.zero;
            r.offsetMax = Vector2.zero;
        }

        private static void StretchPad(RectTransform r, float px, float py)
        {
            r.anchorMin = Vector2.zero;
            r.anchorMax = Vector2.one;
            r.offsetMin = new Vector2(px, py);
            r.offsetMax = new Vector2(-px, -py);
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
