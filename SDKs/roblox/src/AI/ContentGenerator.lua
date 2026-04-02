-- Copyright (c) 2026 Intelli-verse-X
-- MIT License — see LICENSE in the project root.

--!strict

--- AI Content Generator: procedural game content via LLM.
--- @module AI.ContentGenerator

local HttpClient = require(script.Parent.Parent.HttpClient)

local ContentGenerator = {}

--- Generate text content (quests, dialog, descriptions, lore).
function ContentGenerator.generate_text(prompt: string, params: { [string]: any }?): (any?, string?)
	local body = { prompt = prompt }
	if params then
		for k, v in params do
			body[k] = v
		end
	end
	local resp = HttpClient.ai_request("POST", "/v1/content/text", HttpClient.json_encode(body))
	if resp.ok then
		return resp.body, nil
	end
	return nil, "Text generation failed: HTTP " .. tostring(resp.status)
end

--- Generate structured game data (item stats, enemy configs, level layouts).
function ContentGenerator.generate_structured(schema_name: string, params: { [string]: any }?): (any?, string?)
	local body = { schema = schema_name, params = params or {} }
	local resp = HttpClient.ai_request("POST", "/v1/content/structured", HttpClient.json_encode(body))
	if resp.ok then
		return resp.body, nil
	end
	return nil, "Structured generation failed: HTTP " .. tostring(resp.status)
end

--- Generate quiz questions for a topic.
function ContentGenerator.generate_quiz(topic: string, count: number?, difficulty: string?): (any?, string?)
	local resp = HttpClient.ai_request("POST", "/v1/content/quiz", HttpClient.json_encode({
		topic = topic,
		count = count or 5,
		difficulty = difficulty or "medium",
	}))
	if resp.ok then
		return resp.body, nil
	end
	return nil, "Quiz generation failed: HTTP " .. tostring(resp.status)
end

return ContentGenerator
