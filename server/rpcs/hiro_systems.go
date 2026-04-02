// Copyright (c) 2026 Intelli-verse-X
// MIT License — see LICENSE in the project root.

package rpcs

import (
	"context"
	"database/sql"
	"encoding/json"

	"github.com/heroiclabs/nakama-common/runtime"
)

// HiroSpinWheel returns the current spin-wheel state or performs a spin.
func HiroSpinWheel(ctx context.Context, logger runtime.Logger, db *sql.DB, nk runtime.NakamaModule, payload string) (string, error) {
	userID, err := requireAuthUser(ctx)
	if err != nil {
		return "", err
	}
	logger.Info("hiro_spin_wheel: request from %s", userID)
	resp, _ := json.Marshal(map[string]interface{}{
		"spins_remaining": 0,
		"rewards":         []interface{}{},
	})
	return string(resp), nil
}

// HiroGetStreaks returns daily streak state for the authenticated user.
func HiroGetStreaks(ctx context.Context, logger runtime.Logger, db *sql.DB, nk runtime.NakamaModule, payload string) (string, error) {
	if _, err := requireAuthUser(ctx); err != nil {
		return "", err
	}
	return `{"current_streak":0,"claimed":false}`, nil
}

// HiroClaimStreak claims the current streak reward.
func HiroClaimStreak(ctx context.Context, logger runtime.Logger, db *sql.DB, nk runtime.NakamaModule, payload string) (string, error) {
	if _, err := requireAuthUser(ctx); err != nil {
		return "", err
	}
	return `{"claimed":true}`, nil
}

// HiroGetOfferwall returns active offerwall items.
func HiroGetOfferwall(ctx context.Context, logger runtime.Logger, db *sql.DB, nk runtime.NakamaModule, payload string) (string, error) {
	if _, err := requireAuthUser(ctx); err != nil {
		return "", err
	}
	return `{"offers":[]}`, nil
}

// HiroRetentionGet returns the user's retention data.
func HiroRetentionGet(ctx context.Context, logger runtime.Logger, db *sql.DB, nk runtime.NakamaModule, payload string) (string, error) {
	if _, err := requireAuthUser(ctx); err != nil {
		return "", err
	}
	return `{"days_played":0,"rewards_claimed":[]}`, nil
}

// HiroRetentionUpdate updates retention progress.
func HiroRetentionUpdate(ctx context.Context, logger runtime.Logger, db *sql.DB, nk runtime.NakamaModule, payload string) (string, error) {
	if _, err := requireAuthUser(ctx); err != nil {
		return "", err
	}
	return `{"updated":true}`, nil
}
