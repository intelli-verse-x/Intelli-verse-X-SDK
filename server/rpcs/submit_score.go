// Copyright (c) 2026 Intelli-verse-X
// MIT License — see LICENSE in the project root.

package rpcs

import (
	"context"
	"database/sql"
	"encoding/json"

	"github.com/heroiclabs/nakama-common/runtime"
)

type submitScoreRequest struct {
	UserID        string            `json:"user_id"`
	Username      string            `json:"username"`
	DeviceID      string            `json:"device_id"`
	GameID        string            `json:"game_id"`
	Score         int64             `json:"score"`
	Subscore      int64             `json:"subscore"`
	CurrentStreak int               `json:"current_streak"`
	Metadata      map[string]string `json:"metadata,omitempty"`
}

type rewardDetails struct {
	BaseReward       int64   `json:"base_reward"`
	Score            int64   `json:"score"`
	Multiplier       float64 `json:"multiplier"`
	StreakMultiplier float64 `json:"streak_multiplier"`
	MilestoneBonus   int64   `json:"milestone_bonus"`
}

type submitScoreResponse struct {
	Success        bool           `json:"success"`
	Error          string         `json:"error,omitempty"`
	RewardEarned   bool           `json:"reward_earned"`
	RewardCurrency string         `json:"reward_currency"`
	WalletBalance  int64          `json:"wallet_balance"`
	RewardDetails  *rewardDetails `json:"reward_details,omitempty"`
}

const (
	baseRewardPerPoint = 10
	defaultLeaderboard = "weekly_high_scores"
	maxReasonableStreak = 10000
)

// SubmitScoreAndSync writes a leaderboard record and calculates a score-based reward.
// Unity SDK calls this after a game round to update scores and grant rewards.
func SubmitScoreAndSync(ctx context.Context, logger runtime.Logger, db *sql.DB, nk runtime.NakamaModule, payload string) (string, error) {
	userID, err := requireAuthUser(ctx)
	if err != nil {
		return "", err
	}

	var req submitScoreRequest
	if err := json.Unmarshal([]byte(payload), &req); err != nil {
		return "", runtime.NewError("invalid payload", 3)
	}

	if req.Score < 0 || req.Subscore < 0 {
		return "", runtime.NewError("score and subscore must be non-negative", 3)
	}
	if req.CurrentStreak < 0 || req.CurrentStreak > maxReasonableStreak {
		return "", runtime.NewError("current_streak out of range", 3)
	}

	leaderboardID := defaultLeaderboard
	if req.GameID != "" {
		leaderboardID = req.GameID + "_scores"
	}

	_ = nk.LeaderboardCreate(ctx, leaderboardID, false, "desc", "best", "0 0 * * 1", nil, false)

	meta := map[string]interface{}{}
	if req.Metadata != nil {
		for k, v := range req.Metadata {
			meta[k] = v
		}
	}
	_, err = nk.LeaderboardRecordWrite(ctx, leaderboardID, userID, req.Username, req.Score, req.Subscore, meta, nil)
	if err != nil {
		logger.Error("submit_score_and_sync: leaderboard write failed: %v", err)
		return "", runtime.NewError("leaderboard write failed", 13)
	}

	streakMul := 1.0 + float64(req.CurrentStreak)*0.1
	if streakMul > 3.0 {
		streakMul = 3.0
	}
	milestoneBonus := int64(0)
	if req.Score >= 1000 {
		milestoneBonus = 500
	} else if req.Score >= 500 {
		milestoneBonus = 200
	} else if req.Score >= 100 {
		milestoneBonus = 50
	}

	reward := int64(float64(req.Score*baseRewardPerPoint)*streakMul) + milestoneBonus

	changeset := map[string]int64{"coins": reward}
	if _, _, werr := nk.WalletUpdate(ctx, userID, changeset, nil, true); werr != nil {
		logger.Error("submit_score_and_sync: wallet update failed for %s: %v", userID, werr)
		return "", runtime.NewError("wallet update failed", 13)
	}

	account, _ := nk.AccountGetId(ctx, userID)
	walletBalance := int64(0)
	if account != nil && account.GetWallet() != "" {
		wallet := make(map[string]int64)
		_ = json.Unmarshal([]byte(account.GetWallet()), &wallet)
		walletBalance = wallet["coins"]
	}

	return marshalJSON(submitScoreResponse{
		Success:        true,
		RewardEarned:   true,
		RewardCurrency: "coins",
		WalletBalance:  walletBalance,
		RewardDetails: &rewardDetails{
			BaseReward:       req.Score * baseRewardPerPoint,
			Score:            req.Score,
			Multiplier:       1.0,
			StreakMultiplier: streakMul,
			MilestoneBonus:   milestoneBonus,
		},
	})
}

func marshalJSON(v interface{}) (string, error) {
	out, err := json.Marshal(v)
	return string(out), err
}
