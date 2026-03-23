# IntelliVerseX Nakama Server

Sample [Nakama](https://heroiclabs.com/nakama/) server runtime written in Go that implements all RPCs expected by the IntelliVerseX client SDKs.

## Prerequisites

- [Docker](https://docs.docker.com/get-docker/) and Docker Compose
- Go 1.22+ (only if building the plugin locally)

## Quick Start (Local Development)

```bash
cd server
docker-compose up -d
```

This starts:

| Service    | URL                          | Purpose                   |
|------------|------------------------------|---------------------------|
| Nakama API | http://localhost:7350         | HTTP / WebSocket endpoint |
| Console    | http://localhost:7351         | Admin dashboard           |
| gRPC       | localhost:7349               | gRPC endpoint             |
| PostgreSQL | localhost:5432               | Database                  |

Default console credentials: `admin` / `password`.

### Connecting SDKs (Local)

Override the default cloud host in your SDK config:

```
host:      127.0.0.1
port:      7350
serverKey: defaultkey
useSSL:    false
```

## Production (Cloud)

All IntelliVerseX SDKs default to the cloud Nakama instance:

```
host:      nakama-rest.intelli-verse-x.ai
port:      443
serverKey: defaultkey
useSSL:    true
```

No SDK configuration changes are needed for production.

## Implemented RPCs

### Shared (all 8 SDKs)

| RPC ID               | Description                                  |
|----------------------|----------------------------------------------|
| `ivx_sync_metadata`  | Stores SDK metadata (version, platform, engine) on the user account |
| `hiro_economy_list`  | Returns the user's wallet currencies         |
| `hiro_economy_grant` | Grants currencies to the user's wallet       |

### Unity-specific

| RPC ID                      | Description                                            |
|-----------------------------|--------------------------------------------------------|
| `create_or_sync_user`       | Creates or syncs a game-specific user identity         |
| `submit_score_and_sync`     | Submits a leaderboard score and calculates rewards     |
| `get_all_leaderboards`      | Returns daily, weekly, and all-time leaderboard data   |
| `get_wallet_balance`        | Returns the user's coin balance                        |
| `update_wallet_balance`     | Increments or sets the wallet balance                  |
| `calculate_score_reward`    | Previews the reward for a given score and streak       |
| `update_game_reward_config` | Persists game reward configuration in Nakama storage   |

## Project Structure

```
server/
  docker-compose.yml    Nakama 3.37 + PostgreSQL 16
  local.yml             Nakama runtime config
  go.mod                Go module definition
  main.go               InitModule: registers all 10 RPCs
  rpcs/
    ivx_sync_metadata.go
    hiro_economy_list.go
    hiro_economy_grant.go
    create_or_sync_user.go
    submit_score.go
    get_leaderboards.go
    wallet.go
    reward_config.go
  README.md             This file
```

## Reward System

The score reward formula used by `submit_score_and_sync` and `calculate_score_reward`:

```
reward = (score * 10) * streak_multiplier + milestone_bonus
```

- **Streak multiplier**: `1.0 + (current_streak * 0.1)`, capped at `3.0`
- **Milestone bonuses**: 50 coins at 100 pts, 200 at 500 pts, 500 at 1000 pts

## License

MIT License -- see [LICENSE](../LICENSE) in the project root.
