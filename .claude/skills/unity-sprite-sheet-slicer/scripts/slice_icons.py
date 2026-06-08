from __future__ import annotations

import argparse
import csv
import statistics
from collections import Counter
from pathlib import Path

from PIL import Image, ImageDraw, ImageFilter

DEFAULT_NAMES_BY_ROW = [
    ["video_panel", "hierarchy", "server", "usb", "settings"],
    ["grid", "sort_down", "sort_up", "refresh", "search", "folder"],
    ["cast", "mode_2d", "screen_bottom", "screen_middle", "screen_columns", "screen_top"],
    ["battery_charge", "layout_horizontal", "layout_vertical", "battery_dual"],
    ["rotate_180_left", "rotate_180_right", "rotate_180", "rotate_180_alt", "rotate_360_left", "rotate_360_right"],
    ["play", "pause", "stop", "previous", "next", "fast_forward"],
    ["repeat_one", "repeat", "shuffle", "shuffle_off", "slider"],
    ["volume", "volume_mute", "volume_high", "volume_low"],
    ["fullscreen", "window", "fit_screen", "fit_width", "undo", "mirror"],
    ["plus", "minus", "download", "upload", "info", "warning"],
    ["close_circle", "favorite", "bookmark", "show", "hide", "power"],
    ["back", "home", "more", "close", "confirm"],
]


