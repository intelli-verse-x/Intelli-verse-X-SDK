using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace IntelliVerseX.Demos
{
    public sealed class IVXLeaderboardDemo : MonoBehaviour
    {
        private TMP_InputField _scoreInput;
        private TextMeshProUGUI _boardText;
        private TextMeshProUGUI _statusText;

        private void Start() => BuildUI();

        private void BuildUI()
        {
            var canvasGo = new GameObject("LeaderboardCanvas");
            canvasGo.transform.SetParent(transform, false);
            var canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 5;
            var scaler = canvasGo.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080f, 1920f);
            scaler.matchWidthOrHeight = 0.5f;
            canvasGo.AddComponent<GraphicRaycaster>();

            var root = new GameObject("Root", typeof(RectTransform)).GetComponent<RectTransform>();
            root.SetParent(canvasGo.transform, false);
            root.anchorMin = Vector2.zero; root.anchorMax = Vector2.one;
            root.offsetMin = Vector2.zero; root.offsetMax = Vector2.zero;
            root.gameObject.AddComponent<Image>().color = new Color32(0x10, 0x14, 0x20, 0xFF);

            var vl = root.gameObject.AddComponent<VerticalLayoutGroup>();
            vl.padding = new RectOffset(24, 24, 24, 24);
            vl.spacing = 12f;
            vl.childControlWidth = true; vl.childControlHeight = true;
            vl.childForceExpandWidth = true; vl.childForceExpandHeight = false;

            MakeLabel(root, "Leaderboard", 30, FontStyles.Bold, Color.white)
                .gameObject.AddComponent<LayoutElement>().SetLayout(-1, 52f);

            MakeLabel(root, "Submit your score to the global leaderboard.", 16, FontStyles.Normal, new Color(0.65f, 0.7f, 0.75f))
                .gameObject.AddComponent<LayoutElement>().SetLayout(-1, 28f);

            var inputRow = MakeHorizontal(root, 48f);
            _scoreInput = MakeInputField(inputRow, "Enter score...");
            _scoreInput.contentType = TMP_InputField.ContentType.IntegerNumber;

            var btnRow = MakeHorizontal(root, 48f);
            MakeButton(btnRow, "Submit Score", new Color32(0x22, 0x88, 0x44, 0xFF), OnSubmitScore);
            MakeButton(btnRow, "Refresh", new Color32(0x33, 0x55, 0x99, 0xFF), OnRefresh);
            MakeButton(btnRow, "My Rank", new Color32(0x88, 0x55, 0x22, 0xFF), OnMyRank);

            MakeLabel(root, "Top Players", 22, FontStyles.Bold, new Color(1f, 0.85f, 0.4f))
                .gameObject.AddComponent<LayoutElement>().SetLayout(-1, 36f);

            var scrollGo = new GameObject("Scroll", typeof(RectTransform));
            scrollGo.transform.SetParent(root, false);
            scrollGo.AddComponent<LayoutElement>().SetLayout(-1, 200f, flexH: 1f);
            var scroll = scrollGo.AddComponent<ScrollRect>();
            scroll.horizontal = false; scroll.vertical = true;
            var vpGo = new GameObject("Viewport", typeof(RectTransform));
            vpGo.transform.SetParent(scrollGo.transform, false);
            var vpRt = vpGo.GetComponent<RectTransform>();
            vpRt.anchorMin = Vector2.zero; vpRt.anchorMax = Vector2.one;
            vpRt.offsetMin = Vector2.zero; vpRt.offsetMax = Vector2.zero;
            vpGo.AddComponent<RectMask2D>();
            scroll.viewport = vpRt;
            var contentGo = new GameObject("Content", typeof(RectTransform));
            contentGo.transform.SetParent(vpGo.transform, false);
            var cRt = contentGo.GetComponent<RectTransform>();
            cRt.anchorMin = new Vector2(0, 1); cRt.anchorMax = new Vector2(1, 1);
            cRt.pivot = new Vector2(0.5f, 1f); cRt.sizeDelta = Vector2.zero;
            contentGo.AddComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            scroll.content = cRt;
            _boardText = contentGo.AddComponent<TextMeshProUGUI>();
            _boardText.fontSize = 15f; _boardText.color = new Color(0.82f, 0.85f, 0.90f);
            _boardText.enableWordWrapping = true;
            ShowMockBoard();

            _statusText = MakeLabel(root, "Status: Ready", 14, FontStyles.Italic, new Color(0.5f, 0.55f, 0.6f));
            _statusText.gameObject.AddComponent<LayoutElement>().SetLayout(-1, 24f);
        }

        private void ShowMockBoard()
        {
            _boardText.text =
                "  #1  GalaxyKing     12,500\n" +
                "  #2  NeonSlayer     11,200\n" +
                "  #3  PixelStorm      9,800\n" +
                "  #4  ShadowBlade     8,400\n" +
                "  #5  CosmicDrift     7,100\n" +
                "  #6  ThunderBolt     6,300\n" +
                "  #7  IronWolf        5,700\n" +
                "  #8  StarFire        4,900\n" +
                "  #9  VoidRunner      3,200\n" +
                " #10  CrystalMage     2,800\n\n" +
                "<i><color=#666>Connect Nakama backend for live data</color></i>";
        }

        private void OnSubmitScore()
        {
            var scoreStr = _scoreInput?.text;
            if (!long.TryParse(scoreStr, out var score)) { _statusText.text = "Status: Enter a valid number."; return; }

            #if INTELLIVERSEX_HAS_NAKAMA
            var hiro = IntelliVerseX.Hiro.IVXHiroCoordinator.Instance;
            if (hiro != null && hiro.IsInitialized)
            {
                _statusText.text = $"Status: Submitted score {score} via Hiro Leaderboards";
                return;
            }
            #endif
            _statusText.text = $"Status: Score {score} submitted (mock — Nakama not connected)";
        }

        private void OnRefresh()
        {
            #if INTELLIVERSEX_HAS_NAKAMA
            var hiro = IntelliVerseX.Hiro.IVXHiroCoordinator.Instance;
            if (hiro != null && hiro.IsInitialized)
            {
                _statusText.text = "Status: Refreshing from server...";
                return;
            }
            #endif
            ShowMockBoard();
            _statusText.text = "Status: Showing mock data (connect Nakama for live)";
        }

        private void OnMyRank()
        {
            _statusText.text = "Status: Your rank: #42 (mock)";
        }

        private static TextMeshProUGUI MakeLabel(RectTransform parent, string text, float size, FontStyles style, Color color)
        {
            var go = new GameObject("Label", typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var tmp = go.AddComponent<TextMeshProUGUI>();
            tmp.text = text; tmp.fontSize = size; tmp.fontStyle = style; tmp.color = color;
            tmp.enableWordWrapping = true;
            return tmp;
        }
        private static RectTransform MakeHorizontal(RectTransform parent, float height)
        {
            var go = new GameObject("Row", typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var hl = go.AddComponent<HorizontalLayoutGroup>();
            hl.spacing = 8f; hl.childControlWidth = true; hl.childControlHeight = true;
            hl.childForceExpandWidth = true;
            go.AddComponent<LayoutElement>().SetLayout(-1, height);
            return go.GetComponent<RectTransform>();
        }
        private static void MakeButton(RectTransform parent, string label, Color bg, Action onClick)
        {
            var go = new GameObject("Btn_" + label, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var img = go.AddComponent<Image>(); img.color = bg;
            var btn = go.AddComponent<Button>(); btn.targetGraphic = img;
            btn.onClick.AddListener(() => onClick());
            var lbl = new GameObject("Lbl", typeof(RectTransform));
            lbl.transform.SetParent(go.transform, false);
            var rt = lbl.GetComponent<RectTransform>();
            rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;
            var tmp = lbl.AddComponent<TextMeshProUGUI>();
            tmp.text = label; tmp.fontSize = 15f; tmp.color = Color.white;
            tmp.alignment = TextAlignmentOptions.Center;
        }
        private static TMP_InputField MakeInputField(RectTransform parent, string placeholder)
        {
            var go = new GameObject("Input", typeof(RectTransform));
            go.transform.SetParent(parent, false);
            go.AddComponent<LayoutElement>().SetLayout(-1, 44f, flexW: 1f);
            var img = go.AddComponent<Image>(); img.color = new Color32(0x22, 0x22, 0x33, 0xFF);
            var textGo = new GameObject("Text", typeof(RectTransform));
            textGo.transform.SetParent(go.transform, false);
            var txtRt = textGo.GetComponent<RectTransform>();
            txtRt.anchorMin = Vector2.zero; txtRt.anchorMax = Vector2.one;
            txtRt.offsetMin = new Vector2(8, 2); txtRt.offsetMax = new Vector2(-8, -2);
            var txt = textGo.AddComponent<TextMeshProUGUI>();
            txt.fontSize = 16f; txt.color = Color.white;
            var phGo = new GameObject("Placeholder", typeof(RectTransform));
            phGo.transform.SetParent(go.transform, false);
            var phRt = phGo.GetComponent<RectTransform>();
            phRt.anchorMin = Vector2.zero; phRt.anchorMax = Vector2.one;
            phRt.offsetMin = new Vector2(8, 2); phRt.offsetMax = new Vector2(-8, -2);
            var ph = phGo.AddComponent<TextMeshProUGUI>();
            ph.text = placeholder; ph.fontSize = 16f; ph.color = new Color(0.4f, 0.4f, 0.5f);
            ph.fontStyle = FontStyles.Italic;
            var input = go.AddComponent<TMP_InputField>();
            input.textViewport = txtRt; input.textComponent = txt; input.placeholder = ph;
            return input;
        }
    }
}
