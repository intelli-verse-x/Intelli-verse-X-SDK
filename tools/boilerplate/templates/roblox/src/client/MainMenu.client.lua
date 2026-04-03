local Players = game:GetService("Players")
local ReplicatedStorage = game:GetService("ReplicatedStorage")

local player = Players.LocalPlayer
local Config = require(ReplicatedStorage:WaitForChild("Config"))

local gui = Instance.new("ScreenGui")
gui.Name = "IVXMainMenu"
gui.ResetOnSpawn = false
gui.Parent = player:WaitForChild("PlayerGui")

local top = Instance.new("Frame")
top.Size = UDim2.new(1, 0, 0, 48)
top.BackgroundColor3 = Config.PRIMARY_COLOR
top.Parent = gui

local wallet = Instance.new("TextLabel")
wallet.Size = UDim2.new(1, -16, 1, -8)
wallet.Position = UDim2.new(0, 8, 0, 4)
wallet.BackgroundTransparency = 1
wallet.TextXAlignment = Enum.TextXAlignment.Left
wallet.Font = Enum.Font.GothamMedium
wallet.TextSize = 18
wallet.TextColor3 = Color3.new(1, 1, 1)
wallet.Text = ("Wallet · coins %d · gems %d"):format(Config.INITIAL_COINS, Config.INITIAL_GEMS)
wallet.Parent = top

local tabs = { "Home", "Store", "Achievements", "Daily Rewards", "Leaderboard", "Settings" }
local tabBar = Instance.new("Frame")
tabBar.Size = UDim2.new(1, 0, 0, 36)
tabBar.Position = UDim2.new(0, 0, 0, 48)
tabBar.BackgroundColor3 = Config.SECONDARY_COLOR
tabBar.Parent = gui

for i, name in ipairs(tabs) do
	local b = Instance.new("TextButton")
	b.Size = UDim2.new(1 / #tabs, 0, 1, 0)
	b.Position = UDim2.new((i - 1) / #tabs, 0, 0, 0)
	b.BackgroundTransparency = 1
	b.Text = name
	b.TextColor3 = Color3.new(1, 1, 1)
	b.Font = Enum.Font.Gotham
	b.TextSize = 14
	b.Parent = tabBar
end
