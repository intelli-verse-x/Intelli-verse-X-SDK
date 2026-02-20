#if UNITY_EDITOR
using System;
using System.IO;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using IntelliVerseX.Social;
using IntelliVerseX.Social.UI;

namespace IntelliVerseX.Social.Editor
{
    /// <summary>
    /// Setup wizard for the Friends module.
    /// Handles DOTween installation, config creation, and prefab setup.
    /// 
    /// Access via: Tools → IntelliVerse-X → Setup Friends Flow
    /// </summary>
    public class IVXFriendsSetupWizard : EditorWindow
    {
        #region Constants

        private const string WINDOW_TITLE = "Friends Setup Wizard";
        private const string MENU_PATH = "Tools/IntelliVerse-X/Setup Friends Flow";
        
        private const string CONFIG_RESOURCE_PATH = "Assets/Resources/IntelliVerseX";
        private const string CONFIG_ASSET_NAME = "FriendsConfig.asset";
        
        private const string PREFABS_SOURCE_PATH = "Assets/_IntelliVerseXSDK/Social/Prefabs";
        private const string PREFABS_DEST_PATH = "Assets/Prefabs/IntelliVerseX/Social";
        
        private const string DEMO_SCENE_SOURCE = "Assets/_IntelliVerseXSDK/Social/Scenes/IVX_FriendsDemo.unity";
        private const string DEMO_SCENE_DEST = "Assets/Scenes/IVX_FriendsDemo.unity";

        private const string DOTWEEN_PACKAGE_NAME = "com.demigiant.dotween";
        private const string DOTWEEN_ASSET_STORE_URL = "https://assetstore.unity.com/packages/tools/animation/dotween-hotween-v2-27676";

        #endregion

        #region State

        private bool _isDOTweenInstalled;
        private bool _isConfigCreated;
        private bool _arePrefabsCopied;
        private bool _isDemoSceneCreated;

        private bool _createConfig = true;
        private bool _copyPrefabs = false;
        private bool _createDemoScene = true;
        private bool _addToBuiltSettings = true;

        private Vector2 _scrollPosition;
        private GUIStyle _headerStyle;
        private GUIStyle _sectionStyle;
        private GUIStyle _statusStyle;
        private bool _stylesInitialized;

        #endregion

        #region Menu Items

        // NOTE: Main entry point is now through IntelliVerseX/SDK Setup Wizard
        // This wizard is kept for backwards compatibility but redirects to the main wizard
        
        [MenuItem(MENU_PATH, false, 100)]
        public static void ShowWindow()
        {
            // Redirect to main SDK wizard with a helpful message
            if (EditorUtility.DisplayDialog("Friends Setup",
                "The Friends module is now integrated into the main SDK Setup Wizard.\n\n" +
                "Go to: IntelliVerseX → SDK Setup Wizard → Auth & Social tab\n\n" +
                "Would you like to open the main wizard?",
                "Open SDK Wizard", "Open Legacy Wizard"))
            {
                EditorApplication.ExecuteMenuItem("IntelliVerse-X SDK/SDK Setup Wizard");
                return;
            }
            
            var window = GetWindow<IVXFriendsSetupWizard>(true, WINDOW_TITLE, true);
            window.minSize = new Vector2(450, 500);
            window.maxSize = new Vector2(500, 600);
            window.RefreshStatus();
        }

