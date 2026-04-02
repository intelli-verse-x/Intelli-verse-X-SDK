-- Copyright (c) 2026 Intelli-verse-X
-- MIT License — see LICENSE in the project root.

--!strict

local HttpClient = require(script.Parent.Parent.HttpClient)
local Auth = require(script.Parent.Parent.Auth)

local DailyRewards = {}

function DailyRewards.get_status(player: Player): (any?, string?)
	local token = Auth.get_token(player)
	if not token then return nil, "No session" end
	local resp = HttpClient.rpc_post("daily_rewards_get", "{}", token)
	return if resp.ok then resp.body else nil, if resp.ok then nil else "DailyRewards.get_status failed"
end

function DailyRewards.claim(player: Player): (any?, string?)
	local token = Auth.get_token(player)
	if not token then return nil, "No session" end
	local resp = HttpClient.rpc_post("daily_rewards_claim", "{}", token)
	return if resp.ok then resp.body else nil, if resp.ok then nil else "DailyRewards.claim failed"
end

return DailyRewards
