// IVXGShareManager.cs
// Cross-platform native sharing for IntelliVerseX Games SDK
// Supports iOS, Android with fallback to Application.OpenURL

using System;
using System.Collections;
using System.IO;
using UnityEngine;

namespace IntelliVerseX.Games.Social
{
    /// <summary>
    /// IVXGShareManager - Production-Ready Native Share Manager
    /// --------------------------------------------------------
    /// 
    /// Features:
    /// - Native share sheet on iOS and Android
    /// - Screenshot capture and sharing
    /// - Text, URL, and image sharing
    /// - Fallback handling for unsupported platforms
    /// - Event callbacks for share completion
    /// </summary>
    public class IVXGShareManager : MonoBehaviour
    {
        #region Singleton

        private static IVXGShareManager _instance;
        private static readonly object _lock = new object();
        private static bool _isQuitting;

        public static IVXGShareManager Instance
        {
            get
            {
                if (_isQuitting) return null;

                lock (_lock)
                {
                    if (_instance == null)
                    {
#if UNITY_2023_1_OR_NEWER
                        _instance = FindFirstObjectByType<IVXGShareManager>();
#else
                        _instance = FindObjectOfType<IVXGShareManager>();
#endif
                        if (_instance == null)
                        {
                            var go = new GameObject("[IVXGShareManager]");
                            _instance = go.AddComponent<IVXGShareManager>();
                            DontDestroyOnLoad(go);
                        }
                    }
                }
                return _instance;
            }
        }

        public static bool HasInstance => _instance != null;

        #endregion

        #region Config

        [Header("Share Configuration")]
        [SerializeField] private string defaultShareUrl = "https://play.google.com/store/apps/details?id=";
        [SerializeField] private string appStoreUrl = "https://apps.apple.com/app/id";
        [SerializeField] private string playStoreUrl = "https://play.google.com/store/apps/details?id=";

        [Header("App Info")]
        [SerializeField] private string appName = "My Game";
        [SerializeField] private string iosAppId = "";
        [SerializeField] private string androidPackageName = "";

        [Header("Settings")]
        [SerializeField] private int maxImageSize = 1920;
        [SerializeField] private bool logDebug = true;

        #endregion

        #region Events

        /// <summary>Called when share completes (success, cancelled, or failed)</summary>
        public event Action<bool> OnShareCompleted;

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

            // Auto-detect package name
            if (string.IsNullOrEmpty(androidPackageName))
            {
                androidPackageName = Application.identifier;
            }
        }

        private void OnApplicationQuit()
        {
            _isQuitting = true;
        }

        #endregion

        #region Public API - Share Methods

        /// <summary>
        /// Share text with optional URL
        /// </summary>
        public void ShareText(string text, string url = null, Action<bool> callback = null)
        {
            StartCoroutine(ShareTextRoutine(text, url, callback));
        }

        /// <summary>
        /// Share score with screenshot
        /// </summary>
        public void ShareScore(string gameName, int score, Texture2D screenshot = null, Action<bool> callback = null)
        {
            string text = $"I scored {score} in {gameName}! Can you beat me? 🎮";
            string url = GetStoreUrl();

            StartCoroutine(ShareWithScreenshotRoutine(text, url, screenshot, callback));
        }

        /// <summary>
        /// Share achievement
        /// </summary>
        public void ShareAchievement(string achievementName, string description, Texture2D screenshot = null, Action<bool> callback = null)
        {
            string text = $"🏆 I unlocked '{achievementName}' in {appName}!\n{description}";
            string url = GetStoreUrl();

            StartCoroutine(ShareWithScreenshotRoutine(text, url, screenshot, callback));
        }

        /// <summary>
        /// Share referral/invite
        /// </summary>
        public void ShareReferral(string referralCode, string customMessage = null, Action<bool> callback = null)
        {
            string text = customMessage ?? $"Join me in {appName}! Use my code: {referralCode} for bonus rewards! 🎁";
            string url = GetStoreUrl();

            StartCoroutine(ShareTextRoutine(text, url, callback));
        }

        /// <summary>
        /// Share with custom screenshot capture
        /// </summary>
        public void ShareWithScreenshot(string text, string url = null, Action<bool> callback = null)
        {
            StartCoroutine(CaptureAndShareRoutine(text, url, callback));
        }

        /// <summary>
        /// Share image directly
        /// </summary>
        public void ShareImage(Texture2D image, string text = null, Action<bool> callback = null)
        {
            StartCoroutine(ShareImageRoutine(image, text, callback));
        }

        #endregion

        #region Coroutines

