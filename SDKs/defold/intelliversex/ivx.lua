--- IntelliVerseX SDK for Defold
--- Central module for Nakama integration: auth, profile, wallet, leaderboards, storage, RPC.
--- @module ivx

local nakama = require "nakama.nakama"
local defold_engine = require "nakama.engine.defold"
local json = require "nakama.util.json"
local log = require "nakama.util.log"

local M = {}

M.SDK_VERSION = "5.1.0"

local config = {}
local client = nil
local session = nil
local socket = nil
local initialized = false
local callbacks = {}

local SESSION_FILE = sys.get_save_file("intelliversex", "session")

--- Configure the SDK.
--- Matches Unity SDK: host, port, server_key, use_ssl, debug, game_id (optional, for create_or_sync_user RPC).
--- @param opts table Configuration: host, port, server_key, use_ssl, debug, game_id
function M.configure(opts)
    config.host = opts.host or "127.0.0.1"
    config.port = opts.port or 7350
    config.server_key = opts.server_key or "defaultkey"
    config.use_ssl = opts.use_ssl or false
    config.debug = opts.debug or false
    config.game_id = opts.game_id or ""

    local scheme = config.use_ssl and "https" or "http"
    client = nakama.create_client({
        host = config.host,
        port = config.port,
        server_key = config.server_key,
        use_ssl = config.use_ssl,
        engine = defold_engine,
    })

    initialized = true
    _log("SDK initialized — %s://%s:%d", scheme, config.host, config.port)
end

--- Check whether the SDK has been initialized via configure().
--- @return boolean
function M.is_initialized()
    return initialized
end

--- Register a callback: "auth_success", "auth_error", "error", "profile", "wallet"
--- @param event string Event name
--- @param fn function Callback function
function M.on(event, fn)
    callbacks[event] = fn
end

--- Authenticate with device ID.
--- Flow matches Unity: persistent device ID, then create_or_sync_user after success.
--- @param device_id string Optional device ID (auto-generated if nil)
function M.authenticate_device(device_id)
    if not initialized then
        _emit("auth_error", "SDK not initialized")
        return
    end

    device_id = device_id or _get_persistent_device_id()
    if device_id == "" then
        _emit("auth_error", "Device ID required")
        return
    end

    nakama.authenticate_device(client, device_id, nil, true, nil, function(result)
        if result.error then
            _emit("auth_error", _normalize_auth_error(result.error.message))
            return
        end
        _on_auth_success(result)
    end)
end

--- Authenticate with email/password.
--- @param email string
--- @param password string
--- @param create boolean Create account if not exists
function M.authenticate_email(email, password, create)
    if not initialized then
        _emit("auth_error", "SDK not initialized")
        return
    end
    if not email or email == "" or not password or password == "" then
        _emit("auth_error", "Email and password required")
        return
    end

    nakama.authenticate_email(client, email, password, nil, create or false, nil, function(result)
        if result.error then
            _emit("auth_error", _normalize_auth_error(result.error.message))
            return
        end
        _on_auth_success(result)
    end)
end

--- Authenticate with Google token.
--- @param token string Google OAuth token (required)
function M.authenticate_google(token)
    if not initialized then
        _emit("auth_error", "SDK not initialized")
        return
    end
    if not token or token == "" then
        _emit("auth_error", "Google token required")
        return
    end

    nakama.authenticate_google(client, token, nil, true, nil, function(result)
        if result.error then
            _emit("auth_error", _normalize_auth_error(result.error.message))
            return
        end
        _on_auth_success(result)
    end)
end

--- Authenticate with Apple token.
--- @param token string Apple Sign-In token (required)
function M.authenticate_apple(token)
    if not initialized then
        _emit("auth_error", "SDK not initialized")
        return
    end
    if not token or token == "" then
        _emit("auth_error", "Apple token required")
        return
    end

    nakama.authenticate_apple(client, token, nil, true, nil, function(result)
        if result.error then
            _emit("auth_error", _normalize_auth_error(result.error.message))
            return
        end
        _on_auth_success(result)
    end)
end

