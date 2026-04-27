-- Copyright (c) 2026 Intelli-verse-X
-- MIT License — see LICENSE in the project root.

--!strict

--- IntelliVerseX Multiplayer (RPC-only) bridge for Roblox.
---
--- Roblox cannot speak the Nakama match WebSocket protocol directly
--- (HttpService is request/response only — no long-lived sockets,
--- no MatchData frames). This module instead exposes the kernel
--- through the same RPC surface used by other adapters:
---
---   * mp_create_match(template_id, game_id, init?) → { match_id }
---   * mp_read_match_result(match_id) → IMatchResultEnvelope
---   * mp_list_templates() → { templates = { … } }
---
--- Plus convenience helpers for the conversational-party + agent
--- templates which are the only multiplayer modes that make sense in
--- a request/response context (turn-based, slow-tick).
---
--- @module IntelliVerseX.Multiplayer

local HttpClient = require(script.Parent.HttpClient)
local Auth = require(script.Parent.Auth)

local Multiplayer = {}

export type MatchResult = {
	match_id: string,
	template_id: string,
	end_reason: number,
	duration_ms: number,
	participants: { any },
	game_payload: any?,
}

local function _rpc(player: Player, rpc_id: string, payload: any): (any?, string?)
	local token = Auth.get_token(player)
	if not token then
		return nil, "no session for " .. player.Name
	end
	local body = if payload == nil then nil else HttpClient.json_encode(payload)
	local resp = HttpClient.rpc_post(rpc_id, body, token)
	if resp.ok then
		return resp.body, nil
	end
	return nil, "rpc " .. rpc_id .. " failed: HTTP " .. tostring(resp.status)
end

--- Create a kernel match. `init_` is an arbitrary table that the
--- specific match template will validate.
function Multiplayer.create_match(player: Player, template_id: string, game_id: string, init_: any?): (string?, string?)
	local req = {
		template_id = template_id,
		game_id = game_id,
		init = init_,
	}
	local body, err = _rpc(player, "mp_create_match", req)
	if err then return nil, err end
	if typeof(body) ~= "table" or typeof(body.match_id) ~= "string" then
		return nil, "malformed mp_create_match response"
	end
	return body.match_id, nil
end

--- Read the typed result envelope for a finished match.
function Multiplayer.read_match_result(player: Player, match_id: string): (MatchResult?, string?)
	local body, err = _rpc(player, "mp_read_match_result", { match_id = match_id })
	if err then return nil, err end
	return body :: MatchResult, nil
end

--- List registered templates.
function Multiplayer.list_templates(player: Player): ({ string }, string?)
	local body, err = _rpc(player, "mp_list_templates", nil)
	if err then return {}, err end
	if typeof(body) ~= "table" or typeof(body.templates) ~= "table" then
		return {}, "malformed mp_list_templates response"
	end
	local out = {}
	for _, t in body.templates do
		if typeof(t) == "table" and typeof(t.template_id) == "string" then
			table.insert(out, t.template_id)
		end
	end
	return out, nil
end

-- Conversational-party convenience helpers. These funnel through the
-- generic kernel RPCs but tag init payloads with the well-known
-- conversational-party template id.

--- Start a conversational-party room with a list of agent personas.
function Multiplayer.start_party(player: Player, opts: {
	game_id: string,
	max_humans: number?,
	agents: { string }?,
	topic: string?,
}): (string?, string?)
	local init_ = {
		max_humans = opts.max_humans or 8,
		agent_personas = opts.agents or {},
		topic = opts.topic or "",
	}
	return Multiplayer.create_match(player, "conversational-party-v1", opts.game_id, init_)
end

--- Submit a turn (text or transcribed voice) for a conversational
--- party. Real-time voice frames are NOT supported on Roblox; only
--- finalized utterances pass through HttpService.
function Multiplayer.submit_turn(player: Player, match_id: string, text: string): (any?, string?)
	return _rpc(player, "mp_party_submit_turn", { match_id = match_id, text = text })
end

return Multiplayer
