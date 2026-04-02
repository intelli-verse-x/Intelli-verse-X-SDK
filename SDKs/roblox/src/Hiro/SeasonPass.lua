-- Copyright (c) 2026 Intelli-verse-X
-- MIT License — see LICENSE in the project root.

--!strict

local HttpClient = require(script.Parent.Parent.HttpClient)
local Auth = require(script.Parent.Parent.Auth)

local SeasonPass = {}

function SeasonPass.get(player: Player): (any?, string?)
	local token = Auth.get_token(player)
	if not token then return nil, "No session" end
	local resp = HttpClient.rpc_post("season_pass_get", "{}", token)
	return if resp.ok then resp.body else nil, if resp.ok then nil else "SeasonPass.get failed"
end

function SeasonPass.claim_tier(player: Player, tier: number): (any?, string?)
	local token = Auth.get_token(player)
	if not token then return nil, "No session" end
	local resp = HttpClient.rpc_post("season_pass_claim", HttpClient.json_encode({ tier = tier }), token)
	return if resp.ok then resp.body else nil, if resp.ok then nil else "SeasonPass.claim_tier failed"
end

function SeasonPass.add_xp(player: Player, amount: number): (any?, string?)
	local token = Auth.get_token(player)
	if not token then return nil, "No session" end
	local resp = HttpClient.rpc_post("season_pass_add_xp", HttpClient.json_encode({ xp = amount }), token)
	return if resp.ok then resp.body else nil, if resp.ok then nil else "SeasonPass.add_xp failed"
end

return SeasonPass
