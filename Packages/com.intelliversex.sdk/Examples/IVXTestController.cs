// File: IVXTestController.cs
// Purpose: Test controller for SDK login/signup and feature verification
// Version: 1.2.0 - Works with or without Nakama

using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using IntelliVerseX.Core;

#if NAKAMA_INSTALLED
using IntelliVerseX.Backend.Nakama;
#endif

namespace IntelliVerseX.Examples
{
    /// <summary>
    /// Test controller for verifying SDK functionality.
    /// 
    /// Usage:
    /// 1. Create a test scene
    /// 2. Add this script to a GameObject
    /// 3. Wire up UI references or use auto-create
    /// 4. Test login, leaderboard, wallet features
    /// 
    /// Note: Leaderboard and Wallet features require Nakama SDK to be installed.
    /// </summary>
    public class IVXTestController : MonoBehaviour
    {
        #region UI References

        [Header("Login UI")]
        [SerializeField] private TMP_InputField emailInput;
        [SerializeField] private TMP_InputField passwordInput;
        [SerializeField] private Button guestLoginBtn;
        [SerializeField] private Button emailLoginBtn;
        [SerializeField] private Button logoutBtn;

        [Header("Status Display")]
        [SerializeField] private TMP_Text statusText;
        [SerializeField] private TMP_Text sessionInfoText;

        [Header("Feature Test Buttons")]
        [SerializeField] private Button testLeaderboardBtn;
        [SerializeField] private Button testWalletBtn;
        [SerializeField] private Button refreshSessionBtn;

        [Header("Settings")]
        [SerializeField] private bool autoCreateUI = true;
        [SerializeField] private bool verboseLogging = true;

        #endregion

        #region Private Fields

        private const string LOG_TAG = "[IVX-TEST]";
        private Canvas _canvas;
        private bool _isInitialized;

        #endregion

        #region Unity Lifecycle

        private void Start()
        {
            if (autoCreateUI && statusText == null)
            {
                CreateTestUI();
            }

            SetupButtonListeners();
            UpdateSessionDisplay();

            Log("Test Controller initialized. Ready for testing.");
            
#if !NAKAMA_INSTALLED
            Log("⚠️ Nakama SDK not installed. Leaderboard and Wallet features disabled.", true);
            SetStatus("Ready (Nakama not installed - some features disabled)");
#endif
        }

        private void OnDestroy()
        {
#if NAKAMA_INSTALLED
            // Cleanup event subscriptions
            IVXNWalletManager.OnWalletBalanceChanged -= OnWalletChanged;
#endif
        }

        #endregion

        #region UI Setup

        private void SetupButtonListeners()
        {
            guestLoginBtn?.onClick.AddListener(TestGuestLogin);
            emailLoginBtn?.onClick.AddListener(TestEmailLogin);
            logoutBtn?.onClick.AddListener(TestLogout);
            testLeaderboardBtn?.onClick.AddListener(TestLeaderboard);
            testWalletBtn?.onClick.AddListener(TestWallet);
            refreshSessionBtn?.onClick.AddListener(UpdateSessionDisplay);

#if NAKAMA_INSTALLED
            // Subscribe to wallet events
            IVXNWalletManager.OnWalletBalanceChanged += OnWalletChanged;
#endif
        }

        private void CreateTestUI()
        {
            // Create Canvas
            var canvasGo = new GameObject("TestCanvas");
            _canvas = canvasGo.AddComponent<Canvas>();
            _canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasGo.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            canvasGo.AddComponent<GraphicRaycaster>();

            // Create panel
            var panelGo = CreatePanel(_canvas.transform, "TestPanel");
            var vlg = panelGo.AddComponent<VerticalLayoutGroup>();
            vlg.padding = new RectOffset(20, 20, 20, 20);
            vlg.spacing = 10;
            vlg.childControlWidth = true;
            vlg.childControlHeight = false;
            vlg.childForceExpandWidth = true;

            // Title
            CreateText(panelGo.transform, "IntelliVerseX SDK Test", 24, FontStyles.Bold);

            // Status text
            statusText = CreateText(panelGo.transform, "Ready", 16).GetComponent<TMP_Text>();

            // Session info
            sessionInfoText = CreateText(panelGo.transform, "No session", 14).GetComponent<TMP_Text>();

            // Email input
            emailInput = CreateInputField(panelGo.transform, "Email");
            
            // Password input
            passwordInput = CreateInputField(panelGo.transform, "Password");
            passwordInput.contentType = TMP_InputField.ContentType.Password;

            // Buttons
            guestLoginBtn = CreateButton(panelGo.transform, "Guest Login");
            emailLoginBtn = CreateButton(panelGo.transform, "Email Login");
            logoutBtn = CreateButton(panelGo.transform, "Logout");

            CreateText(panelGo.transform, "--- Feature Tests ---", 14);

            testLeaderboardBtn = CreateButton(panelGo.transform, "Test Leaderboard");
            testWalletBtn = CreateButton(panelGo.transform, "Test Wallet");
            refreshSessionBtn = CreateButton(panelGo.transform, "Refresh Session");

            Log("Test UI created automatically");
        }

