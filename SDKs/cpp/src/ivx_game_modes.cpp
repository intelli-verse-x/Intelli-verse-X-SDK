// Copyright (c) 2026 Intelli-verse-X
// MIT License — see LICENSE in the project root.

#include "intelliversex/ivx_game_modes.h"
#include <algorithm>
#include <iostream>
#include <sstream>

namespace ivx {

GameModes& GameModes::instance() {
    static GameModes inst;
    return inst;
}

// --- Mode selection ---

void GameModes::selectMode(GameMode mode) {
    if (_matchActive) {
        if (onError) onError({-1, "Cannot change mode while match is in progress"});
        return;
    }
    _mode = mode;
    log("mode changed to " + std::to_string(static_cast<int>(mode)));
    if (onModeChanged) onModeChanged(mode);
}

// --- Player management ---

void GameModes::addPlayer(const PlayerSlot& player) {
    for (const auto& p : _players) {
        if (p.playerId == player.playerId) return;
    }
    _players.push_back(player);
    log("player added: " + player.displayName);
    if (onPlayerAdded) onPlayerAdded(player);
}

void GameModes::removePlayer(const std::string& playerId) {
    auto it = std::remove_if(_players.begin(), _players.end(),
        [&](const PlayerSlot& s) { return s.playerId == playerId; });
    if (it != _players.end()) {
        _players.erase(it, _players.end());
        if (onPlayerRemoved) onPlayerRemoved(playerId);
    }
}

void GameModes::setPlayerReady(const std::string& playerId, bool ready) {
    for (auto& p : _players) {
        if (p.playerId == playerId) {
            p.ready = ready;
            return;
        }
    }
}

bool GameModes::allPlayersReady() const {
    if (_players.empty()) return false;
    return std::all_of(_players.begin(), _players.end(),
        [](const PlayerSlot& s) { return s.ready; });
}

// --- Match lifecycle ---

void GameModes::startMatch(const MatchConfig& config) {
    if (_matchActive) {
        if (onError) onError({-1, "Match already in progress"});
        return;
    }
    _mode = config.mode;
    _matchActive = true;

    std::ostringstream ss;
    ss << "match-" << reinterpret_cast<uintptr_t>(this) << "-"
       << static_cast<int>(config.mode);
    _currentMatchId = ss.str();

    log("match started: " + _currentMatchId);
    if (onMatchStarted) onMatchStarted(_currentMatchId);
}

void GameModes::endMatch(const MatchResult& result) {
    if (!_matchActive) return;
    _matchActive = false;
    log("match ended: " + _currentMatchId + " winner=" + result.winnerId);
    if (onMatchEnded) onMatchEnded(result);
    _currentMatchId.clear();
}

void GameModes::reset() {
    _players.clear();
    _matchActive = false;
    _searching = false;
    _currentMatchId.clear();
    _currentRoomId.clear();
    _mode = GameMode::Solo;
    log("reset");
}

// --- Lobby ---

void GameModes::createRoom(const std::string& roomName, const MatchConfig& config,
                           RoomCb cb, ErrorCb err) {
    std::ostringstream ss;
    ss << "room-" << reinterpret_cast<uintptr_t>(this);
    _currentRoomId = ss.str();

    RoomInfo info;
    info.roomId = _currentRoomId;
    info.roomName = roomName;
    info.maxPlayers = config.maxPlayers;
    info.mode = config.mode;
    info.playerCount = 1;

    log("room created: " + roomName + " (" + _currentRoomId + ")");
    if (cb) cb(info);
    if (onMatchFound) onMatchFound(info);
}

void GameModes::joinRoom(const std::string& roomId, SuccessCb cb, ErrorCb err) {
    _currentRoomId = roomId;
    log("joined room: " + roomId);
    if (cb) cb();
}

void GameModes::listRooms(RoomListCb cb, ErrorCb err) {
    log("requesting room list");
    if (cb) {
        std::vector<RoomInfo> empty;
        cb(empty);
    }
}

void GameModes::leaveRoom(SuccessCb cb) {
    log("left room: " + _currentRoomId);
    _currentRoomId.clear();
    if (cb) cb();
}

// --- Matchmaking ---

void GameModes::findMatch(const MatchConfig& config, RoomCb cb, ErrorCb err) {
    if (_searching) {
        if (onError) onError({-1, "Already searching for a match"});
        return;
    }
    _searching = true;
    log("matchmaking search started");
}

void GameModes::cancelSearch() {
    _searching = false;
    log("matchmaking search cancelled");
}

// --- Logging ---

void GameModes::log(const std::string& msg) {
    std::cout << "[IVX:GameModes] " << msg << std::endl;
}

} // namespace ivx
