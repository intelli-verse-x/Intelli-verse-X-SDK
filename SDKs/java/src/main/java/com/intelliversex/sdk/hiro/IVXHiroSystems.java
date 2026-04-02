// Copyright (c) 2026 Intelli-verse-X
// MIT License — see LICENSE in the project root.

package com.intelliversex.sdk.hiro;

import com.google.gson.Gson;
import com.google.gson.annotations.SerializedName;
import com.google.gson.reflect.TypeToken;
import com.heroiclabs.nakama.Client;
import com.heroiclabs.nakama.Session;
import com.heroiclabs.nakama.api.Rpc;

import java.lang.reflect.Type;
import java.util.Collections;
import java.util.List;
import java.util.Objects;
import java.util.concurrent.CompletableFuture;

/**
 * Wrapper around Hiro live-ops systems exposed as Nakama RPCs.
 * <p>
 * Provides typed access to Spin Wheel, Streaks, Offerwall, Retention,
 * Friend Quests, and Friend Battles. Each subsystem is accessed through
 * its own inner class (e.g. {@link SpinWheel}, {@link Streaks}).
 * <p>
 * <b>Usage:</b>
 * <pre>{@code
 * IVXHiroSystems hiro = new IVXHiroSystems(nakamaClient, session);
 * hiro.spinWheel().spin("daily_wheel").thenAccept(result -> { ... });
 * hiro.streaks().getState().thenAccept(state -> { ... });
 * }</pre>
 */
public class IVXHiroSystems {

    private final Client client;
    private volatile Session session;
    private final Gson gson = new Gson();

    private final SpinWheel spinWheel;
    private final Streaks streaks;
    private final Offerwall offerwall;
    private final Retention retention;
    private final FriendQuests friendQuests;
    private final FriendBattles friendBattles;
    private final IapTrigger iapTrigger;
    private final SmartAdTimer smartAdTimer;

    /**
     * Creates a new Hiro systems wrapper.
     *
     * @param client  an initialized Nakama client
     * @param session an authenticated Nakama session
     * @throws NullPointerException if either argument is null
     */
    public IVXHiroSystems(Client client, Session session) {
        this.client = Objects.requireNonNull(client, "client must not be null");
        this.session = Objects.requireNonNull(session, "session must not be null");
        this.spinWheel = new SpinWheel();
        this.streaks = new Streaks();
        this.offerwall = new Offerwall();
        this.retention = new Retention();
        this.friendQuests = new FriendQuests();
        this.friendBattles = new FriendBattles();
        this.iapTrigger = new IapTrigger();
        this.smartAdTimer = new SmartAdTimer();
    }

    /**
     * Updates the session (e.g. after token refresh) across all subsystems.
     *
     * @param session the refreshed session
     */
    public void updateSession(Session session) {
        this.session = Objects.requireNonNull(session, "session must not be null");
    }

    /** Returns the Spin Wheel subsystem. */
    public SpinWheel spinWheel() { return spinWheel; }

    /** Returns the Streaks subsystem. */
    public Streaks streaks() { return streaks; }

    /** Returns the Offerwall subsystem. */
    public Offerwall offerwall() { return offerwall; }

    /** Returns the Retention subsystem. */
    public Retention retention() { return retention; }

    /** Returns the Friend Quests subsystem. */
    public FriendQuests friendQuests() { return friendQuests; }

    /** Returns the Friend Battles subsystem. */
    public FriendBattles friendBattles() { return friendBattles; }

    /** Returns the IAP Trigger subsystem. */
    public IapTrigger iapTrigger() { return iapTrigger; }

    /** Returns the Smart Ad Timer subsystem. */
    public SmartAdTimer smartAdTimer() { return smartAdTimer; }

    // ──────────────────────────────────────────────
    //  Data Models
    // ──────────────────────────────────────────────

    /** Result of a spin-wheel spin. */
    public static final class IVXSpinWheelResult {
        @SerializedName("reward_id")
        private final String rewardId;

        @SerializedName("reward_type")
        private final String rewardType;

