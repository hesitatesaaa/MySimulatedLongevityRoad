"""Render 26 anime-style spells from original four-stage concept paintings.

The source art in scripts/spell_vfx_concepts is newly generated for this set.
Each 2x2 painting supplies gathering, formation, effect, and dispersal poses.
No geometry or pixels from the retired eight-frame effects are used.
"""

from __future__ import annotations

import math
import random
import re
from pathlib import Path

from PIL import Image, ImageDraw, ImageFilter, ImageOps


ROOT = Path(__file__).resolve().parents[1]
CONCEPTS = ROOT / "scripts" / "spell_vfx_concepts"
OUT = ROOT / "GameResources" / "effects" / "Spells"
SPELLS_SOURCE = ROOT / "code" / "MySimulatedLongevityRoad" / "Systems" / "Cultivation" / "MclslSpellSystem.cs"
SIZE = 192
SOURCE_PATTERN = re.compile(r'Spell\("(S\d{3})",\s*"[^"]+",\s*"[^"]+",\s*(\d+)')
OFFENSIVE = {"S001", "S004", "S007", "S008", "S009", "S010", "S012", "S013",
             "S015", "S016", "S017", "S019", "S021", "S022", "S023", "S026"}


def ease(value: float) -> float:
    return value * value * (3 - 2 * value)


def clean_panel(panel: Image.Image) -> Image.Image:
    panel = panel.convert("RGBA")
    # The image generator leaves very-low-alpha color noise in empty regions.
    alpha = panel.getchannel("A").point(
        lambda amount: 0 if amount < 28 else min(255, round((amount - 28) * 255 / 227))
    )
    panel.putalpha(alpha)
    return ImageOps.pad(panel, (SIZE - 12, SIZE - 12), method=Image.Resampling.LANCZOS,
                        color=(0, 0, 0, 0))


def load_poses(spell_id: str) -> list[Image.Image]:
    path = CONCEPTS / f"{spell_id}.png"
    if not path.exists():
        raise FileNotFoundError(f"Missing original concept painting: {path}")
    with Image.open(path) as source:
        source = source.convert("RGBA")
        half_x, half_y = source.width // 2, source.height // 2
        bounds = ((0, 0, half_x, half_y), (half_x, 0, source.width, half_y),
                  (0, half_y, half_x, source.height),
                  (half_x, half_y, source.width, source.height))
        return [clean_panel(source.crop(box)) for box in bounds]


def positioned(pose: Image.Image, scale: float, angle: float, x_shift: int, y_shift: int) -> Image.Image:
    width = max(1, round(pose.width * scale))
    resized = pose.resize((width, width), Image.Resampling.BICUBIC)
    if abs(angle) > 0.01:
        resized = resized.rotate(angle, resample=Image.Resampling.BICUBIC, expand=False)
    result = Image.new("RGBA", (SIZE, SIZE))
    result.alpha_composite(resized, ((SIZE - width) // 2 + x_shift,
                                    (SIZE - width) // 2 + y_shift))
    return result


def add_motes(image: Image.Image, spell_id: str, t: float, frame: int) -> None:
    rng = random.Random(int(spell_id[1:]) * 1001 + frame * 31)
    motes = Image.new("RGBA", image.size)
    draw = ImageDraw.Draw(motes)
    attack = spell_id in OFFENSIVE
    for index in range(16):
        angle = index * 2.39996 + t * (2.3 if attack else -1.4)
        radius = 17 + (index * 19 + round(t * 34)) % 65
        x = round(SIZE / 2 + math.cos(angle) * radius + (t - 0.5) * (22 if attack else 0))
        y = round(SIZE / 2 + math.sin(angle) * radius * 0.75 - t * (12 if not attack else 0))
        radius_px = 1 + (index % 5 == 0)
        if 5 <= x < SIZE - 5 and 5 <= y < SIZE - 5:
            draw.ellipse((x - radius_px, y - radius_px, x + radius_px, y + radius_px),
                         fill=(255, 249, 225, rng.randrange(75, 160)))
    glow = motes.filter(ImageFilter.GaussianBlur(2.8))
    image.alpha_composite(glow)
    image.alpha_composite(motes)


def render_frame(poses: list[Image.Image], spell_id: str, index: int, count: int) -> Image.Image:
    t = index / (count - 1)
    stage = min(2, int(t * 3))
    local = ease(min(1.0, t * 3 - stage))
    attack = spell_id in OFFENSIVE
    drift = round((t - 0.5) * 13) if attack else 0
    float_y = round(3 * math.sin(t * math.tau * 1.5))
    first = positioned(poses[stage], 0.93 + 0.04 * local,
                       (2.5 * math.sin(t * math.tau)) if attack else 0,
                       drift - 2, float_y)
    second = positioned(poses[stage + 1], 0.96 + 0.035 * local,
                        (-2 * math.sin(t * math.tau)) if attack else 0,
                        drift + 2, float_y)
    # The separate painted poses alter the actual flame, water, blade, leaf, or
    # seal structure. Their transition is a short luminous metamorphosis.
    image = Image.blend(first, second, local)
    add_motes(image, spell_id, t, index)
    opacity = 0.37 + 0.63 * ease(min(1, t / 0.14))
    if t > 0.83:
        opacity *= 1 - 0.68 * ease((t - 0.83) / 0.17)
    image.putalpha(image.getchannel("A").point(lambda amount: round(amount * opacity)))
    return image


def render_spell(spell_id: str, realm: int) -> int:
    count = 16 if realm <= 1 else 20 if realm <= 3 else 24
    poses = load_poses(spell_id)
    folder = OUT / spell_id
    folder.mkdir(parents=True, exist_ok=True)
    for index in range(count):
        render_frame(poses, spell_id, index, count).save(folder / f"{index + 1}.png", optimize=True)
    return count


def main() -> None:
    spells = [(spell_id, int(realm)) for spell_id, realm in SOURCE_PATTERN.findall(
        SPELLS_SOURCE.read_text(encoding="utf-8"))]
    if len(spells) != 26 or len({spell_id for spell_id, _ in spells}) != 26:
        raise ValueError("Expected 26 unique spell definitions")
    total = sum(render_spell(spell_id, realm) for spell_id, realm in spells)
    print(f"Rendered {len(spells)} anime-style spells, {total} independent {SIZE}x{SIZE} frames")


if __name__ == "__main__":
    main()
