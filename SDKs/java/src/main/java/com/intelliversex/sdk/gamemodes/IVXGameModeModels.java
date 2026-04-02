// Copyright (c) 2026 Intelli-verse-X
// MIT License — see LICENSE in the project root.

package com.intelliversex.sdk.gamemodes;

import com.google.gson.annotations.SerializedName;

import java.util.Collections;
import java.util.List;
import java.util.Map;
import java.util.Objects;

/**
 * Data models for the IntelliVerseX game-modes and lobby modules.
 */
public final class IVXGameModeModels {

    private IVXGameModeModels() {}

    /**
     * Represents a single player slot within a match lobby.
     */
    public static final class IVXPlayerSlot {
        private final int slotIndex;
        private final String playerName;
        private final boolean local;
        private volatile boolean ready;

        public IVXPlayerSlot(int slotIndex, String playerName, boolean local) {
            this.slotIndex = slotIndex;
            this.playerName = playerName != null ? playerName : "";
            this.local = local;
            this.ready = false;
        }

        public int getSlotIndex() { return slotIndex; }
        public String getPlayerName() { return playerName; }
        public boolean isLocal() { return local; }
        public boolean isReady() { return ready; }
        void setReady(boolean ready) { this.ready = ready; }

        @Override
        public boolean equals(Object o) {
            if (this == o) return true;
            if (!(o instanceof IVXPlayerSlot)) return false;
            IVXPlayerSlot that = (IVXPlayerSlot) o;
            return slotIndex == that.slotIndex && Objects.equals(playerName, that.playerName);
        }

        @Override
        public int hashCode() {
            return Objects.hash(slotIndex, playerName);
        }

        @Override
        public String toString() {
            return "IVXPlayerSlot{slot=" + slotIndex + ", name='" + playerName
                    + "', local=" + local + ", ready=" + ready + '}';
        }
    }

    /**
     * Configuration for creating or joining a match.
     */
    public static final class IVXMatchConfig {
        @SerializedName("game_mode")
        private final String gameMode;

        @SerializedName("max_players")
        private final int maxPlayers;

        @SerializedName("is_ranked")
        private final boolean ranked;

        @SerializedName("metadata")
        private final Map<String, String> metadata;

        public IVXMatchConfig(String gameMode, int maxPlayers, boolean ranked, Map<String, String> metadata) {
            this.gameMode = gameMode != null ? gameMode : "";
            this.maxPlayers = maxPlayers;
            this.ranked = ranked;
            this.metadata = metadata != null ? Collections.unmodifiableMap(metadata) : Collections.emptyMap();
        }

        public String getGameMode() { return gameMode; }
        public int getMaxPlayers() { return maxPlayers; }
        public boolean isRanked() { return ranked; }
        public Map<String, String> getMetadata() { return metadata; }

        @Override
        public boolean equals(Object o) {
            if (this == o) return true;
            if (!(o instanceof IVXMatchConfig)) return false;
            IVXMatchConfig that = (IVXMatchConfig) o;
            return maxPlayers == that.maxPlayers
                    && ranked == that.ranked
                    && Objects.equals(gameMode, that.gameMode);
        }

        @Override
        public int hashCode() {
            return Objects.hash(gameMode, maxPlayers, ranked);
        }

        @Override
        public String toString() {
            return "IVXMatchConfig{mode='" + gameMode + "', maxPlayers=" + maxPlayers
                    + ", ranked=" + ranked + '}';
        }
    }

    /**
     * Information about a lobby room visible to players browsing for matches.
     */
    public static final class IVXRoomInfo {
        @SerializedName("room_id")
        private final String roomId;

        @SerializedName("room_name")
        private final String roomName;

        @SerializedName("host_name")
        private final String hostName;

        @SerializedName("player_count")
        private final int playerCount;

        @SerializedName("max_players")
        private final int maxPlayers;

        @SerializedName("game_mode")
        private final String gameMode;

        @SerializedName("is_locked")
        private final boolean locked;

        public IVXRoomInfo(String roomId, String roomName, String hostName,
                           int playerCount, int maxPlayers, String gameMode, boolean locked) {
            this.roomId = roomId != null ? roomId : "";
            this.roomName = roomName != null ? roomName : "";
            this.hostName = hostName != null ? hostName : "";
            this.playerCount = playerCount;
            this.maxPlayers = maxPlayers;
            this.gameMode = gameMode != null ? gameMode : "";
            this.locked = locked;
        }

        public String getRoomId() { return roomId; }
        public String getRoomName() { return roomName; }
        public String getHostName() { return hostName; }
        public int getPlayerCount() { return playerCount; }
        public int getMaxPlayers() { return maxPlayers; }
        public String getGameMode() { return gameMode; }
        public boolean isLocked() { return locked; }

        /** Returns {@code true} if the room has open slots. */
        public boolean hasSpace() { return playerCount < maxPlayers; }

        @Override
        public boolean equals(Object o) {
            if (this == o) return true;
            if (!(o instanceof IVXRoomInfo)) return false;
            IVXRoomInfo that = (IVXRoomInfo) o;
            return Objects.equals(roomId, that.roomId);
        }

        @Override
        public int hashCode() {
            return Objects.hash(roomId);
        }

        @Override
        public String toString() {
            return "IVXRoomInfo{roomId='" + roomId + "', name='" + roomName
                    + "', players=" + playerCount + '/' + maxPlayers
                    + ", mode='" + gameMode + "', locked=" + locked + '}';
        }
    }

    /**
     * Result summary for a completed match.
     */
    public static final class IVXMatchResult {
        @SerializedName("match_id")
        private final String matchId;

        @SerializedName("winner_slot")
        private final int winnerSlot;

        @SerializedName("scores")
        private final Map<Integer, Integer> scores;

        @SerializedName("duration_ms")
        private final long durationMs;

        @SerializedName("completed")
        private final boolean completed;

        public IVXMatchResult(String matchId, int winnerSlot, Map<Integer, Integer> scores,
                              long durationMs, boolean completed) {
            this.matchId = matchId != null ? matchId : "";
            this.winnerSlot = winnerSlot;
            this.scores = scores != null ? Collections.unmodifiableMap(scores) : Collections.emptyMap();
            this.durationMs = durationMs;
            this.completed = completed;
        }

        public String getMatchId() { return matchId; }
        public int getWinnerSlot() { return winnerSlot; }
        public Map<Integer, Integer> getScores() { return scores; }
        public long getDurationMs() { return durationMs; }
        public boolean isCompleted() { return completed; }

        @Override
        public boolean equals(Object o) {
            if (this == o) return true;
            if (!(o instanceof IVXMatchResult)) return false;
            IVXMatchResult that = (IVXMatchResult) o;
            return Objects.equals(matchId, that.matchId);
        }

        @Override
        public int hashCode() {
            return Objects.hash(matchId);
        }

        @Override
        public String toString() {
            return "IVXMatchResult{matchId='" + matchId + "', winner=" + winnerSlot
                    + ", duration=" + durationMs + "ms, completed=" + completed + '}';
        }
    }
}
