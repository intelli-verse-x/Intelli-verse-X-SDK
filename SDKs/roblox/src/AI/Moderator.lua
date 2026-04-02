-- Copyright (c) 2026 Intelli-verse-X
-- MIT License — see LICENSE in the project root.

--!strict

--- AI Content Moderator: real-time text and content moderation.
--- @module AI.Moderator

local HttpClient = require(script.Parent.Parent.HttpClient)

local Moderator = {}

export type ModerationResult = {
	flagged: boolean,
	categories: { [string]: boolean }?,
	scores: { [string]: number }?,
	filtered_text: string?,
}

--- Moderate a text string for policy violations.
function Moderator.check_text(text: string, context: string?): (ModerationResult?, string?)
	local resp = HttpClient.ai_request("POST", "/v1/moderation/text", HttpClient.json_encode({
		text = text,
		context = context,
	}))
	if resp.ok then
		return resp.body :: ModerationResult, nil
	end
	return nil, "Moderation failed: HTTP " .. tostring(resp.status)
end

--- Moderate a player username or display name.
function Moderator.check_username(username: string): (ModerationResult?, string?)
	local resp = HttpClient.ai_request("POST", "/v1/moderation/username", HttpClient.json_encode({
		username = username,
	}))
	if resp.ok then
		return resp.body :: ModerationResult, nil
	end
	return nil, "Username moderation failed: HTTP " .. tostring(resp.status)
end

--- Batch-moderate multiple texts.
function Moderator.check_batch(texts: { string }): ({ ModerationResult }?, string?)
	local resp = HttpClient.ai_request("POST", "/v1/moderation/batch", HttpClient.json_encode({
		texts = texts,
	}))
	if resp.ok then
		return resp.body :: { ModerationResult }, nil
	end
	return nil, "Batch moderation failed: HTTP " .. tostring(resp.status)
end

return Moderator
