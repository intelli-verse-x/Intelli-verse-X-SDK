// Copyright (c) 2026 Intelli-verse-X
// MIT License — see LICENSE in the project root.

package com.intelliversex.sdk.ai;

import java.util.List;
import java.util.concurrent.CompletableFuture;

/**
 * NPC profile registration and dialog sessions (Unity {@code IVXAINPCDialogManager}).
 */
public final class IVXAINPCDialogManager {

    private static final IVXAINPCDialogManager INSTANCE = new IVXAINPCDialogManager();

    private IVXAINPCDialogManager() {}

    public static IVXAINPCDialogManager getInstance() {
        return INSTANCE;
    }

    public boolean isInitialized() {
        return false;
    }

    public void initialize(Object config) {
        throw new UnsupportedOperationException("Not implemented");
    }

    public void setAuthToken(String token) {
        throw new UnsupportedOperationException("Not implemented");
    }

    public void registerNPC(IVXAINPCProfile profile) {
        throw new UnsupportedOperationException("Not implemented");
    }

    public void unregisterNPC(String npcId) {
        throw new UnsupportedOperationException("Not implemented");
    }

    public CompletableFuture<IVXAINPCDialogSession> startDialog(
            String npcId, String playerId, String playerContext) {
        return failed();
    }

    public CompletableFuture<String> sendMessage(String sessionId, String message) {
        return failed();
    }

    public CompletableFuture<Void> endDialog(String sessionId) {
        return failed();
    }

    public IVXAINPCDialogSession getSession(String sessionId) {
        throw new UnsupportedOperationException("Not implemented");
    }

    public List<IVXAINPCDialogSession> getSessionsForNPC(String npcId) {
        throw new UnsupportedOperationException("Not implemented");
    }

    private static <T> CompletableFuture<T> failed() {
        CompletableFuture<T> f = new CompletableFuture<>();
        f.completeExceptionally(new UnsupportedOperationException("Not implemented"));
        return f;
    }

    public static final class IVXAINPCProfile {
        public String npcId;
        public int maxTurns;
    }

    public static final class IVXAINPCDialogSession {
        public String sessionId;
        public String npcId;
        public String playerId;
    }
}
