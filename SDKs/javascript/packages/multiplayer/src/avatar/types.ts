// IIVXAvatar — JS / TypeScript mirror of the cross-platform avatar surface.
//
// Schema source of truth: schemas/avatar/avatar_v1.proto.
// Spec:                   docs/multiplayer/avatar-interop.md.
//
// Web adapters (Three.js, Babylon, A-Frame, Discord Activities, native
// `<canvas>`) implement IIVXAvatar to drive a glTF avatar from incoming
// AvatarReplicationMatch frames. The kernel never trusts the client for
// authority — these adapters only render.

export enum IVXAvatarSource {
  Unspecified = 0,
  Ovr = 1,
  Vrm = 2,
  ReadyPlayerMe = 3,
  VisionOsPersona = 4,
  IvxNative = 5,
  FallbackBillboard = 6,
}

export enum IVXBlendshapeProfile {
  None = 0,
  Arkit52 = 1,
  Ovr60 = 2,
  Vrm69 = 3,
}

export enum IVXAvatarLOD {
  Full = 0,
  Mid = 1,
  Low = 2,
  Billboard = 3,
}

export interface IVXAvatarDescriptor {
  avatar_id: string;
  user_id: string;
  /** Always "ivx_humanoid_v1" in v1. */
  skeleton_profile: string;
  /** "arkit_52" | "ovr_60" | "vrm_69" | "none". */
  blendshape_profile: string;
  source: IVXAvatarSource;
  mesh_url: string;
  material_count?: number;
  lod_root_url?: string;
  max_lod?: number;
  fingerprint_sha256?: string;
  schema_version: number;
}

export interface IVXPoseQuantized {
  px_mm: number;
  py_mm: number;
  pz_mm: number;
  rot_packed: number;
  quant_profile?: number;
  ts_ms?: number;
  confidence_pct?: number;
}

export interface IVXAvatarCapability {
  has_humanoid_rig: boolean;
  has_blendshapes: boolean;
  has_finger_tracking: boolean;
  has_face_tracking: boolean;
  has_full_body_ik: boolean;
  best_blendshape_profile: IVXBlendshapeProfile;
  max_avatars_renderable: number;
  avatar_schema_versions: number[];
}

/**
 * Per-(match, user_id) avatar surface. One instance for the local user
 * (`isLocalAuthority = true`) and one for each remote user. The local
 * instance publishes head/hand/body/face/finger via the parent
 * IIVXMultiplayer session; remote instances render incoming frames.
 */
export interface IIVXAvatar {
  readonly descriptor: IVXAvatarDescriptor;
  readonly currentLOD: IVXAvatarLOD;
  readonly isLoaded: boolean;
  readonly isLocalAuthority: boolean;

  loadAsync(descriptor: IVXAvatarDescriptor, signal?: AbortSignal): Promise<void>;

  applyHeadPose(pose: IVXPoseQuantized): void;
  applyHandPose(isLeft: boolean, pose: IVXPoseQuantized, gripPct: number, triggerPct: number): void;
  applyBodyJoints(joints: IVXPoseQuantized[]): void;
  applyBlendshapes(weights: Uint8Array, profile: IVXBlendshapeProfile): void;
  applyFingerCurls(isLeft: boolean, curls: Uint8Array): void;

  /** Server-driven LOD change. Adapter MUST honor; clients never promote. */
  setLOD(lod: IVXAvatarLOD, reason: string): void;

  fallbackToBillboard(reason: string): void;

  on(event: "loaded", cb: () => void): () => void;
  on(event: "lod_changed", cb: (lod: IVXAvatarLOD, reason: string) => void): () => void;
  on(event: "load_failed", cb: (reason: string) => void): () => void;

  dispose(): void;
}
