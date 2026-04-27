// IVXLiveKitVoiceProvider — JS/Web client implementation of IIVXVoice.
//
// Wraps `livekit-client` (https://www.npmjs.com/package/livekit-client) so a
// browser or React-Native app can join the same SFU room the Unity SDK
// joins. To avoid a hard dependency we resolve `livekit-client` at runtime;
// callers that haven't installed it get a `voice-unavailable` event and
// the rest of the IVX kernel runs without voice.

import {
  IIVXVoice,
  IVXVoiceProvider,
  IVXVoiceCapability,
  IVXVoiceCodec,
  IVXVoiceMode,
  IVXVoiceSessionToken,
  IVXSpeakerStateChanged,
  IVXVoiceLevels,
  IVXVoiceLevelsSample,
  IVXPoseFrameRef,
  IVXUnsubscribe,
} from "./types";

/* eslint-disable @typescript-eslint/no-explicit-any */
type Listener = (...args: any[]) => void;

export class IVXLiveKitVoiceProvider implements IIVXVoice {
  readonly provider = IVXVoiceProvider.LiveKit;
  readonly capability: IVXVoiceCapability;

  currentMode: IVXVoiceMode = IVXVoiceMode.Off;
  isConnected = false;
  isLocallyMuted = false;
  hasFloor = false;

  private _listeners = new Map<string, Set<Listener>>();
  private _room: any = null;
  private _localTrack: any = null;
  private _disposed = false;

  constructor(capability?: Partial<IVXVoiceCapability>) {
    this.capability = {
      can_publish: capability?.can_publish ?? true,
      can_subscribe: capability?.can_subscribe ?? true,
      can_spatial: capability?.can_spatial ?? true,
      codecs: capability?.codecs ?? [IVXVoiceCodec.Opus],
      max_publishers: capability?.max_publishers ?? 16,
      can_change_provider: capability?.can_change_provider ?? true,
      can_passthrough_external: capability?.can_passthrough_external ?? false,
      ptt_supported: capability?.ptt_supported ?? true,
      broadcast_supported: capability?.broadcast_supported ?? true,
      spatial_supported: capability?.spatial_supported ?? true,
    };
  }

  on(event: string, cb: Listener): IVXUnsubscribe {
    let set = this._listeners.get(event);
    if (!set) { set = new Set(); this._listeners.set(event, set); }
    set.add(cb);
    return () => set!.delete(cb);
  }

  private _emit(event: string, ...args: any[]) {
    const set = this._listeners.get(event);
    if (!set) return;
    for (const fn of set) {
      try { fn(...args); } catch (e) { /* listener errors must not break voice */ }
    }
  }

  async connect(token: IVXVoiceSessionToken): Promise<void> {
    if (token.provider !== IVXVoiceProvider.LiveKit) {
      // Caller may have failed-over; still attempt LiveKit connect so the
      // adapter can be reused, but warn loudly.
      // eslint-disable-next-line no-console
      console.warn("[IVXLiveKitVoiceProvider] token provider mismatch:", token.provider);
    }
    if (!token.token || !token.url) {
      this._emit("voice-unavailable", "livekit_token_missing");
      return;
    }

    // Lazy resolve livekit-client. Works both with bundlers (rollup, vite,
    // esbuild) and Node.js/Deno via dynamic import.
    let lkMod: any;
    try {
      lkMod = await import("livekit-client").catch(() => null);
    } catch {
      lkMod = null;
    }
    if (!lkMod) {
      this._emit("voice-unavailable", "livekit_sdk_not_installed");
      return;
    }

    const { Room, RoomEvent, ConnectionState } = lkMod;
    this._room = new Room({ adaptiveStream: true, dynacast: true });

    this._room.on(RoomEvent.ConnectionStateChanged, (state: any) => {
      this.isConnected = state === ConnectionState.Connected;
      this._emit("connection-changed", this.isConnected);
      if (state === ConnectionState.Disconnected) {
        this._emit("voice-unavailable", "livekit_disconnected");
      }
    });

    this._room.on(RoomEvent.ActiveSpeakersChanged, (speakers: any[]) => {
      const samples: IVXVoiceLevelsSample[] = speakers.map((p) => ({
        user_id: p.identity,
        talking_pct: Math.round(((p.audioLevel ?? 0) as number) * 100),
        silent: false,
      }));
      const levels: IVXVoiceLevels = { samples, ts_ms: Date.now() };
      this._emit("voice-levels", levels);
    });

    await this._room.connect(token.url, token.token, {
      autoSubscribe: token.can_subscribe,
    });
    this.isConnected = true;
    this._emit("connection-changed", true);

    if (token.can_publish) {
      try {
        const tracks = await lkMod.createLocalAudioTrack();
        this._localTrack = tracks;
        await this._room.localParticipant.publishTrack(tracks);
      } catch (e) {
        this._emit("voice-unavailable", "livekit_publish_failed");
      }
    }
  }

  async disconnect(): Promise<void> {
    try {
      if (this._localTrack) { try { this._localTrack.stop(); } catch {} this._localTrack = null; }
      if (this._room) { try { await this._room.disconnect(); } catch {}; this._room = null; }
    } finally {
      this.isConnected = false;
      this._emit("connection-changed", false);
    }
  }

  async setLocalMute(muted: boolean): Promise<void> {
    this.isLocallyMuted = muted;
    if (this._localTrack && typeof this._localTrack.mute === "function") {
      try { muted ? await this._localTrack.mute() : await this._localTrack.unmute(); } catch {}
    }
  }

  async requestSpeaker(_topicHint?: string): Promise<void> {
    // Floor is kernel-authoritative. Caller signals via OP_CONV_SPEAKER_REQUEST.
  }

  async releaseSpeaker(): Promise<void> {
    // No-op at LiveKit layer.
  }

  async publishSpatialPosition(frame: IVXPoseFrameRef, x: number, y: number, z: number, yawDeg: number): Promise<void> {
    if (!this._room?.localParticipant) return;
    const payload = JSON.stringify({ frame: frame.frame_id, x, y, z, yaw: yawDeg, ts: frame.ts_ms });
    const data = new TextEncoder().encode(payload);
    try {
      await this._room.localParticipant.publishData(data, /* lossy */ 1);
    } catch {
      /* lossy data publish is best-effort */
    }
  }

  async setVoiceMode(mode: IVXVoiceMode): Promise<void> {
    this.currentMode = mode;
    this._emit("voice-mode-changed", mode);
  }

  __onKernelSpeakerStateChanged(ev: IVXSpeakerStateChanged): void {
    this.hasFloor = !!ev.granted;
    this._emit("speaker-state-changed", ev);
  }

  __onKernelProviderFailover(next: IVXVoiceProvider): void {
    this._emit("provider-failover", next);
    void this.disconnect();
  }
}