        private IEnumerator ShareTextRoutine(string text, string url, Action<bool> callback)
        {
            bool success = false;

            try
            {
                string fullText = string.IsNullOrEmpty(url) ? text : $"{text}\n\n{url}";
                success = NativeShare(fullText, null);
            }
            catch (Exception ex)
            {
                Log($"ShareText error: {ex.Message}", true);
            }

            yield return null;

            callback?.Invoke(success);
            OnShareCompleted?.Invoke(success);
        }

        private IEnumerator ShareWithScreenshotRoutine(string text, string url, Texture2D screenshot, Action<bool> callback)
        {
            Texture2D captured = null;

            if (screenshot == null)
            {
                yield return new WaitForEndOfFrame();
                captured = CaptureScreenshot();
                screenshot = captured;
            }

            bool success = false;

            try
            {
                string fullText = string.IsNullOrEmpty(url) ? text : $"{text}\n\n{url}";
                success = NativeShare(fullText, screenshot);
            }
            catch (Exception ex)
            {
                Log($"ShareWithScreenshot error: {ex.Message}", true);
            }

            // Cleanup captured screenshot
            if (captured != null)
            {
                Destroy(captured);
            }

            yield return null;

            callback?.Invoke(success);
            OnShareCompleted?.Invoke(success);
        }

        private IEnumerator CaptureAndShareRoutine(string text, string url, Action<bool> callback)
        {
            yield return new WaitForEndOfFrame();

            var screenshot = CaptureScreenshot();
            bool success = false;

            try
            {
                string fullText = string.IsNullOrEmpty(url) ? text : $"{text}\n\n{url}";
                success = NativeShare(fullText, screenshot);
            }
            catch (Exception ex)
            {
                Log($"CaptureAndShare error: {ex.Message}", true);
            }

            if (screenshot != null)
            {
                Destroy(screenshot);
            }

            yield return null;

            callback?.Invoke(success);
            OnShareCompleted?.Invoke(success);
        }

        private IEnumerator ShareImageRoutine(Texture2D image, string text, Action<bool> callback)
        {
            bool success = false;

            try
            {
                success = NativeShare(text ?? "", image);
            }
            catch (Exception ex)
            {
                Log($"ShareImage error: {ex.Message}", true);
            }

            yield return null;

            callback?.Invoke(success);
            OnShareCompleted?.Invoke(success);
        }

        #endregion

        #region Native Share Implementation

