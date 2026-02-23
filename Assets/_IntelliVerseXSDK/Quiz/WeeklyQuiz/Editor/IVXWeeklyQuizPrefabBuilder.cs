// IVXWeeklyQuizPrefabBuilder.cs
// Editor tool for creating Weekly Quiz test scene and prefabs

using System.Reflection;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;

namespace IntelliVerseX.Quiz.WeeklyQuiz.Editor
{
    /// <summary>
    /// Editor tool for creating the Weekly Quiz test scene and prefabs.
    /// </summary>
    public static class IVXWeeklyQuizPrefabBuilder
    {
        #region Constants

        private const string SCENES_FOLDER = "Assets/Scenes/Tests";
        private const string PREFABS_FOLDER = "Assets/_IntelliVerseXSDK/Quiz/WeeklyQuiz/Prefabs";
        private const string SCENE_NAME = "IVX_WeeklyQuizTest.unity";

        private static readonly Color PrimaryButtonColor = new Color(0.3f, 0.5f, 0.9f, 1f);
        private static readonly Color SecondaryButtonColor = new Color(0.4f, 0.4f, 0.4f, 1f);
        private static readonly Color TextColor = new Color(0.95f, 0.95f, 0.95f, 1f);
        private static readonly Color BackgroundColor = new Color(0.12f, 0.12f, 0.14f, 0.95f);

        #endregion

        #region Menu Items

        [MenuItem("IntelliVerse-X SDK/Quiz/Create Weekly Quiz Test Scene", false, 300)]
        public static void CreateWeeklyQuizTestScene()
        {
            EnsureFolder(SCENES_FOLDER);
            EnsureFolder(PREFABS_FOLDER);

            var newScene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            var lightGO = GameObject.Find("Directional Light");
            if (lightGO != null) Object.DestroyImmediate(lightGO);

            SetupCamera();
            SetupEventSystem();
            CreateWeeklyQuizBootstrapInScene();
            CreateWeeklyQuizTestUI();

            string scenePath = $"{SCENES_FOLDER}/{SCENE_NAME}";
            EditorSceneManager.SaveScene(newScene, scenePath);
            AddSceneToBuildSettings(scenePath);

            Debug.Log($"[IVXWeeklyQuizPrefabBuilder] Created scene: {scenePath}");
        }

        [MenuItem("IntelliVerse-X SDK/Quiz/Rebuild Weekly Quiz Test UI", false, 301)]
        public static void RebuildWeeklyQuizTestUI()
        {
            var existingCanvas = GameObject.Find("IVX_WeeklyQuizCanvas");
            if (existingCanvas != null)
            {
                Object.DestroyImmediate(existingCanvas);
            }

            CreateWeeklyQuizTestUI();
            Debug.Log("[IVXWeeklyQuizPrefabBuilder] UI rebuilt");
        }

        [MenuItem("IntelliVerse-X SDK/Quiz/Save Weekly Quiz UI as Prefab", false, 302)]
        public static void SaveWeeklyQuizUIPrefab()
        {
            EnsureFolder(PREFABS_FOLDER);

            var canvas = GameObject.Find("IVX_WeeklyQuizCanvas");
            if (canvas == null)
            {
                Debug.LogError("[IVXWeeklyQuizPrefabBuilder] No IVX_WeeklyQuizCanvas found in scene");
                return;
            }

            string prefabPath = $"{PREFABS_FOLDER}/IVXWeeklyQuizTestUI.prefab";
            PrefabUtility.SaveAsPrefabAsset(canvas, prefabPath);
            Debug.Log($"[IVXWeeklyQuizPrefabBuilder] Saved UI prefab: {prefabPath}");
        }

