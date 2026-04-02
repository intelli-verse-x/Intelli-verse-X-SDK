-- Copyright (c) 2026 Intelli-verse-X
-- MIT License — see LICENSE in the project root.

--- IntelliVerseX Studio Plugin
--- Toolbar button + DockWidget config panel for configuring the SDK.

local HttpService = game:GetService("HttpService")

local PLUGIN_NAME = "IntelliVerseX"
local WIDGET_ID = "IVXConfigPanel"

local toolbar = plugin:CreateToolbar(PLUGIN_NAME)
local button = toolbar:CreateButton(
	"Configure",
	"Open IntelliVerseX SDK Configuration",
	"rbxassetid://0"
)

local widgetInfo = DockWidgetPluginGuiInfo.new(
	Enum.InitialDockState.Float,
	false,
	false,
	360,
	480,
	300,
	400
)

local widget = plugin:CreateDockWidgetPluginGui(WIDGET_ID, widgetInfo)
widget.Title = "IntelliVerseX Config"

-- Theme-aware colors
local function get_theme_color(item: Enum.StudioStyleGuideColor): Color3
	local ok, color = pcall(function()
		return settings().Studio.Theme:GetColor(item)
	end)
	return if ok then color else Color3.fromRGB(30, 30, 40)
end

local bg = get_theme_color(Enum.StudioStyleGuideColor.MainBackground)
local text_color = get_theme_color(Enum.StudioStyleGuideColor.MainText)
local input_bg = get_theme_color(Enum.StudioStyleGuideColor.InputFieldBackground)
local button_bg = get_theme_color(Enum.StudioStyleGuideColor.Button)

-- UI Construction
local frame = Instance.new("Frame")
frame.Size = UDim2.fromScale(1, 1)
frame.BackgroundColor3 = bg
frame.BorderSizePixel = 0
frame.Parent = widget

local layout = Instance.new("UIListLayout")
layout.Padding = UDim.new(0, 8)
layout.HorizontalAlignment = Enum.HorizontalAlignment.Center
layout.Parent = frame

local padding = Instance.new("UIPadding")
padding.PaddingTop = UDim.new(0, 12)
padding.PaddingLeft = UDim.new(0, 12)
padding.PaddingRight = UDim.new(0, 12)
padding.Parent = frame

-- Title
local title = Instance.new("TextLabel")
title.Size = UDim2.new(1, 0, 0, 28)
title.BackgroundTransparency = 1
title.Text = "IntelliVerseX SDK v5.8.0"
title.TextColor3 = text_color
title.TextSize = 16
title.Font = Enum.Font.GothamBold
title.TextXAlignment = Enum.TextXAlignment.Left
title.Parent = frame

local subtitle = Instance.new("TextLabel")
subtitle.Size = UDim2.new(1, 0, 0, 18)
subtitle.BackgroundTransparency = 1
subtitle.Text = "AI + Hiro Live-Ops + Cross-Game Identity"
subtitle.TextColor3 = Color3.fromRGB(150, 150, 170)
subtitle.TextSize = 12
subtitle.Font = Enum.Font.Gotham
subtitle.TextXAlignment = Enum.TextXAlignment.Left
subtitle.Parent = frame

local function create_input(label_text: string, placeholder: string): TextBox
	local container = Instance.new("Frame")
	container.Size = UDim2.new(1, 0, 0, 50)
	container.BackgroundTransparency = 1
	container.Parent = frame

	local inner_layout = Instance.new("UIListLayout")
	inner_layout.Padding = UDim.new(0, 2)
	inner_layout.Parent = container

	local lbl = Instance.new("TextLabel")
	lbl.Size = UDim2.new(1, 0, 0, 16)
	lbl.BackgroundTransparency = 1
	lbl.Text = label_text
	lbl.TextColor3 = text_color
	lbl.TextSize = 12
	lbl.Font = Enum.Font.GothamMedium
	lbl.TextXAlignment = Enum.TextXAlignment.Left
	lbl.Parent = container

	local input = Instance.new("TextBox")
	input.Size = UDim2.new(1, 0, 0, 28)
	input.BackgroundColor3 = input_bg
	input.TextColor3 = text_color
	input.PlaceholderText = placeholder
	input.PlaceholderColor3 = Color3.fromRGB(100, 100, 120)
	input.TextSize = 12
	input.Font = Enum.Font.GothamMedium
	input.ClearTextOnFocus = false
	input.Parent = container

	local corner = Instance.new("UICorner")
	corner.CornerRadius = UDim.new(0, 4)
	corner.Parent = input

	local input_padding = Instance.new("UIPadding")
	input_padding.PaddingLeft = UDim.new(0, 6)
	input_padding.Parent = input

	return input
