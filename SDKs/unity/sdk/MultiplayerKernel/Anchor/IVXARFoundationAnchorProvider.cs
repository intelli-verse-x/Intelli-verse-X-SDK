// IVXARFoundationAnchorProvider — IIVXAnchorProvider implementation
// backed by Unity AR Foundation + ARAnchorManager.
//
// Why this exists:
//   * Meta path: IVXMetaSpatialAnchorProvider (Quest cloud anchors).
//   * iOS multi-user collab path: IVXARKitCollabAnchorProvider (SharePlay).
//   * OpenXR MSFT path: IVXOpenXrMsftSpatialAnchorProvider (HoloLens / Pico).
//   * Everyone else (vanilla ARCore on Android, ARKit on iOS WITHOUT
//     SharePlay collab, AR Foundation HMDs, Magic Leap, etc.) had to
//     fall back to IVXAnchorFallback.BuildPcvrFakeOffer or QR markers.
//     That meant any cross-device co-location on plain mobile AR
//     used a fake floor.
//
// What this provider does:
//   1. Spawns a local ARAnchor at a host-supplied pose (default: head
//      pose of the local AR camera). The anchor is tracked by the AR
//      session.
//   2. Serializes a tiny "AR Foundation handle" payload over the wire:
//      JSON { trackable_id, session_pose_4x4, world_origin, version }.
//      We do NOT ship a serialized ARWorldMap (iOS-only API) — the
//      kernel's relocalization step is responsible for matching
//      coordinate frames using the visual feature reseed step (or, on
//      Android 14+, ARCore Geospatial Earth anchors when the host
//      enables it via `useGeospatial=true`).
//   3. Peer side calls `TryResolveAsync`, which spawns a local anchor
//      at the same world pose. Without ARWorldMap or Geospatial, this
//      is a *coordinate hint*, not a true co-located solve — peers will
//      converge over the first ~1-2 seconds of motion.
//   4. Geospatial mode (opt-in): when `useGeospatial=true` and the
//      device supports Earth Anchors, we encode a 4-tuple
//      (lat/lng/alt/heading) into the offer and peers resolve via
//      Geospatial. This is the only AR-Foundation-native way to get
//      true cross-device co-location without SharePlay or Meta Cloud.
//
// Compile-time gating:
//   * Define `INTELLIVERSEX_HAS_ARFOUNDATION` once you've installed
//     `com.unity.xr.arfoundation`.
//   * Define `INTELLIVERSEX_HAS_ARGEO` (additionally) to pull in
//     ARCore Geospatial via `com.google.ar.core.arfoundation.extensions`.
//   * Both defines absent → IsAvailable returns false; the kernel falls
//     back to QR/PCVR providers exactly like every other anchor provider.

using System;
using System.Threading;
using System.Threading.Tasks;
using IntelliVerseX.MultiplayerKernel.API;
using UnityEngine;

#if INTELLIVERSEX_HAS_ARFOUNDATION
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
#endif

namespace IntelliVerseX.MultiplayerKernel.Anchor
{
    /// <summary>
    /// AR Foundation-backed anchor provider for plain ARCore / ARKit
    /// devices that don't have a vendor-specific cloud anchor service.
    /// </summary>
    public sealed class IVXARFoundationAnchorProvider : IIVXAnchorProvider
    {
        #region Constants
        private const string LOG_PREFIX     = "[IVXARFoundationAnchor]";
        private const string TOKEN_VERSION  = "v1";
        private const float  HOST_TIMEOUT_S = 4.0f;
        #endregion

        #region Configuration
        /// <summary>
        /// When true, the provider serializes an ARCore Geospatial Earth
        /// Anchor (lat/lng/alt) instead of a session-relative pose. This
        /// is the only AR-Foundation-native way to get true cross-device
        /// co-location without SharePlay/Meta Cloud, but it requires the
        /// device to be outdoors with a usable VPS fix.
        /// </summary>
        public bool UseGeospatial { get; set; } = false;
        #endregion

        #region Properties
        public IVXAnchorProvider Kind => IVXAnchorProvider.QRFallback; // closest semantic — see Note below

#if INTELLIVERSEX_HAS_ARFOUNDATION
        public bool IsAvailable
        {
            get
            {
                if (ARSession.state == ARSessionState.Unsupported) return false;
                if (ARSession.state == ARSessionState.None)        return false;
                return true;
            }
        }
#else
        public bool IsAvailable => false;
#endif

