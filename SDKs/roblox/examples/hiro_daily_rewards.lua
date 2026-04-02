-- IntelliVerseX Roblox Example: Daily Rewards + Streaks (ServerScript)
-- Place in ServerScriptService

local IVX = require(game.ServerScriptService.IntelliVerseX)

IVX.configure({
	game_id = "YOUR_GAME_ID",
	debug = true,
})

IVX.enable_auto_auth()

-- When client requests daily reward status
IVX.Remotes.on_server_invoke("IVX_GetDailyRewards", function(player)
	local status, err = IVX.Hiro.DailyRewards.get_status(player)
	if status then
		return { ok = true, data = status }
	end
	return { ok = false, error = err }
end)

-- When client claims daily reward
IVX.Remotes.on_server_invoke("IVX_ClaimDailyReward", function(player)
	local result, err = IVX.Hiro.DailyRewards.claim(player)
	if result then
		-- Also update the player's streak
		IVX.Hiro.Streaks.update(player)
		return { ok = true, data = result }
	end
	return { ok = false, error = err }
end)

-- When client requests streak info
IVX.Remotes.on_server_invoke("IVX_GetStreaks", function(player)
	local streaks, err = IVX.Hiro.Streaks.get(player)
	if streaks then
		return { ok = true, data = streaks }
	end
	return { ok = false, error = err }
end)

-- Spin the fortune wheel
IVX.Remotes.on_server_invoke("IVX_SpinWheel", function(player)
	local result, err = IVX.Hiro.SpinWheel.spin(player)
	if result then
		return { ok = true, data = result }
	end
	return { ok = false, error = err }
end)
