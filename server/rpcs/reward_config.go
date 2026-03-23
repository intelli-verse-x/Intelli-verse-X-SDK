// Copyright (c) 2026 Intelli-verse-X
// MIT License — see LICENSE in the project root.

package rpcs

import (
	"context"
	"database/sql"
	"encoding/json"

	"github.com/heroiclabs/nakama-common/runtime"
)

type calculateRewardRequest struct {
	GameID        string `json:"game_id"`
	Score         int64  `json:"score"`
	CurrentStreak int    `json:"current_streak"`
}

type calculateRewardResponse struct {
	Success      bool   `json:"success"`
	RewardAmount int64  `json:"reward_amount"`
	Currency     string `json:"currency"`
}

type updateRewardConfigRequest struct {
	GameID string      `json:"game_id"`
	Config interface{} `json:"config"`
}

const rewardConfigCollection = "game_config"
const rewardConfigKey = "reward_config"

// CalculateScoreReward computes the reward for a given score and streak without persisting.
// Unity SDK calls this to preview potential rewards before submission.
func CalculateScoreReward(ctx context.Context, logger runtime.Logger, db *sql.DB, nk runtime.NakamaModule, payload string) (string, error) {
	var req calculateRewardRequest
	if err := json.Unmarshal([]byte(payload), &req); err != nil {
		return marshalJSON(calculateRewardResponse{Success: false})
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

	return marshalJSON(calculateRewardResponse{
		Success:      true,
		RewardAmount: reward,
		Currency:     "coins",
	})
}

// UpdateGameRewardConfig persists a game's reward configuration in Nakama storage.
// Unity SDK calls this from admin/editor tools to tune reward parameters.
func UpdateGameRewardConfig(ctx context.Context, logger runtime.Logger, db *sql.DB, nk runtime.NakamaModule, payload string) (string, error) {
	userID, ok := ctx.Value(runtime.RUNTIME_CTX_USER_ID).(string)
	if !ok || userID == "" {
		return `{"success":false,"error":"not authenticated"}`, nil
	}

	var req updateRewardConfigRequest
	if err := json.Unmarshal([]byte(payload), &req); err != nil {
		return `{"success":false,"error":"invalid payload"}`, nil
	}

	configJSON, _ := json.Marshal(req.Config)
	key := rewardConfigKey
	if req.GameID != "" {
		key = rewardConfigKey + "_" + req.GameID
	}

	_, err := nk.StorageWrite(ctx, []*runtime.StorageWrite{{
		Collection:      rewardConfigCollection,
		Key:             key,
		UserID:          userID,
		Value:           string(configJSON),
		PermissionRead:  2,
		PermissionWrite: 1,
	}})
	if err != nil {
		logger.Error("update_game_reward_config: storage write failed: %v", err)
		return `{"success":false,"error":"storage write failed"}`, nil
	}

	resp := map[string]interface{}{
		"success": true,
		"game_id": req.GameID,
		"config":  req.Config,
	}
	out, _ := json.Marshal(resp)
	return string(out), nil
}
