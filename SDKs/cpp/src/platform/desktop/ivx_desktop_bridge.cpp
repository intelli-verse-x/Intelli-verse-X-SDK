// Copyright (c) 2026 Intelli-verse-X
// MIT License — see LICENSE in the project root.
//
// Default desktop bridge — used by Windows / macOS / Linux builds (and
// any target where IVX_PLATFORM_* is not defined). Uses environment-
// variable-derived identity so unit / integration tests run without
// needing a console SDK.

#include "intelliversex/platform/ivx_console_bridge.h"

#if !defined(IVX_PLATFORM_XBOX) && !defined(IVX_PLATFORM_PS5) && !defined(IVX_PLATFORM_SWITCH)

#include <chrono>
#include <cstdlib>

namespace ivx {
namespace multiplayer {

namespace {
const char* SafeGetEnv(const char* k) {
    const char* v = std::getenv(k);
    return v == nullptr ? "" : v;
}
}

class DesktopBridge : public IConsoleBridge {
public:
    void RequestIdentity(
        std::function<void(ConsoleIdentity, std::string)> cb) override {
        ConsoleIdentity id;
        id.platform_user_id = SafeGetEnv("IVX_DEV_USER_ID");
        if (id.platform_user_id.empty()) {
            id.platform_user_id = "ivx-dev-anon";
        }
        id.display_name     = SafeGetEnv("IVX_DEV_DISPLAY_NAME");
        id.identity_ticket  = SafeGetEnv("IVX_DEV_TICKET");
        id.region_hint      = SafeGetEnv("IVX_DEV_REGION");
        id.ticket_expires_unix_ms =
            std::chrono::duration_cast<std::chrono::milliseconds>(
                std::chrono::system_clock::now().time_since_epoch())
                .count() + 60LL * 60 * 1000;
        cb(std::move(id), "");
    }

    void SubscribeNetworkAvailability(
        std::function<void(ConsoleNetworkAvailability)> cb) override {
        cb(ConsoleNetworkAvailability::Online);
    }

    void RequestInviteSend(const std::string& /*match_id*/) override {}

    void SubscribeInviteAccepted(
        std::function<void(const std::string&)> /*cb*/) override {}

    ConsolePlatform Platform() const override {
        return ConsolePlatform::Unknown;
    }
};

std::unique_ptr<IConsoleBridge> CreateConsoleBridge() {
    return std::make_unique<DesktopBridge>();
}

} // namespace multiplayer
} // namespace ivx

#endif
