using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using IntelliVerseX.AI;

namespace IntelliVerseX.Demos
{
    /// <summary>
    /// Self-contained AI moderation demo: classify, filter, batch scan, custom rules, Discord metadata.
    /// </summary>
    public sealed class IVXAIModerationDemo : MonoBehaviour
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

        private IVXAIModerator _mod;
        private TMP_InputField _testMessage;
        private TextMeshProUGUI _resultCategory;
        private TextMeshProUGUI _resultSeverity;
        private TextMeshProUGUI _resultConfidence;
        private TextMeshProUGUI _resultAction;
        private TextMeshProUGUI _resultReplacement;
        private TextMeshProUGUI _resultFiltered;
        private TextMeshProUGUI _batchSummary;
        private TMP_InputField _rulePattern;
        private TMP_InputField _ruleReplace;
        private TextMeshProUGUI _actionPickLabel;
        private IVXModerationActionType _pickedAction = IVXModerationActionType.Block;
        private readonly string[] _actionNames = { "Allow", "Warn", "Replace", "Block", "Flag" };
        private RectTransform _rulesContent;
        private TextMeshProUGUI _discordMeta;
        private IVXModerationResult _lastResult;

        private static readonly string[] BatchPresets =
        {
            "Hello, good luck in the match!",
            "Buy cheap gold now!!! click here",
            "I will find you and hurt your family",
            "My email is test@example.com and my phone is 555-0100"
        };

        private void Start()
        {
            EnsureEventSystem();
            BuildUi();
            WireModerator();
            RefreshRulesList();
        }

        private static void EnsureEventSystem()
        {
            if (FindFirstObjectByType<EventSystem>() != null)
                return;
            new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
        }

        private void WireModerator()
        {
            _mod = IVXAIModerator.Instance;
            if (_mod == null)
                _mod = new GameObject("IVXAIModerator").AddComponent<IVXAIModerator>();
            if (_aiConfig != null)
                _mod.Initialize(_aiConfig);
        }

        private void OnClassify()
        {
            if (_mod == null)
                return;
            string t = _testMessage != null ? _testMessage.text : string.Empty;
            _mod.ClassifyText(t, r =>
            {
                _lastResult = r;
                ApplyResultToUi(r);
            });
        }

        private void OnFilter()
        {
            if (_mod == null)
                return;
            string t = _testMessage != null ? _testMessage.text : string.Empty;
            _mod.FilterMessage(t, filtered =>
            {
                if (_resultFiltered != null)
                    _resultFiltered.text = string.IsNullOrEmpty(filtered) ? "(empty)" : filtered;
            });
        }

        private void OnBatch()
        {
            if (_mod == null)
                return;
            var list = new List<string>(BatchPresets);
            _mod.ScanBatch(list, results =>
            {
                var sb = new StringBuilder();
                for (int i = 0; i < results.Count; i++)
                {
                    IVXModerationResult r = results[i];
                    sb.AppendLine($"[{i}] {r.Category} | {r.Severity} | {r.SuggestedAction} | conf={r.Confidence:0.###}");
                }

                if (_batchSummary != null)
                    _batchSummary.text = sb.ToString();
            });
        }

        private void OnAddRule()
        {
            if (_mod == null || _rulePattern == null)
                return;
            string pat = _rulePattern.text?.Trim();
            if (string.IsNullOrEmpty(pat))
                return;

            var rule = new IVXModerationRule
            {
                Pattern = pat,
                Category = IVXContentCategory.Custom,
                Action = _pickedAction,
                ReplacementText = _ruleReplace != null ? _ruleReplace.text : string.Empty
            };
            _mod.AddCustomRule(rule);
            _rulePattern.text = string.Empty;
            if (_ruleReplace != null)
                _ruleReplace.text = string.Empty;
            RefreshRulesList();
        }

        private void OnDiscordMeta()
        {
            if (_discordMeta == null)
                return;
            if (_lastResult == null)
            {
                _discordMeta.text = "Run Classify first.";
                return;
            }

            Dictionary<string, string> d = _mod.GetDiscordModerationMetadata(_lastResult);
            var sb = new StringBuilder();
            foreach (var kv in d)
                sb.AppendLine($"{kv.Key} = {kv.Value}");
            _discordMeta.text = sb.ToString();
        }

        private void ApplyResultToUi(IVXModerationResult r)
        {
            if (r == null)
                return;
            if (_resultCategory != null) _resultCategory.text = r.Category.ToString();
            if (_resultSeverity != null) _resultSeverity.text = r.Severity.ToString();
            if (_resultConfidence != null) _resultConfidence.text = r.Confidence.ToString("0.###");
            if (_resultAction != null) _resultAction.text = r.SuggestedAction.ToString();
            if (_resultReplacement != null) _resultReplacement.text = string.IsNullOrEmpty(r.Replacement) ? "—" : r.Replacement;
        }

