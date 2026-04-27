// Copyright (c) 2026 Intelli-verse-X
// MIT License — see LICENSE in the project root.
//
// IVXConsoleBridge — platform abstraction layer that maps the IVX
// multiplayer adapter onto the platform-specific networking + identity
// stacks shipped by Nintendo Switch (NEX/NSA), Sony PlayStation 5
// (PSN/NP), and Microsoft Xbox / GDK (XSAPI).
//
// THIS HEADER IS PUBLIC. The implementation files live behind NDA in
// platform-specific repos:
//
//   * src/platform/switch/   — requires NintendoSDK + NEX (NDA)
//   * src/platform/ps5/      — requires PS5 SDK + libNp (NDA)
//   * src/platform/xbox/     — requires GDK + XSAPI (publicly available
//                               with a Xbox dev account; this repo can
//                               carry a stub).
//
// Games / the IVX core multiplayer module never touch console SDKs
// directly. They construct a console-specific subclass via the
// platform factory and pass it to IVXMultiplayerKernel::Initialize.

#pragma once

#include <cstdint>
#include <functional>
#include <memory>
#include <string>

namespace ivx {
namespace multiplayer {

/// Identifier of the host console.
enum class ConsolePlatform : uint8_t {
    Unknown = 0,
    Switch  = 1,
    PS5     = 2,
    XboxOne = 3,
    XboxSeries = 4
};

/// Platform-issued, per-user identity used to authenticate Nakama.
/// All three console platforms expose a verifiable JWT-ish blob for
/// online services; we stitch it through to Nakama's `authenticateCustom`
/// or device auth flow so the IVX server can assert the platform user
/// id without trusting the client.
struct ConsoleIdentity {
    /// Stable platform user id (Switch: NSA principal id, PS5: account
    /// id, Xbox: XUID).
    std::string platform_user_id;

    /// Display name as the platform reports it. Subject to friendly
    /// name policies — DO NOT use as a primary key.
    std::string display_name;

    /// Platform-issued ticket / JWT. The IVX server validates this
    /// using the platform's documented public key endpoint.
    std::string identity_ticket;
    int64_t     ticket_expires_unix_ms = 0;

    /// Region hint surfaced by the platform (e.g. "us", "eu", "jp").
    /// Used by the kernel multi-region router to pick a Nakama pod.
    std::string region_hint;
};

/// Platform-specific overrides for transport. PS5 and Xbox prefer
/// the platform's own UDP path; we still route IVX kernel traffic
/// through Nakama-cpp's WebSocket but the matchmaking signal can be
/// exchanged via platform party services to reduce join latency.
struct ConsoleTransportOptions {
    /// If non-empty, the bridge will listen for invite drops on this
    /// service id. PS5: NP invitation, Switch: presence service, Xbox:
    /// MPSD session.
    std::string invite_service_id;

    /// Disable IVX's own friendly-name matchmaking when the platform
    /// already provides one (Xbox SmartMatch, PS5 Match, Switch NEX).
    /// Default false; IVX matchmaking is the canonical path because
    /// only it understands kernel template ids + tournament state.
    bool defer_matchmaking_to_platform = false;
};

/// Platform-specific certification gate. Most consoles require games
/// to prove they degrade gracefully when the platform's online
/// services are unreachable. The bridge surfaces these signals so the
/// IVX adapter can show the right error.
enum class ConsoleNetworkAvailability : uint8_t {
    Unknown        = 0,
    Online         = 1,
    Offline        = 2,
    SignedOut      = 3,
    ParentalLocked = 4,
    Maintenance    = 5
};

/// Console bridge contract. Concrete subclasses live under platform-
/// specific subtrees and are NEVER linked into a non-console build.
class IConsoleBridge {
public:
    virtual ~IConsoleBridge() = default;

    /// Resolve the local user's platform identity. Implementations are
    /// async: the callback fires on the bridge's thread (NOT the IVX
    /// kernel thread); the kernel will marshal it.
    virtual void RequestIdentity(
        std::function<void(ConsoleIdentity, std::string /*err*/)> cb) = 0;

    /// Subscribe to network availability transitions. The IVX adapter
    /// uses this to pre-empt connection attempts when the platform is
    /// offline (avoids mass-retry storms on cert).
    virtual void SubscribeNetworkAvailability(
        std::function<void(ConsoleNetworkAvailability)> cb) = 0;

    /// Open the platform's friend / invite flow. IVX matches encode
    /// `match_id` as the platform invite payload so `OnInviteAccepted`
    /// receives it round-trip.
    virtual void RequestInviteSend(const std::string& match_id) = 0;

    /// Subscribe to inbound invites. The handler runs on the bridge
    /// thread; the kernel marshals the join attempt.
    virtual void SubscribeInviteAccepted(
        std::function<void(const std::string& match_id)> cb) = 0;

    /// Console identifier. Used for telemetry tagging and to enable
    /// cert-specific error messages ("Sign in to Xbox Live").
    virtual ConsolePlatform Platform() const = 0;

    /// Platform-specific transport hints applied when the kernel opens
    /// its Nakama socket.
    virtual ConsoleTransportOptions TransportOptions() const { return {}; }
};

/// Factory function — implemented in the platform-specific
/// translation unit. Return nullptr if the bridge can't initialise
/// (missing platform cert, no signed-in user, dev-mode parental lock).
std::unique_ptr<IConsoleBridge> CreateConsoleBridge();

} // namespace multiplayer
} // namespace ivx
