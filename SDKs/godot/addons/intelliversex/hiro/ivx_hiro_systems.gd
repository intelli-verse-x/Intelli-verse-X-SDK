# Copyright (c) 2026 Intelli-verse-X
# MIT License — see LICENSE in the project root.

class_name IVXHiroSystems
extends Node

## IntelliVerseX Hiro Systems — spin wheel, streaks, offerwall, friend quests & battles.
## Wraps Nakama RPC calls for Hiro server-side systems.

signal spin_wheel_result(reward: Dictionary)
signal streak_updated(streak: Dictionary)
signal streak_claimed(streak_id: String, milestone: String, reward: Dictionary)
signal offerwall_updated(offers: Array)
signal friend_quest_updated(quest: Dictionary)
signal friend_battle_result(result: Dictionary)

var _nakama_client: Variant = null
var _session: Variant = null
var _is_initialized: bool = false


## Initialize with a live Nakama client and session.
func initialize(nakama_client: Variant, session: Variant) -> void:
	_nakama_client = nakama_client
	_session = session
	_is_initialized = true
	print("[IVXHiroSystems] Initialized")


# ── Spin Wheel ───────────────────────────────────────────────────────────────

## Retrieve the current spin-wheel state.
func spin_wheel_get() -> Dictionary:
	return await _rpc("hiro/spinwheel/get", "")


## Execute a spin on the wheel.
func spin_wheel_spin() -> Dictionary:
	var result := await _rpc("hiro/spinwheel/spin", "")
	if result.size() > 0:
		spin_wheel_result.emit(result)
	return result


# ── Streaks ──────────────────────────────────────────────────────────────────

## Retrieve all active streaks.
func streaks_get() -> Dictionary:
	return await _rpc("hiro/streaks/get", "")


## Update progress on a specific streak.
func streaks_update(streak_id: String) -> Dictionary:
	var payload := JSON.stringify({"id": streak_id})
	var result := await _rpc("hiro/streaks/update", payload)
	if result.size() > 0:
		streak_updated.emit(result)
	return result


## Claim a milestone reward for a streak.
func streaks_claim(streak_id: String, milestone: String) -> Dictionary:
	var payload := JSON.stringify({"id": streak_id, "milestone": milestone})
	var result := await _rpc("hiro/streaks/claim", payload)
	if result.size() > 0:
		streak_claimed.emit(streak_id, milestone, result)
	return result


# ── Offerwall ────────────────────────────────────────────────────────────────

## Get all available offerwall entries.
func offerwall_get() -> Array:
	var result := await _rpc("hiro/offerwall/get", "")
	if result.has("offers"):
		offerwall_updated.emit(result["offers"])
		return result["offers"]
	return []


## Mark an offer as completed.
func offerwall_complete(offer_id: String) -> void:
	var payload := JSON.stringify({"id": offer_id})
	await _rpc("hiro/offerwall/complete", payload)


## Claim all pending offerwall rewards.
func offerwall_claim_pending() -> void:
	await _rpc("hiro/offerwall/claim_pending", "")


# ── Friend Quests ────────────────────────────────────────────────────────────

## Get active friend quests.
func friend_quests_get() -> Dictionary:
	return await _rpc("hiro/friend_quests/get", "")


## Contribute progress to a friend quest.
func friend_quests_contribute(quest_id: String, amount: int = 1) -> Dictionary:
	var payload := JSON.stringify({"id": quest_id, "amount": amount})
	var result := await _rpc("hiro/friend_quests/contribute", payload)
	if result.size() > 0:
		friend_quest_updated.emit(result)
	return result


# ── Friend Battles ───────────────────────────────────────────────────────────

## Challenge a friend to a battle.
func friend_battles_challenge(friend_id: String, score: int = 0) -> Dictionary:
	var payload := JSON.stringify({"friend_id": friend_id, "score": score})
	var result := await _rpc("hiro/friend_battles/challenge", payload)
	if result.size() > 0:
		friend_battle_result.emit(result)
	return result


## Get pending and active friend battles.
func friend_battles_get() -> Dictionary:
	return await _rpc("hiro/friend_battles/get", "")


# ── Private helpers ──────────────────────────────────────────────────────────

func _rpc(rpc_id: String, payload: String) -> Dictionary:
	if not _is_initialized:
		push_warning("[IVXHiroSystems] Not initialized — call initialize() first")
		return {}
	var result: Variant = await _nakama_client.rpc_async(_session, rpc_id, payload)
	if result == null:
		push_warning("[IVXHiroSystems] RPC '%s' returned null" % rpc_id)
		return {}
	if result is Dictionary:
		return result
	var json := JSON.new()
	if result.has("payload") and json.parse(result.payload) == OK:
		return json.data if json.data is Dictionary else {"data": json.data}
	return {"raw": str(result)}
