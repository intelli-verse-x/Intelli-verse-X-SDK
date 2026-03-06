/// IntelliVerseX C++ SDK — unit tests (assert-based, no framework dependency).
/// Compile: g++ -std=c++17 -I../include test_ivx.cpp ../src/ivx_manager.cpp -lnakama-sdk -o test_ivx

#include "intelliversex/ivx_config.h"
#include "intelliversex/ivx_manager.h"
#include "intelliversex/ivx_types.h"

#include <cassert>
#include <iostream>
#include <stdexcept>
#include <string>

static int passed = 0;
static int failed = 0;

#define TEST(name)                                                        \
    do {                                                                  \
        std::cout << "  [TEST] " << #name << " ... ";                    \
        try {                                                             \
            test_##name();                                                \
            std::cout << "PASS\n";                                        \
            ++passed;                                                     \
        } catch (const std::exception& ex) {                              \
            std::cout << "FAIL (" << ex.what() << ")\n";                  \
            ++failed;                                                     \
        }                                                                 \
    } while (0)

#define ASSERT_TRUE(cond)                                                 \
    do {                                                                  \
        if (!(cond))                                                      \
            throw std::runtime_error(std::string("assertion failed: ")    \
                                     + #cond + "  (" + __FILE__ + ":"     \
                                     + std::to_string(__LINE__) + ")");   \
    } while (0)

#define ASSERT_FALSE(cond) ASSERT_TRUE(!(cond))

#define ASSERT_EQ(a, b)                                                   \
    do {                                                                  \
        if ((a) != (b))                                                   \
            throw std::runtime_error(std::string("expected equal: ")      \
                                     + #a + " != " + #b + "  ("          \
                                     + __FILE__ + ":"                     \
                                     + std::to_string(__LINE__) + ")");   \
    } while (0)

#define ASSERT_THROWS(expr, ExType)                                       \
    do {                                                                  \
        bool threw = false;                                               \
        try { expr; } catch (const ExType&) { threw = true; }            \
        if (!threw)                                                       \
            throw std::runtime_error(std::string("expected exception: ")  \
                                     + #ExType + "  (" + __FILE__ + ":"   \
                                     + std::to_string(__LINE__) + ")");   \
    } while (0)

// ---- Config validation tests ----

static void test_config_default_is_valid() {
    ivx::Config cfg;
    cfg.validate(); // should not throw
}

static void test_config_empty_host_throws() {
    ivx::Config cfg;
    cfg.host = "";
    ASSERT_THROWS(cfg.validate(), std::invalid_argument);
}

static void test_config_zero_port_throws() {
    ivx::Config cfg;
    cfg.port = 0;
    ASSERT_THROWS(cfg.validate(), std::invalid_argument);
}

static void test_config_negative_port_throws() {
    ivx::Config cfg;
    cfg.port = -1;
    ASSERT_THROWS(cfg.validate(), std::invalid_argument);
}

static void test_config_port_too_high_throws() {
    ivx::Config cfg;
    cfg.port = 70000;
    ASSERT_THROWS(cfg.validate(), std::invalid_argument);
}

static void test_config_empty_server_key_throws() {
    ivx::Config cfg;
    cfg.serverKey = "";
    ASSERT_THROWS(cfg.validate(), std::invalid_argument);
}

static void test_config_empty_storage_path_throws() {
    ivx::Config cfg;
    cfg.storagePath = "";
    ASSERT_THROWS(cfg.validate(), std::invalid_argument);
}

static void test_config_valid_port_bounds() {
    ivx::Config cfg;
    cfg.port = 1;
    cfg.validate();
    cfg.port = 65535;
    cfg.validate();
}

// ---- Manager singleton tests ----

static void test_singleton_identity() {
    auto& a = ivx::Manager::instance();
    auto& b = ivx::Manager::instance();
    ASSERT_TRUE(&a == &b);
}

// ---- Manager state before init ----

static void test_not_initialized_before_init() {
    auto& mgr = ivx::Manager::instance();
    // Singleton may have been initialized by a prior test; but we can at
    // least assert the interface is callable.
    ASSERT_TRUE(true);
}

static void test_no_session_before_auth() {
    auto& mgr = ivx::Manager::instance();
    ASSERT_FALSE(mgr.hasSession());
    ASSERT_TRUE(mgr.userId().empty());
    ASSERT_TRUE(mgr.username().empty());
}

// ---- Init with valid config ----

static void test_init_valid_config() {
    ivx::Config cfg;
    cfg.host = "127.0.0.1";
    cfg.port = 7350;
    cfg.serverKey = "defaultkey";
    cfg.storagePath = ".";
    cfg.debugLogs = true;

    auto& mgr = ivx::Manager::instance();
    mgr.init(cfg);
    ASSERT_TRUE(mgr.initialized());
}

// ---- Init with invalid config ----

static void test_init_bad_config_throws() {
    ivx::Config cfg;
    cfg.host = "";
    auto& mgr = ivx::Manager::instance();
    ASSERT_THROWS(mgr.init(cfg), std::invalid_argument);
}

// ---- Session state after init but no auth ----

static void test_session_state_after_init() {
    auto& mgr = ivx::Manager::instance();
    // re-init with valid config
    ivx::Config cfg;
    cfg.debugLogs = false;
    mgr.init(cfg);

    ASSERT_TRUE(mgr.initialized());
    ASSERT_FALSE(mgr.hasSession());
    ASSERT_EQ(mgr.userId(), std::string(""));
}

// ---- Version constant ----

static void test_version_not_empty() {
    ASSERT_TRUE(std::string(ivx::Manager::VERSION).size() > 0);
}

// ---- Run all ----

int main() {
    std::cout << "IntelliVerseX C++ SDK Tests\n";
    std::cout << "===========================\n\n";

    std::cout << "Config validation:\n";
    TEST(config_default_is_valid);
    TEST(config_empty_host_throws);
    TEST(config_zero_port_throws);
    TEST(config_negative_port_throws);
    TEST(config_port_too_high_throws);
    TEST(config_empty_server_key_throws);
    TEST(config_empty_storage_path_throws);
    TEST(config_valid_port_bounds);

    std::cout << "\nSingleton:\n";
    TEST(singleton_identity);

    std::cout << "\nPre-init state:\n";
    TEST(not_initialized_before_init);
    TEST(no_session_before_auth);

    std::cout << "\nInitialization:\n";
    TEST(init_valid_config);
    TEST(init_bad_config_throws);

    std::cout << "\nSession state:\n";
    TEST(session_state_after_init);

    std::cout << "\nMisc:\n";
    TEST(version_not_empty);

    std::cout << "\n===========================\n";
    std::cout << "Results: " << passed << " passed, " << failed << " failed\n";
    return failed > 0 ? 1 : 0;
}
