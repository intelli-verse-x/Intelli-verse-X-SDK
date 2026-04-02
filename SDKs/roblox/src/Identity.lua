-- Copyright (c) 2026 Intelli-verse-X
-- MIT License — see LICENSE in the project root.

--!strict

--- Cross-Game Identity: sync player profiles, wallets, and cloud storage
--- across Roblox experiences AND non-Roblox games via Nakama.
--- @module Identity

local HttpClient = require(script.Parent.HttpClient)
local Auth = require(script.Parent.Auth)

local Identity = {}

--- Fetch the player's cross-game profile from Nakama.
function Identity.fetch_profile(player: Player): (any?, string?)
	local token = Auth.get_token(player)
	if not token then return nil, "No session" end

	local resp = HttpClient.get("/v2/account", token)
	if not resp.ok then
		return nil, "Fetch profile failed: HTTP " .. tostring(resp.status)
	end

	local account = resp.body
	local profile = {
		user_id = if account.user then account.user.id else "",
		username = if account.user then account.user.username else "",
		display_name = if account.user then account.user.display_name else "",
		avatar_url = if account.user then account.user.avatar_url else "",
		lang_tag = if account.user then account.user.lang_tag else "",
		metadata = account.user and account.user.metadata,
		wallet = account.wallet,
	}

	return profile, nil
end

--- Update the player's cross-game profile.
function Identity.update_profile(player: Player, fields: {
	display_name: string?,
	avatar_url: string?,
	lang_tag: string?,
}): (boolean, string?)
	local token = Auth.get_token(player)
	if not token then return false, "No session" end

	local resp = HttpClient.put("/v2/account", fields, token)
	return resp.ok, if resp.ok then nil else "Update profile failed"
end

--- Fetch wallet balances via Hiro economy RPC.
function Identity.fetch_wallet(player: Player): (any?, string?)
	local token = Auth.get_token(player)
	if not token then return nil, "No session" end

	local resp = HttpClient.rpc_post("hiro_economy_list", "{}", token)
	return if resp.ok then resp.body else nil, if resp.ok then nil else "Fetch wallet failed"
end

--- Grant currency to a player via Hiro economy RPC.
function Identity.grant_currency(player: Player, currency_id: string, amount: number): (any?, string?)
	local token = Auth.get_token(player)
	if not token then return nil, "No session" end

	local payload = HttpClient.json_encode({ currencies = { [currency_id] = amount } })
	local resp = HttpClient.rpc_post("hiro_economy_grant", payload, token)
	return if resp.ok then resp.body else nil, if resp.ok then nil else "Grant currency failed"
end

--- Read a cloud storage object (cross-game persistent data).
function Identity.read_storage(player: Player, collection: string, key: string): (any?, string?)
	local token = Auth.get_token(player)
	local user_id = Auth.get_nakama_user_id(player)
	if not token or not user_id then return nil, "No session" end

	local resp = HttpClient.post("/v2/storage", {
		object_ids = {
			{ collection = collection, key = key, user_id = user_id },
		},
	}, token)

	if not resp.ok then
		return nil, "Read storage failed: HTTP " .. tostring(resp.status)
	end

	local objects = resp.body and resp.body.objects
	if objects and #objects > 0 then
		local value = objects[1].value
		if type(value) == "string" then
			local ok, decoded = pcall(HttpClient.json_decode, value)
			return if ok then decoded else value, nil
		end
		return value, nil
	end

	return nil, nil
end

--- Write a cloud storage object (cross-game persistent data).
function Identity.write_storage(player: Player, collection: string, key: string, value: { [string]: any }): (boolean, string?)
	local token = Auth.get_token(player)
	if not token then return false, "No session" end

	local resp = HttpClient.put("/v2/storage", {
		objects = {
			{
				collection = collection,
				key = key,
				value = value,
				permission_read = 1,
				permission_write = 1,
			},
		},
	}, token)

	return resp.ok, if resp.ok then nil else "Write storage failed"
end

return Identity
