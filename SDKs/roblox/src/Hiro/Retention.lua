-- Copyright (c) 2026 Intelli-verse-X
-- MIT License — see LICENSE in the project root.

--!strict

local HttpClient = require(script.Parent.Parent.HttpClient)
local Auth = require(script.Parent.Parent.Auth)

local Retention = {}

function Retention.get(player: Player): (any?, string?)
	local token = Auth.get_token(player)
	if not token then return nil, "No session" end
	local resp = HttpClient.rpc_post("retention_get", "{}", token)
	return if resp.ok then resp.body else nil, if resp.ok then nil else "Retention.get failed"
end

function Retention.update(player: Player): (any?, string?)
	local token = Auth.get_token(player)
	if not token then return nil, "No session" end
	local resp = HttpClient.rpc_post("retention_update", "{}", token)
	return if resp.ok then resp.body else nil, if resp.ok then nil else "Retention.update failed"
end

return Retention
