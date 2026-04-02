// Copyright (c) 2026 Intelli-verse-X
// MIT License — see LICENSE in the project root.

package com.intelliversex.sdk.discord;

import com.google.gson.annotations.SerializedName;

import java.util.Collections;
import java.util.List;
import java.util.Map;
import java.util.Objects;

/**
 * Data models for the IntelliVerseX Discord Social module.
 * <p>
 * All classes are immutable once constructed by Gson deserialization.
 */
public final class IVXDiscordModels {

    private IVXDiscordModels() {}

    /**
     * Configuration for the Discord Social SDK integration.
     */
    public static final class IVXDiscordConfig {
        @SerializedName("application_id")
        private final String applicationId;

        @SerializedName("client_id")
        private final String clientId;

        @SerializedName("redirect_uri")
        private final String redirectUri;

        @SerializedName("enable_debug_logs")
        private final boolean enableDebugLogs;

        public IVXDiscordConfig(String applicationId, String clientId, String redirectUri, boolean enableDebugLogs) {
            this.applicationId = applicationId != null ? applicationId : "";
            this.clientId = clientId != null ? clientId : "";
            this.redirectUri = redirectUri;
            this.enableDebugLogs = enableDebugLogs;
        }

        /** The Discord application ID. */
        public String getApplicationId() { return applicationId; }

        /** The Discord OAuth client ID. */
        public String getClientId() { return clientId; }

        /** Optional redirect URI for OAuth flows. */
        public String getRedirectUri() { return redirectUri; }

        /** Whether debug logging is enabled. */
        public boolean isEnableDebugLogs() { return enableDebugLogs; }

        @Override
        public boolean equals(Object o) {
            if (this == o) return true;
            if (!(o instanceof IVXDiscordConfig)) return false;
            IVXDiscordConfig that = (IVXDiscordConfig) o;
            return enableDebugLogs == that.enableDebugLogs
                    && Objects.equals(applicationId, that.applicationId)
                    && Objects.equals(clientId, that.clientId)
                    && Objects.equals(redirectUri, that.redirectUri);
        }

        @Override
        public int hashCode() {
            return Objects.hash(applicationId, clientId, redirectUri, enableDebugLogs);
        }

        @Override
        public String toString() {
            return "IVXDiscordConfig{applicationId='" + applicationId + "', clientId='" + clientId + "'}";
        }
    }

    /**
     * A friend entry from the unified (Discord + game) friends list.
     */
    public static final class IVXUnifiedFriend {
        @SerializedName("user_id")
        private final String userId;

        @SerializedName("discord_id")
        private final String discordId;

        @SerializedName("username")
        private final String username;

        @SerializedName("display_name")
        private final String displayName;

        @SerializedName("avatar_url")
        private final String avatarUrl;

        @SerializedName("source")
        private final String source;

        @SerializedName("status")
        private final String status;

        public IVXUnifiedFriend(String userId, String discordId, String username,
                                String displayName, String avatarUrl, String source, String status) {
            this.userId = userId != null ? userId : "";
            this.discordId = discordId;
            this.username = username != null ? username : "";
            this.displayName = displayName != null ? displayName : "";
            this.avatarUrl = avatarUrl != null ? avatarUrl : "";
            this.source = source != null ? source : "game";
            this.status = status != null ? status : "offline";
        }

        public String getUserId() { return userId; }
        public String getDiscordId() { return discordId; }
        public String getUsername() { return username; }
        public String getDisplayName() { return displayName; }
        public String getAvatarUrl() { return avatarUrl; }

        /** "discord", "game", or "both". */
        public String getSource() { return source; }

        /** "online", "idle", "dnd", or "offline". */
        public String getStatus() { return status; }

        @Override
        public boolean equals(Object o) {
            if (this == o) return true;
            if (!(o instanceof IVXUnifiedFriend)) return false;
            IVXUnifiedFriend that = (IVXUnifiedFriend) o;
            return Objects.equals(userId, that.userId)
                    && Objects.equals(discordId, that.discordId)
                    && Objects.equals(username, that.username);
        }

        @Override
        public int hashCode() {
            return Objects.hash(userId, discordId, username);
        }

        @Override
        public String toString() {
            return "IVXUnifiedFriend{userId='" + userId + "', username='" + username
                    + "', source='" + source + "', status='" + status + "'}";
        }
    }

    /**
     * An incoming game invite from another player.
     */
    public static final class IVXGameInvite {
        @SerializedName("invite_id")
        private final String inviteId;

        @SerializedName("sender_id")
        private final String senderId;

        @SerializedName("sender_name")
        private final String senderName;

        @SerializedName("message")
        private final String message;

        @SerializedName("lobby_id")
        private final String lobbyId;

        @SerializedName("timestamp")
        private final long timestamp;

