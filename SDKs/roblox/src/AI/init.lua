-- Copyright (c) 2026 Intelli-verse-X
-- MIT License — see LICENSE in the project root.

--!strict

--- IntelliVerseX AI module for Roblox.
--- Provides AI NPC dialog, voice, content generation, moderation, and profiling.
--- All calls go through HttpService to AI/LLM endpoints (server-side only).
--- @module AI

local AI = {}

AI.Voice = require(script.Voice)
AI.Assistant = require(script.Assistant)
AI.ContentGenerator = require(script.ContentGenerator)
AI.Moderator = require(script.Moderator)
AI.NPC = require(script.NPC)
AI.Profiler = require(script.Profiler)

return AI