        private void RefreshRulesList()
        {
            if (_rulesContent == null || _mod == null)
                return;
            for (int i = _rulesContent.childCount - 1; i >= 0; i--)
                Destroy(_rulesContent.GetChild(i).gameObject);

            IReadOnlyList<IVXModerationRule> rules = _mod.CustomRules;
            for (int i = 0; i < rules.Count; i++)
            {
                IVXModerationRule rule = rules[i];
                var row = Panel(_rulesContent, "R" + i, ColPanel);
                row.gameObject.AddComponent<LayoutElement>().minHeight = 36f;
                string line = $"{rule.Pattern}  →  {rule.Action}  (cat {rule.Category})";
                if (rule.Action == IVXModerationActionType.Replace && !string.IsNullOrEmpty(rule.ReplacementText))
                    line += $"  replace:\"{rule.ReplacementText}\"";
                Tmp(row, "T", line, 13f, FontStyles.Normal, ColText, TextAlignmentOptions.MidlineLeft);
            }

            Canvas.ForceUpdateCanvases();
        }

        private void CycleAction(int delta)
        {
            int n = System.Enum.GetValues(typeof(IVXModerationActionType)).Length;
            int cur = (int)_pickedAction;
            cur = (cur + delta + n) % n;
            _pickedAction = (IVXModerationActionType)cur;
            if (_actionPickLabel != null)
                _actionPickLabel.text = "Action: " + _pickedAction;
        }

        #region UI build

