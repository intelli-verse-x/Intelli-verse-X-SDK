// File: IVXUIResolutionHandler.cs
// Purpose: Responsive UI resolution handler for all SDK UI elements
// Prefix: IVX - Standard SDK code
// Version: 1.1.0

using UnityEngine;
using UnityEngine.UI;

namespace IntelliVerseX.UI
{
    /// <summary>
    /// Resolution handler for SDK UI elements.
    /// Ensures UI scales properly across different screen sizes and aspect ratios.
    /// 
    /// Usage:
    /// - Add to root Canvas
    /// - Configure reference resolution
    /// - Set safe area support as needed
    /// </summary>
    [RequireComponent(typeof(CanvasScaler))]
    public class IVXUIResolutionHandler : MonoBehaviour
    {
        #region Serialized Fields

        [Header("Resolution Settings")]
        [SerializeField] private Vector2 referenceResolution = new Vector2(1920, 1080);
        [SerializeField] [Range(0, 1)] private float matchWidthOrHeight = 0.5f;
        [SerializeField] private bool useConstantPixelSize = false;
        [SerializeField] private float scaleFactor = 1f;

        [Header("Safe Area")]
        [SerializeField] private bool applySafeArea = true;
        [SerializeField] private RectTransform safeAreaPanel;

        [Header("Orientation")]
        [SerializeField] private bool supportBothOrientations = true;
        [SerializeField] private Vector2 portraitReferenceResolution = new Vector2(1080, 1920);

        [Header("Debug")]
        [SerializeField] private bool showDebugInfo = false;

        #endregion

        #region Private Fields

        private CanvasScaler _canvasScaler;
        private Rect _lastSafeArea;
        private ScreenOrientation _lastOrientation;

        #endregion

        #region Unity Lifecycle

        private void Awake()
        {
            _canvasScaler = GetComponent<CanvasScaler>();
            SetupCanvasScaler();
        }

        private void Start()
        {
            ApplyCurrentSettings();
        }

        private void Update()
        {
            // Check for orientation or safe area changes
            if (applySafeArea && Screen.safeArea != _lastSafeArea)
            {
                ApplySafeArea();
            }

            if (supportBothOrientations && Screen.orientation != _lastOrientation)
            {
                OnOrientationChanged();
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Apply current resolution settings
        /// </summary>
        public void ApplyCurrentSettings()
        {
            SetupCanvasScaler();
            
            if (applySafeArea)
            {
                ApplySafeArea();
            }

            _lastOrientation = Screen.orientation;
        }

        /// <summary>
        /// Set reference resolution
        /// </summary>
        public void SetReferenceResolution(Vector2 resolution)
        {
            referenceResolution = resolution;
            SetupCanvasScaler();
        }

        /// <summary>
        /// Set width/height match ratio
        /// </summary>
        public void SetMatchRatio(float ratio)
        {
            matchWidthOrHeight = Mathf.Clamp01(ratio);
            if (_canvasScaler != null)
            {
                _canvasScaler.matchWidthOrHeight = matchWidthOrHeight;
            }
        }

        #endregion

        #region Private Methods

        private void SetupCanvasScaler()
        {
            if (_canvasScaler == null)
                _canvasScaler = GetComponent<CanvasScaler>();

            if (_canvasScaler == null)
                return;

            if (useConstantPixelSize)
            {
                _canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ConstantPixelSize;
                _canvasScaler.scaleFactor = scaleFactor;
            }
            else
            {
                _canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                
                // Use appropriate reference based on orientation
                if (supportBothOrientations && IsPortrait())
                {
                    _canvasScaler.referenceResolution = portraitReferenceResolution;
                }
                else
                {
                    _canvasScaler.referenceResolution = referenceResolution;
                }
                
                _canvasScaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
                _canvasScaler.matchWidthOrHeight = matchWidthOrHeight;
            }

            if (showDebugInfo)
            {
                Debug.Log($"[IVX-UI] Canvas configured: {_canvasScaler.referenceResolution}, " +
                          $"Match: {_canvasScaler.matchWidthOrHeight}");
            }
        }

        private void ApplySafeArea()
        {
            var safeArea = Screen.safeArea;
            _lastSafeArea = safeArea;

            if (safeAreaPanel == null)
            {
                // Try to find or create safe area panel
                var existingPanel = transform.Find("SafeAreaPanel");
                if (existingPanel != null)
                {
                    safeAreaPanel = existingPanel.GetComponent<RectTransform>();
                }
            }

            if (safeAreaPanel == null)
                return;

            var canvas = GetComponent<Canvas>();
            if (canvas == null)
                return;

            var canvasRect = canvas.GetComponent<RectTransform>();
            var canvasSize = canvasRect.rect.size;

            // Calculate safe area in canvas coordinates
            var anchorMin = safeArea.position;
            var anchorMax = safeArea.position + safeArea.size;

            anchorMin.x /= Screen.width;
            anchorMin.y /= Screen.height;
            anchorMax.x /= Screen.width;
            anchorMax.y /= Screen.height;

            safeAreaPanel.anchorMin = anchorMin;
            safeAreaPanel.anchorMax = anchorMax;
            safeAreaPanel.offsetMin = Vector2.zero;
            safeAreaPanel.offsetMax = Vector2.zero;

            if (showDebugInfo)
            {
                Debug.Log($"[IVX-UI] Safe area applied: {safeArea}");
            }
        }

        private void OnOrientationChanged()
        {
            _lastOrientation = Screen.orientation;
            SetupCanvasScaler();
            
            if (applySafeArea)
            {
                ApplySafeArea();
            }

            if (showDebugInfo)
            {
                Debug.Log($"[IVX-UI] Orientation changed: {_lastOrientation}");
            }
        }

        private bool IsPortrait()
        {
            return Screen.height > Screen.width ||
                   Screen.orientation == ScreenOrientation.Portrait ||
                   Screen.orientation == ScreenOrientation.PortraitUpsideDown;
        }

        #endregion

        #region Editor Helpers

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (Application.isPlaying)
            {
                ApplyCurrentSettings();
            }
        }

        [ContextMenu("Apply Settings Now")]
        private void ApplySettingsNow()
        {
            ApplyCurrentSettings();
        }

        [ContextMenu("Create Safe Area Panel")]
        private void CreateSafeAreaPanel()
        {
            if (safeAreaPanel != null)
                return;

            var panelGo = new GameObject("SafeAreaPanel");
            panelGo.transform.SetParent(transform, false);
            
            var rt = panelGo.AddComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;

            safeAreaPanel = rt;
            
            Debug.Log("[IVX-UI] Safe area panel created");
        }
#endif

        #endregion
    }
}
