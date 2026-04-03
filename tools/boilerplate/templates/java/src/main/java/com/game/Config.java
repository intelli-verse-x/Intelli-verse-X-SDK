package com.game;

/**
 * Template-expanded Nakama / IntelliVerseX connection settings.
 */
public final class Config {
    private Config() {}

    public static final String GAME_ID = "{{game_id}}";
    public static final String SERVER_HOST = "{{server_host}}";
    public static final int SERVER_PORT = Integer.parseInt("{{server_port}}");
    public static final String SERVER_KEY = "{{server_key}}";
    public static final String BUNDLE_ID = "{{bundle_id}}";
    public static final String PACKAGE_NAME = "{{package_name}}";
    public static final String COMPANY_NAME = "{{company_name}}";
    public static final String TAGLINE = "{{tagline}}";
    public static final int MAX_ENERGY = {{max_energy}};
    public static final int ENERGY_REFILL_MINUTES = {{energy_refill_minutes}};
    public static final long INITIAL_COINS = {{initial_coins}}L;
    public static final long INITIAL_GEMS = {{initial_gems}}L;
}