        [MenuItem("IntelliVerse-X SDK/Quiz/Create All Weekly Quiz Prefabs", false, 303)]
        public static void CreateAllWeeklyQuizPrefabs()
        {
            EnsureFolder(PREFABS_FOLDER);

            CreateWeeklyQuizBootstrapPrefab();
            
            var existingCanvas = GameObject.Find("IVX_WeeklyQuizCanvas");
            if (existingCanvas == null)
            {
                CreateWeeklyQuizTestUI();
            }
            SaveWeeklyQuizUIPrefab();

            AssetDatabase.Refresh();
            Debug.Log("[IVXWeeklyQuizPrefabBuilder] All prefabs created");
        }

        #endregion

        #region Scene Setup

        private static void SetupCamera()
        {
            var cameraGO = new GameObject("Main Camera");
            cameraGO.tag = "MainCamera";
            var camera = cameraGO.AddComponent<Camera>();
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.1f, 0.1f, 0.12f);
            camera.orthographic = false;
            cameraGO.AddComponent<AudioListener>();
        }

        private static void SetupEventSystem()
        {
            var eventSystemGO = new GameObject("EventSystem");
            eventSystemGO.AddComponent<UnityEngine.EventSystems.EventSystem>();

            var inputSystemType = FindType("UnityEngine.InputSystem.UI.InputSystemUIInputModule");
            if (inputSystemType != null)
            {
                eventSystemGO.AddComponent(inputSystemType);
            }
            else
            {
                eventSystemGO.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
            }
        }

        private static void CreateWeeklyQuizBootstrapInScene()
        {
            var existing = GameObject.Find("IVX_WeeklyQuizBootstrap");
            if (existing != null) return;

            var bootstrapGO = new GameObject("IVX_WeeklyQuizBootstrap");

            var managerType = FindType("IntelliVerseX.Quiz.WeeklyQuiz.IVXWeeklyQuizManager");
            if (managerType != null)
            {
                bootstrapGO.AddComponent(managerType);
            }

            var controllerType = FindType("IntelliVerseX.Quiz.WeeklyQuiz.IVXWeeklyQuizTestController");
            if (controllerType != null)
            {
                bootstrapGO.AddComponent(controllerType);
            }

            CreateWeeklyQuizBootstrapPrefab();
        }

        private static void CreateWeeklyQuizBootstrapPrefab()
        {
            string prefabPath = $"{PREFABS_FOLDER}/IVXWeeklyQuizBootstrap.prefab";
            if (AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath) != null)
            {
                Debug.Log($"[IVXWeeklyQuizPrefabBuilder] Bootstrap prefab already exists: {prefabPath}");
                return;
            }

            var bootstrapGO = new GameObject("IVX_WeeklyQuizBootstrap");

            var managerType = FindType("IntelliVerseX.Quiz.WeeklyQuiz.IVXWeeklyQuizManager");
            if (managerType != null)
            {
                bootstrapGO.AddComponent(managerType);
            }

            var controllerType = FindType("IntelliVerseX.Quiz.WeeklyQuiz.IVXWeeklyQuizTestController");
            if (controllerType != null)
            {
                bootstrapGO.AddComponent(controllerType);
            }

