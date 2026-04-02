// Copyright (c) 2026 Intelli-verse-X
// MIT License — see LICENSE in the project root.

package rpcs

import (
	"context"
	"database/sql"
	"encoding/json"

	"github.com/heroiclabs/nakama-common/runtime"
)

type updateWalletRequest struct {
	DeviceID   string `json:"device_id"`
	GameID     string `json:"game_id"`
	Amount     int64  `json:"amount"`
	WalletType string `json:"wallet_type"`
	ChangeType string `json:"change_type"`
}

// GetWalletBalance returns the total coin balance for the authenticated user.
// Unity SDK calls this to display the player's wallet.
func GetWalletBalance(ctx context.Context, logger runtime.Logger, db *sql.DB, nk runtime.NakamaModule, payload string) (string, error) {
	userID, err := requireAuthUser(ctx)
	if err != nil {
		return "", err
	}

	account, err := nk.AccountGetId(ctx, userID)
	if err != nil {
		logger.Error("get_wallet_balance: account lookup failed for %s: %v", userID, err)
		return "", runtime.NewError("account lookup failed", 13) // INTERNAL
	}

	wallet := make(map[string]int64)
	if account.GetWallet() != "" {
		_ = json.Unmarshal([]byte(account.GetWallet()), &wallet)
	}

	balance := wallet["coins"]
	resp := map[string]interface{}{"balance": balance}
	out, _ := json.Marshal(resp)
	return string(out), nil
}

// UpdateWalletBalance increments or sets the wallet balance.
// Unity SDK calls this for reward payouts and purchase deductions.
func UpdateWalletBalance(ctx context.Context, logger runtime.Logger, db *sql.DB, nk runtime.NakamaModule, payload string) (string, error) {
	userID, err := requireAuthUser(ctx)
	if err != nil {
		return "", err
	}

	var req updateWalletRequest
	if err := json.Unmarshal([]byte(payload), &req); err != nil {
		return "", runtime.NewError("invalid payload", 3)
	}

	if req.Amount < 0 {
		return "", runtime.NewError("amount must be non-negative", 3)
	}

	changeset := map[string]int64{"coins": req.Amount}
	if req.ChangeType == "set" {
		account, err := nk.AccountGetId(ctx, userID)
		if err == nil && account.GetWallet() != "" {
			wallet := make(map[string]int64)
			_ = json.Unmarshal([]byte(account.GetWallet()), &wallet)
			changeset["coins"] = req.Amount - wallet["coins"]
		}
	}

	_, _, err = nk.WalletUpdate(ctx, userID, changeset, nil, true)
	if err != nil {
		logger.Error("update_wallet_balance: failed for %s: %v", userID, err)
		return "", runtime.NewError("wallet update failed", 13)
	}

	return `{"success":true}`, nil
}
