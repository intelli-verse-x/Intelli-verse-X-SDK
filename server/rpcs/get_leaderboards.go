// Copyright (c) 2026 Intelli-verse-X
// MIT License — see LICENSE in the project root.

package rpcs

import (
	"context"
	"database/sql"
	"encoding/json"

	"github.com/heroiclabs/nakama-common/api"
	"github.com/heroiclabs/nakama-common/runtime"
)

type getLeaderboardsRequest struct {
	UserID   string `json:"user_id"`
	DeviceID string `json:"device_id"`
	GameID   string `json:"game_id"`
	Limit    int    `json:"limit"`
}

type leaderboardEntry struct {
	UserID   string `json:"user_id"`
	Username string `json:"username"`
	Score    int64  `json:"score"`
	Rank     int64  `json:"rank"`
}

type getLeaderboardsResponse struct {
	Success     bool               `json:"success"`
	Error       string             `json:"error,omitempty"`
	Daily       []leaderboardEntry `json:"daily"`
	Weekly      []leaderboardEntry `json:"weekly"`
	AllTime     []leaderboardEntry `json:"allTime"`
	CurrentUser *leaderboardEntry  `json:"currentUser,omitempty"`
}

const maxLeaderboardLimit = 100

// GetAllLeaderboards returns daily, weekly, and all-time leaderboard records.
// Unity SDK calls this to populate leaderboard UI panels.
func GetAllLeaderboards(ctx context.Context, logger runtime.Logger, db *sql.DB, nk runtime.NakamaModule, payload string) (string, error) {
	userID, err := requireAuthUser(ctx)
	if err != nil {
		return "", err
	}

	var req getLeaderboardsRequest
	if err := json.Unmarshal([]byte(payload), &req); err != nil {
		return "", runtime.NewError("invalid payload", 3)
	}

	limit := req.Limit
	if limit <= 0 {
		limit = 20
	}
	if limit > maxLeaderboardLimit {
		return "", runtime.NewError("limit out of range", 3)
	}

	prefix := "weekly_high_scores"
	if req.GameID != "" {
		prefix = req.GameID + "_scores"
	}

	weekly := fetchRecords(ctx, nk, prefix, userID, limit)
	daily := fetchRecords(ctx, nk, prefix+"_daily", userID, limit)
	allTime := fetchRecords(ctx, nk, prefix+"_alltime", userID, limit)

	var currentUser *leaderboardEntry
	for _, e := range weekly {
		if e.UserID == userID {
			copy := e
			currentUser = &copy
			break
		}
	}

	return marshalJSON(getLeaderboardsResponse{
		Success:     true,
		Daily:       daily,
		Weekly:      weekly,
		AllTime:     allTime,
		CurrentUser: currentUser,
	})
}

func fetchRecords(ctx context.Context, nk runtime.NakamaModule, leaderboardID string, ownerID string, limit int) []leaderboardEntry {
	_ = nk.LeaderboardCreate(ctx, leaderboardID, false, "desc", "best", "0 0 * * 1", nil, false)

	records, _, _, _, err := nk.LeaderboardRecordsList(ctx, leaderboardID, []string{ownerID}, limit, "", 0)
	if err != nil {
		return []leaderboardEntry{}
	}

	return convertRecords(records)
}

func convertRecords(records []*api.LeaderboardRecord) []leaderboardEntry {
	entries := make([]leaderboardEntry, 0, len(records))
	for _, r := range records {
		entries = append(entries, leaderboardEntry{
			UserID:   r.OwnerId,
			Username: r.Username.GetValue(),
			Score:    r.Score,
			Rank:     r.Rank,
		})
	}
	return entries
}
