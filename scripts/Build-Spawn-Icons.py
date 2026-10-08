"""Make comparison previews of the retired idle-crop icon method.

This script must never write a production spawn_icon.png: all 62 icons in the
redraw request need their own composition and source art.
"""
import argparse
import json
import math
from pathlib import Path

from PIL import Image

ROOT = Path(__file__).resolve().parents[1]
ACTORS = ROOT / "GameResources" / "actors"
MANIFEST = ROOT / "scripts" / "beast_sprite_sources" / "manifest.json"


def folders():
    data = json.loads(MANIFEST.read_text(encoding="utf-8"))
    for name in data["beasts"]:
        for form in ("Beast", "Human"):
            yield ACTORS / "Beasts" / name / form
    for name in data["named"]:
        if name == "BaiXianSheng":
            continue
        folder = ACTORS / "Named" / name
        if name in ("DiYi", "DiSanMo", "QingShenLong"):
            for form in ("Beast", "Human"):
                yield folder / form
        else:
            yield folder
    for name in ("Bai", "Chuanfa"):
        yield ACTORS / "Immortals" / name


def build(folder, preview_root):
    source = folder / "main" / "idle_0.png"
    with Image.open(source) as loaded:
        image = loaded.convert("RGBA")
    box = image.getchannel("A").getbbox()
    if box is None:
        raise ValueError(f"empty idle sprite: {source}")
    width, height = box[2] - box[0], box[3] - box[1]
    # Keep the same apparent button occupancy when future frames are larger.
    side = max(16, math.ceil(max(width, height) / 0.80))
    icon = Image.new("RGBA", (side, side), (0, 0, 0, 0))
    icon.alpha_composite(image.crop(box), ((side - width) // 2, (side - height) // 2))
    destination = preview_root / folder.relative_to(ACTORS) / "spawn_icon.png"
    destination.parent.mkdir(parents=True, exist_ok=True)
    icon.save(destination, format="PNG", compress_level=0)
    if icon.getchannel("A").getbbox() is None:
        raise ValueError(f"empty icon: {folder}")


if __name__ == "__main__":
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--preview-dir", type=Path, required=True)
    args = parser.parse_args()
    preview = args.preview_dir.resolve()
    if preview == ACTORS.resolve() or ACTORS.resolve() in preview.parents:
        parser.error("preview output cannot be inside production GameResources/actors")
    items = list(folders())
    for item in items:
        build(item, preview)
    print(f"Built {len(items)} retired-method icon previews at {preview}")
