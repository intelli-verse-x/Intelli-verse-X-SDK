// LiveKit data-channel receiver for the viseme.v1 protocol.
//
// Drop-in stream that any web renderer (Three.js / Babylon / Discord
// Activities / native canvas) wires to `Room.on('dataReceived', …)`.
// Decodes the JSON envelope, fires typed events, and exposes a tiny
// helper that drives a glTF MorphTargetInfluences array directly so
// Three.js callers can do `receiver.driveMesh(headMesh)` and forget.

import {
  IIVXVisemeStream,
  IVXVisemeFrame,
  IVXPhonemeFrame,
  IVXFacialExpressionFrame,
  IVXVisemeStreamFooter,
  IVXVisemeStreamHeader,
  decodeVisemeJson,
  IVX_VISEME_TOPIC,
} from "./viseme";

type Cb<T> = (payload: T) => void;
type Unsub = () => void;

interface ListenerMap {
  header:     Set<Cb<IVXVisemeStreamHeader>>;
  frame:      Set<Cb<IVXVisemeFrame>>;
  phoneme:    Set<Cb<IVXPhonemeFrame>>;
  expression: Set<Cb<IVXFacialExpressionFrame>>;
  footer:     Set<Cb<IVXVisemeStreamFooter>>;
}

/** Optional binding from ARKit-52 index → glTF morph target index. */
export interface ArkitToMorphMap {
  [arkitIndex: number]: number;
}

export interface IVXLiveKitVisemeReceiverOptions {
  /** Diagnostics + console.warn() on out-of-order frames. */
  verbose?: boolean;
  /** Drop frames with a frame_seq lower than the last one we played. */
  rejectOutOfOrder?: boolean;
}

export class IVXLiveKitVisemeReceiver implements IIVXVisemeStream {
  isActive = false;
  currentLineId = 0;
  lastIntensityPct = 0;

  private listeners: ListenerMap = {
    header: new Set(),
    frame: new Set(),
    phoneme: new Set(),
    expression: new Set(),
    footer: new Set(),
  };
  private lastFrameSeq = 0;
  private droppedFrames = 0;
  private morphMap: ArkitToMorphMap | null = null;
  private morphTargetInfluences: number[] | null = null;

  constructor(private readonly opts: IVXLiveKitVisemeReceiverOptions = {}) {}

  on(event: "header",     cb: Cb<IVXVisemeStreamHeader>): Unsub;
  on(event: "frame",      cb: Cb<IVXVisemeFrame>): Unsub;
  on(event: "phoneme",    cb: Cb<IVXPhonemeFrame>): Unsub;
  on(event: "expression", cb: Cb<IVXFacialExpressionFrame>): Unsub;
  on(event: "footer",     cb: Cb<IVXVisemeStreamFooter>): Unsub;
  on(event: keyof ListenerMap, cb: (p: unknown) => void): Unsub {
    const set = this.listeners[event] as Set<(p: unknown) => void>;
    set.add(cb);
    return () => set.delete(cb);
  }

  /**
   * Bind a glTF mesh's `morphTargetInfluences` array. The receiver
   * will write blendshape weights directly to it on every frame —
   * skipping a per-frame allocation and keeping the renderer hot.
   */
  driveMesh(target: { morphTargetInfluences?: number[] }, arkitToMorph: ArkitToMorphMap): void {
    this.morphTargetInfluences = target.morphTargetInfluences ?? null;
    this.morphMap = arkitToMorph;
  }

  /**
   * Wire to `Room.on('dataReceived', (payload, _participant, _kind, topic) => receiver.onLiveKitData(payload, topic))`.
   * Filters by topic so other data channels don't leak into the
   * viseme stream.
   */
  onLiveKitData(payload: Uint8Array, topic?: string): void {
    if (topic && topic !== IVX_VISEME_TOPIC) return;
    this.dispatch(payload, true);
  }

  /**
   * One-line wiring against a live livekit-client `Room` — equivalent
   * to the Unity `IVXLiveKitVisemeBinder` and Unreal
   * `UIVXLiveKitVisemeStream::AttachToRoom`. Call once after the room
   * is connected; the returned function detaches the listener.
   *
   *   import { Room, RoomEvent } from 'livekit-client';
   *   const room = new Room(); await room.connect(url, token);
   *   const receiver = new IVXLiveKitVisemeReceiver();
   *   const detach = receiver.attachLiveKitRoom(room);
   *
   * Without this method the dev had to remember the exact event name
   * (`RoomEvent.DataReceived`) and the argument order. We accept any
   * EventEmitter-shaped object so the helper compiles even when
   * `livekit-client` isn't a hard dependency of the package.
   */
  attachLiveKitRoom(room: {
    on: (event: string, cb: (...args: unknown[]) => void) => void;
    off?: (event: string, cb: (...args: unknown[]) => void) => void;
  }): Unsub {
    const handler = (...args: unknown[]): void => {
      // livekit-client signature: (payload: Uint8Array, participant?, kind?, topic?: string)
      const payload = args[0];
      const topic   = args[3] as string | undefined;
      if (payload instanceof Uint8Array) {
        this.onLiveKitData(payload, topic);
      } else if (payload && (payload as { byteLength?: number }).byteLength != null) {
        this.onLiveKitData(new Uint8Array(payload as ArrayBuffer), topic);
      }
    };
    // 'dataReceived' is the runtime event name; `RoomEvent.DataReceived`
    // resolves to the same string in livekit-client ≥ 1.0.
    room.on("dataReceived", handler);
    return () => {
      try { room.off?.("dataReceived", handler); } catch { /* swallow */ }
    };
  }

