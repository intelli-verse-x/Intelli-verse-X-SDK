// IIVXVoice — public types for the JS multiplayer voice abstraction.
//
// Mirrors `Intelli-verse-X-SDK/schemas/multiplayer/services/voice.proto`.
// Keep types in sync with `Assets/.../MultiplayerKernel/API/IIVXVoice.cs`
// and `nakama/data/modules/src/multiplayer-kernel/voice.ts`.

export enum IVXVoiceProvider {
  Unspecified = 0,
  LiveKit     = 1,
  Agora       = 2,
  Twilio      = 3,
  Dolby       = 4,
  None        = 5,
}

export enum IVXVoiceCodec {
  Unspecified = 0,
  Opus        = 1,
  Aac         = 2,
}

export enum IVXVoiceMode {
  Off       = 0,
  Broadcast = 1,
  Spatial   = 2,
  Ptt       = 3,
}

export interface IVXVoiceSessionToken {
  provider: IVXVoiceProvider;
  token: string;
  room_id: string;
  identity: string;
  url: string;
  expires_at_ms: number;
  can_publish: boolean;
  can_subscribe: boolean;
  spatial: boolean;
  region: string;
  provider_opts?: Record<string, string>;
}

export interface IVXVoiceCapability {
  can_publish: boolean;
  can_subscribe: boolean;
  can_spatial: boolean;
  codecs: IVXVoiceCodec[];
  max_publishers: number;
  can_change_provider: boolean;
  can_passthrough_external: boolean;
  ptt_supported: boolean;
  broadcast_supported: boolean;
  spatial_supported: boolean;
}

export interface IVXSpeakerStateChanged {
  user_id: string;
  granted: boolean;
  muted_by_self: boolean;
  muted_by_kernel: boolean;
  floor_seconds_remaining: number;
  reason: string;
}

export interface IVXVoiceLevelsSample {
  user_id: string;
  talking_pct: number; // 0..100
  silent: boolean;
}

export interface IVXVoiceLevels {
  samples: IVXVoiceLevelsSample[];
  ts_ms: number;
}

export interface IVXPoseFrameRef {
  frame_id: string;
  ts_ms: number;
}

/** Generic listener cleanup token. */
export type IVXUnsubscribe = () => void;

export interface IIVXVoice {
  readonly provider: IVXVoiceProvider;
  readonly capability: IVXVoiceCapability;
  readonly currentMode: IVXVoiceMode;
  readonly isConnected: boolean;
  readonly isLocallyMuted: boolean;
  readonly hasFloor: boolean;

  on(event: "connection-changed", cb: (connected: boolean) => void): IVXUnsubscribe;
  on(event: "speaker-state-changed", cb: (e: IVXSpeakerStateChanged) => void): IVXUnsubscribe;
  on(event: "voice-levels", cb: (levels: IVXVoiceLevels) => void): IVXUnsubscribe;
  on(event: "voice-mode-changed", cb: (mode: IVXVoiceMode) => void): IVXUnsubscribe;
  on(event: "provider-failover", cb: (next: IVXVoiceProvider) => void): IVXUnsubscribe;
  on(event: "voice-unavailable", cb: (reason: string) => void): IVXUnsubscribe;

  connect(token: IVXVoiceSessionToken): Promise<void>;
  disconnect(): Promise<void>;
  setLocalMute(muted: boolean): Promise<void>;
  requestSpeaker(topicHint?: string): Promise<void>;
  releaseSpeaker(): Promise<void>;
  publishSpatialPosition(frame: IVXPoseFrameRef, x: number, y: number, z: number, yawDeg: number): Promise<void>;
  setVoiceMode(mode: IVXVoiceMode): Promise<void>;

  /** Internal: kernel forwards SpeakerStateChanged here. */
  __onKernelSpeakerStateChanged(ev: IVXSpeakerStateChanged): void;
  /** Internal: kernel forwards provider failover here. */
  __onKernelProviderFailover(next: IVXVoiceProvider): void;
}
