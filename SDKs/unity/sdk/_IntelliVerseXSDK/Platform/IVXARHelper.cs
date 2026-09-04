using System;
using UnityEngine;

namespace IntelliVerseX.Platform
{
    /// <summary>
    /// Wraps AR Foundation subsystems to expose AR session state, plane detection,
    /// image tracking, light estimation, and world mesh availability.
    /// Requires the INTELLIVERSEX_HAS_ARFOUNDATION scripting define when AR Foundation is installed.
    /// </summary>
    public sealed class IVXARHelper : MonoBehaviour
    {
        #region Constants
        private const string LOG_TAG = nameof(IVXARHelper);
        #endregion

        #region Events
        /// <summary>Fired when the AR session state changes.</summary>
        public event Action<string> OnARSessionStateChanged;
        /// <summary>Fired when a new plane is detected.</summary>
        public event Action OnPlaneDetected;
        /// <summary>Fired when a tracked image is recognized.</summary>
        public event Action<string> OnImageTracked;
        /// <summary>Fired when light estimation data is updated.</summary>
        public event Action<float> OnLightEstimationUpdated;
        #endregion

        #region Private Fields
        private string _arSessionState = "None";
        private int _planeCount;
        private bool _isWorldMeshAvailable;
#if INTELLIVERSEX_HAS_ARFOUNDATION
        private UnityEngine.XR.ARFoundation.ARSession _arSession;
        private UnityEngine.XR.ARFoundation.ARPlaneManager _planeManager;
        private UnityEngine.XR.ARFoundation.ARTrackedImageManager _imageManager;
        private UnityEngine.XR.ARFoundation.ARCameraManager _cameraManager;
#endif
        #endregion

        #region Properties
        /// <summary>Whether AR is available on the current device.</summary>
        public bool IsARAvailable { get; private set; }
        /// <summary>Current AR session state as a string.</summary>
        public string ARSessionState => _arSessionState;
        /// <summary>Number of detected planes.</summary>
        public int PlaneCount => _planeCount;
        /// <summary>Whether world mesh (LiDAR) scanning is available.</summary>
        public bool IsWorldMeshAvailable => _isWorldMeshAvailable;
        #endregion

        #region Singleton
        private static IVXARHelper _instance;
        public static IVXARHelper Instance => _instance;

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }
            _instance = this;
            DontDestroyOnLoad(gameObject);
            Initialize();
        }

        private void OnDestroy()
        {
            if (_instance == this) _instance = null;
            Cleanup();
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Force a refresh of AR availability and subsystem state.
        /// </summary>
        public void Refresh()
        {
#if INTELLIVERSEX_HAS_ARFOUNDATION
            CheckARAvailability();
            UpdatePlaneCount();
            CheckWorldMeshAvailability();
#endif
        }
        #endregion

        #region Private Methods
        private void Initialize()
        {
#if INTELLIVERSEX_HAS_ARFOUNDATION
            _arSession = FindAnyObjectByType<UnityEngine.XR.ARFoundation.ARSession>();
            _planeManager = FindAnyObjectByType<UnityEngine.XR.ARFoundation.ARPlaneManager>();
            _imageManager = FindAnyObjectByType<UnityEngine.XR.ARFoundation.ARTrackedImageManager>();
            _cameraManager = FindAnyObjectByType<UnityEngine.XR.ARFoundation.ARCameraManager>();

            UnityEngine.XR.ARFoundation.ARSession.stateChanged += HandleARSessionStateChanged;

            if (_planeManager != null)
            {
                _planeManager.trackablesChanged.AddListener(HandlePlanesChanged);
            }

            if (_imageManager != null)
            {
                _imageManager.trackablesChanged.AddListener(HandleImagesChanged);
            }

            if (_cameraManager != null)
            {
                _cameraManager.frameReceived += HandleCameraFrameReceived;
            }

            CheckARAvailability();
            CheckWorldMeshAvailability();
            Debug.Log($"[{LOG_TAG}] Initialized, AR available: {IsARAvailable}");
#else
            IsARAvailable = false;
            Debug.Log($"[{LOG_TAG}] AR Foundation not available (define INTELLIVERSEX_HAS_ARFOUNDATION to enable)");
#endif
        }

        private void Cleanup()
        {
#if INTELLIVERSEX_HAS_ARFOUNDATION
            UnityEngine.XR.ARFoundation.ARSession.stateChanged -= HandleARSessionStateChanged;

            if (_planeManager != null)
            {
                _planeManager.trackablesChanged.RemoveListener(HandlePlanesChanged);
            }

            if (_imageManager != null)
            {
                _imageManager.trackablesChanged.RemoveListener(HandleImagesChanged);
            }

            if (_cameraManager != null)
            {
                _cameraManager.frameReceived -= HandleCameraFrameReceived;
            }
#endif
        }

#if INTELLIVERSEX_HAS_ARFOUNDATION
        private void CheckARAvailability()
        {
            var state = UnityEngine.XR.ARFoundation.ARSession.state;
            IsARAvailable = state != UnityEngine.XR.ARFoundation.ARSessionState.Unsupported
                         && state != UnityEngine.XR.ARFoundation.ARSessionState.None;
        }

        private void UpdatePlaneCount()
        {
            if (_planeManager != null)
            {
                int count = 0;
                foreach (var plane in _planeManager.trackables)
                {
                    if (plane.trackingState == UnityEngine.XR.ARSubsystems.TrackingState.Tracking)
                        count++;
                }
                _planeCount = count;
            }
        }

        private void CheckWorldMeshAvailability()
        {
            var meshManager = FindAnyObjectByType<UnityEngine.XR.ARFoundation.ARMeshManager>();
            _isWorldMeshAvailable = meshManager != null && meshManager.enabled;
        }

        private void HandleARSessionStateChanged(UnityEngine.XR.ARFoundation.ARSessionStateChangedEventArgs args)
        {
            _arSessionState = args.state.ToString();
            CheckARAvailability();
            Debug.Log($"[{LOG_TAG}] AR session state: {_arSessionState}");
            OnARSessionStateChanged?.Invoke(_arSessionState);
        }

        private void HandlePlanesChanged(UnityEngine.XR.ARFoundation.ARTrackablesChangedEventArgs<UnityEngine.XR.ARFoundation.ARPlane> args)
        {
            UpdatePlaneCount();
            if (args.added.Count > 0)
            {
                OnPlaneDetected?.Invoke();
            }
        }

        private void HandleImagesChanged(UnityEngine.XR.ARFoundation.ARTrackablesChangedEventArgs<UnityEngine.XR.ARFoundation.ARTrackedImage> args)
        {
            foreach (var image in args.added)
            {
                string imageName = image.referenceImage.name ?? "unknown";
                Debug.Log($"[{LOG_TAG}] Image tracked: {imageName}");
                OnImageTracked?.Invoke(imageName);
            }
        }

        private void HandleCameraFrameReceived(UnityEngine.XR.ARFoundation.ARCameraFrameEventArgs args)
        {
            if (args.lightEstimation.averageBrightness.HasValue)
            {
                OnLightEstimationUpdated?.Invoke(args.lightEstimation.averageBrightness.Value);
            }
        }
#endif
        #endregion
    }
}
