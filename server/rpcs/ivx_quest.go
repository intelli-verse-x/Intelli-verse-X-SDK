// Copyright (c) 2026 Intelli-verse-X
// MIT License — see LICENSE in the project root.

package rpcs

import (
	"context"
	"database/sql"
	"encoding/json"
	"fmt"
	"time"

	"github.com/heroiclabs/nakama-common/runtime"
)

const (
	questStorageCollection = "ivx_quests"
	questConfigKey         = "quest_config"
	questProgressKey       = "quest_progress"
)

type questProgressPayload struct {
	EventName string            `json:"event_name"`
	Value     int               `json:"value"`
	Metadata  map[string]string `json:"metadata"`
}

type questClaimPayload struct {
	QuestID string `json:"quest_id"`
}

type questGetPayload struct {
	QuestType string `json:"quest_type"`
}

type questEntry struct {
	QuestID         string                 `json:"quest_id"`
	Title           string                 `json:"title"`
	Description     string                 `json:"description"`
	QuestType       string                 `json:"quest_type"`
	Status          string                 `json:"status"`
	TargetProgress  int                    `json:"target_progress"`
	CurrentProgress int                    `json:"current_progress"`
	Rewards         []map[string]interface{} `json:"rewards"`
	ExpiresAt       string                 `json:"expires_at"`
}

type userQuestState struct {
	Quests    []questEntry `json:"quests"`
	UpdatedAt string       `json:"updated_at"`
}

// IVXQuestGet returns the current quests for the authenticated user.
func IVXQuestGet(ctx context.Context, logger runtime.Logger, db *sql.DB, nk runtime.NakamaModule, payload string) (string, error) {
	userID, err := requireAuthUser(ctx)
	if err != nil {
		return "", err
	}
	logger.Info("ivx_quest_get: request from %s", userID)

	var req questGetPayload
	if payload != "" && payload != "{}" {
		if err := json.Unmarshal([]byte(payload), &req); err != nil {
			return "", fmt.Errorf("invalid payload: %w", err)
		}
	}

	state, err := loadQuestState(ctx, nk, userID, logger)
	if err != nil {
		return "", err
	}

	quests := state.Quests
	if req.QuestType != "" {
		var filtered []questEntry
		for _, q := range quests {
			if q.QuestType == req.QuestType {
				filtered = append(filtered, q)
			}
		}
		quests = filtered
	}

	resp, _ := json.Marshal(map[string]interface{}{
		"quests":    quests,
		"resets_at": nextDailyReset(),
	})
	return string(resp), nil
}

// IVXQuestProgress processes a game event and advances matching quest progress.
func IVXQuestProgress(ctx context.Context, logger runtime.Logger, db *sql.DB, nk runtime.NakamaModule, payload string) (string, error) {
	userID, err := requireAuthUser(ctx)
	if err != nil {
		return "", err
	}

	var req questProgressPayload
	if err := json.Unmarshal([]byte(payload), &req); err != nil {
		return "", fmt.Errorf("invalid payload: %w", err)
	}
	if req.EventName == "" {
		return "", fmt.Errorf("event_name is required")
	}
	if req.Value <= 0 {
		req.Value = 1
	}

	logger.Info("ivx_quest_progress: user=%s event=%s value=%d", userID, req.EventName, req.Value)

	state, err := loadQuestState(ctx, nk, userID, logger)
	if err != nil {
		return "", err
	}

	var result map[string]interface{}
	for i := range state.Quests {
		q := &state.Quests[i]
		if q.Status != "active" && q.Status != "inprogress" {
			continue
		}
		prev := q.CurrentProgress
		q.CurrentProgress += req.Value
		if q.CurrentProgress > q.TargetProgress {
			q.CurrentProgress = q.TargetProgress
		}
		wasComplete := prev >= q.TargetProgress
		nowComplete := q.CurrentProgress >= q.TargetProgress
		if nowComplete {
			q.Status = "completed"
		} else if q.CurrentProgress > 0 {
			q.Status = "inprogress"
		}
		result = map[string]interface{}{
			"quest_id":         q.QuestID,
			"current_progress": q.CurrentProgress,
			"completed":        nowComplete,
			"newly_completed":  !wasComplete && nowComplete,
		}
		break
	}

	if result == nil {
		result = map[string]interface{}{
			"quest_id":         "",
			"current_progress": 0,
			"completed":        false,
			"newly_completed":  false,
		}
	}

	if err := saveQuestState(ctx, nk, userID, state); err != nil {
		return "", err
	}

	resp, _ := json.Marshal(result)
	return string(resp), nil
}

