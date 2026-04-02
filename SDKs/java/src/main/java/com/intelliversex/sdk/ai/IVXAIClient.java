// Copyright (c) 2026 Intelli-verse-X
// MIT License — see LICENSE in the project root.

package com.intelliversex.sdk.ai;

import com.google.gson.Gson;
import com.google.gson.JsonObject;
import com.google.gson.reflect.TypeToken;
import com.intelliversex.sdk.ai.IVXAIModels.*;

import java.lang.reflect.Type;
import java.net.URI;
import java.net.http.HttpClient;
import java.net.http.HttpRequest;
import java.net.http.HttpResponse;
import java.time.Duration;
import java.util.List;
import java.util.Objects;
import java.util.concurrent.CompletableFuture;

/**
 * Thread-safe singleton client for the IntelliVerseX AI service.
 * <p>
 * Provides voice sessions, AI-host sessions, text messaging, entitlement
 * checks, and persona listing. All network calls return {@link CompletableFuture}
 * and are non-blocking.
 * <p>
 * <b>Usage:</b>
 * <pre>{@code
 * IVXAIClient.getInstance().initialize("https://ai.intelli-verse-x.ai", "my-api-key");
 * IVXAIClient.getInstance().getPersonas().thenAccept(personas -> { ... });
 * }</pre>
 */
public class IVXAIClient {

    private static final Duration DEFAULT_TIMEOUT = Duration.ofSeconds(30);

    private static volatile IVXAIClient instance;

    private volatile boolean initialized;
    private String apiBaseUrl;
    private String apiKey;
    private HttpClient httpClient;
    private final Gson gson = new Gson();

    private IVXAIClient() {}

    /**
     * Returns the singleton instance, creating it on first access (double-checked locking).
     *
     * @return the shared {@link IVXAIClient} instance
     */
    public static IVXAIClient getInstance() {
        if (instance == null) {
            synchronized (IVXAIClient.class) {
                if (instance == null) {
                    instance = new IVXAIClient();
                }
            }
        }
        return instance;
    }

    /**
     * Initializes the AI client with the given service URL and API key.
     * Must be called before any other method.
     *
     * @param apiBaseUrl base URL of the AI service (e.g. {@code "https://ai.intelli-verse-x.ai"})
     * @param apiKey     bearer token for authentication
     * @throws IllegalArgumentException if either argument is null or blank
     */
    public synchronized void initialize(String apiBaseUrl, String apiKey) {
        if (apiBaseUrl == null || apiBaseUrl.trim().isEmpty()) {
            throw new IllegalArgumentException("apiBaseUrl must not be null or empty");
        }
        if (apiKey == null || apiKey.trim().isEmpty()) {
            throw new IllegalArgumentException("apiKey must not be null or empty");
        }
        this.apiBaseUrl = apiBaseUrl.endsWith("/")
                ? apiBaseUrl.substring(0, apiBaseUrl.length() - 1)
                : apiBaseUrl;
        this.apiKey = apiKey;
        this.httpClient = HttpClient.newBuilder()
                .connectTimeout(DEFAULT_TIMEOUT)
                .build();
        this.initialized = true;
    }

    // ──────────────────────────────────────────────
    //  Voice Sessions
    // ──────────────────────────────────────────────

    /**
     * Starts a new AI voice session for the given persona and user.
     *
     * @param personaId the persona to use for the session
     * @param userId    the user starting the session
     * @return a future resolving to the session details
     */
    public CompletableFuture<IVXAISessionResponse> startVoiceSession(String personaId, String userId) {
        requireInitialized();
        JsonObject body = new JsonObject();
        body.addProperty("persona_id", personaId);
        body.addProperty("user_id", userId);
        return post("/v1/voice/sessions", body, IVXAISessionResponse.class);
    }

    /**
     * Ends an active voice session.
     *
     * @param sessionId the session to end
     * @return a future that completes when the server acknowledges the request
     */
    public CompletableFuture<Void> endVoiceSession(String sessionId) {
        requireInitialized();
        return post("/v1/voice/sessions/" + sessionId + "/end", new JsonObject(), Void.class)
                .thenApply(v -> null);
    }

    // ──────────────────────────────────────────────
    //  Text Messaging
    // ──────────────────────────────────────────────

    /**
     * Sends a text message within an active session.
     *
     * @param sessionId the target session
     * @param text      the message content
     * @return a future that completes when the message is delivered
     */
    public CompletableFuture<Void> sendText(String sessionId, String text) {
        requireInitialized();
        JsonObject body = new JsonObject();
        body.addProperty("text", text);
        return post("/v1/sessions/" + sessionId + "/messages", body, Void.class)
                .thenApply(v -> null);
    }

