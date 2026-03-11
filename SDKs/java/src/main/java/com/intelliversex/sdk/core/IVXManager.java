package com.intelliversex.sdk.core;

import com.heroiclabs.nakama.Client;
import com.heroiclabs.nakama.DefaultClient;
import com.heroiclabs.nakama.Session;
import com.heroiclabs.nakama.api.Account;
import com.heroiclabs.nakama.api.LeaderboardRecord;
import com.heroiclabs.nakama.api.LeaderboardRecordList;
import com.heroiclabs.nakama.api.Rpc;
import com.heroiclabs.nakama.api.StorageObjectAcks;
import com.heroiclabs.nakama.api.StorageObjects;
import com.heroiclabs.nakama.api.WriteStorageObject;
import com.heroiclabs.nakama.api.ReadStorageObjectId;
import com.google.common.util.concurrent.ListenableFuture;
import com.google.gson.Gson;
import com.google.gson.JsonObject;
import com.google.protobuf.StringValue;

import java.util.*;
import java.util.concurrent.CompletableFuture;
import java.util.concurrent.ConcurrentHashMap;
import java.util.concurrent.CopyOnWriteArrayList;
import java.util.function.Consumer;
import java.util.prefs.BackingStoreException;
import java.util.prefs.Preferences;

/**
 * Central manager for the IntelliVerseX Java/Android SDK.
 * <p>
 * Wraps the Nakama Java client with auth, profile, wallet, leaderboards,
 * storage, and RPC.
 * <p>
 * <b>Thread-safety:</b> All public methods that mutate state use synchronized
 * blocks on this instance. Callbacks and futures are safe to use from any thread.
 * <p>
 * <b>Android note:</b> {@code java.util.prefs.Preferences} may not work on all
 * Android devices. For Android projects, call
 * {@link #setPreferences(Preferences)} with a custom {@code Preferences}
 * implementation backed by {@code SharedPreferences} <i>before</i> calling
 * {@link #initialize(IVXConfig)}.
 */
public class IVXManager {
    public static final String SDK_VERSION = "5.1.0";

    private static final String PREF_SESSION_TOKEN = "ivx_session_token";
    private static final String PREF_REFRESH_TOKEN = "ivx_refresh_token";
    private static final String PREF_DEVICE_ID = "ivx_device_id";

    private static volatile IVXManager instance;

    private IVXConfig config;
    private Client client;
    private volatile Session session;
    private volatile boolean initialized = false;
    private final Gson gson = new Gson();
    private Preferences prefs = Preferences.userNodeForPackage(IVXManager.class);

    private final Map<String, List<Consumer<Object>>> listeners = new ConcurrentHashMap<>();

    private IVXManager() {}

    public static IVXManager getInstance() {
        if (instance == null) {
            synchronized (IVXManager.class) {
                if (instance == null) {
                    instance = new IVXManager();
                }
            }
        }
        return instance;
    }

    /**
     * Replace the default {@link Preferences} store. On Android, provide one backed
     * by SharedPreferences. Must be called <i>before</i> {@link #initialize(IVXConfig)}.
     */
    public synchronized void setPreferences(Preferences prefs) {
        Objects.requireNonNull(prefs, "prefs must not be null");
        this.prefs = prefs;
    }

    public boolean isInitialized() { return initialized; }
    public Client getClient() { return client; }
    public Session getSession() { return session; }
    public String getUserId() { return session != null ? session.getUserId() : ""; }
    public String getUsername() { return session != null ? session.getUsername() : ""; }
    public boolean hasValidSession() { return session != null && !session.isExpired(); }

    // ─── Events ─────────────────────────────────────────────────

    @SuppressWarnings("unchecked")
    public <T> void on(String event, Consumer<T> handler) {
        listeners.computeIfAbsent(event, k -> new CopyOnWriteArrayList<>()).add((Consumer<Object>) handler);
    }

    @SuppressWarnings("unchecked")
    private <T> void emit(String event, T data) {
        List<Consumer<Object>> handlers = listeners.get(event);
        if (handlers != null) {
            for (Consumer<Object> h : handlers) {
                h.accept(data);
            }
        }
    }

    // ─── Init ───────────────────────────────────────────────────

