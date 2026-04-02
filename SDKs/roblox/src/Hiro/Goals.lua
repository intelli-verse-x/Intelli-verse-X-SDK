-- Copyright (c) 2026 Intelli-verse-X
-- MIT License — see LICENSE in the project root.

--!strict

local HttpClient = require(script.Parent.Parent.HttpClient)
local Auth = require(script.Parent.Parent.Auth)

local Goals = {}

function Goals.get_weekly(player: Player): (any?, string?)
	local token = Auth.get_token(player)
	if not token then return nil, "No session" end
	local resp = HttpClient.rpc_post("goals_weekly_get", "{}", token)
	return if resp.ok then resp.body else nil, if resp.ok then nil else "Goals.get_weekly failed"
end

function Goals.get_monthly(player: Player): (any?, string?)
	local token = Auth.get_token(player)
	if not token then return nil, "No session" end
	local resp = HttpClient.rpc_post("goals_monthly_get", "{}", token)
	return if resp.ok then resp.body else nil, if resp.ok then nil else "Goals.get_monthly failed"
end

function Goals.update_progress(player: Player, goal_id: string, progress: number): (any?, string?)
	local token = Auth.get_token(player)
	if not token then return nil, "No session" end
	local resp = HttpClient.rpc_post("goals_update", HttpClient.json_encode({
		id = goal_id,
		progress = progress,
	}), token)
	return if resp.ok then resp.body else nil, if resp.ok then nil else "Goals.update_progress failed"
end

function Goals.claim(player: Player, goal_id: string): (any?, string?)
	local token = Auth.get_token(player)
	if not token then return nil, "No session" end
	local resp = HttpClient.rpc_post("goals_claim", HttpClient.json_encode({ id = goal_id }), token)
	return if resp.ok then resp.body else nil, if resp.ok then nil else "Goals.claim failed"
end

return Goals
