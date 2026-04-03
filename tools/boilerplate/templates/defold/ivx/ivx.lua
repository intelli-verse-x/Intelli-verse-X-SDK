-- Fake IVX module stub for templates
local ivx = {}
_G.ivx = ivx

function ivx.configure(opts)
    print("[IVX] Mock configure", opts.game_id)
end

function ivx.authenticate_guest(cb)
    print("[IVX] Mock authenticate_guest")
    if cb then cb(true) end
end

function ivx.load_hiro()
    print("[IVX] Mock load_hiro")
end

function ivx.track(event, data)
    print("[IVX] Mock track", event)
end

return ivx