def crop_image(image: Image.Image, mode: str) -> Image.Image:
    width, height = image.size
    if mode == "none":
        return image.copy()
    if mode == "right-third":
        return image.crop((width * 2 // 3, 0, width, height))
    if mode == "middle-third":
        return image.crop((width // 3, 0, width * 2 // 3, height))
    if mode == "left-third":
        return image.crop((0, 0, width // 3, height))
    if mode.startswith("box:"):
        parts = [int(value) for value in mode[4:].split(",")]
        if len(parts) != 4:
            raise ValueError("box crop must be box:x,y,width,height")
        x, y, box_width, box_height = parts
        return image.crop((x, y, x + box_width, y + box_height))
    raise ValueError(f"Unsupported crop mode: {mode}")


def recover_white_alpha_from_dark_background(image: Image.Image) -> Image.Image:
    if "A" in image.getbands():
        return image.convert("RGBA")

    crop = image.convert("RGB")
    width, height = crop.size
    brightness = Image.new("L", crop.size)
    brightness_pixels = brightness.load()
    crop_pixels = crop.load()
    for y in range(height):
        for x in range(width):
            red, green, blue = crop_pixels[x, y]
            brightness_pixels[x, y] = int((red + green + blue) / 3)

    mask = Image.new("L", crop.size, 0)
    mask_pixels = mask.load()
    for y in range(height):
        for x in range(width):
            if brightness_pixels[x, y] > 72:
                mask_pixels[x, y] = 255
    mask = mask.filter(ImageFilter.MaxFilter(9))
    mask_pixels = mask.load()

    background = Image.new("L", crop.size)
    background_pixels = background.load()
    for y in range(height):
        for x in range(width):
            if mask_pixels[x, y] == 0:
                background_pixels[x, y] = brightness_pixels[x, y]
                continue
            samples = []
            for radius in (8, 16, 32, 48):
                x0, x1 = max(0, x - radius), min(width - 1, x + radius)
                y0, y1 = max(0, y - radius), min(height - 1, y + radius)
                step = max(1, radius // 4)
                for yy in range(y0, y1 + 1, step):
                    for xx in (x0, x1):
                        if mask_pixels[xx, yy] == 0:
                            samples.append(brightness_pixels[xx, yy])
                for xx in range(x0, x1 + 1, step):
                    for yy in (y0, y1):
                        if mask_pixels[xx, yy] == 0:
                            samples.append(brightness_pixels[xx, yy])
                if len(samples) >= 8:
                    break
            background_pixels[x, y] = int(statistics.median(samples)) if samples else 48

    background = background.filter(ImageFilter.MedianFilter(3))
    background_pixels = background.load()
    rgba = Image.new("RGBA", crop.size, (255, 255, 255, 0))
    rgba_pixels = rgba.load()
    for y in range(height):
        for x in range(width):
            observed = brightness_pixels[x, y]
            base = max(1, min(254, background_pixels[x, y]))
            alpha = int(round((observed - base) * 255 / (255 - base)))
            alpha = max(0, min(255, alpha))
            if alpha < 10:
                alpha = 0
            rgba_pixels[x, y] = (255, 255, 255, alpha)
    return rgba


def cluster_indices(indices: list[int], max_gap: int) -> list[tuple[int, int]]:
    if not indices:
        return []
    clusters = []
    start = previous = indices[0]
    for value in indices[1:]:
        if value - previous <= max_gap:
            previous = value
        else:
            clusters.append((start, previous))
            start = previous = value
    clusters.append((start, previous))
    return clusters


def detect_content_bounds(image: Image.Image, names_by_row: list[list[str]], alpha_threshold: int) -> list[dict[str, int | str]]:
    alpha = image.getchannel("A")
    width, height = image.size
    y_indices = []
    for y in range(height):
        if sum(1 for x in range(width) if alpha.getpixel((x, y)) > alpha_threshold) > 5:
            y_indices.append(y)
    row_clusters = cluster_indices(y_indices, 18)
    if len(row_clusters) != len(names_by_row):
        raise ValueError(f"Detected {len(row_clusters)} rows, expected {len(names_by_row)} rows")

    records = []
    for row_index, (y0, y1) in enumerate(row_clusters):
        x_indices = []
        for x in range(width):
            if sum(1 for y in range(max(0, y0 - 4), min(height, y1 + 5)) if alpha.getpixel((x, y)) > alpha_threshold) > 2:
                x_indices.append(x)
        x_clusters = cluster_indices(x_indices, 18)
        names = names_by_row[row_index]
        if len(x_clusters) != len(names):
            raise ValueError(f"Row {row_index + 1}: detected {len(x_clusters)} icons, expected {len(names)}")
        for col_index, (x0, x1) in enumerate(x_clusters):
            points = []
            for yy in range(max(0, y0 - 5), min(height, y1 + 6)):
                for xx in range(max(0, x0 - 5), min(width, x1 + 6)):
                    if alpha.getpixel((xx, yy)) > 10:
                        points.append((xx, yy))
            min_x = max(0, min(point[0] for point in points) - 2)
            min_y = max(0, min(point[1] for point in points) - 2)
            max_x = min(width - 1, max(point[0] for point in points) + 2)
            max_y = min(height - 1, max(point[1] for point in points) + 2)
            records.append({
                "name": names[col_index],
                "source_row": row_index + 1,
                "source_col": col_index + 1,
                "content_x": min_x,
                "content_y_top": min_y,
                "content_width": max_x - min_x + 1,
                "content_height": max_y - min_y + 1,
            })
    return records


def square_slot(width: int, height: int, base_slot: int) -> int:
    max_dimension = max(width, height)
    return base_slot if max_dimension <= base_slot else max_dimension


def pack_icons(image: Image.Image, records: list[dict[str, int | str]], base_slot: int, atlas_width: int) -> tuple[Image.Image, list[dict[str, int | str]]]:
    x = y = row_height = 0
    placements = []
    for record in records:
        slot = square_slot(int(record["content_width"]), int(record["content_height"]), base_slot)
        if x + slot > atlas_width:
            x = 0
            y += row_height
            row_height = 0
        placements.append((record, x, y, slot))
        x += slot
        row_height = max(row_height, slot)
    atlas_height = y + row_height
    atlas = Image.new("RGBA", (atlas_width, atlas_height), (255, 255, 255, 0))
    packed_records = []
    for record, slot_x, slot_y, slot in placements:
        content = image.crop((
            int(record["content_x"]),
            int(record["content_y_top"]),
            int(record["content_x"]) + int(record["content_width"]),
            int(record["content_y_top"]) + int(record["content_height"]),
        ))
        content_x = slot_x + (slot - int(record["content_width"])) // 2
        content_y = slot_y + (slot - int(record["content_height"])) // 2
        atlas.alpha_composite(content, (content_x, content_y))
        packed_records.append({
            "name": record["name"],
            "source_row": record["source_row"],
            "source_col": record["source_col"],
            "x": slot_x,
            "y_top": slot_y,
            "width": slot,
            "height": slot,
            "unity_y": atlas_height - (slot_y + slot),
            "slot_size": slot,
            "content_x": content_x,
            "content_y_top": content_y,
            "content_width": record["content_width"],
            "content_height": record["content_height"],
        })
    return atlas, packed_records


def save_csv(path: Path, records: list[dict[str, int | str]]) -> None:
    headers = ["name", "source_row", "source_col", "x", "y_top", "width", "height", "unity_y", "slot_size", "content_x", "content_y_top", "content_width", "content_height"]
    path.parent.mkdir(parents=True, exist_ok=True)
    with path.open("w", encoding="utf-8", newline="") as file:
        writer = csv.DictWriter(file, fieldnames=headers)
        writer.writeheader()
        writer.writerows(records)


def save_preview(path: Path, atlas: Image.Image, records: list[dict[str, int | str]], base_slot: int) -> None:
    preview = Image.new("RGBA", atlas.size, (45, 45, 45, 255))
    preview.alpha_composite(atlas)
    draw = ImageDraw.Draw(preview)
    for record in records:
        color = (0, 255, 80, 255) if int(record["slot_size"]) == base_slot else (255, 180, 0, 255)
        x = int(record["x"])
        y = int(record["y_top"])
        size = int(record["slot_size"])
        draw.rectangle([x, y, x + size - 1, y + size - 1], outline=color, width=1)
        draw.text((x + 2, y + 2), str(record["name"])[:8], fill=color)
    path.parent.mkdir(parents=True, exist_ok=True)
    preview.save(path)


def main() -> None:
    parser = argparse.ArgumentParser(description="Crop, recover alpha, detect, and repack Unity icon sprites.")
    parser.add_argument("--source", required=True, type=Path)
    parser.add_argument("--crop", default="none")
    parser.add_argument("--output-atlas", required=True, type=Path)
    parser.add_argument("--output-csv", required=True, type=Path)
    parser.add_argument("--preview", type=Path)
    parser.add_argument("--intermediate", type=Path)
    parser.add_argument("--base-slot", type=int, default=64)
    parser.add_argument("--atlas-width", type=int, default=512)
    parser.add_argument("--alpha-threshold", type=int, default=45)
    args = parser.parse_args()

    source = Image.open(args.source)
    cropped = crop_image(source, args.crop)
    transparent = recover_white_alpha_from_dark_background(cropped)
    if args.intermediate:
        args.intermediate.parent.mkdir(parents=True, exist_ok=True)
        transparent.save(args.intermediate)

    content_records = detect_content_bounds(transparent, DEFAULT_NAMES_BY_ROW, args.alpha_threshold)
    atlas, records = pack_icons(transparent, content_records, args.base_slot, args.atlas_width)
    args.output_atlas.parent.mkdir(parents=True, exist_ok=True)
    atlas.save(args.output_atlas)
    save_csv(args.output_csv, records)
    if args.preview:
        save_preview(args.preview, atlas, records, args.base_slot)

    counts = Counter(int(record["slot_size"]) for record in records)
    print(f"atlas={args.output_atlas} size={atlas.size[0]}x{atlas.size[1]}")
    print(f"csv={args.output_csv}")
    print(f"sprites={len(records)} sizes={dict(sorted(counts.items()))}")


if __name__ == "__main__":
    main()
