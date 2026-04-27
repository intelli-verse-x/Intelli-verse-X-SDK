# IVXLiveKitVisemeReceiver — pure-GDScript decoder for the
# `viseme.v1` data-channel envelope shipped by the LiveKit avatar
# agent worker. Engine-agnostic: the LiveKit transport is BYO (use a
# third-party GDExtension that exposes `data_received(payload, topic)`
# and forward `payload, topic` to `on_livekit_data(payload, topic)`).
#
# Mirrors:
#   * Unity:  Assets/Intelli-verse-X-SDK/MultiplayerKernel/Voice/IVXLiveKitVisemeStream.cs
#   * UE5:    SDKs/unreal/Source/IntelliVerseX/Public/IVXLiveKitVisemeStream.h
#   * JS:     SDKs/javascript/packages/multiplayer/src/avatar/livekit-viseme-receiver.ts
#
# Wire format: see schemas/multiplayer/viseme_v1.proto (JSON-on-wire in
# Phase-4; binary protobuf reserved for v1.1).
#
# Usage:
#
#   var receiver := IVXLiveKitVisemeReceiver.new()
#   receiver.on_frame.connect(func(f): _apply_blendshapes(f.blendshapes))
#   livekit_room.data_received.connect(func(payload, topic):
#       receiver.on_livekit_data(payload, topic))

class_name IVXLiveKitVisemeReceiver
extends RefCounted

const VISEME_TOPIC := "viseme.v1"
const KIND_HEADER  := "header"
const KIND_FRAME   := "frame"
const KIND_PHONEME := "phoneme"
const KIND_EXPR    := "expression"
const KIND_FOOTER  := "footer"

signal on_header(header: Dictionary)
signal on_frame(frame: Dictionary)
signal on_phoneme(phoneme: Dictionary)
signal on_expression(expression: Dictionary)
signal on_footer(footer: Dictionary)

var is_active: bool = false
var current_line_id: int = 0
var last_frame_seq: int = 0
var last_intensity_pct: int = 0
var dropped_frames: int = 0

# Optional: filter-by-publisher (e.g. only accept "ai-host-1")
var publisher_identity_filter: String = ""

# `payload` may be PackedByteArray or String. `topic` filters out other
# data channels — pass empty to accept everything.
func on_livekit_data(payload, topic: String = "") -> void:
    if not topic.is_empty() and topic != VISEME_TOPIC:
        return
    var text: String
    match typeof(payload):
        TYPE_STRING:
            text = payload
        TYPE_PACKED_BYTE_ARRAY:
            text = (payload as PackedByteArray).get_string_from_utf8()
        _:
            push_warning("[IVXLiveKitVisemeReceiver] unsupported payload type: %d" % typeof(payload))
            return
    _dispatch_text(text)

func reset(_reason: String = "") -> void:
    is_active = false
    current_line_id = 0
    last_frame_seq = 0
    last_intensity_pct = 0
    dropped_frames = 0

func diagnostics() -> Dictionary:
    return {
        "topic": VISEME_TOPIC,
        "is_active": is_active,
        "current_line_id": current_line_id,
        "last_frame_seq": last_frame_seq,
        "last_intensity_pct": last_intensity_pct,
        "dropped_frames": dropped_frames,
    }

# ---- internals --------------------------------------------------------

func _dispatch_text(text: String) -> void:
    if text.is_empty(): return
    var parsed = JSON.parse_string(text)
    if typeof(parsed) != TYPE_DICTIONARY: return

    if not publisher_identity_filter.is_empty():
        var pub: String = String(parsed.get("publisher_identity", ""))
        if pub != publisher_identity_filter: return

    var kind: String = String(parsed.get("kind", ""))
    match kind:
        KIND_HEADER:    _handle_header(parsed)
        KIND_FRAME:     _handle_frame(parsed)
        KIND_PHONEME:   _handle_phoneme(parsed)
        KIND_EXPR:      _handle_expression(parsed)
        KIND_FOOTER:    _handle_footer(parsed)
        _:
            # Tolerate unknown kinds (forward-compat with v1.1 binary frames).
            pass

func _handle_header(d: Dictionary) -> void:
    is_active = true
    current_line_id = int(d.get("line_id", 0))
    last_frame_seq = 0
    dropped_frames = 0
    on_header.emit(d)

func _handle_frame(d: Dictionary) -> void:
    var seq: int = int(d.get("frame_seq", 0))
    if seq < last_frame_seq:
        dropped_frames += 1
        return
    last_frame_seq = seq
    if d.has("intensity_pct"):
        last_intensity_pct = int(d.get("intensity_pct", 0))
    on_frame.emit(d)

func _handle_phoneme(d: Dictionary) -> void:
    on_phoneme.emit(d)

func _handle_expression(d: Dictionary) -> void:
    on_expression.emit(d)

func _handle_footer(d: Dictionary) -> void:
    is_active = false
    on_footer.emit(d)
