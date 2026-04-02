// Copyright (c) 2026 Intelli-verse-X
// MIT License — see LICENSE in the project root.

#include "intelliversex/ivx_discord_social.h"
#include <iostream>

namespace ivx {

DiscordSocial& DiscordSocial::instance() {
    static DiscordSocial inst;
    return inst;
}

bool DiscordSocial::ensureInit(ErrorCb err) const {
    if (!_initialized) {
        if (err) err({-1, "Discord Social SDK not initialized"});
    }
    return _initialized;
}

// ---------------------------------------------------------------------------
// Manager
// ---------------------------------------------------------------------------

void DiscordSocial::initialize(const DiscordConfig& config, SuccessCb cb, ErrorCb err) {
    if (_initialized) {
        log("already initialized");
        if (cb) cb();
        return;
    }

    _config = config;
    // Discord Social SDK init goes here — integration point.
    _initialized = true;
    log("initialized with app id " + std::to_string(config.applicationId));
    if (_onReady) _onReady(false);
    if (cb) cb();
}

void DiscordSocial::linkAccount(SuccessCb cb, ErrorCb err) {
    if (!ensureInit(err)) return;
    log("account linked");
    if (cb) cb();
}

void DiscordSocial::unlinkAccount(SuccessCb cb, ErrorCb err) {
    if (!ensureInit(err)) return;
    log("account unlinked");
    if (cb) cb();
}

bool DiscordSocial::isInitialized() const {
    return _initialized;
}

// ---------------------------------------------------------------------------
// Rich Presence
// ---------------------------------------------------------------------------

void DiscordSocial::setActivity(const std::string& details, const std::string& state,
                                 int64_t startTimestamp, int64_t endTimestamp) {
    if (!ensureInit()) return;
    log("setActivity: " + details + " — " + state);
}

void DiscordSocial::setParty(const std::string& partyId, int32_t currentSize,
                              int32_t maxSize, const std::string& joinSecret) {
    if (!ensureInit()) return;
    log("setParty: " + partyId + " (" + std::to_string(currentSize) + "/" + std::to_string(maxSize) + ")");
}

void DiscordSocial::clearPresence() {
    if (!ensureInit()) return;
    log("presence cleared");
}

// ---------------------------------------------------------------------------
// Friends
// ---------------------------------------------------------------------------

void DiscordSocial::getUnifiedFriends(UnifiedFriendsCb cb, ErrorCb err) {
    if (!ensureInit(err)) return;
    // Merge Discord + game friends — integration point.
    std::vector<UnifiedFriend> friends;
    log("getUnifiedFriends: returned " + std::to_string(friends.size()) + " friends");
    if (cb) cb(friends);
}

// ---------------------------------------------------------------------------
// Lobby
// ---------------------------------------------------------------------------

void DiscordSocial::createOrJoinLobby(const std::string& lobbySecret, SuccessCb cb, ErrorCb err) {
    if (!ensureInit(err)) return;
    _activeLobbyId = lobbySecret;
    log("joined lobby: " + lobbySecret);
    if (cb) cb();
}

void DiscordSocial::leaveLobby(SuccessCb cb, ErrorCb err) {
    if (!ensureInit(err)) return;
    log("left lobby: " + _activeLobbyId);
    _activeLobbyId.clear();
    if (cb) cb();
}

void DiscordSocial::sendLobbyMessage(const std::string& content) {
    if (!ensureInit()) return;
    if (_activeLobbyId.empty()) {
        log("sendLobbyMessage: not in a lobby");
        return;
    }
    log("lobby message sent: " + content);
}

// ---------------------------------------------------------------------------
// Voice
// ---------------------------------------------------------------------------

void DiscordSocial::joinVoiceCall(const std::string& lobbyId, SuccessCb cb, ErrorCb err) {
    if (!ensureInit(err)) return;
    if (!_config.enableVoice) {
        if (err) err({-1, "voice is disabled in config"});
        return;
    }
    _activeVoiceChannelId = lobbyId;
    log("joined voice call: " + lobbyId);
    if (cb) cb();
}

void DiscordSocial::leaveVoiceCall(SuccessCb cb, ErrorCb err) {
    if (!ensureInit(err)) return;
    log("left voice call: " + _activeVoiceChannelId);
    _activeVoiceChannelId.clear();
    _selfMuted = false;
    _selfDeafened = false;
    if (cb) cb();
}

void DiscordSocial::setSelfMute(bool mute) {
    if (!ensureInit()) return;
    _selfMuted = mute;
    log(std::string("self mute: ") + (mute ? "true" : "false"));
}

void DiscordSocial::setSelfDeafen(bool deafen) {
    if (!ensureInit()) return;
    _selfDeafened = deafen;
    log(std::string("self deafen: ") + (deafen ? "true" : "false"));
}

void DiscordSocial::setParticipantVolume(const std::string& userId, float volume) {
    if (!ensureInit()) return;
    log("setParticipantVolume: " + userId + " -> " + std::to_string(volume));
}

void DiscordSocial::getVoiceParticipants(VoiceParticipantsCb cb, ErrorCb err) {
    if (!ensureInit(err)) return;
    std::vector<VoiceParticipant> participants;
    if (cb) cb(participants);
}

// ---------------------------------------------------------------------------
// Invites
// ---------------------------------------------------------------------------

void DiscordSocial::sendInvite(const std::string& userId, const std::string& message,
                                SuccessCb cb, ErrorCb err) {
    if (!ensureInit(err)) return;
    log("invite sent to " + userId + ": " + message);
    if (cb) cb();
}

void DiscordSocial::acceptInvite(const std::string& inviteId, SuccessCb cb, ErrorCb err) {
    if (!ensureInit(err)) return;
    log("invite accepted: " + inviteId);
    if (cb) cb();
}

void DiscordSocial::declineInvite(const std::string& inviteId) {
    if (!ensureInit()) return;
    log("invite declined: " + inviteId);
}

// ---------------------------------------------------------------------------
// Event Registration
// ---------------------------------------------------------------------------

void DiscordSocial::onDiscordReady(std::function<void(bool provisional)> cb) { _onReady = std::move(cb); }
void DiscordSocial::onDiscordError(StringCb cb) { _onError = std::move(cb); }
void DiscordSocial::onInviteReceived(GameInviteCb cb) { _onInvite = std::move(cb); }
void DiscordSocial::onJoinRequest(std::function<void(const std::string&, const std::string&)> cb) { _onJoinRequest = std::move(cb); }
void DiscordSocial::onLobbyMessage(LobbyMessageCb cb) { _onLobbyMessage = std::move(cb); }
void DiscordSocial::onVoiceStateUpdate(std::function<void(const std::string&, bool)> cb) { _onVoiceState = std::move(cb); }

// ---------------------------------------------------------------------------
// Logging
// ---------------------------------------------------------------------------

void DiscordSocial::log(const std::string& msg) {
    std::cout << "[IVX:DiscordSocial] " << msg << std::endl;
}

} // namespace ivx
