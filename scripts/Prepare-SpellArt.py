"""Prepare pixel sampled spell sheets and right-sized power/attribute icons."""
from pathlib import Path
from PIL import Image

ROOT = Path(__file__).resolve().parents[1]
effects = ROOT / "GameResources" / "effects" / "Spells"
for index in range(1, 27):
    path = effects / f"S{index:03d}.png"
    with Image.open(path) as original:
        source = original.convert("RGBA")
        if source.size == (384, 48):
            continue
        if source.size != (192, 24):
            raise ValueError(f"Unexpected sprite sheet dimensions: {path} {source.size}")
        enlarged = source.resize((384, 48), Image.Resampling.NEAREST)
        enlarged.save(path, optimize=True)

for name in ("SpellCodexEntrance", "WuXing"):
    path = ROOT / "GameResources" / "ui" / "Icons" / f"{name}.png"
    with Image.open(path) as original:
        icon = original.convert("RGBA")
        if icon.size == (128, 128):
            continue
        icon.resize((128, 128), Image.Resampling.LANCZOS).save(path, optimize=True)
