--- IntelliVerseX Defold SDK — unit tests.
---
--- Run with any Lua test runner (e.g. busted, luaunit) or Defold's built-in
--- test facilities. Each test_ function is self-contained.
---
--- Usage with busted:
---   busted tests/test_ivx.lua
---
--- Usage standalone (Defold script component):
---   local t = require "tests.test_ivx"
---   t.run_all()

local M = {}

-- ---------------------------------------------------------------------------
-- Minimal test harness (works without external runner)
-- ---------------------------------------------------------------------------

local passed = 0
local failed = 0
local errors = {}

local function assert_true(val, msg)
    if not val then
        error(msg or "expected true, got " .. tostring(val), 2)
    end
end

local function assert_false(val, msg)
    if val then
        error(msg or "expected false, got " .. tostring(val), 2)
    end
end

local function assert_eq(a, b, msg)
    if a ~= b then
        error((msg or "assert_eq") .. ": expected " .. tostring(b) .. ", got " .. tostring(a), 2)
    end
end

local function assert_not_nil(val, msg)
    if val == nil then
        error(msg or "expected non-nil value", 2)
    end
end

local function assert_nil(val, msg)
    if val ~= nil then
        error(msg or "expected nil, got " .. tostring(val), 2)
    end
end

local function assert_type(val, expected_type, msg)
    if type(val) ~= expected_type then
        error((msg or "assert_type") .. ": expected " .. expected_type .. ", got " .. type(val), 2)
    end
end

local function run_test(name, fn)
    local ok, err = pcall(fn)
    if ok then
        passed = passed + 1
        print(string.format("  ✓ %s", name))
    else
        failed = failed + 1
        table.insert(errors, { name = name, err = err })
        print(string.format("  ✗ %s — %s", name, tostring(err)))
    end
end


-- ---------------------------------------------------------------------------
-- Mock sys module for tests running outside Defold
-- ---------------------------------------------------------------------------

local _saved_files = {}

if not sys then
    sys = {
        get_save_file = function(app, file)
            return app .. "/" .. file
        end,
        load = function(path)
            return _saved_files[path] or {}
        end,
        save = function(path, data)
            _saved_files[path] = data
            return true
        end,
        get_sys_info = function()
            return { system_name = "test", device_ident = "test-device-001" }
        end,
        get_engine_info = function()
            return { version = "1.0.0-test" }
        end,
    }
end


-- ---------------------------------------------------------------------------
-- Tests: Module loading
-- ---------------------------------------------------------------------------

function M.test_module_loads()
    run_test("module loads without error", function()
        local ivx = require "intelliversex.ivx"
        assert_not_nil(ivx, "ivx module should not be nil")
    end)
end

function M.test_module_has_version()
    run_test("module exposes SDK_VERSION", function()
        local ivx = require "intelliversex.ivx"
        assert_not_nil(ivx.SDK_VERSION)
        assert_type(ivx.SDK_VERSION, "string", "SDK_VERSION type")
        assert_eq(ivx.SDK_VERSION, "5.1.0", "SDK version")
    end)
end

function M.test_module_exports_all_functions()
    run_test("module exports expected public API", function()
        local ivx = require "intelliversex.ivx"
        local expected = {
            "configure", "is_initialized", "on",
            "authenticate_device", "authenticate_email",
            "authenticate_google", "authenticate_apple", "authenticate_custom",
            "restore_session", "clear_session", "disconnect_socket",
            "has_valid_session", "get_user_id", "get_username",
            "fetch_profile", "update_profile",
            "fetch_wallet", "grant_currency",
            "submit_score", "fetch_leaderboard",
            "write_storage", "read_storage",
            "call_rpc", "connect_socket",
        }
        for _, fn_name in ipairs(expected) do
            assert_not_nil(ivx[fn_name], "missing function: " .. fn_name)
            assert_type(ivx[fn_name], "function", fn_name .. " should be a function")
        end
    end)
end


-- ---------------------------------------------------------------------------
-- Tests: Not initialized before configure
-- ---------------------------------------------------------------------------

function M.test_not_initialized_before_configure()
    run_test("is_initialized() returns false before configure()", function()
        -- Force a fresh require by clearing the cached module
        package.loaded["intelliversex.ivx"] = nil
        local ivx = require "intelliversex.ivx"
        assert_false(ivx.is_initialized(), "should not be initialized before configure()")
    end)
end

function M.test_has_no_session_before_configure()
    run_test("has_valid_session() returns false before configure()", function()
        package.loaded["intelliversex.ivx"] = nil
        local ivx = require "intelliversex.ivx"
        assert_false(ivx.has_valid_session(), "should have no session before configure()")
    end)
end

function M.test_user_id_empty_before_configure()
    run_test("get_user_id() returns empty string before configure()", function()
        package.loaded["intelliversex.ivx"] = nil
        local ivx = require "intelliversex.ivx"
        assert_eq(ivx.get_user_id(), "", "user_id before configure")
    end)
end

