// Copyright (c) 2026 Intelli-verse-X
// MIT License — see LICENSE in the project root.
//
// IVXMultiplayerKernel — C++ implementation. See `ivx_multiplayer_kernel.h`
// for the public contract and rationale.
//
// Wire protocol: schemas/multiplayer/*.proto (kernel envelope = {h:{s,t,u}, p:<payload>}).
// Server handlers: nakama/data/modules/src/multiplayer-kernel/*.

#include "intelliversex/ivx_multiplayer_kernel.h"

#include <chrono>
#include <random>
#include <sstream>
#include <iomanip>

// nakama-cpp headers. The user's build system must point at the installed
// nakama-cpp include path (see SDKs/cpp/CMakeLists.txt).
#include "nakama-cpp/Nakama.h"
#include "nakama-cpp/realtime/NRtClientInterface.h"
#include "nakama-cpp/realtime/rtdata/NMatchData.h"
#include "nakama-cpp/realtime/rtdata/NMatchPresenceEvent.h"
#include "nakama-cpp/data/NMatch.h"

namespace ivx { namespace multiplayer {

namespace {

int64_t NowUnixMs() {
    using namespace std::chrono;
    return duration_cast<milliseconds>(system_clock::now().time_since_epoch()).count();
}

// Minimal embedded JSON helpers. We deliberately avoid a heavy JSON dep so
// nakama-cpp's include footprint stays the same. The kernel envelope shape
// is fixed (`{"h":{"s":N,"t":N,"u":S},"p":<value>}`) so we hand-roll it.

std::string EscapeJsonString(const std::string& s) {
    std::ostringstream o;
    for (char c : s) {
        switch (c) {
            case '"':  o << "\\\""; break;
            case '\\': o << "\\\\"; break;
            case '\b': o << "\\b";  break;
            case '\f': o << "\\f";  break;
            case '\n': o << "\\n";  break;
            case '\r': o << "\\r";  break;
            case '\t': o << "\\t";  break;
            default:
                if (static_cast<unsigned char>(c) < 0x20) {
                    o << "\\u" << std::hex << std::setw(4) << std::setfill('0')
                      << static_cast<int>(c) << std::dec;
                } else {
                    o << c;
                }
        }
    }
    return o.str();
}

bool LooksLikeJsonObjectOrArray(const std::string& s) {
    for (char c : s) {
        if (c == ' ' || c == '\t' || c == '\n' || c == '\r') continue;
        return c == '{' || c == '[';
    }
    return false;
}

// Pull a top-level field's RAW JSON sub-string out of a JSON object body.
// Returns true if found; sub_out points at the verbatim sub-tree (object,
// array, or primitive). NOT a complete parser — adequate for the fixed
// kernel envelope shape and tolerated non-strict input.
bool ExtractTopLevelField(const std::string& body, const std::string& key, std::string& sub_out) {
    const std::string needle = "\"" + key + "\"";
    size_t pos = body.find(needle);
    if (pos == std::string::npos) return false;
    pos = body.find(':', pos + needle.size());
    if (pos == std::string::npos) return false;
    pos++;
    while (pos < body.size() && (body[pos] == ' ' || body[pos] == '\t' || body[pos] == '\n' || body[pos] == '\r')) pos++;
    if (pos >= body.size()) return false;
    char c = body[pos];
    if (c == '{' || c == '[') {
        char open = c;
        char close = (c == '{') ? '}' : ']';
        int depth = 0;
        bool in_str = false;
        bool escape = false;
        size_t start = pos;
        for (; pos < body.size(); ++pos) {
            char ch = body[pos];
            if (in_str) {
                if (escape) { escape = false; }
                else if (ch == '\\') { escape = true; }
                else if (ch == '"') { in_str = false; }
            } else {
                if (ch == '"') in_str = true;
                else if (ch == open)  depth++;
                else if (ch == close) { depth--; if (depth == 0) { sub_out = body.substr(start, pos - start + 1); return true; } }
            }
        }
        return false;
    } else if (c == '"') {
        size_t start = pos;
        bool escape = false;
        for (++pos; pos < body.size(); ++pos) {
            if (escape) { escape = false; continue; }
            if (body[pos] == '\\') { escape = true; continue; }
            if (body[pos] == '"')  { sub_out = body.substr(start, pos - start + 1); return true; }
        }
        return false;
    } else {
        // Primitive (number, true/false/null).
        size_t start = pos;
        while (pos < body.size() && body[pos] != ',' && body[pos] != '}' && body[pos] != ']'
               && body[pos] != ' ' && body[pos] != '\t' && body[pos] != '\n' && body[pos] != '\r') pos++;
        sub_out = body.substr(start, pos - start);
        return !sub_out.empty();
    }
}

bool ExtractInt64(const std::string& obj, const std::string& key, int64_t& out) {
    std::string sub;
    if (!ExtractTopLevelField(obj, key, sub)) return false;
    try { out = std::stoll(sub); return true; } catch (...) { return false; }
}

bool ExtractString(const std::string& obj, const std::string& key, std::string& out) {
    std::string sub;
    if (!ExtractTopLevelField(obj, key, sub)) return false;
    if (sub.size() >= 2 && sub.front() == '"' && sub.back() == '"') {
        out = sub.substr(1, sub.size() - 2);
        return true;
    }
    return false;
}

} // namespace

// ===========================================================================
// MatchSession
// ===========================================================================

MatchSession::MatchSession(MultiplayerKernel* owner, std::string match_id, std::string local_user_id)
    : owner_(owner), match_id_(std::move(match_id)), local_user_id_(std::move(local_user_id)) {}

MatchSession::~MatchSession() { Dispose(); }

Subscription MatchSession::Subscribe(int32_t op_code, EnvelopeHandler handler) {
    if (disposed_.load()) return Subscription();
    const uint64_t id = next_id_.fetch_add(1);
    {
        std::lock_guard<std::mutex> lock(mtx_);
        exact_.push_back({op_code, id, std::move(handler)});
    }
    return Subscription([this, id]() {
        std::lock_guard<std::mutex> lock(mtx_);
        for (auto it = exact_.begin(); it != exact_.end(); ++it) {
            if (it->id == id) { exact_.erase(it); return; }
        }
    });
}

Subscription MatchSession::SubscribeRange(int32_t from, int32_t to, EnvelopeHandler handler) {
    if (disposed_.load()) return Subscription();
    const uint64_t id = next_id_.fetch_add(1);
    {
        std::lock_guard<std::mutex> lock(mtx_);
        ranges_.push_back({from, to, id, std::move(handler)});
    }
    return Subscription([this, id]() {
        std::lock_guard<std::mutex> lock(mtx_);
        for (auto it = ranges_.begin(); it != ranges_.end(); ++it) {
            if (it->id == id) { ranges_.erase(it); return; }
        }
    });
}

Subscription MatchSession::OnTransportStateChanged(StateHandler handler) {
    if (disposed_.load()) return Subscription();
    const uint64_t id = next_id_.fetch_add(1);
    {
        std::lock_guard<std::mutex> lock(mtx_);
        state_handlers_.push_back({id, std::move(handler)});
    }
    return Subscription([this, id]() {
        std::lock_guard<std::mutex> lock(mtx_);
        for (auto it = state_handlers_.begin(); it != state_handlers_.end(); ++it) {
            if (it->id == id) { state_handlers_.erase(it); return; }
        }
    });
}

void MatchSession::Send(int32_t op_code, const std::string& payload_json) {
    if (disposed_.load() || !owner_) return;
    auto* rt = owner_->Internal_RtClient();
    if (!rt) return;
    const int64_t seq = local_seq_.fetch_add(1) + 1;
    const std::string env = BuildEnvelopeJson(seq, current_match_time_ms_.load(),
                                              MakeUuidV4(), payload_json);
    // nakama-cpp's sendMatchData takes raw bytes + opCode.
    rt->sendMatchData(match_id_, op_code,
        Nakama::NBytes(env.data(), env.data() + env.size()), {});
}

void MatchSession::Leave() {
    if (disposed_.load() || !owner_) return;
    auto* rt = owner_->Internal_RtClient();
    if (rt) {
        rt->leaveMatch(match_id_, [](){}, [](const Nakama::NRtError&){});
    }
}

void MatchSession::Dispose() {
    bool was = disposed_.exchange(true);
    if (was) return;
    Leave();
    Internal_SetState(TransportState::Disconnected);
    {
        std::lock_guard<std::mutex> lock(mtx_);
        exact_.clear();
        ranges_.clear();
        state_handlers_.clear();
    }
    if (owner_) owner_->Internal_RemoveSession(match_id_);
}

void MatchSession::Internal_Dispatch(const Envelope& env) {
    if (disposed_.load()) return;
    current_match_time_ms_.store(env.header.match_time_ms);
    std::vector<EnvelopeHandler> exact_snapshot;
    std::vector<RangeBinding>    range_snapshot;
    {
        std::lock_guard<std::mutex> lock(mtx_);
        for (const auto& b : exact_) if (b.op == env.header.op_code) exact_snapshot.push_back(b.handler);
        range_snapshot = ranges_;
    }
    for (auto& h : exact_snapshot) {
        if (h) try { h(env); } catch (...) {}
    }
    for (auto& r : range_snapshot) {
        if (env.header.op_code >= r.from && env.header.op_code <= r.to && r.handler) {
            try { r.handler(env); } catch (...) {}
        }
    }
}

void MatchSession::Internal_SetState(TransportState s) {
    state_.store(s);
    std::vector<StateHandler> snapshot;
    {
        std::lock_guard<std::mutex> lock(mtx_);
        for (const auto& b : state_handlers_) snapshot.push_back(b.handler);
    }
    for (auto& h : snapshot) {
        if (h) try { h(s); } catch (...) {}
    }
}

void MatchSession::Internal_HandlePresence(int32_t joined, int32_t left) {
    int32_t cur = active_player_count_.load();
    int32_t next = std::max(0, cur + joined - left);
    active_player_count_.store(next);
}

std::string MatchSession::MakeUuidV4() {
    static thread_local std::mt19937_64 rng{std::random_device{}()};
    uint8_t b[16];
    for (int i = 0; i < 16; ++i) b[i] = static_cast<uint8_t>(rng() & 0xFF);
    b[6] = (b[6] & 0x0F) | 0x40;
    b[8] = (b[8] & 0x3F) | 0x80;
    static const char* HX = "0123456789abcdef";
    std::string s; s.reserve(36);
    for (int i = 0; i < 16; ++i) {
        s.push_back(HX[(b[i] >> 4) & 0xF]);
        s.push_back(HX[b[i] & 0xF]);
        if (i == 3 || i == 5 || i == 7 || i == 9) s.push_back('-');
    }
    return s;
}

std::string MatchSession::BuildEnvelopeJson(int64_t seq, int64_t match_time_ms,
                                            const std::string& uuid,
                                            const std::string& payload_json) {
    std::ostringstream o;
    o << "{\"h\":{\"s\":" << seq
      << ",\"t\":" << match_time_ms
      << ",\"u\":\"" << EscapeJsonString(uuid) << "\""
      << "},\"p\":";
    if (payload_json.empty()) o << "{}";
    else if (LooksLikeJsonObjectOrArray(payload_json)) o << payload_json;
    else o << "\"" << EscapeJsonString(payload_json) << "\"";
    o << "}";
    return o.str();
}

// ===========================================================================
// MultiplayerKernel
// ===========================================================================

MultiplayerKernel::MultiplayerKernel(std::shared_ptr<Nakama::NClientInterface> client,
                                     std::shared_ptr<Nakama::NSessionInterface> session,
                                     std::shared_ptr<Nakama::NRtTransportInterface> transport)
    : client_(std::move(client)),
      session_(std::move(session)),
      rt_transport_(std::move(transport)) {}

MultiplayerKernel::~MultiplayerKernel() { Shutdown(); }

bool MultiplayerKernel::Initialize() {
    if (initialized_.load()) return true;
    if (!client_ || !session_) return false;
#if !defined(WITH_EXTERNAL_WS) && !defined(BUILD_IO_EXTERNAL)
    rt_client_ = rt_transport_
        ? client_->createRtClient(rt_transport_)
        : client_->createRtClient();
#else
    if (!rt_transport_) {
        SetState(TransportState::FailedFatal);
        return false;
    }
    rt_client_ = client_->createRtClient(rt_transport_);
#endif
    if (!rt_client_) {
        SetState(TransportState::FailedFatal);
        return false;
    }
    rt_listener_ = std::make_shared<Nakama::NRtDefaultClientListener>();
    rt_listener_->setConnectCallback([this]() { SetState(TransportState::Connected); });
    rt_listener_->setDisconnectCallback([this](const Nakama::NRtClientDisconnectInfo&) { SetState(TransportState::Disconnected); });
    rt_listener_->setErrorCallback([](const Nakama::NRtError&){ /* fanned through disconnect */ });
    rt_listener_->setMatchDataCallback([this](const Nakama::NMatchData& d){ Internal_OnMatchData(d); });
    rt_listener_->setMatchPresenceCallback([this](const Nakama::NMatchPresenceEvent& e){ Internal_OnMatchPresence(e); });
    rt_client_->setListener(rt_listener_.get());
    SetState(TransportState::Connecting);
    rt_client_->connect(session_, /*createStatus*/ true);
    initialized_.store(true);
    return true;
}

void MultiplayerKernel::Shutdown() {
    if (!initialized_.exchange(false)) return;
    {
        std::lock_guard<std::mutex> lock(sessions_mtx_);
        for (auto& kv : active_sessions_) {
            if (auto sp = kv.second.lock()) sp->Dispose();
        }
        active_sessions_.clear();
    }
    if (rt_client_) {
        rt_client_->disconnect();
        rt_client_.reset();
    }
    rt_listener_.reset();
    SetState(TransportState::Disconnected);
}

void MultiplayerKernel::CreateMatch(const CreateMatchRequest& req, CreateMatchCb cb) {
    if (!initialized_.load() || !client_ || !session_) {
        if (cb) cb(CreateMatchResponse{ "", "", "", 0, false, "not_initialized" });
        return;
    }
    std::ostringstream o;
    o << "{\"template_id\":\"" << EscapeJsonString(req.template_id) << "\","
      <<  "\"game_id\":\""     << EscapeJsonString(req.game_id)     << "\","
      <<  "\"region\":\""      << EscapeJsonString(req.region)      << "\","
      <<  "\"template_init\":";
    if (req.template_init_json.empty() || !LooksLikeJsonObjectOrArray(req.template_init_json)) {
        o << "{}";
    } else {
        o << req.template_init_json;
    }
    o << "}";
    const std::string payload = o.str();

    client_->rpc(session_, "mp_create_match", payload,
        [cb](const Nakama::NRpc& rpc) {
            CreateMatchResponse out;
            const std::string body = rpc.payload;
            ExtractString(body, "match_id",    out.match_id);
            ExtractString(body, "template_id", out.template_id);
            ExtractString(body, "region",      out.region);
            ExtractInt64 (body, "expires_unix_ms", out.expires_unix_ms);
            out.ok = !out.match_id.empty();
            if (cb) cb(out);
        },
        [cb](const Nakama::NError& e) {
            CreateMatchResponse err;
            err.ok = false;
            err.error_message = e.message;
            if (cb) cb(err);
        });
}

void MultiplayerKernel::ListTemplates(RpcCb cb) {
    RpcRaw("mp_list_templates", "{}", std::move(cb));
}

void MultiplayerKernel::ReadMatchResult(const std::string& match_id, RpcCb cb) {
    std::ostringstream o;
    o << "{\"match_id\":\"" << EscapeJsonString(match_id) << "\"}";
    RpcRaw("mp_read_match_result", o.str(), std::move(cb));
}

void MultiplayerKernel::ListAgentPersonas(RpcCb cb) {
    RpcRaw("mp_agent_list_personas", "{}", std::move(cb));
}

void MultiplayerKernel::SpawnAgent(const std::string& request_json, RpcCb cb) {
    RpcRaw("mp_agent_spawn", LooksLikeJsonObjectOrArray(request_json) ? request_json : "{}", std::move(cb));
}

void MultiplayerKernel::DespawnAgent(const std::string& request_json, RpcCb cb) {
    RpcRaw("mp_agent_despawn", LooksLikeJsonObjectOrArray(request_json) ? request_json : "{}", std::move(cb));
}

void MultiplayerKernel::AgentSpeak(const std::string& request_json, RpcCb cb) {
    RpcRaw("mp_agent_speak", LooksLikeJsonObjectOrArray(request_json) ? request_json : "{}", std::move(cb));
}

void MultiplayerKernel::JoinMatch(const std::string& match_id,
                                  std::function<void(std::shared_ptr<MatchSession>)> cb) {
    if (!initialized_.load() || !rt_client_) {
        if (cb) cb(nullptr);
        return;
    }
    auto session = std::make_shared<MatchSession>(this, match_id, session_ ? session_->getUserId() : "");
    {
        std::lock_guard<std::mutex> lock(sessions_mtx_);
        active_sessions_[match_id] = session;
    }
    rt_client_->joinMatch(match_id, {},
        [cb, session](const Nakama::NMatch& m) {
            session->Internal_SetTemplateId(m.label);
            session->Internal_SetState(TransportState::Connected);
            if (cb) cb(session);
        },
        [this, match_id, cb, session](const Nakama::NRtError&) {
            session->Internal_SetState(TransportState::FailedFatal);
            Internal_RemoveSession(match_id);
            if (cb) cb(nullptr);
        });
}

void MultiplayerKernel::CreateAndJoin(const CreateMatchRequest& req,
                                      std::function<void(std::shared_ptr<MatchSession>)> cb) {
    CreateMatch(req, [this, cb](const CreateMatchResponse& r) {
        if (!r.ok) { if (cb) cb(nullptr); return; }
        JoinMatch(r.match_id, cb);
    });
}

Subscription MultiplayerKernel::OnTransportStateChanged(StateHandler handler) {
    const uint64_t id = next_state_id_.fetch_add(1);
    {
        std::lock_guard<std::mutex> lock(state_handlers_mtx_);
        state_handlers_.push_back({id, std::move(handler)});
    }
    return Subscription([this, id]() {
        std::lock_guard<std::mutex> lock(state_handlers_mtx_);
        for (auto it = state_handlers_.begin(); it != state_handlers_.end(); ++it) {
            if (it->id == id) { state_handlers_.erase(it); return; }
        }
    });
}

void MultiplayerKernel::Internal_OnMatchData(const Nakama::NMatchData& data) {
    std::shared_ptr<MatchSession> sess;
    {
        std::lock_guard<std::mutex> lock(sessions_mtx_);
        auto it = active_sessions_.find(data.matchId);
        if (it != active_sessions_.end()) sess = it->second.lock();
    }
    if (!sess) return;
    const std::string body(data.data.begin(), data.data.end());
    Envelope env;
    if (!ParseEnvelope(body, static_cast<int32_t>(data.opCode),
                       data.presence.userId, env)) {
        return;
    }
    sess->Internal_Dispatch(env);
}

void MultiplayerKernel::Internal_OnMatchPresence(const Nakama::NMatchPresenceEvent& evt) {
    std::shared_ptr<MatchSession> sess;
    {
        std::lock_guard<std::mutex> lock(sessions_mtx_);
        auto it = active_sessions_.find(evt.matchId);
        if (it != active_sessions_.end()) sess = it->second.lock();
    }
    if (!sess) return;
    sess->Internal_HandlePresence(static_cast<int32_t>(evt.joins.size()),
                                  static_cast<int32_t>(evt.leaves.size()));
}

void MultiplayerKernel::Internal_RemoveSession(const std::string& match_id) {
    std::lock_guard<std::mutex> lock(sessions_mtx_);
    active_sessions_.erase(match_id);
}

void MultiplayerKernel::SetState(TransportState s) {
    transport_.store(s);
    std::vector<StateHandler> snapshot;
    {
        std::lock_guard<std::mutex> lock(state_handlers_mtx_);
        for (const auto& b : state_handlers_) snapshot.push_back(b.handler);
    }
    for (auto& h : snapshot) {
        if (h) try { h(s); } catch (...) {}
    }
}

void MultiplayerKernel::RpcRaw(const std::string& rpc_id, const std::string& payload_json, RpcCb cb) {
    if (!initialized_.load() || !client_ || !session_) {
        if (cb) cb(RpcResponse{ "", false, "not_initialized" });
        return;
    }
    client_->rpc(session_, rpc_id, payload_json.empty() ? "{}" : payload_json,
        [cb](const Nakama::NRpc& rpc) {
            if (cb) cb(RpcResponse{ rpc.payload, true, "" });
        },
        [cb](const Nakama::NError& e) {
            if (cb) cb(RpcResponse{ "", false, e.message });
        });
}

bool MultiplayerKernel::ParseEnvelope(const std::string& body, int32_t op_code,
                                      const std::string& sender, Envelope& out) {
    out.header.op_code = op_code;
    out.header.sender_user_id = sender;
    out.recv_unix_ms = NowUnixMs();
    std::string h_sub;
    if (ExtractTopLevelField(body, "h", h_sub)) {
        ExtractInt64 (h_sub, "s", out.header.seq);
        ExtractInt64 (h_sub, "t", out.header.match_time_ms);
        ExtractString(h_sub, "u", out.header.uuid);
    }
    ExtractTopLevelField(body, "p", out.payload_json);
    return true;
}

}} // namespace ivx::multiplayer
