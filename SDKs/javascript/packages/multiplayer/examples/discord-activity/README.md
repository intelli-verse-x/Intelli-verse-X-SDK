# IVX Discord Activity sample

A drop-in Discord Activity that uses the IVX multiplayer adapter for
non-realtime kernel templates (lobby, conversational-party, sync-turn).
Voice is owned by Discord — IVX does not publish its own voice
provider in this surface (capability `none`).

## Setup

1. Create a Discord app at https://discord.com/developers/applications.
2. Enable the `Activities` capability and register an Activity URL
   pointing at the host you'll deploy this sample on.
3. In your app dashboard add an OAuth redirect for the activity origin.
4. Implement `/api/discord/token` on your backend that swaps the OAuth
   `code` for an `access_token` using the app's `client_secret`.
5. Set `window.DISCORD_CLIENT_ID = "<your-client-id>"` before this
   page loads (or hard-code it in `index.html`).
6. Deploy the page over HTTPS — Discord requires it.

## Architecture

```
Discord client ── iframe ──> activity (this page)
                                │
                                ▼
                         IVXDiscordActivityAdapter
                                │
                                ▼
                  IVX multiplayer client (Nakama-JS)
                                │
                                ▼
                       Nakama (kernel templates)
```

The adapter:

* Authorizes via `@discord/embedded-app-sdk`.
* Exchanges the OAuth code for an access token via your backend.
* Authenticates Nakama using the Discord user id as the device id
  (idempotent across activity instances).
* Tags every match with `discord_application_id`,
  `discord_channel_id`, and `discord_instance_id` in the template
  init so kernel-side moderation can correlate IVX rooms with Discord
  channels.

## Limitations vs. native Unity / Web clients

| Feature | Discord Activity | Notes |
|---------|------------------|-------|
| Realtime tick (60Hz) | ✅ | WebSocket allowed by Discord CSP |
| Voice (LiveKit) | ❌ | Discord owns audio; capability advertised as `none` |
| Avatar replication | ✅ | Best-effort; iframe perf varies |
| File downloads | ⚠ | Limited by Discord CSP — host on origin |
