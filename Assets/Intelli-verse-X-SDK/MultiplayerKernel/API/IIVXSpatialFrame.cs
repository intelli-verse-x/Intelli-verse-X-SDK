// IIVXSpatialFrame — adapter-side mirror of the server SpatialFrame service.
// Every XR template embeds spatial-frame state and clients use this surface
// to subscribe to frame switches, ack offers, and tag pose messages with
// the active frame_id.
//
// Wire contract: schemas/multiplayer/services/spatial.proto.
//
// Adapters install a concrete IIVXSpatialFrame implementation per session;
// engine bindings (Unity / WebXR / visionOS / Unreal) then translate frames
// to native coordinate systems at the boundary.

using System;
using Newtonsoft.Json;

namespace IntelliVerseX.MultiplayerKernel.API
{
    /// <summary>
    /// Canonical SpatialFrame kinds. Adapters MUST advertise the subset they
    /// support via <see cref="IVXSpatialCapability.SupportedFrames"/>; the
    /// kernel intersects across the room and picks the highest fallback
    /// chain entry everyone supports.
    /// </summary>
    public enum IVXSpatialFrameKind
    {
        Unspecified  = 0,
        KernelWorld  = 1,
        CloudAnchor  = 2,
        QrMarker     = 3,
        ImageMarker  = 4,
        LocalFloor   = 5,
        PcvrPseudo   = 6
    }

    [Serializable]
    public class IVXSpatialFrame
    {
        [JsonProperty("frame_id")]
        public string FrameId { get; set; } = string.Empty;

        [JsonProperty("kind")]
        public IVXSpatialFrameKind Kind { get; set; } = IVXSpatialFrameKind.Unspecified;

        [JsonProperty("provider")]
        public string Provider { get; set; } = string.Empty;

        [JsonProperty("vendor_token")]
        public string VendorToken { get; set; } = string.Empty;

        [JsonProperty("payload", NullValueHandling = NullValueHandling.Ignore)]
        public string PayloadBase64 { get; set; }

        [JsonProperty("issued_ms")]
        public long IssuedMs { get; set; }

        [JsonProperty("region")]
        public string Region { get; set; } = string.Empty;

        [JsonProperty("floor_height_m")]
        public float FloorHeightM { get; set; }

        [JsonProperty("forward_yaw_deg")]
        public float ForwardYawDeg { get; set; }

        [JsonProperty("relocalize_grace_ms")]
        public uint RelocalizeGraceMs { get; set; }
    }

    [Serializable]
    public class IVXSpatialCapability
    {
        [JsonProperty("supported_frames")]
        public IVXSpatialFrameKind[] SupportedFrames { get; set; } = Array.Empty<IVXSpatialFrameKind>();

        [JsonProperty("can_publish_anchor")]
        public bool CanPublishAnchor { get; set; }

        [JsonProperty("can_resolve_cloud_anchor")]
        public bool CanResolveCloudAnchor { get; set; }

        [JsonProperty("can_print_qr")]
        public bool CanPrintQr { get; set; }

        [JsonProperty("can_print_image_marker")]
        public bool CanPrintImageMarker { get; set; }

        /// <summary>"right" or "left".</summary>
        [JsonProperty("handedness")]
        public string Handedness { get; set; } = "right";

        /// <summary>"Y" or "Z".</summary>
        [JsonProperty("up_axis")]
        public string UpAxis { get; set; } = "Y";

        /// <summary>"-Z", "+Z", "+X", etc.</summary>
        [JsonProperty("forward_axis")]
        public string ForwardAxis { get; set; } = "-Z";

        /// <summary>Capability with handedness=right, Y-up, forward=-Z.
        /// Use this when in doubt; engines that ship Z-up or left-handed
        /// MUST set the relevant fields and the adapter MUST convert at
        /// the boundary.</summary>
        public static IVXSpatialCapability IvxCanonical(IVXSpatialFrameKind[] supported,
            bool canPublishAnchor = false,
            bool canResolveCloudAnchor = false,
            bool canPrintQr = false,
            bool canPrintImageMarker = false)
        {
            return new IVXSpatialCapability
            {
                SupportedFrames = supported ?? Array.Empty<IVXSpatialFrameKind>(),
                CanPublishAnchor = canPublishAnchor,
                CanResolveCloudAnchor = canResolveCloudAnchor,
                CanPrintQr = canPrintQr,
                CanPrintImageMarker = canPrintImageMarker,
                Handedness = "right",
                UpAxis = "Y",
                ForwardAxis = "-Z"
            };
        }
    }

    /// <summary>
    /// Subscriber surface for spatial-frame events on a match session.
    /// </summary>
    public interface IIVXSpatialFrame
    {
        IVXSpatialFrame CurrentFrame { get; }
        IVXSpatialFrame PendingFrame { get; }

        /// <summary>Server announced an offer; resolve it on this device and call <see cref="AckPendingFrameAsync"/>.</summary>
        event Action<IVXSpatialFrame> OnFrameOffered;

        /// <summary>Server committed a new frame. All pose messages must now use this frame_id.</summary>
        event Action<IVXSpatialFrame, string /* previousFrameId */> OnFrameSwitched;

        /// <summary>The currently active frame was lost (e.g. cloud anchor delocalized). Caller will receive a follow-up offer or fallback.</summary>
        event Action<IVXSpatialFrame, string /* reason */> OnFrameLost;

        /// <summary>
        /// Acknowledge the pending frame after the engine has resolved/relocalized.
        /// </summary>
        void AckPendingFrame(bool ok, string detail = null);

        /// <summary>
        /// Convenience: tag a pose payload with the active frame_id and the
        /// current server-clock timestamp before sending. XR templates
        /// MUST use this to prevent stale-frame poses leaking after switch.
        /// </summary>
        void StampPoseFrame(IVXPoseFrameRef destination);

        /// <summary>
        /// Returns true if `frameId` is the current frame OR the pending
        /// frame within its grace window (used by inbound pose handlers).
        /// </summary>
        bool IsAcceptable(string frameId);
    }

    [Serializable]
    public class IVXPoseFrameRef
    {
        [JsonProperty("frame_id")]
        public string FrameId { get; set; } = string.Empty;

        [JsonProperty("ts_ms")]
        public long TsMs { get; set; }
    }
}