--- Authenticate with custom ID.
--- @param custom_id string (required)
function M.authenticate_custom(custom_id)
    if not initialized then
        _emit("auth_error", "SDK not initialized")
        return
    end
    if not custom_id or custom_id == "" then
        _emit("auth_error", "Custom ID required")
        return
    end

    nakama.authenticate_custom(client, custom_id, nil, true, nil, function(result)
        if result.error then
            _emit("auth_error", _normalize_auth_error(result.error.message))
            return
        end
        _on_auth_success(result)
    end)
end

--- Try to restore a previously saved session.
--- @return boolean True if session was restored
function M.restore_session()
    local saved = sys.load(SESSION_FILE)
    if saved and saved.token and saved.token ~= "" then
        session = nakama.session_restore(saved.token, saved.refresh_token)
        if session and not nakama.session_is_expired(session) then
            _log("Session restored for user: %s", session.user_id or "unknown")
            _sync_metadata()
            return true
        end
    end
    return false
end

--- Clear the current session and disconnect the socket.
--- Preserves device_id for next device auth (matches Unity flow).
function M.clear_session()
    M.disconnect_socket()
    session = nil
    local saved = sys.load(SESSION_FILE) or {}
    saved.token = ""
    saved.refresh_token = ""
    sys.save(SESSION_FILE, saved)
    _log("Session cleared")
end

--- Disconnect the real-time socket if connected.
function M.disconnect_socket()
    if socket then
        nakama.socket_disconnect(socket)
        socket = nil
        _log("Socket disconnected")
    end
end

--- Check if we have a valid session.
--- @return boolean
function M.has_valid_session()
    return session ~= nil and not nakama.session_is_expired(session)
end

--- Get current user ID.
--- @return string
function M.get_user_id()
    if not session then return "" end
    return session.user_id or ""
end

--- Get current username.
--- @return string
function M.get_username()
    return session and session.username or ""
end

--- Fetch the current user's profile.
--- @param callback function Receives profile table
function M.fetch_profile(callback)
    if not M.has_valid_session() then
        _emit("error", "No valid session")
        return
    end

    nakama.get_account(client, session, function(result)
        if result.error then
            _emit("error", result.error.message)
            if callback then callback(nil) end
            return
        end

        local meta = result.user and result.user.metadata
        if type(meta) == "string" and meta ~= "" then
            local ok, decoded = pcall(json.decode, meta)
            if ok then meta = decoded end
        end

        local user = result.user or {}
        local profile = {
            user_id = user.id,
            username = user.username,
            display_name = user.display_name,
            avatar_url = user.avatar_url,
            lang_tag = user.lang_tag,
            metadata = meta,
            wallet = result.wallet,
        }

        _emit("profile", profile)
        if callback then callback(profile) end
    end)
end

--- Update profile fields.
--- @param display_name string
--- @param avatar_url string
--- @param lang_tag string
function M.update_profile(display_name, avatar_url, lang_tag)
    if not M.has_valid_session() then
        _emit("error", "No valid session")
        return
    end

    nakama.update_account(client, session, nil, display_name, avatar_url, lang_tag, nil, function(result)
        if result.error then
            _emit("error", result.error.message)
            return
        end
        _log("Profile updated")
    end)
end

--- Fetch wallet via Hiro economy RPC.
--- @param callback function
function M.fetch_wallet(callback)
    M.call_rpc("hiro_economy_list", "{}", function(result)
        _emit("wallet", result)
        if callback then callback(result) end
    end)
end

--- Grant currency via Hiro economy RPC.
--- @param currency_id string
--- @param amount number
--- @param callback function
function M.grant_currency(currency_id, amount, callback)
    local payload = json.encode({ currencies = { [currency_id] = amount } })
    M.call_rpc("hiro_economy_grant", payload, callback)
end

