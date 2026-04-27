// JSON wire envelope shared with the TypeScript-runtime kernel.
//
// The proto3 contract (`schemas/multiplayer/envelope.proto`) is the source
// of truth for field names; the TS kernel runs on Goja which can't load
// google-protobuf JS, so both sides agree to ship JSON with the same
// field names. Go-backed templates (RealtimeTick, AvatarReplication) use
// binary protobuf — those clients should plug in `IVXBinaryCodec`
// (added later) instead of the JSON codec exposed here.

import { IVXWireVersion } from "./constants";

/**
 * Wire header. Every envelope carries one. Fields use snake_case to
 * match the proto3 wire format; we never re-case them when (de)serialising.
 */
export interface IVXHeader {
  wire_version: number;
  op: number;
  seq: number;
  match_time_ms: number;
  sender_user_id: string;
  match_id: string;
  client_opcode_uuid: string;
  quantization_profile?: number;
  delta_base_seq?: number;
  feature_flags?: number;
  trace_parent?: string;
}

/** Generic envelope = `{ h: header, p: payload }`. */
export interface IVXEnvelope<T = unknown> {
  h: IVXHeader;
  p: T;
}

export interface IVXError {
  code: number;
  detail?: string;
  retry_after_ms?: number;
  min_required_version?: string;
}

export function createHeader(opts: {
  op: number;
  matchId: string;
  senderUserId: string;
  seq?: number;
  matchTimeMs?: number;
  clientOpcodeUuid?: string;
  quantizationProfile?: number;
  featureFlags?: number;
  traceParent?: string;
}): IVXHeader {
  return {
    wire_version: IVXWireVersion.V1,
    op: opts.op,
    seq: opts.seq ?? 0,
    match_time_ms: opts.matchTimeMs ?? 0,
    sender_user_id: opts.senderUserId ?? "",
    match_id: opts.matchId,
    client_opcode_uuid: opts.clientOpcodeUuid ?? randomUuid(),
    quantization_profile: opts.quantizationProfile,
    feature_flags: opts.featureFlags,
    trace_parent: opts.traceParent,
  };
}

export function buildEnvelope<T>(opts: {
  op: number;
  payload: T;
  matchId: string;
  senderUserId: string;
  seq?: number;
  matchTimeMs?: number;
}): IVXEnvelope<T> {
  return {
    h: createHeader(opts),
    p: opts.payload,
  };
}

export function encodeEnvelope<T>(env: IVXEnvelope<T>): string {
  return JSON.stringify(env);
}

export function decodeEnvelope<T = unknown>(raw: string | Uint8Array): IVXEnvelope<T> | null {
  let s: string;
  if (typeof raw === "string") {
    s = raw;
  } else if (raw instanceof Uint8Array) {
    s = new TextDecoder().decode(raw);
  } else {
    return null;
  }
  if (!s) return null;
  try {
    const parsed = JSON.parse(s) as IVXEnvelope<T>;
    if (!parsed || typeof parsed !== "object" || !parsed.h) return null;
    return parsed;
  } catch {
    return null;
  }
}

function randomUuid(): string {
  if (typeof crypto !== "undefined" && typeof crypto.randomUUID === "function") {
    return crypto.randomUUID().replace(/-/g, "");
  }
  let s = "";
  for (let i = 0; i < 32; i++) {
    s += Math.floor(Math.random() * 16).toString(16);
  }
  return s;
}
