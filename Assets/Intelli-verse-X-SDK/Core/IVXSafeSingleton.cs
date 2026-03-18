using UnityEngine;

namespace IntelliVerseX.Core
{
    /// <summary>
    /// Thread-safe singleton base class for MonoBehaviour singletons.
    /// Provides consistent singleton pattern implementation across all SDK managers.
    /// Part of IntelliVerse.GameSDK.Core package.
    /// </summary>
    /// <typeparam name="T">The type of the singleton class</typeparam>
    public abstract class IVXSafeSingleton<T> : MonoBehaviour where T : MonoBehaviour
    {
        private static readonly object _lock = new object();
        private static T _instance;
        private static bool _isQuitting = false;

        /// <summary>
        /// Thread-safe singleton instance
        /// </summary>
        public static T Instance
        {
            get
            {
                if (_isQuitting)
                {
                    Debug.LogWarning($"[IVXSafeSingleton<{typeof(T).Name}>] Instance requested while quitting. Returning null.");
                    return null;
                }

                lock (_lock)
                {
                    if (_instance == null)
                    {
                        // Try to find existing instance (Unity 2022.3+)
                        #if UNITY_2022_3_OR_NEWER
                        _instance = FindFirstObjectByType<T>();
                        #else
                        _instance = FindObjectOfType<T>();
                        #endif

                        if (_instance == null)
                        {
                            // Create new instance
                            GameObject singleton = new GameObject($"[IVX_{typeof(T).Name}]");
                            _instance = singleton.AddComponent<T>();
                            
                            Debug.Log($"[IVXSafeSingleton<{typeof(T).Name}>] Created new singleton instance");
                        }
                    }

                    return _instance;
                }
            }
        }

        /// <summary>
        /// Checks if singleton instance exists without creating it
        /// </summary>
        public static bool HasInstance => _instance != null;

        /// <summary>
        /// Whether this singleton should persist across scenes.
        /// Override in derived class to change behavior (default: true).
        /// </summary>
        protected virtual bool ShouldPersist => true;

        /// <summary>
        /// Whether to allow multiple instances (for testing).
        /// Override in derived class to change behavior (default: false).
        /// </summary>
        protected virtual bool AllowMultipleInstances => false;

        protected virtual void Awake()
        {
            lock (_lock)
            {
                if (_instance != null && _instance != this)
                {
                    if (!AllowMultipleInstances)
                    {
                        Debug.LogWarning($"[IVXSafeSingleton<{typeof(T).Name}>] Duplicate instance detected. Destroying: {gameObject.name}");
                        Destroy(gameObject);
                        return;
                    }
                }

                _instance = this as T;

                if (ShouldPersist)
                {
                    DontDestroyOnLoad(gameObject);
                }

                OnInitialize();
            }
        }

        protected virtual void OnDestroy()
        {
            lock (_lock)
            {
                if (_instance == this)
                {
                    OnCleanup();
                    _instance = null;
                }
            }
        }

        protected virtual void OnApplicationQuit()
        {
            _isQuitting = true;
        }

        /// <summary>
        /// Called when singleton is initialized.
        /// Override this instead of Awake in derived classes.
        /// </summary>
        protected virtual void OnInitialize()
        {
            Debug.Log($"[IVXSafeSingleton<{typeof(T).Name}>] Initialized");
        }

        /// <summary>
        /// Called when singleton is being destroyed.
        /// Override this instead of OnDestroy in derived classes.
        /// </summary>
        protected virtual void OnCleanup()
        {
            Debug.Log($"[IVXSafeSingleton<{typeof(T).Name}>] Cleanup");
        }

        /// <summary>
        /// Manually destroy the singleton instance (for testing or cleanup)
        /// </summary>
        public static void DestroyInstance()
        {
            lock (_lock)
            {
                if (_instance != null)
                {
                    Destroy(_instance.gameObject);
                    _instance = null;
                }
            }
        }
    }
}