    public synchronized void initialize(IVXConfig config) {
        Objects.requireNonNull(config, "config must not be null");
        this.config = config;

        this.client = new DefaultClient(
                config.getNakamaServerKey(),
                config.getNakamaHost(),
                config.getNakamaPort(),
                config.isUseSSL()
        );

        this.initialized = true;
        log("SDK initialized — " + config.getBaseUrl());
        emit("initialized", null);
    }

    // ─── Auth (blocking) ────────────────────────────────────────

    public void authenticateDevice(String deviceId) {
        ensureInitialized();
        String resolvedId = (deviceId == null || deviceId.isEmpty()) ? getPersistentDeviceId() : deviceId;

        try {
            Session newSession = client.authenticateDevice(resolvedId).get();
            onAuthSuccess(newSession);
        } catch (Exception e) {
            log("Auth failed: " + e.getMessage());
            emit("authError", e.getMessage());
        }
    }

    public void authenticateEmail(String email, String password, boolean create) {
        ensureInitialized();

        try {
            Session newSession = client.authenticateEmail(email, password, create).get();
            onAuthSuccess(newSession);
        } catch (Exception e) {
            emit("authError", e.getMessage());
        }
    }

    public void authenticateGoogle(String token) {
        ensureInitialized();

        try {
            Session newSession = client.authenticateGoogle(token).get();
            onAuthSuccess(newSession);
        } catch (Exception e) {
            emit("authError", e.getMessage());
        }
    }

    public void authenticateApple(String token) {
        ensureInitialized();

        try {
            Session newSession = client.authenticateApple(token).get();
            onAuthSuccess(newSession);
        } catch (Exception e) {
            emit("authError", e.getMessage());
        }
    }

    public void authenticateCustom(String customId) {
        ensureInitialized();

        try {
            Session newSession = client.authenticateCustom(customId).get();
            onAuthSuccess(newSession);
        } catch (Exception e) {
            emit("authError", e.getMessage());
        }
    }

    // ─── Auth (async) ───────────────────────────────────────────

    /**
     * Authenticates with a device ID asynchronously.
     *
     * @param deviceId device identifier, or {@code null}/empty to auto-generate
     * @return a {@link CompletableFuture} that completes with the user ID on success
     */
    public CompletableFuture<String> authenticateDeviceAsync(String deviceId) {
        ensureInitialized();
        String resolvedId = (deviceId == null || deviceId.isEmpty()) ? getPersistentDeviceId() : deviceId;

        return CompletableFuture.supplyAsync(() -> {
            try {
                Session newSession = client.authenticateDevice(resolvedId).get();
                onAuthSuccess(newSession);
                return newSession.getUserId();
            } catch (Exception e) {
                log("Async auth failed: " + e.getMessage());
                emit("authError", e.getMessage());
                throw new RuntimeException("Authentication failed", e);
            }
        });
    }

    /**
     * Authenticates with email/password asynchronously.
     */
    public CompletableFuture<String> authenticateEmailAsync(String email, String password, boolean create) {
        ensureInitialized();

        return CompletableFuture.supplyAsync(() -> {
            try {
                Session newSession = client.authenticateEmail(email, password, create).get();
                onAuthSuccess(newSession);
                return newSession.getUserId();
            } catch (Exception e) {
                emit("authError", e.getMessage());
                throw new RuntimeException("Authentication failed", e);
            }
        });
    }

    /**
     * Authenticates with a Google token asynchronously.
     */
    public CompletableFuture<String> authenticateGoogleAsync(String token) {
        ensureInitialized();

        return CompletableFuture.supplyAsync(() -> {
            try {
                Session newSession = client.authenticateGoogle(token).get();
                onAuthSuccess(newSession);
                return newSession.getUserId();
            } catch (Exception e) {
                emit("authError", e.getMessage());
                throw new RuntimeException("Authentication failed", e);
            }
        });
    }

    /**
     * Authenticates with an Apple token asynchronously.
     */
    public CompletableFuture<String> authenticateAppleAsync(String token) {
        ensureInitialized();

        return CompletableFuture.supplyAsync(() -> {
            try {
                Session newSession = client.authenticateApple(token).get();
                onAuthSuccess(newSession);
                return newSession.getUserId();
            } catch (Exception e) {
                emit("authError", e.getMessage());
                throw new RuntimeException("Authentication failed", e);
            }
        });
    }