        // REMOVED: Menu item consolidated into SDK Setup Wizard
        // [MenuItem("IntelliVerse-X SDK/Friends/Add Friends UI to Scene", false, 101)]
        // Use: IntelliVerse-X SDK > SDK Setup Wizard > Features tab
        public static void AddFriendsUIToScene()
        {
            // Check if Friends UI already exists
#if UNITY_2023_1_OR_NEWER
            var existing = UnityEngine.Object.FindFirstObjectByType<IVXFriendsPanel>();
#else
            var existing = UnityEngine.Object.FindObjectOfType<IVXFriendsPanel>();
#endif
            if (existing != null)
            {
                EditorUtility.DisplayDialog("Friends UI Exists",
                    "A Friends Panel already exists in this scene.", "OK");
                Selection.activeGameObject = existing.gameObject;
                return;
            }

            // Generate prefabs if they don't exist
            string prefabsPath = "Assets/_IntelliVerseXSDK/Social/Prefabs";
            if (!Directory.Exists(prefabsPath) || 
                Directory.GetFiles(prefabsPath, "*.prefab").Length == 0)
            {
                IVXFriendsPrefabBuilder.SavePrefabs(prefabsPath);
            }

            // Create the Friends Canvas
            var friendsCanvas = IVXFriendsPrefabBuilder.CreateFriendsCanvas();

            // Wire up prefab references
            var panel = friendsCanvas.GetComponentInChildren<IVXFriendsPanel>();
            if (panel != null)
            {
                var serializedObject = new SerializedObject(panel);
                
                var friendSlotPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(
                    Path.Combine(prefabsPath, "IVXFriendSlot.prefab"));
                var requestSlotPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(
                    Path.Combine(prefabsPath, "IVXFriendRequestSlot.prefab"));
                var searchSlotPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(
                    Path.Combine(prefabsPath, "IVXFriendSearchSlot.prefab"));

                var friendSlotProp = serializedObject.FindProperty("friendSlotPrefab");
                var requestSlotProp = serializedObject.FindProperty("requestSlotPrefab");
                var searchSlotProp = serializedObject.FindProperty("searchSlotPrefab");

                if (friendSlotProp != null && friendSlotPrefab != null)
                    friendSlotProp.objectReferenceValue = friendSlotPrefab;
                if (requestSlotProp != null && requestSlotPrefab != null)
                    requestSlotProp.objectReferenceValue = requestSlotPrefab;
                if (searchSlotProp != null && searchSlotPrefab != null)
                    searchSlotProp.objectReferenceValue = searchSlotPrefab;

                serializedObject.ApplyModifiedPropertiesWithoutUndo();
            }

            // Select the new object
            Selection.activeGameObject = friendsCanvas;
            EditorGUIUtility.PingObject(friendsCanvas);

            // Mark scene dirty
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
                UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());

            Debug.Log("[IVXFriends] Added Friends UI to current scene. Call IVXFriendsPanel.Instance.Open() to show it.");
        }

        // [MenuItem("IntelliVerse-X SDK/Friends/Regenerate Friends Prefabs", false, 102)]
        // Use: IntelliVerse-X SDK > SDK Setup Wizard > Features tab
        public static void RegeneratePrefabs()
        {
            string prefabsPath = "Assets/_IntelliVerseXSDK/Social/Prefabs";
            IVXFriendsPrefabBuilder.SavePrefabs(prefabsPath);
            AssetDatabase.Refresh();
            EditorUtility.DisplayDialog("Prefabs Regenerated",
                $"Friends prefabs have been regenerated at:\n{prefabsPath}", "OK");
        }

        // [MenuItem("IntelliVerse-X SDK/Friends/Create Friends Demo Scene", false, 103)]
        // Use: IntelliVerse-X SDK > SDK Setup Wizard > Test Scenes tab
        public static void CreateFriendsDemoSceneMenuItem()
        {
            CreateFriendsDemoScene();
        }

        /// <summary>
        /// Creates a complete Friends demo scene with all UI elements.
        /// </summary>
        public static void CreateFriendsDemoScene()
        {
            string scenePath = DEMO_SCENE_DEST;
            string scenesDir = Path.GetDirectoryName(scenePath);
            
            if (!Directory.Exists(scenesDir))
            {
                Directory.CreateDirectory(scenesDir);
            }

            // Create new scene
            var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);

            // Set camera background color
            var camera = Camera.main;
            if (camera != null)
            {
                camera.backgroundColor = new Color(0.22f, 0.27f, 0.35f, 1f);
                camera.clearFlags = CameraClearFlags.SolidColor;
            }

            // Add EventSystem if not present
#if UNITY_2023_1_OR_NEWER
            if (UnityEngine.Object.FindFirstObjectByType<UnityEngine.EventSystems.EventSystem>() == null)
#else
            if (UnityEngine.Object.FindObjectOfType<UnityEngine.EventSystems.EventSystem>() == null)
#endif
            {
                var eventSystem = new GameObject("EventSystem");
                eventSystem.AddComponent<UnityEngine.EventSystems.EventSystem>();
                AddAppropriateInputModule(eventSystem);
            }

            // Generate prefabs if needed
            string prefabsPath = "Assets/_IntelliVerseXSDK/Social/Prefabs";
            if (!Directory.Exists(prefabsPath) || Directory.GetFiles(prefabsPath, "*.prefab").Length == 0)
            {
                IVXFriendsPrefabBuilder.SavePrefabs(prefabsPath);
            }

