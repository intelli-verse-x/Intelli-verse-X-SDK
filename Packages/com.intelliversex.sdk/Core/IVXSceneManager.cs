// IVXSceneManager.cs
// IntelliVerseX SDK Scene Manager Base Class
// Provides centralized scene loading with transitions, loading screens, and preloading

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace IntelliVerseX.Core
{
    /// <summary>
    /// Base class for game scene management
    /// Provides unified scene loading with loading screens, transitions, and error handling
    /// 
    /// Usage:
    /// 1. Create game-specific class: public class QuizVerseSceneManager : IVXSceneManager<QuizVerseSceneManager>
    /// 2. Override GetLoadingScreenPrefab() if you have a custom loading screen
    /// 3. Use LoadScene(name) instead of SceneManager.LoadScene(name)
    /// 4. Subscribe to OnSceneLoading/OnSceneLoaded events for transitions
    /// </summary>
    public abstract class IVXSceneManager<T> : MonoBehaviour where T : IVXSceneManager<T>
    {
        #region Singleton Pattern
        
        private static T _instance;
        public static T Instance
        {
            get
            {
                if (_instance == null)
                {
#if UNITY_2023_1_OR_NEWER
                    _instance = FindFirstObjectByType<T>();
#else
                    _instance = FindObjectOfType<T>();
#endif
                    if (_instance != null)
                    {
                        DontDestroyOnLoad(_instance.gameObject);
                    }
                }
                return _instance;
            }
        }
        
        #endregion
        
        #region Events
        
        /// <summary>
        /// Fired when a scene starts loading
        /// </summary>
        public event Action<string> OnSceneLoadingStarted;
        
        /// <summary>
        /// Fired during scene loading progress (0-1)
        /// </summary>
        public event Action<float> OnSceneLoadingProgress;
        
        /// <summary>
        /// Fired when a scene finishes loading
        /// </summary>
        public event Action<string> OnSceneLoaded;
        
        /// <summary>
        /// Fired when scene loading fails
        /// </summary>
        public event Action<string, string> OnSceneLoadFailed;
        
        #endregion
        
        #region Properties
        
        /// <summary>
        /// Currently active scene name
        /// </summary>
        public string CurrentSceneName => SceneManager.GetActiveScene().name;
        
        /// <summary>
        /// Is a scene currently loading?
        /// </summary>
        public bool IsLoading { get; private set; }
        
        /// <summary>
        /// Loading progress (0-1)
        /// </summary>
        public float LoadingProgress { get; private set; }
        
        /// <summary>
        /// Previous scene name (before current)
        /// </summary>
        public string PreviousSceneName { get; private set; }
        
        #endregion
        
        #region Configuration
        
        /// <summary>
        /// Minimum time to show loading screen (prevents flicker on fast loads)
        /// </summary>
        protected virtual float MinimumLoadingTime => 0.5f;
        
        /// <summary>
        /// Enable debug logging for scene transitions
        /// </summary>
        protected virtual bool EnableDebugLogs => true;
        
        /// <summary>
        /// Game-specific log prefix for debugging
        /// </summary>
        protected virtual string GetLogPrefix() => "[IVX-SCENE]";
        
        #endregion
        
        #region Unity Lifecycle
        
        protected virtual void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }
            
            _instance = this as T;
            DontDestroyOnLoad(gameObject);
            
            if (EnableDebugLogs)
            {
                Debug.Log($"{GetLogPrefix()} Scene Manager initialized");
            }
        }
        
        #endregion
        
        #region Public API - Scene Loading
        
        /// <summary>
        /// Load a scene by name with optional loading screen
        /// </summary>
        /// <param name="sceneName">Scene name to load</param>
        /// <param name="showLoadingScreen">Show loading screen during load</param>
        public virtual void LoadScene(string sceneName, bool showLoadingScreen = true)
        {
            if (IsLoading)
            {
                Debug.LogWarning($"{GetLogPrefix()} Scene load already in progress. Ignoring request for '{sceneName}'.");
                return;
            }
            
            StartCoroutine(LoadSceneAsync(sceneName, showLoadingScreen));
        }
        
        /// <summary>
        /// Load a scene by build index with optional loading screen
        /// </summary>
        public virtual void LoadScene(int sceneBuildIndex, bool showLoadingScreen = true)
        {
            if (sceneBuildIndex < 0 || sceneBuildIndex >= SceneManager.sceneCountInBuildSettings)
            {
                Debug.LogError($"{GetLogPrefix()} Invalid scene build index: {sceneBuildIndex}");
                OnSceneLoadFailed?.Invoke(sceneBuildIndex.ToString(), "Invalid build index");
                return;
            }
            
            LoadScene(GetSceneNameFromBuildIndex(sceneBuildIndex), showLoadingScreen);
        }
        
        /// <summary>
        /// Reload the current scene
        /// </summary>
        public virtual void ReloadCurrentScene(bool showLoadingScreen = true)
        {
            LoadScene(CurrentSceneName, showLoadingScreen);
        }
        
        /// <summary>
        /// Load the previous scene (if available)
        /// </summary>
        public virtual void LoadPreviousScene(bool showLoadingScreen = true)
        {
            if (string.IsNullOrEmpty(PreviousSceneName))
            {
                Debug.LogWarning($"{GetLogPrefix()} No previous scene to load.");
                return;
            }
            
            LoadScene(PreviousSceneName, showLoadingScreen);
        }
        
        #endregion
        
        #region Protected - Scene Loading Implementation
        
        /// <summary>
        /// Async scene loading coroutine with loading screen support
        /// </summary>
        protected virtual IEnumerator LoadSceneAsync(string sceneName, bool showLoadingScreen)
        {
            IsLoading = true;
            LoadingProgress = 0f;
            
            if (EnableDebugLogs)
            {
                Debug.Log($"{GetLogPrefix()} Loading scene: {sceneName}");
            }
            
            // Fire loading started event
            OnSceneLoadingStarted?.Invoke(sceneName);
            
            // Show loading screen if requested
            GameObject loadingScreen = null;
            if (showLoadingScreen)
            {
                loadingScreen = ShowLoadingScreen();
            }
            
            // Track time for minimum loading duration
            float startTime = Time.realtimeSinceStartup;
            
            // Start async scene load
            AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);
            if (asyncLoad == null)
            {
                Debug.LogError($"{GetLogPrefix()} Failed to start loading scene: {sceneName}");
                OnSceneLoadFailed?.Invoke(sceneName, "AsyncOperation is null");
                IsLoading = false;
                
                if (loadingScreen != null)
                {
                    HideLoadingScreen(loadingScreen);
                }
                
                yield break;
            }
            
            // Prevent scene activation until ready
            asyncLoad.allowSceneActivation = false;
            
            // Update progress during load
            while (!asyncLoad.isDone)
            {
                // AsyncOperation progress goes 0-0.9 during load, then 0.9-1.0 on activation
                float progress = Mathf.Clamp01(asyncLoad.progress / 0.9f);
                LoadingProgress = progress;
                OnSceneLoadingProgress?.Invoke(progress);
                
                // Scene is ready to activate when progress reaches 0.9
                if (asyncLoad.progress >= 0.9f)
                {
                    // Wait for minimum loading time
                    float elapsedTime = Time.realtimeSinceStartup - startTime;
                    if (elapsedTime < MinimumLoadingTime)
                    {
                        yield return new WaitForSecondsRealtime(MinimumLoadingTime - elapsedTime);
                    }
                    
                    // Activate the scene
                    asyncLoad.allowSceneActivation = true;
                }
                
                yield return null;
            }
            
            // Update previous scene
            PreviousSceneName = CurrentSceneName;
            
            // Scene loaded successfully
            LoadingProgress = 1f;
            OnSceneLoadingProgress?.Invoke(1f);
            
            if (EnableDebugLogs)
            {
                Debug.Log($"{GetLogPrefix()} Scene loaded: {sceneName}");
            }
            
            // Hide loading screen
            if (loadingScreen != null)
            {
                HideLoadingScreen(loadingScreen);
            }
            
            // Fire loaded event
            OnSceneLoaded?.Invoke(sceneName);
            
            IsLoading = false;
        }
        
        #endregion
        
        #region Protected - Loading Screen Management
        
        /// <summary>
        /// Show the loading screen
        /// Override to provide custom loading screen prefab
        /// </summary>
        protected virtual GameObject ShowLoadingScreen()
        {
            // Default: Create simple loading canvas
            GameObject loadingScreen = new GameObject("LoadingScreen");
            DontDestroyOnLoad(loadingScreen);
            
            Canvas canvas = loadingScreen.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 9999; // Render on top
            
            // Optional: Override GetLoadingScreenPrefab() to instantiate custom prefab
            GameObject customPrefab = GetLoadingScreenPrefab();
            if (customPrefab != null)
            {
                GameObject instance = Instantiate(customPrefab, canvas.transform);
                return loadingScreen;
            }
            
            return loadingScreen;
        }
        
        /// <summary>
        /// Hide/destroy the loading screen
        /// </summary>
        protected virtual void HideLoadingScreen(GameObject loadingScreen)
        {
            if (loadingScreen != null)
            {
                Destroy(loadingScreen);
            }
        }
        
        /// <summary>
        /// Override to provide a custom loading screen prefab
        /// Return null to use default simple loading screen
        /// </summary>
        protected virtual GameObject GetLoadingScreenPrefab()
        {
            return null; // Games can override to provide prefab
        }
        
        #endregion
        
        #region Helpers
        
        /// <summary>
        /// Get scene name from build index (hacky but works without loading)
        /// </summary>
        private string GetSceneNameFromBuildIndex(int buildIndex)
        {
            string path = SceneUtility.GetScenePathByBuildIndex(buildIndex);
            if (string.IsNullOrEmpty(path))
            {
                return $"Scene{buildIndex}";
            }
            
            string sceneName = System.IO.Path.GetFileNameWithoutExtension(path);
            return sceneName;
        }
        
        #endregion
        
        #region Debug Helpers
        
        [ContextMenu("Log Current Scene")]
        protected void DebugLogCurrentScene()
        {
            Debug.Log($"{GetLogPrefix()} Current Scene: {CurrentSceneName}");
            Debug.Log($"{GetLogPrefix()} Previous Scene: {PreviousSceneName}");
            Debug.Log($"{GetLogPrefix()} Is Loading: {IsLoading}");
        }
        
        [ContextMenu("Reload Current Scene")]
        protected void DebugReloadScene()
        {
            ReloadCurrentScene();
        }
        
        #endregion
    }
}
