// Copyright (c) 2026 Intelli-verse-X
// MIT License — see LICENSE in the project root.

package com.intelliversex.sdk.discord;

/**
 * Discord Social Settings — notification preferences, privacy, DND mode.
 * <p>
 * Stub: API shape matches Unity {@code IVXDiscordSettings} for zero-code-change upgrade.
 */
public class IVXDiscordSettings {

    private boolean notificationsEnabled = true;
    private boolean friendRequestsEnabled = true;
    private boolean doNotDisturb = false;
    private boolean showOnlineStatus = true;
    private boolean allowDirectMessages = true;

    public boolean isNotificationsEnabled() { return notificationsEnabled; }
    public void setNotificationsEnabled(boolean v) { notificationsEnabled = v; }

    public boolean isFriendRequestsEnabled() { return friendRequestsEnabled; }
    public void setFriendRequestsEnabled(boolean v) { friendRequestsEnabled = v; }

    public boolean isDoNotDisturb() { return doNotDisturb; }
    public void setDoNotDisturb(boolean v) { doNotDisturb = v; }
    public void enableDoNotDisturb() { doNotDisturb = true; }
    public void disableDoNotDisturb() { doNotDisturb = false; }

    public boolean isShowOnlineStatus() { return showOnlineStatus; }
    public void setShowOnlineStatus(boolean v) { showOnlineStatus = v; }

    public boolean isAllowDirectMessages() { return allowDirectMessages; }
    public void setAllowDirectMessages(boolean v) { allowDirectMessages = v; }

    /** Reset all settings to defaults. */
    public void resetToDefaults() {
        notificationsEnabled = true;
        friendRequestsEnabled = true;
        doNotDisturb = false;
        showOnlineStatus = true;
        allowDirectMessages = true;
    }
}
