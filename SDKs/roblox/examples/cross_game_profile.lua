-- IntelliVerseX Roblox Example: Cross-Game Player Profile (ServerScript)
-- Syncs player data between Roblox experiences and non-Roblox games via Nakama.
-- Place in ServerScriptService

local IVX = require(game.ServerScriptService.IntelliVerseX)

IVX.configure({
	game_id = "YOUR_GAME_ID",
	debug = true,
})

IVX.enable_auto_auth()

-- Fetch cross-game profile when client requests it
IVX.Remotes.on_server_invoke("IVX_GetProfile", function(player)
	local profile, err = IVX.Identity.fetch_profile(player)
	if profile then
		return { ok = true, data = profile }
	end
	return { ok = false, error = err }
end)

-- Update display name (synced across all games using IntelliVerseX)
IVX.Remotes.on_server_invoke("IVX_UpdateDisplayName", function(player, new_name)
	if type(new_name) ~= "string" or #new_name < 1 or #new_name > 50 then
		return { ok = false, error = "Invalid display name" }
	end

	local ok, err = IVX.Identity.update_profile(player, { display_name = new_name })
	return { ok = ok, error = err }
end)

-- Fetch cross-game wallet (shared currency across experiences)
IVX.Remotes.on_server_invoke("IVX_GetWallet", function(player)
	local wallet, err = IVX.Identity.fetch_wallet(player)
	if wallet then
		return { ok = true, data = wallet }
	end
	return { ok = false, error = err }
end)

-- Save game progress to cross-game cloud storage
IVX.Remotes.on_server_invoke("IVX_SaveProgress", function(player, progress_data)
	if type(progress_data) ~= "table" then
		return { ok = false, error = "Invalid data" }
	end

	local ok, err = IVX.Identity.write_storage(player, "game_progress", "save_slot_1", progress_data)
	return { ok = ok, error = err }
end)

-- Load game progress from cross-game cloud storage
IVX.Remotes.on_server_invoke("IVX_LoadProgress", function(player)
	local data, err = IVX.Identity.read_storage(player, "game_progress", "save_slot_1")
	return { ok = data ~= nil, data = data, error = err }
end)
