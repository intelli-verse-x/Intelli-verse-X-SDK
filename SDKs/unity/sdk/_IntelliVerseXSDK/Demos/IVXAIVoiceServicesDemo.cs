using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using IntelliVerseX.AI;

namespace IntelliVerseX.Demos
{
    /// <summary>
    /// Demo for standalone STT, TTS, voice listing, and language detection
    /// via <see cref="IVXAIVoiceServices"/>.
    /// </summary>
    public sealed class IVXAIVoiceServicesDemo : MonoBehaviour
    {
        private TMP_InputField _ttsInput;
        private TextMeshProUGUI _outputText;
        private TextMeshProUGUI _statusText;
        private bool _isRecording;

        private void Start() => BuildUI();

        private void BuildUI()
        {
            var canvasGo = new GameObject("VoiceSvcCanvas");
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
            root.gameObject.AddComponent<Image>().color = new Color32(0x0F, 0x12, 0x1A, 0xFF);

            var vl = root.gameObject.AddComponent<VerticalLayoutGroup>();
            vl.padding = new RectOffset(24, 24, 24, 24);
            vl.spacing = 12f;
            vl.childControlWidth = true; vl.childControlHeight = true;
            vl.childForceExpandWidth = true; vl.childForceExpandHeight = false;

            MakeLabel(root, "AI Voice Services", 28, FontStyles.Bold, Color.white)
                .gameObject.AddComponent<LayoutElement>().SetLayout(-1, 48f);

            MakeLabel(root, "Text-to-Speech", 20, FontStyles.Bold, new Color(0.4f, 0.8f, 1f))
                .gameObject.AddComponent<LayoutElement>().SetLayout(-1, 30f);
            _ttsInput = MakeInputField(MakeHorizontal(root, 48f), "Enter text to synthesize...");
            var ttsRow = MakeHorizontal(root, 44f);
            MakeButton(ttsRow, "Speak", new Color32(0x22, 0x66, 0xAA, 0xFF), OnSpeak);
            MakeButton(ttsRow, "List Voices", new Color32(0x44, 0x44, 0x77, 0xFF), OnListVoices);

            MakeLabel(root, "Speech-to-Text", 20, FontStyles.Bold, new Color(1f, 0.7f, 0.4f))
                .gameObject.AddComponent<LayoutElement>().SetLayout(-1, 30f);
            var sttRow = MakeHorizontal(root, 48f);
            MakeButton(sttRow, "Start Streaming", new Color32(0xAA, 0x33, 0x33, 0xFF), OnStartRecording);
            MakeButton(sttRow, "Stop & Transcribe", new Color32(0x33, 0xAA, 0x33, 0xFF), OnStopTranscribe);

            MakeLabel(root, "Language Detection", 20, FontStyles.Bold, new Color(0.8f, 0.6f, 1f))
                .gameObject.AddComponent<LayoutElement>().SetLayout(-1, 30f);
            MakeButton(MakeHorizontal(root, 44f), "Detect Language (from mic)", new Color32(0x66, 0x33, 0x99, 0xFF), OnDetectLanguage);

            var scrollGo = new GameObject("Scroll", typeof(RectTransform));
            scrollGo.transform.SetParent(root, false);
            scrollGo.AddComponent<LayoutElement>().SetLayout(-1, 150f, flexH: 1f);
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
            cRt.pivot = new Vector2(0.5f, 1f); cRt.sizeDelta = Vector2.zero;
            contentGo.AddComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            scroll.content = cRt;
            _outputText = contentGo.AddComponent<TextMeshProUGUI>();
            _outputText.fontSize = 14f; _outputText.color = new Color(0.78f, 0.82f, 0.88f);
            _outputText.textWrappingMode = TextWrappingModes.Normal;
            _outputText.text = "Ready. Use the controls above.";

            _statusText = MakeLabel(root, "Status: Idle", 14, FontStyles.Italic, new Color(0.5f, 0.55f, 0.6f));
            _statusText.gameObject.AddComponent<LayoutElement>().SetLayout(-1, 24f);
        }

        private void OnSpeak()
        {
            var text = _ttsInput?.text;
            if (string.IsNullOrWhiteSpace(text)) { Append("[!] Enter text first."); return; }
            var svc = IVXAIVoiceServices.Instance;
            if (svc == null || !svc.IsInitialized) { Append("[!] Voice Services not initialized."); return; }
            _statusText.text = "Status: Synthesizing...";
            svc.SynthesizeSpeech(text, null, audioBytes =>
            {
                Append($"[TTS] Synthesized {audioBytes?.Length ?? 0} bytes of audio.");
                _statusText.text = "Status: Speech synthesized";
            });
        }

        private void OnListVoices()
        {
            var svc = IVXAIVoiceServices.Instance;
            if (svc == null || !svc.IsInitialized) { Append("[!] Voice Services not initialized."); return; }
            svc.ListVoices(voices =>
            {
                Append($"[Voices] {voices?.Count ?? 0} voices available:");
                if (voices != null) foreach (var v in voices) Append($"  - {v}");
            });
        }

        private void OnStartRecording()
        {
            var svc = IVXAIVoiceServices.Instance;
            if (svc == null || !svc.IsInitialized) { Append("[!] Voice Services not initialized."); return; }
            _isRecording = true;
            _statusText.text = "Status: Streaming...";
            Append("[REC] Streaming transcription started — speak now.");
            svc.StartStreamingTranscription();
        }

        private void OnStopTranscribe()
        {
            if (!_isRecording) { Append("[!] Not recording."); return; }
            _isRecording = false;
            var svc = IVXAIVoiceServices.Instance;
            if (svc == null || !svc.IsInitialized) { Append("[!] Voice Services not initialized."); return; }
            svc.StopStreamingTranscription();
            _statusText.text = "Status: Transcribing...";
            svc.TranscribeAudio(new byte[0], 16000, result =>
            {
                Append($"[STT] Transcription: {result}");
                _statusText.text = "Status: Transcription complete";
            });
        }

        private void OnDetectLanguage()
        {
            var svc = IVXAIVoiceServices.Instance;
            if (svc == null || !svc.IsInitialized) { Append("[!] Voice Services not initialized."); return; }
            _statusText.text = "Status: Detecting language...";
            svc.DetectLanguage(new byte[0], 16000, (lang, confidence) =>
            {
                Append($"[Language] Detected: {lang} (confidence: {confidence:P0})");
                _statusText.text = $"Status: Language detected — {lang}";
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
            tmp.textWrappingMode = TextWrappingModes.Normal;
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
