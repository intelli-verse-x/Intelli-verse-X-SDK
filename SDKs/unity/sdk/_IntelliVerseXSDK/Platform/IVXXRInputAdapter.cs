using System;
using UnityEngine;

namespace IntelliVerseX.Platform
{
    #region Enums
    /// <summary>Identifies left or right hand/controller.</summary>
    public enum Hand
    {
        Left,
        Right
    }
    #endregion

    #region Interface
    /// <summary>
    /// Abstraction layer for XR input across vendor SDKs.
    /// Implement this interface to bridge a vendor SDK into IntelliVerseX input events.
    /// </summary>
    public interface IIVXXRInputProvider
    {
        /// <summary>Get the tracked hand pose, or null if unavailable.</summary>
        Pose? GetHandPose(Hand hand);
        /// <summary>Get the tracked controller pose, or null if unavailable.</summary>
        Pose? GetControllerPose(Hand hand);
        /// <summary>Get the gaze direction ray, or null if eye tracking is unavailable.</summary>
        Ray? GetGazeDirection();
        /// <summary>Whether the user is grabbing with the specified hand.</summary>
        bool IsGrabbing(Hand hand);
        /// <summary>Whether the user is pinching with the specified hand.</summary>
        bool IsPinching(Hand hand);
        /// <summary>Trigger axis value (0-1) for the specified hand.</summary>
        float GetTriggerValue(Hand hand);
    }
    #endregion

    /// <summary>
    /// Singleton adapter that delegates XR input queries to the active <see cref="IIVXXRInputProvider"/>.
    /// Auto-detects Meta XR or OpenXR providers based on <see cref="IVXXRPlatformHelper.ActivePlatform"/>.
    /// </summary>
    public sealed class IVXXRInputAdapter : MonoBehaviour
    {
        #region Constants
        private const string LOG_TAG = nameof(IVXXRInputAdapter);
        private const float PINCH_HOLD_THRESHOLD = 0.15f;
        #endregion

        #region Events
        /// <summary>Fired when a grab gesture begins. Parameter: hand.</summary>
        public event Action<Hand> OnGrab;
        /// <summary>Fired when a grab gesture ends. Parameter: hand.</summary>
        public event Action<Hand> OnRelease;
        /// <summary>Fired when a pinch gesture is detected. Parameter: hand.</summary>
        public event Action<Hand> OnPinch;
        /// <summary>Fired when gaze lands on a new target. Parameter: gaze ray.</summary>
        public event Action<Ray> OnGazeTarget;
        #endregion

        #region Private Fields
        private IIVXXRInputProvider _provider;
        private bool _wasGrabbingLeft;
        private bool _wasGrabbingRight;
        private bool _wasPinchingLeft;
        private bool _wasPinchingRight;
        private Ray? _lastGazeRay;
        #endregion

        #region Properties
        /// <summary>The currently active input provider, if any.</summary>
        public IIVXXRInputProvider ActiveProvider => _provider;
        #endregion

