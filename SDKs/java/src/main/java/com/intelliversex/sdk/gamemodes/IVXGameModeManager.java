// Copyright (c) 2026 Intelli-verse-X
// MIT License — see LICENSE in the project root.

package com.intelliversex.sdk.gamemodes;

import com.intelliversex.sdk.gamemodes.IVXGameModeModels.IVXMatchResult;
import com.intelliversex.sdk.gamemodes.IVXGameModeModels.IVXPlayerSlot;

import java.util.ArrayList;
import java.util.Collections;
import java.util.List;
import java.util.concurrent.CopyOnWriteArrayList;
import java.util.function.Consumer;

/**
 * Thread-safe singleton that manages game-mode selection, player slots,
 * readiness checks, and match lifecycle.
 * <p>
 * Register callbacks via the {@code onXxx} methods to react to state changes.
 * <p>
 * <b>Usage:</b>
 * <pre>{@code
 * IVXGameModeManager mgr = IVXGameModeManager.getInstance();
 * mgr.selectMode(IVXGameMode.ONLINE_VERSUS, 4);
 * mgr.addPlayer("Alice", true);
 * mgr.setPlayerReady(0, true);
 * if (mgr.canStartMatch()) mgr.startMatch();
 * }</pre>
 */
public class IVXGameModeManager {

    /**
     * Supported game modes.
     */
    public enum IVXGameMode {
        SOLO,
        LOCAL_MULTIPLAYER,
        ONLINE_VERSUS,
        ONLINE_COOP,
        RANKED,
        TURN_BASED
    }

    private static volatile IVXGameModeManager instance;

    private volatile IVXGameMode currentMode;
    private volatile int maxPlayers;
    private volatile boolean matchActive;
    private final List<IVXPlayerSlot> players = new CopyOnWriteArrayList<>();

    private final List<Consumer<IVXGameMode>> modeChangedListeners = new CopyOnWriteArrayList<>();
    private final List<Consumer<IVXPlayerSlot>> playerAddedListeners = new CopyOnWriteArrayList<>();
    private final List<Consumer<Integer>> playerRemovedListeners = new CopyOnWriteArrayList<>();
    private final List<Consumer<IVXPlayerSlot>> playerReadyListeners = new CopyOnWriteArrayList<>();
    private final List<Consumer<Boolean>> matchStateListeners = new CopyOnWriteArrayList<>();

    private IVXGameModeManager() {}

    /**
     * Returns the singleton instance, creating it on first access.
     *
     * @return the shared {@link IVXGameModeManager} instance
     */
    public static IVXGameModeManager getInstance() {
        if (instance == null) {
            synchronized (IVXGameModeManager.class) {
                if (instance == null) {
                    instance = new IVXGameModeManager();
                }
            }
        }
        return instance;
    }

    // ──────────────────────────────────────────────
    //  Mode Selection
    // ──────────────────────────────────────────────

    /**
     * Selects a game mode and sets the maximum number of players.
     * Resets any existing player slots.
     *
     * @param mode       the game mode to use
     * @param maxPlayers maximum players allowed (must be &ge; 1)
     * @throws IllegalArgumentException if maxPlayers &lt; 1
     */
    public synchronized void selectMode(IVXGameMode mode, int maxPlayers) {
        if (mode == null) {
            throw new IllegalArgumentException("mode must not be null");
        }
        if (maxPlayers < 1) {
            throw new IllegalArgumentException("maxPlayers must be >= 1, got: " + maxPlayers);
        }
        this.currentMode = mode;
        this.maxPlayers = maxPlayers;
        this.matchActive = false;
        this.players.clear();
        modeChangedListeners.forEach(l -> l.accept(mode));
    }

    /**
     * Returns the currently selected game mode, or {@code null} if none selected.
     *
     * @return the current {@link IVXGameMode}
     */
    public IVXGameMode getCurrentMode() { return currentMode; }

    /**
     * Returns the maximum number of players for the current mode.
     *
     * @return max player count
     */
    public int getMaxPlayers() { return maxPlayers; }

    // ──────────────────────────────────────────────
    //  Player Management
    // ──────────────────────────────────────────────

    /**
     * Adds a player to the next available slot.
     *
     * @param name    display name for the player
     * @param isLocal {@code true} if this is a local player
     * @return the newly created {@link IVXPlayerSlot}
     * @throws IllegalStateException if the lobby is full or no mode is selected
     */
    public synchronized IVXPlayerSlot addPlayer(String name, boolean isLocal) {
        requireModeSelected();
        if (players.size() >= maxPlayers) {
            throw new IllegalStateException("Lobby is full (" + maxPlayers + " players max)");
        }
        IVXPlayerSlot slot = new IVXPlayerSlot(players.size(), name, isLocal);
        players.add(slot);
        playerAddedListeners.forEach(l -> l.accept(slot));
        return slot;
    }

