using System;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using TMPro;
using IntelliVerseX.Auth.UI;

namespace IntelliVerseX.Auth.Editor
{
    /// <summary>
    /// Editor utility for creating Auth prefabs with proper UI wiring.
    /// Supports both development and UPM package installations.
    /// </summary>
    public static class IVXAuthPrefabBuilder
    {
        #region Constants

        // Default path for development mode
        private const string DEFAULT_AUTH_PREFABS_PATH = "Assets/_IntelliVerseXSDK/Auth/Prefabs";
        // Writable path for UPM installs
        private const string UPM_AUTH_PREFABS_PATH = "Assets/IntelliVerseX/Generated/Auth/Prefabs";
        
        private static readonly Color AccentColor = new Color(0.25f, 0.52f, 0.96f);
        private static readonly Color DarkBackground = new Color(0.12f, 0.14f, 0.18f);
        private static readonly Color PanelBackground = new Color(0.18f, 0.20f, 0.25f);
        private static readonly Color InputBackground = new Color(0.25f, 0.27f, 0.32f);
        private static readonly Color ButtonSecondary = new Color(0.3f, 0.32f, 0.37f);

        #endregion

        #region Path Resolution

        /// <summary>
        /// Gets the appropriate writable path for auth prefabs.
        /// Returns UPM path if SDK is installed as package, otherwise default path.
        /// </summary>
        private static string GetWritablePrefabPath()
        {
            // Check if SDK is installed as UPM package
            string packagePath = Path.Combine(Application.dataPath, "..", "Library", "PackageCache");
            bool isUPM = false;
            
            if (Directory.Exists(packagePath))
            {
                var dirs = Directory.GetDirectories(packagePath, "com.intelliversex.sdk@*");
                isUPM = dirs.Length > 0;
            }
            
            // Also check manifest.json
            if (!isUPM)
            {
                string manifestPath = Path.Combine(Application.dataPath, "..", "Packages", "manifest.json");
                if (File.Exists(manifestPath))
                {
                    string manifest = File.ReadAllText(manifestPath);
                    isUPM = manifest.Contains("\"com.intelliversex.sdk\"") && 
                            !Directory.Exists("Assets/_IntelliVerseXSDK");
                }
            }
            
            return isUPM ? UPM_AUTH_PREFABS_PATH : DEFAULT_AUTH_PREFABS_PATH;
        }

        #endregion

        #region Menu Items

        // REMOVED: Menu items consolidated into SDK Setup Wizard
        // [MenuItem("IntelliVerse-X SDK/Auth/Create Auth Canvas Prefab", false, 100)]
        // Use: IntelliVerse-X SDK > SDK Setup Wizard > Auth & Social tab
        
        public static void CreateAuthCanvasPrefabMenuItem()
        {
            string prefabPath = GetWritablePrefabPath();
            CreateAuthCanvasPrefabAtPath(prefabPath);
            Debug.Log($"[IVXAuthPrefabBuilder] Auth Canvas prefab created at: {prefabPath}");
        }

