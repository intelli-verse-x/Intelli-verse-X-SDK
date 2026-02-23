// File: IVXAuthSceneBuilder.cs
// Purpose: Editor tool to build auth scene with proper UI

using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

namespace IntelliVerseX.Editor
{
    /// <summary>
    /// Editor tool for creating complete auth scene UI
    /// </summary>
    public static class IVXAuthSceneBuilder
    {
        private static readonly Color InputBgColor = new Color(0.25f, 0.27f, 0.32f, 1f);
        private static readonly Color PanelBgColor = new Color(0.12f, 0.14f, 0.18f, 1f);
        private static readonly Color ButtonColor = new Color(0.25f, 0.52f, 0.96f, 1f);
        private static readonly Color TextColor = new Color(1f, 1f, 1f, 1f);
        private static readonly Color SubtitleColor = new Color(0.7f, 0.7f, 0.7f, 1f);

        [MenuItem("IntelliVerseX/Auth/Build ForgotPassword Panel UI", false, 100)]
        public static void BuildForgotPasswordPanelUI()
        {
            var panel = FindPanelInAuthCanvas("ForgotPasswordPanel");
            if (panel == null)
            {
                Debug.LogError("[IVXAuthSceneBuilder] ForgotPasswordPanel not found in scene");
                return;
            }

            BuildForgotPasswordUI(panel);
            Debug.Log("[IVXAuthSceneBuilder] ForgotPasswordPanel UI built successfully");
        }

        [MenuItem("IntelliVerseX/Auth/Build Referral Panel UI", false, 101)]
        public static void BuildReferralPanelUI()
        {
            var panel = FindPanelInAuthCanvas("ReferralPanel");
            if (panel == null)
            {
                Debug.LogError("[IVXAuthSceneBuilder] ReferralPanel not found in scene");
                return;
            }

            BuildReferralUI(panel);
            Debug.Log("[IVXAuthSceneBuilder] ReferralPanel UI built successfully");
        }

        [MenuItem("IntelliVerseX/Auth/Build Login Panel UI", false, 102)]
        public static void BuildLoginPanelUI()
        {
            var panel = FindPanelInAuthCanvas("LoginPanel");
            if (panel == null)
            {
                Debug.LogError("[IVXAuthSceneBuilder] LoginPanel not found in scene");
                return;
            }

            BuildLoginUI(panel);
            Debug.Log("[IVXAuthSceneBuilder] LoginPanel UI built successfully");
        }

        [MenuItem("IntelliVerseX/Auth/Build Register Panel UI", false, 103)]
        public static void BuildRegisterPanelUI()
        {
            var panel = FindPanelInAuthCanvas("RegisterPanel");
            if (panel == null)
            {
                Debug.LogError("[IVXAuthSceneBuilder] RegisterPanel not found in scene");
                return;
            }

            BuildRegisterUI(panel);
            Debug.Log("[IVXAuthSceneBuilder] RegisterPanel UI built successfully");
        }

        [MenuItem("IntelliVerseX/Auth/Build OTP Panel UI", false, 104)]
        public static void BuildOTPPanelUI()
        {
            var panel = FindPanelInAuthCanvas("OTPPanel");
            if (panel == null)
            {
                Debug.LogError("[IVXAuthSceneBuilder] OTPPanel not found in scene");
                return;
            }

            BuildOTPUI(panel);
            Debug.Log("[IVXAuthSceneBuilder] OTPPanel UI built successfully");
        }

        /// <summary>
        /// Find panel in auth canvas hierarchy (works for inactive objects too)
        /// </summary>
        private static GameObject FindPanelInAuthCanvas(string panelName)
        {
            // First try direct find (works for active objects)
            var result = GameObject.Find(panelName);
            if (result != null) return result;

            // Find the auth canvas first
            var canvas = GameObject.Find("IVX_AuthCanvas");
            if (canvas == null)
            {
                // Try finding by component
                var canvasComp = Object.FindFirstObjectByType<IntelliVerseX.Auth.UI.IVXCanvasAuth>();
                if (canvasComp != null) canvas = canvasComp.gameObject;
            }

            if (canvas == null) return null;

            // Search in children (includes inactive)
            foreach (Transform child in canvas.transform)
            {
                if (child.name == panelName)
                    return child.gameObject;
            }

            return null;
        }

        [MenuItem("IntelliVerseX/Auth/Build All Panel UI", false, 110)]
        public static void BuildAllPanelUI()
        {
            BuildLoginPanelUI();
            BuildRegisterPanelUI();
            BuildOTPPanelUI();
            BuildForgotPasswordPanelUI();
            BuildReferralPanelUI();
            
            // Save scene
            UnityEditor.SceneManagement.EditorSceneManager.SaveOpenScenes();
            Debug.Log("[IVXAuthSceneBuilder] All panel UI built and scene saved");
        }