            // Create the complete Friends Canvas
            var friendsCanvas = IVXFriendsPrefabBuilder.CreateFriendsCanvas();

            // Wire up prefab references
            var panel = friendsCanvas.GetComponentInChildren<IVXFriendsPanel>();
            if (panel != null)
            {
                var serializedObject = new SerializedObject(panel);
                
                var friendSlotPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(
                    Path.Combine(prefabsPath, "IVXFriendSlot.prefab"));
                var requestSlotPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(
                    Path.Combine(prefabsPath, "IVXFriendRequestSlot.prefab"));
                var searchSlotPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(
                    Path.Combine(prefabsPath, "IVXFriendSearchSlot.prefab"));

                var friendSlotProp = serializedObject.FindProperty("friendSlotPrefab");
                var requestSlotProp = serializedObject.FindProperty("requestSlotPrefab");
                var searchSlotProp = serializedObject.FindProperty("searchSlotPrefab");

                if (friendSlotProp != null && friendSlotPrefab != null)
                    friendSlotProp.objectReferenceValue = friendSlotPrefab;
                if (requestSlotProp != null && requestSlotPrefab != null)
                    requestSlotProp.objectReferenceValue = requestSlotPrefab;
                if (searchSlotProp != null && searchSlotPrefab != null)
                    searchSlotProp.objectReferenceValue = searchSlotPrefab;

                serializedObject.ApplyModifiedPropertiesWithoutUndo();
            }

            // Create Demo Canvas with "Open Friends" button
            var demoCanvasGO = new GameObject("DemoCanvas");
            var demoCanvas = demoCanvasGO.AddComponent<Canvas>();
            demoCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            demoCanvas.sortingOrder = 50;

