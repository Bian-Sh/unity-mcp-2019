---
name: unity-sprite-sheet-slicer
description: Use when processing an icon atlas for Unity Sprite Mode Multiple: crop a source icon group, recover transparency from a checkerboard background when alpha is missing, detect icon bounds, repack icons into centered square sprite slots, generate CSV metadata, and write Unity TextureImporter SpriteMetaData through Unity MCP.
---

# Unity Sprite Sheet Slicer

Use this skill when a user wants to turn an icon sheet into a Unity `Sprite (2D and UI)` atlas with `Sprite Mode = Multiple`, especially when icons must keep stable UI slots for state switching.

## Workflow

1. Inspect the source image size and alpha mode.
2. If the useful icon group is only part of the image, crop that region first.
3. If the source has no alpha but is composited on a checkerboard/dark background, recover alpha instead of using a hard threshold.
4. Detect each icon content bounding box from alpha projection.
5. Repack icons into a new atlas using square rects:
   - If content fits in `base_slot` such as `64`, use `base_slot x base_slot`.
   - If content exceeds `base_slot`, use the smallest square side that can contain the content.
   - Center content in the square slot.
6. Import the packed atlas in Unity as `TextureImporterType.Sprite` and `SpriteImportMode.Multiple`.
7. Write sprite metadata from the generated CSV, then verify loaded Sprite count and rect sizes.

## Recommended Script

Use `scripts/slice_icons.py` from this skill instead of hand-writing one-off slicing code.

Example for this project:

```powershell
python <skill>/scripts/slice_icons.py `
  --source "docs/Design/new icons.png" `
  --crop right-third `
  --output-atlas "Assets/Arts/Textures/white_icons_sheet_packed.png" `
  --output-csv "Assets/Arts/Textures/white_icons_sheet_packed_sprites.csv" `
  --preview "docs/Design/white_icons_sheet_packed_preview.png" `
  --base-slot 64 `
  --atlas-width 512
```

## Unity Metadata Update

After generating the atlas and CSV, use Unity MCP `refresh_unity`, then execute Editor code to set:

- `textureType = TextureImporterType.Sprite`
- `spriteImportMode = SpriteImportMode.Multiple`
- `alphaIsTransparency = true`
- `mipmapEnabled = false`
- `textureCompression = TextureImporterCompression.Uncompressed`
- `spritesheet = SpriteMetaData[]` loaded from CSV

CSV columns expected by the Unity step:

- `name`
- `x`
- `unity_y`
- `width`
- `height`

## Validation

Always verify:

- Loaded Sprite count equals CSV row count.
- `nonSquare` Sprite rect count is `0` unless user asked otherwise.
- Console has no new errors.
- Preview image visually confirms centered icons and no adjacent-icon contamination.

## Notes

- Do not split icons into many standalone PNG files unless explicitly requested; Unity supports `Multiple Sprite` atlases.
- If a true transparent source is available, prefer it over alpha reconstruction.
- For UI consistency, same-state icon families should generally share the same slot size.