--- Submit a leaderboard score (Nakama native — single leaderboard).
--- @param leaderboard_id string
--- @param score number
--- @param callback function Optional callback receiving boolean success
function M.submit_score(leaderboard_id, score, callback)
    if not M.has_valid_session() then
        _emit("error", "No valid session")
        if callback then callback(false) end
        return
    end

    nakama.write_leaderboard_record(client, session, leaderboard_id, score, nil, nil, nil, function(result)
        if result.error then
            _emit("error", result.error.message)
            if callback then callback(false) end
            return
        end
        _log("Score submitted: %d to %s", score, leaderboard_id)
        if callback then callback(true) end
    end)
end

--- Submit score via backend RPC (same as Unity submit_score_and_sync).
--- Use this when your backend provides submit_score_and_sync (rewards, wallet sync, multi-leaderboard).
--- @param score number
--- @param callback function Optional callback(success, data) — data has reward_earned, wallet_balance, error, etc.
function M.submit_score_and_sync(score, callback)
    if not M.has_valid_session() then
        _emit("error", "No valid session")
        if callback then callback(false, { error = "No valid session" }) end
        return
    end

    local device_id = _get_persistent_device_id()
    local username = session.username or ("Player_" .. (session.user_id and string.sub(session.user_id, 1, 8) or "unknown"))
    local payload = json.encode({
        user_id = session.user_id,
        username = username,
        device_id = device_id,
        game_id = config.game_id or "",
        score = score,
        subscore = 0,
        current_streak = 0,
        metadata = {},
    })

    nakama.rpc(client, session, "submit_score_and_sync", payload, function(result)
        if result.error then
            _emit("error", result.error.message)
            if callback then callback(false, { error = result.error.message }) end
            return
        end
        local data = {}
        if result.payload and result.payload ~= "" then
            local ok, decoded = pcall(json.decode, result.payload)
            if ok and decoded then data = decoded end
        end
        local success = data.success == true
        if callback then callback(success, data) end
    end)
end

--- Fetch all leaderboards via backend RPC (same as Unity get_all_leaderboards).
--- Use when your backend provides this RPC; otherwise use fetch_leaderboard for a single board.
--- @param limit number Optional, default 50 (or pass callback only: fetch_all_leaderboards(callback))
--- @param callback function Receives (data) — data.success, data.leaderboards or data.error
function M.fetch_all_leaderboards(limit_or_callback, callback)
    local limit = 50
    if type(limit_or_callback) == "function" then
        callback = limit_or_callback
    else
        limit = limit_or_callback or 50
    end

    if not M.has_valid_session() then
        _emit("error", "No valid session")
        if callback then callback({ success = false, error = "No valid session" }) end
        return
    end
    local device_id = _get_persistent_device_id()
    local payload = json.encode({
        user_id = session.user_id,
        device_id = device_id,
        game_id = config.game_id or "",
        limit = limit,
    })

    nakama.rpc(client, session, "get_all_leaderboards", payload, function(result)
        if result.error then
            _emit("error", result.error.message)
            if callback then callback({ success = false, error = result.error.message }) end
            return
        end
        local data = {}
        if result.payload and result.payload ~= "" then
            local ok, decoded = pcall(json.decode, result.payload)
            if ok and decoded then data = decoded else data = { success = false, error = "Invalid response" } end
        else
            data = { success = false, error = "Empty response" }
        end
        if callback then callback(data) end
    end)
end

--- Fetch wallet balances via backend RPC (same as Unity wallet_get_balances).
--- Use when your backend uses this RPC; otherwise use fetch_wallet (Hiro hiro_economy_list).
--- @param callback function Receives (data) — data.success, data.game_balance, data.global_balance or data.error
function M.fetch_wallet_balances(callback)
    if not M.has_valid_session() then
        _emit("error", "No valid session")
        if callback then callback({ success = false, error = "No valid session" }) end
        return
    end

    local payload = json.encode({ gameId = config.game_id or "" })

    nakama.rpc(client, session, "wallet_get_balances", payload, function(result)
        if result.error then
            _emit("error", result.error.message)
            if callback then callback({ success = false, error = result.error.message }) end
            return
        end
        local data = {}
        if result.payload and result.payload ~= "" then
            local ok, decoded = pcall(json.decode, result.payload)
            if ok and decoded then data = decoded else data = { success = false, error = "Invalid response" } end
        else
            data = { success = false, error = "Empty response" }
        end
        if callback then callback(data) end
    end)
