// Copyright (c) 2026 Intelli-verse-X — MIT License

#include "IVXDiscordSettings.h"

namespace ivx {

void IVXDiscordSettings::resetToDefaults() {
    _notificationsEnabled = true;
    _friendRequestsEnabled = true;
    _doNotDisturb = false;
    _showOnlineStatus = true;
    _allowDirectMessages = true;
}

} // namespace ivx
