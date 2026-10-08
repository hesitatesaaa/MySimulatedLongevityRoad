"""Compile individually generated source frames to a pixel-size review sheet.

Only single-frame source PNGs are accepted. No atlas slicing or colour
quantisation is performed. The original source PNGs remain untouched.
"""
import sys
from pathlib import Path
from PIL import Image, ImageDraw

CANVAS = 192
INNER = 160
ZOOM = 3


def frame(path):
    image = Image.open(path).convert("RGBA")
    alpha = image.getchannel("A").point(lambda value: value if value >= 16 else 0)
    image.putalpha(alpha)
    image = image.resize((INNER, INNER), Image.Resampling.NEAREST)
    result = Image.new("RGBA", (CANVAS, CANVAS))
    result.alpha_composite(image, ((CANVAS - INNER) // 2,
                                   (CANVAS - INNER) // 2))
    box = result.getchannel("A").getbbox()
    if box is None or min(box[0], box[1], CANVAS-box[2], CANVAS-box[3]) < 8:
        raise ValueError(f"Frame lacks safety margin: {path}, {box}")
    return result


def main(paths):
    paths = [Path(path) for path in paths]
    if not paths:
        raise ValueError("Pass at least one individual source frame")
    output = paths[0].parent / "preview"
    output.mkdir(exist_ok=True)
    sheet = Image.new("RGB", (CANVAS * ZOOM * len(paths),
                              (CANVAS + 24) * ZOOM), "#33414c")
    draw = ImageDraw.Draw(sheet)
    for index, path in enumerate(paths):
        result = frame(path)
        result.save(output / path.name)
        enlarged = result.resize((CANVAS * ZOOM, CANVAS * ZOOM),
                                  Image.Resampling.NEAREST)
        sheet.paste(enlarged, (index * CANVAS * ZOOM, 0), enlarged)
        draw.text((index * CANVAS * ZOOM + 12, CANVAS * ZOOM + 8),
                  path.stem, fill="white")
    sheet_path = output / "review_sheet.png"
    sheet.save(sheet_path)
    print(sheet_path)


if __name__ == "__main__":
    main(sys.argv[1:])
