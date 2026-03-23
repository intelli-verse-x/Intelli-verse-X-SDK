--- IntelliVerseX SDK for Defold
--- Central module for Nakama integration: auth, profile, wallet, leaderboards, storage, RPC.
--- @module ivx

local nakama = require "nakama.nakama"
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
--- @param opts table Configuration: host, port, server_key, use_ssl, debug
function M.configure(opts)
    config.host = opts.host or "127.0.0.1"
    config.port = opts.port or 7350
    config.server_key = opts.server_key or "defaultkey"
    config.use_ssl = opts.use_ssl or false
    config.debug = opts.debug or false

    local scheme = config.use_ssl and "https" or "http"
    client = nakama.create_client({
        host = config.host,
        port = config.port,
        server_key = config.server_key,
        use_ssl = config.use_ssl,
        engine = nakama.ENGINE_DEFOLD,
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
--- @param device_id string Optional device ID (auto-generated if nil)
function M.authenticate_device(device_id)
    if not initialized then
        _emit("error", "SDK not initialized")
        return
    end

    device_id = device_id or _get_persistent_device_id()

    nakama.authenticate_device(client, device_id, nil, true, nil, function(result)
        if result.error then
            _emit("auth_error", result.error.message or "Auth failed")
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
        _emit("error", "SDK not initialized")
        return
    end

    nakama.authenticate_email(client, email, password, nil, create or false, nil, function(result)
        if result.error then
            _emit("auth_error", result.error.message or "Auth failed")
            return
        end
        _on_auth_success(result)
    end)
end

--- Authenticate with Google token.
--- @param token string Google OAuth token
function M.authenticate_google(token)
    if not initialized then
        _emit("error", "SDK not initialized")
        return
    end

    nakama.authenticate_google(client, token, nil, true, nil, function(result)
        if result.error then
            _emit("auth_error", result.error.message or "Auth failed")
            return
        end
        _on_auth_success(result)
    end)
end

--- Authenticate with Apple token.
--- @param token string Apple Sign-In token
function M.authenticate_apple(token)
    if not initialized then
        _emit("error", "SDK not initialized")
        return
    end

    nakama.authenticate_apple(client, token, nil, true, nil, function(result)
        if result.error then
            _emit("auth_error", result.error.message or "Auth failed")
            return
        end
        _on_auth_success(result)
    end)
end

--- Authenticate with custom ID.
--- @param custom_id string
function M.authenticate_custom(custom_id)
    if not initialized then
        _emit("error", "SDK not initialized")
        return
    end

    nakama.authenticate_custom(client, custom_id, nil, true, nil, function(result)
        if result.error then
            _emit("auth_error", result.error.message or "Auth failed")
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
function M.clear_session()
    M.disconnect_socket()
    session = nil
    sys.save(SESSION_FILE, { token = "", refresh_token = "" })
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
    return session and session.user_id or ""
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
            return
        end

        local meta = result.user.metadata
        if type(meta) == "string" and meta ~= "" then
            local ok, decoded = pcall(json.decode, meta)
            if ok then meta = decoded end
        end

        local profile = {
            user_id = result.user.id,
            username = result.user.username,
            display_name = result.user.display_name,
            avatar_url = result.user.avatar_url,
            lang_tag = result.user.lang_tag,
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

--- Submit a leaderboard score.
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

    nakama.read_storage_objects(client, session, {
        { collection = collection, key = key, user_id = M.get_user_id() }
    }, function(result)
        if result.error then
            _emit("error", result.error.message)
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
--- @param rpc_id string
--- @param payload string JSON payload
--- @param callback function
function M.call_rpc(rpc_id, payload, callback)
    if not M.has_valid_session() then
        _emit("error", "No valid session")
        return
    end

    payload = payload or "{}"
    nakama.rpc(client, session, rpc_id, payload, function(result)
        if result.error then
            _emit("error", result.error.message)
            return
        end

        _log("RPC %s response received", rpc_id)
        local data = {}
        if result.payload then
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


-- Internal helpers

function _on_auth_success(result)
    if not result.token or result.token == "" then
        _emit("auth_error", "Authentication response missing token")
        return
    end

    session = nakama.session_create(result.token, result.refresh_token)
    sys.save(SESSION_FILE, { token = result.token, refresh_token = result.refresh_token })
    _log("Authenticated — UserId: %s", session.user_id or "unknown")
    _sync_metadata()
    _emit("auth_success", session)
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
