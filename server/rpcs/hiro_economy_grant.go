// Copyright (c) 2026 Intelli-verse-X
// MIT License — see LICENSE in the project root.

package rpcs

import (
	"context"
	"database/sql"
	"encoding/json"
	"strings"

	"github.com/heroiclabs/nakama-common/runtime"
)

const maxEconomyCurrenciesPerRequest = 32

type economyGrantRequest struct {
	Currencies map[string]int64 `json:"currencies"`
}

// EconomyGrant adds currencies to the authenticated user's wallet.
// Called by all 8 SDKs for reward payouts and in-game grants.
func EconomyGrant(ctx context.Context, logger runtime.Logger, db *sql.DB, nk runtime.NakamaModule, payload string) (string, error) {
	userID, err := requireAuthUser(ctx)
	if err != nil {
		return "", err
	}

	var req economyGrantRequest
	if err := json.Unmarshal([]byte(payload), &req); err != nil {
		return "", runtime.NewError("invalid payload", 3)
	}

	if req.Currencies == nil {
		return "", runtime.NewError("currencies is required", 3)
	}
	if len(req.Currencies) > maxEconomyCurrenciesPerRequest {
		return "", runtime.NewError("too many currency entries", 3)
	}

	if len(req.Currencies) == 0 {
		out, _ := json.Marshal(map[string]interface{}{
			"success":    true,
			"currencies": map[string]int64{},
		})
		return string(out), nil
	}

	changeset := make(map[string]int64)
	for currency, amount := range req.Currencies {
		currency = strings.TrimSpace(currency)
		if currency == "" {
			return "", runtime.NewError("currency id must be non-empty", 3)
		}
		if amount < 0 {
			return "", runtime.NewError("amount must be non-negative", 3)
		}
		changeset[currency] = amount
	}

	updated, _, err := nk.WalletUpdate(ctx, userID, changeset, nil, true)
	if err != nil {
		logger.Error("hiro_economy_grant: wallet update failed for %s: %v", userID, err)
		return "", runtime.NewError("wallet update failed", 13)
	}

	resp := map[string]interface{}{
		"success":    true,
		"currencies": updated,
	}
	out, _ := json.Marshal(resp)
	return string(out), nil
}
