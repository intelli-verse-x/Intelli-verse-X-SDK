// IVXNativeShareHelper.cs
// IntelliVerseX SDK - Native Share Helper
// Works with or without NativeShare plugin

using UnityEngine;

namespace IntelliVerseX.Social
{
    /// <summary>
    /// Static helper for native share functionality.
    /// Works with NativeShare plugin if installed, falls back to clipboard otherwise.
    /// </summary>
    public static class IVXNativeShareHelper
    {
        /// <summary>
        /// Check if native sharing is available on this platform.
        /// </summary>
        public static bool IsNativeShareAvailable
        {
            get
            {
#if UNITY_ANDROID || UNITY_IOS
    #if NATIVE_SHARE_INSTALLED
                return true;
    #else
                return false;
    #endif
#else
                return false;
#endif
            }
        }

        /// <summary>
        /// Share text and URL using native share dialog (iOS/Android).
        /// Falls back to clipboard on other platforms or if NativeShare is not installed.
        /// </summary>
        public static void ShareReferralCode(string referralCode, string referralUrl, string customMessage = null)
        {
            string shareMessage = customMessage ?? $"Join me on IntelliVerseX! Use my referral code: {referralCode}\n{referralUrl}";

#if (UNITY_ANDROID || UNITY_IOS) && NATIVE_SHARE_INSTALLED
            if (Application.platform == RuntimePlatform.Android || Application.platform == RuntimePlatform.IPhonePlayer)
            {
                try
                {
                    new NativeShare()
                        .SetSubject("Join IntelliVerseX!")
                        .SetText(shareMessage)
                        .SetUrl(referralUrl)
                        .Share();
                    
                    Debug.Log("[IVXNativeShare] Native share dialog opened");
                    return;
                }
                catch (System.Exception ex)
                {
                    Debug.LogError($"[IVXNativeShare] Native share failed: {ex.Message}");
                }
            }
#endif
            
            // Fallback: Copy to clipboard
            CopyToClipboard(shareMessage);
            Debug.Log("[IVXNativeShare] Referral link copied to clipboard (Native Share not available)");
        }

        /// <summary>
        /// Share generic content using native share dialog.
        /// </summary>
        public static void Share(string subject, string text, string url = null)
        {
#if (UNITY_ANDROID || UNITY_IOS) && NATIVE_SHARE_INSTALLED
            if (Application.platform == RuntimePlatform.Android || Application.platform == RuntimePlatform.IPhonePlayer)
            {
                try
                {
                    var share = new NativeShare()
                        .SetSubject(subject)
                        .SetText(text);
                    
                    if (!string.IsNullOrEmpty(url))
                        share.SetUrl(url);
                    
                    share.Share();
                    Debug.Log("[IVXNativeShare] Native share dialog opened");
                    return;
                }
                catch (System.Exception ex)
                {
                    Debug.LogError($"[IVXNativeShare] Native share failed: {ex.Message}");
                }
            }
#endif
            
            // Fallback: Copy to clipboard
            string content = string.IsNullOrEmpty(url) ? text : $"{text}\n{url}";
            CopyToClipboard(content);
        }

        /// <summary>
        /// Copy text to clipboard.
        /// </summary>
        public static void CopyToClipboard(string text)
        {
            GUIUtility.systemCopyBuffer = text;
            Debug.Log($"[IVXNativeShare] Copied to clipboard: {text}");
        }
    }
}