end

local game_id_input = create_input("Game ID", "your-game-uuid-here")
local host_input = create_input("Host", "nakama-rest.intelli-verse-x.ai")
local key_input = create_input("Server Key", "defaultkey")
local ai_url_input = create_input("AI Base URL", "https://ai.intelli-verse-x.ai")

-- Load saved settings
local SETTINGS_KEY = "IVXPluginSettings"
local function load_settings()
	local ok, saved = pcall(plugin.GetSetting, plugin, SETTINGS_KEY)
	if ok and saved then
		game_id_input.Text = saved.game_id or ""
		host_input.Text = saved.host or ""
		key_input.Text = saved.server_key or ""
		ai_url_input.Text = saved.ai_base_url or ""
	end
end

local function save_settings()
	pcall(plugin.SetSetting, plugin, SETTINGS_KEY, {
		game_id = game_id_input.Text,
		host = host_input.Text,
		server_key = key_input.Text,
		ai_base_url = ai_url_input.Text,
	})
end

-- Test Connection button
local test_btn = Instance.new("TextButton")
test_btn.Size = UDim2.new(1, 0, 0, 32)
test_btn.BackgroundColor3 = button_bg
test_btn.TextColor3 = text_color
test_btn.Text = "Test Connection"
test_btn.TextSize = 13
test_btn.Font = Enum.Font.GothamBold
test_btn.Parent = frame

local test_corner = Instance.new("UICorner")
test_corner.CornerRadius = UDim.new(0, 4)
test_corner.Parent = test_btn

local status_label = Instance.new("TextLabel")
status_label.Size = UDim2.new(1, 0, 0, 18)
status_label.BackgroundTransparency = 1
status_label.Text = ""
status_label.TextSize = 11
status_label.Font = Enum.Font.Gotham
status_label.TextXAlignment = Enum.TextXAlignment.Left
status_label.Parent = frame

test_btn.MouseButton1Click:Connect(function()
	save_settings()
	status_label.Text = "Testing..."
	status_label.TextColor3 = Color3.fromRGB(200, 200, 200)

	local host = host_input.Text ~= "" and host_input.Text or "nakama-rest.intelli-verse-x.ai"
	local url = "https://" .. host .. "/healthcheck"

	local success, result = pcall(function()
		return HttpService:RequestAsync({ Url = url, Method = "GET" })
	end)

	if success and result.StatusCode == 200 then
		status_label.Text = "Connected successfully!"
		status_label.TextColor3 = Color3.fromRGB(0, 200, 120)
	else
		local err_msg = if success then "HTTP " .. tostring(result.StatusCode) else tostring(result)
		status_label.Text = "Connection failed: " .. err_msg
		status_label.TextColor3 = Color3.fromRGB(255, 80, 80)
	end
end)

-- Save button
local save_btn = Instance.new("TextButton")
save_btn.Size = UDim2.new(1, 0, 0, 32)
save_btn.BackgroundColor3 = Color3.fromRGB(0, 150, 100)
save_btn.TextColor3 = Color3.fromRGB(255, 255, 255)
save_btn.Text = "Save Configuration"
save_btn.TextSize = 13
save_btn.Font = Enum.Font.GothamBold
save_btn.Parent = frame

local save_corner = Instance.new("UICorner")
save_corner.CornerRadius = UDim.new(0, 4)
save_corner.Parent = save_btn

save_btn.MouseButton1Click:Connect(function()
	save_settings()
	status_label.Text = "Settings saved!"
	status_label.TextColor3 = Color3.fromRGB(0, 200, 120)
end)

-- Toggle widget visibility
button.Click:Connect(function()
	widget.Enabled = not widget.Enabled
end)

load_settings()
