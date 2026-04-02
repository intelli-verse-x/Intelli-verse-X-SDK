using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace IntelliVerseX.Editor
{
    public static class IVXClanSceneBuilder
    {
        #region Colors

        private static readonly Color COL_BG = new Color(0.08f, 0.10f, 0.14f, 0.97f);
        private static readonly Color COL_PANEL = new Color(0.12f, 0.15f, 0.20f, 0.95f);
        private static readonly Color COL_INPUT = new Color(0.16f, 0.19f, 0.25f, 1f);
        private static readonly Color COL_PRIMARY = new Color(0.20f, 0.45f, 0.78f, 1f);
        private static readonly Color COL_DANGER = new Color(0.78f, 0.22f, 0.22f, 1f);
        private static readonly Color COL_SECONDARY = new Color(0.28f, 0.32f, 0.40f, 1f);
        private static readonly Color COL_TEXT = new Color(0.92f, 0.93f, 0.95f, 1f);
        private static readonly Color COL_DIM = new Color(0.60f, 0.63f, 0.68f, 1f);
        private static readonly Color COL_TOAST = new Color(0.14f, 0.17f, 0.22f, 0.96f);
        private static readonly Color COL_SCROLL = new Color(0.10f, 0.13f, 0.17f, 0.8f);

        #endregion

        [MenuItem("IntelliVerseX/Build IVX_Clan UI")]
        public static void BuildClanUI()
        {
            var canvasGO = GameObject.Find("IVX_ClanCanvas");
            if (canvasGO == null)
            {
                Debug.LogError("[IVXClanSceneBuilder] IVX_ClanCanvas not found.");
                return;
            }

            Undo.RegisterFullObjectHierarchyUndo(canvasGO, "Build IVX_Clan UI");
            ClearChildren(canvasGO.transform);
            EnsureCanvasComponents(canvasGO);

            // ── Background (full-screen) ──
            var bg = MakePanel(canvasGO.transform, "Background", COL_BG);
            Stretch(bg);

            // ── SafeArea (full-screen, VLayout: Header | Status | ScrollArea | Footer) ──
            var safe = MakeRect(canvasGO.transform, "SafeArea");
            Stretch(safe);
            var safeVlg = AddVLayout(safe.gameObject, 8f, new RectOffset(16, 16, 12, 12));
            safeVlg.childForceExpandHeight = false;

            // ── HEADER (fixed height, no grow) ──
            var header = MakeFixedSection(safe, "Header", 56f);
            var headerVlg = AddVLayout(header.gameObject, 4f, new RectOffset(4, 4, 4, 4));
            var titleSlot = MakeLabel(header, "TitleSlot", "IVX Clan", 30f, FontStyles.Bold, COL_TEXT);

            // ── STATUS (fixed height) ──
            var statusPanel = MakeFixedSection(safe, "StatusPanel", 44f);
            var spImg = statusPanel.gameObject.AddComponent<Image>();
            spImg.color = COL_PANEL; spImg.raycastTarget = false;
            AddVLayout(statusPanel.gameObject, 4f, new RectOffset(12, 12, 6, 6));
            var statusSlot = MakeLabel(statusPanel, "StatusTextSlot", "Initializing...", 18f, FontStyles.Normal, COL_DIM);

            // ── SCROLLABLE CONTENT AREA (fills remaining space) ──
            var scrollArea = MakeRect(safe, "ScrollArea");
            ConfigureVerticalLayoutChild(scrollArea);
            var scrollLe = scrollArea.gameObject.AddComponent<LayoutElement>();
            scrollLe.minHeight = 400f;
            scrollLe.flexibleHeight = 1f;
            scrollLe.flexibleWidth = 1f;

            var scrollRect = scrollArea.gameObject.AddComponent<ScrollRect>();
            scrollRect.horizontal = false;
            scrollRect.vertical = true;
            scrollRect.movementType = ScrollRect.MovementType.Elastic;
            scrollRect.elasticity = 0.1f;
            scrollRect.scrollSensitivity = 30f;
            scrollArea.gameObject.AddComponent<RectMask2D>();

            var viewport = MakeRect(scrollArea, "Viewport");
            Stretch(viewport);

            var contentContainer = MakeRect(viewport, "Content");
            contentContainer.anchorMin = new Vector2(0f, 1f);
            contentContainer.anchorMax = new Vector2(1f, 1f);
            contentContainer.pivot = new Vector2(0.5f, 1f);
            contentContainer.sizeDelta = new Vector2(0f, 0f);
            var contentVlg = AddVLayout(contentContainer.gameObject, 10f, new RectOffset(0, 0, 0, 0));
            var contentCsf = contentContainer.gameObject.AddComponent<ContentSizeFitter>();
            contentCsf.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
            contentCsf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            scrollRect.viewport = viewport;
            scrollRect.content = contentContainer;

            // ── Content Panels (inside scrollable content) ──
            RectTransform currentClanPanel, createClanPanel, browseClanPanel, membersPanel, emptyStatePanel;
            RectTransform summarySlot, detailsSlot, refreshSlot, leaveSlot;
            RectTransform nameInputSlot, descInputSlot, openToggleSlot, createBtnSlot;
            RectTransform searchInputSlot, searchBtnSlot, browseSummarySlot, browseContainer, membersContainer;
            RectTransform emptyTextSlot;

            BuildCurrentClanPanel(contentContainer, out currentClanPanel, out summarySlot, out detailsSlot, out refreshSlot, out leaveSlot);
            BuildCreateClanPanel(contentContainer, out createClanPanel, out nameInputSlot, out descInputSlot, out openToggleSlot, out createBtnSlot);
            BuildBrowseClanPanel(contentContainer, out browseClanPanel, out searchInputSlot, out searchBtnSlot, out browseSummarySlot, out browseContainer);
            BuildMembersPanel(contentContainer, out membersPanel, out membersContainer);
            BuildEmptyStatePanel(contentContainer, out emptyStatePanel, out emptyTextSlot);

            // ── FOOTER (fixed height at bottom) ──
            var footer = MakeFixedSection(safe, "Footer", 64f);
            var footerHlg = footer.gameObject.AddComponent<HorizontalLayoutGroup>();
            footerHlg.spacing = 12f;
            footerHlg.childAlignment = TextAnchor.MiddleCenter;
            footerHlg.childControlWidth = true; footerHlg.childControlHeight = true;
            footerHlg.childForceExpandWidth = true; footerHlg.childForceExpandHeight = false;
            footerHlg.padding = new RectOffset(4, 4, 4, 4);
            var primaryBtn = MakeBtn(footer, "PrimaryFooterButtonSlot", "Search Clans", COL_PRIMARY, flex: true);
            var secondaryBtn = MakeBtn(footer, "SecondaryFooterButtonSlot", "Create Clan", COL_SECONDARY, flex: true);

            // ── MODAL & TOAST (overlays) ──
            var modalRoot = MakeRect(canvasGO.transform, "ModalRoot");
            Stretch(modalRoot);
            modalRoot.gameObject.SetActive(false);

            var toastRoot = MakeRect(canvasGO.transform, "ToastRoot");
            toastRoot.anchorMin = new Vector2(0.05f, 0f);
            toastRoot.anchorMax = new Vector2(0.95f, 0f);
            toastRoot.pivot = new Vector2(0.5f, 0f);
            toastRoot.sizeDelta = new Vector2(0f, 52f);
            toastRoot.anchoredPosition = new Vector2(0f, 16f);
            toastRoot.gameObject.AddComponent<Image>().color = COL_TOAST;
            var tvlg = AddVLayout(toastRoot.gameObject, 2f, new RectOffset(16, 16, 8, 8));
            tvlg.childAlignment = TextAnchor.MiddleCenter;
            tvlg.childControlHeight = true;
            var toastSlot = MakeLabel(toastRoot, "ToastTextSlot", "", 16f, FontStyles.Italic, COL_DIM, TextAlignmentOptions.Center);

            // ── WIRE IVXClanPanel ──
            WirePanel(canvasGO, safe, bg, header, statusPanel, contentContainer,
                currentClanPanel, createClanPanel, browseClanPanel, membersPanel, emptyStatePanel,
                footer, modalRoot, toastRoot,
                titleSlot, statusSlot, summarySlot, detailsSlot,
                refreshSlot, leaveSlot,
                nameInputSlot, descInputSlot, openToggleSlot, createBtnSlot,
                searchInputSlot, searchBtnSlot, browseSummarySlot, browseContainer, membersContainer,
                emptyTextSlot, primaryBtn, secondaryBtn, toastSlot);

            EditorUtility.SetDirty(canvasGO);
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
                UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());
            Debug.Log("[IVXClanSceneBuilder] UI rebuilt with scrollable layout and proper anchoring.");
        }

        #region Panel Builders

        private static void BuildCurrentClanPanel(RectTransform parent,
            out RectTransform panel, out RectTransform summary, out RectTransform details,
            out RectTransform refreshBtn, out RectTransform leaveBtn)
        {
            panel = MakeSection(parent, "CurrentClanPanel", 6f, new RectOffset(14, 14, 10, 10), COL_PANEL);
            MakeLabel(panel, "CurrentClanLabel", "Your Clan", 22f, FontStyles.Bold, COL_TEXT);
            summary = MakeLabel(panel, "CurrentClanSummarySlot", "No clan joined.", 20f, FontStyles.Bold, COL_TEXT);
            details = MakeLabel(panel, "CurrentClanDetailsSlot", "Create or join a clan below.", 16f, FontStyles.Normal, COL_DIM);
            var row = MakeHRow(panel, "ClanActionRow", 8f);
            refreshBtn = MakeBtn(row, "RefreshButtonSlot", "Refresh", COL_PRIMARY);
            leaveBtn = MakeBtn(row, "LeaveButtonSlot", "Leave Clan", COL_DANGER);
        }

        private static void BuildCreateClanPanel(RectTransform parent,
            out RectTransform panel, out RectTransform nameInput, out RectTransform descInput,
            out RectTransform openToggle, out RectTransform createBtn)
        {
            panel = MakeSection(parent, "CreateClanPanel", 8f, new RectOffset(14, 14, 10, 10), COL_PANEL);
            MakeLabel(panel, "CreateClanLabel", "Create a Clan", 22f, FontStyles.Bold, COL_TEXT);
            nameInput = MakeInput(panel, "CreateNameInputSlot", "Clan name...", false);
            descInput = MakeInput(panel, "CreateDescriptionInputSlot", "Description (optional)...", true);
            openToggle = MakeToggle(panel, "CreateOpenToggleSlot", "Open (anyone can join)");
            createBtn = MakeBtn(panel, "CreateButtonSlot", "Create Clan", COL_PRIMARY);
        }

        private static void BuildBrowseClanPanel(RectTransform parent,
            out RectTransform panel, out RectTransform searchInput, out RectTransform searchBtn,
            out RectTransform browseSummary, out RectTransform browseContainer)
        {
            panel = MakeSection(parent, "BrowseClanPanel", 6f, new RectOffset(14, 14, 10, 10), COL_PANEL);
            MakeLabel(panel, "BrowseClanLabel", "Browse Clans", 22f, FontStyles.Bold, COL_TEXT);
            var searchRow = MakeHRow(panel, "SearchRow", 8f);
            searchInput = MakeInput(searchRow, "SearchInputSlot", "Search clans...", false, flex: true);
            searchBtn = MakeBtn(searchRow, "SearchButtonSlot", "Search", COL_PRIMARY, width: 110f);
            browseSummary = MakeLabel(panel, "BrowseSummarySlot", "Results: 0", 16f, FontStyles.Normal, COL_DIM);
            browseContainer = MakeScroll(panel, "BrowseResultsContainer", 140f);
        }

        private static void BuildMembersPanel(RectTransform parent,
            out RectTransform panel, out RectTransform container)
        {
            panel = MakeSection(parent, "MembersPanel", 6f, new RectOffset(14, 14, 10, 10), COL_PANEL);
            MakeLabel(panel, "MembersLabel", "Members", 22f, FontStyles.Bold, COL_TEXT);
            container = MakeScroll(panel, "MembersContainer", 120f);
        }

        private static void BuildEmptyStatePanel(RectTransform parent,
            out RectTransform panel, out RectTransform textSlot)
        {
            panel = MakeSection(parent, "EmptyStatePanel", 6f, new RectOffset(20, 20, 30, 30), COL_PANEL);
            textSlot = MakeLabel(panel, "EmptyStateTextSlot",
                "No clan joined yet.\nSearch for a clan or create your own.",
                20f, FontStyles.Normal, COL_DIM, TextAlignmentOptions.Center);
        }

        #endregion

        #region Primitives

        private static RectTransform MakeRect(Transform parent, string name)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            return go.GetComponent<RectTransform>();
        }

        private static RectTransform MakePanel(Transform parent, string name, Color c)
        {
            var r = MakeRect(parent, name);
            r.gameObject.AddComponent<Image>().color = c;
            return r;
        }

        private static RectTransform MakeFixedSection(RectTransform parent, string name, float height)
        {
            var r = MakeRect(parent, name);
            ConfigureVerticalLayoutChild(r);
            var le = r.gameObject.AddComponent<LayoutElement>();
            le.minHeight = height;
            le.preferredHeight = height;
            le.flexibleHeight = 0f;
            le.flexibleWidth = 1f;
            return r;
        }

        private static RectTransform MakeSection(RectTransform parent, string name, float sp, RectOffset pad, Color? bg = null)
        {
            var r = MakeRect(parent, name);
            ConfigureVerticalLayoutChild(r);
            if (bg.HasValue) { var img = r.gameObject.AddComponent<Image>(); img.color = bg.Value; img.raycastTarget = false; }
            AddVLayout(r.gameObject, sp, pad);
            r.gameObject.AddComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            r.gameObject.AddComponent<LayoutElement>().flexibleWidth = 1f;
            return r;
        }

        private static RectTransform MakeHRow(RectTransform parent, string name, float sp)
        {
            var r = MakeRect(parent, name);
            ConfigureVerticalLayoutChild(r);
            var h = r.gameObject.AddComponent<HorizontalLayoutGroup>();
            h.spacing = sp; h.childAlignment = TextAnchor.MiddleCenter;
            h.childControlWidth = true; h.childControlHeight = true;
            h.childForceExpandWidth = true; h.childForceExpandHeight = false;
            r.gameObject.AddComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            return r;
        }

        private static RectTransform MakeLabel(RectTransform parent, string name, string text,
            float size, FontStyles style, Color col, TextAlignmentOptions align = TextAlignmentOptions.TopLeft)
        {
            var r = MakeRect(parent, name);
            ConfigureVerticalLayoutChild(r);
            var t = r.gameObject.AddComponent<TextMeshProUGUI>();
            t.text = text; t.fontSize = size; t.fontStyle = style; t.color = col;
            t.alignment = align; t.textWrappingMode = TextWrappingModes.Normal;
            t.overflowMode = TextOverflowModes.Ellipsis; t.raycastTarget = false;
            r.gameObject.AddComponent<LayoutElement>().minHeight = size + 8f;
            return r;
        }

        private static RectTransform MakeBtn(RectTransform parent, string name, string label,
            Color bg, float width = -1f, bool flex = false)
        {
            var r = MakeRect(parent, name);
            ConfigureVerticalLayoutChild(r);
            var img = r.gameObject.AddComponent<Image>(); img.color = bg;
            var btn = r.gameObject.AddComponent<Button>(); btn.targetGraphic = img;
            var cb = btn.colors;
            cb.normalColor = Color.white;
            cb.highlightedColor = new Color(1f, 1f, 1f, 0.85f);
            cb.pressedColor = new Color(0.75f, 0.75f, 0.75f, 1f);
            cb.disabledColor = new Color(0.6f, 0.6f, 0.6f, 0.5f);
            cb.fadeDuration = 0.1f;
            btn.colors = cb;
            var tr = MakeRect(r, "Label"); Stretch(tr, 6f);
            var tmp = tr.gameObject.AddComponent<TextMeshProUGUI>();
            tmp.text = label; tmp.fontSize = 20f; tmp.fontStyle = FontStyles.Bold;
            tmp.color = COL_TEXT; tmp.alignment = TextAlignmentOptions.Center; tmp.raycastTarget = false;
            var le = r.gameObject.AddComponent<LayoutElement>(); le.minHeight = 46f;
            if (width > 0) { le.minWidth = width; le.preferredWidth = width; }
            if (flex) le.flexibleWidth = 1f;
            return r;
        }

        private static RectTransform MakeInput(RectTransform parent, string name, string ph, bool multi, bool flex = false)
        {
            var r = MakeRect(parent, name);
            ConfigureVerticalLayoutChild(r);
            var bgImg = r.gameObject.AddComponent<Image>(); bgImg.color = COL_INPUT;
            var area = MakeRect(r, "Text Area"); Stretch(area, 8f);
            area.gameObject.AddComponent<RectMask2D>();
            var textR = MakeRect(area, "Text"); Stretch(textR);
            var textT = textR.gameObject.AddComponent<TextMeshProUGUI>();
            textT.fontSize = 18f; textT.color = COL_TEXT; textT.raycastTarget = false;
            textT.alignment = multi ? TextAlignmentOptions.TopLeft : TextAlignmentOptions.MidlineLeft;
            textT.textWrappingMode = TextWrappingModes.Normal;
            var phR = MakeRect(area, "Placeholder"); Stretch(phR);
            var phT = phR.gameObject.AddComponent<TextMeshProUGUI>();
            phT.text = ph; phT.fontSize = 18f; phT.fontStyle = FontStyles.Italic;
            phT.color = COL_DIM; phT.raycastTarget = false;
            phT.alignment = multi ? TextAlignmentOptions.TopLeft : TextAlignmentOptions.MidlineLeft;
            phT.textWrappingMode = TextWrappingModes.Normal;
            var inp = r.gameObject.AddComponent<TMP_InputField>();
            inp.textViewport = area; inp.textComponent = textT; inp.placeholder = phT;
            inp.lineType = multi ? TMP_InputField.LineType.MultiLineNewline : TMP_InputField.LineType.SingleLine;
            inp.transition = Selectable.Transition.ColorTint; inp.targetGraphic = bgImg;
            var le = r.gameObject.AddComponent<LayoutElement>();
            le.minHeight = multi ? 80f : 46f;
            if (flex) le.flexibleWidth = 1f;
            return r;
        }

        private static RectTransform MakeToggle(RectTransform parent, string name, string label)
        {
            var r = MakeRect(parent, name);
            ConfigureVerticalLayoutChild(r);
            var h = r.gameObject.AddComponent<HorizontalLayoutGroup>();
            h.spacing = 10f; h.childAlignment = TextAnchor.MiddleLeft;
            h.childControlWidth = false; h.childControlHeight = false; h.childForceExpandWidth = false;
            var bgR = MakeRect(r, "ToggleBG"); bgR.sizeDelta = new Vector2(28f, 28f);
            var bgI = bgR.gameObject.AddComponent<Image>(); bgI.color = COL_INPUT;
            var ckR = MakeRect(bgR, "Checkmark"); Stretch(ckR, 4f);
            var ckI = ckR.gameObject.AddComponent<Image>(); ckI.color = new Color(0.25f, 0.85f, 0.50f, 1f);
            var tgl = r.gameObject.AddComponent<Toggle>(); tgl.targetGraphic = bgI; tgl.graphic = ckI; tgl.isOn = true;
            var lr = MakeRect(r, "ToggleLabel");
            var lt = lr.gameObject.AddComponent<TextMeshProUGUI>();
            lt.text = label; lt.fontSize = 18f; lt.color = COL_TEXT;
            lt.alignment = TextAlignmentOptions.MidlineLeft; lt.raycastTarget = false;
            r.gameObject.AddComponent<LayoutElement>().minHeight = 36f;
            return r;
        }

        private static RectTransform MakeScroll(RectTransform parent, string name, float minH)
        {
            var r = MakeRect(parent, name);
            ConfigureVerticalLayoutChild(r);
            var img = r.gameObject.AddComponent<Image>(); img.color = COL_SCROLL; img.raycastTarget = false;
            r.gameObject.AddComponent<RectMask2D>();
            AddVLayout(r.gameObject, 4f, new RectOffset(6, 6, 6, 6));
            r.gameObject.AddComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            var le = r.gameObject.AddComponent<LayoutElement>(); le.minHeight = minH; le.flexibleHeight = 1f;
            return r;
        }

        #endregion

        #region Utilities

        private static VerticalLayoutGroup AddVLayout(GameObject go, float sp, RectOffset pad)
        {
            var v = go.AddComponent<VerticalLayoutGroup>();
            v.spacing = sp; v.padding = pad;
            v.childAlignment = TextAnchor.UpperLeft;
            v.childControlWidth = true; v.childControlHeight = false;
            v.childForceExpandWidth = true; v.childForceExpandHeight = false;
            return v;
        }

        private static void Stretch(RectTransform r, float p = 0f)
        {
            r.anchorMin = Vector2.zero; r.anchorMax = Vector2.one;
            r.offsetMin = new Vector2(p, p); r.offsetMax = new Vector2(-p, -p);
        }

        private static void ConfigureVerticalLayoutChild(RectTransform r)
        {
            r.anchorMin = new Vector2(0f, 1f);
            r.anchorMax = new Vector2(1f, 1f);
            r.pivot = new Vector2(0.5f, 1f);
            r.anchoredPosition = Vector2.zero;
            r.sizeDelta = new Vector2(0f, r.sizeDelta.y);
            r.localScale = Vector3.one;
            r.localRotation = Quaternion.identity;
        }

        private static void ClearChildren(Transform t)
        {
            for (int i = t.childCount - 1; i >= 0; i--)
                Undo.DestroyObjectImmediate(t.GetChild(i).gameObject);
        }

        private static void EnsureCanvasComponents(GameObject go)
        {
            if (!go.GetComponent<Canvas>())
            {
                var c = Undo.AddComponent<Canvas>(go);
                c.renderMode = RenderMode.ScreenSpaceOverlay;
            }
            if (!go.GetComponent<CanvasScaler>())
            {
                var s = Undo.AddComponent<CanvasScaler>(go);
                s.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                s.referenceResolution = new Vector2(1080, 1920);
                s.matchWidthOrHeight = 0.5f;
            }
            if (!go.GetComponent<GraphicRaycaster>())
                Undo.AddComponent<GraphicRaycaster>(go);
        }

        private static void WirePanel(GameObject canvasGO,
            RectTransform safe, RectTransform bg, RectTransform header, RectTransform statusPanel,
            RectTransform content, RectTransform currentClan, RectTransform createClan,
            RectTransform browseClan, RectTransform members, RectTransform emptyState,
            RectTransform footer, RectTransform modal, RectTransform toast,
            RectTransform title, RectTransform status, RectTransform summary, RectTransform details,
            RectTransform refresh, RectTransform leave,
            RectTransform nameIn, RectTransform descIn, RectTransform openTgl, RectTransform createBtn,
            RectTransform searchIn, RectTransform searchBtn, RectTransform browseSummary,
            RectTransform browseContainer, RectTransform membersContainer,
            RectTransform emptyText, RectTransform primaryBtn, RectTransform secondaryBtn,
            RectTransform toastText)
        {
            var panel = canvasGO.GetComponent<IntelliVerseX.Social.UI.IVXClanPanel>();
            if (panel == null)
            {
                Debug.LogWarning("[IVXClanSceneBuilder] IVXClanPanel not found on canvas.");
                return;
            }

            var so = new SerializedObject(panel);
            W(so, "_safeAreaRoot", safe); W(so, "_backgroundRoot", bg);
            W(so, "_headerRoot", header); W(so, "_statusPanelRoot", statusPanel);
            W(so, "_contentRoot", content); W(so, "_currentClanPanelRoot", currentClan);
            W(so, "_createClanPanelRoot", createClan); W(so, "_browseClanPanelRoot", browseClan);
            W(so, "_membersPanelRoot", members); W(so, "_emptyStatePanelRoot", emptyState);
            W(so, "_footerRoot", footer); W(so, "_modalRoot", modal); W(so, "_toastRoot", toast);
            W(so, "_titleSlot", title); W(so, "_statusTextSlot", status);
            W(so, "_currentClanSummarySlot", summary); W(so, "_currentClanDetailsSlot", details);
            W(so, "_refreshButtonSlot", refresh); W(so, "_leaveButtonSlot", leave);
            W(so, "_createNameInputSlot", nameIn); W(so, "_createDescriptionInputSlot", descIn);
            W(so, "_createOpenToggleSlot", openTgl); W(so, "_createButtonSlot", createBtn);
            W(so, "_searchInputSlot", searchIn); W(so, "_searchButtonSlot", searchBtn);
            W(so, "_browseSummarySlot", browseSummary); W(so, "_browseResultsContainer", browseContainer);
            W(so, "_membersContainer", membersContainer); W(so, "_emptyStateTextSlot", emptyText);
            W(so, "_primaryFooterButtonSlot", primaryBtn); W(so, "_secondaryFooterButtonSlot", secondaryBtn);
            W(so, "_toastTextSlot", toastText);
            so.ApplyModifiedProperties();
        }

        private static void W(SerializedObject so, string f, RectTransform t)
        {
            var p = so.FindProperty(f);
            if (p != null) p.objectReferenceValue = t != null ? (Object)t.transform : null;
        }

        #endregion
    }
}
