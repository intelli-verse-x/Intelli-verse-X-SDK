// IVXOpenXrMsftSpatialAnchorProvider — OpenXR XR_MSFT_spatial_anchor +
// XR_MSFT_spatial_anchor_persistence implementation of IIVXAnchorProvider.
//
// Targets: HoloLens 2, Windows Mixed Reality, and any OpenXR runtime
// exposing the MSFT spatial anchor extensions (Pico XR Enterprise,
// some HTC Vive Focus runtimes). Persistent anchors are stored via
// XR_MSFT_spatial_anchor_persistence; sharing across devices uses
// either Azure Spatial Anchors (if configured) or a chunked upload
// of the serialized anchor blob.
//
// The native bridge lives in Plugins/OpenXR/IVXOpenXrMsftAnchorBridge.cpp.
// Here we only wrap the C entry points so the multiplayer kernel stays
// portable across engines.

using System;
using System.Threading;
using System.Threading.Tasks;
using IntelliVerseX.MultiplayerKernel.API;

#if (UNITY_STANDALONE_WIN || UNITY_WSA) && !UNITY_EDITOR
using System.Runtime.InteropServices;
#endif

namespace IntelliVerseX.MultiplayerKernel.Anchor
{
    public sealed class IVXOpenXrMsftSpatialAnchorProvider : IIVXAnchorProvider
    {
        public IVXAnchorProvider Kind => IVXAnchorProvider.AzureSpatial; // Generic OpenXR/MSFT is folded under AzureSpatial in proto enum.

#if (UNITY_STANDALONE_WIN || UNITY_WSA) && !UNITY_EDITOR
        public bool IsAvailable
        {
            get
            {
                try { return ivx_openxr_msft_supported() != 0; } catch { return false; }
            }
        }
#else
        public bool IsAvailable => false;
#endif

        public event Action<string> OnAnchorLost;
        private string _activeAnchorId;

        public async Task<IVXAnchorOffer> CreateAnchorAsync(string roomLabel, CancellationToken ct = default)
        {
#if (UNITY_STANDALONE_WIN || UNITY_WSA) && !UNITY_EDITOR
            string r = ivx_openxr_msft_capture(roomLabel ?? "");
            if (string.IsNullOrEmpty(r))
                throw new InvalidOperationException("[IVXOpenXrMsft] capture returned null");
            var parsed = UnityEngine.JsonUtility.FromJson<CaptureResult>(r);
            if (parsed == null || !parsed.ok)
                throw new InvalidOperationException("[IVXOpenXrMsft] capture failed: " + (parsed?.detail ?? "?"));
            _activeAnchorId = parsed.anchor_id;
            return new IVXAnchorOffer
            {
                AnchorId       = "msft:" + parsed.anchor_id,
                Provider       = Kind,
                ProviderToken  = parsed.b64,
                RoomLabel      = string.IsNullOrEmpty(roomLabel) ? "MSFT Room" : roomLabel,
                Region         = ""
            };
#else
            await Task.CompletedTask;
            throw new PlatformNotSupportedException("OpenXR MSFT not available on this platform");
#endif
        }

        public async Task<IVXAnchorResolveResult> TryResolveAsync(IVXAnchorOffer offer, int timeoutMs, CancellationToken ct = default)
        {
#if (UNITY_STANDALONE_WIN || UNITY_WSA) && !UNITY_EDITOR
            if (string.IsNullOrEmpty(offer.ProviderToken))
                return new IVXAnchorResolveResult { Ok = false, FailureDetail = "missing_token" };
            string r = ivx_openxr_msft_resolve(offer.ProviderToken, timeoutMs);
            var parsed = UnityEngine.JsonUtility.FromJson<ResolveResult>(r);
            if (parsed == null) return new IVXAnchorResolveResult { Ok = false, FailureDetail = "bad_response" };
            return new IVXAnchorResolveResult
            {
                Ok = parsed.ok, FailureDetail = parsed.detail ?? "",
                AttemptIndex = parsed.attempt, ProviderUsed = Kind
            };
#else
            await Task.CompletedTask;
            return new IVXAnchorResolveResult { Ok = false, FailureDetail = "OpenXR MSFT not available" };
#endif
        }

        public IVXAnchorRelocalization Probe()
        {
#if (UNITY_STANDALONE_WIN || UNITY_WSA) && !UNITY_EDITOR
            int conf = ivx_openxr_msft_confidence();
            return new IVXAnchorRelocalization
            {
                AnchorId = "msft:" + (_activeAnchorId ?? ""),
                ConfidencePct = (uint)Math.Max(0, Math.Min(100, conf))
            };
#else
            return new IVXAnchorRelocalization { AnchorId = "msft:", ConfidencePct = 0 };
#endif
        }

        public void Dispose() { }

#if (UNITY_STANDALONE_WIN || UNITY_WSA) && !UNITY_EDITOR
        [DllImport("IVXOpenXrMsftBridge")]
        private static extern int ivx_openxr_msft_supported();
        [DllImport("IVXOpenXrMsftBridge")]
        private static extern string ivx_openxr_msft_capture(string roomLabel);
        [DllImport("IVXOpenXrMsftBridge")]
        private static extern string ivx_openxr_msft_resolve(string token, int timeoutMs);
        [DllImport("IVXOpenXrMsftBridge")]
        private static extern int ivx_openxr_msft_confidence();
#endif

        [Serializable]
        private class CaptureResult
        {
            public string anchor_id;
            public string b64;
            public bool ok;
            public string detail;
        }

        [Serializable]
        private class ResolveResult
        {
            public bool ok;
            public string detail;
            public uint attempt;
        }
    }
}
