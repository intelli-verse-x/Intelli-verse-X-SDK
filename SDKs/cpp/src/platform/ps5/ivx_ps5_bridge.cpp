// Copyright (c) 2026 Intelli-verse-X
// MIT License — see LICENSE in the project root.
//
// PS5 / libNp console bridge stub. The real implementation requires
// the Sony PS5 SDK and is delivered to licensed studios via Sony's
// developer portal. This stub keeps the IVX build green for non-PS5
// targets and serves as the integration template:
//
//   #include <np.h>
//   #include <np_signaling.h>
//   #include <user_service.h>
//
//   1. sceUserServiceInitialize / sceUserServiceGetInitialUser → user_id.
//   2. sceNpManagerSignIn (if needed).
//   3. sceNpAuthCreateRequest + sceNpAuthGetAuthorizationCode →
//      identity_ticket. Use it as Nakama device-auth secret.
//   4. sceNpInvitationDialogOpen for invite send.
//   5. sceNpStateCallbackRegister for online/offline transitions.

#include "intelliversex/platform/ivx_console_bridge.h"

#if defined(IVX_PLATFORM_PS5)

namespace ivx {
namespace multiplayer {

class PS5Bridge : public IConsoleBridge {
public:
    PS5Bridge() = default;
    ~PS5Bridge() override = default;

    void RequestIdentity(
        std::function<void(ConsoleIdentity, std::string)> cb) override {
        cb({}, "ivx-ps5-bridge: PSN identity not yet wired");
    }

    void SubscribeNetworkAvailability(
        std::function<void(ConsoleNetworkAvailability)> cb) override {
        cb(ConsoleNetworkAvailability::Unknown);
    }

    void RequestInviteSend(const std::string& /*match_id*/) override {}

    void SubscribeInviteAccepted(
        std::function<void(const std::string&)> /*cb*/) override {}

    ConsolePlatform Platform() const override {
        return ConsolePlatform::PS5;
    }
};

std::unique_ptr<IConsoleBridge> CreateConsoleBridge() {
    return std::make_unique<PS5Bridge>();
}

} // namespace multiplayer
} // namespace ivx

#endif // IVX_PLATFORM_PS5
