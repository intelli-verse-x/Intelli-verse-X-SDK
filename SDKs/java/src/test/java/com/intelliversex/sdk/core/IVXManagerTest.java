package com.intelliversex.sdk.core;

import org.junit.jupiter.api.BeforeEach;
import org.junit.jupiter.api.DisplayName;
import org.junit.jupiter.api.Nested;
import org.junit.jupiter.api.Test;

import static org.junit.jupiter.api.Assertions.*;

/**
 * Unit tests for the IntelliVerseX Java SDK.
 * <p>
 * These tests cover configuration validation, singleton behaviour, and
 * pre-init / post-init state. They do NOT require a running Nakama server.
 */
class IVXManagerTest {

    // ──────────────────────────────────────────────────────────────
    // Config validation
    // ──────────────────────────────────────────────────────────────

    @Nested
    @DisplayName("IVXConfig.Builder validation")
    class ConfigValidation {

        @Test
        @DisplayName("default builder produces valid config")
        void defaultBuilderIsValid() {
            IVXConfig cfg = IVXConfig.builder().build();
            assertNotNull(cfg);
            assertEquals("127.0.0.1", cfg.getNakamaHost());
            assertEquals(7350, cfg.getNakamaPort());
            assertEquals("defaultkey", cfg.getNakamaServerKey());
        }

        @Test
        @DisplayName("null host throws IllegalArgumentException")
        void nullHostThrows() {
            assertThrows(IllegalArgumentException.class, () ->
                    IVXConfig.builder().nakamaHost(null).build());
        }

        @Test
        @DisplayName("empty host throws IllegalArgumentException")
        void emptyHostThrows() {
            assertThrows(IllegalArgumentException.class, () ->
                    IVXConfig.builder().nakamaHost("").build());
        }

        @Test
        @DisplayName("blank host throws IllegalArgumentException")
        void blankHostThrows() {
            assertThrows(IllegalArgumentException.class, () ->
                    IVXConfig.builder().nakamaHost("   ").build());
        }

        @Test
        @DisplayName("null server key throws IllegalArgumentException")
        void nullServerKeyThrows() {
            assertThrows(IllegalArgumentException.class, () ->
                    IVXConfig.builder().nakamaServerKey(null).build());
        }

        @Test
        @DisplayName("empty server key throws IllegalArgumentException")
        void emptyServerKeyThrows() {
            assertThrows(IllegalArgumentException.class, () ->
                    IVXConfig.builder().nakamaServerKey("").build());
        }

        @Test
        @DisplayName("port 0 throws IllegalArgumentException")
        void port0Throws() {
            assertThrows(IllegalArgumentException.class, () ->
                    IVXConfig.builder().nakamaPort(0).build());
        }

        @Test
        @DisplayName("port -1 throws IllegalArgumentException")
        void negativePortThrows() {
            assertThrows(IllegalArgumentException.class, () ->
                    IVXConfig.builder().nakamaPort(-1).build());
        }

        @Test
        @DisplayName("port 70000 throws IllegalArgumentException")
        void portTooHighThrows() {
            assertThrows(IllegalArgumentException.class, () ->
                    IVXConfig.builder().nakamaPort(70000).build());
        }

        @Test
        @DisplayName("port boundary values (1 and 65535) are accepted")
        void portBoundaryValues() {
            assertDoesNotThrow(() -> IVXConfig.builder().nakamaPort(1).build());
            assertDoesNotThrow(() -> IVXConfig.builder().nakamaPort(65535).build());
        }

        @Test
        @DisplayName("toString returns non-empty description")
        void toStringNotEmpty() {
            IVXConfig cfg = IVXConfig.builder().build();
            String s = cfg.toString();
            assertNotNull(s);
            assertTrue(s.contains("IVXConfig{"));
            assertTrue(s.contains("127.0.0.1"));
        }

        @Test
        @DisplayName("getBaseUrl respects SSL setting")
        void baseUrlRespectsSSL() {
            IVXConfig http = IVXConfig.builder().useSSL(false).build();
            assertTrue(http.getBaseUrl().startsWith("http://"));

            IVXConfig https = IVXConfig.builder().useSSL(true).build();
            assertTrue(https.getBaseUrl().startsWith("https://"));
        }
    }

    // ──────────────────────────────────────────────────────────────
    // Singleton
    // ──────────────────────────────────────────────────────────────

    @Nested
    @DisplayName("Singleton behaviour")
    class Singleton {

        @Test
        @DisplayName("getInstance returns non-null")
        void instanceNotNull() {
            assertNotNull(IVXManager.getInstance());
        }

