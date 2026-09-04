using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace IntelliVerseX.Social.UI
{
    /// <summary>
    /// Production-ready clan panel that ensures visible uGUI components exist on
    /// authored placeholder slots. SDK users can replace the default components
    /// with their own styled versions while keeping the hierarchy stable.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class IVXClanPanel : MonoBehaviour
    {
        #region Constants

        private static readonly Color COLOR_BG = new Color(0.08f, 0.10f, 0.14f, 0.97f);
        private static readonly Color COLOR_PANEL = new Color(0.12f, 0.15f, 0.20f, 0.95f);
        private static readonly Color COLOR_INPUT_BG = new Color(0.16f, 0.19f, 0.25f, 1f);
        private static readonly Color COLOR_BTN_PRIMARY = new Color(0.20f, 0.45f, 0.78f, 1f);
        private static readonly Color COLOR_BTN_DANGER = new Color(0.78f, 0.22f, 0.22f, 1f);
        private static readonly Color COLOR_BTN_SECONDARY = new Color(0.28f, 0.32f, 0.40f, 1f);
        private static readonly Color COLOR_TEXT = new Color(0.92f, 0.93f, 0.95f, 1f);
        private static readonly Color COLOR_TEXT_DIM = new Color(0.60f, 0.63f, 0.68f, 1f);
        private static readonly Color COLOR_TOAST = new Color(0.14f, 0.17f, 0.22f, 0.96f);

        #endregion

        #region Serialized Fields

        [Header("Scene Roots")]
        [SerializeField] private Transform _safeAreaRoot;
        [SerializeField] private Transform _backgroundRoot;
        [SerializeField] private Transform _headerRoot;
        [SerializeField] private Transform _statusPanelRoot;
        [SerializeField] private Transform _contentRoot;
        [SerializeField] private Transform _currentClanPanelRoot;
        [SerializeField] private Transform _createClanPanelRoot;
        [SerializeField] private Transform _browseClanPanelRoot;
        [SerializeField] private Transform _membersPanelRoot;
        [SerializeField] private Transform _emptyStatePanelRoot;
        [SerializeField] private Transform _footerRoot;
        [SerializeField] private Transform _modalRoot;
        [SerializeField] private Transform _toastRoot;

        [Header("Placeholder Slots")]
        [SerializeField] private Transform _titleSlot;
        [SerializeField] private Transform _statusTextSlot;
        [SerializeField] private Transform _currentClanSummarySlot;
        [SerializeField] private Transform _currentClanDetailsSlot;
        [SerializeField] private Transform _refreshButtonSlot;
        [SerializeField] private Transform _leaveButtonSlot;
        [SerializeField] private Transform _createNameInputSlot;
        [SerializeField] private Transform _createDescriptionInputSlot;
        [SerializeField] private Transform _createOpenToggleSlot;
        [SerializeField] private Transform _createButtonSlot;
        [SerializeField] private Transform _searchInputSlot;
        [SerializeField] private Transform _searchButtonSlot;
        [SerializeField] private Transform _browseSummarySlot;
        [SerializeField] private Transform _browseResultsContainer;
        [SerializeField] private Transform _membersContainer;
        [SerializeField] private Transform _emptyStateTextSlot;
        [SerializeField] private Transform _primaryFooterButtonSlot;
        [SerializeField] private Transform _secondaryFooterButtonSlot;
        [SerializeField] private Transform _toastTextSlot;

        #endregion

        #region Private Fields

        private IVXClanSceneController _controller;
        private string _searchQuery = string.Empty;
        private string _createClanName = string.Empty;
        private string _createClanDescription = string.Empty;
        private bool _createClanOpen = true;
        private bool _eventsHooked;
        private bool _slotsEnsured;

        private TMP_Text _titleText;
        private TMP_Text _statusText;
        private TMP_Text _currentClanSummaryText;
        private TMP_Text _currentClanDetailsText;
        private Button _refreshButton;
        private Button _leaveButton;
        private TMP_InputField _createNameInput;
        private TMP_InputField _createDescriptionInput;
        private Toggle _createOpenToggle;
        private Button _createButton;
        private TMP_InputField _searchInput;
        private Button _searchButton;
        private TMP_Text _browseSummaryText;
        private TMP_Text _emptyStateText;
        private Button _primaryFooterButton;
        private Button _secondaryFooterButton;
        private TMP_Text _toastText;
        private readonly List<GameObject> _generatedRows = new List<GameObject>();

        #endregion

        #region Public Methods

        /// <summary>
        /// Initializes the panel against the scene controller.
        /// </summary>
        public void Initialize(IVXClanSceneController controller)
        {
            _controller = controller;
            AutoWireReferences();
            EnsureSlotComponents();
            ResolveUiReferences();
            HookEvents();
            RebuildLayout();
            RefreshView();
        }

        /// <summary>
        /// Refreshes the currently wired UI state from controller data.
        /// </summary>
        public void RefreshView()
        {
            if (!_slotsEnsured)
            {
                AutoWireReferences();
                EnsureSlotComponents();
                ResolveUiReferences();
                RebuildLayout();
            }

            bool isReady = _controller != null;
            bool isBusy = isReady && _controller.IsBusy;
            IVXClanManager clanManager = isReady ? _controller.ClanManager : null;
            IVXClanData currentClan = clanManager != null ? clanManager.CurrentClan : null;
            IReadOnlyList<IVXClanMemberData> members = clanManager != null ? clanManager.Members : Array.Empty<IVXClanMemberData>();
            IReadOnlyList<IVXClanData> browseResults = isReady ? _controller.BrowseResults : Array.Empty<IVXClanData>();
            string status = isReady ? (_controller.StatusMessage ?? string.Empty) : "Clan controller not connected.";

            bool showNotReadyState = !isReady || status.IndexOf("not ready", StringComparison.OrdinalIgnoreCase) >= 0;
            bool hasClan = currentClan != null;

            SetActive(_currentClanPanelRoot, hasClan);
            SetActive(_membersPanelRoot, hasClan);
            SetActive(_createClanPanelRoot, true);
            SetActive(_browseClanPanelRoot, true);
            SetActive(_emptyStatePanelRoot, showNotReadyState || (!hasClan && browseResults.Count == 0));
            SetActive(_toastRoot, isBusy || showNotReadyState);

            SetText(_titleText, "IVX Clan");
            SetText(_statusText, status);
            if (showNotReadyState)
            {
                SetText(_browseSummaryText, "Browse is disabled until authentication is ready.");
            }
            else if (hasClan)
            {
                SetText(_browseSummaryText, $"Results: {browseResults.Count} (leave current clan to join another)");
            }
            else
            {
                SetText(_browseSummaryText, $"Results: {browseResults.Count}");
            }

            if (hasClan)
            {
                SetText(_currentClanSummaryText, $"{currentClan.Name} ({currentClan.MemberCount}/{currentClan.MaxMembers})");
                SetText(
                    _currentClanDetailsText,
                    $"Role: {NullSafe(currentClan.UserRole)}\n" +
                    $"Level: {currentClan.Level}\n" +
                    $"XP: {currentClan.Experience}\n" +
                    $"Description: {NullSafe(currentClan.Description, "No description set.")}");
            }
            else
            {
                SetText(_currentClanSummaryText, "No clan joined.");
                SetText(_currentClanDetailsText, "Create a clan or join an existing one below.");
            }

            SyncInputWithoutNotify(_createNameInput, _createClanName);
            SyncInputWithoutNotify(_createDescriptionInput, _createClanDescription);
            SyncInputWithoutNotify(_searchInput, _searchQuery);

            if (_createOpenToggle != null)
            {
                _createOpenToggle.SetIsOnWithoutNotify(_createClanOpen);
            }

            PopulateMembers(members);
            PopulateBrowseResults(browseResults, isBusy, hasClan, showNotReadyState);

            string emptyStateMessage;
            if (showNotReadyState)
            {
                emptyStateMessage = "Nakama is not ready yet.\nLog in from IVX_AuthTest, then reopen this scene.";
            }
            else if (!hasClan && browseResults.Count == 0)
            {
                emptyStateMessage = "No clan joined yet.\nSearch for a clan or create your own.";
            }
            else
            {
                emptyStateMessage = string.Empty;
            }

            SetText(_emptyStateText, emptyStateMessage);
            SetText(_toastText, isBusy ? status : (showNotReadyState ? "Waiting for authentication..." : string.Empty));

            SetInteractable(_createNameInput, !isBusy && !showNotReadyState && !hasClan);
            SetInteractable(_createDescriptionInput, !isBusy && !showNotReadyState && !hasClan);
            SetInteractable(_createOpenToggle, !isBusy && !showNotReadyState && !hasClan);
            SetInteractable(_searchInput, !isBusy && !showNotReadyState && !hasClan);
            SetInteractable(_createButton, !isBusy && !showNotReadyState && !hasClan && !string.IsNullOrWhiteSpace(_createClanName));
            SetInteractable(_refreshButton, !isBusy && hasClan);
            SetInteractable(_leaveButton, !isBusy && hasClan);
            SetInteractable(_searchButton, !isBusy && !showNotReadyState && !hasClan);
            SetInteractable(_primaryFooterButton, !isBusy && !showNotReadyState);
            SetInteractable(_secondaryFooterButton, !isBusy);

            if (_primaryFooterButton != null)
            {
                SetButtonLabel(_primaryFooterButton, hasClan ? "Refresh Clan" : "Search Clans");
            }

            if (_secondaryFooterButton != null)
            {
                SetButtonLabel(_secondaryFooterButton, hasClan ? "Leave Clan" : "Create Clan");
            }

            RebuildLayout();
        }

        #endregion

        #region Unity Lifecycle

        private void OnDestroy()
        {
            RemoveEvents();
            ClearAllGeneratedRows();
        }

        #endregion

        #region Slot Bootstrapping

        private void EnsureSlotComponents()
        {
            if (_slotsEnsured)
            {
                return;
            }

            _slotsEnsured = true;

            RectTransform selfRect = EnsureRectTransform(transform);
            Stretch(selfRect);
            EnsureCanvasGroup(gameObject);

            EnsureBackgroundImage(_backgroundRoot, COLOR_BG);
            EnsureSectionLayout(_safeAreaRoot, 12f, new RectOffset(20, 20, 20, 20));
            EnsureSectionLayout(_headerRoot, 8f, new RectOffset(0, 0, 0, 0));
            EnsureSectionLayout(_statusPanelRoot, 4f, new RectOffset(12, 12, 8, 8), COLOR_PANEL);
            EnsureSectionLayout(_contentRoot, 12f, new RectOffset(0, 0, 0, 0));
            EnsureSectionLayout(_currentClanPanelRoot, 8f, new RectOffset(16, 16, 12, 12), COLOR_PANEL);
            EnsureSectionLayout(_createClanPanelRoot, 10f, new RectOffset(16, 16, 12, 12), COLOR_PANEL);
            EnsureSectionLayout(_browseClanPanelRoot, 8f, new RectOffset(16, 16, 12, 12), COLOR_PANEL);
            EnsureSectionLayout(_membersPanelRoot, 8f, new RectOffset(16, 16, 12, 12), COLOR_PANEL);
            EnsureSectionLayout(_emptyStatePanelRoot, 8f, new RectOffset(24, 24, 40, 40), COLOR_PANEL);
            EnsureHorizontalLayout(_footerRoot, 12f, new RectOffset(0, 0, 8, 0));
            EnsureSectionLayout(_toastRoot, 4f, new RectOffset(16, 16, 10, 10), COLOR_TOAST);

            _titleText = EnsureTextOnSlot(_titleSlot, "IVX Clan", 34f, FontStyles.Bold, COLOR_TEXT);
            _statusText = EnsureTextOnSlot(_statusTextSlot, "Initializing...", 20f, FontStyles.Normal, COLOR_TEXT_DIM);
            _currentClanSummaryText = EnsureTextOnSlot(_currentClanSummarySlot, "No clan joined.", 26f, FontStyles.Bold, COLOR_TEXT);
            _currentClanDetailsText = EnsureTextOnSlot(_currentClanDetailsSlot, "", 20f, FontStyles.Normal, COLOR_TEXT_DIM);
            _browseSummaryText = EnsureTextOnSlot(_browseSummarySlot, "Results: 0", 20f, FontStyles.Normal, COLOR_TEXT_DIM);
            _emptyStateText = EnsureTextOnSlot(_emptyStateTextSlot, "", 22f, FontStyles.Normal, COLOR_TEXT_DIM, TextAlignmentOptions.Center);
            _toastText = EnsureTextOnSlot(_toastTextSlot, "", 18f, FontStyles.Italic, COLOR_TEXT_DIM, TextAlignmentOptions.Center);

            _refreshButton = EnsureButtonOnSlot(_refreshButtonSlot, "Refresh", COLOR_BTN_PRIMARY);
            _leaveButton = EnsureButtonOnSlot(_leaveButtonSlot, "Leave Clan", COLOR_BTN_DANGER);
            _createButton = EnsureButtonOnSlot(_createButtonSlot, "Create Clan", COLOR_BTN_PRIMARY);
            _searchButton = EnsureButtonOnSlot(_searchButtonSlot, "Search", COLOR_BTN_PRIMARY);
            _primaryFooterButton = EnsureButtonOnSlot(_primaryFooterButtonSlot, "Search Clans", COLOR_BTN_PRIMARY);
            _secondaryFooterButton = EnsureButtonOnSlot(_secondaryFooterButtonSlot, "Create Clan", COLOR_BTN_SECONDARY);

            _createNameInput = EnsureInputOnSlot(_createNameInputSlot, "Clan name...", false);
            _createDescriptionInput = EnsureInputOnSlot(_createDescriptionInputSlot, "Description (optional)...", true);
            _searchInput = EnsureInputOnSlot(_searchInputSlot, "Search clans...", false);

            _createOpenToggle = EnsureToggleOnSlot(_createOpenToggleSlot, "Open (anyone can join)");

            EnsureScrollContainer(_browseResultsContainer);
            EnsureScrollContainer(_membersContainer);
        }

        private TMP_Text EnsureTextOnSlot(Transform slot, string defaultText, float fontSize, FontStyles style, Color color, TextAlignmentOptions alignment = TextAlignmentOptions.TopLeft)
        {
            if (slot == null) return null;

            RectTransform rect = EnsureRectTransform(slot);
            if (rect == null) return null;
            GameObject go = rect.gameObject;
            EnsureLayoutElement(go, fontSize + 12f);

            TextMeshProUGUI tmp = go.GetComponent<TextMeshProUGUI>();
            if (tmp == null) tmp = go.GetComponentInChildren<TextMeshProUGUI>(true);
            if (tmp == null) tmp = go.AddComponent<TextMeshProUGUI>();

            tmp.text = defaultText;
            tmp.fontSize = fontSize;
            tmp.fontStyle = style;
            tmp.color = color;
            tmp.alignment = alignment;
            tmp.textWrappingMode = TextWrappingModes.Normal;
            tmp.overflowMode = TextOverflowModes.Ellipsis;
            tmp.raycastTarget = false;
            return tmp;
        }

        private Button EnsureButtonOnSlot(Transform slot, string label, Color bgColor)
        {
            if (slot == null) return null;

            RectTransform rect = EnsureRectTransform(slot);
            if (rect == null) return null;
            GameObject go = rect.gameObject;
            EnsureLayoutElement(go, 52f);

            Image image = go.GetComponent<Image>();
            if (image == null) image = go.AddComponent<Image>();
            image.color = bgColor;
            image.type = Image.Type.Sliced;

            Button button = go.GetComponent<Button>();
            if (button == null) button = go.AddComponent<Button>();
            button.targetGraphic = image;

            ColorBlock cb = button.colors;
            cb.normalColor = Color.white;
            cb.highlightedColor = new Color(0.9f, 0.9f, 0.9f, 1f);
            cb.pressedColor = new Color(0.7f, 0.7f, 0.7f, 1f);
            cb.disabledColor = new Color(0.5f, 0.5f, 0.5f, 0.6f);
            button.colors = cb;

            TextMeshProUGUI btnText = go.GetComponentInChildren<TextMeshProUGUI>(true);
            if (btnText == null)
            {
                GameObject textChild = new GameObject("Label", typeof(RectTransform));
                textChild.transform.SetParent(rect, false);
                Stretch(textChild.GetComponent<RectTransform>(), 8f);
                btnText = textChild.AddComponent<TextMeshProUGUI>();
            }

            btnText.text = label;
            btnText.fontSize = 22f;
            btnText.fontStyle = FontStyles.Bold;
            btnText.color = COLOR_TEXT;
            btnText.alignment = TextAlignmentOptions.Center;
            btnText.raycastTarget = false;
            return button;
        }

        private TMP_InputField EnsureInputOnSlot(Transform slot, string placeholder, bool multiline)
        {
            if (slot == null) return null;

            RectTransform rect = EnsureRectTransform(slot);
            if (rect == null) return null;
            GameObject go = rect.gameObject;
            EnsureLayoutElement(go, multiline ? 100f : 52f);

            Image bg = go.GetComponent<Image>();
            if (bg == null) bg = go.AddComponent<Image>();
            bg.color = COLOR_INPUT_BG;

            TMP_InputField input = go.GetComponent<TMP_InputField>();
            if (input == null) input = go.AddComponent<TMP_InputField>();

            RectTransform textArea = FindOrCreateChild(rect, "TextArea");
            Stretch(textArea, 10f);
            textArea.gameObject.AddComponentIfMissing<RectMask2D>();

            RectTransform textRect = FindOrCreateChild(textArea, "Text");
            Stretch(textRect);
            TextMeshProUGUI textComp = textRect.GetComponent<TextMeshProUGUI>();
            if (textComp == null) textComp = textRect.gameObject.AddComponent<TextMeshProUGUI>();
            textComp.fontSize = 20f;
            textComp.color = COLOR_TEXT;
            textComp.alignment = multiline ? TextAlignmentOptions.TopLeft : TextAlignmentOptions.MidlineLeft;
            textComp.textWrappingMode = TextWrappingModes.Normal;
            textComp.raycastTarget = false;

            RectTransform phRect = FindOrCreateChild(textArea, "Placeholder");
            Stretch(phRect);
            TextMeshProUGUI phComp = phRect.GetComponent<TextMeshProUGUI>();
            if (phComp == null) phComp = phRect.gameObject.AddComponent<TextMeshProUGUI>();
            phComp.text = placeholder;
            phComp.fontSize = 20f;
            phComp.fontStyle = FontStyles.Italic;
            phComp.color = COLOR_TEXT_DIM;
            phComp.alignment = multiline ? TextAlignmentOptions.TopLeft : TextAlignmentOptions.MidlineLeft;
            phComp.textWrappingMode = TextWrappingModes.Normal;
            phComp.raycastTarget = false;

            input.textViewport = textArea;
            input.textComponent = textComp;
            input.placeholder = phComp;
            input.lineType = multiline ? TMP_InputField.LineType.MultiLineNewline : TMP_InputField.LineType.SingleLine;
            input.transition = Selectable.Transition.ColorTint;
            input.targetGraphic = bg;
            return input;
        }

        private Toggle EnsureToggleOnSlot(Transform slot, string label)
        {
            if (slot == null) return null;

            RectTransform rect = EnsureRectTransform(slot);
            if (rect == null) return null;
            GameObject go = rect.gameObject;
            EnsureLayoutElement(go, 40f);

            HorizontalLayoutGroup hlg = go.GetComponent<HorizontalLayoutGroup>();
            if (hlg == null) hlg = go.AddComponent<HorizontalLayoutGroup>();
            hlg.spacing = 12f;
            hlg.childAlignment = TextAnchor.MiddleLeft;
            hlg.childControlWidth = false;
            hlg.childControlHeight = false;
            hlg.childForceExpandWidth = false;

            Toggle toggle = go.GetComponent<Toggle>();
            if (toggle == null) toggle = go.AddComponent<Toggle>();

            RectTransform bgRect = FindOrCreateChild(rect, "ToggleBG");
            bgRect.sizeDelta = new Vector2(30f, 30f);
            Image bgImage = bgRect.GetComponent<Image>();
            if (bgImage == null) bgImage = bgRect.gameObject.AddComponent<Image>();
            bgImage.color = COLOR_INPUT_BG;

            RectTransform checkRect = FindOrCreateChild(bgRect, "Checkmark");
            Stretch(checkRect, 5f);
            Image checkImage = checkRect.GetComponent<Image>();
            if (checkImage == null) checkImage = checkRect.gameObject.AddComponent<Image>();
            checkImage.color = new Color(0.25f, 0.85f, 0.50f, 1f);

            toggle.targetGraphic = bgImage;
            toggle.graphic = checkImage;
            toggle.isOn = true;

            TextMeshProUGUI labelText = go.GetComponentInChildren<TextMeshProUGUI>(true);
            if (labelText == null || labelText.transform.parent != rect)
            {
                GameObject labelObj = new GameObject("Label", typeof(RectTransform));
                labelObj.transform.SetParent(rect, false);
                labelText = labelObj.AddComponent<TextMeshProUGUI>();
            }

            labelText.text = label;
            labelText.fontSize = 20f;
            labelText.color = COLOR_TEXT;
            labelText.alignment = TextAlignmentOptions.MidlineLeft;
            labelText.raycastTarget = false;
            return toggle;
        }

        private void EnsureScrollContainer(Transform container)
        {
            if (container == null) return;

            RectTransform rect = EnsureRectTransform(container);
            if (rect == null) return;
            GameObject go = rect.gameObject;
            EnsureLayoutElement(go, 160f, flexible: true);

            go.AddComponentIfMissing<RectMask2D>();
            EnsureBackgroundImage(rect, new Color(0.10f, 0.13f, 0.17f, 0.8f));

            VerticalLayoutGroup vlg = go.GetComponent<VerticalLayoutGroup>();
            if (vlg == null) vlg = go.AddComponent<VerticalLayoutGroup>();
            vlg.spacing = 6f;
            vlg.padding = new RectOffset(8, 8, 8, 8);
            vlg.childControlWidth = true;
            vlg.childControlHeight = false;
            vlg.childForceExpandWidth = true;
            vlg.childForceExpandHeight = false;

            ContentSizeFitter csf = go.GetComponent<ContentSizeFitter>();
            if (csf == null) csf = go.AddComponent<ContentSizeFitter>();
            csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        }

        private void EnsureSectionLayout(Transform section, float spacing, RectOffset padding, Color? bgColor = null)
        {
            if (section == null) return;

            RectTransform rect = EnsureRectTransform(section);
            if (rect == null) return;
            GameObject go = rect.gameObject;

            VerticalLayoutGroup vlg = go.GetComponent<VerticalLayoutGroup>();
            if (vlg == null) vlg = go.AddComponent<VerticalLayoutGroup>();
            vlg.spacing = spacing;
            vlg.padding = padding;
            vlg.childAlignment = TextAnchor.UpperLeft;
            vlg.childControlWidth = true;
            vlg.childControlHeight = false;
            vlg.childForceExpandWidth = true;
            vlg.childForceExpandHeight = false;

            ContentSizeFitter csf = go.GetComponent<ContentSizeFitter>();
            if (csf == null) csf = go.AddComponent<ContentSizeFitter>();
            csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            if (bgColor.HasValue)
            {
                EnsureBackgroundImage(rect, bgColor.Value);
            }
        }

        private void EnsureHorizontalLayout(Transform section, float spacing, RectOffset padding)
        {
            if (section == null) return;

            RectTransform rect = EnsureRectTransform(section);
            if (rect == null) return;
            GameObject go = rect.gameObject;

            HorizontalLayoutGroup hlg = go.GetComponent<HorizontalLayoutGroup>();
            if (hlg == null) hlg = go.AddComponent<HorizontalLayoutGroup>();
            hlg.spacing = spacing;
            hlg.padding = padding;
            hlg.childAlignment = TextAnchor.MiddleCenter;
            hlg.childControlWidth = true;
            hlg.childControlHeight = true;
            hlg.childForceExpandWidth = true;
            hlg.childForceExpandHeight = false;
        }

        #endregion

        #region Resolve & Events

        private void AutoWireReferences()
        {
            if (_safeAreaRoot == null) _safeAreaRoot = FindTransform("SafeArea");
            if (_backgroundRoot == null) _backgroundRoot = FindTransform("Background");
            if (_headerRoot == null) _headerRoot = FindTransform("Header");
            if (_statusPanelRoot == null) _statusPanelRoot = FindTransform("StatusPanel");
            if (_contentRoot == null) _contentRoot = FindTransform("Content");
            if (_currentClanPanelRoot == null) _currentClanPanelRoot = FindTransform("CurrentClanPanel");
            if (_createClanPanelRoot == null) _createClanPanelRoot = FindTransform("CreateClanPanel");
            if (_browseClanPanelRoot == null) _browseClanPanelRoot = FindTransform("BrowseClanPanel");
            if (_membersPanelRoot == null) _membersPanelRoot = FindTransform("MembersPanel");
            if (_emptyStatePanelRoot == null) _emptyStatePanelRoot = FindTransform("EmptyStatePanel");
            if (_footerRoot == null) _footerRoot = FindTransform("Footer");
            if (_modalRoot == null) _modalRoot = FindTransform("ModalRoot");
            if (_toastRoot == null) _toastRoot = FindTransform("ToastRoot");

            if (_titleSlot == null) _titleSlot = FindTransform("TitleSlot");
            if (_statusTextSlot == null) _statusTextSlot = FindTransform("StatusTextSlot");
            if (_currentClanSummarySlot == null) _currentClanSummarySlot = FindTransform("CurrentClanSummarySlot");
            if (_currentClanDetailsSlot == null) _currentClanDetailsSlot = FindTransform("CurrentClanDetailsSlot");
            if (_refreshButtonSlot == null) _refreshButtonSlot = FindTransform("RefreshButtonSlot");
            if (_leaveButtonSlot == null) _leaveButtonSlot = FindTransform("LeaveButtonSlot");
            if (_createNameInputSlot == null) _createNameInputSlot = FindTransform("CreateNameInputSlot");
            if (_createDescriptionInputSlot == null) _createDescriptionInputSlot = FindTransform("CreateDescriptionInputSlot");
            if (_createOpenToggleSlot == null) _createOpenToggleSlot = FindTransform("CreateOpenToggleSlot");
            if (_createButtonSlot == null) _createButtonSlot = FindTransform("CreateButtonSlot");
            if (_searchInputSlot == null) _searchInputSlot = FindTransform("SearchInputSlot");
            if (_searchButtonSlot == null) _searchButtonSlot = FindTransform("SearchButtonSlot");
            if (_browseSummarySlot == null) _browseSummarySlot = FindTransform("BrowseSummarySlot");
            if (_browseResultsContainer == null) _browseResultsContainer = FindTransform("BrowseResultsContainer");
            if (_membersContainer == null) _membersContainer = FindTransform("MembersContainer");
            if (_emptyStateTextSlot == null) _emptyStateTextSlot = FindTransform("EmptyStateTextSlot");
            if (_primaryFooterButtonSlot == null) _primaryFooterButtonSlot = FindTransform("PrimaryFooterButtonSlot");
            if (_secondaryFooterButtonSlot == null) _secondaryFooterButtonSlot = FindTransform("SecondaryFooterButtonSlot");
            if (_toastTextSlot == null) _toastTextSlot = FindTransform("ToastTextSlot");
        }

        private void ResolveUiReferences()
        {
            _titleText = Resolve<TMP_Text>(_titleSlot);
            _statusText = Resolve<TMP_Text>(_statusTextSlot);
            _currentClanSummaryText = Resolve<TMP_Text>(_currentClanSummarySlot);
            _currentClanDetailsText = Resolve<TMP_Text>(_currentClanDetailsSlot);
            _refreshButton = Resolve<Button>(_refreshButtonSlot);
            _leaveButton = Resolve<Button>(_leaveButtonSlot);
            _createNameInput = Resolve<TMP_InputField>(_createNameInputSlot);
            _createDescriptionInput = Resolve<TMP_InputField>(_createDescriptionInputSlot);
            _createOpenToggle = Resolve<Toggle>(_createOpenToggleSlot);
            _createButton = Resolve<Button>(_createButtonSlot);
            _searchInput = Resolve<TMP_InputField>(_searchInputSlot);
            _searchButton = Resolve<Button>(_searchButtonSlot);
            _browseSummaryText = Resolve<TMP_Text>(_browseSummarySlot);
            _emptyStateText = Resolve<TMP_Text>(_emptyStateTextSlot);
            _primaryFooterButton = Resolve<Button>(_primaryFooterButtonSlot);
            _secondaryFooterButton = Resolve<Button>(_secondaryFooterButtonSlot);
            _toastText = Resolve<TMP_Text>(_toastTextSlot);
        }

        private void HookEvents()
        {
            RemoveEvents();

            SafeAddClick(_refreshButton, OnRefreshClicked);
            SafeAddClick(_leaveButton, OnLeaveClicked);
            SafeAddClick(_createButton, OnCreateClicked);
            SafeAddClick(_searchButton, OnSearchClicked);
            SafeAddClick(_primaryFooterButton, OnPrimaryFooterClicked);
            SafeAddClick(_secondaryFooterButton, OnSecondaryFooterClicked);

            if (_createNameInput != null) _createNameInput.onValueChanged.AddListener(v => _createClanName = v ?? string.Empty);
            if (_createDescriptionInput != null) _createDescriptionInput.onValueChanged.AddListener(v => _createClanDescription = v ?? string.Empty);
            if (_searchInput != null)
            {
                _searchInput.onValueChanged.AddListener(v => _searchQuery = v ?? string.Empty);
                _searchInput.onSubmit.AddListener(_ => OnSearchClicked());
            }

            if (_createOpenToggle != null) _createOpenToggle.onValueChanged.AddListener(v => _createClanOpen = v);
            _eventsHooked = true;
        }

        private void RemoveEvents()
        {
            if (!_eventsHooked) return;

            SafeRemoveClick(_refreshButton, OnRefreshClicked);
            SafeRemoveClick(_leaveButton, OnLeaveClicked);
            SafeRemoveClick(_createButton, OnCreateClicked);
            SafeRemoveClick(_searchButton, OnSearchClicked);
            SafeRemoveClick(_primaryFooterButton, OnPrimaryFooterClicked);
            SafeRemoveClick(_secondaryFooterButton, OnSecondaryFooterClicked);

            if (_createNameInput != null) _createNameInput.onValueChanged.RemoveAllListeners();
            if (_createDescriptionInput != null) _createDescriptionInput.onValueChanged.RemoveAllListeners();
            if (_searchInput != null)
            {
                _searchInput.onValueChanged.RemoveAllListeners();
                _searchInput.onSubmit.RemoveAllListeners();
            }

            if (_createOpenToggle != null) _createOpenToggle.onValueChanged.RemoveAllListeners();
            _eventsHooked = false;
        }

        #endregion

        #region Dynamic List Population

        private void PopulateMembers(IReadOnlyList<IVXClanMemberData> members)
        {
            ClearGeneratedRows(_membersContainer);
            if (_membersContainer == null) return;

            if (members == null || members.Count == 0)
            {
                CreateTextRow(_membersContainer, "No members loaded yet.", FontStyles.Italic);
                return;
            }

            for (int i = 0; i < members.Count; i++)
            {
                IVXClanMemberData m = members[i];
                string status = m.IsOnline ? "<color=#4ddb74>Online</color>" : "<color=#888>Offline</color>";
                CreateTextRow(_membersContainer, $"{NullSafe(m.DisplayName, "Unknown")} ({NullSafe(m.Username, "n/a")}) - {status}");
            }
        }

        private void PopulateBrowseResults(IReadOnlyList<IVXClanData> results, bool isBusy, bool hasClan, bool showNotReadyState)
        {
            ClearGeneratedRows(_browseResultsContainer);
            if (_browseResultsContainer == null) return;

            if (showNotReadyState)
            {
                CreateTextRow(_browseResultsContainer, "Log in from IVX_AuthTest to enable clan search and joining.", FontStyles.Italic);
                return;
            }

            if (hasClan)
            {
                CreateTextRow(_browseResultsContainer, "Leave your current clan to join a different one.", FontStyles.Italic);
                return;
            }

            if (results == null || results.Count == 0)
            {
                CreateTextRow(_browseResultsContainer, "No clans found.", FontStyles.Italic);
                return;
            }

            for (int i = 0; i < results.Count; i++)
            {
                IVXClanData clan = results[i];
                string openLabel = clan.IsOpen ? "Open" : "Closed";
                string rowText = $"<b>{NullSafe(clan.Name, "Unnamed")}</b>  {clan.MemberCount}/{clan.MaxMembers}  ({openLabel})";

                if (!isBusy && !hasClan && !string.IsNullOrWhiteSpace(clan.ClanId))
                {
                    CreateJoinRow(_browseResultsContainer, rowText, clan.ClanId);
                }
                else
                {
                    CreateTextRow(_browseResultsContainer, rowText);
                }
            }
        }

        private void CreateTextRow(Transform parent, string text, FontStyles style = FontStyles.Normal)
        {
            GameObject row = new GameObject("RuntimeRow", typeof(RectTransform));
            row.transform.SetParent(parent, false);

            LayoutElement le = row.AddComponent<LayoutElement>();
            le.minHeight = 36f;

            TextMeshProUGUI tmp = row.AddComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = 19f;
            tmp.fontStyle = style;
            tmp.color = COLOR_TEXT;
            tmp.alignment = TextAlignmentOptions.MidlineLeft;
            tmp.textWrappingMode = TextWrappingModes.Normal;
            tmp.richText = true;
            tmp.raycastTarget = false;

            _generatedRows.Add(row);
        }

        private void CreateJoinRow(Transform parent, string text, string clanId)
        {
            GameObject row = new GameObject("RuntimeRow", typeof(RectTransform));
            row.transform.SetParent(parent, false);

            HorizontalLayoutGroup hlg = row.AddComponent<HorizontalLayoutGroup>();
            hlg.spacing = 8f;
            hlg.childAlignment = TextAnchor.MiddleLeft;
            hlg.childControlWidth = true;
            hlg.childControlHeight = true;
            hlg.childForceExpandWidth = true;
            hlg.childForceExpandHeight = false;
            hlg.padding = new RectOffset(4, 4, 4, 4);

            Image rowBg = row.AddComponent<Image>();
            rowBg.color = new Color(0.14f, 0.17f, 0.23f, 0.9f);

            LayoutElement rowLe = row.AddComponent<LayoutElement>();
            rowLe.minHeight = 50f;

            GameObject textObj = new GameObject("RowText", typeof(RectTransform));
            textObj.transform.SetParent(row.transform, false);
            TextMeshProUGUI tmp = textObj.AddComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = 18f;
            tmp.color = COLOR_TEXT;
            tmp.alignment = TextAlignmentOptions.MidlineLeft;
            tmp.textWrappingMode = TextWrappingModes.Normal;
            tmp.richText = true;
            tmp.raycastTarget = false;
            LayoutElement textLe = textObj.AddComponent<LayoutElement>();
            textLe.flexibleWidth = 1f;

            GameObject btnObj = new GameObject("JoinBtn", typeof(RectTransform));
            btnObj.transform.SetParent(row.transform, false);
            Image btnImg = btnObj.AddComponent<Image>();
            btnImg.color = COLOR_BTN_PRIMARY;
            Button btn = btnObj.AddComponent<Button>();
            btn.targetGraphic = btnImg;
            LayoutElement btnLe = btnObj.AddComponent<LayoutElement>();
            btnLe.minWidth = 80f;
            btnLe.preferredWidth = 80f;

            GameObject btnLabel = new GameObject("Label", typeof(RectTransform));
            btnLabel.transform.SetParent(btnObj.transform, false);
            Stretch(btnLabel.GetComponent<RectTransform>());
            TextMeshProUGUI btnText = btnLabel.AddComponent<TextMeshProUGUI>();
            btnText.text = "Join";
            btnText.fontSize = 18f;
            btnText.fontStyle = FontStyles.Bold;
            btnText.color = COLOR_TEXT;
            btnText.alignment = TextAlignmentOptions.Center;
            btnText.raycastTarget = false;

            string captured = clanId;
            btn.onClick.AddListener(() => OnJoinClicked(captured));

            _generatedRows.Add(row);
        }

        private void ClearAllGeneratedRows()
        {
            ClearGeneratedRows(_membersContainer);
            ClearGeneratedRows(_browseResultsContainer);
        }

        private void ClearGeneratedRows(Transform parent)
        {
            if (parent == null) return;

            for (int i = parent.childCount - 1; i >= 0; i--)
            {
                Transform child = parent.GetChild(i);
                if (!string.Equals(child.name, "RuntimeRow", StringComparison.Ordinal)) continue;

                if (Application.isPlaying)
                    Destroy(child.gameObject);
                else
                    DestroyImmediate(child.gameObject);
            }

            _generatedRows.Clear();
        }

        #endregion

        #region Event Handlers

        private void OnRefreshClicked()
        {
            if (_controller == null) return;
            _ = _controller.RefreshCurrentClanAsync();
        }

        private void OnLeaveClicked()
        {
            if (_controller == null) return;
            _ = _controller.LeaveClanAsync();
        }

        private void OnCreateClicked()
        {
            if (_controller == null) return;

            string trimmedName = (_createClanName ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(trimmedName))
            {
                SetText(_statusText, "Clan name is required.");
                return;
            }

            _ = _controller.CreateClanAsync(trimmedName, (_createClanDescription ?? string.Empty).Trim(), _createClanOpen);
        }

        private void OnSearchClicked()
        {
            if (_controller == null) return;
            _ = _controller.SearchClansAsync((_searchQuery ?? string.Empty).Trim());
        }

        private void OnPrimaryFooterClicked()
        {
            if (_controller?.ClanManager?.CurrentClan != null)
            {
                OnRefreshClicked();
                return;
            }

            OnSearchClicked();
        }

        private void OnSecondaryFooterClicked()
        {
            if (_controller?.ClanManager?.CurrentClan != null)
            {
                OnLeaveClicked();
                return;
            }

            OnCreateClicked();
        }

        private void OnJoinClicked(string clanId)
        {
            if (_controller == null || string.IsNullOrWhiteSpace(clanId)) return;
            _ = _controller.JoinClanAsync(clanId);
        }

        #endregion

        #region Utility

        private Transform FindTransform(string objectName)
        {
            return FindDeepChild(transform, objectName);
        }

        private static Transform FindDeepChild(Transform parent, string objectName)
        {
            if (parent == null) return null;
            if (string.Equals(parent.name, objectName, StringComparison.Ordinal)) return parent;

            for (int i = 0; i < parent.childCount; i++)
            {
                Transform result = FindDeepChild(parent.GetChild(i), objectName);
                if (result != null) return result;
            }

            return null;
        }

        private static T Resolve<T>(Transform slot) where T : Component
        {
            if (slot == null) return null;
            T comp = slot.GetComponent<T>();
            return comp != null ? comp : slot.GetComponentInChildren<T>(true);
        }

        private static RectTransform EnsureRectTransform(Transform t)
        {
            if (t == null) return null;
            RectTransform rt = t as RectTransform;
            if (rt != null) return rt;
            rt = t.GetComponent<RectTransform>();
            if (rt != null) return rt;
            return t.gameObject.AddComponent<RectTransform>();
        }

        private static RectTransform FindOrCreateChild(RectTransform parent, string childName)
        {
            Transform existing = parent.Find(childName);
            if (existing != null)
            {
                RectTransform rt = existing as RectTransform;
                return rt != null ? rt : existing.gameObject.AddComponent<RectTransform>();
            }

            GameObject child = new GameObject(childName, typeof(RectTransform));
            child.transform.SetParent(parent, false);
            return child.GetComponent<RectTransform>();
        }

        private static void EnsureBackgroundImage(Transform target, Color color)
        {
            if (target == null) return;
            RectTransform rect = EnsureRectTransform(target);
            if (rect == null) return;
            GameObject go = rect.gameObject;
            Image img = go.GetComponent<Image>();
            if (img == null) img = go.AddComponent<Image>();
            img.color = color;
            img.raycastTarget = true;
        }

        private static CanvasGroup EnsureCanvasGroup(GameObject go)
        {
            CanvasGroup cg = go.GetComponent<CanvasGroup>();
            if (cg == null) cg = go.AddComponent<CanvasGroup>();
            cg.alpha = 1f;
            cg.interactable = true;
            cg.blocksRaycasts = true;
            return cg;
        }

        private static void EnsureLayoutElement(GameObject go, float minHeight, bool flexible = false)
        {
            LayoutElement le = go.GetComponent<LayoutElement>();
            if (le == null) le = go.AddComponent<LayoutElement>();
            le.minHeight = minHeight;
            if (flexible) le.flexibleHeight = 1f;
        }

        private static void Stretch(RectTransform rect, float padding = 0f)
        {
            if (rect == null) return;
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = new Vector2(padding, padding);
            rect.offsetMax = new Vector2(-padding, -padding);
            rect.localScale = Vector3.one;
            rect.localRotation = Quaternion.identity;
        }

        private static void SetActive(Transform target, bool value)
        {
            if (target != null) target.gameObject.SetActive(value);
        }

        private static void SetText(TMP_Text comp, string value)
        {
            if (comp != null) comp.text = value ?? string.Empty;
        }

        private static void SetButtonLabel(Button btn, string value)
        {
            if (btn == null) return;
            TMP_Text label = btn.GetComponentInChildren<TMP_Text>(true);
            if (label != null) label.text = value ?? string.Empty;
        }

        private static void SetInteractable(Selectable sel, bool value)
        {
            if (sel != null) sel.interactable = value;
        }

        private static void SyncInputWithoutNotify(TMP_InputField input, string value)
        {
            if (input != null && input.text != (value ?? string.Empty))
            {
                input.SetTextWithoutNotify(value ?? string.Empty);
            }
        }

        private void RebuildLayout()
        {
            Canvas.ForceUpdateCanvases();

            RectTransform safeAreaRect = _safeAreaRoot as RectTransform;
            RectTransform contentRect = _contentRoot as RectTransform;

            if (contentRect != null)
            {
                LayoutRebuilder.ForceRebuildLayoutImmediate(contentRect);
            }

            if (safeAreaRect != null)
            {
                LayoutRebuilder.ForceRebuildLayoutImmediate(safeAreaRect);
            }
        }

        private static void SafeAddClick(Button btn, UnityEngine.Events.UnityAction action)
        {
            if (btn != null) btn.onClick.AddListener(action);
        }

        private static void SafeRemoveClick(Button btn, UnityEngine.Events.UnityAction action)
        {
            if (btn != null) btn.onClick.RemoveListener(action);
        }

        private static string NullSafe(string value, string fallback = "N/A")
        {
            return string.IsNullOrWhiteSpace(value) ? fallback : value;
        }

        #endregion
    }

    internal static class ComponentExtensions
    {
        public static T AddComponentIfMissing<T>(this GameObject go) where T : Component
        {
            T comp = go.GetComponent<T>();
            return comp != null ? comp : go.AddComponent<T>();
        }
    }
}