// IVXQuestClaim claims the reward for a completed quest.
func IVXQuestClaim(ctx context.Context, logger runtime.Logger, db *sql.DB, nk runtime.NakamaModule, payload string) (string, error) {
	userID, err := requireAuthUser(ctx)
	if err != nil {
		return "", err
	}

	var req questClaimPayload
	if err := json.Unmarshal([]byte(payload), &req); err != nil {
		return "", fmt.Errorf("invalid payload: %w", err)
	}
	if req.QuestID == "" {
		return "", fmt.Errorf("quest_id is required")
	}

	logger.Info("ivx_quest_claim: user=%s quest=%s", userID, req.QuestID)

	state, err := loadQuestState(ctx, nk, userID, logger)
	if err != nil {
		return "", err
	}

	var target *questEntry
	for i := range state.Quests {
		if state.Quests[i].QuestID == req.QuestID {
			target = &state.Quests[i]
			break
		}
	}

	if target == nil {
		return "", fmt.Errorf("quest %s not found", req.QuestID)
	}
	if target.Status == "claimed" {
		return "", fmt.Errorf("quest %s already claimed", req.QuestID)
	}
	if target.Status != "completed" {
		return "", fmt.Errorf("quest %s not yet completed (status: %s)", req.QuestID, target.Status)
	}

	walletUpdate := map[string]int64{}
	for _, r := range target.Rewards {
		cid, _ := r["currency_id"].(string)
		amt, _ := r["amount"].(float64)
		if cid != "" && amt > 0 {
			walletUpdate[cid] += int64(amt)
		}
	}

	if len(walletUpdate) > 0 {
		changeset := map[string]int64{}
		for k, v := range walletUpdate {
			changeset[k] = v
		}
		changesetJSON, _ := json.Marshal(changeset)
		if _, _, err := nk.WalletUpdate(ctx, userID, string(changesetJSON), nil, true); err != nil {
			logger.Error("wallet update failed for quest %s: %v", req.QuestID, err)
			return "", fmt.Errorf("reward distribution failed: %w", err)
		}
	}

	target.Status = "claimed"
	if err := saveQuestState(ctx, nk, userID, state); err != nil {
		return "", err
	}

	resp, _ := json.Marshal(map[string]interface{}{
		"quest_id":      req.QuestID,
		"rewards":       target.Rewards,
		"claimed":       true,
		"wallet_update": walletUpdate,
	})
	return string(resp), nil
}

// IVXQuestConfig returns the quest configuration.
func IVXQuestConfig(ctx context.Context, logger runtime.Logger, db *sql.DB, nk runtime.NakamaModule, payload string) (string, error) {
	if _, err := requireAuthUser(ctx); err != nil {
		return "", err
	}

	objs, err := nk.StorageRead(ctx, []*runtime.StorageRead{{
		Collection: questStorageCollection,
		Key:        questConfigKey,
		UserID:     "",
	}})
	if err != nil || len(objs) == 0 {
		return `{"quests":[],"refresh_interval_sec":86400,"max_active_quests":5,"event_mappings":{}}`, nil
	}
	return objs[0].Value, nil
}

func loadQuestState(ctx context.Context, nk runtime.NakamaModule, userID string, logger runtime.Logger) (*userQuestState, error) {
	objs, err := nk.StorageRead(ctx, []*runtime.StorageRead{{
		Collection: questStorageCollection,
		Key:        questProgressKey,
		UserID:     userID,
	}})
	if err != nil {
		return nil, fmt.Errorf("storage read failed: %w", err)
	}

	state := &userQuestState{}
	if len(objs) > 0 {
		if err := json.Unmarshal([]byte(objs[0].Value), state); err != nil {
			logger.Warn("corrupted quest state for %s, resetting: %v", userID, err)
			state = &userQuestState{}
		}
	}

	if state.Quests == nil {
		state.Quests = []questEntry{}
	}
	return state, nil
}

func saveQuestState(ctx context.Context, nk runtime.NakamaModule, userID string, state *userQuestState) error {
	state.UpdatedAt = time.Now().UTC().Format(time.RFC3339)
	data, _ := json.Marshal(state)

	_, err := nk.StorageWrite(ctx, []*runtime.StorageWrite{{
		Collection:      questStorageCollection,
		Key:             questProgressKey,
		UserID:          userID,
		Value:           string(data),
		PermissionRead:  1,
		PermissionWrite: 0,
	}})
	return err
}

func nextDailyReset() string {
	now := time.Now().UTC()
	next := time.Date(now.Year(), now.Month(), now.Day()+1, 0, 0, 0, 0, time.UTC)
	return next.Format(time.RFC3339)
}
