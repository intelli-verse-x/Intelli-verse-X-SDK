using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace IntelliVerseX.Demos
{
    /// <summary>
    /// Demo UI for the Spin Wheel system.
    /// Renders a segmented wheel with spin animation, reward reveal, and cooldown timer.
    /// Attach to a Canvas.
    /// </summary>
    public class IVXSpinWheelDemo : MonoBehaviour
    {
        #region Constants

        private static readonly Color COL_BG = new Color(0.05f, 0.06f, 0.09f);
        private static readonly Color COL_PANEL = new Color(0.10f, 0.12f, 0.17f);
        private static readonly Color COL_PRIMARY = new Color(0.25f, 0.50f, 0.90f);
        private static readonly Color COL_GOLD = new Color(0.95f, 0.78f, 0.20f);
        private static readonly Color COL_TEXT = new Color(0.92f, 0.93f, 0.95f);
        private static readonly Color COL_DIM = new Color(0.55f, 0.58f, 0.64f);

        private static readonly string[] SEGMENT_LABELS = { "50 Coins", "100 Gems", "2x XP", "Shield", "200 Coins", "Rare Item", "25 Gems", "Free Spin" };
        private static readonly Color[] SEGMENT_COLORS =
        {
            new Color(0.85f, 0.30f, 0.30f), new Color(0.30f, 0.70f, 0.40f),
            new Color(0.25f, 0.50f, 0.90f), new Color(0.90f, 0.60f, 0.20f),
            new Color(0.70f, 0.30f, 0.80f), new Color(0.95f, 0.78f, 0.20f),
            new Color(0.30f, 0.80f, 0.80f), new Color(0.60f, 0.60f, 0.60f)
        };

        #endregion

        #region Private Fields

        private RectTransform _wheelContainer;
        private RectTransform _wheel;
        private Button _spinBtn;
        private TextMeshProUGUI _spinBtnLabel;
        private TextMeshProUGUI _resultText;
        private TextMeshProUGUI _cooldownText;
        private TextMeshProUGUI _spinsLeftText;
        private bool _isSpinning;
        private int _spinsLeft = 3;

        #endregion

        #region Unity Lifecycle

        private void Start()
        {
            BuildUI();
        }

        #endregion

        #region UI Construction

        private void BuildUI()
        {
            var bg = MakePanel(transform, "BG", COL_BG);
            Stretch(bg);

            var root = MakeRect(transform, "Root");
            Stretch(root);
            var vlg = AddVLayout(root.gameObject, 16f, new RectOffset(20, 20, 40, 20));
            vlg.childAlignment = TextAnchor.UpperCenter;
            vlg.childForceExpandHeight = false;

            // Title
            var title = MakeTMP(root, "Title", "Daily Spin Wheel", 30f, FontStyles.Bold, COL_GOLD, TextAlignmentOptions.Center);
            title.gameObject.AddComponent<LayoutElement>().minHeight = 44f;

            _spinsLeftText = MakeTMP(root, "Spins", $"Spins remaining: {_spinsLeft}", 16f, FontStyles.Normal, COL_DIM, TextAlignmentOptions.Center);
            _spinsLeftText.gameObject.AddComponent<LayoutElement>().minHeight = 24f;

            // Wheel area
            _wheelContainer = MakeRect(root, "WheelArea");
            _wheelContainer.gameObject.AddComponent<LayoutElement>().SetLayout(-1, 340f, flexH: 0f);

            // Wheel circle
            _wheel = MakeRect(_wheelContainer, "Wheel");
            _wheel.anchorMin = new Vector2(0.5f, 0.5f);
            _wheel.anchorMax = new Vector2(0.5f, 0.5f);
            _wheel.sizeDelta = new Vector2(300f, 300f);
            _wheel.anchoredPosition = Vector2.zero;

            var wheelBg = _wheel.gameObject.AddComponent<Image>();
            wheelBg.color = COL_PANEL;

            BuildSegments();

            // Pointer (triangle at top)
            var pointer = MakeRect(_wheelContainer, "Pointer");
            pointer.anchorMin = new Vector2(0.5f, 0.5f);
            pointer.anchorMax = new Vector2(0.5f, 0.5f);
            pointer.sizeDelta = new Vector2(30f, 40f);
            pointer.anchoredPosition = new Vector2(0, 165f);
            var pImg = pointer.gameObject.AddComponent<Image>();
            pImg.color = COL_GOLD;

            // Center dot
            var center = MakeRect(_wheel, "Center");
            center.anchorMin = new Vector2(0.5f, 0.5f);
            center.anchorMax = new Vector2(0.5f, 0.5f);
            center.sizeDelta = new Vector2(50f, 50f);
            center.anchoredPosition = Vector2.zero;
            center.gameObject.AddComponent<Image>().color = new Color(0.15f, 0.17f, 0.23f);
            MakeTMP(center, "Star", "\u2605", 28f, FontStyles.Normal, COL_GOLD, TextAlignmentOptions.Center).rectTransform.anchoredPosition = Vector2.zero;
            Stretch(center.GetComponentInChildren<TextMeshProUGUI>().rectTransform);

            // Result text
            _resultText = MakeTMP(root, "Result", "", 22f, FontStyles.Bold, COL_GOLD, TextAlignmentOptions.Center);
            _resultText.gameObject.AddComponent<LayoutElement>().minHeight = 36f;

            // Spin button
            var btnGo = MakeRect(root, "SpinBtn");
            btnGo.gameObject.AddComponent<LayoutElement>().SetLayout(-1, 56f, flexW: 1f);
            var btnImg = btnGo.gameObject.AddComponent<Image>(); btnImg.color = COL_PRIMARY;
            _spinBtn = btnGo.gameObject.AddComponent<Button>(); _spinBtn.targetGraphic = btnImg;
            _spinBtnLabel = MakeTMP(btnGo, "Lbl", "SPIN!", 24f, FontStyles.Bold, COL_TEXT, TextAlignmentOptions.Center);
            Stretch(_spinBtnLabel.rectTransform);
            _spinBtn.onClick.AddListener(OnSpin);

            // Cooldown
            _cooldownText = MakeTMP(root, "Cooldown", "", 14f, FontStyles.Italic, COL_DIM, TextAlignmentOptions.Center);
            _cooldownText.gameObject.AddComponent<LayoutElement>().minHeight = 22f;
        }

        private void BuildSegments()
        {
            int count = SEGMENT_LABELS.Length;
            float anglePerSegment = 360f / count;

            for (int i = 0; i < count; i++)
            {
                var seg = MakeRect(_wheel, $"Seg{i}");
                seg.anchorMin = new Vector2(0.5f, 0.5f);
                seg.anchorMax = new Vector2(0.5f, 0.5f);
                seg.sizeDelta = new Vector2(120f, 24f);
                seg.anchoredPosition = Vector2.zero;
                seg.localRotation = Quaternion.Euler(0, 0, -anglePerSegment * i);

                var label = MakeTMP(seg, "Label", SEGMENT_LABELS[i], 11f, FontStyles.Bold, COL_TEXT, TextAlignmentOptions.MidlineRight);
                label.rectTransform.anchorMin = Vector2.zero;
                label.rectTransform.anchorMax = Vector2.one;
                label.rectTransform.offsetMin = Vector2.zero;
                label.rectTransform.offsetMax = new Vector2(-30f, 0f);

                var dot = MakeRect(seg, "Dot");
                dot.anchorMin = new Vector2(1f, 0.5f);
                dot.anchorMax = new Vector2(1f, 0.5f);
                dot.sizeDelta = new Vector2(16f, 16f);
                dot.anchoredPosition = new Vector2(-10f, 0f);
                dot.gameObject.AddComponent<Image>().color = SEGMENT_COLORS[i];
            }
        }

        #endregion

        #region Spin Logic

        private void OnSpin()
        {
            if (_isSpinning || _spinsLeft <= 0) return;
            _isSpinning = true;
            _spinsLeft--;
            _spinsLeftText.text = $"Spins remaining: {_spinsLeft}";
            _resultText.text = "";
            _spinBtn.interactable = false;
            _spinBtnLabel.text = "Spinning...";

            StartCoroutine(SpinAnimation());
        }

        private IEnumerator SpinAnimation()
        {
            int winIdx = Random.Range(0, SEGMENT_LABELS.Length);
            float anglePerSeg = 360f / SEGMENT_LABELS.Length;
            float targetAngle = 360f * 5 + (anglePerSeg * winIdx) + Random.Range(5f, anglePerSeg - 5f);

            float duration = 3.5f;
            float elapsed = 0f;
            float startAngle = _wheel.localEulerAngles.z;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                float ease = 1f - Mathf.Pow(1f - t, 3f); // ease-out cubic
                float angle = Mathf.Lerp(startAngle, startAngle + targetAngle, ease);
                _wheel.localRotation = Quaternion.Euler(0, 0, angle);
                yield return null;
            }

            _wheel.localRotation = Quaternion.Euler(0, 0, startAngle + targetAngle);
            _resultText.text = $"\ud83c\udf89  You won: {SEGMENT_LABELS[winIdx]}!";
            _isSpinning = false;

            if (_spinsLeft > 0)
            {
                _spinBtn.interactable = true;
                _spinBtnLabel.text = "SPIN!";
            }
            else
            {
                _spinBtnLabel.text = "Come back tomorrow!";
                _cooldownText.text = "Next free spin resets at midnight UTC";
            }
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

        #endregion
    }

    internal static class SpinLayoutExt
    {
        public static void SetLayout(this LayoutElement le, float minW, float minH, float flexW = -1f, float flexH = -1f)
        {
            if (minW >= 0) { le.minWidth = minW; le.preferredWidth = minW; }
            if (minH >= 0) { le.minHeight = minH; le.preferredHeight = minH; }
            if (flexW >= 0) le.flexibleWidth = flexW;
            if (flexH >= 0) le.flexibleHeight = flexH;
        }
    }
}