    /**
     * Authenticates with a custom ID asynchronously.
     */
    public CompletableFuture<String> authenticateCustomAsync(String customId) {
        ensureInitialized();

        return CompletableFuture.supplyAsync(() -> {
            try {
                Session newSession = client.authenticateCustom(customId).get();
                onAuthSuccess(newSession);
                return newSession.getUserId();
            } catch (Exception e) {
                emit("authError", e.getMessage());
                throw new RuntimeException("Authentication failed", e);
            }
        });
    }

    // ─── Session ────────────────────────────────────────────────

    public synchronized boolean restoreSession() {
        String token = prefs.get(PREF_SESSION_TOKEN, "");
        String refresh = prefs.get(PREF_REFRESH_TOKEN, "");

        if (token.isEmpty()) return false;

        session = DefaultClient.restoreSession(token, refresh);
        if (session.isExpired()) {
            session = null;
            return false;
        }

        log("Session restored for user: " + session.getUserId());
        syncMetadata();
        return true;
    }

    public synchronized void clearSession() {
        session = null;
        prefs.put(PREF_SESSION_TOKEN, "");
        prefs.put(PREF_REFRESH_TOKEN, "");
        flushPrefs();
        log("Session cleared");
    }

    // ─── Profile ────────────────────────────────────────────────

    /**
     * Fetches the current player's profile (blocking).
     *
     * @return an {@link IVXProfile} or {@code null} on failure
     */
    public IVXProfile fetchProfile() {
        ensureSession();

        try {
            Account account = client.getAccount(session).get();
            IVXProfile profile = new IVXProfile(
                    account.getUser().getId(),
                    account.getUser().getUsername(),
                    account.getUser().getDisplayName(),
                    account.getUser().getAvatarUrl(),
                    account.getUser().getLangTag(),
                    account.getUser().getMetadata(),
                    account.getWallet()
            );
            emit("profileLoaded", profile);
            return profile;
        } catch (Exception e) {
            emit("error", e.getMessage());
            return null;
        }
    }

    /**
     * Fetches the current player's profile asynchronously.
     */
    public CompletableFuture<IVXProfile> fetchProfileAsync() {
        ensureSession();

        return CompletableFuture.supplyAsync(() -> fetchProfile());
    }

    public void updateProfile(String displayName, String avatarUrl, String langTag) {
        ensureSession();

        try {
            client.updateAccount(session, null, displayName, avatarUrl, langTag, null).get();
            log("Profile updated");
        } catch (Exception e) {
            emit("error", e.getMessage());
        }
    }

    // ─── Wallet ─────────────────────────────────────────────────

    public String fetchWallet() {
        return callRpc("hiro_economy_list", "{}");
    }

    public String grantCurrency(String currencyId, long amount) {
        JsonObject currencies = new JsonObject();
        currencies.addProperty(currencyId, amount);
        JsonObject payload = new JsonObject();
        payload.add("currencies", currencies);
        return callRpc("hiro_economy_grant", gson.toJson(payload));
    }

    // ─── Leaderboard ────────────────────────────────────────────

    public void submitScore(String leaderboardId, long score) {
        ensureSession();

        try {
            client.writeLeaderboardRecord(session, leaderboardId, score).get();
            log("Score submitted: " + score + " to " + leaderboardId);
        } catch (Exception e) {
            emit("error", e.getMessage());
        }
    }

    public List<Map<String, Object>> fetchLeaderboard(String leaderboardId, int limit) {
        ensureSession();

        try {
            LeaderboardRecordList result = client.listLeaderboardRecords(session, leaderboardId, null, null, limit).get();
            List<Map<String, Object>> records = new ArrayList<>();
            for (LeaderboardRecord r : result.getRecordsList()) {
                Map<String, Object> record = new HashMap<>();
                record.put("ownerId", r.getOwnerId());

                StringValue usernameVal = r.getUsername();
                record.put("username", (usernameVal != null && !usernameVal.getValue().isEmpty())
                        ? usernameVal.getValue() : "");

                record.put("score", r.getScore());
                record.put("rank", r.getRank());
                records.add(record);
            }
            return records;
        } catch (Exception e) {
            emit("error", e.getMessage());
            return Collections.emptyList();
        }
    }

    // ─── Storage ────────────────────────────────────────────────

