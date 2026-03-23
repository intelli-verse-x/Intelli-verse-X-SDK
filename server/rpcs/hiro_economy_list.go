// Copyright (c) 2026 Intelli-verse-X
// MIT License — see LICENSE in the project root.

package rpcs

import (
	"context"
	"database/sql"
	"encoding/json"

	"github.com/heroiclabs/nakama-common/runtime"
)

// EconomyList returns the authenticated user's wallet currencies.
// Called by all 8 SDKs to display the player's balance.
func EconomyList(ctx context.Context, logger runtime.Logger, db *sql.DB, nk runtime.NakamaModule, payload string) (string, error) {
	userID, ok := ctx.Value(runtime.RUNTIME_CTX_USER_ID).(string)
	if !ok || userID == "" {
		return `{"currencies":{}}`, nil
	}

	account, err := nk.AccountGetId(ctx, userID)
	if err != nil {
		logger.Error("hiro_economy_list: account lookup failed for %s: %v", userID, err)
		return `{"currencies":{}}`, nil
	}

	wallet := make(map[string]int64)
	if account.GetWallet() != "" {
		_ = json.Unmarshal([]byte(account.GetWallet()), &wallet)
	}

	resp := map[string]interface{}{
		"currencies": wallet,
	}
	out, _ := json.Marshal(resp)
	return string(out), nil
}
