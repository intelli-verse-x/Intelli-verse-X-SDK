-- IntelliVerseX Roblox Example: AI NPC Dialog (ServerScript)
-- Place in ServerScriptService

local IVX = require(game.ServerScriptService.IntelliVerseX)

IVX.configure({
	game_id = "YOUR_GAME_ID",
	debug = true,
})

IVX.enable_auto_auth()

-- Define an NPC configuration
local shopkeeper = {
	npc_id = "shopkeeper_01",
	persona_id = "friendly_merchant",
	name = "Elara the Merchant",
	system_prompt = "You are Elara, a friendly merchant in a fantasy RPG. You sell potions, weapons, and armor. Be helpful but try to upsell.",
	max_turns = 20,
}

-- Track active dialogs per player
local active_dialogs = {}

-- Handle player starting a conversation with an NPC
IVX.Remotes.on_server_event("IVX_StartNPCDialog", function(player)
	local session, err = IVX.AI.NPC.start_dialog(shopkeeper, tostring(player.UserId))
	if session then
		active_dialogs[player.UserId] = session.dialog_id
		IVX.Remotes.fire_client("IVX_NPCResponse", player, {
			dialog_id = session.dialog_id,
			message = session.greeting or "Welcome, traveler! What can I get for you today?",
		})
	else
		warn("Failed to start NPC dialog: " .. (err or "unknown"))
	end
end)

-- Handle player sending a message to the NPC
IVX.Remotes.on_server_event("IVX_SendNPCMessage", function(player, message)
	local dialog_id = active_dialogs[player.UserId]
	if not dialog_id then return end

	local response, err = IVX.AI.NPC.send_message(dialog_id, message)
	if response then
		IVX.Remotes.fire_client("IVX_NPCResponse", player, {
			dialog_id = dialog_id,
			message = response.text or response.message or "",
		})
	end
end)

-- Cleanup on player leave
game.Players.PlayerRemoving:Connect(function(player)
	local dialog_id = active_dialogs[player.UserId]
	if dialog_id then
		IVX.AI.NPC.end_dialog(dialog_id)
		active_dialogs[player.UserId] = nil
	end
end)
