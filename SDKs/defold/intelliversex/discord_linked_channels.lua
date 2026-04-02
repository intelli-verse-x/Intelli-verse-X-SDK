-- Copyright (c) 2026 Intelli-verse-X
-- MIT License — see LICENSE in the project root.

--- Discord Social SDK — Linked Channels: bridge in-game chat to Discord server text channels.
--- Stub matching Unity/JS IVXDiscordLinkedChannels; integrate with the native Discord Social SDK.
--- @module discord_linked_channels

local M = {}

--- Link a Discord text channel to a game lobby for message bridging.
--- @param lobby_id string
--- @param channel_id string
--- @return table Linked channel: channel_id, guild_id, name, lobby_id, linked_at
function M.link_channel(_lobby_id, _channel_id)
    error("Not implemented — requires Discord Social SDK native integration.")
end

--- Unlink a previously linked channel from a lobby.
--- @param lobby_id string
--- @param channel_id string
function M.unlink_channel(_lobby_id, _channel_id)
    error("Not implemented — requires Discord Social SDK native integration.")
end

--- Get all linked channels for a given lobby.
--- @param lobby_id string
--- @return table[] Array of linked channel tables
function M.get_linked_channels(_lobby_id)
    error("Not implemented — requires Discord Social SDK native integration.")
end

return M
