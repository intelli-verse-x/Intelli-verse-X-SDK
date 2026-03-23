// Copyright (c) 2026 Intelli-verse-X
// MIT License — see LICENSE in the project root.

package rpcs

import (
	"context"
	"database/sql"
	"encoding/json"

	"github.com/heroiclabs/nakama-common/runtime"
)

type economyGrantRequest struct {
	Currencies map[string]int64 `json:"currencies"`
}

// EconomyGrant adds currencies to the authenticated user's wallet.
// Called by all 8 SDKs for reward payouts and in-game grants.
func EconomyGrant(ctx context.Context, logger runtime.Logger, db *sql.DB, nk runtime.NakamaModule, payload string) (string, error) {
	userID, ok := ctx.Value(runtime.RUNTIME_CTX_USER_ID).(string)
	if !ok || userID == "" {
		return `{"success":false,"error":"not authenticated"}`, nil
	}

	var req economyGrantRequest
	if err := json.Unmarshal([]byte(payload), &req); err != nil {
		return `{"success":false,"error":"invalid payload"}`, nil
	}

	changeset := make(map[string]int64)
	for currency, amount := range req.Currencies {
		changeset[currency] = amount
	}

	updated, _, err := nk.WalletUpdate(ctx, userID, changeset, nil, true)
	if err != nil {
		logger.Error("hiro_economy_grant: wallet update failed for %s: %v", userID, err)
		return `{"success":false,"error":"wallet update failed"}`, nil
	}

	resp := map[string]interface{}{
		"success":    true,
		"currencies": updated,
	}
	out, _ := json.Marshal(resp)
	return string(out), nil
}