        [MenuItem("IntelliVerseX/Auth/Auto-Wire All Panels", false, 200)]
        public static void AutoWireAllPanels()
        {
            var canvas = GameObject.Find("IVX_AuthCanvas");
            if (canvas == null)
            {
                Debug.LogError("[IVXAuthSceneBuilder] IVX_AuthCanvas not found");
                return;
            }

            AutoWirePanel<IntelliVerseX.Auth.UI.IVXPanelLogin>("LoginPanel");
            AutoWirePanel<IntelliVerseX.Auth.UI.IVXPanelRegister>("RegisterPanel");
            AutoWirePanel<IntelliVerseX.Auth.UI.IVXPanelOTP>("OTPPanel");
            AutoWirePanel<IntelliVerseX.Auth.UI.IVXPanelForgotPassword>("ForgotPasswordPanel");
            AutoWirePanel<IntelliVerseX.Auth.UI.IVXPanelReferral>("ReferralPanel");

            // Wire IVXCanvasAuth
            IVXPrefabBuilder.AutoWireAuthCanvas(canvas);

            Debug.Log("[IVXAuthSceneBuilder] All panels auto-wired");
        }

        private static void AutoWirePanel<T>(string panelName) where T : Component
        {
            var panel = GameObject.Find(panelName);
            if (panel == null) return;

            var component = panel.GetComponent<T>();
            if (component == null) return;

            var so = new SerializedObject(component);

            // Wire _panel reference
            var panelProp = so.FindProperty("_panel");
            if (panelProp != null)
                panelProp.objectReferenceValue = panel;

            // Wire _canvasGroup reference
            var cgProp = so.FindProperty("_canvasGroup");
            if (cgProp != null)
            {
                var cg = panel.GetComponent<CanvasGroup>();
                if (cg == null) cg = panel.AddComponent<CanvasGroup>();
                cgProp.objectReferenceValue = cg;
            }

            so.ApplyModifiedProperties();
            EditorUtility.SetDirty(component);
        }

        private static void BuildForgotPasswordUI(GameObject panel)
        {
            // Clear existing children
            while (panel.transform.childCount > 0)
            {
                Object.DestroyImmediate(panel.transform.GetChild(0).gameObject);
            }

            // Setup panel rect
            var panelRect = panel.GetComponent<RectTransform>();
            if (panelRect == null) panelRect = panel.AddComponent<RectTransform>();
            panelRect.anchorMin = Vector2.zero;
            panelRect.anchorMax = Vector2.one;
            panelRect.offsetMin = Vector2.zero;
            panelRect.offsetMax = Vector2.zero;

            // Add Image for background
            var img = panel.GetComponent<Image>();
            if (img == null) img = panel.AddComponent<Image>();
            img.color = PanelBgColor;

            // Ensure CanvasGroup
            var cg = panel.GetComponent<CanvasGroup>();
            if (cg == null) cg = panel.AddComponent<CanvasGroup>();

            // Create Content container - centered with responsive max width
            var content = CreateContainer("Content", panel.transform);
            var contentRect = content.GetComponent<RectTransform>();
            contentRect.anchorMin = new Vector2(0.5f, 0.5f);
            contentRect.anchorMax = new Vector2(0.5f, 0.5f);
            contentRect.pivot = new Vector2(0.5f, 0.5f);
            contentRect.anchoredPosition = Vector2.zero;
            contentRect.sizeDelta = new Vector2(400, 550); // Fixed max width for responsiveness

            var layout = content.AddComponent<VerticalLayoutGroup>();
            layout.childAlignment = TextAnchor.UpperCenter;
            layout.childControlHeight = false;
            layout.childControlWidth = true;
            layout.childForceExpandWidth = true;
            layout.spacing = 15;
            layout.padding = new RectOffset(30, 30, 30, 30);

            // Title
            CreateTextElement("Title", content.transform, "Reset Password", 28, TextAlignmentOptions.Center, TextColor);

            // Step 1 Container
            var step1 = CreateContainer("Step1Container", content.transform);
            var step1Layout = step1.AddComponent<VerticalLayoutGroup>();
            step1Layout.childAlignment = TextAnchor.UpperCenter;
            step1Layout.childControlHeight = false;
            step1Layout.childControlWidth = true;
            step1Layout.spacing = 10;

            CreateTextElement("Subtitle", step1.transform, "Enter your email to receive a reset code", 16, TextAlignmentOptions.Center, SubtitleColor);
            var emailInput = CreateInputField("EmailInput", step1.transform, "Email Address", TMP_InputField.ContentType.EmailAddress);
            var requestBtn = CreateButton("RequestOTPButton", step1.transform, "Send Reset Code", ButtonColor);

            // Step 2 Container (hidden by default)
            var step2 = CreateContainer("Step2Container", content.transform);
            step2.SetActive(false);
            var step2Layout = step2.AddComponent<VerticalLayoutGroup>();
            step2Layout.childAlignment = TextAnchor.UpperCenter;
            step2Layout.childControlHeight = false;
            step2Layout.childControlWidth = true;
            step2Layout.spacing = 10;

            var emailDisplay = CreateTextElement("EmailDisplayText", step2.transform, "Code sent to: example@email.com", 14, TextAlignmentOptions.Center, SubtitleColor);
            var otpInput = CreateInputField("OTPInput", step2.transform, "Enter 6-digit code", TMP_InputField.ContentType.IntegerNumber);
            var newPassInput = CreateInputField("NewPasswordInput", step2.transform, "New Password", TMP_InputField.ContentType.Password);
            var confirmPassInput = CreateInputField("ConfirmPasswordInput", step2.transform, "Confirm Password", TMP_InputField.ContentType.Password);
            var resetBtn = CreateButton("ResetPasswordButton", step2.transform, "Reset Password", ButtonColor);

            // Timer and Resend
            var timerText = CreateTextElement("TimerText", step2.transform, "Resend code in 30s", 14, TextAlignmentOptions.Center, SubtitleColor);
            var resendBtn = CreateButton("ResendButton", step2.transform, "Resend Code", new Color(0.3f, 0.32f, 0.37f, 1f));

            // Back button
            var backBtn = CreateButton("BackToLoginButton", content.transform, "Back to Login", new Color(0.3f, 0.32f, 0.37f, 1f));

            // Error/Status text
            var errorText = CreateTextElement("ErrorText", content.transform, "", 14, TextAlignmentOptions.Center, new Color(1f, 0.4f, 0.4f, 1f));
            var statusText = CreateTextElement("StatusText", content.transform, "", 14, TextAlignmentOptions.Center, new Color(0.4f, 1f, 0.4f, 1f));

            // Wire component
            var component = panel.GetComponent<IntelliVerseX.Auth.UI.IVXPanelForgotPassword>();
            if (component != null)
            {
                var so = new SerializedObject(component);
                SetField(so, "_panel", panel);
                SetField(so, "_canvasGroup", cg);
                SetField(so, "_step1Container", step1);
                SetField(so, "_emailInput", emailInput.GetComponent<TMP_InputField>());
                SetField(so, "_requestOTPButton", requestBtn.GetComponent<Button>());
                SetField(so, "_step2Container", step2);
                SetField(so, "_otpInput", otpInput.GetComponent<TMP_InputField>());
                SetField(so, "_newPasswordInput", newPassInput.GetComponent<TMP_InputField>());
                SetField(so, "_confirmPasswordInput", confirmPassInput.GetComponent<TMP_InputField>());
                SetField(so, "_resetPasswordButton", resetBtn.GetComponent<Button>());
                SetField(so, "_resendButton", resendBtn.GetComponent<Button>());
                SetField(so, "_timerText", timerText.GetComponent<TextMeshProUGUI>());
                SetField(so, "_backToLoginButton", backBtn.GetComponent<Button>());
                SetField(so, "_errorText", errorText.GetComponent<TextMeshProUGUI>());
                SetField(so, "_statusText", statusText.GetComponent<TextMeshProUGUI>());
                SetField(so, "_emailDisplayText", emailDisplay.GetComponent<TextMeshProUGUI>());
                so.ApplyModifiedProperties();
                EditorUtility.SetDirty(component);
            }

            Undo.RegisterCreatedObjectUndo(content, "Build ForgotPassword UI");
        }

