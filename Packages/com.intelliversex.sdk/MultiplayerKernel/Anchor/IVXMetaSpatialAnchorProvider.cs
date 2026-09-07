// IVXMetaSpatialAnchorProvider — Meta Spatial Anchors (Quest / Quest 3)
// implementation of IIVXAnchorProvider.
//
// Backed by Meta's Shared Spatial Anchors API
// (Oculus Integration / Meta XR SDK). Conditionally compiled — when the
// IVX_META_XR define is not present, this provider reports
// IsAvailable == false and CreateAnchorAsync throws, matching what every
// other IIVXAnchorProvider implementation does on a wrong-platform host.
//
// Provider tokens: we ship the Meta `OVRSpatialAnchor.UUID` (16-byte
// guid) over the wire as a base64 string.
//
// Permissions required at runtime:
//   * com.oculus.permission.USE_SCENE
//   * com.oculus.permission.USE_ANCHOR_API
//
// Sharing flow (Meta-specific):
//   1. Host: Save anchor to cloud (OVRSpatialAnchor.SaveAsync(StorageLocation.Cloud)).
//   2. Host: Share anchor with the user_ids of joined peers.
//   3. Peer: LoadFromUuid(host_uuid) → returns OVRSpatialAnchor on success.

using System;
using System.Threading;
using System.Threading.Tasks;
using IntelliVerseX.MultiplayerKernel.API;

#if IVX_META_XR
using Oculus.Platform; // for User.GetLoggedInUser
using OVRSpatialAnchor = OVRSpatialAnchor;
#endif

namespace IntelliVerseX.MultiplayerKernel.Anchor
{
    public sealed class IVXMetaSpatialAnchorProvider : IIVXAnchorProvider
    {
        public IVXAnchorProvider Kind => IVXAnchorProvider.MetaShared;

#if IVX_META_XR
        public bool IsAvailable => OVRManager.instance != null && OVRManager.isHmdPresent;
#else
        public bool IsAvailable => false;
#endif

        public event Action<string> OnAnchorLost;

        private string _hostAnchorUuid; // base64 of the 16-byte UUID

        public async Task<IVXAnchorOffer> CreateAnchorAsync(string roomLabel, CancellationToken ct = default)
        {
#if IVX_META_XR
            // Spawn a stationary anchor at the user's current head position.
            // Real implementations should let the user place a marker
            // (table corner, doorway, etc.) — we use head-pose for the
            // smoke path.
            var hostObj = new UnityEngine.GameObject("IVXMetaAnchorHost");
            hostObj.transform.position = OVRManager.instance != null && OVRManager.instance.headPoseRelativeOffsetTranslation != null
                ? UnityEngine.Camera.main != null ? UnityEngine.Camera.main.transform.position : UnityEngine.Vector3.zero
                : UnityEngine.Vector3.zero;
            var anchor = hostObj.AddComponent<OVRSpatialAnchor>();
            // Wait for the localization signal.
            int waitMs = 0;
            while (!anchor.Localized && waitMs < 4000 && !ct.IsCancellationRequested)
            {
                await Task.Delay(50, ct);
                waitMs += 50;
            }
            if (!anchor.Localized)
            {
                UnityEngine.Object.Destroy(hostObj);
                throw new InvalidOperationException("[IVXMetaAnchor] localization timeout");
            }
            // Save to cloud.
            var saveOp = anchor.SaveAsync(new OVRSpatialAnchor.SaveOptions { Storage = OVRSpace.StorageLocation.Cloud });
            await saveOp;
            if (!saveOp.GetResult())
            {
                UnityEngine.Object.Destroy(hostObj);
                throw new InvalidOperationException("[IVXMetaAnchor] cloud save failed");
            }
            _hostAnchorUuid = Convert.ToBase64String(anchor.Uuid.ToByteArray());
            return new IVXAnchorOffer
            {
                AnchorId       = "meta:" + _hostAnchorUuid,
                Provider       = Kind,
                ProviderToken  = _hostAnchorUuid,
                RoomLabel      = string.IsNullOrEmpty(roomLabel) ? "Meta Room" : roomLabel,
                Region         = ""
            };
#else
            await Task.CompletedTask;
            throw new PlatformNotSupportedException("IVX_META_XR not defined");
#endif
        }

        public async Task<IVXAnchorResolveResult> TryResolveAsync(IVXAnchorOffer offer, int timeoutMs, CancellationToken ct = default)
        {
#if IVX_META_XR
            if (offer.Provider != IVXAnchorProvider.MetaShared)
            {
                return new IVXAnchorResolveResult { Ok = false, FailureDetail = "wrong_provider" };
            }
            if (string.IsNullOrEmpty(offer.ProviderToken))
            {
                return new IVXAnchorResolveResult { Ok = false, FailureDetail = "missing_token" };
            }
            byte[] uuidBytes;
            try { uuidBytes = Convert.FromBase64String(offer.ProviderToken); }
            catch { return new IVXAnchorResolveResult { Ok = false, FailureDetail = "bad_token" }; }
            if (uuidBytes.Length != 16)
            {
                return new IVXAnchorResolveResult { Ok = false, FailureDetail = "bad_uuid_len" };
            }
            var guid = new Guid(uuidBytes);
            var loadOp = OVRSpatialAnchor.LoadUnboundAnchorsAsync(new OVRSpatialAnchor.LoadOptions
            {
                Uuids       = new System.Collections.Generic.List<Guid> { guid },
                StorageLocation = OVRSpace.StorageLocation.Cloud
            });
            int waited = 0;
            while (!loadOp.IsCompleted && waited < timeoutMs && !ct.IsCancellationRequested)
            {
                await Task.Delay(50, ct);
                waited += 50;
            }
            if (!loadOp.IsCompleted)
            {
                return new IVXAnchorResolveResult { Ok = false, FailureDetail = "timeout", AttemptIndex = 1 };
            }
            var anchors = loadOp.GetResult();
            if (anchors == null || anchors.Length == 0)
            {
                return new IVXAnchorResolveResult { Ok = false, FailureDetail = "not_found", AttemptIndex = 1 };
            }
            return new IVXAnchorResolveResult
            {
                Ok = true, AttemptIndex = 1, ProviderUsed = Kind
            };
#else
            await Task.CompletedTask;
            return new IVXAnchorResolveResult { Ok = false, FailureDetail = "IVX_META_XR not defined" };
#endif
        }

        public IVXAnchorRelocalization Probe()
        {
            return new IVXAnchorRelocalization
            {
                AnchorId = "meta:" + (_hostAnchorUuid ?? ""),
                ConfidencePct = (uint)(IsAvailable ? 90 : 0)
            };
        }

        public void Dispose() { }
    }
}
