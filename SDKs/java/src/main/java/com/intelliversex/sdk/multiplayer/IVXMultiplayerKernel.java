/*
 * Copyright (c) 2026 Intelli-verse-X
 * MIT License — see LICENSE in the project root.
 *
 * IVXMultiplayerKernel — Java / Android adapter for the IntelliVerseX
 * Multiplayer Kernel. Mirrors the IIVXMultiplayer / IIVXMatchSession contract
 * from the Unity, JS, Unreal, Godot, Flutter, C++, and Web3 SDKs.
 *
 * Wraps the official `nakama-java` client (com.heroiclabs.nakama:nakama-java)
 * and speaks the wire protocol defined in
 *   `Intelli-verse-X-SDK/schemas/multiplayer/*.proto`.
 *
 * Usage:
 *   IVXMultiplayerKernel kernel = new IVXMultiplayerKernel(client, session);
 *   kernel.initialize().get();
 *   IVXCreateMatchResponse r = kernel.createMatch(
 *       new IVXCreateMatchRequest("sync-turn-v1", "demo", "",
 *                                 java.util.Map.of("min_players", 2))).get();
 *   IVXMatchSession session = kernel.joinMatch(r.matchId).get();
 *   session.subscribe(0xC100, env -> Log.d("QV", "question " + env.payloadJson));
 *   session.send(0xC101, "{\"answer_id\":\"a\"}").get();
 */
package com.intelliversex.sdk.multiplayer;

import com.google.gson.Gson;
import com.google.gson.JsonObject;
import com.google.gson.JsonElement;
import com.heroiclabs.nakama.Client;
import com.heroiclabs.nakama.Session;
import com.heroiclabs.nakama.SocketClient;
import com.heroiclabs.nakama.SocketListener;
import com.heroiclabs.nakama.api.MatchData;
import com.heroiclabs.nakama.api.Match;

import java.nio.charset.StandardCharsets;
import java.util.ArrayList;
import java.util.Collections;
import java.util.HashMap;
import java.util.List;
import java.util.Map;
import java.util.Random;
import java.util.UUID;
import java.util.concurrent.CompletableFuture;
import java.util.concurrent.ConcurrentHashMap;
import java.util.concurrent.atomic.AtomicLong;
import java.util.function.Consumer;

/**
 * Top-level adapter. One per authenticated player.
 *
 * <p>This class is thread-safe; subscriptions and send() may be called from
 * any thread. Inbound dispatch happens on whichever thread the underlying
 * Nakama-Java socket fires its callback on (typically a Netty IO thread —
 * games should marshal back to the UI thread before mutating UI state).
 */
public final class IVXMultiplayerKernel {

    /** Transport-state machine — identical across all kernel adapters. */
    public enum TransportState { DISCONNECTED, CONNECTING, CONNECTED, RECONNECTING, FAILED_FATAL }

    /** Mirrors `kernel.proto EndReason`. */
    public enum EndReason {
        UNKNOWN, COMPLETED, CANCELLED, DURATION_EXCEEDED,
        KERNEL_INTERNAL, ALL_PLAYERS_LEFT, HOST_TERMINATED
    }

    public static final class Header {
        public final long seq;
        public final long matchTimeMs;
        public final String uuid;
        public final int opCode;
        public final String senderUserId;
        public Header(long s, long t, String u, int op, String sender) {
            this.seq = s; this.matchTimeMs = t; this.uuid = u;
            this.opCode = op; this.senderUserId = sender;
        }
    }

    /** Inbound event delivered to subscribers. payloadJson is the raw kernel payload. */
    public static final class Envelope {
        public final Header header;
        public final String payloadJson;
        public final long recvUnixMs;
        public Envelope(Header h, String p, long t) {
            this.header = h; this.payloadJson = p; this.recvUnixMs = t;
        }
    }

    public static final class IVXCreateMatchRequest {
        public final String templateId;
        public final String gameId;
        public final String region;
        public final Map<String, Object> templateInit;
        public IVXCreateMatchRequest(String tid, String gid, String region, Map<String, Object> init) {
            this.templateId = tid;
            this.gameId = gid == null ? "" : gid;
            this.region = region == null ? "" : region;
            this.templateInit = init == null ? Collections.emptyMap() : init;
        }
    }