function M.test_username_empty_before_configure()
    run_test("get_username() returns empty string before configure()", function()
        package.loaded["intelliversex.ivx"] = nil
        local ivx = require "intelliversex.ivx"
        assert_eq(ivx.get_username(), "", "username before configure")
    end)
end


-- ---------------------------------------------------------------------------
-- Tests: Configure sets up state
-- ---------------------------------------------------------------------------

function M.test_configure_sets_initialized()
    run_test("configure() sets is_initialized to true", function()
        package.loaded["intelliversex.ivx"] = nil
        local ivx = require "intelliversex.ivx"
        ivx.configure({
            host = "127.0.0.1",
            port = 7350,
            server_key = "defaultkey",
            debug = false,
        })
        assert_true(ivx.is_initialized(), "should be initialized after configure()")
    end)
end

function M.test_configure_accepts_custom_options()
    run_test("configure() accepts custom host/port", function()
        package.loaded["intelliversex.ivx"] = nil
        local ivx = require "intelliversex.ivx"
        ivx.configure({
            host = "play.example.com",
            port = 443,
            server_key = "prod-key",
            use_ssl = true,
            debug = true,
        })
        assert_true(ivx.is_initialized())
    end)
end

function M.test_configure_defaults():
    run_test("configure() fills in defaults for missing fields", function()
        package.loaded["intelliversex.ivx"] = nil
        local ivx = require "intelliversex.ivx"
        ivx.configure({})
        assert_true(ivx.is_initialized(), "should be initialized even with empty opts")
    end)
end


-- ---------------------------------------------------------------------------
-- Tests: Session functions without session
-- ---------------------------------------------------------------------------

function M.test_restore_session_returns_false_with_no_data()
    run_test("restore_session() returns false when no saved token", function()
        _saved_files = {}
        package.loaded["intelliversex.ivx"] = nil
        local ivx = require "intelliversex.ivx"
        ivx.configure({ debug = false })
        local restored = ivx.restore_session()
        assert_false(restored, "restore_session should return false with no data")
    end)
end

function M.test_clear_session_does_not_crash()
    run_test("clear_session() works without active session", function()
        package.loaded["intelliversex.ivx"] = nil
        local ivx = require "intelliversex.ivx"
        ivx.configure({ debug = false })
        ivx.clear_session()
        assert_false(ivx.has_valid_session(), "should have no session after clear")
        assert_eq(ivx.get_user_id(), "", "user_id should be empty after clear")
    end)
end

function M.test_disconnect_socket_safe_when_nil()
    run_test("disconnect_socket() does not crash when socket is nil", function()
        package.loaded["intelliversex.ivx"] = nil
        local ivx = require "intelliversex.ivx"
        ivx.configure({ debug = false })
        ivx.disconnect_socket()
        assert_true(true, "disconnect_socket did not crash")
    end)
end

function M.test_callback_registration()
    run_test("on() registers callbacks without error", function()
        package.loaded["intelliversex.ivx"] = nil
        local ivx = require "intelliversex.ivx"
        local called = false
        ivx.on("error", function(msg) called = true end)
        assert_true(true, "on() completed without error")
    end)
end

function M.test_error_callback_fires_when_not_initialized()
    run_test("auth emits error callback when not initialized", function()
        package.loaded["intelliversex.ivx"] = nil
        local ivx = require "intelliversex.ivx"
        local error_msg = nil
        ivx.on("error", function(msg) error_msg = msg end)
        ivx.authenticate_device("test-id")
        assert_not_nil(error_msg, "error callback should have fired")
        assert_eq(error_msg, "SDK not initialized", "error message")
    end)
end


-- ---------------------------------------------------------------------------
-- Runner
-- ---------------------------------------------------------------------------

function M.run_all()
    passed = 0
    failed = 0
    errors = {}

    print("\n========================================")
    print("  IntelliVerseX Defold SDK — Test Suite")
    print("========================================\n")

    M.test_module_loads()
    M.test_module_has_version()
    M.test_module_exports_all_functions()
    M.test_not_initialized_before_configure()
    M.test_has_no_session_before_configure()
    M.test_user_id_empty_before_configure()
    M.test_username_empty_before_configure()
    M.test_configure_sets_initialized()
    M.test_configure_accepts_custom_options()
    M.test_configure_defaults()
    M.test_restore_session_returns_false_with_no_data()
    M.test_clear_session_does_not_crash()
    M.test_disconnect_socket_safe_when_nil()
    M.test_callback_registration()
    M.test_error_callback_fires_when_not_initialized()

    print(string.format("\n----------------------------------------"))
    print(string.format("  Results: %d passed, %d failed", passed, failed))
    print(string.format("----------------------------------------"))

    if #errors > 0 then
        print("\nFailures:")
        for i, e in ipairs(errors) do
            print(string.format("  %d. %s\n     %s", i, e.name, e.err))
        end
    end

    print("")
    return failed == 0
end

-- Auto-run when executed directly
if not pcall(debug.getlocal, 4, 1) then
    local success = M.run_all()
    if not success then os.exit(1) end
end

return M
