"""Inspection-only contact pages from independent PNGs; never a production source."""
import importlib.util
import json
from pathlib import Path
from PIL import Image, ImageDraw

ROOT = Path(__file__).resolve().parents[1]
spec = importlib.util.spec_from_file_location("authoring", ROOT / "tools/actor-redraw.py")
A = importlib.util.module_from_spec(spec)
spec.loader.exec_module(A)
targets = A.load_json(A.RESOURCES / "actor-redraw.targets.json")
approved = {g["actorId"] for g in A.load_json(A.RESOURCES / "actor-frames.manifest.json")["groups"] if g.get("approved")}
records = []
for form in targets["forms"]:
    if form["id"] in approved:
        continue
    file = A.WORK / f"frames/{form['id']}/main/idle_0.png"
    if not file.exists():
        continue
    info = A.image_info(file)
    proof = A.load_json(file.with_suffix(".provenance.json"))
    if proof.get("method") != "built-in-imagegen-independent-frame" or info["sha256"] != proof.get("outputSha256"):
        raise ValueError("Invalid standard source: " + form["id"])
    records.append({"actor": form["id"], "name": form["name"], "file": str(file), "sha256": info["sha256"],
                    "family": form["family"], "bodyBounds": info["bodyBounds"], "canvas": info["canvas"]})
directory = A.WORK / "previews/detailed-standards"
directory.mkdir(parents=True, exist_ok=True)
for page in range((len(records) + 8) // 9):
    selected = records[page * 9:(page + 1) * 9]
    # Fixed scale for visual inspection; no production image is derived from this page.
    sheet = Image.new("RGBA", (2304, 2400), "#34463e")
    pen = ImageDraw.Draw(sheet)
    for cell, record in enumerate(selected):
        x, y = (cell % 3) * 768, (cell // 3) * 800
        pen.rectangle((x, y, x + 767, y + 799), outline="#74867c", width=2)
        pen.text((x + 12, y + 12), record["actor"], fill="white")
        pen.text((x + 12, y + 35), record["family"] + " / " + record["sha256"][:12], fill="#cfddd4")
        with Image.open(record["file"]) as raw:
            sprite = raw.convert("RGBA").resize((raw.width * 3, raw.height * 3), Image.Resampling.NEAREST)
        sheet.alpha_composite(sprite, (x + (768 - sprite.width) // 2, y + 64 + (736 - sprite.height) // 2))
        record["inspectionPage"] = str(directory / f"page-{page + 1:02}.png")
    sheet.save(directory / f"page-{page + 1:02}.png")
A.save_json(directory / "index.json", {"previewOnly": True, "count": len(records), "records": records})
print(json.dumps({"standards": len(records), "pages": (len(records) + 8) // 9, "index": str(directory / "index.json")}))
