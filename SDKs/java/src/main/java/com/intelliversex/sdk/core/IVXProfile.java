package com.intelliversex.sdk.core;

import java.util.Objects;

/**
 * Immutable model representing a player profile fetched from the server.
 */
public final class IVXProfile {
    private final String userId;
    private final String username;
    private final String displayName;
    private final String avatarUrl;
    private final String langTag;
    private final String metadata;
    private final String wallet;

    public IVXProfile(String userId,
                      String username,
                      String displayName,
                      String avatarUrl,
                      String langTag,
                      String metadata,
                      String wallet) {
        this.userId      = userId      != null ? userId      : "";
        this.username    = username    != null ? username    : "";
        this.displayName = displayName != null ? displayName : "";
        this.avatarUrl   = avatarUrl   != null ? avatarUrl   : "";
        this.langTag     = langTag     != null ? langTag     : "";
        this.metadata    = metadata    != null ? metadata    : "";
        this.wallet      = wallet      != null ? wallet      : "";
    }

    public String getUserId()      { return userId; }
    public String getUsername()    { return username; }
    public String getDisplayName() { return displayName; }
    public String getAvatarUrl()   { return avatarUrl; }
    public String getLangTag()     { return langTag; }
    public String getMetadata()    { return metadata; }
    public String getWallet()      { return wallet; }

    @Override
    public boolean equals(Object o) {
        if (this == o) return true;
        if (!(o instanceof IVXProfile)) return false;
        IVXProfile that = (IVXProfile) o;
        return Objects.equals(userId, that.userId)
                && Objects.equals(username, that.username)
                && Objects.equals(displayName, that.displayName)
                && Objects.equals(avatarUrl, that.avatarUrl)
                && Objects.equals(langTag, that.langTag)
                && Objects.equals(metadata, that.metadata)
                && Objects.equals(wallet, that.wallet);
    }

    @Override
    public int hashCode() {
        return Objects.hash(userId, username, displayName, avatarUrl, langTag, metadata, wallet);
    }

    @Override
    public String toString() {
        return "IVXProfile{"
                + "userId='" + userId + '\''
                + ", username='" + username + '\''
                + ", displayName='" + displayName + '\''
                + ", avatarUrl='" + avatarUrl + '\''
                + ", langTag='" + langTag + '\''
                + ", metadata='" + metadata + '\''
                + ", wallet='" + wallet + '\''
                + '}';
    }
}