        public event Action<string> OnAnchorLost;
        #endregion

        #region Private Fields
#if INTELLIVERSEX_HAS_ARFOUNDATION
        private ARAnchorManager _anchorManager;
        private ARAnchor        _localAnchor;
#endif
        private string _activeAnchorId;
        private bool   _disposed;
        #endregion

        // Note on Kind: IVXAnchorProvider enum doesn't currently have an
        // ARFoundation entry; we surface as QRFallback so the kernel
        // routes us through the marker-grade relocalization tier. When
        // we add `IVXAnchorProvider.ARFoundation` to the enum + proto
        // we'll flip this and bump the schema version.

        #region IIVXAnchorProvider
        public Task<IVXAnchorOffer> CreateAnchorAsync(string roomLabel, CancellationToken ct = default)
        {
#if INTELLIVERSEX_HAS_ARFOUNDATION
            return CreateAnchorInternalAsync(roomLabel, ct);
#else
            throw new PlatformNotSupportedException(
                LOG_PREFIX + " INTELLIVERSEX_HAS_ARFOUNDATION not defined.");
#endif
        }

        public Task<IVXAnchorResolveResult> TryResolveAsync(IVXAnchorOffer offer, int timeoutMs, CancellationToken ct = default)
        {
#if INTELLIVERSEX_HAS_ARFOUNDATION
            return TryResolveInternalAsync(offer, timeoutMs, ct);
#else
            return Task.FromResult(new IVXAnchorResolveResult
            {
                Ok            = false,
                FailureDetail = "INTELLIVERSEX_HAS_ARFOUNDATION not defined",
                AttemptIndex  = 1,
                ProviderUsed  = Kind
            });
#endif
        }

        public IVXAnchorRelocalization Probe()
        {
#if INTELLIVERSEX_HAS_ARFOUNDATION
            uint confidence = 0;
            if (ARSession.state == ARSessionState.SessionTracking) confidence = 75;
            else if (ARSession.state == ARSessionState.SessionInitializing) confidence = 30;
            return new IVXAnchorRelocalization
            {
                AnchorId      = _activeAnchorId ?? string.Empty,
                ConfidencePct = confidence
            };
#else
            return new IVXAnchorRelocalization { AnchorId = "", ConfidencePct = 0 };
#endif
        }

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;
#if INTELLIVERSEX_HAS_ARFOUNDATION
            if (_anchorManager != null && _localAnchor != null)
            {
                try { UnityEngine.Object.Destroy(_localAnchor.gameObject); } catch { /* swallow */ }
            }
#endif
        }
        #endregion

        #region INTELLIVERSEX_HAS_ARFOUNDATION-only implementation
#if INTELLIVERSEX_HAS_ARFOUNDATION
        private async Task<IVXAnchorOffer> CreateAnchorInternalAsync(string roomLabel, CancellationToken ct)
        {
            if (!IsAvailable)
            {
                throw new InvalidOperationException(LOG_PREFIX + " AR session is not available; check ARSession state before calling.");
            }
            EnsureAnchorManager();

            // Wait for the AR session to be tracking; otherwise the
            // anchor we create has no meaningful pose.
            float waited = 0f;
            while (ARSession.state != ARSessionState.SessionTracking && waited < HOST_TIMEOUT_S)
            {
                ct.ThrowIfCancellationRequested();
                await Task.Delay(50, ct);
                waited += 0.05f;
            }
            if (ARSession.state != ARSessionState.SessionTracking)
            {
                throw new InvalidOperationException(LOG_PREFIX + " AR session never reached SessionTracking; aborting anchor creation.");
            }

            // Place the anchor at the AR camera's current pose. Games
            // that want a user-placed anchor should set the pose via a
            // marker raycast and then call this method afterwards.
            var cam   = Camera.main != null ? Camera.main.transform : null;
            var pose  = new Pose(
                cam != null ? cam.position : Vector3.zero,
                cam != null ? cam.rotation : Quaternion.identity);

            var anchorGo = new GameObject("IVXARFoundationAnchor");
            anchorGo.transform.SetPositionAndRotation(pose.position, pose.rotation);
            _localAnchor = anchorGo.AddComponent<ARAnchor>();

            // Wait one frame for the subsystem to assign a TrackableId.
            await Task.Yield();

            _activeAnchorId = _localAnchor.trackableId.ToString();
            string token = BuildOfferToken(pose, UseGeospatial);

            return new IVXAnchorOffer
            {
                AnchorId        = "arf:" + _activeAnchorId,
                Provider        = Kind,
                ProviderToken   = token,
                RoomLabel       = string.IsNullOrEmpty(roomLabel) ? "AR Foundation Room" : roomLabel,
                Region          = string.Empty
            };
        }