        @SerializedName("amount")
        private final int amount;

        @SerializedName("spins_remaining")
        private final int spinsRemaining;

        public IVXSpinWheelResult(String rewardId, String rewardType, int amount, int spinsRemaining) {
            this.rewardId = rewardId != null ? rewardId : "";
            this.rewardType = rewardType != null ? rewardType : "";
            this.amount = amount;
            this.spinsRemaining = spinsRemaining;
        }

        public String getRewardId() { return rewardId; }
        public String getRewardType() { return rewardType; }
        public int getAmount() { return amount; }
        public int getSpinsRemaining() { return spinsRemaining; }

        @Override
        public String toString() {
            return "IVXSpinWheelResult{rewardId='" + rewardId + "', type='" + rewardType
                    + "', amount=" + amount + ", remaining=" + spinsRemaining + '}';
        }
    }

    /** Current state of a player's streak (login, play, etc.). */
    public static final class IVXStreakState {
        @SerializedName("streak_id")
        private final String streakId;

        @SerializedName("current_day")
        private final int currentDay;

        @SerializedName("max_day")
        private final int maxDay;

        @SerializedName("claimed_today")
        private final boolean claimedToday;

        @SerializedName("reset_at")
        private final long resetAt;

        public IVXStreakState(String streakId, int currentDay, int maxDay, boolean claimedToday, long resetAt) {
            this.streakId = streakId != null ? streakId : "";
            this.currentDay = currentDay;
            this.maxDay = maxDay;
            this.claimedToday = claimedToday;
            this.resetAt = resetAt;
        }

        public String getStreakId() { return streakId; }
        public int getCurrentDay() { return currentDay; }
        public int getMaxDay() { return maxDay; }
        public boolean isClaimedToday() { return claimedToday; }
        public long getResetAt() { return resetAt; }

        @Override
        public String toString() {
            return "IVXStreakState{streakId='" + streakId + "', day=" + currentDay + '/' + maxDay
                    + ", claimed=" + claimedToday + '}';
        }
    }

    /** A single offer from the offerwall. */
    public static final class IVXOffer {
        @SerializedName("offer_id")
        private final String offerId;

        @SerializedName("title")
        private final String title;

        @SerializedName("description")
        private final String description;

        @SerializedName("reward_amount")
        private final int rewardAmount;

        @SerializedName("expires_at")
        private final long expiresAt;

        @SerializedName("completed")
        private final boolean completed;

        public IVXOffer(String offerId, String title, String description,
                        int rewardAmount, long expiresAt, boolean completed) {
            this.offerId = offerId != null ? offerId : "";
            this.title = title != null ? title : "";
            this.description = description != null ? description : "";
            this.rewardAmount = rewardAmount;
            this.expiresAt = expiresAt;
            this.completed = completed;
        }

        public String getOfferId() { return offerId; }
        public String getTitle() { return title; }
        public String getDescription() { return description; }
        public int getRewardAmount() { return rewardAmount; }
        public long getExpiresAt() { return expiresAt; }
        public boolean isCompleted() { return completed; }

        @Override
        public String toString() {
            return "IVXOffer{offerId='" + offerId + "', title='" + title
                    + "', reward=" + rewardAmount + ", completed=" + completed + '}';
        }
    }

    /** A friend quest definition and progress. */
    public static final class IVXFriendQuest {
        @SerializedName("quest_id")
        private final String questId;

        @SerializedName("friend_id")
        private final String friendId;

        @SerializedName("title")
        private final String title;

        @SerializedName("progress")
        private final int progress;

        @SerializedName("goal")
        private final int goal;

        @SerializedName("reward_amount")
        private final int rewardAmount;

        public IVXFriendQuest(String questId, String friendId, String title,
                              int progress, int goal, int rewardAmount) {
            this.questId = questId != null ? questId : "";
            this.friendId = friendId != null ? friendId : "";
            this.title = title != null ? title : "";
            this.progress = progress;
            this.goal = goal;
            this.rewardAmount = rewardAmount;
        }

