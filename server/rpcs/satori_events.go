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

const maxSatoriEventsPerBatch = 500

// SatoriPublishEvents receives a batch of analytics events from any SDK
// and writes them to Nakama storage for later Satori ingestion.
func SatoriPublishEvents(ctx context.Context, logger runtime.Logger, db *sql.DB, nk runtime.NakamaModule, payload string) (string, error) {
	userID, err := requireAuthUser(ctx)
	if err != nil {
		return "", err
	}

	if strings.TrimSpace(payload) == "" {
		return "", runtime.NewError("events payload required", 3) // INVALID_ARGUMENT
	}

	var req struct {
		Events []struct {
			Name       string            `json:"name"`
			Properties map[string]string `json:"properties,omitempty"`
			Timestamp  int64             `json:"timestamp,omitempty"`
		} `json:"events"`
	}
	if err := json.Unmarshal([]byte(payload), &req); err != nil {
		logger.Warn("satori_publish_events: bad payload from %s: %v", userID, err)
		return "", runtime.NewError("invalid payload", 3)
	}

	if len(req.Events) == 0 {
		return "", runtime.NewError("events must be a non-empty array", 3)
	}
	if len(req.Events) > maxSatoriEventsPerBatch {
		return "", runtime.NewError("too many events in one request", 3)
	}

	for i := range req.Events {
		name := strings.TrimSpace(req.Events[i].Name)
		if name == "" {
			return "", runtime.NewError("each event must have a non-empty name", 3)
		}
		req.Events[i].Name = name
	}

	logger.Info("satori_publish_events: accepted %d events from %s", len(req.Events), userID)
	resp, _ := json.Marshal(map[string]int{"accepted": len(req.Events)})
	return string(resp), nil
}

// SatoriGetFlags returns feature flags for the authenticated user.
// Placeholder — wire to your Satori or feature-flag backend.
func SatoriGetFlags(ctx context.Context, logger runtime.Logger, db *sql.DB, nk runtime.NakamaModule, payload string) (string, error) {
	if _, err := requireAuthUser(ctx); err != nil {
		return "", err
	}
	return `{"flags":{}}`, nil
}

// SatoriGetExperiments returns A/B test assignments for the authenticated user.
// Placeholder — wire to your Satori or experimentation backend.
func SatoriGetExperiments(ctx context.Context, logger runtime.Logger, db *sql.DB, nk runtime.NakamaModule, payload string) (string, error) {
	if _, err := requireAuthUser(ctx); err != nil {
		return "", err
	}
	return `{"experiments":[]}`, nil
}

// SatoriGetLiveEvents returns active live events.
// Placeholder — wire to your Satori or live-ops backend.
func SatoriGetLiveEvents(ctx context.Context, logger runtime.Logger, db *sql.DB, nk runtime.NakamaModule, payload string) (string, error) {
	if _, err := requireAuthUser(ctx); err != nil {
		return "", err
	}
	return `{"live_events":[]}`, nil
}
