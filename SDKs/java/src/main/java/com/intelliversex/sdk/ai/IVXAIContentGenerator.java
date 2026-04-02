// Copyright (c) 2026 Intelli-verse-X
// MIT License — see LICENSE in the project root.

package com.intelliversex.sdk.ai;

import java.util.Map;
import java.util.concurrent.CompletableFuture;

/**
 * Procedural content generation (Unity {@code IVXAIContentGenerator}).
 */
public final class IVXAIContentGenerator {

    private static final IVXAIContentGenerator INSTANCE = new IVXAIContentGenerator();

    private IVXAIContentGenerator() {}

    public static IVXAIContentGenerator getInstance() {
        return INSTANCE;
    }

    public boolean isGenerating() {
        return false;
    }

    public void initialize(Object config) {
        throw new UnsupportedOperationException("Not yet implemented — stub only");
    }

    public CompletableFuture<IVXGeneratedQuest> generateQuest(
            IVXQuestTemplate template, String playerContext) {
        return failed();
    }

    public CompletableFuture<IVXGeneratedStory> generateStory(
            String prompt, String genre, int maxWords) {
        return failed();
    }

    public CompletableFuture<IVXGeneratedItem> generateItemDescription(
            String itemName, String itemType, String rarity) {
        return failed();
    }

    public CompletableFuture<IVXGeneratedDialogue> generateDialogue(
            String scenario, String[] characters) {
        return failed();
    }

    public CompletableFuture<String> generateFromTemplate(
            String template, Map<String, String> variables) {
        return failed();
    }

    public void cancelGeneration() {
        throw new UnsupportedOperationException("Not yet implemented — stub only");
    }

    private static <T> CompletableFuture<T> failed() {
        CompletableFuture<T> f = new CompletableFuture<>();
        f.completeExceptionally(new UnsupportedOperationException("Not yet implemented — stub only"));
        return f;
    }

    public static final class IVXQuestTemplate {
        public String genre;
        public String difficulty;
        public String[] requiredElements;
        public int estimatedDurationMinutes;
        public String customPrompt;
    }

    public static final class IVXGeneratedQuest {
        public String title;
        public String description;
    }

    public static final class IVXGeneratedStory {
        public String title;
        public String body;
    }

    public static final class IVXGeneratedItem {
        public String name;
        public String description;
    }

    public static final class IVXGeneratedDialogue {
        public String rawJson;
    }
}
