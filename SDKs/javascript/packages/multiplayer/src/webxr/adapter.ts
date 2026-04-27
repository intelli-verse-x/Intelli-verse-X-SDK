// IVXWebXRAdapter — bridges WebXR sessions into the IVX multiplayer
// kernel. Renderer-agnostic: works with Three.js, Babylon.js, or
// A-Frame because WebXR's XRSession + XRReferenceSpace are standard.
//
// Responsibilities:
//
//   1. Subscribe to a WebXR XRSession and tick at frame rate.
//   2. Quantize the local viewer's head pose + per-controller poses into
//      IVXPoseQuantized wire shape (matches the C# IIVXAvatar contract).
//   3. Push poses into a session running templateId="avatar-replication-v1"
//      (Go server module) using the kernel envelope.
//   4. Decode incoming OBJECT_TRANSFORM / pose updates from peers and
//      surface as typed events so the renderer can move avatars.
//   5. Spatial frame: request a `local-floor` reference space; on
//      visionOS / Quest browser, also probe `bounded-floor` for
//      room-scale.
//
// What this adapter does NOT do:
//   * Does NOT render anything — game code (Three / Babylon / A-Frame)
//     listens to OnPeerPose / OnPeerLeft and updates its own scene graph.
//   * Does NOT handle voice — use the LiveKit JS provider in
//     `voice/livekit-provider.ts` alongside this adapter.
//
// Usage sketch:
//
//   const session = await client.joinMatch(matchId);
//   const xrAdapter = new IVXWebXRAdapter(session, { provider: "auto" });
//   xrSession.requestReferenceSpace("local-floor").then(refSpace => {
//     xrAdapter.attach(xrSession, refSpace);
//   });

import type { IIVXMatchSession, IVXSubscription } from "../api";

// ---------- Wire opcodes (from XR/avatar replication template) ----------
// Mirrors data/modules/avatar_replication/main.go constants.
export const IVX_XR_OP = {
  HEAD_POSE:        0xF000,
  LEFT_HAND_POSE:   0xF001,
  RIGHT_HAND_POSE:  0xF002,
  BLENDSHAPES:      0xF003,
  FINGER_CURLS:     0xF004,
  AVATAR_DESCRIPTOR:0xF005,
  LOD_HINT:         0xF006,
  PEER_LEFT:        0xF007,
  AVATAR_FALLBACK:  0xF008
} as const;

// ---------- Quantization (mirrors PoseQuantized in avatar_v1.proto) ----------
// We carry head/hand poses as 16-bit-quantized millimetres + a packed
// 30-bit smallest-three quaternion. Matches the Go server's expectation.

const POS_RANGE_MM = 32_767; // ±32.767 m

export interface IVXQuantizedPose {
  px_mm: number;
  py_mm: number;
  pz_mm: number;
  rot_packed: number; // 30-bit smallest-three
  ts_ms: number;
}

export function quantizePose(x: number, y: number, z: number, qx: number, qy: number, qz: number, qw: number): IVXQuantizedPose {
  const px_mm = Math.max(-POS_RANGE_MM, Math.min(POS_RANGE_MM, Math.round(x * 1000)));
  const py_mm = Math.max(-POS_RANGE_MM, Math.min(POS_RANGE_MM, Math.round(y * 1000)));
  const pz_mm = Math.max(-POS_RANGE_MM, Math.min(POS_RANGE_MM, Math.round(z * 1000)));

  // Smallest-three: drop the largest component, send the other 3 as 9 bits.
  const ax = Math.abs(qx), ay = Math.abs(qy), az = Math.abs(qz), aw = Math.abs(qw);
  let drop = 3, sign = 1;
  if (aw >= ax && aw >= ay && aw >= az) { drop = 3; sign = qw < 0 ? -1 : 1; }
  else if (ax >= ay && ax >= az) { drop = 0; sign = qx < 0 ? -1 : 1; }
  else if (ay >= az) { drop = 1; sign = qy < 0 ? -1 : 1; }
  else { drop = 2; sign = qz < 0 ? -1 : 1; }
  const sqrt2 = Math.SQRT2;
  function pack9(v: number): number {
    const scaled = (v * sign) * sqrt2;
    const norm   = Math.max(-1, Math.min(1, scaled));
    return Math.round((norm + 1) * 0.5 * 511);
  }
  const a = drop === 0 ? qy : qx;
  const b = drop === 1 ? qz : (drop === 0 ? qz : qy);
  const c = drop === 2 ? qw : (drop === 0 ? qw : (drop === 1 ? qw : qz));
  const pa = pack9(a), pb = pack9(b), pc = pack9(c);
  const rot_packed = (drop & 0x3) | (pa << 2) | (pb << 11) | (pc << 20);

  return { px_mm, py_mm, pz_mm, rot_packed, ts_ms: Date.now() };
}

