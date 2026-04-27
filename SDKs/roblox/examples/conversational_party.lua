-- Copyright (c) 2026 Intelli-verse-X
-- MIT License — see LICENSE in the project root.

--!strict

--- Conversational-party example for Roblox (RPC-only).
---
--- Roblox cannot speak the realtime match socket, so multiplayer in
--- this adapter is limited to RPC-driven slow-tick templates. The
--- conversational-party template is perfect for that: turns are JSON
--- and pacing is human-readable.
---
--- Place this script under ServerScriptService.

local IntelliVerseX = require(game.ServerStorage.IntelliVerseX)

IntelliVerseX.configure({
	game_id = "ivx.roblox.party-room",
	host = "nakama.intelliverse.example",
	use_ssl = true,
	debug = true,
})
IntelliVerseX.enable_auto_auth()

local Players = game:GetService("Players")

Players.PlayerAdded:Connect(function(player)
	task.wait(2)
	local match_id, err = IntelliVerseX.Multiplayer.start_party(player, {
		game_id = "ivx.roblox.party-room",
		max_humans = 8,
		agents = { "ivx-quiz-host", "ivx-icebreaker" },
		topic = "ai+gaming",
	})
	if not match_id then
		warn("[IVX] start_party failed: " .. tostring(err))
		return
	end

	IntelliVerseX.Multiplayer.submit_turn(player, match_id, "Hi everyone!")

	task.wait(15)
	local result, e2 = IntelliVerseX.Multiplayer.read_match_result(player, match_id)
	if result then
		print("[IVX] match ended:", result.end_reason, "duration_ms:", result.duration_ms)
	else
		print("[IVX] match still running:", e2)
	end
end)