        public IVXGameInvite(String inviteId, String senderId, String senderName,
                             String message, String lobbyId, long timestamp) {
            this.inviteId = inviteId != null ? inviteId : "";
            this.senderId = senderId != null ? senderId : "";
            this.senderName = senderName != null ? senderName : "";
            this.message = message;
            this.lobbyId = lobbyId;
            this.timestamp = timestamp;
        }

        public String getInviteId() { return inviteId; }
        public String getSenderId() { return senderId; }
        public String getSenderName() { return senderName; }
        public String getMessage() { return message; }
        public String getLobbyId() { return lobbyId; }
        public long getTimestamp() { return timestamp; }

        @Override
        public boolean equals(Object o) {
            if (this == o) return true;
            if (!(o instanceof IVXGameInvite)) return false;
            IVXGameInvite that = (IVXGameInvite) o;
            return timestamp == that.timestamp && Objects.equals(inviteId, that.inviteId);
        }

        @Override
        public int hashCode() {
            return Objects.hash(inviteId, timestamp);
        }

        @Override
        public String toString() {
            return "IVXGameInvite{inviteId='" + inviteId + "', senderId='" + senderId
                    + "', senderName='" + senderName + "'}";
        }
    }

    /**
     * Information about a Discord lobby.
     */
    public static final class IVXDiscordLobbyInfo {
        @SerializedName("lobby_id")
        private final String lobbyId;

        @SerializedName("secret")
        private final String secret;

        @SerializedName("owner_id")
        private final String ownerId;

        @SerializedName("member_count")
        private final int memberCount;

        @SerializedName("max_members")
        private final int maxMembers;

        @SerializedName("metadata")
        private final Map<String, String> metadata;

        public IVXDiscordLobbyInfo(String lobbyId, String secret, String ownerId,
                                   int memberCount, int maxMembers, Map<String, String> metadata) {
            this.lobbyId = lobbyId != null ? lobbyId : "";
            this.secret = secret != null ? secret : "";
            this.ownerId = ownerId != null ? ownerId : "";
            this.memberCount = memberCount;
            this.maxMembers = maxMembers;
            this.metadata = metadata != null ? Collections.unmodifiableMap(metadata) : Collections.emptyMap();
        }

        public String getLobbyId() { return lobbyId; }
        public String getSecret() { return secret; }
        public String getOwnerId() { return ownerId; }
        public int getMemberCount() { return memberCount; }
        public int getMaxMembers() { return maxMembers; }
        public Map<String, String> getMetadata() { return metadata; }

        @Override
        public boolean equals(Object o) {
            if (this == o) return true;
            if (!(o instanceof IVXDiscordLobbyInfo)) return false;
            IVXDiscordLobbyInfo that = (IVXDiscordLobbyInfo) o;
            return Objects.equals(lobbyId, that.lobbyId) && Objects.equals(secret, that.secret);
        }

        @Override
        public int hashCode() {
            return Objects.hash(lobbyId, secret);
        }

        @Override
        public String toString() {
            return "IVXDiscordLobbyInfo{lobbyId='" + lobbyId + "', members=" + memberCount
                    + "/" + maxMembers + '}';
        }
    }

    /**
     * A participant in a Discord voice call.
     */
    public static final class IVXVoiceParticipant {
        @SerializedName("user_id")
        private final String userId;

        @SerializedName("username")
        private final String username;

        @SerializedName("is_muted")
        private final boolean isMuted;

        @SerializedName("is_deafened")
        private final boolean isDeafened;

        @SerializedName("is_speaking")
        private final boolean isSpeaking;

        @SerializedName("volume")
        private final int volume;

        public IVXVoiceParticipant(String userId, String username, boolean isMuted,
                                   boolean isDeafened, boolean isSpeaking, int volume) {
            this.userId = userId != null ? userId : "";
            this.username = username != null ? username : "";
            this.isMuted = isMuted;
            this.isDeafened = isDeafened;
            this.isSpeaking = isSpeaking;
            this.volume = volume;
        }

        public String getUserId() { return userId; }
        public String getUsername() { return username; }
        public boolean isMuted() { return isMuted; }
        public boolean isDeafened() { return isDeafened; }
        public boolean isSpeaking() { return isSpeaking; }
        public int getVolume() { return volume; }

        @Override
        public boolean equals(Object o) {
            if (this == o) return true;
            if (!(o instanceof IVXVoiceParticipant)) return false;
            IVXVoiceParticipant that = (IVXVoiceParticipant) o;
            return Objects.equals(userId, that.userId);
        }

        @Override
        public int hashCode() {
            return Objects.hash(userId);
        }

        @Override
        public String toString() {
            return "IVXVoiceParticipant{userId='" + userId + "', username='" + username
                    + "', muted=" + isMuted + ", deafened=" + isDeafened + '}';
        }
    }
}
