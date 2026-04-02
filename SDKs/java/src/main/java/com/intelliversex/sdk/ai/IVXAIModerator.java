// Copyright (c) 2026 Intelli-verse-X
// MIT License — see LICENSE in the project root.

package com.intelliversex.sdk.ai;

import java.util.List;
import java.util.Map;
import java.util.concurrent.CompletableFuture;

/**
 * Text moderation: classify, filter, batch, rules (Unity {@code IVXAIModerator}).
 */
public final class IVXAIModerator {

    private static final IVXAIModerator INSTANCE = new IVXAIModerator();

    private IVXAIModerator() {}

    public static IVXAIModerator getInstance() {
        return INSTANCE;
    }

    public boolean isEnabled() {
        return false;
    }

    public void initialize(Object config) {
        throw new UnsupportedOperationException("Not yet implemented — stub only");
    }

    public CompletableFuture<IVXModerationResult> classifyText(String text) {
        return failed();
    }

    public CompletableFuture<String> filterMessage(String text) {
        return failed();
    }

    public CompletableFuture<List<IVXModerationResult>> scanBatch(List<String> messages) {
        return failed();
    }

    public void addCustomRule(IVXModerationRule rule) {
        throw new UnsupportedOperationException("Not yet implemented — stub only");
    }

    public void removeCustomRule(String pattern) {
        throw new UnsupportedOperationException("Not yet implemented — stub only");
    }

    public void setCustomRules(List<IVXModerationRule> rules) {
        throw new UnsupportedOperationException("Not yet implemented — stub only");
    }

    public void clearCustomRules() {
        throw new UnsupportedOperationException("Not yet implemented — stub only");
    }

    public IVXModerationResult checkLocalRules(String text) {
        throw new UnsupportedOperationException("Not yet implemented — stub only");
    }

    public Map<String, String> getDiscordModerationMetadata(IVXModerationResult result) {
        throw new UnsupportedOperationException("Not yet implemented — stub only");
    }

    private static <T> CompletableFuture<T> failed() {
        CompletableFuture<T> f = new CompletableFuture<>();
        f.completeExceptionally(new UnsupportedOperationException("Not yet implemented — stub only"));
        return f;
    }

    public enum IVXContentCategory {
        Clean,
        Toxic,
        Spam,
        PII,
        Harassment,
        HateSpeech,
        SelfHarm,
        Sexual,
        Violence,
        Custom
    }

    public enum IVXModerationSeverity {
        None,
        Low,
        Medium,
        High,
        Critical
    }

    public enum IVXModerationActionType {
        Allow,
        Warn,
        Replace,
        Block,
        Flag
    }

    public static final class IVXModerationResult {
        public IVXContentCategory category;
        public IVXModerationSeverity severity;
        public float confidence;
        public IVXModerationActionType suggestedAction;
        public String replacement;
        public String originalText;
    }

    public static final class IVXModerationRule {
        public String pattern;
        public IVXContentCategory category;
        public IVXModerationActionType action;
        public String replacementText;
    }
}
