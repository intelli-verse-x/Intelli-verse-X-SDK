class_name WalletDisplay
extends Control

## IVXEconomy-style wallet UI: listens to IntelliVerseX.wallet_updated (balance_changed equivalent).
var _coins: Label
var _gems: Label

var _ivx: Node

func _ready() -> void:
	_coins = find_child("CoinsLabel", true, false) as Label
	_gems = find_child("GemsLabel", true, false) as Label
	_ivx = get_node_or_null("/root/IntelliVerseX")
	if _ivx:
		_ivx.wallet_updated.connect(_on_balance_changed)
		var w: Dictionary = await _ivx.fetch_wallet()
		_on_balance_changed(w)


func _on_balance_changed(wallet: Dictionary) -> void:
	_refresh_from_wallet(wallet)


func _refresh_from_wallet(wallet: Dictionary) -> void:
	var c: Variant = wallet.get("coins", wallet.get("currencies", {}).get("coins", "—"))
	var g: Variant = wallet.get("gems", wallet.get("currencies", {}).get("gems", "—"))
	if _coins:
		_coins.text = "Coins: %s" % str(c)
	if _gems:
		_gems.text = "Gems: %s" % str(g)