        private static void BuildReferralUI(GameObject panel)
        {
            // Clear existing children
            while (panel.transform.childCount > 0)
            {
                Object.DestroyImmediate(panel.transform.GetChild(0).gameObject);
            }

            // Setup panel rect
            var panelRect = panel.GetComponent<RectTransform>();
            if (panelRect == null) panelRect = panel.AddComponent<RectTransform>();
            panelRect.anchorMin = Vector2.zero;
            panelRect.anchorMax = Vector2.one;
            panelRect.offsetMin = Vector2.zero;
            panelRect.offsetMax = Vector2.zero;

            // Add Image for background (semi-transparent overlay)
            var img = panel.GetComponent<Image>();
            if (img == null) img = panel.AddComponent<Image>();
            img.color = new Color(0, 0, 0, 0.7f);

            // Ensure CanvasGroup
            var cg = panel.GetComponent<CanvasGroup>();
            if (cg == null) cg = panel.AddComponent<CanvasGroup>();

            // Create popup container
            var popup = CreateContainer("Popup", panel.transform);
            var popupRect = popup.GetComponent<RectTransform>();
            popupRect.anchorMin = new Vector2(0.1f, 0.35f);
            popupRect.anchorMax = new Vector2(0.9f, 0.65f);
            popupRect.offsetMin = Vector2.zero;
            popupRect.offsetMax = Vector2.zero;

            var popupImg = popup.AddComponent<Image>();
            popupImg.color = PanelBgColor;

            var layout = popup.AddComponent<VerticalLayoutGroup>();
            layout.childAlignment = TextAnchor.UpperCenter;
            layout.childControlHeight = false;
            layout.childControlWidth = true;
            layout.spacing = 15;
            layout.padding = new RectOffset(30, 30, 30, 30);

            // Title
            CreateTextElement("Title", popup.transform, "Enter Referral Code", 24, TextAlignmentOptions.Center, TextColor);

            // Subtitle
            CreateTextElement("Subtitle", popup.transform, "Have a referral code? Enter it below for bonus rewards!", 14, TextAlignmentOptions.Center, SubtitleColor);

            // Input
            var codeInput = CreateInputField("ReferralCodeInput", popup.transform, "Referral Code", TMP_InputField.ContentType.Alphanumeric);

            // Buttons container
            var buttonsContainer = CreateContainer("Buttons", popup.transform);
            var btnLayout = buttonsContainer.AddComponent<HorizontalLayoutGroup>();
            btnLayout.childAlignment = TextAnchor.MiddleCenter;
            btnLayout.childControlHeight = true;
            btnLayout.childControlWidth = true;
            btnLayout.spacing = 15;

            var submitBtn = CreateButton("SubmitButton", buttonsContainer.transform, "Submit", ButtonColor);
            var clearBtn = CreateButton("ClearButton", buttonsContainer.transform, "Clear", new Color(0.3f, 0.32f, 0.37f, 1f));
            var closeBtn = CreateButton("CloseButton", buttonsContainer.transform, "Skip", new Color(0.3f, 0.32f, 0.37f, 1f));

            // Error/Status text
            var errorText = CreateTextElement("ErrorText", popup.transform, "", 14, TextAlignmentOptions.Center, new Color(1f, 0.4f, 0.4f, 1f));
            var statusText = CreateTextElement("StatusText", popup.transform, "", 14, TextAlignmentOptions.Center, new Color(0.4f, 1f, 0.4f, 1f));

            // Wire component
            var component = panel.GetComponent<IntelliVerseX.Auth.UI.IVXPanelReferral>();
            if (component != null)
            {
                var so = new SerializedObject(component);
                SetField(so, "_panel", panel);
                SetField(so, "_canvasGroup", cg);
                SetField(so, "_referralCodeInput", codeInput.GetComponent<TMP_InputField>());
                SetField(so, "_submitButton", submitBtn.GetComponent<Button>());
                SetField(so, "_clearButton", clearBtn.GetComponent<Button>());
                SetField(so, "_closeButton", closeBtn.GetComponent<Button>());
                SetField(so, "_errorText", errorText.GetComponent<TextMeshProUGUI>());
                SetField(so, "_statusText", statusText.GetComponent<TextMeshProUGUI>());
                so.ApplyModifiedProperties();
                EditorUtility.SetDirty(component);
            }

            Undo.RegisterCreatedObjectUndo(popup, "Build Referral UI");
        }