        private bool NativeShare(string text, Texture2D image)
        {
#if UNITY_EDITOR
            Log("Share called in Editor - simulating success");
            return true;
#elif UNITY_ANDROID
            return AndroidNativeShare(text, image);
#elif UNITY_IOS
            return IOSNativeShare(text, image);
#else
            // Fallback: open URL
            var url = GetStoreUrl();
            if (!string.IsNullOrEmpty(url))
            {
                Application.OpenURL(url);
                return true;
            }
            return false;
#endif
        }

#if UNITY_ANDROID
        private bool AndroidNativeShare(string text, Texture2D image)
        {
            try
            {
                // Try NativeShare plugin first
                var nativeShareType = FindType("NativeShare");
                if (nativeShareType != null)
                {
                    var instance = Activator.CreateInstance(nativeShareType);
                    var setTextMethod = nativeShareType.GetMethod("SetText");
                    var shareMethod = nativeShareType.GetMethod("Share");

                    setTextMethod?.Invoke(instance, new object[] { text });

                    if (image != null)
                    {
                        var addFileMethod = nativeShareType.GetMethod("AddFile", new Type[] { typeof(Texture2D), typeof(string) });
                        addFileMethod?.Invoke(instance, new object[] { image, "share.png" });
                    }

                    shareMethod?.Invoke(instance, null);
                    return true;
                }

                // Fallback: Android Intent
                using (var intentClass = new AndroidJavaClass("android.content.Intent"))
                using (var intentObject = new AndroidJavaObject("android.content.Intent"))
                {
                    intentObject.Call<AndroidJavaObject>("setAction", intentClass.GetStatic<string>("ACTION_SEND"));
                    intentObject.Call<AndroidJavaObject>("setType", image != null ? "image/*" : "text/plain");
                    intentObject.Call<AndroidJavaObject>("putExtra", intentClass.GetStatic<string>("EXTRA_TEXT"), text);

                    if (image != null)
                    {
                        string imagePath = SaveImageToCache(image);
                        if (!string.IsNullOrEmpty(imagePath))
                        {
                            using (var uri = GetContentUri(imagePath))
                            {
                                if (uri != null)
                                {
                                    intentObject.Call<AndroidJavaObject>("putExtra", intentClass.GetStatic<string>("EXTRA_STREAM"), uri);
                                    intentObject.Call<AndroidJavaObject>("addFlags", 1); // FLAG_GRANT_READ_URI_PERMISSION
                                }
                            }
                        }
                    }

                    using (var chooserIntent = intentClass.CallStatic<AndroidJavaObject>("createChooser", intentObject, "Share via"))
                    using (var unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
                    using (var activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity"))
                    {
                        activity.Call("startActivity", chooserIntent);
                    }

                    return true;
                }
            }
            catch (Exception ex)
            {
                Log($"Android share error: {ex.Message}", true);
                return false;
            }
        }

        private string SaveImageToCache(Texture2D image)
        {
            try
            {
                byte[] bytes = image.EncodeToPNG();
                string path = Path.Combine(Application.temporaryCachePath, "share_image.png");
                File.WriteAllBytes(path, bytes);
                return path;
            }
            catch
            {
                return null;
            }
        }

        private AndroidJavaObject GetContentUri(string filePath)
        {
            try
            {
                using (var file = new AndroidJavaObject("java.io.File", filePath))
                using (var unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
                using (var activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity"))
                using (var fileProvider = new AndroidJavaClass("androidx.core.content.FileProvider"))
                {
                    string authority = Application.identifier + ".fileprovider";
                    return fileProvider.CallStatic<AndroidJavaObject>("getUriForFile", activity, authority, file);
                }
            }
            catch
            {
                // Fallback without FileProvider
                using (var uri = new AndroidJavaClass("android.net.Uri"))
                using (var file = new AndroidJavaObject("java.io.File", filePath))
                {
                    return uri.CallStatic<AndroidJavaObject>("fromFile", file);
                }
            }
        }
#endif

#if UNITY_IOS
        private bool IOSNativeShare(string text, Texture2D image)
        {
            try
            {
                // Try NativeShare plugin first
                var nativeShareType = FindType("NativeShare");
                if (nativeShareType != null)
                {
                    var instance = Activator.CreateInstance(nativeShareType);
                    var setTextMethod = nativeShareType.GetMethod("SetText");
                    var shareMethod = nativeShareType.GetMethod("Share");

                    setTextMethod?.Invoke(instance, new object[] { text });

                    if (image != null)
                    {
                        var addFileMethod = nativeShareType.GetMethod("AddFile", new Type[] { typeof(Texture2D), typeof(string) });
                        addFileMethod?.Invoke(instance, new object[] { image, "share.png" });
                    }

                    shareMethod?.Invoke(instance, null);
                    return true;
                }

                Log("NativeShare plugin not found on iOS", true);
                return false;
            }
            catch (Exception ex)
            {
                Log($"iOS share error: {ex.Message}", true);
                return false;
            }
        }
#endif

        #endregion

        #region Helpers

        private Texture2D CaptureScreenshot()
        {
            try
            {
                var texture = ScreenCapture.CaptureScreenshotAsTexture();

                // Resize if needed
                if (texture.width > maxImageSize || texture.height > maxImageSize)
                {
                    float scale = Mathf.Min((float)maxImageSize / texture.width, (float)maxImageSize / texture.height);
                    int newWidth = Mathf.RoundToInt(texture.width * scale);
                    int newHeight = Mathf.RoundToInt(texture.height * scale);

                    var resized = new Texture2D(newWidth, newHeight, TextureFormat.RGB24, false);
                    var rt = RenderTexture.GetTemporary(newWidth, newHeight);

                    Graphics.Blit(texture, rt);
                    RenderTexture.active = rt;
                    resized.ReadPixels(new Rect(0, 0, newWidth, newHeight), 0, 0);
                    resized.Apply();

                    RenderTexture.active = null;
                    RenderTexture.ReleaseTemporary(rt);
                    Destroy(texture);

                    return resized;
                }

                return texture;
            }
            catch (Exception ex)
            {
                Log($"Screenshot capture error: {ex.Message}", true);
                return null;
            }
        }

        private string GetStoreUrl()
        {
#if UNITY_IOS
            return string.IsNullOrEmpty(iosAppId) ? defaultShareUrl : $"{appStoreUrl}{iosAppId}";
#else
            return string.IsNullOrEmpty(androidPackageName) ? defaultShareUrl : $"{playStoreUrl}{androidPackageName}";
#endif
        }

        private Type FindType(string typeName)
        {
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                try
                {
                    var type = assembly.GetType(typeName, false);
                    if (type != null) return type;
                }
                catch { }
            }
            return null;
        }

        private void Log(string message, bool isError = false)
        {
            if (!logDebug && !isError) return;

            if (isError)
                Debug.LogError($"[IVXGShareManager] {message}");
            else
                Debug.Log($"[IVXGShareManager] {message}");
        }

        #endregion

        #region Configuration

        /// <summary>
        /// Set app information for share URLs
        /// </summary>
        public void SetAppInfo(string name, string iosId, string androidPackage)
        {
            appName = name;
            iosAppId = iosId;
            androidPackageName = androidPackage;
        }

        #endregion
    }
}
