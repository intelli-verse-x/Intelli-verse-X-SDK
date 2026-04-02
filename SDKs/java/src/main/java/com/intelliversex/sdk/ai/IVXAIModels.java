// Copyright (c) 2026 Intelli-verse-X
// MIT License — see LICENSE in the project root.

package com.intelliversex.sdk.ai;

import com.google.gson.annotations.SerializedName;

import java.util.Collections;
import java.util.List;
import java.util.Objects;

/**
 * Data models for the IntelliVerseX AI module.
 * <p>
 * All classes are immutable once constructed by Gson deserialization.
 */
public final class IVXAIModels {

    private IVXAIModels() {}

    /**
     * Response returned when a new AI voice or host session is created.
     */
    public static final class IVXAISessionResponse {
        @SerializedName("session_id")
        private final String sessionId;

        @SerializedName("persona_id")
        private final String personaId;

        @SerializedName("status")
        private final String status;

        @SerializedName("created_at")
        private final long createdAt;

        public IVXAISessionResponse(String sessionId, String personaId, String status, long createdAt) {
            this.sessionId = sessionId != null ? sessionId : "";
            this.personaId = personaId != null ? personaId : "";
            this.status = status != null ? status : "";
            this.createdAt = createdAt;
        }

        /** Unique identifier for this session. */
        public String getSessionId() { return sessionId; }

        /** The persona driving this session. */
        public String getPersonaId() { return personaId; }

        /** Current session status (e.g. "active", "ended"). */
        public String getStatus() { return status; }

        /** Epoch millis when the session was created. */
        public long getCreatedAt() { return createdAt; }

        @Override
        public boolean equals(Object o) {
            if (this == o) return true;
            if (!(o instanceof IVXAISessionResponse)) return false;
            IVXAISessionResponse that = (IVXAISessionResponse) o;
            return createdAt == that.createdAt
                    && Objects.equals(sessionId, that.sessionId)
                    && Objects.equals(personaId, that.personaId)
                    && Objects.equals(status, that.status);
        }

        @Override
        public int hashCode() {
            return Objects.hash(sessionId, personaId, status, createdAt);
        }

        @Override
        public String toString() {
            return "IVXAISessionResponse{sessionId='" + sessionId + "', personaId='" + personaId
                    + "', status='" + status + "', createdAt=" + createdAt + '}';
        }
    }

    /**
     * A single message within an AI session (text or voice transcript).
     */
    public static final class IVXAIMessage {
        @SerializedName("message_id")
        private final String messageId;

        @SerializedName("session_id")
        private final String sessionId;

        @SerializedName("role")
        private final String role;

        @SerializedName("content")
        private final String content;

        @SerializedName("timestamp")
        private final long timestamp;

        public IVXAIMessage(String messageId, String sessionId, String role, String content, long timestamp) {
            this.messageId = messageId != null ? messageId : "";
            this.sessionId = sessionId != null ? sessionId : "";
            this.role = role != null ? role : "";
            this.content = content != null ? content : "";
            this.timestamp = timestamp;
        }

        public String getMessageId() { return messageId; }
        public String getSessionId() { return sessionId; }

        /** "user", "assistant", or "system". */
        public String getRole() { return role; }

        public String getContent() { return content; }
        public long getTimestamp() { return timestamp; }

        @Override
        public boolean equals(Object o) {
            if (this == o) return true;
            if (!(o instanceof IVXAIMessage)) return false;
            IVXAIMessage that = (IVXAIMessage) o;
            return timestamp == that.timestamp
                    && Objects.equals(messageId, that.messageId)
                    && Objects.equals(sessionId, that.sessionId)
                    && Objects.equals(role, that.role)
                    && Objects.equals(content, that.content);
        }

        @Override
        public int hashCode() {
            return Objects.hash(messageId, sessionId, role, content, timestamp);
        }

        @Override
        public String toString() {
            return "IVXAIMessage{messageId='" + messageId + "', role='" + role
                    + "', content='" + content + "', timestamp=" + timestamp + '}';
        }
    }

    /**
     * An AI persona that can drive voice or host sessions.
     */
    public static final class IVXAIPersona {
        @SerializedName("persona_id")
        private final String personaId;

        @SerializedName("name")
        private final String name;

        @SerializedName("description")
        private final String description;

        @SerializedName("voice_id")
        private final String voiceId;

        @SerializedName("tags")
        private final List<String> tags;

