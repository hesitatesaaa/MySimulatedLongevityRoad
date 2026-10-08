"""Validate packed spell frames and render contact/GIF previews."""

from __future__ import annotations

import re
from pathlib import Path

from PIL import Image, ImageDraw


ROOT = Path(__file__).resolve().parents[1]
ASSETS = ROOT / "GameResources" / "effects" / "Spells"
CONCEPTS = ROOT / "scripts" / "spell_vfx_concepts"
SOURCE = ROOT / "code" / "MySimulatedLongevityRoad" / "Systems" / "Cultivation" / "MclslSpellSystem.cs"
PREVIEWS = ROOT / "_archive" / "spell-vfx-previews"
SPELL_PATTERN = re.compile(r'Spell\("(S\d{3})",\s*"([^"]+)",\s*"[^"]+",\s*(\d+)')


def main() -> None:
    spells = SPELL_PATTERN.findall(SOURCE.read_text(encoding="utf-8"))
    assert len(spells) == 26 and len({spell_id for spell_id, _, _ in spells}) == 26
    assert not any(p.is_dir() for p in ASSETS.iterdir()), "unpacked frames remain"
    assert {p.stem for p in ASSETS.glob("S???.png")} == {spell_id for spell_id, _, _ in spells}
    assert {p.stem for p in CONCEPTS.glob("S???.png")} == {spell_id for spell_id, _, _ in spells}
    PREVIEWS.mkdir(parents=True, exist_ok=True)
    contact = Image.new("RGB", (6 * 128, len(spells) * 128), "#20232d")
    labels = ImageDraw.Draw(contact)
    total = 0
    for row, (spell_id, name, realm_str) in enumerate(spells):
        realm = int(realm_str)
        count = 16 if realm <= 1 else 20 if realm <= 3 else 24
        rows = (count + 3) // 4
        with Image.open(ASSETS / f"{spell_id}.png") as loaded:
            assert loaded.size == (768, rows * 192), f"{spell_id}: atlas size"
            atlas = loaded.convert("RGBA")
        frames = []
        signatures = set()
        for i in range(1, count + 1):
            index = i - 1
            x, y = (index % 4) * 192, (index // 4) * 192
            image = atlas.crop((x, y, x + 192, y + 192))
            alpha = image.getchannel("A")
            assert alpha.getextrema()[0] == 0 and alpha.getextrema()[1] > 0, f"{spell_id}/{i}: alpha"
            assert sum(1 for a in alpha.getdata() if a > 0) >= 600, f"{spell_id}/{i}: nearly blank"
            if i == count // 2:
                assert len(image.getcolors(maxcolors=65536) or []) > 64, f"{spell_id}/{i}: lost painterly shading"
            signatures.add(image.tobytes())
            frames.append(image)
        assert len(signatures) == count, f"{spell_id}: duplicate frames"
        total += count
        for col, index in enumerate((0, count // 4, count // 2, 3 * count // 4, count - 1)):
            tile = frames[index].resize((112, 112), Image.Resampling.LANCZOS)
            contact.paste(tile, (col * 128 + 8, row * 128 + 8), tile)
        labels.text((5 * 128 + 8, row * 128 + 12), spell_id, fill="#f2e2a8")
        labels.text((5 * 128 + 8, row * 128 + 34), f"{count} frames", fill="#a7bdc9")
        frames[0].save(PREVIEWS / f"{spell_id}.gif", save_all=True, append_images=frames[1:],
                       duration=65, loop=0, disposal=2)
    contact.save(PREVIEWS / "contact-sheet.png")
    print(f"Validated {len(spells)} spells and {total} 192x192 RGBA frames; previews: {PREVIEWS}")


if __name__ == "__main__":
    main()
