-- Copyright (c) 2026 Intelli-verse-X
-- MIT License — see LICENSE in the project root.

--!strict

--- AI Voice services: TTS/STT via REST endpoints.
--- @module AI.Voice

local HttpClient = require(script.Parent.Parent.HttpClient)

local Voice = {}

--- Start a voice session for a persona.
function Voice.start_session(persona_id: string, user_id: string): (any?, string?)
	local resp = HttpClient.ai_request("POST", "/v1/voice/sessions", HttpClient.json_encode({
		persona_id = persona_id,
		user_id = user_id,
	}))
	if resp.ok then
		return resp.body, nil
	end
	return nil, "Voice session failed: HTTP " .. tostring(resp.status)
end

--- End an active voice session.
function Voice.end_session(session_id: string): (boolean, string?)
	local resp = HttpClient.ai_request("POST", "/v1/voice/sessions/" .. session_id .. "/end", "{}")
	return resp.ok, if resp.ok then nil else "End session failed"
end

--- Send text for TTS synthesis.
function Voice.send_text(session_id: string, text: string): (any?, string?)
	local resp = HttpClient.ai_request("POST", "/v1/voice/sessions/" .. session_id .. "/send", HttpClient.json_encode({
		text = text,
	}))
	if resp.ok then
		return resp.body, nil
	end
	return nil, "Send text failed: HTTP " .. tostring(resp.status)
end

--- Poll for messages in a voice session.
function Voice.poll_messages(session_id: string): ({ any }?, string?)
	local resp = HttpClient.ai_request("GET", "/v1/voice/sessions/" .. session_id .. "/messages", nil)
	if resp.ok then
		return resp.body, nil
	end
	return nil, "Poll failed: HTTP " .. tostring(resp.status)
end

return Voice
