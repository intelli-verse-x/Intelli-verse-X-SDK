-- Copyright (c) 2026 Intelli-verse-X
-- MIT License — see LICENSE in the project root.

--!strict

--- HTTP client wrapping Roblox HttpService with retry, auth headers, and JSON.
--- All calls MUST originate from ServerScripts (HttpService restriction).
--- @module HttpClient

local HttpService = game:GetService("HttpService")
local Config = require(script.Parent.Config)

local HttpClient = {}

local MAX_RETRIES = 2
local RETRY_DELAY = 1

local B64_CHARS = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789+/"

local function _base64_encode(input: string): string
	local out = {}
	local len = #input
	local i = 1
	while i <= len do
		local a = string.byte(input, i) or 0
		local b = if i + 1 <= len then string.byte(input, i + 1) else 0
		local c = if i + 2 <= len then string.byte(input, i + 2) else 0
		local triple = bit32.bor(bit32.lshift(a, 16), bit32.lshift(b, 8), c)
		table.insert(out, string.sub(B64_CHARS, bit32.rshift(triple, 18) % 64 + 1, bit32.rshift(triple, 18) % 64 + 1))
		table.insert(out, string.sub(B64_CHARS, bit32.rshift(triple, 12) % 64 + 1, bit32.rshift(triple, 12) % 64 + 1))
		if i + 1 <= len then
			table.insert(out, string.sub(B64_CHARS, bit32.rshift(triple, 6) % 64 + 1, bit32.rshift(triple, 6) % 64 + 1))
		else
			table.insert(out, "=")
		end
		if i + 2 <= len then
			table.insert(out, string.sub(B64_CHARS, triple % 64 + 1, triple % 64 + 1))
		else
			table.insert(out, "=")
		end
		i += 3
	end
	return table.concat(out)
end

export type HttpResponse = {
	ok: boolean,
	status: number,
	body: any,
	raw: string?,
}

local function _log(fmt: string, ...: any)
	if Config.get().debug then
		print(string.format("[IVX.Http] " .. fmt, ...))
	end
end

local function _auth_header(session_token: string?): { [string]: string }
	local headers: { [string]: string } = {
		["Content-Type"] = "application/json",
		["Accept"] = "application/json",
	}

	if session_token and session_token ~= "" then
		headers["Authorization"] = "Bearer " .. session_token
	else
		local key = Config.get().server_key
		headers["Authorization"] = "Basic " .. _base64_encode(key .. ":")
	end

	return headers
end

function HttpClient.request(
	method: string,
	url: string,
	body: string?,
	session_token: string?,
	custom_headers: { [string]: string }?
): HttpResponse
	local headers = _auth_header(session_token)
	if custom_headers then
		for k, v in custom_headers do
			headers[k] = v
		end
	end

	local attempt = 0
	while attempt <= MAX_RETRIES do
		local success, result = pcall(function()
			return HttpService:RequestAsync({
				Url = url,
				Method = method,
				Headers = headers,
				Body = (method ~= "GET" and body) or nil,
			})
		end)

		if success and result then
			local response_body: any = nil
			if result.Body and result.Body ~= "" then
				local decode_ok, decoded = pcall(HttpService.JSONDecode, HttpService, result.Body)
				response_body = decode_ok and decoded or result.Body
			end

			local ok = result.StatusCode >= 200 and result.StatusCode < 300
			_log("%s %s -> %d", method, url, result.StatusCode)

			return {
				ok = ok,
				status = result.StatusCode,
				body = response_body,
				raw = result.Body,
			}
		end

		attempt += 1
		if attempt <= MAX_RETRIES then
			_log("Retry %d/%d for %s %s", attempt, MAX_RETRIES, method, url)
			task.wait(RETRY_DELAY * attempt)
		end
	end

	return { ok = false, status = 0, body = nil, raw = nil }
end

function HttpClient.get(path: string, session_token: string?): HttpResponse
	return HttpClient.request("GET", Config.base_url() .. path, nil, session_token)
end

function HttpClient.post(path: string, payload: { [string]: any }?, session_token: string?): HttpResponse
	local body = if payload then HttpService:JSONEncode(payload) else "{}"
	return HttpClient.request("POST", Config.base_url() .. path, body, session_token)
end

function HttpClient.put(path: string, payload: { [string]: any }?, session_token: string?): HttpResponse
	local body = if payload then HttpService:JSONEncode(payload) else "{}"
	return HttpClient.request("PUT", Config.base_url() .. path, body, session_token)
end

function HttpClient.rpc(rpc_id: string, payload: string?, session_token: string): HttpResponse
	local encoded_payload = if payload then HttpService:UrlEncode(payload) else ""
	local url = string.format("%s/v2/rpc/%s?unwrap&payload=%s", Config.base_url(), rpc_id, encoded_payload)
	return HttpClient.request("GET", url, nil, session_token)
end

function HttpClient.rpc_post(rpc_id: string, payload: string?, session_token: string): HttpResponse
	local url = string.format("%s/v2/rpc/%s?unwrap", Config.base_url(), rpc_id)
	return HttpClient.request("POST", url, payload or "{}", session_token)
end

function HttpClient.ai_request(method: string, path: string, body: string?): HttpResponse
	local cfg = Config.get()
	local url = cfg.ai_base_url .. path
	local headers: { [string]: string } = {
		["Content-Type"] = "application/json",
		["Accept"] = "application/json",
	}
	if cfg.ai_api_key ~= "" then
		headers["Authorization"] = "Bearer " .. cfg.ai_api_key
	end
	return HttpClient.request(method, url, body, nil, headers)
end

function HttpClient.json_encode(tbl: any): string
	return HttpService:JSONEncode(tbl)
end

function HttpClient.json_decode(str: string): any
	return HttpService:JSONDecode(str)
end

return HttpClient
