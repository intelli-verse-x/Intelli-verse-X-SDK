// Copyright (c) 2026 Intelli-verse-X
// MIT License — see LICENSE in the project root.

package rpcs

import (
	"context"
	"database/sql"
	"encoding/json"
	"fmt"
	"strings"

	"github.com/heroiclabs/nakama-common/runtime"
)

type createOrSyncUserRequest struct {
	Username       string `json:"username"`
	UserID         string `json:"user_id"`
	PlatformUserID string `json:"platform_user_id"`
	DeviceID       string `json:"device_id"`
	GameID         string `json:"game_id"`
}

type createOrSyncUserResponse struct {
	Success        bool   `json:"success"`
	Created        bool   `json:"created"`
	Username       string `json:"username"`
	WalletID       string `json:"wallet_id"`
	GlobalWalletID string `json:"global_wallet_id"`
	Error          string `json:"error,omitempty"`
}

// CreateOrSyncUser creates or syncs a user identity in the game backend.
// Unity SDK calls this after Nakama authentication to set up game-specific state.
func CreateOrSyncUser(ctx context.Context, logger runtime.Logger, db *sql.DB, nk runtime.NakamaModule, payload string) (string, error) {
	userID, err := requireAuthUser(ctx)
	if err != nil {
		return "", err
	}

	var req createOrSyncUserRequest
	if err := json.Unmarshal([]byte(payload), &req); err != nil {
		return "", runtime.NewError("invalid payload", 3)
	}

	req.GameID = strings.TrimSpace(req.GameID)
	if req.GameID == "" {
		return "", runtime.NewError("game_id is required", 3)
	}

	objects, err := nk.StorageRead(ctx, []*runtime.StorageRead{{
		Collection: "game_users",
		Key:        req.GameID,
		UserID:     userID,
	}})

	created := false
	if err != nil || len(objects) == 0 {
		created = true
		data, _ := json.Marshal(map[string]interface{}{
			"username":  req.Username,
			"device_id": req.DeviceID,
			"game_id":   req.GameID,
		})
		_, err = nk.StorageWrite(ctx, []*runtime.StorageWrite{{
			Collection:      "game_users",
			Key:             req.GameID,
			UserID:          userID,
			Value:           string(data),
			PermissionRead:  1,
			PermissionWrite: 1,
		}})
		if err != nil {
			logger.Error("create_or_sync_user: storage write failed: %v", err)
			return "", runtime.NewError("storage write failed", 13)
		}
	}

	walletID := fmt.Sprintf("wallet_%s_%s", userID, req.GameID)
	globalWalletID := fmt.Sprintf("global_wallet_%s", userID)

	resp := createOrSyncUserResponse{
		Success:        true,
		Created:        created,
		Username:       req.Username,
		WalletID:       walletID,
		GlobalWalletID: globalWalletID,
	}
	out, _ := json.Marshal(resp)
	return string(out), nil
}
