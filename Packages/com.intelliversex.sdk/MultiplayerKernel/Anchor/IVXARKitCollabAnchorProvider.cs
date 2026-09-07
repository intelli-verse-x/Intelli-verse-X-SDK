// IVXARKitCollabAnchorProvider — Apple ARKit Collaboration + SharePlay
// implementation of IIVXAnchorProvider for iOS / iPadOS / visionOS.
//
// Backed by ARWorldMap serialization + ARKit collaboration data, with a
// SharePlay GroupSession transport on iOS 15+. The visionOS variant uses
// the system "Shared World Anchor" surface available via ARKitSession +
// SharePlay.
//
// Wire shape:
//   * AnchorOffer.provider = ARKIT_COLLAB
//   * provider_anchor_token = base64(ARWorldMap.NSData) trimmed to a
//     ChunkRef if size > 1 MiB (chunked upload to S3 with the URL in
//     the token; the kernel doesn't care).
//
// The actual ARKit P/Invoke + SharePlay glue is intentionally NOT in
// this file — it lives in the platform-specific Objective-C++ bridge
// at Plugins/iOS/IVXARKitCollabBridge.mm. This C# class just wraps the
// bridge calls so the multiplayer kernel can stay portable.

using System;
using System.Threading;
using System.Threading.Tasks;
using IntelliVerseX.MultiplayerKernel.API;

#if UNITY_IOS && !UNITY_EDITOR
using System.Runtime.InteropServices;
#endif

namespace IntelliVerseX.MultiplayerKernel.Anchor
{
    public sealed class IVXARKitCollabAnchorProvider : IIVXAnchorProvider
    {
        public IVXAnchorProvider Kind => IVXAnchorProvider.ARKitCollab;

#if UNITY_IOS && !UNITY_EDITOR
        public bool IsAvailable
        {
            get
            {
                try { return ivx_arkit_collab_supported() != 0; }
                catch { return false; }
            }
        }
#else
        public bool IsAvailable => false;
#endif

        public event Action<string> OnAnchorLost;
        private string _activeAnchorId;

        public async Task<IVXAnchorOffer> CreateAnchorAsync(string roomLabel, CancellationToken ct = default)
        {
#if UNITY_IOS && !UNITY_EDITOR
            // Returns a JSON like {"anchor_id":"...","b64":"...","ok":true,
            //                       "shareplay_session":"...","detail":""}
            string r = ivx_arkit_collab_capture(roomLabel ?? "");
            if (string.IsNullOrEmpty(r))
                throw new InvalidOperationException("[IVXARKitCollab] capture returned null");
            var parsed = UnityEngine.JsonUtility.FromJson<CaptureResult>(r);
            if (parsed == null || !parsed.ok)
                throw new InvalidOperationException("[IVXARKitCollab] capture failed: " + (parsed?.detail ?? "?"));
            _activeAnchorId = parsed.anchor_id;
            return new IVXAnchorOffer
            {
                AnchorId       = "arkit:" + parsed.anchor_id,
                Provider       = Kind,
                ProviderToken  = parsed.b64,
                RoomLabel      = string.IsNullOrEmpty(roomLabel) ? "ARKit Room" : roomLabel,
                Region         = ""
            };
#else
            await Task.CompletedTask;
            throw new PlatformNotSupportedException("ARKit Collab requires iOS / iPadOS / visionOS");
#endif
        }

        public async Task<IVXAnchorResolveResult> TryResolveAsync(IVXAnchorOffer offer, int timeoutMs, CancellationToken ct = default)
        {
#if UNITY_IOS && !UNITY_EDITOR
            if (offer.Provider != IVXAnchorProvider.ARKitCollab)
                return new IVXAnchorResolveResult { Ok = false, FailureDetail = "wrong_provider" };
            if (string.IsNullOrEmpty(offer.ProviderToken))
                return new IVXAnchorResolveResult { Ok = false, FailureDetail = "missing_token" };
            string r = ivx_arkit_collab_resolve(offer.ProviderToken, timeoutMs);
            var parsed = UnityEngine.JsonUtility.FromJson<ResolveResult>(r);
            if (parsed == null) return new IVXAnchorResolveResult { Ok = false, FailureDetail = "bad_response" };
            return new IVXAnchorResolveResult
            {
                Ok = parsed.ok, FailureDetail = parsed.detail ?? "",
                AttemptIndex = parsed.attempt, ProviderUsed = Kind
            };
#else
            await Task.CompletedTask;
            return new IVXAnchorResolveResult { Ok = false, FailureDetail = "ARKit Collab not available" };
#endif
        }

        public IVXAnchorRelocalization Probe()
        {
#if UNITY_IOS && !UNITY_EDITOR
            int conf = ivx_arkit_collab_confidence();
            return new IVXAnchorRelocalization
            {
                AnchorId = "arkit:" + (_activeAnchorId ?? ""),
                ConfidencePct = (uint)Math.Max(0, Math.Min(100, conf))
            };
#else
            return new IVXAnchorRelocalization
            {
                AnchorId = "arkit:" + (_activeAnchorId ?? ""),
                ConfidencePct = 0
            };
#endif
        }

        public void Dispose() { }

#if UNITY_IOS && !UNITY_EDITOR
        [DllImport("__Internal")]
        private static extern int ivx_arkit_collab_supported();
        [DllImport("__Internal")]
        private static extern string ivx_arkit_collab_capture(string roomLabel);
        [DllImport("__Internal")]
        private static extern string ivx_arkit_collab_resolve(string token, int timeoutMs);
        [DllImport("__Internal")]
        private static extern int ivx_arkit_collab_confidence();
#endif

        [Serializable]
        private class CaptureResult
        {
            public string anchor_id;
            public string b64;
            public bool ok;
            public string detail;
            public string shareplay_session;
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
