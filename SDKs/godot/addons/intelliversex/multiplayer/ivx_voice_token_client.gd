# IVXVoiceTokenClient — Godot 4 helper for the kernel's `mp_voice_token`
# RPC. Returns a Dictionary in the same shape the JS adapter exposes as
# IVXVoiceSessionToken, so a Godot game (no native LiveKit binding) can
# still mint a token and hand it to a third-party LiveKit GDExtension.
#
# Usage:
#
#   var client : IVXVoiceTokenClient = IVXVoiceTokenClient.new()
#   var token := await client.mint_async({
#       "client":   nakama_client,
#       "session":  nakama_session,
#       "match_id": session.match_id,
#       "spatial":  false,
#   })
#   if token.has("error"):
#       push_warning("voice token failed: %s" % token.error)
#   else:
#       livekit_extension.connect_to_room(token.url, token.token)
#
# Mirrors:
#   * Unity:  Assets/Intelli-verse-X-SDK/MultiplayerKernel/Voice/IVXVoiceTokenClient.cs
#   * UE5:    SDKs/unreal/Source/IntelliVerseX/Public/IVXVoiceTokenClient.h
#   * JS:     SDKs/javascript/packages/multiplayer/src/voice/token-client.ts

class_name IVXVoiceTokenClient
extends RefCounted

const RPC_VOICE_TOKEN := "mp_voice_token"

# req keys: client, session, match_id, can_publish?, can_subscribe?, spatial?, region?
# return:  Dictionary with same shape as IVXVoiceSessionToken or {"error": <code>, "message": <str>}
func mint_async(req: Dictionary) -> Dictionary:
    if req == null:
        return {"error": "bad_args", "message": "req is null"}

    var client = req.get("client", null)
    var session = req.get("session", null)
    var match_id: String = String(req.get("match_id", ""))

    if client == null:
        return {"error": "bad_args", "message": "client (NakamaClient) required"}
    if session == null:
        return {"error": "bad_args", "message": "session (NakamaSession) required"}
    if match_id.is_empty():
        return {"error": "bad_args", "message": "match_id required"}

    # NakamaSession exposes `expired` (bool) in @heroiclabs/nakama-godot 3.x.
    if session.has_method("expired") and session.expired:
        return {"error": "session_expired", "message": "Nakama session expired; refresh before minting"}

    var payload_dict := {
        "match_id":      match_id,
        "can_publish":   bool(req.get("can_publish",   true)),
        "can_subscribe": bool(req.get("can_subscribe", true)),
        "spatial":       bool(req.get("spatial",       false)),
    }
    var region: String = String(req.get("region", ""))
    if not region.is_empty():
        payload_dict["region"] = region

    var rpc = await client.rpc_async(session, RPC_VOICE_TOKEN, JSON.stringify(payload_dict))
    if rpc.is_exception():
        return {"error": "rpc_failed", "message": rpc.get_exception().message}

    if String(rpc.payload).is_empty():
        return {
            "error":   "voice_unconfigured",
            "message": "kernel returned empty payload (LiveKit env vars missing or feature flag off?)",
        }

    var parsed = JSON.parse_string(rpc.payload)
    if typeof(parsed) != TYPE_DICTIONARY:
        return {"error": "decode_failed", "message": "could not parse voice token JSON: %s" % rpc.payload}

    if not parsed.has("token") or not parsed.has("url"):
        return {"error": "invalid_token", "message": "kernel response missing token/url: %s" % rpc.payload}

    # Backfill provider field for callers pre-dating the v1.1 schema.
    if not parsed.has("provider"):
        parsed["provider"] = 1   # LiveKit
    return parsed
