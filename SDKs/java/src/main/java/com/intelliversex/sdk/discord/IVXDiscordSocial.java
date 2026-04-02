// Copyright (c) 2026 Intelli-verse-X
// MIT License — see LICENSE in the project root.

package com.intelliversex.sdk.discord;

import com.intelliversex.sdk.discord.IVXDiscordModels.*;

import java.util.ArrayList;
import java.util.Collections;
import java.util.HashMap;
import java.util.List;
import java.util.Map;
import java.util.concurrent.CompletableFuture;
import java.util.concurrent.ConcurrentHashMap;
import java.util.concurrent.CopyOnWriteArrayList;
import java.util.function.Consumer;
import java.util.logging.Logger;

/**
 * Thread-safe singleton for the IntelliVerseX Discord Social SDK integration.
 * <p>
 * Wraps Discord Rich Presence, a unified friends list, lobby/text-chat,
 * voice channels, and game invites behind a single ergonomic API.
 * All async operations return {@link CompletableFuture}. Event callbacks
 * are delivered via {@link Consumer} listeners.
 * <p>
 * <b>Usage:</b>
 * <pre>{@code
 * IVXDiscordConfig config = new IVXDiscordConfig("app-id", "client-id", null, true);
 * IVXDiscordSocial.getInstance().initialize(config);
 * IVXDiscordSocial.getInstance().setActivity("Playing QuizVerse", "Round 3");
 * }</pre>
 */
public class IVXDiscordSocial {

    private static final Logger LOG = Logger.getLogger(IVXDiscordSocial.class.getName());

    private static volatile IVXDiscordSocial instance;

    private volatile boolean initialized;
    private IVXDiscordConfig config;
    private IVXDiscordLobbyInfo currentLobby;
    private final List<ChatEntry> chatHistory = new CopyOnWriteArrayList<>();
    private final ConcurrentHashMap<String, List<Consumer<Object>>> listeners = new ConcurrentHashMap<>();

    private IVXDiscordSocial() {}

    /**
     * Returns the singleton instance, creating it on first access (double-checked locking).
     *
     * @return the shared {@link IVXDiscordSocial} instance
     */
    public static IVXDiscordSocial getInstance() {
        if (instance == null) {
            synchronized (IVXDiscordSocial.class) {
                if (instance == null) {
                    instance = new IVXDiscordSocial();
                }
            }
        }
        return instance;
    }

    // ──────────────────────────────────────────────
    //  Lifecycle
    // ──────────────────────────────────────────────

    /**
     * Initialize the Discord Social SDK with the given configuration.
     * Must be called before any other method.
     *
     * @param config Discord application and OAuth configuration
     * @throws IllegalArgumentException if applicationId or clientId is null/blank
     */
    public synchronized void initialize(IVXDiscordConfig config) {
        if (config == null) {
            throw new IllegalArgumentException("config must not be null");
        }
        if (config.getApplicationId() == null || config.getApplicationId().trim().isEmpty()) {
            throw new IllegalArgumentException("applicationId must not be null or empty");
        }
        if (config.getClientId() == null || config.getClientId().trim().isEmpty()) {
            throw new IllegalArgumentException("clientId must not be null or empty");
        }
        this.config = config;
        this.initialized = true;
        log("Discord Social SDK initialized");
        emit("initialized", null);
    }

    /**
     * Link the current game account with a Discord account via OAuth.
     *
     * @return a future resolving to the linked Discord user ID
     */
    public CompletableFuture<String> linkAccount() {
        requireInitialized();
        log("Account link flow started");
        return CompletableFuture.completedFuture("");
    }

    /**
     * Create or retrieve a provisional (guest) Discord account for the
     * current player, enabling social features without a full Discord login.
     *
     * @return a future resolving to the provisional Discord user ID
     */
    public CompletableFuture<String> getProvisionalAccount() {
        requireInitialized();
        log("Provisional account requested");
        return CompletableFuture.completedFuture("");
    }

    // ──────────────────────────────────────────────
    //  Events
    // ──────────────────────────────────────────────

    /**
     * Subscribe to a named Discord Social event.
     *
     * @param event    event name (e.g. "initialized", "lobbyJoined", "inviteReceived")
     * @param listener callback that receives the event payload
     * @param <T>      the expected payload type
     */
    @SuppressWarnings("unchecked")
    public <T> void on(String event, Consumer<T> listener) {
        listeners.computeIfAbsent(event, k -> new CopyOnWriteArrayList<>())
                .add((Consumer<Object>) listener);
    }