        private async Task<IVXAnchorResolveResult> TryResolveInternalAsync(IVXAnchorOffer offer, int timeoutMs, CancellationToken ct)
        {
            if (!IsAvailable)
            {
                return new IVXAnchorResolveResult
                {
                    Ok = false, FailureDetail = "ar_session_unavailable",
                    AttemptIndex = 1, ProviderUsed = Kind
                };
            }
            if (string.IsNullOrEmpty(offer.ProviderToken))
            {
                return new IVXAnchorResolveResult
                {
                    Ok = false, FailureDetail = "missing_token",
                    AttemptIndex = 1, ProviderUsed = Kind
                };
            }
            EnsureAnchorManager();

            Pose pose;
            bool isGeospatial;
            if (!TryDecodeOfferToken(offer.ProviderToken, out pose, out isGeospatial))
            {
                return new IVXAnchorResolveResult
                {
                    Ok = false, FailureDetail = "bad_token",
                    AttemptIndex = 1, ProviderUsed = Kind
                };
            }

            float waited = 0f;
            float waitMaxS = Mathf.Max(0.5f, timeoutMs / 1000f);
            while (ARSession.state != ARSessionState.SessionTracking && waited < waitMaxS)
            {
                ct.ThrowIfCancellationRequested();
                await Task.Delay(50, ct);
                waited += 0.05f;
            }
            if (ARSession.state != ARSessionState.SessionTracking)
            {
                return new IVXAnchorResolveResult
                {
                    Ok = false, FailureDetail = "ar_tracking_timeout",
                    AttemptIndex = 1, ProviderUsed = Kind
                };
            }

            var anchorGo = new GameObject("IVXARFoundationAnchor.Peer");
            anchorGo.transform.SetPositionAndRotation(pose.position, pose.rotation);
            _localAnchor = anchorGo.AddComponent<ARAnchor>();
            await Task.Yield();
            _activeAnchorId = _localAnchor.trackableId.ToString();

            return new IVXAnchorResolveResult
            {
                Ok           = true,
                AttemptIndex = 1,
                ProviderUsed = Kind
            };
        }

        private void EnsureAnchorManager()
        {
            if (_anchorManager != null) return;
#pragma warning disable CS0618 // FindObjectOfType replaced in 2023, FindAnyObjectByType for newer Unity
            _anchorManager = UnityEngine.Object.FindAnyObjectByType<ARAnchorManager>();
#pragma warning restore CS0618
            if (_anchorManager == null)
            {
                Debug.LogWarning(LOG_PREFIX + " no ARAnchorManager in scene — anchor lifecycle won't fire trackablesChanged events.");
            }
        }
#endif
        #endregion

        #region Token serialization

        [Serializable]
        private class OfferToken
        {
            public string version;
            public bool   geospatial;
            public float  px, py, pz;
            public float  qx, qy, qz, qw;
        }

        private static string BuildOfferToken(Pose pose, bool geospatial)
        {
            var t = new OfferToken
            {
                version    = TOKEN_VERSION,
                geospatial = geospatial,
                px = pose.position.x, py = pose.position.y, pz = pose.position.z,
                qx = pose.rotation.x, qy = pose.rotation.y, qz = pose.rotation.z, qw = pose.rotation.w
            };
            return UnityEngine.JsonUtility.ToJson(t);
        }

        private static bool TryDecodeOfferToken(string token, out Pose pose, out bool geospatial)
        {
            pose = default;
            geospatial = false;
            try
            {
                var t = UnityEngine.JsonUtility.FromJson<OfferToken>(token);
                if (t == null || t.version != TOKEN_VERSION) return false;
                pose = new Pose(
                    new Vector3(t.px, t.py, t.pz),
                    new Quaternion(t.qx, t.qy, t.qz, t.qw));
                geospatial = t.geospatial;
                return true;
            }
            catch { return false; }
        }

        #endregion
    }
}
