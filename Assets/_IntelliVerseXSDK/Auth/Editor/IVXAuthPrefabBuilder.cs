using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using TMPro;
using IntelliVerseX.Auth.UI;

namespace IntelliVerseX.Auth.Editor
{
    /// <summary>
    /// Editor utility for creating Auth prefabs.
    /// </summary>
    public static class IVXAuthPrefabBuilder
    {
        #region Constants

        private const string AUTH_PREFABS_PATH = "Assets/_IntelliVerseXSDK/Auth/Prefabs";
        private static readonly Color AccentColor = new Color(0.25f, 0.52f, 0.96f);
        private static readonly Color DarkBackground = new Color(0.12f, 0.14f, 0.18f);
        private static readonly Color PanelBackground = new Color(0.18f, 0.20f, 0.25f);

        #endregion

        #region Menu Items

        [MenuItem("IntelliVerseX/Auth/Create Auth Canvas Prefab", false, 100)]
        public static void CreateAuthCanvasPrefabMenuItem()
        {
            var canvas = CreateAuthCanvasPrefab();
            if (canvas != null)
            {
                SavePrefab(canvas, "IVX_AuthCanvas");
                Debug.Log("[IVXAuthPrefabBuilder] Auth Canvas prefab created successfully");
            }
        }

        [MenuItem("IntelliVerseX/Auth/Add Auth Canvas to Scene", false, 101)]
        public static void AddAuthCanvasToScene()
        {
            var existingCanvas = Object.FindFirstObjectByType<IVXCanvasAuth>();
            if (existingCanvas != null)
            {
                Selection.activeGameObject = existingCanvas.gameObject;
                Debug.Log("[IVXAuthPrefabBuilder] Auth Canvas already exists in scene");
                return;
            }

            var prefabPath = AUTH_PREFABS_PATH + "/IVX_AuthCanvas.prefab";
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            
            if (prefab != null)
            {
                var instance = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
                Undo.RegisterCreatedObjectUndo(instance, "Add Auth Canvas");
                Selection.activeGameObject = instance;
            }
            else
            {
                var canvas = CreateAuthCanvasPrefab();
                if (canvas != null)
                {
                    Undo.RegisterCreatedObjectUndo(canvas, "Add Auth Canvas");
                    Selection.activeGameObject = canvas;
                }
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Create the Auth Canvas prefab in the scene
        /// </summary>
        public static GameObject CreateAuthCanvasPrefab()
        {
            // Create Canvas
            var canvasGO = new GameObject("IVX_AuthCanvas");
            var canvas = canvasGO.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 100;

            var scaler = canvasGO.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080, 1920);
            scaler.matchWidthOrHeight = 0.5f;

            canvasGO.AddComponent<GraphicRaycaster>();

            // Add IVXCanvasAuth component
            var canvasAuth = canvasGO.AddComponent<IVXCanvasAuth>();

            // Create panels
            var loginPanel = CreateLoginPanel(canvasGO.transform);
            var registerPanel = CreateRegisterPanel(canvasGO.transform);
            var otpPanel = CreateOTPPanel(canvasGO.transform);
            var loadingPanel = CreateLoadingPanel(canvasGO.transform);

            // Assign references via SerializedObject
            AssignCanvasReferences(canvasAuth, loginPanel, registerPanel, otpPanel, loadingPanel);

            // Hide register and OTP panels by default
            registerPanel.SetActive(false);
            otpPanel.SetActive(false);
            loadingPanel.SetActive(false);

            return canvasGO;
        }

        #endregion

        #region Private Methods - Panel Creation

        private static GameObject CreateLoginPanel(Transform parent)
        {
            var panel = CreatePanel(parent, "LoginPanel");
            panel.AddComponent<IVXPanelLogin>();

            var content = CreatePanelContent(panel.transform);

            // Title
            CreateTitle(content.transform, "Welcome Back", "Sign in to continue");

            // Email input
            CreateInputField(content.transform, "EmailInput", "Email", TMP_InputField.ContentType.EmailAddress);

            // Password input
            CreateInputField(content.transform, "PasswordInput", "Password", TMP_InputField.ContentType.Password);

            // Remember me
            CreateToggle(content.transform, "RememberMe", "Remember me");

            // Login button
            CreateButton(content.transform, "LoginButton", "Sign In", AccentColor);

            // Divider
            CreateDivider(content.transform, "or continue with");

            // Social buttons
            var socialRow = CreateHorizontalGroup(content.transform, "SocialButtons");
            CreateSocialButton(socialRow.transform, "GoogleButton", "G");
            CreateSocialButton(socialRow.transform, "AppleButton", "");
            CreateSocialButton(socialRow.transform, "GuestButton", "👤");

            // Register link
            CreateLinkButton(content.transform, "RegisterLink", "Don't have an account? Sign Up");

            // Forgot password link
            CreateLinkButton(content.transform, "ForgotPasswordLink", "Forgot Password?");

            return panel;
        }

        private static GameObject CreateRegisterPanel(Transform parent)
        {
            var panel = CreatePanel(parent, "RegisterPanel");
            panel.AddComponent<IVXPanelRegister>();

            var content = CreatePanelContent(panel.transform);

            // Title
            CreateTitle(content.transform, "Create Account", "Join us today");

            // Display name
            CreateInputField(content.transform, "DisplayNameInput", "Display Name", TMP_InputField.ContentType.Standard);

            // Email input
            CreateInputField(content.transform, "EmailInput", "Email", TMP_InputField.ContentType.EmailAddress);

            // Password input
            CreateInputField(content.transform, "PasswordInput", "Password", TMP_InputField.ContentType.Password);

            // Confirm password
            CreateInputField(content.transform, "ConfirmPasswordInput", "Confirm Password", TMP_InputField.ContentType.Password);

            // Terms toggle
            CreateToggle(content.transform, "TermsToggle", "I agree to the Terms & Conditions");

            // Register button
            CreateButton(content.transform, "RegisterButton", "Create Account", AccentColor);

            // Back to login link
            CreateLinkButton(content.transform, "BackToLoginLink", "Already have an account? Sign In");

            return panel;
        }

        private static GameObject CreateOTPPanel(Transform parent)
        {
            var panel = CreatePanel(parent, "OTPPanel");
            panel.AddComponent<IVXPanelOTP>();

            var content = CreatePanelContent(panel.transform);

            // Title
            CreateTitle(content.transform, "Verify Email", "Enter the 6-digit code sent to your email");

            // OTP input
            CreateInputField(content.transform, "OTPInput", "000000", TMP_InputField.ContentType.IntegerNumber);

            // Verify button
            CreateButton(content.transform, "VerifyButton", "Verify", AccentColor);

            // Resend link
            CreateLinkButton(content.transform, "ResendLink", "Didn't receive code? Resend");

            // Back button
            CreateLinkButton(content.transform, "BackButton", "← Back");

            return panel;
        }

        private static GameObject CreateLoadingPanel(Transform parent)
        {
            var panel = CreatePanel(parent, "LoadingPanel");
            
            // Semi-transparent background
            var image = panel.GetComponent<Image>();
            image.color = new Color(0, 0, 0, 0.7f);

            // Loading indicator
            var spinnerGO = new GameObject("Spinner");
            spinnerGO.transform.SetParent(panel.transform, false);
            
            var spinnerRect = spinnerGO.AddComponent<RectTransform>();
            spinnerRect.anchorMin = new Vector2(0.5f, 0.5f);
            spinnerRect.anchorMax = new Vector2(0.5f, 0.5f);
            spinnerRect.sizeDelta = new Vector2(60, 60);

            var spinnerImage = spinnerGO.AddComponent<Image>();
            spinnerImage.color = AccentColor;

            // Loading text
            var textGO = new GameObject("LoadingText");
            textGO.transform.SetParent(panel.transform, false);
            
            var textRect = textGO.AddComponent<RectTransform>();
            textRect.anchorMin = new Vector2(0.5f, 0.5f);
            textRect.anchorMax = new Vector2(0.5f, 0.5f);
            textRect.anchoredPosition = new Vector2(0, -60);
            textRect.sizeDelta = new Vector2(200, 40);

            var tmp = textGO.AddComponent<TextMeshProUGUI>();
            tmp.text = "Loading...";
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.fontSize = 18;
            tmp.color = Color.white;

            return panel;
        }

        #endregion

        #region Private Methods - UI Components

        private static GameObject CreatePanel(Transform parent, string name)
        {
            var panelGO = new GameObject(name);
            panelGO.transform.SetParent(parent, false);

            var rect = panelGO.AddComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.sizeDelta = Vector2.zero;

            var image = panelGO.AddComponent<Image>();
            image.color = DarkBackground;

            return panelGO;
        }

        private static GameObject CreatePanelContent(Transform parent)
        {
            var contentGO = new GameObject("Content");
            contentGO.transform.SetParent(parent, false);

            var rect = contentGO.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = new Vector2(400, 600);

            var image = contentGO.AddComponent<Image>();
            image.color = PanelBackground;

            var layout = contentGO.AddComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset(30, 30, 40, 40);
            layout.spacing = 15;
            layout.childAlignment = TextAnchor.UpperCenter;
            layout.childControlWidth = true;
            layout.childControlHeight = false;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;

            contentGO.AddComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            return contentGO;
        }

        private static void CreateTitle(Transform parent, string title, string subtitle)
        {
            // Title
            var titleGO = new GameObject("Title");
            titleGO.transform.SetParent(parent, false);
            
            var titleRect = titleGO.AddComponent<RectTransform>();
            titleRect.sizeDelta = new Vector2(0, 50);
            
            var titleTMP = titleGO.AddComponent<TextMeshProUGUI>();
            titleTMP.text = title;
            titleTMP.alignment = TextAlignmentOptions.Center;
            titleTMP.fontSize = 32;
            titleTMP.fontStyle = FontStyles.Bold;
            titleTMP.color = Color.white;

            // Subtitle
            var subtitleGO = new GameObject("Subtitle");
            subtitleGO.transform.SetParent(parent, false);
            
            var subtitleRect = subtitleGO.AddComponent<RectTransform>();
            subtitleRect.sizeDelta = new Vector2(0, 30);
            
            var subtitleTMP = subtitleGO.AddComponent<TextMeshProUGUI>();
            subtitleTMP.text = subtitle;
            subtitleTMP.alignment = TextAlignmentOptions.Center;
            subtitleTMP.fontSize = 16;
            subtitleTMP.color = new Color(0.7f, 0.7f, 0.7f);
        }

        private static TMP_InputField CreateInputField(Transform parent, string name, string placeholder, TMP_InputField.ContentType contentType)
        {
            var inputGO = new GameObject(name);
            inputGO.transform.SetParent(parent, false);

            var rect = inputGO.AddComponent<RectTransform>();
            rect.sizeDelta = new Vector2(0, 50);

            var image = inputGO.AddComponent<Image>();
            image.color = new Color(0.25f, 0.27f, 0.32f);

            var input = inputGO.AddComponent<TMP_InputField>();
            input.contentType = contentType;

            // Text area
            var textAreaGO = new GameObject("TextArea");
            textAreaGO.transform.SetParent(inputGO.transform, false);
            
            var textAreaRect = textAreaGO.AddComponent<RectTransform>();
            textAreaRect.anchorMin = Vector2.zero;
            textAreaRect.anchorMax = Vector2.one;
            textAreaRect.offsetMin = new Vector2(15, 5);
            textAreaRect.offsetMax = new Vector2(-15, -5);

            textAreaGO.AddComponent<RectMask2D>();

            // Placeholder
            var placeholderGO = new GameObject("Placeholder");
            placeholderGO.transform.SetParent(textAreaGO.transform, false);
            
            var placeholderRect = placeholderGO.AddComponent<RectTransform>();
            placeholderRect.anchorMin = Vector2.zero;
            placeholderRect.anchorMax = Vector2.one;
            placeholderRect.sizeDelta = Vector2.zero;
            
            var placeholderTMP = placeholderGO.AddComponent<TextMeshProUGUI>();
            placeholderTMP.text = placeholder;
            placeholderTMP.fontSize = 16;
            placeholderTMP.color = new Color(0.5f, 0.5f, 0.5f);
            placeholderTMP.alignment = TextAlignmentOptions.MidlineLeft;

            // Text
            var textGO = new GameObject("Text");
            textGO.transform.SetParent(textAreaGO.transform, false);
            
            var textRect = textGO.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.sizeDelta = Vector2.zero;
            
            var textTMP = textGO.AddComponent<TextMeshProUGUI>();
            textTMP.fontSize = 16;
            textTMP.color = Color.white;
            textTMP.alignment = TextAlignmentOptions.MidlineLeft;

            input.textViewport = textAreaRect;
            input.textComponent = textTMP;
            input.placeholder = placeholderTMP;

            return input;
        }

        private static Button CreateButton(Transform parent, string name, string text, Color color)
        {
            var buttonGO = new GameObject(name);
            buttonGO.transform.SetParent(parent, false);

            var rect = buttonGO.AddComponent<RectTransform>();
            rect.sizeDelta = new Vector2(0, 50);

            var image = buttonGO.AddComponent<Image>();
            image.color = color;

            var button = buttonGO.AddComponent<Button>();
            button.targetGraphic = image;

            // Text
            var textGO = new GameObject("Text");
            textGO.transform.SetParent(buttonGO.transform, false);
            
            var textRect = textGO.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.sizeDelta = Vector2.zero;
            
            var tmp = textGO.AddComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.fontSize = 18;
            tmp.fontStyle = FontStyles.Bold;
            tmp.color = Color.white;

            return button;
        }

        private static Button CreateLinkButton(Transform parent, string name, string text)
        {
            var buttonGO = new GameObject(name);
            buttonGO.transform.SetParent(parent, false);

            var rect = buttonGO.AddComponent<RectTransform>();
            rect.sizeDelta = new Vector2(0, 30);

            var button = buttonGO.AddComponent<Button>();

            // Text
            var textGO = new GameObject("Text");
            textGO.transform.SetParent(buttonGO.transform, false);
            
            var textRect = textGO.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.sizeDelta = Vector2.zero;
            
            var tmp = textGO.AddComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.fontSize = 14;
            tmp.color = AccentColor;

            return button;
        }

        private static Toggle CreateToggle(Transform parent, string name, string label)
        {
            var toggleGO = new GameObject(name);
            toggleGO.transform.SetParent(parent, false);

            var rect = toggleGO.AddComponent<RectTransform>();
            rect.sizeDelta = new Vector2(0, 30);

            var layout = toggleGO.AddComponent<HorizontalLayoutGroup>();
            layout.spacing = 10;
            layout.childAlignment = TextAnchor.MiddleLeft;
            layout.childControlWidth = false;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = false;
            layout.childForceExpandHeight = true;

            // Checkbox background
            var bgGO = new GameObject("Background");
            bgGO.transform.SetParent(toggleGO.transform, false);
            
            var bgRect = bgGO.AddComponent<RectTransform>();
            bgRect.sizeDelta = new Vector2(24, 24);
            
            var bgImage = bgGO.AddComponent<Image>();
            bgImage.color = new Color(0.25f, 0.27f, 0.32f);

            // Checkmark
            var checkGO = new GameObject("Checkmark");
            checkGO.transform.SetParent(bgGO.transform, false);
            
            var checkRect = checkGO.AddComponent<RectTransform>();
            checkRect.anchorMin = Vector2.zero;
            checkRect.anchorMax = Vector2.one;
            checkRect.sizeDelta = new Vector2(-6, -6);
            checkRect.anchoredPosition = Vector2.zero;
            
            var checkImage = checkGO.AddComponent<Image>();
            checkImage.color = AccentColor;

            // Label
            var labelGO = new GameObject("Label");
            labelGO.transform.SetParent(toggleGO.transform, false);
            
            var labelRect = labelGO.AddComponent<RectTransform>();
            labelRect.sizeDelta = new Vector2(300, 24);
            
            var labelTMP = labelGO.AddComponent<TextMeshProUGUI>();
            labelTMP.text = label;
            labelTMP.fontSize = 14;
            labelTMP.color = new Color(0.8f, 0.8f, 0.8f);
            labelTMP.alignment = TextAlignmentOptions.MidlineLeft;

            var toggle = toggleGO.AddComponent<Toggle>();
            toggle.targetGraphic = bgImage;
            toggle.graphic = checkImage;
            toggle.isOn = false;

            return toggle;
        }

        private static void CreateDivider(Transform parent, string text)
        {
            var dividerGO = new GameObject("Divider");
            dividerGO.transform.SetParent(parent, false);

            var rect = dividerGO.AddComponent<RectTransform>();
            rect.sizeDelta = new Vector2(0, 30);

            var layout = dividerGO.AddComponent<HorizontalLayoutGroup>();
            layout.spacing = 10;
            layout.childAlignment = TextAnchor.MiddleCenter;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = true;

            // Left line
            var leftLine = new GameObject("LeftLine");
            leftLine.transform.SetParent(dividerGO.transform, false);
            var leftImage = leftLine.AddComponent<Image>();
            leftImage.color = new Color(0.4f, 0.4f, 0.4f);
            var leftLayout = leftLine.AddComponent<LayoutElement>();
            leftLayout.preferredHeight = 1;
            leftLayout.flexibleWidth = 1;

            // Text
            var textGO = new GameObject("Text");
            textGO.transform.SetParent(dividerGO.transform, false);
            var textLayout = textGO.AddComponent<LayoutElement>();
            textLayout.preferredWidth = 120;
            var tmp = textGO.AddComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = 12;
            tmp.color = new Color(0.5f, 0.5f, 0.5f);
            tmp.alignment = TextAlignmentOptions.Center;

            // Right line
            var rightLine = new GameObject("RightLine");
            rightLine.transform.SetParent(dividerGO.transform, false);
            var rightImage = rightLine.AddComponent<Image>();
            rightImage.color = new Color(0.4f, 0.4f, 0.4f);
            var rightLayout = rightLine.AddComponent<LayoutElement>();
            rightLayout.preferredHeight = 1;
            rightLayout.flexibleWidth = 1;
        }

        private static GameObject CreateHorizontalGroup(Transform parent, string name)
        {
            var groupGO = new GameObject(name);
            groupGO.transform.SetParent(parent, false);

            var rect = groupGO.AddComponent<RectTransform>();
            rect.sizeDelta = new Vector2(0, 50);

            var layout = groupGO.AddComponent<HorizontalLayoutGroup>();
            layout.spacing = 15;
            layout.childAlignment = TextAnchor.MiddleCenter;
            layout.childControlWidth = false;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = false;
            layout.childForceExpandHeight = true;

            return groupGO;
        }

        private static Button CreateSocialButton(Transform parent, string name, string icon)
        {
            var buttonGO = new GameObject(name);
            buttonGO.transform.SetParent(parent, false);

            var rect = buttonGO.AddComponent<RectTransform>();
            rect.sizeDelta = new Vector2(60, 50);

            var image = buttonGO.AddComponent<Image>();
            image.color = new Color(0.3f, 0.32f, 0.37f);

            var button = buttonGO.AddComponent<Button>();
            button.targetGraphic = image;

            // Icon
            var iconGO = new GameObject("Icon");
            iconGO.transform.SetParent(buttonGO.transform, false);
            
            var iconRect = iconGO.AddComponent<RectTransform>();
            iconRect.anchorMin = Vector2.zero;
            iconRect.anchorMax = Vector2.one;
            iconRect.sizeDelta = Vector2.zero;
            
            var tmp = iconGO.AddComponent<TextMeshProUGUI>();
            tmp.text = icon;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.fontSize = 24;
            tmp.color = Color.white;

            return button;
        }

        #endregion

        #region Private Methods - Utilities

        private static void AssignCanvasReferences(IVXCanvasAuth canvasAuth, GameObject login, GameObject register, GameObject otp, GameObject loading)
        {
            var serializedObject = new SerializedObject(canvasAuth);
            
            serializedObject.FindProperty("_loginPanel").objectReferenceValue = login;
            serializedObject.FindProperty("_registerPanel").objectReferenceValue = register;
            serializedObject.FindProperty("_otpPanel").objectReferenceValue = otp;
            serializedObject.FindProperty("_loadingPanel").objectReferenceValue = loading;
            
            serializedObject.ApplyModifiedProperties();
        }

        private static void SavePrefab(GameObject gameObject, string name)
        {
            // Ensure directory exists
            if (!AssetDatabase.IsValidFolder(AUTH_PREFABS_PATH))
            {
                var parts = AUTH_PREFABS_PATH.Split('/');
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

            var prefabPath = $"{AUTH_PREFABS_PATH}/{name}.prefab";
            PrefabUtility.SaveAsPrefabAsset(gameObject, prefabPath);
            Object.DestroyImmediate(gameObject);
            AssetDatabase.Refresh();

            Debug.Log($"[IVXAuthPrefabBuilder] Prefab saved: {prefabPath}");
        }

        #endregion
    }
}