            PrefabUtility.SaveAsPrefabAsset(bootstrapGO, prefabPath);
            Object.DestroyImmediate(bootstrapGO);
            Debug.Log($"[IVXWeeklyQuizPrefabBuilder] Created bootstrap prefab: {prefabPath}");
        }

        #endregion

        #region UI Creation

        private static void CreateWeeklyQuizTestUI()
        {
            var canvasGO = new GameObject("IVX_WeeklyQuizCanvas");
            var canvas = canvasGO.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 100;

            var scaler = canvasGO.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080, 1920);
            scaler.matchWidthOrHeight = 0.5f;

            canvasGO.AddComponent<GraphicRaycaster>();

            var containerGO = new GameObject("Container");
            containerGO.transform.SetParent(canvasGO.transform, false);

            var containerRect = containerGO.AddComponent<RectTransform>();
            containerRect.anchorMin = new Vector2(0.1f, 0.1f);
            containerRect.anchorMax = new Vector2(0.9f, 0.9f);
            containerRect.offsetMin = Vector2.zero;
            containerRect.offsetMax = Vector2.zero;

            var bgImage = containerGO.AddComponent<Image>();
            bgImage.color = BackgroundColor;

            var vertLayout = containerGO.AddComponent<VerticalLayoutGroup>();
            vertLayout.childAlignment = TextAnchor.UpperCenter;
            vertLayout.childControlWidth = true;
            vertLayout.childControlHeight = false;
            vertLayout.childForceExpandWidth = true;
            vertLayout.childForceExpandHeight = false;
            vertLayout.spacing = 15;
            vertLayout.padding = new RectOffset(30, 30, 40, 40);

            CreateText(containerGO.transform, "Title", "Weekly Quiz Test", 48, true);
            CreateText(containerGO.transform, "WeekInfoText", "Week: Loading...", 24, false);
            CreateText(containerGO.transform, "StatusText", "Ready - Select a quiz type", 28, false);

            CreateSpacer(containerGO.transform, 30);

            CreateButton(containerGO.transform, "FortuneButton", "🔮 Fortune Quiz", new Color(0.5f, 0.2f, 0.8f, 1f));
            CreateButton(containerGO.transform, "EmojiButton", "🎉 Emoji Quiz", new Color(0.9f, 0.7f, 0.1f, 1f));
            CreateButton(containerGO.transform, "PredictionButton", "⚽ Prediction Quiz", new Color(0.2f, 0.6f, 0.9f, 1f));
            CreateButton(containerGO.transform, "HealthButton", "💧 Health Quiz", new Color(0.2f, 0.8f, 0.6f, 1f));

            CreateSpacer(containerGO.transform, 30);

            var utilityContainer = new GameObject("UtilityButtons");
            utilityContainer.transform.SetParent(containerGO.transform, false);

            var utilRect = utilityContainer.AddComponent<RectTransform>();
            utilRect.sizeDelta = new Vector2(0, 60);

            var horizLayout = utilityContainer.AddComponent<HorizontalLayoutGroup>();
            horizLayout.childAlignment = TextAnchor.MiddleCenter;
            horizLayout.childControlWidth = true;
            horizLayout.childControlHeight = true;
            horizLayout.childForceExpandWidth = true;
            horizLayout.childForceExpandHeight = true;
            horizLayout.spacing = 20;

            var layoutElem = utilityContainer.AddComponent<LayoutElement>();
            layoutElem.preferredHeight = 60;

            CreateButton(utilityContainer.transform, "ResetButton", "Reset All", SecondaryButtonColor);
            CreateButton(utilityContainer.transform, "LogStatusButton", "Log Status", SecondaryButtonColor);

            Debug.Log("[IVXWeeklyQuizPrefabBuilder] Weekly Quiz Test UI created");
        }

        private static GameObject CreateText(Transform parent, string name, string text, int fontSize, bool bold, Color? color = null)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);

            var rect = go.AddComponent<RectTransform>();
            rect.sizeDelta = new Vector2(0, fontSize + 20);

            var tmpType = FindType("TMPro.TextMeshProUGUI");
            if (tmpType != null)
            {
                var tmp = go.AddComponent(tmpType);
                SetPropertyValue(tmp, "text", text);
                SetPropertyValue(tmp, "fontSize", (float)fontSize);
                SetPropertyValue(tmp, "fontStyle", bold ? 1 : 0);
                SetPropertyValue(tmp, "alignment", 514);
                SetPropertyValue(tmp, "color", color ?? TextColor);
                SetPropertyValue(tmp, "raycastTarget", false);
            }
            else
            {
                var legacyText = go.AddComponent<Text>();
                legacyText.text = text;
                legacyText.fontSize = fontSize;
                legacyText.fontStyle = bold ? FontStyle.Bold : FontStyle.Normal;
                legacyText.alignment = TextAnchor.MiddleCenter;
                legacyText.color = color ?? TextColor;
                legacyText.raycastTarget = false;
            }

            var le = go.AddComponent<LayoutElement>();
            le.preferredHeight = fontSize + 20;

            return go;
        }

        private static GameObject CreateButton(Transform parent, string name, string label, Color buttonColor)
        {
            var buttonGO = new GameObject(name);
            buttonGO.transform.SetParent(parent, false);

            var rect = buttonGO.AddComponent<RectTransform>();
            rect.sizeDelta = new Vector2(0, 70);

            var image = buttonGO.AddComponent<Image>();
            image.color = buttonColor;
            image.raycastTarget = true;

            var button = buttonGO.AddComponent<Button>();
            var colors = button.colors;
            colors.normalColor = buttonColor;
            colors.highlightedColor = buttonColor * 1.1f;
            colors.pressedColor = buttonColor * 0.9f;
            colors.selectedColor = buttonColor;
            button.colors = colors;

            var textGO = new GameObject("Text");
            textGO.transform.SetParent(buttonGO.transform, false);

            var textRect = textGO.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = new Vector2(10, 5);
            textRect.offsetMax = new Vector2(-10, -5);

            var tmpType = FindType("TMPro.TextMeshProUGUI");
            if (tmpType != null)
            {
                var tmp = textGO.AddComponent(tmpType);
                SetPropertyValue(tmp, "text", label);
                SetPropertyValue(tmp, "fontSize", 28f);
                SetPropertyValue(tmp, "fontStyle", 1);
                SetPropertyValue(tmp, "alignment", 514);
                SetPropertyValue(tmp, "color", TextColor);
                SetPropertyValue(tmp, "raycastTarget", false);
            }
            else
            {
                var legacyText = textGO.AddComponent<Text>();
                legacyText.text = label;
                legacyText.fontSize = 28;
                legacyText.fontStyle = FontStyle.Bold;
                legacyText.alignment = TextAnchor.MiddleCenter;
                legacyText.color = TextColor;
                legacyText.raycastTarget = false;
            }

            var le = buttonGO.AddComponent<LayoutElement>();
            le.preferredHeight = 70;

            return buttonGO;
        }

        private static void CreateSpacer(Transform parent, float height)
        {
            var spacer = new GameObject("Spacer");
            spacer.transform.SetParent(parent, false);

            var rect = spacer.AddComponent<RectTransform>();
            rect.sizeDelta = new Vector2(0, height);

            var le = spacer.AddComponent<LayoutElement>();
            le.preferredHeight = height;
            le.flexibleHeight = 0;
        }

        #endregion

        #region Helpers

        private static void EnsureFolder(string path)
        {
            if (!AssetDatabase.IsValidFolder(path))
            {
                string[] parts = path.Split('/');
                string current = parts[0];
                for (int i = 1; i < parts.Length; i++)
                {
                    string next = current + "/" + parts[i];
                    if (!AssetDatabase.IsValidFolder(next))
                    {
                        AssetDatabase.CreateFolder(current, parts[i]);
                    }
                    current = next;
                }
            }
        }

        private static void AddSceneToBuildSettings(string scenePath)
        {
            var scenes = new System.Collections.Generic.List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
            
            foreach (var s in scenes)
            {
                if (s.path == scenePath) return;
            }

            scenes.Add(new EditorBuildSettingsScene(scenePath, true));
            EditorBuildSettings.scenes = scenes.ToArray();
            Debug.Log($"[IVXWeeklyQuizPrefabBuilder] Added scene to build settings: {scenePath}");
        }

        private static System.Type FindType(string fullName)
        {
            foreach (var assembly in System.AppDomain.CurrentDomain.GetAssemblies())
            {
                var type = assembly.GetType(fullName);
                if (type != null) return type;
            }
            return null;
        }

        private static void SetPropertyValue(object component, string propertyName, object value)
        {
            var type = component.GetType();
            var prop = type.GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance);
            if (prop != null && prop.CanWrite)
            {
                prop.SetValue(component, value);
            }
        }

        #endregion
    }
}
