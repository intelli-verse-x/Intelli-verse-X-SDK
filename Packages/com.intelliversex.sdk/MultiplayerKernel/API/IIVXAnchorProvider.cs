// IIVXAnchorProvider — pluggable cloud-spatial-anchor abstraction.
//
// Implementers: Meta Spatial Anchors (Quest/Quest Pro/Quest 3),
//               ARKit Collab + SharePlay (iOS/iPadOS),
//               visionOS Shared World Anchor (visionOS),
//               OpenXR XR_MSFT_spatial_anchor (HoloLens / WMR / Pico),
//               Azure Spatial Anchors (cross-vendor),
//               QR-marker fallback,
//               Image-marker fallback,
//               PCVR fake-floor fallback (for desktop spectators).
//
// Wire contract: schemas/multiplayer/templates/mixed_reality_anchor.proto.
// Match template: nakama/data/modules/src/multiplayer-kernel/templates/
//                 mixed-reality-anchor-match.ts (templateId
//                 "mixed-reality-anchor-v1").
//
// What this is NOT:
//   * NOT a positional tracker. Each provider returns the *anchor token*
//     and the client surfaces it through OP_ANCHOR_OFFER → OP_ANCHOR_RESOLVED.
//     The actual XR pose tracking is owned by the engine's XR subsystem
//     (XR Interaction Toolkit, Apple ARSession, Unreal XR_Tracking, etc.).

using System;
using System.Threading;
using System.Threading.Tasks;

namespace IntelliVerseX.MultiplayerKernel.API
{
    /// <summary>
    /// Identifies which anchor provider produced a token. Mirrors
    /// AnchorProvider in mixed_reality_anchor.proto.
    /// </summary>
    public enum IVXAnchorProvider
    {
        Unspecified     = 0,
        MetaShared      = 1,
        VisionOSShared  = 2,
        ARKitCollab     = 3,
        AzureSpatial    = 4,
        QRFallback      = 5,
        ImageMarker     = 6,
        PCVRFake        = 7
    }

    /// <summary>
    /// Outcome of a host-side scan. The token is opaque + vendor-specific;
    /// the kernel just relays it to peers verbatim.
    /// </summary>
    public struct IVXAnchorOffer
    {
        public string AnchorId;
        public IVXAnchorProvider Provider;
        public string ProviderToken;
        public byte[] FallbackQrPayload;     // optional
        public byte[] FallbackMarkerPayload; // optional
        public string RoomLabel;
        public string Region;
    }

    /// <summary>
    /// Outcome of a peer attempt to resolve a host's anchor offer locally.
    /// </summary>
    public struct IVXAnchorResolveResult
    {
        public bool Ok;
        public string FailureDetail;
        public uint AttemptIndex;
        public IVXAnchorProvider ProviderUsed;
    }

    /// <summary>
    /// Per-frame relocalization confidence after the anchor is shared.
    /// Surfaced over OP_RELOCALIZED so peers can hide objects when the
    /// anchor is shaky (confidence_pct &lt; 50).
    /// </summary>
    public struct IVXAnchorRelocalization
    {
        public string AnchorId;
        public uint ConfidencePct;
    }

    /// <summary>
    /// Pluggable cloud-anchor provider. Game code obtains a concrete
    /// implementation (MetaSpatialAnchorProvider, ARKitCollabAnchorProvider,
    /// etc.) and passes it to the MixedRealityAnchorMatch session.
    /// </summary>
    public interface IIVXAnchorProvider : IDisposable
    {
        IVXAnchorProvider Kind { get; }

        /// <summary>
        /// True only if the running device + entitlement set supports
        /// this provider. Surfaced into the SDK adapter's `Capability`
        /// negotiation so unsupported providers degrade gracefully.
        /// </summary>
        bool IsAvailable { get; }

        /// <summary>
        /// Host-side: scan the local room and produce an anchor offer
        /// token. Implementations MAY require user interaction (e.g.
        /// Meta's "share with friends" dialog).
        /// </summary>
        Task<IVXAnchorOffer> CreateAnchorAsync(string roomLabel, CancellationToken ct = default);

        /// <summary>
        /// Peer-side: attempt to resolve the host's anchor token locally.
        /// Implementations MUST honour `timeoutMs` and return a failure
        /// detail when the timeout fires (kernel will downgrade the
        /// participant after `anchor_resolve_timeout_ms`).
        /// </summary>
        Task<IVXAnchorResolveResult> TryResolveAsync(IVXAnchorOffer offer, int timeoutMs, CancellationToken ct = default);

        /// <summary>
        /// Best-effort relocalization probe. Called by the engine on a
        /// slow timer (e.g. 1 Hz) so the client can warn peers when its
        /// anchor confidence is dropping.
        /// </summary>
        IVXAnchorRelocalization Probe();

        /// <summary>
        /// Fired when the local anchor is invalidated (occlusion, room
        /// scan changed, host left the room, etc). Client should surface
        /// OP_ANCHOR_LOST to the kernel.
        /// </summary>
        event Action<string /*reason*/> OnAnchorLost;
    }

    /// <summary>
    /// Static helper for adapter implementations that want to build a
    /// minimal QR-fallback offer when no native provider is available
    /// (e.g. PCVR spectators or unsupported devices).
    /// </summary>
    public static class IVXAnchorFallback
    {
        public static IVXAnchorOffer BuildPcvrFakeOffer(string roomLabel = "")
            => new IVXAnchorOffer
            {
                AnchorId       = "pcvr-floor-fake-v1",
                Provider       = IVXAnchorProvider.PCVRFake,
                ProviderToken  = "fake-floor",
                RoomLabel      = string.IsNullOrEmpty(roomLabel) ? "PCVR Floor" : roomLabel,
                Region         = ""
            };

        public static IVXAnchorOffer BuildQrOffer(byte[] qrBytes, string roomLabel = "")
            => new IVXAnchorOffer
            {
                AnchorId          = "qr-" + (qrBytes != null ? qrBytes.Length.ToString() : "0"),
                Provider          = IVXAnchorProvider.QRFallback,
                ProviderToken     = "",
                FallbackQrPayload = qrBytes,
                RoomLabel         = string.IsNullOrEmpty(roomLabel) ? "QR Room" : roomLabel,
                Region            = ""
            };
    }
}
