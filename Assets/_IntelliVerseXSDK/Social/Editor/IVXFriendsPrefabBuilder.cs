#if UNITY_EDITOR
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using TMPro;
using IntelliVerseX.Social;
using IntelliVerseX.Social.UI;

namespace IntelliVerseX.Social.Editor
{
    /// <summary>
    /// Programmatically builds the complete Friends UI prefabs.
    /// Creates a polished, production-ready Friends panel with all components.
    /// </summary>
    public static class IVXFriendsPrefabBuilder
    {
        #region Colors & Styling

        // Modern dark theme colors
        private static readonly Color PanelBackground = new Color(0.12f, 0.14f, 0.18f, 0.98f);
        private static readonly Color HeaderBackground = new Color(0.15f, 0.17f, 0.22f, 1f);
        private static readonly Color TabActiveColor = new Color(0.25f, 0.52f, 0.96f, 1f);
        private static readonly Color TabInactiveColor = new Color(0.3f, 0.32f, 0.38f, 1f);
        private static readonly Color SlotBackground = new Color(0.18f, 0.20f, 0.25f, 1f);
        private static readonly Color SlotHoverColor = new Color(0.22f, 0.24f, 0.30f, 1f);
        private static readonly Color AcceptButtonColor = new Color(0.2f, 0.7f, 0.3f, 1f);
        private static readonly Color RejectButtonColor = new Color(0.8f, 0.25f, 0.25f, 1f);
        private static readonly Color AddButtonColor = new Color(0.25f, 0.52f, 0.96f, 1f);
        private static readonly Color SearchFieldBackground = new Color(0.1f, 0.12f, 0.15f, 1f);
        private static readonly Color TextPrimary = new Color(0.95f, 0.95f, 0.95f, 1f);
        private static readonly Color TextSecondary = new Color(0.6f, 0.62f, 0.68f, 1f);
        private static readonly Color OnlineColor = new Color(0.3f, 0.85f, 0.4f, 1f);
        private static readonly Color OfflineColor = new Color(0.5f, 0.52f, 0.58f, 1f);
        private static readonly Color BadgeColor = new Color(0.9f, 0.3f, 0.3f, 1f);
        private static readonly Color ToastBackground = new Color(0.1f, 0.1f, 0.1f, 0.95f);
        private static readonly Color OverlayColor = new Color(0f, 0f, 0f, 0.7f);

        #endregion

        #region Public Methods

        /// <summary>
        /// Creates the complete Friends Canvas with all UI elements.
        /// </summary>
        public static GameObject CreateFriendsCanvas(Transform parent = null)
        {
            // Create Canvas
            var canvasGO = new GameObject("IVXFriendsCanvas");
            if (parent != null) canvasGO.transform.SetParent(parent, false);

            var canvas = canvasGO.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 100;

            var scaler = canvasGO.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080, 1920);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;

            canvasGO.AddComponent<GraphicRaycaster>();

            // Create the Friends Panel
            var panelGO = CreateFriendsPanel(canvasGO.transform);

            // Add the panel controller
            var panelController = panelGO.AddComponent<IVXFriendsPanel>();
            
            // Wire up all references
            WireUpPanelReferences(panelController, panelGO);

            return canvasGO;
        }

        /// <summary>
        /// Creates a Friend Slot prefab.
        /// </summary>
        public static GameObject CreateFriendSlotPrefab()
        {
            var slotGO = CreateFriendSlotUI("IVXFriendSlot");
            slotGO.AddComponent<IVXFriendSlot>();
            WireUpFriendSlotReferences(slotGO);
            return slotGO;
        }

        /// <summary>
        /// Creates a Friend Request Slot prefab.
        /// </summary>
        public static GameObject CreateFriendRequestSlotPrefab()
        {
            var slotGO = CreateFriendRequestSlotUI("IVXFriendRequestSlot");
            slotGO.AddComponent<IVXFriendRequestSlot>();
            WireUpRequestSlotReferences(slotGO);
            return slotGO;
        }

        /// <summary>
        /// Creates a Search Result Slot prefab.
        /// </summary>
        public static GameObject CreateSearchSlotPrefab()
        {
            var slotGO = CreateSearchSlotUI("IVXFriendSearchSlot");
            slotGO.AddComponent<IVXFriendSearchSlot>();
            WireUpSearchSlotReferences(slotGO);
            return slotGO;
        }

        /// <summary>
        /// Saves all prefabs to the specified folder.
        /// </summary>
        public static void SavePrefabs(string folderPath)
        {
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }

            // Create and save Friend Slot prefab
            var friendSlot = CreateFriendSlotPrefab();
            SavePrefab(friendSlot, Path.Combine(folderPath, "IVXFriendSlot.prefab"));
            Object.DestroyImmediate(friendSlot);

            // Create and save Request Slot prefab
            var requestSlot = CreateFriendRequestSlotPrefab();
            SavePrefab(requestSlot, Path.Combine(folderPath, "IVXFriendRequestSlot.prefab"));
            Object.DestroyImmediate(requestSlot);

            // Create and save Search Slot prefab
            var searchSlot = CreateSearchSlotPrefab();
            SavePrefab(searchSlot, Path.Combine(folderPath, "IVXFriendSearchSlot.prefab"));
            Object.DestroyImmediate(searchSlot);

