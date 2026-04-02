// Copyright (c) 2026 Intelli-verse-X — MIT License
#pragma once

#include "cocos2d.h"

namespace ivx {

/**
 * Discord Social Settings — notification preferences, privacy, DND mode.
 * Stub: API shape matches Unity IVXDiscordSettings.
 */
class IVXDiscordSettings : public cocos2d::Ref {
public:
    CREATE_FUNC(IVXDiscordSettings);

    virtual bool init() { return true; }

    bool isNotificationsEnabled() const { return _notificationsEnabled; }
    void setNotificationsEnabled(bool v) { _notificationsEnabled = v; }

    bool isFriendRequestsEnabled() const { return _friendRequestsEnabled; }
    void setFriendRequestsEnabled(bool v) { _friendRequestsEnabled = v; }

    bool isDoNotDisturb() const { return _doNotDisturb; }
    void setDoNotDisturb(bool v) { _doNotDisturb = v; }
    void enableDoNotDisturb() { _doNotDisturb = true; }
    void disableDoNotDisturb() { _doNotDisturb = false; }

    bool isShowOnlineStatus() const { return _showOnlineStatus; }
    void setShowOnlineStatus(bool v) { _showOnlineStatus = v; }

    bool isAllowDirectMessages() const { return _allowDirectMessages; }
    void setAllowDirectMessages(bool v) { _allowDirectMessages = v; }

    void resetToDefaults();

private:
    bool _notificationsEnabled = true;
    bool _friendRequestsEnabled = true;
    bool _doNotDisturb = false;
    bool _showOnlineStatus = true;
    bool _allowDirectMessages = true;
};

} // namespace ivx