        private GameObject CreatePanel(Transform parent, string name)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            
            var rt = go.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.1f, 0.1f);
            rt.anchorMax = new Vector2(0.9f, 0.9f);
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;

            var image = go.AddComponent<Image>();
            image.color = new Color(0.1f, 0.1f, 0.1f, 0.9f);

            return go;
        }

        private GameObject CreateText(Transform parent, string text, int fontSize = 16, FontStyles style = FontStyles.Normal)
        {
            var go = new GameObject("Text");
            go.transform.SetParent(parent, false);

            var tmp = go.AddComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = fontSize;
            tmp.fontStyle = style;
            tmp.alignment = TextAlignmentOptions.Left;

            var le = go.AddComponent<LayoutElement>();
            le.minHeight = fontSize + 10;

            return go;
        }

        private TMP_InputField CreateInputField(Transform parent, string placeholder)
        {
            var go = new GameObject("InputField");
            go.transform.SetParent(parent, false);

            var image = go.AddComponent<Image>();
            image.color = new Color(0.2f, 0.2f, 0.2f, 1f);

            var input = go.AddComponent<TMP_InputField>();
            
            // Text area
            var textGo = new GameObject("Text");
            textGo.transform.SetParent(go.transform, false);
            var tmp = textGo.AddComponent<TextMeshProUGUI>();
            tmp.alignment = TextAlignmentOptions.Left;
            tmp.fontSize = 16;
            var textRt = textGo.GetComponent<RectTransform>();
            textRt.anchorMin = Vector2.zero;
            textRt.anchorMax = Vector2.one;
            textRt.offsetMin = new Vector2(10, 5);
            textRt.offsetMax = new Vector2(-10, -5);

            input.textComponent = tmp;
            input.placeholder = CreateText(go.transform, placeholder, 16).GetComponent<TMP_Text>();
            input.placeholder.color = Color.gray;

            var le = go.AddComponent<LayoutElement>();
            le.minHeight = 40;

            return input;
        }

        private Button CreateButton(Transform parent, string text)
        {
            var go = new GameObject("Button_" + text);
            go.transform.SetParent(parent, false);

            var image = go.AddComponent<Image>();
            image.color = new Color(0.2f, 0.4f, 0.8f, 1f);

            var btn = go.AddComponent<Button>();
            btn.targetGraphic = image;

            var textGo = CreateText(go.transform, text, 16);
            var tmp = textGo.GetComponent<TMP_Text>();
            tmp.alignment = TextAlignmentOptions.Center;
            var textRt = textGo.GetComponent<RectTransform>();
            textRt.anchorMin = Vector2.zero;
            textRt.anchorMax = Vector2.one;
            textRt.offsetMin = Vector2.zero;
            textRt.offsetMax = Vector2.zero;

            var le = go.AddComponent<LayoutElement>();
            le.minHeight = 40;

            return btn;
        }

        #endregion

        #region Test Methods

        public async void TestGuestLogin()
        {
            SetStatus("Testing guest login...");
            Log("Starting guest login test");

            try
            {
                var response = await APIManager.GuestSignupAsync(
                    role: "user",
                    configureUserAuthOnSuccess: true,
                    persistSession: true
                );

                if (response != null && response.status)
                {
                    UserSessionManager.SaveFromGuestResponse(response);
                    SetStatus($"✅ Guest login SUCCESS\nUser: {response.data?.user?.userName ?? "Guest"}");
                    Log($"Guest login success: {response.data?.user?.id}");
                }
                else
                {
                    SetStatus($"❌ Guest login FAILED\n{response?.message ?? "Unknown error"}");
                    Log($"Guest login failed: {response?.message}", true);
                }
            }
            catch (Exception ex)
            {
                SetStatus($"❌ Guest login ERROR\n{ex.Message}");
                Log($"Guest login exception: {ex}", true);
            }

            UpdateSessionDisplay();
        }

        public async void TestEmailLogin()
        {
            if (string.IsNullOrWhiteSpace(emailInput?.text) || string.IsNullOrWhiteSpace(passwordInput?.text))
            {
                SetStatus("❌ Please enter email and password");
                return;
            }

            SetStatus("Testing email login...");
            Log($"Starting email login test for: {emailInput.text}");

            try
            {
                var request = new APIManager.LoginRequest
                {
                    email = emailInput.text,
                    password = passwordInput.text,
                    fromDevice = "machine",
                    macAddress = SystemInfo.deviceUniqueIdentifier
                };

                var response = await APIManager.LoginAsync(request, true, true, default);

                if (response != null && response.status)
                {
                    UserSessionManager.SaveFromLoginResponse(response);
                    SetStatus($"✅ Login SUCCESS\nUser: {response.data.user.email}");
                    Log($"Email login success: {response.data.user.id}");
                }
                else
                {
                    SetStatus($"❌ Login FAILED\n{response?.message ?? "Unknown error"}");
                    Log($"Email login failed: {response?.message}", true);
                }
            }
            catch (Exception ex)
            {
                SetStatus($"❌ Login ERROR\n{ex.Message}");
                Log($"Email login exception: {ex}", true);
            }

            UpdateSessionDisplay();
        }

        public void TestLogout()
        {
            Log("Testing logout");
            UserSessionManager.Clear();
            
#if NAKAMA_INSTALLED
            IVXNWalletManager.Initialize(); // Reset wallet state
#endif
            
            SetStatus("✅ Logged out");
            UpdateSessionDisplay();
        }

        public async void TestLeaderboard()
        {
#if !NAKAMA_INSTALLED
            SetStatus("❌ Nakama SDK not installed.\nInstall from: github.com/heroiclabs/nakama-unity");
            Log("Leaderboard test skipped - Nakama not installed", true);
            return;
#else
            if (!UserSessionManager.HasSession)
            {
                SetStatus("❌ Login first to test leaderboard");
                return;
            }

            SetStatus("Testing leaderboard...");
            Log("Starting leaderboard test");

            try
            {
                // Submit test score
                var submitResult = await IVXNLeaderbordManager.SubmitScoreAsync(
                    UnityEngine.Random.Range(100, 1000)
                );

                if (submitResult != null && submitResult.success)
                {
                    SetStatus($"✅ Score submitted!\nScore: {submitResult.score}");
                    Log($"Score submitted successfully, score: {submitResult.score}");
                }
                else
                {
                    SetStatus($"⚠️ Score submission: {submitResult?.error ?? "No response"}");
                    Log($"Score submission issue: {submitResult?.error}");
                }

                // Fetch leaderboards
                var leaderboards = await IVXNLeaderbordManager.GetAllLeaderboardsAsync(10);
                if (leaderboards != null && leaderboards.success)
                {
                    var dailyCount = leaderboards.daily?.records?.Count ?? 0;
                    var weeklyCount = leaderboards.weekly?.records?.Count ?? 0;
                    SetStatus($"✅ Leaderboards fetched\nDaily: {dailyCount}, Weekly: {weeklyCount}");
                    Log($"Leaderboards: Daily={dailyCount}, Weekly={weeklyCount}");
                }
            }
            catch (Exception ex)
            {
                SetStatus($"❌ Leaderboard ERROR\n{ex.Message}");
                Log($"Leaderboard exception: {ex}", true);
            }
#endif
        }

        public void TestWallet()
        {
#if !NAKAMA_INSTALLED
            SetStatus("❌ Nakama SDK not installed.\nInstall from: github.com/heroiclabs/nakama-unity");
            Log("Wallet test skipped - Nakama not installed", true);
            return;
#else
            SetStatus("Testing wallet...");
            Log("Starting wallet test");

            try
            {
                // Initialize if needed
                if (!IVXNWalletManager.IsInitialized)
                {
                    IVXNWalletManager.Initialize(1000, 500);
                    Log("Wallet initialized with Game=1000, Global=500");
                }

                var gameBalance = IVXNWalletManager.GameBalance;
                var globalBalance = IVXNWalletManager.GlobalBalance;

                SetStatus($"✅ Wallet Status\nGame: {gameBalance}\nGlobal: {globalBalance}");
                Log($"Wallet: Game={gameBalance}, Global={globalBalance}");

                // Test spend
                _ = IVXNWalletManager.TrySpendGameAsync(10, "Test spend");
            }
            catch (Exception ex)
            {
                SetStatus($"❌ Wallet ERROR\n{ex.Message}");
                Log($"Wallet exception: {ex}", true);
            }
#endif
        }

        #endregion

        #region Helper Methods

        private void UpdateSessionDisplay()
        {
            var session = UserSessionManager.Current;
            
            if (session != null && !string.IsNullOrWhiteSpace(session.accessToken))
            {
                var displayName = session.userName ?? session.firstName ?? session.email?.Split('@')[0] ?? "User";
                var info = $"Session Active\n" +
                           $"User ID: {session.userId}\n" +
                           $"Name: {displayName}\n" +
                           $"Guest: {session.isGuest}\n" +
                           $"Token Fresh: {UserSessionManager.IsAccessTokenFresh()}";
                
                if (sessionInfoText != null)
                    sessionInfoText.text = info;

                Log($"Session: {session.userId}, Guest: {session.isGuest}");
            }
            else
            {
                if (sessionInfoText != null)
                    sessionInfoText.text = "No active session";

                Log("No active session");
            }
        }

        private void SetStatus(string message)
        {
            if (statusText != null)
                statusText.text = message;
        }

#if NAKAMA_INSTALLED
        private void OnWalletChanged(long game, long global)
        {
            Log($"Wallet changed: Game={game}, Global={global}");
            SetStatus($"Wallet Updated\nGame: {game}\nGlobal: {global}");
        }
#endif

        private void Log(string message, bool isError = false)
        {
            if (!verboseLogging && !isError) return;

            if (isError)
                Debug.LogError($"{LOG_TAG} {message}");
            else
                Debug.Log($"{LOG_TAG} {message}");
        }

        #endregion
    }
}