        public String getQuestId() { return questId; }
        public String getFriendId() { return friendId; }
        public String getTitle() { return title; }
        public int getProgress() { return progress; }
        public int getGoal() { return goal; }
        public int getRewardAmount() { return rewardAmount; }
        public boolean isComplete() { return progress >= goal; }

        @Override
        public String toString() {
            return "IVXFriendQuest{questId='" + questId + "', progress=" + progress + '/' + goal + '}';
        }
    }

    // ──────────────────────────────────────────────
    //  Data Models: IAP Trigger / Smart Ad Timer
    // ──────────────────────────────────────────────

    /** Result of an IAP trigger check. */
    public static final class IVXIapTriggerResult {
        @SerializedName("should_show")
        private final boolean shouldShow;

        @SerializedName("offer_id")
        private final String offerId;

        @SerializedName("discount")
        private final double discount;

        @SerializedName("expires_at")
        private final long expiresAt;

        public IVXIapTriggerResult(boolean shouldShow, String offerId, double discount, long expiresAt) {
            this.shouldShow = shouldShow;
            this.offerId = offerId != null ? offerId : "";
            this.discount = discount;
            this.expiresAt = expiresAt;
        }

        public boolean isShouldShow() { return shouldShow; }
        public String getOfferId() { return offerId; }
        public double getDiscount() { return discount; }
        public long getExpiresAt() { return expiresAt; }

        @Override
        public String toString() {
            return "IVXIapTriggerResult{show=" + shouldShow + ", offerId='" + offerId
                    + "', discount=" + discount + '}';
        }
    }

    /** Result of a smart-ad availability check. */
    public static final class IVXSmartAdResult {
        @SerializedName("can_show")
        private final boolean canShow;

        @SerializedName("next_available_at")
        private final long nextAvailableAt;

        @SerializedName("reason")
        private final String reason;

        public IVXSmartAdResult(boolean canShow, long nextAvailableAt, String reason) {
            this.canShow = canShow;
            this.nextAvailableAt = nextAvailableAt;
            this.reason = reason != null ? reason : "";
        }

        public boolean isCanShow() { return canShow; }
        public long getNextAvailableAt() { return nextAvailableAt; }
        public String getReason() { return reason; }

        @Override
        public String toString() {
            return "IVXSmartAdResult{canShow=" + canShow + ", nextAt=" + nextAvailableAt
                    + ", reason='" + reason + "'}";
        }
    }

    // ──────────────────────────────────────────────
    //  Subsystem: Spin Wheel
    // ──────────────────────────────────────────────

    /**
     * Spin-wheel subsystem — daily/event reward spins.
     */
    public class SpinWheel {
        private static final String RPC_SPIN = "hiro_spin_wheel";
        private static final String RPC_STATE = "hiro_spin_wheel_state";

        /**
         * Executes a spin on the specified wheel.
         *
         * @param wheelId the wheel identifier (e.g. "daily_wheel")
         * @return a future resolving to the spin result
         */
        public CompletableFuture<IVXSpinWheelResult> spin(String wheelId) {
            return rpc(RPC_SPIN, "{\"wheel_id\":\"" + wheelId + "\"}")
                    .thenApply(p -> gson.fromJson(p, IVXSpinWheelResult.class));
        }

        /**
         * Gets the current state of the spin wheel (spins remaining, cooldown).
         *
         * @param wheelId the wheel identifier
         * @return a future resolving to the spin-wheel result (with remaining spins)
         */
        public CompletableFuture<IVXSpinWheelResult> getState(String wheelId) {
            return rpc(RPC_STATE, "{\"wheel_id\":\"" + wheelId + "\"}")
                    .thenApply(p -> gson.fromJson(p, IVXSpinWheelResult.class));
        }
    }

    // ──────────────────────────────────────────────
    //  Subsystem: Streaks
    // ──────────────────────────────────────────────

    /**
     * Streaks subsystem — login streaks, play streaks, etc.
     */
    public class Streaks {
        private static final String RPC_STATE = "hiro_streak_state";
        private static final String RPC_CLAIM = "hiro_streak_claim";

