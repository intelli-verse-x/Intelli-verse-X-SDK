// Copyright (c) 2026 Intelli-verse-X
// MIT License — see LICENSE in the project root.

package rpcs

import (
	"context"

	"github.com/heroiclabs/nakama-common/runtime"
)

// requireAuthUser returns the authenticated Nakama user ID or a UNAUTHENTICATED error.
func requireAuthUser(ctx context.Context) (string, error) {
	userID, ok := ctx.Value(runtime.RUNTIME_CTX_USER_ID).(string)
	if !ok || userID == "" {
		return "", runtime.NewError("unauthorized", 16) // UNAUTHENTICATED
	}
	return userID, nil
}
