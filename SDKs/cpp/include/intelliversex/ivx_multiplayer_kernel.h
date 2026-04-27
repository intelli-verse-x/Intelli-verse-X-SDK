// Copyright (c) 2026 Intelli-verse-X
// MIT License — see LICENSE in the project root.
//
// IVXMultiplayerKernel — C++ adapter for the IntelliVerseX Multiplayer
// Kernel. Mirrors the IIVXMultiplayer / IIVXMatchSession contract from the
// Unity, JS, Unreal, Godot, Flutter, Java, and Web3 SDKs.
//
// Wraps the official `nakama-cpp` client and speaks the wire protocol
// defined in `Intelli-verse-X-SDK/schemas/multiplayer/*.proto`. Used directly
// by C++ games, by the Cocos2d-x adapter (which is just C++), and by the
// Defold adapter (via a thin Lua bridge).

#pragma once

#include <atomic>
#include <cstdint>
#include <functional>
#include <memory>
#include <mutex>
#include <string>
#include <unordered_map>
#include <vector>

namespace Nakama {
    class NClientInterface;
    class NSessionInterface;
    class NRtClientInterface;
    struct NMatchData;
    struct NMatchPresenceEvent;
    struct NMatch;
}

namespace ivx {
namespace multiplayer {

enum class TransportState : uint8_t {
    Disconnected = 0,
    Connecting   = 1,
    Connected    = 2,
    Reconnecting = 3,
    FailedFatal  = 4
};

enum class EndReason : uint8_t {
    Unknown            = 0,
    Completed          = 1,
    Cancelled          = 2,
    DurationExceeded   = 3,
    KernelInternal     = 4,
    AllPlayersLeft     = 5,
    HostTerminated     = 6
};

struct Header {
    int64_t  seq            = 0;
    int64_t  match_time_ms  = 0;
    std::string uuid;
    int32_t  op_code        = 0;
    std::string sender_user_id;
};

struct Envelope {
    Header   header;
    /// Decoded JSON payload as a string (kernel emits structured JSON; the
    /// adapter intentionally does NOT parse it for you so games can pick
    /// their own JSON library — RapidJSON, simdjson, nlohmann::json).
    std::string payload_json;
    int64_t  recv_unix_ms   = 0;
};

struct CreateMatchRequest {
    std::string template_id;
    std::string game_id;
    std::string region;
    /// Server-side template_init payload (raw JSON object). Empty string
    /// is treated as "{}".
    std::string template_init_json;
};

struct CreateMatchResponse {
    std::string match_id;
    std::string template_id;
    std::string region;
    int64_t     expires_unix_ms = 0;
    bool        ok              = false;
    /// On error, this is non-empty. On success, empty.
    std::string error_message;
};

using EnvelopeHandler  = std::function<void(const Envelope&)>;
using StateHandler     = std::function<void(TransportState)>;
using CreateMatchCb    = std::function<void(const CreateMatchResponse&)>;

class MultiplayerKernel;
class MatchSession;

/**
 * Subscription token returned from MatchSession::Subscribe. RAII-friendly:
 * call .Dispose() or let it go out of scope to unsubscribe.
 */
class Subscription {
public:
    using DisposeFn = std::function<void()>;
    Subscription() = default;
    explicit Subscription(DisposeFn fn) : dispose_(std::move(fn)) {}
    ~Subscription() { Dispose(); }
    Subscription(Subscription&& o) noexcept : dispose_(std::move(o.dispose_)) { o.dispose_ = nullptr; }
    Subscription& operator=(Subscription&& o) noexcept {
        if (this != &o) { Dispose(); dispose_ = std::move(o.dispose_); o.dispose_ = nullptr; }
        return *this;
    }
    Subscription(const Subscription&)            = delete;
    Subscription& operator=(const Subscription&) = delete;

    void Dispose() {
        if (dispose_) { dispose_(); dispose_ = nullptr; }
    }
    bool IsActive() const { return static_cast<bool>(dispose_); }
private:
    DisposeFn dispose_;
};

/**
 * Live handle for one joined match.
 *
 * Thread-safety:
 *   - Subscribe / SubscribeRange / Send / Leave / Dispose are thread-safe.
 *   - Inbound dispatch happens on whichever thread the underlying nakama-cpp
 *     event loop tick runs on (game-defined; usually the main thread).
 */
class MatchSession {
public:
    /// Construction is internal; obtain one from MultiplayerKernel::JoinMatch.
    MatchSession(MultiplayerKernel* owner, std::string match_id, std::string local_user_id);
    ~MatchSession();

    const std::string& GetMatchId()      const noexcept { return match_id_; }
    const std::string& GetTemplateId()   const noexcept { return template_id_; }
    const std::string& GetLocalUserId()  const noexcept { return local_user_id_; }
    int64_t            CurrentMatchTimeMs() const noexcept { return current_match_time_ms_.load(); }
    int32_t            ActivePlayerCount()  const noexcept { return active_player_count_.load(); }
    TransportState     GetState()           const noexcept { return state_.load(); }