            AssetDatabase.Refresh();
            Debug.Log($"[IVXFriends] Prefabs saved to: {folderPath}");
        }

        #endregion

        #region Panel Creation

        private static GameObject CreateFriendsPanel(Transform parent)
        {
            // Panel Root (full screen overlay)
            var panelRoot = CreateUIElement("PanelRoot", parent);
            StretchToFill(panelRoot);
            panelRoot.gameObject.SetActive(false); // Start hidden

            var rootCanvasGroup = panelRoot.gameObject.AddComponent<CanvasGroup>();

            // Background overlay (click to close)
            var overlay = CreateUIElement("Overlay", panelRoot);
            StretchToFill(overlay);
            var overlayImage = overlay.gameObject.AddComponent<Image>();
            overlayImage.color = OverlayColor;
            var overlayButton = overlay.gameObject.AddComponent<Button>();
            overlayButton.transition = Selectable.Transition.None;

            // Main Panel Container
            var panelContainer = CreateUIElement("PanelContainer", panelRoot);
            panelContainer.anchorMin = new Vector2(0.05f, 0.05f);
            panelContainer.anchorMax = new Vector2(0.95f, 0.95f);
            panelContainer.offsetMin = Vector2.zero;
            panelContainer.offsetMax = Vector2.zero;

            var panelBg = panelContainer.gameObject.AddComponent<Image>();
            panelBg.color = PanelBackground;
            AddRoundedCorners(panelContainer.gameObject, 20f);

            var panelLayout = panelContainer.gameObject.AddComponent<VerticalLayoutGroup>();
            panelLayout.padding = new RectOffset(0, 0, 0, 0);
            panelLayout.spacing = 0;
            panelLayout.childControlHeight = false;
            panelLayout.childControlWidth = true;
            panelLayout.childForceExpandHeight = false;
            panelLayout.childForceExpandWidth = true;

            // Header
            CreateHeader(panelContainer);

            // Tab Bar
            CreateTabBar(panelContainer);

            // Content Area
            CreateContentArea(panelContainer);

            // Loading Overlay
            CreateLoadingOverlay(panelContainer);

            // Toast
            CreateToast(panelRoot);

            // Confirmation Dialog
            CreateConfirmDialog(panelRoot);

            return panelRoot.gameObject;
        }

        private static void CreateHeader(RectTransform parent)
        {
            var header = CreateUIElement("Header", parent);
            header.sizeDelta = new Vector2(0, 80);

            var headerBg = header.gameObject.AddComponent<Image>();
            headerBg.color = HeaderBackground;

            var headerLayout = header.gameObject.AddComponent<HorizontalLayoutGroup>();
            headerLayout.padding = new RectOffset(20, 20, 15, 15);
            headerLayout.spacing = 10;
            headerLayout.childAlignment = TextAnchor.MiddleLeft;
            headerLayout.childControlHeight = false;
            headerLayout.childControlWidth = false;

            // Title
            var titleGO = CreateUIElement("Title", header);
            titleGO.sizeDelta = new Vector2(200, 50);
            var titleText = titleGO.gameObject.AddComponent<TextMeshProUGUI>();
            titleText.text = "Friends";
            titleText.fontSize = 28;
            titleText.fontStyle = FontStyles.Bold;
            titleText.color = TextPrimary;
            titleText.alignment = TextAlignmentOptions.Left;

            // Spacer
            var spacer = CreateUIElement("Spacer", header);
            spacer.sizeDelta = new Vector2(100, 50);
            var spacerLayout = spacer.gameObject.AddComponent<LayoutElement>();
            spacerLayout.flexibleWidth = 1;

            // Close Button
            var closeBtn = CreateButton("CloseButton", header, "✕", 50, 50);
            var closeBtnImage = closeBtn.GetComponent<Image>();
            closeBtnImage.color = new Color(0.3f, 0.32f, 0.38f, 1f);
            var closeBtnText = closeBtn.GetComponentInChildren<TextMeshProUGUI>();
            closeBtnText.fontSize = 24;
        }

        private static void CreateTabBar(RectTransform parent)
        {
            var tabBar = CreateUIElement("TabBar", parent);
            tabBar.sizeDelta = new Vector2(0, 60);

            var tabBarBg = tabBar.gameObject.AddComponent<Image>();
            tabBarBg.color = HeaderBackground;

            var tabLayout = tabBar.gameObject.AddComponent<HorizontalLayoutGroup>();
            tabLayout.padding = new RectOffset(10, 10, 5, 5);
            tabLayout.spacing = 5;
            tabLayout.childAlignment = TextAnchor.MiddleCenter;
            tabLayout.childControlHeight = true;
            tabLayout.childControlWidth = true;
            tabLayout.childForceExpandWidth = true;

            // Friends Tab
            CreateTabButton("FriendsTab", tabBar, "Friends", true);

            // Requests Tab (with badge)
            var requestsTab = CreateTabButton("RequestsTab", tabBar, "Requests", false);
            CreateBadge(requestsTab.transform);

            // Search Tab
            CreateTabButton("SearchTab", tabBar, "Search", false);
        }

        private static GameObject CreateTabButton(string name, RectTransform parent, string text, bool isActive)
        {
            var tabGO = CreateUIElement(name, parent);

            var tabImage = tabGO.gameObject.AddComponent<Image>();
            tabImage.color = isActive ? TabActiveColor : TabInactiveColor;
            AddRoundedCorners(tabGO.gameObject, 8f);

            var tabBtn = tabGO.gameObject.AddComponent<Button>();
            tabBtn.targetGraphic = tabImage;

            var tabTextGO = CreateUIElement("Text", tabGO);
            StretchToFill(tabTextGO);
            var tabText = tabTextGO.gameObject.AddComponent<TextMeshProUGUI>();
            tabText.text = text;
            tabText.fontSize = 16;
            tabText.fontStyle = FontStyles.Bold;
            tabText.color = TextPrimary;
            tabText.alignment = TextAlignmentOptions.Center;

            // Tab indicator
            var indicator = CreateUIElement("Indicator", tabGO);
            indicator.anchorMin = new Vector2(0, 0);
            indicator.anchorMax = new Vector2(1, 0);
            indicator.pivot = new Vector2(0.5f, 0);
            indicator.sizeDelta = new Vector2(0, 3);
            indicator.anchoredPosition = Vector2.zero;
            var indicatorImage = indicator.gameObject.AddComponent<Image>();
            indicatorImage.color = TextPrimary;
            indicator.gameObject.SetActive(isActive);

            return tabGO.gameObject;
        }

        private static void CreateBadge(Transform parent)
        {
            var badge = CreateUIElement("Badge", (RectTransform)parent);
            badge.anchorMin = new Vector2(1, 1);
            badge.anchorMax = new Vector2(1, 1);
            badge.pivot = new Vector2(1, 1);
            badge.sizeDelta = new Vector2(24, 24);
            badge.anchoredPosition = new Vector2(5, 5);

            var badgeImage = badge.gameObject.AddComponent<Image>();
            badgeImage.color = BadgeColor;
            AddRoundedCorners(badge.gameObject, 12f);

            var badgeTextGO = CreateUIElement("Text", badge);
            StretchToFill(badgeTextGO);
            var badgeText = badgeTextGO.gameObject.AddComponent<TextMeshProUGUI>();
            badgeText.text = "0";
            badgeText.fontSize = 12;
            badgeText.fontStyle = FontStyles.Bold;
            badgeText.color = TextPrimary;
            badgeText.alignment = TextAlignmentOptions.Center;

            badge.gameObject.SetActive(false);
        }

        private static void CreateContentArea(RectTransform parent)
        {
            var content = CreateUIElement("ContentArea", parent);
            var contentLayout = content.gameObject.AddComponent<LayoutElement>();
            contentLayout.flexibleHeight = 1;

            // Friends Content
            CreateFriendsContent(content);

            // Requests Content
            CreateRequestsContent(content);

            // Search Content
            CreateSearchContent(content);
        }

        private static void CreateFriendsContent(RectTransform parent)
        {
            var friendsContent = CreateUIElement("FriendsContent", parent);
            StretchToFill(friendsContent);
            friendsContent.gameObject.AddComponent<CanvasGroup>();

            var layout = friendsContent.gameObject.AddComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset(15, 15, 15, 15);
            layout.spacing = 10;
            layout.childControlHeight = false;
            layout.childControlWidth = true;
            layout.childForceExpandHeight = false;

            // Count text
            var countGO = CreateUIElement("CountText", friendsContent);
            countGO.sizeDelta = new Vector2(0, 30);
            var countText = countGO.gameObject.AddComponent<TextMeshProUGUI>();
            countText.text = "0 Friends";
            countText.fontSize = 14;
            countText.color = TextSecondary;

            // Scroll View
            CreateScrollView("FriendsScrollView", friendsContent, "FriendsListContainer");

            // Empty text
            var emptyGO = CreateUIElement("EmptyText", friendsContent);
            emptyGO.sizeDelta = new Vector2(0, 100);
            var emptyText = emptyGO.gameObject.AddComponent<TextMeshProUGUI>();
            emptyText.text = "No friends yet.\nSearch for users to add!";
            emptyText.fontSize = 16;
            emptyText.color = TextSecondary;
            emptyText.alignment = TextAlignmentOptions.Center;
            emptyGO.gameObject.SetActive(false);
        }

        private static void CreateRequestsContent(RectTransform parent)
        {
            var requestsContent = CreateUIElement("RequestsContent", parent);
            StretchToFill(requestsContent);
            requestsContent.gameObject.SetActive(false);
            requestsContent.gameObject.AddComponent<CanvasGroup>();

            var layout = requestsContent.gameObject.AddComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset(15, 15, 15, 15);
            layout.spacing = 10;
            layout.childControlHeight = false;
            layout.childControlWidth = true;
            layout.childForceExpandHeight = false;

            // Count text
            var countGO = CreateUIElement("CountText", requestsContent);
            countGO.sizeDelta = new Vector2(0, 30);
            var countText = countGO.gameObject.AddComponent<TextMeshProUGUI>();
            countText.text = "0 Requests";
            countText.fontSize = 14;
            countText.color = TextSecondary;

            // Scroll View
            CreateScrollView("RequestsScrollView", requestsContent, "RequestsListContainer");

            // Empty text
            var emptyGO = CreateUIElement("EmptyText", requestsContent);
            emptyGO.sizeDelta = new Vector2(0, 100);
            var emptyText = emptyGO.gameObject.AddComponent<TextMeshProUGUI>();
            emptyText.text = "No pending requests";
            emptyText.fontSize = 16;
            emptyText.color = TextSecondary;
            emptyText.alignment = TextAlignmentOptions.Center;
            emptyGO.gameObject.SetActive(false);
        }

        private static void CreateSearchContent(RectTransform parent)
        {
            var searchContent = CreateUIElement("SearchContent", parent);
            StretchToFill(searchContent);
            searchContent.gameObject.SetActive(false);
            searchContent.gameObject.AddComponent<CanvasGroup>();

            var layout = searchContent.gameObject.AddComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset(15, 15, 15, 15);
            layout.spacing = 10;
            layout.childControlHeight = false;
            layout.childControlWidth = true;
            layout.childForceExpandHeight = false;

            // Search Bar
            CreateSearchBar(searchContent);

            // Results count
            var countGO = CreateUIElement("ResultsText", searchContent);
            countGO.sizeDelta = new Vector2(0, 30);
            var countText = countGO.gameObject.AddComponent<TextMeshProUGUI>();
            countText.text = "";
            countText.fontSize = 14;
            countText.color = TextSecondary;

            // Scroll View
            CreateScrollView("SearchScrollView", searchContent, "SearchResultsContainer");

            // Instructions
            var instructionsGO = CreateUIElement("Instructions", searchContent);
            instructionsGO.sizeDelta = new Vector2(0, 100);
            var instructionsText = instructionsGO.gameObject.AddComponent<TextMeshProUGUI>();
            instructionsText.text = "Search for users by name\nto add them as friends";
            instructionsText.fontSize = 16;
            instructionsText.color = TextSecondary;
            instructionsText.alignment = TextAlignmentOptions.Center;

            // Empty text
            var emptyGO = CreateUIElement("EmptyText", searchContent);
            emptyGO.sizeDelta = new Vector2(0, 100);
            var emptyText = emptyGO.gameObject.AddComponent<TextMeshProUGUI>();
            emptyText.text = "No users found";
            emptyText.fontSize = 16;
            emptyText.color = TextSecondary;
            emptyText.alignment = TextAlignmentOptions.Center;
            emptyGO.gameObject.SetActive(false);
        }

        private static void CreateSearchBar(RectTransform parent)
        {
            var searchBar = CreateUIElement("SearchBar", parent);
            searchBar.sizeDelta = new Vector2(0, 50);

            var searchLayout = searchBar.gameObject.AddComponent<HorizontalLayoutGroup>();
            searchLayout.padding = new RectOffset(0, 0, 0, 0);
            searchLayout.spacing = 10;
            searchLayout.childControlHeight = true;
            searchLayout.childControlWidth = true;
            searchLayout.childForceExpandWidth = false;

            // Input Field
            var inputGO = CreateUIElement("SearchInput", searchBar);
            var inputLayout = inputGO.gameObject.AddComponent<LayoutElement>();
            inputLayout.flexibleWidth = 1;

            var inputBg = inputGO.gameObject.AddComponent<Image>();
            inputBg.color = SearchFieldBackground;
            AddRoundedCorners(inputGO.gameObject, 8f);

            var inputField = inputGO.gameObject.AddComponent<TMP_InputField>();
            inputField.textViewport = inputGO;

            // Text Area
            var textArea = CreateUIElement("TextArea", inputGO);
            StretchToFill(textArea);
            textArea.offsetMin = new Vector2(15, 5);
            textArea.offsetMax = new Vector2(-15, -5);

            // Placeholder
            var placeholderGO = CreateUIElement("Placeholder", textArea);
            StretchToFill(placeholderGO);
            var placeholder = placeholderGO.gameObject.AddComponent<TextMeshProUGUI>();
            placeholder.text = "Search users...";
            placeholder.fontSize = 16;
            placeholder.color = TextSecondary;
            placeholder.alignment = TextAlignmentOptions.Left;

            // Text
            var textGO = CreateUIElement("Text", textArea);
            StretchToFill(textGO);
            var text = textGO.gameObject.AddComponent<TextMeshProUGUI>();
            text.fontSize = 16;
            text.color = TextPrimary;
            text.alignment = TextAlignmentOptions.Left;

            inputField.textViewport = textArea;
            inputField.textComponent = text;
            inputField.placeholder = placeholder;

            // Search Button
            var searchBtn = CreateButton("SearchButton", searchBar, "Search", 100, 50);
            var searchBtnImage = searchBtn.GetComponent<Image>();
            searchBtnImage.color = AddButtonColor;
        }

        private static void CreateScrollView(string name, RectTransform parent, string containerName)
        {
            var scrollGO = CreateUIElement(name, parent);
            var scrollLayout = scrollGO.gameObject.AddComponent<LayoutElement>();
            scrollLayout.flexibleHeight = 1;

            var scrollRect = scrollGO.gameObject.AddComponent<ScrollRect>();
            scrollRect.horizontal = false;
            scrollRect.vertical = true;
            scrollRect.movementType = ScrollRect.MovementType.Elastic;
            scrollRect.elasticity = 0.1f;

            var scrollMask = scrollGO.gameObject.AddComponent<Mask>();
            scrollMask.showMaskGraphic = false;
            var scrollImage = scrollGO.gameObject.AddComponent<Image>();
            scrollImage.color = Color.clear;

            // Viewport
            var viewport = CreateUIElement("Viewport", scrollGO);
            StretchToFill(viewport);
            scrollRect.viewport = viewport;

            // Content
            var content = CreateUIElement(containerName, viewport);
            content.anchorMin = new Vector2(0, 1);
            content.anchorMax = new Vector2(1, 1);
            content.pivot = new Vector2(0.5f, 1);
            content.sizeDelta = new Vector2(0, 0);

            var contentLayout = content.gameObject.AddComponent<VerticalLayoutGroup>();
            contentLayout.padding = new RectOffset(0, 0, 0, 0);
            contentLayout.spacing = 8;
            contentLayout.childControlHeight = false;
            contentLayout.childControlWidth = true;
            contentLayout.childForceExpandHeight = false;
            contentLayout.childForceExpandWidth = true;

            var contentFitter = content.gameObject.AddComponent<ContentSizeFitter>();
            contentFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            scrollRect.content = content;
        }

        private static void CreateLoadingOverlay(RectTransform parent)
        {
            var loading = CreateUIElement("LoadingOverlay", parent);
            StretchToFill(loading);
            loading.gameObject.SetActive(false);

            var loadingBg = loading.gameObject.AddComponent<Image>();
            loadingBg.color = new Color(0, 0, 0, 0.5f);

            // Spinner
            var spinner = CreateUIElement("Spinner", loading);
            spinner.anchorMin = new Vector2(0.5f, 0.5f);
            spinner.anchorMax = new Vector2(0.5f, 0.5f);
            spinner.sizeDelta = new Vector2(50, 50);

            var spinnerImage = spinner.gameObject.AddComponent<Image>();
            spinnerImage.color = TextPrimary;
            // Note: You'd assign a spinner sprite here

            // Loading Text
            var loadingTextGO = CreateUIElement("LoadingText", loading);
            loadingTextGO.anchorMin = new Vector2(0.5f, 0.5f);
            loadingTextGO.anchorMax = new Vector2(0.5f, 0.5f);
            loadingTextGO.sizeDelta = new Vector2(200, 30);
            loadingTextGO.anchoredPosition = new Vector2(0, -50);

            var loadingText = loadingTextGO.gameObject.AddComponent<TextMeshProUGUI>();
            loadingText.text = "Loading...";
            loadingText.fontSize = 16;
            loadingText.color = TextPrimary;
            loadingText.alignment = TextAlignmentOptions.Center;
        }

        private static void CreateToast(RectTransform parent)
        {
            var toast = CreateUIElement("Toast", parent);
            toast.anchorMin = new Vector2(0.5f, 0);
            toast.anchorMax = new Vector2(0.5f, 0);
            toast.pivot = new Vector2(0.5f, 0);
            toast.sizeDelta = new Vector2(300, 50);
            toast.anchoredPosition = new Vector2(0, 100);
            toast.gameObject.SetActive(false);

            var toastBg = toast.gameObject.AddComponent<Image>();
            toastBg.color = ToastBackground;
            AddRoundedCorners(toast.gameObject, 25f);

            var toastTextGO = CreateUIElement("Text", toast);
            StretchToFill(toastTextGO);
            toastTextGO.offsetMin = new Vector2(20, 5);
            toastTextGO.offsetMax = new Vector2(-20, -5);

            var toastText = toastTextGO.gameObject.AddComponent<TextMeshProUGUI>();
            toastText.text = "Toast message";
            toastText.fontSize = 14;
            toastText.color = TextPrimary;
            toastText.alignment = TextAlignmentOptions.Center;
        }

        private static void CreateConfirmDialog(RectTransform parent)
        {
            var dialog = CreateUIElement("ConfirmDialog", parent);
            StretchToFill(dialog);
            dialog.gameObject.SetActive(false);

            var dialogBg = dialog.gameObject.AddComponent<Image>();
            dialogBg.color = OverlayColor;

            // Dialog Box
            var box = CreateUIElement("DialogBox", dialog);
            box.anchorMin = new Vector2(0.5f, 0.5f);
            box.anchorMax = new Vector2(0.5f, 0.5f);
            box.sizeDelta = new Vector2(350, 200);

            var boxBg = box.gameObject.AddComponent<Image>();
            boxBg.color = PanelBackground;
            AddRoundedCorners(box.gameObject, 15f);

            var boxLayout = box.gameObject.AddComponent<VerticalLayoutGroup>();
            boxLayout.padding = new RectOffset(20, 20, 20, 20);
            boxLayout.spacing = 15;
            boxLayout.childControlHeight = false;
            boxLayout.childControlWidth = true;

            // Title
            var titleGO = CreateUIElement("Title", box);
            titleGO.sizeDelta = new Vector2(0, 30);
            var titleText = titleGO.gameObject.AddComponent<TextMeshProUGUI>();
            titleText.text = "Confirm";
            titleText.fontSize = 20;
            titleText.fontStyle = FontStyles.Bold;
            titleText.color = TextPrimary;
            titleText.alignment = TextAlignmentOptions.Center;

            // Message
            var messageGO = CreateUIElement("Message", box);
            messageGO.sizeDelta = new Vector2(0, 60);
            var messageText = messageGO.gameObject.AddComponent<TextMeshProUGUI>();
            messageText.text = "Are you sure?";
            messageText.fontSize = 16;
            messageText.color = TextSecondary;
            messageText.alignment = TextAlignmentOptions.Center;

            // Buttons
            var buttonsGO = CreateUIElement("Buttons", box);
            buttonsGO.sizeDelta = new Vector2(0, 45);

            var buttonsLayout = buttonsGO.gameObject.AddComponent<HorizontalLayoutGroup>();
            buttonsLayout.spacing = 15;
            buttonsLayout.childControlHeight = true;
            buttonsLayout.childControlWidth = true;
            buttonsLayout.childForceExpandWidth = true;

            var noBtn = CreateButton("NoButton", buttonsGO, "Cancel", 0, 45);
            var noBtnImage = noBtn.GetComponent<Image>();
            noBtnImage.color = TabInactiveColor;

            var yesBtn = CreateButton("YesButton", buttonsGO, "Confirm", 0, 45);
            var yesBtnImage = yesBtn.GetComponent<Image>();
            yesBtnImage.color = RejectButtonColor;
        }

        #endregion

        #region Slot Creation

        private static GameObject CreateFriendSlotUI(string name)
        {
            var slotGO = new GameObject(name);
            var slotRect = slotGO.AddComponent<RectTransform>();
            slotRect.sizeDelta = new Vector2(0, 80);

            slotGO.AddComponent<CanvasGroup>();

            var slotBg = slotGO.AddComponent<Image>();
            slotBg.color = SlotBackground;
            AddRoundedCorners(slotGO, 10f);

            var slotLayout = slotGO.AddComponent<HorizontalLayoutGroup>();
            slotLayout.padding = new RectOffset(15, 15, 10, 10);
            slotLayout.spacing = 12;
            slotLayout.childAlignment = TextAnchor.MiddleLeft;
            slotLayout.childControlHeight = false;
            slotLayout.childControlWidth = false;

            // Avatar with status indicator
            var avatarContainer = CreateUIElement("AvatarContainer", slotRect);
            avatarContainer.sizeDelta = new Vector2(60, 60);

            var avatar = CreateUIElement("Avatar", avatarContainer);
            StretchToFill(avatar);
            var avatarImage = avatar.gameObject.AddComponent<Image>();
            avatarImage.color = TabInactiveColor;
            AddRoundedCorners(avatar.gameObject, 30f);

            // Online status indicator
            var status = CreateUIElement("StatusIndicator", avatarContainer);
            status.anchorMin = new Vector2(1, 0);
            status.anchorMax = new Vector2(1, 0);
            status.pivot = new Vector2(1, 0);
            status.sizeDelta = new Vector2(16, 16);
            status.anchoredPosition = new Vector2(2, -2);
            var statusImage = status.gameObject.AddComponent<Image>();
            statusImage.color = OnlineColor;
            AddRoundedCorners(status.gameObject, 8f);

            // Info container
            var infoContainer = CreateUIElement("InfoContainer", slotRect);
            infoContainer.sizeDelta = new Vector2(150, 60);
            var infoLayout = infoContainer.gameObject.AddComponent<LayoutElement>();
            infoLayout.flexibleWidth = 1;

            var infoVertLayout = infoContainer.gameObject.AddComponent<VerticalLayoutGroup>();
            infoVertLayout.childControlHeight = true;
            infoVertLayout.childControlWidth = true;
            infoVertLayout.childForceExpandHeight = true;
            infoVertLayout.spacing = 2;

            // Name
            var nameGO = CreateUIElement("NameText", infoContainer);
            var nameText = nameGO.gameObject.AddComponent<TextMeshProUGUI>();
            nameText.text = "Friend Name";
            nameText.fontSize = 18;
            nameText.fontStyle = FontStyles.Bold;
            nameText.color = TextPrimary;
            nameText.alignment = TextAlignmentOptions.Left;

            // Status text
            var statusTextGO = CreateUIElement("StatusText", infoContainer);
            var statusText = statusTextGO.gameObject.AddComponent<TextMeshProUGUI>();
            statusText.text = "Online";
            statusText.fontSize = 14;
            statusText.color = TextSecondary;
            statusText.alignment = TextAlignmentOptions.Left;

            // Actions container
            var actionsContainer = CreateUIElement("ActionsContainer", slotRect);
            actionsContainer.sizeDelta = new Vector2(90, 60);

            var actionsLayout = actionsContainer.gameObject.AddComponent<HorizontalLayoutGroup>();
            actionsLayout.spacing = 8;
            actionsLayout.childAlignment = TextAnchor.MiddleRight;
            actionsLayout.childControlHeight = false;
            actionsLayout.childControlWidth = false;

            // Profile button
            var profileBtn = CreateIconButton("ProfileButton", actionsContainer, "👤", 40, 40);
            var profileBtnImage = profileBtn.GetComponent<Image>();
            profileBtnImage.color = TabInactiveColor;

            // Remove button
            var removeBtn = CreateIconButton("RemoveButton", actionsContainer, "✕", 40, 40);
            var removeBtnImage = removeBtn.GetComponent<Image>();
            removeBtnImage.color = RejectButtonColor;

            return slotGO;
        }

        private static GameObject CreateFriendRequestSlotUI(string name)
        {
            var slotGO = new GameObject(name);
            var slotRect = slotGO.AddComponent<RectTransform>();
            slotRect.sizeDelta = new Vector2(0, 90);

            slotGO.AddComponent<CanvasGroup>();

            var slotBg = slotGO.AddComponent<Image>();
            slotBg.color = SlotBackground;
            AddRoundedCorners(slotGO, 10f);

            var slotLayout = slotGO.AddComponent<HorizontalLayoutGroup>();
            slotLayout.padding = new RectOffset(15, 15, 10, 10);
            slotLayout.spacing = 12;
            slotLayout.childAlignment = TextAnchor.MiddleLeft;
            slotLayout.childControlHeight = false;
            slotLayout.childControlWidth = false;

            // Avatar
            var avatar = CreateUIElement("Avatar", slotRect);
            avatar.sizeDelta = new Vector2(60, 60);
            var avatarImage = avatar.gameObject.AddComponent<Image>();
            avatarImage.color = TabInactiveColor;
            AddRoundedCorners(avatar.gameObject, 30f);

            // Info container
            var infoContainer = CreateUIElement("InfoContainer", slotRect);
            infoContainer.sizeDelta = new Vector2(120, 70);
            var infoLayout = infoContainer.gameObject.AddComponent<LayoutElement>();
            infoLayout.flexibleWidth = 1;

            var infoVertLayout = infoContainer.gameObject.AddComponent<VerticalLayoutGroup>();
            infoVertLayout.childControlHeight = true;
            infoVertLayout.childControlWidth = true;
            infoVertLayout.childForceExpandHeight = true;
            infoVertLayout.spacing = 2;

            // Name
            var nameGO = CreateUIElement("NameText", infoContainer);
            var nameText = nameGO.gameObject.AddComponent<TextMeshProUGUI>();
            nameText.text = "User Name";
            nameText.fontSize = 18;
            nameText.fontStyle = FontStyles.Bold;
            nameText.color = TextPrimary;
            nameText.alignment = TextAlignmentOptions.Left;

            // Time text
            var timeGO = CreateUIElement("TimeText", infoContainer);
            var timeText = timeGO.gameObject.AddComponent<TextMeshProUGUI>();
            timeText.text = "2h ago";
            timeText.fontSize = 13;
            timeText.color = TextSecondary;
            timeText.alignment = TextAlignmentOptions.Left;

            // Message text
            var messageGO = CreateUIElement("MessageText", infoContainer);
            var messageText = messageGO.gameObject.AddComponent<TextMeshProUGUI>();
            messageText.text = "";
            messageText.fontSize = 13;
            messageText.fontStyle = FontStyles.Italic;
            messageText.color = TextSecondary;
            messageText.alignment = TextAlignmentOptions.Left;
            messageGO.gameObject.SetActive(false);

            // Buttons container
            var buttonsContainer = CreateUIElement("ButtonsContainer", slotRect);
            buttonsContainer.sizeDelta = new Vector2(110, 70);

            var buttonsLayout = buttonsContainer.gameObject.AddComponent<VerticalLayoutGroup>();
            buttonsLayout.spacing = 5;
            buttonsLayout.childAlignment = TextAnchor.MiddleCenter;
            buttonsLayout.childControlHeight = false;
            buttonsLayout.childControlWidth = true;

            // Accept button
            var acceptBtn = CreateButton("AcceptButton", buttonsContainer, "Accept", 0, 32);
            var acceptBtnImage = acceptBtn.GetComponent<Image>();
            acceptBtnImage.color = AcceptButtonColor;

            // Reject button
            var rejectBtn = CreateButton("RejectButton", buttonsContainer, "Decline", 0, 32);
            var rejectBtnImage = rejectBtn.GetComponent<Image>();
            rejectBtnImage.color = RejectButtonColor;

            // Loading indicator
            var loadingGO = CreateUIElement("LoadingIndicator", slotRect);
            loadingGO.sizeDelta = new Vector2(30, 30);
            var loadingImage = loadingGO.gameObject.AddComponent<Image>();
            loadingImage.color = TextPrimary;
            loadingGO.gameObject.SetActive(false);

            return slotGO;
        }

        private static GameObject CreateSearchSlotUI(string name)
        {
            var slotGO = new GameObject(name);
            var slotRect = slotGO.AddComponent<RectTransform>();
            slotRect.sizeDelta = new Vector2(0, 80);

            slotGO.AddComponent<CanvasGroup>();

            var slotBg = slotGO.AddComponent<Image>();
            slotBg.color = SlotBackground;
            AddRoundedCorners(slotGO, 10f);

            var slotLayout = slotGO.AddComponent<HorizontalLayoutGroup>();
            slotLayout.padding = new RectOffset(15, 15, 10, 10);
            slotLayout.spacing = 12;
            slotLayout.childAlignment = TextAnchor.MiddleLeft;
            slotLayout.childControlHeight = false;
            slotLayout.childControlWidth = false;

            // Avatar
            var avatar = CreateUIElement("Avatar", slotRect);
            avatar.sizeDelta = new Vector2(60, 60);
            var avatarImage = avatar.gameObject.AddComponent<Image>();
            avatarImage.color = TabInactiveColor;
            AddRoundedCorners(avatar.gameObject, 30f);

            // Info container
            var infoContainer = CreateUIElement("InfoContainer", slotRect);
            infoContainer.sizeDelta = new Vector2(150, 60);
            var infoLayout = infoContainer.gameObject.AddComponent<LayoutElement>();
            infoLayout.flexibleWidth = 1;

            var infoVertLayout = infoContainer.gameObject.AddComponent<VerticalLayoutGroup>();
            infoVertLayout.childControlHeight = true;
            infoVertLayout.childControlWidth = true;
            infoVertLayout.childForceExpandHeight = true;
            infoVertLayout.spacing = 2;

            // Name
            var nameGO = CreateUIElement("NameText", infoContainer);
            var nameText = nameGO.gameObject.AddComponent<TextMeshProUGUI>();
            nameText.text = "User Name";
            nameText.fontSize = 18;
            nameText.fontStyle = FontStyles.Bold;
            nameText.color = TextPrimary;
            nameText.alignment = TextAlignmentOptions.Left;

            // Status text
            var statusGO = CreateUIElement("StatusText", infoContainer);
            var statusText = statusGO.gameObject.AddComponent<TextMeshProUGUI>();
            statusText.text = "";
            statusText.fontSize = 14;
            statusText.color = TextSecondary;
            statusText.alignment = TextAlignmentOptions.Left;
            statusGO.gameObject.SetActive(false);

            // Add button
            var addBtn = CreateButton("AddButton", slotRect, "Add", 80, 40);
            var addBtnImage = addBtn.GetComponent<Image>();
            addBtnImage.color = AddButtonColor;

            // Loading indicator
            var loadingGO = CreateUIElement("LoadingIndicator", slotRect);
            loadingGO.sizeDelta = new Vector2(30, 30);
            var loadingImage = loadingGO.gameObject.AddComponent<Image>();
            loadingImage.color = TextPrimary;
            loadingGO.gameObject.SetActive(false);

            return slotGO;
        }

        #endregion

        #region Wiring References

        private static void WireUpPanelReferences(IVXFriendsPanel panel, GameObject panelGO)
        {
            var serializedObject = new SerializedObject(panel);

            // Panel Root
            SetSerializedField(serializedObject, "panelRoot", panelGO);
            SetSerializedField(serializedObject, "panelCanvasGroup", panelGO.GetComponent<CanvasGroup>());
            SetSerializedField(serializedObject, "panelRectTransform", panelGO.GetComponent<RectTransform>());

            // Find and set all references
            var header = panelGO.transform.Find("PanelContainer/Header");
            if (header != null)
            {
                SetSerializedField(serializedObject, "closeButton", header.Find("CloseButton")?.GetComponent<Button>());
            }

            var tabBar = panelGO.transform.Find("PanelContainer/TabBar");
            if (tabBar != null)
            {
                SetSerializedField(serializedObject, "friendsTabButton", tabBar.Find("FriendsTab")?.GetComponent<Button>());
                SetSerializedField(serializedObject, "requestsTabButton", tabBar.Find("RequestsTab")?.GetComponent<Button>());
                SetSerializedField(serializedObject, "searchTabButton", tabBar.Find("SearchTab")?.GetComponent<Button>());

                SetSerializedField(serializedObject, "friendsTabIndicator", tabBar.Find("FriendsTab/Indicator")?.gameObject);
                SetSerializedField(serializedObject, "requestsTabIndicator", tabBar.Find("RequestsTab/Indicator")?.gameObject);
                SetSerializedField(serializedObject, "searchTabIndicator", tabBar.Find("SearchTab/Indicator")?.gameObject);

                var requestsBadge = tabBar.Find("RequestsTab/Badge");
                if (requestsBadge != null)
                {
                    SetSerializedField(serializedObject, "requestsBadge", requestsBadge.gameObject);
                    SetSerializedField(serializedObject, "requestsBadgeText", requestsBadge.Find("Text")?.GetComponent<TextMeshProUGUI>());
                }
            }

            var contentArea = panelGO.transform.Find("PanelContainer/ContentArea");
            if (contentArea != null)
            {
                // Friends Content
                var friendsContent = contentArea.Find("FriendsContent");
                if (friendsContent != null)
                {
                    SetSerializedField(serializedObject, "friendsContent", friendsContent.gameObject);
                    SetSerializedField(serializedObject, "friendsContentCanvasGroup", friendsContent.GetComponent<CanvasGroup>());
                    SetSerializedField(serializedObject, "friendsCountText", friendsContent.Find("CountText")?.GetComponent<TextMeshProUGUI>());
                    SetSerializedField(serializedObject, "friendsEmptyText", friendsContent.Find("EmptyText")?.GetComponent<TextMeshProUGUI>());
                    SetSerializedField(serializedObject, "friendsListContainer", friendsContent.Find("FriendsScrollView/Viewport/FriendsListContainer"));
                }

                // Requests Content
                var requestsContent = contentArea.Find("RequestsContent");
                if (requestsContent != null)
                {
                    SetSerializedField(serializedObject, "requestsContent", requestsContent.gameObject);
                    SetSerializedField(serializedObject, "requestsContentCanvasGroup", requestsContent.GetComponent<CanvasGroup>());
                    SetSerializedField(serializedObject, "requestsCountText", requestsContent.Find("CountText")?.GetComponent<TextMeshProUGUI>());
                    SetSerializedField(serializedObject, "requestsEmptyText", requestsContent.Find("EmptyText")?.GetComponent<TextMeshProUGUI>());
                    SetSerializedField(serializedObject, "requestsListContainer", requestsContent.Find("RequestsScrollView/Viewport/RequestsListContainer"));
                }

                // Search Content
                var searchContent = contentArea.Find("SearchContent");
                if (searchContent != null)
                {
                    SetSerializedField(serializedObject, "searchContent", searchContent.gameObject);
                    SetSerializedField(serializedObject, "searchContentCanvasGroup", searchContent.GetComponent<CanvasGroup>());
                    SetSerializedField(serializedObject, "searchResultsText", searchContent.Find("ResultsText")?.GetComponent<TextMeshProUGUI>());
                    SetSerializedField(serializedObject, "searchEmptyText", searchContent.Find("EmptyText")?.GetComponent<TextMeshProUGUI>());
                    SetSerializedField(serializedObject, "searchInstructions", searchContent.Find("Instructions")?.gameObject);
                    SetSerializedField(serializedObject, "searchResultsContainer", searchContent.Find("SearchScrollView/Viewport/SearchResultsContainer"));

                    var searchBar = searchContent.Find("SearchBar");
                    if (searchBar != null)
                    {
                        SetSerializedField(serializedObject, "searchInput", searchBar.Find("SearchInput")?.GetComponent<TMP_InputField>());
                        SetSerializedField(serializedObject, "searchButton", searchBar.Find("SearchButton")?.GetComponent<Button>());
                    }
                }
            }

            // Loading Overlay
            var loading = panelGO.transform.Find("PanelContainer/LoadingOverlay");
            if (loading != null)
            {
                SetSerializedField(serializedObject, "loadingOverlay", loading.gameObject);
                SetSerializedField(serializedObject, "loadingSpinner", loading.Find("Spinner")?.GetComponent<RectTransform>());
                SetSerializedField(serializedObject, "loadingText", loading.Find("LoadingText")?.GetComponent<TextMeshProUGUI>());
            }

            // Toast
            var toast = panelGO.transform.Find("Toast");
            if (toast != null)
            {
                SetSerializedField(serializedObject, "toastPanel", toast.gameObject);
                SetSerializedField(serializedObject, "toastText", toast.Find("Text")?.GetComponent<TextMeshProUGUI>());
            }

            // Confirm Dialog
            var confirm = panelGO.transform.Find("ConfirmDialog");
            if (confirm != null)
            {
                SetSerializedField(serializedObject, "confirmDialog", confirm.gameObject);
                SetSerializedField(serializedObject, "confirmTitleText", confirm.Find("DialogBox/Title")?.GetComponent<TextMeshProUGUI>());
                SetSerializedField(serializedObject, "confirmMessageText", confirm.Find("DialogBox/Message")?.GetComponent<TextMeshProUGUI>());
                SetSerializedField(serializedObject, "confirmYesButton", confirm.Find("DialogBox/Buttons/YesButton")?.GetComponent<Button>());
                SetSerializedField(serializedObject, "confirmNoButton", confirm.Find("DialogBox/Buttons/NoButton")?.GetComponent<Button>());
            }

            serializedObject.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void WireUpFriendSlotReferences(GameObject slotGO)
        {
            var slot = slotGO.GetComponent<IVXFriendSlot>();
            var serializedObject = new SerializedObject(slot);

            SetSerializedField(serializedObject, "avatarImage", slotGO.transform.Find("AvatarContainer/Avatar")?.GetComponent<Image>());
            SetSerializedField(serializedObject, "nameText", slotGO.transform.Find("InfoContainer/NameText")?.GetComponent<TextMeshProUGUI>());
            SetSerializedField(serializedObject, "statusText", slotGO.transform.Find("InfoContainer/StatusText")?.GetComponent<TextMeshProUGUI>());
            SetSerializedField(serializedObject, "statusIndicator", slotGO.transform.Find("AvatarContainer/StatusIndicator")?.GetComponent<Image>());
            SetSerializedField(serializedObject, "removeButton", slotGO.transform.Find("ActionsContainer/RemoveButton")?.GetComponent<Button>());
            SetSerializedField(serializedObject, "profileButton", slotGO.transform.Find("ActionsContainer/ProfileButton")?.GetComponent<Button>());
            SetSerializedField(serializedObject, "canvasGroup", slotGO.GetComponent<CanvasGroup>());
            SetSerializedField(serializedObject, "rectTransform", slotGO.GetComponent<RectTransform>());

            serializedObject.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void WireUpRequestSlotReferences(GameObject slotGO)
        {
            var slot = slotGO.GetComponent<IVXFriendRequestSlot>();
            var serializedObject = new SerializedObject(slot);

            SetSerializedField(serializedObject, "avatarImage", slotGO.transform.Find("Avatar")?.GetComponent<Image>());
            SetSerializedField(serializedObject, "nameText", slotGO.transform.Find("InfoContainer/NameText")?.GetComponent<TextMeshProUGUI>());
            SetSerializedField(serializedObject, "timeText", slotGO.transform.Find("InfoContainer/TimeText")?.GetComponent<TextMeshProUGUI>());
            SetSerializedField(serializedObject, "messageText", slotGO.transform.Find("InfoContainer/MessageText")?.GetComponent<TextMeshProUGUI>());
            SetSerializedField(serializedObject, "acceptButton", slotGO.transform.Find("ButtonsContainer/AcceptButton")?.GetComponent<Button>());
            SetSerializedField(serializedObject, "rejectButton", slotGO.transform.Find("ButtonsContainer/RejectButton")?.GetComponent<Button>());
            SetSerializedField(serializedObject, "buttonsContainer", slotGO.transform.Find("ButtonsContainer")?.gameObject);
            SetSerializedField(serializedObject, "loadingIndicator", slotGO.transform.Find("LoadingIndicator")?.gameObject);
            SetSerializedField(serializedObject, "canvasGroup", slotGO.GetComponent<CanvasGroup>());
            SetSerializedField(serializedObject, "rectTransform", slotGO.GetComponent<RectTransform>());

            serializedObject.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void WireUpSearchSlotReferences(GameObject slotGO)
        {
            var slot = slotGO.GetComponent<IVXFriendSearchSlot>();
            var serializedObject = new SerializedObject(slot);

            SetSerializedField(serializedObject, "avatarImage", slotGO.transform.Find("Avatar")?.GetComponent<Image>());
            SetSerializedField(serializedObject, "nameText", slotGO.transform.Find("InfoContainer/NameText")?.GetComponent<TextMeshProUGUI>());
            SetSerializedField(serializedObject, "statusText", slotGO.transform.Find("InfoContainer/StatusText")?.GetComponent<TextMeshProUGUI>());
            SetSerializedField(serializedObject, "addButton", slotGO.transform.Find("AddButton")?.GetComponent<Button>());
            SetSerializedField(serializedObject, "addButtonText", slotGO.transform.Find("AddButton/Text")?.GetComponent<TextMeshProUGUI>());
            SetSerializedField(serializedObject, "addButtonImage", slotGO.transform.Find("AddButton")?.GetComponent<Image>());
            SetSerializedField(serializedObject, "loadingIndicator", slotGO.transform.Find("LoadingIndicator")?.gameObject);
            SetSerializedField(serializedObject, "canvasGroup", slotGO.GetComponent<CanvasGroup>());
            SetSerializedField(serializedObject, "rectTransform", slotGO.GetComponent<RectTransform>());

            serializedObject.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void SetSerializedField(SerializedObject so, string fieldName, Object value)
        {
            var prop = so.FindProperty(fieldName);
            if (prop != null && value != null)
            {
                prop.objectReferenceValue = value;
            }
        }

        #endregion

        #region Utility Methods

        private static RectTransform CreateUIElement(string name, Transform parent)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            return go.AddComponent<RectTransform>();
        }

        private static RectTransform CreateUIElement(string name, RectTransform parent)
        {
            return CreateUIElement(name, (Transform)parent);
        }

        private static void StretchToFill(RectTransform rect)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }

        private static GameObject CreateButton(string name, Transform parent, string text, float width, float height)
        {
            var btnGO = CreateUIElement(name, parent);
            if (width > 0) btnGO.sizeDelta = new Vector2(width, height);
            else
            {
                var layout = btnGO.gameObject.AddComponent<LayoutElement>();
                layout.preferredHeight = height;
            }

            var btnImage = btnGO.gameObject.AddComponent<Image>();
            btnImage.color = TabActiveColor;
            AddRoundedCorners(btnGO.gameObject, 8f);

            var btn = btnGO.gameObject.AddComponent<Button>();
            btn.targetGraphic = btnImage;

            var textGO = CreateUIElement("Text", btnGO);
            StretchToFill(textGO);

            var tmpText = textGO.gameObject.AddComponent<TextMeshProUGUI>();
            tmpText.text = text;
            tmpText.fontSize = 14;
            tmpText.fontStyle = FontStyles.Bold;
            tmpText.color = TextPrimary;
            tmpText.alignment = TextAlignmentOptions.Center;

            return btnGO.gameObject;
        }

        private static GameObject CreateIconButton(string name, Transform parent, string icon, float width, float height)
        {
            var btnGO = CreateUIElement(name, parent);
            btnGO.sizeDelta = new Vector2(width, height);

            var btnImage = btnGO.gameObject.AddComponent<Image>();
            btnImage.color = TabActiveColor;
            AddRoundedCorners(btnGO.gameObject, width / 2f);

            var btn = btnGO.gameObject.AddComponent<Button>();
            btn.targetGraphic = btnImage;

            var textGO = CreateUIElement("Icon", btnGO);
            StretchToFill(textGO);

            var tmpText = textGO.gameObject.AddComponent<TextMeshProUGUI>();
            tmpText.text = icon;
            tmpText.fontSize = 18;
            tmpText.color = TextPrimary;
            tmpText.alignment = TextAlignmentOptions.Center;

            return btnGO.gameObject;
        }

        private static void AddRoundedCorners(GameObject go, float radius)
        {
            // Note: Unity's built-in UI doesn't support rounded corners natively.
            // This is a placeholder - in production, you'd use a custom shader or UI Toolkit.
            // For now, we just mark the intent.
        }

        private static void SavePrefab(GameObject go, string path)
        {
            PrefabUtility.SaveAsPrefabAsset(go, path);
        }

        #endregion
    }
}
#endif
