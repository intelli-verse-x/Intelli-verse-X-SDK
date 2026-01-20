// File: IVXSDKSetupWizard.cs
// Purpose: Comprehensive Unified SDK Setup Wizard for IntelliVerseX SDK
// Version: 2.1.0
// Author: IntelliVerseX Team
// Description: Single unified panel for ALL SDK module setup including Auth, Friends, Monetization, etc.
// Note: Supports both development (Assets/_IntelliVerseXSDK) and UPM package (Packages/com.intelliversex.sdk) installations.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace IntelliVerseX.Editor
{
    /// <summary>
    /// Unified SDK Setup Wizard - All SDK features in one place.
    /// Access via: IntelliVerseX → SDK Setup Wizard
    /// Supports both development and UPM package installations.
    /// </summary>
    public class IVXSDKSetupWizard : EditorWindow
    {
        #region Constants

        private const string WINDOW_TITLE = "IntelliVerseX SDK Setup";
        private const string SDK_VERSION = "2.1.0";
        private const string PACKAGE_NAME = "com.intelliversex.sdk";

        // Paths - These are relative paths within the SDK, resolved at runtime
        private const string SDK_ASSETS_ROOT = "Assets/_IntelliVerseXSDK";
        private const string SDK_PACKAGE_ROOT = "Packages/com.intelliversex.sdk";
        private const string QUIZVERSE_ROOT = "Assets/_QuizVerse";
        private const string RESOURCES_PATH = "Assets/Resources/IntelliVerseX";
        
        // Writable output path for UPM installs (prefabs, configs, generated assets)
        private const string GENERATED_ASSETS_ROOT = "Assets/IntelliVerseX/Generated";

        // Cached SDK root path (resolved at runtime)
        private static string _cachedSDKRoot = null;
        private static bool _isUPMInstall = false;

        // Dynamic paths (resolved using SDK_ROOT property)
        // For READ operations - use SDK_ROOT (works for both dev and UPM)
        private static string PREFABS_ROOT => SDK_ROOT + "/Prefabs";
        private static string MANAGERS_PREFAB_PATH => PREFABS_ROOT + "/Managers";
        private static string AUTH_ROOT => SDK_ROOT + "/Auth";
        private static string SOCIAL_ROOT => SDK_ROOT + "/Social";
        
        // For WRITE operations - use writable paths (Assets/ folder for UPM, SDK_ROOT for dev)
        private static string WRITABLE_AUTH_PREFABS_PATH => GetWritablePath("Auth/Prefabs");
        private static string WRITABLE_SOCIAL_PREFABS_PATH => GetWritablePath("Social/Prefabs");
        private static string WRITABLE_MANAGERS_PATH => GetWritablePath("Managers");
        
        // Legacy paths (kept for backward compatibility, redirect to writable paths)
        private static string AUTH_PREFABS_PATH => WRITABLE_AUTH_PREFABS_PATH;
        private static string SOCIAL_PREFABS_PATH => WRITABLE_SOCIAL_PREFABS_PATH;
        
        /// <summary>
        /// Gets a writable path for the given sub-folder.
        /// For UPM installs: Assets/IntelliVerseX/Generated/{subFolder}
        /// For dev installs: Assets/_IntelliVerseXSDK/{subFolder}
        /// </summary>
        private static string GetWritablePath(string subFolder)
        {
            if (_isUPMInstall)
            {
                return $"{GENERATED_ASSETS_ROOT}/{subFolder}";
            }
            return $"{SDK_ASSETS_ROOT}/{subFolder}";
        }
        
        /// <summary>
        /// Checks if we're in a UPM install context.
        /// </summary>
        public static bool IsUPMInstall
        {
            get
            {
                if (_cachedSDKRoot == null)
                {
                    ResolveSDKRoot();
                }
                return _isUPMInstall;
            }
        }

        /// <summary>
        /// Gets the SDK root path, automatically detecting whether this is a development
        /// project (Assets/_IntelliVerseXSDK) or a UPM package installation (Packages/com.intelliversex.sdk).
        /// </summary>
        private static string SDK_ROOT
        {
            get
            {
                if (_cachedSDKRoot == null)
                {
                    ResolveSDKRoot();
                }
                return _cachedSDKRoot;
            }
        }

        /// <summary>
        /// Resolves the SDK root path by checking both possible locations.
        /// Prioritizes UPM package path for consumer projects.
        /// </summary>
        private static void ResolveSDKRoot()
        {
            // First, check if SDK is installed as UPM package
            string packagePath = GetPackagePath(PACKAGE_NAME);
            if (!string.IsNullOrEmpty(packagePath))
            {
                _cachedSDKRoot = packagePath;
                _isUPMInstall = true;
                Debug.Log($"[IVXSDKSetupWizard] SDK detected as UPM package at: {_cachedSDKRoot}");
                return;
            }

            // Fallback to Assets folder (development mode)
            if (Directory.Exists(SDK_ASSETS_ROOT))
            {
                _cachedSDKRoot = SDK_ASSETS_ROOT;
                _isUPMInstall = false;
                Debug.Log($"[IVXSDKSetupWizard] SDK detected in Assets folder at: {_cachedSDKRoot}");
                return;
            }

            // Default to Assets path if nothing found (will show as missing)
            _cachedSDKRoot = SDK_ASSETS_ROOT;
            _isUPMInstall = false;
            Debug.LogWarning("[IVXSDKSetupWizard] SDK not found in expected locations. Please verify installation.");
        }

        /// <summary>
        /// Gets the physical path to a UPM package.
        /// </summary>
        private static string GetPackagePath(string packageName)
        {
            // Try using Unity's Package Manager API
            try
            {
                // Check if package exists in Packages folder
                string packagesPath = Path.Combine(Application.dataPath, "..", "Packages", packageName);
                if (Directory.Exists(packagesPath))
                {
                    return $"Packages/{packageName}";
                }

                // Check Library/PackageCache for resolved packages
                string packageCachePath = Path.Combine(Application.dataPath, "..", "Library", "PackageCache");
                if (Directory.Exists(packageCachePath))
                {
                    var dirs = Directory.GetDirectories(packageCachePath, $"{packageName}@*");
                    if (dirs.Length > 0)
                    {
                        // Return the Unity-style path for package
                        return $"Packages/{packageName}";
                    }
                }

                // Also check manifest.json for the package
                string manifestPath = Path.Combine(Application.dataPath, "..", "Packages", "manifest.json");
                if (File.Exists(manifestPath))
                {
                    string manifestContent = File.ReadAllText(manifestPath);
                    if (manifestContent.Contains($"\"{packageName}\""))
                    {
                        return $"Packages/{packageName}";
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[IVXSDKSetupWizard] Error checking package path: {ex.Message}");
            }

            return null;
        }

        /// <summary>
        /// Checks if a file exists within the SDK, handling both development and UPM paths.
        /// </summary>
        private static bool SDKFileExists(string relativePath)
        {
            // For UPM packages, we need to use AssetDatabase or check the resolved path
            string fullPath = Path.Combine(SDK_ROOT, relativePath);
            
            // Try AssetDatabase first (works for both Assets and Packages)
            string assetPath = fullPath.Replace("\\", "/");
            if (AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(assetPath) != null)
            {
                return true;
            }

            // For UPM packages, also check the physical file system
            if (_isUPMInstall)
            {
                string physicalPath = GetPhysicalPathForPackageFile(relativePath);
                if (!string.IsNullOrEmpty(physicalPath) && File.Exists(physicalPath))
                {
                    return true;
                }
            }
            else
            {
                // For Assets folder, direct file check
                string absolutePath = Path.Combine(Application.dataPath, "..", fullPath);
                if (File.Exists(absolutePath))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Gets the physical file system path for a file in the SDK package.
        /// </summary>
        private static string GetPhysicalPathForPackageFile(string relativePath)
        {
            try
            {
                // Check Library/PackageCache
                string packageCachePath = Path.Combine(Application.dataPath, "..", "Library", "PackageCache");
                if (Directory.Exists(packageCachePath))
                {
                    var dirs = Directory.GetDirectories(packageCachePath, $"{PACKAGE_NAME}@*");
                    if (dirs.Length > 0)
                    {
                        string packageDir = dirs[0];
                        string filePath = Path.Combine(packageDir, relativePath);
                        return filePath;
                    }
                }

                // Check local Packages folder (for local development packages)
                string localPackagePath = Path.Combine(Application.dataPath, "..", "Packages", PACKAGE_NAME, relativePath);
                if (File.Exists(localPackagePath))
                {
                    return localPackagePath;
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[IVXSDKSetupWizard] Error resolving package file path: {ex.Message}");
            }

            return null;
        }

        /// <summary>
        /// Checks if a type exists by searching all loaded assemblies.
        /// This is the most reliable way to check if SDK scripts are available.
        /// </summary>
        private static bool TypeExists(string fullTypeName)
        {
            return GetTypeByName(fullTypeName) != null;
        }

        /// <summary>
        /// Forces a refresh of the cached SDK root path.
        /// Call this if the SDK installation changes.
        /// </summary>
        private static void RefreshSDKPath()
        {
            _cachedSDKRoot = null;
            ResolveSDKRoot();
        }

        #endregion

        #region Module States

        [Serializable]
        public class ModuleSetupState
        {
            public bool isExpanded = false;
            public bool isSetupComplete = false;
            public string statusMessage = "";
            public List<string> setupSteps = new List<string>();
            public List<bool> stepCompleted = new List<bool>();
        }

        // Core Modules
        private ModuleSetupState coreModule = new ModuleSetupState();
        private ModuleSetupState identityModule = new ModuleSetupState();
        private ModuleSetupState backendModule = new ModuleSetupState();

        // Auth Module
        private ModuleSetupState authModule = new ModuleSetupState();

        // Social Module (Friends)
        private ModuleSetupState friendsModule = new ModuleSetupState();

        // Feature Modules
        private ModuleSetupState walletModule = new ModuleSetupState();
        private ModuleSetupState leaderboardModule = new ModuleSetupState();
        private ModuleSetupState socialModule = new ModuleSetupState();
        private ModuleSetupState quizModule = new ModuleSetupState();
        private ModuleSetupState localizationModule = new ModuleSetupState();

        // Monetization Modules
        private ModuleSetupState adsModule = new ModuleSetupState();
        private ModuleSetupState iapModule = new ModuleSetupState();
        private ModuleSetupState retentionModule = new ModuleSetupState();

        #endregion

        #region UI State

        private Vector2 scrollPosition;
        private int selectedTab = 0;
        private readonly string[] tabNames = new string[]
        {
            "Quick Setup",
            "Core",
            "Auth & Social",
            "Features",
            "Monetization",
            "More Of Us",
            "Test Scenes"
        };

        // Styles
        private GUIStyle headerStyle;
        private GUIStyle subHeaderStyle;
        private GUIStyle moduleBoxStyle;
        private GUIStyle successStyle;
        private GUIStyle warningStyle;
        private GUIStyle buttonStyle;
        private bool stylesInitialized = false;

        // Colors
        private readonly Color successColor = new Color(0.2f, 0.8f, 0.2f);
        private readonly Color warningColor = new Color(0.9f, 0.7f, 0.1f);
        private readonly Color accentColor = new Color(0.25f, 0.52f, 0.96f);

        #endregion

        #region Window Management

        [MenuItem("IntelliVerseX/SDK Setup Wizard", false, 0)]
        public static void ShowWindow()
        {
            var window = GetWindow<IVXSDKSetupWizard>(WINDOW_TITLE);
            window.minSize = new Vector2(650, 750);
            window.Show();
        }

        private void OnEnable()
        {
            InitializeModuleStates();
            // Delay refresh to avoid issues during editor startup
            EditorApplication.delayCall += () =>
            {
                if (this != null)
                {
                    RefreshAllModuleStatus();
                    Repaint();
                }
            };
        }

        private void OnFocus()
        {
            // Ensure module states are initialized before refresh
            if (coreModule.setupSteps == null || coreModule.setupSteps.Count == 0)
            {
                InitializeModuleStates();
            }
            RefreshAllModuleStatus();
        }

        #endregion

        #region Initialization

        private void InitializeStyles()
        {
            if (stylesInitialized) return;

            headerStyle = new GUIStyle(EditorStyles.boldLabel)
            {
                fontSize = 18,
                alignment = TextAnchor.MiddleCenter,
                margin = new RectOffset(0, 0, 10, 10)
            };

            subHeaderStyle = new GUIStyle(EditorStyles.boldLabel)
            {
                fontSize = 14,
                margin = new RectOffset(0, 0, 5, 5)
            };

            moduleBoxStyle = new GUIStyle("box")
            {
                padding = new RectOffset(15, 15, 10, 10),
                margin = new RectOffset(5, 5, 5, 5)
            };

            successStyle = new GUIStyle(EditorStyles.label)
            {
                normal = { textColor = successColor }
            };

            warningStyle = new GUIStyle(EditorStyles.label)
            {
                normal = { textColor = warningColor }
            };

            buttonStyle = new GUIStyle(GUI.skin.button)
            {
                fontStyle = FontStyle.Bold
            };

            stylesInitialized = true;
        }

        private void InitializeModuleStates()
        {
            // Core Module
            coreModule.setupSteps = new List<string>
            {
                "IntelliVerseXManager exists",
                "Core assembly definition configured",
                "SDK configuration asset created"
            };
            coreModule.stepCompleted = new List<bool>(new bool[coreModule.setupSteps.Count]);

            // Identity Module
            identityModule.setupSteps = new List<string>
            {
                "UserSessionManager script exists",
                "APIManager script exists",
                "Identity assembly definition configured"
            };
            identityModule.stepCompleted = new List<bool>(new bool[identityModule.setupSteps.Count]);

            // Backend Module
            backendModule.setupSteps = new List<string>
            {
                "IVXNakamaManager exists",
                "IVXBackendService configured",
                "NakamaManager prefab created"
            };
            backendModule.stepCompleted = new List<bool>(new bool[backendModule.setupSteps.Count]);

            // Auth Module
            authModule.setupSteps = new List<string>
            {
                "AuthConfig asset created",
                "IVX_AuthCanvas prefab exists",
                "Login/Register panels configured",
                "OTP panel configured"
            };
            authModule.stepCompleted = new List<bool>(new bool[authModule.setupSteps.Count]);

            // Friends Module
            friendsModule.setupSteps = new List<string>
            {
                "FriendsConfig asset created",
                "IVXFriendSlot prefab exists",
                "IVXFriendRequestSlot prefab exists",
                "IVXFriendSearchSlot prefab exists",
                "IVXFriendsPanel configured"
            };
            friendsModule.stepCompleted = new List<bool>(new bool[friendsModule.setupSteps.Count]);

            // Wallet Module
            walletModule.setupSteps = new List<string>
            {
                "IVXGWalletManager exists",
                "Wallet display prefab configured"
            };
            walletModule.stepCompleted = new List<bool>(new bool[walletModule.setupSteps.Count]);

            // Leaderboard Module
            leaderboardModule.setupSteps = new List<string>
            {
                "IVXGLeaderboardManager exists",
                "Leaderboard UI configured"
            };
            leaderboardModule.stepCompleted = new List<bool>(new bool[leaderboardModule.setupSteps.Count]);

            // Social Module (Share & Rate)
            socialModule.setupSteps = new List<string>
            {
                "IVXGShareManager exists",
                "IVXGRateAppManager exists"
            };
            socialModule.stepCompleted = new List<bool>(new bool[socialModule.setupSteps.Count]);

            // Quiz Module
            quizModule.setupSteps = new List<string>
            {
                "IVXQuizSessionManager exists",
                "Quiz UI components configured"
            };
            quizModule.stepCompleted = new List<bool>(new bool[quizModule.setupSteps.Count]);

            // Localization Module
            localizationModule.setupSteps = new List<string>
            {
                "IVXLocalizationService exists",
                "Language files configured"
            };
            localizationModule.stepCompleted = new List<bool>(new bool[localizationModule.setupSteps.Count]);

            // Ads Module
            adsModule.setupSteps = new List<string>
            {
                "IVXAdsManager exists",
                "Ad network configured"
            };
            adsModule.stepCompleted = new List<bool>(new bool[adsModule.setupSteps.Count]);

            // IAP Module
            iapModule.setupSteps = new List<string>
            {
                "IVXIAPManager exists",
                "Product catalog configured"
            };
            iapModule.stepCompleted = new List<bool>(new bool[iapModule.setupSteps.Count]);

            // Retention Module
            retentionModule.setupSteps = new List<string>
            {
                "Daily rewards configured",
                "Streak system configured"
            };
            retentionModule.stepCompleted = new List<bool>(new bool[retentionModule.setupSteps.Count]);
        }

        #endregion

        #region Main GUI

        private void OnGUI()
        {
            InitializeStyles();

            EditorGUILayout.Space(10);
            DrawHeader();
            EditorGUILayout.Space(5);

            // Tab Bar
            selectedTab = GUILayout.Toolbar(selectedTab, tabNames, GUILayout.Height(30));
            EditorGUILayout.Space(10);

            // Content
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

            switch (selectedTab)
            {
                case 0: DrawQuickSetupTab(); break;
                case 1: DrawCoreTab(); break;
                case 2: DrawAuthSocialTab(); break;
                case 3: DrawFeaturesTab(); break;
                case 4: DrawMonetizationTab(); break;
                case 5: DrawMoreOfUsTab(); break;
                case 6: DrawTestScenesTab(); break;
            }

            EditorGUILayout.EndScrollView();

            DrawFooter();
        }

        private void DrawHeader()
        {
            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();

            EditorGUILayout.BeginVertical();
            GUILayout.Label("🎮 IntelliVerseX SDK Setup Wizard", headerStyle);
            
            // Show installation type (UPM Package vs Development)
            string installType = _isUPMInstall ? "📦 UPM Package" : "🔧 Development";
            GUILayout.Label($"Version {SDK_VERSION} | {installType}", EditorStyles.centeredGreyMiniLabel);
            EditorGUILayout.EndVertical();

            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
        }

        private void DrawFooter()
        {
            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button("📖 Documentation", GUILayout.Height(25)))
            {
                Application.OpenURL("https://docs.intelliverse-x.ai");
            }

            if (GUILayout.Button("🔄 Refresh Status", GUILayout.Height(25)))
            {
                RefreshAllModuleStatus();
                Repaint();
            }

            if (GUILayout.Button("✅ Verify All", GUILayout.Height(25)))
            {
                VerifyFullSetup();
            }

            EditorGUILayout.EndHorizontal();
        }

        #endregion

        #region Quick Setup Tab

        private void DrawQuickSetupTab()
        {
            EditorGUILayout.LabelField("Quick Setup", subHeaderStyle ?? EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(
                "Use Quick Setup to automatically configure SDK modules. " +
                "For individual module control, use the specific tabs.",
                MessageType.Info);

            EditorGUILayout.Space(10);

            // One-Click Full Setup
            EditorGUILayout.BeginVertical(moduleBoxStyle);
            EditorGUILayout.LabelField("🚀 One-Click Full Setup", EditorStyles.boldLabel);
            EditorGUILayout.LabelField("Sets up ALL SDK modules with default configuration", EditorStyles.wordWrappedMiniLabel);

            EditorGUILayout.Space(5);

            GUI.backgroundColor = accentColor;
            if (GUILayout.Button("Setup All Modules", GUILayout.Height(40)))
            {
                if (EditorUtility.DisplayDialog("Full SDK Setup",
                    "This will set up all SDK modules:\n\n" +
                    "• Core & Identity\n" +
                    "• Authentication\n" +
                    "• Friends System\n" +
                    "• Wallet & Leaderboard\n" +
                    "• Monetization\n\n" +
                    "Continue?", "Yes, Setup All", "Cancel"))
                {
                    SetupAllModules();
                }
            }
            GUI.backgroundColor = Color.white;
            EditorGUILayout.EndVertical();

            EditorGUILayout.Space(10);

            // Individual Quick Actions
            EditorGUILayout.LabelField("Individual Quick Actions", subHeaderStyle ?? EditorStyles.boldLabel);

            DrawQuickActionButton("🔐 Setup Authentication",
                "Creates AuthConfig, prefabs, and wires UI components",
                SetupAuthModule);

            DrawQuickActionButton("👥 Setup Friends System",
                "Creates FriendsConfig, slot prefabs, and panel UI",
                SetupFriendsModule);

            DrawQuickActionButton("💰 Setup Wallet & Leaderboard",
                "Configures wallet display and leaderboard managers",
                SetupWalletAndLeaderboard);

            DrawQuickActionButton("📺 Setup Monetization",
                "Configures ads and IAP systems",
                SetupMonetization);

            DrawQuickActionButton("🌍 Setup Localization",
                "Configures multi-language support",
                SetupLocalization);
        }

        private void DrawQuickActionButton(string title, string description, Action action)
        {
            EditorGUILayout.BeginVertical(moduleBoxStyle);
            EditorGUILayout.BeginHorizontal();

            EditorGUILayout.BeginVertical();
            EditorGUILayout.LabelField(title, EditorStyles.boldLabel);
            EditorGUILayout.LabelField(description, EditorStyles.wordWrappedMiniLabel);
            EditorGUILayout.EndVertical();

            if (GUILayout.Button("Setup", GUILayout.Width(80), GUILayout.Height(35)))
            {
                action?.Invoke();
            }

            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();
        }

        #endregion

        #region Core Tab

        private void DrawCoreTab()
        {
            EditorGUILayout.LabelField("Core SDK Modules", subHeaderStyle ?? EditorStyles.boldLabel);

            DrawModuleSection("🎯 Core Manager", coreModule, CheckCoreModule,
                "Central SDK manager and configuration");

            DrawModuleSection("[ID] Identity System", identityModule, CheckIdentityModule,
                "User session management and API integration");

            DrawModuleSection("🔗 Backend Services", backendModule, CheckBackendModule,
                "Nakama backend connection and services");

            EditorGUILayout.Space(10);

            // Core Actions
            EditorGUILayout.BeginVertical(moduleBoxStyle);
            EditorGUILayout.LabelField("Core Actions", EditorStyles.boldLabel);

            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button("Create SDK Config", GUILayout.Height(30)))
            {
                CreateSDKConfigAsset();
            }

            if (GUILayout.Button("Add Managers to Scene", GUILayout.Height(30)))
            {
                AddManagersToCurrentScene();
            }

            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();
        }

        #endregion

        #region Auth & Social Tab

        private void DrawAuthSocialTab()
        {
            EditorGUILayout.LabelField("Authentication & Social", subHeaderStyle ?? EditorStyles.boldLabel);

            // Auth Section
            DrawExpandedModuleSection("🔐 Authentication Module", authModule, CheckAuthModule,
                "Login, Register, OTP, Guest, and Social auth",
                DrawAuthModuleActions);

            EditorGUILayout.Space(10);

            // Friends Section
            DrawExpandedModuleSection("👥 Friends Module", friendsModule, CheckFriendsModule,
                "Friend list, requests, search, and social features",
                DrawFriendsModuleActions);
        }

        private void DrawAuthModuleActions()
        {
            EditorGUILayout.Space(5);
            EditorGUILayout.LabelField("Auth Actions:", EditorStyles.miniBoldLabel);

            // Store button states to process actions after layout
            bool createConfig = false;
            bool createPrefabs = false;
            bool addToScene = false;
            bool createDemoScene = false;
            bool editConfig = false;

            EditorGUILayout.BeginHorizontal();
            createConfig = GUILayout.Button("Create Auth Config", GUILayout.Height(25));
            createPrefabs = GUILayout.Button("Create Auth Prefabs", GUILayout.Height(25));
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            addToScene = GUILayout.Button("Add Auth Canvas to Scene", GUILayout.Height(25));
            createDemoScene = GUILayout.Button("Create Auth Demo Scene", GUILayout.Height(25));
            EditorGUILayout.EndHorizontal();

            // Auth Config Quick Settings
            var authConfig = Resources.Load<ScriptableObject>("IntelliVerseX/AuthConfig");
            if (authConfig != null)
            {
                EditorGUILayout.Space(5);
                EditorGUILayout.LabelField("Auth Config:", EditorStyles.miniBoldLabel);

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.ObjectField("Config Asset:", authConfig, typeof(ScriptableObject), false);
                editConfig = GUILayout.Button("Edit", GUILayout.Width(50));
                EditorGUILayout.EndHorizontal();
            }

            // Process button actions after all GUI layout is complete
            if (createConfig) EditorApplication.delayCall += CreateAuthConfig;
            if (createPrefabs) EditorApplication.delayCall += CreateAuthPrefabs;
            if (addToScene) EditorApplication.delayCall += AddAuthCanvasToScene;
            if (createDemoScene) CreateAuthDemoScene();
            if (editConfig && authConfig != null) Selection.activeObject = authConfig;
        }

        private void DrawFriendsModuleActions()
        {
            EditorGUILayout.Space(5);
            EditorGUILayout.LabelField("Friends Actions:", EditorStyles.miniBoldLabel);

            // Store button states to process actions after layout
            bool createConfig = false;
            bool createPrefabs = false;
            bool addToScene = false;
            bool createDemoScene = false;
            bool openDOTweenStore = false;

            EditorGUILayout.BeginHorizontal();
            createConfig = GUILayout.Button("Create Friends Config", GUILayout.Height(25));
            createPrefabs = GUILayout.Button("Create Slot Prefabs", GUILayout.Height(25));
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            addToScene = GUILayout.Button("Add Friends UI to Scene", GUILayout.Height(25));
            createDemoScene = GUILayout.Button("Create Friends Demo Scene", GUILayout.Height(25));
            EditorGUILayout.EndHorizontal();

            // DOTween Check
            bool hasDOTween = CheckDOTweenInstalled();
            if (!hasDOTween)
            {
                EditorGUILayout.Space(5);
                EditorGUILayout.HelpBox(
                    "DOTween is recommended for Friends UI animations. Install from Asset Store.",
                    MessageType.Warning);

                openDOTweenStore = GUILayout.Button("Open DOTween Asset Store", GUILayout.Height(22));
            }

            // Process button actions after all GUI layout is complete
            if (createConfig) EditorApplication.delayCall += CreateFriendsConfig;
            if (createPrefabs) EditorApplication.delayCall += CreateFriendsPrefabs;
            if (addToScene) EditorApplication.delayCall += AddFriendsUIToScene;
            if (createDemoScene) CreateFriendsDemoScene();
            if (openDOTweenStore) Application.OpenURL("https://assetstore.unity.com/packages/tools/animation/dotween-hotween-v2-27676");
        }

        #endregion

        #region Features Tab

        private void DrawFeaturesTab()
        {
            EditorGUILayout.LabelField("Game Feature Modules", subHeaderStyle ?? EditorStyles.boldLabel);

            DrawExpandedModuleSection("💰 Wallet System", walletModule, CheckWalletModule,
                "Dual-wallet system for game and global currency",
                DrawWalletModuleActions);

            DrawExpandedModuleSection("🏆 Leaderboard", leaderboardModule, CheckLeaderboardModule,
                "Daily, weekly, monthly, and all-time leaderboards",
                DrawLeaderboardModuleActions);

            DrawExpandedModuleSection("📤 Share & Rate", socialModule, CheckSocialModule,
                "Native sharing and in-app rating features",
                DrawSocialModuleActions);

            DrawModuleSection("❓ Quiz System", quizModule, CheckQuizModule,
                "Quiz session management and UI components");

            DrawModuleSection("🌍 Localization", localizationModule, CheckLocalizationModule,
                "Multi-language support with RTL");

            EditorGUILayout.Space(10);

            // Feature Actions
            EditorGUILayout.BeginVertical(moduleBoxStyle);
            EditorGUILayout.LabelField("Quick Actions", EditorStyles.boldLabel);

            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button("Setup Wallet UI", GUILayout.Height(25)))
            {
                EditorApplication.delayCall += SetupWalletUI;
            }

            if (GUILayout.Button("Setup Leaderboard UI", GUILayout.Height(25)))
            {
                EditorApplication.delayCall += SetupLeaderboardUI;
            }

            if (GUILayout.Button("Setup Social Features", GUILayout.Height(25)))
            {
                EditorApplication.delayCall += SetupSocialFeatures;
            }

            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();
        }

        private void DrawWalletModuleActions()
        {
            EditorGUILayout.Space(5);
            EditorGUILayout.LabelField("Wallet Actions:", EditorStyles.miniBoldLabel);

            EditorGUILayout.BeginHorizontal();
            
            bool createPrefabs = GUILayout.Button("Create Prefabs", GUILayout.Height(25));
            bool addToScene = GUILayout.Button("Add to Scene", GUILayout.Height(25));

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();

            bool validateSetup = GUILayout.Button("Validate Setup", GUILayout.Height(25));
            bool createTestScene = GUILayout.Button("Create Test Scene", GUILayout.Height(25));

            EditorGUILayout.EndHorizontal();

            // Show prefab references if they exist
            var managerPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(SDK_ROOT + "/Wallet/Prefabs/IVXGWalletManager.prefab");
            var displayPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(SDK_ROOT + "/Wallet/Prefabs/IVXGWalletDisplay.prefab");

            if (managerPrefab != null || displayPrefab != null)
            {
                EditorGUILayout.Space(5);
                EditorGUILayout.LabelField("Prefabs:", EditorStyles.miniBoldLabel);

                if (managerPrefab != null)
                {
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.ObjectField("Manager:", managerPrefab, typeof(GameObject), false);
                    bool selectManager = GUILayout.Button("Select", GUILayout.Width(50));
                    EditorGUILayout.EndHorizontal();
                    
                    if (selectManager)
                    {
                        Selection.activeObject = managerPrefab;
                        EditorGUIUtility.PingObject(managerPrefab);
                    }
                }

                if (displayPrefab != null)
                {
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.ObjectField("Display:", displayPrefab, typeof(GameObject), false);
                    bool selectDisplay = GUILayout.Button("Select", GUILayout.Width(50));
                    EditorGUILayout.EndHorizontal();
                    
                    if (selectDisplay)
                    {
                        Selection.activeObject = displayPrefab;
                        EditorGUIUtility.PingObject(displayPrefab);
                    }
                }
            }

            // Process button actions after all GUI layout is complete
            if (createPrefabs)
            {
                EditorApplication.delayCall += CreateWalletPrefabs;
            }
            
            if (addToScene)
            {
                EditorApplication.delayCall += SetupWalletUI;
            }
            
            if (validateSetup)
            {
                EditorApplication.delayCall += () =>
                {
                    CheckWalletModule();
                    string message = walletModule.isSetupComplete
                        ? "✅ Wallet module is fully configured!"
                        : "⚠️ Wallet module setup incomplete.\n\nMissing:\n" +
                          (!walletModule.stepCompleted[0] ? "• IVXGWalletManager script\n" : "") +
                          (!walletModule.stepCompleted[1] ? "• Wallet UI configured\n" : "");
                    EditorUtility.DisplayDialog("Wallet Validation", message, "OK");
                    Repaint();
                };
            }
            
            if (createTestScene)
            {
                EditorApplication.delayCall += CreateWalletTestScene;
            }
        }

        private void DrawSocialModuleActions()
        {
            EditorGUILayout.Space(5);
            EditorGUILayout.LabelField("Social Actions:", EditorStyles.miniBoldLabel);

            EditorGUILayout.BeginHorizontal();
            
            bool createPrefabs = GUILayout.Button("Create Prefabs", GUILayout.Height(25));
            bool addShareToScene = GUILayout.Button("Add Share", GUILayout.Height(25));
            bool addRateToScene = GUILayout.Button("Add Rate App", GUILayout.Height(25));

            EditorGUILayout.EndHorizontal();

            // Show prefab references if they exist
            var sharePrefab = AssetDatabase.LoadAssetAtPath<GameObject>(SDK_ROOT + "/Social/Prefabs/IVXGShareManager.prefab");
            var ratePrefab = AssetDatabase.LoadAssetAtPath<GameObject>(SDK_ROOT + "/Social/Prefabs/IVXGRateAppManager.prefab");

            if (sharePrefab != null || ratePrefab != null)
            {
                EditorGUILayout.Space(5);
                EditorGUILayout.LabelField("Prefabs:", EditorStyles.miniBoldLabel);

                if (sharePrefab != null)
                {
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.ObjectField("Share:", sharePrefab, typeof(GameObject), false);
                    bool selectShare = GUILayout.Button("Select", GUILayout.Width(50));
                    EditorGUILayout.EndHorizontal();
                    
                    if (selectShare)
                    {
                        Selection.activeObject = sharePrefab;
                        EditorGUIUtility.PingObject(sharePrefab);
                    }
                }

                if (ratePrefab != null)
                {
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.ObjectField("Rate:", ratePrefab, typeof(GameObject), false);
                    bool selectRate = GUILayout.Button("Select", GUILayout.Width(50));
                    EditorGUILayout.EndHorizontal();
                    
                    if (selectRate)
                    {
                        Selection.activeObject = ratePrefab;
                        EditorGUIUtility.PingObject(ratePrefab);
                    }
                }
            }

            // Process button actions after all GUI layout is complete
            if (createPrefabs)
            {
                EditorApplication.delayCall += CreateSocialPrefabs;
            }
            
            if (addShareToScene)
            {
                EditorApplication.delayCall += AddShareToScene;
            }
            
            if (addRateToScene)
            {
                EditorApplication.delayCall += AddRateAppToScene;
            }
        }

        private void DrawLeaderboardModuleActions()
        {
            EditorGUILayout.Space(5);
            EditorGUILayout.LabelField("Leaderboard Actions:", EditorStyles.miniBoldLabel);

            EditorGUILayout.BeginHorizontal();
            
            bool createPrefabs = GUILayout.Button("Create Prefabs", GUILayout.Height(25));
            bool addToScene = GUILayout.Button("Add to Scene", GUILayout.Height(25));

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();

            bool validateSetup = GUILayout.Button("Validate Setup", GUILayout.Height(25));
            bool createTestScene = GUILayout.Button("Create Test Scene", GUILayout.Height(25));

            EditorGUILayout.EndHorizontal();

            // Show prefab references if they exist
            var canvasPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(SDK_ROOT + "/Leaderboard/Prefabs/IVXGLeaderboardCanvas.prefab");
            var entryPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(SDK_ROOT + "/Leaderboard/Prefabs/IVXGLeaderboardEntry.prefab");

            if (canvasPrefab != null || entryPrefab != null)
            {
                EditorGUILayout.Space(5);
                EditorGUILayout.LabelField("Prefabs:", EditorStyles.miniBoldLabel);

                if (canvasPrefab != null)
                {
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.ObjectField("Canvas Prefab:", canvasPrefab, typeof(GameObject), false);
                    bool selectCanvas = GUILayout.Button("Select", GUILayout.Width(50));
                    EditorGUILayout.EndHorizontal();
                    
                    if (selectCanvas)
                    {
                        Selection.activeObject = canvasPrefab;
                        EditorGUIUtility.PingObject(canvasPrefab);
                    }
                }

                if (entryPrefab != null)
                {
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.ObjectField("Entry Prefab:", entryPrefab, typeof(GameObject), false);
                    bool selectEntry = GUILayout.Button("Select", GUILayout.Width(50));
                    EditorGUILayout.EndHorizontal();
                    
                    if (selectEntry)
                    {
                        Selection.activeObject = entryPrefab;
                        EditorGUIUtility.PingObject(entryPrefab);
                    }
                }
            }

            // Process button actions after all GUI layout is complete
            if (createPrefabs)
            {
                EditorApplication.delayCall += CreateLeaderboardPrefabs;
            }
            
            if (addToScene)
            {
                EditorApplication.delayCall += SetupLeaderboardUI;
            }
            
            if (validateSetup)
            {
                EditorApplication.delayCall += () =>
                {
                    CheckLeaderboardModule();
                    string message = leaderboardModule.isSetupComplete
                        ? "✅ Leaderboard module is fully configured!"
                        : "⚠️ Leaderboard module setup incomplete.\n\nMissing:\n" +
                          (!leaderboardModule.stepCompleted[0] ? "• IVXGLeaderboardManager script\n" : "") +
                          (!leaderboardModule.stepCompleted[1] ? "• Leaderboard UI configured\n" : "");
                    EditorUtility.DisplayDialog("Leaderboard Validation", message, "OK");
                    Repaint();
                };
            }
            
            if (createTestScene)
            {
                EditorApplication.delayCall += CreateLeaderboardTestScene;
            }
        }

        private void CreateLeaderboardPrefabs()
        {
            var builderType = GetTypeByName("IntelliVerseX.Games.Leaderboard.Editor.IVXGLeaderboardPrefabBuilder");
            if (builderType != null)
            {
                var createAllMethod = builderType.GetMethod("CreateAllPrefabs",
                    System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);

                if (createAllMethod != null)
                {
                    createAllMethod.Invoke(null, null);
                    CheckLeaderboardModule();
                    return;
                }
            }

            Debug.LogWarning("[IVXSDKSetupWizard] IVXGLeaderboardPrefabBuilder not found");
            EditorUtility.DisplayDialog("Error",
                "Leaderboard Prefab Builder not found.\n\nMake sure the leaderboard module is properly installed.",
                "OK");
        }

        #endregion

        #region Monetization Tab

        private void DrawMonetizationTab()
        {
            EditorGUILayout.LabelField("Monetization Modules", subHeaderStyle ?? EditorStyles.boldLabel);

            DrawModuleSection("📺 Ads System", adsModule, CheckAdsModule,
                "Ad mediation with LevelPlay/Appodeal support");

            DrawModuleSection("💳 In-App Purchases", iapModule, CheckIAPModule,
                "IAP with Unity Purchasing integration");

            DrawModuleSection("🎁 Retention Features", retentionModule, CheckRetentionModule,
                "Daily rewards, streaks, and session boosters");

            EditorGUILayout.Space(10);

            // Monetization Actions
            EditorGUILayout.BeginVertical(moduleBoxStyle);
            EditorGUILayout.LabelField("Monetization Actions", EditorStyles.boldLabel);

            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button("Configure Ads", GUILayout.Height(25)))
            {
                ConfigureAds();
            }

            if (GUILayout.Button("Configure IAP", GUILayout.Height(25)))
            {
                ConfigureIAP();
            }

            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();
        }

        #endregion

        #region More Of Us Tab

        private void DrawMoreOfUsTab()
        {
            EditorGUILayout.LabelField("More Of Us - Cross-Promotion", subHeaderStyle ?? EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(
                "The 'More Of Us' feature displays a Netflix-style carousel showcasing your other apps. " +
                "It automatically fetches app data from your S3 catalog and displays them with smooth animations.",
                MessageType.Info);

            EditorGUILayout.Space(10);

            // Configuration Section
            EditorGUILayout.BeginVertical(moduleBoxStyle);
            EditorGUILayout.LabelField("Configuration", EditorStyles.boldLabel);

            EditorGUILayout.LabelField("Android Catalog URL:", EditorStyles.miniLabel);
            EditorGUILayout.SelectableLabel(
                "https://intelli-verse-x-media.s3.us-east-1.amazonaws.com/app-catalog/unified/intelliversex/android.json",
                EditorStyles.textField, GUILayout.Height(20));

            EditorGUILayout.Space(5);
            EditorGUILayout.LabelField("iOS Catalog URL:", EditorStyles.miniLabel);
            EditorGUILayout.SelectableLabel(
                "https://intelli-verse-x-media.s3.us-east-1.amazonaws.com/app-catalog/unified/intelliversex/ios.json",
                EditorStyles.textField, GUILayout.Height(20));

            EditorGUILayout.EndVertical();

            EditorGUILayout.Space(10);

            // Prefab Creation Section
            EditorGUILayout.BeginVertical(moduleBoxStyle);
            EditorGUILayout.LabelField("UI Prefabs", EditorStyles.boldLabel);
            EditorGUILayout.LabelField(
                "Create production-ready UI prefabs with Netflix-style hover animations and carousel scrolling.",
                EditorStyles.wordWrappedMiniLabel);

            EditorGUILayout.Space(10);
            EditorGUILayout.BeginHorizontal();

            GUI.backgroundColor = accentColor;
            if (GUILayout.Button("Build All Prefabs", GUILayout.Height(35)))
            {
                try
                {
                    // Call the prefab builder
                    var builderType = System.Type.GetType("IntelliVerseX.MoreOfUs.Editor.IVXMoreOfUsPrefabBuilder, IntelliVerseX.MoreOfUs.Editor");
                    if (builderType != null)
                    {
                        var method = builderType.GetMethod("BuildAllPrefabs", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
                        method?.Invoke(null, null);
                    }
                    else
                    {
                        EditorUtility.DisplayDialog("Info", "Please ensure the MoreOfUs assembly is compiled first.", "OK");
                    }
                }
                catch (System.Exception ex)
                {
                    Debug.LogError($"[IVXSDKSetupWizard] Failed to build More Of Us prefabs: {ex.Message}");
                    EditorUtility.DisplayDialog("Error", $"Failed to build prefabs: {ex.Message}", "OK");
                }
            }
            GUI.backgroundColor = Color.white;

            if (GUILayout.Button("Add To Scene", GUILayout.Height(35)))
            {
                try
                {
                    var builderType = System.Type.GetType("IntelliVerseX.MoreOfUs.Editor.IVXMoreOfUsPrefabBuilder, IntelliVerseX.MoreOfUs.Editor");
                    if (builderType != null)
                    {
                        var method = builderType.GetMethod("AddToCurrentScene", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
                        method?.Invoke(null, null);
                    }
                    else
                    {
                        EditorUtility.DisplayDialog("Info", "Please ensure the MoreOfUs assembly is compiled first.", "OK");
                    }
                }
                catch (System.Exception ex)
                {
                    Debug.LogError($"[IVXSDKSetupWizard] Failed to add More Of Us canvas: {ex.Message}");
                    EditorUtility.DisplayDialog("Error", $"Failed to add to scene: {ex.Message}", "OK");
                }
            }

            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();

            EditorGUILayout.Space(10);

            // Features Overview
            EditorGUILayout.BeginVertical(moduleBoxStyle);
            EditorGUILayout.LabelField("Features", EditorStyles.boldLabel);

            EditorGUILayout.LabelField("The More Of Us feature includes:", EditorStyles.wordWrappedLabel);
            EditorGUILayout.Space(5);

            DrawFeatureItem("Netflix-style horizontal carousel with smooth scrolling");
            DrawFeatureItem("Hover animations with scale and detail reveal effects");
            DrawFeatureItem("Automatic icon loading with caching");
            DrawFeatureItem("Platform-aware display (shows Android apps on Android, iOS on iOS)");
            DrawFeatureItem("Auto-scroll with configurable interval");
            DrawFeatureItem("Navigation arrows for keyboard/controller support");
            DrawFeatureItem("Offline cache for previously fetched data");
            DrawFeatureItem("Loading and empty states with retry functionality");

            EditorGUILayout.EndVertical();

            EditorGUILayout.Space(10);

            // Usage Instructions
            EditorGUILayout.BeginVertical(moduleBoxStyle);
            EditorGUILayout.LabelField("Usage", EditorStyles.boldLabel);

            EditorGUILayout.LabelField(
                "1. Click 'Build All Prefabs' to create the UI components\n" +
                "2. Click 'Add To Scene' to add the canvas to your current scene\n" +
                "3. Call IVXMoreOfUsCanvas.Show() from your game to display the panel\n\n" +
                "Example code:",
                EditorStyles.wordWrappedLabel);

            EditorGUILayout.Space(5);
            EditorGUILayout.TextArea(
                "// Show the More Of Us panel\n" +
                "var canvas = FindObjectOfType<IVXMoreOfUsCanvas>();\n" +
                "if (canvas != null)\n" +
                "    canvas.Show();\n\n" +
                "// Or create it programmatically\n" +
                "var canvas = IVXMoreOfUsCanvas.Create();\n" +
                "canvas.Show();",
                EditorStyles.helpBox, GUILayout.Height(100));

            EditorGUILayout.EndVertical();
        }

        private void DrawFeatureItem(string text)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("  *", GUILayout.Width(20));
            EditorGUILayout.LabelField(text, EditorStyles.wordWrappedLabel);
            EditorGUILayout.EndHorizontal();
        }

        #endregion

        #region Test Scenes Tab

        private void DrawTestScenesTab()
        {
            EditorGUILayout.LabelField("Test Scene Creator", subHeaderStyle ?? EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(
                "Create test scenes to manually verify each SDK feature. " +
                "Each scene includes the necessary prefabs and a simple UI to test functionality.",
                MessageType.Info);

            EditorGUILayout.Space(10);

            // Test Scene Buttons
            DrawTestSceneButton("🔐 Auth Test Scene",
                "Test login, register, OTP, guest, and social auth flows",
                "IVX_AuthTest",
                CreateAuthTestScene);

            DrawTestSceneButton("👥 Friends Test Scene",
                "Test friend list, requests, search, and social features",
                "IVX_FriendsTest",
                CreateFriendsTestScene);

            DrawTestSceneButton("💰 Wallet Test Scene",
                "Test wallet display, balance updates, and transactions",
                "IVX_WalletTest",
                CreateWalletTestScene);

            DrawTestSceneButton("🏆 Leaderboard Test Scene",
                "Test leaderboard display, score submission, and rankings",
                "IVX_LeaderboardTest",
                CreateLeaderboardTestScene);

            DrawTestSceneButton("📺 Ads Test Scene",
                "Test ad loading, display, and reward callbacks",
                "IVX_AdsTest",
                CreateAdsTestScene);

            DrawTestSceneButton("🎮 Full Integration Test",
                "Complete test scene with all SDK features",
                "IVX_FullTest",
                CreateFullTestScene);

            EditorGUILayout.Space(10);

            // Scene Management
            EditorGUILayout.BeginVertical(moduleBoxStyle);
            EditorGUILayout.LabelField("Scene Management", EditorStyles.boldLabel);

            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button("Open Test Scenes Folder", GUILayout.Height(25)))
            {
                OpenTestScenesFolder();
            }

            if (GUILayout.Button("Add All Test Scenes to Build", GUILayout.Height(25)))
            {
                AddTestScenesToBuild();
            }

            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();
        }

        private void DrawTestSceneButton(string title, string description, string sceneName, Action createAction)
        {
            string scenePath = $"Assets/Scenes/Tests/{sceneName}.unity";
            bool sceneExists = File.Exists(scenePath);

            EditorGUILayout.BeginVertical(moduleBoxStyle);
            EditorGUILayout.BeginHorizontal();

            EditorGUILayout.BeginVertical();
            EditorGUILayout.LabelField(title + (sceneExists ? " ✅" : ""), EditorStyles.boldLabel);
            EditorGUILayout.LabelField(description, EditorStyles.wordWrappedMiniLabel);
            EditorGUILayout.EndVertical();

            EditorGUILayout.BeginVertical(GUILayout.Width(85));

            if (sceneExists)
            {
                if (GUILayout.Button("Open", GUILayout.Height(25)))
                {
                    if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                    {
                        EditorSceneManager.OpenScene(scenePath);
                    }
                }
                if (GUILayout.Button("Play", GUILayout.Height(25)))
                {
                    if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                    {
                        EditorSceneManager.OpenScene(scenePath);
                        EditorApplication.isPlaying = true;
                    }
                }
            }
            else
            {
                if (GUILayout.Button("Create", GUILayout.Height(52)))
                {
                    createAction?.Invoke();
                }
            }

            EditorGUILayout.EndVertical();
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();
        }

        #endregion

        #region Module Section Drawing

        private void DrawModuleSection(string title, ModuleSetupState state, Action checkAction, string description)
        {
            EditorGUILayout.BeginVertical(moduleBoxStyle);

            EditorGUILayout.BeginHorizontal();

            state.isExpanded = EditorGUILayout.Foldout(state.isExpanded, title, true, EditorStyles.foldoutHeader);

            // Status indicator
            DrawStatusIndicator(state);

            EditorGUILayout.EndHorizontal();

            if (state.isExpanded)
            {
                EditorGUILayout.Space(5);
                EditorGUILayout.LabelField(description, EditorStyles.wordWrappedMiniLabel);

                EditorGUILayout.Space(5);

                // Setup steps checklist
                for (int i = 0; i < state.setupSteps.Count; i++)
                {
                    EditorGUILayout.BeginHorizontal();
                    GUILayout.Label(state.stepCompleted[i] ? "  ✅" : "  ⬜", GUILayout.Width(30));
                    EditorGUILayout.LabelField(state.setupSteps[i]);
                    EditorGUILayout.EndHorizontal();
                }

                if (!string.IsNullOrEmpty(state.statusMessage))
                {
                    EditorGUILayout.Space(5);
                    EditorGUILayout.HelpBox(state.statusMessage,
                        state.isSetupComplete ? MessageType.Info : MessageType.Warning);
                }

                EditorGUILayout.Space(5);

                if (GUILayout.Button("🔄 Refresh Status", GUILayout.Height(22)))
                {
                    checkAction?.Invoke();
                    Repaint();
                }
            }

            EditorGUILayout.EndVertical();
        }

        private void DrawExpandedModuleSection(string title, ModuleSetupState state, Action checkAction,
            string description, Action drawActions)
        {
            // Safely handle null state
            if (state == null)
            {
                EditorGUILayout.HelpBox($"Module state for '{title}' is not initialized.", MessageType.Warning);
                return;
            }

            EditorGUILayout.BeginVertical(moduleBoxStyle ?? EditorStyles.helpBox);

            try
            {
                EditorGUILayout.BeginHorizontal();
                state.isExpanded = EditorGUILayout.Foldout(state.isExpanded, title, true, EditorStyles.foldoutHeader);
                DrawStatusIndicator(state);
                EditorGUILayout.EndHorizontal();

                if (state.isExpanded)
                {
                    EditorGUILayout.Space(5);
                    EditorGUILayout.LabelField(description ?? "", EditorStyles.wordWrappedMiniLabel);

                    EditorGUILayout.Space(5);

                    // Setup steps checklist with null safety
                    if (state.setupSteps != null && state.stepCompleted != null)
                    {
                        int stepCount = Mathf.Min(state.setupSteps.Count, state.stepCompleted.Count);
                        for (int i = 0; i < stepCount; i++)
                        {
                            EditorGUILayout.BeginHorizontal();
                            GUILayout.Label(state.stepCompleted[i] ? "  ✅" : "  ⬜", GUILayout.Width(30));
                            EditorGUILayout.LabelField(state.setupSteps[i] ?? "");
                            EditorGUILayout.EndHorizontal();
                        }
                    }

                    // Module-specific actions - wrap in try-catch to prevent layout issues
                    try
                    {
                        drawActions?.Invoke();
                    }
                    catch (System.Exception ex)
                    {
                        Debug.LogError($"[IVXSDKSetupWizard] Error in module actions for '{title}': {ex.Message}");
                    }

                    EditorGUILayout.Space(5);

                    if (GUILayout.Button("🔄 Refresh Status", GUILayout.Height(22)))
                    {
                        try
                        {
                            checkAction?.Invoke();
                        }
                        catch (System.Exception ex)
                        {
                            Debug.LogError($"[IVXSDKSetupWizard] Error checking module '{title}': {ex.Message}");
                        }
                        Repaint();
                    }
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[IVXSDKSetupWizard] Layout error in module '{title}': {ex.Message}");
            }
            
            EditorGUILayout.EndVertical();
        }

        private void DrawStatusIndicator(ModuleSetupState state)
        {
            if (state.isSetupComplete)
            {
                GUILayout.Label("✅", GUILayout.Width(25));
            }
            else if (state.stepCompleted.Any(x => x))
            {
                GUILayout.Label("🔶", GUILayout.Width(25));
            }
            else
            {
                GUILayout.Label("⬜", GUILayout.Width(25));
            }
        }

        #endregion

        #region Module Check Methods

        private void RefreshAllModuleStatus()
        {
            // Refresh SDK path detection in case installation changed
            RefreshSDKPath();
            
            // Ensure module states are initialized
            if (coreModule.stepCompleted == null || coreModule.stepCompleted.Count == 0)
            {
                InitializeModuleStates();
            }
            
            CheckCoreModule();
            CheckIdentityModule();
            CheckBackendModule();
            CheckAuthModule();
            CheckFriendsModule();
            CheckWalletModule();
            CheckLeaderboardModule();
            CheckSocialModule();
            CheckQuizModule();
            CheckLocalizationModule();
            CheckAdsModule();
            CheckIAPModule();
            CheckRetentionModule();
        }

        /// <summary>
        /// Checks Core module status using type-based detection (works for both Assets and UPM).
        /// </summary>
        private void CheckCoreModule()
        {
            if (coreModule.stepCompleted == null || coreModule.stepCompleted.Count < 3) return;
            
            // Use type-based checking - most reliable for UPM packages
            coreModule.stepCompleted[0] = TypeExists("IntelliVerseX.Core.IntelliVerseXManager");
            coreModule.stepCompleted[1] = TypeExists("IntelliVerseX.Core.IntelliVerseXConfig") || 
                                          TypeExists("IntelliVerseX.Core.IVXLogger");
            coreModule.stepCompleted[2] = Resources.Load("IntelliVerseX/GameConfig") != null ||
                                          AssetDatabase.LoadAssetAtPath<ScriptableObject>(RESOURCES_PATH + "/GameConfig.asset") != null ||
                                          TypeExists("IntelliVerseX.Core.IntelliVerseXConfig"); // Config type exists = SDK installed

            coreModule.isSetupComplete = coreModule.stepCompleted.All(x => x);
            coreModule.statusMessage = coreModule.isSetupComplete 
                ? "Core module is configured" + (_isUPMInstall ? " (UPM Package)" : " (Development)")
                : "Some core components missing";
        }

        /// <summary>
        /// Checks Identity module status using type-based detection.
        /// </summary>
        private void CheckIdentityModule()
        {
            if (identityModule.stepCompleted == null || identityModule.stepCompleted.Count < 3) return;
            
            // Use type-based checking
            identityModule.stepCompleted[0] = TypeExists("IntelliVerseX.Identity.UserSessionManager");
            identityModule.stepCompleted[1] = TypeExists("IntelliVerseX.Identity.APIManager");
            identityModule.stepCompleted[2] = TypeExists("IntelliVerseX.Identity.IntelliVerseXUserIdentity");

            identityModule.isSetupComplete = identityModule.stepCompleted.All(x => x);
            identityModule.statusMessage = identityModule.isSetupComplete 
                ? "Identity module is configured" 
                : "Some identity components missing";
        }

        /// <summary>
        /// Checks Backend module status using type-based detection.
        /// </summary>
        private void CheckBackendModule()
        {
            if (backendModule.stepCompleted == null || backendModule.stepCompleted.Count < 3) return;
            
            // Use type-based checking
            backendModule.stepCompleted[0] = TypeExists("IntelliVerseX.Backend.IVXNakamaManager");
            backendModule.stepCompleted[1] = TypeExists("IntelliVerseX.Backend.IVXBackendService");
            // Prefab check - try both Assets and Package paths
            backendModule.stepCompleted[2] = AssetDatabase.LoadAssetAtPath<GameObject>(MANAGERS_PREFAB_PATH + "/NakamaManager.prefab") != null ||
                                             TypeExists("IntelliVerseX.Backend.IVXNakamaRPC"); // Alternative: check if RPC type exists

            backendModule.isSetupComplete = backendModule.stepCompleted.All(x => x);
            backendModule.statusMessage = backendModule.isSetupComplete 
                ? "Backend module is configured" 
                : "Some backend components missing";
        }

        /// <summary>
        /// Checks Auth module status using type-based detection.
        /// </summary>
        private void CheckAuthModule()
        {
            if (authModule.stepCompleted == null || authModule.stepCompleted.Count < 4) return;
            
            // Config check - either asset exists OR auth types exist (SDK installed)
            authModule.stepCompleted[0] = Resources.Load("IntelliVerseX/AuthConfig") != null ||
                                          AssetDatabase.LoadAssetAtPath<ScriptableObject>(RESOURCES_PATH + "/AuthConfig.asset") != null ||
                                          TypeExists("IntelliVerseX.Auth.UI.IVXCanvasAuth"); // Auth canvas type = SDK has auth
            
            // Prefab check - try package path
            authModule.stepCompleted[1] = AssetDatabase.LoadAssetAtPath<GameObject>(AUTH_PREFABS_PATH + "/IVX_AuthCanvas.prefab") != null ||
                                          TypeExists("IntelliVerseX.Auth.UI.IVXCanvasAuth");
            
            // UI scripts check using types
            authModule.stepCompleted[2] = TypeExists("IntelliVerseX.Auth.UI.IVXPanelLogin") ||
                                          TypeExists("IntelliVerseX.Auth.UI.IVXPanelRegister");
            authModule.stepCompleted[3] = TypeExists("IntelliVerseX.Auth.UI.IVXPanelOTP");

            authModule.isSetupComplete = authModule.stepCompleted.All(x => x);
            authModule.statusMessage = authModule.isSetupComplete 
                ? "Auth module is fully configured" 
                : "Auth setup incomplete";
        }

        /// <summary>
        /// Checks Friends module status using type-based detection.
        /// All Friends types are in IntelliVerseX.Social namespace.
        /// </summary>
        private void CheckFriendsModule()
        {
            if (friendsModule.stepCompleted == null || friendsModule.stepCompleted.Count < 5) return;
            
            // Config check (IVXFriendsConfig is in IntelliVerseX.Social)
            friendsModule.stepCompleted[0] = Resources.Load("IntelliVerseX/FriendsConfig") != null ||
                                             AssetDatabase.LoadAssetAtPath<ScriptableObject>(RESOURCES_PATH + "/FriendsConfig.asset") != null ||
                                             TypeExists("IntelliVerseX.Social.IVXFriendsConfig");
            
            // Prefab/Type checks - UI types are in IntelliVerseX.Social.UI
            friendsModule.stepCompleted[1] = AssetDatabase.LoadAssetAtPath<GameObject>(SOCIAL_PREFABS_PATH + "/IVXFriendSlot.prefab") != null ||
                                             TypeExists("IntelliVerseX.Social.UI.IVXFriendSlot");
            friendsModule.stepCompleted[2] = AssetDatabase.LoadAssetAtPath<GameObject>(SOCIAL_PREFABS_PATH + "/IVXFriendRequestSlot.prefab") != null ||
                                             TypeExists("IntelliVerseX.Social.UI.IVXFriendRequestSlot");
            friendsModule.stepCompleted[3] = AssetDatabase.LoadAssetAtPath<GameObject>(SOCIAL_PREFABS_PATH + "/IVXFriendSearchSlot.prefab") != null ||
                                             TypeExists("IntelliVerseX.Social.UI.IVXFriendSearchSlot");
            friendsModule.stepCompleted[4] = TypeExists("IntelliVerseX.Social.UI.IVXFriendsPanel") ||
                                             TypeExists("IntelliVerseX.Social.IVXFriendsService");

            friendsModule.isSetupComplete = friendsModule.stepCompleted.All(x => x);
            friendsModule.statusMessage = friendsModule.isSetupComplete 
                ? "Friends module is fully configured" 
                : "Friends setup incomplete";
        }

        /// <summary>
        /// Checks Wallet module status using type-based detection.
        /// </summary>
        private void CheckWalletModule()
        {
            if (walletModule.stepCompleted == null || walletModule.stepCompleted.Count < 2) return;
            
            // Check for wallet manager types (IVXWalletManager is in Backend namespace)
            walletModule.stepCompleted[0] = TypeExists("IntelliVerseX.Backend.IVXWalletManager") ||
                                            TypeExists("IntelliVerseX.Games.Wallet.IVXGWalletManager") ||
                                            TypeExists("IntelliVerseX.V2.Manager.IVXNWalletManager");
            
            // Check for wallet UI types (IVXWalletDisplay is in UI namespace)
            walletModule.stepCompleted[1] = TypeExists("IntelliVerseX.UI.IVXWalletDisplay") ||
                                            TypeExists("IntelliVerseX.Games.Wallet.IVXGWalletDisplay") ||
                                            AssetDatabase.LoadAssetAtPath<GameObject>(SDK_ROOT + "/Wallet/Prefabs/IVXGWalletDisplay.prefab") != null;

            walletModule.isSetupComplete = walletModule.stepCompleted.All(x => x);
            walletModule.statusMessage = walletModule.isSetupComplete 
                ? "Wallet module is configured" 
                : "Wallet components missing";
        }

        /// <summary>
        /// Checks Social (Share & Rate) module status using type-based detection.
        /// </summary>
        private void CheckSocialModule()
        {
            if (socialModule.stepCompleted == null || socialModule.stepCompleted.Count < 2) return;
            
            // Check for share manager (IVXGShareManager is in IntelliVerseX.Games.Social)
            socialModule.stepCompleted[0] = TypeExists("IntelliVerseX.Games.Social.IVXGShareManager") ||
                                            TypeExists("IntelliVerseX.Social.IVXNativeShareHelper") ||
                                            AssetDatabase.LoadAssetAtPath<GameObject>(SDK_ROOT + "/Social/Prefabs/IVXGShareManager.prefab") != null;
            
            // Check for rate app manager (IVXGRateAppManager is in IntelliVerseX.Games.Social)
            socialModule.stepCompleted[1] = TypeExists("IntelliVerseX.Games.Social.IVXGRateAppManager") ||
                                            AssetDatabase.LoadAssetAtPath<GameObject>(SDK_ROOT + "/Social/Prefabs/IVXGRateAppManager.prefab") != null;

            socialModule.isSetupComplete = socialModule.stepCompleted.All(x => x);
            socialModule.statusMessage = socialModule.isSetupComplete 
                ? "Social module is configured" 
                : "Social components missing";
        }

        /// <summary>
        /// Checks Leaderboard module status using type-based detection.
        /// </summary>
        private void CheckLeaderboardModule()
        {
            if (leaderboardModule.stepCompleted == null || leaderboardModule.stepCompleted.Count < 2) return;
            
            // Check for leaderboard manager types (IVXGLeaderboardManager is in IntelliVerseX.Games.Leaderboard)
            leaderboardModule.stepCompleted[0] = TypeExists("IntelliVerseX.Games.Leaderboard.IVXGLeaderboardManager") ||
                                                 TypeExists("IntelliVerseX.Games.Leaderboard.IVXGLeaderboard") ||
                                                 TypeExists("IntelliVerseX.V2.Manager.IVXNLeaderbordManager");
            
            // Check for leaderboard UI types (IVXGLeaderboardUI is in IntelliVerseX.Games.Leaderboard.UI)
            leaderboardModule.stepCompleted[1] = TypeExists("IntelliVerseX.Games.Leaderboard.UI.IVXGLeaderboardUI") ||
                                                 TypeExists("IntelliVerseX.Games.Leaderboard.UI.IVXGLeaderboardEntryView") ||
                                                 AssetDatabase.LoadAssetAtPath<GameObject>(SDK_ROOT + "/Leaderboard/Prefabs/IVXGLeaderboardCanvas.prefab") != null;

            leaderboardModule.isSetupComplete = leaderboardModule.stepCompleted.All(x => x);
            leaderboardModule.statusMessage = leaderboardModule.isSetupComplete 
                ? "Leaderboard module is configured" 
                : "Leaderboard components missing";
        }

        /// <summary>
        /// Checks Quiz module status using type-based detection.
        /// </summary>
        private void CheckQuizModule()
        {
            if (quizModule.stepCompleted == null || quizModule.stepCompleted.Count < 2) return;
            
            // Check for quiz manager types
            quizModule.stepCompleted[0] = TypeExists("IntelliVerseX.Quiz.IVXQuizSessionManager") ||
                                          TypeExists("IntelliVerseX.Quiz.IVXQuizManager");
            
            // Check for quiz UI types
            quizModule.stepCompleted[1] = TypeExists("IntelliVerseX.QuizUI.IVXQuizQuestionPanel") ||
                                          TypeExists("IntelliVerseX.QuizUI.IVXQuizUI");

            quizModule.isSetupComplete = quizModule.stepCompleted.All(x => x);
            quizModule.statusMessage = quizModule.isSetupComplete 
                ? "Quiz module is configured" 
                : "Quiz components missing";
        }

        /// <summary>
        /// Checks Localization module status using type-based detection.
        /// </summary>
        private void CheckLocalizationModule()
        {
            if (localizationModule.stepCompleted == null || localizationModule.stepCompleted.Count < 2) return;
            
            // Check for localization service types
            localizationModule.stepCompleted[0] = TypeExists("IntelliVerseX.Localization.IVXLocalizationService") ||
                                                  TypeExists("IntelliVerseX.Localization.IVXLocalizationManager");
            
            // Check for language manager types
            localizationModule.stepCompleted[1] = TypeExists("IntelliVerseX.Localization.IVXLanguageManager") ||
                                                  TypeExists("IntelliVerseX.Localization.IVXLocalizedText");

            localizationModule.isSetupComplete = localizationModule.stepCompleted.All(x => x);
            localizationModule.statusMessage = localizationModule.isSetupComplete 
                ? "Localization module is configured" 
                : "Localization components missing";
        }

        /// <summary>
        /// Checks Ads module status using type-based detection.
        /// </summary>
        private void CheckAdsModule()
        {
            if (adsModule.stepCompleted == null || adsModule.stepCompleted.Count < 2) return;
            
            // Check for ads manager types
            adsModule.stepCompleted[0] = TypeExists("IntelliVerseX.Monetization.IVXAdsManager") ||
                                         TypeExists("IntelliVerseX.Monetization.IVXAdsWaterfallManager");
            
            // Check for platform-specific ads types
            adsModule.stepCompleted[1] = TypeExists("IntelliVerseX.Monetization.IVXWebGLAdsManager") ||
                                         TypeExists("IntelliVerseX.Monetization.IVXAdsWaterfallManager") ||
                                         TypeExists("IntelliVerseX.Monetization.IVXLevelPlayAdsManager");

            adsModule.isSetupComplete = adsModule.stepCompleted.All(x => x);
            adsModule.statusMessage = adsModule.isSetupComplete 
                ? "Ads module is configured" 
                : "Ads components missing";
        }

        /// <summary>
        /// Checks IAP module status using type-based detection.
        /// </summary>
        private void CheckIAPModule()
        {
            if (iapModule.stepCompleted == null || iapModule.stepCompleted.Count < 2) return;
            
            // Check for IAP manager types
            iapModule.stepCompleted[0] = TypeExists("IntelliVerseX.Monetization.IVXIAPManager") ||
                                         TypeExists("IntelliVerseX.IAP.IVXIAPManager");
            
            // Check for IAP config types
            iapModule.stepCompleted[1] = TypeExists("IntelliVerseX.Monetization.IVXIAPConfig") ||
                                         TypeExists("IntelliVerseX.IAP.IVXIAPConfig") ||
                                         TypeExists("IntelliVerseX.Monetization.IVXIAPManager"); // If manager exists, config likely does too

            iapModule.isSetupComplete = iapModule.stepCompleted.All(x => x);
            iapModule.statusMessage = iapModule.isSetupComplete 
                ? "IAP module is configured" 
                : "IAP components missing";
        }

        /// <summary>
        /// Checks Retention module status (QuizVerse-specific).
        /// </summary>
        private void CheckRetentionModule()
        {
            if (retentionModule.stepCompleted == null || retentionModule.stepCompleted.Count < 2) return;
            
            // Retention module is QuizVerse-specific, check both file and type
            retentionModule.stepCompleted[0] = File.Exists(QUIZVERSE_ROOT + "/Scripts/Rewards/DailyRewardManager.cs") ||
                                               TypeExists("QuizVerse.Rewards.DailyRewardManager");
            retentionModule.stepCompleted[1] = File.Exists(QUIZVERSE_ROOT + "/Scripts/Retention/StreakShieldManager.cs") ||
                                               TypeExists("QuizVerse.Retention.StreakShieldManager");

            retentionModule.isSetupComplete = retentionModule.stepCompleted.All(x => x);
            retentionModule.statusMessage = retentionModule.isSetupComplete 
                ? "Retention module is configured" 
                : "Retention components missing (QuizVerse-specific)";
        }

        /// <summary>
        /// Checks if DOTween is installed in the project.
        /// </summary>
        private bool CheckDOTweenInstalled()
        {
            // Check via type (most reliable)
            if (TypeExists("DG.Tweening.DOTween")) return true;

            // Check assemblies
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                if (assembly.GetName().Name == "DOTween")
                    return true;
            }

            // Fallback to directory check
            return Directory.Exists("Assets/Plugins/Demigiant/DOTween") ||
                   Directory.Exists("Assets/DOTween");
        }

        #endregion

        #region Setup Methods

        private void SetupAllModules()
        {
            try
            {
                EditorUtility.DisplayProgressBar("SDK Setup", "Setting up all modules...", 0f);

                EditorUtility.DisplayProgressBar("SDK Setup", "Creating SDK Config...", 0.1f);
                CreateSDKConfigAsset();

                EditorUtility.DisplayProgressBar("SDK Setup", "Setting up Auth...", 0.3f);
                SetupAuthModule();

                EditorUtility.DisplayProgressBar("SDK Setup", "Setting up Friends...", 0.5f);
                SetupFriendsModule();

                EditorUtility.DisplayProgressBar("SDK Setup", "Setting up Features...", 0.7f);
                SetupWalletAndLeaderboard();

                EditorUtility.DisplayProgressBar("SDK Setup", "Finalizing...", 0.9f);
                AssetDatabase.Refresh();

                RefreshAllModuleStatus();

                EditorUtility.ClearProgressBar();
                EditorUtility.DisplayDialog("Setup Complete",
                    "All SDK modules have been set up!\n\n" +
                    "Use the 'Test Scenes' tab to create test scenes and verify functionality.",
                    "OK");
            }
            catch (Exception ex)
            {
                EditorUtility.ClearProgressBar();
                EditorUtility.DisplayDialog("Setup Error", $"Error: {ex.Message}", "OK");
                Debug.LogError($"[IVXSDKSetupWizard] Setup failed: {ex}");
            }
        }

        private void SetupAuthModule()
        {
            CreateAuthConfig();
            CreateAuthPrefabs();
            CheckAuthModule();
            Debug.Log("[IVXSDKSetupWizard] Auth module setup complete");
        }

        private void SetupFriendsModule()
        {
            CreateFriendsConfig();
            CreateFriendsPrefabs();
            CheckFriendsModule();
            Debug.Log("[IVXSDKSetupWizard] Friends module setup complete");
        }

        private void SetupWalletAndLeaderboard()
        {
            CheckWalletModule();
            CheckLeaderboardModule();
            Debug.Log("[IVXSDKSetupWizard] Wallet & Leaderboard setup complete");
        }

        private void SetupMonetization()
        {
            CheckAdsModule();
            CheckIAPModule();
            Debug.Log("[IVXSDKSetupWizard] Monetization setup complete");
        }

        private void SetupLocalization()
        {
            CheckLocalizationModule();
            Debug.Log("[IVXSDKSetupWizard] Localization setup complete");
        }

        #endregion

        #region Config Creation

        private void CreateSDKConfigAsset()
        {
            EnsureDirectoryExists(RESOURCES_PATH);

            var configPath = RESOURCES_PATH + "/GameConfig.asset";
            if (AssetDatabase.LoadAssetAtPath<ScriptableObject>(configPath) != null)
            {
                Debug.Log("[IVXSDKSetupWizard] SDK Config already exists");
                return;
            }

            var configType = GetTypeByName("IntelliVerseX.Core.IntelliVerseXConfig");
            if (configType != null)
            {
                var config = ScriptableObject.CreateInstance(configType);
                AssetDatabase.CreateAsset(config, configPath);
                AssetDatabase.SaveAssets();
                Debug.Log($"[IVXSDKSetupWizard] Created SDK Config at: {configPath}");
            }
            else
            {
                Debug.LogWarning("[IVXSDKSetupWizard] IntelliVerseXConfig type not found");
            }
        }

        private void CreateAuthConfig()
        {
            EnsureDirectoryExists(RESOURCES_PATH);

            var configPath = RESOURCES_PATH + "/AuthConfig.asset";
            if (AssetDatabase.LoadAssetAtPath<ScriptableObject>(configPath) != null)
            {
                Debug.Log("[IVXSDKSetupWizard] AuthConfig already exists");
                return;
            }

            // Auth configuration is now done via IVXCanvasAuth Inspector
            // No ScriptableObject config needed
            Debug.Log("[IVXSDKSetupWizard] Auth configuration is now done via IVXCanvasAuth Inspector - no separate config asset needed.");
        }

        private void CreateFriendsConfig()
        {
            EnsureDirectoryExists(RESOURCES_PATH);

            var configPath = RESOURCES_PATH + "/FriendsConfig.asset";
            if (AssetDatabase.LoadAssetAtPath<ScriptableObject>(configPath) != null)
            {
                Debug.Log("[IVXSDKSetupWizard] FriendsConfig already exists");
                return;
            }

            var configType = GetTypeByName("IntelliVerseX.Social.IVXFriendsConfig");
            if (configType != null)
            {
                var config = ScriptableObject.CreateInstance(configType);
                AssetDatabase.CreateAsset(config, configPath);
                AssetDatabase.SaveAssets();
                Debug.Log($"[IVXSDKSetupWizard] Created FriendsConfig at: {configPath}");
            }
            else
            {
                Debug.LogWarning("[IVXSDKSetupWizard] IVXFriendsConfig type not found");
            }
        }

        #endregion

        #region Prefab Creation

        private void CreateAuthPrefabs()
        {
            GameObject canvas = null;
            try
            {
                // Use writable path (handles both UPM and dev installs)
                string prefabPath = WRITABLE_AUTH_PREFABS_PATH;
                EnsureDirectoryExists(prefabPath);
                
                Debug.Log($"[IVXSDKSetupWizard] Creating Auth prefabs at writable path: {prefabPath}");

                var builderType = GetTypeByName("IntelliVerseX.Auth.Editor.IVXAuthPrefabBuilder");
                if (builderType != null)
                {
                    // Try the new method with path parameter first
                    var createMethodWithPath = builderType.GetMethod("CreateAuthCanvasPrefabAtPath",
                        System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
                    
                    if (createMethodWithPath != null)
                    {
                        createMethodWithPath.Invoke(null, new object[] { prefabPath });
                        AssetDatabase.Refresh();
                        Debug.Log($"[IVXSDKSetupWizard] Created Auth prefab at: {prefabPath}/IVX_AuthCanvas.prefab");
                        return;
                    }
                    
                    // Fallback to old method
                    var createMethod = builderType.GetMethod("CreateAuthCanvasPrefab",
                        System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);

                    if (createMethod != null)
                    {
                        canvas = createMethod.Invoke(null, null) as GameObject;
                        if (canvas != null)
                        {
                            var fullPrefabPath = prefabPath + "/IVX_AuthCanvas.prefab";
                            PrefabUtility.SaveAsPrefabAsset(canvas, fullPrefabPath);
                            DestroyImmediate(canvas);
                            canvas = null;
                            AssetDatabase.Refresh();
                            Debug.Log($"[IVXSDKSetupWizard] Created Auth prefab at: {fullPrefabPath}");
                        }
                    }
                }
                else
                {
                    Debug.LogWarning("[IVXSDKSetupWizard] IVXAuthPrefabBuilder not found");
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"[IVXSDKSetupWizard] Auth prefab creation failed: {ex.Message}\n{ex.StackTrace}");
                
                // Clean up any partially created objects
                if (canvas != null)
                {
                    try { DestroyImmediate(canvas); } catch { }
                }
            }
        }

        private void CreateFriendsPrefabs()
        {
            try
            {
                // Use writable path (handles both UPM and dev installs)
                string prefabPath = WRITABLE_SOCIAL_PREFABS_PATH;
                EnsureDirectoryExists(prefabPath);
                
                Debug.Log($"[IVXSDKSetupWizard] Creating Friends prefabs at writable path: {prefabPath}");

                var builderType = GetTypeByName("IntelliVerseX.Social.Editor.IVXFriendsPrefabBuilder");
                if (builderType != null)
                {
                    var saveMethod = builderType.GetMethod("SavePrefabs",
                        System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);

                    if (saveMethod != null)
                    {
                        saveMethod.Invoke(null, new object[] { prefabPath });
                        AssetDatabase.Refresh();
                        Debug.Log($"[IVXSDKSetupWizard] Created Friends prefabs at: {prefabPath}");
                    }
                }
                else
                {
                    Debug.LogWarning("[IVXSDKSetupWizard] IVXFriendsPrefabBuilder not found");
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"[IVXSDKSetupWizard] Friends prefab creation failed: {ex.Message}\n{ex.StackTrace}");
            }
        }

        #endregion

        #region Scene Actions

        private void AddManagersToCurrentScene()
        {
            var scene = EditorSceneManager.GetActiveScene();

            // Create managers root
            var managersRoot = GameObject.Find("--- SDK Managers ---");
            if (managersRoot == null)
            {
                managersRoot = new GameObject("--- SDK Managers ---");
            }

            // Add NakamaManager
            var nakamaManagerPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(MANAGERS_PREFAB_PATH + "/NakamaManager.prefab");
            if (nakamaManagerPrefab != null && GameObject.Find("NakamaManager") == null)
            {
                var instance = PrefabUtility.InstantiatePrefab(nakamaManagerPrefab) as GameObject;
                instance.transform.SetParent(managersRoot.transform);
            }

            // Add UserData
            var userDataPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(MANAGERS_PREFAB_PATH + "/UserData.prefab");
            if (userDataPrefab != null && GameObject.Find("UserData") == null)
            {
                var instance = PrefabUtility.InstantiatePrefab(userDataPrefab) as GameObject;
                instance.transform.SetParent(managersRoot.transform);
            }

            EditorSceneManager.MarkSceneDirty(scene);
            Debug.Log("[IVXSDKSetupWizard] Added managers to scene");
        }

        private void AddAuthCanvasToScene()
        {
            // Check if Auth Canvas already exists in scene
            var canvasAuthType = GetTypeByName("IntelliVerseX.Auth.UI.IVXCanvasAuth");
            if (canvasAuthType != null)
            {
                var existing = GameObject.FindAnyObjectByType(canvasAuthType);
                if (existing != null)
                {
                    Selection.activeGameObject = (existing as Component)?.gameObject;
                    Debug.Log("[IVXSDKSetupWizard] Auth Canvas already exists in scene");
                    return;
                }
            }

            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(AUTH_PREFABS_PATH + "/IVX_AuthCanvas.prefab");
            if (prefab != null)
            {
                var instance = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
                if (instance != null)
                {
                    Undo.RegisterCreatedObjectUndo(instance, "Add Auth Canvas");
                    Selection.activeGameObject = instance;
                    EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
                    Debug.Log("[IVXSDKSetupWizard] Added Auth Canvas to scene");
                }
            }
            else
            {
                // Auth Canvas prefab doesn't exist - show guidance
                Debug.LogWarning("[IVXSDKSetupWizard] Auth Canvas prefab not found. Auth module may not be fully set up.");
                EditorUtility.DisplayDialog("Auth Setup Required",
                    "The Auth Canvas prefab doesn't exist yet.\n\n" +
                    "To set up authentication:\n" +
                    "1. Ensure the Auth module files exist in Assets/_IntelliVerseXSDK/Auth/\n" +
                    "2. Use 'Create Auth Prefabs' button to generate prefabs\n" +
                    "3. Or manually create an Auth Canvas with IVXCanvasAuth component",
                    "OK");
            }
        }

        private void AddFriendsUIToScene()
        {
            var existingType = GetTypeByName("IntelliVerseX.Social.UI.IVXFriendsPanel");
            if (existingType != null)
            {
                var existing = GameObject.FindAnyObjectByType(existingType);
                if (existing != null)
                {
                    Selection.activeGameObject = (existing as Component)?.gameObject;
                    Debug.Log("[IVXSDKSetupWizard] Friends Panel already exists in scene");
                    return;
                }
            }

            var builderType = GetTypeByName("IntelliVerseX.Social.Editor.IVXFriendsPrefabBuilder");
            if (builderType != null)
            {
                var createMethod = builderType.GetMethod("CreateFriendsCanvas",
                    System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);

                if (createMethod != null)
                {
                    var canvas = createMethod.Invoke(null, new object[] { null }) as GameObject;
                    if (canvas != null)
                    {
                        Undo.RegisterCreatedObjectUndo(canvas, "Add Friends Canvas");
                        Selection.activeGameObject = canvas;
                        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
                        Debug.Log("[IVXSDKSetupWizard] Added Friends Canvas to scene");
                    }
                }
            }
        }

        private void CreateAuthDemoScene()
        {
            // Use delayCall to avoid GUI layout issues when changing scenes
            EditorApplication.delayCall += () =>
            {
                if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;

                EnsureDirectoryExists(AUTH_ROOT + "/Scenes");

                var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);

                // Add EventSystem
                EnsureEventSystem();

                // Add Auth Canvas
                var builderType = GetTypeByName("IntelliVerseX.Auth.Editor.IVXAuthPrefabBuilder");
                if (builderType != null)
                {
                    var createMethod = builderType.GetMethod("CreateAuthCanvasPrefab",
                        System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);

                    if (createMethod != null)
                    {
                        createMethod.Invoke(null, null);
                    }
                }

                var scenePath = AUTH_ROOT + "/Scenes/IVX_AuthDemo.unity";
                EditorSceneManager.SaveScene(scene, scenePath);

                AddSceneToBuildSettings(scenePath);

                Debug.Log($"[IVXSDKSetupWizard] Created Auth Demo Scene at: {scenePath}");
                EditorUtility.DisplayDialog("Demo Scene Created",
                    $"Auth Demo Scene created at:\n{scenePath}\n\nPress Play to test.", "OK");
            };
        }

        private void CreateFriendsDemoScene()
        {
            // Use delayCall to avoid GUI layout issues when changing scenes
            EditorApplication.delayCall += () =>
            {
                if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;

                EnsureDirectoryExists("Assets/Scenes");

                var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);

                // Set camera
                var camera = Camera.main;
                if (camera != null)
                {
                    camera.backgroundColor = new Color(0.22f, 0.27f, 0.35f, 1f);
                    camera.clearFlags = CameraClearFlags.SolidColor;
                }

                // Add EventSystem
                EnsureEventSystem();

                // Add Friends Canvas
                AddFriendsUIToScene();

                // Create Demo Button
                CreateDemoButton("Open Friends", "OpenFriendsButton");

                var scenePath = "Assets/Scenes/IVX_FriendsDemo.unity";
                EditorSceneManager.SaveScene(scene, scenePath);

                AddSceneToBuildSettings(scenePath);

                Debug.Log($"[IVXSDKSetupWizard] Created Friends Demo Scene at: {scenePath}");
                EditorUtility.DisplayDialog("Demo Scene Created",
                    $"Friends Demo Scene created at:\n{scenePath}\n\nPress Play to test.", "OK");
            };
        }

        #endregion

        #region Test Scene Creation

        private void CreateAuthTestScene()
        {
            CreateTestScene("IVX_AuthTest", () =>
            {
                AddAuthCanvasToScene();
            });
        }

        private void CreateFriendsTestScene()
        {
            CreateTestScene("IVX_FriendsTest", () =>
            {
                AddFriendsUIToScene();
                CreateDemoButton("Open Friends", "OpenFriendsButton");
            });
        }

        private void CreateWalletTestScene()
        {
            CreateTestScene("IVX_WalletTest", () =>
            {
                CreateTestLabel("Wallet Test Scene", "Add IVXWalletDisplay to test wallet functionality");
            });
        }

        private void CreateLeaderboardTestScene()
        {
            CreateTestScene("IVX_LeaderboardTest", () =>
            {
                CreateTestLabel("Leaderboard Test Scene", "Add IVXLeaderboardUI to test leaderboard functionality");
            });
        }

        private void CreateAdsTestScene()
        {
            CreateTestScene("IVX_AdsTest", () =>
            {
                CreateTestLabel("Ads Test Scene", "Test ad loading, display, and rewards");
                CreateDemoButton("Show Rewarded Ad", "ShowRewardedAdButton");
                CreateDemoButton("Show Interstitial", "ShowInterstitialButton", new Vector2(0, -80));
            });
        }

        private void CreateFullTestScene()
        {
            CreateTestScene("IVX_FullTest", () =>
            {
                AddManagersToCurrentScene();
                AddAuthCanvasToScene();
                AddFriendsUIToScene();
                CreateTestLabel("Full Integration Test", "All SDK features available for testing");
            });
        }

        private void CreateTestScene(string sceneName, Action setupAction)
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;

            EnsureDirectoryExists("Assets/Scenes/Tests");

            var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);

            // Set camera
            var camera = Camera.main;
            if (camera != null)
            {
                camera.backgroundColor = new Color(0.15f, 0.18f, 0.22f, 1f);
                camera.clearFlags = CameraClearFlags.SolidColor;
            }

            EnsureEventSystem();

            setupAction?.Invoke();

            var scenePath = $"Assets/Scenes/Tests/{sceneName}.unity";
            EditorSceneManager.SaveScene(scene, scenePath);

            AddSceneToBuildSettings(scenePath);

            Debug.Log($"[IVXSDKSetupWizard] Created test scene: {scenePath}");
            EditorUtility.DisplayDialog("Test Scene Created",
                $"Test scene created at:\n{scenePath}\n\nPress Play to test.", "OK");
        }

        private void OpenTestScenesFolder()
        {
            var folder = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>("Assets/Scenes/Tests");
            if (folder != null)
            {
                Selection.activeObject = folder;
                EditorGUIUtility.PingObject(folder);
            }
            else
            {
                EnsureDirectoryExists("Assets/Scenes/Tests");
                AssetDatabase.Refresh();
                OpenTestScenesFolder();
            }
        }

        private void AddTestScenesToBuild()
        {
            var testScenesPath = "Assets/Scenes/Tests";
            if (!Directory.Exists(testScenesPath)) return;

            var sceneFiles = Directory.GetFiles(testScenesPath, "*.unity");
            var scenes = new List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);

            foreach (var sceneFile in sceneFiles)
            {
                var path = sceneFile.Replace("\\", "/");
                if (!scenes.Any(s => s.path == path))
                {
                    scenes.Add(new EditorBuildSettingsScene(path, true));
                }
            }

            EditorBuildSettings.scenes = scenes.ToArray();
            EditorUtility.DisplayDialog("Build Settings Updated",
                $"Added {sceneFiles.Length} test scenes to build settings.", "OK");
        }

        #endregion

        #region UI Actions

        private void SetupWalletUI()
        {
            // Try to use the prefab builder
            var builderType = GetTypeByName("IntelliVerseX.Games.Wallet.Editor.IVXGWalletPrefabBuilder");
            if (builderType != null)
            {
                var addToSceneMethod = builderType.GetMethod("AddWalletToScene",
                    System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);

                if (addToSceneMethod != null)
                {
                    addToSceneMethod.Invoke(null, null);
                    CheckWalletModule();
                    Debug.Log("[IVXSDKSetupWizard] Wallet UI added to scene");
                    return;
                }
            }

            // Fallback: show instructions
            Debug.Log("[IVXSDKSetupWizard] Wallet UI setup - Add IVXGWalletDisplay component to your UI");
            EditorUtility.DisplayDialog("Wallet Setup",
                "To set up wallet UI:\n\n" +
                "1. Go to IntelliVerseX → Wallet → Add to Scene\n" +
                "   OR manually:\n" +
                "2. Add IVXGWalletRuntime for runtime management\n" +
                "3. Add IVXGWalletDisplay to show balance\n" +
                "4. Configure wallet type (Game/Global)\n\n" +
                "Usage:\n" +
                "await IVXGWalletManager.CreditGameAsync(100);\n" +
                "await IVXGWalletManager.TrySpendGameAsync(50);",
                "OK");
        }

        private void CreateWalletPrefabs()
        {
            var builderType = GetTypeByName("IntelliVerseX.Games.Wallet.Editor.IVXGWalletPrefabBuilder");
            if (builderType != null)
            {
                var createAllMethod = builderType.GetMethod("CreateAllPrefabs",
                    System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);

                if (createAllMethod != null)
                {
                    createAllMethod.Invoke(null, null);
                    CheckWalletModule();
                    return;
                }
            }

            Debug.LogWarning("[IVXSDKSetupWizard] IVXGWalletPrefabBuilder not found");
            EditorUtility.DisplayDialog("Error",
                "Wallet Prefab Builder not found.\n\nMake sure the wallet module is properly installed.",
                "OK");
        }

        private void SetupSocialFeatures()
        {
            AddShareToScene();
            AddRateAppToScene();
        }

        private void CreateSocialPrefabs()
        {
            var builderType = GetTypeByName("IntelliVerseX.Games.Social.Editor.IVXGSocialPrefabBuilder");
            if (builderType != null)
            {
                var createAllMethod = builderType.GetMethod("CreateAllPrefabs",
                    System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);

                if (createAllMethod != null)
                {
                    createAllMethod.Invoke(null, null);
                    CheckSocialModule();
                    return;
                }
            }

            Debug.LogWarning("[IVXSDKSetupWizard] IVXGSocialPrefabBuilder not found");
        }

        private void AddShareToScene()
        {
            var builderType = GetTypeByName("IntelliVerseX.Games.Social.Editor.IVXGSocialPrefabBuilder");
            if (builderType != null)
            {
                var addMethod = builderType.GetMethod("AddShareToScene",
                    System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);

                if (addMethod != null)
                {
                    addMethod.Invoke(null, null);
                    CheckSocialModule();
                    return;
                }
            }

            Debug.LogWarning("[IVXSDKSetupWizard] IVXGSocialPrefabBuilder not found");
        }

        private void AddRateAppToScene()
        {
            var builderType = GetTypeByName("IntelliVerseX.Games.Social.Editor.IVXGSocialPrefabBuilder");
            if (builderType != null)
            {
                var addMethod = builderType.GetMethod("AddRateAppToScene",
                    System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);

                if (addMethod != null)
                {
                    addMethod.Invoke(null, null);
                    CheckSocialModule();
                    return;
                }
            }

            Debug.LogWarning("[IVXSDKSetupWizard] IVXGSocialPrefabBuilder not found");
        }

        private void SetupLeaderboardUI()
        {
            // Try to use the prefab builder
            var builderType = GetTypeByName("IntelliVerseX.Games.Leaderboard.Editor.IVXGLeaderboardPrefabBuilder");
            if (builderType != null)
            {
                var addToSceneMethod = builderType.GetMethod("AddLeaderboardToScene",
                    System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);

                if (addToSceneMethod != null)
                {
                    addToSceneMethod.Invoke(null, null);
                    CheckLeaderboardModule();
                    Debug.Log("[IVXSDKSetupWizard] Leaderboard UI added to scene");
                    return;
                }
            }

            // Fallback: show instructions
            Debug.Log("[IVXSDKSetupWizard] Leaderboard UI setup - Add IVXGLeaderboardUI component to your UI");
            EditorUtility.DisplayDialog("Leaderboard Setup",
                "To set up leaderboard UI:\n\n" +
                "1. Go to IntelliVerseX → Leaderboard → Add to Scene\n" +
                "   OR manually:\n" +
                "2. Add IVXGLeaderboard component for runtime management\n" +
                "3. Add IVXGLeaderboardUI component to a Canvas\n" +
                "4. Create entry prefab with IVXGLeaderboardEntryView\n" +
                "5. Configure references and call RefreshLeaderboardFromServer()",
                "OK");
        }

        private void ConfigureAds()
        {
            EditorUtility.DisplayDialog("Ads Configuration",
                "To configure ads:\n\n" +
                "1. Install LevelPlay or Appodeal SDK\n" +
                "2. Configure ad unit IDs in IVXAdsManager\n" +
                "3. Initialize ads in your game's startup",
                "OK");
        }

        private void ConfigureIAP()
        {
            EditorUtility.DisplayDialog("IAP Configuration",
                "To configure IAP:\n\n" +
                "1. Install Unity Purchasing from Package Manager\n" +
                "2. Configure product IDs in IVXIAPConfig\n" +
                "3. Initialize IAP in your game's startup",
                "OK");
        }

        #endregion

        #region Verification

        private void VerifyFullSetup()
        {
            RefreshAllModuleStatus();

            var modules = new[]
            {
                (coreModule, "Core"),
                (identityModule, "Identity"),
                (backendModule, "Backend"),
                (authModule, "Authentication"),
                (friendsModule, "Friends"),
                (walletModule, "Wallet"),
                (leaderboardModule, "Leaderboard"),
                (quizModule, "Quiz"),
                (localizationModule, "Localization"),
                (adsModule, "Ads"),
                (iapModule, "IAP"),
                (retentionModule, "Retention")
            };

            int completed = modules.Count(m => m.Item1.isSetupComplete);
            int total = modules.Length;

            var incomplete = modules.Where(m => !m.Item1.isSetupComplete)
                                   .Select(m => m.Item2)
                                   .ToList();

            string message = $"Setup Status: {completed}/{total} modules complete.";

            if (incomplete.Count > 0)
            {
                message += "\n\nIncomplete modules:\n• " + string.Join("\n• ", incomplete);
            }
            else
            {
                message += "\n\n✅ All modules are configured!";
            }

            EditorUtility.DisplayDialog("Setup Verification", message, "OK");
        }

        #endregion

        #region Utility Methods

        private void EnsureDirectoryExists(string path)
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

        /// <summary>
        /// Ensures an EventSystem exists in the scene.
        /// Automatically detects the active input system and uses the appropriate input module:
        /// - New Input System: Uses InputSystemUIInputModule
        /// - Legacy Input Manager: Uses StandaloneInputModule
        /// - Both: Uses InputSystemUIInputModule (preferred)
        /// </summary>
        private void EnsureEventSystem()
        {
            var eventSystemType = typeof(UnityEngine.EventSystems.EventSystem);
            if (GameObject.FindAnyObjectByType(eventSystemType) != null)
            {
                return; // EventSystem already exists
            }

            var eventSystem = new GameObject("EventSystem");
            eventSystem.AddComponent<UnityEngine.EventSystems.EventSystem>();

            // Check if the new Input System package is installed and should be used
            bool useNewInputSystem = IsNewInputSystemActive();

            if (useNewInputSystem)
            {
                // Try to add InputSystemUIInputModule (from Input System package)
                var inputModuleType = GetTypeByName("UnityEngine.InputSystem.UI.InputSystemUIInputModule");
                if (inputModuleType != null)
                {
                    eventSystem.AddComponent(inputModuleType);
                    Debug.Log("[IVXSDKSetupWizard] Created EventSystem with InputSystemUIInputModule (New Input System)");
                }
                else
                {
                    // Fallback to StandaloneInputModule if type not found (shouldn't happen)
                    Debug.LogWarning("[IVXSDKSetupWizard] New Input System is active but InputSystemUIInputModule not found. Using StandaloneInputModule as fallback.");
                    eventSystem.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
                }
            }
            else
            {
                // Use legacy StandaloneInputModule
                eventSystem.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
                Debug.Log("[IVXSDKSetupWizard] Created EventSystem with StandaloneInputModule (Legacy Input)");
            }
        }

        /// <summary>
        /// Checks if the New Input System is active in the project settings.
        /// Returns true if:
        /// - Input System package is installed AND
        /// - Player Settings has Input System enabled (either "Input System Package" or "Both")
        /// </summary>
        private bool IsNewInputSystemActive()
        {
            // Check if InputSystem package is installed by looking for its main type
            var inputSystemType = GetTypeByName("UnityEngine.InputSystem.InputSystem");
            if (inputSystemType == null)
            {
                return false; // Input System package not installed
            }

            // Check Player Settings for active input handling
            // PlayerSettings.GetActiveInputHandler() returns:
            // 0 = Input Manager (old)
            // 1 = Input System Package (new)
            // 2 = Both
            try
            {
                var playerSettingsType = typeof(UnityEditor.PlayerSettings);
                
                // Try to get the activeInputHandler property via reflection (Unity 2019.3+)
                var prop = playerSettingsType.GetProperty("activeInputHandler", 
                    System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
                
                if (prop != null)
                {
                    var value = (int)prop.GetValue(null);
                    // 1 = Input System Package, 2 = Both
                    return value >= 1;
                }

                // Alternative: Check via EditorSettings (older Unity versions)
                // If we can't determine, assume new input system if the package is installed
                // and the InputSystemUIInputModule type exists
                var inputModuleType = GetTypeByName("UnityEngine.InputSystem.UI.InputSystemUIInputModule");
                return inputModuleType != null;
            }
            catch (System.Exception ex)
            {
                Debug.LogWarning($"[IVXSDKSetupWizard] Could not determine input system setting: {ex.Message}");
                // If we can't determine, check if InputSystemUIInputModule exists
                return GetTypeByName("UnityEngine.InputSystem.UI.InputSystemUIInputModule") != null;
            }
        }

        private void CreateDemoButton(string text, string name, Vector2? position = null)
        {
            var pos = position ?? Vector2.zero;

            var canvasGO = new GameObject("DemoCanvas");
            var canvas = canvasGO.AddComponent<UnityEngine.Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 50;

            var scaler = canvasGO.AddComponent<UnityEngine.UI.CanvasScaler>();
            scaler.uiScaleMode = UnityEngine.UI.CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080, 1920);
            scaler.matchWidthOrHeight = 0.5f;

            canvasGO.AddComponent<UnityEngine.UI.GraphicRaycaster>();

            var buttonGO = new GameObject(name);
            buttonGO.transform.SetParent(canvasGO.transform, false);

            var buttonRect = buttonGO.AddComponent<RectTransform>();
            buttonRect.anchorMin = new Vector2(0.5f, 0.5f);
            buttonRect.anchorMax = new Vector2(0.5f, 0.5f);
            buttonRect.sizeDelta = new Vector2(280, 70);
            buttonRect.anchoredPosition = pos;

            var buttonImage = buttonGO.AddComponent<UnityEngine.UI.Image>();
            buttonImage.color = accentColor;

            buttonGO.AddComponent<UnityEngine.UI.Button>();

            var textGO = new GameObject("Text");
            textGO.transform.SetParent(buttonGO.transform, false);

            var textRect = textGO.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.sizeDelta = Vector2.zero;

            var tmp = textGO.AddComponent<TMPro.TextMeshProUGUI>();
            tmp.text = text;
            tmp.alignment = TMPro.TextAlignmentOptions.Center;
            tmp.fontSize = 28;
            tmp.fontStyle = TMPro.FontStyles.Bold;
            tmp.color = Color.white;
        }

        private void CreateTestLabel(string title, string description)
        {
            var canvasGO = new GameObject("InfoCanvas");
            var canvas = canvasGO.AddComponent<UnityEngine.Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 10;

            var scaler = canvasGO.AddComponent<UnityEngine.UI.CanvasScaler>();
            scaler.uiScaleMode = UnityEngine.UI.CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080, 1920);

            canvasGO.AddComponent<UnityEngine.UI.GraphicRaycaster>();

            // Title
            var titleGO = new GameObject("Title");
            titleGO.transform.SetParent(canvasGO.transform, false);

            var titleRect = titleGO.AddComponent<RectTransform>();
            titleRect.anchorMin = new Vector2(0.5f, 1);
            titleRect.anchorMax = new Vector2(0.5f, 1);
            titleRect.pivot = new Vector2(0.5f, 1);
            titleRect.sizeDelta = new Vector2(800, 80);
            titleRect.anchoredPosition = new Vector2(0, -50);

            var titleTMP = titleGO.AddComponent<TMPro.TextMeshProUGUI>();
            titleTMP.text = title;
            titleTMP.alignment = TMPro.TextAlignmentOptions.Center;
            titleTMP.fontSize = 36;
            titleTMP.fontStyle = TMPro.FontStyles.Bold;
            titleTMP.color = Color.white;

            // Description
            var descGO = new GameObject("Description");
            descGO.transform.SetParent(canvasGO.transform, false);

            var descRect = descGO.AddComponent<RectTransform>();
            descRect.anchorMin = new Vector2(0.5f, 1);
            descRect.anchorMax = new Vector2(0.5f, 1);
            descRect.pivot = new Vector2(0.5f, 1);
            descRect.sizeDelta = new Vector2(800, 60);
            descRect.anchoredPosition = new Vector2(0, -130);

            var descTMP = descGO.AddComponent<TMPro.TextMeshProUGUI>();
            descTMP.text = description;
            descTMP.alignment = TMPro.TextAlignmentOptions.Center;
            descTMP.fontSize = 20;
            descTMP.color = new Color(0.7f, 0.7f, 0.7f);
        }

        private void AddSceneToBuildSettings(string scenePath)
        {
            var scenes = new List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);

            if (!scenes.Any(s => s.path == scenePath))
            {
                scenes.Add(new EditorBuildSettingsScene(scenePath, true));
                EditorBuildSettings.scenes = scenes.ToArray();
            }
        }

        /// <summary>
        /// Gets a Type by its full name, searching all loaded assemblies.
        /// This is the most reliable way to check if SDK scripts are available,
        /// regardless of whether they're in Assets or Packages.
        /// </summary>
        private static Type GetTypeByName(string fullName)
        {
            if (string.IsNullOrEmpty(fullName)) return null;
            
            // Try direct type lookup first
            var type = Type.GetType(fullName);
            if (type != null) return type;
            
            // Search all loaded assemblies
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                try
                {
                    type = assembly.GetType(fullName);
                    if (type != null) return type;
                }
                catch
                {
                    // Ignore assemblies that can't be searched
                }
            }
            return null;
        }

        #endregion
    }
}