            var scaler = demoCanvasGO.AddComponent<UnityEngine.UI.CanvasScaler>();
            scaler.uiScaleMode = UnityEngine.UI.CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080, 1920);
            scaler.matchWidthOrHeight = 0.5f;

            demoCanvasGO.AddComponent<UnityEngine.UI.GraphicRaycaster>();

            // Create "Open Friends" button
            var buttonGO = new GameObject("OpenFriendsButton");
            buttonGO.transform.SetParent(demoCanvasGO.transform, false);
            
            var buttonRect = buttonGO.AddComponent<RectTransform>();
            buttonRect.anchorMin = new Vector2(0.5f, 0.5f);
            buttonRect.anchorMax = new Vector2(0.5f, 0.5f);
            buttonRect.sizeDelta = new Vector2(280, 70);
            
            var buttonImage = buttonGO.AddComponent<UnityEngine.UI.Image>();
            buttonImage.color = new Color(0.25f, 0.52f, 0.96f, 1f);
            
            var button = buttonGO.AddComponent<UnityEngine.UI.Button>();
            button.targetGraphic = buttonImage;

            // Add button text
            var textGO = new GameObject("Text");
            textGO.transform.SetParent(buttonGO.transform, false);
            
            var textRect = textGO.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.sizeDelta = Vector2.zero;

            var text = textGO.AddComponent<TMPro.TextMeshProUGUI>();
            text.text = "Open Friends";
            text.alignment = TMPro.TextAlignmentOptions.Center;
            text.fontSize = 28;
            text.fontStyle = TMPro.FontStyles.Bold;
            text.color = Color.white;

            // Add demo controller (runtime version)
            var demoController = demoCanvasGO.AddComponent<IntelliVerseX.Social.UI.IVXFriendsDemoController>();

            // Save scene
            EditorSceneManager.SaveScene(scene, scenePath);

            // Add to build settings
            var scenes = new System.Collections.Generic.List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
            bool alreadyAdded = false;
            foreach (var s in scenes)
            {
                if (s.path == scenePath)
                {
                    alreadyAdded = true;
                    break;
                }
            }
            if (!alreadyAdded)
            {
                scenes.Add(new EditorBuildSettingsScene(scenePath, true));
                EditorBuildSettings.scenes = scenes.ToArray();
            }

            Debug.Log($"[IVXFriends] Created Friends Demo Scene at: {scenePath}");
            EditorUtility.DisplayDialog("Demo Scene Created",
                "Friends Demo Scene created successfully!\n\n" +
                "Click 'Open Friends' button in Play Mode to test the Friends panel.",
                "OK");
        }

        #endregion

        #region Unity Callbacks

        private void OnEnable()
        {
            RefreshStatus();
        }

        private void OnGUI()
        {
            InitStyles();

            _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);

            DrawHeader();
            EditorGUILayout.Space(10);

            DrawDOTweenSection();
            EditorGUILayout.Space(10);

            DrawConfigSection();
            EditorGUILayout.Space(10);

            DrawPrefabsSection();
            EditorGUILayout.Space(10);

            DrawDemoSceneSection();
            EditorGUILayout.Space(20);

            DrawRunSetupButton();
            EditorGUILayout.Space(10);

            DrawHelpSection();

            EditorGUILayout.EndScrollView();
        }

        #endregion

        #region Status Check

        private void RefreshStatus()
        {
            _isDOTweenInstalled = CheckDOTweenInstalled();
            _isConfigCreated = CheckConfigExists();
            _arePrefabsCopied = CheckPrefabsCopied();
            _isDemoSceneCreated = CheckDemoSceneExists();
        }

        private bool CheckDOTweenInstalled()
        {
            // Check for DOTween namespace
            var dotweenType = Type.GetType("DG.Tweening.DOTween, DOTween");
            if (dotweenType != null) return true;

            // Check for DOTween assembly
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                if (assembly.GetName().Name == "DOTween")
                    return true;
            }

            // Check for DOTween folder
            return Directory.Exists("Assets/Plugins/Demigiant/DOTween") ||
                   Directory.Exists("Assets/DOTween");
        }

        private bool CheckConfigExists()
        {
            string path = Path.Combine(CONFIG_RESOURCE_PATH, CONFIG_ASSET_NAME);
            return File.Exists(path);
        }

        private bool CheckPrefabsCopied()
        {
            return Directory.Exists(PREFABS_DEST_PATH) &&
                   Directory.GetFiles(PREFABS_DEST_PATH, "*.prefab").Length > 0;
        }

        private bool CheckDemoSceneExists()
        {
            return File.Exists(DEMO_SCENE_DEST);
        }

        #endregion

        #region GUI Drawing

        private void InitStyles()
        {
            if (_stylesInitialized) return;

            _headerStyle = new GUIStyle(EditorStyles.boldLabel)
            {
                fontSize = 16,
                alignment = TextAnchor.MiddleCenter
            };

            _sectionStyle = new GUIStyle(EditorStyles.helpBox)
            {
                padding = new RectOffset(10, 10, 10, 10)
            };

            _statusStyle = new GUIStyle(EditorStyles.label)
            {
                richText = true
            };

            _stylesInitialized = true;
        }

        private void DrawHeader()
        {
            EditorGUILayout.LabelField("IntelliVerse-X Friends Module", _headerStyle);
            EditorGUILayout.LabelField("Setup Wizard", EditorStyles.centeredGreyMiniLabel);
        }

        private void DrawDOTweenSection()
        {
            EditorGUILayout.BeginVertical(_sectionStyle);
            
            EditorGUILayout.LabelField("1. DOTween (Required)", EditorStyles.boldLabel);
            
            EditorGUILayout.BeginHorizontal();
            DrawStatusIcon(_isDOTweenInstalled);
            EditorGUILayout.LabelField(_isDOTweenInstalled 
                ? "DOTween is installed" 
                : "DOTween is not installed", _statusStyle);
            EditorGUILayout.EndHorizontal();

            if (!_isDOTweenInstalled)
            {
                EditorGUILayout.HelpBox(
                    "DOTween is required for UI animations. Install it from the Asset Store (free version available).",
                    MessageType.Warning);

                EditorGUILayout.BeginHorizontal();
                
                if (GUILayout.Button("Open Asset Store", GUILayout.Height(25)))
                {
                    Application.OpenURL(DOTWEEN_ASSET_STORE_URL);
                }

                if (GUILayout.Button("Run DOTween Setup", GUILayout.Height(25)))
                {
                    RunDOTweenSetup();
                }
                
                EditorGUILayout.EndHorizontal();
            }
            else
            {
                if (GUILayout.Button("Run DOTween Setup Panel", GUILayout.Height(25)))
                {
                    RunDOTweenSetup();
                }
            }

            EditorGUILayout.EndVertical();
        }

        private void DrawConfigSection()
        {
            EditorGUILayout.BeginVertical(_sectionStyle);
            
            EditorGUILayout.LabelField("2. Friends Config Asset", EditorStyles.boldLabel);
            
            EditorGUILayout.BeginHorizontal();
            DrawStatusIcon(_isConfigCreated);
            EditorGUILayout.LabelField(_isConfigCreated 
                ? "Config asset exists" 
                : "Config asset not created", _statusStyle);
            EditorGUILayout.EndHorizontal();

            _createConfig = EditorGUILayout.Toggle("Create Config Asset", _createConfig || !_isConfigCreated);

            if (_isConfigCreated)
            {
                if (GUILayout.Button("Select Config Asset", GUILayout.Height(22)))
                {
                    var asset = AssetDatabase.LoadAssetAtPath<IVXFriendsConfig>(
                        Path.Combine(CONFIG_RESOURCE_PATH, CONFIG_ASSET_NAME));
                    if (asset != null)
                    {
                        Selection.activeObject = asset;
                        EditorGUIUtility.PingObject(asset);
                    }
                }
            }

            EditorGUILayout.EndVertical();
        }

        private void DrawPrefabsSection()
        {
            EditorGUILayout.BeginVertical(_sectionStyle);
            
            EditorGUILayout.LabelField("3. UI Prefabs (Optional)", EditorStyles.boldLabel);
            
            EditorGUILayout.BeginHorizontal();
            DrawStatusIcon(_arePrefabsCopied);
            EditorGUILayout.LabelField(_arePrefabsCopied 
                ? "Prefabs copied to project" 
                : "Using SDK prefabs directly", _statusStyle);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.HelpBox(
                "You can use SDK prefabs directly or copy them to your project for customization.",
                MessageType.Info);

            _copyPrefabs = EditorGUILayout.Toggle("Copy Prefabs to Project", _copyPrefabs);

            EditorGUILayout.EndVertical();
        }

        private void DrawDemoSceneSection()
        {
            EditorGUILayout.BeginVertical(_sectionStyle);
            
            EditorGUILayout.LabelField("4. Demo Scene", EditorStyles.boldLabel);
            
            EditorGUILayout.BeginHorizontal();
            DrawStatusIcon(_isDemoSceneCreated);
            EditorGUILayout.LabelField(_isDemoSceneCreated 
                ? "Demo scene exists" 
                : "Demo scene not created", _statusStyle);
            EditorGUILayout.EndHorizontal();

            _createDemoScene = EditorGUILayout.Toggle("Create Demo Scene", _createDemoScene && !_isDemoSceneCreated);

            if (_createDemoScene && !_isDemoSceneCreated)
            {
                _addToBuiltSettings = EditorGUILayout.Toggle("Add to Build Settings", _addToBuiltSettings);
            }

            if (_isDemoSceneCreated)
            {
                if (GUILayout.Button("Open Demo Scene", GUILayout.Height(22)))
                {
                    if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                    {
                        EditorSceneManager.OpenScene(DEMO_SCENE_DEST);
                    }
                }
            }

            EditorGUILayout.EndVertical();
        }

        private void DrawRunSetupButton()
        {
            bool canRun = !_isDOTweenInstalled || _createConfig || _copyPrefabs || _createDemoScene;

            EditorGUI.BeginDisabledGroup(!canRun && _isDOTweenInstalled);
            
            var buttonStyle = new GUIStyle(GUI.skin.button)
            {
                fontSize = 14,
                fontStyle = FontStyle.Bold
            };

            if (GUILayout.Button("Run Setup", buttonStyle, GUILayout.Height(40)))
            {
                RunSetup();
            }
            
            EditorGUI.EndDisabledGroup();

            if (!_isDOTweenInstalled)
            {
                EditorGUILayout.HelpBox(
                    "Please install DOTween first before running setup.",
                    MessageType.Warning);
            }
        }

        private void DrawHelpSection()
        {
            EditorGUILayout.BeginVertical(_sectionStyle);
            
            EditorGUILayout.LabelField("Help & Documentation", EditorStyles.boldLabel);

            if (GUILayout.Button("Open README", GUILayout.Height(22)))
            {
                var readme = AssetDatabase.LoadAssetAtPath<TextAsset>(
                    "Assets/_IntelliVerseXSDK/Social/README.md");
                if (readme != null)
                {
                    Selection.activeObject = readme;
                    EditorGUIUtility.PingObject(readme);
                }
            }

            if (GUILayout.Button("Open SDK Documentation", GUILayout.Height(22)))
            {
                Application.OpenURL("https://docs.intelli-verse-x.ai/sdk/friends");
            }

            EditorGUILayout.EndVertical();
        }

        private void DrawStatusIcon(bool isComplete)
        {
            var icon = isComplete 
                ? EditorGUIUtility.IconContent("TestPassed")
                : EditorGUIUtility.IconContent("TestNormal");
            
            GUILayout.Label(icon, GUILayout.Width(20), GUILayout.Height(20));
        }

        #endregion

        #region Setup Actions

        private void RunSetup()
        {
            try
            {
                EditorUtility.DisplayProgressBar("Friends Setup", "Starting setup...", 0f);

                // Step 1: Check DOTween
                if (!_isDOTweenInstalled)
                {
                    EditorUtility.ClearProgressBar();
                    if (EditorUtility.DisplayDialog("DOTween Required",
                        "DOTween must be installed before running setup. Would you like to open the Asset Store?",
                        "Open Asset Store", "Cancel"))
                    {
                        Application.OpenURL(DOTWEEN_ASSET_STORE_URL);
                    }
                    return;
                }

                // Step 2: Create config
                if (_createConfig && !_isConfigCreated)
                {
                    EditorUtility.DisplayProgressBar("Friends Setup", "Creating config asset...", 0.25f);
                    CreateConfigAsset();
                }

                // Step 3: Copy prefabs
                if (_copyPrefabs && !_arePrefabsCopied)
                {
                    EditorUtility.DisplayProgressBar("Friends Setup", "Copying prefabs...", 0.5f);
                    CopyPrefabs();
                }

                // Step 4: Create demo scene
                if (_createDemoScene && !_isDemoSceneCreated)
                {
                    EditorUtility.DisplayProgressBar("Friends Setup", "Creating demo scene...", 0.75f);
                    CreateDemoScene();
                }

                EditorUtility.DisplayProgressBar("Friends Setup", "Finalizing...", 1f);
                AssetDatabase.Refresh();

                RefreshStatus();

                EditorUtility.ClearProgressBar();
                EditorUtility.DisplayDialog("Setup Complete",
                    "Friends module setup completed successfully!\n\n" +
                    "Next steps:\n" +
                    "1. Add IVXFriendsCanvas prefab to your scene\n" +
                    "2. Call IVXFriendsPanel.Instance.Open() to show the panel\n" +
                    "3. Customize the FriendsConfig asset as needed",
                    "OK");
            }
            catch (Exception ex)
            {
                EditorUtility.ClearProgressBar();
                EditorUtility.DisplayDialog("Setup Error",
                    $"An error occurred during setup:\n{ex.Message}",
                    "OK");
                Debug.LogError($"[IVXFriends Setup] Error: {ex}");
            }
        }

        private void CreateConfigAsset()
        {
            // Ensure directory exists
            if (!Directory.Exists(CONFIG_RESOURCE_PATH))
            {
                Directory.CreateDirectory(CONFIG_RESOURCE_PATH);
            }

            string fullPath = Path.Combine(CONFIG_RESOURCE_PATH, CONFIG_ASSET_NAME);

            // Create the asset
            var config = CreateInstance<IVXFriendsConfig>();
            AssetDatabase.CreateAsset(config, fullPath);
            AssetDatabase.SaveAssets();

            Debug.Log($"[IVXFriends Setup] Created config asset at: {fullPath}");
            _isConfigCreated = true;
        }

        private void CopyPrefabs()
        {
            // Ensure destination directory exists
            if (!Directory.Exists(PREFABS_DEST_PATH))
            {
                Directory.CreateDirectory(PREFABS_DEST_PATH);
            }

            // Copy prefabs
            if (Directory.Exists(PREFABS_SOURCE_PATH))
            {
                var prefabFiles = Directory.GetFiles(PREFABS_SOURCE_PATH, "*.prefab");
                foreach (var file in prefabFiles)
                {
                    string fileName = Path.GetFileName(file);
                    string destPath = Path.Combine(PREFABS_DEST_PATH, fileName);
                    
                    if (!File.Exists(destPath))
                    {
                        AssetDatabase.CopyAsset(file, destPath);
                    }
                }
            }

            Debug.Log($"[IVXFriends Setup] Copied prefabs to: {PREFABS_DEST_PATH}");
            _arePrefabsCopied = true;
        }

        private void CreateDemoScene()
        {
            // Ensure scenes directory exists
            string scenesDir = Path.GetDirectoryName(DEMO_SCENE_DEST);
            if (!Directory.Exists(scenesDir))
            {
                Directory.CreateDirectory(scenesDir);
            }

            // Create new scene
            var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);

            // Add EventSystem if not present
