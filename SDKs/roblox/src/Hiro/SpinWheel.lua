-- Copyright (c) 2026 Intelli-verse-X
-- MIT License — see LICENSE in the project root.

--!strict

local HttpClient = require(script.Parent.Parent.HttpClient)
local Auth = require(script.Parent.Parent.Auth)

local SpinWheel = {}

function SpinWheel.get(player: Player): (any?, string?)
	local token = Auth.get_token(player)
	if not token then return nil, "No session" end
	local resp = HttpClient.rpc_post("hiro/spinwheel/get", "{}", token)
	return if resp.ok then resp.body else nil, if resp.ok then nil else "SpinWheel.get failed"
end

function SpinWheel.spin(player: Player): (any?, string?)
	local token = Auth.get_token(player)
	if not token then return nil, "No session" end
	local resp = HttpClient.rpc_post("hiro/spinwheel/spin", "{}", token)
	return if resp.ok then resp.body else nil, if resp.ok then nil else "SpinWheel.spin failed"
end

return SpinWheel
