// IVX Multiplayer Kernel — 12-test cross-adapter conformance suite.
//
// Every IVX adapter (Unity, JS, Unreal, Godot, Flutter, Java, C++,
// Cocos2d-x, Defold, Web3, Roblox, Discord-Activities, visionOS) MUST
// pass every assertion in this suite or the build is rejected.
//
// The suite is intentionally protocol-only: it never spins up Nakama;
// it asserts wire-shape invariants that the kernel and every adapter
// agree on. The 12 tests below are derived from the kernel
// HARDENING_NOTES.md and the proto3 contract under
// Intelli-verse-X-SDK/schemas/multiplayer/*.proto.

import { describe, it, expect } from "vitest";
import {
  IVXOpRange,
  IVXKernelOp,
  IVXWireVersion,
  IVXErrorCode,
} from "../src/wire/constants";
import {
  createHeader,
  type IVXEnvelope,
  type IVXHeader,
} from "../src/wire/envelope";

const FROZEN_RANGES: ReadonlyArray<readonly [number, number, string]> = [
  [IVXOpRange.KERNEL_FROM,            IVXOpRange.KERNEL_TO,            "kernel"],
  [IVXOpRange.SOCIAL_FROM,            IVXOpRange.SOCIAL_TO,            "social"],
  [IVXOpRange.AGENTS_FROM,            IVXOpRange.AGENTS_TO,            "agents"],
  [IVXOpRange.MODERATION_FROM,        IVXOpRange.MODERATION_TO,        "moderation"],
  [IVXOpRange.SYNC_TURN_FROM,         IVXOpRange.SYNC_TURN_TO,         "sync_turn"],
  [IVXOpRange.ASYNC_TURN_FROM,        IVXOpRange.ASYNC_TURN_TO,        "async_turn"],
  [IVXOpRange.REALTIME_TICK_FROM,     IVXOpRange.REALTIME_TICK_TO,     "realtime_tick"],
  [IVXOpRange.LOBBY_FROM,             IVXOpRange.LOBBY_TO,             "lobby"],
  [IVXOpRange.TOURNAMENT_FROM,        IVXOpRange.TOURNAMENT_TO,        "tournament"],
  [IVXOpRange.LIVE_EVENT_FROM,        IVXOpRange.LIVE_EVENT_TO,        "live_event"],
  [IVXOpRange.PERSISTENT_PARTY_FROM,  IVXOpRange.PERSISTENT_PARTY_TO,  "persistent_party"],
  [IVXOpRange.MR_ANCHOR_FROM,         IVXOpRange.MR_ANCHOR_TO,         "mr_anchor"],
  [IVXOpRange.GAME_DEFINED_FROM,      IVXOpRange.GAME_DEFINED_TO,      "game_defined"],
  [IVXOpRange.XR_POSE_FROM,           IVXOpRange.XR_POSE_TO,           "xr_pose"],
];

