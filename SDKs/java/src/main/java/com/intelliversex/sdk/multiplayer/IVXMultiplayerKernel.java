/*
 * Copyright (c) 2026 Intelli-verse-X
 * MIT License - see LICENSE in the project root.
 */
package com.intelliversex.sdk.multiplayer;

import com.google.gson.Gson;
import com.google.gson.JsonObject;
import com.heroiclabs.nakama.Client;
import com.heroiclabs.nakama.Session;
import com.heroiclabs.nakama.api.Rpc;

import java.util.Collections;
import java.util.Map;
import java.util.Objects;
import java.util.concurrent.CompletableFuture;

/**
 * Java / Android adapter for the IntelliVerseX Multiplayer Kernel RPC surface.
 *
 * <p>The official Nakama Java realtime socket API has changed across releases.
 * This adapter intentionally keeps the Java package buildable and aligned with
 * the deployed server by exposing the stable RPC surface first. Realtime match
 * socket helpers should be reintroduced behind a version-pinned transport once
 * the Nakama Java dependency is upgraded and verified.</p>
 */
public final class IVXMultiplayerKernel {
    private static final Gson GSON = new Gson();

    private final Client client;
    private final Session session;
    private volatile boolean initialized = false;

    /** Request sent to `mp_create_match`. */
    public static final class IVXCreateMatchRequest {
        public final String templateId;
        public final String gameId;
        public final String region;
        public final Map<String, Object> templateInit;

        public IVXCreateMatchRequest(String templateId, String gameId, String region, Map<String, Object> templateInit) {
            this.templateId = templateId == null ? "" : templateId;
            this.gameId = gameId == null ? "" : gameId;
            this.region = region == null ? "" : region;
            this.templateInit = templateInit == null ? Collections.emptyMap() : templateInit;
        }
    }

    /** Response returned by `mp_create_match`. */
    public static final class IVXCreateMatchResponse {
        public final String matchId;
        public final String templateId;
        public final String gameId;
        public final String region;
        public final long serverUnixMs;
        public final String rawJson;

        public IVXCreateMatchResponse(String matchId, String templateId, String gameId, String region, long serverUnixMs, String rawJson) {
            this.matchId = matchId == null ? "" : matchId;
            this.templateId = templateId == null ? "" : templateId;
            this.gameId = gameId == null ? "" : gameId;
            this.region = region == null ? "" : region;
            this.serverUnixMs = serverUnixMs;
            this.rawJson = rawJson == null ? "{}" : rawJson;
        }
    }

    public IVXMultiplayerKernel(Client client, Session session) {
        this.client = Objects.requireNonNull(client, "client must not be null");
        this.session = Objects.requireNonNull(session, "session must not be null");
    }

    /** Marks the RPC adapter ready. No realtime socket is opened. */
    public CompletableFuture<Void> initialize() {
        initialized = true;
        return CompletableFuture.completedFuture(null);
    }

    /** Marks the RPC adapter not ready. */
    public CompletableFuture<Void> shutdown() {
        initialized = false;
        return CompletableFuture.completedFuture(null);
    }

    public boolean isInitialized() {
        return initialized;
    }

    public CompletableFuture<IVXCreateMatchResponse> createMatch(IVXCreateMatchRequest req) {
        ensureInitialized();
        if (req == null || req.templateId.trim().isEmpty()) {
            return failedFuture(new IllegalArgumentException("templateId required"));
        }

        JsonObject payload = new JsonObject();
        payload.addProperty("template_id", req.templateId);
        payload.addProperty("game_id", req.gameId);
        payload.addProperty("region", req.region);
        payload.add("template_init", GSON.toJsonTree(req.templateInit));

        return rpc("mp_create_match", GSON.toJson(payload))
            .thenApply(body -> {
                JsonObject obj = parseObject(body);
                return new IVXCreateMatchResponse(
                    optString(obj, "match_id"),
                    optString(obj, "template_id"),
                    optString(obj, "game_id"),
                    optString(obj, "region"),
                    optLong(obj, "server_unix_ms"),
                    body
                );
            });
    }

    public CompletableFuture<String> listTemplates() {
        return rpc("mp_list_templates", "{}");
    }

    public CompletableFuture<String> readMatchResult(String matchId) {
        if (matchId == null || matchId.trim().isEmpty()) {
            return failedFuture(new IllegalArgumentException("matchId required"));
        }
        JsonObject payload = new JsonObject();
        payload.addProperty("match_id", matchId);
        return rpc("mp_read_match_result", GSON.toJson(payload));
    }

    public CompletableFuture<String> listAgentPersonas() {
        return rpc("mp_agent_list_personas", "{}");
    }

    public CompletableFuture<String> spawnAgent(String requestJson) {
        return rpc("mp_agent_spawn", normalizeJsonObject(requestJson));
    }

    public CompletableFuture<String> despawnAgent(String requestJson) {
        return rpc("mp_agent_despawn", normalizeJsonObject(requestJson));
    }

    public CompletableFuture<String> agentSpeak(String requestJson) {
        return rpc("mp_agent_speak", normalizeJsonObject(requestJson));
    }

    /** Generic escape hatch for newly deployed multiplayer RPCs. */
    public CompletableFuture<String> rpc(String rpcId, String payloadJson) {
        ensureInitialized();
        if (rpcId == null || rpcId.trim().isEmpty()) {
            return failedFuture(new IllegalArgumentException("rpcId required"));
        }
        return CompletableFuture.supplyAsync(() -> {
            try {
                Rpc result = client.rpc(session, rpcId, normalizeJsonObject(payloadJson)).get();
                String payload = result.getPayload();
                return payload == null || payload.isEmpty() ? "{}" : payload;
            } catch (Exception e) {
                throw new RuntimeException("RPC " + rpcId + " failed", e);
            }
        });
    }

    private void ensureInitialized() {
        if (!initialized) {
            throw new IllegalStateException("not initialized");
        }
    }

    private static String normalizeJsonObject(String raw) {
        if (raw == null || raw.trim().isEmpty()) {
            return "{}";
        }
        String trimmed = raw.trim();
        return trimmed.startsWith("{") && trimmed.endsWith("}") ? trimmed : "{}";
    }

    private static JsonObject parseObject(String raw) {
        try {
            JsonObject obj = GSON.fromJson(raw, JsonObject.class);
            return obj == null ? new JsonObject() : obj;
        } catch (Exception ignored) {
            return new JsonObject();
        }
    }

    private static String optString(JsonObject obj, String key) {
        return obj.has(key) && !obj.get(key).isJsonNull() ? obj.get(key).getAsString() : "";
    }

    private static long optLong(JsonObject obj, String key) {
        return obj.has(key) && !obj.get(key).isJsonNull() ? obj.get(key).getAsLong() : 0L;
    }

    private static <T> CompletableFuture<T> failedFuture(Throwable throwable) {
        CompletableFuture<T> future = new CompletableFuture<>();
        future.completeExceptionally(throwable);
        return future;
    }
}
