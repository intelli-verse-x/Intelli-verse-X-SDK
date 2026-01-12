#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace IntelliVerseX.Games.Leaderboard.Editor
{
    /// <summary>
    /// Prefab builder for IVXGLeaderboard components.
    /// Creates ready-to-use leaderboard prefabs via the SDK Setup Wizard.
    /// </summary>
    public static class IVXGLeaderboardPrefabBuilder
    {
        private const string PREFABS_PATH = "Assets/_IntelliVerseXSDK/Leaderboard/Prefabs";

        // Default colors
        private static readonly Color PrimaryColor = new Color(0.25f, 0.52f, 0.96f);
        private static readonly Color SecondaryColor = new Color(0.15f, 0.15f, 0.20f);
        private static readonly Color TextColor = Color.white;
        private static readonly Color HighlightColor = new Color(1f, 0.85f, 0.1f);

        /// <summary>
        /// Create a leaderboard entry prefab.
        /// </summary>
        [MenuItem("IntelliVerseX/Leaderboard/Create Entry Prefab")]
        public static GameObject CreateLeaderboardEntryPrefab()
        {
            // Root
            var entryGO = new GameObject("IVXGLeaderboardEntry");
            var entryRect = entryGO.AddComponent<RectTransform>();
            entryRect.sizeDelta = new Vector2(800, 80);

            // Add LayoutElement for scroll views
            var layoutElement = entryGO.AddComponent<LayoutElement>();
            layoutElement.preferredHeight = 80;
            layoutElement.minHeight = 80;

            // Background
            var bgImage = entryGO.AddComponent<Image>();
            bgImage.color = new Color(0.2f, 0.2f, 0.25f, 0.9f);

            // Horizontal layout
            var hlg = entryGO.AddComponent<HorizontalLayoutGroup>();
            hlg.childAlignment = TextAnchor.MiddleLeft;
            hlg.childControlWidth = false;
            hlg.childControlHeight = false;
            hlg.childForceExpandWidth = false;
            hlg.childForceExpandHeight = false;
            hlg.spacing = 15;
            hlg.padding = new RectOffset(20, 20, 10, 10);

            // Rank
            var rankGO = CreateTextElement("Rank", "#1", entryRect, new Vector2(60, 60));
            var rankTMP = rankGO.GetComponent<TextMeshProUGUI>();
            rankTMP.fontSize = 28;
            rankTMP.fontStyle = FontStyles.Bold;
            rankTMP.color = HighlightColor;
            rankTMP.alignment = TextAlignmentOptions.Center;

            // Username
            var usernameGO = CreateTextElement("Username", "PlayerName", entryRect, new Vector2(400, 60));
            var usernameTMP = usernameGO.GetComponent<TextMeshProUGUI>();
            usernameTMP.fontSize = 24;
            usernameTMP.alignment = TextAlignmentOptions.Left;
            usernameTMP.overflowMode = TextOverflowModes.Ellipsis;

            // Score
            var scoreGO = CreateTextElement("Score", "10,000", entryRect, new Vector2(180, 60));
            var scoreTMP = scoreGO.GetComponent<TextMeshProUGUI>();
            scoreTMP.fontSize = 24;
            scoreTMP.fontStyle = FontStyles.Bold;
            scoreTMP.alignment = TextAlignmentOptions.Right;
            scoreTMP.color = PrimaryColor;

            // Add the entry view component
            var entryView = entryGO.AddComponent<UI.IVXGLeaderboardEntryView>();

            // Set serialized fields via reflection
            SetPrivateField(entryView, "rankText", rankTMP);
            SetPrivateField(entryView, "usernameText", usernameTMP);
            SetPrivateField(entryView, "scoreText", scoreTMP);
            SetPrivateField(entryView, "backgroundImage", bgImage);
            SetPrivateField(entryView, "normalColor", new Color(0.2f, 0.2f, 0.25f, 0.9f));
            SetPrivateField(entryView, "selfColor", new Color(0.3f, 0.5f, 0.2f, 0.9f));
            SetPrivateField(entryView, "alternateRowColor", new Color(0.25f, 0.25f, 0.3f, 0.9f));

            return entryGO;
        }

        /// <summary>
        /// Create a complete leaderboard UI canvas.
        /// </summary>
        [MenuItem("IntelliVerseX/Leaderboard/Create Leaderboard Canvas")]
        public static GameObject CreateLeaderboardCanvas()
        {
            // Canvas
            var canvasGO = new GameObject("IVXGLeaderboardCanvas");
            var canvas = canvasGO.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 100;

            var scaler = canvasGO.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080, 1920);
            scaler.matchWidthOrHeight = 0.5f;

            canvasGO.AddComponent<GraphicRaycaster>();

            // Main Panel
            var panelGO = new GameObject("LeaderboardPanel");
            panelGO.transform.SetParent(canvasGO.transform, false);

            var panelRect = panelGO.AddComponent<RectTransform>();
            panelRect.anchorMin = Vector2.zero;
            panelRect.anchorMax = Vector2.one;
            panelRect.sizeDelta = Vector2.zero;

            var panelImage = panelGO.AddComponent<Image>();
            panelImage.color = new Color(0.1f, 0.1f, 0.15f, 0.98f);

            var vlg = panelGO.AddComponent<VerticalLayoutGroup>();
            vlg.childAlignment = TextAnchor.UpperCenter;
            vlg.childControlWidth = true;
            vlg.childControlHeight = false;
            vlg.childForceExpandWidth = true;
            vlg.childForceExpandHeight = false;
            vlg.spacing = 15;
            vlg.padding = new RectOffset(30, 30, 30, 30);

            // Header
            var headerGO = CreateHeaderSection(panelRect);

            // Period buttons
            var buttonsGO = CreatePeriodButtons(panelRect);

            // Scroll View
            var scrollGO = CreateScrollView(panelRect);

            // Status text
            var statusGO = CreateTextElement("Status", "", panelRect, new Vector2(800, 40));
            var statusTMP = statusGO.GetComponent<TextMeshProUGUI>();
            statusTMP.fontSize = 18;
            statusTMP.alignment = TextAlignmentOptions.Center;
            statusTMP.color = new Color(0.7f, 0.7f, 0.7f);

            // Add IVXGLeaderboardUI component
            var leaderboardUI = panelGO.AddComponent<UI.IVXGLeaderboardUI>();

            // Set references via reflection
            var scrollRect = scrollGO.GetComponent<ScrollRect>();
            var content = scrollGO.transform.Find("Viewport/Content");

            SetPrivateField(leaderboardUI, "entriesParent", content);
            SetPrivateField(leaderboardUI, "scrollRect", scrollRect);
            SetPrivateField(leaderboardUI, "statusText", statusTMP);
            SetPrivateField(leaderboardUI, "headingText", headerGO.GetComponentInChildren<TextMeshProUGUI>());

            // Set button references
            var buttons = buttonsGO.GetComponentsInChildren<Button>();
            if (buttons.Length >= 4)
            {
                SetPrivateField(leaderboardUI, "dailyButton", buttons[0]);
                SetPrivateField(leaderboardUI, "weeklyButton", buttons[1]);
                SetPrivateField(leaderboardUI, "monthlyButton", buttons[2]);
                SetPrivateField(leaderboardUI, "alltimeButton", buttons[3]);
            }
            if (buttons.Length >= 5)
            {
                SetPrivateField(leaderboardUI, "refreshButton", buttons[4]);
            }

            // Close button
            var closeBtn = headerGO.GetComponentInChildren<Button>();
            if (closeBtn != null)
            {
                SetPrivateField(leaderboardUI, "closeButton", closeBtn);
            }

            return canvasGO;
        }

        /// <summary>
        /// Create and save all leaderboard prefabs.
        /// </summary>
        [MenuItem("IntelliVerseX/Leaderboard/Create All Prefabs")]
        public static void CreateAllPrefabs()
        {
            EnsureDirectoryExists(PREFABS_PATH);

            // Create entry prefab
            var entryGO = CreateLeaderboardEntryPrefab();
            var entryPath = $"{PREFABS_PATH}/IVXGLeaderboardEntry.prefab";
            PrefabUtility.SaveAsPrefabAsset(entryGO, entryPath);
            Object.DestroyImmediate(entryGO);
            Debug.Log($"[IVXGLeaderboardPrefabBuilder] Created entry prefab at: {entryPath}");

            // Create canvas prefab
            var canvasGO = CreateLeaderboardCanvas();
            var canvasPath = $"{PREFABS_PATH}/IVXGLeaderboardCanvas.prefab";
            PrefabUtility.SaveAsPrefabAsset(canvasGO, canvasPath);
            Object.DestroyImmediate(canvasGO);
            Debug.Log($"[IVXGLeaderboardPrefabBuilder] Created canvas prefab at: {canvasPath}");

            AssetDatabase.Refresh();

            EditorUtility.DisplayDialog("Prefabs Created",
                $"Leaderboard prefabs created at:\n{PREFABS_PATH}\n\n" +
                "• IVXGLeaderboardEntry.prefab\n" +
                "• IVXGLeaderboardCanvas.prefab",
                "OK");
        }

        /// <summary>
        /// Add leaderboard canvas to current scene.
        /// </summary>
        [MenuItem("IntelliVerseX/Leaderboard/Add to Scene")]
        public static void AddLeaderboardToScene()
        {
            var prefabPath = $"{PREFABS_PATH}/IVXGLeaderboardCanvas.prefab";
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);

            if (prefab == null)
            {
                // Create prefabs first
                CreateAllPrefabs();
                prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            }

            if (prefab != null)
            {
                var instance = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
                Undo.RegisterCreatedObjectUndo(instance, "Add Leaderboard Canvas");
                Selection.activeGameObject = instance;

                // Also add the runtime manager if not present
                var existingManager = Object.FindObjectOfType<IVXGLeaderboard>();
                if (existingManager == null)
                {
                    var managerGO = new GameObject("IVXGLeaderboard");
                    managerGO.AddComponent<IVXGLeaderboard>();
                    Undo.RegisterCreatedObjectUndo(managerGO, "Add IVXGLeaderboard Manager");
                }

                Debug.Log("[IVXGLeaderboardPrefabBuilder] Added leaderboard to scene");
            }
            else
            {
                EditorUtility.DisplayDialog("Error",
                    "Failed to create leaderboard prefab.",
                    "OK");
            }
        }

        #region Helper Methods

        private static GameObject CreateHeaderSection(RectTransform parent)
        {
            var headerGO = new GameObject("Header");
            headerGO.transform.SetParent(parent, false);

            var headerRect = headerGO.AddComponent<RectTransform>();
            headerRect.sizeDelta = new Vector2(1020, 80);

            var hlg = headerGO.AddComponent<HorizontalLayoutGroup>();
            hlg.childAlignment = TextAnchor.MiddleCenter;
            hlg.childControlWidth = false;
            hlg.childControlHeight = false;
            hlg.childForceExpandWidth = false;
            hlg.spacing = 20;
            hlg.padding = new RectOffset(20, 20, 0, 0);

            // Title
            var titleGO = CreateTextElement("Title", "Leaderboard", headerRect, new Vector2(800, 70));
            var titleTMP = titleGO.GetComponent<TextMeshProUGUI>();
            titleTMP.fontSize = 36;
            titleTMP.fontStyle = FontStyles.Bold;
            titleTMP.alignment = TextAlignmentOptions.Center;
            titleTMP.color = TextColor;

            // Close button
            var closeGO = new GameObject("CloseButton");
            closeGO.transform.SetParent(headerRect, false);

            var closeRect = closeGO.AddComponent<RectTransform>();
            closeRect.sizeDelta = new Vector2(60, 60);

            var closeImage = closeGO.AddComponent<Image>();
            closeImage.color = new Color(0.8f, 0.2f, 0.2f);

            var closeBtn = closeGO.AddComponent<Button>();
            closeBtn.targetGraphic = closeImage;

            var closeTxtGO = CreateTextElement("Text", "✕", closeRect, new Vector2(60, 60));
            var closeTMP = closeTxtGO.GetComponent<TextMeshProUGUI>();
            closeTMP.fontSize = 32;
            closeTMP.alignment = TextAlignmentOptions.Center;
            closeTMP.color = Color.white;

            return headerGO;
        }

        private static GameObject CreatePeriodButtons(RectTransform parent)
        {
            var buttonsGO = new GameObject("PeriodButtons");
            buttonsGO.transform.SetParent(parent, false);

            var buttonsRect = buttonsGO.AddComponent<RectTransform>();
            buttonsRect.sizeDelta = new Vector2(1020, 60);

            var hlg = buttonsGO.AddComponent<HorizontalLayoutGroup>();
            hlg.childAlignment = TextAnchor.MiddleCenter;
            hlg.childControlWidth = false;
            hlg.childControlHeight = false;
            hlg.childForceExpandWidth = false;
            hlg.spacing = 10;

            CreatePeriodButton("Daily", buttonsRect);
            CreatePeriodButton("Weekly", buttonsRect);
            CreatePeriodButton("Monthly", buttonsRect);
            CreatePeriodButton("All-Time", buttonsRect);
            CreatePeriodButton("↻", buttonsRect, 60); // Refresh button

            return buttonsGO;
        }

        private static GameObject CreatePeriodButton(string text, RectTransform parent, float width = 180)
        {
            var btnGO = new GameObject($"{text}Button");
            btnGO.transform.SetParent(parent, false);

            var btnRect = btnGO.AddComponent<RectTransform>();
            btnRect.sizeDelta = new Vector2(width, 50);

            var btnImage = btnGO.AddComponent<Image>();
            btnImage.color = SecondaryColor;

            var btn = btnGO.AddComponent<Button>();
            btn.targetGraphic = btnImage;

            var colors = btn.colors;
            colors.normalColor = SecondaryColor;
            colors.highlightedColor = PrimaryColor;
            colors.pressedColor = new Color(PrimaryColor.r * 0.8f, PrimaryColor.g * 0.8f, PrimaryColor.b * 0.8f);
            colors.selectedColor = PrimaryColor;
            btn.colors = colors;

            var txtGO = CreateTextElement("Text", text, btnRect, new Vector2(width, 50));
            var tmp = txtGO.GetComponent<TextMeshProUGUI>();
            tmp.fontSize = 18;
            tmp.fontStyle = FontStyles.Bold;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color = TextColor;

            return btnGO;
        }

        private static GameObject CreateScrollView(RectTransform parent)
        {
            var scrollGO = new GameObject("ScrollView");
            scrollGO.transform.SetParent(parent, false);

            var scrollRect = scrollGO.AddComponent<RectTransform>();
            scrollRect.sizeDelta = new Vector2(1020, 1200);

            var scrollImage = scrollGO.AddComponent<Image>();
            scrollImage.color = new Color(0.15f, 0.15f, 0.2f, 0.5f);

            var scroll = scrollGO.AddComponent<ScrollRect>();
            scroll.horizontal = false;
            scroll.vertical = true;
            scroll.movementType = ScrollRect.MovementType.Elastic;
            scroll.elasticity = 0.1f;

            // Viewport
            var viewportGO = new GameObject("Viewport");
            viewportGO.transform.SetParent(scrollGO.transform, false);

            var viewportRect = viewportGO.AddComponent<RectTransform>();
            viewportRect.anchorMin = Vector2.zero;
            viewportRect.anchorMax = Vector2.one;
            viewportRect.sizeDelta = Vector2.zero;
            viewportRect.pivot = new Vector2(0, 1);

            var viewportImage = viewportGO.AddComponent<Image>();
            viewportImage.color = Color.clear;

            var mask = viewportGO.AddComponent<Mask>();
            mask.showMaskGraphic = false;

            // Content
            var contentGO = new GameObject("Content");
            contentGO.transform.SetParent(viewportGO.transform, false);

            var contentRect = contentGO.AddComponent<RectTransform>();
            contentRect.anchorMin = new Vector2(0, 1);
            contentRect.anchorMax = new Vector2(1, 1);
            contentRect.pivot = new Vector2(0.5f, 1);
            contentRect.sizeDelta = new Vector2(0, 0);

            var contentSizeFitter = contentGO.AddComponent<ContentSizeFitter>();
            contentSizeFitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
            contentSizeFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            var vlg = contentGO.AddComponent<VerticalLayoutGroup>();
            vlg.childAlignment = TextAnchor.UpperCenter;
            vlg.childControlWidth = true;
            vlg.childControlHeight = false;
            vlg.childForceExpandWidth = true;
            vlg.childForceExpandHeight = false;
            vlg.spacing = 5;
            vlg.padding = new RectOffset(10, 10, 10, 10);

            // Wire up scroll rect
            scroll.viewport = viewportRect;
            scroll.content = contentRect;

            return scrollGO;
        }

        private static GameObject CreateTextElement(string name, string text, RectTransform parent, Vector2 size)
        {
            var txtGO = new GameObject(name);
            txtGO.transform.SetParent(parent, false);

            var txtRect = txtGO.AddComponent<RectTransform>();
            txtRect.sizeDelta = size;

            var tmp = txtGO.AddComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.color = TextColor;
            tmp.fontSize = 24;

            return txtGO;
        }

        private static void SetPrivateField(object target, string fieldName, object value)
        {
            var field = target.GetType().GetField(fieldName,
                System.Reflection.BindingFlags.NonPublic |
                System.Reflection.BindingFlags.Instance);

            if (field != null)
            {
                field.SetValue(target, value);
            }
        }

        private static void EnsureDirectoryExists(string path)
        {
            if (AssetDatabase.IsValidFolder(path)) return;

            var parts = path.Split('/');
            var currentPath = parts[0];

            for (int i = 1; i < parts.Length; i++)
            {
                var nextPath = currentPath + "/" + parts[i];
                if (!AssetDatabase.IsValidFolder(nextPath))
                {
                    AssetDatabase.CreateFolder(currentPath, parts[i]);
                }
                currentPath = nextPath;
            }
        }

        #endregion
    }
}
#endif
