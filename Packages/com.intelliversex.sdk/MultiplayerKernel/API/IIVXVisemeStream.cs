// IIVXVisemeStream — Phase-4 viseme/blendshape data-channel surface.
//
// The Content-Factory's server-rendered avatar (LiveKit Egress + agent
// worker) publishes a TTS audio track AND a parallel VisemePacket stream
// on a LiveKit data channel labelled "viseme.v1". This file declares the
// C# types and the receiver interface the Unity / WebGL adapters
// implement to drive a local skinned mesh from those packets without
// having to subscribe to the (much larger) video track.
//
// Wire contract: schemas/avatar/viseme_v1.proto.

using System;
using Newtonsoft.Json;

namespace IntelliVerseX.MultiplayerKernel.API
{
    /// <summary>Source of the viseme stream — mirrors <c>ivx.avatar.v1.VisemeSource</c>.</summary>
    public enum IVXVisemeSource
    {
        Unspecified = 0,
        Agent       = 1,
        UserFace    = 2,
        UserTts     = 3,
        Fallback    = 4
    }

    /// <summary>Single 16 ms blendshape frame.</summary>
    [Serializable]
    public class IVXVisemeFrame
    {
        [JsonProperty("user_id")]        public string UserId        { get; set; } = string.Empty;
        [JsonProperty("blendshapes")]    public byte[] Blendshapes   { get; set; } = Array.Empty<byte>();
        [JsonProperty("profile")]        public IVXBlendshapeProfile Profile { get; set; } = IVXBlendshapeProfile.Arkit52;
        [JsonProperty("audio_seq")]      public ulong  AudioSeq      { get; set; }
        [JsonProperty("audio_ts_ms")]    public long   AudioTsMs     { get; set; }
        [JsonProperty("intensity_pct")]  public uint   IntensityPct  { get; set; } = 100;
        [JsonProperty("frame_seq")]      public uint   FrameSeq      { get; set; }
    }

    /// <summary>Coarse 14-viseme phoneme frame.</summary>
    [Serializable]
    public class IVXPhonemeFrame
    {
        [JsonProperty("user_id")]    public string UserId    { get; set; } = string.Empty;
        [JsonProperty("viseme")]     public uint   Viseme    { get; set; } // 0..14
        [JsonProperty("weight_pct")] public uint   WeightPct { get; set; } // 0..100
        [JsonProperty("audio_seq")]  public ulong  AudioSeq  { get; set; }
        [JsonProperty("audio_ts_ms")]public long   AudioTsMs { get; set; }
        [JsonProperty("frame_seq")]  public uint   FrameSeq  { get; set; }
    }

    /// <summary>Eye + brow expression frame, typically 30 Hz.</summary>
    [Serializable]
    public class IVXFacialExpressionFrame
    {
        [JsonProperty("user_id")]              public string UserId            { get; set; } = string.Empty;
        [JsonProperty("brow_inner_up_pct")]    public uint BrowInnerUpPct      { get; set; }
        [JsonProperty("brow_outer_up_pct")]    public uint BrowOuterUpPct      { get; set; }
        [JsonProperty("eye_blink_l_pct")]      public uint EyeBlinkLPct        { get; set; }
        [JsonProperty("eye_blink_r_pct")]      public uint EyeBlinkRPct        { get; set; }
        [JsonProperty("gaze_yaw_centideg")]    public int  GazeYawCentideg     { get; set; }
        [JsonProperty("gaze_pitch_centideg")]  public int  GazePitchCentideg   { get; set; }
        [JsonProperty("frame_seq")]            public ulong FrameSeq           { get; set; }
    }

    /// <summary>TTS line header — emitted before any frames for a given line.</summary>
    [Serializable]
    public class IVXVisemeStreamHeader
    {
        [JsonProperty("user_id")]         public string UserId          { get; set; } = string.Empty;
        [JsonProperty("track_id")]        public string TrackId         { get; set; } = string.Empty;
        [JsonProperty("source")]          public IVXVisemeSource Source { get; set; } = IVXVisemeSource.Agent;
        [JsonProperty("expected_frames")] public uint ExpectedFrames    { get; set; }
        [JsonProperty("sample_rate_hz")]  public uint SampleRateHz      { get; set; } = 24000;
        [JsonProperty("frame_hz")]        public uint FrameHz           { get; set; } = 60;
        [JsonProperty("profile")]         public IVXBlendshapeProfile Profile { get; set; } = IVXBlendshapeProfile.Arkit52;
        [JsonProperty("line_id")]         public ulong LineId           { get; set; }
    }

    /// <summary>TTS line tail — emitted after the last frame.</summary>
    [Serializable]
    public class IVXVisemeStreamFooter
    {
        [JsonProperty("user_id")]        public string UserId        { get; set; } = string.Empty;
        [JsonProperty("line_id")]        public ulong  LineId        { get; set; }
        [JsonProperty("frames_sent")]    public uint   FramesSent    { get; set; }
        [JsonProperty("final_audio_seq")]public ulong  FinalAudioSeq { get; set; }
    }

    /// <summary>
    /// Receiver surface. Engine adapters (Unity, WebGL renderer, Quest,
    /// visionOS) implement this to drive their facial rig from the
    /// LiveKit "viseme.v1" data channel. Headers are guaranteed before
    /// frames; footer is best-effort.
    /// </summary>
    public interface IIVXVisemeStream : IDisposable
    {
        /// <summary>True once a header has been received and renderer is primed.</summary>
        bool IsActive { get; }

        /// <summary>Current TTS line being driven (0 if none).</summary>
        ulong CurrentLineId { get; }

        /// <summary>Per-frame intensity — used by the renderer for envelope shaping.</summary>
        uint LastIntensityPct { get; }

        event Action<IVXVisemeStreamHeader> OnHeader;
        event Action<IVXVisemeFrame>        OnFrame;
        event Action<IVXPhonemeFrame>       OnPhoneme;
        event Action<IVXFacialExpressionFrame> OnExpression;
        event Action<IVXVisemeStreamFooter> OnFooter;

        /// <summary>Decode a raw LiveKit data-channel payload (proto bytes
        /// or JSON) and dispatch one of the OnXxx events. Adapters wire
        /// this to their LiveKit DataReceived callback.</summary>
        void Dispatch(ReadOnlyMemory<byte> bytes, bool isJson);

        /// <summary>Force a reset (e.g. on track-change or session end).</summary>
        void Reset(string reason);
    }
}
