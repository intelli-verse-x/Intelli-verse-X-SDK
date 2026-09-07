// IIVXAvatar — cross-platform avatar interop surface.
//
// One IIVXAvatar instance per user_id in a match (local + remote). Engine
// adapters (Unity built-in, URP/HDRP renderer, Meta Avatars SDK, RPM,
// VRM, visionOS Persona) implement this interface to drive an actual mesh
// + bones + blendshapes from incoming pose / blendshape frames.
//
// Wire contract: schemas/avatar/avatar_v1.proto. Spec:
//   docs/multiplayer/avatar-interop.md
//
// Lifecycle:
//   1. Match join → kernel emits PlayerJoined with AvatarDescriptor.
//   2. Adapter spawns `IIVXAvatar` and calls `LoadAsync(descriptor)`.
//   3. Until LoadAsync completes, adapter renders the billboard fallback.
//   4. AvatarReplicationMatch streams head/hand/body/face/finger pose +
//      LOD demotions; adapter drives the mesh.
//   5. PlayerLeft → adapter calls `Dispose()` and removes the avatar.

using System;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace IntelliVerseX.MultiplayerKernel.API
{
    /// <summary>
    /// Canonical avatar source as published in <see cref="IVXAvatarDescriptor.Source"/>.
    /// Mirror of <c>ivx.avatar.v1.AvatarSource</c>.
    /// </summary>
    public enum IVXAvatarSource
    {
        Unspecified         = 0,
        Ovr                 = 1,
        Vrm                 = 2,
        ReadyPlayerMe       = 3,
        VisionOsPersona     = 4,
        IvxNative           = 5,
        FallbackBillboard   = 6
    }

    /// <summary>
    /// Canonical blendshape profile id. Wire encoding for `bytes blendshapes`
    /// is fixed by profile. See docs/multiplayer/avatar-interop.md §3.
    /// </summary>
    public enum IVXBlendshapeProfile
    {
        None    = 0,
        Arkit52 = 1,
        Ovr60   = 2,
        Vrm69   = 3
    }

    /// <summary>
    /// LOD level driven by AvatarReplicationMatch.
    /// 0 = full, 1 = mid, 2 = low, 3 = billboard.
    /// </summary>
    public enum IVXAvatarLOD
    {
        Full      = 0,
        Mid       = 1,
        Low       = 2,
        Billboard = 3
    }

    [Serializable]
    public class IVXAvatarDescriptor
    {
        [JsonProperty("avatar_id")]         public string AvatarId         { get; set; } = string.Empty;
        [JsonProperty("user_id")]           public string UserId           { get; set; } = string.Empty;
        [JsonProperty("skeleton_profile")]  public string SkeletonProfile  { get; set; } = "ivx_humanoid_v1";
        [JsonProperty("blendshape_profile")]public string BlendshapeProfile{ get; set; } = "arkit_52";
        [JsonProperty("source")]            public IVXAvatarSource Source  { get; set; } = IVXAvatarSource.IvxNative;
        [JsonProperty("mesh_url")]          public string MeshUrl          { get; set; } = string.Empty;
        [JsonProperty("material_count")]    public uint   MaterialCount    { get; set; }
        [JsonProperty("lod_root_url")]      public string LodRootUrl       { get; set; } = string.Empty;
        [JsonProperty("max_lod")]           public uint   MaxLod           { get; set; } = 3;
        [JsonProperty("fingerprint_sha256")]public string FingerprintHex   { get; set; } = string.Empty;
        [JsonProperty("schema_version")]    public uint   SchemaVersion    { get; set; } = 1;
    }

    /// <summary>
    /// PoseQuantized — wire format for head / hand / body joints.
    /// Mirrors `PoseQuantized` proto.
    /// </summary>
    [Serializable]
    public class IVXPoseQuantized
    {
        [JsonProperty("px_mm")]          public int    PxMm          { get; set; }
        [JsonProperty("py_mm")]          public int    PyMm          { get; set; }
        [JsonProperty("pz_mm")]          public int    PzMm          { get; set; }
        [JsonProperty("rot_packed")]     public uint   RotPacked     { get; set; }
        [JsonProperty("quant_profile")]  public uint   QuantProfile  { get; set; } = 1;
        [JsonProperty("ts_ms")]          public long   TsMs          { get; set; }
        [JsonProperty("confidence_pct")] public uint   ConfidencePct { get; set; } = 100;
    }

    /// <summary>
    /// Per-platform capability flags an adapter advertises in OP_CLIENT_HELLO.
    /// </summary>
    [Serializable]
    public class IVXAvatarCapability
    {
        public bool HasHumanoidRig          { get; set; } = true;
        public bool HasBlendshapes          { get; set; } = true;
        public bool HasFingerTracking       { get; set; } = false;
        public bool HasFaceTracking         { get; set; } = false;
        public bool HasFullBodyIK           { get; set; } = false;
        public IVXBlendshapeProfile BestBlendshapeProfile { get; set; } = IVXBlendshapeProfile.Arkit52;
        public uint MaxAvatarsRenderable    { get; set; } = 16;
        public uint[] AvatarSchemaVersions  { get; set; } = new uint[] { 1 };
    }

    /// <summary>
    /// Cross-platform avatar surface. One instance per (match, user_id).
    /// </summary>
    public interface IIVXAvatar : IDisposable
    {
        IVXAvatarDescriptor Descriptor { get; }
        IVXAvatarLOD CurrentLOD        { get; }
        bool         IsLoaded          { get; }
        bool         IsLocalAuthority  { get; }

        /// <summary>Fired when the avatar mesh + skeleton + blendshapes
        /// finish loading and are ready to render.</summary>
        event Action OnLoaded;

        /// <summary>Fired when the kernel demotes / promotes this avatar's LOD.</summary>
        event Action<IVXAvatarLOD, string> OnLODChanged;

        /// <summary>Fired if mesh download / fingerprint check fails. Adapter
        /// falls back to billboard automatically before raising this.</summary>
        event Action<string> OnLoadFailed;

        /// <summary>Begin async load of mesh + skeleton from descriptor.</summary>
        Task LoadAsync(IVXAvatarDescriptor descriptor, CancellationToken ct = default);

        /// <summary>Apply incoming head pose. Quantized format.</summary>
        void ApplyHeadPose(IVXPoseQuantized pose);

        /// <summary>Apply incoming hand pose. `isLeft` selects side.</summary>
        void ApplyHandPose(bool isLeft, IVXPoseQuantized pose, uint gripPct, uint triggerPct);

        /// <summary>Apply incoming body joints (skeleton-v1 order, max 32).</summary>
        void ApplyBodyJoints(IVXPoseQuantized[] joints);

        /// <summary>Apply incoming blendshape weights (52 bytes for ARKit profile).</summary>
        void ApplyBlendshapes(byte[] blendshapes, IVXBlendshapeProfile profile);

        /// <summary>Apply incoming finger curls (15 bytes per hand).</summary>
        void ApplyFingerCurls(bool isLeft, byte[] curls);

        /// <summary>Server-driven LOD change. Adapter MUST swap mesh / drop
        /// channels accordingly. Clients must NEVER promote on their own.</summary>
        void SetLOD(IVXAvatarLOD lod, string reason);

        /// <summary>Force a billboard fallback (used on load failure or by
        /// kernel for license/moderation reasons).</summary>
        void FallbackToBillboard(string reason);
    }
}
