-- Copyright (c) 2026 Intelli-verse-X
-- MIT License — see LICENSE in the project root.

--!strict

local HttpClient = require(script.Parent.Parent.HttpClient)
local Auth = require(script.Parent.Parent.Auth)

local DailyMissions = {}

function DailyMissions.list(player: Player): (any?, string?)
	local token = Auth.get_token(player)
	if not token then return nil, "No session" end
	local resp = HttpClient.rpc_post("daily_missions_list", "{}", token)
	return if resp.ok then resp.body else nil, if resp.ok then nil else "DailyMissions.list failed"
end

function DailyMissions.complete(player: Player, mission_id: string): (any?, string?)
	local token = Auth.get_token(player)
	if not token then return nil, "No session" end
	local resp = HttpClient.rpc_post("daily_missions_complete", HttpClient.json_encode({ id = mission_id }), token)
	return if resp.ok then resp.body else nil, if resp.ok then nil else "DailyMissions.complete failed"
end

function DailyMissions.claim(player: Player, mission_id: string): (any?, string?)
	local token = Auth.get_token(player)
	if not token then return nil, "No session" end
	local resp = HttpClient.rpc_post("daily_missions_claim", HttpClient.json_encode({ id = mission_id }), token)
	return if resp.ok then resp.body else nil, if resp.ok then nil else "DailyMissions.claim failed"
end

return DailyMissions