  dispatch(bytes: Uint8Array, isJson?: boolean): void {
    // Phase-4 ships JSON-on-wire; binary proto is wired up separately.
    if (isJson === false) {
      // Reserved for future protobuf decoder.
      return;
    }
    const packet = decodeVisemeJson(bytes);
    if (!packet) return;
    switch (packet.kind) {
      case "header":     this.onHeader(packet.header); break;
      case "frame":      this.onFrame(packet.frame); break;
      case "phoneme":    this.onPhoneme(packet.phoneme); break;
      case "expression": this.onExpression(packet.expression); break;
      case "footer":     this.onFooter(packet.footer); break;
    }
  }

  reset(reason: string): void {
    this.isActive = false;
    this.currentLineId = 0;
    this.lastIntensityPct = 0;
    this.lastFrameSeq = 0;
    this.droppedFrames = 0;
    this.zeroOutMorphTargets();
    if (this.opts.verbose) console.info(`[viseme] reset reason=${reason}`);
  }

  dispose(): void {
    this.reset("dispose");
    for (const set of Object.values(this.listeners)) set.clear();
  }

  /** For QA dashboards — mirrors `DiagnosticsJson()` on the C# side. */
  diagnostics(): Record<string, unknown> {
    return {
      topic: IVX_VISEME_TOPIC,
      isActive: this.isActive,
      currentLineId: this.currentLineId,
      lastFrameSeq: this.lastFrameSeq,
      lastIntensityPct: this.lastIntensityPct,
      droppedFrames: this.droppedFrames,
    };
  }

  // ---- handlers --------------------------------------------------

  private onHeader(h: IVXVisemeStreamHeader): void {
    this.isActive = true;
    this.currentLineId = h.line_id;
    this.lastFrameSeq = 0;
    this.droppedFrames = 0;
    for (const cb of this.listeners.header) cb(h);
    if (this.opts.verbose)
      console.info(`[viseme] header line=${h.line_id} expected=${h.expected_frames} hz=${h.frame_hz}`);
  }

  private onFrame(f: IVXVisemeFrame): void {
    if (this.opts.rejectOutOfOrder !== false && f.frame_seq < this.lastFrameSeq) {
      this.droppedFrames++;
      if (this.opts.verbose) console.warn(`[viseme] drop ooo seq=${f.frame_seq} last=${this.lastFrameSeq}`);
      return;
    }
    this.lastFrameSeq = f.frame_seq;
    this.lastIntensityPct = f.intensity_pct;
    this.applyToMesh(f);
    for (const cb of this.listeners.frame) cb(f);
  }

  private onPhoneme(p: IVXPhonemeFrame): void {
    for (const cb of this.listeners.phoneme) cb(p);
  }

  private onExpression(e: IVXFacialExpressionFrame): void {
    for (const cb of this.listeners.expression) cb(e);
  }

  private onFooter(f: IVXVisemeStreamFooter): void {
    for (const cb of this.listeners.footer) cb(f);
    this.isActive = false;
    this.zeroOutMorphTargets();
    if (this.opts.verbose)
      console.info(`[viseme] footer line=${f.line_id} sent=${f.frames_sent} dropped=${this.droppedFrames}`);
  }

  // ---- mesh I/O --------------------------------------------------

  private applyToMesh(f: IVXVisemeFrame): void {
    if (!this.morphTargetInfluences || !this.morphMap) return;
    for (let arkitIdx = 0; arkitIdx < f.blendshapes.length; arkitIdx++) {
      const morphIdx = this.morphMap[arkitIdx];
      if (morphIdx == null || morphIdx < 0 || morphIdx >= this.morphTargetInfluences.length) continue;
      // ARKit weights are 0..255 quantized; glTF morph targets expect 0..1.
      this.morphTargetInfluences[morphIdx] = f.blendshapes[arkitIdx] / 255;
    }
  }

  private zeroOutMorphTargets(): void {
    if (!this.morphTargetInfluences) return;
    for (let i = 0; i < this.morphTargetInfluences.length; i++) {
      this.morphTargetInfluences[i] = 0;
    }
  }
}