end

--- Fetch leaderboard records.
--- @param leaderboard_id string
--- @param limit number
--- @param callback function
function M.fetch_leaderboard(leaderboard_id, limit, callback)
    if not M.has_valid_session() then
        _emit("error", "No valid session")
        return
    end

    limit = limit or 20
    nakama.list_leaderboard_records(client, session, leaderboard_id, nil, nil, limit, nil, function(result)
        if result.error then
            _emit("error", result.error.message)
            if callback then callback(nil) end
            return
        end

        local records = {}
        for _, r in ipairs(result.records or {}) do
            table.insert(records, {
                owner_id = r.owner_id,
                username = r.username,
                score = r.score,
                rank = r.rank,
            })
        end
        if callback then callback(records) end
    end)
end

--- Write a storage object.
--- @param collection string
--- @param key string
--- @param value table
--- @param callback function Optional callback receiving boolean success
function M.write_storage(collection, key, value, callback)
    if not M.has_valid_session() then
        _emit("error", "No valid session")
        if callback then callback(false) end
        return
    end

    nakama.write_storage_objects(client, session, {
        { collection = collection, key = key, value = json.encode(value), permission_read = 1, permission_write = 1 }
    }, function(result)
        if result.error then
            _emit("error", result.error.message)
            if callback then callback(false) end
            return
        end
        _log("Storage write complete: %s/%s", collection, key)
        if callback then callback(true) end
    end)
end

--- Read a storage object.
--- @param collection string
--- @param key string
--- @param callback function
function M.read_storage(collection, key, callback)
    if not M.has_valid_session() then
        _emit("error", "No valid session")
        return
    end

    local user_id = M.get_user_id()
    nakama.read_storage_objects(client, session, {
        { collection = collection, key = key, user_id = user_id }
    }, function(result)
        if result.error then
            _emit("error", result.error.message)
            if callback then callback(nil) end
            return
        end

        if result.objects and #result.objects > 0 then
            local ok, data = pcall(json.decode, result.objects[1].value)
            if not ok then
                _emit("error", "Failed to decode storage data for " .. collection .. "/" .. key)
                if callback then callback(nil) end
                return
            end
            if callback then callback(data) end
        else
            if callback then callback(nil) end
        end
    end)
end

--- Call an RPC endpoint.
--- Production-ready: callback is always invoked (with data or { error = message } on RPC failure).
--- @param rpc_id string
--- @param payload string JSON payload
--- @param callback function Receives (data) — data may contain .error on failure
function M.call_rpc(rpc_id, payload, callback)
    if not M.has_valid_session() then
        _emit("error", "No valid session")
        if callback then callback({ error = "No valid session" }) end
        return
    end

    if not client or not session then
        _emit("error", "SDK not ready")
        if callback then callback({ error = "SDK not ready" }) end
        return
    end

    payload = payload or "{}"
    nakama.rpc(client, session, rpc_id, payload, function(result)
        if result.error then
            local msg = result.error.message or "RPC failed"
            _emit("error", msg)
            if callback then callback({ error = msg }) end
            return
        end

        _log("RPC %s response received", rpc_id)
        local data = {}
        if result.payload and result.payload ~= "" then
            local ok, decoded = pcall(json.decode, result.payload)
            data = ok and decoded or {}
            if not ok then
                _log("Warning: failed to decode RPC response for %s", rpc_id)
            end
        end
        if callback then callback(data) end
    end)
end

--- Connect the real-time socket.
--- @param callback function
function M.connect_socket(callback)
    if not M.has_valid_session() then
        _emit("error", "No valid session")
        return
    end

    socket = nakama.create_socket(client)
    nakama.socket_connect(socket, session, function(result)
        if result.error then
            _emit("error", "Socket connection failed")
            return
        end
        _log("Socket connected")
        if callback then callback() end
    end)
end


-- Internal helpers (aligned with Unity SDK auth flow)

local RPC_CREATE_OR_SYNC_USER = "create_or_sync_user"