    public static final class IVXCreateMatchResponse {
        public final String matchId;
        public final String templateId;
        public final String region;
        public final long expiresUnixMs;
        public IVXCreateMatchResponse(String mid, String tid, String region, long exp) {
            this.matchId = mid == null ? "" : mid;
            this.templateId = tid == null ? "" : tid;
            this.region = region == null ? "" : region;
            this.expiresUnixMs = exp;
        }
    }

    public interface EnvelopeHandler { void onEnvelope(Envelope env); }
    public interface StateHandler    { void onTransportStateChanged(TransportState s); }

    /** Returned by subscribe(); call dispose() to unbind. Idempotent. */
    public interface Subscription { void dispose(); }

    private static final Gson GSON = new Gson();

    private final Client client;
    private final Session session;
    private SocketClient socket;
    private volatile boolean initialized = false;
    private volatile TransportState transport = TransportState.DISCONNECTED;
    private final ConcurrentHashMap<String, IVXMatchSession> activeSessions = new ConcurrentHashMap<>();
    private final List<StateHandler> stateHandlers = new ArrayList<>();
    private final Object stateLock = new Object();

    public IVXMultiplayerKernel(Client client, Session session) {
        this.client = client;
        this.session = session;
    }

    public TransportState getTransportState() { return transport; }

    public Subscription onTransportStateChanged(StateHandler handler) {
        synchronized (stateLock) { stateHandlers.add(handler); }
        return () -> { synchronized (stateLock) { stateHandlers.remove(handler); } };
    }

    /** Open the realtime socket. Idempotent; subsequent calls return the same future. */
    public CompletableFuture<Void> initialize() {
        if (initialized) return CompletableFuture.completedFuture(null);
        if (client == null || session == null) {
            return failedFuture(new IllegalStateException("client or session is null"));
        }
        socket = client.createSocket();
        // SocketListener wires our message routing into the socket library.
        socket.addListener(new SocketListener() {
            @Override public void onDisconnect(Throwable t) { setState(TransportState.DISCONNECTED); }
            @Override public void onError(Error error)      { /* fanned through onDisconnect */ }
            @Override public void onChannelMessage(com.heroiclabs.nakama.api.ChannelMessage m) { }
            @Override public void onChannelPresence(com.heroiclabs.nakama.api.ChannelPresenceEvent e) { }
            @Override public void onMatchmakerMatched(com.heroiclabs.nakama.api.MatchmakerMatched m) { }
            @Override public void onMatchData(MatchData m)  { dispatchInbound(m); }
            @Override public void onMatchPresence(com.heroiclabs.nakama.api.MatchPresenceEvent e) {
                IVXMatchSession s = activeSessions.get(e.getMatchId());
                if (s != null) s.handlePresence(e);
            }
            @Override public void onNotifications(com.heroiclabs.nakama.api.NotificationList n) { }
            @Override public void onStatusPresence(com.heroiclabs.nakama.api.StatusPresenceEvent e) { }
            @Override public void onStreamData(com.heroiclabs.nakama.api.StreamData d) { }
            @Override public void onStreamPresence(com.heroiclabs.nakama.api.StreamPresenceEvent e) { }
        });
        setState(TransportState.CONNECTING);
        return socket.connect(session, true)
            .thenAccept(v -> { initialized = true; setState(TransportState.CONNECTED); })
            .exceptionally(t -> { setState(TransportState.FAILED_FATAL); return null; });
    }

    public CompletableFuture<Void> shutdown() {
        if (!initialized) return CompletableFuture.completedFuture(null);
        for (IVXMatchSession s : activeSessions.values()) s.dispose();
        activeSessions.clear();
        initialized = false;
        setState(TransportState.DISCONNECTED);
        if (socket != null) {
            try { socket.disconnect(); } catch (Exception ignored) {}
        }
        return CompletableFuture.completedFuture(null);
    }

