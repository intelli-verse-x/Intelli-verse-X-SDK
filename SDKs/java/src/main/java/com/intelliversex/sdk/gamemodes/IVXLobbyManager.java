// Copyright (c) 2026 Intelli-verse-X
// MIT License — see LICENSE in the project root.

package com.intelliversex.sdk.gamemodes;

import com.google.gson.Gson;
import com.google.gson.reflect.TypeToken;
import com.heroiclabs.nakama.Client;
import com.heroiclabs.nakama.Session;
import com.heroiclabs.nakama.api.Rpc;
import com.intelliversex.sdk.gamemodes.IVXGameModeModels.IVXMatchConfig;
import com.intelliversex.sdk.gamemodes.IVXGameModeModels.IVXRoomInfo;

import java.lang.reflect.Type;
import java.util.Collections;
import java.util.List;
import java.util.Objects;
import java.util.concurrent.CompletableFuture;
import java.util.function.Consumer;

/**
 * Thread-safe singleton that manages lobby rooms (create, join, leave, list)
 * via Nakama RPCs.
 * <p>
 * Must be initialized with a Nakama {@link Client} and {@link Session} before
 * use. All network operations return {@link CompletableFuture}.
 * <p>
 * <b>Usage:</b>
 * <pre>{@code
 * IVXLobbyManager lobby = IVXLobbyManager.getInstance();
 * lobby.initialize(nakamaClient, session);
 * lobby.listRooms().thenAccept(rooms -> { ... });
 * }</pre>
 */
public class IVXLobbyManager {

    private static final String RPC_CREATE_ROOM = "ivx_lobby_create";
    private static final String RPC_JOIN_ROOM = "ivx_lobby_join";
    private static final String RPC_LEAVE_ROOM = "ivx_lobby_leave";
    private static final String RPC_LIST_ROOMS = "ivx_lobby_list";

    private static volatile IVXLobbyManager instance;

    private volatile boolean initialized;
    private Client client;
    private Session session;
    private final Gson gson = new Gson();

    private final List<Consumer<IVXRoomInfo>> roomJoinedListeners = new java.util.concurrent.CopyOnWriteArrayList<>();
    private final List<Consumer<String>> roomLeftListeners = new java.util.concurrent.CopyOnWriteArrayList<>();

    private IVXLobbyManager() {}

    /**
     * Returns the singleton instance, creating it on first access.
     *
     * @return the shared {@link IVXLobbyManager} instance
     */
    public static IVXLobbyManager getInstance() {
        if (instance == null) {
            synchronized (IVXLobbyManager.class) {
                if (instance == null) {
                    instance = new IVXLobbyManager();
                }
            }
        }
        return instance;
    }

    /**
     * Initializes the lobby manager with a Nakama client and an active session.
     *
     * @param client  the Nakama client
     * @param session an authenticated session
     * @throws IllegalArgumentException if either argument is null
     */
    public synchronized void initialize(Client client, Session session) {
        Objects.requireNonNull(client, "client must not be null");
        Objects.requireNonNull(session, "session must not be null");
        this.client = client;
        this.session = session;
        this.initialized = true;
    }

    /**
     * Updates the session (e.g. after a token refresh) without re-initializing.
     *
     * @param session the new session
     */
    public synchronized void updateSession(Session session) {
        Objects.requireNonNull(session, "session must not be null");
        this.session = session;
    }

    // ──────────────────────────────────────────────
    //  Room Operations
    // ──────────────────────────────────────────────

    /**
     * Creates a new lobby room with the given configuration.
     *
     * @param roomName the human-readable room name
     * @param config   match configuration (mode, max players, etc.)
     * @return a future resolving to the created room info
     */
    public CompletableFuture<IVXRoomInfo> createRoom(String roomName, IVXMatchConfig config) {
        requireInitialized();
        Objects.requireNonNull(config, "config must not be null");
        com.google.gson.JsonObject body = new com.google.gson.JsonObject();
        body.addProperty("room_name", roomName);
        body.add("config", gson.toJsonTree(config));
        return rpc(RPC_CREATE_ROOM, gson.toJson(body))
                .thenApply(payload -> {
                    IVXRoomInfo room = gson.fromJson(payload, IVXRoomInfo.class);
                    roomJoinedListeners.forEach(l -> l.accept(room));
                    return room;
                });
    }

    /**
     * Joins an existing lobby room by its ID.
     *
     * @param roomId the room to join
     * @return a future resolving to the room info
     */
    public CompletableFuture<IVXRoomInfo> joinRoom(String roomId) {
        requireInitialized();
        com.google.gson.JsonObject body = new com.google.gson.JsonObject();
        body.addProperty("room_id", roomId);
        return rpc(RPC_JOIN_ROOM, gson.toJson(body))
                .thenApply(payload -> {
                    IVXRoomInfo room = gson.fromJson(payload, IVXRoomInfo.class);
                    roomJoinedListeners.forEach(l -> l.accept(room));
                    return room;
                });
    }

    /**
     * Leaves a lobby room.
     *
     * @param roomId the room to leave
     * @return a future that completes when the server acknowledges the leave
     */
    public CompletableFuture<Void> leaveRoom(String roomId) {
        requireInitialized();
        com.google.gson.JsonObject body = new com.google.gson.JsonObject();
        body.addProperty("room_id", roomId);
        return rpc(RPC_LEAVE_ROOM, gson.toJson(body))
                .thenRun(() -> roomLeftListeners.forEach(l -> l.accept(roomId)));
    }

    /**
     * Lists all available lobby rooms for the current game mode.
     *
     * @return a future resolving to an unmodifiable list of rooms
     */
    public CompletableFuture<List<IVXRoomInfo>> listRooms() {
        requireInitialized();
        Type listType = new TypeToken<List<IVXRoomInfo>>() {}.getType();
        return rpc(RPC_LIST_ROOMS, "{}")
                .thenApply(payload -> {
                    List<IVXRoomInfo> rooms = gson.fromJson(payload, listType);
                    return rooms != null ? Collections.unmodifiableList(rooms) : Collections.emptyList();
                });
    }

    // ──────────────────────────────────────────────
    //  Event Callbacks
    // ──────────────────────────────────────────────

    /**
     * Registers a callback fired when the local player joins a room.
     *
     * @param listener the callback
     */
    public void onRoomJoined(Consumer<IVXRoomInfo> listener) {
        roomJoinedListeners.add(listener);
    }

    /**
     * Registers a callback fired when the local player leaves a room (receives room ID).
     *
     * @param listener the callback
     */
    public void onRoomLeft(Consumer<String> listener) {
        roomLeftListeners.add(listener);
    }

    // ──────────────────────────────────────────────
    //  Internals
    // ──────────────────────────────────────────────

    private CompletableFuture<String> rpc(String rpcId, String payload) {
        return CompletableFuture.supplyAsync(() -> {
            try {
                Rpc result = client.rpc(session, rpcId, payload).get();
                return result.getPayload();
            } catch (Exception e) {
                throw new RuntimeException("RPC '" + rpcId + "' failed: " + e.getMessage(), e);
            }
        });
    }

    private void requireInitialized() {
        if (!initialized) {
            throw new IllegalStateException("IVXLobbyManager has not been initialized. Call initialize() first.");
        }
    }
}