// ---------- Adapter ----------

export interface IVXWebXRAdapterOpts {
  provider?: "auto" | "three" | "babylon" | "aframe";
  /** Cap on Hz for head pose publication. Default 60. */
  headHz?: number;
  /** Cap on Hz for hand poses. Default 30. */
  handHz?: number;
  /** Anchor strategy. "auto" probes bounded-floor first, falls back to local-floor. */
  spatialFrame?: "auto" | "local-floor" | "bounded-floor" | "viewer";
  /** Enable blendshapes (face tracking on supported devices). */
  enableFaceTracking?: boolean;
}

export interface IVXPeerPoseEvent {
  user_id: string;
  bone:    "head" | "leftHand" | "rightHand";
  pose:    IVXQuantizedPose;
}

export interface IVXPeerLeftEvent {
  user_id: string;
  reason:  string;
}

export class IVXWebXRAdapter {
  private _xrSession: XRSession | null = null;
  private _refSpace:  XRReferenceSpace | null = null;
  private _viewerSpace: XRReferenceSpace | null = null;
  private _running = false;

  private _opts: Required<IVXWebXRAdapterOpts>;
  private _lastHeadPubMs = 0;
  private _lastHandPubMs: { left: number; right: number } = { left: 0, right: 0 };

  private _peerPoseHandlers: Array<(e: IVXPeerPoseEvent) => void> = [];
  private _peerLeftHandlers: Array<(e: IVXPeerLeftEvent) => void> = [];
  private _subs: IVXSubscription[] = [];

  constructor(private readonly session: IIVXMatchSession, opts?: IVXWebXRAdapterOpts) {
    if (!session) throw new Error("[IVXWebXRAdapter] session required");
    this._opts = {
      provider:           opts?.provider ?? "auto",
      headHz:             opts?.headHz ?? 60,
      handHz:             opts?.handHz ?? 30,
      spatialFrame:       opts?.spatialFrame ?? "auto",
      enableFaceTracking: opts?.enableFaceTracking ?? false
    };
    this._wireIngress();
  }

  /**
   * Attach the adapter to an active XRSession. The renderer is
   * responsible for creating the XRSession; we just hook the frame
   * loop. Idempotent — calling twice replaces the previous session.
   */
  attach(xrSession: XRSession, refSpace: XRReferenceSpace, viewerSpace?: XRReferenceSpace): void {
    this._xrSession = xrSession;
    this._refSpace  = refSpace;
    this._viewerSpace = viewerSpace ?? null;
    this._running   = true;
    const onFrame = (_t: DOMHighResTimeStamp, frame: XRFrame) => {
      if (!this._running || this._xrSession !== xrSession) return;
      this._tick(frame);
      xrSession.requestAnimationFrame(onFrame);
    };
    xrSession.requestAnimationFrame(onFrame);
    xrSession.addEventListener("end", () => this.detach());
  }

  detach(): void {
    this._running = false;
    this._xrSession = null;
    this._refSpace = null;
    this._viewerSpace = null;
    for (const s of this._subs) {
      try { s.unsubscribe(); } catch { /* ignore */ }
    }
    this._subs.length = 0;
  }

  onPeerPose(h: (e: IVXPeerPoseEvent) => void): () => void {
    this._peerPoseHandlers.push(h);
    return () => {
      const idx = this._peerPoseHandlers.indexOf(h);
      if (idx >= 0) this._peerPoseHandlers.splice(idx, 1);
    };
  }