    public CompletableFuture<IVXCreateMatchResponse> createMatch(IVXCreateMatchRequest req) {
        if (!initialized) return failedFuture(new IllegalStateException("not initialized"));
        JsonObject payload = new JsonObject();
        payload.addProperty("template_id", req.templateId);
        payload.addProperty("game_id",     req.gameId);
        payload.addProperty("region",      req.region);
        payload.add("template_init",       GSON.toJsonTree(req.templateInit));
        return client.rpc(session, "mp_create_match", payload.toString())
            .thenApply(rpc -> {
                String body = rpc.getPayload();
                JsonObject obj = GSON.fromJson(body, JsonObject.class);
                if (obj == null) return new IVXCreateMatchResponse("", "", "", 0L);
                return new IVXCreateMatchResponse(
                    optString(obj, "match_id"),
                    optString(obj, "template_id"),
                    optString(obj, "region"),
                    optLong(obj, "expires_unix_ms"));
            });
    }

    public CompletableFuture<IVXMatchSession> joinMatch(String matchId) {
        if (!initialized || socket == null) return failedFuture(new IllegalStateException("not initialized"));
        IVXMatchSession sess = new IVXMatchSession(this, matchId, session.getUserId());
        return socket.joinMatch(matchId).thenApply((Match m) -> {
            sess.templateId = m.getLabel() == null ? "" : m.getLabel();
            sess.setState(TransportState.CONNECTED);
            activeSessions.put(matchId, sess);
            return sess;
        });
    }

    public CompletableFuture<IVXMatchSession> createAndJoin(IVXCreateMatchRequest req) {
        return createMatch(req).thenCompose(r -> {
            if (r.matchId.isEmpty()) return failedFuture(new RuntimeException("createMatch returned empty matchId"));
            return joinMatch(r.matchId);
        });
    }

    // ---- internals ----

    SocketClient socket() { return socket; }

    private void dispatchInbound(MatchData m) {
        IVXMatchSession s = activeSessions.get(m.getMatchId());
        if (s == null) return;
        String body = new String(m.getData(), StandardCharsets.UTF_8);
        JsonObject obj;
        try { obj = GSON.fromJson(body, JsonObject.class); }
        catch (Exception e) { return; }
        if (obj == null) return;
        JsonObject h = obj.getAsJsonObject("h");
        long seq = h != null && h.has("s") ? h.get("s").getAsLong() : 0;
        long t   = h != null && h.has("t") ? h.get("t").getAsLong() : 0;
        String u = h != null && h.has("u") ? h.get("u").getAsString() : "";
        Header hdr = new Header(seq, t, u, m.getOpCode(),
            m.getPresence() != null ? m.getPresence().getUserId() : "");
        JsonElement p = obj.get("p");
        Envelope env = new Envelope(hdr, p == null ? "" : p.toString(), System.currentTimeMillis());
        s.dispatch(env);
    }

    private void setState(TransportState s) {
        this.transport = s;
        List<StateHandler> snapshot;
        synchronized (stateLock) { snapshot = new ArrayList<>(stateHandlers); }
        for (StateHandler h : snapshot) {
            try { h.onTransportStateChanged(s); } catch (Throwable ignore) {}
        }
    }

    void removeSession(String matchId) { activeSessions.remove(matchId); }

    private static String optString(JsonObject o, String k) {
        return o.has(k) && !o.get(k).isJsonNull() ? o.get(k).getAsString() : "";
    }
    private static long optLong(JsonObject o, String k) {
        return o.has(k) && !o.get(k).isJsonNull() ? o.get(k).getAsLong() : 0L;
    }

    private static <T> CompletableFuture<T> failedFuture(Throwable t) {
        CompletableFuture<T> f = new CompletableFuture<>();
        f.completeExceptionally(t);
        return f;
    }

    /**
     * Live handle for one joined match. Disposing tears down handlers and
     * politely leaves the match. Safe to call multiple times.
     */
    public static final class IVXMatchSession {
        public final String matchId;
        public final String localUserId;
        String templateId = "";
        public volatile long currentMatchTimeMs = 0L;
        public volatile int activePlayerCount = 0;
        private final IVXMultiplayerKernel kernel;
        private final ConcurrentHashMap<Integer, List<EnvelopeHandler>> handlers = new ConcurrentHashMap<>();
        private final List<RangeBinding> ranges = Collections.synchronizedList(new ArrayList<>());
        private final List<StateHandler> stateHandlers = Collections.synchronizedList(new ArrayList<>());
        private final AtomicLong localSeq = new AtomicLong(0);
        private volatile TransportState state = TransportState.CONNECTING;
        private volatile boolean disposed = false;

