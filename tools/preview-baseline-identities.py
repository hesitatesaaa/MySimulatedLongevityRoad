"""Read-only reference contact sheets; never used as production image sources."""
from pathlib import Path
import json
from PIL import Image, ImageDraw

root = Path(__file__).resolve().parents[1]
targets = json.loads((root / "resources/actor-redraw.targets.json").read_text(encoding="utf-8-sig"))
for label, forms in (("beast", [f for f in targets["forms"] if f["family"] != "human"]),
                     ("human", [f for f in targets["forms"] if f["family"] == "human"])):
    cell, cols = 210, 6
    sheet = Image.new("RGBA", (cols * cell, ((len(forms) + cols - 1) // cols) * cell), (42, 51, 48, 255))
    draw = ImageDraw.Draw(sheet)
    for i, form in enumerate(forms):
        card = json.loads((root / form["referenceCard"]).read_text(encoding="utf-8-sig"))
        with Image.open(root / card["identityReference"]) as image:
            sprite = image.convert("RGBA")
            sprite.thumbnail((180, 170), Image.Resampling.NEAREST)
        x, y = (i % cols) * cell, (i // cols) * cell
        sheet.alpha_composite(sprite, (x + (cell - sprite.width) // 2, y + 25))
        draw.text((x + 5, y + 5), form["species"], fill="white")
    out = root / f"output/actor-redraw/previews/baseline-{label}-identities.png"
    out.parent.mkdir(parents=True, exist_ok=True)
    sheet.save(out)
    print(out)
