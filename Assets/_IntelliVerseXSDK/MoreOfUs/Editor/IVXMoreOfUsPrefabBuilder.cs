// ============================================================================
// IVXMoreOfUsPrefabBuilder.cs - Editor Tool for Building MoreOfUs Prefabs
// ============================================================================
// IntelliVerseX SDK - Cross-Promotion Feature
// Creates production-ready UI prefabs with proper styling
// ============================================================================

#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using IntelliVerseX.MoreOfUs.UI;

namespace IntelliVerseX.MoreOfUs.Editor
{
    /// <summary>
    /// Editor tool for creating "More Of Us" UI prefabs
    /// </summary>
    public static class IVXMoreOfUsPrefabBuilder
    {
        #region Constants

        private const string LOG_PREFIX = "[IVXMoreOfUsPrefabBuilder]";
        private const string DEFAULT_PREFABS_PATH = "Assets/IntelliVerseX/Generated/MoreOfUs/Prefabs";
        private const string PACKAGE_PREFABS_PATH = "Assets/_IntelliVerseXSDK/MoreOfUs/Prefabs";

        // Colors - Dark Netflix-style theme
        private static readonly Color COLOR_BACKGROUND = new Color(0.08f, 0.08f, 0.1f, 0.98f);
        private static readonly Color COLOR_CARD_NORMAL = new Color(0.15f, 0.15f, 0.18f, 1f);
        private static readonly Color COLOR_CARD_HOVER = new Color(0.22f, 0.22f, 0.28f, 1f);
        private static readonly Color COLOR_PRIMARY = new Color(0.9f, 0.15f, 0.2f, 1f); // Netflix red
        private static readonly Color COLOR_TEXT_PRIMARY = Color.white;
        private static readonly Color COLOR_TEXT_SECONDARY = new Color(0.7f, 0.7f, 0.7f, 1f);
        private static readonly Color COLOR_BUTTON = new Color(0.9f, 0.15f, 0.2f, 1f);
        private static readonly Color COLOR_STAR = new Color(1f, 0.85f, 0.2f, 1f);

        #endregion

        #region Menu Items

        // REMOVED: Menu items consolidated into SDK Setup Wizard
        // [MenuItem("IntelliVerse-X SDK/More Of Us/Build All Prefabs", priority = 100)]
        // Use: IntelliVerse-X SDK > SDK Setup Wizard > More Of Us tab
        public static void BuildAllPrefabs()
        {
            string savePath = GetWritablePrefabPath();
            EnsureDirectoryExists(savePath);

            BuildAppCardPrefab(savePath);
            BuildMoreOfUsCanvasPrefab(savePath);

            AssetDatabase.Refresh();
            Debug.Log($"{LOG_PREFIX} All prefabs created in: {savePath}");
            EditorUtility.DisplayDialog("Success", $"More Of Us prefabs created in:\n{savePath}", "OK");
        }

        // [MenuItem("IntelliVerse-X SDK/More Of Us/Build App Card Prefab", priority = 101)]
        // Use: IntelliVerse-X SDK > SDK Setup Wizard > More Of Us tab
        public static void BuildAppCardPrefabMenu()
        {
            string savePath = GetWritablePrefabPath();
            EnsureDirectoryExists(savePath);
            BuildAppCardPrefab(savePath);
            AssetDatabase.Refresh();
        }

        // [MenuItem("IntelliVerse-X SDK/More Of Us/Build Canvas Prefab", priority = 102)]
        // Use: IntelliVerse-X SDK > SDK Setup Wizard > More Of Us tab
        public static void BuildCanvasPrefabMenu()
        {
            string savePath = GetWritablePrefabPath();
            EnsureDirectoryExists(savePath);
            BuildMoreOfUsCanvasPrefab(savePath);
            AssetDatabase.Refresh();
        }

        // [MenuItem("IntelliVerse-X SDK/More Of Us/Add To Current Scene", priority = 200)]
        // Use: IntelliVerse-X SDK > SDK Setup Wizard > More Of Us tab
        public static void AddToCurrentScene()
        {
            // Try to find existing prefab
            string prefabPath = GetWritablePrefabPath() + "/IVX_MoreOfUsCanvas.prefab";
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);