        IVXMatchSession(IVXMultiplayerKernel k, String mid, String uid) {
            this.kernel = k;
            this.matchId = mid;
            this.localUserId = uid == null ? "" : uid;
        }

        public TransportState getState()         { return state; }
        public String         getTemplateId()    { return templateId; }

        public Subscription subscribe(int opCode, EnvelopeHandler handler) {
            handlers.computeIfAbsent(opCode, k -> Collections.synchronizedList(new ArrayList<>()))
                    .add(handler);
            return () -> {
                List<EnvelopeHandler> list = handlers.get(opCode);
                if (list != null) {
                    list.remove(handler);
                    if (list.isEmpty()) handlers.remove(opCode);
                }
            };
        }

        public Subscription subscribeRange(int from, int to, EnvelopeHandler handler) {
            RangeBinding r = new RangeBinding(from, to, handler);
            ranges.add(r);
            return () -> ranges.remove(r);
        }

        public Subscription onTransportStateChanged(StateHandler h) {
            stateHandlers.add(h);
            return () -> stateHandlers.remove(h);
        }

        public CompletableFuture<Void> send(int opCode, String payloadJson) {
            if (disposed || kernel.socket == null) return CompletableFuture.completedFuture(null);
            long seq = localSeq.incrementAndGet();
            JsonObject env = new JsonObject();
            JsonObject h   = new JsonObject();
            h.addProperty("s", seq);
            h.addProperty("t", currentMatchTimeMs);
            h.addProperty("u", UUID.randomUUID().toString());
            env.add("h", h);
            try {
                JsonElement p = payloadJson == null || payloadJson.isEmpty()
                    ? new JsonObject() : GSON.fromJson(payloadJson, JsonElement.class);
                env.add("p", p);
            } catch (Exception ex) {
                env.addProperty("p", payloadJson);
            }
            byte[] bytes = env.toString().getBytes(StandardCharsets.UTF_8);
            return kernel.socket.sendMatchData(matchId, opCode, bytes);
        }

        public CompletableFuture<Void> leave() {
            if (disposed) return CompletableFuture.completedFuture(null);
            CompletableFuture<Void> f = kernel.socket != null
                ? kernel.socket.leaveMatch(matchId) : CompletableFuture.completedFuture(null);
            return f.whenComplete((v, t) -> dispose());
        }

        public void dispose() {
            if (disposed) return;
            disposed = true;
            handlers.clear();
            ranges.clear();
            stateHandlers.clear();
            setState(TransportState.DISCONNECTED);
            kernel.removeSession(matchId);
        }

        // ---- internals ----

        void dispatch(Envelope env) {
            if (disposed) return;
            currentMatchTimeMs = env.header.matchTimeMs;
            List<EnvelopeHandler> exact = handlers.get(env.header.opCode);
            if (exact != null) {
                for (EnvelopeHandler h : new ArrayList<>(exact)) {
                    try { h.onEnvelope(env); } catch (Throwable ignore) {}
                }
            }
            for (RangeBinding r : new ArrayList<>(ranges)) {
                if (env.header.opCode >= r.from && env.header.opCode <= r.to) {
                    try { r.handler.onEnvelope(env); } catch (Throwable ignore) {}
                }
            }
        }

        void handlePresence(com.heroiclabs.nakama.api.MatchPresenceEvent e) {
            int joined = e.getJoinsCount();
            int left   = e.getLeavesCount();
            activePlayerCount = Math.max(0, activePlayerCount + joined - left);
        }

        void setState(TransportState s) {
            this.state = s;
            for (StateHandler h : new ArrayList<>(stateHandlers)) {
                try { h.onTransportStateChanged(s); } catch (Throwable ignore) {}
            }
        }
    }

    private static final class RangeBinding {
        final int from;
        final int to;
        final EnvelopeHandler handler;
        RangeBinding(int from, int to, EnvelopeHandler h) {
            this.from = from; this.to = to; this.handler = h;
        }
    }
}