        /**
         * Gets the current streak state for the authenticated user.
         *
         * @return a future resolving to the streak state
         */
        public CompletableFuture<IVXStreakState> getState() {
            return rpc(RPC_STATE, "{}")
                    .thenApply(p -> gson.fromJson(p, IVXStreakState.class));
        }

        /**
         * Claims the reward for the current streak day.
         *
         * @return a future resolving to the updated streak state
         */
        public CompletableFuture<IVXStreakState> claim() {
            return rpc(RPC_CLAIM, "{}")
                    .thenApply(p -> gson.fromJson(p, IVXStreakState.class));
        }
    }

    // ──────────────────────────────────────────────
    //  Subsystem: Offerwall
    // ──────────────────────────────────────────────

    /**
     * Offerwall subsystem — rewarded tasks and offers.
     */
    public class Offerwall {
        private static final String RPC_LIST = "hiro_offerwall_list";
        private static final String RPC_CLAIM = "hiro_offerwall_claim";

        /**
         * Lists all available offers for the authenticated user.
         *
         * @return a future resolving to the list of offers
         */
        public CompletableFuture<List<IVXOffer>> list() {
            Type type = new TypeToken<List<IVXOffer>>() {}.getType();
            return rpc(RPC_LIST, "{}")
                    .thenApply(p -> {
                        List<IVXOffer> offers = gson.fromJson(p, type);
                        return offers != null ? Collections.unmodifiableList(offers) : Collections.emptyList();
                    });
        }

        /**
         * Claims the reward for a completed offer.
         *
         * @param offerId the offer to claim
         * @return a future resolving to the claimed offer details
         */
        public CompletableFuture<IVXOffer> claim(String offerId) {
            return rpc(RPC_CLAIM, "{\"offer_id\":\"" + offerId + "\"}")
                    .thenApply(p -> gson.fromJson(p, IVXOffer.class));
        }
    }

    // ──────────────────────────────────────────────
    //  Subsystem: Retention
    // ──────────────────────────────────────────────

    /**
     * Retention subsystem — tracks daily engagement and rewards returning players.
     */
    public class Retention {
        private static final String RPC_CHECK = "hiro_retention_check";
        private static final String RPC_CLAIM = "hiro_retention_claim";

        /**
         * Checks the user's retention state (consecutive days, reward tier).
         *
         * @return a future resolving to the streak state representing retention progress
         */
        public CompletableFuture<IVXStreakState> check() {
            return rpc(RPC_CHECK, "{}")
                    .thenApply(p -> gson.fromJson(p, IVXStreakState.class));
        }

        /**
         * Claims the current retention reward.
         *
         * @return a future resolving to the updated retention state
         */
        public CompletableFuture<IVXStreakState> claim() {
            return rpc(RPC_CLAIM, "{}")
                    .thenApply(p -> gson.fromJson(p, IVXStreakState.class));
        }
    }

    // ──────────────────────────────────────────────
    //  Subsystem: Friend Quests
    // ──────────────────────────────────────────────

    /**
     * Friend Quests subsystem — cooperative quests between friends.
     */
    public class FriendQuests {
        private static final String RPC_LIST = "hiro_friend_quests_list";
        private static final String RPC_CLAIM = "hiro_friend_quests_claim";

        /**
         * Lists all active friend quests for the authenticated user.
         *
         * @return a future resolving to the list of friend quests
         */
        public CompletableFuture<List<IVXFriendQuest>> list() {
            Type type = new TypeToken<List<IVXFriendQuest>>() {}.getType();
            return rpc(RPC_LIST, "{}")
                    .thenApply(p -> {
                        List<IVXFriendQuest> quests = gson.fromJson(p, type);
                        return quests != null ? Collections.unmodifiableList(quests) : Collections.emptyList();
                    });
        }

        /**
         * Claims the reward for a completed friend quest.
         *
         * @param questId the quest to claim
         * @return a future resolving to the claimed quest details
         */
        public CompletableFuture<IVXFriendQuest> claim(String questId) {
            return rpc(RPC_CLAIM, "{\"quest_id\":\"" + questId + "\"}")
                    .thenApply(p -> gson.fromJson(p, IVXFriendQuest.class));
        }
    }