            if (prefab == null)
            {
                // Build prefabs first
                BuildAllPrefabs();
                prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            }

            if (prefab != null)
            {
                var instance = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
                if (instance != null)
                {
                    Undo.RegisterCreatedObjectUndo(instance, "Add More Of Us Canvas");
                    Selection.activeGameObject = instance;
                    Debug.Log($"{LOG_PREFIX} Added MoreOfUsCanvas to scene");
                }
            }
            else
            {
                Debug.LogError($"{LOG_PREFIX} Failed to create or find prefab");
            }
        }

        #endregion

        #region Prefab Builders

        /// <summary>
        /// Build the app card prefab
        /// </summary>
        public static GameObject BuildAppCardPrefab(string savePath)
        {
            var cardRoot = new GameObject("IVX_AppCard");

            try
            {
                // RectTransform
                var rectTransform = cardRoot.AddComponent<RectTransform>();
                rectTransform.sizeDelta = new Vector2(280, 380);
                rectTransform.pivot = new Vector2(0.5f, 0.5f);

                // Card background
                var bgImage = cardRoot.AddComponent<Image>();
                bgImage.color = COLOR_CARD_NORMAL;
                bgImage.raycastTarget = true;

                // Add rounded corners mask (via child)
                var maskObj = CreateChild(cardRoot, "Mask");
                var mask = maskObj.AddComponent<Mask>();
                mask.showMaskGraphic = false;
                var maskImage = maskObj.AddComponent<Image>();
                maskImage.color = Color.white;
                SetRectFill(maskObj.GetComponent<RectTransform>());

                // Content container
                var contentObj = CreateChild(maskObj, "Content");
                SetRectFill(contentObj.GetComponent<RectTransform>());
                var contentLayout = contentObj.AddComponent<VerticalLayoutGroup>();
                contentLayout.childControlHeight = false;
                contentLayout.childControlWidth = true;
                contentLayout.childForceExpandHeight = false;
                contentLayout.childForceExpandWidth = true;
                contentLayout.spacing = 0;
                contentLayout.padding = new RectOffset(0, 0, 0, 0);

                // App Icon Container (top 60%)
                var iconContainer = CreateChild(contentObj, "IconContainer");
                var iconContainerRect = iconContainer.GetComponent<RectTransform>();
                iconContainerRect.sizeDelta = new Vector2(0, 220);
                var iconContainerLE = iconContainer.AddComponent<LayoutElement>();
                iconContainerLE.preferredHeight = 220;
                iconContainerLE.flexibleWidth = 1;

                // App Icon
                var iconObj = CreateChild(iconContainer, "AppIcon");
                var iconRect = iconObj.GetComponent<RectTransform>();
                iconRect.anchorMin = Vector2.zero;
                iconRect.anchorMax = Vector2.one;
                iconRect.offsetMin = Vector2.zero;
                iconRect.offsetMax = Vector2.zero;
                var iconImage = iconObj.AddComponent<RawImage>();
                iconImage.color = Color.white;

                // Loading Spinner (on icon)
                var spinnerObj = CreateChild(iconContainer, "LoadingSpinner");
                var spinnerRect = spinnerObj.GetComponent<RectTransform>();
                spinnerRect.anchorMin = new Vector2(0.5f, 0.5f);
                spinnerRect.anchorMax = new Vector2(0.5f, 0.5f);
                spinnerRect.sizeDelta = new Vector2(50, 50);
                var spinnerImage = spinnerObj.AddComponent<Image>();
                spinnerImage.color = COLOR_TEXT_SECONDARY;
                // Note: Would need a spinner sprite in production

                // Info Panel (bottom 40%)
                var infoPanel = CreateChild(contentObj, "InfoPanel");
                var infoPanelRect = infoPanel.GetComponent<RectTransform>();
                infoPanelRect.sizeDelta = new Vector2(0, 160);
                var infoPanelLE = infoPanel.AddComponent<LayoutElement>();
                infoPanelLE.preferredHeight = 160;
                infoPanelLE.flexibleWidth = 1;
                var infoPanelLayout = infoPanel.AddComponent<VerticalLayoutGroup>();
                infoPanelLayout.childControlHeight = false;
                infoPanelLayout.childControlWidth = true;
                infoPanelLayout.childForceExpandHeight = false;
                infoPanelLayout.childForceExpandWidth = true;
                infoPanelLayout.spacing = 8;
                infoPanelLayout.padding = new RectOffset(15, 15, 12, 12);

                // App Name
                var nameObj = CreateChild(infoPanel, "AppName");
                var nameText = nameObj.AddComponent<TextMeshProUGUI>();
                nameText.text = "App Name";
                nameText.fontSize = 18;
                nameText.fontStyle = FontStyles.Bold;
                nameText.color = COLOR_TEXT_PRIMARY;
                nameText.alignment = TextAlignmentOptions.Left;
                nameText.overflowMode = TextOverflowModes.Ellipsis;
                nameText.maxVisibleLines = 2;
                var nameLE = nameObj.AddComponent<LayoutElement>();
                nameLE.preferredHeight = 48;

                // Description
                var descObj = CreateChild(infoPanel, "Description");
                var descText = descObj.AddComponent<TextMeshProUGUI>();
                descText.text = "App description goes here...";
                descText.fontSize = 12;
                descText.color = COLOR_TEXT_SECONDARY;
                descText.alignment = TextAlignmentOptions.TopLeft;
                descText.overflowMode = TextOverflowModes.Ellipsis;
                descText.maxVisibleLines = 2;
                var descLE = descObj.AddComponent<LayoutElement>();
                descLE.preferredHeight = 36;

                // Bottom Row (Rating + Price)
                var bottomRow = CreateChild(infoPanel, "BottomRow");
                var bottomRowLayout = bottomRow.AddComponent<HorizontalLayoutGroup>();
                bottomRowLayout.childControlHeight = true;
                bottomRowLayout.childControlWidth = false;
                bottomRowLayout.childForceExpandHeight = false;
                bottomRowLayout.childForceExpandWidth = false;
                bottomRowLayout.spacing = 10;
                var bottomRowLE = bottomRow.AddComponent<LayoutElement>();
                bottomRowLE.preferredHeight = 24;

                // Stars Container
                var starsContainer = CreateChild(bottomRow, "StarsContainer");
                var starsLayout = starsContainer.AddComponent<HorizontalLayoutGroup>();
                starsLayout.childControlHeight = true;
                starsLayout.childControlWidth = true;
                starsLayout.spacing = 2;
                var starsLE = starsContainer.AddComponent<LayoutElement>();
                starsLE.preferredWidth = 80;
                starsLE.preferredHeight = 16;

                // Create 5 star images
                for (int i = 0; i < 5; i++)
                {
                    var starObj = CreateChild(starsContainer, $"Star{i}");
                    var starImage = starObj.AddComponent<Image>();
                    starImage.color = COLOR_STAR;
                    var starLE = starObj.AddComponent<LayoutElement>();
                    starLE.preferredWidth = 14;
                    starLE.preferredHeight = 14;
                }

                // Rating Text
                var ratingObj = CreateChild(bottomRow, "Rating");
                var ratingText = ratingObj.AddComponent<TextMeshProUGUI>();
                ratingText.text = "4.5";
                ratingText.fontSize = 14;
                ratingText.color = COLOR_TEXT_PRIMARY;
                ratingText.alignment = TextAlignmentOptions.Left;
                var ratingLE = ratingObj.AddComponent<LayoutElement>();
                ratingLE.preferredWidth = 35;

                // Spacer
                var spacerObj = CreateChild(bottomRow, "Spacer");
                var spacerLE = spacerObj.AddComponent<LayoutElement>();
                spacerLE.flexibleWidth = 1;

                // Free Label
                var freeLabelObj = CreateChild(bottomRow, "FreeLabel");
                var freeLabelBg = freeLabelObj.AddComponent<Image>();
                freeLabelBg.color = COLOR_PRIMARY;
                var freeLabelLayout = freeLabelObj.AddComponent<HorizontalLayoutGroup>();
                freeLabelLayout.padding = new RectOffset(8, 8, 2, 2);
                var freeLabelLE = freeLabelObj.AddComponent<LayoutElement>();
                freeLabelLE.preferredHeight = 22;

                var freeLabelTextObj = CreateChild(freeLabelObj, "Text");
                var freeLabelText = freeLabelTextObj.AddComponent<TextMeshProUGUI>();
                freeLabelText.text = "FREE";
                freeLabelText.fontSize = 11;
                freeLabelText.fontStyle = FontStyles.Bold;
                freeLabelText.color = Color.white;
                freeLabelText.alignment = TextAlignmentOptions.Center;

                // Details Panel (shown on hover)
                var detailsPanel = CreateChild(cardRoot, "DetailsPanel");
                var detailsPanelRect = detailsPanel.GetComponent<RectTransform>();
                SetRectFill(detailsPanelRect);
                detailsPanelRect.offsetMin = new Vector2(0, 0);
                detailsPanelRect.offsetMax = new Vector2(0, -220); // Only cover info area
                detailsPanelRect.anchorMin = new Vector2(0, 0);
                detailsPanelRect.anchorMax = new Vector2(1, 1);
                
                var detailsBg = detailsPanel.AddComponent<Image>();
                detailsBg.color = new Color(0, 0, 0, 0.8f);
                
                var detailsGroup = detailsPanel.AddComponent<CanvasGroup>();
                detailsGroup.alpha = 0;
                detailsGroup.interactable = false;
                detailsGroup.blocksRaycasts = false;

                // Install Button (in details)
                var installBtnObj = CreateChild(detailsPanel, "InstallButton");
                var installBtnRect = installBtnObj.GetComponent<RectTransform>();
                installBtnRect.anchorMin = new Vector2(0.5f, 0.5f);
                installBtnRect.anchorMax = new Vector2(0.5f, 0.5f);
                installBtnRect.sizeDelta = new Vector2(120, 40);
                var installBtn = installBtnObj.AddComponent<Button>();
                var installBtnBg = installBtnObj.AddComponent<Image>();
                installBtnBg.color = COLOR_BUTTON;
                installBtn.targetGraphic = installBtnBg;

                var installBtnTextObj = CreateChild(installBtnObj, "Text");
                var installBtnText = installBtnTextObj.AddComponent<TextMeshProUGUI>();
                installBtnText.text = "GET";
                installBtnText.fontSize = 16;
                installBtnText.fontStyle = FontStyles.Bold;
                installBtnText.color = Color.white;
                installBtnText.alignment = TextAlignmentOptions.Center;
                SetRectFill(installBtnTextObj.GetComponent<RectTransform>());

                // Add IVXAppCard component
                var cardComponent = cardRoot.AddComponent<IVXAppCard>();
                
                // Wire up references using SerializedObject
                var so = new SerializedObject(cardComponent);
                so.FindProperty("_appIcon").objectReferenceValue = iconImage;
                so.FindProperty("_appNameText").objectReferenceValue = nameText;
                so.FindProperty("_descriptionText").objectReferenceValue = descText;
                so.FindProperty("_ratingText").objectReferenceValue = ratingText;
                so.FindProperty("_freeLabel").objectReferenceValue = freeLabelObj;
                so.FindProperty("_installButton").objectReferenceValue = installBtn;
                so.FindProperty("_detailsPanel").objectReferenceValue = detailsGroup;
                so.FindProperty("_cardBackground").objectReferenceValue = bgImage;
                so.FindProperty("_starsContainer").objectReferenceValue = starsContainer.transform;
                so.FindProperty("_loadingSpinner").objectReferenceValue = spinnerObj;
                so.ApplyModifiedPropertiesWithoutUndo();

                // Save prefab
                string prefabPath = $"{savePath}/IVX_AppCard.prefab";
                PrefabUtility.SaveAsPrefabAsset(cardRoot, prefabPath);
                Debug.Log($"{LOG_PREFIX} Created App Card prefab: {prefabPath}");

                return cardRoot;
            }
            finally
            {
                // Clean up scene object
                if (cardRoot != null)
                    Object.DestroyImmediate(cardRoot);
            }
        }

