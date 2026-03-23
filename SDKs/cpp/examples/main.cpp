/// IntelliVerseX C++ SDK — complete usage example.
/// Compile: g++ -std=c++17 -I../include main.cpp ../src/ivx_manager.cpp -lnakama-sdk -o example

#include "intelliversex/ivx_manager.h"
#include <iostream>
#include <thread>
#include <chrono>

static void pumpTicks(ivx::Manager& mgr, int count = 60) {
    for (int i = 0; i < count; ++i) {
        mgr.tick();
        std::this_thread::sleep_for(std::chrono::milliseconds(50));
    }
}

int main() {
    std::cout << "=== IntelliVerseX C++ SDK Example ===\n"
              << "SDK version: " << ivx::Manager::VERSION << "\n\n";

    // ── 1. Configure ──
    ivx::Config cfg;
    cfg.host       = "127.0.0.1";
    cfg.port       = 7350;
    cfg.serverKey  = "defaultkey";
    cfg.useSSL     = false;
    cfg.debugLogs  = true;
    cfg.storagePath = ".";       // writable directory for session/device files

    auto& mgr = ivx::Manager::instance();

    try {
        mgr.init(cfg);
    } catch (const std::exception& ex) {
        std::cerr << "Init failed: " << ex.what() << "\n";
        return 1;
    }

    // ── 2. Restore or authenticate ──
    if (!mgr.restoreSession()) {
        std::cout << "No cached session — authenticating with device ID...\n";

        bool authDone = false;
        bool authOk = false;

        mgr.authDevice("",
            [&]() {
                authOk = true;
                authDone = true;
                std::cout << "Auth succeeded!  userId=" << mgr.userId()
                          << "  username=" << mgr.username() << "\n";
            },
            [&](const ivx::Error& e) {
                authDone = true;
                std::cerr << "Auth error [" << e.code << "]: " << e.message << "\n";
            });

        while (!authDone) {
            mgr.tick();
            std::this_thread::sleep_for(std::chrono::milliseconds(50));
        }

        if (!authOk) {
            std::cerr << "Cannot continue without authentication.\n";
            return 1;
        }
    } else {
        std::cout << "Session restored for user " << mgr.userId() << "\n";
    }

    // ── 3. Fetch profile ──
    std::cout << "\n--- Fetching profile ---\n";
    mgr.fetchProfile(
        [](const ivx::Profile& p) {
            std::cout << "  userId:      " << p.userId << "\n"
                      << "  username:    " << p.username << "\n"
                      << "  displayName: " << p.displayName << "\n"
                      << "  avatarUrl:   " << p.avatarUrl << "\n"
                      << "  langTag:     " << p.langTag << "\n"
                      << "  wallet:      " << p.wallet << "\n";
        },
        [](const ivx::Error& e) {
            std::cerr << "  Profile error: " << e.message << "\n";
        });
    pumpTicks(mgr);

    // ── 4. Fetch wallet ──
    std::cout << "\n--- Fetching wallet ---\n";
    mgr.fetchWallet(
        [](const std::string& json) {
            std::cout << "  Wallet JSON: " << json << "\n";
        },
        [](const ivx::Error& e) {
            std::cerr << "  Wallet error: " << e.message << "\n";
        });
    pumpTicks(mgr);

    // ── 5. Grant currency ──
    std::cout << "\n--- Granting 100 coins ---\n";
    mgr.grantCurrency("coins", 100,
        [](const std::string& json) {
            std::cout << "  Grant response: " << json << "\n";
        },
        [](const ivx::Error& e) {
            std::cerr << "  Grant error: " << e.message << "\n";
        });
    pumpTicks(mgr);

    // ── 6. Submit a leaderboard score ──
    std::cout << "\n--- Submitting score ---\n";
    mgr.submitScore("global_leaderboard", 4200,
        []() { std::cout << "  Score submitted!\n"; },
        [](const ivx::Error& e) {
            std::cerr << "  Submit error: " << e.message << "\n";
        });
    pumpTicks(mgr);

    // ── 7. Fetch leaderboard ──
    std::cout << "\n--- Top 10 leaderboard ---\n";
    mgr.fetchLeaderboard("global_leaderboard", 10,
        [](const std::vector<ivx::LeaderboardRecord>& recs) {
            for (auto& r : recs) {
                std::cout << "  #" << r.rank << "  " << r.username
                          << "  score=" << r.score << "\n";
            }
            if (recs.empty()) std::cout << "  (no records)\n";
        },
        [](const ivx::Error& e) {
            std::cerr << "  Leaderboard error: " << e.message << "\n";
        });
    pumpTicks(mgr);

    // ── 8. Storage round-trip ──
    std::cout << "\n--- Storage write/read ---\n";
    mgr.writeStorage("game_data", "player_prefs", "{\"volume\":0.8,\"difficulty\":\"hard\"}",
        [&mgr]() {
            std::cout << "  Written. Reading back...\n";
            mgr.readStorage("game_data", "player_prefs",
                [](const std::string& json) {
                    std::cout << "  Read back: " << json << "\n";
                },
                [](const ivx::Error& e) {
                    std::cerr << "  Read error: " << e.message << "\n";
                });
        },
        [](const ivx::Error& e) {
            std::cerr << "  Write error: " << e.message << "\n";
        });
    pumpTicks(mgr, 120);

    // ── 9. Custom RPC ──
    std::cout << "\n--- Custom RPC ---\n";
    mgr.rpc("my_custom_rpc", "{\"action\":\"ping\"}",
        [](const std::string& json) {
            std::cout << "  RPC response: " << json << "\n";
        },
        [](const ivx::Error& e) {
            std::cerr << "  RPC error: " << e.message << "\n";
        });
    pumpTicks(mgr);

    std::cout << "\n=== Example complete ===\n";
    return 0;
}