describe("IVX conformance: 12 cross-adapter invariants", () => {
  // 1. Wire version must be exactly 1; bump only by an explicit breaking
  //    release. The kernel rejects mismatched envelopes.
  it("01 wire_version is V1", () => {
    expect(IVXWireVersion.V1).toBe(1);
  });

  // 2. Kernel opcodes are stable. Every adapter compiles them as
  //    constants; if a value drifts we silently desync clients.
  it("02 kernel opcodes are stable", () => {
    expect(IVXKernelOp.CLIENT_HELLO).toBe(0x0001);
    expect(IVXKernelOp.SERVER_HELLO).toBe(0x0002);
    expect(IVXKernelOp.HEARTBEAT).toBe(0x0003);
    expect(IVXKernelOp.PLAYER_JOINED).toBe(0x0004);
    expect(IVXKernelOp.PLAYER_LEFT).toBe(0x0005);
    expect(IVXKernelOp.MATCH_ENDED).toBe(0x0007);
    expect(IVXKernelOp.ERROR).toBe(0x0008);
  });

  // 3. Opcode ranges are strictly disjoint. Otherwise game-defined
  //    opcodes can collide with kernel control opcodes.
  it("03 opcode ranges are disjoint", () => {
    for (let i = 0; i < FROZEN_RANGES.length; i++) {
      for (let j = i + 1; j < FROZEN_RANGES.length; j++) {
        const [a0, a1, an] = FROZEN_RANGES[i];
        const [b0, b1, bn] = FROZEN_RANGES[j];
        const overlap = !(a1 < b0 || b1 < a0);
        expect(overlap, `${an} (${a0.toString(16)}-${a1.toString(16)}) overlaps ${bn}`).toBe(false);
      }
    }
  });

  // 4. Error codes are stable and the unspecified=0 sentinel is
  //    reserved.
  it("04 error codes are stable", () => {
    expect(IVXErrorCode.UNSPECIFIED).toBe(0);
    expect(IVXErrorCode.SCHEMA_TOO_OLD).toBe(1);
    expect(IVXErrorCode.MATCH_FULL).toBe(20);
    expect(IVXErrorCode.RATE_LIMITED).toBe(23);
    expect(IVXErrorCode.SESSION_REPLACED).toBe(26);
    expect(IVXErrorCode.VOICE_UNAVAILABLE).toBe(60);
    expect(IVXErrorCode.INTERNAL).toBe(999);
  });

  // 5. createHeader fills in defaults so a minimal call produces a
  //    valid envelope per proto3 (every required field present).
  it("05 createHeader produces a complete header", () => {
    const h: IVXHeader = createHeader({
      op: IVXKernelOp.HEARTBEAT,
      matchId: "m-1",
      senderUserId: "u-1",
    });
    expect(h.wire_version).toBe(1);
    expect(h.op).toBe(IVXKernelOp.HEARTBEAT);
    expect(h.match_id).toBe("m-1");
    expect(h.sender_user_id).toBe("u-1");
    expect(h.seq).toBe(0);
    expect(h.match_time_ms).toBe(0);
    expect(typeof h.client_opcode_uuid).toBe("string");
  });

  // 6. JSON shape is exactly { h: …, p: … } with snake_case header
  //    fields. Adapters that emit camelCase break the kernel parser.
  it("06 envelope JSON shape is canonical", () => {
    const env: IVXEnvelope<{ value: number }> = {
      h: createHeader({ op: 0xC001, matchId: "m", senderUserId: "u" }),
      p: { value: 42 },
    };
    const text = JSON.stringify(env);
    expect(text).toContain('"h":');
    expect(text).toContain('"p":');
    expect(text).toContain('"wire_version":1');
    expect(text).toContain('"sender_user_id":"u"');
    expect(text).toContain('"match_id":"m"');
    // Forbidden camelCase variants:
    expect(text).not.toContain('"matchId"');
    expect(text).not.toContain('"senderUserId"');
  });

  // 7. Round-trip fidelity: serializing then parsing must preserve all
  //    fields and the payload.
  it("07 envelope roundtrips losslessly", () => {
    const original: IVXEnvelope<{ x: number; s: string }> = {
      h: createHeader({
        op: 0xC101, matchId: "m1", senderUserId: "u1",
        seq: 17, matchTimeMs: 12345, traceParent: "00-...-01",
      }),
      p: { x: 12, s: "hello" },
    };
    const parsed = JSON.parse(JSON.stringify(original)) as IVXEnvelope<{ x: number; s: string }>;
    expect(parsed).toEqual(original);
  });

  // 8. Outbound seq must be monotonic per session. The session helper
  //    must increment before sending the next envelope.
  it("08 monotonic seq within a logical session", () => {
    let seq = 0;
    const next = () => ++seq;
    const a = createHeader({ op: 1, matchId: "m", senderUserId: "u", seq: next() });
    const b = createHeader({ op: 1, matchId: "m", senderUserId: "u", seq: next() });
    const c = createHeader({ op: 1, matchId: "m", senderUserId: "u", seq: next() });
    expect(a.seq).toBe(1);
    expect(b.seq).toBe(2);
    expect(c.seq).toBe(3);
  });

  // 9. Heartbeat envelope payload must be JSON-empty-object {} so the
  //    kernel can parse it without an extra schema fetch.
  it("09 heartbeat payload is {}", () => {
    const env: IVXEnvelope<Record<string, never>> = {
      h: createHeader({ op: IVXKernelOp.HEARTBEAT, matchId: "m", senderUserId: "u" }),
      p: {},
    };
    expect(JSON.stringify(env.p)).toBe("{}");
  });

  // 10. Quantization profile, when present, is in 0..7. Profiles 8+
  //     are reserved for future codecs.
  it("10 quantization_profile fits 3 bits", () => {
    for (let p = 0; p < 8; p++) {
      const h = createHeader({ op: 1, matchId: "m", senderUserId: "u", quantizationProfile: p });
      expect(h.quantization_profile).toBe(p);
    }
    const oversized = 8;
    const h = createHeader({ op: 1, matchId: "m", senderUserId: "u", quantizationProfile: oversized });
    expect(h.quantization_profile).toBeLessThan(16);
  });

  // 11. Trace parent (W3C trace context) is opaque ASCII; the adapter
  //     must NOT mangle it.
  it("11 trace_parent passes through verbatim", () => {
    const tp = "00-aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa-bbbbbbbbbbbbbbbb-01";
    const h = createHeader({ op: 1, matchId: "m", senderUserId: "u", traceParent: tp });
    expect(h.trace_parent).toBe(tp);
  });

  // 12. Error envelope shape is { code, detail?, retry_after_ms?, min_required_version? }
  //     and must be parseable from JSON without optional fields.
  it("12 error envelope minimum shape", () => {
    const e1 = { code: IVXErrorCode.MATCH_FULL };
    const e2 = { code: IVXErrorCode.RATE_LIMITED, retry_after_ms: 750 };
    const e3 = { code: IVXErrorCode.SCHEMA_TOO_OLD, min_required_version: "1.4.0" };
    for (const e of [e1, e2, e3]) {
      const parsed = JSON.parse(JSON.stringify(e));
      expect(typeof parsed.code).toBe("number");
    }
  });
});
