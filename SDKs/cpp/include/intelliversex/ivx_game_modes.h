// Copyright (c) 2026 Intelli-verse-X
// MIT License — see LICENSE in the project root.

#pragma once

#include "ivx_types.h"
#include <cstdint>
#include <functional>
#include <string>
#include <unordered_map>
#include <vector>

namespace ivx {

enum class GameMode : uint8_t {
    Solo,
    LocalMultiplayer,
    OnlineVersus,
    OnlineCoop,
    Ranked,
    TurnBased
};

struct PlayerSlot {
    std::string playerId;
    std::string displayName;
    int32_t slotIndex = 0;
    bool ready = false;
    bool isLocal = true;
};

struct MatchConfig {
    GameMode mode = GameMode::Solo;
    int32_t maxPlayers = 4;
    int32_t roundCount = 1;
    float timeLimitSeconds = 0.0f;
    std::unordered_map<std::string, std::string> customProperties;
};

struct RoomInfo {
    std::string roomId;
    std::string roomName;
    std::string hostId;
    int32_t playerCount = 0;
    int32_t maxPlayers = 0;
    GameMode mode = GameMode::Solo;
    bool inProgress = false;
};

struct MatchResult {
    std::string matchId;
    std::string winnerId;
    std::unordered_map<std::string, int32_t> playerScores;
    float durationSeconds = 0.0f;
};

using ModeCb       = std::function<void(GameMode)>;
using PlayerCb     = std::function<void(const PlayerSlot&)>;
using RoomCb       = std::function<void(const RoomInfo&)>;
using RoomListCb   = std::function<void(const std::vector<RoomInfo>&)>;
using MatchResultCb = std::function<void(const MatchResult&)>;
using MatchIdCb    = std::function<void(const std::string&)>;

/// Game mode selection, lobby, and matchmaking manager.
///
/// Thread-safety: same as Manager — single-thread only.
class GameModes {
public:
    static GameModes& instance();

    // Mode selection
    void selectMode(GameMode mode);
    GameMode currentMode() const { return _mode; }

    // Player management
    void addPlayer(const PlayerSlot& player);
    void removePlayer(const std::string& playerId);
    void setPlayerReady(const std::string& playerId, bool ready);
    const std::vector<PlayerSlot>& players() const { return _players; }
    bool allPlayersReady() const;

    // Match lifecycle
    void startMatch(const MatchConfig& config);
    void endMatch(const MatchResult& result);
    bool matchInProgress() const { return _matchActive; }
    void reset();

    // Lobby
    void createRoom(const std::string& roomName, const MatchConfig& config,
                    RoomCb cb = nullptr, ErrorCb err = nullptr);
    void joinRoom(const std::string& roomId, SuccessCb cb = nullptr, ErrorCb err = nullptr);
    void listRooms(RoomListCb cb = nullptr, ErrorCb err = nullptr);
    void leaveRoom(SuccessCb cb = nullptr);

    // Matchmaking
    void findMatch(const MatchConfig& config, RoomCb cb = nullptr, ErrorCb err = nullptr);
    void cancelSearch();

    // Event callbacks
    ModeCb onModeChanged;
    PlayerCb onPlayerAdded;
    std::function<void(const std::string&)> onPlayerRemoved;
    RoomCb onMatchFound;
    RoomListCb onRoomListUpdated;
    MatchIdCb onMatchStarted;
    MatchResultCb onMatchEnded;
    ErrorCb onError;

private:
    GameModes() = default;
    GameMode _mode = GameMode::Solo;
    std::vector<PlayerSlot> _players;
    bool _matchActive = false;
    bool _searching = false;
    std::string _currentMatchId;
    std::string _currentRoomId;

    void log(const std::string& msg);
};

} // namespace ivx