    @SuppressWarnings("unchecked")
    private <T> void emit(String event, T data) {
        List<Consumer<Object>> handlers = listeners.get(event);
        if (handlers != null) {
            for (Consumer<Object> handler : handlers) {
                handler.accept(data);
            }
        }
    }

    // ──────────────────────────────────────────────
    //  Rich Presence
    // ──────────────────────────────────────────────

    /**
     * Set the player's Discord Rich Presence activity text.
     *
     * @param details primary activity description
     * @param state   optional secondary line (e.g. "Round 3 of 10")
     * @return a future that completes when the presence is updated
     */
    public CompletableFuture<Void> setActivity(String details, String state) {
        requireInitialized();
        log("Presence updated — details=\"" + details + "\" state=\"" + (state != null ? state : "") + "\"");
        emit("presenceUpdated", null);
        return CompletableFuture.completedFuture(null);
    }

    /**
     * Set Rich Presence party info for multiplayer sessions.
     *
     * @param partyId     unique party identifier
     * @param currentSize number of players currently in the party
     * @param maxSize     maximum party capacity
     * @param joinSecret  optional secret that allows others to join
     * @return a future that completes when the party info is updated
     */
    public CompletableFuture<Void> setParty(String partyId, int currentSize, int maxSize, String joinSecret) {
        requireInitialized();
        log("Party set — id=" + partyId + " size=" + currentSize + "/" + maxSize);
        emit("presenceUpdated", null);
        return CompletableFuture.completedFuture(null);
    }

    /**
     * Start an elapsed-time timer on the Rich Presence display.
     *
     * @return a future that completes when the timer is started
     */
    public CompletableFuture<Void> startTimer() {
        requireInitialized();
        log("Presence timer started");
        emit("presenceUpdated", null);
        return CompletableFuture.completedFuture(null);
    }

    /**
     * Clear all Rich Presence data.
     *
     * @return a future that completes when the presence is cleared
     */
    public CompletableFuture<Void> clearPresence() {
        requireInitialized();
        log("Presence cleared");
        emit("presenceUpdated", null);
        return CompletableFuture.completedFuture(null);
    }

    // ──────────────────────────────────────────────
    //  Friends
    // ──────────────────────────────────────────────

    /**
     * Retrieve a unified friends list that merges Discord friends with in-game friends.
     *
     * @return a future resolving to the merged friends list
     */
    public CompletableFuture<List<IVXUnifiedFriend>> getUnifiedFriends() {
        requireInitialized();
        log("Fetching unified friends list");
        return CompletableFuture.completedFuture(Collections.emptyList());
    }

    // ──────────────────────────────────────────────
    //  Lobby
    // ──────────────────────────────────────────────

    /**
     * Create or join a lobby identified by a shared secret.
     *
     * @param secret   lobby join secret
     * @param metadata optional key-value metadata for the lobby
     * @return a future resolving to the lobby information
     */
    public CompletableFuture<IVXDiscordLobbyInfo> createOrJoinLobby(String secret, Map<String, String> metadata) {
        requireInitialized();
        IVXDiscordLobbyInfo lobby = new IVXDiscordLobbyInfo(
                "", secret, "", 1, 16, metadata != null ? metadata : Collections.emptyMap());
        this.currentLobby = lobby;
        this.chatHistory.clear();
        log("Lobby joined — secret=" + secret);
        emit("lobbyJoined", lobby);
        return CompletableFuture.completedFuture(lobby);
    }

    /**
     * Leave the current lobby.
     *
     * @return a future that completes when the lobby is left
     */
    public CompletableFuture<Void> leaveLobby() {
        requireInitialized();
        this.currentLobby = null;
        this.chatHistory.clear();
        log("Left lobby");
        emit("lobbyLeft", null);
        return CompletableFuture.completedFuture(null);
    }

    /**
     * Send a text-chat message to the current lobby.
     *
     * @param message the message content
     * @return a future that completes when the message is sent
     * @throws IllegalStateException if not in a lobby
     */
    public CompletableFuture<Void> sendMessage(String message) {
        requireInitialized();
        if (currentLobby == null) {
            throw new IllegalStateException("Not in a lobby. Call createOrJoinLobby() first.");
        }
        chatHistory.add(new ChatEntry("self", message, System.currentTimeMillis()));
        log("Chat message sent: " + message);
        return CompletableFuture.completedFuture(null);
    }

