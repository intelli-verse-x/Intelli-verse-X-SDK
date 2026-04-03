--[[
  IntelliVerseX starter — shared config (template-expanded).
  primary_color / secondary_color / background_color: RRGGBB or #RRGGBB
]]
local function hexToColor3(hex: string): Color3
	hex = hex:gsub("#", "")
	local r = tonumber(hex:sub(1, 2), 16) or 0
	local g = tonumber(hex:sub(3, 4), 16) or 0
	local b = tonumber(hex:sub(5, 6), 16) or 0
	return Color3.fromRGB(r, g, b)
end

local Config = {}

Config.GAME_ID = "{{game_id}}"
Config.SERVER_HOST = "{{server_host}}"
Config.SERVER_PORT = tonumber("{{server_port}}") or 7350
Config.SERVER_KEY = "{{server_key}}"
Config.PRIMARY_COLOR = hexToColor3("{{primary_color}}")
Config.SECONDARY_COLOR = hexToColor3("{{secondary_color}}")
Config.BACKGROUND_COLOR = hexToColor3("{{background_color}}")
Config.MAX_ENERGY = {{max_energy}}
Config.ENERGY_REFILL_MINUTES = {{energy_refill_minutes}}
Config.INITIAL_COINS = {{initial_coins}}
Config.INITIAL_GEMS = {{initial_gems}}

return Config
