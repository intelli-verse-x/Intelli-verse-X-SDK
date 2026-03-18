using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
#if UNITY_EDITOR
using UnityEditor.SceneManagement;
#endif

namespace IntelliVerseX.Core
{
    /// <summary>
    /// Lightweight test-scene navigation overlay for SDK validation flows.
    /// Allows switching between test scenes and returning to the home scene.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class IVXTestSceneNavigator : MonoBehaviour
    {
        [Serializable]
        public struct SceneLink
        {
            public string Label;
            public string SceneName;
            public bool ShowOnHome;
            public bool ShowOnFeatureScenes;
        }

        [Header("Navigation")]
        [SerializeField] private string _homeSceneName = "IVX_HomeScreen";
        [SerializeField] private KeyCode _toggleOverlayKey = KeyCode.F8;
        [SerializeField] private bool _overlayVisible = true;
        [SerializeField] private int _sortingOrder = 500;

        [Header("Links")]
        [SerializeField] private List<SceneLink> _sceneLinks = new List<SceneLink>
        {
            new SceneLink { Label = "Auth",        SceneName = "IVX_AuthTest",        ShowOnHome = true, ShowOnFeatureScenes = true },
            new SceneLink { Label = "Ads",         SceneName = "IVX_AdsTest",         ShowOnHome = true, ShowOnFeatureScenes = true },
            new SceneLink { Label = "Leaderboard", SceneName = "IVX_LeaderboardTest", ShowOnHome = true, ShowOnFeatureScenes = true },
            new SceneLink { Label = "Wallet",      SceneName = "IVX_WalletTest",      ShowOnHome = true, ShowOnFeatureScenes = true },
            new SceneLink { Label = "Weekly Quiz", SceneName = "IVX_WeeklyQuizTest",  ShowOnHome = true, ShowOnFeatureScenes = true },
            new SceneLink { Label = "Daily Quiz",  SceneName = "IVX_DailyQuiz",       ShowOnHome = true, ShowOnFeatureScenes = true },
            new SceneLink { Label = "Profile",     SceneName = "IVX_Profile",         ShowOnHome = true, ShowOnFeatureScenes = true },
            new SceneLink { Label = "Friends",     SceneName = "IVX_Friends",         ShowOnHome = true, ShowOnFeatureScenes = true },
            new SceneLink { Label = "More Of Us",  SceneName = "IVX_MoreOfUs",        ShowOnHome = true, ShowOnFeatureScenes = true },
            new SceneLink { Label = "Share/Rate",  SceneName = "IVX_Share&RateUs",    ShowOnHome = true, ShowOnFeatureScenes = true }
        };

        private GameObject _canvasObject;
        private RectTransform _panelRoot;
        private Font _font;

        private void Awake()
        {
            EnsureRequiredLinks();
            EnsureEventSystem();
            BuildNavigationUI();
            ApplyVisibility();
        }

        private void Update()
        {
            if (IsToggleKeyPressed(_toggleOverlayKey))
            {
                _overlayVisible = !_overlayVisible;
                ApplyVisibility();
            }
        }

        private void EnsureRequiredLinks()
        {
            if (_sceneLinks == null)
            {
                _sceneLinks = new List<SceneLink>();
            }

            AddLinkIfMissing("Auth", "IVX_AuthTest");
            AddLinkIfMissing("Ads", "IVX_AdsTest");
            AddLinkIfMissing("Leaderboard", "IVX_LeaderboardTest");
            AddLinkIfMissing("Wallet", "IVX_WalletTest");
            AddLinkIfMissing("Weekly Quiz", "IVX_WeeklyQuizTest");
            AddLinkIfMissing("More Of Us", "IVX_MoreOfUsTest");
            AddLinkIfMissing("Friends", "IVX_FriendsTest");
            AddLinkIfMissing("Full", "IVX_FullTest");
            AddBuildSettingsTestSceneLinks();
            AddEditorTestSceneLinks();
        }

        private void AddLinkIfMissing(string label, string sceneName)
        {
            for (int i = 0; i < _sceneLinks.Count; i++)
            {
                if (string.Equals(_sceneLinks[i].SceneName, sceneName, StringComparison.Ordinal))
                {
                    return;
                }
            }

            _sceneLinks.Add(new SceneLink
            {
                Label = label,
                SceneName = sceneName,
                ShowOnHome = true,
                ShowOnFeatureScenes = true
            });
        }

        private void AddBuildSettingsTestSceneLinks()
        {
            int count = SceneManager.sceneCountInBuildSettings;
            for (int i = 0; i < count; i++)
            {
                string scenePath = SceneUtility.GetScenePathByBuildIndex(i);
                if (string.IsNullOrWhiteSpace(scenePath) || scenePath.IndexOf("/Scenes/Tests/", StringComparison.OrdinalIgnoreCase) < 0)
                {
                    continue;
                }

                string sceneName = Path.GetFileNameWithoutExtension(scenePath);
                if (string.IsNullOrWhiteSpace(sceneName) || string.Equals(sceneName, _homeSceneName, StringComparison.Ordinal))
                {
                    continue;
                }

                AddLinkIfMissing(sceneName, sceneName);
            }
        }

        private void AddEditorTestSceneLinks()
        {
#if UNITY_EDITOR
            const string editorTestFolder = "Assets/Scenes/Tests";
            if (!Directory.Exists(editorTestFolder))
            {
                return;
            }

            string[] scenePaths = Directory.GetFiles(editorTestFolder, "*.unity", SearchOption.TopDirectoryOnly);
            for (int i = 0; i < scenePaths.Length; i++)
            {
                string sceneName = Path.GetFileNameWithoutExtension(scenePaths[i]);
                if (string.IsNullOrWhiteSpace(sceneName) || string.Equals(sceneName, _homeSceneName, StringComparison.Ordinal))
                {
                    continue;
                }

                AddLinkIfMissing(sceneName, sceneName);
            }
#endif
        }

        private void EnsureEventSystem()
        {
            if (FindFirstObjectByType<EventSystem>() != null)
            {
                return;
            }

            var eventSystemGo = new GameObject("EventSystem");
            eventSystemGo.AddComponent<EventSystem>();

            // Prefer Input System UI module when package is available, fallback to legacy.
            Type inputSystemUiModule = FindType("UnityEngine.InputSystem.UI.InputSystemUIInputModule");
            if (inputSystemUiModule != null && typeof(BaseInputModule).IsAssignableFrom(inputSystemUiModule))
            {
                eventSystemGo.AddComponent(inputSystemUiModule);
                return;
            }

            eventSystemGo.AddComponent<StandaloneInputModule>();
        }

        private void BuildNavigationUI()
        {
            string activeScene = SceneManager.GetActiveScene().name;
            bool isHomeScene = string.Equals(activeScene, _homeSceneName, StringComparison.Ordinal);

            _font = _font != null ? _font : Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

            _canvasObject = new GameObject("IVX_TestSceneMenuCanvas");
            _canvasObject.transform.SetParent(transform, false);

            var canvas = _canvasObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = _sortingOrder;

            var scaler = _canvasObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080f, 1920f);
            scaler.matchWidthOrHeight = 0.5f;
            _canvasObject.AddComponent<GraphicRaycaster>();

            var panelGo = CreateUIObject("Panel", _canvasObject.transform);
            _panelRoot = panelGo.GetComponent<RectTransform>();
            _panelRoot.anchorMin = new Vector2(0f, 1f);
            _panelRoot.anchorMax = new Vector2(0f, 1f);
            _panelRoot.pivot = new Vector2(0f, 1f);
            _panelRoot.anchoredPosition = new Vector2(24f, -24f);
            _panelRoot.sizeDelta = new Vector2(420f, 0f);

            var panelImage = panelGo.AddComponent<Image>();
            panelImage.color = new Color(0.09f, 0.11f, 0.14f, 0.9f);

            var layout = panelGo.AddComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset(14, 14, 14, 14);
            layout.spacing = 8f;
            layout.childControlWidth = true;
            layout.childControlHeight = false;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;

            var fitter = panelGo.AddComponent<ContentSizeFitter>();
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            CreateLabel(panelGo.transform, "IVX Test Scenes", 24, FontStyle.Bold, Color.white);
            CreateLabel(panelGo.transform, $"Current: {activeScene}", 18, FontStyle.Normal, new Color(0.8f, 0.86f, 0.95f));

            if (!isHomeScene)
            {
                CreateButton(panelGo.transform, $"Home ({_homeSceneName})", () => LoadScene(_homeSceneName));
            }

            for (int i = 0; i < _sceneLinks.Count; i++)
            {
                SceneLink link = _sceneLinks[i];
                if (string.IsNullOrWhiteSpace(link.SceneName))
                {
                    continue;
                }

                if (string.Equals(link.SceneName, activeScene, StringComparison.Ordinal))
                {
                    continue;
                }

                if (!IsSceneAvailable(link.SceneName))
                {
                    continue;
                }

                if (isHomeScene && !link.ShowOnHome)
                {
                    continue;
                }

                if (!isHomeScene && !link.ShowOnFeatureScenes)
                {
                    continue;
                }

                string buttonLabel = string.IsNullOrWhiteSpace(link.Label)
                    ? link.SceneName
                    : $"{link.Label} ({link.SceneName})";

                string targetScene = link.SceneName;
                CreateButton(panelGo.transform, buttonLabel, () => LoadScene(targetScene));
            }
        }