    public void writeStorage(String collection, String key, String valueJson) {
        ensureSession();

        try {
            WriteStorageObject obj = WriteStorageObject.newBuilder()
                    .setCollection(collection)
                    .setKey(key)
                    .setValue(valueJson)
                    .setPermissionRead(1)
                    .setPermissionWrite(1)
                    .build();
            client.writeStorageObjects(session, obj).get();
            log("Storage write: " + collection + "/" + key);
        } catch (Exception e) {
            emit("error", e.getMessage());
        }
    }

    public String readStorage(String collection, String key) {
        ensureSession();

        try {
            ReadStorageObjectId id = ReadStorageObjectId.newBuilder()
                    .setCollection(collection)
                    .setKey(key)
                    .setUserId(getUserId())
                    .build();
            StorageObjects result = client.readStorageObjects(session, id).get();
            if (result.getObjectsCount() > 0) {
                return result.getObjects(0).getValue();
            }
            return "{}";
        } catch (Exception e) {
            emit("error", e.getMessage());
            return "{}";
        }
    }

    // ─── RPC ────────────────────────────────────────────────────

    /**
     * Calls a server RPC endpoint (blocking).
     *
     * @param rpcId       the RPC identifier
     * @param payloadJson JSON payload string; {@code null} is normalized to "{}"
     * @return the response payload JSON, or "{}" on failure
     */
    public String callRpc(String rpcId, String payloadJson) {
        ensureSession();
        String safePayload = (payloadJson == null) ? "{}" : payloadJson;

        try {
            Rpc result = client.rpc(session, rpcId, safePayload).get();
            log("RPC " + rpcId + " response received");
            return result.getPayload();
        } catch (Exception e) {
            log("RPC " + rpcId + " failed: " + e.getMessage());
            emit("error", e.getMessage());
            return "{}";
        }
    }

    /**
     * Calls a server RPC endpoint asynchronously.
     */
    public CompletableFuture<String> callRpcAsync(String rpcId, String payloadJson) {
        ensureSession();
        String safePayload = (payloadJson == null) ? "{}" : payloadJson;

        return CompletableFuture.supplyAsync(() -> {
            try {
                Rpc result = client.rpc(session, rpcId, safePayload).get();
                log("RPC " + rpcId + " response received");
                return result.getPayload();
            } catch (Exception e) {
                log("RPC " + rpcId + " failed: " + e.getMessage());
                emit("error", e.getMessage());
                throw new RuntimeException("RPC " + rpcId + " failed", e);
            }
        });
    }

    // ─── Internal ───────────────────────────────────────────────

    private synchronized void onAuthSuccess(Session newSession) {
        this.session = newSession;
        prefs.put(PREF_SESSION_TOKEN, newSession.getAuthToken());
        prefs.put(PREF_REFRESH_TOKEN, newSession.getRefreshToken());
        flushPrefs();
        log("Authenticated — UserId: " + newSession.getUserId());
        syncMetadata();
        emit("authSuccess", newSession.getUserId());
    }

    private void syncMetadata() {
        if (!hasValidSession()) return;
        JsonObject meta = new JsonObject();
        meta.addProperty("sdk_version", SDK_VERSION);
        meta.addProperty("platform", System.getProperty("os.name", "unknown"));
        meta.addProperty("engine", "java");
        meta.addProperty("java_version", System.getProperty("java.version", "unknown"));

        JsonObject payload = new JsonObject();
        payload.add("metadata", meta);

        try {
            callRpc("ivx_sync_metadata", gson.toJson(payload));
        } catch (Exception ignored) {
            // Metadata sync failure is non-fatal
        }
    }

    private synchronized String getPersistentDeviceId() {
        String id = prefs.get(PREF_DEVICE_ID, "");
        if (id.isEmpty()) {
            id = UUID.randomUUID().toString();
            prefs.put(PREF_DEVICE_ID, id);
            flushPrefs();
        }
        return id;
    }

    private void flushPrefs() {
        try {
            prefs.flush();
        } catch (BackingStoreException e) {
            log("WARNING: Could not flush preferences — " + e.getMessage());
        }
    }

    private void ensureInitialized() {
        if (!initialized || client == null) {
            throw new IllegalStateException("IntelliVerseX SDK not initialized. Call initialize() first.");
        }
    }

    private void ensureSession() {
        ensureInitialized();
        if (!hasValidSession()) {
            throw new IllegalStateException("No valid Nakama session. Authenticate first.");
        }
    }

    private void log(String message) {
        if (config != null && config.isEnableDebugLogs()) {
            System.out.println("[IntelliVerseX] " + message);
        }
    }
}
