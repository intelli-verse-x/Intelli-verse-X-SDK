--- IntelliVerseX Defold SDK — complete usage example.
---
--- Demonstrates:
---   1. SDK initialisation
---   2. Device authentication (with session restore)
---   3. Fetching and updating the player profile
---   4. Wallet operations (fetch / grant currency)
---   5. Leaderboard (submit score / fetch records)
---   6. Cloud storage (write / read)
---
--- Usage:
---   Attach this as a script component to a game object in your Defold project.
---   Make sure the IntelliVerseX module is in your project at intelliversex/ivx.lua
---   and the Nakama Defold client is installed as a dependency.

local ivx = require "intelliversex.ivx"

-- Configuration — adjust for your environment
local SERVER_HOST = "127.0.0.1"
local SERVER_PORT = 7350
local SERVER_KEY  = "defaultkey"
local USE_SSL     = false
local DEBUG_LOGS  = true

local LEADERBOARD_ID = "weekly_high_scores"


-- =========================================================================
-- Lifecycle
-- =========================================================================

function init(self)
    math.randomseed(os.time())
    _register_callbacks()
    _configure_sdk()
end

function final(self)
    ivx.disconnect_socket()
    print("[Example] Cleaned up")
end


-- =========================================================================
-- 1. SDK Initialisation
-- =========================================================================

function _configure_sdk()
    ivx.configure({
        host       = SERVER_HOST,
        port       = SERVER_PORT,
        server_key = SERVER_KEY,
        use_ssl    = USE_SSL,
        debug      = DEBUG_LOGS,
    })

    print("[Example] SDK initialized: " .. tostring(ivx.is_initialized()))
    _attempt_auth()
end


-- =========================================================================
-- 2. Authentication
-- =========================================================================

function _attempt_auth()
    if ivx.restore_session() then
        print("[Example] Session restored — user: " .. ivx.get_user_id())
        _post_auth_flow()
    else
        print("[Example] No saved session — authenticating with device ID…")
        ivx.authenticate_device()
    end
end


-- =========================================================================
-- 3. Callbacks
-- =========================================================================

function _register_callbacks()
    ivx.on("auth_success", function(session)
        print("[Example] Auth success! User ID: " .. ivx.get_user_id())
        print("[Example] Username: " .. ivx.get_username())
        _post_auth_flow()
    end)

    ivx.on("auth_error", function(message)
        print("[Example] Auth error: " .. tostring(message))
    end)

    ivx.on("profile", function(profile)
        print("[Example] [signal] Profile loaded for " .. tostring(profile.user_id))
    end)

    ivx.on("wallet", function(wallet)
        print("[Example] [signal] Wallet updated")
    end)

    ivx.on("error", function(message)
        print("[Example] ERROR: " .. tostring(message))
    end)
end


-- =========================================================================
-- 4. Post-auth workflow
-- =========================================================================

function _post_auth_flow()
    _demo_profile()
end


-- =========================================================================
-- 5. Profile
-- =========================================================================

function _demo_profile()
    print("\n--- Profile ---")

    ivx.fetch_profile(function(profile)
        if not profile then
            print("  (could not fetch profile)")
            _demo_wallet()
            return
        end

        print("  Display name : " .. tostring(profile.display_name))
        print("  Avatar URL   : " .. tostring(profile.avatar_url))
        print("  Language      : " .. tostring(profile.lang_tag))

        ivx.update_profile("DefoldPlayer", nil, "en")
        print("  Profile update requested")

        _demo_wallet()
    end)
end


-- =========================================================================
-- 6. Wallet
-- =========================================================================

function _demo_wallet()
    print("\n--- Wallet ---")

    ivx.fetch_wallet(function(wallet)
        print("  Current wallet:")
        if wallet then
            for k, v in pairs(wallet) do
                print("    " .. tostring(k) .. " = " .. tostring(v))
            end
        end

        ivx.grant_currency("coins", 100, function(result)
            print("  Granted 100 coins — result:")
            if result then
                for k, v in pairs(result) do
                    print("    " .. tostring(k) .. " = " .. tostring(v))
                end
            end

            _demo_leaderboard()
        end)
    end)
end


-- =========================================================================
-- 7. Leaderboard
-- =========================================================================

function _demo_leaderboard()
    print("\n--- Leaderboard ---")

    local score = math.random(500, 5000)
    ivx.submit_score(LEADERBOARD_ID, score, function(success)
        print("  Score " .. tostring(score) .. " submitted: " .. tostring(success))

        ivx.fetch_leaderboard(LEADERBOARD_ID, 10, function(records)
            if not records then
                print("  (no records returned)")
                _demo_storage()
                return
            end

            print("  Top " .. tostring(#records) .. " records:")
            for _, r in ipairs(records) do
                print(string.format("    #%s  %s — %d pts",
                    tostring(r.rank),
                    tostring(r.username or "???"),
                    r.score or 0
                ))
            end

            _demo_storage()
        end)
    end)
end


-- =========================================================================
-- 8. Cloud Storage
-- =========================================================================

function _demo_storage()
    print("\n--- Storage ---")

    local save_data = {
        level = 5,
        xp = 2350,
        inventory = { "sword", "shield", "potion" },
        last_save = os.date("%Y-%m-%dT%H:%M:%S"),
    }

    ivx.write_storage("player_saves", "slot_1", save_data, function(success)
        print("  Write OK: " .. tostring(success))

        ivx.read_storage("player_saves", "slot_1", function(loaded)
            if not loaded then
                print("  (nothing loaded)")
            else
                print("  Loaded save — level=" .. tostring(loaded.level) .. "  xp=" .. tostring(loaded.xp))
                if loaded.inventory then
                    print("  Inventory: " .. table.concat(loaded.inventory, ", "))
                end
            end

            print("\n--- All demos complete ---")
        end)
    end)
end
