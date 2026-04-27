// Copyright (c) 2026 Intelli-verse-X
// MIT License — see LICENSE in the project root.
//
// Xbox / GDK console bridge stub. The real implementation lives in a
// repo gated by the Microsoft Game Development Kit (GDK) license. This
// file compiles in a default IVX build by behaving as a no-op bridge
// so non-Xbox targets do not need a GDK toolchain installed.
//
// Replace with the GDK-flavoured implementation when integrating with
// xbox-live-sdk / XSAPI:
//
//   #include <XGameRuntime.h>
//   #include <XUser.h>
//   #include <XGameSaveFiles.h>
//
// The real implementation flow:
//
//   1. XUserAddAsync(XUserAddOptions::AddDefaultUserAllowingUI, ...)
//   2. XUserGetGamertagAsync → display_name
//   3. XUserGetTokenAndSignatureUtf16Async(... "https://multiplayer.xboxlive.com" ...)
//   4. Cache identity_ticket + expires.
//   5. Subscribe to XGameRuntimeRegisterTokenAndSignatureChangedCallback.
//   6. For invites, XGameInviteRegisterForEvent.

#include "intelliversex/platform/ivx_console_bridge.h"

#if defined(IVX_PLATFORM_XBOX)

namespace ivx {
namespace multiplayer {

class XboxBridge : public IConsoleBridge {
public:
    XboxBridge() = default;
    ~XboxBridge() override = default;

    void RequestIdentity(
        std::function<void(ConsoleIdentity, std::string)> cb) override {
        // Replace with XUserAddAsync + XUserGetTokenAndSignatureUtf16Async.
        cb({}, "ivx-xbox-bridge: GDK identity not yet wired");
    }

    void SubscribeNetworkAvailability(
        std::function<void(ConsoleNetworkAvailability)> cb) override {
        cb(ConsoleNetworkAvailability::Unknown);
    }

    void RequestInviteSend(const std::string& /*match_id*/) override {}

    void SubscribeInviteAccepted(
        std::function<void(const std::string&)> /*cb*/) override {}

    ConsolePlatform Platform() const override {
        return ConsolePlatform::XboxSeries;
    }
};

std::unique_ptr<IConsoleBridge> CreateConsoleBridge() {
    return std::make_unique<XboxBridge>();
}

} // namespace multiplayer
} // namespace ivx

#endif // IVX_PLATFORM_XBOX
