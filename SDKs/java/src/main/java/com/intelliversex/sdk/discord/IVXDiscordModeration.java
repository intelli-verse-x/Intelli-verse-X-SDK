// Copyright (c) 2026 Intelli-verse-X
// MIT License — see LICENSE in the project root.

package com.intelliversex.sdk.discord;

import java.util.Map;
import java.util.concurrent.CompletableFuture;

/**
 * Discord moderation metadata, voice capture for moderation, user reporting.
 * Stub API matching Unity {@code IVXDiscordModeration}.
 */
public final class IVXDiscordModeration {

    private static final IVXDiscordModeration INSTANCE = new IVXDiscordModeration();

    private volatile boolean autoModerateEnabled = true;

    private IVXDiscordModeration() {}

    public static IVXDiscordModeration getInstance() {
        return INSTANCE;
    }

    public boolean isAutoModerateEnabled() {
        return autoModerateEnabled;
    }

    public void setAutoModerateEnabled(boolean autoModerateEnabled) {
        this.autoModerateEnabled = autoModerateEnabled;
    }

    public void enableAutoModeration(boolean enable) {
        throw new UnsupportedOperationException("Not implemented");
    }

    public void processModerationMetadata(String messageId, Map<String, String> metadata) {
        throw new UnsupportedOperationException("Not implemented");
    }

    public static IVXModerationDecision getModerationAction(Map<String, String> metadata) {
        throw new UnsupportedOperationException("Not implemented");
    }

    public void startVoiceModerationCapture(String lobbyId) {
        throw new UnsupportedOperationException("Not implemented");
    }

    public void stopVoiceModerationCapture() {
        throw new UnsupportedOperationException("Not implemented");
    }

    public CompletableFuture<Boolean> reportUser(String userId, String reason) {
        CompletableFuture<Boolean> f = new CompletableFuture<>();
        f.completeExceptionally(new UnsupportedOperationException("Not implemented"));
        return f;
    }

    public enum IVXModerationAction {
        Show,
        Hide,
        Blur,
        Replace
    }

    public static final class IVXModerationDecision {
        public String messageId;
        public IVXModerationAction action;
        public String reason;
        public String replacement;
        public String severity;
    }
}
