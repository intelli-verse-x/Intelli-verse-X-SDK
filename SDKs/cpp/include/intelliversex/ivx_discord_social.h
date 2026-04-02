// Copyright (c) 2026 Intelli-verse-X
// MIT License — see LICENSE in the project root.

#pragma once

#include "ivx_types.h"
#include <cstdint>
#include <functional>
#include <string>
#include <vector>

namespace ivx {

// --- Config ---

struct DiscordConfig {
    int64_t applicationId = 0;
    std::string defaultLobbySecret;
    bool enableVoice = true;
    bool enableOverlay = false;
};

// --- Unified Friend ---

enum class FriendSource { Game, Discord, Both };

struct UnifiedFriend {
    std::string userId;
    std::string displayName;
    std::string avatarUrl;
    FriendSource source = FriendSource::Game;
    bool online = false;
};

// --- Game Invite ---

struct GameInvite {
    std::string inviteId;
    std::string senderId;
    std::string senderName;
    std::string message;
    std::string lobbySecret;
};

// --- Voice Participant ---

struct VoiceParticipant {
    std::string userId;
    std::string displayName;
    bool muted = false;
    bool deafened = false;
    float volume = 1.0f;
};

// --- Lobby Message ---

struct LobbyMessage {
    std::string senderId;
    std::string senderName;
    std::string content;
    int64_t timestamp = 0;
};

// Callback typedefs
using UnifiedFriendsCb    = std::function<void(const std::vector<UnifiedFriend>&)>;
using GameInviteCb        = std::function<void(const GameInvite&)>;
using VoiceParticipantsCb = std::function<void(const std::vector<VoiceParticipant>&)>;
using LobbyMessageCb      = std::function<void(const LobbyMessage&)>;

/// Wraps Discord Social SDK features: Rich Presence, unified friends,
/// lobby chat, voice calls, and game invites.
///
/// Thread-safety: same as Manager — single-thread only.
class DiscordSocial {
public:
    static DiscordSocial& instance();

    // Manager
    void initialize(const DiscordConfig& config, SuccessCb cb = nullptr, ErrorCb err = nullptr);
    void linkAccount(SuccessCb cb = nullptr, ErrorCb err = nullptr);
    void unlinkAccount(SuccessCb cb = nullptr, ErrorCb err = nullptr);
    bool isInitialized() const;

    // Rich Presence
    void setActivity(const std::string& details, const std::string& state,
                     int64_t startTimestamp = 0, int64_t endTimestamp = 0);
    void setParty(const std::string& partyId, int32_t currentSize,
                  int32_t maxSize, const std::string& joinSecret = "");
    void clearPresence();

    // Friends
    void getUnifiedFriends(UnifiedFriendsCb cb, ErrorCb err = nullptr);

    // Lobby
    void createOrJoinLobby(const std::string& lobbySecret, SuccessCb cb = nullptr, ErrorCb err = nullptr);
    void leaveLobby(SuccessCb cb = nullptr, ErrorCb err = nullptr);
    void sendLobbyMessage(const std::string& content);

    // Voice
    void joinVoiceCall(const std::string& lobbyId, SuccessCb cb = nullptr, ErrorCb err = nullptr);
    void leaveVoiceCall(SuccessCb cb = nullptr, ErrorCb err = nullptr);
    void setSelfMute(bool mute);
    void setSelfDeafen(bool deafen);
    void setParticipantVolume(const std::string& userId, float volume);
    void getVoiceParticipants(VoiceParticipantsCb cb, ErrorCb err = nullptr);

    // Invites
    void sendInvite(const std::string& userId, const std::string& message,
                    SuccessCb cb = nullptr, ErrorCb err = nullptr);
    void acceptInvite(const std::string& inviteId, SuccessCb cb = nullptr, ErrorCb err = nullptr);
    void declineInvite(const std::string& inviteId);

    // Event registration
    void onDiscordReady(std::function<void(bool provisional)> cb);
    void onDiscordError(StringCb cb);
    void onInviteReceived(GameInviteCb cb);
    void onJoinRequest(std::function<void(const std::string& userId, const std::string& username)> cb);
    void onLobbyMessage(LobbyMessageCb cb);
    void onVoiceStateUpdate(std::function<void(const std::string& userId, bool speaking)> cb);

private:
    DiscordSocial() = default;

    DiscordConfig _config;
    bool _initialized = false;
    std::string _activeLobbyId;
    std::string _activeVoiceChannelId;
    bool _selfMuted = false;
    bool _selfDeafened = false;

    std::function<void(bool)> _onReady;
    StringCb _onError;
    GameInviteCb _onInvite;
    std::function<void(const std::string&, const std::string&)> _onJoinRequest;
    LobbyMessageCb _onLobbyMessage;
    std::function<void(const std::string&, bool)> _onVoiceState;

    bool ensureInit(ErrorCb err = nullptr) const;
    void log(const std::string& msg);
};

} // namespace ivx
