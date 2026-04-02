// Copyright (c) 2026 Intelli-verse-X
// MIT License — see LICENSE in the project root.

#pragma once

/// @file ivx.h
/// @brief Single-include header for the IntelliVerseX C/C++ SDK.

#include "ivx_config.h"
#include "ivx_types.h"
#include "ivx_ai_types.h"
#include "ivx_manager.h"
#include "ivx_ai_client.h"
#include "ivx_game_modes.h"
#include "ivx_hiro_systems.h"

// Discord DMs & moderation / AI LLM stack (stub surfaces; optional includes)
#include "IVXDiscordMessages.h"
#include "IVXDiscordModeration.h"
#include "IVXAINPCDialogManager.h"
#include "IVXAIAssistant.h"
#include "IVXAIModerator.h"
#include "IVXAIContentGenerator.h"
#include "IVXAIProfiler.h"
#include "IVXAIVoiceServices.h"