    /**
     * Removes the player at the given slot index.
     *
     * @param slotIndex the slot to remove
     * @throws IndexOutOfBoundsException if the index is invalid
     */
    public synchronized void removePlayer(int slotIndex) {
        requireModeSelected();
        if (slotIndex < 0 || slotIndex >= players.size()) {
            throw new IndexOutOfBoundsException("Invalid slot index: " + slotIndex);
        }
        players.remove(slotIndex);
        playerRemovedListeners.forEach(l -> l.accept(slotIndex));
    }

    /**
     * Sets the ready state for a player slot.
     *
     * @param slot  the slot index
     * @param ready {@code true} to mark ready
     * @throws IndexOutOfBoundsException if the index is invalid
     */
    public synchronized void setPlayerReady(int slot, boolean ready) {
        requireModeSelected();
        if (slot < 0 || slot >= players.size()) {
            throw new IndexOutOfBoundsException("Invalid slot index: " + slot);
        }
        IVXPlayerSlot ps = players.get(slot);
        ps.setReady(ready);
        playerReadyListeners.forEach(l -> l.accept(ps));
    }

    /**
     * Returns an unmodifiable snapshot of the current player list.
     *
     * @return list of {@link IVXPlayerSlot}
     */
    public List<IVXPlayerSlot> getPlayers() {
        return Collections.unmodifiableList(new ArrayList<>(players));
    }

    // ──────────────────────────────────────────────
    //  Match Lifecycle
    // ──────────────────────────────────────────────

    /**
     * Returns {@code true} if the match can start: a mode is selected,
     * at least one player is present, and all players are ready.
     *
     * @return whether the match is ready to begin
     */
    public boolean canStartMatch() {
        if (currentMode == null || players.isEmpty()) return false;
        for (IVXPlayerSlot p : players) {
            if (!p.isReady()) return false;
        }
        return true;
    }

    /**
     * Starts the match. Requires {@link #canStartMatch()} to return {@code true}.
     *
     * @throws IllegalStateException if preconditions are not met
     */
    public synchronized void startMatch() {
        if (!canStartMatch()) {
            throw new IllegalStateException("Cannot start match — check canStartMatch()");
        }
        this.matchActive = true;
        matchStateListeners.forEach(l -> l.accept(true));
    }

    /**
     * Ends the current match.
     *
     * @throws IllegalStateException if no match is active
     */
    public synchronized void endMatch() {
        if (!matchActive) {
            throw new IllegalStateException("No active match to end");
        }
        this.matchActive = false;
        matchStateListeners.forEach(l -> l.accept(false));
    }

    /**
     * Returns {@code true} if a match is currently in progress.
     *
     * @return match active state
     */
    public boolean isMatchActive() { return matchActive; }

    /**
     * Resets all state: clears mode, players, and match flag.
     */
    public synchronized void reset() {
        this.currentMode = null;
        this.maxPlayers = 0;
        this.matchActive = false;
        this.players.clear();
    }

    // ──────────────────────────────────────────────
    //  Event Callbacks
    // ──────────────────────────────────────────────

    /**
     * Registers a callback fired when the game mode changes.
     *
     * @param listener the callback
     */
    public void onModeChanged(Consumer<IVXGameMode> listener) {
        modeChangedListeners.add(listener);
    }

    /**
     * Registers a callback fired when a player is added.
     *
     * @param listener the callback
     */
    public void onPlayerAdded(Consumer<IVXPlayerSlot> listener) {
        playerAddedListeners.add(listener);
    }

    /**
     * Registers a callback fired when a player is removed (receives slot index).
     *
     * @param listener the callback
     */
    public void onPlayerRemoved(Consumer<Integer> listener) {
        playerRemovedListeners.add(listener);
    }

    /**
     * Registers a callback fired when a player's ready state changes.
     *
     * @param listener the callback
     */
    public void onPlayerReady(Consumer<IVXPlayerSlot> listener) {
        playerReadyListeners.add(listener);
    }

    /**
     * Registers a callback fired when the match starts ({@code true}) or ends ({@code false}).
     *
     * @param listener the callback
     */
    public void onMatchStateChanged(Consumer<Boolean> listener) {
        matchStateListeners.add(listener);
    }

    // ──────────────────────────────────────────────
    //  Internals
    // ──────────────────────────────────────────────

    private void requireModeSelected() {
        if (currentMode == null) {
            throw new IllegalStateException("No game mode selected. Call selectMode() first.");
        }
    }
}
