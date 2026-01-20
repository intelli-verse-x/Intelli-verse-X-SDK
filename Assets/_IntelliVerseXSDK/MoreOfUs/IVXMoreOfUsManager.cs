// ============================================================================
// IVXMoreOfUsManager.cs - "More Of Us" App Catalog Manager
// ============================================================================
// IntelliVerseX SDK - Cross-Promotion Feature
// Handles fetching, caching, and managing app catalog data
// ============================================================================

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace IntelliVerseX.MoreOfUs
{
    /// <summary>
    /// Central manager for the "More Of Us" cross-promotion feature.
    /// Handles fetching app catalogs from S3, caching, and providing data to UI.
    /// </summary>
    public class IVXMoreOfUsManager : MonoBehaviour
    {
        #region Singleton

        private static IVXMoreOfUsManager _instance;
        private static bool _isApplicationQuitting = false;
        private static bool _isCreatingInstance = false;
        
        /// <summary>
        /// Checks if the instance exists without creating it.
        /// Use this to safely check before accessing Instance during cleanup.
        /// </summary>
        public static bool HasInstance => _instance != null;
        
        /// <summary>
        /// Singleton instance. Returns null if application is quitting.
        /// </summary>
        public static IVXMoreOfUsManager Instance
        {
            get
            {
                // Prevent creation during application quit or scene unload
                if (_isApplicationQuitting)
                    return null;
                    
                if (_instance == null && !_isCreatingInstance)
                {
                    _isCreatingInstance = true;
                    try
                    {
                        _instance = FindFirstObjectByType<IVXMoreOfUsManager>();
                        if (_instance == null)
                        {
                            var go = new GameObject("[IVX] MoreOfUsManager");
                            _instance = go.AddComponent<IVXMoreOfUsManager>();
                            DontDestroyOnLoad(go);
                        }
                    }
                    finally
                    {
                        _isCreatingInstance = false;
                    }
                }
                return _instance;
            }
        }

        #endregion

        #region Constants

        private const string LOG_PREFIX = "[IVXMoreOfUs]";
        private const string CACHE_FILE_NAME = "ivx_app_catalog_cache.json";
        private const int REQUEST_TIMEOUT_SECONDS = 30;
        private const int MAX_RETRIES = 3;

        #endregion

        #region Configuration

        [Header("Configuration")]
        [SerializeField] private IVXMoreOfUsConfig _config;

        /// <summary>
        /// Current configuration
        /// </summary>
        public IVXMoreOfUsConfig Config
        {
            get
            {
                if (_config == null)
                    _config = new IVXMoreOfUsConfig();
                return _config;
            }
            set => _config = value;
        }

        #endregion

        #region State

        private IVXMergedAppCatalog _cachedCatalog;
        private bool _isLoading;
        private DateTime _lastFetchTime;
        private readonly Dictionary<string, Texture2D> _iconCache = new Dictionary<string, Texture2D>();
        private readonly object _lockObject = new object();

        /// <summary>
        /// Is data currently being loaded?
        /// </summary>
        public bool IsLoading => _isLoading;

        /// <summary>
        /// Is cached data available?
        /// </summary>
        public bool HasCachedData => _cachedCatalog != null && _cachedCatalog.apps.Count > 0;

        /// <summary>
        /// Total number of apps available
        /// </summary>
        public int TotalAppCount => _cachedCatalog?.apps.Count ?? 0;

        #endregion

        #region Events

        /// <summary>
        /// Fired when catalog data is successfully loaded
        /// </summary>
        public event Action<IVXMergedAppCatalog> OnCatalogLoaded;

        /// <summary>
        /// Fired when loading fails
        /// </summary>
        public event Action<string> OnLoadFailed;

        /// <summary>
        /// Fired when an app icon is loaded
        /// </summary>
        public event Action<string, Texture2D> OnIconLoaded;

        /// <summary>
        /// Fired when user taps on an app card
        /// </summary>
        public event Action<IVXUnifiedAppInfo> OnAppSelected;

        #endregion

        #region Unity Lifecycle

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }
            
            _instance = this;
            DontDestroyOnLoad(gameObject);
            
            // Try to load from cache on startup
            LoadFromCache();
        }

        private void OnDestroy()
        {
            // Clear icon cache safely
            if (_iconCache != null)
            {
                foreach (var tex in _iconCache.Values)
                {
                    if (tex != null)
                    {
                        Destroy(tex);
                    }
                }
                _iconCache.Clear();
            }

            if (_instance == this)
            {
                _instance = null;
            }
        }

        private void OnApplicationQuit()
        {
            _isApplicationQuitting = true;
        }

        #endregion

        #region Public API

        /// <summary>
        /// Fetch the app catalog from remote sources.
        /// Will use cache if available and not expired.
        /// </summary>
        /// <param name="forceRefresh">Force refresh even if cache is valid</param>
        public void FetchCatalog(bool forceRefresh = false)
        {
            StartCoroutine(FetchCatalogCoroutine(forceRefresh));
        }

        /// <summary>
        /// Fetch catalog asynchronously
        /// </summary>
        public async Task<IVXMergedAppCatalog> FetchCatalogAsync(bool forceRefresh = false, CancellationToken ct = default)
        {
            if (_isLoading)
            {
                // Wait for current load to complete
                while (_isLoading && !ct.IsCancellationRequested)
                    await Task.Delay(100, ct);
                return _cachedCatalog;
            }

            if (!forceRefresh && IsCacheValid())
            {
                Log("Using cached catalog data");
                return _cachedCatalog;
            }

            _isLoading = true;
            
            try
            {
                var merged = new IVXMergedAppCatalog();

                // Fetch Android catalog
                var androidCatalog = await FetchPlatformCatalogAsync<IVXAndroidAppCatalog>(
                    Config.androidCatalogUrl, ct);
                if (androidCatalog?.apps != null)
                {
                    foreach (var app in androidCatalog.apps)
                        merged.apps.Add(app.ToUnified());
                    merged.dataVersion = androidCatalog.dataVersion;
                }

                // Fetch iOS catalog
                var iosCatalog = await FetchPlatformCatalogAsync<IVXiOSAppCatalog>(
                    Config.iosCatalogUrl, ct);
                if (iosCatalog?.apps != null)
                {
                    foreach (var app in iosCatalog.apps)
                        merged.apps.Add(app.ToUnified());
                }

                merged.fetchedAtUtc = DateTime.UtcNow;
                _cachedCatalog = merged;
                _lastFetchTime = DateTime.UtcNow;

                // Save to cache
                SaveToCache();

                Log($"Catalog loaded: {merged.apps.Count} apps total");
                OnCatalogLoaded?.Invoke(merged);

                return merged;
            }
            catch (Exception ex)
            {
                LogError($"Failed to fetch catalog: {ex.Message}");
                OnLoadFailed?.Invoke(ex.Message);
                return _cachedCatalog; // Return cached data if available
            }
            finally
            {
                _isLoading = false;
            }
        }

        /// <summary>
        /// Get apps for the current platform, excluding the running app
        /// </summary>
        public List<IVXUnifiedAppInfo> GetAppsForCurrentPlatform()
        {
            if (_cachedCatalog == null)
                return new List<IVXUnifiedAppInfo>();

            var apps = _cachedCatalog.GetAppsForCurrentPlatform();
            
            // Limit to max display count
            if (apps.Count > Config.maxAppsToDisplay)
                apps = apps.GetRange(0, Config.maxAppsToDisplay);

            return apps;
        }

        /// <summary>
        /// Get all apps excluding the running app
        /// </summary>
        public List<IVXUnifiedAppInfo> GetAllOtherApps()
        {
            if (_cachedCatalog == null)
                return new List<IVXUnifiedAppInfo>();

            return _cachedCatalog.GetOtherApps();
        }

        /// <summary>
        /// Load app icon asynchronously
        /// </summary>
        public void LoadAppIcon(IVXUnifiedAppInfo appInfo, Action<Texture2D> onComplete)
        {
            if (appInfo == null || string.IsNullOrEmpty(appInfo.appIconUrl))
            {
                onComplete?.Invoke(null);
                return;
            }

            // Check cache
            if (_iconCache.TryGetValue(appInfo.appIconUrl, out var cachedTex))
            {
                onComplete?.Invoke(cachedTex);
                return;
            }

            StartCoroutine(LoadIconCoroutine(appInfo.appIconUrl, onComplete));
        }

        /// <summary>
        /// Load app icon asynchronously with Task
        /// </summary>
        public async Task<Texture2D> LoadAppIconAsync(IVXUnifiedAppInfo appInfo, CancellationToken ct = default)
        {
            if (appInfo == null || string.IsNullOrEmpty(appInfo.appIconUrl))
                return null;

            // Check memory cache
            if (_iconCache.TryGetValue(appInfo.appIconUrl, out var cachedTex))
                return cachedTex;

            // Check if already loaded on app info
            if (appInfo.cachedIcon != null)
                return appInfo.cachedIcon;

            var tcs = new TaskCompletionSource<Texture2D>();
            
            LoadAppIcon(appInfo, tex =>
            {
                appInfo.cachedIcon = tex;
                appInfo.iconLoadAttempted = true;
                tcs.TrySetResult(tex);
            });

            return await tcs.Task;
        }

        /// <summary>
        /// Open store page for an app
        /// </summary>
        public void OpenStorePage(IVXUnifiedAppInfo appInfo)
        {
            if (appInfo == null || string.IsNullOrEmpty(appInfo.storeUrl))
            {
                LogWarning("Cannot open store page: invalid app info");
                return;
            }

            Log($"Opening store page: {appInfo.appName}");
            OnAppSelected?.Invoke(appInfo);
            Application.OpenURL(appInfo.storeUrl);
        }

        /// <summary>
        /// Clear all cached data
        /// </summary>
        public void ClearCache()
        {
            _cachedCatalog = null;
            _lastFetchTime = DateTime.MinValue;

            foreach (var tex in _iconCache.Values)
            {
                if (tex != null)
                    Destroy(tex);
            }
            _iconCache.Clear();

            // Delete cache file
            string cachePath = GetCachePath();
            if (File.Exists(cachePath))
            {
                try { File.Delete(cachePath); }
                catch (Exception ex) { LogWarning($"Failed to delete cache file: {ex.Message}"); }
            }

            Log("Cache cleared");
        }

        #endregion

        #region Private Methods

        private IEnumerator FetchCatalogCoroutine(bool forceRefresh)
        {
            if (_isLoading)
                yield break;

            if (!forceRefresh && IsCacheValid())
            {
                Log("Using cached catalog data");
                OnCatalogLoaded?.Invoke(_cachedCatalog);
                yield break;
            }

            _isLoading = true;
            var merged = new IVXMergedAppCatalog();
            string errorMessage = null;

            // Fetch Android catalog
            yield return FetchPlatformCatalog<IVXAndroidAppCatalog>(
                Config.androidCatalogUrl,
                catalog =>
                {
                    if (catalog?.apps != null)
                    {
                        foreach (var app in catalog.apps)
                            merged.apps.Add(app.ToUnified());
                        merged.dataVersion = catalog.dataVersion;
                    }
                },
                error => errorMessage = error);

            // Fetch iOS catalog
            yield return FetchPlatformCatalog<IVXiOSAppCatalog>(
                Config.iosCatalogUrl,
                catalog =>
                {
                    if (catalog?.apps != null)
                    {
                        foreach (var app in catalog.apps)
                            merged.apps.Add(app.ToUnified());
                    }
                },
                error =>
                {
                    if (string.IsNullOrEmpty(errorMessage))
                        errorMessage = error;
                });

            _isLoading = false;

            if (merged.apps.Count > 0)
            {
                merged.fetchedAtUtc = DateTime.UtcNow;
                _cachedCatalog = merged;
                _lastFetchTime = DateTime.UtcNow;
                SaveToCache();

                Log($"Catalog loaded: {merged.apps.Count} apps total");
                OnCatalogLoaded?.Invoke(merged);
            }
            else if (!string.IsNullOrEmpty(errorMessage))
            {
                LogError($"Failed to fetch catalog: {errorMessage}");
                OnLoadFailed?.Invoke(errorMessage);
            }
        }

        private IEnumerator FetchPlatformCatalog<T>(string url, Action<T> onSuccess, Action<string> onError) where T : class
        {
            for (int retry = 0; retry < MAX_RETRIES; retry++)
            {
                using (var request = UnityWebRequest.Get(url))
                {
                    request.timeout = REQUEST_TIMEOUT_SECONDS;

                    yield return request.SendWebRequest();

                    if (request.result == UnityWebRequest.Result.Success)
                    {
                        try
                        {
                            var data = JsonUtility.FromJson<T>(request.downloadHandler.text);
                            onSuccess?.Invoke(data);
                            yield break;
                        }
                        catch (Exception ex)
                        {
                            LogWarning($"Failed to parse JSON from {url}: {ex.Message}");
                        }
                    }
                    else
                    {
                        LogWarning($"Request failed ({retry + 1}/{MAX_RETRIES}): {url} - {request.error}");
                    }
                }

                // Wait before retry
                if (retry < MAX_RETRIES - 1)
                    yield return new WaitForSeconds(1f * (retry + 1));
            }

            onError?.Invoke($"Failed to fetch catalog after {MAX_RETRIES} attempts");
        }

        private async Task<T> FetchPlatformCatalogAsync<T>(string url, CancellationToken ct) where T : class
        {
            for (int retry = 0; retry < MAX_RETRIES; retry++)
            {
                ct.ThrowIfCancellationRequested();

                using (var request = UnityWebRequest.Get(url))
                {
                    request.timeout = REQUEST_TIMEOUT_SECONDS;
                    var operation = request.SendWebRequest();

                    while (!operation.isDone)
                    {
                        ct.ThrowIfCancellationRequested();
                        await Task.Yield();
                    }

                    if (request.result == UnityWebRequest.Result.Success)
                    {
                        try
                        {
                            return JsonUtility.FromJson<T>(request.downloadHandler.text);
                        }
                        catch (Exception ex)
                        {
                            LogWarning($"Failed to parse JSON from {url}: {ex.Message}");
                        }
                    }
                    else
                    {
                        LogWarning($"Request failed ({retry + 1}/{MAX_RETRIES}): {url} - {request.error}");
                    }
                }

                // Wait before retry
                if (retry < MAX_RETRIES - 1)
                    await Task.Delay(1000 * (retry + 1), ct);
            }

            return null;
        }

        private IEnumerator LoadIconCoroutine(string url, Action<Texture2D> onComplete)
        {
            // Check cache again (might have been loaded by another call)
            if (_iconCache.TryGetValue(url, out var cachedTex))
            {
                onComplete?.Invoke(cachedTex);
                yield break;
            }

            using (var request = UnityWebRequestTexture.GetTexture(url))
            {
                request.timeout = 15;
                yield return request.SendWebRequest();

                if (request.result == UnityWebRequest.Result.Success)
                {
                    var texture = DownloadHandlerTexture.GetContent(request);
                    
                    lock (_lockObject)
                    {
                        if (!_iconCache.ContainsKey(url))
                            _iconCache[url] = texture;
                    }

                    OnIconLoaded?.Invoke(url, texture);
                    onComplete?.Invoke(texture);
                }
                else
                {
                    LogWarning($"Failed to load icon: {url} - {request.error}");
                    onComplete?.Invoke(null);
                }
            }
        }

        private bool IsCacheValid()
        {
            if (_cachedCatalog == null || _cachedCatalog.apps.Count == 0)
                return false;

            var cacheAge = DateTime.UtcNow - _lastFetchTime;
            return cacheAge.TotalHours < Config.cacheDurationHours;
        }

        private void SaveToCache()
        {
            if (!Config.enableOfflineCache || _cachedCatalog == null)
                return;

            try
            {
                string json = JsonUtility.ToJson(_cachedCatalog, true);
                string cachePath = GetCachePath();
                File.WriteAllText(cachePath, json);
                Log("Catalog saved to cache");
            }
            catch (Exception ex)
            {
                LogWarning($"Failed to save cache: {ex.Message}");
            }
        }

        private void LoadFromCache()
        {
            if (!Config.enableOfflineCache)
                return;

            string cachePath = GetCachePath();
            if (!File.Exists(cachePath))
                return;

            try
            {
                string json = File.ReadAllText(cachePath);
                _cachedCatalog = JsonUtility.FromJson<IVXMergedAppCatalog>(json);
                _lastFetchTime = _cachedCatalog?.fetchedAtUtc ?? DateTime.MinValue;
                Log($"Loaded {_cachedCatalog?.apps.Count ?? 0} apps from cache");
            }
            catch (Exception ex)
            {
                LogWarning($"Failed to load cache: {ex.Message}");
            }
        }

        private string GetCachePath()
        {
            return Path.Combine(Application.persistentDataPath, CACHE_FILE_NAME);
        }

        #endregion

        #region Logging

        private static void Log(string message)
        {
            Debug.Log($"{LOG_PREFIX} {message}");
        }

        private static void LogWarning(string message)
        {
            Debug.LogWarning($"{LOG_PREFIX} {message}");
        }

        private static void LogError(string message)
        {
            Debug.LogError($"{LOG_PREFIX} {message}");
        }

        #endregion
    }
}