    Subscription Subscribe(int32_t op_code, EnvelopeHandler handler);
    Subscription SubscribeRange(int32_t op_code_from, int32_t op_code_to, EnvelopeHandler handler);
    Subscription OnTransportStateChanged(StateHandler handler);

    /// Send `payload_json` to the server. The adapter stamps the kernel
    /// header (seq / match_time_ms / uuid) for you. Pass `{}` for empty.
    void Send(int32_t op_code, const std::string& payload_json);

    /// Politely leave the match. Idempotent.
    void Leave();

    /// Tear down handlers; calls Leave() for you. Idempotent.
    void Dispose();

    // ---- internal (called by MultiplayerKernel's dispatcher) ----
    void Internal_Dispatch(const Envelope& env);
    void Internal_SetState(TransportState s);
    void Internal_HandlePresence(int32_t joined, int32_t left);
    void Internal_SetTemplateId(std::string tid) { template_id_ = std::move(tid); }

private:
    MultiplayerKernel* owner_;
    std::string match_id_;
    std::string local_user_id_;
    std::string template_id_;
    std::atomic<int64_t> current_match_time_ms_{0};
    std::atomic<int32_t> active_player_count_{0};
    std::atomic<TransportState> state_{TransportState::Connecting};
    std::atomic<int64_t> local_seq_{0};
    std::atomic<bool> disposed_{false};

    struct ExactBinding   { int32_t op; uint64_t id; EnvelopeHandler handler; };
    struct RangeBinding   { int32_t from; int32_t to; uint64_t id; EnvelopeHandler handler; };
    struct StateBinding   { uint64_t id; StateHandler handler; };

    std::mutex mtx_;
    std::vector<ExactBinding> exact_;
    std::vector<RangeBinding> ranges_;
    std::vector<StateBinding> state_handlers_;
    std::atomic<uint64_t>     next_id_{1};

    static std::string MakeUuidV4();
    static std::string BuildEnvelopeJson(int64_t seq, int64_t match_time_ms,
                                         const std::string& uuid, const std::string& payload_json);
};

/**
 * Top-level adapter. One per authenticated player.
 *
 * Lifecycle: construct with a fully-authenticated nakama-cpp client + session;
 * call Initialize(); use CreateMatch / JoinMatch / CreateAndJoin; call
 * Shutdown when done. Initialize is idempotent.
 *
 * The adapter does NOT spin its own event loop — call your nakama-cpp
 * client's `tick()` method on your main update loop as usual.
 */
class MultiplayerKernel {
public:
    MultiplayerKernel(std::shared_ptr<Nakama::NClientInterface> client,
                      std::shared_ptr<Nakama::NSessionInterface> session);
    ~MultiplayerKernel();

    MultiplayerKernel(const MultiplayerKernel&)            = delete;
    MultiplayerKernel& operator=(const MultiplayerKernel&) = delete;

    bool           IsInitialized()  const noexcept { return initialized_.load(); }
    TransportState GetTransportState() const noexcept { return transport_.load(); }

    /// Open the realtime socket. Idempotent. Returns false if the underlying
    /// client/session is null. Asynchronous — observe TransportState via
    /// OnTransportStateChanged.
    bool Initialize();

    /// Tear down sockets, joined matches, and pending callbacks. Idempotent.
    void Shutdown();

    /// `mp_create_match` Nakama RPC. Async; result in `cb`.
    void CreateMatch(const CreateMatchRequest& req, CreateMatchCb cb);

    /// Join an existing match. Async.
    void JoinMatch(const std::string& match_id,
                   std::function<void(std::shared_ptr<MatchSession>)> cb);

    /// Convenience: create + join in one call.
    void CreateAndJoin(const CreateMatchRequest& req,
                       std::function<void(std::shared_ptr<MatchSession>)> cb);

    Subscription OnTransportStateChanged(StateHandler handler);

    // ---- internal ----
    void Internal_OnMatchData(const Nakama::NMatchData& data);
    void Internal_OnMatchPresence(const Nakama::NMatchPresenceEvent& evt);
    void Internal_RemoveSession(const std::string& match_id);
    Nakama::NRtClientInterface* Internal_RtClient() { return rt_client_.get(); }

private:
    std::shared_ptr<Nakama::NClientInterface>  client_;
    std::shared_ptr<Nakama::NSessionInterface> session_;
    std::shared_ptr<Nakama::NRtClientInterface> rt_client_;
    std::atomic<bool>           initialized_{false};
    std::atomic<TransportState> transport_{TransportState::Disconnected};
    std::mutex sessions_mtx_;
    std::unordered_map<std::string, std::weak_ptr<MatchSession>> active_sessions_;

    struct StateBinding { uint64_t id; StateHandler handler; };
    std::mutex state_handlers_mtx_;
    std::vector<StateBinding> state_handlers_;
    std::atomic<uint64_t> next_state_id_{1};

    void SetState(TransportState s);
    static bool ParseEnvelope(const std::string& body, int32_t op_code,
                              const std::string& sender, Envelope& out);
};

} // namespace multiplayer
} // namespace ivx
