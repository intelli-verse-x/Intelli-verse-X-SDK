// Viseme / blendshape data-channel protocol — TypeScript mirror of
// schemas/avatar/viseme_v1.proto.
//
// Used by:
//   • Web LiveKit adapter (`@intelliversex/multiplayer/adapters/livekit-web`)
//   • Discord Activities renderer
//   • Three.js / Babylon glTF renderers
//   • Bot-harness QA assertions
//
// The wire format on the LiveKit data channel labelled "viseme.v1" is
// either Protocol Buffers (Phase-4 default) or JSON (debug mode); both
// decode to one of the typed payloads below.

import { IVXBlendshapeProfile } from "./types";

/** Source of truth for a viseme stream — mirrors `ivx.avatar.v1.VisemeSource`. */
export enum IVXVisemeSource {
  Unspecified = 0,
  Agent = 1,
  UserFace = 2,
  UserTts = 3,
  Fallback = 4,
}

export interface IVXVisemeFrame {
  user_id: string;
  /** ARKit-52 / OVR-60 / VRM-69 weights as uint8 (0..255). */
  blendshapes: Uint8Array;
  profile: IVXBlendshapeProfile;
  audio_seq: number;
  audio_ts_ms: number;
  /** 0..100 — used by renderer to amplify expressiveness. */
  intensity_pct: number;
  frame_seq: number;
}

export interface IVXPhonemeFrame {
  user_id: string;
  /** 0..14 (sil, PP, FF, TH, DD, kk, CH, SS, nn, RR, aa, E, ih, oh, ou). */
  viseme: number;
  /** 0..100 weight of the viseme blend. */
  weight_pct: number;
  audio_seq: number;
  audio_ts_ms: number;
  frame_seq: number;
}

export interface IVXFacialExpressionFrame {
  user_id: string;
  brow_inner_up_pct: number;
  brow_outer_up_pct: number;
  eye_blink_l_pct: number;
  eye_blink_r_pct: number;
  /** Hundredths of a degree. */
  gaze_yaw_centideg: number;
  gaze_pitch_centideg: number;
  frame_seq: number;
}

export interface IVXVisemeStreamHeader {
  user_id: string;
  /** LiveKit audio track ID for the TTS line. */
  track_id: string;
  source: IVXVisemeSource;
  expected_frames: number;
  /** Audio sample rate (24 000 typical, must match LiveKit publisher). */
  sample_rate_hz: number;
  /** Animation frame rate (60 default, capped to 60). */
  frame_hz: number;
  profile: IVXBlendshapeProfile;
  /** Unique line ID — repeated across header/frames/footer. */
  line_id: number;
}

export interface IVXVisemeStreamFooter {
  user_id: string;
  line_id: number;
  frames_sent: number;
  final_audio_seq: number;
}

/** Discriminated wire packet — single envelope for all 5 payload kinds. */
export type IVXVisemePacket =
  | { kind: "header";     header: IVXVisemeStreamHeader }
  | { kind: "frame";      frame: IVXVisemeFrame }
  | { kind: "phoneme";    phoneme: IVXPhonemeFrame }
  | { kind: "expression"; expression: IVXFacialExpressionFrame }
  | { kind: "footer";     footer: IVXVisemeStreamFooter };

/** Receiver surface — mirrors `IIVXVisemeStream` in C#. */
export interface IIVXVisemeStream {
  readonly isActive: boolean;
  readonly currentLineId: number;
  readonly lastIntensityPct: number;

  on(event: "header",     cb: (h: IVXVisemeStreamHeader) => void): () => void;
  on(event: "frame",      cb: (f: IVXVisemeFrame) => void): () => void;
  on(event: "phoneme",    cb: (p: IVXPhonemeFrame) => void): () => void;
  on(event: "expression", cb: (e: IVXFacialExpressionFrame) => void): () => void;
  on(event: "footer",     cb: (f: IVXVisemeStreamFooter) => void): () => void;

  /** Decode + dispatch a raw LiveKit data-channel payload. */
  dispatch(bytes: Uint8Array, isJson?: boolean): void;

  reset(reason: string): void;
  dispose(): void;
}

/** JSON payload shape for debug mode — flat map of `kind` to body. */
interface JsonEnvelope {
  kind: "header" | "frame" | "phoneme" | "expression" | "footer";
  header?: IVXVisemeStreamHeader & { blendshapes?: never };
  frame?: Omit<IVXVisemeFrame, "blendshapes"> & { blendshapes?: number[] };
  phoneme?: IVXPhonemeFrame;
  expression?: IVXFacialExpressionFrame;
  footer?: IVXVisemeStreamFooter;
}

/**
 * Decode a JSON envelope into a typed `IVXVisemePacket`. Used in
 * developer / debug mode where the agent worker publishes JSON
 * instead of proto bytes for easier inspection.
 */
export function decodeVisemeJson(bytes: Uint8Array): IVXVisemePacket | null {
  let parsed: JsonEnvelope;
  try {
    parsed = JSON.parse(new TextDecoder().decode(bytes));
  } catch {
    return null;
  }
  switch (parsed.kind) {
    case "header":
      if (!parsed.header) return null;
      return { kind: "header", header: parsed.header };
    case "frame":
      if (!parsed.frame) return null;
      return {
        kind: "frame",
        frame: {
          ...parsed.frame,
          blendshapes: parsed.frame.blendshapes
            ? Uint8Array.from(parsed.frame.blendshapes)
            : new Uint8Array(0),
        } as IVXVisemeFrame,
      };
    case "phoneme":
      if (!parsed.phoneme) return null;
      return { kind: "phoneme", phoneme: parsed.phoneme };
    case "expression":
      if (!parsed.expression) return null;
      return { kind: "expression", expression: parsed.expression };
    case "footer":
      if (!parsed.footer) return null;
      return { kind: "footer", footer: parsed.footer };
    default:
      return null;
  }
}

/** LiveKit data-channel topic name for this protocol — keep in sync across SDKs. */
export const IVX_VISEME_TOPIC = "viseme.v1" as const;
