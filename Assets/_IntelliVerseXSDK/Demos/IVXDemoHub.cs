using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace IntelliVerseX.Demos
{
    /// <summary>
    /// Full-screen demo hub that lists SDK samples and launches each as a runtime GameObject with a back overlay.
    /// </summary>
    public sealed class IVXDemoHub : MonoBehaviour
    {
        #region Constants

        private const string VERSION_LABEL = "v5.6.0";

        private static readonly Color HubBackground = new Color32(0x1A, 0x1A, 0x2E, 0xFF);

        private static readonly DemoCardSpec[] DemoCards =
        {
            new DemoCardSpec(
                "Discord Social",
                "Account linking, presence, friends, DMs, lobbies, voice, invites, moderation",
                new Color32(0x58, 0x65, 0xF2, 0xFF),
                typeof(IVXDiscordSocialDemo)),
            new DemoCardSpec(
                "AI Voice Chat",
                "AI persona conversations with voice and text",
                new Color32(0x3B, 0x82, 0xF6, 0xFF),
                typeof(IVXAIVoiceChatDemo)),
            new DemoCardSpec(
                "AI Host",
                "AI game host and live commentary",
                new Color32(0x8B, 0x5C, 0xF6, 0xFF),
                typeof(IVXAIHostDemo)),
            new DemoCardSpec(
                "AI NPC Dialog",
                "Dynamic NPC conversations with branching and actions",
                new Color32(0xEC, 0x48, 0x99, 0xFF),
                typeof(IVXAINPCDemo)),
            new DemoCardSpec(
                "AI Assistant",
                "In-game help, hints, tutorials powered by LLM",
                new Color32(0x14, 0xB8, 0xA6, 0xFF),
                typeof(IVXAIAssistantDemo)),
            new DemoCardSpec(
                "AI Moderation",
                "Content classification, filtering, custom rules",
                new Color32(0xF5, 0x9E, 0x0B, 0xFF),
                typeof(IVXAIModerationDemo)),
            new DemoCardSpec(
                "AI Content Gen",
                "Quest, story, item, dialogue generation",
                new Color32(0x10, 0xB9, 0x81, 0xFF),
                typeof(IVXAIContentGenDemo)),
            new DemoCardSpec(
                "Spin Wheel",
                "Daily spin wheel with rewards",
                new Color32(0xEF, 0x44, 0x44, 0xFF),
                typeof(IVXSpinWheelDemo)),
            new DemoCardSpec(
                "Daily Streak",
                "Login streak and daily rewards",
                new Color32(0xF9, 0x73, 0x16, 0xFF),
                typeof(IVXStreakDemo)),
            new DemoCardSpec(
                "Offerwall",
                "Offerwall and ad monetization",
                new Color32(0x84, 0xCC, 0x16, 0xFF),
                typeof(IVXOfferwallDemo)),
            new DemoCardSpec(
                "Game Modes",
                "Solo, local MP, online MP mode selection",
                new Color32(0x06, 0xB6, 0xD4, 0xFF),
                typeof(IVXGameModeSelectorDemo)),
            new DemoCardSpec(
                "Lobby",
                "Online lobby and matchmaking",
                new Color32(0xA7, 0x8B, 0xFA, 0xFF),
                typeof(IVXLobbyDemo)),
        };

        #endregion

        #region Private Fields

        private GameObject _hubCanvasRoot;
        private GameObject _currentDemoRoot;

        #endregion

        #region Unity Lifecycle

        private void Start()
        {
            EnsureEventSystemExists();
            BuildHubUi();
        }

        private void OnDestroy()
        {
            if (_currentDemoRoot != null)
            {
                Destroy(_currentDemoRoot);
                _currentDemoRoot = null;
            }
        }

        #endregion

        #region Private Methods

        private static void EnsureEventSystemExists()
        {
            if (UnityEngine.Object.FindFirstObjectByType<EventSystem>() != null)
            {
                return;
            }

            var es = new GameObject("EventSystem");
            es.AddComponent<EventSystem>();
            es.AddComponent<StandaloneInputModule>();
        }

        private void BuildHubUi()
        {
            var canvasGo = new GameObject("IVX_DemoHub_Canvas");
            canvasGo.transform.SetParent(transform, false);

            var canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 0;
            var hubScaler = canvasGo.AddComponent<CanvasScaler>();
            hubScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            hubScaler.referenceResolution = new Vector2(1920f, 1080f);
            hubScaler.matchWidthOrHeight = 0.5f;
            canvasGo.AddComponent<GraphicRaycaster>();

            _hubCanvasRoot = canvasGo;

            var bg = CreateUiObject("Background", canvasGo.transform);
            StretchFull(bg);
            var bgImage = bg.gameObject.AddComponent<Image>();
            bgImage.color = HubBackground;
            bg.transform.SetAsFirstSibling();

            var root = CreateUiObject("HubRoot", canvasGo.transform);
            StretchFull(root);

            var v = root.gameObject.AddComponent<VerticalLayoutGroup>();
            v.padding = new RectOffset(32, 32, 28, 28);
            v.spacing = 16f;
            v.childAlignment = TextAnchor.UpperCenter;
            v.childControlHeight = true;
            v.childControlWidth = true;
            v.childForceExpandHeight = false;
            v.childForceExpandWidth = true;

            var header = CreateUiObject("Header", root);
            var headerHl = header.gameObject.AddComponent<HorizontalLayoutGroup>();
            headerHl.childAlignment = TextAnchor.MiddleLeft;
            headerHl.childForceExpandWidth = true;
            headerHl.spacing = 12f;

            var titleRt = CreateUiObject("Title", header);
            var titleLe = titleRt.gameObject.AddComponent<LayoutElement>();
            titleLe.flexibleWidth = 1f;
            titleLe.minHeight = 56f;
            var titleTmp = titleRt.gameObject.AddComponent<TextMeshProUGUI>();
            titleTmp.text = "IntelliVerseX SDK — Demo Hub";
            titleTmp.fontSize = 36f;
            titleTmp.fontStyle = FontStyles.Bold;
            titleTmp.color = Color.white;
            titleTmp.alignment = TextAlignmentOptions.Left;

            var verRt = CreateUiObject("Version", header);
            var verLe = verRt.gameObject.AddComponent<LayoutElement>();
            verLe.preferredWidth = 120f;
            var verTmp = verRt.gameObject.AddComponent<TextMeshProUGUI>();
            verTmp.text = VERSION_LABEL;
            verTmp.fontSize = 22f;
            verTmp.color = new Color(0.65f, 0.68f, 0.75f);
            verTmp.alignment = TextAlignmentOptions.MidlineRight;

            var scrollGo = CreateUiObject("Scroll", root);
            var scrollLe = scrollGo.gameObject.AddComponent<LayoutElement>();
            scrollLe.flexibleHeight = 1f;
            scrollLe.minHeight = 200f;

            var scroll = scrollGo.gameObject.AddComponent<ScrollRect>();
            scroll.horizontal = false;
            scroll.vertical = true;
            scroll.movementType = ScrollRect.MovementType.Clamped;
            scroll.scrollSensitivity = 40f;

            var viewport = CreateUiObject("Viewport", scrollGo.transform);
            StretchFull(viewport);
            viewport.gameObject.AddComponent<RectMask2D>();
            var vpImage = viewport.gameObject.AddComponent<Image>();
            vpImage.color = new Color(0f, 0f, 0f, 0.02f);
            scroll.viewport = viewport;

            var content = CreateUiObject("Content", viewport);
            var contentRt = content;
            content.anchorMin = new Vector2(0f, 1f);
            content.anchorMax = new Vector2(1f, 1f);
            content.pivot = new Vector2(0.5f, 1f);
            content.anchoredPosition = Vector2.zero;
            content.sizeDelta = new Vector2(0f, 0f);

            var grid = content.gameObject.AddComponent<GridLayoutGroup>();
            grid.cellSize = new Vector2(440f, 200f);
            grid.spacing = new Vector2(16f, 16f);
            grid.padding = new RectOffset(8, 8, 8, 8);
            grid.startCorner = GridLayoutGroup.Corner.UpperLeft;
            grid.startAxis = GridLayoutGroup.Axis.Horizontal;
            grid.childAlignment = TextAnchor.UpperCenter;
            grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            grid.constraintCount = 2;

            content.gameObject.AddComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            scroll.content = content;

            for (var i = 0; i < DemoCards.Length; i++)
            {
                BuildCard(content, DemoCards[i]);
            }
        }

        private void BuildCard(RectTransform parent, DemoCardSpec spec)
        {
            var card = CreateUiObject("Card_" + spec.Title, parent);
            var cardBg = card.gameObject.AddComponent<Image>();
            cardBg.color = new Color(0.12f, 0.13f, 0.18f, 0.98f);

            var cardV = card.gameObject.AddComponent<VerticalLayoutGroup>();
            cardV.padding = new RectOffset(14, 14, 14, 14);
            cardV.spacing = 8f;
            cardV.childAlignment = TextAnchor.UpperLeft;
            cardV.childControlWidth = true;
            cardV.childControlHeight = true;
            cardV.childForceExpandWidth = true;

            var row = CreateUiObject("IconRow", card);
            var rowHl = row.gameObject.AddComponent<HorizontalLayoutGroup>();
            rowHl.spacing = 12f;
            rowHl.childAlignment = TextAnchor.UpperLeft;
            rowHl.childForceExpandWidth = true;

            var iconRt = CreateUiObject("Icon", row);
            var iconLe = iconRt.gameObject.AddComponent<LayoutElement>();
            iconLe.preferredWidth = 56f;
            iconLe.preferredHeight = 56f;
            iconLe.minWidth = 56f;
            iconLe.minHeight = 56f;
            var iconImg = iconRt.gameObject.AddComponent<Image>();
            iconImg.color = spec.Accent;

            var titleCol = CreateUiObject("TitleCol", row);
            var titleColLe = titleCol.gameObject.AddComponent<LayoutElement>();
            titleColLe.flexibleWidth = 1f;
            var titleTmp = titleCol.gameObject.AddComponent<TextMeshProUGUI>();
            titleTmp.text = spec.Title;
            titleTmp.fontSize = 22f;
            titleTmp.fontStyle = FontStyles.Bold;
            titleTmp.color = Color.white;
            titleTmp.alignment = TextAlignmentOptions.TopLeft;

            var descRt = CreateUiObject("Description", card);
            var descLe = descRt.gameObject.AddComponent<LayoutElement>();
            descLe.minHeight = 52f;
            descLe.flexibleHeight = 1f;
            var descTmp = descRt.gameObject.AddComponent<TextMeshProUGUI>();
            descTmp.text = spec.Description;
            descTmp.fontSize = 15f;
            descTmp.color = new Color(0.72f, 0.74f, 0.80f);
            descTmp.enableWordWrapping = true;
            descTmp.alignment = TextAlignmentOptions.TopLeft;

            var launchRt = CreateUiObject("Launch", card);
            var launchLe = launchRt.gameObject.AddComponent<LayoutElement>();
            launchLe.preferredHeight = 40f;
            var launchBtn = launchRt.gameObject.AddComponent<Button>();
            var launchImg = launchRt.gameObject.AddComponent<Image>();
            launchImg.color = new Color(spec.Accent.r * 0.85f, spec.Accent.g * 0.85f, spec.Accent.b * 0.85f, 1f);
            launchBtn.targetGraphic = launchImg;

            var launchLabel = CreateUiObject("Label", launchRt);
            StretchFull(launchLabel);
            var launchTmp = launchLabel.gameObject.AddComponent<TextMeshProUGUI>();
            launchTmp.text = "Launch";
            launchTmp.fontSize = 18f;
            launchTmp.fontStyle = FontStyles.Bold;
            launchTmp.color = Color.white;
            launchTmp.alignment = TextAlignmentOptions.Center;

            var capturedDemoType = spec.DemoComponentType;
            launchBtn.onClick.AddListener(() => LaunchDemo(capturedDemoType));
        }

        private void LaunchDemo(Type demoType)
        {
            if (!typeof(MonoBehaviour).IsAssignableFrom(demoType))
            {
                Debug.LogError($"[IVXDemoHub] Invalid demo type: {demoType.Name}");
                return;
            }

            if (_hubCanvasRoot != null)
            {
                _hubCanvasRoot.SetActive(false);
            }

            if (_currentDemoRoot != null)
            {
                Destroy(_currentDemoRoot);
                _currentDemoRoot = null;
            }

            var root = new GameObject("IVX_Demo_" + demoType.Name);
            root.transform.SetParent(transform, false);
            _currentDemoRoot = root;

            root.AddComponent(demoType);
            BuildBackOverlay(root);
        }

        private void BuildBackOverlay(GameObject demoRoot)
        {
            var overlayGo = new GameObject("BackOverlay");
            overlayGo.transform.SetParent(demoRoot.transform, false);

            var overlayCanvas = overlayGo.AddComponent<Canvas>();
            overlayCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            overlayCanvas.sortingOrder = 1000;
            overlayGo.AddComponent<GraphicRaycaster>();

            var scaler = overlayGo.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;

            var backRt = CreateUiObject("BackButton", overlayGo.transform);
            backRt.anchorMin = new Vector2(0f, 1f);
            backRt.anchorMax = new Vector2(0f, 1f);
            backRt.pivot = new Vector2(0f, 1f);
            backRt.anchoredPosition = new Vector2(20f, -20f);
            backRt.sizeDelta = new Vector2(200f, 44f);

            var backBtn = backRt.gameObject.AddComponent<Button>();
            var backImg = backRt.gameObject.AddComponent<Image>();
            backImg.color = new Color(0.18f, 0.20f, 0.28f, 0.96f);
            backBtn.targetGraphic = backImg;

            var labelRt = CreateUiObject("Label", backRt);
            StretchFull(labelRt);
            var labelTmp = labelRt.gameObject.AddComponent<TextMeshProUGUI>();
            labelTmp.text = "\u2190 Back to Hub";
            labelTmp.fontSize = 18f;
            labelTmp.color = Color.white;
            labelTmp.alignment = TextAlignmentOptions.Center;

            backBtn.onClick.AddListener(() =>
            {
                if (_currentDemoRoot != null)
                {
                    Destroy(_currentDemoRoot);
                    _currentDemoRoot = null;
                }

                if (_hubCanvasRoot != null)
                {
                    _hubCanvasRoot.SetActive(true);
                }
            });
        }

        private static RectTransform CreateUiObject(string name, Transform parent)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            return go.AddComponent<RectTransform>();
        }

        private static void StretchFull(RectTransform rt)
        {
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
        }

        #endregion

        #region Nested Types

        private readonly struct DemoCardSpec
        {
            public readonly string Title;
            public readonly string Description;
            public readonly Color Accent;
            public readonly Type DemoComponentType;

            public DemoCardSpec(string title, string description, Color accent, Type demoComponentType)
            {
                Title = title;
                Description = description;
                Accent = accent;
                DemoComponentType = demoComponentType;
            }
        }

        #endregion
    }
}