        private static void BuildLoginUI(GameObject panel)
        {
            // Clear existing children
            while (panel.transform.childCount > 0)
            {
                Object.DestroyImmediate(panel.transform.GetChild(0).gameObject);
            }

            // Setup panel rect
            var panelRect = panel.GetComponent<RectTransform>();
            if (panelRect == null) panelRect = panel.AddComponent<RectTransform>();
            panelRect.anchorMin = Vector2.zero;
            panelRect.anchorMax = Vector2.one;
            panelRect.offsetMin = Vector2.zero;
            panelRect.offsetMax = Vector2.zero;

            // Add Image for background
            var img = panel.GetComponent<Image>();
            if (img == null) img = panel.AddComponent<Image>();
            img.color = PanelBgColor;

            // Ensure CanvasGroup
            var cg = panel.GetComponent<CanvasGroup>();
            if (cg == null) cg = panel.AddComponent<CanvasGroup>();

            // Create Content container - centered with responsive max width
            var content = CreateContainer("Content", panel.transform);
            var contentRect = content.GetComponent<RectTransform>();
            contentRect.anchorMin = new Vector2(0.5f, 0.5f);
            contentRect.anchorMax = new Vector2(0.5f, 0.5f);
            contentRect.pivot = new Vector2(0.5f, 0.5f);
            contentRect.anchoredPosition = Vector2.zero;
            contentRect.sizeDelta = new Vector2(400, 650); // Fixed max width for responsiveness

            var layout = content.AddComponent<VerticalLayoutGroup>();
            layout.childAlignment = TextAnchor.UpperCenter;
            layout.childControlHeight = false;
            layout.childControlWidth = true;
            layout.childForceExpandWidth = true;
            layout.spacing = 12;
            layout.padding = new RectOffset(30, 30, 30, 30);

            // Title
            CreateTextElement("Title", content.transform, "Welcome Back", 32, TextAlignmentOptions.Center, TextColor);
            CreateTextElement("Subtitle", content.transform, "Sign in to continue", 16, TextAlignmentOptions.Center, SubtitleColor);

            // Spacer
            var spacer1 = CreateContainer("Spacer1", content.transform);
            spacer1.AddComponent<LayoutElement>().preferredHeight = 20;

            // Email Input
            var emailInput = CreateInputField("EmailInput", content.transform, "Email Address", TMP_InputField.ContentType.EmailAddress);

            // Password Input with toggle
            var passwordContainer = CreateContainer("PasswordContainer", content.transform);
            var passwordInput = CreateInputField("PasswordInput", passwordContainer.transform, "Password", TMP_InputField.ContentType.Password);
            var passwordRect = passwordInput.GetComponent<RectTransform>();
            passwordRect.anchorMin = Vector2.zero;
            passwordRect.anchorMax = Vector2.one;
            passwordRect.offsetMin = Vector2.zero;
            passwordRect.offsetMax = Vector2.zero;
            passwordContainer.AddComponent<LayoutElement>().preferredHeight = 50;

            // Remember Me and Forgot Password row
            var optionsRow = CreateContainer("OptionsRow", content.transform);
            var optionsLayout = optionsRow.AddComponent<HorizontalLayoutGroup>();
            optionsLayout.childAlignment = TextAnchor.MiddleCenter;
            optionsLayout.spacing = 10;
            optionsRow.AddComponent<LayoutElement>().preferredHeight = 30;

            // Remember Me Toggle
            var rememberMeToggle = CreateToggle("RememberMeToggle", optionsRow.transform, "Remember Me");

            // Forgot Password Button
            var forgotBtn = CreateTextButton("ForgotPasswordButton", optionsRow.transform, "Forgot Password?", SubtitleColor);

            // Login Button
            var loginBtn = CreateButton("LoginButton", content.transform, "Sign In", ButtonColor);

            // Divider
            var divider = CreateTextElement("Divider", content.transform, "— or continue with —", 14, TextAlignmentOptions.Center, SubtitleColor);

            // Social Login row
            var socialRow = CreateContainer("SocialRow", content.transform);
            var socialLayout = socialRow.AddComponent<HorizontalLayoutGroup>();
            socialLayout.childAlignment = TextAnchor.MiddleCenter;
            socialLayout.childControlWidth = true;
            socialLayout.spacing = 15;
            socialRow.AddComponent<LayoutElement>().preferredHeight = 50;

            var googleBtn = CreateButton("GoogleSignInButton", socialRow.transform, "Google", new Color(0.85f, 0.26f, 0.22f, 1f));
            var appleBtn = CreateButton("AppleSignInButton", socialRow.transform, "Apple", new Color(0.2f, 0.2f, 0.2f, 1f));

            // Guest Login
            var guestBtn = CreateTextButton("GuestLoginButton", content.transform, "Continue as Guest", SubtitleColor);

            // Register link
            var registerRow = CreateContainer("RegisterRow", content.transform);
            var registerLayout = registerRow.AddComponent<HorizontalLayoutGroup>();
            registerLayout.childAlignment = TextAnchor.MiddleCenter;
            registerLayout.spacing = 5;
            registerRow.AddComponent<LayoutElement>().preferredHeight = 30;

            CreateTextElement("NoAccountText", registerRow.transform, "Don't have an account?", 14, TextAlignmentOptions.Center, SubtitleColor);
            var registerBtn = CreateTextButton("RegisterButton", registerRow.transform, "Sign Up", ButtonColor);

            // Error text
            var errorText = CreateTextElement("ErrorText", content.transform, "", 14, TextAlignmentOptions.Center, new Color(1f, 0.4f, 0.4f, 1f));

            // Wire component
            var component = panel.GetComponent<IntelliVerseX.Auth.UI.IVXPanelLogin>();
            if (component != null)
            {
                var so = new SerializedObject(component);
                SetField(so, "_panel", panel);
                SetField(so, "_canvasGroup", cg);
                SetField(so, "_emailInput", emailInput.GetComponent<TMP_InputField>());
                SetField(so, "_passwordInput", passwordInput.GetComponent<TMP_InputField>());
                SetField(so, "_rememberMeToggle", rememberMeToggle.GetComponent<Toggle>());
                SetField(so, "_loginButton", loginBtn.GetComponent<Button>());
                SetField(so, "_registerButton", registerBtn.GetComponent<Button>());
                SetField(so, "_forgotPasswordButton", forgotBtn.GetComponent<Button>());
                SetField(so, "_guestLoginButton", guestBtn.GetComponent<Button>());
                SetField(so, "_googleSignInButton", googleBtn.GetComponent<Button>());
                SetField(so, "_appleSignInButton", appleBtn.GetComponent<Button>());
                SetField(so, "_errorText", errorText.GetComponent<TextMeshProUGUI>());
                so.ApplyModifiedProperties();
                EditorUtility.SetDirty(component);
            }

            Undo.RegisterCreatedObjectUndo(content, "Build Login UI");
        }

