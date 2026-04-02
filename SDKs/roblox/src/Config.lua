-- Copyright (c) 2026 Intelli-verse-X
-- MIT License — see LICENSE in the project root.

--!strict

--- Configuration store for the IntelliVerseX Roblox SDK.
--- @module Config

export type IVXConfig = {
	game_id: string,
	host: string,
	port: number,
	server_key: string,
	use_ssl: boolean,
	ai_base_url: string,
	ai_api_key: string,
	debug: boolean,
}

local DEFAULT: IVXConfig = {
	game_id = "",
	host = "nakama-rest.intelli-verse-x.ai",
	port = 443,
	server_key = "defaultkey",
	use_ssl = true,
	ai_base_url = "https://ai.intelli-verse-x.ai",
	ai_api_key = "",
	debug = false,
}

local Config = {}
local _current: IVXConfig = table.clone(DEFAULT)

function Config.set(opts: { [string]: any })
	for k, v in opts do
		if DEFAULT[k] ~= nil then
			(_current :: any)[k] = v
		end
	end

	if _current.game_id == "" then
		warn("[IntelliVerseX] game_id is empty. Get yours from https://intelli-verse-x.ai/developers")
	end
end

function Config.get(): IVXConfig
	return _current
end

function Config.base_url(): string
	local scheme = if _current.use_ssl then "https" else "http"
	return string.format("%s://%s:%d", scheme, _current.host, _current.port)
end

function Config.reset()
	_current = table.clone(DEFAULT)
end

return Config
