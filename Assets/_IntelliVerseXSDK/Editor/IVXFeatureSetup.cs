// File: IVXFeatureSetup.cs
// Purpose: One-click setup for SDK features (Leaderboard, Friends, etc.)
// Version: 1.0.0

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace IntelliVerseX.Editor
{
    /// <summary>
    /// One-click setup utilities for SDK features.
    /// Provides easy integration for Leaderboard, Friends, Wallet, and other features.
    /// </summary>
    public class IVXFeatureSetup : EditorWindow
    {
        #region Constants

        private const string SDK_ROOT = "Assets/_IntelliVerseXSDK";
        private const string PACKAGE_NAME = "com.intelliversex.sdk";
        private const string SDK_PACKAGE_ROOT = "Packages/" + PACKAGE_NAME;
        private const string PREFABS_ROOT = SDK_ROOT + "/Prefabs";
        private const string SOCIAL_PREFABS = SDK_ROOT + "/Social/Prefabs";

        #endregion

        #region UI State

        private Vector2 scrollPosition;
        private int selectedFeature = 0;
        private string[] featureNames = new string[]
        {
            "🏆 Leaderboard",
            "👥 Friends System",
            "💰 Wallet",
            "🔐 Authentication",
            "📊 Analytics",
            "📺 Ads"
        };

        #endregion

        #region Menu Items

        [MenuItem("IntelliVerseX/Feature Setup/Leaderboard Setup", false, 200)]
        public static void OpenLeaderboardSetup()
        {
            var window = GetWindow<IVXFeatureSetup>("Feature Setup");
            window.selectedFeature = 0;
            window.Show();
        }

        [MenuItem("IntelliVerseX/Feature Setup/Friends Setup", false, 201)]
        public static void OpenFriendsSetup()
        {
            var window = GetWindow<IVXFeatureSetup>("Feature Setup");
            window.selectedFeature = 1;
            window.Show();
        }

        [MenuItem("IntelliVerseX/Feature Setup/Wallet Setup", false, 202)]
        public static void OpenWalletSetup()
        {
            var window = GetWindow<IVXFeatureSetup>("Feature Setup");
            window.selectedFeature = 2;
            window.Show();
        }

        [MenuItem("IntelliVerseX/Feature Setup/One-Click Leaderboard", false, 250)]
        public static void QuickSetupLeaderboard()
        {
            SetupLeaderboard(null);
            EditorUtility.DisplayDialog("Leaderboard Setup",
                "Leaderboard has been set up!\n\n" +
                "Usage:\n" +
                "  await IVXGLeaderboardManager.SubmitScoreAsync(score);\n" +
                "  var response = await IVXGLeaderboardManager.GetAllLeaderboardsAsync();",
                "OK");
        }

        [MenuItem("IntelliVerseX/Feature Setup/One-Click Friends", false, 251)]
        public static void QuickSetupFriends()
        {
            SetupFriendsSystem(null);
            EditorUtility.DisplayDialog("Friends Setup",
                "Friends system has been set up!\n\n" +
                "Prefabs added to scene. Wire up UI buttons as needed.\n\n" +
                "Usage:\n" +
                "  var friends = await IVXFriendsService.GetFriendsAsync();\n" +
                "  await IVXFriendsService.SendFriendRequestAsync(userId);",
                "OK");
        }

        #endregion

        #region GUI

        private void OnGUI()
        {
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

            DrawHeader();
            DrawFeatureTabs();
            
            switch (selectedFeature)
            {
                case 0: DrawLeaderboardSetup(); break;
                case 1: DrawFriendsSetup(); break;
                case 2: DrawWalletSetup(); break;
                case 3: DrawAuthSetup(); break;
                case 4: DrawAnalyticsSetup(); break;
                case 5: DrawAdsSetup(); break;
            }

            EditorGUILayout.EndScrollView();
        }

        private void DrawHeader()
        {
            EditorGUILayout.Space(10);
            
            var headerStyle = new GUIStyle(EditorStyles.boldLabel)
            {
                fontSize = 16,
                alignment = TextAnchor.MiddleCenter
            };
            
            GUILayout.Label("🎯 Feature Setup", headerStyle);
            GUILayout.Label("One-click setup for SDK features", EditorStyles.centeredGreyMiniLabel);
            
            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
        }

        private void DrawFeatureTabs()
        {
            selectedFeature = GUILayout.Toolbar(selectedFeature, featureNames, GUILayout.Height(30));
            EditorGUILayout.Space(10);
        }

        #endregion

        #region Leaderboard Setup

        private void DrawLeaderboardSetup()
        {
            EditorGUILayout.LabelField("🏆 Leaderboard Integration", EditorStyles.boldLabel);
            
            EditorGUILayout.HelpBox(
                "The Leaderboard system uses Nakama backend for score storage and retrieval.\n" +
                "Supports daily, weekly, monthly, and all-time leaderboards.",
                MessageType.Info);

            EditorGUILayout.Space(10);

            // Status check
            DrawStatusBox("Leaderboard Status", new[]
            {
                ("IVXGLeaderboardManager.cs", SDKFileExists("Leaderboard/Static/IVXGLeaderboardManager.cs")),
                ("IVXGLeaderboard.cs (Runtime)", SDKFileExists("Leaderboard/Runtime/IVXGLeaderboard.cs")),
                ("IVXGLeaderboardUI.cs", SDKFileExists("Leaderboard/UI/IVXGLeaderboardUI.cs")),
                ("Backend Service", SDKFileExists("Backend/IVXBackendService.cs"))
            });

            EditorGUILayout.Space(10);

            // Quick setup button
            if (GUILayout.Button("⚡ One-Click Setup", GUILayout.Height(35)))
            {
                SetupLeaderboard(EditorSceneManager.GetActiveScene().GetRootGameObjects()[0].transform);
            }

            EditorGUILayout.Space(10);

            // Code examples
            DrawCodeExample("Submit Score",
@"// Submit a score to the leaderboard using IVXGLeaderboardManager
using IntelliVerseX.Games.Leaderboard;

int score = 1000;
var result = await IVXGLeaderboardManager.SubmitScoreAsync(score);

if (result != null && result.success)
{
    Debug.Log($""Score submitted! Reward: {result.reward_earned}"");
}");

            DrawCodeExample("Fetch Leaderboard",
@"// Get all leaderboards (daily, weekly, monthly, all-time, global)
using IntelliVerseX.Games.Leaderboard;

var response = await IVXGLeaderboardManager.GetAllLeaderboardsAsync(limit: 50);

if (response != null && response.success)
{
    // Access different leaderboard types
    var daily = response.daily;      // Daily leaderboard
    var weekly = response.weekly;    // Weekly leaderboard
    var monthly = response.monthly;  // Monthly leaderboard
    var alltime = response.alltime;  // All-time leaderboard
    var global = response.global_alltime; // Global leaderboard
}

// Get player rank
int rank = await IVXGLeaderboardManager.GetPlayerRankAsync();");

            EditorGUILayout.Space(10);

            // Leaderboard types
            EditorGUILayout.LabelField("Leaderboard Types:", EditorStyles.boldLabel);
            EditorGUILayout.LabelField("• quizverse_global - All-time global scores");
            EditorGUILayout.LabelField("• quizverse_daily - Daily reset");
            EditorGUILayout.LabelField("• quizverse_weekly - Weekly reset");
            EditorGUILayout.LabelField("• quizverse_monthly - Monthly reset");
        }

        private static void SetupLeaderboard(Transform parent)
        {
            Debug.Log("[IVXFeatureSetup] Setting up Leaderboard...");

            // Ensure backend is set up
            var managerType = FindType("IntelliVerseX.Backend.IVXNManager");
            if (managerType != null)
            {
                var existing = UnityEngine.Object.FindObjectOfType(managerType);
                if (existing == null)
                {
                    var go = new GameObject("NakamaManager");
                    go.AddComponent(managerType);
                    
                    var geoType = FindType("IntelliVerseX.Services.GeoLocationService");
                    if (geoType != null) go.AddComponent(geoType);
                    
                    if (parent != null) go.transform.SetParent(parent);
                    Undo.RegisterCreatedObjectUndo(go, "Add NakamaManager");
                    
                    Debug.Log("[IVXFeatureSetup] Created NakamaManager");
                }
            }

            Debug.Log("[IVXFeatureSetup] Leaderboard setup complete");
        }

        #endregion

        #region Friends Setup

        private void DrawFriendsSetup()
        {
            EditorGUILayout.LabelField("👥 Friends System Integration", EditorStyles.boldLabel);
            
            EditorGUILayout.HelpBox(
                "The Friends system provides friend requests, search, and friend list management.\n" +
                "Includes ready-to-use UI prefabs for common friend interactions.",
                MessageType.Info);

            EditorGUILayout.Space(10);

            // Status check
            DrawStatusBox("Friends Status", new[]
            {
                ("IVXFriendsService.cs", SDKFileExists("Social/Runtime/IVXFriendsService.cs")),
                ("IVXFriendsConfig.cs", SDKFileExists("Social/Runtime/IVXFriendsConfig.cs")),
                ("IVXFriendsPanel.cs", SDKFileExists("Social/UI/IVXFriendsPanel.cs")),
                ("UI Prefabs", SDKFolderExists("Social/Prefabs"))
            });

            EditorGUILayout.Space(10);

            // Quick setup buttons
            EditorGUILayout.BeginHorizontal();
            
            if (GUILayout.Button("⚡ One-Click Setup", GUILayout.Height(35)))
            {
                SetupFriendsSystem(null);
            }

            if (GUILayout.Button("📦 Add UI Prefabs to Scene", GUILayout.Height(35)))
            {
                AddFriendsPrefabsToScene();
            }

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(10);

            // Code examples
            DrawCodeExample("Get Friends List",
@"// Get all friends
var friends = await IVXFriendsService.GetFriendsAsync();

// Subscribe to updates
IVXFriendsService.OnFriendsListUpdated += (list) => {
    Debug.Log($""Friends updated: {list.Count}"");
};");

            DrawCodeExample("Friend Requests",
@"// Send a friend request
await IVXFriendsService.SendFriendRequestAsync(""user123"");

// Get incoming requests
var requests = await IVXFriendsService.GetIncomingRequestsAsync();

// Accept/Reject requests
await IVXFriendsService.AcceptFriendRequestAsync(""request_id"");
await IVXFriendsService.RejectFriendRequestAsync(""request_id"");");

            DrawCodeExample("Search Users",
@"// Search for users
var results = await IVXFriendsService.SearchUsersAsync(""john"", limit: 20);

foreach (var user in results)
{
    Debug.Log($""Found: {user.displayName}"");
}");

            EditorGUILayout.Space(10);

            // Available prefabs
            EditorGUILayout.LabelField("Available UI Prefabs:", EditorStyles.boldLabel);
            EditorGUILayout.LabelField("• IVXFriendsPanel - Complete friends panel UI");
            EditorGUILayout.LabelField("• IVXFriendSlot - Single friend display slot");
            EditorGUILayout.LabelField("• IVXFriendRequestSlot - Friend request display");
            EditorGUILayout.LabelField("• IVXFriendSearchSlot - Search result slot");
        }

        private static void SetupFriendsSystem(Transform parent)
        {
            Debug.Log("[IVXFeatureSetup] Setting up Friends System...");

            // Create config if needed
            var configPath = "Assets/Resources/IntelliVerseX/FriendsConfig.asset";
            var configDir = Path.GetDirectoryName(configPath);
            
            if (!AssetDatabase.IsValidFolder(configDir))
            {
                var parts = configDir.Split('/');
                var current = parts[0];
                for (int i = 1; i < parts.Length; i++)
                {
                    var next = current + "/" + parts[i];
                    if (!AssetDatabase.IsValidFolder(next))
                        AssetDatabase.CreateFolder(current, parts[i]);
                    current = next;
                }
            }

            var configType = FindType("IntelliVerseX.Social.IVXFriendsConfig");
            if (configType != null && !File.Exists(configPath))
            {
                var config = ScriptableObject.CreateInstance(configType);
                AssetDatabase.CreateAsset(config, configPath);
                AssetDatabase.SaveAssets();
                Debug.Log("[IVXFeatureSetup] Created FriendsConfig asset");
            }

            Debug.Log("[IVXFeatureSetup] Friends System setup complete");
        }

        private void AddFriendsPrefabsToScene()
        {
            if (!SDKFolderExists("Social/Prefabs"))
            {
                EditorUtility.DisplayDialog("Error", "Friends prefabs folder not found", "OK");
                return;
            }

            var searchRoots = new[]
            {
                $"{SDK_ROOT}/Social/Prefabs",
                $"{SDK_PACKAGE_ROOT}/Social/Prefabs"
            };

            var prefabGuids = AssetDatabase.FindAssets("t:Prefab", searchRoots);
            var canvas = FindOrCreateCanvas();

            int added = 0;
            foreach (var prefabGuid in prefabGuids)
            {
                var assetPath = AssetDatabase.GUIDToAssetPath(prefabGuid);
                var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
                
                if (prefab != null && !GameObject.Find(prefab.name))
                {
                    var instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab, canvas.transform);
                    Undo.RegisterCreatedObjectUndo(instance, $"Add {prefab.name}");
                    added++;
                }
            }

            EditorUtility.DisplayDialog("Prefabs Added",
                $"Added {added} prefabs to the scene.\n\nRemember to wire up button events!", "OK");
        }

        #endregion

        #region Wallet Setup

        private void DrawWalletSetup()
        {
            EditorGUILayout.LabelField("💰 Wallet Integration", EditorStyles.boldLabel);
            
            EditorGUILayout.HelpBox(
                "Dual-wallet system with Game Coins (game-specific) and Global Coins (cross-game).\n" +
                "All operations are synced with the backend.",
                MessageType.Info);

            EditorGUILayout.Space(10);

            // Status check
            DrawStatusBox("Wallet Status", new[]
            {
                ("IVXWalletManager.cs", SDKFileExists("Backend/IVXWalletManager.cs")),
                ("IVXNWalletManager.cs (V2)", SDKFileExists("V2/Manager/IVXNWalletManager.cs"))
            });

            EditorGUILayout.Space(10);

            if (GUILayout.Button("⚡ One-Click Setup", GUILayout.Height(35)))
            {
                EditorUtility.DisplayDialog("Wallet Setup",
                    "Wallet is automatically initialized when NakamaManager connects.\n\n" +
                    "Make sure NakamaManager is in your scene.",
                    "OK");
            }

            EditorGUILayout.Space(10);

            DrawCodeExample("Get Balances",
@"// Get current balances
long gameBalance = IVXNWalletManager.GameBalance;
long globalBalance = IVXNWalletManager.GlobalBalance;

// Subscribe to balance changes
IVXNWalletManager.OnWalletBalanceChanged += (game, global) => {
    Debug.Log($""Balances: Game={game}, Global={global}"");
};");

            DrawCodeExample("Spend/Credit Coins",
@"// Try to spend (will fail if insufficient)
bool success = await IVXNWalletManager.TrySpendGameAsync(100, ""Bought power-up"");

// Credit coins (rewards, purchases)
await IVXNWalletManager.CreditGameAsync(500, ""Daily reward"");

// Transfer between wallets
await IVXNWalletManager.TransferGameToGlobalAsync(100, ""Withdraw"");");
        }

        #endregion

        #region Auth Setup

        private void DrawAuthSetup()
        {
            EditorGUILayout.LabelField("🔐 Authentication Setup", EditorStyles.boldLabel);
            
            EditorGUILayout.HelpBox(
                "Authentication supports Guest, Email, Google, and Apple sign-in.\n" +
                "User sessions are automatically managed and persisted.",
                MessageType.Info);

            EditorGUILayout.Space(10);

            DrawStatusBox("Auth Status", new[]
            {
                ("UserSessionManager.cs", SDKFileExists("Identity/UserSessionManager.cs")),
                ("APIManager.cs", SDKFileExists("Identity/APIManager.cs")),
                ("IntelliVerseXUserIdentity.cs", SDKFileExists("Identity/IntelliVerseXUserIdentity.cs"))
            });

            EditorGUILayout.Space(10);

            DrawCodeExample("Guest Login",
@"// Create guest account
var response = await APIManager.GuestSignupAsync(deviceId);
if (response.success)
{
    UserSessionManager.SaveFromGuestResponse(response);
}");

            DrawCodeExample("Email Login",
@"// Email login
var response = await APIManager.LoginAsync(email, password);
if (response.success)
{
    UserSessionManager.SaveFromLoginResponse(response);
}

// Check if logged in
var session = UserSessionManager.Current;
bool isLoggedIn = session != null && !string.IsNullOrEmpty(session.accessToken);");

            DrawCodeExample("Guest to User Conversion",
@"// Convert guest account to full account
var response = await APIManager.ConvertGuestToUserAsync(email, password, username);
if (response.success)
{
    // Guest account is now a full account
    UserSessionManager.SaveFromLoginResponse(response);
}");
        }

        #endregion

        #region Analytics Setup

        private void DrawAnalyticsSetup()
        {
            EditorGUILayout.LabelField("📊 Analytics Setup", EditorStyles.boldLabel);
            
            EditorGUILayout.HelpBox(
                "Track user events, game sessions, and custom metrics.\n" +
                "Integrates with multiple analytics backends.",
                MessageType.Info);

            EditorGUILayout.Space(10);

            DrawStatusBox("Analytics Status", new[]
            {
                ("IVXAnalyticsManager.cs", SDKFileExists("Analytics/IVXAnalyticsManager.cs")),
                ("IVXAnalyticsService.cs", SDKFileExists("Analytics/IVXAnalyticsService.cs"))
            });

            EditorGUILayout.Space(10);

            DrawCodeExample("Track Events",
@"// Track custom event
IVXAnalyticsManager.TrackEvent(""level_completed"", new Dictionary<string, object>
{
    [""level""] = 5,
    [""score""] = 1000,
    [""time""] = 120.5f
});

// Track screen view
IVXAnalyticsManager.TrackScreenView(""MainMenu"");

// Track purchase
IVXAnalyticsManager.TrackPurchase(""coin_pack_100"", 0.99m, ""USD"");");
        }

        #endregion

        #region Ads Setup

        private void DrawAdsSetup()
        {
            EditorGUILayout.LabelField("📺 Ads Integration", EditorStyles.boldLabel);
            
            EditorGUILayout.HelpBox(
                "Supports LevelPlay (IronSource) and Appodeal with A/B testing.\n" +
                "Includes rewarded, interstitial, and banner ads.",
                MessageType.Info);

            EditorGUILayout.Space(10);

            DrawStatusBox("Ads Status", new[]
            {
                ("IVXAdsManager.cs", SDKFileExists("Monetization/IVXAdsManager.cs")),
                ("IVXAdsWaterfallManager.cs", SDKFileExists("Monetization/IVXAdsWaterfallManager.cs")),
                ("LevelPlay Package", CheckPackageInstalled("com.unity.services.levelplay"))
            });

            EditorGUILayout.Space(10);

            DrawCodeExample("Show Rewarded Ad",
@"// Show rewarded ad
IVXAdsManager.ShowRewardedAd(""reward_double_coins"", (success) => {
    if (success)
    {
        // Give reward
        IVXNWalletManager.CreditGameAsync(100, ""Ad reward"");
    }
});

// Async version
var result = await IVXAdsManager.ShowRewardedAdAsync(""reward_extra_life"");
if (result == AdResult.Completed)
{
    // Reward granted
}");

            DrawCodeExample("Interstitial & Banner",
@"// Show interstitial
IVXAdsManager.ShowInterstitial(""level_complete"");

// Banner ads
IVXAdsManager.ShowBanner(BannerPosition.Bottom);
IVXAdsManager.HideBanner();");
        }

        #endregion

        #region Helper Methods

        private static bool SDKFileExists(string relativePath)
        {
            string[] roots = { SDK_ROOT, SDK_PACKAGE_ROOT };

            foreach (var root in roots)
            {
                string assetPath = $"{root}/{relativePath}".Replace("\\", "/");

                if (AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(assetPath) != null)
                {
                    return true;
                }

                string absolutePath = Path.Combine(Application.dataPath, "..", assetPath);
                if (File.Exists(absolutePath))
                {
                    return true;
                }
            }

            return false;
        }

        private static bool SDKFolderExists(string relativePath)
        {
            string[] roots = { SDK_ROOT, SDK_PACKAGE_ROOT };

            foreach (var root in roots)
            {
                string assetPath = $"{root}/{relativePath}".Replace("\\", "/");
                if (AssetDatabase.IsValidFolder(assetPath))
                {
                    return true;
                }

                string absolutePath = Path.Combine(Application.dataPath, "..", assetPath);
                if (Directory.Exists(absolutePath))
                {
                    return true;
                }
            }

            return false;
        }

        private void DrawStatusBox(string title, (string name, bool exists)[] items)
        {
            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.LabelField(title, EditorStyles.boldLabel);
            
            foreach (var item in items)
            {
                EditorGUILayout.BeginHorizontal();
                GUILayout.Label(item.exists ? "✅" : "❌", GUILayout.Width(25));
                EditorGUILayout.LabelField(item.name);
                EditorGUILayout.EndHorizontal();
            }
            
            EditorGUILayout.EndVertical();
        }

        private void DrawCodeExample(string title, string code)
        {
            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.LabelField(title, EditorStyles.miniBoldLabel);
            
            var codeStyle = new GUIStyle(EditorStyles.textArea)
            {
                fontSize = 10,
                wordWrap = true,
                richText = false
            };
            
            EditorGUILayout.TextArea(code, codeStyle, GUILayout.MinHeight(80));
            
            if (GUILayout.Button("📋 Copy", GUILayout.Width(60)))
            {
                EditorGUIUtility.systemCopyBuffer = code;
            }
            
            EditorGUILayout.EndVertical();
            EditorGUILayout.Space(5);
        }

        private static Type FindType(string fullName)
        {
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                var type = assembly.GetType(fullName);
                if (type != null) return type;
            }
            return null;
        }

        private Canvas FindOrCreateCanvas()
        {
            var canvas = UnityEngine.Object.FindObjectOfType<Canvas>();
            if (canvas != null) return canvas;

            var go = new GameObject("Canvas");
            canvas = go.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            go.AddComponent<UnityEngine.UI.CanvasScaler>();
            go.AddComponent<UnityEngine.UI.GraphicRaycaster>();
            
            Undo.RegisterCreatedObjectUndo(go, "Create Canvas");
            return canvas;
        }

        private static bool CheckPackageInstalled(string packageId)
        {
            var manifestPath = Path.Combine(Application.dataPath, "..", "Packages", "manifest.json");
            if (File.Exists(manifestPath))
            {
                return File.ReadAllText(manifestPath).Contains(packageId);
            }
            return false;
        }

        #endregion
    }
}
