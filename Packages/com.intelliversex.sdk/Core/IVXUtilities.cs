using System;
using System.Collections.Generic;
using UnityEngine;

namespace IntelliVerseX.Core
{
    /// <summary>
    /// Simple service locator for dependency injection.
    /// Provides centralized access to services throughout the SDK.
    /// Part of IntelliVerse.GameSDK.Core package.
    /// </summary>
    public static class IVXServiceLocator
    {
        private static Dictionary<Type, object> _services = new Dictionary<Type, object>();

        /// <summary>
        /// Registers a service instance
        /// </summary>
        public static void Register<T>(T service) where T : class
        {
            Type type = typeof(T);
            
            if (_services.ContainsKey(type))
            {
                Debug.LogWarning($"[IVXServiceLocator] Service {type.Name} already registered, replacing");
                _services[type] = service;
            }
            else
            {
                _services.Add(type, service);
                Debug.Log($"[IVXServiceLocator] Registered service: {type.Name}");
            }
        }

        /// <summary>
        /// Gets a registered service
        /// </summary>
        public static T Get<T>() where T : class
        {
            Type type = typeof(T);
            
            if (_services.TryGetValue(type, out object service))
            {
                return service as T;
            }

            Debug.LogError($"[IVXServiceLocator] Service {type.Name} not registered");
            return null;
        }

        /// <summary>
        /// Tries to get a service without logging errors
        /// </summary>
        public static bool TryGet<T>(out T service) where T : class
        {
            Type type = typeof(T);
            
            if (_services.TryGetValue(type, out object obj))
            {
                service = obj as T;
                return service != null;
            }

            service = null;
            return false;
        }

        /// <summary>
        /// Checks if a service is registered
        /// </summary>
        public static bool IsRegistered<T>() where T : class
        {
            return _services.ContainsKey(typeof(T));
        }

        /// <summary>
        /// Unregisters a service
        /// </summary>
        public static void Unregister<T>() where T : class
        {
            Type type = typeof(T);
            
            if (_services.Remove(type))
            {
                Debug.Log($"[IVXServiceLocator] Unregistered service: {type.Name}");
            }
        }

        /// <summary>
        /// Clears all registered services
        /// </summary>
        public static void Clear()
        {
            Debug.Log("[IVXServiceLocator] Clearing all services");
            _services.Clear();
        }
    }

    /// <summary>
    /// Generic object pool to reduce allocations and improve performance.
    /// </summary>
    public class IVXObjectPool<T> where T : new()
    {
        private Stack<T> _pool = new Stack<T>();
        private Func<T> _createFunc;
        private Action<T> _onGet;
        private Action<T> _onRelease;
        private int _maxSize;

        public IVXObjectPool(Func<T> createFunc = null, Action<T> onGet = null, Action<T> onRelease = null, int maxSize = 100)
        {
            _createFunc = createFunc ?? (() => new T());
            _onGet = onGet;
            _onRelease = onRelease;
            _maxSize = maxSize;
        }

        /// <summary>
        /// Gets an object from the pool or creates a new one
        /// </summary>
        public T Get()
        {
            T item;
            
            if (_pool.Count > 0)
            {
                item = _pool.Pop();
            }
            else
            {
                item = _createFunc();
            }

            _onGet?.Invoke(item);
            return item;
        }

        /// <summary>
        /// Returns an object to the pool
        /// </summary>
        public void Release(T item)
        {
            if (_pool.Count < _maxSize)
            {
                _onRelease?.Invoke(item);
                _pool.Push(item);
            }
        }

        /// <summary>
        /// Clears the pool
        /// </summary>
        public void Clear()
        {
            _pool.Clear();
        }

        public int Count => _pool.Count;
    }

    /// <summary>
    /// Coroutine runner for non-MonoBehaviour classes.
    /// Allows running coroutines from static or pure C# classes.
    /// </summary>
    public class IVXCoroutineRunner : MonoBehaviour
    {
        private static IVXCoroutineRunner _instance;

        public static IVXCoroutineRunner Instance
        {
            get
            {
                if (_instance == null)
                {
                    GameObject go = new GameObject("[IVXCoroutineRunner]");
                    _instance = go.AddComponent<IVXCoroutineRunner>();
                    DontDestroyOnLoad(go);
                }
                return _instance;
            }
        }

        /// <summary>
        /// Starts a coroutine from a static context
        /// </summary>
        public static Coroutine Run(System.Collections.IEnumerator routine)
        {
            return Instance.StartCoroutine(routine);
        }

        /// <summary>
        /// Stops a running coroutine
        /// </summary>
        public static void Stop(Coroutine routine)
        {
            if (routine != null && _instance != null)
            {
                _instance.StopCoroutine(routine);
            }
        }

        private void OnDestroy()
        {
            if (_instance == this)
            {
                _instance = null;
            }
        }
    }
}
