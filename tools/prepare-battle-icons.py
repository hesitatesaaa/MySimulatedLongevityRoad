"""Condition the four imagegen trait icons; never creates character/effect artwork."""
import hashlib
import json
from pathlib import Path
from PIL import Image

ROOT = Path(__file__).resolve().parents[1]
GENERATED = Path("C:/Users/Arthur.lu/.codex/generated_images/01a15377-fb06-7c90-aad8-c61e189a8304")
ICONS = [
    ("Sword", "exec-78f88f91-1887-45a6-96bd-07ab76f17f70.png", "silver flying sword with jade hilt and cyan sword-qi arc"),
    ("Body", "exec-f9433bb5-af7f-4e4b-a42e-dac095a92d1a.png", "bronze-gold clenched fist wrapped in golden qi with rocky fragments"),
    ("Arts", "exec-0484cf6f-ff32-4bb1-bd6a-68c46bf9a010.png", "jade-centered five-element magical wheel, gold green blue red ochre petals"),
    ("Pact", "exec-8a2c0040-3753-477f-abf4-649dfc965255.png", "jade spirit-contract seal, white tiger paw and turquoise qi wisp"),
]
records = []
sheet = Image.new("RGBA", (256, 64), (30, 40, 47, 255))
for index, (name, filename, subject) in enumerate(ICONS):
    source = GENERATED / filename
    output = ROOT / "GameResources" / "trait" / f"MclslBattle{name}.png"
    with Image.open(source) as original:
        icon = original.convert("RGBA")
        assert icon.getchannel("A").getextrema()[0] == 0, f"{name}: generated alpha missing"
        icon = icon.resize((64, 64), Image.Resampling.NEAREST)
        assert icon.getbbox(), f"{name}: empty icon"
        output.parent.mkdir(parents=True, exist_ok=True)
        icon.save(output)
        sheet.alpha_composite(icon, (index * 64, 0))
    records.append({"id": f"MclslBattle{name}", "path": output.relative_to(ROOT).as_posix(),
                    "source": str(source), "sourceSha256": hashlib.sha256(source.read_bytes()).hexdigest(),
                    "sha256": hashlib.sha256(output.read_bytes()).hexdigest(), "size": [64, 64],
                    "tool": "built-in image_gen", "transparentBackground": True,
                    "promptSubject": subject,
                    "promptConstraints": "one standalone 64x64 Chinese xianxia WorldBox-style pixel-art trait icon; transparent; hard pixel edges; centered compact silhouette; no text, frame, scenery or extra icons",
                    "conditioning": "RGBA, nearest-neighbor resize to 64x64; preserve generated alpha"})
(ROOT / "resources/battle-icons.manifest.json").write_text(json.dumps({"version": 1, "icons": records}, ensure_ascii=True, indent=2)+"\n", encoding="utf-8")
preview = ROOT / "build/battle-icons-preview.png"
sheet.resize((1024, 256), Image.Resampling.NEAREST).save(preview)
print(json.dumps({"icons": len(records), "size": [64,64], "preview": str(preview)}))