        private static void BuildRegisterUI(GameObject panel)
        {
            // Clear existing children
            while (panel.transform.childCount > 0)
            {
                Object.DestroyImmediate(panel.transform.GetChild(0).gameObject);
            }

            // Setup panel rect
            var panelRect = panel.GetComponent<RectTransform>();
            if (panelRect == null) panelRect = panel.AddComponent<RectTransform>();
            panelRect.anchorMin = Vector2.zero;
            panelRect.anchorMax = Vector2.one;
            panelRect.offsetMin = Vector2.zero;
            panelRect.offsetMax = Vector2.zero;

            // Add Image for background
            var img = panel.GetComponent<Image>();
            if (img == null) img = panel.AddComponent<Image>();
            img.color = PanelBgColor;

            // Ensure CanvasGroup
            var cg = panel.GetComponent<CanvasGroup>();
            if (cg == null) cg = panel.AddComponent<CanvasGroup>();

            // Create scroll view content - centered with responsive max width
            var content = CreateContainer("Content", panel.transform);
            var contentRect = content.GetComponent<RectTransform>();
            contentRect.anchorMin = new Vector2(0.5f, 0.5f);
            contentRect.anchorMax = new Vector2(0.5f, 0.5f);
            contentRect.pivot = new Vector2(0.5f, 0.5f);
            contentRect.anchoredPosition = Vector2.zero;
            contentRect.sizeDelta = new Vector2(400, 700); // Fixed max width for responsiveness

            var layout = content.AddComponent<VerticalLayoutGroup>();
            layout.childAlignment = TextAnchor.UpperCenter;
            layout.childControlHeight = false;
            layout.childControlWidth = true;
            layout.childForceExpandWidth = true;
            layout.spacing = 10;
            layout.padding = new RectOffset(30, 30, 20, 20);

            // Title
            CreateTextElement("Title", content.transform, "Create Account", 28, TextAlignmentOptions.Center, TextColor);
            CreateTextElement("Subtitle", content.transform, "Sign up to get started", 14, TextAlignmentOptions.Center, SubtitleColor);

            // Username
            var usernameInput = CreateInputField("UsernameInput", content.transform, "Username", TMP_InputField.ContentType.Standard);

            // Email
            var emailInput = CreateInputField("EmailInput", content.transform, "Email Address", TMP_InputField.ContentType.EmailAddress);

            // Password
            var passwordInput = CreateInputField("PasswordInput", content.transform, "Password", TMP_InputField.ContentType.Password);

            // Confirm Password
            var confirmPasswordInput = CreateInputField("ConfirmPasswordInput", content.transform, "Confirm Password", TMP_InputField.ContentType.Password);

            // Terms Toggle
            var termsRow = CreateContainer("TermsRow", content.transform);
            var termsLayout = termsRow.AddComponent<HorizontalLayoutGroup>();
            termsLayout.childAlignment = TextAnchor.MiddleLeft;
            termsLayout.spacing = 10;
            termsRow.AddComponent<LayoutElement>().preferredHeight = 35;

            var termsToggle = CreateToggle("TermsToggle", termsRow.transform, "I agree to the Terms of Service");

            // Register Button
            var registerBtn = CreateButton("RegisterButton", content.transform, "Create Account", ButtonColor);

            // Back to Login
            var backRow = CreateContainer("BackRow", content.transform);
            var backLayout = backRow.AddComponent<HorizontalLayoutGroup>();
            backLayout.childAlignment = TextAnchor.MiddleCenter;
            backLayout.spacing = 5;
            backRow.AddComponent<LayoutElement>().preferredHeight = 30;

            CreateTextElement("HaveAccountText", backRow.transform, "Already have an account?", 14, TextAlignmentOptions.Center, SubtitleColor);
            var backBtn = CreateTextButton("BackToLoginButton", backRow.transform, "Sign In", ButtonColor);

            // Error text
            var errorText = CreateTextElement("ErrorText", content.transform, "", 14, TextAlignmentOptions.Center, new Color(1f, 0.4f, 0.4f, 1f));

            // Wire component
            var component = panel.GetComponent<IntelliVerseX.Auth.UI.IVXPanelRegister>();
            if (component != null)
            {
                var so = new SerializedObject(component);
                SetField(so, "_panel", panel);
                SetField(so, "_canvasGroup", cg);
                SetField(so, "_usernameInput", usernameInput.GetComponent<TMP_InputField>());
                SetField(so, "_emailInput", emailInput.GetComponent<TMP_InputField>());
                SetField(so, "_passwordInput", passwordInput.GetComponent<TMP_InputField>());
                SetField(so, "_confirmPasswordInput", confirmPasswordInput.GetComponent<TMP_InputField>());
                SetField(so, "_termsToggle", termsToggle.GetComponent<Toggle>());
                SetField(so, "_registerButton", registerBtn.GetComponent<Button>());
                SetField(so, "_backToLoginButton", backBtn.GetComponent<Button>());
                SetField(so, "_errorText", errorText.GetComponent<TextMeshProUGUI>());
                so.ApplyModifiedProperties();
                EditorUtility.SetDirty(component);
            }

            Undo.RegisterCreatedObjectUndo(content, "Build Register UI");
        }