function _on_auth_success(result)
    if not result.token or result.token == "" then
        _emit("auth_error", "Authentication response missing token")
        return
    end

    session = nakama.session_create(result.token, result.refresh_token)
    -- Merge save so device_id is preserved (matches Unity session persistence)
    local saved = sys.load(SESSION_FILE) or {}
    saved.token = result.token
    saved.refresh_token = result.refresh_token or ""
    sys.save(SESSION_FILE, saved)

    _log("Authenticated — UserId: %s", session.user_id or "unknown")
    _sync_metadata()
    -- Unity calls create_or_sync_user after auth; emit auth_success only after identity sync
    _sync_user_identity(function(success, err_msg)
        if success then
            _emit("auth_success", session)
        else
            _emit("auth_error", err_msg or "Identity sync failed")
        end
    end)
end

--- Call create_or_sync_user RPC (same as Unity IVXNakamaManager.SyncUserIdentity).
--- Payload: username, user_id, platform_user_id, device_id, game_id.
function _sync_user_identity(callback)
    if not session or not M.has_valid_session() then
        if callback then callback(false, "No valid session") end
        return
    end
    local device_id = _get_persistent_device_id()
    local username = session.username or ("Player_" .. (session.user_id and string.sub(session.user_id, 1, 8) or "unknown"))
    local payload = json.encode({
        username = username,
        user_id = session.user_id,
        platform_user_id = session.user_id,
        device_id = device_id,
        game_id = config.game_id or "",
    })
    nakama.rpc(client, session, RPC_CREATE_OR_SYNC_USER, payload, function(result)
        if result.error then
            local msg = result.error.message or ""
            -- If server has no create_or_sync_user RPC, still succeed (minimal backend)
            if string.find(string.lower(msg), "not found") or string.find(string.lower(msg), "unknown") then
                _log("create_or_sync_user not on server — continuing (auth success)")
                if callback then callback(true, nil) end
                return
            end
            _log("create_or_sync_user failed: %s", msg)
            if callback then callback(false, msg) end
            return
        end
        local data = {}
        if result.payload and result.payload ~= "" then
            local ok, decoded = pcall(json.decode, result.payload)
            if ok and decoded then data = decoded end
        end
        local success = data.success == true or data.success == nil
        if not success and data.error then
            _log("create_or_sync_user error: %s", data.error)
        end
        if callback then callback(success, data.error) end
    end)
end

--- Normalize auth error messages for consistent UX (Unity-style).
function _normalize_auth_error(msg)
    if not msg or msg == "" then return "Authentication failed" end
    local lower = string.lower(msg)
    if string.find(lower, "invalid") and string.find(lower, "credential") then
        return "Invalid email or password"
    end
    if string.find(lower, "user not found") or string.find(lower, "account") then
        return "Account not found"
    end
    return msg
end

function _sync_metadata()
    if not M.has_valid_session() then return end

    local meta = {
        sdk_version = M.SDK_VERSION,
        platform = sys.get_sys_info().system_name,
        engine = "defold",
        engine_version = sys.get_engine_info().version,
    }

    M.call_rpc("ivx_sync_metadata", json.encode({ metadata = meta }))
end

function _get_persistent_device_id()
    local saved = sys.load(SESSION_FILE) or {}
    if saved.device_id and saved.device_id ~= "" then
        return saved.device_id
    end

    local info = sys.get_sys_info()
    local id = info.device_ident ~= "" and info.device_ident or _uuid()
    saved.device_id = id
    sys.save(SESSION_FILE, saved)
    return id
end

function _uuid()
    local template = "xxxxxxxx-xxxx-4xxx-yxxx-xxxxxxxxxxxx"
    return string.gsub(template, "[xy]", function(c)
        local v = (c == "x") and math.random(0, 15) or math.random(8, 11)
        return string.format("%x", v)
    end)
end

function _emit(event, ...)
    if callbacks[event] then
        callbacks[event](...)
    end
end

function _log(fmt, ...)
    if config.debug then
        print(string.format("[IntelliVerseX] " .. fmt, ...))
    end
end

return M
