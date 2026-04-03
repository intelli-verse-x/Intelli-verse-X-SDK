--- Template-expanded IVX / Nakama settings
local M = {}

M.game_id = "{{game_id}}"
M.server_host = "{{server_host}}"
M.server_port = tonumber("{{server_port}}") or 7350
M.server_key = "{{server_key}}"
M.company_name = "{{company_name}}"
M.tagline = "{{tagline}}"
M.max_energy = {{max_energy}}
M.energy_refill_minutes = {{energy_refill_minutes}}
M.initial_coins = {{initial_coins}}
M.initial_gems = {{initial_gems}}

return M