        @Test
        @DisplayName("getInstance returns same reference on repeated calls")
        void sameInstance() {
            IVXManager a = IVXManager.getInstance();
            IVXManager b = IVXManager.getInstance();
            assertSame(a, b);
        }
    }

    // ──────────────────────────────────────────────────────────────
    // Pre-init state
    // ──────────────────────────────────────────────────────────────

    @Nested
    @DisplayName("State before initialize()")
    class PreInit {

        @Test
        @DisplayName("hasValidSession is false before init")
        void noSessionBeforeInit() {
            IVXManager mgr = IVXManager.getInstance();
            assertFalse(mgr.hasValidSession());
        }

        @Test
        @DisplayName("getUserId returns empty string before auth")
        void emptyUserIdBeforeAuth() {
            IVXManager mgr = IVXManager.getInstance();
            assertEquals("", mgr.getUserId());
        }

        @Test
        @DisplayName("getUsername returns empty string before auth")
        void emptyUsernameBeforeAuth() {
            IVXManager mgr = IVXManager.getInstance();
            assertEquals("", mgr.getUsername());
        }
    }

    // ──────────────────────────────────────────────────────────────
    // Initialize
    // ──────────────────────────────────────────────────────────────

    @Nested
    @DisplayName("initialize()")
    class Initialization {

        @Test
        @DisplayName("initialize with valid config sets initialized flag")
        void initSetsFlag() {
            IVXManager mgr = IVXManager.getInstance();
            IVXConfig cfg = IVXConfig.builder()
                    .nakamaHost("127.0.0.1")
                    .nakamaPort(7350)
                    .nakamaServerKey("defaultkey")
                    .enableDebugLogs(true)
                    .build();
            mgr.initialize(cfg);
            assertTrue(mgr.isInitialized());
        }

        @Test
        @DisplayName("initialize with null config throws NullPointerException")
        void nullConfigThrows() {
            IVXManager mgr = IVXManager.getInstance();
            assertThrows(NullPointerException.class, () -> mgr.initialize(null));
        }
    }

    // ──────────────────────────────────────────────────────────────
    // Session state (post-init, no auth)
    // ──────────────────────────────────────────────────────────────

    @Nested
    @DisplayName("Session state after init, before auth")
    class SessionState {

        @BeforeEach
        void setUp() {
            IVXManager mgr = IVXManager.getInstance();
            IVXConfig cfg = IVXConfig.builder().build();
            mgr.initialize(cfg);
        }

        @Test
        @DisplayName("hasValidSession is false after init with no auth")
        void noSession() {
            assertFalse(IVXManager.getInstance().hasValidSession());
        }

        @Test
        @DisplayName("getSession returns null after init with no auth")
        void nullSession() {
            assertNull(IVXManager.getInstance().getSession());
        }

        @Test
        @DisplayName("fetchProfile throws when no session")
        void fetchProfileThrowsWithoutSession() {
            assertThrows(IllegalStateException.class, () ->
                    IVXManager.getInstance().fetchProfile());
        }

        @Test
        @DisplayName("callRpc throws when no session")
        void callRpcThrowsWithoutSession() {
            assertThrows(IllegalStateException.class, () ->
                    IVXManager.getInstance().callRpc("some_rpc", "{}"));
        }
    }

    // ──────────────────────────────────────────────────────────────
    // IVXProfile model
    // ──────────────────────────────────────────────────────────────

    @Nested
    @DisplayName("IVXProfile model")
    class ProfileModel {

        @Test
        @DisplayName("constructor null-coerces fields to empty strings")
        void nullFieldsBecomeEmpty() {
            IVXProfile p = new IVXProfile(null, null, null, null, null, null, null);
            assertEquals("", p.getUserId());
            assertEquals("", p.getUsername());
            assertEquals("", p.getDisplayName());
            assertEquals("", p.getAvatarUrl());
            assertEquals("", p.getLangTag());
            assertEquals("", p.getMetadata());
            assertEquals("", p.getWallet());
        }

        @Test
        @DisplayName("equals and hashCode contract")
        void equalsAndHashCode() {
            IVXProfile a = new IVXProfile("u1", "user", "User", "", "en", "{}", "{}");
            IVXProfile b = new IVXProfile("u1", "user", "User", "", "en", "{}", "{}");
            assertEquals(a, b);
            assertEquals(a.hashCode(), b.hashCode());
        }

        @Test
        @DisplayName("toString contains userId")
        void toStringContainsUserId() {
            IVXProfile p = new IVXProfile("abc-123", "player", "Player", "", "", "", "");
            assertTrue(p.toString().contains("abc-123"));
        }
    }
}
