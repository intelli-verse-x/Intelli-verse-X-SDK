-- Copyright (c) 2026 Intelli-verse-X
-- MIT License — see LICENSE in the project root.

--!strict

--- AI Assistant: conversational sessions with LLM backend.
--- @module AI.Assistant

local HttpClient = require(script.Parent.Parent.HttpClient)

local Assistant = {}

--- Start a new assistant conversation session.
function Assistant.create_session(persona_id: string, user_id: string, context: { [string]: any }?): (any?, string?)
	local resp = HttpClient.ai_request("POST", "/v1/assistant/sessions", HttpClient.json_encode({
		persona_id = persona_id,
		user_id = user_id,
		context = context,
	}))
	if resp.ok then
		return resp.body, nil
	end
	return nil, "Create session failed: HTTP " .. tostring(resp.status)
end

--- Send a message and receive a response.
function Assistant.send_message(session_id: string, message: string): (any?, string?)
	local resp = HttpClient.ai_request("POST", "/v1/assistant/sessions/" .. session_id .. "/messages", HttpClient.json_encode({
		message = message,
	}))
	if resp.ok then
		return resp.body, nil
	end
	return nil, "Send message failed: HTTP " .. tostring(resp.status)
end

--- End an assistant session.
function Assistant.end_session(session_id: string): (boolean, string?)
	local resp = HttpClient.ai_request("POST", "/v1/assistant/sessions/" .. session_id .. "/end", "{}")
	return resp.ok, if resp.ok then nil else "End session failed"
end

--- List available personas.
function Assistant.list_personas(): ({ any }?, string?)
	local resp = HttpClient.ai_request("GET", "/v1/assistant/personas", nil)
	if resp.ok then
		return resp.body, nil
	end
	return nil, "List personas failed: HTTP " .. tostring(resp.status)
end

return Assistant