        #region Singleton
        private static IVXXRInputAdapter _instance;
        public static IVXXRInputAdapter Instance => _instance;

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }
            _instance = this;
            DontDestroyOnLoad(gameObject);
            AutoDetectProvider();
        }

        private void OnDestroy()
        {
            if (_instance == this) _instance = null;
        }
        #endregion

        #region Unity Lifecycle
        private void Update()
        {
            if (_provider == null) return;

            PollGrabState(Hand.Left, ref _wasGrabbingLeft);
            PollGrabState(Hand.Right, ref _wasGrabbingRight);
            PollPinchState(Hand.Left, ref _wasPinchingLeft);
            PollPinchState(Hand.Right, ref _wasPinchingRight);
            PollGaze();
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Set a custom input provider. Pass null to clear.
        /// </summary>
        public void SetProvider(IIVXXRInputProvider provider)
        {
            _provider = provider;
            Debug.Log($"[{LOG_TAG}] Provider set: {provider?.GetType().Name ?? "null"}");
        }

        /// <summary>Get the tracked hand pose, or null if unavailable.</summary>
        public Pose? GetHandPose(Hand hand)
        {
            return _provider?.GetHandPose(hand);
        }

        /// <summary>Get the tracked controller pose, or null if unavailable.</summary>
        public Pose? GetControllerPose(Hand hand)
        {
            return _provider?.GetControllerPose(hand);
        }

        /// <summary>Get the gaze direction ray, or null if eye tracking is unavailable.</summary>
        public Ray? GetGazeDirection()
        {
            return _provider?.GetGazeDirection();
        }

        /// <summary>Whether the user is grabbing with the specified hand.</summary>
        public bool IsGrabbing(Hand hand)
        {
            return _provider?.IsGrabbing(hand) ?? false;
        }

        /// <summary>Whether the user is pinching with the specified hand.</summary>
        public bool IsPinching(Hand hand)
        {
            return _provider?.IsPinching(hand) ?? false;
        }

        /// <summary>Trigger axis value (0-1) for the specified hand.</summary>
        public float GetTriggerValue(Hand hand)
        {
            return _provider?.GetTriggerValue(hand) ?? 0f;
        }
        #endregion

        #region Private Methods
        private void AutoDetectProvider()
        {
            var platform = IVXXRPlatformHelper.Instance?.ActivePlatform
                        ?? IVXXRPlatformHelper.XRPlatformType.None;

#if INTELLIVERSEX_HAS_META_XR
            if (platform == IVXXRPlatformHelper.XRPlatformType.MetaQuest)
            {
                _provider = new MetaXRInputProvider();
                Debug.Log($"[{LOG_TAG}] Auto-detected Meta XR input provider");
                return;
            }
#endif

#if INTELLIVERSEX_HAS_OPENXR
            if (platform == IVXXRPlatformHelper.XRPlatformType.GenericOpenXR
             || platform == IVXXRPlatformHelper.XRPlatformType.SteamVR)
            {
                _provider = new OpenXRInputProvider();
                Debug.Log($"[{LOG_TAG}] Auto-detected OpenXR input provider");
                return;
            }
#endif

            if (IVXXRPlatformHelper.Instance != null && IVXXRPlatformHelper.Instance.IsXRActive)
            {
                _provider = new FallbackInputProvider();
                Debug.Log($"[{LOG_TAG}] Using fallback XR input provider for platform: {platform}");
                return;
            }

            Debug.Log($"[{LOG_TAG}] No XR input provider detected for platform: {platform}");
        }

        private void PollGrabState(Hand hand, ref bool wasGrabbing)
        {
            bool isGrabbing = _provider.IsGrabbing(hand);
            if (isGrabbing && !wasGrabbing)
                OnGrab?.Invoke(hand);
            else if (!isGrabbing && wasGrabbing)
                OnRelease?.Invoke(hand);
            wasGrabbing = isGrabbing;
        }

        private void PollPinchState(Hand hand, ref bool wasPinching)
        {
            bool isPinching = _provider.IsPinching(hand);
            if (isPinching && !wasPinching)
                OnPinch?.Invoke(hand);
            wasPinching = isPinching;
        }

        private void PollGaze()
        {
            Ray? gaze = _provider.GetGazeDirection();
            if (gaze.HasValue && (!_lastGazeRay.HasValue
                || Vector3.Angle(_lastGazeRay.Value.direction, gaze.Value.direction) > 1f))
            {
                _lastGazeRay = gaze;
                OnGazeTarget?.Invoke(gaze.Value);
            }
        }
        #endregion

        #region Built-in Providers

#if INTELLIVERSEX_HAS_META_XR
        /// <summary>
        /// Input provider backed by the Meta (Oculus) XR SDK.
        /// </summary>
        private sealed class MetaXRInputProvider : IIVXXRInputProvider
        {
            public Pose? GetHandPose(Hand hand)
            {
                var step = OVRPlugin.Step.Render;
                var ovrHand = hand == Hand.Left ? OVRPlugin.Hand.HandLeft : OVRPlugin.Hand.HandRight;
                if (OVRPlugin.GetHandState(step, ovrHand, out var state))
                {
                    var pos = state.RootPose.Position;
                    var rot = state.RootPose.Orientation;
                    return new Pose(
                        new Vector3(pos.x, pos.y, -pos.z),
                        new Quaternion(-rot.x, -rot.y, rot.z, rot.w));
                }
                return null;
            }

            public Pose? GetControllerPose(Hand hand)
            {
                var node = hand == Hand.Left
                    ? UnityEngine.XR.XRNode.LeftHand
                    : UnityEngine.XR.XRNode.RightHand;
                var devices = new System.Collections.Generic.List<UnityEngine.XR.InputDevice>();
                UnityEngine.XR.InputDevices.GetDevicesAtXRNode(node, devices);
                if (devices.Count > 0
                    && devices[0].TryGetFeatureValue(UnityEngine.XR.CommonUsages.devicePosition, out var pos)
                    && devices[0].TryGetFeatureValue(UnityEngine.XR.CommonUsages.deviceRotation, out var rot))
                {
                    return new Pose(pos, rot);
                }
                return null;
            }

            public Ray? GetGazeDirection()
            {
                if (OVRPlugin.GetEyeTrackingEnabled()
                    && OVRPlugin.GetEyeGazesState(OVRPlugin.Step.Render, -1, out var gazes))
                {
                    var gaze = gazes.EyeGazes[0];
                    var pos = gaze.Pose.Position;
                    var rot = gaze.Pose.Orientation;
                    var origin = new Vector3(pos.x, pos.y, -pos.z);
                    var quat = new Quaternion(-rot.x, -rot.y, rot.z, rot.w);
                    return new Ray(origin, quat * Vector3.forward);
                }
                return null;
            }

            public bool IsGrabbing(Hand hand)
            {
                var ovrHand = hand == Hand.Left ? OVRPlugin.Hand.HandLeft : OVRPlugin.Hand.HandRight;
                if (OVRPlugin.GetHandState(OVRPlugin.Step.Render, ovrHand, out var state))
                {
                    return (state.Status & OVRPlugin.HandStatus.HandTracked) != 0
                        && state.HandConfidence == OVRPlugin.TrackingConfidence.High
                        && state.Pinches == 0
                        && state.GripStrength > 0.8f;
                }
                return false;
            }

            public bool IsPinching(Hand hand)
            {
                var ovrHand = hand == Hand.Left ? OVRPlugin.Hand.HandLeft : OVRPlugin.Hand.HandRight;
                if (OVRPlugin.GetHandState(OVRPlugin.Step.Render, ovrHand, out var state))
                {
                    return (state.Pinches & OVRPlugin.HandFingerPinch.Index) != 0;
                }
                return false;
            }

            public float GetTriggerValue(Hand hand)
            {
                var node = hand == Hand.Left
                    ? UnityEngine.XR.XRNode.LeftHand
                    : UnityEngine.XR.XRNode.RightHand;
                var devices = new System.Collections.Generic.List<UnityEngine.XR.InputDevice>();
                UnityEngine.XR.InputDevices.GetDevicesAtXRNode(node, devices);
                if (devices.Count > 0
                    && devices[0].TryGetFeatureValue(UnityEngine.XR.CommonUsages.trigger, out float val))
                {
                    return val;
                }
                return 0f;
            }
        }
#endif

#if INTELLIVERSEX_HAS_OPENXR
        /// <summary>
        /// Input provider backed by the Unity OpenXR subsystem and XR Hands package.
        /// </summary>
        private sealed class OpenXRInputProvider : IIVXXRInputProvider
        {
            public Pose? GetHandPose(Hand hand)
            {
                var subsystems = new System.Collections.Generic.List<UnityEngine.XR.Hands.XRHandSubsystem>();
                SubsystemManager.GetSubsystems(subsystems);
                if (subsystems.Count == 0 || !subsystems[0].running) return null;

                var xrHand = hand == Hand.Left ? subsystems[0].leftHand : subsystems[0].rightHand;
                if (xrHand.isTracked)
                {
                    var rootJoint = xrHand.GetJoint(UnityEngine.XR.Hands.XRHandJointID.Wrist);
                    if (rootJoint.TryGetPose(out var pose))
                        return pose;
                }
                return null;
            }

            public Pose? GetControllerPose(Hand hand)
            {
                var node = hand == Hand.Left
                    ? UnityEngine.XR.XRNode.LeftHand
                    : UnityEngine.XR.XRNode.RightHand;
                var devices = new System.Collections.Generic.List<UnityEngine.XR.InputDevice>();
                UnityEngine.XR.InputDevices.GetDevicesAtXRNode(node, devices);
                if (devices.Count > 0
                    && devices[0].TryGetFeatureValue(UnityEngine.XR.CommonUsages.devicePosition, out var pos)
                    && devices[0].TryGetFeatureValue(UnityEngine.XR.CommonUsages.deviceRotation, out var rot))
                {
                    return new Pose(pos, rot);
                }
                return null;
            }

            public Ray? GetGazeDirection()
            {
                var eyeSubsystems = new System.Collections.Generic.List<UnityEngine.XR.XREyeSubsystem>();
                SubsystemManager.GetSubsystems(eyeSubsystems);
                if (eyeSubsystems.Count > 0 && eyeSubsystems[0].running)
                {
                    if (eyeSubsystems[0].TryGetFixationPoint(out var point))
                    {
                        var cam = Camera.main;
                        if (cam != null)
                            return new Ray(cam.transform.position, (point - cam.transform.position).normalized);
                    }
                }
                return null;
            }

            public bool IsGrabbing(Hand hand)
            {
                var node = hand == Hand.Left
                    ? UnityEngine.XR.XRNode.LeftHand
                    : UnityEngine.XR.XRNode.RightHand;
                var devices = new System.Collections.Generic.List<UnityEngine.XR.InputDevice>();
                UnityEngine.XR.InputDevices.GetDevicesAtXRNode(node, devices);
                if (devices.Count > 0
                    && devices[0].TryGetFeatureValue(UnityEngine.XR.CommonUsages.gripButton, out bool val))
                {
                    return val;
                }
                return false;
            }

            public bool IsPinching(Hand hand)
            {
                var subsystems = new System.Collections.Generic.List<UnityEngine.XR.Hands.XRHandSubsystem>();
                SubsystemManager.GetSubsystems(subsystems);
                if (subsystems.Count == 0 || !subsystems[0].running) return false;

                var xrHand = hand == Hand.Left ? subsystems[0].leftHand : subsystems[0].rightHand;
                if (!xrHand.isTracked) return false;

                var thumbTip = xrHand.GetJoint(UnityEngine.XR.Hands.XRHandJointID.ThumbTip);
                var indexTip = xrHand.GetJoint(UnityEngine.XR.Hands.XRHandJointID.IndexTip);
                if (thumbTip.TryGetPose(out var thumbPose) && indexTip.TryGetPose(out var indexPose))
                {
                    return Vector3.Distance(thumbPose.position, indexPose.position) < 0.02f;
                }
                return false;
            }

            public float GetTriggerValue(Hand hand)
            {
                var node = hand == Hand.Left
                    ? UnityEngine.XR.XRNode.LeftHand
                    : UnityEngine.XR.XRNode.RightHand;
                var devices = new System.Collections.Generic.List<UnityEngine.XR.InputDevice>();
                UnityEngine.XR.InputDevices.GetDevicesAtXRNode(node, devices);
                if (devices.Count > 0
                    && devices[0].TryGetFeatureValue(UnityEngine.XR.CommonUsages.trigger, out float val))
                {
                    return val;
                }
                return 0f;
            }
        }
#endif

        /// <summary>
        /// Generic input provider using Unity's XR InputDevices API.
        /// Works with any XR loader (PSVR2, WindowsMR, visionOS, etc.) without vendor-specific SDKs.
        /// </summary>
        private sealed class FallbackInputProvider : IIVXXRInputProvider
        {
            public Pose? GetHandPose(Hand hand)
            {
                return GetControllerPose(hand);
            }

            public Pose? GetControllerPose(Hand hand)
            {
                var node = hand == Hand.Left
                    ? UnityEngine.XR.XRNode.LeftHand
                    : UnityEngine.XR.XRNode.RightHand;
                var devices = new System.Collections.Generic.List<UnityEngine.XR.InputDevice>();
                UnityEngine.XR.InputDevices.GetDevicesAtXRNode(node, devices);
                if (devices.Count > 0
                    && devices[0].TryGetFeatureValue(UnityEngine.XR.CommonUsages.devicePosition, out var pos)
                    && devices[0].TryGetFeatureValue(UnityEngine.XR.CommonUsages.deviceRotation, out var rot))
                {
                    return new Pose(pos, rot);
                }
                return null;
            }

            public Ray? GetGazeDirection()
            {
                var devices = new System.Collections.Generic.List<UnityEngine.XR.InputDevice>();
                UnityEngine.XR.InputDevices.GetDevicesAtXRNode(UnityEngine.XR.XRNode.CenterEye, devices);
                if (devices.Count > 0
                    && devices[0].TryGetFeatureValue(UnityEngine.XR.CommonUsages.devicePosition, out var pos)
                    && devices[0].TryGetFeatureValue(UnityEngine.XR.CommonUsages.deviceRotation, out var rot))
                {
                    return new Ray(pos, new Quaternion(rot.x, rot.y, rot.z, rot.w) * Vector3.forward);
                }
                return null;
            }

            public bool IsGrabbing(Hand hand)
            {
                var node = hand == Hand.Left
                    ? UnityEngine.XR.XRNode.LeftHand
                    : UnityEngine.XR.XRNode.RightHand;
                var devices = new System.Collections.Generic.List<UnityEngine.XR.InputDevice>();
                UnityEngine.XR.InputDevices.GetDevicesAtXRNode(node, devices);
                if (devices.Count > 0
                    && devices[0].TryGetFeatureValue(UnityEngine.XR.CommonUsages.gripButton, out bool val))
                {
                    return val;
                }
                return false;
            }

            public bool IsPinching(Hand hand)
            {
                var node = hand == Hand.Left
                    ? UnityEngine.XR.XRNode.LeftHand
                    : UnityEngine.XR.XRNode.RightHand;
                var devices = new System.Collections.Generic.List<UnityEngine.XR.InputDevice>();
                UnityEngine.XR.InputDevices.GetDevicesAtXRNode(node, devices);
                if (devices.Count > 0
                    && devices[0].TryGetFeatureValue(UnityEngine.XR.CommonUsages.primaryButton, out bool val))
                {
                    return val;
                }
                return false;
            }

            public float GetTriggerValue(Hand hand)
            {
                var node = hand == Hand.Left
                    ? UnityEngine.XR.XRNode.LeftHand
                    : UnityEngine.XR.XRNode.RightHand;
                var devices = new System.Collections.Generic.List<UnityEngine.XR.InputDevice>();
                UnityEngine.XR.InputDevices.GetDevicesAtXRNode(node, devices);
                if (devices.Count > 0
                    && devices[0].TryGetFeatureValue(UnityEngine.XR.CommonUsages.trigger, out float val))
                {
                    return val;
                }
                return 0f;
            }
        }

        #endregion
    }
}
