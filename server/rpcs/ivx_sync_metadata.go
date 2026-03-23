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

// SyncMetadata stores SDK metadata (version, platform, engine) on the user account.
// Called by all 8 SDKs after successful authentication.
func SyncMetadata(ctx context.Context, logger runtime.Logger, db *sql.DB, nk runtime.NakamaModule, payload string) (string, error) {
	userID, ok := ctx.Value(runtime.RUNTIME_CTX_USER_ID).(string)
	if !ok || userID == "" {
		return `{"success":false,"error":"not authenticated"}`, nil
	}

	var req syncMetadataRequest
	if err := json.Unmarshal([]byte(payload), &req); err != nil {
		logger.Warn("ivx_sync_metadata: invalid payload from user %s: %v", userID, err)
		return `{"success":false,"error":"invalid payload"}`, nil
	}

	account, err := nk.AccountGetId(ctx, userID)
	if err != nil {
		logger.Error("ivx_sync_metadata: failed to get account %s: %v", userID, err)
		return `{"success":false,"error":"account lookup failed"}`, nil
	}

	existing := make(map[string]interface{})
	if account.GetUser().GetMetadata() != "" {
		_ = json.Unmarshal([]byte(account.GetUser().GetMetadata()), &existing)
	}

	for k, v := range req.Metadata {
		existing[k] = v
	}

	updated, _ := json.Marshal(existing)
	if err := nk.AccountUpdateId(ctx, userID, "", nil, "", "", "", "", string(updated)); err != nil {
		logger.Error("ivx_sync_metadata: failed to update account %s: %v", userID, err)
		return `{"success":false,"error":"update failed"}`, nil
	}

	logger.Info("ivx_sync_metadata: updated metadata for user %s", userID)
	return `{"success":true}`, nil
}