    // ──────────────────────────────────────────────
    //  Subsystem: Friend Battles
    // ──────────────────────────────────────────────

    /**
     * Friend Battles subsystem — asynchronous PvP challenges between friends.
     */
    public class FriendBattles {
        private static final String RPC_CHALLENGE = "hiro_friend_battle_challenge";
        private static final String RPC_ACCEPT = "hiro_friend_battle_accept";
        private static final String RPC_RESULT = "hiro_friend_battle_result";

        /**
         * Sends a battle challenge to a friend.
         *
         * @param friendId the friend to challenge
         * @param matchData JSON-encoded challenge payload (score, category, etc.)
         * @return a future that completes when the challenge is sent
         */
        public CompletableFuture<Void> challenge(String friendId, String matchData) {
            return rpc(RPC_CHALLENGE,
                    "{\"friend_id\":\"" + friendId + "\",\"match_data\":\"" + matchData + "\"}")
                    .thenApply(p -> null);
        }

        /**
         * Accepts an incoming friend battle challenge.
         *
         * @param challengeId the challenge to accept
         * @param matchData   the responding player's match data
         * @return a future that completes when the acceptance is acknowledged
         */
        public CompletableFuture<Void> accept(String challengeId, String matchData) {
            return rpc(RPC_ACCEPT,
                    "{\"challenge_id\":\"" + challengeId + "\",\"match_data\":\"" + matchData + "\"}")
                    .thenApply(p -> null);
        }

        /**
         * Gets the result of a completed friend battle.
         *
         * @param challengeId the challenge to query
         * @return a future resolving to a JSON string containing the battle result
         */
        public CompletableFuture<String> getResult(String challengeId) {
            return rpc(RPC_RESULT, "{\"challenge_id\":\"" + challengeId + "\"}");
        }
    }

    // ──────────────────────────────────────────────
    //  Subsystem: IAP Trigger
    // ──────────────────────────────────────────────

    /**
     * IAP Trigger subsystem — context-sensitive purchase prompts.
     */
    public class IapTrigger {
        private static final String RPC_CHECK = "hiro_iap_trigger_check";

        /**
         * Checks whether an IAP offer should be shown for the given event.
         *
         * @param eventType the triggering event (e.g. "level_fail", "session_end")
         * @return a future resolving to the trigger result
         */
        public CompletableFuture<IVXIapTriggerResult> check(String eventType) {
            return rpc(RPC_CHECK, "{\"event_type\":\"" + eventType + "\"}")
                    .thenApply(p -> gson.fromJson(p, IVXIapTriggerResult.class));
        }
    }

    // ──────────────────────────────────────────────
    //  Subsystem: Smart Ad Timer
    // ──────────────────────────────────────────────

    /**
     * Smart Ad Timer subsystem — ad-frequency management.
     */
    public class SmartAdTimer {
        private static final String RPC_CAN_SHOW = "hiro_smart_ad_can_show";

        /**
         * Checks if an ad can be shown for the given placement.
         *
         * @param placement the ad placement identifier
         * @return a future resolving to the ad availability result
         */
        public CompletableFuture<IVXSmartAdResult> canShowAd(String placement) {
            return rpc(RPC_CAN_SHOW, "{\"placement\":\"" + placement + "\"}")
                    .thenApply(p -> gson.fromJson(p, IVXSmartAdResult.class));
        }
    }

    // ──────────────────────────────────────────────
    //  Internal RPC helper
    // ──────────────────────────────────────────────

    private CompletableFuture<String> rpc(String rpcId, String payload) {
        return CompletableFuture.supplyAsync(() -> {
            try {
                Rpc result = client.rpc(session, rpcId, payload).get();
                return result.getPayload();
            } catch (Exception e) {
                throw new RuntimeException("Hiro RPC '" + rpcId + "' failed: " + e.getMessage(), e);
            }
        });
    }
}
