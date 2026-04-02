-- Copyright (c) 2026 Intelli-verse-X
-- MIT License — see LICENSE in the project root.

--!strict

local HttpClient = require(script.Parent.Parent.HttpClient)
local Auth = require(script.Parent.Parent.Auth)

local Leagues = {}

function Leagues.get(player: Player): (any?, string?)
	local token = Auth.get_token(player)
	if not token then return nil, "No session" end
	local resp = HttpClient.rpc_post("leagues_get", "{}", token)
	return if resp.ok then resp.body else nil, if resp.ok then nil else "Leagues.get failed"
end

function Leagues.get_leaderboard(player: Player, league_id: string?): (any?, string?)
	local token = Auth.get_token(player)
	if not token then return nil, "No session" end
	local resp = HttpClient.rpc_post("leagues_leaderboard", HttpClient.json_encode({ id = league_id }), token)
	return if resp.ok then resp.body else nil, if resp.ok then nil else "Leagues.get_leaderboard failed"
end

function Leagues.submit_score(player: Player, score: number): (any?, string?)
	local token = Auth.get_token(player)
	if not token then return nil, "No session" end
	local resp = HttpClient.rpc_post("leagues_submit", HttpClient.json_encode({ score = score }), token)
	return if resp.ok then resp.body else nil, if resp.ok then nil else "Leagues.submit_score failed"
end

return Leagues
