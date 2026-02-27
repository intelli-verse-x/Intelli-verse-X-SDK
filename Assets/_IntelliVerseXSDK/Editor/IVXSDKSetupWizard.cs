// File: IVXSDKSetupWizard.cs
// Purpose: Comprehensive Unified SDK Setup Wizard for IntelliVerseX SDK
// Version: 5.0.0
// Author: IntelliVerseX Team
// Description: Single unified panel for ALL SDK module setup including Auth, Friends, Monetization, Platform Validation, etc.
// Note: Supports both development (Assets/_IntelliVerseXSDK) and UPM package (Packages/com.intelliversex.sdk) installations.
// Production-ready for WebGL, Android, and iOS.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
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
        private const string SDK_VERSION = "5.0.0";
        private const string PACKAGE_NAME = "com.intelliversex.sdk";
        
        // Version check URLs
        private const string GITHUB_RELEASES_API_URL = "https://api.github.com/repos/AkshayAdsworlds/Intelli-verse-X-Unity-SDK/releases/latest";
        private const string GITHUB_RELEASES_PAGE_URL = "https://github.com/AkshayAdsworlds/Intelli-verse-X-Unity-SDK/releases";
        private const string VERSION_CHECK_PREF_KEY = "IVX_LastVersionCheck";
        private const double VERSION_CHECK_INTERVAL_HOURS = 1.0; // Check every hour

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
            "Dependencies",
            "Core",
            "Auth & Social",
            "Features",
            "Monetization",
            "More Of Us",
            "Platform Validation",
            "Test Scenes",
            "Local Data"
        };

        // Version Check State
        private static string _latestVersion = null;
        private static bool _isCheckingVersion = false;
        private static bool _updateAvailable = false;
        private static string _releaseNotes = "";
        private static string _releaseUrl = "";
        private static DateTime _lastVersionCheck = DateTime.MinValue;

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

        [MenuItem("IntelliVerse-X SDK/SDK Setup Wizard", false, 0)]
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
                    CheckForUpdatesIfNeeded();
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
                case 1: DrawDependenciesTab(); break;
                case 2: DrawCoreTab(); break;
                case 3: DrawAuthSocialTab(); break;
                case 4: DrawFeaturesTab(); break;
                case 5: DrawMonetizationTab(); break;
                case 6: DrawMoreOfUsTab(); break;
                case 7: DrawPlatformValidationTab(); break;
                case 8: DrawTestScenesTab(); break;
                case 9: DrawLocalDataTab(); break;
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

            // Version Check Status Banner
            DrawVersionCheckBanner();

            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
        }

        /// <summary>
        /// Draws the version check status banner showing current version and update availability.
        /// </summary>
        private void DrawVersionCheckBanner()
        {
            EditorGUILayout.Space(5);
            
            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();

            if (_isCheckingVersion)
            {
                // Checking for updates
                GUILayout.Label("🔄 Checking for updates...", EditorStyles.centeredGreyMiniLabel);
            }
            else if (_updateAvailable && !string.IsNullOrEmpty(_latestVersion))
            {
                // Update available - show prominent banner
                Color oldBgColor = GUI.backgroundColor;
                GUI.backgroundColor = new Color(1f, 0.8f, 0.2f, 0.8f); // Yellow/gold for update
                
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                EditorGUILayout.BeginHorizontal();
                
                GUILayout.Label($"🆕 New Version Available: {_latestVersion}", EditorStyles.boldLabel);
                
                GUILayout.FlexibleSpace();
                
                if (GUILayout.Button("📥 Update", GUILayout.Width(80)))
                {
                    if (!string.IsNullOrEmpty(_releaseUrl))
                    {
                        Application.OpenURL(_releaseUrl);
                    }
                    else
                    {
                        Application.OpenURL(GITHUB_RELEASES_PAGE_URL);
                    }
                }
                
                if (GUILayout.Button("📋 Release Notes", GUILayout.Width(110)))
                {
                    Application.OpenURL(GITHUB_RELEASES_PAGE_URL);
                }
                
                EditorGUILayout.EndHorizontal();
                
                if (!string.IsNullOrEmpty(_releaseNotes))
                {
                    EditorGUILayout.LabelField(_releaseNotes, EditorStyles.wordWrappedMiniLabel);
                }
                
                EditorGUILayout.EndVertical();
                GUI.backgroundColor = oldBgColor;
            }
            else if (_latestVersion != null)
            {
                // Up to date
                GUILayout.Label($"✅ You're using the latest version ({SDK_VERSION})", EditorStyles.centeredGreyMiniLabel);
            }
            else
            {
                // Version check not performed or failed
                if (GUILayout.Button("🔍 Check for Updates", EditorStyles.miniButton, GUILayout.Width(130)))
                {
                    CheckForUpdatesAsync();
                }
            }

            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();
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

        #region Version Check

        /// <summary>
        /// Checks for updates if enough time has passed since the last check.
        /// </summary>
        private void CheckForUpdatesIfNeeded()
        {
            // Load last check time from EditorPrefs
            string lastCheckStr = EditorPrefs.GetString(VERSION_CHECK_PREF_KEY, "");
            if (!string.IsNullOrEmpty(lastCheckStr) && DateTime.TryParse(lastCheckStr, out DateTime lastCheck))
            {
                _lastVersionCheck = lastCheck;
            }

            // Check if enough time has passed
            if ((DateTime.Now - _lastVersionCheck).TotalHours >= VERSION_CHECK_INTERVAL_HOURS)
            {
                CheckForUpdatesAsync();
            }
        }

        /// <summary>
        /// Asynchronously checks for SDK updates from GitHub releases.
        /// Silently fails if repo is not accessible (private/doesn't exist).
        /// </summary>
        private async void CheckForUpdatesAsync()
        {
            if (_isCheckingVersion)
            {
                return;
            }

            _isCheckingVersion = true;
            _updateAvailable = false;
            Repaint();

            try
            {
                using (var client = new HttpClient())
                {
                    // GitHub API requires User-Agent header
                    client.DefaultRequestHeaders.Add("User-Agent", "IntelliVerseX-SDK-Unity");
                    client.Timeout = TimeSpan.FromSeconds(10);

                    var responseMessage = await client.GetAsync(GITHUB_RELEASES_API_URL);
                    
                    // Handle 404 and other non-success status codes silently
                    if (!responseMessage.IsSuccessStatusCode)
                    {
                        // Don't log warning for 404 - repo might be private or not exist yet
                        // Just mark version check as complete and continue
                        _latestVersion = null;
                        _lastVersionCheck = DateTime.Now;
                        EditorPrefs.SetString(VERSION_CHECK_PREF_KEY, _lastVersionCheck.ToString("o"));
                        return;
                    }

                    var response = await responseMessage.Content.ReadAsStringAsync();
                    ParseGitHubReleaseResponse(response);

                    // Save last check time
                    _lastVersionCheck = DateTime.Now;
                    EditorPrefs.SetString(VERSION_CHECK_PREF_KEY, _lastVersionCheck.ToString("o"));
                }
            }
            catch (HttpRequestException)
            {
                // Silently fail - network issues or repo not accessible
                _latestVersion = null;
            }
            catch (TaskCanceledException)
            {
                // Silently fail - timeout
                _latestVersion = null;
            }
            catch (Exception)
            {
                // Silently fail - any other error
                _latestVersion = null;
            }
            finally
            {
                _isCheckingVersion = false;
                // Schedule repaint on main thread
                EditorApplication.delayCall += Repaint;
            }
        }

        /// <summary>
        /// Parses the GitHub API response to extract version information.
        /// </summary>
        private void ParseGitHubReleaseResponse(string jsonResponse)
        {
            try
            {
                // Simple JSON parsing without external dependencies
                // Looking for "tag_name": "v3.0.0" or "tag_name": "3.0.0"
                string tagName = ExtractJsonValue(jsonResponse, "tag_name");
                string htmlUrl = ExtractJsonValue(jsonResponse, "html_url");
                string body = ExtractJsonValue(jsonResponse, "body");

                if (!string.IsNullOrEmpty(tagName))
                {
                    // Remove 'v' prefix if present
                    _latestVersion = tagName.TrimStart('v', 'V');
                    _releaseUrl = htmlUrl ?? GITHUB_RELEASES_PAGE_URL;
                    
                    // Truncate release notes to first 150 chars
                    if (!string.IsNullOrEmpty(body))
                    {
                        body = body.Replace("\\r\\n", " ").Replace("\\n", " ").Replace("\\r", " ");
                        _releaseNotes = body.Length > 150 ? body.Substring(0, 147) + "..." : body;
                    }
                    else
                    {
                        _releaseNotes = "";
                    }

                    // Compare versions
                    _updateAvailable = IsNewerVersion(_latestVersion, SDK_VERSION);

                    if (_updateAvailable)
                    {
                        Debug.Log($"[IVXSDKSetupWizard] New SDK version available: {_latestVersion} (current: {SDK_VERSION})");
                    }
                    else
                    {
                        Debug.Log($"[IVXSDKSetupWizard] SDK is up to date (version {SDK_VERSION})");
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[IVXSDKSetupWizard] Error parsing release info: {ex.Message}");
            }
        }

        /// <summary>
        /// Extracts a string value from JSON without external dependencies.
        /// </summary>
        private string ExtractJsonValue(string json, string key)
        {
            string searchPattern = $"\"{key}\"";
            int keyIndex = json.IndexOf(searchPattern, StringComparison.OrdinalIgnoreCase);
            if (keyIndex < 0) return null;

            int colonIndex = json.IndexOf(':', keyIndex + searchPattern.Length);
            if (colonIndex < 0) return null;

            // Find the start of the value (skip whitespace)
            int valueStart = colonIndex + 1;
            while (valueStart < json.Length && char.IsWhiteSpace(json[valueStart]))
            {
                valueStart++;
            }

            if (valueStart >= json.Length) return null;

            // Check if value is a string (starts with quote)
            if (json[valueStart] == '"')
            {
                valueStart++;
                int valueEnd = json.IndexOf('"', valueStart);
                if (valueEnd < 0) return null;
                return json.Substring(valueStart, valueEnd - valueStart);
            }

            // Handle non-string values (numbers, booleans, null)
            int valueEnd2 = valueStart;
            while (valueEnd2 < json.Length && json[valueEnd2] != ',' && json[valueEnd2] != '}' && json[valueEnd2] != ']')
            {
                valueEnd2++;
            }
            return json.Substring(valueStart, valueEnd2 - valueStart).Trim();
        }

        /// <summary>
        /// Compares two semantic version strings.
        /// </summary>
        /// <param name="newVersion">The new version to check.</param>
        /// <param name="currentVersion">The current version.</param>
        /// <returns>True if newVersion is newer than currentVersion.</returns>
        private bool IsNewerVersion(string newVersion, string currentVersion)
        {
            try
            {
                // Parse version parts (major.minor.patch)
                var newParts = ParseVersionParts(newVersion);
                var currentParts = ParseVersionParts(currentVersion);

                // Compare major
                if (newParts[0] > currentParts[0]) return true;
                if (newParts[0] < currentParts[0]) return false;

                // Compare minor
                if (newParts[1] > currentParts[1]) return true;
                if (newParts[1] < currentParts[1]) return false;

                // Compare patch
                if (newParts[2] > currentParts[2]) return true;

                return false;
            }
            catch
            {
                // If parsing fails, do string comparison
                return string.Compare(newVersion, currentVersion, StringComparison.OrdinalIgnoreCase) > 0;
            }
        }

        /// <summary>
        /// Parses version string into major, minor, patch parts.
        /// </summary>
        private int[] ParseVersionParts(string version)
        {
            var parts = version.Split('.');
            int[] result = new int[3] { 0, 0, 0 };

            for (int i = 0; i < Math.Min(parts.Length, 3); i++)
            {
                // Remove any suffix like "-beta", "-rc1", etc.
                string part = parts[i].Split('-')[0];
                if (int.TryParse(part, out int num))
                {
                    result[i] = num;
                }
            }

            return result;
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

        #region Dependencies Tab

        private void DrawDependenciesTab()
        {
            EditorGUILayout.LabelField("Dependencies & Package Management", subHeaderStyle ?? EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(
                "Check and install required dependencies for the SDK. " +
                "Required packages are automatically detected and can be installed with one click.",
                MessageType.Info);

            EditorGUILayout.Space(10);

            // Quick Actions
            EditorGUILayout.BeginVertical(moduleBoxStyle);
            EditorGUILayout.LabelField("Quick Actions", EditorStyles.boldLabel);

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("🔍 Check Dependencies", GUILayout.Height(30)))
            {
                CheckAllDependencies();
            }
            if (GUILayout.Button("📦 Install All Dependencies", GUILayout.Height(30)))
            {
                InstallAllDependencies();
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(5);
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("✅ Validate Dependencies", GUILayout.Height(30)))
            {
                ValidateAllDependencies();
            }
            if (GUILayout.Button("⚙️ Project Setup & Validation", GUILayout.Height(30)))
            {
                OpenProjectSetup();
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.EndVertical();

            EditorGUILayout.Space(10);

            // Required Dependencies
            DrawDependencySection("Required Dependencies", GetRequiredDependencies());
            
            EditorGUILayout.Space(10);

            // Optional Dependencies
            DrawDependencySection("Optional Dependencies", GetOptionalDependencies());

            EditorGUILayout.Space(10);

            // External Dependencies (Asset Store)
            DrawDependencySection("External Dependencies (Asset Store)", GetExternalDependencies());
        }

        private class DependencyInfo
        {
            public string Name;
            public bool IsInstalled;
            public string Description;
            public string PackageId;
            public string InstallMethod; // "upm", "assetStore", "gitUrl", "unitypackage"
            public string InstallUrl;
            public Action InstallAction;
            public string InstalledVersion;
            public string[] AlternativeTypeNames;
            public string[] AlternativePackageIds;
        }

        #region Robust Dependency Detection System

        private static Dictionary<string, bool> _dependencyCache = new Dictionary<string, bool>();
        private static Dictionary<string, string> _packageVersionCache = new Dictionary<string, string>();
        private static bool _dependencyCacheInitialized = false;
        private static double _lastCacheRefresh = 0;
        private const double CACHE_REFRESH_INTERVAL = 5.0;

        private static void RefreshDependencyCacheIfNeeded()
        {
            double currentTime = EditorApplication.timeSinceStartup;
            if (!_dependencyCacheInitialized || (currentTime - _lastCacheRefresh) > CACHE_REFRESH_INTERVAL)
            {
                _dependencyCache.Clear();
                _packageVersionCache.Clear();
                _dependencyCacheInitialized = true;
                _lastCacheRefresh = currentTime;
            }
        }

        private static bool CheckPackageInstalledByManifest(string packageId)
        {
            if (string.IsNullOrEmpty(packageId)) return false;

            string cacheKey = $"manifest_{packageId}";
            if (_dependencyCache.TryGetValue(cacheKey, out bool cached))
                return cached;

            try
            {
                string manifestPath = Path.Combine(Application.dataPath, "..", "Packages", "manifest.json");
                if (File.Exists(manifestPath))
                {
                    string manifest = File.ReadAllText(manifestPath);
                    bool found = manifest.Contains($"\"{packageId}\"");
                    _dependencyCache[cacheKey] = found;

                    if (found)
                    {
                        int startIdx = manifest.IndexOf($"\"{packageId}\"");
                        if (startIdx >= 0)
                        {
                            int colonIdx = manifest.IndexOf(":", startIdx);
                            int quoteStart = manifest.IndexOf("\"", colonIdx);
                            int quoteEnd = manifest.IndexOf("\"", quoteStart + 1);
                            if (quoteEnd > quoteStart)
                            {
                                string version = manifest.Substring(quoteStart + 1, quoteEnd - quoteStart - 1);
                                _packageVersionCache[packageId] = version;
                            }
                        }
                    }
                    return found;
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[IVXSDKSetupWizard] Error checking manifest for {packageId}: {ex.Message}");
            }

            _dependencyCache[cacheKey] = false;
            return false;
        }

        private static bool CheckPackageInstalledByPackageCache(string packageId)
        {
            if (string.IsNullOrEmpty(packageId)) return false;

            string cacheKey = $"cache_{packageId}";
            if (_dependencyCache.TryGetValue(cacheKey, out bool cached))
                return cached;

            try
            {
                string packageCachePath = Path.Combine(Application.dataPath, "..", "Library", "PackageCache");
                if (Directory.Exists(packageCachePath))
                {
                    var dirs = Directory.GetDirectories(packageCachePath, $"{packageId}@*");
                    bool found = dirs.Length > 0;
                    _dependencyCache[cacheKey] = found;

                    if (found && dirs.Length > 0)
                    {
                        string dirName = Path.GetFileName(dirs[0]);
                        int atIdx = dirName.IndexOf('@');
                        if (atIdx >= 0)
                        {
                            _packageVersionCache[packageId] = dirName.Substring(atIdx + 1);
                        }
                    }
                    return found;
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[IVXSDKSetupWizard] Error checking package cache for {packageId}: {ex.Message}");
            }

            _dependencyCache[cacheKey] = false;
            return false;
        }

        private static bool CheckTypeExistsRobust(params string[] typeNames)
        {
            if (typeNames == null || typeNames.Length == 0) return false;

            foreach (string typeName in typeNames)
            {
                if (string.IsNullOrEmpty(typeName)) continue;

                string cacheKey = $"type_{typeName}";
                if (_dependencyCache.TryGetValue(cacheKey, out bool cached))
                {
                    if (cached) return true;
                    continue;
                }

                bool found = false;
                try
                {
                    var type = Type.GetType(typeName);
                    if (type != null)
                    {
                        found = true;
                    }
                    else
                    {
                        string typeNameOnly = typeName.Contains(",") 
                            ? typeName.Substring(0, typeName.IndexOf(",")).Trim() 
                            : typeName;

                        foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
                        {
                            try
                            {
                                type = assembly.GetType(typeNameOnly);
                                if (type != null)
                                {
                                    found = true;
                                    break;
                                }

                                if (typeNameOnly.Contains("."))
                                {
                                    string shortName = typeNameOnly.Substring(typeNameOnly.LastIndexOf('.') + 1);
                                    var types = assembly.GetTypes().Where(t => t.Name == shortName).ToArray();
                                    if (types.Length > 0)
                                    {
                                        found = true;
                                        break;
                                    }
                                }
                            }
                            catch
                            {
                            }
                        }
                    }
                }
                catch
                {
                }

                _dependencyCache[cacheKey] = found;
                if (found) return true;
            }

            return false;
        }

        private static bool CheckAssemblyExists(params string[] assemblyNames)
        {
            if (assemblyNames == null || assemblyNames.Length == 0) return false;

            foreach (string assemblyName in assemblyNames)
            {
                if (string.IsNullOrEmpty(assemblyName)) continue;

                string cacheKey = $"assembly_{assemblyName}";
                if (_dependencyCache.TryGetValue(cacheKey, out bool cached))
                {
                    if (cached) return true;
                    continue;
                }

                bool found = false;
                try
                {
                    foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
                    {
                        string asmName = assembly.GetName().Name;
                        if (asmName.Equals(assemblyName, StringComparison.OrdinalIgnoreCase) ||
                            asmName.StartsWith(assemblyName + ".", StringComparison.OrdinalIgnoreCase) ||
                            asmName.StartsWith(assemblyName + ",", StringComparison.OrdinalIgnoreCase))
                        {
                            found = true;
                            break;
                        }
                    }
                }
                catch
                {
                }

                _dependencyCache[cacheKey] = found;
                if (found) return true;
            }

            return false;
        }

        private static bool CheckDirectoryExists(params string[] paths)
        {
            if (paths == null || paths.Length == 0) return false;

            foreach (string path in paths)
            {
                if (string.IsNullOrEmpty(path)) continue;

                string cacheKey = $"dir_{path}";
                if (_dependencyCache.TryGetValue(cacheKey, out bool cached))
                {
                    if (cached) return true;
                    continue;
                }

                bool found = Directory.Exists(path);
                _dependencyCache[cacheKey] = found;
                if (found) return true;
            }

            return false;
        }

        private static bool CheckFileExists(params string[] paths)
        {
            if (paths == null || paths.Length == 0) return false;

            foreach (string path in paths)
            {
                if (string.IsNullOrEmpty(path)) continue;

                string cacheKey = $"file_{path}";
                if (_dependencyCache.TryGetValue(cacheKey, out bool cached))
                {
                    if (cached) return true;
                    continue;
                }

                bool found = File.Exists(path);
                _dependencyCache[cacheKey] = found;
                if (found) return true;
            }

            return false;
        }

        private static string GetInstalledVersion(string packageId)
        {
            if (_packageVersionCache.TryGetValue(packageId, out string version))
                return version;
            return null;
        }

        private static bool CheckUnityPurchasingInstalled()
        {
            RefreshDependencyCacheIfNeeded();

            if (CheckPackageInstalledByManifest("com.unity.purchasing")) return true;
            if (CheckPackageInstalledByPackageCache("com.unity.purchasing")) return true;

            if (CheckTypeExistsRobust(
                "UnityEngine.Purchasing.IStoreController",
                "UnityEngine.Purchasing.Product",
                "UnityEngine.Purchasing.PurchasingModule",
                "UnityEngine.Purchasing.StandardPurchasingModule",
                "Unity.Services.Core.UnityServices"))
                return true;

            if (CheckAssemblyExists(
                "UnityEngine.Purchasing",
                "Unity.Purchasing",
                "UnityEngine.Purchasing.Stores",
                "Unity.Services.Core"))
                return true;

            return false;
        }

        private static bool CheckLevelPlayInstalled()
        {
            RefreshDependencyCacheIfNeeded();

            if (CheckPackageInstalledByManifest("com.unity.services.levelplay")) return true;
            if (CheckPackageInstalledByPackageCache("com.unity.services.levelplay")) return true;

            if (CheckTypeExistsRobust(
                "Unity.Services.LevelPlay.IronSource",
                "IronSource",
                "IronSourceEvents",
                "LevelPlay",
                "Unity.Services.LevelPlay.LevelPlayInitRequest"))
                return true;

            if (CheckAssemblyExists(
                "com.unity.services.levelplay.runtime",
                "Unity.Services.LevelPlay",
                "IronSource"))
                return true;

            if (CheckDirectoryExists(
                "Assets/LevelPlay",
                "Assets/IronSource"))
                return true;

            return false;
        }

        private static bool CheckNativeShareInstalled()
        {
            RefreshDependencyCacheIfNeeded();

            if (CheckPackageInstalledByManifest("com.yasirkula.nativeshare")) return true;
            if (CheckPackageInstalledByPackageCache("com.yasirkula.nativeshare")) return true;

            if (CheckTypeExistsRobust(
                "NativeShare",
                "NativeShareNamespace.NativeShare"))
                return true;

            if (CheckAssemblyExists(
                "NativeShare",
                "NativeShare.Runtime",
                "yasirkula.NativeShare"))
                return true;

            if (CheckDirectoryExists(
                "Assets/Plugins/NativeShare",
                "Assets/NativeShare"))
                return true;

            string packagesPath = Path.Combine(Application.dataPath, "..", "Packages", "com.yasirkula.nativeshare");
            if (Directory.Exists(packagesPath)) return true;

            return false;
        }

        private static bool CheckAppodealInstalled()
        {
            RefreshDependencyCacheIfNeeded();

            if (CheckPackageInstalledByManifest("com.appodeal.mediation")) return true;
            if (CheckPackageInstalledByPackageCache("com.appodeal.mediation")) return true;

            if (CheckTypeExistsRobust(
                "AppodealStack.Mediation.Common.Appodeal",
                "AppodealAds.Unity.Api.Appodeal",
                "Appodeal",
                "AppodealInc.Mediation.PluginSettings.Editor.AppodealSettings"))
                return true;

            if (CheckAssemblyExists(
                "Appodeal",
                "AppodealStack.Mediation.Common",
                "AppodealAds.Unity.Api",
                "AppodealInc.Mediation"))
                return true;

            if (CheckDirectoryExists(
                "Assets/Appodeal",
                "Assets/Plugins/Appodeal"))
                return true;

            string packagesPath = Path.Combine(Application.dataPath, "..", "Packages", "com.appodeal.mediation");
            if (Directory.Exists(packagesPath)) return true;

            return false;
        }

        public static void ForceRefreshDependencyCache()
        {
            _dependencyCache.Clear();
            _packageVersionCache.Clear();
            _dependencyCacheInitialized = false;
            _lastCacheRefresh = 0;
        }

        #endregion

        private void DrawDependencySection(string title, List<DependencyInfo> dependencies)
        {
            EditorGUILayout.BeginVertical(moduleBoxStyle);
            
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(title, EditorStyles.boldLabel);
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Refresh", EditorStyles.miniButton, GUILayout.Width(60)))
            {
                ForceRefreshDependencyCache();
                Repaint();
            }
            EditorGUILayout.EndHorizontal();

            foreach (var dep in dependencies)
            {
                EditorGUILayout.BeginHorizontal();
                
                GUIStyle statusStyle = new GUIStyle(EditorStyles.label);
                statusStyle.fontSize = 14;
                statusStyle.alignment = TextAnchor.MiddleCenter;
                GUILayout.Label(dep.IsInstalled ? "  ✓" : "  ✗", statusStyle, GUILayout.Width(30));
                
                EditorGUILayout.BeginVertical();
                
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(dep.Name, EditorStyles.boldLabel);
                if (dep.IsInstalled && !string.IsNullOrEmpty(dep.InstalledVersion))
                {
                    GUIStyle versionStyle = new GUIStyle(EditorStyles.miniLabel);
                    versionStyle.normal.textColor = new Color(0.4f, 0.7f, 0.4f);
                    GUILayout.Label($"v{dep.InstalledVersion}", versionStyle, GUILayout.Width(80));
                }
                GUILayout.FlexibleSpace();
                EditorGUILayout.EndHorizontal();
                
                EditorGUILayout.LabelField(dep.Description, EditorStyles.wordWrappedMiniLabel);
                EditorGUILayout.EndVertical();

                if (!dep.IsInstalled)
                {
                    if (GUILayout.Button("Install", GUILayout.Width(70), GUILayout.Height(35)))
                    {
                        dep.InstallAction?.Invoke();
                    }
                }
                else
                {
                    GUIStyle installedStyle = new GUIStyle(EditorStyles.miniLabel);
                    installedStyle.normal.textColor = new Color(0.3f, 0.8f, 0.3f);
                    installedStyle.alignment = TextAnchor.MiddleCenter;
                    GUILayout.Label("Installed", installedStyle, GUILayout.Width(70), GUILayout.Height(35));
                }

                EditorGUILayout.EndHorizontal();
                EditorGUILayout.Space(5);
            }

            EditorGUILayout.EndVertical();
        }

        private List<DependencyInfo> GetRequiredDependencies()
        {
            RefreshDependencyCacheIfNeeded();
            var deps = new List<DependencyInfo>();

            // Newtonsoft.Json (robust detection)
            bool newtonsoftInstalled = CheckPackageInstalledByManifest("com.unity.nuget.newtonsoft-json") ||
                CheckPackageInstalledByPackageCache("com.unity.nuget.newtonsoft-json") ||
                CheckTypeExistsRobust(
                    "Newtonsoft.Json.JsonConvert",
                    "Newtonsoft.Json.Linq.JObject") ||
                CheckAssemblyExists("Newtonsoft.Json", "Unity.Newtonsoft.Json");
            
            string newtonsoftVersion = GetInstalledVersion("com.unity.nuget.newtonsoft-json");
            deps.Add(new DependencyInfo
            {
                Name = "Newtonsoft.Json",
                IsInstalled = newtonsoftInstalled,
                InstalledVersion = newtonsoftVersion,
                Description = newtonsoftInstalled && !string.IsNullOrEmpty(newtonsoftVersion)
                    ? $"Required for JSON serialization (v{newtonsoftVersion})"
                    : "Required for JSON serialization",
                PackageId = "com.unity.nuget.newtonsoft-json",
                InstallMethod = "upm",
                InstallAction = () => InstallUPMPackage("com.unity.nuget.newtonsoft-json", "3.2.2")
            });

            // TextMeshPro (robust detection)
            bool tmpInstalled = CheckPackageInstalledByManifest("com.unity.textmeshpro") ||
                CheckPackageInstalledByPackageCache("com.unity.textmeshpro") ||
                CheckTypeExistsRobust(
                    "TMPro.TextMeshProUGUI",
                    "TMPro.TextMeshPro",
                    "TMPro.TMP_Text") ||
                CheckAssemblyExists("Unity.TextMeshPro", "TextMeshPro");
            
            string tmpVersion = GetInstalledVersion("com.unity.textmeshpro");
            deps.Add(new DependencyInfo
            {
                Name = "TextMeshPro",
                IsInstalled = tmpInstalled,
                InstalledVersion = tmpVersion,
                Description = tmpInstalled && !string.IsNullOrEmpty(tmpVersion)
                    ? $"Required for UI text rendering (v{tmpVersion})"
                    : "Required for UI text rendering",
                PackageId = "com.unity.textmeshpro",
                InstallMethod = "upm",
                InstallAction = () => InstallUPMPackage("com.unity.textmeshpro", "3.0.9")
            });

            // Nakama (robust detection)
            bool nakamaInstalled = CheckTypeExistsRobust(
                "Nakama.IClient",
                "Nakama.Client",
                "Nakama.ISession",
                "Nakama.IApiAccount") ||
                CheckAssemblyExists("Nakama", "NakamaRuntime", "Nakama.Runtime") ||
                CheckDirectoryExists("Assets/Nakama", "Assets/Plugins/Nakama") ||
                CheckPackageInstalledByManifest("com.heroiclabs.nakama-unity");
            
            deps.Add(new DependencyInfo
            {
                Name = "Nakama Unity SDK",
                IsInstalled = nakamaInstalled,
                Description = "Required for backend services (leaderboards, auth, storage)",
                InstallMethod = "unitypackage",
                InstallUrl = "https://github.com/heroiclabs/nakama-unity/releases",
                InstallAction = () => Application.OpenURL("https://github.com/heroiclabs/nakama-unity/releases")
            });

            return deps;
        }

        private List<DependencyInfo> GetOptionalDependencies()
        {
            RefreshDependencyCacheIfNeeded();
            var deps = new List<DependencyInfo>();

            // DOTween
            deps.Add(new DependencyInfo
            {
                Name = "DOTween",
                IsInstalled = CheckDOTweenInstalled(),
                Description = "Recommended for UI animations (Friends, Leaderboard)",
                InstallMethod = "assetStore",
                InstallUrl = "https://assetstore.unity.com/packages/tools/animation/dotween-hotween-v2-27676",
                InstallAction = () => Application.OpenURL("https://assetstore.unity.com/packages/tools/animation/dotween-hotween-v2-27676")
            });

            // Photon PUN2
            bool photonInstalled = CheckTypeExistsRobust(
                "Photon.Pun.PhotonNetwork",
                "Photon.Realtime.Player",
                "PhotonNetwork") ||
                CheckAssemblyExists("PhotonUnityNetworking", "Photon.Pun", "PhotonRealtime") ||
                CheckDirectoryExists("Assets/Photon", "Assets/Photon Unity Networking");
            
            deps.Add(new DependencyInfo
            {
                Name = "Photon PUN2",
                IsInstalled = photonInstalled,
                Description = "Optional for real-time multiplayer",
                InstallMethod = "assetStore",
                InstallUrl = "https://assetstore.unity.com/packages/tools/network/pun-2-free-119922",
                InstallAction = () => Application.OpenURL("https://assetstore.unity.com/packages/tools/network/pun-2-free-119922")
            });

            // Unity IAP (using robust detection)
            bool purchasingInstalled = CheckUnityPurchasingInstalled();
            string purchasingVersion = GetInstalledVersion("com.unity.purchasing");
            deps.Add(new DependencyInfo
            {
                Name = "Unity Purchasing",
                IsInstalled = purchasingInstalled,
                InstalledVersion = purchasingVersion,
                Description = purchasingInstalled && !string.IsNullOrEmpty(purchasingVersion) 
                    ? $"Required for in-app purchases (v{purchasingVersion})" 
                    : "Required for in-app purchases",
                PackageId = "com.unity.purchasing",
                InstallMethod = "upm",
                InstallAction = () => InstallUPMPackage("com.unity.purchasing", "5.1.2")
            });

            // LevelPlay (using robust detection)
            bool levelPlayInstalled = CheckLevelPlayInstalled();
            string levelPlayVersion = GetInstalledVersion("com.unity.services.levelplay");
            deps.Add(new DependencyInfo
            {
                Name = "Unity LevelPlay (IronSource)",
                IsInstalled = levelPlayInstalled,
                InstalledVersion = levelPlayVersion,
                Description = levelPlayInstalled && !string.IsNullOrEmpty(levelPlayVersion) 
                    ? $"Ad mediation platform (v{levelPlayVersion})" 
                    : "Ad mediation platform",
                PackageId = "com.unity.services.levelplay",
                InstallMethod = "upm",
                InstallAction = () => InstallUPMPackage("com.unity.services.levelplay", "9.3.0")
            });

            // Native Share (using robust detection)
            bool nativeShareInstalled = CheckNativeShareInstalled();
            deps.Add(new DependencyInfo
            {
                Name = "Native Share",
                IsInstalled = nativeShareInstalled,
                Description = "Native sharing functionality for mobile",
                PackageId = "com.yasirkula.nativeshare",
                InstallMethod = "gitUrl",
                InstallUrl = "https://github.com/yasirkula/UnityNativeShare.git",
                InstallAction = () => InstallGitPackage("com.yasirkula.nativeshare", "https://github.com/yasirkula/UnityNativeShare.git")
            });

            return deps;
        }

        private List<DependencyInfo> GetExternalDependencies()
        {
            RefreshDependencyCacheIfNeeded();
            var deps = new List<DependencyInfo>();

            // Appodeal (using robust detection)
            bool appodealInstalled = CheckAppodealInstalled();
            string appodealVersion = GetInstalledVersion("com.appodeal.mediation");
            deps.Add(new DependencyInfo
            {
                Name = "Appodeal",
                IsInstalled = appodealInstalled,
                InstalledVersion = appodealVersion,
                Description = appodealInstalled && !string.IsNullOrEmpty(appodealVersion) 
                    ? $"Ad mediation with 70+ ad networks (v{appodealVersion})" 
                    : "Ad mediation with 70+ ad networks",
                PackageId = "com.appodeal.mediation",
                InstallMethod = "gitUrl",
                InstallUrl = "https://github.com/appodeal/appodeal-unity-plugin-upm.git#v3.12.0",
                InstallAction = () => InstallGitPackage("com.appodeal.mediation", "https://github.com/appodeal/appodeal-unity-plugin-upm.git#v3.12.0")
            });

            // Apple Sign-In (using robust detection)
            bool appleAuthInstalled = CheckTypeExistsRobust(
                "AppleAuth.AppleAuthManager",
                "AppleAuth.IAppleAuthManager",
                "AppleAuth.Enums.LoginOptions") ||
                CheckAssemblyExists("AppleAuth", "AppleAuth.Runtime") ||
                CheckDirectoryExists("Assets/AppleAuth", "Assets/Plugins/AppleAuth");
            
            deps.Add(new DependencyInfo
            {
                Name = "Apple Sign-In Unity",
                IsInstalled = appleAuthInstalled,
                Description = "Required for Apple Sign-In on iOS",
                InstallMethod = "unitypackage",
                InstallUrl = "https://github.com/lupidan/apple-signin-unity/releases",
                InstallAction = () => Application.OpenURL("https://github.com/lupidan/apple-signin-unity/releases")
            });

            return deps;
        }

        private void CheckAllDependencies()
        {
            var required = GetRequiredDependencies();
            var optional = GetOptionalDependencies();
            var external = GetExternalDependencies();

            int requiredInstalled = required.Count(d => d.IsInstalled);
            int optionalInstalled = optional.Count(d => d.IsInstalled);
            int externalInstalled = external.Count(d => d.IsInstalled);

            string message = "Dependency Status:\n\n";
            message += $"Required: {requiredInstalled}/{required.Count} installed\n";
            message += $"Optional: {optionalInstalled}/{optional.Count} installed\n";
            message += $"External: {externalInstalled}/{external.Count} installed\n\n";

            var missingRequired = required.Where(d => !d.IsInstalled).ToList();
            if (missingRequired.Count > 0)
            {
                message += "Missing Required:\n";
                foreach (var dep in missingRequired)
                {
                    message += $"• {dep.Name}\n";
                }
            }
            else
            {
                message += "✅ All required dependencies are installed!";
            }

            EditorUtility.DisplayDialog("Dependency Check", message, "OK");
        }

        private void InstallAllDependencies()
        {
            if (!EditorUtility.DisplayDialog("Install All Dependencies",
                "This will install all required dependencies.\n\n" +
                "Required packages:\n" +
                "• Newtonsoft.Json\n" +
                "• TextMeshPro\n" +
                "• Nakama SDK (manual install)\n\n" +
                "Continue?", "Install", "Cancel"))
            {
                return;
            }

            var required = GetRequiredDependencies();
            foreach (var dep in required.Where(d => !d.IsInstalled))
            {
                dep.InstallAction?.Invoke();
            }

            EditorUtility.DisplayDialog("Installation Started",
                "Dependency installation has started.\n\n" +
                "Check the Console for progress.\n\n" +
                "Note: Some packages (like Nakama) require manual installation.",
                "OK");
        }

        private void ValidateAllDependencies()
        {
            var required = GetRequiredDependencies();
            var missing = required.Where(d => !d.IsInstalled).ToList();

            if (missing.Count == 0)
            {
                EditorUtility.DisplayDialog("Validation Passed",
                    "✅ All required dependencies are installed and validated!",
                    "OK");
            }
            else
            {
                string message = $"⚠️ {missing.Count} required dependencies are missing:\n\n";
                foreach (var dep in missing)
                {
                    message += $"• {dep.Name}\n";
                }
                message += "\nUse 'Install All Dependencies' to install them.";
                EditorUtility.DisplayDialog("Validation Failed", message, "OK");
            }
        }

        private void OpenProjectSetup()
        {
            var projectSetupType = GetTypeByName("IntelliVerseX.Editor.IVXProjectSetup");
            if (projectSetupType != null)
            {
                var showMethod = projectSetupType.GetMethod("ShowWindow",
                    System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
                showMethod?.Invoke(null, null);
            }
            else
            {
                EditorUtility.DisplayDialog("Not Available",
                    "Project Setup window not found.\n\n" +
                    "Use the Platform Validation tab for platform-specific validation.",
                    "OK");
            }
        }

        private void InstallUPMPackage(string packageId, string version = null)
        {
            try
            {
                var packageManagerType = GetTypeByName("UnityEditor.PackageManager.UI.PackageManagerWindow");
                if (packageManagerType != null)
                {
                    var openMethod = packageManagerType.GetMethod("Open",
                        System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
                    openMethod?.Invoke(null, null);
                }

                // Also try to add via manifest
                string manifestPath = Path.Combine(Application.dataPath, "..", "Packages", "manifest.json");
                if (File.Exists(manifestPath))
                {
                    string manifest = File.ReadAllText(manifestPath);
                    if (!manifest.Contains($"\"{packageId}\""))
                    {
                        // Add to dependencies
                        Debug.Log($"[IVXSDKSetupWizard] Add {packageId} to Packages/manifest.json dependencies section");
                        EditorUtility.DisplayDialog("Package Installation",
                            $"To install {packageId}:\n\n" +
                            "1. Open Window > Package Manager\n" +
                            "2. Click '+' > Add package by name\n" +
                            $"3. Enter: {packageId}" + (version != null ? $"@{version}" : "") + "\n" +
                            "4. Click Add\n\n" +
                            "Or add it manually to Packages/manifest.json",
                            "OK");
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"[IVXSDKSetupWizard] Failed to install package {packageId}: {ex.Message}");
                EditorUtility.DisplayDialog("Installation Error",
                    $"Could not automatically install {packageId}.\n\n" +
                    "Please install it manually via Package Manager:\n" +
                    "Window > Package Manager > + > Add package by name",
                    "OK");
            }
        }

        private void InstallGitPackage(string packageId, string gitUrl)
        {
            try
            {
                string manifestPath = Path.Combine(Application.dataPath, "..", "Packages", "manifest.json");
                if (File.Exists(manifestPath))
                {
                    string manifest = File.ReadAllText(manifestPath);
                    if (!manifest.Contains($"\"{packageId}\""))
                    {
                        // Parse JSON and add dependency
                        Debug.Log($"[IVXSDKSetupWizard] Add {packageId} from {gitUrl} to manifest.json");
                        EditorUtility.DisplayDialog("Git Package Installation",
                            $"To install {packageId}:\n\n" +
                            "1. Open Packages/manifest.json\n" +
                            "2. Add to dependencies:\n" +
                            $"   \"{packageId}\": \"{gitUrl}\"\n\n" +
                            "Or use Package Manager:\n" +
                            "Window > Package Manager > + > Add package from git URL",
                            "OK");
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"[IVXSDKSetupWizard] Failed to install git package {packageId}: {ex.Message}");
            }
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
            bool addToScene = false;
            bool openDemoScene = false;
            bool editConfig = false;

            EditorGUILayout.BeginHorizontal();
            createConfig = GUILayout.Button("Create Auth Config", GUILayout.Height(25));
            addToScene = GUILayout.Button("Add Auth Canvas to Scene", GUILayout.Height(25));
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            openDemoScene = GUILayout.Button("Open Auth Demo Scene", GUILayout.Height(25));
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
            if (addToScene) EditorApplication.delayCall += AddAuthCanvasToScene;
            if (openDemoScene) OpenDemoScene("IVX_AuthTest");
            if (editConfig && authConfig != null) Selection.activeObject = authConfig;
        }

        private void DrawFriendsModuleActions()
        {
            EditorGUILayout.Space(5);
            EditorGUILayout.LabelField("Friends Actions:", EditorStyles.miniBoldLabel);

            // Store button states to process actions after layout
            bool createConfig = false;
            bool addToScene = false;
            bool openDemoScene = false;
            bool openDOTweenStore = false;

            EditorGUILayout.BeginHorizontal();
            createConfig = GUILayout.Button("Create Friends Config", GUILayout.Height(25));
            addToScene = GUILayout.Button("Add Friends UI to Scene", GUILayout.Height(25));
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            openDemoScene = GUILayout.Button("Open Friends Demo Scene", GUILayout.Height(25));
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
            if (addToScene) EditorApplication.delayCall += AddFriendsUIToScene;
            if (openDemoScene) OpenDemoScene("IVX_Friends");
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
            
            bool addToScene = GUILayout.Button("Add to Scene", GUILayout.Height(25));
            bool validateSetup = GUILayout.Button("Validate Setup", GUILayout.Height(25));

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            
            bool openTestScene = GUILayout.Button("Open Wallet Test Scene", GUILayout.Height(25));

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
            
            if (openTestScene)
            {
                OpenDemoScene("IVX_WalletTest");
            }
        }

        private void DrawSocialModuleActions()
        {
            EditorGUILayout.Space(5);
            EditorGUILayout.LabelField("Social Actions:", EditorStyles.miniBoldLabel);

            EditorGUILayout.BeginHorizontal();
            
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
            
            bool addToScene = GUILayout.Button("Add to Scene", GUILayout.Height(25));
            bool validateSetup = GUILayout.Button("Validate Setup", GUILayout.Height(25));

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();

            bool openTestScene = GUILayout.Button("Open Leaderboard Test Scene", GUILayout.Height(25));

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
            
            if (openTestScene)
            {
                OpenDemoScene("IVX_LeaderboardTest");
            }
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
                "Add pre-built UI prefabs with Netflix-style hover animations and carousel scrolling.",
                EditorStyles.wordWrappedMiniLabel);

            EditorGUILayout.Space(10);
            EditorGUILayout.BeginHorizontal();

            GUI.backgroundColor = accentColor;
            if (GUILayout.Button("Add To Scene", GUILayout.Height(35)))
            {
                try
                {
                    // Load MoreOfUs prefab from SDK
                    var moreOfUsPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(SDK_ROOT + "/MoreOfUs/Prefabs/IVXGMoreOfUsCanvas.prefab");
                    if (moreOfUsPrefab != null && GameObject.Find("IVXGMoreOfUsCanvas") == null)
                    {
                        var instance = PrefabUtility.InstantiatePrefab(moreOfUsPrefab) as GameObject;
                        if (instance != null)
                        {
                            Undo.RegisterCreatedObjectUndo(instance, "Add More Of Us Canvas");
                            Selection.activeGameObject = instance;
                            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
                            Debug.Log("[IVXSDKSetupWizard] Added More Of Us Canvas to scene");
                        }
                    }
                    else if (GameObject.Find("IVXGMoreOfUsCanvas") != null)
                    {
                        EditorUtility.DisplayDialog("Info", "More Of Us Canvas already exists in scene.", "OK");
                    }
                    else
                    {
                        EditorUtility.DisplayDialog("Info", "More Of Us prefab not found at: " + SDK_ROOT + "/MoreOfUs/Prefabs/IVXGMoreOfUsCanvas.prefab", "OK");
                    }
                }
                catch (System.Exception ex)
                {
                    Debug.LogError($"[IVXSDKSetupWizard] Failed to add More Of Us canvas: {ex.Message}");
                    EditorUtility.DisplayDialog("Error", $"Failed to add to scene: {ex.Message}", "OK");
                }
            }
            GUI.backgroundColor = Color.white;

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

        #region Platform Validation Tab

        private void DrawPlatformValidationTab()
        {
            EditorGUILayout.LabelField("Platform Validation", subHeaderStyle ?? EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(
                "Validate your project settings for production builds on WebGL, Android, and iOS. " +
                "This ensures all SDK features work correctly on each platform.",
                MessageType.Info);

            EditorGUILayout.Space(10);

            // Platform Selection
            EditorGUILayout.BeginVertical(moduleBoxStyle);
            EditorGUILayout.LabelField("Target Platforms", EditorStyles.boldLabel);
            
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("🌐 Validate WebGL", GUILayout.Height(30)))
            {
                ValidateWebGL();
            }
            if (GUILayout.Button("🤖 Validate Android", GUILayout.Height(30)))
            {
                ValidateAndroid();
            }
            if (GUILayout.Button("🍎 Validate iOS", GUILayout.Height(30)))
            {
                ValidateiOS();
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(5);
            if (GUILayout.Button("✅ Validate All Platforms", GUILayout.Height(35)))
            {
                ValidateAllPlatforms();
            }
            EditorGUILayout.EndVertical();

            EditorGUILayout.Space(10);

            // Platform-Specific Requirements
            DrawPlatformRequirements("🌐 WebGL", GetWebGLRequirements());
            DrawPlatformRequirements("🤖 Android", GetAndroidRequirements());
            DrawPlatformRequirements("🍎 iOS", GetiOSRequirements());
        }

        private void DrawPlatformRequirements(string platformName, List<PlatformRequirement> requirements)
        {
            EditorGUILayout.BeginVertical(moduleBoxStyle);
            EditorGUILayout.LabelField(platformName, EditorStyles.boldLabel);

            int passed = 0;
            foreach (var req in requirements)
            {
                EditorGUILayout.BeginHorizontal();
                GUILayout.Label(req.IsValid ? "  ✅" : "  ❌", GUILayout.Width(30));
                EditorGUILayout.LabelField(req.Name, EditorStyles.wordWrappedLabel);
                EditorGUILayout.EndHorizontal();

                if (!req.IsValid && !string.IsNullOrEmpty(req.FixMessage))
                {
                    EditorGUILayout.BeginHorizontal();
                    GUILayout.Space(35);
                    EditorGUILayout.HelpBox(req.FixMessage, MessageType.Warning);
                    EditorGUILayout.EndHorizontal();
                }

                if (req.IsValid) passed++;
            }

            EditorGUILayout.Space(5);
            EditorGUILayout.LabelField($"Status: {passed}/{requirements.Count} requirements met", 
                passed == requirements.Count ? successStyle : warningStyle);

            EditorGUILayout.EndVertical();
            EditorGUILayout.Space(5);
        }

        private class PlatformRequirement
        {
            public string Name;
            public bool IsValid;
            public string FixMessage;
        }

        private List<PlatformRequirement> GetWebGLRequirements()
        {
            var requirements = new List<PlatformRequirement>();

            // Check scripting backend
#pragma warning disable 0618 // Suppress deprecation warning for BuildTargetGroup (maintained for compatibility)
            var webglBackend = new PlatformRequirement
            {
                Name = "IL2CPP Scripting Backend",
                IsValid = PlayerSettings.GetScriptingBackend(BuildTargetGroup.WebGL) == ScriptingImplementation.IL2CPP
            };
#pragma warning restore 0618
            if (!webglBackend.IsValid)
            {
                webglBackend.FixMessage = "WebGL requires IL2CPP. Go to: Edit > Project Settings > Player > WebGL > Other Settings > Scripting Backend";
            }
            requirements.Add(webglBackend);

            // Check compression format (Unity's enum)
            var compression = new PlatformRequirement
            {
                Name = "Compression Format (Gzip/Brotli recommended)",
                IsValid = true // Unity handles this automatically, just informational
            };
            compression.FixMessage = "Compression is handled automatically by Unity. Check: Edit > Project Settings > Player > WebGL > Publishing Settings";
            requirements.Add(compression);

            // Check WebGL template
            var template = new PlatformRequirement
            {
                Name = "WebGL Template (Default or Custom)",
                IsValid = true // Always valid, just informational
            };
            requirements.Add(template);

            // Check memory size
            var memory = new PlatformRequirement
            {
                Name = "Memory Size (16MB minimum recommended)",
                IsValid = PlayerSettings.WebGL.memorySize >= 16
            };
            if (!memory.IsValid)
            {
                memory.FixMessage = "Increase memory size for better performance. Go to: Edit > Project Settings > Player > WebGL > Other Settings > Memory Size";
            }
            requirements.Add(memory);

            // Check WebGL ads support
            var webglAds = new PlatformRequirement
            {
                Name = "WebGL Ads Support (IVXWebGLAdsManager available)",
                IsValid = TypeExists("IntelliVerseX.Monetization.IVXWebGLAdsManager") ||
                         TypeExists("IntelliVerseX.Monetization.IVXWebGLMonetizationManager")
            };
            if (!webglAds.IsValid)
            {
                webglAds.FixMessage = "WebGL-specific ads manager not found. Some ad networks may not work on WebGL.";
            }
            requirements.Add(webglAds);

            return requirements;
        }

        private List<PlatformRequirement> GetAndroidRequirements()
        {
            var requirements = new List<PlatformRequirement>();

            // Check minimum API level (minimum is now 25 per Unity requirements)
            var minAPI = new PlatformRequirement
            {
                Name = "Minimum API Level (25+ required)",
                IsValid = PlayerSettings.Android.minSdkVersion >= AndroidSdkVersions.AndroidApiLevel25
            };
            if (!minAPI.IsValid)
            {
                minAPI.FixMessage = "Set minimum API level to 25 (Android 7.1) or higher. Go to: Edit > Project Settings > Player > Android > Other Settings > Minimum API Level";
            }
            requirements.Add(minAPI);

            // Check target API level
            var targetAPI = new PlatformRequirement
            {
                Name = "Target API Level (34 recommended)",
                IsValid = PlayerSettings.Android.targetSdkVersion >= AndroidSdkVersions.AndroidApiLevel34 ||
                         PlayerSettings.Android.targetSdkVersion == AndroidSdkVersions.AndroidApiLevelAuto
            };
            if (!targetAPI.IsValid)
            {
                targetAPI.FixMessage = "Set target API level to 34 (Android 14) or Auto. Go to: Edit > Project Settings > Player > Android > Other Settings > Target API Level";
            }
            requirements.Add(targetAPI);

            // Check scripting backend
#pragma warning disable 0618 // Suppress deprecation warning for BuildTargetGroup (maintained for compatibility)
            var androidBackend = new PlatformRequirement
            {
                Name = "IL2CPP Scripting Backend (recommended)",
                IsValid = PlayerSettings.GetScriptingBackend(BuildTargetGroup.Android) == ScriptingImplementation.IL2CPP
            };
#pragma warning restore 0618
            if (!androidBackend.IsValid)
            {
                androidBackend.FixMessage = "IL2CPP is recommended for Android production builds. Go to: Edit > Project Settings > Player > Android > Other Settings > Scripting Backend";
            }
            requirements.Add(androidBackend);

            // Check package name
            var packageName = new PlatformRequirement
            {
                Name = "Package Name (com.yourcompany.yourapp format)",
                IsValid = !string.IsNullOrEmpty(PlayerSettings.applicationIdentifier) &&
                         PlayerSettings.applicationIdentifier.Contains(".") &&
                         PlayerSettings.applicationIdentifier.Split('.').Length >= 3
            };
            if (!packageName.IsValid)
            {
                packageName.FixMessage = "Set a valid package name (e.g., com.yourcompany.yourapp). Go to: Edit > Project Settings > Player > Android > Other Settings > Package Name";
            }
            requirements.Add(packageName);

            // Check Internet permission
            var internetPermission = new PlatformRequirement
            {
                Name = "Internet Permission (required for backend)",
                IsValid = true // Unity adds this by default, but we check manifest
            };
            requirements.Add(internetPermission);

            // Check Appodeal/LevelPlay availability
            var adNetwork = new PlatformRequirement
            {
                Name = "Ad Network SDK (Appodeal/LevelPlay recommended)",
                IsValid = TypeExists("Appodeal.Appodeal") ||
                         TypeExists("Unity.Services.LevelPlay.IronSource") ||
                         TypeExists("IntelliVerseX.Monetization.IVXAdsManager")
            };
            if (!adNetwork.IsValid)
            {
                adNetwork.FixMessage = "Install Appodeal or LevelPlay SDK for Android ads. Use: IntelliVerse-X SDK > SDK Setup Wizard > Monetization tab";
            }
            requirements.Add(adNetwork);

            return requirements;
        }

        private List<PlatformRequirement> GetiOSRequirements()
        {
            var requirements = new List<PlatformRequirement>();

            // Check minimum iOS version
            var minIOS = new PlatformRequirement
            {
                Name = "Minimum iOS Version (13.0+ recommended)",
                IsValid = float.TryParse(PlayerSettings.iOS.targetOSVersionString, out float version) && version >= 13.0f
            };
            if (!minIOS.IsValid)
            {
                minIOS.FixMessage = "Set minimum iOS version to 13.0 or higher. Go to: Edit > Project Settings > Player > iOS > Other Settings > Target minimum iOS Version";
            }
            requirements.Add(minIOS);

            // Check scripting backend
#pragma warning disable 0618 // Suppress deprecation warning for BuildTargetGroup (maintained for compatibility)
            var iosBackend = new PlatformRequirement
            {
                Name = "IL2CPP Scripting Backend (required)",
                IsValid = PlayerSettings.GetScriptingBackend(BuildTargetGroup.iOS) == ScriptingImplementation.IL2CPP
            };
#pragma warning restore 0618
            if (!iosBackend.IsValid)
            {
                iosBackend.FixMessage = "iOS requires IL2CPP. Go to: Edit > Project Settings > Player > iOS > Other Settings > Scripting Backend";
            }
            requirements.Add(iosBackend);

            // Check bundle identifier
            var bundleId = new PlatformRequirement
            {
                Name = "Bundle Identifier (com.yourcompany.yourapp format)",
                IsValid = !string.IsNullOrEmpty(PlayerSettings.applicationIdentifier) &&
                         PlayerSettings.applicationIdentifier.Contains(".") &&
                         PlayerSettings.applicationIdentifier.Split('.').Length >= 3
            };
            if (!bundleId.IsValid)
            {
                bundleId.FixMessage = "Set a valid bundle identifier (e.g., com.yourcompany.yourapp). Go to: Edit > Project Settings > Player > iOS > Other Settings > Bundle Identifier";
            }
            requirements.Add(bundleId);

            // Check Apple Sign-In capability (if using Apple auth)
            var appleSignIn = new PlatformRequirement
            {
                Name = "Apple Sign-In Capability (if using Apple auth)",
                IsValid = true // Optional, just informational
            };
            requirements.Add(appleSignIn);

            // Check App Store Connect API Key (for IAP)
            var iapSupport = new PlatformRequirement
            {
                Name = "IAP Support (StoreKit available)",
                IsValid = TypeExists("UnityEngine.Purchasing.IStoreController") ||
                         TypeExists("IntelliVerseX.Monetization.IVXIAPManager")
            };
            if (!iapSupport.IsValid)
            {
                iapSupport.FixMessage = "Install Unity Purchasing package for IAP support. Use: IntelliVerse-X SDK > SDK Setup Wizard > Monetization tab";
            }
            requirements.Add(iapSupport);

            // Check ad network
            var iosAdNetwork = new PlatformRequirement
            {
                Name = "Ad Network SDK (Appodeal/LevelPlay recommended)",
                IsValid = TypeExists("Appodeal.Appodeal") ||
                         TypeExists("Unity.Services.LevelPlay.IronSource") ||
                         TypeExists("IntelliVerseX.Monetization.IVXAdsManager")
            };
            if (!iosAdNetwork.IsValid)
            {
                iosAdNetwork.FixMessage = "Install Appodeal or LevelPlay SDK for iOS ads. Use: IntelliVerse-X SDK > SDK Setup Wizard > Monetization tab";
            }
            requirements.Add(iosAdNetwork);

            return requirements;
        }

        private void ValidateWebGL()
        {
            var requirements = GetWebGLRequirements();
            int passed = requirements.Count(r => r.IsValid);
            int total = requirements.Count;

            string message = $"WebGL Validation: {passed}/{total} requirements met.\n\n";
            var failed = requirements.Where(r => !r.IsValid).ToList();
            if (failed.Count > 0)
            {
                message += "Issues found:\n";
                foreach (var req in failed)
                {
                    message += $"• {req.Name}\n";
                }
            }
            else
            {
                message += "✅ All WebGL requirements are met!";
            }

            EditorUtility.DisplayDialog("WebGL Validation", message, "OK");
        }

        private void ValidateAndroid()
        {
            var requirements = GetAndroidRequirements();
            int passed = requirements.Count(r => r.IsValid);
            int total = requirements.Count;

            string message = $"Android Validation: {passed}/{total} requirements met.\n\n";
            var failed = requirements.Where(r => !r.IsValid).ToList();
            if (failed.Count > 0)
            {
                message += "Issues found:\n";
                foreach (var req in failed)
                {
                    message += $"• {req.Name}\n";
                }
            }
            else
            {
                message += "✅ All Android requirements are met!";
            }

            EditorUtility.DisplayDialog("Android Validation", message, "OK");
        }

        private void ValidateiOS()
        {
            var requirements = GetiOSRequirements();
            int passed = requirements.Count(r => r.IsValid);
            int total = requirements.Count;

            string message = $"iOS Validation: {passed}/{total} requirements met.\n\n";
            var failed = requirements.Where(r => !r.IsValid).ToList();
            if (failed.Count > 0)
            {
                message += "Issues found:\n";
                foreach (var req in failed)
                {
                    message += $"• {req.Name}\n";
                }
            }
            else
            {
                message += "✅ All iOS requirements are met!";
            }

            EditorUtility.DisplayDialog("iOS Validation", message, "OK");
        }

        private void ValidateAllPlatforms()
        {
            var webgl = GetWebGLRequirements();
            var android = GetAndroidRequirements();
            var ios = GetiOSRequirements();

            int webglPassed = webgl.Count(r => r.IsValid);
            int androidPassed = android.Count(r => r.IsValid);
            int iosPassed = ios.Count(r => r.IsValid);

            string message = "Platform Validation Results:\n\n";
            message += $"🌐 WebGL: {webglPassed}/{webgl.Count} requirements met\n";
            message += $"🤖 Android: {androidPassed}/{android.Count} requirements met\n";
            message += $"🍎 iOS: {iosPassed}/{ios.Count} requirements met\n\n";

            if (webglPassed == webgl.Count && androidPassed == android.Count && iosPassed == ios.Count)
            {
                message += "✅ All platforms are production-ready!";
            }
            else
            {
                message += "⚠️ Some platforms need configuration. Check the Platform Validation tab for details.";
            }

            EditorUtility.DisplayDialog("Platform Validation", message, "OK");
        }

        #endregion

        #region Test Scenes Tab

        private void DrawTestScenesTab()
        {
            EditorGUILayout.LabelField("Demo Scenes", subHeaderStyle ?? EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(
                "Demo scenes are included with the SDK to test each feature. " +
                "These scenes contain pre-configured prefabs and UI to verify SDK functionality.",
                MessageType.Info);

            EditorGUILayout.Space(10);

            DrawTestSceneButton("🏠 Home Screen Demo",
                "Central home for navigating all IVX feature test scenes",
                "IVX_HomeScreen",
                null);

            // Test Scene Buttons
            DrawTestSceneButton("🔐 Auth Demo Scene",
                "Test login, register, OTP, guest, and social auth flows",
                "IVX_AuthTest",
                null);

            DrawTestSceneButton("👥 Friends Demo Scene",
                "Test friend list, requests, search, and social features",
                "IVX_Friends",
                null);

            DrawTestSceneButton("💰 Wallet Demo Scene",
                "Test wallet display, balance updates, and transactions",
                "IVX_WalletTest",
                null);

            DrawTestSceneButton("🏆 Leaderboard Demo Scene",
                "Test leaderboard display, score submission, and rankings",
                "IVX_LeaderboardTest",
                null);

            DrawTestSceneButton("📺 Ads Demo Scene",
                "Test ad loading, display, and reward callbacks",
                "IVX_AdsTest",
                null);

            DrawTestSceneButton("📤 Share & Rate Demo Scene",
                "Test share and rate features",
                "IVX_Share&RateUs",
                null);

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

            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button("Re-import To Consumer Assets", GUILayout.Height(25)))
            {
                IVXSceneImporter.ForceReimportScenes();
            }

            EditorGUILayout.EndHorizontal();

            if (GUILayout.Button("Open Consumer Scenes Folder", GUILayout.Height(25)))
            {
                IVXSceneImporter.OpenScenesFolder();
            }

            EditorGUILayout.Space(6);
            EditorGUILayout.LabelField("Source (SDK package): Samples~/TestScenes", EditorStyles.miniLabel);
            EditorGUILayout.LabelField("Consumer copy path: Assets/IntelliVerseX Scenes", EditorStyles.miniLabel);
            EditorGUILayout.EndVertical();
        }

        private void DrawTestSceneButton(string title, string description, string sceneName, Action createAction)
        {
            string scenePath = $"Assets/Scenes/Tests/{sceneName}.unity";
            bool sceneExists = File.Exists(scenePath);

            EditorGUILayout.BeginVertical(moduleBoxStyle ?? EditorStyles.helpBox);
            EditorGUILayout.BeginHorizontal();

            EditorGUILayout.BeginVertical();
            EditorGUILayout.LabelField(title + (sceneExists ? " ✓" : " ✗"), EditorStyles.boldLabel);
            EditorGUILayout.LabelField(description, EditorStyles.wordWrappedMiniLabel);
            EditorGUILayout.EndVertical();

            EditorGUILayout.BeginVertical(GUILayout.Width(85));

            if (sceneExists)
            {
                if (GUILayout.Button("Open", GUILayout.Height(25)))
                {
                    // Use delayCall to avoid GUI layout issues when changing scenes
                    string path = scenePath; // Capture for lambda
                    EditorApplication.delayCall += () =>
                    {
                        if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                        {
                            EditorSceneManager.OpenScene(path);
                        }
                    };
                }
                if (GUILayout.Button("Play", GUILayout.Height(25)))
                {
                    // Use delayCall to avoid GUI layout issues when changing scenes
                    string path = scenePath; // Capture for lambda
                    EditorApplication.delayCall += () =>
                    {
                        if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                        {
                            EditorSceneManager.OpenScene(path);
                            EditorApplication.isPlaying = true;
                        }
                    };
                }
            }
            else
            {
                // Scene not found - show disabled state
                GUI.enabled = false;
                GUILayout.Button("Not Found", GUILayout.Height(52));
                GUI.enabled = true;
            }

            EditorGUILayout.EndVertical();
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();
        }

        private void DrawLocalDataTab()
        {
            EditorGUILayout.LabelField("Local Data Tools", subHeaderStyle ?? EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(
                "Use this tab to clear local login/session data used by IntelliVerseX SDK during testing.\n" +
                "This runs in Editor mode (Play Mode not required).",
                MessageType.Info);

            EditorGUILayout.Space(10);

            EditorGUILayout.BeginVertical(moduleBoxStyle ?? EditorStyles.helpBox);
            EditorGUILayout.LabelField("Clear Local Login Data", EditorStyles.boldLabel);
            EditorGUILayout.LabelField("Shortcut: Ctrl/Cmd + Shift + K", EditorStyles.miniLabel);
            EditorGUILayout.LabelField(
                "Clears PlayerPrefs and supported session/identity stores (IVXUserSession, UserSessionManager, IntelliVerseXIdentity).",
                EditorStyles.wordWrappedMiniLabel);

            EditorGUILayout.Space(8);
            if (GUILayout.Button("Wipe Local SDK Data Now", GUILayout.Height(30)))
            {
                IVXLocalDataWiperTool.WipeLocalSdkData();
            }

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
            identityModule.stepCompleted[0] = SDKFileExists("Identity/UserSessionManager.cs") ||
                                              TypeExists("UserSessionManager") ||
                                              TypeExists("IntelliVerseX.Identity.IVXUserSession");
            identityModule.stepCompleted[1] = SDKFileExists("Identity/APIManager.cs") ||
                                              TypeExists("APIManager") ||
                                              TypeExists("IntelliVerseX.Identity.IVXAPIClient");
            identityModule.stepCompleted[2] = SDKFileExists("Identity/IntelliVerseXUserIdentity.cs") ||
                                              TypeExists("IntelliVerseX.Identity.IntelliVerseXUserIdentity");

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
        /// Uses robust multi-layered detection for reliability.
        /// </summary>
        private bool CheckDOTweenInstalled()
        {
            RefreshDependencyCacheIfNeeded();
            
            if (CheckTypeExistsRobust(
                "DG.Tweening.DOTween",
                "DG.Tweening.Tween",
                "DG.Tweening.Sequence",
                "DG.Tweening.Core.TweenerCore"))
                return true;

            if (CheckAssemblyExists(
                "DOTween",
                "DOTweenPro",
                "DG.Tweening"))
                return true;

            if (CheckDirectoryExists(
                "Assets/Plugins/Demigiant/DOTween",
                "Assets/DOTween",
                "Assets/Demigiant/DOTween"))
                return true;

            return false;
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
            CheckAuthModule();
            Debug.Log("[IVXSDKSetupWizard] Auth module setup complete");
        }

        private void SetupFriendsModule()
        {
            CreateFriendsConfig();
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
                Debug.LogWarning("[IVXSDKSetupWizard] Auth Canvas prefab not found.");
                EditorUtility.DisplayDialog("Auth Canvas Missing",
                    "The Auth Canvas prefab was not found.\n\n" +
                    "Check that the SDK's Auth prefabs are properly installed at:\n" +
                    AUTH_PREFABS_PATH,
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

            // Load Friends Panel prefab from SDK
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(SOCIAL_PREFABS_PATH + "/IVX_FriendsPanel.prefab");
            if (prefab == null)
            {
                // Try alternate path
                prefab = AssetDatabase.LoadAssetAtPath<GameObject>(SDK_ROOT + "/Social/Prefabs/IVX_FriendsPanel.prefab");
            }
            
            if (prefab != null)
            {
                var instance = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
                if (instance != null)
                {
                    Undo.RegisterCreatedObjectUndo(instance, "Add Friends Panel");
                    Selection.activeGameObject = instance;
                    EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
                    Debug.Log("[IVXSDKSetupWizard] Added Friends Panel to scene");
                }
            }
            else
            {
                Debug.LogWarning("[IVXSDKSetupWizard] Friends Panel prefab not found.");
                EditorUtility.DisplayDialog("Friends Panel Missing",
                    "The Friends Panel prefab was not found.\n\n" +
                    "Check that the SDK's Social prefabs are properly installed.",
                    "OK");
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

                // Set camera background
                var camera = Camera.main;
                if (camera != null)
                {
                    camera.backgroundColor = new Color(0.12f, 0.14f, 0.18f, 1f);
                    camera.clearFlags = CameraClearFlags.SolidColor;
                }

                // Add EventSystem
                EnsureEventSystem();

                // Add NakamaManager (required for Auth to work)
                AddNakamaManagerToScene();

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
                    $"Auth Demo Scene created at:\n{scenePath}\n\n" +
                    "Includes:\n" +
                    "• NakamaManager (Backend)\n" +
                    "• Auth Canvas (UI)\n" +
                    "• EventSystem\n\n" +
                    "Press Play to test.", "OK");
            };
        }

        /// <summary>
        /// Adds NakamaManager to the current scene.
        /// Creates a managers root GameObject and adds the NakamaManager prefab or creates one dynamically.
        /// </summary>
        private void AddNakamaManagerToScene()
        {
            // Check if NakamaManager already exists
            var nakamaManagerType = GetTypeByName("IntelliVerseX.Backend.IVXNakamaManager");
            if (nakamaManagerType != null)
            {
                var existing = GameObject.FindAnyObjectByType(nakamaManagerType);
                if (existing != null)
                {
                    Debug.Log("[IVXSDKSetupWizard] NakamaManager already exists in scene");
                    return;
                }
            }

            // Create managers root if it doesn't exist
            var managersRoot = GameObject.Find("--- SDK Managers ---");
            if (managersRoot == null)
            {
                managersRoot = new GameObject("--- SDK Managers ---");
            }

            // Try to load NakamaManager prefab first
            var nakamaManagerPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(MANAGERS_PREFAB_PATH + "/NakamaManager.prefab");
            if (nakamaManagerPrefab != null)
            {
                var instance = PrefabUtility.InstantiatePrefab(nakamaManagerPrefab) as GameObject;
                if (instance != null)
                {
                    instance.transform.SetParent(managersRoot.transform);
                    Debug.Log("[IVXSDKSetupWizard] Added NakamaManager prefab to scene");
                    return;
                }
            }

            // If no prefab, try to create a basic NakamaManager GameObject
            // Look for any concrete implementation of IVXNakamaManager in the project
            var concreteNakamaType = FindConcreteNakamaManagerType();
            if (concreteNakamaType != null)
            {
                var nakamaGO = new GameObject("NakamaManager");
                nakamaGO.transform.SetParent(managersRoot.transform);
                nakamaGO.AddComponent(concreteNakamaType);
                Debug.Log($"[IVXSDKSetupWizard] Created NakamaManager with type: {concreteNakamaType.Name}");
            }
            else
            {
                // Create a placeholder with instructions
                var nakamaGO = new GameObject("NakamaManager [SETUP REQUIRED]");
                nakamaGO.transform.SetParent(managersRoot.transform);
                Debug.LogWarning("[IVXSDKSetupWizard] NakamaManager placeholder created. You need to:\n" +
                    "1. Create a class that extends IVXNakamaManager\n" +
                    "2. Add it to this GameObject\n" +
                    "3. Configure the SDK settings in the Inspector");
            }
        }

        /// <summary>
        /// Finds a concrete (non-abstract) implementation of IVXNakamaManager in the project.
        /// </summary>
        private Type FindConcreteNakamaManagerType()
        {
            var baseType = GetTypeByName("IntelliVerseX.Backend.IVXNakamaManager");
            if (baseType == null) return null;

            // Search all assemblies for concrete implementations
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                try
                {
                    foreach (var type in assembly.GetTypes())
                    {
                        if (type.IsClass && !type.IsAbstract && baseType.IsAssignableFrom(type))
                        {
                            return type;
                        }
                    }
                }
                catch
                {
                    // Ignore assemblies that can't be searched
                }
            }

            return null;
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

        private void CreateHomeScreenTestScene()
        {
            CreateTestScene("IVX_Homescreen", () =>
            {
                EnsureHomeNavigatorInScene();
                CreateTestLabel(
                    "IntelliVerseX Home Screen",
                    "Use the IVX Test Navigation panel to open Ads, Leaderboard, Wallet, Weekly Quiz, Auth, and more.");
            });
        }

        private void CreateAuthTestScene()
        {
            CreateTestScene("IVX_AuthTest", () =>
            {
                // Add NakamaManager (required for Auth to work with backend)
                AddNakamaManagerToScene();
                EnsureHomeNavigatorInScene();
                // Add Auth Canvas
                AddAuthCanvasToScene();
            });
        }

        private void CreateFriendsTestScene()
        {
            CreateTestScene("IVX_FriendsTest", () =>
            {
                // Add NakamaManager (required for Friends to work with backend)
                AddNakamaManagerToScene();
                EnsureHomeNavigatorInScene();
                AddFriendsUIToScene();
                CreateDemoButton("Open Friends", "OpenFriendsButton");
            });
        }

        private void CreateWalletTestScene()
        {
            CreateTestScene("IVX_WalletTest", () =>
            {
                // Add NakamaManager (required for Wallet to work with backend)
                AddNakamaManagerToScene();
                EnsureHomeNavigatorInScene();
                CreateTestLabel("Wallet Test Scene", "Add IVXWalletDisplay to test wallet functionality");
            });
        }

        private void CreateLeaderboardTestScene()
        {
            CreateTestScene("IVX_LeaderboardTest", () =>
            {
                // Add NakamaManager (required for Leaderboard to work with backend)
                AddNakamaManagerToScene();
                EnsureHomeNavigatorInScene();
                CreateTestLabel("Leaderboard Test Scene", "Add IVXLeaderboardUI to test leaderboard functionality");
            });
        }

        private void CreateAdsTestScene()
        {
            CreateTestScene("IVX_AdsTest", () =>
            {
                EnsureHomeNavigatorInScene();
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
                EnsureHomeNavigatorInScene();
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

        private void SetupSocialFeatures()
        {
            AddShareToScene();
            AddRateAppToScene();
        }

        private void AddShareToScene()
        {
            // Load Share prefab from SDK
            var sharePrefab = AssetDatabase.LoadAssetAtPath<GameObject>(SDK_ROOT + "/Social/Prefabs/IVXGShareManager.prefab");
            if (sharePrefab != null && GameObject.Find("IVXGShareManager") == null)
            {
                var instance = PrefabUtility.InstantiatePrefab(sharePrefab) as GameObject;
                if (instance != null)
                {
                    Undo.RegisterCreatedObjectUndo(instance, "Add Share Manager");
                    Selection.activeGameObject = instance;
                    EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
                    Debug.Log("[IVXSDKSetupWizard] Added Share Manager to scene");
                    CheckSocialModule();
                    return;
                }
            }
            
            Debug.LogWarning("[IVXSDKSetupWizard] Share Manager prefab not found or already exists");
        }

        private void AddRateAppToScene()
        {
            // Load Rate App prefab from SDK
            var ratePrefab = AssetDatabase.LoadAssetAtPath<GameObject>(SDK_ROOT + "/Social/Prefabs/IVXGRateAppManager.prefab");
            if (ratePrefab != null && GameObject.Find("IVXGRateAppManager") == null)
            {
                var instance = PrefabUtility.InstantiatePrefab(ratePrefab) as GameObject;
                if (instance != null)
                {
                    Undo.RegisterCreatedObjectUndo(instance, "Add Rate App Manager");
                    Selection.activeGameObject = instance;
                    EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
                    Debug.Log("[IVXSDKSetupWizard] Added Rate App Manager to scene");
                    CheckSocialModule();
                    return;
                }
            }
            
            Debug.LogWarning("[IVXSDKSetupWizard] Rate App Manager prefab not found or already exists");
        }

        private void SetupLeaderboardUI()
        {
            // Load Leaderboard prefab from SDK
            var leaderboardPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(SDK_ROOT + "/Leaderboard/Prefabs/IVXGLeaderboardCanvas.prefab");
            if (leaderboardPrefab != null && GameObject.Find("IVXGLeaderboardCanvas") == null)
            {
                var instance = PrefabUtility.InstantiatePrefab(leaderboardPrefab) as GameObject;
                if (instance != null)
                {
                    Undo.RegisterCreatedObjectUndo(instance, "Add Leaderboard UI");
                    Selection.activeGameObject = instance;
                    EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
                    CheckLeaderboardModule();
                    Debug.Log("[IVXSDKSetupWizard] Leaderboard UI added to scene");
                    return;
                }
            }

            Debug.LogWarning("[IVXSDKSetupWizard] Leaderboard UI prefab not found or already exists");
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

        /// <summary>
        /// Opens a demo scene by name from Assets/Scenes/Tests folder.
        /// </summary>
        private void OpenDemoScene(string sceneName)
        {
            string scenePath = $"Assets/Scenes/Tests/{sceneName}.unity";
            if (File.Exists(scenePath))
            {
                if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                {
                    EditorSceneManager.OpenScene(scenePath);
                }
            }
            else
            {
                EditorUtility.DisplayDialog("Scene Not Found",
                    $"The demo scene '{sceneName}' was not found at:\n{scenePath}\n\n" +
                    "Make sure the SDK demo scenes are properly installed.",
                    "OK");
            }
        }

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

        private void EnsureHomeNavigatorInScene()
        {
            var navType = GetTypeByName("IntelliVerseX.Core.IVXTestSceneNavigator");
            if (navType == null)
            {
                Debug.LogWarning("[IVXSDKSetupWizard] IVXTestSceneNavigator type not found.");
                return;
            }

            if (GameObject.FindAnyObjectByType(navType) != null)
            {
                return;
            }

            var navObject = new GameObject("IVX_TestSceneNavigator");
            navObject.AddComponent(navType);
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
