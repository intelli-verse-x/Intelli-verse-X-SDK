local Players = game:GetService("Players")
local ReplicatedStorage = game:GetService("ReplicatedStorage")
local player = Players.LocalPlayer
local Config = require(ReplicatedStorage:WaitForChild("Config"))

-- Create a basic Store Panel UI stub
local gui = Instance.new("ScreenGui")
gui.Name = "IVXStorePanel"
gui.ResetOnSpawn = false
gui.Parent = player:WaitForChild("PlayerGui")

local panel = Instance.new("Frame")
panel.Size = UDim2.new(0.5, 0, 0.5, 0)
panel.Position = UDim2.new(0.25, 0, 0.25, 0)
panel.BackgroundColor3 = Color3.fromRGB(40, 40, 40)
panel.Visible = false
panel.Parent = gui

local title = Instance.new("TextLabel")
title.Size = UDim2.new(1, 0, 0, 40)
title.BackgroundColor3 = Config.PRIMARY_COLOR
title.TextColor3 = Color3.fromRGB(255, 255, 255)
title.Text = "Store"
title.Font = Enum.Font.GothamBold
title.TextSize = 20
title.Parent = panel

-- Listen for tab clicks from MainMenu if we wanted to toggle visibility
-- For now, just a functional stub.