#if UNITY_2023_1_OR_NEWER
            if (UnityEngine.Object.FindFirstObjectByType<UnityEngine.EventSystems.EventSystem>() == null)
#else
            if (UnityEngine.Object.FindObjectOfType<UnityEngine.EventSystems.EventSystem>() == null)
#endif
            {
                var eventSystem = new GameObject("EventSystem");
                eventSystem.AddComponent<UnityEngine.EventSystems.EventSystem>();
                AddAppropriateInputModule(eventSystem);
            }

            // Create demo UI
            CreateDemoUI();

            // Save scene
            EditorSceneManager.SaveScene(scene, DEMO_SCENE_DEST);

            // Add to build settings if requested
            if (_addToBuiltSettings)
            {
                AddSceneToBuildSettings(DEMO_SCENE_DEST);
            }

            Debug.Log($"[IVXFriends Setup] Created demo scene at: {DEMO_SCENE_DEST}");
            _isDemoSceneCreated = true;
        }

        private void CreateDemoUI()
        {
            // First, generate and save the slot prefabs
            string prefabsPath = "Assets/_IntelliVerseXSDK/Social/Prefabs";
            IVXFriendsPrefabBuilder.SavePrefabs(prefabsPath);

            // Create the complete Friends Canvas with all UI
            var friendsCanvas = IVXFriendsPrefabBuilder.CreateFriendsCanvas();
            
            // Load the slot prefabs and assign them to the panel
            var panel = friendsCanvas.GetComponentInChildren<IVXFriendsPanel>();
            if (panel != null)
            {
                var serializedObject = new SerializedObject(panel);
                
                // Load and assign prefabs
                var friendSlotPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(
                    Path.Combine(prefabsPath, "IVXFriendSlot.prefab"));
                var requestSlotPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(
                    Path.Combine(prefabsPath, "IVXFriendRequestSlot.prefab"));
                var searchSlotPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(
                    Path.Combine(prefabsPath, "IVXFriendSearchSlot.prefab"));

                var friendSlotProp = serializedObject.FindProperty("friendSlotPrefab");
                var requestSlotProp = serializedObject.FindProperty("requestSlotPrefab");
                var searchSlotProp = serializedObject.FindProperty("searchSlotPrefab");

                if (friendSlotProp != null && friendSlotPrefab != null)
                    friendSlotProp.objectReferenceValue = friendSlotPrefab;
                if (requestSlotProp != null && requestSlotPrefab != null)
                    requestSlotProp.objectReferenceValue = requestSlotPrefab;
                if (searchSlotProp != null && searchSlotPrefab != null)
                    searchSlotProp.objectReferenceValue = searchSlotPrefab;

                serializedObject.ApplyModifiedPropertiesWithoutUndo();
            }

            // Create Demo Canvas with "Open Friends" button
            var demoCanvasGO = new GameObject("DemoCanvas");
            var demoCanvas = demoCanvasGO.AddComponent<Canvas>();
            demoCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            demoCanvas.sortingOrder = 50; // Below friends panel

            var scaler = demoCanvasGO.AddComponent<UnityEngine.UI.CanvasScaler>();
            scaler.uiScaleMode = UnityEngine.UI.CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080, 1920);
            scaler.matchWidthOrHeight = 0.5f;

            demoCanvasGO.AddComponent<UnityEngine.UI.GraphicRaycaster>();

            // Create "Open Friends" button
            var buttonGO = new GameObject("OpenFriendsButton");
            buttonGO.transform.SetParent(demoCanvasGO.transform, false);
            
            var buttonRect = buttonGO.AddComponent<RectTransform>();
            buttonRect.anchorMin = new Vector2(0.5f, 0.5f);
            buttonRect.anchorMax = new Vector2(0.5f, 0.5f);
            buttonRect.sizeDelta = new Vector2(250, 60);
            
            var buttonImage = buttonGO.AddComponent<UnityEngine.UI.Image>();
            buttonImage.color = new Color(0.25f, 0.52f, 0.96f, 1f);
            
            var button = buttonGO.AddComponent<UnityEngine.UI.Button>();
            button.targetGraphic = buttonImage;

            // Add button text
            var textGO = new GameObject("Text");
            textGO.transform.SetParent(buttonGO.transform, false);
            
            var textRect = textGO.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.sizeDelta = Vector2.zero;

            var text = textGO.AddComponent<TMPro.TextMeshProUGUI>();
            text.text = "Open Friends";
            text.alignment = TMPro.TextAlignmentOptions.Center;
            text.fontSize = 24;
            text.fontStyle = TMPro.FontStyles.Bold;
            text.color = Color.white;

            // Add demo controller (runtime version, not editor)
            var demoController = demoCanvasGO.AddComponent<IntelliVerseX.Social.UI.IVXFriendsDemoController>();

            Debug.Log("[IVXFriends Setup] Created complete Friends UI in scene");
        }

        private void AddSceneToBuildSettings(string scenePath)
        {
            var scenes = new System.Collections.Generic.List<EditorBuildSettingsScene>(
                EditorBuildSettings.scenes);

            // Check if already added
            foreach (var scene in scenes)
            {
                if (scene.path == scenePath)
                    return;
            }

            scenes.Add(new EditorBuildSettingsScene(scenePath, true));
            EditorBuildSettings.scenes = scenes.ToArray();

            Debug.Log($"[IVXFriends Setup] Added scene to build settings: {scenePath}");
        }

        private void RunDOTweenSetup()
        {
            // Try to open DOTween utility panel
            var dotweenUtilityType = Type.GetType("DG.DOTweenEditor.DOTweenUtilityWindow, DOTweenEditor");
            if (dotweenUtilityType != null)
            {
                var showMethod = dotweenUtilityType.GetMethod("Open",
                    System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
                if (showMethod != null)
                {
                    showMethod.Invoke(null, null);
                    return;
                }
            }

            // Fallback: try menu item
            EditorApplication.ExecuteMenuItem("Tools/Demigiant/DOTween Utility Panel");
        }

        /// <summary>
        /// Adds the appropriate input module to an EventSystem based on the active input handling setting.
        /// Supports both New Input System (InputSystemUIInputModule) and Legacy (StandaloneInputModule).
        /// </summary>
        private static void AddAppropriateInputModule(GameObject eventSystem)
        {
            // Check if the new Input System package is installed and should be used
            bool useNewInputSystem = IsNewInputSystemActive();

            if (useNewInputSystem)
            {
                // Try to add InputSystemUIInputModule (from Input System package)
                var inputModuleType = GetTypeByName("UnityEngine.InputSystem.UI.InputSystemUIInputModule");
                if (inputModuleType != null)
                {
                    eventSystem.AddComponent(inputModuleType);
                    Debug.Log("[IVXFriendsSetupWizard] Created EventSystem with InputSystemUIInputModule (New Input System)");
                    return;
                }
            }

            // Fallback to legacy StandaloneInputModule
            eventSystem.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
            Debug.Log("[IVXFriendsSetupWizard] Created EventSystem with StandaloneInputModule (Legacy Input)");
        }

        /// <summary>
        /// Checks if the New Input System is active in the project settings.
        /// </summary>
        private static bool IsNewInputSystemActive()
        {
            // Check if InputSystem package is installed
            var inputSystemType = GetTypeByName("UnityEngine.InputSystem.InputSystem");
            if (inputSystemType == null)
            {
                return false;
            }

            try
            {
                // Check Player Settings for active input handling
                var playerSettingsType = typeof(UnityEditor.PlayerSettings);
                var prop = playerSettingsType.GetProperty("activeInputHandler",
                    System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);

                if (prop != null)
                {
                    var value = (int)prop.GetValue(null);
                    return value >= 1; // 1 = Input System Package, 2 = Both
                }

                // Fallback: if InputSystemUIInputModule exists, use it
                return GetTypeByName("UnityEngine.InputSystem.UI.InputSystemUIInputModule") != null;
            }
            catch
            {
                return GetTypeByName("UnityEngine.InputSystem.UI.InputSystemUIInputModule") != null;
            }
        }

        /// <summary>
        /// Gets a Type by its full name from all loaded assemblies.
        /// </summary>
        private static Type GetTypeByName(string fullName)
        {
            if (string.IsNullOrEmpty(fullName)) return null;

            var type = Type.GetType(fullName);
            if (type != null) return type;

            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                try
                {
                    type = assembly.GetType(fullName);
                    if (type != null) return type;
                }
                catch { }
            }
            return null;
        }

        #endregion
    }

}
#endif
