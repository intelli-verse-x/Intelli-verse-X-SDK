// IVXVoiceTokenClient (JS/TS) — typed helper around the kernel's
// `mp_voice_token` Nakama RPC. Returns the same `IVXVoiceSessionToken`
// shape every adapter uses (Unity/UE5/Godot/Swift), so a Three.js or
// Babylon game can do:
//
//   import { Client } from "@heroiclabs/nakama-js";
//   import { mintVoiceToken } from "@intelliversex/multiplayer";
//
//   const token = await mintVoiceToken({
//     client, session,
//     matchId: match.match_id,
//     canPublish: true, canSubscribe: true, spatial: false,
//   });
//   await voice.connect(token);
//
// The wire shape mirrors `nakama/data/modules/src/multiplayer-kernel/
// voice-providers/index.ts → rpcVoiceToken` and the typed Unity helper
// `Assets/Intelli-verse-X-SDK/MultiplayerKernel/Voice/IVXVoiceTokenClient.cs`.
//
// Errors are normalised: empty payload → "voice_unconfigured", session
// expired → "session_expired", JSON parse failure → "decode_failed".

import type { Client, Session } from "@heroiclabs/nakama-js";

import { type IVXVoiceSessionToken, IVXVoiceProvider } from "./types";

const RPC_VOICE_TOKEN = "mp_voice_token";

export interface IVXMintVoiceTokenRequest {
  client: Client;
  session: Session;
  matchId: string;
  canPublish?:   boolean; // default true
  canSubscribe?: boolean; // default true
  spatial?:      boolean; // default false (multi-human Party uses spatial:true)
  region?:       string;  // optional override; kernel picks closest if empty
}

export class IVXVoiceTokenError extends Error {
  constructor(public readonly code: string, message: string) {
    super(`[mintVoiceToken] ${code}: ${message}`);
    this.name = "IVXVoiceTokenError";
  }
}

export async function mintVoiceToken(req: IVXMintVoiceTokenRequest): Promise<IVXVoiceSessionToken> {
  if (!req || !req.client)  throw new IVXVoiceTokenError("bad_args", "client required");
  if (!req.session)         throw new IVXVoiceTokenError("bad_args", "session required");
  if (!req.matchId)         throw new IVXVoiceTokenError("bad_args", "matchId required");

  // nakama-js `Session.isexpired(now)` is the documented check; some
  // builds expose `.expires_at`. Fall back gracefully.
  const isExpired = typeof (req.session as { isexpired?: (n: number) => boolean }).isexpired === "function"
    ? (req.session as { isexpired: (n: number) => boolean }).isexpired(Math.floor(Date.now() / 1000))
    : false;
  if (isExpired) {
    throw new IVXVoiceTokenError("session_expired", "Nakama session expired; refresh before minting voice token");
  }

  const payload = {
    match_id:      req.matchId,
    can_publish:   req.canPublish   ?? true,
    can_subscribe: req.canSubscribe ?? true,
    spatial:       req.spatial      ?? false,
    region:        req.region       ?? undefined,
  };

  let resp: { payload?: string | object } | undefined;
  try {
    resp = await req.client.rpc(req.session, RPC_VOICE_TOKEN, payload);
  } catch (e) {
    throw new IVXVoiceTokenError("rpc_failed", (e as Error).message);
  }

  if (!resp || !resp.payload) {
    throw new IVXVoiceTokenError("voice_unconfigured", "kernel returned empty payload (LiveKit env vars missing or feature flag off?)");
  }

  let parsed: IVXVoiceSessionToken;
  try {
    parsed = (typeof resp.payload === "string"
      ? JSON.parse(resp.payload)
      : resp.payload) as IVXVoiceSessionToken;
  } catch (e) {
    throw new IVXVoiceTokenError("decode_failed", (e as Error).message);
  }

  if (!parsed.token || !parsed.url) {
    throw new IVXVoiceTokenError("invalid_token", `kernel returned token without url/token fields: ${JSON.stringify(parsed)}`);
  }
  // Backfill provider for callers that pre-date the v1.1 schema; default
  // to LiveKit since that's the only first-class provider today.
  if (parsed.provider == null) parsed.provider = IVXVoiceProvider.LiveKit;
  return parsed;
}
