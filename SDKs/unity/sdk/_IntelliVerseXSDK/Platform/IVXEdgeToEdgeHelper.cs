using UnityEngine;

namespace IntelliVerseX.Platform
{
    /// <summary>
    /// Helper for edge-to-edge / full-screen display on modern devices.
    /// Provides safe area insets so UI elements avoid notches, rounded corners,
    /// and system gesture bars.
    /// </summary>
    public static class IVXEdgeToEdgeHelper
    {
        #region Constants
        private const string LOG_TAG = "[IVX-EdgeToEdge]";
        #endregion

        #region Properties
        /// <summary>
        /// The device safe area in screen coordinates.
        /// </summary>
        public static Rect SafeArea => Screen.safeArea;

        /// <summary>
        /// Normalised safe area (0..1) for anchoring RectTransforms.
        /// </summary>
        public static Rect SafeAreaNormalized
        {
            get
            {
                var sa = Screen.safeArea;
                return new Rect(
                    sa.x / Screen.width,
                    sa.y / Screen.height,
                    sa.width / Screen.width,
                    sa.height / Screen.height);
            }
        }

        /// <summary>
        /// True when the current device has a notch or cutout that shrinks the safe area.
        /// </summary>
        public static bool HasNotch
        {
            get
            {
                var sa = Screen.safeArea;
                return sa.x > 0 || sa.y > 0
                       || sa.width < Screen.width
                       || sa.height < Screen.height;
            }
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Apply safe-area anchors to a RectTransform so it stays within the safe zone.
        /// Call once after layout, or on orientation change.
        /// </summary>
        public static void ApplySafeArea(RectTransform target)
        {
            if (target == null) return;

            var norm = SafeAreaNormalized;
            target.anchorMin = new Vector2(norm.x, norm.y);
            target.anchorMax = new Vector2(norm.x + norm.width, norm.y + norm.height);

            Debug.Log($"{LOG_TAG} Applied safe area: {norm}");
        }
        #endregion
    }
}
