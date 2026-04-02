// Copyright (c) 2026 Intelli-verse-X — MIT License
#pragma once

namespace ivx {

/**
 * Discord Social Settings — notification preferences, privacy, DND mode.
 * Stub: API shape matches Unity IVXDiscordSettings.
 */
struct DiscordSettings {
    bool notifications_enabled = true;
    bool friend_requests_enabled = true;
    bool do_not_disturb = false;
    bool show_online_status = true;
    bool allow_direct_messages = true;

    void enable_dnd() { do_not_disturb = true; }
    void disable_dnd() { do_not_disturb = false; }

    void reset_to_defaults() {
        notifications_enabled = true;
        friend_requests_enabled = true;
        do_not_disturb = false;
        show_online_status = true;
        allow_direct_messages = true;
    }
};

} // namespace ivx