        public IVXAIPersona(String personaId, String name, String description,
                            String voiceId, List<String> tags) {
            this.personaId = personaId != null ? personaId : "";
            this.name = name != null ? name : "";
            this.description = description != null ? description : "";
            this.voiceId = voiceId != null ? voiceId : "";
            this.tags = tags != null ? Collections.unmodifiableList(tags) : Collections.emptyList();
        }

        public String getPersonaId() { return personaId; }
        public String getName() { return name; }
        public String getDescription() { return description; }
        public String getVoiceId() { return voiceId; }
        public List<String> getTags() { return tags; }

        @Override
        public boolean equals(Object o) {
            if (this == o) return true;
            if (!(o instanceof IVXAIPersona)) return false;
            IVXAIPersona that = (IVXAIPersona) o;
            return Objects.equals(personaId, that.personaId)
                    && Objects.equals(name, that.name);
        }

        @Override
        public int hashCode() {
            return Objects.hash(personaId, name);
        }

        @Override
        public String toString() {
            return "IVXAIPersona{personaId='" + personaId + "', name='" + name + "'}";
        }
    }

    /**
     * AI entitlement state for a user (credits, tier, etc.).
     */
    public static final class IVXAIEntitlement {
        @SerializedName("user_id")
        private final String userId;

        @SerializedName("tier")
        private final String tier;

        @SerializedName("credits_remaining")
        private final int creditsRemaining;

        @SerializedName("voice_enabled")
        private final boolean voiceEnabled;

        @SerializedName("host_enabled")
        private final boolean hostEnabled;

        public IVXAIEntitlement(String userId, String tier, int creditsRemaining,
                                boolean voiceEnabled, boolean hostEnabled) {
            this.userId = userId != null ? userId : "";
            this.tier = tier != null ? tier : "free";
            this.creditsRemaining = creditsRemaining;
            this.voiceEnabled = voiceEnabled;
            this.hostEnabled = hostEnabled;
        }

        public String getUserId() { return userId; }
        public String getTier() { return tier; }
        public int getCreditsRemaining() { return creditsRemaining; }
        public boolean isVoiceEnabled() { return voiceEnabled; }
        public boolean isHostEnabled() { return hostEnabled; }

        @Override
        public boolean equals(Object o) {
            if (this == o) return true;
            if (!(o instanceof IVXAIEntitlement)) return false;
            IVXAIEntitlement that = (IVXAIEntitlement) o;
            return creditsRemaining == that.creditsRemaining
                    && voiceEnabled == that.voiceEnabled
                    && hostEnabled == that.hostEnabled
                    && Objects.equals(userId, that.userId)
                    && Objects.equals(tier, that.tier);
        }

        @Override
        public int hashCode() {
            return Objects.hash(userId, tier, creditsRemaining, voiceEnabled, hostEnabled);
        }

        @Override
        public String toString() {
            return "IVXAIEntitlement{userId='" + userId + "', tier='" + tier
                    + "', credits=" + creditsRemaining + ", voice=" + voiceEnabled
                    + ", host=" + hostEnabled + '}';
        }
    }

    /**
     * Profile configuration for an AI host session.
     */
    public static final class IVXHostProfile {
        @SerializedName("persona_id")
        private final String personaId;

        @SerializedName("style")
        private final String style;

        @SerializedName("language")
        private final String language;

        @SerializedName("difficulty")
        private final String difficulty;

        public IVXHostProfile(String personaId, String style, String language, String difficulty) {
            this.personaId = personaId != null ? personaId : "";
            this.style = style != null ? style : "default";
            this.language = language != null ? language : "en";
            this.difficulty = difficulty != null ? difficulty : "medium";
        }

        public String getPersonaId() { return personaId; }
        public String getStyle() { return style; }
        public String getLanguage() { return language; }
        public String getDifficulty() { return difficulty; }

        @Override
        public boolean equals(Object o) {
            if (this == o) return true;
            if (!(o instanceof IVXHostProfile)) return false;
            IVXHostProfile that = (IVXHostProfile) o;
            return Objects.equals(personaId, that.personaId)
                    && Objects.equals(style, that.style)
                    && Objects.equals(language, that.language)
                    && Objects.equals(difficulty, that.difficulty);
        }

        @Override
        public int hashCode() {
            return Objects.hash(personaId, style, language, difficulty);
        }

        @Override
        public String toString() {
            return "IVXHostProfile{personaId='" + personaId + "', style='" + style
                    + "', language='" + language + "', difficulty='" + difficulty + "'}";
        }
    }
}
