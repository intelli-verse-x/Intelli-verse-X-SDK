-- Copyright (c) 2026 Intelli-verse-X
-- MIT License — see LICENSE in the project root.

--!strict

--- AI Player Profiler: behavioral analysis and player segmentation.
--- @module AI.Profiler

local HttpClient = require(script.Parent.Parent.HttpClient)

local Profiler = {}

--- Submit player behavior events for profiling.
function Profiler.track_event(player_id: string, event_name: string, properties: { [string]: any }?): (boolean, string?)
	local resp = HttpClient.ai_request("POST", "/v1/profiler/events", HttpClient.json_encode({
		player_id = player_id,
		event = event_name,
		properties = properties or {},
		timestamp = os.time(),
	}))
	return resp.ok, if resp.ok then nil else "Track event failed"
end

--- Get the AI-generated player profile/segments.
function Profiler.get_profile(player_id: string): (any?, string?)
	local resp = HttpClient.ai_request("GET", "/v1/profiler/players/" .. player_id .. "/profile", nil)
	if resp.ok then
		return resp.body, nil
	end
	return nil, "Get profile failed: HTTP " .. tostring(resp.status)
end

--- Get recommended actions for a player (personalization).
function Profiler.get_recommendations(player_id: string, context: string?): (any?, string?)
	local resp = HttpClient.ai_request("POST", "/v1/profiler/players/" .. player_id .. "/recommendations", HttpClient.json_encode({
		context = context,
	}))
	if resp.ok then
		return resp.body, nil
	end
	return nil, "Get recommendations failed: HTTP " .. tostring(resp.status)
end

--- Get player churn prediction score.
function Profiler.predict_churn(player_id: string): (any?, string?)
	local resp = HttpClient.ai_request("GET", "/v1/profiler/players/" .. player_id .. "/churn", nil)
	if resp.ok then
		return resp.body, nil
	end
	return nil, "Churn prediction failed: HTTP " .. tostring(resp.status)
end

return Profiler
