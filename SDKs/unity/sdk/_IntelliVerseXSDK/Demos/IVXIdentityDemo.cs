using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace IntelliVerseX.Demos
{
    public sealed class IVXIdentityDemo : MonoBehaviour
    {
        private TextMeshProUGUI _userInfoText;
        private TextMeshProUGUI _outputText;
        private TextMeshProUGUI _statusText;
        private string _mockUserId;
        private string _mockUserName;
        private bool _mockAuthenticated;

        private void Start() => BuildUI();

        private void BuildUI()
        {
            var canvasGo = new GameObject("IdentityCanvas");
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
            root.gameObject.AddComponent<Image>().color = new Color32(0x0E, 0x10, 0x1C, 0xFF);

            var vl = root.gameObject.AddComponent<VerticalLayoutGroup>();
            vl.padding = new RectOffset(24, 24, 24, 24);
            vl.spacing = 12f;
            vl.childControlWidth = true; vl.childControlHeight = true;
            vl.childForceExpandWidth = true; vl.childForceExpandHeight = false;

            MakeLabel(root, "Identity & Auth", 30, FontStyles.Bold, Color.white)
                .gameObject.AddComponent<LayoutElement>().SetLayout(-1, 52f);

            // User info card
            var cardGo = new GameObject("Card", typeof(RectTransform));
            cardGo.transform.SetParent(root, false);
            cardGo.AddComponent<LayoutElement>().SetLayout(-1, 100f);
            cardGo.AddComponent<Image>().color = new Color32(0x1A, 0x1C, 0x2E, 0xFF);
            var cardVl = cardGo.AddComponent<VerticalLayoutGroup>();
            cardVl.padding = new RectOffset(16, 16, 12, 12);
            cardVl.spacing = 4f;
            cardVl.childControlWidth = true; cardVl.childControlHeight = true;
            cardVl.childForceExpandWidth = true;
            var cardRt = cardGo.GetComponent<RectTransform>();
            _userInfoText = MakeLabel(cardRt, "Not authenticated", 16, FontStyles.Normal, new Color(0.7f, 0.75f, 0.8f));

            // Auth buttons
            MakeLabel(root, "Authentication", 20, FontStyles.Bold, new Color(0.4f, 0.9f, 0.6f))
                .gameObject.AddComponent<LayoutElement>().SetLayout(-1, 32f);
            var authRow = MakeHorizontal(root, 48f);
            MakeButton(authRow, "Device Auth", new Color32(0x22, 0x66, 0x44, 0xFF), OnDeviceAuth);
            MakeButton(authRow, "Guest Login", new Color32(0x44, 0x44, 0x88, 0xFF), OnGuestLogin);

            // Session management
            MakeLabel(root, "Session Management", 20, FontStyles.Bold, new Color(0.9f, 0.7f, 0.3f))
                .gameObject.AddComponent<LayoutElement>().SetLayout(-1, 32f);
            var sessRow = MakeHorizontal(root, 48f);
            MakeButton(sessRow, "Save Session", new Color32(0x33, 0x55, 0x88, 0xFF), OnSaveSession);
            MakeButton(sessRow, "Restore Session", new Color32(0x55, 0x55, 0x33, 0xFF), OnRestoreSession);
            MakeButton(sessRow, "Clear Session", new Color32(0x88, 0x33, 0x33, 0xFF), OnClearSession);

            // Output
            var scrollGo = new GameObject("Scroll", typeof(RectTransform));
            scrollGo.transform.SetParent(root, false);
            scrollGo.AddComponent<LayoutElement>().SetLayout(-1, 150f, flexH: 1f);
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
            _outputText = contentGo.AddComponent<TextMeshProUGUI>();
            _outputText.fontSize = 14f; _outputText.color = new Color(0.78f, 0.82f, 0.88f);
            _outputText.enableWordWrapping = true;
            _outputText.text = "Tap an auth method to begin.";

            _statusText = MakeLabel(root, "Status: Not authenticated", 14, FontStyles.Italic, new Color(0.5f, 0.55f, 0.6f));
            _statusText.gameObject.AddComponent<LayoutElement>().SetLayout(-1, 24f);
        }

        private void OnDeviceAuth()
        {
            _mockUserId = "dev-" + SystemInfo.deviceUniqueIdentifier.Substring(0, 8);
            _mockUserName = "Device_" + _mockUserId.Substring(4, 4);
            _mockAuthenticated = true;
            UpdateUserInfo();
            Append($"[OK] Device authenticated: {_mockUserId}");

            #if INTELLIVERSEX_HAS_NAKAMA
            Append("[Info] Nakama available — use IVXBootstrap for full server auth.");
            #else
            Append("[Info] Nakama not installed — running in mock mode.");
            #endif
        }

        private void OnGuestLogin()
        {
            _mockUserId = "guest-" + Guid.NewGuid().ToString().Substring(0, 8);
            _mockUserName = "Guest_" + UnityEngine.Random.Range(1000, 9999);
            _mockAuthenticated = true;
            UpdateUserInfo();
            Append($"[OK] Guest login: {_mockUserName} ({_mockUserId})");
        }

        private void OnSaveSession()
        {
            if (!_mockAuthenticated) { Append("[!] Authenticate first."); return; }
            PlayerPrefs.SetString("IVX_Demo_UserId", _mockUserId);
            PlayerPrefs.SetString("IVX_Demo_UserName", _mockUserName);
            PlayerPrefs.Save();
            Append("[OK] Session saved to PlayerPrefs.");
            _statusText.text = "Status: Session saved";
        }

        private void OnRestoreSession()
        {
            var uid = PlayerPrefs.GetString("IVX_Demo_UserId", "");
            var uname = PlayerPrefs.GetString("IVX_Demo_UserName", "");
            if (string.IsNullOrEmpty(uid)) { Append("[!] No saved session found."); return; }
            _mockUserId = uid; _mockUserName = uname; _mockAuthenticated = true;
            UpdateUserInfo();
            Append($"[OK] Session restored: {_mockUserName} ({_mockUserId})");
        }

        private void OnClearSession()
        {
            PlayerPrefs.DeleteKey("IVX_Demo_UserId");
            PlayerPrefs.DeleteKey("IVX_Demo_UserName");
            _mockAuthenticated = false; _mockUserId = null; _mockUserName = null;
            UpdateUserInfo();
            Append("[OK] Session cleared.");
            _statusText.text = "Status: Not authenticated";
        }

        private void UpdateUserInfo()
        {
            if (_mockAuthenticated)
            {
                _userInfoText.text = $"<b>User ID:</b> {_mockUserId}\n<b>Username:</b> {_mockUserName}\n<b>Session:</b> Active";
                _statusText.text = $"Status: Authenticated as {_mockUserName}";
            }
            else
            {
                _userInfoText.text = "Not authenticated";
                _statusText.text = "Status: Not authenticated";
            }
        }

        private void Append(string msg) => _outputText.text += $"\n{msg}";

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
    }
}