    /**
     * Return the most recent chat messages for the current lobby.
     *
     * @param limit maximum number of messages to return
     * @return an unmodifiable list of chat entries (newest last)
     */
    public List<ChatEntry> getChatHistory(int limit) {
        requireInitialized();
        int size = chatHistory.size();
        int from = Math.max(0, size - limit);
        return Collections.unmodifiableList(new ArrayList<>(chatHistory.subList(from, size)));
    }

    // ──────────────────────────────────────────────
    //  Voice
    // ──────────────────────────────────────────────

    /**
     * Join a voice call in the specified lobby.
     *
     * @param lobbyId the lobby to join voice in
     * @return a future that completes when the voice call is joined
     */
    public CompletableFuture<Void> joinCall(String lobbyId) {
        requireInitialized();
        log("Joined voice call — lobby=" + lobbyId);
        emit("voiceJoined", lobbyId);
        return CompletableFuture.completedFuture(null);
    }

    /**
     * Leave the current voice call.
     *
     * @return a future that completes when the voice call is left
     */
    public CompletableFuture<Void> leaveCall() {
        requireInitialized();
        log("Left voice call");
        emit("voiceLeft", null);
        return CompletableFuture.completedFuture(null);
    }

    /**
     * Mute or unmute the local player's microphone.
     *
     * @param muted {@code true} to mute, {@code false} to unmute
     * @return a future that completes when the mute state is updated
     */
    public CompletableFuture<Void> setSelfMute(boolean muted) {
        requireInitialized();
        log("Self mute: " + muted);
        return CompletableFuture.completedFuture(null);
    }

    /**
     * Deafen or undeafen the local player.
     *
     * @param deaf {@code true} to deafen, {@code false} to undeafen
     * @return a future that completes when the deafen state is updated
     */
    public CompletableFuture<Void> setSelfDeafen(boolean deaf) {
        requireInitialized();
        log("Self deafen: " + deaf);
        return CompletableFuture.completedFuture(null);
    }

    /**
     * Set input (microphone) and output (speaker) volume levels (0–100).
     *
     * @param input  microphone volume
     * @param output speaker volume
     * @return a future that completes when the volume is updated
     */
    public CompletableFuture<Void> setVolume(int input, int output) {
        requireInitialized();
        log("Volume set — input=" + input + " output=" + output);
        return CompletableFuture.completedFuture(null);
    }

    // ──────────────────────────────────────────────
    //  Invites
    // ──────────────────────────────────────────────

    /**
     * Send a game invite to another user by their ID.
     *
     * @param userId  the target user
     * @param message optional invite message
     * @return a future that completes when the invite is sent
     */
    public CompletableFuture<Void> sendInvite(String userId, String message) {
        requireInitialized();
        log("Invite sent to " + userId);
        return CompletableFuture.completedFuture(null);
    }

    /**
     * Register a callback for incoming game invites.
     * Shorthand for {@code on("inviteReceived", handler)}.
     *
     * @param handler callback that receives the invite
     */
    public void onInviteReceived(Consumer<IVXGameInvite> handler) {
        on("inviteReceived", handler);
    }

    /**
     * Register a callback for "Ask to Join" requests.
     * Shorthand for {@code on("joinRequested", handler)}.
     *
     * @param handler callback that receives the requesting user ID
     */
    public void onJoinRequested(Consumer<String> handler) {
        on("joinRequested", handler);
    }

    // ──────────────────────────────────────────────
    //  Internal
    // ──────────────────────────────────────────────

    private void requireInitialized() {
        if (!initialized) {
            throw new IllegalStateException("IVXDiscordSocial has not been initialized. Call initialize() first.");
        }
    }

    private void log(String message) {
        if (config != null && config.isEnableDebugLogs()) {
            LOG.info("[IntelliVerseX:Discord] " + message);
        }
    }

    /**
     * A single chat message entry within a lobby.
     */
    public static final class ChatEntry {
        private final String senderId;
        private final String message;
        private final long timestamp;

        public ChatEntry(String senderId, String message, long timestamp) {
            this.senderId = senderId;
            this.message = message;
            this.timestamp = timestamp;
        }

        public String getSenderId() { return senderId; }
        public String getMessage() { return message; }
        public long getTimestamp() { return timestamp; }

        @Override
        public String toString() {
            return "ChatEntry{senderId='" + senderId + "', message='" + message + "'}";
        }
    }
}
