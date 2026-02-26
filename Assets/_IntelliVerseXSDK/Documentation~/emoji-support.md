# Emoji Support

IntelliVerseX SDK includes production-safe emoji support for TextMeshPro with:

- Unicode emoji to TMP sprite-tag conversion.
- Variation-selector handling to reduce TMP missing-character noise.
- Optional validation against a specific sprite asset + fallback chain.
- Editor setup tooling for texture import hardening (compression, mipmaps, platform overrides).

## Runtime API

Namespace: `IntelliVerseX.Core`

- `IVXEmojiTextUtility.ConvertUnicodeToSpriteTags(text)`
- `IVXEmojiTextUtility.ConvertUnicodeToSpriteTags(text, spriteAsset, includeFallbackAssets)`
- `IVXEmojiTextUtility.StripVariationSelectors(text)`
- `IVXEmojiTextUtility.GetMissingSpriteNames(text, spriteAsset, includeFallbackAssets)`
- `TMP_Text.SetTextWithEmojiSprites(text, spriteAssetOverride)`
- `TMP_InputField.SetTextWithEmojiSprites(text, spriteAssetOverride)`
- `TMP_Text.SetTextEmojiSafe(text)`

### Example

```csharp
using IntelliVerseX.Core;
using TMPro;
using UnityEngine;

public sealed class EmojiSample : MonoBehaviour
{
    [SerializeField] private TMP_Text _label;
    [SerializeField] private TMP_SpriteAsset _emojiSpriteAsset;

    private void Start()
    {
        // Converts Unicode emojis to TMP sprite tags with asset-aware fallback handling.
        _label.SetTextWithEmojiSprites("Hello 😀 ❤️ 👨‍👩‍👧‍👦", _emojiSpriteAsset);
    }
}
```

## Editor Setup Tool

Menu:

- `IntelliVerseX -> Emoji -> Setup & Validate`

This tool can:

- Validate your selected emoji sprite asset + fallback chain.
- Validate and fix atlas texture import settings for production.
- Set TMP default sprite asset and enable TMP emoji support.
- Preview conversion output for sample text.

## Auto-Fix Import Guard

When enabled in the setup tool, the SDK texture postprocessor auto-applies safe import settings to known emoji atlas paths and file patterns (for example, `IVXEmojiAtlas_*`, `SidMojiAtlas_*`).

This helps keep emoji rendering stable even when project-level texture compression defaults are changed.

## Atlas Generation Script

A portable Python generator is included at:

- `Tools/Emoji/create_ivx_emoji_spritesheet.py`

Use it to generate atlas PNG + JSON metadata from emoji PNG files:

```bash
python create_ivx_emoji_spritesheet.py --source "<emoji_png_folder>" --output "<output_folder>"
```

Dependencies:

- Python 3.9+
- Pillow (`pip install pillow`)

## Notes

- For best quality, keep emoji atlas textures uncompressed (`RGBA32`) and mipmaps disabled.
- If you use ZWJ/keycap/flag sequences, keep your sprite naming consistent with generated metadata aliases.
