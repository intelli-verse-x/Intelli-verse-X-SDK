-- Copyright (c) 2026 Intelli-verse-X
-- MIT License — see LICENSE in the project root.

--!strict

--- IntelliVerseX Hiro Live-Ops systems for Roblox.
--- Spin wheel, streaks, daily rewards, achievements, season pass, leagues, and more.
--- All systems communicate with Nakama Hiro RPCs via HttpClient.
--- @module Hiro

local Hiro = {}

Hiro.SpinWheel = require(script.SpinWheel)
Hiro.Streaks = require(script.Streaks)
Hiro.DailyRewards = require(script.DailyRewards)
Hiro.DailyMissions = require(script.DailyMissions)
Hiro.Achievements = require(script.Achievements)
Hiro.SeasonPass = require(script.SeasonPass)
Hiro.Leagues = require(script.Leagues)
Hiro.FortuneWheel = require(script.FortuneWheel)
Hiro.Tournaments = require(script.Tournaments)
Hiro.Goals = require(script.Goals)
Hiro.Retention = require(script.Retention)
Hiro.FriendStreaks = require(script.FriendStreaks)

return Hiro
