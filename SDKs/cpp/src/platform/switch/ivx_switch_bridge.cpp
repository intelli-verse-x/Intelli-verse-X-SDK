// Copyright (c) 2026 Intelli-verse-X
// MIT License — see LICENSE in the project root.
//
// Nintendo Switch console bridge stub. The real implementation requires
// the Nintendo NEX SDK and is delivered to licensed studios via the
// Nintendo Developer Portal. This stub keeps the IVX build green for
// non-Switch targets:
//
//   #include <nn/account.h>
//   #include <nn/nex.h>
//
//   1. nn::account::OpenPreselectedUser → UserHandle.
//   2. nn::account::GetNickname → display_name.
//   3. nn::account::CreateAuthorizationRequest → identity_ticket.
//   4. nn::nex::Initialize + nn::nex::Friends for presence/invite.

#include "intelliversex/platform/ivx_console_bridge.h"

#if defined(IVX_PLATFORM_SWITCH)

namespace ivx {
namespace multiplayer {

class SwitchBridge : public IConsoleBridge {
public:
    SwitchBridge() = default;
    ~SwitchBridge() override = default;

    void RequestIdentity(
        std::function<void(ConsoleIdentity, std::string)> cb) override {
        cb({}, "ivx-switch-bridge: NSA identity not yet wired");
    }

    void SubscribeNetworkAvailability(
        std::function<void(ConsoleNetworkAvailability)> cb) override {
        cb(ConsoleNetworkAvailability::Unknown);
    }

    void RequestInviteSend(const std::string& /*match_id*/) override {}

    void SubscribeInviteAccepted(
        std::function<void(const std::string&)> /*cb*/) override {}

    ConsolePlatform Platform() const override {
        return ConsolePlatform::Switch;
    }
};

std::unique_ptr<IConsoleBridge> CreateConsoleBridge() {
    return std::make_unique<SwitchBridge>();
}

} // namespace multiplayer
} // namespace ivx

#endif // IVX_PLATFORM_SWITCH
