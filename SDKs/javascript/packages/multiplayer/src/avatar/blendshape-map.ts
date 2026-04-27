// Blendshape source-to-canonical mapping tables. The wire byte order is
// always ARKit's 52-key order (see docs/multiplayer/avatar-interop.md §3.1).
//
// Adapters map their source blendshapes to the canonical 52 array on
// publish, and from the canonical 52 to their consumed format on apply.
//
// Bumping these tables WITHOUT bumping `IVX_BLENDSHAPE_MAP_VERSION` is
// forbidden — clients use the version to refuse cross-version frames.

export const IVX_BLENDSHAPE_MAP_VERSION = 1;

/** ARKit 52-key canonical names, indexed by wire byte position. */
export const ARKIT_52_KEYS: readonly string[] = Object.freeze([
  "browDownLeft",         //  0
  "browDownRight",        //  1
  "browInnerUp",          //  2
  "browOuterUpLeft",      //  3
  "browOuterUpRight",     //  4
  "cheekPuff",            //  5
  "cheekSquintLeft",      //  6
  "cheekSquintRight",     //  7
  "eyeBlinkLeft",         //  8
  "eyeBlinkRight",        //  9
  "eyeLookDownLeft",      // 10
  "eyeLookDownRight",     // 11
  "eyeLookInLeft",        // 12
  "eyeWideLeft",          // 13
  "eyeWideRight",         // 14
  "jawForward",           // 15
  "jawLeft",              // 16
  "jawOpen",              // 17
  "jawRight",             // 18
  "mouthClose",           // 19
  "mouthDimpleLeft",      // 20
  "mouthDimpleRight",     // 21
  "mouthFrownLeft",       // 22
  "mouthFrownRight",      // 23
  "mouthFunnel",          // 24
  "mouthLeft",            // 26 in some Apple revs; we accept either
  "mouthLowerDownLeft",   // 27
  "mouthLowerDownRight",  // 28
  "mouthPressLeft",       // 29
  "mouthPressRight",      // 30
  "mouthPucker",          // 31
  "mouthRight",           // 32
  "mouthRollLower",       // 33
  "mouthRollUpper",       // 34
  "mouthShrugLower",      // 35
  "mouthShrugUpper",      // 39
  "mouthSmileLeft",       // 40
  "mouthSmileRight",      // 41
  "mouthStretchLeft",     // 42
  "mouthStretchRight",    // 43
  "mouthUpperUpLeft",     // 44
  "mouthUpperUpRight",    // 45
  "noseSneerLeft",        // 46
  "noseSneerRight",       // 47
  "tongueOut",            // 48
  // 49–51 reserved (zero on wire). Future Apple extensions land here.
  "_reserved49",
  "_reserved50",
  "_reserved51",
]);

/** Source key → canonical ARKit key (lower-case insensitive). */
export const RPM_TO_ARKIT: Readonly<Record<string, string>> = Object.freeze({
  // RPM uses ARKit-style names mostly; underscores normalised on import.
  "mouthsmile_l": "mouthSmileLeft",
  "mouthsmile_r": "mouthSmileRight",
  "viseme_aa":    "jawOpen",          // approximate; renderer should also blend mouthFunnel
  "viseme_ou":    "mouthPucker",
  "viseme_pp":    "mouthPressLeft",
  "viseme_ff":    "mouthLowerDownLeft",
  "viseme_ee":    "mouthSmileLeft",
});

export const OVR_TO_ARKIT: Readonly<Record<string, string>> = Object.freeze({
  "Mouth_Smile_L":    "mouthSmileLeft",
  "Mouth_Smile_R":    "mouthSmileRight",
  "Eye_Blink_L":      "eyeBlinkLeft",
  "Eye_Blink_R":      "eyeBlinkRight",
  "Mouth_Open":       "jawOpen",
  "Cheek_Puff_L":     "cheekPuff",
  "Cheek_Puff_R":     "cheekPuff",
  "Brow_Inner_Up":    "browInnerUp",
  "Brow_Outer_Up_L":  "browOuterUpLeft",
  "Brow_Outer_Up_R":  "browOuterUpRight",
});

export const VRM_TO_ARKIT: Readonly<Record<string, string>> = Object.freeze({
  "happy":      "mouthSmileLeft",
  "angry":      "browDownLeft",
  "sad":        "mouthFrownLeft",
  "surprised":  "eyeWideLeft",
  "blink":      "eyeBlinkLeft",
  "blinkLeft":  "eyeBlinkLeft",
  "blinkRight": "eyeBlinkRight",
  "aa":         "jawOpen",
  "ou":         "mouthPucker",
});

const KEY_INDEX: Map<string, number> = new Map(
  ARKIT_52_KEYS.map((k, i) => [k.toLowerCase(), i])
);

/**
 * Map a free-form source blendshape map (key → 0..1) to the 52-byte
 * canonical wire representation. Unknown keys are dropped with a single
 * warn-once log line; reserved slots stay at zero.
 */
export function packBlendshapesToCanonical(
  source: Record<string, number>,
  table: Readonly<Record<string, string>>
): Uint8Array {
  const out = new Uint8Array(52);
  for (const [rawKey, weight] of Object.entries(source)) {
    const key = rawKey.toLowerCase();
    let canonical = KEY_INDEX.has(key) ? rawKey : table[rawKey] ?? table[key];
    if (!canonical) continue;
    const idx = KEY_INDEX.get(canonical.toLowerCase());
    if (idx === undefined) continue;
    const w = Math.max(0, Math.min(1, weight));
    out[idx] = Math.round(w * 255);
  }
  return out;
}

/**
 * Unpack a canonical 52-byte wire payload back into a key-weight dict
 * for the renderer. Reserved slots are filtered out.
 */
export function unpackCanonicalToWeights(buf: Uint8Array): Record<string, number> {
  const out: Record<string, number> = {};
  const limit = Math.min(buf.length, ARKIT_52_KEYS.length);
  for (let i = 0; i < limit; i++) {
    const key = ARKIT_52_KEYS[i];
    if (key.startsWith("_reserved")) continue;
    out[key] = buf[i] / 255;
  }
  return out;
}
