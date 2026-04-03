#pragma once

#include "CoreMinimal.h"

/**
 * Compile-time template values for Nakama / IntelliVerseX.
 */
struct FGameConfig
{
	static constexpr TCHAR const* GAME_ID = TEXT("{{game_id}}");
	static constexpr TCHAR const* SERVER_HOST = TEXT("{{server_host}}");
	static constexpr int32 SERVER_PORT = {{server_port}};
	static constexpr TCHAR const* SERVER_KEY = TEXT("{{server_key}}");
	static constexpr TCHAR const* TAGLINE = TEXT("{{tagline}}");
	static constexpr int32 MAX_ENERGY = {{max_energy}};
	static constexpr int32 ENERGY_REFILL_MINUTES = {{energy_refill_minutes}};
	static constexpr int64 INITIAL_COINS = {{initial_coins}};
	static constexpr int64 INITIAL_GEMS = {{initial_gems}};
};
