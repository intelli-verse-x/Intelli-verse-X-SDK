-- Copyright (c) 2026 Intelli-verse-X
-- MIT License — see LICENSE in the project root.

--!strict

local HttpClient = require(script.Parent.Parent.HttpClient)
local Auth = require(script.Parent.Parent.Auth)

local Achievements = {}

function Achievements.list(player: Player): (any?, string?)
	local token = Auth.get_token(player)
	if not token then return nil, "No session" end
	local resp = HttpClient.rpc_post("achievements_list", "{}", token)
	return if resp.ok then resp.body else nil, if resp.ok then nil else "Achievements.list failed"
end

function Achievements.claim(player: Player, achievement_id: string): (any?, string?)
	local token = Auth.get_token(player)
	if not token then return nil, "No session" end
	local resp = HttpClient.rpc_post("achievements_claim", HttpClient.json_encode({ id = achievement_id }), token)
	return if resp.ok then resp.body else nil, if resp.ok then nil else "Achievements.claim failed"
end

function Achievements.update_progress(player: Player, achievement_id: string, progress: number): (any?, string?)
	local token = Auth.get_token(player)
	if not token then return nil, "No session" end
	local resp = HttpClient.rpc_post("achievements_update", HttpClient.json_encode({
		id = achievement_id,
		progress = progress,
	}), token)
	return if resp.ok then resp.body else nil, if resp.ok then nil else "Achievements.update_progress failed"
end

return Achievements
