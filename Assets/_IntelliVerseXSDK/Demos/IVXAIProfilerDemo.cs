using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using IntelliVerseX.AI;

namespace IntelliVerseX.Demos
{
    public sealed class IVXAIProfilerDemo : MonoBehaviour
    {
        private TMP_InputField _eventNameInput;
        private TextMeshProUGUI _outputText;
        private TextMeshProUGUI _statusText;
        private string _selectedEventType = "gameplay";

        private void Start() => BuildUI();

        private void BuildUI()
        {
            var canvasGo = new GameObject("ProfilerCanvas");
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
            var rootImg = root.gameObject.AddComponent<Image>();
            rootImg.color = new Color32(0x12, 0x12, 0x1A, 0xFF);

            var vl = root.gameObject.AddComponent<VerticalLayoutGroup>();
            vl.padding = new RectOffset(24, 24, 24, 24);
            vl.spacing = 12f;
            vl.childControlWidth = true; vl.childControlHeight = true;
            vl.childForceExpandWidth = true; vl.childForceExpandHeight = false;

            // Title
            MakeLabel(root, "AI Player Profiler", 28, FontStyles.Bold, Color.white).gameObject
                .AddComponent<LayoutElement>().SetLayout(-1, 48f);

            // Event tracking section
            MakeLabel(root, "Track Event", 20, FontStyles.Bold, new Color(0.6f, 0.8f, 1f)).gameObject
                .AddComponent<LayoutElement>().SetLayout(-1, 32f);

            var inputRow = MakeHorizontal(root, 48f);
            _eventNameInput = MakeInputField(inputRow, "Event name...");

            var typeRow = MakeHorizontal(root, 40f);
            foreach (var t in new[] { "gameplay", "economy", "social", "achievement" })
            {
                var captured = t;
                MakeButton(typeRow, t, new Color32(0x33, 0x33, 0x55, 0xFF), () => _selectedEventType = captured);
            }

            MakeButton(MakeHorizontal(root, 48f), "Track Event", new Color32(0x22, 0x88, 0x44, 0xFF), OnTrackEvent);

            // Profile section
            MakeLabel(root, "Player Profile & Predictions", 20, FontStyles.Bold, new Color(0.6f, 1f, 0.8f)).gameObject
                .AddComponent<LayoutElement>().SetLayout(-1, 32f);

            var btnRow = MakeHorizontal(root, 44f);
            MakeButton(btnRow, "Fetch Profile", new Color32(0x33, 0x55, 0x88, 0xFF), OnFetchProfile);
            MakeButton(btnRow, "Get Cohort", new Color32(0x55, 0x33, 0x88, 0xFF), OnGetCohort);
            MakeButton(btnRow, "Predict Churn", new Color32(0x88, 0x33, 0x33, 0xFF), OnPredictChurn);
            MakeButton(btnRow, "Personalization", new Color32(0x33, 0x88, 0x55, 0xFF), OnGetPersonalization);

            // Output area
            var scrollGo = new GameObject("Scroll", typeof(RectTransform));
            scrollGo.transform.SetParent(root, false);
            scrollGo.AddComponent<LayoutElement>().SetLayout(-1, 200f, flexH: 1f);
            var scroll = scrollGo.AddComponent<ScrollRect>();
            scroll.horizontal = false; scroll.vertical = true;
            scroll.movementType = ScrollRect.MovementType.Clamped;
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
            cRt.pivot = new Vector2(0.5f, 1f);
            cRt.sizeDelta = Vector2.zero;
            var csf = contentGo.AddComponent<ContentSizeFitter>();
            csf.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
            csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            scroll.content = cRt;

            _outputText = contentGo.AddComponent<TextMeshProUGUI>();
            _outputText.fontSize = 14f;
            _outputText.color = new Color(0.78f, 0.82f, 0.88f);
            _outputText.enableWordWrapping = true;
            _outputText.text = "Ready. Track events or fetch profile data.";

            // Status
            _statusText = MakeLabel(root, "Status: Idle", 14, FontStyles.Italic, new Color(0.5f, 0.55f, 0.6f));
            _statusText.gameObject.AddComponent<LayoutElement>().SetLayout(-1, 24f);
        }

        private void OnTrackEvent()
        {
            var name = _eventNameInput?.text;
            if (string.IsNullOrWhiteSpace(name)) { Append("[!] Enter an event name."); return; }
            var profiler = IVXAIProfiler.Instance;
            if (profiler == null || !profiler.IsInitialized)
            {
                Append("[!] AI Profiler not initialized. Use IVXBootstrap or call Initialize() first.");
                return;
            }
            profiler.TrackEvent(name, new Dictionary<string, object> { { "type", _selectedEventType } });
            Append($"[OK] Tracked: {_selectedEventType}/{name}");
            _statusText.text = "Status: Event tracked";
        }

        private void OnFetchProfile()
        {
            var profiler = IVXAIProfiler.Instance;
            if (profiler == null || !profiler.IsInitialized)
            { Append("[!] AI Profiler not initialized."); return; }
            profiler.GetPlayerProfile(profile =>
            {
                Append($"[Profile]\n{profile}");
                _statusText.text = "Status: Profile fetched";
            });
        }

        private void OnGetCohort()
        {
            var profiler = IVXAIProfiler.Instance;
            if (profiler == null || !profiler.IsInitialized)
            { Append("[!] AI Profiler not initialized."); return; }
            profiler.ClassifyPlayer(cohort =>
            {
                Append($"[Cohort] {cohort}");
                _statusText.text = "Status: Cohort received";
            });
        }

        private void OnPredictChurn()
        {
            var profiler = IVXAIProfiler.Instance;
            if (profiler == null || !profiler.IsInitialized)
            { Append("[!] AI Profiler not initialized."); return; }
            profiler.PredictChurn((score, factors) =>
            {
                Append($"[Churn Prediction] Risk: {score:P0} | Factors: {string.Join(", ", factors)}");
                _statusText.text = "Status: Churn prediction received";
            });
        }

        private void OnGetPersonalization()
        {
            var profiler = IVXAIProfiler.Instance;
            if (profiler == null || !profiler.IsInitialized)
            { Append("[!] AI Profiler not initialized."); return; }
            profiler.GetPersonalizationHints(hints =>
            {
                Append($"[Personalization] {hints?.Count ?? 0} hints received");
                if (hints != null) foreach (var h in hints) Append($"  - {h}");
                _statusText.text = "Status: Personalization data received";
            });
        }

        private void Append(string msg) => _outputText.text += $"\n{msg}";

        #region UI Helpers
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
        #endregion
    }
}
