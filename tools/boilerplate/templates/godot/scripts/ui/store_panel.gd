class_name StorePanel
extends Control

## IVXEconomy Hiro RPCs: hiro_store_list / hiro_store_purchase (adjust to your backend).
class IVXEconomy extends RefCounted:
	var _ivx: Node
	func _init(ivx: Node) -> void:
		_ivx = ivx
	func get_store_items() -> Array:
		var d: Dictionary = await _ivx.call_rpc("hiro_store_list", "{}")
		return d.get("items", []) if d.get("items") is Array else []
	func purchase_item(item_id: String) -> Dictionary:
		return await _ivx.call_rpc("hiro_store_purchase", JSON.stringify({"item_id": item_id}))

@onready var _grid: GridContainer = get_node_or_null("StoreGrid") as GridContainer

var _ivx: Node
var _economy: IVXEconomy

func _ready() -> void:
	_ivx = get_node_or_null("/root/IntelliVerseX")
	if _ivx:
		_economy = IVXEconomy.new(_ivx)
		await _populate()


func _populate() -> void:
	if _grid == null or _economy == null:
		return
	for c in _grid.get_children():
		c.queue_free()
	var items: Array = await _economy.get_store_items()
	for it in items:
		var id := str(it.get("id", it.get("item_id", "")))
		var card := Button.new()
		card.text = str(it.get("name", id))
		card.pressed.connect(_buy.bind(id))
		_grid.add_child(card)


func _buy(item_id: String) -> void:
	if _economy == null or item_id.is_empty():
		return
	var result: Dictionary = await _economy.purchase_item(item_id)
	Analytics.track_event("store_purchase", {"item_id": item_id, "game_id": "{{game_id}}", "ok": not result.is_empty()})
