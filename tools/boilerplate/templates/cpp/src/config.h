#pragma once

#include <cstdint>

namespace ivx_config
{
inline constexpr char const* GAME_ID = "{{game_id}}";
inline constexpr char const* SERVER_HOST = "{{server_host}}";
inline constexpr int SERVER_PORT = {{server_port}};
inline constexpr char const* SERVER_KEY = "{{server_key}}";
inline constexpr char const* COMPANY_NAME = "{{company_name}}";
inline constexpr char const* TAGLINE = "{{tagline}}";
inline constexpr int MAX_ENERGY = {{max_energy}};
inline constexpr int ENERGY_REFILL_MINUTES = {{energy_refill_minutes}};
inline constexpr std::int64_t INITIAL_COINS = {{initial_coins}};
inline constexpr std::int64_t INITIAL_GEMS = {{initial_gems}};
} // namespace ivx_config
