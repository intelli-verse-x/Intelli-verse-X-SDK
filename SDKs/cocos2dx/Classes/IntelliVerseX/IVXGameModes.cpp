// Copyright (c) 2026 Intelli-verse-X
// MIT License — see LICENSE in the project root.

#include "IntelliVerseX/IVXGameModes.h"
#include "cocos2d.h"
#include <random>
#include <sstream>
#include <iomanip>
#include <algorithm>
#include <chrono>

namespace IntelliVerseX {

IVXGameModes& IVXGameModes::getInstance() {
    static IVXGameModes instance;
    return instance;
}

void IVXGameModes::selectMode(GameMode mode, int maxPlayers) {
    _currentMode = mode;
    _maxPlayers = (maxPlayers > 0) ? maxPlayers : defaultMaxPlayers(mode);
    _players.clear();
    if (onModeChanged) onModeChanged(mode);
}

PlayerSlot IVXGameModes::addPlayer(const std::string& name, bool isLocal) {
    if (static_cast<int>(_players.size()) >= _maxPlayers) {
        log("Lobby full (max " + std::to_string(_maxPlayers) + ")");
        return {-1, "", false, false};
    }
    PlayerSlot slot;
    slot.index = static_cast<int>(_players.size());
    slot.name = name;
    slot.isLocal = isLocal;
    slot.ready = false;
    _players.push_back(slot);
    if (onPlayerAdded) onPlayerAdded(slot);
    return slot;
}

void IVXGameModes::removePlayer(int slotIndex) {
    if (slotIndex < 0 || slotIndex >= static_cast<int>(_players.size())) return;
    _players.erase(_players.begin() + slotIndex);
    for (int i = 0; i < static_cast<int>(_players.size()); ++i) {
        _players[i].index = i;
    }
    if (onPlayerRemoved) onPlayerRemoved(slotIndex);
}

void IVXGameModes::setPlayerReady(int slotIndex, bool ready) {
    if (slotIndex < 0 || slotIndex >= static_cast<int>(_players.size())) return;
    _players[slotIndex].ready = ready;
    if (onPlayerReady) onPlayerReady(slotIndex, ready);
}

bool IVXGameModes::canStartMatch() const {
    if (_players.empty()) return false;
    bool allReady = std::all_of(_players.begin(), _players.end(),
                                [](const PlayerSlot& p) { return p.ready; });
    if (!allReady) return false;
    if (_currentMode == GameMode::SOLO) return _players.size() == 1;
    return _players.size() >= 2;
}

std::string IVXGameModes::startMatch() {
    if (!canStartMatch()) {
        log("Cannot start match — check canStartMatch()");
        return "";
    }
    _matchId = generateId();
    log("Match started: " + _matchId);
    if (onMatchStarted) onMatchStarted(_matchId);
    return _matchId;
}

MatchResult IVXGameModes::endMatch() {
    MatchResult result;
    result.matchId = _matchId;
    result.mode = _currentMode;
    result.players = _players;
    result.startedAt = 0;
    auto now = std::chrono::system_clock::now();
    result.endedAt = std::chrono::duration_cast<std::chrono::milliseconds>(
                         now.time_since_epoch()).count();
    _matchId.clear();
    log("Match ended");
    if (onMatchEnded) onMatchEnded(result);
    return result;
}

void IVXGameModes::reset() {
    _currentMode = GameMode::SOLO;
    _maxPlayers = 1;
    _players.clear();
    _matchId.clear();
    _searching = false;
}

// ---------------------------------------------------------------------------
// Lobby / Rooms
// ---------------------------------------------------------------------------

void IVXGameModes::createRoom(const std::string& name,
                               const RoomConfig& config,
                               RoomInfoCallback onSuccess,
                               ErrorCallback /*onError*/) {
    RoomInfo room;
    room.roomId = generateId();
    room.name = name;
    room.hostName = _players.empty() ? "Host" : _players[0].name;
    room.playerCount = static_cast<int>(_players.size());
    room.maxPlayers = config.maxPlayers > 0 ? config.maxPlayers : _maxPlayers;
    room.isPrivate = config.isPrivate;
    room.metadata = config.metadata;
    log("Room created: " + room.roomId);
    if (onSuccess) onSuccess(room);
}

void IVXGameModes::joinRoom(const std::string& roomId,
                             const std::string& /*password*/,
                             RoomInfoCallback onSuccess,
                             ErrorCallback /*onError*/) {
    RoomInfo room;
    room.roomId = roomId;
    log("Room joined: " + roomId);
    if (onSuccess) onSuccess(room);
}

void IVXGameModes::listRooms(const RoomFilter& /*filter*/,
                              RoomListCallback onSuccess,
                              ErrorCallback /*onError*/) {
    std::vector<RoomInfo> rooms;
    if (onSuccess) onSuccess(rooms);
}

void IVXGameModes::leaveRoom(SuccessCallback onSuccess,
                              ErrorCallback /*onError*/) {
    log("Room left");
    if (onSuccess) onSuccess();
}

// ---------------------------------------------------------------------------
// Matchmaking
// ---------------------------------------------------------------------------

void IVXGameModes::findMatch(const MatchConfig& config,
                              MatchFoundCallback onSuccess,
                              ErrorCallback /*onError*/) {
    selectMode(config.mode, config.maxPlayers);
    _searching = true;

    std::string foundMatchId = generateId();
    _matchId = foundMatchId;
    _searching = false;
    log("Match found: " + foundMatchId);
    if (onSuccess) onSuccess(foundMatchId, _players);
}

void IVXGameModes::cancelSearch() {
    _searching = false;
    log("Matchmaking cancelled");
    if (onMatchmakingCancelled) onMatchmakingCancelled();
}

// ---------------------------------------------------------------------------
// Internal
// ---------------------------------------------------------------------------

int IVXGameModes::defaultMaxPlayers(GameMode mode) {
    switch (mode) {
        case GameMode::SOLO:              return 1;
        case GameMode::LOCAL_MULTIPLAYER: return 4;
        case GameMode::ONLINE_VERSUS:     return 2;
        case GameMode::ONLINE_COOP:       return 4;
        case GameMode::RANKED:            return 2;
        case GameMode::TURN_BASED:        return 4;
    }
    return 2;
}

std::string IVXGameModes::generateId() {
    std::random_device rd;
    std::mt19937 gen(rd());
    std::uniform_int_distribution<uint32_t> dist(0, 0xFFFFFFFF);

    std::ostringstream oss;
    oss << std::hex << std::setfill('0');
    oss << std::setw(8) << dist(gen) << "-";
    oss << std::setw(4) << (dist(gen) & 0xFFFF) << "-4";
    oss << std::setw(3) << (dist(gen) & 0xFFF) << "-";
    oss << std::setw(4) << ((dist(gen) & 0x3FFF) | 0x8000) << "-";
    oss << std::setw(8) << dist(gen);
    oss << std::setw(4) << (dist(gen) & 0xFFFF);
    return oss.str();
}

void IVXGameModes::log(const std::string& message) {
    cocos2d::log("[IntelliVerseX:GameModes] %s", message.c_str());
}

} // namespace IntelliVerseX
