using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;
#if UNITY_EDITOR
using UnityEditor.SceneManagement;
#endif

namespace IntelliVerseX.Samples.UIToolkit
{
    /// <summary>
    /// Base shell for UITK demo scenes: binds UIDocument, animated panel show/hide, status, home nav.
    /// </summary>
    [RequireComponent(typeof(UIDocument))]
    public abstract class IVXUITKDemoShell : MonoBehaviour
    {
        public const string HomeSceneName = "IVX_HomeScreen_UITK";

        [SerializeField] protected UIDocument document;
        [SerializeField] protected VisualTreeAsset visualTree;
        [SerializeField] protected StyleSheet theme;
        [SerializeField] protected string homeSceneName = HomeSceneName;
        [SerializeField] protected float panelAnimSeconds = 0.28f;

        protected VisualElement Root { get; private set; }
        protected Label StatusLabel { get; private set; }
        protected VisualElement LoadingOverlay { get; private set; }

        protected virtual void Awake()
        {
            if (document == null)
                document = GetComponent<UIDocument>();
        }

        protected virtual void OnEnable()
        {
            BindDocument();
            OnUIReady();
        }

        protected virtual void OnDisable()
        {
            OnUITeardown();
        }

        protected abstract void OnUIReady();

        protected virtual void OnUITeardown() { }

        protected void BindDocument()
        {
            if (document == null)
                document = GetComponent<UIDocument>();

            if (visualTree != null)
                document.visualTreeAsset = visualTree;

            Root = document.rootVisualElement;
            if (Root == null)
                return;

            if (theme != null && !Root.styleSheets.Contains(theme))
                Root.styleSheets.Add(theme);

            StatusLabel = Root.Q<Label>("statusLabel");
            LoadingOverlay = Root.Q<VisualElement>("loadingOverlay");
            var homeButton = Root.Q<Button>("homeButton");
            if (homeButton != null)
            {
                homeButton.clicked -= GoHome;
                homeButton.clicked += GoHome;
            }
        }

        protected T Q<T>(string name) where T : VisualElement => Root?.Q<T>(name);

        protected void SetStatus(string message, bool error = false)
        {
            if (StatusLabel == null)
                return;

            StatusLabel.text = message ?? string.Empty;
            StatusLabel.EnableInClassList("ivx-status--err", error);
            StatusLabel.EnableInClassList("ivx-status--ok", !error && !string.IsNullOrEmpty(message));
        }

        protected void SetBusy(bool busy)
        {
            LoadingOverlay?.EnableInClassList("ivx-loading--visible", busy);
            Root?.SetEnabled(!busy);
            if (LoadingOverlay != null)
                LoadingOverlay.pickingMode = busy ? PickingMode.Position : PickingMode.Ignore;
        }

        protected void ShowPanel(VisualElement panel, bool animate = true)
        {
            if (panel == null)
                return;

            panel.RemoveFromClassList("ivx-panel--hidden");
            if (!animate)
            {
                panel.RemoveFromClassList("ivx-panel--entering");
                return;
            }

            panel.AddToClassList("ivx-panel--entering");
            panel.schedule.Execute(() => panel.RemoveFromClassList("ivx-panel--entering")).StartingIn(16);
        }

        protected void HidePanel(VisualElement panel)
        {
            if (panel == null)
                return;

            panel.AddToClassList("ivx-panel--entering");
            panel.schedule.Execute(() =>
            {
                panel.AddToClassList("ivx-panel--hidden");
                panel.RemoveFromClassList("ivx-panel--entering");
            }).StartingIn((long)(panelAnimSeconds * 1000f));
        }

        protected void SwapPanels(VisualElement hide, VisualElement show)
        {
            if (hide != null && hide != show)
                HidePanel(hide);
            ShowPanel(show);
        }

        protected void GoHome()
        {
            LoadScene(string.IsNullOrWhiteSpace(homeSceneName) ? HomeSceneName : homeSceneName);
        }

        protected static void LoadScene(string sceneName)
        {
            if (string.IsNullOrWhiteSpace(sceneName))
                return;

            if (Application.CanStreamedLevelBeLoaded(sceneName))
            {
                SceneManager.LoadScene(sceneName);
                return;
            }

#if UNITY_EDITOR
            string[] candidates =
            {
                $"Assets/IntelliVerseX UITK Demo Scenes/{sceneName}.unity",
                $"Assets/Samples/IntelliVerseX SDK/UIToolkit/Scenes/{sceneName}.unity",
                $"Assets/Scenes/Tests/{sceneName}.unity"
            };

            for (int i = 0; i < candidates.Length; i++)
            {
                if (!System.IO.File.Exists(candidates[i]))
                    continue;

                var current = SceneManager.GetActiveScene();
                if (current.isLoaded && current.isDirty)
                    EditorSceneManager.SaveScene(current);

                EditorSceneManager.LoadSceneInPlayMode(candidates[i], new LoadSceneParameters(LoadSceneMode.Single));
                return;
            }
#endif
            Debug.LogWarning($"[{nameof(IVXUITKDemoShell)}] Scene '{sceneName}' is not available.");
        }

        protected IEnumerator StaggerEnter(VisualElement container, string childClass, float delaySeconds = 0.05f)
        {
            if (container == null)
                yield break;

            var children = container.Query(className: childClass).ToList();
            for (int i = 0; i < children.Count; i++)
            {
                var child = children[i];
                child.AddToClassList("ivx-nav-card--enter");
            }

            yield return null;

            for (int i = 0; i < children.Count; i++)
            {
                var child = children[i];
                // Force style flush so USS transition runs when the enter class is removed.
                child.MarkDirtyRepaint();
                child.RemoveFromClassList("ivx-nav-card--enter");
                yield return new WaitForSecondsRealtime(delaySeconds);
            }
        }

        protected void BindClick(Button button, Action action)
        {
            if (button == null || action == null)
                return;
            button.clicked += () => action();
        }
    }
}