        private static void BuildOTPUI(GameObject panel)
        {
            // Clear existing children
            while (panel.transform.childCount > 0)
            {
                Object.DestroyImmediate(panel.transform.GetChild(0).gameObject);
            }

            // Setup panel rect
            var panelRect = panel.GetComponent<RectTransform>();
            if (panelRect == null) panelRect = panel.AddComponent<RectTransform>();
            panelRect.anchorMin = Vector2.zero;
            panelRect.anchorMax = Vector2.one;
            panelRect.offsetMin = Vector2.zero;
            panelRect.offsetMax = Vector2.zero;

            // Add Image for background (semi-transparent overlay)
            var img = panel.GetComponent<Image>();
            if (img == null) img = panel.AddComponent<Image>();
            img.color = new Color(0, 0, 0, 0.85f);

            // Ensure CanvasGroup
            var cg = panel.GetComponent<CanvasGroup>();
            if (cg == null) cg = panel.AddComponent<CanvasGroup>();

            // Create centered popup
            var popup = CreateContainer("Popup", panel.transform);
            var popupRect = popup.GetComponent<RectTransform>();
            popupRect.anchorMin = new Vector2(0.08f, 0.3f);
            popupRect.anchorMax = new Vector2(0.92f, 0.7f);
            popupRect.offsetMin = Vector2.zero;
            popupRect.offsetMax = Vector2.zero;

            var popupImg = popup.AddComponent<Image>();
            popupImg.color = PanelBgColor;

            var layout = popup.AddComponent<VerticalLayoutGroup>();
            layout.childAlignment = TextAnchor.UpperCenter;
            layout.childControlHeight = false;
            layout.childControlWidth = true;
            layout.spacing = 15;
            layout.padding = new RectOffset(25, 25, 25, 25);

            // Close Button (top right)
            var closeBtn = CreateButton("CloseButton", popup.transform, "X", new Color(0.3f, 0.32f, 0.37f, 1f));
            var closeBtnRect = closeBtn.GetComponent<RectTransform>();
            closeBtnRect.anchorMin = new Vector2(1, 1);
            closeBtnRect.anchorMax = new Vector2(1, 1);
            closeBtnRect.pivot = new Vector2(1, 1);
            closeBtnRect.anchoredPosition = new Vector2(-10, -10);
            closeBtnRect.sizeDelta = new Vector2(40, 40);
            Object.DestroyImmediate(closeBtn.GetComponent<LayoutElement>());

            // Title
            CreateTextElement("Title", popup.transform, "Verify Your Email", 26, TextAlignmentOptions.Center, TextColor);

            // Email Display
            var emailDisplay = CreateTextElement("EmailDisplayText", popup.transform, "Code sent to: user@email.com", 14, TextAlignmentOptions.Center, SubtitleColor);

            // OTP Input
            var otpInput = CreateInputField("OTPInput", popup.transform, "Enter 6-digit code", TMP_InputField.ContentType.IntegerNumber);
            var tmpInput = otpInput.GetComponent<TMP_InputField>();
            tmpInput.characterLimit = 6;

            // Verify Button
            var verifyBtn = CreateButton("VerifyButton", popup.transform, "Verify", ButtonColor);

            // Timer Text
            var timerText = CreateTextElement("TimerText", popup.transform, "Resend code in 30s", 14, TextAlignmentOptions.Center, SubtitleColor);

            // Resend Button
            var resendBtn = CreateButton("ResendButton", popup.transform, "Resend Code", new Color(0.3f, 0.32f, 0.37f, 1f));

            // Error text
            var errorText = CreateTextElement("ErrorText", popup.transform, "", 14, TextAlignmentOptions.Center, new Color(1f, 0.4f, 0.4f, 1f));

            // Wire component
            var component = panel.GetComponent<IntelliVerseX.Auth.UI.IVXPanelOTP>();
            if (component != null)
            {
                var so = new SerializedObject(component);
                SetField(so, "_panel", panel);
                SetField(so, "_canvasGroup", cg);
                SetField(so, "_otpInput", otpInput.GetComponent<TMP_InputField>());
                SetField(so, "_verifyButton", verifyBtn.GetComponent<Button>());
                SetField(so, "_resendButton", resendBtn.GetComponent<Button>());
                SetField(so, "_closeButton", closeBtn.GetComponent<Button>());
                SetField(so, "_timerText", timerText.GetComponent<TextMeshProUGUI>());
                SetField(so, "_emailDisplayText", emailDisplay.GetComponent<TextMeshProUGUI>());
                SetField(so, "_errorText", errorText.GetComponent<TextMeshProUGUI>());
                so.ApplyModifiedProperties();
                EditorUtility.SetDirty(component);
            }

            Undo.RegisterCreatedObjectUndo(popup, "Build OTP UI");
        }