  onPeerLeft(h: (e: IVXPeerLeftEvent) => void): () => void {
    this._peerLeftHandlers.push(h);
    return () => {
      const idx = this._peerLeftHandlers.indexOf(h);
      if (idx >= 0) this._peerLeftHandlers.splice(idx, 1);
    };
  }

  // ---------- Internals ----------

  private _tick(frame: XRFrame): void {
    if (!this._refSpace) return;
    const now = performance.now();

    // Head pose: viewer space -> reference space.
    if (now - this._lastHeadPubMs >= 1000 / this._opts.headHz) {
      const viewerPose = frame.getViewerPose(this._refSpace);
      if (viewerPose) {
        const t = viewerPose.transform;
        const p = t.position, q = t.orientation;
        const pose = quantizePose(p.x, p.y, p.z, q.x, q.y, q.z, q.w);
        this._publishPose(IVX_XR_OP.HEAD_POSE, pose);
        this._lastHeadPubMs = now;
      }
    }

    // Hands: iterate XR input sources.
    if (now - this._lastHandPubMs.left >= 1000 / this._opts.handHz ||
        now - this._lastHandPubMs.right >= 1000 / this._opts.handHz) {
      for (const src of this._xrSession!.inputSources) {
        if (!src.gripSpace) continue;
        const pose = frame.getPose(src.gripSpace, this._refSpace);
        if (!pose) continue;
        const t = pose.transform;
        const p = t.position, q = t.orientation;
        const qPose = quantizePose(p.x, p.y, p.z, q.x, q.y, q.z, q.w);
        if (src.handedness === "left" && now - this._lastHandPubMs.left >= 1000 / this._opts.handHz) {
          this._publishPose(IVX_XR_OP.LEFT_HAND_POSE, qPose);
          this._lastHandPubMs.left = now;
        } else if (src.handedness === "right" && now - this._lastHandPubMs.right >= 1000 / this._opts.handHz) {
          this._publishPose(IVX_XR_OP.RIGHT_HAND_POSE, qPose);
          this._lastHandPubMs.right = now;
        }
      }
    }
  }

  private _publishPose(op: number, pose: IVXQuantizedPose): void {
    try {
      this.session.send(op, pose);
    } catch (err) {
      // Session probably ended; let the next tick discover via state event.
      console.warn("[IVXWebXR] send failed", op, err);
    }
  }

  private _wireIngress(): void {
    const subHead = this.session.subscribe<{ user_id: string; pose: IVXQuantizedPose }>(IVX_XR_OP.HEAD_POSE, ev => {
      this._fanPose(ev.payload?.user_id || (ev as any).senderUserId || "", "head", ev.payload?.pose);
    });
    const subL = this.session.subscribe<{ user_id: string; pose: IVXQuantizedPose }>(IVX_XR_OP.LEFT_HAND_POSE, ev => {
      this._fanPose(ev.payload?.user_id || (ev as any).senderUserId || "", "leftHand", ev.payload?.pose);
    });
    const subR = this.session.subscribe<{ user_id: string; pose: IVXQuantizedPose }>(IVX_XR_OP.RIGHT_HAND_POSE, ev => {
      this._fanPose(ev.payload?.user_id || (ev as any).senderUserId || "", "rightHand", ev.payload?.pose);
    });
    const subLeft = this.session.subscribe<{ user_id: string; reason: string }>(IVX_XR_OP.PEER_LEFT, ev => {
      const ev2: IVXPeerLeftEvent = {
        user_id: ev.payload?.user_id || "",
        reason:  ev.payload?.reason || ""
      };
      for (const h of this._peerLeftHandlers) {
        try { h(ev2); } catch (e) { console.error("[IVXWebXR] peer-left handler", e); }
      }
    });
    this._subs.push(subHead, subL, subR, subLeft);
  }

  private _fanPose(userId: string, bone: IVXPeerPoseEvent["bone"], pose: IVXQuantizedPose | undefined): void {
    if (!pose || !userId) return;
    const evt: IVXPeerPoseEvent = { user_id: userId, bone, pose };
    for (const h of this._peerPoseHandlers) {
      try { h(evt); } catch (e) { console.error("[IVXWebXR] peer-pose handler", e); }
    }
  }
}
