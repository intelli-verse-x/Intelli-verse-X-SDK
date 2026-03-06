package com.intelliversex.sdk.examples;

import com.intelliversex.sdk.core.IVXConfig;
import com.intelliversex.sdk.core.IVXManager;
import com.intelliversex.sdk.core.IVXProfile;

import java.util.List;
import java.util.Map;
import java.util.concurrent.CompletableFuture;

/**
 * Complete example showing init, auth, profile, wallet, leaderboard, storage, and RPC.
 *
 * <p>Run with a local Nakama server:
 * <pre>{@code
 * javac -cp "lib/*" BasicExample.java
 * java  -cp "lib/*:." com.intelliversex.sdk.examples.BasicExample
 * }</pre>
 */
public class BasicExample {

    public static void main(String[] args) {
        System.out.println("=== IntelliVerseX Java SDK Example ===");
        System.out.println("SDK version: " + IVXManager.SDK_VERSION + "\n");

        // ── 1. Configure ──────────────────────────────────────────────
        IVXConfig config = IVXConfig.builder()
                .nakamaHost("127.0.0.1")
                .nakamaPort(7350)
                .nakamaServerKey("defaultkey")
                .useSSL(false)
                .enableDebugLogs(true)
                .build();

        System.out.println("Config: " + config);

        // ── 2. Initialize ─────────────────────────────────────────────
        IVXManager mgr = IVXManager.getInstance();
        mgr.initialize(config);
        System.out.println("Initialized: " + mgr.isInitialized());

        // ── 3. Restore session or authenticate ────────────────────────
        if (!mgr.restoreSession()) {
            System.out.println("No cached session — authenticating with device ID...");
            mgr.authenticateDevice(null);
        } else {
            System.out.println("Session restored for user: " + mgr.getUserId());
        }

        if (!mgr.hasValidSession()) {
            System.err.println("Authentication failed — exiting.");
            return;
        }

        System.out.println("Authenticated — userId: " + mgr.getUserId()
                + ", username: " + mgr.getUsername());

        // ── 4. Fetch profile ──────────────────────────────────────────
        System.out.println("\n--- Fetching profile ---");
        IVXProfile profile = mgr.fetchProfile();
        if (profile != null) {
            System.out.println("  " + profile);
        } else {
            System.out.println("  (could not fetch profile)");
        }

        // ── 5. Fetch wallet ──────────────────────────────────────────
        System.out.println("\n--- Fetching wallet ---");
        String wallet = mgr.fetchWallet();
        System.out.println("  Wallet JSON: " + wallet);

        // ── 6. Grant currency ─────────────────────────────────────────
        System.out.println("\n--- Granting 100 coins ---");
        String grantResult = mgr.grantCurrency("coins", 100);
        System.out.println("  Grant response: " + grantResult);

        // ── 7. Submit leaderboard score ───────────────────────────────
        System.out.println("\n--- Submitting score ---");
        mgr.submitScore("global_leaderboard", 4200);
        System.out.println("  Score submitted!");

        // ── 8. Fetch leaderboard ──────────────────────────────────────
        System.out.println("\n--- Top 10 leaderboard ---");
        List<Map<String, Object>> records = mgr.fetchLeaderboard("global_leaderboard", 10);
        if (records.isEmpty()) {
            System.out.println("  (no records)");
        } else {
            for (Map<String, Object> r : records) {
                System.out.printf("  #%s  %s  score=%s%n",
                        r.get("rank"), r.get("username"), r.get("score"));
            }
        }

        // ── 9. Storage round-trip ─────────────────────────────────────
        System.out.println("\n--- Storage write/read ---");
        mgr.writeStorage("game_data", "player_prefs", "{\"volume\":0.8,\"difficulty\":\"hard\"}");
        String stored = mgr.readStorage("game_data", "player_prefs");
        System.out.println("  Read back: " + stored);

        // ── 10. Custom RPC ────────────────────────────────────────────
        System.out.println("\n--- Custom RPC ---");
        String rpcResult = mgr.callRpc("my_custom_rpc", "{\"action\":\"ping\"}");
        System.out.println("  RPC response: " + rpcResult);

        // ── 11. Async example ─────────────────────────────────────────
        System.out.println("\n--- Async profile fetch ---");
        CompletableFuture<IVXProfile> asyncProfile = mgr.fetchProfileAsync();
        asyncProfile
                .thenAccept(p -> System.out.println("  Async profile: " + p))
                .exceptionally(ex -> {
                    System.err.println("  Async profile error: " + ex.getMessage());
                    return null;
                })
                .join();

        // ── 12. Cleanup ──────────────────────────────────────────────
        System.out.println("\n--- Clearing session ---");
        mgr.clearSession();
        System.out.println("  Session valid: " + mgr.hasValidSession());

        System.out.println("\n=== Example complete ===");
    }
}