        private void BuildUi()
        {
            var canvasGo = new GameObject("IVXAIModerationDemoCanvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
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
            VLayout(root.gameObject, 10f, new RectOffset(18, 18, 18, 18));

            Tmp(root, "Title", "AI Moderation", 26f, FontStyles.Bold, ColText).gameObject.AddComponent<LayoutElement>().minHeight = 34f;
            Tmp(root, "Sub", "IVXAIModerator — classify, filter, batch, rules, Discord metadata", 14f, FontStyles.Normal, ColDim).gameObject.AddComponent<LayoutElement>().minHeight = 20f;

            var inputBlock = Rect(root, "InputBlock");
            inputBlock.gameObject.AddComponent<LayoutElement>().minHeight = 160f;
            inputBlock.gameObject.AddComponent<Image>().color = ColPanel;
            var iv = VLayout(inputBlock.gameObject, 8f, new RectOffset(12, 12, 12, 12));
            iv.childForceExpandHeight = false;
            Tmp(inputBlock, "Lbl", "Test message", 14f, FontStyles.Bold, ColText).gameObject.AddComponent<LayoutElement>().minHeight = 22f;
            _testMessage = TmpInputMultiline(inputBlock, "TestInput");
            _testMessage.gameObject.AddComponent<LayoutElement>().flexibleHeight = 1f;

            var btnRow = Rect(root, "BtnRow");
            btnRow.gameObject.AddComponent<LayoutElement>().minHeight = 48f;
            var bh = btnRow.gameObject.AddComponent<HorizontalLayoutGroup>();
            bh.spacing = 10f;
            bh.childForceExpandWidth = true;
            Btn(btnRow, "Classify", ColAccent, OnClassify);
            Btn(btnRow, "Filter", ColHighlight, OnFilter);
            Btn(btnRow, "Scan Batch", ColAccent, OnBatch);

            var resPanel = Panel(root, "Results", ColPanel);
            resPanel.gameObject.AddComponent<LayoutElement>().minHeight = 140f;
            var rv = VLayout(resPanel.gameObject, 4f, new RectOffset(12, 12, 10, 10));
            Tmp(resPanel, "RH", "Last classify result", 16f, FontStyles.Bold, ColText).gameObject.AddComponent<LayoutElement>().minHeight = 24f;
            _resultCategory = Row(resPanel, "Category: ", "—");
            _resultSeverity = Row(resPanel, "Severity: ", "—");
            _resultConfidence = Row(resPanel, "Confidence: ", "—");
            _resultAction = Row(resPanel, "Suggested action: ", "—");
            _resultReplacement = Row(resPanel, "Replacement: ", "—");
            _resultFiltered = Row(resPanel, "Filtered output: ", "—");
            _batchSummary = Row(resPanel, "Batch scan: ", "—");

            var rulePanel = Panel(root, "CustomRules", ColPanel);
            rulePanel.gameObject.AddComponent<LayoutElement>().minHeight = 200f;
            VLayout(rulePanel.gameObject, 8f, new RectOffset(12, 12, 10, 10));
            Tmp(rulePanel, "CRH", "Custom rules", 16f, FontStyles.Bold, ColText).gameObject.AddComponent<LayoutElement>().minHeight = 24f;

            var pickRow = Rect(rulePanel, "PickRow");
            pickRow.gameObject.AddComponent<LayoutElement>().minHeight = 40f;
            var ph = pickRow.gameObject.AddComponent<HorizontalLayoutGroup>();
            ph.spacing = 8f;
            _actionPickLabel = Tmp(pickRow, "ActLbl", "Action: Block", 15f, FontStyles.Bold, ColText);
            _actionPickLabel.gameObject.AddComponent<LayoutElement>().minWidth = 200f;
            Btn(pickRow, "<", ColAccent, () => CycleAction(-1)).gameObject.AddComponent<LayoutElement>().minWidth = 48f;
            Btn(pickRow, ">", ColAccent, () => CycleAction(1)).gameObject.AddComponent<LayoutElement>().minWidth = 48f;
            for (int i = 0; i < _actionNames.Length; i++)
            {
                int idx = i;
                Btn(pickRow, _actionNames[i], ColHighlight, () =>
                {
                    _pickedAction = (IVXModerationActionType)idx;
                    if (_actionPickLabel != null)
                        _actionPickLabel.text = "Action: " + _pickedAction;
                }).gameObject.AddComponent<LayoutElement>().minWidth = 72f;
            }

            var ruleInRow = Rect(rulePanel, "RuleIn");
            ruleInRow.gameObject.AddComponent<LayoutElement>().minHeight = 44f;
            var rh = ruleInRow.gameObject.AddComponent<HorizontalLayoutGroup>();
            rh.spacing = 8f;
            rh.childForceExpandWidth = true;
            _rulePattern = TmpInput(ruleInRow, "Pattern", "regex or keyword");
            _rulePattern.gameObject.AddComponent<LayoutElement>().flexibleWidth = 1f;
            _ruleReplace = TmpInput(ruleInRow, "Replace", "replacement if Replace");
            _ruleReplace.gameObject.AddComponent<LayoutElement>().flexibleWidth = 0.6f;
            Btn(ruleInRow, "Add Rule", ColAccent, OnAddRule).gameObject.AddComponent<LayoutElement>().minWidth = 120f;

            var rulesScrollRt = Rect(rulePanel, "RulesScroll");
            rulesScrollRt.gameObject.AddComponent<LayoutElement>().flexibleHeight = 1f;
            rulesScrollRt.gameObject.AddComponent<Image>().color = new Color(0.07f, 0.08f, 0.12f, 1f);
            rulesScrollRt.gameObject.AddComponent<RectMask2D>();
            var rs = rulesScrollRt.gameObject.AddComponent<ScrollRect>();
            rs.horizontal = false;
            rs.vertical = true;
            var rvp = Rect(rulesScrollRt, "RVP");
            Stretch(rvp);
            _rulesContent = Rect(rvp, "RContent");
            _rulesContent.anchorMin = new Vector2(0f, 1f);
            _rulesContent.anchorMax = new Vector2(1f, 1f);
            _rulesContent.pivot = new Vector2(0.5f, 1f);
            VLayout(_rulesContent.gameObject, 4f, new RectOffset(6, 6, 6, 6));
            _rulesContent.gameObject.AddComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            rs.viewport = rvp;
            rs.content = _rulesContent;

            var integ = Panel(root, "Integration", ColPanel);
            integ.gameObject.AddComponent<LayoutElement>().minHeight = 120f;
            VLayout(integ.gameObject, 6f, new RectOffset(12, 12, 10, 10));
            var ir = Rect(integ, "IntRow");
            ir.gameObject.AddComponent<LayoutElement>().minHeight = 40f;
            Btn(ir, "Get Discord Metadata", ColHighlight, OnDiscordMeta).gameObject.AddComponent<LayoutElement>().minHeight = 40f;
            _discordMeta = Tmp(integ, "DiscordTxt", "(metadata appears here)", 13f, FontStyles.Normal, ColDim, TextAlignmentOptions.TopLeft);
            _discordMeta.textWrappingMode = TextWrappingModes.Normal;
            _discordMeta.gameObject.AddComponent<LayoutElement>().flexibleHeight = 1f;
        }

        private TextMeshProUGUI Row(RectTransform parent, string label, string initial)
        {
            var row = Rect(parent, "Row_" + label);
            row.gameObject.AddComponent<LayoutElement>().minHeight = 22f;
            var h = row.gameObject.AddComponent<HorizontalLayoutGroup>();
            h.spacing = 6f;
            h.childControlWidth = false;
            Tmp(row, "L", label, 14f, FontStyles.Bold, ColDim).gameObject.AddComponent<LayoutElement>().minWidth = 160f;
            return Tmp(row, "V", initial, 14f, FontStyles.Normal, ColText);
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
            var t = Tmp(r, "T", label, 14f, FontStyles.Bold, ColText, TextAlignmentOptions.Center);
            Stretch(t.rectTransform);
            return r;
        }

        private static TMP_InputField TmpInput(RectTransform parent, string name, string ph)
        {
            var rt = Rect(parent, name);
            rt.gameObject.AddComponent<Image>().color = new Color(0.09f, 0.11f, 0.16f, 1f);
            var field = rt.gameObject.AddComponent<TMP_InputField>();
            var tr = Rect(rt, "Text");
            StretchPad(tr, 10f, 8f);
            var tmp = tr.gameObject.AddComponent<TextMeshProUGUI>();
            tmp.fontSize = 14f;
            tmp.color = ColText;
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

        private static TMP_InputField TmpInputMultiline(RectTransform parent, string name)
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
            pht.text = "Type test messages here\u2026";
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