    /**
     * Polls for new messages in the given session.
     *
     * @param sessionId the session to poll
     * @return a future resolving to the list of messages since last poll
     */
    public CompletableFuture<List<IVXAIMessage>> pollMessages(String sessionId) {
        requireInitialized();
        Type listType = new TypeToken<List<IVXAIMessage>>() {}.getType();
        return get("/v1/sessions/" + sessionId + "/messages", listType);
    }

    // ──────────────────────────────────────────────
    //  AI Host Sessions
    // ──────────────────────────────────────────────

    /**
     * Starts an AI-host session for a match.
     *
     * @param matchId the match this host will drive
     * @param profile host configuration (persona, style, language, difficulty)
     * @return a future resolving to the session details
     */
    public CompletableFuture<IVXAISessionResponse> startHostSession(String matchId, IVXHostProfile profile) {
        requireInitialized();
        Objects.requireNonNull(profile, "profile must not be null");
        JsonObject body = new JsonObject();
        body.addProperty("match_id", matchId);
        body.add("profile", gson.toJsonTree(profile));
        return post("/v1/host/sessions", body, IVXAISessionResponse.class);
    }

    /**
     * Sends a game event to an active host session.
     *
     * @param sessionId the host session
     * @param eventType type of event (e.g. "answer_submitted", "round_start")
     * @param data      JSON-encoded event payload
     * @return a future that completes when the server acknowledges the event
     */
    public CompletableFuture<Void> sendHostEvent(String sessionId, String eventType, String data) {
        requireInitialized();
        JsonObject body = new JsonObject();
        body.addProperty("event_type", eventType);
        body.addProperty("data", data);
        return post("/v1/host/sessions/" + sessionId + "/events", body, Void.class)
                .thenApply(v -> null);
    }

    // ──────────────────────────────────────────────
    //  Entitlements & Personas
    // ──────────────────────────────────────────────

    /**
     * Checks AI entitlement (credits, tier, feature flags) for a user.
     *
     * @param userId the user to check
     * @return a future resolving to the entitlement details
     */
    public CompletableFuture<IVXAIEntitlement> checkEntitlement(String userId) {
        requireInitialized();
        return get("/v1/entitlements/" + userId, IVXAIEntitlement.class);
    }

    /**
     * Lists all available AI personas.
     *
     * @return a future resolving to the persona catalogue
     */
    public CompletableFuture<List<IVXAIPersona>> getPersonas() {
        requireInitialized();
        Type listType = new TypeToken<List<IVXAIPersona>>() {}.getType();
        return get("/v1/personas", listType);
    }

    // ──────────────────────────────────────────────
    //  Internal HTTP helpers
    // ──────────────────────────────────────────────

    private <T> CompletableFuture<T> post(String path, JsonObject body, Class<T> responseType) {
        HttpRequest request = HttpRequest.newBuilder()
                .uri(URI.create(apiBaseUrl + path))
                .timeout(DEFAULT_TIMEOUT)
                .header("Content-Type", "application/json")
                .header("Authorization", "Bearer " + apiKey)
                .POST(HttpRequest.BodyPublishers.ofString(gson.toJson(body)))
                .build();
        return httpClient.sendAsync(request, HttpResponse.BodyHandlers.ofString())
                .thenApply(resp -> parseResponse(resp, responseType));
    }

    @SuppressWarnings("unchecked")
    private <T> CompletableFuture<T> get(String path, Type responseType) {
        HttpRequest request = HttpRequest.newBuilder()
                .uri(URI.create(apiBaseUrl + path))
                .timeout(DEFAULT_TIMEOUT)
                .header("Authorization", "Bearer " + apiKey)
                .GET()
                .build();
        return httpClient.sendAsync(request, HttpResponse.BodyHandlers.ofString())
                .thenApply(resp -> {
                    if (resp.statusCode() < 200 || resp.statusCode() >= 300) {
                        throw new RuntimeException("AI API error " + resp.statusCode() + ": " + resp.body());
                    }
                    return (T) gson.fromJson(resp.body(), responseType);
                });
    }

    private <T> T parseResponse(HttpResponse<String> resp, Class<T> type) {
        if (resp.statusCode() < 200 || resp.statusCode() >= 300) {
            throw new RuntimeException("AI API error " + resp.statusCode() + ": " + resp.body());
        }
        if (type == Void.class) {
            return null;
        }
        return gson.fromJson(resp.body(), type);
    }

    private void requireInitialized() {
        if (!initialized) {
            throw new IllegalStateException("IVXAIClient has not been initialized. Call initialize() first.");
        }
    }
}