        /// <summary>
        /// Build the main canvas prefab
        /// </summary>
        public static GameObject BuildMoreOfUsCanvasPrefab(string savePath)
        {
            // First ensure card prefab exists
            string cardPrefabPath = $"{savePath}/IVX_AppCard.prefab";
            var cardPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(cardPrefabPath);
            if (cardPrefab == null)
            {
                BuildAppCardPrefab(savePath);
                cardPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(cardPrefabPath);
            }

            var canvasRoot = new GameObject("IVX_MoreOfUsCanvas");

            try
            {
                // Canvas
                var canvas = canvasRoot.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                canvas.sortingOrder = 100;

                var scaler = canvasRoot.AddComponent<CanvasScaler>();
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(1920, 1080);
                scaler.matchWidthOrHeight = 0.5f;

                canvasRoot.AddComponent<GraphicRaycaster>();

                var canvasGroup = canvasRoot.AddComponent<CanvasGroup>();

                // Background overlay
                var bgOverlay = CreateChild(canvasRoot, "BackgroundOverlay");
                SetRectFill(bgOverlay.GetComponent<RectTransform>());
                var bgImage = bgOverlay.AddComponent<Image>();
                bgImage.color = COLOR_BACKGROUND;
                bgImage.raycastTarget = true;

                // Main Panel
                var mainPanel = CreateChild(canvasRoot, "MainPanel");
                var mainPanelRect = mainPanel.GetComponent<RectTransform>();
                mainPanelRect.anchorMin = new Vector2(0, 0);
                mainPanelRect.anchorMax = new Vector2(1, 1);
                mainPanelRect.offsetMin = new Vector2(40, 60);
                mainPanelRect.offsetMax = new Vector2(-40, -40);

                var mainLayout = mainPanel.AddComponent<VerticalLayoutGroup>();
                // IMPORTANT: let the layout system drive child sizing/positioning.
                // When disabled, children keep their default RectTransform sizes/positions,
                // causing overlap/off-screen placement at runtime.
                mainLayout.childControlHeight = true;
                mainLayout.childControlWidth = true;
                mainLayout.childForceExpandHeight = false;
                mainLayout.childForceExpandWidth = true;
                mainLayout.spacing = 20;

                // Header
                var header = CreateChild(mainPanel, "Header");
                var headerLayout = header.AddComponent<HorizontalLayoutGroup>();
                headerLayout.childControlHeight = true;
                headerLayout.childControlWidth = true;
                headerLayout.childForceExpandHeight = false;
                headerLayout.childForceExpandWidth = false;
                headerLayout.spacing = 20;
                headerLayout.padding = new RectOffset(0, 0, 0, 10);
                var headerLE = header.AddComponent<LayoutElement>();
                headerLE.preferredHeight = 80;

                // Title Group
                var titleGroup = CreateChild(header, "TitleGroup");
                var titleGroupLayout = titleGroup.AddComponent<VerticalLayoutGroup>();
                titleGroupLayout.childControlHeight = false;
                titleGroupLayout.childControlWidth = true;
                titleGroupLayout.spacing = 5;
                var titleGroupLE = titleGroup.AddComponent<LayoutElement>();
                titleGroupLE.flexibleWidth = 1;

                // Title
                var titleObj = CreateChild(titleGroup, "Title");
                var titleText = titleObj.AddComponent<TextMeshProUGUI>();
                titleText.text = "More From Us";
                titleText.fontSize = 36;
                titleText.fontStyle = FontStyles.Bold;
                titleText.color = COLOR_TEXT_PRIMARY;
                var titleLE = titleObj.AddComponent<LayoutElement>();
                titleLE.preferredHeight = 45;

                // Subtitle
                var subtitleObj = CreateChild(titleGroup, "Subtitle");
                var subtitleText = subtitleObj.AddComponent<TextMeshProUGUI>();
                subtitleText.text = "Check out our other games!";
                subtitleText.fontSize = 16;
                subtitleText.color = COLOR_TEXT_SECONDARY;
                var subtitleLE = subtitleObj.AddComponent<LayoutElement>();
                subtitleLE.preferredHeight = 25;

                // Refresh Button
                var refreshBtnObj = CreateChild(header, "RefreshButton");
                var refreshBtnLE = refreshBtnObj.AddComponent<LayoutElement>();
                refreshBtnLE.preferredWidth = 44;
                refreshBtnLE.preferredHeight = 44;
                var refreshBtn = refreshBtnObj.AddComponent<Button>();
                var refreshBtnBg = refreshBtnObj.AddComponent<Image>();
                refreshBtnBg.color = new Color(1, 1, 1, 0.1f);
                refreshBtn.targetGraphic = refreshBtnBg;

                var refreshIconObj = CreateChild(refreshBtnObj, "Icon");
                var refreshIconText = refreshIconObj.AddComponent<TextMeshProUGUI>();
                refreshIconText.text = "R"; // Would use icon in production
                refreshIconText.fontSize = 20;
                refreshIconText.color = COLOR_TEXT_PRIMARY;
                refreshIconText.alignment = TextAlignmentOptions.Center;
                SetRectFill(refreshIconObj.GetComponent<RectTransform>());

                // Close Button
                var closeBtnObj = CreateChild(header, "CloseButton");
                var closeBtnLE = closeBtnObj.AddComponent<LayoutElement>();
                closeBtnLE.preferredWidth = 44;
                closeBtnLE.preferredHeight = 44;
                var closeBtn = closeBtnObj.AddComponent<Button>();
                var closeBtnBg = closeBtnObj.AddComponent<Image>();
                closeBtnBg.color = new Color(1, 1, 1, 0.1f);
                closeBtn.targetGraphic = closeBtnBg;

                var closeIconObj = CreateChild(closeBtnObj, "Icon");
                var closeIconText = closeIconObj.AddComponent<TextMeshProUGUI>();
                closeIconText.text = "X";
                closeIconText.fontSize = 24;
                closeIconText.fontStyle = FontStyles.Bold;
                closeIconText.color = COLOR_TEXT_PRIMARY;
                closeIconText.alignment = TextAlignmentOptions.Center;
                SetRectFill(closeIconObj.GetComponent<RectTransform>());

                // Carousel Container
                var carouselContainer = CreateChild(mainPanel, "CarouselContainer");
                var carouselContainerLE = carouselContainer.AddComponent<LayoutElement>();
                carouselContainerLE.flexibleHeight = 1;
                carouselContainerLE.preferredHeight = 420;

                // Scroll View
                var scrollView = CreateChild(carouselContainer, "ScrollView");
                SetRectFill(scrollView.GetComponent<RectTransform>());
                var scrollRect = scrollView.AddComponent<ScrollRect>();
                scrollRect.horizontal = true;
                scrollRect.vertical = false;
                scrollRect.movementType = ScrollRect.MovementType.Elastic;
                scrollRect.elasticity = 0.1f;
                scrollRect.inertia = true;
                scrollRect.decelerationRate = 0.135f;
                scrollRect.scrollSensitivity = 1f;

                // Viewport
                var viewport = CreateChild(scrollView, "Viewport");
                SetRectFill(viewport.GetComponent<RectTransform>());
                var viewportMask = viewport.AddComponent<Mask>();
                viewportMask.showMaskGraphic = false;
                var viewportImage = viewport.AddComponent<Image>();
                viewportImage.color = Color.white;
                scrollRect.viewport = viewport.GetComponent<RectTransform>();

                // Content
                var content = CreateChild(viewport, "Content");
                var contentRect = content.GetComponent<RectTransform>();
                contentRect.anchorMin = new Vector2(0, 0);
                contentRect.anchorMax = new Vector2(0, 1);
                contentRect.pivot = new Vector2(0, 0.5f);
                contentRect.sizeDelta = new Vector2(0, 0);
                var contentSizeFitter = content.AddComponent<ContentSizeFitter>();
                contentSizeFitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
                contentSizeFitter.verticalFit = ContentSizeFitter.FitMode.Unconstrained;
                var contentLayout = content.AddComponent<HorizontalLayoutGroup>();
                contentLayout.childControlHeight = false;
                contentLayout.childControlWidth = false;
                contentLayout.childForceExpandHeight = false;
                contentLayout.childForceExpandWidth = false;
                contentLayout.spacing = 20;
                contentLayout.padding = new RectOffset(20, 20, 20, 20);
                scrollRect.content = contentRect;

                // Navigation Arrows
                var leftArrowObj = CreateNavigationArrow(carouselContainer, "LeftArrow", "<", true);
                var rightArrowObj = CreateNavigationArrow(carouselContainer, "RightArrow", ">", false);

                // Loading Panel
                var loadingPanel = CreateChild(carouselContainer, "LoadingPanel");
                SetRectFill(loadingPanel.GetComponent<RectTransform>());
                loadingPanel.SetActive(false);
                var loadingPanelBg = loadingPanel.AddComponent<Image>();
                loadingPanelBg.color = new Color(0, 0, 0, 0.5f);

                var loadingTextObj = CreateChild(loadingPanel, "LoadingText");
                var loadingTextRect = loadingTextObj.GetComponent<RectTransform>();
                loadingTextRect.anchorMin = new Vector2(0.5f, 0.5f);
                loadingTextRect.anchorMax = new Vector2(0.5f, 0.5f);
                loadingTextRect.sizeDelta = new Vector2(300, 50);
                var loadingText = loadingTextObj.AddComponent<TextMeshProUGUI>();
                loadingText.text = "Loading apps...";
                loadingText.fontSize = 24;
                loadingText.color = COLOR_TEXT_PRIMARY;
                loadingText.alignment = TextAlignmentOptions.Center;

                // Empty State Panel
                var emptyStatePanel = CreateChild(carouselContainer, "EmptyStatePanel");
                SetRectFill(emptyStatePanel.GetComponent<RectTransform>());
                emptyStatePanel.SetActive(false);

                var emptyStateTextObj = CreateChild(emptyStatePanel, "EmptyText");
                var emptyStateTextRect = emptyStateTextObj.GetComponent<RectTransform>();
                emptyStateTextRect.anchorMin = new Vector2(0.5f, 0.5f);
                emptyStateTextRect.anchorMax = new Vector2(0.5f, 0.5f);
                emptyStateTextRect.sizeDelta = new Vector2(400, 100);
                var emptyStateText = emptyStateTextObj.AddComponent<TextMeshProUGUI>();
                emptyStateText.text = "No apps available.\nCheck back later!";
                emptyStateText.fontSize = 20;
                emptyStateText.color = COLOR_TEXT_SECONDARY;
                emptyStateText.alignment = TextAlignmentOptions.Center;

                var retryBtnObj = CreateChild(emptyStatePanel, "RetryButton");
                var retryBtnRect = retryBtnObj.GetComponent<RectTransform>();
                retryBtnRect.anchorMin = new Vector2(0.5f, 0.5f);
                retryBtnRect.anchorMax = new Vector2(0.5f, 0.5f);
                retryBtnRect.anchoredPosition = new Vector2(0, -80);
                retryBtnRect.sizeDelta = new Vector2(120, 40);
                var retryBtn = retryBtnObj.AddComponent<Button>();
                var retryBtnBg = retryBtnObj.AddComponent<Image>();
                retryBtnBg.color = COLOR_BUTTON;
                retryBtn.targetGraphic = retryBtnBg;

                var retryBtnTextObj = CreateChild(retryBtnObj, "Text");
                var retryBtnText = retryBtnTextObj.AddComponent<TextMeshProUGUI>();
                retryBtnText.text = "Retry";
                retryBtnText.fontSize = 16;
                retryBtnText.fontStyle = FontStyles.Bold;
                retryBtnText.color = Color.white;
                retryBtnText.alignment = TextAlignmentOptions.Center;
                SetRectFill(retryBtnTextObj.GetComponent<RectTransform>());

                // Add IVXMoreOfUsCanvas component
                var canvasComponent = canvasRoot.AddComponent<IVXMoreOfUsCanvas>();

                // Wire up references
                var so = new SerializedObject(canvasComponent);
                so.FindProperty("_canvas").objectReferenceValue = canvas;
                so.FindProperty("_canvasGroup").objectReferenceValue = canvasGroup;
                so.FindProperty("_contentContainer").objectReferenceValue = contentRect;
                so.FindProperty("_scrollRect").objectReferenceValue = scrollRect;
                so.FindProperty("_layoutGroup").objectReferenceValue = contentLayout;
                so.FindProperty("_titleText").objectReferenceValue = titleText;
                so.FindProperty("_subtitleText").objectReferenceValue = subtitleText;
                so.FindProperty("_closeButton").objectReferenceValue = closeBtn;
                so.FindProperty("_refreshButton").objectReferenceValue = refreshBtn;
                so.FindProperty("_leftArrowButton").objectReferenceValue = leftArrowObj.GetComponent<Button>();
                so.FindProperty("_rightArrowButton").objectReferenceValue = rightArrowObj.GetComponent<Button>();
                so.FindProperty("_leftArrowGroup").objectReferenceValue = leftArrowObj.GetComponent<CanvasGroup>();
                so.FindProperty("_rightArrowGroup").objectReferenceValue = rightArrowObj.GetComponent<CanvasGroup>();
                so.FindProperty("_loadingPanel").objectReferenceValue = loadingPanel;
                so.FindProperty("_loadingText").objectReferenceValue = loadingText;
                so.FindProperty("_emptyStatePanel").objectReferenceValue = emptyStatePanel;
                so.FindProperty("_emptyStateText").objectReferenceValue = emptyStateText;
                so.FindProperty("_retryButton").objectReferenceValue = retryBtn;

                // Set card prefab reference
                var cardPrefabComponent = cardPrefab?.GetComponent<IVXAppCard>();
                if (cardPrefabComponent != null)
                    so.FindProperty("_cardPrefab").objectReferenceValue = cardPrefabComponent;

                so.ApplyModifiedPropertiesWithoutUndo();

                // Save prefab
                string prefabPath = $"{savePath}/IVX_MoreOfUsCanvas.prefab";
                PrefabUtility.SaveAsPrefabAsset(canvasRoot, prefabPath);
                Debug.Log($"{LOG_PREFIX} Created MoreOfUsCanvas prefab: {prefabPath}");

                return canvasRoot;
            }
            finally
            {
                // Clean up scene object
                if (canvasRoot != null)
                    Object.DestroyImmediate(canvasRoot);
            }
        }

