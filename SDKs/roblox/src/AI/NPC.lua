-- Copyright (c) 2026 Intelli-verse-X
-- MIT License — see LICENSE in the project root.

--!strict

--- AI NPC Dialog Manager: intelligent NPC conversations powered by LLM.
--- @module AI.NPC

local HttpClient = require(script.Parent.Parent.HttpClient)

local NPC = {}

export type NPCConfig = {
	npc_id: string,
	persona_id: string,
	name: string,
	system_prompt: string?,
	knowledge_base: string?,
	max_turns: number?,
}

--- Start a dialog session with an NPC.
function NPC.start_dialog(npc_config: NPCConfig, player_id: string): (any?, string?)
	local resp = HttpClient.ai_request("POST", "/v1/npc/dialog/start", HttpClient.json_encode({
		npc_id = npc_config.npc_id,
		persona_id = npc_config.persona_id,
		name = npc_config.name,
		system_prompt = npc_config.system_prompt,
		knowledge_base = npc_config.knowledge_base,
		max_turns = npc_config.max_turns,
		player_id = player_id,
	}))
	if resp.ok then
		return resp.body, nil
	end
	return nil, "Start dialog failed: HTTP " .. tostring(resp.status)
end

--- Send a player message to an active NPC dialog.
function NPC.send_message(dialog_id: string, message: string): (any?, string?)
	local resp = HttpClient.ai_request("POST", "/v1/npc/dialog/" .. dialog_id .. "/message", HttpClient.json_encode({
		message = message,
	}))
	if resp.ok then
		return resp.body, nil
	end
	return nil, "NPC message failed: HTTP " .. tostring(resp.status)
end

--- End an active dialog session.
function NPC.end_dialog(dialog_id: string): (boolean, string?)
	local resp = HttpClient.ai_request("POST", "/v1/npc/dialog/" .. dialog_id .. "/end", "{}")
	return resp.ok, if resp.ok then nil else "End dialog failed"
end

--- Get dialog history for a session.
function NPC.get_history(dialog_id: string): ({ any }?, string?)
	local resp = HttpClient.ai_request("GET", "/v1/npc/dialog/" .. dialog_id .. "/history", nil)
	if resp.ok then
		return resp.body, nil
	end
	return nil, "Get history failed: HTTP " .. tostring(resp.status)
end

return NPC
