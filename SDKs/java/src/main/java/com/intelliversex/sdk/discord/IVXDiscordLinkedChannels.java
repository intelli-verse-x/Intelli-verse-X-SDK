// Copyright (c) 2026 Intelli-verse-X
// MIT License — see LICENSE in the project root.

package com.intelliversex.sdk.discord;

import java.util.Collections;
import java.util.List;

/**
 * Discord channel linking for game lobbies: link/unlink channels and query linked channel metadata.
 * Stub API for lobby ↔ Discord channel associations.
 */
public final class IVXDiscordLinkedChannels {

    private static final IVXDiscordLinkedChannels INSTANCE = new IVXDiscordLinkedChannels();

    private IVXDiscordLinkedChannels() {}

    public static IVXDiscordLinkedChannels getInstance() {
        return INSTANCE;
    }

    /**
     * Links a Discord channel to a game lobby.
     *
     * @param lobbyId   game lobby identifier
     * @param channelId Discord channel snowflake id
     * @return metadata for the newly linked channel
     * @throws UnsupportedOperationException when not implemented
     */
    public IVXLinkedChannel linkChannel(String lobbyId, String channelId) {
        throw new UnsupportedOperationException("Not implemented");
    }

    /**
     * Removes the link between a lobby and a Discord channel.
     *
     * @param lobbyId   game lobby identifier
     * @param channelId Discord channel snowflake id
     * @throws UnsupportedOperationException when not implemented
     */
    public void unlinkChannel(String lobbyId, String channelId) {
        throw new UnsupportedOperationException("Not implemented");
    }

    /**
     * Returns all Discord channels currently linked to the given lobby.
     *
     * @param lobbyId game lobby identifier
     * @return linked channels (empty stub list until implemented)
     */
    public List<IVXLinkedChannel> getLinkedChannels(String lobbyId) {
        return Collections.emptyList();
    }

    /**
     * A Discord channel linked to a game lobby.
     */
    public static final class IVXLinkedChannel {
        /** Discord channel snowflake id. */
        public String channelId;
        /** Discord guild (server) snowflake id. */
        public String guildId;
        /** Display name of the channel. */
        public String name;
        /** Game lobby this channel is linked to. */
        public String lobbyId;
        /** Unix epoch millis when the link was created. */
        public long linkedAt;
    }
}
