// Copyright (c) 2026 Intelli-verse-X
// MIT License — see LICENSE in the project root.

package com.intelliversex.sdk.discord;

import java.util.List;
import java.util.Map;
import java.util.concurrent.CompletableFuture;

/**
 * Discord Social SDK direct messages: send, edit, history, summaries, visibility, deep links.
 * Stub API matching Unity {@code IVXDiscordMessages}.
 */
public final class IVXDiscordMessages {

    private static final IVXDiscordMessages INSTANCE = new IVXDiscordMessages();

    private IVXDiscordMessages() {}

    public static IVXDiscordMessages getInstance() {
        return INSTANCE;
    }

    public boolean isShowingChat() {
        return false;
    }

    /**
     * @return new message snowflake id on success
     */
    public CompletableFuture<String> sendDM(String recipientId, String message) {
        return failed();
    }

    public CompletableFuture<Void> editDM(String recipientId, String messageId, String newContent) {
        return failed();
    }

    public CompletableFuture<List<IVXDirectMessage>> getDMHistory(String recipientId, int limit) {
        return failed();
    }

    public CompletableFuture<List<IVXDMSummary>> getDMSummaries() {
        return failed();
    }

    public void setShowingChat(boolean showing) {
        throw new UnsupportedOperationException("Not implemented");
    }

    public void openMessageInDiscord(String messageId) {
        throw new UnsupportedOperationException("Not implemented");
    }

    public void openDMSettingsInDiscord() {
        throw new UnsupportedOperationException("Not implemented");
    }

    private static <T> CompletableFuture<T> failed() {
        CompletableFuture<T> f = new CompletableFuture<>();
        f.completeExceptionally(new UnsupportedOperationException("Not implemented"));
        return f;
    }

    /** DM line item (Unity IVXDirectMessage). */
    public static final class IVXDirectMessage {
        public String messageId;
        public String authorId;
        public String authorName;
        public String content;
        public long timestamp;
        public boolean isDisclosure;
        public boolean hasAdditionalContent;
        public String additionalContentDescription;
        public Map<String, String> moderationMetadata;
    }

    /** Conversation summary row (Unity IVXDMSummary). */
    public static final class IVXDMSummary {
        public String userId;
        public String displayName;
        public String lastMessageId;
        public long lastMessageTimestamp;
    }
}
