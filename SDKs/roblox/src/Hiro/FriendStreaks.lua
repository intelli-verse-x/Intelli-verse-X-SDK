-- Copyright (c) 2026 Intelli-verse-X
-- MIT License — see LICENSE in the project root.

--!strict

local HttpClient = require(script.Parent.Parent.HttpClient)
local Auth = require(script.Parent.Parent.Auth)

local FriendStreaks = {}

function FriendStreaks.get(player: Player): (any?, string?)
	local token = Auth.get_token(player)
	if not token then return nil, "No session" end
	local resp = HttpClient.rpc_post("friend_streaks_get", "{}", token)
	return if resp.ok then resp.body else nil, if resp.ok then nil else "FriendStreaks.get failed"
end

function FriendStreaks.update(player: Player, friend_id: string): (any?, string?)
	local token = Auth.get_token(player)
	if not token then return nil, "No session" end
	local resp = HttpClient.rpc_post("friend_streaks_update", HttpClient.json_encode({ friend_id = friend_id }), token)
	return if resp.ok then resp.body else nil, if resp.ok then nil else "FriendStreaks.update failed"
end

function FriendStreaks.claim(player: Player, streak_id: string): (any?, string?)
	local token = Auth.get_token(player)
	if not token then return nil, "No session" end
	local resp = HttpClient.rpc_post("friend_streaks_claim", HttpClient.json_encode({ id = streak_id }), token)
	return if resp.ok then resp.body else nil, if resp.ok then nil else "FriendStreaks.claim failed"
end

return FriendStreaks
