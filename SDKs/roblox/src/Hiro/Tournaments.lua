-- Copyright (c) 2026 Intelli-verse-X
-- MIT License — see LICENSE in the project root.

--!strict

local HttpClient = require(script.Parent.Parent.HttpClient)
local Auth = require(script.Parent.Parent.Auth)

local Tournaments = {}

function Tournaments.list(player: Player): (any?, string?)
	local token = Auth.get_token(player)
	if not token then return nil, "No session" end
	local resp = HttpClient.rpc_post("tournaments_list", "{}", token)
	return if resp.ok then resp.body else nil, if resp.ok then nil else "Tournaments.list failed"
end

function Tournaments.join(player: Player, tournament_id: string): (any?, string?)
	local token = Auth.get_token(player)
	if not token then return nil, "No session" end
	local resp = HttpClient.rpc_post("tournaments_join", HttpClient.json_encode({ id = tournament_id }), token)
	return if resp.ok then resp.body else nil, if resp.ok then nil else "Tournaments.join failed"
end

function Tournaments.submit_score(player: Player, tournament_id: string, score: number): (any?, string?)
	local token = Auth.get_token(player)
	if not token then return nil, "No session" end
	local resp = HttpClient.rpc_post("tournaments_submit", HttpClient.json_encode({
		id = tournament_id,
		score = score,
	}), token)
	return if resp.ok then resp.body else nil, if resp.ok then nil else "Tournaments.submit_score failed"
end

function Tournaments.get_leaderboard(player: Player, tournament_id: string): (any?, string?)
	local token = Auth.get_token(player)
	if not token then return nil, "No session" end
	local resp = HttpClient.rpc_post("tournaments_leaderboard", HttpClient.json_encode({ id = tournament_id }), token)
	return if resp.ok then resp.body else nil, if resp.ok then nil else "Tournaments.get_leaderboard failed"
end

return Tournaments