        // [MenuItem("IntelliVerse-X SDK/Auth/Add Auth Canvas to Scene", false, 101)]
        // Use: IntelliVerse-X SDK > SDK Setup Wizard > Auth & Social tab
        public static void AddAuthCanvasToScene()
        {
            var existingCanvas = UnityEngine.Object.FindFirstObjectByType<IVXCanvasAuth>();
            if (existingCanvas != null)
            {
                Selection.activeGameObject = existingCanvas.gameObject;
                Debug.Log("[IVXAuthPrefabBuilder] Auth Canvas already exists in scene");
                return;
            }

            // Try to load from various locations
            string[] possiblePaths = new[]
            {
                GetWritablePrefabPath() + "/IVX_AuthCanvas.prefab",
                DEFAULT_AUTH_PREFABS_PATH + "/IVX_AuthCanvas.prefab",
                "Packages/com.intelliversex.sdk/Auth/Prefabs/IVX_AuthCanvas.prefab"
            };

            GameObject prefab = null;
            foreach (var path in possiblePaths)
            {
                prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if (prefab != null) break;
            }
            
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
        /// Create the Auth Canvas prefab at the specified path.
        /// This is the preferred method for SDK setup wizard.
        /// </summary>
        public static void CreateAuthCanvasPrefabAtPath(string folderPath)
        {
            GameObject canvas = null;
            try
            {
                if (string.IsNullOrEmpty(folderPath))
                {
                    Debug.LogError("[IVXAuthPrefabBuilder] Folder path is null or empty");
                    return;
                }
                
                EnsureDirectoryExists(folderPath);
                
                canvas = CreateAuthCanvasPrefab();
                if (canvas != null)
                {
                    string prefabPath = Path.Combine(folderPath, "IVX_AuthCanvas.prefab").Replace("\\", "/");
                    PrefabUtility.SaveAsPrefabAsset(canvas, prefabPath);
                    UnityEngine.Object.DestroyImmediate(canvas);
                    canvas = null;
                    AssetDatabase.Refresh();
                    Debug.Log($"[IVXAuthPrefabBuilder] Auth Canvas prefab saved: {prefabPath}");
                }
                else
                {
                    Debug.LogWarning("[IVXAuthPrefabBuilder] CreateAuthCanvasPrefab returned null. Auth UI types may be missing or have compilation errors.");
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"[IVXAuthPrefabBuilder] Failed to create Auth prefab: {ex.Message}\n{ex.StackTrace}");
                if (canvas != null)
                {
                    try { UnityEngine.Object.DestroyImmediate(canvas); } catch { }
                }
            }
        }

        /// <summary>
        /// Create the Auth Canvas prefab in the scene with all UI properly wired.
        /// </summary>
        public static GameObject CreateAuthCanvasPrefab()
        {
            GameObject canvasGO = null;
            try
            {
                // Create Canvas
                canvasGO = new GameObject("IVX_AuthCanvas");
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
                if (canvasAuth == null)
                {
                    Debug.LogError("[IVXAuthPrefabBuilder] Failed to add IVXCanvasAuth component");
                    UnityEngine.Object.DestroyImmediate(canvasGO);
                    return null;
                }

                // Create panels with proper wiring
                var loginPanel = CreateLoginPanel(canvasGO.transform);
                var registerPanel = CreateRegisterPanel(canvasGO.transform);
                var otpPanel = CreateOTPPanel(canvasGO.transform);
                var loadingPanel = CreateLoadingPanel(canvasGO.transform);

                // Validate panels were created
                if (loginPanel == null || registerPanel == null || otpPanel == null || loadingPanel == null)
                {
                    Debug.LogWarning("[IVXAuthPrefabBuilder] Some panels could not be created, but continuing with partial setup");
                }

                // Wire up IVXCanvasAuth references
                AssignCanvasReferences(canvasAuth, loginPanel, registerPanel, otpPanel, loadingPanel);

                // Wire up individual panel components
                if (loginPanel != null) WireUpLoginPanel(loginPanel);
                if (registerPanel != null) WireUpRegisterPanel(registerPanel);
                if (otpPanel != null) WireUpOTPPanel(otpPanel);

                // Hide register and OTP panels by default
                if (registerPanel != null) registerPanel.SetActive(false);
                if (otpPanel != null) otpPanel.SetActive(false);
                if (loadingPanel != null) loadingPanel.SetActive(false);

                Debug.Log("[IVXAuthPrefabBuilder] Auth Canvas created with all UI properly wired");
                return canvasGO;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[IVXAuthPrefabBuilder] Failed to create Auth Canvas: {ex.Message}\n{ex.StackTrace}");
                if (canvasGO != null)
                {
                    UnityEngine.Object.DestroyImmediate(canvasGO);
                }
                return null;
            }
        }

        #endregion

        #region UI Wiring Methods

        /// <summary>
        /// Wire up all serialized fields on IVXPanelLogin component.
        /// </summary>
        private static void WireUpLoginPanel(GameObject panel)
        {
            var loginComponent = panel.GetComponent<IVXPanelLogin>();
            if (loginComponent == null) return;

            var serializedObj = new SerializedObject(loginComponent);
            var content = panel.transform.Find("Content");
            if (content == null) return;

            // Wire input fields
            var emailInput = FindChildComponent<TMP_InputField>(content, "EmailInput");
            var passwordInput = FindChildComponent<TMP_InputField>(content, "PasswordInput");
            
            if (emailInput != null)
                serializedObj.FindProperty("_emailInput").objectReferenceValue = emailInput;
            if (passwordInput != null)
                serializedObj.FindProperty("_passwordInput").objectReferenceValue = passwordInput;

            // Wire buttons
            var loginButton = FindChildComponent<Button>(content, "LoginButton");
            var registerLink = FindChildComponent<Button>(content, "RegisterLink");
            var forgotPasswordLink = FindChildComponent<Button>(content, "ForgotPasswordLink");
            
            if (loginButton != null)
                serializedObj.FindProperty("_loginButton").objectReferenceValue = loginButton;
            if (registerLink != null)
                serializedObj.FindProperty("_registerButton").objectReferenceValue = registerLink;
            if (forgotPasswordLink != null)
                serializedObj.FindProperty("_forgotPasswordButton").objectReferenceValue = forgotPasswordLink;

            // Wire social buttons
            var socialButtons = content.Find("SocialButtons");
            if (socialButtons != null)
            {
                var googleBtn = FindChildComponent<Button>(socialButtons, "GoogleButton");
                var appleBtn = FindChildComponent<Button>(socialButtons, "AppleButton");
                var guestBtn = FindChildComponent<Button>(socialButtons, "GuestButton");
                
                if (googleBtn != null)
                    serializedObj.FindProperty("_googleSignInButton").objectReferenceValue = googleBtn;
                if (appleBtn != null)
                    serializedObj.FindProperty("_appleSignInButton").objectReferenceValue = appleBtn;
                if (guestBtn != null)
                    serializedObj.FindProperty("_guestLoginButton").objectReferenceValue = guestBtn;
            }

            // Wire toggle
            var rememberMe = FindChildComponent<Toggle>(content, "RememberMe");
            if (rememberMe != null)
                serializedObj.FindProperty("_rememberMeToggle").objectReferenceValue = rememberMe;

            // Wire error text (create if not exists)
            var errorText = CreateErrorText(content);
            if (errorText != null)
                serializedObj.FindProperty("_errorText").objectReferenceValue = errorText;

            serializedObj.ApplyModifiedProperties();
            Debug.Log("[IVXAuthPrefabBuilder] Login panel wired successfully");
        }

        /// <summary>
        /// Wire up all serialized fields on IVXPanelRegister component.
        /// </summary>
        private static void WireUpRegisterPanel(GameObject panel)
        {
            var registerComponent = panel.GetComponent<IVXPanelRegister>();
            if (registerComponent == null) return;

            var serializedObj = new SerializedObject(registerComponent);
            var content = panel.transform.Find("Content");
            if (content == null) return;

            // Wire input fields
            var displayNameInput = FindChildComponent<TMP_InputField>(content, "DisplayNameInput");
            var emailInput = FindChildComponent<TMP_InputField>(content, "EmailInput");
            var passwordInput = FindChildComponent<TMP_InputField>(content, "PasswordInput");
            var confirmPasswordInput = FindChildComponent<TMP_InputField>(content, "ConfirmPasswordInput");
            
            if (displayNameInput != null)
                serializedObj.FindProperty("_displayNameInput").objectReferenceValue = displayNameInput;
            if (emailInput != null)
                serializedObj.FindProperty("_emailInput").objectReferenceValue = emailInput;
            if (passwordInput != null)
                serializedObj.FindProperty("_passwordInput").objectReferenceValue = passwordInput;
            if (confirmPasswordInput != null)
                serializedObj.FindProperty("_confirmPasswordInput").objectReferenceValue = confirmPasswordInput;

            // Wire buttons
            var registerButton = FindChildComponent<Button>(content, "RegisterButton");
            var backToLoginLink = FindChildComponent<Button>(content, "BackToLoginLink");
            
            if (registerButton != null)
                serializedObj.FindProperty("_registerButton").objectReferenceValue = registerButton;
            if (backToLoginLink != null)
                serializedObj.FindProperty("_backToLoginButton").objectReferenceValue = backToLoginLink;

            // Wire toggle
            var termsToggle = FindChildComponent<Toggle>(content, "TermsToggle");
            if (termsToggle != null)
                serializedObj.FindProperty("_termsToggle").objectReferenceValue = termsToggle;

            // Wire error text
            var errorText = CreateErrorText(content);
            if (errorText != null)
                serializedObj.FindProperty("_errorText").objectReferenceValue = errorText;

            serializedObj.ApplyModifiedProperties();
            Debug.Log("[IVXAuthPrefabBuilder] Register panel wired successfully");
        }

        /// <summary>
        /// Wire up all serialized fields on IVXPanelOTP component.
        /// </summary>
        private static void WireUpOTPPanel(GameObject panel)
        {
            var otpComponent = panel.GetComponent<IVXPanelOTP>();
            if (otpComponent == null) return;

            var serializedObj = new SerializedObject(otpComponent);
            var content = panel.transform.Find("Content");
            if (content == null) return;

            // Wire OTP input
            var otpInput = FindChildComponent<TMP_InputField>(content, "OTPInput");
            if (otpInput != null)
                serializedObj.FindProperty("_singleOtpInput").objectReferenceValue = otpInput;

            // Wire buttons
            var verifyButton = FindChildComponent<Button>(content, "VerifyButton");
            var resendLink = FindChildComponent<Button>(content, "ResendLink");
            var backButton = FindChildComponent<Button>(content, "BackButton");
            
            if (verifyButton != null)
                serializedObj.FindProperty("_verifyButton").objectReferenceValue = verifyButton;
            if (resendLink != null)
                serializedObj.FindProperty("_resendButton").objectReferenceValue = resendLink;
            if (backButton != null)
                serializedObj.FindProperty("_backButton").objectReferenceValue = backButton;

            // Wire error text
            var errorText = CreateErrorText(content);
            if (errorText != null)
                serializedObj.FindProperty("_errorText").objectReferenceValue = errorText;

            serializedObj.ApplyModifiedProperties();
            Debug.Log("[IVXAuthPrefabBuilder] OTP panel wired successfully");
        }

        /// <summary>
        /// Helper to find a child component by name.
        /// </summary>
        private static T FindChildComponent<T>(Transform parent, string name) where T : Component
        {
            var child = parent.Find(name);
            if (child != null)
            {
                return child.GetComponent<T>();
            }
            
            // Search recursively
            foreach (Transform t in parent)
            {
                if (t.name == name)
                {
                    return t.GetComponent<T>();
                }
                var result = FindChildComponent<T>(t, name);
                if (result != null) return result;
            }
            
            return null;
        }

        /// <summary>
        /// Creates an error text element if it doesn't exist.
        /// </summary>
        private static TextMeshProUGUI CreateErrorText(Transform parent)
        {
            var existing = parent.Find("ErrorText");
            if (existing != null)
            {
                return existing.GetComponent<TextMeshProUGUI>();
            }

            var errorGO = new GameObject("ErrorText");
            errorGO.transform.SetParent(parent, false);
            errorGO.transform.SetSiblingIndex(2); // After title/subtitle
            
            var rect = errorGO.AddComponent<RectTransform>();
            rect.sizeDelta = new Vector2(0, 30);
            
            var tmp = errorGO.AddComponent<TextMeshProUGUI>();
            tmp.text = "";
            tmp.fontSize = 14;
            tmp.color = new Color(1f, 0.3f, 0.3f); // Red color for errors
            tmp.alignment = TextAlignmentOptions.Center;
            
            errorGO.SetActive(false); // Hidden by default
            
            return tmp;
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

            // Social buttons - Using ASCII-safe icons instead of emojis
            var socialRow = CreateHorizontalGroup(content.transform, "SocialButtons");
            CreateSocialButton(socialRow.transform, "GoogleButton", "G");
            CreateSocialButton(socialRow.transform, "AppleButton", "A");  // Changed from Apple logo emoji
            CreateSocialButton(socialRow.transform, "GuestButton", "?");   // Changed from user emoji

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
            CreateLinkButton(content.transform, "BackButton", "< Back");  // Changed from arrow emoji

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
            image.color = InputBackground;

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
            bgImage.color = InputBackground;

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
            image.color = ButtonSecondary;

            var button = buttonGO.AddComponent<Button>();
            button.targetGraphic = image;

            // Icon (using ASCII-safe text)
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
            tmp.fontStyle = FontStyles.Bold;
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