        private void ApplyVisibility()
        {
            if (_canvasObject != null)
            {
                _canvasObject.SetActive(_overlayVisible);
            }
        }

        private GameObject CreateUIObject(string name, Transform parent)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.AddComponent<RectTransform>();
            return go;
        }

        private void CreateLabel(Transform parent, string text, int fontSize, FontStyle style, Color color)
        {
            var labelGo = CreateUIObject("Label", parent);
            var layoutElement = labelGo.AddComponent<LayoutElement>();
            layoutElement.preferredHeight = fontSize + 16f;

            var textComp = labelGo.AddComponent<Text>();
            textComp.font = _font;
            textComp.text = text;
            textComp.fontSize = fontSize;
            textComp.fontStyle = style;
            textComp.alignment = TextAnchor.MiddleLeft;
            textComp.color = color;
            textComp.raycastTarget = false;
        }

        private void CreateButton(Transform parent, string text, Action onClick)
        {
            var buttonGo = CreateUIObject(text + "_Button", parent);
            var image = buttonGo.AddComponent<Image>();
            image.color = new Color(0.17f, 0.36f, 0.66f, 0.95f);

            var layoutElement = buttonGo.AddComponent<LayoutElement>();
            layoutElement.preferredHeight = 54f;

            var button = buttonGo.AddComponent<Button>();
            button.targetGraphic = image;
            button.onClick.AddListener(() => onClick?.Invoke());

            var textGo = CreateUIObject("Text", buttonGo.transform);
            var textRect = textGo.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;

            var textComp = textGo.AddComponent<Text>();
            textComp.font = _font;
            textComp.text = text;
            textComp.fontSize = 20;
            textComp.fontStyle = FontStyle.Bold;
            textComp.alignment = TextAnchor.MiddleCenter;
            textComp.color = Color.white;
            textComp.raycastTarget = false;
        }

        private static bool IsToggleKeyPressed(KeyCode key)
        {
            try
            {
#if ENABLE_LEGACY_INPUT_MANAGER
                if (Input.GetKeyDown(key))
                {
                    return true;
                }
#endif
            }
            catch (InvalidOperationException)
            {
                // Active input backend does not support legacy Input APIs.
            }

            if (TryReadInputSystemKeyDown(key))
            {
                return true;
            }

            return false;
        }

        private static bool TryReadInputSystemKeyDown(KeyCode keyCode)
        {
            Type keyboardType = FindType("UnityEngine.InputSystem.Keyboard");
            Type keyEnumType = FindType("UnityEngine.InputSystem.Key");
            if (keyboardType == null || keyEnumType == null)
            {
                return false;
            }

            PropertyInfo currentKeyboardProp = keyboardType.GetProperty("current", BindingFlags.Public | BindingFlags.Static);
            if (currentKeyboardProp == null)
            {
                return false;
            }

            object keyboard = currentKeyboardProp.GetValue(null);
            if (keyboard == null)
            {
                return false;
            }

            object mappedKey;
            try
            {
                mappedKey = Enum.Parse(keyEnumType, keyCode.ToString(), ignoreCase: true);
            }
            catch
            {
                return false;
            }

            MethodInfo getItem = keyboardType.GetMethod("get_Item", new[] { keyEnumType });
            if (getItem == null)
            {
                return false;
            }

            object keyControl = getItem.Invoke(keyboard, new[] { mappedKey });
            if (keyControl == null)
            {
                return false;
            }

            PropertyInfo pressedProp = keyControl.GetType().GetProperty("wasPressedThisFrame", BindingFlags.Public | BindingFlags.Instance);
            if (pressedProp == null)
            {
                return false;
            }

            object value = pressedProp.GetValue(keyControl);
            return value is bool pressed && pressed;
        }

        private static Type FindType(string fullTypeName)
        {
            Type type = Type.GetType(fullTypeName);
            if (type != null)
            {
                return type;
            }

            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            for (int i = 0; i < assemblies.Length; i++)
            {
                type = assemblies[i].GetType(fullTypeName);
                if (type != null)
                {
                    return type;
                }
            }

            return null;
        }

        private static void LoadScene(string sceneName)
        {
            if (string.IsNullOrWhiteSpace(sceneName))
            {
                return;
            }

            if (Application.CanStreamedLevelBeLoaded(sceneName))
            {
                SceneManager.LoadScene(sceneName);
                return;
            }

            if (TryEditorLoadScene(sceneName))
            {
                return;
            }

            Debug.LogWarning($"[{nameof(IVXTestSceneNavigator)}] Scene '{sceneName}' is not available in Build Settings.");
        }

        private static bool IsSceneAvailable(string sceneName)
        {
            if (Application.CanStreamedLevelBeLoaded(sceneName))
            {
                return true;
            }

#if UNITY_EDITOR
            string testPath = $"Assets/Scenes/Tests/{sceneName}.unity";
            if (System.IO.File.Exists(testPath))
            {
                return true;
            }
#endif
            return false;
        }

        private static bool TryEditorLoadScene(string sceneName)
        {
#if UNITY_EDITOR
            string testPath = $"Assets/Scenes/Tests/{sceneName}.unity";
            if (!System.IO.File.Exists(testPath))
            {
                return false;
            }

            var current = SceneManager.GetActiveScene();
            if (current.isLoaded && current.isDirty)
            {
                EditorSceneManager.SaveScene(current);
            }

            EditorSceneManager.LoadSceneInPlayMode(testPath, new LoadSceneParameters(LoadSceneMode.Single));
            return true;
#else
            return false;
#endif
        }
    }
}
