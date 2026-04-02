-- Copyright (c) 2026 Intelli-verse-X
-- MIT License — see LICENSE in the project root.

--!strict

local HttpClient = require(script.Parent.Parent.HttpClient)
local Auth = require(script.Parent.Parent.Auth)

local FortuneWheel = {}

function FortuneWheel.get(player: Player): (any?, string?)
	local token = Auth.get_token(player)
	if not token then return nil, "No session" end
	local resp = HttpClient.rpc_post("fortune_wheel_get", "{}", token)
	return if resp.ok then resp.body else nil, if resp.ok then nil else "FortuneWheel.get failed"
end

function FortuneWheel.spin(player: Player): (any?, string?)
	local token = Auth.get_token(player)
	if not token then return nil, "No session" end
	local resp = HttpClient.rpc_post("fortune_wheel_spin", "{}", token)
	return if resp.ok then resp.body else nil, if resp.ok then nil else "FortuneWheel.spin failed"
end

return FortuneWheel
