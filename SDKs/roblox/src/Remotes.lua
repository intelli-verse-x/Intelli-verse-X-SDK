-- Copyright (c) 2026 Intelli-verse-X
-- MIT License — see LICENSE in the project root.

--!strict

--- Server-client bridge via RemoteEvents and RemoteFunctions.
--- Since HttpService is server-only, clients request data through these remotes.
--- @module Remotes

local ReplicatedStorage = game:GetService("ReplicatedStorage")

local Remotes = {}

local FOLDER_NAME = "IntelliVerseXRemotes"
local _folder: Folder? = nil

local function _ensure_folder(): Folder
	if _folder then return _folder end

	local existing = ReplicatedStorage:FindFirstChild(FOLDER_NAME)
	if existing and existing:IsA("Folder") then
		_folder = existing
		return _folder :: Folder
	end

	local folder = Instance.new("Folder")
	folder.Name = FOLDER_NAME
	folder.Parent = ReplicatedStorage
	_folder = folder
	return folder
end

--- Create or get a RemoteEvent by name.
function Remotes.get_event(name: string): RemoteEvent
	local folder = _ensure_folder()
	local existing = folder:FindFirstChild(name)
	if existing and existing:IsA("RemoteEvent") then
		return existing
	end

	local remote = Instance.new("RemoteEvent")
	remote.Name = name
	remote.Parent = folder
	return remote
end

--- Create or get a RemoteFunction by name.
function Remotes.get_function(name: string): RemoteFunction
	local folder = _ensure_folder()
	local existing = folder:FindFirstChild(name)
	if existing and existing:IsA("RemoteFunction") then
		return existing
	end

	local remote = Instance.new("RemoteFunction")
	remote.Name = name
	remote.Parent = folder
	return remote
end

--- Fire an event to a specific player.
function Remotes.fire_client(event_name: string, player: Player, ...: any)
	local remote = Remotes.get_event(event_name)
	remote:FireClient(player, ...)
end

--- Fire an event to all players.
function Remotes.fire_all(event_name: string, ...: any)
	local remote = Remotes.get_event(event_name)
	remote:FireAllClients(...)
end

--- Listen for a client event on the server.
function Remotes.on_server_event(event_name: string, callback: (Player, ...any) -> ())
	local remote = Remotes.get_event(event_name)
	remote.OnServerEvent:Connect(callback)
end

--- Register a server-side function that clients can invoke.
function Remotes.on_server_invoke(func_name: string, callback: (Player, ...any) -> ...any)
	local remote = Remotes.get_function(func_name)
	remote.OnServerInvoke = callback
end

return Remotes
