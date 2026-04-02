// Copyright (c) 2026 Intelli-verse-X
// MIT License — see LICENSE in the project root.

#pragma once

#include "IntelliVerseX/IVXTypes.h"
#include <functional>
#include <string>
#include <vector>

namespace IntelliVerseX {

enum class GameMode {
    SOLO,
    LOCAL_MULTIPLAYER,
    ONLINE_VERSUS,
    ONLINE_COOP,
    RANKED,
    TURN_BASED,
};

struct PlayerSlot {
    int index = 0;
    std::string name;
    bool isLocal = true;
    bool ready = false;
};

struct MatchConfig {
    GameMode mode = GameMode::SOLO;
    int maxPlayers = 1;
    std::string metadata;
};

struct RoomConfig {
    int maxPlayers = 4;
    bool isPrivate = false;
    std::string password;
    std::string metadata;
};

struct RoomInfo {
    std::string roomId;
    std::string name;
    std::string hostName;
    int playerCount = 0;
    int maxPlayers = 0;
    bool isPrivate = false;
    std::string metadata;
};

struct MatchResult {
    std::string matchId;
    GameMode mode = GameMode::SOLO;
    std::vector<PlayerSlot> players;
    int64_t startedAt = 0;
    int64_t endedAt = 0;
    std::string metadata;
};

struct RoomFilter {
    GameMode mode = GameMode::SOLO;
    bool hasSlots = false;
    std::string query;
};

using PlayerSlotCallback = std::function<void(const PlayerSlot&)>;
using MatchResultCallback = std::function<void(const MatchResult&)>;
using RoomInfoCallback = std::function<void(const RoomInfo&)>;
using RoomListCallback = std::function<void(const std::vector<RoomInfo>&)>;
using MatchFoundCallback = std::function<void(const std::string& matchId, const std::vector<PlayerSlot>&)>;

class IVXGameModes {
public:
    static IVXGameModes& getInstance();

    // Mode & Players
    void selectMode(GameMode mode, int maxPlayers = -1);
    PlayerSlot addPlayer(const std::string& name, bool isLocal = true);
    void removePlayer(int slotIndex);
    void setPlayerReady(int slotIndex, bool ready);
    std::string startMatch();
    MatchResult endMatch();
    void reset();

    // Lobby / Rooms (via HttpClient)
    void createRoom(const std::string& name,
                    const RoomConfig& config = {},
                    RoomInfoCallback onSuccess = nullptr,
                    ErrorCallback onError = nullptr);
    void joinRoom(const std::string& roomId,
                  const std::string& password = "",
                  RoomInfoCallback onSuccess = nullptr,
                  ErrorCallback onError = nullptr);
    void listRooms(const RoomFilter& filter = {},
                   RoomListCallback onSuccess = nullptr,
                   ErrorCallback onError = nullptr);
    void leaveRoom(SuccessCallback onSuccess = nullptr,
                   ErrorCallback onError = nullptr);

    // Matchmaking
    void findMatch(const MatchConfig& config = {},
                   MatchFoundCallback onSuccess = nullptr,
                   ErrorCallback onError = nullptr);
    void cancelSearch();

    // Getters
    GameMode currentMode() const { return _currentMode; }
    int maxPlayers() const { return _maxPlayers; }
    const std::vector<PlayerSlot>& players() const { return _players; }
    const std::string& matchId() const { return _matchId; }
    bool isSearching() const { return _searching; }
    bool canStartMatch() const;

    // Callbacks for local events
    std::function<void(GameMode)> onModeChanged;
    std::function<void(const PlayerSlot&)> onPlayerAdded;
    std::function<void(int)> onPlayerRemoved;
    std::function<void(int, bool)> onPlayerReady;
    std::function<void(const std::string&)> onMatchStarted;
    std::function<void(const MatchResult&)> onMatchEnded;
    std::function<void()> onMatchmakingCancelled;

private:
    IVXGameModes() = default;
    ~IVXGameModes() = default;
    IVXGameModes(const IVXGameModes&) = delete;
    IVXGameModes& operator=(const IVXGameModes&) = delete;

    GameMode _currentMode = GameMode::SOLO;
    int _maxPlayers = 1;
    std::vector<PlayerSlot> _players;
    std::string _matchId;
    bool _searching = false;

    static int defaultMaxPlayers(GameMode mode);
    static std::string generateId();
    void log(const std::string& message);
};

} // namespace IntelliVerseX
