local Players = game:GetService("Players")
local ReplicatedStorage = game:GetService("ReplicatedStorage")

local Config = require(ReplicatedStorage:WaitForChild("Config"))
local IntelliVerseX = require(ReplicatedStorage:WaitForChild("IntelliVerseX"))

local function onPlayerAdded(player: Player)
	local ok, err = pcall(function()
		IntelliVerseX.configure({
			gameId = Config.GAME_ID,
			serverHost = Config.SERVER_HOST,
			serverPort = Config.SERVER_PORT,
			serverKey = Config.SERVER_KEY,
		})
		IntelliVerseX.authenticateAsync(player)
		IntelliVerseX.loadHiroSystems(player)
		IntelliVerseX.trackEvent(player, "session_start", {
			game_id = Config.GAME_ID,
		})
	end)
	if not ok then
		warn(("[IVXBootstrap] %s: %s"):format(player.Name, tostring(err)))
	end
end

IntelliVerseX.initServer()
Players.PlayerAdded:Connect(onPlayerAdded)
for _, p in Players:GetPlayers() do
	task.spawn(onPlayerAdded, p)
end
