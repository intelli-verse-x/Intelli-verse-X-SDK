// IVXShareService.cs
// IntelliVerseX SDK - Static facade for Share functionality
// Wraps IVXGShareManager with world-class ShareText, ShareScreenshot, ShareTextWithScreenshot

using System;

namespace IntelliVerseX.Social
{
    /// <summary>
    /// Static facade for cross-platform share: text, screenshot, or both.
    /// Production-ready for iOS and Android.
    /// </summary>
    public static class IVXShareService
    {
        /// <summary>
        /// Share plain text only.
        /// </summary>
        /// <param name="text">Text to share</param>
        /// <param name="url">Optional URL appended to text</param>
        /// <param name="callback">Optional completion callback (success)</param>
        public static void ShareText(string text, string url = null, Action<bool> callback = null)
        {
            var m = IntelliVerseX.Games.Social.IVXGShareManager.Instance;
            m?.ShareText(text ?? string.Empty, url, callback);
        }

        /// <summary>
        /// Share screenshot only (captures current screen).
        /// </summary>
        /// <param name="callback">Optional completion callback (success)</param>
        public static void ShareScreenshot(Action<bool> callback = null)
        {
            ShareTextWithScreenshot(null, null, callback);
        }

        /// <summary>
        /// Share text + screenshot together (captures current screen).
        /// </summary>
        /// <param name="text">Text to share (can be null for screenshot only)</param>
        /// <param name="url">Optional URL appended to text</param>
        /// <param name="callback">Optional completion callback (success)</param>
        public static void ShareTextWithScreenshot(string text, string url = null, Action<bool> callback = null)
        {
            var m = IntelliVerseX.Games.Social.IVXGShareManager.Instance;
            m?.ShareWithScreenshot(text ?? string.Empty, url, callback);
        }

        /// <summary>
        /// Whether the share manager is available.
        /// </summary>
        public static bool IsAvailable => IntelliVerseX.Games.Social.IVXGShareManager.HasInstance;
    }
}
