package com.intelliversex.sdk.core;

/**
 * Configuration for the IntelliVerseX SDK.
 * <p>
 * Use the {@link Builder} to construct a validated instance:
 * <pre>{@code
 * IVXConfig config = IVXConfig.builder()
 *         .nakamaHost("127.0.0.1")
 *         .nakamaPort(7350)
 *         .nakamaServerKey("defaultkey")
 *         .enableDebugLogs(true)
 *         .build();
 * }</pre>
 */
public class IVXConfig {
    private String nakamaHost = "127.0.0.1";
    private int nakamaPort = 7350;
    private String nakamaServerKey = "defaultkey";
    private boolean useSSL = false;

    private String cognitoRegion = "";
    private String cognitoUserPoolId = "";
    private String cognitoClientId = "";

    private boolean enableAnalytics = true;
    private boolean enableDebugLogs = false;
    private boolean verboseLogging = false;

    public IVXConfig() {}

    public static Builder builder() {
        return new Builder();
    }

    public String getNakamaHost() { return nakamaHost; }
    public int getNakamaPort() { return nakamaPort; }
    public String getNakamaServerKey() { return nakamaServerKey; }
    public boolean isUseSSL() { return useSSL; }
    public String getCognitoRegion() { return cognitoRegion; }
    public String getCognitoUserPoolId() { return cognitoUserPoolId; }
    public String getCognitoClientId() { return cognitoClientId; }
    public boolean isEnableAnalytics() { return enableAnalytics; }
    public boolean isEnableDebugLogs() { return enableDebugLogs; }
    public boolean isVerboseLogging() { return verboseLogging; }

    public String getScheme() { return useSSL ? "https" : "http"; }
    public String getBaseUrl() { return getScheme() + "://" + nakamaHost + ":" + nakamaPort; }

    @Override
    public String toString() {
        return "IVXConfig{"
                + "host='" + nakamaHost + '\''
                + ", port=" + nakamaPort
                + ", serverKey='" + nakamaServerKey + '\''
                + ", ssl=" + useSSL
                + ", analytics=" + enableAnalytics
                + ", debug=" + enableDebugLogs
                + ", verbose=" + verboseLogging
                + ", baseUrl='" + getBaseUrl() + '\''
                + '}';
    }

    public static class Builder {
        private final IVXConfig config = new IVXConfig();

        public Builder nakamaHost(String host) { config.nakamaHost = host; return this; }
        public Builder nakamaPort(int port) { config.nakamaPort = port; return this; }
        public Builder nakamaServerKey(String key) { config.nakamaServerKey = key; return this; }
        public Builder useSSL(boolean ssl) { config.useSSL = ssl; return this; }
        public Builder cognitoRegion(String region) { config.cognitoRegion = region; return this; }
        public Builder cognitoUserPoolId(String poolId) { config.cognitoUserPoolId = poolId; return this; }
        public Builder cognitoClientId(String clientId) { config.cognitoClientId = clientId; return this; }
        public Builder enableAnalytics(boolean enable) { config.enableAnalytics = enable; return this; }
        public Builder enableDebugLogs(boolean enable) { config.enableDebugLogs = enable; return this; }
        public Builder verboseLogging(boolean verbose) { config.verboseLogging = verbose; return this; }

        /**
         * Validates and returns the configuration.
         *
         * @return a validated {@link IVXConfig} instance
         * @throws IllegalArgumentException if host, server key, or port is invalid
         */
        public IVXConfig build() {
            if (config.nakamaHost == null || config.nakamaHost.trim().isEmpty()) {
                throw new IllegalArgumentException("nakamaHost must not be null or empty");
            }
            if (config.nakamaServerKey == null || config.nakamaServerKey.trim().isEmpty()) {
                throw new IllegalArgumentException("nakamaServerKey must not be null or empty");
            }
            if (config.nakamaPort < 1 || config.nakamaPort > 65535) {
                throw new IllegalArgumentException("nakamaPort must be in range [1, 65535], got: " + config.nakamaPort);
            }
            return config;
        }
    }
}
