// Copyright (c) 2026 Intelli-verse-X
// MIT License — see LICENSE in the project root.

#include "IntelliVerseX/IVXDiscordSocial.h"
#include "cocos2d.h"

namespace IntelliVerseX {

IVXDiscordSocial& IVXDiscordSocial::getInstance() {
    static IVXDiscordSocial instance;
    return instance;
}

bool IVXDiscordSocial::ensureInit(ErrorCallback onError) const {
    if (!_initialized) {
        if (onError) onError({-1, "Discord Social SDK not initialized"});
    }
    return _initialized;
}

// ---------------------------------------------------------------------------
// Manager
// ---------------------------------------------------------------------------

void IVXDiscordSocial::initialize(const IVXDiscordConfig& config,
                                   SuccessCallback onSuccess,
                                   ErrorCallback onError) {
    if (_initialized) {
        log("already initialized");
        if (onSuccess) onSuccess();
        return;
    }

    _config = config;
    // Discord Social SDK init goes here — integration point.
    _initialized = true;
    log("initialized with app id " + std::to_string(config.applicationId));
    if (_cbReady) _cbReady(false);
    if (onSuccess) onSuccess();
}

void IVXDiscordSocial::linkAccount(SuccessCallback onSuccess,
                                    ErrorCallback onError) {
    if (!ensureInit(onError)) return;
    log("account linked");
    if (onSuccess) onSuccess();
}

void IVXDiscordSocial::unlinkAccount(SuccessCallback onSuccess,
                                      ErrorCallback onError) {
    if (!ensureInit(onError)) return;
    log("account unlinked");
    if (onSuccess) onSuccess();
}

bool IVXDiscordSocial::isInitialized() const {
    return _initialized;
}

// ---------------------------------------------------------------------------
// Rich Presence
// ---------------------------------------------------------------------------

void IVXDiscordSocial::setActivity(const std::string& details, const std::string& state,
                                    int64_t startTimestamp, int64_t endTimestamp) {
    if (!ensureInit()) return;
    log("setActivity: " + details + " — " + state);
}

void IVXDiscordSocial::setParty(const std::string& partyId, int currentSize,
                                 int maxSize, const std::string& joinSecret) {
    if (!ensureInit()) return;
    log("setParty: " + partyId + " (" + std::to_string(currentSize) + "/" + std::to_string(maxSize) + ")");
}

void IVXDiscordSocial::clearPresence() {
    if (!ensureInit()) return;
    log("presence cleared");
}

// ---------------------------------------------------------------------------
// Friends
// ---------------------------------------------------------------------------

void IVXDiscordSocial::getUnifiedFriends(UnifiedFriendsCallback onSuccess,
                                          ErrorCallback onError) {
    if (!ensureInit(onError)) return;
    // Merge Discord + game friends — integration point.
    std::vector<IVXUnifiedFriend> friends;
    log("getUnifiedFriends: returned " + std::to_string(friends.size()) + " friends");
    if (onSuccess) onSuccess(friends);
}

// ---------------------------------------------------------------------------
// Lobby
// ---------------------------------------------------------------------------

void IVXDiscordSocial::createOrJoinLobby(const std::string& lobbySecret,
                                          SuccessCallback onSuccess,
                                          ErrorCallback onError) {
    if (!ensureInit(onError)) return;
    _activeLobbyId = lobbySecret;
    log("joined lobby: " + lobbySecret);
    if (onSuccess) onSuccess();
}

void IVXDiscordSocial::leaveLobby(SuccessCallback onSuccess,
                                   ErrorCallback onError) {
    if (!ensureInit(onError)) return;
    log("left lobby: " + _activeLobbyId);
    _activeLobbyId.clear();
    if (onSuccess) onSuccess();
}

void IVXDiscordSocial::sendLobbyMessage(const std::string& content) {
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

void IVXDiscordSocial::joinVoiceCall(const std::string& lobbyId,
                                      SuccessCallback onSuccess,
                                      ErrorCallback onError) {
    if (!ensureInit(onError)) return;
    if (!_config.enableVoice) {
        if (onError) onError({-1, "voice is disabled in config"});
        return;
    }
    _activeVoiceChannelId = lobbyId;
    log("joined voice call: " + lobbyId);
    if (onSuccess) onSuccess();
}

void IVXDiscordSocial::leaveVoiceCall(SuccessCallback onSuccess,
                                       ErrorCallback onError) {
    if (!ensureInit(onError)) return;
    log("left voice call: " + _activeVoiceChannelId);
    _activeVoiceChannelId.clear();
    _selfMuted = false;
    _selfDeafened = false;
    if (onSuccess) onSuccess();
}

void IVXDiscordSocial::setSelfMute(bool mute) {
    if (!ensureInit()) return;
    _selfMuted = mute;
    log(std::string("self mute: ") + (mute ? "true" : "false"));
}

void IVXDiscordSocial::setSelfDeafen(bool deafen) {
    if (!ensureInit()) return;
    _selfDeafened = deafen;
    log(std::string("self deafen: ") + (deafen ? "true" : "false"));
}

void IVXDiscordSocial::setParticipantVolume(const std::string& userId, float volume) {
    if (!ensureInit()) return;
    log("setParticipantVolume: " + userId + " -> " + std::to_string(volume));
}

void IVXDiscordSocial::getVoiceParticipants(VoiceParticipantsCallback onSuccess,
                                             ErrorCallback onError) {
    if (!ensureInit(onError)) return;
    std::vector<IVXVoiceParticipant> participants;
    if (onSuccess) onSuccess(participants);
}

// ---------------------------------------------------------------------------
// Invites
// ---------------------------------------------------------------------------

void IVXDiscordSocial::sendInvite(const std::string& userId, const std::string& message,
                                   SuccessCallback onSuccess,
                                   ErrorCallback onError) {
    if (!ensureInit(onError)) return;
    log("invite sent to " + userId + ": " + message);
    if (onSuccess) onSuccess();
}

void IVXDiscordSocial::acceptInvite(const std::string& inviteId,
                                     SuccessCallback onSuccess,
                                     ErrorCallback onError) {
    if (!ensureInit(onError)) return;
    log("invite accepted: " + inviteId);
    if (onSuccess) onSuccess();
}

void IVXDiscordSocial::declineInvite(const std::string& inviteId) {
    if (!ensureInit()) return;
    log("invite declined: " + inviteId);
}

// ---------------------------------------------------------------------------
// Event Registration
// ---------------------------------------------------------------------------

void IVXDiscordSocial::onDiscordReady(std::function<void(bool provisional)> cb) { _cbReady = std::move(cb); }
void IVXDiscordSocial::onDiscordError(std::function<void(const std::string&)> cb) { _cbError = std::move(cb); }
void IVXDiscordSocial::onInviteReceived(GameInviteCallback cb) { _cbInvite = std::move(cb); }
void IVXDiscordSocial::onJoinRequest(std::function<void(const std::string&, const std::string&)> cb) { _cbJoinRequest = std::move(cb); }
void IVXDiscordSocial::onLobbyMessageReceived(LobbyMessageCallback cb) { _cbLobbyMessage = std::move(cb); }
void IVXDiscordSocial::onVoiceStateUpdate(std::function<void(const std::string&, bool)> cb) { _cbVoiceState = std::move(cb); }

// ---------------------------------------------------------------------------
// Internal
// ---------------------------------------------------------------------------

void IVXDiscordSocial::log(const std::string& message) {
    cocos2d::log("[IntelliVerseX:DiscordSocial] %s", message.c_str());
}

} // namespace IntelliVerseX
