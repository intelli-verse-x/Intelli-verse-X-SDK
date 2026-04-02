# Copyright (c) 2026 Intelli-verse-X
# MIT License — see LICENSE in the project root.

class_name IVXDiscordLinkedChannels
extends RefCounted

## Maps Discord channels to game lobbies — local stub; integrate with Activity API / backend as needed.

class LinkedChannel extends RefCounted:
	var channel_id: String = ""
	var guild_id: String = ""
	var name: String = ""
	var lobby_id: String = ""
	var linked_at: String = ""

var _lobby_to_channels: Dictionary = {}


func link_channel(lobby_id: String, channel_id: String) -> LinkedChannel:
	if not _lobby_to_channels.has(lobby_id):
		_lobby_to_channels[lobby_id] = {}
	var by_ch: Dictionary = _lobby_to_channels[lobby_id]
	var lc := LinkedChannel.new()
	lc.channel_id = channel_id
	lc.guild_id = ""
	lc.name = ""
	lc.lobby_id = lobby_id
	lc.linked_at = Time.get_datetime_string_from_system()
	if by_ch.has(channel_id):
		var existing: LinkedChannel = by_ch[channel_id]
		lc.guild_id = existing.guild_id
		lc.name = existing.name
	by_ch[channel_id] = lc
	return lc


func unlink_channel(lobby_id: String, channel_id: String) -> void:
	if not _lobby_to_channels.has(lobby_id):
		return
	var by_ch: Dictionary = _lobby_to_channels[lobby_id]
	by_ch.erase(channel_id)
	if by_ch.is_empty():
		_lobby_to_channels.erase(lobby_id)


func get_linked_channels(lobby_id: String) -> Array[LinkedChannel]:
	var out: Array[LinkedChannel] = []
	if not _lobby_to_channels.has(lobby_id):
		return out
	for _k in _lobby_to_channels[lobby_id]:
		var lc: LinkedChannel = _lobby_to_channels[lobby_id][_k]
		out.append(lc)
	return out
