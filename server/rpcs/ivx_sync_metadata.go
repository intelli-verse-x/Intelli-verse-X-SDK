// Copyright (c) 2026 Intelli-verse-X
// MIT License — see LICENSE in the project root.

package rpcs

import (
	"context"
	"database/sql"
	"encoding/json"

	"github.com/heroiclabs/nakama-common/runtime"
)

type syncMetadataRequest struct {
	Metadata map[string]interface{} `json:"metadata"`
}

const maxMetadataKeys = 200

// SyncMetadata stores SDK metadata (version, platform, engine) on the user account.
// Called by all 8 SDKs after successful authentication.
func SyncMetadata(ctx context.Context, logger runtime.Logger, db *sql.DB, nk runtime.NakamaModule, payload string) (string, error) {
	userID, err := requireAuthUser(ctx)
	if err != nil {
		return "", err
	}

	var req syncMetadataRequest
	if err := json.Unmarshal([]byte(payload), &req); err != nil {
		logger.Warn("ivx_sync_metadata: invalid payload from user %s: %v", userID, err)
		return "", runtime.NewError("invalid payload", 3)
	}

	if req.Metadata == nil {
		return "", runtime.NewError("metadata is required", 3)
	}
	if len(req.Metadata) > maxMetadataKeys {
		return "", runtime.NewError("metadata exceeds maximum allowed keys", 3)
	}

	account, err := nk.AccountGetId(ctx, userID)
	if err != nil {
		logger.Error("ivx_sync_metadata: failed to get account %s: %v", userID, err)
		return "", runtime.NewError("account lookup failed", 13)
	}

	existing := make(map[string]interface{})
	if account.GetUser().GetMetadata() != "" {
		_ = json.Unmarshal([]byte(account.GetUser().GetMetadata()), &existing)
	}

	for k, v := range req.Metadata {
		existing[k] = v
	}

	updated, err := json.Marshal(existing)
	if err != nil {
		return "", runtime.NewError("metadata could not be serialized", 3)
	}
	if err := nk.AccountUpdateId(ctx, userID, "", nil, "", "", "", "", string(updated)); err != nil {
		logger.Error("ivx_sync_metadata: failed to update account %s: %v", userID, err)
		return "", runtime.NewError("update failed", 13)
	}

	logger.Info("ivx_sync_metadata: updated metadata for user %s", userID)
	return `{"success":true}`, nil
}
