// Copyright (c) 2026 Intelli-verse-X
// MIT License — see LICENSE in the project root.

package main

import (
	"context"
	"database/sql"

	"github.com/heroiclabs/nakama-common/runtime"

	"intelliversex-server/rpcs"
)

func InitModule(ctx context.Context, logger runtime.Logger, db *sql.DB, nk runtime.NakamaModule, initializer runtime.Initializer) error {
	logger.Info("IntelliVerseX server module loaded")

	// Shared RPCs (all 8 SDKs)
	if err := initializer.RegisterRpc("ivx_sync_metadata", rpcs.SyncMetadata); err != nil {
		return err
	}
	if err := initializer.RegisterRpc("hiro_economy_list", rpcs.EconomyList); err != nil {
		return err
	}
	if err := initializer.RegisterRpc("hiro_economy_grant", rpcs.EconomyGrant); err != nil {
		return err
	}

	// Unity RPCs
	if err := initializer.RegisterRpc("create_or_sync_user", rpcs.CreateOrSyncUser); err != nil {
		return err
	}
	if err := initializer.RegisterRpc("submit_score_and_sync", rpcs.SubmitScoreAndSync); err != nil {
		return err
	}
	if err := initializer.RegisterRpc("get_all_leaderboards", rpcs.GetAllLeaderboards); err != nil {
		return err
	}
	if err := initializer.RegisterRpc("get_wallet_balance", rpcs.GetWalletBalance); err != nil {
		return err
	}
	if err := initializer.RegisterRpc("update_wallet_balance", rpcs.UpdateWalletBalance); err != nil {
		return err
	}
	if err := initializer.RegisterRpc("calculate_score_reward", rpcs.CalculateScoreReward); err != nil {
		return err
	}
	if err := initializer.RegisterRpc("update_game_reward_config", rpcs.UpdateGameRewardConfig); err != nil {
		return err
	}

	// Satori RPCs
	if err := initializer.RegisterRpc("satori_publish_events", rpcs.SatoriPublishEvents); err != nil {
		return err
	}
	if err := initializer.RegisterRpc("satori_get_flags", rpcs.SatoriGetFlags); err != nil {
		return err
	}
	if err := initializer.RegisterRpc("satori_get_experiments", rpcs.SatoriGetExperiments); err != nil {
		return err
	}
	if err := initializer.RegisterRpc("satori_get_live_events", rpcs.SatoriGetLiveEvents); err != nil {
		return err
	}

	// Hiro system RPCs
	if err := initializer.RegisterRpc("hiro_spin_wheel", rpcs.HiroSpinWheel); err != nil {
		return err
	}
	if err := initializer.RegisterRpc("hiro_get_streaks", rpcs.HiroGetStreaks); err != nil {
		return err
	}
	if err := initializer.RegisterRpc("hiro_claim_streak", rpcs.HiroClaimStreak); err != nil {
		return err
	}
	if err := initializer.RegisterRpc("hiro_get_offerwall", rpcs.HiroGetOfferwall); err != nil {
		return err
	}
	if err := initializer.RegisterRpc("hiro_retention_get", rpcs.HiroRetentionGet); err != nil {
		return err
	}
	if err := initializer.RegisterRpc("hiro_retention_update", rpcs.HiroRetentionUpdate); err != nil {
		return err
	}

	// Quest/Challenge RPCs
	if err := initializer.RegisterRpc("ivx_quest_get", rpcs.IVXQuestGet); err != nil {
		return err
	}
	if err := initializer.RegisterRpc("ivx_quest_progress", rpcs.IVXQuestProgress); err != nil {
		return err
	}
	if err := initializer.RegisterRpc("ivx_quest_claim", rpcs.IVXQuestClaim); err != nil {
		return err
	}
	if err := initializer.RegisterRpc("ivx_quest_config", rpcs.IVXQuestConfig); err != nil {
		return err
	}

	logger.Info("IntelliVerseX: all 24 RPCs registered")
	return nil
}
