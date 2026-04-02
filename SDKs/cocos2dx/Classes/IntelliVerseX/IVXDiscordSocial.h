// Copyright (c) 2026 Intelli-verse-X
// MIT License — see LICENSE in the project root.

#pragma once

#include "IntelliVerseX/IVXTypes.h"
#include "IntelliVerseX/IVXManager.h"
#include <functional>
#include <string>
#include <vector>

namespace IntelliVerseX {

// --- Config ---

struct IVXDiscordConfig {
    int64_t applicationId = 0;
    std::string defaultLobbySecret;
    bool enableVoice = true;
    bool enableOverlay = false;
};

// --- Unified Friend ---

enum class IVXFriendSource { Game, Discord, Both };

struct IVXUnifiedFriend {
    std::string userId;
    std::string displayName;
    std::string avatarUrl;
    IVXFriendSource source = IVXFriendSource::Game;
    bool online = false;
};

// --- Game Invite ---

struct IVXGameInvite {
    std::string inviteId;
    std::string senderId;
    std::string senderName;
    std::string message;
    std::string lobbySecret;
};

// --- Voice Participant ---

struct IVXVoiceParticipant {
    std::string userId;
    std::string displayName;
    bool muted = false;
    bool deafened = false;
    float volume = 1.0f;
};

// --- Lobby Message ---

struct IVXLobbyMessage {
    std::string senderId;
    std::string senderName;
    std::string content;
    int64_t timestamp = 0;
};

// Callback typedefs
using UnifiedFriendsCallback    = std::function<void(const std::vector<IVXUnifiedFriend>&)>;
using GameInviteCallback        = std::function<void(const IVXGameInvite&)>;
using VoiceParticipantsCallback = std::function<void(const std::vector<IVXVoiceParticipant>&)>;
using LobbyMessageCallback      = std::function<void(const IVXLobbyMessage&)>;

class IVXDiscordSocial {
public:
    static IVXDiscordSocial& getInstance();

    // Manager
    void initialize(const IVXDiscordConfig& config,
                    SuccessCallback onSuccess = nullptr,
                    ErrorCallback onError = nullptr);
    void linkAccount(SuccessCallback onSuccess = nullptr,
                     ErrorCallback onError = nullptr);
    void unlinkAccount(SuccessCallback onSuccess = nullptr,
                       ErrorCallback onError = nullptr);
    bool isInitialized() const;

    // Rich Presence
    void setActivity(const std::string& details, const std::string& state,
                     int64_t startTimestamp = 0, int64_t endTimestamp = 0);
    void setParty(const std::string& partyId, int currentSize,
                  int maxSize, const std::string& joinSecret = "");
    void clearPresence();

    // Friends
    void getUnifiedFriends(UnifiedFriendsCallback onSuccess = nullptr,
                           ErrorCallback onError = nullptr);

    // Lobby
    void createOrJoinLobby(const std::string& lobbySecret,
                           SuccessCallback onSuccess = nullptr,
                           ErrorCallback onError = nullptr);
    void leaveLobby(SuccessCallback onSuccess = nullptr,
                    ErrorCallback onError = nullptr);
    void sendLobbyMessage(const std::string& content);

    // Voice
    void joinVoiceCall(const std::string& lobbyId,
                       SuccessCallback onSuccess = nullptr,
                       ErrorCallback onError = nullptr);
    void leaveVoiceCall(SuccessCallback onSuccess = nullptr,
                        ErrorCallback onError = nullptr);
    void setSelfMute(bool mute);
    void setSelfDeafen(bool deafen);
    void setParticipantVolume(const std::string& userId, float volume);
    void getVoiceParticipants(VoiceParticipantsCallback onSuccess = nullptr,
                              ErrorCallback onError = nullptr);

    // Invites
    void sendInvite(const std::string& userId, const std::string& message,
                    SuccessCallback onSuccess = nullptr,
                    ErrorCallback onError = nullptr);
    void acceptInvite(const std::string& inviteId,
                      SuccessCallback onSuccess = nullptr,
                      ErrorCallback onError = nullptr);
    void declineInvite(const std::string& inviteId);

    // Event registration
    void onDiscordReady(std::function<void(bool provisional)> cb);
    void onDiscordError(std::function<void(const std::string&)> cb);
    void onInviteReceived(GameInviteCallback cb);
    void onJoinRequest(std::function<void(const std::string& userId, const std::string& username)> cb);
    void onLobbyMessageReceived(LobbyMessageCallback cb);
    void onVoiceStateUpdate(std::function<void(const std::string& userId, bool speaking)> cb);

private:
    IVXDiscordSocial() = default;
    ~IVXDiscordSocial() = default;
    IVXDiscordSocial(const IVXDiscordSocial&) = delete;
    IVXDiscordSocial& operator=(const IVXDiscordSocial&) = delete;

    IVXDiscordConfig _config;
    bool _initialized = false;
    std::string _activeLobbyId;
    std::string _activeVoiceChannelId;
    bool _selfMuted = false;
    bool _selfDeafened = false;

    std::function<void(bool)> _cbReady;
    std::function<void(const std::string&)> _cbError;
    GameInviteCallback _cbInvite;
    std::function<void(const std::string&, const std::string&)> _cbJoinRequest;
    LobbyMessageCallback _cbLobbyMessage;
    std::function<void(const std::string&, bool)> _cbVoiceState;

    bool ensureInit(ErrorCallback onError = nullptr) const;
    void log(const std::string& message);
};

} // namespace IntelliVerseX