        #endregion

        #region Helpers

        private static GameObject CreateChild(GameObject parent, string name)
        {
            var child = new GameObject(name);
            child.transform.SetParent(parent.transform, false);
            child.AddComponent<RectTransform>();
            return child;
        }

        private static void SetRectFill(RectTransform rect)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }

        private static GameObject CreateNavigationArrow(GameObject parent, string name, string symbol, bool isLeft)
        {
            var arrowObj = CreateChild(parent, name);
            var arrowRect = arrowObj.GetComponent<RectTransform>();
            
            if (isLeft)
            {
                arrowRect.anchorMin = new Vector2(0, 0.5f);
                arrowRect.anchorMax = new Vector2(0, 0.5f);
                arrowRect.anchoredPosition = new Vector2(10, 0);
            }
            else
            {
                arrowRect.anchorMin = new Vector2(1, 0.5f);
                arrowRect.anchorMax = new Vector2(1, 0.5f);
                arrowRect.anchoredPosition = new Vector2(-10, 0);
            }
            
            arrowRect.sizeDelta = new Vector2(50, 100);

            var canvasGroup = arrowObj.AddComponent<CanvasGroup>();
            var button = arrowObj.AddComponent<Button>();
            var bgImage = arrowObj.AddComponent<Image>();
            bgImage.color = new Color(0, 0, 0, 0.5f);
            button.targetGraphic = bgImage;

            var textObj = CreateChild(arrowObj, "Text");
            var text = textObj.AddComponent<TextMeshProUGUI>();
            text.text = symbol;
            text.fontSize = 32;
            text.fontStyle = FontStyles.Bold;
            text.color = Color.white;
            text.alignment = TextAlignmentOptions.Center;
            SetRectFill(textObj.GetComponent<RectTransform>());

            return arrowObj;
        }

        private static string GetWritablePrefabPath()
        {
            // Check if running from UPM package
            bool isPackage = Directory.Exists("Packages/com.intelliversex.sdk");
            return isPackage ? DEFAULT_PREFABS_PATH : PACKAGE_PREFABS_PATH;
        }

        private static void EnsureDirectoryExists(string path)
        {
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
                AssetDatabase.Refresh();
            }
        }

        #endregion
    }
}
#endif