        #region UI Creation Helpers

        private static GameObject CreateContainer(string name, Transform parent)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent);
            var rect = go.AddComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            return go;
        }

        private static GameObject CreateTextElement(string name, Transform parent, string text, int fontSize, TextAlignmentOptions alignment, Color color)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent);

            var rect = go.AddComponent<RectTransform>();
            rect.sizeDelta = new Vector2(0, fontSize + 10);

            var tmp = go.AddComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = fontSize;
            tmp.alignment = alignment;
            tmp.color = color;

            var le = go.AddComponent<LayoutElement>();
            le.preferredHeight = fontSize + 15;

            return go;
        }

        private static GameObject CreateInputField(string name, Transform parent, string placeholder, TMP_InputField.ContentType contentType)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent);

            var rect = go.AddComponent<RectTransform>();
            rect.sizeDelta = new Vector2(0, 50);

            var img = go.AddComponent<Image>();
            img.color = InputBgColor;

            var input = go.AddComponent<TMP_InputField>();
            input.contentType = contentType;

            // Create text area
            var textArea = new GameObject("TextArea");
            textArea.transform.SetParent(go.transform);
            var textAreaRect = textArea.AddComponent<RectTransform>();
            textAreaRect.anchorMin = Vector2.zero;
            textAreaRect.anchorMax = Vector2.one;
            textAreaRect.offsetMin = new Vector2(10, 5);
            textAreaRect.offsetMax = new Vector2(-10, -5);
            textArea.AddComponent<RectMask2D>();

            // Create placeholder
            var placeholderGO = new GameObject("Placeholder");
            placeholderGO.transform.SetParent(textArea.transform);
            var phRect = placeholderGO.AddComponent<RectTransform>();
            phRect.anchorMin = Vector2.zero;
            phRect.anchorMax = Vector2.one;
            phRect.offsetMin = Vector2.zero;
            phRect.offsetMax = Vector2.zero;
            var phText = placeholderGO.AddComponent<TextMeshProUGUI>();
            phText.text = placeholder;
            phText.fontSize = 16;
            phText.color = new Color(1f, 1f, 1f, 0.5f);
            phText.alignment = TextAlignmentOptions.Left;

            // Create text
            var textGO = new GameObject("Text");
            textGO.transform.SetParent(textArea.transform);
            var txtRect = textGO.AddComponent<RectTransform>();
            txtRect.anchorMin = Vector2.zero;
            txtRect.anchorMax = Vector2.one;
            txtRect.offsetMin = Vector2.zero;
            txtRect.offsetMax = Vector2.zero;
            var txtText = textGO.AddComponent<TextMeshProUGUI>();
            txtText.fontSize = 16;
            txtText.color = TextColor;
            txtText.alignment = TextAlignmentOptions.Left;

            input.textViewport = textAreaRect;
            input.textComponent = txtText;
            input.placeholder = phText;

            var le = go.AddComponent<LayoutElement>();
            le.preferredHeight = 50;

            return go;
        }

        private static GameObject CreateButton(string name, Transform parent, string text, Color bgColor)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent);

            var rect = go.AddComponent<RectTransform>();
            rect.sizeDelta = new Vector2(0, 50);

            var img = go.AddComponent<Image>();
            img.color = bgColor;
            img.raycastTarget = true;

            var btn = go.AddComponent<Button>();
            btn.targetGraphic = img;

            // Create text
            var textGO = new GameObject("Text");
            textGO.transform.SetParent(go.transform);
            var txtRect = textGO.AddComponent<RectTransform>();
            txtRect.anchorMin = Vector2.zero;
            txtRect.anchorMax = Vector2.one;
            txtRect.offsetMin = Vector2.zero;
            txtRect.offsetMax = Vector2.zero;
            var tmp = textGO.AddComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = 18;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color = TextColor;
            tmp.raycastTarget = false; // Don't block button clicks

            var le = go.AddComponent<LayoutElement>();
            le.preferredHeight = 50;

            return go;
        }

        private static GameObject CreateTextButton(string name, Transform parent, string text, Color textColor)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent);

            var rect = go.AddComponent<RectTransform>();
            rect.sizeDelta = new Vector2(100, 30);

            // Add transparent Image for raycast target (critical for button clicks!)
            var img = go.AddComponent<Image>();
            img.color = new Color(0, 0, 0, 0); // Fully transparent
            img.raycastTarget = true;

            var btn = go.AddComponent<Button>();
            btn.targetGraphic = img; // Use image for button feedback

            // Create text
            var textGO = new GameObject("Text");
            textGO.transform.SetParent(go.transform);
            var txtRect = textGO.AddComponent<RectTransform>();
            txtRect.anchorMin = Vector2.zero;
            txtRect.anchorMax = Vector2.one;
            txtRect.offsetMin = Vector2.zero;
            txtRect.offsetMax = Vector2.zero;
            var tmp = textGO.AddComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = 14;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color = textColor;
            tmp.raycastTarget = false; // Text shouldn't block clicks

            var le = go.AddComponent<LayoutElement>();
            le.preferredHeight = 30;
            le.preferredWidth = 120;
            le.flexibleWidth = 0;

            return go;
        }

        private static GameObject CreateToggle(string name, Transform parent, string labelText)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent);

            var rect = go.AddComponent<RectTransform>();
            rect.sizeDelta = new Vector2(200, 30);

            var toggle = go.AddComponent<Toggle>();

            // Background
            var bg = new GameObject("Background");
            bg.transform.SetParent(go.transform);
            var bgRect = bg.AddComponent<RectTransform>();
            bgRect.anchorMin = new Vector2(0, 0.5f);
            bgRect.anchorMax = new Vector2(0, 0.5f);
            bgRect.pivot = new Vector2(0, 0.5f);
            bgRect.anchoredPosition = Vector2.zero;
            bgRect.sizeDelta = new Vector2(24, 24);
            var bgImg = bg.AddComponent<Image>();
            bgImg.color = InputBgColor;

            // Checkmark
            var checkmark = new GameObject("Checkmark");
            checkmark.transform.SetParent(bg.transform);
            var checkRect = checkmark.AddComponent<RectTransform>();
            checkRect.anchorMin = Vector2.zero;
            checkRect.anchorMax = Vector2.one;
            checkRect.offsetMin = new Vector2(4, 4);
            checkRect.offsetMax = new Vector2(-4, -4);
            var checkImg = checkmark.AddComponent<Image>();
            checkImg.color = ButtonColor;

            // Label
            var label = new GameObject("Label");
            label.transform.SetParent(go.transform);
            var labelRect = label.AddComponent<RectTransform>();
            labelRect.anchorMin = new Vector2(0, 0.5f);
            labelRect.anchorMax = new Vector2(1, 0.5f);
            labelRect.pivot = new Vector2(0, 0.5f);
            labelRect.anchoredPosition = new Vector2(30, 0);
            labelRect.sizeDelta = new Vector2(-30, 30);
            var labelTmp = label.AddComponent<TextMeshProUGUI>();
            labelTmp.text = labelText;
            labelTmp.fontSize = 14;
            labelTmp.color = SubtitleColor;
            labelTmp.alignment = TextAlignmentOptions.Left;

            toggle.targetGraphic = bgImg;
            toggle.graphic = checkImg;

            var le = go.AddComponent<LayoutElement>();
            le.preferredHeight = 30;
            le.flexibleWidth = 1;

            return go;
        }

        private static void SetField(SerializedObject so, string fieldName, Object value)
        {
            var prop = so.FindProperty(fieldName);
            if (prop != null)
            {
                prop.objectReferenceValue = value;
            }
        }

        #endregion
    }
}
