"""Build reviewable spell atlases and compressed UI candidates.

Usage: python scripts/Build-VisualAtlases.py SOURCE_ROOT OUTPUT_ROOT
SOURCE_ROOT contains GameResources/effects/Spells/S001/1.png and
GameResources/ui/XuanhuangSkin/*.png. OUTPUT_ROOT receives only candidates;
the caller decides which files to install after visual review.
"""

import io
import json
import sys
from pathlib import Path

from PIL import Image, ImageChops, ImageStat


SIZE = 192
COLUMNS = 4
SPELL_COUNTS = {
    **{f"S{i:03}": 16 for i in range(1, 8)},
    **{f"S{i:03}": 20 for i in range(8, 11)},
    **{f"S{i:03}": 24 for i in range(11, 13)},
    **{f"S{i:03}": 16 for i in range(13, 17)},
    **{f"S{i:03}": 20 for i in range(17, 21)},
    **{f"S{i:03}": 24 for i in range(21, 27)},
}
BACKGROUNDS = ((25, 31, 30, 255), (235, 235, 235, 255))


def encoded_palette(image):
    output = io.BytesIO()
    image.quantize(colors=256, method=Image.Quantize.FASTOCTREE).save(
        output, format="PNG", optimize=True
    )
    return output.getvalue()


def visible_error(original, encoded):
    candidate = Image.open(io.BytesIO(encoded)).convert("RGBA")
    if candidate.size != original.size:
        raise ValueError(f"Dimensions changed: {original.size} -> {candidate.size}")
    errors = []
    for background in BACKGROUNDS:
        canvas = Image.new("RGBA", original.size, background)
        old = Image.alpha_composite(canvas, original)
        new = Image.alpha_composite(canvas, candidate)
        mean = ImageStat.Stat(ImageChops.difference(old, new)).mean
        errors.append(sum(mean[:3]) / 3)
    return max(errors)


def spell_atlas(source, destination):
    report = []
    destination.mkdir(parents=True, exist_ok=True)
    present = {p.name for p in source.iterdir() if p.is_dir()}
    if present != set(SPELL_COUNTS):
        raise ValueError(f"Spell IDs differ: {sorted(present ^ set(SPELL_COUNTS))}")
    for spell_id, count in SPELL_COUNTS.items():
        frames_dir = source / spell_id
        actual = {p.name for p in frames_dir.glob("*.png")}
        expected = {f"{i}.png" for i in range(1, count + 1)}
        if actual != expected:
            raise ValueError(f"{spell_id} frames differ: {sorted(actual ^ expected)}")
        rows = (count + COLUMNS - 1) // COLUMNS
        atlas = Image.new("RGBA", (SIZE * COLUMNS, SIZE * rows))
        input_bytes = 0
        for index in range(count):
            path = frames_dir / f"{index + 1}.png"
            frame = Image.open(path).convert("RGBA")
            if frame.size != (SIZE, SIZE):
                raise ValueError(f"Wrong frame size: {path}: {frame.size}")
            atlas.paste(frame, ((index % COLUMNS) * SIZE, (index // COLUMNS) * SIZE))
            input_bytes += path.stat().st_size
        encoded = encoded_palette(atlas)
        error = visible_error(atlas, encoded)
        (destination / f"{spell_id}.png").write_bytes(encoded)
        report.append({"asset": spell_id, "frames": count, "input_bytes": input_bytes,
                       "output_bytes": len(encoded), "visible_mean_error": round(error, 3)})
    return report


def ui_candidates(source, destination):
    report = []
    destination.mkdir(parents=True, exist_ok=True)
    for path in sorted(source.glob("*.png")):
        image = Image.open(path).convert("RGBA")
        encoded = encoded_palette(image)
        error = visible_error(image, encoded)
        (destination / path.name).write_bytes(encoded)
        report.append({"asset": path.name, "input_bytes": path.stat().st_size,
                       "output_bytes": len(encoded), "visible_mean_error": round(error, 3)})
    return report


def main():
    if len(sys.argv) != 3:
        raise SystemExit("Usage: Build-VisualAtlases.py SOURCE_ROOT OUTPUT_ROOT")
    source = Path(sys.argv[1]).resolve()
    output = Path(sys.argv[2]).resolve()
    if source == output or output in source.parents:
        raise ValueError("Output must not overwrite the source tree")
    report = {
        "spells": spell_atlas(source / "GameResources/effects/Spells",
                              output / "GameResources/effects/Spells"),
        "ui": ui_candidates(source / "GameResources/ui/XuanhuangSkin",
                            output / "GameResources/ui/XuanhuangSkin"),
    }
    (output / "visual-asset-report.json").write_text(
        json.dumps(report, ensure_ascii=False, indent=2), encoding="utf-8"
    )
    for section in report.values():
        print(len(section), "assets:", sum(x["input_bytes"] for x in section),
              "->", sum(x["output_bytes"] for x in section), "bytes")


if __name__ == "__main__":
    main()
